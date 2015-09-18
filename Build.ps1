# Install Chocolately if it's not already
if (-not ($env:Path -ilike "*chocolatey*")) {
    Write-Host "Chocolatey not found in PATH environment variable, installing..."
    
    iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}