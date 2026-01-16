using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExtension.Vector;
using MathExtension.Geometry;

namespace DXFReader.DataModel
{
    public class LineClass : MathExtension.Geometry.Line
    {
        bool used = false;
        bool shouldMoved = false;
        LineType lineType = LineType.Unknown;



        public bool Used
        {
            get
            {
                return used;
            }
            set
            {
                used = value;
            }
        } //标识此线是否已经被使用过
        public bool ShouldMoved
        {
            get { return shouldMoved; }
            set { shouldMoved = value; }
        }//标识此线是否应该被舍弃
        public LineType LType 
        {
            get { return lineType; }
            set { lineType = value; }
        }//标识此线段的来源类型
        public string LayerName { get; set; }



        int lastLgpIndex = -1;
        int nextLgpIndex = -1;
        int indexInSourceCollection = -1;
        public int LastLgpIndex
        {
            get
            {
                return lastLgpIndex;
            }
            set
            {
                lastLgpIndex = value;
            }
        }
        public int NextLgpIndex
        {
            get
            {
                return nextLgpIndex;
            }
            set
            {
                nextLgpIndex = value;
            }
        }
        public GpType GPT { get; set; }
        //public LineType BCT { get; set; }
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


        #region 构造器
        public LineClass()
        {
            base.StartPoint = new Point();
            base.EndPoint = new Point();
        }

        public LineClass(Point startPoint, Point endPoint,  string layerName = null,LineType lineType=LineType.Unknown)
        {
            base.StartPoint = startPoint;
            base.EndPoint = endPoint;
            this.LayerName = layerName;
            LType = lineType;
        }

        #endregion


  
 













    }
}
