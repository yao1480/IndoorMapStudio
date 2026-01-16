using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Controls;

namespace MainProgram.Pages
{
    /// <summary>
    /// Page_DXF.xaml 的交互逻辑
    /// </summary>
    public partial class Page_DXF : Page, IPageInterfaces
    {
        #region 公开字段
        public static int PageIndex = 1;//页索引
        public CADImport.CADImportControls.CADViewerControl CadViewer = null;
        #endregion

        #region 内部字段
        //引用的外部变量
        MainWindow homeWindow = null;
        System.Windows.Controls.TabControl tabMainMenu = null;
        #endregion
        public Page_DXF(MainWindow homeWindow)
        {
            InitializeComponent();

            //填充外部变量
            this.homeWindow = homeWindow;
            this.tabMainMenu = homeWindow.tabMainMenu;

            CadViewer = new CADImport.CADImportControls.CADViewerControl();
            CadViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            //CadViewer.StatusBarPanel.Visible = true;
            CadViewer.ToolsPanel.Buttons.RemoveAt(0);//移除打开文件按钮

            CadViewer.ToolsPanel.Buttons[0].Enabled = true;//激活放大按钮
            winformHost.Child = CadViewer;

            btnLast.Click += btnLast_Click;
            btnNext.Click += btnNext_Click;
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
        #endregion

        #region 接口
        public void ActivateControls()
        {
            btnNext.IsEnabled = true;
            (tabMainMenu.Items[Page_FloorPlanConfig.PageIndex] as TabItem).IsEnabled = true;//激活图纸解析页标签
        }

        #endregion
    }
}
