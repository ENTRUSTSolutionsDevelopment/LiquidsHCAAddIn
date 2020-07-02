using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            
            var fls = MapView.Active.Map.GetLayersAsFlattenedList();// .GetLayersAsFlattenedList().First() as FeatureLayer;
            foreach (Layer l in fls)
            {
                if (l.GetType().Name == "FeatureLayer")
                {
                    if ((l as FeatureLayer).ShapeType == esriGeometryType.esriGeometryPoint)
                        if (l.Name.Contains("Point"))
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
            }
            else
            {
                cmbOSPoint.SelectedItem = promdl.OSPointName;
                cmbNHDIntlyr.SelectedItem = promdl.NHDIntName;
                cmbHTPathlyr.SelectedItem = promdl.HTPathName;
                cmbHTSpreadlyr.SelectedItem = promdl.HTSpreadName;
                cmbLSlyr.SelectedItem = promdl.LSName;
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
