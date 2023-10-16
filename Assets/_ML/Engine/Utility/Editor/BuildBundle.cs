using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ML.Engine.Utility.Editor
{
    public class BuildBundle : MonoBehaviour
    {
        [MenuItem("BuildAB/Windows/BuildBundleWithTypeTree")]
        public static void BuildBundleWithTypeTreeToWindows()
        {
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/Windows", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
        }

        [MenuItem("BuildAB/Windows/BuildBundleWithoutTypeTree")]
        public static void BuildBundleWithoutTypeTreeToWindows()
        {
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/Windows", BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree, BuildTarget.StandaloneWindows);
        }
    }

}
