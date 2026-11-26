# LicenseWorker - supporter keys of a licensed mod

A Cloudflare Worker (free plan) that signs the keys of a licensed mod (ModLib `LicenseHelper`,
[guide/07-Build-Deploy](../../docs/guide/07-Build-Deploy.md#licensed-mods-supporter-keys)) for its Ko-fi supporters.

```
Ko-fi payment ──webhook──► Worker /kofi ── signs a key (private key: secret) ──► KV (by SHA-256 of the e-mail)
                                                                              └─► Discord message to the author (optional)
Supporter ──► Worker /claim (e-mail of the Ko-fi payment) ──► key ──► game: title screen, "<mod>: enter key", Save, restart
Game: ModLib checks the signature with the public key of the mod (LICENSE_PUBLIC_KEY), offline
```

| File | |
|---|---|
| `src/license.mjs` | key format, signing (Web Crypto: Worker and Node) |
| `src/worker.js` | `POST /kofi` (webhook), `GET/POST /claim` (claim page), `POST /issue` (key by the author) |
| `scripts/generate-keys.mjs` | key pair of a mod (once) |
| `scripts/sign-key.mjs` | a key made on this machine (gift, test) |
| `wrangler.example.toml` | to copy into the mod repository as `wrangler.toml` |

Requirements: Node 18+ (`npx wrangler` downloads Wrangler), a Cloudflare account, a Ko-fi page.

## 1. Key pair (once per mod)

```
node GuiGuBaHuang-ModLib/tools/LicenseWorker/scripts/generate-keys.mjs <safe folder>/mymod-private.jwk.json
```

- the printed public key -> the mod: `public const string LICENSE_PUBLIC_KEY = "...";` (+ `LICENSE_NAME`, `LICENSE_URL`)
- the private JWK -> the Worker secret `LICENSE_PRIVATE_JWK`, and a backup outside git (lost = every key must be made
  again with a new pair and a new build of the mod; leaked = anyone can make keys: new pair, new build)

## 2. Worker

From the root of the mod repository, with `wrangler.toml` copied from `wrangler.example.toml` (e.g. `license-worker/`):

```
npx wrangler login
npx wrangler kv namespace create KEYS -c license-worker/wrangler.toml       # its id -> [[kv_namespaces]] id
npx wrangler secret put LICENSE_PRIVATE_JWK -c license-worker/wrangler.toml  # paste the content of the .jwk.json
npx wrangler secret put KOFI_VERIFICATION_TOKEN -c license-worker/wrangler.toml
npx wrangler secret put ADMIN_TOKEN -c license-worker/wrangler.toml          # a long random text, for /issue
npx wrangler secret put DISCORD_WEBHOOK_URL -c license-worker/wrangler.toml  # optional: a PRIVATE channel of the author
npx wrangler deploy -c license-worker/wrangler.toml
```

The address printed (`https://<name>.<account>.workers.dev`) serves `/claim`; set `LICENSE_URL` of the mod to it.

Vars of `wrangler.toml`: `MOD_ID`, `MOD_NAME`, `KOFI_URL`, `SUBSCRIPTION_DAYS` (key of each monthly membership payment,
35), `DONATION_DAYS` (one-off donation / shop order, 0 = never expires), `MIN_AMOUNT` (lowest one-off amount, 3),
`TIERS` (membership tiers that get a key, empty = all).

## 3. Ko-fi

1. Ko-fi webhooks page (`https://ko-fi.com/manage/webhooks`; menu **More** -> **API**): Webhook URL `https://<worker>/kofi`, Update.
2. On the same page, **Advanced** -> copy the **Verification Token** -> secret `KOFI_VERIFICATION_TOKEN` (deploy again
   is not needed: secrets apply at once).
3. **Send Test** on the same page: the Worker answers 200 (the test has the type "Donation" and a test e-mail; with a
   Discord webhook the author sees it).
4. Thank-you message (Ko-fi **Settings** -> Donation thank-you message, membership tier welcome message): "Get your key at
   https://<worker>/claim with the e-mail of your payment".

Each Ko-fi message is done once (KV `msg:<message_id>`: Ko-fi sends it again without a 200). Membership: a key of
`SUBSCRIPTION_DAYS` at each monthly payment; the supporter claims the new one when the game says the key expired.

## 4. Keys by hand

```
node scripts/sign-key.mjs --mod <modId> --name "Someone" --days 30 --jwk <file>     # on this machine
curl -X POST https://<worker>/issue -H "Authorization: Bearer <ADMIN_TOKEN>" -d '{"name":"Someone","email":"a@b.c","days":0}'
```

`/issue` with an `email` also keeps the key for `/claim`.

## Limits

- The check runs in the mod's DLL: anyone can remove it (decompiler). It keeps the honest players honest.
- `/claim` gives the key to whoever knows the e-mail of a payment.
- The game clock decides the expiry.
- Logs: `npx wrangler tail -c license-worker/wrangler.toml`.
