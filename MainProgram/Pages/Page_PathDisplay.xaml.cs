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
using DXFReader.DataModel;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Microsoft.Win32;
using EsriMapDataGenerator;
using RE;
using MathExtension.Geometry;

namespace MainProgram.Pages
{
    /// <summary>
    /// Page_PathDisplay.xaml 的交互逻辑
    /// </summary>
    public partial class Page_PathDisplay : System.Windows.Controls.Page, IPageInterfaces
    {
        #region 内部字段
        //引用的外部变量
        MainWindow homeWindow = null;
        TabControl tabMainMenu = null;

        //内部字段
        IFeatureWorkspace pFeatureWorkspace = null;
        const string name_mapData2D = "MapData2D";//存放2D地图数据的数据集名称

        // 路径图相关
        private RouteGraph routeGraph = null;
        private List<string> roomNames = new List<string>();
        private Dictionary<string, int> roomNameToNodeIndex = new Dictionary<string, int>();
        private ICompositeGraphicsLayer highlightLayer = null;

        // 选择高亮相关
        private ICompositeGraphicsLayer selectionHighlightLayer = null;
        private List<MathExtension.Geometry.Point> allNodesForHighlight = null;
        #endregion

        #region 公开字段
        public static int PageIndex = 5;//页索引
        public AxMapControl MapControl = null;
        #endregion

        public Page_PathDisplay(MainWindow homeWindow)
        {
            InitializeComponent();

            //变量初始化
            this.homeWindow = homeWindow;
            this.tabMainMenu = homeWindow.tabMainMenu;

            //确保mapControlWrapper已初始化
            if (mapControlWrapper != null)
            {
                MapControl = mapControlWrapper.MapControl;
            }

            //事件订阅
            btnLast.Click += btnLast_Click;
            btnNext.Click += btnNext_Click;
            btnCreatePath.Click += btnCreatePath_Click;
            btnNavigate.Click += btnNavigate_Click;
            btnClearPath.Click += btnClearPath_Click;
            cmbStartPoint.SelectionChanged += cmbStartPoint_SelectionChanged;
            cmbEndPoint.SelectionChanged += cmbEndPoint_SelectionChanged;
        }

        #region 事件
        void btnLast_Click(object sender, RoutedEventArgs e)
        {
            homeWindow.Navigate(PageIndex - 1);
        }

        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            //没有下一步，禁用按钮
            btnNext.IsEnabled = false;
        }

        void btnCreatePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MapControl.ClearLayers();

                //获取2D地图数据集
                IFeatureDataset pMapDataset2D = homeWindow.Get_MapDataset2D();
                FloorPlan drawing = homeWindow.Drawing;

                //创建符号化图层（建筑要素）
                List<IFeatureLayer> pLayers
                    = EsriMapDataGenerator.DataGenerator.Create_SymbolizedLayers_2D(ref drawing, ref pMapDataset2D);

                //将所有图层添加到地图
                for (int i = 0; i < pLayers.Count; i++)
                {
                    MapControl.Map.AddLayer(pLayers[i]);
                }

                // ============ 计算并绘制路径 + 同时构建路径图 ============
                CreateRouteGraphicsLayersAndBuildGraph(ref drawing);

                // 填充下拉框
                cmbStartPoint.ItemsSource = roomNames;
                cmbEndPoint.ItemsSource = roomNames;
                cmbStartPoint.IsEnabled = true;
                cmbEndPoint.IsEnabled = true;
                btnNavigate.IsEnabled = true;

                MapControl.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void btnNavigate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbStartPoint.SelectedItem == null || cmbEndPoint.SelectedItem == null)
                {
                    MessageBox.Show("请选择起点和终点！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string startName = cmbStartPoint.SelectedItem.ToString();
                string endName = cmbEndPoint.SelectedItem.ToString();

                if (startName == endName)
                {
                    MessageBox.Show("起点和终点不能相同！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 获取节点索引
                int startIndex = roomNameToNodeIndex[startName];
                int endIndex = roomNameToNodeIndex[endName];

                // 计算最短路径
                List<int> path = routeGraph.Dijkstra(startIndex, endIndex);

                if (path == null || path.Count == 0)
                {
                    MessageBox.Show("无法找到从起点到终点的路径！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 计算路径长度
                double totalLength = 0;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    totalLength += routeGraph.GetEdgeWeight(path[i], path[i + 1]);
                }

                // 高亮显示路径（保留选择高亮）
                HighlightPath(path);

                // 更新路径信息
                txtPathLength.Text = string.Format("{0:F2} 米", totalLength / 1000); // 假设单位是毫米，转换为米
                txtNodeCount.Text = path.Count.ToString();

                btnClearPath.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("导航失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void btnClearPath_Click(object sender, RoutedEventArgs e)
        {
            ClearHighlightPath();
            ClearSelectionHighlight();

            // 重置下拉框选择
            cmbStartPoint.SelectedIndex = -1;
            cmbEndPoint.SelectedIndex = -1;

            txtPathLength.Text = "--";
            txtNodeCount.Text = "--";
            btnClearPath.IsEnabled = false;
        }

        void cmbStartPoint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionHighlight();
        }

        void cmbEndPoint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionHighlight();
        }

        /// <summary>
        /// 更新选择高亮显示
        /// </summary>
        void UpdateSelectionHighlight()
        {
            // 清除之前的选择高亮
            ClearSelectionHighlight();

            if (allNodesForHighlight == null || routeGraph == null)
                return;

            // 创建选择高亮图层
            selectionHighlightLayer = new CompositeGraphicsLayerClass();
            ILayer layer = selectionHighlightLayer as ILayer;
            layer.Name = "选择高亮";

            IGraphicsContainer gc = selectionHighlightLayer as IGraphicsContainer;

            // 高亮起点（绿色圆圈）
            if (cmbStartPoint.SelectedItem != null)
            {
                string startName = cmbStartPoint.SelectedItem.ToString();
                if (roomNameToNodeIndex.ContainsKey(startName))
                {
                    int nodeIndex = roomNameToNodeIndex[startName];
                    if (nodeIndex < allNodesForHighlight.Count)
                    {
                        var node = allNodesForHighlight[nodeIndex];

                        ISimpleMarkerSymbol startSymbol = new SimpleMarkerSymbolClass();
                        IRgbColor startColor = new RgbColorClass();
                        startColor.Red = 0;
                        startColor.Green = 200;
                        startColor.Blue = 0;
                        startSymbol.Color = startColor;
                        startSymbol.Size = 18;
                        startSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                        startSymbol.Outline = true;
                        IRgbColor outlineColor = new RgbColorClass();
                        outlineColor.Red = 0;
                        outlineColor.Green = 100;
                        outlineColor.Blue = 0;
                        startSymbol.OutlineColor = outlineColor;
                        startSymbol.OutlineSize = 2;

                        IMarkerElement markerElement = new MarkerElementClass();
                        markerElement.Symbol = startSymbol;
                        IElement element = markerElement as IElement;

                        IPoint point = new PointClass();
                        point.X = node.X;
                        point.Y = node.Y;
                        element.Geometry = point;
                        gc.AddElement(element, 0);
                    }
                }
            }

            // 高亮终点（蓝色圆圈）
            if (cmbEndPoint.SelectedItem != null)
            {
                string endName = cmbEndPoint.SelectedItem.ToString();
                if (roomNameToNodeIndex.ContainsKey(endName))
                {
                    int nodeIndex = roomNameToNodeIndex[endName];
                    if (nodeIndex < allNodesForHighlight.Count)
                    {
                        var node = allNodesForHighlight[nodeIndex];

                        ISimpleMarkerSymbol endSymbol = new SimpleMarkerSymbolClass();
                        IRgbColor endColor = new RgbColorClass();
                        endColor.Red = 0;
                        endColor.Green = 100;
                        endColor.Blue = 255;
                        endSymbol.Color = endColor;
                        endSymbol.Size = 18;
                        endSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                        endSymbol.Outline = true;
                        IRgbColor outlineColor = new RgbColorClass();
                        outlineColor.Red = 0;
                        outlineColor.Green = 0;
                        outlineColor.Blue = 150;
                        endSymbol.OutlineColor = outlineColor;
                        endSymbol.OutlineSize = 2;

                        IMarkerElement markerElement = new MarkerElementClass();
                        markerElement.Symbol = endSymbol;
                        IElement element = markerElement as IElement;

                        IPoint point = new PointClass();
                        point.X = node.X;
                        point.Y = node.Y;
                        element.Geometry = point;
                        gc.AddElement(element, 0);
                    }
                }
            }

            // 添加图层到地图
            MapControl.Map.AddLayer(layer);
            MapControl.Refresh();
        }

        /// <summary>
        /// 清除选择高亮
        /// </summary>
        void ClearSelectionHighlight()
        {
            if (selectionHighlightLayer != null)
            {
                try
                {
                    MapControl.Map.DeleteLayer(selectionHighlightLayer as ILayer);
                }
                catch { }
                selectionHighlightLayer = null;
            }
        }

        #endregion

        #region 路径图构建和绘制（合并为一个方法）

        /// <summary>
        /// 创建路径图形图层并同时构建路径图
        /// </summary>
        void CreateRouteGraphicsLayersAndBuildGraph(ref FloorPlan floorPlan)
        {
            if (floorPlan == null || floorPlan.PResult == null || floorPlan.PResult.SPolygons == null)
                return;

            // 初始化路径图
            routeGraph = new RouteGraph();
            roomNames.Clear();
            roomNameToNodeIndex.Clear();

            // 收集所有路径线和节点
            List<IPolyline> allRouteLines = new List<IPolyline>();
            List<MathExtension.Geometry.Point> allNodes = new List<MathExtension.Geometry.Point>();
            List<Tuple<int, int>> allEdges = new List<Tuple<int, int>>(); // 边（节点索引对）

            // 记录每个房间的功能区中心点索引
            Dictionary<int, string> roomCenterNodeNames = new Dictionary<int, string>();
            int roomCounter = 0;

            // 遍历所有功能区，计算路径
            foreach (var sp in floorPlan.PResult.SPolygons)
            {
                if (sp.PType == PolygonType.Wall || sp.PType == PolygonType.Balustrade || sp.PType == PolygonType.Floor)
                    continue;

                List<List<MathExtension.Geometry.Point>> roomRoutes = null;

                // 根据功能区类型调用不同的路径计算方法
                if (sp.PType == PolygonType.StairRoom)
                {
                    if (floorPlan.PResult.Stairs != null && floorPlan.PResult.Stairs.Count > 0)
                    {
                        // 找到距离当前楼梯间中心最近的楼梯
                        Stair matchedStair = null;
                        double minDist = double.MaxValue;

                        foreach (var stair in floorPlan.PResult.Stairs)
                        {
                            if (stair.InsertPoint != null)
                            {
                                double dist = CalcDistance(stair.InsertPoint, sp.FunctionRegionPoint);
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    matchedStair = stair;
                                }
                            }
                        }

                        // 如果没找到匹配的，用第一个
                        if (matchedStair == null)
                        {
                            matchedStair = floorPlan.PResult.Stairs[0];
                        }

                        SemanticPolygon spCopy = sp;
                        roomRoutes = RouteExtractor.Cacl_DoubleStairRoomNodes(ref spCopy, ref matchedStair, floorPlan.Configuration.Floor_Thickness);
                    }
                }
                else if (sp.PType == PolygonType.ElevatorShaft)
                {
                    SemanticPolygon spCopy = sp;
                    double floorHeight = floorPlan.Configuration.Floor_Headroom + floorPlan.Configuration.Floor_Thickness;
                    roomRoutes = RouteExtractor.Calc_ElevatorShaftNodes(ref spCopy, floorHeight);
                }
                else
                {
                    SemanticPolygon spCopy = sp;
                    roomRoutes = RouteExtractor.Calc_CommonRoomRoutes(ref spCopy);
                }

                // 处理路径
                if (roomRoutes != null)
                {
                    // 获取房间名称
                    string roomName = GetRoomTypeName(sp.PType) + "_" + (roomCounter + 1);
                    roomCounter++;

                    // 找到功能区中心点（通常是路径的终点）
                    MathExtension.Geometry.Point centerPoint = sp.FunctionRegionPoint;
                    int centerNodeIndex = GetOrAddNode(allNodes, centerPoint);

                    // 记录这个中心点对应的房间名称
                    if (!roomCenterNodeNames.ContainsKey(centerNodeIndex))
                    {
                        roomCenterNodeNames[centerNodeIndex] = roomName;
                        roomNames.Add(roomName);
                        roomNameToNodeIndex[roomName] = centerNodeIndex;
                    }

                    foreach (var route in roomRoutes)
                    {
                        if (route == null || route.Count < 2)
                            continue;

                        // 创建路径线用于显示
                        IPolyline polyline = CreatePolylineFromPoints(route);
                        if (polyline != null)
                        {
                            allRouteLines.Add(polyline);
                        }

                        // 添加节点和边到图中
                        for (int i = 0; i < route.Count; i++)
                        {
                            int nodeIndex = GetOrAddNode(allNodes, route[i]);

                            if (i > 0)
                            {
                                int prevNodeIndex = GetOrAddNode(allNodes, route[i - 1]);
                                // 添加边
                                allEdges.Add(new Tuple<int, int>(prevNodeIndex, nodeIndex));
                            }
                        }
                    }
                }
            }

            // ========== 改进：从每个房间的DoorPoints收集门点，对楼梯间进行特殊过滤 ==========
            List<MathExtension.Geometry.Point> doorPoints = new List<MathExtension.Geometry.Point>();
            HashSet<string> addedDoorPoints = new HashSet<string>(); // 用于去重

            foreach (var sp in floorPlan.PResult.SPolygons)
            {
                if (sp.PType == PolygonType.Wall || sp.PType == PolygonType.Balustrade || sp.PType == PolygonType.Floor)
                    continue;

                if (sp.DoorPoints == null || sp.DoorPoints.Count == 0)
                    continue;

                List<MathExtension.Geometry.Point> pointsToAdd = new List<MathExtension.Geometry.Point>();

                // 对楼梯间进行特殊处理：只保留距离中心最远的1-2个门点（真正的入口）
                if (sp.PType == PolygonType.StairRoom && sp.DoorPoints.Count > 2)
                {
                    // 按距离中心点的距离排序，取最远的2个
                    var sortedByDistance = sp.DoorPoints
                        .OrderByDescending(dp => CalcDistance(dp, sp.FunctionRegionPoint))
                        .Take(2)
                        .ToList();
                    pointsToAdd = sortedByDistance;
                }
                else
                {
                    pointsToAdd = sp.DoorPoints.ToList();
                }

                foreach (var dp in pointsToAdd)
                {
                    string key = Math.Round(dp.X, 1) + "_" + Math.Round(dp.Y, 1);
                    if (!addedDoorPoints.Contains(key))
                    {
                        addedDoorPoints.Add(key);
                        doorPoints.Add(dp);
                    }
                }
            }

            // 使用过滤后的门点进行处理
            if (doorPoints != null && doorPoints.Count >= 2)
            {
                // 添加门点到节点列表
                List<int> doorNodeIndices = new List<int>();
                foreach (var dp in doorPoints)
                {
                    int idx = GetOrAddNode(allNodes, dp);
                    doorNodeIndices.Add(idx);
                }

                // 连接互为最近的门点对
                int count = doorPoints.Count;
                int[] nearestIndex = new int[count];

                for (int i = 0; i < count; i++)
                {
                    double minDist = double.MaxValue;
                    int nearest = -1;

                    for (int j = 0; j < count; j++)
                    {
                        if (i == j) continue;
                        double dist = CalcDistance(doorPoints[i], doorPoints[j]);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearest = j;
                        }
                    }
                    nearestIndex[i] = nearest;
                }

                HashSet<string> connectedPairs = new HashSet<string>();
                for (int i = 0; i < count; i++)
                {
                    int j = nearestIndex[i];
                    if (j < 0) continue;

                    if (nearestIndex[j] == i)
                    {
                        string pairKey = Math.Min(i, j) + "_" + Math.Max(i, j);
                        if (!connectedPairs.Contains(pairKey))
                        {
                            connectedPairs.Add(pairKey);

                            // 添加边到图
                            allEdges.Add(new Tuple<int, int>(doorNodeIndices[i], doorNodeIndices[j]));

                            // 创建连接线用于显示
                            IPolyline connectLine = new PolylineClass();
                            IPointCollection pc = connectLine as IPointCollection;
                            object missing = Type.Missing;

                            IPoint pt1 = new PointClass();
                            pt1.X = doorPoints[i].X;
                            pt1.Y = doorPoints[i].Y;
                            pc.AddPoint(pt1, ref missing, ref missing);

                            IPoint pt2 = new PointClass();
                            pt2.X = doorPoints[j].X;
                            pt2.Y = doorPoints[j].Y;
                            pc.AddPoint(pt2, ref missing, ref missing);

                            allRouteLines.Add(connectLine);
                        }
                    }
                }
            }

            // 构建RouteGraph
            for (int i = 0; i < allNodes.Count; i++)
            {
                string nodeName = roomCenterNodeNames.ContainsKey(i) ? roomCenterNodeNames[i] : "节点_" + i;
                routeGraph.AddNode(i, allNodes[i].X, allNodes[i].Y, allNodes[i].Z, nodeName);
            }

            foreach (var edge in allEdges)
            {
                double dist = CalcDistance(allNodes[edge.Item1], allNodes[edge.Item2]);
                routeGraph.AddEdge(edge.Item1, edge.Item2, dist);
            }

            // 保存节点列表供选择高亮使用
            allNodesForHighlight = allNodes;

            // ============ 绘制图形图层 ============

            // 创建路径线图形图层
            if (allRouteLines.Count > 0)
            {
                ICompositeGraphicsLayer routeLineGraphicsLayer = new CompositeGraphicsLayerClass();
                ILayer routeLineLayer = routeLineGraphicsLayer as ILayer;
                routeLineLayer.Name = "导航路径";

                IGraphicsContainer gc = routeLineGraphicsLayer as IGraphicsContainer;
                ILineSymbol lineSymbol = CreateRouteLineSymbol();

                foreach (var line in allRouteLines)
                {
                    ILineElement lineElement = new LineElementClass();
                    lineElement.Symbol = lineSymbol;
                    IElement element = lineElement as IElement;
                    element.Geometry = line;
                    gc.AddElement(element, 0);
                }

                MapControl.Map.AddLayer(routeLineLayer);
            }

            // 创建路径节点图形图层（只绘制有连接的节点）
            HashSet<int> connectedNodeIndices = new HashSet<int>();
            foreach (var edge in allEdges)
            {
                connectedNodeIndices.Add(edge.Item1);
                connectedNodeIndices.Add(edge.Item2);
            }

            if (allNodes.Count > 0)
            {
                ICompositeGraphicsLayer nodeGraphicsLayer = new CompositeGraphicsLayerClass();
                ILayer nodeLayer = nodeGraphicsLayer as ILayer;
                nodeLayer.Name = "路径节点";

                IGraphicsContainer gc = nodeGraphicsLayer as IGraphicsContainer;
                IMarkerSymbol nodeSymbol = CreateRouteNodeSymbol();

                for (int i = 0; i < allNodes.Count; i++)
                {
                    // 只绘制有连接的节点
                    if (!connectedNodeIndices.Contains(i))
                        continue;

                    var node = allNodes[i];
                    IMarkerElement markerElement = new MarkerElementClass();
                    markerElement.Symbol = nodeSymbol;
                    IElement element = markerElement as IElement;

                    IPoint point = new PointClass();
                    point.X = node.X;
                    point.Y = node.Y;
                    point.Z = node.Z;
                    element.Geometry = point;
                    gc.AddElement(element, 0);
                }

                MapControl.Map.AddLayer(nodeLayer);
            }

            // 创建门点图形图层（使用过滤后的门点）
            if (doorPoints != null && doorPoints.Count > 0)
            {
                ICompositeGraphicsLayer doorGraphicsLayer = new CompositeGraphicsLayerClass();
                ILayer doorLayer = doorGraphicsLayer as ILayer;
                doorLayer.Name = "连接点";

                IGraphicsContainer gc = doorGraphicsLayer as IGraphicsContainer;
                IMarkerSymbol doorSymbol = CreateDoorPointSymbol();

                foreach (var dp in doorPoints)
                {
                    IMarkerElement markerElement = new MarkerElementClass();
                    markerElement.Symbol = doorSymbol;
                    IElement element = markerElement as IElement;

                    IPoint point = new PointClass();
                    point.X = dp.X;
                    point.Y = dp.Y;
                    point.Z = dp.Z;
                    element.Geometry = point;
                    gc.AddElement(element, 0);
                }

                MapControl.Map.AddLayer(doorLayer);
            }
        }

        /// <summary>
        /// 获取或添加节点，返回节点索引
        /// </summary>
        int GetOrAddNode(List<MathExtension.Geometry.Point> nodes, MathExtension.Geometry.Point point)
        {
            // 查找是否已存在（使用较小的阈值）
            for (int i = 0; i < nodes.Count; i++)
            {
                if (Math.Abs(nodes[i].X - point.X) < 0.1 &&
                    Math.Abs(nodes[i].Y - point.Y) < 0.1)
                {
                    return i;
                }
            }

            // 添加新节点
            nodes.Add(point);
            return nodes.Count - 1;
        }

        /// <summary>
        /// 判断点是否在多边形内部
        /// </summary>
        bool IsPointInsidePolygon(MathExtension.Geometry.Point point, SemanticPolygon polygon)
        {
            try
            {
                MathExtension.Geometry.Polygon poly = (MathExtension.Geometry.Polygon)polygon;
                var relation = MathExtension.Geometry.GeometryHelper.BasicRelation_2D(poly, point);
                return relation == MathExtension.Geometry.Relation2D_Point_Plane.InPlane;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断点是否在多边形边界上（容差范围内）
        /// </summary>
        bool IsPointOnPolygonBoundary(MathExtension.Geometry.Point point, SemanticPolygon polygon)
        {
            double tolerance = 100; // 容差，单位与图纸一致（mm）

            foreach (var line in polygon.Lines)
            {
                double dist = DistancePointToLine(point, line.StartPoint, line.EndPoint);
                if (dist < tolerance)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 计算点到线段的距离
        /// </summary>
        double DistancePointToLine(MathExtension.Geometry.Point point, MathExtension.Geometry.Point lineStart, MathExtension.Geometry.Point lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;

            if (dx == 0 && dy == 0)
            {
                return CalcDistance(point, lineStart);
            }

            double t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            MathExtension.Geometry.Point closest = new MathExtension.Geometry.Point(
                lineStart.X + t * dx,
                lineStart.Y + t * dy,
                0
            );

            return CalcDistance(point, closest);
        }

        /// <summary>
        /// 高亮显示路径
        /// </summary>
        void HighlightPath(List<int> path)
        {
            // 先清除之前的高亮
            ClearHighlightPath();

            // 创建高亮图层
            highlightLayer = new CompositeGraphicsLayerClass();
            ILayer layer = highlightLayer as ILayer;
            layer.Name = "导航高亮";

            IGraphicsContainer gc = highlightLayer as IGraphicsContainer;

            // 创建高亮线符号（红色粗线）
            ICartographicLineSymbol highlightLineSymbol = new CartographicLineSymbolClass();
            IRgbColor lineColor = new RgbColorClass();
            lineColor.Red = 255;
            lineColor.Green = 0;
            lineColor.Blue = 0;
            highlightLineSymbol.Color = lineColor;
            highlightLineSymbol.Width = 5;
            highlightLineSymbol.Cap = esriLineCapStyle.esriLCSRound;
            highlightLineSymbol.Join = esriLineJoinStyle.esriLJSRound;

            // 绘制路径
            for (int i = 0; i < path.Count - 1; i++)
            {
                RouteNode node1 = routeGraph.GetNode(path[i]);
                RouteNode node2 = routeGraph.GetNode(path[i + 1]);

                IPolyline polyline = new PolylineClass();
                IPointCollection pc = polyline as IPointCollection;
                object missing = Type.Missing;

                IPoint pt1 = new PointClass();
                pt1.X = node1.X;
                pt1.Y = node1.Y;
                pc.AddPoint(pt1, ref missing, ref missing);

                IPoint pt2 = new PointClass();
                pt2.X = node2.X;
                pt2.Y = node2.Y;
                pc.AddPoint(pt2, ref missing, ref missing);

                ILineElement lineElement = new LineElementClass();
                lineElement.Symbol = highlightLineSymbol as ILineSymbol;
                IElement element = lineElement as IElement;
                element.Geometry = polyline;
                gc.AddElement(element, 0);
            }

            // 创建起点标记（绿色大圆）
            ISimpleMarkerSymbol startSymbol = new SimpleMarkerSymbolClass();
            IRgbColor startColor = new RgbColorClass();
            startColor.Red = 0;
            startColor.Green = 200;
            startColor.Blue = 0;
            startSymbol.Color = startColor;
            startSymbol.Size = 15;
            startSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;

            RouteNode startNode = routeGraph.GetNode(path[0]);
            IPoint startPt = new PointClass();
            startPt.X = startNode.X;
            startPt.Y = startNode.Y;

            IMarkerElement startMarker = new MarkerElementClass();
            startMarker.Symbol = startSymbol;
            (startMarker as IElement).Geometry = startPt;
            gc.AddElement(startMarker as IElement, 0);

            // 创建终点标记（蓝色大圆）
            ISimpleMarkerSymbol endSymbol = new SimpleMarkerSymbolClass();
            IRgbColor endColor = new RgbColorClass();
            endColor.Red = 0;
            endColor.Green = 0;
            endColor.Blue = 255;
            endSymbol.Color = endColor;
            endSymbol.Size = 15;
            endSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;

            RouteNode endNode = routeGraph.GetNode(path[path.Count - 1]);
            IPoint endPt = new PointClass();
            endPt.X = endNode.X;
            endPt.Y = endNode.Y;

            IMarkerElement endMarker = new MarkerElementClass();
            endMarker.Symbol = endSymbol;
            (endMarker as IElement).Geometry = endPt;
            gc.AddElement(endMarker as IElement, 0);

            // 添加图层到地图
            MapControl.Map.AddLayer(layer);
            MapControl.Refresh();
        }

        /// <summary>
        /// 清除高亮路径
        /// </summary>
        void ClearHighlightPath()
        {
            if (highlightLayer != null)
            {
                try
                {
                    MapControl.Map.DeleteLayer(highlightLayer as ILayer);
                }
                catch { }
                highlightLayer = null;
                MapControl.Refresh();
            }
        }

        /// <summary>
        /// 获取房间类型名称
        /// </summary>
        string GetRoomTypeName(PolygonType pType)
        {
            switch (pType)
            {
                case PolygonType.BedRoom: return "卧室";
                case PolygonType.Parlour: return "客厅";
                case PolygonType.Toilet: return "卫生间";
                case PolygonType.Kitchen: return "厨房";
                case PolygonType.Balcony: return "阳台";
                case PolygonType.Study: return "书房";
                case PolygonType.StairRoom: return "楼梯间";
                case PolygonType.ElevatorShaft: return "电梯间";
                default: return "房间";
            }
        }

        /// <summary>
        /// 计算两点距离
        /// </summary>
        double CalcDistance(MathExtension.Geometry.Point p1, MathExtension.Geometry.Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2));
        }

        #endregion

        #region 符号创建方法

        /// <summary>
        /// 创建路径线符号（美化样式）
        /// </summary>
        ILineSymbol CreateRouteLineSymbol()
        {
            // 使用CartographicLineSymbol实现更美观的效果
            ICartographicLineSymbol cartographicLine = new CartographicLineSymbolClass();

            IRgbColor lineColor = new RgbColorClass();
            lineColor.Red = 255;
            lineColor.Green = 180;
            lineColor.Blue = 0;  // 橙黄色

            cartographicLine.Color = lineColor;
            cartographicLine.Width = 2.5;
            cartographicLine.Cap = esriLineCapStyle.esriLCSRound;
            cartographicLine.Join = esriLineJoinStyle.esriLJSRound;

            return cartographicLine as ILineSymbol;
        }

        /// <summary>
        /// 创建路径节点符号（美化样式）
        /// </summary>
        IMarkerSymbol CreateRouteNodeSymbol()
        {
            IMultiLayerMarkerSymbol multiSymbol = new MultiLayerMarkerSymbolClass();

            ISimpleMarkerSymbol outerSymbol = new SimpleMarkerSymbolClass();
            IRgbColor outerColor = new RgbColorClass();
            outerColor.Red = 255;
            outerColor.Green = 255;
            outerColor.Blue = 255;
            outerSymbol.Color = outerColor;
            outerSymbol.Size = 9;
            outerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            outerSymbol.Outline = true;
            IRgbColor outerOutlineColor = new RgbColorClass();
            outerOutlineColor.Red = 100;
            outerOutlineColor.Green = 100;
            outerOutlineColor.Blue = 100;
            outerSymbol.OutlineColor = outerOutlineColor;
            outerSymbol.OutlineSize = 1;
            multiSymbol.AddLayer(outerSymbol);

            ISimpleMarkerSymbol innerSymbol = new SimpleMarkerSymbolClass();
            IRgbColor innerColor = new RgbColorClass();
            innerColor.Red = 180;
            innerColor.Green = 30;
            innerColor.Blue = 30;
            innerSymbol.Color = innerColor;
            innerSymbol.Size = 5;
            innerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            multiSymbol.AddLayer(innerSymbol);

            return multiSymbol as IMarkerSymbol;
        }

        /// <summary>
        /// 创建门点符号（美化样式）
        /// </summary>
        IMarkerSymbol CreateDoorPointSymbol()
        {
            IMultiLayerMarkerSymbol multiSymbol = new MultiLayerMarkerSymbolClass();

            ISimpleMarkerSymbol outerSymbol = new SimpleMarkerSymbolClass();
            IRgbColor outerColor = new RgbColorClass();
            outerColor.Red = 0;
            outerColor.Green = 120;
            outerColor.Blue = 60;
            outerSymbol.Color = outerColor;
            outerSymbol.Size = 11;
            outerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;
            multiSymbol.AddLayer(outerSymbol);

            ISimpleMarkerSymbol innerSymbol = new SimpleMarkerSymbolClass();
            IRgbColor innerColor = new RgbColorClass();
            innerColor.Red = 100;
            innerColor.Green = 220;
            innerColor.Blue = 100;
            innerSymbol.Color = innerColor;
            innerSymbol.Size = 7;
            innerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;
            multiSymbol.AddLayer(innerSymbol);

            return multiSymbol as IMarkerSymbol;
        }

        /// <summary>
        /// 从点列表创建IPolyline
        /// </summary>
        IPolyline CreatePolylineFromPoints(List<MathExtension.Geometry.Point> points)
        {
            if (points == null || points.Count < 2)
                return null;

            IPointCollection pointCollection = new PolylineClass();

            foreach (var p in points)
            {
                IPoint point = new PointClass();
                point.X = p.X;
                point.Y = p.Y;
                point.Z = p.Z;
                object missing = Type.Missing;
                pointCollection.AddPoint(point, ref missing, ref missing);
            }

            IPolyline polyline = pointCollection as IPolyline;
            return polyline;
        }

        #endregion

        #region IPageInterfaces
        public void ActivateControls()
        {
            btnNext.IsEnabled = false;//没有下一步，禁用按钮
        }
        #endregion
    }

    #region 路径图数据结构

    /// <summary>
    /// 路径节点
    /// </summary>
    public class RouteNode
    {
        public int Index { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string Name { get; set; }

        public RouteNode(int index, double x, double y, double z, string name)
        {
            Index = index;
            X = x;
            Y = y;
            Z = z;
            Name = name;
        }
    }

    /// <summary>
    /// 路径图（用于Dijkstra算法）
    /// </summary>
    public class RouteGraph
    {
        private List<RouteNode> nodes = new List<RouteNode>();
        private Dictionary<int, List<Tuple<int, double>>> adjacencyList = new Dictionary<int, List<Tuple<int, double>>>();

        public void AddNode(int index, double x, double y, double z, string name)
        {
            nodes.Add(new RouteNode(index, x, y, z, name));
            if (!adjacencyList.ContainsKey(index))
            {
                adjacencyList[index] = new List<Tuple<int, double>>();
            }
        }

        public void AddEdge(int from, int to, double weight)
        {
            if (!adjacencyList.ContainsKey(from))
                adjacencyList[from] = new List<Tuple<int, double>>();
            if (!adjacencyList.ContainsKey(to))
                adjacencyList[to] = new List<Tuple<int, double>>();

            // 检查边是否已存在
            if (!adjacencyList[from].Any(e => e.Item1 == to))
            {
                adjacencyList[from].Add(new Tuple<int, double>(to, weight));
            }
            if (!adjacencyList[to].Any(e => e.Item1 == from))
            {
                adjacencyList[to].Add(new Tuple<int, double>(from, weight)); // 无向图
            }
        }

        public RouteNode GetNode(int index)
        {
            return nodes.FirstOrDefault(n => n.Index == index);
        }

        public int NodeCount => nodes.Count;

        public double GetEdgeWeight(int from, int to)
        {
            if (adjacencyList.ContainsKey(from))
            {
                var edge = adjacencyList[from].FirstOrDefault(e => e.Item1 == to);
                if (edge != null)
                    return edge.Item2;
            }
            return 0;
        }

        /// <summary>
        /// Dijkstra最短路径算法
        /// </summary>
        public List<int> Dijkstra(int start, int end)
        {
            int n = nodes.Count;
            if (n == 0) return null;

            double[] dist = new double[n];
            int[] prev = new int[n];
            bool[] visited = new bool[n];

            for (int i = 0; i < n; i++)
            {
                dist[i] = double.MaxValue;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[start] = 0;

            for (int i = 0; i < n; i++)
            {
                // 找到未访问节点中距离最小的
                int u = -1;
                double minDist = double.MaxValue;
                for (int j = 0; j < n; j++)
                {
                    if (!visited[j] && dist[j] < minDist)
                    {
                        minDist = dist[j];
                        u = j;
                    }
                }

                if (u == -1 || u == end)
                    break;

                visited[u] = true;

                // 更新相邻节点的距离
                if (adjacencyList.ContainsKey(u))
                {
                    foreach (var edge in adjacencyList[u])
                    {
                        int v = edge.Item1;
                        double weight = edge.Item2;

                        if (!visited[v] && dist[u] + weight < dist[v])
                        {
                            dist[v] = dist[u] + weight;
                            prev[v] = u;
                        }
                    }
                }
            }

            // 构建路径
            if (dist[end] == double.MaxValue)
                return null; // 无法到达

            List<int> path = new List<int>();
            int current = end;
            while (current != -1)
            {
                path.Insert(0, current);
                current = prev[current];
            }

            return path;
        }
    }

    #endregion
}