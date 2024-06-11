using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("氏族"), System.Serializable]
    public abstract class NPC
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("姓名"), ReadOnly]
        public string Name = "";
        [LabelText("代号"), ReadOnly]
        public string PetName = "";
        [LabelText("性别"), ReadOnly]
        public Gender Gender;
        [LabelText("性取向"), ReadOnly]
        public SexPreference SexPreference;
        [LabelText("年龄"), ReadOnly]
        public int Age;
        [LabelText("种族"), ReadOnly]
        public string RaceType;
        [LabelText("世界认知ID"), ReadOnly]
        public string WorldCognitionID;
        [LabelText("世界认知"), ReadOnly, ShowInInspector]
        public string WorldCognition => ManagerNS.LocalGameManager.Instance != null ?
            ManagerNS.LocalGameManager.Instance.ClanManager.GetWorldCognition(WorldCognitionID) : "";
        [LabelText("信念ID"), ReadOnly]
        public string BeliefID;
        [LabelText("信念"), ReadOnly, ShowInInspector]
        public string Belief => ManagerNS.LocalGameManager.Instance != null ?
            ManagerNS.LocalGameManager.Instance.ClanManager.GetBelief(BeliefID) : "";

        public List<PersonalityTAG> TAGS = new List<PersonalityTAG>();

        public NPC()
        {
            var manager = ManagerNS.LocalGameManager.Instance.ClanManager;
            Name = manager.GetRandomName();
            Gender = manager.GetRandomGender();
            SexPreference = manager.GetRandomSexPreference();
            Age = manager.GetRandomAge();
            RaceType = manager.GetRandomRaceType();
            WorldCognitionID = manager.GetRandomWorldCognitionID();
            BeliefID = manager.GetRandomBeliefID();
        }

        public void SetName(string name)
        {
            Name = name;
        }
        public void SetPetName(string name)
        {
            PetName = name;
        }
    }
}