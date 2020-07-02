using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping;

namespace LiquidsHCAAddIn
{
    internal class ResetIdentityTool : MapTool
    {
        public ResetIdentityTool()
        {
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            var hasSelection = QueuedTask.Run(() =>
            {
                ProWindowConfig objCfg = new ProWindowConfig();
                objCfg = IdentityConfigWindow.promdl;

                var feat_lyrnames = new List<string>() { };
                var mosic_lyrnames = new List<string>() { };
                string filtername = "LiquidsHCAFilter";

                //Assign defaul layernames if the object is empty
                if (objCfg is null)
                {
                    feat_lyrnames = new List<string> { "HydrographicTransportPaths", "WaterbodySpreadPolygons" };
                    mosic_lyrnames = new List<string> { "MultiDimRasterMosaic" };
                }
                //Assign layer names from the config object 
                else
                {
                    if (!string.IsNullOrEmpty(objCfg.LSName))
                    {
                        mosic_lyrnames.Add(objCfg.LSName);
                    }

                    if (!string.IsNullOrEmpty(objCfg.NHDIntName))
                    {
                        feat_lyrnames.Add(objCfg.NHDIntName);
                    }
                    if (!string.IsNullOrEmpty(objCfg.HTPathName))
                    {
                        feat_lyrnames.Add(objCfg.HTPathName);
                    }
                    if (!string.IsNullOrEmpty(objCfg.HTSpreadName))
                    {
                        feat_lyrnames.Add(objCfg.HTSpreadName);
                    }
                }

                //Remove the Defination query for the feature layers
                foreach (string lyrname in feat_lyrnames)
                {
                    FeatureLayer fl = ActiveMapView.Map.FindLayers(lyrname).First() as FeatureLayer;

                    fl.RemoveDefinitionFilter(filtername);
                }

                // Remove the defination query for hte Mosic dataset
                foreach (string lyrname in mosic_lyrnames)
                {
                    MosaicLayer fl = ActiveMapView.Map.FindLayers(lyrname).First() as MosaicLayer;

                    fl.RemoveDefinitionFilter(filtername);
                }

                ActiveMapView.RedrawAsync(true);

                return true;
            });
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
