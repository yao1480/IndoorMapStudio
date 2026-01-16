using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels.Basic;

namespace BCE.DataModels.Interface
{
    public interface ILengthWidthHeight
    {
        double Length { get; set; }

        double Width { get; set; }

        double Height { get; set; }
    }
}
