using ModCreator.Attributes;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace ModCreator.Commons
{
    public abstract class AutoNotifiableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Consts
        /// </summary>
        public const int AUTO_RENOTIFY_PERIOD = 125;
        public const int AUTO_RENOTIFY_MAX = 8;

        /// <summary>
        /// Delegates
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Variables
        /// </summary>
        [JsonIgnore]
        private int _isPaused = 0;

        /// <summary>
        /// Properties
        /// </summary>
        public static readonly DispatcherTimer AutoUpdateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(AUTO_RENOTIFY_PERIOD), DispatcherPriority.Background, (s, e) => { }, Application.Current.Dispatcher);
        public static readonly Dictionary<Type, PropertyInfo[]> ListNotifyProperties = new Dictionary<Type, PropertyInfo[]>();
        public static readonly Dictionary<PropertyInfo, MethodInfo[]> ListNotifyMethods = new Dictionary<PropertyInfo, MethodInfo[]>();
        public static readonly List<Type> LoadedTypes = new List<Type>();


        [JsonIgnore, IgnoredProperty]
        public static bool StaticLoad { get; private set; } = false;
        [JsonIgnore, IgnoredProperty]
        public static List<AutoNotifiableObject> StaticLoadObjs { get; } = new List<AutoNotifiableObject>();


        [JsonIgnore, IgnoredProperty]
        public Dictionary<PropertyInfo, object> PropertyOldValues { get; } = new Dictionary<PropertyInfo, object>();
        [JsonIgnore, IgnoredProperty]
        public List<PropertyInfo> PausedProperties { get; } = new List<PropertyInfo>();
        [JsonIgnore, IgnoredProperty]
        public bool IsPaused
        {
            get
            {
                return StaticLoad || _isPaused > 0;
            }
        }
        [JsonIgnore, IgnoredProperty]
        public bool IsNotifyDown { get; private set; }
        [JsonIgnore, IgnoredProperty]
        public int NotifyIndex { get; private set; }

        /// <summary>
        /// Begin/End Load
        /// </summary>
        public static void Begin()
        {
            StaticLoadObjs.Clear();
            StaticLoad = true;
        }

        public static void End()
        {
            StaticLoad = false;
            foreach (var obj in StaticLoadObjs)
            {
                obj.RefreshAll(true);
            }
        }

        /// <summary>
        /// Methods
        /// </summary>
        public void Pause()
        {
            _isPaused++;
        }

        public void Unpause()
        {
            if (_isPaused > 0) _isPaused--;
        }

        public void Pause(PropertyInfo prop)
        {
            PausedProperties.Add(prop);
        }

        public void Unpause(PropertyInfo prop)
        {
            PausedProperties.Remove(prop);
        }

        public void NotifyAll()
        {
            foreach (var prop in ListNotifyProperties[GetType()])
            {
                var val = PropertyOldValues.ContainsKey(prop) ? PropertyOldValues[prop] : null;
                Notify(prop, val);
            }
        }

        public void Notify(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            var p = ListNotifyProperties[GetType()].FirstOrDefault(x => x.Name == prop.Name);
            if (p != null)
            {
                var val = PropertyOldValues.TryGetValue(prop, out object value) ? value : null;
                Notify(prop, val);
            }
        }

        public void RefreshAll(bool recursive = false)
        {
            foreach (var prop in ListNotifyProperties[GetType()])
            {
                Refresh(prop, recursive);
            }
        }

        public void Refresh(object obj, PropertyInfo prop, object oldValue, object newValue, bool recursive = false)
        {
            var p = ListNotifyProperties[GetType()].FirstOrDefault(x => x.Name == prop.Name);
            if (p != null)
                Refresh(p, recursive);
        }

        /// <summary>
        /// NotifyPropertyChanged
        /// </summary>
        private void NotifyPropertyChanged(PropertyInfo prop)
        {
            if (PausedProperties.Contains(prop))
                return;
            object val = GetCheckSumValue(prop.GetValue(this));
            object befVal = null;
            PropertyOldValues.TryGetValue(prop, out befVal);
            if (!Equals(val, befVal))
            {
                Notify(prop, val);
            }
        }

        private void Notify(PropertyInfo prop, object val = null)
        {
            //reassign value
            var oldValue = PropertyOldValues.ContainsKey(prop) ? PropertyOldValues[prop] : null;
            PropertyOldValues[prop] = val ?? GetCheckSumValue(prop.GetValue(this));

            //self
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop.Name));

            //notify methods
            MethodInfo[] notiMethods = null;
            ListNotifyMethods.TryGetValue(prop, out notiMethods);
            if (notiMethods != null)
            {
                foreach (var m in notiMethods)
                {
                    m.Invoke(this, new object[] { this, prop, oldValue, val });
                }
            }
        }

        private void Refresh(PropertyInfo prop, bool recursive = false)
        {
            var val = prop.GetValue(this);
            PropertyOldValues[prop] = GetCheckSumValue(val);
            if (recursive && val != null && typeof(AutoNotifiableObject).IsAssignableFrom(val.GetType()))
                ((AutoNotifiableObject)val).RefreshAll(true);
        }

        private object GetCheckSumValue(object iValue)
        {
            if (iValue == null) return null;
            var enumerable = typeof(IEnumerable);
            if (enumerable.IsAssignableFrom(iValue.GetType()))
            {
                long sum = 0;
                long index = 0;
                foreach (var i in (IEnumerable)iValue)
                {
                    sum += i?.GetHashCode() ?? index;
                    index++;
                }
                return sum;
            }
            else
            {
                return iValue;
            }
        }

        /// <summary>
        /// Create AutoUpdateTimer
        /// </summary>
        private void CreateAutoUpdater()
        {
            AutoUpdateTimer.Tick += AutoUpdate;
            if (!AutoUpdateTimer.IsEnabled) AutoUpdateTimer.Start();
        }

        private void AutoUpdate(object sender, EventArgs e)
        {
            if (IsPaused) return;

            PropertyInfo[] properties;
            if (!ListNotifyProperties.TryGetValue(GetType(), out properties))
                return;

            for (int i = 0; i < AUTO_RENOTIFY_MAX; i++)
            {
                if (IsNotifyDown)
                    NotifyIndex++;
                else
                    NotifyIndex--;
                if (NotifyIndex < 0)
                    NotifyIndex = properties.Length - 1;
                if (NotifyIndex >= properties.Length)
                    NotifyIndex = 0;
                var item = properties[NotifyIndex];
                NotifyPropertyChanged(item);
            }
        }

        /// <summary>
        /// Prepare
        /// </summary>
        private void PrepareNotifyProperties()
        {
            var thisType = GetType();
            ListNotifyProperties[thisType] = thisType.GetProperties()
                .Where(p =>
                {
                    return p.CanRead && p.GetCustomAttribute<IgnoredPropertyAttribute>() == null;
                }).ToArray();
        }

        private void PrepareNotifyMethods()
        {
            var thisType = GetType();

            //class
            var classMethods = new List<MethodInfo>();
            var classMethodAtt = thisType.GetCustomAttribute<NotifyMethodAttribute>();
            if (classMethodAtt != null && classMethodAtt.Methods?.Length > 0)
            {
                foreach (string mName in classMethodAtt.Methods)
                {
                    var runMethod = thisType.GetMethod(mName);
                    CheckNotifyMethod(runMethod);
                    classMethods.Add(runMethod);
                }
            }

            //each properties
            foreach (var p in ListNotifyProperties[thisType])
            {
                var listMethods = new List<MethodInfo>();
                var notiMethodAtt = p.GetCustomAttribute<NotifyMethodAttribute>();
                if (notiMethodAtt != null && notiMethodAtt.Methods?.Length > 0)
                {
                    foreach (string mName in notiMethodAtt.Methods)
                    {
                        var runMethod = thisType.GetMethod(mName);
                        CheckNotifyMethod(runMethod);
                        listMethods.Add(runMethod);
                    }
                }
                listMethods.AddRange(classMethods);
                ListNotifyMethods.Add(p, listMethods.ToArray());
            }
        }

        /// <summary>
        /// Check notifying method
        /// </summary>
        private void CheckNotifyMethod(MethodInfo m)
        {
            if (m == null)
                throw new MissingMethodException();
            if (m.GetParameters().Length != 4 ||
                m.GetParameters()[1].ParameterType != typeof(PropertyInfo))
                throw new ArgumentException();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AutoNotifiableObject()
        {
            var thisType = GetType();
            IsNotifyDown = thisType.GetCustomAttribute<NotifyDirectAttribute>()?.WayDown ?? false;
            if (!LoadedTypes.Contains(thisType))
            {
                LoadedTypes.Add(thisType);
                PrepareNotifyProperties();
                PrepareNotifyMethods();
            }
            CreateAutoUpdater();
            if (StaticLoad)
                StaticLoadObjs.Add(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~AutoNotifiableObject()
        {
            AutoUpdateTimer.Tick -= AutoUpdate;
        }
    }
}