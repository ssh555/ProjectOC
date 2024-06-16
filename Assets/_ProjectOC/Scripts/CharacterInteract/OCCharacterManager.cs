using ML.Engine.InventorySystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.TextCore.Text;
using static ML.Engine.UI.UIBtnListInitor;

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
            CharacterFavorTableDataDic.Add("CharacterFavor_1", CharacterFavorTableData.defaultTemplate);
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
        [ShowInInspector]
        private Dictionary<string, OCCharacter> CharacterDic = new();
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
        /// 存储当前已经解锁的角色
        /// </summary>
        [ShowInInspector]
        private List<OCCharacter> UnlockedOCCharacters = new();
        /// <summary>
        /// 存储当前上锁的角色ID
        /// </summary>
        private HashSet<string> UpgradeLockSet = new();

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
            UnityEngine.Debug.Log("AddCharacterFavor " + CID+" "+ offset);
            if (!CharacterFavorTableDataDic.ContainsKey(CID)) return;

            if (CharacterDic.ContainsKey(CID))
            {
                CharacterDic[CID].AddLevel(offset);
            }
        }
        /// <summary>
        /// 该函数用于第一个支线任务完成时，将好感度等级由0至1
        /// </summary>
        public void LevelUP(string CID)
        {
            UnityEngine.Debug.Log("LevelUP " + CID);
            if (!CharacterFavorTableDataDic.ContainsKey(CID)) return;
            if (CharacterDic.ContainsKey(CID)) return;
            this.CharacterDic.Add(CID, new OCCharacter(CharacterFavorTableDataDic[CID]));
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
            public string ID;
            //好感度需求(2级至5级)
            public List<(int, int)> FavorRequire;
            //解锁的支线任务(2级至5级)
            public List<(int, string)> MissionUnlock;
            //认识关系转折点
            public int KnowStage;
            //朋友关系转折点
            public int FriendStage;
            //挚友关系转折点
            public int IntimateStage;
            //日常问候延时
            public (int, int) GreetingDelay;
            //日常问候
            public List<string> Greeting;
            //主动邀约延时
            public (int, int) InvitationDelay;
            //主动邀约
            public List<string> Invitation;
            //送礼延时
            public int GiftDelay;
            //送礼
            public List<string> Gift;
            //风景偏好
            public List<string> SceneFavor;
            //娱乐偏好
            public List<string> GameFavor;
            //口味偏好
            public List<string> TasteFavor;

            public static CharacterFavorTableData defaultTemplate = new CharacterFavorTableData()
            {
                ID = "CharacterFavor_1",
                FavorRequire = new List<(int, int)>() { (2,2), (3, 4) , (4, 6) , (5, 9) },
                MissionUnlock = new List<(int, string)>(),
                KnowStage = 2,
                FriendStage = 3,
                IntimateStage = 4,
                GreetingDelay = (1,1),
                Greeting = new List<string>(),

                InvitationDelay = (1,1),
                Invitation = new List<string>(),
                GiftDelay = 2,

                Gift  = new List<string>(),
                SceneFavor = new List<string>(),
                GameFavor = new List<string>(),
                TasteFavor = new List<string>()
            };
        }
        public ML.Engine.ABResources.ABJsonAssetProcessor<CharacterFavorTableData[]> ABJAProcessor;
        [ShowInInspector]
        private Dictionary<string, CharacterFavorTableData> CharacterFavorTableDataDic = new();
        private void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<CharacterFavorTableData[]>("OCTableData", "MainInteractRing", (datas) =>
            {
                foreach (var data in datas)
                {
                    CharacterFavorTableDataDic.Add(data.ID, data);
                }
            }, "角色好感度数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}
