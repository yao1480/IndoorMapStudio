using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DXFReader.DataModel
{
    public class MLineClass 
    {
        public List<LineClass> LineCollection{get;set;}
        public string LayerName { get; set; }
     

        #region 构造器
        public MLineClass()
        {
            LineCollection = new List<LineClass>();
        }


        public MLineClass(List<LineClass> lineCollection, string layerName = null)
        {
            this.LineCollection = lineCollection;
            this.LayerName = layerName;
        }
        #endregion


    }
}
