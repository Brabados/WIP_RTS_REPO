using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace FlowPathfinding
{
    public class WorldManager
    {
        public WorldData worldData;

        public List<Tile> tilesBlockedAdjusted = new List<Tile>();
        public List<Tile> tilesCostAdjusted = new List<Tile>();

        List<IntVector2> sectorChanges = new List<IntVector2>();
        Dictionary<IntVector2, List<int>> sectorEdgeChangesLowLevel = new Dictionary<IntVector2, List<int>>();
        Dictionary<IntVector3, int> sectorEdgeChangesHighLevel = new Dictionary<IntVector3, int>();


        public void BlockTile(Tile tile)
        {
            if (tile != null && !tile.blocked)
                SetBlock(tile);
        }

        public void UnBlockTile(Tile tile)
        {
            if (tile != null && tile.blocked)
                RemoveBlock(tile);
        }

        public void SetTileCost(Tile tile, int cost)
        {
            if (tile != null)
                CostChanged(tile, cost);
        }




        private void CostChanged(Tile tile, int value)
        {
            tile.cost = value;
            if (!tilesCostAdjusted.Contains(tile))
                tilesCostAdjusted.Add(tile);
        }

        private void SetBlock(Tile tile)
        { 
            tile.blocked = true;
            tile.cost += worldData.tileManager.tileBlockedValue;

            if (tilesBlockedAdjusted.Contains(tile))
                tilesBlockedAdjusted.Remove(tile);
            else
                tilesBlockedAdjusted.Add(tile);
        }


        private void RemoveBlock(Tile tile)
        {
            tile.blocked = false;
            tile.cost -= worldData.tileManager.tileBlockedValue;

            if (tilesBlockedAdjusted.Contains(tile))
                tilesBlockedAdjusted.Remove(tile);
            else
                tilesBlockedAdjusted.Add(tile);
        }



        private void AddChange(IntVector2 key, Tile tile, WorldArea area, int side)
        {
            IntVector2 otherSectorKey = new IntVector2();
            int otherSector = -1;
            // if other side of the sector has already been added, we dont need to add this
            if (side == 0)
                otherSector = tile.sectorIndex - worldData.multiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0);
            else if (side == 1)
                otherSector = tile.sectorIndex + worldData.multiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0);
            else if (side == 2 && area.sectorGrid[0][tile.sectorIndex].gridX > 0)
                otherSector = tile.sectorIndex - 1;
            else if (side == 3 && area.sectorGrid[0][tile.sectorIndex].gridX < worldData.multiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0) - 1)
                otherSector = tile.sectorIndex + 1;

            if (worldData.pathfinder.maxLevelAmount > 1)
            {
                // add highlevel sector changes
                if (worldData.multiLevelSectorManager.GetHigherSectorFromLower(1, area.sectorGrid[0][tile.sectorIndex], area) != worldData.multiLevelSectorManager.GetHigherSectorFromLower(1, area.sectorGrid[0][otherSector], area))
                {
                    IntVector3 highLevelEdgeKey = new IntVector3(area.index, 0, 0);
                    int sideValue = 0;
                    if (tile.sectorIndex < otherSector)
                    {
                        sideValue = side;
                        highLevelEdgeKey.y = tile.sectorIndex;
                        highLevelEdgeKey.z = otherSector;
                    }
                    else
                    {
                        sideValue = worldData.multiLevelSectorManager.FlipDirection(side);
                        highLevelEdgeKey.y = otherSector;
                        highLevelEdgeKey.z = tile.sectorIndex;
                    }

                    if (!sectorEdgeChangesHighLevel.ContainsKey(highLevelEdgeKey))
                        sectorEdgeChangesHighLevel.Add(highLevelEdgeKey, sideValue);
                }
            }


            if (otherSector > 0 && otherSector < area.sectorGrid[0].Length)
            {
                otherSectorKey.x = tile.worldAreaIndex;
                otherSectorKey.y = otherSector;

                if (sectorEdgeChangesLowLevel.ContainsKey(otherSectorKey))
                {
                    if (sectorEdgeChangesLowLevel[otherSectorKey].Contains(worldData.multiLevelSectorManager.FlipDirection(side)))// other side already filled in
                    {
                        if (!sectorChanges.Contains(key))// other sector exist and the side. add our sector for general change
                            sectorChanges.Add(key);
                    }
                    else if (!sectorEdgeChangesLowLevel[key].Contains(side)) //  other sector exist but not the side. add our sector for Edge change
                        sectorEdgeChangesLowLevel[key].Add(side);
                }
                else// other sector not (yet? )added.   add ourselves and other sector for genral change
                {
                    if (!sectorChanges.Contains(otherSectorKey))
                        sectorChanges.Add(otherSectorKey);

                    if (!sectorEdgeChangesLowLevel[key].Contains(side))
                        sectorEdgeChangesLowLevel[key].Add(side);
                }
            }
            else if (!sectorEdgeChangesLowLevel[key].Contains(side))// other sector does not exist, add ourselves
                sectorEdgeChangesLowLevel[key].Add(side);
        }


        private void RemoveWorldAreaNodes(IntVector3 key)
        {
            WorldArea area = worldData.worldAreas[key.x];
            List<AbstractNode> abstractNodes = new List<AbstractNode>(area.sectorGrid[0][key.z].worldAreaNodes.Keys);
            AbstractNode abstractNode;

            for (int i = 0; i < abstractNodes.Count; i ++ )
            {
                abstractNode = abstractNodes[i];
                if (area.sectorGrid[0][key.z].worldAreaNodes[abstractNode] == key.y)
                {
                    List<AbstractNode> abstractNodes2 = null;
                    WorldArea otherArea = worldData.worldAreas[key.y];
                    MultiLevelSector otherSector = otherArea.sectorGrid[0][abstractNode.nodeConnectionToOtherSector.tileConnection.sectorIndex];

                    // remove in other connected sector first
                    worldData.multiLevelSectorManager.RemoveAbstractNode(0, abstractNode.nodeConnectionToOtherSector);
                    otherSector.worldAreaNodes.Remove(abstractNode.nodeConnectionToOtherSector);

                    // visual
                    otherSector.SearchConnections();

                    if (worldData.pathfinder.maxLevelAmount > 1)
                    {
                        abstractNodes2 = new List<AbstractNode>(worldData.multiLevelSectorManager.GetHigherSectorFromLower(1, otherSector, otherArea).worldAreaNodes.Keys);
                        AbstractNode worldAreaNode;
                        for (int j = 0; j < abstractNodes2.Count; j++)
                        {
                            worldAreaNode = abstractNodes2[j];
                            if (worldAreaNode.nodeConnectionToOtherSector.worldAreaIndex == area.index && worldAreaNode.tileConnection.sectorIndex == abstractNode.nodeConnectionToOtherSector.tileConnection.sectorIndex) // if this node connects with other, and in the right lower sector
                            {
                                worldData.multiLevelSectorManager.RemoveAbstractNode(1, worldAreaNode);
                                otherArea.sectorGrid[1][worldAreaNode.sector].worldAreaNodes.Remove(worldAreaNode);

                                // visual
                                otherArea.sectorGrid[1][worldAreaNode.sector].SearchConnections();
                            }
                        }
                    }


                    worldData.multiLevelSectorManager.RemoveAbstractNode(0, abstractNode);
                    area.sectorGrid[0][key.z].worldAreaNodes.Remove(abstractNode);

                    // visual
                    area.sectorGrid[0][key.z].SearchConnections();

                    if (worldData.pathfinder.maxLevelAmount > 1)
                    {
                        abstractNodes2.Clear();
                        abstractNodes2.AddRange(worldData.multiLevelSectorManager.GetHigherSectorFromLower(1, area.sectorGrid[0][key.z], area).worldAreaNodes.Keys);

                        AbstractNode worldAreaNode;
                        for (int j = 0; j < abstractNodes2.Count; j++)
                        {
                            worldAreaNode = abstractNodes2[j];
                            if (worldAreaNode.nodeConnectionToOtherSector.worldAreaIndex == key.y && worldAreaNode.tileConnection.sectorIndex == key.z) // if this node connects with other, and in the right lower sector
                            {
                                worldData.multiLevelSectorManager.RemoveAbstractNode(1, worldAreaNode);

                                Debug.Log("area.sectorGrid  " + area.sectorGrid.Length + "   " + worldAreaNode.sector);
                                area.sectorGrid[1][worldAreaNode.sector].worldAreaNodes.Remove(worldAreaNode);

                                // visual
                                area.sectorGrid[1][worldAreaNode.sector].SearchConnections();
                            }
                        }
                    }
                }
            }
        }


        public void InputChanges()
        {
            if (worldData.pathfinder.maxLevelAmount != 0)
            {
                sectorChanges.Clear();
                sectorEdgeChangesLowLevel.Clear();
                sectorEdgeChangesHighLevel.Clear();
                MultiLevelSector sector;
                WorldArea area;
                IntVector2 key = new IntVector2();
                IntVector2 tilePosKey = new IntVector2();
                IntVector3 sectorsAreaKey = new IntVector3();
                List<IntVector3> sectorsAreaRedo = new List<IntVector3>();
                bool tileOnEdge = false;

                Tile tile;
                for (int i = 0; i < tilesBlockedAdjusted.Count; i ++ )
                {
                    tile = tilesBlockedAdjusted[i];
                    tileOnEdge = false;

                    if (tilesCostAdjusted.Contains(tile))
                        tilesCostAdjusted.Remove(tile);

                    tilePosKey = tile.gridPos;
                    key.x = tile.worldAreaIndex;
                    key.y = tile.sectorIndex;

                    if (!sectorEdgeChangesLowLevel.ContainsKey(key))
                        sectorEdgeChangesLowLevel.Add(key, new List<int>());

                    area = worldData.worldAreas[tile.worldAreaIndex];
                    sector = area.sectorGrid[0][tile.sectorIndex];

                    if (tile.gridPos.y == sector.top && sector.top != 0) //top
                    {
                        tileOnEdge = true;
                        if (!sectorEdgeChangesLowLevel[key].Contains(0))
                            AddChange(key, tile, area, 0);
                    }
                    if (tile.gridPos.y == sector.bottom && sector.gridY < worldData.multiLevelSectorManager.GetSectorGridHeightAtLevel(area, 0) - 1) //sector.bottom != worldData.tileManager.gridHeight - 1) //bot
                    {
                        tileOnEdge = true;
                        if (!sectorEdgeChangesLowLevel[key].Contains(1))
                            AddChange(key, tile, area, 1);
                    }
                    if (tile.gridPos.x == sector.left && sector.left != 0)//left
                    {
                        tileOnEdge = true;
                        if (!sectorEdgeChangesLowLevel[key].Contains(2))
                            AddChange(key, tile, area, 2);
                    }
                    if (tile.gridPos.x == sector.right && sector.gridX < worldData.multiLevelSectorManager.GetSectorGridWidthAtLevel(area, 0) - 1) //right
                    {
                        tileOnEdge = true;
                        if (!sectorEdgeChangesLowLevel[key].Contains(3))
                            AddChange(key, tile, area, 3);
                    }

                    if (!tileOnEdge)
                    {
                        if (!sectorChanges.Contains(key))
                            sectorChanges.Add(key);
                    }

                    // store tiles that will change how world areas connect
                    if (area.worldAreaTileConnections.ContainsKey(tilePosKey))
                    {
                        WorldArea tempArea;
                        sectorsAreaKey.z = area.tileGrid[tilePosKey.x][tilePosKey.y].sectorIndex;
                        foreach (IntVector3 value in area.worldAreaTileConnections[tilePosKey])
                        {
                            sectorsAreaKey.y = value.z;

                            if (area.index < sectorsAreaKey.x) // change nothing
                            {
                                tempArea = area;
                            }
                            else// we only generate these connections from the world area with the lowest index
                            {
                                tempArea = worldData.worldAreas[value.z];

                                sectorsAreaKey.y = area.index;
                                sectorsAreaKey.z = tempArea.tileGrid[value.x][value.y].sectorIndex;
                            }

                            sectorsAreaKey.x = tempArea.index;

                            if (!sectorsAreaRedo.Contains(sectorsAreaKey))
                            {
                                RemoveWorldAreaNodes(sectorsAreaKey);
                                sectorsAreaRedo.Add(sectorsAreaKey);
                            }
                        }
                    }
                }

                List<MultiLevelSector> higherSectors = new List<MultiLevelSector>();

                // rebuild sector edges
                List<IntVector2> keys =  new List<IntVector2>(sectorEdgeChangesLowLevel.Keys);
                IntVector2 indexKey;
                for (int i = 0; i < keys.Count; i++)
                { 
                    indexKey = keys[i];

                    area = worldData.worldAreas[indexKey.x];
                    worldData.multiLevelSectorManager.lowLevel.RebuildNodesOnSectorEdges(sectorEdgeChangesLowLevel[indexKey], indexKey.y, area);
                    worldData.multiLevelSectorManager.RemoveAllConnectionsWithinSector(area.sectorGrid[0][indexKey.y]);
                    // get all sectors that have to recalcualate
                    if (!sectorChanges.Contains(indexKey))
                        sectorChanges.Add(indexKey);
                }

 
                for (int i = 0; i < tilesCostAdjusted.Count; i++)
                { 
                    key.x = tilesCostAdjusted[i].worldAreaIndex;
                    key.y = tilesCostAdjusted[i].sectorIndex;

                    if (!sectorChanges.Contains(key))
                        sectorChanges.Add(key);
                }

                // now we must recalculate connections on sector edges
                for (int i = 0; i < sectorChanges.Count; i ++)
                {
                    indexKey = sectorChanges[i];

                    area = worldData.worldAreas[indexKey.x];
                    worldData.multiLevelSectorManager.lowLevel.ReCalculateDistancesSectorNodes(area.sectorGrid[0][indexKey.y], area);

                    if (worldData.pathfinder.maxLevelAmount > 1)
                    {
                        MultiLevelSector highSector = worldData.multiLevelSectorManager.GetHigherSectorFromLower(1, area.sectorGrid[0][indexKey.y], area);

                        if (!higherSectors.Contains(highSector))
                            higherSectors.Add(highSector);
                    }
                }

                List<IntVector3> _sectorEdgeChangesHighLevel = new List<IntVector3>(sectorEdgeChangesHighLevel.Keys);
                for (int i = 0; i < _sectorEdgeChangesHighLevel.Count; i ++ )
                {
                    worldData.multiLevelSectorManager.highLevel.RebuildNodesOnHighSectorEdges(_sectorEdgeChangesHighLevel[i], sectorEdgeChangesHighLevel[_sectorEdgeChangesHighLevel[i]]);
                }

                for (int i = 0; i < higherSectors.Count; i++)
                {
                    worldData.multiLevelSectorManager.highLevel.HighLevelSectorAdjustedRecalculate(higherSectors[i]);
                }

                //visual
                for (int i = 0; i < sectorChanges.Count; i++)
                {
                    indexKey = sectorChanges[i];
                    worldData.worldAreas[indexKey.x].sectorGrid[0][indexKey.y].SearchConnections();
                }


                // all sector connections fixed & correct world area nodes removed
                // rebuild world area nodes were we removed them
                for (int i = 0; i < sectorsAreaRedo.Count; i++)
                {
                    area = worldData.worldAreas[sectorsAreaRedo[i].x];
                    IntVector2 newKey = new IntVector2(sectorsAreaRedo[i].y, sectorsAreaRedo[i].z);

                    foreach (List<IntVector2> group in area.groupsInSectors[newKey])
                        worldData.worldBuilder.GenerateWordConnectingNodesPerGroup(area, group, newKey);
                }

            }

            tilesCostAdjusted.Clear();
            tilesBlockedAdjusted.Clear();
            worldData.pathfinder.WorldHasBeenChanged(sectorChanges);
        }


    }


}
