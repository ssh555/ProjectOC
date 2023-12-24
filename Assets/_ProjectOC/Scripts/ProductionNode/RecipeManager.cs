using System.Collections.Generic;
using UnityEngine;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using Newtonsoft.Json;
using System.Collections;

namespace ProjectOC.ProductionNodeNS
{
    public sealed class RecipeManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private RecipeManager()
        {
            RecipeCategorys.Add(ItemCategory.Food, new List<string>());
            RecipeCategorys.Add(ItemCategory.Crop, new List<string>());
            RecipeCategorys.Add(ItemCategory.Cloth, new List<string>());
            RecipeCategorys.Add(ItemCategory.Metal, new List<string>());
            RecipeCategorys.Add(ItemCategory.Mineral, new List<string>());
        }

        private static RecipeManager instance;

        public static RecipeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RecipeManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion
        /// <summary>
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered = false;
        public Dictionary<ItemCategory, List<string>> RecipeCategorys = new Dictionary<ItemCategory, List<string>>();
        /// <summary>
        /// ����Recipe���ݱ�
        /// </summary>
        private Dictionary<string, RecipeTableJsonData> RecipeTableDict = new Dictionary<string, RecipeTableJsonData>();

        /// <summary>
        /// �Ƿ�����Ч���䷽ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.RecipeTableDict.ContainsKey(id);
        }
        public List<string> GetRecipeIDsByCategory(ItemCategory category)
        {
            List<string> result = new List<string>();
            if (RecipeCategorys.ContainsKey(category) && RecipeCategorys[category] != null)
            {
                result.AddRange(RecipeCategorys[category]);
            }
            return result;
        }

        /// <summary>
        /// ����id�����µ��䷽
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Recipe SpawnRecipe(string id)
        {
            if (RecipeTableDict.ContainsKey(id))
            {
                RecipeTableJsonData row = this.RecipeTableDict[id];
                Recipe recipe = new Recipe();
                recipe.Init(row);
                return recipe;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " ���䷽");
            return null;
        }

        #region to-do : ���������������� Recipe ����
        public const string TableDataABPath = "Json/TableData";
        public const string TableName = "RecipeTableData";

        [System.Serializable]
        public struct RecipeTableJsonData
        {
            public string id;
            public ItemCategory category;
            public Dictionary<string, int> rawItems;
            public string productItem;
            public int timeCost;
            public int expRecipe;
        }

        public IEnumerator LoadTableData()
        {
            while (GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TableDataABPath, null, out ab);
            yield return crequest;
            if (crequest != null)
                ab = crequest.assetBundle;


            var request = ab.LoadAssetAsync<TextAsset>(TableName);
            yield return request;
            RecipeTableJsonData[] datas = JsonConvert.DeserializeObject<RecipeTableJsonData[]>((request.asset as TextAsset).text);

            foreach (var data in datas)
            {
                this.RecipeTableDict.Add(data.id, data);
                if (!this.RecipeCategorys.ContainsKey(data.category))
                {
                    this.RecipeCategorys[data.category] = new List<string>();
                }
                this.RecipeCategorys[data.category].Add(data.id);
            }

            //abmgr.UnLoadLocalABAsync(ItemTableDataABPath, false, null);

            IsLoadOvered = true;
        }
        #endregion
    }
}
