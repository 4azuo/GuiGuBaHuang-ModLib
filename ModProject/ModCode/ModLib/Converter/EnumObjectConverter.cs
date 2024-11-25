using ModLib.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ModLib.Converter
{
    public class EnumObjectConverter : JsonConverter
    {
        private const string TYPE = "$type";
        private const string NAME = "ObjName";

        public override bool CanConvert(Type objectType)
        {
            return typeof(EnumObject).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                //new
                var jValue = JValue.Load(reader);
                if (jValue != null)
                {
                    var value = jValue.ToString();
                    var s = value.Split('|');
                    return EnumObject.AllEnums.FirstOrDefault(x => x.GetType().FullName == s[0] && x.Name == s[1]) ?? throw new InvalidCastException();
                }

                //old
                var jObj = JObject.Load(reader);
                if (jObj != null)
                {
                    var objType = jObj[TYPE].ToString();
                    var objNm = jObj[NAME].ToString();
                    var obj = EnumObject.AllEnums.FirstOrDefault(x => x.GetType().FullName == objType && x.Name == objNm) ?? throw new InvalidCastException();
                    serializer.Populate(jObj.CreateReader(), obj);
                    return obj;
                }

                throw new InvalidCastException();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
                throw ex;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //new
            var t = JToken.FromObject(value);
            var o = (JObject)t;
            o.ReplaceAll(new JValue($"{value.GetType().FullName}|{(value as EnumObject).Name}"));
            o.WriteTo(writer);

            //old
            //var t = JToken.FromObject(value);
            //var o = (JObject)t;
            //o.ReplaceAll(new JProperty(TYPE, new JValue(value.GetType().FullName)));
            //o.Add(new JProperty(NAME, new JValue((value as EnumObject).Name)));
            //o.WriteTo(writer);
        }
    }
}
