namespace LiquidsHCAAddIn.NHDTools
{
    class StreamVelEditBox : ArcGIS.Desktop.Framework.Contracts.EditBox
    {
        public StreamVelEditBox()
        {
            Module1.Current.StreamVelValue = this;
            Text = "0.5";
        }
    }
}
