using Spine.Unity;
using UnityEngine;

public class RuntimeSpineLoader : MonoBehaviour
{

	public TextAsset atlasJson;
	public TextAsset skeletonJson;
	public Texture2D atlas;
	public Shader shader;

	[SerializeField]
	private SkeletonGraphic skeletonGraphic;

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.A)) {
			Material m = new Material(shader);
			m.mainTexture = atlas;
			var skeleton = Get(atlasJson.text, new Material[] {m}, skeletonJson.text);
			skeletonGraphic.skeletonDataAsset = skeleton;
			skeletonGraphic.material = m;
			skeletonGraphic.initialSkinName = "Monster001";
			skeletonGraphic.Initialize(true);
		}

	}

	public static SkeletonDataAsset Get(string atlasJson, Material[] material, string skeletonJson)
	{
		SpineAtlasAsset atlas = SpineAtlasAsset.CreateRuntimeInstance(new TextAsset(atlasJson), material, true);
		var skeleton = SkeletonDataAsset.CreateRuntimeInstance(new TextAsset(skeletonJson), atlas, true);
		return skeleton;
	}

}
