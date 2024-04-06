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
        /// ��������
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
                    // �������е� Input Actions
                    foreach (InputActionMap actionMap in ia.actionMaps)
                    {
                        // ������ǰ actionMap �е����� Input Actions
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
        /// ��ȡ��ӦInputAction��BindText indexΪѡ��InputAction�µ���һ��Binding Ĭ��Ϊ0��Ϊȫѡ �±��1��ʼ
        /// </summary>
        public string GetInputActionBindText(InputAction inputAction,int index = 0)
        {
            HashSet<string> keys = new HashSet<string>();
            string t = "";
            // ������ǰ InputAction �����а�
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
            // ���ҵ�һ��б�ܵ�����
            int firstSlashIndex = input.IndexOf('/');

            // ���û��б�ܣ�ֱ�ӷ���ԭʼ�ַ���
            if (firstSlashIndex == -1)
            {
                return input;
            }



            // ��ȡ��һ��б��֮ǰ���ַ���
            string extractedString = input.Substring(0, firstSlashIndex);

            return extractedString;
        }

        public KeyTip[] ExportKeyTipValues<T>(T StructT)
        {
            // ��ȡ�ṹ������
            Type structType = typeof(T);

            // ��ȡ�ṹ���е������ֶ�
            FieldInfo[] fields = structType.GetFields();

            // ����һ�����ڴ洢 KeyTip ���б�
            List<KeyTip> keyTipValues = new List<KeyTip>();

            // ���������ֶ�
            foreach (FieldInfo field in fields)
            {
                // ����ֶ������� KeyTip ���ͣ�����ֵ��ӵ��б���
                if (field.FieldType == typeof(KeyTip))
                {
                    KeyTip value = (KeyTip)field.GetValue(StructT);
                    keyTipValues.Add(value);
                }
                // ����ֶ������� KeyTip[] ���ͣ��������������е�Ԫ�ص��б���
                else if (field.FieldType == typeof(KeyTip[]))
                {
                    KeyTip[] arrayValue = (KeyTip[])field.GetValue(StructT);
                    keyTipValues.AddRange(arrayValue);
                }
            }

            // ���б�ת��Ϊ���鲢����
            return keyTipValues.ToArray();
        }
        #endregion
    }

}
