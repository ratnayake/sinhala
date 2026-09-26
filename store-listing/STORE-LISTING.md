# EasyAkuru: Partner Center Store listing (paste-ready)

This file has a value for every field on the Partner Center **Store listings › English
(United States)** page. Fields are in the order the page shows them. The listing's
certification and packaging background is in [`docs/STORE-SUBMISSION.md`](../docs/STORE-SUBMISSION.md),
and [`UPLOAD-CHECKLIST.md`](UPLOAD-CHECKLIST.md) maps each image file to its upload slot.

- App: **EasyAkuru**, Store ID `9P65C2RS1N87`, published by **Ratcon**, author Isuru Ratnayake.
- Field limits were checked against Microsoft Learn on 2026-09-26:
  [Add and edit Store listing info (MSIX)](https://learn.microsoft.com/windows/apps/publish/publish-your-app/msix/add-and-edit-store-listing-info),
  [App screenshots, images, and trailers (MSIX)](https://learn.microsoft.com/windows/apps/publish/publish-your-app/msix/screenshots-and-images).
  Keywords, copyright, license terms and "Developed by" aren't described on the MSIX page any
  more. Their limits come from the same docs set's
  [PWA Store listing page](https://learn.microsoft.com/windows/apps/publish/publish-your-app/pwa/add-and-edit-store-listing-info),
  which describes the same Partner Center form. The rendered Learn page may cut those sections off; they are in the source, [`hub/apps/publish/publish-your-app/pwa/add-and-edit-store-listing-info.md`](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/publish/publish-your-app/pwa/add-and-edit-store-listing-info.md) (sections "Keywords", "Copyright and trademark info", "Additional license terms", "Developed by").
- **Length** means characters as Partner Center counts them (UTF-16 code units, including
  spaces and line breaks). Every value is inside a code block, so copy the block's contents
  and nothing else.
- Store policy: descriptions are **plain text**. Don't use HTML, Markdown, code or URLs, and put
  links in their own fields (privacy policy URL, website, support). The copy below follows
  that rule and makes no claims that can't be checked (no "#1", "best" or "fastest").

## Summary of fields and lengths

| # | Field | Required? | Limit | This listing |
|---|---|---|---|---|
| 1 | Product name | Yes (chosen from reserved names) | reserved name | `EasyAkuru` |
| 2 | Description | **Yes** | 10,000 chars, plain text | 2,133 chars |
| 3 | What's new in this version | No (leave blank on first submission) | 1,500 chars | blank for v1.0.0; template 199 chars |
| 4 | Product features | No | up to 20, ≤200 chars each | 10 features, longest 105 chars |
| 5 | Screenshots (Desktop) | **Yes** (at least 1; 4+ recommended) | up to 10 desktop, PNG, ≥1366×768, ≤50 MB; caption ≤200 chars | 5 at 1920×1080, captions ≤160 chars |
| 6 | Store logos | Recommended | 2:3 poster 720×1080 or 1440×2160; 1:1 box art 1080×1080 or 2160×2160; 1:1 app tile icon 300×300 | all provided |
| 7 | Trailers | No | MP4/MOV 1920×1080 + 1920×1080 PNG thumbnail | skipped |
| 7a | 16:9 Super hero art | No (recommended) | PNG 1920×1080 or 3840×2160, **no text** | provided, 1920×1080 |
| 8 | Short title | No | 50 chars | 9 chars |
| 9 | Sort title | No | 255 chars | 10 chars |
| 10 | Voice title | No | 255 chars | 10 chars |
| 11 | Short description | No (recommended) | 1,000 chars; keep under 270 | 225 chars |
| 12 | Additional system requirements | No | up to 11 items each for Minimum and Recommended hardware, ≤200 chars each | 3 minimum and 2 recommended items |
| 13 | Keywords (formerly "Search terms") | No | up to 7, ≤40 chars each, ≤21 words in total | 7 keywords, 13 words, longest 19 chars |
| 14 | Copyright and trademark info | No | 200 chars | 35 chars |
| 15 | Additional license terms | No | 10,000 chars or one URL | blank (Standard Application License Terms) |
| 16 | Developed by | No | 255 chars | 15 chars |

---

## 1. Product name

Pick **EasyAkuru** from the drop-down. It's the only reserved name, and it matches
`<DisplayName>` in `packaging/AppxManifest.xml`.

## 2. Description

Limit 10,000 characters of plain text. **Length: 2,133.**

```text
EasyAkuru lets you type Sinhala anywhere on Windows using the keyboard you already have. Type words the way they sound in Latin letters ("Singlish"), and EasyAkuru transliterates them into Sinhala Unicode as you type. For example, "mama" becomes මම, "api" becomes අපි and "siMhala" becomes සිංහල.

It works in the apps you already use, including Word and other Office apps, web browsers, email, chat apps and Notepad. You don't need to switch keyboard layouts or install a language pack.

HOW IT WORKS
While you type a word, a small suggestion box appears next to your cursor. It shows the Sinhala spelling and any numbered alternatives. Press Space, Enter or a punctuation key to accept the top suggestion, or press 1 to 9 to pick a different one. Press Esc to keep the Latin letters you typed. EasyAkuru remembers the alternatives you pick, so they appear first next time.

EasyAkuru runs quietly in the system tray. Press Ctrl+Space, or use the tray menu, whenever you want to type ordinary English, and press it again to go back to Sinhala. The tray menu also opens Settings and About.

PRIVACY AND HOW EASYAKURU USES YOUR KEYBOARD
To convert text in any app, EasyAkuru uses a standard Windows system-wide keyboard hook to read the keys you type. All processing happens on your PC, in memory. EasyAkuru has no network features: it never sends, stores or logs your keystrokes. The only things it saves are your settings and the word choices you make in the suggestion box, both kept in your own Windows user profile. EasyAkuru tries to detect password fields and leaves them alone. It recognizes standard Windows password boxes but may not recognize some custom-built ones. There are no ads, no accounts and no analytics.

STARTS WHEN YOU SIGN IN
After you open EasyAkuru for the first time, Windows starts it automatically each time you sign in, so Sinhala typing is always ready. You can turn this off at any time in Windows Settings > Apps > Startup, or on the Startup apps page in Task Manager.

Whether you're messaging family, writing a document or posting online, EasyAkuru lets you type in Sinhala as easily as in English.
```

## 3. What's new in this version

Limit 1,500 characters. **Leave this blank for the first submission (1.0.0)**, as Microsoft's
guidance says. Use the template below for later updates. **Template length: 199.**

```text
Version 1.0.1: [one line per user-visible change, e.g. "Improved suggestions for words with ට/ත"]. EasyAkuru still processes everything on your PC and has no network access. Thanks for your feedback!
```

## 4. Product features

Up to 20 features, ≤200 characters each. Don't add bullets, because the Store adds them. Paste
one line into each **Feature** box.

| # | Feature (paste the text in the code block) | Length |
|---|---|---|
| 1 | `Type Sinhala Unicode in any Windows app by typing Singlish (phonetic Latin letters)` | 83 |
| 2 | `Suggestion box next to your cursor, with numbered alternatives you pick with 1–9` | 80 |
| 3 | `Remembers the spellings you choose and suggests them first next time` | 68 |
| 4 | `Works with your normal keyboard: no layout switching or language pack needed` | 76 |
| 5 | `Turn Sinhala typing on or off instantly with Ctrl+Space or from the tray menu` | 77 |
| 6 | `Supports anusvara, retroflex letters, sanyaka letters, yansaya and rakaransaya` | 78 |
| 7 | `Esc keeps the Latin text you typed; Backspace edits the word you're typing` | 74 |
| 8 | `Runs quietly in the system tray and starts when you sign in (turn this off in Settings > Apps > Startup)` | 104 |
| 9 | `Private by design: everything is processed on your PC, with no network access, accounts, ads or analytics` | 105 |
| 10 | `Tries to detect password fields and leaves them alone (standard Windows password boxes)` | 87 |

Longest: 105 characters (feature 9).

## 5. Screenshots

Desktop, PNG, 1366×768 or larger (these are 1920×1080), ≤50 MB each, up to 10. The caption
limit is 200 characters. Upload them in this order (drag to reorder after uploading). These
are real captures of the Release build, placed on a plain background. As Microsoft's guidance
asks, they have no extra logos or marketing text, and the important content is in the top
two-thirds.

| Order | File | Caption (paste) | Length |
|---|---|---|---|
| 1 | `screenshots/01-typing-with-suggestions.png` | `Type Singlish and a suggestion box shows the Sinhala word and its alternatives. Press Space to accept, or a number to pick another.` | 131 |
| 2 | `screenshots/02-finished-sinhala-text.png` | `Real Sinhala Unicode text, typed in an ordinary app with a regular keyboard.` | 76 |
| 3 | `screenshots/03-tray-menu.png` | `EasyAkuru runs in the system tray. Turn Sinhala typing on or off, or open Settings and About.` | 93 |
| 4 | `screenshots/04-about-window.png` | `About EasyAkuru: version, publisher and author.` | 47 |
| 5 (optional; see note) | `screenshots/05-settings-window.png` | `Settings: turn Sinhala typing on or off and check the Ctrl+Space hotkey. In the Store version, start at sign-in is managed in Windows Settings > Apps > Startup.` | 160 |

> **Screenshot 5: recapture it or leave it out.** Certification can reject screenshots that show
> UI the customer won't get. By default, upload **1–4** and add 5 only after recapturing it
> from an installed MSIX build. These screenshots came from the unpackaged Release build, which
> has its own "Start EasyAkuru when I sign in to Windows" checkbox. In the Store (MSIX) build,
> that checkbox is replaced by a note that says start at sign-in is managed in Windows
> Settings › Apps › Startup (see `docs/STORE-SUBMISSION.md` §6). The caption says this. You
> could instead recapture screenshot 5 from an installed Store/sideloaded build before
> submitting (see `UPLOAD-CHECKLIST.md`).
>
> In screenshots 1 and 2 the text is typed into a plain demo page ("EasyAkuru demo") opened in
> a browser app window. The Sinhala in them is exactly what EasyAkuru produced from these
> keystrokes: `aayuboovan! oyaata kohomada?` / `ada kaalaguNaya hozdayi.` /
> `api heta muhudu weraLata yamu.` / `mama siMhalen liyanna kaemathiyi.` / `sthuthiyi!`

## 6. Store logos

| Slot | File | Size | Notes |
|---|---|---|---|
| 2:3 Poster art | `images/poster-art-1440x2160.png` (or `poster-art-720x1080.png`) | 1440×2160 / 720×1080 | Logo, "EasyAkuru" and both taglines are in the top two-thirds. The bottom third is only a decorative keycap pattern, because the Store may overlay text there. White on #8D153A has a contrast ratio of about 9:1 (the requirement is 4.5:1). |
| 1:1 Box art | `images/box-art-2160x2160.png` (or `box-art-1080x1080.png`) | 2160×2160 / 1080×1080 | Same layout. Title and tagline are in the top two-thirds. |
| 1:1 App tile icon | `images/app-tile-icon-300x300.png` | 300×300 | Full-bleed maroon with the white අ, the same as the package tile. **Strongly recommended**: the Store uses it instead of the package image. |

Leave "Only use images I upload here…" unchecked unless the package images look wrong in the
Store preview.

## 7. Trailers and additional assets

- **Trailers**: skipped (optional).
- **16:9 Super hero art**: `images/hero-art-1920x1080.png` (1920×1080). Microsoft requires
  this image to have **no title or text** and no app UI. It shows only the logo tile on a
  keycap pattern, with the important content centred and above the bottom third. The logo is
  a single Sinhala letter. If certification treats that as "text", upload the listing without
  hero art, because it's optional.
- **Xbox images / Holographic**: not applicable (desktop-only app).

## Supplemental fields

### 8. Short title

Limit 50 characters. **Length: 9.**

```text
EasyAkuru
```

### 9. Sort title

Limit 255 characters. An alternative spelling that helps people find the app in search.
**Length: 10.**

```text
Easy Akuru
```

### 10. Voice title

Limit 255 characters. **Length: 10.**

```text
Easy Akuru
```

### 11. Short description

Limit 1,000 characters (keep under 270, because some views show only the first 270).
It's worded differently from the Description so the listing doesn't repeat itself.
**Length: 225.**

```text
Type Sinhala in any Windows app by typing Singlish on your usual keyboard. Suggestions appear next to your cursor, Ctrl+Space switches back to English, and everything stays on your PC, with no network access, accounts or ads.
```

### 12. Additional system requirements

Up to 11 items for each list, ≤200 characters per item. Don't add bullets. The Windows version
and architecture (Windows 10 version 1809 or later, x64) come from the package and the
**Properties › System requirements** section, so they aren't repeated here.

**Minimum hardware**

| Item (paste) | Length |
|---|---|
| `A physical keyboard with a Latin-letter (QWERTY-style) layout` | 61 |
| `Touch and on-screen keyboards are not supported` | 47 |
| `A Sinhala font, such as Nirmala UI or Iskoola Pota (included with Windows)` | 74 |

**Recommended hardware**

| Item (paste) | Length |
|---|---|
| `A full-size physical keyboard with an English (US) layout` | 57 |
| `Apps that support Unicode text (most current Windows apps do)` | 61 |

> The touch-keyboard line is deliberately conservative. EasyAkuru reads physical key presses
> through a low-level keyboard hook, and it hasn't been tested with the Windows touch keyboard.
> Remove that line if touch input is tested and works.

### 13. Keywords (formerly "Search terms")

Up to 7 keywords, ≤40 characters each, ≤21 words in total. Customers don't see them. Only use
relevant terms. **7 keywords, 13 words.**

| # | Keyword (paste) | Length | Words |
|---|---|---|---|
| 1 | `Sinhala` | 7 | 1 |
| 2 | `Singlish` | 8 | 1 |
| 3 | `Sinhala keyboard` | 16 | 2 |
| 4 | `Sinhala typing` | 14 | 2 |
| 5 | `Sinhala Unicode` | 15 | 2 |
| 6 | `phonetic keyboard` | 17 | 2 |
| 7 | `Sri Lanka Sinhala` | 17 | 3 |

(`docs/STORE-SUBMISSION.md` §4 also lists *transliteration*, *Sinhala input*, *Sri Lanka* and
*akuru*. These were dropped to stay within the 7-keyword limit. "Transliterates" and
"Sri Lanka" still appear in the description and keyword 7. If Partner Center's keyword
suggestions do better, swap *phonetic keyboard* for *transliteration*.)

### 14. Copyright and trademark info

Limit 200 characters. **Length: 35.**

```text
© 2026 Ratcon. All rights reserved.
```

### 15. Additional license terms

Limit 10,000 characters or a single URL. **Leave blank.** EasyAkuru is then licensed under
Microsoft's **Standard Application License Terms**. Only fill this in if Ratcon adopts its own
EULA.

### 16. Developed by

Limit 255 characters. "Published by" shows the account's publisher display name (**Ratcon**)
automatically. **Length: 15.**

```text
Isuru Ratnayake
```

---

## Sinhala (සිංහල, `si`) listing: optional

> **Package language status.** `packaging/AppxManifest.xml` declares only
> `<Resource Language="en-us" />`, so Partner Center lists **English (United States)** as the
> only language the package supports. Use the Sinhala text below **only if you add a Sinhala
> Store listing**. You can do that in either of two ways:
>
> 1. Add `si` (or `si-lk`) as a package language by adding a `<Resource Language="si-lk" />`
>    entry, plus any localized resources, then rebuild and upload. Sinhala then appears under
>    *Languages supported by your packages*. (This is a packaging change and isn't part of this
>    listing PR.)
> 2. Or, with no package change, add Sinhala in **Add/remove languages › Manage additional
>    languages** as an *additional Store listing language*. Microsoft Learn says listings "in
>    additional languages which aren't supported by your packages" are allowed. For such a
>    language you must also choose the **Product name** (EasyAkuru) yourself, because no package
>    supplies it.
>
> Either way, screenshots and logos must be uploaded again for each language. You can reuse
> the same PNGs.

### Description (si)

Limit 10,000. **Length: 1,863.**

```text
EasyAkuru මඟින් ඔබ දැනටමත් භාවිතා කරන යතුරුපුවරුවෙන්ම Windows හි ඕනෑම තැනක සිංහලෙන් ටයිප් කළ හැක. වචනය උච්චාරණය වන ආකාරයට ලතින් අකුරින් ("සිංග්ලිෂ්") ටයිප් කරන්න; ඔබ ටයිප් කරන විටම EasyAkuru එය සිංහල යුනිකෝඩ් බවට පරිවර්තනය කරයි. උදාහරණයක් ලෙස "mama" → මම, "api" → අපි, "siMhala" → සිංහල.

Word ඇතුළු Office යෙදුම්, වෙබ් බ්‍රව්සර්, ඊමේල්, chat යෙදුම් සහ Notepad වැනි ඔබ දැනටමත් භාවිතා කරන යෙදුම් සමඟ එය ක්‍රියා කරයි. යතුරුපුවරු පිරිසැලසුම මාරු කිරීමට හෝ භාෂා පැකේජයක් ස්ථාපනය කිරීමට අවශ්‍ය නැත.

ක්‍රියා කරන ආකාරය
ඔබ වචනයක් ටයිප් කරන විට, කර්සරය අසල කුඩා යෝජනා කවුළුවක සිංහල අක්ෂර වින්‍යාසය සහ අංක සහිත විකල්ප පෙන්වයි. ප්‍රධාන යෝජනාව තහවුරු කිරීමට Space, Enter හෝ විරාම ලකුණක් ඔබන්න; වෙනත් විකල්පයක් තෝරා ගැනීමට 1–9 අංකයක් ඔබන්න; ටයිප් කළ ලතින් අකුරු එලෙසම තබා ගැනීමට Esc ඔබන්න. ඔබ තෝරන විකල්ප EasyAkuru මතක තබා ගන්නා බැවින් ඊළඟ වතාවේ ඒවා මුලින්ම පෙන්වයි.

EasyAkuru system tray එකේ නිහඬව ක්‍රියාත්මක වේ. සාමාන්‍ය ඉංග්‍රීසියෙන් ටයිප් කිරීමට අවශ්‍ය විට Ctrl+Space ඔබන්න (හෝ tray මෙනුව භාවිතා කරන්න); නැවත සිංහලට මාරු වීමට එය නැවත ඔබන්න.

පෞද්ගලිකත්වය
ඕනෑම යෙදුමක අකුරු පරිවර්තනය කිරීම සඳහා EasyAkuru ඔබ ඔබන යතුරු කියවීමට Windows හි සම්මත පද්ධති-පුරා keyboard hook එකක් භාවිතා කරයි. සියලු සැකසීම් ඔබගේ පරිගණකය තුළම, මතකයේ පමණක් සිදු වේ. EasyAkuru හි කිසිදු ජාල විශේෂාංගයක් නැත - ඔබගේ යතුරු එබීම් කිසි විටෙකත් යවන්නේ, ගබඩා කරන්නේ හෝ ලොග් කරන්නේ නැත. එය සුරකින්නේ ඔබගේ සැකසුම් සහ ඔබ තෝරාගත් වචන පමණක් වන අතර ඒවා ඔබගේම Windows පරිශීලක ගිණුම තුළ තැන්පත් වේ. මුරපද ක්ෂේත්‍ර හඳුනාගෙන ඒවා මඟ හැරීමට EasyAkuru උත්සාහ කරයි; සම්මත Windows මුරපද කොටු හඳුනා ගන්නා නමුත් සමහර විශේෂයෙන් සාදන ලද ඒවා හඳුනා නොගැනීමට ඉඩ ඇත. දැන්වීම්, ගිණුම් හෝ analytics නැත.

ඔබ පුරනය වන විට ආරම්භ වේ
EasyAkuru පළමු වරට විවෘත කළ පසු, ඔබ Windows වෙත පුරනය වන සෑම විටම එය ස්වයංක්‍රීයව ආරම්භ වේ. මෙය ඕනෑම වේලාවක Windows Settings > Apps > Startup හි හෝ Task Manager හි Startup apps කොටසේ අක්‍රිය කළ හැක.
```

### Product features (si)

Each ≤200 characters.

| # | Feature (paste) | Length |
|---|---|---|
| 1 | `සිංග්ලිෂ් ටයිප් කර ඕනෑම Windows යෙදුමක සිංහල යුනිකෝඩ් ලියන්න` | 60 |
| 2 | `කර්සරය අසලම යෝජනා කවුළුව; අංක 1–9 මඟින් විකල්ප තෝරන්න` | 53 |
| 3 | `ඔබ තෝරන අක්ෂර වින්‍යාසය මතක තබාගෙන ඊළඟ වතාවේ මුලින්ම පෙන්වයි` | 60 |
| 4 | `සාමාන්‍ය යතුරුපුවරුව සමඟම ක්‍රියා කරයි - පිරිසැලසුම් මාරු කිරීමක් අවශ්‍ය නැත` | 76 |
| 5 | `Ctrl+Space මඟින් සිංහල ටයිප් කිරීම ක්ෂණිකව සක්‍රිය/අක්‍රිය කරන්න` | 64 |
| 6 | `system tray එකේ නිහඬව ක්‍රියා කරයි; Windows වෙත පුරනය වන විට ආරම්භ වේ` | 69 |
| 7 | `සියල්ල ඔබගේ පරිගණකය තුළම - ජාල ප්‍රවේශයක්, ගිණුම්, දැන්වීම් නැත` | 63 |

### Keywords (si)

Up to 7, ≤40 characters each, ≤21 words in total. **7 keywords, 10 words.**

| # | Keyword (paste) | Length | Words |
|---|---|---|---|
| 1 | `සිංහල` | 5 | 1 |
| 2 | `සිංග්ලිෂ්` | 9 | 1 |
| 3 | `සිංහල යතුරුපුවරුව` | 17 | 2 |
| 4 | `සිංහල ටයිප් කිරීම` | 17 | 3 |
| 5 | `යුනිකෝඩ්` | 8 | 1 |
| 6 | `Sinhala` | 7 | 1 |
| 7 | `Singlish` | 8 | 1 |

(Sinhala short description, from `docs/STORE-SUBMISSION.md` §4. **Length: 63.**)

```text
සිංග්ලිෂ් ටයිප් කර ඕනෑම තැනක සිංහල යුනිකෝඩ් ලෙස ක්ෂණිකව ලියන්න.
```
