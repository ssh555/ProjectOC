using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public struct CreatureTableData
    {
        public string ID;
        public string ItemID;
        public string ProRecipeID;
        public string BreRecipeID;
        public Formula Discard;
        public int Activity;
        public bool Sextype;
    }
    [LabelText("生物管理器"), System.Serializable]
    public sealed class CreatureManager : Manager.LocalManager.ILocalManager
    {
        #region ILocalManager
        [ShowInInspector]
        private Dictionary<string, CreatureTableData> TableDict = new Dictionary<string, CreatureTableData>();
        public ABResources.ABJsonAssetProcessor<CreatureTableData[]> ABJAProcessor;
        public void OnRegister()
        {
            ABJAProcessor = new ABResources.ABJsonAssetProcessor<CreatureTableData[]>("OCTableData", "Creature", (datas) => 
            { 
                foreach (var data in datas) 
                { 
                    TableDict.Add(data.ItemID, data); 
                } 
            }, "养殖表数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
        public bool IsValidID(string id) { return !string.IsNullOrEmpty(id) ? TableDict.ContainsKey(id) : false; }
        public string GetCreatureID(string id) { return IsValidID(id) ? TableDict[id].ID : ""; }
        public string GetProRecipeID(string id) { return IsValidID(id) ? TableDict[id].ProRecipeID : ""; }
        public string GetBreRecipeID(string id) { return IsValidID(id) ? TableDict[id].BreRecipeID : ""; }
        public Formula GetDiscard(string id) { return IsValidID(id) ? TableDict[id].Discard : new Formula() { id = ""}; }
        public int GetActivity(string id) { return IsValidID(id) ? TableDict[id].Activity : 0; }
        public bool GetHasSex(string id) { return IsValidID(id) ? TableDict[id].Sextype : false; }
    }
}