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
        /// �����ڵ�ID
        /// </summary>
        [SerializeField, ReadOnly]
        protected string ProNodeID{ get { return this.ProNode?.ID ?? ""; }}
        /// <summary>
        /// ӵ�е������ڵ�
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
            // ����Ҫ Update
            this.enabled = false;
        }
    }
}
