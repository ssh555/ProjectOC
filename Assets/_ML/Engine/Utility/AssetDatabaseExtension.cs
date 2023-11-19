using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace ML.Editor
{
    public static class AssetDatabaseExtension
    {
        public static bool IsAssetPathValid(string assetPath)
        {
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            return !string.IsNullOrEmpty(guid);
        }
    }
}

#endif