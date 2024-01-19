using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public class WorldStore : BuildingPart, IInteraction
    {
        [ShowInInspector, ReadOnly, SerializeField]
        public Store Store;

        public string InteractType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Vector3 PosOffset { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

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

