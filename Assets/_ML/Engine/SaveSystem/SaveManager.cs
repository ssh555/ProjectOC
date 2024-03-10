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


        /// <summary>
        /// �Ƿ��Ѽ���������
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<SaveConfig> ABJAProcessor;
        public SaveManager()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<SaveConfig>("Config", "SaveSystemConfig", (data) =>
                {
                    this.Config = new SaveConfig(data);
                    if (this.Config.SaveType == SaveType.Json)
                    {
                        this.SaveSystem = new JsonSaveSystem();
                    }
                    else if (this.Config.SaveType == SaveType.Binary)
                    {
                        this.SaveSystem = new BinarySaveSystem();
                    }
                    this.SaveController = new SaveController();
                }, null, "�浵ϵͳ��������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }

        /// <summary>
        /// ����浵����
        /// </summary>
        public void SaveData<T>(T data) where T : ISaveData
        {
            this.SaveSystem.SaveData(data, this.Config.UseEncrption);
        }
        /// <summary>
        /// ���ش浵����
        /// </summary>
        /// <param name="path">���·����û�к�׺</param>
        /// <returns></returns>
        public T LoadData<T>(string path) where T : ISaveData
        {
            return (T)this.SaveSystem.LoadData(path, this.Config.UseEncrption);
        }
    }
}