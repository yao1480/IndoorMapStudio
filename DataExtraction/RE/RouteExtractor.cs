using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCE.DataModels;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace RE
{
    public abstract class RouteExtractor
    {
        /// <summary>
        /// 计算普通房间内部路径集
        /// </summary>
        /// <param name="roomSP"></param>
        /// <param name="enableSmoothing">是否启用路径平滑化</param>
        /// <param name="smoothFactor">平滑因子(0-1)</param>
        /// <returns></returns>
        public static List<List<Point>> Calc_CommonRoomRoutes(ref SemanticPolygon roomSP, bool enableSmoothing = true, double smoothFactor = 0.3)
        {
            if (roomSP.PType == PolygonType.Wall || roomSP.PType == PolygonType.Balustrade || roomSP.PType == PolygonType.Floor || roomSP.PType == PolygonType.StairRoom || roomSP.PType == PolygonType.ElevatorShaft || roomSP.DoorPoints.Count == 0)
                return null;


            List<Point> pathNodes = null;//一条路径上的节点集
            List<List<Point>> roomPaths = null;//一个房间所有路径节点集

            roomPaths = new List<List<Point>>();
            Polygon polygon = (Polygon)roomSP;
            foreach (Point dp in roomSP.DoorPoints)
            {
                pathNodes = calc_RouteNodes_FromOneDoor(dp, roomSP.FunctionRegionPoint, polygon, enableSmoothing, smoothFactor);
                roomPaths.Add(pathNodes);
            }


            List<List<Point>> nodes_betweenDoors = calc_RouteNodes_BetweenDoors(ref roomSP, enableSmoothing, smoothFactor);
            if (nodes_betweenDoors != null)
                roomPaths.AddRange(nodes_betweenDoors);

            return roomPaths;
        }

        /// <summary>
        /// 重载方法：保持向后兼容
        /// </summary>
        public static List<List<Point>> Calc_CommonRoomRoutes(ref SemanticPolygon roomSP)
        {
            return Calc_CommonRoomRoutes(ref roomSP, true, 0.3); // 默认启用平滑
        }

        /// <summary>
        /// 计算楼梯间内部路径节点集(高程值从XOY起算,不是楼板表面)
        /// </summary>
        /// <param name="doubeStairRooom"></param>
        /// <param name="stair"></param>
        /// <param name="floorThickness"></param>
        /// <param name="enableSmoothing">是否启用路径平滑化</param>
        /// <param name="smoothFactor">平滑因子(0-1)</param>
        /// <returns></returns>
        public static List<List<Point>> Cacl_DoubleStairRoomNodes(ref SemanticPolygon doubeStairRooom, ref Stair stair, double floorThickness, bool enableSmoothing = true, double smoothFactor = 0.3)
        {
            if (doubeStairRooom.PType != PolygonType.StairRoom) return null;

            List<List<Point>> roomPathNodes = new List<List<Point>>(); //楼梯间间路径节点集
            List<Point> onePathNodes = null; //一条路径节点集



            Point goPoint0 = new Point();
            Point goPoint1 = new Point();
            Point goPoint2 = new Point();
            Point goPoint3 = new Point();
            Point goPoint4 = new Point();
            Point goPoint5 = new Point();
            Point goPoint6 = new Point();


            goPoint0.X = stair.UpstairPosition == UpstairPosition.Left ? stair.Width_Staircase - stair.Width_Stairway / 2 : stair.Width_Stairway / 2;

            if (floorThickness >= stair.Height_Step)
                goPoint0.Y = -(stair.Width_Land + (stair.StepNum / 2 + 1) * stair.Width_Step);
            else
            {
                double firstStepHeightAppend = stair.Height_Step - floorThickness;
                double firstStepWidthInFloor = (stair.Width_Step / stair.Height_Step) * (stair.Height_Step + firstStepHeightAppend);//第一级台阶在XOY平面上的等效高度

                goPoint0.Y = -(stair.Width_Land + (stair.StepNum / 2) * stair.Width_Step + firstStepWidthInFloor);
            }

            goPoint1.X = stair.Width_Staircase - goPoint0.X;
            goPoint1.Y = goPoint0.Y;
            goPoint2.X = goPoint1.X;
            goPoint2.Y = -stair.Width_Land;
            goPoint3.X = goPoint2.X;
            goPoint3.Y = -(stair.Width_Land - stair.Width_Step);
            goPoint4.X = stair.UpstairPosition == UpstairPosition.Left ? stair.Width_Staircase - stair.Width_Stairway / 2 : stair.Width_Stairway / 2;
            goPoint4.Y = goPoint3.Y;
            goPoint5.X = goPoint4.X;
            goPoint5.Y = -(stair.Width_Land + stair.StepNum / 2 * stair.Width_Step);
            goPoint6.X = goPoint4.X;
            goPoint6.Y = goPoint1.Y;


            MathExtension.Geometry.GeometryHelper.AffineTransformPoint2D(ref goPoint0, 1d, 1d, stair.RotateAngle, stair.InsertPoint.X, stair.InsertPoint.Y);
            MathExtension.Geometry.GeometryHelper.AffineTransformPoint2D(ref goPoint1, 1d, 1d, stair.RotateAngle, stair.InsertPoint.X, stair.InsertPoint.Y);
            MathExtension.Geometry.GeometryHelper.AffineTransformPoint2D(ref goPoint2, 1d, 1d, stair.RotateAngle, stair.InsertPoint.X, stair.InsertPoint.Y);
            MathExtension.Geometry.GeometryHelper.AffineTransformPoint2D(ref goPoint3, 1d, 1d, stair.RotateAngle, stair.InsertPoint.X, stair.InsertPoint.Y);
            MathExtension.Geometry.GeometryHelper.AffineTransformPoint2D(ref goPoint4, 1d, 1d, stair.RotateAngle, stair.InsertPoint.X, stair.InsertPoint.Y);
            MathExtension.Geometry.GeometryHelper.AffineTransformPoint2D(ref goPoint5, 1d, 1d, stair.RotateAngle, stair.InsertPoint.X, stair.InsertPoint.Y);
            MathExtension.Geometry.GeometryHelper.AffineTransformPoint2D(ref goPoint6, 1d, 1d, stair.RotateAngle, stair.InsertPoint.X, stair.InsertPoint.Y);





            goPoint0.Z = 0d;
            goPoint1.Z = 0d;
            goPoint2.Z = floorThickness >= stair.Height_Step ? stair.Height_Floor / 2 : stair.Height_Floor / 2 + stair.Height_Step - floorThickness;
            goPoint3.Z = goPoint2.Z;
            goPoint4.Z = goPoint3.Z;
            goPoint5.Z = stair.Height_Floor;
            goPoint6.Z = goPoint5.Z;


            onePathNodes = new List<Point>()
            {
                goPoint0,
                goPoint1,
                goPoint2,
                goPoint3,
                goPoint4,
                goPoint5,
                goPoint6,
            };
            roomPathNodes.Add(onePathNodes);



            Polygon polygon = (Polygon)doubeStairRooom;
            foreach (var item in doubeStairRooom.DoorPoints)
            {
                onePathNodes = calc_RouteNodes_FromOneDoor(item, goPoint0, polygon, enableSmoothing, smoothFactor);
                roomPathNodes.Add(onePathNodes);
                onePathNodes = calc_RouteNodes_FromOneDoor(item, goPoint1, polygon, enableSmoothing, smoothFactor);
                roomPathNodes.Add(onePathNodes);
            }



            List<List<Point>> pathNodes_betweenDoors = calc_RouteNodes_BetweenDoors(ref doubeStairRooom, enableSmoothing, smoothFactor);
            if (pathNodes_betweenDoors != null)
                roomPathNodes.AddRange(pathNodes_betweenDoors);


            return roomPathNodes;
        }

        /// <summary>
        /// 创建电梯井(包括电梯间内部和垂直方向路径)路径
        /// </summary>
        /// <param name="elevatorShaft"></param>
        /// <param name="floorHeight"></param>
        /// <returns></returns>
        public static List<List<Point>> Calc_ElevatorShaftNodes(ref SemanticPolygon elevatorShaft, double floorHeight)
        {
            return Calc_ElevatorShaftNodes(ref elevatorShaft, floorHeight, true, 0.3);
        }

        /// <summary>
        /// 创建电梯井(包括电梯间内部和垂直方向路径)路径 - 带平滑参数
        /// </summary>
        /// <param name="elevatorShaft"></param>
        /// <param name="floorHeight"></param>
        /// <param name="enableSmoothing">是否启用路径平滑化</param>
        /// <param name="smoothFactor">平滑因子(0-1)</param>
        /// <returns></returns>
        public static List<List<Point>> Calc_ElevatorShaftNodes(ref SemanticPolygon elevatorShaft, double floorHeight, bool enableSmoothing, double smoothFactor)
        {
            if (elevatorShaft.PType != PolygonType.ElevatorShaft) return null;

            List<List<Point>> paths = new List<List<Point>>();
            List<Point> path = null;
            Polygon polygon = (Polygon)elevatorShaft;
            foreach (var item in elevatorShaft.DoorPoints)
            {

                path = calc_RouteNodes_FromOneDoor(item, elevatorShaft.FunctionRegionPoint, polygon, enableSmoothing, smoothFactor);

                Point functionPoint = elevatorShaft.FunctionRegionPoint;
                for (int i = 1; i <= 5; i++)
                {
                    double t = (double)i / 6;
                    Point verticalPoint = new Point(
                        functionPoint.X,
                        functionPoint.Y,
                        functionPoint.Z + t * floorHeight
                    );
                    path.Add(verticalPoint);
                }
                paths.Add(path);
            }
            
            return paths;
        }

        /// <summary>
        /// 计算房间之间的连接节点集（相邻房间的门线中点对构成的点集）
        /// </summary>
        /// <param name="floorPlan"></param>
        /// <returns></returns>
        public static List<Point> Calc_RoomLinkedPoints(ref FloorPlan floorPlan)
        {
            List<Point> pairDoorPoints = new List<Point>();

            List<LineClass> doorLines = (from p in floorPlan.PData.NewWallines
                                         where p.LType == LineType.DoorLine
                                         select p).ToList<LineClass>();

            for (int i = 0; i < doorLines.Count; i++)
            {
                pairDoorPoints.Add(doorLines[i].Get_MiddlePoint());
            }

            return pairDoorPoints;
        }



        /// <summary>
        /// 计算所有门点之间的路径(若门点之间不可视，则放弃)
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="enableSmoothing">是否启用路径平滑化</param>
        /// <param name="smoothFactor">平滑因子(0-1)</param>
        /// <returns></returns>
        static List<List<Point>> calc_RouteNodes_BetweenDoors(ref SemanticPolygon sp, bool enableSmoothing = true, double smoothFactor = 0.3)
        {
            if (sp.DoorPoints == null || sp.DoorPoints.Count < 2)
                return null;


            List<List<Point>> pathNodesBetweenDoors = new List<List<Point>>();
            List<Point> onePathNodes = null;
            Line dirSide = null;
            Point nearestIntersection = null;


            Polygon polygon = (Polygon)sp;
            for (int i = 0; i < sp.DoorPoints.Count - 1; i++)
            {

                for (int j = i + 1; j < sp.DoorPoints.Count; j++)
                {
                    onePathNodes = null;
                    if (visualTest(true, sp.DoorPoints[i], sp.DoorPoints[j], out nearestIntersection, out dirSide, polygon.Lines))
                    {
                        onePathNodes = new List<Point>();
                        onePathNodes.Add(sp.DoorPoints[i]);
                        onePathNodes.Add(sp.DoorPoints[j]);
                        
                        // 对门到门的路径也应用平滑化
                        if (enableSmoothing && onePathNodes.Count >= 3)
                        {
                            onePathNodes = smoothPath(onePathNodes, polygon, smoothFactor);
                        }
                        
                        pathNodesBetweenDoors.Add(onePathNodes);
                    }

                }
            }

            return pathNodesBetweenDoors;
        }

        /// <summary>
        /// 重载方法：保持向后兼容
        /// </summary>
        static List<List<Point>> calc_RouteNodes_BetweenDoors(ref SemanticPolygon sp)
        {
            return calc_RouteNodes_BetweenDoors(ref sp, true, 0.3);
        }


        /// <summary>
        /// 使用Catmull-Rom曲线对路径进行平滑化处理
        /// </summary>
        /// <param name="originalPath">原始路径点</param>
        /// <param name="polygon">房间多边形边界，用于验证平滑后的点是否在内部</param>
        /// <param name="smoothFactor">平滑因子(0-1)，值越大曲线越平滑</param>
        /// <returns>平滑后的路径点</returns>
        static List<Point> smoothPath(List<Point> originalPath, Polygon polygon, double smoothFactor = 0.3)
        {
            if (originalPath == null || originalPath.Count < 3)
                return originalPath;

            List<Point> smoothedPath = new List<Point>();
            
            smoothedPath.Add(originalPath[0]);

            for (int i = 1; i < originalPath.Count - 1; i++)
            {
                Point prev = originalPath[i - 1];
                Point current = originalPath[i];
                Point next = originalPath[i + 1];

                Point control1 = calculateControlPoint(prev, current, smoothFactor, true);
                Point control2 = calculateControlPoint(current, next, smoothFactor, false);

                List<Point> segmentPoints = generateSmoothSegment(current, control1, control2, 5);

                foreach (Point p in segmentPoints)
                {
                    if (isPointInPolygon(p, polygon))
                    {
                        smoothedPath.Add(p);
                    }
                }
            }

            smoothedPath.Add(originalPath[originalPath.Count - 1]);

            return smoothedPath;
        }

        /// <summary>
        /// 在三个点之间生成平滑的插值点
        /// </summary>
        private static List<Point> generateSmoothSegment(Point center, Point control1, Point control2, int segmentCount)
        {
            List<Point> segmentPoints = new List<Point>();

            for (int i = 1; i <= segmentCount; i++)
            {
                double t = (double)i / (segmentCount + 1);
                
                double x = (1 - t) * (1 - t) * control1.X + 2 * (1 - t) * t * center.X + t * t * control2.X;
                double y = (1 - t) * (1 - t) * control1.Y + 2 * (1 - t) * t * center.Y + t * t * control2.Y;

                Point smoothPoint = new Point(x, y, center.Z);
                segmentPoints.Add(smoothPoint);
            }

            return segmentPoints;
        }

        /// <summary>
        /// 计算平滑控制点
        /// </summary>
        private static Point calculateControlPoint(Point p1, Point p2, double factor, bool isIncoming)
        {
            double offsetX = (p2.X - p1.X) * factor * 0.3;
            double offsetY = (p2.Y - p1.Y) * factor * 0.3;

            Point control = new Point();
            if (isIncoming)
            {
                control.X = p2.X - offsetX;
                control.Y = p2.Y - offsetY;
            }
            else
            {
                control.X = p1.X + offsetX;
                control.Y = p1.Y + offsetY;
            }
            control.Z = p2.Z;

            return control;
        }

        /// <summary>
        /// 检查点是否在多边形内部
        /// </summary>
        private static bool isPointInPolygon(Point point, Polygon polygon)
        {
            // 使用射线法判断点是否在多边形内部
            int intersectCount = 0;
            Line ray = new Line(point, new Point(point.X + 100000, point.Y)); // 水平向右的射线

            foreach (Line side in polygon.Lines)
            {
                Point intersection = null;
                if (GeometryHelper.Calc2D_Intersection(ray, side, out intersection) == "有唯一交点"
                    && GeometryHelper.BasicRelation_2D(side, intersection) == Relation2D_Point_Line.InLine
                    && intersection.X > point.X) // 只计算右边的交点
                {
                    intersectCount++;
                }
            }

            return intersectCount % 2 == 1; // 奇数个交点在内部
        }




        /// <summary>
        /// 计算单个门点至功能区抽象点的单个路径节点集
        /// </summary>
        /// <param name="doorPoint"></param>
        /// <param name="functionAreaPoint"></param>
        /// <param name="polygon"></param>
        /// <param name="enableSmoothing">是否启用路径平滑化</param>
        /// <param name="smoothFactor">平滑因子(0-1)</param>
        /// <returns></returns>
        static List<Point> calc_RouteNodes_FromOneDoor(Point doorPoint, Point functionAreaPoint, Polygon polygon, bool enableSmoothing, double smoothFactor)
        {
            List<Point> nodes = null;//多边形内部与某一门点之间的路径节点集,首点为门点，尾点为多边形内部抽象点，方向为从门点至抽象点
            Line dirLine = null;
            Point nearestIntersection = null;
            Line dirSide = null;
            Point currentStartPoint = null;
            bool flag = false;//标识当前点与抽象点是否连通（中间无障碍）

            nodes = new List<Point>() { doorPoint };

            currentStartPoint = doorPoint;
            do
            {
                nearestIntersection = null;
                dirSide = null;
                flag = visualTest(true, currentStartPoint, functionAreaPoint, out nearestIntersection, out dirSide, polygon.Lines);


                if (flag)
                {

                    nodes.Add(functionAreaPoint);
                }
                else
                {



                    dirLine = GeometryHelper.Generate_ParallelLine(currentStartPoint, dirSide);
                    Point p1 = null;
                    Line side = null;
                    visualTest(false, dirLine.StartPoint, dirLine.EndPoint, out p1, out side, polygon.Lines);


                    if (p1 == null) throw new Exception("未能计算出线-边交点！");


                    double distance1 = GeometryHelper.Calc_Distance(p1, dirSide.StartPoint);
                    double distance2 = GeometryHelper.Calc_Distance(p1, dirSide.EndPoint);

                    Point p2 = null;
                    if (distance1 < distance2)
                        p2 = dirSide.StartPoint;
                    else
                        p2 = dirSide.EndPoint;


                    Point p = (new Line(p1, p2)).Get_MiddlePoint();


                    nodes.Add(p);
                    currentStartPoint = p;
                }
            } while (!flag);

            // 对生成的路径进行平滑化处理
            if (enableSmoothing)
            {
                return smoothPath(nodes, polygon, smoothFactor);
            }
            
            return nodes;
        }

        /// <summary>
        /// 重载方法：保持向后兼容
        /// </summary>
        static List<Point> calc_RouteNodes_FromOneDoor(Point doorPoint, Point functionAreaPoint, Polygon polygon)
        {
            return calc_RouteNodes_FromOneDoor(doorPoint, functionAreaPoint, polygon, true, 0.3);
        }




        /// <summary>
        /// 两点之间的可视化测试
        /// </summary>
        /// <param name="requireIntersectionInDirLine">是否要求交点必须严格位于两点连线上</param>
        /// <param name="line"></param>
        /// <param name="nearestIntersection"></param>
        /// <param name="nearestIntersectingSide"></param>
        /// <param name="sides"></param>
        /// <returns>若可视则返回true,否则为false</returns>
        static bool visualTest(bool requireIntersectionInDirLine, Point startPoint, Point endPoint, out Point nearestIntersection, out Line nearestIntersectingSide, List<Line> sides)
        {
            Line line = new Line(startPoint, endPoint);


            List<Line> intersectingSides = new List<Line>();//相交边集
            List<Point> intersections = new List<Point>();//交点集


            Point intersection = null;
            if (requireIntersectionInDirLine)
            {

                foreach (var item in sides)
                {
                    if (GeometryHelper.Calc2D_Intersection(line, item, out intersection) == "有唯一交点"
                        && !intersection.Equals(line.StartPoint)
                        && !intersection.Equals(line.EndPoint)
                        && GeometryHelper.BasicRelation_2D(line, intersection) == Relation2D_Point_Line.InLine
                        && GeometryHelper.BasicRelation_2D(item, intersection) == Relation2D_Point_Line.InLine
                        )
                    {
                        intersectingSides.Add(item);
                        intersections.Add(intersection);
                    }
                }
            }
            else
            {

                foreach (var item in sides)
                {
                    if (GeometryHelper.Calc2D_Intersection(line, item, out intersection) == "有唯一交点"
                        && !intersection.Equals(line.StartPoint)
                        && !intersection.Equals(line.EndPoint)
                        && GeometryHelper.BasicRelation_2D(item, intersection) == Relation2D_Point_Line.InLine)
                    {
                        intersectingSides.Add(item);
                        intersections.Add(intersection);
                    }
                }
            }




            if (intersections.Count == 0)
            {
                nearestIntersectingSide = null;
                nearestIntersection = null;
                return true;
            }
            else
            {
                var points = (from p in intersections
                              orderby GeometryHelper.Calc_Distance(startPoint, p)
                              select p).ToList<Point>();

                nearestIntersection = points[0];
                int index = intersections.IndexOf(nearestIntersection);
                nearestIntersectingSide = intersectingSides[index];
                return false;
            }
        }
    }

}
