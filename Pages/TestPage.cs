using GameSlot.Database;
using GameSlot.Helpers;
using GameSlot.Pages.Includes;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class TestPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/test"; }
        }
        public override string TemplateAddr
        {
            get { return "Lottery.html"; }
        }
        public override bool FilterBefore
        {
            get { return true; }
        }
        public override bool Init(Client client)
        {
            //Image ing = Image.FromStream(new MemoryStream());
            List<Chip> Chips = new List<Chip>();
            List<USteamItem> usi = new List<USteamItem>();
            /*
            Helper.ChipHelper.AddChipToUser(4, 1);
            
            Chip chip = new Chip();
            chip.AssertID = 909383131;
            Chips.Add(chip);
            */

            if(Helper.SteamBotHelper.Table.SelectAll().Count == 0)
            {
                XSteamBot bot = new XSteamBot();
                bot.Login = "gabechest01";
                bot.Password = "43uiy48Df785bfcUICda";
                bot.Name = "Sedoy";
                Helper.SteamBotHelper.Table.Insert(bot);

                bot = new XSteamBot();
                bot.Login = "gabechest02";
                bot.Password = "43uiy48cEKbchfcUICda";
                bot.Name = "Tatar";
                Helper.SteamBotHelper.Table.Insert(bot);

                bot = new XSteamBot();
                bot.Login = "gabechest03";
                bot.Password = "kejbcxk78487ckxbCEceebw";
                bot.Name = "Chertila";
                Helper.SteamBotHelper.Table.Insert(bot);

                Logger.ConsoleLog("added bots!", ConsoleColor.Cyan, LogLevel.Info);
            }

            XUser user;
            if (Helper.UserHelper.GetCurrentUser(client, out user) && (user.ID == Configs.ADMIN_ACCOUNT || user.SteamID == 76561198083337086))
            {
                if (client.GetParam("all") != null)
                {
                    Logger.ConsoleLog("count::" + Helper.UserHelper.Table_SteamItemUsersInventory.SelectAll().Count, ConsoleColor.Red);
                    foreach(XSItemUsersInventory x in Helper.UserHelper.Table_SteamItemUsersInventory.SelectAll())
                    {
                        Logger.ConsoleLog(x.ID + ":UserID:" + x.UserID + ":AssertID:" + x.AssertID + ":SteamGameID:" + x.SteamGameID + ":Deleted:" + x.Deleted + ":SteamItemID:" + x.SteamItemID + ":SteamBotID:" + x.SteamBotID + "-------------", ConsoleColor.Cyan, LogLevel.Info);
                    }
                }
                else if (client.GetParam("chipID") != null && client.GetParam("to") != null)
                {
                    Helper.ChipHelper.AddChipToUser(Convert.ToUInt32(client.GetParam("chipID")), Convert.ToUInt32(client.GetParam("to")));
                    Logger.ConsoleLog("Added chip!!!", ConsoleColor.Cyan, LogLevel.Info);
                    //Thread.Sleep(10);
                }

                else if(client.GetParam("game_322") != null)
                {
                    Logger.ConsoleLog("\nDOTA: ", ConsoleColor.Cyan, LogLevel.Info);
                    Lottery dota = Helper.LotteryHelper.GetCurrent(Configs.DOTA2_STEAM_GAME_ID, client);
                    Logger.ConsoleLog("RoundNumber: " + dota.RaundNumber.ToString(), ConsoleColor.Cyan, LogLevel.Info);
                    Logger.ConsoleLog("WinnersToken: " + (int)(dota.JackpotPrice * 100 * dota.RaundNumber), ConsoleColor.Cyan, LogLevel.Info);
                    Logger.ConsoleLog("Winner: " + Helper.LotteryHelper.GetUserByToken((int)(dota.JackpotPrice * 100 * dota.RaundNumber), dota.ID).Name, ConsoleColor.Cyan, LogLevel.Info);
                    Logger.ConsoleLog("--------------------------------------------------------------------\n", ConsoleColor.Cyan, LogLevel.Info);

                    Logger.ConsoleLog("\nCSGO: ", ConsoleColor.Cyan, LogLevel.Info);
                    Lottery csgo = Helper.LotteryHelper.GetCurrent(Configs.CSGO_STEAM_GAME_ID, client);
                    Logger.ConsoleLog("RoundNumber: " + csgo.RaundNumber.ToString(), ConsoleColor.Cyan, LogLevel.Info);
                    Logger.ConsoleLog("WinnersToken: " + (int)(csgo.JackpotPrice * 100 * csgo.RaundNumber), ConsoleColor.Cyan, LogLevel.Info);
                    Logger.ConsoleLog("Winner: " + Helper.LotteryHelper.GetUserByToken((int)(csgo.JackpotPrice * 100 * csgo.RaundNumber), csgo.ID).Name, ConsoleColor.Cyan, LogLevel.Info);
                    Logger.ConsoleLog("--------------------------------------------------------------------\n", ConsoleColor.Cyan, LogLevel.Info);
                }

                else if (client.GetParam("add_money") != null && client.GetParam("to") != null)
                {
                    double money;
                    uint to;
                    XUser us;
                    if (double.TryParse(client.GetParam("add_money"), out money) && uint.TryParse(client.GetParam("to"), out to) && Helper.UserHelper.Table.SelectByID(to, out us))
                    {
                        us.Wallet += money;
                        Helper.UserHelper.Table.UpdateByID(us, us.ID);
                        Logger.ConsoleLog("Added money: " + money + " to user: " + us.Name, ConsoleColor.Cyan, LogLevel.Info);
                    }
                }

                else if(client.GetParam("waiting_images") != null)
                {
                    Logger.ConsoleLog(Helper.SteamItemsHelper.QueueDownloadImageCount(), ConsoleColor.Cyan, LogLevel.Info);
                }


                else if (client.GetParam("add_online_users") != null)
                {
                    int on;
                    if (int.TryParse(client.GetParam("add_online_users"), out on))
                    {
                        UserHelper.ExtraOnlineUsers = on;
                        Logger.ConsoleLog("Added users " + on + "; now online: " + Helper.UserHelper.GetOnlineNum(), ConsoleColor.Cyan, LogLevel.Info);
                        WebSocketPage.UpdateOnlineUsers(Helper.UserHelper.GetOnlineNum());
                    }
                }

                else if(client.GetParam("get_inventory_updating_num") != null)
                {
                    Logger.ConsoleLog("At now inventory updating: " + Helper.UserHelper.GetUpdatingInventoriesThreads().Count, ConsoleColor.Cyan, LogLevel.Info);
                }

                else if (client.GetParam("auth_by_other_user") != null)
                {
                    uint userID;
                    XUser usr;
                    if (uint.TryParse(client.GetParam("auth_by_other_user"), out userID) && Helper.UserHelper.Table.SelectByID(userID, out usr))
                    {
                        Helper.UserHelper.Auth(usr.SteamID, out usr, client);
                        Logger.ConsoleLog("Auth done to: " + usr.Name + " (" + usr.SteamID + ")", ConsoleColor.Cyan, LogLevel.Info);
                    }
                }

                else if (client.GetParam("encode_all_items_names") != null)
                {
                    List<XSteamItem> SteamItems = new List<XSteamItem>(Helper.SteamItemsHelper.Table.SelectAll());
                    int co = 0;
                    for(int i = 0; i < SteamItems.Count; i++)
                    {
                        XSteamItem item = Helper.SteamItemsHelper.Table.SelectByID(SteamItems[i].ID);
                        if (item.Name.Contains("&amp;#39;") || item.RusName.Contains("&amp;#39;"))
                        {
                            item.Name = item.Name.Replace("&amp;#39;", "'");
                            item.RusName = item.RusName.Replace("&amp;#39;", "'");
                            Helper.SteamItemsHelper.Table.UpdateByID(item, item.ID);
                            co++;
                        }
                        
                    }

                    Logger.ConsoleLog("Updated " + co + " items", ConsoleColor.Cyan, LogLevel.Info);
                    
                }

                else if (client.GetParam("get_ws_stat") != null)
                {
                    WebSocketPage.GetStats();
                }

                else if (client.GetParam("get_all_items") != null)
                {
                    foreach (XSteamItem itm in Helper.SteamItemsHelper.Table.SelectAll())
                    {
                        if (itm.Name.Contains("&amp;#39;"))
                        {
                            Logger.ConsoleLog(itm.Name, ConsoleColor.Cyan);
                        }
                    }

                    Logger.ConsoleLog("All: " + Helper.SteamItemsHelper.Table.SelectAll().Count, ConsoleColor.Cyan);

                }

                else if (client.GetParam("update_price_by_rusname") != null)
                {
                    XSteamItem SteamItem;
                    if (Helper.SteamItemsHelper.Table.SelectOne(data => data.RusName == client.GetParam("update_price_by_rusname"), out SteamItem))
                    {
                        SteamItem.Price = Helper.SteamItemsHelper.GetMarketPrice(SteamItem.Name, SteamItem.SteamGameID);
                        Helper.SteamItemsHelper.Table.UpdateByID(SteamItem, SteamItem.ID);
                        Logger.ConsoleLog("Found" + SteamItem.ID, ConsoleColor.Cyan);
                    }
                    else
                        Logger.ConsoleLog("Not found", ConsoleColor.Cyan);

                }

                else if(client.GetParam("update_all_users_top") != null)
                {
                    foreach (XUser XUser in Helper.UserHelper.Table.SelectAll())
                    {
                        XUser CurUser = Helper.UserHelper.Table.SelectByID(XUser.ID);
                        CurUser.CSGO_RUB_WonTotalPrice = 0d;
                        CurUser.CSGO_WonTotalPrice = 0d;
                        CurUser.CSGO_WonItemsCount = 0;

                        CurUser.DOTA_RUB_WonTotalPrice = 0d;
                        CurUser.DOTA_WonTotalPrice = 0d;
                        CurUser.DOTA_WonItemsCount = 0;
                        //
                        CurUser.DOTA_TotalBetItemsCount = 0;
                        CurUser.DOTA_RUB_TotalBetPrice = 0;
                        CurUser.DOTA_TotalBetPrice = 0;

                        CurUser.CSGO_TotalBetItemsCount = 0;
                        CurUser.CSGO_RUB_TotalBetPrice = 0;
                        CurUser.CSGO_TotalBetPrice = 0;

                        List<XLottery> Lotteries;
                        Helper.LotteryHelper.Table.Select(data => data.WinnersToken > 0 && data.Winner == XUser.ID, out Lotteries);
                        foreach (XLottery Lottery in Lotteries)
                        {
                            List<XLotteryUsersBetsPrice> BetsPrice;
                            Helper.LotteryHelper.TableUsersBetsPrice.Select(data => data.LotteryID == Lottery.ID && data.UserID != Lottery.Winner, out BetsPrice);
                            foreach (XLotteryUsersBetsPrice Bet in BetsPrice)
                            {
                                if (Lottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                {
                                    CurUser.DOTA_WonTotalPrice += Bet.TotalBetsPrice;
                                    CurUser.DOTA_RUB_WonTotalPrice += Bet.TotalBetsPrice * Lottery.RubCurrency;
                                    CurUser.DOTA_WonItemsCount += Bet.TotalBetsItemsNum;
                                }
                                else if (Lottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                {
                                    CurUser.CSGO_WonTotalPrice += Bet.TotalBetsPrice;
                                    CurUser.CSGO_RUB_WonTotalPrice += Bet.TotalBetsPrice * Lottery.RubCurrency;
                                    CurUser.CSGO_WonItemsCount += Bet.TotalBetsItemsNum;
                                }
                            }
                        }

                        // сумма ставок
                        //List<XLottery> Lotteries2;
                        Helper.LotteryHelper.Table.Select(data => data.WinnersToken > 0, out Lotteries);
                        foreach (XLottery Lottery in Lotteries)
                        {
                            List<XLotteryUsersBetsPrice> BetsPrice;
                            Helper.LotteryHelper.TableUsersBetsPrice.Select(data => data.LotteryID == Lottery.ID && data.UserID == XUser.ID, out BetsPrice);
                            foreach (XLotteryUsersBetsPrice Bet in BetsPrice)
                            {
                                if (Lottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                {
                                    CurUser.DOTA_TotalBetItemsCount += Bet.TotalBetsItemsNum;
                                    CurUser.DOTA_RUB_TotalBetPrice += Bet.TotalBetsPrice * Lottery.RubCurrency;
                                    CurUser.DOTA_TotalBetPrice += Bet.TotalBetsPrice;
                                }
                                else if (Lottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                {
                                    CurUser.CSGO_TotalBetItemsCount += Bet.TotalBetsItemsNum;
                                    CurUser.CSGO_RUB_TotalBetPrice += Bet.TotalBetsPrice * Lottery.RubCurrency;
                                    CurUser.CSGO_TotalBetPrice += Bet.TotalBetsPrice;
                                }
                            }
                        }
                        Helper.UserHelper.Table.UpdateByID(CurUser, CurUser.ID);
                    }          
                }
                else if (client.GetParam("update_all_groups_top") != null && client.GetParam("sure") != null)
                {
                    foreach (XUser XUser in Helper.UserHelper.Table.SelectAll())
                    {
                        XUser CurUser = Helper.UserHelper.Table.SelectByID(XUser.ID);
                        CurUser.CSGO_RUB_GroupWonTotalPrice = 0d;
                        CurUser.CSGO_GroupWonTotalPrice = 0d;
                        CurUser.CSGO_GroupWonItemsCount = 0;

                        CurUser.DOTA_RUB_GroupWonTotalPrice = 0d;
                        CurUser.DOTA_GroupWonTotalPrice = 0d;
                        CurUser.DOTA_GroupWonItemsCount = 0;

                        List<XUser> group_users;
                        Helper.UserHelper.Table.Select(data => (int)data.GroupOwnerID == XUser.ID, out group_users);
                        foreach (XUser group_user in group_users)
                        {
                            List<XLottery> Lotteries;
                            Helper.LotteryHelper.Table.Select(data => data.WinnersToken > 0 && data.Winner == group_user.ID, out Lotteries);
                            foreach (XLottery Lottery in Lotteries)
                            {
                                List<XLotteryUsersBetsPrice> BetsPrice;
                                Helper.LotteryHelper.TableUsersBetsPrice.Select(data => data.LotteryID == Lottery.ID && data.UserID != Lottery.Winner, out BetsPrice);
                                foreach (XLotteryUsersBetsPrice Bet in BetsPrice)
                                {
                                    if (Lottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                    {
                                        CurUser.DOTA_GroupWonTotalPrice += Bet.TotalBetsPrice;
                                        CurUser.DOTA_RUB_GroupWonTotalPrice += Bet.TotalBetsPrice * Lottery.RubCurrency;
                                        CurUser.DOTA_GroupWonItemsCount += Bet.TotalBetsItemsNum;
                                    }
                                    else if (Lottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                    {
                                        CurUser.CSGO_GroupWonTotalPrice += Bet.TotalBetsPrice;
                                        CurUser.CSGO_RUB_GroupWonTotalPrice += Bet.TotalBetsPrice * Lottery.RubCurrency;
                                        CurUser.CSGO_GroupWonItemsCount += Bet.TotalBetsItemsNum;
                                    }
                                }
                            }
                        }
                        
                        Helper.UserHelper.Table.UpdateByID(CurUser, CurUser.ID);
                    }
                }

                else if(client.GetParam("send_lost_items_to_local_usr") != null && client.GetParam("sure") != null)
                {
                    List<XBotsOffer> XBotsOffers;
                    Helper.SteamBotHelper.Table_BotsOffer.Select(data => (data.Status == 0), out XBotsOffers);
                    int i = 0;
                    foreach(XBotsOffer offer_d in XBotsOffers)
                    {
                        XBotsOffer offer = Helper.SteamBotHelper.Table_BotsOffer.SelectByID(offer_d.ID);
                        offer.Status = 5;
                        Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(offer, offer.ID);
                        Helper.SteamBotHelper.ErrorToSendLocalItem(offer.SteamUserID, offer.SentTime);

                         XUser g_usr;
                         Helper.UserHelper.SelectBySteamID(offer.SteamUserID, out g_usr);
                         Logger.ConsoleLog("Sent to " + g_usr.ID + "SteamID " + offer.SteamUserID + " num: " + i++, ConsoleColor.Cyan, LogLevel.Info);

                        WebSocketPage.SendItemsOffer(offer.SteamUserID, 0);
                    }
                }
                
                else if (client.GetParam("local") != null)
                {
                    List<XBotsOffer> XBotsOffers;
                    Helper.SteamBotHelper.Table_BotsOffer.Select(data => data.SteamUserID == 76561198051486946, out XBotsOffers);
                    foreach (XBotsOffer offer_d in XBotsOffers)
                    {
                        Logger.ConsoleLog(offer_d.ID + ":" + offer_d.Status, ConsoleColor.Cyan, LogLevel.Info);
                    }
                }
            }

            Random rnd = new Random();
            client.HttpSend(rnd.Next(0,2).ToString());
            return true;
        }
    }
}
