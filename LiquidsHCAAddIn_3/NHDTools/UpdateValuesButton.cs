using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Linq;

namespace LiquidsHCAAddIn_3.NHDTools
{
    internal class UpdateValuesButton : Button
    {
        private const string _dockPaneID = "LiquidsHCAAddIn_3_NHDTools_NHDFlowlineEditDockpane";
        protected override void OnClick()
        {
            UpdateValues();
        }

        public void UpdateValues()
        {
            //  This sample is intended for use with a featureclass with a default text field named "Description".
            //  You can replace "Description" with any field name that works for your dataset

            // Check for an active mapview

            //await QueuedTask.Run(async () =>
            //{
            //    if (MapView.Active == null)
            //    {
            //        MessageBox.Show("No MapView currently active. Exiting...", "Info");
            //        return;
            //    }
            //    string nhdlayername = "NHDFlowline";
            //    var featLayer = MapView.Active.Map.FindLayers(nhdlayername).FirstOrDefault() as FeatureLayer;

            //    if (featLayer == null)
            //    {
            //        MessageBox.Show("NHD Flowline layer not added to the Map. Exiting...", "Info");
            //        return;
            //    }

            //    var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();
            //    if (featSelectionOIDs.Count == 0)
            //    {
            //        MessageBox.Show("No records are selected for " + featLayer.Name + ". Exiting...", "Info");
            //        return;
            //    }

                DockPane pane = ArcGIS.Desktop.Framework.FrameworkApplication.DockPaneManager.Find(_dockPaneID);
                if (pane == null)
                    return;

                pane.Activate();
            //} );

            //await QueuedTask.Run(async () =>
            //{
            //    string nhdlayername = "NHDFlowline";
            //    string nhd_nd_name = "NHDFlowline_ND";

            //    var featLayer = MapView.Active.Map.FindLayers(nhdlayername).FirstOrDefault() as FeatureLayer;

            //    if (featLayer == null)
            //    {
            //        MessageBox.Show("NHD Flowline layer not added to the Map. Exiting...", "Info");
            //        return;
            //    }
            //    // Get the selected records, and check/exit if there are none:
            //    var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();
            //    if (featSelectionOIDs.Count == 0)
            //    {
            //        MessageBox.Show("No records are selected for " + featLayer.Name + ". Exiting...", "Info");
            //        return;
            //    }

            //    string strvalue = String.IsNullOrEmpty(Module1.Current.StreamVelValue.Text) ? "0.5" : Module1.Current.StreamVelValue.Text;
            //    double setvalue = double.Parse(strvalue);

            //    try
            //    {
            //        // Now ready to do the actual editing:
            //        // 1. Create a new edit operation and a new inspector for working with the attributes
            //        // 2. Check to see if a valid field name was chosen for the feature layer
            //        // 3. If so, apply the edit

            //        //
            //        var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();
            //        inspector.Load(featLayer, featSelectionOIDs);
            //        //if (inspector.HasAttributes && inspector.Count(a => a.FieldName.ToUpper() == attributename.ToUpper()) > 0)
            //        if (inspector.HasAttributes)
            //        {
            //            inspector["InNetwork"] = 1;
            //            inspector["FlowDir"] = 1;
            //            inspector["VelProvenance"] = "Manual Edit";
            //            inspector["OneWay"] = "FT";
            //            inspector["CalculatedVel"] = setvalue * 3.2808;//fps
            //            inspector["Stream_Vel"] = setvalue; //m/s
            //            inspector["TravelTime"] = 0.05;

            //            var editOp = new EditOperation();
            //            editOp.Name = "Edit " + featLayer.Name + ", " + Convert.ToString(featSelectionOIDs.Count) + " records.";
            //            editOp.Modify(inspector);
            //            editOp.ExecuteAsync();
            //            if (editOp.IsEmpty)
            //            {
            //                editOp.Abort();
            //            }
            //            else
            //            {
            //                //await editOp.ExecuteAsync();
            //                //MessageBox.Show("Update operation completed.", "Editing with Inspector");

            //                //Caliculate TtavelTime based on input stream velocity
            //                var param_values1 = Geoprocessing.MakeValueArray(featLayer, "TravelTime", "!LengthKM! / (!CalculatedVel!*0.0003048*60)");
            //                var gp_result1 = await Geoprocessing.ExecuteToolAsync("management.CalculateField", param_values1, null, null, null, GPExecuteToolFlags.AddToHistory);

            //                string in_features = featLayer.GetPath().LocalPath.Replace(nhdlayername, nhd_nd_name);
            //                var param_values = Geoprocessing.MakeValueArray(in_features);
            //                var gp_result = await Geoprocessing.ExecuteToolAsync("na.BuildNetwork", param_values, null, null, null, GPExecuteToolFlags.AddToHistory);

            //                if (!gp_result.IsFailed)
            //                {
            //                    if (Project.Current.HasEdits)
            //                    {
            //                        await Project.Current.SaveEditsAsync();

            //                        //To Refresh the map
            //                        MapView.Active.Redraw(true);
            //                        // Display all the parameters for the update:
            //                        MessageBox.Show("Here are update parameters details:  " +
            //                            "\r\n Layer: " + featLayer.Name +
            //                            "\r\n Number of records: " + Convert.ToString(featSelectionOIDs.Count) +
            //                            "\r\n\t InNetwork : 'Yes' " +
            //                            "\r\n\t FlowDir : 'WithDigitization' " +
            //                            "\r\n\t VelProvenance : 'Manual Edit' " +
            //                            "\r\n\t OneWay : 'FT' " +
            //                            "\r\n\t CalculatedVel : " + Convert.ToString(setvalue * 3.2808) + " fps" +
            //                            "\r\n\t Stream_Vel : " + Convert.ToString(setvalue) + " m/s" +
            //                            "\r\n\t TravelTime : Calculated value of length, Velocity"
            //                            );
            //                    }
            //                }
            //                //Geoprocessing.ShowMessageBox(gp_result.Messages, "Contents", GPMessageBoxStyle.Default, "Window Title");
            //            }
            //        }
            //        else
            //        {
            //            MessageBox.Show("The Attribute provided is not valid. " +
            //                "\r\n Ensure your attribute name is correct.", "Invalid attribute");
            //            // return;
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        // Catch any exception found and display a message box.
            //        MessageBox.Show("Exception caught while trying to perform update: " + exc.Message);
            //        return;
            //    }
            //});
        }


    }
}
