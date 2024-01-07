using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ProductionNodeNS
{
    public class WorldProductionNode : BuildingPart
    {
        /// <summary>
        /// �����ڵ�ID
        /// </summary>
        [SerializeField, ReadOnly]
        protected string ProductionNodeID = "";
        /// <summary>
        /// ӵ�е������ڵ�
        /// </summary>
        [ShowInInspector, ReadOnly, SerializeField]
        protected ProductionNode ProductionNode;
        public void SetProductionNode(ProductionNode node)
        {
            this.ProductionNode = node;
            this.ProductionNodeID = this.ProductionNode != null ? this.ProductionNode.ID : "";
            this.ProductionNode.UID = this.InstanceID;
        }
        private void Start()
        {
            // ����Ҫ Update
            this.enabled = false;
        }
    }
}
