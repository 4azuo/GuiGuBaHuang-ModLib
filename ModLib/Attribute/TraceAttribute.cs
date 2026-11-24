//// using MethodBoundaryAspect.Fody.Attributes;
//// using System;
//// using System.Linq;
//// using System.Reflection;

//namespace ModLib.Attributes
//{
//    // [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
//    // public class TraceAttribute : OnMethodBoundaryAspect
//    // {
//    //     public string EntryCustomFormat { get; set; } = null;
//    //     public string EntryCustomFormatValue { get; set; } = null; //sample: "{1}=PROPERTY_NAME|{2}=PROPERTY_NAME"
//    //     public string EntryDefaultFormat { get; } = "Call {0}.{1}({2})";
//    //     public string ExitCustomFormat { get; set; } = null;
//    //     public string ExitCustomFormatValue { get; set; } = null; //sample: "{1}=PROPERTY_NAME|{2}=PROPERTY_NAME"
//    //     public string ExitDefaultFormat { get; } = " => {0}";
//    //     public bool IgnoreExitMsg { get; set; } = false;
//    //     public bool IgnoreException { get; set; } = true;
//    //     public static bool Enable { get; set; } = true;

//    //     private int _entryPos;

//    //     public override void OnEntry(MethodExecutionArgs args)
//    //     {
//    //         if (Enable && IsNotIgnoredTrace(args.Method))
//    //         {
//    //             _entryPos = DebugHelper.WriteLine(GetCallLog(args));
//    //         }
//    //     }

//    //     public override void OnExit(MethodExecutionArgs args)
//    //     {
//    //         //if (Enable && !IgnoreExitMsg && IsNotIgnoredTrace(args.Method))
//    //         //{
//    //         //    DebugHelper.WriteAt(_entryPos, GetReturnLog(args));
//    //         //}
//    //     }

//    //     public override void OnException(MethodExecutionArgs args)
//    //     {
//    //         if (Enable && !IgnoreException && IsNotIgnoredTrace(args.Method))
//    //         {
//    //             DebugHelper.WriteLine(args.Exception);
//    //         }
//    //     }

//    //     private bool IsNotIgnoredTrace(MethodBase method)
//    //     {
//    //         return method.GetCustomAttribute<TraceIgnoreAttribute>() == null &&
//    //             method.DeclaringType.GetCustomAttribute<TraceIgnoreAttribute>() == null;
//    //     }

//    //     #region Log
//    //     private string GetCallLog(MethodExecutionArgs args)
//    //     {
//    //         if (string.IsNullOrEmpty(EntryCustomFormat))
//    //         {
//    //             return string.Format(EntryDefaultFormat, DeclaringClassName(args), args.Method.Name, string.Join(", ", args.Arguments.Select(x => ParseValueStr(x))));
//    //         }
//    //         else
//    //         {
//    //             return CustomFormat(EntryCustomFormat, EntryCustomFormatValue, args);
//    //         }
//    //     }

//    //     private string GetReturnLog(MethodExecutionArgs args)
//    //     {
//    //         if (string.IsNullOrEmpty(ExitCustomFormat))
//    //         {
//    //             return string.Format(ExitDefaultFormat, ParseValueStr(args.ReturnValue));
//    //         }
//    //         else
//    //         {
//    //             return CustomFormat(ExitCustomFormat, ExitCustomFormatValue, args);
//    //         }
//    //     }

//    //     private string CustomFormat(string format, string formatValue, MethodExecutionArgs args)
//    //     {
//    //         foreach (var x in formatValue.Split('|'))
//    //         {
//    //             if (!string.IsNullOrEmpty(x))
//    //             {
//    //                 var y = x.Split('=');
//    //                 var replacement = y[0].Trim();
//    //                 var propName = y[1].Trim();
//    //                 switch (propName.ToUpper())
//    //                 {
//    //                     case "CLASS_NAME":
//    //                         format = format.Replace(replacement, DeclaringClassName(args));
//    //                         break;
//    //                     case "METHOD_NAME":
//    //                         format = format.Replace(replacement, args.Method.Name);
//    //                         break;
//    //                     case "PARAMS_VALUE":
//    //                         format = format.Replace(replacement, string.Join(", ", args.Arguments.Select(arg => ParseValueStr(arg))));
//    //                         break;
//    //                     case "RETURN_VALUE":
//    //                         format = format.Replace(replacement, ParseValueStr(args.ReturnValue));
//    //                         break;
//    //                     default:
//    //                         var prop = args.Method.DeclaringType.GetProperty(propName);
//    //                         object propValue;
//    //                         if (args.Instance == null)
//    //                         {
//    //                             propValue = prop.GetValue(null);
//    //                         }
//    //                         else
//    //                         {
//    //                             propValue = prop.GetValue(args.Instance);
//    //                         }
//    //                         format = format.Replace(replacement, ParseValueStr(propValue));
//    //                         break;
//    //                 }
//    //             }
//    //         }
//    //         return format;
//    //     }

//    //     private string ParseValueStr(object v)
//    //     {
//    //         if (v is string x)
//    //         {
//    //             if (string.IsNullOrEmpty(x))
//    //             {
//    //                 return "null";
//    //             }
//    //         }
//    //         if (v == null)
//    //         {
//    //             return "null";
//    //         }
//    //         return v.Parse<string>("?");
//    //     }

//    //     private string DeclaringClassName(MethodExecutionArgs args)
//    //     {
//    //         return (args.Instance == null ? "" : "$") + (args.Instance?.GetType().Name ?? args.Method.ReflectedType.Name);
//    //     }
//    //     #endregion
//    // }
//}