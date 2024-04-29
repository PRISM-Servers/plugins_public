using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using UnityEngine;
using CompanionServer;
using System.Text;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Vote Reminder", "Shady", "1.0.1")]
    class VoteReminder : RustPlugin
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
        //needs to check if has already sent reminder today lol

        //check last sent time for plugin reload compatilbiity

        private int _consecutiveFailedRequests = 0;

        private const string CHAT_STEAM_ID = "76561198865536053";


        private const string RUST_SERVERS_KEY_2 = "";
        private const string RUST_SERVERS_KEY_1 = "";

        [PluginReference]
        private readonly Plugin DiscordAPI2;

        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        private Coroutine _reminderCoroutine = null;


        private enum ServerType
        {
            None,
            x3,
            x10,
            BF
        }

        private ServerType serverType = ServerType.None;

        private readonly Dictionary<string, int> _votedDay = new Dictionary<string, int>();

        private readonly Dictionary<string, float> _lastExternalNotification = new Dictionary<string, float>();

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private System.Collections.IEnumerator VoteCheckAll()
        {
            PrintWarning(nameof(VoteCheckAll));

            var key = serverType == ServerType.x10 ? RUST_SERVERS_KEY_2 : serverType == ServerType.x3 ? RUST_SERVERS_KEY_1 : string.Empty;

            if (string.IsNullOrWhiteSpace(key))
            {
                PrintWarning("key is null on vote check!");
                yield break;
            }


            yield return CoroutineEx.waitForSecondsRealtime(10f);

            while(true)
            {

                var pList = Facepunch.Pool.GetList<string>();

                try
                {
                    GetAllPlayerIDsNoAlloc(ref pList);

                    if (pList.Count < 1)
                    {
                        PrintWarning(nameof(VoteCheckAll) + " had pList.count < 1!");
                        yield break;
                    }

                    PrintWarning("pList.Count is: " + pList.Count);

                    var count = 0;
                    var max = 3;
                    var pCount = pList.Count;
                    var now = DateTime.Now;
                    var notVotedSB = new StringBuilder();
                    var votedSB = new StringBuilder();
                    var notVotedCount = 0;
                    var votedCount = 0;



                    var urlStr = new StringBuilder("https://rust-servers.net/api/?object=votes&element=claim&key=").Append(key).Append("&steamid=").ToString();

                    for (int i = 0; i < pCount; i++)
                    {
                        var countMinus = (pList?.Count ?? -1) - 1;
                        if (countMinus < i)
                        {
                            PrintWarning("VoteCheckAll(): pList.Count -1 is < i: " + countMinus + " < " + i + " -- breaking loop (this probably means a player disconnected while looping)");
                            break;
                        }

                        var userIdString = pList[i];
                        if (userIdString == null) continue;


                        ulong userId;
                        if (!ulong.TryParse(userIdString, out userId))
                        {
                            PrintWarning("unable to parse userIdString into userId?!?!: " + userIdString);
                            continue;
                        }

                        var displayName = GetDisplayNameFromID(userIdString);

                        var player = FindConnectedPlayer(userIdString);

                        int day;
                        if (_votedDay.TryGetValue(userIdString, out day) && day == now.Day)
                        {
                            votedCount++;
                            votedSB.Append(displayName).Append("/").Append(userIdString).Append(Environment.NewLine);
                            continue; //already voted today!
                        }

                        if (count >= max)
                        {
                            count = 0;
                            yield return CoroutineEx.waitForSeconds(4f);
                        }

                        if (_consecutiveFailedRequests > 2)
                        {
                            PrintWarning("at least 3 failed requests in a row, breaking loop");
                            _consecutiveFailedRequests = 0;
                            break;
                        }

                        var url = urlStr + userIdString;
                        webrequest.Enqueue(url, null, (code, response) =>
                        {
                            if (code != 200)
                            {
                                _consecutiveFailedRequests++;
                                PrintWarning("Webrequest failed for vote checking: " + code + Environment.NewLine + response + Environment.NewLine + "Player: " + displayName);
                                return;
                            }
                            else _consecutiveFailedRequests = 0;

                            if (string.IsNullOrWhiteSpace(response))
                            {
                                PrintWarning("Got null or empty response for: " + displayName);
                                return;
                            }

                            if (response == "0")
                            {
                                var voteTxt = "You haven't voted today, you can get a <i>BIG</i> reward! Type <color=#eb3dff>/voteclaim</color> for more info.";

                                var idleTime = (player?.Object as BasePlayer)?.IdleTime ?? 0f;

                                if ((player?.IsConnected ?? false) && idleTime <= 300)
                                {
                                    ShowPopup(userIdString, voteTxt, 9);
                                    SendReply(player, "----<color=orange><size=22>You haven't voted yet!</size></color>----\nGo to: <color=#55cafc>prismrust.com</color>/<color=#62f7bb>vote</color> to vote!\nBe sure to use the same Steam account as the one you want to claim your reward on. For more info and to claim your reward, type <color=#8aff47>/voteclaim</color>.");
                                }

                                float lastNotif;
                                if (!_lastExternalNotification.TryGetValue(userIdString, out lastNotif) || (UnityEngine.Time.realtimeSinceStartup - lastNotif) >= 36000)
                                {
                                    var rustPlusVoteTxt = "You can now vote for the server! Vote to receive a HUGE reward: prismrust.com/vote";
                                    SendRustPlusNotification(userId, rustPlusVoteTxt, "prismrust.com/vote");

                                    // var discordMsg = "You can now vote for the server! Vote to receive a HUGE reward: https://prismrust.com/vote";

                                    //DiscordAPI2?.Call("DMUser", userIdString, discordMsg);

                                    _lastExternalNotification[userIdString] = UnityEngine.Time.realtimeSinceStartup;
                                }

                                notVotedSB.Append(displayName).Append("/").Append(userIdString).Append(Environment.NewLine);
                                notVotedCount++;
                                return;
                            }
                            else if (response == "1" || response == "2") //1 voted, not claimed. 2 voted, claimed
                            {
                                //DateTime time;
                                votedCount++;
                                votedSB.Append(displayName).Append("/").Append(userIdString).Append(Environment.NewLine);
                                _votedDay[userIdString] = now.Day;
                                return;
                            }

                            PrintWarning("Bad response for vote: " + response);
                        }, this);

                        count++;
                        if (pCount > 1) yield return CoroutineEx.waitForSeconds(0.75f);
                    }

                    if (votedSB.Length > 1) votedSB.Length -= 1;
                    if (notVotedSB.Length > 1) notVotedSB.Length -= 1;

                    LogToFile("vote_reminder_log", "Players who voted (" + votedCount.ToString("N0") + "/" + pCount.ToString("N0") + "):" + Environment.NewLine + votedSB.ToString() + "Players who did not vote (" + notVotedCount.ToString("N0") + "/" + pCount.ToString("N0") + "):" + Environment.NewLine + notVotedSB.ToString(), this);

                }
                finally { Facepunch.Pool.FreeList(ref pList); }


                var bigYieldTime = 1800f;

                yield return CoroutineEx.waitForSecondsRealtime(bigYieldTime);
            }

          


        }

        private void Unload()
        {
            if (_reminderCoroutine != null)
            {
                PrintWarning("stopping coroutine on unload");

                ServerMgr.Instance.StopCoroutine(_reminderCoroutine);
                PrintWarning("stopped coroutine!");

            }
            else PrintWarning("reminderCoroutine null on unload");
        }

        private void OnServerInitialized()
        {

            if (ConVar.Server.hostname.Contains("#2")) serverType = ServerType.x10;
            else if (ConVar.Server.hostname.Contains("#3")) serverType = ServerType.BF;
            else serverType = ServerType.x3;


            _reminderCoroutine = ServerMgr.Instance.StartCoroutine(VoteCheckAll());

        }

        private void SendRustPlusNotification(ulong userId, string titleMsg, string bodyMsg = "PRISM", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            NotificationList.SendNotificationTo(userId, channel, titleMsg, bodyMsg, Util.GetServerPairingData());
        }

        private readonly Dictionary<string, Timer> _popupTimer = new Dictionary<string, Timer>();

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

        private void ShowPopupAll(string msg, float duration = 5f)
        {
            foreach (var ply in covalence.Players.Connected) ShowPopup(ply.Id, msg, duration);
        }

        private void GetAllPlayerIDsNoAlloc(ref List<string> players)
        {
            PlayersByDatabase?.Call("GetAllPlayerIDsNoAlloc", players);
        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp))
                throw new ArgumentNullException(nameof(nameOrIdOrIp));

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

    }
}
