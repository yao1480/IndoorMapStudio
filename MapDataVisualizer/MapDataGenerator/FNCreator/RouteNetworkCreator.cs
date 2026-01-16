using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace EsriMapDataGenerator.FNCreator
{
    internal abstract class RouteNetworkCreator
    {
        /// <summary>
        /// 创建并构造网络数据集
        /// </summary>
        /// <param name="containingFeatureDataset">要在其中创建网络数据集的要素数据集</param>
        /// <param name="networkDatasetName">要创建的网络数据集名称</param>
        /// <param name="edgeSourceName">用于构建网络边源的要素类名称</param>
        /// <returns></returns>
        public static bool CreateNetDataSet(ref IFeatureDataset containingFeatureDataset, string networkDatasetName, string edgeSourceName)
        {
            try
            {
               
                IDENetworkDataset2 deNetworkdataset = new DENetworkDatasetClass();

        
                (deNetworkdataset as IDataElement).Name = networkDatasetName;

                deNetworkdataset.NetworkType = esriNetworkDatasetType.esriNDTGeodatabase;
                deNetworkdataset.Buildable = true;
                deNetworkdataset.ElevationModel = esriNetworkElevationModel.esriNEMZCoordinates;

               
                IGeoDataset pGeodataset = containingFeatureDataset as IGeoDataset;
                IDEGeoDataset deGeodataset = deNetworkdataset as IDEGeoDataset;
                deGeodataset.SpatialReference = pGeodataset.SpatialReference;
                deGeodataset.Extent = pGeodataset.Extent;
            

               
                IArray pSources = new ArrayClass();

                INetworkSource pNetWorkSource = new EdgeFeatureSourceClass();
                pNetWorkSource.Name = edgeSourceName;
                pNetWorkSource.ElementType = esriNetworkElementType.esriNETEdge;//边源

                IEdgeFeatureSource edgeFeatureSource = pNetWorkSource as IEdgeFeatureSource;
                edgeFeatureSource.ClassConnectivityGroup = 1;
                edgeFeatureSource.ClassConnectivityPolicy = esriNetworkEdgeConnectivityPolicy.esriNECPAnyVertex;//任意节点连通
                edgeFeatureSource.UsesSubtypes = false;

                pSources.Add(edgeFeatureSource);
                deNetworkdataset.Sources = pSources;
        

             
                IArray pAttributes = new ArrayClass();  

                INetworkAttribute3 netWorkAttribute = new EvaluatedNetworkAttributeClass();

        
                INetworkFieldEvaluator2 netWorkFieldEvaluator = null;
                INetworkConstantEvaluator networkConstantEvaluator = null;


                netWorkAttribute.Name = "Distance";
                netWorkAttribute.DataType = esriNetworkAttributeDataType.esriNADTDouble;
                netWorkAttribute.Units = esriNetworkAttributeUnits.esriNAUUnknown;
                netWorkAttribute.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
                netWorkAttribute.UseByDefault = true;

      
                IEvaluatedNetworkAttribute2 evaluatedNetworkAttribute = netWorkAttribute as IEvaluatedNetworkAttribute2;

                netWorkFieldEvaluator = new NetworkFieldEvaluatorClass();
                netWorkFieldEvaluator.SetExpression("[Shape_Length]", "");
              
                evaluatedNetworkAttribute.set_Evaluator(pNetWorkSource, esriNetworkEdgeDirection.esriNEDAlongDigitized, netWorkFieldEvaluator as INetworkEvaluator);

                netWorkFieldEvaluator = new NetworkFieldEvaluatorClass();
                netWorkFieldEvaluator.SetExpression("[Shape_Length]", "");
                evaluatedNetworkAttribute.set_Evaluator(pNetWorkSource, esriNetworkEdgeDirection.esriNEDAgainstDigitized, netWorkFieldEvaluator as INetworkEvaluator);





              
                networkConstantEvaluator = new NetworkConstantEvaluatorClass();
                networkConstantEvaluator.ConstantValue = 0d;
                evaluatedNetworkAttribute.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge, networkConstantEvaluator as INetworkEvaluator);

                networkConstantEvaluator = new NetworkConstantEvaluatorClass();
                networkConstantEvaluator.ConstantValue = 0d;
                evaluatedNetworkAttribute.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction, networkConstantEvaluator as INetworkEvaluator);

           
                networkConstantEvaluator = new NetworkConstantEvaluatorClass();
                networkConstantEvaluator.ConstantValue = 0d;
                evaluatedNetworkAttribute.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn, networkConstantEvaluator as INetworkEvaluator);

                pAttributes.Add(evaluatedNetworkAttribute);
                deNetworkdataset.Attributes = pAttributes;
     


             
                INetworkDataset pNetWorkDataset = null;

              
                IFeatureDatasetExtensionContainer pFeatureDatasetExtensionContainer = containingFeatureDataset as IFeatureDatasetExtensionContainer;
                IFeatureDatasetExtension pFeatureDatasetExtension = pFeatureDatasetExtensionContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
                IDatasetContainer2 pDatasetContainer = pFeatureDatasetExtension as IDatasetContainer2;

                IDataset pDataset = pDatasetContainer.CreateDataset(deNetworkdataset as IDEDataset);
                pNetWorkDataset = pDataset as INetworkDataset;

                
                INetworkBuild pNetworkBuild = pNetWorkDataset as INetworkBuild;
                pNetworkBuild.BuildNetwork((containingFeatureDataset as IGeoDataset).Extent);



                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("创建网络数据集失败： {0}", ex.Message), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
