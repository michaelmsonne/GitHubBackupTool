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

namespace GithubBackup
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
            var client = new GitHubClient(new ProductHeaderValue(Credentials.Login));
            client.Credentials = Credentials;
            return client;
        }

        // Separate method to fetch branches for a repository
        private static IReadOnlyList<Branch> GetBranchesForRepository(GitHubClient client, Repository repo)
        {
            var branchesTask = client.Repository.Branch.GetAll(repo.Owner.Login, repo.Name);
            branchesTask.Wait();
            return branchesTask.Result;
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
            var backupFolderName = $"Github Backup {DateTime.Now.ToString("dd-MM-yyyy-(HH-mm)", CultureInfo.InvariantCulture)}\\";
            
            // Set destination folder
            Destination = Path.Combine(destination, backupFolderName);
            Globals._backupFolderName = Destination;

            // Log backup folder name
            Message("Backup folder is set to: '" + Destination + "'.", EventType.Information, 1000);
            
            // Set credentials for Github
            Credentials = credentials;
        }

        // Separate method to print repository details and log them
        // private static void PrintRepositoryDetails(Repository repo)
        // {
        //     // Print repository details to console
        //     Console.WriteLine($"Repository Name: '{repo.Name}', Owner: '{repo.Owner.Login}', DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}'\n  (Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}')");
        //     
        //     // Log repository details
        //     Message($"Repository Name: '{repo.Name}', Owner: '{repo.Owner.Login}', DefaultBranch: '{repo.DefaultBranch}', Fork: '{repo.Fork}', (Permissions: Admin: '{repo.Permissions.Admin}', Pull: '{repo.Permissions.Pull}', Push: '{repo.Permissions.Push}')", EventType.Information, 1000);
        // }

        public void CreateBackup()
        {
            // Get user data from Github
            var user = GetUserData();

            Globals._Name = user.Name;

            // Show user data to console (name)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Hello {user.Name} - showing all repos before filtering the token gives access to!");
            Message($"Hello {user.Name} - showing all repos before filtering the token gives access to!", EventType.Information, 1000);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Getting all repos the API key has access to...");
            Message("Getting all repos the API key has access to...", EventType.Information, 1000);

            // Show repository data to console (all)
            Console.ForegroundColor = ConsoleColor.White;
            var repos = GetRepos();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Got all repos the API key has access to - getting what meets the arguments for backup...");
            Message("Got all repos the API key has access to - getting what meets the arguments for backup...", EventType.Information, 1000);
            
            // Show repository count to console (filtered)
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Total repositories found there meets arguments for backup is: {repos.Count}");
            Message($"Total repositories found there meets arguments for backup is: {repos.Count}", EventType.Information, 1000);

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

            // Count number of files and folders in backup folder
            Backups.CountFilesAndFoldersInFolder(Globals._backupFolderName, out var fileCount, out var folderCount);

            // Save count of files and folders in backup folder to global variables
            Globals._backupFileCount = fileCount;
            Globals._backupFolderCount = folderCount;

            // Show errors if any
            if (exceptions.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Backup finished with {exceptions.Count} error(s):");
                Message($"Backup finished with {exceptions.Count} error(s):", EventType.Information, 1000);

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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nBackup finished with error(s) - see log for more information");
                    Message($"Backup finished with error(s) - see log for more information", EventType.Error, 1001);

                    // Handle errors
                    Console.WriteLine("Errors: " + Globals._errors);
                    Message("Errors: " + Globals._errors, EventType.Error, 1001);

                    // Handle errors
                    ApplicationEndStatus.ApplicationEndBackupError();
                }
                else
                {
                    // No errors counted - backup should be finished successfully
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nBackup finished successfully - see log for more information");
                    Message("Backup finished successfully - see log for more information", EventType.Information, 1000);

                    // Handle success
                    ApplicationEndStatus.ApplicationEndBackupSuccess();
                }
            }
        }

        private IReadOnlyList<Repository> GetRepos()
        {
            var client = CreateGithubClient();
            var task = client.Repository.GetAllForCurrent();
            task.Wait();
            var allRepos = task.Result;

            Globals._repoCount = 0; // Reset the _repoCount integer for count of repos in total

            IReadOnlyList<Repository> filteredRepos = null;

            if (Globals._AllRepos)
            {
                filteredRepos = allRepos;
            }
            else if (Globals._AllReposNotForks)
            {
                // Filter out forked repositories
                filteredRepos = allRepos.Where(repo => !repo.Fork).ToList();
            }
            else if (Globals._AllReposNotForksAndIsOwner)
            {
                // Filter out forked and collaborator repositories
                filteredRepos = allRepos
                    .Where(repo => !repo.Fork && repo.Permissions.Admin)
                    .ToList();
            }
            else
            {
                // Use default filter - all repositories where you have access
                Globals._AllRepos = true;
                filteredRepos = allRepos;
            }

            foreach (var repo in allRepos)
            {
                
                Globals._repoCount++; // Increment the _repoCount integer for count of repos in total

                // Get branch names for the current repository
                var branchNames = GetBranchNamesForRepository(client, repo.Owner.Login, repo.Name);

                // Print branch names
                foreach (var branchName in branchNames)
                {
                    Console.WriteLine($"Repository Name: '{repo.Name}', Branch: '{branchName}'");
                }

                // Print repository details to console
                RepositoryDetails.PrintRepositoryDetails(repo);

                // List name for projects to list for email report list
                Globals.repocountelements.Add($"Repository Name: '{repo.Name}', DefaultBranch: '{repo.DefaultBranch}', Owner: '{repo.Owner.Login}'");
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

            var rootProgressBarOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Cyan,
                CollapseWhenFinished = false,
                EnableTaskBarProgress = true,
            };

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

                // Get folder names for the current repository
                var repoDestination = Path.Combine(Destination, repo.FullName);

                ChildProgressBar progressBar = null;
                Exception cloneException = null;

                var cloneOptions = new CloneOptions();
                cloneOptions.RecurseSubmodules = true;
                cloneOptions.OnTransferProgress = (progress) =>
                {
                    if (progressBar == null)
                        
                        if (Globals._AllBranches)
                        {
                            // If all branches is selected for backup (all branches option) - show branches in progressbar
                            foreach (var branchName in branchNames)
                            {
                                progressBar = rootProgressBar.Spawn(progress.TotalObjects, repo.Name + ", Branch: '" + branchName.Name + "'" , new ProgressBarOptions
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
                            progressBar = rootProgressBar.Spawn(progress.TotalObjects, repo.Name + ", Branch: 'DefaultBranch'" , new ProgressBarOptions
                            {
                                CollapseWhenFinished = true,
                                ForegroundColorDone = ConsoleColor.Green,
                                ForegroundColor = ConsoleColor.Yellow
                            });
                        }

                    progressBar.Tick(progress.ReceivedObjects);
                    return true;
                };

                cloneOptions.RepositoryOperationCompleted = (context) =>
                {
                    // Dispose the progressbar
                    progressBar?.Dispose();
                };

                // Set credentials for Github - basic or oauth
                if (Credentials.AuthenticationType == AuthenticationType.Basic)
                {
                    // Set credentials for Github - basic
                    cloneOptions.CredentialsProvider = (url, user, cred)
                        => new LibGit2Sharp.UsernamePasswordCredentials { Username = Credentials.Login, Password = Credentials.Password };
                }
                else if (Credentials.AuthenticationType == AuthenticationType.Oauth)
                {
                    // Set credentials for Github - oauth
                    cloneOptions.CredentialsProvider = (url, user, cred)
                        => new LibGit2Sharp.UsernamePasswordCredentials { Username = Credentials.GetToken(), Password = string.Empty };
                }

                try
                {
                    if (Globals._AllBranches)
                    {
                        // Backup all branches for the current repository selected for backup
                        foreach (var branchName in branchNames)
                        {
                            // Clone the specific branch
                            string clonedRepoPath = Path.Combine(repoDestination, branchName.Name); // Create a folder for the branch
                            LibGit2Sharp.Repository.Clone(repo.CloneUrl, clonedRepoPath, cloneOptions);

                            Message($"Processed repository '{repo.FullName}' for backup - Options: ALL branches: saved data for branch '{branchName.Name}' to disk", EventType.Information, 1000);
                            //Console.WriteLine($"Processed repository {repo.FullName} - ALL branch: saved data for branch {branchName.Name} to disk");

                            Globals.repoitemscountelements.Add($"Repository Name: '{repo.Name}', Branch: '{branchName.Name}', Owner: '{repo.Owner.Login}'");

                            Globals._repoBackupedCount++; // Increment the _repoCount integer for count of repos in total

                            // Count repos processed
                            lock (lockObject)
                            {
                                //Globals._repoBackupedCount++; // Increment the _repoCount integer for count of repos in total
                            }
                        }
                    }
                    else
                    {
                        // Backup only the default branch for the current repository selected for backup (default branch) - this is the default option
                        LibGit2Sharp.Repository.Clone(repo.CloneUrl, repoDestination, cloneOptions);

                        Message($"Processed repository: '{repo.FullName}' for backup, DefaultBranch '{repo.DefaultBranch}' - saved data to disk", EventType.Information, 1000);
                    
                        Globals.repoitemscountelements.Add($"Repository Name: '{repo.Name}', DefaultBranch: '{repo.DefaultBranch}', Owner: '{repo.Owner.Login}'");

                        Globals._repoBackupedCount++; // Increment the _repoCount integer for count of repos in total

                        // Count repos processed
                        lock (lockObject)
                        {
                            //Globals._repoBackupedCount++; // Increment the _repoCount integer for count of repos in total
                        }
                    }
                }
                catch (LibGit2SharpException libGit2SharpException)
                {
                    if (libGit2SharpException.Message == "this remote has never connected")
                    {
                        Console.WriteLine("An error occurred; GitHub may be down or you have no internet!");
                    }
                    else
                    {
                        Console.WriteLine("An unknown error occurred whilst trying to retrieve data from github when processing repository: " + repo.FullName + " when save data to disk - Error: "+ libGit2SharpException.Message);
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
            });

            rootProgressBar.Dispose();

            return exceptions; // Add this return statement at the end
        }
    }
}