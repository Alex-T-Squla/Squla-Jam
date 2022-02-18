using UnityEngine;
using System.Collections;

namespace Squla.Core
{
	public interface ITimelineManager
	{

		void Delay (int frame);

		void Append (System.Action callback, int frame);

		void Add (System.Action callback, int frame);

		void Add (System.Action[] callback, int staggerDelay, int frame);

		void Add (System.Action callback, string increase = "0");

		void Clear();
	}
}