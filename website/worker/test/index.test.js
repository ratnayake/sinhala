// Run with: cd website/worker; node --test
// Uses only Node's built-in test runner and a mocked global fetch (Node 18+).
import { test, beforeEach, afterEach } from "node:test";
import assert from "node:assert/strict";

import worker, { rewriteLocation } from "../src/index.js";

const ENV = { PAGES_ORIGIN: "https://easyakuru.pages.dev" };

let calls;
let nextResponse;
const realFetch = globalThis.fetch;

beforeEach(() => {
  calls = [];
  nextResponse = () => new Response("ok", { status: 200, headers: { "content-type": "text/html" } });
  globalThis.fetch = async (input, init) => {
    calls.push({ input, init });
    return nextResponse();
  };
});

afterEach(() => {
  globalThis.fetch = realFetch;
});

test("/easyakuru redirects to /easyakuru/ with 301 and keeps the query", async () => {
  const res = await worker.fetch(new Request("https://ratnayake.info/easyakuru?lang=si&x=1"), ENV);
  assert.equal(res.status, 301);
  assert.equal(res.headers.get("location"), "https://ratnayake.info/easyakuru/?lang=si&x=1");
  assert.equal(calls.length, 0, "must not call upstream for the slash redirect");
});

test("proxies to the Pages origin with the same path and query", async () => {
  const res = await worker.fetch(
    new Request("https://ratnayake.info/easyakuru/guide/?a=1&b=%E0%B6%85#frag"),
    ENV,
  );
  assert.equal(res.status, 200);
  assert.equal(await res.text(), "ok");
  assert.equal(calls.length, 1);
  assert.equal(calls[0].input, "https://easyakuru.pages.dev/easyakuru/guide/?a=1&b=%E0%B6%85");
  assert.equal(calls[0].init.method, "GET");
  assert.equal(calls[0].init.redirect, "manual");
  assert.equal(calls[0].init.body, undefined);
});

test("proxies the root of the site", async () => {
  await worker.fetch(new Request("https://ratnayake.info/easyakuru/"), ENV);
  assert.equal(calls[0].input, "https://easyakuru.pages.dev/easyakuru/");
});

test("uses PAGES_ORIGIN from env and falls back to the default", async () => {
  await worker.fetch(new Request("https://ratnayake.info/easyakuru/x"), {
    PAGES_ORIGIN: "https://preview.easyakuru.pages.dev/",
  });
  await worker.fetch(new Request("https://ratnayake.info/easyakuru/x"), {});
  assert.equal(calls[0].input, "https://preview.easyakuru.pages.dev/easyakuru/x");
  assert.equal(calls[1].input, "https://easyakuru.pages.dev/easyakuru/x");
});

test("forwards method, headers and body", async () => {
  const req = new Request("https://ratnayake.info/easyakuru/support/?q=1", {
    method: "POST",
    headers: { "content-type": "text/plain", "x-test": "yes", "accept-language": "si" },
    body: "hello body",
  });
  await worker.fetch(req, ENV);
  assert.equal(calls.length, 1);
  const { input, init } = calls[0];
  assert.equal(input, "https://easyakuru.pages.dev/easyakuru/support/?q=1");
  assert.equal(init.method, "POST");
  assert.equal(init.headers.get("x-test"), "yes");
  assert.equal(init.headers.get("accept-language"), "si");
  assert.equal(init.headers.get("content-type"), "text/plain");
  assert.equal(init.headers.get("host"), null);
  assert.equal(await new Response(init.body).text(), "hello body");
});

test("HEAD requests are forwarded without a body", async () => {
  await worker.fetch(new Request("https://ratnayake.info/easyakuru/", { method: "HEAD" }), ENV);
  assert.equal(calls[0].init.method, "HEAD");
  assert.equal(calls[0].init.body, undefined);
});

test("returns upstream status and headers unchanged", async () => {
  nextResponse = () =>
    new Response("missing", {
      status: 404,
      headers: {
        "content-type": "text/html; charset=utf-8",
        "content-security-policy": "default-src 'self'",
        "x-frame-options": "DENY",
      },
    });
  const res = await worker.fetch(new Request("https://ratnayake.info/easyakuru/nope"), ENV);
  assert.equal(res.status, 404);
  assert.equal(res.headers.get("content-security-policy"), "default-src 'self'");
  assert.equal(res.headers.get("x-frame-options"), "DENY");
  assert.equal(await res.text(), "missing");
});

test("rewrites an absolute Location on the Pages origin to the public origin", async () => {
  nextResponse = () =>
    new Response(null, {
      status: 308,
      headers: {
        location: "https://easyakuru.pages.dev/easyakuru/guide/?a=1",
        "x-content-type-options": "nosniff",
      },
    });
  const res = await worker.fetch(new Request("https://ratnayake.info/easyakuru/guide?a=1"), ENV);
  assert.equal(res.status, 308);
  assert.equal(res.headers.get("location"), "https://ratnayake.info/easyakuru/guide/?a=1");
  assert.equal(res.headers.get("x-content-type-options"), "nosniff");
});

test("leaves relative and foreign Location headers alone", async () => {
  nextResponse = () => new Response(null, { status: 301, headers: { location: "/easyakuru/si/" } });
  let res = await worker.fetch(new Request("https://ratnayake.info/easyakuru/si"), ENV);
  assert.equal(res.status, 301);
  assert.equal(res.headers.get("location"), "/easyakuru/si/");

  nextResponse = () =>
    new Response(null, { status: 302, headers: { location: "https://github.com/ratnayake/sinhala" } });
  res = await worker.fetch(new Request("https://ratnayake.info/easyakuru/src"), ENV);
  assert.equal(res.headers.get("location"), "https://github.com/ratnayake/sinhala");
});

test("passes requests outside /easyakuru straight through", async () => {
  for (const path of ["/", "/other", "/easyakurufoo", "/EasyAkuru/"]) {
    calls = [];
    const req = new Request("https://ratnayake.info" + path + "?k=v");
    await worker.fetch(req, ENV);
    assert.equal(calls.length, 1, path);
    assert.equal(calls[0].input, req, path);
    assert.equal(calls[0].init, undefined, path);
  }
});

test("rewriteLocation unit cases", () => {
  const up = "https://easyakuru.pages.dev";
  const pub = "https://ratnayake.info";
  assert.equal(rewriteLocation("https://easyakuru.pages.dev/easyakuru/", up, pub), "https://ratnayake.info/easyakuru/");
  assert.equal(rewriteLocation("https://easyakuru.pages.dev/a?b=c#d", up, pub), "https://ratnayake.info/a?b=c#d");
  assert.equal(rewriteLocation("/easyakuru/", up, pub), "/easyakuru/");
  assert.equal(rewriteLocation("https://abc.easyakuru.pages.dev/x", up, pub), "https://abc.easyakuru.pages.dev/x");
});
