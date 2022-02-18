using UnityEngine;
using Spine.Unity;

namespace Squla.Overrides
{
	public class SqulaBoneFollower : BoneFollower
	{
		public Vector3 angleOverride;

		public new void LateUpdate ()
		{
			base.LateUpdate ();

			Transform thisTransform = transform;

			if (thisTransform.parent == SkeletonRenderer.transform) {
				if (followBoneRotation)
					thisTransform.localRotation = thisTransform.localRotation * Quaternion.Euler (angleOverride);
			} else {
				if (followBoneRotation) {
					thisTransform.rotation = thisTransform.rotation * Quaternion.Euler (angleOverride);
				}
			}
		}
	}
}
