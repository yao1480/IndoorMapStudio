using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.BCExtractors;
using BCE.DataModels;
using BCE.DataModels.Basic;
using DXFReader.DataModel;

namespace BCE
{
    public class BuildingComponentModeler
    {
        public static void Model(ref FloorPlan floorPlan)
        {
            //Result result = floorPlan.PResult;

            ////step1:提取所有电梯参数（依赖电梯间轮廓）
            //EElevator.Extract(ref result);

            ////step2:提取所有的楼梯参数(依赖电梯间轮廓)
            //EStair.Extract(ref floorPlan);

            ////step3:对门窗+电梯+楼梯进行参数化建模
        }

        public static ExtructError ParseDrawing( FloorPlan floorPlan, DXFReader.Reader.DxfReader dxfReader)
        {
            //在此之前需要配置好图纸

            InfoSegementation.SegementInfo(ref floorPlan, ref dxfReader);
            return FloorFrameModeler.Model(ref floorPlan);
        }
    }


}
