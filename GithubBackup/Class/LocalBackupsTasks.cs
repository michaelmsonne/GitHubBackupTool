using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class LocalBackupsTasks
    {
        public static void DaysToKeepBackups(string outBackupDir, int daysToKeep)
        {
            // If other then 30 days of backup
            bool backupsToDelete = false;
            int days = daysToKeep;

            // Log
            Message($"Set to keep {daysToKeep} number of backups (day(s)) in backup folder: '{outBackupDir}'", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Set to keep {daysToKeep} number of backups (day(s)) in backup folder: '{outBackupDir}'");
            Console.ResetColor();

            // Log
            Message("Checking for backups needed to be deleted form backup folder: '" + outBackupDir + "'...", EventType.Information, 1000);

            // Loop folders
            foreach (string dir in Directory.GetDirectories(outBackupDir))
            {
                DateTime createdTime = new DirectoryInfo(dir).CreationTime;

                // Find folders from days to keep
                if (createdTime < DateTime.Now.AddDays(-days))
                {
                    try
                    {
                        // Do work
                        LocalFolderTasks.DeleteDirectory(dir);

                        // Count files
                        Globals._totalBackupsIsDeleted++;

                        // Log
                        Message("Deleted old backup folder: '" + dir + "'.", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Deleted old backup folder: '" + dir + "'.");
                        Console.ResetColor();

                        // Set state
                        backupsToDelete = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Message("Unable to delete old backup folder: '" + dir + "'. Make sure the account you use to run this tool has delete rights to this location.", EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unable to delete old backup folder: '" + dir + "'. Make sure the account you use to run this tool has delete rights to this location.");
                        Console.ResetColor();

                        // Count errors
                        Globals._warnings++;
                    }
                    catch (Exception e)
                    {
                        // Error if cant delete file(s)
                        Message("Exception caught when trying to delete old backup folder: '" + dir + "' - error: " + e, EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exception caught when trying to delete old backup folder: '" + dir + "' - error: " + e);
                        Console.ResetColor();

                        // Add error to counter
                        Globals._errors++;
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            switch (backupsToDelete)
            {
                // If no backups to delete
                case false:
                    // Log
                    Message("> Done - No old backups needed to be deleted form backup folder: '" + outBackupDir + "'", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("No old backups needed to be deleted form backup folder: '" + outBackupDir + "'\n");
                    Console.ResetColor();
                    break;
                case true:
                    // Log
                    Message("> Done - Old backups deleted from backup folder: '" + outBackupDir + "'", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Old backups deleted from backup folder: '" + outBackupDir + "'\n");
                    Console.ResetColor();
                    break;
            }
        }

        public static void DaysToKeepBackupsDefault(string outBackupDir)
        {
            // If default 30 days of backup
            bool backupsToDelete = false;

            // Loop in folder
            foreach (string dir in Directory.GetDirectories(outBackupDir))
            {
                DateTime createdTime = new DirectoryInfo(dir).CreationTime;

                // Find folders from days to keep
                if (createdTime < DateTime.Now.AddDays(-30))
                {
                    try
                    {
                        // Do work
                        LocalFolderTasks.DeleteDirectory(dir);

                        // Count files
                        Globals._totalBackupsIsDeleted++;

                        // Log
                        Message("Deleted old backup folder: " + dir, EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Deleted old backup folder: " + dir);
                        Console.ResetColor();

                        // Set state
                        backupsToDelete = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Message("Unable to delete old backup folder: '" + dir + "'. Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unable to delete old backup folder: '" + dir + "'. Make sure the account you use to run this tool has delete rights to this location.");
                        Console.ResetColor();

                        // Count errors
                        Globals._errors++;
                    }
                    catch (Exception e)
                    {
                        // Error if cant delete file(s)
                        Message("Exception caught when trying to delete old backup folder: '" + dir + "' - error: " + e, EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exception caught when trying to delete old backup folder: '" + dir + "' - error: " + e);
                        Console.ResetColor();

                        // Add error to counter
                        Globals._errors++;
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            // If no backups to delete
            if (backupsToDelete == false)
            {
                // Log
                Message("No old backups (default 30 days) needed to be deleted from backup folder: '" + outBackupDir + "'", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No old backups (default 30 days) needed to be deleted from backup folder: '" + outBackupDir + "'\n");
                Console.ResetColor();
            }
        }
        
        public static void CountCurrentNumbersOfBackup(string outBackupDir)
        {
            //Count backups in folder
            string searchPattern = "*??-??-????-(??-??)";
            int folderCount = 0;
            foreach (var directory in Directory.GetDirectories(outBackupDir, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (Directory.GetFileSystemEntries(directory).Length > 0)
                {
                    folderCount++;
                }
            }

            // Save count
            Globals._currentBackupsInBackupFolderCount = folderCount;
        }

        internal static void CountFilesAndFoldersInFolder(string folderPath, out int fileCount, out int folderCount)
        {
            fileCount = 0;
            folderCount = 0;

            try
            {
                // Count files and folders in the current folder
                fileCount += Directory.GetFiles(folderPath).Length;
                folderCount += Directory.GetDirectories(folderPath).Length;

                // Recursively count files and folders in subfolders
                foreach (var subfolder in Directory.GetDirectories(folderPath))
                {
                    int subfolderFileCount, subfolderFolderCount;
                    CountFilesAndFoldersInFolder(subfolder, out subfolderFileCount, out subfolderFolderCount);
                    fileCount += subfolderFileCount;
                    folderCount += subfolderFolderCount;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle unauthorized access to folders if needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public string ValidateRepo(string repoPath)
        {
            try
            {
                using (var repo = new Repository(repoPath))
                {
                    // Get the repository's commit count
                    int commitCount = repo.Commits.Count();

                    // Verify if the repository is empty (no commits)
                    if (commitCount == 0)
                    {
                        return Globals._logMessageStringBackupValidationWarningEmptyRepoDownloaded;
                    }

                    // Add additional checks as needed

                    // If all checks passed, consider the repository valid
                    return null;
                }
            }
            catch (RepositoryNotFoundException)
            {
                return "Error: The repository does not exist.";
            }
            catch (Exception ex)
            {
                return $"Error: An unexpected error occurred: {ex.Message}";
            }
        }

        public void DoDownloadValidationCheck(string repoPath, string repoFullName)
        {
            // Check if validation is needed
            if (Globals._backupRepoValidation)
            {
                // Validate the repository
                string errorMessage = ValidateRepo(repoPath);
                if (errorMessage != null)
                {
                    // Check if the error message contains the specific indication of possibly empty repository on GitHub
                    if (errorMessage.Contains(Globals._logMessageStringBackupValidationWarningEmptyRepoDownloaded))
                    {
                        // Print success message for possibly empty repository on GitHub
                        Message(Globals._logMessageStringBackupValidationWarningEmptyRepoDownloaded + $" Repository: '{repoFullName}' - Backup path: '{repoPath}'", EventType.Warning, 1002);

                        // Increment the warning counter
                        Globals._warnings++;

                        // Increment the counter for total empty repositories
                        Globals._backupRepoValidationTotalEmptyRepositories++;
                    }
                    else
                    {
                        // Print error message for other validation errors
                        Message($"The downloaded repository is not valid: '{repoFullName}' when saving data to the disk ('{repoPath}'): {errorMessage}", EventType.Error, 1001);

                        // Increment the error counter
                        Globals._errors++;
                    }
                }
                else
                {
                    // Print success message for valid repository
                    Message($"The downloaded repository is valid: '{repoFullName}' when saved data to the disk ('{repoPath}')", EventType.Information, 1000);
                }
            }
        }
    }
}
