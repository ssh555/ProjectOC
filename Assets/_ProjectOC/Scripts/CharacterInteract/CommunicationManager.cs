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
        /// ��������
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
        /// ��ǰ���е���ϵ������
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
                //TODO �ݶ�Ϊ��Ϸʱ 1Сʱ
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
                //���߶���������ǰ
                int result = messageType.CompareTo(other.messageType);
                if (result != 0)
                {
                    return result;
                }

                //���δ����ͨ���������
                result = -isRead.CompareTo(other.isRead);
                if (result != 0)
                {
                    return result;
                }

                //�Ѷ����Ű���ʱ��˳�����У�Խ��Ķ�������Խ���档
                return -ReceiveOrder.CompareTo(other.ReceiveOrder);
            }

            public void StartDelayTime()
            {

                UnityEngine.Debug.Log("��ʼ��ʱ");
                DelayTriggerTimer.Start();
            }
        }
        /// <summary>
        /// �ú�������ʱ������ʱ������ʱ����������DispatchTimeManager����Ƿ񵽵㣬���㼴��������
        /// </summary>
        private void CheckTrigger(MessageInfo mi)
        {
            if(MessageTableDataTableDataDic.ContainsKey(mi.MsgID))
            {
                LocalGameManager.Instance.DispatchTimeManager.AddDelegationAction(MessageTableDataTableDataDic[mi.MsgID].StartTime, () => {
                    UnityEngine.Debug.Log("���ŵ��㴥��");
                    TriggerMessage(mi);
                });
            }
        }
        /// <summary>
        /// ��ǰ�Ѵ����Ķ�������
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, List<MessageInfo>> TriggeredMessageListDic = new();

        private List<MessageInfo> messageInfos = new List<MessageInfo>();
        public List<MessageInfo> MessageInfos { get { return messageInfos; } }

        /// <summary>
        /// ��ǰ��ʱ�����߶��Ŷ���
        /// </summary>
        private Dictionary<string, List<MessageInfo>> DelayedMissionMessageListDic = new();

        /// <summary>
        /// ��ǰ��ʱ����ͨ���Ŷ���
        /// </summary>
        private Dictionary<string, List<MessageInfo>> DelayedNormalMessageListDic = new();

        /// <summary>
        /// ��ҵ�ǰ�Ƿ��ڶԻ�
        /// </summary>
        private bool isInDialog = false;
        public bool IsInDialog { get { return isInDialog; } set { isInDialog = value; } }
        #endregion

        #region Internal
        /// <summary>
        /// �ú���������ͨ���Ŵ�����ʱ��������õ������жϺ���
        /// </summary>
        private void CheckNormalMessage(MessageInfo messageInfo)
        {
            
        }
        #endregion

        #region External
        /// <summary>
        /// �ú������ڴ���ĳ�����ţ����ݸ������ŵ����������ж��Ƿ���Ҫ��ʱ������Ҫ��ʱ�������Ӧ�Ķ��У�������Ҫ�򴥷�
        /// </summary>
        public void TriggerMessage(MessageInfo mi)
        {
            UnityEngine.Debug.Log("TriggerMessage " +mi.MsgID+" "+ mi.OCChacracterID);
            if(mi.messageType == MessageType.Mission)
            {
                //���߶���Ĭ�ϴ���ʱ���̷��ͣ� ����Ҵ��ڶԻ��У�����ʱ1h�󴥷�
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
                    //���ڸý�ɫ��δ����Ϣ��
                    var Mlist = TriggeredMessageListDic[mi.OCChacracterID];
                    for (int i = 0; i < Mlist.Count; ++i)
                    {
                        if (!Mlist[i].isRead) 
                        {
                            mi.StartDelayTime();
                            UnityEngine.Debug.Log("��ʱ " + mi.OCChacracterID+" "+mi.MsgID);
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
        /// �ú������ڽ���ĳ����ɫ����ϵ��
        /// </summary>
        public void UnlockCharacterPhone(string CID)
        {

        }
        /// <summary>
        /// �ú�������ĳ����ɫ����ҷ��Ͷ���,���ݶ������಻ͬ��MessageInfo�Ĺ�������Լ�ʱ���ĳ�ʼ��������ͬ
        /// </summary>
        public void SendMessage(string MID,string CID, MessageType mt, Action action = null)
        {
            UnityEngine.Debug.Log("SendMessage " + MID +" "+ CID);
            CheckTrigger(new MessageInfo(MID, CID, mt, action));
        }

        /// <summary>
        /// �ú�������ĳ����ɫ����Ҵ�绰
        /// </summary>
        public void CharacterCallPlayer(string CID)
        {
            if (!ContactorList.Contains(CID))
            {
                ContactorList.Add(CID);
            }
        }

        /// <summary>
        /// �ú������������ĳ����ɫ��绰
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
        /// ��ɫ������
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
            }, "��������");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}



