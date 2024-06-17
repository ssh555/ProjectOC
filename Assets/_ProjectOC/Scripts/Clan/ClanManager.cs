using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using ML.Engine.MathNS;
using System.Linq;

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
        private List<PersonaTemplateTableData> PersonaTemplates = new List<PersonaTemplateTableData>();
        private List<string> OCNames = new List<string>();
        private Dictionary<string, WorldCognitionTableData> WorldCognitions = new Dictionary<string, WorldCognitionTableData>();
        private Dictionary<string, BeliefTableData> Beliefs = new Dictionary<string, BeliefTableData>();
        private Dictionary<string, PersonalityTAGTableData> PersonalityTAGs = new Dictionary<string, PersonalityTAGTableData>();
        private List<string> RaceTypes = new List<string>();

        public void OnRegister() 
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(IconLabel).Completed += (handle) =>
            {
                iconAtlas = handle.Result;
            };
            var ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<PersonaTemplateTableData[]>("OCTableData", "PersonaTemplate", (datas) =>
            {
                foreach (var data in datas)
                {
                    PersonaTemplates.Add(data);
                }
            }, "人格模板表数据");
            ABJAProcessor.StartLoadJsonAssetData();
            var ABJAProcessor1 = new ML.Engine.ABResources.ABJsonAssetProcessor<OCNameTableData[]>("OCTableData", "OCName", (datas) =>
            {
                foreach (var data in datas)
                {
                    OCNames.Add(data.Name);
                }
            }, "OC名字表数据");
            ABJAProcessor1.StartLoadJsonAssetData();
            var ABJAProcessor2 = new ML.Engine.ABResources.ABJsonAssetProcessor<WorldCognitionTableData[]>("OCTableData", "WorldCognition", (datas) =>
            {
                foreach (var data in datas)
                {
                    WorldCognitions.Add(data.ID, data);
                }
            }, "世界认知表数据");
            ABJAProcessor2.StartLoadJsonAssetData();
            var ABJAProcessor3 = new ML.Engine.ABResources.ABJsonAssetProcessor<BeliefTableData[]>("OCTableData", "Belief", (datas) =>
            {
                foreach (var data in datas)
                {
                    Beliefs.Add(data.ID, data);
                }
            }, "信念表数据");
            ABJAProcessor3.StartLoadJsonAssetData();
            var ABJAProcessor4 = new ML.Engine.ABResources.ABJsonAssetProcessor<PersonalityTAGTableData[]>("OCTableData", "PersonalityTAG", (datas) =>
            {
                foreach (var data in datas)
                {
                    PersonalityTAGs.Add(data.ID, data);
                }
            }, "TAG表数据");
            ABJAProcessor4.StartLoadJsonAssetData();
            var ABJAProcessor5 = new ML.Engine.ABResources.ABJsonAssetProcessor<RaceTypeTableData[]>("OCTableData", "RaceType", (datas) =>
            {
                foreach (var data in datas)
                {
                    RaceTypes.Add(data.RaceType);
                }
            }, "种族类型表数据");
            ABJAProcessor5.StartLoadJsonAssetData();

            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<ClanConfigAsset>("Config_Clan").Completed += (handle) =>
            {
                Config = new ClanConfig(handle.Result.Config);
                Clans.Add(new Clan("1", "氏族1"));
                Clans.Add(new Clan("2", "氏族2"));
                Clans.Add(new Clan("3", "氏族3"));
                Clans.Add(new Clan("4", "氏族4"));
                Clans.Add(new Clan("5", "氏族5"));
            };
        }

        public Sprite GetItemSprite(string id)
        {
            return iconAtlas.GetSprite(id);
        }
        public string GetRandomName()
        {
            return (OCNames.Count > 0) ? OCNames[Math.GetRandomIndex(OCNames.Count)] : "";
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
                int index = Math.GetRandomIndex(weights);
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
                int index = Math.GetRandomIndex(weights);
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
            return (RaceTypes.Count > 0) ? RaceTypes[Math.GetRandomIndex(RaceTypes.Count)] : "";
        }
        public string GetRandomWorldCognitionID()
        {
            return (WorldCognitions.Count > 0) ? WorldCognitions.Keys.ToList()[Math.GetRandomIndex(WorldCognitions.Count)] : "";
        }
        public string GetWorldCognition(string id)
        {
            return (!string.IsNullOrEmpty(id) && WorldCognitions.ContainsKey(id)) ? WorldCognitions[id].Description : "";
        }
        public string GetRandomBeliefID()
        {
            return (Beliefs.Count > 0) ? Beliefs.Keys.ToList()[Math.GetRandomIndex(Beliefs.Count)] : "";
        }
        public string GetBelief(string id)
        {
            return (!string.IsNullOrEmpty(id) && Beliefs.ContainsKey(id)) ? Beliefs[id].Description : "";
        }
        public List<int> GetTalentInitValue(int level)
        {
            double k0 = Math.UniformDistribution(Config.TalentA, Config.TalentB, 2);
            List<int> results = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                List<double> temps = new List<double>() { 0 };
                for (int j = 0; j < 3; j++)
                {
                    double value = Config.TalentM[j] *
                        Math.SkewedDistribution(Config.TalentC, Config.TalentD, k0, Config.TalentVar, Config.TalentLambda, 2);
                    temps.Add(value);
                }
                double result = 0;
                for (int j = 0; j <= level; j++)
                {
                    result += temps[j];
                }
                results.Add((int)System.Math.Round(result));
            }
            return results;
        }
        public PersonalityTAGTableData GetTAG(string id)
        {
            return (!string.IsNullOrEmpty(id) && PersonalityTAGs.ContainsKey(id)) ? PersonalityTAGs[id] : default(PersonalityTAGTableData);
        }
        /// <summary>
        /// (value, low, high)
        /// </summary>
        public List<(int, int, int)> GetPersonalityInitValue(int level)
        {
            List<(int, int, int)> results = new List<(int, int, int)>();
            int len = (level == 0) ? 20 : (10 - 2 * level);
            if (PersonaTemplates.Count > 0)
            {
                PersonaTemplateTableData template = PersonaTemplates[Math.GetRandomIndex(PersonaTemplates.Count)];
                results.Add(GetPersonalityInitValue(len, template.Think));
                results.Add(GetPersonalityInitValue(len, template.Social));
                results.Add(GetPersonalityInitValue(len, template.Basis));
            }
            return results;
        }
        private (int, int, int) GetPersonalityInitValue(int len, int M)
        {
            List<(int, int)> ranges = new List<(int, int)>();
            for (int j = -10; j <= 10; j++)
            {
                if (j + len <= 10 && j <= M && M <= j + len)
                {
                    ranges.Add((j, j + len));
                }
            }
            (int, int) range = ranges[Math.GetRandomIndex(ranges.Count)];
            int value = Math.UniformDistribution(range.Item1, range.Item2);
            return (value, range.Item1, range.Item2);
        }
    }
}