using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class IntergrationField
    {
        public Dictionary<IntVector2, int[]> field = new Dictionary<IntVector2, int[]>();

        public void AddFields(List<int> sectors, int tilesInSectorAmount, List<Tile> tiles, WorldArea area)
        {
            IntVector2 key = new IntVector2();

            foreach (int sectorIndex in sectors)
            {
                key = new IntVector2(area.index, sectorIndex);
                //Debug.Log("key  " + key.x   + " " + key.y);

                if (!field.ContainsKey(key))
                    field.Add(key, new int[tilesInSectorAmount]);
            }


            foreach (Tile tile in tiles)
            {
                key.x = area.index;
                key.y = tile.sectorIndex;
                if (!field.ContainsKey(key))
                    Debug.Log("FALSE KEY  " + key.x + "  " + key.y);

                field[key][tile.indexWithinSector] = tile.integrationValue;
            }
        }

        public void AddEmptyField(int sectorIndex, int tilesInSectorAmount, WorldArea area)
        {
            field.Add(new IntVector2(area.index, sectorIndex), new int[tilesInSectorAmount]);
        }
    }
}
