using System.Collections.Generic;
using System.Linq;
using Oxide.Core.Plugins;
using System;
using System.Text;
using System.Diagnostics;
using Oxide.Core.Libraries.Covalence;
using System.Text.RegularExpressions;

namespace Oxide.Plugins
{
    [Info("PVPStats", "Shady", "0.0.4", ResourceId = 0000)]
    class PVPStats : RustPlugin
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
        //this is one of the most disgustingly written plugins i have ever made

        [PluginReference]
        Plugin LocalStatsAPI;


        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        void Init()
        {
            AddCovalenceCommand("gatherstats", nameof(cmdGatherStats));
            AddCovalenceCommand("gatherstat2", nameof(cmdGatherStatsTest));
        }


        Dictionary<string, object> GetLocalStats(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            var stats = LocalStatsAPI?.Call<Dictionary<string, object>>("GetStatsDict", userId) ?? null;
            return stats;
        }

        long GetStat(string key, Dictionary<string, object> stats)
        {
            if (string.IsNullOrEmpty(key) || stats == null) throw new ArgumentNullException();
            object obj;
            var stat = 0L;
            if (!stats.TryGetValue(key, out obj) || !long.TryParse(obj.ToString(), out stat)) stat = -1;
            return stat;
        }

        object GetStat2(string userId, string stat)
        {
            return LocalStatsAPI?.Call("GetStat", userId, stat) ?? null;
        }

        void SendReply(IPlayer player, string msg, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : (keepTagsConsole) ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else player.Reply(msg);
        }

        string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;
            //	Forbidden formatting tags
            var forbiddenTags = new List<string>{
                "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>"
            };

            //	Replace Color Tags
            phrase = new Regex("(<color=.+?>)").Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = new Regex("(<size=.+?>)").Replace(phrase, string.Empty);
            var phraseSB = new StringBuilder(phrase);
            for (int i = 0; i < forbiddenTags.Count; i++)
            {
                var tag = forbiddenTags[i];
                phraseSB.Replace(tag, string.Empty);
            }

            return phraseSB.ToString();
        }


        private void cmdGatherStats(IPlayer player, string command, string[] args)
        {
            var watch = Stopwatch.StartNew();
            if (LocalStatsAPI == null || !LocalStatsAPI.IsLoaded)
            {
                SendReply(player, "No Local Stats API! (A required plugin is not loaded)");
                return;
            }

            var allGathers = new Dictionary<string, Dictionary<int, long>>();

            var players = Facepunch.Pool.GetList<string>();
            try 
            {
                GetAllPlayerIDsNoAlloc(ref players);

                if (players.Count < 1)
                {
                    PrintWarning("no plyaers from ref!");
                    return;
                }

                for (int i = 0; i < players.Count; i++)
                {
                    var userId = players[i];

                    var gathers = GetStat2(userId, "resourcesGathered") as Dictionary<int, long>;
                    if (gathers == null || gathers.Count < 1) continue;

                    allGathers[userId] = gathers;
                }
            }
            finally
            {
                Facepunch.Pool.FreeList(ref players);
            }


            if (args.Length > 0 && args[0].Equals("top", StringComparison.OrdinalIgnoreCase))
            {

                var tuples = new HashSet<Tuple<string, int, long>>();
                foreach (var kvp in allGathers)
                {
                    foreach (var kvp2 in kvp.Value)
                    {
                        var newTuple = new Tuple<string, int, long>(kvp.Key, kvp2.Key, kvp2.Value);
                        tuples.Add(newTuple);
                    }
                }

                var orderedTuples = tuples.OrderByDescending(p => p.Item3);

                var topSB = new StringBuilder();
                var topSBs = Facepunch.Pool.GetList<StringBuilder>();

                Dictionary<int, long> totalGathered = new Dictionary<int, long>();

                foreach(var tuple2 in orderedTuples)
                {
                    long gather;
                    if (!totalGathered.TryGetValue(tuple2.Item2, out gather)) totalGathered[tuple2.Item2] = tuple2.Item3;
                    else totalGathered[tuple2.Item2] += tuple2.Item3;
                }

                foreach (var tuple in orderedTuples.Take(10))
                {
                    var totalGather = 0L;
                    totalGathered.TryGetValue(tuple.Item2, out totalGather);
                  
                    var percGather = ((float)tuple.Item3 / (float)totalGather) * 100;
                    var str = "<color=#80ffff>" + GetDisplayNameFromID(tuple.Item1) + "</color>: <color=#ffc34d>" + (ItemManager.FindItemDefinition(tuple.Item2)?.displayName?.english ?? "Unknown") + "</color>: <color=#70db70>" + tuple.Item3.ToString("N0") + "</color> (<color=#f14ff9>" + percGather.ToString("0.00").Replace(".00", string.Empty) + "%</color><color=#8aff47> of all</color>)";
                    if (!player.IsServer && (topSB.Length + str.Length >= 896))
                    {
                        topSBs.Add(topSB);
                        topSB = new StringBuilder();
                    }
                    topSB.AppendLine(str);
                }

                if (topSBs.Count < 1 && topSB.Length < 1)SendReply(player, "No top stats!");
                else SendReply(player, "<color=#b1e853>Showing top <color=#8aff47>10</color> gathering stats</color>:");

                if (topSBs.Count > 0) for (int i = 0; i < topSBs.Count; i++) SendReply(player, topSBs[i].ToString().TrimEnd());
                if (topSB.Length > 0) SendReply(player, topSB.ToString().TrimEnd());


                Facepunch.Pool.FreeList(ref topSBs);
                PrintWarning(command + " " + string.Join(" ", args) + " : " + watch.Elapsed.TotalMilliseconds + "ms");
                return;
            }

            var gathersOnly = Facepunch.Pool.GetList<Dictionary<int, long>>();

            foreach(var kvp in allGathers)
            {
                gathersOnly.Add(kvp.Value);
            }

            var sumTotal = new Dictionary<int, long>();
            long outSum;

            foreach (var shit2 in gathersOnly)
            {
               foreach(var shit3 in shit2)
                {
                    if (!sumTotal.TryGetValue(shit3.Key, out outSum)) sumTotal[shit3.Key] = shit3.Value;
                    else sumTotal[shit3.Key] += shit3.Value;
                }
            }
            var sbList = Facepunch.Pool.GetList<StringBuilder>();
            var sb2 = new StringBuilder();
            foreach (var kvp in sumTotal.OrderByDescending(p => p.Value))
            {
                var str = "<color=#ffc34d>" + (ItemManager.FindItemDefinition(kvp.Key)?.displayName?.english ?? (kvp.Key.ToString())) + "</color>: <color=#70db70>" + kvp.Value.ToString("N0") + "</color>";
                if ((sb2.Length + str.Length) >= 896)
                {
                    sbList.Add(sb2);
                    sb2 = new StringBuilder();
                }
                sb2.AppendLine(str);
            }

            if (sbList.Count > 0 || sb2.Length > 0) SendReply(player, "<color=#80ffff>Total resources gathered</color>:");
            else SendReply(player, "No resources gathered!");
            if (sbList.Count > 0) for (int i = 0; i < sbList.Count; i++) SendReply(player, sbList[i].ToString().TrimEnd());
            if (sb2.Length > 0) SendReply(player, sb2.ToString().TrimEnd());
            SendReply(player, "To view the top player gather stats, type <color=#70db70>/" + command + " top</color>.");
            Facepunch.Pool.FreeList(ref gathersOnly);
            Facepunch.Pool.FreeList(ref sbList);
            watch.Stop();
            PrintWarning(command + " " + args + " : " + watch.Elapsed.TotalMilliseconds + "ms");
        }

        private void cmdGatherStatsTest(IPlayer player, string command, string[] args)
        {
            if (LocalStatsAPI == null || !LocalStatsAPI.IsLoaded)
            {
                SendReply(player, "No Local Stats API! (A required plugin is not loaded)");
                return;
            }


            var gathers = GetStat2(player.Id, "resourcesGathered") as Dictionary<int, long>;
            if (gathers == null || gathers.Count < 1)
            {
                SendReply(player, "you have no resources gathered lol");
                return;
            }

            var sb = Facepunch.Pool.Get<StringBuilder>();

            try
            {

                sb.Clear();

                foreach (var kvp in gathers)
                    sb.Append(ItemManager.FindItemDefinition(kvp.Key)?.shortname).Append(": ").Append(kvp.Value).Append(Environment.NewLine);
                

                sb.Length -= 1;

                SendReply(player, sb.ToString());

            }
            finally { Facepunch.Pool.Free(ref sb); }

        }

        [ChatCommand("pvpstats")]
        private void cmdKdTop(BasePlayer player, string command, string[] args)
        {
            var watch = Stopwatch.StartNew();
            if (LocalStatsAPI == null || !LocalStatsAPI.IsLoaded)
            {
                SendReply(player, "No Local Stats API! (A required plugin is not loaded)");
                return;
            }
            var allPKills = new Dictionary<string, long>();
            var allPDeaths = new Dictionary<string, long>();
            var allShots = new Dictionary<string, long>();
            var allKDs = new Dictionary<string, double>();
            var allAccuracy = new Dictionary<string, double>();
            var allHS = new Dictionary<string, long>();
            var pTime = Stopwatch.StartNew();

            var players = Facepunch.Pool.GetList<string>();
            try 
            {

                GetAllPlayerIDsNoAlloc(ref players);

                if (players.Count < 1)
                {
                    SendReply(player, "No players found!");
                    return;
                }

                for (int i = 0; i < players.Count; i++)
                {
                    var userId = players[i];

                    var stats = GetLocalStats(userId);
                    if (stats == null || stats.Count < 1) continue;

                    var ks = GetStat("pvpkills", stats);
                    var ds = GetStat("pvpdeaths", stats);
                    var shots = GetStat("shots", stats);
                    var bodyparts = stats["bodypartshit"] as Dictionary<string, long>;
                    var headshots = 0L;
                    if (bodyparts != null && bodyparts.TryGetValue("Head", out headshots)) allHS[userId] = headshots;
                    var projHits = GetStat("projhits", stats);
                    if (projHits >= 500) allAccuracy[userId] = (double)projHits / (double)shots;
                    if (shots > 0) allShots[userId] = shots;
                    if (ks > 0)
                    {
                        allPKills[userId] = ks;
                        allPDeaths[userId] = ds;
                    }
                }

            }
            finally { Facepunch.Pool.FreeList(ref players); }



            PrintWarning("players all time took: " + pTime.ElapsedMilliseconds + "ms");
            pTime.Stop();

            foreach (var kvp in allPKills)
            {
                if (kvp.Value < 200) continue;
                var deaths = 0L;
                allPDeaths.TryGetValue(kvp.Key, out deaths);
                var kd = (double)kvp.Value / (double)(deaths);
                allKDs[kvp.Key] = kd;
            }
            var topKills = (from entry in allPKills orderby entry.Value descending select entry).Take(5);
            var topDeaths = (from entry in allPDeaths orderby entry.Value descending select entry).Take(5);
            var topShots = (from entry in allShots orderby entry.Value descending select entry).Take(5);
            var topKDs = (from entry in allKDs orderby entry.Value descending select entry).Take(5);
            var topAccuracy = (from entry in allAccuracy orderby entry.Value descending select entry).Take(5);
            var topHS = (from entry in allHS orderby entry.Value descending select entry).Take(5);

            var topSB = new StringBuilder();
            var topSB2 = new StringBuilder();
            var topSB3 = new StringBuilder();
            var topSB4 = new StringBuilder();
            var topSB5 = new StringBuilder();
            var topSB6 = new StringBuilder();
        
            foreach (var k in topKills) topSB.AppendLine("<color=orange>" + GetDisplayNameFromID(k.Key) + "</color>: <color=#8aff47>" + k.Value.ToString("N0") + "</color> kills");
            foreach (var k in topDeaths) topSB2.AppendLine("<color=red>" + GetDisplayNameFromID(k.Key) + "</color>: <color=orange>" + k.Value.ToString("N0") + "</color> deaths");
            topSB3.AppendLine("Top KDs are only counted for players with 200 or more kills.");
            if (topKDs != null && topKDs.Any())
            {
                foreach (var k in topKDs) topSB3.AppendLine("<color=#42dfff>" + GetDisplayNameFromID(k.Key) + "</color>: <color=#eb3dff>" + k.Value.ToString("0.00").Replace(".00", string.Empty) + "</color> K/D Ratio");
            }
            foreach (var k in topShots) topSB4.AppendLine("<color=#eb3dff>" + GetDisplayNameFromID(k.Key) + "</color>: <color=orange>" + k.Value.ToString("N0") + "</color> shots fired");
            foreach (var k in topAccuracy) topSB5.AppendLine("<color=#8aff47>" + GetDisplayNameFromID(k.Key) + "</color>: <color=#42dfff>" + (k.Value * 100).ToString("0.00").Replace(".00", string.Empty) + "</color>% accuracy");
            foreach (var k in topHS) topSB6.AppendLine("<color=#8aff47>" + GetDisplayNameFromID(k.Key) + "</color>: <color=red>" + k.Value.ToString("N0") + "</color> headshots");
            if (topSB.Length > 0 || topSB2.Length > 0) SendReply(player, topSB.ToString().TrimEnd() + "\n\n" + topSB2.ToString().TrimEnd());
            if (topSB3.Length > 0 || topSB4.Length > 0) SendReply(player, topSB3.ToString().TrimEnd() + "\n\n" + topSB4.ToString().TrimEnd());
            if (topSB5.Length > 0 || topSB6.Length > 0) SendReply(player, topSB5.ToString().TrimEnd() + "\n\n" + topSB6.ToString().TrimEnd());
            PrintWarning("pvpstats took: " + watch.Elapsed.TotalMilliseconds + "ms");
        }

        string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p?.UserIDString == userID) return p.displayName;
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var p = BasePlayer.sleepingPlayerList[i];
                if (p?.UserIDString == userID) return p.displayName;
            }
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty; //?? BasePlayer.activePlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? BasePlayer.sleepingPlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? string.Empty;
        }

        private void GetAllPlayerIDsNoAlloc(ref List<string> list)
        {
            PlayersByDatabase?.Call("GetAllPlayerIDsNoAlloc", list);
        }
      
    }
}