using BCE.DataModels;
using BCE.DataModels.Basic;
using DXFReader;
using DXFReader.DataModel;
using MathExtension.Geometry;
using MathExtension.Matrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCE.Helpers
{
    //提供门窗相关的方法
    public class MCHelper
    {

        private const double rotate = 0.25;//系数，用于控制在方向线两端寻找到的墙线到方向线端点的距离是否小于等于方向线自身长度 L*rotate  
        const double threshold_samePoint_forBlock = 0.1d;//点阈值，用于确定单叶门方向线的终点，终点为狐线的悬挂端点，改点需要使用图块设计尺度（通常为1个单位）的阈值进行限定
        static double threashold_sameWidth = 0.001d;

        /// <summary>
        /// 查找门或窗户两端的一对墙线
        /// </summary>
        /// <param name="pDirectionLine">窗户左下角点至右下角点的连线</param>
        /// <param name="wallLines">经过合并平行邻接或压盖处理的墙线</param>
        /// <returns></returns>
        public static LineClass[] Find_WallLines(LineClass pDirectionLine, ref List<LineClass> wallLines)
        {
            LineClass[] potentialWallEnds = new LineClass[2];

  
            List<LineClass> verticalWallLines = wallLines.FindAll(new Predicate<LineClass>((line) =>
            {
                if (Math.Abs(MathExtension.Geometry.GeometryHelper.Calc_Angle(line, pDirectionLine, 0) - 90) <= 1)//垂直判定的角度阈值为+-1度
                    return true;
                return false;
            }));


            if (verticalWallLines.Count > 1)
            {
              
                LineClass swl = verticalWallLines[0];
                double distance1 = GeometryHelper.Calc_Distance2(pDirectionLine.StartPoint, swl);
                double currentDis;
                for (int i = 1; i < verticalWallLines.Count; i++)
                {
                    currentDis = GeometryHelper.Calc_Distance2(pDirectionLine.StartPoint, verticalWallLines[i]);
                    if (currentDis < distance1)
                    {
                        distance1 = currentDis;
                        swl = verticalWallLines[i];
                    }
                }
                potentialWallEnds[0] = swl;

               
                LineClass ewl = verticalWallLines[0];
                double distance2 = GeometryHelper.Calc_Distance2(pDirectionLine.EndPoint, ewl);

                for (int i = 1; i < verticalWallLines.Count; i++)
                {
                    currentDis = GeometryHelper.Calc_Distance2(pDirectionLine.EndPoint, verticalWallLines[i]);
                    if (currentDis < distance2)
                    {
                        distance2 = currentDis;
                        ewl = verticalWallLines[i];
                    }

                }
                potentialWallEnds[1] = ewl;


                
                if (distance1 >= rotate * pDirectionLine.Length)
                    throw new Exception(string.Format("寻找到的墙线距离窗方向线的起点距离大于方向线长度的 {0} 倍", rotate.ToString()));

                if (distance2 >= rotate * pDirectionLine.Length)
                    throw new Exception(string.Format("寻找到的墙线距离窗方向线的终点距离大于方向线长度的 {0} 倍", rotate.ToString()));

                return potentialWallEnds;
            }
            else
                throw new Exception("寻找到的垂直于窗方向线的墙线数目不足 2 条!");
        }


        /// <summary>
        /// 确定门窗块的BlockInfo(普通窗和滑动门将提取三个点；单页、双叶、子母门将提取三个点；此外还提取门窗初步的类型)
        /// </summary>
        /// <param name="pBlock"></param>
        /// <param name="dic_MC_Blockinfo"></param>
        public static void Vertify_MC_BlockInfo(int doorOrWindowFlag, ref BlockClass pBlock, ref Dictionary<string, MCBlockInfo> dic_MC_Blockinfo)
        {
            MCBlockInfo mbi = new MCBlockInfo();
            EntityClass entity = pBlock.Entity;
            int arcCount = entity.Arcs.Count;
            #region 计算包含弧段的块的XY最值,在本系统中包含圆弧的图块一定是门图块



            switch (arcCount)
            {
                case 0:
  
                    IEnumerable<Point> points = BIHelper.Get_BlockDistinctedPoints(pBlock);//块的点集
                    double[] mostXY = BIHelper.Find_XYMostValue(points);
                    mbi.FourPoints = BIHelper.Get_FourBoundPoints(ref mostXY);

         
                    if (doorOrWindowFlag == 0)
                        mbi.Type_MC = MCType.M_Sliding;//滑动门
                    else
                        mbi.Type_MC = MCType.W_General;//普通窗


                    dic_MC_Blockinfo.Add(pBlock.Name, mbi);
                    break;



                case 1:
                    ArcClass arc = entity.Arcs[0];
                    mbi.Type_MC = MCType.M_SingleLeaf;//在提取门窗参数时将进一步明确其开启方向是顺/逆时针
                    mbi.FourPoints[0] = arc.Centre;
                    mbi.FourPoints[1] = arc.StartPoint;
                    mbi.FourPoints[2] = arc.EndPoint;

                    foreach (var item in entity.Lines)
                    {
                        double dis1 = GeometryHelper.Calc_Distance(item.StartPoint, arc.StartPoint);
                        double dis2 = GeometryHelper.Calc_Distance(item.EndPoint, arc.StartPoint);


                        if (dis1 <= threshold_samePoint_forBlock || dis2 <= threshold_samePoint_forBlock)
                        {
                            mbi.FourPoints[1] = arc.EndPoint;
                            mbi.FourPoints[2] = arc.StartPoint;
                            break;
                        }
                    }


                    dic_MC_Blockinfo.Add(pBlock.Name, mbi);
                    break;
                case 2:

                    Point centre_1 = entity.Arcs[0].Centre;
                    Point centre_2 = entity.Arcs[1].Centre;
                    if (Math.Abs(entity.Arcs[0].Radius - entity.Arcs[1].Radius) <= threashold_sameWidth)
                    {
                        mbi.Type_MC = MCType.M_DoubleLeaf;
                        mbi.FourPoints[0] = centre_1;
                        mbi.FourPoints[1] = centre_2;
                    }
                    else
                    {
                        mbi.Type_MC = MCType.M_SonMather;
                        if (entity.Arcs[0].Radius > entity.Arcs[1].Radius)
                        {
                            mbi.FourPoints[0] = centre_1;
                            mbi.FourPoints[1] = centre_2;
                        }
                        else
                        {
                            mbi.FourPoints[0] = centre_2;
                            mbi.FourPoints[1] = centre_1;
                        }
                    }


                 
                    double diff_x = Math.Abs(centre_1.X - centre_2.X);
                    double diff_y = Math.Abs(centre_1.Y - centre_2.Y);
                    Point pointInArc = new Point();
                    if (diff_x > diff_y)
                    {

                        pointInArc.X = (centre_1.X + centre_2.X) / 2;
                        pointInArc.Y = entity.Arcs[0].StartPoint.Y;
                        pointInArc.Z = 0d;

                        Relation2D_Point_Line relation = MathExtension.Geometry.GeometryHelper.BasicRelation_2D(new Line(centre_1, centre_2), pointInArc);
                        if (relation != Relation2D_Point_Line.OutLine)
                            pointInArc.Y = entity.Arcs[0].EndPoint.Y;
                    }
                    else
                    {

                        pointInArc.X = entity.Arcs[0].StartPoint.X;
                        pointInArc.Y = (centre_1.Y + centre_2.Y) / 2;
                        pointInArc.Z = 0d;

                        Relation2D_Point_Line relation = MathExtension.Geometry.GeometryHelper.BasicRelation_2D(new Line(centre_1, centre_2), pointInArc);
                        if (relation != Relation2D_Point_Line.OutLine)
                            pointInArc.X = entity.Arcs[0].EndPoint.X;

                    }
                    mbi.FourPoints[2] = pointInArc;

                    dic_MC_Blockinfo.Add(pBlock.Name, mbi);
                    break;
            }
            #endregion
        }
    }
}
