using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.PinchFace.Config;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;


namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchFaceManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Init

        //引用
        public PinchFaceHelper pinchFaceHelper;
        public CharacterModelPinch ModelPinch;
        
        [ShowInInspector]
        public Dictionary<PinchPartType3, PinchPartType2> pinchPartType3Dic = new Dictionary<PinchPartType3, PinchPartType2>();
        public Dictionary<PinchPartType2, PinchPartType> pinchPartType2Dic = new Dictionary<PinchPartType2, PinchPartType>();
        //存储每一个的Type1下的每一个Type2
        [SerializeField]
        public List<List<PinchPartType2>> pinchPartType1Inclusion;  //

        // public List<PinchType1Struct> PinchType1Structs;
        private const string PinchPartTypePath = "PinchFace_TypePackage";
        private const string PinchPartPath = "PinchFacePart";

        public void OnRegister()
        {
            DataStructInit();
            RegisterPinchPartType();
            pinchFaceHelper = new PinchFaceHelper(this);
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<PinchDataConfig>(pinchDataConfigPath).Completed+=(handle) =>
            {
                Config = handle.Result;

                PlayerCharacter playerCharacter =
                    (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).currentCharacter;
                if (playerCharacter != null)
                {
                    CharacterModelPinch _modelPinch = playerCharacter.GetComponentInChildren<CharacterModelPinch>();
                    _modelPinch.ChangeType(PinchPartType3.HF_HairFront, 0);
                    _modelPinch.ChangeType(PinchPartType3.HD_Dai, 0);
                    _modelPinch.ChangeType(PinchPartType3.HB_HairBack, 0);
                }
            };

            //GeneratePinchRaceUI();
            // GenerateCustomRaceUI();
            // GeneratePinchFaceUI(); 
        }

        public void UnRegister()
        {
            
        }

        void DataStructInit()
        {
            //1-6,不需要存0
            pinchPartType1Inclusion = new List<List<PinchPartType2>>(Enum.GetValues(typeof(PinchPartType1)).Length-1);
            for (int i = 0; i < pinchPartType1Inclusion.Capacity; i++)
            {
                pinchPartType1Inclusion.Add(new List<PinchPartType2>());
            }
        }
        /// <summary>
        /// PinchPart相关字典加载
        /// </summary>
        public void RegisterPinchPartType()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<PinchPartType>(PinchPartTypePath, (ppt) =>
            {
                lock (pinchPartType3Dic)
                {
                    lock (pinchPartType2Dic)
                    {
                        pinchPartType2Dic.Add(ppt.pinchPartType2,ppt);
                        pinchPartType1Inclusion[(int)ppt.pinchPartType1 -1].Add(ppt.pinchPartType2);
                        foreach (var _ppt3 in ppt.pinchPartType3s)
                        {
                            pinchPartType3Dic.Add(_ppt3,ppt.pinchPartType2);
                        }
                    } 
                }
                
            });
        }
        
        #endregion

        
        
        #region 随机部件,随机种族部件
        private string pinchDataConfigPath = "PinchAsset_PinchFaceSetting/PinchDataConfig.asset";
        public PinchDataConfig Config;
        public List<PinchPartType3> RandomRacePart()
        {
            int minPinchType3 = 4;
            List<PinchPartType3> pinchPartType3s = new List<PinchPartType3>();
            //开抽
            while (pinchPartType3s.Count < minPinchType3)
            {
                foreach (var _pinchType2 in pinchPartType2Dic)
                {
                    //跳过普通
                    if(_pinchType2.Value.pinchPartType1 == PinchPartType1.Common)
                        continue;
                
                    float nake = 0.5f;
                    if (Random.Range(0f, 1f) > nake)
                    {
                        PinchPartType3 _type3 = _pinchType2.Value.pinchPartType3s[Random.Range(0, _pinchType2.Value.pinchPartType3s.Count)];
                        pinchPartType3s.Add(_type3);
                    }
                }
            }
   
            return pinchPartType3s;
        }

        public int RandomPinchPart(PinchPartType3 _type3,bool EquipModel,CharacterModelPinch _ModelPinch = null)
        {
            if (_ModelPinch == null)
            {
                _ModelPinch = this.ModelPinch;
            }
            
            if (_type3 == PinchPartType3.HF_HairFront)
            {
                RandomPinchPart(PinchPartType3.HB_HairBack, EquipModel,_ModelPinch);
                RandomPinchPart(PinchPartType3.HD_Dai, EquipModel,_ModelPinch);
            }
            
            int typePrefabCount = Config.typesDatas[(int)_type3 - 1].typeCount;
            if (typePrefabCount != 0)
            {
                int _typeIndex = Random.Range(0, typePrefabCount);
                if (EquipModel)
                {
                    _ModelPinch.ChangeType(_type3, _typeIndex);
                }

                return _typeIndex;
            }

            return -1;
        }

        #endregion
        #region Temp

        private const string PinchRaceUIPath = "Prefabs_PinchPart/UIPanel/Panel/Prefab_FacePinch_RacePanel.prefab";
        private const string CustomRacePath = "Prefabs_PinchPart/UIPanel/Panel/Prefab_FacePinch_RacePartPanel.prefab";
        private const string PinchFacePath = "Prefabs_PinchPart/UIPanel/Panel/Prefab_FacePinch_FacePanel.prefab";
        [HideInInspector]
        public string playerModelPrefabPath = "Prefabs_PinchPart/PinchPart/Prefab_PinchModel.prefab";
        [SerializeField]
        public List<RacePinchData> RacePinchDatas;
        
        public void GeneratePinchRaceUI()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(PinchRaceUIPath)
                .Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<UIPinchRacePanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            };
        }
        public void GenerateCustomRaceUI()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(CustomRacePath)
                .Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<UICustomRacePanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            };
        }

        public void GeneratePinchFaceUI()
        {
            GeneratePinchFaceUI(RacePinchDatas[0]); 
        }
        public void GeneratePinchFaceUI(RacePinchData _raceData)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(PinchFacePath)
                .Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<UIPinchFacePanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);

                panel.InitRaceData(_raceData);
            };
        }
        
        #endregion
    }
}
