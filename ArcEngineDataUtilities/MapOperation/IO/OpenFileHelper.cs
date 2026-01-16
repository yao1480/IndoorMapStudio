using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ArcEngineDataUtilities.DataOperation;

namespace SilenceLibraryForAE.MethodLibrary
{
    public sealed class OpenFileHelper
    {
        //打开Mxd文件
        public static void LoadMXD(string mxdFilePath, IMapControlDefault pMapControl)
        {
            if (!pMapControl.CheckMxFile(mxdFilePath))
            {
                MessageBox.Show(mxdFilePath + " 不是有效的地图文档(*.MXD)", "文档无效", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    pMapControl.MousePointer = esriControlsMousePointer.esriPointerHourglass;
                    pMapControl.LoadMxFile(mxdFilePath);
                    pMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("文档打开失败：\n{0} ！", ex.Message), "文档打开失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        //打开CAD文件
        public static void LoadCadAsOneLayerFromFile(string dwgFilePath, IMapControlDefault pMapControl)
        {
            ICadLayer pCadLayer = LayerHelper.GetCad1_FromFile(dwgFilePath);

            if (pCadLayer != null)
            {
                pMapControl.AddLayer(pCadLayer);
                pMapControl.Refresh();
            }
        }

        //打开CAD文件
        public static void LoadCadAsMultiLayersFromFile(string dwgFilePath, IMapControlDefault pMapControl)
        {
            List<ILayer> layers = LayerHelper.GetCad2_FromFile(dwgFilePath);

            //倒序添加
            for (int i = layers.Count-1; i >-1; i--)
            {
                pMapControl.AddLayer(layers[i]);
            }

            pMapControl.Refresh();
        }
    }
}
