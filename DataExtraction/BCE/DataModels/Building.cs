using BCE.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCE.DataModels
{
    public class Building 
    {
        public string BuildingName;
    
        public string Address { get; set; }
        public string Remark { get; set; }


        public List<FloorPlan> FeaturePlans;


        #region 构造器
        public Building()
        {
            FeaturePlans = new List<FloorPlan>();
        }
        #endregion
    }
}
