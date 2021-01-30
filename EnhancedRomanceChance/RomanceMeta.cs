using System.Collections.Generic;

namespace EnhancedRomanceChance
{
    public class RomanceMeta
    {
        public List<RomanceCounts> romances = new List<RomanceCounts>();
    }

    public class RomanceCounts
    {
        public int id;
        public int confessions = 0;
        public int proposals = 0;

        public RomanceCounts()
        { 
        }
        public RomanceCounts(int id, int which)
        {
            this.id = id;
            if (which == 7000041)
                confessions++;
            else
                proposals++;
        }
    }
}