using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InteractSystem
{
    public interface IInteraction
    {
        /// <summary>
        /// �ɽ�������� -> ����UI
        /// ��Ӧjson�ļ� : InteractKeyTip.json
        /// </summary>
        public string InteractType { get; set; }

        /// <summary>
        /// ������GameObject
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        /// ��ʾ������λ��ƫ��
        /// </summary>
        public Vector3 PosOffset { get; set; }

        /// <summary>
        /// ��Ϊѡ�еĿɽ�����ʱ����
        /// </summary>
        /// <param name="component"></param>
        public void OnSelectedEnter(InteractComponent component)
        {

        }

        /// <summary>
        /// �˳���Ϊѡ�еĿɽ�����ʱ����
        /// </summary>
        /// <param name="component"></param>
        public void OnSelectedExit(InteractComponent component)
        {

        }

        /// <summary>
        /// ����ȷ��ʱ����
        /// </summary>
        /// <param name="component"></param>
        public void Interact(InteractComponent component)
        {

        }
    }
}
