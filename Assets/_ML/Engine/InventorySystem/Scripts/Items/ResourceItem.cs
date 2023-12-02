using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public class ResourceItem : Item, CompositeSystem.IComposition
    {
        public ResourceItem(string ID) : base(ID)
        {
        }

        string IComposition.ID { get => this.ID; set => throw new System.NotImplementedException(); }

        public override void Init(ItemSpawner.ItemTabelJsonData config)
        {
            this.bCanStack = config.bcanstack;

            this.MaxAmount = config.maxamount;

            this.Amount = 1;
        }

        public override void Init(Item item)
        {
            this.bCanStack = item.bCanStack;
            this.MaxAmount = item.MaxAmount;
            this.Amount = item.Amount;
        }



    }
}

