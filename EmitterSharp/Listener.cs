using System;
using System.Runtime.InteropServices;

namespace EmitterSharp
{
    internal class Listener<TArgument>
    {
        public Delegate Callback { get; private set; }
        public bool Once { get; private set; }

        internal Listener(Action<TArgument> Callback, bool Once)
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

        public void Invoke([Optional] TArgument Argument)
        {
            bool IsGenericAction = Callback is Action<TArgument>;

            if (IsGenericAction || Callback is Action)
            {
                if (IsGenericAction)
                {
                    (Callback as Action<TArgument>).Invoke(Argument);
                }
                else
                {
                    (Callback as Action).Invoke();
                }
            }
        }

        public override bool Equals(object Object)
        {
            if (Object is Listener<TArgument>)
            {
                Listener<TArgument> Temp = Object as Listener<TArgument>;

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
