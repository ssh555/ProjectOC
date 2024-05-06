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
    public sealed partial class FunctionLiabrary : ML.Engine.Manager.GlobalManager.IGlobalManager
    {

        private Dictionary<string, string> EventTableDataDic = new Dictionary<string, string>();
        private Dictionary<string, ConditionTableData> ConditionTableDataDic = new Dictionary<string, ConditionTableData>();

        private Dictionary<string, MethodInfo> functions;

        public void OnRegister()
        {

            LoadTableData();

            // 初始化字典，存储函数名和对应的MethodInfo
            functions = new Dictionary<string, MethodInfo>();

            // 获取所有的函数信息
            MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);

            // 遍历所有函数，存储到字典中
            foreach (MethodInfo method in methods)
            {
                if (method.DeclaringType == typeof(FunctionLiabrary))
                {
                    functions[method.Name] = method;
                }
            }
        }

        public void ExecuteEvent(string executeString)
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

        public bool ExecuteCondition(string ConditionID)
        {
            if (!ConditionTableDataDic.ContainsKey(ConditionID))
            {
                Debug.LogError("ConditionID '" + ConditionID + "' does not exist.");
                return false;
            }

            ConditionTableData conditionTableData = ConditionTableDataDic[ConditionID];
            if(!functions.ContainsKey(conditionTableData.CheckType.ToString()))
            {
                Debug.LogError("Function '" + conditionTableData.CheckType.ToString() + "' does not exist.");
                return false;
            }
            MethodInfo method = functions[conditionTableData.CheckType.ToString()];

            object[] parameters = new object[3];
            parameters[0] = conditionTableData.Param1;
            parameters[1] = conditionTableData.Param2;
            parameters[2] = conditionTableData.Param3;

            return (bool)method.Invoke(this, parameters);
        }

        public string GetConditionText(string ConditionID)
        {
            if (!ConditionTableDataDic.ContainsKey(ConditionID))
            {
                Debug.LogError("ConditionID '" + ConditionID + "' does not exist.");
                return null;
            }
        }


        //将参数字符串转换为实际参数
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
                else if (parameterType.IsEnum) // 检查是否为枚举类型
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
            public TextContent.TextContent Name;
            public CheckType CheckType;
            public List<string> Param1;
            public List<int> Param2;
            public List<float> Param3;
            public TextContent.TextContent ConditionText;
        }
        #endregion

        #region Load
        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<EventTableData[]> EventTableDataABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<EventTableData[]>("OCTableData", "Event", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.EventTableDataDic.Add(data.ID, data.Parameter);
                }
            }, "Event数据");
            EventTableDataABJAProcessor.StartLoadJsonAssetData();

            ML.Engine.ABResources.ABJsonAssetProcessor<ConditionTableData[]> ConditionTableDataABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ConditionTableData[]>("OCTableData", "Condition", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.ConditionTableDataDic.Add(data.ID, data);
                }
            }, "Condition数据");
            ConditionTableDataABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}


