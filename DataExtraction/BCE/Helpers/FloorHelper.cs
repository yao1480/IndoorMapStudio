using DXFReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;
using BCE.DataModels;
using BCE.DataModels.Basic;
using BCE.BCExtractors;

namespace BCE.Helpers
{
    public class FloorHelper
    {
        //获取楼层数据
        public static void Get_Floor(FloorPlan fp)
        {
       
        }

        //输出楼层信息
        public static void OutputFloorData(FloorPlan pFloor)
        {
            #region 输出墙体单元
            //for (int i = 0; i < pFloor.Walls.Count; i++)
            //{
            //    Console.WriteLine(string.Format("Wall {0} :", i + 1));
            //    for (int j = 0; j < pFloor.Walls[i].Profile.LinearGPs.Count; j++)
            //    {

            //        LineClass lgp = pFloor.Walls[i].Profile.LinearGPs[j];
            //        Console.WriteLine(string.Format("linear-{0}:\tStartPoint： {1}\t\tEndPoint： {2}", j, lgp.StartPoint, lgp.EndPoint));
            //    }

            //    Console.WriteLine("\n\n");
            //}
            #endregion

            #region 输出阳台单元
            //for (int i = 0; i < pFloor.Balconies.Count; i++)
            //{
            //    Console.WriteLine(string.Format("Balcony {0} :", i + 1));
            //    for (int j = 0; j < pFloor.Balconies[i].Profile.LinearGPs.Count; j++)
            //    {
            //        LineClass lgp = pFloor.Balconies[i].Profile.LinearGPs[j];
            //        Console.WriteLine(string.Format("linear-{0}:\tStartPoint： {1}\t\tEndPoint： {2}", j, lgp.StartPoint, lgp.EndPoint));
            //    }

            //    Console.WriteLine("\n\n");
            //}
            #endregion

            #region 输出窗体单元数据
            //for (int i = 0; i < pFloor.Windows.Count; i++)
            //{
            //    Console.WriteLine(string.Format("Window {0} :", i + 1));
            //    for (int j = 0; j < pFloor.Windows[i].Profile.LinearGPs.Count; j++)
            //    {
            //        LineClass lgp = pFloor.Windows[i].Profile.LinearGPs[j];
            //        Console.WriteLine(string.Format("linear-{0}:\tStartPoint： {1}\t\tEndPoint： {2}", j, lgp.StartPoint, lgp.EndPoint));
            //    }
            //    Console.WriteLine("\n\n");
            //}
            #endregion

            #region 输出门单元数据
            //for (int i = 0; i < pFloor.Doors.Count; i++)
            //{
            //    Console.WriteLine(string.Format("Door {0}", i + 1));
            //    //只输出线段，不输出圆弧
            //    for (int j = 0; j < pFloor.Doors[i].Profile.LinearGPs.Count; j++)
            //    {
            //        LineClass lgp = pFloor.Doors[i].Profile.LinearGPs[j];
            //        Console.WriteLine(string.Format("linear-{0}:\tStartPoint： {1}\t\tEndPoint： {2}", j, lgp.StartPoint, lgp.EndPoint));
            //    }
            //    Console.WriteLine("\n\n");
            //}
            #endregion

            #region 输出电梯单元数据
            //for (int i = 0; i < pFloor.Elevators.Count; i++)
            //{
            //    Console.WriteLine(string.Format("Elevators {0} :", i + 1));
            //    Console.WriteLine("电梯轮廓数据：");
            //    //输出电梯轮廓数据
            //    for (int j = 0; j < pFloor.Elevators[i].Profile.LinearGPs.Count; j++)
            //    {
            //        LineClass lgp = pFloor.Elevators[i].Profile.LinearGPs[j];
            //        Console.WriteLine(string.Format("linear-{0}:\tStartPoint： {1}\t\tEndPoint： {2}", j, lgp.StartPoint, lgp.EndPoint));
            //    }

            //    //输出电梯门线数据
            //    Console.WriteLine("门线数据：");
            //    for (int j = 0; j < pFloor.Elevators[i].DoorLines.Count; j++)
            //    {

            //        LineClass lgp = pFloor.Elevators[i].DoorLines[j];
            //        Console.WriteLine(string.Format("linear-{0}:\tStartPoint： {1}\t\tEndPoint： {2}", j, lgp.StartPoint, lgp.EndPoint));
            //    }
            //    Console.WriteLine("\n\n");
            //}
            #endregion





            #region 提取结果分析
            //Console.WriteLine("\n\n墙体提取情况：");
            //Console.WriteLine(string.Format("墙单元：\t{0} 个", pFloor.Walls.Count));
            //Console.WriteLine(string.Format("非法墙单元：\t{0} 个", pFloor.ELP.UC_Profiles.Count));
            //Console.WriteLine(string.Format("孤立图元：\t{0} 个", pFloor.ELP.SL_Indexs.Count));

            //Console.WriteLine("\n\n阳台提取情况：");
            //Console.WriteLine(string.Format("阳台单元：\t{0} 个", pFloor.Balconies.Count));
            //Console.WriteLine(string.Format("非法阳台单元：\t{0} 个", pFloor.ELP_Balcony.UC_Profiles.Count));
            //Console.WriteLine(string.Format("孤立图元：\t{0} 个", pFloor.ELP_Balcony.SL_Indexs.Count));

            //Console.WriteLine("\n\n窗户提取情况：");
            //Console.WriteLine(string.Format("窗户单元：\t{0} 个", pFloor.Windows.Count));
            //Console.WriteLine(string.Format("窗户图块类型：\t{0} 个", pFloor.EWP.Get_NumOfBlockTypes()));


            //Console.WriteLine("\n\n门提取情况：");
            //Console.WriteLine(string.Format("门单元：\t{0} 个", pFloor.Doors.Count));
            //Console.WriteLine(string.Format("寻找墙端失败门单元：\t{0} 个", pFloor.EDP.Get_FindFailed()));
            //Console.WriteLine(string.Format("门图块类型：\t{0} 个", pFloor.EDP.Get_NumOfBlockTypes()));

            //Console.WriteLine("\n\n电梯提取情况：");
            //Console.WriteLine(string.Format("电梯单元：\t{0} 个", pFloor.EEP.Elevators.Count));
            //Console.WriteLine(string.Format("识别到的电梯点：\t{0} 个", pFloor.EEP.ElevatorPoints.Count));
            //Console.WriteLine(string.Format("未匹配到电梯间的电梯点：\t{0} 个", pFloor.EEP.UnMatched_PointIndexs.Count));
            #endregion
        }

    }
}
