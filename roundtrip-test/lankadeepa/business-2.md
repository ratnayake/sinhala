# Lankadeepa round-trip test — Business #2

- **Source URL:** https://www.lankadeepa.lk/business/අත්කරගත්-ආර්ථික-ස්ථාවරත්වය-දිගුකාලීන-වර්ධනයකට-හැරවිය-යුතුයි/9-698318
- **Fetch date:** 2026-09-25
- **Fetch method:** PowerShell `Invoke-WebRequest -UseBasicParsing`, response bytes decoded as UTF-8, article-body `<p>` tags extracted from the raw HTML (WebFetch was not used, per instructions, to avoid paraphrasing).
- **Excerpt used:** first 5 paragraphs of the article body (227 words).

## Verbatim Sinhala text used

> ශ්‍රී ලංකාව මේ වනවිට අත්කරගෙන ඇති ආර්ථික ස්ථාවරත්වය තිරසර ආයෝජන, ඉහළ ඵලදායිතාව සහ දිගුකාලීන ආර්ථික වර්ධනයක් බවට පරිවර්තනය කළ යුතු බව ශ්‍රී ලංකා මහ බැංකු අධිපති ආචාර්ය නන්දලාල් වීරසිංහ මහතා පසුගියදා ප්‍රකාශ කළේය.
>
> ඒ සඳහා සංවර්ධන මූල්‍ය ආයතන, මූල්‍ය ආයතන, ප්‍රතිපත්ති සම්පාදකයන් සහ පෞද්ගලික අංශය අතර ශක්තිමත් හවුල්කාරීත්වයක් අවශ්‍ය බවත්, එමගින් ආයෝජන සඳහා පවතින මූල්‍ය හා අවදානම් හිඩැස් පියවා පෞද්ගලික ප්‍රාග්ධනය වැඩි වශයෙන් ආකර්ෂණය කරගත හැකි බවත් ඔහු සඳහන් කළේය.
>
> ආසියානු හා පැසිෆික් කලාපයේ සංවර්ධන මූල්‍ය ආයතනවල 49 වැනි වාර්ෂික මහා සභා රැස්වීමේදී පෙරේදා (23) අදහස් දක්වමින් මහ බැංකු අධිපතිවරයා මේ බව පැවසීය.
>
> ශ්‍රී ලංකාවේ ආර්ථික අර්බුදයෙන් පසු සාර්ව ආර්ථික ස්ථාවරත්වය යළි ඇතිකර ගැනීමෙන් රටේ ඉදිරි සංවර්ධන අදියර සඳහා ශක්තිමත් පදනමක් සකස් වී ඇති බවත්, එම ස්ථාවරත්වය දැන් තිරසර ආයෝජන සහ දිගුකාලීන වර්ධනයක් බවට පරිවර්තනය කිරීම කෙරෙහි අවධානය යොමු කළ යුතු බවත් ඔහු පැවසීය. මෙම තත්වය යටතේ කලාපීය සම්බන්ධතා ශක්තිමත් කිරීම සහ ආර්ථික ඒකාබද්ධතාව ගැඹුරු කිරීම ශ්‍රී ලංකාව ඇතුළු රටවලට නව ආයෝජන අවස්ථා ඇති කිරීමට වැදගත් වන බවද මහ බැංකු අධිපතිවරයා සඳහන් කළේය.
>
> කලාපීය ඒකාබද්ධතාව වෙළෙඳාම ඉහළ නැංවීමකට පමණක් සීමා නොවන බවත්, ආයෝජන හා මූල්‍ය සම්බන්ධතා ශක්තිමත් කිරීම, සැපයුම් දාම වඩාත් සම්බන්ධ කිරීම සහ වෙළෙඳපොළ අතර සම්බන්ධතාව වැඩි කිරීමද ඊට ඇතුළත් වන බවත් ඔහු පැවසීය. විශේෂයෙන් කලාපීය සැපයුම් දාම ශක්තිමත් කිරීමෙන් විවිධ මූලාශ්‍රවලින් අවශ්‍ය අමු ද්‍රව්‍ය ලබා ගැනීමටත්, කුඩා හා මධ්‍ය පරිමාණ ව්‍යාපාරවලට කලාපීය ව්‍යාපාර ජාල සමග එක්ව නව වෙළෙඳපොළට පිවිසීමටත් හැකි වන බවද ඔහු සඳහන් කළේය.

## Method

Same method as business-1.md: Singlish spellings derived directly from `src/SinhalaInput.Core/Transliteration/RuleTable.cs`'s character mapping plus the U1 (word-final hal) and U2 (new letters) conventions from the coordinator brief. All 141 distinct words (139 Sinhala words + 2 digit tokens) were piped through the built CLI in a single run and compared codepoint-for-codepoint against the source.

## Status summary

| Status | Count |
|---|---|
| PASS | 101 |
| NEEDS-U1 | 28 |
| NEEDS-U2 | 5 |
| NEEDS-U1+U2 | 1 |
| BUG | 4 |
| SKIP | 2 |
| **Total** | **141** |

## Checklist of BUG rows

- [ ] ආචාර්ය (aachaarya) — source spells the rakaransaya+yansaya cluster as `ර්ය` (no ZWJ), but the engine's r/y-conjunct rule always inserts a ZWJ when `y` follows any consonant+virama (including `r`), producing `ර්‍ය`. Standard Sinhala orthography for this word usually does use the yansaya ligature, so the source is likely missing it — the engine has no way to produce the ZWJ-less form. (Same class of issue as `කාර්ය` in business-1.)
- [ ] ආර්ථික (aarthika) — contains ථ (U+0DAE, aspirated dental "th"), absent from `RuleTable.cs` and outside the U1/U2 scope. Best effort ("th") renders as ත instead. Appears twice in this excerpt.
- [ ] ස්ථාවරත්වය (sthaavarathvaya) — same ථ gap as above. Appears twice in this excerpt.
- [ ] අවස්ථා (avasthaa) — same ථ gap as above.

## Notes

- The pure-digit tokens `49` and `23` are recorded as SKIP.
- `සඳහන්` is the one NEEDS-U1+U2 row: it needs both `ඳ` (zd, U2) and a word-final virama on `න්` (U1).
- All other NEEDS-U2 rows need `ඳ` (zd, in `වෙළෙඳ...` words) or `ඹ` (zb, in `ගැඹුරු`).
- All NEEDS-U1 rows are words ending in a bare consonant + virama (e.g. `අවදානම්`, `සකස්`, `දැන්`, `ගැනීමෙන්`) — these will flip to PASS once Unit 1 lands word-final hal.
