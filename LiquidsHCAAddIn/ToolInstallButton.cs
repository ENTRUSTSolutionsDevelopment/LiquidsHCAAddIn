using ArcGIS.Desktop.Framework.Contracts;
using System;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace LiquidsHCAAddIn
{
    internal class ToolInstallButton : Button
    {
        //Local variables for the tool operation
        private string activeEnvironment = "";
        private string liquidsHCAToolpath = "";
        private bool flagPreLoad = true;

        public ToolInstallButton()
        {
            //Fetch the Executing assemply path
            var pathPython = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
            if (pathPython == null) return;
            pathPython = Uri.UnescapeDataString(pathPython);
            //MessageBox.Show(" pathPython \n " + pathPython, "  Info ");

            //Fetch the ProEnv.txt file where active environment path details are stored
            var proenvfilepath = System.IO.Path.Combine(pathPython.Substring(0, pathPython.LastIndexOf("ESRI")), @"ESRI\conda\envs\proenv.txt");
            //MessageBox.Show(" Pro Environemnt file path \n " + proenvfilepath);

            //Read the file to find the active environment to check the exsistency of installed packages            
            activeEnvironment = System.IO.File.ReadAllText(proenvfilepath);
            //Replaced escape charectors in the string
            activeEnvironment = activeEnvironment.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            //MessageBox.Show("Active Environment is \n " + activeEnvironment);

            //Fetch Liquids HCA tool is installed or not in active environment and assign button caption
            var liquidsHCAToolsubpath = @"Lib\site-packages\liquidshca\esri\toolboxes\LiquidsHCA.pyt";
            liquidsHCAToolpath = System.IO.Path.Combine(activeEnvironment, liquidsHCAToolsubpath);
            Caption = System.IO.File.Exists(liquidsHCAToolpath) ? "Uninstall Tool" : "Install Tool";

            //Button caption, Tool tip and Icon based the tool avilablity
            if (Caption == "Install Tool")
            {
                Caption = "Install Tool";
                TooltipHeading = "Install Liquids HCA";
                Tooltip = "Installs Liquids HCA Tool";
                LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                   @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GeoprocessingToolboxNew32.png"));
            }
            else
            {
                Caption = "Uninstall Tool";
                TooltipHeading = "Uninstall Liquids HCA";
                Tooltip = "Uninstalls Liquids HCA Tool";
                LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                    @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GenericDeleteRed32.png"));
            }

            //Set Flag to distrigwish initial load
            flagPreLoad = false;
        }

        protected override void OnClick()
        {
            string tagInstallUnistall = "";
            try
            {
                string resultMessage = "";

                //Don't load at initial load
                if (flagPreLoad)
                {
                    //Fetch the Executing assemply path
                    var pathPython = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
                    if (pathPython == null) return;
                    pathPython = Uri.UnescapeDataString(pathPython);
                    //MessageBox.Show(" pathPython \n " + pathPython, "  Info ");

                    //Fetch the ProEnv.txt file where active environment path details are stored
                    var proenvfilepath = System.IO.Path.Combine(pathPython.Substring(0, pathPython.LastIndexOf("ESRI")), @"ESRI\conda\envs\proenv.txt");
                    //MessageBox.Show(" Pro Environemnt file path \n " + proenvfilepath);

                    //Read the file to find the active environment to check the exsistency of installed packages
                    activeEnvironment = System.IO.File.ReadAllText(proenvfilepath);
                    activeEnvironment = activeEnvironment.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                    //MessageBox.Show("Active Environment is \n " + activeEnvironment);

                    //Tool path in active environment
                    var liquidsHCAToolsubpath = @"Lib\site-packages\liquidshca\esri\toolboxes\LiquidsHCA.pyt";
                    liquidsHCAToolpath = System.IO.Path.Combine(activeEnvironment, liquidsHCAToolsubpath);

                    //Assign Button caption based othe tool avilability in active environment
                    Caption = System.IO.File.Exists(liquidsHCAToolpath) ? "Uninstall Tool" : "Install Tool";

                }

                //Fetch the default environment where execute is there, this is where conda is also will be there
                var pathProExe = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase)).AbsolutePath);
                if (pathProExe == null) return;
                pathProExe = Uri.UnescapeDataString(pathProExe);
                pathProExe = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3");

                //Fetch the conda path, Which is required to invoke and run Conda packages
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                var condafilepath = System.IO.Path.Combine(pathProExe.Substring(0, pathProExe.LastIndexOf("envs") - 1), @"scripts\conda");
                proc.StartInfo.FileName = condafilepath;
                //MessageBox.Show("Conda file path \n"+condafilepath, " Info");

                //Check Caption based on that invoke the respetive conda commands
                if (Caption == "Uninstall Tool")
                {
                    // Conda uninstall command arguments
                    proc.StartInfo.Arguments = " uninstall liquidshca -y"; // if you need some
                    resultMessage = "Requested packages successfully uninstalled. \nPlease close and re-open ArcGIS Pro, to clear the installed Liquids HCA Tool.";
                    tagInstallUnistall = "uninstall";
                }
                else
                {
                    //Conda install command arguments
                    proc.StartInfo.Arguments = " install -c g2-is liquidshca -y --no-deps"; // if you need some
                    resultMessage = "Requested packages successfully installed. \nPlease close and re-open ArcGIS Pro, to use the installed Liquids HCA Tool.";
                    tagInstallUnistall = "install";
                }

                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                proc.StartInfo.CreateNoWindow = false;

                //For Default Environment Verbose windoww invoke is requried to request modify confirmation
                if (activeEnvironment.Contains("Program Files"))
                {
                    proc.StartInfo.Verb = "runas";
                }
                else
                {
                    //Assign out and Error redirect from shell, for Verbore mode it's not works
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                }

                //Start the process, after assigning all the requied parameters
                proc.Start();

                //Wait till the process executes completly
                proc.WaitForExit();
                
                //Check for error results from standara output
                string errorresult = "";
                if (!activeEnvironment.Contains("Program Files"))
                {
                    string outputresult = proc.StandardOutput.ReadToEnd();
                    errorresult = proc.StandardError.ReadLine();
                }

                if (!String.IsNullOrEmpty(errorresult))
                {
                    //Check write permission error
                    if (errorresult.Contains("CondaIOError: Missing write permissions"))
                    {
                        string errMessage = "Error: You do not have sufficient privileges to modify the active Python environment. Please obtain such privileges, " +
                      "OR create a cloned Python environment and make that your active Python environment.You may then install the Liquids HCA Tool into the active, " +
                      "cloned environment. Please refer to the Python Package Manager help topic for additional information on working with Python environments.\n\n" + tagInstallUnistall + " failed.";
                        MessageBox.Show(errMessage, "  Error");
                    }
                    else
                    {
                        //Through any other error apart from Write permissions 
                        throw new System.InvalidOperationException(errorresult);
                    }
                }
                else
                {
                    //After process check the folder is exist or not, to verfiy the proess went properly
                    string toolExists = System.IO.File.Exists(liquidsHCAToolpath) ? "Uninstall Tool" : "Install Tool";
                    //Check initial caption and after process tag
                    if (Caption == toolExists)
                    {
                        string errMessage = "Error: You do not have sufficient privileges to modify the active Python environment. Please obtain such privileges, " +
                      "OR create a cloned Python environment and make that your active Python environment.You may then install the Liquids HCA Tool into the active, " +
                      "cloned environment. Please refer to the Python Package Manager help topic for additional information on working with Python environments.\n\n" + tagInstallUnistall + " failed.";
                        MessageBox.Show(errMessage, "  Error");
                    }
                    else
                    {
                        //If everything went fine then change the Button lable, Icon and tooltip
                        if (Caption == "Uninstall Tool")
                        {
                            Caption = "Install Tool";
                            TooltipHeading = "Install Liquids HCA";
                            Tooltip = "Installs Liquids HCA Tool";
                            LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                       @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GeoprocessingToolboxNew32.png"));
                        }
                        else
                        {
                            Caption = "Uninstall Tool";
                            TooltipHeading = "Uninstall Liquids HCA";
                            Tooltip = "Uninstalls Liquids HCA Tool";
                            LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                            @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GenericDeleteRed32.png"));
                        }

                        MessageBox.Show(resultMessage, "  Result");
                    }

                }

            }

            //Handle process exceptions 
            catch (UnauthorizedAccessException ex)
            {
                string errMessage = "Error: You do not have sufficient privileges to modify the active Python environment. Please obtain such privileges, " +
                           "OR create a cloned Python environment and make that your active Python environment.You may then install the Liquids HCA Tool into the active, " +
                           "cloned environment. Please refer to the Python Package Manager help topic for additional information on working with Python environments.\n\n" + tagInstallUnistall + " failed.";
                MessageBox.Show(errMessage, "  Error");
            }

            catch (Exception e)
            {
                if (e.Message == "The operation was canceled by the user")
                {
                    string errMessage = "Error: Please allow the process to " + tagInstallUnistall + " the Liquics HCA Tool packages in the active Python environment. Please obtain such privileges, " +
                          "OR create a cloned Python environment and make that your active Python environment. You may then " + tagInstallUnistall + " the Liquids HCA Tool into the active, " +
                          "cloned environment. Please refer to the Python Package Manager help topic for additional information on working with Python environments.\n\n" + tagInstallUnistall + " failed.";
                    MessageBox.Show(errMessage, "  Error");
                }
                else
                {
                    MessageBox.Show("Error: " + e.Message + "\n" + e.StackTrace, "  Error");
                }
            }
            flagPreLoad = true;

        }
    }
}
