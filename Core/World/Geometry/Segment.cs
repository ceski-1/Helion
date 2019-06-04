﻿using Helion.Maps.Geometry;
using Helion.Util.Geometry;

namespace Helion.World.Geometry
{
    /// <summary>
    /// An edge of a subsector.
    /// </summary>
    public class Segment : Seg2Fixed
    {
        public readonly int Id;
        public Side? Side;
        public readonly int OffsetX;

        public Line? Line => Side?.Line;
        public bool IsMiniseg => Side == null;

        public Segment(int id, Side? side, Vec2Fixed start, Vec2Fixed end) : base(start, end)
        {
            Id = id;
            Side = side;
            OffsetX = CalculateLineXOffset();
        }

        private int CalculateLineXOffset()
        {
            if (Side == null || Line == null)
                return 0;

            if (Side.IsFront)
                return (int)(Line.StartVertex.Position - Start.ToDouble()).Length();
            return (int)(Line.EndVertex.Position - End.ToDouble()).Length();
        }
    }
}
