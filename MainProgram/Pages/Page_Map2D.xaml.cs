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
using Microsoft.Win32;

namespace MainProgram.Pages
{
    /// <summary>
    /// Page_2DMap.xaml 的交互逻辑
    /// </summary>
    public partial class Page_Map2D : System.Windows.Controls.Page,IPageInterfaces
    {
        #region 内部字段
        //引用的外部变量
        MainWindow homeWindow = null;
        TabControl tabMainMenu = null;

        //内部字段
        IFeatureWorkspace pFeatureWorkspace = null;
        const string name_mapData2D = "MapData2D";//存放2D地图数据的数据集名称
        #endregion

        #region 公开字段
        public static int PageIndex = 4;//页索引
        public AxMapControl MapControl = null;
        #endregion


        public Page_Map2D(MainWindow homeWindow)
        {
            InitializeComponent();

            //变量初始化
            this.homeWindow = homeWindow;
            this.tabMainMenu = homeWindow.tabMainMenu;

            MapControl = mapControlWrapper.MapControl;
            

            //更改图层树控件中的标题
            //MapControl.Name = "Map2D";
            //mapControlWrapper.TOCControl.Update();
            //mapControlWrapper.TOCControl.Refresh();

            //事件订阅
            btnLast.Click += btnLast_Click;
            btnNext.Click += btnNext_Click;
            btnCreateMapData2D.Click += btnCreateMapData2D_Click;
        }




        #region 事件
        void btnLast_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex - 1);
        }

        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex + 1);
        }


        void btnCreateMapData2D_Click(object sender, RoutedEventArgs e)
        {


            //try
            //{
            MapControl.ClearLayers();

            //在图纸配置页已创建FGDB并能保证FGDB为初始状态
            IFeatureDataset pMapDataset2D = homeWindow.Get_MapDataset2D();
            FloorPlan drawing = homeWindow.Drawing;

            //创建符号化图层
            List<IFeatureLayer> pLayers
                = EsriMapDataGenerator.DataGenerator.Create_SymbolizedLayers_2D(ref drawing, ref pMapDataset2D);

            for (int i = 0; i < pLayers.Count; i++)
            {
                MapControl.Map.AddLayer(pLayers[i]);
            }

            MapControl.Refresh();

            ActivateControls();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message );

            //}
        }

        #endregion

        #region IPageInterfaces
        public void ActivateControls()
        {
            btnNext.IsEnabled = true;
            (tabMainMenu.Items[Page_Map3D.PageIndex] as TabItem).IsEnabled = true;//激活3D地图页标签
        }
        #endregion
    }
}
