param (
    [string]$OutputFolderPath
)

# Remove any double quotes from the provided $OutputFolderPath parameter
$OutputFolderPath = $OutputFolderPath -replace '"', ''

# Function to handle both .exe and .dll files
function ManageBuildFiles($fileExtension) {
    # Folder for old builds
    $FolderName = Join-Path -Path $OutputFolderPath -ChildPath "Old"
    if (Test-Path -Path $FolderName -PathType Container) {   
        Write-Host "Old folder for builds exists"
        Get-ChildItem -Path "$OutputFolderPath\*GitHubBackupTool*Build at*.$fileExtension" -Recurse | Move-Item -Destination $FolderName
        Write-Host "Moved files $OutputFolderPath\*GitHubBackupTool*Build at*.$fileExtension to $FolderName"
    } else {
        Write-Host "Old folder for Release builds doesn't exist - Creating it..."
        # PowerShell Create directory if not exists
        New-Item -Path $FolderName -ItemType Directory
        Write-Host "Old folder for Release builds doesn't exist - Created..."
    }

    # Delete old .exe or .dll files that are not needed anymore (2 seconds old or more)
    Get-ChildItem -Path "$OutputFolderPath\*GitHubBackupTool*Build at*.$fileExtension" -File | Where-Object CreationTime -lt (Get-Date).AddSeconds(-2) | Remove-Item -Force

    # Get the file version for the last build of GitHubBackupTool.exe or GitHubBackupTool.dll
    $FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$OutputFolderPath\GitHubBackupTool.$fileExtension").FileVersion

    # Rename the file to include version and build time (keep original)
    Get-ChildItem -Path "$OutputFolderPath\GitHubBackupTool.$fileExtension" | Where-Object {!$_.PSIsContainer -and $_.Extension -eq ".$fileExtension"} | ForEach-Object {
        $NewFileName = "{0} v. {1} - Build at {2}{3}" -f $_.BaseName, $FileVersion, (Get-Date -Format "ddMMyyyy-HHmmss"), $_.Extension
        $NewFilePath = Join-Path -Path $OutputFolderPath -ChildPath $NewFileName
        Copy-Item -Path $_.FullName -Destination $NewFilePath -Force
        Write-Host "Copied" $_.FullName "to $NewFilePath"
    }
}

# Manage both .exe and .dll files
ManageBuildFiles "exe"
ManageBuildFiles "dll"