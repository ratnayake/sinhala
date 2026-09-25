# Lankadeepa round-trip test — News #1

- **Source URL:** https://www.lankadeepa.lk/news/22-පරධන-ජන-අභලෂයක-ඉට-කරමක/101-698312
- **Fetch date:** 2026-09-25 (via PowerShell `Invoke-WebRequest -UseBasicParsing`, UTF-8 decoded raw HTML, `article-body sinhala-body` div)

## Verbatim Sinhala text used

(අංජුල මහික වීරරත්න සහ සුජිත් හේවාජුලිගේ)

22 සංශෝධනයෙන් කෙරෙන්නේ ජනතාවගේ ප්‍රධාන අභිලාෂයක් ඉටු කිරීම බව කෘෂිකර්ම ඉඩම් සහ වාරිමාර්ග අමාත්‍ය කේ.ඩී ලාල් කාන්ත මහතා පාර්ලිමේන්තුවේදී පැවසීය.

"මේ සංශෝධනයේ අරමුණ ජනතාවගේ එක් ප්‍රධාන අභිලාෂයක් ඉටු කිරීමයි. ඒක බරපතල අභිලාෂයක්.අපේ අධිකරණ පද්ධතිය යාවත්කාලීන නොකිරීම නිසා පොදු මහජනතාව විශාල පීඩාවකට ලක්ව තිබෙනවා.අප රටේ බිඳ වැටුණු දේශපාලන සංස්කෘතියක් නිසා ජනතාව පීඩාවට පත්වුණා වගේම අධිකරණ පද්ධතියේ ස්වභාවය නිසා පොදු ජනතාව මහත් පීඩාවට පත්වෙනවා.අධිකරණ පද්ධතිය යාවත්කාලීන නොකිරීම විශාල වශයෙන් ජනතා විවේචනයට ලක්ව තිබෙනවා.

නඩු කල්යාම නිසා රජයට ඒ වෙනුවෙන් විශාල මුදලක් වැය කරන්නට සිදුවී තිබෙනවා.ඒ වගේම මහජනතාවටත් විශාල මුදලක් වියදම් කරන්නට සිදුවී තිබෙනවා.එහෙම නාස්ති වෙන්නේ  තමන්ගේ ජීවිතයේ වෙනත් ප්‍රතිඵලදායක වැඩකට යොදාගන්න තියෙන මුදල්.මේවා අපි බලන්න ඕන ජනතාවගේ පැත්තේ සිට.ජනතාවගේ ආණ්ඩුවක් ලෙස අප මේවාට උත්තර සෙවිය යුතු නැද්ද? යනුවෙන් ලාල් කාන්ත මහතා ප්‍රශ්න කළේය.

## Status summary (98 distinct words + 1 digit token)

| Status | Count |
|---|---|
| PASS | 77 |
| NEEDS-U1 | 16 |
| NEEDS-U2 | 2 |
| NEEDS-U1+U2 | 1 |
| BUG | 2 |
| SKIP | 1 |

## BUG checklist

- [ ] කල්යාම — engine's automatic consonant+y+vowel → ZWJ yansaya conjunct rule fires for l+ya, producing කල්‍යාම (with ZWJ) instead of plain hal+ya කල්යාම; no escape sequence exists in the scheme to suppress the automatic conjunct here.
- [ ] තමන්ගේ — engine greedily matches the "ng" digraph (→ ඞ) even though n and g belong to separate syllables (than+hal+ge); produced තමඞේ instead of තමන්ගේ; no escape exists to force a separate n+hal+g reading.
