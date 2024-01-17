using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public interface INeedPowerBpart : IBuildingPart
    {
        int PowerCount { get; set; }
    }   
}
