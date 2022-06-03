using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class MultiLevelSectorManager
    {
        public LowLevelSectorManager lowLevel = new LowLevelSectorManager();
        public HighLevelSectorManager highLevel = new HighLevelSectorManager();

        public WorldData worldData;

        public int maxGateSize = 500; // in reality its always 1 bigger   (maxGateSize + 1)


        public void SetupSectorsWorldArea(WorldArea worldArea)
        {

            int sectorWidth = worldData.pathfinder.sectorSize;
            int sectorHeight = worldData.pathfinder.sectorSize;
            if (worldData.pathfinder.maxLevelAmount == 0)
            {
                sectorWidth = worldArea.gridWidth;
                sectorHeight = worldArea.gridLength;
                worldArea.levelDimensions = new int[1][];
                worldArea.levelDimensions[0] = new int[4];
                worldArea.levelDimensions[0][0] = sectorWidth;
                worldArea.levelDimensions[0][1] = sectorHeight;
                worldArea.levelDimensions[0][2] = 1;
                worldArea.levelDimensions[0][3] = 1;
                worldArea.sectorGrid = new MultiLevelSector[1][];
                worldArea.sectorGrid[0] = new MultiLevelSector[worldArea.levelDimensions[0][2] * worldArea.levelDimensions[0][3]];

                int j = 0;
                int i = 0;
                int level = 0;
                int index = 0;
                worldArea.tileSectorNodeConnections = new Dictionary<Tile, List<AbstractNode>>[1];
                worldArea.tileSectorNodeConnections[level] = new Dictionary<Tile, List<AbstractNode>>();
                worldArea.sectorGrid[level][index] = new MultiLevelSector();
                worldArea.sectorGrid[level][index].gridX = j;
                worldArea.sectorGrid[level][index].gridY = i;
                worldArea.sectorGrid[level][index].ID = index;
                worldArea.sectorGrid[level][index].level = level;
                worldArea.sectorGrid[level][index].top = i * worldArea.levelDimensions[level][0];
                worldArea.sectorGrid[level][index].bottom = i * worldArea.levelDimensions[level][0] + worldArea.levelDimensions[level][0] - 1;
                worldArea.sectorGrid[level][index].left = j * worldArea.levelDimensions[level][0];
                worldArea.sectorGrid[level][index].right = j * worldArea.levelDimensions[level][0] + worldArea.levelDimensions[level][0] - 1;
                worldArea.sectorGrid[level][index].tilesInWidth = Mathf.Min(worldArea.gridWidth - worldArea.sectorGrid[level][index].left, worldArea.levelDimensions[level][0]);
                worldArea.sectorGrid[level][index].tilesInHeight = Mathf.Min(worldArea.gridLength - worldArea.sectorGrid[level][index].top, worldArea.levelDimensions[level][1]);
                worldArea.sectorGrid[level][index].worldAreaIndex = worldArea.index;
                worldArea.sectorGrid[level][index].Setup();

            }
            else
            {
                worldArea.tileSectorNodeConnections = new Dictionary<Tile, List<AbstractNode>>[worldData.pathfinder.maxLevelAmount];

                worldArea.lookUpLowerSectors = new int[worldData.pathfinder.maxLevelAmount - 1][][][];
                worldArea.levelDimensions = new int[worldData.pathfinder.maxLevelAmount][];
                worldArea.sectorGrid = new MultiLevelSector[worldData.pathfinder.maxLevelAmount][];
            }



            for (int level = 0; level < worldData.pathfinder.maxLevelAmount; level++)
            {
                worldArea.tileSectorNodeConnections[level] = new Dictionary<Tile, List<AbstractNode>>();

                worldArea.levelDimensions[level] = new int[4];
                worldArea.levelDimensions[level][0] = sectorWidth;
                worldArea.levelDimensions[level][1] = sectorHeight;
                worldArea.levelDimensions[level][2] = Mathf.CeilToInt((worldArea.gridWidth / (float)sectorWidth));  //Mathf.CeilToInt(worldLayer.gridWidth / (float)sectorWidth);
                worldArea.levelDimensions[level][3] = Mathf.CeilToInt((worldArea.gridLength / (float)sectorHeight)); //Mathf.CeilToInt(worldLayer.gridHeight / (float)sectorHeight);

                worldArea.sectorGrid[level] = new MultiLevelSector[worldArea.levelDimensions[level][2] * worldArea.levelDimensions[level][3]];

                for (int i = 0; i < worldArea.levelDimensions[level][3]; i++)
                {
                    for (int j = 0; j < worldArea.levelDimensions[level][2]; j++)
                    {
                        int index = (i * worldArea.levelDimensions[level][2]) + j;
                        worldArea.sectorGrid[level][index] = new MultiLevelSector();
                        worldArea.sectorGrid[level][index].gridX = j;
                        worldArea.sectorGrid[level][index].gridY = i;
                        worldArea.sectorGrid[level][index].ID = index;
                        worldArea.sectorGrid[level][index].level = level;
                        worldArea.sectorGrid[level][index].top = i * worldArea.levelDimensions[level][0];
                        worldArea.sectorGrid[level][index].bottom = i * worldArea.levelDimensions[level][0] + worldArea.levelDimensions[level][0] - 1;
                        worldArea.sectorGrid[level][index].left = j * worldArea.levelDimensions[level][0];
                        worldArea.sectorGrid[level][index].right = j * worldArea.levelDimensions[level][0] + worldArea.levelDimensions[level][0] - 1;
                        worldArea.sectorGrid[level][index].tilesInWidth = Mathf.Min(worldArea.gridWidth - worldArea.sectorGrid[level][index].left, worldArea.levelDimensions[level][0]);
                        worldArea.sectorGrid[level][index].tilesInHeight = Mathf.Min(worldArea.gridLength - worldArea.sectorGrid[level][index].top, worldArea.levelDimensions[level][1]);
                        worldArea.sectorGrid[level][index].worldAreaIndex = worldArea.index;
                        worldArea.sectorGrid[level][index].Setup();
                    }
                }

                sectorWidth *= worldData.pathfinder.levelScaling;
                sectorHeight *= worldData.pathfinder.levelScaling;


                if (level != 0)
                    worldArea.lookUpLowerSectors[level - 1] = new int[worldArea.sectorGrid[level].Length][][];
            }

            if (worldData.pathfinder.maxLevelAmount != 0)
                FillInLookUpLowerSectors(worldArea);
        }

        // create look up table to easily find all the sectors within,  a sector on a higher level
        private void FillInLookUpLowerSectors(WorldArea worldArea)
        {
            for (int i = 0; i < worldArea.lookUpLowerSectors.Length; i++)
            {
                int level = i + 1;

                foreach (MultiLevelSector sector in worldArea.sectorGrid[level])
                {
                    int lowerLevelX = sector.gridX * worldData.pathfinder.levelScaling;
                    int lowerLevelY = sector.gridY * worldData.pathfinder.levelScaling;

                    // get lower sector in the top left corner
                    int lowerIndex = (lowerLevelY * worldArea.levelDimensions[level - 1][2]) + lowerLevelX;

                    int width = GetSectorGridWidthAtLevel(worldArea, sector.level - 1) - lowerLevelX;
                    int arrayWidth = Mathf.Min(width, worldData.pathfinder.levelScaling);
                    worldArea.lookUpLowerSectors[level - 1][sector.ID] = new int[arrayWidth][];

                    int height = GetSectorGridHeightAtLevel(worldArea, sector.level - 1) - lowerLevelY;
                    int arrayHeight = Mathf.Min(height, worldData.pathfinder.levelScaling);
                    for (int j = 0; j < worldArea.lookUpLowerSectors[level - 1][sector.ID].Length; j++)
                        worldArea.lookUpLowerSectors[level - 1][sector.ID][j] = new int[arrayHeight];

                    // get surrounding sectors
                    for (int x = 0; x < arrayWidth; x++)
                    {
                        for (int y = 0; y < arrayHeight; y++)
                            worldArea.lookUpLowerSectors[level - 1][sector.ID][x][y] = lowerIndex + x + (y * GetSectorGridWidthAtLevel(worldArea, sector.level - 1));
                    }
                }
            }
        }

        public void SetupSectorConnections(WorldArea worldArea)
        {
            lowLevel.SetupSectorConnections(worldArea);
            highLevel.SetupSectorConnections(worldArea);


            foreach (MultiLevelSector[] list in worldArea.sectorGrid)
            {
                foreach (MultiLevelSector sector in list)
                    sector.SearchConnections();
            }
        }


        public void RemoveAllConnectionsWithinSector(MultiLevelSector sector)
        {
            foreach (List<AbstractNode> list in sector.sectorNodesOnEdge)
            {
                foreach (AbstractNode node in list)
                {
                    node.connections.Clear();

                    if (node.nodeConnectionToOtherSector != null)
                        node.connections.Add(node.nodeConnectionToOtherSector, 1);
                }
            }

            foreach (AbstractNode node in sector.worldAreaNodes.Keys)
            {
                node.connections.Clear();

                if (node.nodeConnectionToOtherSector != null)
                    node.connections.Add(node.nodeConnectionToOtherSector, 1);
            }
        }


        public void RemoveAllConnectionsOfWorldAreaNode(AbstractNode node)
        {
            foreach (AbstractNode nodeConnected in node.connections.Keys)
                nodeConnected.connections.Remove(node);

            node.connections.Clear();

            if (node.nodeConnectionToOtherSector != null)
                node.connections.Add(node.nodeConnectionToOtherSector, 1);
        }


        public void RemoveAllAbstractNodesInSectorEdges(MultiLevelSector sector)
        {
            RemoveAllConnectionsWithinSector(sector);

            for (int i = 0; i < sector.sectorNodesOnEdge.Length; i++)
                sector.sectorNodesOnEdge[i].Clear();
        }


        public void ConnectNodeInSector(MultiLevelSector sector, AbstractNode sectorNode, WorldArea area)
        {
            if (sector.level == 0)
                lowLevel.ConnectNodeInSector(sector, sectorNode, area);
            else
                highLevel.ConnectNodeInSector(sector, sectorNode, area);
        }

        public void ConnectWorldAreaNodesToSectorNodes(MultiLevelSector sector, WorldArea area, int level)
        {
            if (sector.level == 0)
                lowLevel.ConnectWorldAreaNodes(sector, area);
            else
                highLevel.ConnectWorldAreaNodes(sector, area);
        }


        public void SetSearchFields(int areaIndex, List<int> sectors, bool value)
        {
            WorldArea worldArea = worldData.worldAreas[areaIndex];
            for (int i = 0; i < sectors.Count; i++)
            {
                MultiLevelSector sector = worldArea.sectorGrid[0][sectors[i]];
                for (int x = 0; x < sector.tilesInWidth; x++)
                {
                    for (int y = 0; y < sector.tilesInHeight; y++)
                        worldArea.searchField[sector.left + x][sector.top + y] = value;
                }
            }
        }

        // set which slots/tiles we can expand on.
        public void SetSearchFields(List<int> sectors, WorldArea area, bool value)
        {
            for (int i = 0; i < sectors.Count; i++)
            {
                MultiLevelSector sector = area.sectorGrid[0][sectors[i]];
                for (int x = 0; x < sector.tilesInWidth; x++)
                {
                    for (int y = 0; y < sector.tilesInHeight; y++)
                        area.searchField[sector.left + x][sector.top + y] = value;
                }
            }
        }


        public void CreateSectorNodes(MultiLevelSector sector, int neighbourID, int edgeIndex, int edgeIndexNeighbourSector, Tile inSectorNode, Tile inNeighbourSectorNode, WorldArea area)
        {
            AbstractNode node = CreateAbstractNodeOnSectorEdge(sector, edgeIndex, inSectorNode, area);
            AbstractNode neighbourNode = CreateAbstractNodeOnSectorEdge(area.sectorGrid[sector.level][neighbourID], edgeIndexNeighbourSector, inNeighbourSectorNode, area);

            node.nodeConnectionToOtherSector = neighbourNode;
            neighbourNode.nodeConnectionToOtherSector = node;

            ConnectSectorNodes(node, neighbourNode, 1);
        }

        public AbstractNode CreateAbstractNodeOnSectorEdge(MultiLevelSector sector, int edgeIndex, Tile tile, WorldArea area)
        {
            AbstractNode sectorNode = CreateAbstractNodeInSector(sector, tile, area);
            sector.sectorNodesOnEdge[edgeIndex].Add(sectorNode);

            return sectorNode;
        }

        public AbstractNode CreateAbstractNodeInSector(MultiLevelSector sector, Tile tile, WorldArea area)
        {
            AbstractNode sectorNode = new AbstractNode();
            sectorNode.tileConnection = tile;
            tile.hasAbstractNodeConnection = true;
            sectorNode.sector = sector.ID;
            sectorNode.worldAreaIndex = area.index;


            if (area.tileSectorNodeConnections[sector.level].ContainsKey(tile))
                area.tileSectorNodeConnections[sector.level][tile].Add(sectorNode);
            else
                area.tileSectorNodeConnections[sector.level].Add(tile, new List<AbstractNode>() { sectorNode });

            return sectorNode;
        }

        public void ConnectSectorNodes(AbstractNode sectorNode, List<AbstractNode> sectorNodes, int distance)
        {
            if (sectorNodes.Count > 0)
            {
                foreach (AbstractNode node in sectorNodes)
                    ConnectSectorNodes(sectorNode, node, distance);
            }
        }

        public void ConnectSectorNodes(AbstractNode sectorNode, AbstractNode sectorNode2, int distance)
        {
            if (sectorNode.connections.ContainsKey(sectorNode2))
                sectorNode.connections[sectorNode2] = distance;
            else
                sectorNode.connections.Add(sectorNode2, distance);


            if (sectorNode2.connections.ContainsKey(sectorNode))
                sectorNode2.connections[sectorNode] = distance;
            else
                sectorNode2.connections.Add(sectorNode, distance);
        }

        public void RemoveAllAbstractNodesOnSectorEdge(MultiLevelSector sector, int edgeIndex)
        {
            WorldArea area = worldData.worldAreas[sector.worldAreaIndex];

            // remove the connections from other sectorNodes to the sectorNodes we will remove now
            foreach (AbstractNode sectorNode in sector.sectorNodesOnEdge[edgeIndex])
            {
                foreach (AbstractNode nodeConnected in sectorNode.connections.Keys)
                    nodeConnected.connections.Remove(sectorNode);
            }

            // remove 
            foreach (AbstractNode sectorNode in sector.sectorNodesOnEdge[edgeIndex])
            {
                if (area.tileSectorNodeConnections[sector.level][sectorNode.tileConnection].Count > 1)
                    area.tileSectorNodeConnections[sector.level][sectorNode.tileConnection].Remove(sectorNode);
                else
                {
                    area.tileSectorNodeConnections[sector.level].Remove(sectorNode.tileConnection);
                    sectorNode.tileConnection.hasAbstractNodeConnection = false;
                }
            }

            // remove entire edge
            sector.sectorNodesOnEdge[edgeIndex].Clear();
        }


        public void RemoveAbstractNode(int level, AbstractNode sectorNode)
        {
            if (sectorNode != null)
            {
                List<AbstractNode> keys = new List<AbstractNode>(sectorNode.connections.Keys);
                foreach (AbstractNode node in keys)
                    node.connections.Remove(sectorNode);


                worldData.worldAreas[sectorNode.worldAreaIndex].tileSectorNodeConnections[level][sectorNode.tileConnection].Remove(sectorNode);

                if (worldData.worldAreas[sectorNode.worldAreaIndex].tileSectorNodeConnections[level][sectorNode.tileConnection].Count == 0)
                {
                    worldData.worldAreas[sectorNode.worldAreaIndex].tileSectorNodeConnections[level].Remove(sectorNode.tileConnection);

                    //if (level == 0)
                    sectorNode.tileConnection.hasAbstractNodeConnection = false;
                }

                sectorNode = null;
            }
        }



        public Vector2 DirectionBetweenSectorsVector2(MultiLevelSector a, MultiLevelSector b)
        {
            int deltaX = b.left - a.left;
            int deltaY = b.top - a.top;

            if (deltaX > 1)
                deltaX = 1;

            if (deltaX < -1)
                deltaX = -1;

            if (deltaY > 1)
                deltaY = 1;

            if (deltaY < -1)
                deltaY = -1;

            return new Vector2(deltaX, deltaY);
        }

        // get list of nodes that are on the border of start sector, with next sector 
        public List<Tile> RowBetweenSectorsWithinWorldArea(MultiLevelSector sectorStart, MultiLevelSector sectorNext, WorldArea area)
        {
            Vector2 dir = DirectionBetweenSectorsVector2(sectorStart, sectorNext);

            if (dir == -Vector2.up)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.left, sectorStart.top), Vector2.right, area);
            if (dir == Vector2.up)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.left, sectorStart.bottom), Vector2.right, area);
            if (dir == -Vector2.right)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.left, sectorStart.top), Vector2.up, area);
            if (dir == Vector2.right)
                return GetNodesOnSectorRowNoBlocked(new Vector2(sectorStart.right, sectorStart.top), Vector2.up, area);

            return null;
        }


        private List<Tile> GetNodesOnSectorRowNoBlocked(Vector2 start, Vector2 direction, WorldArea area)
        {
            List<Tile> returnList = new List<Tile>();
            int x;
            int y;

            for (int i = 0; i < GetSectorWidthAtLevel(area, 0); i++)
            {
                x = (int)start.x + (int)direction.x * i;
                y = (int)start.y + (int)direction.y * i;

                if (x > -1 && x < area.gridWidth && y > -1 && y < area.gridLength)
                {
                    Tile tile = area.tileGrid[x][y];
                    if (tile != null && !tile.blocked)
                        returnList.Add(tile);
                }
            }
            return returnList;
        }

        public List<Tile> GetTilesInSector(MultiLevelSector sector, WorldArea area)
        {
            List<Tile> tiles = new List<Tile>();

            for (int x = 0; x < sector.tilesInWidth; x++)
            {
                for (int y = 0; y < sector.tilesInHeight; y++)
                {
                    Tile tile = area.tileGrid[sector.left + x][sector.top + y];
                    if (tile != null && !tile.blocked)
                        tiles.Add(tile);
                }
            }

            return tiles;
        }







        //////////////////////////////////////////////////////////////////////


        public MultiLevelSector GetSectorOfTile(int level, Tile tile, WorldArea area)
        {
            int x = Mathf.FloorToInt(tile.gridPos.x / (float)GetSectorWidthAtLevel(area, level));
            int y = Mathf.FloorToInt(tile.gridPos.y / (float)GetSectorHeightAtLevel(area, level));

            int index = (y * GetSectorGridWidthAtLevel(area, level)) + x;

            return area.sectorGrid[level][index];
        }

        public int GetSectorWidthAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.levelDimensions[level][0];
        }
        public int GetSectorHeightAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.levelDimensions[level][1];
        }
        public int GetSectorGridWidthAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.levelDimensions[level][2];
        }
        public int GetSectorGridHeightAtLevel(WorldArea worldLayer, int level)
        {
            return worldLayer.levelDimensions[level][3];
        }


        public MultiLevelSector GetHigherSectorFromLower(int level, MultiLevelSector sector, WorldArea area)
        {
            int x = Mathf.FloorToInt(sector.left / (float)GetSectorWidthAtLevel(area, level));
            int y = Mathf.FloorToInt(sector.top / (float)GetSectorHeightAtLevel(area, level));

            int index = (y * GetSectorGridWidthAtLevel(area, level)) + x;
            return area.sectorGrid[level][index];
        }

        public int[][] GetLowerSectorsFromHigher(int level, int index, WorldArea area)
        {
            if (level > 0)
                return area.lookUpLowerSectors[level - 1][index];
            else
            {
                Debug.Log("Cant get sectors any lower");
                return null;
            }
        }

        public int FlipDirection(int dir)
        {
            if (dir == 0)
                return 1;
            else if (dir == 1)
                return 0;
            else if (dir == 2)
                return 3;
            else if (dir == 3)
                return 2;
            else
                return -1;

        }


        public List<MultiLevelSector> GetNeighboursWithinWorldArea(MultiLevelSector sector, WorldArea area)
        {
            List<MultiLevelSector> neighbours = new List<MultiLevelSector>();

            int sectorsGridHeight = GetSectorGridHeightAtLevel(area, sector.level);
            int sectorsGridWidth = GetSectorGridWidthAtLevel(area, sector.level);

            int checkX = sector.gridX + 1;
            int checkY = sector.gridY;
            if (checkX >= 0 && checkX < sectorsGridWidth)
                neighbours.Add(area.sectorGrid[sector.level][(checkY * sectorsGridWidth) + checkX]);


            checkX = sector.gridX - 1;
            checkY = sector.gridY;
            if (checkX >= 0 && checkX < sectorsGridWidth)
                neighbours.Add(area.sectorGrid[sector.level][(checkY * sectorsGridWidth) + checkX]);


            checkX = sector.gridX;
            checkY = sector.gridY - 1;
            if (checkY >= 0 && checkY < sectorsGridHeight)
                neighbours.Add(area.sectorGrid[sector.level][(checkY * sectorsGridWidth) + checkX]);


            checkX = sector.gridX;
            checkY = sector.gridY + 1;
            if (checkY >= 0 && checkY < sectorsGridHeight)
                neighbours.Add(area.sectorGrid[sector.level][(checkY * sectorsGridWidth) + checkX]);

            return neighbours;
        }

        public List<int> GetNeighboursIndexes(MultiLevelSector sector, WorldArea area)
        {
            List<int> neighbours = new List<int>();

            foreach (MultiLevelSector neighbourSector in GetNeighboursWithinWorldArea(sector, area))
                neighbours.Add(neighbourSector.ID);

            return neighbours;
        }


        public Vector2 EdgeIndexToVector(int edge)
        {
            if (edge == 0)
                return -Vector2.up;
            else if (edge == 1)
                return Vector2.up;
            else if (edge == 2)
                return -Vector2.right;
            else if (edge == 3)
                return Vector2.right;

            return Vector2.zero;
        }

        public bool LowerSectorEdgeMatchesHigher(MultiLevelSector highSector, MultiLevelSector lowSector, int edgeIndex)
        {
            if (edgeIndex == 0 && highSector.top == lowSector.top)
                return true;
            if (edgeIndex == 1 && highSector.bottom == lowSector.bottom)
                return true;
            if (edgeIndex == 2 && highSector.left == lowSector.left)
                return true;
            if (edgeIndex == 3 && highSector.right == lowSector.right)
                return true;

            return false;
        }



    }
}
