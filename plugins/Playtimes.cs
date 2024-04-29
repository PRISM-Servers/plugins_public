using System;
using System.Collections.Generic;
using Oxide.Core;
using Newtonsoft.Json;
using System.ComponentModel;
using Oxide.Core.Libraries.Covalence;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

namespace Oxide.Plugins
{
    [Info("Playtimes", "Shady", "0.0.6", ResourceId = 0)]
    [Description("Keep track of players' total time on server")]
    internal class Playtimes : CovalencePlugin
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

        private readonly List<string> ForbiddenTags = new List<string> { "</color>",
                "</size>",
                "<b>",
                "</b>",
                "<i>",
                "</i>" };

        private class TimeStats
        {

            [JsonIgnore]
            public DateTime FirstConnection
            {
                get
                {
                    var t = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(_firstConnection))
                    {
                        if (!DateTime.TryParse(_firstConnection, out t)) Interface.Oxide.LogWarning("Failed to parse _firstConnection as DateTime?!");
                    }
                    return t;
                }
                set
                {
                    _firstConnection = value.ToString();
                }
            }

            [JsonIgnore]
            private DateTime _lastConnectionCache = DateTime.MinValue;

            [JsonIgnore]
            public DateTime LastConnection
            {
                get
                {
                    if (_lastConnectionCache != DateTime.MinValue)
                        return _lastConnectionCache;

                    var t = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(_lastConnection))
                    {
                        if (!DateTime.TryParse(_lastConnection, out t)) Interface.Oxide.LogWarning("Failed to parse _lastConnection as DateTime?!");
                    }

                    _lastConnectionCache = t;

                    return t;
                }
                set
                {
                    _lastConnection = value.ToString();
                    _lastConnectionCache = value;
                }
            }

            [JsonIgnore]
            public TimeSpan TimePlayed
            {
                get
                {
                    var t = TimeSpan.MinValue;
                    if (!string.IsNullOrEmpty(_timePlayed))
                    {
                        if (!TimeSpan.TryParse(_timePlayed, out t)) Interface.Oxide.LogWarning("Failed to parse _timePlayed as DateTime?!");
                    }
                    return t;
                }
                set
                {
                    if (value <= TimeSpan.Zero) _timePlayed = string.Empty;
                    else _timePlayed = value.ToString();
                }
            }


            [JsonProperty(PropertyName = "fc", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            private string _firstConnection = string.Empty;

            [JsonProperty(PropertyName = "lc", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            private string _lastConnection = string.Empty;

            [JsonProperty(PropertyName = "tp", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            private string _timePlayed = string.Empty;


            public override string ToString()
            {
                return FirstConnection + "/" + LastConnection + "/" + TimePlayed;
            }

            public TimeStats() { }
        }

       

        private Dictionary<string, TimeStats> TimeData;

    
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
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty;
        }

        private void cmdPlayTimeTopX(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var timeSB = new StringBuilder();
            var minsPlayed2 = new Dictionary<string, TimeSpan>();
            var takeAmt = 10;
            if (args.Length > 0 && !int.TryParse(args[0], out takeAmt)) takeAmt = 10;
            foreach(var kvp in TimeData)
            {
                minsPlayed2[kvp.Key] = kvp.Value.TimePlayed;
            }
            if (minsPlayed2.Count < 1)
            {
                SendReply(player, "No time stats found!");
                return;
            }
            var takeDic = minsPlayed2.OrderByDescending(p => p.Value).Take(Mathf.Clamp(takeAmt, 1, minsPlayed2.Count));
            var count = 0;
            foreach (var top in takeDic)
            {
                count++;
                var val = top.Value;
                timeSB.AppendLine(count.ToString("N0") + ": " + GetDisplayNameFromID(top.Key) + ": " + val + " (hours: " + val.TotalHours.ToString("0.00").Replace(".00", string.Empty) + ")");
            }
            SendReply(player, timeSB.ToString().TrimEnd());
        }

        private void cmdPlayTime(IPlayer player, string command, string[] args)
        {
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();
                if (player.IsAdmin)
                {
                    if (args.Length > 0)
                    {
                        if (args[0].Equals("set", StringComparison.OrdinalIgnoreCase))
                        {
                            if (args.Length < 3)
                            {
                                SendReply(player, "You must specify a name/ID and time (in seconds)!");
                                return;
                            }
                            var target = FindConnectedPlayer(args[1], true);
                            if (target == null)
                            {
                                SendReply(player, "No player found with name/ID: " + args[1]);
                                return;
                            }
                            double seconds;
                            if (!double.TryParse(args[2], out seconds))
                            {
                                SendReply(player, "Not a double: " + args[2]);
                                return;
                            }
                            SetTimePlayed(target.Id, TimeSpan.FromSeconds(seconds));
                            SendReply(player, "Set: " + target?.Name + " (" + target?.Id + ") playtime to: " + seconds + " seconds");
                            return;
                        }
                        if (args[0].Equals("transfer", StringComparison.OrdinalIgnoreCase))
                        {
                            if (args.Length < 3)
                            {
                                SendReply(player, "You must supply a name/ID to transfer from and to transfer to!");
                                return;
                            }
                            var source = FindConnectedPlayer(args[1], true);
                            if (source == null)
                            {
                                SendReply(player, "Could not find player with name/ID: " + args[1]);
                                return;
                            }
                            var target = FindConnectedPlayer(args[2], true);
                            if (target == null)
                            {
                                SendReply(player, "Could not find player with name/ID: " + args[2]);
                                return;
                            }
                            var sourceTime = GetTimeStats(source.Id)?.TimePlayed ?? TimeSpan.Zero;
                            if (sourceTime <= TimeSpan.Zero)
                            {
                                SendReply(player, "Source has no time played!");
                                return;
                            }
                            SetTimePlayed(target.Id, sourceTime);
                            SendReply(player, target.Name + " (" + target.Id + ") now has " + sourceTime.TotalMinutes.ToString("0.00").Replace(".00", string.Empty) + " minutes played.");
                            return;
                        }
                    }
                }
                var validStr = "For valid arguments, try <color=#4dff55>/" + command + "</color>, <color=#4dff55>/" + command + " <color=orange>top</color></color> or <color=#4dff55>/" + command + " <color=orange>old</color></color>.";
                if (args.Length > 0)
                {
                    if (args[0].Equals("old", StringComparison.OrdinalIgnoreCase))
                    {
                        var sort = TimeData.OrderBy(p => p.Value.FirstConnection);

                        var sb = new StringBuilder();
                        var now = DateTime.UtcNow;
                        var count = 0;
                        int take;
                        if (args.Length < 2 || !int.TryParse(args[1], out take))
                        {
                            SendReply(player, "You can specify how many players to list by adding the number, like this: <color=orange>/" + command + " <color=#e7ff4d>old</color> <color=#4dff55>25</color></color>. This will show 25 players.");
                            take = 5;
                        }
                        var isServer = player.IsServer;
                        if (take > 50 && !isServer)
                        {
                            take = 50;
                            SendReply(player, "Maximum players shown is limited to 50");
                        }
                        var listSB = Facepunch.Pool.GetList<StringBuilder>();
                        try
                        {
                            foreach (var kvp in sort.Take(take))
                            {
                                var val = kvp.Value;
                                if (val.FirstConnection <= DateTime.MinValue) continue;
                                count++;
                                var useYellow = !IsDivisble(count, 2) ? "#fff645" : "#aec732";
                                var txt = "<color=#9dff4d>#</color><color=#46c8f0>" + count.ToString("N0") + "</color>: <color=" + useYellow + ">" + GetDisplayNameFromID(kvp.Key) + " <color=#46c8f0>joined: </color>" + val.FirstConnection + " <color=#46c8f0>(<color=" + useYellow + ">" + ReadableTimeSpan(now - val.FirstConnection) + "</color>)</color></color>";
                                if (!isServer && (sb.Length + txt.Length) >= 768)
                                {
                                    listSB.Add(sb);
                                    sb = new StringBuilder();
                                }
                                sb.AppendLine(txt);
                            }
                            if (listSB.Count > 0) for (int i = 0; i < listSB.Count; i++) SendReply(player, listSB[i].ToString().TrimEnd());
                            if (sb.Length > 0) SendReply(player, sb.ToString().TrimEnd());
                        }
                        finally { Facepunch.Pool.FreeList(ref listSB); }

                        return;
                    }
                    if (args[0].Equals("top", StringComparison.OrdinalIgnoreCase))
                    {
                        var sort = TimeData.OrderByDescending(p => p.Value.TimePlayed);

                        var sb = new StringBuilder();
                        var now = DateTime.UtcNow;
                        var count = 0;
                        int take;
                        if (args.Length < 2 || !int.TryParse(args[1], out take))
                        {
                            SendReply(player, "You can specify how many players to list by adding the number, like this: <color=orange>/" + command + " <color=#e7ff4d>top</color> <color=#4dff55>25</color></color>. This will show 25 players.");
                            take = 5;
                        }
                        if (take > 50)
                        {
                            take = 50;
                            SendReply(player, "Maximum players shown is limited to 50");
                        }
                        var listSB = Facepunch.Pool.GetList<StringBuilder>();

                        try
                        {
                            foreach (var kvp in sort.Take(take))
                            {
                                var val = kvp.Value;
                                if (val.TimePlayed <= TimeSpan.Zero) continue;
                                count++;
                                var useYellow = !IsDivisble(count, 2) ? "#fff645" : "#aec732";

                                var txt = "<color=#9dff4d>#</color><color=#46c8f0>" + count.ToString("N0") + "</color>: <color=" + useYellow + ">" + GetDisplayNameFromID(kvp.Key) + " <color=#46c8f0>joined: </color>" + val.FirstConnection + " <color=#46c8f0>(<color=" + useYellow + ">" + ReadableTimeSpan(now - val.FirstConnection) + "</color>)</color>\n<color=#46c8f0>Hours Played: </color>" + val.TimePlayed.TotalHours.ToString("0.00").Replace(".00", string.Empty) + " <color=#46c8f0>(</color>" + ReadableTimeSpan(val.TimePlayed) + "<color=#46c8f0>)</color>" + Environment.NewLine;
                                if ((sb.Length + txt.Length) >= 768)
                                {
                                    listSB.Add(sb);
                                    sb = new StringBuilder();
                                }
                                sb.AppendLine(txt);
                            }
                            if (listSB.Count > 0) for (int i = 0; i < listSB.Count; i++) SendReply(player, listSB[i].ToString().TrimEnd());
                            if (sb.Length > 0) SendReply(player, sb.ToString().TrimEnd());
                        }
                        finally { Facepunch.Pool.FreeList(ref listSB); }

                        return;
                    }
                    SendReply(player, validStr);
                    return;
                }
               
                if (player.IsServer)
                {
                    SendReply(player, "This can only be ran as a player!");
                    return;
                }

                var bpObj = player?.Object as BasePlayer;
                if (bpObj != null) UpdateTotalPlaytime(bpObj);

                var stats = GetTimeStats(player.Id);
                if (stats == null)
                {
                    SendReply(player, "You have no stats found!");
                    return;
                }
                var totalHours = 0D;
                var timeSB = new StringBuilder();
                foreach (var kvp in TimeData)
                {
                    var data = kvp.Value;
                    if (data.TimePlayed.TotalHours < 0)
                    {
                        timeSB.AppendLine("!! FOUND NEGATIVE HOURS: " + kvp.Key + " timespan: " + data.TimePlayed + ", totalhours: " + data.TimePlayed.TotalHours);
                        continue;
                    }
                    totalHours += data.TimePlayed.TotalHours;
                }

                if (timeSB.Length > 0) PrintWarning(timeSB.ToString().TrimEnd());
                SendReply(player, "<color=#ff69dc>You have <color=#69f0ff>" + (stats.TimePlayed <= TimeSpan.Zero ? "0" : stats.TimePlayed.TotalHours.ToString("0.00").Replace(".00", string.Empty)) + "</color> hours played on this server</color>.\n<color=#69f0ff>You first joined: <color=#ff69dc>" + stats.FirstConnection + " (" + (DateTime.UtcNow - stats.FirstConnection.ToUniversalTime()).TotalDays.ToString("N0") + " days ago)</color></color>\n\n<color=#8bff42>Total hours played by all players on this server combined: <color=#ea4dff>" + totalHours.ToString("N0") + "</color> hours, that's <color=#ea4dff>" + (totalHours / 8760).ToString("0.00").Replace(".00", string.Empty) + "</color> years</color>!");
                SendReply(player, validStr);
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                SendReply(player, "Something went seriously wrong! Report this issue to an administrator as soon as possible.");
            }
            finally 
            {
                PrintWarning("/" + command + " took: " + watch.ElapsedMilliseconds.ToString("0.##") + "ms");
                Facepunch.Pool.Free(ref watch); 
            }
        }

        private void SendReply(IPlayer player, string msg, string userId = ChatSteamID, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : keepTagsConsole ? msg : RemoveTags(msg);
#if RUST
              if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
              else player.Command("chat.add", string.Empty, userId, msg);
#else
            player.Reply(msg);
#endif
        }

        private TimeStats GetTimeStats(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            TimeStats stats;
            if (TimeData.TryGetValue(userId, out stats)) return stats;
            return null;
        }

        private void SetTimePlayed(string userId, TimeSpan span)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            var stat = GetTimeStats(userId);
            if (stat == null)
            {
                stat = new TimeStats();
                TimeData[userId] = stat;
            }
            stat.TimePlayed = span;
        }

        public bool IsDivisble(int x, int n) { return (x % n) == 0; }


        private string ReadableTimeSpan(TimeSpan span, string stringFormat = "N0") //I'm sure some of you uMod code snobs absolutely LOVE this one
        {
            if (span == TimeSpan.MinValue) return string.Empty;
            var str = string.Empty;
            var repStr = stringFormat.StartsWith("0.0", StringComparison.CurrentCultureIgnoreCase) ? ("." + stringFormat.Replace("0.", string.Empty)) : "WORKAROUNDGARBAGETEXTTHATCANNEVERBEFOUNDINASTRINGTHISISTOPREVENTREPLACINGANEMPTYSTRINGFOROLDVALUEANDCAUSINGANEXCEPTION"; //this removes unnecessary values, for example for ToString("0.00"), 80.00 will show as 80 instead

            if (span.TotalHours >= 24)
            {

                var totalHoursWereGoingToShowToTheUserAsAString = (span.TotalHours - ((int)span.TotalDays * 24)).ToString(stringFormat).Replace(repStr, string.Empty);
                var totalHoursToShowAsNumber = (int)Math.Round(double.Parse(totalHoursWereGoingToShowToTheUserAsAString), MidpointRounding.AwayFromZero);
                var showHours = totalHoursToShowAsNumber < 24 && totalHoursToShowAsNumber > 0;

                str = (int)span.TotalDays + (!showHours && span.TotalHours > 24 ? 1 : 0) + " day" + (span.TotalDays >= 1.5 ? "s" : "") + (showHours ? (" " + totalHoursWereGoingToShowToTheUserAsAString + " hour(s)") : "");
            }
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));
            try
            {
                var p = covalence.Players.FindPlayerById(nameOrIdOrIp);
                if (p != null) if ((!p.IsConnected && tryFindOfflineIfNoOnline) || p.IsConnected) return p;
                var connected = covalence.Players.Connected;
                List<IPlayer> players = new List<IPlayer>();
                foreach (var player in connected)
                {
                    if (players.Count > 1) break;
                    if (player.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(player);
                    else if (player.Address == nameOrIdOrIp) players.Add(player);
                    else if (player.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0) players.Add(player);
                }
                if (players.Count < 1 && tryFindOfflineIfNoOnline)
                {
                    foreach (var player in covalence.Players.All)
                    {
                        if (player.IsConnected) continue;
                        if (players.Count > 1) break;
                        if (player.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(player);
                        else if (player.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0) players.Add(player);
                    }
                }
                return players.Count == 1 ? players[0] : null;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString() + " ^ FindConnectedPlayer ^ ");
                return null;
            }
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;

            //	Replace Color Tags
            phrase = new Regex("(<color=.+?>)").Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = new Regex("(<size=.+?>)").Replace(phrase, string.Empty);
            var phraseSB = new StringBuilder(phrase);
            for (int i = 0; i < ForbiddenTags.Count; i++) phraseSB.Replace(ForbiddenTags[i], string.Empty);


            return phraseSB.ToString();
        }

        private void Init()
        {
            string[] playTimeCmds = { "hours", "time", "playedtime", "playtime", "timeplayed", "playtimes", "times" };
            AddCovalenceCommand(playTimeCmds, "cmdPlayTime");
            AddCovalenceCommand("playtime.topx", "cmdPlayTimeTopX");
            TimeData = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<string, TimeStats>>("PlayerTimeStats") ?? new Dictionary<string, TimeStats>();
            if (TimeData?.Count > 0)
            {
                var now = DateTime.UtcNow;
                foreach(var kvp in TimeData)
                {
                    var val = kvp.Value;
                    if (val.FirstConnection <= DateTime.MinValue) val.FirstConnection = now;
                    if (val.LastConnection <= DateTime.MinValue) val.LastConnection = now;
                }
            }
        }

       

        private Action SaveAction = null;
        private void OnServerInitialized()
        {
            if (ServerMgr.Instance == null)
            {
                PrintWarning("No ServerMgr.Instance after init!!");
                return;
            }
            SaveAction = new Action(() =>
            {
                var watch = Facepunch.Pool.Get<Stopwatch>();
                try
                {
                    watch.Restart();
                    SaveData();
                    PrintWarning("SaveData for Playtimes took: " + watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms");
                }
                finally
                {
                    Facepunch.Pool.Free(ref watch);
                }
            });
            ServerMgr.Instance.InvokeRepeating(SaveAction, 1800f, 3600f);

            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p != null) UpdateTotalPlaytime(p);
            }
            
        }
    
        private readonly Dictionary<ulong, TimeSpan> lastConnectionSessionUpdateTime = new Dictionary<ulong, TimeSpan>();

        private TimeSpan GetConnectedTimeSpan(BasePlayer player)
        {
            var now = DateTime.UtcNow;
            var val = (player == null || !player.IsConnected) ? TimeSpan.Zero : now - now.AddSeconds(-Mathf.Clamp(player.net.connection.GetSecondsConnected(), 0, int.MaxValue));
            if (val <= TimeSpan.Zero) PrintWarning("getconnectedtimespan has .zero or less: " + val);
            return val;
        }

        private DateTime GetLastConnection(string userId)
        {
            return GetTimeStats(userId)?.LastConnection ?? DateTime.MinValue;
        }

        private DateTime GetFirstConnection(string userId)
        {
            return GetTimeStats(userId)?.FirstConnection ?? DateTime.MinValue;
        }

        private TimeSpan GetTimePlayed(string userId)
        {
            return GetTimeStats(userId)?.TimePlayed ?? TimeSpan.MinValue;
        }

        private void UpdateTotalPlaytime(BasePlayer player, bool isDisconnecting = false)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();
                var nowUtc = DateTime.UtcNow;

                var stats = GetTimeStats(player.UserIDString);
                if (stats == null)
                {
                    stats = new TimeStats();
                    TimeData[player.UserIDString] = stats;
                }

                if (stats.FirstConnection <= DateTime.MinValue) stats.FirstConnection = nowUtc;

                var connectedTime = GetConnectedTimeSpan(player);
                if (connectedTime > TimeSpan.Zero)
                {
                    var originalConnected = connectedTime;
                    if (!isDisconnecting)
                    {
                        TimeSpan outSpan;
                        if (lastConnectionSessionUpdateTime.TryGetValue(player.userID, out outSpan))
                        {
                            PrintWarning("got last update, outSpan: " + outSpan + ", connectedTime before: " + connectedTime);
                            connectedTime = connectedTime.Subtract(outSpan);
                            PrintWarning("ConnectedTime after subtract: " + connectedTime);
                        }
                    }

                    if (stats.TimePlayed <= TimeSpan.Zero) stats.TimePlayed = connectedTime;
                    else stats.TimePlayed = stats.TimePlayed.Add(connectedTime);


                    if (!isDisconnecting) lastConnectionSessionUpdateTime[player.userID] = originalConnected;
                }
                else PrintWarning("connectedTime is TimeSpan.MinValue or zero!? p: " + player?.displayName + "/" + player?.UserIDString + ", " + (player?.net?.connection?.GetSecondsConnected() ?? -1) + " <-- connected seconds");

                stats.LastConnection = nowUtc;
                PrintWarning("lastConnection set to: " + stats.LastConnection);
                watch.Stop();
                if (watch.ElapsedMilliseconds > 3) PrintWarning("UpdateTotalPlaytime took: " + watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms for: " + player?.displayName + " (" + isDisconnecting + ")");
            }
            finally { Facepunch.Pool.Free(ref watch); }

           
        }

        private void OnUserConnected(IPlayer player)
        {
            PrintWarning("OnUserConnected with IPlayer: " + player);
            var stats = GetTimeStats(player.Id);
            if (stats == null)
            {
                stats = new TimeStats();
                TimeData[player.Id] = stats;
            }
            var now = DateTime.UtcNow;
            if (stats.FirstConnection <= DateTime.MinValue) stats.FirstConnection = now;
            stats.LastConnection = now;

            PrintWarning("set last connection to: " + stats.LastConnection);

        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null) return;
            PrintWarning("calling updatetotalplaytime on disconnect! " + player);
            UpdateTotalPlaytime(player, true);
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("PlayerTimeStats", TimeData);
        
        private void Unload()
        {
            if (ServerMgr.Instance != null && SaveAction != null) ServerMgr.Instance.CancelInvoke(SaveAction);
            SaveData();
        }
    }
}
