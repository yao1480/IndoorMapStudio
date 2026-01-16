using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DXFReader.DataModel
{
  public  class EllipseClass:MathExtension.Geometry.Ellipse
    {

     
        public string LayerName { get; set; }
    

        #region 构造器
        public EllipseClass()
        {
            base.Centre = new Point();
            base.MajorRadiusVertex = new Point();
        }
        #endregion
 
    }
}
