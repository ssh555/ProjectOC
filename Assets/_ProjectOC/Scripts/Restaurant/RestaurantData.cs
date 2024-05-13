namespace ProjectOC.RestaurantNS
{
    public struct RestaurantData : System.Collections.Generic.IComparer<RestaurantData>
    {
        public string ID;
        public int Index;
        public FoodPriority Priority;
        public bool HaveFood;
        public int AlterAP;

        public RestaurantData(string itemID, int index, int amount)
        {
            ID = ManagerNS.LocalGameManager.Instance.RestaurantManager.ItemIDToFoodID(itemID);
            Index = index;
            Priority = index == 0 ? FoodPriority.No1 : FoodPriority.None;
            Priority = index == 1 ? FoodPriority.No2 : FoodPriority.None;
            HaveFood = amount > 0;
            AlterAP = ManagerNS.LocalGameManager.Instance.RestaurantManager.Food_AlterAP(ID);
        }

        public int Compare(RestaurantData x, RestaurantData y)
        {
            // 1.��ʳ�������ǰ��
            if (x.HaveFood != y.HaveFood)
            {
                return y.HaveFood.CompareTo(x.HaveFood);
            }
            // 2.���ȼ��������ǰ��
            if (x.Priority != y.Priority)
            {
                return y.Priority.CompareTo(x.Priority);
            }
            // 3.�������ֵС������ǰ��
            if (x.AlterAP != y.AlterAP)
            {
                return x.AlterAP.CompareTo(y.AlterAP);
            }
            // 4.IDС������ǰ��
            string idx = x.ID ?? "";
            string idy = y.ID ?? "";
            return idx.CompareTo(idy);
        }
    }
}
