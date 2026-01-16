using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCE.DataModels;
using DXFReader.DataModel;
using ESRI.ArcGIS.Geometry;


namespace EsriMapDataGenerator.GeometryCreator
{
    /****************************************************************************
     * 该类负责提供将图纸解析成果转换成ArcGIS平台下对应基本要素的方法
     * 
     ****************************************************************************/


    public abstract class ConverterForEsri
    {
        public static IPoint Create_Point(MathExtension.Geometry.Point point, double z)
        {
            return ArcObjectsUtilities.GeometryUtility.Construct_Point3D(point.X, point.Y, z);
        }

        public static IPoint Create_Point(MathExtension.Geometry.Point point)
        {
            return ArcObjectsUtilities.GeometryUtility.Construct_Point3D(point.X, point.Y, point.Z);
        }


        public static List<IPoint> Create_Points_FromPoints(ref List<MathExtension.Geometry.Point> points)
        {
            List<IPoint> pPoints = new List<IPoint>();
            foreach (var item in points)
                pPoints.Add(Create_Point(item));

            return pPoints;
        }


        /// <summary>
        /// 从DXFReader.DataModel.LineClass 创建PolyLine((忽略Line自带的Z坐标))
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static IPolyline Create_Polyline_FromLine(DXFReader.DataModel.LineClass line, double z)
        {
            IPolyline pPolyline = new PolylineClass();
            (pPolyline as IZAware).ZAware = true;

            pPolyline.FromPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(line.StartPoint.X, line.StartPoint.Y, z);
            pPolyline.ToPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(line.EndPoint.X, line.EndPoint.Y, z);

            return pPolyline;
        }

        public static IPolyline Create_Polyline_FromLines(List<DXFReader.DataModel.LineClass> lines, double z)
        {
            ISegmentCollection pSC = new PathClass();
            (pSC as IZAware).ZAware = true;

            ISegment pSegment = null;
            foreach (var item in lines)
            {
                pSegment = new ESRI.ArcGIS.Geometry.LineClass();
                pSegment.FromPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.StartPoint.X, item.StartPoint.Y, z);
                pSegment.ToPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.EndPoint.X, item.EndPoint.Y, z);
                pSC.AddSegment(pSegment);
            }

            IGeometryCollection pGC = new PolylineClass();
            (pGC as IZAware).ZAware = true;
            pGC.AddGeometry(pSC as IGeometry);

            return pGC as IPolyline;
        }

        public static IPolyline Create_Polyline_FromLines(List<DXFReader.DataModel.LineClass> lines)
        {
            ISegmentCollection pSC = new PathClass();
            (pSC as IZAware).ZAware = true;

            ISegment pSegment = null;
            foreach (var item in lines)
            {
                pSegment = new ESRI.ArcGIS.Geometry.LineClass();
                pSegment.FromPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.StartPoint.X, item.StartPoint.Y, item.StartPoint.Z);
                pSegment.ToPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.EndPoint.X, item.EndPoint.Y, item.EndPoint.Z);
                pSC.AddSegment(pSegment);
            }

            IGeometryCollection pGC = new PolylineClass();
            (pGC as IZAware).ZAware = true;
            pGC.AddGeometry(pSC as IGeometry);

            return pGC as IPolyline;
        }

        /// <summary>
        /// 基于点集构造IPolyline
        /// </summary>
        /// <param name="points"></param>
        /// <param name="closed"></param>
        /// <returns></returns>
        public static IPolyline Create_Polyline_FromPoints(ref List<MathExtension.Geometry.Point> points, bool closed)
        {
            //构造PathClass
            IPointCollection pPointCollection = new PolylineClass();
            (pPointCollection as IZAware).ZAware = true;

            foreach (var item in points)
                pPointCollection.AddPoint(ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.X, item.Y, item.Z));

            if (closed)
                pPointCollection.AddPoint(ArcObjectsUtilities.GeometryUtility.Construct_Point3D(points[0].X, points[0].Y, points[0].Z));

            //    IGeometryCollection pGeometryCollection=new PolylineClass();
            //( pGeometryCollection as IZAware).ZAware=true;
            // pGeometryCollection.AddGeometry(pPointCollection as IGeometry);

            return pPointCollection as IPolyline;

        }


        /// <summary>
        /// 从DXFReader.DataModel.Lines创建IPolygon(忽略Line自带的Z坐标)
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="z">指定所有点的Z坐标值</param>
        /// <returns></returns>
        //public static IPolygon Create_Polygon_FromLines(List<DXFReader.DataModel.LineClass> lines, double z)
        //{
        //    ISegmentCollection pSC = new RingClass();
        //    (pSC as IZAware).ZAware = true;

        //    ILine pLine = null;
        //    foreach (var item in lines)
        //    {
        //        pLine = new ESRI.ArcGIS.Geometry.LineClass();

        //        pLine.FromPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.StartPoint.X, item.StartPoint.Y, z);
        //        pLine.ToPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(item.EndPoint.X, item.EndPoint.Y, z);

        //        pSC.AddSegment(pLine as ISegment);
        //    }

        //    (pSC as IRing).Close();

        //    IGeometryCollection pGC = new PolygonClass();
        //    (pGC as IZAware).ZAware = true;
        //    pGC.AddGeometry(pSC as IGeometry);



        //    IPolygon pPolygon = pGC as IPolygon;
        //    (pPolygon as IZAware).ZAware = true;
        //    (pPolygon as ITopologicalOperator).Simplify();

        //    return pPolygon;
        //}

        /// <summary>
        /// 基于无重复点集创建IPolygon
        /// </summary>
        /// <param name="dictinctedPoints"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static IPolygon Create_Polygon_FromPoints(List<MathExtension.Geometry.Point> dictinctedPoints, double z)
        {
            if (dictinctedPoints == null)
                return null;

            IPointCollection pPointCollection = new RingClass();
            (pPointCollection as IZAware).ZAware = true;

            for (int i = 0; i < dictinctedPoints.Count; i++)
            {
                pPointCollection.AddPoint(Create_Point(dictinctedPoints[i], z));
            }

            IRing pRing = pPointCollection as IRing;
            pRing.Close();

            IGeometryCollection pGeometryCollection = new PolygonClass();
            (pGeometryCollection as IZAware).ZAware = true;
            pGeometryCollection.AddGeometry(pRing);

            IPolygon pPolygon = pGeometryCollection as IPolygon;
            (pPolygon as ITopologicalOperator).Simplify();
            return pPolygon;
        }



        #region MyRegion
        /// <summary>
        /// 从Stair封装双跑楼梯参数
        /// </summary>
        /// <param name="stair"></param>
        /// <returns></returns>
        public static StairParameters.DStairParameter Get_DStairParameter(Stair stair)
        {
            StairParameters.RestPlatform restPlatform =
                new StairParameters.RestPlatform(1, stair.Width_Staircase, stair.Width_Land, 150d);


            StairParameters.StairFlight flight = new StairParameters.StairFlight(1,
                         (stair.UpstairPosition == UpstairPosition.Left),
                         stair.Width_Stairway,
                         (int)stair.StepNum / 2,
                         stair.Width_Step,
                         stair.Height_Step,
                         stair.Width_Stairwell
                         );

            StairParameters.Baluster baluster = new StairParameters.Baluster(ref restPlatform, ref flight);
            StairParameters.HandRail handrail = new StairParameters.HandRail(ref baluster);
            StairParameters.StairRailing railing = new StairParameters.StairRailing(ref baluster, ref handrail);

            StairParameters.DStairParameter dsp = new StairParameters.DStairParameter(restPlatform, flight, railing);

            return dsp;
        }
        #endregion

    }
}
