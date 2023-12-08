using ML.Engine.BuildingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ML.Engine.TextContent
{
    [System.Serializable]
    public struct TextContent
    {
        public string Chinese;

        public string English;

        public string GetText()
        {
            if (Config.language == Config.Language.Chinese)
            {
                return Chinese;
            }
            else if (Config.language == Config.Language.English)
            {
                return English;
            }
            return "";
        }
    }

}
