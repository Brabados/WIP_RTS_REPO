using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class FlowFieldPath
    {
        public int key;
        public Tile destination;
        public List<int> worldAreas;
        public FlowField flowField;
        public IntergrationField intergrationField;

        public void Create(Tile _destination, List<int> _worldAreas, IntergrationField _intergrationField, FlowField _flowField, int _key)
        {
            worldAreas = _worldAreas;
            destination = _destination;
            intergrationField = _intergrationField;
            flowField = _flowField;
            key = _key;
        }


    }
}
