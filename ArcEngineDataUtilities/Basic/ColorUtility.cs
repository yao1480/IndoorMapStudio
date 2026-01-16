using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Display;

namespace ArcEngineDataUtilities.Basic
{
    public class ColorUtility
    {
        public static IColor Get_RgbColor(int r, int g, int b, byte transparency = 255)
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = r;
            rgbColor.Green = g;
            rgbColor.Blue = b;

            rgbColor.Transparency = transparency;

            return rgbColor;
        }


        public static IHsvColor Get_HSVColor(int hue, int saturation, int value, byte transparency = 255)
        {
            IHsvColor hsv;
            hsv = new HsvColorClass()
            {
                Hue = hue,
                Saturation = saturation,
                Value = value,

                Transparency=transparency
                
            };
            return hsv;
        }
    }
}
