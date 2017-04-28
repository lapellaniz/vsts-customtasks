[CmdletBinding()]
param()

$ScriptPath = $(Split-Path -Parent $MyInvocation.MyCommand.Definition) 
Import-Module $(Join-Path $ScriptPath "ps_modules\VstsTaskSdk\VstsTaskSdk.psd1")
Import-Module $(Join-Path $ScriptPath "lib\PowerShell.Azure.Rest.dll")

Trace-VstsEnteringInvocation $MyInvocation
try {
    ## Collect input values
    $connectedServiceName = (Get-VstsInput -Name "connectedServiceName").Trim()
    $resourceUri = (Get-VstsInput -Name "resourceProviderUri").Trim()
    $resourceGroupName = (Get-VstsInput -Name "resourceGroupName").Trim()
    $serviceEndpoint = Get-VstsEndpoint -Name "$connectedServiceName"
    $httpMethod = Get-VstsInput -Name "httpMethod"
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

    ## Initialize context
    $context = New-AzureResourceContext -ServicePrincipalId $spnId  -ServicePrincipalKey $spnSecret -TenantId $tenantId -SubscriptionId $subscriptionId

    ## Get resource
    $resource = Get-AzureRestResource -Context $context -ResourceProviderUri $resourceUri -ResourceGroupName $resourceGroupName -HttpMethod $httpMethod

    ## iterate through the output variables and set the VSO variables
    ## TODO : check for property that is missing. This assumes the data returned is JSON.
    foreach ($group in ($outputVariables.PSObject.Members | Where-Object{ $_.MemberType -eq 'NoteProperty'})) {        
        $propertyName = $group.Name        
        $variableName = $group.Value        
        $value = $resource[$propertyName]
        Write-Host ("##vso[task.setvariable variable=$variableName;]$value")        
    }

} finally {
    Trace-VstsLeavingInvocation $MyInvocation
}
