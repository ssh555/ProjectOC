using ProjectOC.ManagerNS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 刁民效果
    /// </summary>
    [System.Serializable]
    public class Effect
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID;
        /// <summary>
        /// 效果参数1 string
        /// </summary>
        public string ParamStr = "";
        /// <summary>
        /// 效果参数2 int
        /// </summary>
        public int ParamInt;
        /// <summary>
        /// 效果参数3 float
        /// </summary>
        public float ParamFloat;
        #region 读表属性
        /// <summary>
        /// 效果名称
        /// </summary>
        public string Name { get => LocalGameManager.Instance.EffectManager.GetName(ID); }
        /// <summary>
        /// 效果类型
        /// </summary>
        public EffectType EffectType { get => LocalGameManager.Instance.EffectManager.GetEffectType(ID); }
        #endregion

        public Effect(EffectManager.EffectTableJsonData config)
        {
            this.ID = config.ID;
            this.ParamStr = config.Param1;
            this.ParamInt = config.Param2;
            this.ParamFloat = config.Param3;
        }
        public Effect(Effect effect)
        {
            this.ID = effect.ID;
            this.ParamStr = effect.ParamStr;
            this.ParamInt = effect.ParamInt;
            this.ParamFloat = effect.ParamFloat;
        }

        public void ApplyEffect(Worker worker)
        {
            if (worker == null)
            {
                Debug.LogError($"Effect {this.ID} Worker is Null");
                return;
            }
            string workTypeStr;
            WorkType workType;
            switch (this.EffectType)
            {
                case EffectType.AlterAPMax:
                    worker.APMax = ParamInt;
                    break;

                case EffectType.AlterExpRate_Cook:
                case EffectType.AlterExpRate_HandCraft:
                case EffectType.AlterExpRate_Industry:
                case EffectType.AlterExpRate_Magic:
                case EffectType.AlterExpRate_Transport:
                case EffectType.AlterExpRate_Collect:
                    workTypeStr = this.EffectType.ToString().Split('_')[1];
                    if (Enum.TryParse(workTypeStr, out workType))
                    {
                        worker.ExpRate[workType] = ParamInt;
                    }
                    break;

                case EffectType.AlterWalkSpeed:
                    worker.WalkSpeed += ParamFloat;
                    break;

                case EffectType.AlterBURMax:
                    worker.BURMax += ParamInt;
                    break;

                case EffectType.AlterEff_Cook:
                case EffectType.AlterEff_HandCraft:
                case EffectType.AlterEff_Industry:
                case EffectType.AlterEff_Magic:
                case EffectType.AlterEff_Transport:
                case EffectType.AlterEff_Collect:
                    workTypeStr = this.EffectType.ToString().Split('_')[1];
                    if (Enum.TryParse(workTypeStr, out workType))
                    {
                        worker.Eff[workType] += ParamInt;
                    }
                    break;
            }
        }
    }
}