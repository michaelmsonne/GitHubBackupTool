using System;
using System.Collections.Generic;
using System.Net.Mail;
using Octokit;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    public class Globals
    {
        // Set Global variables for application
        public static string _currentExeFileName; // name of current exe file
        public static string _vData; // version of current exe file
        public static string _companyName; // company name of application from exe file
        public static string _appName; // name of application from exe file
        public static string _copyrightData; // copyright data from exe file
        public static string _name; // name of application from exe file

        public static TimeSpan _elapsedTime; // time for tool to run
        public static string _startTime; // start time for tool
        public static string _endTime; // end time for tool

        // Set Global variables for backup
        public static bool _allBranches; // backup all branches from repo if true - default is false
        public static bool _allRepos; // backup all repos from Github if true - default is false
        public static bool _allReposNotForks; // backup all repos from Github if true - default is false
        public static bool _allReposNotForksAndIsOwner; // backup all repos from Github if true - default is true as most common use case is to backup own repos and not forks from others repos
        public static int _backupType;
        
        public static bool _emailOptionsIsSet; // check if email options is set and have required values
        public static string _backupFolderName; // name of backup folder
        public static int _daysToKeepBackup; // number of days to keep backup in backup folder before deleting it - default is 30 days if not set
        public static int _currentBackupsInBackupFolderCount;
        public static int _errors; // count errors
        public static int _repoCount; // count repos from Github
        public static int _repoBackupPerformedCount; // count repo items from Github
        public static int _backupFileCount; // 
        public static int _backupFolderCount;
        public static bool _noProjectsToBackup; // check if there is any projects to backup or not
        public static bool _isBackupOk; // check if backup is ok or not state
        public static string _repoCountStatusText; // text to display in email report
        public static string _isDaysToKeepNotDefaultStatusText; // text to display in email report if days to keep backup is not default value (30 days)
        public static string _totalBackupsIsDeletedStatusText; // text to display in email report if backups is deleted

        public static int _processedMainBranchCount;
        public static int _processedAllBranchesCount;
        public static int _downloadedMainBranchFileCount;
        public static int _downloadedAllBranchesFileCount;
        public static int _octokitFileCount;
        public static int _downloadedFilesCount;
        
        // Set Global variables for cleanup
        public static int _totalBackupsIsDeleted; // count of total backups deleted
        public static int _oldLogFilesToDeleteCount; // count of old log files deleted
        public static bool _oldLogfilesToDelete; // delete old log files if true - default is false and function is not used

        // Set Global variables for email
        public static string _mailto; // email address to send email to
        public static string _mailfrom; // email address to send email from
        public static string _mailserver; // email server to use for sending email
        public static int _mailport; // email port to use for sending email
        public static MailPriority _emailPriority = MailPriority.Normal; // email priority to use for sending email - default is normal priority
        public static bool _useSimpleMailReportLayout; // use simple mail report layout if true - default is false
        public static string _fileAttachedIneMailReport; // file to attach to email report - default is current log file of the day
        
        // OLD - CLEANUP LATER
        public static int _totalFilesIsDeletedAfterUnZipped;
        public static int _numZip;
        public static int _numJson;
        public static bool _checkForLeftoverFilesAfterCleanup;
        public static bool _deletedFilesAfterUnzip;

        // Hold Git project details from project to backup
        public static List<string> repocountelements = new List<string>(); // list of repos to backup from Github - used for email report
        public static List<string> repoitemscountelements = new List<string>(); // list of repo items to backup from Github - used for email report
        
    }
}
