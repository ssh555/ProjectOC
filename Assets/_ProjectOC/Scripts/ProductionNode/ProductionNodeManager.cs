using ML.Engine.Manager.LocalManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ProductionNodeNS
{
    /// <summary>
    /// �����ڵ������
    /// TODO:�Ƴ��յ������ڵ�
    /// </summary>
    [System.Serializable]
    public sealed class ProductionNodeManager : ILocalManager
    {
        /// <summary>
        /// �������������ڵ��ID
        /// </summary>
        public HashSet<string> ProductionNodeIDs = new HashSet<string>();
        /// <summary>
        /// ʵ�������ɵ������ڵ�
        /// </summary>
        private Dictionary<string, ProductionNode> ProductionNodeDict = new Dictionary<string, ProductionNode>();

        public ProductionNodeManager()
        {
            // TODO:�����ʼ��ProductionNodeIDs
        }

        /// <summary>
        /// �Ƿ�����Ч�������ڵ�ID
        /// </summary>
        /// <param name="ID">�����ڵ�ID</param>
        /// <returns></returns>
        public bool IsValidID(string ID)
        {
            return ProductionNodeIDs.Contains(ID);
        }
        /// <summary>
        /// TODO: UID�ж�
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public bool IsValidUID(string UID)
        {
            return ProductionNodeDict.ContainsKey(UID);
        }
        /// <summary>
        /// ��ӦUID�������ڵ��Ƿ���������
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
        /// ���������ڵ�
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="ID">�����ڵ�ID</param>
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
        /// ����UID��ȡ�Ѿ�ʵ�����������ڵ�
        /// </summary>
        /// <param name="UID">�����ڵ�UID</param>
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

