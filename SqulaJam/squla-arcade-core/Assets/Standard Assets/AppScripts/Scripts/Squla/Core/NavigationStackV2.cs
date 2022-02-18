using System.Collections.Generic;
using Squla.Core.Logging;
using Squla.Core.IOC;
using Squla.Core.IOC.Builder;
using Squla.Core.ZeroQ;
using Squla.Core.Network;
using Squla.ScreenTransition;

namespace Squla.Core
{
	[Singleton]
	public class NavigationStackV2
	{
		const string DEFAULT_TAB = "home";

		private readonly SmartLogger logger = SmartLogger.GetLogger<NavigationStackV2> ();

		private readonly Dictionary<string, List<NavigationStackItem>> tabs = new Dictionary<string, List<NavigationStackItem>> ();

		/// <summary>
		/// This name says in which tab stack the screens are pushed.
		/// </summary>
		private string currentTabName;

		/// <summary>
		/// This name says which is the actively selected tab UI.
		/// Note: currentTabName and currentTabUIName may not be same at some times.
		/// </summary>
		private string currentTabUIName;

		private List<NavigationStackItem> stack;
		private readonly Stack<OwnerItem> owners = new Stack<OwnerItem> ();

		[Inject]
		private Laizy<ScreenHUD> screenHUDLazy;

		private ScreenHUD screenHUD;

		private readonly Bus bus;

		[Inject]
		public NavigationStackV2 (Bus bus)
		{
			this.bus = bus;
			this.bus.Register (this);

			// Navigation stack has defult tab of home.
			var tab = new List<NavigationStackItem> ();
			tabs.Add (DEFAULT_TAB, tab);

			currentTabName = DEFAULT_TAB;
			currentTabUIName = DEFAULT_TAB;
			stack = tab;
		}

		public int Count => stack.Count;

		public string ScreenName => Count > 0 ? Peek ().GameObjectName : string.Empty;

		public void UnRegisterTab (string name)
		{
			if (name == DEFAULT_TAB)
				return;

			if (!tabs.ContainsKey (name))
				return;

			var tab = tabs [name];
			while (tab.Count > 0) {
				var item = tab [0];
				item.screen.DestroyScreen ();
				tab.RemoveAt (0);
			}
			tabs.Remove (name);
		}

		public bool HasTab (string name)
		{
			return tabs.ContainsKey (name) && tabs [name].Count > 0;
		}

		public void RegisterTab (string name, ScreenBuilder builder)
		{
			logger.Debug("RegisterTab {0}", name);
			List<NavigationStackItem> tab;
			if (tabs.ContainsKey (name)) {
				tab = tabs [name];
			} else {
				tab = new List<NavigationStackItem> ();
				tabs.Add (name, tab);
			}

			tab.Add (new NavigationStackItem { previouslySelectedTab = name, screen = builder });

			if (name == DEFAULT_TAB) {
				currentTabName = name;
				currentTabUIName = name;
				stack = tab;
			}
		}

		public void ChangeTab (string tabName)
		{
			logger.Debug ("ChangeTab {0}, currentUI {1}, currentTab {2}", tabName, currentTabUIName, currentTabName);
			if (currentTabUIName != tabName) {
				// fire command to change the tab UI.
				bus.Publish ("cmd://navigation/tab/change-ui", tabName);
			}

			screenHUD = screenHUDLazy.Get ();

			if (currentTabUIName == tabName || currentTabName == tabName) {
				while (stack.Count > 2) {
					var item = stack [1];
					logger.Debug ("Removing item {0}", item.screen.GameObjectName);
					stack.RemoveAt (1);
				}

				if (stack.Count > 1) {
					// do the wipe out to right transition
					WhenBack ();
				}
			} else {
				Peek ().DeactivateScreen ();
			}

			currentTabName = currentTabUIName = tabName;
			stack = tabs [tabName];
			Peek ().Build ().SetAnimatorWipeInEnd ();
		}

		public void ResetTab (string tabName)
		{
			if (!tabs.ContainsKey (tabName))
				return;

			logger.Debug ("ResetTab {0}", tabName);
			var localStack = tabs [tabName];
			while (localStack.Count > 1) {
				var item = localStack [1];
				item.screen.SetScreenActive (false);
				logger.Debug ("Reseting item {0}", item.screen.GameObjectName);
				localStack.RemoveAt (1);
			}
		}

		public ScreenBuilder Push (ScreenBuilder builder, string tabName)
		{
			var previouslySelectedTab = currentTabUIName;
			if (currentTabUIName != tabName) {
				// fire command to change the tab UI.
				bus.Publish ("cmd://navigation/tab/change-ui", tabName);
				currentTabUIName = tabName;
			}

			var item = new NavigationStackItem {
				previouslySelectedTab = previouslySelectedTab,
				screen = builder
			};

			screenHUD = screenHUDLazy.Get ();
			stack.Add (item);
			screenHUD.BeginTransition ("0202a28ac3f3961cb826469551e22c61");
			logger.Info ("Push: {0}", builder.GameObjectName);
			PrintStack ();
			return builder;
		}

		public ScreenBuilder Push (ScreenBuilder builder)
		{
			stack.Add (new NavigationStackItem { previouslySelectedTab = currentTabUIName, screen = builder });
			screenHUD = screenHUDLazy.Get ();
			screenHUD.BeginTransition ("0202a28ac3f3961cb826469551e22c61");
			logger.Info ("Push: {0}", builder.GameObjectName);
			PrintStack ();
			return builder;
		}

		public ScreenBuilder ShowModal (ScreenBuilder builder)
		{
			stack.Add (new NavigationStackItem { previouslySelectedTab = currentTabUIName, screen = builder });
			screenHUD = screenHUDLazy.Get ();
			logger.Info ("Push: {0}", builder.GameObjectName);
			PrintStack ();
			builder.Build().TransitionInModal();
			return builder;
		}

		public ScreenBuilder HideModal ()
		{
			var screen = Pop ().DeactivateScreen ();
			bus.Publish(ScreenBuilder.cmd_Screen_Transition_Out);
			return screen;
		}

		public NavigationStackV2 Clear ()
		{
			stack.Clear ();
			return this;
		}

		public void DestroyExceptRoot ()
		{
			logger.Debug ("DestroyExceptRoot");
			PrintStack ();
			var top = Peek ();
			while (stack.Count > 1) {
				var item = stack [0];
				if (top != item.screen) {
					// sometimes screen on the top can be in deactivated state
					// somewhere in the stack.  so don't destroy it.
					item.screen.DestroyScreen ();
				}
				stack.RemoveAt (0);
			}

			logger.Info ("DestroyExceptRoot: root {0}", stack [0].screen.GameObjectName);
		}

		public ScreenBuilder Pop (int stackIndex = 0)
		{
			int lastIndex = stack.Count - 1 + stackIndex;
			var item = stack [lastIndex];
			stack.RemoveAt (lastIndex);
			item.screen.NotifyVisibility(false);
			
			lastIndex = stack.Count - 1 + stackIndex;
			stack[lastIndex].screen.NotifyVisibility(true);
			
			logger.Info ("Pop: {0}", item.screen.GameObjectName);
			PrintStack ();
			screenHUD.BeginTransition("988745c0835a673ab968cc82b2b8f246");
			if (item.previouslySelectedTab != currentTabUIName) {
				// fire command to change the selected Tab UI.
				bus.Publish ("cmd://navigation/tab/change-ui", item.previouslySelectedTab);
			}
			currentTabUIName = item.previouslySelectedTab;
			return item.screen;
		}

		public ScreenBuilder Peek (int stackIndex = 0)
		{
			int lastIndex = stack.Count - 1 + stackIndex;
			var item = stack [lastIndex];
			return item.screen;
		}

		public void TakeOwnership (System.Object owner, System.Action onBackCallback)
		{
			logger.Info ("TakeOwnership, count: {0}, owner: '{1}'", owners.Count, owner);

			owners.Push (new OwnerItem {
				owner = owner,
				onBackCallback = onBackCallback
			});
		}

		public void ReleaseOwnership (System.Object owner)
		{
			if (owners.Count == 0) {
				logger.Error ("Owner's stack is empty, releasing {0}", owner);
				return;
			}

			var activeOwner = owners.Peek ();
			if (activeOwner.owner != owner) {
				logger.Error ("ReleaseOwnership: Active owner '{0}' releasing '{1}", activeOwner.owner, owner);
				return;
			}

			logger.Info ("ReleaseOwnership, count: {0}, owner: '{1}'", owners.Count, owner);
			owners.Pop ();
		}

		[Subscribe ("cmd://navigation/back")]
		[Subscribe ("cmd://navigation/close")]
		public void WhenBack ()
		{
			if (Count > 1) {
//				screenHUD.BeginTransition ("c307262423d8776b975857f7b13a6a3a");
				if (owners.Count > 0) {
					var item = owners.Peek ();
					if (item.onBackCallback != null) {
						item.onBackCallback ();
					} else {
						logger.Error ("Back: No ownner to handle the back transition");
					}
				} else {
					logger.Error ("This is worst case scenario.");
					Pop().TransitionOut();
				}
			} else {
				logger.Info ("You are supposed to quit");
			}
		}

		private void PrintStack ()
		{
			logger.Debug ("PrintStack: {0}", currentTabName);
			for (int i = stack.Count - 1, stackIndex = 0; i >= 0; i--, stackIndex--) {
				var item = stack [i];
				logger.Debug ("Stack: {0}, {1}", stackIndex, item.screen.GameObjectName);
			}
		}

		class OwnerItem
		{
			public System.Object owner;
			public System.Action onBackCallback;
		}

		class NavigationStackItem
		{
			public string previouslySelectedTab;
			public ScreenBuilder screen;
		}
	}
}
