using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public interface IPowerBPart : ML.Engine.BuildingSystem.BuildingPart.IBuildingPart
    {
        public int PowerCount { get; set; }
        public int PowerSupportRange { get; set; }
        public bool Inpower { get; set; }
        
    }
}