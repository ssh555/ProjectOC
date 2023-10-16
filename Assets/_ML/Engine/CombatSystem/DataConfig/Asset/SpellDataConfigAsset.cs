using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static ML.Engine.CombatSystem.DataConfig.SpellDataConfig;

namespace ML.Engine.CombatSystem.DataConfig
{
    [CreateAssetMenu(fileName = "SpellConfig", menuName = "ML/CombatSystem/Config/SpellConfig", order = 1)]
    public class SpellDataConfigAsset : SerializedScriptableObject
    {
        #region 配置
        [LabelText("施法方式")]
        public SpellMode mode;

        [LabelText("作用对象"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget")]
        public CombatObject.ICombatObject target;

        [LabelText("场景附加Trigger"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint")]
        public bool IsAttached;

        [LabelText("生成Trigger"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || (this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint && !this.IsAttached) || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public CombatObject.TriggerObject trigger;

        [LabelText("生成点"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public Vector3 spawnPoint;

        [LabelText("旋转方向"), ShowIf("@this.mode == ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection ||(this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint && !this.IsAttached)")]
        public Quaternion spawnRot;

        [LabelText("速度"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public float speed;

        [LabelText("目标位置点"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint && !this.IsAttached")]
        public Vector3 targetPoint;


        [LabelText("Trigger On Awake"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public bool bTriggerOnAwake;


        [LabelText("移动方向"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public Vector3 direction;


        [LabelText("移动最大距离"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public float distance;
        #endregion
    
        public SpellDataConfig ToData()
        {
            SpellDataConfig data = new SpellDataConfig();
            data.mode = this.mode;
            if(this.mode == SpellMode.SpellToTarget)
            {
                data.data.toTarget.target = this.target;
                data.data.toTarget.trigger = this.trigger;
                data.data.toTarget.spawnPoint = this.spawnPoint;
                data.data.toTarget.spawnRot = this.spawnRot;
                data.data.toTarget.speed = this.speed;
            }
            else if (this.mode == SpellMode.SpellToPoint)
            {
                data.data.toPoint.IsAttached = this.IsAttached;
                data.data.toPoint.targetPoint = this.targetPoint;
                data.data.toPoint.trigger = this.trigger;
                data.data.toPoint.spawnRot = this.spawnRot;
                data.data.toPoint.bTriggerOnAwake = this.bTriggerOnAwake;
            }
            else if (this.mode == SpellMode.SpellToDirection)
            {
                data.data.toDirection.direction = this.direction;
                data.data.toDirection.distance = this.distance;
                data.data.toDirection.trigger = this.trigger;
                data.data.toDirection.spawnPoint = this.spawnPoint;
                data.data.toDirection.spawnRot = this.spawnRot;
                data.data.toDirection.bTriggerOnAwake = this.bTriggerOnAwake;
                data.data.toDirection.speed = this.speed;
            }
            return data;
        }
    }
}

