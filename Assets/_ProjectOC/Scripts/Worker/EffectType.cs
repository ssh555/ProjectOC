namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 刁民属性效果类型
    /// Set前缀为直接赋值
    /// Offset前缀为增量
    /// </summary>
    public enum EffectType
    {
        /// <summary>
        /// 变更体力上限
        /// </summary>
        Set_int_APMax,
        /// <summary>
        /// 变更移动速度
        /// </summary>
        Offset_float_WalkSpeed,
        /// <summary>
        /// 变更负重上限
        /// </summary>
        Offset_int_BURMax,

        /// <summary>
        /// 变更烹饪效率加成
        /// </summary>
        Offset_int_Eff_Cook,
        /// <summary>
        /// 变更手工效率加成
        /// </summary>
        Offset_int_Eff_HandCraft,
        /// <summary>
        /// 变更重工效率加成
        /// </summary>
        Offset_int_Eff_Industry,
        /// <summary>
        /// 变更科学效率加成
        /// </summary>
        Offset_int_Eff_Science,
        /// <summary>
        /// 变更魔法效率加成
        /// </summary>
        Offset_int_Eff_Magic,
        /// <summary>
        /// 变更搬运效率加成
        /// </summary>
        Offset_int_Eff_Transport,

        /// <summary>
        /// 变更烹饪经验获取速度
        /// </summary>
        Set_int_ExpRate_Cook,
        /// <summary>
        /// 变更手工经验获取速度
        /// </summary>
        Set_int_ExpRate_HandCraft,
        /// <summary>
        /// 变更重工经验获取速度
        /// </summary>
        Set_int_ExpRate_Industry,
        /// <summary>
        /// 变更科学经验获取速度
        /// </summary>
        Set_int_ExpRate_Science,
        /// <summary>
        /// 变更魔法经验获取速度
        /// </summary>
        Set_int_ExpRate_Magic,
        /// <summary>
        /// 变更搬运经验获取速度
        /// </summary>
        Set_int_ExpRate_Transport,
    }
}