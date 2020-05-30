using System.Collections.Generic;

namespace SaveAnyTime
{
    public class SaveMeta
    {
        public List<NPCMeta> NPClist;
        public List<RideableMeta> RideableList;
        public List<StoreMeta> StoreList;
        public int FishBowlConsumeHour = -1;
        public int WeatherState = -1;
        public float CurPriceIndex = 1f;
        public string playerRot;
    }
    public class NPCMeta
    {
        public int id;
        public string scene;
        public string pos;
    }
    public class RideableMeta
    {
        public int id;
        public string pos;
        public string state;
    }
    public class StoreMeta
    {
        public int id;
        public int money;
        public int recycleCount = 0;
    }

}