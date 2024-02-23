using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public interface INeedPowerBpart : IPowerBPart
    {
        int PowerCount { get; set; }
        bool InPower => PowerCount > 0;
        public void RemoveFromAllPowerCores();
    }   
}
