# Lankadeepa "Latest News" round-trip test — article 1

- **Source URL:** https://www.lankadeepa.lk/latest_news/%E0%B7%83%E0%B7%84%E0%B7%94%E0%B6%BB%E0%B7%94-%E0%B6%B4%E0%B7%80%E0%B6%BB%E0%B7%94-4570%E0%B6%9A%E0%B7%8A-%E0%B6%86%E0%B6%B1%E0%B6%BA%E0%B6%B1%E0%B6%BA-%E0%B6%9A%E0%B7%8F%E0%B6%BA/1-698149
  (displayed title: "සුහුරු පුවරු 4570ක් ආනයනය කරයි")
- **Fetch date:** 2026-09-25 (via PowerShell `Invoke-WebRequest -UseBasicParsing`, UTF-8 decoded raw HTML; body extracted from the `<div class="article-body sinhala-body">` block)

## Verbatim Sinhala text used

> (අංජුල මහික වීරරත්න සහ සුජිත් හේවාජුලිගේ)
>
> දිවයින පුරා පිහිටි පාසල් සඳහා අධ්‍යාපන නවීකරණ ව්‍යාපෘති කිහිපයක් යටතේ සුහුරු පුවරු (Smart boards) 4,570 ක් ආනයනය කර පාසල් වෙත බෙදා හැරීමට පියවර ගෙන ඇති බව අග්‍රාමාත්‍ය ආචාර්ය හරිනි අමරසූරිය පාර්ලිමේන්තුවේදී පැවසුවාය.
>
> පාර්ලිමේන්තු මන්ත්‍රීනී රෝහිණී කුමාරි විජේරත්න මහත්මිය අසන ලද ප්‍රශ්නයකට පිළිතුරු දෙමින් අග්‍රාමාත්‍යවරිය මේ බව කීවාය.
>
> මෙම වැඩසටහන යටතේ 2024 වසරේ විදුලි සංදේශ නියාමන කොමිෂන් සභා ප්‍රතිපාදන ව්‍යාපෘතිය මගින් ලබාගත් සුහුරු පුවරු 1,000 මේ වන විට පාසල් 1,000 ක් වෙත බෙදා දී අවසන් කර ඇති බව අග්‍රාමාත්‍යවරිය සඳහන් කළාය.
>
> 2025-2026 වසර සඳහා චීන ආධාර ව්‍යාපෘතිය යටතේ ආනයනය කරන ලද සුහුරු පුවරු 900 න් පුවරු 500 ක් පාසල් වෙත ලබා දී ඇති අතර, චීන රජයෙන් පත් කර ඇති ව්‍යාපෘති කළමනාකරණ කමිටුව මගින් එම සුහුරු පුවරු ස්ථාපිත කිරීමේ (Installation) කටයුතු මේ වන විට සිදු කරමින් පවතින්නේ යැයිද අග්‍රාමාත්‍යවරිය පැවසුවාය.
>
> මීට අමතරව, ඩිජිටල් කාර්යසාධක බලකායේ මඟපෙන්වීම සහ ඩිජිටල් ආර්ථික අමාත්‍යාංශ ප්‍රතිපාදන ව්‍යාපෘතිය යටතේ පළාත් මට්ටමින් ප්‍රසම්පාදනය කරන ලද සුහුරු පුවරු 2,670 ම සම්පූර්ණයෙන්ම බෙදා හැරීමට පියවර ගෙන ඇති අතර, ඒවා පාසල්වල සවිකිරීමේ කටයුතු ද මේ වන විට සක්‍රීයව සිදු කෙරෙමින් තිබෙන බවද අග්‍රාමාත්‍යවරිය සඳහන් කළාය.

## Method

116 distinct tokens extracted (punctuation stripped). For every distinct Sinhala word, a Singlish spelling was written character-by-character against the target scheme in `src/SinhalaInput.Core/Transliteration/RuleTable.cs` plus the Unit 1 (word-final hal) and Unit 2 (new letters) conventions described by the coordinator. All 106 Sinhala-word spellings were piped through the built CLI in one run (`dotnet build` once, then a single piped `dotnet run --no-build`), and each output was compared codepoint-for-codepoint against the original word (BOM on the first line stripped).

## Status summary

| Status | Count |
|---|---|
| PASS | 80 |
| NEEDS-U1 | 17 |
| NEEDS-U2 | 4 |
| NEEDS-U1+U2 | 1 |
| BUG | 4 |
| SKIP (Latin/digits) | 10 |
| **Total tokens** | **116** |

## BUG checklist

- [ ] ආචාර්ය (`aachaarya`) — engine inserts a spurious ZWJ conjunct for the bare "r"+"y" cluster (ර්ය): outputs ආචාර්‍ය instead of ආචාර්ය. The consonant+r/y+vowel ZWJ-conjunct rule appears to fire even when the preceding consonant is itself "r", which real orthography never ligates. Not part of Unit 1/2 scope — an existing engine issue.
- [ ] කාර්යසාධක (`kaaryasaadhaka`) — same spurious-ZWJ "r"+"y" issue as ආචාර්ය: outputs කාර්‍යසාධක instead of කාර්යසාධක.
- [ ] ස්ථාපිත (`sthaapitha`) — contains U+0DAE (dental aspirated "tha", ථ), which has **no Singlish mapping at all** in the target scheme (only the retroflex aspirates `Th`/`Dh` were added by Unit 2; the dental aspirate ථ is missing from both the base table and the Unit 2 new-letters list). Typing "th" produces U+0DAD (ත) instead. Neither unit's scope currently covers this letter — the scheme itself needs a rule for ථ.
- [ ] ආර්ථික (`aarthika`) — same missing-ථ-mapping issue as ස්ථාපිත.
