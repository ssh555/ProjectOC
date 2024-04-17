using System;
using System.Collections.Generic;
using System.IO;
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
        public Manager.GameManager.Version Version { get; set; }
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
        /// Path+Name+SaveName-��Ӧ�����ݽṹ����ȫ���ַ�����ӳ���
        /// </summary>
        public Dictionary<string, string> FileMap = new Dictionary<string, string>();

        public SaveDataFolder(){}

        public SaveDataFolder(string name, Manager.GameManager.Version version)
        {
            this.SavePath = name;
            this.Version = version;

            this.Name = name;
            this.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.LastSaveTime = this.CreateTime;
        }

        public void ChangeName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                this.IsDirty = true;
                this.SavePath = name;
                Dictionary<string, string> map = new Dictionary<string, string>();

                foreach (var kv in FileMap.ToList())
                {
                    string[] directories = Path.GetDirectoryName(kv.Key).Split(Path.DirectorySeparatorChar);
                    if (directories.Length >= 2)
                    {
                        directories[directories.Length - 1] = name;
                        string updatedDirectory = string.Join(Path.DirectorySeparatorChar, directories);
                        string updatedPath = Path.Combine(updatedDirectory, Path.GetFileName(kv.Key));
                        map[updatedPath] = kv.Value;
                    }
                }
                this.FileMap = map;
                this.Name = name;
            }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}