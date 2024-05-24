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
            [LabelText("A,B类网格导航范式")]
            public ContainerType containerType;

            // 显示在面板上的字段
            [LabelText("连接数据"), ShowIf("ShowLinkDatas")]
            public List<LinkData> LinkDatas;

            [LabelText("BtnList之间的导航是否循环"), ShowIf("ShowLoopOption")]
            public bool isLoop;

            [LabelText("BtnList之间是否为横向排列 默认为竖向排列"), ShowIf("ShowHorizontalOption")]
            public bool isHorizontal;

            [LabelText("滑动窗口")]
            public ScrollRect scrollRect;
            [LabelText("静态初始化UIBtnList")]
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
            
            // 根据 containerType 显示 LinkDatas 字段
            private bool ShowLinkDatas()
            {
                return containerType == ContainerType.B;
            }

            // 根据 containerType 显示 isLoop 字段
            private bool ShowLoopOption()
            {
                return containerType == ContainerType.A;
            }

            // 根据 containerType 显示 isHorizontal 字段
            private bool ShowHorizontalOption()
            {
                return containerType == ContainerType.A;
            }
        }

        public enum EdgeType
        {
            左侧顺时针 = 0,
            左侧逆时针,
            右侧顺时针,
            右侧逆时针,
            上侧顺时针,
            上侧逆时针,
            下侧顺时针,
            下侧逆时针
        }

        public enum LinkType
        {
            左右相连 = 0,
            上下相连
        }

        [Serializable]
        public class LinkData
        {
            [LabelText("连接双方两个BtnList的下标")]
            public int btnlist1, btnlist2;

            [LabelText("连接双方两条边的类型")]
            public EdgeType btnlist1type, btnlist2type;

            [LabelText("该连接的类型")]
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

        [LabelText("面板配置项"), SerializeField]
        public BtnListContainerInitData btnListContainerInitData = BtnListContainerInitData.defaultTemplate;
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            // 根据 containerType 修改其他字段的值或状态
            if (btnListContainerInitData.containerType == ContainerType.A)
            {
                // 如果 containerType 选择了 A，则清空 LinkDatas
                btnListContainerInitData.LinkDatas = new List<LinkData>();

                // 隐藏 LinkDatas 字段
                UnityEditor.EditorUtility.SetDirty(this); // 在 Unity Editor 中需要标记为脏以使更改生效
            }
            else
            {
                // 如果 containerType 选择了 B，则将 isLoop 和 isHorizontal 置为默认值
                btnListContainerInitData.isLoop = BtnListContainerInitData.defaultTemplate.isLoop;
                btnListContainerInitData.isHorizontal = BtnListContainerInitData.defaultTemplate.isHorizontal;
            }
        }
#endif
    }
}


