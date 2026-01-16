using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace MultiPatchExamples.GeometryFactory
{
    public class ElementCreater
    {
        public static IElement Construct_PolylineElement(IGeometry geometry, IColor color, esriSimple3DLineStyle style=esriSimple3DLineStyle.esriS3DLSTube, double width=0.1)
        {
            ISimpleLine3DSymbol simpleLine3DSymbol = new SimpleLine3DSymbolClass();
            simpleLine3DSymbol.Style = style;
            simpleLine3DSymbol.ResolutionQuality = 1;

            ILineSymbol lineSymbol = simpleLine3DSymbol as ILineSymbol;
            lineSymbol.Color = color;
            lineSymbol.Width = width;

            ILine3DPlacement line3DPlacement = lineSymbol as ILine3DPlacement;
            line3DPlacement.Units = ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits;

            ILineElement lineElement = new LineElementClass();
            lineElement.Symbol = lineSymbol;

            IElement element = lineElement as IElement;
            element.Geometry = geometry;

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

            //ISimpleLine3DSymbol simpleLine3DSymbol = new SimpleLine3DSymbolClass();
            //simpleLine3DSymbol.Style = esriSimple3DLineStyle.esriS3DLSTube;
            //simpleLine3DSymbol.ResolutionQuality =1;

            //ILineSymbol lineSymbol = simpleLine3DSymbol as ILineSymbol;
            //lineSymbol.Color = ColorCreater.Get_RgbColor(0, 0, 0);
            //lineSymbol.Width = 1;

            //ILine3DPlacement line3DPlacement = lineSymbol as ILine3DPlacement;
            //line3DPlacement.Units = ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits;


            //simpleFillSymbol.Outline = simpleLine3DSymbol as ILineSymbol;

       

            //对元素应用符号
            (element as IFillShapeElement).Symbol = simpleFillSymbol;

            return element;
 
        }
    }
}


            ////基于几何创建元素
            //IElement element = new MultiPatchElementClass();
            //element.Geometry = geometry;


            ////基于颜色创建填充符号
            //ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
            //simpleFillSymbol.Color = color;



            //ISimpleLine3DSymbol simpleLine3DSymbol = new SimpleLine3DSymbolClass();
            //simpleLine3DSymbol.Style = esriSimple3DLineStyle.esriS3DLSTube;
            //simpleLine3DSymbol.ResolutionQuality =1;

            //ILineSymbol lineSymbol = simpleLine3DSymbol as ILineSymbol;
            //lineSymbol.Color = ColorCreater.Get_RgbColor(0, 0, 0);
            //lineSymbol.Width = 1;

            //ILine3DPlacement line3DPlacement = lineSymbol as ILine3DPlacement;
            //line3DPlacement.Units = ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits;


            //simpleFillSymbol.Outline = simpleLine3DSymbol as ILineSymbol;

            ////对元素应用符号
            //(element as IFillShapeElement).Symbol = simpleFillSymbol;

            //return element;