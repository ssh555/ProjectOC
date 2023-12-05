using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace ProjectOC.TechTree
{
    [System.Serializable]
    public class TechPoint
    {
        public string ID;
        public TechPointCategory Category;
        public string Icon;
        public ML.Engine.TextContent.TextContent Description;
        public bool IsUnlocked;
        public string[] UnLockRecipe;
        public string[] UnLockBuild;
        public string[] PrePoint;
        public ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Formula[] ItemCost;
        public int TimeCost;
    }
}
