using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public class WorldStore : BuildingPart
    {
        [SerializeField, ReadOnly]
        protected string StoreID = "";
        [ShowInInspector, ReadOnly, SerializeField]
        public Store Store;
        public void SetStore(Store store)
        {
            this.Store = store;
            this.StoreID = this.Store != null ? this.Store.ID : "";
            this.Store.UID = this.InstanceID;
        }
        private void Start()
        {
            // ²»ÐèÒª Update
            this.enabled = false;
        }
    }
}

