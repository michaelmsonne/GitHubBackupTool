using Autofac;
using GithubBackup.Class;
using GithubBackup.Commands;
using Octokit;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Core
{
    class Program
    {
        private static IContainer _container;

        private static void LogUnhandledException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();

            Message($"Unhandled exception: {ex.Message}", EventType.Error, 1001);

            if (ex is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                {
                    Message($"Inner exception: {innerException.Message}", EventType.Error, 1001);
                }
            }
            else if (ex.InnerException != null)
            {
                Message($"Inner exception: {ex.InnerException.Message}", EventType.Error, 1001);
            }
        }

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
            AppContext.SetSwitch("System.IO.UseLegacyPathHandling", false);

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

            ApplicationStatus.ApplicationStartMessage();

            Message("Loaded log configuration into the program: " + Globals._appName, EventType.Information, 1000);

            Stopwatch stopWatch = new();
            stopWatch.Start();
            DateTime startTime = DateTime.Now; // get current time as start time for tool

            // Set start time and end time for tool
            Globals._startTime = startTime.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture); // convert start time to string

            //GitHubServiceStatus.Check("Incident with API Requests");

            //Console.ReadKey();

            // Cleanup old log files
            //CleanupLog.CleanupLogs();

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
                LogUnhandledException(ex);

                Globals._isBackupOk = false;
                if (string.IsNullOrWhiteSpace(Globals._repoCountStatusText))
                {
                    Globals._repoCountStatusText = "Failed";
                }

                try
                {
                    ApplicationStatus.ApplicationEndBackup(false);
                }
                catch (Exception endStatusException)
                {
                    Message($"Failed to run ApplicationEndBackup(false): {endStatusException.Message}", EventType.Error, 1001);
                }

                result = 1;
            }

            return result;
        }
    }
}
