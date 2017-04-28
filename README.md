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
Custom tasks allows us to encapsulate logic. They are packaged into a command such as PowerShell or Node.js and a manifest.

### Setup

* Install CLI - ```npm install -g tfx-cli```
* Login - ```tfx login```
    * Enter the collection url: ```https://{companyName}.visualstudio.com/DefaultCollection```
    * Enter PAT: https://www.visualstudio.com/en-us/docs/setup-admin/team-services/use-personal-access-tokens-to-authenticate
* Download Task SDK - ```Save-Module -Name VstsTaskSdk -Path .\```

### SDK
The task SDK is statically linked to your custom task. One references the SDK in their command script. Add the SDK under the folder ```ps_modules\VstsTaskSdk``` in the task directory.

```
$ScriptPath = $(Split-Path -Parent $MyInvocation.MyCommand.Definition) 
Import-Module $(Join-Path $ScriptPath "ps_modules\VstsTaskSdk\VstsTaskSdk.psd1")
```

### Scaffold
Create a new task using the command ```tfx build tasks create```. This will provide you with a manifest, icon, & empty PowerShell script.

![Img1]

### Upload

### Testing

### Versioning

### Tips

#### Tip #1 - Add verbose messages

#### Tip #2 - Test the script locally

#### Tip #3 - Version the task using SemVer

#### Tip #4 - Leverage the task visibility

#### Tip #5 - Provide good documentation about parameters and usage

## References

1. tfx-cli - https://github.com/Microsoft/tfs-cli
2. Microsoft Tasks - https://github.com/Microsoft/vsts-tasks
3. Microsoft VSTS Task Lib - https://github.com/Microsoft/vsts-task-lib
    * Consuming SDK - https://github.com/Microsoft/vsts-task-lib/blob/master/powershell/Docs/Consuming.md


[//]: # (Images)
[Img1]: /doc/img/newTask.png "Scaffold New Task"    