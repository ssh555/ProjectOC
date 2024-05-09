using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [LabelText("效果"), System.Serializable]
    public struct Effect
    {
        [LabelText("ID"), ReadOnly]
        public string ID;
        [LabelText("名称"), ShowInInspector, ReadOnly]
        public string Name => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.EffectManager.GetName(ID) : "";
        [LabelText("效果类型"), ShowInInspector, ReadOnly]
        public EffectType EffectType => ManagerNS.LocalGameManager.Instance != null ? ManagerNS.LocalGameManager.Instance.EffectManager.GetEffectType(ID) : EffectType.None;
        [LabelText("效果参数1 string"), ReadOnly]
        public string ParamStr;
        [LabelText("效果参数2 int"), ReadOnly]
        public int ParamInt;
        [LabelText("效果参数3 float"), ReadOnly]
        public float ParamFloat;
        [LabelText("效果参数4 bool"), ReadOnly]
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