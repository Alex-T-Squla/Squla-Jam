using System;
using UnityEngine;

namespace Squla.Core.Audio
{
	[Serializable]
	public class SpriteEvent
	{
		public string eventname;
		public SpriteEventTime time;
		public string filename;
		[NonSerialized] public AudioClip audioClip;
	
		[Serializable]
		public class SpriteEventTime
		{
			public float start;
			public float end;
		}
	}
}