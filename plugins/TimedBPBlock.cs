using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Timed BP Block", "Shady", "0.0.1", ResourceId = 0)]
    internal class TimedBPBlock : RustPlugin
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
        public Dictionary<int, int> ItemUnavailableForSeconds { get; set; }

        public Dictionary<int, string> ItemIDToDisplayName { get; set; } = new Dictionary<int, string>();

        public TimeSpan SaveSpan { get { return (DateTime.UtcNow - SaveRestore.SaveCreatedTime); } }

        private readonly Dictionary<string, Timer> _popupTimer = new Dictionary<string, Timer>();

        private void Init()
        {
            ItemUnavailableForSeconds = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<int, int>>("TimedBPBlock") ?? new Dictionary<int, int>();

            if (ItemUnavailableForSeconds.Count < 1)
            {
                ItemUnavailableForSeconds[-1] = 7200;
                SaveData();
            }

            string[] cmdAliases = { "bpblock", };
            AddCovalenceCommand(cmdAliases, nameof(cmdBpBlock));
        }


        private void OnServerInitialized()
        {
            for(int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                ItemIDToDisplayName[item.itemid] = item.displayName.english;
            }
        }

        private object CanResearchItem(BasePlayer player, Item targetItem)
        {
            PrintWarning("canresaerch: " + player + ", targetItem: " + targetItem);
            return OnPlayerStudyBlueprint(player, targetItem);
        }

        private object OnPlayerStudyBlueprint(BasePlayer player, Item item)
        {
            if (player == null || item == null) return null;


            var bpMsg = IsBlueprintBlocked(item.info.itemid)?.ToString() ?? IsBlueprintBlocked(item.blueprintTarget)?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(bpMsg)) return null; //not blocked

            SendBlueprintBlockedFX(player, bpMsg);

            PrintWarning("Blocking a BP");

            return true;
        }

        private void SendBlueprintBlockedFX(BasePlayer player, string bpMsg)
        {
           
            if (player == null || !player.IsConnected) return;

            SendLocalEffect(player, "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab");

            if (!string.IsNullOrEmpty(bpMsg))
            {
                SendReply(player, bpMsg);

                ShowPopup(player.UserIDString, bpMsg, 8f);
            }
        }

        private void Unload() => SaveData();

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("TimedBPBlock", ItemUnavailableForSeconds);
        }

        private string GetDisplayName(int itemId)
        {
            string name;
            if (ItemIDToDisplayName.TryGetValue(itemId, out name)) return name;

            return string.Empty;
        }

        private void cmdBpBlock(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsAdmin) return;

            if (args.Length < 1)
            {
                player.Message("Must supply args! Try: list, set");
                return;
            }

            var sb = Facepunch.Pool.Get<StringBuilder>();

            try 
            {
                sb.Clear();
                var arg0 = args[0];

                if (arg0.Equals("list"))
                {
                    foreach (var kvp in ItemUnavailableForSeconds)
                    {
                        var item = kvp.Key;
                        var seconds = kvp.Value;

                        var displayName = GetDisplayName(item);

                        sb.Append(displayName).Append(" (").Append(item).Append("): " + seconds).Append("s").Append(Environment.NewLine);

                    }

                    if (sb.Length > 1) sb.Length -= 1;

                    player.Message(sb.ToString());
                }
                else if (arg0.Equals("set"))
                {
                    if (args.Length < 3)
                    {
                        player.Message("Must specify item ID and seconds (use -1 to remove from list)!");
                        return;
                    }

                    int itemId;

                    if (!int.TryParse(args[1], out itemId))
                    {
                        var findItem = FindItemByPartialName(args[1]);
                        if (findItem != null) itemId = findItem.itemid;
                        else
                        {
                            player.Message("Not an int: " + args[1] + " and we failed to find any item matching that name");
                            return;
                        }
                    }

                    string itemName;

                    if (!ItemIDToDisplayName.TryGetValue(itemId, out itemName))
                    {
                        player.Message("Invalid item ID or item has no display name!!");
                        return;
                    }

                    int seconds;

                    if (!int.TryParse(args[2], out seconds))
                    {
                        player.Message("Not an int: " + args[2]);
                        return;
                    }

                    if (seconds < 0) ItemUnavailableForSeconds.Remove(itemId);
                    else ItemUnavailableForSeconds[itemId] = seconds;

                    player.Message(sb.Clear().Append("Set ").Append(itemName).Append(" (").Append(itemId).Append(") to ").Append(seconds).Append("s").ToString());
                    SaveData();

                }
            }
            finally { Facepunch.Pool.Free(ref sb); }

          

        }

        private object IsBlueprintBlocked(int itemId)
        {

            int seconds;
            if (!ItemUnavailableForSeconds.TryGetValue(itemId, out seconds)) return null; //unchanged behavior

            if (seconds < 0)
            {
                PrintWarning("item has NEGATIVE seconds: " + seconds + " : " + itemId);
                return null;
            }

            if (seconds == 0)
            {
                return "<color=#ff5280>This blueprint cannot be studied</color>."; //disabled permanently
            }

            if (SaveSpan.TotalSeconds >= seconds) return null; //return because time has passed, it's available. otherwise, do blocking below

            var newSpan = TimeSpan.FromSeconds(seconds).Subtract(SaveSpan);


            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                var itemInfo = ItemManager.FindItemDefinition(itemId);
                return sb.Clear().Append("<color=#ffc72b>").Append(itemInfo?.displayName?.english ?? string.Empty).Append("</color> <color=#fff563>will be available to be studied in</color><color=#ff5eb4>:</color> <color=#82ff29>").Append(ReadableTimeSpan(newSpan)).Append("</color>").ToString();
            }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private bool IsBlueprintPermanentlyBlocked(int blueprintTarget)
        {
            int seconds;
            return ItemUnavailableForSeconds.TryGetValue(blueprintTarget, out seconds) && seconds == 0;
        }

        private bool IsBlueprintUnlockedYet(int blueprintTarget)
        {
            int seconds;
            if (!ItemUnavailableForSeconds.TryGetValue(blueprintTarget, out seconds)) return true;

            return seconds > 0 && SaveSpan.TotalSeconds >= seconds;

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

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using (var fx = new Effect(effect, player, boneID, localPos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
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

                str = (int)span.TotalDays + (!showHours && span.TotalHours > 24 ? 1 : 0) + " day" + (span.TotalDays >= 1.5 ? "s" : "") + (showHours ? (" " + totalHoursWereGoingToShowToTheUserAsAString + " hour(s)") : "");
            }
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        private ItemDefinition FindItemByPartialName(string engOrShortName)
        {
            if (string.IsNullOrEmpty(engOrShortName)) throw new ArgumentNullException(nameof(engOrShortName));


            var matches = Facepunch.Pool.GetList<ItemDefinition>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item.displayName.english.Equals(engOrShortName, StringComparison.OrdinalIgnoreCase) || item.shortname.Equals(engOrShortName, StringComparison.OrdinalIgnoreCase)) return item;

                    var engName = item?.displayName?.english;

                    if (engName.IndexOf(engOrShortName, StringComparison.OrdinalIgnoreCase) >= 0 && !matches.Contains(item)) matches.Add(item);
                    if (item.shortname.IndexOf(engOrShortName, StringComparison.OrdinalIgnoreCase) >= 0 && !matches.Contains(item)) matches.Add(item);
                }

                return matches.Count != 1 ? null : matches[0];
            }
            finally { Facepunch.Pool.FreeList(ref matches); }
        }

    }
}