// Cloudflare Worker: keys of a licensed mod (ModLib LicenseHelper) for its Ko-fi supporters.
//   POST /kofi   Ko-fi webhook: checks the verification token, signs a key for an eligible payment, keeps it (KV) for /claim,
//                tells the author (Discord webhook, optional)
//   GET  /claim  page where a supporter gets their key with the e-mail of their Ko-fi payment
//   POST /issue  key made by the author (gift, test): Authorization: Bearer <ADMIN_TOKEN>, JSON {name, email?, days?, tier?}
// Secrets: LICENSE_PRIVATE_JWK, KOFI_VERIFICATION_TOKEN, ADMIN_TOKEN, DISCORD_WEBHOOK_URL (optional)
// Vars (wrangler.toml): MOD_ID, MOD_NAME, KOFI_URL, SUBSCRIPTION_DAYS, DONATION_DAYS, MIN_AMOUNT, TIERS
// KV binding: KEYS

import { importPrivateKey, makeKey } from "./license.mjs";

const KEEP = 5; // keys kept per e-mail (latest first)

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    try {
      if (url.pathname === "/kofi" && request.method === "POST") return await kofi(request, env);
      if (url.pathname === "/claim" && request.method === "GET") return html(claimPage(env));
      if (url.pathname === "/claim" && request.method === "POST") return await claim(request, env);
      if (url.pathname === "/issue" && request.method === "POST") return await issue(request, env);
      if (url.pathname === "/") return Response.redirect(new URL("/claim", url).toString(), 302);
      return new Response("Not found", { status: 404 });
    } catch (e) {
      console.error(e);
      return new Response("Error", { status: 500 });
    }
  },
};

// ---------- Ko-fi ----------

async function kofi(request, env) {
  const form = await request.formData();
  const raw = form.get("data");
  if (!raw) return new Response("no data", { status: 400 });
  const d = JSON.parse(raw);
  if (!env.KOFI_VERIFICATION_TOKEN || d.verification_token !== env.KOFI_VERIFICATION_TOKEN)
    return new Response("bad token", { status: 401 });

  // Ko-fi sends a webhook again when it got no 200: one key per message
  const seen = `msg:${d.message_id}`;
  if (await env.KEYS.get(seen)) return new Response("ok (already done)");

  const days = keyDays(d, env);
  if (days === null) {
    await env.KEYS.put(seen, "skipped", { expirationTtl: 60 * 60 * 24 * 30 });
    await notify(env, `Ko-fi ${d.type} of ${d.from_name} (${d.amount} ${d.currency}${d.tier_name ? ", " + d.tier_name : ""}): no key (rules)`);
    return new Response("ok (no key)");
  }

  const k = await makeKey(await privateKey(env), {
    mod: env.MOD_ID,
    name: d.from_name,
    id: d.kofi_transaction_id || d.message_id,
    tier: d.tier_name || d.type,
    days,
  });
  if (d.email) await keepKey(env, d.email, k);
  await env.KEYS.put(seen, "done", { expirationTtl: 60 * 60 * 24 * 30 });
  await notify(env, `Ko-fi ${d.type} of ${d.from_name} (${d.amount} ${d.currency}${d.tier_name ? ", " + d.tier_name : ""}): key until ${until(k.payload)}` +
    (d.email ? " - kept for /claim" : " - NO e-mail: send the key yourself") + `\n\`\`\`${k.key}\`\`\``);
  return new Response("ok");
}

/** Days of the key of a payment (0 = never expires); null: no key. */
function keyDays(d, env) {
  const tiers = (env.TIERS || "").split(",").map((x) => x.trim().toLowerCase()).filter(Boolean);
  if (d.type === "Subscription") {
    if (tiers.length && !tiers.includes((d.tier_name || "").toLowerCase())) return null;
    return num(env.SUBSCRIPTION_DAYS, 35);
  }
  if (d.type === "Donation" || d.type === "Shop Order") {
    if (parseFloat(d.amount) < num(env.MIN_AMOUNT, 3)) return null;
    return num(env.DONATION_DAYS, 0);
  }
  return null;
}

// ---------- claim ----------

async function claim(request, env) {
  const form = await request.formData();
  const email = String(form.get("email") || "").trim();
  if (!email) return html(claimPage(env, "Enter your e-mail."));
  const list = JSON.parse((await env.KEYS.get(await emailKey(email))) || "[]");
  const now = Math.floor(Date.now() / 1000);
  const valid = list.filter((x) => !x.exp || x.exp > now);
  if (!valid.length) return html(claimPage(env, list.length ? "Your key has expired: support again to get a new one." : "No key for this e-mail (use the e-mail of your Ko-fi payment; a payment takes a minute to arrive)."));
  return html(keyPage(env, valid[0]));
}

async function keepKey(env, email, k) {
  const id = await emailKey(email);
  const list = JSON.parse((await env.KEYS.get(id)) || "[]");
  list.unshift({ key: k.key, exp: k.payload.exp, iat: k.payload.iat, name: k.payload.n, tier: k.payload.t });
  await env.KEYS.put(id, JSON.stringify(list.slice(0, KEEP)));
}

// e-mails are not kept: their SHA-256 is the KV key
async function emailKey(email) {
  const h = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(email.trim().toLowerCase()));
  return "mail:" + [...new Uint8Array(h)].map((b) => b.toString(16).padStart(2, "0")).join("");
}

// ---------- issue (author) ----------

async function issue(request, env) {
  const auth = request.headers.get("Authorization") || "";
  if (!env.ADMIN_TOKEN || auth !== `Bearer ${env.ADMIN_TOKEN}`) return new Response("unauthorized", { status: 401 });
  const o = await request.json();
  const k = await makeKey(await privateKey(env), { mod: env.MOD_ID, name: o.name, tier: o.tier || "gift", days: num(o.days, 0) });
  if (o.email) await keepKey(env, o.email, k);
  return Response.json({ key: k.key, payload: k.payload });
}

// ---------- helpers ----------

let _key;
async function privateKey(env) {
  return (_key ??= await importPrivateKey(env.LICENSE_PRIVATE_JWK));
}

async function notify(env, content) {
  if (!env.DISCORD_WEBHOOK_URL) return;
  try {
    await fetch(env.DISCORD_WEBHOOK_URL, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ content: content.slice(0, 1900) }) });
  } catch (e) {
    console.error("discord", e);
  }
}

function num(v, def) {
  const n = parseFloat(v);
  return Number.isFinite(n) ? n : def;
}

const until = (p) => (p.exp ? new Date(p.exp * 1000).toISOString().slice(0, 10) : "never");

const esc = (s) => String(s ?? "").replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[c]);

function html(body) {
  return new Response(body, { headers: { "Content-Type": "text/html; charset=utf-8" } });
}

function layout(env, inner) {
  return `<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>${esc(env.MOD_NAME)} - Supporter key</title>
<style>body{font-family:system-ui,sans-serif;max-width:640px;margin:40px auto;padding:0 16px;background:#f6f1e7;color:#2b2118}
h1{font-size:1.4em}input,button,textarea{font:inherit;padding:8px;border-radius:6px;border:1px solid #b49a77}
input{width:100%;box-sizing:border-box}button{background:#7a3b1d;color:#fff;border:none;cursor:pointer;margin-top:8px}
textarea{width:100%;box-sizing:border-box;height:130px;word-break:break-all;font-family:monospace;font-size:12px}
.msg{color:#8a1c0c}.note{color:#6b5a45;font-size:.9em}</style></head><body>${inner}</body></html>`;
}

function claimPage(env, msg) {
  return layout(env, `<h1>${esc(env.MOD_NAME)} - Supporter key</h1>
<p>Enter the e-mail of your Ko-fi payment to get your key.</p>
<form method="post"><input type="email" name="email" required placeholder="you@example.com"><button>Get my key</button></form>
${msg ? `<p class="msg">${esc(msg)}</p>` : ""}
${env.KOFI_URL ? `<p class="note">Not a supporter yet? <a href="${esc(env.KOFI_URL)}">${esc(env.KOFI_URL)}</a></p>` : ""}`);
}

function keyPage(env, k) {
  return layout(env, `<h1>${esc(env.MOD_NAME)} - Supporter key</h1>
<p>Thank you, ${esc(k.name)}! Your key (until ${esc(until(k))}):</p>
<textarea id="k" readonly>${esc(k.key)}</textarea>
<button onclick="navigator.clipboard.writeText(document.getElementById('k').value);this.textContent='Copied'">Copy</button>
<p class="note">In the game: title screen, button "${esc(env.MOD_NAME)}: enter key", paste the key, Save, restart the game.</p>`);
}
