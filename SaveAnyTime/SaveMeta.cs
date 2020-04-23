using System.Collections.Generic;

namespace SaveAnyTime
{
    public class SaveMeta
    {
        public List<NPCMeta> NPClist;
        public List<RideableMeta> RideableList;
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
    }
}