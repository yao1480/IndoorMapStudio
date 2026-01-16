using BCE.DataModels.Basic;
using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;

namespace BCE.DataModels
{
    public class Stair
    {
        #region 从图纸提取的参数
        //踏步数+踏步尺寸
        public double StepNum { get; set; }//不含休息平台
        public double Width_Step { get; set; }
        public double Height_Step
        {
            get
            {
                if (Height_Floor <= 0d)
                    return double.NaN;

                return Height_Floor / (StepNum + 2);
            }
        }

        //休息平台+梯间+梯段+井宽+上楼位置
        public double Width_Land { get; set; }
        public double Width_Staircase { get; set; }
        public double Width_Stairway { get; set; }
        public double Width_Stairwell { get; set; }
        public UpstairPosition UpstairPosition { get; set; }


        //定位参数
        public Point InsertPoint { get; set; }
        public double RotateAngle { get; set; }
        #endregion


        #region 外部指定参数
        //楼层高(净高+楼板厚)
        public double Height_Floor { get; set; }

        //扶手参数
        public double Height_Handrail { get; set; }
        public double Width_Handrail { get; set; }
        public double Thickness_Handrail { get; set; }

        #endregion
    }
}
