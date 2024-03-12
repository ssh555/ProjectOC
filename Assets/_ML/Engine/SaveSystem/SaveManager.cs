using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// ����浵�ļ��Ķ�д
    /// ����浵ͳһ����
    /// </summary>
    public class SaveManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        /// <summary>
        /// ��ȡConfigAsset.Config���ݳ�ʼ��
        /// ����Config����SaveSystem
        /// </summary>
        public SaveConfig Config;
        /// <summary>
        /// ʹ�õ����ڴ�ȡ�ļ��Ķ���
        /// </summary>
        public SaveSystem SaveSystem;
        /// <summary>
        /// �浵������
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
        /// ����浵����
        /// </summary>
        public void SaveData<T>(T data) where T : ISaveData
        {
            this.SaveSystem.SaveData<T>(data, this.Config.UseEncrption);
        }
        /// <summary>
        /// ���ش浵����
        /// </summary>
        /// <param name="path">���·����û�к�׺</param>
        /// <returns></returns>
        public T LoadData<T>(string path) where T : ISaveData
        {
            return (T)this.SaveSystem.LoadData<T>(path, this.Config.UseEncrption);
        }
    }
}