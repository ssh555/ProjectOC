using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ClanNS
{
    /// <summary>
    /// 临时用的氏族管理类
    /// </summary>
    [System.Serializable]
    public sealed class ClanManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public List<Clan> Clans = new List<Clan>();
        public ClanManager() 
        {
            Clans.Add(new Clan("1", "氏族1"));
            Clans.Add(new Clan("2", "氏族2"));
            Clans.Add(new Clan("3", "氏族3"));
            Clans.Add(new Clan("4", "氏族4"));
            Clans.Add(new Clan("5", "氏族5"));
        }
    }
}