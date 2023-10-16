using ML.Engine.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Example.InventorySystem
{
    public static class ItemSpawnerExtension
    {

        /// <summary>
        /// ��ʼ�� UIItem
        /// to-do : �����
        /// </summary>
        /// <param name="uIItem"></param>
        /// <param name="item"></param>
        public static void InitUIItem(this ItemSpawner itemSpawner, UIItem uIItem, int ItemIndex)
        {
            uIItem.ItemIndex = ItemIndex;
            uIItem.Init();

            if (uIItem.item != null)
            {
                uIItem.SetImage(itemSpawner.GetItemSprite(uIItem.item.ID));
            }
        }

    }

}
