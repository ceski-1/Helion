using Helion.Maps.Specials;
using Helion.Maps.Specials.ZDoom;
using Helion.Util.Geometry;
using Helion.World.Entities;
using Helion.World.Entities.Players;
using Helion.World.Geometry.Sides;
using Helion.World.Special;
using static Helion.Util.Assertion.Assert;

namespace Helion.World.Geometry.Lines
{
    public class Line
    {
        public readonly int Id;
        public readonly int MapId;
        public readonly Seg2D Segment;
        public readonly Side Front;
        public readonly Side? Back;
        public readonly Side[] Sides;
        public readonly SpecialArgs Args;
        public readonly LineFlags Flags;
        public readonly LineSpecial Special;
        public bool Activated;

        public bool OneSided => Back == null;
        public bool TwoSided => !OneSided;
        public bool HasSpecial => Special.LineSpecialType != ZDoomLineSpecialType.None;
        public bool HasSectorTag => SectorTag > 0;
        
        // TODO: Any way we can encapsulate this somehow?
        public int SectorTag => Args.Arg0;
        public byte TagArg => Args.Arg0;
        public byte SpeedArg => Args.Arg1;
        public byte DelayArg => Args.Arg2;
        public byte AmountArg => Args.Arg2;

        public Line(int id, int mapId, Seg2D segment, Side front, Side? back, LineFlags flags, 
            LineSpecial lineSpecial, SpecialArgs args)
        {
            Precondition(id == mapId, "Line mismatch from generated ID to map ID");
            
            Id = id;
            MapId = mapId;
            Segment = segment;
            Front = front;
            Back = back;
            Sides = (back == null ? new[] { front } : new[] { front, back });
            Flags = flags;
            Special = lineSpecial;
            Args = args;

            front.Line = this;
            front.Sector.Lines.Add(this);

            if (back != null)
            {
                back.Line = this;
                back.Sector.Lines.Add(this);
            }
        }

        /// <summary>
        /// If the line blocks the given entity. Only checks line properties
        /// and flags. No sector checking.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the entity is blocked by this line, false
        /// otherwise.</returns>
        public bool BlocksEntity(Entity entity)
        {
            return OneSided || (entity is Player && Flags.Blocking.Players);
        }
    }
}