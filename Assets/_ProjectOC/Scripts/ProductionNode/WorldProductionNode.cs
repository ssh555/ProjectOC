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
        protected string ProductionNodeID{ get { return this.ProductionNode?.ID ?? ""; }}
        /// <summary>
        /// ӵ�е������ڵ�
        /// </summary>
        [ShowInInspector, ReadOnly, SerializeField]
        public ProductionNode ProductionNode;
        public void SetProductionNode(ProductionNode node)
        {
            if (this.ProductionNode != null)
            {
                this.ProductionNode.WorldProductionNode = null;
            }
            this.ProductionNode = node;
            this.ProductionNode.WorldProductionNode = this;
        }
        private void Start()
        {
            // ����Ҫ Update
            this.enabled = false;
        }
    }
}
