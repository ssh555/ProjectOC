
namespace ProjectOC.LandMassExpand
{
    [System.Serializable]
    public struct MainlandLevelTableData
    {
        public string ID;
        public int Level;
        public ML.Engine.TextContent.TextContent LevelText;
        public bool IsMax;
        public string[] Conditions;
        public string[] Events;
        public ML.Engine.TextContent.TextContent[] EventTexts;
    }
}