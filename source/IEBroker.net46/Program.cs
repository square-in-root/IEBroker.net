using System;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using Microsoft.Win32;
using System.Security.Principal;


namespace IEBroker.net46
{
    class Program
    {
        private static string protocolGeneral = Constant.DEFAULT_PROTOCOL_GENERAL;
        private static string protocolSecure  = Constant.DEFAULT_PROTOCOL_SECURE;
        private static string executablePath  = Constant.DEFAULT_EXECUTABLE_PATH;
        private static string brokerStatusReportUrl = null;


        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return Constant.ERROR_ARGS_NOT_EXIST;
            }

            LoadConfiguration();

            switch (args[0])
            {
                case Constant.MODE_INSTALL:
                    ProgramInstall();
                    break;
                case Constant.MODE_REMOVE:
                    ProgramRemove();
                    break;
                case Constant.MODE_EXECUTE:
                case Constant.MODE_EXECUTE_HIDDEN:
                    RunInternetExplorer(args[1]);
                    break;
                default:
                    PrintUsage();
                    return Constant.ERROR_ARG0_NO_OPTION;
            }

            return Constant.ERROR_NO_ERROR;
        }


        private static void LoadConfiguration()
        {
            String t;
            t = ConfigurationManager.AppSettings["ProtocolGeneral"];
            if (t != null) Program.protocolGeneral = t;

            t = ConfigurationManager.AppSettings["ProtocolSecure"];
            if (t != null) Program.protocolSecure = t;

            t = ConfigurationManager.AppSettings["ExecuteablePath"];
            if (t != null) Program.executablePath = t;

            t = ConfigurationManager.AppSettings["BrokerStatusReportUrl"];
            if (t != null) Program.brokerStatusReportUrl = t;
        }


        private static void ProgramInstall()
        {
            if (IsAdministrator())
            {
                String sourceUri = Assembly.GetExecutingAssembly().CodeBase;
                String sourcePath = new Uri(sourceUri).LocalPath;
                String sourceDir = Path.GetDirectoryName(sourcePath);

                String targetDir = Environment.GetEnvironmentVariable("ProgramFiles") + Path.DirectorySeparatorChar + "IEBroker.net";

                String brokerFile = Path.GetFileName(sourcePath);
                String configFile = brokerFile + ".config";

                String sourceFile1 = sourceDir + Path.DirectorySeparatorChar + brokerFile;
                String targetFile1 = targetDir + Path.DirectorySeparatorChar + brokerFile;

                String sourceFile2 = sourceDir + Path.DirectorySeparatorChar + configFile;
                String targetFile2 = targetDir + Path.DirectorySeparatorChar + configFile;

                Console.WriteLine("[1/4] Create Directory : " + targetDir);
                Directory.CreateDirectory(targetDir);

                Console.WriteLine("[2/4] Copy Executable : " + brokerFile);
                if (File.Exists(sourceFile1)) File.Copy(sourceFile1, targetFile1, true);

                Console.WriteLine("[3/4] Copy Config File : " + configFile);
                if (File.Exists(sourceFile2)) File.Copy(sourceFile2, targetFile2, true);

                Console.WriteLine("[4/4] Setup Registry.");
                CreateRegistryKey(Program.protocolGeneral, targetFile1);
                CreateRegistryKey(Program.protocolSecure, targetFile1);
            }
            else
            {
                // Console.Error.WriteLine("[ERROR] Please run install with administrator privileges.");
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath,
                    WorkingDirectory = Environment.CurrentDirectory,
                    Verb = "runas",
                    Arguments = "install"
                };

                Process.Start(info);
            }
        }


        private static void CreateRegistryKey(string subKey, string executablePath)
        {
            RegistryKey pgk0 = Registry.ClassesRoot.CreateSubKey(subKey);
            RegistryKey pgk1 = pgk0.CreateSubKey("DefaultIcon");
            RegistryKey pgk2 = pgk0.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");

            pgk0.SetValue("", "Internet Explorer Broker Protocol");
            pgk0.SetValue("URL Protocol", "");
            pgk1.SetValue("", "C:\\Windows\\System32\\url.dll,0", RegistryValueKind.String);
            pgk2.SetValue("", String.Format("\"{0}\" execute-without-console \"%1\"", executablePath), RegistryValueKind.String);
        }


        private static void ProgramRemove()
        {
            if (IsAdministrator())
            {
                String sourceUri = Assembly.GetExecutingAssembly().CodeBase;
                String sourcePath = new Uri(sourceUri).LocalPath;
                String sourceDir = Path.GetDirectoryName(sourcePath);

                String targetDir = Environment.GetEnvironmentVariable("ProgramFiles") + Path.DirectorySeparatorChar + "IEBroker.net";

                String brokerFile = Path.GetFileName(sourcePath);
                String configFile = brokerFile + ".config";

                String sourceFile1 = sourceDir + Path.DirectorySeparatorChar + brokerFile;
                String targetFile1 = targetDir + Path.DirectorySeparatorChar + brokerFile;

                String sourceFile2 = sourceDir + Path.DirectorySeparatorChar + configFile;
                String targetFile2 = targetDir + Path.DirectorySeparatorChar + configFile;

                try
                {
                    Console.Write("[1/4] Remove Registry.");
                    DeleteRegistryKey(Program.protocolGeneral);
                    DeleteRegistryKey(Program.protocolSecure);
                    Console.WriteLine(" - OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(" - NG");
                }

                try
                {
                    Console.Write("[2/4] Delete Config File.");
                    File.Delete(targetFile2);
                    Console.WriteLine(" - OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(" - NG");
                }

                try
                {
                    Console.Write("[3/4] Delete Executable.");
                    File.Delete(targetFile1);
                    Console.WriteLine(" - OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(" - NG");
                }

                try
                {
                    Console.Write("[4/4] Delete Directory.");
                    Directory.Delete(targetDir);
                    Console.WriteLine(" - OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(" - NG");
                }
            }
            else
            {
                // Console.Error.WriteLine("[ERROR] Please run install with administrator privileges.");
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath,
                    WorkingDirectory = Environment.CurrentDirectory,
                    Verb = "runas",
                    Arguments = "remove"
                };

                Process.Start(info);
            }
        }


        private static void DeleteRegistryKey(string subKey)
        {
            Registry.ClassesRoot.DeleteSubKeyTree(subKey, false);
        }


        private static void RunInternetExplorer(string url)
        {
            URLCracker cracker = new URLCracker(url, Program.protocolGeneral, Program.protocolSecure);
            cracker.DoCrack();

            ReportBroekrStatus(cracker);

            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.Arguments = "\"" + cracker.GetProcessedUrl(true) + "\"";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.FileName = Program.executablePath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            process.Start();
        }


        private static void ReportBroekrStatus(URLCracker cracker)
        {
            // Parameter validation
            if (String.IsNullOrEmpty(cracker.ControlParam("SECURE"))) return;
            if (String.IsNullOrEmpty(cracker.ControlParam("HOST"))) return;
            if (String.IsNullOrEmpty(cracker.ControlParam("PORT"))) return;
            if (String.IsNullOrEmpty(Program.brokerStatusReportUrl)) return;

            string protocol = "Y".Equals(cracker.ControlParam("SECURE")) ? "https" : "http";
            string targetUrl = String.Format("{0}://{1}:{2}{3}", protocol, cracker.ControlParam("HOST"), cracker.ControlParam("PORT"), Program.brokerStatusReportUrl);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetUrl);
            request.Method = "GET";
            request.Timeout = 5 * 1000; // Timeout : 5 sec
            request.Headers.Add("Cookie", String.Format("{0}={1};", cracker.ControlParam("SESSKEY"), cracker.ControlParam("SESSID")));

            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    HttpStatusCode status = resp.StatusCode;
                    if (status == HttpStatusCode.OK)
                    {
                        Stream respStream = resp.GetResponseStream();
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            string responseText = sr.ReadToEnd();
                            Console.WriteLine(responseText);
                        }
                    }
                }
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }


        private static void PrintUsage()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("NAME");
            Console.ResetColor();
            Console.WriteLine("\tIEBroker.net46 - Internet Explorer Broker, Dot Net Framework 4.6");
            Console.WriteLine();

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("SYNOPSIS");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\tIEBroker.net46.exe  ");
            Console.ResetColor();
            Console.WriteLine("install\t\t// Install IEBroker.net46.exe");
            Console.Write("\t                    ");
            Console.WriteLine("remove\t\t// Remove IEBroker.net46.exe");
            Console.Write("\t                    ");
            Console.WriteLine("execute  URL\t// Launch URL via Internet expolorer.");
        }


        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }
    }
}
