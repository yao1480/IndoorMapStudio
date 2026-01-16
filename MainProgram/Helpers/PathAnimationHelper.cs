using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Animation;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using Microsoft.Win32;

namespace MainProgram.Helpers
{
    public class PathAnimationHelper
    {
        IAGAnimationUtils m_AGAnimationUtils;//动画常用工具接口
        IAGImportPathOptions m_AGImportPathOptions;

        IAGAnimationPlayer m_AGAnimationPlayer;
        public IAGAnimationEnvironment M_AGAnimationEnvironment;//动画播放参数



        ISceneControl m_sceneControl;
        IAGAnimationTracks m_AGAniamationTracks;

        public PathAnimationHelper(ISceneControl pSceneControl)
        {
            m_AGAnimationUtils = new AGAnimationUtilsClass();
            m_AGImportPathOptions = new AGImportPathOptionsClass();


            m_AGAnimationPlayer = m_AGAnimationUtils as IAGAnimationPlayer;
            M_AGAnimationEnvironment = new AGAnimationEnvironmentClass();
            m_AGImportPathOptions.AnimationEnvironment = M_AGAnimationEnvironment;


            m_sceneControl = pSceneControl;
            m_AGAniamationTracks = m_sceneControl.Scene as IAGAnimationTracks;
        }

        /// <summary>
        /// 动画播放
        /// </summary>
        /// <param name="pPalyMode"></param>
        /// <param name="pPalyDurationInSeconds"></param>
        public void PlayAnimation(esriAnimationPlayMode pPalyMode, double pPalyDurationInSeconds)
        {
            if (m_AGAniamationTracks.TrackCount != 1) return;

            M_AGAnimationEnvironment.PlayMode = pPalyMode;
            M_AGAnimationEnvironment.AnimationDuration = pPalyDurationInSeconds;

            m_AGAnimationPlayer.PlayAnimation(m_AGAniamationTracks, M_AGAnimationEnvironment, null);
        }

        /// <summary>
        /// 暂停动画
        /// </summary>
        public void PauseAnimation()
        {
            m_AGAnimationPlayer.PauseAnimation();
        }

        /// <summary>
        /// 停止动画
        /// </summary>
        public void StopAnimation()
        {
            m_AGAnimationPlayer.StopAnimation();
        }

        public void ExportAnimationToVideo()
        {
            if (m_AGAniamationTracks == null || m_AGAniamationTracks.TrackCount < 1)
            {
                MessageBox.Show("尚未创建动画", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "AVI(*.avi)|*.avi";
            sfd.Title = "动画导出路径设置";
            if (sfd.ShowDialog() == true)
            {
                IVideoExporter2 videoExporter = new AnimationExporterAVIClass();
                videoExporter.ExportFileName = sfd.FileName;
                videoExporter.Codec = "CVID";
                videoExporter.ExportAnimation(m_AGAniamationTracks, M_AGAnimationEnvironment, null);

                MessageBox.Show("动画导出成功", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }

          
        }

        /// <summary>
        /// 创建路径动画
        /// </summary>
        /// <param name="pAnimationRoute"></param>
        public void CreateRouteAnimation(IPolyline pAnimationRoute, double cameraVerticalOffset)
        {
            //移除现有动画
            if (m_AGAniamationTracks.TrackCount > 0)
                m_AGAniamationTracks.RemoveAllTracks();

            //配置路径动画参数
            m_AGImportPathOptions.TrackName = "Path Animation";//动画轨迹名

            m_AGImportPathOptions.BasicMap = m_sceneControl.Scene as IBasicMap;
            m_AGImportPathOptions.AnimationType = new AnimationTypeCameraClass();//动画类型：相机动画
            m_AGImportPathOptions.AnimatedObject = m_sceneControl.SceneViewer.Camera;
            m_AGImportPathOptions.PathGeometry = pAnimationRoute;//相机路径
            m_AGImportPathOptions.SimplificationFactor = 0d;//禁止路径简化，这将保证路径动画将严格沿着路径前进
            m_AGImportPathOptions.LookaheadFactor = 0d;//禁止相机前瞻，这将避免相机提前转弯
            m_AGImportPathOptions.ConversionType = esriFlyFromPathType.esriFlyFromPathObsAndTarget;//同时改变观察点和目标点
            m_AGImportPathOptions.OverwriteExisting = true;//覆盖现有动画
            m_AGImportPathOptions.VerticalOffset = cameraVerticalOffset;


            //计算路径的相对角度
            m_AGImportPathOptions.PutAngleCalculationMethods(
                esriPathAngleCalculation.esriAngleAddRelative,
                esriPathAngleCalculation.esriAngleAddRelative,
                esriPathAngleCalculation.esriAngleAddRelative);

            m_AGImportPathOptions.PutAngleCalculationValues(0.0, 0.0, 0.0);

            double pAzimuth, pInclination, pRollVal;
            m_AGImportPathOptions.GetAngleCalculationValues(out pAzimuth, out pInclination, out pRollVal);
   




            //创建路径动画
            m_AGAnimationUtils.CreateFlybyFromPath(m_sceneControl.Scene as IAGAnimationContainer, m_AGImportPathOptions);
        }
    }
}
