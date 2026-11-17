# TO-DO

## High priority
- Add pre-flight validation before backup starts:
  - Validate token scope/lifetime
  - Validate destination path reachability (including UNC)
  - Validate write permissions
  - Validate Git availability (if fallback clone is used)
- Add retry + backoff for transient GitHub API errors:
  - Rate limits
  - HTTP 5xx
  - Network/transient failures

## Medium priority
- Improve long-path handling on Windows/UNC:
  - Optional short path mode (hash/shorten deep branch paths)
  - Better diagnostics when path length limits are hit
- Add configurable clone concurrency (`-maxparallel`) to reduce network/share contention
- Expand owner/org filtering:
  - Multiple owners (`-owner a,b,c`)
  - Organization include/exclude filters

## Reporting and reliability
- Standardize failure categories and exit codes:
  - Auth
  - Network
  - Path/IO
  - Policy/compliance
- Include failure category in email report and logfile summary
- Add structured summary output (JSON/CSV) for automation/pipelines

## Feature completeness
- Complete metadata backup areas that are still partial/disabled:
  - Issues metadata
  - Review comments metadata

## Testing
- Add automated tests for CLI filtering combinations:
  - `-all`
  - `-allnotforked`
  - `-allowner`
  - `-owner`
  - Combined argument scenarios
- Add tests for failure paths and fallback logic
