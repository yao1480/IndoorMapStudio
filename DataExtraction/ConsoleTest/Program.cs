using System;
using System.Collections.Generic;
using DXFReader.Reader;
using MathExtension.Geometry;
using DXFReader.DataModel;
using BCE.DataModels.Basic;
using BCE.DataModels;
using BCE.Helpers;
using BCE;
using MathNet.Spatial.Euclidean;
using System.Linq;

namespace ConsoleTest
{

    public class Program
    {
        [STAThread]
        static void Main()
        {
            FindCpTest();

            Console.ReadKey();
        }



        //DxdReader读取效果测试
        private static void ReaderTest()
        {

 


            TimeSpan timeSpan_Read = new TimeSpan();
            TimeSpan timeSpan_Output = new TimeSpan();

            DateTime t1 = DateTime.Now;

            string path = @"C:\Users\silence\Desktop\Intergrated03033\数据\5号楼标准层平面图 - 副本.dxf";
            DxfReader dxfReader = new DxfReader(path, true);



            //读取DXF
            timeSpan_Read = DateTime.Now - t1;

            DateTime t2 = DateTime.Now;


            #region 输出DXF数据
            #region 输出图层名集合
            Console.WriteLine("图层名集合：\n");
            foreach (var item in dxfReader.LayerNames)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("\n-----------------------------------------------------------------");
            #endregion

            #region 输出Line集合
            if (dxfReader.Lines.Count > 0)
            {
                Console.WriteLine("Line集合：\n");
                for (int i = 0; i < dxfReader.Lines.Count; i++)
                {
                    Console.WriteLine("Line" + i + ":");
                    Console.WriteLine("LayerName: " + dxfReader.Lines[i].LayerName);
                    Console.WriteLine(string.Format("StartPoint:\tX = {0}\tY = {1}\tZ = {2}", dxfReader.Lines[i].StartPoint.X, dxfReader.Lines[i].StartPoint.Y, dxfReader.Lines[i].StartPoint.Z));
                    Console.WriteLine(string.Format("EndPoint:\tX = {0}\tY = {1}\tZ = {2}\n", dxfReader.Lines[i].EndPoint.X, dxfReader.Lines[i].EndPoint.Y, dxfReader.Lines[i].StartPoint.Z));
                }
                Console.WriteLine("-----------------------------------------------------------------");
            }
            #endregion

            #region 输出Circle集合
            if (dxfReader.Circles.Count > 0)
            {
                Console.WriteLine("Circle集合：\n");
                for (int i = 0; i < dxfReader.Circles.Count; i++)
                {
                    Console.WriteLine("Circle" + i + ":");
                    Console.WriteLine("LayerName: " + dxfReader.Circles[i].LayerName);
                    Console.WriteLine(string.Format("Centre:\tX = {0}\tY = {1}\tZ = {2}", dxfReader.Circles[i].Centre.X, dxfReader.Circles[i].Centre.Y, dxfReader.Circles[i].Centre.Z));
                    Console.WriteLine(string.Format("Radius:\tR = {0}\n", dxfReader.Circles[i].Radius));
                }
                Console.WriteLine("\n-----------------------------------------------------------------");
            }
            #endregion

            #region 输出Arc集合
            if (dxfReader.Arcs.Count > 0)
            {
                Console.WriteLine("Arc集合：\n");
                for (int i = 0; i < dxfReader.Arcs.Count; i++)
                {
                    Console.WriteLine("Arc" + i + ":");
                    Console.WriteLine("LayerName: " + dxfReader.Arcs[i].LayerName);
                    Console.WriteLine(string.Format("Centre:\tX = {0}\tY = {1}\tZ = {2}", dxfReader.Arcs[i].Centre.X, dxfReader.Arcs[i].Centre.Y, dxfReader.Arcs[i].Centre.Z));
                    Console.WriteLine(string.Format("Radius:\tR = {0}", dxfReader.Arcs[i].Radius));
                    Console.WriteLine(string.Format("Angles:\tS = {0}\tE = {1}\n", dxfReader.Arcs[i].StartRadian, dxfReader.Arcs[i].EndRadian));
                }
                Console.WriteLine("\n-----------------------------------------------------------------");
            }
            #endregion

            #region 输出Ellipse集合
            if (dxfReader.Ellipses.Count > 0)
            {
                Console.Write("Ellipse集合 ： \n");
                for (int i = 0; i < dxfReader.Ellipses.Count; i++)
                {
                    Console.WriteLine("Ellipse" + i + ":");
                    Console.WriteLine("LayerName: " + dxfReader.Ellipses[i].LayerName);
                    Console.WriteLine(string.Format("Centre:\tX = {0}\tY = {1}\tZ = {2}", dxfReader.Ellipses[i].Centre.X, dxfReader.Ellipses[i].Centre.Y, dxfReader.Ellipses[i].Centre.Z));
                    Console.WriteLine(string.Format("MRVert:\tX = {0}\tY = {1}\tZ = {2}", dxfReader.Ellipses[i].MajorRadiusVertex.X, dxfReader.Ellipses[i].MajorRadiusVertex.Y, dxfReader.Ellipses[i].MajorRadiusVertex.Z));
                    Console.WriteLine(string.Format("Radius:\tA = {0}\tB = {1}\tRatio = {2}", dxfReader.Ellipses[i].MajorRadius, dxfReader.Ellipses[i].MinorRadius, dxfReader.Ellipses[i].RadiusRatio));
                    Console.WriteLine(string.Format("Angles:\tS = {0}\tE = {1}\n", dxfReader.Ellipses[i].StartRadian, dxfReader.Ellipses[i].EndRadian));
                }
                Console.WriteLine("\n-----------------------------------------------------------------");
            }
            #endregion

            #region 输出LwPolyline集合
            if (dxfReader.LwPolylines.Count > 0)
            {
                Console.Write("LwPolyline集合 ： \n");
                for (int i = 0; i < dxfReader.LwPolylines.Count; i++)
                {
                    Console.WriteLine("LwPolyLine" + i + ":");
                    Console.WriteLine("LayerName: " + dxfReader.LwPolylines[i].LayerName);
                    Console.WriteLine(string.Format("CloseFlag : \t{0}", dxfReader.LwPolylines[i].CloseFlag));
                    Console.WriteLine(string.Format("PointCount: \t{0}", dxfReader.LwPolylines[i].PointCollection.Count));
                    Console.WriteLine(string.Format("LineCount: \t{0}", dxfReader.LwPolylines[i].LineCollection.Count));
                    Console.WriteLine("LineCollection:");
                    for (int j = 0; j < dxfReader.LwPolylines[i].LineCollection.Count; j++)
                    {
                        Console.WriteLine("Line" + j + ":");
                        Console.WriteLine("LayerName: " + dxfReader.LwPolylines[i].LineCollection[j].LayerName);
                        Console.WriteLine(string.Format("StartPoint:\tX = {0}\tY = {1}\tZ = {2}", dxfReader.LwPolylines[i].LineCollection[j].StartPoint.X, dxfReader.LwPolylines[i].LineCollection[j].StartPoint.Y, dxfReader.LwPolylines[i].LineCollection[j].StartPoint.Z));
                        Console.WriteLine(string.Format("EndPoint:\tX = {0}\tY = {1}\tZ = {2}\n\n", dxfReader.LwPolylines[i].LineCollection[j].EndPoint.X, dxfReader.LwPolylines[i].LineCollection[j].EndPoint.Y, dxfReader.LwPolylines[i].LineCollection[j].StartPoint.Z));
                    }
                }
                Console.WriteLine("\n-----------------------------------------------------------------");
            }
            #endregion

            #region 输出Blocks集合
            if (dxfReader.Blocks.Count > 0)
            {
                Console.WriteLine("Blocks：\n");
                for (int i = 0; i < dxfReader.Blocks.Count; i++)
                {

                    BlockClass pBlock = dxfReader.Blocks[i];

                    Console.WriteLine(string.Format("Block {0}", i));
                    Console.WriteLine(string.Format("Name {0}", pBlock.Name));
                    Console.WriteLine(string.Format("LayerName {0}", pBlock.LayerName));
                    Console.WriteLine(string.Format("BasePoint X: {0}\tY: {1}\n", pBlock.BasePoint.X, pBlock.BasePoint.Y));
                }

                Console.WriteLine("\n-----------------------------------------------------------------");
            }
            #endregion

            #region 输出Inserts集合
            if (dxfReader.Inserts.Count > 0)
            {
                Console.WriteLine("Inserts：\n");
                for (int i = 0; i < dxfReader.Inserts.Count; i++)
                {

                    InsertClass pInsert = dxfReader.Inserts[i];

                    Console.WriteLine(string.Format("Insert {0}", i));
                    Console.WriteLine(string.Format("BlockName {0}", pInsert.BlockName));
                    Console.WriteLine(string.Format("LayerName {0}", pInsert.LayerName));
                    Console.WriteLine(string.Format("InstertPoint X: {0}\tY: {1}", pInsert.InsertPoint.X, pInsert.InsertPoint.Y));
                    Console.WriteLine(string.Format("RotationAngle {0}", pInsert.RotationAngle));
                    Console.WriteLine(string.Format("ScaleX {0}", pInsert.ScaleX));
                    Console.WriteLine(string.Format("ScaleX {0}\n", pInsert.ScaleY));
                }

                Console.WriteLine("\n-----------------------------------------------------------------");
            }
            #endregion

            #region 输出注记集合
            if (dxfReader.Texts.Count > 0)
            {
                Console.WriteLine("M/Text集合：\n");
                for (int i = 0; i < dxfReader.Texts.Count; i++)
                {
                    Console.WriteLine("M/Text" + i + ":");
                    Console.WriteLine("LayerName: " + dxfReader.Texts[i].LayerName);
                    Console.WriteLine(string.Format("BasePoint:\tX = {0}\tY = {1}\tZ = {2}", dxfReader.Texts[i].BasePoint.X, dxfReader.Texts[i].BasePoint.Y, dxfReader.Texts[i].BasePoint.Z));
                    Console.WriteLine(string.Format("Content： {0}", dxfReader.Texts[i].Content));
                }
                Console.WriteLine("-----------------------------------------------------------------");
            }
            #endregion





            timeSpan_Output = DateTime.Now - t2;
            Console.WriteLine("读取完毕!");
            Console.WriteLine(string.Format("读取用时 ： {0} ms\t输出用时 ： {1} ms", timeSpan_Read.TotalMilliseconds, timeSpan_Output.TotalMilliseconds));
            Console.WriteLine("\nEND");
            #endregion
        }

        //识别建筑图元测试
        public static FloorPlan FindCpTest()
        {
            #region 图纸初始化配置
            string dxfPath = @"C:\Users\silence\Desktop\IndoorMapStudio\Temp\5号楼标准层平面图 - 副本.dxf";
            ParametersConfiguration config = new ParametersConfiguration(dxfPath);

            FloorPlan floorPlan = new FloorPlan(ref config);

            DxfReader dxfReader = new DxfReader(config.DXF_Path, true);


            //图层映射配置
            Mapping_LayerNames mapping_LayerNames = new Mapping_LayerNames();
            mapping_LayerNames.Wall.Add("WALL");//墙元素图层名集合
            mapping_LayerNames.Balcony.Add("BALCONY");//阳台元素图层名集合
            //mapping_LayerNames.Balcony.Add("YANGTAI");
            mapping_LayerNames.Window.Add("WINDOW");//窗元素图层名集合
            mapping_LayerNames.Door.Add("DOOR");//门元素图层名集合
            mapping_LayerNames.Elevator.Add("Elevator");//电梯元素图层名集合
            mapping_LayerNames.Text.Add("PUB_TEXT");//注记元素图层名集合
            mapping_LayerNames.Stairs.Add("STAIR");//楼体线图层名集合
            floorPlan.Configuration.LayerMapping = mapping_LayerNames;
            #endregion


            InfoSegementation.SegementInfo(ref floorPlan, ref dxfReader);
            FloorFrameModeler.Model(ref floorPlan);


            //test
            List<SemanticPolygon> sps = (from p in floorPlan.PResult.SPolygons
                                         where p.PType != PolygonType.Floor &&
                                         p.PType != PolygonType.Wall
                                         && p.PType != PolygonType.Balustrade
                                         select p).ToList<SemanticPolygon>();

            foreach (var item in sps)
            {
                try
                {
                    Console.WriteLine(string.Format("FP:  {0}", item.FunctionRegionPoint.ToString()));
                }
                catch (Exception)
                {
                    throw;
                }

                continue;
            }

            //绘制提取结果
            DrawCU.DrawFloor(floorPlan);

            return floorPlan;
        }




        public static void test()
        {
            //读取图纸
            string path = @"C:\Users\silence\Desktop\TestDXF - 副本.dxf";
            DxfReader dxfReader = new DxfReader(path, true);

            //提取多边形
            List<LineClass> lines = dxfReader.Lines;



            BCE.BCExtractors.ERings er = new BCE.BCExtractors.ERings(lines, 1d);

            if (er.Rings.Count > 0)
            {
                SemanticPolygon sp = er.Rings[0];
                Polygon p = new Polygon(sp.Points);
                Point containedPoint = MathExtension.Geometry.GeometryHelper.Calc2D_PointInPolygon(p);

                Console.WriteLine(containedPoint.ToString());
            }
        }
    }
}
