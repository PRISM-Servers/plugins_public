using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("VoteDay", "Shady", "0.0.5")]
    internal class VoteDay : RustPlugin
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
        private const float VOTE_DURATION = 75f;
        private const string CHAT_STEAM_ID = "76561198865536053";

        private int _consecutiveDaysVoted = 0;

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        //private Action _nightAction = null;
        private string _nightCalledDay = string.Empty;

        private EnvSync _envSync = null;
        public EnvSync EnvSync
        {
            get
            {
                if (_envSync == null)
                {
                    foreach (var entity in BaseNetworkable.serverEntities)
                    {
                        var sync = entity as EnvSync;
                        if (sync != null)
                        {
                            _envSync = sync;
                            break;
                        }
                    }
                }
                return _envSync;
            }
        }

        public VoteInfo DayVote { get; set; }

        public bool DayVoteLoss { get; set; } = false;


        public class VoteInfo
        {
            public int NoVotes { get; set; } = 0;
            public int YesVotes { get; set; } = 0;
            public int VoteCount { get { return YesVotes + NoVotes; } }
            public int NeededVotes
            {
                get
                {
                    if (BasePlayer.activePlayerList == null || BasePlayer.activePlayerList.Count < 1) return 0;
                    var plyCount = 0;
                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var p = BasePlayer.activePlayerList[i];
                        if (p != null && !p.IsDestroyed && p.gameObject != null && p.IsConnected && p.IsAlive() && !p.IsSleeping() && p.IdleTime < 120) plyCount++;
                    }
                    return plyCount == 0 ? 1 : Mathf.Clamp((int)Math.Round(plyCount * 0.175, MidpointRounding.AwayFromZero), 1, 500);
                }
            }

            public HashSet<string> Voters { get; private set; } = new HashSet<string>();

            public bool HasVoted(string userID) { return Voters.Contains(userID); }
            public void AddVote(string userID, bool yesVote)
            {
                if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));

                if (Voters.Add(userID))
                {
                    if (yesVote) YesVotes++;
                    else NoVotes++;
                }
            }

            public float VotePercentage
            {
                get
                {
                    if (NoVotes < 1 && YesVotes < 1) return -1;
                    if (NoVotes < 1 && YesVotes > 0) return 100;
                    if (NoVotes > 0 && YesVotes < 1) return 0;
                    return YesVotes / (float)VoteCount * 100;
                }
            }


            public VoteInfo() { }
        }

       

        private void OnStartNight()
        {
            if (BasePlayer.activePlayerList == null || BasePlayer.activePlayerList.Count < 1) return; //no users connected

            if (_consecutiveDaysVoted >= 2)
            {
                try
                {
                    var sb = Facepunch.Pool.Get<StringBuilder>();

                    try
                    {
                        SimpleBroadcast(sb.Clear().Append("<size=18><color=#FFCD80>A vote to skip <color=#870da5><i>night</i></color> has not been started, because there were ").Append(_consecutiveDaysVoted.ToString("N0")).Append(" consecutive nights that players skipped, so we're going to have night time now. Enjoy!</color> <color=#870da5>:)</color>").ToString(), CHAT_STEAM_ID);
                    }
                    finally { Facepunch.Pool.Free(ref sb); }

                    ShowPopupAll("<color=#870da5>NIGHT TIME IS UPON US</color> :)");
                }
                finally { _consecutiveDaysVoted = 0; }
                

                return;
            }

            DayVote = new VoteInfo();

            var needVotes = DayVote.NeededVotes;
            if (needVotes < 1)
            {
                PrintWarning("got needVotes of < 1!!!");
                return;
            }

            SimpleBroadcast("<size=18><color=#FFCD80>A vote to skip to <color=#FFDB00>morning</color> has started!</color></size> <color=#FFCD80>You can vote <color=#8aff47>yes</color> or <color=#F15858>no</color> by typing <color=#A9F1F6><size=17>/vday</size></color>\nThis vote will last <color=#8aff47>" + VOTE_DURATION.ToString("N0") + "</color> seconds.</color>\nAt least <color=#FFCE64><size=20>" + needVotes.ToString("N0") + "</size></color> players must vote for the votes to count.", CHAT_STEAM_ID);
            ShowPopupAll("<color=#8aff47>VOTE DAY HAS STARTED!</color>");
            ServerMgr.Instance.Invoke(() =>
            {
                try
                {
                    if (DayVote == null)
                    {
                        PrintWarning("DayVote is null on Invoke?!");
                        return;
                    }

                    var needPerc = 67;
                    var votePerc = DayVote.VotePercentage;

                    if (DayVote.VoteCount >= needVotes)
                    {
                        if (votePerc >= needPerc)
                        {
                            _consecutiveDaysVoted++;

                            SimpleBroadcast("<color=#FFDB00>Day</color><color=#A9F1F6> vote <size=20>wins</size> with <color=#FFA9F1>" + votePerc.ToString("0.0").Replace(".0", "") + "% voting yes!</color></color>", CHAT_STEAM_ID);
                            StartNewMorning();
                        }
                        else
                        {

                            var startTxt = "<color=red>Day vote <size=20>loses</size> with ";
                            var endTxt = votePerc <= 0 ? "<color=orange>nobody</color> voting yes!" : "only <color=#8aff47>" + votePerc.ToString("0.0").Replace(".0", "") + "% voting yes!</color></color>";
                            SimpleBroadcast(startTxt + endTxt, CHAT_STEAM_ID);
                            DayVoteLoss = true;
                        }
                    }
                    else
                    {
                        if (DayVote.NoVotes < 1 && DayVote.YesVotes < 1) SimpleBroadcast("<size=17><color=#f48c42>Nobody voted for day!</color></size>", CHAT_STEAM_ID);
                        else SimpleBroadcast("<size=17><color=#f48c42>Not enough people voted for day!</color> <color=#f4cd41>Only <color=#f46441>" + DayVote.VoteCount.ToString("N0") + "</color> voted, at least <color=#8aff47>" + needVotes.ToString("N0") + "</color> were needed.</color></size>", CHAT_STEAM_ID);
                        DayVoteLoss = true;
                    }
                }
                finally 
                {
                    DayVote = null;
                    if (DayVoteLoss) _consecutiveDaysVoted = 0;
                }
            
            }, VOTE_DURATION);
        }

       

        private void StartNewMorning()
        {
            try 
            {
                var time = TOD_Sky.Instance.Cycle.DateTime;
                var addTime0 = time.AddDays(1);
                var addTime = new DateTime(addTime0.Year, addTime0.Month, addTime0.Day, 7, 50, 0, DateTimeKind.Utc);

                TOD_Sky.Instance.Cycle.DateTime = addTime;

                EnvSync?.Invoke(() => EnvSync.SendNetworkUpdate(), 0.1f);
            }
            finally
            {
                DayVoteLoss = false;
                _nightCalledDay = string.Empty;
            }
        }
 
        private void SimpleBroadcast(string msg, string userId = "")
        {
            
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || !player.IsConnected) continue;
                if (!string.IsNullOrEmpty(userId)) player.SendConsoleCommand("chat.add", string.Empty, userId, msg);
                else SendReply(player, msg);
            }
        }

        private bool IsNight() { return TOD_Sky.Instance.Cycle.Hour > TOD_Sky.Instance.SunsetTime || TOD_Sky.Instance.Cycle.Hour < TOD_Sky.Instance.SunriseTime; }
        private bool IsNight(float offset) { return TOD_Sky.Instance.Cycle.Hour > (TOD_Sky.Instance.SunsetTime + offset) || TOD_Sky.Instance.Cycle.Hour < TOD_Sky.Instance.SunriseTime; }
        private bool IsAM() { return TOD_Sky.Instance.Cycle.Hour <= TOD_Sky.Instance.SunriseTime; }
        private bool HasNightStartedForToday()
        {
            return _nightCalledDay.Equals(TOD_Sky.Instance.Cycle.DateTime.ToString("d"), StringComparison.OrdinalIgnoreCase);
        }

        private void Init()
        {
            string[] voteDayCmds = { "vday", "voteday", "dayvote", "skipnight", "votenight", "skipday", "daytime", "votesun", "bright", "nonight", "night", "day" };
            AddCovalenceCommand(voteDayCmds, nameof(cmdVDay));
        }

       /*/
        private void OnServerInitialized()
        {

            _nightAction = new Action(() =>
            {
                var cycleInstance = TOD_Sky.Instance.Cycle;
                if (IsNight(-0.6f) && !IsAM() && !(cycleInstance.Hour > 0 && cycleInstance.Hour < 6) && !HasNightStartedForToday())
                {
                    _nightCalledDay = cycleInstance.DateTime.ToString("d");
                    Interface.Oxide.CallHook("OnStartNight");
                }
            });
            InvokeHandler.InvokeRepeating(ServerMgr.Instance, _nightAction, 8f, 8f);
        }

        private void Unload()
        {
            if (ServerMgr.Instance != null) InvokeHandler.CancelInvoke(ServerMgr.Instance, _nightAction);
        }/*/

        private void cmdVDay(IPlayer player, string command, string[] args)
        {

            if (player.IsAdmin && args.Length > 0 && args[0].Equals("force"))
            {
                StartNewMorning();
                return;
            }

            if (DayVote == null && DayVoteLoss)
            {
                SendReply(player, "Vote day has already ended! Please wait until the next night.");
                return;
            }
            if (DayVote == null)
            {
                SendReply(player, "No day vote active!");
                return;
            }

            if (args.Length > 0)
            {
                if (DayVote.HasVoted(player.Id))
                {
                    SendReply(player, "You've already voted!");
                    return;
                }
                if (args[0].Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    DayVote.AddVote(player.Id, true);
                    SendReply(player, "Voted <color=green>yes</color>");
                }
                else if (args[0].Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    DayVote.AddVote(player.Id, false);
                    SendReply(player, "Voted <color=red>no</color>");
                }
                else
                {
                    SendReply(player, "Bad argument: " + args[0] + " (must be <color=green>yes</color> or <color=red>no</color>)");
                    return;
                }
            }
            else
            {
                SendReply(player, "You can vote yes or no by typing: <color=green>/vday yes</color> or <color=red>/vday no</color>");
                ShowPopup(player.Id, "You must supply a ''Yes'' or ''No'' vote: <color=green>/vday yes</color> or <color=red>/vday no</color>");
            }
            SendReply(player, "Total vote percentage: " + DayVote.VotePercentage.ToString("0.0").Replace(".0", string.Empty) + "%\nTotal vote count: " + DayVote.VoteCount.ToString("N0") + " (needs " + DayVote.NeededVotes.ToString("N0") + " total votes)");
        }

        private readonly Dictionary<string, Timer> popupTimer = new Dictionary<string, Timer>();

        private void ShowPopup(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException(nameof(Id));
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(msg));
            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;
            player.Command("gametip.showgametip", msg);
            Timer endTimer;
            if (popupTimer.TryGetValue(Id, out endTimer)) endTimer.Destroy();
            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });
            popupTimer[Id] = endTimer;
        }

        private void ShowPopupAll(string msg, float duration = 5f)
        {
            foreach (var ply in covalence.Players.Connected) ShowPopup(ply.Id, msg, duration);
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

        private void SendReply(BasePlayer player, string msg, string userId = CHAT_STEAM_ID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
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