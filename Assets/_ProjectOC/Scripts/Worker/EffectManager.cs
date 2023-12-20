using ML.Engine.Manager;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class EffectManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private EffectManager(){}

        private static EffectManager instance;

        public static EffectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EffectManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered = false;
        /// <summary>
        /// 基础数据表
        /// </summary>
        private Dictionary<string, EffectTableJsonData> EffectTableDict = new Dictionary<string, EffectTableJsonData>();

        /// <summary>
        /// 是否是有效的ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.EffectTableDict.ContainsKey(id);
        }
        public Effect SpawnEffect(string id)
        {
            if (EffectTableDict.ContainsKey(id))
            {
                EffectTableJsonData row = this.EffectTableDict[id];
                Effect effect = new Effect();
                effect.Init(row);
                return effect;
            }
            Debug.LogError("没有对应ID为 " + id + " 的效果");
            return null;
        }

        #region to-do : 需读表导入所有所需的 Effect 数据
        public const string TableDataABPath = "Json/TableData";
        public const string TableName = "EffectTableData";

        [System.Serializable]
        public struct EffectTableJsonData
        {
            public string id;
            public string name;
            public EffectType type;
            public string paramStr;
            public int paramInt;
            public float paramFloat;
        }

        public IEnumerator LoadTableData()
        {
            while (GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TableDataABPath, null, out ab);
            yield return crequest;
            if (crequest != null)
                ab = crequest.assetBundle;


            var request = ab.LoadAssetAsync<TextAsset>(TableName);
            yield return request;
            EffectTableJsonData[] datas = JsonConvert.DeserializeObject<EffectTableJsonData[]>((request.asset as TextAsset).text);

            foreach (var data in datas)
            {
                this.EffectTableDict.Add(data.id, data);
            }

            //abmgr.UnLoadLocalABAsync(ItemTableDataABPath, false, null);

            IsLoadOvered = true;
        }
        #endregion
    }
}
