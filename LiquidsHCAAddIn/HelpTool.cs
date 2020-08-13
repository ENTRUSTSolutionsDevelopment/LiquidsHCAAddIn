using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;

namespace LiquidsHCAAddIn
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
            System.Diagnostics.Process.Start(@"https://knowledgecenter.g2-is.com/products/gas-hca-tool/gas-hca-tool-reference/gas-hca-tools/prepare/initialize-database/");
            FrameworkApplication.SetCurrentToolAsync(null);
            return base.OnToolActivateAsync(true);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
