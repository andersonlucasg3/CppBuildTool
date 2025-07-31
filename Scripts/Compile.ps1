#!/usr/local/bin/pwsh

using namespace System.Collections.Generic

param(
    [Parameter(Mandatory = $true)]
    [string] $Project,

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
    [string] $Configuration = "Debug",

    [switch] $Clean,
    [switch] $Recompile,
    [switch] $Relink,
    [switch] $PrintCompileCommands,
    [switch] $PrintLinkCommands
)

. $PSScriptRoot/Commons.ps1

CompileProjectTools

$Arguments = [List[string]]::new()

if ($Clean)
{
    $Arguments.Add("Clean")
}
else
{
    $Arguments.Add("Compile")
}

AddArgument ([ref]$Arguments) "Project" $Project
AddArgument ([ref]$Arguments) "Modules" $Modules
AddArgument ([ref]$Arguments) "Platform" $Platform
AddArgument ([ref]$Arguments) "Configuration" $Configuration
AddSwitch ([ref]$Arguments) "Recompile" $Recompile
AddSwitch ([ref]$Arguments) "PrintCompileCommands" $PrintCompileCommands
AddSwitch ([ref]$Arguments) "PrintLinkCommands" $PrintLinkCommands

dotnet exec ./Engine/Binaries/DotNet/ProjectTools/BuildTool.dll $Arguments

Exit $LASTEXITCODE