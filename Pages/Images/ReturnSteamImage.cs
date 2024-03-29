﻿using GameSlot.Database;
using GameSlot.Helpers;
using GameSlot.Types;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Images
{
    public class ReturnSteamImage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/steam-image/"; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            uint SteamGameID, ItemID;
            if (uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0], out SteamGameID) && uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[1], out ItemID))
            {
                string image;
                if (Helper.SteamItemsHelper.GetImageFromMemory(ItemID, SteamGameID, out image))
                {
                    FileSender.SendCachedFile(client, (CachedFile)UpCacher.files[image]);
                    //FileSender.SendUserFile(client, image);
                    return false;
                }
                else if (SteamItemsHelper.NoneImage != null)
                {
                    FileSender.SendCachedFile(client, (CachedFile)UpCacher.files[SteamItemsHelper.NoneImage]);
                    return false;
                }                
            }

            BaseFuncs.Show404(client);
            return false;
        }
    }
}
