using ML.Engine.TextContent;

namespace ProjectOC.InventorySystem.UI
{
    public class UIProNode
    {
        [System.Serializable]
        public struct ProNodePanel
        {
            public TextTip[] proNodeType;
            public TextContent textEmpty;
            public TextContent textUrgency;
            public TextContent textNormal;
            public TextContent textAlternative;
            public TextContent textStateVacancy;
            public TextContent textStateStagnation;
            public TextContent textStateProduction;
            public TextContent textWorkerStateWork;
            public TextContent textWorkerStateTransport;
            public TextContent textWorkerStateFish;
            public TextContent textWorkerStateRelax;
            public TextContent textWorkerOnDuty;
            public TextContent textPrefixTime;
            public TextContent textPrefixEff;

            public KeyTip ktUpgrade;
            public KeyTip ktNextPriority;
            public KeyTip ktChangeRecipe;
            public KeyTip ktRemove1;
            public KeyTip ktRemove10;
            public KeyTip ktFastAdd;
            public KeyTip ktBack;
            public KeyTip ktChangeWorker;
            public KeyTip ktRemoveWorker;
            public KeyTip ktConfirmRecipe;
            public KeyTip ktBackRecipe;
            public KeyTip ktConfirmWorker;
            public KeyTip ktBackWorker;
            public KeyTip ktConfirmLevel;
            public KeyTip ktBackLevel;
        }
    }
}
