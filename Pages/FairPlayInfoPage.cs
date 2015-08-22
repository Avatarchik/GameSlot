using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class FairPlayInfoPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/fair-play"; }
        }
        public override string TemplateAddr
        {
            get { return "FairPlay.html"; }
        }
        public override bool Init(Client client)
        {
            Hashtable data = new Hashtable();
            data.Add("Title", "Принцип честной игры GAMESLOT");
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
