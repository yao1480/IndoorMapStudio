using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExtension.Geometry;

namespace BCE.DataModels.Interface
{
    public interface ILocatorData
    {
        Point InsertPoint { get; set; }
        double AngleToXAxis { get; set; }
    }
}
