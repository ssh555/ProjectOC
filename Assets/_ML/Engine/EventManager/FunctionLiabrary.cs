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

        private Dictionary<string, MethodInfo> PublicFunctions;
        private Dictionary<string, MethodInfo> PrivateFunctions;

        public void OnRegister()
        {

            LoadTableData();

            // 初始化字典，存储函数名和对应的MethodInfo
            PublicFunctions = new Dictionary<string, MethodInfo>();

            // 初始化字典，存储函数名和对应的MethodInfo
            PrivateFunctions = new Dictionary<string, MethodInfo>();

            // 获取所有的Public函数信息
            MethodInfo[] PublicMethods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);

            // 获取所有的Private函数信息
            MethodInfo[] PrivateMethods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

            // 遍历所有Public函数，存储到字典中
            foreach (MethodInfo method in PublicMethods)
            {
                if (method.DeclaringType == typeof(FunctionLiabrary))
                {
                    PublicFunctions[method.Name] = method;
                }
            }

            // 遍历所有Public函数，存储到字典中
            foreach (MethodInfo method in PrivateMethods)
            {
                if (method.DeclaringType == typeof(FunctionLiabrary))
                {
                    string MethodName = method.Name;
                    // 去掉"GetText"
                    if (MethodName.EndsWith("GetText"))
                    {
                        MethodName = MethodName.Substring(0, MethodName.Length - "GetText".Length);
                    }
                    PrivateFunctions[MethodName] = method;
                }
            }
        }
        public void ExecuteEvent<T>(string executeString,T target)
        {
            string[] stringFunctions = executeString.Split(";");
            foreach (var item in stringFunctions)
            {
                string[] split = item.Split('(');
                string functionName = split[0];
                string parametersString = split[1].Substring(0, split[1].Length - 1); // 去除最后的括号

                if (!PublicFunctions.ContainsKey(functionName))
                {
                    Debug.LogWarning("Function '" + functionName + "' does not exist.");
                    return;
                }

                MethodInfo method = PublicFunctions[functionName];
                ParameterInfo[] parameterInfos = method.GetParameters();
                object[] parameters = ConvertParameters(parametersString, parameterInfos);
                List<object> args = new List<object>(parameters);
                args.Add(target);
                method.Invoke(this, args.ToArray());
            }
        }

        public void ExecuteEvent<T>(string executeString, List<T> target)
        {
            string[] stringFunctions = executeString.Split(";");
            foreach (var item in stringFunctions)
            {
                string[] split = item.Split('(');
                string functionName = split[0];
                string parametersString = split[1].Substring(0, split[1].Length - 1); // 去除最后的括号

                if (!PublicFunctions.ContainsKey(functionName))
                {
                    Debug.LogWarning("Function '" + functionName + "' does not exist.");
                    return;
                }

                MethodInfo method = PublicFunctions[functionName];
                ParameterInfo[] parameterInfos = method.GetParameters();
                object[] parameters = ConvertParameters(parametersString, parameterInfos);
                List<object> args = new List<object>(parameters);
                args.Add(target);
                method.Invoke(this, args.ToArray());
            }
        }
        public void ExecuteEvent(string executeString)
        {
            string[] stringFunctions = executeString.Split(";");
            foreach (var item in stringFunctions)
            {
                string[] split = item.Split('(');
                string functionName = split[0];
                string parametersString = split[1].Substring(0, split[1].Length - 1); // 去除最后的括号

                if (!PublicFunctions.ContainsKey(functionName))
                {
                    Debug.LogWarning("Function '" + functionName + "' does not exist.");
                    return;
                }

                MethodInfo method = PublicFunctions[functionName];
                ParameterInfo[] parameterInfos = method.GetParameters();
                object[] parameters = ConvertParameters(parametersString, parameterInfos);

                method.Invoke(this, parameters);
            }
        }

        public bool ExecuteCondition(string ConditionID)
        {
            if (!ConditionTableDataDic.ContainsKey(ConditionID))
            {
                Debug.LogError("ConditionID '" + ConditionID + "' does not exist.");
                return false;
            }

            ConditionTableData conditionTableData = ConditionTableDataDic[ConditionID];
            if(!PublicFunctions.ContainsKey(conditionTableData.CheckType.ToString()))
            {
                Debug.LogError("Function '" + conditionTableData.CheckType.ToString() + "' does not exist.");
                return false;
            }
            MethodInfo method = PublicFunctions[conditionTableData.CheckType.ToString()];

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
            ConditionTableData conditionTableData = ConditionTableDataDic[ConditionID];
            string ConditionText = conditionTableData.ConditionText.ToString();
            // 找到左中括号和右中括号的位置
            int leftBracketIndex = ConditionText.IndexOf('[');
            int rightBracketIndex = ConditionText.IndexOf(']');

            // 提取中括号中的内容
            string content = ConditionText.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
            string[] splits1 = content.Split('/');
            string target = splits1[1].Trim();
            string[] splits2 = target.Substring(1).Split('|');

            //此时 splits[0] 应为P2 splits[1]应为1
            int tp = splits2[0][1] - '0';
            int index = int.Parse(splits2[1]) - 1;

            string replaceStr = null;
            if(tp == 1)
            {
                replaceStr = conditionTableData.Param1[index];
            }
            else if(tp == 2)
            {
                replaceStr = conditionTableData.Param2[index].ToString();
            }
            else if(tp == 3)
            {
                replaceStr = conditionTableData.Param3[index].ToString();
            }
            ConditionText = ConditionText.Replace(target, replaceStr);

            MethodInfo method = PrivateFunctions[conditionTableData.CheckType.ToString()];

            object[] parameters = new object[4];
            parameters[0] = ConditionText;
            parameters[1] = conditionTableData.Param1;
            parameters[2] = conditionTableData.Param2;
            parameters[3] = conditionTableData.Param3;
            return (string)method.Invoke(this, parameters);
        }


        //将参数字符串转换为实际参数
        private object[] ConvertParameters(string parametersString, ParameterInfo[] parameterInfos)
        {
            if(string.IsNullOrEmpty(parametersString))
            {
                return new object[0];
            }
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
                    //throw new ArgumentException("Unknown parameter type: " + parameterType);
                    Debug.Log("跳过" + parameterType);
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


