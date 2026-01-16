using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExtension.Vector;

namespace DXFReader.DataModel
{
    public class PointStruct : MathExtension.Geometry.Point
    {
        const double precision = 0.01f;//精度，当两点距离<=此值时，被视为同一点
        public string LayerName { get; set; }




        #region 构造器
        public PointStruct()
        {
            this.LayerName = null;
        }

        public PointStruct(double x, double y, double z = 0, string layerName = null)
        {
            base.X = x;
            base.Y = y;
            base.Z = z;
            this.LayerName = layerName;
        }
        #endregion
    }
}
