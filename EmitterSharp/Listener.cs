using System;
using System.Runtime.InteropServices;

namespace EmitterSharp
{
    internal class Listener<T>
    {
        public Delegate Callback { get; private set; }
        public bool Once { get; private set; }

        internal Listener(Action<T> Callback, bool Once)
        {
            Initialize(Callback, Once);
        }

        internal Listener(Action Callback, bool Once)
        {
            Initialize(Callback, Once);
        }

        private void Initialize(Delegate Callback, bool Once)
        {
            this.Callback = Callback;
            this.Once = Once;
        }

        public void Invoke([Optional] T Argument)
        {
            bool IsGenericAction = Callback is Action<T>;

            if (IsGenericAction || Callback is Action)
            {
                if (IsGenericAction)
                {
                    (Callback as Action<T>).Invoke(Argument);
                }
                else
                {
                    (Callback as Action).Invoke();
                }
            }
        }

        public override bool Equals(object Object)
        {
            if (Object is Listener<T>)
            {
                Listener<T> Temp = Object as Listener<T>;

                return Once.Equals(Temp.Once) && Callback.Equals(Temp.Callback);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Callback.GetHashCode().ToString() + Once.GetHashCode().ToString()).GetHashCode();
        }
    }
}
