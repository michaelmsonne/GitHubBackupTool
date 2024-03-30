## [1.3.0.0] - XX-XX-2024

### Added
- Warning states for backups and checks
- Added feature to validate backuped git repositories (-gitbackupvalidation)

## Changed
- Fixed typo in argument tokenfile
- Some small internal code changes and cleanup
- Updated Octokit.9.0.0 to Octokit.10.0.0 (fixes some Windows long path issues to)
- ⚠️ Changed argument -allnf to -allnotforked for better understanding

### Fixed
- Fixed a bug if destination folder is not specified, the tool deleted all folders in the root of the application folder. Now the folder is set to "Backups" in the root of the application folder if not set.
- Fixed if a repository is empty and -allbranches is set, the tool will not create the folder for the repository there was no data to backup (did that is -allbranches was not specifies)
- Fixed a bug where branches was not counted correctly if -allbranches was set and the repository was empty

## [1.2.0.0] - 15-03-2024

### Added
- Add logic to not start backup job tasks and console output if no repositories are found
- Add option to save/use a token from an encrypted file based on hardware (locked to machine)

## Changed
- Typo in about tool
- Update sample command
- Rename class from backups to LocalBackupsTasks
- Rename class from folders to LocalFolderTasks
- Fix typos in metadata classes for release and metadata for logging
- Typos in release .json downloader
- Upgraded some dependencies:
    - Autofac 7.1.0 -> 8.0.0
    - Microsoft.System.Diagnostics.DiagnosticSource 4.7.1 > 7.0.2
    - Microsoft.Windows.Compatibility 8.0.2 > 8.0.3

## [1.1.0.0] - 09-03-2024

### Added
- Added a new option to set days to keep of logs (default 30 days)
- Added new argument to download
    - Repository metadata (.json)
    - Repository releases data (.json)
    - **More to come...**
- Added more loggnig in the tool

### Changed
- Changed some typos
- Moved some code to classes
- Upgraded some dependencies:
    - Microsoft.Windows.Compatibility - 8.0.0 > 8.0.2
    - McMaster.Extensions.CommandLineUtils - 4.1.0 > 4.1.1
- Changed backup folder name to just the timestamp

### Fixed
- Fixed some bugs with the all branche backup function

## [1.0.0] - 05-03-2024

Initial release of the tool