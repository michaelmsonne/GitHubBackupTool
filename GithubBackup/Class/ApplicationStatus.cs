using System;
using System.Diagnostics;
using System.Globalization;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class ApplicationStatus
    {
        public static void ApplicationEndBackup(bool isSuccess)
        {
            if (isSuccess)
            {
                SetConsoleColorDefaultAndError(true);
            }
            else
            {
                SetConsoleColorDefaultAndError(false);
            }

            // Stop timer for runtime of tool
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Stop();
            DateTime endTime = DateTime.Now; // get the current time as the end time for the tool

            // Set start time and end time for the tool - convert to a string and save to global variables
            Globals._endTime = endTime.ToString("dd-MM-yyyy HH:mm:ss"); // convert end time to a string

            // Format and display the TimeSpan value.
            // Parse the start and end times into DateTime objects
            DateTime startTime = DateTime.ParseExact(Globals._startTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            endTime = DateTime.ParseExact(Globals._endTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            // Calculate the elapsed time as a TimeSpan
            TimeSpan elapsedTime = endTime - startTime;
            Globals._elapsedTime = elapsedTime;

            // Display the TimeSpan value to the console
            Console.WriteLine("\nBackup Run Time: " + Globals._elapsedTime);
            Console.WriteLine("Backup start Time: " + Globals._startTime);
            Console.WriteLine("Backup end Time: " + Globals._endTime);
            Console.WriteLine("Errors: " + Globals._errors);

            // Log the TimeSpan value to the log file
            Message("Backup Run Time: " + Globals._elapsedTime, EventType.Information, 1000);
            Message("Backup start Time: " + Globals._startTime, EventType.Information, 1000);
            Message("Backup end Time: " + Globals._endTime, EventType.Information, 1000);
            Message("Errors: " + Globals._errors, EventType.Information, 1000);

            Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;

            // Log total number of files in the backup folder
            if (isSuccess)
            {
                Message($"Total number of files in backup folder '{Globals._backupFolderName}' and its subfolders is: '{Globals._backupFileCount}'", EventType.Information, 1000);

                Console.WriteLine($"Total number of files in backup folder '{Globals._backupFolderName}' and its subfolders is: '{Globals._backupFileCount}'");
            }
            else
            {
                Message($"Total number of files in '{Globals._backupFolderName}' and its subfolders: '{Globals._backupFileCount}' (but not complete as there was some error(s) when backup - check log for more information)", EventType.Information, 1000);

                Console.WriteLine($"Total number of files in '{Globals._backupFolderName}' and its subfolders: '{Globals._backupFileCount}' (but not complete as there was some error(s) when backup - check log for more information)");
            }

            // Send email report if email options are set
            if (Globals._emailOptionsIsSet)
            {
                var emailStatus = isSuccess ? "DONE" : "ERROR";

                // Call method to send email report with the status of backup
                ReportSender.SendEmail(
                    Globals._mailserver,
                    Globals._mailport,
                    Globals._mailfrom,
                    Globals._mailto,
                    emailStatus,
                    Globals.repocountelements,
                    Globals.repoitemscountelements,
                    Globals._repoCount,
                    0,
                    Globals._backupFolderName,
                    Globals._elapsedTime,
                    Globals._errors,
                    Globals._totalBackupsIsDeleted,
                    Globals._daysToKeepBackup,
                    Globals._repoCountStatusText, //isSuccess ? "TEST" : Globals._repoCountStatusText,
                    isSuccess ? Globals._repoCountStatusText : "TEST",
                    isSuccess ? "TEST" : Globals._isDaysToKeepNotDefaultStatusText,
                    Globals._useSimpleMailReportLayout,
                    Globals._isDaysToKeepNotDefaultStatusText,
                    Globals._startTime,
                    Globals._endTime);
            }
            else
            {
                Message("Not set to send an email report - skipping this step for now", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Not set to send an email report - skipping this step for now");
                Console.ResetColor();
            }

            // Call method to end the program
            ApplicationEndMessage();
        }
        
        // todo merge ApplicationEndBackupSuccess and ApplicationEndBackupError into one method

        // merge ApplicationEndBackupSuccess and ApplicationEndBackupError into one method
        
        /*
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
            
            // Display the TimeSpan value to the console
            Console.WriteLine("\nBackup Run Time: " + Globals._elapsedTime);
            Console.WriteLine("Backup start Time: " + Globals._startTime);
            Console.WriteLine("Backup end Time: " + Globals._endTime);
            Console.WriteLine("Errors: " + Globals._errors);

            // Log the TimeSpan value to the log file
            Message("Backup Run Time: " + Globals._elapsedTime, EventType.Information, 1000);
            Message("Backup start Time: " + Globals._startTime, EventType.Information, 1000);
            Message("Backup end Time: " + Globals._endTime, EventType.Information, 1000);
            Message("Errors: " + Globals._errors, EventType.Information, 1000);

            // Log total number of files in backup folder
            Message($"Total number of files in backup folder '{Globals._backupFolderName}' and its subfolders is: '{Globals._backupFileCount}'", EventType.Information, 1000);
            Message($"Total number of folders in backup folder '{Globals._backupFolderName}' is: '{Globals._backupFolderCount}'", EventType.Information, 1000);

            // Send email report if email options is set
            if (Globals._emailOptionsIsSet)
            {
                // Call method to send email report with status of backup
                ReportSender.SendEmail(Globals._mailserver,
                    Globals._mailport,
                    Globals._mailfrom,
                    Globals._mailto,
                    "DONE",
                    Globals.repocountelements,
                    Globals.repoitemscountelements,
                    Globals._repoCount,
                    0,
                    Globals._backupFolderName,
                    Globals._elapsedTime,
                    Globals._errors,
                    Globals._totalBackupsIsDeleted,
                    Globals._daysToKeepBackup,
                    "TEST",
                    Globals._repoCountStatusText,
                    "TEST",
                    Globals._useSimpleMailReportLayout,
                    Globals._isDaysToKeepNotDefaultStatusText,
                    Globals._startTime,
                    Globals._endTime);
            }
            else
            {
                Message("Not set to send email report - skipping", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Not set to send email report - skipping");
                Console.ResetColor();
            }
            
            // Call method to end program
            ApplicationEndMessage();
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
            
            // Display the TimeSpan value to the console
            Console.WriteLine("\nBackup Run Time: " + Globals._elapsedTime);
            Console.WriteLine("Backup start Time: " + Globals._startTime);
            Console.WriteLine("Backup end Time: " + Globals._endTime);
            Console.WriteLine("Errors: " + Globals._errors);

            // Log the TimeSpan value to the log file
            Message("Backup Run Time: " + Globals._elapsedTime, EventType.Information, 1000);
            Message("Backup start Time: " + Globals._startTime, EventType.Information, 1000);
            Message("Backup end Time: " + Globals._endTime, EventType.Information, 1000);
            Message("Errors: " + Globals._errors, EventType.Information, 1000);

            // Log total number of files in backup folder
            Message($"Total number of files in '{Globals._backupFolderName}' and its subfolders: '{Globals._backupFileCount}' (but not complete as there was some error(s) when backup - check log for mere information)", EventType.Information, 1000);

            // Send email report if email options is set
            if (Globals._emailOptionsIsSet)
            {
                // Call method to send email report with status of backup
                ReportSender.SendEmail(Globals._mailserver,
                    Globals._mailport,
                    Globals._mailfrom,
                    Globals._mailto,
                    "ERROR",
                    Globals.repocountelements,
                    Globals.repoitemscountelements,
                    Globals._repoCount,
                    0,
                    Globals._backupFolderName,
                    Globals._elapsedTime,
                    Globals._errors,
                    Globals._totalBackupsIsDeleted,
                    Globals._daysToKeepBackup,
                    Globals._repoCountStatusText,
                    "TEST",
                    "TEST",
                    Globals._useSimpleMailReportLayout,
                    Globals._isDaysToKeepNotDefaultStatusText,
                    Globals._startTime,
                    Globals._endTime);
            }
            else
            {
                Message("Not set to send email report - skipping", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Not set to send email report - skipping");
                Console.ResetColor();
            }
            
            // Call method to end program
            ApplicationEndStatus.ApplicationEndMessage();
        }*/

        public static void ApplicationStartMessage()
        {
            // Log start of program
            Console.WriteLine($"\nWelcome to {Globals._appName}, v." + Globals._vData + " by " + Globals._companyName + "\n");
            Message($"Welcome to {Globals._appName}, v." + Globals._vData + " by " + Globals._companyName, EventType.Information, 1000);
        }
        public static void ApplicationEndMessage()
        {
            // Log end of program
            Console.WriteLine($"\nEnd of application - {Globals._appName} v. {Globals._vData}\n");
            Message($"End of application - {Globals._appName} v. {Globals._vData}", EventType.Information, 1000);
        }

        private static void SetConsoleColorDefaultAndError(bool isSuccess)
        {
            Console.ForegroundColor = isSuccess ? ConsoleColor.White : ConsoleColor.Red;
        }

        private static void SetConsoleColorGreenAndRed(bool isSuccess)
        {
            Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
        }
    }
}
