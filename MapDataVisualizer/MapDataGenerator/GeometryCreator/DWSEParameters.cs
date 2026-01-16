using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcObjectsUtilities;
using BCE.DataModels.Interface;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace EsriMapDataGenerator.GeometryCreator
{

    #region Interfaces& BasicClasses
    public interface IRenderedColor
    {
        IColor Color { get; set; }
    }

    public interface IUnit
    {
        double Factor { get; set; }
    }

    public interface IWidthDepthThickness
    {
        double Width { get; set; }//宽度
        double Depth { get; set; }//深度
        double Thickness { get; set; }//厚度
    }

    public abstract class BasicDoorWindowParameters
    {
        public double Width;
        public double Thickness;
        public double Height;
        public double FrameThickness;
        public double FrameWidth;
    }
    #endregion

    #region Parameters
    public abstract class DoorParameters
    {
        public static double Factor = 1d;//基于毫米的缩放因子
        public static IColor Color_Door = ColorUtility.Get_RgbColor(98, 0, 0);//门框颜色


        /// <summary>
        /// 单叶门参数
        /// </summary>
        public class P_SingleDoor : BasicDoorWindowParameters
        {
            public bool IsClockwiseOpen;

            public P_SingleDoor(bool isClockwiseOpen = false, double doorWidth = 900d, double doorThickness = 50d, double doorHeight = 2100d, double frameThickness = 20, double frameWidth = 100d)
            {
                IsClockwiseOpen = isClockwiseOpen;
                Width = Factor * doorWidth;
                Height = Factor * doorHeight;
                Thickness = Factor * doorThickness;
                FrameWidth = Factor * frameWidth;
                FrameThickness = Factor * frameThickness;
            }

        }

        /// <summary>
        /// 双页门或子母门参数
        /// </summary>
        public class P_DMDoor : BasicDoorWindowParameters
        {
            public bool IsClockwiseOpen;
            public double Rate = 0.5d;//较大的门宽与总的真实门宽(扣除两侧门框厚度)之比

            public P_DMDoor(bool isClockwiseOpen = false, double rate = 0.5d, double doorWidth = 1800d, double doorThickness = 50d, double doorHeight = 2100d, double frameThickness = 20, double frameWidth = 100d)
            {
                IsClockwiseOpen = isClockwiseOpen;
                Rate = rate;

                Width = Factor * doorWidth;
                Height = Factor * doorHeight;
                Thickness = Factor * doorThickness;

                FrameWidth = Factor * frameWidth;
                FrameThickness = Factor * frameThickness;
            }
        }

        public class P_EDoor:BasicDoorWindowParameters
        {
            public double DoorHoleDepth = 0d;

            public P_EDoor(double doorHoleDepth, double width=450d,double thickness=50d,double height=2100d)
            {
                DoorHoleDepth = doorHoleDepth;

                base.Width=width;
                base.Thickness=thickness;
                base.Height=height;
                base.FrameThickness=0d;
                base.FrameWidth=0d;
            }
        }

        public class P_SlidingDoor : BasicDoorWindowParameters
        {
            public double DoorSideWidth;//门边宽度
            public double DoorThickness;//门厚度
            public double GrassThickness;//玻璃厚度

            public P_SlidingDoor(double doorHoleWidth=1200d,double doorHoleDepth=240d, double doorHoleHeight=2100, double frameThickness=20d,double framWidth=200d, double doorSideWidth=100d,double doorThicknes=50d, double grassThickness=4d)
            {
                base.Width = doorHoleWidth;
                base.Thickness = doorHoleDepth;
                base.Height = doorHoleHeight;

                base.FrameThickness = frameThickness;
                base.FrameWidth = framWidth;

                this.DoorSideWidth = doorSideWidth;
                this.DoorThickness = doorThicknes;
                this.GrassThickness = grassThickness;
            }
        }
    }

    public abstract class WindowParameters
    {
        public class P_GeneralWindow : BasicDoorWindowParameters
        {
            public double StartHeight;//窗底部至楼板的距离

            public P_GeneralWindow(double startHeight = 900d, double width = 1000d, double height = 1500d, double frameWidth = 100d, double frameThickness = 20d)
            {
                StartHeight = startHeight;

                base.Width = width;
                base.Height = height;
                base.FrameWidth = frameWidth;
                base.FrameThickness = frameThickness;
               
                //base.Width 窗户宽度由窗框宽度决定，基于图纸提取的宽度为窗洞宽度，除了在提取阶段用于计算定位点外，对建模无用
            }

        }
    }

    public abstract class StairParameters
    {
        public static IColor Color_Beam = ColorUtility.Get_RgbColor(185, 122, 87);
        public static IColor Color_RestPlatform = ColorUtility.Get_RgbColor(185, 122, 87);
        public static IColor Color_StairFlight = ColorUtility.Get_RgbColor(185, 122, 87);
        public static IColor Color_VerticalColumns = ColorUtility.Get_RgbColor(128, 128, 128);
        public static IColor Color_Handrail = ColorUtility.Get_RgbColor(128, 0, 0);


        //与楼梯关联的横梁参数
        public struct Beam : IUnit, ILengthWidthHeight, IRenderedColor
        {
            #region 私有字段
            double factor;
            double length;
            double width;
            double height;
            IColor color;
            #endregion


            public double Factor
            {
                get { return factor; }
                set { factor = value; }
            }
            public double Length
            {
                get { return length; }
                set { length = value; }
            }
            public double Width
            {
                get { return width; }
                set { width = value; }
            }
            public double Height
            {
                get { return height; }
                set { height = value; }
            }
            public IColor Color
            {
                get { return color; }
                set { color = value; }
            }


            public Beam(double factor, double width, double height, double length)
            {
                this.factor = factor;
                this.length = factor * length;
                this.width = factor * width;
                this.height = factor * height;

                //默认设置
                this.color = Color_Beam;
            }
        }

        //休息平台纵截面参数
        public struct RestPlatform : IUnit, IWidthDepthThickness, IRenderedColor
        {
            #region 私有字段
            double factor;
            double width;
            double depth;
            double thickness;
            IColor color;
            Beam restPlatform_Beam;
            #endregion

            public double Factor
            {
                get { return factor; }
                set { factor = value; }
            }

            public double Width
            {
                get { return width; }
                set { width = value; }
            }

            public double Depth
            {
                get { return depth; }
                set { depth = value; }
            }

            public double Thickness
            {
                get { return thickness; }
                set { thickness = value; }
            }
            public IColor Color
            {
                get { return color; }
                set { color = value; }
            }

            public Beam RestPlatform_Beam
            {
                get { return restPlatform_Beam; }
                set { restPlatform_Beam = value; }
            }

            public RestPlatform(double factor, double width = 2310, double depth = 1200, double thickness = 150)
            {
                this.factor = factor;


                this.width = factor * width;
                this.thickness = factor * thickness;
                this.depth = factor * depth;

                //默认设置
                this.restPlatform_Beam = new Beam(factor, 208, 90, width);
                this.color = Color_RestPlatform;

            }

            public RestPlatform(ref Beam beam, double depth, double thickness)
            {
                this.factor = beam.Factor;
                this.restPlatform_Beam = beam;

                this.width = beam.Length;
                this.depth = beam.Factor * depth;
                this.thickness = beam.Factor * thickness;

                //默认设置
                this.color = Color_RestPlatform;
            }
        }

        //单行楼梯段纵截面参数
        public struct StairFlight : IUnit, IRenderedColor
        {
            #region 私有字段
            double factor;
            IColor color;
            #endregion

            public double Factor
            {
                get { return factor; }
                set { factor = value; }
            }
            public IColor Color
            {
                get { return color; }
                set { color = value; }
            }

            public bool IsLeft_downStair;
            public double FlightWidth;
            public double StairwellWidth;
            public int NumOfSteps;
            public double StepWidth;
            public double StepHeight;
            public double DownLineOffset;





            public StairFlight(double factor, bool isLeft_downStair = true, double flightWidth = 1100, int numOfSteps = 8, double stepWidth = 260, double stepHeight = 156, double stairwellWidth = 110, double downLineOffset = 62)
            {
                this.factor = factor;

                this.IsLeft_downStair = isLeft_downStair;
                this.FlightWidth = factor * flightWidth;
                this.StairwellWidth = factor * stairwellWidth;
                this.NumOfSteps = numOfSteps;
                this.StepWidth = factor * stepWidth;
                this.StepHeight = factor * stepHeight;
                this.DownLineOffset = factor * downLineOffset;

                //默认颜色
                this.color = Color_StairFlight;

            }

        }

        //栏杆参数
        public struct StairRailing
        {
            public Baluster Baluster;
            public HandRail Handrail;

            public StairRailing(ref Baluster baluster, ref HandRail handrail)
            {
                this.Baluster = baluster;
                this.Handrail = handrail;
            }
        }

        //栏杆立柱参数
        public struct Baluster : IUnit, IRenderedColor
        {
            #region 私有字段
            double factor;
            IColor color;
            #endregion

            public double Factor
            {
                get { return factor; }
                set { factor = value; }
            }
            public IColor Color
            {
                get { return color; }
                set { color = value; }
            }

            //垂直圆柱杆件底面圆半径
            public double Radius;
            //垂直杆件高度
            public double Height;
            //垂直杆件间距
            public double GapWidth;
            //垂直杆件道楼梯井的水平距离
            public double DistanceToStairwell;

            public StairFlight StairFlight;
            public RestPlatform RestPlatform;

            public Baluster(ref RestPlatform restPlatform, ref StairFlight stairFlight, double radius = 15, double height = 1050, double distanceToStairwell = 45)
            {
                if (restPlatform.Factor != stairFlight.Factor)
                    throw new ArgumentException("休息平台 与 楼梯段 缩放因子不一致！");

                this.factor = stairFlight.Factor;

                this.Radius = factor * radius;
                this.Height = factor * height;
                this.GapWidth = stairFlight.StepWidth / 2;
                this.DistanceToStairwell = factor * distanceToStairwell;

                this.RestPlatform = restPlatform;
                this.StairFlight = stairFlight;

                this.color = Color_VerticalColumns;
            }
        }

        //扶手参数
        public struct HandRail : IUnit, IRenderedColor
        {
            #region 私有字段
            double factor;
            IColor color;


            IVector3D extrudeVector_1;
            IVector3D extrudeVector_2;
            IVector3D extrudeVector_3;
            Baluster baluster;
            #endregion
            public double Factor
            {
                get { return factor; }
                set
                {
                    factor = value;
                }
            }

            public IColor Color
            {
                get { return color; }
                set { color = value; }
            }


            public double Width;
            public double Thickness;



            public IVector3D ExtrudeVector_1
            {
                get { return extrudeVector_1; }
            }


            public IVector3D ExtrudeVector_2
            {
                get { return extrudeVector_2; }
            }

            public IVector3D ExtrudeVector_3
            {
                get { return extrudeVector_3; }
            }

            public Baluster Baluster
            {
                get { return baluster; }
            }




            public HandRail(ref Baluster baluster, double width = 60, double thickness = 80)
            {
                this.factor = baluster.Factor;

                this.baluster = baluster;
                this.Width = baluster.Factor * width;
                this.Thickness = baluster.Factor * thickness;

                //计算上行梯段扶手从上之下的拉伸矢量
                double x = (baluster.StairFlight.NumOfSteps + 0.5) * baluster.StairFlight.StepWidth;
                double z = -(baluster.StairFlight.NumOfSteps + 0.5) * baluster.StairFlight.StepHeight;

                extrudeVector_1 = new Vector3DClass();
                extrudeVector_1.SetComponents(x, 0, z);


                extrudeVector_2 = new Vector3DClass();
                extrudeVector_2.SetComponents(
                    0,
                    (baluster.StairFlight.StairwellWidth + 2 * baluster.DistanceToStairwell) / 2,
                    baluster.StairFlight.StepHeight / 4//休息平台上栏杆高度相差1/2台阶高
                    );


                extrudeVector_3 = new Vector3DClass();
                extrudeVector_3.SetComponents(
                     0,
                   (baluster.StairFlight.StairwellWidth + 2 * baluster.DistanceToStairwell) / 2,
                    -baluster.StairFlight.StepHeight / 4
          );

                //默认颜色
                this.color = Color_Handrail;
            }
        }


        //双跑楼梯参数汇总
        public struct DStairParameter
        {
            public StairFlight Fligth;
            public RestPlatform Platform;
            public StairRailing Railing;

            public DStairParameter(RestPlatform platform, StairFlight fligth, StairRailing railing)
            {
                Fligth = fligth;
                Platform = platform;
                Railing = railing;
            }
        }
    }

    #endregion
}
