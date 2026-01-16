using BCE.DataModels;
using DXFReader.DataModel;
using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels.Basic;
using BCE.Helpers;
using System.Windows;

namespace BCE.BCExtractors
{
    /* ****************************************************************
     * 提取标准电梯符号(仅包含 1 个电梯门线)所在电梯间参数
     * 对于包含多条门线的电梯符号，不进行提取
     ******************************************************************/


    /// <summary>
    /// 提取电梯参数
    /// </summary>
    public abstract class EElevator
    {
        /// <summary>
        /// 提取所有的电梯参数
        /// </summary>
        /// <param name="result"></param>
        public static void Extract(ref FloorPlan floorPlan)
        {
            if (floorPlan.PResult.Elevators== null)
                floorPlan.PResult.Elevators = new List<Elevator>();


            var ers = (from sp in floorPlan.PResult.SPolygons
                       where sp.PType == PolygonType.ElevatorShaft
                       select sp).ToList<SemanticPolygon>();


            for (int i = 0; i < ers.Count; i++)
            {
                SemanticPolygon er = ers[i];
                Elevator elevator = null;


    
                var doorLines = (from line in er.Lines
                                 where line.LType == LineType.DoorLine
                                 select line).ToList<LineClass>();

                switch (doorLines.Count)
                {
                    case 0:
                        //throw new ArgumentException("在电梯间多边形中未检测到门线！");
                        System.Diagnostics.Debug.WriteLine("警告：发现一个没有门的电梯间，已跳过。");
                        break;
                    case 1:
                        LineClass doorLine = doorLines[0];
                        elevator = extract_SElevator(ref er, ref doorLine);
                        break;
                    case 2:
                        MessageBox.Show("包含双门电梯，详细参数无法提取");
                        break;
                }

                floorPlan.PResult.Elevators.Add(elevator);
            }
        }



        #region 内部方法
        /// <summary>
        /// 提取标准电梯的参数（单门）
        /// </summary>
        /// <param name="sp_elevator"></param>
        /// <returns></returns>
        private static Elevator extract_SElevator(ref SemanticPolygon sp_elevator,ref LineClass doorLine)
        {
            Elevator elevator = new Elevator();
            elevator.Type = ElevatorType.SingleDoor;
            elevator.DoorLine = doorLine;
            LineClass maxLine = ((from line in sp_elevator.Lines
                                  where GeometryHelper.Parallel(line, elevator.DoorLine)
                                  orderby line.Length descending
                                  select line).ToArray<LineClass>())[0];

            MathExtension.Geometry.Point middlePoint = elevator.DoorLine.Get_MiddlePoint();

            MathExtension.Vector.Vector v1 = (maxLine.StartPoint - middlePoint).ToVector();
            MathExtension.Vector.Vector v2 = (maxLine.EndPoint - middlePoint).ToVector();

            MathExtension.Vector.Vector v = v2.CrossMultiply(v1);

            if (v.Z > 0)
            {
                elevator.InsertPoint = maxLine.StartPoint;
            }
            else if (v.Z < 0)
            {
                maxLine.Reverse();
                elevator.InsertPoint = maxLine.StartPoint;
            }
            else
                throw new Exception("无法确定标准电梯的定位点！");


            elevator.RotateAngle = GeometryHelper.Calc_AngleTo_XAxis(maxLine);


            elevator.Width = maxLine.Length;

            LineClass verticalLine = ((from line in sp_elevator.Lines
                                       where GeometryHelper.Vertical(line, maxLine)
                                       orderby line.Length descending
                                       select line).ToArray<LineClass>())[0];

            elevator.Depth = verticalLine.Length;

            return elevator;
        }
        #endregion
    }





}
