using ML.Engine.Manager;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.WorkerNS.FeatureManager;

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
        /// �������ݱ�
        /// </summary>
        private Dictionary<string, EffectTableJsonData> EffectTableDict = new Dictionary<string, EffectTableJsonData>();

        /// <summary>
        /// �Ƿ�����Ч��ID
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
            Debug.LogError("û�ж�ӦIDΪ " + id + " ��Ч��");
            return null;
        }

        #region to-do : ���������������� Effect ����

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
        public static ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableJsonData[]>("Json/TableData", "EffectTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.EffectTableDict.Add(data.id, data);
                    }
                }, null, "����Effect������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
       
        #endregion
    }
}
