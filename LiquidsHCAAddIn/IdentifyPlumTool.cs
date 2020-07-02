﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
    internal class IdentifyPlumTool : MapTool
    {
        public IdentifyPlumTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Screen;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var bottomRight = new Point();
            IList<Tuple<string, string, long>> tripleTuplePoints =
                      new List<Tuple<string, string, long>>();
            var hasSelection = await QueuedTask.Run(() =>
            {
                // geometry is a point
                var clickedPnt = geometry as MapPoint;
                if (clickedPnt == null) return false;
                // pixel tolerance
                var tolerance = 3;
                //Get the client point edges
                var topLeft = new Point(clickedPnt.X - tolerance, clickedPnt.Y + tolerance);
                bottomRight = new Point(clickedPnt.X + tolerance, clickedPnt.Y - tolerance);
                //convert the client points to Map points
                var mapTopLeft = MapView.Active.ClientToMap(topLeft);
                var mapBottomRight = MapView.Active.ClientToMap(bottomRight);
                //create a geometry using these points
                Geometry envelopeGeometry = EnvelopeBuilder.CreateEnvelope(mapTopLeft, mapBottomRight);
                if (envelopeGeometry == null) return false;
                //Get the features that intersect the sketch geometry.
                var result = ActiveMapView.GetFeatures(geometry);

                ProWindowConfig objCfg = new ProWindowConfig();
                objCfg = IdentityConfigWindow.promdl;


                //string[] feat_lyrnames = { };
                var feat_lyrnames = new List<string>() { };
                //string[] mosic_lyrnames = { };
                var mosic_lyrnames = new List<string>() { };
                string filtername = "LiquidsHCAFilter";
                string strOSPointlayername = "NHD_Intersections";
                                
                //Assign defaul layernames if the object is empty
                if (objCfg is null)
                {
                    feat_lyrnames = new List<string> { "HydrographicTransportPaths", "WaterbodySpreadPolygons" };
                    mosic_lyrnames = new List<string> { "MultiDimRasterMosaic" };
                }
                //Assign layer names from the config object 
                else
                {
                    //Check the values are null or not    
                    if (!string.IsNullOrEmpty(objCfg.OSPointName))
                    {
                        strOSPointlayername = objCfg.OSPointName;
                    }
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
                var ospointlyr = ActiveMapView.Map.FindLayers(strOSPointlayername).First() as FeatureLayer;

                foreach (var kvp in result)
                {
                    var bfl = kvp.Key;
                    // only look at points
                    if (kvp.Key.ShapeType != esriGeometryType.esriGeometryPoint) continue;
                    var layerName = bfl.Name;
                    var oidName = bfl.GetTable().GetDefinition().GetObjectIDField();
                    foreach (var oid in kvp.Value)
                    {   
                        //Select a single state polygon
                        FeatureLayer fl1 = ActiveMapView.Map.FindLayers(strOSPointlayername).First() as FeatureLayer;
                        QueryFilter queryFilter = new QueryFilter();
                        string whereClause = String.Format("{0} = {1}", oidName, oid);
                        queryFilter.WhereClause = whereClause;

                        //Use a cursor to get to a feature's geometry
                        using (ArcGIS.Core.Data.RowCursor rowCursor = fl1.Search(queryFilter))
                        {
                            //Grab the first record (and hopefully only record)
                            while (rowCursor.MoveNext())
                            {
                                //Grab the features geometry
                                Feature feature = rowCursor.Current as Feature;
                                //Geometry geo = feature.GetShape();
                                string pintid_field = "POINT_ID";
                                var pointidval = feature[pintid_field];

                                foreach (string lyrname in feat_lyrnames)
                                {
                                    FeatureLayer fl = ActiveMapView.Map.FindLayers(lyrname).First() as FeatureLayer;

                                    var lyrfilter = new CIMDefinitionFilter();
                                    lyrfilter.Name = filtername;
                                    lyrfilter.DefinitionExpression = String.Format("{0} = '{1}'", pintid_field, pointidval);

                                    fl.SetDefinitionFilter(lyrfilter);
                                }

                                foreach (string lyrname in mosic_lyrnames)
                                {
                                    MosaicLayer fl = ActiveMapView.Map.FindLayers(lyrname).First() as MosaicLayer;

                                    var lyrfilter = new CIMDefinitionFilter();
                                    lyrfilter.Name = filtername;
                                    lyrfilter.DefinitionExpression = String.Format("Name LIKE '{0}%'", pointidval);

                                    fl.SetDefinitionFilter(lyrfilter);
                                }
                                ActiveMapView.RedrawAsync(true);
                            }
                        }
                    }
                }
                return true;
            });
            return true;
        }
    }
}
