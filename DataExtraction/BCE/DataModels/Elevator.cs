using BCE.DataModels.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExtension.Geometry;
using DXFReader.DataModel;

namespace BCE.DataModels
{
    public class Elevator
    {

        #region 从图纸提取的参数
        public ElevatorType Type { get; set; }
        public Line DoorLine { get; set; }
        public double Thickness_Door { get; set; }
        public double Width_Door
        {
            get {
                if (DoorLine == null)
                    return double.NaN;
                return DoorLine.Length;
            }
        }

        public double Width { get; set; }
        public double Depth { get; set; }
        public double Height { get; set; }

        public Point InsertPoint { get; set; }
        public double RotateAngle { get; set; }


        #endregion

        #region 外部指定的参数
        public double StartHeigth { get; set; }
        public double Height_Door { get; set; }
        #endregion


    }
}
