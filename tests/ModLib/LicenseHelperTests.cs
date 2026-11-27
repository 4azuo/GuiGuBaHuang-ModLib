using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using System;
using System.Security.Cryptography;
using System.Text;

namespace UnitTests.ModLib
{
    /// <summary>
    /// LicenseHelper: the keys of the licensed mods. The keys below were signed with Web Crypto (RSASSA-PKCS1-v1_5, SHA-256,
    /// as the Cloudflare Worker of Dao of Creation signs them) with a key pair made for these tests only: the C# check reads
    /// what such a server signs.
    /// </summary>
    [TestClass]
    public class LicenseHelperTests
    {
        private const string PUB = "s3NWT-JKZvkI_IBxrXG5HP4RiHLlUk4s7WTRItgAYO_9bgdt47_3KdqnFdO6l7ZwC40FrQ1iFMcvPht4fPulDlxcA3SxBiB2ii340UBvTQwfy8tbS7oZ8S0NMOA1uTowUKLx3u9odcsqAjivGkXe8zKK_I2cEoTtqolet0jtXBKAXVBS1zWpqI4O84hHaHRper4enEz1sdAZPF4DKLdQGx_1h_tSpzvQrUf6JXtWVzLcehSV227hI2uUuEGInJ1XtWVyzvYFhJKGRoZX3ZnsRMkKbcgazbQQY0xUoHnkk1mRqWLWCTd_gYPEgBNto_AYwqNE0haUXOviQXrVlzDbpQ";
        //mod testmd, "Tester", never expires
        private const string KEY_FOREVER = "eyJtIjoidGVzdG1kIiwibiI6IlRlc3RlciIsImkiOiI0YTkzOTc3Zi0xNTc3LTQxZWEtODlhYS1lNjk3YzYwNjc4YzUiLCJ0IjoiZ2lmdCIsImlhdCI6MTc5NTcwMzI4NiwiZXhwIjowfQ.AJG5uOPs5Udt2Eyfu443VK_5KeOYoUknadHgL7yfWeYCEtT-vQrCXMxmNAHT2bMw1GpDq26uBKxevxzfWsfASYGqnGcmKDPGNx03hhDVQP4vcZEZgl6D4FWvcMsl1OQphTa9tBW1LFg9wAAyiyYBIxVaRypfv9oufy-gjk7vq42BTRjUfzcoe720-kgKq8KM8g0tvRkBcqmdYG-bTU6WDyNJU3yrOBMZM7UWhaA9Ba6RZoroy8UoYa7lT6fFCcQkzmOYrrJLtON4yMc9bgbV1NScuHeDut1_DovDgF6C9Ve532YtID6fUeLAloHwj-f2SIuOXPBhf81eVm-owXJT5Q";
        //mod testmd, "Old", 30 days from 2020-01-01
        private const string KEY_EXPIRED = "eyJtIjoidGVzdG1kIiwibiI6Ik9sZCIsImkiOiIwZGQ4OTJkYi0xNTAwLTQ0MjctODdlYS1mNWY2NzA2N2ZlOTEiLCJ0IjoiIiwiaWF0IjoxNTc3ODM2ODAwLCJleHAiOjE1ODA0Mjg4MDB9.CT82z_sma_zWfDx6RWCWETg9a58wfxf-I4Rn769waC7V3S9VSWw0rmh-YbT9KYo3FhBLsAVjWwKQGM-UFi4EtxMi0Tg02-AjvD6qliY357pzqS9FrXWiUyfOxUXlYjuNpfZ4CtrNtOm_2OG-yLfZmzi4XAIcQrUCo3O9maFqTRIgEqQC0-KgFh8Ao-o06cthAGBq339YylQqTG8yD3Vo7o78yicY_cWjKBkboAM516s89jsnafNxBmUiKeEc5Bi23mabMVoUcyMyOJ55koTai-_1j4MBcqMsaEWBg7WydnsAsbFPqP2_JaDVBTfNQs3Aae9iypJKkrCFrEfiPOtvaw";
        //mod othermd
        private const string KEY_OTHER_MOD = "eyJtIjoib3RoZXJtZCIsIm4iOiJPdGhlciIsImkiOiI4N2JiNzU2Ni1iZDkyLTRmMDctYTViMS0yZDI5YzQ5N2RlYWIiLCJ0IjoiIiwiaWF0IjoxNzk1NzAzMjg2LCJleHAiOjB9.k2_pgxtaJhtitI-FEG4fLBR7rnHXlbMpO6YQiLS_ddTqNuV-S7_Cb4KSRTX_f3VomcrajVFDcbxiqOB7v4ETD8-4LN-LZYgODIRO1XpXX-vF6lDMlYtTl5Y-xWK96UoIXyaFTLr9N3_WmE2scCGYNOgeYgtUP9cNiSgWWx7ubLFjUHInT2LPphpx6a99JAXZP6FfUY8L5RC0pCAvHueGgzyUvmz8ePcUWr11tnjwNn0DwDB3viGGI5RrpuPaWA4rRCzSq4mieMeAtHgj6Zp0N2JjMxzfbrQIW8SOYmhw1j1Tvq9BO9jqOgBi_J426c21ndhC9WY14zFXHJ-PS8FfpA";
        //mod testmd, "Month", tier Gold, 30 days from 2030-01-01
        private const string KEY_2030 = "eyJtIjoidGVzdG1kIiwibiI6Ik1vbnRoIiwiaSI6ImMzZDY4MTExLTYzNGItNGVmOS05NDlmLTA1YjcyYWNkMWIzZiIsInQiOiJHb2xkIiwiaWF0IjoxODkzNDU2MDAwLCJleHAiOjE4OTYwNDgwMDB9.E7VxKbkW7yOZdfnb8SqD_PfgNW9Fe3vsFRmpFxED2LDHmhMi78lgbYIuDP7fVr2BWxV3bpW70-tkdY3G-nxXRyYpOEaFTTuHff59WKaznW0F9Ges6ic5u9nFJ8aXsBNNgOD_KS0r5LdjlK1u2LF-VwzMZ0t3RKYTgDdSkqJIu6IzZoN_m83ydHUUyBS-70qmA8O3anK0QOT1va7qIByjHZ8uUpKRFjBJhPHuRdeT-c0ZEJ7jKEDemGVyAwuMIyE03w80IDg_2ocjUZK7PPiM-S3Mq60K_h6gA1jBKBN0MnInZN23si1qSzP9sBybpBY5NHiIWKNfuyMEii4jZwhg5g";

        //mod testmd, "Ann", e-mail ann@example.com, tier "Tale of Immortal", 40 days from 2030-01-01
        private const string KEY_EMAIL = "eyJtIjoidGVzdG1kIiwibiI6IkFubiIsImUiOiJhbm5AZXhhbXBsZS5jb20iLCJpIjoiM2RlZTgxNmItZTJjYS00ZjdmLWIxY2EtNTQ1YmRkYjA0OWNhIiwidCI6IlRhbGUgb2YgSW1tb3J0YWwiLCJpYXQiOjE4OTM0NTYwMDAsImV4cCI6MTg5NjkxMjAwMH0.F30LjFO9BURX5-zCAqQEL-RFD3nxlEjeLK88H9kJ7XwiNZen8grgwJOJgVtoWq1ioob-S8s71TMQWMlZoco-TEjy-YasgFPXlIAFUOiDIlrG0UIwj5ZkpgNNwk62gsI2K8PY9CU4OUvQzAPDMDihOTPo77vcMMwdP_G_jz12HuELxG9QYoThIN9w3uiJU2AJVZJtMuY-Naz-Awkx81_NxCGULkM5xrtdhhH1eoMAs4qxYiiv-LlpF_2vGKKBQfrWxuGn3lP5gUIkjA1zmPr9y2osQneGN_Eg3MJPXyDM4EmwL6-w5-BYMIlSmKfAGeomha1qPzLPV4s68xGac87fYQ";

        private static readonly DateTime NOW = new DateTime(2030, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        [TestMethod, Description("Verify: the e-mail of a key (lower case), none in an older key")]
        public void Verify_Email()
        {
            var r = LicenseHelper.Verify(KEY_EMAIL, "testmd", PUB, NOW);
            Check.Eq(LicenseHelper.Status.Valid, r.Status, "status");
            Check.Eq("ann@example.com", r.Email, "e-mail");
            Check.Eq("Tale of Immortal", r.Tier, "tier");
            Check.Eq(new DateTime(2030, 2, 10, 0, 0, 0, DateTimeKind.Utc), r.Expires.Value, "40 days");
            Check.Eq(null, LicenseHelper.Verify(KEY_FOREVER, "testmd", PUB, NOW).Email, "no e-mail");
        }

        [TestMethod, Description("NeedsRenewal: an ended key at once, within 7 days in the background, never without e-mail or end")]
        public void NeedsRenewal_Days()
        {
            var r = LicenseHelper.Verify(KEY_EMAIL, "testmd", PUB, NOW); //ends 2030-02-10
            Check.Eq(LicenseHelper.Renewal.None, LicenseHelper.NeedsRenewal(r, new DateTime(2030, 2, 2, 23, 0, 0, DateTimeKind.Utc)), "8 days left");
            Check.Eq(LicenseHelper.Renewal.Background, LicenseHelper.NeedsRenewal(r, new DateTime(2030, 2, 3, 0, 0, 0, DateTimeKind.Utc)), "7 days left");
            Check.Eq(LicenseHelper.Renewal.Background, LicenseHelper.NeedsRenewal(r, new DateTime(2030, 2, 9, 0, 0, 0, DateTimeKind.Utc)), "1 day left");
            var ended = LicenseHelper.Verify(KEY_EMAIL, "testmd", PUB, new DateTime(2030, 3, 1, 0, 0, 0, DateTimeKind.Utc));
            Check.Eq(LicenseHelper.Renewal.Now, LicenseHelper.NeedsRenewal(ended, new DateTime(2030, 3, 1, 0, 0, 0, DateTimeKind.Utc)), "ended");
            Check.Eq(LicenseHelper.Renewal.None, LicenseHelper.NeedsRenewal(LicenseHelper.Verify(KEY_EXPIRED, "testmd", PUB, NOW), NOW), "ended, no e-mail");
            Check.Eq(LicenseHelper.Renewal.None, LicenseHelper.NeedsRenewal(LicenseHelper.Verify(KEY_FOREVER, "testmd", PUB, NOW), NOW), "never ends");
            Check.Eq(LicenseHelper.Renewal.None, LicenseHelper.NeedsRenewal(LicenseHelper.Verify(null, "testmd", PUB, NOW), NOW), "no key");
            Check.Eq(LicenseHelper.Renewal.None, LicenseHelper.NeedsRenewal(null, NOW), "null");
        }

        [TestMethod, Description("EffectiveNow: the latest of the clock, the latest time seen, the issue time: a clock set back changes nothing")]
        public void EffectiveNow_NeverGoesBack()
        {
            var clock = new DateTime(2030, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var seen = new DateTime(2030, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            Check.Eq(clock, LicenseHelper.EffectiveNow(clock, DateTime.MinValue, null), "nothing seen: the clock");
            Check.Eq(seen, LicenseHelper.EffectiveNow(clock, seen, null), "clock set back: the latest time seen");
            Check.Eq(clock, LicenseHelper.EffectiveNow(clock, seen.AddYears(-1), null), "clock ahead: the clock");
            Check.Eq(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), LicenseHelper.EffectiveNow(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTime.MinValue, new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)), "clock before the issue of the key: its issue time");
            // the 40-day key of 2030-01-01, the clock set back to 2030-01-10 after 2030-03-01 was seen: ended
            Check.Eq(LicenseHelper.Status.Expired, LicenseHelper.Verify(KEY_EMAIL, "testmd", PUB, LicenseHelper.EffectiveNow(clock, seen, null)).Status, "ended key stays ended");
        }

        [TestMethod, Description("ParseShare: the share detection of an answer of the renewal server")]
        public void ParseShare_Answer()
        {
            var s = LicenseHelper.ParseShare(Newtonsoft.Json.Linq.JObject.Parse("{\"key\":null,\"ips\":7,\"warn\":true,\"blocked\":false}"));
            Check.Eq(7, s.Ips, "networks");
            Check.Eq(true, s.Warn, "warning");
            Check.Eq(false, s.Blocked, "not blocked");
            Check.Eq(30, s.Days, "no window sent: 30 days");
            var w = LicenseHelper.ParseShare(Newtonsoft.Json.Linq.JObject.Parse("{\"ips\":3,\"warn\":false,\"blocked\":false,\"days\":14,\"blockAt\":8}"));
            Check.Eq(14, w.Days, "window of the server");
            Check.Eq(8, w.BlockAt, "threshold of the server");
            var b = LicenseHelper.ParseShare(Newtonsoft.Json.Linq.JObject.Parse("{\"key\":null,\"ips\":11,\"warn\":true,\"blocked\":true,\"reason\":\"x\"}"));
            Check.Eq(true, b.Blocked, "blocked");
            Check.Eq(null, LicenseHelper.ParseShare(Newtonsoft.Json.Linq.JObject.Parse("{\"key\":null}")), "an older Worker: none");
            Check.Eq(null, LicenseHelper.ParseShare(null), "no answer");
        }

        [TestMethod, Description("IsBetterKey: a renewed key is kept only when valid for the mod and ending later")]
        public void IsBetterKey_Rules()
        {
            var feb = LicenseHelper.Verify(KEY_EMAIL, "testmd", PUB, NOW);              //ends 2030-02-10
            var jan = LicenseHelper.Verify(KEY_2030, "testmd", PUB, NOW);               //ends 2030-01-31
            var forever = LicenseHelper.Verify(KEY_FOREVER, "testmd", PUB, NOW);
            Check.Eq(true, LicenseHelper.IsBetterKey(jan, feb), "later end");
            Check.Eq(false, LicenseHelper.IsBetterKey(feb, jan), "earlier end");
            Check.Eq(false, LicenseHelper.IsBetterKey(feb, feb), "same end");
            Check.Eq(true, LicenseHelper.IsBetterKey(feb, forever), "never ends");
            Check.Eq(false, LicenseHelper.IsBetterKey(forever, feb), "the current one never ends");
            Check.Eq(true, LicenseHelper.IsBetterKey(LicenseHelper.Verify(KEY_EXPIRED, "testmd", PUB, NOW), feb), "after an ended key");
            Check.Eq(true, LicenseHelper.IsBetterKey(LicenseHelper.Verify(null, "testmd", PUB, NOW), feb), "after no key");
            Check.Eq(false, LicenseHelper.IsBetterKey(jan, LicenseHelper.Verify(KEY_OTHER_MOD, "testmd", PUB, NOW)), "another mod");
            Check.Eq(false, LicenseHelper.IsBetterKey(jan, LicenseHelper.Verify(KEY_EMAIL, "testmd", PUB, new DateTime(2031, 1, 1, 0, 0, 0, DateTimeKind.Utc))), "ended answer");
            Check.Eq(false, LicenseHelper.IsBetterKey(jan, null), "no answer");
        }

        private class LicensedEntry { public const string LICENSE_PUBLIC_KEY = PUB; public const string LICENSE_NAME = "Test Mod"; }
        private class FreeEntry { }
        private class WrongTypeEntry { public const int LICENSE_PUBLIC_KEY = 1; }

        [TestMethod, Description("Verify: a key signed with Web Crypto that never expires is valid, with its supporter")]
        public void Verify_WorkerKey_Valid()
        {
            var r = LicenseHelper.Verify(KEY_FOREVER, "testmd", PUB, NOW);
            Check.Eq(LicenseHelper.Status.Valid, r.Status, "status");
            Check.Eq("Tester", r.Name, "name");
            Check.Eq("gift", r.Tier, "tier");
            Check.Eq(false, r.Expires.HasValue, "no expiry");
            Check.Eq(true, r.IsUnlocked, "unlocked");
        }

        [TestMethod, Description("Verify: expiry, another mod, mod id without case")]
        public void Verify_ExpiryAndMod()
        {
            Check.Eq(LicenseHelper.Status.Expired, LicenseHelper.Verify(KEY_EXPIRED, "testmd", PUB, NOW).Status, "expired");
            Check.Eq(LicenseHelper.Status.WrongMod, LicenseHelper.Verify(KEY_OTHER_MOD, "testmd", PUB, NOW).Status, "other mod");
            var r = LicenseHelper.Verify(KEY_2030, "TESTMD", PUB, NOW);
            Check.Eq(LicenseHelper.Status.Valid, r.Status, "30 days, day 9");
            Check.Eq("Gold", r.Tier, "tier");
            Check.Eq(new DateTime(2030, 1, 31, 0, 0, 0, DateTimeKind.Utc), r.Expires.Value, "expiry");
            Check.Eq(LicenseHelper.Status.Expired, LicenseHelper.Verify(KEY_2030, "testmd", PUB, NOW.AddDays(30)).Status, "day 39");
        }

        [TestMethod, Description("Verify: a pasted key with spaces, line breaks, quotes is read")]
        public void Verify_PastedKey()
        {
            var pasted = "  \"" + KEY_FOREVER.Substring(0, 100) + "\r\n" + KEY_FOREVER.Substring(100) + "\" \n";
            Check.Eq(LicenseHelper.Status.Valid, LicenseHelper.Verify(pasted, "testmd", PUB, NOW).Status, "pasted");
        }

        [TestMethod, Description("Verify: a changed payload or signature, a bad format, no key, another public key")]
        public void Verify_Tampered()
        {
            var parts = KEY_FOREVER.Split('.');
            var payload = Encoding.UTF8.GetString(LicenseHelper.FromBase64Url(parts[0])).Replace("Tester", "Hacker");
            var forged = LicenseHelper.ToBase64Url(Encoding.UTF8.GetBytes(payload)) + "." + parts[1];
            Check.Eq(LicenseHelper.Status.BadSignature, LicenseHelper.Verify(forged, "testmd", PUB, NOW).Status, "payload changed");
            var sig = LicenseHelper.FromBase64Url(parts[1]);
            sig[10] ^= 1;
            Check.Eq(LicenseHelper.Status.BadSignature, LicenseHelper.Verify(parts[0] + "." + LicenseHelper.ToBase64Url(sig), "testmd", PUB, NOW).Status, "signature changed");
            Check.Eq(LicenseHelper.Status.BadSignature, LicenseHelper.Verify(parts[0] + "." + parts[0], "testmd", PUB, NOW).Status, "payload as signature");
            Check.Eq(LicenseHelper.Status.Malformed, LicenseHelper.Verify("abc", "testmd", PUB, NOW).Status, "no dot");
            Check.Eq(LicenseHelper.Status.Malformed, LicenseHelper.Verify("a.b.c", "testmd", PUB, NOW).Status, "two dots");
            Check.Eq(LicenseHelper.Status.Malformed, LicenseHelper.Verify("!!!.???", "testmd", PUB, NOW).Status, "not base64");
            Check.Eq(LicenseHelper.Status.Missing, LicenseHelper.Verify(null, "testmd", PUB, NOW).Status, "null");
            Check.Eq(LicenseHelper.Status.Missing, LicenseHelper.Verify(" \n", "testmd", PUB, NOW).Status, "blank");
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                var otherPub = LicenseHelper.ToBase64Url(rsa.ExportParameters(false).Modulus);
                Check.Eq(LicenseHelper.Status.BadSignature, LicenseHelper.Verify(KEY_FOREVER, "testmd", otherPub, NOW).Status, "other public key");
            }
        }

        [TestMethod, Description("Verify: no public key = no key asked; a bad public key locks")]
        public void Verify_PublicKey()
        {
            Check.Eq(LicenseHelper.Status.NotRequired, LicenseHelper.Verify(null, "testmd", null, NOW).Status, "none");
            Check.Eq(true, LicenseHelper.Verify(null, "testmd", "", NOW).IsUnlocked, "empty");
            Check.Eq(LicenseHelper.Status.BadPublicKey, LicenseHelper.Verify(KEY_FOREVER, "testmd", "abc", NOW).Status, "too short");
            Check.Eq(LicenseHelper.Status.BadPublicKey, LicenseHelper.Verify(KEY_FOREVER, "testmd", "%%%%", NOW).Status, "not base64");
        }

        [TestMethod, Description("VerifyRsaSha256: agrees with the RSA of .NET (PKCS#1 v1.5, SHA-256) on other key pairs")]
        public void VerifyRsaSha256_SameAsDotNet()
        {
            for (var i = 0; i < 3; i++)
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    var data = Encoding.ASCII.GetBytes("payload-" + i);
                    var sig = rsa.SignData(data, CryptoConfig.MapNameToOID("SHA256"));
                    var p = rsa.ExportParameters(false);
                    Check.Eq(true, LicenseHelper.VerifyRsaSha256(data, sig, p.Modulus), $"valid {i}");
                    Check.Eq(false, LicenseHelper.VerifyRsaSha256(Encoding.ASCII.GetBytes("other"), sig, p.Modulus), $"other data {i}");
                    Check.Eq(false, LicenseHelper.VerifyRsaSha256(data, new byte[10], p.Modulus), $"short signature {i}");
                }
            }
        }

        [TestMethod, Description("ReadStringConst: the LICENSE_* constants of an entry class")]
        public void ReadStringConst_Constants()
        {
            Check.Eq(PUB, LicenseHelper.ReadStringConst(typeof(LicensedEntry), LicenseHelper.CONST_PUBLIC_KEY), "public key");
            Check.Eq("Test Mod", LicenseHelper.ReadStringConst(typeof(LicensedEntry), LicenseHelper.CONST_NAME), "name");
            Check.Eq(null, LicenseHelper.ReadStringConst(typeof(LicensedEntry), LicenseHelper.CONST_URL), "no url");
            Check.Eq(null, LicenseHelper.ReadStringConst(typeof(FreeEntry), LicenseHelper.CONST_PUBLIC_KEY), "free mod");
            Check.Eq(null, LicenseHelper.ReadStringConst(typeof(WrongTypeEntry), LicenseHelper.CONST_PUBLIC_KEY), "not a string");
            Check.Eq(null, LicenseHelper.ReadStringConst(null, LicenseHelper.CONST_PUBLIC_KEY), "no type");
        }

        [TestMethod, Description("IsUnlocked: a mod without LICENSE_PUBLIC_KEY always runs")]
        public void IsUnlocked_FreeMod()
        {
            Check.Eq(true, LicenseHelper.IsUnlocked("freemd", typeof(FreeEntry)), "free");
            Check.Eq(true, LicenseHelper.IsUnlocked(null, typeof(LicensedEntry)), "no mod id");
        }
    }
}
