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
        /// �ҵ����е�Ԥ�����϶�Ӧ���͵����
        /// </summary>
        private static List<T> GetPrefabInChildrenComponents<T>()
        {
            List<T> ans = new List<T>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab"); // ��ȡ��Ŀ������Ԥ�����GUID
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    // ���Ԥ�����Ƿ����TextMeshProUGUI���
                    ans.AddRange(prefab.GetComponentsInChildren<T>(true));
                }
            }
            return ans;
        }
    }

}
