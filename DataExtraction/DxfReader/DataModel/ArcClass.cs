using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExtension.Geometry;

namespace DXFReader.DataModel
{
    public class ArcClass : Arc
    {
        Point startPoint;
        Point endPoint;
        int indexInSourceCollection = -1;
        bool used = false;


        public Point StartPoint
        {
            get
            {
                if (startPoint == null)
                {
                    startPoint = new Point();
                    startPoint.X = Centre.X + Radius * Math.Cos(StartRadian);
                    startPoint.Y = Centre.Y + Radius * Math.Sin(StartRadian);

                }
                return startPoint;
            }
        }
        public Point EndPoint
        {
            get
            {
                if (endPoint == null)
                {
                    endPoint = new Point();
                    endPoint.X = Centre.X + Radius * Math.Cos(EndRadian);
                    endPoint.Y = Centre.Y + Radius * Math.Sin(EndRadian);
                }
                return endPoint;
            }
        }
        public bool Used
        {
            get { return used; }
            set { used = value; }
        }
        public string LayerName { get; set; }
        public int IndexInSourceCollection//在源集合中的索引号
        {
            get
            {
                return indexInSourceCollection;
            }
            set
            {
                indexInSourceCollection = value;
            }
        }
        public GpType GPT { get; set; }


        #region 构造器
        public ArcClass()
        {
        }

        public ArcClass(Point centre, double radius, double startAngle, double endAngle, string layerName = null)
        {
            base.Centre = centre;
            base.Radius = radius;
            base.StartRadian = startAngle;
            base.EndRadian = endAngle;
            this.LayerName = layerName;
        }
        #endregion


        #region 方法
        //交换起终点坐标,但是起终弧度并没有改变
        public void Reverse()
        {
            Point pPoint = this.startPoint;
            this.startPoint = this.endPoint;
            this.endPoint = pPoint;
        }

        public LineClass ToLineClass(int indexInSourceCollection)
        {
            LineClass lineClass = new LineClass(this.StartPoint,this.EndPoint);
            lineClass.IndexInSourceCollection = indexInSourceCollection;
            lineClass.GPT = GpType.Arc;
            return lineClass;
        }
        #endregion






















    }
}
