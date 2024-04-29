using Facepunch;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("BuildLimiter", "Shady", "1.2.22", ResourceId = 0000)]
    internal class BuildLimiter : RustPlugin
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
        private const int GLOBAL_MAX_LIMIT = 8192;
        private const int SHOW_GLOBAL_WARNING_START = 6144;
        private const int BUILDING_MAX_LIMIT = 4096;
        private const int SHOW_BUILDING_WARNING_START = 1536;

        private const string CHAT_STEAM_ID = "76561198865536053";

        private readonly HashSet<string> _forbiddenTags = new() { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        [PluginReference]
        private readonly Plugin Clans;

        private readonly Dictionary<string, float> _lastWarningTime = new(256);

        private readonly HashSet<BuildingBlock> _blocks = new();

        private class BlockCache
        {
            public int CachedCount { get; set; }
            public float LastCacheTime { get; set; }
        }


        private readonly Dictionary<uint, BlockCache> _blockCaches = new(512);

        private readonly Dictionary<string, BlockCache> globalCaches = new(512);

        private readonly Dictionary<string, int> _lastWarningShownBlockCount = new(256);

        private void Init()
        {
            AddCovalenceCommand("entities", nameof(cmdEntities));
        }

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

            if (block != null)
            {
                if (_blockCaches.TryGetValue(block.buildingID, out var cache)) cache.CachedCount++;

                _blocks.Add(block);
            }

        }

        private object CanBuild(Planner planner, Construction prefab, Construction.Target target)
        {
            if (planner == null || prefab == null) return null;

            if (prefab?.deployable != null) return null;

            var block = target.entity as BuildingBlock;
            if (block == null || block.buildingID == 0 || block.OwnerID == 0) return null;

            var player = planner?.GetOwnerPlayer() ?? null;
            if (player == null) return null;


            var wasCached = false;

            var globalBlockCount = 0;

            var individualBlockCount = 0;


            var watch = Facepunch.Pool.Get<Stopwatch>();
            try 
            {
                watch.Restart();

                var clanTag = Clans?.Call<string>("GetClanOf", player.UserIDString) ?? string.Empty;

                BlockCache cache;

                if (string.IsNullOrEmpty(clanTag))
                {
                    if (!globalCaches.TryGetValue(player.UserIDString, out cache))
                    {
                        cache = new BlockCache();
                        globalCaches[player.UserIDString] = cache;
                    }
                }
                else
                {
                    if (!globalCaches.TryGetValue(clanTag, out cache))
                    {
                        cache = new BlockCache();
                        globalCaches[clanTag] = cache;
                    }
                }

                

                var now = Time.realtimeSinceStartup;

                var globalLimit = GLOBAL_MAX_LIMIT;
                var buildingLimit = BUILDING_MAX_LIMIT;


                object[] globalArguments = { player, clanTag, globalLimit };
                object[] buildArguments = { player, block, buildingLimit };

                Interface.Oxide.CallHook("OnGetGlobalBuildLimit", globalArguments);
                globalLimit = Convert.ToInt32(globalArguments[2]);
                

                Interface.Oxide.CallHook("OnGetBuildLimit", buildArguments);
                buildingLimit = Convert.ToInt32(buildArguments[2]);
                
                if (buildingLimit < 1)
                    PrintError(nameof(buildingLimit) + " is somehow < 1!!!: " + player + ", " + block + ", build id " + block?.buildingID);

                if (globalLimit < 1)
                    PrintError(nameof(globalLimit) + " is somehow < 1!!!: " + player + ", " + block + ", " + "build id " + block?.buildingID);

                if (!GetCachedCount(cache, globalLimit, out globalBlockCount))
                {
                    var members = string.IsNullOrEmpty(clanTag) ? null : Clans?.Call<List<ulong>>("GetClanMembers", player.userID) ?? null;

                    globalBlockCount = (members == null || members.Count < 1) ? GetBlockCountForUser(player.userID, globalLimit) : GetBlockCountForClan(members, globalLimit);
                    //only checks up until max limit, that way we aren't counting overly large bases (that may have already existed) for no reason

                    cache.CachedCount = globalBlockCount;
                    cache.LastCacheTime = now;
                }
                else wasCached = true;


                if (!_blockCaches.TryGetValue(block.buildingID, out var individualCache)) 
                    individualCache = new BlockCache();



                if (!GetCachedCount(individualCache, buildingLimit, out individualBlockCount))
                {
                    individualBlockCount = GetBlockCountFromID(block.buildingID, buildingLimit);

                    individualCache.CachedCount = individualBlockCount;
                    cache.LastCacheTime = now;
                }
                else wasCached = true;


                if (HasHitBuildLimit(cache, globalLimit, out var reason) || HasHitBuildLimit(individualCache, buildingLimit, out reason))
                {
                    SendReply(player, reason);
                    return false;
                }
                else if (ShouldShowWarningMessage(player.UserIDString, cache.CachedCount, true, out reason) || ShouldShowWarningMessage(player.UserIDString, individualCache.CachedCount, false, out reason))
                {
                    SendReply(player, reason);
                }
            }
            finally 
            {
                watch.Stop();
                if (watch.Elapsed.TotalMilliseconds > (wasCached ? 8 : 15)) PrintWarning("CanBuild took: " + watch.Elapsed.TotalMilliseconds + "ms + (cached: " + wasCached + ", individualCount: " + individualBlockCount.ToString("N0") + ", global: " + globalBlockCount.ToString("N0") + ")");
                Facepunch.Pool.Free(ref watch); 
            }
           
            return null;
        }
        
        public bool IsDivisble(int x, int n) { return (x % n) == 0; }

        private bool HasHitBuildLimit(BlockCache cache, int limit, out string reason)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            reason = string.Empty;

            var blockCount = cache.CachedCount;

            if (blockCount >= limit)
            {
                reason = "You cannot build anymore! You've reached the maximum entity count (" + limit.ToString("N0") + ").";
                return true;
            }

            return false;
        }

        private bool ShouldShowWarningMessage(string userId, int blockCount, bool globalCount, out string warning)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (blockCount < 0) throw new ArgumentOutOfRangeException(nameof(blockCount));

            warning = string.Empty;

            if (blockCount < (globalCount ? SHOW_GLOBAL_WARNING_START : SHOW_BUILDING_WARNING_START)) return false;

            var divis = IsDivisble(blockCount, 25);
            var now = Time.realtimeSinceStartup;
            if (divis || (!_lastWarningTime.TryGetValue(userId, out var lastTime) || (now - lastTime) >= 180))
            {
                if (divis)
                {
                    return !(_lastWarningShownBlockCount.TryGetValue(userId, out var lastCount) && lastCount == blockCount);
                }

                _lastWarningTime[userId] = now;
                _lastWarningShownBlockCount[userId] = blockCount;

                warning = "<color=#F40009>You are nearing the build limit!</color> <color=#689952>" + blockCount.ToString("N0") + "</color>/<color=#F40009>" + (globalCount ? GLOBAL_MAX_LIMIT : BUILDING_MAX_LIMIT).ToString("N0") + "</color>";

                return true;
            }

            return false;

        }

        private bool GetCachedCount(BlockCache cache, int count, out int result)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            
            var now = Time.realtimeSinceStartup;

            var maxCountDiff = (cache == null) ? -1 : (count - cache.CachedCount);
            if (maxCountDiff >= 0 && maxCountDiff >= 175 && (now - cache.LastCacheTime) < (maxCountDiff >= 10240 ? 901 : maxCountDiff >= 7168 ? 721 : maxCountDiff >= 6144 ? 631 : maxCountDiff >= 4096 ? 421 : maxCountDiff >= 2048 ? 241 : maxCountDiff >= 768 ? 121 : 61))
            {
                result = cache.CachedCount;

                return true;
            }

            result = 0;

            return false;
        }

        private int GetBlockCountFromID(uint buildingId, int maxToCheck = 0)
        {
            if (maxToCheck < 0) throw new ArgumentOutOfRangeException(nameof(maxToCheck));

            var count = 0;
            var hasMax = maxToCheck > 0;

            foreach(var block in _blocks)
            {
                if (hasMax && count >= maxToCheck) break;
                if (block?.buildingID == buildingId) count++;
            }

            return count;
        }

        private int GetBlockCountForClan(List<ulong> members, int maxToCheck = 0)
        {
            if (members == null) throw new ArgumentNullException(nameof(members));
            if (members.Count < 1) throw new ArgumentOutOfRangeException(nameof(members));
            if (maxToCheck < 0) throw new ArgumentOutOfRangeException(nameof(maxToCheck));

            var count = 0;
            var hasMax = maxToCheck > 0;

            foreach (var block in _blocks)
            {
                if (hasMax && count >= maxToCheck) break;
                else if (block?.OwnerID <= 0) continue;
                else if (members.Contains(block.OwnerID)) count++;
            }

            return count;
        }

        private int GetBlockCountForUser(ulong userId, int maxToCheck = 0)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (maxToCheck < 0) throw new ArgumentOutOfRangeException(nameof(maxToCheck));

            var count = 0;
            var hasMax = maxToCheck > 0;

            foreach (var block in _blocks)
            {
                if (hasMax && count >= maxToCheck) break;
                else if (block?.OwnerID == userId) count++;
            }

            return count;
        }


        private void cmdEntities(IPlayer player, string command, string[] args)
        {
            if (player == null || player.IsServer) return;

            if (!ulong.TryParse(player.Id, out var ulUserId))
            {
                SendReply(player, "Could not parse your ID as ulong!!!");
                return;
            }


            var clanTag = Clans?.Call<string>("GetClanOf", player.Id) ?? string.Empty;

            var globalLimit = GLOBAL_MAX_LIMIT;

            object[] globalArguments = { player, clanTag, globalLimit };

            Interface.Oxide.CallHook("OnGetGlobalBuildLimit", globalArguments);
            globalLimit = Convert.ToInt32(globalArguments[2]);

            BlockCache cache;

            if (string.IsNullOrEmpty(clanTag))
            {
                if (!globalCaches.TryGetValue(player.Id, out cache))
                {
                    cache = new BlockCache();
                    globalCaches[player.Id] = cache;
                }
            }
            else
            {
                if (!globalCaches.TryGetValue(clanTag, out cache))
                {
                    cache = new BlockCache();
                    globalCaches[clanTag] = cache;
                }
            }



            var now = UnityEngine.Time.realtimeSinceStartup;

            if (!GetCachedCount(cache, globalLimit, out var globalBlockCount))
            {
                var members = string.IsNullOrEmpty(clanTag) ? null : Clans?.Call<List<ulong>>("GetClanMembers", ulUserId) ?? null;

                globalBlockCount = (members == null || members.Count < 1) ? GetBlockCountForUser(ulUserId, globalLimit) : GetBlockCountForClan(members, globalLimit);
                //only checks up until max limit, that way we aren't counting overly large bases (that may have already existed) for no reason

                cache.CachedCount = globalBlockCount;
                cache.LastCacheTime = now;
            }

            SendReply(player, "Current global build info:\n\nGlobal building entity count (includes clan members): " + globalBlockCount.ToString("N0") + "/" + globalLimit);

        }

        private void SendReply(IPlayer player, string msg, string userId = CHAT_STEAM_ID, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : keepTagsConsole ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else
            {
#if RUST
                player.Command("chat.add", string.Empty, userId, msg);
#else
                player.Reply(msg);
#endif
            }
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;


            //	Replace Color Tags
            phrase = _colorRegex.Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = _sizeRegex.Replace(phrase, string.Empty);

            var phraseSB = Pool.Get<StringBuilder>();
            try
            {
                phraseSB.Clear().Append(phrase);

                foreach (var tag in _forbiddenTags)
                    phraseSB.Replace(tag, string.Empty);

                return phraseSB.ToString();
            }
            finally { Pool.Free(ref phraseSB); }
        }
    }
}
 