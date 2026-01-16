using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels.Basic;
using DotNetCommonUtilities.BasicWrapper;
using DXFReader.DataModel;


namespace BCE.DataModels
{
    public class FloorPlan : BindableObject
    {
        public ParametersConfiguration Configuration;

        /// <summary>
        /// 从DxfReader读取后经过信息分割等处理的数据
        /// </summary>
        public Data PData;

        /// <summary>
        /// 图纸解析成果
        /// </summary>
        public Result PResult;

        public FloorPlan(ref ParametersConfiguration configuration)
        {
            this.Configuration = configuration;

            this.PData = new Data();
            this.PResult = new Result();
        }
    }
}
