using ProjectOC.Building;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ClanNS
{
    /// <summary>
    /// 临时的氏族类
    /// </summary>
    public class Clan
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// 名称
        /// </summary>
        public string Name = "";
        /// <summary>
        /// 拥有的床
        /// </summary>
        public Bed Bed;
        /// <summary>
        /// 是否拥有床
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