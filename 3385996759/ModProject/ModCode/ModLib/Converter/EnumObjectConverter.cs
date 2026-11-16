using ModLib.Helper;
using ModLib.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ModLib.Converter
{
    /// <summary>
    /// Json.NET converter that stores EnumObject instances as "TypeFullName|Name" and resolves them back to the singleton instance.
    /// </summary>
    public class EnumObjectConverter : JsonConverter
    {
        private const string TYPE = "$type";
        private const string NAME = "ObjName";

        /// <summary>
        /// True for every EnumObject type.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return typeof(EnumObject).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Reads the new string format or the old object format and returns the existing singleton.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                //new
                if (reader.TokenType == JsonToken.String)
                {
                    var jValue = JValue.Load(reader);
                    var s = jValue.ToString().Split('|');
                    return EnumObject.AllEnums.FirstOrDefault(x => x.GetType().FullName == s[0] && x.Name == s[1]) ?? throw new InvalidCastException();
                }

                //old
                var jObj = JObject.Load(reader);
                var objType = jObj[TYPE].ToString();
                var objNm = jObj[NAME].ToString();
                var obj = EnumObject.AllEnums.FirstOrDefault(x => x.GetType().FullName == objType && x.Name == objNm) ?? throw new InvalidCastException();
                serializer.Populate(jObj.CreateReader(), obj);
                return obj;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Failed to load enum: {objectType?.FullName ?? "null"}");
                DebugHelper.WriteLine(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Writes the enum object as "TypeFullName|Name".
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                //new
                writer.WriteValue($"{value.GetType().FullName}|{(value as EnumObject).Name}");

                //old
                //var t = JToken.FromObject(value);
                //var o = (JObject)t;
                //o.ReplaceAll(new JProperty(TYPE, new JValue(value.GetType().FullName)));
                //o.Add(new JProperty(NAME, new JValue((value as EnumObject).Name)));
                //o.WriteTo(writer);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
                throw ex;
            }
        }
    }
}
