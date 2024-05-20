using ProjectOC.ManagerNS;
using UnityEngine;
using ProjectOC.WorkerNS;

namespace ML.Engine.Event
{
    public sealed partial class FunctionLiabrary : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region 科技树Event
        public void ProNodeUpgrade(string p1,int p2)
        {
            Debug.Log($"ProNodeUpgrade {p1} {p2}");
        }

        public void InteractUpgrade(string p1, int p2)
        {
            Debug.Log($"InteractUpgrade {p1} {p2}");
        }

        #endregion

        #region 订单管理Event
        public void AddOrder(string p1)
        {
            Debug.Log($"AddOrder {p1} ");
        }

        #endregion

        #region 岛屿扩张
        public void SetLifeDiversionNum(int _count)
        {
            LocalGameManager.Instance.BuildPowerIslandManager.PowerCoreMaxCount = _count;
        }

        public void SetSubIslandNum(int p1)
        {
            Debug.Log($"SetSubIslandNum {p1} ");
        }
        
        #endregion

        public void CanDeepSea(bool p1)
        {
            Debug.Log($"CanDeepSea {p1} ");
        }

        public void CanExtremeSea(bool p1)
        {
            Debug.Log($"CanExtremeSea {p1} ");
        }

        public void CanDirectEcho(bool p1)
        {
            Debug.Log($"CanDirectEcho {p1} "); 
            if(p1)
            {
                LocalGameManager.Instance.WorkerEchoManager.LevelUp();
            }
        }

        public void AddProNodeEffBase(int p1,int p2)
        {
            Debug.Log($"AddProNodeEffBase {p1} {p2}");
        }

        public void AddWorkerEff_AllSkill(int p1, int p2, int p3, Worker worker) 
        {
            worker.AddWorkerEff_AllSkill(p1, p2, p3);
        }
        public void RemoveAddWorkerEff_AllSkill(int p1, int p2, int p3, Worker worker)
        {
            worker.RemoveAddWorkerEff_AllSkill(p1, p2, p3);
        }
        public void RelaxExtraSpeed(int p1, Worker worker)
        {
            worker.RelaxExtraSpeed(p1);
        }
        public void RemoveRelaxExtraSpeed(int p1, Worker worker)
        {
            worker.RemoveRelaxExtraSpeed(p1);
        }
        public void FishInNest(Worker worker)
        {
            Debug.Log("FishInNest");
            worker.AddFishInNest();
        }
        public void RemoveFishInNest(Worker worker)
        {
            Debug.Log("RemoveFishInNest");
            worker.RemoveFishInNest();
        }

        public void AddRandomSkill(int p1)
        {
            Debug.Log($"AddRandomSkill {p1}");
        }

        public void EatAddAP(int p1)
        {
            Debug.Log($"EatAddAP {p1}");
        }

        public void EatAddEM(int p1)
        {
            Debug.Log($"EatAddEM {p1}");
        }

        public void UrgentExtraTrust(int p1,int p2)
        {
            Debug.Log($"EatAddEM {p1} {p2}");
        }

        public void NewDayRecover(int p1)
        {
            Debug.Log($"NewDayRecover {p1}");
        }

        public void NegotiationRecover(int p1)
        {
            Debug.Log($"NegotiationRecover {p1}");
        }

        public void CureRecover(int p1)
        {
            Debug.Log($"CureRecover {p1}");
        }
    }
}
