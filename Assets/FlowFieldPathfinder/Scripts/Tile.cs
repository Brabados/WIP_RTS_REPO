using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class Tile
    {
        public int sectorIndex = -1;
        public int worldAreaIndex = -1;
        
        public bool blocked = false;
        
        public int cost = 1;
        public int integrationValue = 0;

        public IntVector2 gridPos = new IntVector2(-1, -1);
        public float yWorldPos = 0;
        public float angle = 0;

        // index as if it was in a seperate grid in a sector 
        public int indexWithinSector = -1;

        // an abstract node matches this tile
        public bool hasAbstractNodeConnection = false;
    }
}
