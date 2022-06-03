using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class LowLevelSectorManager
    {
        public MultiLevelSectorManager manager;
        List<Tile> openSet = new List<Tile>();
        List<Tile> closedSet = new List<Tile>();

        public void SetupSectorConnections(WorldArea area)
        {
            // build connections on the lowest level
            foreach (MultiLevelSector sector in area.sectorGrid[0])
            {
                if (manager.worldData.pathfinder.maxLevelAmount != 0)
                {
                    //bot
                    if (sector.bottom < area.gridLength - 1)
                        RebuildNodesOnSectorEdge(sector, 1, 0, new Vector2(sector.left, sector.bottom), new Vector2(sector.left, sector.bottom + 1), Vector2.right, area);

                    //right
                    if (sector.right < area.gridWidth - 1)
                        RebuildNodesOnSectorEdge(sector, 3, 2, new Vector2(sector.right, sector.top), new Vector2(sector.right + 1, sector.top), Vector2.up, area);
                }
                // recalculate sector sector nodes dictances
                ReCalculateDistancesSectorNodes(sector, area);
            }
        }


        public void RebuildNodesOnSectorEdges(List<int> sides, int sectorIndex, WorldArea area)
        {
            Vector2 startInSector = Vector2.zero;
            Vector2 startInNeighbourSector = Vector2.zero;
            Vector2 direction = Vector2.zero;
            MultiLevelSector sector = area.sectorGrid[0][sectorIndex];

            foreach (int side in sides)
            {
                if (side == 0)
                {
                    startInSector = new Vector2(sector.left, sector.top);
                    startInNeighbourSector = new Vector2(sector.left, sector.top - 1);
                    direction = Vector2.right;
                }
                else if (side == 1)
                {
                    startInSector = new Vector2(sector.left, sector.bottom);
                    startInNeighbourSector = new Vector2(sector.left, sector.bottom + 1);
                    direction = Vector2.right;
                }
                else if (side == 2)
                {
                    startInSector = new Vector2(sector.left, sector.top);
                    startInNeighbourSector = new Vector2(sector.left - 1, sector.top);
                    direction = Vector2.up;
                }
                else if (side == 3)
                {
                    startInSector = new Vector2(sector.right, sector.top);
                    startInNeighbourSector = new Vector2(sector.right + 1, sector.top);
                    direction = Vector2.up;
                }

                RebuildNodesOnSectorEdge(sector, side, manager.FlipDirection(side), startInSector, startInNeighbourSector, direction, area);

            }
        }


        public void RebuildNodesOnSectorEdge(MultiLevelSector sector, int edgeIndex, int edgeIndexNeighbourSector, Vector2 startInSector, Vector2 startInNeighbourSector, Vector2 direction, WorldArea area)
        {
            // remove connections to sector nodes on edge + remove them and those directly linked on neighbour sector
            manager.RemoveAllAbstractNodesOnSectorEdge(sector, edgeIndex);

            int maxStep = 0;
            if (direction == Vector2.right)
                maxStep = sector.tilesInWidth;
            else
                maxStep = sector.tilesInHeight;


            int sec = -1;
            for (int i = 0; i < maxStep; i++)
            {
                Tile neighbour = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * i][(int)startInNeighbourSector.y + (int)direction.y * i];
                if (neighbour != null)
                {
                    sec = neighbour.sectorIndex;
                    manager.RemoveAllAbstractNodesOnSectorEdge(area.sectorGrid[0][sec], edgeIndexNeighbourSector);
                    break;
                }
            }

            if (sec != -1) // if we havent found any tiles, no reason to try and build connections
            {
                // build nodes on edge
                bool sectorNodesOpen = false;
                int openLength = -1;
                int startNodeOfGroup = 0;

                Tile tile1;
                Tile tile2;
                for (int i = 0; i < maxStep; i++)
                {
                    tile1 = area.tileGrid[(int)startInSector.x + (int)direction.x * i][(int)startInSector.y + (int)direction.y * i];
                    tile2 = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * i][(int)startInNeighbourSector.y + (int)direction.y * i];

                    if (tile1 != null && tile2 != null && !tile1.blocked && !tile2.blocked && manager.worldData.tileManager.TilesWithinRangeGeneration(tile1, tile2))
                    {
                        // starting point of a new connection/gate between sectors
                        if (!sectorNodesOpen)
                            sectorNodesOpen = true;

                        openLength++;
                    }
                    else
                    {
                        if (sectorNodesOpen) // if we have had a couple of open nodes couples
                        {
                            // small enough to represent with 1 transition
                            if (openLength < manager.maxGateSize)
                            {
                                int steps = Mathf.FloorToInt(openLength * 0.5f) + startNodeOfGroup;
                                Tile neighbourTile = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * steps][(int)startInNeighbourSector.y + (int)direction.y * steps];
                                manager.CreateSectorNodes(sector, neighbourTile.sectorIndex, edgeIndex, edgeIndexNeighbourSector, area.tileGrid[(int)startInSector.x + (int)direction.x * steps][(int)startInSector.y + (int)direction.y * steps], neighbourTile, area);
                            }
                            else
                            {
                                // to large, 2 transitions. on on each end
                                int multiplyer = startNodeOfGroup;
                                Tile neighbourTile = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * multiplyer][(int)startInNeighbourSector.y + (int)direction.y * multiplyer];
                                manager.CreateSectorNodes(sector, neighbourTile.sectorIndex, edgeIndex, edgeIndexNeighbourSector, area.tileGrid[(int)startInSector.x + (int)direction.x * multiplyer][(int)startInSector.y + (int)direction.y * multiplyer], neighbourTile, area);

                                multiplyer = (startNodeOfGroup + openLength);
                                neighbourTile = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * multiplyer][(int)startInNeighbourSector.y + (int)direction.y * multiplyer];
                                manager.CreateSectorNodes(sector, neighbourTile.sectorIndex, edgeIndex, edgeIndexNeighbourSector, area.tileGrid[(int)startInSector.x + (int)direction.x * multiplyer][(int)startInSector.y + (int)direction.y * multiplyer], neighbourTile, area);
                            }

                            openLength = -1;
                            sectorNodesOpen = false;
                        }
                        startNodeOfGroup = i + 1;
                    }
                }

                if (sectorNodesOpen) // if we have had a couple of open nodes couples
                {
                    if (openLength < manager.maxGateSize)
                    {
                        int steps = Mathf.FloorToInt(openLength * 0.5f) + startNodeOfGroup;
                        Tile neighbourTile = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * steps][(int)startInNeighbourSector.y + (int)direction.y * steps];
                        manager.CreateSectorNodes(sector, neighbourTile.sectorIndex, edgeIndex, edgeIndexNeighbourSector, area.tileGrid[(int)startInSector.x + (int)direction.x * steps][(int)startInSector.y + (int)direction.y * steps], neighbourTile, area);
                    }
                    else
                    {
                        // to large, 2 transitions. on on each end
                        int multiplyer = startNodeOfGroup;
                        Tile neighbourTile = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * multiplyer][(int)startInNeighbourSector.y + (int)direction.y * multiplyer];
                        manager.CreateSectorNodes(sector, neighbourTile.sectorIndex, edgeIndex, edgeIndexNeighbourSector, area.tileGrid[(int)startInSector.x + (int)direction.x * multiplyer][(int)startInSector.y + (int)direction.y * multiplyer], neighbourTile, area);

                        multiplyer = (startNodeOfGroup + openLength);
                        neighbourTile = area.tileGrid[(int)startInNeighbourSector.x + (int)direction.x * multiplyer][(int)startInNeighbourSector.y + (int)direction.y * multiplyer];
                        manager.CreateSectorNodes(sector, neighbourTile.sectorIndex, edgeIndex, edgeIndexNeighbourSector, area.tileGrid[(int)startInSector.x + (int)direction.x * multiplyer][(int)startInSector.y + (int)direction.y * multiplyer], neighbourTile, area);
                    }
                }
            }
        }

        // recalculate distances to other high level nodes
        public void ReCalculateDistancesSectorNodes(MultiLevelSector sector, WorldArea area)
        {
            foreach (List<AbstractNode> list in sector.sectorNodesOnEdge)
            {
                foreach (AbstractNode node in list)
                    ConnectNodeInSector(sector, node, area);
            }
        }

        public void ConnectWorldAreaNodes(MultiLevelSector sector, WorldArea area)
        {
            foreach (AbstractNode node in sector.worldAreaNodes.Keys)
                ConnectNodeInSector(sector, node, area);
        }

        // cacluate distances to other high level nodes
        public void ConnectNodeInSector(MultiLevelSector sector, AbstractNode sectorNode, WorldArea area)
        {
            int maxNodes = sector.worldAreaNodes.Count + sector.sectorNodesOnEdge[0].Count + sector.sectorNodesOnEdge[1].Count + sector.sectorNodesOnEdge[2].Count + sector.sectorNodesOnEdge[3].Count;

            //Debug.Log("lowlevel sector ConnectNodeInSector " + maxNodes);

            // 2 sectorNodes on same location, connect them
            if (area.tileSectorNodeConnections[sector.level][sectorNode.tileConnection].Count > 1 && area.tileSectorNodeConnections[sector.level].ContainsKey(sectorNode.tileConnection))
            {
                maxNodes--;
                manager.ConnectSectorNodes(area.tileSectorNodeConnections[sector.level][sectorNode.tileConnection][0], area.tileSectorNodeConnections[sector.level][sectorNode.tileConnection][1], 0);
            }

            openSet.Clear();
            closedSet.Clear();

            openSet.Add(sectorNode.tileConnection);
            openSet[0].integrationValue = 0;

            // Debug.Log("max nodes " + maxNodes);
            while (openSet.Count > 0 && maxNodes != 0)
            {
                Tile currentNode = openSet[0];

                foreach (Tile neighbour in manager.worldData.tileManager.GetAllNeighboursForSectorNodeSearch(currentNode, area))
                {
                    //Debug.Log("neighbur");
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);

                        // if true, there is a higher node here
                        if (neighbour.hasAbstractNodeConnection)//.higherLevelNodeIndex.Count > 0)
                        {
                            //Debug.Log("found connection");
                            //Get all HigherNodes on this Lower Node  & connect them
                            List<AbstractNode> neighbourSectorNodes = area.tileSectorNodeConnections[sector.level][neighbour];// GetHigherLevelNodeList(neighbour, sector);
                            manager.ConnectSectorNodes(sectorNode, neighbourSectorNodes, neighbour.integrationValue / 10); // 10 times scaling   //more accurate but slower/ Mathf.FloorToInt((neighbour.totalPathCost / 10f) + 0.5f)
                            maxNodes -= neighbourSectorNodes.Count;
                        }
                    }
                }

                closedSet.Add(currentNode);
                openSet.Remove(currentNode);
            }


            //Debug.Log("sectorNode connections " + sectorNode.connections.Count);



            // reset
            foreach (Tile tile in openSet)
                tile.integrationValue = manager.worldData.tileManager.tileResetIntegrationValue;

            foreach (Tile tile in closedSet)
                tile.integrationValue = manager.worldData.tileManager.tileResetIntegrationValue;
        }
    }
}
