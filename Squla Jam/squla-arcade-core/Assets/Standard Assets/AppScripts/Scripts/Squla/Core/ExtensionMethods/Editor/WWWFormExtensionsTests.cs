using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Squla.Core.ExtensionMethods;

namespace Squla.TDD
{
	public class WWWFormExtensionsTests
	{
		
		Dictionary<string, string> formData = new Dictionary<string, string> () {
			{ "AccessToken", "abcdefghijlkmnop" },
			{ "RefreshToken", "123456789" },
		};

		[Test]
		public void EditorTest ()
		{
			WWWForm form = new WWWForm ();
			form.AddFields (formData);
		}
	}
}