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
    public abstract class Route3DCreator
    {
        public const double RouteHeight = 100d;

        /// <summary>
        /// 创建首层楼层内部所有路径（边集+顶点集）
        /// </summary>
        /// <param name="floorPlan"></param>
        /// <param name="sides"></param>
        /// <param name="vertexes"></param>
        /// <param name="enableSmoothing">是否启用路径平滑化</param>
        /// <param name="smoothFactor">平滑因子(0-1)</param>
        public static void CreateFloorRoute(ref FloorPlan floorPlan, out List<IGeometry> sides, out List<IGeometry> vertexes, bool enableSmoothing = true, double smoothFactor = 0.3)
        {


            sides = new List<IGeometry>();
            vertexes = new List<IGeometry>();

            List<MathExtension.Geometry.Point> path = null;
            List<MathExtension.Geometry.Point> nodes = new List<MathExtension.Geometry.Point>();//存储路径的所有节点
            List<List<MathExtension.Geometry.Point>> paths = null;
            List<SemanticPolygon> sps = null;
            List<Stair> stairs = null;
            SemanticPolygon sp = null;
            Stair stair = null;
            IPolyline pPolyline = null;



         
            sps = (from p in floorPlan.PResult.SPolygons
                   where p.PType != PolygonType.Wall
                   && p.PType != PolygonType.Balustrade
                   && p.PType != PolygonType.Floor
                   && p.PType != PolygonType.StairRoom
                   && p.PType != PolygonType.ElevatorShaft
                   && p.DoorPoints.Count != 0
                   select p).ToList<SemanticPolygon>();

            if (sps != null)
            {
                if (sps.Count > 0)
                {
                    for (int i = 0; i < sps.Count; i++)
                    {
                        sp = sps[i];
                        paths = RE.RouteExtractor.Calc_CommonRoomRoutes(ref sp, enableSmoothing, smoothFactor);
                        if (paths == null) throw new Exception("生成普通房间内部路径数据失败：（0条路径）！");

                        for (int j = 0; j < paths.Count; j++)
                        {
                            path = paths[j];
                            nodes.AddRange(path);//收集节点

                            pPolyline = ConverterForEsri.Create_Polyline_FromPoints(ref path, false);

                            sides.Add(pPolyline);//收集边
                        }
                    }
                }
            }



       

            
            sps = (from p in floorPlan.PResult.SPolygons
                   where p.PType == PolygonType.StairRoom
                   select p).ToList<SemanticPolygon>();

            if (sps != null)
            {
                if (sps.Count != 0)
                {
                    stairs = floorPlan.PResult.Stairs;
                    if (stairs.Count != sps.Count)
                        throw new ArgumentException(string.Format("楼梯间与楼梯数目不等：(楼梯间：{0}\t楼梯：{1})", sps.Count, stairs.Count));

                    for (int i = 0; i < sps.Count; i++)
                    {
                        sp = sps[i];
                        stair = stairs[i];
                        paths = RE.RouteExtractor.Cacl_DoubleStairRoomNodes(ref sp, ref stair, floorPlan.Configuration.Floor_Thickness, enableSmoothing, smoothFactor);
                        for (int j = 0; j < paths.Count; j++)
                        {
                            path = paths[j];
                            nodes.AddRange(path);//收集节点

                            pPolyline = ConverterForEsri.Create_Polyline_FromPoints(ref path, false);

                            sides.Add(pPolyline);//收集边
                        }
                    }
                }
            }
        


            sps = (from p in floorPlan.PResult.SPolygons
                   where p.PType == PolygonType.ElevatorShaft
                   select p).ToList<SemanticPolygon>();

            if (sps != null)
            {
                if (sps.Count > 0)
                {
                    double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;
                    for (int i = 0; i < sps.Count; i++)
                    {
                        sp = sps[i];
                        paths = RE.RouteExtractor.Calc_ElevatorShaftNodes(ref sp, floorHeight, enableSmoothing, smoothFactor);
                        if (paths == null) throw new Exception("生成电梯井内部路径数据失败：（0条路径）！");

                        for (int j = 0; j < paths.Count; j++)
                        {
                            path = paths[j];
                            nodes.AddRange(path);//收集节点

                            pPolyline = ConverterForEsri.Create_Polyline_FromPoints(ref path, false);

                            sides.Add(pPolyline);//收集边
                        }
                    }
                }
            }

  

            
            List<MathExtension.Geometry.Point> doorPoints = RE.RouteExtractor.Calc_RoomLinkedPoints(ref floorPlan);
            nodes.AddRange(path);//收集节点
            for (int i = 0; i < doorPoints.Count; i += 2)
            {
                List<MathExtension.Geometry.Point> coupledNodes =
                    new List<MathExtension.Geometry.Point>() { doorPoints[i], doorPoints[i + 1] };

                pPolyline = EsriMapDataGenerator.GeometryCreator.ConverterForEsri.Create_Polyline_FromPoints(ref coupledNodes, false);
                sides.Add(pPolyline);//收集边
            }
    

           
            nodes.Distinct<MathExtension.Geometry.Point>();//剔除重复节点
            vertexes.AddRange(ConverterForEsri.Create_Points_FromPoints(ref nodes));
         
        }
    }
}
