Include "AppVeyor.Functions.ps1"

function Get-BuildVersion {
	$BuildVersion = Get-AppVeyorBuildVersion

	if (-not $BuildVersion) {
		$BuildVersion = "1.0.*"
	}

	Write-Output $BuildVersion
}