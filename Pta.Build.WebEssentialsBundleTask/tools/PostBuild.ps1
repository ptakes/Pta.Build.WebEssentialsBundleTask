param (
	[string]$ProjectDir,
	[string]$TargetDir,
	[string]$TargetFileName
)

$NuGetDir =  Join-Path $ProjectDir 'nuget'
$NuGetBuildDir =  Join-Path $NuGetDir 'build'
$TargetFile =  Join-Path $TargetDir $TargetFileName
Copy-Item  $TargetFile	$NuGetBuildDir -Force

$ToolsDir =  Join-Path $ProjectDir 'tools'
$NuGetCmd =  Join-Path $ToolsDir 'NuGet.exe'
$NuGetCmdArgs = 'pack',  [io.path]::ChangeExtension($TargetFileName, 'nuspec'), '-NonInteractive'
Set-Location  $NuGetDir
& $NuGetCmd $NuGetCmdArgs