using evoleap.Licensing.Desktop;
using evoleap.Licensing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Runtime.Serialization;
using System.Xml;
//using Python.Runtime;
using ActiproSoftware.Products.Logging;
using System.DirectoryServices.AccountManagement;
//using IronPython.Hosting;
//using Microsoft.Scripting.Hosting;
using System.Security.Policy;
using System.Runtime;
using System.Diagnostics;
using System.Data.SqlTypes;


namespace LiquidsHCAAddIn_3
{
    internal class EvoleapLicenseManager
    {


        //private string evoleap_server = @"https://elm.evoleap.com/";
        //private string rsa_public_key = @"-----BEGIN PUBLIC KEY-----\n\
        //                       MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxVeHxDKgjjfiEKYRGbQ4\n\
        //                       xf6+JkRFjHi16Bc6jRs2Y8IWfp5KEuaMCqRF7w1tfjbUrfjpY0u2HTeDnJwETAmX\n\
        //                       vE3qzhnjaVlUKua60BgzKRNLOKx8DE55NX21U9N7jsnmRpe3gt/dJkMWiyANjPi5\n\
        //                       NdYR3NC3+tXHbO5gQiqobtns3+omGKxi4kfB8BEXGMQPnUui3M9VZMTi0lLLevUo\n\
        //                       v/jIUQ+x78jqUx5dFn38Nz1ErK7O/xsYdwSoQmDKjdlAhkzzHkgViR6JpJI9Yj5/\n\
        //                       NPxLh3fLiSx6UzAaa3MUNBO6CO/hMTePhWQsyY71D1uye+m7UsTG5xs1MQGnUJwQ\n\
        //                       oQIDAQAB\n\
        //                       -----END PUBLIC KEY-----";
        private string product_id = "9145E227-422B-434E-89A2-4EEB7AF106D4";
        ////private string version = "2.6.01";

        //Guid product_id = Guid.Parse("9145E227-422B-434E-89A2-4EEB7AF106D4");
        //Version version = Version.Parse("2.6.01");

        public bool _isDisableUpdates = false;


        public EvoleapLicenseManager(string python_exe)
        {
            _isDisableUpdates = CheckDisableUpdatesFeature(python_exe);
        }
       
        private bool CheckDisableUpdatesFeature(string python_exe)
        {
            bool isUpdates = false;

            try
            {
                //ConnectionSettings.Host = evoleap_server;

                //string mac_id = GetMacAddress();
                ////mac_id = "0c:91:92:a5:a6:b2";
                //string hdd_id = GetDiskSerialNumber();

                string appDataDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(appDataDir, "G2-IS", "LiquidsHCA", product_id + ".pkl");

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Product not registered");
                    return isUpdates;
                }

                /////////////////////////////////
                //Environment.SetEnvironmentVariable("PYTHONPATH", python_exe);

                string installPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string file_name = System.IO.Path.Combine(installPath, "Scripts", "load_pickle_file.py");

                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(python_exe, file_name)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                p.Start();
                while (!p.WaitForExit(5000)) ;

                string output = p.StandardOutput.ReadToEnd();
                isUpdates = output.Contains("True");
                return isUpdates;

                //p.WaitForExit();

                //Console.WriteLine(output);

                //Console.ReadLine();




















                //ScriptEngine engine = Python.CreateEngine();

                //var searchPaths = engine.GetSearchPaths();
                //searchPaths = [];
                //searchPaths.Add(@"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\Lib");
                //searchPaths.Add(@"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\Lib\site-packages");

                //searchPaths.Add(@"C:\Users\surendra.pinjala\AppData\Local\ESRI\conda\envs\arcgispro-py3-clone\Lib");
                //searchPaths.Add(@"C:\Users\surendra.pinjala\AppData\Local\ESRI\conda\envs\arcgispro-py3-clone\Lib\site-packages");

                //searchPaths.Add(@"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\DLLs");
                //searchPaths.Add(@"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\Lib\site-packages\win32");
                //searchPaths.Add(@"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\Lib\site-packages\win32\lib");
                //searchPaths.Add(@"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\Lib\site-packages\pythonwin");
                //engine.SetSearchPaths(searchPaths);

                //ScriptRuntime runtime = engine.Runtime;

                // Optionally set environment variables for the Python environment

                //runtime.IO.SetOutput(Console.OpenStandardOutput(), System.Text.Encoding.UTF8);

                //runtime.IO.SetErrorOutput(Console.OpenStandardError(), System.Text.Encoding.UTF8);

                //string installPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                //var file_Path = System.IO.Path.Combine(installPath, "Scripts", "load_pickle_file.py");
                //if (!File.Exists(file_Path))
                //{
                //    Console.WriteLine("load_pickle_file.py file does not exist!");
                //    return isUpdates;
                //}
                //return isUpdates;
                //var scope = engine.ExecuteFile(file_Path);

                //var result = scope.GetVariable<string>("disable_updates");

                //MessageBox.Show(result.ToString());
                //if (result.ToString() == "True") return true;               
                //else return false;




                //using (Py.GIL())
                //{
                //    dynamic py = Py.Import("load_pickle_file.py");
                //    dynamic data = py.data;

                //    // Access the data from the Python object
                //    foreach (var item in data)
                //    {
                //        Console.WriteLine(item);
                //    }
                //}

                // Assuming you're in a domain environment               

                //string py_home = @"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3";
                //string py_path = $"{py_home};";

                //// will be different on linux/mac
                //string[] py_paths = { "DLLs", "lib", "lib/site-packages", "lib/site-packages/win32", "lib/site-packages/win32/lib", "lib/site-packages/Pythonwin" };

                //foreach (string p in py_paths)
                //{
                //    py_path += $"{py_home}/{p};";
                //}
                //try
                //{
                //    //PythonEngine.PythonPath = $"{Program.ScriptsDir};{py_path}";
                //    PythonEngine.PythonHome = @"C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3";
                //    //PythonEngine.ProgramName = Program.ApplicationName;
                //    PythonEngine.Initialize();
                //    Console.WriteLine("Python initialized successfully.");
                //    PythonEngine.BeginAllowThreads();

                //    Console.WriteLine("Python Version: {v}, {dll}", PythonEngine.Version.Trim(), Runtime.PythonDLL);
                //    Console.WriteLine("Python Home: {home}", PythonEngine.PythonHome);
                //    //Console.WriteLine("Python Path: {path}", PythonEngine.PythonPath);
                //}
                //catch (System.TypeInitializationException e)
                //{
                //    throw new Exception($"FATAL, Unable to load Python, dll={Runtime.PythonDLL}", e);
                //}

                /////////////////////////////////////////////

                // Initialize Python runtime
                //PythonEngine.Initialize();

                //// Load the Python module containing the pickle object
                //dynamic module = PyModule.Import("pickle");

                //// Load the pickled object from the file
                //dynamic data = module.load(filePath, "rb");

                //// Access the data
                //Console.WriteLine(data); // Print the entire object
                //Console.WriteLine(data["key"]); // Access a specific key (if applicable)


                //IValidationState savedState;

                //DataContractSerializer serializer = new DataContractSerializer(typeof(IValidationState));
                //using (FileStream fs = new FileStream(filePath, FileMode.Open))
                //{
                //    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                //    savedState = (IValidationState)serializer.ReadObject(reader);
                //}

                //Dictionary<string, string> userIdentityInfo = new Dictionary<string, string>();
                //userIdentityInfo["UPN"] = System.Environment.UserName;


                //IEnumerable<KeyValuePair<string, InstanceIdentityValue>> instanceIdentityInfo = new[]
                //{
                //    new KeyValuePair<string, InstanceIdentityValue>("mac", new InstanceIdentityValue(mac_id)),
                //    new KeyValuePair<string, InstanceIdentityValue>("hddsn", new InstanceIdentityValue(hdd_id))
                //};

                //UserIdentity ui = new UserIdentity(userIdentityInfo);
                //InstanceIdentity ii = new InstanceIdentity(instanceIdentityInfo);

                //internal ControlManager(
                //    Guid productId, 
                //    Version version, 
                //    ILicensingWebService webService, 
                //    UserIdentity userIdentity, 
                //    InstanceIdentity instanceIdentity, 
                //    ControlStrategy controlStrategy = null, 
                //    IValidationState savedState = null, 
                //    IValidatedSessionState sessionState = null)

                //}

                //PublicKeyFile pf = PublicKeyFile.Create(rsa_public_key);
                ////ControlManager controlManager = new ControlManager(product_id, version, pf, ui, ii, null, savedState);
                //ControlManager controlManager = new ControlManager(product_id, version, pf, ui, ii);
                //var state = controlManager.ValidationState;
                //return isUpdates;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error in checking liqudishca tool exists." + e.Message, "  Error");
                return isUpdates;
            }

        }

        private string GetMacAddress()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        return mo["MACAddress"].ToString();
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return string.Empty;
            }
        }

        private string GetDiskSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject disk in searcher.Get())
                {
                    return disk["SerialNumber"].ToString();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return string.Empty;
            }
        }

    }
}
