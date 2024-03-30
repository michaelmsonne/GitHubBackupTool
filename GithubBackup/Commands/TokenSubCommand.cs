using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using GithubBackup.Class;
using GithubBackup.Core;
using McMaster.Extensions.CommandLineUtils;
using Octokit;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Commands
{
    public class TokenSubCommand
    {
        public CommandLineApplication ParentCommand { get; set; }
        public CommandLineApplication Command { get; set; }
        public Func<Credentials, string, BackupService> BackupServiceFactory { get; set; }
        public Func<string, Credentials> CredentialsFactory { get; set; }
        
        private static void SaveTokenToFile(CommandOption tokenFileOption)
        {
            // Get key to use for encryption and decryption
            var key = SecureArgumentHandlerToken.GetComputerId();

            // Get data from console
            string tokentoencrypt = tokenFileOption.Values[0];

            // Encrypt data
            //string key = "your_key"; // Replace with the actual key for encryption
            SecureArgumentHandlerToken.EncryptAndSaveToFile(key, tokentoencrypt);

            //Console.WriteLine("Key: " + key);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Saved information about token to file - Exciting {Globals._appName}, v.{Globals._vData} by {Globals._companyName}!");
            Console.ResetColor();

            // End application
            Environment.Exit(1);
        }

        // Securely clear a SecureString from memory
        private static void ClearSecureString(SecureString secureString)
        {
            if (secureString != null)
            {
                secureString.Clear();

                // Release unmanaged resources
                IntPtr ptr = Marshal.SecureStringToBSTR(secureString);
                Marshal.ZeroFreeBSTR(ptr);
            }
        }

        public TokenSubCommand(CommandLineApplication parentCommand, Func<Credentials, string, BackupService> backupServiceFactory, Func<string, Credentials> credentialsFactory)
        {
            ParentCommand = parentCommand;
            BackupServiceFactory = backupServiceFactory;
            CredentialsFactory = credentialsFactory;

            Command = ParentCommand.Command("token-based", (tokenBasedCmd) =>
            {
                tokenBasedCmd.Description = "Using a token-based authentication.";
                tokenBasedCmd.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw;

                #region Set/show arguments used for token-based backup

                // Define options for backup types when using token-based backup
                // Set the backup type based on options for repos (all, all not forks, all not forks and is owner and so on...)
                var allReposOption = tokenBasedCmd.Option("-all", "Backup all repositories.", CommandOptionType.NoValue);
                var allReposNotForksOption = tokenBasedCmd.Option("-allnf", "Exclude forked repositories.", CommandOptionType.NoValue);
                var allReposOwnerOption = tokenBasedCmd.Option("-allowner", "Backup repositories where you are the owner (default).", CommandOptionType.NoValue);
                var allBranchesOption = tokenBasedCmd.Option("-allbranches", "Backup all branches of repositories (default only DefaultBranch).", CommandOptionType.NoValue);
                var excludeBranchDependabot = tokenBasedCmd.Option("-excludebranchdependabot", "Exclude branches with 'dependabot' in it from backup.", CommandOptionType.NoValue);
                var backupMetadataOption = tokenBasedCmd.Option("-backupmetadata", "Backup metadata for each repository. If set, the data itself will be saved to the repo folder.", CommandOptionType.NoValue);
                var backupIssueDataOption = tokenBasedCmd.Option("-backupissuedata", "(Disabled) Backup metadata for issues for each repository", CommandOptionType.NoValue);
                var backupReviewCommentsDataOption = tokenBasedCmd.Option("-backupreviewcommentdata", "(Disabled) Backup metadata for review comment for each repository. If set, the data itself will be saved to the repo folder.", CommandOptionType.NoValue);
                var backupReleasedataOption = tokenBasedCmd.Option("-backupreleasedata", "Backup release data for each repository. If set, the data itself will be saved to the repo folder.\n", CommandOptionType.NoValue);

                // Define options for email when using token-based backup for sending report to email address (if set)
                var mailToOption = tokenBasedCmd.Option("-mailto <email>", "Specify the email address to send backup notifications to.", CommandOptionType.SingleValue);
                var mailFromOption = tokenBasedCmd.Option("-mailfrom  <email>", "Specify the email address to send backup notifications from.", CommandOptionType.SingleValue);
                var mailServerOption = tokenBasedCmd.Option("-mailserver <server>", "Specify the IP address or DNS name of the SMTP server to use for sending notifications.", CommandOptionType.SingleValue);
                var mailPortOption = tokenBasedCmd.Option("-mailport <port>", "Specify the port to use for the email server.", CommandOptionType.SingleValue);
                
                // Define an option for email priority (if set) - if not set it use default priority (normal)
                var priorityOption = tokenBasedCmd.Option("-priority <priority>", "Set the email report priority (low/normal/high) (if not set default is normal).\n", CommandOptionType.SingleValue);

                // Define an option for simple email report layout (if set) - if not set it use default report layout (more advanced)
                var mailSimpleReport = tokenBasedCmd.Option("-simpelreport", "If set the email report layout there is send is simple, if not set it use the default report layout\n", CommandOptionType.NoValue);

                // Define an option for days to keep backup in backup folder before deleting it (default is 30 days) - if not set it use default value
                var daysToKeepBackupOption = tokenBasedCmd.Option("-daystokeepbackup <days>", "Number of days to keep backups for. Backups older than this will be deleted (default is 30 days).", CommandOptionType.SingleValue);

                // Define an option for backup repo validation
                var backupRepoValidationOption = tokenBasedCmd.Option("-gitbackupvalidation", "Validate backup of repositories after backup is done. If set, the backup will be validated.", CommandOptionType.NoValue);

                // Define an option for days to keep log files in log folder before deleting it (default is 30 days) - if not set it use default value
                var daysToKeepLogFilesOption = tokenBasedCmd.Option("-daystokeeplogfiles <days>", "Number of days to keep log files for. Log files older than this will be deleted (default is 30 days).\n", CommandOptionType.SingleValue);

                // Define the --tokenfile option
                var tokenFileOption = tokenBasedCmd.Option("-tokenfile", "Save token data to a file for encryption. (Only supported on Windows for the time..)\n", CommandOptionType.SingleValue);

                // Define arguments for token-based backup (token and destination folder)
                var tokenArgument = tokenBasedCmd.Argument("Token", "A valid github token.");
                var destinationArgument = tokenBasedCmd.Argument("Destination", "The destination folder for the backup.");
                
                #endregion Set/show arguments used for token-based backup

                // Define the action to take when the command is invoked
                tokenBasedCmd.OnExecute(() =>
                {
                    // Check if the --tokenfile option is present - if it is, save token data to a file and exit the application
                    if (tokenFileOption.HasValue())
                    {
                        // If --tokenfile option is present, save token data to a file
                        SaveTokenToFile(tokenFileOption);
                        return 1; // Exit the application
                    }

                    #region Set email options and check if they are set and have required values

                    // Log
                    Message("Processing arguments set for what type of email report to create...", EventType.Information, 1000);

                    // Check the email priority option
                    var emailPriorityString = priorityOption.Value() ?? "normal"; // Default to "normal" if not specified
                    
                    // Check if the email options are provided and use them
                    if (mailToOption.HasValue() && mailFromOption.HasValue() && mailServerOption.HasValue() && mailPortOption.HasValue())
                    {
                        Globals._mailto = mailToOption.Value();
                        Globals._mailfrom = mailFromOption.Value();
                        Globals._mailserver = mailServerOption.Value();
                        Globals._mailport = int.Parse(mailPortOption.Value() ?? string.Empty);

                        // Set the email options are set to true
                        Globals._emailOptionsIsSet = true;

                        // Convert the string to MailPriority enum using a function
                        Globals._emailPriority = ReportSenderOptions.ParseEmailPriority(emailPriorityString);

                        // Log
                        Console.WriteLine("Email options are set - will send report");
                        Message("Email options are set - will send report", EventType.Information, 1000);

                        // Check if the simple email report layout option is set
                        if (mailSimpleReport.HasValue())
                        {
                            // Log
                            Console.WriteLine("Email options are set - will send simple layout report");
                            Message("Email options are set - will send simple layout report", EventType.Information, 1000);

                            // Set the simple email report layout to true
                            Globals._useSimpleMailReportLayout = true;
                        }
                    }
                    else
                    {
                        // Log
                        Console.WriteLine("Email options are NOT set - will not send report");
                        Message("Email options are NOT set - will not send report", EventType.Information, 1000);

                        // Set the email options are set to false
                        Globals._emailOptionsIsSet = false;
                    }

                    // Log
                    Message("> Done processing arguments set for what type of email report to create and send", EventType.Information, 1000);

                    #endregion Set email options

                    #region Set options for backup to keep

                    // Parse data for daysToKeepBackup
                    if (daysToKeepBackupOption.HasValue())
                    {
                        // Set backup to keep days to the value provided
                        Globals._daysToKeepBackup = int.Parse(daysToKeepBackupOption.Value() ?? string.Empty);

                        // Set status text for email
                        Globals._isDaysToKeepNotDefaultStatusText = "Custom number of old backup(s) set to keep in backup folder (day(s))";

                        Message("Day(s) to keep backups is set to: " + Globals._daysToKeepBackup, EventType.Information, 1000);
                    }
                    else
                    {
                        // Set backup to keep days to default value
                        Globals._daysToKeepBackup = 30;

                        // Set status text for email
                        Globals._isDaysToKeepNotDefaultStatusText = "Default number of old backup(s) set to keep in backup folder (day(s))";

                        Message("Day(s) to keep backups is set to: " + Globals._daysToKeepBackup + " (default value as no argument is set)", EventType.Information, 1000);
                    }

                    #endregion Set options for backup to keep
                    
                    var credentials = CredentialsFactory(tokenArgument.Value);
                    var currentFolder = Directory.GetCurrentDirectory();
                    var destinationFolder = string.IsNullOrWhiteSpace(destinationArgument.Value) ? Path.Combine(currentFolder, "Backup") : destinationArgument.Value;

                    #region SecureToken

                    // If the token is set to "token.bin" then read the token from the file else use the token directly from the command-line arguments
                    if (tokenArgument.Value == "token.bin")
                    {
                        // Read the token information from the -tokentofile
                        // Get key to use for encryption and decryption
                        var key = SecureArgumentHandlerToken.GetComputerId();
                        string decryptedToken = SecureArgumentHandlerToken.DecryptFromFile(key);

                        // Update the credentials with the decrypted token
                        credentials = CredentialsFactory(decryptedToken);

#if DEBUG
                            Console.WriteLine($"Decrypted string for token = {decryptedToken}");
                            Console.ReadKey();
#endif
                    }
                    else
                    {
                        // Use the token directly from the command-line arguments
                        credentials = CredentialsFactory(tokenArgument.Value);
                    }

                    var backupService = BackupServiceFactory(credentials, destinationFolder);

                    /*
                     * Clear string token
                     */
                    // Use SecureString to securely store sensitive information
                    using (SecureString secureToken = new SecureString())
                    {
                        foreach (char c in tokenArgument.Value)
                        {
                            secureToken.AppendChar(c);
                        }

                        // Perform operations with secureToken here

                        // Clear the secureToken from memory
                        ClearSecureString(secureToken);
                    }
                    /*
                     * End clear string token
                     */
                    #endregion SecureToken

                    #region Check backup folder location and create it if not exists

                    // Check if the destination folder exists and create it if not exists
                    if (!Directory.Exists(destinationFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(destinationFolder);
                            Message("Created root backup folder: '" + destinationFolder + "'", EventType.Information, 1000);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Message("Unable to create folder to store the backup(s): '" + destinationFolder + "'. Make sure the account you use to run this tool has write rights/create to this location.", EventType.Error, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Unable to create folder to store the backup(s): '" + destinationFolder + "'. Make sure the account you use to run this tool has write/create rights to this location.");
                            Console.ResetColor();

                            // Count errors
                            Globals._errors++;
                        }
                        catch (Exception e)
                        {
                            // Error when create backup folder
                            Message("Exception caught when trying to create backup folder '" + destinationFolder + "' - error: " + e, EventType.Error, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0} Exception caught.", e);
                            Console.ResetColor();

                            // Count errors
                            Globals._errors++;

                            //throw;
                        }
                    }

                    #endregion Check backup folder location and create it if not exists

                    #region Set options for backup type

                    // Log
                    Message("Processing arguments set for what type of backup(s) to create...", EventType.Information, 1000);


                    if (backupRepoValidationOption.HasValue())
                    {
                        // Set the backup type for metadata to true
                        Globals._backupRepoValidation = true;

                        // Log
                        Message("Set to validate backup of repositories after backup is done", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type for metadata to false
                        Globals._backupRepoValidation = false;

                        // Log
                        Message("Set to NOT validate backup of repositories after backup is done", EventType.Information, 1000);
                    }


                    
                    // Set the backup type based on options for Review Comments metadata
                    if (backupReviewCommentsDataOption.HasValue())
                    {
                        // Set the backup type for metadata to true
                        Globals._backupReviewCommentsdata = true;

                        // Log
                        Message("Set to download review comments metadata for repository in the backup(s)", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type for metadata to false
                        Globals._backupReviewCommentsdata = false;

                        // Log
                        Message("Set to NOT download review comments metadata for repository in the backup(s)", EventType.Information, 1000);
                    }

                    // Set the backup type based on options for issue metadata
                    if (backupIssueDataOption.HasValue())
                    {
                        // Set the backup type for metadata to true
                        Globals._backupIssuedata = true;

                        // Log
                        Message("Set to download issue metadata for repository in the backup(s)", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type for metadata to false
                        Globals._backupIssuedata = false;

                        // Log
                        Message("Set to NOT download issue metadata for repository in the backup(s)", EventType.Information, 1000);
                    }

                    // Set the backup type based on options for metadata
                    if (backupMetadataOption.HasValue())
                    {
                        // Set the backup type for metadata to true
                        Globals._backupRepoMetadata = true;

                        // Log
                        Message("Set to download metadata for repository in the backup(s)", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type for metadata to false
                        Globals._backupRepoMetadata = false;

                        // Log
                        Message("Set to NOT download metadata for repository in the backup(s)", EventType.Information, 1000);
                    }

                    // Set the backup type based on options for release data
                    if (backupReleasedataOption.HasValue())
                    {
                        // Set the backup type for release data to true
                        Globals._backupReleasedata = true;

                        // Log
                        Message("Set to download release data for repository in the backup(s)", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type for release data to false
                        Globals._backupReleasedata = false;

                        // Log
                        Message("Set to NOT download release data for repository in the backup(s)", EventType.Information, 1000);
                    }
                    
                    // Set the backup type based on options
                    if (allReposOption.HasValue())
                    {
                        // Set the backup type to all repos the token have access to (including forks)
                        Globals._allRepos = true;

                        // Log
                        Message("Set to download all repositories the API Key have access to", EventType.Information, 1000);
                    }
                    else if (allReposNotForksOption.HasValue())
                    {
                        // Set the backup type to all repos not forks
                        Globals._allReposNotForks = true;

                        // Log
                        Message("Set to download all repositories the API Key have access to but not forks", EventType.Information, 1000);
                    }

                    // Set the backup type based on options for owner
                    else if (allReposOwnerOption.HasValue())
                    {
                        // Set the backup type to all repos not forks and is owner
                        Globals._allReposNotForksAndIsOwner = true;

                        // Log
                        Message("Set to download all repositories the API Key have access to but not forks and is owner (so your own)", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type to all repos not forks and is owner
                        Globals._allReposNotForksAndIsOwner = true;

                        // Log
                        Message("Set to download all repositories the API Key have access to but not forks and is owner (so your own) - default as no other option is set", EventType.Information, 1000);
                    }

                    // Set the backup type based on options for branches
                    if (allBranchesOption.HasValue())
                    {
                        // Set the backup type to all branches for repos
                        Globals._allBranches = true;

                        // Log
                        Message("Set to download ALL branches for the repositories the API Key have access to", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type to all branches for repos
                        Globals._allBranches = false;

                        // Log
                        Message("Set NOT to download all branches for the repositories the API Key have access to - default branch is ONLY in the backup(s)", EventType.Information, 1000);
                    }

                    // Set the backup type based on options for branches
                    if (excludeBranchDependabot.HasValue())
                    {
                        // Set the backup type to all branches for repos excluding branches with "dependabot" in it
                        Globals._excludeBranchDependabot = true;

                        // Log
                        Message("Set NOT to include 'dependabot' branches for the repositories the API Key have access to", EventType.Information, 1000);
                    }
                    else
                    {
                        // Set the backup type to all branches for repos excluding branches with "dependabot" in it
                        Globals._excludeBranchDependabot = false;

                        // Log
                        Message("Set to include 'dependabot' branches for the repositories the API Key have access to - default as no other option is set", EventType.Information, 1000);
                    }

                    // Log
                    Message("> Done processing arguments set for what type of backup(s) to create", EventType.Information, 1000);

                    #endregion Set options for backup type

                    #region Do options for backup to keep

                    // Check if the daysToKeepBackup option is set
                    if (daysToKeepBackupOption.HasValue())
                    {
                        // If set to 30 (default) show it - other text if -daystokeepbackup is not set
                        if (Globals._daysToKeepBackup == 30)
                        {
                            // Log
                            Message($"Argument -daystokeepbackup is set to (default) {Globals._daysToKeepBackup}", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Argument -daystokeepbackup is set to (default) {Globals._daysToKeepBackup}");
                            Console.ResetColor();

                            // Do work
                            LocalBackupsTasks.DaysToKeepBackupsDefault(destinationFolder);
                        }

                        // If -daystokeepbackup is not set to default 30 - show it and do work
                        if (Globals._daysToKeepBackup != 30)
                        {
                            // Log
                            Message($"Argument -daystokeepbackup is not default (30), it is set to {Globals._daysToKeepBackup} day(s)", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Argument -daystokeepbackup is not default (30), it is set to {Globals._daysToKeepBackup} day(s)");
                            Console.ResetColor();

                            // Do work
                            LocalBackupsTasks.DaysToKeepBackups(destinationFolder, Globals._daysToKeepBackup);
                        }
                    }
                    else
                    {
                        // Log
                        Message($"Argument -daystokeepbackup does not exits - using default backups to keep (30 days)!", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\nArgument -daystokeepbackup does not exits - using default backups to keep (30 days)!\n");
                        Console.ResetColor();

                        // Do work
                        LocalBackupsTasks.DaysToKeepBackupsDefault(destinationFolder);
                    }

                    // Count backups in backup folder
                    LocalBackupsTasks.CountCurrentNumbersOfBackup(destinationFolder);

                    #endregion Do options for backup to keep

                    #region Set log files options for cleanup

                    // Parse data for daysToKeepBackup
                    if (daysToKeepLogFilesOption.HasValue())
                    {
                        // Set backup to keep days to the value provided
                        Globals._daysToKeepLogFilesOption = int.Parse(daysToKeepLogFilesOption.Value() ?? string.Empty);

                        // Set status text for email
                        Globals._isdaysToKeepLogFilesOptionDefaultStatusText = "Custom number of old log(s) set to keep in log folder (days)";

                        Message("Days to keep backups is set to: " + Globals._daysToKeepLogFilesOption, EventType.Information, 1000);
                    }
                    else
                    {
                        // Set backup to keep days to default value
                        Globals._daysToKeepLogFilesOption = 30;

                        // Set status text for email
                        Globals._isdaysToKeepLogFilesOptionDefaultStatusText = "Default number of old log(s) set to keep in log folder (days)";

                        Message("Days to keep backups is set to: " + Globals._daysToKeepLogFilesOption + " (default value as no argument is set)", EventType.Information, 1000);
                    }

                    #endregion Set log files options for cleanup

                    #region Do log files options for cleanup

                    // Check if the daysToKeepLogFilesOption option is set
                    if (daysToKeepLogFilesOption.HasValue())
                    {
                        // If set to 30 (default) show it - other text if -daystokeepbackup is not set
                        if (Globals._daysToKeepLogFilesOption == 30)
                        {
                            // Log
                            Message($"Argument -daystokeeplogfiles is set to (default) {Globals._daysToKeepLogFilesOption}", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Argument -daystokeeplogfiles is set to (default) {Globals._daysToKeepLogFilesOption}");
                            Console.ResetColor();

                            // Do work
                            LocalLogCleanup.CleanupLogs(Globals._daysToKeepLogFilesOption);
                        }

                        // If -daystokeepbackup is not set to default 30 - show it and do work
                        if (Globals._daysToKeepLogFilesOption != 30)
                        {
                            // Log
                            Message($"Argument -daystokeeplogfiles is not default (-30), it is set to -{Globals._daysToKeepLogFilesOption} day(s)", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Argument -daystokeeplogfiles is not default (-30), it is set to -{Globals._daysToKeepLogFilesOption} day(s)");
                            Console.ResetColor();

                            // Do work
                            LocalLogCleanup.CleanupLogs(Globals._daysToKeepLogFilesOption);
                        }
                    }
                    else
                    {
                        // Log
                        Message($"Argument -daystokeeplogfiles does not exits - using default log(s) to keep (30 days)!", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Argument -daystokeeplogfiles does not exits - using default log(s) to keep (30 days)!\n");
                        Console.ResetColor();

                        // Do work
                        LocalLogCleanup.CleanupLogs(Globals._daysToKeepLogFilesOption);
                    }

                    #endregion Do log files options for cleanup

                    //CheckConsole.GetCurrentParentProcessId();

                    // Create the backup and parse the arguments
                    backupService.CreateBackup();

                    return 0;
                });
            });
        }
    }
}