using System.Collections.Generic;
using UnityEngine;

namespace Squla.Core.TDD
{
	public class LogAssertion
	{
		private readonly ITestManager tm;

		private readonly List<string> messages = new List<string>();

		public LogAssertion(ITestManager tm)
		{
			this.tm = tm;
			Application.logMessageReceived += OnLogMessageReceived;
		}

		public void Expect(string msg)
		{
			messages.Add(msg);
		}

		public void NotExpect(string msg)
		{
			messages.Add(msg);
		}

		public void AssertNotArrived(string msg)
		{
			tm.Assert(messages.IndexOf(msg) != -1, "Log messages not received");
		}

		public void AssertAllLogMessagesArrived()
		{
			tm.Assert(messages.Count == 0, "All log messages received");
		}

		private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
		{
			for (int i = messages.Count - 1; i >= 0; i--) {
				if (condition.Contains(messages[i])) {
					messages.RemoveAt(i);
				}
			}
		}
	}
}