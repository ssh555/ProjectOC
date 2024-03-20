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
        private SaveConfig Config;
        /// <summary>
        /// ʹ�õ����ڴ�ȡ�ļ��Ķ���
        /// </summary>
        private SaveSystem SaveSystem;
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
                else if (this.Config.SaveType == SaveType.XML)
                {
                    this.SaveSystem = new XMLSaveSystem();
                }
                this.SaveController = new SaveController(Config, SaveSystem);
            };
        }

        

        /// <summary>
        /// ���ش浵����
        /// </summary>
        /// <param name="relativePathWithoutSuffix">���·����û�к�׺</param>
        public T LoadData<T>(string relativePathWithoutSuffix) where T : ISaveData
        {
            T data = (T)this.SaveSystem.LoadData<T>(relativePathWithoutSuffix, this.Config.UseEncrption);
            return data;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void SaveData<T>(T data) where T : ISaveData
        {
            this.SaveSystem.SaveData<T>(data, this.Config.UseEncrption);
        }
    }
}