using System.Collections.Generic;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public class CreatureItem : Item, ProjectOC.DataNS.IDataObj
    {
        public string CreatureID => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetCreatureID(ID) : "";
        public string ProRecipeID => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetProRecipeID(ID) : "";
        public string BreRecipeID => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetProRecipeID(ID) : "";
        public List<Formula> Discard => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetDiscard(ID) : new List<Formula>();
        public Gender Gender;
        public int Activity;
        public int Output;
        public CreatureItem(string ID, ItemTableData config, int initAmount) : base(ID, config, initAmount)
        {
            bool hasSex = ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetHasSex(config.id);
            Gender = (Gender)(hasSex ? UnityEngine.Random.Range(1, 3) : 0);
            Activity = ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetActivity(config.id);
        }
        public string GetDataID() { return ID; }
    }
}