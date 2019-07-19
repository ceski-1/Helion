using System;
using static Helion.Util.Assertion.Assert;

namespace Helion.Util.Geometry
{
    /// <summary>
    /// A two dimensional box, which follows the cartesian coordinate system.
    /// </summary>
    public struct Box3D
    {
        /// <summary>
        /// The minimum point in the box. This is equal to the bottom left 
        /// corner.
        /// </summary>
        public Vec3D Min;

        /// <summary>
        /// The maximum point in the box. This is equal to the top right 
        /// corner.
        /// </summary>
        public Vec3D Max;

        /// <summary>
        /// The top value of the box.
        /// </summary>
        public double Top => Max.Z;
        
        /// <summary>
        /// The bottom value of the box.
        /// </summary>
        public double Bottom => Min.Z;

        /// <summary>
        /// Creates a box from a bottom left and top right point. It is an 
        /// error if the min has any coordinate greater the maximum point.
        /// </summary>
        /// <param name="min">The bottom left point.</param>
        /// <param name="max">The top right point.</param>
        public Box3D(Vec3D min, Vec3D max)
        {
            Precondition(min.X <= max.X, "Bounding box min X > max X");
            Precondition(min.Y <= max.Y, "Bounding box min Y > max Y");
            Precondition(min.Z <= max.Z, "Bounding box min Z > max Z");

            Min = min;
            Max = max;
        }
        
        /// <summary>
        /// Creates a bigger box from a series of smaller boxes, returning such
        /// a box that encapsulates minimally all the provided arguments.
        /// </summary>
        /// <param name="firstBox">The first box in the sequence.</param>
        /// <param name="boxes">The remaining boxes, if any.</param>
        /// <returns>A box that encases all of the args tightly.</returns>
        public static Box3D Combine(Box3D firstBox, params Box3D[] boxes)
        {
            Vec3D min = firstBox.Min;
            Vec3D max = firstBox.Max;

            foreach (Box3D box in boxes)
            {
                min.X = Math.Min(min.X, box.Min.X);
                min.Y = Math.Min(min.Y, box.Min.Y);
                min.Z = Math.Min(min.Z, box.Min.Z);
                
                max.X = Math.Max(max.X, box.Max.X);
                max.Y = Math.Max(max.Y, box.Max.Y);
                max.Z = Math.Max(max.Z, box.Max.Z);
            }

            return new Box3D(min, max);
        }

        /// <summary>
        /// Checks if the box contains the point. Being on the edge is not
        /// considered to be containing.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if it is inside, false if not.</returns>
        public bool Contains(Vec2D point)
        {
            return point.X <= Min.X || point.X >= Max.X || point.Y <= Min.Y || point.Y >= Max.Y;
        }
        
        /// <summary>
        /// Checks if the box contains the point. Being on the edge is not
        /// considered to be containing.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if it is inside, false if not.</returns>
        public bool Contains(Vec3D point)
        {
            return point.X <= Min.X || point.X >= Max.X || 
                   point.Y <= Min.Y || point.Y >= Max.Y ||
                   point.Z <= Min.Z || point.Z >= Max.Z;
        }

        /// <summary>
        /// Checks if the boxes overlap. Touching is not considered to be
        /// overlapping.
        /// </summary>
        /// <param name="box">The other box to check against.</param>
        /// <returns>True if they overlap, false if not.</returns>
        public bool Overlaps(Box3D box)
        {
            return !(Min.X >= box.Max.X || Max.X <= box.Min.X || 
                     Min.Y >= box.Max.Y || Max.Y <= box.Min.Y ||
                     Min.Z >= box.Max.Z || Max.Z <= box.Min.Z);
        }

        /// <summary>
        /// Calculates the sides of this bounding box.
        /// </summary>
        /// <returns>The sides of the bounding box.</returns>
        public Vec3D Sides() => Max - Min;
        
        /// <summary>
        /// Gets a 2-dimensional box by dropping the Z axis.
        /// </summary>
        /// <returns>The two dimensional representation of this box.</returns>
        public Box2D To2D() => new Box2D(Min.To2D(), Max.To2D());

        /// <inheritdoc/>
        public override string ToString() => $"({Min}), ({Max})";
    }
}