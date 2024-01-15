using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using BAStoryPlayer.Event.Internal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BAStoryPlayer.Event
{
    public static class EventBusUtility
    {
        public static IReadOnlyList<Type> EventTypes { get; private set; }
        public static IReadOnlyList<Type> StaticEventBusesTypes { get; private set; }

#if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; private set; }

        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= HandleEditorStateChange;
            EditorApplication.playModeStateChanged += HandleEditorStateChange;
        }

        private static void HandleEditorStateChange(PlayModeStateChange state)
        {
            PlayModeState = state;

            if (PlayModeState == PlayModeStateChange.EnteredEditMode)
            {
                ClearAllBuses();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type[] assemblyCSharp = null;
            Type[] assemblyCSharpFirstpass = null;

            for (int i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name == "Assembly-CSharp")
                    assemblyCSharp = assemblies[i].GetTypes();
                else
                    if (assemblies[i].GetName().Name == "Assembly-CSharp-firstpass")
                    assemblyCSharpFirstpass = assemblies[i].GetTypes();

                if (assemblyCSharp != null && assemblyCSharpFirstpass != null)
                    break;
            }

            List<Type> eventTypes = new List<Type>();

            for (int i = 0; i < assemblyCSharp.Length; i++)
            {
                var type = assemblyCSharp.GetType();
                if ((typeof(IEvent)) != type && (typeof(IEvent)).IsAssignableFrom(type))
                {
                    eventTypes.Add(type);
                }
            }

            if (assemblyCSharpFirstpass != null) 
            {
                for (int i = 0; i < assemblyCSharpFirstpass.Length; i++)
                {
                    var type = assemblyCSharpFirstpass.GetType();
                    if ((typeof(IEvent)) != type && (typeof(IEvent)).IsAssignableFrom(type))
                    {
                        eventTypes.Add(type);
                    }
                }
            }

            EventTypes = eventTypes;

            List<Type> staticEventBusesTypes = new List<Type>();
            var typedef = typeof(EventBus<>);
            for (int i = 0; i < EventTypes.Count; i++)
            {
                var type = EventTypes[i];
                var gentype = typedef.MakeGenericType(type);
                staticEventBusesTypes.Add(gentype);
            }

            StaticEventBusesTypes = staticEventBusesTypes;
        }
        
        // NOTE 如果播放完毕选择删除播放器 建议载入播放器前使用
        public static void ClearAllBuses()
        {
            for (int i = 0; i < StaticEventBusesTypes.Count; i++)
            {
                var type = EventTypes[i];
                var clearMethod = type.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
                clearMethod.Invoke(null, null);
            }
        }
    }

    public static class EventBus<T> where T : struct, IEvent
    {
        private static EventBinding<T> s_binding = new EventBinding<T>();
        private static List<Callback> s_callbacks = new List<Callback>();

        public static EventBinding<T> Binding
        {
            get
            {
                if (s_binding == null)
                    s_binding = new EventBinding<T>();
                return s_binding;
            }
        }

        public class Awaiter : EventBinding<T>
        {
            public bool IsEventRaised { get; private set; }
            public T Payload { get; private set; }

            public Awaiter() : base((Action)null)
            {
                ((IEventBindingInternal<T>)this).OnEvent = OnEvent;
            }

            private void OnEvent(T ev)
            {
                IsEventRaised = true;
                Payload = ev;
            }
        }

        private struct Callback
        {
            public Action onEventNoArg;
            public Action<T> onEvent;
        }

        public static void Clear()
        {
            s_binding = null;
            s_callbacks.Clear();
        }

        public static void AddCallback(Action callback)
        {
            if (callback == null)
                return;
            s_callbacks.Add(new Callback() { onEventNoArg = callback });
        }

        public static void AddCallback(Action<T> callback)
        {
            if (callback == null)
                return;
            s_callbacks.Add(new Callback() { onEvent = callback });
        }

        public static void ClearCallback()
        {
            s_callbacks.Clear();
        }

        public static void Raise()
        {
            Raise(default);
        }

        public static void Raise(T ev)
        {
#if UNITY_EDITOR
            if (EventBusUtility.PlayModeState == PlayModeStateChange.ExitingPlayMode)
                return;
#endif
            if (!Binding.IsBeingListen)
                return;

            IEventBindingInternal<T> internalBind = s_binding;
            internalBind.OnEvent?.Invoke(ev);
            internalBind.OnEventArgs?.Invoke();

            for (int i = 0; i < s_callbacks.Count; i++)
            {
                s_callbacks[i].onEvent?.Invoke(ev);
                s_callbacks[i].onEventNoArg?.Invoke();
            }

            s_callbacks.Clear();
        }

        public static Awaiter NewAwaiter()
        {
            return new Awaiter();
        }
    }
}
