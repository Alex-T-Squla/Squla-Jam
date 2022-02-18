namespace Squla.Core.Metrics
{
    public class CountMetric
    {
        public int Count { get; private set; }

        internal CountMetric()
        {
        }

        public void Increment()
        {
            Count++;
        }

        internal void Reset()
        {
            Count = 0;
        }

        public override string ToString ()
        {
            return Count.ToString ();
        }
    }
}
