using System.Xml.Schema;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using ML.Engine.Extension;

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
                if(vfxTranf != null)
                    vfxTranf.SetActive(inPower);
            }
        }
        

        [SerializeField,LabelText("充电特效")]
        private GameObject vfxTranf;

        void OnDestroy()
        {
            if (BuildPowerIslandManager.Instance != null  && BuildPowerIslandManager.Instance.electAppliances.Contains(this))
            {
                BuildPowerIslandManager.Instance.electAppliances.Remove(this);
                RemoveFromAllPowerCores();
            }
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //如果没有，说明刚建造则加入
            if (!BuildPowerIslandManager.Instance.electAppliances.Contains(this))
            {
                BuildPowerIslandManager.Instance.electAppliances.Add(this);
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
            foreach (var powerCore in BuildPowerIslandManager.Instance.powerCores)
            {
                if (BuildPowerIslandManager.Instance.CoverEachOther(this, powerCore))
                {
                    powerCore.needPowerBparts.Add(this);
                    tempCount++;
                }
            }
            
            foreach (var powerSub in BuildPowerIslandManager.Instance.powerSubs)
            {
                if (powerSub.InPower && BuildPowerIslandManager.Instance.CoverEachOther(this, powerSub))
                {
                    powerSub.needPowerBparts.Add(this);
                    tempCount++;                    
                }
            }

            PowerCount = tempCount;
        }
        
        public void RemoveFromAllPowerCores()
        {
            foreach (var powerCore in BuildPowerIslandManager.Instance.powerCores)
            {
                powerCore.RemoveNeedPowerBpart(this);
            }

            foreach (var powerSub in BuildPowerIslandManager.Instance.powerSubs)
            {
                (powerSub as ISupportPowerBPart).RemoveNeedPowerBpart(this);
            }
        }
    #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(vfxTranf.transform.position,PowerSupportRange);
        }
    #endif
    }
}