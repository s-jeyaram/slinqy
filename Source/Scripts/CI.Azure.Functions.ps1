# Ensure the Azure PowerShell cmdlets are available
Import-Module Azure -Force | Out-Null

function Check-AzureResourceGroupExists {
    Param(
        $ResourceGroupName
    )

    Switch-AzureMode AzureResourceManager
    $resourceGroups = Get-AzureResourceGroup
    $appGroup = $resourceGroups | where {$_.ResourceGroupName -eq $ResourceGroupName}

    if ($appGroup) {
        Write-Output $True
    }
}