<#
.SYNOPSIS
    Builds SinhalaInput.msix from the SinhalaInput.App WPF project: publish -> stage -> pack -> (optionally) sign.

.DESCRIPTION
    There is no Visual Studio "Windows Application Packaging Project" available in every
    environment this repo is built in, so this script does by hand what a .wapproj would
    otherwise do via MSBuild targets:

      1. `dotnet publish` SinhalaInput.App for win-x64.
      2. Stage the published app + AppxManifest.xml + Assets\ into a package layout folder.
      3. Locate makeappx.exe / signtool.exe (Windows Kits if installed, otherwise the
         Microsoft.Windows.SDK.BuildTools NuGet package cache).
      4. Pack the layout into SinhalaInput.msix with `makeappx pack`.
      5. If -Sign is passed, create/reuse a local self-signed code-signing certificate and
         sign the package with `signtool sign`.

    All paths are resolved relative to $PSScriptRoot, so this script works no matter what
    directory it is invoked from.

.PARAMETER Configuration
    Build configuration to publish. Default: Release.

.PARAMETER SelfContained
    Whether to publish self-contained (bundles its own .NET runtime) or framework-dependent
    (requires the matching Windows Desktop .NET runtime already installed on the target
    machine). Default: $true.

    TRADEOFF (see docs/SINHALA-INPUT-TOOL-DESIGN.md, section 11): framework-dependent publishes are
    far smaller (a few MB vs. ~170MB here) and this dev machine already has the matching
    Microsoft.WindowsDesktop.App runtime, so framework-dependent would "just work" locally.
    But SinhalaInput is meant to be a one-click consumer install for people who may not have
    .NET 10 installed at all, and MSIX already absorbs the extra package size gracefully
    (delta/differential updates, on-disk dedup for shared framework packages is not applicable
    here anyway since this isn't Store-distributed against a shared framework package).
    Self-contained means the installed app never breaks because a shared runtime got
    uninstalled or a different major version replaced it. That is why this script defaults to
    self-contained, matching the design doc's stated preference for a consumer tool. Pass
    -SelfContained:$false for a framework-dependent build if you specifically want the smaller
    package and can guarantee the target machine has the .NET 10 Windows Desktop runtime.

.PARAMETER Sign
    If passed, create (or reuse) a local self-signed code-signing certificate whose Subject
    matches AppxManifest.xml's Identity/Publisher, and sign SinhalaInput.msix with it. Without
    this switch, the script only packs (produces an unsigned .msix).

.PARAMETER CertSubject
    Subject name for the self-signed signing certificate. MUST exactly match the Publisher
    attribute in AppxManifest.xml's <Identity> element, or Windows will refuse to install the
    resulting package even after the cert is trusted. Default: "CN=Isuru Sampath Ratnayake".

.EXAMPLE
    .\Build-MsixPackage.ps1
    Publishes and packs an unsigned SinhalaInput.msix.

.EXAMPLE
    .\Build-MsixPackage.ps1 -Sign
    Publishes, packs, and signs SinhalaInput.msix with a local self-signed certificate.
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [bool]$SelfContained = $true,
    [switch]$Sign,
    [string]$CertSubject = "CN=Isuru Sampath Ratnayake"
)

$ErrorActionPreference = "Stop"

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
$distDir      = Join-Path $packagingDir "dist"
$msixPath     = Join-Path $distDir "SinhalaInput.msix"

$runtimeIdentifier = "win-x64"

Write-Host "== SinhalaInput MSIX build ==" -ForegroundColor Cyan
Write-Host "Repo root:    $repoRoot"
Write-Host "App project:  $appProject"
Write-Host "Config:       $Configuration ($runtimeIdentifier, self-contained=$SelfContained)"
Write-Host ""

if (-not (Test-Path $appProject)) {
    throw "Could not find SinhalaInput.App.csproj at '$appProject'. Is this script still under packaging\ in the repo?"
}
if (-not (Test-Path $manifestPath)) {
    throw "Could not find AppxManifest.xml at '$manifestPath'."
}
if (-not (Test-Path $assetsDir)) {
    throw "Could not find Assets\ at '$assetsDir'."
}

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

$exePath = Join-Path $publishDir "SinhalaInput.App.exe"
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

# Publish output goes at the layout root (this is where AppxManifest.xml's
# Executable="SinhalaInput.App.exe" expects to find it).
Copy-Item -Path (Join-Path $publishDir "*") -Destination $layoutDir -Recurse -Force

Copy-Item -Path $manifestPath -Destination (Join-Path $layoutDir "AppxManifest.xml") -Force
Copy-Item -Path $assetsDir -Destination (Join-Path $layoutDir "Assets") -Recurse -Force

Write-Host "Layout staged at $layoutDir" -ForegroundColor Green
Write-Host ""

# ---------------------------------------------------------------------------
# Step 3: locate makeappx.exe / signtool.exe
# ---------------------------------------------------------------------------
# This machine has no Visual Studio and no full Windows SDK install, so we look in two places:
#   1. A real Windows Kits install, if one happens to be present
#      (C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\).
#   2. The Microsoft.Windows.SDK.BuildTools NuGet package cache. Microsoft publishes this
#      package specifically so makeappx/signtool/makepri can be used from a build/CI context
#      without installing the multi-gigabyte Windows SDK. If it's not already restored, we
#      fetch it into a throwaway project so `dotnet restore` populates the normal NuGet
#      global-packages cache (~/.nuget/packages), then locate the tools inside it. This avoids
#      any interactive installer and keeps disk usage to the tools package only (a few MB).
function Find-SdkTool {
    param([Parameter(Mandatory)][string]$ToolName)

    # 1) Windows Kits, if installed.
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

    # 2) Microsoft.Windows.SDK.BuildTools NuGet package cache.
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

Write-Host "-- Locating makeappx.exe / signtool.exe..." -ForegroundColor Cyan
$makeAppxPath = Get-SdkToolOrFetch -ToolName "makeappx.exe"
Write-Host "makeappx: $makeAppxPath" -ForegroundColor Green
if ($Sign) {
    $signToolPath = Get-SdkToolOrFetch -ToolName "signtool.exe"
    Write-Host "signtool: $signToolPath" -ForegroundColor Green
}
Write-Host ""

# ---------------------------------------------------------------------------
# Step 4: pack
# ---------------------------------------------------------------------------
Write-Host "-- Packing $msixPath..." -ForegroundColor Cyan

New-Item -ItemType Directory -Path $distDir -Force | Out-Null
if (Test-Path $msixPath) {
    Remove-Item $msixPath -Force
}

& $makeAppxPath pack /d $layoutDir /p $msixPath /overwrite
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
# Step 5: optional signing
# ---------------------------------------------------------------------------
if ($Sign) {
    Write-Host "-- Signing $msixPath..." -ForegroundColor Cyan

    # Reuse an existing local self-signed cert with the right subject if one exists,
    # rather than minting a new one (and a new thumbprint/trust requirement) every run.
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Subject -eq $CertSubject } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if (-not $cert) {
        Write-Host "No existing self-signed cert found for '$CertSubject'; creating one (CurrentUser\My)." -ForegroundColor Yellow
        $cert = New-SelfSignedCertificate `
            -Type CodeSigningCert `
            -Subject $CertSubject `
            -KeyUsage DigitalSignature `
            -FriendlyName "SinhalaInput MSIX signing (self-signed, local/internal use only)" `
            -CertStoreLocation "Cert:\CurrentUser\My" `
            -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    }

    & $signToolPath sign /fd SHA256 /sha1 $cert.Thumbprint $msixPath
    if ($LASTEXITCODE -ne 0) {
        throw "signtool sign failed with exit code $LASTEXITCODE."
    }
    Write-Host "Signed with certificate thumbprint $($cert.Thumbprint)" -ForegroundColor Green

    $cerPath = Join-Path $distDir "SinhalaInput-selfsigned.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
    Write-Host "Exported public cert to $cerPath (needed to trust it on any install machine)" -ForegroundColor Green
    Write-Host ""
}

# ---------------------------------------------------------------------------
# Next steps for the human running this script.
# ---------------------------------------------------------------------------
Write-Host "== Done ==" -ForegroundColor Cyan
Write-Host "Package: $msixPath"
Write-Host ""
if ($Sign) {
    Write-Host "This package is signed with a SELF-SIGNED certificate. Before Add-AppxPackage" -ForegroundColor Yellow
    Write-Host "will accept it, the certificate must be trusted on the installing machine:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. Copy $((Split-Path $cerPath -Leaf)) to the target machine (or use this one)."
    Write-Host "  2. As an administrator, import it into the Trusted People store (or Trusted"
    Write-Host "     Root, if you prefer - Trusted People is the narrower, recommended scope):"
    Write-Host "       Import-Certificate -FilePath '$cerPath' -CertStoreLocation Cert:\LocalMachine\TrustedPeople"
    Write-Host "  3. Then install the package:"
    Write-Host "       Add-AppxPackage -Path '$msixPath'"
    Write-Host ""
    Write-Host "This script deliberately does NOT run either of those two commands itself -" -ForegroundColor Yellow
    Write-Host "trusting a certificate into a machine-wide store and installing a package are" -ForegroundColor Yellow
    Write-Host "both changes to shared machine state that a build script shouldn't make on its" -ForegroundColor Yellow
    Write-Host "own. Run them yourself once you've reviewed the package." -ForegroundColor Yellow
} else {
    Write-Host "This package is UNSIGNED. Add-AppxPackage will refuse to install it as-is." -ForegroundColor Yellow
    Write-Host "Re-run with -Sign to sign it with a local self-signed certificate, then see" -ForegroundColor Yellow
    Write-Host "packaging\README.md for the local-trust + install steps." -ForegroundColor Yellow
}
Write-Host ""
Write-Host "For REAL distribution (outside this machine/team), replace the self-signed" -ForegroundColor Yellow
Write-Host "certificate with a certificate from a public code-signing CA, or submit the" -ForegroundColor Yellow
Write-Host "package to the Microsoft Store (which re-signs it with a Store-issued identity" -ForegroundColor Yellow
Write-Host "and handles trust for you). See packaging\README.md." -ForegroundColor Yellow
