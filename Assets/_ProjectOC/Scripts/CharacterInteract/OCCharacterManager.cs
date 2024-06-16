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
        /// ��������
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
        /// ��ɫ�ֵ�
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, OCCharacter> CharacterDic = new();
        /// <summary>
        /// Ĭ�ϻ�ȡ�ĺøж�
        /// </summary>
        int CharacterFavor;
        /// <summary>
        /// ����øж�
        /// </summary>
        int ModifyCharacterFavor;
        /// <summary>
        /// �øжȱ���
        /// </summary>
        float FactorCharacterFavor;
        /// <summary>
        /// ʵ�ʻ�ȡ�ĺøж�
        /// </summary>
        int RealCharacterFavor;
        /// �洢��ǰ�Ѿ������Ľ�ɫ
        /// </summary>
        [ShowInInspector]
        private List<OCCharacter> UnlockedOCCharacters = new();
        /// <summary>
        /// �洢��ǰ�����Ľ�ɫID
        /// </summary>
        private HashSet<string> UpgradeLockSet = new();

        //TODO �����ɫ��ͨѶʱ���

        #endregion

        #region Internal
        /// <summary>
        /// �ú�������ĳ��ɫ��ǰ�øж���ʱ������֧�ߣ���˯�󴥷�����Ҫ�����ⲿģ��
        /// </summary>
        private void UnlockBranchLine(string CID)
        {

        }
        /// <summary>
        /// �ú������ڵ������ø���Ҫ����
        /// </summary>
        private void AddUpgradeLock(string CID)
        {

        }
        #endregion

        #region External
        /// <summary>
        /// �ú�����������ĳ����ɫ�ĺøж�,�������ø���Ҫ���� ���������������߼�������ת�۵�
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
        /// �ú������ڵ�һ��֧���������ʱ�����øжȵȼ���0��1
        /// </summary>
        public void LevelUP(string CID)
        {
            UnityEngine.Debug.Log("LevelUP " + CID);
            if (!CharacterFavorTableDataDic.ContainsKey(CID)) return;
            if (CharacterDic.ContainsKey(CID)) return;
            this.CharacterDic.Add(CID, new OCCharacter(CharacterFavorTableDataDic[CID]));
        }
        /// <summary>
        /// �ú����������֧��ʱ������
        /// </summary>
        public void ReleaseUpgradeLock(string CID)
        {

        }
        /// <summary>
        /// ���ڼ��ĳ��ɫ�Ƿ�������
        /// </summary>
        public bool CheckCharacterIsLock(string CID)
        {
            return false;
        }
        /// <summary>
        /// ����ɫ�ʺ��ʱ������ʱ���ã����Żس�ȡ���Ų�����
        /// </summary>
        public void OnCharacterGreeting(string CID)
        {

        }
        /// <summary>
        /// ����ɫԼ���ʱ������ʱ���ã��Żس�ȡ���Ų�����
        /// </summary>
        public void OnCharacterDating(string CID)
        {

        }
        #endregion

        #region Load
        /// <summary>
        /// ��ɫ�øжȱ�����
        /// </summary>
        [System.Serializable]
        public struct CharacterFavorTableData
        {
            public string ID;
            //�øж�����(2����5��)
            public List<(int, int)> FavorRequire;
            //������֧������(2����5��)
            public List<(int, string)> MissionUnlock;
            //��ʶ��ϵת�۵�
            public int KnowStage;
            //���ѹ�ϵת�۵�
            public int FriendStage;
            //ֿ�ѹ�ϵת�۵�
            public int IntimateStage;
            //�ճ��ʺ���ʱ
            public (int, int) GreetingDelay;
            //�ճ��ʺ�
            public List<string> Greeting;
            //������Լ��ʱ
            public (int, int) InvitationDelay;
            //������Լ
            public List<string> Invitation;
            //������ʱ
            public int GiftDelay;
            //����
            public List<string> Gift;
            //�羰ƫ��
            public List<string> SceneFavor;
            //����ƫ��
            public List<string> GameFavor;
            //��ζƫ��
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
            }, "��ɫ�øж�����");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}
