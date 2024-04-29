using System.Linq;

namespace Oxide.Plugins
{
    [Info("SkinDownload", "Shady", "0.0.2", ResourceId = 0000)]
    class SkinDownload : RustPlugin
    {
        /*---------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	PUBLIC PLUGIN - Developed by Shady (https://oxidemod.org/plugins/authors/shady757.25129/) - Not posted on oxidmod.org because I don't want to have to maintain it  //
		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        [ChatCommand("allskin")]
        private void cmdAllSkins(BasePlayer player, string command, string[] args)
        {
            if (player.IsDead() || player?.inventory == null)
            {
                SendReply(player, "You must be alive to use this.");
                return;
            }
            if (args.Length <= 0 || args[0].ToLower() != "now")
            {
                SendReply(player, "/allskin will attempt to give you every skin in order to trigger a download for all of them. May cause lag.");
                return;
            }
            var skins = Rust.Workshop.Approved.All.Select(p => p.Value).ToList();
            if (skins == null || skins.Count < 1)
            {
                SendReply(player, "Failed to get any skins!");
                return;
            }
            var newShirt = ItemManager.CreateByName("tshirt", 1);
            if (!newShirt.MoveToContainer(player.inventory.containerMain) && !newShirt.MoveToContainer(player.inventory.containerBelt))
            {
                RemoveFromWorld(newShirt);
                SendReply(player, "Please clear your inventory!");
                return;
            }
            newShirt.name = "Skin Placeholder";
            SendReply(player, "Now going through " + skins.Count.ToString("N0") + " skins.");


            var skinIndex = 0;
            Timer newTimer = null;
            newTimer = timer.Repeat(0.1f, skins.Count, () =>
            {
                if (player == null || newShirt == null || !(player?.IsConnected ?? false) || (player?.IsDead() ?? true))
                {
                    newTimer.Destroy();
                    newTimer = null;
                    return;
                }
                newShirt.skin = skins[skinIndex].WorkshopdId;
                newShirt.MarkDirty();
                if ((skinIndex + 1) >= skins.Count)
                {
                    RemoveFromWorld(newShirt);
                    SendReply(player, "Successfully queued (if not already downloaded) " + skins.Count.ToString("N0") + " skins.");
                }
                skinIndex++;
            });
        }

        void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            if (item?.parent != null) item.RemoveFromContainer();
            item.Remove(0.0f);
        }
    }
}