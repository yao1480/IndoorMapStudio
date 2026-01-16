using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ESRI.ArcGIS.esriSystem;

namespace MainProgram
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private LicenseInitializer m_AOLicenseInitializer = new MainProgram.LicenseInitializer();

        // 构造函数保持简洁
        public App()
        {
            // 这里不需要再绑定事件了，我们直接重写 OnStartup
            this.Exit += Application_Exit;
        }

        // 【核心修改】重写 OnStartup，确保顺序绝对正确
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. 最优先：绑定 ArcGIS Runtime (必须放在第一行)
            // 如果不绑定，加载 DLL 时就会崩溃
            if (!ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Engine))
            {
                // 如果 Engine 绑定失败，尝试绑定 Desktop (防止开发机只有 Desktop 环境)
                if (!ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop))
                {
                    MessageBox.Show("无法绑定 ArcGIS 运行时许可，程序将退出。");
                    this.Shutdown();
                    return;
                }
            }

            // 2. 初始化高级许可 (你的 LicenseInitializer 类)
            try
            {
                m_AOLicenseInitializer.InitializeApplication(
                    new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeEngine },
                    new esriLicenseExtensionCode[] {
                        esriLicenseExtensionCode.esriLicenseExtensionCode3DAnalyst,
                        esriLicenseExtensionCode.esriLicenseExtensionCodeNetwork,
                        esriLicenseExtensionCode.esriLicenseExtensionCodeSpatialAnalyst,
                        esriLicenseExtensionCode.esriLicenseExtensionCodeSchematics,
                        esriLicenseExtensionCode.esriLicenseExtensionCodeMLE,
                        esriLicenseExtensionCode.esriLicenseExtensionCodeDataInteroperability,
                        esriLicenseExtensionCode.esriLicenseExtensionCodeTracking
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("许可初始化失败: " + ex.Message);
                this.Shutdown();
                return;
            }

            // 3. 执行你的系统初始化逻辑
            if (!SystemInitialize())
            {
                this.Shutdown();
                return;
            }

            // 4. 【最后一步】手动启动主窗口
            // 必须等上面许可全部搞定，才能去 new MainWindow()，否则解析 XAML 会崩
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        void Application_Exit(object sender, ExitEventArgs e)
        {
            //ESRI License Initializer generated code.
            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();
        }

        #region 系统级方法
        public static bool SystemInitialize()
        {
            try
            {
                //step1: 检查Esri FGDB 模板
                string exePath = System.Environment.CurrentDirectory;
                string fgdbPath = string.Format(@"{0}\{1}", exePath, @"FGDBTemplate\IndoorMapData.gdb");
                // 确保你的 EsriMapDataGenerator 引用没问题
                EsriMapDataGenerator.FGDBHelper.CheckMapDataFGDB_Initial(fgdbPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("系统初始化失败： {0}", ex.Message), "系统初始化错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static void ResetSystem()
        {
        }
        #endregion
    }
}