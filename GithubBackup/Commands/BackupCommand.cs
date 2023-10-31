﻿using System;
using GithubBackup.Class;
using McMaster.Extensions.CommandLineUtils;

namespace GithubBackup.Commands
{
    public class BackupCommand
    {
        public CommandLineApplication Command { get; set; } = new();

        public BackupCommand(Func<CommandLineApplication, TokenSubCommand> tokenCmdWrapperFactory)
        {
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
                                        $"\n  https://sonnes.cloud\n\n" +
                                        $"My blog:" +
                                        $"\n  https://blog.sonnes.cloud\n";
            Command.HelpOption(true);
            
            // Define the token sub-command and execute it
            Command.OnExecute(() =>
            {
                Command.ShowHelp();
                Console.WriteLine();
                Console.WriteLine("Please specify the authentication mode via the appropriate sub-command.");
                Console.WriteLine();
                return 1;
            });

            tokenCmdWrapperFactory(Command);
        }
    }
}