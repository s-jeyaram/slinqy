# Define path parameters
$BasePath = "Uninitialized" # Caller must specify.

properties {
	$SourcePath    = Join-Path $BasePath "Source"
	$ArtifactsPath = Join-Path $BasePath "Artifacts"
	$LogsPath      = Join-Path $ArtifactsPath "Logs"
}

# Define the Task to call when none was specified by the caller.
Task Default -depends Build

Task Clean -description "Removes any artifacts that may be present from prior runs of the CI script." {
	if (Test-Path $ArtifactsPath) {
		Remove-Item $ArtifactsPath -Recurse -Force
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
		-KeepBuildLogOnSuccessfulBuilds

	Write-Host "MsBuildSucceeded: $MsBuildSucceeded"
}

Task Pull -description "Pulls the latest source from master to the local repo." {
	exec { git pull origin master }
}

Task Push -depends Pull,Build -description "Performs pre-push actions before actually pushing to the remote repo." {
	exec { git push }
}