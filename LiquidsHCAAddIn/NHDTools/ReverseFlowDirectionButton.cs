using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Linq;

namespace LiquidsHCAAddIn.NHDTools
{
    internal class ReverseFlowDirectionButton : Button
    {
        protected override void OnClick()
        {
            // Check for an active mapview
            if (MapView.Active == null)
            {
                MessageBox.Show("No MapView currently active. Exiting...", "Info");
                return;
            }

            QueuedTask.Run(async () =>
            {

                string nhdlayername = "NHDFlowline";
                var featLayer = MapView.Active.Map.FindLayers(nhdlayername).First() as FeatureLayer;

                if (featLayer == null)
                {
                    MessageBox.Show("A feature layer must be selected. Exiting...", "Info");
                    return;
                }
                // Get the selected records, and check/exit if there are none:
                var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();

                if (featSelectionOIDs.Count == 0)
                {
                    MessageBox.Show("No records selected for layer, " + featLayer.Name + ". Exiting...", "Info");
                    return;
                }

                try
                {
                    foreach (var fid in featSelectionOIDs)
                    {
                        var inspector = new Inspector(true);
                        inspector.Load(featLayer, fid);
                        //GeometryBag g_bag = inspector.Shape as GeometryBag;
                        if (inspector.HasAttributes)
                        {
                            Geometry geo = GeometryEngine.Instance.ReverseOrientation(inspector.Shape as Multipart);
                            inspector["VelProvenance"] = "Manual Shape Reverse";
                            inspector["SHAPE"] = geo;

                            var editOp = new EditOperation();
                            editOp.Name = "EditReverse " + featLayer.Name + ", " + Convert.ToString(featSelectionOIDs.Count) + " records.";
                            editOp.Modify(inspector);
                            //editOp.ExecuteAsync();
                            if (editOp.IsEmpty)
                            {
                                editOp.Abort();
                            }
                            else
                            {
                                await editOp.ExecuteAsync();
                                await Project.Current.SaveEditsAsync();
                            }
                        }
                    }

                }
                catch (Exception exc)
                {
                    // Catch any exception found and display a message box.
                    MessageBox.Show("Exception caught while trying to perform reverse: " + exc.Message);
                    return;
                }
            });
        }
    }
}
