Include "AppVeyor.Functions.ps1"

function Get-BuildVersion {
    $BuildVersion = Get-AppVeyorBuildVersion

    if (-not $BuildVersion) {
        $BuildVersion = "0.0.0.0"
    }

    Write-Output $BuildVersion
}

function Update-AssemblyInfoVersion {
    Param(
        $Path,
        $Version
    )

    Write-Host "Updating $Path with $Version..." -NoNewline

    $AssemblyInfoContent = Get-Content $Path -Encoding UTF8

    $AssemblyInfoContent = $AssemblyInfoContent -replace 'AssemblyVersion\(".*"\)', "AssemblyVersion(""$Version"")"

    Set-Content $Path $AssemblyInfoContent -Encoding UTF8

    Write-Host "done!"
}

function Assert-HttpStatusCode {
    Param(
        $GetUri,
        $ExpectedStatusCode
    )

    $response   = Invoke-WebRequest $GetUri -UseBasicParsing
    $statusCode = $response.StatusCode
    
    if ($statusCode -ne $ExpectedStatusCode) {
        throw "Unexpected status code: $response"
    }
}

function Write-EnvironmentSecrets {
    Param(
        $SecretsPath,
        $ServiceBusConnectionString
    )

    Write-Host "Saving secrets to $SecretsPath..." -NoNewline

    Set-Content $SecretsPath `
        -Value "<?xml version='1.0' encoding='utf-8'?>
<appSettings>
  <add 
    key=""Microsoft.ServiceBus.ConnectionString""
    value=""$ServiceBusConnectionString""
  />
</appSettings>" `
        -Force

    Write-Host "done!"
}