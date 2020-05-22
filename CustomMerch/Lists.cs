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

namespace CustomMerch
{
    public partial class Main
    {

        private static int productIds = 5200;

        private static List<NewItem> newItems = new List<NewItem>();

        private static List<StoreItem> storeItems = new List<StoreItem>();

        private static Dictionary<int, string> storeNames = new Dictionary<int, string>()
        {
            {1,"Farm Store"},
            {2,"The Round Table"},
            {3,"Clothing Store"},
            {4,"Best Brother"},
            {5,"Total Tools"},
            {7,"Martha's Bakery"},
            {8,"Alice's Flower Shop"},
            {9,"Clinic Shop"},
            {10,"Mysterious Man"},
            {11,"Emily's Food Stall"},
            {12,"Church Store"},
            {13,"Badge Exchange"},
            {14,"A&G Construction Store"},
            {15,"Tody's Fish-o-Rama"},
            {16,"McD's Jumpin' Livestock"},
            {20,"South Block Trade Store"},
            {21,"Harbor Trade Station"},
            {22,"Gift Exchange"},
            {23,"Church Store"},
            {24,"Research Center Exchange"},
            {25,"Oaks' Handicrafts"},
            {26,"McBurger"}
        };

    }
}