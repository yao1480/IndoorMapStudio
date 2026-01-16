using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DXFReader.DataModel
{
    public class InsertClass 
    {
        Point point = null;
        //因以下四个参数均为可选的，故设置其默认值
        double scaleX = 1;
        double scaleY = 1;
        double scaleZ = 1;
        double rotation = 0;

        public string LayerName { get; set; }
        public string BlockName { get; set; }
        public Point InsertPoint
        {
            get
            {
                return point;
            }
            set
            {
                point = value;
            }
        }
        public double ScaleX
        {
            get
            {
                return scaleX;
            }
            set
            {
                scaleX = value;
            }
        }
        public double ScaleY
        {
            get
            {
                return scaleY;
            }
            set
            {
                scaleY = value;
            }
        }
        public double ScaleZ
        {
            get
            {
                return scaleZ;
            }
            set
            {
                scaleZ = value;
            }
        }
        public double RotationAngle
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }



        public InsertClass()
        {
            point = new Point();
        }


    }
}
