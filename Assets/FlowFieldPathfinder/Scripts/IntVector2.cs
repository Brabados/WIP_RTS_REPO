using System;

namespace FlowPathfinding
{
    public struct IntVector2
    {
        public int x, y;

        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator !=(IntVector2 vector1, IntVector2 vector2)
        {
            return vector1.x != vector2.x || vector1.y != vector2.y;
        }

        public static bool operator ==(IntVector2 vector1, IntVector2 vector2)
        {
            return vector1.x == vector2.x && vector1.y == vector2.y;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IntVector2 p = (IntVector2)obj;
            if ((System.Object)p == null)
            {
                return false;
            }
            return (x == p.x) && (y == p.y);
        }



        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}