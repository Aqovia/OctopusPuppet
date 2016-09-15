Param 
(
    [string]$target,
    [string]$destination
)

$here = Split-Path -Parent $MyInvocation.MyCommand.Path

Add-Type -Path "$here\Ionic.Zip.dll"
 
[Ionic.Zip.ZipFile] $zipfile = New-Object Ionic.Zip.ZipFile
  
Write-Host "Target:" (Resolve-Path $target)
  
$zipfile.AddDirectory((Resolve-Path $target))
$zipfile.Save($destination)
$zipfile.Dispose();