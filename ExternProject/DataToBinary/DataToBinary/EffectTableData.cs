using ExcelToJson;
using System;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct EffectTableData : IGenData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
        public EffectType Type;
        public bool GenData(string[] row)
        {
            if (string.IsNullOrEmpty(row[0]))
            {
                return false;
            }
            // 0 -> ID
            this.ID = row[0];
            // 1 -> Name
            this.Name = new ML.Engine.TextContent.TextContent();
            this.Name.Chinese = row[1];
            this.Name.English = row[1];
            // 2 -> Type
            this.Type = (EffectType)Enum.Parse(typeof(EffectType), row[2]);
            return true;
        }
    }
    [System.Serializable]
    public enum EffectType
    {
        None,
        #region Set Int
        AlterAPMax,
        AlterExpRate_Cook,
        AlterExpRate_HandCraft,
        AlterExpRate_Industry,
        AlterExpRate_Magic,
        AlterExpRate_Transport,
        AlterExpRate_Collect,
        #endregion
        #region Offset Int
        AlterBURMax,
        AlterEff_Cook,
        AlterEff_HandCraft,
        AlterEff_Industry,
        AlterEff_Magic,
        AlterEff_Transport,
        AlterEff_Collect,
        #endregion
        #region Offset Float
        AlterWalkSpeed,
        #endregion
    }
}
