using CompanionServer;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Spin For Items", "Shady", "1.0.3", ResourceId = 0)]
    internal class SpinForItems : RustPlugin
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
        //the last item in the spin list should be the actual item they receive (should be done)
        //give spin on connect if after 12 and should be given spin! (done)
        //it should always give maxstackable of the item (where applicable) :P (should be done)

        #region Fields
        private const string CHAT_STEAM_ID = "76561198865536053";

        private const string DATA_FILE_NAME = nameof(SpinForItems) + "Data";

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly HashSet<ItemContainer> _inventories = new HashSet<ItemContainer>();

        private Dictionary<string, SpinData> _spinDatas;

        private readonly Dictionary<string, Timer> _popupTimer = new Dictionary<string, Timer>();

        private readonly Dictionary<string, Coroutine> _activeSpinCoroutine = new Dictionary<string, Coroutine>();

        private readonly Dictionary<string, Coroutine> _activePreSpinCoroutine = new Dictionary<string, Coroutine>();


        private readonly string[] _allowedColors = { "#75ff47", "#459b20", "#8800cc", "#3385ff", "#e6e600", "#669900", "#4d4dff", "#cc0000", "#33ffbb", "#ff00ff", "#725142", "#595959", "#00ffff", "#ffb3cc", "#5c8a8a", "#e6004c", "#db890f", "#ce7235", "#e64d00", "#2c528c", "#42f4e8", "#494f4e", "#b5c40f", "#681144", "#562c2c", "#356656", "#57776d", "#255143", "#1e7258", "#3c3051", "#512f8e", "#2c1d7c", "#DD0000", "#8877ad", "#736b84", "#ff72ad", "#b70b53", "#eaa8d3", "#770750", "#3144a0", "#55a4c1", "#4332fc", "#1d5b77", "#1281C6", "#3E3E3E", "#661B77", "#FACD78", "#E4DCA4", "#4F4533", "#ffe6e6", "#89dc93", "#2B5116", "#BE2227", "#c5a787", "#FF0030", "#FE950A", "#FEF506", "#53D559", "#2BACE2", "#9E88D2", "#58767D", "#525354", "#95A26D", "#552E84", "#939296", "#9B5B5D", "#cd2a2a", "#8D1028", "#C40C23", "#D95A19", "#ECA013", "#6B3421", "#733053", "#FBFF93", "#F9D0DD", "#ffe14a", "#ffabe9", "#ffe0f3", "#c7fcff", "#45f5ff", "#ccf4ff", "#fcffcc", "#f2ff26", "#ff8e52", "#fdebff", "#f494ff", "#c3a1ff", "#7b30ff", "#ff91e7", "#ff00c7", "#ffbff1", "#ff82e4", "#cfff82", "#b5ffd6", "#a0ff52", "#ffdd94", "#ff8fa6", "#2f1cff", "#1c20ff", "#7376ff", "#999bff", "#1d20cc", "#b09419", "#12628a", "#198791", "#37b38e", "#a32a5d", "#992c90", "#2e992c", "#55ab33", "#8aba3d", "#cee64c", "#faaf64", "#ffa587", "#ff9587", "#ff7070", "#8fdbff", "#532696", "#96267e", "#177578", "#006366", "#00521d", "#474000", "#402800", "#421300", "#6e2102", "#8f0c00", "#1b00b3", "#69111e", "#380000", "#000000" };

        private readonly System.Random _rng = new System.Random();

        private Coroutine _dailySpinCoroutine = null;
        private Coroutine _dailyReminderCoroutine = null;

        private List<string> _players = new List<string>();

        [PluginReference]
        private readonly Plugin Luck;

        [PluginReference]
        private readonly Plugin KNGSCredits;

        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        #endregion

        private class SpinData
        {
            public int Spins { get; set; } = 0;

            [JsonRequired]
            private string _lastFreeSpinTimeStr = string.Empty;

            [JsonIgnore]
            public DateTime LastFreeSpinTime
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_lastFreeSpinTimeStr)) return DateTime.MinValue;

                    return DateTime.Parse(_lastFreeSpinTimeStr);
                }
                set
                {
                    _lastFreeSpinTimeStr = value.ToString();
                }
            }

            public bool ShouldGiveFreeSpin()
            {
                return LastFreeSpinTime.Date != DateTime.Now.Date;
            }

            [JsonRequired]
            private bool _hasFreeSpin = false;

            [JsonIgnore]
            public bool HasFreeSpin
            {
                get
                {
                    return _hasFreeSpin;
                }
                set
                {
                    if (value) LastFreeSpinTime = DateTime.Now;

                    _hasFreeSpin = value;
                }
            }


        }

        #region Hooks
        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));

            AddCovalenceCommand("setspins2", nameof(cmdSetSpins));
            AddCovalenceCommand("addspins2", nameof(cmdAddSpins));

            string[] cmds = { "ds", "is", "itemspin", "spins", "spin", "dailyspin", "dailyspins", "dailyitemspin", "itemroll", "itemrolls", "rollforitems", "dailyitem", "dailyitems", "daily", "daliy", "dayly" };
            AddCovalenceCommand(cmds, nameof(cmdRngItem));

            _spinDatas = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<string, SpinData>>(DATA_FILE_NAME) ?? null;


        }

        private System.Collections.IEnumerator SendReminderToPlayers()
        {
            yield return CoroutineEx.waitForSecondsRealtime(10f);

            while (true)
            {


                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    try
                    {
                        if (BasePlayer.activePlayerList.Count < i)
                        {
                            PrintWarning("player count < i now!: " + BasePlayer.activePlayerList.Count + " < " + i);
                            break;
                        }


                        var player = BasePlayer.activePlayerList[i];
                        if (player == null || player.IsDestroyed || !player.IsConnected) continue;

                        var spinData = GetSpinData(player.UserIDString);
                        if (spinData.HasFreeSpin)
                        {
                            SendReply(player, "<color=" + RandomElement(_allowedColors, _rng) + "><size=24>FREE LOOT</color>:</size>\n<color=" + RandomElement(_allowedColors, _rng) + ">You have a <i>free daily item spin</i> to use</color>! <color=" + RandomElement(_allowedColors, _rng) + ">Type <color=" + RandomElement(_allowedColors, _rng) + "><i>/ds</i></color> for more info</color>.");
                            SendLocalEffect(player, "assets/bundled/prefabs/fx/invite_notice.prefab", player.CenterPoint());
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintError(ex.ToString());
                    }


                }

                yield return CoroutineEx.waitForSecondsRealtime(900f);
            }
        }

        private System.Collections.IEnumerator CheckAllPlayers()
        {
            yield return CoroutineEx.waitForSecondsRealtime(5f);


            while (true)
            {

                var now = DateTime.Now;
                if (DateTime.Now.Hour < 12)
                {

                    var neededTime = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0) - now;

                    yield return CoroutineEx.waitForSecondsRealtime((int)Math.Round(neededTime.TotalSeconds, MidpointRounding.AwayFromZero));
                }

                _players.Clear();
                GetAllPlayerIDsNoAlloc(ref _players);

                PrintWarning("found players: " + _players.Count + " (all players this wipe per sql/bps)");

                for (int i = 0; i < _players.Count; i++)
                {
                    var gaveSpin = false;
                    try
                    {



                        var userIdStr = _players[i];
                        var userId = ulong.Parse(userIdStr);

                        var player = BasePlayer.FindByID(userId);

                        var spinData = GetSpinData(userIdStr);

                        if (!spinData.HasFreeSpin && spinData.ShouldGiveFreeSpin())
                        {

                            var dailyMsg = "<size=26><color=" + RandomElement(_allowedColors, _rng) + ">DAILY REWARD</color>:</size>\n<color=" + RandomElement(_allowedColors, _rng) + ">You now have access to your <i>free daily item spin</color>!</i>\n<color=" + RandomElement(_allowedColors, _rng) + ">Type <color=" + RandomElement(_allowedColors, _rng) + ">/ds</color> for more info</color>!";

                            var dailyMsgNoTagsNoNewLine = RemoveTags(dailyMsg).Replace("\n", " ");

                            if (player != null && player.IsConnected) SendReply(player, dailyMsg);

                            if (player == null || !player.IsConnected || player.IdleTime >= 60) SendRustPlusNotification(userId, dailyMsgNoTagsNoNewLine, "PRISM Daily Spin!");

                            spinData.HasFreeSpin = true;

                            if (player != null && player.IsConnected) SendLocalEffect(player, "assets/bundled/prefabs/fx/invite_notice.prefab", player.CenterPoint());

                            var additionalSpinCount = 0;

                            var isVip = KNGSCredits?.Call<bool>("IsVIP", userId) ?? false;

                            if (isVip)
                                additionalSpinCount += 2;
                            

                            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                                additionalSpinCount += 2;
                            

                            if (additionalSpinCount > 0)
                            {
                                spinData.Spins += additionalSpinCount;

                                var extraSpinMsg = "<size=26><color=" + RandomElement(_allowedColors, _rng) + ">EXTRA EXTRA</color>:</size>\n<color=" + RandomElement(_allowedColors, _rng) + ">You've been granted an additional <color=" + RandomElement(_allowedColors, _rng) + ">" + additionalSpinCount.ToString("N0") + " daily spins for free</color>!</i>\n<color=" + RandomElement(_allowedColors, _rng) + ">Type <color=" + RandomElement(_allowedColors, _rng) + ">/ds</color> for more info</color>!";
                                var extraSpinMsgNoTagsNoNewLine = RemoveTags(extraSpinMsg).Replace("\n", " ");

                                if (player != null && player.IsConnected) SendReply(player, extraSpinMsg);

                                if (player == null || !player.IsConnected || player.IdleTime >= 60) SendRustPlusNotification(userId, extraSpinMsgNoTagsNoNewLine, "PRISM EXTRA Daily Spin!");
                            }

                            gaveSpin = true;

                        }
                    }
                    catch (Exception ex) { PrintError(ex.ToString()); }

                    if (!gaveSpin) yield return CoroutineEx.waitForEndOfFrame;
                    else yield return CoroutineEx.waitForSecondsRealtime(2.5f);

                }

                yield break;
            }

        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));

            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var inv = (entity as StorageContainer)?.inventory ?? (entity as LootContainer)?.inventory ?? (entity as NPCPlayerCorpse)?.containers[0] ?? (entity as DroppedItemContainer)?.inventory;

                if (inv != null) _inventories.Add(inv);
            }

            _dailySpinCoroutine = ServerMgr.Instance.StartCoroutine(CheckAllPlayers());
            _dailyReminderCoroutine = ServerMgr.Instance.StartCoroutine(SendReminderToPlayers());
        }

        private void OnEntitySpawned(BaseEntity entity)
        {
            if (entity == null) return;
            var inv = (entity as StorageContainer)?.inventory ?? (entity as LootContainer)?.inventory ?? (entity as DroppedItemContainer)?.inventory;

            if (inv != null) _inventories.Add(inv);
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            var inv = (entity as StorageContainer)?.inventory ?? (entity as LootContainer)?.inventory ?? (entity as DroppedItemContainer)?.inventory;

            if (inv != null) _inventories.Remove(inv);
        }

        private void Unload()
        {
            try
            {
                if (_dailySpinCoroutine != null)
                    ServerMgr.Instance.StopCoroutine(_dailySpinCoroutine);
                
                if (_dailyReminderCoroutine != null)
                    ServerMgr.Instance.StopCoroutine(_dailyReminderCoroutine);
                

                foreach (var kvp in _activeSpinCoroutine)
                {
                    if (ServerMgr.Instance == null) break;

                    try
                    {
                        ServerMgr.Instance.StopCoroutine(kvp.Value);
                    }
                    catch (Exception ex) { PrintError(ex.ToString()); }

                }

                foreach (var kvp in _activePreSpinCoroutine)
                {
                    if (ServerMgr.Instance == null) break;

                    try
                    {
                        ServerMgr.Instance.StopCoroutine(kvp.Value);
                    }
                    catch (Exception ex) { PrintError(ex.ToString()); }
                }
            }
            finally { SaveSpinsData(); }

        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null || !player.IsConnected || player.IsDestroyed || player.gameObject == null) return;

            player.Invoke(() =>
            {
                if (player == null || player.IsDestroyed || !player.IsConnected) return;
            }, 1f);

            var spinData = GetSpinData(player.UserIDString);
            if (!spinData.HasFreeSpin && (spinData.LastFreeSpinTime <= DateTime.MinValue || (DateTime.Now.Hour >= 12 && spinData.ShouldGiveFreeSpin())))
            {
                spinData.HasFreeSpin = true;

                SendReply(player, "<size=26><color=" + RandomElement(_allowedColors, _rng) + ">DAILY REWARD</color>:</size>\n<color=" + RandomElement(_allowedColors, _rng) + ">You now have access to your <i>free daily item spin</color>!</i>\n<color=" + RandomElement(_allowedColors, _rng) + ">Type <color=" + RandomElement(_allowedColors, _rng) + ">/ds</color> for more info</color>!");

                SendLocalEffect(player, "assets/bundled/prefabs/fx/invite_notice.prefab", player.CenterPoint());
            }
        }

        #endregion
        #region Commands


        private void cmdRngItem(IPlayer iPlayer, string command, string[] args)
        {
            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                var player = iPlayer?.Object as BasePlayer;
                if (player == null)
                {
                    iPlayer.Message("This must be ran as a BasePlayer!");
                    return;
                }

                if (player.IsDestroyed || player.IsDead() || !player.IsConnected || player.IsSleeping()) return;

                var spinData = GetSpinData(player.UserIDString);

                var spins = GetItemSpins(player.UserIDString, true);
                if (args.Length < 1)
                {
                    SendReply(player, "<color=#F24998>You have <color=#33ff33>" + spins.ToString("N0") + "</color> available spins.</color>\n<color=#80ffff>You can use these by typing: <color=#42dfff>/" + command + " spin <TimesToSpin> (example: /ds spin 5)</color></color>\nYou'll get one spin every day at 12:00 PM Eastern Time. You can purchase additional spins with <color=#42dfff>/buyspins</color>! Type now for more info.</color>");
                    return;
                }

                var arg0Lower = args[0].ToLower();

                if (arg0Lower.Contains("buy"))
                {
                    player.Command("csay /buyspins");
                    return;
                }

                if (arg0Lower.Contains("stop"))
                {
                    Coroutine preSpinRoutine;

                    if (_activePreSpinCoroutine.TryGetValue(player.UserIDString, out preSpinRoutine))
                    {
                        ServerMgr.Instance.StopCoroutine(preSpinRoutine);
                        _activePreSpinCoroutine.Remove(player.UserIDString);

                        SendReply(player, "No longer automatically spinning");
                    }
                    else SendReply(player, "No spin queue is active!");

                    return;
                }

                var timesToSpin = 1;

                if (arg0Lower != "roll" && arg0Lower != "spin" && !int.TryParse(args[0], out timesToSpin))
                {
                    SendReply(player, "<color=#ff3300>Invalid argument:</color> " + args[0] + Environment.NewLine + "<color=#99ff33>Type <color=#42dfff>/" + command + "</color> to see more information.</color>");
                    return;
                }

                Coroutine currentRoutine;
                if (_activeSpinCoroutine.TryGetValue(player.UserIDString, out currentRoutine))
                {
                    SendReply(player, "<color=#ff3300>You already have a spin in progress!</color>");
                    return;
                }

                if (_activePreSpinCoroutine.TryGetValue(player.UserIDString, out currentRoutine))
                {
                    SendReply(player, "<color=#ff3300>You already have a spin queued up!</color>");
                    return;
                }


                if (spins < 1)
                {
                    SendReply(player, "<color=#ff1a1a>You do not have any spins left!</color>\nYou'll get a free daily spin every day at 12:00 Eastern.\nType <color=#42dfff>/buyspins</color> to purchase additional spins!");
                    return;
                }

                if (args.Length > 1 && !int.TryParse(args[1], out timesToSpin)) timesToSpin = 1;

                timesToSpin = Mathf.Clamp(timesToSpin, 1, spins);

                _activePreSpinCoroutine[player.UserIDString] = ServerMgr.Instance.StartCoroutine(PreSpinCoroutine(player, args, timesToSpin));


            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                iPlayer.Message("Failed to complete command because an error happened. Please report this to an administrator immediately!");
            }
            finally
            {
                try
                {
                    watch.Stop();
                    PrintWarning(nameof(cmdRngItem) + " took: " + watch.ElapsedMilliseconds + "ms (does not include timer)");
                }
                finally { Pool.Free(ref watch); }
            }
        }

        private System.Collections.IEnumerator SpinCoroutine(BasePlayer player, Item itemToGet, bool legendary = false)
        {
            try
            {
                var rarityColor = legendary ? "#F37B00" : itemToGet.info.rarity == Rust.Rarity.VeryRare ? "#F3EBB0" : itemToGet.info.rarity == Rust.Rarity.Rare ? "#7776A6" : "#FCFBFC";
                var rarityStr = "<color=" + rarityColor + ">" + (legendary ? "Legendary" : itemToGet.info.rarity == Rust.Rarity.VeryRare ? "Very Rare" : itemToGet.info.rarity.ToString()) + "</color>";

                var useColors = _allowedColors.Where(p => !string.IsNullOrEmpty(p));
                var max = 56;
                var time = 0.066f;
                var periodCount = 1;

                yield return CoroutineEx.waitForSecondsRealtime(0.1f);

                for (int i = 0; i < max; i++)
                {
                    if (player == null || player.IsDestroyed || player.IsDead() || !player.IsConnected)
                    {
                        PrintError("player has died or is null on coroutine!!! " + nameof(SpinCoroutine));
                        yield break;
                    }

                    if (periodCount > 3) periodCount = 1;

                    SendLocalEffect(player, "assets/bundled/prefabs/fx/notice/loot.copy.fx.prefab");

                    var dotStr = "";
                    for (int j = 0; j < periodCount; j++) dotStr += ".";

                    //perf improvement for rollstr, as well as cleanup - please

                    var rngItem = (i == (max - 1)) ? itemToGet.info : ItemManager.itemList?.Where(p => p.rarity >= Rust.Rarity.Rare || UnityEngine.Random.Range(0, 101) <= 33)?.ToList()?.GetRandom();

                    var rollStr = string.Empty;

                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        rollStr = sb.Clear().Append("<size=").Append(UnityEngine.Random.Range(17, 21)).Append("><color=").Append(RandomElement(useColors, new System.Random())).Append(">").Append(rngItem?.displayName?.english).Append(dotStr).Append("</color></size>").ToString();
                    }
                    finally { Pool.Free(ref sb); }

                    //SendReply(player, rollStr);

                    ShowToast(player, rollStr);

                    periodCount++;

                    if (i == (max - 1))
                    {

                        yield return CoroutineEx.waitForSecondsRealtime(0.33f);

                        var itemStr = itemToGet?.name ?? itemToGet?.info?.displayName?.english ?? "Unknown Item";

                        SendReply(player, "<size=21><color=" + RandomElement(useColors, new System.Random()) + ">You've received <color=" + RandomElement(useColors, new System.Random()) + ">" + itemStr + "</color> (" + rarityStr + ")!</color></size>\n\n<size=22><color=#FF565F>Want to try your luck again</color>? <color=#FF28BE>Spin again</color>! <color=#D221FF>If you're out of spins, check out </color><color=#00A9FF><i>/buyspins</color>!</i>");

                        ShowToast(player, "<color=" + RandomElement(useColors, new System.Random()) + ">" + itemStr + "</color>!");

                        for (int j = 0; j < UnityEngine.Random.Range(8, 22); j++)
                        {
                            Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", player.eyes.transform.position + player.eyes.HeadForward() * 2.2f, Vector3.zero);
                        }

                        SendLocalEffect(player, "assets/prefabs/deployable/repair bench/effects/skinchange_spraypaint.prefab");

                        player.Invoke(() =>
                        {
                            for (int j = 0; j < UnityEngine.Random.Range(1, 3); j++) SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");

                        }, 0.125f);


                        player.Server_CancelGesture();

                        if (legendary || itemToGet.info.rarity >= Rust.Rarity.Rare)
                        {
                            Effect.server.Run("assets/prefabs/misc/casino/slotmachine/effects/payout.prefab", player.transform.position);
                            player.Server_StartGesture(player.gestureList.StringToGesture(UnityEngine.Random.Range(0, 101) <= 33 ? "ok" : "victory"));
                        }

                        if (legendary || itemToGet.info.rarity >= Rust.Rarity.VeryRare)
                        {

                            Effect.server.Run("assets/prefabs/misc/casino/slotmachine/effects/payout_jackpot.prefab", player.transform.position);

                            for (int j = 0; j < 6; j++)
                            {
                                Effect.server.Run("assets/prefabs/misc/halloween/candies/candyexplosion.prefab", player.transform.position);
                            }

                        }
                        else if (itemToGet.info.rarity < Rust.Rarity.Rare && itemToGet.info.rarity != Rust.Rarity.None)
                        {


                            player.Server_StartGesture(player.gestureList.StringToGesture(UnityEngine.Random.Range(0, 101) <= 33 ? "thumbsdown" : "shrug"));

                            for (int j = 0; j < 6; j++)
                            {
                                Effect.server.Run("assets/prefabs/misc/easter/easter basket/effects/eggexplosion.prefab", player.transform.position);
                            }
                        }


                        NoteItemByID(player, itemToGet.info.itemid, 1);

                        //not just charm, we need to check if is legendary for real
                        var isLegendaryItem = Luck?.Call<bool>("IsLegendaryItem", itemToGet) ?? false;

                        if (isLegendaryItem)
                        {
                            PrintWarning("isLegendaryItem! forcing announce");
                            Luck?.Call("CelebrateLegendarySpawnFromItem", player, itemToGet);
                        }

                        if (!itemToGet.MoveToContainer(player.inventory.containerMain) && !itemToGet.MoveToContainer(player.inventory.containerBelt) && !itemToGet.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation))
                            PrintWarning("couldn't drop itemToGet!!");

                    }

                    var adjustedTime = time + (0.00555f * (i * 0.66f)) + ((i >= 44) ? 0.0525f : 0f);

                    yield return CoroutineEx.waitForSecondsRealtime(adjustedTime);
                }

            }
            finally
            {
                _activeSpinCoroutine.Remove(player.UserIDString);
            }
        }

        private System.Collections.IEnumerator PreSpinCoroutine(BasePlayer player, string[] args, int timesToSpin = 1)
        {
            var userId = player?.UserIDString ?? string.Empty;
            try 
            {
                while (timesToSpin > 0)
                {
                    if (player == null || player.IsDestroyed || !player.IsConnected || player.IsDead())
                    {
                        PrintWarning("player null, dead, or disconnected. breaking loop");
                        yield break;
                    }

                    Coroutine spinRoutine;
                    while (_activeSpinCoroutine.TryGetValue(userId, out spinRoutine))
                        yield return CoroutineEx.waitForSeconds(5f);
                    

                    timesToSpin--;

                    var spins = GetItemSpins(player.UserIDString, true);

                    var spinData = GetSpinData(player.UserIDString);

                    Item itemToGet = null;
                    ItemDefinition itemDef = null;

                    var legendaryRng = UnityEngine.Random.Range(0, 110) <= 0 || (player.IsAdmin && args.Length > 1 && args[1].Equals("legendary", StringComparison.OrdinalIgnoreCase));
                    if (legendaryRng)
                    {
                        itemToGet = Luck?.Call<Item>("CreateRandomLegendaryItem");
                        if (itemToGet == null)
                        {
                            PrintError("itemtoget is null from CreateRandomLegendary!!!");
                        }
                    }

                    var charmRng = UnityEngine.Random.Range(0, 101) <= 8 || (player.IsAdmin && args.Length > 1 && args[1].Equals("charm", StringComparison.OrdinalIgnoreCase));
                    if (!legendaryRng && charmRng)
                    {
                        itemToGet = Luck?.Call<Item>("CreateRandomCharm", false, false, UnityEngine.Random.Range(0, 171) <= 0, UnityEngine.Random.Range(0, 281) <= 0, UnityEngine.Random.Range(0.325f, 0.475f)); //rngs are for forcing super
                        if (itemToGet == null)
                        {
                            PrintError("itemtoget is null from createrandomcharm!!!");
                        }
                    }

                   


                    if (itemToGet == null)
                    {
                        var rarityRng = UnityEngine.Random.Range(0, 101);

                        var rarity = rarityRng <= 16 ? Rust.Rarity.VeryRare : rarityRng <= 20 ? Rust.Rarity.None : rarityRng <= 33 ? Rust.Rarity.Rare : rarityRng <= 50 ? Rust.Rarity.Uncommon : Rust.Rarity.Common;

                        var categoryRng = UnityEngine.Random.Range(0, 101);

                        var category = categoryRng <= 10 ? ItemCategory.Items : categoryRng <= 20 ? ItemCategory.Tool : categoryRng <= 25 ? ItemCategory.Misc : categoryRng <= 50 ? ItemCategory.Weapon : categoryRng <= 66 ? ItemCategory.Resources : categoryRng <= 75 ? ItemCategory.Construction : categoryRng <= 85 ? ItemCategory.Ammunition : ItemCategory.Attire;

                        var itemsMatchingRarity = new HashSet<ItemDefinition>();



                        for (int i = 0; i < ItemManager.itemList.Count; i++)
                        {
                            var def = ItemManager.itemList[i];

                            if (def?.rarity != rarity || def?.category != category) continue;

                            var canSpawn = false;
                            if (def.Blueprint == null || !def.Blueprint.userCraftable || !def.Blueprint.isResearchable || (!(def.Blueprint.NeedsSteamDLC || def.Blueprint.NeedsSteamItem)))
                            {
                                //we need to do some additional checks to ensure this item is actually gettable, so we'll loop through all lootcontainers to see if it exists at all. if not, we probably shouldn't spawn it
                                foreach (var inventory in _inventories)
                                {
                                    if (inventory?.itemList == null || inventory.itemList.Count < 1) continue;

                                    if (canSpawn)
                                        break;
                                    

                                    for (int j = 0; j < inventory.itemList.Count; j++)
                                    {
                                        var invItem = inventory.itemList[j];
                                        if (invItem?.info?.itemid == def?.itemid)
                                        {
                                            canSpawn = true;
                                            break;
                                        }
                                    }

                                }
                            }
                            else canSpawn = true;

                            if (canSpawn) itemsMatchingRarity.Add(def);
                        }

                        if (itemsMatchingRarity.Count > 0) itemDef = RandomElement(itemsMatchingRarity, new System.Random());

                        if (itemDef == null)
                        {
                            PrintWarning("initial itemdef null!! grab matching category but skip rarities");

                            for (int i = 0; i < ItemManager.itemList.Count; i++)
                            {
                                var def = ItemManager.itemList[i];

                                if (def?.category != category) continue;

                                var canSpawn = false;
                                if (def.Blueprint == null || !def.Blueprint.userCraftable || !def.Blueprint.isResearchable || (!(def.Blueprint.NeedsSteamDLC || def.Blueprint.NeedsSteamItem)))
                                {
                                    //we need to do some additional checks to ensure this item is actually gettable, so we'll loop through all lootcontainers to see if it exists at all. if not, we probably shouldn't spawn it
                                    foreach (var inventory in _inventories)
                                    {
                                        if (inventory?.itemList == null || inventory.itemList.Count < 1) continue;

                                        if (canSpawn)
                                            break;


                                        for (int j = 0; j < inventory.itemList.Count; j++)
                                        {
                                            var invItem = inventory.itemList[j];
                                            if (invItem?.info?.itemid == def?.itemid)
                                            {
                                                canSpawn = true;
                                                break;
                                            }
                                        }

                                    }
                                }
                                else canSpawn = true;

                                if (canSpawn) itemsMatchingRarity.Add(def);
                            }

                            if (itemsMatchingRarity.Count > 0) itemDef = RandomElement(itemsMatchingRarity, new System.Random());

                            if (itemDef == null)
                            {
                                PrintWarning("still null?!?!?! getting full random");


                                itemDef = ItemManager.itemList.GetRandom();
                                if (itemDef == null)
                                {
                                    PrintWarning("GetRandom() returned nulL!??!?!?!");
                                    yield break;
                                }
                                PrintWarning("fully random get: " + itemDef.displayName.english);
                            }



                        }

                        if (itemDef == null)
                        {
                            SendReply(player, "itemDef is null!!");
                            yield break;
                        }

                        //CREATE THE ITEM

                        itemToGet = ItemManager.Create(itemDef);
                    }


                    var stackScalar = UnityEngine.Random.Range(0.075f, 0.211221f);

                    var maxStack = itemToGet.MaxStackable();

                    itemToGet.amount = Mathf.Clamp((int)(maxStack * stackScalar), 1, maxStack);

                    PrintWarning(player?.displayName + " (" + player?.UserIDString + ") is going to get item: " + itemToGet.info.displayName.english + " x" + itemToGet.amount + " from spin");


                    if (spinData.HasFreeSpin) spinData.HasFreeSpin = false;
                    else SetItemSpins(player.UserIDString, spins - 1);

                    ShowToast(player, "Rolling for an item...");

                    for (int i = 0; i < UnityEngine.Random.Range(2, 6); i++) SendLocalEffect(player, "assets/prefabs/deployable/spinner_wheel/effects/spinner-wheel-deploy.prefab");

                    _activeSpinCoroutine[player.UserIDString] = ServerMgr.Instance.StartCoroutine(SpinCoroutine(player, itemToGet, charmRng || legendaryRng));

                    yield return CoroutineEx.waitForEndOfFrame;
                }
            }
            finally { _activePreSpinCoroutine.Remove(userId); }
        }

        private void cmdAddSpins(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args == null || args.Length < 2) return;
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                player.Message("No player found: " + args[0]);
                return;
            }
            int count;
            if (!int.TryParse(args[1], out count))
            {
                player.Message("Not an int: " + args[1]);
                return;
            }
            AddItemSpins(target.Id, count);
            player.Message("Added spins: " + count + " (total now: " + GetItemSpins(target.Id) + ") for: " + target?.Name);
        }

        private void cmdSetSpins(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args == null || args.Length < 2) return;
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                player.Message("No player found: " + args[0]);
                return;
            }
            int count;
            if (!int.TryParse(args[1], out count))
            {
                player.Message("Not an int: " + args[1]);
                return;
            }
            SetItemSpins(target.Id, count);
            player.Message("Set spins to: " + count + " for: " + target?.Name);
        }
        #endregion
        #region Util
        private ItemDefinition GetRandomItemDef()
        {
            ItemDefinition itemDef = null;

            var rarityRng = UnityEngine.Random.Range(0, 101);

            var rarity = rarityRng <= 12 ? Rust.Rarity.VeryRare : rarityRng <= 15 ? Rust.Rarity.None : rarityRng <= 33 ? Rust.Rarity.Rare : rarityRng <= 50 ? Rust.Rarity.Uncommon : Rust.Rarity.Common;

            var categoryRng = UnityEngine.Random.Range(0, 101);

            var category = categoryRng <= 10 ? ItemCategory.Items : categoryRng <= 20 ? ItemCategory.Tool : categoryRng <= 25 ? ItemCategory.Misc : categoryRng <= 50 ? ItemCategory.Weapon : ItemCategory.Attire;

            var itemsMatchingRarity = new HashSet<ItemDefinition>();

            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var def = ItemManager.itemList[i];

                if (def?.rarity != rarity || def?.category != category) continue;

                var canSpawn = false;
                if (def.Blueprint == null || !def.Blueprint.userCraftable || !def.Blueprint.isResearchable || (!(def.Blueprint.NeedsSteamDLC || def.Blueprint.NeedsSteamItem)))
                {
                    //we need to do some additional checks to ensure this item is actually gettable, so we'll loop through all lootcontainers to see if it exists at all. if not, we probably shouldn't spawn it
                    foreach (var inventory in _inventories)
                    {
                        if (inventory?.itemList == null || inventory.itemList.Count < 1) continue;

                        if (canSpawn)
                            break;


                        for (int j = 0; j < inventory.itemList.Count; j++)
                        {
                            var invItem = inventory.itemList[j];
                            if (invItem?.info?.itemid == def?.itemid)
                            {
                                canSpawn = true;
                                break;
                            }
                        }

                    }
                }
                else canSpawn = true;

                if (canSpawn) itemsMatchingRarity.Add(def);
            }

            if (itemsMatchingRarity.Count > 0) itemDef = RandomElement(itemsMatchingRarity, new System.Random());

            if (itemDef == null)
            {
                PrintWarning("initial itemdef null!! grab fully random");

                itemDef = ItemManager.itemList.GetRandom();

                if (itemDef == null)
                {
                    PrintError("GetRandom() returned nulL!??!?!?!");
                    return null;
                }

                PrintWarning("fully random get: " + itemDef.displayName.english);

            }

            if (itemDef == null)
            {
                PrintError("itemDef is null!!");
                return null;
            }

            //CREATE THE ITEM

            return itemDef;
        }

        private Item GetRandomItem()
        {
            ItemDefinition itemDef = null;

            var rarityRng = UnityEngine.Random.Range(0, 101);

            var rarity = rarityRng <= 12 ? Rust.Rarity.VeryRare : rarityRng <= 15 ? Rust.Rarity.None : rarityRng <= 33 ? Rust.Rarity.Rare : rarityRng <= 50 ? Rust.Rarity.Uncommon : Rust.Rarity.Common;

            var categoryRng = UnityEngine.Random.Range(0, 101);

            var category = categoryRng <= 10 ? ItemCategory.Items : categoryRng <= 20 ? ItemCategory.Tool : categoryRng <= 25 ? ItemCategory.Misc : categoryRng <= 50 ? ItemCategory.Weapon : ItemCategory.Attire;

            var itemsMatchingRarity = new HashSet<ItemDefinition>();

            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var def = ItemManager.itemList[i];

                if (def?.rarity != rarity || def?.category != category) continue;

                var canSpawn = false;
                if (def.Blueprint == null || !def.Blueprint.userCraftable || !def.Blueprint.isResearchable || (!(def.Blueprint.NeedsSteamDLC || def.Blueprint.NeedsSteamItem)))
                {
                    //we need to do some additional checks to ensure this item is actually gettable, so we'll loop through all lootcontainers to see if it exists at all. if not, we probably shouldn't spawn it
                    foreach (var inventory in _inventories)
                    {
                        if (inventory?.itemList == null || inventory.itemList.Count < 1) continue;

                        if (canSpawn)
                            break;


                        for (int j = 0; j < inventory.itemList.Count; j++)
                        {
                            var invItem = inventory.itemList[j];
                            if (invItem?.info?.itemid == def?.itemid)
                            {
                                canSpawn = true;
                                break;
                            }
                        }

                    }
                }
                else canSpawn = true;

                if (canSpawn) itemsMatchingRarity.Add(def);
            }

            if (itemsMatchingRarity.Count > 0) itemDef = RandomElement(itemsMatchingRarity, new System.Random());

            if (itemDef == null)
            {
                PrintWarning("initial itemdef null!! grab fully random");

                itemDef = ItemManager.itemList.GetRandom();

                if (itemDef == null)
                {
                    PrintError("GetRandom() returned nulL!??!?!?!");
                    return null;
                }

                PrintWarning("fully random get: " + itemDef.displayName.english);

            }

            if (itemDef == null)
            {
                PrintError("itemDef is null!!");
                return null;
            }

            //CREATE THE ITEM

            return ItemManager.Create(itemDef);
        }

        private void SendReply(BasePlayer player, string msg, string userId = CHAT_STEAM_ID, params object[] args)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            player.SendConsoleCommand("chat.add", string.Empty, userId, msg, args);
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

        private void GetAllPlayerIDsNoAlloc(ref List<string> list)
        {
            PlayersByDatabase?.Call("GetAllPlayerIDsNoAlloc", list);
        }

        private void SendRustPlusNotification(ulong userId, string titleMsg, string bodyMsg = "PRISM", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            NotificationList.SendNotificationTo(userId, channel, titleMsg, bodyMsg, Util.GetServerPairingData());
        }

        public bool IsDivisble(int x, int n) { return (x % n) == 0; }

        private void ShowToast(BasePlayer player, string message, int type = 0)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.IsConnected)
                return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                player.SendConsoleCommand(sb.Clear().Append("gametip.showtoast ").Append(type).Append(" \"").Append(message).Append("\"").ToString());
            }
            finally { Pool.Free(ref sb); }
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

        private void NoteItemByID(BasePlayer player, int itemID, int amount)
        {
            if (player == null || !player.IsConnected) return;

            var sb = Pool.Get<StringBuilder>();
            try { player.SendConsoleCommand(sb.Clear().Append("note.inv ").Append(itemID).Append(" ").Append(" ").Append(" ").Append(amount).ToString()); } //if the amount is - it will be included automatically, so don't need to add it in the " " sb append
            finally { Pool.Free(ref sb); }
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

        private int GetItemSpins(string userId, bool includeFreeSpin = false)
        {
            if (string.IsNullOrEmpty(userId)) return 0;

            SpinData data;
            if (_spinDatas != null && _spinDatas.TryGetValue(userId, out data))
            {
                return data.Spins + (includeFreeSpin && data.HasFreeSpin ? 1 : 0);
            }
            else return 0;
        }

        private SpinData GetSpinData(string userId)
        {
            SpinData data;
            if (!_spinDatas.TryGetValue(userId, out data))
            {
                data = new SpinData();
                _spinDatas[userId] = data;
            }

            return data;
        }

        private void SetItemSpins(string userId, int amount)
        {
            SpinData data;
            if (!_spinDatas.TryGetValue(userId, out data)) _spinDatas[userId] = new SpinData();

            _spinDatas[userId].Spins = amount;
        }

        private void AddItemSpins(string userId, int add)
        {
            var currentSpins = GetItemSpins(userId);

            if (currentSpins > 75)
                return;

            var totalSpinsAfterAdd = currentSpins + add;
            if (totalSpinsAfterAdd > 75)
                totalSpinsAfterAdd = 75;

            SetItemSpins(userId, totalSpinsAfterAdd);
        }

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using (var fx = new Effect(effect, pos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using (var fx = new Effect(effect, player, boneID, localPos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        private void SaveSpinsData() => Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _spinDatas);

        private void SendInitMessage(BasePlayer player, string msg)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            var isSnap = player?.IsReceivingSnapshot ?? true;
            if (isSnap) timer.Once(1.5f, () => SendInitMessage(player, msg));
            else SendReply(player, msg);
        }
        #endregion
    }
}