using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Display;

namespace FloorConstructor.DataModels.Interfaces
{
    public interface IRenderedColor
    {
        IColor Color { get; set; }
    }
}
