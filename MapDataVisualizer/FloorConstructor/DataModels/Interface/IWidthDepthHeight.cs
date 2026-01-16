using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloorConstructor.DataModels.Interfaces
{
   public interface IWidthDepthThickness
    {

       double Width { get; set; }//宽度
       double Depth { get; set; }//深度
       double Thickness{ get; set; }//厚度
    }
}
