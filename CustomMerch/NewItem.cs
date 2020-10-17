using Pathea.ItemSystem;
using SimpleJSON;

namespace CustomMerch
{
    internal class NewItem
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

		public NewItem(JSONNode node)
		{
			Main.Dbgl("1");
			name = node["name"];
			description = node["description"];
			effect = node["effect"];
			buyPrice = node["buyPrice"].AsInt;
			sellPrice = new DoubleInt(node["sellPrice"],':');
			iconPath = node["iconPath"];
			modelPath = node["modelPath"];
			dropModelPath = node["dropModelPath"];
			displayScale = node["displayScale"].AsFloat;
			orderIndex = node["orderIndex"].AsInt;
			
			source = node["source"];

			intendType = node["intendType"].AsInt;

			string temp = node["skillIds"];
			string[] skillTemp = temp.Split(',');
			if (skillTemp.Length > 0)
			{
				skillIds = new int[skillTemp.Length];
				for (int i = 0; i < skillTemp.Length; i++)
				{
					skillIds[i] = int.Parse(skillTemp[i]);
				}
			}

			attack = node["attack"].AsInt;
			defense = node["defense"].AsInt;
			critical = node["critical"].AsFloat;
			antiCritical = node["antiCritical"].AsFloat;
			hpMax = node["hpMax"].AsFloat;
			cpMax = node["cpMax"].AsFloat;
			alwaysOnHand = node["alwaysOnHand"].AsBool;
			holdInBothHands = node["holdInBothHands"].AsBool;
			digGridCount = node["digGridCount"].AsFloat;
			digIntensity = node["digIntensity"].AsFloat;
			rate = node["rate"].AsFloat;
			meleeCriticalAmount = node["meleeCriticalAmount"].AsFloat;

			storeId = node["storeId"].AsInt;
			chance = node["chance"].AsInt;
		}

	}
}