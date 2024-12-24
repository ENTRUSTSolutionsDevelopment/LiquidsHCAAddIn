using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidsHCAAddIn_3.NHDTools
{
    internal class StreamVelEditBox : ArcGIS.Desktop.Framework.Contracts.EditBox
    {
        public StreamVelEditBox()
        {
            Module1.Current.StreamVelValue = this;
            Text = "0.5";
        }
    }
}
