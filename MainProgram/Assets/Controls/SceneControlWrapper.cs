using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Geometry;

namespace MainProgram.Assets.Controls
{
    public partial class SceneControlWrapper : UserControl
    {
        public AxSceneControl SceneControl { get { return axSceneControl1; } }

        public SceneControlWrapper()
        {
            InitializeComponent();



            //if (hideToolBar)
            //{
            //    axToolbarControl1.Height = 0;
            //    splitContainer2.SplitterDistance = 0;
            //}

            //if (hideTOOC)
            //{
            //    //无法显式设置 SplitterPanel 的宽度。改在 SplitContainer 上设置 SplitterDistance。
            //    splitContainer1.SplitterDistance = 0;
            //}

            this.MouseWheel += SceneControlWrapper_MouseWheel;

        }

        //滚轮缩放
        void SceneControlWrapper_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                System.Drawing.Point pSceneLocation = axSceneControl1.PointToScreen(axSceneControl1.Location);
                System.Drawing.Point Pt = axSceneControl1.PointToScreen(e.Location);
                if (Pt.X < pSceneLocation.X | Pt.X > pSceneLocation.X + axSceneControl1.Width | Pt.Y < pSceneLocation.Y | Pt.Y > pSceneLocation.Y + axSceneControl1.Height)
                    return;

                double scale;
                if (e.Delta < 0)
                {
                    scale = 0.2;
                    axSceneControl1.MousePointer = esriControlsMousePointer.esriPointerZoomOut;
                }
                else
                {
                    scale = -0.2;
                    axSceneControl1.MousePointer = esriControlsMousePointer.esriPointerZoomIn;
                }


                ICamera pCamera = axSceneControl1.Camera;
                IPoint pObserver = pCamera.Observer;
                IPoint pTarget = pCamera.Target;
                pObserver.X += (pObserver.X - pTarget.X) * scale;
                pObserver.Y += (pObserver.Y - pTarget.Y) * scale;
                pObserver.Z += (pObserver.Z - pTarget.Z) * scale;
                pCamera.Observer = pObserver;
                axSceneControl1.SceneGraph.RefreshViewers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
