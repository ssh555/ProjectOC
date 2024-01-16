using ML.Engine.Manager;
using ML.Engine.TextContent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class SkillManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Load And Data
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        /// <summary>
        /// Skill 数据表
        /// </summary>
        private Dictionary<string, SkillTableJsonData> SkillTableDict = new Dictionary<string, SkillTableJsonData>();

        public const string Texture2DPath = "ui/WorkerAbility/texture2d";
        
        [System.Serializable]
        public struct SkillTableJsonData
        {
            public string ID;
            public int Sort;
            public string Icon;
            public WorkType AbilityType;
            public List<string> Effects;
            public TextContent ItemDescription;
            public TextContent EffectsDescription;
        }

        public static ML.Engine.ABResources.ABJsonAssetProcessor<SkillTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<SkillTableJsonData[]>("Json/TableData", "WorkerAbilityTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.SkillTableDict.Add(data.ID, data);
                    }
                }, null, "隐兽Skill表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region Spawn
        public Skill SpawnSkill(string id)
        {
            if (this.SkillTableDict.TryGetValue(id, out SkillTableJsonData row))
            {
                Skill skill = new Skill(row);
                return skill;
            }
            Debug.LogError("没有对应ID为 " + id + " 的Skill");
            return null;
        }
        #endregion

        #region Getter
        public string[] GetAllSkillID()
        {
            return SkillTableDict.Keys.ToArray();
        }

        public bool IsValidSkillID(string id)
        {
            return SkillTableDict.ContainsKey(id);
        }

        public int GetSort(string id)
        {
            if (!SkillTableDict.ContainsKey(id))
            {
                return int.MaxValue;
            }
            return SkillTableDict[id].Sort;
        }

        public Texture2D GetTexture2D(string id)
        {
            if (!SkillTableDict.ContainsKey(id))
            {
                return null;
            }
            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(SkillTableDict[id].Icon);
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

        public WorkType GetSkillType(string id)
        {
            if (!SkillTableDict.ContainsKey(id))
            {
                return WorkType.None;
            }
            return SkillTableDict[id].AbilityType;
        }

        public string GetItemDescription(string id)
        {
            if (!SkillTableDict.ContainsKey(id))
            {
                return "";
            }
            return SkillTableDict[id].ItemDescription;
        }

        public string GetEffectsDescription(string id)
        {
            if (!SkillTableDict.ContainsKey(id))
            {
                return "";
            }
            return SkillTableDict[id].EffectsDescription;
        }
        #endregion
    }
}
