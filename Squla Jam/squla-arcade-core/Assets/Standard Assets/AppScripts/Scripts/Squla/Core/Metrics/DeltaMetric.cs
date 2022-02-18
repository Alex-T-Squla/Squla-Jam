namespace Squla.Core.Metrics
{
    public class DeltaMetric
    {
        public int Count { get; private set; }

        internal DeltaMetric()
        {
        }

        public void Increment()
        {
            Count++;
        }

        public void Decrement()
        {
            Count--;
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
