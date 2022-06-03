using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class TileManager
    {
        private int[] straightDirections = new int[] { 0, 1, 0, -1, 1, 0, -1, 0 };
        private int[] diagonalDirections = new int[] { 1, -1, 1, 1, -1, 1, -1, -1 };

        public WorldData worldData;

        public int tileBlockedValue = 2000000000;// 2 bilion
        public int tileResetIntegrationValue = 2000000000; // 2 bilion


        public List<Tile> GetAllNeighboursforAStarSearch(Tile tile, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();
            Tile neighbour = null;
            int checkX = 0;
            int checkY = 0;

            //straight
            for (int i = 0; i < straightDirections.Length; i += 2)
            {
                checkX = tile.gridPos.x + straightDirections[i];
                checkY = tile.gridPos.y + straightDirections[i + 1];

                if (checkX > -1 && checkX < area.gridWidth && checkY > -1 && checkY < area.gridLength)
                {
                    neighbour = area.tileGrid[checkX][checkY];

                    if (neighbour != null && !neighbour.blocked && neighbour.sectorIndex == tile.sectorIndex && TilesWithinRangeGeneration(tile, neighbour))
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }

            // diagonal
            for (int i = 0; i < diagonalDirections.Length; i += 2)
            {
                checkX = tile.gridPos.x + diagonalDirections[i];
                checkY = tile.gridPos.y + diagonalDirections[i + 1];

                if (checkX > -1 && checkX < area.gridWidth && checkY > -1 && checkY < area.gridLength)
                {
                    neighbour = area.tileGrid[checkX][checkY];
                    if (neighbour != null && !neighbour.blocked && neighbour.sectorIndex == tile.sectorIndex)
                    {

                        if ((area.tileGrid[neighbour.gridPos.x][tile.gridPos.y] == null || area.tileGrid[neighbour.gridPos.x][tile.gridPos.y].blocked) && (area.tileGrid[tile.gridPos.x][neighbour.gridPos.y] == null || area.tileGrid[tile.gridPos.x][neighbour.gridPos.y].blocked))
                        {
                            // diagonal was blocked off
                        }
                        else if (TilesWithinRangeGeneration(tile, neighbour))
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                }
            }

            return neighbours;
        }

        public List<Tile> GetAllNeighboursForSectorNodeSearch(Tile tile, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();
            Tile neighbour = null;
            int checkX = 0;
            int checkY = 0;

            //straight
            for (int i = 0; i < straightDirections.Length; i += 2)
            {
                checkX = tile.gridPos.x + straightDirections[i];
                checkY = tile.gridPos.y + straightDirections[i + 1];

                if (checkX > -1 && checkX < area.gridWidth && checkY > -1 && checkY < area.gridLength)
                {
                    

                    neighbour = area.tileGrid[checkX][checkY];


                    //if (worldData.worldBuilder.worldIsBuilt)
                    //    Debug.Log("tile.sector " + tile.sector + "   neighbour  " + neighbour.sector);


                    if (neighbour != null && !neighbour.blocked && neighbour.sectorIndex == tile.sectorIndex && TilesWithinRangeGeneration(tile, neighbour))
                    {
                        int newCost = tile.integrationValue + neighbour.cost * 10;

                        //if (worldData.worldBuilder.worldIsBuilt)
                        //    Debug.Log("add right?");
                        if (newCost < neighbour.integrationValue)
                        {

                            //if (worldData.worldBuilder.worldIsBuilt)
                            //    Debug.Log("ADDED");
                            neighbour.integrationValue = newCost;
                            neighbours.Add(neighbour);
                        }
                    }
                }
            }

            // diagonal
            for (int i = 0; i < diagonalDirections.Length; i += 2)
            {
                checkX = tile.gridPos.x + diagonalDirections[i];
                checkY = tile.gridPos.y + diagonalDirections[i + 1];

                if (checkX > -1 && checkX < area.gridWidth && checkY > -1 && checkY < area.gridLength)
                {
                    neighbour = area.tileGrid[checkX][checkY];
                    if (neighbour != null && !neighbour.blocked && neighbour.sectorIndex == tile.sectorIndex)
                    {
                        int newCost = tile.integrationValue + neighbour.cost * 14;
                        if (newCost < neighbour.integrationValue) // if not blocked
                        {
                            if ((area.tileGrid[neighbour.gridPos.x][tile.gridPos.y] == null || area.tileGrid[neighbour.gridPos.x][tile.gridPos.y].blocked) && (area.tileGrid[tile.gridPos.x][neighbour.gridPos.y] == null || area.tileGrid[tile.gridPos.x][neighbour.gridPos.y].blocked))
                            {
                                // diagonal was blocked off
                            }
                            else if (TilesWithinRangeGeneration(tile, neighbour))
                            {
                                neighbour.integrationValue = newCost;
                                neighbours.Add(neighbour);
                            }
                        }
                    }
                }
            }

            return neighbours;
        }

        public List<Tile> GetNeighboursExpansionSearch(Tile node, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();

            int directionValue = node.gridPos.x + 1;
            if (directionValue < area.gridWidth && area.searchField[directionValue][node.gridPos.y] && area.tileGrid[directionValue][node.gridPos.y] != null) // right
            {
                if (!area.tileGrid[directionValue][node.gridPos.y].blocked && TilesWithinRangeGeneration(node, area.tileGrid[directionValue][node.gridPos.y]))
                {
                    int newCost = node.integrationValue + area.tileGrid[directionValue][node.gridPos.y].cost;
                    if (newCost < area.tileGrid[directionValue][node.gridPos.y].integrationValue)
                    {
                        area.tileGrid[directionValue][node.gridPos.y].integrationValue = newCost;
                        neighbours.Add(area.tileGrid[directionValue][node.gridPos.y]);
                    }
                }
            }


            directionValue = node.gridPos.x - 1;
            if (directionValue > -1 && area.searchField[directionValue][node.gridPos.y] && area.tileGrid[directionValue][node.gridPos.y] != null) // left
            {
                if (!area.tileGrid[directionValue][node.gridPos.y].blocked && TilesWithinRangeGeneration(node, area.tileGrid[directionValue][node.gridPos.y]))
                {
                    int newCost = node.integrationValue + area.tileGrid[directionValue][node.gridPos.y].cost;
                    if (newCost < area.tileGrid[directionValue][node.gridPos.y].integrationValue)
                    {
                        area.tileGrid[directionValue][node.gridPos.y].integrationValue = newCost;
                        neighbours.Add(area.tileGrid[directionValue][node.gridPos.y]);
                    }
                }
            }


            directionValue = node.gridPos.y - 1;
            if (directionValue > -1 && area.searchField[node.gridPos.x][directionValue] && area.tileGrid[node.gridPos.x][directionValue] != null) // top
            {
                if (!area.tileGrid[node.gridPos.x][directionValue].blocked && TilesWithinRangeGeneration(node, area.tileGrid[node.gridPos.x][directionValue]))
                {
                    int newCost = node.integrationValue + area.tileGrid[node.gridPos.x][directionValue].cost;
                    if (newCost < area.tileGrid[node.gridPos.x][directionValue].integrationValue)
                    {
                        area.tileGrid[node.gridPos.x][directionValue].integrationValue = newCost;
                        neighbours.Add(area.tileGrid[node.gridPos.x][directionValue]);
                    }
                }
            }


            directionValue = node.gridPos.y + 1;
            if (directionValue < area.gridLength && area.searchField[node.gridPos.x][directionValue] && area.tileGrid[node.gridPos.x][directionValue] != null) // bot
            {
                if (!area.tileGrid[node.gridPos.x][directionValue].blocked && TilesWithinRangeGeneration(node, area.tileGrid[node.gridPos.x][directionValue]))
                {
                    int newCost = node.integrationValue + area.tileGrid[node.gridPos.x][directionValue].cost;
                    if (newCost < area.tileGrid[node.gridPos.x][directionValue].integrationValue)
                    {
                        area.tileGrid[node.gridPos.x][directionValue].integrationValue = newCost;
                        neighbours.Add(area.tileGrid[node.gridPos.x][directionValue]);
                    }
                }
            }

            return neighbours;
        }


        public Tile GetLowestIntergrationCostTile(Tile tile, WorldArea area)
        {
            Tile neighbour;
            Tile lowestCostNode = tile;
            int checkX = 0;
            int checkY = 0;
            if (area != null)
            {
                //straight
                for (int i = 0; i < straightDirections.Length; i += 2)
                {
                    checkX = tile.gridPos.x + straightDirections[i];
                    checkY = tile.gridPos.y + straightDirections[i + 1];

                    if (checkX > -1 && checkX < area.gridWidth && checkY > -1 && checkY < area.gridLength && area.tileGrid[checkX][checkY] != null)
                    {
                        neighbour = area.tileGrid[checkX][checkY];
                        if (neighbour.integrationValue < lowestCostNode.integrationValue && TilesWithinRangeGeneration(tile, neighbour))
                            lowestCostNode = neighbour;
                    }
                }


                // diagonal
                for (int i = 0; i < diagonalDirections.Length; i += 2)
                {
                    checkX = tile.gridPos.x + diagonalDirections[i];
                    checkY = tile.gridPos.y + diagonalDirections[i + 1];

                    if (checkX > -1 && checkX < area.gridWidth && checkY > -1 && checkY < area.gridLength && area.tileGrid[checkX][checkY] != null)
                    {
                        neighbour = area.tileGrid[checkX][checkY];
                        if (neighbour.integrationValue < lowestCostNode.integrationValue && TilesWithinRangeGeneration(tile, neighbour))
                        {
                            if ((area.tileGrid[neighbour.gridPos.x][tile.gridPos.y] == null || area.tileGrid[neighbour.gridPos.x][tile.gridPos.y].blocked) || (area.tileGrid[tile.gridPos.x][neighbour.gridPos.y] == null || area.tileGrid[tile.gridPos.x][neighbour.gridPos.y].blocked))
                            {
                                // diagonal was blocked off
                            }
                            else
                                lowestCostNode = neighbour;
                        }
                    }
                }
            }


            return lowestCostNode;
        }


        public Vector3 GetTileWorldPosition(Tile tile, WorldArea worldArea)
        {
            return new Vector3(worldData.pathfinder.worldStart.x + ((worldArea.leftOffset + tile.gridPos.x) * worldData.pathfinder.tileSize) + (worldData.pathfinder.tileSize * 0.5f), tile.yWorldPos, worldData.pathfinder.worldStart.z - ((worldArea.topOffset + tile.gridPos.y) * worldData.pathfinder.tileSize) - (worldData.pathfinder.tileSize * 0.5f));
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            //return new Vector3(-gridWidth * 0.5f + tile.gridPos.x + 0.5f, 0, gridHeight * 0.5f - tile.gridPos.y - 0.5f);
            return new Vector3(worldData.pathfinder.worldStart.x + (x * worldData.pathfinder.tileSize) + (worldData.pathfinder.tileSize * 0.5f), 0, worldData.pathfinder.worldStart.z - (y * worldData.pathfinder.tileSize) - (worldData.pathfinder.tileSize * 0.5f));
        }

        public WorldArea GetWorldAreaAtPosition(Vector3 location)
        {
            if (location.x > worldData.pathfinder.worldStart.x && location.x < worldData.pathfinder.worldStart.x + worldData.pathfinder.worldWidth && location.z < worldData.pathfinder.worldStart.z && location.z > worldData.pathfinder.worldStart.z - worldData.pathfinder.worldLength)
            {
                if (worldData.pathfinder.worldIsMultiLayered)
                {
                    int worldX = (int)((location.x - worldData.pathfinder.worldStart.x) / worldData.pathfinder.tileSize); //Mathf.FloorToInt(location.x) + (int)(pathfinder.worldWidth * 0.5f);
                    int worldY = (int)(((worldData.pathfinder.worldStart.z - location.z) / worldData.pathfinder.tileSize));       //(int)(pathfinder.worldLength * 0.5f) - Mathf.CeilToInt(location.z);

                    int yLayer = GetHeightLayer(location.y);
                    int prevYlayer = -1;

                    int searchCount = 0;
                    while (searchCount < 3)
                    {
                        if (searchCount == 1)
                            yLayer = GetHeightLayer(location.y + worldData.pathfinder.generationClimbHeight);
                        else if (searchCount == 2)
                            yLayer = GetHeightLayer(location.y - worldData.pathfinder.generationClimbHeight);


                        //Debug.Log("ylayer " + yLayer + "   area  " + worldData.layerdWorldAreaIndexes[yLayer][worldX][worldY]);
                        if (prevYlayer != yLayer && yLayer > -1 && yLayer < worldData.layerdWorldAreaIndexes.Count && worldData.layerdWorldAreaIndexes[yLayer][worldX][worldY] != -1) // not empty
                            return worldData.worldAreas[worldData.layerdWorldAreaIndexes[yLayer][worldX][worldY]];

                        prevYlayer = yLayer;
                        searchCount++;
                    }
                }
                else
                    return worldData.worldAreas[0];
            }

            return null;
        }

        public WorldArea GetWorldAreaAtGuaranteedPosition(int worldX, float y, int worldY)
        {
            int yLayer = GetHeightLayer(y);
            int prevYlayer = -1;

            int searchCount = 0;
            while (searchCount < 3)
            {
                if (searchCount == 1)
                    yLayer = GetHeightLayer(y + worldData.pathfinder.generationClimbHeight);
                else if (searchCount == 2)
                    yLayer = GetHeightLayer(y - worldData.pathfinder.generationClimbHeight);


                if (prevYlayer != yLayer && yLayer > -1 && yLayer < worldData.layerdWorldAreaIndexes.Count && worldData.layerdWorldAreaIndexes[yLayer][worldX][worldY] != -1) // not empty
                    return worldData.worldAreas[worldData.layerdWorldAreaIndexes[yLayer][worldX][worldY]];

                prevYlayer = yLayer;
                searchCount++;
            }
            return null;
        }

        public List<Tile> GetLeftAndRightNeighbour(Tile tile, WorldArea area)
        {
            List<Tile> tiles = new List<Tile>();

            if (tile.gridPos.x + 1 < area.gridWidth && area.tileGrid[tile.gridPos.x + 1][tile.gridPos.y] != null)
                tiles.Add(area.tileGrid[tile.gridPos.x + 1][tile.gridPos.y]);

            if (tile.gridPos.x - 1 > -1 && area.tileGrid[tile.gridPos.x - 1][tile.gridPos.y] != null)
                tiles.Add(area.tileGrid[tile.gridPos.x - 1][tile.gridPos.y]);

            return tiles;
        }

        public List<Tile> GetTopAndBottomNeighbour(Tile tile, WorldArea area)
        {
            List<Tile> tiles = new List<Tile>();

            if (tile.gridPos.y + 1 < area.gridLength && area.tileGrid[tile.gridPos.x][tile.gridPos.y + 1] != null)
                tiles.Add(area.tileGrid[tile.gridPos.x][tile.gridPos.y + 1]);

            if (tile.gridPos.y - 1 > -1 && area.tileGrid[tile.gridPos.x][tile.gridPos.y - 1] != null)
                tiles.Add(area.tileGrid[tile.gridPos.x][tile.gridPos.y - 1]);

            return tiles;
        }


        public int locationToWorldGridX(Vector3 location)
        {
            return (int)((location.x - worldData.pathfinder.worldStart.x) / worldData.pathfinder.tileSize);
        }

        public int locationToWorldGridY(Vector3 location)
        {
            if (worldData.pathfinder.worldIsMultiLayered == false && worldData.pathfinder.twoDimensionalMode)
                return Mathf.Abs( (int)(((worldData.pathfinder.worldStart.y - location.y) / worldData.pathfinder.tileSize)));
            else
                return (int)(((worldData.pathfinder.worldStart.z - location.z) / worldData.pathfinder.tileSize));
        }

        public Tile GetTileInWorldArea(WorldArea area, Vector3 location)
        {
            if (area != null)
            {
                int worldX = locationToWorldGridX(location); //Mathf.FloorToInt(location.x) + (int)(pathfinder.worldWidth * 0.5f);
                int worldY = locationToWorldGridY(location);       //(int)(pathfinder.worldLength * 0.5f) - Mathf.CeilToInt(location.z);

                //Debug.Log("GetTileInWorldArea " + area.index +"  " + area.left + "  " + area.top + "   location " + location);
                //Debug.Log((worldY - area.top) + "  " + area.tileGrid[0].Length);
                int x = worldX - area.leftOffset;
                int y = worldY - area.topOffset;
                if (x > -1 && x < area.tileGrid.Length && y > -1 && y < area.tileGrid[0].Length)
                {
                    Tile tile = area.tileGrid[x][y];

                    if (tile != null && TilesWithinRange(location.y, tile.yWorldPos))
                        return tile;
                    else
                        return null;
                }
                else
                    return null;
            }
            else
                return null;
        }


        public Tile GetTileInWorldArea(WorldArea area, int x, int y)
        {
            if (x > -1 && x < area.gridWidth && y > -1 && y < area.gridLength)
                return area.tileGrid[x][y];
            else
                return null;
        }

        public Tile GetTileFromPosition(Vector3 worldPosition)
        {
            WorldArea area = GetWorldAreaAtPosition(worldPosition);
            if(area != null)
                return GetTileInWorldArea(area, worldPosition);
            else
                return null;
        }




        public int GetHeightLayer(float y)
        {
            return Mathf.FloorToInt((y - worldData.pathfinder.worldStart.y) / worldData.pathfinder.characterHeight);
        }

        public bool TilesWithinRangeGeneration(Tile a, Tile b)
        {
            if (Mathf.Abs(a.yWorldPos - b.yWorldPos) <= worldData.pathfinder.generationClimbHeight)
                return true;
            else
                return false;
        }

        public bool TilesWithinRangeGenerationTEMP(Tile a, Tile b)
        {
            if (Mathf.Abs(a.yWorldPos - b.yWorldPos) <= 0.3f)
                return true;
            else
                return false;
        }

        public bool TilesWithinRange(float worldY1, float worldY2)
        {
            if (Mathf.Abs(worldY1 - worldY2) <= worldData.pathfinder.generationClimbHeight)
                return true;
            else
                return false;
        }


        public List<Tile> GetStraightNeighbours(int x, int y, WorldArea area)
        {
            List<Tile> neighbours = new List<Tile>();

            int directionValue = x + 1;
            if (directionValue < area.gridWidth && area.tileGrid[directionValue][y] != null) // right
                neighbours.Add(area.tileGrid[directionValue][y]);

            directionValue = x - 1;
            if (directionValue > -1 && area.tileGrid[directionValue][y] != null) // left
                neighbours.Add(area.tileGrid[directionValue][y]);

            directionValue = y - 1;
            if (directionValue > -1 && area.tileGrid[x][directionValue] != null) // top
                neighbours.Add(area.tileGrid[x][directionValue]);

            directionValue = y + 1;
            if (directionValue < area.gridLength && area.tileGrid[x][directionValue] != null) // bot
                neighbours.Add(area.tileGrid[x][directionValue]);

            return neighbours;
        }





    }
}
