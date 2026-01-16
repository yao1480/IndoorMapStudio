using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DXFReader.DataModel
{
    public class TextClass
    {
        public string Content { get; set; }//文本内容
        public string LayerName { get; set; }//图层名
        public Point BasePoint { get; set; }//文字基点

        public TextClass()
        {
            this.BasePoint = new Point();
        }

    }
}
