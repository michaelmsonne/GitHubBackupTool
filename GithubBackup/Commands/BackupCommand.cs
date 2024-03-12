using System;
using GithubBackup.Class;
using McMaster.Extensions.CommandLineUtils;

namespace GithubBackup.Commands
{
    public class BackupCommand : IDisposable
    {
        public CommandLineApplication Command { get; set; } = new();

        public BackupCommand(Func<CommandLineApplication, TokenSubCommand> tokenCmdWrapperFactory)
        {
            // Define the main command and options for it and execute it if no sub-command is specified
            Command.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw;
            Command.Name = "GitHubBackupTool";
            Command.Description = "GitHub Backup tool for Git Projects using the API to GitHub.";
            Command.ExtendedHelpText =  "\nMore information about this tool:" +
                                        "\n  This tool creates a local backup of repositories of a given GitHub user with some options to filter " +
                                        "\n  based on some conditions or backup type." +
                                        "\n" +
                                        "\n  While the code is perfectly safe on the GitHub infrastructure, there are cases where a centralized" +
                                        "\n  local backup of all projects and repositories is needed. These might include Corporate Policies," +
                                        "\n  Disaster Recovery and Business Continuity Plans." +
                                        "" +
                                        "\n\nExamples:" +
                                        $"\n  {Globals._currentExeFileName} token-based <token> \"D:\\Backup\\GitHub\" -allowner -mailto \"mail-to@domain.com\"" +
                                        $"\n  -mailfrom \"mail-from@domain.com\" -mailserver \"mailserver.domain.com\" -mailport \"25\" -priority high" +
                                        $"\n  -daystokeepbackup 50 -daystokeeplogfiles 180\n\n" +
                                        $"My Website:" +
                                        $"\n  https://sonnes.cloud\n\n" +
                                        $"My blog:" +
                                        $"\n  https://blog.sonnes.cloud\n\n" +
                                        $"{Globals._appName}, v. {Globals._vData} by {Globals._companyName}\n";
            Command.HelpOption(true);
            
            // Define the token sub-command and execute it
            Command.OnExecute(() =>
            {
                // Show help if no sub-command is specified
                Command.ShowHelp();
                Console.WriteLine();
                Console.WriteLine("Please specify the authentication mode via the appropriate sub-command.");
                Console.WriteLine();

                // Return error exit code
                return 1;
            });

            // Define the token sub-command and execute it
            tokenCmdWrapperFactory(Command);
        }

        // Implementing IDisposable
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose of managed resources (if any)
                    Command.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Finalizer (destructor) for additional cleanup (if needed)
        ~BackupCommand()
        {
            Dispose(false);
        }
    }
}