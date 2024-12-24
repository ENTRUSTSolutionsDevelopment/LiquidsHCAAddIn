using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Linq;


using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LiquidsHCAAddIn_3.NHDTools
{
    internal class ShowSymbolCheckBox : ArcGIS.Desktop.Framework.Contracts.CheckBox
    {
        public ShowSymbolCheckBox()
        {
            Module1.Current.ShowSymbolCheckBoxValue = this;           
            IsChecked = false;
        }

        protected override void OnClick()
        {
            try 
            { 
                //MessageBox.Show("Check box status "+this.IsChecked);

                if (this.IsChecked == true)
                {
                    //Apply symbology
                    ApplyCustomSymbol();
                }
                else
                {
                    //Remove custom symbology
                    ApplyPreviousSymbol();               
                }
            }
            catch 
            {
                return;
            }
        }


        public static void ApplyPreviousSymbol()
        {

            // Check for an active mapview
            if (MapView.Active == null)
            {
                MessageBox.Show("No MapView currently active. Exiting...", "Info");
                return;
            }

            QueuedTask.Run(() =>
            {
                string nhdlayername = "NHDFlowline";
                var featLayer = MapView.Active.Map.FindLayers(nhdlayername).FirstOrDefault() as FeatureLayer;

                if (featLayer == null)
                {
                    MessageBox.Show("NHD Flowline is not added to the map.", "Info");
                    return;
                }

                try
                {
                    CIMRenderer pre_renderer = featLayer.GetRenderer();

                    if (pre_renderer.GetType().Name == "CIMUniqueValueRenderer")
                    {
                        int cnt = ((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes.Length;
                        if (cnt > 0)
                        {
                            for (int i = 0; i < cnt; i++)
                            {
                                int symbollayercount = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers.Length;
                                var presymbollayers = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers;
                                ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers = new CIMSymbolLayer[symbollayercount - 1];

                                int j = 0;
                                foreach (CIMSymbolLayer symlayer in (ArcGIS.Core.CIM.CIMSymbolLayer[])presymbollayers)
                                {
                                    if (symlayer.GetType().Name == "CIMCharacterMarker" && ((CIMCharacterMarker)symlayer).FontFamilyName == "ESRI Arrowhead" && ((CIMCharacterMarker)symlayer).CharacterIndex == 37)
                                    {
                                        //Not adding the Arrow charector marker symbols to the layer list
                                    }
                                    else
                                    {
                                        ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers[j] = symlayer as CIMSymbolLayer;
                                        j = j + 1;
                                    }
                                }
                            }
                        }
                        featLayer.SetRenderer(pre_renderer);
                    }
                    else if (pre_renderer.GetType().Name == "CIMClassBreaksRenderer")
                    {
                        int cnt = ((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks.Length;
                        if (cnt > 0)
                        {
                            for (int i = 0; i < cnt; i++)
                            {
                                int symbollayercount = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers.Length;
                                var presymbollayers = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers;
                                ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers = new CIMSymbolLayer[symbollayercount - 1];

                                int j = 0;
                                foreach (CIMSymbolLayer symlayer in (ArcGIS.Core.CIM.CIMSymbolLayer[])presymbollayers)
                                {
                                    if (symlayer.GetType().Name == "CIMCharacterMarker" && ((CIMCharacterMarker)symlayer).FontFamilyName == "ESRI Arrowhead" && ((CIMCharacterMarker)symlayer).CharacterIndex == 37)
                                    {
                                        //Not adding the Arrow charector marker symbols to the layer list
                                    }
                                    else
                                    {
                                        ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers[j] = symlayer as CIMSymbolLayer;
                                        j = j + 1;
                                    }
                                }
                            }
                        }
                        featLayer.SetRenderer(pre_renderer);
                    }
                    else if (pre_renderer.GetType().Name == "CIMSimpleRenderer")
                    {
                        int symbollayercount = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers.Length;
                        var presymbollayers = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers;

                        ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers = new CIMSymbolLayer[symbollayercount - 1];

                        int j = 0;
                        foreach (CIMSymbolLayer symlayer in (ArcGIS.Core.CIM.CIMSymbolLayer[])presymbollayers)
                        {
                            if (symlayer.GetType().Name == "CIMCharacterMarker" && ((CIMCharacterMarker)symlayer).FontFamilyName == "ESRI Arrowhead" && ((CIMCharacterMarker)symlayer).CharacterIndex == 37)
                            {
                                //Not adding the Arrow charector marker symbols to the layer list
                            }
                            else
                            {
                                ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers[j] = symlayer as CIMSymbolLayer;
                                j = j + 1;
                            }
                        }

                        featLayer.SetRenderer(pre_renderer);
                    }
                    else
                    {
                        MessageBox.Show("Hide Flowline Symbol option is not enabled for this symbology type.");
                    }
                }
                catch (Exception exc)
                {
                    // Catch any exception found and display a message box.
                    //MessageBox.Show("Exception caught while trying to perform update: " + exc.Message);
                    return;
                }
            });

        }

        public static void ApplyCustomSymbol()
        {
            //MessageBox.Show("Test ..", "Info");
            // Check for an active mapview
            if (MapView.Active == null)
            {
                MessageBox.Show("No MapView currently active. Exiting...", "Info");
                return;
            }

            QueuedTask.Run(() =>
            {

                string nhdlayername = "NHDFlowline";
                var featLayer = MapView.Active.Map.FindLayers(nhdlayername).FirstOrDefault() as FeatureLayer;

                if (featLayer == null)
                {
                    MessageBox.Show("A feature layer must be selected. Exiting...", "Info");
                    return;
                }

                try
                {
                    CIMRenderer pre_renderer = featLayer.GetRenderer();
                    var arrowmarker = CreateArrowMarkerPerSpecs();

                    if (pre_renderer.GetType().Name == "CIMUniqueValueRenderer")
                    {
                        int cnt = ((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes.Length;
                        if (cnt > 0)
                        {
                            for (int i = 0; i < cnt; i++)
                            {
                                int symbollayercount = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers.Length;
                                var presymbollayers = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers;

                                ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers = new CIMSymbolLayer[symbollayercount + 1];
                                ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers[0] = arrowmarker as CIMSymbolLayer;

                                int j = 0;
                                foreach (CIMSymbolLayer symlayer in (ArcGIS.Core.CIM.CIMSymbolLayer[])presymbollayers)
                                {
                                    j = j + 1;
                                    ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMUniqueValueRenderer)pre_renderer).Groups[0].Classes[i].Symbol.Symbol).SymbolLayers[j] = symlayer as CIMSymbolLayer;

                                }
                            }
                        }
                        featLayer.SetRenderer(pre_renderer);
                    }
                    else if (pre_renderer.GetType().Name == "CIMClassBreaksRenderer")
                    {
                        int cnt = ((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks.Length;
                        if (cnt > 0)
                        {
                            for (int i = 0; i < cnt; i++)
                            {
                                int symbollayercount = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers.Length;
                                var presymbollayers = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers;

                                ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers = new CIMSymbolLayer[symbollayercount + 1];
                                ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers[0] = arrowmarker as CIMSymbolLayer;

                                int j = 0;
                                foreach (CIMSymbolLayer symlayer in (ArcGIS.Core.CIM.CIMSymbolLayer[])presymbollayers)
                                {
                                    j = j + 1;
                                    ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMClassBreaksRendererBase)pre_renderer).Breaks[i].Symbol.Symbol).SymbolLayers[j] = symlayer as CIMSymbolLayer;

                                }
                            }
                        }
                        featLayer.SetRenderer(pre_renderer);
                    }
                    else if (pre_renderer.GetType().Name == "CIMSimpleRenderer")
                    {
                        int symbollayercount = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers.Length;
                        var presymbollayers = ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers;

                        ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers = new CIMSymbolLayer[symbollayercount + 1];
                        ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers[0] = arrowmarker as CIMSymbolLayer;

                        int j = 0;
                        foreach (CIMSymbolLayer symlayer in (ArcGIS.Core.CIM.CIMSymbolLayer[])presymbollayers)
                        {
                            j = j + 1;
                            ((ArcGIS.Core.CIM.CIMMultiLayerSymbol)((ArcGIS.Core.CIM.CIMSimpleRenderer)pre_renderer).Symbol.Symbol).SymbolLayers[j] = symlayer as CIMSymbolLayer;

                        }
                        featLayer.SetRenderer(pre_renderer);
                    }
                    else
                    {
                        MessageBox.Show("Show Flowline Symbol option is not enabled for this symbology type.");
                    }
                }
                catch (Exception exc)
                {
                    // Catch any exception found and display a message box.
                    MessageBox.Show("Exception caught while applying arrow symbols for NHD Flowline: \n " + exc.Message);
                    return;
                }
            });
        }

        private static CIMMarker CreateArrowMarkerPerSpecs()
        {
            CIMCharacterMarker marker = SymbolFactory.Instance.ConstructMarker(
                   37,
                   "ESRI Arrowhead",
                   "Regular",
                   10
                   ) as CIMCharacterMarker;

            CIMMarkerPlacementOnLine markerPlacement = new CIMMarkerPlacementOnLine()
            {
                AngleToLine = true,
                RelativeTo = PlacementOnLineRelativeTo.LineMiddle
            };
            marker.MarkerPlacement = markerPlacement;
            return marker;
        }
    }
}
