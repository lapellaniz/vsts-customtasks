[CmdletBinding()]
param()

$ScriptPath = $(Split-Path -Parent $MyInvocation.MyCommand.Definition) 
Import-Module $(Join-Path $ScriptPath "ps_modules\VstsTaskSdk\VstsTaskSdk.psd1")

Trace-VstsEnteringInvocation $MyInvocation
try {
    ## Collect input values
    $connectedServiceName = (Get-VstsInput -Name "connectedServiceName").Trim()    
    $resourceGroupName = (Get-VstsInput -Name "resourceGroupName").Trim()
    $serviceEndpoint = Get-VstsEndpoint -Name "$connectedServiceName"    
    $outputVariablesInput = Get-VstsInput -Name "outputVariables"

    ## Log inputs
    Write-Verbose "outputVariables: $outputVariablesInput"
    Write-Verbose "Auth Scheme: $($serviceEndpoint.Auth.Scheme)"
    Write-Verbose "SubscriptionId: $($serviceEndpoint.Data.SubscriptionId)"

    if ($serviceEndpoint.Auth.Scheme -ne "ServicePrincipal")
    {
        Write-Error "Only Service Principal connection is supported."
    }

    ## Local variables
    $spnId = $serviceEndpoint.Auth.Parameters.ServicePrincipalId
    $spnSecret = $serviceEndpoint.Auth.Parameters.ServicePrincipalKey
    $tenantId = $serviceEndpoint.Auth.Parameters.TenantId
    $subscriptionId = $serviceEndpoint.Data.SubscriptionId
    $outputVariables = $outputVariablesInput | ConvertFrom-Json

    ## Login to Azure
    $secpasswd = ConvertTo-SecureString $spnSecret -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential ($spnId, $secpasswd)
    Login-AzureRmAccount -ServicePrincipal -Tenant $tenantId -Credential $cred
    Select-AzureRmSubscription -SubscriptionId $subscriptionId

    ## Get the most recent deployment for the resource group
    $lastDeployment = Get-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName | Sort-Object Timestamp -Descending | Select-Object -First 1  
    
    if(!$lastDeployment)
    {
        Write-Error "Resource Group Deployment could not be found for '$resourceGroupName'."
    }

    $deploymentOutputParameters = $lastDeployment.Outputs

    if(!$deploymentOutputParameters)
    {
        Write-Error "No output parameters could be found for the last deployment of '$resourceGroupName'."
    }
    if($deploymentOutputParameters.Count -eq 0)
    {
        Write-Error "No output parameters could be found for the last deployment of '$resourceGroupName'."
    }

    ## iterate through the output variables and set the VSO variables
    ## TODO : check for property that is missing. This assumes the data returned is JSON.
    foreach ($group in ($outputVariables.PSObject.Members | Where-Object{ $_.MemberType -eq 'NoteProperty'})) {        
        $rgDeploymentOutputParameterName = $group.Name        
        $variableName = $group.Value

        $outputParameter = $deploymentOutputParameters.Item($rgDeploymentOutputParameterName)
        if(!$outputParameter)
        {
            throw "No output parameter could be found with the name of '$rgDeploymentOutputParameterName'."
        }
        $outputParameterValue  = $outputParameter.Value
        Write-Host ("##vso[task.setvariable variable=$variableName;]$outputParameterValue")        
    }

} finally {
    Trace-VstsLeavingInvocation $MyInvocation
}
