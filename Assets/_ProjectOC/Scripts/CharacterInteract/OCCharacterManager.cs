using System.Collections.Generic;
using UnityEngine.TextCore.Text;

namespace ProjectOC.CharacterInteract
{
    [System.Serializable]
    public class OCCharacterManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Base
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        /// <summary>
        /// 单例管理
        /// </summary>
        public static OCCharacterManager Instance { get { return instance; } }

        private static OCCharacterManager instance;

        public void Init()
        {
        }

        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
                Init();
            }
        }

        public void OnUnregister()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        #endregion

        #region Field & Property
        /// <summary>
        /// 角色字典
        /// </summary>
        private Dictionary<string, Character> CharacterDic = new();
        /// <summary>
        /// 默认获取的好感度
        /// </summary>
        int CharacterFavor;
        /// <summary>
        /// 额外好感度
        /// </summary>
        int ModifyCharacterFavor;
        /// <summary>
        /// 好感度倍率
        /// </summary>
        float FactorCharacterFavor;
        /// <summary>
        /// 实际获取的好感度
        /// </summary>
        int RealCharacterFavor;
        /// <summary>
        /// 存储当前上锁的角色ID
        /// </summary>
        private HashSet<string> UpgradeLockSet = new();
        /// <summary>
        /// 存储当前已经抽取的问候短信ID
        /// </summary>
        private HashSet<string> SentMessageSet = new();

        //TODO 管理角色的通讯时间表

        #endregion

        #region Internal
        /// <summary>
        /// 该函数用于某角色当前好感度满时，解锁支线，入睡后触发，需要调用外部模块
        /// </summary>
        private void UnlockBranchLine(string CID)
        {

        }
        /// <summary>
        /// 该函数用于到达满好感需要上锁
        /// </summary>
        private void AddUpgradeLock(string CID)
        {

        }
        #endregion

        #region External
        /// <summary>
        /// 该函数用于增加某个角色的好感度,到达满好感需要上锁 若升级处理升级逻辑，包括转折点
        /// </summary>
        public void AddCharacterFavor(string CID, int offset)
        {

        }
        /// <summary>
        /// 该函数用于第一个支线任务完成时，将好感度等级由0至1
        /// </summary>
        public void LevelUP(string CID)
        {

        }
        /// <summary>
        /// 该函数用于完成支线时，解锁
        /// </summary>
        public void ReleaseUpgradeLock(string CID)
        {

        }
        /// <summary>
        /// 用于检查某角色是否处于上锁
        /// </summary>
        public bool CheckCharacterIsLock(string CID)
        {
            return false;
        }
        /// <summary>
        /// 当角色问候计时器结束时调用，不放回抽取短信并发送
        /// </summary>
        public void OnCharacterGreeting(string CID)
        {

        }
        /// <summary>
        /// 当角色约会计时器结束时调用，放回抽取短信并发送
        /// </summary>
        public void OnCharacterDating(string CID)
        {

        }
        #endregion

        #region Load
        /// <summary>
        /// 角色好感度表数据
        /// </summary>
        [System.Serializable]
        public struct CharacterFavorTableData
        {

        }
        public ML.Engine.ABResources.ABJsonAssetProcessor<CharacterFavorTableData[]> ABJAProcessor;
        private Dictionary<string, CharacterFavorTableData> CharacterFavorTableDataDic = new();
        private void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<CharacterFavorTableData[]>("OCTableData", "MainInteractRing", (datas) =>
            {
                foreach (var data in datas)
                {

                }
            }, "角色好感度数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}
