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
            var jObj = JObject.Load(reader);
            var objType = jObj[TYPE].ToString();
            var objNm = jObj[NAME].ToString();
            var obj = EnumObject.AllEnums.FirstOrDefault(x => x.GetType().FullName == objType && x.Name == objNm) ?? throw new InvalidCastException();
            serializer.Populate(jObj.CreateReader(), obj);
            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = JToken.FromObject(value);
            var o = (JObject)t;
            o.ReplaceAll(new JProperty(TYPE, new JValue(value.GetType().FullName)));
            o.Add(new JProperty(NAME, new JValue((value as EnumObject).Name)));
            o.WriteTo(writer);
        }
    }
}
