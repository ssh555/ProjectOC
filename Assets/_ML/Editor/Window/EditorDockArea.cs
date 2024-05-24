using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using Object = System.Object;

namespace ML.Editor
{
    public static class EditorDockArea
    {
        private static Type _dockAreaType;
        /// <summary>
        /// ����
        /// </summary>
        public static Type DockAreaType
        {
            get
            {
                if (_dockAreaType == null)
                    _dockAreaType = typeof(EditorWindow).Assembly.GetType("UnityEditor.DockArea");
                return _dockAreaType;
            }
        }

        /// <summary>
        /// ����ʵ��
        /// </summary>
        /// <returns></returns>
        public static object CreateInstance()
        {
            return ScriptableObject.CreateInstance(DockAreaType);
        }

        /// <summary>
        /// ���Tab
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="window"></param>
        /// <param name="sendPaneEvents"></param>
        public static void AddTab(object instance, EditorWindow window, bool sendPaneEvents = true)
        {
            MethodInfo mInfo = DockAreaType.GetMethod("AddTab", BindingFlags.Instance | BindingFlags.Public, null,
                 new Type[] { typeof(EditorWindow), typeof(bool) }, null);
            if (mInfo == null) return;
            try
            {
                mInfo.Invoke(instance, new object[] { window, sendPaneEvents });
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Debug.LogError(e.InnerException);
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="position"></param>
        public static void SetPosition(object instance, Rect position)
        {
            PropertyInfo pInfo = DockAreaType.GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
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
            PropertyInfo pInfo = DockAreaType.GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
            if (pInfo == null) return default(Rect);
            return (Rect)pInfo.GetValue(instance);
        }
    }



}
