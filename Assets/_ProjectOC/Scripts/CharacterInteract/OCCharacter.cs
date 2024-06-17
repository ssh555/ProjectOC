using ML.Engine.Timer;
using ProjectOC.ClanNS;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static ProjectOC.CharacterInteract.OCCharacterManager;

namespace ProjectOC.CharacterInteract
{
    public class OCCharacter : ProjectOC.ClanNS.NPC
    {
        private string cID;
        public string CID {  get { return cID; } }
        [ShowInInspector]
        private CharacterFavorTableData characterFavorTableData;
        


        public OCCharacter(CharacterFavorTableData characterFavorTableData) : base()
        {
            this.characterFavorTableData = characterFavorTableData;
            this.cID = characterFavorTableData.ID;

            //�øж�
            FavorValue = 0;
            CurLevelFavorValue = 0;
            FavarLevel = 1;
            relation = Relation.Stranger;

            //�罻����
            this.Greeting = new List<string>(characterFavorTableData.Greeting);
        }

        public override bool AddTAG(PersonalityTAG tag)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveTAG(PersonalityTAG tag)
        {
            throw new System.NotImplementedException();
        }
        #region �øж�
        /// <summary>
        /// �øж���ֵ
        /// </summary>
        [ShowInInspector]
        private int FavorValue;
        /// <summary>
        /// ��ǰ�ȼ��øж���ֵ
        /// </summary>
        [ShowInInspector]
        private int CurLevelFavorValue;
        /// <summary>
        /// �øжȵȼ�
        /// </summary>
        [ShowInInspector]
        private int FavarLevel;
        [ShowInInspector]
        private Relation relation;
        private enum Relation
        {
            Stranger = 0,
            Know,
            Friend,
            Intimate,
            Mate
        }
        public void AddLevel(int offset)
        {
            FavorValue += offset;
            CurLevelFavorValue += offset;
            //�жϵȼ�
            foreach (var (level,requireNum) in characterFavorTableData.FavorRequire)
            {
                if (level > FavarLevel && CurLevelFavorValue >= requireNum) 
                {
                    FavarLevel = level;
                    CurLevelFavorValue = 0;

                    //TODO �жϽ�����֧������

                    //�ж�ת�۵�
                    if (FavarLevel == characterFavorTableData.KnowStage)
                    {
                        Debug.Log("ת�� KnowStage");
                        relation = Relation.Know;
                        StartGreetingTimer();
                    }

                    if (FavarLevel == characterFavorTableData.FriendStage)
                    {
                        Debug.Log("ת�� FriendStage");
                        relation = Relation.Friend;
                        StartDatingTimer();
                    }

                    if (FavarLevel == characterFavorTableData.IntimateStage)
                    {
                        Debug.Log("ת�� IntimateStage");
                        relation = Relation.Intimate;
                    }
                    break;
                }
            }

        }
        #endregion

        #region �罻����
        private CounterDownTimer GreetingTimer;
        private CounterDownTimer DatingTimer;

        /// <summary>
        /// �ճ��ʺ����� ���Żس�ȡ
        /// </summary>
        [ShowInInspector]
        private List<string> Greeting;
        private void StartGreetingTimer()
        {
            Debug.Log("StartGreetingTimer");
            //�����ȡ
            if (Greeting.Count == 0) return;
            var GreetingDelay = characterFavorTableData.GreetingDelay;
            int randomValue = Random.Range(GreetingDelay.Item1, GreetingDelay.Item2);
            string msgId = Greeting[Random.Range(0, Greeting.Count)];
            Greeting.Remove(msgId);
            Debug.Log("Greeting cnt " + Greeting.Count);
            LocalGameManager.Instance.DispatchTimeManager.AddDelegationActionFromNow(0, 1, 0, () =>
            {
                Debug.Log("Time UP");
                if (relation != Relation.Know) return;
                //�����ʺ����
                LocalGameManager.Instance.CommunicationManager.SendMessage(msgId, cID, CommunicationManager.MessageType.Normal, null);
                StartGreetingTimer();
            });
        }

        private void StartDatingTimer()
        {
            Debug.Log("StartDatingTimer");
            //�����ȡ
            if (Greeting.Count == 0) return;
            var InvitationDelay = characterFavorTableData.InvitationDelay;
            int randomValue = Random.Range(InvitationDelay.Item1, InvitationDelay.Item2);
            string msgId = characterFavorTableData.Invitation[Random.Range(0, characterFavorTableData.Invitation.Count)];
            LocalGameManager.Instance.DispatchTimeManager.AddDelegationActionFromNow(randomValue, 0, 0, () =>
            {
                if (relation != Relation.Friend) return;
                //����Լ�����
                LocalGameManager.Instance.CommunicationManager.SendMessage(msgId, cID, CommunicationManager.MessageType.Normal, null);
                StartDatingTimer();
            });
        }

        #endregion

    }
}


