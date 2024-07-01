using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LiquidsHCAAddIn_3
{
    internal class HelpTool : MapTool
    {
        public HelpTool()
        {            
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            // ChromePaneViewModel.Create();           
            //System.Diagnostics.Process.Start(@"https://knowledgecenter.g2-is.com/products/liquids-hca-tool/");
            Process.Start(new ProcessStartInfo { FileName = @"https://knowledgecenter.entrustsol.com/products/liquids-hca-tool/", UseShellExecute = true });
            FrameworkApplication.SetCurrentToolAsync(null);
            return base.OnToolActivateAsync(true);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
