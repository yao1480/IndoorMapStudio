using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ArcObjectsUtilities;
using BCE.DataModels;
using DXFReader.DataModel;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Animation;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using EsriMapDataGenerator;
using EsriMapDataGenerator.GeometryCreator;
using MainProgram.Helpers;

namespace MainProgram.Assets.Controls
{
    /// <summary>
    /// NavigationController.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationController : System.Windows.Window
    {
        FloorPlan m_hookedDrawing = null;
        AxSceneControl m_hookedSceneControl;
        PathAnimationHelper m_PathAnimationHelper;
        IPolyline m_originalNavigatePath = null;//未经移动的导航路径
        //IPolyline m_navigatePath = null;//导航路径
        RouteAnalysisHelper m_routeAnalysisHelper;
        string m_routeAnalysisLayerName = Resource.RouteAnalysisGroupLayerName;
        string m_stopsLayerName = Resource.StopsLayerName;
        string m_routeLayerName = Resource.ResultRouteLayerName;
        IGraphicsContainer3D m_stopsLayer = null;//停靠点图层
        IGraphicsContainer3D m_routeLayer = null;//路径结果图层
        bool m_shouldReCreateAnimation = true;
        const double walkSpeed = 1555.6 / 2;//正常步行速度为1555.6(mm/s)

        double playDuration
        {
            get
            {
                double a;
                if (double.TryParse(tbxPlayDuration.Text.Trim(), out a))
                    return a;
                return -1;
            }
        }//动画播放时长

        double cameraHeightToFloor
        {
            get
            {
                double a;
                if (double.TryParse(tbxCameraHeightToFloor.Text.Trim(), out a))
                    return 1000 * a;//m转换为mm
                return -1;
            }
        }//相机距离楼板高度


        public NavigationController(FloorPlan drawing, AxSceneControl sceneControl, RouteAnalysisHelper pRouteAnalysisHelper)
        {
            InitializeComponent();

            m_hookedDrawing = drawing;
            m_hookedSceneControl = sceneControl;
            m_PathAnimationHelper = new PathAnimationHelper(sceneControl.Object as ISceneControl);
            m_routeAnalysisHelper = pRouteAnalysisHelper;

            #region 控件参数初始化
            //楼层号设置
            for (int i = 0; i < m_hookedDrawing.Configuration.RepeatCount; i++)
            {
                cboFromFloorIndex.Items.Add((i + 1).ToString());
                cboToFloorIndex.Items.Add((i + 1).ToString());
            }

            //功能区绑定源设置
            List<SemanticPolygon> areaSPS = (from p in m_hookedDrawing.PResult.SPolygons
                                             where p.PType != PolygonType.Floor &&
                                             p.PType != PolygonType.Wall &&
                                             p.PType != PolygonType.Balustrade
                                             select p).ToList<SemanticPolygon>();

            cboFromArea.DisplayMemberPath = "PType";
            cboFromArea.ItemsSource = areaSPS;

            cboToArea.DisplayMemberPath = "PType";
            cboToArea.ItemsSource = areaSPS;

            //播放模式源设置，默认播放模式设为“正向播放一次”
            List<TowMemberContainer> playModes = new List<TowMemberContainer>()
            {
                new  TowMemberContainer("正向播放一次", esriAnimationPlayMode.esriAnimationPlayOnceForward),
                new  TowMemberContainer("反向播放一次", esriAnimationPlayMode.esriAnimationPlayOnceReverse),
                new  TowMemberContainer("正向循环", esriAnimationPlayMode.esriAnimationPlayLoopForward),
                new  TowMemberContainer("反向循环", esriAnimationPlayMode.esriAnimationPlayLoopReverse)

            };

            cboPlayMode.DisplayMemberPath = "Key";
            cboPlayMode.SelectedValuePath = "Value";
            cboPlayMode.ItemsSource = playModes;
            cboPlayMode.SelectedIndex = 0;

            //设置相机默认距离地面高度为1.7m
            tbxCameraHeightToFloor.Text = "1.7";
            #endregion

            #region 事件订阅
            cboFromFloorIndex.SelectionChanged += symbolizeSelectedArea;
            cboToFloorIndex.SelectionChanged += symbolizeSelectedArea;
            cboFromArea.SelectionChanged += symbolizeSelectedArea;
            cboToArea.SelectionChanged += symbolizeSelectedArea;
            tbxCameraHeightToFloor.TextChanged += tbxCameraHeightToFloor_TextChanged;
            tbxCameraHeightToFloor.PreviewTextInput += tbxCameraHeightToFloor_PreviewTextInput;
            tbxPlayDuration.PreviewTextInput += tbxPlayDuration_PreviewTextInput;
            tbxPlayDuration.TextChanged += tbxPlayDuration_TextChanged;
            rbtThroughByElevator.Checked += wayOfThroughFloorChanged;
            rbtThroughByStair.Checked += wayOfThroughFloorChanged;

            btnCalcPath.Click += btnCalcPath_Click;
            btnPlay.Click += btnPlay_Click;
            btnPause.Click += btnPause_Click;
            btnStop.Click += btnStop_Click;
            btnExportVideo.Click += btnExportVideo_Click;

            #endregion
        }





        #region 事件
        //及时对选中的功能区进行突出渲染
        void symbolizeSelectedArea(object sender, SelectionChangedEventArgs e)
        {
            //按钮可用性设置
            btnPlay.IsEnabled = false;
            btnExportVideo.IsEnabled = false;

            if (!canRefreshSelectedFuctionArea())
            {
                btnCalcPath.IsEnabled = false;
                return;
            }
            else
            {
                double cameraHeight;
                bool isValidCamaraHeight = double.TryParse(tbxCameraHeightToFloor.Text.Trim(), out cameraHeight);
                if (isValidCamaraHeight)
                    btnCalcPath.IsEnabled = true;
                else
                    btnCalcPath.IsEnabled = false;
            }

            //图层检查
            checkAnalysisGroupLayer();


            #region 渲染起始+目的地功能区
            //更新出发地+目的地功能区

            int floorIndex = -1;
            double startHeight = 0d;
            double extrudeHeight = m_hookedDrawing.Configuration.Floor_Headroom;
            SemanticPolygon sp = null;
            IVector3D pExtrudeVector = null;
            IGeometry sectionGeometry = null;
            IGeometry area3D = null;
            IElement area3DElement = null;
            IColor renderColor = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 0, 255);//3D出发（目标)功能区的颜色


            //创建3D起始功能区元素
            floorIndex = int.Parse(cboFromFloorIndex.SelectedValue.ToString());//楼层号从 1 开始
            startHeight = m_hookedDrawing.Configuration.Floor_Thickness
               + (floorIndex - 1) * (m_hookedDrawing.Configuration.Floor_Thickness + m_hookedDrawing.Configuration.Floor_Headroom);

            pExtrudeVector = GeometryUtility.Construct_Vector3D(0, 0, extrudeHeight);

            sp = (SemanticPolygon)cboFromArea.SelectedValue;
            sectionGeometry = ConverterForEsri.Create_Polygon_FromPoints(sp.Points, startHeight);

            area3D = GeometryUtility.Construct_StretchedBody(sectionGeometry, pExtrudeVector);
            area3DElement = ElementUtility.Contruct_MultiPatchElement(area3D, renderColor);
            m_stopsLayer.AddElement(area3DElement);

            //创建3D目标功能区元素
            floorIndex = int.Parse(cboToFloorIndex.SelectedValue.ToString());
            startHeight = m_hookedDrawing.Configuration.Floor_Thickness
              + (floorIndex - 1) * (m_hookedDrawing.Configuration.Floor_Thickness + m_hookedDrawing.Configuration.Floor_Headroom);

            sp = (SemanticPolygon)cboToArea.SelectedValue;
            sectionGeometry = ConverterForEsri.Create_Polygon_FromPoints(sp.Points, startHeight);
            area3D = GeometryUtility.Construct_StretchedBody(sectionGeometry, pExtrudeVector);
            area3DElement = ElementUtility.Contruct_MultiPatchElement(area3D, renderColor);
            m_stopsLayer.AddElement(area3DElement);


            m_hookedSceneControl.Refresh();
            #endregion
        }
        void tbxPlayDuration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[0-9]+");
            base.OnPreviewTextInput(e);
        }
        void tbxCameraHeightToFloor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[0-9]+");
            base.OnPreviewTextInput(e);
        }
        void tbxPlayDuration_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (playDuration != -1)
            {
                m_PathAnimationHelper.M_AGAnimationEnvironment.AnimationDuration = playDuration;
            }
            else
            {
                btnPlay.IsEnabled = false;
                btnExportVideo.IsEnabled = false;
            }
        }
        void tbxCameraHeightToFloor_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnPlay.IsEnabled = false;
            btnExportVideo.IsEnabled = false;

            if (!canRefreshSelectedFuctionArea())
            {
                btnCalcPath.IsEnabled = false;
                return;
            }

            if (cameraHeightToFloor == -1)
                btnCalcPath.IsEnabled = false;
            else
                btnCalcPath.IsEnabled = true;

        }
        void wayOfThroughFloorChanged(object sender, RoutedEventArgs e)
        {
            btnPlay.IsEnabled = false;
            btnExportVideo.IsEnabled = false;
        }

        void btnCalcPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //计算路径起终点
                MathExtension.Geometry.Point fromPoint = (cboFromArea.SelectedValue as SemanticPolygon).FunctionRegionPoint;
                MathExtension.Geometry.Point toPoint = (cboToArea.SelectedValue as SemanticPolygon).FunctionRegionPoint;

                if (fromPoint.Equals(toPoint))
                {
                    MessageBox.Show("路径起终点相同,无法计算路径", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }



                double floorHeight = m_hookedDrawing.Configuration.Floor_Headroom + m_hookedDrawing.Configuration.Floor_Thickness;

                fromPoint.Z = (int.Parse(cboFromFloorIndex.SelectedValue.ToString()) - 1) * floorHeight + m_hookedDrawing.Configuration.Floor_Thickness;
                toPoint.Z = (int.Parse(cboToFloorIndex.SelectedValue.ToString()) - 1) * floorHeight + m_hookedDrawing.Configuration.Floor_Thickness;

                //计算原始路径多段线(紧贴地板)
                /*功能区内的路径节点位于楼板上表面，而功能区的特征点则位于楼板下表面,
                 * 因此搜索容差选择楼板厚度的两倍
                 */
                double snapTolerance = 2 * m_hookedDrawing.Configuration.Floor_Thickness;
                List<IPoint> elevatorBarriesPoints = null;
                if (rbtThroughByStair.IsChecked == true)
                    elevatorBarriesPoints = get_elevatorBarries();

                m_originalNavigatePath =
                    m_routeAnalysisHelper.Calc_NavigationRoute(ConverterForEsri.Create_Point(fromPoint), ConverterForEsri.Create_Point(toPoint), elevatorBarriesPoints, snapTolerance);


                //按正常步行速度计算动画播放时长的推荐值
                tbxPlayDuration.Text = Math.Round((m_originalNavigatePath.Length / walkSpeed), 3).ToString();

                //double height = double.Parse(tbxCameraHeightToFloor.Text.Trim());
                //m_navigatePath = new ESRI.ArcGIS.Geometry.PolylineClass();
                //(m_navigatePath as IClone).Assign((m_originalNavigatePath as IClone).Clone());
                //(m_navigatePath as ITransform3D).Move3D(0d, 0d, height);

                //符号化路径
                IElement routeElement = get_RouteLineElement();
                if (routeElement != null && m_routeLayer != null)
                {
                    m_routeLayer.DeleteAllElements();
                    m_routeLayer.AddElement(routeElement);
                    m_hookedSceneControl.SceneGraph.RefreshViewers();
                }

                m_shouldReCreateAnimation = true;
                btnPlay.IsEnabled = true;
                btnExportVideo.IsEnabled = true;

            }
            catch (Exception ex)
            {
                btnPlay.IsEnabled = true;
                m_originalNavigatePath = null;

                MessageBox.Show(string.Format("路径计算失败：{0}", ex.Message), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (m_shouldReCreateAnimation)
            {
                m_PathAnimationHelper.CreateRouteAnimation(m_originalNavigatePath, cameraHeightToFloor);
                m_shouldReCreateAnimation = false;//将创建动画标识重置
            }

            m_PathAnimationHelper.PlayAnimation((esriAnimationPlayMode)cboPlayMode.SelectedValue, playDuration);
        }
        void btnPause_Click(object sender, RoutedEventArgs e)
        {
            m_PathAnimationHelper.PauseAnimation();
        }
        void btnStop_Click(object sender, RoutedEventArgs e)
        {
            m_PathAnimationHelper.StopAnimation();
        }
        void btnExportVideo_Click(object sender, RoutedEventArgs e)
        {
            if (m_shouldReCreateAnimation)
            {
                m_PathAnimationHelper.CreateRouteAnimation(m_originalNavigatePath, cameraHeightToFloor);
                m_shouldReCreateAnimation = false;//将创建动画标识重置
            }

            m_PathAnimationHelper.ExportAnimationToVideo();
        }
        #endregion

        #region 辅助方法
        //判断是否可刷新选择的功能区
        bool canRefreshSelectedFuctionArea()
        {
            if (cboFromFloorIndex.SelectedIndex < 0 || cboFromArea.SelectedIndex < 0 || cboToFloorIndex.SelectedIndex < 0 || cboToArea.SelectedIndex < 0)
                return false;
            return true;

        }

        bool canCalcAnimationPath()
        {
            if (!canRefreshSelectedFuctionArea())
                return false;

            if (cameraHeightToFloor == -1)
                return false;
            return true;
        }

        //获取路径元素
        IElement get_RouteLineElement()
        {
            if (m_originalNavigatePath == null)
                return null;

            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Color = ArcObjectsUtilities.ColorUtility.Get_RgbColor(0, 255, 0);
            simpleLineSymbol.Width = 5;
            simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;

            ILineElement routeElement = new LineElementClass();
            routeElement.Symbol = simpleLineSymbol;
            (routeElement as IElement).Geometry = m_originalNavigatePath;

            return routeElement as IElement;
        }

        //获取指定图层名的图层
        ILayer get_LayerByName(string layerName)
        {
            ILayer layer = null;
            for (int i = 0; i < m_hookedSceneControl.Scene.LayerCount; i++)
            {
                layer = m_hookedSceneControl.Scene.get_Layer(i);
                if (layer.Name == layerName)
                    return layer;
            }

            return null;
        }

        //检查路径分析图层组,若未创建"Stops"+"Routes"图层，则创建；否则清空图层包含的元素
        void checkAnalysisGroupLayer()
        {
            if (m_stopsLayer == null && m_routeLayer == null)
            {
                IGroupLayer analysisGroupLayer = (IGroupLayer)get_LayerByName(Resource.RouteAnalysisGroupLayerName);
                ICompositeLayer compositeLayer = (ICompositeLayer)analysisGroupLayer;

                ILayer layer = null;
                for (int i = 0; i < compositeLayer.Count; i++)
                {
                    layer = compositeLayer.get_Layer(i);

                    if (layer.Name == Resource.StopsLayerName)
                        m_stopsLayer = layer as IGraphicsContainer3D;


                    if (layer.Name == Resource.ResultRouteLayerName)
                        m_routeLayer = layer as IGraphicsContainer3D;
                }

                ////新建分析图层组，该图层组包含"Stops"+"Routes"图层
                //IGroupLayer analysisGroupLayer = new GroupLayerClass();
                //(analysisGroupLayer as ILayer).Name = m_routeAnalysisLayerName;

                ////停靠点图层
                //m_stopsLayer = new GraphicsLayer3DClass();
                //(m_stopsLayer as ILayer).Name = m_stopsLayerName;
                //analysisGroupLayer.Add(m_stopsLayer as ILayer);

                ////路径图层
                //m_routeLayer = new GraphicsLayer3DClass();
                //(m_routeLayer as ILayer).Name = m_routeLayerName;
                //analysisGroupLayer.Add(m_routeLayer as ILayer);


                //m_hookedSceneControl.Scene.AddLayer(analysisGroupLayer);
                //m_hookedSceneControl.Scene.MoveLayer(analysisGroupLayer, 0);

            }
            else
            {
                //清空停靠点和路径图层包含的元素
                ILayer analysisGroupLayer = get_LayerByName(m_routeAnalysisLayerName);
                ICompositeLayer compositeLayer = analysisGroupLayer as ICompositeLayer;
                for (int i = 0; i < compositeLayer.Count; i++)
                {
                    ILayer perLayer = compositeLayer.get_Layer(i);

                    if (perLayer.Name == m_routeLayerName || (perLayer.Name == m_stopsLayerName))
                        (perLayer as IGraphicsContainer3D).DeleteAllElements();
                }
            }
        }

        /// <summary>
        /// 获取电梯障碍点集
        /// </summary>
        /// <returns></returns>
        List<IPoint> get_elevatorBarries()
        {
            //获取电梯障碍点
            List<SemanticPolygon> esps = (from p in m_hookedDrawing.PResult.SPolygons
                                          where p.PType == PolygonType.ElevatorShaft
                                          select p).ToList<SemanticPolygon>();

            if (esps.Count == 0) return null;


            int fromFloorIndex = int.Parse(cboFromFloorIndex.SelectedValue.ToString());
            int toFloorIndex = int.Parse(cboToFloorIndex.SelectedValue.ToString());
            List<MathExtension.Geometry.Point> barriesPoints_firstFloor = null;
            List<IClone> barraiesPointClones_firstFloor = null;
            List<IPoint> barriesPoints = null;//结果
            double z_offset = 0d;
            ITransform3D transfrom3D = null;
            IClone clone = null;

            #region 获取电梯障碍点集
            //先收集第一层所有电梯间的特征点
            barriesPoints_firstFloor = new List<MathExtension.Geometry.Point>();
            for (int i = 0; i < esps.Count; i++)
                barriesPoints_firstFloor.Add(esps[i].FunctionRegionPoint);

            //再基于第一层的电梯间特征点集创建整个建筑内部的电梯间特征点集
            if (m_hookedDrawing.Configuration.RepeatCount == 1)
            {
                barriesPoints = EsriMapDataGenerator.GeometryCreator.ConverterForEsri.Create_Points_FromPoints(ref barriesPoints_firstFloor);
                return barriesPoints;
            }
            else
            {
                //先将第一层的点转换为Clones集合
                barraiesPointClones_firstFloor = new List<IClone>();
                barriesPoints = EsriMapDataGenerator.GeometryCreator.ConverterForEsri.Create_Points_FromPoints(ref barriesPoints_firstFloor);

                for (int i = 0; i < barriesPoints.Count; i++)
                    barraiesPointClones_firstFloor.Add(barriesPoints[i] as IClone);


                //再基于第一层创建所有楼层的点(第一层已收集，故以下将从第二层开始收集)
                for (int i = 2; i <= m_hookedDrawing.Configuration.RepeatCount; i++)
                {
                    z_offset = (i - 1) * m_hookedDrawing.Configuration.FloorHeight;
                    for (int j = 0; j < barraiesPointClones_firstFloor.Count; j++)
                    {
                        clone = new ESRI.ArcGIS.Geometry.PointClass();
                        clone.Assign(barraiesPointClones_firstFloor[j].Clone());

                        transfrom3D = clone as ITransform3D;
                        transfrom3D.Move3D(0d, 0d, z_offset);

                        barriesPoints.Add(clone as IPoint);
                    }
                }
                return barriesPoints;
            }
            #endregion
        }
        #endregion
    }


    public class TowMemberContainer
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public TowMemberContainer(string pKey, object pValue)
        {
            Key = pKey;
            Value = pValue;
        }
    }
}
