using Harmony12;
using Pathea.ActorNs;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace ModelSwitcher
{
    public partial class Main
    {
        private static Dictionary<string, string> sortedIdToNames = new Dictionary<string, string>
        {
            {"4000038", "300054"}, // Aadit
            //{"4000002", "300018"}, // Ack
            //{"4000109", "300018"}, // Ack
            //{"4000110", "300018"}, // Ack
            {"4000026", "300042"}, // Albert
            {"4000145", "300493"}, // Albert Jr.
            {"4000050", "300070"}, // Alice
            {"4000121", "300137"}, // Allen Carter
            {"4000137", "300484"}, // Alliance Soldier
            {"4000138", "300484"}, // Alliance Soldier
            {"4000139", "300484"}, // Alliance Soldier
            {"4000009", "300025"}, // Antoine
            //{"4000063", "300090"}, // Arlo
            {"4000107", "300009"}, // Bandirat Prince
            {"4000120", "300133"}, // Builder Wang
            {"4000015", "300031"}, // Carol
            {"4000113", "300421"}, // Cent
            {"4000146", "300499"}, // DMTR 6000
            {"4000115", "300445"}, // Dana
            {"4000102", "300138"}, // Dawa
            {"4000011", "300027"}, // Django
            {"4000017", "300033"}, // Dolly
            //{"4000092", "300121"}, // Dr. Xu
            //{"4000003", "300019"}, // Emily
            {"4000103", "300139"}, // Erwa
            {"4000133", "300482"}, // Everglade
            {"4000134", "103840"}, // First Child
            {"4000008", "300024"}, // Gale
            //{"4000093", "300122"}, // Ginger
            //{"4000091", "300120"}, // Gust
            {"4000112", "300419"}, // Han
            {"4000097", "300126"}, // Higgins
            {"4000024", "300040"}, // Huss
            {"4000013", "300029"}, // Isaac
            {"4000098", "300127"}, // Jack
            {"4000055", "300075"}, // Lee
            {"4000106", "300142"}, // Liuwa
            {"4000053", "300073"}, // Lucy
            {"4000117", "300454"}, // Mali
            {"4000014", "300030"}, // Mars
            {"4000019", "300035"}, // Martha
            {"4000147", "300504"}, // Mason
            {"4000059", "300079"}, // McDonald
            {"4000052", "300072"}, // Mei
            {"4000099", "300091"}, // Merlin
            //{"4000111", "300143"}, // Mint
            {"4000016", "300032"}, // Molly
            {"4000118", "300457"}, // Musa
            {"4000101", "110036"}, // Mysterious Man
            {"4000006", "300022"}, // Nora
            {"4000004", "300020"}, // Oaks
            {"4000136", "300483"}, // Pa
            {"4000041", "300008"}, // Papa Bear
            {"4000100", "300104"}, // Paulie
            {"4000141", "103878"}, // Penny
            {"4000094", "300123"}, // Petra
            //{"4000035", "300051"}, // Phyllis
            {"4000069", "300097"}, // Pinky
            {"4000018", "300034"}, // Polly
            {"4000040", "300056"}, // Presley
            {"4000007", "300023"}, // QQ
            {"4000021", "300037"}, // Qiwa
            {"4000044", "300064"}, // Remington
            {"4000108", "300011"}, // Robot
            {"4000132", "111012"}, // Rogue Knight
            {"4000010", "300026"}, // Russo
            {"4000129", "300476"}, // Ryder
            //{"4000067", "300071"}, // Sam
            {"4000104", "300140"}, // Sanwa
            {"4000128", "300460"}, // Scraps
            {"4000135", "103841"}, // Second Child
            {"4000105", "300141"}, // Siwa
            {"4000033", "300049"}, // Sonia
            {"4000012", "300028"}, // Sophie
            {"4000119", "300135"}, // Sweet
            {"4000130", "300478"}, // Ten
            {"4000140", "91202621"}, // The All Source AI
            {"4000005", "300021"}, // Toby
            {"4000043", "300063"}, // Tody
            {"4000122", "101334"}, // Tourist
            {"4000123", "101334"}, // Tourist
            {"4000124", "101334"}, // Tourist
            {"4000125", "101334"}, // Tourist
            {"4000126", "101334"}, // Tourist
            {"4000127", "101334"}, // Tourist
            {"4000023", "300039"}, // Tuss
            {"4000131", "300474"}, // Ursula
            {"4000142", "103920"}, // Warthog
            {"4000116", "300448"}, // Workshop Rep
            {"4000054", "300074"}, // Wuwa
            {"4000114", "300439"}, // Yeye
            {"4000143", "103921"}, // Yoyo
            {"4000144", "103921"}, // Yoyo

        };
        private static Dictionary<string, string> idToModels = new Dictionary<string, string> // unique
        {
            {"4000038", "Actor/Npc_Aadit"}, // Aadit
            {"4000026", "Actor/Npc_Abert"}, // Albert
            {"4000145", "Actor/Albert_Baby"}, // Albert Jr.
            {"4000050", "Actor/Npc_Alice"}, // Alice
            {"4000121", "Actor/Npc_Alan"}, // Allen Carter
            {"4000137", "Actor/Npc_Officer"}, // Alliance Soldier
            {"4000138", "Actor/Npc_OfficerBlack"}, // Alliance Soldier
            {"4000009", "Actor/Npc_Antoine"}, // Antoine
            //{"4000063", "Actor/Npc_Arlo"}, // Arlo
            {"4000107", "Actor/Bandirat_Prince"}, // Bandirat Prince
            {"4000120", "Actor/Npc_Wang"}, // Builder Wang
            {"4000015", "Actor/Npc_Woman001"}, // Carol
            {"4000113", "Actor/Npc_Bainian"}, // Cent
            {"4000146", "Actor/MysteryRobot_0"}, // DMTR 6000
            {"4000115", "Actor/Npc_FatDina"}, // Dana
            {"4000102", "Actor/Npc_Rusty01"}, // Dawa
            {"4000011", "Actor/Npc_Django"}, // Django
            {"4000017", "Actor/Npc_littlegirl002"}, // Dolly
            //{"4000092", "Actor/Npc_DoctorXu"}, // Dr. Xu
            //{"4000003", "Actor/Npc_Emily"}, // Emily
            {"4000103", "Actor/Npc_Rusty02"}, // Erwa
            {"4000134", "Actor/Baby_1"}, // First Child
            {"4000008", "Actor/Npc_Gale"}, // Gale
            //{"4000093", "Actor/Npc_Ginger"}, // Ginger
            //{"4000091", "Actor/Npc_Gust"}, // Gust
            {"4000112", "Actor/Npc_Han"}, // Han
            {"4000097", "Actor/Npc_Higgins"}, // Higgins
            {"4000024", "Actor/Npc_Huss"}, // Huss
            {"4000013", "Actor/Npc_Issac"}, // Isaac
            {"4000098", "Actor/Npc_LittleBoy"}, // Jack
            {"4000055", "Actor/Npc_Lee"}, // Lee
            {"4000106", "Actor/Npc_Rusty06"}, // Liuwa
            {"4000053", "Actor/Npc_Woman003"}, // Lucy
            {"4000117", "Actor/Npc_Mary"}, // Mali
            {"4000014", "Actor/Npc_Mars"}, // Mars
            {"4000019", "Actor/Npc_Martha"}, // Martha
            {"4000147", "Actor/Npc_Mason"}, // Mason
            {"4000059", "Actor/Npc_FatOldMan"}, // McDonald
            {"4000052", "Actor/Npc_Mei"}, // Mei
            {"4000099", "Actor/Npc_Meilin"}, // Merlin
            //{"4000111", "Actor/Npc_Mint"}, // Mint
            {"4000016", "Actor/Npc_Polly"}, // Molly
            {"4000118", "Actor/Npc_Mutha"}, // Musa
            {"4000101", "Actor/Npc_Mystery_salesman"}, // Mysterious Man
            {"4000006", "Actor/Npc_Nora"}, // Nora
            {"4000004", "Actor/Npc_Oaks"}, // Oaks
            {"4000136", "Actor/Npc_Papa"}, // Pa
            {"4000041", "Actor/Npc_SleepyBear"}, // Papa Bear
            {"4000100", "Actor/Npc_Paul"}, // Paulie
            {"4000141", "Actor/Npc_Liuliu"}, // Penny
            {"4000094", "Actor/Npc_Petra"}, // Petra
            //{"4000035", "Actor/Npc_Phyllis"}, // Phyllis
            {"4000069", "Actor/Gale_cat"}, // Pinky
            {"4000018", "Actor/Npc_littlegirl003"}, // Polly
            {"4000040", "Actor/Npc_Presley"}, // Presley
            {"4000007", "Actor/Npc_QQ"}, // QQ
            {"4000021", "Actor/Npc_Rusty07"}, // Qiwa
            {"4000044", "Actor/Npc_Remington"}, // Remington
            {"4000108", "Actor/Npc_Robot"}, // Robot
            {"4000132", "Actor/Npc_Knight"}, // Rogue Knight
            {"4000010", "Actor/Npc_Russo"}, // Russo
            {"4000129", "Actor/Npc_Ryder"}, // Ryder
            //{"4000067", "Actor/Npc_Sam"}, // Sam
            {"4000104", "Actor/Npc_Rusty03"}, // Sanwa
            {"4000128", "Actor/NPC_Dog"}, // Scraps
            {"4000135", "Actor/Baby_2"}, // Second Child
            {"4000105", "Actor/Npc_Rusty04"}, // Siwa
            {"4000033", "Actor/Npc_Snoia"}, // Sonia
            {"4000012", "Actor/Npc_Sophie"}, // Sophie
            {"4000119", "Actor/Npc_Woman003_1"}, // Sweet
            {"4000130", "Actor/Npc_Ten"}, // Ten
            {"4000140", "Actor/Npc_Prophet"}, // The All Source AI
            {"4000005", "Actor/Npc_Toby"}, // Toby
            {"4000043", "Actor/Npc_man01"}, // Tody
            {"4000122", "Actor/Npc_TravelerMan01"}, // Tourist
            {"4000123", "Actor/Npc_TravelerMan02"}, // Tourist
            {"4000124", "Actor/Npc_TravelerMan03"}, // Tourist
            {"4000125", "Actor/Npc_TravelerMan04"}, // Tourist
            {"4000126", "Actor/Npc_TravelerWom01"}, // Tourist
            {"4000127", "Actor/Npc_Woman002_1"}, // Tourist
            {"4000023", "Actor/Npc_Tuss"}, // Tuss
            {"4000131", "Actor/Npc_Ursula"}, // Ursula
            {"4000142", "Actor/Npc_Yezhu"}, // Warthog
            {"4000116", "Actor/Npc_man02"}, // Workshop Rep
            {"4000054", "Actor/Npc_Rusty05"}, // Wuwa
            {"4000114", "Actor/Npc_Hulugram"}, // Yeye
            {"4000143", "Actor/Npc_Chuanshanjia"}, // Yoyo
        };
        private static Dictionary<string, string> defaultModelIdForId = new Dictionary<string, string> // not unique
        {
            //{"4000002", "4000002"}, // Ack
            //{"4000002", "4000002"}, // Ack
            //{"4000002", "4000002"}, // Ack
            //{"4000003", "4000003"}, // Emily
            {"4000004", "4000004"}, // Oaks
            {"4000005", "4000005"}, // Toby
            {"4000006", "4000006"}, // Nora
            {"4000007", "4000007"}, // QQ
            {"4000008", "4000008"}, // Gale
            {"4000009", "4000009"}, // Antoine
            {"4000010", "4000010"}, // Russo
            {"4000011", "4000011"}, // Django
            {"4000012", "4000012"}, // Sophie
            {"4000013", "4000013"}, // Isaac
            {"4000014", "4000014"}, // Mars
            {"4000015", "4000015"}, // Carol
            {"4000016", "4000016"}, // Molly
            {"4000017", "4000017"}, // Dolly
            {"4000018", "4000018"}, // Polly
            {"4000019", "4000019"}, // Martha
            {"4000021", "4000021"}, // Qiwa
            {"4000023", "4000023"}, // Tuss
            {"4000024", "4000024"}, // Huss
            {"4000026", "4000026"}, // Albert
            {"4000033", "4000033"}, // Sonia
            //{"4000035", "4000035"}, // Phyllis
            {"4000038", "4000038"}, // Aadit
            {"4000040", "4000040"}, // Presley
            {"4000041", "4000041"}, // Papa Bear
            {"4000043", "4000043"}, // Tody
            {"4000044", "4000044"}, // Remington
            {"4000050", "4000050"}, // Alice
            {"4000052", "4000052"}, // Mei
            {"4000053", "4000053"}, // Lucy
            {"4000054", "4000054"}, // Wuwa
            {"4000055", "4000055"}, // Lee
            {"4000059", "4000059"}, // McDonald
            //{"4000063", "4000063"}, // Arlo
            //{"4000067", "4000067"}, // Sam
            {"4000069", "4000069"}, // Pinky
            //{"4000091", "4000091"}, // Gust
            //{"4000092", "4000092"}, // Dr. Xu
            //{"4000093", "4000093"}, // Ginger
            {"4000094", "4000094"}, // Petra
            {"4000097", "4000097"}, // Higgins
            {"4000098", "4000098"}, // Jack
            {"4000099", "4000099"}, // Merlin
            {"4000100", "4000100"}, // Paulie
            {"4000101", "4000101"}, // Mysterious Man
            {"4000102", "4000102"}, // Dawa
            {"4000103", "4000103"}, // Erwa
            {"4000104", "4000104"}, // Sanwa
            {"4000105", "4000105"}, // Siwa
            {"4000106", "4000106"}, // Liuwa
            {"4000107", "4000107"}, // Bandirat Prince
            //{"4000107", "4000107"}, // Bandirat Prince
            //{"4000107", "4000107"}, // Bandirat Prince
            {"4000108", "4000108"}, // Robot
            //{"4000109", "4000109"}, // Ack
            //{"4000109", "4000109"}, // Ack
            //{"4000109", "4000109"}, // Ack
            //{"4000110", "4000110"}, // Ack
            //{"4000110", "4000110"}, // Ack
            //{"4000110", "4000110"}, // Ack
            //{"4000111", "4000111"}, // Mint
            {"4000112", "4000112"}, // Han
            {"4000113", "4000113"}, // Cent
            {"4000114", "4000114"}, // Yeye
            {"4000115", "4000115"}, // Dana
            {"4000116", "4000116"}, // Workshop Rep
            {"4000117", "4000117"}, // Mali
            {"4000118", "4000118"}, // Musa
            {"4000119", "4000119"}, // Sweet
            {"4000120", "4000120"}, // Builder Wang
            {"4000121", "4000121"}, // Allen Carter
            {"4000122", "4000122"}, // Tourist
            {"4000123", "4000123"}, // Tourist
            {"4000124", "4000124"}, // Tourist
            {"4000125", "4000125"}, // Tourist
            {"4000126", "4000126"}, // Tourist
            {"4000127", "4000127"}, // Tourist
            {"4000128", "4000128"}, // Scraps
            {"4000129", "4000129"}, // Ryder
            {"4000130", "4000130"}, // Ten
            {"4000131", "4000131"}, // Ursula
            {"4000132", "4000132"}, // Rogue Knight
            {"4000133", "4000131"}, // Everglade
            {"4000134", "4000134"}, // First Child
            {"4000135", "4000135"}, // Second Child
            {"4000136", "4000136"}, // Pa
            {"4000137", "4000137"}, // Alliance Soldier
            {"4000138", "4000138"}, // Alliance Soldier
            {"4000139", "4000138"}, // Alliance Soldier
            {"4000140", "4000140"}, // The All Source AI
            {"4000141", "4000141"}, // Penny
            {"4000142", "4000142"}, // Warthog
            {"4000143", "4000143"}, // Yoyo
            {"4000144", "4000143"}, // Yoyo
            {"4000145", "4000145"}, // Albert Jr.
            {"4000146", "4000146"}, // DMTR 6000
            {"4000147", "4000147"}, // Mason

        };
        private static Dictionary<string,string> modelToIds = new Dictionary<string,string> //unique 
        {
            {"Actor/Npc_Aadit", "4000038"}, // Aadit
            {"Actor/Npc_Abert", "4000026"}, // Albert
            {"Actor/Albert_Baby", "4000145"}, // Albert Jr.
            {"Actor/Npc_Alice", "4000050"}, // Alice
            {"Actor/Npc_Alan", "4000121"}, // Allen Carter
            {"Actor/Npc_Officer", "4000137"}, // Alliance Soldier
            {"Actor/Npc_OfficerBlack", "4000138"}, // Alliance Soldier
            {"Actor/Npc_Antoine", "4000009"}, // Antoine
            //{"Actor/Npc_Arlo", "4000063"}, // Arlo
            {"Actor/Bandirat_Prince", "4000107"}, // Bandirat Prince
            {"Actor/Npc_Wang", "4000120"}, // Builder Wang
            {"Actor/Npc_Woman001", "4000015"}, // Carol
            {"Actor/Npc_Bainian", "4000113"}, // Cent
            {"Actor/MysteryRobot_0", "4000146"}, // DMTR 6000
            {"Actor/Npc_FatDina", "4000115"}, // Dana
            {"Actor/Npc_Rusty01", "4000102"}, // Dawa
            {"Actor/Npc_Django", "4000011"}, // Django
            {"Actor/Npc_littlegirl002", "4000017"}, // Dolly
            //{"Actor/Npc_DoctorXu", "4000092"}, // Dr. Xu
            //{"Actor/Npc_Emily", "4000003"}, // Emily
            {"Actor/Npc_Rusty02", "4000103"}, // Erwa
            {"Actor/Baby_1", "4000134"}, // First Child
            {"Actor/Npc_Gale", "4000008"}, // Gale
            //{"Actor/Npc_Ginger", "4000093"}, // Ginger
            //{"Actor/Npc_Gust", "4000091"}, // Gust
            {"Actor/Npc_Han", "4000112"}, // Han
            {"Actor/Npc_Higgins", "4000097"}, // Higgins
            {"Actor/Npc_Huss", "4000024"}, // Huss
            {"Actor/Npc_Issac", "4000013"}, // Isaac
            {"Actor/Npc_LittleBoy", "4000098"}, // Jack
            {"Actor/Npc_Lee", "4000055"}, // Lee
            {"Actor/Npc_Rusty06", "4000106"}, // Liuwa
            {"Actor/Npc_Woman003", "4000053"}, // Lucy
            {"Actor/Npc_Mary", "4000117"}, // Mali
            {"Actor/Npc_Mars", "4000014"}, // Mars
            {"Actor/Npc_Martha", "4000019"}, // Martha
            {"Actor/Npc_Mason", "4000147"}, // Mason
            {"Actor/Npc_FatOldMan", "4000059"}, // McDonald
            {"Actor/Npc_Mei", "4000052"}, // Mei
            {"Actor/Npc_Meilin", "4000099"}, // Merlin
            //{"Actor/Npc_Mint", "4000111"}, // Mint
            {"Actor/Npc_Polly", "4000016"}, // Molly
            {"Actor/Npc_Mutha", "4000118"}, // Musa
            {"Actor/Npc_Mystery_salesman", "4000101"}, // Mysterious Man
            {"Actor/Npc_Nora", "4000006"}, // Nora
            {"Actor/Npc_Oaks", "4000004"}, // Oaks
            {"Actor/Npc_Papa", "4000136"}, // Pa
            {"Actor/Npc_SleepyBear", "4000041"}, // Papa Bear
            {"Actor/Npc_Paul", "4000100"}, // Paulie
            {"Actor/Npc_Liuliu", "4000141"}, // Penny
            {"Actor/Npc_Petra", "4000094"}, // Petra
            //{"Actor/Npc_Phyllis", "4000035"}, // Phyllis
            {"Actor/Gale_cat", "4000069"}, // Pinky
            {"Actor/Npc_littlegirl003", "4000018"}, // Polly
            {"Actor/Npc_Presley", "4000040"}, // Presley
            {"Actor/Npc_QQ", "4000007"}, // QQ
            {"Actor/Npc_Rusty07", "4000021"}, // Qiwa
            {"Actor/Npc_Remington", "4000044"}, // Remington
            {"Actor/Npc_Robot", "4000108"}, // Robot
            {"Actor/Npc_Knight", "4000132"}, // Rogue Knight
            {"Actor/Npc_Russo", "4000010"}, // Russo
            {"Actor/Npc_Ryder", "4000129"}, // Ryder
            //{"Actor/Npc_Sam", "4000067"}, // Sam
            {"Actor/Npc_Rusty03", "4000104"}, // Sanwa
            {"Actor/NPC_Dog", "4000128"}, // Scraps
            {"Actor/Baby_2", "4000135"}, // Second Child
            {"Actor/Npc_Rusty04", "4000105"}, // Siwa
            {"Actor/Npc_Snoia", "4000033"}, // Sonia
            {"Actor/Npc_Sophie", "4000012"}, // Sophie
            {"Actor/Npc_Woman003_1", "4000119"}, // Sweet
            {"Actor/Npc_Ten", "4000130"}, // Ten
            {"Actor/Npc_Prophet", "4000140"}, // The All Source AI
            {"Actor/Npc_Toby", "4000005"}, // Toby
            {"Actor/Npc_man01", "4000043"}, // Tody
            {"Actor/Npc_TravelerMan01", "4000122"}, // Tourist
            {"Actor/Npc_TravelerMan02", "4000123"}, // Tourist
            {"Actor/Npc_TravelerMan03", "4000124"}, // Tourist
            {"Actor/Npc_TravelerMan04", "4000125"}, // Tourist
            {"Actor/Npc_TravelerWom01", "4000126"}, // Tourist
            {"Actor/Npc_Woman002_1", "4000127"}, // Tourist
            {"Actor/Npc_Tuss", "4000023"}, // Tuss
            {"Actor/Npc_Ursula", "4000131"}, // Ursula
            {"Actor/Npc_Yezhu", "4000142"}, // Warthog
            {"Actor/Npc_man02", "4000116"}, // Workshop Rep
            {"Actor/Npc_Rusty05", "4000054"}, // Wuwa
            {"Actor/Npc_Hulugram", "4000114"}, // Yeye
            {"Actor/Npc_Chuanshanjia", "4000143"}, // Yoyo
        };
    }
}
