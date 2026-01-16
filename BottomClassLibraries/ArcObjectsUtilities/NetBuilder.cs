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
    public abstract class NetBuilder
    {
        //public static void BuildNetDataset()
        //{
        //    #region step1: 创建空的网络数据集，并设置空间参考+范围+名称
        //    // Create an empty data element for a buildable network dataset.
        //    IDENetworkDataset2 deNetworkDataset = new DENetworkDatasetClass();
        //    deNetworkDataset.Buildable = true;

        //    // Open the feature dataset and cast to the IGeoDataset interface.
        //    Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
        //    IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
        //    IWorkspace workspace = workspaceFactory.OpenFromFile(@"C:\Users\silence\Desktop\SanFrancisco\SanFrancisco.gdb", 0);
        //    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
        //    IFeatureDataset featureDataset = featureWorkspace.OpenFeatureDataset("Transportation");
        //    IGeoDataset geoDataset = (IGeoDataset)featureDataset;

        //    // Copy the feature dataset's extent and spatial reference to the network dataset data element.
        //    IDEGeoDataset deGeoDataset = (IDEGeoDataset)deNetworkDataset;
        //    deGeoDataset.Extent = geoDataset.Extent;
        //    deGeoDataset.SpatialReference = geoDataset.SpatialReference;

        //    // Specify the name of the network dataset.
        //    IDataElement dataElement = (IDataElement)deNetworkDataset;
        //    dataElement.Name = "Streets_ND";
        //    #endregion


        //    #region step2: Specifying connectivity settings for the edge source
        //    //  高程模型   Specify the network dataset's elevation model.
        //    deNetworkDataset.ElevationModel = esriNetworkElevationModel.esriNEMElevationFields;

        //    // Create an EdgeFeatureSource object and point it to the Streets feature class.
        //    INetworkSource edgeNetworkSource = new EdgeFeatureSourceClass();
        //    edgeNetworkSource.Name = "Streets";
        //    edgeNetworkSource.ElementType = esriNetworkElementType.esriNETEdge;

        //    // Set the edge feature source's connectivity settings.
        //    IEdgeFeatureSource edgeFeatureSource = (IEdgeFeatureSource)edgeNetworkSource;
        //    edgeFeatureSource.UsesSubtypes = false;

        //    //连通性组
        //    edgeFeatureSource.ClassConnectivityGroup = 1;

        //    //连通策略
        //    edgeFeatureSource.ClassConnectivityPolicy = esriNetworkEdgeConnectivityPolicy.esriNECPEndVertex;//端点连通性

        //    //高程字段
        //    edgeFeatureSource.FromElevationFieldName = "F_ELEV";
        //    edgeFeatureSource.ToElevationFieldName = "T_ELEV";
        //    #endregion


        //    #region step3: Specifying directions settings for the edge source
        //    // Create a StreetNameFields object and populate its settings.
        //    IStreetNameFields streetNameFields = new StreetNameFieldsClass();
        //    streetNameFields.Priority = 1; // Priority 1 indicates the primary street name.
        //    streetNameFields.StreetNameFieldName = "NAME";

        //    // Add the StreetNameFields object to a new NetworkSourceDirections object,
        //    // then add it to the EdgeFeatureSource created earlier.
        //    INetworkSourceDirections nsDirections = new NetworkSourceDirectionsClass();
        //    IArray nsdArray = new ArrayClass();
        //    nsdArray.Add(streetNameFields);
        //    nsDirections.StreetNameFields = nsdArray;
        //    edgeNetworkSource.NetworkSourceDirections = nsDirections;
        //    #endregion

        //    #region step3: Specifying the turn source
        //    deNetworkDataset.SupportsTurns = true;

        //    // Create a TurnFeatureSource object and point it to the RestrictedTurns feature class.
        //    INetworkSource turnNetworkSource = new TurnFeatureSourceClass();
        //    turnNetworkSource.Name = "RestrictedTurns";
        //    turnNetworkSource.ElementType = esriNetworkElementType.esriNETTurn;


        //    IArray sourceArray = new ArrayClass();
        //    sourceArray.Add(edgeNetworkSource);
        //    sourceArray.Add(turnNetworkSource);
        //    deNetworkDataset.Sources = sourceArray;
        //    #endregion

        //    #region step5: Adding the traffic data tables
        //    // Create a new TrafficData object and populate its historical traffic settings.
        //    var traffData = new TrafficDataClass() as ITrafficData2;
        //    traffData.LengthAttributeName = "Meters";

        //    var histTraff = traffData as IHistoricalTrafficData2;

        //    // Populate the speed profile table settings.
        //    histTraff.ProfilesTableName = "DailyProfiles";
        //    histTraff.FirstTimeSliceFieldName = "SpeedFactor_0000";
        //    histTraff.LastTimeSliceFieldName = "SpeedFactor_2355";
        //    histTraff.TimeSliceDurationInMinutes = 5;
        //    histTraff.FirstTimeSliceStartTime = new DateTime(1, 1, 1, 0, 0, 0); // 12 AM

        //    // Note: the last time slice finish time is implied from the above settings and need not be specified.
        //    // Populate the street-speed profile join table settings.
        //    histTraff.JoinTableName = "Streets_DailyProfiles";
        //    histTraff.JoinTableBaseSpeedFieldName = "SPFREEFLOW";
        //    histTraff.JoinTableBaseSpeedUnits =
        //        esriNetworkAttributeUnits.esriNAUKilometersPerHour;
        //    IStringArray fieldNames = new NamesClass();
        //    fieldNames.Add("PROFILE_1");
        //    fieldNames.Add("PROFILE_2");
        //    fieldNames.Add("PROFILE_3");
        //    fieldNames.Add("PROFILE_4");
        //    fieldNames.Add("PROFILE_5");
        //    fieldNames.Add("PROFILE_6");
        //    fieldNames.Add("PROFILE_7");
        //    histTraff.JoinTableProfileIDFieldNames = fieldNames;

        //    // Add the traffic data to the network dataset data element.
        //    deNetworkDataset.TrafficData = (ITrafficData)histTraff;
        //    #endregion

        //    #region step6: Adding network attributes
        //    IArray attributeArray = new ArrayClass();

        //    // Initialize variables reused when creating attributes:
        //    IEvaluatedNetworkAttribute evalNetAttr;
        //    INetworkAttribute2 netAttr2;
        //    INetworkFieldEvaluator netFieldEval;
        //    INetworkConstantEvaluator netConstEval;

        //    #region Oneway network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "Oneway";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTRestriction;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTBoolean;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUUnknown;
        //    netAttr2.UseByDefault = true;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("restricted", "restricted = False\n\r" +
        //        "Select Case UCase([ONEWAY])\n\r" +
        //        "  Case \"N\", \"TF\", \"T\": restricted = True\n\r" + "End Select");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);

        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("restricted", "restricted = False\n\r" +
        //        "Select Case UCase([ONEWAY])\n\r" +
        //        "  Case \"N\", \"FT\", \"F\": restricted = True\n\r" + "End Select");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)
        //        netFieldEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = false; // False = traversable.
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = false;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = false;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region Minutes network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "Minutes";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUMinutes;
        //    netAttr2.UseByDefault = false;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[MINUTES]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);

        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[MINUTES]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)netFieldEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge, (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region Meters network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "Meters";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUMeters;
        //    netAttr2.UseByDefault = false;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[METERS]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);

        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[METERS]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)
        //        netFieldEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region RoadClass network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "RoadClass";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTDescriptor;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTInteger;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUUnknown;
        //    netAttr2.UseByDefault = false;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("rc", "rc = 1          'Local road\n\r" +
        //        "If [FEATTYP] = 4130 Then\n\r" + "  rc = 4          'Ferry\n\r" + "Else\n\r" +
        //        "  Select Case [FOW]\n\r" + "    Case 1: rc = 2          'Highway\n\r" +
        //        "    Case 10: rc = 3          'Ramp\n\r" +
        //        "    Case 4: rc = 5          'Roundabout\n\r" + "  End Select\n\r" + "End If");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);

        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("rc", "rc = 1 'Local road\n\r" +
        //        "If [FEATTYP] = 4130 Then\n\r" + " rc = 4 'Ferry\n\r" + "Else\n\r" +
        //        " Select Case [FOW]\n\r" + " Case 1: rc = 2 'Highway\n\r" +
        //        " Case 10: rc = 3 'Ramp\n\r" + " Case 4: rc = 5 'Roundabout\n\r" +
        //        " End Select\n\r" + "End If");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)
        //        netFieldEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region WeekdayFallbackTravelTime network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "WeekdayFallbackTravelTime";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUMinutes;
        //    netAttr2.UseByDefault = false;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[FT_WeekdayMinutes]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[TF_WeekdayMinutes]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)
        //        netFieldEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region WeekendFallbackTravelTime network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "WeekendFallbackTravelTime";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUMinutes;
        //    netAttr2.UseByDefault = false;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[FT_WeekendMinutes]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("[TF_WeekendMinutes]", "");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)
        //        netFieldEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region TravelTime network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "TravelTime";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUMinutes;
        //    netAttr2.UseByDefault = true;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    IHistoricalTravelTimeEvaluator histTravelTimeEval = new
        //        NetworkEdgeTrafficEvaluatorClass();
        //    histTravelTimeEval.WeekdayFallbackAttributeName = "WeekdayFallbackTravelTime";
        //    histTravelTimeEval.WeekendFallbackAttributeName = "WeekendFallbackTravelTime";
        //    histTravelTimeEval.TimeNeutralAttributeName = "Minutes";
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)
        //        histTravelTimeEval);

        //    histTravelTimeEval = new NetworkEdgeTrafficEvaluatorClass();
        //    histTravelTimeEval.WeekdayFallbackAttributeName = "WeekdayFallbackTravelTime";
        //    histTravelTimeEval.WeekendFallbackAttributeName = "WeekendFallbackTravelTime";
        //    histTravelTimeEval.TimeNeutralAttributeName = "Minutes";
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)
        //        histTravelTimeEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region RestrictedTurns network attribute
        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "RestrictedTurns";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTRestriction;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTBoolean;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUUnknown;
        //    netAttr2.UseByDefault = true;

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = true;
        //    evalNetAttr.set_Evaluator(turnNetworkSource, esriNetworkEdgeDirection.esriNEDNone,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = false;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = false;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = false;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);
        //    #endregion

        //    #region HierarchyMultiNet network attribute


        //    // Create an EvaluatedNetworkAttribute object and populate its settings.
        //    evalNetAttr = new EvaluatedNetworkAttributeClass();
        //    netAttr2 = (INetworkAttribute2)evalNetAttr;
        //    netAttr2.Name = "HierarchyMultiNet";
        //    netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTHierarchy;
        //    netAttr2.DataType = esriNetworkAttributeDataType.esriNADTInteger;
        //    netAttr2.Units = esriNetworkAttributeUnits.esriNAUUnknown;
        //    netAttr2.UseByDefault = true;

        //    // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("h", "h = [NET2CLASS] + 1\n\r" + "if h > 5 then h = 5");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);

        //    netFieldEval = new NetworkFieldEvaluatorClass();
        //    netFieldEval.SetExpression("h", "h = [NET2CLASS] + 1\n\r" + "if h > 5 then h = 5");
        //    evalNetAttr.set_Evaluator(edgeNetworkSource,
        //        esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)
        //        netFieldEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction,
        //        (INetworkEvaluator)netConstEval);

        //    netConstEval = new NetworkConstantEvaluatorClass();
        //    netConstEval.ConstantValue = 0;
        //    evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn,
        //        (INetworkEvaluator)netConstEval);

        //    // Add the attribute to the array.
        //    attributeArray.Add(evalNetAttr);

        //    // Since this is the hierarchy attribute, also set it as the hierarchy cluster attribute.
        //    deNetworkDataset.HierarchyClusterAttribute = (INetworkAttribute)evalNetAttr;

        //    // Specify the ranges for the hierarchy levels.
        //    deNetworkDataset.HierarchyLevelCount = 3;
        //    deNetworkDataset.set_MaxValueForHierarchy(1, 2); // level 1: up to 2
        //    deNetworkDataset.set_MaxValueForHierarchy(2, 4); // level 2: 3 - 4
        //    deNetworkDataset.set_MaxValueForHierarchy(3, 5);
        //    // level 3: 5 and higher (the values of h only go up to 5)


        //    #endregion
        //    #endregion

        //    #region step7: Specifying directions settings
        //    #region General directions settings
        //    // Create a NetworkDirections object and populate its settings.
        //    INetworkDirections networkDirections = new NetworkDirectionsClass();
        //    networkDirections.DefaultOutputLengthUnits = esriNetworkAttributeUnits.esriNAUMiles;
        //    networkDirections.LengthAttributeName = "Meters";
        //    networkDirections.TimeAttributeName = "Minutes";
        //    networkDirections.RoadClassAttributeName = "RoadClass";
        //    ISignposts netDirSignposts = (ISignposts)networkDirections;
        //    netDirSignposts.SignpostFeatureClassName = "Signposts";
        //    netDirSignposts.SignpostStreetsTableName = "Signposts_Streets";

        //    // Add the NetworkDirections object to the network dataset data element.
        //    deNetworkDataset.Directions = networkDirections;
        //    #endregion
        //    #endregion

        //    #region Creating and building the network dataset
        //    // Get the feature dataset extension and create the network dataset based on the data element.
        //    IFeatureDatasetExtensionContainer fdxContainer = (IFeatureDatasetExtensionContainer)
        //        featureDataset;
        //    IFeatureDatasetExtension fdExtension = fdxContainer.FindExtension
        //        (esriDatasetType.esriDTNetworkDataset);
        //    IDatasetContainer2 datasetContainer2 = (IDatasetContainer2)fdExtension;
        //    IDEDataset deDataset = (IDEDataset)deNetworkDataset;
        //    INetworkDataset networkDataset = (INetworkDataset)datasetContainer2.CreateDataset
        //        (deDataset);

        //    // Once the network dataset is created, build it.
        //    INetworkBuild networkBuild = (INetworkBuild)networkDataset;
        //    networkBuild.BuildNetwork(geoDataset.Extent);
        //    #endregion
        //}

        public static void Create_NetDataSet(string fileGDBPath)
        {
            #region step1: 创建空的网络数据集，并设置空间参考+范围+名称
            // Create an empty data element for a buildable network dataset.
            IDENetworkDataset2 deNetworkDataset = new DENetworkDatasetClass();
            deNetworkDataset.Buildable = true;
            deNetworkDataset.NetworkType = esriNetworkDatasetType.esriNDTGeodatabase;

            // Open the feature dataset and cast to the IGeoDataset interface.
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            IWorkspace workspace = workspaceFactory.OpenFromFile(fileGDBPath, 0);
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            IFeatureDataset featureDataset = featureWorkspace.OpenFeatureDataset("MapData3D");
            IGeoDataset geoDataset = (IGeoDataset)featureDataset;

            // Copy the feature dataset's extent and spatial reference to the network dataset data element.
            IDEGeoDataset deGeoDataset = (IDEGeoDataset)deNetworkDataset;
            deGeoDataset.Extent = geoDataset.Extent;
            deGeoDataset.SpatialReference = geoDataset.SpatialReference;

            // Specify the name of the network dataset.
            IDataElement dataElement = (IDataElement)deNetworkDataset;
            dataElement.Name = "Route_ND";
            #endregion

            #region step2: Specifying connectivity settings for the edge source
            //  高程模型   Specify the network dataset's elevation model.
            deNetworkDataset.ElevationModel = esriNetworkElevationModel.esriNEMZCoordinates;

            // Create an EdgeFeatureSource object and point it to the Streets feature class.
            INetworkSource edgeNetworkSource = new EdgeFeatureSourceClass();
            edgeNetworkSource.Name = "Sides3D";
            edgeNetworkSource.ElementType = esriNetworkElementType.esriNETEdge;

            // Set the edge feature source's connectivity settings.
            IEdgeFeatureSource edgeFeatureSource = (IEdgeFeatureSource)edgeNetworkSource;
            edgeFeatureSource.UsesSubtypes = false;

            //连通性组
            edgeFeatureSource.ClassConnectivityGroup = 1;

            //连通策略
            edgeFeatureSource.ClassConnectivityPolicy = esriNetworkEdgeConnectivityPolicy.esriNECPAnyVertex;//节点连通性

            ////高程字段
            //edgeFeatureSource.FromElevationFieldName = "F_ELEV";
            //edgeFeatureSource.ToElevationFieldName = "T_ELEV";
            #endregion

            #region step3: 设置距离成本属性
            IArray attributeArray = new ArrayClass();


            IEvaluatedNetworkAttribute evalNetAttr;
            INetworkAttribute2 netAttr2;
            INetworkFieldEvaluator netFieldEval;
            INetworkConstantEvaluator netConstEval;

            // Create an EvaluatedNetworkAttribute object and populate its settings.
            evalNetAttr = new EvaluatedNetworkAttributeClass();
            netAttr2 = (INetworkAttribute2)evalNetAttr;
            netAttr2.Name = "Dist";
            netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
            netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
            netAttr2.Units = esriNetworkAttributeUnits.esriNAUUnknown;
            netAttr2.UseByDefault = false;

            // Create evaluator objects and set them on the EvaluatedNetworkAttribute object.
            //netFieldEval = new NetworkFieldEvaluatorClass();
            //netFieldEval.SetExpression("[Length]", "");
            //evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAlongDigitized, (INetworkEvaluator)netFieldEval);

            //netFieldEval = new NetworkFieldEvaluatorClass();
            //netFieldEval.SetExpression("[Length]", "");
            //evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAgainstDigitized, (INetworkEvaluator)netFieldEval);

            netConstEval = new NetworkConstantEvaluatorClass();
            netConstEval.ConstantValue = 0;
            evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge, (INetworkEvaluator)netConstEval);

            netConstEval = new NetworkConstantEvaluatorClass();
            netConstEval.ConstantValue = 0;
            evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction, (INetworkEvaluator)netConstEval);

            netConstEval = new NetworkConstantEvaluatorClass();
            netConstEval.ConstantValue = 0;
            evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn, (INetworkEvaluator)netConstEval);

            // Add the attribute to the array.
            attributeArray.Add(evalNetAttr);

            #endregion

            #region Creating and building the network dataset
            // Get the feature dataset extension and create the network dataset based on the data element.
            IFeatureDatasetExtensionContainer fdxContainer = (IFeatureDatasetExtensionContainer)featureDataset;
            IFeatureDatasetExtension fdExtension = fdxContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
            IDatasetContainer2 datasetContainer2 = (IDatasetContainer2)fdExtension;
            IDEDataset deDataset = (IDEDataset)deNetworkDataset;

            deNetworkDataset.Attributes = attributeArray;
            IArray pArr = new ArrayClass();
            pArr.Add(edgeFeatureSource);
            deNetworkDataset.Sources = pArr;
            //deNetworkDataset.Sources.Add(edgeFeatureSource);
            INetworkDataset networkDataset = (INetworkDataset)datasetContainer2.CreateDataset(deDataset);


            // Once the network dataset is created, build it.
            INetworkBuild networkBuild = (INetworkBuild)networkDataset;

            //networkBuild.AddSource(edgeNetworkSource);
            networkBuild.BuildNetwork(geoDataset.Extent);
            #endregion
        }


        public static void BuildNetDataSet(string fgdbPath, string dataSetName)
        {
            #region 创建IDENetworkDataset
            IDENetworkDataset2 deNetworkdataset = null;
            IArray pSources = new ArrayClass();//源集
            IArray pAttributes = new ArrayClass();  //属性集



            #region step1: 设置源集(只设置边源，点源和转弯可以不设置)
            INetworkSource pNetWorkSource = new EdgeFeatureSourceClass();
            pNetWorkSource.Name = "Sides3D";//AE将在数据集内部按此名称查找要素类
            pNetWorkSource.ElementType = esriNetworkElementType.esriNETEdge;//边源

            IEdgeFeatureSource edgeFeatureSource = pNetWorkSource as IEdgeFeatureSource;
            edgeFeatureSource.ClassConnectivityGroup = 1;//连通性组
            edgeFeatureSource.ClassConnectivityPolicy = esriNetworkEdgeConnectivityPolicy.esriNECPAnyVertex;//任意节点连通
            edgeFeatureSource.UsesSubtypes = false;//不使用子类型

            pSources.Add(edgeFeatureSource);
            #endregion


            #region step2: 设置网络属性(仅添加距离成本属性)
            INetworkAttribute3 netWorkAttribute = new EvaluatedNetworkAttributeClass();

            //赋值器有4种，作用是为网络的边设置权值
            INetworkFieldEvaluator2 netWorkFieldEvaluator = null;//字段赋值器
            INetworkConstantEvaluator networkConstantEvaluator = null;//常量赋值器


            netWorkAttribute.Name = "Distance";//网络属性名
            netWorkAttribute.DataType = esriNetworkAttributeDataType.esriNADTDouble;
            netWorkAttribute.Units = esriNetworkAttributeUnits.esriNAUUnknown;
            netWorkAttribute.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;//使用类型为成本
            netWorkAttribute.UseByDefault = true;

            //设置边属性赋值器(字段赋值器不能设为默认赋值器 Cannot assign a field evaluator as a default evalautor.)
            IEvaluatedNetworkAttribute2 evaluatedNetworkAttribute = netWorkAttribute as IEvaluatedNetworkAttribute2;
            netWorkFieldEvaluator = new NetworkFieldEvaluatorClass();
            netWorkFieldEvaluator.SetExpression("[Shape_Length]", "");//从字段Shape_Length为边的权供值
            evaluatedNetworkAttribute.set_Evaluator(pNetWorkSource, esriNetworkEdgeDirection.esriNEDAlongDigitized, netWorkFieldEvaluator as INetworkEvaluator);


            /*网络元素的属性赋值器是必须具备的，因此必须设置默认的属性赋值器
             */

            //设置默认的边属性赋值器
            networkConstantEvaluator = new NetworkConstantEvaluatorClass();
            networkConstantEvaluator.ConstantValue = 0d;
            evaluatedNetworkAttribute.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge, networkConstantEvaluator as INetworkEvaluator);

            //设置默认的交汇点属性赋值器
            networkConstantEvaluator = new NetworkConstantEvaluatorClass();
            networkConstantEvaluator.ConstantValue = 0d;
            evaluatedNetworkAttribute.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction, networkConstantEvaluator as INetworkEvaluator);

            //设置默认的转弯属性赋值器
            networkConstantEvaluator = new NetworkConstantEvaluatorClass();
            networkConstantEvaluator.ConstantValue = 0d;
            evaluatedNetworkAttribute.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn, networkConstantEvaluator as INetworkEvaluator);

            pAttributes.Add(evaluatedNetworkAttribute);
            #endregion


            #region step3: 创建网络数据集
            deNetworkdataset = new DENetworkDatasetClass();
            deNetworkdataset.NetworkType = esriNetworkDatasetType.esriNDTGeodatabase;//设置网络数据集的类型
            deNetworkdataset.Buildable = true;
            deNetworkdataset.Sources = pSources;
            deNetworkdataset.Attributes = pAttributes;
            deNetworkdataset.ElevationModel = esriNetworkElevationModel.esriNEMZCoordinates;//高程模型设为使用自身几何



            #endregion





            #endregion

            #region 构件网络数据集
            INetworkDataset pNetWorkDataset = null;//网络数据集
            //设置网络数据集的名称
            (deNetworkdataset as IDataElement).Name = "Route_3D";

            IWorkspaceFactory pWorlsapceFactory = new FileGDBWorkspaceFactory();
            IFeatureWorkspace pFeatureWorkspace = pWorlsapceFactory.OpenFromFile(fgdbPath, 0) as IFeatureWorkspace;
            IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(dataSetName);

            IFeatureDatasetExtensionContainer pFeatureDatasetExtensionContainer = pFeatureDataset as IFeatureDatasetExtensionContainer;
            IFeatureDatasetExtension pFeatureDatasetExtension = pFeatureDatasetExtensionContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
            IDatasetContainer2 pDatasetContainer = pFeatureDatasetExtension as IDatasetContainer2;

            //设置网络数据集的空间参考和范围
            IGeoDataset pGeodataset = pFeatureDataset as IGeoDataset;
            IDEGeoDataset deGeodataset = deNetworkdataset as IDEGeoDataset;
            deGeodataset.SpatialReference = pGeodataset.SpatialReference;
            deGeodataset.Extent = pGeodataset.Extent;


            IDataset pDataset = pDatasetContainer.CreateDataset(deNetworkdataset as IDEDataset);
            pNetWorkDataset = pDataset as INetworkDataset;

            INetworkBuild pNetworkBuild = pNetWorkDataset as INetworkBuild;
            pNetworkBuild.BuildNetwork((pFeatureDataset as IGeoDataset).Extent);
            #endregion





        }
    }


}
