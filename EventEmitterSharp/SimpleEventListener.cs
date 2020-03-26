namespace EventEmitterSharp
{
    public delegate void SimpleListenerAction(params object[] Arguments);

    public class SimpleEventListener
    {
        public SimpleListenerAction Callback { get; private set; }
        public bool Once { get; private set; }

        internal SimpleEventListener(SimpleListenerAction Callback, bool Once = false)
        {
            this.Callback = Callback;
            this.Once = Once;
        }

        public override bool Equals(object obj)
        {
            if (obj is SimpleEventListener)
            {
                SimpleEventListener o = obj as SimpleEventListener;

                return Callback.Equals(o.Callback) && Once.Equals(o.Once);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Callback.GetHashCode().ToString() + Once.GetHashCode().ToString()).GetHashCode();
        }
    }
}
