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
    public struct MultiTextContent
    {
        public TextContent[] description;
        public int index;
        public string GetDescription()
        {
            return this.description[this.index].GetText();
        }
    }

}
