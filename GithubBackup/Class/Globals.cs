using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace GithubBackup.Class
{
    public class Globals
    {
        // Set Global variables for application
        public static string _currentExeFileName;
        public static string _vData;
        public static string _companyName;
        public static string AppName;
        public static string _copyrightData;
        public static string _Name;
        public static TimeSpan _elapsedTime;
        public static string _startTime;
        public static string _endTime;

        // Set Global variables for backup
        public static bool _AllBranches;
        public static bool _AllRepos;
        public static bool _AllReposNotForks;
        public static bool _AllReposNotForksAndIsOwner;
        public static int _BackupType;
        
        public static bool _emailOptionsIsSet;
        public static string _backupFolderName;
        public static int _daysToKeepBackup;
        public static int _currentBackupsInBackupFolderCount;
        public static int _errors;

        // Set Global variables for cleanup
        public static int _totalBackupsIsDeleted;
        public static int _oldLogFilesToDeleteCount;
        public static bool _oldLogfilesToDelete;

        // Set Global variables for email
        public static string _mailto;
        public static string _mailfrom;
        public static string _mailserver;
        public static int _mailport;
        public static MailPriority EmailPriority = MailPriority.Normal;
        public static bool _useSimpleMailReportLayout;
        public static string _fileAttachedIneMailReport;
        
        // OLD - CLEANUP
        public static int _totalFilesIsDeletedAfterUnZipped;
        public static int _numZip;
        public static int _numJson;
        public static bool _checkForLeftoverFilesAfterCleanup;
        public static bool _deletedFilesAfterUnzip;

        // Hold Git project details from project to backup
        public static List<string> repocountelements = new List<string>();
        public static List<string> repoitemscountelements = new List<string>();
        
        public static void ApplicationStartMessage()
        {
            // Log start of program
            //Message($"Welcome to {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName, EventType.Information, 1000);
            Console.WriteLine($"\nWelcome to {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "\n");
        }
        public static void ApplicationEndMessage()
        {
            // Log end of program
            //Message($"End of application - {Globals.AppName}, v." + Globals._vData, EventType.Information, 1000);
            Console.WriteLine($"\nEnd of application - {Globals.AppName}, v. {Globals._vData}\n");
        }
    }
}
