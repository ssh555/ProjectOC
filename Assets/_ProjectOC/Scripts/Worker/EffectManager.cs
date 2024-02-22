using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct EffectTableData
    {
        public string ID;
        public TextContent Name;
        public EffectType Type;
    }

    [System.Serializable]
    public sealed class EffectManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        /// <summary>
        /// 基础数据表
        /// </summary>
        private Dictionary<string, EffectTableData> EffectTableDict = new Dictionary<string, EffectTableData>();
        
        public static ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]>("Json/TableData", "Effect", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        EffectTableDict.Add(data.ID, data);
                    }
                }, null, "隐兽Effect表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        public Effect SpawnEffect(string id, string value)
        {
            if (IsValidID(id) && !string.IsNullOrEmpty(value))
            {
                return new Effect(EffectTableDict[id], value);
            }
            Debug.LogError($"ID:{id} Value:{value} 无法创建Effect");
            return null;
        }
        #endregion

        #region Getter
        public string[] GetAllID()
        {
            return EffectTableDict.Keys.ToArray();
        }
        public bool IsValidID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return EffectTableDict.ContainsKey(id);
            }
            return false;
        }
        public string GetName(string id)
        {
            if (IsValidID(id))
            {
                return EffectTableDict[id].Name;
            }
            return "";
        }
        public EffectType GetEffectType(string id)
        {
            if (IsValidID(id))
            {
                return EffectTableDict[id].Type;
            }
            return EffectType.None;
        }
        #endregion
    }
}
