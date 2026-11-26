// Makes a key on this machine (gift, test, a supporter without e-mail), with the same code as the Worker:
//   node scripts/sign-key.mjs --mod rLvJjS --name "Someone" [--days 30] [--tier gift] [--jwk license-private.jwk.json]
import { readFileSync } from "node:fs";
import { importPrivateKey, makeKey } from "../src/license.mjs";

const args = Object.fromEntries(
  process.argv.slice(2).reduce((a, x, i, all) => (x.startsWith("--") ? [...a, [x.slice(2), all[i + 1]]] : a), [])
);
if (!args.mod) {
  console.error('usage: node scripts/sign-key.mjs --mod <modId> --name "<name>" [--days N] [--tier T] [--jwk file]');
  process.exit(1);
}
const jwk = readFileSync(args.jwk || "license-private.jwk.json", "utf8");
const k = await makeKey(await importPrivateKey(jwk), { mod: args.mod, name: args.name, tier: args.tier || "gift", days: Number(args.days || 0) });
console.log(JSON.stringify(k.payload));
console.log(k.key);
