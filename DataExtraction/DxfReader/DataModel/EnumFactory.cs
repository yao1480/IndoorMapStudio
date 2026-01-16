using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DXFReader.DataModel
{
    /// <summary>
    /// 功能区类型
    /// </summary>
    public enum PolygonType
    {
        Unknown,
        Wall,
        Floor,
        Parlour,
        DiningRoom,
        BedRoom,
        Study,
        Kitchen,
        Toilet,
        Balcony,
        Balustrade,
        ElevatorShaft,
        StairRoom,


        FunctionRegion//除墙、阳台扶手之外的一切闭合环类型


    }

    /// <summary>
    /// 线段类型
    /// </summary>
    public enum LineType
    {
        Unknown,
        WallLine,
        BalconyLine,
        DoorLine,
        WindowLine,
        Any//任意线段类型
    }


    /// <summary>
    /// 门窗类型
    /// </summary>
    public enum MCType
    {
        Unknown = 0,
        W_General = 2,
        W_Sliding = 4,

        M_SingleLeaf_Clockwise = 8,
        M_SingleLeaf_CounterClockwise = 16,
        M_SingleLeaf = MCType.M_SingleLeaf_Clockwise | MCType.M_SingleLeaf_CounterClockwise,//24

        M_SonMother_Clockwise = 32,
        M_SonMother_CounterClockwise =64,
        M_SonMather = MCType.M_SonMother_Clockwise | MCType.M_SonMother_CounterClockwise,

        M_DoubleLeaf = 128,
        M_Sliding = 256,

        M_ElevatorDoor=512
    }

    /// <summary>
    /// 电梯类型
    /// </summary>
    public enum ElevatorType
    {
        Unknown,
        SingleDoor,
        DoubleDoor
    }


    /// <summary>
    /// 双跑楼梯上楼位置
    /// </summary>
    public enum UpstairPosition
    {
        Unknow,
        Left,
        Right,
    }

    //图纸类型
    public enum PlaneType
    {
        Plan_FirstFloor,
        Plan_StandardFloor,
        Pan_TopFloor
    }

    //图纸解析结果
    public enum ExtructError
    {
        None,
        Err_Wall,
        Err_Baluster,
        Err_Region

    }
}
