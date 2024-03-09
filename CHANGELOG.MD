## [x.x.x.x] - xx-xx-xxxx - DEV

### Added
- Add logic to not start backup job tasks and console output if no repositories are found


## [1.1.0.0] - 09-03-2024

### Added
- Added a new option to set days to keep of logs (default 30 days)
- Added new argument to download
    - repository metadata (.json)
    - repository releases data (.json)
    - More to come...
- Added more loggnig in the tool

### Changed
- Changed some typos
- Moved some code to classes
- Upgraded some dependencies
    - Microsoft.Windows.Compatibility - 8.0.0 > 8.0.2
    - McMaster.Extensions.CommandLineUtils - 4.1.0 > 4.1.1
- Changed backup folder name to just the timestamp

### Fixed
- Fixed some bugs with the all branche backup function

## [1.0.0] - 05-03-2024

Initial release of the tool