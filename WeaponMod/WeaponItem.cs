using Pathea.ItemSystem;
using SimpleJSON;

namespace WeaponMod
{
    internal class WeaponItem
    {
		public string name;
		public string description;
		public string effect;
		public int buyPrice;
		public DoubleInt sellPrice;
		public string iconPath = "Sprites/Package/Item_weapon_001";
		public string modelPath = "Weapon/Marco_Weapon_Mujian:R_Hand";
		public string dropModelPath = "ItemMall/Gift/ItemGift_blade_0";
		public float displayScale = 1f;
		public int orderIndex = 11016;
		public string source;

		public int intendType;
		public int[] skillIds;
		public int attack = 90;
		public int defense = 0;
		public float critical = 1;
		public float antiCritical = 0;
		public float hpMax = 0;
		public float cpMax = 0;
		public bool alwaysOnHand;
		public bool holdInBothHands;
		public float digGridCount = 0;
		public float digIntensity = 0;
		public float rate = 1;
		public float meleeCriticalAmount = 0.5f;

		public int storeId;
		public int chance;

		public WeaponItem(JSONNode node)
		{
			Main.Dbgl("1");
			this.name = node["name"];
			this.description = node["description"];
			this.effect = node["effect"];
			this.buyPrice = node["buyPrice"].AsInt;
			this.sellPrice = new DoubleInt(node["sellPrice"],':');
			this.iconPath = node["iconPath"];
			this.modelPath = node["modelPath"];
			this.dropModelPath = node["dropModelPath"];
			this.displayScale = node["displayScale"].AsFloat;
			this.orderIndex = node["orderIndex"].AsInt;
			
			this.source = node["source"];

			this.intendType = node["intendType"].AsInt;

			string temp = node["skillIds"];
			string[] skillTemp = temp.Split(',');
			if (skillTemp.Length > 0)
			{
				this.skillIds = new int[skillTemp.Length];
				for (int i = 0; i < skillTemp.Length; i++)
				{
					this.skillIds[i] = int.Parse(skillTemp[i]);
				}
			}

			this.attack = node["attack"].AsInt;
			this.defense = node["defense"].AsInt;
			this.critical = node["critical"].AsFloat;
			this.antiCritical = node["antiCritical"].AsFloat;
			this.hpMax = node["hpMax"].AsFloat;
			this.cpMax = node["cpMax"].AsFloat;
			this.alwaysOnHand = node["alwaysOnHand"].AsBool;
			this.holdInBothHands = node["holdInBothHands"].AsBool;
			this.digGridCount = node["digGridCount"].AsFloat;
			this.digIntensity = node["digIntensity"].AsFloat;
			this.rate = node["rate"].AsFloat;
			this.meleeCriticalAmount = node["meleeCriticalAmount"].AsFloat;

			this.storeId = node["storeId"].AsInt;
			this.chance = node["chance"].AsInt;
		}

	}
}