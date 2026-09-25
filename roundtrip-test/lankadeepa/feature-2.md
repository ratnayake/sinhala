# Lankadeepa round-trip test — feature-2 (Visheshanga / features)

**Source:** https://www.lankadeepa.lk/visheshanga/%E0%B6%B6%E0%B7%9C%E0%B6%BD%E0%B7%8A%E0%B6%BD%E0%B6%A7-%E0%B6%B1%E0%B7%92%E0%B6%AF-%E0%B6%B6%E0%B7%9C%E0%B6%BD%E0%B7%8D%E0%B6%BD%E0%B6%B1-%E0%B6%BD%E0%B7%92%E0%B6%BA%E0%B6%B4%E0%B6%AF%E0%B7%8A%E0%B6%A0%E0%B7%8A-%E0%B6%9A%E0%B6%BB%E0%B6%9C%E0%B6%B1%E0%B7%93%E0%B6%B8/26-698194
("බල්ලට නොදා බල්ලන් ලියාපදිංචි කරගැනීම" — "Registering dogs without giving them their due", Visheshanga/features section, on a new circular making dog registration and rabies vaccination mandatory)

**Fetched:** 2026-09-25, via PowerShell `Invoke-WebRequest -UseBasicParsing` decoded as UTF-8 (not WebFetch), extracting the `<div class="article-body sinhala-body">` block and HTML-decoding entities (the source HTML itself already encodes ZWJ conjuncts as literal `&zwj;` entities, e.g. `චක්&zwj;රලේඛයක්`).

**Text used** (title + first ~221 words of the body, verbatim, as published):

> බල්ලට නොදා බල්ලන් ලියාපදිංචි කරගැනීම
>
> කලකට ඉහත මෙරට වාර්තා වූයේ ලංකාවේ බල්ලන්ගේ ජනගහනය කොල්ලන්ගේ ජනගහනය ඉක්මවා ගොස් ඇති බවය. පසුගිය සතියේ බල්ලන් ගැන දැඩි සමාජ කතිකාවතක් ඇති වූයේ උන් පිළිබඳ චක්‍රලේඛයක් නිකුත් වීමත් සමඟමය. අන්තිමට සිදුවූයේ ගැසට් එකද බල්ලටම යෑමය. ඒ කියන්නේ බල්ලන් ගැන නිර්දේශ ඇතුළත් ගැසට් නිවේදනයක්ද නිකුත් වීමෙනි.
>
> තහවුරු වූ වාර්තා අනුව ජල භීතිකා රෝගය මෙරටින් තුරන් කිරීමේ අරමුණින් සුනඛයන් ලියාපදිංචි කිරීම අනිවාර්ය කර ඇත. සුනඛයන් ලියාපදිංචි නොකරන අයට එරෙහිව ලියාපදිංචි ගාස්තුව සමඟ රුපියල් 2000ක දඩයක් නියම කරමින් රාජ්‍ය පරිපාලන, ස්වදේශ කටයුතු, පළාත් සභා හා පළාත් පාලන අමාත්‍යාංශය චක්‍රලේඛයක් නිකුත් කර හමාරය. එමෙන්ම මීට අදාළ ගැසට් නිවේදනයද නිකුත් කර තිබේ.
>
> එම නව චක්‍රලේඛයට අනුව මෙරට සිටින සෑම සුනඛයකුම වාර්ෂිකව ජල භීතිකා රෝගයට එන්නත් කළ යුතු බවටත් ලියාපදිංචි කළ යුතු බවටත් පළාත් පාලන ආයතන වෙත නියෝග නිකුත් කර ඇත. වයස සති හයකට වැඩි සුනඛයන්ට ජල භීතිකා එන්නත ලබා දිය යුතුය. සුනඛයන් ලියාපදිංචි නොකර සිටින්නන්ට ලියාපදිංචි ගාස්තුවට අමතරව රුපියල් 2000ක දඩ මුදලක් ගෙවීමට සිදුවේ. මින් ඉදිරියට පළාත් පාලන ආයතන බල ප්‍රදේශවල සිටින අයාලයේ යන සුනඛයන් වන්ද්‍යාකරණය කිරීමට ඒ ආයතන පියවර ගත යුතුය. එමෙන්ම අමතරව සුනඛ අභිජනන මධ්‍යස්ථාන පවත්වාගෙන යන පුද්ගලයන් අනිවාර්යයෙන්ම බලපත්‍රයක් ගත යුතු අතර බලපත්‍රයක් නොමැතිව සුනඛ පැටවුන් විකිණීමට අවසර හිමි නොවේ.
>
> මේ කවරාකාරයෙන් පැවසුවත් මිනිසාට වඩාත්ම සමීප හිතවතා, සුරතලා, ආරක්ෂකයා සහ ලෙන්ගතුම මිත්‍රයා සුනඛයා යැයි යමෙක් කිවහොත් එය කිසිවිටෙකත් අතිශයෝක්තියක් නොවේ. මන්ද යත් සුරතලා තරම් මිනිසා සමඟ සම්බන්ධතා පවත්වන අන් කිසිදු සිවුපාවකු නොවන නිසාය.

## Method

Every distinct Sinhala word (165) plus one Latin/digit token (`2000`) was spelled in the target Singlish scheme (see `src/SinhalaInput.Core/Transliteration/RuleTable.cs`), built once, then piped through the CLI in a single run and compared codepoint-for-codepoint against the original (BOM on the first output line stripped).

## Status summary (166 rows)

> Round 2: every row now passes (see [CHECKLIST.md](CHECKLIST.md)). The counts below are the Round-1 triage.

| Status | Count |
|---|---|
| PASS | 125 |
| NEEDS-U1 | 33 |
| NEEDS-U2 | 3 |
| BUG | 4 |
| SKIP | 1 |

## BUG checklist

- [x] බල්ලන්ගේ (ballangee, "the dogs'") — typed as `ballangee`. The source word is බල්ල(ball) + ල(l) + න්(n, hal) + ග(ga) + ේ(ee), a plain mid-word cluster of "l"-hal-"n" then "n"-hal-"g" — no special ඟ (nga) letter and no ZWJ. The trie greedily matches the substring "ng" inside "ballangee" as the ඞ digraph (registered as a single consonant token) instead of parsing it as "n" (needing a virama before the following consonant) + "g", so the CLI outputs බල්ලඞේ instead. *(Round 2: fixed, see [CHECKLIST.md](CHECKLIST.md).)*
- [x] කොල්ලන්ගේ (kollangee, "the boys'") — same defect as බල්ලන්ගේ above; output කොල්ලඞේ. *(Round 2: fixed, see [CHECKLIST.md](CHECKLIST.md).)*
- [x] ලෙන්ගතුම (lengathuma, "the most affectionate") — same defect; output ලෙඞතුම. Together with අවසන්ය in *feature-1* (where "ny" collapses to ඤ the same way), this shows the "ny"/"ng" digraph rules in `RuleTable.Consonants` need to back off when the word's actual glyphs are a plain consonant-hal-consonant cluster rather than the dedicated ඤ/ඞ letter — currently the trie has no way to disambiguate the two, and no separator/escape is documented in the scheme. *(Round 2: fixed, see [CHECKLIST.md](CHECKLIST.md).)*
- [x] මධ්‍යස්ථාන (madhyasthaana, "centers") — contains ස්ථ = ස + ් (virama) + ථ (dental aspirated "tha", U+0DAE). ථ has no mapping anywhere in `RuleTable.Consonants` — it is not part of Unit 1 (word-final hal) or Unit 2 (the listed new letters ඟ/ඳ/ඬ/ඹ/ඦ/ඨ/ඪ/ෘ/ඎ/ඥ) scope, it is a separate pre-existing gap. `th` is already taken by ත (dental unaspirated), so there is currently no way to type ථ at all; best-effort attempt `madhyasthaana` produces මධ්‍යස්තාන (ත instead of ථ). *(Round 2: fixed, see [CHECKLIST.md](CHECKLIST.md).)*
