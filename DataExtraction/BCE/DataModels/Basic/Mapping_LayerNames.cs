using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCE.DataModels.Basic
{
    public class Mapping_LayerNames
    {
        public List<string> Wall = null;
        public List<string> Balcony = null;
        public List<string> Window = null;
        public List<string> Door = null;
        public List<string> Elevator = null;
        public List<string> Text = null;
        public List<string> Stairs = null;


        public Mapping_LayerNames()
        {
            Wall = new List<string>();
            Balcony = new List<string>();
            Window = new List<string>();
            Door = new List<string>();
            Elevator = new List<string>();
            Text = new List<string>();
            Stairs = new List<string>();
        }
    }

    /// <summary>
    /// 用于记录部分建筑构件类型(墙、阳台扶手、功能区)对应闭合环的提取结果
    /// </summary>
    public class RingsExtractionInfo
    {
        string ComponentType { get; set; }
        int Num_Rings { get; set; }
        int Num_ImcomRings { get; set; }
        int Num_IsolatedLines { get; set; }
        bool Result { get; set; }
    }
}
