using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace ProjectOC.ClanNS
{
    [LabelText("临时的氏族管理"), System.Serializable]
    public sealed class ClanManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [LabelText("氏族")]
        public List<Clan> Clans = new List<Clan>();
        public ClanConfig Config;
        private SpriteAtlas iconAtlas;
        public const string IconLabel = "SA_Clan_UI";
        public void OnRegister() 
        {
            Clans.Add(new Clan("1", "氏族1"));
            Clans.Add(new Clan("2", "氏族2"));
            Clans.Add(new Clan("3", "氏族3"));
            Clans.Add(new Clan("4", "氏族4"));
            Clans.Add(new Clan("5", "氏族5"));
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(IconLabel).Completed += (handle) =>
            {
                iconAtlas = handle.Result;
            };
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<ClanConfigAsset>("Config_Clan").Completed += (handle) =>
            {
                Config = new ClanConfig(handle.Result.Config);
            };
        }

        public Sprite GetItemSprite(string id)
        {
            return iconAtlas.GetSprite(id);
        }
        public string GetRandomName()
        {
            return "";
        }
        public Gender GetRandomGender()
        {
            if (Config.GenderWeight.Count > 0)
            {
                List<int> weights = new List<int>();
                foreach (var tup in Config.GenderWeight)
                {
                    weights.Add(tup.Weight);
                }
                int index = ML.Engine.MathNS.Math.GetRandomIndex(weights);
                return Config.GenderWeight[index].Gender;
            }
            return Gender.None;
        }
        public SexPreference GetRandomSexPreference()
        {
            if (Config.SexPreferenceWeight.Count > 0)
            {
                List<int> weights = new List<int>();
                foreach (var tup in Config.SexPreferenceWeight)
                {
                    weights.Add(tup.Weight);
                }
                int index = ML.Engine.MathNS.Math.GetRandomIndex(weights);
                return Config.SexPreferenceWeight[index].SexPreference;
            }
            return SexPreference.None;
        }
        public int GetRandomAge()
        {
            return (Config.AgeLow <= Config.AgeHigh) ?
                UnityEngine.Random.Range(Config.AgeLow, Config.AgeHigh + 1) : 0;
        }
        public string GetRandomRaceType()
        {
            return "";
        }
        public string GetRandomWorldCognitionID()
        {
            return "";
        }
        public string GetWorldCognition(string id)
        {
            return "";
        }
        public string GetRandomBeliefID()
        {
            return "";
        }
        public string GetBelief(string id)
        {
            return "";
        }
    }
}