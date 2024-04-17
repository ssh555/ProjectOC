using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [LabelText("效果"), System.Serializable]
    public class Effect
    {
        [LabelText("ID"), ReadOnly]
        public string ID = "";
        [LabelText("效果参数1 string"), ReadOnly]
        public string ParamStr = "";
        [LabelText("效果参数2 int"), ReadOnly]
        public int ParamInt;
        [LabelText("效果参数3 float"), ReadOnly]
        public float ParamFloat;
        #region 读表属性
        [LabelText("效果名称"), ShowInInspector, ReadOnly]
        public string Name { get => LocalGameManager.Instance!=null ? LocalGameManager.Instance.EffectManager.GetName(ID) : ""; }
        [LabelText("效果类型"), ShowInInspector, ReadOnly]
        public EffectType EffectType { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.EffectManager.GetEffectType(ID) : EffectType.None; }
        #endregion

        public Effect(EffectTableData config, string value)
        {
            this.ID = config.ID;
            if (string.IsNullOrEmpty(value))
            {
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
                    ParamFloat = float.Parse(value);
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
        }

        public void ApplyEffect(Worker worker)
        {
            if (worker == null)
            {
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
                        if (worker.Eff.ContainsKey(workType))
                        {
                            worker.Eff[workType] = ParamInt;
                        }
                    }
                    break;
            }
        }
    }
}