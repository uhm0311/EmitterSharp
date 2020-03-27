namespace EventEmitterSharp
{
    public delegate void SimpleListenerAction<T>(T Arguments);

    public class SimpleEventListener<T>
    {
        public SimpleListenerAction<T> Callback { get; private set; }
        public bool Once { get; private set; }

        internal SimpleEventListener(SimpleListenerAction<T> Callback, bool Once = false)
        {
            this.Callback = Callback;
            this.Once = Once;
        }

        public override bool Equals(object Object)
        {
            if (Object is SimpleEventListener<T>)
            {
                SimpleEventListener<T> Temp = Object as SimpleEventListener<T>;

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
