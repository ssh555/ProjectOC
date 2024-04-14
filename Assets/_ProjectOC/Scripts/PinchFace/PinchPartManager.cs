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
        
        //���Ǹ���PinchPartType3 ȥ�Ҷ�Ӧ���ļ���
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
            // �� value �� fromMin �� fromMax ��Χӳ�䵽 toMin �� toMax ��Χ
            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }

        #region �������ݽṹ��


        #endregion
    }
}
