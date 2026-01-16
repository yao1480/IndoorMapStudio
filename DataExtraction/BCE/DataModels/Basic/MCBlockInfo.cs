using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace BCE.DataModels.Basic
{
    public class MCBlockInfo
    {
        public Point[] FourPoints;
        public MCType Type_MC;

        public MCBlockInfo()
        {
            FourPoints = new Point[4];
            Type_MC = MCType.Unknown;
        }
    }
}
