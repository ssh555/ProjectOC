using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("��ʱ������"), System.Serializable]
    public class Clan
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("����"), ReadOnly]
        public string Name = "";
        [LabelText("ӵ�еĴ�"), ReadOnly, System.NonSerialized]
        public ClanBed Bed;
        [LabelText("�Ƿ�ӵ�д�"), ShowInInspector, ReadOnly]
        public bool HasBed { get { return Bed != null && !string.IsNullOrEmpty(Bed.InstanceID); } }

        public Clan() { }
        public Clan(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public class Sort : IComparer<Clan>
        {
            public int Compare(Clan x, Clan y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y == null));
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