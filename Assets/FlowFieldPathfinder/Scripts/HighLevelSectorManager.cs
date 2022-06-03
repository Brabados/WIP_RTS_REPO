using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class HighLevelSectorManager
    {
        public MultiLevelSectorManager manager;

        public void SetupSectorConnections(WorldArea area)
        {
            // skip lowest level
            for (int i = 1; i < manager.worldData.pathfinder.maxLevelAmount; i++)
            {
                foreach (MultiLevelSector sector in area.sectorGrid[i])
                {
                    //bot
                    if (sector.bottom < area.gridLength - 1)
                        RebuildNodesOnHighSectorEdge(i, sector, 1, Vector2.up, area);

                    //right
                    if (sector.right < area.gridWidth - 1)
                        RebuildNodesOnHighSectorEdge(i, sector, 3, Vector2.right, area);

                    // recalculate sector sector nodes dictances
                    ReCalculateDistancesHigherSectorNodes(sector, area);
                }
            }

        }

        public void RebuildNodesOnHighSectorEdges(IntVector3 areaWithSectors, int side)
        {
            WorldArea area = manager.worldData.worldAreas[areaWithSectors.x];
            MultiLevelSector sector = area.sectorGrid[0][areaWithSectors.y];
            MultiLevelSector higherSector = manager.GetHigherSectorFromLower(1, sector, area);

            Debug.Log("Lower level  start " + areaWithSectors.y + "  end " + areaWithSectors.z);

            // clear out both sides  //area.sectorGrid[1][areaWithSectors.z]
            manager.RemoveAllAbstractNodesOnSectorEdge(higherSector, side);
            manager.RemoveAllAbstractNodesOnSectorEdge(manager.GetHigherSectorFromLower(1, area.sectorGrid[0][areaWithSectors.z], area), manager.FlipDirection(side));

            // rebuild side
            RebuildNodesOnHighSectorEdge(1, higherSector, side, manager.EdgeIndexToVector(side), area);
        }

        private void RebuildNodesOnHighSectorEdge(int level, MultiLevelSector sector, int edgeNumber, Vector2 direction, WorldArea area)
        {
            // get all lower level sectors within
            foreach (int[] list in manager.GetLowerSectorsFromHigher(level, sector.ID, area))//   .lookUpLowerSectors[level - 1][sector.ID])
            {
                // go through lowerLevel Sector (indexes), make copies of the abstract nodes on the correct edges
                foreach (int lowerLevelSectorIndex in list)
                {
                    MultiLevelSector lowerSector = area.sectorGrid[level - 1][lowerLevelSectorIndex];

                    if (manager.LowerSectorEdgeMatchesHigher(sector, lowerSector, edgeNumber)) // match edge
                    {
                        foreach (AbstractNode node in lowerSector.sectorNodesOnEdge[edgeNumber]) // get nodes to copy from the edge
                        {
                            int neighbourID = 0;
                            int neighbourEdgeNumber = 0;

                            if (edgeNumber == 0)
                            {
                                neighbourID = sector.ID - manager.GetSectorGridWidthAtLevel(area, level);// levelDimensions[level][2];
                                neighbourEdgeNumber = 1;
                            }
                            else if (edgeNumber == 1)
                            {
                                neighbourID = sector.ID + manager.GetSectorGridWidthAtLevel(area, level);// levelDimensions[level][2];
                                neighbourEdgeNumber = 0;
                            }
                            else if (edgeNumber == 2)
                            {
                                neighbourID = sector.ID - 1;
                                neighbourEdgeNumber = 3;
                            }
                            else if (edgeNumber == 3)
                            {
                                neighbourID = sector.ID + 1;
                                neighbourEdgeNumber = 2;
                            }

                            manager.CreateSectorNodes(sector, neighbourID, edgeNumber, neighbourEdgeNumber, node.tileConnection, manager.worldData.tileManager.GetTileInWorldArea(area, node.tileConnection.gridPos.x + (int)direction.x, node.tileConnection.gridPos.y + (int)direction.y), area);
                        }
                    }
                }
            }
        }

        public void ReCalculateDistancesHigherSectorNodes(MultiLevelSector sector, WorldArea area)
        {
            List<AbstractNode> allNodesInSector = new List<AbstractNode>();
            foreach (List<AbstractNode> list in sector.sectorNodesOnEdge)
                allNodesInSector.AddRange(list);
            allNodesInSector.AddRange(sector.worldAreaNodes.Keys);

            List<AbstractNode> allNodesInSector2 = new List<AbstractNode>(allNodesInSector);



            foreach (AbstractNode node in allNodesInSector2)
            {
                foreach (AbstractNode nodeToFind in allNodesInSector)
                    manager.worldData.hierachalPathfinder.FindConnectionInsideSectorOnly(node, nodeToFind, sector, area);
            }


            //foreach (List<AbstractNode> list in sector.sectorNodesOnEdge)
            //{
            //    foreach (AbstractNode node in list)
            //    {
            //        foreach (AbstractNode nodeToFind in allNodesInSector)
            //            manager.worldData.hierachalPathfinder.FindConnectionInsideSectorOnly(node, nodeToFind, sector, area);
            //    }
            //}
        }


        public void HighLevelSectorAdjustedRecalculate(MultiLevelSector sector)
        {
            WorldArea area = manager.worldData.worldAreas[sector.worldAreaIndex];
            manager.RemoveAllConnectionsWithinSector(sector);
            ReCalculateDistancesHigherSectorNodes(sector, area);

            // visual
            sector.SearchConnections();
        }


        public void ConnectWorldAreaNodes(MultiLevelSector sector, WorldArea area)
        {
            foreach (AbstractNode node in sector.worldAreaNodes.Keys)
                ConnectNodeInSector(sector, node, area);
        }

        public void ConnectNodeInSector(MultiLevelSector sector, AbstractNode node, WorldArea area)
        {
            List<AbstractNode> allNodesInSector = new List<AbstractNode>();
            foreach (List<AbstractNode> list in sector.sectorNodesOnEdge)
                allNodesInSector.AddRange(list);

            allNodesInSector.AddRange(sector.worldAreaNodes.Keys);

            foreach (AbstractNode nodeToFind in allNodesInSector)
                manager.worldData.hierachalPathfinder.FindConnectionInsideSectorOnly(node, nodeToFind, sector, area);
        }

    }
}
