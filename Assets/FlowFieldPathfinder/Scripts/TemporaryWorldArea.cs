using UnityEngine;
using System.Collections;

namespace FlowPathfinding
{
    public class TemporaryWorldArea
    {
        public Tile[][] tileGrid;
        public bool[][] tileSlotTakenGrid;
        public int gridWidth;
        public int gridHeight;

        public void Setup(WorldData worldData)
        {
            gridWidth = (int)(worldData.pathfinder.worldWidth / worldData.pathfinder.tileSize);
            gridHeight = (int)(worldData.pathfinder.worldLength / worldData.pathfinder.tileSize);

            tileGrid = new Tile[gridWidth][];
            tileSlotTakenGrid = new bool[gridWidth][];
            for (int j = 0; j < gridWidth; j++)
            {
                tileGrid[j] = new Tile[gridHeight];
                tileSlotTakenGrid[j] = new bool[gridHeight];
            }
        }
    }
}
