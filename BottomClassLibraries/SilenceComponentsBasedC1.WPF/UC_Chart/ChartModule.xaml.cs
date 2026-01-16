using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using C1.WPF.C1Chart;
using Microsoft.Win32;

using System.ComponentModel;

namespace SilenceComponentsBasedC1.WPF.UC_Chart
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class ChartModule : UserControl
    {
        #region 注册依赖项属性
        public static readonly DependencyProperty RightTopControlsVisibilityPropeprty = DependencyProperty.Register("RightTopControlsVisibility", typeof(Visibility), typeof(ChartModule));
        public static readonly DependencyProperty Legend_OrientationPropeprty = DependencyProperty.Register("Legend_Orientation", typeof(Orientation), typeof(ChartModule));
        public static readonly DependencyProperty Legend_PositionProperty = DependencyProperty.Register("Legend_Position", typeof(LegendPosition), typeof(ChartModule));

        #endregion

        #region 属性
        /// <summary>
        /// 右上角打印、图片导出、关闭按钮的可见性
        /// </summary>
        public Visibility RightTopControlsVisibility
        {
            get { return (Visibility)this.GetValue(RightTopControlsVisibilityPropeprty); }

            set { this.SetValue(RightTopControlsVisibilityPropeprty, value); }
        }

        /// <summary>
        /// 图例方向
        /// 默认值：Vertical
        /// </summary>
        [DefaultValue(typeof(Orientation), "Vertical")]
        public Orientation Legend_Orientation
        {
            get { return (Orientation)this.GetValue(Legend_OrientationPropeprty); }

            set { this.SetValue(Legend_OrientationPropeprty, value); }
        }

        /// <summary>
        /// 图例位置
        /// 默认值：Right
        /// </summary>
        [DefaultValue(typeof(LegendPosition), "Right")]
        public LegendPosition Legend_Position
        {
            get { return (LegendPosition)this.GetValue(Legend_PositionProperty); }

            set { this.SetValue(Legend_PositionProperty, value); }
        }
        #endregion





        public ChartModule()
        {
            InitializeComponent();

            #region 事件注册
            cboChartTypes.SelectionChanged += cboChartTypes_SelectionChanged;
            cboPalatte.SelectionChanged += cboPalatte_SelectionChanged;

            rbtShowYValue.Checked += rbtShowYValue_Checked;
            rbtHideYValue.Checked += rbtHideYValue_Checked;

            hbtnExportPicture.Click += hbtnExportPicture_Click;
            hbtnPrintPicture.Click += hbtnPrintPicture_Click;
            #endregion

            #region 默认设置
            //默认柱状图
            cboChartTypes.SelectedIndex = 0;

            //默认标准配色
            cboPalatte.SelectedIndex = 1;

            //默认显示图中Y值
            rbtShowYValue.IsChecked = true;
            #endregion
        }



        #region 事件
        //图表类型切换
        void cboChartTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboChartTypes.SelectedIndex > -1)
            {
                #region 图表类型调整
                //当Chart层面的ChartType不为null值时，所有DataSeries层面的ChartType必须与之相同，否则因冲突而不显示
                //换言之，只有当Chart层面的ChartType为null值时，才支持DataSeries有不同的ChartType
                ChartType chartType = (ChartType)(Enum.Parse(typeof(ChartType), (cboChartTypes.SelectedItem as ComboBoxItem).Tag.ToString()));
                chart.ChartType = chartType;
                for (int i = 0; i < chart.Data.Children.Count; i++)
                {
                    chart.Data.Children[i].ChartType = chartType;
                }
                #endregion


                #region 样式及模板
                for (int i = 0; i < chart.Data.Children.Count; i++)
                {
                    set_StyleAndTemplateForDataSeries(chart.Data.Children[i]);
                }
                #endregion
            }
        }

        //调色板颜色切换
        void cboPalatte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chart.Palette = (ColorGeneration)cboPalatte.SelectedValue;
        }

        //显示图中Y值
        void rbtShowYValue_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in chart.Data.Children)
            {
                //根据图表类型向数据点应用不同标签
                if (item.ChartType == ChartType.Pie || item.ChartType == ChartType.Pie3DExploded)
                {
                    //如果是饼图系列，则采用百分比形式的点标签
                    item.PointLabelTemplate = TryFindResource("percentage_pointLableTemplate") as DataTemplate;
                }
                else
                {
                    //饼图系列之外的则采用正常形式的点标签
                    item.PointLabelTemplate = TryFindResource("pointLableTemplate") as DataTemplate;
                }
            }
        }

        //隐藏图中Y值
        void rbtHideYValue_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in chart.Data.Children)
            {
                item.PointLabelTemplate = TryFindResource("pointLableTemplate_Hide") as DataTemplate;
            }
        }


        //图表导出(jpg/png)
        void hbtnExportPicture_Click(object sender, RoutedEventArgs e)
        {
            hbtnExportPicture.IsEnabled = false;

            SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = "(*.JPEG)|*.jpg|(*.PNG)|*.png";
            sfd.Filter = "(*.PNG)|*.png";

            if (sfd.ShowDialog() == true)
            {
                using (var stream = sfd.OpenFile())
                {
                    //switch (sfd.FilterIndex)
                    //{
                    //    //保存为jpg格式
                    //    case 1:
                    //        RenderTargetBitmap bm1 = new RenderTargetBitmap((int)gridChartArea.ActualWidth, (int)gridChartArea.ActualHeight, 96d, 96d, PixelFormats.Default);
                    //        bm1.Render(chart);
                    //        JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
                    //        jpgEncoder.Frames.Add(BitmapFrame.Create(bm1));
                    //        jpgEncoder.Save(stream);
                    //        break;

                    //保存为png格式
                    //case 2:
                    RenderTargetBitmap bm2 = new RenderTargetBitmap((int)gridChartArea.ActualWidth, (int)gridChartArea.ActualHeight, 96d, 96d, PixelFormats.Default);
                    bm2.Render(chart);
                    PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(bm2));
                    pngEncoder.Save(stream);
                    //break;
                    //}
                }

                MessageBox.Show("导出成功！", "消息", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            hbtnExportPicture.IsEnabled = true;
        }

        //图片打印
        void hbtnPrintPicture_Click(object sender, RoutedEventArgs e)
        {
            hbtnPrintPicture.IsEnabled = false;
            PrintDialog pdg = new PrintDialog();

            if (pdg.ShowDialog() == true)
            {
                pdg.PrintVisual(chart, "chart");
            }
            hbtnPrintPicture.IsEnabled = true;
        }
        #endregion


        #region 公开方法

        #region 绘图
        public void DrawChart_BaseSameX(string chartTitle, List<string> X_sharedNames, List<DataSeries> yDataSeries)
        {
            chart.BeginUpdate();
            chart.Data.Children.Clear();

            tbkChartTitle.Text = chartTitle;

            //X绑定(X是共享的)
            chart.Data.ItemNames = X_sharedNames;
            for (int i = 0; i < yDataSeries.Count(); i++)
            {
                chart.Data.Children.Add(yDataSeries[i]);
            }

            //检查图表类型唯一性
            if (check_IsUniqueChartType())
            {
                cboChartTypes.IsEnabled = true;

                int targetSelectedIndex = get_MatchedCboChartTypeSelectIndex(yDataSeries[0].ChartType);

                if (targetSelectedIndex != cboChartTypes.SelectedIndex)
                    cboChartTypes.SelectedIndex = targetSelectedIndex;
                else
                    for (int i = 0; i < chart.Data.Children.Count; i++)
                    {
                        set_StyleAndTemplateForDataSeries(chart.Data.Children[i]);
                    }
            }
            else
            {
                cboChartTypes.SelectedIndex = -1;
                cboChartTypes.IsEnabled = false;

                for (int i = 0; i < chart.Data.Children.Count; i++)
                {
                    set_StyleAndTemplateForDataSeries(chart.Data.Children[i]);
                }
            }

            chart.EndUpdate();
        }

        public void DrawChart(string chartTitle, List<XYDataSeries> xyDataSeries)
        {
            chart.BeginUpdate();
            chart.Data.Children.Clear();

            tbkChartTitle.Text = chartTitle;
            for (int i = 0; i < xyDataSeries.Count(); i++)
            {
                chart.Data.Children.Add(xyDataSeries[i]);
            }


            //检查图表类型唯一性
            if (check_IsUniqueChartType())
            {
                cboChartTypes.IsEnabled = true;
                cboChartTypes.SelectedIndex = get_MatchedCboChartTypeSelectIndex(xyDataSeries[0].ChartType);
            }
            else
            {
                cboChartTypes.SelectedIndex = -1;
                cboChartTypes.IsEnabled = false;

                for (int i = 0; i < chart.Data.Children.Count; i++)
                {
                    set_StyleAndTemplateForDataSeries(chart.Data.Children[i]);
                }
            }

            chart.EndUpdate();
        }
        #endregion


        #region 构造DataSeries
        /// <summary>
        /// 创建基于相同的X的DataSerirs(X在DrawChart_BaseSameX方法里指定)
        /// </summary>
        /// <param name="chartType"></param>
        /// <param name="dataTable"></param>
        /// <param name="y_ColumnName"></param>
        /// <returns></returns>
        public DataSeries Create_DataSeriesBaseSameX(ChartType? chartType, DataTable dataTable, int y_ColumnIndex = 1)
        {
            DataSeries dataSeries = new DataSeries();
            //设置DataSerise图表类型
            dataSeries.ChartType = chartType;

            //Y绑定
            dataSeries.ValuesSource = (from dr in dataTable.AsEnumerable() select double.Parse(dr[y_ColumnIndex].ToString())).ToArray();

            return dataSeries;
        }

        /// <summary>
        /// 创建基于相同的X的DataSerirs(X在DrawChart_BaseSameX方法里指定)
        /// </summary>
        /// <param name="chartType"></param>
        /// <param name="dataTable"></param>
        /// <param name="y_ColumnName"></param>
        /// <returns></returns>
        public DataSeries Create_DataSeriesBaseSameX(ChartType? chartType, DataTable dataTable, string y_ColumnName)
        {
            DataSeries dataSeries = new DataSeries();
            //设置DataSerise图表类型
            dataSeries.ChartType = chartType;

            //Y绑定
            dataSeries.ValuesSource = (from dr in dataTable.AsEnumerable() select double.Parse(dr[y_ColumnName].ToString())).ToArray();

            return dataSeries;
        }



        /// <summary>
        /// 创建XYDataSeries
        /// </summary>
        /// <param name="chartType"></param>
        /// <param name="dataTable">从中X、Y值集合的DataTable</param>
        /// <param name="x_ColumnIndex"></param>
        /// <param name="y_ColumnIndex"></param>
        /// <returns></returns>
        public XYDataSeries Create_DataSeries(ChartType? chartType, DataTable dataTable, int x_ColumnIndex = 0, int y_ColumnIndex = 1)
        {
            XYDataSeries xyDataSeries = new XYDataSeries();
            //设置DataSerise图表类型
            xyDataSeries.ChartType = chartType;

            //Y绑定
            xyDataSeries.XValuesSource = (from dr in dataTable.AsEnumerable() select dr[x_ColumnIndex].ToString()).ToArray();
            xyDataSeries.ValuesSource = (from dr in dataTable.AsEnumerable() select double.Parse(dr[y_ColumnIndex].ToString())).ToArray();

            return xyDataSeries;

        }

        /// <summary>
        /// 创建XYDataSeries
        /// </summary>
        /// <param name="chartType"></param>
        /// <param name="dataTable">从中X、Y值集合的DataTable</param>
        /// <param name="x_ColumnName"></param>
        /// <param name="y_ColumnName"></param>
        /// <returns></returns>
        public XYDataSeries Create_DataSeries(ChartType? chartType, DataTable dataTable, string x_ColumnName, string y_ColumnName)
        {
            XYDataSeries xyDataSeries = new XYDataSeries();

            //设置DataSerise图表类型
            xyDataSeries.ChartType = chartType;

            //Y绑定
            xyDataSeries.XValuesSource = (from dr in dataTable.AsEnumerable() select dr[x_ColumnName].ToString()).ToArray();
            xyDataSeries.ValuesSource = (from dr in dataTable.AsEnumerable() select double.Parse(dr[y_ColumnName].ToString())).ToArray();

            return xyDataSeries;
        }
        #endregion

        #endregion

        #region 内部方法
        /// <summary>
        /// 根据DataSeries的ChartType设置DataSeries的样式及模板
        /// </summary>
        /// <param name="dataSeries"></param>
        private void set_StyleAndTemplateForDataSeries(DataSeries dataSeries)
        {
            if (dataSeries.ChartType == null)
                dataSeries.ChartType = ChartType.Column;

            #region SymbolStyle
            string styleName = "symbolStyle";
            if (dataSeries.ChartType == ChartType.Ribbon || dataSeries.ChartType.ToString().Contains("3D"))
            {

                styleName = "symbolStyle3D";
                chart.Actions.Add(new Rotate3DAction());
            }
            dataSeries.SymbolStyle = FindResource(styleName) as Style;
            dataSeries.ConnectionStyle = FindResource(styleName) as Style;
            #endregion


            #region PointLabelTemplate
            if ((bool)rbtShowYValue.IsChecked)
            {
                //根据图表类型向数据点应用不同标签
                if (dataSeries.ChartType == ChartType.Pie || dataSeries.ChartType == ChartType.Pie3DExploded)
                {
                    //如果是饼图系列，则采用百分比形式的PointLabelTemplate and ToopTipTemplate
                    dataSeries.PointLabelTemplate = TryFindResource("percentage_pointLableTemplate") as DataTemplate;
                    dataSeries.PointTooltipTemplate = TryFindResource("pointToolTipTemplate_Percentage") as DataTemplate;
                }
                else
                {
                    //饼图系列之外的则采用正常形式的PointLabelTemplate and ToopTipTemplate
                    dataSeries.PointLabelTemplate = TryFindResource("pointLableTemplate") as DataTemplate;
                    dataSeries.PointTooltipTemplate = FindResource("pointToolTipTemplate") as DataTemplate;
                }
            }
            else
            {
                //不在图中显示Y值
                dataSeries.PointLabelTemplate = TryFindResource("pointLableTemplate_Hide") as DataTemplate;
            }
            #endregion



        }

        /// <summary>
        /// 判断Chart的图表类型是否唯一
        /// </summary>
        /// <returns></returns>
        private bool check_IsUniqueChartType()
        {
            if (chart.Data.Children.Count == 1 || chart.Data.Children.Count == 0)
                return true;

            if (chart.Data.Children.Count > 1)
            {
                ChartType? firstChartType = chart.Data.Children[0].ChartType;
                for (int i = 1; i < chart.Data.Children.Count; i++)
                {
                    if (chart.Data.Children[i].ChartType != firstChartType)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取CboChartTypes中SelectedValue值与传入参数相同的项的Index
        /// </summary>
        /// <param name="chartType"></param>
        /// <returns></returns>
        private int get_MatchedCboChartTypeSelectIndex(ChartType? chartType = null)
        {
            //如果未指定类型，就设为柱状图索引： 0
            if (chartType == null)
                return 0;

            for (int i = 0; i < cboChartTypes.Items.Count; i++)
            {
                ChartType cty = (ChartType)(Enum.Parse(typeof(ChartType), (cboChartTypes.Items[i] as ComboBoxItem).Tag.ToString()));
                if (chartType == cty)
                    return i;
            }
            return -1;
        }
        #endregion
    }
}
