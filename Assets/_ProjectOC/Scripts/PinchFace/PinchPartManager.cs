using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchFaceManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [ShowInInspector]
        public Dictionary<PinchPartType3, PinchPartType> pinchPartTypeDic;

        private const string PinchPartTypePath = "PinchFaceType";
        private const string PinchPartPath = "PinchFacePart";
        private AsyncOperationHandle PinchPartHandle;
        private AsyncOperationHandle PinchPartTypeHandle;

        public void OnRegister()
        {
            RegisterPinchPartType();
        }

        public void UnRegister()
        {
            
        }

        public void RegisterPinchPartType()
        {
            pinchPartTypeDic = new Dictionary<PinchPartType3, PinchPartType>();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<PinchPartType>(PinchPartTypePath, (ppt) =>
            {
                lock (pinchPartTypeDic)
                {
                    pinchPartTypeDic.Add(ppt.pinchPartType3,ppt);
                }
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
        
        public static float RemapValue(float value, float fromMin, float fromMax, float toMin = 0f, float toMax = 1f)
        {
            // 将 value 从 fromMin 到 fromMax 范围映射到 toMin 到 toMax 范围
            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }

        #region 捏脸数据结构体


        #endregion
    }
}
