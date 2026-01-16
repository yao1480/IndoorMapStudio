// Copyright 2011 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at http://resourcesbeta.arcgis.com/en/help/arcobjects-net/usagerestrictions.htm
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

using MultiPatchExamples.GeometryFactory;
using DataModels;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Analyst3DTools;
using BCE.DataModels;
using FloorConstructor;
//using FloorConstructor;

namespace MultiPatchExamples
{
    public partial class MultiPatchExamples : Form
    {
        private object _missing = Type.Missing;
        private IGraphicsContainer3D _axesGraphicsContainer3D;
        private IGraphicsContainer3D outline_GraphicsContainer3D;
        private IGraphicsContainer3D multiPatch_GraphicsContainer3D;

        public MultiPatchExamples()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            this.TopMost = false;


            _axesGraphicsContainer3D = GraphicsLayer3DUtilities.ConstructGraphicsLayer3D("Axes");
            multiPatch_GraphicsContainer3D = GraphicsLayer3DUtilities.ConstructGraphicsLayer3D("MultiPatch");
            outline_GraphicsContainer3D = GraphicsLayer3DUtilities.ConstructGraphicsLayer3D("Outline");

            GraphicsLayer3DUtilities.DisableLighting(multiPatch_GraphicsContainer3D);

            axSceneControl.Scene.AddLayer(_axesGraphicsContainer3D as ILayer, true);
            axSceneControl.Scene.AddLayer(multiPatch_GraphicsContainer3D as ILayer, true);
            axSceneControl.Scene.AddLayer(outline_GraphicsContainer3D as ILayer, true);

            DrawUtilities.DrawAxes(_axesGraphicsContainer3D);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleStrip1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleStripExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleStrip2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleStripExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleStrip3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleStripExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleStrip4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleStripExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleStrip5Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleStripExamples.GetExample5();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleFan1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleFanExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleFan2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleFanExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleFan3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleFanExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleFan4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleFanExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleFan5Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleFanExamples.GetExample5();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangleFan6Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TriangleFanExamples.GetExample6();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangles1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TrianglesExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangles2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TrianglesExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangles3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TrianglesExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangles4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TrianglesExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangles5Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TrianglesExamples.GetExample5();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void triangles6Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = TrianglesExamples.GetExample6();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }




        private void ring1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ring2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ring3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ring4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ring5Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingExamples.GetExample5();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }



        private void vector3D1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Vector3DExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void vector3D2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Vector3DExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void vector3D3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Vector3DExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void vector3D4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Vector3DExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void vector3D5Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Vector3DExamples.GetExample5();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }



        private void transform3D1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Transform3DExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void transform3D2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Transform3DExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void transform3D3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Transform3DExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void transform3D4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = Transform3DExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }





        //基于线段拉伸出一个面
        private void extrusion1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        //将Polygon拉伸为3D实体
        private void extrusion3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }







        private void extrusion6Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample6();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion7Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample7();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion8Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample8();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusionButton9_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample9();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion10Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample10();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion11Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample11();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion12Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample12();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion13Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample13();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion14Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample14();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void extrusion15Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = ExtrusionExamples.GetExample15();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }




        private void ringGroup1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingGroupExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ringGroup2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingGroupExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ringGroup3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingGroupExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ringGroup4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingGroupExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void ringGroup5Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = RingGroupExamples.GetExample5();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }






        private void composite1Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = CompositeExamples.GetExample1();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void composite2Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = CompositeExamples.GetExample2();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void composite3Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = CompositeExamples.GetExample3();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }

        private void composite4Button_Click(object sender, EventArgs e)
        {
            IGeometry geometry = CompositeExamples.GetExample4();

            DrawUtilities.DrawMultiPatch(multiPatch_GraphicsContainer3D, geometry);
            DrawUtilities.DrawOutline(outline_GraphicsContainer3D, geometry);

            axSceneControl.SceneGraph.RefreshViewers();
        }














        private IPointCollection createPointCollection2D(IPointCollection pPointCollection, double xMin, double xMax, double yMIn, double yMax)
        {
            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint2D(xMin, yMIn), ref _missing, ref _missing);
            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint2D(xMin, yMax), ref _missing, ref _missing);
            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint2D(xMax, yMax), ref _missing, ref _missing);
            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint2D(xMax, yMIn), ref _missing, ref _missing);


            return pPointCollection;
        }


        private IPointCollection createPolygonPointCollection3D(IPointCollection pPointCollection, double xMin, double xMax, double yMIn, double yMax)
        {

            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint3D(xMin, yMIn, 0), ref _missing, ref _missing);
            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint3D(xMin, yMax, 0), ref _missing, ref _missing);
            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint3D(xMax, yMax, 0), ref _missing, ref _missing);
            pPointCollection.AddPoint(GeometryUtilities.ConstructPoint3D(xMax, yMIn, 0), ref _missing, ref _missing);


            return pPointCollection;
        }


        private IGeometry createC(IPoint centre, double radius, IVector3D relativeUpperVector, int divisionNum = 6)
        {

            try
            {
                PolygonClass polygon = new PolygonClass();

                //对圆边进行 点采样
                double papi = 2 * Math.PI / divisionNum;
                for (int i = 0; i < divisionNum + 1; i++)
                {
                    polygon.AddPoint(GeometryUtilities.ConstructPoint3D(radius * Math.Cos(i * papi), radius * Math.Sin(i * papi), 0d));
                }

                //必须使得多边形多Z值敏感，否则无法进行3D平移
                polygon.ZAware = true;



                IGeometry geometry = polygon as IGeometry;
                ITransform3D pTransform3D = geometry as ITransform3D;
                //pTransform3D.RotateVector3D(GeometryUtilities.ConstructVector3D(0,10,0), Math.PI/4);

                ////移动圆
                pTransform3D.Move3D(centre.X, centre.Y, centre.Z);


                return pTransform3D as IGeometry;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                return null;
            }


        }







        private void extrusion5Button_Click(object sender, EventArgs e)
        {
            try
            {

                FloorPlan floorPlan = ConsoleTest.Program.FindCpTest();

                List<IElement> elements = FloorConstructor_3D.Construct_Walls_3D(ref floorPlan);



                multiPatch_GraphicsContainer3D.DeleteAllElements();
                DrawHelper.Draw_Elements(multiPatch_GraphicsContainer3D, elements);
                //DrawHelper.DrwaMultipatchElementOutlines(multiPatch_GraphicsContainer3D, elements);


                axSceneControl.SceneGraph.RefreshViewers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //绘制楼梯
        private void DrawStairs()
        {
            //multiPatch_GraphicsContainer3D.DeleteAllElements();
            //outline_GraphicsContainer3D.DeleteAllElements();

            //StairParameters.RestPlatform restPlatform = new StairParameters.RestPlatform(0.01);
            //StairParameters.StairFlight stairFlight = new StairParameters.StairFlight(0.01, true);
            //StairParameters.Baluster baluster = new StairParameters.Baluster(ref restPlatform, ref stairFlight);
            //StairParameters.HandRail handrail = new StairParameters.HandRail(ref baluster);
            //StairParameters.StairRailing stairRailing = new StairParameters.StairRailing(ref baluster, ref handrail);
            //List<IElement> stairElements = DRunningStairGeneraotr.Construct_DRunningStair(restPlatform, stairFlight, stairRailing);




            //DrawHelper.Draw_Elements(multiPatch_GraphicsContainer3D, stairElements);
            //DrawHelper.DrwaMultipatchElementOutlines(multiPatch_GraphicsContainer3D, stairElements);

        }

        //绘制单扇门
        private void drawSingleDoor()
        {
            //multiPatch_GraphicsContainer3D.DeleteAllElements();


            //DoorParameters.SingleDoorParameters singleDoorParameters = new DoorParameters.SingleDoorParameters();

            //IGeometry doorFrameGeometry = SingleDoorGenerator.construct_DoorPost(ref singleDoorParameters);

            //IElement doorFrameElements = ElementCreater.Contruct_MultiPatchElement(doorFrameGeometry, DoorParameters.Color_Door);

            //List<IElement> elemens = new List<IElement>();
            //elemens.Add(doorFrameElements);

            //DrawHelper.Draw_Elements(multiPatch_GraphicsContainer3D, elemens);
            //DrawHelper.DrwaMultipatchElementOutlines(multiPatch_GraphicsContainer3D, elemens);
        }


        #region Test
        ///<summary>Simple helper to create a featureclass in a geodatabase.</summary>
        /// 
        ///<param name="workspace">An IWorkspace2 interface</param>
        ///<param name="featureDataset">An IFeatureDataset interface or Nothing</param>
        ///<param name="featureClassName">A System.String that contains the name of the feature class to open or create. Example: "states"</param>
        ///<param name="fields">An IFields interface</param>
        ///<param name="CLSID">A UID value or Nothing. Example "esriGeoDatabase.Feature" or Nothing</param>
        ///<param name="CLSEXT">A UID value or Nothing (this is the class extension if you want to reference a class extension when creating the feature class).</param>
        ///<param name="strConfigKeyword">An empty System.String or RDBMS table string for ArcSDE. Example: "myTable" or ""</param>
        ///  
        ///<returns>An IFeatureClass interface or a Nothing</returns>
        ///  
        ///<remarks>
        ///  (1) If a 'featureClassName' already exists in the workspace a reference to that feature class 
        ///      object will be returned.
        ///  (2) If an IFeatureDataset is passed in for the 'featureDataset' argument the feature class
        ///      will be created in the dataset. If a Nothing is passed in for the 'featureDataset'
        ///      argument the feature class will be created in the workspace.
        ///  (3) When creating a feature class in a dataset the spatial reference is inherited 
        ///      from the dataset object.
        ///  (4) If an IFields interface is supplied for the 'fields' collection it will be used to create the
        ///      table. If a Nothing value is supplied for the 'fields' collection, a table will be created using 
        ///      default values in the method.
        ///  (5) The 'strConfigurationKeyword' parameter allows the application to control the physical layout 
        ///      for this table in the underlying RDBMS?for example, in the case of an Oracle database, the 
        ///      configuration keyword controls the tablespace in which the table is created, the initial and 
        ///     next extents, and other properties. The 'strConfigurationKeywords' for an ArcSDE instance are 
        ///      set up by the ArcSDE data administrator, the list of available keywords supported by a workspace 
        ///      may be obtained using the IWorkspaceConfiguration interface. For more information on configuration 
        ///      keywords, refer to the ArcSDE documentation. When not using an ArcSDE table use an empty 
        ///      string (ex: "").
        ///</remarks>
        public ESRI.ArcGIS.Geodatabase.IFeatureClass CreateFeatureClass(ESRI.ArcGIS.Geodatabase.IWorkspace2 workspace, ESRI.ArcGIS.Geodatabase.IFeatureDataset featureDataset, System.String featureClassName, ESRI.ArcGIS.Geodatabase.IFields fields, ESRI.ArcGIS.esriSystem.UID CLSID, ESRI.ArcGIS.esriSystem.UID CLSEXT, System.String strConfigKeyword)
        {
            if (featureClassName == "") return null; // name was not passed in 

            ESRI.ArcGIS.Geodatabase.IFeatureClass featureClass;
            ESRI.ArcGIS.Geodatabase.IFeatureWorkspace featureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)workspace; // Explicit Cast

            if (workspace.get_NameExists(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass, featureClassName)) //feature class with that name already exists 
            {
                featureClass = featureWorkspace.OpenFeatureClass(featureClassName);
                return featureClass;
            }

            // assign the class id value if not assigned
            if (CLSID == null)
            {
                CLSID = new ESRI.ArcGIS.esriSystem.UIDClass();
                CLSID.Value = "esriGeoDatabase.Feature";
            }

            ESRI.ArcGIS.Geodatabase.IObjectClassDescription objectClassDescription = new ESRI.ArcGIS.Geodatabase.FeatureClassDescriptionClass();

            // if a fields collection is not passed in then supply our own
            if (fields == null)
            {
                // create the fields using the required fields method
                fields = objectClassDescription.RequiredFields;
                ESRI.ArcGIS.Geodatabase.IFieldsEdit fieldsEdit = (ESRI.ArcGIS.Geodatabase.IFieldsEdit)fields; // Explicit Cast
                ESRI.ArcGIS.Geodatabase.IField field = new ESRI.ArcGIS.Geodatabase.FieldClass();

                // create a user defined text field
                ESRI.ArcGIS.Geodatabase.IFieldEdit fieldEdit = (ESRI.ArcGIS.Geodatabase.IFieldEdit)field; // Explicit Cast

                // setup field properties
                fieldEdit.Name_2 = "SampleField";
                fieldEdit.Type_2 = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeString;
                fieldEdit.IsNullable_2 = true;
                fieldEdit.AliasName_2 = "Sample Field Column";
                fieldEdit.DefaultValue_2 = "test";
                fieldEdit.Editable_2 = true;
                fieldEdit.Length_2 = 100;

                // add field to field collection
                fieldsEdit.AddField(field);
                fields = (ESRI.ArcGIS.Geodatabase.IFields)fieldsEdit; // Explicit Cast
            }

            System.String strShapeField = "";

            // locate the shape field
            for (int j = 0; j < fields.FieldCount; j++)
            {
                if (fields.get_Field(j).Type == ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeGeometry)
                {
                    strShapeField = fields.get_Field(j).Name;
                }
            }

            // Use IFieldChecker to create a validated fields collection.
            ESRI.ArcGIS.Geodatabase.IFieldChecker fieldChecker = new ESRI.ArcGIS.Geodatabase.FieldCheckerClass();
            ESRI.ArcGIS.Geodatabase.IEnumFieldError enumFieldError = null;
            ESRI.ArcGIS.Geodatabase.IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (ESRI.ArcGIS.Geodatabase.IWorkspace)workspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.


            // finally create and return the feature class
            if (featureDataset == null)// if no feature dataset passed in, create at the workspace level
            {
                featureClass = featureWorkspace.CreateFeatureClass(featureClassName, validatedFields, CLSID, CLSEXT, ESRI.ArcGIS.Geodatabase.esriFeatureType.esriFTSimple, strShapeField, strConfigKeyword);
            }
            else
            {
                featureClass = featureDataset.CreateFeatureClass(featureClassName, validatedFields, CLSID, CLSEXT, ESRI.ArcGIS.Geodatabase.esriFeatureType.esriFTSimple, strShapeField, strConfigKeyword);
            }
            return featureClass;
        }


        #endregion

    }
}




//IPointCollection pPointCollection = new RingClass();
//              (pPointCollection as IZAware).ZAware = true;

//              pPointCollection.AddPoint(GeometryCreater.Construct_Point3D(0, 0, 0));
//              pPointCollection.AddPoint(GeometryCreater.Construct_Point3D(0, 10, 0));
//              pPointCollection.AddPoint(GeometryCreater.Construct_Point3D(10, 10, 0));
//              pPointCollection.AddPoint(GeometryCreater.Construct_Point3D(10, 0, 0));

//              //闭合 Ring
//              (pPointCollection as IRing).Close();

//              //旋转环
//              ITransform3D transform3D = pPointCollection as ITransform3D;
//              transform3D.RotateVector3D(GeometryCreater.Construct_Vector3D(0, 1, 0),89.9*Math.PI/180);

//              //构造 Polygon
//              IGeometryCollection pPolygonGeometryCollection = new PolygonClass();
//              (pPolygonGeometryCollection as IZAware).ZAware = true;

//              IGeometry ringGeometry = pPointCollection as IGeometry;
//              pPolygonGeometryCollection.AddGeometry(ringGeometry);

//              ITopologicalOperator t = pPolygonGeometryCollection as ITopologicalOperator;
//              t.Simplify();

//              IConstructMultiPatch pConstructMultipatch = new MultiPatchClass();
//              (pConstructMultipatch as IZAware).ZAware = true;


//              IVector3D extrudeVector = GeometryCreater.Construct_Vector3D(5, 0, 0.0001);//拉升矢量为Y轴正方向
//              pConstructMultipatch.ConstructExtrudeRelative(extrudeVector, pPolygonGeometryCollection as IGeometry);

//              IGeometry geometry = pConstructMultipatch as IGeometry;


//              DrawUtilities.DrawMultiPatch(_multiPatchGraphicsContainer3D, geometry);
//              DrawUtilities.DrawOutline(_outlineGraphicsContainer3D, geometry);
//              axSceneControl.SceneGraph.RefreshViewers();
