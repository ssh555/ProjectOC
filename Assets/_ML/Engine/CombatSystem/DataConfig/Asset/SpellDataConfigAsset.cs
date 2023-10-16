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
        #region ����
        [LabelText("ʩ����ʽ")]
        public SpellMode mode;

        [LabelText("���ö���"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget")]
        public CombatObject.ICombatObject target;

        [LabelText("��������Trigger"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint")]
        public bool IsAttached;

        [LabelText("����Trigger"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || (this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint && !this.IsAttached) || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public CombatObject.TriggerObject trigger;

        [LabelText("���ɵ�"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public Vector3 spawnPoint;

        [LabelText("��ת����"), ShowIf("@this.mode == ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection ||(this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint && !this.IsAttached)")]
        public Quaternion spawnRot;

        [LabelText("�ٶ�"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToTarget || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public float speed;

        [LabelText("Ŀ��λ�õ�"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint && !this.IsAttached")]
        public Vector3 targetPoint;


        [LabelText("Trigger On Awake"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToPoint || this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public bool bTriggerOnAwake;


        [LabelText("�ƶ�����"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
        public Vector3 direction;


        [LabelText("�ƶ�������"), ShowIf("@this.mode== ML.Engine.CombatSystem.DataConfig.SpellDataConfig.SpellMode.SpellToDirection")]
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

