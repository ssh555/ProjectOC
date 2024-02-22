using ProjectOC.ManagerNS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ����Ч��
    /// </summary>
    [System.Serializable]
    public class Effect
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// Ч������1 string
        /// </summary>
        public string ParamStr = "";
        /// <summary>
        /// Ч������2 int
        /// </summary>
        public int ParamInt;
        /// <summary>
        /// Ч������3 float
        /// </summary>
        public float ParamFloat;
        #region ��������
        /// <summary>
        /// Ч������
        /// </summary>
        public string Name { get => LocalGameManager.Instance.EffectManager.GetName(ID); }
        /// <summary>
        /// Ч������
        /// </summary>
        public EffectType EffectType { get => LocalGameManager.Instance.EffectManager.GetEffectType(ID); }
        #endregion

        public Effect(EffectTableData config, string value)
        {
            this.ID = config.ID;
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError($"Effect Value {value} is empty or null");
                return;
            }
            switch (EffectType)
            {
                #region int
                case EffectType.AlterAPMax:
                case EffectType.AlterExpRate_Cook:
                case EffectType.AlterExpRate_HandCraft:
                case EffectType.AlterExpRate_Industry:
                case EffectType.AlterExpRate_Magic:
                case EffectType.AlterExpRate_Transport:
                case EffectType.AlterExpRate_Collect:
                case EffectType.AlterBURMax:
                case EffectType.AlterEff_Cook:
                case EffectType.AlterEff_HandCraft:
                case EffectType.AlterEff_Industry:
                case EffectType.AlterEff_Magic:
                case EffectType.AlterEff_Transport:
                case EffectType.AlterEff_Collect:
                    ParamInt = int.Parse(value);
                    break;
                #endregion
                #region float
                case EffectType.AlterWalkSpeed:
                    ParamFloat = int.Parse(value);
                    break;
                #endregion
            }
        }
        public Effect(Effect effect)
        {
            if (effect != null)
            {
                this.ID = effect.ID;
                this.ParamStr = effect.ParamStr;
                this.ParamInt = effect.ParamInt;
                this.ParamFloat = effect.ParamFloat;
            }
            else
            {
                Debug.LogError("Effect is null");
            }
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
                        if (worker.ExpRate.ContainsKey(workType))
                        {
                            worker.ExpRate[workType] = ParamInt;
                        }
                        else
                        {
                            Debug.LogError($"Worker {worker} ExpRate not contains WorkType {workType}");
                        }
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
                        if (worker.ExpRate.ContainsKey(workType))
                        {
                            worker.ExpRate[workType] = ParamInt;
                        }
                        else
                        {
                            Debug.LogError($"Worker {worker} ExpRate not contains WorkType {workType}");
                        }
                    }
                    break;
            }
        }
    }
}