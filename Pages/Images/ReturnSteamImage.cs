using System;
using System.Collections.Generic;
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

        public override bool Init(Client client)
        {
            uint SteamGameID, ItemID;
            if (uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0], out SteamGameID) && uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[1], out ItemID))
            {
                UFile image;
                if(Helper.SteamItemsHelper.GetImageFromMemory(ItemID, SteamGameID, out image))
                {
                    FileSender.SendUserFile(client, image);
                }
            }

            BaseFuncs.Show404(client);
            return false;
        }
    }
}
