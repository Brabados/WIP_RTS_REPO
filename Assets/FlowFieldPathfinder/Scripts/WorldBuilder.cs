using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class WorldBuilder
    {
        public bool worldIsBuilt = false;
        private WorldData worldData;

        public int gridWidth;
        public int gridLength;

        public List<Vector3> visualizeTiles = new List<Vector3>();
        public List<TemporaryWorldArea> temporaryWorldLayers = new List<TemporaryWorldArea>();


        public void GenerateWorld(WorldData _worldData, bool GenerateWhileInPlayMode, bool loadCostField)
        {
            worldData = _worldData;
            temporaryWorldLayers.Clear();
            visualizeTiles.Clear();

            if (worldData.pathfinder.worldIsMultiLayered)
            {
                GenerateWorldAreaIndexLayers();
                GenerateTemporaryWorldLayered();
                GenerateWorldAreas();
                ///------------------------------
                
                // load in saved cost field values 
                bool costFieldsDontMatchWorld = false;
                if (GenerateWhileInPlayMode && loadCostField && worldData.costFields.Count > 0)
                {
                    foreach (WorldArea area in worldData.worldAreas)
                    {
                        if (area.index < worldData.costFields.Count)
                        {
                            for (int x = 0; x < area.gridWidth; x++)
                            {
                                for (int y = 0; y < area.gridLength; y++)
                                {
                                    if (x < worldData.costFields[area.index].Length && y < worldData.costFields[area.index][x].Length)
                                    {
                                        if (area.tileGrid[x][y] != null)
                                            area.tileGrid[x][y].cost += worldData.costFields[area.index][x][y];
                                    }
                                    else
                                    {
                                        costFieldsDontMatchWorld = true;
                                        break;
                                    }
                                }
                                if (costFieldsDontMatchWorld)
                                    break;
                            }
                            if (costFieldsDontMatchWorld)
                                break;
                        }
                        else
                            costFieldsDontMatchWorld = true;
                    }
                    worldData.costFields.Clear();
                }

                if (costFieldsDontMatchWorld)
                    Debug.Log("<color=red> WARNING: Saved costfields for this scene, do NOT match up with the world geometry.  \n create new costFields or reset the geometry to match the existing one. </color>");

                ////---------------
                CreateSectorGraph();
                ConnectWorldAreas(false);
                CombineWorldAreaConnectionsWithSectorGraph();
            }
            else
            {
                DefineFlatWorld();

                // load in saved cost field values 
                bool costFieldsDontMatchWorld = false;
                if (GenerateWhileInPlayMode && loadCostField && worldData.costFields.Count > 0)
                {
                    foreach (WorldArea area in worldData.worldAreas)
                    {
                        for (int x = 0; x < area.gridWidth; x++)
                        {
                            for (int y = 0; y < area.gridLength; y++)
                            {
                                if (x < worldData.costFields[area.index].Length && y < worldData.costFields[area.index][x].Length)
                                {
                                    if (area.tileGrid[x][y] != null)
                                        area.tileGrid[x][y].cost += worldData.costFields[area.index][x][y];
                                }
                                else
                                {
                                    costFieldsDontMatchWorld = true;
                                    break;
                                }
                            }

                            if (costFieldsDontMatchWorld)
                                break;
                        }

                        if (costFieldsDontMatchWorld)
                            break;
                    }
                    worldData.costFields.Clear();
                }


                if (costFieldsDontMatchWorld)
                    Debug.Log("<color=red> WARNING: Saved costfields for this scene, do NOT match up with the world geometry.  \n create new costFields or reset the geometry to match the existing one. </color>");



                CreateSectorGraph();
            }

            worldIsBuilt = true;
            VisualizeTiles();
        }

        public void GenerateWorldAreaIndexLayers()
        {
            worldData.layerdWorldAreaIndexes = new List<int[][]>();

            for (float i = 0; i < worldData.pathfinder.worldHeight; i += worldData.pathfinder.characterHeight)
            {
                int[][] worldIndexArray = new int[gridWidth][];
                for (int j = 0; j < gridWidth; j++)
                    worldIndexArray[j] = new int[gridLength];


                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridLength; y++)
                        worldIndexArray[x][y] = -1;
                }

                worldData.layerdWorldAreaIndexes.Add(worldIndexArray);
            }
        }



        public void GenerateTemporaryWorldLayered()
        {
            // we have temporary world areas that later on will be better refined
            temporaryWorldLayers.Clear();
            for (float i = 0; i < worldData.pathfinder.worldHeight; i += worldData.pathfinder.characterHeight)
            {
                TemporaryWorldArea temporaryWorldLayer = new TemporaryWorldArea();
                temporaryWorldLayer.Setup(worldData);
                temporaryWorldLayers.Add(temporaryWorldLayer);
            }

            Vector3 startingPoint = worldData.pathfinder.worldStart;

            Vector3 rayStartingPoint = new Vector3(0, worldData.pathfinder.worldStart.y + worldData.pathfinder.worldHeight, 0);
            float tileOffset = (worldData.pathfinder.tileSize * 0.5f);
            float distanceCovered = 0;

            int layerMask = (1 << worldData.pathfinder.groundLayer) | (1 << worldData.pathfinder.obstacleLayer);
            // find tiles and store them in temporary worldLayers; 
            for (int x = 0; x < temporaryWorldLayers[0].gridWidth; x++)
            {
                for (int y = 0; y < temporaryWorldLayers[0].gridHeight; y++)
                {
                    rayStartingPoint.x = startingPoint.x + (x * worldData.pathfinder.tileSize) + tileOffset;
                    rayStartingPoint.z = startingPoint.z - (y * worldData.pathfinder.tileSize) - tileOffset;

                    RaycastHit hit;
                    distanceCovered = 0;

                    float blockedLowestY = worldData.pathfinder.worldStart.y + worldData.pathfinder.worldHeight;
                    while (distanceCovered < worldData.pathfinder.worldHeight)
                    {
                        if (Physics.Raycast(rayStartingPoint - new Vector3(0, distanceCovered, 0), Vector3.down, out hit, worldData.pathfinder.worldHeight - distanceCovered, layerMask)) // (worldHeight - distanceCovered) + 0.2f
                        {
                            if (hit.transform.gameObject.layer == worldData.pathfinder.groundLayer)
                            {
                                distanceCovered += hit.distance + worldData.pathfinder.characterHeight;


                                int yLayer = worldData.tileManager.GetHeightLayer(hit.point.y);
                                if (yLayer < 0 || yLayer > temporaryWorldLayers.Count - 1)
                                {
                                    Debug.Log("<color=red> WARNING: piece of world geometry sticks out of the bounding box.</color>");
                                }
                                else
                                {
                                    temporaryWorldLayers[yLayer].tileGrid[x][y] = new Tile();
                                    temporaryWorldLayers[yLayer].tileGrid[x][y].gridPos = new IntVector2(x, y);

                                    temporaryWorldLayers[yLayer].tileGrid[x][y].yWorldPos = hit.point.y;
                                    temporaryWorldLayers[yLayer].tileGrid[x][y].angle = Vector3.Angle(hit.normal, Vector3.up);

                                    if (hit.point.y > blockedLowestY)
                                    {
                                        temporaryWorldLayers[yLayer].tileGrid[x][y].blocked = true;
                                        temporaryWorldLayers[yLayer].tileGrid[x][y].cost += worldData.tileManager.tileBlockedValue;
                                    }

                                    temporaryWorldLayers[yLayer].tileSlotTakenGrid[x][y] = true;
                                }
                            }
                            else
                            {
                                blockedLowestY = hit.point.y - hit.collider.bounds.size.y;
                                distanceCovered += hit.distance + 0.05f;
                            }
                        }
                        else
                            break;
                    }

                }
            }

            // making sure tiles that stickout, beyond the edge of their area get removed, if there is nothing there
            int[] straightDirections = new int[] { 0, 1, 0, -1, 1, 0, -1, 0 };
            int checkX;
            int checkY;
            foreach (TemporaryWorldArea area in temporaryWorldLayers)
            {
                for (int x = 0; x < area.gridWidth; x++)
                {
                    for (int y = 0; y < area.gridHeight; y++)
                    {
                        if (area.tileSlotTakenGrid[x][y])
                        {
                            for (int j = 0; j < straightDirections.Length; j += 2)
                            {
                                checkX = x + straightDirections[j];
                                checkY = y + straightDirections[j + 1];

                                //if position NOT within area or null or tiles canNOT connect (within range)
                                if (checkX < 0 || checkX > area.gridWidth - 1 || checkY < 0 || checkY > area.gridHeight - 1 || !area.tileSlotTakenGrid[checkX][checkY] || !worldData.tileManager.TilesWithinRangeGeneration(area.tileGrid[x][y], area.tileGrid[checkX][checkY])) // null neighbour
                                {
                                    if (CheckIfTileOverEdge(area, new Vector3(startingPoint.x + (x * worldData.pathfinder.tileSize) + tileOffset, area.tileGrid[x][y].yWorldPos, startingPoint.z - (y * worldData.pathfinder.tileSize) - tileOffset), straightDirections[j], straightDirections[j + 1]))
                                    {
                                        area.tileGrid[x][y] = null;
                                        area.tileSlotTakenGrid[x][y] = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CheckIfTileOverEdge(TemporaryWorldArea area, Vector3 rayStartingPoint, int x, int y)
        {
            bool overEdge = true;

            if (Physics.Raycast(rayStartingPoint + new Vector3(x * 0.5f * worldData.pathfinder.tileSize, worldData.pathfinder.generationClimbHeight * 0.5f, -y * 0.5f * worldData.pathfinder.tileSize), Vector3.down, worldData.pathfinder.generationClimbHeight + 0.01f, 1 << worldData.pathfinder.groundLayer)) // (worldHeight - distanceCovered) + 0.2f
                overEdge = false;


            return overEdge;
        }




        public void GenerateWorldAreas()
        {
            worldData.worldAreas.Clear();

            // flat areas are defined
            for (int i = 0; i < temporaryWorldLayers.Count; i++)// temporaryWorldLayers.Count 
                DefineFlatWorldAreas(temporaryWorldLayers[i], i);


            // define sloped areas
            for (int i = 0; i < temporaryWorldLayers.Count; i++)// temporaryWorldLayers.Count 
                DefineSlopedWorldAreas(temporaryWorldLayers[i], i);
        }

        private void GenerateFlatWorldArea(Tile[][] tileGrid, IntVector2 tileGridOffset, int yLayer)
        {
            WorldArea area = new WorldArea();
            area.index = worldData.worldAreas.Count;
            area.FlatAreaCopyTiles(tileGridOffset, tileGrid, yLayer, worldData);
            worldData.worldAreas.Add(area);
        }

        private void DefineFlatWorldAreas(TemporaryWorldArea tempArea, int yLayer)
        {
            for (int x = 0; x < tempArea.gridWidth; x++)
            {
                for (int y = 0; y < tempArea.gridHeight; y++)
                {
                    // tile found, get the area its connected with
                    if (tempArea.tileSlotTakenGrid[x][y] && tempArea.tileGrid[x][y].angle == 0)
                    {
                        int left = x;
                        int right = x;
                        int top = y;
                        int bot = y;

                        List<IntVector2> closedList = new List<IntVector2>();
                        List<IntVector2> openList = new List<IntVector2>();
                        openList.Add(new IntVector2(x, y));
                        IntVector2 current = openList[0];

                        int directionValue;

                        while (openList.Count > 0)
                        {
                            current = openList[0];

                            directionValue = current.x + 1;
                            if (directionValue < tempArea.gridWidth && tempArea.tileSlotTakenGrid[directionValue][current.y] && tempArea.tileGrid[directionValue][current.y].angle == 0) // right
                            {
                                tempArea.tileSlotTakenGrid[directionValue][current.y] = false;
                                worldData.layerdWorldAreaIndexes[yLayer][directionValue][current.y] = worldData.worldAreas.Count; // set world Index

                                if (directionValue > right)
                                    right = current.x + 1;

                                openList.Add(new IntVector2(directionValue, current.y));
                            }

                            directionValue = current.x - 1;
                            if (directionValue > -1 && tempArea.tileSlotTakenGrid[directionValue][current.y] && tempArea.tileGrid[directionValue][current.y].angle == 0) // left
                            {
                                tempArea.tileSlotTakenGrid[directionValue][current.y] = false;
                                worldData.layerdWorldAreaIndexes[yLayer][directionValue][current.y] = worldData.worldAreas.Count; // set world Index

                                if (directionValue < left)
                                    left = directionValue;
                                openList.Add(new IntVector2(directionValue, current.y));
                            }

                            directionValue = current.y - 1;
                            if (directionValue > -1 && tempArea.tileSlotTakenGrid[current.x][directionValue] && tempArea.tileGrid[current.x][directionValue].angle == 0) // up
                            {
                                tempArea.tileSlotTakenGrid[current.x][directionValue] = false;
                                worldData.layerdWorldAreaIndexes[yLayer][current.x][directionValue] = worldData.worldAreas.Count; // set world Index

                                if (directionValue < top)
                                    top = directionValue;
                                openList.Add(new IntVector2(current.x, directionValue));
                            }

                            directionValue = current.y + 1;
                            if (directionValue < tempArea.gridHeight && tempArea.tileSlotTakenGrid[current.x][directionValue] && tempArea.tileGrid[current.x][directionValue].angle == 0) // down
                            {
                                tempArea.tileSlotTakenGrid[current.x][directionValue] = false;
                                worldData.layerdWorldAreaIndexes[yLayer][current.x][directionValue] = worldData.worldAreas.Count; // set world Index

                                if (directionValue > bot)
                                    bot = current.y + 1;
                                openList.Add(new IntVector2(current.x, directionValue));
                            }

                            closedList.Add(current);
                            openList.Remove(current);
                        }

                        if (closedList.Count == 1 && tempArea.tileGrid[closedList[0].x][closedList[0].y].angle != 0)
                        {

                        }
                        else
                        {
                            // open list is empty, define world area
                            WorldArea area = new WorldArea();
                            area.index = worldData.worldAreas.Count;
                            area.FlatAreaCopyTiles(left, right, top, bot, closedList, tempArea.tileGrid, yLayer, worldData);
                            worldData.worldAreas.Add(area);
                        }
                    }
                }
            }
        }

        private void DefineSlopedWorldAreas(TemporaryWorldArea tempArea, int yLayer)
        {
            for (int x = 0; x < tempArea.gridWidth; x++)
            {
                for (int y = 0; y < tempArea.gridHeight; y++)
                {
                    // tile found, get the area its connected with
                    if (tempArea.tileSlotTakenGrid[x][y])
                    {
                        tempArea.tileSlotTakenGrid[x][y] = false;
                        worldData.layerdWorldAreaIndexes[yLayer][x][y] = worldData.worldAreas.Count; // set world Index
                        // open list is empty, define world area
                        WorldArea area = DefineSlopedWorldArea(x, y, tempArea.tileGrid[x][y].angle, yLayer);
                        if (area != null)
                            worldData.worldAreas.Add(area);
                    }
                }
            }
        }




        public WorldArea DefineSlopedWorldArea(int x, int y, float angle, int yLayer)
        {
            //Debug.Log("angle  " + angle);
            float angleLeanWay = 0.1f;
            WorldArea area = null;
            int left = x;
            int right = x;
            int top = y;
            int bot = y;

            List<List<IntVector2>> closedList = new List<List<IntVector2>>();
            List<IntVector2> openList = new List<IntVector2>();
            openList.Add(new IntVector2(x, y));
            IntVector2 current = openList[0];

            int directionValue;

            for (int i = yLayer; i < temporaryWorldLayers.Count; i++)
            {
                List<IntVector2> closedListTemp = new List<IntVector2>();
                TemporaryWorldArea tempArea = temporaryWorldLayers[i];

                if (i != yLayer) // find all tiles around previous defined rectangle of tiles, so we can start of these tiles for our while loop search below
                {
                    int tempTop = top;
                    int tempBot = bot;
                    int tempLeft = left;
                    int tempRight = right;

                    // top row
                    directionValue = top - 1;
                    if (directionValue > -1)
                    {
                        for (int X = left; X < right + 1; X++)
                        {
                            if (tempArea.tileSlotTakenGrid[X][directionValue] && tempArea.tileGrid[X][directionValue].angle > angle - angleLeanWay && tempArea.tileGrid[X][directionValue].angle < angle + angleLeanWay)
                            {
                                tempTop = directionValue;

                                tempArea.tileSlotTakenGrid[X][directionValue] = false;
                                worldData.layerdWorldAreaIndexes[i][X][directionValue] = worldData.worldAreas.Count; //layerdWorldAreaIndexes[yLayer][X][directionValue] = worldAreas.Count; // set world Index
                                openList.Add(new IntVector2(X, directionValue));
                            }
                        }
                    }

                    // bot row
                    directionValue = bot + 1;
                    if (directionValue < tempArea.gridHeight)
                    {
                        for (int X = left; X < right + 1; X++)
                        {
                            if (tempArea.tileSlotTakenGrid[X][directionValue] && tempArea.tileGrid[X][directionValue].angle > angle - angleLeanWay && tempArea.tileGrid[X][directionValue].angle < angle + angleLeanWay)
                            {
                                tempBot = directionValue;

                                tempArea.tileSlotTakenGrid[X][directionValue] = false;
                                worldData.layerdWorldAreaIndexes[i][X][directionValue] = worldData.worldAreas.Count; //layerdWorldAreaIndexes[yLayer][X][directionValue] = worldAreas.Count; // set world Index
                                openList.Add(new IntVector2(X, directionValue));
                            }
                        }
                    }

                    // left row
                    directionValue = left - 1;
                    if (directionValue > -1)
                    {
                        for (int Y = top; Y < bot + 1; Y++)
                        {
                            if (tempArea.tileSlotTakenGrid[directionValue][Y] && tempArea.tileGrid[directionValue][Y].angle > angle - angleLeanWay && tempArea.tileGrid[directionValue][Y].angle < angle + angleLeanWay)
                            {
                                tempLeft = directionValue;

                                tempArea.tileSlotTakenGrid[directionValue][Y] = false;
                                worldData.layerdWorldAreaIndexes[i][directionValue][Y] = worldData.worldAreas.Count;//layerdWorldAreaIndexes[yLayer][directionValue][Y] = worldAreas.Count; // set world Index
                                openList.Add(new IntVector2(directionValue, Y));
                            }
                        }
                    }

                    // right row
                    directionValue = right + 1;
                    if (directionValue < tempArea.gridWidth)
                    {
                        for (int Y = top; Y < bot + 1; Y++)
                        {
                            if (tempArea.tileSlotTakenGrid[directionValue][Y] && tempArea.tileGrid[directionValue][Y].angle > angle - angleLeanWay && tempArea.tileGrid[directionValue][Y].angle < angle + angleLeanWay)
                            {
                                tempRight = directionValue;

                                tempArea.tileSlotTakenGrid[directionValue][Y] = false;
                                worldData.layerdWorldAreaIndexes[i][directionValue][Y] = worldData.worldAreas.Count;//layerdWorldAreaIndexes[yLayer][directionValue][Y] = worldAreas.Count; // set world Index
                                openList.Add(new IntVector2(directionValue, Y));
                            }
                        }
                    }


                    right = tempRight;
                    left = tempLeft;
                    bot = tempBot;
                    top = tempTop;
                }



                if (openList.Count == 0) // nothing left to find
                {
                    area = new WorldArea();
                    area.index = worldData.worldAreas.Count;
                    area.SlopedAreaCopyTiles(angle, left, right, top, bot, closedList, temporaryWorldLayers, yLayer, worldData);

                    //SubdivideSlopedWorldArea(area);
                    return area;
                }


                //search for tiles with matching angle in temporary world layer
                while (openList.Count > 0)
                {
                    current = openList[0];

                    directionValue = current.x + 1;
                    if (directionValue < tempArea.gridWidth && tempArea.tileSlotTakenGrid[directionValue][current.y] && tempArea.tileGrid[directionValue][current.y].angle > angle - angleLeanWay && tempArea.tileGrid[directionValue][current.y].angle < angle + angleLeanWay) // right
                    {
                        if (directionValue > right)
                            right = directionValue;

                        tempArea.tileSlotTakenGrid[directionValue][current.y] = false;
                        worldData.layerdWorldAreaIndexes[i][directionValue][current.y] = worldData.worldAreas.Count; // set world Index

                        openList.Add(new IntVector2(directionValue, current.y));
                    }

                    directionValue = current.x - 1;
                    if (directionValue > -1 && tempArea.tileSlotTakenGrid[directionValue][current.y] && tempArea.tileGrid[directionValue][current.y].angle > angle - angleLeanWay && tempArea.tileGrid[directionValue][current.y].angle < angle + angleLeanWay) // left
                    {
                        tempArea.tileSlotTakenGrid[directionValue][current.y] = false;
                        worldData.layerdWorldAreaIndexes[i][directionValue][current.y] = worldData.worldAreas.Count; // set world Index

                        if (directionValue < left)
                            left = directionValue;
                        openList.Add(new IntVector2(directionValue, current.y));
                    }

                    directionValue = current.y - 1;
                    if (directionValue > -1 && tempArea.tileSlotTakenGrid[current.x][directionValue] && tempArea.tileGrid[current.x][directionValue].angle > angle - angleLeanWay && tempArea.tileGrid[current.x][directionValue].angle < angle + angleLeanWay) // up
                    {
                        tempArea.tileSlotTakenGrid[current.x][directionValue] = false;
                        worldData.layerdWorldAreaIndexes[i][current.x][directionValue] = worldData.worldAreas.Count; // set world Index

                        if (directionValue < top)
                            top = directionValue;
                        openList.Add(new IntVector2(current.x, directionValue));
                    }

                    directionValue = current.y + 1;
                    if (directionValue < tempArea.gridHeight && tempArea.tileSlotTakenGrid[current.x][directionValue] && tempArea.tileGrid[current.x][directionValue].angle > angle - angleLeanWay && tempArea.tileGrid[current.x][directionValue].angle < angle + angleLeanWay) // down
                    {
                        tempArea.tileSlotTakenGrid[current.x][directionValue] = false;
                        worldData.layerdWorldAreaIndexes[i][current.x][directionValue] = worldData.worldAreas.Count; // set world Index

                        if (directionValue > bot)
                            bot = directionValue;
                        openList.Add(new IntVector2(current.x, directionValue));
                    }

                    closedListTemp.Add(current);
                    openList.Remove(current);
                }

                closedList.Add(closedListTemp);
            }



            return area;
        }



        public void CreateSectorGraph()
        {
            for (int i = 0; i < worldData.worldAreas.Count; i++)
            {
                worldData.multiLevelSectorManager.SetupSectorConnections(worldData.worldAreas[i]);
            }
        }



        private void ConnectWorldAreas(bool manuallySet)
        {
            WorldArea area;
            for (int i = 0; i < worldData.worldAreas.Count; i++)
            { 
                area =  worldData.worldAreas[i];
                //inner tiles check
                for (int x = 0; x < area.gridWidth; x++)
                {
                    for (int y = 0; y < area.gridLength; y++)
                    {
                        // null tile space, there might be an other tile of other world area here
                        if (area.tileGrid[x][y] == null)
                        {
                            // does this space connect to a tile?
                            foreach (Tile neighbour in worldData.tileManager.GetStraightNeighbours(x, y, area))
                                FindWorldAreaConnection(x, y, neighbour, area, manuallySet);
                        }
                    }
                }

                // perimeter check
                //left
                if (area.leftOffset - 1 > -1) // dont leave world
                {
                    for (int y = 0; y < area.gridLength; y++)
                    {
                        if (area.tileGrid[0][y] != null)
                        {
                            FindWorldAreaConnection(-1, y, area.tileGrid[0][y], area, manuallySet);
                        }
                    }
                }


                //right
                if (area.leftOffset + area.gridWidth < gridWidth) // dont leave world
                {
                    for (int y = 0; y < area.gridLength; y++)
                    {
                        if (area.tileGrid[area.gridWidth - 1][y] != null)
                        {
                            FindWorldAreaConnection(area.gridWidth, y, area.tileGrid[area.gridWidth - 1][y], area, manuallySet);
                        }
                    }
                }

                //top
                if (area.topOffset - 1 > -1) // dont leave world
                {
                    for (int x = 0; x < area.gridWidth; x++)
                    {
                        if (area.tileGrid[x][0] != null)
                        {
                            FindWorldAreaConnection(x, -1, area.tileGrid[x][0], area, manuallySet);
                        }
                    }
                }

                //bot
                if (area.topOffset + area.gridLength < gridLength) // dont leave world
                {
                    for (int x = 0; x < area.gridWidth; x++)
                    {
                        if (area.tileGrid[x][area.gridLength - 1] != null)
                        {
                            FindWorldAreaConnection(x, area.gridLength, area.tileGrid[x][area.gridLength - 1], area, manuallySet);
                        }
                    }
                }
            }
        }

        private void FindWorldAreaConnection(int x, int y, Tile neighbour, WorldArea area, bool forceConnection)
        {
            //Debug.Log("area  " + area.index);
            WorldArea area2 = worldData.tileManager.GetWorldAreaAtGuaranteedPosition(x + area.leftOffset, neighbour.yWorldPos, y + area.topOffset);
            if (area2 != null)
                ConnectWorldAreaTiles(neighbour, area, x + area.leftOffset, y + area.topOffset, area2, forceConnection);
        }

        private void ConnectWorldAreaTiles(Tile tile1, WorldArea area1, int worldX, int worldY, WorldArea area2, bool forceConnection)
        {
            //Debug.Log("connect world area tiles   worldX " + worldX + "  worldY  " + worldY + "    area1 " + area1.index);

            Tile tile2 = area2.tileGrid[worldX - area2.leftOffset][worldY - area2.topOffset];
            if (worldData.tileManager.TilesWithinRangeGeneration(tile1, tile2)) // are the tiles within reach of eachother?
            {
                // positions of each tile in their own area
                IntVector2 tile1Vector2 = tile1.gridPos;//.x, tile1.gridPos.y);
                IntVector2 tile2Vector2 = tile2.gridPos;//.x, tile2.gridPos.y);
                bool goOn = false;


                if (forceConnection) // we want to connect these 2   2D areas, but the system normally never has to connect flat areas next to eachother, as they would be one and the same
                {
                    goOn = true;
                }
                else
                {
                    if (!area1.flatArea)
                    {
                        if (area1.angleDirectionX)
                        {
                            if (tile1.gridPos.y + area1.topOffset == tile2.gridPos.y + area2.topOffset)
                            {
                                goOn = true;
                            }
                        }
                        else
                        {
                            if (tile1.gridPos.x + area1.leftOffset == tile2.gridPos.x + area2.leftOffset)
                            {
                                goOn = true;
                            }
                        }
                    }
                    else if (!area2.flatArea)
                    {
                        if (area2.angleDirectionX)
                        {
                            if (tile1.gridPos.y + area1.topOffset == tile2.gridPos.y + area2.topOffset)
                            {
                                goOn = true;
                            }
                        }
                        else
                        {
                            if (tile1.gridPos.x + area1.leftOffset == tile2.gridPos.x + area2.leftOffset)
                            {
                                goOn = true;
                            }
                        }
                    }
                }

                if (goOn)
                {

                    // search from area 1, it has no data on this key/tile yet, so connect them
                    if (!area1.worldAreaTileConnections.ContainsKey(tile1Vector2))
                    {
                        area1.worldAreaTileConnections.Add(tile1Vector2, new List<IntVector3> { new IntVector3(tile2.gridPos.x, tile2.gridPos.y, area2.index) });

                        if (!area2.worldAreaTileConnections.ContainsKey(tile2Vector2))
                            area2.worldAreaTileConnections.Add(tile2Vector2, new List<IntVector3> { new IntVector3(tile1.gridPos.x, tile1.gridPos.y, area1.index) });
                        else
                            area2.worldAreaTileConnections[tile2Vector2].Add(new IntVector3(tile1.gridPos.x, tile1.gridPos.y, area1.index));


                        if (!area1.worldAreasConnectedIndexes.Contains(area2.index))
                            area1.worldAreasConnectedIndexes.Add(area2.index);
                        if (!area2.worldAreasConnectedIndexes.Contains(area1.index))
                            area2.worldAreasConnectedIndexes.Add(area1.index);

                    }
                    else
                    {
                        // we already have data on this key/tile, but is it connected yet to the tile we are examining?
                        IntVector3 vectorToAdd = new IntVector3(tile2.gridPos.x, tile2.gridPos.y, area2.index);
                        bool alreadyConnected = false;
                        foreach (IntVector3 vector3 in area1.worldAreaTileConnections[tile1Vector2])
                        {
                            if (vector3 == vectorToAdd)
                            {
                                alreadyConnected = true;
                                break;
                            }
                        }

                        // they are not connected yet, so connect them
                        if (!alreadyConnected)
                        {
                            area1.worldAreaTileConnections[new IntVector2(tile1.gridPos.x, tile1.gridPos.y)].Add(new IntVector3(tile2.gridPos.x, tile2.gridPos.y, area2.index));

                            if (!area2.worldAreaTileConnections.ContainsKey(tile2Vector2))
                                area2.worldAreaTileConnections.Add(tile2Vector2, new List<IntVector3> { new IntVector3(tile1.gridPos.x, tile1.gridPos.y, area1.index) });
                            else
                                area2.worldAreaTileConnections[tile2Vector2].Add(new IntVector3(tile1.gridPos.x, tile1.gridPos.y, area1.index));


                            if (!area1.worldAreasConnectedIndexes.Contains(area2.index))
                                area1.worldAreasConnectedIndexes.Add(area2.index);
                            if (!area2.worldAreasConnectedIndexes.Contains(area1.index))
                                area2.worldAreasConnectedIndexes.Add(area1.index);
                        }
                    }
                }
            }
        }




        private void CombineWorldAreaConnectionsWithSectorGraph()
        {
            CreateWorldAreaConnectionGroups();

            CreateWorldAreaConnectionNodes();
        }

        private void CreateWorldAreaConnectionGroups()
        {
            int groupNumber = -1;
            List<IntVector2> openList = new List<IntVector2>();
            List<IntVector2> group = new List<IntVector2>();
            IntVector2 current;
            Dictionary<IntVector2, IntVector3> leftOverKeysDictionary = new Dictionary<IntVector2, IntVector3>();
            List<IntVector2> leftOverKeysList = new List<IntVector2>();

            foreach (WorldArea area in worldData.worldAreas)
            {
                groupNumber = -1;
                foreach (int connectedWorldAreaIndex in area.worldAreasConnectedIndexes)
                {
                    // find legit of connections
                    leftOverKeysDictionary.Clear();
                    foreach (IntVector2 key in area.worldAreaTileConnections.Keys)
                    {
                        foreach (IntVector3 vector3 in area.worldAreaTileConnections[key])
                        {
                            if (vector3.z == connectedWorldAreaIndex)
                            {
                                leftOverKeysDictionary.Add(key, vector3);
                                break;
                            }
                        }
                    }

                    ////////////////////////

                    leftOverKeysList = new List<IntVector2>(leftOverKeysDictionary.Keys);
                    Tile currentTile = null;
                    //find rows/groups of connections for each sector
                    foreach (IntVector2 key in leftOverKeysList)
                    {
                        if (leftOverKeysDictionary.ContainsKey(key))// this key is still left over
                        {
                            groupNumber++;
                            group = new List<IntVector2>();
                            openList.Add(key);
                            leftOverKeysDictionary.Remove(key);
                            bool newSearh = true;
                            bool swapInsertion = false;
                            bool swapTo0 = false;

                            while (openList.Count > 0) // find legit neighbours until no longer possible
                            {
                                current = openList[0];
                                currentTile = area.tileGrid[current.x][current.y];

                                int neighboursFound = 0;
                                foreach (Tile neighbour in worldData.tileManager.GetStraightNeighbours(current.x, current.y, area))
                                {
                                    if (neighbour.sectorIndex == currentTile.sectorIndex)
                                    {
                                        if (leftOverKeysDictionary.ContainsKey(neighbour.gridPos))// neighbour is part of the connection
                                        {
                                           
                                            neighboursFound++;
                                            openList.Add(neighbour.gridPos);

                                            leftOverKeysDictionary.Remove(neighbour.gridPos);
                                        }
                                    }
                                }


                                if (swapTo0)
                                    group.Insert(0, current);
                                else
                                    group.Add(current);


                                if (swapInsertion)
                                {
                                    if (neighboursFound == 0)
                                        swapInsertion = false;

                                    swapTo0 = !swapTo0;
                                }

                                //group.Add(current);
                                openList.Remove(current);


                                // order way tiles are added, we only do this at the first search of a new group
                                if (newSearh && openList.Count == 2) // somewhere not at the end of a group
                                    swapInsertion = true;

                                newSearh = false;
                            }

                            IntVector2 groupKey = new IntVector2(connectedWorldAreaIndex, currentTile.sectorIndex);

                            if (!area.groupsInSectors.ContainsKey(groupKey))
                                area.groupsInSectors.Add(groupKey, new List<List<IntVector2>>());

                            area.groupsInSectors[groupKey].Add(group);
                        }
                    }
                }
            }
        }


        public void GenerateWordConnectingNodesPerGroup(WorldArea area, List<IntVector2> group, IntVector2 key)
        {
            bool newItemFound = false;
            int startIndex = 0;
            int size = 0;

            for (int i = 0; i < group.Count; i++)
            {
                newItemFound = false;
                foreach (IntVector3 otherTile in area.worldAreaTileConnections[group[i]])
                {
                    if (otherTile.z == key.x && worldData.worldAreas[otherTile.z].tileGrid[otherTile.x][otherTile.y] != null && !worldData.worldAreas[otherTile.z].tileGrid[otherTile.x][otherTile.y].blocked) // if the tile connected is blocked we ignore this one aswell
                    {
                        if (!area.tileGrid[group[i].x][group[i].y].blocked)
                        {
                            newItemFound = true;
                            size++;
                            break;
                        }
                    }

                }

                if (i == group.Count - 1)
                    newItemFound = false;

                if (!newItemFound) // group closed   if size == 0 we immediatly founf a obstacle on  one of the sides, dont try and make a connection here then
                {
                    if (size != 0)
                    {
                        int middle = startIndex + (size / 2);

                        Tile middleTile = area.tileGrid[group[middle].x][group[middle].y];

                        IntVector3 connectedSpot = FindCorrectAreaNodeConnection(area.worldAreaTileConnections[group[middle]], key.x);
                        Tile tileConnectedWith = worldData.worldAreas[connectedSpot.z].tileGrid[connectedSpot.x][connectedSpot.y];

                        for (int j = 0; j < worldData.pathfinder.maxLevelAmount; j++)
                        {
                            MultiLevelSector sector = area.sectorGrid[0][middleTile.sectorIndex];
                            MultiLevelSector sectorConnectedWith = worldData.worldAreas[connectedSpot.z].sectorGrid[0][tileConnectedWith.sectorIndex];

                            if (j != 0)
                            {
                                sector = worldData.multiLevelSectorManager.GetHigherSectorFromLower(j, sector, area);
                                sectorConnectedWith = worldData.multiLevelSectorManager.GetHigherSectorFromLower(j, sectorConnectedWith, worldData.worldAreas[connectedSpot.z]);
                            }

                            AbstractNode node = worldData.multiLevelSectorManager.CreateAbstractNodeInSector(sector, middleTile, area);
                            AbstractNode nodeConnectedWith = worldData.multiLevelSectorManager.CreateAbstractNodeInSector(sectorConnectedWith, tileConnectedWith, worldData.worldAreas[connectedSpot.z]);

                            sector.worldAreaNodes.Add(node, connectedSpot.z);
                            sectorConnectedWith.worldAreaNodes.Add(nodeConnectedWith, area.index);

                            node.nodeConnectionToOtherSector = nodeConnectedWith;
                            nodeConnectedWith.nodeConnectionToOtherSector = node;

                            worldData.multiLevelSectorManager.ConnectSectorNodes(node, nodeConnectedWith, 1);


                            worldData.multiLevelSectorManager.ConnectWorldAreaNodesToSectorNodes(sector, area, j);
                            worldData.multiLevelSectorManager.ConnectWorldAreaNodesToSectorNodes(sectorConnectedWith, worldData.worldAreas[connectedSpot.z], j);



                            //visual
                            sectorConnectedWith.SearchConnections();
                            sector.SearchConnections();

                        }

                        startIndex = i + 1;
                        size = 0;
                    }
                    else
                    {
                        startIndex = i + 1;
                        size = 0;
                    }
                }
            }
        }

        private void CreateWorldAreaConnectionNodes()
        {
            foreach (WorldArea area in worldData.worldAreas)
            {
                foreach (IntVector2 key in area.groupsInSectors.Keys)
                {
                    if (area.index < key.x) // making sure worldareas connect only 1 time with each other
                    {
                        foreach (List<IntVector2> group in area.groupsInSectors[key])
                        {
                            GenerateWordConnectingNodesPerGroup(area, group, key);
                        }
                    }
                }
            }
        }

        private IntVector3 FindCorrectAreaNodeConnection(List<IntVector3> nodeSpots, int worldAreaIndex)
        {
            foreach (IntVector3 nodeSpot in nodeSpots)
            {
                if (nodeSpot.z == worldAreaIndex && !worldData.worldAreas[worldAreaIndex].tileGrid[nodeSpot.x][nodeSpot.y].blocked)
                    return nodeSpot;
            }

            Debug.Log("shouldnt get here  " + "other pos  " + nodeSpots[0].x + "  " + nodeSpots[0].y + "   other world area  " + worldAreaIndex);
            return new IntVector3(0, 0, 0); // null
        }



        public void VisualizeTiles()
        {
            Vector3 start = new Vector3();
            Vector3 end = new Vector3();
            visualizeTiles.Clear();
            Vector3 offset = new Vector3(-worldData.pathfinder.tileSize * 0.5f, 0.15f, worldData.pathfinder.tileSize * 0.5f);
            Vector3 extraSlopedOffset = new Vector3(0, 0, 0);

            foreach (WorldArea worldArea in worldData.worldAreas)
            {
                extraSlopedOffset = Vector3.zero;
                for (int x = 0; x < worldArea.gridWidth; x++)
                {
                    for (int y = 0; y < worldArea.gridLength; y++)
                    {
                        if (worldArea.tileGrid[x][y] != null)
                        {

                            start = worldData.tileManager.GetTileWorldPosition(worldArea.tileGrid[x][y], worldArea);
                            
                            if (x + 1 < worldArea.gridWidth)
                            {
                                if (worldArea.tileGrid[x + 1][y] == null)
                                    VisualizeTilesXStop(worldArea, worldArea.tileGrid[x][y], start, offset);
                                else
                                {
                                    if (!worldArea.flatArea)
                                    {
                                        if (worldArea.anglePositive < 0)
                                            extraSlopedOffset.y = Mathf.Tan(worldArea.tileGrid[x][y].angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;
                                        else
                                            extraSlopedOffset.y = -Mathf.Tan(worldArea.tileGrid[x][y].angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;
                                    }

                                    end = worldData.tileManager.GetTileWorldPosition(worldArea.tileGrid[x + 1][y], worldArea);
                                }

                                visualizeTiles.Add(start + offset + extraSlopedOffset);
                                visualizeTiles.Add(end + offset + extraSlopedOffset);
                            }
                            else
                                VisualizeTilesXStop(worldArea, worldArea.tileGrid[x][y], start, offset);


                            if (y + 1 < worldArea.gridLength)
                            {
                                if (worldArea.tileGrid[x][y + 1] == null)
                                    VisualizeTilesZStop(worldArea, worldArea.tileGrid[x][y], start, offset);
                                else
                                {
                                    if (!worldArea.flatArea)
                                    {
                                        if (worldArea.anglePositive < 0)
                                            extraSlopedOffset.y = Mathf.Tan(worldArea.tileGrid[x][y].angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;
                                        else
                                            extraSlopedOffset.y = -Mathf.Tan(worldArea.tileGrid[x][y].angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;
                                    }

                                    end = worldData.tileManager.GetTileWorldPosition(worldArea.tileGrid[x][y + 1], worldArea);
                                }

                                visualizeTiles.Add(start + offset + extraSlopedOffset);
                                visualizeTiles.Add(end + offset + extraSlopedOffset);
                            }
                            else
                                VisualizeTilesZStop(worldArea, worldArea.tileGrid[x][y], start, offset);
                        }
                    }
                }
            }
        }

        private void VisualizeTilesXStop(WorldArea area, Tile startTile, Vector3 start, Vector3 offset)
        {
            Vector3 end;
            Vector3 newEnd;
            if (area.flatArea)
            {
                // x continue line
                end = start + new Vector3(worldData.pathfinder.tileSize, 0, 0);

                visualizeTiles.Add(start + offset);
                visualizeTiles.Add(end + offset);

                //// from end to z direction
                newEnd = end;//
                newEnd.z -= worldData.pathfinder.tileSize;

                visualizeTiles.Add(end + offset);
                visualizeTiles.Add(newEnd + offset);
            }
            else
            {

                Vector3 extraSlopedOffset = new Vector3(0, 0, 0);

                if (area.anglePositive < 0)
                    extraSlopedOffset.y = Mathf.Tan(startTile.angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;
                else
                    extraSlopedOffset.y = -Mathf.Tan(startTile.angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;


                if (area.angleDirectionX)
                {
                    end = start;
                    Vector2 values = tileCosSin(area, startTile.angle);

                    if (area.anglePositive == 1)
                    {
                        end.y += values.y;
                        end.x -= values.x;
                    }
                    else
                    {
                        end.y -= values.y;
                        end.x += values.x;
                    }

                    visualizeTiles.Add(start + offset + extraSlopedOffset);
                    visualizeTiles.Add(end + offset + extraSlopedOffset);


                    newEnd = end;
                    newEnd.z -= worldData.pathfinder.tileSize;
                    visualizeTiles.Add(end + offset + extraSlopedOffset);
                    visualizeTiles.Add(newEnd + offset + extraSlopedOffset);
                }
                else
                {
                    end = start + new Vector3(worldData.pathfinder.tileSize, 0, 0);

                    visualizeTiles.Add(start + offset + extraSlopedOffset);
                    visualizeTiles.Add(end + offset + extraSlopedOffset);

                    //// from end to z direction
                    newEnd = end;
                    Vector2 values = tileCosSin(area, startTile.angle);// *area.angledAreaResolutionDiffrence;



                    if (area.anglePositive == -1)
                    {
                        newEnd.y -= values.y;
                        newEnd.z -= values.x;
                    }
                    else
                    {
                        newEnd.y += values.y;
                        newEnd.z += values.x;
                    }

                    visualizeTiles.Add(end + offset + extraSlopedOffset);
                    visualizeTiles.Add(newEnd + offset + extraSlopedOffset);
                }

            }
        }

        private void VisualizeTilesZStop(WorldArea area, Tile startTile, Vector3 start, Vector3 offset)
        {
            Vector3 end;
            Vector3 newEnd;
            if (area.flatArea)
            {
                // x continue line
                end = start + new Vector3(0, 0, -worldData.pathfinder.tileSize);

                visualizeTiles.Add(start + offset);
                visualizeTiles.Add(end + offset);

                //// from end to z direction
                newEnd = end;//
                newEnd.x += worldData.pathfinder.tileSize;

                visualizeTiles.Add(end + offset);
                visualizeTiles.Add(newEnd + offset);
            }
            else
            {
                Vector3 extraSlopedOffset = new Vector3(0, 0, 0);

                if (area.anglePositive < 0)
                    extraSlopedOffset.y = Mathf.Tan(startTile.angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;
                else
                    extraSlopedOffset.y = -Mathf.Tan(startTile.angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * 0.5f;


                if (area.angleDirectionX)
                {
                    end = start + new Vector3(0, 0, -worldData.pathfinder.tileSize);

                    visualizeTiles.Add(start + offset + extraSlopedOffset);
                    visualizeTiles.Add(end + offset + extraSlopedOffset);

                    //// from end to z direction
                    newEnd = end;
                    Vector2 values = tileCosSin(area, startTile.angle);


                    if (area.anglePositive == 1)
                    {
                        newEnd.y += values.y;
                        newEnd.x -= values.x;
                    }
                    else
                    {
                        newEnd.y -= values.y;
                        newEnd.x += values.x;
                    }

                    visualizeTiles.Add(end + offset + extraSlopedOffset);
                    visualizeTiles.Add(newEnd + offset + extraSlopedOffset);

                }
                else
                {
                    end = start;
                    Vector2 values = tileCosSin(area, startTile.angle);

                    if (area.anglePositive == -1)
                    {
                        end.y -= values.y;
                        end.z -= values.x;
                    }
                    else
                    {
                        end.y += values.y;
                        end.z += values.x;
                    }

                    visualizeTiles.Add(start + offset + extraSlopedOffset);
                    visualizeTiles.Add(end + offset + extraSlopedOffset);


                    newEnd = end;
                    newEnd.x += worldData.pathfinder.tileSize;
                    visualizeTiles.Add(end + offset + extraSlopedOffset);
                    visualizeTiles.Add(newEnd + offset + extraSlopedOffset);
                }

            }
        }



        private Vector2 tileCosSin(WorldArea area, float angle)
        {
            Vector2 value = new Vector2();

            value.x = Mathf.Cos(angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * area.angledAreaResolutionDiffrence * -area.anglePositive;
            value.y = Mathf.Sin(angle * (Mathf.PI / 180)) * worldData.pathfinder.tileSize * area.angledAreaResolutionDiffrence;
            return value;

        }



        private void DefineFlatWorld()
        {
            worldData.worldAreas.Clear();

            WorldArea area = new WorldArea();
            area.index = 0;
            area.SetValuesFlatWorld(0, gridWidth, 0, gridLength, worldData);
            worldData.multiLevelSectorManager.SetupSectorsWorldArea(area);

            Vector3 startingPoint = worldData.pathfinder.worldStart;

            Vector3 rayStartingPoint = new Vector3(0, worldData.pathfinder.worldStart.y + worldData.pathfinder.worldHeight, 0);
            float tileOffset = (worldData.pathfinder.tileSize * 0.5f);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridLength; y++)
                {
                    rayStartingPoint.x = startingPoint.x + (x * worldData.pathfinder.tileSize) + tileOffset;
                    rayStartingPoint.z = startingPoint.z - (y * worldData.pathfinder.tileSize) - tileOffset;

                    RaycastHit hit;
                    if (Physics.Raycast(rayStartingPoint, Vector3.down, out hit, worldData.pathfinder.worldHeight + 0.15f, 1 << worldData.pathfinder.groundLayer)) // (worldHeight - distanceCovered) + 0.2f
                    {
                        area.tileGrid[x][y] = new Tile();
                        area.tileGrid[x][y].gridPos.x = x;
                        area.tileGrid[x][y].gridPos.y = y;
                        area.tileGrid[x][y].integrationValue = worldData.tileManager.tileResetIntegrationValue;
                        area.tileGrid[x][y].worldAreaIndex = 0;

                        area.tileGrid[x][y].yWorldPos = hit.point.y;
                        area.tileGrid[x][y].angle = Vector3.Angle(hit.normal, Vector3.up);


                        int SectorX = Mathf.FloorToInt(x / (float)area.levelDimensions[0][0]); //   /sectorwidth
                        int SectorY = Mathf.FloorToInt(y / (float)area.levelDimensions[0][1]);//    /sectorheight

                        area.tileGrid[x][y].sectorIndex = (SectorY * area.levelDimensions[0][2]) + SectorX;// *sectorgridWidth

                        // Debug.Log("world index  " + index + "   tileGrid[x][y].sector  " + tileGrid[x][y].sector + "  levelDimensions[0][0] " + levelDimensions[0][0]);
                        MultiLevelSector sector = area.sectorGrid[0][area.tileGrid[x][y].sectorIndex];
                        int deltaX = x - sector.left;
                        int deltaY = y - sector.top;
                        area.tileGrid[x][y].indexWithinSector = (deltaY * sector.tilesInWidth) + deltaX;
                    }
                }
            }

            worldData.worldAreas.Add(area);

        }



        public void GenerateWorldManually(WorldData _worldData, List<Tile[][]> tileGrids, List<IntVector2> tileGridOffset, bool autoConnectWorldAreas)
        {
            if (tileGrids.Count != tileGridOffset.Count)
                Debug.Log("<color=red> WARNING: each manually created WorldArea needs an offset value. </color>");
            else
            {
                worldData = _worldData;
                temporaryWorldLayers.Clear();
                visualizeTiles.Clear();

                GenerateWorldAreaIndexLayers();

                Debug.Log("worldData.layerdWorldAreaIndexes   " + worldData.layerdWorldAreaIndexes.Count);
                worldData.worldAreas.Clear();
                for (int i = 0; i < tileGrids.Count; i++)
                {  
                    GenerateFlatWorldArea(tileGrids[i], tileGridOffset[i], 0);
                }
                worldIsBuilt = true;
            }

            CreateSectorGraph();

            if (autoConnectWorldAreas)
            {
                ConnectWorldAreas(true);

                CombineWorldAreaConnectionsWithSectorGraph();
            }

            VisualizeTiles();
        }

        public void ConnectWorldAreas()
        { 
            ConnectWorldAreas(true);

            CombineWorldAreaConnectionsWithSectorGraph();

            VisualizeTiles();
        }

        private Vector2 WorldSideToVector2(WorldArea.Side areaSide)
        {
            switch (areaSide)
            {
                case WorldArea.Side.Top:
                    {
                        return Vector2.up;
                    }
                case WorldArea.Side.Down:
                    {
                        return -Vector2.up;
                    }
                case WorldArea.Side.Left:
                    {
                        return -Vector2.right;
                    }
                case WorldArea.Side.Right:
                    {
                        return Vector2.right;
                    }
            }

            return Vector2.zero;
        }

        private int WorldSideToDir(WorldArea.Side areaSide)
        {
            switch (areaSide)
            {
                case WorldArea.Side.Top:
                    {
                        return 1;
                    }
                case WorldArea.Side.Down:
                    {
                        return 2;
                    }
                case WorldArea.Side.Left:
                    {
                        return 6;
                    }
                case WorldArea.Side.Right:
                    {
                        return 3;
                    }
            }

            return 0;
        }

        public void ForceWorldAreaConnection(WorldArea area1, WorldArea area2, WorldArea.Side area1Side, WorldArea.Side area2Side, bool autoConnect)
        {
            IntVector2 manualAreaConnectionKey = new IntVector2(area1.index, area2.index);
            if (!worldData.manualWorldAreaConnections.ContainsKey(manualAreaConnectionKey))
                worldData.manualWorldAreaConnections.Add(manualAreaConnectionKey, WorldSideToDir(area1Side));

            manualAreaConnectionKey.x = area2.index;
            manualAreaConnectionKey.y = area1.index;
            if (!worldData.manualWorldAreaConnections.ContainsKey(manualAreaConnectionKey))
                worldData.manualWorldAreaConnections.Add(manualAreaConnectionKey, WorldSideToDir(area2Side));

            Tile tileArea1 = null;
            Tile tileArea2 = null;

            int area1XDirValue = 0;
            int area1YDirValue = 0;
            int area1XStart = 0;
            int area1YStart = 0;

            int area2XDirValue = 0;
            int area2YDirValue = 0;
            int area2XStart = 0;
            int area2YStart = 0;

            int length1 = 0;
            int length2 = 0;
            int length = 0;

            switch (area1Side)
            {
                case WorldArea.Side.Top:
                    {
                        length1 = area1.gridWidth;
                        area1XDirValue = 1;
                        break;
                    }
                case WorldArea.Side.Down:
                    {
                        length1 = area1.gridWidth;
                        area1XDirValue = 1;
                        area1YStart= area1.tileGrid[0].Length - 1;
                        break;
                    }
                case WorldArea.Side.Left:
                    {
                        length1 = area1.gridLength;
                        area1YDirValue = 1;
                        break;
                    }
                case WorldArea.Side.Right:
                    {
                        length1 = area1.gridLength;
                        area1YDirValue = 1;
                        area1XStart = area1.tileGrid.Length - 1;
                        break;
                    }
            }


            switch (area2Side)
            {
                case WorldArea.Side.Top:
                    {
                        length2 = area2.gridWidth;
                        area2XDirValue = 1;
                        break;
                    }
                case WorldArea.Side.Down:
                    {
                        length2 = area2.gridWidth;
                        area2XDirValue = 1;
                        area2YStart = area2.tileGrid[0].Length - 1;
                        break;
                    }
                case WorldArea.Side.Left:
                    {
                        length2 = area2.gridLength;
                        area2YDirValue = 1;
                        break;
                    }
                case WorldArea.Side.Right:
                    {
                        length2 = area2.gridLength;
                        area2YDirValue = 1;
                        area2XStart = area2.tileGrid.Length - 1;
                        break;
                    }
            }

            length = Mathf.Min(length1, length2);

            for (int i = 0; i < length; i++)
            {
                tileArea1 = area1.tileGrid[area1XStart + area1XDirValue * i][area1YStart + area1YDirValue * i];
                tileArea2 = area2.tileGrid[area2XStart + area2XDirValue * i][area2YStart + area2YDirValue * i];
                ConnectWorldAreaTiles(tileArea1, area1, tileArea2.gridPos.x + area2.leftOffset, tileArea2.gridPos.y + area2.topOffset, area2, true);
            }

            if (autoConnect)
            {
                CombineWorldAreaConnectionsWithSectorGraph();
                VisualizeTiles();
            }
        }





    }
}
