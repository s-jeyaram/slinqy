#requires -RunAsAdministrator

# Make sure all the system level prerequisites that require Admin rights are installed.

# Chocolately: Used to install subsequent dependencies.
if (-not ($env:Path -ilike "*chocolatey*")) {
    Write-Host "Chocolatey not found in PATH environment variable, installing..."
    iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}

# PowerShell is used to run scripts.
cinst powershell --version 5.0.10514-ProductionPreview --confirm

# .NET 4.6 is the Framework that compiles and runs compiled code.
cinst dotnet4.6  --version 4.6.00081.20150925          --confirm

# PSake is a tool that coordinates CI tasks.
cinst psake      --version 4.4.1                       --confirm