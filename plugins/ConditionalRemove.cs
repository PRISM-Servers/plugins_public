using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Conditional Remove", "Shady", "0.0.2")]
    internal class ConditionalRemove : RustPlugin
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
        #region Fields
        [PluginReference]
        private readonly Plugin NoEscape;

        private const int MAX_ENTITIES_BEFORE_CUPBOARD_REQUIRED = 128;

        private readonly HashSet<BuildingBlock> _blocks = new HashSet<BuildingBlock>();
        #endregion

        #region Hooks
        private void OnServerInitialized()
        {
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();
                foreach (var entity in BaseEntity.saveList)
                {
                    var block = entity as BuildingBlock;
                    if (block != null) _blocks.Add(block);
                }
            }
            finally
            {
                PrintWarning(nameof(OnServerInitialized) + " took: " + watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms");
                Facepunch.Pool.Free(ref watch);
            }
        }
        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null) return;

            var block = entity as BuildingBlock;

            if (block != null) _blocks.Remove(block);
        }

        private void OnEntityBuilt(Planner plan, GameObject objectBlock)
        {
            var block = objectBlock?.ToBaseEntity() as BuildingBlock;

            if (block != null) _blocks.Add(block);
        }

        private object canRemove(BasePlayer player)
        {
            if (player == null) return null;

            if (IsEscapeBlocked(player.UserIDString))
            {
                var sb = Facepunch.Pool.Get<StringBuilder>();
                try { return sb.Clear().Append("<color=orange>You cannot remove while <color=red>raid</color></color>/<color=orange>combat blocked</color><color=red>!</color>").ToString(); }
                finally { Facepunch.Pool.Free(ref sb); }
              
            }

            return null;
        }
        private object CanRemoveEntity(BasePlayer player, int type, BaseEntity targetEntity)
        {
            if (player == null || targetEntity == null) return null;

            var block = targetEntity as BuildingBlock;
            if (block == null || block.buildingID <= 0) return null;

            var cupboard = block.GetBuildingPrivilege();
            if (cupboard == null) return null;

            var hasAuth = false;
            var userId = player.userID;

            foreach(var ap in cupboard.authorizedPlayers)
            {
                if (ap.userid == userId)
                {
                    hasAuth = true;
                    break;
                }
            }

            if (hasAuth) return null;

            var blocks = Facepunch.Pool.Get<HashSet<BuildingBlock>>();
            try 
            {
                GetBlocksFromIDNoAlloc(block.buildingID, ref blocks);

                if (blocks.Count >= MAX_ENTITIES_BEFORE_CUPBOARD_REQUIRED)
                {
                    var sb = Facepunch.Pool.Get<StringBuilder>();
                    try { return sb.Clear().Append("<color=yellow>You cannot remove this base without being authorized to the Tool Cupboard</color><color=red>!</color>").ToString(); }
                    finally { Facepunch.Pool.Free(ref sb); }
                }
                else
                {
                    var ownedCount = 0;
                    foreach (var buildingBlock in blocks)
                    {
                      
                        if (buildingBlock != null && buildingBlock.grade > BuildingGrade.Enum.Twigs && buildingBlock.OwnerID == userId)
                        {
                            ownedCount++;
                        }
                    }

                    if (ownedCount < (blocks.Count - ownedCount + 1))
                    {
                        //ownedCount was not a majority!
                        return "<color=#db3716><size=20>You are unable to remove this object!</size></color>" + Environment.NewLine + "<color=#ff6017>You're not authorized to this building's <color=#cf9d21>Tool Cupboard</color> and you also do <i>not</i> own a majority of this building.</color>";
                    }
                }

            }
            finally { Facepunch.Pool.Free(ref blocks); }
            /*/
            var ownedCount = 0;
            foreach (var b in blocks)
            {
                var buildingBlock = b as BuildingBlock;
                if (buildingBlock != null && buildingBlock.grade > BuildingGrade.Enum.Twigs && buildingBlock.OwnerID == userId)
                {
                    ownedCount++;
                }
            }

            if (ownedCount < (blocks.Count - ownedCount + 1))
            {
                //ownedCount was not a majority!
                return "<color=#db3716><size=20>You are unable to remove this object!</size></color>" + Environment.NewLine + "<color=#ff6017>You're not authorized to this building's <color=#cf9d21>Tool Cupboard</color> and you also do <i>not</i> own a majority of this building.</color>";
            }/*/

           // var count = GetBlockCountFromID(block.buildingID, MAX_ENTITIES_BEFORE_CUPBOARD_REQUIRED);
        

            return null;
        }
        #endregion
        #region Util
        private void GetBlocksFromIDNoAlloc(uint buildingId, ref HashSet<BuildingBlock> blocks)
        {
            if (blocks == null) throw new ArgumentNullException(nameof(blocks));
            if (buildingId <= 0) return;

            foreach(var entity in BaseEntity.saveList)
            {
                var block = entity as BuildingBlock;
                if (block?.buildingID == buildingId) blocks.Add(block);
            }
        }

        private int GetBlockCountFromID(uint buildingId, int maxToCheck = 0)
        {
            if (maxToCheck < 0) throw new ArgumentOutOfRangeException(nameof(maxToCheck));

            var count = 0;
            var hasMax = maxToCheck > 0;

            foreach (var block in _blocks)
            {
                if (hasMax && count >= maxToCheck) break;
                if (block?.buildingID == buildingId) count++;
            }

            return count;
        }

        private bool IsEscapeBlocked(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            return NoEscape?.Call<bool>("IsEscapeBlockedS", userID) ?? false;
        }
        #endregion
    }
}