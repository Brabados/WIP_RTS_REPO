using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class Pathfinder : MonoBehaviour
    {
        delegate void RemappingCompleted();
        RemappingCompleted onRemappingCompleted;

        public int drawSectorLevel = 0;
        public bool drawSectorNetwork = false;
        public bool drawSectors = false;
        public bool drawTiles = true;
        public bool drawTree = false;

        public int maxCostValue = 20;
        public int brushSize = 3;
        public int brushStrength = 4;
        public int brushFallOff = 1;

        private GameObject flowFieldHolder;
        private GameObject integrationFieldHolder;


        public bool worldIsMultiLayered = false;
        public bool twoDimensionalMode = false;
        public Vector3 worldStart;
        public float worldWidth = 10;
        public float worldLength = 10;

        public float tileSize = 1;
        public int sectorSize = 10;
        public float characterHeight = 2;

        public int levelScaling = 3;// how many sectors fit in level above (3x3)
        public int maxLevelAmount = 2;// amount of levels of abstraction

        public int obstacleLayer = 0;
        public int groundLayer = 0;

        public float worldHeight = 10;
        public float generationClimbHeight = 0.8f;

        public bool showIntergrationField = false;
        public bool showFlowField = false;


        public float invalidYValue = 0;


        private bool aJobIsRunning = false;
        private List<ThreadedJob> pathJobs = new List<ThreadedJob>();
        private Dictionary<int, List<Seeker>> charactersInFlowPath = new Dictionary<int, List<Seeker>>();
        private List<FlowFieldPath> drawQue = new List<FlowFieldPath>();

        public GameManager game;

        public WorldData worldData = new WorldData(null);
        public SeekerMovementManager seekerManager;
        public CostFieldManager costManager;

        private List<IntVector3> pathAdjustmentsInProgress = new List<IntVector3>();

        private float startTime;


        public void Awake()
        {
            GenerateMapping();
        }

        private void GenerateMapping()
        {
            RemapWorldSetup(true);

            if (onRemappingCompleted != null)
                onRemappingCompleted();
        }

        private void RemapWorldSetup(bool automatic)
        {
            if (automatic)
                GenerateWorld(true, true);
            else
                GenerateWorldManualExample();


            seekerManager = GetComponent<SeekerMovementManager>();
            seekerManager.Setup(this, worldData);

            foreach (Transform field in transform.GetChild(0).transform)
                field.gameObject.SetActive(false);

            // manual example   
            //FindPath(worldData.worldAreas[4].tileGrid[5][2], new List<Seeker>() { GameObject.Find("UnitHolder").transform.GetChild(0).GetComponent<Seeker>() });
        }

        public void Start()
        {
            invalidYValue = worldStart.y + worldHeight + 1;
            worldData.pathfinder = this;
        }

        public void GenerateWorld(bool GenerateWhileInPlayMode, bool LoadCostField)
        {
            if (GenerateWhileInPlayMode)
            {
                GetComponent<SaveLoad>().LoadLevel();
                worldData.GenerateWorld(this, GenerateWhileInPlayMode, LoadCostField);
                worldData.Setup();
            }
            else
            {
                if (LoadCostField)
                {
                    GetComponent<SaveLoad>().LoadLevel();
                    worldData.GenerateWorld(this, GenerateWhileInPlayMode, LoadCostField);
                }
                else
                {
                    worldData.costFields.Clear();
                    worldData.GenerateWorld(this, GenerateWhileInPlayMode, LoadCostField);
                }
            }
        }

        private void GenerateWorldManualExample()
        {
            int gridWidth = 10;
            int gridHeight = 4;

            List<Tile[][]> tileGrids = new List<Tile[][]>();
            List<IntVector2> tileGridsOffset = new List<IntVector2> { new IntVector2(0, 0), new IntVector2(gridWidth * 4, 0), new IntVector2(0, gridHeight * 4), new IntVector2(gridWidth, gridHeight * 4), new IntVector2(gridWidth * 2, 0), new IntVector2(gridWidth * 2, gridHeight) }; // , new IntVector2(0, 20), new IntVector2(10, 20)


            for (int i = 0; i < 6; i++)
            {
                Tile[][] tileGrid = new Tile[gridWidth][];
                for (int j = 0; j < gridWidth; j++)
                {
                    tileGrid[j] = new Tile[gridHeight];
                }


                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        tileGrid[x][y] = new Tile();
                        tileGrid[x][y].gridPos = new IntVector2(x, y);
                    }
                }

                tileGrids.Add(tileGrid);
            }

            GenerateWorldManually(tileGrids, tileGridsOffset, true);

            worldData.worldBuilder.ForceWorldAreaConnection(worldData.worldAreas[0], worldData.worldAreas[4], WorldArea.Side.Top, WorldArea.Side.Right, false);

            worldData.worldBuilder.ConnectWorldAreas();
        }

        public void GenerateWorldManually(List<Tile[][]> tileGrids, List<IntVector2> tileGridOffset, bool autoConnectWorldAreas)
        {
            worldData.GenerateWorldManually(this, tileGrids, tileGridOffset, autoConnectWorldAreas);
            worldData.Setup();
        }


        public void FindPath(Tile destinationNode, List<Seeker> units)
        {
            if (destinationNode != null)
                SearchPath(destinationNode, units);
        }



        private bool _regenerating = false;
        public void RegenerateMapping()
        {
            _regenerating = true;
        }

        void LateUpdate()
        {
            if (_regenerating && !aJobIsRunning)
            {
                _regenerating = false;
                GenerateMapping();
            }
        }




        public Vector3 GetMousePosition()
        {
            int layer = (1 << groundLayer);


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, layer))
                return hit.point;

            return new Vector3(0, invalidYValue, 0);
        }



        private void SearchPath(Tile destinationNode, List<Seeker> units)
        {
            pathJobs.Add(new SearchPathJob(destinationNode, units, this));
        }

        public void AddSectorInPath(WorldArea area, Tile tile, FlowFieldPath path)
        {
            pathAdjustmentsInProgress.Add(new IntVector3(path.key, area.index, tile.sectorIndex));
            pathJobs.Add(new AddToPathJob(area, tile, path, this));
        }

        public bool WorkingOnPathAdjustment(int pathIndex, int areaIndex, int tileSector)
        {
            if (pathAdjustmentsInProgress.Contains(new IntVector3(pathIndex, areaIndex, tileSector)))
                return true;
            else
                return false;
        }

        void Update()
        {
            if (!aJobIsRunning)
            {
                // map changes take priority
                if (worldData.worldManager.tilesBlockedAdjusted.Count > 0 || worldData.worldManager.tilesCostAdjusted.Count > 0)
                    worldData.worldManager.InputChanges();

                if (pathJobs.Count > 0)
                {
                    aJobIsRunning = true;

                    if (pathJobs[0].GetType() == typeof(SearchPathJob))
                        startTime = Time.realtimeSinceStartup;

                    pathJobs[0].Start();
                }
            }
            else
            {
                if (_regenerating)
                    return;

                if (pathJobs.Count > 0 && pathJobs[0].Update())
                {
                    // job is done
                    aJobIsRunning = false;

                    if (pathJobs[0].GetType() == typeof(SearchPathJob))
                    {
                        float endTime = Time.realtimeSinceStartup;
                        float timeElapsed = (endTime - startTime);
                        Debug.Log("Search Time Elapsed  ms: " + (timeElapsed * 1000f).ToString());
                    }

                    pathJobs[0] = null;
                    pathJobs.Remove(pathJobs[0]);
                }
            }


            if (drawQue.Count > 0)
            {
                for (int i = 0; i < drawQue.Count; i ++ )
                {
                    DrawFlowField(drawQue[i].flowField);

                    DrawIntegrationField(drawQue[i].intergrationField);
                }

                drawQue.Clear();
            }
        }



        // path finished, send it to the right characters, and draw it 
        public void PathCreated(FlowFieldPath path, int key, bool edit)
        {
            if (path != null)
            {
                SendOutPath(key, path, edit);
                drawQue.Add(path);
            }
        }

        // send to corresponding units
        public void SendOutPath(int key, FlowFieldPath path, bool edit)
        {
            foreach (Seeker seeker in charactersInFlowPath[key])
                seeker.SetFlowField(path, edit);
        }

        // Add seeker to already calculated flowPath, for example, there is 1 flowfield path for all characters in a tower defence game
        // and you want to add characters over time. 
        public void AddSeekerToExistingFlowField(int flowFieldPathKey, Seeker seeker)
        {
            charactersInFlowPath[flowFieldPathKey].Add(seeker);
            seeker.SetFlowField(worldData.flowFieldManager.flowFieldPaths[flowFieldPathKey], false);
        }

        public void KeepTrackOfUnitsInPaths(List<Seeker> units)
        {
            Dictionary<int, FlowFieldPath> checkedKeys = new Dictionary<int, FlowFieldPath>();

            foreach (Seeker seeker in units)
            {
                if (seeker.flowFieldPath != null)
                {
                    charactersInFlowPath[seeker.flowFieldPath.key].Remove(seeker);

                    if (!checkedKeys.ContainsKey(seeker.flowFieldPath.key))
                        checkedKeys.Add(seeker.flowFieldPath.key, seeker.flowFieldPath);
                }
            }

            foreach (int key in checkedKeys.Keys)
            {
                if (charactersInFlowPath[key].Count == 0)
                {
                    charactersInFlowPath.Remove(key);
                    worldData.flowFieldManager.flowFieldPaths.Remove(checkedKeys[key]);
                }
            }
        }

        public int GenerateKey(List<Seeker> units)
        {
            int i = 0;
            while (charactersInFlowPath.ContainsKey(i))
                i++;

            charactersInFlowPath.Add(i, units);
            return i;
        }



        public void PathAdjusted(FlowFieldPath path, WorldArea area, Tile tile)
        {
            pathAdjustmentsInProgress.Remove(new IntVector3(path.key, area.index, tile.sectorIndex));

            DrawFlowField(path.flowField);

            DrawIntegrationField(path.intergrationField);
        }


        public void AddToPath(WorldArea area, Tile startingPoint, FlowFieldPath path)
        {
            Dictionary<IntVector2, Tile> startingPoints = new Dictionary<IntVector2, Tile>();
            IntVector2 pointKey = new IntVector2(area.index, startingPoint.sectorIndex);
            startingPoints.Add(pointKey, startingPoint);

            List<List<int>> areasAndSectorsPath = worldData.hierachalPathfinder.FindPaths(startingPoints, path.destination, worldData.worldAreas[path.destination.worldAreaIndex]);

            worldData.intergrationFieldManager.StartIntegrationFieldCreation(path.destination, areasAndSectorsPath, path, null, path.key, true);
        }


        public void WorldHasBeenChanged(List<IntVector2> changedAreasAndSectors)
        {
            List<FlowFieldPath> changedPaths = new List<FlowFieldPath>();

            for (int i = 0; i < worldData.flowFieldManager.flowFieldPaths.Count; i ++)
            {
                for (int j = 0; j < changedAreasAndSectors.Count; j ++ )
                {
                    if (worldData.flowFieldManager.flowFieldPaths[i].flowField.field.ContainsKey(changedAreasAndSectors[j]))
                    {
                        if (!changedPaths.Contains(worldData.flowFieldManager.flowFieldPaths[i]))
                            changedPaths.Add(worldData.flowFieldManager.flowFieldPaths[i]);

                        break;
                    }
                }
            }

            for (int i = 0; i < changedPaths.Count; i ++ )
                FindPath(changedPaths[i].destination, charactersInFlowPath[changedPaths[i].key]);
        }



        void OnDrawGizmos()
        {
            // draw world bounding box
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(worldStart, Vector3.one);

            // botom
            Gizmos.DrawLine(worldStart, worldStart + new Vector3(worldWidth, 0, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, 0), worldStart + new Vector3(worldWidth, 0, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, -worldLength), worldStart + new Vector3(0, 0, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(0, 0, -worldLength), worldStart);

            //top 
            Gizmos.DrawLine(worldStart + new Vector3(0, worldHeight, 0), worldStart + new Vector3(worldWidth, worldHeight, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, worldHeight, 0), worldStart + new Vector3(worldWidth, worldHeight, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, worldHeight, -worldLength), worldStart + new Vector3(0, worldHeight, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(0, worldHeight, -worldLength), worldStart + new Vector3(0, worldHeight, 0));


            //sides
            Gizmos.DrawLine(worldStart, worldStart + new Vector3(0, worldHeight, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, 0), worldStart + new Vector3(worldWidth, worldHeight, 0));
            Gizmos.DrawLine(worldStart + new Vector3(worldWidth, 0, -worldLength), worldStart + new Vector3(worldWidth, worldHeight, -worldLength));
            Gizmos.DrawLine(worldStart + new Vector3(0, 0, -worldLength), worldStart + new Vector3(0, worldHeight, -worldLength));


            if (worldData.pathfinder != null)
                worldData.DrawGizmos();
        }


        public void DrawFlowField(FlowField flowField)
        {
            if (flowFieldHolder != null)
                Destroy(flowFieldHolder);

            GameObject arrow = Resources.Load("Prefab/FlowArrow") as GameObject;

            if (worldData.pathfinder.showFlowField)
            {
                flowFieldHolder = new GameObject();
                for (int x = 0; x < worldData.worldAreas.Count; x++)
                {
                    for (int i = 0; i < worldData.worldAreas[x].sectorGrid[0].Length; i++)
                    {
                        if (flowField.field.ContainsKey(new IntVector2(x, i)))
                        {
                            MultiLevelSector sector = worldData.worldAreas[x].sectorGrid[0][i];
                            Vector2 sectorPos = new Vector2(sector.left, sector.top);

                            for (int j = 0; j < sector.tilesInWidth * sector.tilesInHeight; j++)
                            {
                                int y = Mathf.FloorToInt((float)j / sector.tilesInWidth);

                                Vector2 node = sectorPos + new Vector2(j - (sector.tilesInWidth * y), y);

                                if (worldData.worldAreas[x].tileGrid[(int)node.x][(int)node.y] != null)
                                {
                                    GameObject b = Instantiate(arrow, worldData.tileManager.GetTileWorldPosition(worldData.worldAreas[x].tileGrid[(int)node.x][(int)node.y], worldData.worldAreas[x]) + new Vector3(0, 0.2f, 0), Quaternion.identity) as GameObject;

                                    Vector2 flow = worldData.flowFieldManager.DirToVector(flowField.field[new IntVector2(x, i)][j]);
                                    b.transform.LookAt(b.transform.position + new Vector3(flow.x, 0, flow.y));

                                    b.transform.parent = flowFieldHolder.transform;
                                    b.transform.localScale = new Vector3(worldData.pathfinder.tileSize, worldData.pathfinder.tileSize, worldData.pathfinder.tileSize);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DrawIntegrationField(IntergrationField integrationfield)
        {
            if (integrationFieldHolder != null)
                Destroy(integrationFieldHolder);

            GameObject integrationTile = Resources.Load("Prefab/IntegrationTile") as GameObject;

            if (worldData.pathfinder.showIntergrationField)
            {
                integrationFieldHolder = new GameObject();
                for (int x = 0; x < worldData.worldAreas.Count; x++)
                {
                    for (int i = 0; i < worldData.worldAreas[x].sectorGrid[0].Length; i++)
                    {
                        if (integrationfield.field.ContainsKey(new IntVector2(x, i)))
                        {
                            MultiLevelSector sector = worldData.worldAreas[x].sectorGrid[0][i];
                            Vector2 sectorPos = new Vector2(sector.left, sector.top);

                            for (int j = 0; j < sector.tilesInWidth * sector.tilesInHeight; j++)
                            {
                                int y = Mathf.FloorToInt((float)j / sector.tilesInWidth);

                                Vector2 node = sectorPos + new Vector2(j - (sector.tilesInWidth * y), y);

                                if (worldData.worldAreas[x].tileGrid[(int)node.x][(int)node.y] != null)
                                {
                                    GameObject b = Instantiate(integrationTile, worldData.tileManager.GetTileWorldPosition(worldData.worldAreas[x].tileGrid[(int)node.x][(int)node.y], worldData.worldAreas[x]), Quaternion.identity) as GameObject;

                                    int value = integrationfield.field[new IntVector2(x, i)][j];

                                    if (value * 3 >= worldData.colorLists.pathCostColors.Length - 2)
                                        b.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = worldData.colorLists.pathCostColors[worldData.colorLists.pathCostColors.Length - 2];
                                    else
                                    {
                                        if (value < worldData.colorLists.pathCostColors.Length - 2)
                                            b.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = worldData.colorLists.pathCostColors[value * 3];
                                    }

                                    b.transform.position += Vector3.up * 0.15f;
                                    b.transform.parent = integrationFieldHolder.transform;
                                    b.transform.localScale = new Vector3(worldData.pathfinder.tileSize, worldData.pathfinder.tileSize, worldData.pathfinder.tileSize);
                                }
                            }
                        }
                    }
                }
            }

        }




    }
}
