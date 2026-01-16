using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExtension.Geometry;


namespace DXFReader.DataModel
{
    public class SemanticPolygon
    {
        #region 私有变量
        List<LineClass> lines;
        List<Point> points;
        PolygonType pType = PolygonType.Unknown;

        Point functionRegionPoint = null;
        List<Point> doorPoints = null;
        #endregion

        #region 属性
        /// <summary>
        /// 边集
        /// </summary>
        public List<LineClass> Lines
        {
            get { return lines; }
            set { lines = value; }
        }

        /// <summary>
        /// 顶点集(ERing算法可能并没有理顺边,但在运行点集获取代码时能够保证所有边都是S-E理顺状态,点集也是唯一的)
        /// </summary>
        public List<Point> Points
        {
            get
            {
                if (points == null)
                {
                    //构造点集

                    if (lines == null || lines.Count < 3)
                        throw new ArgumentException("语义多边形边不足 3 条 ！");
                    points = new List<Point>();

                    #region 将多边形点梳理顺畅
                    //先取前两条线段确定点采集方向（始终采集终点）
                    if (lines[1].StartPoint.Equals(lines[0].StartPoint) ||
                        lines[1].EndPoint.Equals(lines[0].StartPoint))
                    {
                        lines[0].Reverse();
                    }

                    points.Add(lines[0].StartPoint);
                    points.Add(lines[0].EndPoint);

                    //理顺后续线段,始终采集后续线段的终点
                    for (int i = 1; i < lines.Count; i++)
                    {
                        if (lines[i].EndPoint.Equals(lines[i - 1].EndPoint))
                            lines[i].Reverse();

                        points.Add(lines[i].EndPoint);
                    }

                    //如果最后一点与起始点重复, 则移除
                    if (points[points.Count - 1].Equals(points[0]))
                        points.RemoveAt(points.Count - 1);
                    #endregion
                }

                return points;
            }
            set { points = value; }
        }

        /// <summary>
        /// 功能区抽象点（Z=0）
        /// </summary>
        public Point FunctionRegionPoint
        {
            get
            {
                if (functionRegionPoint == null)
                {
                    functionRegionPoint = MathExtension.Geometry.GeometryHelper.Calc_PointInPolygon(this.Points);
                    functionRegionPoint.Z = 0d;//Z坐标全部置为0
                }

                return functionRegionPoint;
            }
        }

        /// <summary>
        /// 门线中点集
        /// </summary>
        public List<Point> DoorPoints
        {
            get
            {
                if (doorPoints == null)
                {
                    doorPoints = new List<Point>();

                    List<LineClass> doorLines = (from p in Lines
                                                 where p.LType == LineType.DoorLine
                                                 select p).ToList<LineClass>();

                    foreach (var item in doorLines)
                    {
                        doorPoints.Add(item.Get_MiddlePoint());
                    }
                }
                return doorPoints;
            }
        }

        /// <summary>
        /// 面积
        /// </summary>
        public double Area
        {
            get { return GeometryHelper.Calc_Area_2D((Polygon)this); }
        }

        /// <summary>
        /// 周长
        /// </summary>
        public double Circumference
        {
            get { return GeometryHelper.Calc_Circumference_2D((Polygon)this); }
        }

        /// <summary>
        /// 功能区类型
        /// </summary>
        public PolygonType PType
        {
            get { return pType; }
            set { pType = value; }
        }
        #endregion


        #region 构造器
        public SemanticPolygon()
        {
            lines = new List<LineClass>();
        }
        public SemanticPolygon(List<LineClass> lines)
        {
            //要求给出闭合环线条
            if (lines.Count < 3)
                throw new ArgumentException("线段数目小于 3，无法构造多边形 ！");
            else
                //线集赋值
                this.lines = lines;

        }


        public SemanticPolygon(List<Point> points)
        {
            if (points.Count < 3)
                throw new ArgumentException("点数小于 3，无法构造多边形 ！");
            else
            {
                //点集赋值
                this.points = points;

                //构造线集
                this.lines = new List<LineClass>();
                for (int i = 0; i < points.Count - 1; i++)
                {
                    this.lines.Add(new LineClass(points[i], points[i + 1]));
                }
                this.lines.Add(new LineClass(points[points.Count - 1], points[0]));
            }
        }
        #endregion



        public static explicit operator Polygon(SemanticPolygon pc)
        {
            Polygon p = new Polygon();
            for (int i = 0; i < pc.Lines.Count; i++)
            {
                p.Lines.Add(pc.Lines[i]);
            }
            return p;
        }

        //public bool Equals(object obj)
        //{
        //    SemanticPolygon sp = obj as SemanticPolygon;

        //    if (sp.lines.Count != this.Lines.Count
        //        || sp.points.Count != this.Points.Count
        //        || Math.Abs(sp.Circumference - this.Circumference) > 1
        //        || Math.Abs(sp.Area - this.Area) > 1)

        //        return false;
        //    return true;

        //}
    }
}
