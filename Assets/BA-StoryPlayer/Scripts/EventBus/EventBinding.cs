using System;
using BAStoryPlayer.Event.Internal;

namespace BAStoryPlayer.Event.Internal
{
    internal interface IEventBindingInternal<T> where T : struct, IEvent
    {
        public Action<T> OnEvent { get; set; }
        public Action OnEventArgs { get; set; }
    }
}

namespace BAStoryPlayer.Event
{
    public class EventBinding<T> : IEventBindingInternal<T> where T : struct, IEvent
    {
        private bool isBeingListen;

        private Action<T> onEvent;
        private Action onEventNoArgs;

        public bool IsBeingListen
        {
            get => isBeingListen;
            set => SetListen(value);
        }

        Action<T> IEventBindingInternal<T>.OnEvent { get => onEvent; set => onEvent = value; }
        Action IEventBindingInternal<T>.OnEventArgs { get => onEventNoArgs; set => onEventNoArgs = value; }

        public EventBinding(Action<T> onEvent)
        {
            this.onEvent = onEvent;
            this.IsBeingListen = true;
        }

        public EventBinding(Action onEventNoArgs)
        {
            this.onEventNoArgs = onEventNoArgs;
            this.IsBeingListen = true;
        }

        public EventBinding()
        {
            this.IsBeingListen = true;
        }

        public void Add(Action<T> onEvent) => this.onEvent += onEvent;
        public void Remove(Action<T> onEvent) => this.onEvent -= onEvent;

        public void Add(Action onEvent) => onEventNoArgs += onEvent;
        public void Remove(Action onEvent) => onEventNoArgs -= onEvent;

        private void SetListen(bool value)
        {
            if (value == isBeingListen)
                return;
            isBeingListen = value;
        }

        public static implicit operator EventBinding<T>(Action onEventNoArgs)
        {
            return new EventBinding<T>(onEventNoArgs);
        }

        public static implicit operator EventBinding<T>(Action<T> onEvent)
        {
            return new EventBinding<T>(onEvent);
        }

        public static implicit operator bool(EventBinding<T> bind)
        {
            return bind != null;
        }
    }
}
