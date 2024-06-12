using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ClanNS
{
    [LabelText("����"), System.Serializable]
    public abstract class NPC
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("����"), ReadOnly]
        public string Name = "";
        [LabelText("����"), ReadOnly]
        public string PetName = "";
        [LabelText("�Ա�"), ReadOnly]
        public Gender Gender;
        [LabelText("��ȡ��"), ReadOnly]
        public SexPreference SexPreference;
        [LabelText("����"), ReadOnly]
        public int Age;
        [LabelText("����"), ReadOnly]
        public string RaceType;
        [LabelText("������֪ID"), ReadOnly]
        public string WorldCognitionID;
        [LabelText("������֪"), ReadOnly, ShowInInspector]
        public string WorldCognition => ManagerNS.LocalGameManager.Instance != null ?
            ManagerNS.LocalGameManager.Instance.ClanManager.GetWorldCognition(WorldCognitionID) : "";
        [LabelText("����ID"), ReadOnly]
        public string BeliefID;
        [LabelText("����"), ReadOnly, ShowInInspector]
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