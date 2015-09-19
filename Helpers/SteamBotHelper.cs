using GameSlot.Database;
using GameSlot.Types;
using SteamBotUTRequest;
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
        public XTable<XSteamBotProcessItems> Table_ProcessItems = new XTable<XSteamBotProcessItems>();

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
                int count = int.MaxValue;
                while (count == int.MaxValue)
                {
                    foreach (XSteamBot Bot in steambots)
                    {
                        List<XSteamBotProcessItems> processes;

                        if (!this.Table_ProcessItems.Select(data => data.SteamBotID == Bot.ID && data.Status <= 1, out processes))
                        {
                            count = processes.Count;
                            XSteamBot = Bot;
                        }
                        else if (this.Table_ProcessItems.Select(data => data.SteamBotID == Bot.ID && data.Status == 1, out processes))
                        {
                            if (processes.Count < count)
                            {
                                count = processes.Count;
                                XSteamBot = Bot;
                            }
                        }
                    }

                    Thread.Sleep(100);
                }

                return true;
            }

            return false;
        }

        public bool SelectByOfferID(ulong OfferID, out XSteamBotProcessItems XSteamBotProcessItem)
        {
            XSteamBotProcessItem = new XSteamBotProcessItems();
            return this.Table_ProcessItems.SelectOne(data => data.OfferID == OfferID, out XSteamBotProcessItem);
        }

        public void ErrorToSendLocalItem(ulong SteamUserID, int SentTime, uint BotID, bool to_admin = false)
        {
            XUser user;
            if (Helper.UserHelper.Table.SelectOne(data => data.SteamID == SteamUserID, out user))
            {
                XBotsOffer XBotsOffer;
                if (this.Table_BotsOffer.SelectOne(data => data.SteamUserID == SteamUserID && data.Status == 0 && data.SentTime == SentTime && data.BotID==BotID, out XBotsOffer))
                {
                    XBotOffersItem[] XBotOffersItems;
                    if (Table_BotOffersItem.SelectArr(data => data.BotsOfferID == XBotsOffer.ID, out XBotOffersItems))
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

                    XBotsOffer.Status = 5;
                    this.Table_BotsOffer.UpdateByID(XBotsOffer, XBotsOffer.ID);
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

        public void SendLocalItemsToSteam(XUser user, uint SteamGameID, Client client)
        {
            List<USteamItem> SteamItems = Helper.UserHelper.GetSteamLocalInventory(user.ID, SteamGameID, false);
            if (SteamItems.Count > 0)
            {
                Dictionary<uint, List<UTRequestSteamItem>> BotsRequests = new Dictionary<uint, List<UTRequestSteamItem>>();
                Dictionary<long, uint> AssertItemsIDs = new Dictionary<long, uint>();

                foreach (USteamItem SteamItem in SteamItems)
                {                
                    XSItemUsersInventory XSItemUsersInventory;
                    if (Helper.UserHelper.Table_SteamItemUsersInventory.SelectOne(data => data.AssertID == SteamItem.AssertID && !data.Deleted, out XSItemUsersInventory))
                    {
                        XSItemUsersInventory.Deleted = true;
                        Helper.UserHelper.Table_SteamItemUsersInventory.UpdateByID(XSItemUsersInventory, XSItemUsersInventory.ID);

                        UTRequestSteamItem UTRequestSteamItem = new UTRequestSteamItem();
                        UTRequestSteamItem.appid = (int)SteamItem.SteamGameID;
                        UTRequestSteamItem.contextid = 2;

                        UTRequestSteamItem.assertid = (long)SteamItem.AssertID;

                        Logger.ConsoleLog("Making dictionary for out item: " + user.ID + " SteamUserID: " + user.SteamID + " Bot ID: " + SteamItem.SteamBotID, ConsoleColor.Cyan, LogLevel.Info);

                        AssertItemsIDs.Add((long)SteamItem.AssertID, SteamItem.ID);
                        if (BotsRequests.ContainsKey(SteamItem.SteamBotID))
                        {
                            BotsRequests[SteamItem.SteamBotID].Add(UTRequestSteamItem);
                        }
                        else
                        {
                            List<UTRequestSteamItem> item_rq = new List<UTRequestSteamItem>();
                            item_rq.Add(UTRequestSteamItem);
                            BotsRequests.Add(SteamItem.SteamBotID, item_rq);
                        }
                    }
                }

                if (!UTSteam.SendClientOffer.ContainsKey(user.SteamID))
                {
                    UTSteam.SendClientOffer.Add(user.SteamID, client);
                }
                else if (UTSteam.SendClientOffer[user.SteamID] != client)
                {
                    UTSteam.SendClientOffer[user.SteamID] = client;
                }

                foreach (uint key in BotsRequests.Keys)
                {                
                    XBotsOffer XBotsOffer = new XBotsOffer();
                    XBotsOffer.BotID = key;
                    XBotsOffer.SteamUserID = user.SteamID;
                    XBotsOffer.SentTime = Helper.GetCurrentTime();
                    uint bots_offerid = Helper.SteamBotHelper.Table_BotsOffer.Insert(XBotsOffer);

                    UTRequestSteamMain UTRequestSteamMain = new UTRequestSteamMain();

                    foreach (UTRequestSteamItem URequest in BotsRequests[key])
                    {
                        if (!AssertItemsIDs.ContainsKey(URequest.assertid))
                        {
                            Logger.ConsoleLog("Cлучилась непонятная ошибка: " + URequest.assertid + "user" + user.ID, ConsoleColor.Yellow, LogLevel.Warning);
                            continue;
                        }

                        XBotOffersItem XBotOffersItem = new XBotOffersItem();
                        XBotOffersItem.BotsOfferID = bots_offerid;
                        XBotOffersItem.AssertID = (ulong)URequest.assertid;

                        XBotOffersItem.SteamItemID = AssertItemsIDs[URequest.assertid];
                        Helper.SteamBotHelper.Table_BotOffersItem.Insert(XBotOffersItem);

                        XSteamItemsClassID XSteamItemsClassID;
                        Helper.SteamItemsHelper.Table_ClassID.SelectOne(dt => dt.AssertID == XBotOffersItem.AssertID, out XSteamItemsClassID);
                        URequest.assertid = (long)XSteamItemsClassID.ClassID;

                        UTRequestSteamMain.Items.Add(URequest);

                        Logger.ConsoleLog("-----------------------------");
                        Logger.ConsoleLog("Item class id: " + URequest.assertid);
                        Logger.ConsoleLog("Item contex id: " + URequest.contextid);
                        Logger.ConsoleLog("Item app id: " + URequest.appid);
                        Logger.ConsoleLog("Steam item id: " + XBotOffersItem.SteamItemID);

                        Logger.ConsoleLog("Saving to UTRequestSteamMain:" + user.ID + " SteamUserID: " + user.SteamID + " Bot ID: " + key + " AsserID:" + URequest.assertid, ConsoleColor.Cyan, LogLevel.Info);
                    }

                    UTRequestSteamMain.message = "Peredacha veshhej iz vremennogo inventarja GAMESLOT. Dannoe predlozhenie budet avtomaticheski udaleno spustja chas.";
                    UTRequestSteamMain.BotID = (int)key;
                    
                    UTRequestSteamMain.steamid = user.SteamID.ToString();
                    UTRequestSteamMain.trade_acc_id = user.TradeToken;
                    UTRequestSteamMain.SendItems = true;

                    Logger.ConsoleLog("Sending offer from local inventory... [" + UTRequestSteamMain.Items.Count + "] Bot ID:" + UTRequestSteamMain.BotID);
                    UpTunnel.Sender.Send(UTSteam.sk, UTRequestSteamMain);

                    while (Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.SteamUserID == user.SteamID && bt.Status == 0, out XBotsOffer))
                    {
                        Thread.Sleep(300);
                    }

                }
            }
        }
    }
}
