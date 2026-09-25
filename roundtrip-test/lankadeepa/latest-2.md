# Lankadeepa "Latest News" round-trip test — article 2

- **Source URL:** https://www.lankadeepa.lk/latest_news/22-%E0%B7%80%E0%B6%B1-%E0%B6%B1%E0%B7%92%E0%B6%BB%E0%B7%94%E0%B7%80%E0%B7%80%E0%B7%92%E0%B6%9A-%E0%B7%83%E0%B6%82%E0%B7%81%E0%B7%9D%E0%B6%B0%E0%B6%B1%E0%B6%BA%E0%B6%9A/1-698286
  (displayed title: "22 වන නිරුවත් සංශෝධනයක්" — opposition leader on the 22nd Amendment)
- **Fetch date:** 2026-09-25 (via PowerShell `Invoke-WebRequest -UseBasicParsing`, UTF-8 decoded raw HTML; body extracted from the `<div class="article-body sinhala-body">` block)

## Verbatim Sinhala text used

> (අංජුල මහික වීරරත්න සහ සුජිත් හේවාජුලිගේ)
>
> 22 වැනි ආණ්ඩුක්‍රම ව්‍යවස්ථා සංශෝධනය නිරුවත් සංශෝධනයක් බව විපක්ෂ නායක සජිත් ප්‍රේමදාස මහතා පාර්ලිමේන්තුවේදී අද පැවසීය.
>
> "22 වැනි ආණ්ඩුක්‍රම ව්‍යවස්ථා සංශෝධනය රටට හොඳ යැයි කොච්චර කිව්වත් ඒ කතාව ඇත්ත නොවෙයි.ඇත්තටම මේක නිරුවත් සංශෝධනයක්.එය ප්‍රජාතන්ත්‍ර විරෝධී රටේ ජනතාවගේ ආත්ම ගරුත්වය කෙලෙසන මානව අයිතීන් උල්ලංඝනය කරන සංශෝධනයක් බව කිව යුතුයි.
>
> මෑත කාලයේ ආණ්ඩුක්‍රම ව්‍යවස්ථා සංශෝධන ගණනාවක් ආවා.17 සංශෝධනය ආවේ විධායක බලතල අඩු කරන්න.18 සංශෝධනයෙන් කළේ අඩු කරපු විධායක බලතල නැවත විධායකය විසින් ලබා ගත්තා. 19 සංශෝධනයෙන් නැවත විධායක බලතල අඩු කළා.20 සංශෝධනයෙන් කළේ මොකක්ද? 19 සංශෝධනයෙන් අඩු කරපු බලතල 69 ලක්ෂයෙන් පත්වුණු ජනාධිපතිවරයා නැවත ලබා ගත්තා.ඊට පස්සේ රට බංකොලොත් වුණා.රට විනාශ මුඛය කරා ගියා. ඊට පස්සේ 21 වෙනි සංශෝධනය පැමිණියා.එයින් කළේ 20 සංශෝධනයෙන් විධායකට ගත් බලතල නැවත අඩු කිරීමයි. උපරිමාධිකරණ විනිසුරුවන් හිතුමතේ පත් කිරීමට නොහැකි වගන්ති එහි ඇතුළත් වුණා.මේ රටේ ප්‍රජාතන්ත්‍රවාදයේ ඛේදවාචකය කුමක්ද?17,18,19,20,21 ඔය ඔක්කොටම අත උස්සපු කට්ටිය මේ රටේ දේශපාලන ක්ෂේත්‍රයේ ඉන්නවා.මේ රටේ ජනතාව ඒ අයට ඡන්දත් දීලා තියෙනවා. ඔන්න ඕක තමයි ඛේදවාචකය යනුවෙන්ද විපක්ෂ නායකවරයා කීය.

## Method

124 distinct tokens extracted (punctuation stripped). For every distinct Sinhala word, a Singlish spelling was written character-by-character against the target scheme in `src/SinhalaInput.Core/Transliteration/RuleTable.cs` plus the Unit 1 (word-final hal) and Unit 2 (new letters) conventions described by the coordinator. All 116 Sinhala-word spellings were piped through the built CLI in one run (`dotnet build` once, then a single piped `dotnet run --no-build`), and each output was compared codepoint-for-codepoint against the original word (BOM on the first line stripped).

## Status summary

| Status | Count |
|---|---|
| PASS | 97 |
| NEEDS-U1 | 17 |
| NEEDS-U2 | 1 |
| NEEDS-U1+U2 | 0 |
| BUG | 1 |
| SKIP (Latin/digits) | 8 |
| **Total tokens** | **124** |

## BUG checklist

- [ ] ව්‍යවස්ථා (`vyavasthaa`) — contains U+0DAE (dental aspirated "tha", ථ), which has **no Singlish mapping at all** in the target scheme (only the retroflex aspirates `Th`/`Dh` were added by Unit 2; the dental aspirate ථ is missing from both the base table and the Unit 2 new-letters list). Typing "th" produces U+0DAD (ත) instead. Same root cause as ස්ථාපිත / ආර්ථික in latest-1.md — the scheme itself needs a rule for ථ.
