using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ��������Ч��
    /// </summary>
    [System.Serializable]
    public class Effect
    {
        /// <summary>
        /// ID Effects_Ч����
        /// </summary>
        public string ID = "";
        /// <summary>
        /// Ч������
        /// </summary>
        public string Name = "";
        /// <summary>
        /// Ч������
        /// </summary>
        public EffectType Type;
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

        /// <summary>
        /// �洢Set����֮ǰ����ֵ������RemoveEffect
        /// </summary>
        public string PreParamStr = "";
        /// <summary>
        /// �洢Set����֮ǰ����ֵ������RemoveEffect
        /// </summary>
        public int PreParamInt;
        /// <summary>
        /// �洢Set����֮ǰ����ֵ������RemoveEffect
        /// </summary>
        public float PreParamFloat;

        public Effect(string id)
        {
            // TODO: ����������
        }

        public void ApplyEffectToWorker(Worker worker)
        {
            if (worker == null)
            {
                return;
            }
            string workTypeStr;
            WorkType workType;
            switch (this.Type)
            {
                case EffectType.Set_int_APMax:
                    PreParamInt = ParamInt - worker.APMax;
                    worker.APMax = ParamInt;
                    break;
                case EffectType.Set_int_ExpRate_Cook:
                case EffectType.Set_int_ExpRate_HandCraft:
                case EffectType.Set_int_ExpRate_Industry:
                case EffectType.Set_int_ExpRate_Science:
                case EffectType.Set_int_ExpRate_Magic:
                case EffectType.Set_int_ExpRate_Transport:
                    workTypeStr = this.Type.ToString().Split('_')[3];
                    if (Enum.TryParse(workTypeStr, out workType))
                    {
                        PreParamInt = ParamInt - worker.ExpRate[workType];
                        worker.ExpRate[workType] = ParamInt;
                    }
                    break;

                case EffectType.Offset_float_WalkSpeed:
                    worker.WalkSpeed += ParamFloat;
                    break;
                case EffectType.Offset_int_BURMax:
                    worker.BURMax += ParamInt;
                    break;
                case EffectType.Offset_int_Eff_Cook:
                case EffectType.Offset_int_Eff_HandCraft:
                case EffectType.Offset_int_Eff_Industry:
                case EffectType.Offset_int_Eff_Science:
                case EffectType.Offset_int_Eff_Magic:
                case EffectType.Offset_int_Eff_Transport:
                    workTypeStr = this.Type.ToString().Split('_')[3];
                    if (Enum.TryParse(workTypeStr, out workType))
                    {
                        worker.Eff[workType] += ParamInt;
                    }
                    break;
            }
        }

        public void RemoveEffectToWorker(Worker worker)
        {
            if (worker == null)
            {
                return;
            }
            string workTypeStr;
            WorkType workType;
            switch (this.Type)
            {
                case EffectType.Set_int_APMax:
                    worker.APMax -= PreParamInt;
                    break;
                case EffectType.Offset_float_WalkSpeed:
                    worker.WalkSpeed -= ParamFloat;
                    break;
                case EffectType.Offset_int_BURMax:
                    worker.BURMax -= ParamInt;
                    break;
                case EffectType.Offset_int_Eff_Cook:
                case EffectType.Offset_int_Eff_HandCraft:
                case EffectType.Offset_int_Eff_Industry:
                case EffectType.Offset_int_Eff_Science:
                case EffectType.Offset_int_Eff_Magic:
                case EffectType.Offset_int_Eff_Transport:
                    workTypeStr = this.Type.ToString().Split('_')[3];
                    if (Enum.TryParse(workTypeStr, out workType))
                    {
                        worker.Eff[workType] -= ParamInt;
                    }
                    break;
                case EffectType.Set_int_ExpRate_Cook:
                case EffectType.Set_int_ExpRate_HandCraft:
                case EffectType.Set_int_ExpRate_Industry:
                case EffectType.Set_int_ExpRate_Science:
                case EffectType.Set_int_ExpRate_Magic:
                case EffectType.Set_int_ExpRate_Transport:
                    workTypeStr = this.Type.ToString().Split('_')[3];
                    if (Enum.TryParse(workTypeStr, out workType))
                    {
                        worker.ExpRate[workType] -= PreParamInt;
                    }
                    break;
            }
        }
    }
}