using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace SilenceLibraryForAE.MethodLibrary
{
    public sealed class OutputHelper
    {
        /// <summary>
        /// 从ActiveView创建指定Dpi的图片
        /// </summary>
        /// <param name="activeView">视图</param>
        /// <param name="pathFileName">图片的完整文件名</param>
        /// <param name="outputDpi">输出的Dpi(缺省值：300dpi)</param>
        /// <returns></returns>
        public static System.Boolean CreatePictureFromActiveView(IActiveView activeView, String pathFileName, double outputDpi = 300)
        {
            //参数检查
            if (activeView == null)
            {
                return false;
            }

            ESRI.ArcGIS.Output.IExport export = new ESRI.ArcGIS.Output.ExportJPEGClass();

            //导出路径
            export.ExportFileName = pathFileName;

            //导出分辨率
            export.Resolution = outputDpi;

            //假设屏幕默认的默认的Dpi=96
            int screenDpi = 96;

            //输出范围
            ESRI.ArcGIS.esriSystem.tagRECT exportRectangle = new ESRI.ArcGIS.esriSystem.tagRECT()
            {
                left = activeView.ExportFrame.left,
                top = activeView.ExportFrame.top,
                right = (int)(activeView.ExportFrame.right * (outputDpi / screenDpi)),
                bottom = (int)(activeView.ExportFrame.bottom * (outputDpi / screenDpi))
            };

            IEnvelope exportPixelBounds = new EnvelopeClass()
            {
                XMin = 0,
                YMin = 0,
                XMax = exportRectangle.right,
                YMax = exportRectangle.bottom
            };
            export.PixelBounds = exportPixelBounds;


            /*（1）输出范围的像素数不一定是当前设备下导出对象的像素数，而是在目标Dpi下对应的像素数
             *（2）export.PixelBounds一定要和Output中的PixelBounds相同
             *（3）目前只能基于ActiveView.ExportFrame去控制采样范围,暂没发现可以定于采样区
             */

            int hDC = export.StartExporting();
            activeView.Output(hDC, (System.Int16)export.Resolution, ref exportRectangle, activeView.FullExtent, null); // Explicit Cast and 'ref' keyword needed 
            export.FinishExporting();
            export.Cleanup();
            return true;
        }


        public static bool PrintMap()
        {
            return false;
        }

    }
}


