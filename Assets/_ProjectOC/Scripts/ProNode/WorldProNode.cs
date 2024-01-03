using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ProNodeNS
{
    public class WorldProNode : BuildingPart
    {
        /// <summary>
        /// 生产节点ID
        /// </summary>
        [SerializeField, ReadOnly]
        protected string ProNodeID{ get { return this.ProNode?.ID ?? ""; }}
        /// <summary>
        /// 拥有的生产节点
        /// </summary>
        [ShowInInspector, ReadOnly, SerializeField]
        public ProNode ProNode;
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
            // 不需要 Update
            this.enabled = false;
        }
    }
}
