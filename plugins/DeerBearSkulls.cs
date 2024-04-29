using System;
using System.Text;

namespace Oxide.Plugins
{
    [Info("Deer and Bear Skulls", "Shady", "0.0.2")]
    internal class DeerBearSkulls : RustPlugin
    {
        /*
         * Copyright 2024 PRISM
         *
         * This file is part of PRISM's plugins.
         *
         * PRISM's plugins is free software: you can redistribute it and/or modify
         * it under the terms of the GNU General Public License as published by
         * the Free Software Foundation, either version 3 of the License, or
         * (at your option) any later version.
         *
         * PRISM's plugins is distributed in the hope that it will be useful,
         * but WITHOUT ANY WARRANTY; without even the implied warranty of
         * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
         * GNU General Public License for more details.
         *
         * You should have received a copy of the GNU General Public License
         * along with PRISM's plugins. If not, see <https://www.gnu.org/licenses/>.
         */

        /*
         * The below is not a license (that can be found above), but simply a notice to the reader of this code:
         * The development of PRISM has been gradual and started in 2014.
         * Some of the code provided in this repository may be outdated and may not be the best representation of the author's current coding practices.
         * Some of the code provided in this repository may have been initially developed in 2014.
         * Files not containing the above license are not uniquely part of PRISM's plugins and are subject to the license of the original plugin, where applicable.
         * No copyright infringement is ever intended and the author of PRISM's plugins will comply with any requests to remove code.
         * PRISM's plugins are provided as is and no functionality is guaranteed.
         * Many of these plugins will not work without significant changes as there are numerous instances of hard-coded functions and may rely on external API calls that are not a part of this project itself.
         * 
         * 
         * With those notices out of the way, I, Shady, the author of this plugin, would like you to read and acknowledge the following:
         * These plugins are part of a decade long passion project where we, PRISM, created a unique experience to all players on our Rust servers.
         * This was a significant part of my life. I dedicated a decade to developing and maintaining these plugins and the servers themselves.
         * I implore you that, should you use any of the code or plugins provided in this repository, where it belongs solely to PRISM, that you respect the time and effort put into it.
         * Please remember that I, a single human, invested a significant amount of time and effort into these plugins and the servers they were used on.
         * Please feel free to critique my code as it is written; I am aware it is not perfect. Much of it is old, and I'm capable of doing far better now.
         * Moreover, I ask, sincerely, that you do not use these plugins or code without acknowledging that someone dedicated themselves to this passion project.
         * In asking that, it is my simple desire that you do not let these plugins be used in a way that is inconsistent with my own desires and love.
         * Please do not use them and claim them as your own. I wish for you to use them on your server and make that server something of your own.
         * Something unique and beautiful. However, I ask that you do not use these plugins and claim them as your own work.
         * Should you make changes to them, you are, of course, the author of those changes. But, if your code is based on one of PRISM's original plugins, please do not claim it as your own.
         * I also request that you do not use the plugins in a way that would be detrimental to the Rust community or the community of server owners.
         * In doing so, I ask that you please do not monetize the functions within these plugins with any changes you should make.
         * Should you use any of PRISM's plugins or its code, please do not lock anything behind a paywall or in a way that is unfair to the community of Rust.
         * My origin is that of one where I saw, daily, very young players who only wished to enjoy a few hours of their life on a video game and in many cases had little or no expendable income.
         * For many people, even purchasing the game is significantly difficult. It is, in my eyes, unfair to create fun, custom content, and suggest that you care about the community while also charging for it or creating an unfair environment where players who pay are significantly advantaged compared to those who cannot afford to do so.
         * I cannot control what you do with this code, but I ask that you respect the spirit in which it was created:
         * That is one of sincere love, care, and appreciation for the people who played on PRISM's servers.
         * Our community, much like yours, is your entire server. Your people matter, just as you matter. Please remember that *everyone* deserves a fair, fun environment.
         * I ask that should you use these plugins, you do so with the same love and care that I put into them. I ask that you do so with the same respect, love, care, and appreciation for the people who will play on your server.
         *
         * One of my greatest helps over the years was MBR. He has been a wonderful person, friend, and developer; I am beyond grateful for his contributions to us, but moreover, his friendship.
         * Some of these plugins were authored solely by him, or with his help. Where this has occurred, it should be noted in the author field at the top of the plugin.
         * Worthy of note is that he is solely responsible for our top-tier Discord integration.
         * If you so wish to see his GitHub page, here it is linked below:
         * https://github.com/MBR-0001
         * 
         * 
         * Should you wish to support the work that I have put into this project of the past decade, please consider doing so here:
         * https://www.buymeacoffee.com/shady757
         * 
         * I neither expect nor anticipate any such donations. You are by no means obligated, it is truly a donation. It should be done out of the kindness of your heart, if you so choose.
         * 
         * PRISM, as a community, has not ceased and continues to exist. We have no plans of ceasing our community. We have, however, ceased our Rust servers and thus made these plugins open source.
         * If you ever wish to contact me, or simply join our community and become one of our beloved members, please join us here:
         * https://discord.gg/DUCnZhZ
         * 
         * To quote Rush: "There is magic at your fingers".
         * With love,
         * Shady and all of PRISM. Thank you for everything, always.
         * 
         */
        private const ulong BEAR_SKULL_SKIN_ID = 2839885463;
        private const ulong DEER_SKULL_SKIN_ID = 2839885353;

        private const uint BEAR_CORPSE_ID = 4102891990;
        private const uint POLAR_BEAR_CORPSE_ID = 2275652760;
        private const uint DEER_CORPSE_ID = 784238137;

        private ItemDefinition _skullDef = null;

        public ItemDefinition SkullItemDefinition
        {
            get
            {
                if (_skullDef == null) _skullDef = ItemManager.FindItemDefinition(2048317869); //wolf skull item id

                return _skullDef;
            }
        }

        private Item GetDeerSkull(int amount = 1)
        {
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));

            var item = ItemManager.Create(SkullItemDefinition, amount, DEER_SKULL_SKIN_ID);
            item.name = "Deer Antlers";

            return item;
        }

        private Item GetBearSkull(int amount = 1)
        {
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));

            var item = ItemManager.Create(SkullItemDefinition, amount, BEAR_SKULL_SKIN_ID);
            item.name = "Bear Skull";
            

            return item;
        }

        private void OnMeleeAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info?.HitEntity == null) return;

            var prefabId = info.HitEntity.prefabID;
            if (prefabId != BEAR_CORPSE_ID && prefabId != DEER_CORPSE_ID && prefabId != POLAR_BEAR_CORPSE_ID)
                return;

            player.Invoke(() =>
            {
                if (info?.HitEntity != null || !(info?.HitEntity.IsDestroyed ?? true)) return;

                if (UnityEngine.Random.Range(0, 101) <= (prefabId == DEER_CORPSE_ID ? 40 : 75))
                {

                    var skullItem = prefabId == DEER_CORPSE_ID ? GetDeerSkull() : GetBearSkull();

                    NoteItemByID(player, skullItem.info.itemid, 1); //take a look into how this works now, as it seems Rust istelf can note items by their display name (item.name) - if you pick up a custom named item, it shows the custom name instead

                    if (!skullItem.MoveToContainer(player.inventory.containerMain) && !skullItem.MoveToContainer(player.inventory.containerBelt))
                    {
                        NoteItemByID(player, skullItem.info.itemid, -1);

                        if (!skullItem.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation))
                        {
                            PrintWarning("failed to move skull item OR drop!!!");
                            RemoveFromWorld(skullItem);
                        }
                    
                    }

                }

            }, 0.1f);

        }

        private void NoteItemByID(BasePlayer player, int itemID, int amount)
        {
            if (player == null || !player.IsConnected) return;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try { player.SendConsoleCommand(sb.Clear().Append("note.inv ").Append(itemID).Append(" ").Append(" ").Append(" ").Append(amount).ToString()); } //if the amount is - it will be included automatically, so don't need to add it in the " " sb append
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

    }
}
