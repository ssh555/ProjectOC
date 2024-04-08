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


        #region 更换样式
        public void ChangeType(PinchPartType2 boneType2, int typeIndex)
        {
            
        }
        
        public void ChangeType(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            
        }

        void EquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //生成 gameObejct Asset
            //装备
        }

        void UnEquipItem(PinchPartType2 boneType2, GameObject _PinchGo)
        {
            //先移出Parent.Transform，然后 删除 gameObject 
        }
        #endregion


        public void ChangeTexture(PinchPartType2 boneType2, int textureIndex)
        {
            //更换Mat 的 _MainTex 参数
        }

        public void ChangeBoneScale(PinchPartType2 boneType2, Vector3 scaleValue)
        {
            //上半身要缩放两跟骨骼，需要单独处理
            if (boneType2 == PinchPartType2.Arm)
            {
                
            }
            else
            {
                //身体骨骼部分的排序
                boneTransf[(int)boneType2 - (int)(PinchPartType2.Body)].localScale = scaleValue;    
            }
        }

        public void ChangeBoneScale(Transform boneTransf, Vector3 scaleValue)
        {
            boneTransf.localScale = scaleValue;
        }

        public void ChangeColor(PinchPartType2 boneType2, Color _color)
        {
            //从boneType找到对应的两个 Mat，更改Mat的_Color 属性
        }
        
        
    }
}