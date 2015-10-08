function Get-EnvironmentSettings {
	Param(
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
		$params = New-Object PSCustomObject
	}

	$hash.ProductName         = if ($params.productName)         { $params.productName.value }         else { Get-Setting "ProductName"         "Slinqy"  }
	$hash.EnvironmentName     = if ($params.environmentName)     { $params.environmentName.value }     else { Get-Setting "EnvironmentName"     "Dev" }
	$hash.EnvironmentLocation = if ($params.environmentLocation) { $params.environmentLocation.value } else { Get-Setting "EnvironmentLocation" "West US" }

	$hash.ResourceGroupName	  = $hash.EnvironmentName + '-' + $hash.ProductName
	$hash.ExampleAppName	  = $hash.ProductName + '-ExampleApp'
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
		# Ask console user
		$SettingValue = Read-Host -Prompt "What should the value be for setting '${settingName}'?"
	} 

	return $SettingValue
}