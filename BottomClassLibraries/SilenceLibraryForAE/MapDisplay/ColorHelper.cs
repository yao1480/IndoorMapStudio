using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;

namespace SilenceLibraryForAE.MapDisplay
{
    public class ColorHelper
    {
        public static IRgbColor Get_RGBColor(int red, int green, int blue)
        {
            IRgbColor rgb;
            rgb = new RgbColorClass()
            {
                Red = red,
                Green = green,
                Blue = blue,

                Transparency=1
            };

            return rgb;
        }

        public static IHsvColor Get_HSVColor(int hue, int saturation, int value)
        {
            IHsvColor hsv;
            hsv = new HsvColorClass()
            {
                Hue = hue,
                Saturation = saturation,
                Value = value
            };
            return hsv;
        }
    }
}

