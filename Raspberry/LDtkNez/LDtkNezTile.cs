using System;
using Nez;

namespace LDtkNez
{
    public class LDtkNezTile
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsSlope()
        {
            throw new NotImplementedException();
        }

        public double GetSlope()
        {
            throw new NotImplementedException();
        }

        public bool IsOneWayPlatform()
        {
            throw new NotImplementedException();
        }

        public Edge GetNearestEdge(int perpindicularPosition)
        {
            throw new NotImplementedException();
        }

        public Edge GetHighestSlopeEdge()
        {
            throw new NotImplementedException();
        }

        public double GetSlopeOffset()
        {
            throw new NotImplementedException();
        }
    }
}