using System;
using System.Reflection;
using UnityEngine;

namespace ML.Engine.Utility
{
    public static class DeepCopyUtility
    {
        public static object DeepCopy(object original)
        {
            if (original == null) return null;

            var type = original.GetType();

            // ����� ScriptableObject��ʹ�� ScriptableObject.CreateInstance ������ʵ��
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                return DeepCopyScriptableObject(original as ScriptableObject);
            }

            // �����ֵ���ͻ��ַ�����ֱ�ӷ���
            if (type.IsValueType || type == typeof(string))
            {
                return original;
            }

            // ������ʵ��
            var copy = Activator.CreateInstance(type);

            // ���������ֶ�
            CopyFields(original, copy);

            return copy;
        }

        private static void CopyFields(object source, object destination)
        {
            var type = source.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var value = field.GetValue(source);

                // ����Ƕ�׶�������
                if (value != null && !field.FieldType.IsValueType && field.FieldType != typeof(string))
                {
                    var copyValue = DeepCopy(value);
                    field.SetValue(destination, copyValue);
                }
                else
                {
                    field.SetValue(destination, value);
                }
            }
        }

        private static ScriptableObject DeepCopyScriptableObject(ScriptableObject original)
        {
            var copy = ScriptableObject.CreateInstance(original.GetType());
            CopyFields(original, copy);
            return copy;
        }
    }
}
