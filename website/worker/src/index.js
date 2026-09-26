// EasyAkuru route Worker.
//
// Serves https://ratnayake.info/easyakuru/... from the Cloudflare Pages project
// (https://easyakuru.pages.dev/easyakuru/...). Pages cannot be mounted on a sub-path
// of another domain by itself, so this Worker is bound to the route
// "ratnayake.info/easyakuru*" and proxies those requests 1:1 (same path and query).
//
// The Pages response (status, body and headers, including the security headers from
// website/_headers and any redirects) is returned unchanged, except that an absolute
// Location header pointing at the Pages origin is rewritten to the public origin.

const DEFAULT_PAGES_ORIGIN = "https://easyakuru.pages.dev";
const PREFIX = "/easyakuru";

function pagesOrigin(env) {
  const configured = env && typeof env.PAGES_ORIGIN === "string" ? env.PAGES_ORIGIN.trim() : "";
  return new URL(configured || DEFAULT_PAGES_ORIGIN).origin;
}

/**
 * Rewrites an absolute Location on the Pages origin to the public origin.
 * Relative locations (e.g. "/easyakuru/") and other hosts are returned unchanged.
 */
export function rewriteLocation(location, upstreamOrigin, publicOrigin) {
  let target;
  try {
    target = new URL(location);
  } catch {
    return location; // relative URL - already correct for the public host
  }
  if (target.origin !== upstreamOrigin) {
    return location;
  }
  return publicOrigin + target.pathname + target.search + target.hash;
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);

    // The route only sends /easyakuru* here, but "/easyakurufoo" also matches the
    // route pattern; anything that is not /easyakuru or /easyakuru/... goes to the
    // zone's normal origin untouched.
    if (url.pathname !== PREFIX && !url.pathname.startsWith(PREFIX + "/")) {
      return fetch(request);
    }

    if (url.pathname === PREFIX) {
      url.pathname = PREFIX + "/";
      return Response.redirect(url.toString(), 301);
    }

    const upstreamOrigin = pagesOrigin(env);
    const upstreamUrl = upstreamOrigin + url.pathname + url.search;

    const headers = new Headers(request.headers);
    headers.delete("host");

    const init = {
      method: request.method,
      headers,
      // Hand Pages redirects (e.g. /easyakuru/guide -> /easyakuru/guide/) back to
      // the browser instead of following them inside the Worker.
      redirect: "manual",
    };
    if (request.method !== "GET" && request.method !== "HEAD" && request.body !== null) {
      init.body = request.body;
      init.duplex = "half"; // required by fetch implementations for streamed bodies
    }

    const upstream = await fetch(upstreamUrl, init);

    const location = upstream.headers.get("location");
    if (location === null) {
      return upstream;
    }
    const rewritten = rewriteLocation(location, upstreamOrigin, url.origin);
    if (rewritten === location) {
      return upstream;
    }
    const response = new Response(upstream.body, upstream);
    response.headers.set("location", rewritten);
    return response;
  },
};
