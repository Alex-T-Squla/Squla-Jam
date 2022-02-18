using System.Collections.Generic;
using System.Text;
using Squla.Core.IOC;
using Squla.Core.Logging;
using Squla.Core.ZeroQ;
using UnityEngine;

namespace Squla.Core.Network
{
	[Singleton]
	public class ScreenHUD
	{
		private IScreenHUD_UI hudUX;

		private readonly HashSet<string> runningTransactions = new HashSet<string>();

		private readonly List<System.Action> endTransactionCallbacks = new List<System.Action> ();

		private readonly List<System.Action> abortTransactionCallbacks = new List<System.Action> ();

		private readonly SmartLogger logger = SmartLogger.GetLogger<ScreenHUD> ();

		private readonly Bus bus;
		private readonly ScreenHUD_RequestManager requestManager;
		private readonly SpinnerModel spinnerModel;

		public bool isInTransition {
			get;
			private set;
		}

		[Inject]
		public ScreenHUD (Bus bus, IScreenHUD_UI hud, ScreenHUD_RequestManager requestManager, SpinnerModel spinnerModel)
		{
			this.bus = bus;
			this.requestManager = requestManager;
			this.spinnerModel = spinnerModel;
			requestManager.Aborted += OnAbort;
			requestManager.BeforeRetry += OnBeforeRetry;

			hudUX = hud;
			hudUX.Done += OnUXDone;

			bus.Register (this);
		}

		public bool HasRunningTransaction(string key)
		{
			return runningTransactions.Contains(key);
		}

		public void SetIsFirstEnabled (bool isFirstEnabled)
		{
			hudUX.SetIsFirstEnabled (isFirstEnabled);
		}

		public void SetIsSecondEnabled (bool isSecondEnabled)
		{
			hudUX.SetIsSecondEnabled (isSecondEnabled);
		}

		public void Execute (JsonRequest request)
		{
			requestManager.Execute (request);
		}

		public string GetRunningTransactionList()
		{
			StringBuilder sb = new StringBuilder();
			foreach (string transaction in runningTransactions) {
				sb.Append(transaction);
				sb.Append(" ");
			}
			return sb.ToString();
		}
		
		public void TerminateAll ()
		{
			bus.Publish("cmd://screen-hud/terminate-all");
			logger.Debug ("Terminating all ({0}) transactions", runningTransactions.Count);
			requestManager.TerminateAll();
			// remove by name
			runningTransactions.Clear ();
			// remove by name

			RunCallbacks (abortTransactionCallbacks);
			endTransactionCallbacks.Clear ();
		}

		/// <summary>
		/// Rule: This can be called multiple times within the transition.
		/// </summary>
		public void BeginTransition (string key = "")
		{
			logger.Debug ("BeginTransition with key {0}", key);

			if (!isInTransition) {
				hudUX.StartTransition ();
			}
			isInTransition = true;
		}

		/// <summary>
		/// Rule: This should be called only one time for any transition.
		/// </summary>
		public void EndTransition (string key = "")
		{
			logger.Debug ("EndTransition with key {0}", key);
			if (!isInTransition) {
				logger.Warning ("EndTransition: You are breaking the rule. Fix it.");
			}
			isInTransition = false;
			hudUX.EndTransition ();
		}

		/// <summary>
		/// You are calling a method where care must be taken care.
		/// Warning: For the given transaction <paramref name="key"/> is there a possibility of calling this method
		/// again before completion of the transaction?  Do not do that.  It won't work.
		/// Note: All begin transaction to be called with key.  This was an optional parameter, With this commit,
		/// it is changed to mandatory parameter
		/// </summary>
		/// <param name="key"></param>
		/// <param name="forgive></param>
		/// <returns></returns>
		public ScreenHUD BeginTransaction (string key, bool forgive=false)
		{
			if (string.IsNullOrEmpty (key)) {
				logger.Error ("BeginTransaction key is empty. It is a non-empty unique key. txn:// like");
				return this;
			}

			if (!key.StartsWith ("txn://")) {
				logger.Error ("transaction naming convention {0}", key);
			}

			if (forgive && runningTransactions.Contains(key)) {
				return this;
			}

			if (runningTransactions.Contains(key)) {
				Debug.LogError(string.Format("Transaction '{0}' is already running. Nesting is not possible.", key));
				return this;
			}

			spinnerModel.Begin(key);
			runningTransactions.Add (key);
			logger.Debug ("BeginTransaction: {0} with key {1}, no-spinners-count: {2}", runningTransactions.Count, key, spinnerModel.UniqueTransactions);

			return this;
		}

		public ScreenHUD OnAbort (System.Action abortTransaction)
		{
			abortTransactionCallbacks.Add (abortTransaction);
			return this;
		}

		public void EndTransaction (System.Action endTransaction, params string[] key)
		{
			if (endTransaction != null)
				endTransactionCallbacks.Add (endTransaction);

			OnTransactionComplete (key);
		}

		public void EndTransaction (params string[] keys)
		{
			EndTransaction (null, keys);
		}

		public void EndTransactionIfContains (string key)
		{
			if (runningTransactions.Contains(key))
				EndTransaction (key);
		}

		public void ShowError (string message = null, string first_command = null, string second_command = null)
		{
			hudUX.ShowError (message, first_command, second_command);
		}

		private void OnAbort ()
		{
			runningTransactions.Clear ();
			spinnerModel.Clear();

			RunCallbacks (abortTransactionCallbacks);
			endTransactionCallbacks.Clear ();
			abortTransactionCallbacks.Clear ();
		}

		private void OnBeforeRetry ()
		{
		}

		private void OnUXDone ()
		{
			if (runningTransactions.Count != 0)
				logger.Warning ("ScreenHUD.OnDone transaction count: {0}", runningTransactions.Count);

			runningTransactions.Clear ();
			abortTransactionCallbacks.Clear();
			logger.Debug ("OnDone called");
			RunCallbacks (endTransactionCallbacks);
		}

		/// <summary>
		/// This method can be called only with one ore more keys from this commit onwards.
		/// </summary>
		/// <param name="keys"></param>
		private void OnTransactionComplete (params string[] keys)
		{
			if (keys == null || keys.Length == 0) {
				logger.Error ("OnTransactionComplete with empty keys not supported.");
				return;
			}

			// At least one of the key shouldn't be completed.
			var found = false;
			for (var i = 0; i < keys.Length; i++) {
				if (runningTransactions.Contains(keys [i])) {
					found = true;
					runningTransactions.Remove (keys [i]);
					spinnerModel.End(keys[i]);
				} else {
					// TODO: needs to be checked with other live battle transactions,
					// can this be migrated to Error.
					logger.Info ("OnTransactionComplete was already executed for '{0}'", keys [i]);
				}
			}

			if (!found) {
				var detail = (keys.Length > 0) ? keys[0] : string.Empty;				
				logger.Error($"OnTransactionComplete was already called for the given keys. not doing anything, Transaction id: {detail}");
				return;
			}

			var count = runningTransactions.Count;
			if (logger.IsDebugLevel) {
				logger.Debug ("OnTransactionComplete: Transaction {0} with key(s): {1}", count, string.Join (",", keys));
			}

			if (count > 0) {
				if (logger.IsDebugLevel) {
					logger.Debug ("OpenTransactions: {0}", spinnerModel.TransactionNames);
				}
			} else {
				logger.Debug ("EndTransaction keys: {0}", count);
			}
		}

		// check ok
		private static void RunCallbacks (List<System.Action> actions)
		{
			var tempActions = actions.ToArray ();
			actions.Clear ();

			for (var i = tempActions.Length - 1; i >= 0; i--) {
				// call backwards. at the end is the most recently added one.
				tempActions [i] ();
			}
		}
	}
}
