using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("临时的氏族管理"), System.Serializable]
    public sealed class ClanManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [LabelText("氏族")]
        public List<Clan> Clans = new List<Clan>();
        public void OnRegister() 
        {
            Clans.Add(new Clan("1", "氏族1"));
            Clans.Add(new Clan("2", "氏族2"));
            Clans.Add(new Clan("3", "氏族3"));
            Clans.Add(new Clan("4", "氏族4"));
            Clans.Add(new Clan("5", "氏族5"));
        }
    }
}