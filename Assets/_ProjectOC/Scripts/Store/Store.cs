using System;
using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    [LabelText("²Ö¿â"), Serializable]
    public class Store : IStore
    {
        public Store(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType, int level) : base(storeType, level) { }
    }
}