using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels.Basic;
using DXFReader.DataModel;

namespace BCE.DataModels.Interface
{
    public interface IDoorWindow :ILengthWidthHeight,ILocatorData
    {
         MCType Type { get; set; }
    }
}
