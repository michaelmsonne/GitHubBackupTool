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
            // Print repository details to consoles
            Console.Write($"\nRepository Name: '{repo.Name}', Owner: ");

            // Change console foreground color for the 'Owner' information
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"'{repo.Owner.Login}'");
            Console.ForegroundColor = ConsoleColor.White;

            // Print the remaining details
            Console.Write($", Default branch: '{repo.DefaultBranch}', is fork: '{repo.Fork}'\n");
            Console.WriteLine($"Repository Name: '{repo.Name}', Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}', Visibility: '{repo.Visibility}', Private: '{repo.Private}', Archived: '{repo.Archived}', Created: '{repo.CreatedAt}', Last updated: '{repo.UpdatedAt}'");
            
            // Console.WriteLine($"Repository Name: '{repo.Name}', Owner: '{repo.Owner.Login}', DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}'\nRepository Name: '{repo.Name}', Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}'");

            // Log repository details
            Message($"Repository details: Name: '{repo.Name}', Owner: '{repo.Owner.Login}', Default branch: '{repo.DefaultBranch}', Fork: '{repo.Fork}', (Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}'), Visibility: '{repo.Visibility}', Private: '{repo.Private}', Archived: '{repo.Archived}', Created: '{repo.CreatedAt}', Last updated: '{repo.UpdatedAt}'", EventType.Information, 1000);
        }
    }
}
