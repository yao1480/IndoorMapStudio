using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCE.BCExtractors;
using DXFReader.DataModel;
using System.Collections.Generic;

namespace BCE.DataModels.Basic
{

    /// <summary>
    /// 用于组织图纸附加属性及解析图纸所采用的阈值
    /// </summary>
    [Serializable]
    public class ParametersConfiguration
    {
        //图纸附加属性
        /// <summary>
        /// DXF文件路径
        /// </summary>
        public string DXF_Path;

        /// <summary>
        /// 图层映射
        /// </summary>
        public Mapping_LayerNames LayerMapping { get; set; }

        /// <summary>
        /// 图纸类型
        /// </summary>
        public PlaneType FPType = PlaneType.Plan_StandardFloor;

        /// <summary>
        /// 楼板厚 
        /// </summary>
        public double Floor_Thickness = 100d;

        /// <summary>
        /// 楼层净高
        /// </summary>
        public double Floor_Headroom = 2700d;

        /// <summary>
        /// 楼层总高
        /// </summary>
        public double FloorHeight
        {
            get { return Floor_Thickness + Floor_Headroom; }
        }


        /// <summary>
        /// 楼层起始高度
        /// </summary>
        public double StartHeight = 0d;

        /// <summary>
        /// 图纸重复使用次数(即为基于当前图纸生成的楼层数)
        /// </summary>
        public int RepeatCount =2;

        //门附加属性
        public double Door_StartHeight = 0d;
        public double Door_Heigth = 2100d;

        //窗附加属性
        public double Window_StartHeight = 900d;
        public double Window_Heigth = 1500d;



        //阳台附加属性
        public double BalconyHandrail_Height = 1050d;

        //楼梯扶手附加属性
        public double StairHandrail_Height = 1050d;
        public double StairHandrail_Width = 80d;
        public double StairHandrail_Thickness = 100d;

        //电梯附加属性
        public double Elevator_Height = 2600d;
        public double Elevator_Door_Thickness = 100d;
        public double Elevator_Door_Heigth = 2100d;



        //4 个阈值
        /// <summary>
        /// 墙宽阈值
        /// </summary>
        public double ThreadValue_MaxWallWidth = 240d;

        /// <summary>
        /// 提取闭合环使用的缝隙阈值(缺省值：10mm)
        /// </summary>
        public double ThreadValue_GapWidth = 10d;

        /// <summary>
        /// 共点阈值
        /// </summary>
        public double ThreadValue_SamePoint = 0.001d;

        /// <summary>
        /// /等角阈值
        /// </summary>
        public double Threadshold_EqualAngle = 1d;







        public ParametersConfiguration(string dxfPath)
        {
            DXF_Path = dxfPath;
            LayerMapping = new Mapping_LayerNames();
        }


        //为了使该类支持序列化而设置
        public ParametersConfiguration()
        {

        }
    }
}
