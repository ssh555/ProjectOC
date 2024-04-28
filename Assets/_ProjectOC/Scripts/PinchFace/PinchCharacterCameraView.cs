using System.Collections;
using System.Collections.Generic;
using ML.Engine.UI;
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
        }

        [System.Serializable]
        public struct ViewPointStruct
        {
            public ViewPointEnum viewPointEnum;
            public float distance;
            public Transform lookAtTransform;
        }
        
        public List<ViewPointStruct> viewPoints;
        private PinchFaceManager pinchFaceManager;

        public void Init(PinchFaceManager _pinchFaceManager)
        {
            pinchFaceManager = _pinchFaceManager;
            // for (int i = 0; i < viewPoints.Count; i++)
            // {
            //     viewPoints[i].lookAtTransform = viewTrans.GetChild(i);
            // }
        }
        
        public void CameraLookAtSwitch(UICameraImage _uiCameraImage, PinchPartType2 _type2)
        {
            ViewPointEnum _viewPointEnum = pinchFaceManager.pinchPartType2Dic[_type2].ViewPointEnum;
            ViewPointStruct _viewPointStruct = viewPoints[(int)_viewPointEnum];
            Debug.Log(_viewPointStruct.lookAtTransform.gameObject.name);
            _uiCameraImage.LookAtGameObject(_viewPointStruct.lookAtTransform.gameObject);
            _uiCameraImage.distanceFromObject = _viewPointStruct.distance;
        }
    }   
}
