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
            [LabelText("A,B�����񵼺���ʽ")]
            public ContainerType containerType;
            [LabelText("��������")]
            public List<LinkData> LinkDatas;
            [LabelText("BtnList����  �±��ӦBtnlist")]
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
            [LabelText("����˫������BtnList���±�")]
            public int btnlist1, btnlist2;
            [LabelText("����˫�������ߵ�����")]
            public EdgeType btnlist1type, btnlist2type; 
            [LabelText("�����ӵ�����")]
            public LinkType linktype;
        }

        public enum ContainerType
        {
            A = 0,
            B
        }

        [Serializable]
        [LabelText("��ק����")]
        public struct NavagationStruct
        {
            [LabelText("��")]
            public UIBtnListInitor Up;
            [LabelText("��")]
            public UIBtnListInitor Down;
            [LabelText("��")]
            public UIBtnListInitor Left;
            [LabelText("��")]
            public UIBtnListInitor Right;
        }

        [LabelText("���������"), SerializeField]
        public BtnListContainerInitData btnListContainerInitData;
    }
}


