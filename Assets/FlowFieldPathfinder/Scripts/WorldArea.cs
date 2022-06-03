using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class WorldArea
    {

        public enum Side
        {
            Top = 0,
            Down = 1,
            Left = 2,
            Right = 3
        };

        public WorldData worldData;

        public Vector3 origin;

        public int index;

        // left and top tile offset from worldStart. for example: our left is 5 tiles away from worldstart.x  or  our top is 8 tiles away from worldstart.z
        public int leftOffset;
        public int topOffset;

        public Tile[][] tileGrid;
        public int gridWidth;
        public int gridLength;
        public int totalAmountOfTiles;

        public float angle = 0;
        public bool flatArea = false;
        public float angledAreaResolutionDiffrence = 1;
        public bool angleDirectionX = false;
        public int anglePositive = 1;


        public bool[][] searchField = null;// matches tile grid, decieds what tiles should be expanded over during integration-/flow- field generation
        public int[][] levelDimensions; //per level of abstraction we store:   sector width, sector height, sectorgrid width, sectorgrid height  
        public int[][][][] lookUpLowerSectors;// lookUpLowerSectors[sectorlevel-1][hih level sectorID]  = get sectors on a lower layer of abstraction as a 2D array,  that are inside the higher level sector id
        public MultiLevelSector[][] sectorGrid = null;


        public Dictionary<Tile, List<AbstractNode>>[] tileSectorNodeConnections; //tileSectorNodeConnections[sectorlevel][Tile]  = get list of abstarctNodes on this position
        public Dictionary<IntVector2, List<IntVector3>> worldAreaTileConnections; // worldAreaTileConnections[Intvector2(areaindex, sectorindex)]  = list of tile positions V3.x, V3.y :  V3.z is the index of the worldArea its in (never our own)
        public Dictionary<IntVector2, List<List<IntVector2>>> groupsInSectors = new Dictionary<IntVector2, List<List<IntVector2>>>();// list of groups of tiles per sector   groupsInSetor[intVector2(connectedWorldAreaIndex, currentTile.sectorIndex)]


        public List<int> worldAreasConnectedIndexes = new List<int>();


        public void SlopedAreaCopyTiles(float _angle, int _left, int right, int _top, int bot, List<List<IntVector2>> tileList, List<TemporaryWorldArea> tempWorldAreas, int Ylayer, WorldData _worldData)
        {
            angle = _angle;
            SetValues(_left, right, _top, bot, Ylayer, _worldData);
            worldData.multiLevelSectorManager.SetupSectorsWorldArea(this);

            
            Tile highestYTile = null;
            Tile lowestYTile = null;

            int i = 0;
            foreach (List<IntVector2> vec2List in tileList)
            {
                Tile[][] grid = tempWorldAreas[Ylayer + i].tileGrid;
                foreach (IntVector2 vec2 in vec2List)
                {
                    int x = vec2.x - leftOffset;
                    int y = vec2.y - topOffset;

                    tileGrid[x][y] = grid[vec2.x][vec2.y];
                    tileGrid[x][y].gridPos = new IntVector2(x, y);
                    tileGrid[x][y].integrationValue = worldData.tileManager.tileResetIntegrationValue;
                    tileGrid[x][y].worldAreaIndex = index;

                    int SectorX = Mathf.FloorToInt(x / (float)levelDimensions[0][0]); //   /sectorwidth
                    int SectorY = Mathf.FloorToInt(y / (float)levelDimensions[0][1]);//    /sectorheight

                    if (worldData.pathfinder.maxLevelAmount == 0)
                        tileGrid[x][y].sectorIndex = 0;
                    else
                        tileGrid[x][y].sectorIndex = (SectorY * levelDimensions[0][2]) + SectorX;// *sectorgridWidth

                    MultiLevelSector sector = sectorGrid[0][tileGrid[x][y].sectorIndex];
                    int deltaX = x - sector.left;
                    int deltaY = y - sector.top;
                    tileGrid[x][y].indexWithinSector = (deltaY * sector.tilesInWidth) + deltaX;

                    if (lowestYTile == null || tileGrid[vec2.x - leftOffset][vec2.y - topOffset].yWorldPos < lowestYTile.yWorldPos)
                        lowestYTile = tileGrid[vec2.x - leftOffset][vec2.y - topOffset];

                    if (highestYTile == null || tileGrid[vec2.x - leftOffset][vec2.y - topOffset].yWorldPos > highestYTile.yWorldPos)
                        highestYTile = tileGrid[vec2.x - leftOffset][vec2.y - topOffset];
                }
                i++;
            }

            origin.y = highestYTile.yWorldPos - ((highestYTile.yWorldPos - lowestYTile.yWorldPos) * 0.5f);

            // X or Z  direction slope
            Vector3 rayStartingPoint = new Vector3(0, lowestYTile.yWorldPos + worldData.pathfinder.characterHeight * 0.4f, 0);
            float tileOffset = (worldData.pathfinder.tileSize * 0.5f);

            rayStartingPoint.x = worldData.pathfinder.worldStart.x + ((lowestYTile.gridPos.x + leftOffset) * worldData.pathfinder.tileSize) + tileOffset;
            rayStartingPoint.z = worldData.pathfinder.worldStart.z - ((lowestYTile.gridPos.y + topOffset) * worldData.pathfinder.tileSize) - tileOffset;

            RaycastHit hit;

            if (Physics.Raycast(rayStartingPoint, Vector3.down, out hit, worldData.pathfinder.characterHeight * 0.6f, 1 << worldData.pathfinder.groundLayer))
            {
                if (Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.z))
                {
                    angleDirectionX = true;

                    if (hit.normal.x > 0)
                        anglePositive = -1;
                    else
                        anglePositive = 1;
                }
                else
                {
                    angleDirectionX = false;
                    if (hit.normal.z > 0)
                        anglePositive = 1;
                    else
                        anglePositive = -1;
                }
            }


            float length = 0; //highestYTile.yWorldPos - lowestYTile.yWorldPos
            if (angleDirectionX)
                length = (gridWidth - 1) * worldData.pathfinder.tileSize;
            else
                length = (gridLength - 1) * worldData.pathfinder.tileSize;

            if (length != 0)
            {
                angledAreaResolutionDiffrence = Vector3.Distance(worldData.tileManager.GetTileWorldPosition(lowestYTile, this), worldData.tileManager.GetTileWorldPosition(highestYTile, this)) / length;
                //Debug.Log("angledAreaResolutionDiffrence  " + angledAreaResolutionDiffrence);
            }
        }

        public void FlatAreaCopyTiles(IntVector2 offset, Tile[][] _tileGrid, int yLayer, WorldData _worldData)
        {
            flatArea = true;
            SetValues(offset, _tileGrid.Length, _tileGrid[0].Length, yLayer, _worldData);
            worldData.multiLevelSectorManager.SetupSectorsWorldArea(this);
            bool firstTile = true;

            for (int x = 0; x < _tileGrid.Length; x++)
            {
                for (int y = 0; y < _tileGrid[0].Length; y++)
                {
                    tileGrid[x][y] = _tileGrid[x][y];

                    // manually
                    tileGrid[x][y].yWorldPos = _worldData.pathfinder.worldStart.y;// 0f; // forced y = 0.  2D worldAreas
                    worldData.layerdWorldAreaIndexes[yLayer][x + leftOffset][y + topOffset] = index; // set world Index

                    tileGrid[x][y].gridPos = new IntVector2(x, y);
                    tileGrid[x][y].integrationValue = worldData.tileManager.tileResetIntegrationValue;
                    tileGrid[x][y].worldAreaIndex = index;

                    int SectorX = Mathf.FloorToInt(x / (float)levelDimensions[0][0]); //   /sectorwidth
                    int SectorY = Mathf.FloorToInt(y / (float)levelDimensions[0][1]);//    /sectorheight

                    if (worldData.pathfinder.maxLevelAmount == 0)
                        tileGrid[x][y].sectorIndex = 0;
                    else
                        tileGrid[x][y].sectorIndex = (SectorY * levelDimensions[0][2]) + SectorX;


                    MultiLevelSector sector = sectorGrid[0][tileGrid[x][y].sectorIndex];
                    int deltaX = x - sector.left;
                    int deltaY = y - sector.top;
                    tileGrid[x][y].indexWithinSector = (deltaY * sector.tilesInWidth) + deltaX;

                    if (firstTile)
                    {
                        origin.y = tileGrid[x][y].yWorldPos;
                        firstTile = false;
                    }
                }
            }
        }

        public void FlatAreaCopyTiles(int _left, int right, int _top, int bot, List<IntVector2> tileList, Tile[][] grid, int Ylayer, WorldData _worldData)
        {
            flatArea = true;
            SetValues(_left, right, _top, bot, Ylayer, _worldData);
            worldData.multiLevelSectorManager.SetupSectorsWorldArea(this);
            bool firstTile = true;

            foreach (IntVector2 vec2 in tileList)
            {
                int x = vec2.x - leftOffset;
                int y = vec2.y - topOffset;

                tileGrid[x][y] = grid[vec2.x][vec2.y];
                tileGrid[x][y].gridPos = new IntVector2(x, y);
                tileGrid[x][y].integrationValue = worldData.tileManager.tileResetIntegrationValue;
                tileGrid[x][y].worldAreaIndex = index;

                int SectorX = Mathf.FloorToInt(x / (float)levelDimensions[0][0]); //   /sectorwidth
                int SectorY = Mathf.FloorToInt(y / (float)levelDimensions[0][1]);//    /sectorheight

                if (worldData.pathfinder.maxLevelAmount == 0)
                    tileGrid[x][y].sectorIndex = 0;
                else
                    tileGrid[x][y].sectorIndex = (SectorY * levelDimensions[0][2]) + SectorX;


                MultiLevelSector sector = sectorGrid[0][tileGrid[x][y].sectorIndex];
                int deltaX = x - sector.left;
                int deltaY = y - sector.top;
                tileGrid[x][y].indexWithinSector = (deltaY * sector.tilesInWidth) + deltaX;

                if (firstTile)
                {
                    origin.y = tileGrid[x][y].yWorldPos;

                    firstTile = false;
                }
            }
        }


        private void SetValues(IntVector2 offSet, int width, int lenght, int Ylayer, WorldData _worldData)
        {
            IntVectorComparer comparer = new IntVectorComparer();
            worldAreaTileConnections = new Dictionary<IntVector2, List<IntVector3>>(comparer);
            worldData = _worldData;
            leftOffset = offSet.x;
            topOffset = offSet.y;


            origin = worldData.tileManager.GetWorldPosition(leftOffset, topOffset);
            origin.y = worldData.pathfinder.worldStart.y + Ylayer * worldData.pathfinder.characterHeight + worldData.pathfinder.characterHeight * 0.5f;

            gridWidth = width;
            gridLength = lenght;

            searchField = new bool[gridWidth][];
            tileGrid = new Tile[gridWidth][];
            for (int x = 0; x < gridWidth; x++)
            {
                tileGrid[x] = new Tile[gridLength];
                searchField[x] = new bool[gridLength];
            }
        }

        private void SetValues(int _left, int right, int _top, int bot, int Ylayer, WorldData _worldData)
        {
            IntVectorComparer comparer = new IntVectorComparer();
            worldAreaTileConnections = new Dictionary<IntVector2, List<IntVector3>>(comparer);
            worldData = _worldData;
            leftOffset = _left;
            topOffset = _top;


            origin = worldData.tileManager.GetWorldPosition(leftOffset, topOffset);
            origin.y = worldData.pathfinder.worldStart.y + Ylayer * worldData.pathfinder.characterHeight + worldData.pathfinder.characterHeight * 0.5f;

            gridWidth = 1 + right - leftOffset;
            gridLength = 1 + bot - topOffset;

            searchField = new bool[gridWidth][];
            tileGrid = new Tile[gridWidth][];
            for (int x = 0; x < gridWidth; x++)
            {
                tileGrid[x] = new Tile[gridLength];
                searchField[x] = new bool[gridLength];
            }
        }


        public void SetValuesFlatWorld(int _left, int right, int _top, int bot, WorldData _worldData)
        {
            worldData = _worldData;
            leftOffset = _left;
            topOffset = _top;

            origin = worldData.tileManager.GetWorldPosition(leftOffset, topOffset);

            gridWidth = 1 + right - leftOffset;
            gridLength = 1 + bot - topOffset;

            searchField = new bool[gridWidth][];
            tileGrid = new Tile[gridWidth][];
            for (int x = 0; x < gridWidth; x++)
            {
                tileGrid[x] = new Tile[gridLength];
                searchField[x] = new bool[gridLength];
            }
        }

        public Tile GetTileFromGrid(int x, int y)
        {
            if (x < 0 || y < 0 || x > gridWidth - 1 || y > gridLength -1)
            {
                Debug.Log("sampling outside of grid range");
                return null;
            }
            else
                return tileGrid[x][y];
        }

    }
}
