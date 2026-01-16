using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DXFReader.DataModel
{
    public class LwPolyLineClass 
    {
        List<Point> pointCollection = null;
        List<LineClass> lineCollection = null;

        public int CloseFlag { get; set; }
        public List<Point> PointCollection
        {
            get { return pointCollection; }

            set { pointCollection = value; }
        }
        public List<LineClass> LineCollection
        {
            get
            {
                lineCollection.Clear();

                for (int i = 0; i < PointCollection.Count - 1; i++)
                {
                    LineClass pLine = new LineClass();
                    pLine.StartPoint = PointCollection[i];
                    pLine.EndPoint = PointCollection[i + 1];
                    pLine.LayerName = LayerName;
                    lineCollection.Add(pLine);
                }

                if (CloseFlag == 1)
                {
                    LineClass pLine = new LineClass();
                    pLine.StartPoint = PointCollection[PointCollection.Count - 1];
                    pLine.EndPoint = PointCollection[0];
                    pLine.LayerName = LayerName;
                    lineCollection.Add(pLine);
                }

                return lineCollection;
            }

            set { lineCollection = value; }
        }
        public string LayerName { get; set; }
    

        #region 构造器
        public LwPolyLineClass()
        {
            lineCollection = new List<LineClass>();
            pointCollection = new List<Point>();
        }

        #endregion














    }
}
