using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;

using UnityEngine;


namespace ProjectOC.LandMassExpand
{
    public class BuildPowerIslandManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        public static BuildPowerIslandManager Instance = null;

        void OnDestroy()
        {
            if(Instance == this)
            {
                Instance = null;
            }
        }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }
            Instance = this;
        }

        private void Start()
        {
            GameManager.Instance.RegisterLocalManager(this);
        }

        public List<BuildPowerCore> powerCores = new List<BuildPowerCore>();
        public List<BuildPowerSub> powerSubs = new List<BuildPowerSub>();
        public List<ElectAppliance> electAppliances = new List<ElectAppliance>();
        
        
        //重新计算电线受电情况
        public void PowerSupportCalculation()
        {
            List<BuildPowerSub> detectInPowers = new List<BuildPowerSub>();
            //powerSubs.RemoveAll(p => p == null);
            //powerCores.RemoveAll(p => p == null);
            foreach (var powerSub in powerSubs)
            {
                foreach (var powerCore in powerCores)
                {
                    if (CoverEachOther(powerCore, powerSub))
                    {
                        detectInPowers.Add(powerSub);                    
                        continue;
                    } 
                }
            }
            
            
            List<BuildPowerSub> inPowers = new List<BuildPowerSub>(detectInPowers);
            List<BuildPowerSub> unPowers = new List<BuildPowerSub>(powerSubs);
            foreach (var detectInPower in detectInPowers)
            {
                unPowers.Remove(detectInPower);
            }
            
            while (detectInPowers.Count !=0)
            {
                List<BuildPowerSub> tempInPowers = new List<BuildPowerSub>();
                foreach (var unPower in unPowers)
                {
                    foreach (var detectInPower in detectInPowers)
                    {
                        if (CoverEachOther(unPower, detectInPower))
                        {
                            tempInPowers.Add(unPower);
                            continue;
                        }
                    }
                }
                
                detectInPowers.Clear();
                detectInPowers = tempInPowers;
                
                foreach (var detectInPower in detectInPowers)
                {
                    inPowers.Add(detectInPower);
                    unPowers.Remove(detectInPower);
                    
                }
            }

            foreach (var inPower in inPowers)
            {
                inPower.Inpower = true;
            }
            foreach (var unPower in unPowers)
            {
                unPower.Inpower = false;
            }
            
        }

        public bool CoverEachOther(IPowerBPart powerBPart1, IPowerBPart powerBPart2)
        {
            float r1 = powerBPart1.PowerSupportRange;
            float r2 = powerBPart2.PowerSupportRange;
            Vector3 pos1 = powerBPart1.transform.position;
            Vector3 pos2 = powerBPart2.transform.position;
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