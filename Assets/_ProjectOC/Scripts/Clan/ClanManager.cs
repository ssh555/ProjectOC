using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ClanNS
{
    /// <summary>
    /// ��ʱ�õ����������
    /// </summary>
    [System.Serializable]
    public sealed class ClanManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public List<Clan> Clans = new List<Clan>();
        public ClanManager() 
        {
            Clans.Add(new Clan("1", "����1"));
            Clans.Add(new Clan("2", "����2"));
            Clans.Add(new Clan("3", "����3"));
            Clans.Add(new Clan("4", "����4"));
            Clans.Add(new Clan("5", "����5"));
        }
    }
}