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
        /// ��������
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
        /// ��ǰ���е���ϵ������
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
                //TODO �ݶ�Ϊ��Ϸʱ 1Сʱ
                this.DelayedTime = 1;
                this.DelayTriggerTimer = new CounterDownTimer(LocalGameManager.Instance.DispatchTimeManager.TimeScale * 60 * DelayedTime, autocycle: false, autoStart: false);
                this.OnReplyCharacter = OnReplyCharacter;
            }
        }
        /// <summary>
        /// �ú�������ʱ������ʱ������ʱ����������DispatchTimeManager����Ƿ񵽵㣬���㼴��������
        /// </summary>
        private void CheckTrigger()
        {

        }
        /// <summary>
        /// ��ǰ�Ѵ����Ķ�������
        /// </summary>
        private List<MessageInfo> TriggeredMessageList = new();

        /// <summary>
        /// ��ǰ��ʱ�����߶��Ŷ���
        /// </summary>
        private List<MessageInfo> DelayedMissionMessageList = new();

        /// <summary>
        /// ��ǰ��ʱ����ͨ���Ŷ���
        /// </summary>
        private List<MessageInfo> DelayedNormalMessageList = new();

        /// <summary>
        /// ��ҵ�ǰ�Ƿ��ڶԻ�
        /// </summary>
        private bool isInDialog;
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
        public void SendMessage(string MID, MessageType mt, Action action = null)
        {

        }

        /// <summary>
        /// �ú�������ĳ����ɫ����Ҵ�绰
        /// </summary>
        public void CharacterCallPlayer(string CID)
        {

        }

        /// <summary>
        /// �ú������������ĳ����ɫ��绰
        /// </summary>
        public void PlayerCallCharacter(string CID)
        {

        }

        #endregion

        #region Load
        /// <summary>
        /// ��ɫ������
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
            }, "��������");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}



