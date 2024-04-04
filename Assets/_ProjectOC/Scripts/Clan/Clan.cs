using ProjectOC.Building;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ClanNS
{
    /// <summary>
    /// ��ʱ��������
    /// </summary>
    public class Clan
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ����
        /// </summary>
        public string Name = "";
        /// <summary>
        /// ӵ�еĴ�
        /// </summary>
        public Bed Bed;
        /// <summary>
        /// �Ƿ�ӵ�д�
        /// </summary>
        public bool HasBed { get { return Bed != null && !string.IsNullOrEmpty(Bed.InstanceID); } }

        public Clan() { }
        public Clan(string id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}