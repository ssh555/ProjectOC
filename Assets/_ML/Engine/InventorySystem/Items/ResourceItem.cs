using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public class ResourceItem : Item
    {
        public ResourceItem(string ID, ItemTableData config, int initAmount) : base(ID, config, initAmount)
        {

        }
    }
}