using System.Runtime.Serialization;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// �浵�������ͽӿ�
    /// </summary>
    public interface ISaveData
    {
        /// <summary>
        /// �洢�����·��λ�ã��������ļ���
        /// </summary>
        [IgnoreDataMember]
        public string SavePath { get; set; }
        /// <summary>
        /// ������ļ���
        /// </summary>
        [IgnoreDataMember]
        public string SaveName { get; set; }
        /// <summary>
        /// �Ƿ������ݸ��£����Ƿ���Ҫ���´浵
        /// </summary>
        [IgnoreDataMember]
        public bool IsDirty { get; set; }
        /// <summary>
        /// �汾��
        /// </summary>
        [DataMember]
        public ML.Engine.Utility.Version Version { get; set; }
        public object Clone();
    }
}