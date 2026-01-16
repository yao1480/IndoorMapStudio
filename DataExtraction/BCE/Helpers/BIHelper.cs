using DXFReader.DataModel;
using MathExtension.Geometry;
using MathExtension.Matrix;
using MathExtension.Vector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader;
using BCE.DataModels.Basic;

namespace BCE.Helpers
{

    /// <summary>
    /// 提取插入体，该类可提取以插入体形式存在的门窗
    /// </summary>
    public class BIHelper
    {

        public static BlockClass Find_Block(string blockName, List<BlockClass> pBlocks)
        {
            return pBlocks.Find(new Predicate<BlockClass>((pBlock) =>
            {
                if (pBlock.Name == blockName)
                    return true;
                return false;
            }));
        }

        public static IEnumerable<Point> Get_BlockDistinctedPoints(BlockClass pBlock)
        {
            List<Point> points = new List<Point>();
            foreach (LineClass line in pBlock.Entity.Lines)
            {
                points.Add(line.StartPoint);
                points.Add(line.EndPoint);
            }


            return points.Distinct<Point>();
        }

        //寻找X Y最值（最值存储顺序：Xmin,Xmax,Ymin,Ymax）
        public static double[] Find_XYMostValue(IEnumerable<Point> points)
        {
            Point fp = points.ElementAt<Point>(0);
            double x_min = fp.X;
            double x_max = fp.X;
            double y_min = fp.Y;
            double y_max = fp.Y;

            foreach (Point point in points)
            {
                if (point.X < x_min)
                    x_min = point.X;
                if (point.X > x_max)
                    x_max = point.X;
                if (point.Y < y_min)
                    y_min = point.Y;
                if (point.Y > y_max)
                    y_max = point.Y;
            }

            double[] mostValues = new double[4];
            mostValues[0] = x_min;
            mostValues[1] = x_max;
            mostValues[2] = y_min;
            mostValues[3] = y_max;


            return mostValues;
        }

        //基于XY最值构造四至点
        public static Point[] Get_FourBoundPoints(ref double[] xy_MValues)
        {
            if (xy_MValues.Count() != 4)
                return null;
            else
            {
                Point[] points = new Point[4];

                points[0] = new Point(xy_MValues[0], xy_MValues[2]);//左下角点
                points[1] = new Point(xy_MValues[0], xy_MValues[3]);//左上角点
                points[2] = new Point(xy_MValues[1], xy_MValues[3]);//右上角点
                points[3] = new Point(xy_MValues[1], xy_MValues[2]);//右下角点

                return points;
            }


        }
    }
}
