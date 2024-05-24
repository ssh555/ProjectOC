using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace ML.Editor
{
    public static class EditorSplitView
    {
        private static Type _splitViewType;

        public static Type SplitViewType
        {
            get
            {
                if (_splitViewType == null)
                    _splitViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SplitView");
                return _splitViewType;
            }
        }

        /// <summary>
        /// ����ʵ��
        /// </summary>
        /// <returns></returns>
        public static object CreateInstance()
        {
            return ScriptableObject.CreateInstance(SplitViewType);
        }
        /// <summary>
        /// �������ͼ
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="view"></param>
        public static void AddChild(object instance, object view)
        {
            MethodInfo mInfo = SplitViewType.GetMethod("AddChild", BindingFlags.Public | BindingFlags.Instance, null,
                new Type[] { typeof(EditorWindow).Assembly.GetType("UnityEditor.View") }, null);
            if (mInfo == null) return;
            mInfo.Invoke(instance, new object[] { view });
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="position"></param>
        public static void SetPosition(object instance, Rect position)
        {
            PropertyInfo pInfo = SplitViewType.GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
            if (pInfo == null) return;
            pInfo.SetValue(instance, position);
        }
        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static Rect GetPosition(object instance)
        {
            PropertyInfo pInfo = SplitViewType.GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
            if (pInfo == null) return default(Rect);
            return (Rect)pInfo.GetValue(instance);
        }
        /// <summary>
        /// �����Ƿ�Ϊ��ֱ����
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="isVertical"></param>
        public static void SetVertical(object instance, bool isVertical)
        {
            FieldInfo fInfo = SplitViewType.GetField("vertical", BindingFlags.Public | BindingFlags.Instance);
            if (fInfo == null) return;
            fInfo.SetValue(instance, isVertical);
        }
    }

}
