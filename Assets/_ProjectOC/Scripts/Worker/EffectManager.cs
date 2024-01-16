using ML.Engine.Manager;
using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ProjectOC.WorkerNS
{
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
        private Dictionary<string, EffectTableJsonData> EffectTableDict = new Dictionary<string, EffectTableJsonData>();

        [System.Serializable]
        public struct EffectTableJsonData
        {
            public string ID;
            public TextContent Name;
            public EffectType Type;
            public string Param1;
            public int Param2;
            public float Param3;
        }
        public static ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableJsonData[]>("Json/TableData", "EffectTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.EffectTableDict.Add(data.ID, data);
                    }
                }, null, "隐兽Effect表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        public Effect SpawnEffect(string id)
        {
            if (this.EffectTableDict.TryGetValue(id, out EffectTableJsonData row))
            {
                Effect effect = new Effect(row);
                return effect;
            }
            Debug.LogError("没有对应ID为 " + id + " 的Effect");
            return null;
        }
        #endregion

        #region Getter
        public string[] GetAllEffectID()
        {
            return EffectTableDict.Keys.ToArray();
        }
        public bool IsValidID(string id)
        {
            return EffectTableDict.ContainsKey(id);
        }
        public string GetName(string id)
        {
            if (!this.EffectTableDict.ContainsKey(id))
            {
                return "";
            }
            return this.EffectTableDict[id].Name;
        }
        public EffectType GetEffectType(string id)
        {
            if (!this.EffectTableDict.ContainsKey(id))
            {
                return EffectType.None;
            }
            return this.EffectTableDict[id].Type;
        }
        #endregion
    }
}
