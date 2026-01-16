using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels;
using BCE.DataModels.Basic;
using DXFReader.DataModel;

namespace BCE.BCExtractors
{
    public abstract class EWalls
    {
        public static double GapWidth;//测试值240

        public static bool Extract(ref FloorPlan floorPlan)
        {
            Data data = floorPlan.PData;
            Result result = floorPlan.PResult;
            GapWidth = floorPlan.Configuration.ThreadValue_GapWidth;
            result.ER_ForWalls = new ERings(data.WallLines, GapWidth);
            result.CollectRingExtractionResult(
                ref result.ER_ForWalls,
                PolygonType.Wall,
                LineType.WallLine,
                (r, t) => { return ERings.DeepCopy_Rings(result.ER_ForWalls.Rings, PolygonType.Wall); });



            return ERings.Evaluate_RingsCreation(ref  result.ER_ForWalls);
        }
    }
}
