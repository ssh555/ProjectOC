using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    /// <summary>
    /// ����|��֡ ����
    /// </summary>
    [System.Serializable]
    public struct FreezeFrameParams
    {
        /// <summary>
        /// ����|��֡�ȼ�
        /// </summary>
        [LabelText("����|��֡�ȼ�"), SerializeField]
        public byte freezeLevel;

        /// <summary>
        /// ����ʱ��
        /// </summary>
        [LabelText("����ʱ��"), SerializeField]
        public float duration;

        /// <summary>
        /// ʱ������
        /// </summary>
        [LabelText("ʱ������"), SerializeField]
        public float timeScale;
    }

    /// <summary>
    /// ����|��֡ �ӿ�
    /// </summary>
    public interface IFreezeFrame
    {
        /// <summary>
        /// Ӧ�ÿ���
        /// </summary>
        public abstract void ApplyFreezeFrame(FreezeFrameParams freezeFrameParams);

        /// <summary>
        /// ��������
        /// </summary>
        public abstract void EndFreezeFrame();
    }

}
