using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.UserAuthorizationManagement;
using ABB.Robotics.Controllers.FileSystemDomain;
using CommandLine;

namespace ABB_IRC5_Local_Network_Backup
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            //[Option("outputDir", Required = false, HelpText = "Output directory to save backups locally")]
            public bool Verbose { get; set; }

            [Option('o', Required = false, HelpText = "Defines local folder for backups. Example: -o \"C:\\Users\\User\\Documents\\Robostudio Backups\\Auto\\\"")]
            public string OutputDir { get; set; }

            [Option('a', Required = true, HelpText = "Backup all available controllers (including virtual ones).")]
            public bool backupAll { get; set; }

            [Option('l', Required = false, HelpText = "List all available controllers (including virtual ones).")]
            public bool list { get; set; }
        }
        static void Main(string[] args)
        {
            

            //parsing command-line options
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed<Options>(o =>
               {
                   //detecting all controllers
                   ControllerInfoCollection controllersAvailable = findControllers(o.backupAll);
                   string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                   string destinationFolder = System.IO.Path.Combine(userFolder, "Documents\\RobotStudio Backups\\Auto Backups");
                   if (o.OutputDir != null)
                   {
                       //Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                       //Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                       if (System.IO.Directory.Exists(o.OutputDir))
                       {
                           //then create the backup here
                           destinationFolder = o.OutputDir;
                       }
                   }
                   if (o.list)
                   {
                       listAllCOntrollers(findControllers(true));
                   }
                   backupAllControllers(controllersAvailable, destinationFolder);
               });
        }
        

       static ControllerInfoCollection findControllers(bool includingVirtual=false)
        {
            NetworkScanner scanner = null;
            scanner = new NetworkScanner();
            scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;
            if (!includingVirtual)
            {
                //requires refactoring
                for (int i=0;i<controllers.Count;i++)
                {
                    if (controllers[i].IsVirtual)
                        controllers.RemoveAt(i);
                }
            }
            return controllers;
        }

        static void listAllCOntrollers(ControllerInfoCollection list)
        {
            //ListViewItem item = null;
            foreach (ControllerInfo controllerInfo in list)
            {
                Console.WriteLine("Controller found:");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("IP Address: " + controllerInfo.IPAddress.ToString());
                Console.WriteLine("SystemID: " + controllerInfo.SystemId);
                //Console.WriteLine("" + controllerInfo.Availability.ToString());
                Console.WriteLine("IsVirtual: " + controllerInfo.IsVirtual.ToString());
                Console.WriteLine("System Name: "+controllerInfo.SystemName);
                Console.WriteLine("RobotWare Version: "+controllerInfo.Version.ToString());
                Console.WriteLine("Controller Name" + controllerInfo.ControllerName);
                Console.WriteLine("-----------------------------------");
            }
        }

        static bool createBackup(ControllerInfo controllerInfo, string localFolderPath, string backupName)
        {
            
            Controller currentController = Controller.Connect(controllerInfo, ConnectionType.Standalone, true);
            string message = string.Format("Creating backup {1} for controller: {0} with IP: {2}", controllerInfo.ControllerName, backupName, controllerInfo.IPAddress.ToString());
            Console.Write(message);
            Console.WriteLine("...");
            logToFile(localFolderPath, message);
            currentController.Backup("..\\BKP\\" + backupName); //in relation to remoteBackupDir

            while (currentController.BackupInProgress) { }; //can stuck here
            
            Console.WriteLine(string.Format("{0} Backup was saved on the controller!",DateTime.Now.ToString("dd/MM/yyyy HH:mm")));
            logToFile(localFolderPath, string.Format("Backup saved on controller: {0} Path: {1}", controllerInfo.ControllerName, "\\BKP\\"+backupName));
            string localBackupDirPath = System.IO.Path.Combine(localFolderPath,backupName);//"C:\\Users\\User\\Documents\\Robostudio Backups\\Auto\\" + backupName;
            Console.WriteLine("Attempting to copy the backup to: " + localBackupDirPath);
            logToFile(localFolderPath, "Attempting to copy backup from the controller to: "+ localBackupDirPath);
            Console.WriteLine(localBackupDirPath);
            currentController.FileSystem.GetDirectory("..\\BKP\\" + backupName, localBackupDirPath);
            logToFile(localFolderPath,"Compressing backup into: "+ localBackupDirPath + ".zip");
            ZipFile.CreateFromDirectory(localBackupDirPath, localBackupDirPath+".zip");
            logToFile(localFolderPath, "Backup compressed successfuly.");
            System.IO.Directory.Delete(localBackupDirPath,true);
            logToFile(localFolderPath, "Temporary backup folder deleted.");

            return true;//requires refactoring
        }

        static void backupAllControllers(ControllerInfoCollection controllersCollection, string localFolder)
        {
            foreach (ControllerInfo controller in controllersCollection)
            {
                string bkp_name = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMdd_HHmmss"), controller.SystemName);
                //Console.WriteLine(bkp_name);
                createBackup(controller, localFolder, bkp_name);
            }
            
        }

        static void logToFile(string logFolderPath, string message)
        {
            using (StreamWriter w = File.AppendText(Path.Combine(logFolderPath,"backup_log.txt")))
            {
                w.Write(DateTime.Now.ToString("dd/MM/yyyy HH:mm "));
                w.WriteLine(message);
            }
        }
    }
}
