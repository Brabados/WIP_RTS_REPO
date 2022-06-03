using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class IntVectorComparer : EqualityComparer<IntVector2>
    {

        public override bool Equals(IntVector2 vector1, IntVector2 vector2)
        {
            return vector1.x == vector2.x && vector1.y == vector2.y;
        }

        public override int GetHashCode(IntVector2 obj)
        {
            return base.GetHashCode();
        }
    }
}
