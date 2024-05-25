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

            // 如果是 ScriptableObject，使用 ScriptableObject.CreateInstance 创建新实例
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                return DeepCopyScriptableObject(original as ScriptableObject);
            }

            // 如果是值类型或字符串，直接返回
            if (type.IsValueType || type == typeof(string))
            {
                return original;
            }

            // 创建新实例
            var copy = Activator.CreateInstance(type);

            // 复制所有字段
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

                // 处理嵌套对象的深拷贝
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
