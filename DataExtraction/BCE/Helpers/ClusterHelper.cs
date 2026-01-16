using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace BCE.Helpers
{
    public class ClusterHelper
    {
        /// <summary>
        /// 对线段集按照与X轴夹角聚类
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<List<LineClass>> Lines_BySlope(ref List<LineClass> lines)
        {
            List<List<LineClass>> list = new List<List<LineClass>>();

            for (int i = 0; i < lines.Count; i++)
            {
                LineClass pLi = lines[i];
                if (pLi.Used == false)
                {
                    pLi.Used = true;
                    List<LineClass> item_Lines = new List<LineClass>();
                    item_Lines.Add(pLi);
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        LineClass pLj = lines[j];
                        if (pLj.Used == false && GeometryHelper.Parallel(pLj, pLi))
                        {
                            item_Lines.Add(pLj);
                            pLj.Used = true;
                        }
                    }

                    list.Add(item_Lines);
                }
            }

            //将所有line.Used属性重置为false
            for (int i = 0; i < lines.Count; i++)
                lines[i].Used = false;

            return list;
        }

        /// <summary>
        /// 通过图层和线段最小长度筛选线段
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="layerNames"></param>
        /// <param name="minLength"></param>
        /// <returns></returns>
        public static List<LineClass> Lines_ByLayerAndMinLength(ref List<LineClass> lines, List<string> layerNames, double minLength)
        {
            return lines.FindAll(new Predicate<LineClass>((pLine) =>
            {
                if (pLine.Length >= minLength && layerNames.IndexOf(pLine.LayerName) >= 0)
                    return true;
                else
                    return false;
            }));
        }

        /// <summary>
        /// 筛选指定图层的插入体
        /// </summary>
        /// <param name="pInserts"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static List<InsertClass> Inserts_ByLayer(ref List<InsertClass> pInserts, List<string> layerNames)
        {
            return pInserts.FindAll(new Predicate<InsertClass>((insert) =>
            {
                if (layerNames.IndexOf(insert.LayerName) >= 0)
                    return true;
                else
                    return false;
            }));
        }

        /// <summary>
        /// 筛选插入体涉及到的块
        /// </summary>
        /// <param name="pSourceBlocks"></param>
        /// <param name="pInserts"></param>
        /// <returns></returns>
        public static List<BlockClass> Blocks_ByRelatedInserts(ref List<BlockClass> pSourceBlocks, ref List<InsertClass> pInserts)
        {
            //收集插入体涉及到的图块名
            List<string> blockNames = new List<string>();
            foreach (var item in pInserts)
            {
                blockNames.Add(item.BlockName);
            }

            //去除重复图块名
            IEnumerable<string> blockNames_distincted = blockNames.Distinct<string>();

            return pSourceBlocks.FindAll(new Predicate<BlockClass>(
                  (block) =>
                  {
                      if (blockNames_distincted.Contains<string>(block.Name))
                          return true;
                      return false;
                  }));
        }

        /// <summary>
        /// 筛选指定图层的注记
        /// </summary>
        /// <param name="layerNames"></param>
        /// <param name="pTextClasss"></param>
        /// <returns></returns>
        public static List<TextClass> GetTexts_ByLayer(ref List<TextClass> pTextClasss, List<string> layerNames)
        {
            //1.提取指定图层的图元
            return pTextClasss.FindAll(new Predicate<TextClass>((pTC) =>
            {
                if (layerNames.IndexOf(pTC.LayerName) >= 0)
                    return true;
                else
                    return false;
            }));
        }


        /// <summary>
        /// 筛选出长度小于指定值的线段集
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static List<LineClass> GetLines_ByLength(List<LineClass> lines, double maxLength)
        {
            return lines.FindAll(new Predicate<Line>((pLine) =>
            {
                if (pLine.Length <= maxLength)
                    return true;
                return false;
            }));
        }

        /// <summary>
        /// 筛选指定类型的语义多边形集合
        /// </summary>
        /// <param name="semanticPolygons"></param>
        /// <param name="polygonType"></param>
        /// <returns></returns>
        public static List<SemanticPolygon> GetSPolygons_ByPolygonType(List<SemanticPolygon> semanticPolygons, PolygonType polygonType)
        {

            return semanticPolygons.FindAll(new Predicate<SemanticPolygon>((p) =>
            {
                if (p.PType == polygonType)
                    return true;
                return false;
            }));
        }


        //public static void Cluster_SoSmallInserts(ref List<InsertClass> inserts, ref List<BlockClass> blocks)
        //{
        //    BlockClass block = null;
        //    IEnumerable<Point> points = null;//块的点集
        //    foreach (var insert in inserts)
        //    {
        //        block = BIHelper.Find_Block(insert.BlockName, blocks);
        //        if (block == null)
        //            throw new ArgumentException(string.Format("未找到插入体对应的图块!\n图块名: {0}", insert.BlockName));
        //        points = BIHelper.Get_BlockDistinctedPoints(block);
        //        double[] mostXY = BIHelper.Find_XYMostValue(points);
        //    }
        //}
    }
}
