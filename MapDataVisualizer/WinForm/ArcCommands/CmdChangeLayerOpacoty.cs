using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;

namespace WinForm.ArcCommands
{
    /// <summary>
    /// Command that works in ArcScene or SceneControl
    /// </summary>
    [Guid("668b339b-32cf-4672-a6c8-da221cf37a46")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("WinForm.ArcCommands.CmdChangeLayerOpacoty")]
    public sealed class CmdChangeLayerOpacoty : BaseCommand
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            SxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            SxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private ISceneHookHelper m_sceneHookHelper = null;

        public CmdChangeLayerOpacoty()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "category"; //localizable text
            base.m_caption = "caption";  //localizable text 
            base.m_message = "This should work in ArcScene/SceneControl";  //localizable text
            base.m_toolTip = "toolTip";  //localizable text
            base.m_name = "category_CmdChangeLayerOpacity";   //unique id, non-localizable (e.g. "MyCategory_MyCommand")

            

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                base.m_bitmap = Resource1.CmdChangeLayerOpacoty;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Attributes
        public override int Bitmap
        {
            get
            {
                return base.m_bitmap.GetHbitmap().ToInt32();
            }
        }
        #endregion



        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            try
            {
                m_sceneHookHelper = new SceneHookHelperClass();
                m_sceneHookHelper.Hook = hook;
                if (m_sceneHookHelper.ActiveViewer == null)
                {
                    m_sceneHookHelper = null;
                }
            }
            catch
            {
                m_sceneHookHelper = null;
            }

            if (m_sceneHookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add CmdChangeLayerOpacoty.OnClick implementation

            if (m_sceneHookHelper == null) return;


        }

        #endregion
    }
}
