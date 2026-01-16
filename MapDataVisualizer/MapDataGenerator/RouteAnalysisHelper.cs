using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.NetworkAnalyst;

namespace EsriMapDataGenerator
{
    /// <summary>
    /// 为路径分析功能提供帮助函数
    /// </summary>
    public class RouteAnalysisHelper
    {
        const string m_resultLayerName = "ShortestPath";
        const string m_stopsName = "Stops";
        const string m_pointBarriesName = "Barriers";
        const string m_routeName = "Routes";
        IFeatureClass m_stopsFeatureClass = null;
        IFeatureClass m_pointBarriesFeatureClass = null;
        INetworkDataset m_networkDataset = null;


        public RouteAnalysisHelper(ref IFeatureClass pStopsFeatureClass, ref IFeatureClass pPointBarriesFeatureClass, ref INetworkDataset pNetworkDataset)
        {
            m_stopsFeatureClass = pStopsFeatureClass;
            m_networkDataset = pNetworkDataset;
            m_pointBarriesFeatureClass = pPointBarriesFeatureClass;
        }


        /// <summary>
        /// 求解两点最短路径
        /// </summary>
        /// <param name="fromPoint"></param>
        /// <param name="toPoint"></param>
        /// <param name="pSnapTolerance">搜索容差</param>
        /// <returns>IPolyline</returns>
        public IPolyline Calc_NavigationRoute(IPoint fromPoint, IPoint toPoint,  List<IPoint> pointBarries,  double pSnapTolerance = 100)
        {
            #region 停靠点和点障碍要素更新
            if (m_stopsFeatureClass.FeatureCount(null) > 0)
            {
                ITable table = m_stopsFeatureClass as ITable;
                table.DeleteSearchedRows(null);
            }

  
            IFeature feature = null;
            feature = m_stopsFeatureClass.CreateFeature();
            feature.Shape = fromPoint;
            feature.Store();

            feature = m_stopsFeatureClass.CreateFeature();
            feature.Shape = toPoint;
            feature.Store();




            if (m_pointBarriesFeatureClass.FeatureCount(null) > 0)
            {
                ITable table = m_pointBarriesFeatureClass as ITable;
                table.DeleteSearchedRows(null);
            }

 
            if (pointBarries != null)
            {
                for (int i = 0; i < pointBarries.Count; i++)
                {
                    feature = m_pointBarriesFeatureClass.CreateFeature();
                    feature.Shape = pointBarries[i];
                    feature.Store();
                }
            }
            #endregion


            #region 路径求解
            INASolver naSolver = null;
            INARouteSolver2 naRouteSolver = null;
            INAContext naContext = null;
            INAContextEdit naContextEdit = null;
            IDENetworkDataset deNeworkDataset = null;
            INAClass stopsNAClass = null;
            INAClass pointBarriersNAClass = null;//点障碍类
            INAClassFieldMap naClassFieldMap = null;
            INAClassLoader naClassLoader = null;
            INALocator3 locator = null;

            naSolver = new NARouteSolverClass();

    
            IDatasetComponent datasetComponent = m_networkDataset as IDatasetComponent;
            deNeworkDataset = datasetComponent.DataElement as IDENetworkDataset;
            naContext = naSolver.CreateContext(deNeworkDataset, m_resultLayerName);
            naContextEdit = naContext as INAContextEdit;
            naContextEdit.Bind(m_networkDataset, new GPMessagesClass());


            naRouteSolver = naSolver as INARouteSolver2;
            naRouteSolver.PreserveFirstStop = true;
            naRouteSolver.PreserveLastStop = true;
            naRouteSolver.OutputLines = esriNAOutputLineType.esriNAOutputLineTrueShapeWithMeasure;
            naRouteSolver.FindBestSequence = true;
            naRouteSolver.UseTimeWindows = false;//不考虑Windows时间
            naSolver.UpdateContext(naContext, deNeworkDataset, new GPMessagesClass());

         
            naClassLoader = new NAClassLoaderClass();

            #region 加载停靠点“Stops”（Stops为固定名称，INAContext.NAClasses 5 个类之一）
            naClassLoader.Locator = naContext.Locator;
            naClassLoader.Locator.SnapTolerance = pSnapTolerance;//停靠点搜索容差

            naClassFieldMap = new NAClassFieldMapClass();
            naClassFieldMap.set_MappedField("FID", "FID");
            naClassLoader.FieldMap = naClassFieldMap;

            stopsNAClass = naContext.NAClasses.get_ItemByName(m_stopsName) as INAClass;
            naClassLoader.NAClass = stopsNAClass;

            // Avoid loading network locations onto non-traversable portions of elements
            locator = naContext.Locator as INALocator3;
            locator.ExcludeRestrictedElements = true;
            locator.CacheRestrictedElements(naContext);

            int rowsInCursor = 0;
            int rowsLocated = 0;
            ICursor cursor = m_stopsFeatureClass.Search(new QueryFilterClass(), false) as ICursor;
            naClassLoader.Load(cursor, new CancelTrackerClass(), ref rowsInCursor, ref rowsLocated);
            #endregion

            #region 加载点障碍
            pointBarriersNAClass = naContext.NAClasses.get_ItemByName(m_pointBarriesName) as INAClass;
            naClassLoader.NAClass = pointBarriersNAClass;

            // Avoid loading network locations onto non-traversable portions of elements
            locator = naContext.Locator as INALocator3;
            locator.ExcludeRestrictedElements = true;
            locator.CacheRestrictedElements(naContext);

            rowsInCursor = 0;
            rowsLocated = 0;
            cursor = m_pointBarriesFeatureClass.Search(new QueryFilterClass(), false) as ICursor;
            naClassLoader.Load(cursor, new CancelTrackerClass(), ref rowsInCursor, ref rowsLocated);
            #endregion




            //Message all of the network analysis agents that the analysis context has changed
            naContextEdit.ContextChanged();

            GPMessagesClass gpm = new GPMessagesClass();
            naSolver.Solve(naContext, gpm, new CancelTrackerClass());
            INAResult naResult = naContext.Result;
            #endregion


            if (!naResult.HasValidResult)
                throw new Exception("路径求解结果无效");


            IFeatureClass routeFeatureClass = naResult.NAContext.NAClasses.get_ItemByName(m_routeName) as IFeatureClass;

            IFeature routeFeature = routeFeatureClass.Search(null, false).NextFeature();
            return routeFeature.Shape as IPolyline;
        }
    }
}
