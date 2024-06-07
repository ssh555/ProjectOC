using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct EffectTableData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public EffectType Type;
        public string Param1;
        public int Param2;
        public float Param3;
        public bool Param4;
    }

    [System.Serializable]
    public sealed class EffectManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region ILocalManager
        private Dictionary<string, EffectTableData> EffectTableDict = new Dictionary<string, EffectTableData>();
        public ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]> ABJAProcessor;
        public void OnRegister()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<EffectTableData[]>("OCTableData", "Effect", (datas) =>
            {
                foreach (var data in datas)
                {
                    EffectTableDict.Add(data.ID, data);
                }
            }, "ÒþÊÞEffect±íÊý¾Ý");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion

        #region Spawn
        public Effect SpawnEffect(string id) { return IsValidID(id) ? new Effect(EffectTableDict[id]) : default(Effect); }
        #endregion
        #region Getter
        private const string str = "";
        public bool IsValidID(string id) { return !string.IsNullOrEmpty(id) && EffectTableDict.ContainsKey(id); }
        public string GetName(string id) { return IsValidID(id) ? EffectTableDict[id].Name : str; }
        public EffectType GetEffectType(string id) { return IsValidID(id) ? EffectTableDict[id].Type : EffectType.None; }
        #endregion
    }
}