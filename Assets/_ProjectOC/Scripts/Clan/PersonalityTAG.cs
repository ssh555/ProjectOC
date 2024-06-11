using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ClanNS
{
    public struct PersonalityTAG
    {
        public string ID;
        public string Name;
        public string Desc;
        public TAGType Type;
        public int Level;

        public int Wisdom;
        public int Combat;
        public int Resilience;
        public int Thinking;
        public int Social;
        public int Basis;

        public bool IsActive;
    }
}