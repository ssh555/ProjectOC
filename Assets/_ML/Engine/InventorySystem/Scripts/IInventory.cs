using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public interface IInventory
    {
        public bool AddItem(Item item);

        public bool RemoveItem(Item item);

        public Item RemoveItem(Item item, int amount);

        public bool RemoveItem(string itemID, int amount);

        public int GetItemAllNum(string id);

    }

}
