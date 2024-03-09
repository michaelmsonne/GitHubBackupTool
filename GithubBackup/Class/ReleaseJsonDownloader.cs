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
        public static void SaveReleaseDataFortheRepository(string owner, Repository repo, GitHubClient client, string destinationPath)
        {
            try
            {
                // Get all releases for the repository
                var releases = client.Repository.Release.GetAll(owner, repo.Name).Result;

                // Check if there are any releases
                if (releases.Any())
                {
                    // Save metadata for the repository
                    destinationPath = Path.Combine(destinationPath, "repository_releases.json");

                    // Serialize releases to JSON and save directly to the file
                    File.WriteAllText(destinationPath, JsonConvert.SerializeObject(releases, Formatting.Indented));

                    // Log a message
                    Console.WriteLine($"Release information saved to: '{destinationPath}' for repository '{owner}/{repo.Name}'");
                    Message($"Release information saved to: '{destinationPath}' for repository '{owner}/{repo.Name}'", EventType.Information, 1000);
                }
                // If no releases are found
                else
                {
                    // Log a message and skip further processing
                    Console.WriteLine($"Skipped - no releases data found for repository '{owner}/{repo.Name}' to be saved to: '{destinationPath}'.");
                    Message($"! Skipped - no releases data found for repository '{owner}/{repo.Name}' to be saved to: '{destinationPath}'\\.", EventType.Information, 1000);
                    return; // Skip further processing if the repository is empty
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or display an error message)
                Console.WriteLine($"Error downloading releases for repository '{owner}/{repo.Name}' - Error: {ex.Message}");
                Message($"Error downloading releases for repository '{owner}/{repo.Name}' - Error: {ex.Message}'", EventType.Error, 1001);

                // Increment the _errors integer
                Globals._errors++; // Increment the _errors integer
            }
        }
    }
}
