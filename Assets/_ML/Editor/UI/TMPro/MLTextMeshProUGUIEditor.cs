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
        /// �Ƿ񱾵ػ� -> ������Ҫһ���ʲ��ļ�ͳһ���й�����������д洢���ܳ־ô���
        /// ���ʲ��ļ������ҵ��������Ϊtrue�������ҵ�����Ϊfalse
        /// </summary>
        private SerializedProperty IsLocalize;
        private SerializedProperty LocalizeID;


        protected override void OnEnable()
        {
            // ��ȡ��ǰѡ���TextMeshProUGUI���
            textMeshPro = (MLTextMeshProUGUI)target;

            IsLocalize = serializedObject.FindProperty("IsLocalize");
            LocalizeID = serializedObject.FindProperty("localizeID");

            CheckLocalizeGUID();
        }

        public override void OnInspectorGUI()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(textMeshPro.gameObject) || (PrefabStageUtility.GetCurrentPrefabStage() != null && textMeshPro.transform.IsChildOf(PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.transform)))
            {
                // ���һ��Toggle���ڿ���TextMeshPro�Ŀɼ���
                EditorGUI.BeginChangeCheck();
                // ���һ��Toggle���ڿ���TextMeshPro�Ŀɼ���
                EditorGUILayout.PropertyField(IsLocalize, new GUIContent("�Ƿ񱾵ػ�"));


                if (EditorGUI.EndChangeCheck())
                {
                    CheckLocalizationTable();
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.LabelField("���ػ�GUID", LocalizeID.stringValue);

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }

        /// <summary>
        /// TODO : ������Ҫ�������е�Ԥ�����ϵĴ������LocalizeID
        /// ��Ԥ�����ϵĶ�ӦMLTextMeshProUGUIһ��Ψһ��LocalizeID
        /// ������ID�Ѿ����ڣ�����ID��Ӧ��������������������һ���µ�ID
        /// ����ǿɱ��ػ����Ҳ��ڱ��У������
        /// </summary>
        private void CheckLocalizeGUID()
        {
            if (string.IsNullOrEmpty(LocalizeID.stringValue))
            {
                LocalizeID.stringValue = System.Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// �ɱ��ػ�:�����
        /// ���ɱ��ػ�:�ӱ����Ƴ�
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
//        // �������е��κ����巢���ı�ʱ����
//        // ����ص�Ҳ������ӡ��Ƴ����������ʱ����

//        // ���ÿ��GameObject���������
//        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
//        {
//            Component[] components = obj.GetComponents<Component>();

//            foreach (Component component in components)
//            {
//                if (component == null)
//                {
//                    // ������Ƴ���
//                    Debug.Log("Component removed from: " + obj.name);
//                    // ������ִ������Ҫ�Ĳ���

//                    // ����Ƿ��Ǵ洢ֵ��������Ƴ�
//                    ToggleTextMeshPro toggleComponent = obj.GetComponent<ToggleTextMeshPro>();
//                    if (toggleComponent != null)
//                    {
//                        // ����ǣ�����洢��ֵ
//                        PlayerPrefs.DeleteKey("TextMeshProToggleState");
//                        PlayerPrefs.Save();
//                    }
//                }
//            }
//        }
//    }
//}
