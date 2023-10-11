using System;
using System.Diagnostics;
using System.Globalization;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class ApplicationEndStatus
    {
        public static void ApplicationEndBackupSuccess()
        {
            Console.ForegroundColor = ConsoleColor.White;

            // Stop timer for runtime of tool
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Stop();
            DateTime endTime = DateTime.Now; // get current time as end time for tool

            // Set start time and end time for tool - convert to string and save to global variables
            Globals._endTime = endTime.ToString("dd-MM-yyyy HH:mm:ss"); // convert end time to string

            // Format and display the TimeSpan value.
            // Parse the start and end times into DateTime objects
            DateTime startTime = DateTime.ParseExact(Globals._startTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            endTime = DateTime.ParseExact(Globals._endTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            // Calculate the elapsed time as a TimeSpan
            TimeSpan elapsedTime = endTime - startTime;
            Globals._elapsedTime = elapsedTime;
                            
            Console.WriteLine("\nBackup Run Time: " + Globals._elapsedTime);
            Console.WriteLine("Backup start Time: " + Globals._startTime);
            Console.WriteLine("Backup end Time: " + Globals._endTime);
            Console.WriteLine("Errors: " + Globals._errors);

            Message("Backup Run Time: " + Globals._elapsedTime, EventType.Information, 1000);
            Message("Backup start Time: " + Globals._startTime, EventType.Information, 1000);
            Message("Backup end Time: " + Globals._endTime, EventType.Information, 1000);
            Message("Errors: " + Globals._errors, EventType.Information, 1000);

            if (Globals._emailOptionsIsSet)
            {
                ReportSender.SendEmail(Globals._mailserver, Globals._mailport, Globals._mailfrom, Globals._mailto, "DONE", Globals.repocountelements, Globals.repoitemscountelements, 0, 0, Globals._backupFolderName, Globals._elapsedTime, Globals._errors, Globals._totalBackupsIsDeleted, Globals._daysToKeepBackup, "TEST", "TEST", "TEST", Globals._useSimpleMailReportLayout, "TEST", Globals._startTime, Globals._endTime);
                //Globals._mailserver, Globals._mailport, Globals._mailfrom, Globals._mailto, "emailStatus", "", 0, 0, 0, 0, 0, 0, Globals._backupFolderName, Globals._elapsedTime, Globals._errors, "", "", Globals._daysToKeepBackup, "", "", "", "", "", "", "", "", "", Globals._useSimpleMailReportLayout, "", "", Globals._startTime, Globals._endTime, 0, 0);
            }
            
            Globals.ApplicationEndMessage();
        }

        public static void ApplicationEndBackupError()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            // Stop timer for runtime of tool
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Stop();
            DateTime endTime = DateTime.Now; // get current time as end time for tool

            // Set start time and end time for tool - convert to string and save to global variables
            Globals._endTime = endTime.ToString("dd-MM-yyyy HH:mm:ss"); // convert end time to string

            // Format and display the TimeSpan value.
            // Parse the start and end times into DateTime objects
            DateTime startTime = DateTime.ParseExact(Globals._startTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            endTime = DateTime.ParseExact(Globals._endTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            // Calculate the elapsed time as a TimeSpan
            TimeSpan elapsedTime = endTime - startTime;
            Globals._elapsedTime = elapsedTime;
                            
            Console.WriteLine("\nBackup Run Time: " + Globals._elapsedTime);
            Console.WriteLine("Backup start Time: " + Globals._startTime);
            Console.WriteLine("Backup end Time: " + Globals._endTime);
            Console.WriteLine("Errors: " + Globals._errors);

            Message("Backup Run Time: " + Globals._elapsedTime, EventType.Information, 1000);
            Message("Backup start Time: " + Globals._startTime, EventType.Information, 1000);
            Message("Backup end Time: " + Globals._endTime, EventType.Information, 1000);
            Message("Errors: " + Globals._errors, EventType.Information, 1000);

            if (Globals._emailOptionsIsSet)
            {
                //ReportSender.SendEmail(Globals._mailserver, Globals._mailport, Globals._mailfrom, Globals._mailto, "emailStatus", "", 0, 0, 0, 0, 0, 0, Globals._backupFolderName, Globals._elapsedTime, Globals._errors, "", "", Globals._daysToKeepBackup, "", "", "", "", "", "", "", "", "", Globals._useSimpleMailReportLayout, "", "", Globals._startTime, Globals._endTime, 0, 0);
            }
            
            Globals.ApplicationEndMessage();
        }
    }
}
