using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Internal.Core.Conda;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using Microsoft.Win32;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using static System.Net.WebRequestMethods;
using System.Windows.Automation;
using evoleap.Licensing;
using System.Management;
using System.Collections.Generic;
using evoleap.Licensing.Desktop;


namespace LiquidsHCAAddIn_3
{
    internal class ToolInstallButton : Button
    {
        //Local variables for the tool operation
        private string activeEnvironment = @"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3";
        private string liquidsHCAToolpath = "";
        //private bool flagPreLoad = true;
        private string _packageName = " liquidshca";
        private string _channelName = " g2-is-test "; //for Test g2-is-test  for Prod " g2-is "
        private string _sslcertName = "anacondacert.crt";
        private string _parentFolder = "G2-IS";
        private string _childFolder = "LiquidsHCA";
        //private bool _isupdate = false;
        private string condafilepath = @"c:\Program Files\ArcGIS\Pro\bin\Python\scripts\conda.exe"; // ArcGIS Pro installed for all users
        //private string condafilepath = @"%LOCALAPPDATA%\ArcGIS\Pro\bin\Python\scripts\conda.exe"; // ArcGIS Pro installed for the current user
        private string _localPackageVersion = "0";        
        private bool _isCloneEnvironment = false;
        private bool _isLiquidsExists = false;
        private bool _isAdmin = false;
        string liquidsHCAToolsubpath = @"Lib\site-packages\liquidshca\esri\toolboxes\LiquidsHCA.pyt";
        private string liquidsHCAFolder = @"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\Lib\site-packages\liquidshca";
      
        private bool _isDisableUpdates = false;
        private string _package_version = "=3.3.1";

        public ToolInstallButton()
        {
            try
            { 
                //get Pro installation path
                CheckActiveEnvironment();

                // check for disable updates
                string python_exe = System.IO.Path.Combine(activeEnvironment, "python.exe");
                if (!System.IO.File.Exists(python_exe))
                {
                    python_exe = @"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\python.exe";
                   
                }

                EvoleapLicenseManager elm = new EvoleapLicenseManager(python_exe);

                _isDisableUpdates = elm._isDisableUpdates;


                _isLiquidsExists = System.IO.File.Exists(liquidsHCAToolpath) ? true : false;

                //To check the folder for SSL certificates
                CheckRoamingFolders();             

                if (_isLiquidsExists)
                {
                    Caption = "Uninstall Liquids HCA Tool";
                    TooltipHeading = "Uninstall Liquids HCA Tool";
                    Tooltip = "Uninstalls the G2-IS Liquids HCA Tool";
                    LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                        @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GenericDeleteRed32.png"));

                    if (!_isDisableUpdates)
                    {
                        //To Check and update the buttons, if updated version avilable
                        this.CheckUpdatedVersionAync();
                    }
                    
                }
                else
                {
                    Caption = "Install Liquids HCA Tool";
                    TooltipHeading = "Install Liquids HCA Tool";
                    Tooltip = "Installs the G2-IS Liquids HCA Tool";
                    LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                       @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/GeoprocessingToolboxNew32.png"));
                }              
            }
            
            catch (Exception e)
            {
                MessageBox.Show("Error in checking Liquids HCA Toolbox path " + e.Message, "   Error ");
            }
        }

        private void CheckActiveEnvironment()
        {
            
            try
            {
                RegistryKey machineRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ESRI\ArcGISPro");
                object machineActiveEnv = machineRegistryKey.GetValue("PythonCondaEnv"); // arcgispro-py3
                string strMachineActiveEnv = machineActiveEnv.ToString();
                object machineEnvPath = machineRegistryKey.GetValue("PythonCondaRoot"); // C:\Program Files\ArcGIS\Pro\bin\Python
                string strMachineEnvPath = machineEnvPath.ToString();
                object machineInstallDir = machineRegistryKey.GetValue("InstallDir"); // C:\Program Files\ArcGIS\Pro\
                activeEnvironment = System.IO.Path.Combine(strMachineEnvPath, @"envs", strMachineActiveEnv);

                condafilepath = System.IO.Path.Combine(strMachineEnvPath, @"scripts\conda.exe");

                RegistryKey userRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ESRI\ArcGISPro");
               
                if (userRegistryKey.GetValueNames().Contains("PythonCondaEnv"))
                {
                    object userActiveEnv = userRegistryKey.GetValue("PythonCondaEnv");
                    string strUserActiveEnv = userActiveEnv.ToString();   //arcgispro-py3 OR C:\Users\<user>\AppData\Local\ESRI\conda\envs\arcgispro-py3-clone
                    if (strUserActiveEnv != strMachineActiveEnv && strUserActiveEnv is not null)
                    {
                        activeEnvironment = strUserActiveEnv;
                    }

                    //if (strUserActiveEnv == strMachineActiveEnv || strUserActiveEnv is null)
                    //{
                    //    activeEnvironment = System.IO.Path.Combine(strMachineEnvPath, @"envs", strMachineActiveEnv); //C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3
                    //}
                }

                //object userActiveEnv = userRegistryKey.GetValue("PythonCondaEnv"); //C:\Users\<user>\AppData\Local\ESRI\conda\envs\arcgispro-py3-clone
                //activeEnvironment = userActiveEnv.ToString();
                //if (activeEnvironment == strMachineActiveEnv || activeEnvironment is null)
                //{
                //    activeEnvironment = System.IO.Path.Combine(strMachineEnvPath, @"envs", strMachineActiveEnv); //C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3
                //}
                if (!activeEnvironment.Contains("Program Files"))
                {
                    _isCloneEnvironment = true;
                }

                liquidsHCAToolsubpath = @"Lib\site-packages\liquidshca\esri\toolboxes\LiquidsHCA.pyt";
                liquidsHCAToolpath = System.IO.Path.Combine(activeEnvironment, liquidsHCAToolsubpath);

                //_isLiquidsExists = System.IO.File.Exists(liquidsHCAToolpath) ? true : false;

                liquidsHCAFolder = System.IO.Path.Combine(activeEnvironment, @"Lib\site-packages\liquidshca");

            }
            catch (Exception e)
            {
                MessageBox.Show("Error in checking liqudishca tool exists." + e.Message, "  Error");
            }
           
        }

        private async void CheckUpdatedVersionAync()
        {
            try
            {
                string _condaPackageVersion = "0";
                
                // Process to fetch Conda package details
                using (var process = new Process
                {
                    StartInfo =
                                {
                                    FileName = condafilepath,
                                    Arguments = " search -c " + _channelName + "  " + _packageName,
                                    //Arguments = " search -c " + _channelName + "  " + _packageName + " --json",
                                    //Arguments = " search -c " + _channelName + "  " + _packageName + " | tail -n1",
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                },
                    EnableRaisingEvents = true
                })
                {
                    //Run package search from conda to fetch the current package version from Conda
                    var outputsearchresult = await FetchCondaPackageVersionAsync(process).ConfigureAwait(false);                 

                    string[] latestVersion = outputsearchresult.Split(new string[] { "liquidshca" }, StringSplitOptions.None);
                    int length = latestVersion.Length;
                    _condaPackageVersion = latestVersion[length - 1].Split('p')[0].Trim();
                    FetchLocalPackageVersion();
                    Version localversion = new Version(_localPackageVersion);
                    Version latestversion = new Version(_condaPackageVersion);

                    var versionresult = localversion.CompareTo(latestversion);
                    if (versionresult < 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                       {
                            // Update UI component here
                           Caption = "Update Liquids HCA Tool";
                           TooltipHeading = "Updates Liquids HCA Tool";
                           Tooltip = "Updates the G2-IS Liquids HCA Tool from version " + _localPackageVersion + " to version " + _condaPackageVersion;
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

        protected static void CheckRoamingFolders()
        {
            try
            {
                string pathRoamingAppData = System.Environment.ExpandEnvironmentVariables("%APPDATA%");
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
            catch (Exception e)
            {
                MessageBox.Show("Error in checking roaming folder for SSL certificates" + e.Message, "  Error");
            }
        }

        protected override void OnClick()
        {
            CheckActiveEnvironment();
            //_isLiquidsExists = System.IO.File.Exists(liquidsHCAToolpath) ? true : false;
            string tagInstallUnistall = "";
            try
            {

                string resultMessage = "";
                //string errorresult = "";
                //              


                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = condafilepath;

                    proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                    proc.StartInfo.CreateNoWindow = false;

                    if (!_isCloneEnvironment)
                    {
                        proc.StartInfo.Verb = "runas";
                        _isAdmin = checkIsAdmin();
                        if (_isAdmin == false)
                        {
                            proc.StartInfo.UseShellExecute = true;
                        }
                    }

                    //Check Caption based on that invoke the respetive conda commands
                    if (Caption == "Uninstall Liquids HCA Tool")
                    {
                        // Conda uninstall command arguments    
                        //proc.StartInfo.Arguments = " uninstall " + _packageName + " -y"; // if you need some
                        proc.StartInfo.Arguments = " remove --force " + _packageName + " -y";
                        resultMessage = "Requested packages successfully uninstalled. \nPlease close and re-open ArcGIS Pro, to clear the installed Liquids HCA Tool.";
                        tagInstallUnistall = "uninstall";
                    }
                    else
                    {
                        // Check the process need to consider ssl certificate in local app data path                       
                        string pathRoamingAppData = System.Environment.ExpandEnvironmentVariables("%APPDATA%");

                        var pathSSLFilePath_R1D = System.IO.Path.Combine(pathRoamingAppData, _parentFolder);
                        if (System.IO.Directory.Exists(pathSSLFilePath_R1D))
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

                        //Conda install command arguments
                        if (_isDisableUpdates)
                        {                            
                            proc.StartInfo.Arguments = " install -c " + _channelName + " " + _packageName + _package_version + "  --no-deps  -y";
                        }
                        else
                        {
                            proc.StartInfo.Arguments = " install -c " + _channelName + " " + _packageName + "  --no-deps  -y";
                        }
                        //proc.StartInfo.Arguments = " install -c " + _channelName + " " + _packageName + "  --no-deps  -y"; 
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

                    while (!proc.WaitForExit(5000)) ;
                }
                _isLiquidsExists = System.IO.File.Exists(liquidsHCAToolpath) ? true : false;
                //After process check the folder is exist or not, to verfiy the proess went properly
                string toolExists = _isLiquidsExists ? "Uninstall Liquids HCA Tool" : "Install Liquids HCA Tool";
                //Check initial caption and after process tag
                if (Caption == toolExists)
                {
                    string errMessage = "Error: The process is not installed. \nPlease run ArcGIS Pro as an Administrator and try again.\n\n" + tagInstallUnistall + " failed.";
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
                    //if (!_isCloneEnvironment)
                    //{
                    //    ApplyFilePermissions();
                    //}
                        

                    MessageBox.Show(resultMessage, "  Result");
                }

                //ApplyFilePermissions();
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
            //flagPreLoad = true;
        }

        protected void OnClick_old()
        {
            CheckActiveEnvironment();
            //_isLiquidsExists = System.IO.File.Exists(liquidsHCAToolpath) ? true : false;
            string tagInstallUnistall = "";
            try
            {               

                string resultMessage = "";
                //string errorresult = "";                         

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = condafilepath;

                    proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                    proc.StartInfo.CreateNoWindow = false;                   
                    
                    if (!_isCloneEnvironment) 
                    {
                        proc.StartInfo.Verb = "runas";
                        _isAdmin = checkIsAdmin();
                        if (_isAdmin == false)
                        {                            
                            proc.StartInfo.UseShellExecute = true;
                        }                            
                    }                   

                    //Check Caption based on that invoke the respetive conda commands
                    if (Caption == "Uninstall Liquids HCA Tool")
                    {
                        // Conda uninstall command arguments    
                        //proc.StartInfo.Arguments = " uninstall " + _packageName + " -y"; // if you need some
                        proc.StartInfo.Arguments = " remove --force " + _packageName + " -y"; 
                        resultMessage = "Requested packages successfully uninstalled. \nPlease close and re-open ArcGIS Pro, to clear the installed Liquids HCA Tool.";
                        tagInstallUnistall = "uninstall";
                    }
                    else
                    {
                        // Check the process need to consider ssl certificate in local app data path                       
                        string pathRoamingAppData = System.Environment.ExpandEnvironmentVariables("%APPDATA%");
                          
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

                        //Conda install command arguments
                        proc.StartInfo.Arguments = " install -c " + _channelName + " " + _packageName + "  --no-deps  -y"; // if you need some
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

                    while (!proc.WaitForExit(5000)) ;
                }
                _isLiquidsExists = System.IO.File.Exists(liquidsHCAToolpath) ? true : false;
                //After process check the folder is exist or not, to verfiy the proess went properly
                string toolExists = _isLiquidsExists ? "Uninstall Liquids HCA Tool" : "Install Liquids HCA Tool";
                //Check initial caption and after process tag
                if (Caption == toolExists)
                {
                    string errMessage = "Error: The process is not installed. \nPlease run ArcGIS Pro as an Administrator and try again.\n\n" + tagInstallUnistall + " failed.";
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
                    ApplyFilePermissions();

                    MessageBox.Show(resultMessage, "  Result");
                }

                //ApplyFilePermissions();
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
            //flagPreLoad = true;
        }

        private bool checkIsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void FetchLocalPackageVersion()
        {
            try {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = condafilepath;                      
                    proc.StartInfo.Arguments = " list " + _packageName; 
                    proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;

                    proc.Start();
                    string outputresult = proc.StandardOutput.ReadToEnd();
                    _localPackageVersion = outputresult.Split(new string[] { "liquidshca" }, StringSplitOptions.None)[1].Split('p')[0].Trim();
                }
            }
            catch (Exception e) {
                MessageBox.Show("Error in checking installed liquidshca version!" + e.Message, "  Error");
            }
        }

        private void ApplyFilePermissions()
        {
            if (System.IO.Directory.Exists(liquidsHCAFolder))
            {             

                try
                {                       
                    using (Process proc1 = new Process())
                    {
                        proc1.StartInfo.FileName =  "CMD.exe";
                        //proc1.StartInfo.Arguments = "/c icacls " + liquidsHCAFolder + " /INHERITANCE:e /T ";
                        proc1.StartInfo.Arguments = "/c icacls " + liquidsHCAFolder + " /inheritance:e /grant:r Everyone:(OI)(CI)RX /T";
                        proc1.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                        proc1.StartInfo.CreateNoWindow = true;
                        //proc1.StartInfo.ErrorDialog = true;
                        proc1.StartInfo.Verb = "runas";
                        //_isAdmin = checkIsAdmin();
                        //if (_isAdmin == false)
                        //{
                            proc1.StartInfo.UseShellExecute = true;
                        //}

                                             
                        proc1.Start();                       

                        //proc1.WaitForExit();
                        while (!proc1.WaitForExit(5000)) ;

                        //string q = "";
                        //while (!proc1.HasExited)
                        //{
                        //    q += proc1.StandardOutput.ReadToEnd();
                        //}

                        //MessageBox.Show(q);  


                        //while (!proc1.WaitForExit(1000)) ;
                        //proc1.CloseMainWindow();
                        //proc1.Close();
                        //string outputresult = proc1.StandardOutput.ReadToEnd();
                        //Console.WriteLine(proc1.StandardOutput.ReadToEnd());
                        //StreamReader myStreamReader = proc1.StandardError;                        
                        //Console.WriteLine(myStreamReader.ReadLine());
                        //MessageBox.Show("Updated file persmissions!");

                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error in setting liquidshca package folder/file permissions, please set file permissions to allow all users to access Liquids HCA Tools!" );
                }



               
            }
        }

       

    }
}
