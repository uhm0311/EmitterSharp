using System;
using System.Runtime.InteropServices;

namespace EventEmitterSharp
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
            Callback.DynamicInvoke(Argument);
        }

        public override bool Equals(object Object)
        {
            if (Object is Listener<T>)
            {
                Listener<T> Temp = Object as Listener<T>;

                return Callback.Equals(Temp.Callback) && Once.Equals(Temp.Once);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Callback.GetHashCode().ToString() + Once.GetHashCode().ToString()).GetHashCode();
        }
    }
}
