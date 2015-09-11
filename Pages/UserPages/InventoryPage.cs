using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.UserPages
{
    public class InventoryPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/inventory/"; }
        }
        public override string TemplateAddr
        {
            get { return "UserPages.Inventory.html"; }
        }
        public override bool Init(Client client)
        {
            XUser User;
            if (Helper.UserHelper.GetCurrentUser(client, out User))
            {
                uint SteamGameID = 0;
                string Action = BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0];

                if (Action.Equals("dota2"))
                {
                    SteamGameID = Configs.DOTA2_STEAM_GAME_ID;
                }
                else if (Action.Equals("csgo"))
                {
                    SteamGameID = Configs.CSGO_STEAM_GAME_ID;
                }
                else
                {
                    BaseFuncs.Show404(client);
                    return false;
                }

                List<Chip> Chips = Helper.UserHelper.GetChipInventory(User.ID);

                int f5 = 0 , f10 = 0, f25=0, f50=0, f100 = 0;
                foreach(Chip chip in Chips)
                {
                    switch(chip.ID)
                    {
                        case 0:
                            f5++;
                            break;
                        case 1:
                            f10++;
                            break;
                        case 2:
                            f25++;
                            break;
                        case 3:
                            f50++;
                            break;
                        case 4:
                            f100++;
                            break;
                    }                       
                }

                Hashtable data = new Hashtable();
                data.Add("SteamGameID", SteamGameID);
                data.Add("Chips", Helper.UserHelper.GetChipInventory(User.ID));
                data.Add("Title", "Мой инвентарь");
                data.Add("User", User);

                data.Add("f5", f5);
                data.Add("f10", f10);
                data.Add("f25", f25);
                data.Add("f50", f50);
                data.Add("f100", f100);

                client.HttpSend(TemplateActivator.Activate(this, client, data));
                return true;
            }

            BaseFuncs.Show403(client);
            return false;
        }
    }
}
