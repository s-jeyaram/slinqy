# Define path parameters
$BasePath = "Uninitialized" # Caller must specify.

properties {
	$EnvName               = $null # Caller must specify.
	$EnvLocation           = "West US"
	$ProductName           = "Slinqy"
	$ExampleAppName        = "ExampleApp"
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
	}
}

Task Build -depends Clean,InstallDependencies -description "Compiles all source code." {
	$SolutionFileName = "$ProductName.sln"
	$SolutionPath     = Join-Path $SourcePath $SolutionFileName

	Write-Host "Building $SolutionPath"
	
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
	$WebProjectFileName = "$ExampleAppName.csproj"
	$WebProjectPath     = Join-Path $SourcePath "$ExampleAppName\$WebProjectFileName"
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

Task ProvisionEnvironment -description "Ensures the needed resources are set up in the target runtime environment." {
	$AzureResourceGroupScriptPath   = Join-Path $ScriptsPath   "Deploy-AzureResourceGroup.ps1"
	$TemplatesPath					= Join-Path $ArtifactsPath "Templates"
	$TemplateFilePath				= Join-Path $TemplatesPath "$ExampleAppName.json"
	$TemplateParametersFilePath		= Join-Path $TemplatesPath "$ExampleAppName.param.env.json"
	
	Write-Host "Provisioning Location: $EnvLocation"

	# WARNING: 
	#	If the following call fails, the error doesn't bubble up and the build script will continue on. :(
	#	But the impact of this occurring should be minimal.  The script is likely to fail in subsequent 
	#	tasks if changes have been made to the template files and they fail to be applied.

	# TODO: Find a way to end the script if this call fails
	. $AzureResourceGroupScriptPath `
		-ResourceGroupLocation  $EnvLocation `
		-TemplateFile           $TemplateFilePath `
		-TemplateParametersFile $TemplateParametersFilePath
}

Task Deploy -depends ProvisionEnvironment -description "Deploys artifacts from the last build that occurred to the target environment." {
	$ExampleAppWebsiteName = "$ProductName$ExampleAppName"
	$ExampleAppPackagePath = Join-Path $PublishedWebsitesPath "${ExampleAppName}_Package\$ExampleAppName.zip"

	Switch-AzureMode AzureServiceManagement

	Write-Host "Deploying $ExampleAppPackagePath to $ExampleAppWebsiteName..." -NoNewline

	Publish-AzureWebsiteProject `
		-Package $ExampleAppPackagePath `
		-Name    $ExampleAppWebsiteName

	Write-Host "done!"
}

Task Pull -description "Pulls the latest source from master to the local repo." {
	exec { git pull origin master }
}

Task Push -depends Pull,Build -description "Performs pre-push actions before actually pushing to the remote repo." {
	exec { git push }
}