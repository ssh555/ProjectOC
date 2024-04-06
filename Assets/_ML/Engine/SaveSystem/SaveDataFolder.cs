using ML.Engine.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// ��Ӧһ���浵�ļ����µ�SaveConfig�ļ�
    /// </summary>
    public class SaveDataFolder : ISaveData
    {
        #region ISaveData
        public string SavePath { get; set; } = "";
        public string SaveName { get; set; } = "SaveConfig";
        public bool IsDirty { get; set; }
        public Utility.Version Version { get; set; }
        #endregion

        /// <summary>
        /// �˴浵������
        /// </summary>
        public string Name;

        /// <summary>
        /// �浵����ʱ��
        /// </summary>
        public string CreateTime;

        /// <summary>
        /// ����޸�ʱ��
        /// </summary>
        public string LastSaveTime;

        /// <summary>
        /// �浵�ļ�����-��Ӧ�����ݽṹ����ȫ���ַ�����ӳ���
        /// SaveName -> Path+Name+SaveName+�ļ���׺
        /// </summary>
        public Dictionary<string, string> FileMap = new Dictionary<string, string>();

        public SaveDataFolder(){}

        public SaveDataFolder(string name, Utility.Version version)
        {
            this.SavePath = name;
            this.Version = version;

            this.Name = name;
            this.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.LastSaveTime = this.CreateTime;
        }

        public void ChangeName(string name)
        {
            this.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.IsDirty = true;
            this.SavePath = name;
            foreach (var kv in FileMap.ToList())
            {
                FileMap[kv.Key] = kv.Value.Replace(this.Name, name);
            }
            this.Name = name;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}