using Hont;
using Pathea.MailNs;
using Pathea.ModuleNs;
using System.Collections.Generic;
using static Harmony12.AccessTools;

namespace ShippingBin
{
    public class MailShipping : MailGifts
    {
        public MailShipping() 
        {
        }
        public MailShipping(int gols)
        {
            money = gols;
            Type = MailType.Gift;
            itemList = new List<IdCount>();
            GameDateTime gdt = Module<TimeManager>.Self.DateTime;
            GameDateTime morning = new GameDateTime(gdt.Year, gdt.Month, gdt.Day + 1, 7, 0, 0);
            sendDateTicks = morning.Ticks;

            baseData = new Pathea.MailConfInfo
            {
                Title = 103556,
                Content = new int[] { 100777 },
                Type = (int)MailType.Gift,
            };
            typeof(MailItemBase).GetProperty("titleId").SetValue(this, baseData.Title, null);
            typeof(MailItemBase).GetProperty("contentId").SetValue(this, baseData.GetContent(), null);
			mailIcon = "Mail/Mail3";
		}

        public override string GetContent()
        {
            string text = "<b>"+TextMgr.GetStr(103556, -1) + "</b>\r\n\r\n" + TextMgr.GetStr(contentId, -1) + "\r\n\r\n"+money;

            return text;
        }
    }
}