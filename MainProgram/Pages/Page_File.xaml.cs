using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace MainProgram.Pages
{
    /// <summary>
    /// Page_File.xaml 的交互逻辑
    /// </summary>
    public partial class Page_File : Page, IPageInterfaces
    {
        #region 内部字段
        //引用的外部变量
        MainWindow homeWindow = null;
        #endregion

        #region 公开字段
        public static int PageIndex = 0;//页索引

        public FileType CurrentType;//当前文档类型
        public string FileName = null;//文件路径
        public bool HasSetTask = false;//标识是否已经建立任务
        #endregion

        public Page_File(MainWindow homeWindow)
        {
            InitializeComponent();

            //使用外部变量赋值
            this.homeWindow = homeWindow;

            //订阅事件
            homeWindow.btnOpenFile.Click += btnOpenFile_Click;
            homeWindow.btnReset.Click += btnReset_Click;
            homeWindow.btnExit.Click += btnExit_Click;
        }

        #region 事件
        //打开文件
        void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "打开文件";
            openFileDialog.Filter = "DXF文件(.dxf)|*.dxf|SXD文件(.sxd)|*.sxd|XML文件(.xml)|*.xml";

            if (openFileDialog.ShowDialog() == true)
            { 
                //如果已经选择文件,即视为已经建立任务
                HasSetTask = true;

                FileName = openFileDialog.FileName;

                string extension = DotNetCommonUtilities.BasicWrapper.FileUtilities.Get_Extension(openFileDialog.FileName);

                switch (extension)
                {
                    case "dxf":
                        CurrentType = FileType.DXF;
                        break;
                    case "sxd":
                        CurrentType = FileType.SXD;
                        break;
                    case "xml":
                        CurrentType = FileType.XML;
                        break;
                }

                homeWindow.btnOpenFile.IsEnabled = false;

                //进入相关模式
                homeWindow.SelectPattern(CurrentType);
            }
        }

        //重置
        void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (HasSetTask)
            {
                if (MessageBox.Show("确定放弃当前处理批次吗？", "", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    homeWindow.SelectPattern(FileType.UnSelected);
            }
        }

        //退出
        void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("要关闭程序吗？", "", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No))
                Application.Current.Shutdown();
        }
        #endregion

        #region 接口
        public void ActivateControls()
        {
        }

        #endregion
    }
}
