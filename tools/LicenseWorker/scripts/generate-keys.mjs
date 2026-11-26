// Makes the key pair of a licensed mod (once per mod):
//   node scripts/generate-keys.mjs [out-file]   (default: license-private.jwk.json, ignored by git)
// The private JWK goes to the Worker secret LICENSE_PRIVATE_JWK (and a safe place: a lost key = every key to be made again);
// the public key printed goes to the mod: public const string LICENSE_PUBLIC_KEY = "...".
import { generateKeyPairSync } from "node:crypto";
import { existsSync, writeFileSync } from "node:fs";

const out = process.argv[2] || "license-private.jwk.json";
if (existsSync(out)) {
  console.error(`${out} exists: not overwritten (the keys made with it would stop working)`);
  process.exit(1);
}
const { privateKey } = generateKeyPairSync("rsa", { modulusLength: 2048, publicExponent: 0x10001 });
const jwk = privateKey.export({ format: "jwk" });
writeFileSync(out, JSON.stringify(jwk), { mode: 0o600 });
console.log(`Private key: ${out} (secret: never commit it)`);
console.log(`Public key (LICENSE_PUBLIC_KEY of the mod):\n${jwk.n}`);
