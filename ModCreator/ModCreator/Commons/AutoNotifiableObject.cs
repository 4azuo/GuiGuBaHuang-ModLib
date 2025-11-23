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
        public const int AUTO_RENOTIFY_PERIOD = 100;
        public const int AUTO_RENOTIFY_MAX = 10;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        [JsonIgnore]
        private int _isPaused = 0;

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
        public Stack<PropertyInfo> ProcessingProperties { get; private set; }
        [JsonIgnore, IgnoredProperty]
        public List<string> PausedProperties { get; } = new List<string>();
        [JsonIgnore, IgnoredProperty]
        public bool IsPaused
        {
            get
            {
                return StaticLoad || _isPaused > 0;
            }
        }

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

        public void Pause()
        {
            _isPaused++;
        }

        public void Unpause()
        {
            if (_isPaused > 0) _isPaused--;
        }

        public void Pause(string propName)
        {
            PausedProperties.Add(propName);
        }

        public void Unpause(string propName)
        {
            PausedProperties.Remove(propName);
        }

        public void NotifyAll()
        {
            foreach (var prop in ListNotifyProperties[GetType()])
            {
                Notify(prop);
            }
        }

        public void Notify(string propName)
        {
            var prop = ListNotifyProperties[GetType()].FirstOrDefault(x => x.Name == propName);
            if (prop != null)
                Notify(prop);
        }

        public void RefreshAll(bool recursive = false)
        {
            foreach (var prop in ListNotifyProperties[GetType()])
            {
                Refresh(prop, recursive);
            }
        }

        public void Refresh(string propName, bool recursive = false)
        {
            var prop = ListNotifyProperties[GetType()].FirstOrDefault(x => x.Name == propName);
            if (prop != null)
                Refresh(prop, recursive);
        }

        private bool NotifyPropertyChanged(PropertyInfo prop)
        {
            if (PausedProperties.Contains(prop.Name)) return false;
            object val = HashValue(prop.GetValue(this));
            object befVal = null;
            PropertyOldValues.TryGetValue(prop, out befVal);
            if (!Equals(val, befVal))
            {
                Notify(prop, val);
                return true;
            }
            return false;
        }

        private void Notify(PropertyInfo prop, object val = null)
        {
            PropertyOldValues[prop] = val ?? HashValue(prop.GetValue(this));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop.Name));
            MethodInfo[] notiMethods = null;
            ListNotifyMethods.TryGetValue(prop, out notiMethods);
            if (notiMethods != null)
            {
                foreach (var m in notiMethods)
                {
                    m.Invoke(this, new object[] { prop.Name });
                }
            }
        }

        private void Refresh(PropertyInfo prop, bool recursive = false)
        {
            var val = prop.GetValue(this);
            PropertyOldValues[prop] = HashValue(val);
            if (recursive && val != null && typeof(AutoNotifiableObject).IsAssignableFrom(val.GetType()))
                ((AutoNotifiableObject)val).RefreshAll(true);
        }

        private object HashValue(object iValue)
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

        private void CreateAutoUpdater()
        {
            AutoUpdateTimer.Tick += AutoUpdate;
            if (!AutoUpdateTimer.IsEnabled) AutoUpdateTimer.Start();
        }

        private void AutoUpdate(object sender, EventArgs e)
        {
            if (IsPaused) return;
            if (ProcessingProperties == null)
            {
                PropertyInfo[] properties;
                if (ListNotifyProperties.TryGetValue(GetType(), out properties))
                    ProcessingProperties = new Stack<PropertyInfo>(properties);
                else
                    return;
            }
            int cnt = 0;
            while (ProcessingProperties.Count > 0)
            {
                var item = ProcessingProperties.Pop();
                if (NotifyPropertyChanged(item))
                {
                    cnt++;
                    if (cnt >= AUTO_RENOTIFY_MAX)
                        return;
                }
            }
            ProcessingProperties = null;
        }

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
            foreach (var p in ListNotifyProperties[thisType])
            {
                var notiMethodAtt = p.GetCustomAttribute<NotifyMethodAttribute>();
                if (notiMethodAtt != null && notiMethodAtt.Methods?.Length > 0)
                {
                    var listMethods = new List<MethodInfo>();
                    foreach (string mName in notiMethodAtt.Methods)
                    {
                        var runMethod = thisType.GetMethod(mName);
                        if (runMethod.GetParameters().Length != 1 && runMethod.GetParameters()[0].ParameterType != typeof(string)) throw new ArgumentException();
                        listMethods.Add(runMethod);
                    }
                    ListNotifyMethods.Add(p, listMethods.ToArray());
                }
            }
        }

        public AutoNotifiableObject()
        {
            var thisType = this.GetType();
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

        ~AutoNotifiableObject()
        {
            AutoUpdateTimer.Tick -= AutoUpdate;
        }
    }
}
