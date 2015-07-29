using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class MainPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/"; }
        }
        public override string TemplateAddr
        {
            get { return "Main.html"; }
        }
        public override bool Init(Client client)
        {
            client.HttpSend(TemplateActivator.Activate(this, client));
            return true;
        }
    }
}
