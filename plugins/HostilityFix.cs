using Oxide.Core.Plugins;
using System;
using System.Globalization;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("HostilityFix", "Shady", "0.0.2")]
    class HostilityFix : RustPlugin
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
        [PluginReference]
        Plugin NoEscape;

        private object CanPlayerBeHostile(BasePlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player == null || player.gameObject == null || player.IsDestroyed || !player.InSafeZone() || !player.IsConnected || player.IsDead()) return null;

            var escapeBlocked = NoEscape?.Call<bool>("IsEscapeBlockedS", player.UserIDString) ?? false;
            if (escapeBlocked) return null;

            var plyPos = player?.transform?.position ?? Vector3.zero;
            var nearSafeZone = false;
            for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
            {
                var mon = TerrainMeta.Path.Monuments[i];
                var engName = mon?.displayPhrase?.english ?? string.Empty;
                if (string.IsNullOrEmpty(engName)) continue;
                if (engName.Contains("outpost", CompareOptions.OrdinalIgnoreCase))
                {
                    if (mon.IsInBounds(plyPos)) nearSafeZone = true;
                    break;
                }
            }

            if (!nearSafeZone) return false;

            //var heldEntity = player?.GetHeldEntity() ?? null;
            var criteria = 4;
            var needed = 3;
            //  if (heldEntity == null || (heldEntity as BaseProjectile) == null) criteria--;
            var hasAnyGun = false;
            for (int i = 0; i < player.inventory.containerBelt.itemList.Count; i++)
            {
                var item = player.inventory.containerBelt.itemList[i];
                if (item?.GetHeldEntity() is BaseProjectile)
                {
                    hasAnyGun = true;
                    break;
                }
            }

            if (!hasAnyGun)
            {
                for (int i = 0; i < player.inventory.containerMain.itemList.Count; i++)
                {
                    var item = player.inventory.containerMain.itemList[i];
                    if (item?.GetHeldEntity() is BaseProjectile)
                    {
                        hasAnyGun = true;
                        break;
                    }
                }
            }

          

            if (!hasAnyGun) criteria--;
            if (!player.CanAttack()) criteria--;
            if ((UnityEngine.Time.realtimeSinceStartup - player.lastDealtDamageTime) >= 10) criteria--;
            if ((UnityEngine.Time.realtimeSinceStartup - player.lastAttackedTime) >= 30) criteria--;
            if (criteria < needed)
            {
                // PrintWarning("not allowing player to be marked hostile: " + player + " because criteria was: " + criteria + " and we need at least: " + needed);
                return false;
            }
            else return null;
        }

        object CanEntityBeHostile(BasePlayer player)
        {
            if (player == null) return null;
            var hostile = CanPlayerBeHostile(player);
            if (hostile == null) return null;
            else
            {
                if (!(bool)hostile) return false; //return false, CANNOT be marked hostile
            }
            return null;
        }

       object OnEntityMarkHostile(BasePlayer player)
        {
            if (player == null) return null;
            var hostile = CanPlayerBeHostile(player);
            if (hostile == null) return null;
            else
            {
                if (!(bool)hostile) return true; //return true, override default behavior
            }
            return null;
        }

    }
}