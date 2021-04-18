using Pathea.StoreNs;

namespace CustomMerch
{
    internal class StoreItem
    {
        public int itemId;
        public int productId;
        public int count;
        public int currency;
        public DoubleInt exchange;
        public int[] requireMissions;
        public float chance;
        public int storeId;

        public StoreItem(int itemId, int productId, int count, int currency, string exchange, string requireMission, int storeId, float chance = 1f)
        {
            this.itemId = itemId;
            this.productId = productId;
            this.count = count;
            this.currency = currency;
            this.exchange = new DoubleInt(exchange, ':');
            string[] reqMissions = requireMission.Split(',');
            requireMissions = new int[reqMissions.Length];
            for (int i = 0; i < requireMissions.Length; i++)
            {
                requireMissions[i] = int.Parse(reqMissions[i]);
            }
            this.storeId = storeId;
            this.chance = chance;
        }
    }
}