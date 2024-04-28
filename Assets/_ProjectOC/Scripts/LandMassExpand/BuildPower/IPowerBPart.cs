using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public interface IPowerBPart : IBuildingPart
    {
        public float PowerSupportRange { get; set; }
    }   
}

