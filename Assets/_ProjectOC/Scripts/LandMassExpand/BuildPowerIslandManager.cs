using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;

using UnityEngine;


namespace ProjectOC.LandMassExpand
{
    public class BuildPowerIslandManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public List<ISupportPowerBPart> powerCores = new List<ISupportPowerBPart>();
        public List<BuildPowerSub> powerSubs = new List<BuildPowerSub>();
        public List<INeedPowerBpart> electAppliances = new List<INeedPowerBpart>();
        

        public bool CoverEachOther(IPowerBPart powerBPart1, IPowerBPart powerBPart2)
        {
            float r1 = powerBPart1.PowerSupportRange;
            float r2 = powerBPart2.PowerSupportRange;
            Vector3 pos1 = powerBPart1.transform.position;
            Vector3 pos2 = powerBPart2.transform.position;
            return CoverEachOther(r1, r2, pos1, pos2);
        }
        public bool CoverEachOther(IPowerBPart powerBPart1, float r2, Vector3 pos2)
        {
            float r1 = powerBPart1.PowerSupportRange;
            Vector3 pos1 = powerBPart1.transform.position;
            return CoverEachOther(r1, r2, pos1, pos2);
        }
        
        public bool CoverEachOther(float r1, float r2, Vector3 pos1, Vector3 pos2)
        {
            float distanceSquared = (pos1 - pos2).sqrMagnitude;
            float radiusSunSquared = (r1 + r2) * (r1 + r2);
            return radiusSunSquared >= distanceSquared;
        }


    }
}