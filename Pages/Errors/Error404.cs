using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Errors
{
    public class Error404 : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.NotFound; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            client.HttpSend("404 ERROR!");
            return true;
        }
    }
}
