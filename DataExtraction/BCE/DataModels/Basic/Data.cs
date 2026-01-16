using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DXFReader.DataModel;

namespace BCE.DataModels.Basic
{
    public class Data
    {
        //public Mapping_LayerNames LayerNames;
        public List<LineClass> WallLines;
        public List<LineClass> BalcLines;
        public List<LineClass> ElevLines;
        public List<LineClass> StairLines;
        public List<InsertClass> Inserts_Door;
        public List<InsertClass> Inserts_Window;
        public List<BlockClass> Blocks_Door;
        public List<BlockClass> Blocks_Window;
        public List<TextClass> Annotations;

        public List<LineClass> NewWallines;//存储在处理门窗、阳台符号过程中生成的新墙线

        public Data()
        {
            //LayerNames = new Mapping_LayerNames();
            NewWallines = new List<LineClass>();
        }

    }
}
