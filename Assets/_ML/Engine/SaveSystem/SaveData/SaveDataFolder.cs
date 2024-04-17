using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 对应一个存档文件夹下的SaveConfig文件
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
        /// 此存档的名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 存档创建时间
        /// </summary>
        public string CreateTime;

        /// <summary>
        /// 最近修改时间
        /// </summary>
        public string LastSaveTime;

        /// <summary>
        /// Path+Name+SaveName-对应的数据结构类型全名字符串的映射表
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