﻿using System.Collections.Generic;
using System.Linq;
using Helion.Maps.Geometry;
using Helion.Maps.Geometry.Lines;
using Helion.Maps.Special.Specials;
using Helion.World.Physics;

namespace Helion.Maps.Special
{
    public class SpecialManager
    {
        private List<ISpecial> m_specials = new List<ISpecial>();
        private PhysicsManager m_physicsManager;
        private IMap m_map;

        public SpecialManager(PhysicsManager physicsManager, IMap map)
        {
            m_physicsManager = physicsManager;
            m_map = map;
        }

        public bool AddActivatedLineSpecial(EntityActivateSpecialEventArgs args)
        {
            if (args.ActivateLineSpecial == null || (args.ActivateLineSpecial.Activated && !args.ActivateLineSpecial.Special.Repeat))
                return false;

            int startSpecialCount = m_specials.Count;
            var special = args.ActivateLineSpecial.Special;

            if (special.IsTeleport())
            {
                AddSpecial(new TeleportSpecial(args, m_physicsManager, m_map));
            }
            else if (special.IsSectorMoveSpecial())
            {
                LineSpecialData specialData = special.GetLineSpecialData();
                if (specialData.Speed > 0.0)
                {
                    List<Sector> sectors = GetSectorsFromSpecialLine(args.ActivateLineSpecial);
                    foreach (var sector in sectors)
                    {
                        if (sector != null && !sector.IsMoving)
                        {
                            double destZ = GetDestZ(sector, specialData.SectorDestination);
                            AddSpecial(new SectorMoveSpecial(sector, destZ, specialData));
                        }
                    }
                }
            }

            if (m_specials.Count > startSpecialCount)
            {
                args.ActivateLineSpecial.Activated = true;
                return true;
            }

            return false;
        }

        public void AddSpecial(ISpecial special)
        {
            m_specials.Add(special);
        }

        public void Tick()
        {
            for (int i = 0; i < m_specials.Count; i++)
            {
                ISpecial special = m_specials[i];

                if (special.Tick() == SpecialTickStatus.Destroy)
                {
                    m_specials.RemoveAt(i);
                    i--;
                }
            }
        }

        private List<Sector> GetSectorsFromSpecialLine(Line line)
        {
            if (line.HasSectorTag)
                return m_map.Sectors.Where(x => x.Tag == line.SectorTag).ToList();
            else if (!line.OneSided)
                return new List<Sector> { line.Sides[1].Sector };

            return new List<Sector> { };
        }

        private double GetDestZ(Sector sector, SectorDest destination)
        {
            switch (destination)
            {
                case SectorDest.LowestAdjacentFloor:
                    return GetLowestFloorDestZ(sector);
                case SectorDest.HighestAdjacentFloor:
                    return GetHighestFloorDestZ(sector);
                case SectorDest.LowestAdjacentCeiling:
                    return GetLowestCeilingDestZ(sector);
                case SectorDest.HighestAdjacentCeiling:
                    return GetHighestCeilingDestZ(sector);
                case SectorDest.Floor:
                    return sector.Floor.Z;
                case SectorDest.Ceiling:
                    return sector.Ceiling.Z;
            }

            return 0;
        }

        private double GetLowestFloorDestZ(Sector sector)
        {
            Sector? destSector = m_map.GetLowestAdjacentFloor(sector);
            if (destSector == null)
                return sector.Floor.Z;
            else
                return destSector.Floor.Z;
        }

        private double GetHighestFloorDestZ(Sector sector)
        {
            Sector? destSector = m_map.GetHighestAdjacentFloor(sector);
            if (destSector == null)
                return sector.Floor.Z;
            else
                return destSector.Floor.Z;
        }

        private double GetLowestCeilingDestZ(Sector sector)
        {
            Sector? destSector = m_map.GetLowestAdjacentCeiling(sector);
            if (destSector == null)
                return sector.Ceiling.Z;
            else
                return destSector.Ceiling.Z;
        }

        private double GetHighestCeilingDestZ(Sector sector)
        {
            Sector? destSector = m_map.GetHighestAdjacentCeiling(sector);
            if (destSector == null)
                return sector.Ceiling.Z;
            else
                return destSector.Ceiling.Z;
        }
    }
}
