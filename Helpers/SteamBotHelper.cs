using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;
using XData;

namespace GameSlot.Helpers
{
    public class SteamBotHelper
    {
        public XTable<XSteamBot> Table = new XTable<XSteamBot>();
        public XTable<XSteamBotProcessItems> Table_Items = new XTable<XSteamBotProcessItems>();

        public XTable<XBotOffersItem> Table_BotOffersItem = new XTable<XBotOffersItem>();
        public XTable<XBotsOffer> Table_BotsOffer = new XTable<XBotsOffer>();

        public SteamBotHelper()
        {
            new Thread(delegate()
            {
                try
                {
                    new UTSteam();
                }
                catch (Exception ex)
                {
                    Logger.ConsoleLog(ex);
                }
            }).Start();        
        }

        public bool GetFreeBot(out XSteamBot XSteamBot)
        {
            XSteamBot = new XSteamBot();
            List<XSteamBot> steambots = this.Table.SelectAll();

            if (steambots.Count > 0)
            {
                int count = 0;
                foreach (XSteamBot Bot in steambots)
                {
                    List<XSteamBotProcessItems> processes;

                    if (this.Table_Items.Select(data => data.SteamBotID == Bot.ID && data.Status > 0, out processes))
                    {
                        if (processes.Count > count)
                        {
                            count = processes.Count;
                            XSteamBot = Bot;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public bool SelectByOfferID(ulong OfferID, out XSteamBotProcessItems XSteamBotProcessItem)
        {
            XSteamBotProcessItem = new XSteamBotProcessItems();
            return this.Table_Items.SelectOne(data => data.OfferID == OfferID, out XSteamBotProcessItem);
        }

        public void ErrorToSendLocalItem(ulong SteamUserID, bool to_admin = false)
        {
            XUser user;
            if (Helper.UserHelper.Table.SelectOne(data => data.SteamID == SteamUserID, out user))
            {
                XBotsOffer XBotsOffer;
                if (Table_BotsOffer.SelectOne(data => data.SteamUserID == SteamUserID && data.Status == 0, out XBotsOffer))
                {
                    XBotOffersItem[] XBotOffersItems;
                    if(Table_BotOffersItem.SelectArr(data => data.BotsOfferID == XBotsOffer.ID, out XBotOffersItems))
                    {
                        for(int i = 0; i < XBotOffersItems.Length; i++)
                        {
                            XSItemUsersInventory XSItemUsersInventory = new XSItemUsersInventory();
                            XSItemUsersInventory.UserID = (!to_admin) ? user.ID : Configs.ADMIN_ACCOUNT;
                            XSItemUsersInventory.SteamItemID = XBotOffersItems[i].SteamItemID;
                            XSItemUsersInventory.AssertID = XBotOffersItems[i].AssertID;
                            XSItemUsersInventory.SteamGameID = Helper.SteamItemsHelper.Table.SelectByID(XBotOffersItems[i].SteamItemID).SteamGameID;
                            XSItemUsersInventory.SteamBotID = XBotsOffer.BotID;
                            Helper.UserHelper.Table_SteamItemUsersInventory.Insert(XSItemUsersInventory);
                        }
                    }
                }
            }

            return;
        }

        public void DeclinedLocalItemOffer(ulong offer_id)
        {
            XBotsOffer XBotsOffer;
            if (Table_BotsOffer.SelectOne(data => data.OfferID == offer_id, out XBotsOffer))
            {
                XBotOffersItem[] XBotOffersItems;
                if (Table_BotOffersItem.SelectArr(data => data.BotsOfferID == XBotsOffer.ID, out XBotOffersItems))
                {
                    for (int i = 0; i < XBotOffersItems.Length; i++)
                    {
                        XSItemUsersInventory XSItemUsersInventory = new XSItemUsersInventory();
                        XSItemUsersInventory.UserID = Configs.ADMIN_ACCOUNT;
                        XSItemUsersInventory.SteamItemID = XBotOffersItems[i].SteamItemID;
                        XSItemUsersInventory.AssertID = XBotOffersItems[i].AssertID;
                        XSItemUsersInventory.SteamGameID = Helper.SteamItemsHelper.Table.SelectByID(XBotOffersItems[i].SteamItemID).SteamGameID;
                        XSItemUsersInventory.SteamBotID = XBotsOffer.BotID;
                        Helper.UserHelper.Table_SteamItemUsersInventory.Insert(XSItemUsersInventory);
                    }
                }
            }

            return;
        }
    }
}
