namespace ProjectOC.Dialog
{
    [System.Serializable]
    public struct DialogTableData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Content;
        public string CharacterID;
        public ML.Engine.TextContent.TextContent Name;
        public string Audio;
        public string MoodID;
        public string ActionID;
        public string NextID;
        public string OptionID;
        public string BHasOption;
    }
}