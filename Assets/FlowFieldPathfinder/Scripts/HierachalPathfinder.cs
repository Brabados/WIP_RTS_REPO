using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class HierachalPathfinder
    {
        bool limitPrevious = false;

        public WorldData worldData;

        Dictionary<IntVector2, bool> validSectorsFound = new Dictionary<IntVector2, bool>();
        public List<AbstractNode> previousAbstractNodes = new List<AbstractNode>();

        AbstractNode startMultiSectorNode = null;
        AbstractNode destinationMultiSectorNode = null;

        MultiLevelSector startNodeSector = null;
        MultiLevelSector destinationNodeSector = null;

        private List<AbstractNode> multiSectorNodes;
        private int previousNodesStartingPointCount = 0;

        private List<List<int>> sectorIndexesAllLowerPaths = new List<List<int>>();



        private void AddToPath(List<int> areasAndSectors)
        {
            sectorIndexesAllLowerPaths.Add(areasAndSectors);
        }

        // find path on higher level between start and goal/destination
        public List<List<int>> FindPaths(Dictionary<IntVector2, Tile> startingPoints, Tile destinationTile, WorldArea destinationArea)
        {
            multiSectorNodes = new List<AbstractNode>();

            int maxLevelAmount = worldData.pathfinder.maxLevelAmount;
            if (maxLevelAmount == 0)
            {
                if (worldData.pathfinder.worldIsMultiLayered)
                {
                    Debug.Log("Invalid search: A  Multi Layerd world cannot find paths with no Levels Of Abstraction ");
                    return null;
                }
                else
                    maxLevelAmount = 1;
            }
            // adding all the start/destination nodes per level.
            for (int i = 0; i < maxLevelAmount; i++)
            {
                foreach (IntVector2 key in startingPoints.Keys)
                {
                    Tile startTile = startingPoints[key];

                    startNodeSector = worldData.multiLevelSectorManager.GetSectorOfTile(i, startTile, worldData.worldAreas[key.x]);
                    startMultiSectorNode = worldData.multiLevelSectorManager.CreateAbstractNodeInSector(startNodeSector, startTile, worldData.worldAreas[key.x]);

                    worldData.multiLevelSectorManager.ConnectNodeInSector(startNodeSector, startMultiSectorNode, worldData.worldAreas[key.x]);//, new List<HigherNode>()); 
                    multiSectorNodes.Add(startMultiSectorNode);
                }

                // create temporary highlevel node on the start node
                // calculate its distance to the other high level nodes in the same sector, build connections between them
                destinationNodeSector = worldData.multiLevelSectorManager.GetSectorOfTile(i, destinationTile, destinationArea);
                destinationMultiSectorNode = worldData.multiLevelSectorManager.CreateAbstractNodeInSector(destinationNodeSector, destinationTile, destinationArea);
                worldData.multiLevelSectorManager.ConnectNodeInSector(destinationNodeSector, destinationMultiSectorNode, destinationArea);
 
                multiSectorNodes.Add(destinationMultiSectorNode);
            }

            bool nextStep = false;

            AbstractNode start;
            AbstractNode destination;

            previousNodesStartingPointCount = startingPoints.Count;
            sectorIndexesAllLowerPaths.Clear();


            for (int j = 0; j < startingPoints.Count; j++) // for each starting point
            {
                for (int i = multiSectorNodes.Count - 1; i > -1; i -= startingPoints.Count + 1)
                {
                    nextStep = false;
                    int level = i / (startingPoints.Count + 1);

                    start = multiSectorNodes[i - (j + 1)];
                    destination = multiSectorNodes[i];

                    if (start.sector == destination.sector)// starting point == destination
                    {
                        if (worldData.pathfinder.maxLevelAmount == 0)
                        {
                            AddToPath(new List<int> { start.worldAreaIndex, start.sector });
                            nextStep = true;
                        }
                        else
                        {
                            foreach (AbstractNode sectorNode in destination.connections.Keys)
                            {
                                if (sectorNode == start)// direct connection in sector
                                {

                                    if (level != 0) // go to the next layer
                                        nextStep = true;
                                    else // level = 0, our goal is right next to us

                                        AddToPath(new List<int> { start.worldAreaIndex, start.sector });

                                    break;
                                }
                            }
                        }
                    }

                    //validSectorsFound
                    if (!nextStep)
                    {
                        if (level != 0) // we have to search the path between them, using A-star on the high level network
                        {
                            List<int> sectorIndexes = null;

                            if (validSectorsFound.Count == 0)
                                sectorIndexes = SearchHighLevelPath(start, destination, true);
                            else
                                sectorIndexes = SearchHighLevelPath(start, destination, validSectorsFound, true);


                            if (sectorIndexes == null)
                            {
                                Debug.Log("indexes ARE null,  no Path  on this hierachal Level: " + level);
                            }
                            else
                            {
                                validSectorsFound.Clear();
                                for (int k = 0; k < sectorIndexes.Count; k += 2)
                                {
                                    foreach (int[] list in worldData.multiLevelSectorManager.GetLowerSectorsFromHigher(level, sectorIndexes[k + 1], worldData.worldAreas[sectorIndexes[k]]))
                                    {
                                        foreach (int lowerIndex in list)
                                            validSectorsFound.Add(new IntVector2(sectorIndexes[k], lowerIndex), false);
                                    }
                                }
                            }
                        }
                        else // lowest level reached
                        {
                            List<int> sectorIndexes = null;

                            if (validSectorsFound.Count == 0)
                                sectorIndexes = SearchHighLevelPath(start, destination, true);
                            else
                                sectorIndexes = SearchHighLevelPath(start, destination, validSectorsFound, true);

                            validSectorsFound.Clear();

                            if (sectorIndexes != null)
                                AddToPath(sectorIndexes);
                        }
                    }

                }

            }

            return sectorIndexesAllLowerPaths;
        }




        private List<int> SearchHighLevelPath(AbstractNode start, AbstractNode destination, bool returnPath)
        {      
            Heap<AbstractNode> openSet = new Heap<AbstractNode>(3000);
            HashSet<AbstractNode> closedSet = new HashSet<AbstractNode>();
            openSet.Add(start);
            while (openSet.Count > 0)
            {
                AbstractNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == destination)
                {
                    //Debug.Log("Retrace Path");
                    return RetracePath(start, destination, returnPath);
                }
                foreach (AbstractNode neighbour in currentNode.connections.Keys)
                {

                    if (closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.G + currentNode.connections[neighbour];
                    if (newMovementCostToNeighbour < neighbour.G || !openSet.Contains(neighbour))
                    {
                        neighbour.G = newMovementCostToNeighbour;

                        // divide by 10, G works on a factor 10 smaller
                        neighbour.H = GetDistance(neighbour, destination);// / 10;
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
            Debug.Log("no High Level path found");

            return null;
        }

        private List<int> SearchHighLevelPath(AbstractNode start, AbstractNode destination, Dictionary<IntVector2, bool> validSectors, bool returnPath)
        {
            Heap<AbstractNode> openSet = new Heap<AbstractNode>(2000);
            HashSet<AbstractNode> closedSet = new HashSet<AbstractNode>();
            IntVector2 neigbourVector;
            openSet.Add(start);
            while (openSet.Count > 0)
            {
                AbstractNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == destination)
                    return RetracePath(start, destination, returnPath);

                foreach (AbstractNode neighbour in currentNode.connections.Keys)
                {
                    neigbourVector = new IntVector2(neighbour.worldAreaIndex, neighbour.sector);
                    if (closedSet.Contains(neighbour) || !validSectors.ContainsKey(neigbourVector))
                        continue;

                    int newMovementCostToNeighbour = currentNode.G + currentNode.connections[neighbour];
                    if (newMovementCostToNeighbour < neighbour.G || !openSet.Contains(neighbour))
                    {
                        neighbour.G = newMovementCostToNeighbour;
                        neighbour.H = GetDistance(neighbour, destination);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }

            return null;
        }


        private int GetDistance(AbstractNode sectorA, AbstractNode sectorB)
        {
            Vector3 a = worldData.tileManager.GetTileWorldPosition(sectorA.tileConnection, worldData.worldAreas[sectorA.worldAreaIndex]);
            Vector3 b = worldData.tileManager.GetTileWorldPosition(sectorB.tileConnection, worldData.worldAreas[sectorB.worldAreaIndex]);

            return (int)((Vector3.Distance(a, b) + 0.5f) / worldData.pathfinder.tileSize);
        }


        private List<int> RetracePath(AbstractNode startNode, AbstractNode endNode, bool retracePath)
        {
            if (!limitPrevious)
                previousAbstractNodes.Clear();

            List<int> path = new List<int>();
            if (retracePath)
            {
                AbstractNode currentNode = endNode;
                if (!limitPrevious)
                    previousAbstractNodes.Add(currentNode);// visual
                path.Add(currentNode.worldAreaIndex);
                path.Add(currentNode.sector);

                while (currentNode != startNode)
                {// if sector OR worldArea dont match, its a new one
                    if (path[path.Count - 1] != currentNode.sector || path[path.Count - 2] != currentNode.worldAreaIndex)
                    {
                        path.Add(currentNode.worldAreaIndex);
                        path.Add(currentNode.sector);
                    }

                    if (!limitPrevious)
                        previousAbstractNodes.Add(currentNode);// visual
                    currentNode = currentNode.parent;
                }

                if (path[path.Count - 1] != currentNode.sector)
                {
                    if (!limitPrevious)
                        previousAbstractNodes.Add(currentNode); // visual
                    path.Add(currentNode.worldAreaIndex);
                    path.Add(currentNode.sector);

                }
            }
            else
            {
                path.Add(0);
                AbstractNode currentNode = endNode;

                while (currentNode != startNode)
                {
                    if (currentNode.connections.ContainsKey(currentNode.parent))
                        path[0] += currentNode.connections[currentNode.parent];
                    else
                        path[0] += 1;

                    currentNode = currentNode.parent;
                }
            }


            return path;
        }

        public void RemovePreviousSearch()
        {
            for (int i = multiSectorNodes.Count - 1; i > -1; i -= previousNodesStartingPointCount + 1)
            {
                int level = (i / (previousNodesStartingPointCount + 1));

                for (int j = 0; j < previousNodesStartingPointCount + 1; j++)
                    worldData.multiLevelSectorManager.RemoveAbstractNode(level, multiSectorNodes[i - j]);
            }
        }


        public void FindConnectionInsideSectorOnly(AbstractNode start, AbstractNode destination, MultiLevelSector sector, WorldArea area)
        {
            // if we look for ourself or already have a connection, we are done here
            if (start == destination || start.connections.ContainsKey(destination))
                return;

            int lowerLevel = sector.level - 1;

            if (!area.tileSectorNodeConnections[lowerLevel].ContainsKey(start.tileConnection))
                Debug.Log("node index  " + start.worldAreaIndex + "   actual world index  " + area.index + "   start high " + start.tileConnection.gridPos.x + "   y " + start.tileConnection.gridPos.y);

            int connectionDistance = FindConnectionThroughPathInsideSectorOnly(lowerLevel,
                area.tileSectorNodeConnections[lowerLevel][start.tileConnection],
                area.tileSectorNodeConnections[lowerLevel][destination.tileConnection],
                worldData.multiLevelSectorManager.GetLowerSectorsFromHigher(sector.level, sector.ID, area));

            if (connectionDistance > -1)
                worldData.multiLevelSectorManager.ConnectSectorNodes(start, destination, connectionDistance);
        }

        public int FindConnectionThroughPathInsideSectorOnly(int level, List<AbstractNode> start, List<AbstractNode> destination, int[][] sectorIndexes)
        {
            Dictionary<IntVector2, bool> lowerSectors = new Dictionary<IntVector2, bool>();
            foreach (int[] list in sectorIndexes)
            {
                foreach (int index in list)
                    lowerSectors.Add(new IntVector2(start[0].worldAreaIndex, index), false);
            }


            List<int> connectionDistance = SearchHighLevelPath(start[0], destination[0], lowerSectors, false);// new List<int>();

            if (connectionDistance != null && connectionDistance.Count > 0)
                return connectionDistance[0];
            else
            {
                //no connection path
                return -1;
            }
        }





    }
}
