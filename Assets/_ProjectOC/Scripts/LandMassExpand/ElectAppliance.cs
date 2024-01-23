using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

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
                powerCount = value;
                
                bool inPower = powerCount > 0;
                if(vfxTranf != null)
                    vfxTranf.SetActive(inPower);
            }
        }
        

        [SerializeField,LabelText("�����Ч")]
        private GameObject vfxTranf;

        void OnDestroy()
        {
            if (BuildPowerIslandManager.Instance != null  && BuildPowerIslandManager.Instance.electAppliances.Contains(this))
            {
                BuildPowerIslandManager.Instance.electAppliances.Remove(this);
            }
        }

        public void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //���û�У�˵���ս��������
            if (!BuildPowerIslandManager.Instance.electAppliances.Contains(this))
            {
                BuildPowerIslandManager.Instance.electAppliances.Add(this);
            }
            RecalculatePowerCount();
        }

        private void RecalculatePowerCount()
        {
            int tempCount = 0;
            foreach (var powerCore in BuildPowerIslandManager.Instance.powerCores)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(this,powerCore))
                    tempCount++;
            }
            
            foreach (var powerSub in BuildPowerIslandManager.Instance.powerSubs)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(this,powerSub))
                    tempCount++;                    
            }

            PowerCount = tempCount;
        }


    }
}