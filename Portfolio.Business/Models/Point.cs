using System.Collections.Generic;

namespace Portfolio.Business.Models
{
    public struct Point
    {
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; private set; }

        public float Y { get; private set; }
    }

    public class Points
        : List<Point>
    {
    }
}
