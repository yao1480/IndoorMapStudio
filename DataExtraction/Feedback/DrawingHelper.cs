using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCE.BCExtractors;
using BCE.DataModels.Basic;
using DXFReader.DataModel;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ArcEngineDataUtilities.DataOperation;

namespace Feedback
{
    internal abstract class DrawingHelper
    {
        public static void Drawing_Result_Wall(IMapControlDefault graphicsControl, Result result)
        {
            IGraphicsContainer container = graphicsControl.ActiveView as IGraphicsContainer;



            IColor color_for_ring = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 255);
            IColor color_for_incomRing = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
            IColor color_for_isolatedLines = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 0);
            double width_for_ring = 1;
            double width_for_incomRing = 3;
            double width_for_isolatedLines = 3;

            IPolyline polyline = null;
            IElement element = null;


            List<SemanticPolygon> walls = (from p in result.SPolygons
                                           where p.PType == PolygonType.Wall
                                           select p).ToList<SemanticPolygon>();

            foreach (var item in walls)
            {
                polyline = Create_Polyline_FromLines(item.Lines);
                element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_ring, width_for_ring);
                container.AddElement(element, 0);
            }

            if (result.ER_ForWalls.IncompRings != null)
            {
                foreach (var item in result.ER_ForWalls.IncompRings)
                {
                    polyline = Create_Polyline_FromLines(item.Lines);
                    element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_incomRing, width_for_incomRing);
                    container.AddElement(element, 0);
                }
            }



            if (result.ER_ForWalls.IsolateLines != null)
            {

                polyline = Create_Polyline_FromLines(result.ER_ForWalls.IsolateLines);
                element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_isolatedLines, width_for_isolatedLines);
                container.AddElement(element, 0);
            }


            if (result.ER_ForWalls != null)
            {
                double[] extent = result.ER_ForWalls.Calc_Extent();
                IEnvelope envelope = new EnvelopeClass() { XMin = extent[0], XMax = extent[1], YMin = extent[2], YMax = extent[3] };
                graphicsControl.ActiveView.Extent = envelope;
                graphicsControl.ActiveView.FullExtent = envelope;
                graphicsControl.ActiveView.Refresh();
            }
        }

        public static void Drawing_Result_Balustrade(IMapControlDefault graphicsControl, Result result)
        {
            IGraphicsContainer container = graphicsControl.ActiveView as IGraphicsContainer;

            IColor color_for_ring = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 255);
            IColor color_for_incomRing = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
            IColor color_for_isolatedLines = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 0);
            double width_for_ring = 1;
            double width_for_incomRing = 3;
            double width_for_isolatedLines = 3;


            IPolyline polyline = null;
            IElement element = null;



            List<SemanticPolygon> walls = (from p in result.SPolygons
                                           where p.PType == PolygonType.Balustrade
                                           select p).ToList<SemanticPolygon>();

            foreach (var item in walls)
            {
                polyline = Create_Polyline_FromLines(item.Lines);
                element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_ring, width_for_ring);
                container.AddElement(element, 0);
            }

            if (result.ER_ForBalustrade.IncompRings != null)
            {
                foreach (var item in result.ER_ForBalustrade.IncompRings)
                {
                    polyline = Create_Polyline_FromLines(item.Lines);
                    element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_incomRing, width_for_incomRing);
                    container.AddElement(element, 0);
                }
            }


            if (result.ER_ForBalustrade.IsolateLines != null)
            {

                polyline = Create_Polyline_FromLines(result.ER_ForBalustrade.IsolateLines);
                element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_isolatedLines, width_for_isolatedLines);
                container.AddElement(element, 0);
            }

            if (result.ER_ForBalustrade != null)
            {
                double[] extent = result.ER_ForBalustrade.Calc_Extent();
                IEnvelope envelope = new EnvelopeClass() { XMin = extent[0], XMax = extent[1], YMin = extent[2], YMax = extent[3] };
                graphicsControl.ActiveView.Extent = envelope;
                graphicsControl.ActiveView.FullExtent = envelope;
                graphicsControl.ActiveView.Refresh();
            }



        }

        public static void Drawing_Result_FuncRegion(IMapControlDefault graphicsControl, Result result)
        {
            /*由于墙在之后的门窗洞填补过程中可能被裁剪，因此不能使用 Result. ER_ForWalls
             不完整的墙和独立线段也有可能被裁剪，但是几率很小,故使用Result. ER_ForWalls存储的InComLines 和 IsolatedLines*/


            IGraphicsContainer container = graphicsControl.ActiveView as IGraphicsContainer;


          
            IColor color_for_floorRing = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 255);
            IColor color_for_rooms = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 0);
            IColor color_for_incomRing = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
            IColor color_for_isolatedLines = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 0);
            double width_for_ring = 1;
            double width_for_incomRing = 3;
            double width_for_isolatedLines = 3;


            IPolyline polyline = null;
            IPolygon polygon = null;
            IElement element = null;


            List<SemanticPolygon> funcRegions = (from p in result.SPolygons
                                                 where p.PType != PolygonType.Wall && p.PType != PolygonType.Balustrade
                                                 select p).ToList<SemanticPolygon>();


            foreach (var item in funcRegions)
            {
                if (item.PType == PolygonType.Floor)
                {
                    polyline = Create_Polyline_FromLines(item.Lines);
                    element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_floorRing, width_for_ring);
                }
                else
                {
                    polygon = create_Polygon_FromPoints(item.Points, 0);
                    element = ArcObjectsUtilities.ElementUtility.Construct_PolygonElement(polygon,color_for_rooms);
                }


                container.AddElement(element, 0);
            }

  
            if (result.ER_ForFuncRegion.IncompRings != null)
            {
                foreach (var item in result.ER_ForFuncRegion.IncompRings)
                {
                    polyline = Create_Polyline_FromLines(item.Lines);
                    element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_incomRing, width_for_incomRing);
                    container.AddElement(element, 0);
                }
            }


            if (result.ER_ForFuncRegion.IsolateLines != null)
            {

                polyline = Create_Polyline_FromLines(result.ER_ForFuncRegion.IsolateLines);
                element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color_for_isolatedLines, width_for_isolatedLines);
                container.AddElement(element, 0);
            }


            //if (result.ER_ForFuncRegion != null)
            //{
            //    double[] extent = result.ER_ForBalustrade.Calc_Extent();
            //    IEnvelope envelope = new EnvelopeClass() { XMin = extent[0], XMax = extent[1], YMin = extent[2], YMax = extent[3] };
            //    graphicsControl.ActiveView.Extent = envelope;
            //    graphicsControl.ActiveView.FullExtent = envelope;
            //    graphicsControl.ActiveView.Refresh();
            //}

        }


        public static void Drawing_Result_DoorLines(IMapControlDefault graphicsControl, Result result)
        {



            IGraphicsContainer container = graphicsControl.ActiveView as IGraphicsContainer;

            IColor color = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 255);
            double width = 1.5;


            IPolyline polyline = null;
            IElement element = null;

            if (result.DoorLines != null)
            {
                foreach (var item in result.DoorLines)
                {
                    polyline = Create_Polyline_FromLine(item);
                    element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color, width);
                    container.AddElement(element, 0);
                }
            }

            if (result.ER_ForWalls != null)
            {
                double[] extent = result.ER_ForWalls.Calc_Extent();
                IEnvelope envelope = new EnvelopeClass() { XMin = extent[0], XMax = extent[1], YMin = extent[2], YMax = extent[3] };
                graphicsControl.ActiveView.Extent = envelope;
                graphicsControl.ActiveView.FullExtent = envelope;
                graphicsControl.ActiveView.Refresh();
            }

        }

        public static void Drawing_Result_WindowLines(IMapControlDefault graphicsControl, Result result)
        {
            /*由于阳台扶手轮廓线在之后的门窗洞填补过程中可能被裁剪，因此不能使用 Result. ER_ForWalls
             不完整的环和独立线段也有可能被裁剪，但是几率很小,故使用Result. ER_ForWalls存储的InComLines 和 IsolatedLines*/


            IGraphicsContainer container = graphicsControl.ActiveView as IGraphicsContainer;


            IColor color = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 255);
            double width = 1.5;


  
            IPolyline polyline = null;
            IElement element = null;

            if (result.WindowLines != null)
            {
                foreach (var item in result.WindowLines)
                {
                    polyline = Create_Polyline_FromLine(item);
                    element = ArcObjectsUtilities.ElementUtility.Construct_LineElement(polyline, color, width);
                    container.AddElement(element, 0);
                }
            }


            if (result.ER_ForWalls != null)
            {
                double[] extent = result.ER_ForWalls.Calc_Extent();
                IEnvelope envelope = new EnvelopeClass() { XMin = extent[0], XMax = extent[1], YMin = extent[2], YMax = extent[3] };
                graphicsControl.ActiveView.Extent = envelope;
                graphicsControl.ActiveView.FullExtent = envelope;
                graphicsControl.ActiveView.Refresh();
            }
        }









        #region 内部方法
        static IPolyline Create_Polyline_FromLine(DXFReader.DataModel.LineClass line)
        {

            IPolyline pPolyline = new PolylineClass();

            pPolyline.FromPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(line.StartPoint.X, line.StartPoint.Y);
            pPolyline.ToPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(line.EndPoint.X, line.EndPoint.Y);

            return pPolyline;
        }

        static IPolyline Create_Polyline_FromLines(List<DXFReader.DataModel.LineClass> lines)
        {
            ISegmentCollection pSC = new PathClass();

            ISegment pSegment = null;
            foreach (var item in lines)
            {
                pSegment = new ESRI.ArcGIS.Geometry.LineClass();
                pSegment.FromPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.StartPoint.X, item.StartPoint.Y);
                pSegment.ToPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.EndPoint.X, item.EndPoint.Y);
                pSC.AddSegment(pSegment);
            }

            IGeometryCollection pGC = new PolylineClass();
            pGC.AddGeometry(pSC as IGeometry);

            IPolyline pPolyline = pGC as IPolyline;
            (pPolyline as ITopologicalOperator).Simplify();

            return pPolyline;
        }

        /// <summary>
        /// 基于无重复点集创建IPolygon
        /// </summary>
        /// <param name="dictinctedPoints"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        static IPolygon create_Polygon_FromPoints(List<MathExtension.Geometry.Point> dictinctedPoints, double z)
        {
            if (dictinctedPoints == null)
                return null;

            IPointCollection pPointCollection = new RingClass();

            for (int i = 0; i < dictinctedPoints.Count; i++)
            {
                IPoint pPoint = new PointClass();
                pPoint.X = dictinctedPoints[i].X;
                pPoint.Y = dictinctedPoints[i].Y;
                pPointCollection.AddPoint(pPoint);
            }

            IRing pRing = pPointCollection as IRing;
            pRing.Close();

            IGeometryCollection pGeometryCollection = new PolygonClass();
            pGeometryCollection.AddGeometry(pRing);

            (pGeometryCollection as ITopologicalOperator).Simplify();
            return pGeometryCollection as IPolygon;
        }

        #endregion
    }
}
