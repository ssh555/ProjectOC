using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using System;
using System.Collections.Generic;



namespace ProjectOC.CharacterInteract
{
    [System.Serializable]
    public class CommunicationManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Base
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        /// <summary>
        /// 单例管理
        /// </summary>
        public static CommunicationManager Instance { get { return instance; } }

        private static CommunicationManager instance;

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
        /// 当前已有的联系人数组
        /// </summary>
        private List<string> ContactorList = new List<string>();

        public enum MessageType
        { 
            Mission = 0,
            Normal
        }

        public class MessageInfo
        {
            public string MsgID;
            public bool isRead;
            public MessageType messageType;
            public int DelayedTime;
            public CounterDownTimer DelayTriggerTimer;
            public Action OnReplyCharacter;

            public MessageInfo(string MsgID,MessageType messageType,Action OnReplyCharacter)
            {
                this.MsgID = MsgID;
                this.isRead = false;
                this.messageType = messageType;
                //TODO 暂定为游戏时 1小时
                this.DelayedTime = 1;
                this.DelayTriggerTimer = new CounterDownTimer(LocalGameManager.Instance.DispatchTimeManager.TimeScale * 60 * DelayedTime, autocycle: false, autoStart: false);
                this.OnReplyCharacter = OnReplyCharacter;
            }
        }
        /// <summary>
        /// 该函数在延时触发计时器结束时触发，交由DispatchTimeManager检测是否到点，到点即触发短信
        /// </summary>
        private void CheckTrigger()
        {

        }
        /// <summary>
        /// 当前已触发的短信数组
        /// </summary>
        private List<MessageInfo> TriggeredMessageList = new();

        /// <summary>
        /// 当前延时的主线短信队列
        /// </summary>
        private List<MessageInfo> DelayedMissionMessageList = new();

        /// <summary>
        /// 当前延时的普通短信队列
        /// </summary>
        private List<MessageInfo> DelayedNormalMessageList = new();

        /// <summary>
        /// 玩家当前是否在对话
        /// </summary>
        private bool isInDialog;
        public bool IsInDialog { get { return isInDialog; } set { isInDialog = value; } }
        #endregion

        #region Internal
        /// <summary>
        /// 该函数用于普通短信触发延时结束后调用的条件判断函数
        /// </summary>
        private void CheckNormalMessage(MessageInfo messageInfo)
        {
            
        }
        #endregion

        #region External
        /// <summary>
        /// 该函数用于触发某条短信，根据该条短信的类型首先判断是否需要延时，若需要延时则加入相应的队列，若不需要则触发
        /// </summary>
        public void TriggerMessage(MessageInfo mi)
        {

        }
        /// <summary>
        /// 该函数用于解锁某个角色的联系人
        /// </summary>
        public void UnlockCharacterPhone(string CID)
        {

        }
        /// <summary>
        /// 该函数用于某个角色向玩家发送短信,根据短信种类不同在MessageInfo的构造器里对计时器的初始化有所不同
        /// </summary>
        public void SendMessage(string MID, MessageType mt, Action action = null)
        {

        }

        /// <summary>
        /// 该函数用于某个角色向玩家打电话
        /// </summary>
        public void CharacterCallPlayer(string CID)
        {

        }

        /// <summary>
        /// 该函数用于玩家向某个角色打电话
        /// </summary>
        public void PlayerCallCharacter(string CID)
        {

        }

        #endregion

        #region Load
        /// <summary>
        /// 角色表数据
        /// </summary>
        [System.Serializable]
        public struct MessageTableData
        {
            
        }
        public ML.Engine.ABResources.ABJsonAssetProcessor<MessageTableData[]> ABJAProcessor;
        private Dictionary<string, MessageTableData> MessageTableDataTableDataDic = new();
        private void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<MessageTableData[]>("OCTableData", "MainInteractRing", (datas) =>
            {
                foreach (var data in datas)
                {

                }
            }, "短信数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}



