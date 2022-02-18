using UnityEngine;
using System;
using System.Collections;
using Spine;
using Spine.Unity;

namespace Squla.Overrides
{
	public class SqulaBoneRotationFollower : MonoBehaviour
	{
		public enum TargetAxis
		{
			X,
			Y,
			Z}

		;

		#region Inspector

		public SkeletonRenderer skeletonRenderer;

		public SkeletonRenderer SkeletonRenderer {
			get { return skeletonRenderer; }
			set {
				skeletonRenderer = value;
				Initialize ();
			}
		}

		/// <summary>If a bone isn't set in code, boneName is used to find the bone.</summary>
		[SpineBone (dataField: "skeletonRenderer")]
		public String boneName;

		public TargetAxis targetAxis;

		public int targetAngleOffset;

		[UnityEngine.Serialization.FormerlySerializedAs ("resetOnAwake")]
		public bool initializeOnAwake = true;

		#endregion

		[NonSerialized] public bool valid;
		[NonSerialized] public Bone bone;
		protected Transform skeletonTransform;

		public void Awake ()
		{
			if (initializeOnAwake)
				Initialize ();
		}

		public void HandleRebuildRenderer (SkeletonRenderer skeletonRenderer)
		{
			Initialize ();
		}

		public void Initialize ()
		{
			bone = null;
			valid = skeletonRenderer != null && skeletonRenderer.valid;
			if (!valid)
				return;

			skeletonTransform = skeletonRenderer.transform;
			skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
			skeletonRenderer.OnRebuild += HandleRebuildRenderer;

			if (!string.IsNullOrEmpty (boneName))
				bone = skeletonRenderer.skeleton.FindBone (boneName);

			#if UNITY_EDITOR
			if (Application.isEditor)
				LateUpdate ();
			#endif
		}

		void OnDestroy ()
		{
			if (skeletonRenderer != null)
				skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
		}

		public virtual void LateUpdate ()
		{
			if (!valid) {
				Initialize ();
				return;
			}

			if (bone == null) {
				if (string.IsNullOrEmpty (boneName))
					return;
				bone = skeletonRenderer.skeleton.FindBone (boneName);
				if (bone == null) {
					Debug.LogError ("Bone not found: " + boneName, this);
					return;
				}
			}

			Transform thisTransform = this.transform;
			if (thisTransform.parent == skeletonTransform) {
				thisTransform.localRotation = Quaternion.Euler (0f, 0f, bone.WorldRotationX);
			} else {
				Vector3 worldRotation = skeletonTransform.rotation.eulerAngles;
				Vector3 thisRotation = thisTransform.rotation.eulerAngles;
				if (targetAxis == TargetAxis.X) {
					thisTransform.rotation = Quaternion.Euler (bone.WorldRotationX + targetAngleOffset, thisRotation.y, thisRotation.z);
				} else if (targetAxis == TargetAxis.Y) {
					thisTransform.rotation = Quaternion.Euler (thisRotation.x, bone.WorldRotationX + targetAngleOffset, thisRotation.z);
				} else {
					thisTransform.rotation = Quaternion.Euler (thisRotation.x, thisRotation.y, bone.WorldRotationX + targetAngleOffset);
				}
			}
		}
	}
}
