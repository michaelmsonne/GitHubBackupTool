using Autofac;
using GithubBackup.Class;
using Octokit;
using System;
using System.Diagnostics;
using System.Reflection;
using GithubBackup.Commands;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup
{
    class Program
    {
        private static IContainer _container;

        public static void SetupIocContainer()
        {
            var builder = new ContainerBuilder();

            // Registering types within the assembly:
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsSelf();

            // Registering types of 3rd party assemblies
            builder.RegisterType<Credentials>().AsSelf();

            _container = builder.Build();
        }

        static int Main(string[] args)
        {
            int result = -1;

            Console.ForegroundColor = ConsoleColor.White;

            // Check requirements for tool to work
            //Requirements.SystemCheck();

            // Load data from exe file to use in tool
            ApplicationInfo.GetExeInfo();

            // Set Global Logfile properties for log
            DateFormat = "dd-MM-yyyy";
            DateTimeFormat = "dd-MM-yyyy HH:mm:ss";
            WriteOnlyErrorsToEventLog = false;
            WriteToEventLog = false;
            WriteToFile = true;

            ApplicationEndStatus.ApplicationStartMessage();

            Message("Loaded log configuration into the program: " + Globals.AppName , EventType.Information, 1000);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            DateTime startTime = DateTime.Now; // get current time as start time for tool

            // Set start time and end time for tool
            Globals._startTime = startTime.ToString("dd-MM-yyyy HH:mm:ss"); // convert start time to string

            // Cleanup old log files
            CleanupLog.CleanupLogs();

            try
            {
                // Clear console
                Console.Clear();

                // Setup IoC container for dependency injection
                SetupIocContainer();

                // Run tool
                var githubBackupCmdWrapper = _container.Resolve<BackupCommand>();
                result = githubBackupCmdWrapper.Command.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}
