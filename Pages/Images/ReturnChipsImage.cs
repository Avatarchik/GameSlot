using GameSlot.Types;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Images
{
    public class ReturnChipsImage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/chip-image/"; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            uint ChipID;
            if (uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0], out ChipID))
            {
                Chip chip;
                if (Helper.ChipHelper.SelectByID(ChipID, out chip) && chip.Image != null)
                {
                    FileSender.SendCachedFile(client, (CachedFile)UpCacher.files[chip.Image]);
                    return false;
                }
            }

            BaseFuncs.Show404(client);
            return false;
        }
    }
}
