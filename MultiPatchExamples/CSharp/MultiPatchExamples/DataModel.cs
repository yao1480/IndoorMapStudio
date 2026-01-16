using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using MultiPatchExamples.GeometryFactory;

namespace DataModels
{

    public abstract class StairParameters
    {
        public static IColor Color_RestPlatform = ColorCreater.Get_RgbColor(185, 122, 87);
        public static IColor Color_StairFlight = ColorCreater.Get_RgbColor(185, 122, 87);
        public static IColor Color_VerticalColumns = ColorCreater.Get_RgbColor(128, 128, 128);
        public static IColor Color_Handrail = ColorCreater.Get_RgbColor(128, 0, 0);

        //休息平台纵截面参数
        public struct RestPlatform
        {
            public double Factor;
            public double Width;
            public double Depth;
            public double Thickness;

            public Beam Beam; //楼板下方横梁参数

            public IColor RenderColor;//采用的渲染颜色

            public RestPlatform(double factor, double width = 2310, double depth = 1200, double thickness = 150)
            {
                this.Factor = factor;

                this.Width = Factor * width;
                this.Thickness = Factor * thickness;
                this.Depth = Factor * depth;

                this.Beam = new StairParameters.Beam(Factor, 208, 90, width);


                this.RenderColor = Color_RestPlatform;
            }

            public RestPlatform(ref Beam beam, double depth, double thickness)
            {
                this.Factor = beam.Factor;

                Beam = beam;

                this.Width = beam.Length;
                this.Depth = beam.Factor * depth;
                this.Thickness = beam.Factor * thickness;

                this.RenderColor = Color_RestPlatform;
            }
        }

        //单行楼梯段纵截面参数
        public struct StairFlight
        {
            public double Factor;

            public IColor RenderColor;//采用的渲染颜色

            public bool IsLeft_downStair;
            public double FlightWidth;
            public double StairwellWidth;
            public int NumOfSteps;
            public double StepWidth;
            public double StepHeight;
            public double DownLineOffset;

            public StairFlight(double factor, bool isLeft_downStair, double flightWidth = 1100, int numOfSteps = 8, double stepWidth = 260, double stepHeight = 156, double stairwellWidth = 110, double downLineOffset = 62)
            {
                this.Factor = factor;

                this.IsLeft_downStair = isLeft_downStair;
                this.NumOfSteps = numOfSteps;
                this.StepWidth = Factor * stepWidth;
                this.StepHeight = Factor * stepHeight;
                this.FlightWidth = Factor * flightWidth;
                this.StairwellWidth = Factor * stairwellWidth;
                this.DownLineOffset = Factor * downLineOffset;

                this.RenderColor = Color_StairFlight;
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
        public struct Baluster
        {
            public double Factor;

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


            public IColor RenderColor;//采用的渲染颜色

            public Baluster(ref RestPlatform restPlatform, ref StairFlight stairFlight, double radius = 15, double height = 1050, double distanceToStairwell = 45)
            {
                if (restPlatform.Factor != stairFlight.Factor)
                    throw new ArgumentException("休息平台 与 楼梯段 缩放因子不一致！");

                this.Factor = stairFlight.Factor;

                this.Radius = Factor * radius;
                this.Height = Factor * height;
                this.GapWidth = stairFlight.StepWidth / 2;
                this.DistanceToStairwell = Factor * distanceToStairwell;

                this.RestPlatform = restPlatform;
                this.StairFlight = stairFlight;

                this.RenderColor = Color_VerticalColumns;
            }
        }

        //扶手参数
        public struct HandRail
        {
            IVector3D extrudeVector_1;
            IVector3D extrudeVector_2;
            IVector3D extrudeVector_3;
            Baluster baluster;

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

            public IColor RenderColor;//采用的渲染颜色


            public HandRail(ref Baluster baluster, double width = 60, double thickness = 80)
            {
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


                this.RenderColor = Color_Handrail;
            }

        }


        //与楼梯关联的横梁参数
        public struct Beam
        {
            public double Factor;

            public double Width;
            public double Height;
            public double Length;


            public Beam(double factor, double width, double height, double length)
            {
                this.Factor = factor;

                this.Width = Factor * width;
                this.Height = Factor * height;
                this.Length = Factor * length;
            }
        }
    }


    public abstract class ElevatorParameters
    {

    }

    public abstract class DoorParameters
    {
        public static double Factor = 0.01;//基于毫米的缩放因子
        public static IColor Color_Door = ColorCreater.Get_RgbColor(98, 0, 0);//门框颜色

        public class SingleDoorParameters
        {

            public double HoleWidth;
            public double WallWidth;
            public double HoleHeight;
            public double BordureWidth;//门柱包边宽
            public double BordureThickness;//门柱包边厚

            public SingleDoorParameters(double holeWidth=860, double wallWidth=240, double holeHeight=2050, double bordureWidth=70, double bordureThickness=10)
            {
                this.HoleWidth = Factor * holeWidth;
                this.WallWidth = Factor * wallWidth;
                this.HoleHeight = Factor * holeHeight;

                this.BordureWidth = Factor * bordureWidth;
                this.BordureThickness = Factor * bordureThickness;


            }
        }
    }
}
