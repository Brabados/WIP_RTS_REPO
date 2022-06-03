using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class FlowFieldManager
    {
        private Vector2[] flowVectors = new Vector2[] { Vector2.zero, Vector2.up, -Vector2.up, Vector2.right, new Vector2(1,1).normalized, new Vector2(1,-1).normalized, -Vector2.right, new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized };
        //0 = 0
        //1 = up
        //2 = down
        //3 = right
        //4 = right, top
        //5 = right, down
        //6 = left
        //7 = left, top
        //8 = left, down


        public WorldData worldData = null;
        public List<FlowFieldPath> flowFieldPaths = new List<FlowFieldPath>();

        // create flowfield
        public void CreateFlowFieldPath(List<Tile> tiles, List<int> sectors, List<int> worldAreas, Tile destination, SearchPathJob pathJob, WorldArea area, int key)
        {
            FlowFieldPath path = new FlowFieldPath();
            path.Create(destination, worldAreas, worldData.intergrationFieldManager.CreateIntergrationField(tiles, sectors, destination, area), CreateFlowField(sectors, area), key);
            flowFieldPaths.Add(path);
        }

        // add sectors/fields
        public void AddToFlowFieldPath(List<Tile> tiles, List<int> sectors, Tile destination, SearchPathJob pathJob, WorldArea area)
        {
            int amount = worldData.multiLevelSectorManager.GetSectorWidthAtLevel(area, 0) * worldData.multiLevelSectorManager.GetSectorHeightAtLevel(area, 0);

            flowFieldPaths[flowFieldPaths.Count - 1].intergrationField.AddFields(sectors, amount, tiles, area);
            flowFieldPaths[flowFieldPaths.Count - 1].flowField.AddFields(sectors, amount, area);
        }

        // add sectors/fields
        public void AddToFlowFieldPath(List<Tile> tiles, List<int> sectors, Tile destination, AddToPathJob pathJob, WorldArea area, FlowFieldPath path)
        {
            int amount = worldData.multiLevelSectorManager.GetSectorWidthAtLevel(area, 0) * worldData.multiLevelSectorManager.GetSectorHeightAtLevel(area, 0);

            path.intergrationField.AddFields(sectors, amount, tiles, area);
            path.flowField.AddFields(sectors, amount, area);
        }


        // set flow on tiles that point towards a diffrent world area (World area connecting tiles)
        public void AddAreaTilesToFlowFieldPath(Dictionary<IntVector2, List<Tile>> areaConnectionTiles)
        {
            WorldArea current = null;
            WorldArea previous = null;
            Tile lowestCostNode;
            IntVector2 flowKey;
            List<Tile> value;
            bool manuallySet = false;

            foreach (IntVector2 key in areaConnectionTiles.Keys)
            {
                current = worldData.worldAreas[key.x];
                previous = worldData.worldAreas[key.y];
                value = areaConnectionTiles[key];

                for (int i = 0; i < value.Count; i += 2)
                {
                    flowKey = new IntVector2(key.x, value[i].sectorIndex);
                    manuallySet = false;

                    //previous curr
                    if (worldData.worldCreatedManually)
                    {
                        IntVector2 manualKey = new IntVector2(key.x, key.y);
                        if (worldData.manualWorldAreaConnections.ContainsKey(manualKey))
                        {
                            manuallySet = true;
                            flowFieldPaths[flowFieldPaths.Count - 1].flowField.field[flowKey][value[i].indexWithinSector] = worldData.manualWorldAreaConnections[manualKey];
                        }
                    }

                    if (!manuallySet)
                    {
                        lowestCostNode = value[i + 1];
                        flowFieldPaths[flowFieldPaths.Count - 1].flowField.field[flowKey][value[i].indexWithinSector] = VectorToDir(new Vector2((lowestCostNode.gridPos.x + previous.leftOffset) - (value[i].gridPos.x + current.leftOffset), (value[i].gridPos.y + current.topOffset) - (lowestCostNode.gridPos.y + previous.topOffset)).normalized);// GetDirBetweenVectors();
                            
                            
                            //new Vector2((lowestCostNode.gridPos.x + previous.leftOffset) - (value[i].gridPos.x + current.leftOffset), (value[i].gridPos.y + current.topOffset) - (lowestCostNode.gridPos.y + previous.topOffset)).normalized;
                    }
                }
            }
        }


        public FlowField CreateFlowField(List<int> sectors, WorldArea area)
        {
            FlowField flowField = new FlowField();
            flowField.AddFields(sectors, worldData.multiLevelSectorManager.GetSectorWidthAtLevel(area, 0) * worldData.multiLevelSectorManager.GetSectorHeightAtLevel(area, 0), area);//.amountOfTilesPerSector);

            return flowField;
        }

        public Vector2 DirToVector(int dir)
        {
            return flowVectors[dir];
        }

        public int VectorToDir(Vector2 vec)
        {
            if(vec.x == 0 && vec.y ==0)
            {
                return 0;
            }
            else
            {
                if (vec.x == 0)
                {
                    if (vec.y < 0)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    if (vec.x > 0)
                    {
                        if (vec.y < 0)
                        {
                            return 5;
                        }
                        else
                        {
                            if (vec.y > 0)
                            {
                                return 4;
                            }
                            else
                            {
                                return 3;
                            }
                        }
                    }
                    else
                    {
                        if (vec.x < 0)
                        {
                            if (vec.y < 0)
                            {
                                return 8;
                            }
                            else
                            {
                                if (vec.y > 0)
                                {
                                    return 7;
                                }
                                else
                                {
                                    return 6;
                                }
                            }
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }

        public int GetDirBetweenVectors(IntVector2 start, IntVector2 end)
        {
            if (start == end)
            {
                return 0;
            }
            else
            {
                if (start.x == end.x)
                {
                    if (end.y > start.y)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    if (end.x > start.x)
                    {
                        if (end.y > start.y)
                        {
                            return 5;
                        }
                        else
                        {
                            if (end.y < start.y)
                            {
                                return 4;
                            }
                            else
                            {
                                return 3;
                            }
                        }
                    }
                    else
                    {
                        if (end.x < start.x)
                        {
                            if (end.y > start.y)
                            {
                                return 8;
                            }
                            else
                            {
                                if (end.y < start.y)
                                {
                                    return 7;
                                }
                                else
                                {
                                    return 6;
                                }
                            }
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }

    }
}
