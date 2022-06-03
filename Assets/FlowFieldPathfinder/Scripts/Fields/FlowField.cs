using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class FlowField
    {
        public Dictionary<IntVector2, int[]> field;

        public FlowField()
        {
            IntVectorComparer comparer = new IntVectorComparer();
            field = new Dictionary<IntVector2, int[]>(comparer);
        }

        public void AddFields(List<int> sectors, int tilesInSectorAmount, WorldArea area)
        {
            IntVector2 key = new IntVector2();
            key.x = area.index;
            foreach (int sectorIndex in sectors)
            {
                key.y = sectorIndex;
                if (!field.ContainsKey(key))
                    field.Add(key, new int[tilesInSectorAmount]);
            }
        }


        public void FillFlowField(List<Tile> tiles, WorldData worldData)
        {
            WorldArea area = null;

            if (tiles.Count > 0)
                area = worldData.worldAreas[tiles[0].worldAreaIndex];

            IntVector2 key = new IntVector2();
            //Vector2 vec = new Vector2();
            Tile lowestCostTile;

            foreach (Tile tile in tiles)
            {
                if (key.x != tile.worldAreaIndex)
                    area = worldData.worldAreas[tile.worldAreaIndex];

                key.x = tile.worldAreaIndex;
                key.y = tile.sectorIndex;


                lowestCostTile = worldData.tileManager.GetLowestIntergrationCostTile(tile, area);

                if (lowestCostTile != tile)
                {
                    field[key][tile.indexWithinSector] = worldData.flowFieldManager.GetDirBetweenVectors(tile.gridPos, lowestCostTile.gridPos); 

                    //vec.x = lowestCostTile.gridPos.x - tile.gridPos.x;
                    //vec.y = tile.gridPos.y - lowestCostTile.gridPos.y;
                    //field[key][tile.indexWithinSector] = vec.normalized; //new Vector2(lowestCostTile.gridPos.x - tile.gridPos.x, tile.gridPos.y - lowestCostTile.gridPos.y).normalized;
                }
            }
        }

        public void AddEmptyField(int sectorIndex, int tilesInSectorAmount, WorldArea area)
        {
            field.Add(new IntVector2(area.index, sectorIndex), new int[tilesInSectorAmount]);
        }
    }
}
