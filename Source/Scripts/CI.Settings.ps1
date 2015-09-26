class Settings {
	[String]         $ProductName;
	[String]         $EnvironmentName;
	[String]         $EnvironmentLocation;
	[String]         $ResourceGroupName;
	[String]         $ExampleAppName;
	[String]         $ExampleAppSiteName;
	[PSCustomObject] $SettingsFileContent = $null;

	Settings($settingsFilePath) {
		Write-Host "Loading settings from $settingsFilePath..."

		# Load the specified settings
		$this.settingsFileContent = Get-Content `
			-Path $settingsFilePath `
			-Raw | 
				ConvertFrom-Json

		# Load settings from either local file or environment variables.
		$params = $this.SettingsFileContent.parameters
		
		$this.ProductName         = if ($params.productName)         { $params.productName.value }         else { $this.GetSetting("ProductName",         "Slinqy")  }
		$this.EnvironmentName     = if ($params.environmentName)     { $params.environmentName.value }     else { $this.GetSetting("EnvironmentName",     "Dev")     }
		$this.EnvironmentLocation = if ($params.environmentLocation) { $params.environmentLocation.value } else { $this.GetSetting("EnvironmentLocation", "West US") }

		$this.ResourceGroupName	  = $this.EnvironmentName + '-' + $this.ProductName
		$this.ExampleAppName	  = $this.ProductName + '-ExampleApp'
		$this.ExampleAppSiteName  = $this.EnvironmentName + '-' + $this.ExampleAppName
	}

	[string] GetSetting(
		[String] $settingName, 
		[String] $defaultValue = $null) 
	{
		# Try to get the value from the local environment variables.
		$SettingValue = [System.Environment]::GetEnvironmentVariable($settingName)

		# Try to use the default value.
		$SettingValue = if (-not $SettingValue) { $defaultValue }

		if (-not $SettingValue) {
			# Ask console user
			$SettingValue = Read-Host -Prompt "What should the value be for setting ${settingName}?"
		} 

		return $SettingValue
	}

	[Hashtable] GetSettings() {
		$hash = New-Object Hashtable

		$hash.Add("environmentName",     $this.EnvironmentName)
		$hash.Add("environmentLocation", $this.EnvironmentLocation)
		$hash.Add("exampleAppSiteName",  $this.ExampleAppSiteName)

		return $hash
	}
}