using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ItemNS
{
    /// <summary>
    /// �䷽
    /// </summary>
    [System.Serializable]
    public class Recipe
    {
        /// <summary>
        /// ID��Recipe_����_���
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ��Ŀ
        /// </summary>
        public ItemCategory Category;
        /// <summary>
        /// ԭ��
        /// </summary>
        public Dictionary<string, int> RawItems = new Dictionary<string, int>();
        /// <summary>
        /// ��Ʒ
        /// </summary>
        public string ProductItems = "";
        /// <summary>
        /// ʱ�����ģ�����1��������Ҫ������
        /// </summary>
        public int TimeCost = 1;
        /// <summary>
        /// �䷽���飬���䷽�������ʱ�ɻ�õľ���ֵ
        /// </summary>
        public int ExpRecipe;
        public Recipe(string id)
        {
            // TODO: ����������
        }
    }
}
