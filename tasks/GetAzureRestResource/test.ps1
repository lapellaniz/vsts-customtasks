
$VerbosePreference = "Continue"

# Input 'MyInput':


# Endpoint:
#$env:INPUT_MYENDPOINT = 'EP1'
$env:ENDPOINT_URL_EP1 = 'http://test.com'
$env:ENDPOINT_AUTH_EP1 = '{ "Parameters": { "ServicePrincipalId": "", "ServicePrincipalKey": "", "TenantId": "" }, "Scheme": "ServicePrincipal" }'
$env:ENDPOINT_DATA_EP1 = '{ "SubscriptionId": "", "SubscriptionName": "" }'

$scriptPath = "$PSScriptRoot\main.ps1"
Import-Module $PSScriptRoot\ps_modules\VstsTaskSdk
Invoke-VstsTaskScript -ScriptBlock ([scriptblock]::Create(". $scriptPath"))