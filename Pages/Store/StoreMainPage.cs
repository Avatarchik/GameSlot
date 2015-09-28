using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Store
{
    public class StoremainPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/shop"; }
        }
        public override string TemplateAddr
        {
            get { return "Store.StoreMain.html"; }
        }
        public override bool Init(Client client)
        {

            Dictionary<uint, USteamItem> StoreItems = new Dictionary<uint,USteamItem>();
            int ElementsOnPage = 2;

            uint SteamGameID = Configs.DOTA2_STEAM_GAME_ID;
            if(client.GetParam("game") != null && client.GetParam("game").Equals(Configs.CSGO_STEAM_GAME_ID.ToString()))
            {
                SteamGameID = Configs.CSGO_STEAM_GAME_ID;
            }

            foreach (USteamItem SteamItem in Helper.UserHelper.GetSteamLocalInventory(Configs.ADMIN_ACCOUNT, SteamGameID, false, 1))
            {
                if(StoreItems.ContainsKey(SteamItem.ID))
                {
                    StoreItems[SteamItem.ID].Num++;
                }
                else
                {
                    SteamItem.Num++;
                    SteamItem.ShopPrice = SteamItem.Price - (SteamItem.Price / 100 * 40);
                    SteamItem.ShopPrice_Str = SteamItem.ShopPrice.ToString("###,###,##0");
                    StoreItems.Add(SteamItem.ID, SteamItem);
                }
            }

            Dictionary<uint, USteamItem> StoreItemsData = new Dictionary<uint, USteamItem>();
            int Page = 1;
            if (client.GetParam("page") != null)
            {
                if(int.TryParse(client.GetParam("page"), out Page))
                {
                    if (Page > StoreItems.Count / ElementsOnPage)
                    {
                        Page = StoreItems.Count / ElementsOnPage;
                    }
                }
            }

            if(Page <= 0)
            {
                Page = 1;
            }

            int count = 0;
            foreach(uint Key in StoreItems.Keys)
            {
                if(count < (Page -1) * ElementsOnPage)
                {
                    count++;
                    continue;
                }

                StoreItemsData.Add(Key, StoreItems[Key]);         
                count++;

                if (count >= Page * ElementsOnPage)
                {
                    break;
                }
            }

            Hashtable data = new Hashtable();
            data.Add("Title", "Магазин GAMESLOT");
            data.Add("StoreItems", StoreItemsData);
            data.Add("Page", Page);
            data.Add("ItemsNum", StoreItems.Count);
            data.Add("ElementsOnPage", ElementsOnPage);
            data.Add("Game", SteamGameID);
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
