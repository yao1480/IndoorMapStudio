using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels;
using BCE.DataModels.Basic;
using BCE.Helpers;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace BCE.BCExtractors
{
    public abstract class EBalustrade
    {
        public static double MaxLength_EndLine_Baluster;//测试值240

        public static bool Extract(ref FloorPlan floorPlan)
        {
            Data data = floorPlan.PData;
            Result result = floorPlan.PResult;
            MaxLength_EndLine_Baluster = floorPlan.Configuration.ThreadValue_MaxWallWidth;

            result.ER_ForBalustrade = new ERings(data.BalcLines, floorPlan.Configuration.ThreadValue_GapWidth);

            result.CollectRingExtractionResult(
                ref result.ER_ForBalustrade,
                PolygonType.Balustrade,
                LineType.BalconyLine,
                (r, t) => { return ERings.DeepCopy_Rings(result.ER_ForBalustrade.Rings, PolygonType.Balustrade); });

     

  
            if (!ERings.Evaluate_RingsCreation(ref result.ER_ForBalustrade)) return false;


            #region 与阳台衔接墙线的裁剪
            List<LineClass> balc_ends = ClusterHelper.GetLines_ByLength(data.BalcLines, MaxLength_EndLine_Baluster);

            for (int i = 0; i < balc_ends.Count; i++)
            {
                LineClass balc_End = balc_ends[i] as LineClass;
                LineClass wallLine = find_ContainningLine(ref data.WallLines, balc_End);
                if (wallLine != null)
                {
                    balc_End.ShouldMoved = true;//标识该阳台端线需要移除

                    wallLine.Used = true;
                    wallLine.ShouldMoved = true;//标识该墙线需要移除

                    //添加裁剪生成的墙线
                    LineClass[] lines = FloorFrameModeler.Get_TwoLines_ByTwoBreakPoints(balc_End.StartPoint, balc_End.EndPoint, wallLine);
                    for (int k = 0; k < lines.Length; k++)
                    {
                        if (!lines[k].StartPoint.Equals(lines[k].EndPoint))
                            data.NewWallines.Add(lines[k]);
                    }
                }
            }
            #endregion


            for (int i = 0; i < data.WallLines.Count; i++)
                data.WallLines[i].Used = false;


            for (int i = 0; i < data.BalcLines.Count; i++)
            {
                data.BalcLines[i].LType = LineType.BalconyLine;
            }
            data.WallLines.AddRange(data.BalcLines);


            FloorFrameModeler.DeleteMoved_AddNew_WallLine(ref data);

            return true;
        }

        #region 内部方法
        private static LineClass find_ContainningLine(ref List<LineClass> wallLines, LineClass balc_End)
        {
            foreach (LineClass item in wallLines)
            {
                if (!item.Used)
                {
                    Relation2D_Collinear rc = GeometryHelper.CollinearRelation_2D(item, balc_End);
                    if (rc == Relation2D_Collinear.PartialOverlapping || rc == Relation2D_Collinear.Containning || rc == Relation2D_Collinear.Coincident)
                        return item;
                }
            }
            return null;
        }
        #endregion
    }
}
