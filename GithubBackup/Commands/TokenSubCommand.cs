using System;
using System.IO;
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

                // Define options for backup types
                var allReposOption = tokenBasedCmd.Option("-all", "Backup all repositories.", CommandOptionType.NoValue);
                var allReposNotForksOption = tokenBasedCmd.Option("-allnf", "Exclude forked repositories.", CommandOptionType.NoValue);
                var allReposOwnerOption = tokenBasedCmd.Option("-allowner", "Backup repositories where you are the owner (default).", CommandOptionType.NoValue);
                var allBranchesOption = tokenBasedCmd.Option("-allbranches", "Backup all branches of repositories (default only DefaultBranch).\n", CommandOptionType.NoValue);
                
                // Define options for email when using token-based backup
                var mailToOption = tokenBasedCmd.Option("-mailto <email>", "Specify the email address to send backup notifications to.", CommandOptionType.SingleValue);
                var mailFromOption = tokenBasedCmd.Option("-mailfrom  <email>", "Specify the email address to send backup notifications from.", CommandOptionType.SingleValue);
                var mailServerOption = tokenBasedCmd.Option("-mailserver <server>", "Specify the IP address or DNS name of the SMTP server to use for sending notifications.", CommandOptionType.SingleValue);
                var mailPortOption = tokenBasedCmd.Option("-mailport <port>", "Specify the port to use for the email server.\n", CommandOptionType.SingleValue);
                
                // Define an option for email priority
                var priorityOption = tokenBasedCmd.Option("-priority <priority>", "Set the email report priority (low/normal/high) (if not set default is normal).", CommandOptionType.SingleValue);

                // Define an option for days to keep backup
                var daysToKeepBackupOption = tokenBasedCmd.Option("-daystokeepbackup <days>", "Number of days to keep backups for. Backups older than this will be deleted (default is 30 days).", CommandOptionType.SingleValue);

                // Define an option for simple email report layout
                var mailSimpleReport = tokenBasedCmd.Option("-simpelreport", "If set the email report layout there is send is simple, if not set it use the default report layout", CommandOptionType.NoValue);
                
                // Define arguments for token-based backup
                var tokenArgument = tokenBasedCmd.Argument("Token", "A valid github token.").IsRequired();
                var destinationArgument = tokenBasedCmd.Argument("Destination", "The destination folder for the backup.");

                #endregion Set/show arguments used for token-based backup

                // Define the action to take when the command is invoked
                tokenBasedCmd.OnExecute(() =>
                {
                    #region Set email options
                    
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

                        // Check if the simple email report layout option is set
                        if (mailSimpleReport.HasValue())
                        {
                            Globals._useSimpleMailReportLayout = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Email options are NOT set - will not send report");
                        Message("Email options are NOT set - will not send report", EventType.Information, 1000);

                        // Set the email options are set to false
                        Globals._emailOptionsIsSet = false;
                    }

                    #endregion Set email options

                    #region Set options for backup to keep

                    // Parse data for daysToKeepBackup
                    if (daysToKeepBackupOption.HasValue())
                    {
                        // Set backup to keep days to the value provided
                        Globals._daysToKeepBackup = int.Parse(daysToKeepBackupOption.Value() ?? string.Empty);

                        // Set status text for email
                        Globals._isDaysToKeepNotDefaultStatusText = "Custom number of old backup(s) set to keep in backup folder (days)";

                        Message("Days to keep backups is set to: " + Globals._daysToKeepBackup, EventType.Information, 1000);
                    }
                    else
                    {
                        // Set backup to keep days to default value
                        Globals._daysToKeepBackup = 30;

                        // Set status text for email
                        Globals._isDaysToKeepNotDefaultStatusText = "Default number of old backup(s) set to keep in backup folder (days)";

                        Message("Days to keep backups is set to: " + Globals._daysToKeepBackup + " (default value as no argument is set)", EventType.Information, 1000);
                    }

                    #endregion Set options for backup to keep

                    var credentials = CredentialsFactory(tokenArgument.Value);
                    var currentFolder = Directory.GetCurrentDirectory();
                    var destinationFolder = string.IsNullOrWhiteSpace(destinationArgument.Value) ? currentFolder : destinationArgument.Value;
                    var backupService = BackupServiceFactory(credentials, destinationFolder);

                    #region Check backup folder location and create it if not exists
                    
                    // Check if the destination folder exists and create it if not exists
                    if (!Directory.Exists(destinationFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(destinationFolder);
                            Message("Created backup folder: " + destinationFolder, EventType.Information, 1000);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Message("Unable to create folder to store the backups: " + destinationFolder + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Unable to create folder to store the backups: " + destinationFolder + ". Make sure the account you use to run this tool has write rights to this location.");
                            Console.ResetColor();

                            // Count errors
                            Globals._errors++;
                        }
                        catch (Exception e)
                        {
                            // Error when create backup folder
                            Message("Exception caught when trying to create backup folder '" + destinationFolder + " - error: " + e, EventType.Error, 1001);
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

                    // Set the backup type based on options
                    if (allReposOption.HasValue())
                    {
                        // Set the backup type to all repos the token have access to (including forks)
                        Globals._allRepos = true;
                    }
                    else if (allReposNotForksOption.HasValue())
                    {
                        // Set the backup type to all repos not forks
                        Globals._allReposNotForks = true;
                    }

                    // Set the backup type based on options for owner
                    else if (allReposOwnerOption.HasValue())
                    {
                        // Set the backup type to all repos not forks and is owner
                        Globals._allReposNotForksAndIsOwner = true;
                    }
                    else
                    {
                        // Set the backup type to all repos not forks and is owner
                        Globals._allReposNotForksAndIsOwner = true;
                    }

                    // Set the backup type based on options for branches
                    if (allBranchesOption.HasValue())
                    {
                        // Set the backup type to all branches for repos
                        Globals._allBranches = true;
                    }
                    else
                    {
                        // Set the backup type to all branches for repos
                        Globals._allBranches = false;
                    }

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
                            Backups.DaysToKeepBackupsDefault(destinationFolder);
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
                            Backups.DaysToKeepBackups(destinationFolder, Globals._daysToKeepBackup);
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
                        Backups.DaysToKeepBackupsDefault(destinationFolder);
                    }

                    // Count backups in backup folder
                    Backups.CountCurrentNumersOfBackup(destinationFolder);

                    #endregion Do options for backup to keep

                    // Create the backup and parse the arguments
                    backupService.CreateBackup();
                });
            });
        }
    }
}