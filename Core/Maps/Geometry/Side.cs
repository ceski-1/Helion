﻿using Helion.Maps.Geometry.Lines;
using Helion.Util;
using Helion.Util.Geometry;

namespace Helion.Maps.Geometry
{
    public class Side
    {
        public readonly int Id;
        public Vec2I Offset;
        public UpperString LowerTexture;
        public UpperString MiddleTexture;
        public UpperString UpperTexture;
        public readonly Sector Sector;

        public Line Line {
            get {
                if (line == null)
                    throw new HelionException("Trying to get the line for a side that has not had it set");
                return line;
            }
            set { line = value; }
        }
        private Line? line;

        public bool IsFront => ReferenceEquals(this, Line.Front);
        public bool IsBack => !IsFront;
        public Side? PartnerSide => Line.TwoSided ? (ReferenceEquals(this, Line.Front) ? Line.Back : Line.Front) : null;

        public Side(int id, Vec2I offset, UpperString lowerTexture, UpperString middleTexture, UpperString upperTexture, 
            Sector sector)
        {
            Id = id;
            Offset = offset;
            LowerTexture = lowerTexture;
            MiddleTexture = middleTexture;
            UpperTexture = upperTexture;
            Sector = sector;

            sector.Add(this);
        }
    }
}
