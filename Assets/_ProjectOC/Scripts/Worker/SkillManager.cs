using ML.Engine.Manager;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProjectOC.WorkerNS.WorkerManager;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class SkillManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private SkillManager(){}

        private static SkillManager instance;

        public static SkillManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SkillManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// ���� Skill ���ݱ�
        /// </summary>
        private Dictionary<string, SkillTableJsonData> SkillTableDict = new Dictionary<string, SkillTableJsonData>();

        /// <summary>
        /// �Ƿ�����Ч��ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.SkillTableDict.ContainsKey(id);
        }
        public Skill SpawnSkill(string id)
        {
            if (SkillTableDict.ContainsKey(id))
            {
                SkillTableJsonData row = this.SkillTableDict[id];
                Skill skill = new Skill();
                skill.Init(row);
                return skill;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " �ļ���");
            return null;
        }
        public Texture2D GetTexture2D(string id)
        {
            if (!this.SkillTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.SkillTableDict[id].texture2d);
        }
        public Sprite GetSprite(string id)
        {
            var tex = this.GetTexture2D(id);
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        #region to-do : ���������������� Skill ����
        public const string Texture2DPath = "ui/Skill/texture2d";

        [System.Serializable]
        public struct SkillTableJsonData
        {
            public string id;
            public string name;
            public int sort;
            public WorkType type;
            public string desciption;
            public string effectsDescription;
            public List<string> effectIDs;
            public string texture2d;
        }

        public static ML.Engine.ABResources.ABJsonAssetProcessor<SkillTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<SkillTableJsonData[]>("Json/TableData", "SkillTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.SkillTableDict.Add(data.id, data);
                    }
                }, null, "����Skill������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion
    }
}
