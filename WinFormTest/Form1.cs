using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace WinFormTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.KeyDown += Form1_KeyDown;
            this.KeyPress += Form1_KeyPress;



            this.MouseClick += Form1_MouseClick;


        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            MessageBox.Show("KeyPress");
        }

        void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("MouseClick");
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.W:
                    MessageBox.Show(e.KeyData.ToString());
                    break;
                case Keys.S:
                    MessageBox.Show(e.KeyData.ToString());
                    break;
                case Keys.A:
                    MessageBox.Show(e.KeyData.ToString());
                    break;
                case Keys.D:
                    MessageBox.Show(e.KeyData.ToString());
                    break;
            }
        }





    }
}
