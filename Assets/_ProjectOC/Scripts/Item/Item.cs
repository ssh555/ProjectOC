using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ItemNS
{
    /// <summary>
    /// ��Ʒ
    /// </summary>
    [System.Serializable]
    public class Item
    {
        /// <summary>
        /// ��ƷID��Item_����_���
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ��Ŀ
        /// </summary>
        public ItemCategory Category;
        /// <summary>
        /// ������Ӱ�����
        /// </summary>
        public int Weight;
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public string ItemDescription = "";
        /// <summary>
        /// Ч������
        /// </summary>
        public string EffectsDescription = "";
        public Item(string id)
        {
            // TODO: ����
        }
    }
}
