<#
.SYNOPSIS
    Builds EasyAkuru.msix from the SinhalaInput.App WPF project: publish -> stage -> makepri -> pack -> (optionally) sign.

.DESCRIPTION
    There is no Visual Studio "Windows Application Packaging Project" available in every
    environment this repo is built in, so this script does by hand what a .wapproj would
    otherwise do via MSBuild targets:

      1. `dotnet publish` SinhalaInput.App (which produces EasyAkuru.exe) for win-x64.
      2. Stage the published app + AppxManifest.xml + Assets\ into a package layout folder.
      3. Locate makeappx.exe / makepri.exe / signtool.exe (Windows Kits if installed,
         otherwise the Microsoft.Windows.SDK.BuildTools NuGet package cache).
      4. Generate resources.pri with makepri so Windows can pick the scale-/targetsize-
         qualified logo variants in Assets\ (Square44x44Logo.scale-200.png, ...).
      5. Pack the layout with `makeappx pack`.
      6. Either sign it with a local self-signed certificate (-Sign), or wrap the unsigned
         package in an .msixupload for Partner Center (-StoreUpload), or leave it unsigned.

    All paths are resolved relative to $PSScriptRoot, so this script works no matter what
    directory it is invoked from.

.PARAMETER Configuration
    Build configuration to publish. Default: Release.

.PARAMETER SelfContained
    Whether to publish self-contained (bundles its own .NET runtime) or framework-dependent
    (requires the matching Windows Desktop .NET runtime already installed on the target
    machine). Default: $true.

    TRADEOFF (see docs/SINHALA-INPUT-TOOL-DESIGN.md, section 11): framework-dependent publishes
    are far smaller (a few MB vs. ~170MB) but only run where the matching
    Microsoft.WindowsDesktop.App runtime is installed. EasyAkuru is a one-click consumer
    install for people who may not have .NET 10 at all, so the script defaults to
    self-contained. Pass -SelfContained:$false for a framework-dependent build if you can
    guarantee the target machine has the .NET 10 Windows Desktop runtime.

.PARAMETER Version
    Optional four-part package version (e.g. 1.0.1.0) written into the staged manifest's
    Identity/Version. Defaults to the version in AppxManifest.xml. Every Store submission
    needs a higher version than the last one, and the Store requires the fourth part to be 0.

.PARAMETER Sign
    Create (or reuse) a local self-signed code-signing certificate whose Subject equals
    AppxManifest.xml's Identity/Publisher, and sign EasyAkuru.msix with it. For sideload
    testing only.

.PARAMETER StoreUpload
    Build an unsigned package for Microsoft Store submission (the Store signs it) and wrap it
    in dist\EasyAkuru_<version>_x64.msixupload for upload to Partner Center.
    Cannot be combined with -Sign.

.PARAMETER CertSubject
    Subject name for the self-signed signing certificate. Defaults to the Publisher attribute
    of AppxManifest.xml's <Identity> element; it MUST match that value exactly or Windows will
    refuse to install the signed package.

.EXAMPLE
    .\Build-MsixPackage.ps1
    Publishes and packs an unsigned EasyAkuru.msix.

.EXAMPLE
    .\Build-MsixPackage.ps1 -Sign
    Publishes, packs, and signs EasyAkuru.msix with a local self-signed certificate.

.EXAMPLE
    .\Build-MsixPackage.ps1 -StoreUpload -Version 1.0.1.0
    Builds dist\EasyAkuru_1.0.1.0_x64.msixupload for Partner Center.
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [bool]$SelfContained = $true,
    [string]$Version,
    [switch]$Sign,
    [switch]$StoreUpload,
    [string]$CertSubject
)

$ErrorActionPreference = "Stop"

if ($Sign -and $StoreUpload) {
    throw "-Sign and -StoreUpload cannot be combined: the Store signs submitted packages itself, so a Store upload must be unsigned."
}

# ---------------------------------------------------------------------------
# Paths (all relative to this script's own location, per repo convention).
# ---------------------------------------------------------------------------
$packagingDir = $PSScriptRoot
$repoRoot     = Resolve-Path (Join-Path $packagingDir "..")
$appProject   = Join-Path $repoRoot "src\SinhalaInput.App\SinhalaInput.App.csproj"
$manifestPath = Join-Path $packagingDir "AppxManifest.xml"
$assetsDir    = Join-Path $packagingDir "Assets"

$workDir      = Join-Path $packagingDir "obj"
$publishDir   = Join-Path $workDir "publish"
$layoutDir    = Join-Path $workDir "layout"
$priDir       = Join-Path $workDir "pri"
$distDir      = Join-Path $packagingDir "dist"
$msixPath     = Join-Path $distDir "EasyAkuru.msix"

$exeName           = "EasyAkuru.exe"
$runtimeIdentifier = "win-x64"

if (-not (Test-Path $appProject)) {
    throw "Could not find SinhalaInput.App.csproj at '$appProject'. Is this script still under packaging\ in the repo?"
}
if (-not (Test-Path $manifestPath)) {
    throw "Could not find AppxManifest.xml at '$manifestPath'."
}
if (-not (Test-Path $assetsDir)) {
    throw "Could not find Assets\ at '$assetsDir'."
}

[xml]$manifest = Get-Content -Path $manifestPath -Raw -Encoding UTF8
$identity = $manifest.Package.Identity
if ($Version) {
    if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
        throw "-Version must be a four-part version such as 1.0.1.0 (got '$Version')."
    }
    $identity.Version = $Version
}
$packageVersion = $identity.Version
if ($StoreUpload -and $packageVersion -notmatch '\.0$') {
    throw "The Microsoft Store requires the fourth part of the package version to be 0 (got '$packageVersion')."
}
if (-not $CertSubject) {
    $CertSubject = $identity.Publisher
}

$mode = if ($StoreUpload) { "Store upload (unsigned)" } elseif ($Sign) { "self-signed" } else { "unsigned" }
Write-Host "== EasyAkuru MSIX build ==" -ForegroundColor Cyan
Write-Host "Repo root:    $repoRoot"
Write-Host "App project:  $appProject"
Write-Host "Identity:     $($identity.Name) $packageVersion ($($identity.Publisher))"
Write-Host "Config:       $Configuration ($runtimeIdentifier, self-contained=$SelfContained)"
Write-Host "Mode:         $mode"
Write-Host ""

# ---------------------------------------------------------------------------
# Step 1: dotnet publish
# ---------------------------------------------------------------------------
Write-Host "-- Publishing SinhalaInput.App ($runtimeIdentifier)..." -ForegroundColor Cyan

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

$selfContainedArg = if ($SelfContained) { "true" } else { "false" }

& dotnet publish $appProject `
    -c $Configuration `
    -r $runtimeIdentifier `
    --self-contained $selfContainedArg `
    -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$exePath = Join-Path $publishDir $exeName
if (-not (Test-Path $exePath)) {
    throw "Publish succeeded but '$exePath' was not produced. Check the publish output above."
}
Write-Host "Published to $publishDir" -ForegroundColor Green
Write-Host ""

# ---------------------------------------------------------------------------
# Step 2: stage the package layout (publish output + manifest + assets)
# ---------------------------------------------------------------------------
Write-Host "-- Staging package layout..." -ForegroundColor Cyan

if (Test-Path $layoutDir) {
    Remove-Item $layoutDir -Recurse -Force
}
New-Item -ItemType Directory -Path $layoutDir -Force | Out-Null

# Publish output goes at the layout root, where the manifest's Executable="EasyAkuru.exe" expects it.
Copy-Item -Path (Join-Path $publishDir "*") -Destination $layoutDir -Recurse -Force

$stagedManifestPath = Join-Path $layoutDir "AppxManifest.xml"
$manifest.Save($stagedManifestPath)
Copy-Item -Path $assetsDir -Destination (Join-Path $layoutDir "Assets") -Recurse -Force

Write-Host "Layout staged at $layoutDir" -ForegroundColor Green
Write-Host ""

# ---------------------------------------------------------------------------
# Step 3: locate makeappx.exe / makepri.exe / signtool.exe
# ---------------------------------------------------------------------------
# This machine may have no Visual Studio and no full Windows SDK install, so we look in two places:
#   1. A real Windows Kits install, if one happens to be present
#      (C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\).
#   2. The Microsoft.Windows.SDK.BuildTools NuGet package cache. Microsoft publishes this
#      package specifically so makeappx/signtool/makepri can be used from a build/CI context
#      without installing the multi-gigabyte Windows SDK. If it's not already restored, we
#      fetch it into a throwaway project so `dotnet restore` populates the normal NuGet
#      global-packages cache (~/.nuget/packages), then locate the tools inside it.
function Find-SdkTool {
    param([Parameter(Mandatory)][string]$ToolName)

    $kitsRoot = "C:\Program Files (x86)\Windows Kits\10\bin"
    if (Test-Path $kitsRoot) {
        $candidate = Get-ChildItem -Path $kitsRoot -Recurse -Filter $ToolName -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "\\x64\\" } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
        if ($candidate) {
            return $candidate.FullName
        }
    }

    $nugetPackagesRoot = Join-Path $env:USERPROFILE ".nuget\packages\microsoft.windows.sdk.buildtools"
    if (Test-Path $nugetPackagesRoot) {
        $candidate = Get-ChildItem -Path $nugetPackagesRoot -Recurse -Filter $ToolName -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "\\x64\\" } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
        if ($candidate) {
            return $candidate.FullName
        }
    }

    return $null
}

function Get-SdkToolOrFetch {
    param([Parameter(Mandatory)][string]$ToolName)

    $tool = Find-SdkTool -ToolName $ToolName
    if ($tool) {
        return $tool
    }

    Write-Host "$ToolName not found locally; fetching Microsoft.Windows.SDK.BuildTools from NuGet..." -ForegroundColor Yellow
    $fetchDir = Join-Path $workDir "sdk-buildtools-fetch"
    New-Item -ItemType Directory -Path $fetchDir -Force | Out-Null
    $fetchProj = Join-Path $fetchDir "fetch.csproj"
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.28000.2705" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path $fetchProj -Encoding utf8

    & dotnet restore $fetchProj | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw @"
Could not find $ToolName and could not fetch it either.

Neither a Windows Kits install (C:\Program Files (x86)\Windows Kits\10\bin)
nor the Microsoft.Windows.SDK.BuildTools NuGet package is available, and
restoring that package failed (see dotnet output above -- likely no network
access to nuget.org from this machine).

To finish packaging on a machine with proper tooling, either:
  - Install the Windows 10/11 SDK (or Visual Studio with the "Desktop
    development with C++" or "Universal Windows Platform" workload, which
    includes the SDK), or
  - Ensure this machine can reach nuget.org and re-run this script so it can
    restore the Microsoft.Windows.SDK.BuildTools package on your behalf.
"@
    }

    $tool = Find-SdkTool -ToolName $ToolName
    if (-not $tool) {
        throw "Restored Microsoft.Windows.SDK.BuildTools but still could not find $ToolName under $nugetPackagesRoot. The package layout may have changed; inspect it manually."
    }
    return $tool
}

Write-Host "-- Locating SDK tools..." -ForegroundColor Cyan
$makeAppxPath = Get-SdkToolOrFetch -ToolName "makeappx.exe"
Write-Host "makeappx: $makeAppxPath" -ForegroundColor Green
$makePriPath = Get-SdkToolOrFetch -ToolName "makepri.exe"
Write-Host "makepri:  $makePriPath" -ForegroundColor Green
if ($Sign) {
    $signToolPath = Get-SdkToolOrFetch -ToolName "signtool.exe"
    Write-Host "signtool: $signToolPath" -ForegroundColor Green
}
Write-Host ""

# ---------------------------------------------------------------------------
# Step 4: resources.pri
# ---------------------------------------------------------------------------
# makepri indexes every file under its project root, so it is pointed at a folder holding
# only Assets\ rather than the whole layout (which contains hundreds of runtime DLLs). The
# resource names ("Files/Assets/...") are relative to that root, so they match the layout.
Write-Host "-- Generating resources.pri..." -ForegroundColor Cyan

if (Test-Path $priDir) {
    Remove-Item $priDir -Recurse -Force
}
$priRoot = Join-Path $priDir "root"
New-Item -ItemType Directory -Path $priRoot -Force | Out-Null
Copy-Item -Path $assetsDir -Destination (Join-Path $priRoot "Assets") -Recurse -Force
$priConfigPath = Join-Path $priDir "priconfig.xml"
$priPath = Join-Path $layoutDir "resources.pri"

& $makePriPath createconfig /cf $priConfigPath /dq en-US /pv 10.0.0 /o
if ($LASTEXITCODE -ne 0) {
    throw "makepri createconfig failed with exit code $LASTEXITCODE."
}
# The default config splits scale/language candidates into resources.<qualifier>.pri files meant
# for resource packages in a bundle; a single .msix needs them all in one resources.pri.
[xml]$priConfig = Get-Content -Path $priConfigPath -Raw
$priPackaging = $priConfig.SelectSingleNode("/resources/packaging")
if ($priPackaging) {
    $priPackaging.ParentNode.RemoveChild($priPackaging) | Out-Null
}
$priConfig.Save($priConfigPath)
& $makePriPath new /pr $priRoot /cf $priConfigPath /mn $stagedManifestPath /of $priPath /o
if ($LASTEXITCODE -ne 0) {
    throw "makepri new failed with exit code $LASTEXITCODE."
}
Write-Host "Wrote $priPath" -ForegroundColor Green
Write-Host ""

# ---------------------------------------------------------------------------
# Step 5: pack
# ---------------------------------------------------------------------------
Write-Host "-- Packing $msixPath..." -ForegroundColor Cyan

New-Item -ItemType Directory -Path $distDir -Force | Out-Null
if (Test-Path $msixPath) {
    Remove-Item $msixPath -Force
}

& $makeAppxPath pack /d $layoutDir /p $msixPath /o
if ($LASTEXITCODE -ne 0) {
    throw "makeappx pack failed with exit code $LASTEXITCODE."
}
if (-not (Test-Path $msixPath)) {
    throw "makeappx reported success but '$msixPath' does not exist."
}

$msixSizeMb = [math]::Round((Get-Item $msixPath).Length / 1MB, 1)
Write-Host "Packed $msixPath ($msixSizeMb MB)" -ForegroundColor Green
Write-Host ""

# ---------------------------------------------------------------------------
# Step 6a: optional self-signing (sideload testing)
# ---------------------------------------------------------------------------
if ($Sign) {
    Write-Host "-- Signing $msixPath..." -ForegroundColor Cyan

    # Reuse an existing local self-signed cert with the right subject if one exists,
    # rather than minting a new one (and a new thumbprint/trust requirement) every run.
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Subject -eq $CertSubject -and $_.NotAfter -gt (Get-Date) } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if (-not $cert) {
        Write-Host "No existing self-signed cert found for '$CertSubject'; creating one (CurrentUser\My)." -ForegroundColor Yellow
        $cert = New-SelfSignedCertificate `
            -Type CodeSigningCert `
            -Subject $CertSubject `
            -KeyUsage DigitalSignature `
            -FriendlyName "EasyAkuru MSIX signing (self-signed, local/internal use only)" `
            -CertStoreLocation "Cert:\CurrentUser\My" `
            -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    }

    & $signToolPath sign /fd SHA256 /sha1 $cert.Thumbprint $msixPath
    if ($LASTEXITCODE -ne 0) {
        throw "signtool sign failed with exit code $LASTEXITCODE."
    }
    Write-Host "Signed with certificate thumbprint $($cert.Thumbprint)" -ForegroundColor Green

    $cerPath = Join-Path $distDir "EasyAkuru-selfsigned.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
    Write-Host "Exported public cert to $cerPath (needed to trust it on any install machine)" -ForegroundColor Green
    Write-Host ""
}

# ---------------------------------------------------------------------------
# Step 6b: optional Store upload package
# ---------------------------------------------------------------------------
if ($StoreUpload) {
    Write-Host "-- Creating Partner Center upload..." -ForegroundColor Cyan

    # An .msixupload is a zip holding the unsigned .msix (plus optional symbol files).
    $packageFileName = "$($identity.Name)_$($packageVersion)_x64.msix"
    $uploadPath = Join-Path $distDir "EasyAkuru_$($packageVersion)_x64.msixupload"
    if (Test-Path $uploadPath) {
        Remove-Item $uploadPath -Force
    }
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::Open($uploadPath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $msixPath, $packageFileName, [System.IO.Compression.CompressionLevel]::NoCompression) | Out-Null
    } finally {
        $zip.Dispose()
    }
    Write-Host "Wrote $uploadPath" -ForegroundColor Green
    Write-Host ""
}

# ---------------------------------------------------------------------------
# Next steps for the human running this script.
# ---------------------------------------------------------------------------
Write-Host "== Done ==" -ForegroundColor Cyan
Write-Host "Package: $msixPath"
Write-Host ""
if ($StoreUpload) {
    Write-Host "Upload $((Split-Path $uploadPath -Leaf)) (or the unsigned $((Split-Path $msixPath -Leaf)))" -ForegroundColor Yellow
    Write-Host "on the Packages page of the EasyAkuru submission in Partner Center. The Store" -ForegroundColor Yellow
    Write-Host "signs it; see packaging\README.md for the full submission checklist." -ForegroundColor Yellow
} elseif ($Sign) {
    Write-Host "This package is signed with a SELF-SIGNED certificate. Before Add-AppxPackage" -ForegroundColor Yellow
    Write-Host "will accept it, the certificate must be trusted on the installing machine:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. Copy $((Split-Path $cerPath -Leaf)) to the target machine (or use this one)."
    Write-Host "  2. As an administrator, import it into the Trusted People store:"
    Write-Host "       Import-Certificate -FilePath '$cerPath' -CertStoreLocation Cert:\LocalMachine\TrustedPeople"
    Write-Host "  3. Then install the package:"
    Write-Host "       Add-AppxPackage -Path '$msixPath'"
    Write-Host ""
    Write-Host "This script deliberately does NOT run either of those commands itself -" -ForegroundColor Yellow
    Write-Host "trusting a certificate into a machine-wide store and installing a package are" -ForegroundColor Yellow
    Write-Host "both changes to shared machine state that a build script shouldn't make on its" -ForegroundColor Yellow
    Write-Host "own. Run them yourself once you've reviewed the package." -ForegroundColor Yellow
} else {
    Write-Host "This package is UNSIGNED. Add-AppxPackage will refuse to install it as-is." -ForegroundColor Yellow
    Write-Host "Re-run with -Sign for a sideloadable test build, or -StoreUpload for a" -ForegroundColor Yellow
    Write-Host "Partner Center submission. See packaging\README.md." -ForegroundColor Yellow
}
