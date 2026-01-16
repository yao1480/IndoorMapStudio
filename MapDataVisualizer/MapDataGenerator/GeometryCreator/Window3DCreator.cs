using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcObjectsUtilities;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace EsriMapDataGenerator.GeometryCreator
{
    public abstract class Window3DCreator
    {
        public static IGeometry[] Create_GWindow(ref WindowParameters.P_GeneralWindow g_wp)
        {
            //step1. 创建窗框
            IGeometry pBottomSection_windowColumn =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, g_wp.FrameThickness, g_wp.FrameWidth, 0d);

            //左窗柱
            IVector3D v3d_extrude = GeometryUtility.Construct_Vector3D(0, 0, g_wp.Height - 2 * g_wp.FrameThickness);
            IGeometry pColumn_left = GeometryUtility.Construct_StretchedBody(pBottomSection_windowColumn, v3d_extrude);
            (pColumn_left as ITransform3D).Move3D(0d, 0d, g_wp.StartHeight + g_wp.FrameThickness);//将窗柱移动到窗台高度

            //右窗柱
            IClone pColumn_right = new MultiPatchClass();
            pColumn_right.Assign((pColumn_left as IClone).Clone());
            double x_offset_leftToRight = g_wp.Width - g_wp.FrameThickness;
            (pColumn_right as ITransform3D).Move3D(x_offset_leftToRight, 0d, 0d);

            //中间窗柱
            double middleColumnWidth = 2 * g_wp.FrameThickness;
            IGeometry pBottomSection_middleColumn =
               GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, middleColumnWidth, g_wp.FrameWidth, 0d);
            IGeometry pColumn_centre = GeometryUtility.Construct_StretchedBody(pBottomSection_middleColumn, v3d_extrude);
            double x_offset_leftToCentre = (g_wp.Width - middleColumnWidth) / 2;
            (pColumn_centre as ITransform3D).Move3D(x_offset_leftToCentre, 0d, g_wp.StartHeight + g_wp.FrameThickness);

            //顶框
            IGeometry pTopFrame_section =
                GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, g_wp.Width, g_wp.FrameWidth, 0d);
            v3d_extrude.ZComponent = g_wp.FrameThickness;
            IGeometry pTopFrame = GeometryUtility.Construct_StretchedBody(pTopFrame_section, v3d_extrude);

            //底框
            IClone pBottomFrame = new MultiPatchClass();
            pBottomFrame.Assign((pTopFrame as IClone).Clone());

            (pTopFrame as ITransform3D).Move3D(0d, 0d, g_wp.StartHeight + g_wp.Height - g_wp.FrameThickness);//将窗柱移动到窗台高度
            (pBottomFrame as ITransform3D).Move3D(0d, 0d, g_wp.StartHeight);//将窗柱移动到窗台高度



            //组装窗框
            ITopologicalOperator pOpt = pColumn_left as ITopologicalOperator;
            pOpt = pOpt.Union(pColumn_centre as IGeometry) as ITopologicalOperator;
            pOpt = pOpt.Union(pColumn_right as IGeometry) as ITopologicalOperator;
            pOpt = pOpt.Union(pBottomFrame as IGeometry) as ITopologicalOperator;
            IGeometry pWindowFrame = pOpt.Union(pTopFrame);//完整的滑动窗窗框架

            //移动窗框夹使其左外侧中点位于坐标原点
            (pWindowFrame as ITransform3D).Move3D(0, -g_wp.FrameWidth / 2, 0);



            //step2.创建玻璃几何体
            double glassThickness = 5d;//玻璃厚度
            IGeometry pGlassBottomSection = GeometryUtility.Construct_Rectangle(GeometryUtility.OriginPoint, g_wp.Width - 2 * g_wp.FrameThickness, glassThickness, 0);
            v3d_extrude.ZComponent = g_wp.Height - 2 * g_wp.FrameThickness;
            IGeometry pGeometry_glass = GeometryUtility.Construct_StretchedBody(pGlassBottomSection, v3d_extrude);
            (pGeometry_glass as ITransform3D).Move3D(g_wp.FrameThickness, -glassThickness / 2, g_wp.StartHeight + g_wp.FrameThickness);


            //收集窗元素(第一个为框架，第二个为玻璃)
            IGeometry[] pGeometry_window = new IGeometry[2];
            pGeometry_window[0] = pWindowFrame;
            pGeometry_window[1] = pGeometry_glass;

            return pGeometry_window;
        }
    }
}
