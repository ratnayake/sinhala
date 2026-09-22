# SinhalaInput MSIX packaging

This directory packages `src/SinhalaInput.App` as an MSIX for Windows 10 (1809+, build
17763) and Windows 11, without depending on Visual Studio or a Windows Application
Packaging Project (`.wapproj`) — those aren't available in every environment this repo is
built in. Instead it's `dotnet publish` + a hand-authored manifest + `makeappx`/`signtool`,
driven by one PowerShell script.

See `docs/SINHALA-INPUT-TOOL-DESIGN.md` §11 for the packaging rationale in context.

## Contents

| File | Purpose |
|---|---|
| `AppxManifest.xml` | The package manifest: identity, capabilities (`runFullTrust`), visual elements, target OS versions. Heavily commented — read the comment at the top before changing `Identity`. |
| `Assets/` | Placeholder tile/logo PNGs (`Square44x44Logo.png`, `Square150x150Logo.png`, etc.) generated programmatically — see below. |
| `Build-MsixPackage.ps1` | Publishes, stages, packs, and (optionally) signs the package. |
| `.gitignore` | Excludes the `obj/` (staging) and `dist/` (output) directories the script creates. |

There is no real app icon design yet, so `Assets/` contains simple solid-color "Si"
monogram placeholders (generated with a throwaway `System.Drawing.Common` console app, not
hand-crafted bytes). Replace them with real artwork before any public release —
`makeappx pack` only validates that assets are well-formed PNGs at the sizes the manifest
declares, not that they look good.

## Running it

From a Windows PowerShell prompt, with the .NET SDK on `PATH`:

```powershell
cd packaging
.\Build-MsixPackage.ps1              # publish + pack an UNSIGNED SinhalaInput.msix
.\Build-MsixPackage.ps1 -Sign        # also sign it with a local self-signed certificate
```

Output lands in `packaging\dist\SinhalaInput.msix` (and, with `-Sign`,
`packaging\dist\SinhalaInput-selfsigned.cer`). All paths inside the script are resolved
from the script's own location (`$PSScriptRoot`), so it doesn't matter what directory you
invoke it from.

### What the script does

1. `dotnet publish`s `SinhalaInput.App` for `win-x64`, **self-contained by default**
   (see the tradeoff note below and the comment on the `-SelfContained` parameter in the
   script).
2. Stages the published output + `AppxManifest.xml` + `Assets\` into `packaging\obj\layout`.
3. Locates `makeappx.exe` (and, with `-Sign`, `signtool.exe`): first under a real Windows
   Kits install if one is present, otherwise from the `Microsoft.Windows.SDK.BuildTools`
   NuGet package cache (fetching it via a throwaway `dotnet restore` if it isn't already
   cached). This is exactly the scenario that package exists for — using the AppX packaging
   tools from a machine/CI agent that has the .NET SDK but not the full Windows SDK or
   Visual Studio.
4. Packs the layout into `packaging\dist\SinhalaInput.msix` with `makeappx pack`.
5. With `-Sign`: creates (or reuses) a self-signed code-signing certificate in
   `Cert:\CurrentUser\My` whose Subject matches `AppxManifest.xml`'s
   `Identity/Publisher` (`CN=Isuru Sampath Ratnayake`), signs the package with
   `signtool sign`, and exports the certificate's public half to
   `packaging\dist\SinhalaInput-selfsigned.cer`.

### Self-contained vs. framework-dependent

The script defaults to a **self-contained** publish (bundles its own .NET runtime,
~170MB) rather than framework-dependent (a few MB, but requires the matching
`Microsoft.WindowsDesktop.App` runtime already installed on the target machine).

This matches the design doc's stated preference for a consumer tool: SinhalaInput is meant
to be installed by people who type in Sinhala day-to-day, not developers — it shouldn't
fail to run because .NET isn't installed, or because a different major .NET version later
replaces the one it needed. MSIX already handles large packages fine (delta installs aren't
relevant here since this isn't Store-distributed against a shared framework package), so the
size cost is an acceptable tradeoff. Pass `-SelfContained:$false` to the script if you
specifically want the smaller, framework-dependent build and can guarantee the target
machine has the matching runtime — e.g. for quick internal testing on dev machines that
already have the SDK.

## Local install / trust caveat (self-signed certificate)

MSIX packages must be signed, and Windows will not install an unsigned or
untrusted-certificate package via `Add-AppxPackage`. Running `Build-MsixPackage.ps1 -Sign`
signs the package, but with a **self-signed** certificate that is not trusted by Windows out
of the box. To install the resulting package on a machine, that machine must first be told
to trust this specific certificate:

```powershell
# 1. As an administrator, trust the exported certificate (once per machine):
Import-Certificate -FilePath 'packaging\dist\SinhalaInput-selfsigned.cer' `
    -CertStoreLocation Cert:\LocalMachine\TrustedPeople

# 2. Then install the package:
Add-AppxPackage -Path 'packaging\dist\SinhalaInput.msix'
```

**This repo's build script does not run either of those two commands itself** — importing a
certificate into a machine-wide trust store and installing a package are both changes to
shared machine state, and that's a decision for whoever is doing the install, not something
a build script should do unattended.

This setup is appropriate for local development and internal/team distribution (everyone
installing it trusts the same self-signed cert once). It is **not** appropriate for public
distribution — an end user has no reason to trust a self-signed certificate you generated,
and Windows SmartScreen will flag it.

## What real distribution would need instead

- **A real code-signing certificate**, purchased from a public/EV code-signing CA. Set
  `AppxManifest.xml`'s `Identity/Publisher` to that certificate's exact Subject, sign with
  it via `signtool sign /f <pfx> ...` (or an HSM-backed signing service), and there is no
  local-trust step for end users — the CA's root is already trusted by Windows.
- **Or, Microsoft Store submission** (Partner Center): the Store issues its own
  `Identity/Name` and `Identity/Publisher` values (you don't choose them) and re-signs the
  package with a Store certificate on your behalf, handling trust and updates for the user
  automatically. This is also the path that gets a Store listing that shows the tool in
  search.
- **Real app icon/tile artwork** in place of the placeholder monogram PNGs in `Assets/`.
- Optionally, an MSIX **startup task** extension (`<uap5:Extension Category="windows.startupTask">`)
  if you want the tray app to auto-start at sign-in as part of the package manifest instead
  of a separate Registry Run-key/shortcut — not added here since it's a behavior change, not
  a packaging-only change, and is out of scope for this pass.
