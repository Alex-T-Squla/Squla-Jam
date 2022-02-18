using UnityEngine;
using System;
using System.Collections;

namespace Squla.Core.IOC
{
	internal class RectTransformProvider : Provider
	{
		private System.Object target;

		public RectTransformProvider (ObjectGraph graph, Type targetType, System.Object target) : base (graph, true)
		{
			this.target = target;
		}

		protected override System.Object CreateInstance ()
		{
			return target;
		}

	}
}