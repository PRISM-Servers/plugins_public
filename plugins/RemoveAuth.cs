using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Remove Authorization", "Shady", "1.0.1")]
    internal class RemoveAuth : RustPlugin
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
        private const string DATA_FILE_NAME = "RemoveFriendData";
        private const string CHAT_STEAM_ID = "76561198865536053";

        private readonly HashSet<string> _forbiddenTags = new() { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private RemoveData _removeData;

        [PluginReference] private readonly Plugin PlayersByDatabase;


        #endregion
        #region Hooks
        private void Init()
        {
            _removeData = Interface.Oxide?.DataFileSystem?.ReadObject<RemoveData>(DATA_FILE_NAME) ?? new RemoveData();

            string[] remCmds = { "remauth", "removeauth", "remfriend" };
            AddCovalenceCommand(remCmds, nameof(cmdRemoveAuth));
        }
        private void Unload() => SaveRemoveData();
        #endregion
        #region Command
        private void cmdRemoveAuth(IPlayer player, string command, string[] args)
        {
            var friends = _removeData?.GetFriends(player.Id) ?? null;
            if (args?.Length < 1)
            {
                SendReply(player, "<color=#00e600>----<color=#996633>Remover Tool Authorization</color>----</color>\n-You can allow your friends to use <color=orange>/remove</color> on your buildings/objects by typing <color=#ff912b>/" + command + " add PlayerName</color>.\n-You can deauthorize a player using <color=#ff912b>/" + command + " remove PlayerName</color>.");

                var sbList = Facepunch.Pool.GetList<StringBuilder>();
                try
                {
                    var friendSB = new StringBuilder();
                    if (friends != null && friends.Count > 0)
                    {
                        for (int i = 0; i < friends.Count; i++)
                        {
                            var friend = friends[i];
                            var str = "<color=#8aff47>" + GetDisplayNameFromID(friend) + "</color>";
                            if ((friendSB.Length + str.Length) >= 800)
                            {
                                sbList.Add(friendSB);
                                friendSB = new StringBuilder();
                            }
                            friendSB.AppendLine(str);
                        }
                    }

                    if (friendSB.Length > 0 || sbList.Count > 0) SendReply(player, "Authorized players:");
                    if (sbList.Count > 0) for (int i = 0; i < sbList.Count; i++) SendReply(player, sbList[i].ToString().TrimEnd());
                    if (friendSB.Length > 0) SendReply(player, friendSB.ToString().TrimEnd());
                    if (friends == null || friends?.Count < 1) SendReply(player, "You currently have no players authorized to remove your objects.");

                    return;
                }
                finally { Facepunch.Pool.FreeList(ref sbList); }

            }
            var arg0Lower = args[0].ToLower();
            if (arg0Lower != "add" && arg0Lower != "remove")
            {
                SendReply(player, "Invalid argument supplied, try <color=#ff912b>/" + command + " remove</color> or <color=#ff912b>/" + command + " add</color>");
                return;
            }
            if (args.Length < 2)
            {
                SendReply(player, "You must supply a player name!");
                return;
            }
            IPlayer target = null;
            try
            {
                if (friends?.Count > 0)
                {
                    var findFriends = Facepunch.Pool.GetList<string>();
                    try
                    {
                        for (int i = 0; i < friends.Count; i++)
                        {
                            var friend = friends[i];
                            var friendName = GetDisplayNameFromID(friend);
                            if (string.IsNullOrEmpty(friendName)) continue;
                            if (friendName.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0) findFriends.Add(friend);
                        }
                        if (findFriends.Count == 1) target = covalence.Players?.FindPlayerById(findFriends[0]) ?? null;
                    }
                    finally { Facepunch.Pool.FreeList(ref findFriends); }
                }
                target ??= covalence.Players?.FindPlayerById(FindPlayerByPartialName(args[1], true)?.UserIDString ?? string.Empty);
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^^ Exception on getting target player for remauth ^^"); }

            if (target == null)
            {
                SendReply(player, "Failed to find a player with the name of: " + args[1]);
                return;
            }
            if (arg0Lower == "remove")
            {
                if (!HasRemoveFriend(player.Id, target.Id))
                {
                    SendReply(player, "You do not have this player authorized.");
                    return;
                }
                RemoveRemoveFriend(player.Id, target.Id);
                SendReply(player, "<color=#ff912b>" + target.Name + "</color> <color=red>can no longer</color> remove your objects.");
            }
            if (arg0Lower == "add")
            {
                if (HasRemoveFriend(player.Id, target.Id))
                {
                    SendReply(player, "You have already authorized this player.");
                    return;
                }
                if (target.Id == player.Id)
                {
                    SendReply(player, "You cannot authorize yourself!");
                    return;
                }
                if ((friends?.Count ?? 0) > 4)
                {
                    SendReply(player, "You cannot add any more players!");
                    return;
                }
                AddRemoveFriend(player.Id, target.Id);
                SendReply(player, "<color=#ff912b>" + target.Name + "</color> <color=green>can now</color> remove your objects.");
            }
        }
        #endregion

        #region Class
        private class RemoveData
        {
            public Dictionary<string, List<string>> removePlayers = new();

            public List<string> GetFriends(string userId)
            {
                if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
                List<string> outList;
                if (!removePlayers.TryGetValue(userId, out outList)) return null;
                else return outList;
            }

            public bool HasFriend(string userId, string friendId)
            {
                if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
                if (string.IsNullOrEmpty(friendId)) throw new ArgumentNullException(nameof(friendId));
                List<string> outList;
                if (!removePlayers.TryGetValue(userId, out outList)) return false;
                return outList?.Contains(friendId) ?? false;
            }

            public void AddFriend(string userId, string friendId)
            {
                if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
                if (string.IsNullOrEmpty(friendId)) throw new ArgumentNullException(nameof(friendId));
                if (!HasFriend(userId, friendId))
                {
                    List<string> outList;
                    if (!removePlayers.TryGetValue(userId, out outList)) removePlayers[userId] = new List<string>();
                    removePlayers[userId].Add(friendId);
                }
            }

            public void RemoveFriend(string userId, string friendId)
            {
                if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
                if (string.IsNullOrEmpty(friendId)) throw new ArgumentNullException(nameof(friendId));
                if (HasFriend(userId, friendId)) removePlayers[userId].Remove(friendId);
            }

            public RemoveData() { }
        }

        private bool HasRemoveFriend(string userId, string friendId) { return _removeData?.HasFriend(userId, friendId) ?? false; }

        private void AddRemoveFriend(string userId, string friendId) => _removeData?.AddFriend(userId, friendId);

        private void RemoveRemoveFriend(string userId, string friendId) => _removeData?.RemoveFriend(userId, friendId);
        #endregion
        #region Util
        private void SaveRemoveData() => Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _removeData);

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

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var p = BasePlayer.sleepingPlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty; //BasePlayer.activePlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? BasePlayer.sleepingPlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? string.Empty;
        }

        private string CleanPlayerName(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));

            var strSB = Pool.Get<StringBuilder>();
            try
            {
                strSB.Clear();

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

                return strSB.ToString();
            }
            finally { Pool.Free(ref strSB); }
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


        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false, bool checkDatabase = true)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            BasePlayer player = null;

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null) continue;

                var pName = p?.displayName ?? string.Empty;

                var cleanName = CleanPlayerName(pName);
                if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;


                if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                {

                    if (player != null)
                        return null;


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

                    PrintWarning("cleanName (s): " + cleanName);


                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        PrintWarning("(s) string equals: " + pName + " == " + name + " (ignore case)");

                        if (player != null)
                        {
                            PrintWarning("player was already not null");
                            return null;
                        }

                        player = p;
                        return player;
                    }
                }
            }

            var userIds = Pool.Get<HashSet<ulong>>();

            try
            {
                if (checkDatabase)
                {
                    PlayersByDatabase?.Call("GetAllPlayerIDsNoAllocUH", userIds);
                    foreach (var userId in userIds)
                    {
                        var p = RelationshipManager.FindByID(userId);
                        if (p == null)
                            continue;

                        var pName = p?.displayName ?? string.Empty;

                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName))
                            pName = cleanName;

                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }

                    }
                }

                var matches = Pool.GetList<BasePlayer>();
                try
                {




                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var p = BasePlayer.activePlayerList[i];
                        if (p == null) continue;

                        var pName = p?.displayName ?? string.Empty;

                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;

                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                            matches.Add(p);

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
                                matches.Add(p);

                        }
                    }

                    if (checkDatabase)
                    {
                        foreach (var userId in userIds)
                        {
                            var p = RelationshipManager.FindByID(userId);
                            if (p == null)
                                continue;


                            var pName = p?.displayName ?? string.Empty;
                            var cleanName = CleanPlayerName(pName);

                            if (!string.IsNullOrEmpty(cleanName))
                                pName = cleanName;

                            if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                                matches.Add(p);

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

                    return player;
                }
                finally { Pool.FreeList(ref matches); }

            }
            finally { Pool.Free(ref userIds); }


        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));

            var ply = covalence.Players.FindPlayerById(nameOrIdOrIp);
            if (ply != null) if ((!ply.IsConnected && tryFindOfflineIfNoOnline) || ply.IsConnected) return ply;

            IPlayer player = null;
            foreach (var p in covalence.Players.Connected)
            {

                if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Address == nameOrIdOrIp || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (player != null) return null;
                    else player = p;
                }
            }

            if (tryFindOfflineIfNoOnline && player == null)
            {
                foreach (var p in covalence.Players.All)
                {
                    if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (player != null) return null;
                        else player = p;
                    }
                }
            }

            return player;
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
        #endregion
    }
}