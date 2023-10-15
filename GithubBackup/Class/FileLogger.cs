﻿using System;
using System.IO;

namespace GithubBackup.Class
{
    internal class FileLogger
    {
        // Control if saves log to logfile
        public static bool WriteToFile { get; set; } = true;

        // Control if saves log to Windows eventlog
        public static bool WriteToEventLog { get; set; } = true;

        public static bool WriteOnlyErrorsToEventLog { get; set; } = true;

        // Sets the App name for the log function
        //public static string AppName { get; set; } = Globals.AppName; // "Unknown",;

        // Set date format short
        public static string DateFormat { get; set; } = "dd-MM-yyyy";

        // Set date format long
        public static string DateTimeFormat { get; set; } = "dd-MM-yyyy HH:mm:ss";

        // Get logfile path
        public static string GetLogPath(string df)
        {
            return Files.LogFilePath + @"\" + Globals.AppName + " Log " + df + ".log";
        }

        // Get datetime
        public static string GetDateTime(DateTime datetime)
        {
            return datetime.ToString(DateTimeFormat);
        }

        // Get date
        public static string GetDate(DateTime datetime)
        {
            return datetime.ToString(DateFormat);
        }

        // Set event type
        public enum EventType
        {
            Warning,
            Error,
            Information,
        }

        // Add message
        public static void Message(string logText, EventType type, int id)
        {
            var now = DateTime.Now;
            var date = GetDate(now);
            var dateTime = GetDateTime(now);
            var logPath = GetLogPath(date);

            lock (LogLock) // Use a lock to synchronize access to the log file
            {
                // Set where to save log message to
                if (WriteToFile)
                    AppendMessageToFile(logText, type, dateTime, logPath, id);
                if (!WriteToEventLog)
                    return;
                AddMessageToEventLog(logText, type, dateTime, logPath, id);
            }
        }

        // Define an object for locking
        private static readonly object LogLock = new object();

        // Save message to logfile
        private static void AppendMessageToFile(string mess, EventType type, string dtf, string path, int id)
        {
            try
            {
                // Check if file exists else create it
                if (!Directory.Exists(Files.LogFilePath))
                {
                    Directory.CreateDirectory(Files.LogFilePath);

                    // Log folder exists - will not create a new folder
                    Message("Output folder to log files created: '" + Files.LogFilePath + "'.", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Output folder to log files created: '" + Files.LogFilePath + "'.");
                    Console.ResetColor();
                }
                // else
                // {
                //     // Log folder exists - will not create a new folder
                //     Message("Output folder to log files exists (will not create it again): '" + Files.LogFilePath + "'.", EventType.Information, 1000);
                //     Console.ForegroundColor = ConsoleColor.Green;
                //     Console.WriteLine("Output folder to log files exists (will not create it again): '" + Files.LogFilePath + "'.");
                //     Console.ResetColor();
                // }
                
                var str = type.ToString().Length > 7 ? "\t" : "\t\t";
                if (!File.Exists(path))
                {
                    using (var text = File.CreateText(path))
                        text.WriteLine(
                            $"{(object)dtf} - [EventID {(object)id.ToString()}] {(object)type.ToString()}{(object)str}{(object)mess}");
                }
                else
                {
                    using (var streamWriter = File.AppendText(path))
                        streamWriter.WriteLine(
                            $"{(object)dtf} - [EventID {(object)id.ToString()}] {(object)type.ToString()}{(object)str}{(object)mess}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Message("Unable to create folder to store the log files: " + Files.LogFilePath + "'. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to create folder to store the log files: ´'" + Files.LogFilePath + "'. Make sure the account you use to run this tool has write rights to this location.");
                Console.ResetColor();

                // Count errors
                Globals._errors++;
            }
            catch (Exception e)
            {
                // Error when create backup folder
                Message("Exception caught when trying to create log file folder - error: " + e, EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception caught when trying to create log file folder - error: " + e);
                Console.ResetColor();

                // Count errors
                Globals._errors++;

                // Handle exception if error when create log file folder
                if (!WriteToEventLog)
                    return;
                AddMessageToEventLog($"Error writing to log file, {e.Message}", EventType.Error, dtf, path, 0);
                AddMessageToEventLog("Writing log file have been disabled.", EventType.Information, dtf, path, 0);
                WriteToFile = false;
            }
        }

        // Save message to Windows event log
        private static void AddMessageToEventLog(string mess, EventType type, string dtf, string path, int id)
        {
            /*try
            {
                if (type != EventType.Error && WriteOnlyErrorsToEventLog)
                    return;
                var eventLog = new EventLog("");
                if (!EventLog.SourceExists(Globals.AppName))
                    EventLog.CreateEventSource(Globals.AppName, "Application");
                eventLog.Source = Globals.AppName;
                eventLog.EnableRaisingEvents = true;
                var type1 = EventLogEntryType.Error;
                switch (type)
                {
                    case EventType.Warning:
                        type1 = EventLogEntryType.Warning;
                        break;
                    case EventType.Error:
                        type1 = EventLogEntryType.Error;
                        break;
                    case EventType.Information:
                        type1 = EventLogEntryType.Information;
                        break;
                }
                eventLog.WriteEntry(mess, type1, id);
            }
            catch (SecurityException ex)
            {
                if (WriteToFile)
                {
                    AppendMessageToFile($"Security Exeption: {ex.Message}", EventType.Error, dtf, path, id);
                    AppendMessageToFile("Run this software as Administrator once to solve the problem.", EventType.Information, dtf, path, id);
                    AppendMessageToFile("Event log entries have been disabled.", EventType.Information, dtf, path, id);
                    WriteToEventLog = false;
                }
            }
            catch (Exception ex)
            {
                if (WriteToFile)
                {
                    AppendMessageToFile(ex.Message, EventType.Error, dtf, path, id);
                    AppendMessageToFile("Event log entries have been disabled.", EventType.Information, dtf, path, id);
                    WriteToEventLog = false;
                }
            }*/
        }
    }
}