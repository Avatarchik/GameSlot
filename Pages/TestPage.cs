using GameSlot.Database;
using GameSlot.Helpers;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
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
                bot.Login = "trarara";
                Helper.SteamBotHelper.Table.Insert(bot);
                Logger.ConsoleLog("added bot!");
            }

            XUser user;
            if (Helper.UserHelper.GetCurrentUser(client, out user))
            {
                if (client.GetParam("token") != null && client.GetParam("partner") != null)
                {

                    user.TradeToken = client.GetParam("token");
                    user.TradePartner = Convert.ToUInt64(client.GetParam("partner"));
                    Helper.UserHelper.Table.UpdateByID(user, user.ID);
                    Logger.ConsoleLog("updated steam trade");
                }
                else if (client.GetParam("all") != null)
                {
                    Logger.ConsoleLog("count::" + Helper.UserHelper.Table_SteamItemUsersInventory.SelectAll().Count, ConsoleColor.Red);
                    foreach(XSItemUsersInventory x in Helper.UserHelper.Table_SteamItemUsersInventory.SelectAll())
                    {
                        Logger.ConsoleLog(x.ID + ":UserID:" + x.UserID + ":AssertID:" + x.AssertID + ":SteamGameID:" + x.SteamGameID + ":Deleted:" + x.Deleted + ":SteamItemID:" + x.SteamItemID + ":SteamBotID:" + x.SteamBotID + "-------------");
                    }
                }
                else
                {
                    if (client.GetParam("item_id") != null && client.GetParam("asert_id") != null)
                    {
                        USteamItem neww = new USteamItem();
                        neww.ID = Convert.ToUInt32(client.GetParam("item_id"));
                        neww.AssertID = Convert.ToUInt64(client.GetParam("asert_id"));
                        usi.Add(neww);
                        Logger.ConsoleLog(neww.ID + ":" + neww.AssertID);
                        Logger.ConsoleLog("\n---СТАВКА_СТАТУС::" + Helper.LotteryHelper.SetBet(Helper.LotteryHelper.GetCurrent(Configs.DOTA2_STEAM_GAME_ID).ID, user.ID, usi, Chips), ConsoleColor.Red);
                    }
                }
            }

            XLottery last_lot;
            Logger.ConsoleLog((Helper.LotteryHelper.Table.SelectOne(data => data.SteamGameID == 570, out last_lot)));

            client.HttpSend(Helper.LotteryHelper.GetBank(0, out usi, out Chips).ToString());
            return true;
        }
    }
}
