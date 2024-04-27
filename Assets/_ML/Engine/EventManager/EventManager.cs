using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using ProjectOC.Player;

namespace ML.Engine.Event
{
    public sealed partial class EventManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        [ShowInInspector]
        private Dictionary<string, string> functionParameters;
        [ShowInInspector]
        private Dictionary<string, MethodInfo> functions;

        public void OnRegister()
        {
            // ģ��ӱ����������
            functionParameters = new Dictionary<string, string>();
            functionParameters["InteractUpgrade"] = "string,int";
            functionParameters["Attack"] = "bool";
            functionParameters["UseItem"] = "int";

            // ��ʼ���ֵ䣬�洢�������Ͷ�Ӧ��MethodInfo
            functions = new Dictionary<string, MethodInfo>();

            // ��ȡ���еĺ�����Ϣ
            MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);

            // �������к������洢���ֵ���
            foreach (MethodInfo method in methods)
            {
                if (method.DeclaringType == typeof(EventManager))
                {
                    functions[method.Name] = method;
                }
            }
        }

        public void ExecuteFunction(string executeString)
        {
            string[] split = executeString.Split('(');
            string functionName = split[0];
            string parametersString = split[1].Substring(0, split[1].Length - 1); // ȥ����������

            if (!functions.ContainsKey(functionName))
            {
                Debug.LogError("Function '" + functionName + "' does not exist.");
                return;
            }

            MethodInfo method = functions[functionName];
            ParameterInfo[] parameterInfos = method.GetParameters();
            object[] parameters = ConvertParameters(parametersString, parameterInfos);

            method.Invoke(this, parameters);
        }

        

        // �����������������ַ���ת��Ϊʵ�ʲ���
        private object[] ConvertParameters(string parametersString, ParameterInfo[] parameterInfos)
        {
            string[] parameters = parametersString.Split(',');
            object[] convertedParameters = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                string parameter = parameters[i].Trim();
                Type parameterType = parameterInfos[i].ParameterType;

                if (parameterType == typeof(string))
                {
                    convertedParameters[i] = parameter;
                }
                else if (parameterType == typeof(int))
                {
                    convertedParameters[i] = int.Parse(parameter);
                }
                else if (parameterType == typeof(bool))
                {
                    convertedParameters[i] = bool.Parse(parameter);
                }
                else
                {
                    throw new ArgumentException("Unknown parameter type: " + parameterType);
                }
            }

            return convertedParameters;
        }
    }
}


