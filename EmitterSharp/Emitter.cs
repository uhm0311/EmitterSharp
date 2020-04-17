using SimpleThreadMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EmitterSharp
{
    /// <summary>
    /// C# implementation of <see href="https://github.com/component/emitter">Emitter</see> in JavaScript module.
    /// </summary>
    public abstract class Emitter<TChildClass, TEvent, TArgument> where TChildClass : Emitter<TChildClass, TEvent, TArgument>
    {
        private readonly ConcurrentDictionary<TEvent, List<Listener<TArgument>>> Listeners = new ConcurrentDictionary<TEvent, List<Listener<TArgument>>>();
        private readonly object EventMutex = new object();

        protected Emitter()
        {
            if (!(this is TChildClass))
            {
                throw new ArgumentException(string.Format(@"'{0}' does not match with '{1}'.", GetType().Name, typeof(TChildClass).Name));
            }
        }

        public TChildClass On(TEvent Event, Action<TArgument> Callback)
        {
            return AddListener(Event, Callback, false);
        }

        public TChildClass On(TEvent Event, Action Callback)
        {
            return AddListener(Event, Callback, false);
        }

        public TChildClass Once(TEvent Event, Action<TArgument> Callback)
        {
            return AddListener(Event, Callback, true);
        }

        public TChildClass Once(TEvent Event, Action Callback)
        {
            return AddListener(Event, Callback, true);
        }

        private TChildClass AddListener(TEvent Event, Delegate Callback, bool Once)
        {
            bool IsGenericAction = Callback is Action<TArgument>;

            if (Event != null && (IsGenericAction || Callback is Action))
            {
                Listener<TArgument> Listener = IsGenericAction ? new Listener<TArgument>(Callback as Action<TArgument>, Once) : new Listener<TArgument>(Callback as Action, Once);

                Listeners.AddOrUpdate(Event, (_) => new List<Listener<TArgument>>() { Listener }, (_, Listeners) =>
                {
                    SimpleMutex.Lock(EventMutex, () => Listeners.Add(Listener));

                    return Listeners;
                });
            }

            return this as TChildClass;
        }

        public TChildClass Off()
        {
            Listeners.Clear();

            return this as TChildClass;
        }

        public TChildClass Off(TEvent Event, bool Backward = false)
        {
            return RemoveListener(Event, null, Backward);
        }

        public TChildClass Off(TEvent Event, Action<TArgument> Callback, bool Backward = false)
        {
            return RemoveListener(Event, Callback, Backward);
        }

        public TChildClass Off(TEvent Event, Action Callback, bool Backward = false)
        {
            return RemoveListener(Event, Callback, Backward);
        }

        private TChildClass RemoveListener(TEvent Event, Delegate Callback, bool Backward)
        {
            if (Event != null)
            {
                if (Callback == null)
                {
                    Listeners.TryRemove(Event, out _);
                }
                else
                {
                    if (this.Listeners.TryGetValue(Event, out List<Listener<TArgument>> Listeners))
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

            return this as TChildClass;
        }

        public TChildClass Emit(TEvent Event, [Optional] TArgument Argument)
        {
            if (Event != null)
            {
                if (this.Listeners.TryGetValue(Event, out List<Listener<TArgument>> Listeners))
                {
                    SimpleMutex.Lock(EventMutex, () =>
                    {
                        for (int i = 0; i < Listeners.Count; i++)
                        {
                            Listener<TArgument> Listener = Listeners[i];

                            if (Listener.Once)
                            {
                                Listeners.RemoveAt(i--);
                            }

                            Listener.Invoke(Argument);
                        }
                    });
                }
            }

            return this as TChildClass;
        }

        public List<Delegate> GetListenerList(TEvent Event)
        {
            List<Delegate> Result = new List<Delegate>();

            if (Event != null && this.Listeners.TryGetValue(Event, out List<Listener<TArgument>> Listeners))
            {
                SimpleMutex.Lock(EventMutex, () =>
                {
                    foreach (Listener<TArgument> Listener in Listeners)
                    {
                        Result.Add(Listener.Callback);
                    }
                });
            }

            return Result;
        }

        public int GetListenerCount(TEvent Event)
        {
            if (this.Listeners.TryGetValue(Event, out List<Listener<TArgument>> Listeners))
            {
                return Listeners.Count;
            }

            return 0;
        }

        public bool HasListener(TEvent Event)
        {
            return Event != null && GetListenerCount(Event) > 0;
        }
    }
}
