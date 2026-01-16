using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;

namespace SilenceLibraryForAE.MethodLibrary
{
    public sealed class GetLayerHelper
    {

        #region 获取CAD图层
        /*1结尾：获取结果是ICadLayer,即获取的是矢量+栅格组成的混合形式的Cad图层;此时不能Identify
         *2结尾：获取结果是AE分类后的List<ILyer>集合
         */


        /// <summary>
        /// 从dwg文件获取ICadLayer，原始Cad图层被整合到单图层中(获取结果是是矢量+栅格组成的混合形式的Cad图层;此时不能Identify)
        /// </summary>
        /// <param name="dwgFilePath">dwg文件路径</param>
        /// <returns></returns>
        public static ICadLayer GetCad1_FromFile(string dwgFilePath)
        {
            try
            {
                //获取Cad文件的父路径
                string filePathWithoutFileName = System.IO.Directory.GetParent(dwgFilePath).FullName;

                //获取父路径所在的工作空间
                IWorkspaceFactory pWorkspaceFactory = new CadWorkspaceFactoryClass();
                ICadDrawingWorkspace pCadDrawingWorksapce = pWorkspaceFactory.OpenFromFile(filePathWithoutFileName, 0) as ICadDrawingWorkspace;

                //获取Cad纯文件名（带扩展名）
                int index = dwgFilePath.LastIndexOf(@"\");
                int legth = dwgFilePath.Length - index-1;

                //获取Cad数据
                string safeFileName = dwgFilePath.Substring(index+1, legth);
                ICadDrawingDataset pCadDrawingDataSet = pCadDrawingWorksapce.OpenCadDrawingDataset(safeFileName);

                //设置Cad图层
                ICadLayer pCadLayer = new CadLayerClass();
                pCadLayer.CadDrawingDataset = pCadDrawingDataSet;
                pCadLayer.Name = safeFileName;

                return pCadLayer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("从DWG文件获取Cad图层失败：\n{0} ！", ex.Message), "获取CAD图层失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 从dwg文件获取List<ILyer>(原始Cad图层被分为矢量+栅格两大类，其中矢量部分又被分为几何（依据点线面再细分）+注记两大部分,此时能Identify)
        /// </summary>
        /// <param name="dwgFilePath">dwg文件路径</param>
        /// <returns></returns>
        public static List<ILayer> GetCad2_FromFile(string dwgFilePath)
        {
            List<ILayer> layers = new List<ILayer>();


            //获取Cad文件的父路径
            string filePathWithoutFileName = System.IO.Directory.GetParent(dwgFilePath).FullName;

            //获取父路径所在的工作空间
            IWorkspaceFactory pWorkspaceFactory = new CadWorkspaceFactoryClass();
            IFeatureWorkspace pFeatureWorksapce = pWorkspaceFactory.OpenFromFile(filePathWithoutFileName, 0) as IFeatureWorkspace;

            //获取Cad纯文件名（带扩展名）
            int index = dwgFilePath.LastIndexOf(@"\");
            int legth = dwgFilePath.Length - index-1;

            //获取Cad数据集
            string safeFileName = dwgFilePath.Substring(index+1, legth);
            IFeatureDataset pFeatureDataSet = pFeatureWorksapce.OpenFeatureDataset(safeFileName);

            //将Cad要素图层分成点线面+注记两大类型
            IFeatureClassContainer pFeatureClassContainer = pFeatureDataSet as IFeatureClassContainer;
            IFeatureClass pFeatureClass;
            IFeatureLayer pFeatureLayer;

            for (int i = 0; i < pFeatureClassContainer.ClassCount; i++)
            {
                //分为注记+普通要素图层
                pFeatureClass = pFeatureClassContainer.get_Class(i);
                if (pFeatureClass.FeatureType == esriFeatureType.esriFTCoverageAnnotation)
                {
                    pFeatureLayer = new CadAnnotationLayerClass();

                    //注记图层置顶
                    layers.Insert(0, pFeatureLayer);
                }

                else
                {
                    pFeatureLayer = new FeatureLayerClass();

                    //添加到总图层集合
                    layers.Add(pFeatureLayer);
                }

                //配置图层属性
                pFeatureLayer.Name = pFeatureClass.AliasName;
                pFeatureLayer.FeatureClass = pFeatureClass;
            }

            return layers;
        }

        //从数据库获取ICadLayer
        public static ICadLayer Get_CadLayerFromGeo()
        {
            return null;
        }

        #endregion


        #region 获取TIN图层
        public static ITinLayer Get_TinLayerFromFile()
        {
            return null;
        }

        public static ITinLayer Get_TinLayerFromGeoDataBase()
        {
            return null;
        }
        #endregion
    }
}
