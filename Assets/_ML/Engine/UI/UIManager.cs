using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
namespace ML.Engine.UI
{
    public sealed class UIManager : Manager.GlobalManager.IGlobalManager
    {
        #region UIջ����
        /// <summary>
        /// ��ǰ��ʾ��UIջ
        /// </summary>
        private Stack<UIBasePanel> panelStack = new Stack<UIBasePanel>();

        /// <summary>
        /// �رյ�ǰ���ϲ�UI����ײ�UI�޷��رգ�������û����ײ�UI����Ϊnull
        /// </summary>
        public UIBasePanel PopPanel()
        {
            // ��ײ�UI->start�޷��Ƴ�
            if (panelStack.Count < 2)
            {
                return null;
            }
            // ����ջ��
            UIBasePanel topPanel = panelStack.Pop();
            topPanel.OnExit();
            // ����ջ��
            panelStack.Peek().OnRecovery();
            return topPanel;
        }

        /// <summary>
        /// ��UI��ʾ�ڽ�����
        /// </summary>
        /// <param name="panelType"></param>
        public void PushPanel(UIBasePanel panel)
        {
            // ��ͣջ��
            if (panelStack.Count > 0)
            {
                panelStack.Peek().OnPause();
            }
            // ѹ��ջ
            panel.OnEnter();
            panelStack.Push(panel);
        }
        /// <summary>
        /// ��UIջ����Ϊ0��1ʱ�����Ը�����ײ�UIPanel��ֻ��ͨ���˺�������ײ�UIPanel��ջ
        /// ��ջΪ��ʱ������ͨ��PushPanelѹ����ײ��UIPanel
        /// </summary>
        public bool ChangeBotUIPanel(UIBasePanel panel)
        {
            if(this.panelStack.Count > 2)
            {
                return false;
            }
            if (this.panelStack.Count == 1)
            {
                this.panelStack.Pop().OnExit();
            }

            if(panel != null)
            {
                this.PushPanel(panel);
            }

            return true;
        }

        /// <summary>
        /// ���ջ����UIPanel
        /// </summary>
        /// <returns></returns>
        public UIBasePanel GetTopUIPanel()
        {
            return this.panelStack.Peek();
        }

        #endregion
    }

}
