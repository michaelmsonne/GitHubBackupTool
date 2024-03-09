using Octokit;
using ShellProgressBar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GithubBackup.Class;
using LibGit2Sharp;
using static GithubBackup.Class.FileLogger;
using Branch = Octokit.Branch;
using Credentials = Octokit.Credentials;
using Repository = Octokit.Repository;

// ReSharper disable AccessToDisposedClosure

namespace GithubBackup.Core
{
    public class BackupService
    {
        public string Destination { get; set; }

        public Credentials Credentials { get; set; }

        private User GetUserData()
        {
            var client = CreateGithubClient();

            User user = null;
            try
            {
                var userTask = client.User.Current();
                userTask.Wait();
                user = userTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return user;
        }

        private GitHubClient CreateGithubClient()
        {
            var client = new GitHubClient(new ProductHeaderValue(Credentials.Login))
            {
                Credentials = Credentials
            };
            return client;
        }

        // Separate method to fetch branches for a repository
        public static IReadOnlyList<Branch> GetBranchesForRepository(GitHubClient client, Repository repo)
        {
            var branchesTask = client.Repository.Branch.GetAll(repo.Owner.Login, repo.Name);
            branchesTask.Wait();

            // Assuming 'repo' is an instance of LibGit2Sharp.Repository
            Globals._alloriginalBranches = branchesTask.Result.Select(branch => branch.Name).ToList();

            // Filter out branches with "dependabot" in the name if the option is set
            if (Globals._excludeBranchDependabot)
            {
                // Filter out branches with "dependabot" in the name
                var filteredBranches = branchesTask.Result.Where(branch => !branch.Name.Contains("dependabot", StringComparison.OrdinalIgnoreCase)).ToList();

                // Return the filtered branches
                return filteredBranches;
            }
            else
            {
                // Return the branches
                return branchesTask.Result;
            }
            
            //var branchesTask = client.Repository.Branch.GetAll(repo.Owner.Login, repo.Name);
            //branchesTask.Wait();
            //return branchesTask.Result;
        }

        private static List<string> GetBranchNamesForRepository(GitHubClient client, string owner, string repoName)
        {
            var branches = client.Repository.Branch.GetAll(owner, repoName).Result;
            return branches.Select(branch => branch.Name).ToList();
        }

        public BackupService(Credentials credentials, string destination)
        {
            // Create backup folder name
            //var backupFolderName = $"Github Backup ({DateTime.Now.ToString("dd-MM-yyyy HH-mm", CultureInfo.InvariantCulture)})\\";
            var backupFolderName = $"{DateTime.Now.ToString("dd-MM-yyyy-(HH-mm)", CultureInfo.InvariantCulture)}\\";

            // Set destination folder
            Destination = Path.Combine(destination, backupFolderName);
            Globals._backupFolderName = Destination;

            // Log backup folder name
            Message("Backup folder is set to: '" + Destination + "'.", EventType.Information, 1000);

            // Set credentials for Github
            Credentials = credentials;
        }

        public void CreateBackup()
        {
            // Get user data from Github
            var user = GetUserData();

            // Set user/org name to global variable
            Globals._name = user.Name;

            // Show user data to console (name)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Hello '{user.Name}' - Account type: '{user.Type}', you have {user.PublicRepos} public repositorie(s) - profile link {user.HtmlUrl}");
            Message($"Hello '{user.Name}' - Account type: '{user.Type}', you have {user.PublicRepos} public repositorie(s) - profile link {user.HtmlUrl}", EventType.Information, 1000);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nShowing all repos before filtering the token gives access to!...");
            Message("Showing all repos before filtering the token gives access to!...", EventType.Information, 1000);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Getting all repos the API key has access to...");
            Message("Getting all repos the API key has access to...", EventType.Information, 1000);

            // Show repository data to console (all)
            Console.ForegroundColor = ConsoleColor.White;
            var repos = GetRepos();

            // Log
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Got all repos the API key has access to - getting what meets the arguments for backup...");
            Message("Got all repos the API key has access to - getting what meets the arguments for backup...", EventType.Information, 1000);

            // Show repository count to console (filtered)
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Total repositories found there meets arguments for backup is: '{repos.Count}'");
            Message($"Total repositories found there meets arguments for backup is: '{repos.Count}'", EventType.Information, 1000);

            // if repos is empty - set state to no projects to backup
            if (repos.Count != 0)
            {
                // Set state to projects to backup
                Globals._noProjectsToBackup = false;

                // Repos to backup - do work

                // TODO
                // Console.WriteLine("Selected backup type is: ");
                // Message("Selected backup type is: ", EventType.Information, 1000);

                Console.WriteLine($"Backup destination folder is set to: '\"{Destination}\"'");
                Message($"Backup destination folder is set to: '\"{Destination}\"'", EventType.Information, 1000);

#if DEBUG
                Console.ReadKey();
#endif

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Starting to clone all repositories into backup folder: '{Destination}'...");
                Message($"Starting to clone all repositories into backup folder: '{Destination}'...", EventType.Information, 1000);

                // Clone all repositories into backup folder specified
                var exceptions = CloneRepos(repos);

                // Count number of branches in backup folder for the current repository (main or all)
                // Log
                Console.WriteLine("Starting counting of branches in backup folder...");
                Message($"Starting counting of branches in backup folder...", EventType.Information, 1000);

                // Do work to count branches in backup folder
                Globals._repoBackupPerformedBranchCount = Folders.GetSubfolderCountForBranchFolders(Globals._backupFolderName, 3);

                // Log
                Console.WriteLine("Counted branches in backup folder.");
                Message("> Done - counted branches in backup folder.", EventType.Information, 1000);

                // Count number of files and folders in backup folder
                LocalBackupsTasks.CountFilesAndFoldersInFolder(Globals._backupFolderName, out var fileCount, out var folderCount);

                // Save count of files and folders in backup folder to global variables
                Globals._backupFileCount = fileCount;
                Globals._backupFolderCount = folderCount;

                // Show errors if any
                if (exceptions.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Backup finished with {exceptions.Count} error(s):");
                    Message($"Backup finished with {exceptions.Count} error(s):", EventType.Information, 1000);

                    // Loop through exceptions and print them to console
                    foreach (var repoName in exceptions.Keys)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Error while cloning repository '{repoName}':");
                        Message($"Error while cloning repository '{repoName}':", EventType.Error, 1001);

                        Console.WriteLine(exceptions[repoName]);
                        Message(exceptions[repoName].ToString(), EventType.Error, 1001);
                    }
                }
                else
                {
                    if (Globals._errors > 0)
                    {
                        //Set backup status
                        Globals._isBackupOk = false;

                        Globals._repoCountStatusText = "Warning!";

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nBackup finished with error(s) - see log for more information");
                        Message($"Backup finished with error(s) - see log for more information", EventType.Error, 1001);

                        // Handle errors
                        ApplicationStatus.ApplicationEndBackup(false);
                    }
                    else
                    {
                        //Set backup status
                        Globals._isBackupOk = true;

                        Globals._repoCountStatusText = "Good";

                        // No errors counted - backup should be finished successfully
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nBackup finished successfully - see log for more information");
                        Message("Backup finished successfully - see log for more information", EventType.Information, 1000);

                        // Handle success
                        ApplicationStatus.ApplicationEndBackup(true);
                    }
                }
            }
            else
            {
                // No repos to backup - set state to no projects to backup
                Globals._noProjectsToBackup = true;

                // Set backup status to email report
                Globals._repoCountStatusText = "Nothing to backup";

                // Nothing to backup - finished successfully
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nNothing to backup - finished successfully - see log for more information");
                Message("Backup finished successfully - see log for more information", EventType.Information, 1000);

                // Handle success
                ApplicationStatus.ApplicationEndBackup(true);
            }
        }

        private IReadOnlyList<Repository> GetRepos()
        {
            var client = CreateGithubClient();
            var task = client.Repository.GetAllForCurrent();
            task.Wait();
            var allRepos = task.Result;

            Globals._repoCount = 0; // Reset the _repoCount integer for count of repos in total

            IReadOnlyList<Repository> filteredRepos;

            // Filter repositories based on the selected options - If none of the conditions are met, all repositories will be returned by default
            if (Globals._allRepos) // Set to all repositories - no filter
            {
                filteredRepos = allRepos;
            }
            else if (Globals._allReposNotForks) // Filter out forked repositories
            {
                // Filter out forked repositories
                filteredRepos = allRepos.Where(repo => !repo.Fork).ToList();
            }
            else if (Globals._allReposNotForksAndIsOwner) // Filter out forked and collaborator repositories
            {
                // Filter out forked and collaborator repositories
                filteredRepos = allRepos
                    .Where(repo => !repo.Fork && repo.Permissions.Admin)
                    .ToList();
            }
            else
            {
                // Use default filter - all repositories where you have access
                Globals._allRepos = true;
                filteredRepos = allRepos;
            }
            
            foreach (var repo in allRepos)
            {
                // Increment the _repoCount integer for count of repos in total
                Globals._repoCount++;

                // Get branch names for the current repository
                var branchNames = GetBranchNamesForRepository(client, repo.Owner.Login, repo.Name);

                // Print main repository details to console
                RepositoryDetails.PrintRepositoryDetails(repo);

                // Print branch start text to console
                Console.WriteLine("Branche(s) for repo:");

                // Print branch names to console
                foreach (var branchName in branchNames)
                {
                    Console.WriteLine($"  Repository Name: '{repo.Name}', Branch: '{branchName}'");
                }

                // List name for projects to list for email report list
                //Globals.repocountelements.Add($"Repository Name: '{repo.Name}', DefaultBranch: '{repo.DefaultBranch}', Owner: '{repo.Owner.Login}'");
                Globals._repocountelements.Add($"{repo.Name}, ('{repo.DefaultBranch}' branch), Owner: '{repo.Owner.Login}'");
            }

            // Return the filtered repositories, or all repositories if none of the conditions are met
            return filteredRepos ?? allRepos;
        }

        private ConcurrentDictionary<string, Exception> CloneRepos(IReadOnlyList<Repository> repos)
        {
            /*
             * TODO
             * ProgressBar shows wrong status if all branches - as count on repo numbers - NOT number of branches
             */
            var exceptions = new ConcurrentDictionary<string, Exception>();

            // Set options for the progressbar
            var rootProgressBarOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Cyan,
                CollapseWhenFinished = false,
                EnableTaskBarProgress = true,
            };

            // Create a progressbar for the overall progress
            var rootProgressBar = new ProgressBar(repos.Count, "Overall Progress", rootProgressBarOptions);

            // Use an object for locking
            var lockObject = new object();

            Parallel.ForEach(repos, (repo) =>
            {
                // Backup all branches for the current repository selected for backup
                var client = CreateGithubClient();
                var task = client.Repository.GetAllForCurrent();
                task.Wait();

                // Get branch names for the current repository
                var branchNames = GetBranchesForRepository(client, repo);
                
                // Set folder names for the current repository
                string repoDestinationBackupCode;                                                // Code folder (root of the repository or branch folder)
                var repoDestinationBackupMetadata = Path.Combine(Destination, repo.FullName); // Metadata folder (root of the repository) + branch/code folder will be added later 
                var repoDestinationBackupReleasedata = Path.Combine(Destination, repo.FullName); // Release folder (root of the repository() + branch/code folder will be added later )
                
                /*
                 *
                 * Specify metadata backup options
                 *
                 */
                bool backupMetadata = Globals._backupRepoMetadata; // Set to true or false based on the option selected for backupMetadata

                // Set folder names for the current repository based on the backupMetadata option
                if (backupMetadata)
                {
                    // If backing up metadata, add "code\" in front of the folder name
                    repoDestinationBackupCode = Path.Combine(Destination, repo.FullName, "code");
                }
                else
                {
                    // If not backing up metadata, use only the repository name as the folder name
                    repoDestinationBackupCode = Path.Combine(Destination, repo.FullName);
                }
                
                /*
                 * Specify release backup options
                 */
                bool backupReleasedata = Globals._backupReleasedata; // Set to true or false based on the option selected for backup releasedata
                
                ChildProgressBar progressBar = null;
                Exception cloneException = null;

                var cloneOptions = new CloneOptions
                {
                    RecurseSubmodules = true,
                    OnTransferProgress = (progress) =>
                    {
                        if (progressBar == null)

                            if (Globals._allBranches)
                            {
                                // If all branches is selected for backup (all branches option) - show branches in progressbar
                                foreach (var branchName in branchNames)
                                {
                                    progressBar = rootProgressBar.Spawn(progress.TotalObjects, repo.Name + ", Branch: '" + branchName.Name + "'", new ProgressBarOptions
                                    {
                                        CollapseWhenFinished = true,
                                        ForegroundColorDone = ConsoleColor.Green,
                                        ForegroundColor = ConsoleColor.Yellow
                                    });
                                }
                            }
                            else
                            {
                                // If only default branch is selected for backup (default option)
                                progressBar = rootProgressBar.Spawn(progress.TotalObjects, repo.Name + ", Branch: 'DefaultBranch'", new ProgressBarOptions
                                {
                                    CollapseWhenFinished = true,
                                    ForegroundColorDone = ConsoleColor.Green,
                                    ForegroundColor = ConsoleColor.Yellow
                                });
                            }

                        if (progressBar != null) progressBar.Tick(progress.ReceivedObjects);
                        return true;
                    },
                    RepositoryOperationCompleted = (context) =>
                    {
                        // Dispose the progressbar
                        progressBar?.Dispose();
                    }
                };

                /*
                 * Set credentials for Github - basic or oauth
                 */
                if (Credentials.AuthenticationType == AuthenticationType.Basic)
                {
                    // Set credentials for Github - basic
                    cloneOptions.CredentialsProvider = (url, user, cred)
                        => new UsernamePasswordCredentials { Username = Credentials.Login, Password = Credentials.Password };
                }
                else if (Credentials.AuthenticationType == AuthenticationType.Oauth)
                {
                    // Set credentials for Github - oauth
                    cloneOptions.CredentialsProvider = (url, user, cred)
                        => new UsernamePasswordCredentials { Username = Credentials.GetToken(), Password = string.Empty };
                }

                /*
                 * Clone the repository based on the selected options for branches
                 */
                try
                {
                    // If all branches is selected for backup (all branches option) - show branches in progressbar
                    if (Globals._allBranches)
                    {
                        // Backup all branches for the current repository selected for backup
                        foreach (var branchName in branchNames)
                        {
                            // Replacing "\" with "-" in the branch name
                            string sanitizedBranchName = branchName.Name.Replace("/", "-");

                            // Create a folder path for the branch
                            string clonedRepoPath = Path.Combine(repoDestinationBackupCode, sanitizedBranchName);

                            // Log
                            Message($"Processing repository '{repo.FullName}' for backup - Options: ALL branches: saved data for branch '{branchName.Name}' to disk: '{clonedRepoPath}\\'", EventType.Information, 1000);

                            // Clone the repository without checking out any branch
                            LibGit2Sharp.Repository.Clone(repo.CloneUrl, clonedRepoPath, cloneOptions);

                            using (var repository = new LibGit2Sharp.Repository(clonedRepoPath))
                            {
                                // Fetch the specific branch
                                var fetchOptions = new FetchOptions
                                {
                                    CredentialsProvider = cloneOptions.CredentialsProvider
                                };

                                // Fetch the specific branch
                                LibGit2Sharp.Commands.Fetch(repository, "origin", new[] { $"refs/heads/{branchName.Name}:refs/remotes/origin/{branchName.Name}" }, fetchOptions, null);

                                // Checkout the specific branch in the repository
                                var branch = repository.Branches[$"origin/{branchName.Name}"];
                                if (branch != null)
                                {
                                    // Checkout the specific branch
                                    LibGit2Sharp.Commands.Checkout(repository, branch);

                                    // Log
                                    Message($"> Done processing repository '{repo.FullName}' for backup - Options: ALL branches: saved data for branch '{branchName.Name}' to disk: '{clonedRepoPath}\\'", EventType.Information, 1000);
                                }
                                else
                                {
                                    // Handle the case where the branch doesn't exist
                                    Console.WriteLine($"Branch '{branchName.Name}' does not exist in repository '{repo.FullName}'.");
                                    Message($"Branch '{branchName.Name}' does not exist in repository '{repo.FullName}'.", EventType.Warning, 1001);
                                }
                            }

                            // Log
                            //Message($"Done processing repository '{repo.FullName}' for backup - Options: ALL branches: saved data  to disk.", EventType.Information, 1000);

                            // Used for email report list - list name for projects to list for email report list
                            Globals._repoitemscountelements.Add($"{repo.Name}, ('{branchName.Name}' branch), Owner: '{repo.Owner.Login}'");

                            //Globals._repoBackupPerformedCount++; // Increment the _repoCount integer for count of repos in total
                            //Globals._repoBackupPerformedBranchCount++; // Increment the BranchCount

                            // Count repos processed
                            lock (lockObject)
                            {
                                //Globals._repoBackupedCount++; // Increment the _repoCount integer for count of repos in total
                                //Globals._repoBackupPerformedCount++;
                                //Globals._repoBackupPerformedBranchCount++; // Confirm if this count should be increased here
                            }
                        }
                    }
                    // If only default branch is selected for backup (default option)
                    else
                    {
                        // Create a folder path for the branch
                        var clonedRepoPath = Path.Combine(repoDestinationBackupCode, repo.DefaultBranch);

                        // Backup only the default branch for the current repository selected for backup (default branch) - this is the default option
                        LibGit2Sharp.Repository.Clone(repo.CloneUrl, clonedRepoPath, cloneOptions);

                        // Log
                        Message($"Processed repository: '{repo.FullName}' for backup, DefaultBranch '{repo.DefaultBranch}' - saved data to disk: '" + clonedRepoPath + "'", EventType.Information, 1000);

                        // Used for email report list - list name for projects to list for email report list
                        Globals._repoitemscountelements.Add($"{repo.Name}, ('{repo.DefaultBranch}' Default Branch), Owner: '{repo.Owner.Login}'");

                        //Globals._repoBackupPerformedCount++; // Increment the _repoCount integer for count of repos in total
                        //Globals._repoBackupPerformedBranchCount++; // Increment the BranchCount

                        // Count repos processed
                        lock (lockObject)
                        {
                            //Globals._repoBackupedCount++; // Increment the _repoCount integer for count of repos in total
                            //Globals._repoBackupPerformedCount++;
                            //Globals._repoBackupPerformedBranchCount++; // Confirm if this count should be increased here
                        }
                    }

                    // Increment the _repoCount integer for count of repos in total
                    Globals._repoBackupPerformedCount++;
                }
                catch (LibGit2SharpException libGit2SharpException)
                {
                    // Log the exception
                    if (libGit2SharpException.Message == "this remote has never connected")
                    {
                        // Log the exception
                        Console.WriteLine("An error occurred; GitHub may be down or you have no internet!");
                        Message($"An error occurred; GitHub may be down or you have no internet!", EventType.Error, 1001);
                    }
                    else
                    {
                        // Log the exception
                        Console.WriteLine("An error occurred whilst trying to retrieve data from github when processing repository: " + repo.FullName + " when save data to the disk - Error: " + libGit2SharpException.Message);
                        Message($"An error occurred whilst trying to retrieve data from github when processing repository: " + repo.FullName + " when save data to the disk - Error: " + libGit2SharpException.Message, EventType.Error, 1001);
                    }
                }
                finally
                {
                    rootProgressBar.Tick();

                    if (cloneException != null)
                    {
                        lock (lockObject)
                        {
                            Globals._errors++; // Increment the _errors integer
                        }
                    }
                }

                /*
                 *
                 * Set options to save different data for the repository
                 *
                 * This is etc. metadata for the repository
                 *
                 */

                // Save metadata for the repository if the option is set
                if (backupMetadata)
                {
                    // Call a method to download metadata and save them to a JSON file
                    MetadataJsonDownloader.SaveMetadataFortheRepository(repoDestinationBackupMetadata, client, repo);
                }

                // Save metadata for the repository if the option is set
                if (backupReleasedata)
                {
                    // Call a method to download releases and save them to a JSON file
                    ReleaseJsonDownloader.SaveReleaseDataFortheRepository(repo.Owner.Login, repo, client, repoDestinationBackupReleasedata);
                }

                // More to come here


            });

            // Dispose the root progressbar
            rootProgressBar.Dispose();

            return exceptions; // Add this return statement at the end
        }
    }
}