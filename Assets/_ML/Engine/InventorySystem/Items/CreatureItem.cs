using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public class CreatureItem : Item, ProjectOC.DataNS.IDataObj
    {
        public string CreatureID => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetCreatureID(ID) : "";
        public string ProRecipeID => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetProRecipeID(ID) : "";
        public string BreRecipeID => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetProRecipeID(ID) : "";
        public Formula Discard => ProjectOC.ManagerNS.LocalGameManager.Instance != null ? ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetDiscard(ID) : new Formula() { id = ""};
        public Gender Gender;
        public int Activity;
        public int Output;
        public CreatureItem(string ID, ItemTableData config, int initAmount) : base(ID, config, initAmount)
        {
            bool hasSex = ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetHasSex(config.id);
            Gender = (Gender)(hasSex ? UnityEngine.Random.Range(1, 3) : 0);
            Activity = ProjectOC.ManagerNS.LocalGameManager.Instance.CreatureManager.GetActivity(config.id);
        }
        public override WorldItem.IWorldItemData GetItemWorldData()
        {
            return new WorldItem.WorldCreatureItemData(Amount, Gender, Activity, Output);
        }

        #region IDataObj
        public string GetDataID() { return ID ?? ""; }
        public int GetDataWeight() { return Weight; }
        public bool DataEquales(ProjectOC.DataNS.IDataObj other)
        {
            return (other != null && other is CreatureItem otherObj) ? ReferenceEquals(this, otherObj) : false;
        }
        public void AddToPlayerInventory(int num)
        {
            ProjectOC.ManagerNS.LocalGameManager.Instance.Player.GetInventory().AddItem(this);
        }
        public int RemoveFromPlayerInventory(int num, bool containStore = false)
        {
            return ProjectOC.ManagerNS.LocalGameManager.Instance.Player.GetInventory().RemoveItem(this) ? 1 : 0;
        }
        public void ConvertToWorldObj(int num, Transform transform)
        {
#pragma warning disable CS4014
            ItemManager.Instance.SpawnWorldItem(this, transform.position, transform.rotation);
#pragma warning restore CS4014
        }
        public List<Item> ConvertToItem(int num) { return new List<Item>() { this }; }
        public int CompareTo(ProjectOC.DataNS.IDataObj other)
        {
            if (this == null || other == null)
            {
                return (this == null).CompareTo((other == null));
            }
            if (other is CreatureItem item)
            {
                return item.Output.CompareTo(Output);
            }
            else
            {
                return GetDataID().CompareTo(other.GetDataID());
            }
        }
        #endregion
    }
}