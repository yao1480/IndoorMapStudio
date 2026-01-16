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

namespace Feedback.Controls
{
    public partial class MapControlWrapper : UserControl
    {

        public AxMapControl MapControl { get { return axMapControl1; } }

        public MapControlWrapper()
        {
            InitializeComponent();
        }
    }
}
