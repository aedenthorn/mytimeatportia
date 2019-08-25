using Harmony12;
using Pathea;
using Pathea.ItemSystem;
using Pathea.Missions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleCommerce
{
    public static partial class Main
    {
        private static int[] FishInts = { 4000244, 4000248, 4000249, 4000247, 4000250, 4000253, 4000245, 4000255, 4000252, 4000254, 4000256 };

        [HarmonyPatch(typeof(RequirementData), "LoadDataBase", new Type[] { })]
        static class RequirementData_LoadDataBase_Patch
        {
            static void Postfix()
            {
                int index = 42000;
                foreach (int f in FishInts)
                {
                    RequirementData.refDataDic.Add(index, new RequirementData(index++, f, new DoubleInt(1, 1), new DoubleInt(2000, 2000), new DoubleInt(150, 150), new DoubleInt(70, 70), new DoubleInt(85, 85), 5, -1, 100, 10, new List<int> { }, new List<int> { }));
                }
            }
        }

        private static void Junk()
        {
            SqliteDataReader sqliteDataReader = LocalDb.cur.ReadFullTable("Item_equipment");
            while (sqliteDataReader.Read())
            {
                int @int = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("orderId"));
                string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemReq"));
                string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("gold"));
                string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("exp"));
                string string4 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("relationship"));
                string string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("workshoppt"));
                string string6 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("level"));
                string[] array = string6.Split(new char[]
                {
                    '_'
                });
                int num = int.Parse(array[0]);
                int num2 = -1;
                if (array.Length > 1)
                {
                    num2 = int.Parse(array[1]);
                }
                int int2 = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("weight"));
                int int3 = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("deadline"));
                string[] array2 = @string.Split(new char[]
                {
                    '_'
                });
                int num3 = int.Parse(array2[0]);
                DoubleInt doubleInt = DoubleInt.ParseDoubleId(array2[1], '-');
                List<int> list = new List<int>();
                List<int> list2 = new List<int>();
                string string7 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("season"));
                string string8 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("weather"));
                if (!string.IsNullOrEmpty(string7) && string7 != "-1")
                {
                    string[] array3 = string7.Split(new char[]
                    {
                        ','
                    });
                    foreach (string s in array3)
                    {
                        list.Add(int.Parse(s));
                    }
                }
                if (!string.IsNullOrEmpty(string8) && string8 != "-1")
                {
                    string[] array5 = string8.Split(new char[]
                    {
                        ','
                    });
                    foreach (string s2 in array5)
                    {
                        list2.Add(int.Parse(s2));
                    }
                }
                DoubleInt di2 = DoubleInt.ParseDoubleId(string2, '-');
                DoubleInt di3 = DoubleInt.ParseDoubleId(string3, '-');
                DoubleInt di4 = DoubleInt.ParseDoubleId(string4, '-');
                DoubleInt di5 = DoubleInt.ParseDoubleId(string5, '-');
                Dbgl(@int + " " + ItemDataMgr.Self.GetItemName(num3) + ": RequirementData.refDataDic.Add(index, reqDatas[index] = new RequirementData(index++," + num3 + ", new DoubleInt(" + doubleInt.id0 + "," + doubleInt.id1 + "), new DoubleInt(" + di2.id0 + "," + di2.id1 + "), new DoubleInt(" + di3.id0 + "," + di3.id1 + "), new DoubleInt(" + di4.id0 + "," + di4.id1 + "), new DoubleInt(" + di5.id0 + "," + di5.id1 + ")," + num + "," + num2 + "," + int2 + "," + int3 + ", new int[]{" + string.Join(",", list.Select(x => x.ToString()).ToArray()) + "},new int[]{" + string.Join(",", list2.Select(x => x.ToString()).ToArray()) + "}));",false);
            }
        }
    }
}