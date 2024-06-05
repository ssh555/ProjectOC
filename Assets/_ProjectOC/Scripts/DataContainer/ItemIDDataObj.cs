using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.DataNS
{
    public struct ItemIDDataObj : IDataObj
    {
        [LabelText("ŒÔ∆∑ID"), ReadOnly, ShowInInspector]
        private string ID;
        public ItemIDDataObj(string id) { ID = id; }
        public string GetDataID() { return ID ?? ""; }
        public int GetDataWeight()
        {
            return ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.ItemManager.GetWeight(ID) : 0;
        }
        public bool DataEquales(IDataObj other)
        {
            return other != null && other is ItemIDDataObj otherObj && ID == otherObj.ID;
        }
        public void AddToPlayerInventory(int num)
        {
            ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(ID, num);
        }
        public int RemoveFromPlayerInventory(int num, bool containStore = false)
        {
            return ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(ID, num, containStore);
        }
        public void ConvertToWorldObj(int num, Transform transform)
        {
            List<ML.Engine.InventorySystem.Item> items = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(ID, num);
            foreach (var item in items)
            {
#pragma warning disable CS4014
                ML.Engine.InventorySystem.ItemManager.Instance.SpawnWorldItem(item, transform.position, transform.rotation);
#pragma warning restore CS4014
            }
        }
        public List<ML.Engine.InventorySystem.Item> ConvertToItem(int num) 
        { 
            return ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(ID, num); 
        }
        public int DataCompareTo(IDataObj other)
        {
            if (other == null) { return -1; }
            return GetDataID().CompareTo(other.GetDataID());
        }
    }
}