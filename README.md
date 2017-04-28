# Overview
In DevOps we find ourselves writing lots of small scripts over and over. The need for building a library of tasks in your choice of tooling is important. This repository contains custom VSTS tasks using PowerShell and C#.

## Task Groups
The 1st reusable approach in VSTS for sharing your tasks is to leverage Task Groups. This is essentially a way to combine tasks into a unique group.

Task Groups can be used for build or release definitions. These are easily created by selecting one or more tasks, right click, and select *Create task group*.

### Limitations

1. Lack of history **NOTE: seems Microsoft has now added this**
2. Versioning support
3. Parameters are required

## Custom Tasks
Custom tasks allows us to encapsulate logic. They are packaged into a command written in PowerShell or Node.js and defined by a JSON manifest.

### Setup

* Install CLI - ```npm install -g tfx-cli```
* Login - ```tfx login```
    * Enter the collection url: ```https://{companyName}.visualstudio.com/DefaultCollection```
    * Enter PAT: https://www.visualstudio.com/en-us/docs/setup-admin/team-services/use-personal-access-tokens-to-authenticate
* Download Task SDK - ```Save-Module -Name VstsTaskSdk -Path .\```

### SDK
One can write VSTS tasks as a simple PowerShell or Node.js script without many dependencies. Microsoft has an SDK that allows for a better integration with VSTS. THe SDK provides a consistent approach for testing custom tasks. One main advantage is being able to access VSTS service endpoint programmatically.

The task SDK is statically linked to your custom task. One references the SDK in their command script. Add the SDK under the folder ```ps_modules\VstsTaskSdk``` in the task directory.

```powershell
$ScriptPath = $(Split-Path -Parent $MyInvocation.MyCommand.Definition) 
Import-Module $(Join-Path $ScriptPath "ps_modules\VstsTaskSdk\VstsTaskSdk.psd1")
```

### Scaffold
Create a new task using the command ```tfx build tasks create```. This will provide you with a manifest, icon, & empty PowerShell script.

![Img1]

### Inputs
Inputs map to the parameters for the custom task. These can provide a default value and some level of intelligence using groups, conditional visibility, and validation.

1. string
2. boolean
3. picList
4. filePath
5. multiline
6. radio
7. connectedService:ServiceType
8. connectedService:AzureRM
9. connectedService:Azure
10. connectedService:Azure:Certificate,UsernamePassword
11. connectedService:Chef
12. connectedService:ssh
13. connectedService:Generic
14. connectedService:Jenkins
15. connectedService:servicefabric

#### Groups
Inputs can be grouped into logical visual containers. These are rendered as a collapsible fieldset.

```json
"groups": [    
    {
      "name": "advanced
      "displayName": "Advanced
      "isExpanded": false
    }
],
"inputs": [
    {
      "name": "manageMode
      "type": "pickList
      "label": "Management Mode
      "defaultValue": "Verify
      "required": true,
      "groupName": "advanced
      "helpMarkDown": "Verify or Deploy routes.
      "properties": {
        "EditableOptions": "True"
      },
      "options": {
        "Verify": "Verify
        "Deploy": "Deploy"
      }
    },
    {
      "name": "enabled
      "type": "pickList
      "label": "Enabled
      "defaultValue": "false
      "required": true,
      "groupName": "advanced
      "helpMarkDown": "Specifies if the step will run. Requires 'Control Options | Enabled' to be set as well.
      "properties": {
        "EditableOptions": "True"
      },
      "options": {
        "false": false,
        "true": true
      }
    }
],
```

![Img2]

### Upload
When the task is ready to be run from VSTS, use the tfx-cli to upload the task. Every file under the task folder will be included so keep this in mind.

```tfx build tasks upload --task.path ./CustomTask --overwrite```

### Testing
Using the SDK, one can mock the parameters to the custom task. This includes being able to mock VSTS service endpoints.

```powershell
$env:ENDPOINT_URL_EP1 = 'http://test.com'
$env:ENDPOINT_AUTH_EP1 = '{ "Parameters": { "ServicePrincipalId": " "ServicePrincipalKey": " "TenantId": "" }, "Scheme": "ServicePrincipal" }'
$env:ENDPOINT_DATA_EP1 = '{ "SubscriptionId": " "SubscriptionName": "" }'
```

### Versioning
Versioning follows SemVer and is critical when making changes that may affect existing builds or releases using this task. VSTS will download the latest Minor and Patch changes when a new build or release is queued that uses that task. One is forced to manually upgrade to the latest Major change.

```json
"version": {
    "Major": "2",
    "Minor": "0",
    "Patch": "0"
},
```

### Tips

#### Tip #1 - Add verbose messages
Write-Host and Write-Verbose are your friend. 

```
An error has occurred running task...
```

Setting the build or release variable ```system.debug``` to *true* will output verbose logs.

#### Tip #2 - Test the script locally
It takes a while to upload the task, queue a build, queue a release, view the logs. Test as much of the script locally as you can.

#### Tip #3 - Version the task using SemVer
Changing the major number will force users to manually upgrade. This can be a really good thing!

#### Tip #4 - Leverage the task visibility
Use this to categorize your task in the catalog.

```json
  "visibility": [
    "Build",
    "Release"
  ],
```

![Img3]

#### Tip #5 - Provide good documentation about parameters and usage
Else developers will bother you...

#### Tip #6 - Remove any unnecessary files from the task folder
Do not store the .NET cmdlet source in the same directory.

#### Tip #7 - Consider using a CI/CD pipeline for building/testing/uploading tasks
Consider having a process to check-in the custom task just as any source code. Build the task which may run some cleanup of the directory, restore NPM or Nuget packages and run tests and increment the version. Consider GitVersion to apply this.

#### Tip #8 - Consider packaging custom .NET PowerShell cmdlets using NuGet
Build the custom .NET PowerShell cmdlet and package it using NuGet. Publish it to a feed and restore it in your task.

## References

1. tfx-cli - https://github.com/Microsoft/tfs-cli
2. Microsoft Tasks - https://github.com/Microsoft/vsts-tasks
3. Microsoft VSTS Task Lib - https://github.com/Microsoft/vsts-task-lib
    * Consuming SDK - https://github.com/Microsoft/vsts-task-lib/blob/master/powershell/Docs/Consuming.md
4. Task Manifest Schema - https://www.visualstudio.com/en-us/docs/integrate/extensions/develop/build-task-schema

[//]: # (Images)
[Img1]: /doc/img/newTask.png "Scaffold New Task"
[Img2]: /doc/img/inputGroups.png "Task Input Groups"
[Img3]: /doc/img/taskCatalog.png "Task Catalog"