using SimpleThreadMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EventEmitterSharp
{
    /// <summary>
    /// C# implementation of <see href="https://github.com/component/emitter">Emitter in JavaScript module</see>.
    /// </summary>
    public abstract class Emitter<E, T>
    {
        private readonly ConcurrentDictionary<E, List<Listener<T>>> EventListeners = new ConcurrentDictionary<E, List<Listener<T>>>();
        private readonly object EventMutex = new object();

        public Emitter<E, T> On(E Event, Action<T> Callback)
        {
            return AddEventListener(Event, Callback, false);
        }

        public Emitter<E, T> On(E Event, Action Callback)
        {
            return AddEventListener(Event, Callback, false);
        }

        public Emitter<E, T> Once(E Event, Action<T> Callback)
        {
            return AddEventListener(Event, Callback, true);
        }

        public Emitter<E, T> Once(E Event, Action Callback)
        {
            return AddEventListener(Event, Callback, true);
        }

        private Emitter<E, T> AddEventListener(E Event, Delegate Callback, bool Once)
        {
            if (Event != null && Callback != null)
            {
                Listener<T> Listener;

                if (Callback is Action)
                {
                    Listener = new Listener<T>(Callback as Action, Once);
                }
                else
                {
                    Listener = new Listener<T>(Callback as Action<T>, Once);
                }

                EventListeners.AddOrUpdate(Event, (_) => new List<Listener<T>>() { Listener }, (_, Listeners) =>
                {
                    SimpleMutex.Lock(EventMutex, () => Listeners.Add(Listener));

                    return Listeners;
                });
            }

            return this;
        }

        public Emitter<E, T> Off()
        {
            EventListeners.Clear();

            return this;
        }

        public Emitter<E, T> Off(E Event, Action<T> Callback = null, bool Backward = false)
        {
            return RemoveListener(Event, Callback, Backward);
        }

        public Emitter<E, T> Off(E Event, Action Callback = null, bool Backward = false)
        {
            return RemoveListener(Event, Callback, Backward);
        }

        private Emitter<E, T> RemoveListener(E Event, Delegate Callback, bool Backward)
        {
            if (Event != null)
            {
                if (Callback == null)
                {
                    EventListeners.TryRemove(Event, out _);
                }
                else
                {
                    if (EventListeners.TryGetValue(Event, out List<Listener<T>> Listeners))
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

        public Emitter<E, T> Emit(E Event, [Optional] T Argument)
        {
            if (Event != null)
            {
                if (EventListeners.TryGetValue(Event, out List<Listener<T>> Listeners))
                {
                    for (int i = Listeners.Count - 1; i >= 0; i--)
                    {
                        SimpleMutex.Lock(EventMutex, () =>
                        {
                            Listener<T> EventListener = Listeners[i];

                            if (EventListener.Once)
                            {
                                Listeners.RemoveAt(i);
                            }

                            EventListener.Invoke(Argument);
                        });
                    }
                }
            }

            return this;
        }

        public List<Delegate> GetListeners(E Event)
        {
            List<Delegate> Result = new List<Delegate>();

            if (Event != null && EventListeners.TryGetValue(Event, out List<Listener<T>> Listeners))
            {
                foreach (Listener<T> Listener in Listeners)
                {
                    Result.Add(Listener.Callback);
                }
            }

            return Result;
        }

        public bool HasListeners(E Event)
        {
            return Event != null && EventListeners.TryGetValue(Event, out List<Listener<T>> Listeners) && Listeners.Count > 0;
        }
    }
}
