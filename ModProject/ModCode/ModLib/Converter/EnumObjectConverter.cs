using Il2CppSystem.Data;
using ModLib.Enum;
using ModLib.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModLib.Converter
{
    public class EnumObjectConverter : JsonConverter
    {
        private const string TYPE = "$type";
        private const string NAME = "ObjName";
        private const string KEY = "ObjSeq";

        public override bool CanConvert(Type objectType)
        {
            return typeof(EnumObject).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObj = JObject.Load(reader);
            var objType = jObj[TYPE].ToString();
            var objNm = jObj[NAME].ToString();
            var objSeq = int.Parse(jObj[KEY].ToString());
            var obj = EnumObject.AllEnums.FirstOrDefault(x => x.ObjSeq == objSeq);
            if (obj.GetType().FullName != objType)
                throw new InvalidCastException();
            if (obj.Name != objNm)
                throw new InvalidCastException();
            serializer.Populate(jObj.CreateReader(), obj);
            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = JToken.FromObject(value);
            var o = (JObject)t;
            o.ReplaceAll(new JProperty(TYPE, new JValue(value.GetType().FullName)));
            o.Add(new JProperty(NAME, new JValue((value as EnumObject).Name)));
            o.Add(new JProperty(KEY, new JValue((value as EnumObject).ObjSeq)));
            o.WriteTo(writer);
        }
    }
}
