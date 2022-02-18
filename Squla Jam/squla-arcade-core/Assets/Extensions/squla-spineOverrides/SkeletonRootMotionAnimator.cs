using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;
using System.Collections.Generic;

[RequireComponent (typeof(SkeletonMecanim))]
public class SkeletonRootMotionAnimator : MonoBehaviour
{
	SkeletonMecanim skeletonAnimator;
	Animator animator;
	int rootBoneIndex = -1;
	Dictionary<int, AnimationCurve> rootMotionCurves = new Dictionary<int, AnimationCurve> ();
	Dictionary<int, Spine.Animation> animationTable = new Dictionary<int, Spine.Animation> ();
	Dictionary<AnimationClip, int> clipNameHashCodeTable = new Dictionary<AnimationClip, int> ();

	void OnEnable ()
	{
		if (skeletonAnimator == null)
			skeletonAnimator = GetComponent<SkeletonMecanim> ();

		animator = GetComponent<Animator> ();

		skeletonAnimator.UpdateLocal += ApplyRootMotion;
		skeletonAnimator.UpdateWorld += UpdateBones;
	}

	void OnDisable ()
	{
		skeletonAnimator.UpdateLocal -= ApplyRootMotion;
		skeletonAnimator.UpdateWorld -= UpdateBones;
	}

	void Start ()
	{
		rootBoneIndex = skeletonAnimator.skeleton.FindBoneIndex (skeletonAnimator.skeleton.RootBone.Data.Name);
		HandleStart (skeletonAnimator);
	}

	void HandleStart (SkeletonMecanim skeletonAnimator)
	{

		rootMotionCurves.Clear ();
		animationTable.Clear ();
		clipNameHashCodeTable.Clear ();

		var data = skeletonAnimator.skeletonDataAsset.GetSkeletonData (true);

		foreach (var a in data.Animations) {
			animationTable.Add (a.Name.GetHashCode (), a);
		}

		foreach (var keyvaluepair in animationTable) {
			Spine.Animation anim = keyvaluepair.Value;

			AnimationCurve rootMotionCurve = new AnimationCurve ();
			//find the root bone's translate curve
			foreach (Timeline t in anim.Timelines) {
				if (t.GetType () != typeof(TranslateTimeline))
					continue;

				TranslateTimeline tt = (TranslateTimeline)t;
				if (tt.BoneIndex == rootBoneIndex) {
					float time = 0;
					float increment = 1f / 30f;
					int frameCount = Mathf.FloorToInt (anim.Duration / increment);

					for (int i = 0; i <= frameCount; i++) {
						float x = GetXAtTime (tt, time);
						rootMotionCurve.AddKey (time, x);
						time += increment;
					}

					break;
				}
			}
			rootMotionCurves.Add (keyvaluepair.Key, rootMotionCurve);
		}
	}

	float GetXAtTime (TranslateTimeline timeline, float time)
	{
		float[] frames = timeline.Frames;
		if (time < frames [0])
			return frames [1]; // Time is before first frame.

		Bone bone = skeletonAnimator.skeleton.RootBone;

		if (time >= frames [frames.Length - 3]) { // Time is after last frame.
			return (bone.Data.X + frames [frames.Length - 2] - bone.X);
		}

		// Interpolate between the last frame and the current frame.
		int frameIndex = BinarySearch (frames, time, 3);
		float lastFrameX = frames [frameIndex - 2];
		float frameTime = frames [frameIndex];
		float percent = 1 - (time - frameTime) / (frames [frameIndex + -3] - frameTime);
		percent = timeline.GetCurvePercent (frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

		return (bone.Data.X + lastFrameX + (frames [frameIndex + 1] - lastFrameX) * percent - bone.X);
	}

	void ApplyRootMotion (ISkeletonAnimation iskelAnim)
	{
		//I do not use iSkelAnim, no need to as I already have a reference to the animator.

		//TODO: I am lazy, only layer 0 for now.
		int i = 0;
		float layerWeight = 1.0f;
		var stateInfo = animator.GetCurrentAnimatorStateInfo (i);
		var nextStateInfo = animator.GetNextAnimatorStateInfo (i);

		var clipInfo = animator.GetCurrentAnimatorClipInfo (i);
		var nextClipInfo = animator.GetNextAnimatorClipInfo (i);

		SkeletonMecanim.MecanimTranslator.MixMode mode = skeletonAnimator.Translator.layerMixModes [i];

		if (mode == SkeletonMecanim.MecanimTranslator.MixMode.AlwaysMix) {
			//always use Mix instead of Applying the first non-zero weighted clip
			for (int c = 0; c < clipInfo.Length; c++) {
				var info = clipInfo [c];
				float weight = info.weight * layerWeight;
				if (weight == 0)
					continue;
				
				float delta = weight * GetDeltaForClipInfo (info, stateInfo);
				transform.Translate (delta, 0, 0);
			}

			if (nextStateInfo.fullPathHash != 0) {
				for (int c = 0; c < nextClipInfo.Length; c++) {
					var info = nextClipInfo [c];
					float weight = info.weight * layerWeight;
					if (weight == 0)
						continue;
	
					float delta = weight * GetDeltaForClipInfo (info, nextStateInfo);
					transform.Translate (delta, 0, 0);
				}
			}
		} else if (mode >= SkeletonMecanim.MecanimTranslator.MixMode.MixNext) {
			//the root cycle translation will be the translation for the first non zero-weighted clip
			//apply first non-zero weighted clip
			int c = 0;

			for (; c < clipInfo.Length; c++) {
				var info = clipInfo [c];
				float weight = info.weight * layerWeight;
				if (weight == 0)
					continue;

				float delta = GetDeltaForClipInfo (info, stateInfo);
				transform.Translate (delta, 0, 0);
				break;
			}

			//mix the rest
			for (; c < clipInfo.Length; c++) {
				var info = clipInfo [c];
				float weight = info.weight * layerWeight;
				if (weight == 0)
					continue;

				float delta = weight * GetDeltaForClipInfo (info, stateInfo);
				transform.Translate (delta, 0, 0);
			}

			c = 0;

			if (nextStateInfo.fullPathHash != 0) {
				//apply next clip directly instead of mixing (ie:  no crossfade, ignores mecanim transition weights)
				if (mode == SkeletonMecanim.MecanimTranslator.MixMode.Hard) {
					for (; c < nextClipInfo.Length; c++) {
						var info = nextClipInfo [c];
						float weight = info.weight * layerWeight;
						if (weight == 0)
							continue;

						float delta = GetDeltaForClipInfo (info, nextStateInfo);
						transform.Translate (delta, 0, 0);
						break;
					}
				}

				//mix the rest
				for (; c < nextClipInfo.Length; c++) {
					var info = nextClipInfo [c];
					float weight = info.weight * layerWeight;
					if (weight == 0)
						continue;

					float delta = weight * GetDeltaForClipInfo (info, nextStateInfo);
					transform.Translate (delta, 0, 0);
				}
			}
		}
	}


	float GetDeltaForClipInfo (AnimatorClipInfo clipInfo, AnimatorStateInfo stateInfo)
	{
		float delta = 0.0f;
		int clipHash = GetAnimationClipNameHashCode (clipInfo.clip);


		if (!rootMotionCurves.ContainsKey (clipHash) || rootMotionCurves [clipHash] == null)
			return delta;

		//TODO: ask Herman for a simplified example(s), also for testing layer mixing
		float tTime = stateInfo.normalizedTime * stateInfo.length;
		float tLastTime = tTime - Time.deltaTime;
		float tEndTime = clipInfo.clip.length;


		int loopCount = (int)(tTime / tEndTime);
		//if the animation is not looping, return now
		if (loopCount > 0 && !clipInfo.clip.isLooping) {
			return 0.0f;
		}

		int lastLoopCount = (int)(tLastTime / tEndTime);
		//disregard the unwanted
		if (lastLoopCount < 0)
			lastLoopCount = 0;

		float currentTime = tTime - (tEndTime * loopCount);
		float lastTime = tLastTime - (tEndTime * lastLoopCount);

		float a = rootMotionCurves [clipHash].Evaluate (lastTime);
		float b = rootMotionCurves [clipHash].Evaluate (currentTime);

		//detect if loop occurred and offset
		if (loopCount > lastLoopCount) {
			float e = rootMotionCurves [clipHash].Evaluate (tEndTime);
			float s = rootMotionCurves [clipHash].Evaluate (0);

			delta = (e - a) + (b - s);
		} else {
			delta = b - a;
		}

		if (skeletonAnimator.skeleton.FlipX)
			delta *= -1;

		delta *= (gameObject.transform.localScale.x * 0.01f);
		return delta;
	}

	void UpdateBones (ISkeletonAnimation iskelAnim)
	{
		SkeletonMecanim skelAnim = iskelAnim as SkeletonMecanim;
		//reset the root bone's x component to stick to the origin
		skelAnim.skeleton.RootBone.X = 0;
	}

	private int GetAnimationClipNameHashCode (AnimationClip clip)
	{
		int clipNameHashCode;
		if (!clipNameHashCodeTable.TryGetValue (clip, out clipNameHashCode)) {
			clipNameHashCode = clip.name.GetHashCode ();
			clipNameHashCodeTable.Add (clip, clipNameHashCode);
		}

		return clipNameHashCode;
	}
	
	/// Copied directly from Spine.Animation.BinarySearch
	/// <param name="target">After the first and before the last entry.</param>
	/// <returns>Index of first value greater than the target.</returns>
	internal static int BinarySearch (float[] values, float target, int step) {
		int low = 0;
		int high = values.Length / step - 2;
		if (high == 0) return step;
		int current = (int)((uint)high >> 1);
		while (true) {
			if (values[(current + 1) * step] <= target)
				low = current + 1;
			else
				high = current;
			if (low == high) return (low + 1) * step;
			current = (int)((uint)(low + high) >> 1);
		}
	}
}
