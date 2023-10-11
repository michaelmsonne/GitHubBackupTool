﻿using System;
using GithubBackup.Class;
using McMaster.Extensions.CommandLineUtils;

namespace GithubBackup.Commands
{
    public class BackupCommand
    {
        public CommandLineApplication Command { get; set; }

        public BackupCommand(Func<CommandLineApplication, TokenSubCommand> tokenCmdWrapperFactory)
        {
            Command = new CommandLineApplication();
            Command.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw;
            Command.Name = "GitHubBackupTool";
            Command.Description = "GitHub Backup for Git Projects and is using the API for GitHub.";
            Command.ExtendedHelpText =  "\nMore information about this tool:" +
                                        "\n  This tool creates a local backup of repositories of a given github user with some options to filter " +
                                        "\n  based on some conditions." +
                                        "\n" +
                                        "\n  While the code is perfectly safe on the GitHub infrastructure, there are cases where a centralized" +
                                        "\n  local backup of all projects and repositories is needed. These might include Corporate Policies," +
                                        "\n  Disaster Recovery and Business Continuity Plans." +
                                        "" +
                                        "\n\nExamples:" +
                                        $"\n  {Globals._currentExeFileName} token-based XXX... \"D:\\Backup\\GitHub\" -allowner -mailto \"mail-to@domain.com\"" +
                                        $"\n  -mailfrom \"mail-from@domain.com\" -mailserver \"mailserver.domain.com\" -mailport \"25\" -priority high" +
                                        $"\n  -daystokeepbackup 50\n\n" +
                                        $"My Website:" +
                                        $"\n  https://sonnes.cloud\n";
            Command.HelpOption(true);

            // Define the backup type argument
            // Command.Argument("-all", "Backup all repositories");
            // Command.Argument("-allnf", "Exclude forked repositories");
            // Command.Argument("-allowner", "Backup repositories where you are the owner (default)");
            
            // Define the token subcommand and execute it
            Command.OnExecute(() =>
            {
                Command.ShowHelp();
                Console.WriteLine();
                Console.WriteLine("Please specify the authentication mode via the appropriate subcommand.");
                Console.WriteLine();
                return 1;
            });

            tokenCmdWrapperFactory(Command);
        }
    }
}