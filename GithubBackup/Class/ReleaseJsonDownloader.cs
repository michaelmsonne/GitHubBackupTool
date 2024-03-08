using Newtonsoft.Json;
using Octokit;
using System;
using System.IO;
using System.Linq;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class ReleaseJsonDownloader
    {
        public static void Savereleasedatafortherepository(string owner, Repository repo, GitHubClient client, string destinationPath)
        {
            try
            {
                // Get all releases for the repository
                var releases = client.Repository.Release.GetAll(owner, repo.Name).Result;

                if (releases.Any())
                {
                    // Save metadata for the repository
                    destinationPath = Path.Combine(destinationPath, "repository_releases.json");

                    // Serialize releases to JSON and save directly to the file
                    File.WriteAllText(destinationPath, JsonConvert.SerializeObject(releases, Formatting.Indented));

                    Console.WriteLine($"Releases information saved to: '{destinationPath}' for repository '{owner}/{repo.Name}'");
                    Message($"Releases information saved to: '{destinationPath}' for repository '{owner}/{repo.Name}'", EventType.Information, 1000);
                }
                else
                {
                    Console.WriteLine($"Skipped - no releases found for repository '{owner}/{repo.Name}'.");
                    Message($"! Skipped - no releases found for repository '{owner}/{repo.Name}'.", EventType.Information, 1000);
                    return; // Skip further processing if the repository is empty
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or display an error message)
                Console.WriteLine($"Error downloading releases for repository '{owner}/{repo.Name}' - Error: {ex.Message}");
                Message($"Error downloading releases for repository '{owner}/{repo.Name}' - Error: {ex.Message}'", EventType.Error, 1001);
                Globals._errors++; // Increment the _errors integer
            }
        }
    }
}
