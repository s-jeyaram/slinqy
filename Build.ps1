# Install Chocolately if it's not already
if (-not ($env:Path -ilike "*chocolatey*")) {
    Write-Host "Chocolatey not found in PATH environment variable, installing..."
    
    iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}

# Make sure all other required dependencies are installed...
cinst psake --version 4.4.1 --confirm
