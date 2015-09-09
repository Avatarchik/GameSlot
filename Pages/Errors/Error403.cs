using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Errors
{
    public class Error403 : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.NotAllowed; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            client.Redirect("/");
            return true;
        }
    }
}
