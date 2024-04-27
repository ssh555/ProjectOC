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
            // 模拟从表格读入的数据
            functionParameters = new Dictionary<string, string>();
            functionParameters["InteractUpgrade"] = "string,int";
            functionParameters["Attack"] = "bool";
            functionParameters["UseItem"] = "int";

            // 初始化字典，存储函数名和对应的MethodInfo
            functions = new Dictionary<string, MethodInfo>();

            // 获取所有的函数信息
            MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);

            // 遍历所有函数，存储到字典中
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
            string parametersString = split[1].Substring(0, split[1].Length - 1); // 去除最后的括号

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

        

        // 辅助方法：将参数字符串转换为实际参数
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


