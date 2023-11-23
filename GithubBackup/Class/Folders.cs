using System;
using System.IO;
using System.Linq;

namespace GithubBackup.Class
{
    internal class Folders
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

        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }


        public static int GetSubfolderCountForBranchFolders(string rootFolderPath, int depth)
        {
            // This method counts the number of subfolders in a folder, at a given depth used for branch folders

            if (depth < 1 || !Directory.Exists(rootFolderPath))
            {
                return 0;
            }

            int count = 0;

            try
            {
                if (depth == 1)
                {
                    string[] subfolders = Directory.GetDirectories(rootFolderPath);
                    count += subfolders.Length;
                }
                else
                {
                    string[] subfolders = Directory.GetDirectories(rootFolderPath);
                    foreach (string subfolder in subfolders)
                    {
                        count += GetSubfolderCountForBranchFolders(subfolder, depth - 1);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Access denied: {e.Message}");
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine($"Directory not found: {e.Message}");
            }

            return count;
        }
    }
}
