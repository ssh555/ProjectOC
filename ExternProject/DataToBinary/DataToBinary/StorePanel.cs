using ML.Engine.TextContent;

namespace ProjectOC.InventorySystem.UI
{
    public class UIStore
    {
        [System.Serializable]
        public struct StorePanel
        {
            public TextContent text_Title;
            public TextContent text_Empty;
            public TextContent text_PriorityUrgency;
            public TextContent text_PriorityNormal;
            public TextContent text_PriorityAlternative;
            public TextContent text_Add;
            public TextContent text_Remove;

            public KeyTip kt_NextPriority;
            public KeyTip kt_ChangeItem;
            public KeyTip kt_Remove1;
            public KeyTip kt_Remove10;
            public KeyTip kt_FastAdd;
            public KeyTip kt_ChangeItem_Confirm;
            public KeyTip kt_ChangeItem_Back;
        }
    }
}
