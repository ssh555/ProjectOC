using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.OrderNS
{
    /// <summary>
    /// ����
    /// </summary>
    public struct Order
    {
        public string ID;
        public string Name;
        public string Description;
        public OrderType OrderType;
        /// <summary>
        /// �����嵥 (ItemID, Amount)
        /// </summary>
        public List<Formula> RequireList;
        /// <summary>
        /// ��Ʒ���� (ItemID, Amount)
        /// </summary>
        public List<Formula> ItemRewards;
        /// <summary>
        /// ������������ (����ID, Amount)
        /// </summary>
        public List<Formula> ClanRewards;
        /// <summary>
        /// ��ɫ�������� (��ɫID, Amount)
        /// </summary>
        public List<Formula> CharaRewards;
        /// <summary>
        /// ��������
        /// </summary>
        public int CD;

        /// <summary>
        /// ��ȡ����
        /// </summary>
        public int AcceptDay;
        /// <summary>
        /// �������
        /// </summary>
        public int FinishDay;

        public void End()
        {
            CD -= 1;
        }
    }
}
