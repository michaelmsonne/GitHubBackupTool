using System;
using System.IO;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class LocalFolderTasks
    {
        // public static bool CheckIfHaveSubfolders(string path)
        // {
        //     if (Directory.GetDirectories(path).Length > 0)
        //     {
        //         return true;
        //     }
        //     else
        //     {
        //         return false;
        //     }
        // }

        public static string GetTotalSize(string folderPath)
        {
            long totalSize = 0;

            try
            {
                // Get all files in the specified folder and its subfolders
                string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

                // Calculate the total size
                foreach (var filePath in files)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Handle the exception as needed
            }

            return FormatBytes(totalSize);
        }

        // Helper function to format bytes into human-readable size
        static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int index = 0;
            double size = bytes;

            while (size >= 1024 && index < suffixes.Length - 1)
            {
                size /= 1024;
                index++;
            }

            return $"{size:n2} {suffixes[index]}";
        }

        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            // Remove read-only attribute from files in the directory
            foreach (string file in Directory.GetFiles(path))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;

                    // Log and console output for changed read-only attribute
                    Message($"Removed read-only attribute from file: {file}", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Removed read-only attribute from file: {file}");
                    Console.ResetColor();
                }
            }

            try
            {
                Directory.Delete(path, true);
                // Log and console output for successful deletion
                //Message($"Deleted directory: {path}", EventType.Information, 1000);
                //Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine($"Deleted directory: {path}");
                //Console.ResetColor();
            }
            catch (IOException ex)
            {
                // Log and console output for IOException
                Message($"IOException caught when trying to delete directory: {path} - error: {ex.Message}", EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"IOException caught when trying to delete directory: {path} - error: {ex.Message}");
                Console.ResetColor();
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Log and console output for UnauthorizedAccessException
                Message($"UnauthorizedAccessException caught when trying to delete directory: {path} - error: {ex.Message}", EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"UnauthorizedAccessException caught when trying to delete directory: {path} - error: {ex.Message}");
                Console.ResetColor();
                throw;
            }
            catch (Exception ex)
            {
                // Log and console output for any other exceptions
                Message($"Exception caught when trying to delete directory: {path} - error: {ex.Message}", EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Exception caught when trying to delete directory: {path} - error: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        public static int GetSubfolderCountForBranchFolders(string rootFolderPath, int depth)
        {
            // This method counts the number of subfolders in a folder, at a given depth used for branch folders

            // Checks if the depth is less than 1 or the root folder does not exist.
            // If so, returns 0 as there are no subfolders to count.
            if (depth < 1 || !Directory.Exists(rootFolderPath))
            {
                return 0;
            }

            int count = 0; // Initialize count to keep track of the number of subfolders.

            try
            {
                if (depth == 1)
                {
                    // If the depth is 1, count the immediate subfolders in the root folder.
                    string[] subfolders = Directory.GetDirectories(rootFolderPath);
                    count += subfolders.Length;
                }
                else
                {
                    // If the depth is greater than 1, iterate through the immediate subfolders of the root folder.
                    // For each subfolder, recursively call the GetSubfolderCountForBranchFolders method
                    // to count the subfolders at the specified depth within each subfolder.
                    string[] subfolders = Directory.GetDirectories(rootFolderPath);
                    foreach (string subfolder in subfolders)
                    {
                        count += GetSubfolderCountForBranchFolders(subfolder, depth - 1);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                // Handles an UnauthorizedAccessException if access is denied to a folder.
                Console.WriteLine($"Access denied: {e.Message}");
                
                // Log
                Message($"Access denied: {e.Message}", EventType.Error, 1001);
            }
            catch (DirectoryNotFoundException e)
            {
                // Handles a DirectoryNotFoundException if a directory is not found.
                Console.WriteLine($"Directory not found: {e.Message}");

                // Log
                Message($"Directory not found: {e.Message}", EventType.Error, 1001);
            }

            return count; // Returns the total count of subfolders at the specified depth within the root folder.
        }
    }
}