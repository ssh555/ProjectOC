using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ML.Editor.Prefab
{
    public static class PrefabUtility
    {
        /// <summary>
        /// 找到所有的预制体上对应类型的组件
        /// </summary>
        private static List<T> GetPrefabInChildrenComponents<T>()
        {
            List<T> ans = new List<T>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab"); // 获取项目中所有预制体的GUID
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    // 检查预制体是否包含TextMeshProUGUI组件
                    ans.AddRange(prefab.GetComponentsInChildren<T>(true));
                }
            }
            return ans;
        }
    }

}
