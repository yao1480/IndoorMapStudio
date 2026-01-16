using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace FloorConstructor.DataModels.Implements
{
    public struct ColoredGeometry : IEquatable<ColoredGeometry>
    {
        public IGeometry Geometry;
        public IColor RenderColor;


        public ColoredGeometry(IGeometry geometry, IColor renderColor)
        {
            Geometry = geometry;
            RenderColor = renderColor;
        }



        public bool Equals(ColoredGeometry other)
        {
            if (Object.ReferenceEquals(other, null)) return false;


            if (Object.ReferenceEquals(this, other)) return true;

            if (this.Geometry == other.Geometry && this.RenderColor == other.RenderColor) return true;

            return false;
        }
    }



}
