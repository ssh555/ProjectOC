using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace ProjectOC.ClanNS
{
    [LabelText("��ʱ���������"), System.Serializable]
    public sealed class ClanManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [LabelText("����")]
        public List<Clan> Clans = new List<Clan>();
        public void OnRegister() 
        {
            Clans.Add(new Clan("1", "����1"));
            Clans.Add(new Clan("2", "����2"));
            Clans.Add(new Clan("3", "����3"));
            Clans.Add(new Clan("4", "����4"));
            Clans.Add(new Clan("5", "����5"));
            LoadItemAtlas();
        }

        #region SpriteAtlas
        public const string IconLabel = "SA_Clan_UI";

        private SpriteAtlas iconAtlas;
        public void LoadItemAtlas()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(IconLabel).Completed += (handle) =>
            {
                iconAtlas = handle.Result;
            };
        }
        public Sprite GetItemSprite(string id)
        {
            return iconAtlas.GetSprite(id);
        }
        #endregion
    }
}