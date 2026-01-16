using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExtension.Geometry;


namespace DXFReader.DataModel
{
    public class EntityClass
    {
        List<LineClass> lines;
        List<LwPolyLineClass> lwPolylines;
        List<CircleClass> circles;
        List<ArcClass> arcs;
        List<EllipseClass> ellipses;

        public List<LineClass> Lines
        {
            get
            {
                return lines;
            }
            set
            {
                lines = value;
            }
        }
        public List<LwPolyLineClass> LwPolylines
        {
            get
            {
                return lwPolylines;
            }
            set
            {
                lwPolylines = value;
            }
        }
        public List<CircleClass> Circles
        {
            get
            {
                return circles;
            }
            set
            {
                circles = value;
            }
        }
        public List<ArcClass> Arcs
        {
            get
            {
                return arcs;
            }
            set
            {
                arcs = value;
            }
        }
        public List<EllipseClass> Ellipses
        {
            get
            {
                return ellipses;
            }
            set
            {
                ellipses = value;
            }
        }

        #region 构造器
        public EntityClass()
        {
            lines = new List<LineClass>();
            lwPolylines = new List<LwPolyLineClass>();
            circles = new List<CircleClass>();
            arcs = new List<ArcClass>();
            ellipses = new List<EllipseClass>();
        }
        #endregion




        #region 方法
        public void MergeLines()
        {
            //遍历多段线来收集多段线中的直线集
            foreach (var lwLine in this.LwPolylines)
            {
                lines.AddRange(lwLine.LineCollection);
            }
        }
        #endregion



    }
}
