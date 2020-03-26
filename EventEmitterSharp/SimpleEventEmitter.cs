using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventEmitterSharp
{
    public abstract class SimpleEventEmitter
    {
        private readonly ConcurrentDictionary<object, List<SimpleEventListener>> EventListeners = new ConcurrentDictionary<object, List<SimpleEventListener>>();

        public void On(object Event, SimpleListenerAction Callback)
        {
            AddEventListener(Event, Callback, false);
        }

        public void Once(object Event, SimpleListenerAction Callback)
        {
            AddEventListener(Event, Callback, true);
        }

        private void AddEventListener(object Event, SimpleListenerAction Callback, bool Once = false)
        {
            if (IsValid(Event, Callback))
            {
                if (!EventListeners.ContainsKey(Event))
                {
                    EventListeners.TryAdd(Event, new List<SimpleEventListener>());
                }

                EventListeners[Event].Add(new SimpleEventListener(Callback, Once));
            }
        }

        public void Off(object Event, SimpleListenerAction Callback)
        {
            if (IsValid(Event, Callback) && EventListeners.ContainsKey(Event))
            {
                for (int i = EventListeners[Event].Count - 1; i >= 0; i--)
                {
                    if (EventListeners[Event][i].Callback.Equals(Callback))
                    {
                        EventListeners[Event].RemoveAt(i);
                        return;
                    }
                }
            }
        }

        protected void Emit(object Event, params object[] Arguments)
        {
            if (Event != null)
            {
                if (EventListeners.ContainsKey(Event))
                {
                    for (int i = EventListeners[Event].Count - 1; i >= 0; i--)
                    {
                        SimpleEventListener EventListener = EventListeners[Event][i];

                        if (EventListener.Once)
                        {
                            EventListeners[Event].RemoveAt(i);
                        }

                        try { EventListener.Callback(Arguments); }
                        catch { }
                    }
                }
            }
        }

        private bool IsValid(object Event, SimpleListenerAction Callback)
        {
            return Event != null && Callback != null;
        }
    }
}
