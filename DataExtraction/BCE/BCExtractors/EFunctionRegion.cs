using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels;
using BCE.DataModels.Basic;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace BCE.BCExtractors
{ 
    public abstract class EFunctionRegion
    {
        public static double GapWidth;


        public static bool Extract(ref FloorPlan floorPlan)
        {
            Data data = floorPlan.PData;
            Result result = floorPlan.PResult;
            GapWidth = floorPlan.Configuration.ThreadValue_GapWidth;

            result.ER_ForFuncRegion = new ERings(data.WallLines, GapWidth);
            result.CollectRingExtractionResult(ref result.ER_ForFuncRegion);
            identify_SP_TypeInfo(ref result.ER_ForFuncRegion, ref data.Annotations, ref data.ElevLines, ref result.SPolygons);

            //计算除去楼板之外的楼板多边形的门洞中点点
            //calc_SP_DoorPoints(ref result.SPolygons, ref data);


            return ERings.Evaluate_RingsCreation(ref result.ER_ForFuncRegion);
        }



        #region 内部方法
        /// <summary>
        /// 根据引导特征标识功能区类型
        /// </summary>
        /// <param name="data"></param>
        /// <param name="result"></param>
        private static void identify_SP_TypeInfo(ref ERings eRings, ref List<TextClass> labels, ref List<LineClass> elevatorLines, ref List<SemanticPolygon> sPolygons)
        {
            #region 引导特征 1：面积特征->标识楼板,楼板面积最大


            List<SemanticPolygon> orderedSPs = (from p in eRings.Rings
                                                orderby p.Area descending
                                                select p).ToList<SemanticPolygon>();

            orderedSPs[0].PType = PolygonType.Floor;
            #endregion

            #region 引导特征 2： 标注文本->标识房间及楼梯间
            if (orderedSPs.Count < 2)
                throw new Exception("网格小于<2个,没有房间可以提取!");

            for (int i = 1; i < orderedSPs.Count; i++)
            {
                SemanticPolygon sp = orderedSPs[i];
                StringBuilder sb = new StringBuilder();

                for (int j = 0; j < labels.Count; j++)
                {
                    TextClass pAnnotation = labels[j];

               
                    if (GeometryHelper.BasicRelation_2D((Polygon)sp, pAnnotation.BasePoint) == Relation2D_Point_Plane.InPlane)
                        sb.Append(pAnnotation.Content);
                }

                //根据注记文本标定房间类型及备注信息
                string annotation = sb.ToString().Trim();
                if (annotation.Contains("卧室"))
                    sp.PType = PolygonType.BedRoom;
                else if (annotation.Contains("客厅"))
                    sp.PType = PolygonType.Parlour;
                else if (annotation.Contains("卫生间"))
                    sp.PType = PolygonType.Toilet;
                else if (annotation.Contains("厨房"))
                    sp.PType = PolygonType.Kitchen;
                else if (annotation.Contains("阳台"))
                    sp.PType = PolygonType.Balcony;
                else if (annotation.Contains("书房"))
                    sp.PType = PolygonType.Study;
                else if (annotation.Contains('上') || annotation.Contains('下'))
                    sp.PType = PolygonType.StairRoom;
                else
                    sp.PType = PolygonType.Unknown;
            }
            #endregion

            #region 引导特征 3： 电梯特征点->表示电梯间，电梯符号存在等长交叉电梯线的交点
            List<Point> elevatorPoints = find_ElevatorPoint(ref elevatorLines);
            SemanticPolygon elevator_sp;
            foreach (var ePoint in elevatorPoints)
            {

                for (int i = 1; i < orderedSPs.Count; i++)
                {
                    elevator_sp = orderedSPs[i];
                    if (elevator_sp.PType == PolygonType.Unknown && GeometryHelper.BasicRelation_2D((Polygon)elevator_sp, ePoint) == Relation2D_Point_Plane.InPlane)
                    {
                        orderedSPs[i].PType = PolygonType.ElevatorShaft;
                        break;
                    }
                }
            }

            #endregion

            #region 引导特征4：线段类型特征->标识尚未完成类型标识的区域(如不含标注的阳台)
            List<SemanticPolygon> unknownTypeSPs = (from p in orderedSPs
                                                    where p.PType == PolygonType.Unknown
                                                    select p).ToList<SemanticPolygon>();


            foreach (var item in unknownTypeSPs)
            {
                //标识不含注记的阳台功能区
                int balconyLineCount = item.Lines.FindAll((line) =>
                {
                    if (line.LType == LineType.BalconyLine)
                        return true;
                    return false;
                }).Count;

                if (balconyLineCount > 0) item.PType = PolygonType.Balcony;
            }


            #endregion
        }

        /// <summary>
        /// 计算电梯特征点交
        /// </summary>
        /// <param name="elevatorLines"></param>
        /// <returns></returns>
        private static List<Point> find_ElevatorPoint(ref List<LineClass> elevatorLines)
        {
            List<Point> elevatorPoints = new List<Point>();

            for (int i = 0; i < elevatorLines.Count - 1; i++)
            {
                if (elevatorLines[i].Used == false)
                {
                    Point middlePoint = elevatorLines[i].Get_MiddlePoint();
                    for (int j = i + 1; j < elevatorLines.Count; j++)
                    {
                        if (elevatorLines[j].Get_MiddlePoint().Equals(middlePoint))
                        {
                            elevatorLines[j].Used = true;
                            elevatorPoints.Add(middlePoint);
                            break;
                        }
                    }
                }
            }

            return elevatorPoints;
        }
        #endregion
    }
}
