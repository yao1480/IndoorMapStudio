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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Collections;
using DotNetCommonUtilities.BasicWrapper;
using System.Threading;
using BCE.DataModels;
using BCE.DataModels.Basic;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;

namespace MainProgram.Pages
{
    /// <summary>
    /// Page_FloorPlanConfig.xaml 的交互逻辑
    /// </summary>
    public partial class Page_FloorPlanConfig : Page, IPageInterfaces
    {
        #region 公开字段
        public static int PageIndex = 2;//页索引
        public bool HasConfigDrawing = false;
        public ParametersConfiguration fpConfig;
        public DXFReader.Reader.DxfReader DxfReader = null;
        public FloorPlan Drawing = null;

        public string FgdbParentDirectory
        {
            get { return tbxFGDBPath.Text.Trim(); }
        }
        public string FgdbSafeName
        {
            get { return tbxFGDBName.Text.Trim(); }
        }
        #endregion

        #region 内部字段
        //引用的外部变量
        MainWindow homeWindow = null;
        TabControl tabMainMenu = null;


        Button btnConfigFromFile = null;
        Button btnExportDrawingConfig = null;
        Button btnResetConfig = null;

        //其他
        LayerNamesForBind layerNamesForBind = null;
        #endregion

        public Page_FloorPlanConfig(MainWindow homeWindow)
        {
            InitializeComponent();

            //填充引用的外部变量
            this.homeWindow = homeWindow;
            tabMainMenu = homeWindow.tabMainMenu;
            btnConfigFromFile = homeWindow.btnConfigFromFile;
            btnExportDrawingConfig = homeWindow.btnExportDrawingConfig;
            btnResetConfig = homeWindow.btnResetConfig;



            this.fpConfig = new ParametersConfiguration(homeWindow.FileName);
            this.Drawing = new BCE.DataModels.FloorPlan(ref fpConfig);



            //事件订阅
            btnConfigFromFile.Click += btnConfigFromFile_Click;
            btnExportDrawingConfig.Click += btnExportDrawingConfig_Click;
            btnResetConfig.Click += btnResetConfig_Click;
            btnSelectFGDBPath.Click += btnSelectFGDBPath_Click;

            btnEnsureConfig.Click += btnEnsureConfig_Click;

            btnLast.Click += btnLast_Click;
            btnNext.Click += btnNext_Click;

            this.Loaded += Page_FloorPlanConfig_Loaded;
        }




        #region 公开方法
        /// <summary>
        /// 重置图纸（该方法仅由图纸解析页的图纸解析命令调用; 需要同时重置DxfReader和FloorPlan，但配置不需要重新设置）
        /// </summary>
        public void ResetFloorPlan()
        {
            DxfReader = new DXFReader.Reader.DxfReader(fpConfig.DXF_Path, true);
            Drawing = new FloorPlan(ref fpConfig);
            this.homeWindow.page_drawingParsing.btnParsingDrawing.IsEnabled = true;//激活图纸解析页图纸解析按钮
        }

        #endregion

        #region 接口
        public void ActivateControls()
        {
            btnNext.IsEnabled = true;
            (tabMainMenu.Items[Page_DrawingParsing.PageIndex] as TabItem).IsEnabled = true;//激活图纸解析页标签
        }

        #endregion


        #region 事件
        void Page_FloorPlanConfig_Loaded(object sender, RoutedEventArgs e)
        {
            if (DxfReader == null)
            {
                //try
                //{
                homeWindow.RefreshStatusMessage("正在解析DXF图纸...");
                homeWindow.SetCursor(Cursors.Wait);
                System.Windows.Forms.Application.DoEvents();

                this.DxfReader = new DXFReader.Reader.DxfReader(fpConfig.DXF_Path, true);


                //更新图纸配置页UI
                updateUI();


                homeWindow.RefreshStatusMessage("DXF图纸解析完毕！");
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(500);
                homeWindow.RefreshStatusMessage("正在加载图层信息...");
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(500);

                homeWindow.SetCursor(Cursors.Arrow);
                homeWindow.RefreshStatusMessage("图层信息加载完毕！");


                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
                //finally
                //{
                //    //控件可用性设置 1/2
                //    (tabMainMenu.Items[PageIndex] as TabItem).IsEnabled = true;//激活图纸配置选项卡
                //}
            }
        }

        void btnConfigFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "导入图纸解析配置文件";
            ofd.Filter = "XML文件(*.xml)|*.xml";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    //XML验证逻辑
                    System.Xml.Serialization.XmlSerializer xmlSerializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(ParametersConfiguration));

                    System.IO.StreamReader sr = new StreamReader(ofd.FileName);
                    ParametersConfiguration drawingConfig = xmlSerializer.Deserialize(sr) as ParametersConfiguration;
                    sr.Close();

                    if (drawingConfig == null)
                    {
                        MessageBox.Show("图纸解析配置文件无效！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    //应用配置


                    #region  应用图层配置
                    layerNamesForBind.Reset();
                    applyOneLayerMappingFromFile(ref drawingConfig.LayerMapping.Wall, ref layerNamesForBind.Wall_LayerNames);
                    applyOneLayerMappingFromFile(ref drawingConfig.LayerMapping.Balcony, ref layerNamesForBind.Balcony_LayerNames);
                    applyOneLayerMappingFromFile(ref drawingConfig.LayerMapping.Door, ref layerNamesForBind.Door_LayerNames);
                    applyOneLayerMappingFromFile(ref drawingConfig.LayerMapping.Window, ref layerNamesForBind.Window_LayerNames);
                    applyOneLayerMappingFromFile(ref drawingConfig.LayerMapping.Stairs, ref layerNamesForBind.Stair_LayerNames);
                    applyOneLayerMappingFromFile(ref drawingConfig.LayerMapping.Elevator, ref layerNamesForBind.Elevator_LayerNames);
                    applyOneLayerMappingFromFile(ref drawingConfig.LayerMapping.Text, ref layerNamesForBind.Label_LayerNames);



                    homeWindow.RefreshStatusMessage("图纸解析配置导入成功！");
                    MessageBox.Show("导入成功！", "", MessageBoxButton.OK, MessageBoxImage.Information);

                    #endregion
                }
                catch (Exception ex)
                {
                    homeWindow.RefreshStatusMessage(string.Format("读取配置文件出错： {0}", ex.Message));
                    MessageBox.Show(string.Format("读取配置文件出错： {0}", ex.Message), "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void btnExportDrawingConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "选择图纸解析配置文件保存位置";
            sfd.Filter = "XML文件(*.xml)|*.xml";
            sfd.FileName = "DrawingParsingConfig";

            if (sfd.ShowDialog() == true)
            {
                StreamWriter sw = null;

                try
                {
                    sw = new StreamWriter(sfd.FileName);
                    System.Xml.Serialization.XmlSerializer xmlSerializer =
                        new System.Xml.Serialization.XmlSerializer(this.Drawing.Configuration.GetType());
                    xmlSerializer.Serialize(sw, Drawing.Configuration);

                    sw.Close();

                    homeWindow.RefreshStatusMessage("图纸解析配置导出成功！");
                    MessageBox.Show("导出成功！", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    if (sw != null)
                        sw.Close();

                    homeWindow.RefreshStatusMessage(string.Format("图纸配置导出失败：{0}", ex.Message));
                    MessageBox.Show(string.Format("图纸配置导出失败：{0}", ex.Message), "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void btnResetConfig_Click(object sender, RoutedEventArgs e)
        {
            if (layerNamesForBind != null)
            {
                layerNamesForBind.Reset();
                btnNext.IsEnabled = false;


                HasConfigDrawing = false;

                if (homeWindow.page_drawingParsing != null)
                    homeWindow.page_drawingParsing.HasParsedDrawing = false;//重置图纸配置状态时同时重置图纸解析状态

                homeWindow.RefreshStatusMessage("图纸配置被重置！");
            }
        }

        void btnEnsureConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                applyBasicConfig();//配置基本属性
                applyLayerMapping(); //配置图层映射
                checkFGDBConfig();//检查地图数据存储路径及名称配置
                HasConfigDrawing = true;

                //控件可用性设置 
                ActivateControls();

                homeWindow.RefreshStatusMessage("图纸配置已完成！");
                MessageBox.Show("图纸配置已完成！", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void btnSelectFGDBPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "请选择用来存储地图数据(FGDB)的位置";

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                tbxFGDBPath.Text = fbd.SelectedPath;
        }

        //前后导航
        void btnLast_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex - 1);
        }
        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex + 1);
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// 更新UI(加载分类图层信息+图纸路径信息)
        /// </summary>
        private void updateUI()
        {
            //更新图层路径
            this.tbxDxfPath.Text = fpConfig.DXF_Path;


            if (layerNamesForBind == null)
                layerNamesForBind = new LayerNamesForBind();
            else
                layerNamesForBind.Clear();




            //更新图层映射待选项
            layerNamesForBind.Wall_LayerNames = filterAndOrderLayerNames(LayerType.Wall, ref   DxfReader.LayerNames);
            layerNamesForBind.Balcony_LayerNames = filterAndOrderLayerNames(LayerType.Balcony, ref DxfReader.LayerNames);
            layerNamesForBind.Door_LayerNames = filterAndOrderLayerNames(LayerType.Door, ref DxfReader.LayerNames);
            layerNamesForBind.Window_LayerNames = filterAndOrderLayerNames(LayerType.Window, ref DxfReader.LayerNames);
            layerNamesForBind.Stair_LayerNames = filterAndOrderLayerNames(LayerType.Stair, ref DxfReader.LayerNames);
            layerNamesForBind.Elevator_LayerNames = filterAndOrderLayerNames(LayerType.Elevator, ref DxfReader.LayerNames);
            layerNamesForBind.Label_LayerNames = filterAndOrderLayerNames(LayerType.Label, ref DxfReader.LayerNames);

            lbWall.ItemsSource = layerNamesForBind.Wall_LayerNames;
            lbBalcony.ItemsSource = layerNamesForBind.Balcony_LayerNames;
            lbDoor.ItemsSource = layerNamesForBind.Door_LayerNames;
            lbWindow.ItemsSource = layerNamesForBind.Window_LayerNames;
            lbStair.ItemsSource = layerNamesForBind.Stair_LayerNames;
            lbElevator.ItemsSource = layerNamesForBind.Elevator_LayerNames;
            lbLabel.ItemsSource = layerNamesForBind.Label_LayerNames;
        }

        /// <summary>
        /// 应用基本图纸配置
        /// </summary>
        private void applyBasicConfig()
        {
            try
            {
                //配置基本属性
                //1. 建筑
                fpConfig.RepeatCount = int.Parse(tbxNumOfFloorLevels.Text.Trim());
                fpConfig.StartHeight = double.Parse(tbxStartHeight.Text.Trim());
                fpConfig.Floor_Thickness = double.Parse((cboFloorThickness.SelectedValue as ComboBoxItem).Content.ToString());
                fpConfig.Floor_Headroom = double.Parse((cboFloorHeadroom.SelectedValue as ComboBoxItem).Content.ToString());

                //2. 门窗
                fpConfig.Door_StartHeight = double.Parse(tbxDoorStartHeight.Text.Trim());
                fpConfig.Door_Heigth = double.Parse(tbxDoorheight.Text.Trim());
                fpConfig.Window_StartHeight = double.Parse(tbxWindowStartHeight.Text.Trim());
                fpConfig.Window_Heigth = double.Parse(tbxWindowheight.Text.Trim());

                //3. 楼梯
                fpConfig.StairHandrail_Height = double.Parse((cboStairHandrailHeight.SelectedValue as ComboBoxItem).Content.ToString());
                fpConfig.StairHandrail_Width = double.Parse(tbxStairHandrailWidth.Text.Trim());
                fpConfig.StairHandrail_Thickness = double.Parse(tbxStairHandrailThickness.Text.Trim());

                //4. 电梯
                fpConfig.Elevator_Height = double.Parse(tbxLiftCarHeight.Text.Trim());
                fpConfig.Elevator_Door_Heigth = double.Parse(tbxElevatorDoorHeight.Text.Trim());
                fpConfig.Elevator_Door_Thickness = double.Parse(tbxElevatorDoorThickness.Text.Trim());

                //阳台
                fpConfig.BalconyHandrail_Height = double.Parse((cboBalusterHandrailHeight.SelectedValue as ComboBoxItem).Content.ToString());


                //5. 阈值
                fpConfig.ThreadValue_MaxWallWidth = double.Parse((cboWallWidthThreshold.SelectedValue as ComboBoxItem).Content.ToString());
                fpConfig.ThreadValue_SamePoint = double.Parse((cboSamePointThreshold.SelectedValue as ComboBoxItem).Content.ToString());
                fpConfig.Threadshold_EqualAngle = double.Parse((cboEqualAngleThreshold.SelectedValue as ComboBoxItem).Content.ToString());

            }
            catch (Exception ex)
            {
                throw new InvalidDataException(string.Format("图纸基本配置存在错误： {0}", ex.Message));
            }
        }

        /// <summary>
        /// 检查地图数据(FGDB)存储配置，若未创建,则创建FGDB；否则，将检查FGDB是否为初始状态
        /// </summary>
        private void checkFGDBConfig()
        {
            if (string.IsNullOrEmpty(FgdbParentDirectory) || string.IsNullOrWhiteSpace(FgdbParentDirectory))
                throw new ArgumentException("未选择FGDB存储路径！");

            if (string.IsNullOrEmpty(FgdbSafeName) || string.IsNullOrWhiteSpace(FgdbSafeName))
                throw new ArgumentException("无效的FGDB名称！");

            if (FgdbSafeName.Contains(".gdb"))
                tbxFGDBName.Text = FgdbSafeName.Remove(FgdbSafeName.IndexOf("."));//移除.gdb后缀名


            string newFgdbPath = string.Format(@"{0}\{1}.gdb", FgdbParentDirectory, FgdbSafeName);
            if (!System.IO.Directory.Exists(newFgdbPath))
            {
                //若在newFgdbPath路径下未曾创建FGDB，则创建
                string exePath = System.Environment.CurrentDirectory;
                string templateFgdbParentDirectory = string.Format(@"{0}\{1}", exePath, @"FGDBTemplate");
                EsriMapDataGenerator.FGDBHelper.CreateMapDataFGDB_FromTemplate(templateFgdbParentDirectory, FgdbParentDirectory, FgdbSafeName);
            }
            else
            {
                //检查MapDataFGDB是否处于初始状态
                EsriMapDataGenerator.FGDBHelper.CheckMapDataFGDB_Initial(newFgdbPath);
            }
        }








        /// <summary>
        /// 应用图层映射配置
        /// </summary>
        private void applyLayerMapping()
        {

            //List<string> layerNames_NullOrEmptyLayerNames = new List<string>();
            //Type myType = Drawing.Configuration.LayerMapping.GetType();
            //System.Reflection.PropertyInfo[] propertyInfos = myType.GetProperties();


            //foreach (var pi in propertyInfos)
            //{
            //    var item = pi.GetValue(pi.Name);
            //    List<string> ls = item as List<string>;

            //    if (ls ==null||ls.Count==0)
            //    {
            //        layerNames_NullOrEmptyLayerNames.Add(pi.Name);
            //    }
            //}




            //应用图层映射,并检查是否为每个图层勾选了对应图层
            foreach (string item in Enum.GetNames(typeof(LayerType)))
            {
                LayerType layType = (LayerType)Enum.Parse(typeof(LayerType), item, false);



                //if (layType == LayerType.Balcony)
                //    continue;

                setOneKindLayerMapping(layType);
            }
        }



        /// <summary>
        /// 设置单个图层映射（仅供applyLayerMapping调用）
        /// </summary>
        /// <param name="layerType"></param>
        private void setOneKindLayerMapping(LayerType layerType)
        {
            ObservableCollection<BoolString> layerNames = null;
            List<string> oneTypeLayerNames = null;

            switch (layerType)
            {
                case LayerType.Wall:
                    layerNames = layerNamesForBind.Wall_LayerNames;
                    oneTypeLayerNames = fpConfig.LayerMapping.Wall;
                    break;
                case LayerType.Balcony:
                    layerNames = layerNamesForBind.Balcony_LayerNames;
                    oneTypeLayerNames = fpConfig.LayerMapping.Balcony;
                    break;
                case LayerType.Door:
                    layerNames = layerNamesForBind.Door_LayerNames;
                    oneTypeLayerNames = fpConfig.LayerMapping.Door;
                    break;
                case LayerType.Window:
                    layerNames = layerNamesForBind.Window_LayerNames;
                    oneTypeLayerNames = fpConfig.LayerMapping.Window;
                    break;
                case LayerType.Stair:
                    layerNames = layerNamesForBind.Stair_LayerNames;
                    oneTypeLayerNames = fpConfig.LayerMapping.Stairs;
                    break;
                case LayerType.Elevator:
                    layerNames = layerNamesForBind.Elevator_LayerNames;
                    oneTypeLayerNames = fpConfig.LayerMapping.Elevator;
                    break;
                case LayerType.Label:
                    layerNames = layerNamesForBind.Label_LayerNames;
                    oneTypeLayerNames = fpConfig.LayerMapping.Text;
                    break;
            }

            if (oneTypeLayerNames.Count > 0)
                oneTypeLayerNames.Clear();

            foreach (var item in layerNames)
            {
                if (item.IsChecked == true)
                    oneTypeLayerNames.Add(item.LayerName);
            }

            //跳过非必需图层的后续检查
            if (layerType == LayerType.Balcony ||
                layerType == LayerType.Stair ||
                layerType == LayerType.Elevator)
                return;
            else
            {
                if (oneTypeLayerNames.Count == 0)
                    throw new InvalidDataException(string.Format("{0} 未选择图层！", layerType.ToString()));
            }
        }

        /// <summary>
        /// 筛选并排序图层名
        /// </summary>
        /// <param name="layerType"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        private ObservableCollection<BoolString> filterAndOrderLayerNames(LayerType layerType, ref List<string> layerNames)
        {
            ObservableCollection<BoolString> result = null;
            List<String> filterLayers = null;
            IOrderedEnumerable<string> orderLayerNames = null;
            switch (layerType)
            {


                case LayerType.Wall:
                    //按常用图层名初步筛选可能的图层集
                    filterLayers = layerNames.FindAll(new Predicate<string>((p) =>
                    {
                        if (filterKeyword(p, "墙") || filterKeyword(p, "WALL") || p.StartsWith("Q", true, null))
                            return true;
                        return false;
                    }));
                    //对图层名进行排序
                    orderLayerNames = filterLayers.OrderBy(String => String);
                    break;

                case LayerType.Balcony:
                    filterLayers = layerNames.FindAll(new Predicate<string>((p) =>
                    {
                        if (filterKeyword(p, "阳台") || filterKeyword(p, "BALC") || filterKeyword(p, "YANGTAI"))
                            return true;
                        return false;
                    }));
                    orderLayerNames = filterLayers.OrderBy(String => String);
                    break;

                case LayerType.Door:
                    filterLayers = layerNames.FindAll(new Predicate<string>((p) =>
                    {
                        if (filterKeyword(p, "门") || p.StartsWith("D", true, null) || p.StartsWith("M", true, null))
                            return true;
                        return false;
                    }));
                    orderLayerNames = filterLayers.OrderBy(String => String);
                    break;

                case LayerType.Window:
                    filterLayers = layerNames.FindAll(new Predicate<string>((p) =>
                    {
                        if (filterKeyword(p, "窗") || p.StartsWith("W", true, null) || p.StartsWith("C", true, null))
                            return true;
                        return false;
                    }));
                    orderLayerNames = filterLayers.OrderBy(String => String);
                    break;

                case LayerType.Stair:
                    filterLayers = layerNames.FindAll(new Predicate<string>((p) =>
                    {
                        if (filterKeyword(p, "楼梯") || filterKeyword(p, "STAIR") || filterKeyword(p, "LOUTI"))
                            return true;
                        return false;
                    }));
                    orderLayerNames = filterLayers.OrderBy(String => String);
                    break;

                case LayerType.Elevator:
                    filterLayers = layerNames.FindAll(new Predicate<string>((p) =>
                    {
                        if (filterKeyword(p, "电梯") || filterKeyword(p, "ELEVATOR") || filterKeyword(p, "DIANTI"))
                            return true;
                        return false;
                    }));
                    orderLayerNames = filterLayers.OrderBy(String => String);
                    break;

                case LayerType.Label:
                    filterLayers = layerNames.FindAll(new Predicate<string>((p) =>
                    {
                        if (filterKeyword(p, "标注") || filterKeyword(p, "注记") || filterKeyword(p, "TEXT") || filterKeyword(p, "TXT"))
                            return true;
                        return false;
                    }));
                    orderLayerNames = filterLayers.OrderBy(String => String);
                    break;
            }

            //填充可供绑定的图层名集合
            result = new ObservableCollection<BoolString>();
            IEnumerator enumerator = orderLayerNames.GetEnumerator();
            while (enumerator.MoveNext())
            {
                result.Add(new BoolString(enumerator.Current as string));
            }
            return result;
        }


        /// <summary>
        /// 按关键字筛选字符串(不区分大小写)
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private bool filterKeyword(string sourceString, string keyword)
        {
            return sourceString.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }


        private void applyOneLayerMappingFromFile(ref List<string> oneKindLayers, ref   ObservableCollection<BoolString> oneKindLayersForBind)
        {
            int index = -1;
            foreach (var item in oneKindLayers)
            {
                index = oneKindLayersForBind.IndexOf(new BoolString(item.Trim(), false));
                if (index >= 0)
                    oneKindLayersForBind[index].IsChecked = true;
            }

        }
        #endregion
    }

    /// <summary>
    /// 将图层信息以bool?+string形式绑定到ListItem
    /// </summary>
    public class BoolString : BindableObject
    {
        bool? isChecked;

        public bool? IsChecked
        {
            get { return isChecked; }
            set { SetProperty<bool?>(ref isChecked, value); }
        }

        public string LayerName { get; set; }


        public BoolString(string layerName, bool? isChecked = false)
        {
            IsChecked = isChecked;
            LayerName = layerName;
        }

        public override bool Equals(object obj)
        {
            BoolString bs = obj as BoolString;

            if (bs.IsChecked == this.IsChecked && bs.LayerName == this.LayerName)
                return true;
            return false;
        }
    }
}
