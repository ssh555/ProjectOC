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
        protected string StoreID { get { return this.Store?.ID ?? ""; } }
        [ShowInInspector, ReadOnly, SerializeField]
        public Store Store;
        public void SetStore(Store store)
        {
            if (this.Store != null)
            {
                this.Store.WorldStore = null;
            }
            this.Store = store;
            this.Store.WorldStore = this;
        }
        private void Start()
        {
            // ²»ÐèÒª Update
            this.enabled = false;
        }
    }
}

