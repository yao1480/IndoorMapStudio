using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace ArcEngineDataUtilities.DataOperation
{
   public abstract class FileGDBUtility
    {
        /// <summary>
        /// 检查指定路径是否是有效（检查文件夹存在性、FGDB有效性、且不含其他GDB）的文件地理数据库
        /// </summary>
        /// <param name="fgdbPath">地理数据库文件夹路径</param>
        public static void CheckFGDB(string fgdbPath)
        {
            //检查文件夹是否存在
            if (!System.IO.Directory.Exists(fgdbPath))
                throw new ArgumentException(string.Format("未找到地理数据库(查找路径：{0})", fgdbPath));

            //检查文件夹是否是有效的地理数据库
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            if (!pWorkspaceFactory.IsWorkspace(fgdbPath))
                throw new ArgumentException(string.Format("地理数据库无效(模板路径：{0})", fgdbPath));
            

            //检查文件夹是否包含其他地理数据库
                        string[] fileNames = System.IO.Directory.GetDirectories(fgdbPath);
            IFileNames pFileNames = new FileNamesClass();
            foreach (var item in fileNames)
            {
                pFileNames.Add(item);
            }

            if (pWorkspaceFactory.ContainsWorkspace(fgdbPath, pFileNames))
                throw new ArgumentException(string.Format("位置 {0} 不允许包含其他地理数据库！",fgdbPath));
        }

       /// <summary>
       /// 将现有FGDB拷贝到指定路径
       /// </summary>
       /// <param name="sourceParentDirectory"></param>
       /// <param name="destinationParentDirectory"></param>
       /// <returns></returns>
        public static IWorkspaceName CopyFGDB(string sourceFGDB, string destinationParentDirectory)
        {
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();

            string sourceParentDirectory = System.IO.Directory.GetParent(sourceFGDB).ToString();
            string[] fileNames = System.IO.Directory.GetDirectories(sourceParentDirectory);
            IFileNames pFileNames = new FileNamesClass();
            foreach (var item in fileNames)
            {
                pFileNames.Add(item);
            }

            IWorkspaceName pSourceWorksapceName = pWorkspaceFactory.GetWorkspaceName(sourceParentDirectory, pFileNames);
            IWorkspaceName pToWorkspaceName = new WorkspaceNameClass();
            bool result = pWorkspaceFactory.Copy(pSourceWorksapceName, destinationParentDirectory, out pToWorkspaceName);

            if (result == false)
                throw new ArgumentException("复制地理数据库模板失败！");

            return pToWorkspaceName;
        }
    }
}
