using BCE.DataModels.Basic;
using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;
using BCE.DataModels.Interface;

namespace BCE.DataModels
{
   public class Door:IDoorWindow
    {
       public Door()
       { 
       }


       public MCType Type { get; set; }


       public double Length { get; set; }


       public double Width { get; set; }


       public double Height { get; set; }


       public MathExtension.Geometry.Point InsertPoint { get; set; }


       public double AngleToXAxis { get; set; }
    }
}
