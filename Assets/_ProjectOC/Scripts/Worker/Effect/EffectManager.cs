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
        public void OnRegister()
        {
            LoadTableData();
        }

        #region Load And Data
        /// <summary>
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        /// <summary>
        /// �������ݱ�
        /// </summary>
        private Dictionary<string, EffectTableData> EffectTableDict = new Dictionary<string, EffectTableData>();
        
        public ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]>("OCTableData", "Effect", (datas) =>
            {
                foreach (var data in datas)
                {
                    EffectTableDict.Add(data.ID, data);
                }
            }, "����Effect������");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion

        #region Spawn
        public Effect SpawnEffect(string id, string value)
        {
            if (IsValidID(id) && !string.IsNullOrEmpty(value))
            {
                return new Effect(EffectTableDict[id], value);
            }
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
