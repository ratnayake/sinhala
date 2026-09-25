# Lankadeepa/Kelimandala round-trip test — Sport 2

- Source: https://www.kelimandala.lk/latest_news/%E0%B6%B8%E0%B7%90%E0%B6%BD%E0%B7%9A%E0%B7%83%E0%B7%92%E0%B6%BA%E0%B7%8F%E0%B7%80-%E0%B6%B4%E0%B6%BB%E0%B7%8F%E0%B6%A2%E0%B6%BA-%E0%B6%9A%E0%B6%BB%E0%B6%B8%E0%B7%92%E0%B6%B1%E0%B7%8A--%E0%B7%81%E0%B7%8A%E2%80%8D%E0%B6%BB%E0%B7%93-%E0%B6%BD%E0%B6%82%E0%B6%9A%E0%B7%8F%E0%B7%80-%E0%B6%AD%E0%B7%99%E0%B7%80%E0%B7%90%E0%B6%B1%E0%B7%92-%E0%B7%83%E0%B7%8A%E0%B6%AE%E0%B7%8F%E0%B6%B1%E0%B6%BA%E0%B6%A7/20-142663
  (Kelimandala.lk — Lankadeepa's sports sister site; "මැලේසියාව පරාජය කරමින් ශ්‍රී ලංකාව තෙවැනි ස්ථානයට")
- Fetched: 2026-09-25 (PowerShell `Invoke-WebRequest -UseBasicParsing`, decoded as UTF-8, article body extracted from the `<h3 class="f1-l-3...">...<!-- Share -->` region of the raw HTML)
- Author byline in source: නාමල් පතිරගේ

## Verbatim Sinhala text used

මැලේසියාව පරාජය කරමින්  ශ්‍රී ලංකාව තෙවැනි ස්ථානයට

නාමල් පතිරගේ

හොංකොං හිදී පෙරේදා (15) නිමාවට පත් වූ 14 වැනි ආසියානු දැල්පන්දු (නෙට්බෝල්) තරගාවලියේ තෙවැනි ස්ථානය ශ්‍රී ලංකාවට හිමි විය.පෙරේදා පැවැති තෙවැනි ස්ථානය තේරීමේ තරගයේදී ලකුණු 65 -60 කට මැලේසියාව පරාජය කිරීමට ශ්‍රී ලංකාව සමත් විය.

මෙවර තරගාවලිය ජයගත් සිංගප්පූරුව අවසන් මහ තරගයේදී ලකුණු 57 -33 කින් පහසුවෙන් සත්කාරක හොංකොං පරාජය කිරීමට සමත් විය.මුළු තරගාවලියේදී ඔවුන් තරග හතෙන් හයක් දිනා ගත්තේය.එහිදී ශ්‍රී ලංකාව දෙවතාවක් (මූලික හා අවසන් පූර්ව) ඔවුන්ට පරාජය විය. සිංගප්පූරුව ආසියානු කුසලාන දිනා ගත් පස් වැනි අවස්ථාව එයයි.

එහිදී ශ්‍රී ලංකාව වට දෙකක් පෙරමුණ ගනිමින් (12/13 - 19/17 - 15/17 - 19/13) තරගය ජය ගත්තේය.මෙවර තරගාවලියේදී ශ්‍රී ලංකාව මැලේසියාව පරාජයට පත් කළ දෙවැනි අවස්ථාව එය විය.මූලික වටයේදී ශ්‍රී ලංකාව විසින් මැලේසියාව පරාජය කරන ලද්දේ ලකුණු 66 -52 කිනි.

ඒ අනුව මෙවර තරගාවලියේදී ශ්‍රී ලංකාව තරග හතකට සහභාගි වෙමින් ඉන් තරග හතරක් ජය ගෙන එක් තරගයක් සම කරමින් හා සෙසු තරග දෙක පරාජයට පත් කරමින් සිය සහභාගිත්වය අවසන් කර තිබේ. එහිදී ශ්‍රී ලංකාව මුළු තරගාවලිය පුරා ලකුණු 471 ක් රැස් කර තිබේ.ශ්‍රී ලංකාවට එරෙහිව ප්‍රතිවාදිනියන් විසින් එහිදී ලකුණු 380 ක් ලබා තිබේ.

## Method

96 distinct Sinhala words were extracted (punctuation stripped; digits and mixed number/slash tokens treated as SKIP and excluded from the word list, since none needed transliteration; the article's last three paragraphs, covering historical records, were left out of the extraction to keep the sample near the 200–300 word target — the four paragraphs above were used in full). Each word's Singlish spelling was derived deterministically from its Unicode codepoints against the target scheme. All 96 Singlish strings were run through the CLI in the same single piped session as sport-1 (build once, `--no-build` run) and compared codepoint-for-codepoint against the original word (BOM stripped from the first line).

## Status summary

| Status | Count |
|---|---|
| PASS | 68 |
| NEEDS-U1 | 25 |
| NEEDS-U2 | 0 |
| NEEDS-U1+U2 | 0 |
| BUG | 3 |
| SKIP | 0 |
| **Total** | **96** |

The 25 NEEDS-U1 rows are all bare word-final (hal) consonants (e.g. කරමින්, පත්, සමත්, ජයගත්, අවසන්) — expected to pass once Unit 1 lands.

The 3 BUG rows (ස්ථානයට, ස්ථානය, අවස්ථාව) all contain the Sinhala letter ථ (dental aspirated "tha", U+0DAE, as in ස්ථානය "place/position"). This letter has **no Singlish token at all** in the current scheme — it is not in the base consonant table (which only has `th` → dental *unaspirated* ත, U+0DAD) and it is **not** in the Unit 2 "new letters" list either (which covers ඨ/Th and ඪ/Dh, the retroflex aspirates, but not the dental aspirate ථ). With the best-effort spelling `sthaana...` the engine currently substitutes plain ත for the missing ථ, e.g. `sthaanayaTa` → ස්තානයට instead of ස්ථානයට. This is a real scheme gap distinct from the two units already in flight — flagging it for the coordinator to fold into the letter set (a common everyday word, "ස්ථානය" = "place/station", uses it).

## Checklist (BUG rows)

- [ ] ස්ථානයට — missing Singlish token for dental aspirated ථ (U+0DAE); typed `sthaanayaTa`, got ස්තානයට (plain ත) instead of ස්ථානයට
- [ ] ස්ථානය — same root cause as above; typed `sthaanaya`, got ස්තානය instead of ස්ථානය
- [ ] අවස්ථාව — same root cause; typed `avasthaava`, got අවස්තාව instead of අවස්ථාව
