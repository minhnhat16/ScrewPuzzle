using UnityEditor;

public class BuildBundles
{
    [MenuItem("Tools/Build AssetBundles")]
    static void Build()
    {
        BuildPipeline.BuildAssetBundles(
            "Assets/Resources_moved",  // đường dẫn folder
            BuildAssetBundleOptions.None,
            BuildTarget.Android
        );
    }
}