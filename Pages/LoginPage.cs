using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class LoginPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/login"; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            if(Helper.UserHelper.Authorized(client))
            {
                BaseFuncs.Show404(client);
                return false;
            }

            if (client.GetParam("openid.identity") == null)
            {
                client.Redirect("https://steamcommunity.com/openid/login/?openid.ns=http://specs.openid.net/auth/2.0&openid.mode=checkid_setup&openid.return_to=http://" + client.Host + "/login&openid.realm=http://" + client.Host + "&openid.ns.sreg=http://openid.net/extensions/sreg/1.1&openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select&openid.identity=http://specs.openid.net/auth/2.0/identifier_select");
                return false;
            }
            else
            {
                try
                {
                    using (WebClient WebClient = new WebClient())
                    {
                        string url = "https://steamcommunity.com/openid/login/?openid.sig=" + client.GetParam("openid.sig")
                            + "&openid.ns=" + client.GetParam("openid.ns")
                            + "&openid.mode=check_authentication";

                        foreach (string get in client.GetParam("openid.signed").Split(','))
                        {
                            url += "&openid." + get + "=" + client.GetParam("openid." + get);
                        }

                        if (!WebClient.DownloadString(url.Replace("+", "%2B")).Contains("true"))
                        {
                            client.Head.GetParams.Clear();
                            this.Init(client);
                        }
                        else
                        {
                            string[] pre = client.GetParam("openid.identity").Split('/');
                            if (pre.Length > 5)
                            {
                                ulong SteamID = ulong.Parse(pre[5]);
                                XUser user;
                                if (!Helper.UserHelper.Auth(SteamID, out user, client))
                                {
                                    client.Head.GetParams.Clear();
                                    this.Init(client);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.ConsoleLog(ex, ConsoleColor.Red);
                }
            }

            client.Redirect(Helper.GetReferer(client));
            return false;
        }
    }
}
