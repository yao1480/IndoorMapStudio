using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.BCExtractors;
using DXFReader.DataModel;
using MathExtension.Geometry;

namespace BCE.DataModels.Basic
{
    public class Result
    {
        List<LineClass> doorLines;
        List<LineClass> windowLines;

        //语义多边形提取成果
        public List<SemanticPolygon> SPolygons = null;
        public List<SemanticPolygon> ImSPolygons = null;
        public List<LineClass> IsolateLines = null;

        //建筑附件
        public List<Door> Doors = null;
        public List<Window> Windows = null;
        public List<Elevator> Elevators = null;
        public List<Stair> Stairs = null;

        /// <summary>
        /// 门线
        /// </summary>
        public List<LineClass> DoorLines
        {
            get
            {
                if (doorLines == null)
                {
                    doorLines = new List<LineClass>();
                    List<SemanticPolygon> sps = (from p in this.SPolygons
                                                 where p.PType != PolygonType.Wall && p.PType != PolygonType.Balustrade
                                                 select p).ToList<SemanticPolygon>();

                    foreach (var item in sps)
                    {
                        List<LineClass> dLines = (from p in item.Lines
                                                  where p.LType == LineType.DoorLine
                                                  select p).ToList<LineClass>();

                        if (dLines.Count > 0)
                            doorLines.AddRange(dLines);
                    }
                }

                return doorLines;
            }
        }

        /// <summary>
        /// 窗线
        /// </summary>
        public List<LineClass> WindowLines
        {
            get
            {
                if (windowLines == null)
                {
                    windowLines = new List<LineClass>();
                    List<SemanticPolygon> sps = (from p in this.SPolygons
                                                 where p.PType != PolygonType.Wall && p.PType != PolygonType.Balustrade
                                                 select p).ToList<SemanticPolygon>();

                    foreach (var item in sps)
                    {
                        List<LineClass> wLines = (from p in item.Lines
                                                  where p.LType == LineType.WindowLine
                                                  select p).ToList<LineClass>();

                        if (wLines.Count > 0)
                            windowLines.AddRange(wLines);
                    }
                }

                return windowLines;
            }
        }

        //图块信息
        public Dictionary<string, MCBlockInfo> Dic_MC_BlockInfo;


        /*提取墙、阳台扶手轮廓、功能区用到的闭合环，用于支持提取结果的可视化检查;
         * 通过CollectRingExtractionResult方法赋值
        */
        public ERings ER_ForWalls = null;
        public ERings ER_ForBalustrade = null;
        public ERings ER_ForFuncRegion= null;



        public Result()
        {
            SPolygons = new List<SemanticPolygon>();
            ImSPolygons = new List<SemanticPolygon>();
            IsolateLines = new List<LineClass>();

            Dic_MC_BlockInfo = new Dictionary<string, MCBlockInfo>();
        }

        public void CollectRingExtractionResult(ref ERings er, PolygonType ringType = PolygonType.Unknown, LineType lineType = LineType.Unknown, Func<List<SemanticPolygon>, PolygonType, List<SemanticPolygon>> deepCopyRings = null)
        {
            //IncompRings、IsolatedLines 浅拷贝
            for (int i = 0; i < er.IncompRings.Count; i++)
            {
                er.IncompRings[i].PType = ringType;
            }

            for (int i = 0; i < er.IsolateLines.Count; i++)
            {
                er.IsolateLines[i].LType = lineType;
            }

            this.ImSPolygons.AddRange(er.IncompRings);
            this.IsolateLines.AddRange(er.IsolateLines);

            //自动判断是否使用Rings的深拷贝副本
            if (deepCopyRings == null)
            {
                for (int i = 0; i < er.Rings.Count; i++)
                {
                    er.Rings[i].PType = ringType;
                }



                this.SPolygons.AddRange(er.Rings);
            }
            else
            {
                this.SPolygons.AddRange(deepCopyRings(er.Rings, ringType));
            }

        }
    }
}
