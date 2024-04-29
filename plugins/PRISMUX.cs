using CompanionServer;
using ConVar;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries;
using Oxide.Game.Rust.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("PRISM UX", "Shady", "0.0.5")]
    internal class PRISMUX : RustPlugin
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

        private const string CHAT_STEAM_ID = "76561198865536053";
        private const string DATA_FILE_NAME = "PRISMUX_Data";
        private const string PRISM_UX_CHAT_PREFIX = "<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color> | <color=#fffcb5>UX:</color> ";
        private const string LUCK_COLOR = "#5DC540";

        private const string PRISM_RAINBOW_TXT = "<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color>";

        private const int BATTERY_ITEM_ID = 609049394;
        private const int NOTE_ITEM_ID = 1414245162; //used for charms
        private const ulong PRISM_BLUE_NOTE_SKIN_ID = 3033755472;

        private readonly FieldInfo _chatCommandsField = typeof(Command).GetField("chatCommands", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

        private readonly FieldInfo _registeredCommandsField = typeof(RustCommandSystem).GetField("registeredCommands", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        private readonly PropertyInfo _covProviderInstanceProperty = typeof(RustCovalenceProvider).GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);


        private readonly HashSet<string> _forbiddenTags = new() { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        private readonly HashSet<string> _allCommandNames = new();

        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        [PluginReference]
        private readonly Plugin CargoshipControl;

        [PluginReference]
        private readonly Plugin HeliControl;

        [PluginReference]
        private readonly Plugin Compilation;

        [PluginReference]
        private readonly Plugin Luck;

        [PluginReference]
        private readonly Plugin ZLevelsRemastered;

        private Dictionary<string, UXData> _userUxData;

        private class UXData
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool HasLootedBatteries { get; set; } = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool HasLootedCharm { get; set; } = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool HasLootedBackpack { get; set; } = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool HasEarnedSurvivalXP { get; set; } = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool HasEarnedWoodcuttingXP { get; set; } = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool HasEarnedMiningXP { get; set; } = false;

            public UXData() { }

        }

        #region Commands

        #endregion

        #region Hooks

        private void Init()
        {
            Puts(nameof(Init) + " loading data...");

            _userUxData = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, UXData>>(DATA_FILE_NAME);

            Puts("Loaded data: " + _userUxData.Count);


        }

        private void OnServerInitialized()
        {
            PrintWarning("Populating command names...");

            UpdateAllCommandNames();

            PrintWarning("Populated. Command names: " + _allCommandNames.Count.ToString("N0"));
        }

        private void Unload()
        {
            Puts(nameof(Unload) + " saving data...");

            SaveData();

            Puts("data saved");
        }

        private void OnAutoDoorClose(Door door, BasePlayer player) //hook provided by AutoDoors.cs (Shady edit)
        {
            if (door == null || player == null) return;

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                var str = sb.Clear().Append(PRISM_UX_CHAT_PREFIX).Append("Your doors close <i>automatically</i>. To adjust this, type <i><color=#33ccff>/ad</color>!</i>").ToString();

                SendCooldownMessage(player, str, 600f, "AUTO_DOOR_CLOSE");
            }
            finally { Pool.Free(ref sb); }

        }

        private void OnStructureUpgrade(BuildingBlock block, BasePlayer player, BuildingGrade.Enum grade)
        {
            if (block == null || player == null || (grade != BuildingGrade.Enum.Stone && grade != BuildingGrade.Enum.Metal))
                return;

            block.Invoke(() =>
            {
                if (player == null || !player.IsConnected || block.skinID != 0)
                    return;

                var sb = Pool.Get<StringBuilder>();

                try
                {
                    var skinName = grade == BuildingGrade.Enum.Stone ? "Adobe" : "Shipping Container";
                    var cmdName = grade == BuildingGrade.Enum.Stone ? "adb" : "sc";

                    var reminderName = sb.Clear().Append(grade == BuildingGrade.Enum.Stone ? "ADOBE_" : "SHIPPING_").Append("REMINDER").ToString();

                    SendCooldownMessage(player, sb.Clear().Append("Want to use the <size=15>NEW</size> <i>").Append(skinName).Append("</i> skin? Just type <i><color=#C99D75>/").Append(cmdName).Append("</color>!</i>").ToString(), 180f, reminderName);
                }
                finally { Pool.Free(ref sb); }


            }, 0.1f);
        }

        private void OnEarnZXP(BasePlayer player, Item item, string skill, double points)
        {
            if (player == null || string.IsNullOrWhiteSpace(skill)) return;


            //placeholder variables - move to consts
            var survivalMsg = PRISM_UX_CHAT_PREFIX + "Survival can be leveled up by performing general <i>survival <size=10>[ ;) ]</size></i> actions, such as picking berries, gutting animals, and cracking open barrels!\nWith each level, you'll gain more resources from crops and animals. You will also <i>run</i> a little bit faster with each level!";
            var miningMsg = PRISM_UX_CHAT_PREFIX + "Smash ores to gain Mining XP!\nWith each level, you'll gain more resources from ores. If you're a Miner, you'll see ores on your map starting at level 25!";
            var woodcuttingMsg = PRISM_UX_CHAT_PREFIX + "Chop <color=#977C46>trees</color> or slash <color=#217342>cacti</color> to gain <color=#977C46><i>Woodcutting XP</color>!</i>\nWith each level, you'll gain more resources from <color=#977C46>trees</color> (and <color=#217342>cacti</color>). Be on the lookout for <i>valuable</i> eggs dropping from <color=#977C46>trees</color>, as some of these trees happen to be homes to birds who've nested there!";

            var uxData = GetUserUXData(player.UserIDString);

            if (uxData == null) return;

            if (skill.Equals("S") && !uxData.HasEarnedSurvivalXP)
            {
                uxData.HasEarnedSurvivalXP = true;
                SendReply(player, survivalMsg);
            }
            else if (skill.Equals("M") && !uxData.HasEarnedMiningXP)
            {
                uxData.HasEarnedMiningXP = true;
                SendReply(player, miningMsg);
            }
            else if (skill.Equals("WC") && !uxData.HasEarnedWoodcuttingXP)
            {
                uxData.HasEarnedWoodcuttingXP = true;
                SendReply(player, woodcuttingMsg);
            }
        }

        private void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null || item?.info == null) return;

            var player = container?.playerOwner ?? container?.GetOwnerPlayer() ?? FindPlayerByContainer(container, true) ?? null;


            if (player == null || !IsValidPlayer(player)) return;

            var ux = GetUserUXData(player.UserIDString);
            if (ux == null)
            {
                PrintError("somehow ux data is null: " + player);
                return;
            }

            if (item.info.itemid == BATTERY_ITEM_ID && !ux.HasLootedBatteries)
            {
                PrintWarning("first loot batteries for: " + player);

                var foundStr = PRISM_UX_CHAT_PREFIX + "Those are some nice lookin' <size=16><i><color=" + LUCK_COLOR + ">batteries!</i></size> Did you know, you can trade your batteries for other goods, such as <color=#FDFAE7>components?</color>\nYou can find " + PRISM_RAINBOW_TXT + "-exclusive Vending Machines at <i>Outpost</i> where you can purchase numerous things with batteries!";
                var foundStrShort = "Trade your batteries for goods at Outpost!";

                ShowPopup(player, foundStrShort);

                SendReply(player, foundStr);
                ux.HasLootedBatteries = true;
            }

            if (item.info.itemid == NOTE_ITEM_ID && item.skin != 0 && item.skin != PRISM_BLUE_NOTE_SKIN_ID && !string.IsNullOrWhiteSpace(item.text) && !ux.HasLootedCharm)
            {
                var foundStr = PRISM_UX_CHAT_PREFIX + "Would you look at that? A charm! A <color=#E04957>l</color><color=#E18543>u</color><color=#E6DC47>c</color><color=#A5CC57>k</color><color=#62CBC9>y</color> <color=#6367A6>charm</color>, oh, no, sorry<color=#BD1E38>...</color> A <color=" + LUCK_COLOR + "><i>Luck</i></color> charm! These things give you bonuses as long as they're in your inventory.\nClick on the Charm in your inventory to read its text & see the bonus!";
                var foundStrShort = "A <color=#E04957>l</color><color=#E18543>u</color><color=#E6DC47>c</color><color=#A5CC57>k</color><color=#62CBC9>y</color><color=yellow>--</color>A <color=" + LUCK_COLOR + "><i>Luck</i></color> charm!";

                ShowPopup(player, foundStrShort);

                SendReply(player, foundStr);
                ux.HasLootedCharm = true;
            }

            if (!ux.HasLootedBackpack && (Luck?.Call<bool>("IsBackpackSkin", item.skin) ?? false))
            {
                var foundStr = PRISM_UX_CHAT_PREFIX + "What a nice <color=" + LUCK_COLOR + "><size=17>Backpack</i></color> you've got there! You can equip it like it's a piece of armor, then look behind yourself or click on the item in your inventory and press <size=15><i>'<color=" + LUCK_COLOR + ">Open Bag</color>'</i></size>  to open it!"; //double space after italic open bag since it seems to squish text a bit
                var foundStrShort = "<size=17><i><color=" + LUCK_COLOR + ">Nice Backpack!</i>\n<color=#fffcb5>Wear it like a piece of armor - you'll even see it on your back!</color>";

                ShowPopup(player, foundStrShort);

                SendReply(player, foundStr);
                ux.HasLootedBackpack = true;
            }

        }

        private void SimpleBroadcast(string msg, string userID = CHAT_STEAM_ID)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            Chat.ChatEntry chatEntry = new()
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

                if (!string.IsNullOrWhiteSpace(userID)) player.SendConsoleCommand("chat.add", string.Empty, userID, msg);
                else SendReply(player, msg);
            }

        }

        private void BroadcastCargoInfo()
        {
            var nextShip = CargoshipControl?.Call<DateTime>("NextShipTime") ?? DateTime.MinValue;

            if (nextShip == DateTime.MinValue) return;

            var now = DateTime.Now;

            var str = PRISM_UX_CHAT_PREFIX + (nextShip <= now ? "There's a cargo ship out, right now!" : ("Cargo Ship will be entering the map in " + ReadableTimeSpan(nextShip.Subtract(now))));

            BroadcastCooldownMessage(str, 120f, "UX_CARGO");
        }

        private void BroadcastHeliInfo()
        {
            var nextHeli = HeliControl?.Call<TimeSpan>("GetNextHeliTime") ?? TimeSpan.Zero;

            if (nextHeli <= TimeSpan.Zero) return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var str = sb.Clear().Append(PRISM_UX_CHAT_PREFIX).Append("Patrol Heli will enter the map again in ").Append(ReadableTimeSpan(nextHeli)).ToString();

                BroadcastCooldownMessage(str, 120f, "UX_HELI");
            }
            finally { Pool.Free(ref sb); }
        }

        private void BroadcastLoveInfo() //intellisense created this after inferring what i was doing from the two above, strange, but I like it
        {

        }

        private void BroadcastDiscordInfo()
        {
            var helpStr = Compilation?.Call<string>("GetContactHelpMessage") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(helpStr)) return;

            var sb = Pool.Get<StringBuilder>();
            try 
            {
                var str = sb.Clear().Append(PRISM_UX_CHAT_PREFIX).Append(helpStr).ToString();

                BroadcastCooldownMessage(str, 180f, "UX_CONTACT_HELP");
            }
            finally { Pool.Free(ref sb); }
            
        }

        private void BroadcastChinookInfo()
        {
            var nextCh47 = HeliControl?.Call<TimeSpan>("GetNextCH47Time") ?? TimeSpan.Zero;

            if (nextCh47 <= TimeSpan.Zero) return;

            var str = PRISM_UX_CHAT_PREFIX + "The CH-47 Chinook will enter the map again in " + ReadableTimeSpan(nextCh47);
            BroadcastCooldownMessage(str, 120f, "UX_CHINOOK");
        }

        private void BroadcastLuckInfo()
        {
            var str = PRISM_UX_CHAT_PREFIX + "<size=19><color=" + LUCK_COLOR + "><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color> ARPG System</color>:</size>\n\n<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color> <color=#61f4c1>provides a <i>premium</i> <color=#6ff7a1>ARPG</color> experience for Rust. To read info about this, press <color=#63f968><size=22><i>F1</i></size></color> on your keyboard - you'll see an expansive help menu!\nFinally, try <color=#f963ae><i>/skt</i></color> or <color=#f9ae63><i>/class</i></color> to read more info, too!</color>";

            BroadcastCooldownMessage(str, 210f, "UX_LUCK_INFO");
        }

        private void BroadcastCharmInfo()
        {
            var str = PRISM_UX_CHAT_PREFIX + "<size=20>Luck <size=10>(not luck<i>y</i>)</size> Charms:</size>\nType <color=" + LUCK_COLOR + "><i><size=20>/charm</size></i></color> to find out more info about these little things!";

            BroadcastCooldownMessage(str, 120f, "UX_CHARM");
        }

        private void UpdateListOfCommandNames()
        {

        }

        private Item GenerateBLUENote()
        {
            var note = ItemManager.CreateByItemID(NOTE_ITEM_ID, 1, PRISM_BLUE_NOTE_SKIN_ID); //use custom icon skin later on

            note.name = "PRISM | BLUE";

            note.text = "Welcome to PRISM!\nPlease press F1 to read our Help Menu.\nJoin Discord for additional help:\ndiscord.gg/DUCnZhZ";

            return note;
        }

        private object OnServerCommand(ConsoleSystem.Arg arg)
        {
            if (arg == null) return null;
            var args = arg?.Args ?? null;
            var command = arg?.cmd?.FullName ?? string.Empty;

            if (string.IsNullOrEmpty(command) || args == null) return null;
            if (args != null && args.Length > 1 && command.Equals("note.update", StringComparison.OrdinalIgnoreCase))
            {
                var note = FindItemByIDU(ulong.Parse(args[0]));

                if (note != null)
                {
                    var oldText = note?.text ?? string.Empty;

                    if (oldText.IndexOf("Welcome to PRISM!", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }

            return null;
        }

        private void OnDefaultItemsReceived(PlayerInventory inventory)
        {
            var note = GenerateBLUENote();

            if (!note.MoveToContainer(inventory.containerBelt, allowStack:false))
                RemoveFromWorld(note);

        }

        private void OnBetterChatClean(string userId, string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;

            /*/
            var msgNoSpaceLowerCase = RemoveRepeatingCharacters(msg.ToLower(CultureInfo.InvariantCulture)).Replace(" ", string.Empty);

            PrintWarning(nameof(msgNoSpaceLowerCase) + " is: " + msgNoSpaceLowerCase);

            if (msgNoSpaceLowerCase.Contains("heli"))
            {

            }/*/ //example of an issue with this "yo he lied about that" ("he lied" -> "helied" -> .contains('heli')

            var lowerCaseMsg = msg.ToLower(CultureInfo.InvariantCulture);

            var p = covalence.Players.FindPlayerById(userId);
            if (p != null && p.IsConnected)
            {
                foreach (var cmd in _allCommandNames)
                {
                    if (!msg.Equals(cmd, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        var str = sb.Clear().Append(PRISM_UX_CHAT_PREFIX).Append(p?.Name).Append(", did you mean <color=green>/</color><color=yellow>").Append(cmd).Append("</color>?").ToString();

                        BroadcastCooldownMessage(str, 30f, sb.Clear().Append("UX_CMD_").Append(cmd).ToString());
                    }
                    finally { Pool.Free(ref sb); }

                    break;
                }
            }

            if (lowerCaseMsg.Contains("heli") || lowerCaseMsg.Contains("patrol"))
            {
                BroadcastHeliInfo();
            }

            if (lowerCaseMsg.Contains("cargo") || lowerCaseMsg.Contains("ship ") || lowerCaseMsg.Contains("nextship"))
            {
                BroadcastCargoInfo();
            }

            if (lowerCaseMsg.Contains("ch47") || lowerCaseMsg.Contains("chinook"))
            {
                BroadcastChinookInfo();
            }

            if (lowerCaseMsg.Contains("discord") || lowerCaseMsg.Contains("ts3"))
            {
                BroadcastDiscordInfo();
            }

            if (lowerCaseMsg.Contains("skt") || lowerCaseMsg.Contains("skill tree") || lowerCaseMsg.Contains("skills") || lowerCaseMsg.Contains("skils") || lowerCaseMsg.Contains("arpg") || lowerCaseMsg.Contains("rpg") || lowerCaseMsg.Contains("class"))
            {
                BroadcastLuckInfo();   
            }

            if (lowerCaseMsg.Contains("charm") || lowerCaseMsg.Contains("cahrm"))
                BroadcastCharmInfo();

        }

        #endregion

        #region Util
        private void UpdateAllCommandNames()
        {
            var cmd = Interface.Oxide.GetLibrary<Command>();

            var covInstance = _covProviderInstanceProperty.GetValue(null) as RustCovalenceProvider;

            var chatCmds = _chatCommandsField.GetValue(cmd);

            var keys = (IEnumerable<string>)chatCmds.GetType().GetProperty("Keys").GetValue(chatCmds, null);

            foreach (var str in keys)
                _allCommandNames.Add(str);


            var regCmds = _registeredCommandsField.GetValue(covInstance.CommandSystem);


            keys = (IEnumerable<string>)regCmds.GetType().GetProperty("Keys").GetValue(regCmds, null);

            foreach (var str in keys)
                _allCommandNames.Add(str);
        }

        private string RemoveRepeatingCharacters(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            var sb = Pool.Get<StringBuilder>();

            try 
            {
                sb.Clear();

                for (int i = 0; i < str.Length; i++)
                {
                    var chr = str[i];
                    if ((i + 1 < str.Length) && str[i + 1] == chr) continue;
                    sb.Append(chr.ToString());
                }

                return sb.ToString();
            }
            finally { Pool.Free(ref sb); }

            
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _userUxData);
        }

        private UXData GetUserUXData(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            return _userUxData.TryGetValue(userId, out UXData data) ? data : _userUxData[userId] = new UXData();
        }

        private void SendRustPlusNotification(ulong userId, string titleMsg, string bodyMsg = "PRISM", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            NotificationList.SendNotificationTo(userId, channel, titleMsg, bodyMsg, Util.GetServerPairingData());
        }

        private BasePlayer FindPlayerByContainer(ItemContainer container, bool checkSleepers = false)
        {
            if (container == null) return null;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null || p.IsDead() || p?.inventory == null) continue;
                if (p.inventory.containerMain == container || p.inventory.containerWear == container || p.inventory.containerBelt == container) return p;
            }
            if (checkSleepers)
            {
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var p = BasePlayer.sleepingPlayerList[i];
                    if (p == null || p.IsDead() || p?.inventory == null) continue;
                    if (p.inventory.containerMain == container || p.inventory.containerWear == container || p.inventory.containerBelt == container) return p;
                }
            }
            return null;
        }

        private void ShowPopup(BasePlayer player, string msg, float duration = 5f)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));

            player.SendConsoleCommand("gametip.showgametip", msg);

            player.Invoke(() =>
            {
                if (player != null && player.IsConnected) player.SendConsoleCommand("gametip.hidegametip");
            }, duration);
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
            else if (span.TotalMinutes > 60)
            {
                var totalMinutesWithoutHours = (span.TotalMinutes - ((int)span.TotalHours * 60));
                str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + (totalMinutesWithoutHours <= 0 ? string.Empty : " " + totalMinutesWithoutHours.ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)");
            }
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
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

        private readonly Dictionary<string, Dictionary<string, float>> _lastCooldownMsgTime = new();

        private void SendCooldownMessage(BasePlayer player, string msg, float mustHaveWaited, string identifier = "")
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(msg))
                throw new ArgumentNullException(nameof(msg));

            if (mustHaveWaited < 0f)
                throw new ArgumentOutOfRangeException(nameof(mustHaveWaited));

            if (!player.IsConnected)
                return;

            var now = UnityEngine.Time.realtimeSinceStartup;

            var findVal = !string.IsNullOrWhiteSpace(identifier) ? identifier : msg;



            if (!_lastCooldownMsgTime.TryGetValue(player.UserIDString, out Dictionary<string, float> lastTimeDictionary))
                _lastCooldownMsgTime[player.UserIDString] = lastTimeDictionary = new Dictionary<string, float>();

            if (lastTimeDictionary.TryGetValue(findVal, out float lastTime) && (now - lastTime) < mustHaveWaited)
                return;


            lastTimeDictionary[findVal] = now;

            _lastCooldownMsgTime[player.UserIDString] = lastTimeDictionary;

            SendReply(player, msg);

        }

        private void BroadcastCooldownMessage(string msg, float mustHaveWaited, string identifier = "")
        {

            if (string.IsNullOrWhiteSpace(msg))
                throw new ArgumentNullException(nameof(msg));

            if (mustHaveWaited < 0f)
                throw new ArgumentOutOfRangeException(nameof(mustHaveWaited));

            var now = UnityEngine.Time.realtimeSinceStartup;

            var findVal = !string.IsNullOrWhiteSpace(identifier) ? identifier : msg;



            var fakeUserIdString = "GLOBAL";

            if (!_lastCooldownMsgTime.TryGetValue(fakeUserIdString, out Dictionary<string, float> lastTimeDictionary))
                _lastCooldownMsgTime[fakeUserIdString] = lastTimeDictionary = new Dictionary<string, float>();

            if (lastTimeDictionary.TryGetValue(findVal, out float lastTime) && (now - lastTime) < mustHaveWaited)
                return;


            lastTimeDictionary[findVal] = now;

            _lastCooldownMsgTime[fakeUserIdString] = lastTimeDictionary;

            SimpleBroadcast(msg);

        }

        private Item FindItemByIDU(ulong uID) //modified, less useful version of FindItemByIDU from Luck
        {
            if (uID == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(uID));
            }

            var itemId = new ItemId(uID);

            var allItems = Pool.GetList<Item>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead() || !player.IsConnected) continue;

                    player.inventory.AllItemsNoAlloc(ref allItems);

                    for (int j = 0; j < allItems.Count; j++)
                    {
                        if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;
                        var item = allItems?.Count >= j ? allItems[j] : null;
                        if (item?.uid == itemId) return item;
                    }

                    try
                    {
                        var lootContainers = player?.inventory?.loot?.containers ?? null;
                        if (lootContainers == null || lootContainers.Count < 1) continue;

                        var containerList = new List<ItemContainer>(lootContainers);

                        for (int j = 0; j < containerList.Count; j++)
                        {
                            if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;
                            var itemList = containerList?.Count >= j ? containerList[j]?.itemList ?? null : null;
                            if (itemList == null || itemList.Count < 1) continue;
                            var loopList = new List<Item>(itemList);
                            for (int k = 0; k < loopList.Count; k++)
                            {
                                if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;
                                var item = loopList?.Count >= k ? loopList[k] : null;
                                if (item?.uid == itemId) return item;
                            }
                        }

                    }
                    catch (Exception ex) { PrintError("Failed on containerList loops (activePlayerList): " + ex.ToString()); }

                }
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var player = BasePlayer.sleepingPlayerList[i];
                    if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) continue;

                    player.inventory.AllItemsNoAlloc(ref allItems);

                    for (int j = 0; j < allItems.Count; j++)
                    {
                        if (player == null || player.IsDestroyed || player.gameObject == null || player.inventory == null || player.IsDead()) break;
                        var item = allItems?.Count >= j ? allItems[j] : null;
                        if (item?.uid == itemId) return item;
                    }
                }
            }
            finally { Pool.FreeList(ref allItems); }

            foreach (var entry in BaseNetworkable.serverEntities)
            {
                if (entry == null) continue;
                var storage = entry as StorageContainer;

                Item item;
                if ((storage?.inventory?.itemList?.Count ?? 0) > 0)
                {
                    for (int i = 0; i < storage.inventory.itemList.Count; i++)
                    {
                        item = storage.inventory.itemList[i];
                        if (item?.uid == itemId)
                            return item;
                    }
                }

                var corpse = entry as LootableCorpse;
                if ((corpse?.containers?.Length ?? 0) > 0)
                {
                    for (int i = 0; i < corpse.containers.Length; i++)
                    {
                        var container = corpse.containers[i];
                        if (container?.itemList == null || container.itemList.Count < 1) continue;

                        for (int j = 0; j < container.itemList.Count; j++)
                        {
                            if (container?.itemList == null || container.itemList.Count < 1) break;
                            item = container.itemList[j];
                            if (item?.uid == itemId) return item;
                        }
                    }
                }


                item = (entry as DroppedItemContainer)?.inventory?.FindItemByUID(itemId);
                if (item != null)
                    return item;
            }

            return null;
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private bool IsValidPlayer(BasePlayer player) { return player != null && player.gameObject != null && !player.IsDestroyed && !player.IsNpc && player.prefabID == 4108440852; }

        #endregion
    }
}
