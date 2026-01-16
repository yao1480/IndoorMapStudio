using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Windows.Forms;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ArcObjectsUtilities;
using BC_ModelParameters;

namespace MultiPatchExamples.Building3DGenerator
{
    public class DRunningStairGeneraotr
    {
        //构造 3D双跑楼梯
        public static List<IElement> Construct_DRunningStair(StairParameters.RestPlatform restPlatform, StairParameters.StairFlight stairFlight, StairParameters.StairRailing stairRailing)
        {
            List<IElement> elements = new List<IElement>();

            //使休息平台宽为梯段宽之和+梯段缝隙宽
            restPlatform.Width = stairFlight.FlightWidth * 2 + stairFlight.StairwellWidth;

            //1.    构造休息平台
            ITopologicalOperator restPlatform_topologicalOperator = Construct_StairLanding(restPlatform) as ITopologicalOperator;

            //2.    构造梯段
            ITransform3D stairFlight_transform3D = Construct_StairSection(stairFlight) as ITransform3D;

            //3.    构造栏杆(栏杆柱+扶手)
            IGeometry verticalColumns3D = Construct_VerticalColumns(ref stairRailing.Baluster);
            IGeometry handrails3D = Construct_Handrail(ref stairRailing.Handrail);


            //4.    调整构件位置
            (verticalColumns3D as ITransform3D).Move3D(
                 restPlatform.Depth - stairRailing.Baluster.GapWidth / 2,
                 restPlatform.Width / 2,
                 0
                );
            (handrails3D as ITransform3D).Move3D(
                restPlatform.Depth - stairRailing.Baluster.GapWidth / 2,
                restPlatform.Width / 2,
                stairRailing.Baluster.Height);

            stairFlight_transform3D.Move3D(
                restPlatform.Depth,
                0,
                -stairFlight.StepHeight);
    

            //基于Geometry创建Elements并添加进楼梯元素集合
            elements.Add(ElementUtility.Contruct_MultiPatchElement(restPlatform_topologicalOperator as IGeometry, restPlatform.Color));
            elements.Add(ElementUtility.Contruct_MultiPatchElement(stairFlight_transform3D as IGeometry, stairFlight.Color));
            elements.Add(ElementUtility.Contruct_MultiPatchElement(verticalColumns3D, stairRailing.Baluster.Color));
            elements.Add(ElementUtility.Contruct_MultiPatchElement(handrails3D, stairRailing.Handrail.Color));

            return elements;
        }

        #region 内部方法
        ////构造 3D休息平台
        private static IGeometry Construct_StairLanding(StairParameters.RestPlatform restPlatform)
        {
            /* -------------------------------------------
             * 以休息平台 左上角顶点 为OCS原点
             * -------------------------------------------*/

            //1.    构造外层矩形
            PolygonClass externalPolygon = GeometryUtility.Construct_Rectangle_XYP(
                GeometryUtility.Construct_Point3D(restPlatform.Depth / 2, (restPlatform.Thickness + restPlatform.RestPlatform_Beam.Height) / 2, 0),
                restPlatform.Depth,
                restPlatform.Thickness + restPlatform.RestPlatform_Beam.Height,
                0);

            //2.     构造外层矩形
            PolygonClass internalPolygon = GeometryUtility.Construct_Rectangle_XYP(
                GeometryUtility.Construct_Point3D(restPlatform.Depth / 2, restPlatform.RestPlatform_Beam.Height / 2, 0),
                restPlatform.Depth - 2 * restPlatform.RestPlatform_Beam.Width,
                restPlatform.RestPlatform_Beam.Height,
                0);

            //3.    externalPolygon-internalPolygon
            IGeometry diff = externalPolygon.Difference(internalPolygon);

            //4.    拉伸截面
            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            (pConstructMultipatch as IZAware).ZAware = true;
            pConstructMultipatch.ConstructExtrudeFromTo(0, -restPlatform.Width, diff);

            //5.    将截面围绕 X轴 旋转到XZ平面
            IVector3D rotateAxis = GeometryUtility.Construct_Vector3D(1, 0, 0);
            double rotateAngle = Math.PI / 2;
            ITransform3D transform3D = pConstructMultipatch as ITransform3D;
            transform3D.RotateVector3D(rotateAxis, rotateAngle);

            //6.    将休息平台下移使得休息平台左上角顶点 位于 OCS原点
            transform3D.Move3D(0, 0, -(restPlatform.Thickness + restPlatform.RestPlatform_Beam.Height));

            return pConstructMultipatch as IGeometry;
        }

        //构造 3D梯段
        private static IGeometry Construct_StairSection(StairParameters.StairFlight stairFlight)
        {
            /* -------------------------------------------
             * 不论上行梯段在左或在右， 
             * 梯段组合体的 左上角顶点 始终位于OCS的Z轴上，
             * OCS的XY平面始终与上行梯段顶层台阶面平齐
             *--------------------------------------------*/


            #region 上行梯段
            IPointCollection pPointCollection = new PolygonClass();
            (pPointCollection as IZAware).ZAware = true;


            //1.    创建台阶角点
            for (int i = 0; i < stairFlight.NumOfSteps; i++)
            {

                pPointCollection.AddPoint(GeometryUtility.Construct_Point3D(
                    stairFlight.StepWidth * i,
                    -stairFlight.StepHeight * i,
                    0));

                pPointCollection.AddPoint(GeometryUtility.Construct_Point3D(
                   stairFlight.StepWidth * (i + 1),
                   -stairFlight.StepHeight * i,
                   0));
            }
            //2.    创建梯段下方线段端点
            //计算Y轴偏移量
            //----计算楼梯段坡度角的余弦值
            double cosValue = stairFlight.StepWidth / Math.Sqrt(Math.Pow(stairFlight.StepWidth, 2) + Math.Pow(stairFlight.StepHeight, 2));
            double offsetY = stairFlight.DownLineOffset / cosValue;

            //计算线段端点
            IPoint p1 = pPointCollection.get_Point(pPointCollection.PointCount - 1);
            pPointCollection.AddPoint(GeometryUtility.Construct_Point3D(p1.X, p1.Y - (stairFlight.StepHeight + offsetY), 0));

            IPoint p2 = pPointCollection.get_Point(0);
            pPointCollection.AddPoint(GeometryUtility.Construct_Point3D(p2.X, p2.Y - offsetY, 0));

            //使多边形闭合
            IPolygon pPolygon = pPointCollection as IPolygon;
            pPolygon.Close();

            //创建台阶实体
            IConstructMultiPatch pMultipatch = new MultiPatchClass();
            (pMultipatch as IZAware).ZAware = true;
            pMultipatch.ConstructExtrudeFromTo(0, -stairFlight.FlightWidth, pPointCollection as IGeometry);

            ITransform3D transform3D = pMultipatch as ITransform3D;

            IVector3D rotateAxis = GeometryUtility.Construct_Vector3D(1, 0, 0);
            double rotateAngle = Math.PI / 2;
            transform3D.RotateVector3D(rotateAxis, rotateAngle);

            #endregion

            #region 下行梯段(基于上行梯段进行3D变换)
            //1.    从上行梯段克隆出下行梯段
            IClone uppeClone = pMultipatch as IClone;
            IClone lowerclone = new MultiPatchClass();
            lowerclone.Assign(uppeClone.Clone());

            //2.    调整上下行梯段位置关系
            double moveX = 0;
            double moveY = 0;
            double moveZ = 0;

            ITransform3D downTransform3D = lowerclone as ITransform3D;
            ITransform3D upTransform3D = null;


            //---旋转上行梯段
            downTransform3D.RotateVector3D(GeometryUtility.Construct_Vector3D(0, 0, 1), Math.PI);

            //---移动上下行梯段
            if (stairFlight.IsLeft_downStair)
            {
                //上行梯段在左（只需移动 下行梯段）
                moveX = stairFlight.StepWidth * stairFlight.NumOfSteps;
                moveY = stairFlight.FlightWidth * 2 + stairFlight.StairwellWidth;
                moveZ = stairFlight.StepHeight * (stairFlight.NumOfSteps + 1);
                downTransform3D.Move3D(moveX, moveY, moveZ);
            }
            else
            {
                //上行梯段在右（需移动 上下两个梯段）
                //---移动上行梯段
                upTransform3D = uppeClone as ITransform3D;
                moveX = 0;
                moveY = stairFlight.FlightWidth + stairFlight.StairwellWidth;
                moveZ = 0;
                upTransform3D.Move3D(moveX, moveY, moveZ);

                //---移动下行梯段
                moveX = stairFlight.StepWidth * stairFlight.NumOfSteps;
                moveY = stairFlight.FlightWidth;
                moveZ = stairFlight.StepHeight * (stairFlight.NumOfSteps + 1);
                downTransform3D.Move3D(moveX, moveY, moveZ);
            }


            //3.    合并上下梯段
            ITopologicalOperator lowerTopologicalOperator = downTransform3D as ITopologicalOperator;
            lowerTopologicalOperator.Simplify();
            IGeometry unionGeometry = lowerTopologicalOperator.Union(uppeClone as IGeometry);
            #endregion

            return unionGeometry;
        }

        //构造栏杆
        private static IGeometry Construct_StairRailing(ref StairParameters.StairRailing stairRailing)
        {
            /* -------------------------------------------
            * 不论上行梯段在左或在右， 
            * 始终以休息平台上第一根立柱的 底面圆心 为OCS原点，
            *--------------------------------------------*/
            IGeometry stairRailing3D = null;

            IGeometry verticalColumns3D = Construct_VerticalColumns(ref stairRailing.Baluster);
            IGeometry handrails3D = Construct_Handrail(ref stairRailing.Handrail);

            //组装栏杆和扶手
            (handrails3D as ITransform3D).Move3D(0, 0, stairRailing.Baluster.Height);


            stairRailing3D = (verticalColumns3D as ITopologicalOperator).Union(handrails3D);

            return stairRailing3D;
        }



        // 构造完整垂直圆柱形栏杆柱(以上下立柱在休息平台上的两个立柱顶端圆心连线中点为OCS原点)
        public static IGeometry Construct_VerticalColumns(ref StairParameters.Baluster baluster)
        {
            IGeometry columns;


            //1.    生成上行栏杆立柱原型
            //1.1    构造位于休息平台上的圆柱形垂直栏杆构件
            IPoint pointOnStairLanding = GeometryUtility.Construct_Point3D(0, 0, 0);
            IGeometry firsteometry = GeometryUtility.Construct_Cylinder(pointOnStairLanding, baluster.Radius, baluster.Height, new MathExtension.Vector.Vector(0, 0, 1), 5);
            ITopologicalOperator unionTopologicalOperator = firsteometry as ITopologicalOperator;
            unionTopologicalOperator.Simplify();

            //1.2   构造位于梯段上的圆柱形垂直栏杆构件
            IGeometry g1 = null;
            IGeometry g2 = null;

            for (int i = 1; i < baluster.StairFlight.NumOfSteps + 1; i++)
            {
                g1 = GeometryUtility.Construct_Cylinder(
                    GeometryUtility.Construct_Point3D((2 * i - 1) * baluster.GapWidth, 0, -i * baluster.StairFlight.StepHeight),
                    baluster.Radius,
                    baluster.Height + baluster.StairFlight.StepHeight / 2,
                    new MathExtension.Vector.Vector(0, 0, 1), 5);

                g2 = GeometryUtility.Construct_Cylinder(
                    GeometryUtility.Construct_Point3D(2 * i * baluster.GapWidth, 0, -i * baluster.StairFlight.StepHeight),
                    baluster.Radius,
                    baluster.Height,
                    new MathExtension.Vector.Vector(0, 0, 1), 5);

                //合并栏杆立柱
                unionTopologicalOperator = unionTopologicalOperator.Union(g1) as ITopologicalOperator;
                unionTopologicalOperator = unionTopologicalOperator.Union(g2) as ITopologicalOperator;
            }

            //1.3    构造位于上行梯段开始处休息平台上的圆柱形垂直栏杆立柱
            double x = (2 * (baluster.StairFlight.NumOfSteps + 1) - 1) * baluster.GapWidth;
            double z = -(baluster.StairFlight.NumOfSteps + 1) * baluster.StairFlight.StepHeight;

            IGeometry lastGeometry = GeometryUtility.Construct_Cylinder(
                      GeometryUtility.Construct_Point3D(x, 0, z),
                      baluster.Radius,
                      baluster.Height + baluster.StairFlight.StepHeight / 2,
                      new MathExtension.Vector.Vector(0, 0, 1), 6);

            firsteometry = unionTopologicalOperator.Union(lastGeometry);


            //2.    生成下行立柱并将其移动到原点
            ITransform3D transform_down = new MultiPatchClass();
            (transform_down as IClone).Assign((firsteometry as IClone).Clone());

            transform_down.RotateVector3D(GeometryUtility.Axis_Vector(3), Math.PI);

            double moveX = baluster.StairFlight.StepWidth * baluster.StairFlight.NumOfSteps + baluster.GapWidth;
            double moveY = 0;
            double moveZ = baluster.StairFlight.StepHeight * (baluster.StairFlight.NumOfSteps + 1);
            transform_down.Move3D(moveX, moveY, moveZ);

            //3.    组装上下行栏杆柱
            if (baluster.StairFlight.IsLeft_downStair)
            {
                //移动下行立柱
                moveY = baluster.StairFlight.StairwellWidth + 2 * baluster.DistanceToStairwell;
                transform_down.Move3D(0, moveY, 0);
            }
            else
            {
                //移动上行立柱
                moveX = 0;
                moveY = baluster.StairFlight.StairwellWidth + 2 * baluster.DistanceToStairwell;
                moveZ = 0;
                (firsteometry as ITransform3D).Move3D(moveX, moveY, moveZ);
            }

            //合并上下行立柱
            columns = (firsteometry as ITopologicalOperator).Union(transform_down as IGeometry);

            //将上下立柱在休息平台上的两个立柱的底面圆心移动到OCS原点
            double y_offset = -moveY / 2;
            (columns as ITransform3D).Move3D(0, y_offset, 0);

            return columns;
        }


        //构造完整的上下行扶手（以休息平台上的拐弯处扶手两端连线中点在XY平面上的投影点为OCS原点）
        private static IGeometry Construct_Handrail(ref StairParameters.HandRail handrail)
        {

            //已经变换过的完整扶手
            IGeometry geometry_handrail;

            //上行扶手主体部分
            IGeometry mainHandrailSection = GeometryUtility.Construct_Rectangle
                 (
                    GeometryUtility.Construct_Point3D(0, 0, 0),
                    handrail.Thickness,
                    handrail.Width,
                    GeometryUtility.Construct_Vector3D(0, 1, 0),
                     89.9 / 180 * Math.PI
                    );

            IConstructMultiPatch constructMultipatch_1 = new MultiPatchClass();
            (constructMultipatch_1 as IZAware).ZAware = true;
            constructMultipatch_1.ConstructExtrudeRelative(handrail.ExtrudeVector_1, mainHandrailSection);



            //扶手拐弯处纵断面
            IGeometry subHandrailSection = GeometryUtility.Construct_Rectangle
            (
             GeometryUtility.Construct_Point3D(0, 0, 0),
              handrail.Width,
             handrail.Thickness,
             GeometryUtility.Construct_Vector3D(1, 0, 0),
             89.9 / 180 * Math.PI
            );

            //扶手拐弯处
            IConstructMultiPatch constructMultipatch_2 = new MultiPatchClass();
            IConstructMultiPatch constructMultipatch_3 = new MultiPatchClass();
            (constructMultipatch_2 as IZAware).ZAware = true;
            (constructMultipatch_3 as IZAware).ZAware = true;

            IGeometry subHandrail_up;
            ITransform3D subHandrail_down;

            /*根据上下行梯段左右分布情况分别构造上下方拐弯处扶手，并与主体扶手合并生成上行梯段，
             * 再基于完整上行梯段变换生成下行梯段
            */

            double moveX = 0;
            double moveY = 0;
            double moveZ = 0;
            if (handrail.Baluster.StairFlight.IsLeft_downStair)
            {
                //上扬扶手拐弯处
                constructMultipatch_2.ConstructExtrudeRelative(handrail.ExtrudeVector_2, subHandrailSection);
                subHandrail_up = constructMultipatch_2 as IGeometry;

                //下倾扶手拐弯处
                constructMultipatch_3.ConstructExtrudeRelative(handrail.ExtrudeVector_3, subHandrailSection);
                subHandrail_down = constructMultipatch_3 as ITransform3D;

                //移动下倾拐弯处至上行扶手起始处
                StairParameters.Baluster baluster = handrail.Baluster;
                moveX = baluster.StairFlight.StepWidth * baluster.StairFlight.NumOfSteps + baluster.GapWidth;
                moveY = 0;
                moveZ = -baluster.StairFlight.StepHeight * (baluster.StairFlight.NumOfSteps + 0.5);
                subHandrail_down.Move3D(moveX, moveY, moveZ);

                //将拐弯处于主体扶手合并
                ITopologicalOperator mainHandrail = constructMultipatch_1 as ITopologicalOperator;
                IGeometry upHandrail = mainHandrail.Union(constructMultipatch_2 as IGeometry);
                upHandrail = (upHandrail as ITopologicalOperator).Union(constructMultipatch_3 as IGeometry);

                //创建下行扶手
                ITransform3D transform_handrail_down = new MultiPatchClass();
                (transform_handrail_down as IClone).Assign((upHandrail as IClone).Clone());

                //旋转并移动下行扶手
                transform_handrail_down.RotateVector3D(GeometryUtility.Axis_Vector(3), Math.PI);
                moveY = baluster.StairFlight.StairwellWidth + 2 * baluster.DistanceToStairwell;//楼梯井宽+2倍栏杆距楼梯井边缘水平距离
                moveZ = baluster.StairFlight.StepHeight * (baluster.StairFlight.NumOfSteps + 1);
                transform_handrail_down.Move3D(moveX, moveY, moveZ);

                //上、下行扶手合并
                geometry_handrail = (upHandrail as ITopologicalOperator).Union(transform_handrail_down as IGeometry);

                //将上下扶手在休息平台上的拐弯处的水平投影中心移动到OCS原点
                double y_offset = -(baluster.StairFlight.StairwellWidth / 2 + baluster.DistanceToStairwell);
                (geometry_handrail as ITransform3D).Move3D(0, y_offset, 0);

            }
            else
            {
                //改变拐弯处扶手的朝向，使其面向Y轴负方向
                handrail.ExtrudeVector_2.YComponent = -handrail.ExtrudeVector_2.YComponent;
                handrail.ExtrudeVector_3.YComponent = -handrail.ExtrudeVector_3.YComponent;

                //上扬扶手拐弯处
                constructMultipatch_2.ConstructExtrudeRelative(handrail.ExtrudeVector_2, subHandrailSection);
                subHandrail_up = constructMultipatch_2 as IGeometry;

                //下倾扶手拐弯处
                constructMultipatch_3.ConstructExtrudeRelative(handrail.ExtrudeVector_3, subHandrailSection);
                subHandrail_down = constructMultipatch_3 as ITransform3D;


                //移动下倾拐弯处至上行扶手起始处
                StairParameters.Baluster baluster = handrail.Baluster;
                moveX = baluster.StairFlight.StepWidth * baluster.StairFlight.NumOfSteps + baluster.GapWidth;
                moveY = 0;
                moveZ = -baluster.StairFlight.StepHeight * (baluster.StairFlight.NumOfSteps + 0.5);
                subHandrail_down.Move3D(moveX, moveY, moveZ);

                //将拐弯处于主体扶手合并
                ITopologicalOperator mainHandrail = constructMultipatch_1 as ITopologicalOperator;
                IGeometry upHandrail = mainHandrail.Union(constructMultipatch_2 as IGeometry);
                upHandrail = (upHandrail as ITopologicalOperator).Union(constructMultipatch_3 as IGeometry);

                //创建下行扶手
                ITransform3D transform_handrail_down = new MultiPatchClass();
                (transform_handrail_down as IClone).Assign((upHandrail as IClone).Clone());

                //旋转并移动下行扶手
                transform_handrail_down.RotateVector3D(GeometryUtility.Axis_Vector(3), Math.PI);

                moveZ = baluster.StairFlight.StepHeight * (baluster.StairFlight.NumOfSteps + 1);
                transform_handrail_down.Move3D(moveX, moveY, moveZ);

                //移动上行扶手
                moveX = 0;
                moveY = baluster.StairFlight.StairwellWidth + 2 * baluster.DistanceToStairwell;//楼梯井宽+2倍栏杆距楼梯井边缘水平距离
                moveZ = 0;
                ITransform3D transform_handrail_up = upHandrail as ITransform3D;
                transform_handrail_up.Move3D(moveX, moveY, moveZ);




                //合并上、下行扶手
                geometry_handrail = (transform_handrail_up as ITopologicalOperator).Union(transform_handrail_down as IGeometry);

                //将上下扶手在休息平台上的拐弯处的水平投影中心移动到OCS原点
                double y_offset = -(baluster.StairFlight.StairwellWidth / 2 + baluster.DistanceToStairwell);
                (geometry_handrail as ITransform3D).Move3D(0, y_offset, 0);
            }
            return geometry_handrail;
        }

        #endregion
    }
}
