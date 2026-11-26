// Keys of the licensed mods (ModLib LicenseHelper): "<payload>.<signature>", both base64url.
// payload = JSON {m: mod id, n: supporter name, i: payment id, t: tier, iat: issued (unix s), exp: expiry (unix s, 0 = never)}
// signature = RSASSA-PKCS1-v1_5 SHA-256 of the ASCII payload part. Web Crypto only: runs in a Cloudflare Worker and in Node 18+.

const ALG = { name: "RSASSA-PKCS1-v1_5", hash: "SHA-256" };

export function toBase64Url(bytes) {
  let s = "";
  for (const b of new Uint8Array(bytes)) s += String.fromCharCode(b);
  return btoa(s).replace(/=+$/, "").replace(/\+/g, "-").replace(/\//g, "_");
}

export function fromBase64Url(s) {
  s = s.replace(/-/g, "+").replace(/_/g, "/");
  while (s.length % 4) s += "=";
  return Uint8Array.from(atob(s), (c) => c.charCodeAt(0));
}

/** The private key (JWK of an RSA key, the secret LICENSE_PRIVATE_JWK) for signing. */
export async function importPrivateKey(jwk) {
  const k = typeof jwk === "string" ? JSON.parse(jwk) : { ...jwk };
  delete k.key_ops;
  delete k.ext;
  return crypto.subtle.importKey("jwk", k, ALG, false, ["sign"]);
}

/** The public key of a private JWK, as the mod writes it (LICENSE_PUBLIC_KEY): the base64url modulus (exponent 65537). */
export function publicKeyOf(jwk) {
  const k = typeof jwk === "string" ? JSON.parse(jwk) : jwk;
  if (k.e !== "AQAB") throw new Error("the exponent must be 65537 (AQAB)");
  return k.n;
}

/**
 * A signed key.
 * @param {CryptoKey} privateKey from importPrivateKey
 * @param {{mod: string, name?: string, id?: string, tier?: string, days?: number, now?: number}} o days 0 or missing = never expires
 */
export async function makeKey(privateKey, o) {
  const now = Math.floor((o.now ?? Date.now()) / 1000);
  const payload = {
    m: o.mod,
    n: (o.name || "Supporter").slice(0, 60),
    i: String(o.id || crypto.randomUUID()),
    t: o.tier || "",
    iat: now,
    exp: o.days > 0 ? now + Math.round(o.days * 86400) : 0,
  };
  const body = toBase64Url(new TextEncoder().encode(JSON.stringify(payload)));
  const sig = await crypto.subtle.sign(ALG, privateKey, new TextEncoder().encode(body));
  return { key: `${body}.${toBase64Url(sig)}`, payload };
}

/** The payload of a key (no check of its signature: the mod checks it). */
export function readKey(key) {
  return JSON.parse(new TextDecoder().decode(fromBase64Url(key.split(".")[0])));
}
