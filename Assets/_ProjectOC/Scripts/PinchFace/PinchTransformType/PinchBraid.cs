using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class PinchBraid : PinchTransfType
    {
        float radius = -0.22f;
        public Vector3 offset = new Vector3(0f,0.15f,0.04f);
        public Transform headBone;
        public GameObject braidGo;
        
        public PinchBraid(Transform _headBone,GameObject _braidGo,int _index,Vector2 _curPos) : base(_index)
        {
            headBone = _headBone;
            transformType = TransformType.Sphere;
            braidGo = _braidGo;
            braidGo.transform.SetParent(_headBone);
            ModifyValue(_curPos);
        }
        
        public override void ModifyValue(Vector2 _value)
        {
            //(0-1)->(0.6,1)
            base.ModifyValue(_value);
            Vector3 center = CalculateCenter(headBone,offset);
            Vector3 point = CalculatePointOnSphere(center, radius, _value);
            Vector3 eulerAngles = CalculateEulerAngles(center - point);
            braidGo.transform.position = point;
            braidGo.transform.localRotation = Quaternion.Euler(eulerAngles);
        }
        public Vector3 CalculateCenter(Transform originCenter, Vector3 offset)
        {
            if (originCenter == null)
                return Vector3.zero;
            
            // ��ȡԭʼ���ĵ�����λ�á���ת������
            Vector3 worldPosition = originCenter.position;
            Quaternion worldRotation = originCenter.rotation;
            Vector3 worldScale = originCenter.lossyScale;

            // ���������ԭʼ����ƫ����������λ��
            Vector3 scaledOffset = Vector3.Scale(offset, worldScale); // ��������
            Vector3 rotatedOffset = worldRotation * scaledOffset; // ������ת
            Vector3 finalCenter = worldPosition + rotatedOffset;

            return finalCenter;
        }
        Vector3 CalculatePointOnSphere(Vector3 center, float radius, Vector2 rot)
        {
            // ���Ƕ�ת��Ϊ����
            float rotXRad = rot.x * Mathf.Deg2Rad;
            float rotYRad = rot.y * Mathf.Deg2Rad;

            // ���������ϵĵ�
            float x = center.x + radius * Mathf.Sin(rotXRad) * Mathf.Cos(rotYRad);
            float y = center.y + radius * Mathf.Sin(rotXRad) * Mathf.Sin(rotYRad);
            float z = center.z + radius * Mathf.Cos(rotXRad);

            return new Vector3(x, y, z);
        }
        Vector3 CalculateEulerAngles(Vector3 normal)
        {
            // ���㷨�߷����ŷ����
            Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
            return rotation.eulerAngles;
        }
        
        
        public override void Release()
        {
            base.Release();
            if (braidGo != null)
            {
                //��������������������Ҫж�ع���
                Object.Destroy(braidGo.gameObject);
            }
        }

        
        
    }
}