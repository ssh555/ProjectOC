using System;
using System.Collections.Generic;
using System.Linq;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 对应一个存档文件夹下的SaveConfig文件
    /// </summary>
    public class SaveDataFolder : ISaveData
    {
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
        /// 存档文件名称-对应的数据结构类型全名字符串的映射表
        /// SaveName -> Path+Name+SaveName+文件后缀
        /// </summary>
        public Dictionary<string, string> FileMap = new Dictionary<string, string>();

        public SaveDataFolder() : base("", "SaveConfig") { }

        public SaveDataFolder(string name) : base(name, "SaveConfig")
        {
            this.Name = name;
            this.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.LastSaveTime = this.CreateTime;
        }
        public void ChangeName(string name)
        {
            this.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.IsDirty = true;
            this.Path = name;
            foreach (var kv in FileMap.ToList())
            {
                FileMap[kv.Key] = kv.Value.Replace(this.Name, name);
            }
            this.Name = name;
        }
    }
}