using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geometry;

namespace ArcEngineDataUtilities.Basic
{

    public class GeometryUtility
    {

        public static IPoint OriginPoint { get { return Construct_Point3D(0d, 0d, 0d); } }

        #region 点及向量
        //构造常见坐标轴向量
        public static IVector3D Axis_Vector(int axisFlag)
        {
            switch (axisFlag)
            {
                case 1:
                    return Construct_Vector3D(1, 0, 0);
                case 2:
                    return Construct_Vector3D(0, 1, 0);
                case 3:
                    return Construct_Vector3D(0, 0, 1);

                default:
                    throw new ArgumentException("指定坐标轴类型的参数无效： axisFlag必须为 1~3中 的数字");
            }
        }

        //构造 IVector3D
        public static IVector3D Construct_Vector3D(double x, double y, double z)
        {
            IVector3D vector3D = new Vector3DClass();
            vector3D.SetComponents(x, y, z);

            return vector3D;
        }

        //构造 2D点（ZAware=false）
        public static IPoint Construct_Point2D(double x = 0d, double y = 0d)
        {
            IPoint point = new PointClass();
            point.X = x;
            point.Y = y;
            return point;
        }

        //构造 3D点（ZAware=true）
        public static IPoint Construct_Point3D(double x = 0d, double y = 0d, double z = 0d)
        {
            IPoint point = Construct_Point2D(x, y);
            point.Z = z;

            (point as IZAware).ZAware = true;


            return point;
        }
        #endregion


        //平面图形（带Z坐标）
        /// <summary>
        /// 构造矩形(平行于XY平面)
        /// </summary>
        /// <param name="leftDownPoint"></param>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="rotateAngle_to_leftDownPoint"></param>
        /// <returns></returns>
        public static PolygonClass Construct_Rectangle(IPoint leftDownPoint, double length, double width, double rotateAngle_to_leftDownPoint)
        {
            PolygonClass polygon = new PolygonClass();
            polygon.ZAware = true;

            polygon.AddPoint(Construct_Point3D(leftDownPoint.X, leftDownPoint.Y, leftDownPoint.Z));//左下角点
            polygon.AddPoint(Construct_Point3D(leftDownPoint.X, leftDownPoint.Y + width, leftDownPoint.Z));//左上角点
            polygon.AddPoint(Construct_Point3D(leftDownPoint.X + length, leftDownPoint.Y + width, leftDownPoint.Z));//右上角点
            polygon.AddPoint(Construct_Point3D(leftDownPoint.X + length, leftDownPoint.Y, leftDownPoint.Z));//右下角点

            polygon.Close();
            polygon.Simplify();

            //以左下角点为旋转中心，在XY平面内旋转
            polygon.Rotate(leftDownPoint, rotateAngle_to_leftDownPoint);

            return polygon;
        }

        public static PolygonClass Construct_Rectangle_XYP(IPoint centrePoint, double length, double width, double rotateAngle_to_centrePoint)
        {
            PolygonClass polygon = new PolygonClass();
            polygon.ZAware = true;

            polygon.AddPoint(Construct_Point3D(centrePoint.X - length / 2, centrePoint.Y - width / 2, centrePoint.Z));//左下角点
            polygon.AddPoint(Construct_Point3D(centrePoint.X - length / 2, centrePoint.Y + width / 2, centrePoint.Z));//左上角点
            polygon.AddPoint(Construct_Point3D(centrePoint.X + length / 2, centrePoint.Y + width / 2, centrePoint.Z));//右上角点
            polygon.AddPoint(Construct_Point3D(centrePoint.X + length / 2, centrePoint.Y - width / 2, centrePoint.Z));//右下角点

            polygon.Close();
            polygon.Simplify();

            //以几何中心为旋转中心，在XY平面内旋转
            polygon.Rotate(centrePoint, rotateAngle_to_centrePoint);

            return polygon;
        }

        public static PolygonClass Construct_Rectangle(IPoint centrePoint, double length, double width, IVector3D rotateVector, double rotateAngle)
        {
            if (rotateAngle == 0)
            {
                return Construct_Rectangle_XYP(centrePoint, length, width, rotateAngle);
            }
            else
            {
                //构件几何中心位于原点的矩形
                IGeometryCollection geometryCollection = Construct_Rectangle_XYP(GeometryUtility.Construct_Point3D(0, 0, 0), length, width, 0);
                IRing ring = geometryCollection.get_Geometry(0) as IRing;
                ring.Close();

                //矩形位置调整
                ITransform3D transform3D = ring as ITransform3D;
                transform3D.RotateVector3D(rotateVector, rotateAngle);
                transform3D.Move3D(centrePoint.X, centrePoint.Y, centrePoint.Z);


                geometryCollection = new PolygonClass();
                (geometryCollection as IZAware).ZAware = true;
                geometryCollection.AddGeometry(transform3D as IGeometry);


                return geometryCollection as PolygonClass;
            }
        }

        /// <summary>
        /// 基于点集构造多边形(Z坐标值由点自行确定)
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static IGeometry Construct_Polygon(List<IPoint> points)
        {
            IPointCollection pointCollections = new RingClass();
            (pointCollections as IZAware).ZAware = true;

            for (int i = 0; i < points.Count; i++)
            {
                pointCollections.AddPoint(points[i]);
            }

            (pointCollections as IRing).Close();

            IGeometryCollection geometryCollection = new PolygonClass();
            (geometryCollection as IZAware).ZAware = true;
            (geometryCollection as ITopologicalOperator).Simplify();

            geometryCollection.AddGeometry(pointCollections as IGeometry);

            return geometryCollection as IGeometry;
        }








        /// <summary>
        /// 构造 圆（平行于XY平面）
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="radius"></param>
        /// <param name="numOfSides"></param>
        /// <returns></returns>
        public static PolygonClass Construct_Circle(IPoint centre, double radius = 1d, int numOfSides = 12)
        {
            //创建圆
            PolygonClass polygon = new PolygonClass();
            polygon.ZAware = true;

            double papi = 2 * Math.PI / numOfSides;
            for (int i = 0; i < numOfSides + 1; i++)
            {
                polygon.AddPoint(Construct_Point3D(radius * Math.Cos(i * papi), radius * Math.Sin(i * papi), 0d));
            }

            polygon.Close();
            polygon.Simplify();

            //移动圆
            polygon.Move3D(centre.X, centre.Y, centre.Z);

            return polygon;
        }

        //三维实体
        /// <summary>
        /// 构造 长方体(底面将随拉伸矢量旋转)
        /// </summary>
        /// <param name="bottomCentrePoint"></param>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="extrudeVector"></param>
        /// <returns></returns>
        public static MultiPatchClass Construct_Cuboid(IPoint bottomCentrePoint, double length, double width, double height, MathExtension.Vector.Vector extrudeVector)
        {
            //1.    创建未经移动的长方体
            PolygonClass noTransformedPolygon = Construct_Rectangle(Construct_Point3D(0, 0, 0), length, width, 0);
            noTransformedPolygon.ZAware = true;

            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            pConstructMultipatch.ConstructExtrudeFromTo(0, height, noTransformedPolygon);

            //2.    使长方体 沿拉伸向量extrudeVector 倾斜(必须先旋转再倾斜)
            IVector3D rotateAxisVector = null;
            double rotateAngle = 0;
            calc_rotateAxisVector_rotateAngle(extrudeVector, out rotateAxisVector, out rotateAngle);

            ITransform3D pTransform3D = pConstructMultipatch as ITransform3D;
            pTransform3D.RotateVector3D(rotateAxisVector, rotateAngle);

            //3.    将长方体 移动到指定位置
            pTransform3D.Move3D(bottomCentrePoint.X, bottomCentrePoint.Y, bottomCentrePoint.Z);


            return pConstructMultipatch as MultiPatchClass;
        }


        /// <summary>
        /// 构造拉伸体
        /// </summary>
        /// <param name="polygonGeometry"></param>
        /// <param name="extrudeLength"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static IGeometry Construct_StretchedBody(IGeometry polygonGeometry, double extrudeLength, bool upper = true)
        {
            IZAware pZAware = polygonGeometry as IZAware;
            pZAware.ZAware = true;

            IPolygon pPolygon = polygonGeometry as IPolygon;
            pPolygon.Close();

            ITopologicalOperator pTopologicalOperator = polygonGeometry as ITopologicalOperator;
            pTopologicalOperator.Simplify();

            IArea3D pArea3D = polygonGeometry as IArea3D;

            IVector3D surfaceNormalVector = calc_normalVectorOfSurface(polygonGeometry, upper);
            ILine extrudeLine = calc_DirectionalLine(pArea3D.Centroid3D, extrudeLength, surfaceNormalVector);

            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            (pConstructMultipatch as ITopologicalOperator).Simplify();

            pConstructMultipatch.ConstructExtrudeAlongLine(extrudeLine, pPolygon);

            IGeometry geometry = pConstructMultipatch as IGeometry;

            return geometry;
        }

        /// <summary>
        /// 构造拉伸体
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="extrudeVector"></param>
        /// <returns></returns>
        public static IGeometry Construct_StretchedBody(IGeometry polygon, IVector3D extrudeVector)
        {
            IPolygon pPolygon = polygon as IPolygon;
            if (pPolygon == null)
                return null;


            (polygon as IZAware).ZAware = true;
            pPolygon.Close();

            ITopologicalOperator pTopologicalOperator = polygon as ITopologicalOperator;
            pTopologicalOperator.Simplify();

            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            (pConstructMultipatch as IZAware).ZAware = true;

            pConstructMultipatch.ConstructExtrudeRelative(extrudeVector, polygon);

            (pConstructMultipatch as ITopologicalOperator).Simplify();

            return pConstructMultipatch as IGeometry;
        }






        /// <summary>
        /// 基于点集构造拉伸体(底面平行于XY平面)
        /// </summary>
        /// <param name="points"></param>
        /// <param name="fromHeight"></param>
        /// <param name="toHeight"></param>
        /// <returns></returns>
        public static IGeometry Construct_StretchedBody_PZY(List<IPoint> points, double fromHeight, double toHeight)
        {
            IGeometry polygonGeometry = Construct_Polygon(points);

            IConstructMultiPatch constructMultipatch = new MultiPatchClass();
            (constructMultipatch as IZAware).ZAware = true;

            constructMultipatch.ConstructExtrudeFromTo(fromHeight, toHeight, polygonGeometry);

            return constructMultipatch as IGeometry;
        }




        //(底面将随拉伸矢量旋转)
        public static MultiPatchClass Construct_Cylinder(IPoint bottomCircleCentre, double radius, double height, MathExtension.Vector.Vector extrudeVector, int numOfSides = 12)
        {
            //1.    创建未经移动的圆柱
            IGeometry noMovedBottomCircle = Construct_Circle(Construct_Point3D(0, 0, 0), radius, numOfSides);
            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            pConstructMultipatch.ConstructExtrudeFromTo(0, height, noMovedBottomCircle);

            //2.    使圆柱 沿向量directionVector 倾斜
            IVector3D rotateAxisVector;
            double rotateAngle;
            calc_rotateAxisVector_rotateAngle(extrudeVector, out rotateAxisVector, out rotateAngle);

            ITransform3D pTransform3D = pConstructMultipatch as ITransform3D;
            pTransform3D.RotateVector3D(rotateAxisVector, rotateAngle);

            //3.    将圆柱移动到指定位置
            pTransform3D.Move3D(bottomCircleCentre.X, bottomCircleCentre.Y, bottomCircleCentre.Z);

            return pTransform3D as MultiPatchClass;
        }

        public static MultiPatchClass Construct_Cylinder(PolygonClass circle, double height, bool upper = true)
        {
            circle.ZAware = true;
            circle.Close();
            circle.Simplify();


            IVector3D surfaceNormalVector = calc_normalVectorOfSurface(circle, upper);
            ILine extrudeLine = calc_DirectionalLine(circle.Centroid3D, height, surfaceNormalVector);

            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            pConstructMultipatch.ConstructExtrudeAlongLine(extrudeLine, circle);

            return pConstructMultipatch as MultiPatchClass;
        }






        #region MyRegion
        public static IGeometryCollection TriangleStrip_GeometryCollection(IGeometry triangleStripGeometry)
        {
            IGeometryCollection outlineGeometryCollection = new GeometryBagClass();

            IPointCollection triangleStripPointCollection = triangleStripGeometry as IPointCollection;

            // TriangleStrip: a linked strip of triangles, where every vertex (after the first two) completes a new triangle.
            //                A new triangle is always formed by connecting the new vertex with its two immediate predecessors.

            for (int i = 2; i < triangleStripPointCollection.PointCount; i++)
            {
                IPointCollection outlinePointCollection = new PolylineClass();

                outlinePointCollection.AddPoint(triangleStripPointCollection.get_Point(i - 2));
                outlinePointCollection.AddPoint(triangleStripPointCollection.get_Point(i - 1));
                outlinePointCollection.AddPoint(triangleStripPointCollection.get_Point(i));
                outlinePointCollection.AddPoint(triangleStripPointCollection.get_Point(i - 2)); //Simulate: Polygon.Close

                IGeometry outlineGeometry = outlinePointCollection as IGeometry;

                (outlineGeometry as IZAware).ZAware = true;

                outlineGeometryCollection.AddGeometry(outlineGeometry);
            }

            return outlineGeometryCollection;
        }

        public static IGeometryCollection TriangleFan_GeometryCollection(IGeometry triangleFanGeometry)
        {
            IGeometryCollection outlineGeometryCollection = new GeometryBagClass();

            IPointCollection triangleFanPointCollection = triangleFanGeometry as IPointCollection;

            // TriangleFan: a linked fan of triangles, where every vertex (after the first two) completes a new triangle. 
            //              A new triangle is always formed by connecting the new vertex with its immediate predecessor 
            //              and the first vertex of the part.

            for (int i = 2; i < triangleFanPointCollection.PointCount; i++)
            {
                IPointCollection outlinePointCollection = new PolylineClass();

                outlinePointCollection.AddPoint(triangleFanPointCollection.get_Point(0));
                outlinePointCollection.AddPoint(triangleFanPointCollection.get_Point(i - 1));
                outlinePointCollection.AddPoint(triangleFanPointCollection.get_Point(i));
                outlinePointCollection.AddPoint(triangleFanPointCollection.get_Point(0)); //Simulate: Polygon.Close

                IGeometry outlineGeometry = outlinePointCollection as IGeometry;

                (outlineGeometry as IZAware).ZAware = true;

                outlineGeometryCollection.AddGeometry(outlineGeometry);
            }

            return outlineGeometryCollection;
        }

        public static IGeometryCollection Triangle_GeometryCollection(IGeometry trianglesGeometry)
        {
            IGeometryCollection outlineGeometryCollection = new GeometryBagClass();

            IPointCollection trianglesPointCollection = trianglesGeometry as IPointCollection;

            // Triangles: an unlinked set of triangles, where every three vertices completes a new triangle.

            if ((trianglesPointCollection.PointCount % 3) != 0)
            {
                throw new Exception("Triangles Geometry Point Count Must Be Divisible By 3. " + trianglesPointCollection.PointCount);
            }
            else
            {
                for (int i = 0; i < trianglesPointCollection.PointCount; i += 3)
                {
                    IPointCollection outlinePointCollection = new PolylineClass();

                    outlinePointCollection.AddPoint(trianglesPointCollection.get_Point(i));
                    outlinePointCollection.AddPoint(trianglesPointCollection.get_Point(i + 1));
                    outlinePointCollection.AddPoint(trianglesPointCollection.get_Point(i + 2));
                    outlinePointCollection.AddPoint(trianglesPointCollection.get_Point(i)); //Simulate: Polygon.Close

                    IGeometry outlineGeometry = outlinePointCollection as IGeometry;

                    (outlineGeometry as IZAware).ZAware = true;

                    outlineGeometryCollection.AddGeometry(outlineGeometry);
                }
            }

            return outlineGeometryCollection;
        }

        public static IGeometry Ring_GeometryCollection(IGeometry ringGeometry)
        {
            IGeometry outlineGeometry = new PolylineClass();

            IPointCollection outlinePointCollection = outlineGeometry as IPointCollection;

            IPointCollection ringPointCollection = ringGeometry as IPointCollection;

            for (int i = 0; i < ringPointCollection.PointCount; i++)
            {
                outlinePointCollection.AddPoint(ringPointCollection.get_Point(i));
            }

            outlinePointCollection.AddPoint(ringPointCollection.get_Point(0)); //Simulate: Polygon.Close

            (outlineGeometry as IZAware).ZAware = true;

            return outlineGeometry;
        }
        #endregion



        #region 内部方法
        private static void calc_rotateAxisVector_rotateAngle(MathExtension.Vector.Vector extrudeVector, out IVector3D rotateAxisVector, out double rotateAngle)
        {
            //Z轴向量
            MathExtension.Vector.Vector zAxisVector = new MathExtension.Vector.Vector(0d, 0d, 1d);

            //旋转轴向量
            MathExtension.Vector.Vector rotateAxisV = zAxisVector.CrossMultiply(extrudeVector);


            if (rotateAxisV.Norm == 0)
            {
                //旋转轴模为零，说明倾斜方形为Z轴正向
                rotateAxisVector = Construct_Vector3D(0, 1, 0);
                rotateAngle = 0;
            }
            else
            {
                //将rotateAxisVector转换为IVector
                rotateAxisVector = Construct_Vector3D(rotateAxisV.X, rotateAxisV.Y, rotateAxisV.Z);

                //计算旋转角
                rotateAngle = MathExtension.Vector.VectorHelper.Calc_AngleInRadian(extrudeVector, zAxisVector);
            }




        }

        //计算平面的法向量
        public static IVector3D calc_normalVectorOfSurface(IGeometry geometry, bool upper = true)
        {



            IPointCollection pPointCollection = (geometry as PolygonClass) as IPointCollection;

            //点数检查
            if (pPointCollection.PointCount < 3)
                throw new ArgumentException("无法计算表面的法向量：构成表面的点数<3 ！");

            //以前三点生成平面法向量
            IPoint p1 = pPointCollection.get_Point(0);
            IPoint p2 = pPointCollection.get_Point(1);
            IPoint p3 = pPointCollection.get_Point(2);

            MathExtension.Vector.Vector v1 = new MathExtension.Vector.Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            MathExtension.Vector.Vector v2 = new MathExtension.Vector.Vector(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

            //三点不共线检查
            if (MathExtension.Vector.VectorHelper.Parallel(v1, v2))
            {
                throw new ArgumentException("无法计算平面的法向量：用于生成平面法向量的三点共线 ！");
            }



            //计算平面法向量
            MathExtension.Vector.Vector v3 = v2.CrossMultiply(v1);
            if (upper)
            {
                //指向多边形上方
                v3.Z = Math.Abs(v3.Z);
            }
            else
            {
                //指向多边形下方
                v3.Z = -Math.Abs(v3.Z);
            }

            IVector3D v = new Vector3DClass();
            v.SetComponents(v3.X, v3.Y, v3.Z);

            return v;
        }

        //根据起点、长度、向量计算出等效的有向线段
        public static ILine calc_DirectionalLine(IPoint fromPoint, double length, IVector3D vector)
        {
            vector.Magnitude = length;
            vector.Move(fromPoint.X, fromPoint.Y, fromPoint.Z);

            ILine pLine = new LineClass();
            pLine.FromPoint = fromPoint;
            pLine.ToPoint = Construct_Point3D(vector.XComponent, vector.YComponent, vector.ZComponent);

            return pLine;
        }

        #endregion

    }

}
