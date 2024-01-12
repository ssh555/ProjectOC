using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
using Unity.VisualScripting;
using UnityEngine;

namespace ML.Engine.BuildingSystem
{
    public class BuildPowerIslandManager : Manager.LocalManager.ILocalManager
    {
        private static BuildPowerIslandManager instance = null;

        public static BuildPowerIslandManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BuildPowerIslandManager();
                    Manager.GameManager.Instance.RegisterLocalManager(instance);
                }
                return instance;
            }    
        }
        ~BuildPowerIslandManager()
        {
            if(instance == this)
            {
                instance = null;
            }
        }

        void Start()
        {
            
        }
        public List<BuildPowerCore> powerCores;
        public List<BuildPowerSub> powerSubs;



        //重新计算电线受电情况
        public void PowerSupportCalculation()
        {
            List<BuildPowerSub> detectInPower;
            
            foreach (var _powerSub in powerSubs)
            {
                _powerSub.inPower = false;
            }
        }

        bool CoverEachOther(BuildPowerCore buildPowerCore, BuildPowerSub buildPowerSub)
        {
            float r1 = buildPowerCore.powerSupportRange;
            float r2 = buildPowerSub.powerSupportRange;
            Vector3 pos1 = buildPowerCore.transform.position;
            Vector3 pos2 = buildPowerCore.transform.position;
            return RangeCoverEachOther(r1, r2, pos1, pos2);
        }

        bool CoverEachOther(BuildPowerSub buildPowerSub1, BuildPowerSub buildPowerSub2)
        {
            float r1 = buildPowerSub1.powerSupportRange;
            float r2 = buildPowerSub2.powerSupportRange;
            Vector3 pos1 = buildPowerSub1.transform.position;
            Vector3 pos2 = buildPowerSub2.transform.position;
            return RangeCoverEachOther(r1, r2, pos1, pos2);
        }

        bool RangeCoverEachOther(float r1, float r2, Vector3 pos1, Vector3 pos2)
        {
            float distanceSquared = (pos1 - pos2).sqrMagnitude;
            float radiusSunSquared = (r1 + r2) * (r1 + r2);
            return radiusSunSquared >= distanceSquared;
        }


    }
}