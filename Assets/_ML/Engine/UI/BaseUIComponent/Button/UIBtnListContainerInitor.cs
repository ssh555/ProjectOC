using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

            [LabelText("��������")]
            public ScrollRect scrollRect;
            [LabelText("��̬��ʼ��UIBtnList")]
            public List<UIBtnListInitor> UIBtnListInitors;

            public static BtnListContainerInitData defaultTemplate = new BtnListContainerInitData()
            {
                containerType = ContainerType.A,
                isLoop = false,
                isHorizontal = false,
               
            };

            public void AddLinkData(LinkData linkData)
            {
                this.LinkDatas.Add(linkData);
            }

            /// <summary>
            /// List.RemoveRange()
            /// </summary>
            /// <param name="_index"></param>
            /// <param name="_count"></param>
            public void RemoveLinkData(int _index,int _count)
            {
                this.LinkDatas.RemoveRange(_index,_count);
            }
            
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
        public class LinkData
        {
            [LabelText("����˫������BtnList���±�")]
            public int btnlist1, btnlist2;

            [LabelText("����˫�������ߵ�����")]
            public EdgeType btnlist1type, btnlist2type;

            [LabelText("�����ӵ�����")]
            public LinkType linktype;

            public LinkData()
            {
                
            }
            public LinkData(int btnlist1, int btnlist2, EdgeType btnlist1type, EdgeType btnlist2type, LinkType linktype)
            {
                this.btnlist1 = btnlist1;
                this.btnlist2 = btnlist2;

                this.btnlist1type = btnlist1type;
                this.btnlist2type = btnlist2type;
                this.linktype = linktype;
            }

            public static LinkData Null = new LinkData();
        }

        public enum ContainerType
        {
            A = 0,
            B
        }

        [LabelText("���������"), SerializeField]
        public BtnListContainerInitData btnListContainerInitData = BtnListContainerInitData.defaultTemplate;
#if UNITY_EDITOR
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
#endif
    }
}


