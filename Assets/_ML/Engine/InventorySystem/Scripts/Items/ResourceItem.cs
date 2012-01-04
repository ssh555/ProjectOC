using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public class ResourceItem : Item, CompositeSystem.IComposition
    {
        public ResourceItem(string ID, ItemSpawner.ItemTableJsonData config, int initAmount) : base(ID, config, initAmount)
        {

        }

        string IComposition.ID { get => this.ID; set => throw new System.NotImplementedException(); }
    }
}

