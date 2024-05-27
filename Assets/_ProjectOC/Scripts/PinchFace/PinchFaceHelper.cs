using System.Collections;
using System.Collections.Generic;
using ML.Engine.UI;
using UnityEngine;
using UnityEngine.UI;


namespace ProjectOC.PinchFace
{
    //存放一些常用的工具，例如路径查找；Layout刷新
    public class PinchFaceHelper
    {
        private PinchFaceManager pinchFaceManager;
        public PinchFaceHelper(PinchFaceManager _pinchFaceManager)
        {
            pinchFaceManager = _pinchFaceManager;
        }
        
        public void RefreshPanelLayout(Transform _transf)
        {
            LayoutGroup[] layoutGroups = _transf.GetComponentsInChildren<LayoutGroup>();
            foreach (var group in layoutGroups)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.transform as RectTransform);
            }
        }

        public float RemapValue(float value, Vector2 input, Vector2 output)
        {
            return RemapValue(value, input.x, input.y, output.x, output.y);
        }
        public float RemapValue(float value, float fromMin, float fromMax, float toMin = 0f, float toMax = 1f)
        {
            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }

        
        public void SortUIAfterGenerate(Transform _transf,Transform _parent,UIPinchFacePanel _pinchFacePanel,int returnBtnListCount = 4)
        {
            //Debug.LogWarning("SortUI AfterGenerate");
            //右侧的各种捏脸组件
            Transform[] childTransforms = new Transform[_parent.childCount];
            for (int i = 0; i < _parent.childCount; i++)
            {
                childTransforms[i] = _parent.GetChild(i);
            }
            //排序
            System.Array.Sort(childTransforms,(x,y)=>
                int.Parse(x.name).CompareTo(int.Parse(y.name)));
            // bigMap.SmallMapEditDatas.Sort((a, b) =>
            //     (int.Parse(a.name.Split("_")[1]).CompareTo(int.Parse(b.name.Split("_")[1]))));
            
            List<UIBtnListInitor> btnListInitors = new List<UIBtnListInitor>();
            for (int i = 0; i < childTransforms.Length; i++)
            {
                if (!childTransforms[i].gameObject.activeSelf)
                {
                    continue;
                }
                childTransforms[i].transform.SetSiblingIndex(i);
                UIBtnListInitor[] btnLists = childTransforms[i].transform.GetComponentsInChildren<UIBtnListInitor>();
                btnListInitors.AddRange(btnLists);
            }
            
            _pinchFacePanel.ReGenerateBtnListContainer(btnListInitors);
            Transform _pinchFacePanelTransf = _parent.GetComponentInParent<UIPinchFacePanel>().transform;
            RefreshPanelLayout(_pinchFacePanelTransf);
            
            //大概率第一个，也可能 更新颜色面板模式， 返回颜色面板
            _pinchFacePanel.ReturnBtnList(returnBtnListCount);
        }
        
        //5_Common_PinchType1/20_FaceDress_PinchType2/45_FD_FaceDress_PinchType3
        public string GetType2Path(PinchPartType2 _type2)
        {
            //根据Type寻找对应文件，根据上面的Component生成Prefab
            PinchPartType _type = pinchFaceManager.pinchPartType2Dic[_type2];
            PinchPartType1 _type1 = _type.pinchPartType1;
            //OC/Character/PinchFace/Prefabs/
            //5_Common_PinchType1/20_FaceDress_PinchType2/45_FD_FaceDress_PinchType3
            string typeFold1 = $"{(int)_type1-1}_{_type1.ToString()}_PinchType1";
            string typeFold2 = $"{(int)_type2-1}_{_type2.ToString()}_PinchType2";
            string _path = $"{typeFold1}/{typeFold2}";
            return _path;
        }

        public string GetType3PrefabPath(PinchPartType3 _type3)
        {
            PinchPartType2 _type2 = pinchFaceManager.pinchPartType3Dic[_type3];
            string _type2Path = GetType2Path(_type2);
            string _type3Path = $"{(int)_type3-1}_{_type3.ToString()}_PinchType3";
            return $"{_type2Path}/{_type3Path}";
        }
    }
}


