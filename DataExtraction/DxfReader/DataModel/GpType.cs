using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace DXFReader
{
    public enum GpType
    {
        UnKnow = 0,
        Line = 1,
        Arc = 2,
        Circle = 3,
        Ellipse = 4
    }

    //public enum LineType
    //{
    //    UnKnow = 0,
    //    WallLine=1,
    //    DoorLine=2,
    //    WindowLine=3,
    //    Elevator=4,
    //    Stairs=5
    //}

    //描述块的走向以及块实际意义上的宽度
    public class DLandWidth
    {
        public Point BasePoint = new Point(0f,0f);
        public LineClass DirectionLine = new LineClass();
        public LineClass WidthLine = new LineClass();
        public int ArcCount = 0;//记录图块涉及到的圆弧个数，可以简单区分门的类型（单扇、双扇、滑动门）


        public DLandWidth Copy()
        {
            DLandWidth dlw = new DLandWidth();
            dlw.BasePoint = new Point(this.BasePoint.X,this.BasePoint.Y,this.BasePoint.Z);
            dlw.DirectionLine = new LineClass(this.DirectionLine.StartPoint,this.DirectionLine.EndPoint);
            dlw.WidthLine = new LineClass(this.WidthLine.StartPoint,this.WidthLine.EndPoint);
            dlw.ArcCount = this.ArcCount;

            return dlw;
        }
        //public double Width = 0;
    }
}
