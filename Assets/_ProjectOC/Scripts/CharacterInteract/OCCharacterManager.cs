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
        /// ��������
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
        /// ��ɫ�ֵ�
        /// </summary>
        private Dictionary<string, Character> CharacterDic = new();
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
        /// <summary>
        /// �洢��ǰ�����Ľ�ɫID
        /// </summary>
        private HashSet<string> UpgradeLockSet = new();
        /// <summary>
        /// �洢��ǰ�Ѿ���ȡ���ʺ����ID
        /// </summary>
        private HashSet<string> SentMessageSet = new();

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

        }
        /// <summary>
        /// �ú������ڵ�һ��֧���������ʱ�����øжȵȼ���0��1
        /// </summary>
        public void LevelUP(string CID)
        {

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
            }, "��ɫ�øж�����");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}
