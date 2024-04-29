using JSON;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Wipe Information", "Shady", "1.0.2")]
    internal class WipeInfo : RustPlugin
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
        #region Plugin References
        [PluginReference]
        private readonly Plugin DiscordAPI2;
        #endregion

        private const string DATA_FILE_NAME = "WipeDate";
        private const string DATA_FILE_NAME_SCHEDULE = "WipeSchedule";
        private const string CHAT_STEAM_ID = "76561198865536053";

        private readonly Dictionary<string, Timer> _popupTimer = new Dictionary<string, Timer>();

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private bool _resetWipeDate = false;

        private WipeData _wipeData;

        public int WipeEveryDays { get; set; }

        public DateTime LocalSaveTime { get { return SaveRestore.SaveCreatedTime.ToLocalTime(); } }

        #endregion
        #region Hooks

        private void OnNewSave()
        {
            PrintWarning(nameof(OnNewSave));
            _resetWipeDate = true;
        }

        private void OnServerInitialized()
        {
            PrintWarning(nameof(OnServerInitialized));

            if (_resetWipeDate)
            {
                PrintWarning(nameof(_resetWipeDate));
                _wipeData = new WipeData();
            }

            PrintWarning("setting unix timestamp to: " + GetWipeDate() + " but universal: " + GetWipeDate().ToUniversalTime());
            WipeTimer.wipeUnixTimestampOverride = GetUnixTimeStampFromDateTime(GetWipeDate().ToUniversalTime());
        }

        private void Init()
        {
            _wipeData = _resetWipeDate ? new WipeData() : Interface.Oxide?.DataFileSystem?.ReadObject<WipeData>(DATA_FILE_NAME) ?? new WipeData();

            WipeEveryDays = Interface.Oxide?.DataFileSystem?.ReadObject<int>(DATA_FILE_NAME_SCHEDULE) ?? 14;

            string[] wipeCommands = { "wipe", "reset", "nextwipe", "wipedate", "wip", "whenwip", "wipwhen" };
            AddCovalenceCommand(wipeCommands, nameof(cmdNextWipe));

            AddCovalenceCommand("setwipe", nameof(cmdSetWipe));
            AddCovalenceCommand("wipeschedule", nameof(cmdSetWipeSchedule));
        }


        private void Unload() => SaveWipeData();

        #endregion
        #region Commands
        private void cmdNextWipe(IPlayer player, string command, string[] args)
        {
            var wipeDate = GetWipeDate();
            var span = ReadableTimeSpan(wipeDate - DateTime.Now);

            var readableSpan2 = GetTimeSpanFromMs((wipeDate - DateTime.Now).TotalMilliseconds);

            PrintWarning("readableSpan2: " + readableSpan2);

            var nextMsg = "Next wipe: <color=#E6BE8A>" + wipeDate.ToString("d") + "</color>";
            if (DateTime.Now < wipeDate) nextMsg += " (<color=#DA9100>" + span + " from now</color>)";
            else nextMsg += " (Soon)";
            var bpLvlMsg = "<color=#3CFA8E>Our wipe schedule is every <i>" + WipeEveryDays + "</i> days.</color>\n<color=#3ec2d1>Blueprints and levels wipe monthly (first Thursday of each month).</color>";
            var wipeMsg = "Server wiped: <color=#C6930A>" + LocalSaveTime.ToString("d") + "</color> (<color=#FFE800>" + ReadableTimeSpan(DateTime.Now - LocalSaveTime) + " ago</color>)" + Environment.NewLine + nextMsg + Environment.NewLine + bpLvlMsg;
            if (wipeDate.DayOfWeek == DayOfWeek.Thursday && wipeDate.Day <= 7) wipeMsg += Environment.NewLine + "<color=#e68fac>Next wipe will be a forced wipe, so the date may not be 100% accurate. We'll wipe as soon as Facepunch releases the update.</color>\n<color=#f96193>This also means this wipe is a *FULL* wipe. That means a new map and <i>blueprints</i> will reset!</color>";

            SendReply(player, wipeMsg);
            ShowPopup(player.Id, "Wipe: <color=#e2c653>" + span + "</color>", 7f);
        }

        private void cmdSetWipe(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin || args.Length < 1) return;

            var doUpdate = false;

            try 
            {
                if (args[0].Equals("unset"))
                {
                    _wipeData.ClearForcedDate();
                    SendReply(player, "Unset forced wipe date");

                    doUpdate = true;
                    return;
                }

                DateTime outTime;
                if (!DateTime.TryParse(args[0], out outTime))
                {
                    SendReply(player, "Not a date time: " + args[0]);
                    return;
                }

                SendReply(player, "Set wipe date to: " + (_wipeData.ForcedWipeDate = outTime));
                doUpdate = true;
            }
            finally
            {
                try
                {
                    if (doUpdate && DiscordAPI2 != null && DiscordAPI2.IsLoaded)
                        DiscordAPI2.Call("UpdateWipeInfo");
                }
                finally
                {
                    WipeTimer.wipeUnixTimestampOverride = GetUnixTimeStampFromDateTime(GetWipeDate().ToUniversalTime());
                }
            }
        }
        private void cmdSetWipeSchedule(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin || args.Length < 1) return;

            int days;
            if (!int.TryParse(args[0], out days))
            {
                SendReply(player, "Not an integer (specify days): " + args[0]);
                return;
            }


            SendReply(player, "Set schedule to: " + (WipeEveryDays = days));

            if (DiscordAPI2 != null && DiscordAPI2.IsLoaded) DiscordAPI2.Call("UpdateWipeInfo");
        }
        #endregion
        #region Class
        private class WipeData
        {
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [DefaultValue("")]
            public string _forcedWipeDate = string.Empty;

            [JsonIgnore]
            public DateTime ForcedWipeDate
            {
                get
                {
                    if (string.IsNullOrEmpty(_forcedWipeDate)) return DateTime.MinValue;

                    DateTime outTime;
                    if (!DateTime.TryParse(_forcedWipeDate, out outTime)) return DateTime.MinValue;
                    else return outTime;
                }
                set
                {
                    try
                    {
                        Interface.Oxide.LogWarning("setting timestamp override via setter. value: " + value + " to universal: " + value.ToUniversalTime());
                        WipeTimer.wipeUnixTimestampOverride = ((DateTimeOffset)value.ToUniversalTime()).ToUnixTimeSeconds();
                    }
                    finally
                    { 
                        _forcedWipeDate = value.ToString(); 
                    }
                }
            }

            public void ClearForcedDate()
            {
                _forcedWipeDate = string.Empty;
            }

            public WipeData() { }
            public WipeData(DateTime wipeDate)
            {
                ForcedWipeDate = wipeDate;
            }
        }
        #endregion
        #region Util
        private void SaveWipeData()
        {
            if (_wipeData != null)
                Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _wipeData);

            Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME_SCHEDULE, WipeEveryDays);
        }

        private DateTime GetWipeDate()
        {
            if (_wipeData?.ForcedWipeDate > DateTime.MinValue) return _wipeData.ForcedWipeDate;

            var saveTime = LocalSaveTime;

            var wipeSchedDays = WipeEveryDays;

            if (wipeSchedDays == 30)
            {
                var now = DateTime.Now;
                var firstOfNextMonth = new DateTime(now.Year + (now.Month >= 12 ?  1 : 0), now.AddMonths(1).Month, 1);

                var wipeDay = firstOfNextMonth;

                while (wipeDay.DayOfWeek != DayOfWeek.Thursday)
                    wipeDay = wipeDay.AddDays(1);

                return wipeDay;
            }

            var lastAdd = new DateTime(saveTime.Ticks, DateTimeKind.Local);
            for(int i = 0; i < wipeSchedDays; i++)
            {
                lastAdd = lastAdd.AddDays(1);
                if (lastAdd.DayOfWeek == DayOfWeek.Thursday && lastAdd.Day <= 7) return lastAdd;
            }

            return saveTime.AddDays(wipeSchedDays);
        }

        private TimeSpan TimeUntilWipe()
        {
            var span = GetWipeDate() - DateTime.Now;

            if (span <= TimeSpan.Zero)
                PrintWarning(nameof(TimeUntilWipe) + " is BAD!!!!!: " + span + ", wipe date is: " + GetWipeDate());

            return span;
            //return GetWipeDate() - DateTime.Now;
        }

        private void ShowPopup(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException(nameof(Id));
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));

            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;

            player.Command("gametip.showgametip", msg);

            Timer endTimer;
            if (_popupTimer.TryGetValue(Id, out endTimer)) endTimer.Destroy();

            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });

            _popupTimer[Id] = endTimer;
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

        public static string GetTimeSpanFromMs(double ms, bool isShort = false)
        {
            if (ms < 1000)
                return "Less than a second";

            ms /= 1000;
            int seconds = (int)(ms % 60);
            ms /= 60;
            int minutes = (int)(ms % 60);
            ms /= 60;
            int hours = (int)(ms % 24);
            int days = (int)(ms / 24);
            int years = days / 365;
            days -= years * 365;

            var sb = Pool.Get<StringBuilder>();

            try 
            {
                string year_s = sb.Clear().Append(years).Append(" year").Append(years != 1 ? "s" : "").ToString();
                string day_s = sb.Clear().Append(days).Append(" day").Append(days != 1 ? "s" : "").ToString();
                string hour_s = sb.Clear().Append(hours).Append(" hour").Append(hours != 1 ? "s" : "").ToString();
                string mins_s = sb.Clear().Append(minutes).Append(" minute").Append(minutes != 1 ? "s" : "").ToString();
                string sec_s = sb.Clear().Append(seconds).Append(" second").Append(seconds != 1 ? "s" : "").ToString();


                var arr = Pool.GetList<string>();
                try
                {
                    if (years > 0)
                        arr.Add(year_s);
                    if (days > 0)
                        arr.Add(day_s);
                    if (hours > 0)
                        arr.Add(hour_s);
                    if (minutes > 0)
                        arr.Add(mins_s);
                    if (seconds > 0 && (!isShort || arr.Count < 1))
                        arr.Add(sec_s);

                    if (arr.Count < 1)
                        return "Unknown";
                    if (arr.Count < 2)
                        return arr[0];

                    string last = arr[arr.Count - 1];
                    arr.RemoveAt(arr.Count - 1);

                    sb.Clear();

                    for (int i = 0; i < arr.Count; i++)
                    {
                        var ar = arr[i];

                        sb.Append(ar).Append(", ");

                    }

                    if (sb.Length > 2) //trim last 2 to trim ", "
                        sb.Length -= 2;

                    sb.Append(" and ").Append(last);

                    return sb.ToString();
                }
                finally { Pool.FreeList(ref arr); }

              
            }
            finally { Pool.Free(ref sb); ; }

          
        }

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

                str = (int)span.TotalDays + (!showHours && span.TotalHours >= 48 ? 1 : 0) + " day" + (span.TotalDays >= 1.5 ? "s" : "") + (showHours ? (" " + totalHoursWereGoingToShowToTheUserAsAString + " hour(s)") : "");
            }
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        private long GetUnixTimeStampFromDateTime(DateTime time)
        {
           return (long)(time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        #endregion
    }
}