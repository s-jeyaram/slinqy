# Define path parameters
$BasePath = "Uninitialized" # Caller must specify.

properties {
	$SourcePath    = Join-Path $BasePath "Source"
	$ArtifactsPath = Join-Path $BasePath "Artifacts"
	$LogsPath      = Join-Path $ArtifactsPath "Logs"
	$ScriptsPath   = Join-Path $ArtifactsPath "Scripts"
	$EnvLocation   = "West US"
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
	$SolutionPath = Join-Path $SourcePath "Slinqy.sln"

	Write-Host "Building $SolutionPath"
	
	# Make sure the path exists, or the logs won't be written.
	New-Item `
		-ItemType Directory `
		-Path $LogsPath |
			Out-Null

	$MsBuildSucceeded = Invoke-MsBuild `
		-Path $SolutionPath `
		-BuildLogDirectoryPath $LogsPath `
		-MsBuildParameters "/p:OutDir=$ArtifactsPath\" `
		-KeepBuildLogOnSuccessfulBuilds

	Write-Host "MsBuildSucceeded: $MsBuildSucceeded"
}

Task ProvisionEnvironment -description "Ensures the needed resources are set up in the target runtime environment." {
	$AzureResourceGroupScriptPath   = Join-Path $ScriptsPath   "Deploy-AzureResourceGroup.ps1"
	$TemplatesPath					= Join-Path $ArtifactsPath "Templates"
	$TemplateFilePath				= Join-Path $TemplatesPath "ExampleApp.json"
	$TemplateParametersFilePath		= Join-Path $TemplatesPath "ExampleApp.param.env.json"
	
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

Task Pull -description "Pulls the latest source from master to the local repo." {
	exec { git pull origin master }
}

Task Push -depends Pull,Build -description "Performs pre-push actions before actually pushing to the remote repo." {
	exec { git push }
}