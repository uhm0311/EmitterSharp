using SimpleThreadMonitor;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventEmitterSharp
{
    /// <summary>
    /// C# implementation of <see href="https://github.com/socketio/engine.io-client-java/blob/master/src/main/java/io/socket/emitter/Emitter.java">Emitter</see>.
    /// </summary>
    public abstract class SimpleEventEmitter<E, T>
    {
        private readonly ConcurrentDictionary<E, List<SimpleEventListener<T>>> EventListeners = new ConcurrentDictionary<E, List<SimpleEventListener<T>>>();
        private readonly object EventMutex = new object();

        public SimpleEventEmitter<E, T> On(E Event, SimpleListenerAction<T> Callback)
        {
            return AddEventListener(Event, Callback, false);
        }

        public SimpleEventEmitter<E, T> Once(E Event, SimpleListenerAction<T> Callback)
        {
            return AddEventListener(Event, Callback, true);
        }

        private SimpleEventEmitter<E, T> AddEventListener(E Event, SimpleListenerAction<T> Callback, bool Once)
        {
            if (Event != null && Callback != null)
            {
                SimpleEventListener<T> Listener = new SimpleEventListener<T>(Callback, Once);

                EventListeners.AddOrUpdate(Event, (_) => new List<SimpleEventListener<T>>() { Listener }, (_, Listeners) =>
                {
                    SimpleMutex.Lock(EventMutex, () => Listeners.Add(Listener));

                    return Listeners;
                });
            }

            return this;
        }

        public SimpleEventEmitter<E, T> Off()
        {
            EventListeners.Clear();

            return this;
        }

        public SimpleEventEmitter<E, T> Off(E Event, SimpleListenerAction<T> Callback = null, bool Backward = false)
        {
            if (Event != null)
            {
                if (Callback == null)
                {
                    EventListeners.TryRemove(Event, out _);
                }
                else
                {
                    if (EventListeners.TryGetValue(Event, out List<SimpleEventListener<T>> Listeners))
                    {
                        SimpleMutex.Lock(EventMutex, () =>
                        {
                            if (!Backward)
                            {
                                for (int i = 0; i < Listeners.Count; i++)
                                {
                                    if (Listeners[i].Callback.Equals(Callback))
                                    {
                                        Listeners.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = Listeners.Count - 1; i >= 0; i--)
                                {
                                    if (Listeners[i].Callback.Equals(Callback))
                                    {
                                        Listeners.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        });
                    }
                }
            }

            return this;
        }

        public SimpleEventEmitter<E, T> Emit(E Event, T Argument)
        {
            if (Event != null)
            {
                if (EventListeners.TryGetValue(Event, out List<SimpleEventListener<T>> Listeners))
                {
                    for (int i = Listeners.Count - 1; i >= 0; i--)
                    {
                        SimpleMutex.Lock(EventMutex, () =>
                        {
                            SimpleEventListener<T> EventListener = Listeners[i];

                            if (EventListener.Once)
                            {
                                Listeners.RemoveAt(i);
                            }

                            EventListener.Callback(Argument);
                        });
                    }
                }
            }

            return this;
        }

        public List<SimpleListenerAction<T>> GetListeners(E Event)
        {
            List<SimpleListenerAction<T>> Result = new List<SimpleListenerAction<T>>();

            if (Event != null && EventListeners.TryGetValue(Event, out List<SimpleEventListener<T>> Listeners))
            {
                foreach (SimpleEventListener<T> Listener in Listeners)
                {
                    Result.Add(Listener.Callback);
                }
            }

            return Result;
        }

        public bool HasListeners(E Event)
        {
            return Event != null && EventListeners.TryGetValue(Event, out List<SimpleEventListener<T>> Listeners) && Listeners.Count > 0;
        }
    }
}
