using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ML.Engine.CombatSystem.DataConfig
{
    /// <summary>
    /// 施法数据
    /// </summary>
    public struct SpellDataConfig
    {
        /// <summary>
        /// 施法模式
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
            /// 目标,为null则报错
            /// </summary>
            [LabelText("作用对象")]
            public CombatObject.ICombatObject target;

            /// <summary>
            /// 要生成的trigger,为null则直接应用于 target
            /// </summary>
            [LabelText("生成Trigger")]
            public CombatObject.TriggerObject trigger;

            /// <summary>
            /// 生成点
            /// </summary>
            [LabelText("生成点")]
            public Vector3 spawnPoint;

            /// <summary>
            /// 旋转
            /// </summary>
            [LabelText("旋转方向")]
            public Quaternion spawnRot;

            ///// <summary>
            ///// 尺寸
            ///// </summary>
            //public Vector3 spawnScale;

            /// <summary>
            /// 向 target 移动的速度
            /// </summary>
            [LabelText("速度")]
            public float speed;
        }
        
        public struct SpellToPoint
        {
            /// <summary>
            /// true : 使用场景中的trigger
            /// false : 使用预制体生成
            /// </summary>
            [LabelText("附加在Character上的Trigger")]
            public bool IsAttached;

            /// <summary>
            /// 目标点
            /// </summary>
            [LabelText("目标位置点")]
            public Vector3 targetPoint;

            /// <summary>
            /// 要生成的trigger,为null则报错
            /// </summary>
            [LabelText("生成Trigger")]
            public CombatObject.TriggerObject trigger;

            /// <summary>
            /// 旋转
            /// </summary>
            [LabelText("旋转方向")]
            public Quaternion spawnRot;

            ///// <summary>
            ///// 尺寸
            ///// </summary>
            //public Vector3 spawnScale;

            /// <summary>
            /// 开始就激活 Trigger
            /// </summary>
            [LabelText("Trigger On Awake")]
            public bool bTriggerOnAwake;
        }
        
        public struct SpellToDirection
        {
            /// <summary>
            /// 方向
            /// </summary>
            [LabelText("移动方向")]
            public Vector3 direction;

            /// <summary>
            /// 距离
            /// </summary>
            [LabelText("移动最大距离")]
            public float distance;

            /// <summary>
            /// 要生成的trigger,为null则报错
            /// </summary>
            [LabelText("生成Trigger")]
            public CombatObject.TriggerObject trigger;

            /// <summary>
            /// 生成点
            /// </summary>
            [LabelText("生成位置点")]
            public Vector3 spawnPoint;

            /// <summary>
            /// 旋转
            /// </summary>
            [LabelText("旋转方向")]
            public Quaternion spawnRot;

            ///// <summary>
            ///// 尺寸
            ///// </summary>
            //public Vector3 spawnScale;

            /// <summary>
            /// 开始就激活 Trigger
            /// </summary>
            [LabelText("Trigger On Awake")]
            public bool bTriggerOnAwake;

            /// <summary>
            /// 移动速度
            /// </summary>
            [LabelText("速度")]
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
        /// 施法模式
        /// </summary>
        public SpellMode mode;
        /// <summary>
        /// 施法数据
        /// </summary>
        public SpellData data;

        public static implicit operator SpellData(SpellDataConfig A)
        {
            return A.data;
        }
    }
}

