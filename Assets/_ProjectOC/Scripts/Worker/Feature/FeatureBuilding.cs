using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [LabelText("ß÷ß÷ÎÑ")]
    public class FeatureBuilding : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        #region Data Init Destroy
        private System.Random Random = new System.Random();
        [LabelText("ß÷ß÷ÎÑ×ùÎ»"), ShowInInspector, ReadOnly]
        private FeatureSeat[] Seats = new FeatureSeat[2];
        public bool IsExchange => timer != null && !timer.IsStoped;
        public void Init()
        {
            for (int i = 0; i < Seats.Length; i++)
            {
                Seats[i] = new FeatureSeat(this, transform.Find($"seat{i + 1}"));
            }
        }
        public void OnDestroy()
        {
            foreach (var seat in Seats)
            {
                (seat as IWorkerContainer).RemoveWorker();
            }
        }
        #endregion

        #region BuildingPart IInteraction
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
                foreach (var seat in Seats)
                {
                    (seat as IWorkerContainer).RemoveWorker();
                }
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

        #region ´ÊÌõ´«ÊÚ
        [NonSerialized]
        private ML.Engine.Timer.CounterDownTimer timer;
        [LabelText("¼ÆÊ±Æ÷")]
        public ML.Engine.Timer.CounterDownTimer Timer
        {
            get
            {
                if (timer == null && ManagerNS.LocalGameManager.Instance != null)
                {
                    timer = new ML.Engine.Timer.CounterDownTimer
                        (ManagerNS.LocalGameManager.Instance.FeatureManager.Config.FeatTransTime, false, false);
                    timer.OnEndEvent += EndActionForTimer;
                }
                return timer;
            }
        }
        public bool CanExhcange()
        {
            bool flag1 = Seats[0].IsArrive && Seats[1].IsArrive;
            var Config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
            bool flag2 = !ManagerNS.LocalGameManager.Instance.Player.InventoryHaveItem(Config.FeatTransCostItemID, Config.FeatTransCostItemNum);
            bool flag3 = Seats[0].FeatureIDs.Count == 0 && Seats[1].FeatureIDs.Count == 0;
            bool flag4 = Seats[0].FeatureIDs.Count == 3 && Seats[1].FeatureIDs.Count == 3;
            return flag1 || flag2 || flag3 || flag4;
        }
        public void ChangeWorker(int index, Worker worker)
        {
            if (0 > index && index > 1) { return; }
            (Seats[index] as IWorkerContainer).SetWorker(worker);
        }
        public void ExchangeFeature()
        {
            if (CanExhcange() && !IsExchange)
            {
                var Config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
                ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(Config.FeatTransCostItemID, Config.FeatTransCostItemNum, priority:-1);
                Timer.Start();
            }
        }
        public void ConfirmExchange()
        {
            Seats[0].ChangerWorkerFeature();
            Seats[1].ChangerWorkerFeature();
        }
        public void CancleExchange()
        {
            if (IsExchange)
            {
                Seats[0].SetFeatureID();
                Seats[1].SetFeatureID();
                Timer.End();
                var config = ManagerNS.LocalGameManager.Instance.FeatureManager.Config;
                ManagerNS.LocalGameManager.Instance.Player.InventoryAddItems(config.FeatTransCostItemID, config.FeatTransCostItemNum);
            }
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
                Seats[0].FeatureIDs.Add(set2.ToList()[Random.Next(0, set2.Count + 1)]);
            }
            if (cnt2 < 3 && set1.Count > 0)
            {
                Seats[1].FeatureIDs.Add(set1.ToList()[Random.Next(0, set1.Count + 1)]);
            }
        }
        #endregion

        #region ´ÊÌõÐÞÕý

        #endregion
    }
}