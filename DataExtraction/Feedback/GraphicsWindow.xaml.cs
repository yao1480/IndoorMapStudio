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
using ArcEngineDataUtilities.DataOperation;
using BCE.DataModels.Basic;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using Feedback.Controls;

namespace Feedback
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GraphicsWindow : Window
    {
        Result extractResult = null;
        IMapControlDefault iMapControl = null;
        string dxfPath = null;


        public GraphicsWindow(Result pExtractResult, string dxfPath)
        {
            InitializeComponent();

            this.dxfPath = dxfPath;
            rbtDisplayDXF.IsChecked = true;

            MapControlWrapper mapControlWrapper = wfh.Child as MapControlWrapper;
            iMapControl = mapControlWrapper.MapControl.Object as IMapControlDefault;
            extractResult = pExtractResult;
            cboType.SelectionChanged += cboType_SelectionChanged;
            mapControlWrapper.MapControl.OnMouseMove += MapControl_OnMouseMove;
            rbtDisplayDXF.Checked += rbtDisplayDXF_Checked;
            rbtHiddenDXF.Checked += rbtHiddenDXF_Checked;
        }

        void rbtHiddenDXF_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < iMapControl.LayerCount; i++)
            {
                ILayer pLayer = iMapControl.get_Layer(i);
                if (pLayer.Name == "底图")
                {
                    pLayer.Visible = false;
                    break;
                }
            }
            iMapControl.ActiveView.Refresh();
        }

        void rbtDisplayDXF_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < iMapControl.LayerCount; i++)
            {
                ILayer pLayer = iMapControl.get_Layer(i);
                if (pLayer.Name == "底图")
                {
                    pLayer.Visible = true;
                    break;
                }
            }
            iMapControl.ActiveView.Refresh();
        }

        void MapControl_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
        }


        void cboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (iMapControl.LayerCount < 1)
            {
                List<ILayer> pCadLayers = LayerHelper.GetCad2_FromFile(dxfPath);
                IGroupLayer pCadGroupLayer = new GroupLayerClass();
                pCadGroupLayer.Name = "底图";
                for (int i = 0; i < pCadLayers.Count; i++)
                {
                    pCadGroupLayer.Add(pCadLayers[i]);
                }

                iMapControl.ActiveView.FocusMap.AddLayer(pCadGroupLayer);
            }

            (iMapControl.ActiveView as IGraphicsContainer).DeleteAllElements();
            ComboBox cbo = sender as ComboBox;
            switch ((cbo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                case "墙":
                    if (extractResult.ER_ForWalls == null) return;
                    DrawingHelper.Drawing_Result_Wall(iMapControl, extractResult);
                    break;
                case "阳台扶手轮廓":
                    if (extractResult.ER_ForBalustrade == null) return;
                    DrawingHelper.Drawing_Result_Balustrade(iMapControl, extractResult);
                    break;
                case "门线":
                    if (extractResult.DoorLines == null) return;
                    DrawingHelper.Drawing_Result_DoorLines(iMapControl, extractResult);
                    break;
                case "窗线":
                    if (extractResult.WindowLines == null) return;
                    DrawingHelper.Drawing_Result_WindowLines(iMapControl, extractResult);
                    break;
                case "功能区":
                    if (extractResult.ER_ForFuncRegion == null) return;
                    DrawingHelper.Drawing_Result_FuncRegion(iMapControl, extractResult);
                    break;
            }

            iMapControl.ActiveView.Refresh();
        }
    }
}
