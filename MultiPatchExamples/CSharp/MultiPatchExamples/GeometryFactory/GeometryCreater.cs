using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace MultiPatchExamples.GeometryFactory
{
    public class GeometryCreater
    {
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



        #region 构造多边形
        //构造 矩形
        public static PolygonClass Construct_Rectangle_XYplane(IPoint centrePoint, double length, double width, double rotateAngle_to_centrePoint)
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
                return Construct_Rectangle_XYplane(centrePoint, length, width, rotateAngle);
            }
            else
            {
                //构件几何中心位于原点的矩形
                IGeometryCollection geometryCollection = Construct_Rectangle_XYplane(GeometryCreater.Construct_Point3D(0, 0, 0), length, width, 0);
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


        public static PolygonClass Construct_Polygon(List<IPoint> points3D)
        {
            PolygonClass polygon = new PolygonClass();
            polygon.ZAware = true;
            for (int i = 0; i < points3D.Count; i++)
            {
                polygon.AddPoint(points3D[i]);
            }

            polygon.Close();
            polygon.Simplify();

            return polygon;
        }

        //基于点集构造 位于XY平面的多边形
        public static IGeometry Construct_Polygon_XYPLane(List<IPoint> points)
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
        #endregion



        //构造 圆（位于XY平面）
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


        #region 构造 长方体或棱柱
        //构造 长方体(底面将随拉伸矢量旋转)
        public static MultiPatchClass Construct_Cuboid(IPoint bottomCentrePoint, double length, double width, double height, MathExtension.Vector.Vector extrudeVector)
        {
            //1.    创建未经移动的长方体
            PolygonClass noTransformedPolygon = Construct_Rectangle_XYplane(Construct_Point3D(0, 0, 0), length, width, 0);
            noTransformedPolygon.ZAware = true;

            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            pConstructMultipatch.ConstructExtrudeFromTo(0, height, noTransformedPolygon);

            //2.    使长方体 沿拉伸向量extrudeVector 倾斜(必须先旋转再倾斜)
            IVector3D rotateAxisVector;
            double rotateAngle;
            Calc_rotateAxisVector_rotateAngle(extrudeVector, out rotateAxisVector, out rotateAngle);

            ITransform3D pTransform3D = pConstructMultipatch as ITransform3D;
            pTransform3D.RotateVector3D(rotateAxisVector, rotateAngle);

            //3.    将长方体 移动到指定位置
            pTransform3D.Move3D(bottomCentrePoint.X, bottomCentrePoint.Y, bottomCentrePoint.Z);

            return pConstructMultipatch as MultiPatchClass;
        }


        //构造 棱柱体
        public static IGeometry Construct_Prismoid(IGeometry polygonGeometry, double height, bool upper = true)
        {
            IZAware pZAware = polygonGeometry as IZAware;
            pZAware.ZAware = true;

            IPolygon pPolygon = polygonGeometry as IPolygon;
            pPolygon.Close();

            ITopologicalOperator pTopologicalOperator = polygonGeometry as ITopologicalOperator;
            pTopologicalOperator.Simplify();

            IArea3D pArea3D = polygonGeometry as IArea3D;

            IVector3D surfaceNormalVector = Calc_normalVectorOfSurface(polygonGeometry, upper);
            ILine extrudeLine = Calc_DirectionalLine(pArea3D.Centroid3D, height, surfaceNormalVector);

            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            pConstructMultipatch.ConstructExtrudeAlongLine(extrudeLine, pPolygon);

            IGeometry geometry = pConstructMultipatch as IGeometry;

            return geometry;
        }

        //基于点集构造 位于XY平面的棱柱体
        public static IGeometry Construct_Prismoid_XYPlane(List<IPoint> points, double fromHeight, double toHeight)
        {
            IGeometry polygonGeometry = Construct_Polygon_XYPLane(points);

            IConstructMultiPatch constructMultipatch = new MultiPatchClass();
            (constructMultipatch as IZAware).ZAware = true;

            constructMultipatch.ConstructExtrudeFromTo(fromHeight,toHeight, polygonGeometry);

            return constructMultipatch as IGeometry;
        }

        #endregion


        #region 构造 圆柱体
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
            Calc_rotateAxisVector_rotateAngle(extrudeVector, out rotateAxisVector, out rotateAngle);

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


            IVector3D surfaceNormalVector = Calc_normalVectorOfSurface(circle, upper);
            ILine extrudeLine = Calc_DirectionalLine(circle.Centroid3D, height, surfaceNormalVector);

            IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
            pConstructMultipatch.ConstructExtrudeAlongLine(extrudeLine, circle);

            return pConstructMultipatch as MultiPatchClass;
        }
        #endregion





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
        private static void Calc_rotateAxisVector_rotateAngle(MathExtension.Vector.Vector extrudeVector, out IVector3D rotateAxisVector, out double rotateAngle)
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
        public static IVector3D Calc_normalVectorOfSurface(IGeometry geometry, bool upper = true)
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
            MathExtension.Vector.Vector v3 = new MathExtension.Vector.Vector();
            if (upper)
            {
                //指向多边形上方
                v3 = v2.CrossMultiply(v1);
            }
            else
            {
                //指向多边形下方
                v3 = v1.CrossMultiply(v2);
            }

            IVector3D v = new Vector3DClass();
            v.SetComponents(v3.X, v3.Y, v3.Z);

            return v;
        }





        //根据起点、长度、向量计算出等效的有向线段
        public static ILine Calc_DirectionalLine(IPoint fromPoint, double length, IVector3D vector)
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
