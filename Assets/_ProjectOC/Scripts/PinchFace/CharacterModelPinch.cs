using System.Collections;
using System.Collections.Generic;
using ProjectOC.PinchFace;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class CharacterModelPinch : MonoBehaviour
    {
        [SerializeField]
        List<Transform> boneTransf;
        [SerializeField] 
        List<GameObject> replaceGo;


        #region ������ʽ
        public void ChangeType(PinchPartType2 boneType2, int typeIndex)
        {
            
        }
        
        public void ChangeType(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            
        }

        void EquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //���� gameObejct Asset
            //װ��
        }

        void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //���Ƴ�Parent.Transform��Ȼ�� ɾ�� gameObject 
        }
        #endregion


        public void ChangeTexture(PinchPartType2 boneType2, int textureIndex)
        {
            //����Mat �� _MainTex ����
        }

        public void ChangeBoneScale(PinchPartType2 boneType2, Vector3 scaleValue)
        {
            //�ϰ���Ҫ����������������Ҫ��������
            if (boneType2 == PinchPartType2.Arm)
            {
                
            }
            else
            {
                //����������ֵ�����
                boneTransf[(int)boneType2 - (int)(PinchPartType2.Body)].localScale = scaleValue;    
            }
        }

        public void ChangeBoneScale(Transform boneTransf, Vector3 scaleValue)
        {
            boneTransf.localScale = scaleValue;
        }

        public void ChangeColor(PinchPartType2 boneType2, Color _color)
        {
            //��boneType�ҵ���Ӧ������ Mat������Mat��_Color ����
        }
        
        
    }
}