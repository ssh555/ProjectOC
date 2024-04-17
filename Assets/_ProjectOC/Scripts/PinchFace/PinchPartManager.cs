using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchFaceManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [ShowInInspector]
        //public Dictionary<PinchPartType3, PinchPartType> pinchPartType3Dic;
        public Dictionary<PinchPartType2, PinchPartType> pinchPartType2Dic;
        //存储每一个的Type1下的每一个Type2
        [SerializeField]
        public List<List<PinchPartType2>> pinchPartType1Inclusion;  //

        // public List<PinchType1Struct> PinchType1Structs;
        private const string PinchPartTypePath = "PinchFaceType";
        private const string PinchPartPath = "PinchFacePart";
        private AsyncOperationHandle PinchPartHandle;
        private AsyncOperationHandle PinchPartTypeHandle;

        public void OnRegister()
        {
            DataStructInit();
            RegisterPinchPartType();
            // GeneratePinchRaceUI();
            GenerateCustomRaceUI();
        }

        public void UnRegister()
        {
            
        }

        void DataStructInit()
        {
            // PinchType1Structs = new List<PinchType1Struct>(Enum.GetValues(typeof(PinchPartType1)).Length);
            // for (int i = 0; i < PinchType1Structs.Capacity; i++)
            // {
            //     PinchType1Structs[0].Type1 = PinchPartType1.None; 
            //     PinchType1Structs.Add(new PinchType1Struct());
            // }
            
            
            
            
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
            // pinchPartType3Dic = new Dictionary<PinchPartType3, PinchPartType>();
            pinchPartType2Dic = new Dictionary<PinchPartType2, PinchPartType>();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<PinchPartType>(PinchPartTypePath, (ppt) =>
            {
                // lock (pinchPartType3Dic)
                // {
                //     pinchPartType3Dic.Add(ppt.pinchPartType3,ppt);
                // }
                lock (pinchPartType2Dic)
                {
                    //有效的Capacity
                    if (ppt.pinchPartType3s.Capacity != 0)
                    {
                        pinchPartType2Dic.Add(ppt.pinchPartType2,ppt);
                        pinchPartType1Inclusion[(int)ppt.pinchPartType1 -1].Add(ppt.pinchPartType2);
                    }
                }
                // lock (pinchPartType2Dic)
                // {
                //     //如果第一次，初始化Struct
                //     if (!pinchPartType2Dic.ContainsKey(ppt.pinchPartType2))
                //     {
                //         pinchPartType2Dic[ppt.pinchPartType2] = ppt.pinchPartType1;
                //
                //         // PinchType1Structs[(int)ppt.pinchPartType1].Type1].PinchType1Structs = new List<PinchType1Struct>();
                //     }
                // }
            }).Completed+= (handle) =>
            {
                PinchPartTypeHandle = handle;
            };
        }
        
        //考虑根据PinchPartType3 去找对应的文件夹
        // public void RegisterPinchPartPrefab()
        // {
        //     ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<GameObject>(PinchPartPath, (obj) =>
        //     {
        //         lock (pinchPartType)
        //         {
        //             
        //         }
        //     }).Completed+= (handle) =>
        //     {
        //         PinchPartHandle = handle;
        //     };
        // }
        /// <summary>
        /// 将 value 从 fromMin 到 fromMax 范围映射到 toMin 到 toMax 范围
        /// </summary>
        public static float RemapValue(float value, float fromMin, float fromMax, float toMin = 0f, float toMax = 1f)
        {

            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }

        #region 捏脸数据结构体
        //可优化为结构体，主要是结构体初始化修改太麻烦了
        [System.Serializable]
        public class PinchType1Struct
        {
            public PinchPartType1 Type1;
            public List<PinchPartType2> Type2s;
        }
        [System.Serializable]
        public class PinchType2Struct
        {
            public PinchPartType2 Type2;
            public List<PinchPartType3> Type3s;
        }
        
        #endregion

        #region Temp

        private const string PinchRaceUIPath = "OC/UIPanel/FacePinch_RacePanel.prefab";
        private const string CustomRacePath = "OC/UIPanel/FacePinch_RacePartPanel.prefab";
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
                Debug.LogWarning(panel.name);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                
            };
        }

        #endregion
    }
}
