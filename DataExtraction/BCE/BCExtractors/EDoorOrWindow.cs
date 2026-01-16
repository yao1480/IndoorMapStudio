using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels;
using BCE.DataModels.Basic;
using BCE.DataModels.Interface;
using BCE.Helpers;
using DXFReader.DataModel;
using MathExtension.Geometry;
using MathExtension.Matrix;

namespace BCE.BCExtractors
{


   /// <summary>
   /// 该类用于提取图块形式的门窗参数
   /// </summary>
    public abstract class EDoorOrWindow
    {
        static double threadValue_sameLength = 0.001d;//等长阈值
        static double threadValue_MaxWallWidth = 300d;//墙端最大长度
        static double threadValue_MinLength_ForDirL = 400d;//有效方向线的最小长度;方向线小于此值的插图体视为噪声插入体


        #region 填补门窗洞+提取门窗洞参数+门窗类型参数
        /// <summary>
        /// 处理与门窗相关的墙线，并提取门窗参数
        /// </summary>
        /// <param name="doorOrWindowFlag"></param>
        /// <param name="data"></param>
        /// <param name="result"></param>
        public static void Get_Window_TranformWindow(int doorOrWindowFlag, ref FloorPlan floorPlan)
        {
            //*******2017.07.01修改
            Data data = floorPlan.PData;
            Result result = floorPlan.PResult;
            List<BlockClass> blocks;
            List<InsertClass> inserts;
            LineType lineType = LineType.Unknown;

            #region 变量初始化及图块处理
            if (doorOrWindowFlag == 0)
            {
                blocks = data.Blocks_Door;
                inserts = data.Inserts_Door;
                lineType = LineType.DoorLine;
                if (result.Doors == null) result.Doors = new List<Door>();
            }
            else if (doorOrWindowFlag == 1)
            {
                blocks = data.Blocks_Window;
                inserts = data.Inserts_Window;
                lineType = LineType.WindowLine;
                if (result.Windows == null) result.Windows = new List<Window>();
            }
            else
                throw new ArgumentException("参数无效：" + "请将doorOrWindowFlag指定为 0 或 1");


            //处理并生成与门或窗相关的图块信息
            process_blockAndInserts(doorOrWindowFlag, ref blocks, ref result.Dic_MC_BlockInfo);
            #endregion

            #region 恢复墙体完整性+提取窗洞参数
            for (int i = 0; i < inserts.Count; i++)
            {
                InsertClass insert = inserts[i];

                MCBlockInfo mcbi=null;
                Point[] transformedPoints=null;
                LineClass dirL_initial=null;
                LineClass dirL_real = null;
                bool directionFixed;
                IDoorWindow pDoorWindow = null;


                #region step1: 获取方向线的基本信息（包括确定方向线的指向）
                getBasicInfo_DirL(ref  doorOrWindowFlag, ref data, ref result, ref  insert, out  mcbi, out  transformedPoints, out dirL_initial, out directionFixed);

     
                if (dirL_initial.Length < threadValue_MinLength_ForDirL)
                    continue;


                #endregion

                #region step2: 在方向线起终点两端寻找垂直且距离最近的两条墙线

                //Point p = new Point();
                //p.X = -78591.0427;
                //p.Y = 95416.8325;
                //p.Z = 1;
                //if (dirL_initial.StartPoint.Equals(p))
                //{
                //    int aasasa = 0;
                //    aasasa++;
                //}


                LineClass[] wallLines = MCHelper.Find_WallLines(dirL_initial, ref data.WallLines);//寻找到的墙线
                LineClass[] wallEnds = null;//真正的墙端
                LineClass wl_1 = wallLines[0];
                LineClass wl_2 = wallLines[1];
                wl_1.ShouldMoved = true;
                wl_2.ShouldMoved = true;

    
                if (wl_1.Length > wl_2.Length)
                {
                    LineClass tempLine = wl_1;
                    wl_1 = wl_2;
                    wl_2 = tempLine;
                }
                #endregion

                #region step3: 墙线裁剪分析(裁剪过长墙端+生成新墙线并标识类型+获取真实墙端对)
                if (wl_1.Length <= threadValue_MaxWallWidth + threadValue_sameLength)
                {
                    if (Math.Abs(wl_2.Length - wl_1.Length) <= threadValue_sameLength)
                    {
                        #region 等长墙端，无需裁剪墙线，直接基于墙端生成新墙线
                        if (GeometryHelper.Parallel(
                            new Line(wl_1.StartPoint, wl_2.StartPoint),
                            new Line(wl_1.EndPoint, wl_2.EndPoint)))
                        {
                 
                            data.NewWallines.Add(new LineClass(wl_1.StartPoint, wl_2.StartPoint, null, lineType));
                            data.NewWallines.Add(new LineClass(wl_1.EndPoint, wl_2.EndPoint, null, lineType));
                        }
                        else
                        {
                
                            data.NewWallines.Add(new LineClass(wl_1.StartPoint, wl_2.EndPoint, null, lineType));
                            data.NewWallines.Add(new LineClass(wl_1.EndPoint, wl_2.StartPoint, null, lineType));
                        }

               
                        wallEnds = wallLines;
                        #endregion
                    }
                    else
                    {
                        #region 非等长墙端，基于较短墙端对另一墙端进行裁剪
                        Line sl = GeometryHelper.Generate_ParallelLine(wl_1.StartPoint, dirL_initial);
                        Line el = GeometryHelper.Generate_ParallelLine(wl_1.EndPoint, dirL_initial);

                        Point s_point, e_point;
                        GeometryHelper.Calc2D_Intersection(sl, wl_2, out s_point);
                        GeometryHelper.Calc2D_Intersection(el, wl_2, out e_point);

         
                        data.NewWallines.Add(new LineClass(wl_1.StartPoint, s_point, null, lineType));
                        data.NewWallines.Add(new LineClass(wl_1.EndPoint, e_point, null, lineType));

   
                        LineClass[] lines = FloorFrameModeler.Get_TwoLines_ByTwoBreakPoints(s_point, e_point, wl_2);
                        for (int k = 0; k < lines.Length; k++)
                        {
                            if (!lines[k].StartPoint.Equals(lines[k].EndPoint))
                                data.NewWallines.Add(lines[k]);
                        }


                        wallEnds = new LineClass[2];
                        wallEnds[0] = wl_1;
                        wallEnds[1] = new LineClass(s_point, e_point);
                        #endregion
                    }
                }
                else
                {

                    #region 两墙端长度均超过墙宽阈值,以窗符号轮廓宽度为基础在两端裁剪墙线

                    Line e_Line = GeometryHelper.Generate_ParallelLine(transformedPoints[1], dirL_initial);
                    Line s_Line = GeometryHelper.Generate_ParallelLine(transformedPoints[0], dirL_initial);

 
                    Point s_point1, e_point1, s_point2, e_point2;
                    GeometryHelper.Calc2D_Intersection(s_Line, wl_1, out s_point1);
                    GeometryHelper.Calc2D_Intersection(s_Line, wl_2, out s_point2);
                    GeometryHelper.Calc2D_Intersection(e_Line, wl_1, out e_point1);
                    GeometryHelper.Calc2D_Intersection(e_Line, wl_2, out e_point2);


                    data.NewWallines.Add(new LineClass(s_point1, s_point2, null, lineType));//新增墙线类型指定为窗
                    data.NewWallines.Add(new LineClass(e_point1, e_point2, null, lineType));//新增墙线类型指定为窗

                    LineClass[] lines1 = FloorFrameModeler.Get_TwoLines_ByTwoBreakPoints(s_point1, e_point1, wl_1);
                    LineClass[] lines2 = FloorFrameModeler.Get_TwoLines_ByTwoBreakPoints(s_point2, e_point2, wl_2);
                    List<LineClass> lines = new List<LineClass>();
                    lines.AddRange(lines1);
                    lines.AddRange(lines2);
                    for (int k = 0; k < lines.Count; k++)
                    {
                        if (!lines[k].StartPoint.Equals(lines[k].EndPoint))
                            data.NewWallines.Add(lines[k]);
                    }

         
                    wallEnds = new LineClass[2];
                    wallEnds[0] = new LineClass(s_point1, e_point1);
                    wallEnds[1] = new LineClass(s_point2, e_point2);
                    #endregion
                }
                #endregion


                #region step4: 基于墙端对提取门洞或窗洞参数并收集
                if (doorOrWindowFlag == 0)
                {
                    pDoorWindow = new Door();
                    result.Doors.Add(pDoorWindow as Door);
                }
                else
                {
                    pDoorWindow = new Window();
                    result.Windows.Add(pDoorWindow as Window);
                }

                pDoorWindow.Type = mcbi.Type_MC;

                if (pDoorWindow.Type == MCType.M_SingleLeaf || pDoorWindow.Type == MCType.M_SonMather)
                {
                    MathExtension.Vector.Vector v_dirL = dirL_initial.ToVector();
                    MathExtension.Vector.Vector v_leaf = new MathExtension.Vector.Vector(
                                 transformedPoints[2].X - transformedPoints[0].X,
                                 transformedPoints[2].Y - transformedPoints[0].Y,
                                 0);
                    MathExtension.Vector.Vector v_cross = v_dirL.CrossMultiply(v_leaf);

                    if (pDoorWindow.Type == MCType.M_SingleLeaf)
                    {
                        if (v_cross.Z > 0) pDoorWindow.Type = MCType.M_SingleLeaf_CounterClockwise;
                        else pDoorWindow.Type = MCType.M_SingleLeaf_Clockwise;
                    }
                    else
                    {
                        if (v_cross.Z > 0) pDoorWindow.Type = MCType.M_SonMother_CounterClockwise;
                        else pDoorWindow.Type = MCType.M_SonMother_Clockwise;
                    }
                }

                Point midPoint_1 = wallEnds[0].Get_MiddlePoint();
                Point midPoint_2 = wallEnds[1].Get_MiddlePoint();




                pDoorWindow.Length = GeometryHelper.Calc_Distance(midPoint_1, wallEnds[1]);
                pDoorWindow.Width = wallEnds[0].Length;


                if (directionFixed)
                {

                    double distance_1 = GeometryHelper.Calc_Distance(dirL_initial.StartPoint, wallEnds[0]);
                    double distance_2 = GeometryHelper.Calc_Distance(dirL_initial.StartPoint, wallEnds[1]);

                    if (distance_1 < distance_2)
                    {

                        dirL_real = new LineClass(midPoint_1, midPoint_2);
                        pDoorWindow.InsertPoint = dirL_real.StartPoint;
                        pDoorWindow.AngleToXAxis = GeometryHelper.Calc_AngleTo_XAxis(dirL_real);
                    }
                    else
                    {

                        dirL_real = new LineClass(midPoint_2, midPoint_1);
                        pDoorWindow.InsertPoint = dirL_real.StartPoint;
                        pDoorWindow.AngleToXAxis = GeometryHelper.Calc_AngleTo_XAxis(dirL_real);
                    }
                }
                else
                {


                    //确定门洞定位参数
                    if (wl_1.Length <= threadValue_MaxWallWidth + threadValue_sameLength)
                    {

                        dirL_real = new LineClass(midPoint_1, midPoint_2);
                        pDoorWindow.InsertPoint = dirL_real.StartPoint;
                        pDoorWindow.AngleToXAxis = GeometryHelper.Calc_AngleTo_XAxis(dirL_real);
                    }
                    else
                    {
                        if (midPoint_1.X + midPoint_1.Y < midPoint_2.X + midPoint_2.Y)
                        {
                            dirL_real = new LineClass(midPoint_1, midPoint_2);

                            pDoorWindow.InsertPoint = dirL_real.StartPoint;

                            pDoorWindow.AngleToXAxis = GeometryHelper.Calc_AngleTo_XAxis(dirL_real);
                        }
                        else
                        {
                            dirL_real = new LineClass(midPoint_2, midPoint_1);

                            pDoorWindow.InsertPoint = dirL_real.StartPoint;

                            pDoorWindow.AngleToXAxis = GeometryHelper.Calc_AngleTo_XAxis(dirL_real);
                        }
                    }
                }

                #endregion
            }
            #endregion

            FloorFrameModeler.DeleteMoved_AddNew_WallLine(ref data);
        }


        static void process_blockAndInserts(int doorOrWindowFlag, ref List<BlockClass> blocks, ref Dictionary<string, MCBlockInfo> mcb_info)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockClass block = blocks[i];
                block.Entity.MergeLines();
                MCHelper.Vertify_MC_BlockInfo(doorOrWindowFlag, ref block, ref mcb_info);
            }
        }

        //获取初级的方向线信息
        static void getBasicInfo_DirL(ref int doorOrWindowFlag, ref Data data, ref Result result, ref InsertClass insert, out MCBlockInfo mcbi, out Point[] transformedPoints, out LineClass dirL_initial, out bool directionFixed)
        {
            BlockClass block = null;
            if (doorOrWindowFlag == 0)
                block = BIHelper.Find_Block(insert.BlockName, data.Blocks_Door);
            else
                block = BIHelper.Find_Block(insert.BlockName, data.Blocks_Window);

            mcbi = result.Dic_MC_BlockInfo[block.Name];

            Matrix affineMatrix = MatrixHelper.Get_AffineTransformMatrix
                (insert.ScaleX,
               insert.ScaleY,
                insert.RotationAngle,
                insert.InsertPoint.X - block.BasePoint.X,
                insert.InsertPoint.Y - block.BasePoint.Y
                );


            transformedPoints = new Point[4];
            for (int j = 0; j < 4; j++)
            {
                Point pp = mcbi.FourPoints[j];
                if (pp != null)
                    transformedPoints[j] = GeometryHelper.TransformPoint(pp, ref affineMatrix);
            }


            switch (mcbi.Type_MC)
            {
                case MCType.W_General:
                case MCType.M_Sliding:
                    double distance_height = GeometryHelper.Calc_Distance(transformedPoints[0], transformedPoints[1]);
                    double distance_width = GeometryHelper.Calc_Distance(transformedPoints[0], transformedPoints[3]);

                    if (distance_width > distance_height)
                    {
                        dirL_initial = new LineClass(
                            new Point((transformedPoints[0].X + transformedPoints[1].X) / 2, (transformedPoints[0].Y + transformedPoints[1].Y) / 2, 0),
                            new Point((transformedPoints[2].X + transformedPoints[3].X) / 2, (transformedPoints[2].Y + transformedPoints[3].Y) / 2, 0));
                    }
                    else
                    {
                        dirL_initial = new LineClass(
                            new Point((transformedPoints[0].X + transformedPoints[3].X) / 2, (transformedPoints[0].Y + transformedPoints[3].Y) / 2, 0),
                            new Point((transformedPoints[1].X + transformedPoints[2].X) / 2, (transformedPoints[1].Y + transformedPoints[2].Y) / 2, 0));
                    }




                    directionFixed = false;
                    break;

                case MCType.M_SingleLeaf:
                case MCType.M_SonMather:
                    dirL_initial = new LineClass(transformedPoints[0], transformedPoints[1]);
                    directionFixed = true;
                    break;
                case MCType.M_DoubleLeaf:
                    Point centre_1 = transformedPoints[0];
                    Point centre_2 = transformedPoints[1];
                    Point pointInArc = transformedPoints[2];


                    MathExtension.Vector.Vector v_arc = new LineClass(centre_1, pointInArc).ToVector();
                    MathExtension.Vector.Vector v_dirL = new LineClass(centre_1, centre_2).ToVector();

                    MathExtension.Vector.Vector v_cross = v_dirL.CrossMultiply(v_arc);
                    if (v_cross.Z < 0)
                    {
                        Point tem = centre_1;
                        centre_1 = centre_2;
                        centre_2 = tem;
                    }

                    dirL_initial = new LineClass(centre_1, centre_2);
                    directionFixed = true;
                    break;
                default:
                    dirL_initial = null;
                    directionFixed = false;
                    break;
            }
        }


        #endregion

        #region 分拣电梯门参数
        /// <summary>
        /// 将电梯门分拣出来（该方法必须在识别全部功能区之后调用）
        /// </summary>
        /// <param name="fp"></param>
        public static void ClusterElevatorDoor(ref FloorPlan fp)
        {
            List<SemanticPolygon> esp = (from p in fp.PResult.SPolygons
                                         where p.PType == PolygonType.ElevatorShaft
                                         select p).ToList<SemanticPolygon>();

            if (esp.Count == 0 || fp.PResult.Doors.Count == 0) return;

            List<LineClass> doorLines = null;
            double dis_1 = 0d;
            double dis_2 = 0d;
            foreach (var elevator in esp)
            {
                doorLines = (from p in elevator.Lines
                             where p.LType == LineType.DoorLine
                             select p).ToList<LineClass>();

                foreach (var doorLine in doorLines)
                {

                    foreach (var door in fp.PResult.Doors)
                    {

                        dis_1 = MathExtension.Geometry.GeometryHelper.Calc_Distance(doorLine.StartPoint, door.InsertPoint);
                        dis_2 = MathExtension.Geometry.GeometryHelper.Calc_Distance(doorLine.EndPoint, door.InsertPoint);

                        if (dis_1 < fp.Configuration.ThreadValue_MaxWallWidth || dis_2 < fp.Configuration.ThreadValue_MaxWallWidth)
                        {
                            door.Type = MCType.M_ElevatorDoor;
                            continue;
                        }

                    }
                }
            }
        }
        #endregion
    }
}
