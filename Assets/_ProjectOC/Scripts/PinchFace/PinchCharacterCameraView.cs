using System.Collections;
using System.Collections.Generic;
using ML.Engine.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchCharacterCameraView
    {
        public enum ViewPointEnum
        {
            Total,
            Head,
            Face,
            Leg,
            Arm,
            Side
        }

        [System.Serializable]
        public struct ViewPointStruct
        {
            [LabelText("机位类型")]
            public ViewPointEnum viewPointEnum;
            [LabelText("目标位置")]
            public Transform lookAtTransform;
            [LabelText("相机位置")]
            public Transform cameraTransform;
        }
        
        public List<ViewPointStruct> viewPoints;
        private PinchFaceManager pinchFaceManager;

        public void Init(PinchFaceManager _pinchFaceManager)
        {
            pinchFaceManager = _pinchFaceManager;
            // Transform viewTrans = _pinchFaceManager.ModelPinch.CameraView.;
            // for (int i = 0; i < viewPoints.Count; i++)
            // {
            //     viewPoints[i].lookAtTransform = viewTrans.GetChild(i);
            // }
        }
        
        public void CameraLookAtSwitch(UICameraImage _uiCameraImage, PinchPartType2 _type2)
        {
            ViewPointEnum _viewPointEnum = pinchFaceManager.pinchPartType2Dic[_type2].ViewPointEnum;
            ViewPointStruct _viewPointStruct = viewPoints[(int)_viewPointEnum];
            _uiCameraImage.LookAtGameObjectMultCamera(_viewPointStruct.lookAtTransform.gameObject,_viewPointStruct.cameraTransform);
            //_uiCameraImage.distanceFromObject = _viewPointStruct.distance;
        }
    }   
}
