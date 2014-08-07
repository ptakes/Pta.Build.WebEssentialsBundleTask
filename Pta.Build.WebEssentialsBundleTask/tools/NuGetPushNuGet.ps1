if ($PSVersionTable.PSVersion.Major -lt 3)
{
	$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}

$ToolsDir = $PSScriptRoot
$ProjectDir = Split-Path -Parent -Path $ToolsDir
$NuGetDir =  Join-Path $ProjectDir 'nuget'

$SpecPath = @(Get-ChildItem $NuGetDir -Filter '*.nuspec')[0].FullName
[xml]$Spec = Get-Content $SpecPath
$PackageVersion = Select-Xml '//version' $Spec | % {$_.Node.'#text'}

$PackageFilter = '*.' + $PackageVersion + '.nupkg'
$Package = @(Get-ChildItem $NuGetDir -Filter $PackageFilter)[0].Name

$NuGetCmd =  Join-Path $ToolsDir 'NuGet.exe'
$NuGetCmdArgs = 'push',  $Package, '-Source', 'nuget.org'

$OldLocation = Get-Location 
Set-Location $NuGetDir
& $NuGetCmd $NuGetCmdArgs
Set-Location $OldLocation
