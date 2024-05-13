using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [LabelText("Ч��"), System.Serializable]
    public struct Effect
    {
        [LabelText("ID"), ReadOnly]
        public string ID;
        [LabelText("����"), ShowInInspector, ReadOnly]
        public string Name => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.EffectManager.GetName(ID) : "";
        [LabelText("Ч������"), ShowInInspector, ReadOnly]
        public EffectType EffectType => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.EffectManager.GetEffectType(ID) : EffectType.None;
        [LabelText("Ч������1 string"), ReadOnly]
        public string ParamStr;
        [LabelText("Ч������2 int"), ReadOnly]
        public int ParamInt;
        [LabelText("Ч������3 float"), ReadOnly]
        public float ParamFloat;
        [LabelText("Ч������4 bool"), ReadOnly]
        public bool ParamBool;

        public Effect(EffectTableData config)
        {
            ID = config.ID;
            ParamStr = config.Param1;
            ParamInt = config.Param2;
            ParamFloat = config.Param3;
            ParamBool = config.Param4;
        }
    }
}