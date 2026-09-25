# Lankadeepa round-trip test — Business #1

- **Source URL:** https://www.lankadeepa.lk/business/අගෝස්තුවේදී-ඇඟලු‍ම්---රෙදිපිළි-අපනයන-ආදායම-සියයට-8-4කින්-පහළ-ගිහින්/9-698313
- **Fetch date:** 2026-09-25
- **Fetch method:** PowerShell `Invoke-WebRequest -UseBasicParsing`, response bytes decoded as UTF-8, article-body `<p>` tags extracted from the raw HTML (WebFetch was not used, per instructions, to avoid paraphrasing).
- **Excerpt used:** first 5 paragraphs of the article body (262 words).

## Verbatim Sinhala text used

> ගෝලීය වෙළෙඳපොළේ ඇඟලු‍ම් සඳහා ඇති ඉල්ලු‍ම අඩුවීම සහ වෙළෙඳපොළ තත්වය වෙනස්වීම හේතුවෙන් ඉකුත් අගෝස්තු මාසයේදී මෙරට ඇඟලු‍ම් හා රෙදිපිළි අපනයන ආදායම සියයට 8.4කින් පහළ ගොස් ඇතැයි අපනයන සංවර්ධන මණ්ඩලයේ සභාපති හා ප්‍රධාන විධායක නිලධාරී මංගල විජේසිංහ මහතා පැවසීය.
>
> පසුගිය වසරේ අගෝස්තු මාසයේදී ඇමෙරිකානු ඩොලර් මිලියන 506.8කට ආසන්නව පැවැති ඇඟලු‍ම් හා රෙදිපිළි අපනයන ආදායම මේ වසරේ අගෝස්තු මාසයේදී ඇමෙරිකානු ඩොලර් මිලියන 464.27ක් දක්වා අඩුවී තිබේ.
>
> ඇමෙරිකාවට සහ එක්සත් රාජධානියට කරන ඇඟලු‍ම් අපනයනද මෙම තත්වය හේතුවෙන් පහළ ගොස් ඇති අතර, අගෝස්තු මාසයේදී ඇමෙරිකා එක්සත් ජනපදයට සිදුකළ ඇඟලු‍ම් අපනයන සියයට 15.96කින්ද, එක්සත් රාජධානියට කළ ඇඟලු‍ම් අපනයන සියයට 11.79කින්ද අඩුවී තිබේ. එම මාසයේදී ඇමෙරිකාවට කළ ඇඟලු‍ම් අපනයන ආදායම ඇමෙරිකානු ඩොලර් මිලියන 177.6ක් පමණ දක්වාත්, එක්සත් රාජධානියෙන් ලැබුණු ඇඟලු‍ම් අපනයන ආදායම ඩොලර් මිලියන 53.6ක් පමණ දක්වාත් පහළ ගොස් ඇත.
>
> ශ්‍රී ලංකාවේ ඇඟලු‍ම් අපනයන ආදායම අඩුවීම මෙරටට පමණක් බලපෑ තත්වයක් නොවන බවත්, ගෝලීය වෙළෙඳපොළේ ඇඟලු‍ම් සඳහා තිබෙන ඉල්ලු‍ම අඩුවීම එයට හේතුව බවත් විජේසිංහ මහතා ප්‍රකාශ කළේය. ලොව පුරා රටවල් මෙම තත්වයට මුහුණ දෙමින් සිටින බවද ඔහු පැවැසුවේය. පසුගිය අගෝස්තු මාසයේ අපනයන කාර්ය සාධනය සම්බන්ධයෙන් මාධ්‍ය දැනුවත් කිරීම සඳහා රජයේ ප්‍රවෘත්ති දෙපාර්තමේන්තුවේ පැවැති මාධ්‍ය හමුවේදී ඔහු මේ බව කියා සිටියේය.
>
> මේ අතර, මෙරට සමස්ත අපනයන ආදායම මේ වසරේ මුල් මාස අට තුළදී පළමු වරට ඇමෙරිකානු ඩොලර් බිලියන 12 සීමාව ඉක්මවා ඇති අතර, ජනවාරි සිට අගෝස්තු දක්වා වූ කාලයේදී සමස්ත අපනයන ආදායම ඇමෙරිකානු ඩොලර් බිලියන 12.01ක් දක්වා ඉහළ ගොස් තිබේ. එය පසුගිය වසරේ එම කාලයේ වාර්තා වූ අපනයන ආදායමට සාපේක්ෂව සියයට 4.26ක වර්ධනයක් බව අපනයන සංවර්ධන මණ්ඩලයේ දත්ත පෙන්වා දෙයි. මාස 8ක් සඳහා මෙරට අපනයන ආදායම ප්‍රථම වතාවට ඇමෙරිකානු ඩොලර් බිලියන 12 සීමාව පසුකර තිබෙන බවද එම මණ්ඩලය ප්‍රකාශ කළේය.

## Method

For every distinct Sinhala word (punctuation stripped) the Singlish spelling was derived directly from `src/SinhalaInput.Core/Transliteration/RuleTable.cs`'s character mapping (independent vowels, dependent vowel signs, consonants, virama-cluster rule, ZWJ r/y-conjunct rule, anusvara) plus the U1 (word-final hal) and U2 (new letters: `zg`,`zd`,`zD`,`zb`,`zj`,`Th`,`Dh`,`R`/`RR`,`jny`) conventions described in the coordinator brief. All 133 distinct words (132 Sinhala words + 1 digit token) were piped through the built CLI in a single run (`dotnet run --project src/SinhalaInput.Cli/SinhalaInput.Cli.csproj --no-build`) and compared codepoint-for-codepoint against the source.

## Status summary

| Status | Count |
|---|---|
| PASS | 106 |
| NEEDS-U1 | 18 |
| NEEDS-U2 | 4 |
| NEEDS-U1+U2 | 0 |
| BUG | 4 |
| SKIP | 1 |
| **Total** | **133** |

## Checklist of BUG rows

- [ ] ඇඟලු‍ම් (aezgalum) — source HTML has a stray ZWJ (U+200D) between `ු` and `ම්` not adjacent to any r/y conjunct (likely a lankadeepa.lk CMS/copy-paste artifact); the engine cannot emit a bare ZWJ, so no spelling reproduces this exact sequence. The word also separately needs U1 (word-final `ම්`) and U2 (`ඟ` for `zg`).
- [ ] ඉල්ලු‍ම (illuma) — same stray-ZWJ source artifact as above (no U1/U2 needed otherwise; `ල්ල` and `ම` round-trip fine once the extra ZWJ is ignored).
- [ ] කාර්ය (kaarya) — source spells the rakaransaya+yansaya cluster as `ර්ය` (no ZWJ), but the engine's r/y-conjunct rule always inserts a ZWJ when `y` follows any consonant+virama (including `r`), producing `ර්‍ය`. Standard Sinhala orthography for this word usually does use the yansaya ligature, so the source is likely missing it — the engine has no way to produce the ZWJ-less form.
- [ ] ප්‍රථම (prathama) — contains ථ (U+0DAE, aspirated dental "th"), which is entirely absent from `RuleTable.cs` and not covered by either the U1 or U2 scope. No Singlish spelling can currently produce this letter; best effort ("th") renders as ත (dental t) instead.

## Notes

- `කින්`/`කට`/`ක්`/`ක` etc. are fragments split out of digit+suffix tokens such as `8.4කින්`, `506.8කට`, `464.27ක්` (the leading number is a pure digit and is not itself transliterable; only the trailing Sinhala case-suffix is tested). The pure-digit token `12` is recorded as SKIP.
- All NEEDS-U1 rows are words ending in a bare consonant + virama (e.g. `ඉකුත්`, `ගොස්`, `ක්`, `රටවල්`) — these will flip to PASS once Unit 1 lands word-final hal.
- All NEEDS-U2 rows need one of the new letters `ඳ` (zd, in `වෙළෙඳ...` words) or `ෘ` (R, in `ප්‍රවෘත්ති`) — these will flip to PASS once Unit 2 lands.
