using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    //������BuildingPart������OnChangePlaceEvent�����ú�����õ�����

    public class BuildPowerSub : BuildingPart, ISupportPowerBPart, IComposition
    {
        [Header("���粿��"),SerializeField,LabelText("���緶Χ")]
        private float powerSupportRange = 5;
        public float PowerSupportRange
        {
            get => powerSupportRange;
            set
            {
                powerSupportRange = value;
                float localScale = value * 0.2f;
                powerSupportVFX.transform.localScale = new Vector3(localScale,1,localScale);
            }
        }
        
        [SerializeField,LabelText("���緶Χ��Ч")]
        private GameObject powerSupportVFX;
        [SerializeField]
        private Material powerVFXMat;

        #region IPowerBPart

        private int powerCount;
        public int PowerCount 
        { 
            get=>powerCount;
            set => powerCount = value;
        }
  
        private bool inPower = false;
        [ShowInInspector]
        public bool InPower
        {
            get=>this.inPower;
            set
            {
                if (value != inPower)
                {
                    inPower = value;
                    //������ɫ
                    Color32 vfxColor = (value ? new Color32(255, 178,126,255): new Color32(139, 167,236,255));
                    powerVFXMat.SetColor("_VFXColor",vfxColor);

                    //��λ�ò��������£����㸽���õ���powercount�����
                    CalculatePowerCount(transform.position,InPower);
                }
            }
        }

        #endregion

        private new void Awake()
        {
            powerVFXMat = powerSupportVFX.GetComponent<Renderer>().material;
            powerSupportVFX.GetComponent<Renderer>().sharedMaterial = powerVFXMat;
            powerVFXMat.SetColor("_VFXColor",new Color32(139, 167,236,255));
            
            base.Awake();
        }

        void OnDestroy()
        {
            if (BuildPowerIslandManager.Instance != null && BuildPowerIslandManager.Instance.powerSubs.Contains(this))
            {
                InPower = false;
                BuildPowerIslandManager.Instance.powerSubs.Remove(this);
            }
        }

        
        public new void OnChangePlaceEvent(Vector3 oldPos,Vector3 newPos)
        {
            //���û�У�˵���ս��������
            if (!BuildPowerIslandManager.Instance.powerSubs.Contains(this))
            {
                BuildPowerIslandManager.Instance.powerSubs.Add(this);
            }
            //ԭ�ط�
            else if(oldPos == newPos)
            {
                return;
            }
            

            //˵�����ƶ������ǽ���
            if (InPower)
            {
                inPower = false;
                Color32 vfxColor = new Color32(139, 167,236,255);
                powerVFXMat.SetColor("_VFXColor",vfxColor);
                //ԭ����λ��powerCount-1
                CalculatePowerCount(oldPos,false);
            }

            foreach (var powerCore in BuildPowerIslandManager.Instance.powerCores)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(powerCore,this))
                {
                    InPower = true;
                }
            }
        }

        private void CalculatePowerCount(Vector3 pos,bool isAdd)
        {
            int add;
            if (isAdd) 
                add = 1;
            else 
                add = -1;
            
            foreach (var electAppliance in BuildPowerIslandManager.Instance.electAppliances)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(electAppliance,PowerSupportRange,pos))
                    electAppliance.PowerCount += add;
            }
        }
        
        public bool CoverEachOther(ElectAppliance electAppliance, Vector3 powerSubPos)
        {

            return CoverEachOther(electAppliance.PowerSupportRange, PowerSupportRange, 
                electAppliance.transform.position, powerSubPos);
        }
        
        bool CoverEachOther(float r1, float r2, Vector3 pos1, Vector3 pos2)
        {
            float distanceSquared = (pos1 - pos2).sqrMagnitude;
            float radiusSunSquared = (r1 + r2) * (r1 + r2);
            return radiusSunSquared >= distanceSquared;
        }
        

    }
    
}