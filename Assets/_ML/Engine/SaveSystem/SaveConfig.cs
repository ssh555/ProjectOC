namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// �浵����
    /// </summary>
    public struct SaveConfig
    {
        /// <summary>
        /// �浵�ļ�����
        /// </summary>
        public SaveType SaveType;
        /// <summary>
        /// �Ƿ����
        /// </summary>
        public bool UseEncrption;
        public SaveConfig(SaveConfig config)
        {
            this.SaveType = config.SaveType;
            this.UseEncrption = config.UseEncrption;
        }
    }
}