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
            Console.WriteLine($"Repository Name: '{repo.Name}', Owner: '{repo.Owner.Login}', DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}'\n > (Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}')");
            
            // Log repository details
            Message($"Repository Name: '{repo.Name}', Owner: '{repo.Owner.Login}', DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}', (Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}')", EventType.Information, 1000);
        }
    }
}
