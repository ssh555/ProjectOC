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
        public SaveConfig Config;
        /// <summary>
        /// 使用的用于存取文件的对象
        /// </summary>
        public SaveSystem SaveSystem;
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
                else if (this.Config.SaveType == SaveType.Binary)
                {
                    this.SaveSystem = new BinarySaveSystem();
                }
                this.SaveController = new SaveController();
            };
        }

        /// <summary>
        /// 保存存档数据
        /// </summary>
        public void SaveData<T>(T data) where T : ISaveData
        {
            this.SaveSystem.SaveData<T>(data, this.Config.UseEncrption);
        }
        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="path">相对路径，没有后缀</param>
        /// <returns></returns>
        public T LoadData<T>(string path) where T : ISaveData
        {
            return (T)this.SaveSystem.LoadData<T>(path, this.Config.UseEncrption);
        }
    }
}