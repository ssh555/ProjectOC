using System.Collections.Generic;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public struct OptionTableData
    {
        public string ID;
        public List<OnePieceOption> Options;
        public struct OnePieceOption
        {
            public ML.Engine.TextContent.TextContent OptionText;
            public string NextID;

        }
    }
}
