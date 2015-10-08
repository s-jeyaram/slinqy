#requires -RunAsAdministrator

# Make sure all the system level prerequisites that require Admin rights are installed.

# Chocolately: Used to install subsequent dependencies.
if (-not ($env:Path -ilike "*chocolatey*")) {
    Write-Host "Chocolatey not found in PATH environment variable, installing..."
    iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}

# .NET 4.6 is the Framework that compiles and runs compiled code.
cinst dotnet4.6         --version 4.6.00081.20150925          --confirm

# Used to install .net packages.
cinst nuget.commandline --version 2.8.6                       --confirm

# PSake is a tool that coordinates CI tasks.
cinst psake             --version 4.4.1                       --confirm