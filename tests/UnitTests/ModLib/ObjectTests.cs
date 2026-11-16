using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Converter;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UnitTests.ModLib
{
    /// <summary>Test enum object (plain C#, no game types).</summary>
    public class TestColorEnum : EnumObject
    {
        public static TestColorEnum Red { get; } = new TestColorEnum("r", 1);
        public static TestColorEnum Green { get; } = new TestColorEnum("g", 2);
        public int Weight { get; private set; }
        private TestColorEnum(string v, int w) : base(v) { Weight = w; }
    }

    /// <summary>Holder serialized with the EnumObject converter.</summary>
    public class TestHolder
    {
        [JsonConverter(typeof(EnumObjectConverter))]
        public TestColorEnum Color { get; set; }
    }

    /// <summary>EnumObject, EnumObjectConverter, MultiValue, EncryptionHelper, PngEncodeHelper, ModBattleEvent keys, MathLogicOptions.</summary>
    [TestClass]
    public class ObjectTests
    {
        [ClassInitialize]
        public static void Init(TestContext _) => RuntimeHelpers.RunClassConstructor(typeof(TestColorEnum).TypeHandle);

        [TestMethod, Description("EnumObject: GetAllEnums/GetEnumByVal/GetEnumByName/GetEnum return the static singletons; Name and ToString")]
        public void EnumObject_Lookups()
        {
            Check.Info("TestColorEnum { Red = \"r\", Green = \"g\" }");
            Check.Seq(new[] { TestColorEnum.Red, TestColorEnum.Green }, EnumObject.GetAllEnums<TestColorEnum>().ToArray(), "GetAllEnums in declaration order");
            Check.Same(TestColorEnum.Green, EnumObject.GetEnumByVal<TestColorEnum>("g"), "GetEnumByVal(\"g\")");
            Check.Same(TestColorEnum.Red, EnumObject.GetEnumByName<TestColorEnum>("Red"), "GetEnumByName(\"Red\")");
            Check.Same(TestColorEnum.Red, EnumObject.GetEnum<TestColorEnum>("Red"), "GetEnum(\"Red\")");
            Check.Null(EnumObject.GetEnumByVal<TestColorEnum>("x"), "GetEnumByVal(\"x\") (unknown)");
            Check.Eq("Green", TestColorEnum.Green.Name, "Green.Name");
            Check.Eq($"{typeof(TestColorEnum).FullName}.Red", TestColorEnum.Red.ToString(), "Red.ToString()");
        }

        [TestMethod, Description("EnumObjectConverter: JSON round trip writes '<type>|<name>' and reads back the same singleton")]
        public void EnumObjectConverter_RoundTripKeepsSingleton()
        {
            var json = JsonConvert.SerializeObject(new TestHolder { Color = TestColorEnum.Green });
            Check.Contains(json, $"{typeof(TestColorEnum).FullName}|Green", "serialized JSON");
            var back = JsonConvert.DeserializeObject<TestHolder>(json);
            Check.Same(TestColorEnum.Green, back.Color, "deserialized Color");
        }

        [TestMethod, Description("MultiValue: Value0/Value1/Get(i) of MultiValue.Create(\"a\", 2, 3.5); out of range -> null")]
        public void MultiValue_Accessors()
        {
            var mv = MultiValue.Create("a", 2, 3.5);
            Check.Eq<object>("a", mv.Value0, "Value0");
            Check.Eq<object>(2, mv.Value1, "Value1");
            Check.Eq<object>(3.5, mv.Get(2), "Get(2)");
            Check.Null(mv.Get(3), "Get(3) (out of range)");
            Check.Null(new MultiValue().Value0, "empty MultiValue.Value0");
        }

        [TestMethod, Description("MathLogicOptions: Plus/Minus/Multi/Devide operators on (2, 3) and (6, 3)")]
        public void MathLogicOptions_Operators()
        {
            Check.Eq(5f, MathLogicOptions.Plus.Exe(2, 3), "Plus(2, 3)");
            Check.Eq(-1f, MathLogicOptions.Minus.Exe(2, 3), "Minus(2, 3)");
            Check.Eq(6f, MathLogicOptions.Multi.Exe(2, 3), "Multi(2, 3)");
            Check.Eq(2f, MathLogicOptions.Devide.Exe(6, 3), "Devide(6, 3)");
        }

        [TestMethod, Description("EncryptionHelper: Encrypt/Decrypt round trip (unicode), '+' restored from spaces; SHA256 hash is stable")]
        public void Encryption_RoundTrip()
        {
            var text = "Taoist save - 鬼谷八荒 123";
            var cipher = EncryptionHelper.Encrypt(text);
            Check.Info($"plain {Check.Fmt(text)} -> cipher {Check.Fmt(cipher)}");
            Check.NotEq(text, cipher, "cipher differs from the text");
            Check.Eq(text, EncryptionHelper.Decrypt(cipher), "Decrypt(cipher)");
            Check.Eq(text, EncryptionHelper.Decrypt(cipher.Replace("+", " ")), "Decrypt(cipher with '+' as spaces, from URLs)");
            Check.Eq("abc".GetSha256HashAsInt(), "abc".GetSha256HashAsInt(), "hash(\"abc\") stable");
            Check.NotEq("abc".GetSha256HashAsInt(), "abd".GetSha256HashAsInt(), "hash(\"abc\") != hash(\"abd\")");
        }

        [TestMethod, Description("PngEncodeHelper.EncodeRGBA: the 8-byte PNG signature (0x89 'PNG' CR LF 0x1A LF; BUG-07)")]
        public void PngEncode_Signature()
        {
            var png = PngEncodeHelper.EncodeRGBA(new byte[4], 1, 1);
            Check.Seq(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, png.Take(8).ToArray(), "PNG signature (first 8 bytes)");
        }

        [TestMethod, Description("PngEncodeHelper.EncodeRGBA(2x2 white): IHDR chunk with big-endian size, IEND last")]
        public void PngEncode_WritesChunks()
        {
            var rgba = Enumerable.Repeat((byte)255, 2 * 2 * 4).ToArray();
            var png = PngEncodeHelper.EncodeRGBA(rgba, 2, 2);
            Check.Info($"{png.Length} bytes");
            Check.Seq(new byte[] { (byte)'I', (byte)'H', (byte)'D', (byte)'R' }, png.Skip(12).Take(4).ToArray(), "bytes 12-15: first chunk is IHDR");
            Check.Seq(new byte[] { 0, 0, 0, 2, 0, 0, 0, 2 }, png.Skip(16).Take(8).ToArray(), "bytes 16-23: width/height 2x2 big-endian");
            Check.Seq(new byte[] { (byte)'I', (byte)'E', (byte)'N', (byte)'D' }, png.Skip(png.Length - 8).Take(4).ToArray(), "last chunk is IEND");
        }

        [TestMethod, Description("ModBattleEvent: damage statistic keys and the physic/magic/basis classification of damage types")]
        public void ModBattleEvent_DamageKeysAndTypes()
        {
            Check.Eq("DmgDealt_Fire", ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Fire), "GetDmgKey(DmgDealt, Fire)");
            Check.True(ModBattleEvent.IsBasisPhysic(ModBattleEvent.DmgTypeEnum.Sword), "Sword is basis physic");
            Check.False(ModBattleEvent.IsBasisPhysic(ModBattleEvent.DmgTypeEnum.Fire), "Fire is basis physic");
            Check.True(ModBattleEvent.IsBasisMagic(ModBattleEvent.DmgTypeEnum.Wood), "Wood is basis magic");
            Check.False(ModBattleEvent.IsBasisDmg(ModBattleEvent.DmgTypeEnum.Physic), "Physic is a basis damage");
            Check.False(ModBattleEvent.IsBasisDmg(ModBattleEvent.DmgTypeEnum.Damage), "Damage is a basis damage");
            Check.Eq(ModBattleEvent.DmgTypeEnum.Damage, ModBattleEvent.GetDmgBasisType(null), "GetDmgBasisType(null)");
            Check.Eq(ModBattleEvent.DmgTypeEnum.Damage, ModBattleEvent.GetDmgType(null), "GetDmgType(null)");
        }
    }
}
