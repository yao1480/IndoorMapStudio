using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BCE.DataModels;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using EsriMapDataGenerator;
using EsriMapDataGenerator.FNCreator;


namespace EsriMapDataGenerator
{
    public abstract class DataGenerator
    {
        const string networkEdgeSourceName = "Sides3D";//用于构造网络边的要素名

        #region 要素+网络数据集+符号化图层生成方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="floorPlan"></param>
        /// <param name="pMapDataset2D">承载2D MapData地图数据的要素数据集</param>
        /// <param name="pMapDataset3D">承载3D MapData地图数据的要素数据集</param>
        /// <param name="networkDatasetName">路径网络数据集名称</param>
        /// <param name="edgeSourceName">用于生成路径网络边源的要素类名称</param>
        public static void Generate_EsriData(ref FloorPlan floorPlan, ref IFeatureDataset pMapDataset2D, ref IFeatureDataset pMapDataset3D, string networkDatasetName, string edgeSourceName)
        {
            try
            {
                if (FGDBHelper.IsIntialMapDataset(ref pMapDataset2D))
                {
                    FeaturesCreator2D.CreateFeatures(ref floorPlan, ref pMapDataset2D);
                }
                else
                {
                    if (MessageBoxResult.Yes == MessageBox.Show("已生成Esri MapDataset2D地图数据，是否要清除当前地图数据并重新创建？", "", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                    {
                        FGDBHelper.ResetMapDataset2D(ref pMapDataset2D);
                        FeaturesCreator2D.CreateFeatures(ref floorPlan, ref pMapDataset2D);
                    }
                }

                if (FGDBHelper.IsIntialMapDataset(ref pMapDataset3D))
                {
                    FeaturesCreator3D.CreateFeatures(ref floorPlan, ref pMapDataset3D);
                    RouteNetworkCreator.CreateNetDataSet(ref pMapDataset3D, networkDatasetName, edgeSourceName);
                }
                else
                {
                    if (MessageBoxResult.Yes == MessageBox.Show("已生成Esri地图数据，是否要清除当前地图数据并重新创建？", "", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                    {
                        FGDBHelper.ResetMapDataset3D(ref pMapDataset3D);

                        FeaturesCreator3D.CreateFeatures(ref floorPlan, ref pMapDataset3D);
                        RouteNetworkCreator.CreateNetDataSet(ref pMapDataset3D, networkDatasetName, edgeSourceName);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("生成EsriData失败： {0}！", ex.Message));
            }
        }

        /// <summary>
        /// 创建2D 符号化图层（
        /// </summary>
        /// <param name="pMapDataset2D"></param>
        /// <returns></returns>
        public static List<IFeatureLayer> Create_SymbolizedLayers_2D(ref FloorPlan floorPlan, ref IFeatureDataset pMapDataset2D)
        {
            //try
            //{
            List<IFeatureLayer> featureLayers = new List<IFeatureLayer>();

            if (FGDBHelper.IsIntialMapDataset(ref pMapDataset2D))
                FeaturesCreator2D.CreateFeatures(ref floorPlan, ref pMapDataset2D);

            //创建对应的要素图层并符号化
            IFeatureClassContainer pFeatureClassContainer = pMapDataset2D as IFeatureClassContainer;
            IFeatureClass pFeatureClass = null;
            IFeatureLayer pFeatureLayer = null;
            IColor pColor = null;
            double width_line = 0d;

            for (int i = 0; i < pFeatureClassContainer.ClassCount; i++)
            {
                pFeatureClass = pFeatureClassContainer.get_Class(i);
                pFeatureLayer = new FeatureLayerClass();
                pFeatureLayer.FeatureClass = pFeatureClass;


                switch (pFeatureClass.AliasName)
                {
                    case "FloorRegions":
                        pFeatureLayer.Name = "楼板";
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(221, 222, 224);
                        sampleRender_PolygonLayer(ref pFeatureLayer, ref pFeatureClass, ref pColor);
                        break;
                    case "Rooms":
                        pFeatureLayer.Name = "房间";
                        //uniqueRender_RoomLayer(ref pFeatureLayer, ref pFeatureClass);
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(189, 183, 107);
                        sampleRender_PolygonLayer(ref pFeatureLayer,ref pFeatureClass,ref pColor);
                        break;
                    case "StairRooms":
                        pFeatureLayer.Name = "楼梯间";
                        //pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(189, 148, 104);
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(189, 148, 104, 100);
                        sampleRender_PolygonLayer(ref pFeatureLayer, ref pFeatureClass, ref pColor);
                        break;
                    case "ElevatorShafts":
                        pFeatureLayer.Name = "电梯间";
                        //pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(63, 72, 204);
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(51, 51, 255);
                        sampleRender_PolygonLayer(ref pFeatureLayer, ref pFeatureClass, ref pColor);
                        break;
                    case "DoorLines":
                        pFeatureLayer.Name = "门";
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(128, 0, 0);
                        width_line = 5;
                        simpleRender_LineLayer(ref pFeatureLayer, ref pColor, width_line);

                        break;
                    case "WindowLines":
                        pFeatureLayer.Name = "窗";
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(34, 177, 76);
                        width_line = 5;
                        simpleRender_LineLayer(ref pFeatureLayer, ref pColor, width_line);
                        break;
                }

                featureLayers.Add(pFeatureLayer);
            }
            return featureLayers;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(string.Format("创建2D符号化图层失败: {0}!", ex.Message));
            //}
        }

        /// <summary>
        /// 创建 3D 符号化图层
        /// </summary>
        /// <param name="floorPlan"></param>
        /// <param name="pMapDataset3D"></param>
        /// <param name="networkDatasetName"></param>
        /// <param name="edgeSourceName"></param>
        /// <returns></returns>
        public static List<IFeatureLayer> Create_SymbolizedLayers_3D(ref FloorPlan floorPlan, ref IFeatureDataset pMapDataset3D, string networkDatasetName)
        {
            try
            {
                List<IFeatureLayer> featureLayers = new List<IFeatureLayer>();

                if (FGDBHelper.IsIntialMapDataset(ref pMapDataset3D))
                {
                    FeaturesCreator3D.CreateFeatures(ref floorPlan, ref pMapDataset3D);
                    RouteNetworkCreator.CreateNetDataSet(ref pMapDataset3D, networkDatasetName, networkEdgeSourceName);
                }


                IFeatureClassContainer pFeatureClassContainer = pMapDataset3D as IFeatureClassContainer;
                IFeatureClass pFeatureClass = null;
                IFeatureLayer pFeatureLayer = null;
                IColor pColor = null;
                double width_line = 0d;
                double size = 0d;

                for (int i = 0; i < pFeatureClassContainer.ClassCount; i++)
                {
                    pFeatureClass = pFeatureClassContainer.get_Class(i);
                    pFeatureLayer = new FeatureLayerClass();
                    pFeatureLayer.FeatureClass = pFeatureClass;

                    //图层属性设置+图层渲染
                    switch (pFeatureClass.AliasName)
                    {
                        case "Land3D":
                            pFeatureLayer.Name = "Land3D";
                            uniqueRender_3DGrassLandLayer(ref pFeatureLayer, ref pFeatureClass);
                            break;

                        case "Floor3D":
                            pFeatureLayer.Name = "Floor3D";
                            pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(203, 203, 101);
                            sampleRender_PolygonLayer(ref pFeatureLayer, ref pFeatureClass, ref pColor);
                            break;

                        case "Wall3D":
                            pFeatureLayer.Name = "Wall3D";
                            pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 255);
                            sampleRender_PolygonLayer(ref pFeatureLayer, ref pFeatureClass, ref pColor);
                            break;

                        case "Door3D":
                            pFeatureLayer.Name = "Door3D";
                            pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(127, 0, 0);
                            //sampleRender_PolygonLayer(ref pFeatureLayer, ref pFeatureClass, ref pColor);
                            uniqueRender_3DDoorLayer(ref pFeatureLayer, ref pFeatureClass);
                            break;
                        case "Window3D":
                            pFeatureLayer.Name = "Window3D";
                            uniqueRender_3DWindowLayer(ref pFeatureLayer, ref pFeatureClass);
                            break;
                        case "Stair3D":
                            pFeatureLayer.Name = "Stair3D";
                            uniqueRender_3DStairLayer(ref pFeatureLayer, ref pFeatureClass);
                            break;
                        case "Elevator3D":
                            //pFeatureLayer.Name = "Elevator3D";
                            //pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(128, 0, 0);
                            //width_line = 5;
                            //simpleRender_LineLayer(ref pFeatureLayer, ref pFeatureClass, ref pColor, width_line);

                            break;

                        case "Nodes3D":
                            pFeatureLayer.Name = "Nodes3D";
                            pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
                            size = 10d;
                            simpleRender_PointLayer(ref pFeatureLayer, ref pColor, ref size);
                            break;

                        case "Sides3D":
                            pFeatureLayer.Name = "Sides3D";
                            pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 0);
                            width_line = 3;
                            simpleRender_LineLayer(ref pFeatureLayer, ref pColor, width_line);
                            break;
                    }

                    featureLayers.Add(pFeatureLayer);
                }
                return featureLayers;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("创建3D符号化图层失败: {0}!", ex.Message));
            }
        }

        /// <summary>
        /// 对3D符号化图层进行分组（共4组： 环境组、框架组、建筑附件组+路径组）
        /// </summary>
        /// <param name="featureLayers3D"></param>
        /// <returns></returns>
        public static List<IGroupLayer> Get_Map3DGroupLayers(ref List<IFeatureLayer> featureLayers3D)
        {
            List<IGroupLayer> groupLayers = new List<IGroupLayer>();

            IGroupLayer pGroupLayer = null;
            List<ILayer> layers = null;

            //环境组
            pGroupLayer = new GroupLayerClass();
            pGroupLayer.Name = "环境";
            layers = (from p in featureLayers3D
                      where p.Name == "Land3D"
                      select p).ToList<ILayer>();

            foreach (var item in layers)
            {
                pGroupLayer.Add(item);
            }
            groupLayers.Add(pGroupLayer);

            //框架图层组
            pGroupLayer = new GroupLayerClass();
            pGroupLayer.Name = "框架";
            layers = (from p in featureLayers3D
                      where
                          p.Name == "Floor3D" ||
                          p.Name == "Wall3D"
                      select p).ToList<ILayer>();

            foreach (var item in layers)
            {
                pGroupLayer.Add(item);
            }
            groupLayers.Add(pGroupLayer);

            //建筑附件图层组
            pGroupLayer = new GroupLayerClass();
            pGroupLayer.Name = "建筑附件";
            layers = (from p in featureLayers3D
                      where
                          p.Name == "Door3D" ||
                          p.Name == "Window3D" ||
                          p.Name == "Stair3D" ||
                          p.Name == "Elevator3D"
                      select p).ToList<ILayer>();

            foreach (var item in layers)
            {
                pGroupLayer.Add(item);
            }
            groupLayers.Add(pGroupLayer);


            //路径组
            //pGroupLayer = new GroupLayerClass();
            //pGroupLayer.Name = "路径";
            //layers = (from p in featureLayers3D
            //          where
            //              p.Name == "Nodes3D" ||
            //              p.Name == "Sides3D"
            //          select p).ToList<ILayer>();

            foreach (var item in layers)
            {
                pGroupLayer.Add(item);
            }
            groupLayers.Add(pGroupLayer);


            return groupLayers;
        }


     


        #endregion



        #region 图层渲染
        //简单渲染点图层
        static void simpleRender_PointLayer(ref  IFeatureLayer pFeatureLayer, ref IColor pColor, ref double size, esriSimpleMarkerStyle style = esriSimpleMarkerStyle.esriSMSCircle)
        {
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            pSimpleMarkerSymbol.Color = pColor;
            pSimpleMarkerSymbol.Size = size;
            pSimpleMarkerSymbol.Style = style;

            ISimpleRenderer pSimpleRender = new SimpleRendererClass();
            pSimpleRender.Symbol = pSimpleMarkerSymbol as ISymbol;

            (pFeatureLayer as IGeoFeatureLayer).Renderer = pSimpleRender as IFeatureRenderer;
        }

        //简单渲染线图层
        static void simpleRender_LineLayer(ref IFeatureLayer pFeatureLayer, ref IColor pColor, double width)
        {
            ISimpleRenderer pSimpleRenderer;
            ISimpleLineSymbol pSimpleLineSymbol;


            pSimpleLineSymbol = new SimpleLineSymbolClass();
            pSimpleLineSymbol.Color = pColor;
            pSimpleLineSymbol.Width = width;
            pSimpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;

            pSimpleRenderer = new SimpleRendererClass();
            pSimpleRenderer.Symbol = pSimpleLineSymbol as ISymbol;
            (pFeatureLayer as IGeoFeatureLayer).Renderer = pSimpleRenderer as IFeatureRenderer;
        }

        //简单渲染面图层
        static void sampleRender_PolygonLayer(ref IFeatureLayer pFeatureLayer, ref IFeatureClass pFeatureClass, ref IColor pColor)
        {
            ISimpleRenderer pSimpleRenderer = new SimpleRendererClass();
            pSimpleRenderer.Symbol = create_simpleFillSymbol(pColor, 2) as ISymbol;
            (pFeatureLayer as IGeoFeatureLayer).Renderer = pSimpleRenderer as IFeatureRenderer;
        }

        //唯一值渲染房间图层
        static void uniqueRender_RoomLayer(ref IFeatureLayer pFeatureLayer, ref IFeatureClass pFeatureClass)
        {
            IUniqueValueRenderer pUniqueValueRender = new UniqueValueRendererClass();
            pUniqueValueRender.FieldCount = 1;
            pUniqueValueRender.set_Field(0, "RegionType");

            //设置默认颜色
            pUniqueValueRender.DefaultSymbol = create_simpleFillSymbol(ArcObjectsUtilities.ColorUtility.Get_RgbColor(189, 183, 107), 1.5) as ISymbol;
            pUniqueValueRender.UseDefaultSymbol = true;


            Array polygonTypes = Enum.GetValues(typeof(DXFReader.DataModel.PolygonType));
            foreach (var item in polygonTypes)
            {
                IColor pColor = null;

                switch (item.ToString())
                {
                    case "Parlour":
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(240, 229, 147);
                        break;
                    case "DiningRoom":
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(128, 128, 0);
                        break;
                    case "BedRoom":
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(208, 200, 137);
                        break;
                    case "Kitchen":
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(216, 227, 184);
                        break;
                    case "Toilet":
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(80, 167, 96);
                        break;
                    case "Balcony":
                        pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 136, 99);
                        break;
                }

                if (pColor != null)
                {
                    ISymbol pSymbol = create_simpleFillSymbol(pColor, 1.5) as ISymbol;
                    pUniqueValueRender.AddValue(item.ToString(), "RegionType", pSymbol);
                }
            }
            (pFeatureLayer as IGeoFeatureLayer).Renderer = pUniqueValueRender as IFeatureRenderer;
        }

        //唯一值渲染 3D草地图层
        static void uniqueRender_3DGrassLandLayer(ref IFeatureLayer pFeatureLayer, ref IFeatureClass pFeatureClass)
        {
            IUniqueValueRenderer pUniqueValueRender = new UniqueValueRendererClass();
            pUniqueValueRender.FieldCount = 1;
            pUniqueValueRender.set_Field(0, "PartFlag");
            pUniqueValueRender.DefaultSymbol = create_simpleFillSymbol(ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0), 1.5) as ISymbol;
            pUniqueValueRender.UseDefaultSymbol = true;

            ISymbol pSymbol = null;
            ITextureFillSymbol pTextureFillSymbol = null;//草地纹理填充符号

            //渲染草地
            string exePath = System.Environment.CurrentDirectory;
            string grassTexurePath = string.Format(@"{0}\ProFile\pictures\grassTexture.png", exePath);
            pTextureFillSymbol = create_textureFillSymbol(grassTexurePath, 8000d);
            if (pTextureFillSymbol != null)
            {
                pSymbol = pTextureFillSymbol as ISymbol;
            }
            else
            {
                MessageBox.Show("创建纹理填充符号失败，将使用简单填充符号渲染草坪！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                IColor pColor_grassSurface = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 255, 0);
                pSymbol = create_simpleFillSymbol(pColor_grassSurface, 0) as ISymbol;
            }

            pUniqueValueRender.AddValue("1", "PartFlag", pSymbol);

            //渲染土壤
            IColor pColor_soil = ArcObjectsUtilities.ColorUtility.Get_RgbColor(100, 0, 0);
            pSymbol = create_simpleFillSymbol(pColor_soil, 0) as ISymbol;
            pUniqueValueRender.AddValue("2", "PartFlag", pSymbol);


            (pFeatureLayer as IGeoFeatureLayer).Renderer = pUniqueValueRender as IFeatureRenderer;
        }

        //唯一值渲染 3D门图层
        static void uniqueRender_3DDoorLayer(ref IFeatureLayer pFeatureLayer, ref IFeatureClass pFeatureClass)
        {
            IUniqueValueRenderer pUniqueValueRender = new UniqueValueRendererClass();
            pUniqueValueRender.FieldCount = 2;
            pUniqueValueRender.set_Field(0, "Type");
            pUniqueValueRender.set_Field(1, "PartFlag");
            pUniqueValueRender.DefaultSymbol = create_simpleFillSymbol(ArcObjectsUtilities.ColorUtility.Get_RgbColor(127, 0, 0), 1.5) as ISymbol;
            pUniqueValueRender.UseDefaultSymbol = true;

            //多字段合并后的唯一值字段(基于字符串的拼接，字段值之间以固定分隔符(pUniqueValueRender.FieldDelimiter)隔开)
            string codeValue = "";

            //电梯门符号
            ISymbol pSymbol = null;
            IColor pColor_elevator = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 0, 255);
            pSymbol = create_simpleFillSymbol(pColor_elevator, 0) as ISymbol;

            codeValue = "M_ElevatorDoor" + pUniqueValueRender.FieldDelimiter + "0";
            pUniqueValueRender.AddValue(codeValue, "Type", pSymbol);

            //推拉门的门框及门扇
            IColor pColor_slidingDoorFrameLeaf = ArcObjectsUtilities.ColorUtility.Get_RgbColor(83, 83, 83);
            pSymbol = create_simpleFillSymbol(pColor_slidingDoorFrameLeaf, 10) as ISymbol;
            codeValue = "M_Sliding" + pUniqueValueRender.FieldDelimiter + "0";//门框
            pUniqueValueRender.AddValue(codeValue, "Type", pSymbol);
            codeValue = "M_Sliding" + pUniqueValueRender.FieldDelimiter + "1";//门扇
            pUniqueValueRender.AddValue(codeValue, "Type", pSymbol);

            //推拉门的玻璃
            IColor pColor_slidingDoorGlass = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 0, 255, 20);
            pSymbol = create_simpleFillSymbol(pColor_slidingDoorGlass, 10) as ISymbol;
            codeValue = "M_Sliding" + pUniqueValueRender.FieldDelimiter + "2";
            pUniqueValueRender.AddValue(codeValue, "Type", pSymbol);


            (pFeatureLayer as IGeoFeatureLayer).Renderer = pUniqueValueRender as IFeatureRenderer;
        }


        //唯一值渲染 3D窗图层
        static void uniqueRender_3DWindowLayer(ref IFeatureLayer pFeatureLayer, ref IFeatureClass pFeatureClass)
        {
            IUniqueValueRenderer pUniqueValueRender = new UniqueValueRendererClass();
            pUniqueValueRender.FieldCount = 1;
            pUniqueValueRender.set_Field(0, "PartFlag");
            pUniqueValueRender.DefaultSymbol = create_simpleFillSymbol(ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0), 1.5) as ISymbol;
            pUniqueValueRender.UseDefaultSymbol = true;


            IColor pColor_windowFrame = ArcObjectsUtilities.ColorUtility.Get_RgbColor(83, 83, 83);
            IColor pColor_glass = ArcObjectsUtilities.ColorUtility.Get_RgbColor(64, 64, 255, 30);

            ISymbol pSymbol = create_simpleFillSymbol(pColor_windowFrame, 1.5) as ISymbol;
            pUniqueValueRender.AddValue("1", "PartFlag", pSymbol);


            pSymbol = create_simpleFillSymbol(pColor_glass, 1) as ISymbol;
            pUniqueValueRender.AddValue("2", "PartFlag", pSymbol);

            (pFeatureLayer as IGeoFeatureLayer).Renderer = pUniqueValueRender as IFeatureRenderer;
        }


        //唯一值渲染 3D楼梯图层
        static void uniqueRender_3DStairLayer(ref IFeatureLayer pFeatureLayer, ref IFeatureClass pFeatureClass)
        {
            IUniqueValueRenderer pUniqueValueRender = new UniqueValueRendererClass();
            pUniqueValueRender.FieldCount = 1;
            pUniqueValueRender.set_Field(0, "PartFlag");
            pUniqueValueRender.DefaultSymbol = create_simpleFillSymbol(ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0), 1.5) as ISymbol;
            pUniqueValueRender.UseDefaultSymbol = true;


            IColor pColor_landAndFlight = ArcObjectsUtilities.ColorUtility.Get_RgbColor(83, 83, 83);
            IColor pColor_verticalColumns = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 0, 0);
            IColor pColor_handrail = ArcObjectsUtilities.ColorUtility.Get_RgbColor(127, 0, 0);

            ISymbol pSymbol = create_simpleFillSymbol(pColor_landAndFlight, 1.5) as ISymbol;
            pUniqueValueRender.AddValue("1", "PartFlag", pSymbol);

            pUniqueValueRender.AddValue("2", "PartFlag", pSymbol);

            pSymbol = create_simpleFillSymbol(pColor_verticalColumns, 1.5) as ISymbol;
            pUniqueValueRender.AddValue("3", "PartFlag", pSymbol);


            pSymbol = create_simpleFillSymbol(pColor_handrail, 1.5) as ISymbol;
            pUniqueValueRender.AddValue("4", "PartFlag", pSymbol);



            (pFeatureLayer as IGeoFeatureLayer).Renderer = pUniqueValueRender as IFeatureRenderer;
        }

        static ISimpleFillSymbol create_simpleFillSymbol(IColor color, double outLineWidth)
        {
            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            pSimpleFillSymbol.Color = color;
            pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            pSimpleFillSymbol.Outline = new SimpleLineSymbolClass()
            {
                Color = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 0, 0),
                Style = esriSimpleLineStyle.esriSLSSolid,
                Width = outLineWidth
            };

            return pSimpleFillSymbol;
        }

        static ITextureFillSymbol create_textureFillSymbol(string textureFileName, double size)
        {
            ITextureFillSymbol pTextureFillSymbol = new TextureFillSymbolClass();

            try
            {
                pTextureFillSymbol.CreateFillSymbolFromFile(textureFileName);
                pTextureFillSymbol.Size = size;
            }
            catch (Exception ex)
            {
                return null;
            }

            return pTextureFillSymbol;
        }
        #endregion
    }
}
