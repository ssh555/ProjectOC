using ML.Engine.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System.Reflection;
using System;
using ML.Engine.TextContent;
using UnityEngine.UIElements;
using System.IO;
using UnityEngine.InputSystem.Controls;
namespace ML.Engine.Input
{
    public class InputManager : Manager.GlobalManager.IGlobalManager
    {
        private static InputManager instance = null;

        [ShowInInspector]
        private Dictionary<(string actionMapName, string actionName), InputAction> actionDictionary = new Dictionary<(string, string), InputAction>();

        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Manager.GameManager.Instance.GetGlobalManager<InputManager>();
                }
                return instance;
            }
        }

        ~InputManager()
        {
            if (instance == this)
            {
                Common.Disable();
                instance = null;
            }

        }


        /// <summary>
        /// 公共操作
        /// </summary>
        public CommomInput Common = new CommomInput();


        public InputManager()
        {
            this.Common.Enable();
            InitUIInputActionAssets();
        }


        #region InputActionAsset
        public void InitUIInputActionAssets()
        {
            GameManager.Instance.ABResourceManager.LoadAssetsAsync<InputActionAsset>("InputActionAssets", (ia) =>
            {
                lock (actionDictionary)
                {
                    // 遍历所有的 Input Actions
                    foreach (InputActionMap actionMap in ia.actionMaps)
                    {
                        // 遍历当前 actionMap 中的所有 Input Actions
                        foreach (InputAction action in actionMap.actions)
                        {
                            (string, string) key = (actionMap.name, action.name);
                            actionDictionary[key] = action;
                        }
                    }
                }
            });
        }

        public InputAction GetInputAction((string, string) key)
        {
            if(this.actionDictionary.ContainsKey(key))
            {
                return this.actionDictionary[key];
            }
            return null;
        }

        /// <summary>
        /// 获取对应InputAction的BindText index为选择InputAction下的哪一项Binding 默认为0即为全选 下标从1开始
        /// </summary>
        public string GetInputActionBindText(InputAction inputAction,int index = 0)
        {
            HashSet<string> keys = new HashSet<string>();
            string t = "";
            // 遍历当前 InputAction 的所有绑定
            var options = InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames;
            string humanReadableString;
            for (int i = 0; i < inputAction.bindings.Count; i++)
            {
                if(index == 0)
                {
                    humanReadableString = InputControlPath.ToHumanReadableString(inputAction.bindings[i].path, options);
                    if (Config.inputDevice == Config.InputDevice.Keyboard && inputAction.bindings[i].path.StartsWith("<Keyboard>"))
                    {
                        keys.Add(ExtractString(humanReadableString));
                    }
                    else if (Config.inputDevice == Config.InputDevice.XBOX && inputAction.bindings[i].path.StartsWith("<XInputController>"))
                    {
                        keys.Add(ExtractString(humanReadableString));
                    }
                }
                else if(index - 1 == i)
                {
                    humanReadableString = InputControlPath.ToHumanReadableString(inputAction.bindings[i].path, options);
                    if (Config.inputDevice == Config.InputDevice.Keyboard && inputAction.bindings[i].path.StartsWith("<Keyboard>"))
                    {
                        return humanReadableString;
                    }
                    else if (Config.inputDevice == Config.InputDevice.XBOX && inputAction.bindings[i].path.StartsWith("<XInputController>"))
                    {
                        return humanReadableString;
                    }
                }
                
            }
            

            foreach (var item in keys)
            {
                t += item;
            }

            return t;
        }

        private string ExtractString(string input)
        {

            if (input.StartsWith("2DVector")) return null;
            // 查找第一个斜杠的索引
            int firstSlashIndex = input.IndexOf('/');

            // 如果没有斜杠，直接返回原始字符串
            if (firstSlashIndex == -1)
            {
                return input;
            }



            // 提取第一个斜杠之前的字符串
            string extractedString = input.Substring(0, firstSlashIndex);

            return extractedString;
        }

        public KeyTip[] ExportKeyTipValues<T>(T StructT)
        {
            // 获取结构体类型
            Type structType = typeof(T);

            // 获取结构体中的所有字段
            FieldInfo[] fields = structType.GetFields();

            // 创建一个用于存储 KeyTip 的列表
            List<KeyTip> keyTipValues = new List<KeyTip>();

            // 遍历所有字段
            foreach (FieldInfo field in fields)
            {
                // 如果字段类型是 KeyTip 类型，则将其值添加到列表中
                if (field.FieldType == typeof(KeyTip))
                {
                    KeyTip value = (KeyTip)field.GetValue(StructT);
                    keyTipValues.Add(value);
                }
                // 如果字段类型是 KeyTip[] 类型，则逐个添加数组中的元素到列表中
                else if (field.FieldType == typeof(KeyTip[]))
                {
                    KeyTip[] arrayValue = (KeyTip[])field.GetValue(StructT);
                    keyTipValues.AddRange(arrayValue);
                }
            }

            // 将列表转换为数组并返回
            return keyTipValues.ToArray();
        }
        #endregion
    }

}
