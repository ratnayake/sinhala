# EasyAkuru website

The marketing site, user guide, support page and privacy policy for EasyAkuru,
published at **https://ratnayake.info/easyakuru/**.

Plain static HTML and CSS: no build step, no JavaScript, no third-party requests
(fonts, analytics, CDNs). It is hosted on **Cloudflare Pages** and mounted on the
`/easyakuru` path of `ratnayake.info` by a small **Cloudflare Worker**.

## Layout

```
website/                     <- Pages build output directory
  _redirects                 Pages redirects: /  and /easyakuru  ->  /easyakuru/
  _headers                   Security headers (CSP, HSTS, ...) and asset caching
  404.html                   Minimal top-level 404; must exist, or Pages switches to
                             single-page-app mode and never returns 404s
  easyakuru/                 Every page lives under this folder
    index.html               Home (English)
    si/                      Home (Sinhala)
    guide/  support/         User guide, support / FAQ
    privacy/  si/privacy/    Privacy policy (English, Sinhala)
    assets/                  CSS, images, fonts
    404.html  robots.txt  sitemap.xml
  worker/                    Route Worker for ratnayake.info/easyakuru*
    src/index.js
    test/index.test.js
    wrangler.toml
  tools/serve.ps1            Local preview server
```

Because the whole site lives in `website/easyakuru/`, the Pages URL
`https://easyakuru.pages.dev/easyakuru/...` and the public URL
`https://ratnayake.info/easyakuru/...` map 1:1, so pages use root-relative links
(`/easyakuru/assets/site.css`) that work on both.

Page rules for contributors: no inline `style=""` attributes, `<style>` or `<script>`
blocks, and nothing loaded from another origin - the Content-Security-Policy in
`_headers` blocks them.

## Local preview

No Node.js needed. From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File website\tools\serve.ps1
# then open http://localhost:8788/easyakuru/
# other port: ... serve.ps1 -Port 8790
```

The server maps folders to `index.html`, sends the right MIME types, applies
`_redirects` and `_headers` (so the CSP is enforced locally as well), and serves the
nearest `404.html` for missing paths. Press Ctrl+C to stop.

## How requests flow

```
browser --> ratnayake.info/easyakuru/guide/?x=1
        --> Worker "easyakuru-route" (route ratnayake.info/easyakuru*)
              /easyakuru          -> 301 /easyakuru/ (query kept)
              /easyakuru/...      -> fetch https://easyakuru.pages.dev/easyakuru/guide/?x=1
        <-- Pages response as-is (status, headers from _headers, redirects;
            absolute Location headers on easyakuru.pages.dev rewritten to ratnayake.info)
```

The Pages origin is the `PAGES_ORIGIN` variable in `worker/wrangler.toml`.

## Deployment (one-time setup)

### 1. Create the Pages project

Cloudflare dashboard -> **Workers & Pages** -> **Create** -> **Pages** ->
**Connect to Git**:

| Setting                | Value               |
| ---------------------- | ------------------- |
| Repository             | `ratnayake/sinhala` |
| Project name           | `easyakuru` (gives `easyakuru.pages.dev`) |
| Production branch      | `main`              |
| Framework preset       | None                |
| Build command          | *(empty)*           |
| Build output directory | `website`           |

Every push to `main` then redeploys the site; other branches get preview URLs.
Check `https://easyakuru.pages.dev/easyakuru/` loads before continuing.
If the name `easyakuru` was taken and Pages gave a different `*.pages.dev`
subdomain, put that URL in `PAGES_ORIGIN` in `worker/wrangler.toml`.

### 2. Deploy the route Worker

Option A - Wrangler (needs Node.js; not installed on the dev machine yet:
`winget install OpenJS.NodeJS.LTS`, then open a new terminal):

```powershell
cd website/worker
npx wrangler login     # opens the browser to authorise the Cloudflare account
npx wrangler deploy    # creates "easyakuru-route" and the ratnayake.info/easyakuru* route
```

Option B - dashboard: **Workers & Pages** -> **Create** -> **Worker** ->
name it `easyakuru-route`, **Edit code**, paste `worker/src/index.js`, **Deploy**.
Then in the Worker's **Settings**: add the variable `PAGES_ORIGIN` =
`https://easyakuru.pages.dev`, and under **Domains & Routes** add the route
`ratnayake.info/easyakuru*` on zone `ratnayake.info`.

No account IDs or secrets are stored in this repository.

### 3. DNS

A Worker route only fires for hostnames whose DNS record is **proxied**
(orange cloud) through Cloudflare. In **ratnayake.info -> DNS -> Records**, make sure
the apex `ratnayake.info` has a proxied record. The apex currently answers with a
Cloudflare 522 (origin unreachable); that does not affect `/easyakuru` because the
Worker answers those requests before Cloudflare contacts the origin. If there is no
apex record at all, add a placeholder: type `AAAA`, name `@`, IPv6 `100::`,
proxy status **Proxied**.

### 4. Verify

```powershell
curl.exe -sI https://ratnayake.info/easyakuru/          # 200, text/html, CSP / HSTS / X-Frame-Options ...
curl.exe -sI https://ratnayake.info/easyakuru           # 301, Location: https://ratnayake.info/easyakuru/
curl.exe -sI https://ratnayake.info/easyakuru/privacy/  # 200
curl.exe -sI https://ratnayake.info/easyakuru/nope      # 404 (site 404 page)
curl.exe -sI https://easyakuru.pages.dev/               # 301, Location: /easyakuru/
```

- Privacy policy URL for the Microsoft Store listing:
  **https://ratnayake.info/easyakuru/privacy/**
- Open the home page in a browser with DevTools -> Console: there must be no
  Content-Security-Policy violations.

### 5. Analytics

Cloudflare Web Analytics (and any other analytics) is intentionally **not**
enabled for the Pages project or the zone, so the privacy policy's "no analytics,
no tracking" statement stays true. Leave it off in **Pages project -> Metrics**.

## Worker tests

The Worker tests use Node's built-in test runner with a mocked `fetch` and have no
dependencies (Node.js 18 or later):

```powershell
cd website/worker
node --test
```
