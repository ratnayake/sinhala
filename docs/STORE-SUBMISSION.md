# Microsoft Store submission guide — EasyAkuru

This is the checklist for submitting **EasyAkuru** (internal code name `SinhalaInput`; see the
naming note at the top of `docs/SINHALA-INPUT-TOOL-DESIGN.md`) to the Microsoft Store via
[Partner Center](https://partner.microsoft.com/dashboard). It covers the Partner Center
identity values already reserved for this app, how to build the package Partner Center wants,
the capability-justification text the submission form asks for, the listing copy, and the two
things the Store will not let this app skip: a privacy-policy URL and disclosure of the
auto-start behaviour.

> This checklist assumes the app-identity rename (`EasyAkuru.exe`, new logo/Store assets,
> `packaging/AppxManifest.xml` StartupTask, and a `-StoreUpload` mode on
> `packaging/Build-MsixPackage.ps1`) and the runtime rename (user-facing strings,
> `%AppData%\EasyAkuru\`, the `EasyAkuru` Run-key value) have landed from the sibling
> branches that implement them. If `packaging/Build-MsixPackage.ps1 -StoreUpload` or
> `EasyAkuru.exe` don't exist yet in your checkout, merge those branches first — this document
> does not itself change any code or packaging files.

## 1. Partner Center identity

These values are already reserved in Partner Center for this app. The manifest's `<Identity>`
element must match them exactly — Partner Center issues them, they are not something to pick
independently:

| Field | Value |
|---|---|
| Identity `Name` | `Ratcon.EasyAkuru` |
| Identity `Publisher` | `CN=E7B0A46A-21DB-4DDA-BDA6-CCAB7E8753EA` |
| `PublisherDisplayName` | `Ratcon` |
| Package Family Name (PFN) | `Ratcon.EasyAkuru_swc2syyz6eg42` |
| Store ID | `9P65C2RS1N87` |

Confirm `packaging/AppxManifest.xml`'s `<Identity>` and `<Properties><PublisherDisplayName>`
match this table before packing an upload — a mismatch is rejected by Partner Center at
ingestion, not silently corrected.

## 2. Building the upload package

Build the Store submission package from `packaging/`:

```powershell
cd packaging
.\Build-MsixPackage.ps1 -StoreUpload
```

`-StoreUpload` produces a package suitable for Partner Center ingestion (unsigned or signed
with a placeholder — Partner Center re-signs every package with a Store certificate on
upload, so do not use `-Sign`'s local self-signed certificate for a Store submission). See
`packaging/README.md` for the other build modes (`-Sign` for local/internal distribution,
`-SelfContained:$false` for a framework-dependent build) — those are unrelated to the Store
path and should not be used for the file uploaded to Partner Center.

Before uploading, double-check:

- `Identity/Version` in `AppxManifest.xml` has been bumped from the previous submission
  (Partner Center rejects a re-upload of a version it has already seen).
- The package was built from a clean `Release` publish, not a local dev/debug build.

## 3. `runFullTrust` capability justification

`packaging/AppxManifest.xml` declares `<rescap:Capability Name="runFullTrust" />`. This is a
*restricted* capability, so the Store submission form requires a written justification before
the app is allowed to declare it. Use text along these lines:

> EasyAkuru is a system-wide phonetic (Singlish-to-Sinhala) input tool. It needs
> `runFullTrust` because its core function — transliterating what the user types into Sinhala
> Unicode as they type it, in whatever application currently has focus (Notepad, Word,
> browsers, chat apps, Office, etc.) — requires a low-level, system-wide keyboard hook
> (`SetWindowsHookEx` / `WH_KEYBOARD_LL`) to observe keystrokes across process boundaries, and
> `SendInput` to replace the Latin text the target application already rendered with the
> transliterated Sinhala text. Both APIs are classic Win32 APIs unavailable to a sandboxed
> UWP/AppContainer process; `runFullTrust` (via the desktop bridge) is the only capability
> that grants a packaged app this access. The app does not use `runFullTrust` for anything
> beyond this: it does not access the network, other users' data, or the filesystem outside
> its own per-user settings/dictionary files under `%AppData%\EasyAkuru\`.

## 4. Store listing text

> The final, paste-ready values for every Store listing field (with character limits, screenshots,
> captions and Store art) are in [`store-listing/STORE-LISTING.md`](../store-listing/STORE-LISTING.md),
> with a file-to-field map in [`store-listing/UPLOAD-CHECKLIST.md`](../store-listing/UPLOAD-CHECKLIST.md).
> The draft copy below is kept for reference.

**Category:** Productivity → Utilities & tools (or "Productivity" if the "Utilities & tools"
sub-category is not offered for this app type — pick the closest available match at
submission time).

**Keywords:** Sinhala, Sinhala keyboard, Singlish, Sinhala typing, Sinhala Unicode, transliteration,
Sri Lanka, phonetic keyboard, Sinhala input, akuru

### Short description (English, ≤100 characters for the Store's short-description field)

> Type Sinhala anywhere by typing Singlish — instant Latin-to-Sinhala Unicode transliteration.

### Short description (Sinhala)

> සිංග්ලිෂ් ටයිප් කර ඕනෑම තැනක සිංහල යුනිකෝඩ් ලෙස ක්ෂණිකව ලියන්න.

### Long description (English)

> EasyAkuru is a lightweight background tool that lets you type Sinhala anywhere on Windows —
> Word, browsers, chat apps, email, Notepad, Office — using an ordinary QWERTY keyboard and
> familiar Singlish (Latin-letter phonetic) spelling. As you type a word, EasyAkuru shows a
> small candidate popup near your cursor with the top Sinhala transliteration and numbered
> alternates; press Space, Enter, a digit, or keep typing to commit it. For example, typing
> `mama` gives `මම` and `api` gives `අපි`.
>
> EasyAkuru runs quietly in the system tray, works with the applications you already use (it
> does not require you to switch keyboard layouts or install a language pack), and can be
> paused instantly with a hotkey when you want to type in plain Latin text. It starts
> automatically when you sign in to Windows (you can turn this off at any time in
> Settings › Apps › Startup), and everything it does happens entirely on your device — no
> keystrokes are ever stored or sent anywhere. See the in-app privacy notice or
> [the privacy policy](#7-privacy-policy-url) for details.
>
> Whether you're writing a message to family, drafting a document, or posting online, EasyAkuru
> makes typing fluent Sinhala as easy as typing English.

### Long description (Sinhala)

> EasyAkuru යනු ඔබගේ සාමාන්‍ය QWERTY යතුරුපුවරුව සහ හුරුපුරුදු සිංග්ලිෂ් අක්ෂර වින්‍යාසය
> භාවිතයෙන් Windows මත ඕනෑම යෙදුමක සිංහල භාෂාවෙන් ටයිප් කිරීමට ඉඩ දෙන සැහැල්ලු පසුබිම් මෙවලමකි —
> Word, බ්‍රව්සර, chat යෙදුම්, විද්‍යුත් තැපෑල, Notepad, Office ඇතුළුව. ඔබ වචනයක් ටයිප් කරන
> විට, ඔබගේ කර්සරය අසල දිස්වන කුඩා විකල්ප කවුළුවක ප්‍රධාන සිංහල පරිවර්තනය සහ ඉලක්කම් ලකුණු
> කළ විකල්ප පෙන්වයි. උදාහරණයක් ලෙස, `mama` ටයිප් කිරීමෙන් `මම` ලැබෙන අතර `api` වලින් `අපි`
> ලැබේ.
>
> EasyAkuru system tray එකේ නිහඬව ක්‍රියාත්මක වන අතර, ඔබ දැනටමත් භාවිතා කරන යෙදුම් සමඟ ක්‍රියා
> කරයි (යතුරුපුවරු පිරිසැලසුම මාරු කිරීමට හෝ භාෂා පැකේජයක් ස්ථාපනය කිරීමට අවශ්‍ය නොවේ), සහ ඔබට
> Latin අකුරින් ටයිප් කිරීමට අවශ්‍ය විටෙක hotkey එකකින් ක්ෂණිකව විරාම කළ හැක. ඔබ Windows වෙත
> පිවිසෙන සෑම විටම එය ස්වයංක්‍රීයව ආරම්භ වේ (මෙය ඕනෑම වේලාවක Settings › Apps › Startup හි
> අක්‍රිය කළ හැක), සහ එය සිදු කරන සියල්ල ඔබගේ උපාංගයේම සිදු වේ — කිසිදු යතුරු එබීමක් කිසි විටෙකත්
> ගබඩා කර හෝ වෙනතකට යවා නොමැත.

**Age rating guidance:** answer the Store's age-rating questionnaire (IARC) as a general
productivity/utility app with no user-generated content shared with others, no in-app
purchases, no ads, no chat/social features, and no access to a camera or microphone. It should
qualify for the lowest available rating (typically "3+" / "Everyone" once the questionnaire is
completed) — do not skip the questionnaire itself, since the Store computes the rating from
the answers rather than accepting a self-declared rating.

## 5. Screenshots to capture

Partner Center requires at least one screenshot (recommended: several, at 1366×768 or a
16:9-scaled equivalent that Partner Center's uploader accepts). Capture, on a clean desktop:

1. **Tray icon + context menu** — the app's icon in the notification area with its right-click
   menu open (Enable/Disable, Settings, Exit) visible.
2. **Candidate popup in action** — a Latin word mid-type in a real app (e.g. Notepad or a
   browser text field) with the candidate popup showing the numbered Sinhala alternates.
3. **Before/after in a real document** — a short sentence typed in Singlish next to (or
   followed by) the committed Sinhala Unicode result, in an app someone would recognize
   (Word, Outlook, or a browser).
4. **Settings window** — showing the enable/disable and start-at-sign-in controls, and the
   "managed by Windows" startup note for the packaged build.
5. *(Optional)* the About window, if it names the app version and publisher.

Do not stage screenshots with fabricated third-party branding or content; use generic sample
text.

## 6. Auto-start disclosure

EasyAkuru starts automatically at every Windows sign-in via an MSIX **startup task**
(`desktop:StartupTask`, declared in `packaging/AppxManifest.xml`), not by writing to the
registry or scheduling a task itself — Windows owns this mechanism for packaged apps. Disclose
this plainly in the Store listing description (see §4 above) and be ready to state it again if
asked during Store certification review:

- The app starts at sign-in by default once installed.
- Users can turn this off at any time in **Settings › Apps › Startup** (or Task Manager's
  Startup Apps tab) — the app's own Settings window does not offer a start-at-sign-in toggle
  for the packaged build and instead points the user to that Windows setting, because a
  packaged app cannot write this setting itself.
- Because MSIX packages cannot run arbitrary code at install time, EasyAkuru's first launch
  after installing from the Store is always user-initiated — via the Store's own **Launch**
  button on the product page, or by opening it from the Start menu — never something the
  installer does automatically. The startup task only takes effect starting from the next
  sign-in after that first launch.

## 7. Privacy policy URL

The Store requires a privacy policy URL for any app that processes user input, which EasyAkuru
does (it reads every keystroke via a system-wide keyboard hook in order to transliterate it).
The policy is published at **<https://ratnayake.info/easyakuru/privacy/>** (the web page is
`website/easyakuru/privacy/index.html`; its source text is `docs/PRIVACY-POLICY.md`, and a
Sinhala summary is at <https://ratnayake.info/easyakuru/si/privacy/>). Confirm the URL loads,
then enter it in Partner Center's **Privacy policy URL** field before submitting. Partner
Center will reject a submission that has user-input access declared without one.
