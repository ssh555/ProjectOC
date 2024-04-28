using System;
using System.Xml.Schema;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using ML.Engine.Extension;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;

namespace ProjectOC.LandMassExpand
{
    public class ElectAppliance : BuildingPart, INeedPowerBpart
    {
        private int powerCount = 0;
        [Header("�õ�������"),SerializeField,LabelText("��緶Χ")]
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
                int lastPowerCount = PowerCount;            
                powerCount = value;
                //count
                if ((lastPowerCount > 0 && powerCount == 0) || (lastPowerCount == 0 && powerCount > 0))
                {
                    PowerStateChange();
                }
            }
        }
        public bool InPower => PowerCount > 0;



        private BuildPowerIslandManager bpIslandManager;

        protected override void Awake()
        {
            base.Awake();
            bpIslandManager = LocalGameManager.Instance.BuildPowerIslandManager;
        }
        

        void OnDestroy()
        {
            if (bpIslandManager != null  && bpIslandManager.electAppliances.Contains(this))
            {
                bpIslandManager.electAppliances.Remove(this);
                RemoveFromAllPowerCores();
            }
        }
        
        //��һ���½�Ϊtrue������Ϊfalse
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //���û�У�˵���ս��������
            if (isFirstBuild)
            {
                bpIslandManager.electAppliances.Add(this);
            }
            //����
            RemoveFromAllPowerCores();
            //���¼���
            RecalculatePowerCount();
            base.OnChangePlaceEvent(oldPos,newPos);
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

        public virtual void PowerStateChange()
        {
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position,PowerSupportRange);
        }
    }
}