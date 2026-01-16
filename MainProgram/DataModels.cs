using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MainProgram.Pages;
using DXFReader.DataModel;
using BCE.DataModels.Basic;


namespace MainProgram
{
    /// <summary>
    /// 指定当前系统打开的初始文件的类型
    /// </summary>
    public enum FileType { UnSelected, DXF, XML, SXD }

    /// <summary>
    /// 图层类型，用于图层映射阶段
    /// </summary>
    public enum LayerType { Wall, Balcony, Door, Window, Stair, Elevator, Label }

    /// <summary>
    /// 用于将图层信息绑定到UI
    /// </summary>
    public class LayerNamesForBind
    {
        public ObservableCollection<BoolString> Wall_LayerNames = null;
        public ObservableCollection<BoolString> Balcony_LayerNames = null;
        public ObservableCollection<BoolString> Door_LayerNames = null;
        public ObservableCollection<BoolString> Window_LayerNames = null;
        public ObservableCollection<BoolString> Stair_LayerNames = null;
        public ObservableCollection<BoolString> Elevator_LayerNames = null;
        public ObservableCollection<BoolString> Label_LayerNames = null;

        public void Clear()
        {
            if (Wall_LayerNames != null)
                Wall_LayerNames.Clear();

            if (Balcony_LayerNames != null)
                Balcony_LayerNames.Clear();

            if (Door_LayerNames != null)
                Door_LayerNames.Clear();

            if (Window_LayerNames != null)
                Window_LayerNames.Clear();

            if (Stair_LayerNames != null)
                Stair_LayerNames.Clear();

            if (Elevator_LayerNames != null)
                Elevator_LayerNames.Clear();

            if (Label_LayerNames != null)
                Label_LayerNames.Clear();
        }

        public void Reset()
        {
            if (Wall_LayerNames != null)
                foreach (var item in Wall_LayerNames)
                {
                    item.IsChecked = false;
                }

            if (Balcony_LayerNames != null)
                foreach (var item in Balcony_LayerNames)
                {
                    item.IsChecked = false;
                }

            if (Door_LayerNames != null)
                foreach (var item in Door_LayerNames)
                {
                    item.IsChecked = false;
                }

            if (Window_LayerNames != null)
                foreach (var item in Window_LayerNames)
                {
                    item.IsChecked = false;
                }

            if (Stair_LayerNames != null)
                foreach (var item in Stair_LayerNames)
                {
                    item.IsChecked = false;
                }

            if (Elevator_LayerNames != null)
                foreach (var item in Elevator_LayerNames)
                {
                    item.IsChecked = false;
                }

            if (Label_LayerNames != null)
                foreach (var item in Label_LayerNames)
                {
                    item.IsChecked = false;
                }
        }
    }


    public class RingsEvaluationForBind
    {

        public PolygonType PType { get; set; }

        public int Num_Rings { get; private set; }
        public int Num_IncomRings { get; private set; }
        public int Num_IsolatedLines { get; private set; }

        public bool Correct
        {
            get
            {
                if (Num_IncomRings > 0) return false;

                if (Num_Rings == 0) return false;

                return true;
            }
        }

        public RingsEvaluationForBind(Result result, PolygonType pType, LineType lType)
        {
            PType = pType;


            switch (pType)
            {
                case PolygonType.Wall:
                case PolygonType.Balustrade:
                    Num_Rings = result.SPolygons.Where<SemanticPolygon>(
                        (p) =>
                        {
                            if (p.PType == PType)
                                return true;
                            else
                                return false;
                        }).Count<SemanticPolygon>();

                    Num_IncomRings = result.ImSPolygons.Where<SemanticPolygon>(
                        (p) =>
                        {
                            if (p.PType == PType)
                                return true;
                            else
                                return false;
                        }).Count<SemanticPolygon>();

                    Num_IsolatedLines = 0;

                    Num_IsolatedLines = result.IsolateLines.Where<LineClass>(
                        (p) =>
                        {
                            if (p.LType == lType)
                                return true;
                            else
                                return false;
                        }).Count<LineClass>();
                    break;
                default:
                    Num_Rings = result.SPolygons.Where<SemanticPolygon>(
                        (p) =>
                        {
                            if (p.PType != PolygonType.Wall && p.PType != PolygonType.Balustrade)
                                return true;
                            else
                                return false;
                        }).Count<SemanticPolygon>();

                    Num_IncomRings = result.ImSPolygons.Where<SemanticPolygon>(
                        (p) =>
                        {
                            if (p.PType != PolygonType.Wall && p.PType != PolygonType.Balustrade)
                                return true;
                            else
                                return false;
                        }).Count<SemanticPolygon>();

                    Num_IsolatedLines = 0;

                    Num_IsolatedLines = result.IsolateLines.Where<LineClass>(
                        (p) =>
                        {
                            if (p.LType != LineType.WallLine && p.LType != LineType.BalconyLine)
                                return true;
                            else
                                return false;
                        }).Count<LineClass>();
                    break;
            }






            #region MyRegion


            //Num_Rings = result.SPolygons.Where<SemanticPolygon>((p) =>
            //{
            //    if (ptype == PolygonType.Wall || ptype == PolygonType.Balustrade)
            //    {
            //        if (p.PType == ptype)
            //            return true;
            //        return false;
            //    }
            //    else
            //    {
            //        //反选
            //        if (ptype != PolygonType.Wall && ptype != PolygonType.Balustrade)
            //            return true;
            //        return false;
            //    }
            //}).Count<SemanticPolygon>();

            //Num_IncomRings = result.ImSPolygons.Where<SemanticPolygon>((p) =>
            //{
            //    if (ptype == PolygonType.Wall || ptype == PolygonType.Balustrade)
            //    {
            //        if (p.PType == ptype)
            //            return true;
            //        return false;
            //    }
            //    else
            //    {   //反选
            //        if (ptype != PolygonType.Wall && ptype != PolygonType.Balustrade)
            //            return true;
            //        return false;
            //    }


            //}).Count<SemanticPolygon>();

            //Num_IsolatedLines = result.IsolateLines.Where<LineClass>((p) =>
            //{
            //    if (p.LType == ltype)
            //        return true;
            //    else
            //        return false;
            //}).Count<LineClass>();






            //if (Num_IncomRings > 0 || Num_IncomRings + Num_Rings == 0)
            //    Correct = false;
            //else
            //    Correct = true;
            #endregion
        }
    }
}
