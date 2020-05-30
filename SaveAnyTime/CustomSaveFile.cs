namespace SaveAnyTime
{
    internal class CustomSaveFile
    {
        public string fileName;
        public string name;
        public string creationTime;
        public string saveTime;
        public string saveTimeFormatted;
        public string date;
        public string dateFormatted;
        public string playTime;
        public string GUID;
        public string version;
        public string hour;
        public string minute;
        public string sceneName;
        public string position;
        public string saveTitle;
        public bool auto;

        public CustomSaveFile(string _fileName)
        {
            //"name_20200218-113951_20200412-235250_1Y3M6D_21h32m16s_362be1aa-bc88-4db3-aee2-93faffe7cda2_Final 2.0.138271_7h0m_scene_position"
            try
            {
                this.fileName = _fileName;
                string[] list = fileName.Split('_');
                int i = 0;
                this.name = list[i++];
                this.creationTime = list[i++];
                this.saveTime = list[i++];
                this.date = list[i++];
                this.playTime = list[i++];
                this.GUID = list[i++];
                this.version = list[i++];
                this.hour = list[i].Split('h')[0];
                this.minute = list[i++].Split('h')[1].Split('m')[0];
                this.sceneName = list[i++];
                this.position = list[i++];
                this.auto = list[list.Length - 1] == "auto";
                this.saveTimeFormatted = string.Format("{0}-{1}-{2} at {3}:{4}:{5}", saveTime.Substring(6, 2), saveTime.Substring(4, 2), saveTime.Substring(0, 4), saveTime.Substring(9, 2), saveTime.Substring(11, 2), saveTime.Substring(13, 2));
                this.dateFormatted = string.Format("Year {0}, Month {1}, Day {2}", date.Split('Y')[0], date.Split('Y')[1].Split('M')[0], date.Split('Y')[1].Split('M')[1].Split('D')[0]);
                this.saveTitle = string.Format("{0} at {1}:{2:00} on {3} in {4} ({5}saved on {6})", name, hour, minute, dateFormatted, sceneName, (auto?"auto":""), saveTimeFormatted);
            }
            catch
            {
            }
         }
        
        public bool isValid()
        {
            return this.fileName != null && this.name != null && this.hour != null && this.minute != null && this.creationTime != null && this.saveTime != null && this.date != null && this.playTime != null && this.GUID != null && this.version != null && this.hour != null && this.minute != null && this.sceneName != null && this.position != null;
        }

    }
}