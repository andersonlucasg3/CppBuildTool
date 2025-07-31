#!/usr/local/bin/pwsh

param(
    [Parameter(Mandatory = $true)]
    [string] $Project,

    [Parameter(Mandatory = $true)]
    [ValidateSet(
        "Clang",
        "VisualStudio"
    )]
    [string] $Generator,

    [Parameter()]
    [string[]] $Modules,

    [Parameter(Mandatory = $true)]
    [ValidateSet(
        "iOS",
        "tvOS",
        "visionOS",
        "macOS",
        "Windows"
    )]
    [string] $Platform,

    [ValidateSet(
        "Debug",
        "Release"
    )]
    [string] $Configuration = "Debug"
)


. $PSScriptRoot/Commons.ps1

CompileProjectTools

$Arguments = [List[string]]::new()
$Arguments.Add("GenCodeProject")

AddArgument ([ref]$Arguments) "Project" $Project
AddArgument ([ref]$Arguments) "Generator" $Generator
AddArgument ([ref]$Arguments) "Modules" $Modules
AddArgument ([ref]$Arguments) "Platform" $Platform
AddArgument ([ref]$Arguments) "Configuration" $Configuration
AddSwitch ([ref]$Arguments) "DisableMT" $true

dotnet exec ./Engine/Binaries/DotNet/ProjectTools/BuildTool.dll $Arguments

Exit $LASTEXITCODE