using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcObjectsUtilities;
using BCE.DataModels;
using DXFReader.DataModel;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using EsriMapDataGenerator.GeometryCreator;

namespace EsriMapDataGenerator.FNCreator
{
    internal abstract class FeaturesCreator3D
    {
        public static void CreateFeatures(ref FloorPlan floorPlan, ref IFeatureDataset pMapDataSet3D)
        {
            IFeatureClassContainer pFeatureClassContainer = pMapDataSet3D as IFeatureClassContainer;
            IFeatureClass pFeatureClass = null;
            IFeatureClass pFeatureClass2 = null;

            #region 修饰性要素
            //创建陆地要素(草坪)
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Land3D");
            createFeatures_land3D(ref  pFeatureClass, ref floorPlan);
            #endregion

            #region 恢复楼层框架
            //创建3D楼板要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Floor3D");
            createFeatures_floor3D(ref  pFeatureClass, ref floorPlan);

            //创建3D墙体要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Wall3D");
            createFeatures_Wall3D(ref  pFeatureClass, ref floorPlan);
            #endregion

            #region 创建并安装建筑附件（门窗+电梯+楼梯）
            //创建3D 楼梯要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Stair3D");
            if (floorPlan.PResult.Stairs != null)
            {
                if (floorPlan.PResult.Stairs.Count > 0)
                {
                    createFeatures_Stairs3D(ref pFeatureClass, ref floorPlan);
                }
            }

            //创建 3D 门要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Door3D");
            if (floorPlan.PResult.Doors != null)
            {
                if (floorPlan.PResult.Doors.Count > 0)
                {
                    createFeatures_Doors3D(ref pFeatureClass, ref floorPlan);
                }
            }



            //创建 3D 窗要素
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Window3D");
            if (floorPlan.PResult.Windows != null)
            {
                if (floorPlan.PResult.Windows.Count > 0)
                {
                    createFeatures_Windows3D(ref pFeatureClass, ref floorPlan);
                }
            }
            #endregion

            #region 创建室内路径的边+节点
            //创建路径(线要素+点要素)
            pFeatureClass = pFeatureClassContainer.get_ClassByName("Sides3D");
            pFeatureClass2 = pFeatureClassContainer.get_ClassByName("Nodes3D");
            createFeatures_Route3D(ref pFeatureClass, ref pFeatureClass2, ref floorPlan);
            #endregion
        }

        static void createFeatures_land3D(ref IFeatureClass pFC_Land3D, ref FloorPlan floorPlan)
        {
            const double grassSurfaceThickness = 50d;//草地表面厚度
            const double soilThickness = 500d;//土壤厚度
            const double moveDownOffset = 1d;
            double landLength = 0d;
            double landWidth = 0d;


            SemanticPolygon sp = (from p in floorPlan.PResult.SPolygons
                                  where p.PType == PolygonType.Floor
                                  select p).First<SemanticPolygon>();

            MathExtension.Geometry.Polygon polygon = new MathExtension.Geometry.Polygon(sp.Points);
            double[] xy_most = MathExtension.Geometry.GeometryHelper.Get_XYMost(polygon);

            //楼板多边形近似几何中心
            IPoint pCentre =
                ArcObjectsUtilities.GeometryUtility.Construct_Point3D((xy_most[0] + xy_most[1]) / 2, (xy_most[2] + xy_most[3]) / 2, 0d);

            //草地的尺寸（近似取楼板面积的16倍）
            landLength = (xy_most[1] - xy_most[0]) * 4;
            landWidth = (xy_most[3] - xy_most[2]) * 4;

            //创建草地表面几何体
            IGeometry pLandBottomSection =
                ArcObjectsUtilities.GeometryUtility.Construct_Rectangle_XYP(pCentre, landLength, landWidth, 0d);//草地底面

            IGeometry pGrassSurfaceGeometry =
                GeometryUtility.Construct_StretchedBody(pLandBottomSection, ArcObjectsUtilities.GeometryUtility.Construct_Vector3D(0, 0, grassSurfaceThickness));
            (pGrassSurfaceGeometry as ITransform3D).Move3D(0, 0, -(grassSurfaceThickness + moveDownOffset));


            //创建土壤几何体
            IGeometry pSoilGeometry =
                ArcObjectsUtilities.GeometryUtility.Construct_StretchedBody(pLandBottomSection, ArcObjectsUtilities.GeometryUtility.Construct_Vector3D(0, 0, soilThickness));
            (pSoilGeometry as ITransform3D).Move3D(0, 0, -(grassSurfaceThickness + soilThickness + moveDownOffset));



            //创建草地表面
            int fieldIndex_PartFlag = pFC_Land3D.Fields.FindField("PartFlag");
            IFeature pFeature = pFC_Land3D.CreateFeature();
            pFeature.set_Value(fieldIndex_PartFlag, 1);//1 标识草地表面
            pFeature.Shape = pGrassSurfaceGeometry;
            pFeature.Store();


            //创建土壤要素
            pFeature = pFC_Land3D.CreateFeature();
            pFeature.set_Value(fieldIndex_PartFlag, 2);//2 表示土壤
            pFeature.Shape = pSoilGeometry;
            pFeature.Store();
        }

        static void createFeatures_floor3D(ref IFeatureClass pFC_floor3D, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            //创建首层 3D楼板几何
            IGeometry pGeometry3D = FrameGeometryCreator.Create_FloorGeometry3D(ref floorPlan);

            //创建所有楼层的 3D楼板要素
            IClone originalGeometryClone = pGeometry3D as IClone;
            int fieldIndex_FloorIndex = pFC_floor3D.Fields.FindField("FloorIndex");
            for (int i = 1; i <= num_floors; i++)
            {
                double z = (i - 1) * floorHeight;
                IFeature pFeature = pFC_floor3D.CreateFeature();
                pFeature.set_Value(fieldIndex_FloorIndex, i);

                if (z == 0d)
                {
                    pFeature.Shape = pGeometry3D;
                }
                else
                {
                    IClone pClone = new MultiPatchClass();
                    pClone.Assign(originalGeometryClone.Clone());
                    (pClone as ITransform3D).Move3D(0, 0, z);
                    pFeature.Shape = pClone as IGeometry;


                }

                pFeature.Store();
            }
        }

        static void createFeatures_Wall3D(ref IFeatureClass pFC_Walls3D, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            //创建首层 3D墙几何
            IGeometry pGeometry3D = null;
            //try
            //{
            //    string parentDir_geoProcessFeatureClasses = @"C:\Users\silence\Desktop\新建文件夹 (2)\TempMapData";
            //    string parentDir_outPut = @"C:\Users\silence\Desktop\新建文件夹 (2)\diff";

            //    pGeometry3D = FrameGeometryCreator.CreateWallsGeometry_Difference(ref floorPlan,ref parentDir_geoProcessFeatureClasses,ref parentDir_outPut);
            //}
            //catch (Exception ex)
            //{
            //MessageBox.Show(string.Format("尝试使用挖洞法创建 3D墙体几何 失败：({0}！) \n将使用拼装法创建 3D墙体几何！", ex.Message),
            //    "", MessageBoxButton.OK, MessageBoxImage.Warning);
            pGeometry3D = FrameGeometryCreator.CreateWallsGeometry_Composition(ref floorPlan);
            //}



            //创建所有楼层的3D墙元素
            IClone originalGeometryClone = pGeometry3D as IClone;
            int fieldIndex_FloorIndex = pFC_Walls3D.Fields.FindField("FloorIndex");
            for (int i = 1; i <= num_floors; i++)
            {
                double z = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness;

                IFeature pFeature = pFC_Walls3D.CreateFeature();
                pFeature.set_Value(fieldIndex_FloorIndex, i);
                IClone pClone = new MultiPatchClass();
                pClone.Assign(originalGeometryClone.Clone());
                (pClone as ITransform3D).Move3D(0, 0, z);
                pFeature.Shape = pClone as IGeometry;

                pFeature.Store();
            }
        }

        static void createFeatures_Doors3D(ref IFeatureClass pFC_Doors3D, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            //检索基本属性字段索引
            int fieldIndex_FloorIndex = pFC_Doors3D.Fields.FindField("FloorIndex");
            int fieldIndex_Width = pFC_Doors3D.Fields.FindField("Width");
            int fieldIndex_Thickness = pFC_Doors3D.Fields.FindField("Thickness");
            int fieldIndex_Height = pFC_Doors3D.Fields.FindField("Height");
            int fieldIndex_Type = pFC_Doors3D.Fields.FindField("Type");
            int fieldIndex_PartFlag = pFC_Doors3D.Fields.FindField("PartFlag");

            List<IClone[]> pDoorClones = new List<IClone[]>();
            IClone[] perClones = null;

            const double rate = 0.7d;//子母门左门扇占整个门长度(扣除门框厚度)的比例
            const double doorThickness = 50d; //门厚度
            const double doorHeight = 2100d;//门高
            DoorParameters.P_SingleDoor sdp = null;
            DoorParameters.P_DMDoor dmdp = null;
            DoorParameters.P_EDoor edp = null;
            DoorParameters.P_SlidingDoor sli_dp = null;
            IGeometry pGeometryDoor = null;

            foreach (var door in floorPlan.PResult.Doors)
            {
                switch (door.Type)
                {
                    case MCType.M_SingleLeaf_Clockwise:
                        sdp = new DoorParameters.P_SingleDoor(true, door.Length, doorThickness, doorHeight);
                        pGeometryDoor = Door3DCreator.Create_SDoor(ref sdp);
                        perClones = new IClone[1];
                        perClones[0] = pGeometryDoor as IClone;
                        break;
                    case MCType.M_SingleLeaf_CounterClockwise:
                        sdp = new DoorParameters.P_SingleDoor(false, door.Length, doorThickness, doorHeight);
                        pGeometryDoor = Door3DCreator.Create_SDoor(ref sdp);
                        perClones = new IClone[1];
                        perClones[0] = pGeometryDoor as IClone;
                        break;

                    case MCType.M_SonMother_Clockwise:
                        dmdp = new DoorParameters.P_DMDoor(true, rate, door.Length, doorThickness, doorHeight);
                        pGeometryDoor = Door3DCreator.Create_DMSDoor(ref dmdp);
                        perClones = new IClone[1];
                        perClones[0] = pGeometryDoor as IClone;
                        break;
                    case MCType.M_SonMother_CounterClockwise:
                        dmdp = new DoorParameters.P_DMDoor(false, rate, door.Length, doorThickness, doorHeight);
                        pGeometryDoor = Door3DCreator.Create_DMSDoor(ref dmdp);
                        perClones = new IClone[1];
                        perClones[0] = pGeometryDoor as IClone;
                        break;

                    case MCType.M_DoubleLeaf:
                        dmdp = new DoorParameters.P_DMDoor(false, 0.5d, door.Length, doorThickness, doorHeight);
                        pGeometryDoor = Door3DCreator.Create_DMSDoor(ref dmdp);
                        perClones = new IClone[1];
                        perClones[0] = pGeometryDoor as IClone;
                        break;
                    case MCType.M_Sliding:
                        sli_dp = new DoorParameters.P_SlidingDoor(door.Length, door.Width, floorPlan.Configuration.Door_Heigth, 20d, 200d, 60d);
                        List<IGeometry> slidingDoors = Door3DCreator.Create_SlidingDoor(ref sli_dp);
                        perClones = new IClone[slidingDoors.Count];
                        for (int i = 0; i < slidingDoors.Count; i++)
                            perClones[i] = slidingDoors[i] as IClone;
                        break;

                    case MCType.M_ElevatorDoor:
                        edp = new DoorParameters.P_EDoor(door.Width, door.Length);
                        pGeometryDoor = Door3DCreator.Create_ElevatorDoor(ref edp);
                        perClones = new IClone[1];
                        perClones[0] = pGeometryDoor as IClone;
                        break;
                }
                pDoorClones.Add(perClones);
            }

            //创建所有楼层的3D墙元素
            double z_offset = 0;
            IFeature pFeature = null;
            IClone pClone = null;
            Door perDoor = null;
            ITransform3D pTransform3D = null;
            for (int i = 1; i <= num_floors; i++)
            {
                z_offset = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness;
                for (int j = 0; j < floorPlan.PResult.Doors.Count; j++)
                {
                    for (int k = 0; k < perClones.Length; k++)
                    {
                        perDoor = floorPlan.PResult.Doors[j];

                        pFeature = pFC_Doors3D.CreateFeature();
                        pFeature.set_Value(fieldIndex_FloorIndex, i);
                        pFeature.set_Value(fieldIndex_Width, perDoor.Length);
                        pFeature.set_Value(fieldIndex_Thickness, perDoor.Width);
                        pFeature.set_Value(fieldIndex_Height, perDoor.Height);
                        pFeature.set_Value(fieldIndex_Type, perDoor.Type.ToString());


                        //几何信息赋值
                        perClones = pDoorClones[j];


                        pClone = new MultiPatchClass();
                        pClone.Assign(perClones[k].Clone());
                        pTransform3D = pClone as ITransform3D;
                        pTransform3D.RotateVector3D(ArcObjectsUtilities.GeometryUtility.Axis_Vector(3), Math.PI * perDoor.AngleToXAxis / 180);
                        pTransform3D.Move3D(perDoor.InsertPoint.X, perDoor.InsertPoint.Y, z_offset);
                        pFeature.Shape = pClone as IGeometry;



                        if (k == 0)
                            pFeature.set_Value(fieldIndex_PartFlag, 0);
                        else
                        {
                            int mod = k % 2;
                            switch (mod)
                            {
                                case 0:
                                    pFeature.set_Value(fieldIndex_PartFlag, 2);
                                    break;
                                case 1:
                                    pFeature.set_Value(fieldIndex_PartFlag, 1);
                                    break;
                            }
                        }
                        pFeature.Store();
                    }
                }
            }
        }


        static void createFeatures_Windows3D(ref IFeatureClass pFC_Windows3D, ref FloorPlan floorPlan)
        {
            int num_floors = floorPlan.Configuration.RepeatCount;
            double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;

            //检索基本属性字段索引
            int fieldIndex_FloorIndex = pFC_Windows3D.Fields.FindField("FloorIndex");
            int fieldIndex_Width = pFC_Windows3D.Fields.FindField("Width");
            int fieldIndex_Thickness = pFC_Windows3D.Fields.FindField("Thickness");
            int fieldIndex_Height = pFC_Windows3D.Fields.FindField("Height");
            int fieldIndex_StartHeight = pFC_Windows3D.Fields.FindField("StartHeight");
            int fieldIndex_Type = pFC_Windows3D.Fields.FindField("Type");
            int fieldIndex_PartFlag = pFC_Windows3D.Fields.FindField("PartFlag");

            //构造首层所有窗几何体的参数
            List<IClone[]> pWindowClones = new List<IClone[]>();

            WindowParameters.P_GeneralWindow gwp = null;
            IClone[] pClones = null;
            IGeometry[] pGeometries = null;

            foreach (var window in floorPlan.PResult.Windows)
            {
                switch (window.Type)
                {
                    case MCType.Unknown:
                    case MCType.W_General:
                        gwp = new WindowParameters.P_GeneralWindow(floorPlan.Configuration.Window_StartHeight, window.Length, floorPlan.Configuration.Window_Heigth);
                        pGeometries = Window3DCreator.Create_GWindow(ref gwp);
                        pClones = new IClone[2];
                        pClones[0] = pGeometries[0] as IClone;
                        pClones[1] = pGeometries[1] as IClone;

                        break;
                    case MCType.W_Sliding:
                        pClones = null;
                        break;
                }

                pWindowClones.Add(pClones);
            }

            //创建所有楼层的3D窗元素
            double z_offset = 0;
            IFeature pFeature = null;
            IClone[] perClones = null;
            IClone perClone = null;
            BCE.DataModels.Window perWindow = null;
            ITransform3D pTransform3D = null;
            for (int i = 1; i <= num_floors; i++)
            {
                z_offset = (i - 1) * floorHeight + floorPlan.Configuration.Floor_Thickness;
                for (int j = 0; j < floorPlan.PResult.Windows.Count; j++)
                {

                    perClones = pWindowClones[j];
                    for (int k = 0; k < 2; k++)
                    {
                        perWindow = floorPlan.PResult.Windows[j];

                        pFeature = pFC_Windows3D.CreateFeature();
                        pFeature.set_Value(fieldIndex_FloorIndex, i);
                        pFeature.set_Value(fieldIndex_Width, perWindow.Length);
                        pFeature.set_Value(fieldIndex_Thickness, perWindow.Width);
                        pFeature.set_Value(fieldIndex_Height, perWindow.Height);
                        pFeature.set_Value(fieldIndex_StartHeight, floorPlan.Configuration.Window_StartHeight);
                        pFeature.set_Value(fieldIndex_Type, perWindow.Type.ToString());
                        pFeature.set_Value(fieldIndex_PartFlag, k + 1);


                        if (perClones != null)
                        {
                            perClone = new MultiPatchClass();
                            perClone.Assign(perClones[k].Clone());

                            pTransform3D = perClone as ITransform3D;
                            pTransform3D.RotateVector3D(ArcObjectsUtilities.GeometryUtility.Axis_Vector(3), Math.PI * perWindow.AngleToXAxis / 180);
                            pTransform3D.Move3D(perWindow.InsertPoint.X, perWindow.InsertPoint.Y, z_offset);

                            pFeature.Shape = perClone as IGeometry;
                        }

                        pFeature.Store();
                    }
                }
            }

        }


        static void createFeatures_Elevators3D()
        {

        }

        static void createFeatures_Stairs3D(ref IFeatureClass pFC_stair3D, ref FloorPlan fp)
        {
            int num_floors = fp.Configuration.RepeatCount;


            double floorHeight = fp.Configuration.Floor_Headroom + fp.Configuration.Floor_Thickness;

            //检索基本属性字段索引
            int fieldIndex_FloorIndex = pFC_stair3D.Fields.FindField("FloorIndex");
            int fieldIndex_Num_Steps = pFC_stair3D.Fields.FindField("Num_Steps");
            int fieldIndex_Height_Step = pFC_stair3D.Fields.FindField("Height_Step");
            int fieldIndex_Width_Step = pFC_stair3D.Fields.FindField("Width_Step");
            int fieldIndex_Width_Land = pFC_stair3D.Fields.FindField("Width_Land");
            int fieldIndex_Width_Staircase = pFC_stair3D.Fields.FindField("Width_Staircase");
            int fieldIndex_Width_Stairwell = pFC_stair3D.Fields.FindField("Width_Stairwell");
            int fieldIndex_Width_StairWay = pFC_stair3D.Fields.FindField("Width_StairWay");
            int fieldIndex_UpstairPosition = pFC_stair3D.Fields.FindField("UpstairPosition");
            int fieldIndex_PartFlag = pFC_stair3D.Fields.FindField("PartFlag");

            //逐楼层创建
            List<IGeometry> fourParts = null;

            StairParameters.DStairParameter dsp;
            List<Stair> stairs = fp.PResult.Stairs;
            Stair stair = null;
            double z = 0;
            for (int i = 0; i < num_floors; i++)
            {
                //创建单个楼层的所有楼梯
                for (int j = 0; j < stairs.Count; j++)
                {
                    stair = stairs[j];
     
                    if (fp.Configuration.Floor_Thickness >= stair.Height_Step)
                        z = floorHeight * (i + 0.5) + fp.Configuration.Floor_Thickness;
                    else
                        z = floorHeight * (i + 0.5) + stair.Height_Step;



                    dsp = ConverterForEsri.Get_DStairParameter(stair);
                    fourParts = Stair3DCreator.Create_DRunningStair(dsp);//生成休息平台、梯段、立柱、扶手四个部件

                    //生成四个要素，即每个双跑楼梯对应四个要素
                    for (int k = 0; k < fourParts.Count; k++)
                    {
                        IFeature pFeature = pFC_stair3D.CreateFeature();

                        //基本属性信息赋值
                        pFeature.set_Value(fieldIndex_FloorIndex, i);
                        pFeature.set_Value(fieldIndex_Num_Steps, stair.StepNum);
                        pFeature.set_Value(fieldIndex_Height_Step, stair.Height_Step);
                        pFeature.set_Value(fieldIndex_Width_Step, stair.Width_Step);
                        pFeature.set_Value(fieldIndex_Width_Land, stair.Width_Land);
                        pFeature.set_Value(fieldIndex_Width_Staircase, stair.Width_Staircase);
                        pFeature.set_Value(fieldIndex_Width_Stairwell, stair.Width_Stairwell);
                        pFeature.set_Value(fieldIndex_Width_StairWay, stair.Width_Stairway);
                        pFeature.set_Value(fieldIndex_UpstairPosition, stair.UpstairPosition.ToString());
                        pFeature.set_Value(fieldIndex_PartFlag, k + 1);

                        //几何信息赋值
                        IClone pClone = new MultiPatchClass();
                        pClone.Assign((fourParts[k] as IClone).Clone());
                        (pClone as ITransform3D).Move3D(stair.InsertPoint.X, stair.InsertPoint.Y, z);
                        pFeature.Shape = pClone as IGeometry;

                        pFeature.Store();
                    }
                }
            }
        }


        static void createFeatures_Route3D(ref IFeatureClass pFC_Sides3D, ref IFeatureClass pFC_Nodes3D, ref FloorPlan fp)
        {
            int num_floors = fp.Configuration.RepeatCount;
            double floorHeight = fp.Configuration.Floor_Headroom + fp.Configuration.Floor_Thickness;

            List<IGeometry> sides = null;
            List<IGeometry> nodes = null;
            List<IClone> pClone_sides = new List<IClone>();
            List<IClone> pClone_nodes = new List<IClone>();
            IClone pClone = null;
            IFeature pFeature = null;
            double z_heightToFloor = 50;
            double z_offset = 0d;


            Route3DCreator.CreateFloorRoute(ref fp, out sides, out nodes);

            foreach (var item in sides)
                pClone_sides.Add(item as IClone);

            foreach (var item in nodes)
                pClone_nodes.Add(item as IClone);



            for (int i = 1; i <= num_floors; i++)
            {
                z_offset = fp.Configuration.Floor_Thickness + (i - 1) * floorHeight + z_heightToFloor;

                //边
                foreach (IClone item in pClone_sides)
                {
                    pClone = new PolylineClass();
                    pClone.Assign(item.Clone());
                    (pClone as ITransform3D).Move3D(0d, 0d, z_offset);

                    //创建边要素
                    pFeature = pFC_Sides3D.CreateFeature();
                    pFeature.Shape = pClone as IGeometry;
                    pFeature.Store();
                }

                //节点
                foreach (IClone item in pClone_nodes)
                {
                    pClone = new PointClass();
                    pClone.Assign(item.Clone());
                    (pClone as ITransform3D).Move3D(0d, 0d, z_offset);

                    //创建要素
                    pFeature = pFC_Nodes3D.CreateFeature();
                    pFeature.Shape = pClone as IGeometry;
                    pFeature.Store();
                }
            }
        }


    }
}
