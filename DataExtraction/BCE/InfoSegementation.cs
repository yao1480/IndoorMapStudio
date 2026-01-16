using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCE.DataModels;
using BCE.DataModels.Basic;
using BCE.Helpers;
using DXFReader.DataModel;

namespace BCE
{
    public abstract class InfoSegementation
    {
        const double threshold_scale_forInserts = 1d;//过滤无效插入体的阈值(通过缩放影子判断插入体大小)

        public static void SegementInfo(ref FloorPlan floorPan, ref DXFReader.Reader.DxfReader dxfReader)
        {
            Data data = floorPan.PData;
            ParametersConfiguration config = floorPan.Configuration;

            data.WallLines = ClusterHelper.Lines_ByLayerAndMinLength(ref dxfReader.Lines, config.LayerMapping.Wall, 0.001d);
            data.BalcLines = ClusterHelper.Lines_ByLayerAndMinLength(ref dxfReader.Lines, config.LayerMapping.Balcony, 0.001f);
            data.ElevLines = ClusterHelper.Lines_ByLayerAndMinLength(ref dxfReader.Lines, config.LayerMapping.Elevator, 0.001f);
            data.StairLines = ClusterHelper.Lines_ByLayerAndMinLength(ref dxfReader.Lines, config.LayerMapping.Stairs, 0.001f);
            data.Annotations = ClusterHelper.GetTexts_ByLayer(ref dxfReader.Texts, config.LayerMapping.Text);
            data.Inserts_Door = ClusterHelper.Inserts_ByLayer(ref dxfReader.Inserts, config.LayerMapping.Door);
            data.Inserts_Window = ClusterHelper.Inserts_ByLayer(ref dxfReader.Inserts, config.LayerMapping.Window);
            data.Blocks_Door = ClusterHelper.Blocks_ByRelatedInserts(ref dxfReader.Blocks, ref data.Inserts_Door);
            data.Blocks_Window = ClusterHelper.Blocks_ByRelatedInserts(ref dxfReader.Blocks, ref data.Inserts_Window);
        }
    }
}
