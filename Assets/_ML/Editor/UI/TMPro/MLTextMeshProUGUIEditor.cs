using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ML.Engine.UI.Text;

namespace ML.Editor.UI.Text
{
    [CustomEditor(typeof(MLTextMeshProUGUI))]
    public class TextMeshProUGUIEditor : Sirenix.OdinInspector.Editor.OdinEditor
    {
        private MLTextMeshProUGUI textMeshPro;

        /// <summary>
        /// 是否本地化 -> 后续需要一个资产文件统一进行管理，在这里进行存储不能持久存在
        /// 在资产文件中能找到这个，则为true，不能找到，则为false
        /// </summary>
        private SerializedProperty IsLocalize;
        private SerializedProperty LocalizeID;


        protected override void OnEnable()
        {
            // 获取当前选择的TextMeshProUGUI组件
            textMeshPro = (MLTextMeshProUGUI)target;

            IsLocalize = serializedObject.FindProperty("IsLocalize");
            LocalizeID = serializedObject.FindProperty("localizeID");

            CheckLocalizeGUID();
        }

        public override void OnInspectorGUI()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(textMeshPro.gameObject) || (PrefabStageUtility.GetCurrentPrefabStage() != null && textMeshPro.transform.IsChildOf(PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.transform)))
            {
                // 添加一个Toggle用于控制TextMeshPro的可见性
                EditorGUI.BeginChangeCheck();
                // 添加一个Toggle用于控制TextMeshPro的可见性
                EditorGUILayout.PropertyField(IsLocalize, new GUIContent("是否本地化"));


                if (EditorGUI.EndChangeCheck())
                {
                    CheckLocalizationTable();
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.LabelField("本地化GUID", LocalizeID.stringValue);

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }

        /// <summary>
        /// TODO : 后续需要跟踪所有的预制体上的此组件的LocalizeID
        /// 给预制体上的对应MLTextMeshProUGUI一个唯一的LocalizeID
        /// 如果这个ID已经存在，而且ID对应的组件不是这个组件，则给一个新的ID
        /// 如果是可本地化，且不在表中，则加入
        /// </summary>
        private void CheckLocalizeGUID()
        {
            if (string.IsNullOrEmpty(LocalizeID.stringValue))
            {
                LocalizeID.stringValue = System.Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// 可本地化:加入表
        /// 不可本地化:从表中移除
        /// </summary>
        private void CheckLocalizationTable()
        {

        }
    }
}

//using UnityEditor;
//using UnityEngine;

//[InitializeOnLoad]
//public static class ComponentRemovalListener
//{
//    static ComponentRemovalListener()
//    {
//        EditorApplication.hierarchyChanged += OnHierarchyChanged;
//    }

//    private static void OnHierarchyChanged()
//    {
//        // 当场景中的任何物体发生改变时触发
//        // 这个回调也会在添加、移除、复制组件时触发

//        // 检查每个GameObject的所有组件
//        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
//        {
//            Component[] components = obj.GetComponents<Component>();

//            foreach (Component component in components)
//            {
//                if (component == null)
//                {
//                    // 组件被移除了
//                    Debug.Log("Component removed from: " + obj.name);
//                    // 在这里执行你想要的操作

//                    // 检查是否是存储值的组件被移除
//                    ToggleTextMeshPro toggleComponent = obj.GetComponent<ToggleTextMeshPro>();
//                    if (toggleComponent != null)
//                    {
//                        // 如果是，清除存储的值
//                        PlayerPrefs.DeleteKey("TextMeshProToggleState");
//                        PlayerPrefs.Save();
//                    }
//                }
//            }
//        }
//    }
//}
