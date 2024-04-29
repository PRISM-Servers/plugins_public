using Facepunch.Extend;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("MonumentBlock", "Shady", "0.0.91", ResourceId = 0)]
    internal class MonumentBlock : RustPlugin
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
        private const string CargoMessage = "<size=20><color=#de2f4f>You cannot do this while on or near <color=#4d4e4f>Cargo Ship<color=#1685f5>!</color></color> <color=#fff94f>Leave the area, then try again</color>.</color>";

        private CargoShip CargoInstance;

        private void OnServerInitialized()
        {
            foreach(var ent in BaseNetworkable.serverEntities)
            {
                var cargo = ent as CargoShip;
                if (cargo != null)
                {
                    CargoInstance = cargo;
                    break;
                }
            }
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var cargo = entity as CargoShip;
            if (cargo != null) CargoInstance = cargo;
        }
        

        [PluginReference]
        private readonly Plugin NoEscape;

        private readonly Dictionary<string, float> LastCargoTime = new Dictionary<string, float>();

        private bool IsOnCargo(BasePlayer player)
        {
            return (player?.GetParentEntity()?.prefabID ?? 0) == 3234960997;
        }

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (IsOnCargo(player)) LastCargoTime[player.UserIDString] = Time.realtimeSinceStartup;
        }


        private object AllowedToTeleport(BasePlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.IsDestroyed || player.gameObject == null) return null;
            var escapeBlocked = NoEscape?.Call<bool>("IsEscapeBlockedS", player.UserIDString) ?? false;
            if (escapeBlocked) return null;
            var inSafe = player.InSafeZone();
            if (player.IsHostile() && inSafe)
            {
                return "<size=20><color=#de2f4f>You cannot do this while hostile and in a <color=#1685f5>safe zone</color>!";
            }

            if (inSafe) return null; //can tp from safe zones as long as you're not hostile :)

            var plyPos = player?.transform?.position ?? Vector3.zero;

            if (IsOnCargo(player))
            {
                return CargoMessage;
            }
            else if (CargoInstance != null && !CargoInstance.IsDestroyed && CargoInstance.gameObject != null && CargoInstance.transform != null)
            {
                var cargoPos = CargoInstance.CenterPoint();
                cargoPos.y = plyPos.y;
                if (Vector3.Distance(cargoPos, plyPos) < 200) return CargoMessage; 
            }

            float lastCargo;
            if (LastCargoTime.TryGetValue(player.UserIDString, out lastCargo))
            {
                var diff = Time.realtimeSinceStartup - lastCargo;
                if (diff < 180)
                {
                    return "<size=20><color=#de2f4f>You cannot do this after having recently been on <color=#4d4e4f>Cargo Ship<color=#1685f5>!</color></color> <color=#fff94f>You must wait <color=#1685f5>180</color> seconds, and you've waited <color=#1685f5>" + diff.ToString("N0") + "</color> seconds</color>.</color>";
                }
            }

            for(int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
            {
                var mon = TerrainMeta.Path.Monuments[i];
                if (mon == null || !mon.shouldDisplayOnMap || mon.Type == MonumentType.Lighthouse || mon.Type == MonumentType.WaterWell || mon.Type == MonumentType.Cave || mon.IsSafeZone) continue;
                var pos = mon?.transform?.position ?? Vector3.zero;
                pos.y = plyPos.y; //more accurate checking by not checking Y 
                var monumentName = mon?.displayPhrase?.english ?? string.Empty;

                if (string.IsNullOrEmpty(monumentName)) continue; //caves and some other tiny things are counted as monuments, but have no name. this should help filter them out, along with shouldDisplayOnMap above

                if (monumentName.Contains("oil", CompareOptions.OrdinalIgnoreCase))
                {
                    if (mon.IsInBounds(plyPos) || Vector3.Distance(plyPos, pos) < 140)
                    {
                        return "<size=20><color=#de2f4f>You cannot do this while near <color=#4d4e4f>" + monumentName + "<color=#1685f5>!</color></color> <color=#fff94f>Leave the area, then try again</color>.</color>";
                    }
                }
                else
                {
                    if (monumentName.Contains("quarry", CompareOptions.OrdinalIgnoreCase) || monumentName.Contains("outpost", CompareOptions.OrdinalIgnoreCase) || monumentName.Contains("bandit", CompareOptions.OrdinalIgnoreCase) || monumentName.Contains("harbor", CompareOptions.OrdinalIgnoreCase) || monumentName.Contains("sewer", CompareOptions.OrdinalIgnoreCase) || monumentName.Contains("cabin", CompareOptions.OrdinalIgnoreCase)) continue;

                    var maxDist = (mon.Type == MonumentType.Roadside && monumentName.Contains("train", CompareOptions.OrdinalIgnoreCase)) ? 35 : (monumentName.Contains("oxum", CompareOptions.OrdinalIgnoreCase) || monumentName.Contains("supermarket", CompareOptions.OrdinalIgnoreCase) || monumentName.Contains("dome", CompareOptions.OrdinalIgnoreCase)) ? 50 : monumentName.Contains("launch", CompareOptions.OrdinalIgnoreCase) ? 280 : 140;

                    if (mon.IsInBounds(plyPos) || Vector3.Distance(plyPos, pos) < maxDist)
                    {
                        //PrintWarning("plyPos: " + plyPos + ", pos: " + pos);
                        return "<size=20><color=#de2f4f>You cannot do this while near <color=#1685f5>" + monumentName + "</color>! <color=#fff94f>Leave the area, then try again</color>.</color>";
                    }
                }
            }


            return null;
        }

        private object CanPlayerTrade(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null) return null;
            return AllowedToTeleport(player);
        }

        private object CanTeleport(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null) return null;
            return AllowedToTeleport(player);
        }

        private object CanJoinDeathmatch(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null) return null;
            return AllowedToTeleport(player);
        }
    }
}