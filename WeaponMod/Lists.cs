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

        private static Dictionary<int, string> storeNames = new Dictionary<int, string>()
        {
            {1,"Farm Store"},
            {2,"The Round Table"},
            {3,"Clothing Store"},
            {4,"Best Brother"},
            {5,"Total Tools"},
            {7,"Martha's Bakery"},
            {8,"Alice's Flower Shop"},
            {9,"Dr. Xu's Clinic"},
            {10,"Mysterious Man"},
            {11,"Emily's Food Stall"},
        };

    }
}