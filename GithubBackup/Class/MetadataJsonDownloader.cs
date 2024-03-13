using Newtonsoft.Json;
using Octokit;
using System;
using System.IO;
using System.Linq;
using GithubBackup.Core;
using static GithubBackup.Class.FileLogger;
using System.Text;

namespace GithubBackup.Class
{
    internal class MetadataJsonDownloader
    {
        public static void SaveMetadataForTheRepository(string repoDestinationBackupMetadataFilePath, GitHubClient client, Repository repo)
        {
            try
            {
                // Check if the repository has any branches
                var branchNames = BackupService.GetBranchesForRepository(client, repo); // Replace with your method to get branches

                //Console.WriteLine($"Processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'");
                Message($"Processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'", EventType.Information, 1000);

                // Save metadata for the repository if it has any branches
                if (branchNames.Any())
                {
                    // Save metadata for the repository
                    repoDestinationBackupMetadataFilePath = Path.Combine(repoDestinationBackupMetadataFilePath, "repository_metadata.json");
                    File.WriteAllText(repoDestinationBackupMetadataFilePath, JsonConvert.SerializeObject(repo, Formatting.Indented));

                    // Log the result
                    //Console.WriteLine($"Done processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'");
                    Message($"> Done processing metadata for repository '{repo.FullName}' for backup to: '{repoDestinationBackupMetadataFilePath}'", EventType.Information, 1000);
                }
                // Skip further processing if the repository is empty
                else
                {
                    //Console.WriteLine($"Skipped saving metadata for empty repository '{repo.FullName}' - if there was data to backup, repository metadata has been saved to: '{repoDestinationBackupMetadataFilePath}'");
                    Message($"! Skipped saving metadata for empty repository '{repo.FullName}' - if there was data to backup, repository metadata has been saved to: '{repoDestinationBackupMetadataFilePath}'", EventType.Warning, 1001);
                    return; // Skip further processing if the repository is empty
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or display an error message)
                //Console.WriteLine($"Error saving metadata for repository '{repo.FullName}': {ex.Message}");
                Message($"Error saving metadata for repository '{repo.FullName}': {ex.Message}", EventType.Error, 1001);

                // Increment the _errors integer
                Globals._errors++; // Increment the _errors integer
            }
        }

        public static void SaveReleaseDataForTheRepository(string owner, Repository repo, GitHubClient client, string destinationPath)
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
                    //Console.WriteLine($"Release information saved to: '{destinationPath}' for repository '{owner}/{repo.Name}'");
                    Message($"Release information saved to: '{destinationPath}' for repository '{owner}/{repo.Name}'", EventType.Information, 1000);
                }
                // If no releases are found
                else
                {
                    // Log a message and skip further processing
                    //Console.WriteLine($"Skipped - no releases data found for repository '{owner}/{repo.Name}' to be saved to: '{destinationPath}'.");
                    Message($"! Skipped - no releases data found for repository '{owner}/{repo.Name}' to be saved to: '{destinationPath}\\'.", EventType.Information, 1000);
                    return; // Skip further processing if the repository is empty
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or display an error message)
                //Console.WriteLine($"Error downloading releases for repository '{owner}/{repo.Name}' - Error: {ex.Message}");
                Message($"Error downloading releases for repository '{owner}/{repo.Name}' - Error: {ex.Message}'", EventType.Error, 1001);

                // Increment the _errors integer
                Globals._errors++; // Increment the _errors integer
            }
        }

        public static void SaveIssueDataForTheRepository(string owner, Repository repo, GitHubClient client, string destinationPath)
        {
            // TODO : Error: One or more errors occurred. (Value was either too large or too small for an Int32.)' in the GitHub API
            try
            {
                // Get all issues for the repository
                var issues = client.Issue.GetAllForRepository(owner, repo.Name).Result;

                // Check if there are any issues
                if (issues.Any())
                {
                    // Create a folder to store individual issue files
                    string issuesFolderPath = Path.Combine(destinationPath, "issues");
                    Directory.CreateDirectory(issuesFolderPath);

                    // Save metadata for the repository
                    string repositoryIssuesFilePath = Path.Combine(issuesFolderPath, "repository_issues.json");
                    File.WriteAllText(repositoryIssuesFilePath, JsonConvert.SerializeObject(issues, Formatting.Indented));

                    // Save each issue as a separate file
                    foreach (var issue in issues)
                    {
                        // Generate a unique hash from the issue URL
                        string uniqueHash = GetSha1Hash(issue.Url);

                        string issueFileName = $"issue_{uniqueHash}.json";
                        string issueFilePath = Path.Combine(issuesFolderPath, issueFileName);

                        // Serialize the individual issue to JSON and save directly to the file
                        File.WriteAllText(issueFilePath, JsonConvert.SerializeObject(issue, Formatting.Indented));

                        // Log a message for each issue
                        //Console.WriteLine($"Issue information saved to: '{issueFilePath}' for issue ID '{issue.Number}' in repository '{owner}/{repo.Name}'");
                        Message($"Issue information saved to: '{issueFilePath}' for issue ID '{issue.Number}' in repository '{owner}/{repo.Name}'", EventType.Information, 1000);
                    }

                    // Log a message for the overall issue list
                    //Console.WriteLine($"Issue information saved to: '{repositoryIssuesFilePath}' for repository '{owner}/{repo.Name}'");
                    Message($"Issue information saved to: '{repositoryIssuesFilePath}' for repository '{owner}/{repo.Name}'", EventType.Information, 1000);
                }
                // If no issues are found
                else
                {
                    // Log a message and skip further processing
                    //Console.WriteLine($"Skipped - no issues data found for repository '{owner}/{repo.Name}' to be saved to: '{destinationPath}'.");
                    Message($"! Skipped - no issues data found for repository '{owner}/{repo.Name}' to be saved to: '{destinationPath}'\\.", EventType.Information, 1000);
                    return; // Skip further processing if no issues are found
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or display an error message)
                //Console.WriteLine($"Error downloading issues for repository '{owner}/{repo.Name}' - Error: {ex.Message}");
                Message($"Error downloading issues for repository '{owner}/{repo.Name}' - Error: {ex.Message}'", EventType.Error, 1001);

                // Increment the _errors integer
                Globals._errors++; // Increment the _errors integer
            }
        }

        // Helper method to generate a SHA-1 hash from a string
        private static string GetSha1Hash(string input)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1Managed())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        
        public static void SaveCommitCommentDataForRepository(string owner, Repository repo, GitHubClient client, string destinationPath)
        {
            try
            {
                // Create a folder to store individual commit comment files
                string commitCommentsFolderPath = Path.Combine(destinationPath, "commit_comments");
                Directory.CreateDirectory(commitCommentsFolderPath);

                // Get all pull requests for the repository
                var pullRequests = client.PullRequest.GetAllForRepository(owner, repo.Name).Result;

                foreach (var pullRequest in pullRequests)
                {
                    Message($"Processing commit comments for pull request '{pullRequest.Number}' in repository '{owner}/{repo.Name}'", EventType.Information, 1000);

                    // Get commit comments for the pull request
                    var commitComments = client.PullRequest.ReviewComment.GetAll(owner, repo.Name, pullRequest.Number).Result;

                    if (commitComments.Any())
                    {
                        foreach (var comment in commitComments)
                        {
                            var commentFileName = $"{comment.Id}_commit_comment.json";
                            var commentFilePath = Path.Combine(commitCommentsFolderPath, commentFileName);

                            // Serialize commit comment to JSON and save directly to the file
                            File.WriteAllText(commentFilePath, JsonConvert.SerializeObject(comment, Formatting.Indented));

                            // Log a message for each commit comment
                            Message($"Commit comment information saved to: '{commentFilePath}' for pull request '{pullRequest.Number}' in repository '{owner}/{repo.Name}'", EventType.Information, 1000);
                        }
                    }
                    else
                    {
                        Message($"No commit comments found for pull request '{pullRequest.Number}' in repository '{owner}/{repo.Name}'", EventType.Information, 1000);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or display an error message)
                Message($"Error downloading commit comments for repository '{owner}/{repo.Name}' - Error: {ex.Message}'", EventType.Error, 1001);

                // Increment the _errors integer
                Globals._errors++; // Increment the _errors integer
            }
        }
        
    }
}
