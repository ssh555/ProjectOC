using ML.Engine.Manager.LocalManager;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ItemNS
{
    [System.Serializable]
    public sealed class RecipeManager : ILocalManager
    {
        public List<string> RecipeIDs = new List<string>();
        public Dictionary<ItemCategory, List<string>> RecipeCategorys = new Dictionary<ItemCategory, List<string>>();

        public RecipeManager()
        {
            // TODO: ¶Á±íÄÃÊý¾Ý
            RecipeCategorys.Add(ItemCategory.Food, new List<string>());
            RecipeCategorys.Add(ItemCategory.Crop, new List<string>());
            RecipeCategorys.Add(ItemCategory.Cloth, new List<string>());
            RecipeCategorys.Add(ItemCategory.Metal, new List<string>());
            RecipeCategorys.Add(ItemCategory.Mineral, new List<string>());
        }

        public bool IsValidID(string id)
        {
            return RecipeIDs.Contains(id);
        }

        public List<string> GetRecipeIDsByCategory(ItemCategory category)
        {
            if (RecipeCategorys.ContainsKey(category) &&
                RecipeCategorys[category] != null)
            {
                return RecipeCategorys[category];
            }
            else
            {
                return new List<string>();
            }
        }

        public Recipe CreateRecipe(string id)
        {
            if (this.IsValidID(id))
            {
                return new Recipe(id);
            }
            else
            {
                return null;
            }
        }
    }
}
