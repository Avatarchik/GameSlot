using GameSlot.Database;
using GameSlot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class LogoutPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/logout"; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            XUser user;
            if (Helper.UserHelper.GetCurrentUser(client, out user))
            {
                client.Session.Clear();
                client.Redirect("/");
            }
            else
            {
                UpServer.BaseFuncs.Show404(client);
            }
            return false;
        }
    }
}
