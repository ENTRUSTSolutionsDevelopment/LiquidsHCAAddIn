using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;

namespace LiquidsHCAAddIn
{
    internal class IdenityConfigTool : MapTool
    {
        public IdenityConfigTool()
        {
            //IsSketchTool = true;
            //SketchType = SketchGeometryType.Rectangle;
            //SketchOutputMode = SketchOutputMode.Map;
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            IdentityConfigWindow _prowindow1 = null;
            //already open?
            if (_prowindow1 != null)
                return base.OnToolActivateAsync(active);
            _prowindow1 = new IdentityConfigWindow();
            _prowindow1.Owner = FrameworkApplication.Current.MainWindow;
            _prowindow1.Closed += (o, e) => { _prowindow1 = null; };
            _prowindow1.Show();
            //uncomment for modal
            //_prowindow1.ShowDialog();
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
