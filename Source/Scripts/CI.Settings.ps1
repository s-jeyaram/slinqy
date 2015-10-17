function Get-EnvironmentSettings {
	Param(
		[Parameter(Mandatory=$true)]
		[String]
		$ProductName,
		[Parameter(Mandatory=$true)]
		[String]
		$SettingsFilePath
	)
	# Create the Hashtable
	$hash = @{}

	# Populate it
	if (Test-Path $settingsFilePath) {
		Write-Host "Loading settings from $settingsFilePath..."

		# Load the specified settings
		$SettingsFileContent = Get-Content `
			-Path $settingsFilePath `
			-Raw | 
				ConvertFrom-Json

		$params = $SettingsFileContent.parameters
	} else {
		Write-Host "Loading settings from environment variables..."
		$params = New-Object PSCustomObject
	}

	# Build the hash values from a combination of sources.
	$hash.EnvironmentName     = if ($params.environmentName)     { $params.environmentName.value }     else { Get-Setting "EnvironmentName" }
	$hash.EnvironmentLocation = if ($params.environmentLocation) { $params.environmentLocation.value } else { Get-Setting "EnvironmentLocation" }

	# Autogenerate some setting values.
	$hash.ResourceGroupName	  = $hash.EnvironmentName + '-' + $ProductName
	$hash.ExampleAppName	  = $ProductName + '-ExampleApp'
	$hash.ExampleAppSiteName  = $hash.EnvironmentName + '-' + $hash.ExampleAppName

	Write-Output $hash
}

function Get-Setting(
	[String] $settingName, 
	[String] $defaultValue = $null) 
{
	# Try to get the value from the local environment variables.
	$SettingValue = [System.Environment]::GetEnvironmentVariable($settingName)

	# Try to use the default value.
	$SettingValue = if (-not $SettingValue) { $defaultValue }

	if (-not $SettingValue) {
		$error = "Could not find a value for '$settingName'.  Add a value for this setting either in your environment variables or your environment settings file then retry your command again."

		throw $error
	} 

	return $SettingValue.ToString()
}