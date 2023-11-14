using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    [System.Serializable]
    public class ResourceItem : Item, CompositeSystem.IComposition
    {
        public ResourceItem(int ID) : base(ID)
        {
        }

        int IComposition.ID { get => this.ID; set => throw new System.NotImplementedException(); }

        public override void Init(ItemSpawner.ItemTabelData config)
        {
            this.bCanStack = config.bCanStack;

            this.MaxAmount = config.maxAmount;

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

