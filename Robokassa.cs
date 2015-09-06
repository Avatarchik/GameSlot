﻿using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot
{
    public class Robokassa : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/robokassa/"; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool MaintenanceAffect
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            string Action = BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0];

            uint OrderID;
            XOrder Order;

            if (Action == "success")
            {
                client.Redirect(Helper.GetReferer(client));
            }
            else if (uint.TryParse(client.GetParam("InvId"), out OrderID) && Helper.OrderHelper.Table.SelectOne(data => data.ID == OrderID && data.Status == 0, out Order))
            {
                if (Action == "result")
                {
                    if (this.ResultCRC(client).Equals(client.GetParam("SignatureValue")))
                    {
                        Order.Status = 1;
                        XUser user = Helper.UserHelper.Table.SelectByID(Order.UserID);
                        user.Wallet += Order.Price;
                        Helper.OrderHelper.Table.UpdateByID(Order, Order.ID);
                        client.HttpSend("OK" + Order.ID);
                    }
                }
            }
            BaseFuncs.Show404(client);
            return true;
        }

        private static string mrh_login = "gameslot_kozo4ka";

        public static string GetPaymentURL(XOrder order)
        {
            string crc = Robokassa.SignatureValue(order);
            string attr = "?MrchLogin=" + Robokassa.mrh_login + "&OutSum=" + order.Price.ToString() + ".00&InvId=" + order.ID + "&SignatureValue=" + crc;
            return "https://auth.robokassa.ru/Merchant/Index.aspx" + attr;
        }

        public static string SignatureValue(XOrder order)
        {
            string mrh_pass1 = "12311974a";
            return BaseFuncs.MD5(Robokassa.mrh_login + ":" + order.Price.ToString() + ".00:" + order.ID + ":" + mrh_pass1);
        }

        private string ResultCRC(Client client)
        {
            string crc = client.GetParam("OutSum") + ":" + client.GetParam("InvId") + ":" + "12311974b";
            Console.WriteLine(crc);
            return BaseFuncs.MD5(crc).ToUpper();
        }
    }
}
