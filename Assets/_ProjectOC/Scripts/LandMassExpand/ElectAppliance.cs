using System;
using System.Xml.Schema;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using ML.Engine.Extension;
using ML.Engine.Manager;

namespace ProjectOC.LandMassExpand
{
    public class ElectAppliance : BuildingPart, INeedPowerBpart
    {
        private int powerCount = 0;
        [Header("用电器部分"),SerializeField,LabelText("充电范围")]
        private float powerSupportRange;
        public float PowerSupportRange
        {
            get => powerSupportRange;
            set
            {
                powerSupportRange = value;
            }
        }

        [ShowInInspector]
        public int PowerCount
        {
            get => powerCount;
            set
            {
                powerCount = value;
                
                bool inPower = powerCount > 0;
            }
        }
        



        private BuildPowerIslandManager bpIslandManager;

        protected virtual void Start()
        {
            bpIslandManager = GameManager.Instance.GetLocalManager<BuildPowerIslandManager>();
        }

        void OnDestroy()
        {
            if (bpIslandManager != null  && bpIslandManager.electAppliances.Contains(this))
            {
                bpIslandManager.electAppliances.Remove(this);
                RemoveFromAllPowerCores();
            }
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //如果没有，说明刚建造则加入
            if (!bpIslandManager.electAppliances.Contains(this))
            {
                bpIslandManager.electAppliances.Add(this);
            }
            //重置
            PowerCount = 0;
            RemoveFromAllPowerCores();
            //重新计算
            RecalculatePowerCount();
        }

        private void RecalculatePowerCount()
        {
            int tempCount = 0;
            foreach (var powerCore in bpIslandManager.powerCores)
            {
                if (bpIslandManager.CoverEachOther(this, powerCore))
                {
                    powerCore.needPowerBparts.Add(this);
                    tempCount++;
                }
            }
            
            foreach (var powerSub in bpIslandManager.powerSubs)
            {
                if (powerSub.InPower && bpIslandManager.CoverEachOther(this, powerSub))
                {
                    powerSub.needPowerBparts.Add(this);
                    tempCount++;                    
                }
            }

            PowerCount = tempCount;
        }
        
        public void RemoveFromAllPowerCores()
        {
            foreach (var powerCore in bpIslandManager.powerCores)
            {
                powerCore.RemoveNeedPowerBpart(this);
            }

            foreach (var powerSub in bpIslandManager.powerSubs)
            {
                (powerSub as ISupportPowerBPart).RemoveNeedPowerBpart(this);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position,PowerSupportRange);
        }
    }
}