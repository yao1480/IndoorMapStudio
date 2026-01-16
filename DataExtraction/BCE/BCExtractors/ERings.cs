using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DXFReader.Reader;
using DXFReader;
using DXFReader.DataModel;
using BCE.DataModels;
using System.Windows;

namespace BCE.BCExtractors
{

    /// <summary>
    /// 基于零散线性图元（LinearGP）提取闭合轮廓
    /// </summary>
    public class ERings
    {
        double gapWidth;//闭合阈值
        List<LineClass> Lines = null;

        public List<LineClass> IsolateLines;
        public List<SemanticPolygon> IncompRings = null;
        public List<SemanticPolygon> Rings = null;

        public ERings(List<LineClass> pLines, double gapWidth = 10d)
        {
            Lines = pLines;
            this.gapWidth = gapWidth;

            IsolateLines = new List<LineClass>();
            IncompRings = new List<SemanticPolygon>();
            Rings = new List<SemanticPolygon>();

            //try
            //{
            //提取所有LBU
            extractRings();
            //}
            //catch (Exception ex)
            //{

            //throw new Exception("提取多边形失败： " + ex.Message + " !");
            //}
            //finally
            //{
            //    //重置线段使用标记
            for (int i = 0; i < pLines.Count; i++)
                pLines[i].Used = false;
            //}
        }



        /// <summary>
        /// 评价闭合环提取效果
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public static bool Evaluate_RingsCreation(ref ERings er)
        {
            int error, warnning;
            #region 计算warnning、error数目
            error = er.IncompRings.Count;
            warnning = er.IsolateLines.Count;

            if (er.Rings.Count + er.IncompRings.Count == 0)
            {
                warnning = er.IsolateLines.Count;
                error = 1;
            }
            #endregion

            #region 闭合环提取效果评估及反馈
            if (error > 0)
            {
                MessageBox.Show(string.Format("闭合环提取过程中出现 {0} 个错误,必须修改才能继续", error),
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            else if (warnning > 0)
            {
                if (MessageBoxResult.Yes == MessageBox.Show(string.Format("闭合环提取过程中出现 {0} 个警告,要中断并修改吗？", warnning),
                   "错误",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Warning))
                    return false;
            }

            return true;
            #endregion






        }

        /// <summary>
        /// 深拷贝ECircuits中的多边形，并指定其类型
        /// </summary>
        /// <param name="oldSPs"></param>
        /// <param name="polygonType"></param>
        /// <returns></returns>
        public static List<SemanticPolygon> DeepCopy_Rings(List<SemanticPolygon> oldSPs, PolygonType polygonType)
        {
            List<SemanticPolygon> newSPs = new List<SemanticPolygon>();

            SemanticPolygon oldSP;
            SemanticPolygon newSP;
            LineClass oldLine;
            for (int i = 0; i < oldSPs.Count; i++)
            {
                oldSP = oldSPs[i];
                newSP = new SemanticPolygon();
                newSP.PType = polygonType;

                for (int j = 0; j < oldSP.Lines.Count; j++)
                {
                    oldLine = oldSP.Lines[j];
                    newSP.Lines.Add(new LineClass(
                         new MathExtension.Geometry.Point(oldLine.StartPoint.X, oldLine.StartPoint.Y, oldLine.StartPoint.Z),
                         new MathExtension.Geometry.Point(oldLine.EndPoint.X, oldLine.EndPoint.Y, oldLine.EndPoint.Z)
                          ));
                }
                newSPs.Add(newSP);
            }

            return newSPs;
        }

        /// <summary>
        /// 计算Erings包含的所有线段的XY值范围(顺序：Xmin,Xmax.Ymin,Ymax)
        /// </summary>
        /// <returns></returns>
        public double[] Calc_Extent()
        {
            List<MathExtension.Geometry.Point> points = new List<MathExtension.Geometry.Point>() { Capacity = 50 };

            if (Rings != null)
            {
                foreach (var item in this.Rings)
                {
                    points.AddRange(item.Points);
                }
            }

            if (IncompRings != null)
            {
                foreach (var item in this.IncompRings)
                {
                    points.AddRange(item.Points);
                }
            }

            if (IsolateLines != null)
            {
                foreach (var item in this.IsolateLines)
                {
                    points.Add(item.StartPoint);
                    points.Add(item.EndPoint);
                }
            }


            if (points.Count == 0) return null;


            double[] extent = new double[4];


            extent[0] = (from p in points
                         orderby p.X
                         select p.X).First<double>();

            extent[1] = (from p in points
                         orderby p.X descending
                         select p.X).First<double>();

            extent[2] = (from p in points
                         orderby p.Y
                         select p.Y).First<double>();

            extent[3] = (from p in points
                         orderby p.Y descending
                         select p.Y).First<double>();

            return extent;

        }


        #region 内部方法
        private void extractRings()
        {
            List<LineClass> unusedLines = null;
            List<LineClass> nextLines = null;
            LineClass currentLine = null;

            foreach (var item in Lines)
            {
                if (item.Used)
                    continue;

                currentLine = item;
                currentLine.Used = true;

                if (isIsolatedLine(currentLine))
                {
                
                    IsolateLines.Add(currentLine);
                }
                else
                {
          
                    List<LineClass> ringLines = new List<LineClass>();
                    ringLines.Add(currentLine);

                    unusedLines = (from p in Lines
                                   where !p.Used
                                   select p).ToList<LineClass>();

                    LineClass nextLine = null;
                    foreach (var p in unusedLines)
                    {
                        var tempP = p; // 【关键】创建一个临时变量来承接 p
                        if (isAllowedDistace(ref currentLine, ref tempP, gapWidth))
                        {
                            nextLine = tempP;
                            break; // 找到第一个就退出，模拟 .First()
                        }
                    }

                    /*                    LineClass nextLine = (from p in unusedLines
                                                              where isAllowedDistace(ref currentLine, ref p, gapWidth)
                                                              select p).First<LineClass>();*/


                    while (nextLine != null)
                    {
                        ringLines.Add(nextLine);
                        nextLine.Used = true;

                        currentLine = nextLine;

                        nextLines = new List<LineClass>();
                        foreach (var p in unusedLines)
                        {
                            // 手动执行 where !p.Used
                            if (p.Used) continue;

                            var tempP = p; // 【关键】创建一个临时变量来承接 p

                            // 使用临时变量 tempP 传入 ref
                            if (isAllowedDistace(ref currentLine, ref tempP, gapWidth))
                            {
                                nextLines.Add(tempP);
                            }
                        }

                        /* nextLines = (from p in unusedLines
                                      where !p.Used && isAllowedDistace(ref currentLine, ref p, gapWidth)
                                      select p).ToList<LineClass>();*/

                        if (nextLines.Count == 0)
                            nextLine = null;
                        else
                            nextLine = nextLines[0];
                    }


                    LineClass firstLine = ringLines[0];
                    LineClass lastLine = ringLines[ringLines.Count - 1];

                    SemanticPolygon sp = new SemanticPolygon();
                    sp.Lines = ringLines;
                    if (MathExtension.Geometry.GeometryHelper.Calc_Distance(lastLine.EndPoint, firstLine.StartPoint) <= gapWidth)
                        Rings.Add(sp);
                    else
                        IncompRings.Add(sp);
                }
            }

        }

        private bool isAllowedDistace(ref LineClass currentLine, ref LineClass otherLine, double threadvalue_distance)
        {
            if (MathExtension.Geometry.GeometryHelper.Calc_Distance(currentLine.EndPoint, otherLine.StartPoint) < threadvalue_distance)
                return true;

            if (MathExtension.Geometry.GeometryHelper.Calc_Distance(currentLine.EndPoint, otherLine.EndPoint) < threadvalue_distance)
            {
                otherLine.Reverse();
                return true;
            }
            return false;
        }

        private bool isIsolatedLine(LineClass startLine)
        {
            // === 第一轮检查 ===
            foreach (var p in Lines)
            {
                // 1. 排除已使用的
                if (p.Used) continue;

                // 2.【关键】创建临时变量以传递 ref 参数
                var tempP = p;

                // 3. 判断距离
                if (isAllowedDistace(ref startLine, ref tempP, gapWidth))
                {
                    // 只要找到任意一条符合条件的，就说明它不是孤立的
                    return false;
                }
            }

            // === 如果第一轮没找到，反转方向进行第二轮检查 ===
            startLine.Reverse();

            foreach (var p in Lines)
            {
                if (p.Used) continue;

                var tempP = p; // 【关键】临时变量

                if (isAllowedDistace(ref startLine, ref tempP, gapWidth))
                {
                    return false;
                }
            }

            // 两轮都没找到连接的线，说明是孤立的
            return true;
        }

        /*        private bool isIsolatedLine(LineClass startLine)
                {
                    var foundLines = (from p in Lines
                                      where !p.Used && isAllowedDistace(ref startLine, ref  p, gapWidth)
                                      select p).ToList<LineClass>();

                    if (foundLines.Count != 0)
                        return false;
                    else
                    {
                        startLine.Reverse();
                        foundLines = (from p in Lines
                                      where !p.Used && isAllowedDistace(ref startLine, ref p, gapWidth)
                                      select p).ToList<LineClass>();

                        if (foundLines.Count != 0)
                            return false;
                        return true;
                    }
                }*/


        #endregion
    }
}
