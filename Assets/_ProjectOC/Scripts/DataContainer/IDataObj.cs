using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.DataNS
{
    public interface IDataObj
    {
        public string GetDataID();
        public int GetDataWeight();
        public bool DataEquales(IDataObj other);
        public void AddToPlayerInventory(int num);
        public int RemoveFromPlayerInventory(int num, bool containStore = false);
        public void ConvertToWorldObj(int num, Transform transform);
        public List<ML.Engine.InventorySystem.Item> ConvertToItem(int num);
        public int DataCompareTo(IDataObj other);
    }
}