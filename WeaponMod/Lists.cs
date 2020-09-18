using Harmony12;
using Pathea;
using Pathea.DungeonModuleNs;
using Pathea.ItemDropNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.StoreNs;
using Pathea.TreasureRevealerNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace WeaponMod
{
    public partial class Main
    {

        private static int productIds = 4200;

        private static List<WeaponItem> weaponItems = new List<WeaponItem>();
        
        private static List<Weapon> weapons = new List<Weapon>();

        private static List<int> storeIds = new List<int>()
        {
            1,
            2,
            3,
            4,
            5,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            20,
            21,
            22,
            23,
            24,
            25,
            26,
        };
            
        public static List<string> storeNames = new List<string>()
        {
            "Farm Store",
            "The Round Table",
            "Clothing Store",
            "Best Brother",
            "Total Tools",
            "Martha's Bakery",
            "Alice's Flower Shop",
            "Clinic Shop",
            "Mysterious Man",
            "Emily's Food Stall",
            "Church Store",
            "Badge Exchange",
            "A&G Construction Store",
            "Tody's Fish-o-Rama",
            "McD's Jumpin' Livestock",
            "South Block Trade Store",
            "Harbor Trade Station",
            "Gift Exchange",
            "Church Store",
            "Research Center Exchange",
            "Oaks' Handicrafts",
            "McBurger"
        };

    }
}