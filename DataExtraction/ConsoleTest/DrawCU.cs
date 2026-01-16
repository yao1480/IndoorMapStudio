using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using DXFReader.DataModel;
using BCE.DataModels;

namespace ConsoleTest
{
    public class DrawCU
    {
        public static void DrawFloor(FloorPlan pFloor)
        {
            using (var game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();


                    //确定绘图窗口坐标范围
                    double heightToWidthRate = (double)game.Height / (double)game.Width;
                    double left = 0;
                    double right = 40000;
                    double bottom = 0;
                    double height = bottom + (right - left) * heightToWidthRate;
                    double zNear = -100;
                    double zFar = 100;

                    GL.Ortho(left, right, bottom, height, zNear, zFar);


                    #region 绘制层数据
                    //墙
                    //var wallSP = from p in pFloor.PResult.SPolygons
                    //             where p.PType != PolygonType.Wall && p.PType != PolygonType.Balustrade
                    //             select p;
                    //foreach (var item in wallSP)
                    //{
                    //    DrawLineLoop(item.Points, Color.White);
                    //}

                    //功能区点
                    List<SemanticPolygon> sps = (from p in pFloor.PResult.SPolygons
                                                 where p.PType != PolygonType.Floor &&
                                                 p.PType != PolygonType.Wall
                                                 && p.PType != PolygonType.Balustrade
                                                 select p).ToList<SemanticPolygon>();

                    foreach (var item in sps)
                    {
                        drawPoint(item.FunctionRegionPoint, Color.Green);
                    }

                    #endregion



                    game.SwapBuffers();
                };

                // Run the game at 60 updates per second
                game.Run(1.0);
            }
        }


        static void drawPoint(MathExtension.Geometry.Point point, System.Drawing.Color color)
        {
            GL.Color3(color);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(point.X,point.Y);
            GL.End();
        }

        /// <summary>
        /// 绘制点集
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        static void DrawPoints(List<MathExtension.Geometry.Point> points, System.Drawing.Color color)
        {
            GL.Color3(color);
            GL.Begin(PrimitiveType.Points);
            for (int i = 0; i < points.Count; i++)
            {
                GL.Vertex2(points[i].X, points[i].Y);
            }
            GL.End();
        }

        /// <summary>
        /// 绘制单个闭合环
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        static void DrawLineLoop(List<MathExtension.Geometry.Point> points, System.Drawing.Color color)
        {
            GL.Color3(color);
            GL.Begin(PrimitiveType.LineLoop);
            for (int i = 0; i < points.Count; i++)
            {
                GL.Vertex2(points[i].X, points[i].Y);
            }
            GL.End();
        }

        /// <summary>
        /// 绘制线段集
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="color"></param>
        static void DrawLineSegment(List<DXFReader.DataModel.LineClass> lines, System.Drawing.Color color)
        {
            GL.Color3(color);

            for (int i = 0; i < lines.Count; i++)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2(lines[i].StartPoint.X, lines[i].StartPoint.Y);
                GL.Vertex2(lines[i].EndPoint.X, lines[i].EndPoint.Y);
                GL.End();
            }

        }
    }
}
