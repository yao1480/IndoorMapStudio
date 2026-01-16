// Copyright 2011 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at http://resourcesbeta.arcgis.com/en/help/arcobjects-net/usagerestrictions.htm
// 

using ESRI.ArcGIS.Display;

namespace MultiPatchExamples
{
    public enum TransparencyType
    {
        Transparent = 0,
        Opaque = 255
    }

    public static class ColorUtilities
    {
        private static TransparencyType _transparency = TransparencyType.Opaque;

        public static IColor GetColor(int red, int green, int blue)
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = red;
            rgbColor.Green = green;
            rgbColor.Blue = blue;

            IColor color = rgbColor as IColor;
            color.Transparency = (byte)_transparency;

            return color;
        }
    }
}