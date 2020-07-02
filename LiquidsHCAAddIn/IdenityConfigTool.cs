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
