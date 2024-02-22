using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct SkillTableData
    {
        public string ID;
        public int Sort;
        public string Icon;
        public WorkType AbilityType;
        public List<Tuple<string, string>> Effects;
        public TextContent ItemDescription;
        public TextContent EffectsDescription;
    }

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
        private Dictionary<string, SkillTableData> SkillTableDict = new Dictionary<string, SkillTableData>();

        public static ML.Engine.ABResources.ABJsonAssetProcessor<SkillTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<SkillTableData[]>("Binary/TableData", "Skill", (datas) =>
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
            if (IsValidID(id))
            {
                return new Skill(SkillTableDict[id]);
            }
            //Debug.LogError("没有对应ID为 " + id + " 的Skill");
            return null;
        }
        #endregion

        #region Getter
        public string[] GetAllID()
        {
            return SkillTableDict.Keys.ToArray();
        }

        public bool IsValidID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return SkillTableDict.ContainsKey(id);
            }
            return false;
        }

        public int GetSort(string id)
        {
            if (IsValidID(id))
            {
                return SkillTableDict[id].Sort;
            }
            return int.MaxValue;
        }

        public WorkType GetSkillType(string id)
        {
            if (IsValidID(id))
            {
                return SkillTableDict[id].AbilityType;
            }
            return WorkType.None;
        }

        public string GetItemDescription(string id)
        {
            if (IsValidID(id))
            {
                return SkillTableDict[id].ItemDescription;
            }
            return "";
        }

        public string GetEffectsDescription(string id)
        {
            if (IsValidID(id))
            {
                return SkillTableDict[id].EffectsDescription;
            }
            return "";
        }
        #endregion
    }
}
