using DXFReader.DataModel;
using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BCE.DataModels.Basic
{
    public interface IProfile
    {
        List<LineClass> LinearGPs { get; set; }
        List<Point> DistinctedPoints { get; }
        Polygon ToPolygon();
        double Area { get; }
        double PerArcLength { get; set; }
        //BGPExtractor.DataModels.Basic.Type_Profile PT { get; set; }

    }
}
