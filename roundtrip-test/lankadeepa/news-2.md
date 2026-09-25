# Lankadeepa round-trip test — News #2

- **Source URL:** https://www.lankadeepa.lk/news/ආබධතයගන-අලලස-ගත-ගරම-නලධරයග-දඬවම-ඉහළ-උසවයනත-තහවරය/101-698315
- **Fetch date:** 2026-09-25 (via PowerShell `Invoke-WebRequest -UseBasicParsing`, UTF-8 decoded raw HTML, `article-body sinhala-body` div, first 6 paragraphs / 152 words)

## Verbatim Sinhala text used

(මනෝප්‍රිය ගුණසේකර සහ රංජන් කස්තුරි)

ආබාධිත පුද්ගලයකුට නිවාස සෑදීම සදහා රජයේ මුදල් ලබාදීමට රුපියල් ලක්ෂයක අල්ලසක් ඉල්ලා එයින් රුපියල් 25000ක් ලබා ගැනීමේ චෝදනාවට කොළඹ මහාධිකරණය මගින් වරදකරු කොට බරපතළ වැඩ ඇතිව වසර පහක සිර දඬුවම් නියමව සිටි රත්නපුර කුඩාව ප්‍රදේශයේ හිටපු ග්‍රාම නිලධාරියකුට ලබා දුන් අදාළ සිර දඬුවම ශ්‍රේෂ්ඨාධිකරණය තහවුරු කළේය.

ශ්‍රේෂ්ඨාධිකරණය මෙසේ සිරදඬුවම තහවුරු කරනු ලැබුවේ එම හිටපු ග්‍රාම නිලධාරියා විසින් ගොනුකොට තිබූ විශේෂ අභියාචන පෙතසමක් විභාගයට ගැනීම ප්‍රතික්ෂේප කරමිනි.

මෙම විශේෂ අභියාචන පෙත්සම අගවිනිසුරු ප්‍රීතී පද්මන් සූරසේන සහ ශ්‍රේෂ්ඨාධිකරණ විනිසුරු අචල වෙන්ගප්පුලි යන මහත්වරුන්ගේ සමන්විත ශ්‍රේෂ්ඨාධිකරණ විනිසුරු මඩුල්ලක් හමුවේ විභාගයට ගැනීණි.

රත්නපුර කුඩව ප්‍රදේශයේ හිටපු ග්‍රාම නිලධාරී එම්" රණසිංහ නමැත්තා විසින් ගොනුකොට තිබූ පෙතසමේ වගඋත්තරකරු වශයෙන් අල්ලස් හෝ දූෂණ කොමිසමේ අධ්‍යක්ෂ ජනරාල්වරයා නම් කර තිබිණී.

පෙත්සම අධිකරණයේ සලකා බැලූ අවස්ථාවේදී විත්තිකාර අභියාචක වෙනුවෙන් පෙනී සිටි ජනාධිපති නීතිඥ කාලිංග ඉන්ද්‍රතිස්ස මහතා කරුණු දක්වමින් කියා සිටියේ තම සේවාදායකයා අදාළ මුදල කිසිවිටෙකත් ලබා නොගත් බවත් උපාය දූතයාගේ සාක්ෂි සහ පැමිණිල්ලේ සාක්ෂි පරස්පර බවත්ය.

## Status summary (124 distinct words + 1 digit token)

| Status | Count |
|---|---|
| PASS | 93 |
| NEEDS-U1 | 19 |
| NEEDS-U2 | 6 |
| NEEDS-U1+U2 | 1 |
| BUG | 5 |
| SKIP | 1 |

## BUG checklist

- [ ] වෙන්ගප්පුලි — engine greedily matches the "ng" digraph (→ ඞ) even though n and g belong to separate syllables (ven+hal+ga); produced වෙඞප්පුලි instead of වෙන්ගප්පුලි.
- [ ] මහත්වරුන්ගේ — same "ng" digraph collision; produced මහත්වරුඞේ instead of මහත්වරුන්ගේ.
- [ ] වගඋත්තරකරු — compound-word hiatus: the independent vowel උ follows a bare consonant ග (vaga+uttharakaru); typed "gau" is read as the "au" dependent vowel sign attaching to ග, producing වගෞත්තරකරු; the scheme has no way to force an independent-vowel break right after a consonant carrying its inherent a.
- [ ] අවස්ථාවේදී — aspirated dental ථ has no mapping anywhere in the rule table (only plain ත "th" exists) and is not in the Unit 2 new-letter list either; produced අවස්තාවේදී instead of අවස්ථාවේදී.
- [ ] බවත්ය — engine's automatic consonant+y+vowel → ZWJ yansaya conjunct rule fires for th+ya, producing බවත්‍ය (with ZWJ) instead of plain hal+ya බවත්ය.
