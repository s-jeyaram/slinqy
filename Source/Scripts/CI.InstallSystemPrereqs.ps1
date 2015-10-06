#requires -RunAsAdministrator

# Make sure all the system level prerequisites that require Admin rights are installed
# Chocolately: Used to install subsequent dependencies.
if (-not ($env:Path -ilike "*chocolatey*")) {
    Write-Host "Chocolatey not found in PATH environment variable, installing..."
    iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}

cinst powershell --version '5.0.10514-ProductionPreview' --confirm
cinst psake      --version 4.4.1 --confirm