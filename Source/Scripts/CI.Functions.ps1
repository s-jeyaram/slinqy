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

function Assert-HttpResponseCode {
    Param(
        $GetUri,
        $ExpectedResponseCode
    )

    $response   = Invoke-WebRequest $GetUri -UseBasicParsing
    $statusCode = $response.StatusCode

    Write-Host "Response Code: $statusCode"

    if (-not ($statusCode -eq $ExpectedResponseCode)) {
        throw "Unexpected response: $response"
    }
}