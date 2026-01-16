using BCE.DataModels;
using DXFReader.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels.Basic;
using MathExtension.Geometry;
using BCE.Helpers;
using MathExtension.Vector;

namespace BCE.BCExtractors
{


    /// <summary>
    /// 提取楼梯参数
    /// </summary>
    public abstract class EStair
    {
        public static void Extract(ref FloorPlan floorPlan)
        {
            if (floorPlan.PResult.Stairs == null)
                floorPlan.PResult.Stairs = new List<Stair>();

            List<SemanticPolygon> sp_stairRooms = (from sp in floorPlan.PResult.SPolygons
                                                   where sp.PType == PolygonType.StairRoom
                                                   select sp).ToList<SemanticPolygon>();

            if (sp_stairRooms.Count == 0)
                return;

 
            List<List<LineClass>> srLines_GroupBySR = groupLines_ByStairRooms(ref sp_stairRooms, ref floorPlan);

            for (int i = 0; i < srLines_GroupBySR.Count; i++)
            {
                List<LineClass> lines = srLines_GroupBySR[i];
                FloorFrameModeler.MergeParallelLines(ref lines);
                srLines_GroupBySR[i] = lines;
            }


            List<TextClass> downTexts_GroupBySR = getDownTexts_ByStairRooms(ref sp_stairRooms, ref floorPlan);

    
            switch (floorPlan.Configuration.FPType)
            {
                case PlaneType.Plan_FirstFloor:
                    break;
                case PlaneType.Plan_StandardFloor:
                    for (int i = 0; i < srLines_GroupBySR.Count; i++)
                    {
                        List<LineClass> containedLines = srLines_GroupBySR[i];
                        if (containedLines.Count == 0)
                        {
                            // 建议：不要直接报错，而是输出警告并跳过，去处理下一个楼梯间
                            System.Diagnostics.Debug.WriteLine($"警告：第 {i} 个楼梯间分组内未检测到楼梯线，已跳过。");
                            continue; // 【关键】跳过本次循环，进入下一次循环
                        }
                        /*if (containedLines.Count == 0)
                            throw new ArgumentException("在楼梯间内未检测到楼梯线！");*/

                        TextClass label = get_FirstDownText(ref downTexts_GroupBySR);
                        if (label == null)
                            throw new ArgumentException("在楼梯间内未检测到包含\"下\"的注记！");


                        Stair stair = extract_DSatir(ref containedLines, ref label, floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness);
                        floorPlan.PResult.Stairs.Add(stair);
                    }
                    break;
                case PlaneType.Pan_TopFloor:
                    break;
                default:
                    break;
            }
        }

        #region 内部方法
        /// <summary>
        /// 提取单个双跑楼梯参数
        /// </summary>
        /// <param name="stairLines_containedBySR">被单个楼梯间包含的所有线段</param>
        /// <param name="downTexts_GroupBySR">被单个楼梯间包含的所有标注</param>
        /// <returns></returns>
        private static Stair extract_DSatir(ref List<LineClass> containedLines, ref TextClass label, double floorHeight)
        {
            Stair stair = new Stair();//楼梯
            stair.Height_Floor = floorHeight;

            List<LineClass> mainParallelLines = null;//与台阶线平行的所有线段
            List<LineClass> stepLines_first;//第一梯段线
            List<LineClass> stepLines_second;//第二梯段线
            LineClass topLine = null;//休息平台的顶线


            #region 线段聚类
            List<List<LineClass>> groupedLinesByAngle = ClusterHelper.Lines_BySlope(ref containedLines);

            groupedLinesByAngle = (from lines in groupedLinesByAngle
                                   orderby lines.Count descending
                                   select lines).ToList<List<LineClass>>();


            mainParallelLines = groupedLinesByAngle[0];

        
            mainParallelLines = (from line in mainParallelLines
                                 orderby line.Length descending
                                 select line).ToList<LineClass>();
            #endregion





            #region 提取  梯间宽 + 梯段宽 + 井宽 + 台阶数
            topLine = mainParallelLines[0];
            stair.Width_Staircase = topLine.Length;


            List<List<LineClass>> groupedLinesByLength = groupLinesByLength(ref mainParallelLines);
            groupedLinesByLength = (from p in groupedLinesByLength
                                    orderby p.Count descending
                                    select p).ToList<List<LineClass>>();

            stepLines_first = groupedLinesByLength[0];
            stepLines_second = groupedLinesByLength[1];

            int secondStepLinesCount = stepLines_second.Count;
            int countDiff = groupedLinesByAngle[0].Count - groupedLinesByAngle[1].Count;
            double length1 = stepLines_first.OrderByDescending(LineClass => LineClass.Length).ElementAt(0).Length;//找出台阶线最大长度
            double length2 = stepLines_second.OrderByDescending(LineClass => LineClass.Length).ElementAt(0).Length;
            const double maxStairWellWidth = 130;

            if (secondStepLinesCount >= 4 && countDiff <= 3 && length2 >= 700)
            {
              
                stair.StepNum = 2 * (stepLines_first.Count - 1);
                stair.Width_Stairwell = stair.Width_Staircase - (stepLines_first[0].Length + stepLines_second[0].Length);


                if (stair.Width_Stairwell > maxStairWellWidth)
                    stair.Width_Stairwell = maxStairWellWidth;

                stair.Width_Stairway = (stair.Width_Staircase - stair.Width_Stairwell) / 2;
            }
            else
            {
               
                stair.StepNum = stepLines_first.Count + 2 - 2;
                stair.Width_Stairwell = stair.Width_Staircase - 2 * stepLines_first[0].Length;

     
                if (stair.Width_Stairwell > maxStairWellWidth)
                    stair.Width_Stairwell = maxStairWellWidth;

                stair.Width_Stairway = (stair.Width_Staircase - stair.Width_Stairwell) / 2;
            }
            #endregion

            #region 提取  休息平台宽 + 台阶宽
            List<double> distances = new List<double>();
            for (int ii = 0; ii < stepLines_first.Count; ii++)
                distances.Add(GeometryHelper.Calc_Distance(stepLines_first[ii].StartPoint, topLine));

            distances.Sort();
            stair.Width_Land = distances[0];

            double depth_stairway = distances[distances.Count - 1];
            stair.Width_Step = (depth_stairway - stair.Width_Land) / (stair.StepNum / 2);
            #endregion

            #region 确定  上楼位置 + 定位参数


            double distance1 = GeometryHelper.Calc_Distance(topLine.StartPoint, label.BasePoint);
            double distance2 = GeometryHelper.Calc_Distance(topLine.EndPoint, label.BasePoint);
            Point nearPoint;//距离‘下’较近的端点
            Point farPoint;

            if (distance1 > distance2)
                topLine.Reverse();

            nearPoint = topLine.StartPoint;
            farPoint = topLine.EndPoint;

       
            stair.UpstairPosition = get_DoubleStairType(nearPoint, farPoint, label.BasePoint);

            if (stair.UpstairPosition == UpstairPosition.Left)
                stair.InsertPoint = nearPoint;
            else if (stair.UpstairPosition == UpstairPosition.Right)
                stair.InsertPoint = farPoint;
            switch (stair.UpstairPosition)
            {

                case UpstairPosition.Left:
                    stair.RotateAngle = GeometryHelper.Calc_AngleTo_XAxis(topLine);
                    break;
                case UpstairPosition.Right:
                    stair.RotateAngle = GeometryHelper.Calc_AngleTo_XAxis(new Line(farPoint, nearPoint));
                    break;
                case UpstairPosition.Unknow:
                    stair.RotateAngle = double.NaN;
                    break;
            }
            #endregion
            return stair;
        }



        /// <summary>
        /// 将楼梯线按照楼梯间聚类
        /// </summary>
        /// <param name="sp_stairRooms"></param>
        /// <param name="floorPan"></param>
        /// <returns></returns>
        private static List<List<LineClass>> groupLines_ByStairRooms(ref List<SemanticPolygon> sp_stairRooms, ref FloorPlan floorPan)
        {

            LineClass el = new LineClass();
            el.EndPoint = new Point(91044.0145, -77727.5071);
            el.StartPoint = new Point(94404.0145, -77727.5071);

            List<List<LineClass>> srLines_GroupBySR = new List<List<LineClass>>();
            foreach (SemanticPolygon sp_sr in sp_stairRooms)
            {
                List<LineClass> containedLines = new List<LineClass>();
                foreach (var item in floorPan.PData.StairLines)
                {
                    if (item.Equals(el))
                    {
                        int asasas = 0;
                        asasas++;
                    }

                        if (GeometryHelper.BasicRelation_2D((Polygon)sp_sr, item) == Relation2D_Line_Plane.InPlane)
                        {
                            containedLines.Add(item);
                            item.Used = true;
                        }
                }

                srLines_GroupBySR.Add(containedLines);
            }
            for (int i = 0; i < floorPan.PData.StairLines.Count; i++)
                floorPan.PData.StairLines[i].Used = false;

            return srLines_GroupBySR;
        }

        /// <summary>
        /// 将标注按照楼梯间聚类
        /// </summary>
        /// <param name="sp_stairRooms"></param>
        /// <param name="floorPan"></param>
        /// <returns></returns>
        private static List<TextClass> getDownTexts_ByStairRooms(ref  List<SemanticPolygon> sp_stairRooms, ref FloorPlan floorPan)
        {
            
            List<TextClass> texts = new List<TextClass>();

            foreach (SemanticPolygon sp_sr in sp_stairRooms)
            {

                foreach (TextClass item in floorPan.PData.Annotations)
                {
                    if (item.Content.Contains("下") && GeometryHelper.BasicRelation_2D((Polygon)sp_sr, item.BasePoint) != Relation2D_Point_Plane.OutPlane)
                    {
                        texts.Add(item);
                        break;
                    }
                }
            }

            return texts;
        }

        /// <summary>
        /// 找出包好"下"的第一个标注
        /// </summary>
        /// <param name="texts"></param>
        /// <returns></returns>
        private static TextClass get_FirstDownText(ref List<TextClass> texts)
        {
            foreach (TextClass item in texts)
            {
                if (item.Content.Contains("下"))
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 在斜率聚类的基础上再进行长度聚类(长度相差不超过2%)
        /// </summary>
        /// <param name="pStairLines"></param>
        /// <returns></returns>
        private static List<List<LineClass>> groupLinesByLength(ref List<LineClass> pStairLines)
        {
            List<List<LineClass>> list = new List<List<LineClass>>();



            for (int i = 0; i < pStairLines.Count; i++)
            {
                LineClass pLi = pStairLines[i];
                if (!pLi.Used)
                {
                    pLi.Used = true;
                    List<LineClass> item_Lines = new List<LineClass>();
                    item_Lines.Add(pLi);

                    for (int j = i + 1; j < pStairLines.Count; j++)
                    {
                        LineClass pLj = pStairLines[j];
                        if (!pLj.Used && Math.Abs(pLj.Length - pLi.Length) <= pLi.Length * 0.02f)//长度相差不超过2%
                        {
                            item_Lines.Add(pLj);
                            pLj.Used = true;//以true 标记
                        }
                    }
                    list.Add(item_Lines);
                }
            }


            //重置标识标记
            for (int i = 0; i < pStairLines.Count; i++)
                pStairLines[i].Used = false;


            return list;
        }

        /// <summary>
        /// 判别上楼位置（靠左或靠右）
        /// </summary>
        /// <param name="nearPoint"></param>
        /// <param name="farPoint"></param>
        /// <param name="downTextPoint"></param>
        /// <returns></returns>
        private static UpstairPosition get_DoubleStairType(Point nearPoint, Point farPoint, Point downTextPoint)
        {
            //构造两个矢量
            Vector v_near = (nearPoint - downTextPoint).ToVector();
            Vector v_far = (farPoint - downTextPoint).ToVector();
            Vector v = v_near.CrossMultiply(v_far);

            if (v.Z < 0)
                return UpstairPosition.Left;
            if (v.Z > 0)
                return UpstairPosition.Right;
            else
                return UpstairPosition.Unknow;
        }
        #endregion
    }
}
