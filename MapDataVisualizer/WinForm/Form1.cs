using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcObjectsUtilities;
using BCE;
using BCE.DataModels;
using BCE.DataModels.Basic;
using DXFReader.DataModel;
using DXFReader.Reader;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Analyst3DTools;
using ESRI.ArcGIS.Animation;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.NetworkAnalyst;
using EsriMapDataGenerator;
using EsriMapDataGenerator.GeometryCreator;
using MainProgram.Assets.Controls;
using RE;
using WinForm.ArcCommands;

namespace WinForm
{
    public partial class Form1 : Form
    {
        FloorPlan floorPlan = null;

        IAGAnimationUtils pAGAnimationUtils;//动画常用工具接口
        IAGAnimationContainer pAGAnimationContainer;//scene对应的动画容器
        IAGImportPathOptions pAGImportPathOptions;//路径动画参数配置
        IAGAnimationTracks pAGAnimationtracks;
        

        IPolyline navigatePath = null;


        ILayer selectedLayer = null;

        public Form1()
        {
            InitializeComponent();

            this.MouseWheel += Form1_MouseWheel;

            axSceneControl1.KeyIntercept = 1;//允许截获箭头按键值
            axSceneControl1.OnKeyDown += axSceneControl1_OnKeyDown;

            axToolbarControl1.AddItem(new CmdChangeLayerOpacoty());

            axTOCControl1.OnMouseDown += axTOCControl1_OnMouseDown;

            toolStripMenuItem1.Click += toolStripMenuItem1_Click;


        }

        void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //if (selectedLayer == null) return;

            //ILayerEffects pLayerEffect = null;
            //if (selectedLayer is IGroupLayer)
            //{
            //    /*若为图层组，则 必须将IGroupLayerz转换为ICompositeLayer，然后遍历修改每一图层的透明度
            //     * 根本原因： IGroupLayer包含的图层集可以拥有不同的透明度，修改IGroupLayer层面的的透明度无效
            //    */
            //    ICompositeLayer pCompositeLayer = (selectedLayer as IGroupLayer) as ICompositeLayer;

            //    for (int i = 0; i < pCompositeLayer.Count; i++)
            //    {
            //        pLayerEffect = pCompositeLayer.get_Layer(i) as ILayerEffects;
            //        pLayerEffect.Transparency = 50;
            //    }
            //}
            //else
            //{
            //    //若为单图层，则直接转换为ILayerEffects修改
            //    pLayerEffect = selectedLayer as ILayerEffects;
            //    pLayerEffect.Transparency = 50;
            //}

            //axTOCControl1.Update();
            //axTOCControl1.Refresh();


            ILayer pLayer = axSceneControl1.Scene.get_Layer(0);
            ILayerEffects pLayerEffect = pLayer as ILayerEffects;
            pLayerEffect.Transparency = 50;

            axSceneControl1.Scene.ClearLayers();
            axSceneControl1.Scene.AddLayer(pLayer);


            axSceneControl1.Refresh();
        }



        void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {

            if (e.button == 2)
            {
                ESRI.ArcGIS.Controls.esriTOCControlItem Item = ESRI.ArcGIS.Controls.esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pBasicMap = null;
                ILayer pLayer = null;
                object other = null;
                object index = null;
                axTOCControl1.HitTest(e.x, e.y, ref Item, ref pBasicMap, ref pLayer, ref other, ref index);          //实现赋值
                selectedLayer = pLayer;
                if (Item == esriTOCControlItem.esriTOCControlItemLayer)           //点击的是图层的话，就显示右键菜单
                {

                    //contextMenuStrip1.Show(axTOCControl1, new System.Drawing.Point(e.x, e.y));
                    ////显示右键菜单，并定义其相对控件的位置，正好在鼠标出显示


                    contextMenuStrip1.Show(axTOCControl1, e.x, e.y);

                }
                else
                    selectedLayer = null;
            }
            else
                selectedLayer = null;
        }










        void Form1_MouseWheel(object sender, MouseEventArgs e)
        {

            try
            {
                System.Drawing.Point pSceneLocation = axSceneControl1.PointToScreen(axSceneControl1.Location);
                System.Drawing.Point Pt = axSceneControl1.PointToScreen(e.Location);
                if (Pt.X < pSceneLocation.X | Pt.X > pSceneLocation.X + axSceneControl1.Width | Pt.Y < pSceneLocation.Y | Pt.Y > pSceneLocation.Y + axSceneControl1.Height)
                    return;

                double scale;
                if (e.Delta < 0)
                {
                    scale = 0.2;
                    axSceneControl1.MousePointer = esriControlsMousePointer.esriPointerZoomOut;
                }
                else
                {
                    scale = -0.2;
                    axSceneControl1.MousePointer = esriControlsMousePointer.esriPointerZoomIn;
                }


                ICamera pCamera = axSceneControl1.Camera;
                IPoint pObserver = pCamera.Observer;
                IPoint pTarget = pCamera.Target;
                pObserver.X += (pObserver.X - pTarget.X) * scale;
                pObserver.Y += (pObserver.Y - pTarget.Y) * scale;
                pObserver.Z += (pObserver.Z - pTarget.Z) * scale;
                pCamera.Observer = pObserver;
                axSceneControl1.SceneGraph.RefreshViewers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void axSceneControl1_OnKeyDown(object sender, ISceneControlEvents_OnKeyDownEvent e)
        {
            ICamera pCamera = axSceneControl1.Camera;
            IPoint pObserverPosition = pCamera.Observer;
            pObserverPosition.Z = 2000d;

            switch (e.keyCode)
            {
                //左
                case 37:
                    pObserverPosition.X -= 1000d;
                    break;

                //右
                case 39:
                    pObserverPosition.X += 1000d;
                    break;

                //前
                case 38:
                    pObserverPosition.Y += 1000d;
                    break;

                //后
                case 40:
                    pObserverPosition.Y -= 1000d;
                    break;
            }

            axSceneControl1.Camera.Observer = pObserverPosition;//必须重新赋值以强制刷新，否则将无法应用新值
            axSceneControl1.SceneGraph.RefreshViewers();
        }

        private void btnCreate2D_Click(object sender, EventArgs e)
        {
            return;


            #region 图纸初始化配置
            string dxfPath = @"C:\Users\silence\Desktop\IndoorMapStudio\Temp\5号楼标准层平面图 - 副本.dxf";
            ParametersConfiguration config = new ParametersConfiguration(dxfPath);

            config.DXF_Path = dxfPath;

            floorPlan = new FloorPlan(ref config);

            DxfReader dxfReader = new DxfReader(dxfPath, true);


            //图层映射配置
            Mapping_LayerNames mapping_LayerNames = new Mapping_LayerNames();
            mapping_LayerNames.Wall.Add("WALL");//墙元素图层名集合
            mapping_LayerNames.Balcony.Add("BALCONY");//阳台元素图层名集合
            //mapping_LayerNames.Balcony.Add("YANGTAI");
            mapping_LayerNames.Window.Add("WINDOW");//窗元素图层名集合
            mapping_LayerNames.Door.Add("DOOR");//门元素图层名集合
            mapping_LayerNames.Elevator.Add("Elevator");//电梯元素图层名集合
            mapping_LayerNames.Text.Add("PUB_TEXT");//注记元素图层名集合
            mapping_LayerNames.Stairs.Add("STAIR");//楼体线图层名集合
            floorPlan.Configuration.LayerMapping = mapping_LayerNames;
            #endregion


            InfoSegementation.SegementInfo(ref floorPlan, ref dxfReader);
            FloorFrameModeler.Model(ref floorPlan);

            //显示地图
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            string directory = @"C:\Users\silence\Desktop\新建文件夹";
            string toDirectory = @"C:\Users\silence\Desktop\新建文件夹 (2)";
            string[] fileNames = System.IO.Directory.GetDirectories(directory);
            IFileNames pFileNames = new FileNamesClass();
            foreach (var item in fileNames)
            {
                pFileNames.Add(item);
            }
            IWorkspaceName pSourceWorksapceName = pWorkspaceFactory.GetWorkspaceName(directory, pFileNames);
            IWorkspaceName pToWorkspaceName = new WorkspaceNameClass();
            bool result = pWorkspaceFactory.Copy(pSourceWorksapceName, toDirectory, out pToWorkspaceName);
            IWorkspace pWorkspace = (pToWorkspaceName as IName).Open() as IWorkspace;
            IFeatureDataset pFeatureDataset_2D = (pWorkspace as IFeatureWorkspace).OpenFeatureDataset("MapData2D");



            List<IFeatureLayer> featureLayers =
                EsriMapDataGenerator.DataGenerator.Create_SymbolizedLayers_2D(ref floorPlan, ref pFeatureDataset_2D);

            IGroupLayer pGroupLayer = new GroupLayerClass();
            pGroupLayer.Name = "Map_2D";

            foreach (var item in featureLayers)
                pGroupLayer.Add(item);

            axSceneControl1.Scene.ClearLayers();
            axSceneControl1.Scene.AddLayer(pGroupLayer);
            axSceneControl1.SceneGraph.RefreshViewers();
        }

        private void btnCreate3D_Click(object sender, EventArgs e)
        {
            return;

            #region 配置图纸并解析
            string dxfPath = @"C:\Users\silence\Desktop\IndoorMapStudio\Temp\5号楼标准层平面图 - 副本.dxf";
            ParametersConfiguration config = new ParametersConfiguration(dxfPath);

            config.DXF_Path = dxfPath;

            floorPlan = new FloorPlan(ref config);

            DxfReader dxfReader = new DxfReader(dxfPath, true);


            //图层映射配置
            Mapping_LayerNames mapping_LayerNames = new Mapping_LayerNames();
            mapping_LayerNames.Wall.Add("WALL");//墙元素图层名集合
            mapping_LayerNames.Balcony.Add("BALCONY");//阳台元素图层名集合
            //mapping_LayerNames.Balcony.Add("YANGTAI");
            mapping_LayerNames.Window.Add("WINDOW");//窗元素图层名集合
            mapping_LayerNames.Door.Add("DOOR");//门元素图层名集合
            mapping_LayerNames.Elevator.Add("Elevator");//电梯元素图层名集合
            mapping_LayerNames.Text.Add("PUB_TEXT");//注记元素图层名集合
            mapping_LayerNames.Stairs.Add("STAIR");//楼体线图层名集合
            floorPlan.Configuration.LayerMapping = mapping_LayerNames;

            //解析图纸
            InfoSegementation.SegementInfo(ref floorPlan, ref dxfReader);
            FloorFrameModeler.Model(ref floorPlan);
            #endregion



            //显示地图
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            string directory = @"C:\Users\silence\Desktop\新建文件夹";
            string toDirectory = @"C:\Users\silence\Desktop\新建文件夹 (2)";

            string gdbPath = toDirectory + @"\IndoorMapData.gdb";

            if (System.IO.Directory.Exists(gdbPath))
                System.IO.Directory.Delete(gdbPath, true);


            string[] fileNames = System.IO.Directory.GetDirectories(directory);
            IFileNames pFileNames = new FileNamesClass();
            foreach (var item in fileNames)
            {
                pFileNames.Add(item);
            }
            IWorkspaceName pSourceWorksapceName = pWorkspaceFactory.GetWorkspaceName(directory, pFileNames);
            IWorkspaceName pToWorkspaceName = new WorkspaceNameClass();
            bool result = pWorkspaceFactory.Copy(pSourceWorksapceName, toDirectory, out pToWorkspaceName);
            IWorkspace pWorkspace = (pToWorkspaceName as IName).Open() as IWorkspace;
            IFeatureDataset pFeatureDataset_3D = (pWorkspace as IFeatureWorkspace).OpenFeatureDataset("MapData3D");



            List<IFeatureLayer> featureLayers_3D =
                DataGenerator.Create_SymbolizedLayers_3D(ref floorPlan, ref pFeatureDataset_3D, "Route_ND");




            //图层显示
            IGroupLayer pGroupLayer = new GroupLayerClass();
            pGroupLayer.Name = "Map_3D";

            IGroupLayer pGroupLayer_route = new GroupLayerClass();
            pGroupLayer_route.Name = "Route3D";

            foreach (var item in featureLayers_3D)
            {
                if (item.Name == "Wall3D" || item.Name == "Floor3D")
                {
                    //修改墙和楼板图层的透明度为50%

                    ILayerEffects pLayerEffect = item as ILayerEffects;

                    if (pLayerEffect.SupportsTransparency)
                        pLayerEffect.Transparency = 50;
                }


                if (item.Name == "Nodes3D" || item.Name == "Sides3D")
                    pGroupLayer_route.Add(item);
                else
                    pGroupLayer.Add(item);
            }

            axSceneControl1.Scene.ClearLayers();






            axSceneControl1.Scene.AddLayer(pGroupLayer_route);
            axSceneControl1.Scene.AddLayer(pGroupLayer);

            axSceneControl1.SceneGraph.RefreshViewers();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            return;

            if (floorPlan == null) return;

            #region 创建导航路径并显示
            IPolyline path = null;//导航路径

            SemanticPolygon sp = (from p in floorPlan.PResult.SPolygons
                                  where p.PType == PolygonType.Floor
                                  select p).First<SemanticPolygon>();

            MathExtension.Geometry.Polygon polygon = new MathExtension.Geometry.Polygon(sp.Points);
            double[] xy_most = MathExtension.Geometry.GeometryHelper.Get_XYMost(polygon);

            //楼板多边形近似几何中心
            IPoint pCentre =
                ArcObjectsUtilities.GeometryUtility.Construct_Point3D((xy_most[0] + xy_most[1]) / 2, (xy_most[2] + xy_most[3]) / 2, 0d);

            //草地的尺寸（取楼板面积的25倍）
            double centreX = (xy_most[1] + xy_most[0]) / 2;
            double centreY = (xy_most[3] + xy_most[2]) / 2;
            double landLength = xy_most[1] - xy_most[0];
            double landWidth = xy_most[3] - xy_most[2];

            List<DXFReader.DataModel.LineClass> lines = new List<DXFReader.DataModel.LineClass>()
                {       
                    new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(centreX-landLength,centreY-landWidth,0),EndPoint=new MathExtension.Geometry.Point(centreX+landLength,centreY-landWidth,0)},
                     new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(centreX+landLength,centreY-landWidth,0),EndPoint=new MathExtension.Geometry.Point(centreX+landLength,centreY+landWidth,0)},
                      new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(centreX+landLength,centreY+landWidth,0),EndPoint=new MathExtension.Geometry.Point(centreX-landLength,centreY+landWidth,0)},
                       new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(centreX-landLength,centreY+landWidth,0),EndPoint=new MathExtension.Geometry.Point(centreX-landLength,centreY-landWidth,0)}

                };

            path = ConverterForEsri.Create_Polyline_FromLines(lines, 4000d);


            //List<DXFReader.DataModel.LineClass> lines =
            //    new List<DXFReader.DataModel.LineClass>()
            //    {       
            //        new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(0,0,500),EndPoint=new MathExtension.Geometry.Point(6000,0,500)},
            //         new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(6000,0,500),EndPoint=new MathExtension.Geometry.Point(6000,4000,500)},
            //          new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(6000,4000,500),EndPoint=new MathExtension.Geometry.Point(0,4000,500)},
            //           new DXFReader.DataModel.LineClass()  {StartPoint=new MathExtension.Geometry.Point(0,4000,500),EndPoint=new MathExtension.Geometry.Point(0,1000,500)}

            //    };

            //path = MapDataGenerator.DataTransverter.Create_Polyline_FromLines(lines, 500);
            (path as ITopologicalOperator).Simplify();


            IElement pElement = new LineElementClass();
            pElement.Geometry = path;

            ISimpleLineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
            pSimpleLineSymbol.Color = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
            pSimpleLineSymbol.Width = 5;
            pSimpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;

            (pElement as ILineElement).Symbol = pSimpleLineSymbol;


            IGraphicsContainer3D pGraphicsContainer = new GraphicsLayer3DClass();
            pGraphicsContainer.AddElement(pElement);

            //axSceneControl1.Scene.AddLayer(pGraphicsContainer as ILayer);
            axSceneControl1.Refresh();
            #endregion

            if (navigatePath != null)
                path = navigatePath;


            #region 创建路径飞行动画
            //IAGAnimationUtils pAGAnimationUtils;//动画常用工具接口
            //IAGAnimationContainer pAGAnimationContainer;//scene对应的动画容器
            //IAGImportPathOptions pAGImportPathOptions;//路径动画参数配置

            //step1:
            pAGAnimationContainer = axSceneControl1.Scene as IAGAnimationContainer;

            //step2: 配置路径动画参数
            pAGImportPathOptions = new AGImportPathOptionsClass();
            pAGImportPathOptions.TrackName = "First Path Animation";//动画轨迹名
            pAGImportPathOptions.AnimationEnvironment = new AGAnimationEnvironmentClass();
            pAGImportPathOptions.AnimationEnvironment.AnimationDuration = 20d;
            pAGImportPathOptions.BasicMap = axSceneControl1.Scene as IBasicMap;
            pAGAnimationtracks = axSceneControl1.Scene as IAGAnimationTracks;
            pAGImportPathOptions.AnimationType = new AnimationTypeCameraClass();//动画类型：相机动画
            pAGImportPathOptions.AnimatedObject = axSceneControl1.SceneViewer.Camera;
            pAGImportPathOptions.PathGeometry = path;//相机路径
            pAGImportPathOptions.ConversionType = esriFlyFromPathType.esriFlyFromPathObsAndTarget;//同时改变观察点和目标点


            pAGImportPathOptions.OverwriteExisting = true;//覆盖现有动画

            pAGImportPathOptions.PutAngleCalculationMethods(
                esriPathAngleCalculation.esriAngleAddRelative,
                esriPathAngleCalculation.esriAngleAddRelative,
                esriPathAngleCalculation.esriAngleAddRelative);


            //pAGImportPathOptions.PutAngleCalculationValues(0.0, 0.0, 0.0);

            //double pAzimuth, pInclination, pRollVal;
            //pAGImportPathOptions.GetAngleCalculationValues(out pAzimuth, out pInclination, out pRollVal);



            //step3: 创建路径动画
            pAGAnimationUtils = new AGAnimationUtilsClass();
            pAGAnimationUtils.CreateFlybyFromPath(pAGAnimationContainer, pAGImportPathOptions);
            #endregion

            #region 播放动画
            //IAGAnimationPlayer pAGAnimationPlayer = pAGAnimationUtils as IAGAnimationPlayer;
            ////IAGAnimationTracks pAGAnimationtracks = axSceneControl1.Scene as IAGAnimationTracks;

            //pAGAnimationPlayer.PlayAnimation(pAGAnimationtracks, pAGImportPathOptions.AnimationEnvironment, null);            
            #endregion
        }
        private void btnPaly_Click(object sender, EventArgs e)
        {
            return;


            if (pAGAnimationtracks == null || pAGAnimationtracks.AGTracks.Count == 0) return;

            IAGAnimationPlayer pAGAnimationPlayer = pAGAnimationUtils as IAGAnimationPlayer;
            pAGAnimationPlayer.PlayAnimation(pAGAnimationtracks, pAGImportPathOptions.AnimationEnvironment, null);
        }





        private void button1_Click(object sender, EventArgs e)
        {
            return;

            #region 配置图纸并解析
            string dxfPath = @"C:\Users\silence\Desktop\IndoorMapStudio\Temp\5号楼标准层平面图 - 副本.dxf";
            ParametersConfiguration config = new ParametersConfiguration(dxfPath);

            config.DXF_Path = dxfPath;

            floorPlan = new FloorPlan(ref config);

            DxfReader dxfReader = new DxfReader(dxfPath, true);


            //图层映射配置
            Mapping_LayerNames mapping_LayerNames = new Mapping_LayerNames();
            mapping_LayerNames.Wall.Add("WALL");//墙元素图层名集合
            mapping_LayerNames.Balcony.Add("BALCONY");//阳台元素图层名集合
            //mapping_LayerNames.Balcony.Add("YANGTAI");
            mapping_LayerNames.Window.Add("WINDOW");//窗元素图层名集合
            mapping_LayerNames.Door.Add("DOOR");//门元素图层名集合
            mapping_LayerNames.Elevator.Add("Elevator");//电梯元素图层名集合
            mapping_LayerNames.Text.Add("PUB_TEXT");//注记元素图层名集合
            mapping_LayerNames.Stairs.Add("STAIR");//楼体线图层名集合
            floorPlan.Configuration.LayerMapping = mapping_LayerNames;

            //解析图纸
            InfoSegementation.SegementInfo(ref floorPlan, ref dxfReader);
            FloorFrameModeler.Model(ref floorPlan);
            #endregion

            MessageBox.Show("提取完毕！");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            return;

            if (floorPlan == null) return;

            IGraphicsContainer3D pGraphicsContainer = new GraphicsLayer3DClass();
            ILayer pGraphicsLayer = pGraphicsContainer as ILayer;
            pGraphicsLayer.Name = "GraphicsLayer";
            //axSceneControl1.Scene.ClearLayers();
            axSceneControl1.Scene.AddLayer(pGraphicsLayer);
            axSceneControl1.Scene.MoveLayer(pGraphicsLayer, 0);

            #region 绘制墙线

            #endregion


            #region 绘制功能区抽象点
            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType != PolygonType.Wall
                                         && p.PType != PolygonType.Balustrade
                                         && p.PType != PolygonType.Floor
                                         select p).ToList<SemanticPolygon>();


            IColor pPointColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
            foreach (var item in sps)
            {
                IGeometry pPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(
                    item.FunctionRegionPoint.X,
                    item.FunctionRegionPoint.Y,
                    140);

                IElement pPointElement = ArcObjectsUtilities.ElementUtility.Construct_PointElement(pPoint, pPointColor, 20, esriSimpleMarkerStyle.esriSMSSquare);


                pGraphicsContainer.AddElement(pPointElement);
            }


            axSceneControl1.Refresh();

            #endregion


            #region 绘制门点
            IColor pDoorPointColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 255, 0);
            foreach (var item in sps)
            {
                foreach (var point in item.DoorPoints)
                {


                    IGeometry pPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(
                     point.X,
                     point.Y,
                     140);

                    IElement pPointElement = ArcObjectsUtilities.ElementUtility.Construct_PointElement(pPoint, pDoorPointColor, 20, esriSimpleMarkerStyle.esriSMSSquare);


                    pGraphicsContainer.AddElement(pPointElement);

                }
            }

            #endregion
        }

        private void button3_Click(object sender, EventArgs e)
        {

            return;

            string dxfPath = @"C:\Users\silence\Desktop\TestDXF - 副本.dxf";
            DxfReader dxfReader = new DxfReader(dxfPath, true);

            BCE.BCExtractors.ERings er = new BCE.BCExtractors.ERings(dxfReader.Lines);

            bool passCheck = BCE.BCExtractors.ERings.Evaluate_RingsCreation(ref er);

            if (!passCheck)
            {
                MessageBox.Show("Invalid Ring");
                return;
            }

            if (er.Rings.Count != 1)
            {
                MessageBox.Show("UnExpected Result");
                return;
            }
            SemanticPolygon sp = er.Rings[0];


            IGraphicsContainer3D pGraphicsContainer = new GraphicsLayer3DClass();
            ILayer pGraphicsLayer = pGraphicsContainer as ILayer;
            pGraphicsLayer.Name = "GraphicsLayer";
            axSceneControl1.Scene.ClearLayers();
            axSceneControl1.Scene.AddLayer(pGraphicsLayer);


            //绘制多边形边
            IGeometry pG_side = ConverterForEsri.Create_Polyline_FromLines(sp.Lines, 0d);
            IColor pColor_side = ArcObjectsUtilities.ColorUtility.Get_RgbColor(138, 43, 226);
            IElement pElement_side = ArcObjectsUtilities.ElementUtility.Construct_LineElement(pG_side, pColor_side, 2);

            pGraphicsContainer.AddElement(pElement_side);


            //绘制多边形的顶点
            IGeometry pGeometry_vertex;
            IColor pColor_vertex;
            const double size = 10;
            IElement pElement_vertex;
            pColor_vertex = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
            foreach (var item in sp.Points)
            {
                pGeometry_vertex = ConverterForEsri.Create_Point(item, 0d);
                pElement_vertex = ArcObjectsUtilities.ElementUtility.Construct_PointElement(pGeometry_vertex, pColor_vertex, size);
                pGraphicsContainer.AddElement(pElement_vertex);
            }


            //绘制多边形的凹顶点
            IColor pColor_concaveVertex = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 255, 0);
            MathExtension.Geometry.Polygon polygon = new MathExtension.Geometry.Polygon(sp.Points);
            bool isConvexPolygon = MathExtension.Geometry.GeometryHelper.IsConvexPolygon(polygon.Points);
            if (!isConvexPolygon)
            {
                List<int> convasPointIndexes = MathExtension.Geometry.GeometryHelper.Get_ConcaveVertexIndexes(sp.Points);

                foreach (var item in convasPointIndexes)
                {
                    pGeometry_vertex = ConverterForEsri.Create_Point(sp.Points[item], 0d);

                    pElement_vertex = ArcObjectsUtilities.ElementUtility.Construct_PointElement(pGeometry_vertex, pColor_concaveVertex, size);

                    pGraphicsContainer.AddElement(pElement_vertex);
                }
            }


            //绘制任意多边形的几何中心点
            MathExtension.Geometry.Point point = MathExtension.Geometry.GeometryHelper.Calc_PointInPolygon(sp.Points);
            pGeometry_vertex = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(point.X, point.Y, point.Z);

            IColor pColor_contained = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 255);
            pElement_vertex = ArcObjectsUtilities.ElementUtility.Construct_PointElement(pGeometry_vertex, pColor_contained, size);
            pGraphicsContainer.AddElement(pElement_vertex);



            //绘制路径
            //MathExtension.Geometry.Point doorPoint = new MathExtension.Geometry.Point(11315272.5702, 2102805.8818);
            //List<MathExtension.Geometry.Point> routeNodes = RouteExtractor.Calc_RouteNodes(doorPoint, point, polygon);

            //List<DXFReader.DataModel.LineClass> routeLines = new List<DXFReader.DataModel.LineClass>();
            //for (int i = 0; i < routeNodes.Count - 1; i++)
            //{
            //    routeLines.Add(new DXFReader.DataModel.LineClass(routeNodes[i], routeNodes[i + 1]));
            //}


            //IGeometry pG_route = MapDataGenerator.DataTransverter.Create_Polyline_FromLines(routeLines, 0d);
            //IColor pColor_route = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 255, 0);
            //IElement pElement_route = ArcObjectsUtilities.ElementUtility.Construct_LineElement(pG_route, pColor_route, 2);

            //pGraphicsContainer.AddElement(pElement_route);


            ////绘制路径节点
            //IGeometry pGeometry_routeNode;
            //IColor pColor_routeNode;
            //IElement pElement_routeNode;
            //pColor_routeNode = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 0);
            //foreach (var item in routeNodes)
            //{
            //    pGeometry_routeNode = MapDataGenerator.DataTransverter.Create_PointZAware(item, 0d);
            //    pElement_routeNode = ArcObjectsUtilities.ElementUtility.Construct_PointElement(pGeometry_routeNode, pColor_routeNode, size);
            //    pGraphicsContainer.AddElement(pElement_routeNode);
            //}



            axSceneControl1.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            return;

            #region 配置图纸并解析
            string dxfPath = @"C:\Users\silence\Desktop\IndoorMapStudio\Temp\5号楼标准层平面图 - 副本.dxf";
            ParametersConfiguration config = new ParametersConfiguration(dxfPath);

            config.DXF_Path = dxfPath;

            floorPlan = new FloorPlan(ref config);

            DxfReader dxfReader = new DxfReader(dxfPath, true);


            //图层映射配置
            Mapping_LayerNames mapping_LayerNames = new Mapping_LayerNames();
            mapping_LayerNames.Wall.Add("WALL");//墙元素图层名集合
            mapping_LayerNames.Balcony.Add("BALCONY");//阳台元素图层名集合
            //mapping_LayerNames.Balcony.Add("YANGTAI");
            mapping_LayerNames.Window.Add("WINDOW");//窗元素图层名集合
            mapping_LayerNames.Door.Add("DOOR");//门元素图层名集合
            mapping_LayerNames.Elevator.Add("Elevator");//电梯元素图层名集合
            mapping_LayerNames.Text.Add("PUB_TEXT");//注记元素图层名集合
            mapping_LayerNames.Stairs.Add("STAIR");//楼体线图层名集合
            floorPlan.Configuration.LayerMapping = mapping_LayerNames;

            //解析图纸
            InfoSegementation.SegementInfo(ref floorPlan, ref dxfReader);
            FloorFrameModeler.Model(ref floorPlan);
            #endregion



            IGraphicsContainer3D pGraphicsContainer = new GraphicsLayer3DClass();
            ILayer pGraphicsLayer = pGraphicsContainer as ILayer;
            pGraphicsLayer.Name = "GraphicsLayer";
            axSceneControl1.Scene.ClearLayers();
            axSceneControl1.Scene.AddLayer(pGraphicsLayer);

            //绘制多边形边
            var roomSPs = (from p in floorPlan.PResult.SPolygons
                           where p.PType != PolygonType.Wall
                           && p.PType != PolygonType.Balustrade
                           select p).ToArray<SemanticPolygon>();

            foreach (var item in roomSPs)
            {
                IGeometry pG_side = ConverterForEsri.Create_Polyline_FromLines(item.Lines, 0d);
                IColor pColor_side = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 0, 0);
                IElement pElement_side = ArcObjectsUtilities.ElementUtility.Construct_LineElement(pG_side, pColor_side, 2);

                pGraphicsContainer.AddElement(pElement_side);
            }


            List<IGeometry> sides = null;
            List<IGeometry> vertexes = null;

            Route3DCreator.CreateFloorRoute(ref floorPlan, out sides, out vertexes);

            //绘制顶点
            IElement pElement = null;
            IColor pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 255, 0);
            double size = 10d;
            foreach (var item in vertexes)
            {
                pElement = ArcObjectsUtilities.ElementUtility.Construct_PointElement(item, pColor, size);
                pGraphicsContainer.AddElement(pElement);
            }

            //绘制边
            pColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(255, 0, 0);
            foreach (var item in sides)
            {
                pElement = ArcObjectsUtilities.ElementUtility.Construct_LineElement(item, pColor, 2);
                pGraphicsContainer.AddElement(pElement);
            }

            axSceneControl1.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //string fileGDBPath = @"C:\Users\silence\Desktop\IndoorNetDataSet\IndoorMapData.gdb";
            //string networkParentDataset = "MapData3D";

            ////NetBuilder.BuildNetDataSet(fileGDBPath, networkParentDataset);

            //RouteNetworkCreator.CreateNetDataSet(fileGDBPath, networkParentDataset, "Route_ND", "Sides3D");



            //NavigationController nc = new NavigationController(ref floorPlan,ref axSceneControl1);
            //nc.Show();

            //INetworkLayer pNetworkLayer = new NetworkLayerClass();
            //ILayer pLayer_Net = pNetworkLayer as ILayer;
            //pLayer_Net.Name = "NetLayer";
            //axSceneControl1.Scene.AddLayer(pLayer_Net);



            #region 配置图纸并解析
            string dxfPath = @"C:\Users\silence\Desktop\IndoorMapStudio\Temp\5号楼标准层平面图 - 副本.dxf";
            ParametersConfiguration config = new ParametersConfiguration(dxfPath);

            config.DXF_Path = dxfPath;

            floorPlan = new FloorPlan(ref config);

            DxfReader dxfReader = new DxfReader(dxfPath, true);


            //图层映射配置
            Mapping_LayerNames mapping_LayerNames = new Mapping_LayerNames();
            mapping_LayerNames.Wall.Add("WALL");//墙元素图层名集合
            mapping_LayerNames.Balcony.Add("BALCONY");//阳台元素图层名集合
            //mapping_LayerNames.Balcony.Add("YANGTAI");
            mapping_LayerNames.Window.Add("WINDOW");//窗元素图层名集合
            mapping_LayerNames.Door.Add("DOOR");//门元素图层名集合
            mapping_LayerNames.Elevator.Add("Elevator");//电梯元素图层名集合
            mapping_LayerNames.Text.Add("PUB_TEXT");//注记元素图层名集合
            mapping_LayerNames.Stairs.Add("STAIR");//楼体线图层名集合
            floorPlan.Configuration.LayerMapping = mapping_LayerNames;

            //解析图纸
            InfoSegementation.SegementInfo(ref floorPlan, ref dxfReader);
            FloorFrameModeler.Model(ref floorPlan);
            #endregion


            //FGDB创建
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            string directory = @"C:\Users\silence\Desktop\新建文件夹";
            string toDirectory = @"C:\Users\silence\Desktop\新建文件夹 (2)";

            string gdbPath = toDirectory + @"\IndoorMapData.gdb";

            if (System.IO.Directory.Exists(gdbPath))
                System.IO.Directory.Delete(gdbPath, true);


            string[] fileNames = System.IO.Directory.GetDirectories(directory);
            IFileNames pFileNames = new FileNamesClass();
            foreach (var item in fileNames)
            {
                pFileNames.Add(item);
            }
            IWorkspaceName pSourceWorksapceName = pWorkspaceFactory.GetWorkspaceName(directory, pFileNames);
            IWorkspaceName pToWorkspaceName = new WorkspaceNameClass();
            bool result = pWorkspaceFactory.Copy(pSourceWorksapceName, toDirectory, out pToWorkspaceName);
            IWorkspace pWorkspace = (pToWorkspaceName as IName).Open() as IWorkspace;
            IFeatureDataset pFeatureDataset_3D = (pWorkspace as IFeatureWorkspace).OpenFeatureDataset("MapData3D");

            //数据生成
            List<IFeatureLayer> featureLayers_3D =
               DataGenerator.Create_SymbolizedLayers_3D(ref floorPlan, ref pFeatureDataset_3D, "Route_ND");

            //生成最短路径
            IFeatureDataset featureDataset3D = EsriMapDataGenerator.FGDBHelper.Get_MapDataset3D(gdbPath);
            IFeatureClass stopsFeatureClass = (featureDataset3D.Workspace as IFeatureWorkspace).OpenFeatureClass("Stops");
            IFeatureClass pointBarriesFeatureClass = (featureDataset3D.Workspace as IFeatureWorkspace).OpenFeatureClass("Barries");
            INetworkDataset networkDataset = EsriMapDataGenerator.FGDBHelper.Get_NetworkDataset(gdbPath);



            //
            //var fromSP = (from p in floorPlan.PResult.SPolygons
            //              where p.PType == PolygonType.ElevatorShaft
            //              select p).First<SemanticPolygon>();


            //var toSP = (from p in floorPlan.PResult.SPolygons
            //            where p.PType == PolygonType.Balcony
            //            select p).First<SemanticPolygon>();

            //IFeature feature = null;
            //feature = stopsFeatureClass.CreateFeature();
            //feature.Shape = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(fromSP.FunctionRegionPoint.X, fromSP.FunctionRegionPoint.Y, 100);
            //feature.Store();

            //feature = stopsFeatureClass.CreateFeature();
            //feature.Shape = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(toSP.FunctionRegionPoint.X, toSP.FunctionRegionPoint.Y, 100);
            //feature.Store();



            //IPoint fromPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(fromSP.FunctionRegionPoint.X, fromSP.FunctionRegionPoint.Y, 100);
            //IPoint toPoint = ArcObjectsUtilities.GeometryUtility.Construct_Point3D(toSP.FunctionRegionPoint.X, toSP.FunctionRegionPoint.Y, 100);

            RouteAnalysisHelper rah = new RouteAnalysisHelper(ref stopsFeatureClass, ref pointBarriesFeatureClass, ref networkDataset);
            //navigatePath = rah.Calc_NavigationRoute(fromPoint, toPoint);
            //(navigatePath as ITransform3D).Move3D(0, 0, 1700);




            //图层显示
            IGroupLayer pGroupLayer = new GroupLayerClass();
            pGroupLayer.Name = "Map_3D";

            IGroupLayer pGroupLayer_route = new GroupLayerClass();
            pGroupLayer_route.Name = "Route3D";

            foreach (var item in featureLayers_3D)
            {
                //if (item.Name == "Wall3D" || item.Name == "Floor3D")
                //{
                //    //修改墙和楼板图层的透明度为50%

                //    ILayerEffects pLayerEffect = item as ILayerEffects;

                //    //if (pLayerEffect.SupportsTransparency)
                //    //    pLayerEffect.Transparency = 50;
                //}


                if (item.Name == "Nodes3D" || item.Name == "Sides3D")
                    pGroupLayer_route.Add(item);
                else
                    pGroupLayer.Add(item);
            }

            axSceneControl1.Scene.ClearLayers();






            axSceneControl1.Scene.AddLayer(pGroupLayer_route);
            axSceneControl1.Scene.AddLayer(pGroupLayer);

            axSceneControl1.SceneGraph.RefreshViewers();

     

            //NavigationController nc = new NavigationController( floorPlan, axSceneControl1, rah);
            nc.Show();




            //ICompositeLayer2 compositeLayer = naLayer as ICompositeLayer2;

            //ILayer layer = null;

            //for (int i = 0; i < compositeLayer.Count; i++)
            //{
            //    layer = compositeLayer.get_Layer(i);

            //    if (layer.Name == "路径")
            //        break;
            //}

            //IFeatureLayer featureLaer = layer as IFeatureLayer;

            //IFeatureCursor featureCursor = featureLayer.FeatureClass.Search(null, false);

            //IFeature feature2 = featureCursor.NextFeature();

            //IPolyline polyline = null;
            //while (feature2 != null)
            //{
            //    IGeometry geometry = feature2.Shape;
            //    polyline = geometry as IPolyline;



            //    feature2 = featureCursor.NextFeature();
            //}


     


        }
    }
}
