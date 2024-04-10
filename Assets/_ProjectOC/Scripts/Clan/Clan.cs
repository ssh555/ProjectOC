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

        public class Sort : IComparer<Clan>
        {
            public int Compare(Clan x, Clan y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
                }
                bool hasBedX = x.HasBed;
                bool hasBedY = y.HasBed;
                if (hasBedX != hasBedY)
                {
                    return hasBedX.CompareTo(hasBedY);
                }
                return x.ID.CompareTo(y.ID);
            }
        }
    }
}