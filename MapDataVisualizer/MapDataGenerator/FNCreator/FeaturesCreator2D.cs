using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCE.DataModels;
using DXFReader.DataModel;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using EsriMapDataGenerator.GeometryCreator;

namespace EsriMapDataGenerator.FNCreator
{
    internal abstract class FeaturesCreator2D
    {
        public static void CreateFeatures(ref FloorPlan floorPlan, ref IFeatureDataset pMapDataSet2D)
        {
   
            IFeatureClassContainer pFeatureClassContainer = pMapDataSet2D as IFeatureClassContainer;
            IFeatureClass pFeatureClass = null;

            #region 面要素
            // 创建楼板要素类
            pFeatureClass = pFeatureClassContainer.get_ClassByName("FloorRegions");
            create_Floors(ref pFeatureClass, ref floorPlan);

            //创建房间要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Rooms");
            create_Rooms(ref pFeatureClass, ref floorPlan);

            //创建楼梯间要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("StairRooms");
            if (floorPlan.PResult.Stairs != null)
            {
                if (floorPlan.PResult.Stairs.Count > 0)
                    create_StairRooms(ref pFeatureClass, ref floorPlan);
            }


            //创建电梯间要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("ElevatorShafts");
            if (floorPlan.PResult.Elevators != null)
            {
                if (floorPlan.PResult.Elevators.Count > 0)
                    create_ElevatorShafs(ref pFeatureClass, ref floorPlan);
            }

            #endregion

            #region 线要素
            //创建门线要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("DoorLines");

            if (floorPlan.PResult.DoorLines != null)
            {
                if (floorPlan.PResult.DoorLines.Count > 0)
                {
                    create_DoorLines(ref pFeatureClass, ref floorPlan);
                }
            }

            //创建窗线要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("WindowLines");
            if (floorPlan.PResult.WindowLines != null)
            {
                if (floorPlan.PResult.WindowLines.Count > 0)
                {
                    create_WindowLines(ref pFeatureClass, ref floorPlan);
                }
            }
            #endregion
        }



        #region 创建指定楼层内部要素的方法
        static void create_Floors(ref IFeatureClass pFC_floor, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            SemanticPolygon sp = (from p in floorPlan.PResult.SPolygons
                                  where p.PType == PolygonType.Floor
                                  select p).FirstOrDefault();

            if (sp == null)
            {
                // 如果没找到地板，直接返回或报错提示，防止后面代码空指针异常
                // MessageBox.Show("未找到类型为 Floor 的多边形，无法生成楼层！");
                return;
            }

            IClone originalGeometryClone = ConverterForEsri.Create_Polygon_FromPoints(sp.Points, 0) as IClone;


            //检索属性字段索引
            int fieldIndex_FloorIndex = pFC_floor.Fields.FindField("FloorIndex");
            int fieldIndex_Num_Door = pFC_floor.Fields.FindField("Num_Door");
            int fieldIndex_Num_Window = pFC_floor.Fields.FindField("Num_Window");

            int num_door = (from p in sp.Lines
                            where p.LType == LineType.DoorLine
                            select p).Count<DXFReader.DataModel.LineClass>();

            int num_window = (from p in sp.Lines
                              where p.LType == LineType.WindowLine
                              select p).Count<DXFReader.DataModel.LineClass>();




            IClone pClone = null;
            IFeature pFeature = null;
            double z = 0;
            for (int i = 1; i <= num_floors; i++)
            {
                pFeature = pFC_floor.CreateFeature();
                pFeature.set_Value(fieldIndex_FloorIndex, i);
                pFeature.set_Value(fieldIndex_Num_Door, num_door);
                pFeature.set_Value(fieldIndex_Num_Window, num_window);

                pClone = new PolygonClass();
                pClone.Assign(originalGeometryClone.Clone());
                z = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness + 0.1;
                (pClone as ITransform3D).Move3D(0, 0, z);
                pFeature.Shape = pClone as IGeometry;

                pFeature.Store();
            }
        }

        static void create_Rooms(ref IFeatureClass pFC_room, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType != PolygonType.Wall
                                         && p.PType != PolygonType.Balustrade
                                         && p.PType != PolygonType.Floor
                                         && p.PType != PolygonType.StairRoom
                                         && p.PType != PolygonType.ElevatorShaft
                                         select p).ToList<SemanticPolygon>();


            List<IClone> list_originalGeometryClone = new List<IClone>();
            List<int> list_num_doors = new List<int>();
            List<int> list_num_windows = new List<int>();


            foreach (var item in sps)
            {
                list_originalGeometryClone.Add(ConverterForEsri.Create_Polygon_FromPoints(item.Points, 0) as IClone);

                list_num_doors.Add((from p in item.Lines
                                    where p.LType == LineType.DoorLine
                                    select p).Count<DXFReader.DataModel.LineClass>());

                list_num_windows.Add((from p in item.Lines
                                      where p.LType == LineType.WindowLine
                                      select p).Count<DXFReader.DataModel.LineClass>());
            }







            //检索基本属性字段索引
            int fieldIndex_FloorIndex = pFC_room.Fields.FindField("FloorIndex");
            int fieldIndex_Num_Door = pFC_room.Fields.FindField("Num_Door");
            int fieldIndex_Num_Window = pFC_room.Fields.FindField("Num_Window");
            int fieldIndex_RegionType = pFC_room.Fields.FindField("RegionType");

            double z = 0;
            IClone pClone = null;
            for (int i = 1; i <= num_floors; i++)
            {
                z = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness + 0.2;
                for (int j = 0; j < list_originalGeometryClone.Count; j++)
                {
                    IFeature pFeature = pFC_room.CreateFeature();
                    pFeature.set_Value(fieldIndex_FloorIndex, i);
                    pFeature.set_Value(fieldIndex_Num_Door, list_num_doors[j]);
                    pFeature.set_Value(fieldIndex_Num_Window, list_num_windows[j]);
                    pFeature.set_Value(fieldIndex_RegionType, sps[j].PType.ToString());


                    pClone = new PolygonClass();
                    pClone.Assign(list_originalGeometryClone[j].Clone());
                    (pClone as ITransform3D).Move3D(0, 0, z);
                    pFeature.Shape = pClone as IGeometry;

                    pFeature.Store();
                }
            }
        }


        static void create_StairRooms(ref IFeatureClass pFC_stairRoom, ref FloorPlan floorPlan)
        {
            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType == PolygonType.StairRoom
                                         select p).ToList<SemanticPolygon>();

            if (sps.Count == 0) return;

            if (floorPlan.PResult.Stairs.Count != sps.Count)
                throw new ArgumentException("电梯间与电梯数目不等！");


            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;



            List<IClone> list_originalGeometryClone = new List<IClone>();
            List<int> list_num_doors = new List<int>();
            List<int> list_num_windows = new List<int>();


            foreach (var item in sps)
            {
                list_originalGeometryClone.Add(ConverterForEsri.Create_Polygon_FromPoints(item.Points, 0) as IClone);

                list_num_doors.Add((from p in item.Lines
                                    where p.LType == LineType.DoorLine
                                    select p).Count<DXFReader.DataModel.LineClass>());

                list_num_windows.Add((from p in item.Lines
                                      where p.LType == LineType.WindowLine
                                      select p).Count<DXFReader.DataModel.LineClass>());
            }

            //检索基本属性字段索引
            int fieldIndex_FloorIndex = pFC_stairRoom.Fields.FindField("FloorIndex");
            int fieldIndex_Num_Door = pFC_stairRoom.Fields.FindField("Num_Door");
            int fieldIndex_Num_Window = pFC_stairRoom.Fields.FindField("Num_Window");
            int fieldIndex_Num_Steps = pFC_stairRoom.Fields.FindField("Num_Steps");
            int fieldIndex_Height_Step = pFC_stairRoom.Fields.FindField("Height_Step");
            int fieldIndex_Width_Step = pFC_stairRoom.Fields.FindField("Width_Step");
            int fieldIndex_Width_Land = pFC_stairRoom.Fields.FindField("Width_Land");
            int fieldIndex_Width_Staircase = pFC_stairRoom.Fields.FindField("Width_Staircase");
            int fieldIndex_Width_Stairwell = pFC_stairRoom.Fields.FindField("Width_Stairwell");
            int fieldIndex_Width_StairWay = pFC_stairRoom.Fields.FindField("Width_StairWay");
            int fieldIndex_UpstairPosition = pFC_stairRoom.Fields.FindField("UpstairPosition");


            for (int i = 1; i <= num_floors; i++)
            {
                double z = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness + 0.2;
                for (int j = 0; j < list_originalGeometryClone.Count; j++)
                {
                    Stair s = floorPlan.PResult.Stairs[j];

                    IFeature pFeature = pFC_stairRoom.CreateFeature();
                    pFeature.set_Value(fieldIndex_FloorIndex, i);
                    pFeature.set_Value(fieldIndex_Num_Door, list_num_doors[j]);
                    pFeature.set_Value(fieldIndex_Num_Window, list_num_windows[j]);
                    pFeature.set_Value(fieldIndex_Num_Steps, s.StepNum);
                    pFeature.set_Value(fieldIndex_Height_Step, s.Height_Step);
                    pFeature.set_Value(fieldIndex_Width_Step, s.Width_Step);
                    pFeature.set_Value(fieldIndex_Width_Land, s.Width_Land);
                    pFeature.set_Value(fieldIndex_Width_Staircase, s.Width_Staircase);
                    pFeature.set_Value(fieldIndex_Width_Stairwell, s.Width_Stairwell);
                    pFeature.set_Value(fieldIndex_Width_StairWay, s.Width_Stairway);
                    pFeature.set_Value(fieldIndex_UpstairPosition, s.UpstairPosition.ToString());


                    IClone pClone = new PolygonClass();
                    pClone.Assign(list_originalGeometryClone[j].Clone());
                    (pClone as ITransform3D).Move3D(0, 0, z);
                    pFeature.Shape = pClone as IGeometry;

                    pFeature.Store();
                }
            }
        }


        static void create_ElevatorShafs(ref IFeatureClass pFC_elevatorShaft, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType == PolygonType.ElevatorShaft
                                         select p).ToList<SemanticPolygon>();

            List<IClone> list_originalGeometryClone = new List<IClone>();
            List<int> list_num_doors = new List<int>();
            List<int> list_num_windows = new List<int>();


            foreach (var item in sps)
            {
                list_originalGeometryClone.Add(ConverterForEsri.Create_Polygon_FromPoints(item.Points, 0) as IClone);

                list_num_doors.Add((from p in item.Lines
                                    where p.LType == LineType.DoorLine
                                    select p).Count<DXFReader.DataModel.LineClass>());

                list_num_windows.Add((from p in item.Lines
                                      where p.LType == LineType.WindowLine
                                      select p).Count<DXFReader.DataModel.LineClass>());
            }

            //检索基本属性字段索引
            int fieldIndex_FloorIndex = pFC_elevatorShaft.Fields.FindField("FloorIndex");
            int fieldIndex_Num_Door = pFC_elevatorShaft.Fields.FindField("Num_Door");

            for (int i = 1; i <= num_floors; i++)
            {
                double z = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness + 0.2;
                for (int j = 0; j < list_originalGeometryClone.Count; j++)
                {
                    //Stair s = floorPlan.PResult.Stairs[j];

                    IFeature pFeature = pFC_elevatorShaft.CreateFeature();
                    pFeature.set_Value(fieldIndex_FloorIndex, i);
                    pFeature.set_Value(fieldIndex_Num_Door, list_num_doors[j]);

                    IClone pClone = new PolygonClass();
                    pClone.Assign(list_originalGeometryClone[j].Clone());
                    (pClone as ITransform3D).Move3D(0, 0, z);
                    pFeature.Shape = pClone as IGeometry;

                    pFeature.Store();
                }
            }
        }

        static void create_DoorLines(ref IFeatureClass pFC_doorLine, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;
            List<DXFReader.DataModel.LineClass> lines = floorPlan.PResult.DoorLines;

            //检索属性字段索引
            int fieldIndex_FloorIndex = pFC_doorLine.Fields.FindField("FloorIndex");
            for (int i = 1; i <= num_floors; i++)
            {
                double z = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness + 0.2;
                for (int j = 0; j < lines.Count; j++)
                {
                    IFeature pFeature = pFC_doorLine.CreateFeature();
                    pFeature.set_Value(fieldIndex_FloorIndex, i);
                    pFeature.Shape = ConverterForEsri.Create_Polyline_FromLine(lines[j], z);

                    pFeature.Store();
                }
            }
        }

        static void create_WindowLines(ref IFeatureClass pFC_windowLine, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;
            List<DXFReader.DataModel.LineClass> lines = floorPlan.PResult.WindowLines;

            //检索属性字段索引
            int fieldIndex_FloorIndex = pFC_windowLine.Fields.FindField("FloorIndex");
            for (int i = 1; i <= num_floors; i++)
            {
                double z = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness + 0.2;
                for (int j = 0; j < lines.Count; j++)
                {
                    IFeature pFeature = pFC_windowLine.CreateFeature();
                    pFeature.set_Value(fieldIndex_FloorIndex, i);
                    pFeature.Shape = ConverterForEsri.Create_Polyline_FromLine(lines[j], z);

                    pFeature.Store();
                }
            }
        }

        #endregion



    }
}
