using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Utility
{
    /// <summary>
    /// CSV工具
    /// </summary>
    public static class CSVUtils
    {
        /// <summary>
        /// 解析CSV
        /// </summary>
        public static List<List<string>> ParseCSV(string path, int beginParseRow, bool isLog = true, bool IsEditor = false)
        {
            TextAsset ta = null; ;
            if (IsEditor)
            {
                ta = Resources.Load<TextAsset>(path);
            }
            else
            {
                ta = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAsset<TextAsset>(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileName(path), false);
            }
            if (ta == null)
            {
                if(isLog)
                    Debug.LogError("CSV文件不存在：" + path);
                return null;
            }
            List<List<string>> dataList = new List<List<string>>();

            string[] rowCollection = ta.text.Split('\n');
            for (int row = beginParseRow; row < rowCollection.Length; ++row)
            {
                if (string.IsNullOrEmpty(rowCollection[row]))
                    continue;
                rowCollection[row] = rowCollection[row].Replace("\r", "");
                string[] colCollection = rowCollection[row].Split(',');

                List<string> tempList = new List<string>();
                for (int col = 0; col < colCollection.Length; col++)
                {
                    tempList.Add(colCollection[col]);
                }
                dataList.Add(tempList);
            }
            return dataList;
        }

        /// <summary>
        /// 得到数据数组 -> to-do : 需修改
        /// </summary>
        public static List<T> GetDataArray<T>(string dataStr, char separator)
        {
            List<T> dataList = new List<T>();
            string[] dataArray = dataStr.Split(separator);
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (string.IsNullOrEmpty(dataArray[i])) continue;
                dataArray[i] = dataArray[i].Replace("\r", "");
                try
                {
                    T data = (T)Convert.ChangeType(dataArray[i], typeof(T));
                    dataList.Add(data);
                }
                catch
                {
                    Debug.LogError($"string类型转{typeof(T)}失败：{dataArray[i]}");
                }
            }
            return dataList;
        }
    }
}
