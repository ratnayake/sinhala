# EasyAkuru Store listing upload checklist

This checklist is for Partner Center › **EasyAkuru** (Store ID `9P65C2RS1N87`) › submission ›
**Store listings › English (United States)**. The text for every field is in
[`STORE-LISTING.md`](STORE-LISTING.md), and the packaging and certification steps are in
[`docs/STORE-SUBMISSION.md`](../docs/STORE-SUBMISSION.md).

## File → Partner Center field

All paths are relative to `store-listing/`. Every file is PNG, has no transparency (24-bit RGB),
and is far below the 50 MB limit.

| File | Pixels | Partner Center field | Required? |
|---|---|---|---|
| `screenshots/01-typing-with-suggestions.png` | 1920×1080 | Screenshots › Desktop, #1 (caption in STORE-LISTING.md §5) | **Yes** (at least 1 screenshot) |
| `screenshots/02-finished-sinhala-text.png` | 1920×1080 | Screenshots › Desktop, #2 | recommended (4+) |
| `screenshots/03-tray-menu.png` | 1920×1080 | Screenshots › Desktop, #3 | recommended |
| `screenshots/04-about-window.png` | 1920×1080 | Screenshots › Desktop, #4 | recommended |
| `screenshots/05-settings-window.png` | 1920×1080 | Screenshots › Desktop, #5: **leave out unless recaptured from the MSIX build** (step 6 below) | optional |
| `images/poster-art-1440x2160.png` | 1440×2160 | Store logos › 2:3 Poster art | optional for apps |
| `images/poster-art-720x1080.png` | 720×1080 | Store logos › 2:3 Poster art (lower-resolution alternative; upload only one) | optional |
| `images/box-art-2160x2160.png` | 2160×2160 | Store logos › 1:1 Box art | optional for apps |
| `images/box-art-1080x1080.png` | 1080×1080 | Store logos › 1:1 Box art (lower-resolution alternative; upload only one) | optional |
| `images/app-tile-icon-300x300.png` | 300×300 | Store logos › 1:1 App tile icon | **strongly recommended** |
| `images/hero-art-1920x1080.png` | 1920×1080 | Trailers and additional assets › Windows 10 or Windows 11 and Xbox image › 16:9 Super hero art | optional (recommended) |
| *(none)* | | Trailers | skipped |
| *(none)* | | Xbox images, Holographic 2:1 image | not applicable |

## Text fields (copy from STORE-LISTING.md)

- [ ] Product name: `EasyAkuru`
- [ ] Description (§2)
- [ ] What's new in this version: **leave blank** for 1.0.0
- [ ] Product features 1–10 (§4), one per box, with no bullets
- [ ] Screenshot captions 1–4, plus 5 only if you recaptured it (§5)
- [ ] Short title, Sort title, Voice title (§8–10)
- [ ] Short description (§11)
- [ ] Additional system requirements: 3 Minimum and 2 Recommended hardware items (§12)
- [ ] Keywords 1–7 (§13)
- [ ] Copyright and trademark info (§14)
- [ ] Additional license terms: **leave blank** (§15)
- [ ] Developed by: `Isuru Ratnayake` (§16)

## Remaining manual steps (outside the Store listings page)

1. **Host the privacy policy publicly first.** Convert `docs/PRIVACY-POLICY.md` into a web
   page at a stable public HTTPS URL (for example GitHub Pages, or a page on
   ratnayake.info). Enter that URL in **Properties › Privacy policy URL**. EasyAkuru reads
   keystrokes through a system-wide hook, so reviewers will expect a privacy policy. Don't
   put the URL in the Description, because Store policy says to put links in their own fields.
2. **Properties**: Category *Productivity* (subcategory *Utilities & tools* if it's offered),
   plus a website and support contact (e.g. isuru.sampath@ratnayake.info) if you want them
   shown.
3. **Age ratings**: complete the IARC questionnaire (see `docs/STORE-SUBMISSION.md` §4).
4. **Packages**: upload `packaging\dist\EasyAkuru_<version>_x64.msixupload`, built with
   `packaging\Build-MsixPackage.ps1 -StoreUpload`.
5. **Submission options › Restricted capabilities**: paste the `runFullTrust` justification
   from `docs/STORE-SUBMISSION.md` §3.
6. **Screenshot 5 (Settings)**: this shows the unpackaged build's "Start EasyAkuru when I sign
   in to Windows" checkbox. The Store build shows a "managed by Windows" note instead. Because
   certification can reject screenshots that don't match the app, **by default leave it out**.
   Four screenshots still meet the recommendation. To include it, install the
   package (Store or sideload), open **Settings…** from the tray, and replace this file with a
   1920×1080 capture of that window before you submit.
7. **Optional Sinhala listing**: the package declares only `en-us`. To use the Sinhala text in
   STORE-LISTING.md, add Sinhala in **Add/remove languages › Manage additional languages**.
   Then select the Product name for it, and upload the same images and screenshots again for
   that language.
8. Preview the listing in Partner Center. Check that the poster and box art look right with
   the Store's bottom-third text overlay, and that the hero art (which has no text) crops
   well. Then **Submit for certification**.

## How these assets were produced

- Screenshots are real captures of the Release build of `EasyAkuru.exe` (version 1.0.0,
  commit `e4b50ab`), running on Windows 11 at 125% scaling. Only the app's own windows and a
  separate demo browser window were captured, never the rest of the desktop. Each capture was
  placed on a plain 1920×1080 background. The tray menu (screenshot 3) is shown at 2.5× scale,
  and About and Settings (screenshots 4–5) at 2×, so they're readable. The tray icon under the
  menu in screenshot 3 is the app's real `SinhalaInput.ico`, drawn onto the canvas, because
  capturing the real notification area would have included other apps' icons.
- Store art was drawn programmatically in the brand maroon (`#8D153A`), with the white **අ**
  mark (Nirmala UI Bold), "EasyAkuru" in Segoe UI Bold and the tagline
  "සිංහල පහසුවෙන් ලියන්න" / "Type Sinhala the easy way". The generator scripts were one-off
  scripts and aren't kept in the repo.
