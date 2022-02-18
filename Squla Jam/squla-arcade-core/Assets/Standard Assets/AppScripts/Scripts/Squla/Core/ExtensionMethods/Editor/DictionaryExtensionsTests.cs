using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NUnit.Framework;
using Squla.Core.ExtensionMethods;

namespace Squla.TDD
{
	public class DictionaryExtensionsTests
	{
		Dictionary<string, int> valTypeKeys = new Dictionary<string, int> () {
			{ "Tom", 1 },
			{ "Dick", 2 },
			{ "Harry", 3 }

		};

		Dictionary<string, string> refTypeKeys = new Dictionary<string, string> () {
			{ "Tom", "Plumber" },
			{ "Jane", "Engineer" },
			{ "Dick", "Farmer" }
		};

		[Test]
		public void GetValueOrDefaultTest ()
		{
			var valTom = valTypeKeys.GetValueOrDefault ("Tom", -1);
			var valJane = valTypeKeys.GetValueOrDefault ("Jane", -1);

			Assert.AreEqual (valTom, 1);
			Assert.AreEqual (valJane, -1);
		}

		[Test]
		public void CopyDictionaryRefTypes ()
		{
			var copy = refTypeKeys.CloneDictionaryCloningValues ();
			copy ["Tom"] = "Scientist";

			Assert.AreEqual (refTypeKeys ["Tom"], "Plumber");
			Assert.AreEqual (copy ["Tom"], "Scientist");
		}
	}
}
