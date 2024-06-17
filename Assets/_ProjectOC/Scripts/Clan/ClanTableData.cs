namespace ProjectOC.ClanNS
{
    [System.Serializable]
    public struct PersonaTemplateTableData
    {
        public string ID;
        public int Think;
        public int Social;
        public int Basis;
    }
    [System.Serializable]
    public struct OCNameTableData
    {
        public string ID;
        public string Name;
    }
    [System.Serializable]
    public struct WorldCognitionTableData
    {
        public string ID;
        public string Name;
        public string Description;
    }
    [System.Serializable]
    public struct BeliefTableData
    {
        public string ID;
        public string Name;
        public string Description;
    }
    [System.Serializable]
    public struct PersonalityTAGTableData
    {
        public string ID;
        public int Sort;
        public string Name;
        public string Description;
        public TAGType Type;
        public int Level;
        public int Wisdom;
        public int Combat;
        public int Resilience;
        public int ThinkingLow;
        public int ThinkingHigh;
        public int SocialLow;
        public int SocialHigh;
        public int BasisLow;
        public int BasisHigh;
    }
    [System.Serializable]
    public struct RaceTypeTableData
    {
        public string ID;
        public string RaceType;
    }
}