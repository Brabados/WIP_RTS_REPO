using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace FlowPathfinding
{
    [Serializable]
    public class WorldData : ISerializable
    {
        public bool drawCost = false;

        [NonSerialized]
        public bool worldCreatedManually = false;

        [NonSerialized]
        public Dictionary<IntVector2, int> manualWorldAreaConnections = new Dictionary<IntVector2, int>();

        [NonSerialized]
        public List<int[][]> layerdWorldAreaIndexes = new List<int[][]>();
        [NonSerialized]
        public List<WorldArea> worldAreas = new List<WorldArea>();
        [NonSerialized]
        public Vector3 worldPositionOffset;

        [NonSerialized]
        public List<int[][]> costFields = new List<int[][]>();

        [NonSerialized]
        public TileManager tileManager = new TileManager();
        [NonSerialized]
        public HierachalPathfinder hierachalPathfinder = new HierachalPathfinder();
        [NonSerialized]
        public MultiLevelSectorManager multiLevelSectorManager = new MultiLevelSectorManager();
        [NonSerialized]
        public ColorLists colorLists = new ColorLists();
        [NonSerialized]
        public IntergrationFieldManager intergrationFieldManager = new IntergrationFieldManager();
        [NonSerialized]
        public FlowFieldManager flowFieldManager = new FlowFieldManager();
        [NonSerialized]
        public WorldBuilder worldBuilder = new WorldBuilder();
        [NonSerialized]
        public WorldManager worldManager = new WorldManager();

        [NonSerialized]
        public Pathfinder pathfinder;


        [NonSerialized]
        public bool worldGenerated = false;


        public void Setup()
        {
            tileManager.worldData = multiLevelSectorManager.worldData = hierachalPathfinder.worldData = intergrationFieldManager.worldData = flowFieldManager.worldData = worldManager.worldData= this;
            colorLists.Setup();
        }


        public void GenerateWorld(Pathfinder _pathfinder, bool GenerateWhileInPlayMode, bool loadCostField)
        {
            pathfinder = _pathfinder;
            float startTime = Time.realtimeSinceStartup;

            tileManager.worldData = multiLevelSectorManager.worldData = hierachalPathfinder.worldData = intergrationFieldManager.worldData = flowFieldManager.worldData = this;
            multiLevelSectorManager.lowLevel.manager = multiLevelSectorManager.highLevel.manager = multiLevelSectorManager;
            worldPositionOffset = new Vector3(0, pathfinder.generationClimbHeight * 0.15f, 0);

            worldBuilder.gridWidth = (int)(pathfinder.worldWidth / pathfinder.tileSize);
            worldBuilder.gridLength = (int)(pathfinder.worldLength / pathfinder.tileSize);



            worldBuilder.GenerateWorld(this, GenerateWhileInPlayMode, loadCostField);



            worldGenerated = true;
            float endTime = Time.realtimeSinceStartup;
            float timeElapsed = (endTime - startTime);
            Debug.Log("timeElapsed Generate World ms: " + (timeElapsed * 1000f).ToString());
        }


        public void GenerateWorldManually(Pathfinder _pathfinder, List<Tile[][]> tileGrids, List<IntVector2> tileGridOffset, bool autoConnectWorldAreas)
        {
            worldCreatedManually = true;

            pathfinder = _pathfinder;
            float startTime = Time.realtimeSinceStartup;

            tileManager.worldData = multiLevelSectorManager.worldData = hierachalPathfinder.worldData = intergrationFieldManager.worldData = flowFieldManager.worldData = this;
            multiLevelSectorManager.lowLevel.manager = multiLevelSectorManager.highLevel.manager = multiLevelSectorManager;
            worldPositionOffset = new Vector3(0, pathfinder.generationClimbHeight * 0.15f, 0);

            worldBuilder.gridWidth = (int)(pathfinder.worldWidth / pathfinder.tileSize);
            worldBuilder.gridLength = (int)(pathfinder.worldLength / pathfinder.tileSize);



            worldBuilder.GenerateWorldManually(this, tileGrids, tileGridOffset, autoConnectWorldAreas);


            worldGenerated = true;
            float endTime = Time.realtimeSinceStartup;
            float timeElapsed = (endTime - startTime);
            Debug.Log("timeElapsed Generate World ms: " + (timeElapsed * 1000f).ToString());
        }


        public void DrawGizmos()
        {

            // draw World area boundaries

            //Gizmos.color = Color.red;
            //if (worldAreas.Count > 0)
            //{
            //    foreach (WorldArea worldArea in worldAreas)
            //    {
            //        Gizmos.DrawLine(worldArea.origin, worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, 0));
            //        Gizmos.DrawLine(worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, 0), worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, -worldArea.gridLength * pathfinder.tileSize));
            //        Gizmos.DrawLine(worldArea.origin + new Vector3(worldArea.gridWidth * pathfinder.tileSize, 0, -worldArea.gridLength * pathfinder.tileSize), worldArea.origin + new Vector3(0, 0, -worldArea.gridLength * pathfinder.tileSize));
            //        Gizmos.DrawLine(worldArea.origin + new Vector3(0, 0, -worldArea.gridLength * pathfinder.tileSize), worldArea.origin);
            //    }
            //}




            if (worldGenerated)
            {
                if (pathfinder.drawTree)
                    DrawTree();


                if (pathfinder.drawTiles)
                {
                    Gizmos.color = Color.yellow;
                    for (int i = 0; i < worldBuilder.visualizeTiles.Count; i += 2)
                        Gizmos.DrawLine(worldBuilder.visualizeTiles[i], worldBuilder.visualizeTiles[i + 1]);
                }

                // draw sectors
                if (pathfinder.drawSectors)
                {
                    Gizmos.color = Color.green;
                    foreach (WorldArea worldArea in worldAreas)
                    {
                        Vector3 start;
                        int level = 0;
                        foreach (MultiLevelSector sector in worldArea.sectorGrid[level])
                        {
                            start = worldArea.origin + new Vector3((sector.left * pathfinder.tileSize) - (pathfinder.tileSize * 0.5f), 0, -((sector.top * pathfinder.tileSize) - (pathfinder.tileSize * 0.5f)));
                            float width = multiLevelSectorManager.GetSectorWidthAtLevel(worldArea, level) * pathfinder.tileSize;
                            float length = multiLevelSectorManager.GetSectorHeightAtLevel(worldArea, level) * pathfinder.tileSize;

                            Gizmos.DrawLine(start, start + new Vector3(width, 0, 0));
                            Gizmos.DrawLine(start + new Vector3(width, 0, 0), start + new Vector3(width, 0, -length));
                            Gizmos.DrawLine(start + new Vector3(width, 0, -length), start + new Vector3(0, 0, -length));
                            Gizmos.DrawLine(start + new Vector3(0, 0, -length), start);
                        }
                    }
                }

                if (pathfinder.drawSectorNetwork)
                {
                    foreach (WorldArea worldArea in worldAreas)
                    {
                        if (worldArea.sectorGrid != null && pathfinder.drawSectorLevel != -1 && pathfinder.drawSectorLevel < pathfinder.maxLevelAmount)
                        {
                            foreach (MultiLevelSector sector in worldArea.sectorGrid[pathfinder.drawSectorLevel])
                            {
                                Gizmos.color = Color.black;

                                int j = 0;
                                for (int i = 0; i < sector.connections.Count; i += 2)
                                {
                                    Gizmos.DrawLine(tileManager.GetTileWorldPosition(sector.connections[i], worldArea), tileManager.GetTileWorldPosition(sector.connections[i + 1], worldArea));
                                    j++;
                                }

                                if (pathfinder.worldIsMultiLayered)
                                {
                                    Gizmos.color = Color.blue;
                                    foreach (AbstractNode node in sector.worldAreaNodes.Keys)
                                    {
                                        Vector3 posStart = tileManager.GetTileWorldPosition(node.tileConnection, worldArea);

                                        Vector3 posEnd = tileManager.GetTileWorldPosition(node.nodeConnectionToOtherSector.tileConnection, worldAreas[sector.worldAreaNodes[node]]);

                                        Gizmos.DrawLine(posStart, posEnd);
                                    }
                                }

                                Gizmos.color = Color.black;
                                foreach (AbstractNode node in sector.worldAreaNodes.Keys)
                                {
                                    Vector3 posStart = tileManager.GetTileWorldPosition(node.tileConnection, worldArea);

                                    foreach (AbstractNode nodeConnected in node.connections.Keys)
                                    {
                                        if (nodeConnected != node.nodeConnectionToOtherSector)
                                        {
                                            Vector3 posEnd = tileManager.GetTileWorldPosition(nodeConnected.tileConnection, worldArea);

                                            Gizmos.DrawLine(posStart, posEnd);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private void DrawTree()
        {
            Gizmos.color = Color.red;

            if (pathfinder != null && pathfinder.seekerManager != null)
            {
                if (pathfinder.seekerManager.quadTree != null)
                {
                    //quad
                    List<QuadTree> openList = new List<QuadTree>();
                    openList.Add(pathfinder.seekerManager.quadTree as QuadTree);

                    QuadTree current = null;
                    while (openList.Count > 0)
                    {
                        current = openList[0];

                        Vector3 origin = new Vector3(pathfinder.worldStart.x + current.bounds.xMin, pathfinder.worldStart.y + pathfinder.worldHeight + 1, pathfinder.worldStart.z - current.bounds.yMin);

                        Gizmos.DrawLine(origin, origin + new Vector3(current.bounds.width, 0, 0));
                        Gizmos.DrawLine(origin + new Vector3(current.bounds.width, 0, 0), origin + new Vector3(current.bounds.width, 0, -current.bounds.height));
                        Gizmos.DrawLine(origin + new Vector3(current.bounds.width, 0, -current.bounds.height), origin + new Vector3(0, 0, -current.bounds.height));
                        Gizmos.DrawLine(origin + new Vector3(0, 0, -current.bounds.height), origin);


                        if (current.nodesInUse)
                        {
                            openList.Add(current.nodes[0]);
                            openList.Add(current.nodes[1]);
                            openList.Add(current.nodes[2]);
                            openList.Add(current.nodes[3]);
                        }

                        openList.Remove(current);
                    }
                }
                else
                {
                    // oct
                    Gizmos.color = new Color(1, 0, 0, 0.04F);
                    List<OcTree> openList = new List<OcTree>();
                    openList.Add(pathfinder.seekerManager.octTree as OcTree);

                    OcTree current = null;
                    while (openList.Count > 0)
                    {
                        current = openList[0];
                        Gizmos.color = new Color(1, 0, 0, 0.1F + current.level * 0.1f);
                        Gizmos.DrawCube(new Vector3(current.bounds[0], current.bounds[2], current.bounds[1]), new Vector3(current.bounds[3] * 2f, current.bounds[5] * 2f, current.bounds[4] * 2f));

                        if (current.nodesInUse)
                        {
                            openList.Add(current.nodes[0]);
                            openList.Add(current.nodes[1]);
                            openList.Add(current.nodes[2]);
                            openList.Add(current.nodes[3]);
                            openList.Add(current.nodes[4]);
                            openList.Add(current.nodes[5]);
                            openList.Add(current.nodes[6]);
                            openList.Add(current.nodes[7]);
                        }

                        openList.Remove(current);
                    }
                }
            }
        }



        public WorldData(Pathfinder _pathfinder)
        {
            pathfinder = _pathfinder;
        }

        public WorldData(SerializationInfo info, StreamingContext context)
        {
            costFields = new List<int[][]>();
            costFields = (info.GetValue("costFields", costFields.GetType())) as List<int[][]>;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("costFields", costFields);
        }


        public static WorldData Load(byte[] levelBytes)
        {
            return SerializeUtility.DeserializeObject(levelBytes) as WorldData;
        }

        public byte[] Save()
        {
            return SerializeUtility.SerializeObject(this);
        }

    }



}
