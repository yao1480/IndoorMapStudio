using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace EsriMapDataGenerator
{
    public abstract class FGDBHelper
    {
        const string name_mapDataset2D = "MapData2D";
        const string name_mapDataset3D = "MapData3D";
        const string networkDatasetName = "Route_ND";//路径网络数据集名称

        /// <summary>
        /// 检查MapDataFGDB是否有效（检查文件夹存在性+工作空间有效性+是否包含两个数据集（MapDataset2D、MapDataset3D）,不检查是否处于初始状态）
        /// </summary>
        /// <param name="fgdbPath"></param>
        /// <returns></returns>
        public static void CheckMapDataFGDB_Validity(string fgdbPath)
        {
            //检查文件夹是否存在
            if (!System.IO.Directory.Exists(fgdbPath))
                throw new ArgumentException(string.Format("文件地理数据库不存在(查找路径：{0})", fgdbPath));

            //检查文件夹是否是有效的地理数据库
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            if (!pWorkspaceFactory.IsWorkspace(fgdbPath))
                throw new ArgumentException(string.Format("无效的工作空间(路径：{0})", fgdbPath));

            //检查FGDB是否包含两个数据集：MapDataset2D+MapDataset3D
            bool hasMapDataset2D = false;
            bool hasMapDataset3D = false;
            IWorkspace pWorlspace = pWorkspaceFactory.OpenFromFile(fgdbPath, 0);
            IDataset pDataset = (IDataset)pWorlspace;
            IEnumDataset pEnumDataset = pDataset.Subsets;
            pDataset = pEnumDataset.Next();
            while (pDataset != null)
            {
                if (pDataset.Name == name_mapDataset2D)
                    hasMapDataset2D = true;

                if (pDataset.Name == name_mapDataset3D)
                    hasMapDataset3D = true;

                if (hasMapDataset2D && hasMapDataset3D)
                    break;

                pDataset = pEnumDataset.Next();
            }

            if (!hasMapDataset2D)
                throw new Exception(string.Format("在 {0} 中未找到数据集 {}！", fgdbPath, name_mapDataset2D));

            if (!hasMapDataset3D)
                throw new Exception(string.Format("在 {0} 中未找到数据集 {}！", fgdbPath, name_mapDataset3D));
        }

        /// <summary>
        /// 检查MapDataFGDB是否处于初始状态(MapDataFGDB有效性+是否处于初始状态)
        /// </summary>
        /// <param name="fgdbPath">地理数据库文件夹路径</param>
        public static void CheckMapDataFGDB_Initial(string fgdbPath)
        {
            //检查文件夹是否是有效的MapDataFGDB
            CheckMapDataFGDB_Validity(fgdbPath);
           
            //检查数据集是否是初始状态
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace pFeatureWorlspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(fgdbPath, 0);
            IFeatureDataset pFeatureDataset = null;
            pFeatureDataset = pFeatureWorlspace.OpenFeatureDataset(name_mapDataset2D);
            if (!IsIntialMapDataset(ref pFeatureDataset))
                throw new Exception(string.Format("数据集 {0} 已包含要素", name_mapDataset2D));

            pFeatureDataset = pFeatureWorlspace.OpenFeatureDataset(name_mapDataset3D);
            if (!IsIntialMapDataset(ref pFeatureDataset))
                throw new Exception(string.Format("数据集 {0} 已包含要素", name_mapDataset3D));
        }

        public static void CreateMapDataFGDB_FromTemplate(string sourceParentDirectory, string destinationParentDirectory, string fgdbSafeName = null)
        {
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();

            string[] fileNames = System.IO.Directory.GetDirectories(sourceParentDirectory);
            IFileNames pFileNames = new FileNamesClass();
            foreach (var item in fileNames)
            {
                pFileNames.Add(item);
            }

            IWorkspaceName pSourceWorksapceName = pWorkspaceFactory.GetWorkspaceName(sourceParentDirectory, pFileNames);
            IWorkspaceName pToWorkspaceName = new WorkspaceNameClass();
            bool result = pWorkspaceFactory.Copy(pSourceWorksapceName, destinationParentDirectory, out pToWorkspaceName);

            if (result == false)
                throw new ArgumentException("复制地理数据库模板失败！");

            //更改名称
            if (!string.IsNullOrEmpty(fgdbSafeName) || !string.IsNullOrWhiteSpace(fgdbSafeName))
            {
                IDataset pDataset = ((pToWorkspaceName as IName).Open() as IWorkspace) as IDataset;
                if (pDataset.CanRename())
                    pDataset.Rename(fgdbSafeName);
                else
                    throw new Exception(string.Format("无法创建指定名称（{0}）的地理数据库：无法重命名FGDB！", fgdbSafeName));
            }
        }

        /// <summary>
        /// 检查地图数据集是否是初始状态（逐要素类检查包含的要素数目是否为0）
        /// </summary>
        /// <param name="pMapDataset"></param>
        /// <param name="pMapDataset3D"></param>
        /// <returns></returns>
        public static bool IsIntialMapDataset(ref IFeatureDataset pMapDataset)
        {
            IFeatureClassContainer pFeatureClassContainer = null;
            IFeatureClass pFeatureClass = null;

            //检查MapDataset2D
            pFeatureClassContainer = pMapDataset as IFeatureClassContainer;
            for (int i = 0; i < pFeatureClassContainer.ClassCount; i++)
            {
                pFeatureClass = pFeatureClassContainer.get_Class(i);
                if (pFeatureClass.FeatureCount(null) > 0) return false;
            }

            return true;
        }

        /// <summary>
        /// 重置MapDataset2D
        /// </summary>
        /// <param name="pMapDataset2D"></param>
        public static void ResetMapDataset2D(ref IFeatureDataset pMapDataset2D)
        {
            try
            {
                IFeatureClassContainer pFeatureClassContainer = null;
                IFeatureClass pFeatureClass = null;
                IFeatureCursor pFeatureCursor = null;
                IFeature pFeature = null;

                //检查MapDataset2D
                pFeatureClassContainer = pMapDataset2D as IFeatureClassContainer;
                for (int i = 0; i < pFeatureClassContainer.ClassCount; i++)
                {
                    pFeatureClass = pFeatureClassContainer.get_Class(i);
                    pFeatureCursor = pFeatureClass.Search(null, false);

                    pFeature = pFeatureCursor.NextFeature();
                    while (pFeature != null)
                    {
                        pFeature.Delete();
                        pFeature = pFeatureCursor.NextFeature();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("重置MapDataSet2D失败： {0}!", ex.Message));
            }
        }

        /// <summary>
        /// 重置MapDataset3D
        /// </summary>
        /// <param name="pMapDataset3D"></param>
        public static void ResetMapDataset3D(ref IFeatureDataset pMapDataset3D)
        {
            try
            {
                IFeatureClassContainer pFeatureClassContainer = null;
                IFeatureClass pFeatureClass = null;
                IFeatureCursor pFeatureCursor = null;
                IFeature pFeature = null;

                //检查MapDataset2D
                pFeatureClassContainer = pMapDataset3D as IFeatureClassContainer;
                for (int i = 0; i < pFeatureClassContainer.ClassCount; i++)
                {
                    pFeatureClass = pFeatureClassContainer.get_Class(i);
                    pFeatureCursor = pFeatureClass.Search(null, false);

                    pFeature = pFeatureCursor.NextFeature();
                    while (pFeature != null)
                    {
                        pFeature.Delete();
                        pFeature = pFeatureCursor.NextFeature();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("重置MapDataSet3D失败： {0}!", ex.Message));
            }
        }

        /// <summary>
        /// 返回Worspace
        /// </summary>
        /// <param name="fgdbPath"></param>
        /// <returns></returns>
        public static IWorkspace Get_Workspace(string fgdbPath)
        {
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            if (!pWorkspaceFactory.IsWorkspace(fgdbPath))
                throw new ArgumentException(string.Format("无效的文件地理数据库(路径：{0})", fgdbPath));

            return pWorkspaceFactory.OpenFromFile(fgdbPath, 0);
        }

        public static IFeatureDataset Get_MapDataset2D(string fgdbPath)
        {
            IFeatureWorkspace pFeatureWorkspace = Get_Workspace(fgdbPath) as IFeatureWorkspace;
            return pFeatureWorkspace.OpenFeatureDataset(name_mapDataset2D);
        }

        public static IFeatureDataset Get_MapDataset3D(string fgdbPath)
        {
            IFeatureWorkspace pFeatureWorkspace = Get_Workspace(fgdbPath) as IFeatureWorkspace;
            return pFeatureWorkspace.OpenFeatureDataset(name_mapDataset3D);
        }

        /// <summary>
        /// 获取FGDB中MapData3D数据集中的网络数据集Route_ND
        /// </summary>
        /// <param name="fgdbPath"></param>
        /// <returns></returns>
        public static INetworkDataset Get_NetworkDataset(string fgdbPath)
        {
            INetworkDataset pNetWorkDataset = null;

            IFeatureDataset containingFeatureDataset = Get_MapDataset3D(fgdbPath);
            IFeatureDatasetExtensionContainer pFeatureDatasetExtensionContainer = containingFeatureDataset as IFeatureDatasetExtensionContainer;
            IFeatureDatasetExtension pFeatureDatasetExtension = pFeatureDatasetExtensionContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
            IDatasetContainer2 pDatasetContainer = pFeatureDatasetExtension as IDatasetContainer2;

            if (pDatasetContainer == null)
                return null;
            else
            {
                IDataset pDataset = pDatasetContainer.get_DatasetByName(esriDatasetType.esriDTNetworkDataset, networkDatasetName);
                pNetWorkDataset = pDataset as INetworkDataset;
                return pNetWorkDataset;
            }

        }
    }
}
