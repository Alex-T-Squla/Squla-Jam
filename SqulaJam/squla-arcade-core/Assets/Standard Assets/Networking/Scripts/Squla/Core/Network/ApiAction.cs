using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJson;

namespace Squla.Core.Network
{
	public class ApiAction
	{
		public string href;
		public Dictionary<string, string> @params = new Dictionary<string, string> ();
	}
}