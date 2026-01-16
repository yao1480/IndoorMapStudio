using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;

namespace MultiPatchExamples.GeometryFactory
{
   public class ColorCreater
    {
       public static IColor Get_RgbColor(int r,int g,int b,byte transparency=255)
       {
           IRgbColor rgbColor = new RgbColorClass();
           rgbColor.Red = r;
           rgbColor.Green = g;
           rgbColor.Blue = b;

           rgbColor.Transparency = transparency;

           return rgbColor;
       }
    }
}
