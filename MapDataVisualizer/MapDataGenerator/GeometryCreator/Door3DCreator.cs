using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ArcObjectsUtilities;

namespace EsriMapDataGenerator.GeometryCreator
{
    public class Door3DCreator
    {
        /// <summary>
        /// 创建单扇门几何体
        /// </summary>
        /// <param name="s_dp"></param>
        /// <returns></returns>
        public static IGeometry Create_SDoor(ref DoorParameters.P_SingleDoor s_dp)
        {
            //step1. 创建门框
            IGeometry pBottomSection_doorColumn =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, s_dp.FrameThickness, s_dp.FrameWidth, 0d);

            //左门柱
            IVector3D v3d_extrude = GeometryUtility.Construct_Vector3D(0, 0, s_dp.Height);
            IGeometry pColumn_left = GeometryUtility.Construct_StretchedBody(pBottomSection_doorColumn, v3d_extrude);

            //右门柱
            IClone pColumn_right = new MultiPatchClass();
            pColumn_right.Assign((pColumn_left as IClone).Clone());
            double x_offset_leftToRight = s_dp.Width - s_dp.FrameThickness;
            (pColumn_right as ITransform3D).Move3D(x_offset_leftToRight, 0, 0);

            //顶框
            IGeometry pTopFrame_section =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, s_dp.Width, s_dp.FrameWidth, 0d);
            v3d_extrude.ZComponent = s_dp.FrameThickness;
            IGeometry pTopFrame = GeometryUtility.Construct_StretchedBody(pTopFrame_section, v3d_extrude);
            (pTopFrame as ITransform3D).Move3D(0, 0, s_dp.Height);


            //step2.创建门
            double doorWidthForModeling = s_dp.Width - 2 * s_dp.FrameThickness;
            double doorBottomDistanceToFloor = 5d;//门底面至底面的距离
            double doorOpenAngle = 0d;//门开启角度
            IPoint pRotatePoint_door = null;//门合页旋转点
            if (s_dp.IsClockwiseOpen)
            {
                doorOpenAngle = -Math.PI / 2;
                pRotatePoint_door =
                  ArcObjectsUtilities.GeometryUtility.Construct_Point3D(s_dp.FrameThickness, s_dp.FrameWidth / 2 - s_dp.Thickness / 2, 0);
            }

            else
            {
                doorOpenAngle = Math.PI / 2;
                pRotatePoint_door =
                    ArcObjectsUtilities.GeometryUtility.Construct_Point3D(s_dp.FrameThickness, s_dp.FrameWidth / 2 + s_dp.Thickness / 2, 0);
            }

            //创建门扇实体并移动（预留5mm下门缝）、旋转
            v3d_extrude.ZComponent = s_dp.Height - doorBottomDistanceToFloor;
            IGeometry pDoorBottomSection =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, doorWidthForModeling, s_dp.Thickness, 0d);
            IGeometry pDoor = GeometryUtility.Construct_StretchedBody(pDoorBottomSection, v3d_extrude);
            double y_offset_door = (s_dp.FrameWidth - s_dp.Thickness) / 2;
            ITransform3D pDoor_trans = pDoor as ITransform3D;
            pDoor_trans.Move3D(s_dp.FrameThickness, y_offset_door, doorBottomDistanceToFloor);
            (pDoor as ITransform2D).Rotate(pRotatePoint_door, doorOpenAngle);


            //组装零件
            ITopologicalOperator pOpt = pDoor as ITopologicalOperator;
            pOpt = pOpt.Union(pColumn_left) as ITopologicalOperator;
            pOpt = pOpt.Union(pColumn_right as IGeometry) as ITopologicalOperator;
            pOpt = pOpt.Union(pTopFrame) as ITopologicalOperator;


            (pOpt as ITransform3D).Move3D(0, -s_dp.FrameWidth / 2, 0);

            return pOpt as IGeometry;
        }

        /// <summary>
        /// 创建双叶门或子母门
        /// </summary>
        /// <param name="dm_dp"></param>
        /// <returns></returns>
        public static IGeometry Create_DMSDoor(ref DoorParameters.P_DMDoor dm_dp)
        {
            //step1. 创建门框
            IGeometry pBottomSection_doorColumn =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, dm_dp.FrameThickness, dm_dp.FrameWidth, 0d);

            //左门柱
            IVector3D v3d_extrude = GeometryUtility.Construct_Vector3D(0, 0, dm_dp.Height);
            IGeometry pColumn_left = GeometryUtility.Construct_StretchedBody(pBottomSection_doorColumn, v3d_extrude);

            //右门柱
            IClone pColumn_right = new MultiPatchClass();
            pColumn_right.Assign((pColumn_left as IClone).Clone());
            double x_offset_leftToRight = dm_dp.Width - dm_dp.FrameThickness;
            (pColumn_right as ITransform3D).Move3D(x_offset_leftToRight, 0, 0);

            //顶框
            IGeometry pTopFrame_section =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, dm_dp.Width, dm_dp.FrameWidth, 0d);
            v3d_extrude.ZComponent = dm_dp.FrameThickness;
            IGeometry pTopFrame = GeometryUtility.Construct_StretchedBody(pTopFrame_section, v3d_extrude);
            (pTopFrame as ITransform3D).Move3D(0, 0, dm_dp.Height);



            //step2.创建门
            double doorBottomDistanceToFloor = 5d;//门底面至底面的距离
            v3d_extrude.ZComponent = dm_dp.Height - doorBottomDistanceToFloor;
            double doorWidthForModeling = dm_dp.Width - 2 * dm_dp.FrameThickness;

            double doorWidth_left = doorWidthForModeling * dm_dp.Rate;
            double doorWidth_right = doorWidthForModeling * (1 - dm_dp.Rate);

            double doorOpenAngle_left = 0d;
            double doorOpenAngle_right = 0d;
            IPoint pRotatePoint_left = null;
            IPoint pRotatePoint_right = null;
            if (dm_dp.IsClockwiseOpen)
            {
                doorOpenAngle_left = -Math.PI / 2;
                doorOpenAngle_right = Math.PI / 2;

                pRotatePoint_left =
                    ArcObjectsUtilities.GeometryUtility.Construct_Point3D(dm_dp.FrameThickness, (dm_dp.FrameWidth - dm_dp.Thickness) / 2, 0);

                pRotatePoint_right =
                    ArcObjectsUtilities.GeometryUtility.Construct_Point3D(dm_dp.Width - dm_dp.FrameThickness, (dm_dp.FrameWidth - dm_dp.Thickness) / 2, 0);

            }
            else
            {
                doorOpenAngle_left = Math.PI / 2;
                doorOpenAngle_right = -Math.PI / 2;

                pRotatePoint_left =
                  ArcObjectsUtilities.GeometryUtility.Construct_Point3D(dm_dp.FrameThickness, (dm_dp.FrameWidth + dm_dp.Thickness) / 2, 0);

                pRotatePoint_right =
                    ArcObjectsUtilities.GeometryUtility.Construct_Point3D(dm_dp.Width - dm_dp.FrameThickness, (dm_dp.FrameWidth + dm_dp.Thickness) / 2, 0);
            }

            //创建左门扇实体并移动（预留5mm下门缝）、旋转
            double y_offset_door = (dm_dp.FrameWidth - dm_dp.Thickness) / 2;
            IGeometry pDoorBottomSection_left =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, doorWidth_left, dm_dp.Thickness, 0d);
            IGeometry pDoor_left = GeometryUtility.Construct_StretchedBody(pDoorBottomSection_left, v3d_extrude);

            ITransform3D pDoor_trans = pDoor_left as ITransform3D;
            pDoor_trans.Move3D(dm_dp.FrameThickness, y_offset_door, doorBottomDistanceToFloor);
            (pDoor_left as ITransform2D).Rotate(pRotatePoint_left, doorOpenAngle_left);

            //创建右门扇实体并移动（预留5mm下门缝）、旋转
            IGeometry pDoorBottomSection_right =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, doorWidth_right, dm_dp.Thickness, 0d);
            IGeometry pDoor_right = GeometryUtility.Construct_StretchedBody(pDoorBottomSection_right, v3d_extrude);

            pDoor_trans = pDoor_right as ITransform3D;
            pDoor_trans.Move3D(dm_dp.Width - dm_dp.FrameThickness - doorWidth_right, y_offset_door, doorBottomDistanceToFloor);
            (pDoor_right as ITransform2D).Rotate(pRotatePoint_right, doorOpenAngle_right);



            //组装零件
            ITopologicalOperator pOpt = pDoor_left as ITopologicalOperator;
            pOpt = pOpt.Union(pDoor_right) as ITopologicalOperator;
            pOpt = pOpt.Union(pColumn_left) as ITopologicalOperator;
            pOpt = pOpt.Union(pColumn_right as IGeometry) as ITopologicalOperator;
            pOpt = pOpt.Union(pTopFrame) as ITopologicalOperator;

            
            (pOpt as ITransform3D).Move3D(0, -dm_dp.FrameWidth / 2, 0);

            return pOpt as IGeometry;

        }

        /// <summary>
        /// 创建电梯门
        /// </summary>
        /// <param name="e_dp"></param>
        /// <returns></returns>
        public static IGeometry Create_ElevatorDoor(ref DoorParameters.P_EDoor e_dp)
        {
            IPoint pLeftDownPoint = GeometryUtility.Construct_Point3D(0d, -e_dp.DoorHoleDepth / 2, 0d);

            IGeometry pDoorBottomSection =
              GeometryUtility.Construct_Rectangle(pLeftDownPoint, e_dp.Width / 2 - 1, e_dp.Thickness, 0d);

            IGeometry pDoorLeft =
                GeometryUtility.Construct_StretchedBody(pDoorBottomSection, GeometryUtility.Construct_Vector3D(0d, 0d, e_dp.Height));

            (pDoorLeft as ITransform2D).Move(0d, (e_dp.DoorHoleDepth - e_dp.Thickness) / 2);

            IClone pDoorRight = new MultiPatchClass();
            pDoorRight.Assign((pDoorLeft as IClone).Clone());
            (pDoorRight as ITransform2D).Move(e_dp.Width / 2 + 1, 0);


            return (pDoorLeft as ITopologicalOperator).Union(pDoorRight as IGeometry);
        }

        public static List<IGeometry> Create_SlidingDoor(ref DoorParameters.P_SlidingDoor sli_dp)
        {

            List<IGeometry> slidingDoorParts = new List<IGeometry>();//滑动门部件

            const double oneDoorWidth = 600d;//单扇门的标准门洞宽
            double m_width = 0d;//建模门宽
            double m_height = sli_dp.Height - 2 * sli_dp.FrameThickness;//建模门高
            int num_doorLeaf = 0;//门扇数目

            //确定门扇数目、建模门宽
            int inter = (int)(sli_dp.Width / oneDoorWidth);
            int remander = (int)(sli_dp.Width % oneDoorWidth);

            if (remander > oneDoorWidth * 0.2)
                num_doorLeaf = ++inter;
            else
                num_doorLeaf = inter;

            //确定用于建模的门宽
            double total_overlapingWidth = (num_doorLeaf - 1) * sli_dp.DoorSideWidth;//因门扇重叠而增加的门洞长度
            m_width = (sli_dp.Width - 2 * sli_dp.FrameThickness + total_overlapingWidth) / num_doorLeaf;


            //创建门框(位置已经调整)
            IGeometry pFrame3D = create_nestedCuboid_XOZ(
                sli_dp.Width,
                sli_dp.Height,
                sli_dp.Width - 2 * sli_dp.FrameThickness,
                sli_dp.Height - 2 * sli_dp.FrameThickness,
                sli_dp.FrameWidth);

            //创建单扇门
            IGeometry pDoor3D = create_nestedCuboid_XOZ(
               m_width,
               m_height,
               m_width - 2 * sli_dp.DoorSideWidth,
               m_height - 2 * sli_dp.DoorSideWidth,
               sli_dp.DoorThickness);

            //门扇位置调整
            ITransform3D pTransform3D = null;
            pTransform3D = pDoor3D as ITransform3D;
            pTransform3D.Move3D(sli_dp.FrameThickness, 0d, sli_dp.FrameThickness);


            //创建单扇门的玻璃(位置已经调整)
            IPoint pleftDownPoint_glass =
                GeometryUtility.Construct_Point3D(sli_dp.FrameThickness+sli_dp.DoorSideWidth, -sli_dp.GrassThickness / 2, sli_dp.FrameThickness + sli_dp.DoorSideWidth);

            IGeometry pGlass3D = GeometryUtility.Construct_StretchedBody(
                GeometryUtility.Construct_Rectangle(pleftDownPoint_glass, m_width - 2 * sli_dp.DoorSideWidth, sli_dp.GrassThickness, 0d),
                GeometryUtility.Construct_Vector3D(0, 0, m_height - 2 * sli_dp.DoorSideWidth)
                );


            slidingDoorParts.Add(pFrame3D);
            for (int i = 0; i < num_doorLeaf; i++)
            {
                if (i == 0)
                {
                    slidingDoorParts.Add(pDoor3D);
                    slidingDoorParts.Add(pGlass3D);
                }
                else
                {
                    //深拷贝门扇和玻璃
                    IClone pClone_doorLeaf = new MultiPatchClass();
                    pClone_doorLeaf.Assign((pDoor3D as IClone).Clone());

                    IClone pClone_glass = new MultiPatchClass();
                    pClone_glass.Assign((pGlass3D as IClone).Clone());

                    //门扇沿X轴的偏移量
                    double move_X = i * (m_width - sli_dp.DoorSideWidth);

                    //门扇沿Y轴的偏移量,目的是使各推拉门交错排列
                    double move_Y = (i % 2) * sli_dp.DoorThickness;

                    pTransform3D = pClone_doorLeaf as ITransform3D;
                    pTransform3D.Move3D(move_X, move_Y, 0d);

                    pTransform3D = pClone_glass as ITransform3D;
                    pTransform3D.Move3D(move_X, move_Y, 0d);

                    slidingDoorParts.Add(pClone_doorLeaf as IGeometry);
                    slidingDoorParts.Add(pClone_glass as IGeometry);
                }
            }
            return slidingDoorParts;
        }


        #region MyRegion
        /// <summary>
        /// 创建XOZ平面上的嵌套长方体（前后底面镂空，且长方体左下角边线中点位于坐标原点）
        /// </summary>
        /// <param name="outerWidth"></param>
        /// <param name="outerHeight"></param>
        /// <param name="internalWidth"></param>
        /// <param name="internalHeight"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public static IGeometry create_nestedCuboid_XOZ(double outerWidth, double outerHeight, double internalWidth, double internalHeight, double thickness)
        {
            IGeometry outerFrameSection =
                      GeometryUtility.Construct_Rectangle_XYP(GeometryUtility.OriginPoint, outerWidth, outerHeight, 0d);

            IGeometry internalFrameSection =
               GeometryUtility.Construct_Rectangle_XYP(GeometryUtility.OriginPoint, internalWidth, internalHeight, 0d);


            IGeometry pFrameSection = (outerFrameSection as ITopologicalOperator).Difference(internalFrameSection);
            IGeometry pFrame3D = GeometryUtility.Construct_StretchedBody(pFrameSection, GeometryUtility.Construct_Vector3D(0, 0, thickness));

            ITransform3D pTransform3D = pFrame3D as ITransform3D;
            pTransform3D.RotateVector3D(GeometryUtility.Axis_Vector(1), Math.PI / 2);//将X0Y平面的门截面旋转到X0Z平面
            pTransform3D.Move3D(outerWidth/2, thickness/2, outerHeight / 2);//将门框左下角中点移动到原点

            return pFrame3D;
        }
        #endregion
    }
}
