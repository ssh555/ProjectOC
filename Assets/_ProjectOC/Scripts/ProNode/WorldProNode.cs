using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ProNodeNS
{
    public class WorldProNode : BuildingPart, IInteraction
    {
        [ShowInInspector, ReadOnly, SerializeField]
        public ProNode ProNode;

        public string InteractType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Vector3 PosOffset { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void SetProNode(ProNode node)
        {
            if (this.ProNode != null)
            {
                this.ProNode.WorldProNode = null;
            }
            this.ProNode = node;
            this.ProNode.WorldProNode = this;
        }

        private void Start()
        {
            // ²»ÐèÒª Update
            this.enabled = false;
        }
    }
}
