using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace LiquidsHCAAddIn
{
    internal class ToolInstallButton : Button
    {
        //Local variables for the tool operation
        private string activeEnvironment = "";
        private string liquidsHCAToolpath = "";
        private bool flagPreLoad = true;
        private string _packageName = " liquidshca ";
        private string _channelName = " g2-is-test "; //for Test g2-is-test  for Prod " g2-is "
        private string _sslcertName = "anacondacert.crt";
        private string _parentFolder = "G2-IS";
        private string _childFolder = "LiquidsHCA";
        private bool _isupdate = false;

        public ToolInstallButton()
        {
            //To check the roaming 
            CheckRoamingFolders();
            //Fetch the Executing assemply path
            var pathPython = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
            if (pathPython == null) return;
            pathPython = Uri.UnescapeDataString(pathPython);
            //MessageBox.Show(" pathPython \n " + pathPython, "  Info ");

            //Fetch the ProEnv.txt file where active environment path details are stored
            var proenvfilepath = System.IO.Path.Combine(pathPython.Substring(0, pathPython.LastIndexOf("ESRI")), @"ESRI\conda\envs\proenv.txt");
            //MessageBox.Show(" Pro Environemnt file path \n " + proenvfilepath);
            if (System.IO.File.Exists(proenvfilepath))
            {
                //Read the file to find the active environment to check the exsistency of installed packages            
                activeEnvironment = System.IO.File.ReadAllText(proenvfilepath);
                //Replaced escape charectors in the string
                activeEnvironment = activeEnvironment.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                //MessageBox.Show("Active Environment is \n " + activeEnvironment);
            }
            else
            {
                //Fetch the default environment where execute is there, this is where conda is also will be there
                var pathProExe = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase)).AbsolutePath);
                if (pathProExe == null) return;
                pathProExe = Uri.UnescapeDataString(pathProExe);
                activeEnvironment = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3");
            }

            //Fetch Liquids HCA tool is installed or not in active environment and assign button caption
            var liquidsHCAToolsubpath = @"Lib\site-packages\liquidshca\esri\toolboxes\LiquidsHCA.pyt";
            liquidsHCAToolpath = System.IO.Path.Combine(activeEnvironment, liquidsHCAToolsubpath);
            Caption = System.IO.File.Exists(liquidsHCAToolpath) ? "Uninstall Liquids HCA Tool" : "Install Liquids HCA Tool";

            //Button caption, Tool tip and Icon based the tool avilablity
            if (Caption == "Install Liquids HCA Tool")
            {
                Caption = "Install Liquids HCA Tool";
                TooltipHeading = "Install Liquids HCA Tool";
                Tooltip = "Installs the G2-IS Liquids HCA Tool";
                LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                   @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GeoprocessingToolboxNew32.png"));
            }
            else
            {
                Caption = "Uninstall Liquids HCA Tool";
                TooltipHeading = "Uninstall Liquids HCA Tool";
                Tooltip = "Uninstalls the G2-IS Liquids HCA Tool";
                LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                    @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GenericDeleteRed32.png"));

                //To Check and update the buttons, if updated version avilable
                this.CheckUpdatedVersion(activeEnvironment);

            }

            //Set Flag to distrigwish initial load
            flagPreLoad = false;
        }

        protected bool IsUpdatedVersion()
        {
            bool checkValue = false;
            try
            {
                //Fetch the default environment where execute is there, this is where conda is also will be there
                var pathProExe = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase)).AbsolutePath);
                if (pathProExe == null) return checkValue;
                pathProExe = Uri.UnescapeDataString(pathProExe);
                pathProExe = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3");

                //Fetch the conda path, Which is required to invoke and run Conda packages            
                var condafilepath = System.IO.Path.Combine(pathProExe.Substring(0, pathProExe.LastIndexOf("envs") - 1), @"scripts\conda");

                using (Process proc = new Process())
                {
                    string _localPackageVersion = "0";
                    string _condaPackageVersion = "0";
                    proc.StartInfo.FileName = condafilepath;
                    //MessageBox.Show("Conda file path \n"+condafilepath, " Info");                
                    proc.StartInfo.Arguments = " list " + _packageName; // if you need some

                    proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                    proc.StartInfo.CreateNoWindow = true;

                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;

                    proc.Start();
                    string outputresult = proc.StandardOutput.ReadToEnd();
                    _localPackageVersion = outputresult.Split(new string[] { "liquidshca" }, StringSplitOptions.None)[1].Split('p')[0].Trim();

                    proc.StartInfo.Arguments = " search -c " + _channelName + "  " + _packageName;

                    proc.Start();
                    string outputsearchresult = proc.StandardOutput.ReadToEnd();
                    _condaPackageVersion = outputsearchresult.Split(new string[] { "liquidshca" }, StringSplitOptions.None)[1].Split('p')[0].Trim();

                    if (_condaPackageVersion.Contains(_localPackageVersion))
                    {
                        checkValue = false;
                    }
                    else
                    {
                        checkValue = true;
                    }
                }
            }
            catch (Exception e)
            {
                checkValue = false;
            }

            return checkValue;

        }

        protected void CheckUpdatedVersion(string pathProExe)
        {
            try
            {
                //Fetch the conda path, Which is required to invoke and run Conda packages            
                var condafilepath = System.IO.Path.Combine(pathProExe.Substring(0, pathProExe.LastIndexOf("envs") - 1), @"scripts\conda");

                this.CheckUpdatedVersionAync(condafilepath);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in checking latest version " + e.Message, "   Error ");
            }
        }

        private async void CheckUpdatedVersionAync(string condafilepath)
        {
            try
            {
                string _condaPackageVersion = "0";
                string _localPackageVersion = "0";
                // Process to fetch Conda package details
                using (var process = new Process
                {
                    StartInfo =
                                {
                                    FileName = condafilepath, Arguments = " search -c " + _channelName + "  " + _packageName,
                                    UseShellExecute = false, CreateNoWindow = true,
                                    RedirectStandardOutput = true, RedirectStandardError = true
                                },
                    EnableRaisingEvents = true
                })
                {
                    //Run package search from conda to fetch the current package version from Conda
                    var outputsearchresult = await FetchCondaPackageVersionAsync(process).ConfigureAwait(false);

                    //Run package list from local to fetch the current linstalled package version
                    _localPackageVersion = FetchLocalPackageVersion(condafilepath, _packageName);

                    _condaPackageVersion = outputsearchresult.Split(new string[] { "liquidshca" }, StringSplitOptions.None)[1].Split('p')[0].Trim();

                    //Check the both version or diffrent, then invoke update button.
                    if (!_condaPackageVersion.Contains(_localPackageVersion))
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                        {
                        // Update UI component here
                        Caption = "Update Liquids HCA Tool";
                            TooltipHeading = "Updates Liquids HCA Tool";
                            Tooltip = "Updates the G2-IS Liquids HCA Tool from version " + _localPackageVersion + " to version '" + _condaPackageVersion;
                            LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                                @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GeoprocessingToolboxPythonNew32.png"));

                        });
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in latest version checking " + e.Message, "  Error");
            }
        }

        private static Task<string> FetchCondaPackageVersionAsync(Process process)
        {
            var tcs = new TaskCompletionSource<string>();
            string resultmsg = "", errmsg = "";
            process.Exited += (s, ea) => tcs.SetResult(resultmsg);
            process.OutputDataReceived += (s, ea) => resultmsg += ea.Data;
            process.ErrorDataReceived += (s, ea) => errmsg = ea.Data;

            bool started = process.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        private static string FetchLocalPackageVersion(string condafilepath, string _packageName)
        {
            string _localPackageVersion = "0";
            using (Process proc = new Process())
            {

                proc.StartInfo.FileName = condafilepath;
                //MessageBox.Show("Conda file path \n"+condafilepath, " Info");                
                proc.StartInfo.Arguments = " list " + _packageName; // if you need some

                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                proc.StartInfo.CreateNoWindow = true;

                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;

                proc.Start();
                string outputresult = proc.StandardOutput.ReadToEnd();
                _localPackageVersion = outputresult.Split(new string[] { "liquidshca" }, StringSplitOptions.None)[1].Split('p')[0].Trim();

            }
            return _localPackageVersion;

        }

        protected static void CheckRoamingFolders()
        {
            string pathRoamingAppData = Environment.ExpandEnvironmentVariables("%APPDATA%");
            var pathSSLFilePath_R1D = System.IO.Path.Combine(pathRoamingAppData, "G2-IS");
            if (!System.IO.Directory.Exists(pathSSLFilePath_R1D))
            {
                System.IO.Directory.CreateDirectory(pathSSLFilePath_R1D);
            }

            var pathSSLFilePath_R2D = System.IO.Path.Combine(pathRoamingAppData, "G2-IS", "LiquidsHCA");
            if (!System.IO.Directory.Exists(pathSSLFilePath_R2D))
            {
                System.IO.Directory.CreateDirectory(pathSSLFilePath_R2D);
            }
        }
        protected override void OnClick()
        {
            string tagInstallUnistall = "";
            try
            {
                string resultMessage = "";
                string errorresult = "";
                //Don't load at initial load
                if (flagPreLoad)
                {
                    //Fetch the Executing assemply path
                    var pathPython = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
                    if (pathPython == null) return;
                    pathPython = Uri.UnescapeDataString(pathPython);

                    //Fetch the ProEnv.txt file where active environment path details are stored
                    var proenvfilepath = System.IO.Path.Combine(pathPython.Substring(0, pathPython.LastIndexOf("ESRI")), @"ESRI\conda\envs\proenv.txt");

                    if (System.IO.File.Exists(proenvfilepath))
                    {
                        //Read the file to find the active environment to check the exsistency of installed packages            
                        activeEnvironment = System.IO.File.ReadAllText(proenvfilepath);
                        //Replaced escape charectors in the string
                        activeEnvironment = activeEnvironment.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                        //MessageBox.Show("Active Environment is \n " + activeEnvironment);
                    }
                    else
                    {
                        //Fetch the default environment where execute is there, this is where conda is also will be there
                        var pathProExe1 = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase)).AbsolutePath);
                        if (pathProExe1 == null) return;
                        pathProExe1 = Uri.UnescapeDataString(pathProExe1);
                        activeEnvironment = System.IO.Path.Combine(pathProExe1, @"Python\envs\arcgispro-py3");
                    }

                    //Tool path in active environment
                    var liquidsHCAToolsubpath = @"Lib\site-packages\liquidshca\esri\toolboxes\LiquidsHCA.pyt";
                    liquidsHCAToolpath = System.IO.Path.Combine(activeEnvironment, liquidsHCAToolsubpath);

                    //Assign Button caption based other tool avilability in active environment
                    if (_isupdate)
                    {
                        Caption = "Update Liquids HCA Tool";
                        TooltipHeading = "Update Liquids HCA Tool";
                        Tooltip = "Update the G2-IS Liquids HCA Tool";
                        LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                            @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GeoprocessingToolboxPythonNew32.png"));
                    }
                    else
                    {
                        Caption = System.IO.File.Exists(liquidsHCAToolpath) ? "Uninstall Liquids HCA Tool" : "Install Liquids HCA Tool";
                    }

                }

                //Fetch the default environment where execute is there, this is where conda is also will be there
                var pathProExe = System.IO.Path.GetDirectoryName((new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase)).AbsolutePath);
                if (pathProExe == null) return;
                pathProExe = Uri.UnescapeDataString(pathProExe);
                pathProExe = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3");

                //Fetch the conda path, Which is required to invoke and run Conda packages
                var condafilepath = System.IO.Path.Combine(pathProExe.Substring(0, pathProExe.LastIndexOf("envs") - 1), @"scripts\conda");

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = condafilepath;

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

                    //Check Caption based on that invoke the respetive conda commands
                    if (Caption == "Uninstall Liquids HCA Tool")
                    {
                        // Conda uninstall command arguments    
                        proc.StartInfo.Arguments = " uninstall " + _packageName + " -y"; // if you need some
                        resultMessage = "Requested packages successfully uninstalled. \nPlease close and re-open ArcGIS Pro, to clear the installed Liquids HCA Tool.";
                        tagInstallUnistall = "uninstall";
                    }
                    else
                    {
                        //// Check the process need to consider ssl certificate in local app data path
                        //string pathLocalAppData = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%");
                        
                        //var pathSSLFilePath = System.IO.Path.Combine(pathLocalAppData, _sslcertName);                        
                        //if (System.IO.File.Exists(pathSSLFilePath))
                        //{                            
                        //    proc.StartInfo.Arguments = " config --set ssl_verify " + pathSSLFilePath + " ";
                        //    proc.Start();
                        //}
                        //else
                        //{
                            string pathRoamingAppData = Environment.ExpandEnvironmentVariables("%APPDATA%");

                            //var pathSSLFilePath_R = System.IO.Path.Combine(pathRoamingAppData, _sslcertName);
                            //if (System.IO.File.Exists(pathSSLFilePath_R))
                            //{
                            //    proc.StartInfo.Arguments = " config --set ssl_verify " + pathSSLFilePath_R + " ";
                            //    proc.Start();
                            //}
                            //else
                            //{
                                var pathSSLFilePath_R1D = System.IO.Path.Combine(pathRoamingAppData, _parentFolder);
                                if(System.IO.Directory.Exists(pathSSLFilePath_R1D))
                                {
                                    var pathSSLFilePath_R1 = System.IO.Path.Combine(pathRoamingAppData, _parentFolder, _sslcertName);
                                    if (System.IO.File.Exists(pathSSLFilePath_R1))
                                    {
                                        proc.StartInfo.Arguments = " config --set ssl_verify " + pathSSLFilePath_R1 + " ";
                                        proc.Start();
                                    }
                                    else
                                    {
                                        var pathSSLFilePath_R2D = System.IO.Path.Combine(pathRoamingAppData, _parentFolder, _childFolder);
                                        if (System.IO.Directory.Exists(pathSSLFilePath_R2D))
                                        {
                                            var pathSSLFilePath_R2 = System.IO.Path.Combine(pathRoamingAppData, _parentFolder, _childFolder, _sslcertName);
                                            if (System.IO.File.Exists(pathSSLFilePath_R2))
                                            {
                                                proc.StartInfo.Arguments = " config --set ssl_verify " + pathSSLFilePath_R2 + " ";
                                                proc.Start();
                                            }
                                        }
                                    }
                                }
                            //}
                        //}

                        //Conda install command arguments
                        proc.StartInfo.Arguments = " install -c " + _channelName + " " + _packageName + "  -y --no-deps"; // if you need some
                        if (Caption == "Update Liquids HCA Tool")
                        {
                            resultMessage = "Requested packages successfully updated. \nPlease close and re-open ArcGIS Pro, to use the updated Liquids HCA Tool.";
                            tagInstallUnistall = "update";
                        }
                        else
                        {
                            resultMessage = "Requested packages successfully installed. \nPlease close and re-open ArcGIS Pro, to use the installed Liquids HCA Tool.";
                            tagInstallUnistall = "install";
                        }
                    }
                    //Start the process, after assigning all the requied parameters
                    proc.Start();
                    //Wait till the process executes completly                   
                    //proc.WaitForExit(15 * 60 * 1000);
                    while (proc.Responding)
                    {
                        Thread.Sleep(5 * 1000);
                    }

                    //Check for error results from standard output
                    if (!activeEnvironment.Contains("Program Files"))
                    {
                        string outputresult = proc.StandardOutput.ReadToEnd();
                        errorresult = proc.StandardError.ReadLine();
                    }
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
                    string toolExists = System.IO.File.Exists(liquidsHCAToolpath) ? "Uninstall Liquids HCA Tool" : "Install Liquids HCA Tool";
                    //Check initial caption and after process tag
                    if (Caption == toolExists)
                    {
                        string errMessage = "Error: The process isnot installed. Please try again.\n\n" + tagInstallUnistall + " failed.";
                        MessageBox.Show(errMessage, "  Error");
                    }
                    else
                    {
                        //If everything went fine then change the Button lable, Icon and tooltip
                        if (Caption == "Uninstall Liquids HCA Tool")
                        {
                            Caption = "Install Liquids HCA Tool";
                            TooltipHeading = "Install Liquids HCA Tool";
                            Tooltip = "Installs the G2-IS Liquids HCA Tool";
                            LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                       @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GeoprocessingToolboxNew32.png"));
                        }
                        else
                        {
                            Caption = "Uninstall Liquids HCA Tool";
                            TooltipHeading = "Uninstall Liquids HCA Tool";
                            Tooltip = "Uninstalls the G2-IS Liquids HCA Tool";
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
