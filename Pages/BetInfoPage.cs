using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class BetInfoPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/bet-info"; }
        }
        public override string TemplateAddr
        {
            get { return "BetInfo.html"; }
        }
        public override bool Init(Client client)
        {
            Hashtable data = new Hashtable();
            data.Add("Title", "Как поставить вещи в GAMESLOT");
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
