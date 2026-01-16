using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace ArcObjectsUtilities
{
   public class Draw3DUtility
    {
        //绘制元素
        public static void Draw_Elements(IGraphicsContainer3D graphicsContainer3D, List<IElement> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                graphicsContainer3D.AddElement(elements[i]);
            }
        }

        //绘制外轮廓线
        public static void DrwaMultipatchElementOutlines(IGraphicsContainer3D graphicsContainer3D, List<IElement> elements)
        {
            //const esriSimple3DLineStyle OutlineStyle = esriSimple3DLineStyle.esriS3DLSTube;
            //const double OutlineWidth = 0.1;
            IColor rgbColor = ColorUtility.Get_RgbColor(0, 0, 0);

            for (int i = 0; i < elements.Count; i++)
            {
                IGeometryCollection geometryCollections = MultiPatch_GeometryCollection(elements[i].Geometry);

                for (int j = 0; j < geometryCollections.GeometryCount; j++)
                {
                    graphicsContainer3D.AddElement(ElementUtility.Construct_LineElement(geometryCollections.get_Geometry(j), rgbColor));
                }
            }
        }

        #region 内部方法
        private static IGeometryCollection MultiPatch_GeometryCollection(IGeometry multiPatchGeometry)
        {
            IGeometryCollection outlineGeometryCollection = new GeometryBagClass();

            IGeometryCollection multiPatchGeometryCollection = multiPatchGeometry as IGeometryCollection;

            for (int i = 0; i < multiPatchGeometryCollection.GeometryCount; i++)
            {
                IGeometry geometry = multiPatchGeometryCollection.get_Geometry(i);

                switch (geometry.GeometryType)
                {
                    case (esriGeometryType.esriGeometryTriangleStrip):
                        outlineGeometryCollection.AddGeometryCollection(GeometryUtility.TriangleStrip_GeometryCollection(geometry));
                        break;

                    case (esriGeometryType.esriGeometryTriangleFan):
                        outlineGeometryCollection.AddGeometryCollection(GeometryUtility.TriangleFan_GeometryCollection(geometry));
                        break;

                    case (esriGeometryType.esriGeometryTriangles):
                        outlineGeometryCollection.AddGeometryCollection(GeometryUtility.Triangle_GeometryCollection(geometry));
                        break;

                    case (esriGeometryType.esriGeometryRing):
                        outlineGeometryCollection.AddGeometry(GeometryUtility.Ring_GeometryCollection(geometry));
                        break;

                    default:
                        throw new Exception("Unhandled Geometry Type. " + geometry.GeometryType);
                }
            }

            return outlineGeometryCollection;
        }
        #endregion
    }
}
