using System;
using Octokit;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class RepositoryDetails
    {
        // Separate method to print repository details and log them
        public static void PrintRepositoryDetails(Repository repo)
        {
            // Print repository details to console
            Console.Write($"Repository Name: '{repo.Name}', Owner: ");

            // Change console foreground color for the 'Owner' information
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"'{repo.Owner.Login}'");
            Console.ForegroundColor = ConsoleColor.White;

            // Print the remaining details
            Console.Write($", DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}'\n");
            Console.WriteLine($"Repository Name: '{repo.Name}', Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}'");
            
            // Console.WriteLine($"Repository Name: '{repo.Name}', Owner: '{repo.Owner.Login}', DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}'\nRepository Name: '{repo.Name}', Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}'");

            // Log repository details
            Message($"Repository Name: '{repo.Name}', Owner: '{repo.Owner.Login}', DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}', (Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}')", EventType.Information, 1000);
        }
    }
}
