using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcObjectsUtilities;
using BCE.DataModels;
using DXFReader.DataModel;
using ESRI.ArcGIS.Analyst3DTools;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;

namespace EsriMapDataGenerator.GeometryCreator
{
    public abstract class FrameGeometryCreator
    {
        const double holeWidth_append = 20d;//为确保门窗洞能够穿透墙面而增加的洞宽附加值

        #region 创建 3D楼板
        /// <summary>
        /// 创建3D 墙体的Geometry
        /// </summary>
        /// <param name="floorPlan"></param>
        /// <returns></returns>
        public static IGeometry Create_FloorGeometry3D(ref FloorPlan floorPlan)
        {

            //楼板截面
            SemanticPolygon sp = (from p in floorPlan.PResult.SPolygons
                                  where p.PType == PolygonType.Floor
                                  select p).First<SemanticPolygon>();


            IGeometry pGeometry = ConverterForEsri.Create_Polygon_FromPoints(sp.Points, 0);
            ITopologicalOperator pTopological = pGeometry as ITopologicalOperator;

 
            List<IGeometry> stairRoom_realSections = create_stairRoom_realSections(ref floorPlan);
            if (stairRoom_realSections != null)
            {
                foreach (var item in stairRoom_realSections)
                {
                    pTopological = pTopological.Difference(item) as ITopologicalOperator;
                }
            }


          
            List<SemanticPolygon> elevatorShaft_sps = (from p in floorPlan.PResult.SPolygons
                                                       where p.PType == PolygonType.ElevatorShaft
                                                       select p).ToList<SemanticPolygon>();

            if (elevatorShaft_sps.Count != 0)
            {
                foreach (var item in elevatorShaft_sps)
                {
                    pTopological = 
                        pTopological.Difference(ConverterForEsri.Create_Polygon_FromPoints(item.Points, 0)) as ITopologicalOperator;
                }
            }


            #region 创建 3D 楼板
            IVector3D pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, floorPlan.Configuration.Floor_Thickness);
            IGeometry pGeometry3D = GeometryUtility.Construct_StretchedBody(pTopological as IGeometry, pExtrudeVector);
            #endregion

            return pGeometry3D;
        }

        //收缩楼板截面
        static void shrinkSemanticPolygon(ref SemanticPolygon sp, double shrinkValue)
        {
            //确定多边形的几何中心
            double[] Xs = (from p in sp.Points
                           orderby p.X
                           select p.X).ToArray<double>();

            double[] Ys = (from p in sp.Points
                           orderby p.Y
                           select p.Y).ToArray<double>();

            double centreX = (Xs[0] + Xs[Xs.Length - 1]) / 2;
            double centreY = (Ys[0] + Ys[Ys.Length - 1]) / 2;


            //收缩多边形的几何边界
            MathExtension.Vector.Vector v = null;
            double angleToAxis_X = 0;
            double angleToAxis_Y = 0;
            MathExtension.Vector.Vector V_XAxis = new MathExtension.Vector.Vector(1, 0, 0);
            MathExtension.Vector.Vector V_YAxis = new MathExtension.Vector.Vector(0, 1, 0);
            foreach (var item in sp.Points)
            {
                v = new MathExtension.Vector.Vector(item.X - centreX, item.Y - centreY, 0);

                angleToAxis_X = MathExtension.Vector.VectorHelper.Calc_Angle(v, V_XAxis);
                angleToAxis_Y = MathExtension.Vector.VectorHelper.Calc_Angle(v,V_YAxis);

                //收缩X坐标
                if (angleToAxis_X < 90d)
                    item.X -= shrinkValue;
                else
                    item.X += shrinkValue;

                //收缩Y坐标
                if (angleToAxis_Y < 90d)
                    item.Y -= shrinkValue;
                else
                    item.Y += shrinkValue;
            }

            sp = new SemanticPolygon(sp.Points);
        }


        //创建楼梯间真实界面(原始楼梯间截面偏大)
        static List<IGeometry> create_stairRoom_realSections(ref FloorPlan floorPlan)
        {
            if (floorPlan.PResult.Stairs.Count == 0) return null;


            List<IGeometry> stairRoom_sections = new List<IGeometry>();
            IPoint pOrigin = GeometryUtility.Construct_Point3D(0, 0, 0);
            double width_staircase;
            double depth_stair;
            IGeometry pRectangle_section = null;
            double angle = 0d;

            foreach (var item in floorPlan.PResult.Stairs)
            {
                angle = MathExtension.Unit.UnitHelper.Get_AngleInRadians(item.RotateAngle);
                width_staircase = item.Width_Staircase;
                depth_stair = item.Width_Land + item.Width_Step * item.StepNum / 2;//台阶数StepNum为不含休息平台在内的双梯段台阶数
                pRectangle_section = GeometryUtility.Construct_Rectangle(pOrigin, width_staircase, depth_stair, angle);

                //计算OCS下旋转后的定位点坐标
                IPoint pInsertPoint_OCS = GeometryUtility.Construct_Point3D(0, depth_stair, 0);
                (pInsertPoint_OCS as ITransform2D).Rotate(pOrigin, angle);

                //将截面移动到定位点
                (pRectangle_section as ITransform3D).Move3D(item.InsertPoint.X - pInsertPoint_OCS.X,
                    item.InsertPoint.Y - pInsertPoint_OCS.Y,
                    0d);

                stairRoom_sections.Add(pRectangle_section);
            }

            return stairRoom_sections;
        }
        #endregion

        #region 拼装法创建 3D 墙体
        public static IGeometry CreateWallsGeometry_Composition(ref FloorPlan floorPlan)
        {
            //墙体
            List<IGeometry> walls = create_walls(ref floorPlan);

            //门洞上方墙体
            List<IGeometry> walls_upperDoor = create_doorHoleWalls(ref floorPlan);

            //门洞下方墙体
            List<IGeometry> walls_upper_lower_window = create_windowHolegWalls(ref floorPlan);

            //阳台栏杆实体
            List<IGeometry> balconyHandrails = create_balconyHandrail(ref floorPlan);

            //合并所有墙体
            if (walls_upperDoor != null)
                walls.AddRange(walls_upperDoor);

            if (walls_upper_lower_window != null)
                walls.AddRange(walls_upper_lower_window);

            if (balconyHandrails != null)
                walls.AddRange(balconyHandrails);

            ITopologicalOperator pTopologicalOperator = walls[0] as ITopologicalOperator;
            for (int i = 1; i < walls.Count; i++)
            {
                pTopologicalOperator = pTopologicalOperator.Union(walls[i]) as ITopologicalOperator;
            }

            return pTopologicalOperator as IGeometry;
        }

        #region 拼装法的子方法
        //创建所有提取的墙体（首层）
        static List<IGeometry> create_walls(ref FloorPlan floorPlan)
        {
            List<IGeometry> walls = new List<IGeometry>();

            IGeometry pGeometry_polygon = null;
            IGeometry pGeometry_3D = null;
            double floorHeight = floorPlan.Configuration.Floor_Headroom ;
            IVector3D pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, floorHeight);

            List<SemanticPolygon> sps_wall = (from p in floorPlan.PResult.SPolygons
                                              where p.PType == PolygonType.Wall
                                              select p).ToList<SemanticPolygon>();

            //创建墙实体
            for (int i = 0; i < sps_wall.Count; i++)
            {
                pGeometry_polygon = ConverterForEsri.Create_Polygon_FromPoints(sps_wall[i].Points,0);
                pGeometry_3D = GeometryUtility.Construct_StretchedBody(pGeometry_polygon, pExtrudeVector);
                walls.Add(pGeometry_3D);
            }

            return walls;
        }

        //创建所有门洞上下的墙体（首层）
        static List<IGeometry> create_doorHoleWalls(ref FloorPlan floorPlan)
        {
            List<IGeometry> walls = new List<IGeometry>();

            double extrudeHeight = floorPlan.Configuration.Floor_Headroom - floorPlan.Configuration.Door_Heigth;
            double moveHeight = floorPlan.Configuration.Door_Heigth;

            IVector3D extrudeVextor = ArcObjectsUtilities.GeometryUtility.Construct_Vector3D(0, 0, extrudeHeight);

            IPolygon pPolygonSection = null;//窗洞截面
            IGeometry wall = null;

            foreach (BCE.DataModels.Interface.IDoorWindow item in floorPlan.PResult.Doors)
            {
                pPolygonSection = create_DW_holeSection_WCS(item);

                //创建门顶至天花板之间的墙体
                wall = ArcObjectsUtilities.GeometryUtility.Construct_StretchedBody(pPolygonSection, extrudeVextor);
                (wall as ITransform3D).Move3D(0, 0, moveHeight);
                walls.Add(wall);
            }

            return walls;
        }

        //创建所有窗洞上下的墙体（首层）
        static List<IGeometry> create_windowHolegWalls(ref FloorPlan floorPlan)
        {
            List<IGeometry> walls = new List<IGeometry>();

            double windowHeight_toFloor = floorPlan.Configuration.Window_StartHeight + floorPlan.Configuration.Window_Heigth;
            double extrudeHeight_lower = floorPlan.Configuration.Window_StartHeight;
            double extrudeHeight_upper = floorPlan.Configuration.Floor_Headroom - windowHeight_toFloor;


            IVector3D v_bellowWall = ArcObjectsUtilities.GeometryUtility.Construct_Vector3D(0, 0, extrudeHeight_lower);
            IVector3D v_upperWall = ArcObjectsUtilities.GeometryUtility.Construct_Vector3D(0, 0, extrudeHeight_upper);
            IPolygon pPolygonSection = null;//窗洞截面
            IGeometry wall = null;
            foreach (BCE.DataModels.Interface.IDoorWindow item in floorPlan.PResult.Windows)
            {
                pPolygonSection = create_DW_holeSection_WCS(item);

                //创建地板至窗起始处之间的墙体
                wall = ArcObjectsUtilities.GeometryUtility.Construct_StretchedBody(pPolygonSection, v_bellowWall);
                walls.Add(wall);

                //创建窗顶至天花板之间的墙体
                wall = ArcObjectsUtilities.GeometryUtility.Construct_StretchedBody(pPolygonSection, v_upperWall);
                (wall as ITransform3D).Move3D(0, 0, windowHeight_toFloor);
                walls.Add(wall);
            }

            return walls;
        }

        //创建阳台栏杆实体
        static List<IGeometry> create_balconyHandrail(ref FloorPlan floorPlan)
        {
            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType == PolygonType.Balustrade
                                         select p).ToList<SemanticPolygon>();

            if (sps.Count == 0) return null;

            List<IGeometry> pGeometry_balustrades3D = new List<IGeometry>();
            IVector3D pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, floorPlan.Configuration.BalconyHandrail_Height);

            for (int i = 0; i < sps.Count; i++)
            {
                IPolygon pPolygon_section = ConverterForEsri.Create_Polygon_FromPoints(sps[i].Points, 0d);
                IGeometry balconyHandrail3D = GeometryUtility.Construct_StretchedBody(pPolygon_section, pExtrudeVector);
                pGeometry_balustrades3D.Add(balconyHandrail3D);
            }

            return pGeometry_balustrades3D;
        }


        //创建WCS下的门窗洞截面(Z=0)
        static IPolygon create_DW_holeSection_WCS(BCE.DataModels.Interface.IDoorWindow pDoorWindow)
        {
            //旋转角
            double rotateAngleInRadians = MathExtension.Unit.UnitHelper.Get_AngleInRadians(pDoorWindow.AngleToXAxis);

            //OCS下的插入点(已旋转)
            IPoint pInsertPoint_OCS = GeometryUtility.Construct_Point3D(0, pDoorWindow.Width / 2, 0);//旋转前
            (pInsertPoint_OCS as ITransform3D).RotateVector3D(GeometryUtility.Axis_Vector(3), rotateAngleInRadians);

            //创建窗洞横截面（已经旋转+平移）
            IPoint pOrigin = GeometryUtility.Construct_Point3D(0, 0, 0);
            IPolygon pPolygon = GeometryUtility.Construct_Rectangle(pOrigin, pDoorWindow.Length, pDoorWindow.Width, rotateAngleInRadians);//OCS下门窗洞截面
            (pPolygon as ITransform3D).Move3D(pDoorWindow.InsertPoint.X - pInsertPoint_OCS.X, pDoorWindow.InsertPoint.Y - pInsertPoint_OCS.Y, 0);//WCS下门窗洞截面

            return pPolygon;
        }
        #endregion
        #endregion

        #region 挖洞法创建 3D 墙体
        public static IGeometry CreateWallsGeometry_Difference(ref FloorPlan floorPlan, ref string parentDir_geoProcessFeatureClasses, ref string parentDir_outPut)
        {
 
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            #region 创建单层 3D墙
            SemanticPolygon sp = (from p in floorPlan.PResult.SPolygons
                                  where p.PType == PolygonType.Floor
                                  select p).First<SemanticPolygon>();

            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType != PolygonType.Wall
                                         && p.PType != PolygonType.Balustrade
                                         && p.PType != PolygonType.Floor
                                         select p).ToList<SemanticPolygon>();

            IGeometry pGeometry = ConverterForEsri.Create_Polygon_FromPoints(sp.Points, 0);
            ITopologicalOperator pTopological = pGeometry as ITopologicalOperator;


            //sps.Reverse();

            //step1: 挖去楼梯间+电梯间+各种房间

            int repeat = sps.Count;
            for (int i = 0; i < repeat; i++)
            {
                //if (i != 16)
                //{


                SemanticPolygon item = sps[i];
                pTopological = pTopological.Difference(ConverterForEsri.Create_Polygon_FromPoints(item.Points, 0)) as ITopologicalOperator;
                //}

            }


            //step2: 创建 3D墙体
            IVector3D pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, floorPlan.Configuration.Floor_Headroom);
            IGeometry pGeometry_wall = GeometryUtility.Construct_StretchedBody(pTopological as IGeometry, pExtrudeVector);


            //step3: 挖去门窗洞+阳台扶手上方墙体
            //创建门洞实体
            IGeometry pGeometry_doorHoles = create_DWHoleEntities(0, floorPlan);

            //创建窗洞实体
            IGeometry pGeometry_windowHoles = create_DWHoleEntities(1, floorPlan);



            //创建阳台扶手上方墙实体
            //IGeometry pGeometry_balustrade_diff = create_balustrades_difference(ref floorPlan);


            //step4: 从墙实体中扣去门窗洞和阳台扶手上方墙体
            //合并3个要扣去的实体
            pTopological = pGeometry_doorHoles as ITopologicalOperator;//任何建筑物肯定有门

            if (pGeometry_windowHoles != null)//建筑物可能不存在窗户
                pTopological = pTopological.Union(pGeometry_windowHoles) as ITopologicalOperator;

            //if (pGeometry_balustrade_diff != null)
            //    pTopological = pTopological.Union(pGeometry_balustrade_diff) as ITopologicalOperator;



            #region 检查闭合
            //将两几何封装成要素
            string parentDir_geoProcessFeatureClasses1 = @"C:\Users\silence\Desktop\新建文件夹 (2)\TempMapData2";
            IFeatureClass[] pcs = FrameGeometryCreator.Get_TwoFC_Fromfile(parentDir_geoProcessFeatureClasses1);

            IFeature f1 = pcs[0].CreateFeature();
            f1.Shape = pGeometry_wall as IGeometry;
            f1.Store();

            //IFeature f2 = pcs[0].CreateFeature();
            //f2.Shape = pConstructMultipatch2 as IGeometry;
            //f2.Store();


            ESRI.ArcGIS.Analyst3DTools.IsClosed3D ep = new IsClosed3D();
            ep.in_feature_class = pcs[0];

            string resultFullPath = @"C:\Users\silence\Desktop\新建文件夹 (4)\E";
            ep.output_feature_class = resultFullPath;
            //ep.grid_size = 10;

            //ESRI.ArcGIS.Analyst3DTools.EncloseMultiPatch ep2 = new EncloseMultiPatch(f2, pcs[1]);

            Geoprocessor gp = new Geoprocessor();
            IGeoProcessorResult results1 = (IGeoProcessorResult)gp.Execute(ep, null);
            //IGeoProcessorResult results2 = (IGeoProcessorResult)gp.Execute(ep2, null);

            for (int i = 0; i < results1.MessageCount; i++)
            {
                string a = results1.GetMessage(i);
            }


            IWorkspaceFactory pF = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactory();
            IFeatureWorkspace pS = pF.OpenFromFile(resultFullPath, 0) as IFeatureWorkspace;
            IFeatureClass pFeatureClass_diff = pS.OpenFeatureClass("E");
            IFeature pFeature3232 = null;
            if (pFeatureClass_diff.FeatureCount(null) == 0)
            {
                int B = 1;
            }
            else
            {
                pFeature3232 = pFeatureClass_diff.GetFeature(0);
            }

            pGeometry_wall = pFeature3232.Shape;
            #endregion




            //从墙体中扣除3个实体
            IMultiPatch opt1 = pGeometry_wall as IMultiPatch;
            IMultiPatch opt2 = pTopological as IMultiPatch;
            IGeometry pDiff = get_Difference(opt1, opt2, parentDir_geoProcessFeatureClasses, parentDir_outPut);

            return pDiff;


            //测试

            #endregion
        }





        //创建首层中所有门洞或窗洞实体
        static IGeometry create_DWHoleEntities(int doorOrWindowFlag, FloorPlan floorPlan)
        {
            double startHeight = 0;
            double extrudeHeight = 0;
            List<BCE.DataModels.Interface.IDoorWindow> list_dw = new List<BCE.DataModels.Interface.IDoorWindow>();

            switch (doorOrWindowFlag)
            {
                case 0:
                    startHeight = floorPlan.Configuration.Door_StartHeight;
                    extrudeHeight = floorPlan.Configuration.Door_Heigth;
                    foreach (BCE.DataModels.Interface.IDoorWindow item in floorPlan.PResult.Doors)
                        list_dw.Add(item);
                    break;

                case 1:
                    startHeight = floorPlan.Configuration.Window_StartHeight;
                    extrudeHeight = floorPlan.Configuration.Window_Heigth;
                    foreach (BCE.DataModels.Interface.IDoorWindow item in floorPlan.PResult.Windows)
                        list_dw.Add(item);
                    break;
                default:
                    throw new ArgumentException(string.Format("参数错误：请使用0或1以区分生成门洞或窗洞实体！"));
            }

            switch (list_dw.Count)
            {
                case 0:
                    return null;
                case 1:
                    return create_DWHoleEntity(list_dw[0], extrudeHeight, startHeight);
                default:
                    IGeometry pGeometry = create_DWHoleEntity(list_dw[0], extrudeHeight, startHeight);
                    ITopologicalOperator pTopologicalOperator = pGeometry as ITopologicalOperator;
                    for (int i = 1; i < list_dw.Count; i++)
                    {
                        pGeometry = create_DWHoleEntity(list_dw[i], extrudeHeight, startHeight);
                        pTopologicalOperator = pTopologicalOperator.Union(pGeometry) as ITopologicalOperator;
                    }
                    return pTopologicalOperator as IGeometry;
            }
        }

        //创建单个门或窗洞实体
        static IGeometry create_DWHoleEntity(BCE.DataModels.Interface.IDoorWindow pDoorWindow, double extrudeHeight, double startHeight)
        {
            //拉伸矢量
            IVector3D pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, extrudeHeight);

            //旋转角
            double rotateAngleInRadians = MathExtension.Unit.UnitHelper.Get_AngleInRadians(pDoorWindow.AngleToXAxis);

            //创建窗洞横截面（已经旋转+平移）
            IPoint pOrigin = GeometryUtility.Construct_Point3D(0, 0, 0);
            IPolygon pPolygon = GeometryUtility.Construct_Rectangle(pOrigin, pDoorWindow.Length, pDoorWindow.Width + holeWidth_append, rotateAngleInRadians);//OCS

            //模型坐标系下的插入点(已旋转)
            IPoint pInsertPoint = GeometryUtility.Construct_Point3D(0, (pDoorWindow.Width + holeWidth_append) / 2, 0);//旋转前
            (pInsertPoint as ITransform3D).RotateVector3D(GeometryUtility.Axis_Vector(3), rotateAngleInRadians);


            (pPolygon as ITransform3D).Move3D(pDoorWindow.InsertPoint.X - pInsertPoint.X, pDoorWindow.InsertPoint.Y - pInsertPoint.Y, startHeight);//WCS

            //创建门洞实体
            IGeometry pGeometry = ArcObjectsUtilities.GeometryUtility.Construct_StretchedBody(pPolygon, pExtrudeVector);

            return pGeometry;
        }



        //创建要减去的阳台扶手上方的墙体（单楼层）
        static IGeometry create_balustrades_difference(ref FloorPlan floorPlan)
        {
            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType != PolygonType.Balustrade
                                         select p).ToList<SemanticPolygon>();


            if (sps.Count == 0)
                return null;
            else
            {
                //阳台扶手顶面至楼顶的距离
                double height_diff = floorPlan.Configuration.Floor_Headroom - floorPlan.Configuration.BalconyHandrail_Height;

                IVector3D pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, height_diff);
                IPolygon pPolygon = ConverterForEsri.Create_Polygon_FromPoints(sps[0].Points, floorPlan.Configuration.BalconyHandrail_Height);
                IGeometry pGeometry = ArcObjectsUtilities.GeometryUtility.Construct_StretchedBody(pPolygon, pExtrudeVector);
                if (sps.Count == 1)
                    return pGeometry;
                else
                {
                    ITopologicalOperator pTopologicalOperator = pGeometry as ITopologicalOperator;
                    for (int i = 1; i < sps.Count; i++)
                    {
                        pPolygon = ConverterForEsri.Create_Polygon_FromPoints(sps[i].Points, 0);
                        pGeometry = GeometryUtility.Construct_StretchedBody(pPolygon, pExtrudeVector);
                        pTopologicalOperator = pTopologicalOperator.Union(pGeometry) as ITopologicalOperator;
                    }
                    return pTopologicalOperator as IGeometry;
                }
            }
        }



        //门

        //窗

        //创建要减去的阳台扶手上方的墙体（单楼层）
        //static IGeometry create_balustrades_difference(ref FloorPlan floorPlan)
        //{
        //    List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
        //                                 where p.PType != PolygonType.Balustrade
        //                                 select p).ToList<SemanticPolygon>();


        //    if (sps.Count == 0)
        //        return null;
        //    else
        //    {
        //        //阳台扶手顶面至楼顶的距离
        //        double height_diff = floorPlan.Configuration.Floor_Headroom - floorPlan.Configuration.BalconyHandrail_Height;

        //        IVector3D pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, height_diff);
        //        IPolygon pPolygon = DataTransverter.Create_Polygon_FromLines(sps[0].Lines, floorPlan.Configuration.BalconyHandrail_Height);
        //        IGeometry pGeometry = ArcObjectsUtilities.GeometryUtility.Construct_StretchedBody(pPolygon, pExtrudeVector);
        //        if (sps.Count == 1)
        //            return pGeometry;
        //        else
        //        {
        //            ITopologicalOperator pTopologicalOperator = pGeometry as ITopologicalOperator;
        //            for (int i = 1; i < sps.Count; i++)
        //            {
        //                pPolygon = DataTransverter.Create_Polygon_FromLines(sps[i].Lines, 0);
        //                pGeometry = GeometryUtility.Construct_StretchedBody(pPolygon, pExtrudeVector);
        //                pTopologicalOperator = pTopologicalOperator.Union(pGeometry) as ITopologicalOperator;
        //            }
        //            return pTopologicalOperator as IGeometry;
        //        }
        //    }
        //}

        //从现有shp文件创建两个要素类，用于包装要进行地理处理的要素
       public static IGeometry get_Difference(IMultiPatch pMultipatch_menuend, IMultiPatch pMultipatch_subtrahend, string parentDir_geoProcessFeatureClasses, string parentDir_outPut)
        {
            IWorkspaceFactory pworkspaceFactory = null;
            IFeatureWorkspace pFeatureWorkspace = null;
            IFeatureClass opt_FC1 = null;
            IFeatureClass opt_FC2 = null;

            #region 从现有shp文件创建两个要素类好，并将要进行差异分析的IMultipatch封装成IFeature
            pworkspaceFactory = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = pworkspaceFactory.OpenFromFile(parentDir_geoProcessFeatureClasses, 0) as IFeatureWorkspace;

            opt_FC1 = pFeatureWorkspace.OpenFeatureClass("Opt1");
            opt_FC2 = pFeatureWorkspace.OpenFeatureClass("Opt2");


            //清空要素类中的要素
            IFeatureCursor pFeatureCursor = opt_FC1.Search(null, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            while (pFeature != null)
            {
                pFeature.Delete();
                pFeature = pFeatureCursor.NextFeature();
            }

            pFeatureCursor = opt_FC2.Search(null, false);
            pFeature = pFeatureCursor.NextFeature();
            while (pFeature != null)
            {
                pFeature.Delete();
                pFeature = pFeatureCursor.NextFeature();
            }

            //将IMultipatch封装成IFeature
            pFeature = opt_FC1.CreateFeature();
            pFeature.Shape = pMultipatch_menuend as IGeometry;
            pFeature.Store();

            pFeature = opt_FC2.CreateFeature();
            pFeature.Shape = pMultipatch_subtrahend as IGeometry;
            pFeature.Store();
            #endregion

            #region 差异分析
            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;//覆盖输出

            //差异分析
            ESRI.ArcGIS.Analyst3DTools.Difference3D differ = new ESRI.ArcGIS.Analyst3DTools.Difference3D();

            differ.in_features_minuend = opt_FC1;
            differ.in_features_subtrahend = opt_FC2;

            string resultFullPath = parentDir_outPut + "\\differ.shp";
            differ.out_feature_class = resultFullPath;

            IGeoProcessorResult results = (IGeoProcessorResult)gp.Execute(differ, null);

            if (results.Status != esriJobStatus.esriJobSucceeded)
                throw new InvalidOperationException("Failed to Difference\r\n");
            #endregion

            for (int i = 0; i < results.MessageCount; i++)
            {
                string a = results.GetMessage(i);
            }


            //return pRes;
            //提取相差处理结果
            IWorkspaceFactory pF = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactory();

            IFeatureWorkspace pS = pF.OpenFromFile(parentDir_outPut, 0) as IFeatureWorkspace;
            IFeatureClass pFeatureClass_diff = pS.OpenFeatureClass("differ");

            if (pFeatureClass_diff.FeatureCount(null) == 0)
                return null;
            else
            {
                pFeature = pFeatureClass_diff.GetFeature(0);
                return pFeature.Shape;
            }

        }

       public static IFeatureClass[] Get_TwoFC_Fromfile(string parentDir_geoProcessFeatureClasses)
       {
           IWorkspaceFactory pworkspaceFactory = null;
           IFeatureWorkspace pFeatureWorkspace = null;
           IFeatureClass opt_FC1 = null;
           IFeatureClass opt_FC2 = null;

           //从现有shp文件创建两个要素类好，并将要进行差异分析的IMultipatch封装成IFeature
           pworkspaceFactory = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
           pFeatureWorkspace = pworkspaceFactory.OpenFromFile(parentDir_geoProcessFeatureClasses, 0) as IFeatureWorkspace;

           opt_FC1 = pFeatureWorkspace.OpenFeatureClass("Opt1");
           opt_FC2 = pFeatureWorkspace.OpenFeatureClass("Opt2");


           //清空要素类中的要素
           IFeatureCursor pFeatureCursor = opt_FC1.Search(null, false);
           IFeature pFeature = pFeatureCursor.NextFeature();
           while (pFeature != null)
           {
               pFeature.Delete();
               pFeature = pFeatureCursor.NextFeature();
           }

           pFeatureCursor = opt_FC2.Search(null, false);
           pFeature = pFeatureCursor.NextFeature();
           while (pFeature != null)
           {
               pFeature.Delete();
               pFeature = pFeatureCursor.NextFeature();
           }

           IFeatureClass[] fcs = new IFeatureClass[2];
           fcs[0] = opt_FC1;
           fcs[1] = opt_FC2;

           return fcs;
       }
        
        #endregion
    }
}
