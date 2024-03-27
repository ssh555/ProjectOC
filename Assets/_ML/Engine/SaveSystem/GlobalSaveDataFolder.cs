using System.Collections.Generic;

namespace ML.Engine.SaveSystem
{
    public class GlobalSaveDataFolder : ISaveData
    {
        /// <summary>
        /// ´æµµÃû³Æ
        /// </summary>
        public List<string> SaveDataFolders = new List<string>();
        public GlobalSaveDataFolder() : base("", "GlobalSaveConfig")
        {
        }
        public GlobalSaveDataFolder(List<string> saveDataFolders) :base("", "GlobalSaveConfig")
        {
            this.SaveDataFolders = new List<string>(saveDataFolders);
        }
    }
}