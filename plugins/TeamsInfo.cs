﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("TeamsInfo", "Shady", "0.0.2", ResourceId = 0)]
    internal class TeamsInfo : RustPlugin
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
        private const string ChatSteamID = "76561198865536053";

        private void Init()
        {
            AddCovalenceCommand("teaminfo", nameof(cmdTeamInfo));
        }

        private void SendReply(IPlayer player, string msg, string userId = ChatSteamID, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : (keepTagsConsole) ? msg : RemoveTags(msg);
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

        private void SendReply(BasePlayer player, string msg, string userId = ChatSteamID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
        }

        private void cmdTeamInfo(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                SendReply(player, "You must supply a target player to lookup team info!");
                return;
            }
            var ins = RelationshipManager.ServerInstance;
            if (ins == null)
            {
                SendReply(player, "No RelationshipManager Instance was found!");
                return;
            }
            var joinArgs = string.Join("", args);

            var target = FindPlayerByPartialName(joinArgs, true) ?? FindPlayerByID(joinArgs);
            if (target == null)
            {
                SendReply(player, "No player found with that name!");
                return;
            }
            if (target?.Team == null)
            {
                SendReply(player, "This player does not have a team!");
                return;
            }
            var team = target.Team;
            var teamSB = new StringBuilder("This team has " + team.members.Count.ToString("N0") + " members:" + Environment.NewLine);
            for(int i = 0; i < team.members.Count; i++)
            {
                var mem = team.members[i];
                var ply = covalence.Players?.FindPlayerById(mem.ToString()) ?? null;
                var isConnected = ply?.IsConnected ?? false;
                teamSB.AppendLine((team.teamLeader == mem ? "<size=11>(<color=#b86406>*</color>)</size> " : string.Empty) + ply?.Name + " (<color=" + (isConnected ? "green" : "red") + ">" + (isConnected ? "Connected" : "Offline") + "</color>)");
            }
            SendReply(player, teamSB.ToString().TrimEnd());
        }

        private string RemoveTags(string phrase)
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

            for (int i = 0; i < forbiddenTags.Count; i++) phraseSB.Replace(forbiddenTags[i], string.Empty);
            

            return phraseSB.ToString();
        }

        public const string VALID_CHARACTERS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=[]{}\|\'\"",./`*!@#$%^&*()_+<>?~;: "; //extra space at end is to allow space as a valid character

        public string[] GetValidCharactersString()
        {
            var length = VALID_CHARACTERS.Length;
            var array = new string[length];
            for (int i = 0; i < length; i++) array[i] = VALID_CHARACTERS[i].ToString();
            return array;
        }

        public char[] GetValidCharacters() { return VALID_CHARACTERS.ToCharArray(); }

        private int Fitness(string individual, string target)
        {
            var count = 0;
            var range = Enumerable.Range(0, Math.Min(individual.Length, target.Length));
            foreach (var i in range)
            {
                if (individual[i] == target[i]) count++;
            }
            return count;
            //return Enumerable.Range(0, Math.Min(individual.Length, target.Length)).Count(i => individual[i] == target[i]);
        }

        private int ExactMatch(string comp1, string comp2, StringComparison options = StringComparison.CurrentCulture)
        {
            if (string.IsNullOrEmpty(comp1) || string.IsNullOrEmpty(comp2)) return 0;
            var val = 0;


            if (comp1.Length > 0 && comp2.Length > 0)
            {
                for (int i = 0; i < comp1.Length; i++)
                {
                    if ((comp2.Length - 1) >= i)
                    {
                        if (comp2[i].ToString().Equals(comp1[i].ToString(), options)) val++;
                    }
                }
            }

            return val;
        }

        private string CleanPlayerName(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            var strSB = new StringBuilder();
            var valid = GetValidCharactersString();
            for (int i = 0; i < str.Length; i++)
            {
                var chrStr = str[i].ToString();
                var skip = true;
                for (int j = 0; j < valid.Length; j++)
                {
                    var v = valid[j];
                    if (v.Equals(chrStr, StringComparison.OrdinalIgnoreCase))
                    {
                        skip = false;
                        break;
                    }
                }
                if (!skip) strSB.Append(chrStr);
            }
            return strSB.ToString().TrimStart().TrimEnd();
        }

        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            BasePlayer player = null;
            var matches = Facepunch.Pool.GetList<BasePlayer>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }

                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(p);
                    }
                }
                if (sleepers)
                {

                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matches.Add(p);
                        }
                    }
                }
                var topMatch = matches?.OrderByDescending(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name, StringComparison.OrdinalIgnoreCase)) ?? null;
                if (topMatch != null && topMatch.Any())
                {
                    var exactMatches = matches?.Select(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name, StringComparison.OrdinalIgnoreCase))?.OrderByDescending(p => p) ?? null;
                    if (exactMatches.All(p => p == 0))
                    {
                        topMatch = matches?.OrderByDescending(p => Fitness(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                    }
                }
                player = topMatch?.FirstOrDefault() ?? null;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            Facepunch.Pool.FreeList(ref matches);
            return player;
        }

        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false, params BasePlayer[] ignore)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            BasePlayer player = null;
            var matches = Facepunch.Pool.GetList<BasePlayer>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || (ignore != null && ignore.Length > 0 && ignore.Contains(p))) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }

                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null || (ignore != null && ignore.Length > 0 && ignore.Contains(p))) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || ignore.Contains(p)) continue;
                    var pName = p?.displayName ?? string.Empty;
                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(p);
                    }
                }
                if (sleepers)
                {

                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null || ignore.Contains(p)) continue;
                        var pName = p?.displayName ?? string.Empty;
                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matches.Add(p);
                        }
                    }
                }
                var topMatch = matches?.OrderByDescending(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                if (topMatch != null && topMatch.Any())
                {
                    var exactMatches = matches?.Select(p => ExactMatch(CleanPlayerName(p?.displayName) ?? p?.displayName, name))?.OrderByDescending(p => p) ?? null;
                    if (exactMatches.All(p => p == 0))
                    {
                        topMatch = matches?.OrderByDescending(p => Fitness(CleanPlayerName(p?.displayName) ?? p?.displayName, name)) ?? null;
                    }
                }
                player = topMatch?.FirstOrDefault() ?? null;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            Facepunch.Pool.FreeList(ref matches);
            return player;
        }

        private BasePlayer FindPlayerByID(string UserIDString)
        {
            if (string.IsNullOrEmpty(UserIDString)) return null;
            if (BasePlayer.activePlayerList != null && BasePlayer.activePlayerList.Count > 0)
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p != null && p.UserIDString == UserIDString) return p;
                }
            }
            if (BasePlayer.sleepingPlayerList != null && BasePlayer.sleepingPlayerList.Count > 0)
            {
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var p = BasePlayer.sleepingPlayerList[i];
                    if (p != null && p.UserIDString == UserIDString) return p;
                }
            }
            return null;
        }

    }
}
