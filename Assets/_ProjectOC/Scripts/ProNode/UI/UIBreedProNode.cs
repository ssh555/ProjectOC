using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ML.Engine.Utility;
using static ProjectOC.ProNodeNS.UI.UIBreedProNode;

namespace ProjectOC.ProNodeNS.UI
{
    public class UIBreedProNode : ML.Engine.UI.UIBasePanel<ProNodePanel>
    {
        public BreedProNode ProNode;
        [System.Serializable]
        public struct ProNodePanel
        {
            public TextContent textProNodeType;
            public TextContent textEmpty;
            public TextContent textLack;
            public TextContent[] TransportPriority;
            public TextContent textStateVacancy;
            public TextContent textStateStagnation;
            public TextContent textStateProduction;
            public TextContent textTime;
            public TextContent textEff;
            public TextContent textBag;

            public KeyTip NextPriority;
            public KeyTip ChangeRecipe;
            public KeyTip ChangeBar;
            public KeyTip FastRemove;
            public KeyTip FastAdd;
            public KeyTip Return;
            public KeyTip Confirm;
            public KeyTip Back;
        }
    }
}