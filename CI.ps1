#########################
# 1. INSTALL DEPENDENCIES
#########################

# Chocolately: Used to install subsequent dependencies.
if (-not ($env:Path -ilike "*chocolatey*")) {
    Write-Host "Chocolatey not found in PATH environment variable, installing..."
    iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}

# Psake: Used to run the CI scripts.
cinst psake --version 4.4.1 --confirm

#########################
# 2. EXECUTE CI SCRIPT(S)
#########################

# Then run the actual CI script using psake
psake .\Source\Scripts\CI.Core.ps1 $args -properties "@{BasePath='$PSScriptRoot'}"