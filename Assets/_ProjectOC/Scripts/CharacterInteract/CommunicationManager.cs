using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static ProjectOC.CharacterInteract.OCCharacterManager;

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
            MessageTableDataTableDataDic.Add("Message_CharacterFavor_1_1",MessageTableData.defaultTemplate1);
            MessageTableDataTableDataDic.Add("Message_CharacterFavor_1_2", MessageTableData.defaultTemplate2);
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
        public class MessageInfo : IComparable<MessageInfo>
        {
            public string MsgID;
            public string OCChacracterID;
            public bool isRead;
            public MessageType messageType;
            public int ReceiveOrder;
            public int DelayedTime;
            public CounterDownTimer DelayTriggerTimer;
            public Action OnReplyCharacter;

            public MessageInfo(string MsgID,string OCChacracterID,MessageType messageType,Action OnReplyCharacter)
            {
                this.MsgID = MsgID;
                this.OCChacracterID = OCChacracterID;
                this.isRead = false;
                this.messageType = messageType;
                //TODO 暂定为游戏时 1小时
                this.DelayedTime = 1;
                this.DelayTriggerTimer = new CounterDownTimer(LocalGameManager.Instance.DispatchTimeManager.TimeScale * 60 * DelayedTime, autocycle: false, autoStart: false);
                DelayTriggerTimer.OnEndEvent += () =>
                {
                    LocalGameManager.Instance.CommunicationManager.TriggerMessage(this);
                };
                this.OnReplyCharacter = OnReplyCharacter;
            }

            public int CompareTo(MessageInfo other)
            {
                //主线短信排在最前
                int result = messageType.CompareTo(other.messageType);
                if (result != 0)
                {
                    return result;
                }

                //如果未读普通短信排其次
                result = -isRead.CompareTo(other.isRead);
                if (result != 0)
                {
                    return result;
                }

                //已读短信按照时间顺序排列，越早的短信排在越下面。
                return -ReceiveOrder.CompareTo(other.ReceiveOrder);
            }

            public void StartDelayTime()
            {

                UnityEngine.Debug.Log("开始延时");
                DelayTriggerTimer.Start();
            }
        }
        /// <summary>
        /// 该函数在延时触发计时器结束时触发，交由DispatchTimeManager检测是否到点，到点即触发短信
        /// </summary>
        private void CheckTrigger(MessageInfo mi)
        {
            if(MessageTableDataTableDataDic.ContainsKey(mi.MsgID))
            {
                LocalGameManager.Instance.DispatchTimeManager.AddDelegationAction(MessageTableDataTableDataDic[mi.MsgID].StartTime, () => {
                    UnityEngine.Debug.Log("短信到点触发");
                    TriggerMessage(mi);
                });
            }
        }
        /// <summary>
        /// 当前已触发的短信数组
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, List<MessageInfo>> TriggeredMessageListDic = new();

        private List<MessageInfo> messageInfos = new List<MessageInfo>();
        public List<MessageInfo> MessageInfos { get { return messageInfos; } }

        /// <summary>
        /// 当前延时的主线短信队列
        /// </summary>
        private Dictionary<string, List<MessageInfo>> DelayedMissionMessageListDic = new();

        /// <summary>
        /// 当前延时的普通短信队列
        /// </summary>
        private Dictionary<string, List<MessageInfo>> DelayedNormalMessageListDic = new();

        /// <summary>
        /// 玩家当前是否在对话
        /// </summary>
        private bool isInDialog = false;
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
            UnityEngine.Debug.Log("TriggerMessage " +mi.MsgID+" "+ mi.OCChacracterID);
            if(mi.messageType == MessageType.Mission)
            {
                //主线短信默认触发时立刻发送， 若玩家处在对话中，则延时1h后触发
                if(isInDialog)
                {
                    mi.StartDelayTime();
                    return;
                }
                else
                {
                    if(!TriggeredMessageListDic.ContainsKey(mi.OCChacracterID))
                    {
                        TriggeredMessageListDic[mi.OCChacracterID] = new List<MessageInfo>();
                    }
                    TriggeredMessageListDic[mi.OCChacracterID].Add(mi);
                    messageInfos.Add(mi);
                    messageInfos.Sort();
                }
            }
            else if(mi.messageType == MessageType.Normal)
            {
                if (!TriggeredMessageListDic.ContainsKey(mi.OCChacracterID))
                {
                    if(isInDialog)
                    {
                        mi.StartDelayTime();
                        return;
                    }
                    else
                    {
                        TriggeredMessageListDic[mi.OCChacracterID] = new List<MessageInfo>
                        {
                            mi
                        };
                        messageInfos.Add(mi);
                        messageInfos.Sort();
                    }
                }
                else
                {
                    //存在该角色的未读信息？
                    var Mlist = TriggeredMessageListDic[mi.OCChacracterID];
                    for (int i = 0; i < Mlist.Count; ++i)
                    {
                        if (!Mlist[i].isRead) 
                        {
                            mi.StartDelayTime();
                            UnityEngine.Debug.Log("延时 " + mi.OCChacracterID+" "+mi.MsgID);
                            return;
                        }
                    }

                    if (isInDialog)
                    {
                        mi.StartDelayTime();
                        return;
                    }
                    else
                    {
                        TriggeredMessageListDic[mi.OCChacracterID].Add(mi);
                        messageInfos.Add(mi);
                        messageInfos.Sort();
                    }
                }
            }
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
        public void SendMessage(string MID,string CID, MessageType mt, Action action = null)
        {
            UnityEngine.Debug.Log("SendMessage " + MID +" "+ CID);
            CheckTrigger(new MessageInfo(MID, CID, mt, action));
        }

        /// <summary>
        /// 该函数用于某个角色向玩家打电话
        /// </summary>
        public void CharacterCallPlayer(string CID)
        {
            if (!ContactorList.Contains(CID))
            {
                ContactorList.Add(CID);
            }
        }

        /// <summary>
        /// 该函数用于玩家向某个角色打电话
        /// </summary>
        public void PlayerCallCharacter(string CID)
        {
            
        }


        public List<string> GetMessageContent(string MsgID)
        {
            if(MessageTableDataTableDataDic.ContainsKey(MsgID))
            {
                return new List<string>(MessageTableDataTableDataDic[MsgID].Content);
            }
            return new List<string>();
        }

        #endregion

        #region Load
        /// <summary>
        /// 角色表数据
        /// </summary>
        [System.Serializable]
        public struct MessageTableData
        {
            public string ID;
            public int StartTime;
            public List<string> Content;

            public static MessageTableData defaultTemplate1 = new MessageTableData()
            {
                ID = "Message_CharacterFavor_1_1",
                StartTime = 2,
                Content = new List<string>() { "Dialog_BeginnerTutorial_0", "Dialog_BeginnerTutorial_1", "Dialog_BeginnerTutorial_2" , "Dialog_BeginnerTutorial_3" }
            };

            public static MessageTableData defaultTemplate2 = new MessageTableData()
            {
                ID = "Message_CharacterFavor_1_2",
                StartTime = 2,
                Content = new List<string>() { "Dialog_BeginnerTutorial_0", "Dialog_BeginnerTutorial_1", "Dialog_BeginnerTutorial_2", "Dialog_BeginnerTutorial_3" }
            };
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



