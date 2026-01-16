using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DXFReader.DataModel
{
    public class CircleClass : MathExtension.Geometry.Circle
    {
        public string LayerName { get; set; }


        #region 构造器
        public CircleClass()
        {
            base.Centre = new Point();
            LayerName = null;
        }



        public CircleClass(Point centre, double radius, string layerName = null)
        {
            base.Centre = centre;
            base.Radius = radius;
            this.LayerName = layerName;
        }
        #endregion




    }
}
