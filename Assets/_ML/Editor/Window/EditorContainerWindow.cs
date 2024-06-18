using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace ML.Editor
{
    public static class EditorContainerWindow
    {
        private static Type _containerWindowType;
        /// <summary>
        /// ����
        /// </summary>
        public static Type ContainerWindowType
        {
            get
            {
                if (_containerWindowType == null)
                    _containerWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ContainerWindow");
                return _containerWindowType;
            }
        }
        /// <summary>
        /// ����ʵ��
        /// </summary>
        public static object CreateInstance()
        {
            return ScriptableObject.CreateInstance(ContainerWindowType);
        }

        /// <summary>
        /// ����RootView
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        public static void SetRootView(object instance, object value)
        {
            FieldInfo finfo = ContainerWindowType.GetField("m_RootView", BindingFlags.Instance | BindingFlags.NonPublic);
            if (finfo != null)
                finfo.SetValue(instance, value);
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="position"></param>
        public static void SetPosition(object instance, Rect position)
        {
            PropertyInfo pInfo = ContainerWindowType.GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
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
            PropertyInfo pInfo = ContainerWindowType.GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
            if (pInfo == null) return default(Rect);
            return (Rect)pInfo.GetValue(instance);
        }
        /// <summary>
        /// ��ʾ
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="showMode"></param>
        /// <param name="loadPosition"></param>
        /// <param name="displayImmediately"></param>
        /// <param name="setFocus"></param>
        public static void Show(object instance, int showMode, bool loadPosition, bool displayImmediately, bool setFocus)
        {
            MethodInfo mInfo = ContainerWindowType.GetMethod("Show", BindingFlags.Public | BindingFlags.Instance, null,
                new Type[]
                {
                typeof(EditorWindow).Assembly.GetType("UnityEditor.ShowMode"), typeof(bool), typeof(bool), typeof(bool)
                }, null);
            if (mInfo == null) return;
            mInfo.Invoke(instance, new object[] { showMode, loadPosition, displayImmediately, setFocus });
        }

        /// <summary>
        /// ���óߴ�
        /// </summary>
        /// <param name="instance"></param>
        public static void OnResize(object instance)
        {
            MethodInfo mInfo = ContainerWindowType.GetMethod("OnResize", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mInfo == null) return;
            mInfo.Invoke(instance, null);
        }

        public static void Close(object instance)
        {
            MethodInfo mInfo = ContainerWindowType.GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mInfo == null) return;
            mInfo.Invoke(instance, null);
        }
    }

}

