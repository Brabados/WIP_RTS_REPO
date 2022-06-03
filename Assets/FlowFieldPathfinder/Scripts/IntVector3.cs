using System;

namespace FlowPathfinding
{
    public struct IntVector3
    {
        public int x, y, z;

        public IntVector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static bool operator !=(IntVector3 vector1, IntVector3 vector2)
        {
            return vector1.x != vector2.x || vector1.y != vector2.y || vector1.z != vector2.z;
        }

        public static bool operator ==(IntVector3 vector1, IntVector3 vector2)
        {
            return vector1.x == vector2.x && vector1.y == vector2.y && vector1.z == vector2.z;
        }


        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IntVector3 p = (IntVector3)obj;
            if ((System.Object)p == null)
            {
                return false;
            }
            return (x == p.x) && (y == p.y) && (z == p.z);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}