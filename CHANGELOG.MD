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