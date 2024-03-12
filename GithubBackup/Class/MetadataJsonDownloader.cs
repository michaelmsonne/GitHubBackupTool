using Newtonsoft.Json;
using Octokit;
using System;
using System.IO;
using System.Linq;
using GithubBackup.Core;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class MetadataJsonDownloader
    {
        public static void SaveMetadataFortheRepository(string repoDestinationBackupMetadataFilePath, GitHubClient client, Repository repo)
        {
            try
            {
                // Check if the repository has any branches
                var branchNames = BackupService.GetBranchesForRepository(client, repo); // Replace with your method to get branches

                Console.WriteLine($"Processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'");
                Message($"Processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'", EventType.Information, 1000);

                // Save metadata for the repository if it has any branches
                if (branchNames.Any())
                {
                    // Save metadata for the repository
                    repoDestinationBackupMetadataFilePath = Path.Combine(repoDestinationBackupMetadataFilePath, "repository_metadata.json");
                    File.WriteAllText(repoDestinationBackupMetadataFilePath, JsonConvert.SerializeObject(repo, Formatting.Indented));

                    // Log the result
                    Console.WriteLine($"Done processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'");
                    Message($"> Done processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'", EventType.Information, 1000);
                }
                // Skip further processing if the repository is empty
                else
                {
                    Console.WriteLine($"Skipped saving metadata for empty repository '{repo.FullName}' - if there was data to backup, repository metadata has been saved to: '{repoDestinationBackupMetadataFilePath}'");
                    Message($"! Skipped saving metadata for empty repository '{repo.FullName}' - if there was data to backup, repository metadata has been saved to: '{repoDestinationBackupMetadataFilePath}'", EventType.Warning, 1001);
                    return; // Skip further processing if the repository is empty
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or display an error message)
                Console.WriteLine($"Error saving metadata for repository '{repo.FullName}': {ex.Message}");
                Message($"Error saving metadata for repository '{repo.FullName}': {ex.Message}", EventType.Error, 1001);

                // Increment the _errors integer
                Globals._errors++; // Increment the _errors integer
            }
        }

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
