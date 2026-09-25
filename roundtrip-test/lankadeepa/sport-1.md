# Lankadeepa/Kelimandala round-trip test — Sport 1

- Source: https://www.kelimandala.lk/top_story/%E0%B7%81%E0%B7%8A%E2%80%8D%E0%B6%BB%E0%B7%93-%E0%B6%BD%E0%B6%82%E0%B6%9A%E0%B7%8F%E0%B7%80%E0%B6%A7--100---34-%E0%B6%9A-%E0%B6%B4%E0%B7%84%E0%B7%83%E0%B7%94-%E0%B6%A2%E0%B6%BA%E0%B6%9A%E0%B7%8A/33-142659
  (Kelimandala.lk — Lankadeepa's sports sister site; "ශ්‍රී ලංකාවට 100 - 34 ක පහසු ජයක්")
- Fetched: 2026-09-25 (PowerShell `Invoke-WebRequest -UseBasicParsing`, decoded as UTF-8, article body extracted from the `<h3 class="f1-l-3...">...<!-- Share -->` region of the raw HTML)
- Author byline in source: නාමල් පතිරගේ

## Verbatim Sinhala text used

ශ්‍රී ලංකාවට  100 - 34 ක පහසු ජයක්

නාමල් පතිරගේ

ඉන්දියාව ලකුණු100 - 34 කින් පරාජයට පත් කරමින් හොංකොංහිදී පැවැත්වෙන 14 වැනි ආසියානු දැල්පන්දු (නෙට්බෝල්) තරගාවලියේ මූලික වටයේ ඊයේ (11) පැවැති තරගයකදී ශ්‍රී ලංකාව පහසු ජයක් වාර්තා කළේය.එය මෙම තරගාවලියේ ශ්‍රී ලංකාව ලද දෙවැනි ජයයි.

ප්‍රබල ප්‍රතිවාදිනියක් නොවන බැවින් ඉන්දීය කණ්ඩායම හමුවේ ඊයේ තරගයේ වට හතරේදීම ශ්‍රී ලංකාව වඩාත් පහසුවෙන් ලකුණු 25/08 - 29/11 - 24/06 - 22/09 ක් ලෙස පෙරමුණ ගනිමින් එම ජය වාර්තා කර සිටියේය.මීට පෙර මූලික වටයේ තරගයකදී ඉන්දියාව මලයාසියාව හමුවේ ලකුණු101- 27 කින් මෙවැනිම දරුණු පරාජයක් ලබා තිබේ.ඉන්දියාව මේ වන විට ජය ගත් එකම තරගය වන්නේ මාලදිවයින ලකුණු 60 - 41 ක් ලෙස පරාජය කිරීමයි.

ඊයේ වඩාත්ම අවධානය දිනා ගත් මූලික වටයේ තරගයක් වූ සිංගප්පූරුව හා මලයාසියාව අතර තරගයෙන් මලයාසියාව එය ජය ගත්තේ ලකුණු 52 - 46 ක් ලෙසය.මලයාසියාව පැවැති මූලික වටයේ තරගයකදී ශ්‍රී ලංකාවට පරාජය වූයේ ලකුණු 66 -52 කටය.

අද (12) මූලික වටයේ සිය අවසන් තරගයට ශ්‍රී ලංකාව මාලදිවයිනට එරෙහිව සහභාගී වේ. ඊයේ පළමු වතාවට භාෂි උඩගෙදර හා තිමී මේරී වසන්තප්‍රිය ශ්‍රී ලංකා කණ්ඩායමේ ක්‍රීඩා කළහ.

## Method

103 distinct Sinhala words were extracted (punctuation stripped; digits and mixed number/slash tokens treated as SKIP and excluded from the word list, since none needed transliteration). Each word's Singlish spelling was derived deterministically from its Unicode codepoints against the target scheme (base consonant/vowel table, inherent-vowel-as-`a` convention, ZWJ conjuncts, anusvara-as-`M`). All 103 Singlish strings were run through the CLI in one piped session (build once, `--no-build` run) and compared codepoint-for-codepoint against the original word (BOM stripped from the first line).

## Status summary

| Status | Count |
|---|---|
| PASS | 86 |
| NEEDS-U1 | 17 |
| NEEDS-U2 | 0 |
| NEEDS-U1+U2 | 0 |
| BUG | 0 |
| SKIP | 0 |
| **Total** | **103** |

All 17 mismatches are words whose Sinhala spelling ends in a bare (hal) consonant — e.g. ජයක් (jayak), නාමල් (naamal), කින් (kin), පත් (path), වඩාත් (vaDaath) — where the current engine leaves the final consonant without a virama. These are expected to become PASS once Unit 1's word-final hal fix lands. No new-letter (Unit 2) gaps and no other bugs were found in this article.

## Checklist (BUG rows)

None — no BUG-status rows in this article.
