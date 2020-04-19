using System;
using System.Collections.Generic;
using System.Text;

namespace Wanderer
{
    public class Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        // The operator methods are used for comparison of two different positions
        public static bool operator ==(Position one, Position another)
        {
            return one.X == another.X && one.Y == another.Y;
        }

        public static bool operator !=(Position one, Position another)
        {
            return one.X != another.X || one.Y != another.Y;
        }
    }
}
