using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using ProjectOC.Player;
using static ProjectOC.Order.OrderManager;

namespace ML.Engine.Event
{
    [System.Serializable]
    public sealed partial class EventManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {

        private Dictionary<string, string> functionParameters;

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
                else if (parameterType.IsEnum) // ����Ƿ�Ϊö������
                {
                    convertedParameters[i] = Enum.Parse(parameterType, parameter);
                }
                else
                {
                    throw new ArgumentException("Unknown parameter type: " + parameterType);
                }
            }

            return convertedParameters;
        }

        #region TableData
        [System.Serializable]
        public struct EventTableData
        {
            public string ID;
            public string Parameter;
        }

        [System.Serializable]

        public enum CheckType
        {
            CheckBagItem = 0,
            CheckBuild,
            CheckWorkerEMCurrent
        }

        [System.Serializable]
        public struct ConditionTableData
        {
            public string ID;
            public string Name;
            public string Description;
            public List<string> Param1;
            public List<int> Param2;
            public List<float> Param3;
        }
        #endregion

        #region Load
        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<OrderTableData[]> ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<OrderTableData[]>("OCTableData", "Order", (datas) =>
            {



            }, "Event����");
            ABJAProcessor.StartLoadJsonAssetData();

        }
        #endregion
    }
}


