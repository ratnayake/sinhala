# EasyAkuru MSIX packaging

This directory packages `src/SinhalaInput.App` (which builds `EasyAkuru.exe`) as an MSIX for
Windows 10 (1809+, build 17763) and Windows 11, for both the Microsoft Store and local
sideload testing. It doesn't depend on Visual Studio or a Windows Application Packaging
Project (`.wapproj`): it's `dotnet publish` + a hand-authored manifest +
`makepri`/`makeappx`/`signtool`, driven by one PowerShell script.

See `docs/SINHALA-INPUT-TOOL-DESIGN.md` §11 for the packaging rationale in context.

## Contents

| File | Purpose |
|---|---|
| `AppxManifest.xml` | The package manifest: Store identity, capabilities (`runFullTrust`), visual elements, start-at-sign-in task, target OS versions. |
| `Assets/` | Logo, tile, Store and splash-screen PNGs at every scale (`.scale-100` … `.scale-400`) plus the taskbar/Start icon sizes (`Square44x44Logo.targetsize-16` … `-256`, with `_altform-unplated` variants). |
| `Build-MsixPackage.ps1` | Publishes, stages, indexes resources, packs, and signs or prepares a Store upload. |
| `.gitignore` | Excludes the `obj/` (staging) and `dist/` (output) directories the script creates. |

### Logo

The EasyAkuru mark is a bold white Sinhala letter **අ** on a maroon (`#8D153A`) rounded
square; the same colour is the tile and splash `BackgroundColor` in the manifest. The PNGs and
the app's multi-resolution icon (`src/SinhalaInput.App/Assets/SinhalaInput.ico`, 16–256 px)
were rendered programmatically from the "Nirmala UI" Bold glyph, with tighter padding at the
16–24 px sizes so the letter stays legible in the tray and taskbar.

## Partner Center identity

These values were reserved in Partner Center and are already in `AppxManifest.xml`. Don't
change them: the Store rejects a package whose identity doesn't match the reservation.

| Field | Value |
|---|---|
| App name | EasyAkuru |
| Store ID | `9P65C2RS1N87` |
| `Identity/Name` | `Ratcon.EasyAkuru` |
| `Identity/Publisher` | `CN=E7B0A46A-21DB-4DDA-BDA6-CCAB7E8753EA` |
| `PublisherDisplayName` | `Ratcon` |
| Package family name | `Ratcon.EasyAkuru_swc2syyz6eg42` |

## Running it

From a PowerShell prompt, with the .NET SDK on `PATH`:

```powershell
cd packaging
.\Build-MsixPackage.ps1                                  # unsigned dist\EasyAkuru.msix
.\Build-MsixPackage.ps1 -Sign                            # self-signed, for sideload testing
.\Build-MsixPackage.ps1 -StoreUpload                     # dist\EasyAkuru_1.0.0.0_x64.msixupload for Partner Center
.\Build-MsixPackage.ps1 -StoreUpload -Version 1.0.1.0    # same, with a bumped package version
```

All paths inside the script are resolved from the script's own location (`$PSScriptRoot`),
so it doesn't matter what directory you invoke it from.

### What the script does

1. `dotnet publish`es `SinhalaInput.App` for `win-x64`, **self-contained by default** (see
   below), producing `EasyAkuru.exe`.
2. Stages the published output + `AppxManifest.xml` (with `-Version` applied) + `Assets\`
   into `packaging\obj\layout`.
3. Locates `makeappx.exe`, `makepri.exe` and (with `-Sign`) `signtool.exe`: first under a
   Windows Kits install if one is present, otherwise from the
   `Microsoft.Windows.SDK.BuildTools` NuGet package cache (fetching it via a throwaway
   `dotnet restore` if it isn't cached yet).
4. Runs `makepri createconfig` + `makepri new` to generate `resources.pri`, which indexes
   the scale- and targetsize-qualified images so Windows picks the right size for each
   display scale, the taskbar, Start and the Store.
5. Packs the layout into `packaging\dist\EasyAkuru.msix` with `makeappx pack`.
6. Then, depending on the switch:
   - `-Sign`: creates (or reuses) a self-signed code-signing certificate in
     `Cert:\CurrentUser\My` whose Subject equals the manifest's `Identity/Publisher`, signs
     the package, and exports the public certificate to
     `packaging\dist\EasyAkuru-selfsigned.cer`.
   - `-StoreUpload`: leaves the package unsigned (the Store signs it) and wraps it in
     `packaging\dist\EasyAkuru_<version>_x64.msixupload`. Refuses a version whose fourth
     part isn't `0`, which the Store reserves.

### Self-contained vs. framework-dependent

The script defaults to a **self-contained** publish (bundles its own .NET runtime) rather
than framework-dependent (a few MB, but requires the matching `Microsoft.WindowsDesktop.App`
runtime on the target machine). EasyAkuru is installed by people who type Sinhala day to
day, not developers, so it shouldn't fail to start because .NET isn't installed. Pass
`-SelfContained:$false` if you specifically want the smaller build and can guarantee the
runtime is present.

## Automatic start

EasyAkuru is meant to be running whenever the user is signed in:

- The manifest declares a `desktop:StartupTask` (`TaskId="EasyAkuruStartup"`,
  `Enabled="true"`), so once installed the app **starts automatically at every sign-in**.
- **MSIX packages cannot run code at install time**, so Windows won't start the app the moment
  the Store finishes installing it. The first run happens when the user clicks **Launch** on
  the Store page (or opens EasyAkuru from Start); from the next sign-in onwards Windows starts
  it by itself.
- Windows owns the setting: users turn it off or on under **Settings › Apps › Startup** (or
  Task Manager's Startup apps tab). The app's own Settings window points there instead of
  offering its own checkbox, and never writes anything itself.

The unpackaged build (plain `EasyAkuru.exe`) has no manifest, so it instead writes a per-user
`HKCU\Software\Microsoft\Windows\CurrentVersion\Run` value (no admin rights needed),
controlled by the start-at-sign-in checkbox in Settings.

## Submitting to the Microsoft Store

1. Bump the version if this isn't the first submission: every upload needs a higher version
   than the last, with the fourth part `0` (e.g. `1.0.1.0`).
2. Build the upload:

   ```powershell
   .\packaging\Build-MsixPackage.ps1 -StoreUpload -Version 1.0.1.0
   ```

3. In Partner Center, open **EasyAkuru** › **Start your submission** (or update the
   existing one):
   - **Packages**: upload `packaging\dist\EasyAkuru_<version>_x64.msixupload`. Partner Center
     validates the identity against the reservation above.
   - **Store listings**: description, screenshots and the Store logo. `Assets\StoreLogo.scale-400.png`
     and `Assets\Square310x310Logo.scale-400.png` are suitable starting points for listing art.
   - **Submission options** › restricted capabilities: justify `runFullTrust`. EasyAkuru is a
     classic Win32/WPF desktop app that installs a global low-level keyboard hook to
     transliterate what the user types into Sinhala in any application; it needs full trust
     for `SetWindowsHookEx`/`SendInput`.
   - **Properties** / **Age ratings** / **Pricing and availability** as appropriate.
4. Submit for certification.

## Local sideload testing (self-signed certificate)

A package signed with `-Sign` only installs on a machine that trusts that specific
self-signed certificate:

```powershell
# 1. As an administrator, trust the exported certificate (once per machine):
Import-Certificate -FilePath 'packaging\dist\EasyAkuru-selfsigned.cer' `
    -CertStoreLocation Cert:\LocalMachine\TrustedPeople

# 2. Then install the package:
Add-AppxPackage -Path 'packaging\dist\EasyAkuru.msix'
```

**The build script does not run either command itself**: importing a certificate into a
machine-wide trust store and installing a package are changes to shared machine state, and
that's a decision for whoever is doing the install.

Because the certificate's Subject equals the Store `Publisher`, a sideloaded test build has
the same package family name as the Store app (`Ratcon.EasyAkuru_swc2syyz6eg42`). Uninstall
the test build (`Get-AppxPackage Ratcon.EasyAkuru | Remove-AppxPackage`) before installing
from the Store.
