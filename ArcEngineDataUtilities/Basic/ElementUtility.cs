using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace ArcEngineDataUtilities.Basic
{
    public class ElementUtility
    {
        public static IElement Construct_PointElement(IGeometry pointGeometry, IColor color, double size = 10, esriSimpleMarkerStyle style = esriSimpleMarkerStyle.esriSMSCircle)
        {
            IElement pElement = new MarkerElementClass();
            pElement.Geometry = pointGeometry;

            ISimpleMarkerSymbol pMarkerSimpleSymbol = new SimpleMarkerSymbolClass();
            pMarkerSimpleSymbol.Color = color;
            pMarkerSimpleSymbol.Size = size;
            pMarkerSimpleSymbol.Style = style;

            (pElement as IMarkerElement).Symbol = pMarkerSimpleSymbol as IMarkerSymbol;

            return pElement;
        }


        /// <summary>
        /// 构造PolylineElement(使用ISimpleLine3DSymbol)
        /// </summary>
        /// <param name="geometry">线段的IGeometry</param>
        /// <param name="color">符号颜色</param>
        /// <param name="width">线宽（默认0.1）</param>
        /// <returns></returns>
        public static IElement Construct_LineElement(IGeometry geometry, IColor color, double width = 1)
        {
            IElement element = new LineElementClass();
            element.Geometry = geometry;

            ILineSymbol lineSymbol = new SimpleLineSymbolClass();
            lineSymbol.Color = color;
            lineSymbol.Width = width;

            (element as ILineElement).Symbol = lineSymbol;

            return element;
        }


        /// <summary>
        /// 构造MultiPatchElement(使用SimpleFillSymbol)
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static IElement Contruct_MultiPatchElement(IGeometry geometry, IColor color)
        {
            //基于几何创建元素
            IElement element = new MultiPatchElementClass();
            element.Geometry = geometry;


            //基于颜色创建填充符号
            ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
            simpleFillSymbol.Color = color;

            //对元素应用符号
            (element as IFillShapeElement).Symbol = simpleFillSymbol;

            return element;

        }

    }
}
