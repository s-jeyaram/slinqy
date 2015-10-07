# Include additional scripts
Include "CI.Settings.ps1"
Include "CI.Functions.ps1"

# Define path parameters
$BasePath = "Uninitialized" # Caller must specify.

# Define the input properties and their default values.
properties {
	$SourcePath            = Join-Path $BasePath "Source"
	$ArtifactsPath         = Join-Path $BasePath "Artifacts"
	$LogsPath              = Join-Path $ArtifactsPath "Logs"
	$ScriptsPath           = Join-Path $ArtifactsPath "Scripts"
	$PublishedWebsitesPath = Join-Path $ArtifactsPath "_PublishedWebsites"
}

# Define the Task to call when none was specified by the caller.
Task Default -depends Build

Task Clean -description "Removes any artifacts that may be present from prior runs of the CI script." {
	if (Test-Path $ArtifactsPath) {
		Write-Host "Deleting $ArtifactsPath..." -NoNewline
		Remove-Item $ArtifactsPath -Recurse -Force
		Write-Host "done!"
	}
}

Task InstallDependencies -description "Installs all dependencies required to execute the tasks in this script." {
	exec { 
		cinst invokemsbuild --version 1.5.17 --confirm
		cinst xunit         --version 2.0.0  --confirm
	}
}

Task LoadSettings -description "Loads the environment specific settings." {
	# Search for a settings file
	$TemplateParametersFileName = 'environment-settings.json'
	$TemplateParametersFilePath = Join-Path $BasePath $TemplateParametersFileName
	
	$Script:Settings = [Settings]::new($TemplateParametersFilePath)
}

Task Build -depends Clean,InstallDependencies,LoadSettings -description "Compiles all source code." {
	$SolutionFileName = "$($Settings.ProductName).sln"
	$SolutionPath     = Join-Path $SourcePath $SolutionFileName

	$BuildVersion = Get-BuildVersion

	Write-Host "Building $($Settings.ProductName) $BuildVersion from $SolutionPath"
	
	# Make sure the path exists, or the logs won't be written.
	New-Item `
		-ItemType Directory `
		-Path $LogsPath |
			Out-Null

	# Compile the whole solution according to how the solution file is configured.
	$MsBuildSucceeded = Invoke-MsBuild `
		-Path                  $SolutionPath `
		-BuildLogDirectoryPath $LogsPath `
		-MsBuildParameters     "/p:OutDir=$ArtifactsPath\" `
		-KeepBuildLogOnSuccessfulBuilds

	if (-not $MsBuildSucceeded) {
		$BuildFilePath = Join-Path $LogsPath "$SolutionFileName.msbuild.log"
		Get-Content $BuildFilePath
		throw "Build Failed!"
	}

	Write-Host "Compilation completed, packaging..." -NoNewline

	# Package up deployables
	$WebProjectFileName = "ExampleApp.csproj"
	$WebProjectPath     = Join-Path $SourcePath "ExampleApp\$WebProjectFileName"
	$MsBuildSucceeded   = Invoke-MsBuild `
		-Path                  $WebProjectPath `
		-BuildLogDirectoryPath $LogsPath `
		-MsBuildParameters     "/p:OutDir=$ArtifactsPath\ /t:Package" `
		-KeepBuildLogOnSuccessfulBuilds

	if (-not $MsBuildSucceeded) {
		$BuildFilePath = Join-Path $LogsPath "$WebProjectFileName.msbuild.log"
		Get-Content $BuildFilePath
		throw "Build Failed!"
	}

	Write-Host "done!"
}

Task ProvisionEnvironment -depends LoadSettings -description "Ensures the needed resources are set up in the target runtime environment." {
	# Ensure the Azure PowerShell cmdlets are available
	Import-Module Azure

	Write-Host "Provisioning $($Settings.EnvironmentName) ($($Settings.EnvironmentLocation))..."

	# First, make sure some Azure credentials are loaded
	$AzureAccount = Get-AzureAccount

	if (-not $AzureAccount) {
		$AzureSubscription = Get-AzureSubscription -Current -ErrorAction SilentlyContinue

		if (-not $AzureSubscription) {
			# Check environment variables for credentials
			$AzureDeployUser = $env:AzureDeployUser
			$AzureDeployPass = $env:AzureDeployPass

			if ($AzureDeployUser -and $AzureDeployPass) {
				$AzureDeployPassSecure = $AzureDeployPass | ConvertTo-SecureString -AsPlainText -Force
				$AzureCredential       = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $AzureDeployUser,$AzureDeployPassSecure

				$Account = Add-AzureAccount -Credential $AzureCredential
			} else {
				Write-Host "Could not find any Azure credentials on the local machine, prompting console user..."
				# Prompt the console user for credentials
				$Account = Add-AzureAccount
			}

			if (-not $Account){
				throw 'No Azure account found or specified!'
			}
		}
	}

	# Set up paths to the Resource Manager template
	$TemplatesPath	  = Join-Path $ArtifactsPath "Templates"
	$TemplateFilePath = Join-Path $TemplatesPath "ExampleApp.json"

	Switch-AzureMode AzureResourceManager

	# WARNING:
	#	If the following call fails, the error doesn't bubble up and the build script will continue on. :(
	#	But the impact of this occurring should be minimal.  The script is likely to fail in subsequent 
	#	tasks if changes have been made to the template files and they fail to be applied.

	# TODO: Find a way to end the script if this call fails
	New-AzureResourceGroup `
		-Name			            $Settings.ResourceGroupName `
		-Location                   $Settings.EnvironmentLocation `
		-TemplateParameterObject    $Settings.GetSettings() `
		-TemplateFile               $TemplateFilePath `
		-Force
}

Task Deploy -depends ProvisionEnvironment -description "Deploys artifacts from the last build that occurred to the target environment." {
	$ExampleAppPackagePath = Join-Path $PublishedWebsitesPath "ExampleApp_Package\ExampleApp.zip"

	Write-Host "Deploying $ExampleAppPackagePath to $($Settings.ExampleAppSiteName)..."

	Switch-AzureMode AzureServiceManagement
	Publish-AzureWebsiteProject `
		-Package $ExampleAppPackagePath `
		-Name    $Settings.ExampleAppSiteName

	Write-Host "done!"

	# Hit the Example App website to make sure it's alive
	$ExampleWebsiteHostName = (Get-AzureWebsite -Name $Settings.ExampleAppSiteName).HostNames[0]

	Write-Host "Checking $ExampleWebsiteHostName..." -NoNewline

	$Response = Invoke-WebRequest $ExampleWebsiteHostName -UseBasicParsing
	$StatusCode = $Response.StatusCode

	Write-Host "Response Code: $StatusCode"

	if (-not ($StatusCode -eq 200)) {
		throw "Unexpected response: $Response"
	}

	Write-Host "Deployment Completed"
}

Task FunctionalTest -description 'Tests that the required features and use cases are working in the target environment.' {
	$TestDlls = @(
		(Join-Path $ArtifactsPath 'ExampleApp.Test.Functional.dll')
	)
	
	Write-Host "Running tests in $TestDlls"

	exec { xunit.console $TestDlls }
}

Task DestroyEnvironment -depends LoadSettings -description "Permanently deletes and removes all services and data from the target environment." {
	$answer = Read-Host `
		-Prompt "Are you use you want to permanently delete all services and data from the target environment $($Settings.EnvironmentName)? (y/n)"

	if ($answer -eq 'y'){
		Switch-AzureMode AzureResourceManager
		Remove-AzureResourceGroup `
			-Name $Settings.ResourceGroupName `
			-Force
	}
}

Task Pull -description "Pulls the latest source from master to the local repo." {
	exec { git pull origin master }
}

Task Push -depends Pull,Build,Deploy,FunctionalTest -description "Performs pre-push actions before actually pushing to the remote repo." {
	exec { git push }
}