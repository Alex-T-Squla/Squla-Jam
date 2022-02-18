using System;
using System.Collections.Generic;
using System.Linq;
using Squla.Core.IOC;

namespace Squla.Core.Network
{
    [Singleton]
    public class SpinnerModel
    {
        private DateTime transactionStartTime;
        private HashSet<string> names = new HashSet<string>();

        public event Action StartSpinner;
        public event Action<int> StopSpinner;
        public event Action Changed;

        public int UniqueTransactions { get; private set; }

        public int TransactionCount {
            get { return names.Count; }
        }

        public int TransactionDuration {
            get {
                var dt = DateTime.Now - transactionStartTime;
                var seconds = (int)Math.Round (dt.TotalSeconds);
                // it is not gaurenteed to have possitive value.
                return seconds < 0 ? 0 : seconds;
            }
        }

        public string[] TransactionNames {
            get { return names.ToArray(); }
        }

        [Inject]
        public SpinnerModel()
        {
        }

	    public void Clear()
	    {
		    names.Clear();
	    }

        public void Begin(string name)
        {
            var added = names.Add(name);
            if (added && names.Count == 1) {
                UniqueTransactions++;
                transactionStartTime = DateTime.Now;
                var evt = StartSpinner;
                if (evt != null) evt();
            }
            OnChanged(added);
        }

        public void End(string name)
        {
            var removed = names.Remove(name);
            if (removed && names.Count == 0) {
                var dt = DateTime.Now - transactionStartTime;
                var seconds = (int)Math.Round (dt.TotalSeconds);
                seconds = seconds < 0 ? 0 : seconds;
                var evt = StopSpinner;
                if (evt != null) evt(seconds);
            }
            OnChanged(removed);
        }

        public void ResetTime()
        {
            transactionStartTime = DateTime.Now;
        }

        public int GetUniqueTransactionsAndReset()
        {
            var val = UniqueTransactions - 1;  // this count includes the current transaction as well. so reduce by 1.
            UniqueTransactions = 0;
            return val;
        }

        private void OnChanged(bool hasChanged)
        {
            if (!hasChanged) return;

            var evt = Changed;
            if (evt != null) evt();
        }
    }
}
