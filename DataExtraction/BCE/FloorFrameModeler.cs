using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.BCExtractors;
using BCE.DataModels;
using BCE.DataModels.Basic;
using BCE.Helpers;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace BCE
{
    public class FloorFrameModeler
    {
        public static ExtructError Model(ref FloorPlan floorPlan)
        {
            bool ringsEvaluation = false;
            Data data = floorPlan.PData;
            Result result = floorPlan.PResult;

            #region 多边形提取+类型标识+门窗参数提取

            MergeParallelLines(ref data.WallLines);

            if (floorPlan.Configuration.LayerMapping.Balcony != null)
            {
                if (floorPlan.Configuration.LayerMapping.Balcony.Count > 0)
                    MergeParallelLines(ref data.BalcLines);
            }



            ringsEvaluation = EWalls.Extract(ref floorPlan);
            if (!ringsEvaluation)
                return ExtructError.Err_Wall;

            if (floorPlan.Configuration.LayerMapping.Balcony != null)
            {
                if (floorPlan.Configuration.LayerMapping.Balcony.Count > 0)
                {
                    ringsEvaluation = EBalustrade.Extract(ref floorPlan);
                    if (!ringsEvaluation)
                        return ExtructError.Err_Baluster;
                }
            }


            if (floorPlan.Configuration.LayerMapping.Door != null)
            {
                if (floorPlan.Configuration.LayerMapping.Door.Count > 0)
                {
                    EDoorOrWindow.Get_Window_TranformWindow(0, ref floorPlan);
                }
            }

            if (floorPlan.Configuration.LayerMapping.Window != null)
            {
                if (floorPlan.Configuration.LayerMapping.Window.Count > 0)
                {
                    EDoorOrWindow.Get_Window_TranformWindow(1, ref floorPlan);
                }
            }


            ringsEvaluation = EFunctionRegion.Extract(ref floorPlan);
            if (!ringsEvaluation)
                return ExtructError.Err_Region;
            #endregion


            #region 楼梯+电梯参数提取
            if (floorPlan.Configuration.LayerMapping.Stairs != null)
            {
                if (floorPlan.Configuration.LayerMapping.Stairs.Count > 0)
                {
                    EStair.Extract(ref floorPlan);
                }
            }

            if (floorPlan.Configuration.LayerMapping.Elevator != null)
            {
                if (floorPlan.Configuration.LayerMapping.Elevator.Count > 0)
                {
                    EElevator.Extract(ref floorPlan);
                }
            }
            #endregion



            #region 类型细分
            EDoorOrWindow.ClusterElevatorDoor(ref floorPlan);
            #endregion

            return ExtructError.None;
        }

        #region 公开方法
        /// <summary>
        /// 合并所有共线且有公共部分的线段
        /// </summary>
        /// <param name="lines">要进行平行线合并的线段集,合并结果将替换原结果</param>
        public static void MergeParallelLines(ref List<LineClass> lines)
        {
            List<List<LineClass>> clusterList = ClusterHelper.Lines_BySlope(ref lines);

            foreach (List<LineClass> clusterLines in clusterList)
            {
                if (clusterLines.Count > 1)
                {
                    for (int i = 0; i < clusterLines.Count; i++)
                    {
                        LineClass pLine1 = clusterLines[i];
                        if (!pLine1.ShouldMoved)
                        {
                            for (int j = 0; j < clusterLines.Count; j++)
                            {
                                LineClass pLine2 = clusterLines[j];
                                if (!pLine2.ShouldMoved && !object.ReferenceEquals(pLine1, pLine2))
                                {
                                    mergeTwoParallelLines(ref pLine1, ref pLine2);
                                }
                            }
                        }
                    }
                }
            }



            List<LineClass> processedLines = lines.FindAll(new Predicate<LineClass>((line) =>
            {
                if (!line.ShouldMoved)
                    return true;
                return false;
            }));

            lines = processedLines;
        }

        /// <summary>
        /// 及时将删除被标记为移除的墙线，并将新墙线添加进集合
        /// </summary>
        /// <param name="data"></param>
        public static void DeleteMoved_AddNew_WallLine(ref Data data)
        {

            List<LineClass> processedLines = data.WallLines.FindAll(new Predicate<LineClass>((line) =>
            {
                if (!line.ShouldMoved)
                    return true;
                return false;
            }));
            data.WallLines = processedLines;


            if (data.NewWallines != null)
            {
                data.WallLines.AddRange(data.NewWallines);
            }
        }

        /// <summary>
        /// 获取两条线段，两条线段来自于通过两个分割点将一条线段分割得到的三条线段中的两端
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static LineClass[] Get_TwoLines_ByTwoBreakPoints(Point point1, Point point2, LineClass line)
        {
            LineClass[] newLines = new LineClass[2];

            double distance1 = GeometryHelper.Calc_Distance(line.StartPoint, point1);
            double distance2 = GeometryHelper.Calc_Distance(line.StartPoint, point2);


            if (distance1 < distance2)
            {
                newLines[0] = new LineClass(line.StartPoint, point1, line.LayerName);
                newLines[1] = new LineClass(line.EndPoint, point2, line.LayerName);
            }
            else
            {
                newLines[0] = new LineClass(line.StartPoint, point2, line.LayerName);
                newLines[1] = new LineClass(line.EndPoint, point1, line.LayerName);
            }

            return newLines;
        }

        #endregion

        #region 内部方法
        /// <summary>
        /// 若两条平行线有公共部分（公共顶点或线段），则将第二条直线合并到第一条直线中
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns>若发生合并行为，返回 1,否则返回 0</returns>
        private static int mergeTwoParallelLines(ref LineClass line1, ref LineClass line2)
        {
            Relation2D_Collinear rc = GeometryHelper.CollinearRelation_2D(line1, line2);
            switch (rc)
            {
                case Relation2D_Collinear.NotCollinear:
                case Relation2D_Collinear.Separate:
                    return 0;
                case Relation2D_Collinear.Coincident:
                case Relation2D_Collinear.Containning:
                    line2.ShouldMoved = true;
                    return 1;
                case Relation2D_Collinear.Contained:
                    line1.ShouldMoved = true;
                    return 1;
                case Relation2D_Collinear.ShareOnePoint:
                case Relation2D_Collinear.PartialOverlapping:
                    Point farPoint1, nearPoint1;
                    Point middlePoint = line2.Get_MiddlePoint();
                    if (GeometryHelper.Calc_Distance(line1.StartPoint, middlePoint) >= GeometryHelper.Calc_Distance(line1.EndPoint, middlePoint))
                    {
                        farPoint1 = line1.StartPoint;
                        nearPoint1 = line1.EndPoint;
                    }
                    else
                    {
                        farPoint1 = line1.EndPoint;
                        nearPoint1 = line1.StartPoint;
                    }


                    Point farPoint2, nearPoint2;
                    if (GeometryHelper.Calc_Distance(line2.EndPoint, farPoint1) >= GeometryHelper.Calc_Distance(line2.StartPoint, farPoint1))
                    {
                        farPoint2 = line2.EndPoint;
                        nearPoint2 = line2.StartPoint;
                    }
                    else
                    {
                        farPoint2 = line2.StartPoint;
                        nearPoint2 = line2.EndPoint;
                    }

                    line1.StartPoint = farPoint1;
                    line1.EndPoint = farPoint2;
                    line2.ShouldMoved = true;//标记line2应该被移除
                    return 1;
                default:
                    throw new Exception("线段合并失败！");
            }
        }
        #endregion
    }
}
