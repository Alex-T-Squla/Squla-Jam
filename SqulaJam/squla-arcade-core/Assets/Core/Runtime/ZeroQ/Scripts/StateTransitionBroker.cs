using UnityEngine;
using System;
using Squla.Core.IOC;

namespace Squla.Core.ZeroQ
{
	public class StateTransitionBroker : StateMachineBehaviour
	{
		public string enterCommand;
		public string exitCommand;

		private Bus bus;
		private bool brokerEnsured;
		private ZeroQBroker broker;

		private void Publish (string command)
		{
			if (string.IsNullOrEmpty (command))
				return;

			if (bus == null) {
				var graph = ObjectGraph.main;
				if (graph != null) {
					bus = graph.Get<Bus> ();
				}
			}

			if (broker != null) {
				broker.Publish (command);
			} else if (bus != null) {
				bus.Publish (command);
			}
		}

		private void EnsureBroker (Animator animator)
		{
			if (!brokerEnsured) {
				broker = animator.gameObject.GetComponent<ZeroQBroker> ();
				brokerEnsured = true;
			}
		}

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		override public void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			EnsureBroker (animator);
			Publish (enterCommand);
		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		override public void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			EnsureBroker (animator);
			Publish (exitCommand);
		}
	}
}
