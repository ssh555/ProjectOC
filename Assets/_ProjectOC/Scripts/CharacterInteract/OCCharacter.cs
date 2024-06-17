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

            //好感度
            FavorValue = 0;
            CurLevelFavorValue = 0;
            FavarLevel = 1;
            relation = Relation.Stranger;

            //社交奖励
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
        #region 好感度
        /// <summary>
        /// 好感度总值
        /// </summary>
        [ShowInInspector]
        private int FavorValue;
        /// <summary>
        /// 当前等级好感度总值
        /// </summary>
        [ShowInInspector]
        private int CurLevelFavorValue;
        /// <summary>
        /// 好感度等级
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
            //判断等级
            foreach (var (level,requireNum) in characterFavorTableData.FavorRequire)
            {
                if (level > FavarLevel && CurLevelFavorValue >= requireNum) 
                {
                    FavarLevel = level;
                    CurLevelFavorValue = 0;

                    //TODO 判断解锁的支线任务

                    //判断转折点
                    if (FavarLevel == characterFavorTableData.KnowStage)
                    {
                        Debug.Log("转变 KnowStage");
                        relation = Relation.Know;
                        StartGreetingTimer();
                    }

                    if (FavarLevel == characterFavorTableData.FriendStage)
                    {
                        Debug.Log("转变 FriendStage");
                        relation = Relation.Friend;
                        StartDatingTimer();
                    }

                    if (FavarLevel == characterFavorTableData.IntimateStage)
                    {
                        Debug.Log("转变 IntimateStage");
                        relation = Relation.Intimate;
                    }
                    break;
                }
            }

        }
        #endregion

        #region 社交奖励
        private CounterDownTimer GreetingTimer;
        private CounterDownTimer DatingTimer;

        /// <summary>
        /// 日常问候数组 不放回抽取
        /// </summary>
        [ShowInInspector]
        private List<string> Greeting;
        private void StartGreetingTimer()
        {
            Debug.Log("StartGreetingTimer");
            //随机抽取
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
                //发送问候短信
                LocalGameManager.Instance.CommunicationManager.SendMessage(msgId, cID, CommunicationManager.MessageType.Normal, null);
                StartGreetingTimer();
            });
        }

        private void StartDatingTimer()
        {
            Debug.Log("StartDatingTimer");
            //随机抽取
            if (Greeting.Count == 0) return;
            var InvitationDelay = characterFavorTableData.InvitationDelay;
            int randomValue = Random.Range(InvitationDelay.Item1, InvitationDelay.Item2);
            string msgId = characterFavorTableData.Invitation[Random.Range(0, characterFavorTableData.Invitation.Count)];
            LocalGameManager.Instance.DispatchTimeManager.AddDelegationActionFromNow(randomValue, 0, 0, () =>
            {
                if (relation != Relation.Friend) return;
                //发送约会短信
                LocalGameManager.Instance.CommunicationManager.SendMessage(msgId, cID, CommunicationManager.MessageType.Normal, null);
                StartDatingTimer();
            });
        }

        #endregion

    }
}


