# Ensure the Azure PowerShell cmdlets are available
Import-Module Azure -Force | Out-Null

function Check-AzureResourceGroupExists {
    Param(
        $ResourceGroupName
    )

    $resourceGroups = Get-AzureRmResourceGroup
    $appGroup = $resourceGroups | where {$_.ResourceGroupName -eq $ResourceGroupName}

    if ($appGroup) {
        Write-Output $True
    }
}

# Gets the existing context, or logs in with the specified credentials.
function GetOrLogin-AzureRmContext {
    Param(
        $AzureDeployUser,
        $AzureDeployPass
    )

    try {
        $context = Get-AzureRmContext
    }
    catch
    {
        if ($_.Exception.Message -ne 'Run Login-AzureRmAccount to login.') {
            throw
        }

        if ($AzureDeployUser -and $AzureDeployPass) {
            $AzureDeployPassSecure = $AzureDeployPass | ConvertTo-SecureString -AsPlainText -Force
            $AzureCredential       = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $AzureDeployUser,$AzureDeployPassSecure

            $context = Login-AzureRmAccount -Credential $AzureCredential
        } else {
            Write-Host "Could not find any Azure credentials on the local machine, prompting console user..."
            # Prompt the console user for credentials
            $context = Login-AzureRmAccount
        }

        if (-not $context){
            throw 'No Azure account found or specified!'
        }
    }

    Write-Output $context
}

function Upload-FileToBlob {
    Param(
        $storageContext,
        $localFilePath,
        $containerName
    )

    $containers = Get-AzureStorageContainer `
        -Context $storageContext

    $container =  $containers | Where-Object {
        $_.Name -eq $containerName
    }

    if (-not $container) {
        $container = New-AzureStorageContainer `
            -Name       $containerName `
            -Context    $storageContext `
            -Permission Off
    }

    $fileName = Split-Path `
        -Path $localFilePath `
        -Leaf

    Set-AzureStorageBlobContent `
        -File      $localFilePath `
        -Container $containerName `
        -Blob      $fileName `
        -Context   $storageContext `
        -Force |
            Out-Null

    Write-Output $container.CloudBlobContainer.Uri.ToString() + '/' + $fileName
}