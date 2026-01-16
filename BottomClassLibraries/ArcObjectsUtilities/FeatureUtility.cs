using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace ArcObjectsUtilities
{
    public abstract class FeatureUtility
    {
        public static IFeatureClass Create_FeatureClass_InMemory(string featureClassName,IFields pFields=null)
        {
            return null;

            //IWorkspaceFactory pWorkspaceFactory = new InMemoryWorkspaceFactory();

            //IWorkspaceName pWorkspaceName = pWorkspaceFactory.Create(null,"myWorksapce",null,0);
            
            //IFeatureWorkspace pFeatureWorksapce=(pWorkspaceName as IName).Open() as IFeatureWorkspace;

            //if(pFields==null)
            //{
            //    IFeatureClassDescription pFCD=new FeatureClassDescriptionClass();
            //    IObjectClassDescription pOCD=pFCD as  IObjectClassDescription;
            //    pFields=new FieldsClass();
            //    pFields=pOCD.RequiredFields;
            //}
            


            //IFeatureClass pFeatureClass=pFeatureWorksapce.CreateFeatureClass(featureClassName,pFields,null,null,esriFeatureType.esriFTSimple)


            //            Public Shared Function GetSimpleFeatureClass(ByVal pFields As IFields, ByVal featureClassName As String) As IFeatureClass

            //Try


            //Dim pSwf As IWorkspaceFactory = New InMemoryWorkspaceFactory


            //Dim pWorkspaceName As IWorkspaceName = pSwf.Create("", "MyWorkspace", Nothing, 0) Dim pFWS As IFeatureWorkspace = CType(pWorkspaceName, IName).Open()
            //Dim pFC As IFeatureClass = pFWS.CreateFeatureClass(featureClassName, pFields, Nothing, Nothing, esriFeatureType.esriFTSimple, "Shape", Nothing)

            //Return pFC


            //Catch ex As Exception


            //Return Nothing


            //End Try


            //End Function
        }
    }
}
