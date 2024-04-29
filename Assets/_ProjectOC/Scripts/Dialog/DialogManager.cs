using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public class DialogManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public void OnRegister()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        private void LoadTableData()
        {
            
        }
        
        

        public struct Dialog
        {
            public string ID;
            public ML.Engine.TextContent.TextContent Content;
            public string CharacterID;
            public ML.Engine.TextContent.TextContent Name;
            public string Audio;
            public string MoodID;
            public string NextID;
            public string OptionID;
            public string BHasOption;
        }

        public struct Option
        {
            public string ID;
            public ML.Engine.TextContent.TextContent Optiontext1;
            public string OptionNextID1;
            public ML.Engine.TextContent.TextContent Optiontext2;
            public string OptionNextID2;
            public ML.Engine.TextContent.TextContent Optiontext3;
            public string OptionNextID3;
        }
    }   
}
