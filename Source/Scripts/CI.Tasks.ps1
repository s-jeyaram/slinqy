# Include additional scripts
Include "CI.Settings.ps1"
Include "CI.Functions.ps1"
Include "CI.Azure.Functions.ps1"

# Define path parameters
$BasePath = "Uninitialized" # Caller must specify.

# Define the input properties and their default values.
properties {
    $ProductName           = "Slinqy"
    $SourcePath            = Join-Path $BasePath "Source"
    $ArtifactsPath         = Join-Path $BasePath "Artifacts"
    $LogsPath              = Join-Path $ArtifactsPath "Logs"
    $ScriptsPath           = Join-Path $ArtifactsPath "Scripts"
    $PublishedWebsitesPath = Join-Path $ArtifactsPath "_PublishedWebsites"
    $SolutionFileName      = "$ProductName.sln"
    $SolutionPath          = Join-Path $SourcePath $SolutionFileName
    $XUnitPath             = Join-Path $Env:ChocolateyInstall 'bin\xunit.console.exe'
}

# Define the Task to call when none was specified by the caller.
Task Default -depends Build

Task InstallDependencies -description "Installs all dependencies required to execute the tasks in this script." {
    exec { 
        cinst invokemsbuild --version 1.5.17 --confirm
        cinst xunit         --version 2.0.0  --confirm
    }
}

Task Clean -depends InstallDependencies -description "Removes any artifacts that may be present from prior runs of the CI script." {
    if (Test-Path $ArtifactsPath) {
        Write-Host "Deleting $ArtifactsPath..." -NoNewline
        Remove-Item $ArtifactsPath -Recurse -Force
        Write-Host "done!"
    }

    Write-Host "Cleaning solution $SolutionPath..." -NoNewline

    Invoke-MsBuild `
        -Path                  $SolutionPath `
        -MsBuildParameters "/t:Clean" `
            | Out-Null
    
    Write-Host "done!"
}

Task LoadSettings -description "Loads the environment specific settings." {
    # Search for a settings file
    $TemplateParametersFileName = 'environment-settings.json'
    $TemplateParametersFilePath = Join-Path $BasePath $TemplateParametersFileName
    
    $Script:Settings = Get-EnvironmentSettings `
        -ProductName      $ProductName `
        -SettingsFilePath $TemplateParametersFilePath
}

Task Build -depends Clean -description "Compiles all source code." {
    $BuildVersion = Get-BuildVersion

    exec { nuget restore $SolutionPath }

    Write-Host "Building $ProductName $BuildVersion from $SolutionPath"
    
    # Make sure the path exists, or the logs won't be written.
    New-Item `
        -ItemType Directory `
        -Path $LogsPath |
            Out-Null

    # Update the AssemblyInfo file with the version #.
    $AssemblyInfoFilePath = Join-Path $SourcePath 'AssemblyInfo.cs'
    Update-AssemblyInfoVersion `
        -Path    $AssemblyInfoFilePath `
        -Version $BuildVersion

    Write-Host "Compiling solution $SolutionPath..." -NoNewline

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

    Write-Host "done!"

    # Unit test the built code
    $TestDlls = @(
        (Join-Path $ArtifactsPath 'Slinqy.Core.Test.Unit.dll')
    )

    Write-Host "Unit testing $TestDlls..."

    $OpenCoverPath       = Join-Path $SourcePath 'packages\OpenCover.4.6.166\tools\OpenCover.Console.exe'
    $OpenCoverOutputPath = Join-Path $ArtifactsPath "coverage.xml"

    $currentDir = Get-Location
    Set-Location $ArtifactsPath
    exec { . $OpenCoverPath -target:$XUnitPath -targetargs:$TestDlls -register:user -output:$OpenCoverOutputPath -filter:"+[*]Slinqy.* -[*.Test.*]*" }
    Set-Location $currentDir

    $ReportGeneratorPath       = Join-Path $SourcePath 'packages\ReportGenerator.2.3.2.0\tools\ReportGenerator.exe'
    $ReportGeneratorOutputPath = Join-Path $ArtifactsPath 'CoverageReport'

    exec { . $ReportGeneratorPath $OpenCoverOutputPath $ReportGeneratorOutputPath }

    $CoverallsPath = Join-Path $SourcePath 'packages\coveralls.io.1.3.4\tools\coveralls.net.exe'

    exec { . $CoverallsPath --opencover $OpenCoverOutputPath }

    Write-Host "done!"

    Write-Host "Packaging..." -NoNewline

    # Package up deployables
    $WebProjectFileName = "ExampleApp.Web.csproj"
    $WebProjectPath     = Join-Path $SourcePath "ExampleApp.Web\$WebProjectFileName"
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

    $AzureSubscription = (Get-AzureSubscription -Current).SubscriptionName

    Write-Host "Provisioning $($Settings.EnvironmentName) ($($Settings.EnvironmentLocation)) in Azure Subscription $AzureSubscription..."

    # Set up paths to the Resource Manager template
    $TemplatesPath	  = Join-Path $ArtifactsPath "Templates"
    $TemplateFilePath = Join-Path $TemplatesPath "ExampleApp.json"

    Switch-AzureMode AzureResourceManager

    $templateParameters                     = @{}
    $templateParameters.environmentName     = $Settings.EnvironmentName
    $templateParameters.environmentLocation = $Settings.EnvironmentLocation
    $templateParameters.exampleAppSiteName  = $Settings.ExampleAppSiteName

    if (-not (Check-AzureResourceGroupExists $Settings.ResourceGroupName)) {
        Write-Host "Creating resource group $($Settings.ResourceGroupName)..." -NoNewline

        New-AzureResourceGroup `
            -Name			            $Settings.ResourceGroupName `
            -Location                   $Settings.EnvironmentLocation

        Write-Host "done!"
    }

    Write-Host "Updating resource group $($Settings.ResourceGroupName)..." -NoNewline

    $result = New-AzureResourceGroupDeployment `
        -ResourceGroupName			$Settings.ResourceGroupName `
        -TemplateParameterObject    $templateParameters `
        -TemplateFile               $TemplateFilePath `
        -Force

    Write-Host "done!"

    # Save connection strings locally.
    $environmentSecretsPath     = Join-Path $BasePath "environment-secrets.config"
    $serviceBusConnectionString = $result.Outputs["serviceBusConnectionString"].Value    
    
    Write-Host "Saving connection strings to $environmentSecretsPath..." -NoNewline

    Set-Content $environmentSecretsPath `
        -Value "<?xml version='1.0' encoding='utf-8'?>
<appSettings>
  <add 
    key=""Microsoft.ServiceBus.ConnectionString""
    value=""$serviceBusConnectionString""
  />
</appSettings>" `
        -Force

    Write-Host "done!"
    Write-Host
    Write-Host "Provisioning completed!"
}

Task Deploy -depends ProvisionEnvironment -description "Deploys artifacts from the last build that occurred to the target environment." {
    $ExampleAppPackagePath = Join-Path $PublishedWebsitesPath "ExampleApp.Web_Package\ExampleApp.Web.zip"

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

Task FunctionalTest -depends LoadSettings -description 'Tests that the required features and use cases are working in the target environment.' {
    Write-Host 'Getting Base URI...' -NoNewline

    $ExampleWebsiteHostName = (Get-AzureWebsite -Name $Settings.ExampleAppSiteName).HostNames[0]
    $ExampleWebsiteBaseUri = "http://$ExampleWebsiteHostName"

    Write-Host $ExampleWebsiteBaseUri

    ${env:ExampleApp.BaseUri} = $ExampleWebsiteBaseUri

    $TestDlls = @(
        (Join-Path $ArtifactsPath 'Slinqy.Test.Functional.dll')
    )
    
    Write-Host "Running tests in $TestDlls"

    exec { & $XUnitPath $TestDlls }
}

Task DestroyEnvironment -depends LoadSettings -description "Permanently deletes and removes all services and data from the target environment." {
    if (-not (Check-AzureResourceGroupExists $Settings.ResourceGroupName)) {
        Write-Host "The resource group $($Settings.ResourceGroupName) doesn't exist, nothing to delete..."
        return
    }

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

Task Push -depends Pull,Full -description "Performs pre-push actions before actually pushing to the remote repo." {
    exec { git push }
}

Task Full -depends DestroyEnvironment,Build,Deploy,FunctionalTest,DestroyEnvironment -description "Runs all pertinent CI tasks and cleans up afterward (destroys the environment)." {
}