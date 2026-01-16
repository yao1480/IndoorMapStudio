using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace SilenceLibraryForAE.MethodLibrary
{
    public abstract class DataSourceHelper
    {
        /// <summary>
        /// 创建文件地理数据库
        /// （在文件地理数据库中创建新的文件地理数据库或在父文件夹中创建同名文件地理数据库将会抛出异常）
        /// </summary>
        /// <param name="parentDirectory"></param>
        /// <param name="gdbName">不要指定完整路径以及.gdb后缀，给定数据库名称即可</param>
        /// <param name="propertySet"></param>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static IWorkspace Create_FileGDB(string parentDirectory, string gdbName, IPropertySet propertySet = null, int hwnd = 0)
        {
            IWorkspaceFactory pWFactory = new FileGDBWorkspaceFactoryClass();

            //禁止在文件地理数据库中嵌套其他文件地理数据库
            if (pWFactory.IsWorkspace(parentDirectory))
                throw new ArgumentException(string.Format("文件夹 {0}\n 自身已经是文件地理数据库，不能在文件地理数据库中嵌套其他文件地理数据库！", parentDirectory));

            //禁止创建同名文件地理上数据库
            gdbName = gdbName.Trim();
            string gdbPath = parentDirectory + @"\" + gdbName + ".gdb";//将要创建的地理数据库路径
            string[] subDirectories = System.IO.Directory.GetDirectories(parentDirectory);

            foreach (var item in subDirectories)
            {
                if(item==gdbPath)
                    throw new ArgumentException(string.Format("文件夹 {0}\n 已存在同名文件地理数据库 {1} !", parentDirectory, gdbName));
            }

            IWorkspaceName pWName = pWFactory.Create(parentDirectory, gdbName, propertySet, hwnd);
            return (pWName as IName).Open() as IWorkspace;
        }
    }
}
