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
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        /// <summary>
        /// �������ݱ�
        /// </summary>
        private Dictionary<string, EffectTableData> EffectTableDict = new Dictionary<string, EffectTableData>();
        
        public static ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]>("Json/TableData", "EffectTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.EffectTableDict.Add(data.ID, data);
                    }
                }, null, "����Effect������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        public Effect SpawnEffect(string id)
        {
            if (this.EffectTableDict.TryGetValue(id, out EffectTableData row))
            {
                Effect effect = new Effect(row);
                return effect;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " ��Effect");
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
