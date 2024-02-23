using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public interface ISupportPowerBPart : IPowerBPart
    {
        public List<INeedPowerBpart> needPowerBparts { get; }

        public void RemoveNeedPowerBpart(INeedPowerBpart iNeedPowerBpart)
        {
            if (needPowerBparts.Contains(iNeedPowerBpart))
            {
                needPowerBparts.Remove(iNeedPowerBpart);
            }
        }
    }   
}