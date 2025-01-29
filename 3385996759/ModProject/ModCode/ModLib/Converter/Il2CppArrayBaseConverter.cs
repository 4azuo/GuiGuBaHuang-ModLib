//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections;
//using System.Linq;
//using UnhollowerBaseLib;

//namespace ModLib.Converter
//{
//    public class Il2CppArrayBaseConverter : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            return typeof(Il2CppArrayBase<>).IsAssignableFrom(objectType);
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            try
//            {
//                var jValue = JValue.Load(reader);
//                var array = jValue.ToString().Split('|');
//                var genericType = array.GetType().GetElementType();
//                return array.Select(x => x.ParseUnknown(genericType)).ToIl2CppList().ToArray();
//            }
//            catch (Exception ex)
//            {
//                DebugHelper.WriteLine(ex);
//                throw ex;
//            }
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            try
//            {
//                writer.WriteValueAsync(string.Join("|", value as IEnumerable));
//            }
//            catch (Exception ex)
//            {
//                DebugHelper.WriteLine(ex);
//                throw ex;
//            }
//        }
//    }
//}
