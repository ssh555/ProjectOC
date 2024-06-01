using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ML.Engine.UI;
using UnityEngine;
using ML.MathExtension;
using ProjectOC.ManagerNS;

namespace ProjectOC.PinchFace
{
    public class PinchJiao : PinchTransfType
    {
        BezierComponent bezier;
        Transform jiaoTransfL, jiaoTransfR;
        private PinchFaceHelper PinchFaceHelper;
        public PinchJiao(BezierComponent _bezier,GameObject _go, int _index,bool inCamera = false) : base(_index)
        {
            PinchFaceHelper = LocalGameManager.Instance.PinchFaceManager.pinchFaceHelper;
            
            transformType = TransformType.Bezier;
            bezier = _bezier;
            //暂时还没有单双耳的需求
            jiaoTransfL = _go.transform;
            jiaoTransfL.SetParent(bezier.transform);
            _go.name = "TopEar_L";
            
            jiaoTransfR = GameObject.Instantiate(_go,bezier.transform).transform;
            jiaoTransfR.name = "TopEar_R";
            ModifyValue(Vector2.right*0.5f);
            jiaoTransfL.localScale = new Vector3(-1, 1, 1);
            if (inCamera)
            {
                UICameraImage.ModeGameObjectLayer(jiaoTransfL);
                UICameraImage.ModeGameObjectLayer(jiaoTransfR);
            }
        }

        public override void ModifyValue(Vector2 _value)
        {
            //(0-1)->(0.6,1)
            base.ModifyValue(_value);
            float _valueX = _value.x/2f;
            //value 0--0.8  最底到最顶
            //修改位置
            Vector3 _pos = bezier.Evaluate(_valueX);
            jiaoTransfL.position = _pos;
            _pos = jiaoTransfL.localPosition;
            _pos.x = -_pos.x;
            jiaoTransfR.localPosition = _pos;
            //修改旋转
            float startRotate = 0f, endRotate = 30f;
            _valueX =  PinchFaceHelper.RemapValue(_valueX, 0f, 0.8f);
            float resRotateX = Mathf.Lerp(startRotate, endRotate, _valueX);
            Vector3 resRotate = new Vector3(resRotateX, 0, 0);
            jiaoTransfL.localRotation = Quaternion.Euler(resRotate);
            jiaoTransfR.localRotation = Quaternion.Euler(resRotate);
        }

        public override void Release()
        {
            base.Release();
            if (jiaoTransfL != null)
            {
                Object.Destroy(jiaoTransfL.gameObject);
                Object.Destroy(jiaoTransfR.gameObject);
            }
        }
        
    }
}