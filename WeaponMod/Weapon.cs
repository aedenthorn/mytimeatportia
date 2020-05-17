using Pathea.StoreNs;

namespace WeaponMod
{
    internal class Weapon
    {
        public int itemId;
        public int productId;
        public int count;
        public int currency;
        public DoubleInt exchange = new DoubleInt("-1",':');
        public int[] requireMissions;
        public float chance;
        public int storeId;

        public Weapon(int itemId, int productId, int count, int currency, string requireMission, int storeId, float chance = 1f)
        {
            this.itemId = itemId;
            this.productId = productId;
            this.count = count;
            this.currency = currency;
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