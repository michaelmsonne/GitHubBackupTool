using System;
using System.IO;
using GithubBackup.Class;
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
                
                // Define options for backup types
                var allReposOption = tokenBasedCmd.Option("-all", "Backup all repositories.", CommandOptionType.NoValue);
                var allReposnotForksOption = tokenBasedCmd.Option("-allnf", "Exclude forked repositories.", CommandOptionType.NoValue);
                var allReposownerOption = tokenBasedCmd.Option("-allowner", "Backup repositories where you are the owner (default).", CommandOptionType.NoValue);
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
                var mailSimpelReport = tokenBasedCmd.Option("-simpelreport", "If set the email report layout there is send is simple, if not set it use the default report layout", CommandOptionType.NoValue);
                
                // Define arguments for token-based backup
                var tokenArgument = tokenBasedCmd.Argument("Token", "A valid github token.").IsRequired();
                var destinationArgument = tokenBasedCmd.Argument("Destination", "The destination folder for the backup.");

                // Define the action to take when the command is invoked
                tokenBasedCmd.OnExecute(() =>
                {
                    // Check the email priority option
                    string emailPriorityString = priorityOption.Value() ?? "normal"; // Default to "normal" if not specified
                    
                    // Check if the email options are provided and use them
                    if (mailToOption.HasValue() && mailFromOption.HasValue() && mailServerOption.HasValue() && mailPortOption.HasValue())
                    {
                        Globals._mailto = mailToOption.Value();
                        Globals._mailfrom = mailFromOption.Value();
                        Globals._mailserver = mailServerOption.Value();
                        Globals._mailport = int.Parse(mailPortOption.Value());

                        // Set the email options are set to true
                        Globals._emailOptionsIsSet = true;

                        // Convert the string to MailPriority enum using a function
                        Globals.EmailPriority = ReportSenderOptions.ParseEmailPriority(emailPriorityString);

                        // Check if the simple email report layout option is set
                        if (mailSimpelReport.HasValue())
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

                    // Parse data for daysToKeepBackup
                    if (daysToKeepBackupOption.HasValue())
                    {
                        // Set backup to keep days to the value provided
                        Globals._daysToKeepBackup = int.Parse(daysToKeepBackupOption.Value());

                        Message("Days to keep backups is set to: " + Globals._daysToKeepBackup, EventType.Information, 1000);
                    }
                    else
                    {
                        // Set backup to keep days to default value
                        Globals._daysToKeepBackup = 30;

                        Message("Days to keep backups is set to: " + Globals._daysToKeepBackup + " (default value as no argument is set)", EventType.Information, 1000);
                    }

                    var credentials = CredentialsFactory(tokenArgument.Value);
                    var currentFolder = Directory.GetCurrentDirectory();
                    var destinationFolder = string.IsNullOrWhiteSpace(destinationArgument.Value) ? currentFolder : destinationArgument.Value;
                    var backupService = BackupServiceFactory(credentials, destinationFolder);

                    // Check if the destination folder exists and create it if not exists
                    if (!Directory.Exists(destinationFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(destinationFolder);
                            Message("Created backup folder: " + destinationFolder, EventType.Information, 1000);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                            Message("Error when try to create backup folder: " + destinationFolder + " Error: " + e, EventType.Information, 1000);
                            throw;
                        }
                    }

                    // Set the backup type based on options
                    if (allReposOption.HasValue())
                    {
                        // Set the backup type to all repos the token have access to (including forks)
                        Globals._AllRepos = true;
                    }
                    else if (allReposnotForksOption.HasValue())
                    {
                        // Set the backup type to all repos not forks
                        Globals._AllReposNotForks = true;
                    }

                    // Set the backup type based on options for owner
                    else if (allReposownerOption.HasValue())
                    {
                        // Set the backup type to all repos not forks and is owner
                        Globals._AllReposNotForksAndIsOwner = true;
                    }
                    else
                    {
                        // Set the backup type to all repos not forks and is owner
                        Globals._AllReposNotForksAndIsOwner = true;
                    }

                    // Set the backup type based on options for branches
                    if (allBranchesOption.HasValue())
                    {
                        // Set the backup type to all branches for repos
                        Globals._AllBranches = true;
                    }
                    else
                    {
                        // Set the backup type to all branches for repos
                        Globals._AllBranches = false;
                    }
                    
                    // Check if the daysToKeepBackup option is set
                    if (daysToKeepBackupOption.HasValue())
                    {
                        //Console.WriteLine("TEST daysToKeepBackupOption - TRUE");
                        
                        // Check if data is not null
                        if (Globals._daysToKeepBackup != null)
                        {
                            // If set to 30 (default) show it - other text if --daystokeepbackup is not set
                            if (Globals._daysToKeepBackup == 30)
                            {
                                // Log
                                Message($"argument -daystokeepbackup is set to (default) {Globals._daysToKeepBackup}", EventType.Information, 1000);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"argument -daystokeepbackup is set to (default) {Globals._daysToKeepBackup}");
                                Console.ResetColor();

                                // Set status text for email
                                //isDaysToKeepNotDefaultStatusText = "Default number of old backup(s) set to keep in backup folder (days)";

                                // Log
                                //Message(isDaysToKeepNotDefaultStatusText, EventType.Information, 1000);

                                // Do work
                                Backups.DaysToKeepBackupsDefault(destinationFolder);
                            }

                            // If --daystokeepbackup is not set to default 30 - show it and do work
                            if (Globals._daysToKeepBackup != 30)
                            {
                                // Log
                                Message($"argument -daystokeepbackup is not default (30), it is set to {Globals._daysToKeepBackup} days", EventType.Information, 1000);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"argument -daystokeepbackup is not default (30), it is set to {Globals._daysToKeepBackup} days");
                                Console.ResetColor();

                                // Set status text for email
                                //isDaysToKeepNotDefaultStatusText = "Custom number of old backup(s) set to keep in backup folder (days)";

                                // Log
                                //Message(isDaysToKeepNotDefaultStatusText, EventType.Information, 1000);

                                // Do work
                                Backups.DaysToKeepBackups(destinationFolder, Globals._daysToKeepBackup);
                            }
                        }
                        else
                        {
                            // Set default
                            Backups.DaysToKeepBackupsDefault(destinationFolder);
                        }

                    }
                    else
                    {
                        //Console.WriteLine("TEST daysToKeepBackupOption - FALSE");

                        // Log
                        Message($"argument -daystokeepbackup does not exits - using default backups to keep (30 days)!", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\nargument -daystokeepbackup does not exits - using default backups to keep (30 days)!\n");
                        Console.ResetColor();

                        // Do work
                        Backups.DaysToKeepBackupsDefault(destinationFolder);
                    }

                    // Count backups in backup folder
                    Backups.CountCurrentNumersOfBackup(destinationFolder);

                    //Console.WriteLine("Globals._currentBackupsInBackupFolderCount: " + Globals._currentBackupsInBackupFolderCount);

                    // Create the backup and parse the arguments
                    backupService.CreateBackup();
                });
            });
        }
    }
}