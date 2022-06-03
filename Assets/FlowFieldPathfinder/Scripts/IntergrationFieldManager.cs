using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class IntergrationFieldManager
    {
        public WorldData worldData;

        List<Tile> closedSet = new List<Tile>();
        List<Tile> closedSetFinish = new List<Tile>();
        List<Tile> openSet = new List<Tile>();
        List<Tile> tilesSearchList = new List<Tile>();


        public void StartIntegrationFieldCreation(Tile destinationTile, List<List<int>> _areasAndSectors, FlowFieldPath _flowPath, SearchPathJob pathJob, int key, bool pathEdit)
        {
            FlowFieldPath flowPath = null;
            bool aPathIsCreated = false;
            List<List<int>> areasAndSectors = _areasAndSectors;

            int index = 0;
            int startIndex = 0;
            List<int> areas = new List<int>();
            List<List<int>> sectors = new List<List<int>>();

            Dictionary<IntVector2, List<Tile>> areaConnectionTiles = new Dictionary<IntVector2, List<Tile>>();

            bool firstWorldArea = true;
            if (pathEdit)
                firstWorldArea = false;

            bool firstWorldAreaOfNewSearch = true;


            for (int a = 0; a < areasAndSectors.Count; a++) // each seperate search //areasAndSectors.Count
            {
                firstWorldAreaOfNewSearch = true;

                index = 0;
                areas.Clear();
                sectors.Clear();
                Dictionary<IntVector2, int[]> alreadyFilledInSector = null;
                IntVector2 areaSectorKey = new IntVector2(0, 0);//area.index, sectorIndex);

               
                if (pathEdit && _flowPath != null)
                    flowPath = _flowPath;

                //setup/starting point of the sectors- and areas-  lists
                if (a == 0 && !pathEdit)// first search
                {
                    startIndex = 0;
                    areas.Add(areasAndSectors[a][0]);
                    sectors.Add(new List<int>());
                }
                else // we start with a search that is not the firts one. we might be able to skip alot of already integrated sectors
                {
                    if (flowPath == null)
                        flowPath = worldData.flowFieldManager.flowFieldPaths[worldData.flowFieldManager.flowFieldPaths.Count - 1];

                    alreadyFilledInSector = flowPath.intergrationField.field;
                    startIndex = -1;

                    for (int i = 0; i < areasAndSectors[a].Count; i += 2)
                    {
                        areaSectorKey.x = areasAndSectors[a][i];
                        areaSectorKey.y = areasAndSectors[a][i + 1];

                        if (!alreadyFilledInSector.ContainsKey(areaSectorKey)) // sector not yet filled in
                        {
                            startIndex = i;
                            areas.Add(areasAndSectors[a][startIndex]);
                            sectors.Add(new List<int>());
                            break;
                        }
                    }
                }



                // if -1 we can skip it all
                if (startIndex != -1) // else entire path already calculated by a diffrent search
                {
                    // set what tiles to cross over during a search

                    // seperate areas and sectors in arrays
                    for (int i = startIndex; i < areasAndSectors[a].Count; i += 2)
                    {
                        areaSectorKey.x = areasAndSectors[a][i];
                        areaSectorKey.y = areasAndSectors[a][i + 1];

                        if (areasAndSectors[a][i] == areas[index])
                            sectors[index].Add(areasAndSectors[a][i + 1]);
                        else
                        {
                            index++;
                            areas.Add(areasAndSectors[a][i]);
                            sectors.Add(new List<int>());
                            sectors[index].Add(areasAndSectors[a][i + 1]);
                        }

                        if (alreadyFilledInSector != null && alreadyFilledInSector.ContainsKey(areaSectorKey)) // added sector already filled in
                        {
                            // a couple of sectors where not already found, then they were, then they arent again
                            // we split up this search, so that every search in the folowing steps is a list of sectors that all directly connect.
                            // no gaps of alredy filledin sectors
                            areasAndSectors.Add(new List<int>());
                            for (int j = i; j < areasAndSectors[a].Count; j++)
                                areasAndSectors[areasAndSectors.Count - 1].Add(areasAndSectors[a][j]);

                            break;
                        }
                    }



                    if (!firstWorldArea && firstWorldAreaOfNewSearch && areasAndSectors[a][startIndex] != areasAndSectors[a][startIndex - 2]) // diffrent world areas
                        firstWorldAreaOfNewSearch = false;

                    // going through our areas- and sectors- lists
                    for (int i = 0; i < areas.Count; i++)
                    {
                        worldData.multiLevelSectorManager.SetSearchFields(areas[i], sectors[i], true);
                        WorldArea currentWorldArea = worldData.worldAreas[areas[i]];

                        if (firstWorldAreaOfNewSearch)
                        {
                            openSet.Clear();

                            List<Tile> oldSectorTiles = new List<Tile>();

                            if (firstWorldArea && !pathEdit)
                            {
                                tilesSearchList.Add(destinationTile);
                                tilesSearchList[0].integrationValue = 0;
                            }
                            else
                            {
                                WorldArea area = worldData.worldAreas[areasAndSectors[a][startIndex]];
                                MultiLevelSector start = area.sectorGrid[0][areasAndSectors[a][startIndex - 1]];
                                MultiLevelSector next = area.sectorGrid[0][areasAndSectors[a][startIndex + 1]];
                                oldSectorTiles = worldData.multiLevelSectorManager.RowBetweenSectorsWithinWorldArea(start, next, area);

                                if (pathEdit) // put old values back in the old tiles
                                {
                                    IntVector2 oldTileKey = new IntVector2();
                                    foreach (Tile tile in oldSectorTiles)
                                    {
                                        oldTileKey.x = tile.worldAreaIndex;
                                        oldTileKey.y = tile.sectorIndex;
                                        tile.integrationValue = flowPath.intergrationField.field[oldTileKey][tile.indexWithinSector];
                                    }
                                }

                                tilesSearchList.AddRange(oldSectorTiles);
                            }


                            foreach (Tile oldtile in oldSectorTiles)
                                closedSet.Remove(oldtile);
                        }
                        else
                        {
                            WorldArea previousWorldArea;
                            int lastSectorOfPreviousWorldArea;

                            if (i == 0) // previous world area is not in array, removed because of alredy covered
                            {
                                previousWorldArea = worldData.worldAreas[areasAndSectors[a][startIndex - 2]];
                                lastSectorOfPreviousWorldArea = areasAndSectors[a][startIndex - 1];
                            }
                            else
                            {
                                previousWorldArea = worldData.worldAreas[areas[i - 1]];
                                lastSectorOfPreviousWorldArea = sectors[i - 1][sectors[i - 1].Count - 1];
                            }

                            int sectorOfCurrentArea = sectors[i][0];


                            IntVector2 areaConnectionKey = new IntVector2(currentWorldArea.index, previousWorldArea.index);
                            if (!areaConnectionTiles.ContainsKey(areaConnectionKey))
                                areaConnectionTiles.Add(areaConnectionKey, new List<Tile>());

                            List<Tile> tiles = SwitchToNextWorldArea(previousWorldArea, currentWorldArea, lastSectorOfPreviousWorldArea, sectorOfCurrentArea, flowPath);

                            areaConnectionTiles[areaConnectionKey].AddRange(tiles);
                        }

                        WaveExpansionSearchTiles(currentWorldArea);

                        closedSetFinish.AddRange(closedSet);

                        // all intergration fields generated, create flow field
                        if (firstWorldArea)
                        {
                            aPathIsCreated = true;
                            worldData.flowFieldManager.CreateFlowFieldPath(closedSet, sectors[i], areas, destinationTile, pathJob, currentWorldArea, key);
                        }
                        else
                        {
                            worldData.flowFieldManager.AddToFlowFieldPath(closedSet, sectors[i], destinationTile, pathJob, currentWorldArea);

                            if (pathEdit)
                                aPathIsCreated = true;
                        }


                        closedSet.Clear();

                        firstWorldAreaOfNewSearch = false;
                        firstWorldArea = false;

                        worldData.multiLevelSectorManager.SetSearchFields(areas[i], sectors[i], false);
                    }
                }
            }




            if (flowPath == null && worldData.flowFieldManager.flowFieldPaths.Count > 0)
                flowPath = worldData.flowFieldManager.flowFieldPaths[worldData.flowFieldManager.flowFieldPaths.Count - 1];



            if (flowPath != null)
            {
                flowPath.flowField.FillFlowField(closedSetFinish, worldData);
                worldData.flowFieldManager.AddAreaTilesToFlowFieldPath(areaConnectionTiles);
            }


            if (pathJob != null)
            {
                if (aPathIsCreated)
                    pathJob.PathCreated(flowPath, false);
                else
                    pathJob.PathCreated(null, false);
            }
            else
            {
                if (aPathIsCreated)
                    worldData.pathfinder.PathCreated(flowPath, flowPath.key, pathEdit);
                else
                    worldData.pathfinder.PathCreated(null, 0, pathEdit);
            }

            ResetTilesAfterSearch();
        }


        // get the tiles between 2 world area in that allign with the specific sectors
        public List<Tile> SwitchToNextWorldArea(WorldArea previousWorldArea, WorldArea currentWorldArea, int sectorOfPreviousWorldArea, int sectorOfCurrentArea, FlowFieldPath flowPath)
        {
            List<Tile> searchedTilesOnAreaEdges = new List<Tile>();
            // go through each list of positions from the previous world area that allign with its sector  And  border against the current world area
            foreach (List<IntVector2> groupLists in previousWorldArea.groupsInSectors[new IntVector2(currentWorldArea.index, sectorOfPreviousWorldArea)])
            {
                foreach (IntVector2 pos in groupLists)
                {
                    // if tile / position  not blocked off
                    if (!previousWorldArea.tileGrid[pos.x][pos.y].blocked)
                    {
                        // go through each tile its linked with in other areas. usually only 1 but can be a maximum of 3
                        foreach (IntVector3 posOtherArea in previousWorldArea.worldAreaTileConnections[pos])
                        {
                            // if the "other pos" matches with the (current)Area we are looking for, is not blocked, and is in the crrect sector
                            if (posOtherArea.z == currentWorldArea.index && !currentWorldArea.tileGrid[posOtherArea.x][posOtherArea.y].blocked && currentWorldArea.tileGrid[posOtherArea.x][posOtherArea.y].sectorIndex == sectorOfCurrentArea)
                            {
                                int cost = 1;
                                if (flowPath != null)
                                    cost = flowPath.intergrationField.field[new IntVector2(previousWorldArea.index, previousWorldArea.tileGrid[pos.x][pos.y].sectorIndex)][previousWorldArea.tileGrid[pos.x][pos.y].indexWithinSector];
                                else
                                    cost = previousWorldArea.tileGrid[pos.x][pos.y].integrationValue;

                                // we "jump" from tile to tile, give it the proper integration value
                                currentWorldArea.tileGrid[posOtherArea.x][posOtherArea.y].integrationValue = cost + currentWorldArea.tileGrid[posOtherArea.x][posOtherArea.y].cost; //previousWorldArea.tileGrid[pos.x][pos.y].cost;

                                tilesSearchList.Add(currentWorldArea.tileGrid[posOtherArea.x][posOtherArea.y]);

                                // store tiles on area edges, for special flow field pointing
                                searchedTilesOnAreaEdges.Add(currentWorldArea.tileGrid[posOtherArea.x][posOtherArea.y]);
                                searchedTilesOnAreaEdges.Add(previousWorldArea.tileGrid[pos.x][pos.y]);
                            }
                        }
                    }
                }
            }

            return searchedTilesOnAreaEdges;
        }


        // keep expanding finding neighbouring tiles & setting their integration values
        private void WaveExpansionSearchTiles(WorldArea area)
        {
            while (tilesSearchList.Count > 0)
            {
                Tile currentTile = tilesSearchList[0];

                // keep expanding finding neighbouring tiles & setting their integration values
                foreach (Tile neighbour in worldData.tileManager.GetNeighboursExpansionSearch(currentTile, area))
                {
                    if (!tilesSearchList.Contains(neighbour))
                        tilesSearchList.Add(neighbour);
                }

                closedSet.Add(currentTile);
                tilesSearchList.Remove(currentTile);
            }
        }

        // reset values after search
        private void ResetTilesAfterSearch()
        {
            //reset Tile values for future searches
            foreach (Tile Tile in closedSetFinish)
                Tile.integrationValue = worldData.tileManager.tileResetIntegrationValue;

            closedSetFinish.Clear();
            closedSet.Clear();
        }

        public IntergrationField CreateIntergrationField(List<Tile> tiles, List<int> sectors, Tile destination, WorldArea area)
        {
            IntergrationField intergrationField = new IntergrationField();
            intergrationField.AddFields(sectors, worldData.multiLevelSectorManager.GetSectorWidthAtLevel(area, 0) * worldData.multiLevelSectorManager.GetSectorHeightAtLevel(area, 0), tiles, area);
            return intergrationField;
        }

        // create extra field, requested by unit if he accidentally leaves a valid sector
        public void CreateExtraField(WorldArea area, Tile tile, FlowFieldPath flowFieldPath, AddToPathJob pathJob)
        {
            if (tile != null && tile != flowFieldPath.destination)
            {
                List<int> neighbourSectors = new List<int>();

                foreach (int neighbour in worldData.multiLevelSectorManager.GetNeighboursIndexes(area.sectorGrid[0][tile.sectorIndex], area))
                {
                    if (flowFieldPath.intergrationField.field.ContainsKey(new IntVector2(area.index, neighbour)))
                        neighbourSectors.Add(neighbour);
                }

                if (neighbourSectors.Count > 0)
                    CreateFieldToAdd(tile.sectorIndex, neighbourSectors, flowFieldPath, pathJob, area);
                else
                {
                    // there are no valid neighboursectors to guide the flowfield
                    // we must find a new connection to the path from here, and fill the flowfields accordingly
                    //Debug.Log("there are no valid neighboursector");
                    worldData.pathfinder.AddToPath(area, tile, flowFieldPath);
                }
            }
        }

        // extra fields are being created, create & add a field by expanding from neighbouring sectors 
        public void CreateFieldToAdd(int emptySector, List<int> neighbourSectors, FlowFieldPath flowFieldPath, AddToPathJob pathJob, WorldArea area)
        {
            // get all tiles between the sectors
            List<Tile> tilesOnEdge = new List<Tile>();
            List<Tile> tilesChangedInNeigbourSectors = new List<Tile>();

            List<int> allSectors = new List<int> { emptySector };
            allSectors.AddRange(neighbourSectors);

            // set which fields/sectors we can do a tile expasion over
            worldData.multiLevelSectorManager.SetSearchFields(allSectors, area, true);

            // for each neighbour sector in the Path
            foreach (int neighbourSector in neighbourSectors)
            {
                // get the tiles on edge
                tilesOnEdge = worldData.multiLevelSectorManager.RowBetweenSectorsWithinWorldArea(area.sectorGrid[0][neighbourSector], area.sectorGrid[0][emptySector], area);

                //request integration values
                int[] fieldValues = flowFieldPath.intergrationField.field[new IntVector2(area.index, neighbourSector)];
                
                // put the integration back in the tiles
                foreach (Tile tile in worldData.multiLevelSectorManager.GetTilesInSector(area.sectorGrid[0][neighbourSector], area))
                {
                    tile.integrationValue = fieldValues[tile.indexWithinSector];
                    tilesChangedInNeigbourSectors.Add(tile);
                }

                tilesSearchList.AddRange(tilesOnEdge);
            }

            // expand over this and neighbouring sectors, starting at the edges
            WaveExpansionSearchTiles(area);


            // remove tiles that have direct connection to a diffrent World Area, their flow direction must not change
            foreach (int neighbourSector in neighbourSectors)
            {
                foreach (AbstractNode node in area.sectorGrid[0][neighbourSector].worldAreaNodes.Keys)
                    closedSet.Remove(node.tileConnection);
            }

            // add new values to flow field pat
            worldData.flowFieldManager.AddToFlowFieldPath(closedSet, allSectors, flowFieldPath.destination, pathJob, area, flowFieldPath);
            flowFieldPath.flowField.FillFlowField(closedSet, worldData);

            // reset earlier removed tiles
            foreach (int neighbourSector in neighbourSectors)
            {
                foreach (AbstractNode node in area.sectorGrid[0][neighbourSector].worldAreaNodes.Keys)
                    node.tileConnection.integrationValue = worldData.tileManager.tileResetIntegrationValue;
            }


            closedSet.AddRange(tilesChangedInNeigbourSectors);
            closedSetFinish.AddRange(closedSet);

            ResetTilesAfterSearch();

            // set search fields back to  false
            worldData.multiLevelSectorManager.SetSearchFields(allSectors, area, false);
        }


    }
}








