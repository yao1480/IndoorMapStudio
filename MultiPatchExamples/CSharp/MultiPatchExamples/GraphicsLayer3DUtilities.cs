// Copyright 2011 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at http://resourcesbeta.arcgis.com/en/help/arcobjects-net/usagerestrictions.htm
// 

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Analyst3D;

namespace MultiPatchExamples
{
    public static class GraphicsLayer3DUtilities
    {
        public static IGraphicsContainer3D ConstructGraphicsLayer3D(string name)
        {
            IGraphicsContainer3D graphicsContainer3D = new GraphicsLayer3DClass();
            
            ILayer layer = graphicsContainer3D as ILayer;
            layer.Name = name;

            return graphicsContainer3D;
        }

        public static void DisableLighting(IGraphicsContainer3D graphicsContainer3D)
        {
            I3DProperties properties3D = new Basic3DPropertiesClass();
            properties3D.Illuminate = false;

            ILayerExtensions layerExtensions = graphicsContainer3D as ILayerExtensions;
            layerExtensions.AddExtension(properties3D);

            properties3D.Apply3DProperties(graphicsContainer3D);
        }

        public static void AddAxisToGraphicsLayer3D(IGraphicsContainer3D graphicsContainer3D, IGeometry geometry, IColor color, esriSimple3DLineStyle style, double width)
        {
            graphicsContainer3D.AddElement(ElementUtilities.ConstructPolylineElement(geometry, color, style, width));
        }

        public static void AddOutlineToGraphicsLayer3D(IGraphicsContainer3D graphicsContainer3D, IGeometryCollection geometryCollection, IColor color, esriSimple3DLineStyle style, double width)
        {
            for (int i = 0; i < geometryCollection.GeometryCount; i++)
            {
                IGeometry geometry = geometryCollection.get_Geometry(i);

                graphicsContainer3D.AddElement(ElementUtilities.ConstructPolylineElement(geometry, color, style, width));
            }
        }

        public static void AddMultiPatchToGraphicsLayer3D(IGraphicsContainer3D graphicsContainer3D, IGeometry geometry, IColor color)
        {
            graphicsContainer3D.AddElement(ElementUtilities.ConstructMultiPatchElement(geometry, color));
        }
   }
}