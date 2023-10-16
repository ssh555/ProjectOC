using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// ʩ������
    /// </summary>
    public struct SpellDataConfig
    {
        /// <summary>
        /// ʩ��ģʽ
        /// </summary>
        public enum SpellMode : ushort
        {
            SpellToTarget,
            SpellToPoint,
            SpellToDirection,
        }
        
        public struct SpellToTarget
        {
            /// <summary>
            /// Ŀ��,Ϊnull�򱨴�
            /// </summary>
            [LabelText("���ö���")]
            public CombatObject.ICombatObject target;

            /// <summary>
            /// Ҫ���ɵ�trigger,Ϊnull��ֱ��Ӧ���� target
            /// </summary>
            [LabelText("����Trigger")]
            public CombatObject.TriggerObject trigger;

            /// <summary>
            /// ���ɵ�
            /// </summary>
            [LabelText("���ɵ�")]
            public Vector3 spawnPoint;

            /// <summary>
            /// ��ת
            /// </summary>
            [LabelText("��ת����")]
            public Quaternion spawnRot;

            ///// <summary>
            ///// �ߴ�
            ///// </summary>
            //public Vector3 spawnScale;

            /// <summary>
            /// �� target �ƶ����ٶ�
            /// </summary>
            [LabelText("�ٶ�")]
            public float speed;
        }
        
        public struct SpellToPoint
        {
            /// <summary>
            /// true : ʹ�ó����е�trigger
            /// false : ʹ��Ԥ��������
            /// </summary>
            [LabelText("������Character�ϵ�Trigger")]
            public bool IsAttached;

            /// <summary>
            /// Ŀ���
            /// </summary>
            [LabelText("Ŀ��λ�õ�")]
            public Vector3 targetPoint;

            /// <summary>
            /// Ҫ���ɵ�trigger,Ϊnull�򱨴�
            /// </summary>
            [LabelText("����Trigger")]
            public CombatObject.TriggerObject trigger;

            /// <summary>
            /// ��ת
            /// </summary>
            [LabelText("��ת����")]
            public Quaternion spawnRot;

            ///// <summary>
            ///// �ߴ�
            ///// </summary>
            //public Vector3 spawnScale;

            /// <summary>
            /// ��ʼ�ͼ��� Trigger
            /// </summary>
            [LabelText("Trigger On Awake")]
            public bool bTriggerOnAwake;
        }
        
        public struct SpellToDirection
        {
            /// <summary>
            /// ����
            /// </summary>
            [LabelText("�ƶ�����")]
            public Vector3 direction;

            /// <summary>
            /// ����
            /// </summary>
            [LabelText("�ƶ�������")]
            public float distance;

            /// <summary>
            /// Ҫ���ɵ�trigger,Ϊnull�򱨴�
            /// </summary>
            [LabelText("����Trigger")]
            public CombatObject.TriggerObject trigger;

            /// <summary>
            /// ���ɵ�
            /// </summary>
            [LabelText("����λ�õ�")]
            public Vector3 spawnPoint;

            /// <summary>
            /// ��ת
            /// </summary>
            [LabelText("��ת����")]
            public Quaternion spawnRot;

            ///// <summary>
            ///// �ߴ�
            ///// </summary>
            //public Vector3 spawnScale;

            /// <summary>
            /// ��ʼ�ͼ��� Trigger
            /// </summary>
            [LabelText("Trigger On Awake")]
            public bool bTriggerOnAwake;

            /// <summary>
            /// �ƶ��ٶ�
            /// </summary>
            [LabelText("�ٶ�")]
            public float speed;
        }
        
        [StructLayout(LayoutKind.Auto)]
        public struct SpellData
        {
            public SpellToTarget toTarget;
            public SpellToPoint toPoint;
            public SpellToDirection toDirection;

            public static implicit operator SpellToTarget(SpellData A)
            {
                return A.toTarget;
            }

            public static implicit operator SpellToPoint(SpellData A)
            {
                return A.toPoint;
            }

            public static implicit operator SpellToDirection(SpellData A)
            {
                return A.toDirection;
            }
        }

        /// <summary>
        /// ʩ��ģʽ
        /// </summary>
        public SpellMode mode;
        /// <summary>
        /// ʩ������
        /// </summary>
        public SpellData data;

        public static implicit operator SpellData(SpellDataConfig A)
        {
            return A.data;
        }
    }
}

