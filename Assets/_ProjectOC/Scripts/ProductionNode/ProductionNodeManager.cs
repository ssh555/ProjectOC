using ML.Engine.Manager.LocalManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ProductionNodeNS
{
    /// <summary>
    /// 生产节点管理器
    /// TODO:移除空的生产节点
    /// </summary>
    [System.Serializable]
    public sealed class ProductionNodeManager : ILocalManager
    {
        /// <summary>
        /// 表里所有生产节点的ID
        /// </summary>
        public HashSet<string> ProductionNodeIDs = new HashSet<string>();
        /// <summary>
        /// 实例化生成的生产节点
        /// </summary>
        private Dictionary<string, ProductionNode> ProductionNodeDict = new Dictionary<string, ProductionNode>();

        public ProductionNodeManager()
        {
            // TODO:读表初始化ProductionNodeIDs
        }

        /// <summary>
        /// 是否是有效的生产节点ID
        /// </summary>
        /// <param name="ID">生产节点ID</param>
        /// <returns></returns>
        public bool IsValidID(string ID)
        {
            return ProductionNodeIDs.Contains(ID);
        }
        /// <summary>
        /// TODO: UID判断
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public bool IsValidUID(string UID)
        {
            return ProductionNodeDict.ContainsKey(UID);
        }
        /// <summary>
        /// 对应UID的生产节点是否正在运作
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public bool IsNodeWorking(string UID)
        {
            if (ProductionNodeDict.ContainsKey(UID))
            {
                return ProductionNodeDict[UID].IsWorking();
            }
            return false;
        }

        /// <summary>
        /// 创建生产节点
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="ID">生产节点ID</param>
        /// <returns></returns>
        public ProductionNode CreateProductionNode(GameObject gameObject, string ID)
        {
            if (this.IsValidID(ID))
            {
                ProductionNode node = gameObject.AddComponent<ProductionNode>();
                node.InitProductionNode(ID);
                ProductionNodeDict.Add(node.UID, node);
                return node;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据UID获取已经实例化的生产节点
        /// </summary>
        /// <param name="UID">生产节点UID</param>
        /// <returns></returns>
        public ProductionNode GetProductionNodeByUID(string UID)
        {
            if (ProductionNodeDict.ContainsKey(UID))
            {
                return ProductionNodeDict[UID];
            }
            return null;
        }
    }
}

