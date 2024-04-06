using ML.Engine.Manager;
using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

namespace ML.Engine.UI
{
    public class UIBtnListContainerInitor : UIBehaviour
    {
        [Serializable]
        public struct BtnListContainerInitData
        {
            [LabelText("A,B类网格导航范式")]
            public ContainerType containerType;
            [LabelText("连接数据")]
            public List<LinkData> LinkDatas;
            [LabelText("BtnList导航  下标对应Btnlist")]
            public List<NavagationStruct> navagations;
        }

        public enum EdgeType
        {
            LP = 0,
            LN,
            RP,
            RN,
            UP,
            UN,
            DP,
            DN
        }


        public enum LinkType
        {
            LTR = 0,
            UTD
        }

        [Serializable]
        public struct LinkData
        {
            [LabelText("连接双方两个BtnList的下标")]
            public int btnlist1, btnlist2;
            [LabelText("连接双方两条边的类型")]
            public EdgeType btnlist1type, btnlist2type; 
            [LabelText("该连接的类型")]
            public LinkType linktype;
        }

        public enum ContainerType
        {
            A = 0,
            B
        }

        [Serializable]
        [LabelText("拖拽配置")]
        public struct NavagationStruct
        {
            [LabelText("上")]
            public UIBtnListInitor Up;
            [LabelText("下")]
            public UIBtnListInitor Down;
            [LabelText("左")]
            public UIBtnListInitor Left;
            [LabelText("右")]
            public UIBtnListInitor Right;
        }

        [LabelText("面板配置项"), SerializeField]
        public BtnListContainerInitData btnListContainerInitData;
    }
}


