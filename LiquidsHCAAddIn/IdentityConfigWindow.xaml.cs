using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using System.ComponentModel;
using System.Windows;

namespace LiquidsHCAAddIn
{
    /// <summary>
    /// Interaction logic for IdentityConfigWindow.xaml
    /// </summary>
    public partial class IdentityConfigWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        protected static string ospointlyrname = "";
        public static ProWindowConfig promdl;
        public IdentityConfigWindow()
        {
            InitializeComponent();
            promdl = new ProWindowConfig();
            var fls = MapView.Active.Map.GetLayersAsFlattenedList();// .GetLayersAsFlattenedList().First() as FeatureLayer;
            foreach (Layer l in fls)
            {
                if (l.GetType().Name == "FeatureLayer")
                {
                    if ((l as FeatureLayer).ShapeType == esriGeometryType.esriGeometryPoint)
                        if (l.Name.ToUpper().Contains("POINT"))
                            cmbOSPoint.Items.Add(l.Name);
                        else
                            cmbNHDIntlyr.Items.Add(l.Name);
                    else if ((l as FeatureLayer).ShapeType == esriGeometryType.esriGeometryPolyline)
                        cmbHTPathlyr.Items.Add(l.Name);
                    else if ((l as FeatureLayer).ShapeType == esriGeometryType.esriGeometryPolygon)
                        cmbHTSpreadlyr.Items.Add(l.Name);
                }
                else if (l.GetType().Name == "MosaicLayer")
                    cmbLSlyr.Items.Add(l.Name);
            }

            if (promdl is null)
            {
                promdl = new ProWindowConfig();
                if (cmbOSPoint.Items.Contains("OSPOINTM"))
                {
                    cmbOSPoint.SelectedItem = "OSPOINTM";
                }
                if (cmbNHDIntlyr.Items.Contains("NHD_Intersection"))
                {
                    cmbNHDIntlyr.SelectedItem = "NHD_Intersection";
                }
                if (cmbHTPathlyr.Items.Contains("HydrographicTransportPaths"))
                {
                    cmbHTPathlyr.SelectedItem = "HydrographicTransportPaths";
                }
                if (cmbHTSpreadlyr.Items.Contains("WaterbodySpreadPolygons"))
                {
                    cmbHTSpreadlyr.SelectedItem = "WaterbodySpreadPolygons";
                }
                if (cmbLSlyr.Items.Contains("MultiDimRasterMosaic"))
                {
                    cmbLSlyr.SelectedItem = "MultiDimRasterMosaic";
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(promdl.OSPointName))
                {
                    cmbOSPoint.SelectedItem = promdl.OSPointName;
                }
                else
                {
                    cmbOSPoint.SelectedItem = "OSPOINTM";
                }
                if (!string.IsNullOrEmpty(promdl.NHDIntName))
                {
                    cmbNHDIntlyr.SelectedItem = promdl.NHDIntName;
                }
                else
                {
                    cmbNHDIntlyr.SelectedItem = "NHD_Intersection";
                }
                if (!string.IsNullOrEmpty(promdl.HTPathName))
                {
                    cmbHTPathlyr.SelectedItem = promdl.HTPathName;
                }
                else
                {
                    cmbHTPathlyr.SelectedItem = "HydrographicTransportPaths";
                }
                if (!string.IsNullOrEmpty(promdl.HTSpreadName))
                {
                    cmbHTSpreadlyr.SelectedItem = promdl.HTSpreadName;
                }
                else
                {
                    cmbHTSpreadlyr.SelectedItem = "WaterbodySpreadPolygons";
                }
                if (!string.IsNullOrEmpty(promdl.LSName))
                {
                    cmbLSlyr.SelectedItem = promdl.LSName;
                }
                else
                {
                    cmbLSlyr.SelectedItem = "MultiDimRasterMosaic";
                }
            }
        }
        private ProWindowConfig _configDetails;
        public ProWindowConfig ConfigDetails
        {
            get { return _configDetails; }
            set
            {
                _configDetails = ConfigDetails;
            }
        }
        private void btnConfigure_Click(object sender, RoutedEventArgs e)
        {
            promdl.OSPointName = cmbOSPoint.Text;
            promdl.NHDIntName = cmbNHDIntlyr.Text;
            promdl.HTPathName = cmbHTPathlyr.Text;
            promdl.HTSpreadName = cmbHTSpreadlyr.Text;
            promdl.LSName = cmbLSlyr.Text;
            ConfigDetails = promdl;
            this.Close();
            FrameworkApplication.SetCurrentToolAsync(null);

        }

        private void ConfigWindow_Closing(object sender, CancelEventArgs e)
        {
            FrameworkApplication.SetCurrentToolAsync(null);
        }
    }

        public class ProWindowConfig
        {
            public string OSPointName { get; set; }
            public string LSName { get; set; }
            public string NHDIntName { get; set; }
            public string HTPathName { get; set; }
            public string HTSpreadName { get; set; }

        }
    }
