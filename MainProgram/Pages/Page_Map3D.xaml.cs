using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BCE.DataModels;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using EsriMapDataGenerator;
using MainProgram.Assets;
using MainProgram.Assets.Controls;
using Microsoft.Win32;

namespace MainProgram.Pages
{
    /// <summary>
    /// Page_3DMap.xaml 的交互逻辑
    /// </summary>
    public partial class Page_Map3D : System.Windows.Controls.Page, IPageInterfaces
    {
        #region 内部字段
        //引用的主页控件
        MainWindow homeWindow = null;
        TabControl tabMainMenu = null;

        //4个图层组名称
        //const string m_routeAnalysisLayerName = "路径分析";
        //const string m_route3DLayerName = "3D 路径";
        //const string m_map2DLayerNamne = "3D 平面地图";
        //const string m_map3DLayerNamne = "3D 立体地图";

        //const string networkDatasetName = "Route_ND";//路径网络数据集名称
        //const string name_mapData3D = "MapData3D";//存放2D地图数据的数据集名称
        #endregion

        #region 公开字段
        public static int PageIndex = 5;//页索引
        public AxSceneControl SceneControl = null;
        #endregion

        public Page_Map3D(MainWindow homeWindow)
        {
            InitializeComponent();

            //变量初始化
            SceneControl = scWrapper.SceneControl;
            this.homeWindow = homeWindow;
            this.tabMainMenu = homeWindow.tabMainMenu;

            //更改图层树控件中的标题
            //MapControl.Name = "Map2D";
            //mapControlWrapper.TOCControl.Update();
            //mapControlWrapper.TOCControl.Refresh();

            //事件订阅
            btnLast.Click += btnLast_Click;
            btnNext.Click += btnNext_Click;
            btnCreateMapData3D.Click += btnCreateMapData3D_Click;
            //homeWindow.btnPathRoaming.Click += btnPathRoaming_Click;
        }


        #region 事件
        void btnCreateMapData3D_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            SceneControl.Scene.ClearLayers();


            //在图纸配置页已创建FGDB并能保证FGDB为初始状态


            //创建2/3D符号化图层
            IFeatureDataset mapDataset2D = homeWindow.Get_MapDataset2D();
            IFeatureDataset mapDataset3D = homeWindow.Get_MapDataset3D();
            FloorPlan drawing = homeWindow.Drawing;
            List<IFeatureLayer> featureLayers2D =
                EsriMapDataGenerator.DataGenerator.Create_SymbolizedLayers_2D(ref drawing, ref mapDataset2D);
            List<IFeatureLayer> featureLayers3D
                = EsriMapDataGenerator.DataGenerator.Create_SymbolizedLayers_3D(ref drawing, ref mapDataset3D, Resource.NetWorkDatasetName);


            //创建 4个 图层组： 
            SceneControl.Scene.AddLayer(getGroupLayer_Map2D(ref featureLayers2D, Resource.GroupLayer3DPlaneMap));
            SceneControl.Scene.AddLayer(getGroupLayer_Map3D(ref featureLayers3D, Resource.GroupLayer3DMap));
            SceneControl.Scene.AddLayer(getGroupLayer_Routes3D(ref featureLayers3D, Resource.GroupLayer3DRoute));
            SceneControl.Scene.AddLayer(getGroupLayer_RouteAnalysis(Resource.GroupLayer3DRouteAnalysis));


            SceneControl.Refresh();

            //激活相关控件
            //ActivateControls();

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message );

            //}
        }
        void btnLast_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex - 1);
        }
        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex + 1);
        }
        #endregion


        #region 公开方法
        /// <summary>
        /// 路劲漫游
        /// </summary>
        public void PathRoaming()
        {
            if (SceneControl.Scene.LayerCount == 0)
            {
                MessageBox.Show("尚未生成3D地图数据！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IFeatureDataset featureDataset3D = EsriMapDataGenerator.FGDBHelper.Get_MapDataset3D(homeWindow.FgdbPath);
                IFeatureClass stopsFeatureClass = (featureDataset3D.Workspace as IFeatureWorkspace).OpenFeatureClass(Resource.StopsFeatureClassName);
                IFeatureClass pointBarriesFeatureClass = (featureDataset3D.Workspace as IFeatureWorkspace).OpenFeatureClass(Resource.PointBarriesFeatureClassName);
                INetworkDataset networkDataset = EsriMapDataGenerator.FGDBHelper.Get_NetworkDataset(homeWindow.FgdbPath);

                RouteAnalysisHelper rah = new RouteAnalysisHelper(ref stopsFeatureClass, ref pointBarriesFeatureClass, ref networkDataset);
                NavigationController nc = new NavigationController(homeWindow.Drawing, SceneControl, rah);
                nc.Owner = homeWindow;
                nc.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("无法进行路径漫游: {0}) ", ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        public void SaveAsSxd()
        {
        }
        #endregion




        #region 内部方法
        IGroupLayer getGroupLayer_RouteAnalysis(string layerName)
        {
            IGroupLayer routeAnalysisGroupLayer = new GroupLayerClass();
            (routeAnalysisGroupLayer as ILayer).Name = layerName;


            IGraphicsContainer3D graphicsContainer = null;

            //停靠点图层
            graphicsContainer = new GraphicsLayer3DClass();
            (graphicsContainer as ILayer).Name = Resource.StopsLayerName;
            routeAnalysisGroupLayer.Add(graphicsContainer as ILayer);

            //路径图层
            graphicsContainer = new GraphicsLayer3DClass();
            (graphicsContainer as ILayer).Name = Resource.ResultRouteLayerName;
            routeAnalysisGroupLayer.Add(graphicsContainer as ILayer);

            return routeAnalysisGroupLayer;
        }

        IGroupLayer getGroupLayer_Routes3D(ref List<IFeatureLayer> featureLayers3D, string layerName)
        {
            IGroupLayer routesGroupLayer = new GroupLayerClass();
            (routesGroupLayer as ILayer).Name = layerName;



            var layers = (from p in featureLayers3D
                          where
                              p.Name == "Nodes3D" ||
                              p.Name == "Sides3D"
                          select p).ToList<ILayer>();

            for (int i = 0; i < layers.Count; i++)
                routesGroupLayer.Add(layers[i]);



            return routesGroupLayer;
        }

        IGroupLayer getGroupLayer_Map2D(ref List<IFeatureLayer> featureLayers2D, string layerName)
        {
            IGroupLayer map2DgroupLayer = new GroupLayerClass();
            (map2DgroupLayer as ILayer).Name = layerName;

            foreach (var item in featureLayers2D)
            {
                map2DgroupLayer.Add(item);
            }

            return map2DgroupLayer;

        }

        IGroupLayer getGroupLayer_Map3D(ref List<IFeatureLayer> featureLayers3D, string layerName)
        {
            IGroupLayer map3DgroupLayer = new GroupLayerClass();
            (map3DgroupLayer as ILayer).Name = layerName;

            #region 分组

            IGroupLayer groupLayer = null;
            List<ILayer> layers = null;

            //环境组
            groupLayer = new GroupLayerClass();
            groupLayer.Name = "环境";
            layers = (from p in featureLayers3D
                      where p.Name == "Land3D"
                      select p).ToList<ILayer>();

            foreach (var item in layers)
            {
                groupLayer.Add(item);
            }
            map3DgroupLayer.Add(groupLayer);

            //框架图层组
            groupLayer = new GroupLayerClass();
            groupLayer.Name = "框架";
            layers = (from p in featureLayers3D
                      where
                          p.Name == "Floor3D" ||
                          p.Name == "Wall3D"
                      select p).ToList<ILayer>();

            foreach (var item in layers)
            {
                groupLayer.Add(item);
            }
            map3DgroupLayer.Add(groupLayer);

            //建筑附件图层组
            groupLayer = new GroupLayerClass();
            groupLayer.Name = "建筑附件";
            layers = (from p in featureLayers3D
                      where
                          p.Name == "Door3D" ||
                          p.Name == "Window3D" ||
                          p.Name == "Stair3D" ||
                          p.Name == "Elevator3D"
                      select p).ToList<ILayer>();

            foreach (var item in layers)
            {
                groupLayer.Add(item);
            }
            map3DgroupLayer.Add(groupLayer);


            //路径组
            //pGroupLayer = new GroupLayerClass();
            //pGroupLayer.Name = "路径";
            //layers = (from p in featureLayers3D
            //          where
            //              p.Name == "Nodes3D" ||
            //              p.Name == "Sides3D"
            //          select p).ToList<ILayer>();

            //foreach (var item in layers)
            //{
            //    groupLayer.Add(item);
            //}
            //map3DgroupLayer.Add(groupLayer);

            #endregion

            return map3DgroupLayer;
        }
        #endregion

        #region IPageInterface
        public void ActivateControls()
        {
            //btnNext.IsEnabled = true;
            //(tabMainMenu.Items[Page_Map3D.PageIndex] as TabItem).IsEnabled = true;//激活2D地图页标签
        }
        #endregion
    }
}
