using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ProductionNodeNS
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
        public ML.Engine.InventorySystem.ItemCategory Category;
        /// <summary>
        /// ԭ��
        /// </summary>
        public Dictionary<string, int> RawItems = new Dictionary<string, int>();
        /// <summary>
        /// ��Ʒ
        /// </summary>
        public string ProductItem = "";
        /// <summary>
        /// ʱ�����ģ�����1��������Ҫ������
        /// </summary>
        public int TimeCost = 1;
        /// <summary>
        /// �䷽���飬���䷽�������ʱ�ɻ�õľ���ֵ
        /// </summary>
        public int ExpRecipe;
        public void Init(RecipeManager.RecipeTableJsonData config)
        {
            this.ID = config.id;
            this.Category = config.category;
            this.RawItems = new Dictionary<string, int>(config.rawItems);
            this.ProductItem = config.productItem;
            this.TimeCost = config.timeCost;
            this.ExpRecipe = config.expRecipe;
        }
        public void Init(Recipe recipe)
        {
            this.ID = recipe.ID;
            this.Category = recipe.Category;
            this.RawItems = new Dictionary<string, int>(recipe.RawItems);
            this.ProductItem = recipe.ProductItem;
            this.TimeCost = recipe.TimeCost;
            this.ExpRecipe = recipe.ExpRecipe;
        }
    }
}
