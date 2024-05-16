using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [LabelText("喵喵窝")]
    public class FeatureBuilding : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        #region BuildingPart
        public string InteractType { get; set; } = "Feature";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                Init();
            }
            if (!isFirstBuild && oldPos != newPos)
            {
                Clear();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Worker_UI/Prefab_Worker_UI_FeaturePanel.prefab",
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
                {
                    UI.UIFeature uiPanel = (handle.Result).GetComponent<UI.UIFeature>();
                    uiPanel.FeatBuild = this;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
        #endregion

        #region Init Destroy
        public void Init()
        {
            Seats[0] = new FeatureSeat(this, transform.Find($"seat1"));
            Seats[1] = new FeatureSeat(this, transform.Find($"seat2"));
            Seat = new FeatureSeat(this, transform.Find($"seat3"));
        }
        public void Clear()
        {
            CancleExchange();
            CancleCorrect();
            (Seats[0] as IWorkerContainer).RemoveWorker();
            (Seats[1] as IWorkerContainer).RemoveWorker();
            (Seat as IWorkerContainer).RemoveWorker();
        }
        public void OnDestroy()
        {
            Clear();
        }
        #endregion

        private System.Random Random = new System.Random();
        #region 词条传授
        [LabelText("喵喵窝座位"), ShowInInspector, ReadOnly]
        public FeatureSeat[] Seats = new FeatureSeat[2];
        public bool IsExchange => timer != null && !timer.IsStoped;
        public bool IsExchangeEnd;
        [NonSerialized]
        private ML.Engine.Timer.CounterDownTimer timer;
        [LabelText("传授计时器"), ShowInInspector]
        public ML.Engine.Timer.CounterDownTimer Timer
        {
            get
            {
                if (timer == null && ManagerNS.LocalGameManager.Instance != null)
                {
                    timer = new ML.Engine.Timer.CounterDownTimer
                        (ManagerNS.LocalGameManager.Instance.FeatureManager.Config.FeatTransTime, false, false);
                    timer.OnUpdateEvent += UpdateActionForTimer;
                    timer.OnEndEvent += EndActionForTimer;
                }
                return timer;
            }
        }
        public event Action<double> OnExchangeUpdateEvent;
        public event Action OnExchangeEndEvent;
        public bool CanExhcange()
        {
            bool flag1 = Seats[0].IsArrive && Seats[1].IsArrive;
            var Config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
            bool flag2 = ManagerNS.LocalGameManager.Instance.Player.InventoryHaveItem(Config.FeatTransCostItemID, Config.FeatTransCostItemNum);
            bool flag3 = Seats[0].WorkerFeatureIDs.Count != 0 || Seats[1].WorkerFeatureIDs.Count != 0;
            bool flag4 = Seats[0].WorkerFeatureIDs.Count != 3 || Seats[1].WorkerFeatureIDs.Count != 3;
            return flag1 && flag2 && flag3 && flag4;
        }
        public void ChangeWorker(int index, Worker worker)
        {
            if (0 > index && index > 1) { return; }
            if (Seats[index].Worker != worker && !IsExchange && !IsExchangeEnd)
            {
                (Seats[index] as IWorkerContainer).SetWorker(worker);
            }
        }
        public void ExchangeFeature()
        {
            if (CanExhcange() && !IsExchange && !IsExchangeEnd)
            {
                var Config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
                ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(Config.FeatTransCostItemID, Config.FeatTransCostItemNum, priority: -1);
                Timer.Start();
            }
        }
        public void ConfirmExchange()
        {
            if (IsExchangeEnd)
            {
                Seats[0].ChangerWorkerFeature();
                Seats[1].ChangerWorkerFeature();
                IsExchangeEnd = false;
            }
        }
        public void CancleExchange()
        {
            bool isExchange = IsExchange;
            if (isExchange || IsExchangeEnd)
            {
                Seats[0].SetFeatureID();
                Seats[1].SetFeatureID();
            }
            if (isExchange)
            {
                Timer.End();
                var config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
                ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(config.FeatTransCostItemID, config.FeatTransCostItemNum);
            }
            IsExchangeEnd = false;
        }
        private void UpdateActionForTimer(double time)
        {
            OnExchangeUpdateEvent?.Invoke(time);
        }
        private void EndActionForTimer()
        {
            var set1 = Seats[0].FeatureIDs.ToHashSet();
            var set2 = Seats[1].FeatureIDs.ToHashSet();
            int cnt1 = set1.Count;
            int cnt2 = set2.Count;
            set1.SymmetricExceptWith(set2);
            if (cnt1 < 3 && set2.Count > 0)
            {
                Seats[0].WorkerFeatureIDs.Add("");
                Seats[0].FeatureIDs.Add(set2.ToList()[Random.Next(0, set2.Count + 1)]);
            }
            if (cnt2 < 3 && set1.Count > 0)
            {
                Seats[1].WorkerFeatureIDs.Add("");
                Seats[1].FeatureIDs.Add(set1.ToList()[Random.Next(0, set1.Count + 1)]);
            }
            IsExchangeEnd = true;
            OnExchangeEndEvent?.Invoke();
        }
        #endregion

        #region 词条修正
        [LabelText("词条修正座位"), ShowInInspector, ReadOnly]
        public FeatureSeat Seat;
        [ShowInInspector, ReadOnly]
        public bool IsCorrect => correctTimer != null && !correctTimer.IsStoped;
        public bool IsCorrectEnd;
        public int CorrectTime => ManagerNS.LocalGameManager.Instance != null ?
            ManagerNS.LocalGameManager.Instance.FeatureManager.GetFeatureCorrectTime(CorrectType) : 0;
        public string CorrectCostItemID => ManagerNS.LocalGameManager.Instance != null ?
            ManagerNS.LocalGameManager.Instance.FeatureManager.GetFeatureCorrectItemID(CorrectType) : "";
        public int CorrectCostItemNum => ManagerNS.LocalGameManager.Instance != null ?
            ManagerNS.LocalGameManager.Instance.FeatureManager.GetFeatureCorrectItemNum(CorrectType) : 0;
        [LabelText("修正类型"), ShowInInspector, ReadOnly]
        public FeatureCorrectType CorrectType { get; private set; }
        [NonSerialized]
        private ML.Engine.Timer.CounterDownTimer correctTimer;
        [LabelText("修正计时器"), ShowInInspector]
        public ML.Engine.Timer.CounterDownTimer CorrectTimer
        {
            get
            {
                if (correctTimer == null && ManagerNS.LocalGameManager.Instance != null)
                {
                    correctTimer = new ML.Engine.Timer.CounterDownTimer(CorrectTime, false, false);
                    correctTimer.OnUpdateEvent += UpdateActionForCorrectTimer;
                    correctTimer.OnEndEvent += EndActionForCorrectTimer;
                }
                return correctTimer;
            }
        }
        public event Action<double> OnCorrectUpdateEvent;
        public event Action OnCorrectEndEvent;
        public void ChangeCorrectType(FeatureCorrectType type)
        {
            if (CorrectType != type && !IsCorrect && !IsCorrectEnd)
            {
                CorrectType = type;
            }
        }
        public bool CanCorrect()
        {
            bool flag1 = Seat.IsArrive;
            bool flag2 = ManagerNS.LocalGameManager.Instance.Player.InventoryHaveItem(CorrectCostItemID, CorrectCostItemNum);
            bool flag3 = Seat.WorkerFeatureIDs.Count > 0;
            return flag1 && flag2 && flag3;
        }
        public void ChangeCorrectWorker(Worker worker)
        {
            if (Seat.Worker != worker && !IsCorrect && !IsCorrectEnd)
            {
                (Seat as IWorkerContainer).SetWorker(worker);
            }
        }
        public void CorrectFeature()
        {
            if (CanCorrect() && !IsCorrect && !IsCorrectEnd)
            {
                ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(CorrectCostItemID, CorrectCostItemNum, priority: -1);
                CorrectTimer.Start();
            }
        }
        public void ConfirmCorrect()
        {
            if (IsCorrectEnd)
            {
                Seat.ChangerWorkerFeature();
                IsCorrectEnd = false;
            }
        }
        public void CancleCorrect()
        {
            bool isCorrect = IsCorrect;
            if (isCorrect || IsCorrectEnd)
            {
                Seat.SetFeatureID();
            }
            if (isCorrect)
            {
                CorrectTimer.End();
                ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(CorrectCostItemID, CorrectCostItemNum);
            }
            IsCorrectEnd = false;
        }
        private void UpdateActionForCorrectTimer(double time)
        {
            OnCorrectUpdateEvent?.Invoke(time);
        }
        private void EndActionForCorrectTimer()
        {
            if (CorrectType != FeatureCorrectType.Reverse)
            {
                int index = Random.Next(0, Seat.FeatureIDs.Count + 1);
                string featID = Seat.FeatureIDs[index];
                string newFeatID = "";
                switch (CorrectType)
                {
                    case FeatureCorrectType.Upgrade:
                        newFeatID = ManagerNS.LocalGameManager.Instance.FeatureManager.GetUpgradeID(featID);
                        break;
                    case FeatureCorrectType.Downgrade:
                        newFeatID = ManagerNS.LocalGameManager.Instance.FeatureManager.GetReduceID(featID);
                        break;
                    case FeatureCorrectType.Delete:
                        Seat.FeatureIDs[index] = "";
                        break;
                }
                if (!string.IsNullOrEmpty(newFeatID))
                {
                    Seat.FeatureIDs[index] = newFeatID;
                }
            }
            else
            {
                for (int i = 0; i < Seat.FeatureIDs.Count; i++)
                {
                    string reverseID = ManagerNS.LocalGameManager.Instance.FeatureManager.GetReverseID(Seat.FeatureIDs[i]);
                    if (!string.IsNullOrEmpty(reverseID))
                    {
                        Seat.FeatureIDs[i] = reverseID;
                    }
                }
            }
            IsCorrectEnd = true;
            OnCorrectEndEvent?.Invoke();
        }
        #endregion
    }
}