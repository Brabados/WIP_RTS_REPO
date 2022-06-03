using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FlowPathfinding
{
    public class AbstractNode : IHeapItem<AbstractNode>
    {
        public int sector = 0;
        public int worldAreaIndex;
        public Tile tileConnection = null;

        public AbstractNode parent = null;

        public int G = 0;
        public int H = 0;
        public int F
        {
            get { return G + H; }
        }

        private int heapIndex;

        public AbstractNode nodeConnectionToOtherSector = null;
        public Dictionary<AbstractNode, int> connections = new Dictionary<AbstractNode, int>();



        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }
            set
            {
                heapIndex = value;
            }
        }


        public int CompareTo(AbstractNode nodeToCompare)
        {
            int compare = F.CompareTo(nodeToCompare.F);
            if (compare == 0)
                compare = H.CompareTo(nodeToCompare.H);

            return -compare;
        }
    }
}

