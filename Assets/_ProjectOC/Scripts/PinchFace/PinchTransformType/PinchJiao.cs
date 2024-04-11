using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class PinchJiao : PinchTransfType
    {
        BezierComponent bezier;
        Transform jiaoTransfL, jiaoTransfR;
        public PinchJiao(BezierComponent _bezier,GameObject _go, int _index) : base(_index)
        {
            transformType = TransformType.Bezier;
            bezier = _bezier;
            //暂时还没有单双耳的需求
            jiaoTransfL = GameObject.Instantiate(_go,bezier.transform).transform;
            jiaoTransfR = GameObject.Instantiate(_go,bezier.transform).transform;
            jiaoTransfL.localScale = new Vector3(-1, 1, 1);
        }

        public override void ModifyValue(Vector2 _value)
        {
            //(0-1)->(0.6,1)
            base.ModifyValue(_value);
            //value 0--0.8  最底到最顶
            //修改位置
            Vector3 _pos = bezier.Evaluate(_value.x);
            jiaoTransfL.position = _pos;
            _pos = jiaoTransfL.localPosition;
            _pos.x = -_pos.x;
            jiaoTransfR.localPosition = _pos;
            //修改旋转
            float startRotate = 0f, endRotate = 30f;
            _value.x = PinchPartDataManager.RemapValue(_value.x, 0f, 0.8f);
            float resRotateX = Mathf.Lerp(startRotate, endRotate, _value.x);
            Vector3 resRotate = new Vector3(resRotateX, 0, 0);
            jiaoTransfL.localRotation = Quaternion.Euler(resRotate);
            jiaoTransfR.localRotation = Quaternion.Euler(resRotate);
        }

        public override void Release()
        {
            base.Release();
            Object.Destroy(jiaoTransfL.gameObject);
            Object.Destroy(jiaoTransfR.gameObject);
        }
        
    }
}