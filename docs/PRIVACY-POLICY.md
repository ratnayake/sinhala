# Privacy Policy — EasyAkuru

**Effective date:** 2026-09-26
**Publisher:** Ratcon
**Contact:** isuru.sampath@ratnayake.info

This policy describes how EasyAkuru ("the app") handles information on your device. It is
written in plain language rather than legal boilerplate, because the answer is simple: the app
processes what you type only to convert it to Sinhala, and nothing you type ever leaves your
device.

## What EasyAkuru does

EasyAkuru is a Windows tray application that watches your keystrokes system-wide so it can
transliterate Latin ("Singlish") text into Sinhala Unicode as you type, in whatever application
you're currently using. To do this, it must observe your keystrokes — that is how a system-wide
phonetic keyboard tool works. This policy explains exactly what happens to that data, and what
does not happen to it.

## Keystrokes are processed locally only

- EasyAkuru reads keystrokes through a Windows keyboard hook so it can recognize and
  transliterate the word you are currently typing.
- This processing happens entirely on your own computer, in memory, in real time. The app has
  no network access and makes no outbound connections of any kind — there is no server for your
  keystrokes to be sent to, and the app contains no code that could send them anywhere.
- The Latin text of the word you are currently typing is held only in memory for as long as it
  takes to transliterate it. It is discarded the moment the word is committed (replaced with
  its Sinhala rendering) or the app is closed. It is never written to disk, logged, or
  transmitted.

## Password fields are excluded on a best-effort basis

EasyAkuru tries to detect when you are typing into a password field (by checking whether the
focused control is a standard Windows password-style input) and, when it recognizes one,
disengages entirely so your keystrokes pass through untouched and are never processed by the
app's transliteration engine. This detection covers standard Windows password controls but is
**best-effort**: it may not recognize password fields built with fully custom, non-standard
controls. EasyAkuru still never stores or transmits any keystroke regardless of whether a field
is recognized as a password field — the concern this section addresses is only whether the
tool's candidate popup and transliteration might visibly engage in a field where you'd rather
it didn't, not whether your data is safe, which it always is.

## What is actually stored, and where

The only information EasyAkuru saves to disk is stored locally, under your own Windows user
profile, at `%AppData%\EasyAkuru\`, and consists of:

- **Your app preferences** — for example, whether the tool is enabled and whether it starts
  automatically at sign-in.
- **Your chosen word preferences** — if you pick a non-default Sinhala spelling for a word from
  the candidate popup, the app remembers that choice locally so it can offer it first next
  time you type that word. This dictionary contains only Sinhala words and spellings you have
  already committed and chosen — never raw, uncommitted keystrokes, and never anything typed
  into a recognized password field.

Nothing else is written to disk. These files are never uploaded, synced, or shared — they stay
on your computer and are readable only by your own Windows user account.

## No telemetry, no analytics, no network access

EasyAkuru does not collect usage statistics, does not phone home, does not check for updates
over the network, and does not include any third-party analytics, advertising, or tracking
libraries. The app has no network-facing code at all. If you inspect your firewall or network
monitor while using it, you will see no outbound connections from EasyAkuru, because it makes
none.

## Data sharing

EasyAkuru does not share any data with anyone, because it does not collect or transmit any data
in the first place. There are no third parties involved in the app's operation.

## Auto-start

EasyAkuru starts automatically when you sign in to Windows, using the standard Windows startup
mechanism for installed apps. This is a convenience feature, not a data-collection feature — it
does not involve sending any information anywhere. You can turn it off at any time in
**Settings › Apps › Startup**.

## Changes to this policy

If this policy changes — for example, if a future version of EasyAkuru adds an optional feature
that involves the network — this document will be updated and the effective date above will
change accordingly. As of the effective date above, the app has no network access of any kind.

## Contact

Questions about this policy or the app's data handling can be sent to
**isuru.sampath@ratnayake.info**.
