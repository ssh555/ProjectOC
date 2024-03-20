using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 负责存档文件的读写
    /// 负责存档统一管理
    /// </summary>
    public class SaveManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        /// <summary>
        /// 读取ConfigAsset.Config数据初始化
        /// 根据Config设置SaveSystem
        /// </summary>
        private SaveConfig Config;
        /// <summary>
        /// 使用的用于存取文件的对象
        /// </summary>
        private SaveSystem SaveSystem;
        /// <summary>
        /// 存档控制器
        /// </summary>
        public SaveController SaveController;

        public SaveManager()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SaveSystemConfigAsset>("ML/Config/SaveSystemConfig.asset").Completed += (handle) =>
            {
                SaveSystemConfigAsset data = handle.Result;
                this.Config = new SaveConfig(data.Config);

                if (this.Config.SaveType == SaveType.Json)
                {
                    this.SaveSystem = new JsonSaveSystem();
                }
                else if (this.Config.SaveType == SaveType.XML)
                {
                    this.SaveSystem = new XMLSaveSystem();
                }
                this.SaveController = new SaveController(Config, SaveSystem);
            };
        }

        

        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="relativePathWithoutSuffix">相对路径，没有后缀</param>
        public T LoadData<T>(string relativePathWithoutSuffix) where T : ISaveData
        {
            T data = (T)this.SaveSystem.LoadData<T>(relativePathWithoutSuffix, this.Config.UseEncrption);
            return data;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveData<T>(T data) where T : ISaveData
        {
            this.SaveSystem.SaveData<T>(data, this.Config.UseEncrption);
        }
    }
}