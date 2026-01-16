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
using BCE.DataModels;
using BCE.DataModels.Basic;
using BCE;
using DXFReader.DataModel;

namespace MainProgram.Pages
{
    /// <summary>
    /// Page_DrawingParsing.xaml 的交互逻辑
    /// </summary>
    public partial class Page_DrawingParsing : Page, IPageInterfaces
    {
        #region 内部字段
        //引用的外部变量
        MainWindow homeWindow = null;
        TabControl tabMainMenu = null;
        #endregion

        #region 公开字段
        public static int PageIndex = 3;//页索引
        public bool HasParsedDrawing = false;//标识是否已经解析图纸
        #endregion

        public Page_DrawingParsing(MainWindow homeWindow)
        {
            InitializeComponent();

            //填充外部变量
            this.homeWindow = homeWindow;
            this.tabMainMenu = homeWindow.tabMainMenu;


            //事件订阅
            btnLast.Click += btnLast_Click;
            btnParsingDrawing.Click += btnParsingDrawing_Click;
            btnNext.Click += btnNext_Click;
            dgFuncRegion.LoadingRow += generateRowIndex;

            this.Loaded += Page_DrawingParsing_Loaded;
        }


        #region 事件
        void Page_DrawingParsing_Loaded(object sender, RoutedEventArgs e)
        {
            btnParsingDrawing.IsEnabled = homeWindow.HasConfigFloorPlan;
            btnNext.IsEnabled = HasParsedDrawing;
        }

        //自动添加行号
        void generateRowIndex(object sender, DataGridRowEventArgs e)
        {
            //(sender as DataGridRow).Header = e.Row.GetIndex() + 1;
        }




        void btnLast_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex - 1);
        }

        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex + 1);
        }

        void btnParsingDrawing_Click(object sender, RoutedEventArgs e)
        {
            if (HasParsedDrawing)
                homeWindow.page_floorPlanConfig.ResetFloorPlan();

            ExtructError extructError = BuildingComponentModeler.ParseDrawing(homeWindow.Drawing, homeWindow.DxfReader);
            HasParsedDrawing = true;


            update_dgRings();
            updateResult(extructError);

            if (extructError == ExtructError.None)
            {
                updatePolygon();
                updateDoorAndWindow();
                updateStairAndElevator();

                //激活相关控件
                ActivateControls();
                homeWindow.RefreshStatusMessage("图纸解析已完成！");
                MessageBox.Show("图纸解析成功！", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(string.Format("图纸解析失败: {0}", extructError.ToString()), "", MessageBoxButton.OK, MessageBoxImage.Error);

                //重置部分图表
                //1.多边形
                dgFuncRegion.ItemsSource = null;
                chart_Polygon.chart.Data.Children.Clear();

                //2.门窗
                dgDoor.ItemsSource = null;
                dgWindow.ItemsSource = null;
                chart_Door.chart.Data.Children.Clear();

                //3.楼梯电梯
                dgStairs.ItemsSource = null;
                dgElevator.ItemsSource = null;

                //弹出反馈窗口以查看提取结果
                //Feedback.GraphicsWindow grapgicsWindow = 
                //    new Feedback.GraphicsWindow(homeWindow.Drawing.PResult,homeWindow.Drawing.Configuration.DXF_Path);
                //grapgicsWindow.Show();
            }


            Feedback.GraphicsWindow grapgicsWindow =
                new Feedback.GraphicsWindow(homeWindow.Drawing.PResult, homeWindow.Drawing.Configuration.DXF_Path);
            grapgicsWindow.Show();
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// 更新评估结果
        /// </summary>
        /// <param name="extructError"></param>
        void updateResult(ExtructError extructError)
        {
            tbkRings.Text = homeWindow.Drawing.PResult.SPolygons.Count.ToString();
            tbkImcopRings.Text = homeWindow.Drawing.PResult.ImSPolygons.Count.ToString();
            tbkIsolatedLines.Text = homeWindow.Drawing.PResult.IsolateLines.Count.ToString();


            if (extructError == ExtructError.None)
            {
                tbkResultNotice.Text = "通 过！";
                tbkResultNotice.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                tbkResultNotice.Text = "不通过！";
                tbkResultNotice.Foreground = new SolidColorBrush(Colors.Red);
            }
        }


        /// <summary>
        /// 更新闭合环表格
        /// </summary>
        void update_dgRings()
        {
            Result result = homeWindow.Drawing.PResult;
            ObservableCollection<RingsEvaluationForBind> res = new ObservableCollection<RingsEvaluationForBind>();

            res.Add(new RingsEvaluationForBind(result, PolygonType.Wall, LineType.WallLine));
            res.Add(new RingsEvaluationForBind(result, PolygonType.Balustrade, LineType.BalconyLine));
            res.Add(new RingsEvaluationForBind(result, PolygonType.FunctionRegion, LineType.Any));

            dgRing.ItemsSource = null;
            dgRing.ItemsSource = res;

        }

        /// <summary>
        /// 更新功能区统计图表
        /// </summary>
        void updatePolygon()
        {
            Result result = homeWindow.Drawing.PResult;


            //表(基于毫米，显示成米)
            var polygons = (from p in result.SPolygons
                            where p.PType!=PolygonType.Wall && p.PType!=PolygonType.Balustrade
                            select new
                            {
                                PType = p.PType,
                                NumOfDoor = p.Lines.Where<LineClass>((l) => { if (l.LType == LineType.DoorLine) return true; return false; }).Count<LineClass>(),
                                NumOfWindow = p.Lines.Where<LineClass>((l) => { if (l.LType == LineType.WindowLine) return true; return false; }).Count<LineClass>(),
                                Area = p.Area * Math.Pow(10, -6),
                                Circumference = p.Circumference*Math.Pow(10,-3)
                            }).ToList();

            dgFuncRegion.ItemsSource = null;
            dgFuncRegion.ItemsSource = polygons;


            //图
            var xyValues = (from p in result.SPolygons
                            where p.PType != PolygonType.Wall && p.PType != PolygonType.Balustrade
                            group p by p.PType into g
                            select new { PType = g.Key, Count = g.Count() }).ToList();

            C1.WPF.C1Chart.DataSeries dataSeries = new C1.WPF.C1Chart.DataSeries();
            List<string> xLabels = (from s in xyValues
                                    select s.PType.ToString()).ToList<string>();
            dataSeries.ValuesSource = (from s in xyValues
                                       select s.Count).ToList<int>();

            //dataSeries.AxisX = "类型";
            //dataSeries.AxisY = "数量";
            chart_Polygon.DrawChart_BaseSameX("语义多边形统计", xLabels, new List<C1.WPF.C1Chart.DataSeries>() { dataSeries });
        }

        /// <summary>
        /// 更新门窗图表
        /// </summary>
        void updateDoorAndWindow()
        {
            Result result = homeWindow.Drawing.PResult;

            //表
            dgDoor.ItemsSource = null;
            dgDoor.ItemsSource = (from d in result.Doors
                                  orderby d.Type
                                  select d).AsEnumerable();
            dgWindow.ItemsSource = null;
            dgWindow.ItemsSource = (from w in result.Windows
                                    orderby w.Type
                                    select w).AsEnumerable();

            //图
            //1. 门
            C1.WPF.C1Chart.DataSeries dataSeries_door = new C1.WPF.C1Chart.DataSeries();
            var xyValues_door = (from d in result.Doors
                                 group d by d.Type into g
                                 select new { PType = g.Key, Count = g.Count() }).ToList();

            List<string> xLabels_door = (from d in xyValues_door
                                         select d.PType.ToString()).ToList<string>();

            dataSeries_door.ValuesSource = (from d in xyValues_door
                                            select d.Count).ToList<int>();

            chart_Door.DrawChart_BaseSameX("门类型统计", xLabels_door, new List<C1.WPF.C1Chart.DataSeries>() { dataSeries_door });
        }

        /// <summary>
        /// 更新楼梯 + 电梯表格
        /// </summary>
        void updateStairAndElevator()
        {
            Result result = homeWindow.Drawing.PResult;

            //表

            //1. 楼梯
            dgStairs.ItemsSource = null;
            dgStairs.ItemsSource = result.Stairs;

            //dgStairs.ItemsSource = (from s in result.Stairs
            //                        select new
            //                        {
            //                            StepNum=s.StepNum.ToString(),
            //                            Width_Step=s.Width_Step.ToString("N4"),
            //                            Height_Step=s.Height_Step.ToString("N4"),
            //                            Width_Land = s.Width_Land.ToString("N4"),
            //                            Width_Staircase = s.Width_Staircase.ToString("N4"),
            //                            Width_Stairway = s.Width_Stairway.ToString("N4"),
            //                            Width_Stairwell = s.Width_Stairwell.ToString("N4"),
            //                            UpstairPosition = s.UpstairPosition,
            //                            InsertPoint = s.InsertPoint,
            //                            RotateAngle = s.RotateAngle.ToString("N4"),
            //                        }
            //                      ).AsEnumerable();

            //2. 电梯
            dgElevator.ItemsSource = null;
            dgElevator.ItemsSource = result.Elevators;

        }
        #endregion

        #region 接口
        public void ActivateControls()
        {
            btnNext.IsEnabled = true;

            //同时激活2/3D地图页标签
            (tabMainMenu.Items[Page_Map2D.PageIndex] as TabItem).IsEnabled = true;
            (tabMainMenu.Items[Page_Map3D.PageIndex] as TabItem).IsEnabled = true;
        }

        #endregion
    }
}
