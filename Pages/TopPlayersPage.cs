﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class TopPlayersPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/top-players"; }
        }
        public override string TemplateAddr
        {
            get { return "CommingSoon.html"; }
        }
        public override bool Init(Client client)
        {
            Hashtable data = new Hashtable();
            data.Add("Title", "Топ игроков GAMESLOT");
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
