using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace LiquidsHCAAddIn
{
    internal class ShowIdentityConfigWindow : Button
    {

        private IdentityConfigWindow _identityconfigwindow = null;

        protected override void OnClick()
        {
            //already open?
            if (_identityconfigwindow != null)
                return;
            _identityconfigwindow = new IdentityConfigWindow();
            _identityconfigwindow.Owner = FrameworkApplication.Current.MainWindow;
            _identityconfigwindow.Closed += (o, e) => { _identityconfigwindow = null; };
            _identityconfigwindow.Show();
            //uncomment for modal
            //_identityconfigwindow.ShowDialog();
        }

    }
}
