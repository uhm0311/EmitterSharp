using SimpleThreadMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EventEmitterSharp
{
    /// <summary>
    /// C# implementation of <see href="https://github.com/component/emitter">Emitter</see> in JavaScript module.
    /// </summary>
    public abstract class Emitter<E, T>
    {
        private readonly ConcurrentDictionary<E, List<Listener<T>>> Listeners = new ConcurrentDictionary<E, List<Listener<T>>>();
        private readonly object EventMutex = new object();

        public Emitter<E, T> On(E Event, Action<T> Callback)
        {
            return AddListener(Event, Callback, false);
        }

        public Emitter<E, T> On(E Event, Action Callback)
        {
            return AddListener(Event, Callback, false);
        }

        public Emitter<E, T> Once(E Event, Action<T> Callback)
        {
            return AddListener(Event, Callback, true);
        }

        public Emitter<E, T> Once(E Event, Action Callback)
        {
            return AddListener(Event, Callback, true);
        }

        private Emitter<E, T> AddListener(E Event, Delegate Callback, bool Once)
        {
            bool IsGenericAction = Callback is Action<T>;

            if (Event != null && (IsGenericAction || Callback is Action))
            {
                Listener<T> Listener = IsGenericAction ? new Listener<T>(Callback as Action<T>, Once) : new Listener<T>(Callback as Action, Once);

                Listeners.AddOrUpdate(Event, (_) => new List<Listener<T>>() { Listener }, (_, Listeners) =>
                {
                    SimpleMutex.Lock(EventMutex, () => Listeners.Add(Listener));

                    return Listeners;
                });
            }

            return this;
        }

        public Emitter<E, T> Off()
        {
            Listeners.Clear();

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
                    Listeners.TryRemove(Event, out _);
                }
                else
                {
                    if (this.Listeners.TryGetValue(Event, out List<Listener<T>> Listeners))
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
                if (this.Listeners.TryGetValue(Event, out List<Listener<T>> Listeners))
                {
                    for (int i = Listeners.Count - 1; i >= 0; i--)
                    {
                        SimpleMutex.Lock(EventMutex, () =>
                        {
                            Listener<T> Listener = Listeners[i];

                            if (Listener.Once)
                            {
                                Listeners.RemoveAt(i);
                            }

                            Listener.Invoke(Argument);
                        });
                    }
                }
            }

            return this;
        }

        public List<Delegate> GetListenerList(E Event)
        {
            List<Delegate> Result = new List<Delegate>();

            if (Event != null && this.Listeners.TryGetValue(Event, out List<Listener<T>> Listeners))
            {
                SimpleMutex.Lock(EventMutex, () =>
                {
                    foreach (Listener<T> Listener in Listeners)
                    {
                        Result.Add(Listener.Callback);
                    }
                });
            }

            return Result;
        }

        public int GetListenerCount(E Event)
        {
            if (this.Listeners.TryGetValue(Event, out List<Listener<T>> Listeners))
            {
                return Listeners.Count;
            }

            return 0;
        }

        public bool HasListener(E Event)
        {
            return Event != null && GetListenerCount(Event) > 0;
        }
    }
}
