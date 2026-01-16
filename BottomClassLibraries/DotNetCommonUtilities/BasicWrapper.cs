using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCommonUtilities.BasicWrapper
{
    public abstract class FileUtilities
    {

        /// <summary>
        /// 返回文件路径的扩展名
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string Get_Extension(string filePath)
        {
            string fileName = filePath.Trim();
            if (fileName.Length == 0 || fileName == null)
                throw new ArgumentException("无效文件名");

            int index_point = filePath.LastIndexOf(".");
            if (index_point == -1)
                throw new ArgumentException("无效文件名");


            return fileName.Substring(index_point + 1, fileName.Length - index_point - 1);
        }
    }


    /// <summary>
    /// 支持属性更改通知的抽象类,将其用在属性设置的Set中以使其支持属性更改通知
    /// </summary>
    public abstract class BindableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


}
