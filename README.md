# GitHubBackupTool

<p align="center">
  <a href="https://github.com/michaelmsonne/GitHubBackupTool"><img src="https://img.shields.io/github/languages/top/michaelmsonne/GitHubBackupTool.svg"></a>
  <a href="https://github.com/michaelmsonne/GitHubBackupTool"><img src="https://img.shields.io/github/languages/code-size/michaelmsonne/GitHubBackupTool.svg"></a>
  <a href="https://github.com/michaelmsonne/GitHubBackupTool"><img src="https://img.shields.io/github/downloads/michaelmsonne/GitHubBackupTool/total.svg"></a>
</p>

# Introduction
This tool creates a local backup of repositories of a given GitHub user with some options to filter based on some conditions.
You can also use it to backup all repositories of an organization of you created a personal access token with the right permissions under there.

All repositories of the given GitHub API key will be concurrently cloned to the provided destination folder if selected.

If you don´t provide a destination folder in the argument, the backup will be created in your current folder there the application is executed.

While your code is perfectly safe on the GitHub infrastructure, there are cases where a centralized local backup of all projects and repositories is needed. These might include Corporate Policies, Disaster Recovery and Business Continuity Plans.

</br>

![GitHubBackupTool screenshot](screenshot.png?raw=true "GitHubBackupTool screenshot")

</br>

## Download

[Download the latest version](../../releases/latest)

[Version History](CHANGELOG.md)

## Usage

```bash
GitHubBackupTool token-based <token> [<destination>] [<backuptype>] -mailto "mail-to@domain.com" -mailfrom "mail-from@domain.com" -mailserver "mailserver.domain.com" -mailport "25" -priority "high" -daystokeepbackup 50
```

etc.: 
```bash
GitHubBackupTool token-based XXX... "D:\Backup\GitHub\" -allowner -mailto "mail-to@domain.com" -mailfrom "mail-from@domain.com" -mailserver "mailserver.domain.com" -mailport "25" -priority "high" -daystokeepbackup 50
```

### Needed:

You can create a personal access token here: https://github.com/settings/tokens/new

Access: Read access to repositories who needs backup and read access to user profile.

</br>

# Final thoughts
This is not an exhaustive method to retrieve every artifact on GitHub. There’s a lot more to be done to make this a complete solution.
However, it’s a good starting point to backup your GitHub projects and keep a local repository of these like I do! 😜😉

There is send an email report to the specified email address when the backup is done with status and usefull information about the backup job and more information.

You can also use the tool to backup all repositories of an organization of you created a personal access token with the right permissions under there.

And the tool can also be used to backup all repositories of a user.

Use it in a scheduled task to backup your repositories every day or week or month or whatever you want manually or automatically.

# Email report sample:

**Full layout:**

![Screenshot](docs/email-report-full.png)

**Simpel layout:**

![Screenshot](docs/email-report-simpel.png)

# Console use:

**Help menu:**

![Screenshot](docs/help-menu.png)

**About menu:**

![Screenshot](docs/help-about.png)

# To-do list:
> Add branch filter option/parameter (done)


## Building

```bash
$ dotnet publish -r win10-x64 -c release
```

So far I tested the application only for win10-x64 systems, but it might also work on other platforms.


## Used 3rd party libraries for the tool:

[Octokit.NET](https://github.com/octokit/octokit.net)

[McMaster's CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils)

[LitGit2Sharp](https://github.com/libgit2/libgit2sharp)

[Autofac](https://github.com/autofac/Autofac)

[ShellProgressBar](https://github.com/Mpdreamz/shellprogressbar)
