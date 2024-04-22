using ProjectOC.Building;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ClanNS
{
    [LabelText("临时的氏族"), System.Serializable]
    public class Clan
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("名称"), ReadOnly]
        public string Name = "";
        [LabelText("拥有的床"), ReadOnly]
        public ClanBed Bed;
        [LabelText("是否拥有床"), ShowInInspector, ReadOnly]
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