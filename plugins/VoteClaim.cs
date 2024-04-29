using ConVar;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Vote Claim", "Shady", "0.0.2", ResourceId = 0)]
    internal class VoteClaim : RustPlugin
    {/*
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
        private const string CHAT_STEAM_ID = "76561198865536053";

        private const string RUST_SERVERS_KEY_2 = ""; //blue (PIZM is almost PRISM!!)
       // private const string RUST_SERVERS_KEY_1 = ""; //red; unused at this time

        private const int VOTE_COOLDOWN_SECS = 45;

        [PluginReference] private readonly Plugin SpinForSkins;

        [PluginReference] private readonly Plugin SpinForItems;

        [PluginReference] private readonly Plugin KNGSCredits;

        [PluginReference] private readonly Plugin PRISMID;

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly string[] _allowedColorsNoSpace = { "#75ff47", "#459b20", "#8800cc", "#3385ff", "#e6e600", "#669900", "#4d4dff", "#cc0000", "#33ffbb", "#ff00ff", "#725142", "#595959", "#00ffff", "#ffb3cc", "#5c8a8a", "#e6004c", "#db890f", "#ce7235", "#e64d00", "#2c528c", "#42f4e8", "#494f4e", "#b5c40f", "#681144", "#562c2c", "#356656", "#57776d", "#255143", "#1e7258", "#3c3051", "#512f8e", "#2c1d7c", "#DD0000", "#8877ad", "#736b84", "#ff72ad", "#b70b53", "#eaa8d3", "#770750", "#3144a0", "#55a4c1", "#4332fc", "#1d5b77", "#1281C6", "#3E3E3E", "#661B77", "#FACD78", "#E4DCA4", "#4F4533", "#ffe6e6", "#89dc93", "#2B5116", "#BE2227", "#c5a787", "#FF0030", "#FE950A", "#FEF506", "#53D559", "#2BACE2", "#9E88D2", "#58767D", "#525354", "#95A26D", "#552E84", "#939296", "#9B5B5D", "#cd2a2a", "#8D1028", "#C40C23", "#D95A19", "#ECA013", "#6B3421", "#733053", "#FBFF93", "#F9D0DD", "#ffe14a", "#ffabe9", "#ffe0f3", "#c7fcff", "#45f5ff", "#ccf4ff", "#fcffcc", "#f2ff26", "#ff8e52", "#fdebff", "#f494ff", "#c3a1ff", "#7b30ff", "#ff91e7", "#ff00c7", "#ffbff1", "#ff82e4", "#cfff82", "#b5ffd6", "#a0ff52", "#ffdd94", "#ff8fa6", "#2f1cff", "#1c20ff", "#7376ff", "#999bff", "#1d20cc", "#b09419", "#12628a", "#198791", "#37b38e", "#a32a5d", "#992c90", "#2e992c", "#55ab33", "#8aba3d", "#cee64c", "#faaf64", "#ffa587", "#ff9587", "#ff7070", "#8fdbff", "#532696", "#96267e", "#177578", "#006366", "#00521d", "#474000", "#402800", "#421300", "#6e2102", "#8f0c00", "#1b00b3", "#69111e", "#380000", "#000000" };

        private readonly Dictionary<string, DateTime> _lastVoteClaim = new Dictionary<string, DateTime>();

        private bool _isHardServer;

        private static readonly System.Random rng = new System.Random();

        private void Init()
        {

            string[] voteCmds = { "voteclaim", "claimvote", "voted", "claimvotes", "votesclaim", "votedclaim", "votereward", "rewardvote", "vote" };
            AddCovalenceCommand(voteCmds, nameof(cmdVoteClaim));

            string[] voteURLCmds = { "voteprism", "voteprism2", "prismrust.com/vote" };
            AddCovalenceCommand(voteURLCmds, nameof(cmdVoteURL));
        }

        private void OnServerInitalized()
        {
            _isHardServer = (PRISMID?.Call<int>("GetServerTypeInt") ?? -1) == 1;
        }

        private void cmdVoteURL(IPlayer player, string command, string[] args)
        {
            SendReply(player, "<color=yellow>This is not a command!</color> <color=orange>If you are looking to vote, go to the webpage on your internet browser: <color=#5af542>https://prismrust.com/vote</color>.</color>" + Environment.NewLine + "You may also type <color=#69fff2>/voteclaim</color> for more info.");
        }

        private void cmdVoteClaim(IPlayer player, string command, string[] args)
        {
            if (player == null) return;
            var pObj = player?.Object as BasePlayer;
            if (pObj == null)
            {
                SendReply(player, "This command can only be ran as a player!");
                return;
            }
            if (!pObj.IsConnected)
            {
                PrintWarning("voteClaim called for non-connected player!");
                return;
            }
            if (pObj.IsDestroyed || pObj.gameObject == null)
            {
                PrintWarning("pObj is destroyed or gameObject is null on vote claim!");
                return;
            }
            if (pObj.IsDead() || pObj.IsSpectating() || pObj?.inventory == null)
            {
                SendReply(player, "You must use this command while alive!");
                return;
            }

            var now = DateTime.UtcNow;
            var nowLocal = DateTime.Now;
            DateTime lastClaim;
            if (_lastVoteClaim.TryGetValue(player.Id, out lastClaim))
            {
                var diff = now - lastClaim;
                if (diff.TotalSeconds < VOTE_COOLDOWN_SECS)
                {
                    SendReply(player, "You're doing this too quickly! Please wait " + (VOTE_COOLDOWN_SECS - diff.TotalSeconds).ToString("0.0").Replace(".0", string.Empty) + " more seconds.");
                    return;
                }
            }


            _lastVoteClaim[player.Id] = now;

            var key = RUST_SERVERS_KEY_2; //BLUE

            if (string.IsNullOrEmpty(key))
            {
                SendReply(player, "This server does not currently have a vote key set. You may want to contact an administrator.");
                return;
            }

            var url = string.Format("https://rust-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}", key, player.Id);

            webrequest.Enqueue(url, null, (code, response) =>
            {
                try
                {
                    if (code != 200)
                    {
                        SendReply(player, "Webrequest failed for vote checking. Please try again soon.");
                        return;
                    }
                    if (string.IsNullOrEmpty(response))
                    {
                        PrintWarning("Got null or empty response for: " + player?.Name);
                        return;
                    }

                    PrintWarning("Vote web request response code: " + code + ", response: " + response);
                    if (pObj == null || pObj.gameObject == null || pObj.IsDestroyed || pObj.inventory == null || pObj.IsDead() || !pObj.IsConnected)
                    {
                        PrintWarning("player is dead/null or disconnected after vote webrequest, returning!");
                        return;
                    }

                    if (response == "0")
                    {
                        SendReply(player, "You haven't voted yet!\nGo to: <color=#55cafc>prismrust.com</color>/<color=#62f7bb>vote</color> to vote!\nBe sure to use the same Steam account as the one you want to claim your reward on.");
                        return;
                    }

                    var isValidTestArg = (args.Length > 0 && args[0].Equals("test") && player.IsAdmin);

                    if (response == "1" || isValidTestArg)
                    {
                        PrintWarning("Voted but not claimed! Giving reward for: " + player?.Name);

                        var isFirst = DateTime.UtcNow.Day == 1;
                        var spinMin = isFirst ? 12 : 8;
                        var spinMax = isFirst ? 32 : 16;
                        var scrapMin = isFirst ? 300 : 100;
                        var scrapMax = isFirst ? 580 : 250;

                        var wipeDate = SaveRestore.SaveCreatedTime;

                        if (nowLocal.DayOfWeek == DayOfWeek.Saturday || nowLocal.DayOfWeek == DayOfWeek.Sunday)
                        {
                            spinMin *= 2;
                            spinMax *= 2;
                            scrapMin *= 2;
                            scrapMax *= 2;
                            SendReply(player, "Reward doubled for voting on the weekend. Thank you! <color=red><3</color>");
                        }
                        else
                        {
                            var span = now - wipeDate;
                            if (wipeDate.Day == now.Day || span.TotalDays <= 3)
                            {
                                spinMin *= 2;
                                spinMax *= 2;
                                scrapMin *= 2;
                                scrapMax *= 2;
                                SendReply(player, "Reward doubled for voting shortly after wipe. Thank you! <color=red><3</color>");
                            }
                        }

                        if (!isValidTestArg)
                            SetVoteClaimed(player.Id, key);

                        var rngSpins = UnityEngine.Random.Range(spinMin, spinMax);
                        var rngItemSpins = UnityEngine.Random.Range(spinMin, spinMax);
                        var credsToAdd = isFirst ? 75 : 5;

                        SpinForSkins?.Call("AddSkinSpins", player.Id, rngSpins);

                        SpinForItems?.Call("AddItemSpins", player.Id, rngItemSpins);

                        KNGSCredits?.Call("GiveCredits", player.Id, credsToAdd);

                        var rngScrapAmount = UnityEngine.Random.Range(scrapMin, scrapMax);

                        if (_isHardServer) //RED
                        {
                            PrintWarning(nameof(_isHardServer) + " true!");
                            rngScrapAmount = Mathf.Clamp((int)(rngScrapAmount * 0.25f), 25, int.MaxValue);
                        }

                        var scrapDef = ItemManager.FindItemDefinition("scrap");

                        NoteItem(pObj, scrapDef, rngScrapAmount);

                        var rewardItem = ItemManager.Create(scrapDef, rngScrapAmount);
                        if (!rewardItem.MoveToContainer(pObj.inventory.containerMain) && !rewardItem.MoveToContainer(pObj.inventory.containerBelt) && !rewardItem.Drop(pObj.GetDropPosition(), pObj.GetDropVelocity()))
                        {
                            PrintWarning("failed to move rewardItem or drop rewardItem, now removing from world!!");
                            RemoveFromWorld(rewardItem);
                        }

                      
                        //fix this disaster of a message
                        //slightly fixed? had some fun for fun's sake
                        SimpleBroadcast("<size=17>Thanks for voting, <i><size=20><color=" + RandomElement(_allowedColorsNoSpace, rng) + ">" + player.Name + "</color>! <3</size></i>\n\nBecause you voted, we'll throw in a FREE OFFER: <color=" + RandomElement(_allowedColorsNoSpace, rng) + "><i><size=25>" + rngScrapAmount.ToString("N0") + "</size></i></color> <size=21><color=" + RandomElement(_allowedColorsNoSpace, rng) + ">" + rewardItem.info.displayName.english + "!</color></size> But wait, there's more! <i><size=30><color=" + RandomElement(_allowedColorsNoSpace, rng) + ">" + rngItemSpins.ToString("N0") + "</size></i></color> item spins to use on <i><size=20><color=orange>/ds</color></size></i>.\nThat's not all folks, for voting, we'll even throw in this $" + (credsToAdd * 0.01) + " offer of " + credsToAdd.ToString("N0") + " Credit" + ((credsToAdd > 1) ? "s" : "") + " to spend on <color=green>/creds</color>!\nIt's easy! Type <i><size=32><color=green>/vote</color></size></i> to learn more now!</size>");

                        //SendReply(player, "Thanks for voting! Here's your reward: " + credsToAdd.ToString("N0") + " Credit" + ((credsToAdd > 1) ? "s" : "") + " to use in <color=green>/creds</color> (used to purchase ranks!)" + rewardItem.info.displayName.english + " x" + rngScrapAmount.ToString("N0") + Environment.NewLine + "<color=#FFBF00>You have also gained <color=#BC21BC>" + rngSpins.ToString("N0") + "</color> roll(s) to use on <color=#569E2C>/skins</color>!</color>\nALSO:\nYou've gained " + rngItemSpins.ToString("N0") + " item spins for <color=orange>/ds</color>!"); ;
                       
                        return;
                    }

                    if (response == "2")
                    {
                        SendReply(player, "We appreciate you voting, but you've already claimed today's reward! Please be sure to vote again later for a <size=20>BIG</size> reward!");
                        return;
                    }

                    SendReply(player, "Unknown response: " + response + " REPORT TO AN ADMIN!");
                    PrintWarning("Bad response for vote: " + response);

                }
                catch (Exception ex)
                {
                    PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete vote web request^");
                    SendReply(player, "Couldn't get vote information (" + ex.Message + ")");
                }
            }, this);
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

        public T RandomElement<T>(IEnumerable<T> source, System.Random rng)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }

        private void SetVoteClaimed(string userId, string serverKey)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(serverKey)) return;
            var url = string.Format("https://rust-servers.net/api/?action=post&object=votes&element=claim&key={0}&steamid={1}", serverKey, userId);
            webrequest.Enqueue(url, null, (code, response) =>
            {
                try
                {
                    if (code != 200)
                    {
                        PrintWarning("Not code 200 for setting vote claimed for player: " + userId);
                        return;
                    }
                    if (string.IsNullOrEmpty(response))
                    {
                        PrintWarning("Got null or empty response for: " + userId);
                        return;
                    }
                    PrintWarning("response: " + response + ", code: " + code);
                }
                catch (Exception ex)
                {
                    PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete vote web request^");
                }
            }, this, Core.Libraries.RequestMethod.POST);
        }

        private void SimpleBroadcast(string msg, ulong userID = 0)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            Chat.ChatEntry chatEntry = new Chat.ChatEntry()
            {
                Channel = Chat.ChatChannel.Server,
                Message = RemoveTags(msg),
                UserId = userID.ToString(),
                Time = Facepunch.Math.Epoch.Current
            };

            Facepunch.RCon.Broadcast(Facepunch.RCon.LogType.Chat, chatEntry);
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || !player.IsConnected) continue;
                if (userID > 0) player.SendConsoleCommand("chat.add", string.Empty, userID, msg);
                else SendReply(player, msg);
            }

        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private void NoteItemByID(BasePlayer player, int itemID, int amount)
        {
            if (player == null || !player.IsConnected) return;

            var sb = Pool.Get<StringBuilder>();
            try { player.SendConsoleCommand(sb.Clear().Append("note.inv ").Append(itemID).Append(" ").Append(" ").Append(" ").Append(amount).ToString()); } //if the amount is - it will be included automatically, so don't need to add it in the " " sb append
            finally { Pool.Free(ref sb); }
        }

        private void NoteItem(BasePlayer player, ItemDefinition def, int amount)
        {
            NoteItemByID(player, def.itemid, amount);
        }

        private void NoteItemByAmount(BasePlayer player, ItemAmount amount)
        {
            NoteItemByID(player, amount.itemDef.itemid, (int)amount.amount);
        }

    }
}