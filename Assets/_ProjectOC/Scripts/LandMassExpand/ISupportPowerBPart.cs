using ML.Engine.BuildingSystem.BuildingPart;

namespace ProjectOC.LandMassExpand
{
    public interface ISupportPowerBPart : IBuildingPart
    {
        bool InPower { get; set; }
        int PowerSupportRange { get; set; }
    }   
}

