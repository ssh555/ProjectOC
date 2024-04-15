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
using static ML.Engine.UI.UIBtnListInitor;

namespace ML.Engine.UI
{
    public class UIBtnListContainerInitor : UIBehaviour
    {
        [Serializable]
        public struct BtnListContainerInitData
        {
            [LabelText("A,B�����񵼺���ʽ")]
            public ContainerType containerType;

            // ��ʾ������ϵ��ֶ�
            [LabelText("��������"), ShowIf("ShowLinkDatas")]
            public List<LinkData> LinkDatas;

            [LabelText("BtnList֮��ĵ����Ƿ�ѭ��"), ShowIf("ShowLoopOption")]
            public bool isLoop;

            [LabelText("BtnList֮���Ƿ�Ϊ�������� Ĭ��Ϊ��������"), ShowIf("ShowHorizontalOption")]
            public bool isHorizontal;

            public static BtnListContainerInitData defaultTemplate = new BtnListContainerInitData()
            {
                containerType = ContainerType.A,
                isLoop = false,
                isHorizontal = false
            };

            // ���� containerType ��ʾ LinkDatas �ֶ�
            private bool ShowLinkDatas()
            {
                return containerType == ContainerType.B;
            }

            // ���� containerType ��ʾ isLoop �ֶ�
            private bool ShowLoopOption()
            {
                return containerType == ContainerType.A;
            }

            // ���� containerType ��ʾ isHorizontal �ֶ�
            private bool ShowHorizontalOption()
            {
                return containerType == ContainerType.A;
            }
        }

        public enum EdgeType
        {
            ���˳ʱ�� = 0,
            �����ʱ��,
            �Ҳ�˳ʱ��,
            �Ҳ���ʱ��,
            �ϲ�˳ʱ��,
            �ϲ���ʱ��,
            �²�˳ʱ��,
            �²���ʱ��
        }

        public enum LinkType
        {
            �������� = 0,
            ��������
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

        [LabelText("���������"), SerializeField]
        public BtnListContainerInitData btnListContainerInitData = BtnListContainerInitData.defaultTemplate;

        protected override void OnValidate()
        {
            // ���� containerType �޸������ֶε�ֵ��״̬
            if (btnListContainerInitData.containerType == ContainerType.A)
            {
                // ��� containerType ѡ���� A������� LinkDatas
                btnListContainerInitData.LinkDatas = new List<LinkData>();

                // ���� LinkDatas �ֶ�
                UnityEditor.EditorUtility.SetDirty(this); // �� Unity Editor ����Ҫ���Ϊ����ʹ������Ч
            }
            else
            {
                // ��� containerType ѡ���� B���� isLoop �� isHorizontal ��ΪĬ��ֵ
                btnListContainerInitData.isLoop = BtnListContainerInitData.defaultTemplate.isLoop;
                btnListContainerInitData.isHorizontal = BtnListContainerInitData.defaultTemplate.isHorizontal;
            }
        }
    }
}


