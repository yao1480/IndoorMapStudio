//#define debug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Controls;
using MainProgram.Pages;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections;
using System.IO;
using BCE.DataModels;
using BCE.DataModels.Basic;
using ESRI.ArcGIS.Geodatabase;



namespace MainProgram
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region 内部字段
        StringBuilder sbStatus = null;//状态栏信息
        #endregion

        #region 公开字段或属性
        public FileType CurrentType
        {
            get
            {
                if (page_file == null)
                    return FileType.UnSelected;

                return page_file.CurrentType;
            }
        }

        public string FileName
        {
            get
            {
                if (page_file == null)
                    return null;
                return page_file.FileName;
            }
        }


        //进度标识
        public bool hasSetTask;//标识是否已经建立一个批次任务
        public bool HasConfigFloorPlan
        {
            get
            {
                if (page_floorPlanConfig == null)
                    return false;
                return page_floorPlanConfig.HasConfigDrawing;
            }
        }//标识是否已完成图纸配置
        public bool HasParsedDrawing
        {
            get
            {
                if (page_drawingParsing == null)
                    return false;
                return page_drawingParsing.HasParsedDrawing;
            }

        }//标识是否已经解析图纸
        public FloorPlan Drawing
        {
            get
            {
                return page_floorPlanConfig == null ? null : page_floorPlanConfig.Drawing;
            }
        }
        public DXFReader.Reader.DxfReader DxfReader
        {
            get
            {
                if (page_floorPlanConfig == null)
                    return null;
                return page_floorPlanConfig.DxfReader;
            }
        }
        public ParametersConfiguration FloorPlanConfig
        {
            get
            {
                if (page_floorPlanConfig == null)
                    return null;
                return page_floorPlanConfig.fpConfig;
            }
        }
        public string FgdbParentDirectory
        {
            get
            {
                return page_floorPlanConfig == null ? null : page_floorPlanConfig.FgdbParentDirectory;
            }
        }
        public string FgdbSafeName
        {
            get
            {
                return page_floorPlanConfig == null ? null : page_floorPlanConfig.FgdbSafeName;
            }
        }

        public string FgdbPath 
        {
            get { return FgdbParentDirectory + @"\" + FgdbSafeName+".gdb"; }
        }

        //模块页面
        public Page_File page_file;//文件页
        public Page_DXF page_dxf;//dxf页
        public Page_FloorPlanConfig page_floorPlanConfig;//图纸配置页
        public Page_DrawingParsing page_drawingParsing;//图纸解析页
        public Page_Map2D page_Map2D;//2D地图页
        public Page_PathDisplay page_PathDisplay;//路径展示页面
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //部分变量初始化
            frame.JournalOwnership = JournalOwnership.UsesParentJournal;
            //taskFactory = new TaskFactory();
            sbStatus = new StringBuilder();

            #region 订阅事件
            //基本事件
            btnMinimize.Click += btnMinimize_Click;
            btnClose.Click += btnClose_Click;
            gridTop.MouseDown += gridTop_MouseDown;
            btnMaxNormal.Click += btnMaxNormal_Click;
            btnHideOrShow.Click += btnHideOrShow_Click;
            btnExplandOrCollapseStatus.Click += btnExplandOrCollapseStatus_Click;
            this.Loaded += MainWindow_Loaded;

            
            #endregion
        }



        #region 事件
        #region 基本事件
        //最小化窗体
        void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Minimized)
                this.WindowState = System.Windows.WindowState.Minimized;
        }

        //最大化或正常显示窗体
        void btnMaxNormal_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width == SystemParameters.WorkArea.Width)
            {
                //设为正常窗口
                pathMax1.Visibility = System.Windows.Visibility.Hidden;
                pathMax2.Visibility = System.Windows.Visibility.Hidden;
                pathNormal.Visibility = System.Windows.Visibility.Visible;

                const double rate_width = 0.6d;
                const double rate_height = 0.8d;
                this.Left = SystemParameters.WorkArea.Width * (1 - rate_width) / 2;
                this.Top = SystemParameters.WorkArea.Height * (1 - rate_height) / 2;
                this.Width = SystemParameters.WorkArea.Width * rate_width;
                this.Height = SystemParameters.WorkArea.Height * rate_height;

                this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;

            }
            else
            {
                //设置为最大化的窗口

                pathNormal.Visibility = System.Windows.Visibility.Hidden;
                pathMax1.Visibility = System.Windows.Visibility.Visible;
                pathMax2.Visibility = System.Windows.Visibility.Visible;

                this.Left = 0d;
                this.Top = 0d;
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;

                this.ResizeMode = System.Windows.ResizeMode.NoResize;
            }
        }

        //关闭程序
        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("要关闭程序吗？", "", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No))
                Application.Current.Shutdown();
        }

        //移动窗体
        void gridTop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //控制侧边栏显隐
        void btnHideOrShow_Click(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();

            if (spLeftSubMenus.Margin.Left >= 0)
            {
                thicknessAnimation.From = new Thickness(0, 0, 0, 0);
                thicknessAnimation.To = new Thickness(-121, 0, 0, 0);
                (btnHideOrShow.RenderTransform as RotateTransform).Angle = 180;
                tbkHideOrShow.Text = "显示侧栏";
            }
            else
            {
                thicknessAnimation.From = new Thickness(-121, 0, 0, 0);
                thicknessAnimation.To = new Thickness(0, 0, 0, 0);
                (btnHideOrShow.RenderTransform as RotateTransform).Angle = 0;
                tbkHideOrShow.Text = "隐藏侧栏";
            }

            thicknessAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            Storyboard.SetTargetName(thicknessAnimation, spLeftSubMenus.Name);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(StackPanel.MarginProperty));

            Storyboard storybord = new Storyboard();
            storybord.Children.Add(thicknessAnimation);
            storybord.Begin(spLeftSubMenus);
        }

        void btnExplandOrCollapseStatus_Click(object sender, RoutedEventArgs e)
        {


            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();



            if (gridStatus.Margin.Bottom >= 0)
            {
                //收起消息栏
                thicknessAnimation.From = new Thickness(0, 0, 0, gridStatus.Margin.Bottom);
                thicknessAnimation.To = new Thickness(0, 0, 0, -120);
                (btnExplandOrCollapseStatus.RenderTransform as RotateTransform).Angle = 0d;
            }
            else
            {
                //展开消息栏
                thicknessAnimation.From = new Thickness(0, 0, 0, gridStatus.Margin.Bottom);
                thicknessAnimation.To = new Thickness(0, 0, 0, 0);
                (btnExplandOrCollapseStatus.RenderTransform as RotateTransform).Angle = 180d;
            }

            thicknessAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            Storyboard.SetTargetName(thicknessAnimation, gridStatus.Name);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Grid.MarginProperty));

            Storyboard storybord = new Storyboard();
            storybord.Children.Add(thicknessAnimation);
            storybord.Begin(gridStatus);
        }


        //初始化设置
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tabMainMenu.SelectionChanged += tabMainMenu_SelectionChanged;

            //未指定文件类型的初始化状态设置
            SelectPattern(FileType.UnSelected);
        }

        //主菜单切换
        void tabMainMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentSubMenuIndex = tabMainMenu.SelectedIndex;

            //跳转到对应模块页
            goto_Page(currentSubMenuIndex);


            //切换到对应子菜单
            UIElement subTab = null;
            for (int i = 0; i < spLeftSubMenus.Children.Count; i++)
            {
                subTab = spLeftSubMenus.Children[i];
                if (i != currentSubMenuIndex)
                    subTab.Visibility = System.Windows.Visibility.Collapsed;
                else
                    subTab.Visibility = System.Windows.Visibility.Visible;
            }

            //如果侧边栏处于隐藏状态，则显示
            if (spLeftSubMenus.Margin.Left < 0)
            {
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();

                thicknessAnimation.From = new Thickness(-121, 0, 0, 0);
                thicknessAnimation.To = new Thickness(0, 0, 0, 0);
                (btnHideOrShow.RenderTransform as RotateTransform).Angle = 0;
                tbkHideOrShow.Text = "隐藏侧栏";


                thicknessAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                Storyboard.SetTargetName(thicknessAnimation, spLeftSubMenus.Name);
                Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(StackPanel.MarginProperty));

                Storyboard storybord = new Storyboard();
                storybord.Children.Add(thicknessAnimation);
                storybord.Begin(spLeftSubMenus);
            }
        }

        #endregion

        #region 业务事件
        #region Page_DXF
        //dxf加载
        void page_dxf_Loaded(object sender, RoutedEventArgs e)
        {
            //更新状态栏信息
            RefreshStatusMessage("正在加载 " + FileName + " ... ");

            SetCursor(Cursors.Wait);
            System.Windows.Forms.Application.DoEvents();

            page_dxf.CadViewer.EndLoadFile += (b) =>
            {
                page_dxf.Loaded -= page_dxf_Loaded;//卸载事件
                SetCursor(Cursors.Arrow);

                //控件可用性设置
                (sender as IPageInterfaces).ActivateControls();

                RefreshStatusMessage("加载完毕！");
            };

            //加载DXF
#if debug
            page_dxf.Loaded -= page_dxf_Loaded;//卸载事件
            SetCursor(Cursors.Arrow);

            //控件可用性设置
            (sender as IPageInterfaces).ActivateControls();

            RefreshStatusMessage("加载完毕！");
            return;
#endif
            page_dxf.CadViewer.LoadFile(FileName);
        }
        #endregion

        #region Page_Map2D
        
        #endregion

        #endregion
        #endregion

        #region 公开方法
        /// <summary>
        /// 选择工作模式
        /// </summary>
        /// <param name="fileType"></param>
        public void SelectPattern(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.UnSelected:
                    pattern_UnIndicated();
                    break;
                case FileType.DXF:
                    pattern_CAD();
                    break;
                case FileType.XML:
                    break;
                case FileType.SXD:
                    break;
            }
        }

        /// <summary>
        /// 导航到指定页
        /// </summary>
        /// <param name="pageIndex">基于0的页索引</param>
        public void Navigate(int pageIndex)
        {
            if (pageIndex < 0) return;
            tabMainMenu.SelectedIndex = pageIndex;
        }

        /// <summary>
        /// 更新状态栏信息
        /// </summary>
        /// <param name="message"></param>
        public void RefreshStatusMessage(string message)
        {
            sbStatus.AppendLine(message);
            tbxStatus.Text = sbStatus.ToString();
            tbxStatus.ScrollToEnd();
        }

        /// <summary>
        /// 设置鼠标状态
        /// </summary>
        /// <param name="cursor"></param>
        public void SetCursor(System.Windows.Input.Cursor cursor)
        {
            gridRoot.Cursor = cursor;

            Page currentPage = frame.Content as Page;
            if (currentPage != null)
                currentPage.Cursor = cursor;
        }


        public IFeatureDataset Get_MapDataset2D()
        {
            if (!HasConfigFloorPlan) return null;

            string fgdbPath = FgdbParentDirectory + @"\" + FgdbSafeName + ".gdb";
            return EsriMapDataGenerator.FGDBHelper.Get_MapDataset2D(fgdbPath);
        }

        public IFeatureDataset Get_MapDataset3D()
        {
            if (!HasConfigFloorPlan) return null;

            string fgdbPath = FgdbParentDirectory + @"\" + FgdbSafeName + ".gdb";
            return EsriMapDataGenerator.FGDBHelper.Get_MapDataset3D(fgdbPath);
        }
        #endregion

        #region 内部方法
        //模式设置
        void pattern_UnIndicated()
        {
            //冻结除文件之外的所有选项卡
            for (int i = 1; i < tabMainMenu.Items.Count; i++)
            {
                (tabMainMenu.Items[i] as TabItem).IsEnabled = false;
            }

            //激活打开按钮
            btnOpenFile.IsEnabled = true;

#if !debug
            if (page_dxf != null)
                page_dxf.CadViewer.CloseFile();
#endif


            //释放各页资源
            page_dxf = null;
            page_floorPlanConfig = null;
            page_drawingParsing = null;



            //导航到文件页
            goto_PageFile();
        }
        void pattern_CAD()
        {
            //激活CAD视图选项卡
            (tabMainMenu.Items[1] as TabItem).IsEnabled = true;

            //将触发页面跳转事件
            tabMainMenu.SelectedIndex = 1;
        }


        //导航控制
        void goto_Page(int pageIndex)
        {
            switch (pageIndex)
            {
                case 0:
                    goto_PageFile();
                    break;
                case 1:
                    goto_Page_DXF();
                    break;
                case 2:
                    goto_Page_DrawingConfig();
                    break;
                case 3:
                    goto_Page_DrawingParsing();
                    break;
                case 4:
                    goto_Page_Map2D();
                    break;
                case 5:
                    goto_Page_PathDisplay();
                    break;
            }
        }
        void goto_PageFile()
        {
            if (page_file == null)
            {
                page_file = new Page_File(this);
                btnOpenFile.IsEnabled = true;
            }

            frame.Navigate(page_file);
        }
        void goto_Page_DXF()
        {
            if (page_dxf == null)
            {
                page_dxf = new Page_DXF(this);
                page_dxf.Loaded += page_dxf_Loaded;
            }
            frame.Navigate(page_dxf);
        }
        void goto_Page_DrawingConfig()
        {
            if (page_floorPlanConfig == null)
                page_floorPlanConfig = new Page_FloorPlanConfig(this);

            frame.Navigate(page_floorPlanConfig);
        }
        void goto_Page_DrawingParsing()
        {
            if (page_drawingParsing == null)
                page_drawingParsing = new Page_DrawingParsing(this);

            frame.Navigate(page_drawingParsing);
        }

        void goto_Page_Map2D()
        {
            if (page_Map2D == null)
                page_Map2D = new Page_Map2D(this);

            frame.Navigate(page_Map2D);
        }
        
        void goto_Page_PathDisplay()
        {
            if (page_PathDisplay == null)
                page_PathDisplay = new Page_PathDisplay(this);

            frame.Navigate(page_PathDisplay);
        }
        //在指定文件夹中检索指定扩展名的第一个文件名
        private string get_FirstFileByExtension(string folderPath, string extension)
        {
            string resultFileName = "";
            string[] fileNames = Directory.GetFiles(folderPath);


            for (int i = 0; i < fileNames.Length; i++)
            {
                int pointIndex = fileNames[i].LastIndexOf(".");
                string pExtension = fileNames[i].Substring(pointIndex + 1, 3);
                if (pExtension == extension)
                {
                    resultFileName = fileNames[i];
                    break;
                }
            }

            return resultFileName;
        }
        #endregion
    }
}
