namespace SaveAnyTime
{
    internal class CustomSaveFile
    {
        public string fileName;
        public string name;
        public string creationTime;
        public string saveTime;
        public string date;
        public string playTime;
        public string GUID;
        public string version;
        public int hour;
        public int minute;
        public string sceneName;
        public string position;

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
                this.hour = int.Parse(list[i].Split('h')[0]);
                this.minute = int.Parse(list[i++].Split('h')[1].Split('m')[0]);
                this.sceneName = list[i++];
                this.position = list[i++];
            }
            catch
            {
            }
         }
        
        public bool isValid()
        {
            return this.fileName != null && this.name != null && this.hour != null && this.minute != null && this.creationTime != null && this.saveTime != null && this.date != null && this.playTime != null && this.GUID != null && this.version != null && this.hour != null && this.minute != null && this.sceneName != null && this.position != null;
        }

        public string GetSaveTitle()
        {
            return string.Format("{0} at {1}:{2:00} on {3} ({4} in {5})", name, hour, minute, date, saveTime, sceneName);
        }
    }
}