using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Core.Internal.CIM;
using System.Security.Cryptography;


namespace LiquidsHCAAddIn_3.NHDTools
{
    internal class NHDFlowlineEditDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "LiquidsHCAAddIn_3_NHDTools_NHDFlowlineEditDockpane";

        public ICommand CmdEditFlowline { get; set; }
        public ICommand CmdCancelEdit { get; set; }

        Map _activeMap;

        //private string VelProvenance = "Manual Edit";
        private string NHDFlowlineName = "NHDFlowline";
        private string NHDNDName = "NHDFlowline_ND";
        //private double StreamVel = 0.5;
        protected NHDFlowlineEditDockpaneViewModel()
        {
            CmdEditFlowline = new RelayCommand(() => EditFlowline(), () => true);
            CmdCancelEdit = new RelayCommand(() => CancelEditFlowline(), () => true);

        }

        protected override Task InitializeAsync()
        {
            if (MapView.Active == null)
                return Task.FromResult(0);

            _activeMap = MapView.Active.Map;

            return InitDataSources();

            //return UpdateForActiveMap();
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Edit NHDF lowline";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }

        private string _nhdFlowlineLayerName = "NHDFlowline";
        public string NHDFlowlineLayerName
        {
            get { return _nhdFlowlineLayerName; }
            set
            {
                SetProperty(ref _nhdFlowlineLayerName, value, () => NHDFlowlineLayerName);
            }
        }

        private int _selectedFeaturesCount = 0;
        public int SelectedFeaturesCount
        {
            get { return _selectedFeaturesCount; }
            set
            {
                SetProperty(ref _selectedFeaturesCount, value, () => SelectedFeaturesCount);
            }
        }

        private double _streamVelocity = 0.5;
        public double StreamVelocity
        {
            get { return _streamVelocity; }
            set
            {
                SetProperty(ref _streamVelocity, value, () => StreamVelocity);
                if (_streamVelocity > 0)
                {

                    //_calculatedVelocity = _streamVelocity * 3.28084;
                    SetProperty(ref _calculatedVelocity, _streamVelocity * 3.28084, () => CalculatedVelocity);
                }
            }
        }

        private double _calculatedVelocity = 0.5 * 3.28084;
        public double CalculatedVelocity
        {
            get { return _calculatedVelocity; }
            set
            {
                SetProperty(ref _calculatedVelocity, value, () => CalculatedVelocity);
                if (_calculatedVelocity > 0)
                {
                    SetProperty(ref _streamVelocity, _calculatedVelocity / 3.28084, () => StreamVelocity);
                }
            }
        }

        //private double _travelTime = 0;
        //public double TravelTime
        //{
        //    get { return _travelTime; }
        //    set
        //    {
        //        SetProperty(ref _travelTime, value, () => TravelTime);
        //    }
        //}


        private ObservableCollection<string> _inNetwork = new ObservableCollection<string>() { "Yes", "No" };
        public ObservableCollection<string> InNetwork
        {
            get { return _inNetwork; }
            set
            {
                SetProperty(ref _inNetwork, value, () => InNetwork);
            }
        }

        public string _selectedInNetwork = "Yes";
        public string SelectedInNetwork
        {
            get { return _selectedInNetwork; }
            set
            {
                SetProperty(ref _selectedInNetwork, value, () => SelectedInNetwork);
            }
        }

        private ObservableCollection<string> _flowDirenction = new ObservableCollection<string>() { "WithDigitized", "Uninitialized" };
        public ObservableCollection<string> FlowDirenction
        {
            get { return _flowDirenction; }
            set
            {
                SetProperty(ref _flowDirenction, value, () => FlowDirenction);
            }
        }

        public string _selectedFlowDirenction = "WithDigitized";
        public string SelectedFlowDirenction
        {
            get { return _selectedFlowDirenction; }
            set
            {
                SetProperty(ref _selectedFlowDirenction, value, () => SelectedFlowDirenction);
            }
        }


        private ObservableCollection<string> _oneWay = new ObservableCollection<string>() { "FT", "BOTH", "TF" };
        public ObservableCollection<string> OneWay
        {
            get { return _oneWay; }
            set
            {
                SetProperty(ref _oneWay, value, () => OneWay);
            }
        }

        public string _selectedOneWay = "FT";
        public string SelectedOneWay
        {
            get { return _selectedOneWay; }
            set
            {
                SetProperty(ref _selectedOneWay, value, () => SelectedOneWay);
            }
        }


        private string _velProvenance = "Manual Edit";
        public string VelProvenance
        {
            get { return _velProvenance; }
            set
            {
                SetProperty(ref _velProvenance, value, () => VelProvenance);
            }
        }

        private string _editMessages = "";
        public string EditMessages
        {
            get { return _editMessages; }
            set
            {
                SetProperty(ref _editMessages, value, () => EditMessages);
            }
        }


        internal Task InitDataSources()
        {
            return QueuedTask.Run(() =>
            {

                var featLayer = MapView.Active.Map.FindLayers(NHDFlowlineName).FirstOrDefault() as FeatureLayer;

                if (featLayer == null)
                {
                    System.Windows.MessageBox.Show("NHD Flowline layer not added to the Map. Exiting...", "Info");
                    return;
                }

                var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();
                if (featSelectionOIDs.Count == 0)
                {
                    System.Windows.MessageBox.Show("No NHD Flowline features are selected! ", "Info");
                    return;
                }
                SetProperty(ref _nhdFlowlineLayerName, featLayer.Name, () => NHDFlowlineLayerName);
                SetProperty(ref _selectedFeaturesCount, featSelectionOIDs.Count, () => SelectedFeaturesCount);


            });
        }

        public async void EditFlowline()
        {
            try
            {
                await QueuedTask.Run(async () =>
                {
                    string nhdlayername = "NHDFlowline";
                    string nhd_nd_name = "NHDFlowline_ND";


                    var featLayer = MapView.Active.Map.FindLayers(nhdlayername).FirstOrDefault() as FeatureLayer;

                    if (featLayer == null)
                    {
                        System.Windows.MessageBox.Show("NHD Flowline layer not added to the Map. Exiting...", "Info");
                        return;
                    }
                    // Get the selected records, and check/exit if there are none:
                    var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();
                    if (featSelectionOIDs.Count == 0)
                    {
                        System.Windows.MessageBox.Show("No NHD Flowline features selected for " + featLayer.Name + ". Exiting...", "Info");
                        return;
                    }

                    if (StreamVelocity <= 0)
                    {
                        System.Windows.MessageBox.Show("Please provide valid stream velocity! ", "Info");
                        return;
                    }

                    try
                    {
                        //string in_features = featLayer.GetPath().LocalPath.Replace(nhdlayername, nhd_nd_name);
                        //var param_values = Geoprocessing.MakeValueArray(in_features, "NO_FORCE_FULL_BUILD");
                        //var gp_result = await Geoprocessing.ExecuteToolAsync("na.BuildNetwork", param_values, null, null, null, GPExecuteToolFlags.AddToHistory);

                        //System.Windows.MessageBox.Show("Selected Flowline feature(s) updated.");

                        // Now ready to do the actual editing:
                        // 1. Create a new edit operation and a new inspector for working with the attributes
                        // 2. Check to see if a valid field name was chosen for the feature layer
                        // 3. If so, apply the edit


                        ////Construct a query filter using the OIDs of the features that intersected the sketch geometry
                        //featLayer.SetEditable(true);

                        //var oid = featLayer.GetTable().GetDefinition().GetObjectIDField();
                        //var qf = new ArcGIS.Core.Data.QueryFilter()
                        //{

                        //    ObjectIDs = featSelectionOIDs
                        //};
                        //var cursor = featLayer.Search(qf);
                        //while (cursor.MoveNext())
                        //{
                        //    using (Row row = cursor.Current)
                        //    {

                        //        row["InNetwork"] = SelectedInNetwork == "No" ? 0 : 1;
                        //        row["FlowDir"] = SelectedFlowDirenction == "Uninitialized" ? 0 : 1;
                        //        row["OneWay"] = String.IsNullOrEmpty(SelectedOneWay) ? "FT" : SelectedOneWay;
                        //        row["CalculatedVel"] = CalculatedVelocity;//fps
                        //        row["Stream_Vel"] = StreamVelocity; //m/s
                        //        double lengthM = Convert.ToDouble(row["LengthKM"]) * 1000;

                        //        row["TravelTime"] = lengthM / (Convert.ToDouble(row["Stream_Vel"]) * 60); // minutes
                        //        row["VelProvenance"] = VelProvenance;
                        //        row.Store();
                        //    }

                        //}
                        //if (Project.Current.HasEdits)
                        //{
                        //    await Project.Current.SaveEditsAsync();
                        //}
                        //System.Windows.MessageBox.Show("Selected Flowline feature(s) updated.");
                        //MapView.Active.Redraw(true);

                        SetProperty(ref _editMessages, "Updating Selected NHD Flowline Values...", () => EditMessages);
                        var environment = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

                        var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                        inspector.Load(featLayer, featSelectionOIDs);
                        //if (inspector.HasAttributes && inspector.Count(a => a.FieldName.ToUpper() == attributename.ToUpper()) > 0)
                        if (inspector.HasAttributes)
                        {

                            //inspector["InNetwork"] = 1;
                            inspector["InNetwork"] = SelectedInNetwork == "Yes" ? 1 : 0;

                            //inspector["FlowDir"] = 1;
                            inspector["FlowDir"] = String.IsNullOrEmpty(SelectedFlowDirenction) ? "WithDigitized" : SelectedFlowDirenction;

                            //inspector["OneWay"] = "FT";
                            inspector["OneWay"] = String.IsNullOrEmpty(SelectedOneWay) ? "FT" : SelectedOneWay;

                            inspector["CalculatedVel"] = CalculatedVelocity;//fps
                            inspector["Stream_Vel"] = StreamVelocity; //m/s
                            //inspector["TravelTime"] = 0.05;
                            //inspector["TravelTime"] = (Convert.ToDouble(inspector["LengthKM"]) * 1000) / (StreamVelocity * 60); // minutes
                            inspector["VelProvenance"] = VelProvenance;

                            var editOp = new EditOperation();
                            editOp.Name = "Edit " + featLayer.Name + ", " + Convert.ToString(featSelectionOIDs.Count) + " records.";
                            editOp.Modify(inspector);
                            await editOp.ExecuteAsync();
                            if (editOp.IsEmpty)
                            {
                                editOp.Abort();
                            }
                            else
                            {
                                //var progDlg = new ProgressDialog("Running Geoprocessing Tool", "Cancel", 100, true);
                                //progDlg.Show();

                                //var progSrc = new CancelableProgressorSource(progDlg);

                                //await editOp.ExecuteAsync();
                                //MessageBox.Show("Update operation completed.", "Editing with Inspector");

                                //Caliculate TtavelTime based on input stream velocity
                                var param_values1 = Geoprocessing.MakeValueArray(featLayer, "TravelTime", "!LengthKM! / (!CalculatedVel!*0.0003048*60)");
                                var gp_result1 = await Geoprocessing.ExecuteToolAsync("management.CalculateField", 
                                                                            param_values1, 
                                                                            environment, 
                                                                            null, // progSrc.Progressor , 
                                                                            null, 
                                                                            GPExecuteToolFlags.AddToHistory);

                                if (gp_result1.IsFailed)
                                {
                                    SetProperty(ref _editMessages, "There is an Error Encountered while Updating Selected NHD Flowline Values,!" +
                                                        "\nPlease Try Again!", () => EditMessages);
                                    return;
                                }
                               

                                SetProperty(ref _editMessages, "Building Network Dataset NHDFlowline_ND...", () => EditMessages);

                                string in_features = featLayer.GetPath().LocalPath.Replace(nhdlayername, nhd_nd_name);
                                var param_values = Geoprocessing.MakeValueArray(in_features, "NO_FORCE_FULL_BUILD");
                                var gp_result = await Geoprocessing.ExecuteToolAsync("na.BuildNetwork", 
                                                                                    param_values, 
                                                                                    environment,
                                                                                    null, // progSrc.Progressor , 
                                                                                    null, 
                                                                                    GPExecuteToolFlags.AddToHistory);


                                if (!gp_result.IsFailed)
                                {
                                    if (Project.Current.HasEdits)
                                    {
                                        await Project.Current.SaveEditsAsync();
                                    }
                                    SetProperty(ref _editMessages, "Build Network Dataset NHDFlowline_ND Completed.", () => EditMessages);
                                    System.Windows.MessageBox.Show("Selected Flowline feature(s) updated.");
                                    MapView.Active.Redraw(true);
                                    SetProperty(ref _editMessages, "", () => EditMessages);
                                }
                                else
                                {
                                    SetProperty(ref _editMessages, "There is an Error Encountered while Building Network!" +                                                       
                                                        "\nPlease Build Network Manually! " +
                                                        "\nRight Click on NHDFlowline_ND in NWDataset and Build.", () => EditMessages);
                                }


                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("The Attribute provided is not valid. " +
                                "\r\n Ensure your attribute name is correct.", "Invalid attribute");
                            // return;
                        }
                    }
                    catch (Exception exc)
                    {
                        // Catch any exception found and display a message box.
                        System.Windows.MessageBox.Show("Exception caught while trying to perform update: " + exc.Message);
                        SetProperty(ref _editMessages, "There is an Error Encountered while Building Network! \n"  + 
                                                        exc.Message + 
                                                        "\nPlease Build Network Manually! " +
                                                        "\nRight Click on NHDFlowline_ND in NWDataset and Build.", () => EditMessages);
                        return;
                    }
                });



                //await QueuedTask.Run(async () =>
                //{


                //    var vl = SelectedValveLayer.GetTable() as FeatureClass;
                //    var createValve = new EditOperation();

                //    createValve.ProgressMessage = "Working...";
                //    createValve.CancelMessage = "Operation canceled";
                //    createValve.ErrorMessage = "Error creating valve";

                //    createValve.Name = "Create Valve";
                //    var attributes = new Dictionary<string, object>();
                //    attributes.Add("SHAPE", ValveGeom);

                //    // TODO add following values from release points and segments layers

                //    QueryFilter queryFilter = new QueryFilter
                //    {
                //        WhereClause = string.Format("ROUTE_ID = {0} AND MEASURE <= {1}", ValveRouteIDTxt, ValveMeasureTxt),
                //        SubFields = "POINT_INDEX, MEASURE, ELEVATION, Z_LENGTH, DRAIN_RATE, DRAIN_VOL, OP_UP_DRAIN_VOL, OP_DN_DRAIN_VOL, OP_TOTAL_DRAIN_VOL",
                //        PostfixClause = "ORDER BY MEASURE DESC"
                //    };


                //    using (RowCursor rowCursor = SelectedOspointmLayer.Search(queryFilter))
                //    {
                //        while (rowCursor.MoveNext())
                //        {
                //            using (Row row = rowCursor.Current)
                //            {
                //                attributes.Add("ELEVATION", Convert.ToDouble(row["ELEVATION"]));
                //                attributes.Add("LOW_POINT_INDEX", Convert.ToDouble(row["POINT_INDEX"]));
                //                attributes.Add("HIGH_POINT_INDEX", Convert.ToDouble(row["POINT_INDEX"]));
                //                attributes.Add("Z_LENGTH", Convert.ToDouble(row["Z_LENGTH"]));
                //                attributes.Add("VALVE_UP_DRAIN_RATE", Convert.ToDouble(row["DRAIN_RATE"]));
                //                attributes.Add("VALVE_DN_DRAIN_RATE", Convert.ToDouble(row["DRAIN_RATE"]));
                //                attributes.Add("VALVE_UP_VOL", Convert.ToDouble(row["OP_UP_DRAIN_VOL"]));
                //                attributes.Add("VALVE_DN_VOL", Convert.ToDouble(row["OP_DN_DRAIN_VOL"]));
                //                attributes.Add("VALVE_TOTAL_VOL", Convert.ToDouble(row["OP_TOTAL_DRAIN_VOL"]));
                //                break;
                //            }
                //        }
                //    }

                //    attributes.Add(SelectedMeasureField.Name, ValveMeasureTxt);
                //    attributes.Add(SelectedValveRouteIDField.Name, ValveRouteIDTxt);

                //    QueryFilter queryFilter1 = new QueryFilter
                //    {
                //        WhereClause = string.Format("ROUTE_ID = {0} AND SEGMENT_NAME = '{1}'", ValveRouteIDTxt, SelectedAddValveSegment.SegmentName),
                //        SubFields = "SEGMENT_INDEX, SEGMENT_NAME"
                //    };


                //    using (RowCursor rowCursor = SelectedSegmentsLayer.Search(queryFilter1))
                //    {
                //        while (rowCursor.MoveNext())
                //        {
                //            using (Row row = rowCursor.Current)
                //            {
                //                attributes.Add("LOW_SEGMENT_INDEX", Convert.ToInt32(row["SEGMENT_INDEX"]));
                //                attributes.Add("HIGH_SEGMENT_INDEX", Convert.ToInt32(row["SEGMENT_INDEX"]));
                //                break;
                //            }
                //        }
                //    }

                //    attributes.Add(SelectedValveRouteIDField.Name, ValveRouteIDTxt);
                //    attributes.Add(SelectedMeasureField.Name, ValveMeasureTxt);
                //    if (SelectedValveType == "Mainline")
                //    {
                //        //attributes.Add("CLOSURE_TIME", ValveClosureTimeTxt);
                //        attributes.Add(SelectedClosureTimeField.Name, ValveClosureTimeTxt);
                //    }

                //    attributes.Add("SCENARIO_ID", SelectedScenario);

                //    var token = createValve.Create(vl, attributes);

                //    createValve.Execute();

                //    // add scenarios row
                //    if (createValve.IsSucceeded)
                //    {
                //        QueryFilter queryFilter2 = new QueryFilter
                //        {
                //            WhereClause = string.Format("ROUTE_ID = {0} AND SCENARIO_ID <> '{1}' AND SCENARIO_ID <> '{2}' AND SEGMENT_INDEX = {3} AND SCENARIO_ID = '{4}'",
                //                                        SelectedRouteIDValue, "Operational", "Baseline", SelectedAddValveSegment.SegmentIndex, SelectedScenario)
                //        };

                //        int objid = 0;

                //        using (RowCursor rowCursor = SelectedScenariosLayer.Search(queryFilter2))
                //        {
                //            while (rowCursor.MoveNext())
                //            {
                //                using (Row row = rowCursor.Current)
                //                {
                //                    objid = Convert.ToInt16(row["OBJECTID"]);
                //                    break;
                //                }
                //            }
                //        }
                //        if (objid > 0) // update scenario values
                //        {
                //            //// messagebox to update the values
                //            //MessageBoxResult result = System.Windows.MessageBox.Show("Scenario ID " + SelectedScenario + " already exists in the " + 
                //            //    SelectedScenariosLayer + " for the selected segment. \nDo you want to continue?", 
                //            //    "Scenariod ID exists!", MessageBoxButton.YesNo, MessageBoxImage.Information);
                //            //if (result == MessageBoxResult.Yes)
                //            //{
                //            //    var inspector = new Inspector();
                //            //    await inspector.LoadAsync(SelectedScenariosLayer, objid);
                //            //    inspector["MAX_DRAIN_VOL"] = -999;
                //            //    inspector["AVG_DRAIN_VOL"] = -999;
                //            //    inspector["TOTAL_VOL_AVG_STDEV"] = -999;
                //            //    inspector["MAX_EFRD_INDEX"] = -999;
                //            //    inspector["AVG_EFRD_INDEX"] = -999;
                //            //    inspector["EFRD_INDEX_AVG_STDEV"] = -999;

                //            //    var editOperation = new EditOperation();
                //            //    editOperation.Name = string.Format("Update  SelectedScenariosLayer {0}", objid);
                //            //    editOperation.Modify(inspector);
                //            //    editOperation.Execute();
                //            //    if (!editOperation.IsSucceeded)
                //            //    {
                //            //        System.Windows.MessageBox.Show("Edit Scenario FAILED! \n" + editOperation.ErrorMessage);
                //            //    }
                //            //}
                //        }

                //        // TODO clear selection in efrd_scenarios table

                //        ///////////*********************///////////////


                //        if (Project.Current.HasEdits)
                //        {
                //            _ = Project.Current.SaveEditsAsync();
                //        }

                //        System.Windows.MessageBox.Show("Valve Feature Created!");
                //    }
                //    else
                //    {
                //        System.Windows.MessageBox.Show("Create Valve FAILED! \n" + createValve.ErrorMessage);
                //    }
                //});
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }

        public void CancelEditFlowline()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.IsVisible = false;
            //pane.Hide();

        }

        /// <summary>
        /// Button implementation to show the DockPane.
        /// </summary>
        //internal class NHDFlowlineEditDockpane_ShowButton : Button
        //{
        //    protected override void OnClick()
        //    {
        //        NHDFlowlineEditDockpaneViewModel.Show();
        //    }
        //}
    }
}
