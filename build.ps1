# RaftVR Mod Builder Script
# This script compiles RaftVRMod, copies dependencies, compiles RaftVRLoader, and packages both RaftVR.rmod and RaftNonVR.rmod.
# It can be run locally or in GitHub Actions.

param(
    [string]$RaftPath = "F:\SteamLibrary\steamapps\common\Raft",
    [string]$RMLPath = "$env:APPDATA\RaftModLoader\binaries",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Helper to find MSBuild
function Find-MSBuild {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($vsPath) {
            $msbuild = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $msbuild) { return $msbuild }
        }
    }
    
    # Fallback paths
    $fallbacks = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
    )
    foreach ($path in $fallbacks) {
        if (Test-Path $path) { return $path }
    }
    return $null
}

$msbuild = Find-MSBuild
if (-not $msbuild) {
    Write-Error "Could not find MSBuild.exe on this system. Please make sure Visual Studio or Build Tools are installed."
}
Write-Host "Using MSBuild: $msbuild" -ForegroundColor Green

# Prepare search paths for references
$raftManaged = Join-Path $RaftPath "Raft_Data\Managed"
if (-not (Test-Path $raftManaged)) {
    $raftManaged = Join-Path $RaftPath "Raft_Data/Managed"
}

# Resolve additional lib paths as plain string paths (no PathInfo objects)
$referencePaths = @()

if (Test-Path $raftManaged) {
    $referencePaths += [System.IO.Path]::GetFullPath($raftManaged)
    Write-Host "Raft Reference Path: $raftManaged" -ForegroundColor Cyan
} else {
    Write-Warning "Raft Managed folder not found at: $raftManaged. If compiling in CI, ensure ReferencePath override contains these DLLs."
}

if (Test-Path $RMLPath) {
    $referencePaths += [System.IO.Path]::GetFullPath($RMLPath)
    Write-Host "RML Reference Path: $RMLPath" -ForegroundColor Cyan
} else {
    Write-Warning "RML binaries folder not found at: $RMLPath."
}

# Add local assemblies/libs to search paths using full paths
$referencePaths += [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "RaftVRLoader\RaftVRLoader\Assemblies"))
$referencePaths += [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "RaftVRMod\RaftVRMod\Patching\Assemblies"))
$referencePaths += [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "Libs"))

# Unique resolved paths
$resolvedPaths = @()
foreach ($path in $referencePaths) {
    if ($path) {
        $normalized = $path.Trim().TrimEnd('\').TrimEnd('/')
        if ($resolvedPaths -notcontains $normalized) {
            $resolvedPaths += $normalized
        }
    }
}

# Join reference paths for MSBuild
$refPathArg = [string]::Join(";", $resolvedPaths)
Write-Host "MSBuild ReferencePath Search Paths: $refPathArg" -ForegroundColor Cyan

Write-Host "--- 1. Restoring NuGet Packages ---" -ForegroundColor Green
# Restore NuGet packages
& $msbuild "RaftVRMod\RaftVRMod.sln" /t:Restore "/p:Configuration=$Configuration"
& $msbuild "RaftVRLoader\RaftVRLoader.sln" /t:Restore "/p:Configuration=$Configuration"
& $msbuild "RaftNonVR\RaftNonVR.sln" /t:Restore "/p:Configuration=$Configuration"

Write-Host "--- 2. Building RaftVRMod ---" -ForegroundColor Green
& $msbuild "RaftVRMod\RaftVRMod.sln" "/p:Configuration=$Configuration" "/p:ReferencePath=`"$refPathArg`""

# Copy RaftVRMod.dll to RaftVRLoader assemblies
$modDllSrc = Join-Path $PSScriptRoot "RaftVRMod\RaftVRMod\bin\$Configuration\RaftVRMod.dll"
# Fallback to obj if built there in some older configurations
if (-not (Test-Path $modDllSrc)) {
    $modDllSrc = Join-Path $PSScriptRoot "RaftVRMod\RaftVRMod\obj\$Configuration\RaftVRMod.dll"
}

if (Test-Path $modDllSrc) {
    $loaderAssembliesDir = Join-Path $PSScriptRoot "RaftVRLoader\RaftVRLoader\Assemblies"
    if (-not (Test-Path $loaderAssembliesDir)) {
        New-Item -ItemType Directory -Force -Path $loaderAssembliesDir | Out-Null
    }
    Copy-Item $modDllSrc (Join-Path $loaderAssembliesDir "RaftVRMod.dll") -Force
    Write-Host "Copied RaftVRMod.dll to loader Assemblies directory." -ForegroundColor Green
} else {
    Write-Error "Could not find built RaftVRMod.dll at: $modDllSrc"
}

Write-Host "--- 3. Building RaftVRLoader ---" -ForegroundColor Green
& $msbuild "RaftVRLoader\RaftVRLoader.sln" "/p:Configuration=$Configuration" "/p:ReferencePath=`"$refPathArg`""

Write-Host "--- 4. Building RaftNonVR ---" -ForegroundColor Green
& $msbuild "RaftNonVR\RaftNonVR.sln" "/p:Configuration=$Configuration" "/p:ReferencePath=`"$refPathArg`""

Write-Host "--- 5. Packaging RaftVR.rmod ---" -ForegroundColor Green
# Create temporary packaging folder
$buildDir = Join-Path $PSScriptRoot "build_vr"
if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force }
New-Item -ItemType Directory -Path $buildDir | Out-Null

# Copy files for .rmod structure (RaftVRLoader output, modinfo, assets, etc.)
# Replicates robocopy /E /XF *.csproj *.rmod /XD bin obj
$loaderSrc = Join-Path $PSScriptRoot "RaftVRLoader\RaftVRLoader"
Get-ChildItem $loaderSrc -Recurse | ForEach-Object {
    $relative = $_.FullName.Substring($loaderSrc.Length + 1)
    if ($relative -like "bin*" -or $relative -like "obj*" -or $relative -like "*.csproj" -or $relative -like "*.rmod" -or $relative -like "*.sln") {
        return
    }
    $dest = Join-Path $buildDir $relative
    if ($_.PsIsContainer) {
        New-Item -ItemType Directory -Force -Path $dest | Out-Null
    } else {
        Copy-Item $_.FullName $dest -Force
    }
}

# Zip it up to RaftVR.rmod
$rmodPath = Join-Path $PSScriptRoot "RaftVR.rmod"
if (Test-Path $rmodPath) { Remove-Item $rmodPath -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($buildDir, $rmodPath)

# Clean up build dir
Remove-Item $buildDir -Recurse -Force
Write-Host "Packaging successful: RaftVR.rmod" -ForegroundColor Green

Write-Host "--- 6. Packaging RaftNonVR.rmod ---" -ForegroundColor Green
$buildNonVRDir = Join-Path $PSScriptRoot "build_nonvr"
if (Test-Path $buildNonVRDir) { Remove-Item $buildNonVRDir -Recurse -Force }
New-Item -ItemType Directory -Path $buildNonVRDir | Out-Null

# Replicate robocopy /E /XF *.csproj *.rmod /XD bin obj
$nonVRSrc = Join-Path $PSScriptRoot "RaftNonVR\RaftNonVR"
Get-ChildItem $nonVRSrc -Recurse | ForEach-Object {
    $relative = $_.FullName.Substring($nonVRSrc.Length + 1)
    if ($relative -like "bin*" -or $relative -like "obj*" -or $relative -like "*.csproj" -or $relative -like "*.rmod" -or $relative -like "*.sln") {
        return
    }
    $dest = Join-Path $buildNonVRDir $relative
    if ($_.PsIsContainer) {
        New-Item -ItemType Directory -Force -Path $dest | Out-Null
    } else {
        Copy-Item $_.FullName $dest -Force
    }
}

# Zip it up to RaftNonVR.rmod
$rmodNonVRPath = Join-Path $PSScriptRoot "RaftNonVR.rmod"
if (Test-Path $rmodNonVRPath) { Remove-Item $rmodNonVRPath -Force }
[System.IO.Compression.ZipFile]::CreateFromDirectory($buildNonVRDir, $rmodNonVRPath)

# Clean up build dir
Remove-Item $buildNonVRDir -Recurse -Force
Write-Host "Packaging successful: RaftNonVR.rmod" -ForegroundColor Green

Write-Host "Build complete! Output files: RaftVR.rmod, RaftNonVR.rmod" -ForegroundColor Green
