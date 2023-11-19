using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player
{
    /// <summary>
    /// ������ PlayerModel ��������ϵĶ���֡�¼��ӿ�
    /// </summary>
    public class PlayerModelAniamtionEvent : MonoBehaviour
    {
        PlayerModelStateController stateController => this.GetComponentInParent<PlayerCharacter>().playerModelStateController;

        PlayerNormalMove moveAbility => stateController.moveAbility;

        /// <summary>
        /// ���� => �����ò���
        /// </summary>
        private void EnterInPreJump()
        {
            moveAbility.IsInPreJump = true;
        }

        /// <summary>
        /// ��ʽ��Ծ => ��Ҫ����������(IsInPreJump)�б�Ƕ���֡�¼�����Ȼ��������
        /// </summary>
        private void ExitInPreJump()
        {
            moveAbility.IsInPreJump = false;
        }
    }
}

