using ModLib.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Helps in discovery of the target of an Attribute
    /// </summary>
    /// <typeparam name="TAttribute">An Attribute derived type</typeparam>
    /// <remarks>
    /// The .NET framework does not provide navigation from attributes back to their targets, principally for the reason that 
    /// in typical usage scenarios for attributes, the attribute is discovered by a routine which already has a reference to a 
    /// member type.
    /// 
    /// There are, however, bona-fide cases where an attribute needs to detect it's target - an example is a localizable sub-class of the 
    /// DescriptionAttribute. In order for the DescriptionAttribute to return a localized string, it requires a resource key and, ideally,
    /// a type reference as the base-key for the ResourceManager. A DescriptionAttribute could not provide this information without
    /// a reference to it's target type.
    /// 
    /// Note to callers:
    /// 
    /// Your Attribute-derived class must implement Equals and GetHashCode, otherwise a run-time exception will occur, since this class
    /// creates a dictionary of attributes in order to speed up target lookups.
    /// </remarks>
    [ActionCatIgn]
    public static class AttributeTargetHelper<TAttribute>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Map of attributes and their respective targets
        /// </summary>
        private static Dictionary<TAttribute, object> targetMap;

        /// <summary>
        /// List of assemblies that should not be rescanned for types.
        /// </summary>
        private static List<string> skipAssemblies;

        /// <summary>
        /// Adds an attribute and it's target to the dictionary
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="item"></param>
        private static void Add(TAttribute attribute, object item)
        {
            targetMap.Add(attribute, item);
        }

        /// <summary>
        /// Scans an assembly for all instances of the attribute.
        /// </summary>
        /// <param name="assembly"></param>
        private static void ScanAssembly(Assembly assembly)
        {
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            if (!skipAssemblies.Contains(assembly.FullName))
            {
                skipAssemblies.Add(assembly.FullName);

                Debug.WriteLine("Loading attribute targets for " + typeof(TAttribute).Name + " from assembly " + assembly.FullName);

                foreach (TAttribute attr in assembly.GetCustomAttributes(typeof(TAttribute), false))
                    Add(attr, assembly);

                foreach (Type type in assembly.GetTypes())
                {
                    foreach (TAttribute attr in type.GetCustomAttributes(typeof(TAttribute), false))
                        Add(attr, type);

                    foreach (MemberInfo member in type.GetMembers(memberInfoBinding))
                    {
                        foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                            Add(attr, member);

                        if (member.MemberType == MemberTypes.Method)
                            foreach (var parameter in ((MethodInfo)member).GetParameters())
                                foreach (TAttribute attr in parameter.GetCustomAttributes(typeof(TAttribute), false))
                                    Add(attr, parameter);
                    }
                }
            }

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (!skipAssemblies.Contains(assemblyName.FullName))
                    ScanAssembly(Assembly.Load(assemblyName));
            }
        }

        /// <summary>
        /// Returns the target of an attribute.
        /// </summary>
        /// <param name="attribute">The attribute for which a target is sought</param>
        /// <returns>The target of the attribute - either an Assembly, Type or MemberInfo instance.</returns>
        [ActionCatIgn]
        public static object GetTarget(TAttribute attribute)
        {
            object result;
            if (!targetMap.TryGetValue(attribute, out result))
            {
                // Since types can be loaded at any time, recheck that all assemblies are included...
                // Walk up the stack in a last-ditch effort to find instances of the attribute.
                StackTrace stackTrace = new StackTrace();           // get call stack
                StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                // write call stack method names
                foreach (StackFrame stackFrame in stackFrames)
                {
                    Console.WriteLine(stackFrame.GetMethod().Name);   // write method name
                    ScanAssembly(stackFrame.GetMethod().GetType().Assembly);
                }

                if (!targetMap.TryGetValue(attribute, out result))
                    throw new InvalidProgramException("Cannot find assembly referencing attribute");
            }
            return result;
        }

        /// <summary>
        /// Static constructor for type.
        /// </summary>
        static AttributeTargetHelper()
        {
            targetMap = new Dictionary<TAttribute, object>();

            // Do not load any assemblies reference by the assembly which declares the attribute, since they cannot possibly use the attribute
            skipAssemblies = new List<string>(typeof(TAttribute).Assembly.GetReferencedAssemblies().Select(c => c.FullName));

            // Skip common system assemblies
            skipAssemblies.Add("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            skipAssemblies.Add("System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            skipAssemblies.Add("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            skipAssemblies.Add("System.Data.SqlXml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            skipAssemblies.Add("System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            skipAssemblies.Add("System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            // Scan the entire application
            ScanAssembly(Assembly.GetEntryAssembly());
        }

    }


    /// <summary>
    /// Extends attributes so that their targets can be discovered
    /// </summary>
    [ActionCatIgn]
    public static class AttributeHelperExtension
    {
        /// <summary>
        /// Gets the target of an attribute
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="attribute">The attribute for which a target is sought</param>
        /// <returns>The target of the attribute - either an Assembly, Type or MemberInfo instance.</returns>
        public static object GetTarget<TAttribute>(this TAttribute attribute)
            where TAttribute : Attribute
        {
            return AttributeTargetHelper<TAttribute>.GetTarget(attribute);
        }

        public static T GetAttributeOnMethodOrClass<T>(this MethodBase method) where T : Attribute
        {
            return method.GetCustomAttribute<T>() ?? method.DeclaringType.GetCustomAttribute<T>();
        }
    }
}