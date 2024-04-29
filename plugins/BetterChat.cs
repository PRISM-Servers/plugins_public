using System.Text.RegularExpressions;
using System.Collections.Generic;
using Oxide.Game.Rust.Libraries;
using Oxide.Core.Plugins;
using System.Linq;
using Oxide.Core;
using System;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using Oxide.Core.Libraries.Covalence;
using ConVar;
using Facepunch;
using Facepunch.Math;
using System.Globalization;
using CompanionServer;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Better Chat", "Shady", "3.7.1", ResourceId = 1520)] // Significant changes, so much so as to effectively make it its own plugin, were made.
    [Description("Customize chat colors, formatting, prefix and more.")]
    internal class BetterChat : RustPlugin
    {
        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private readonly Regex _ipPattern = new Regex(@"(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[1-9])(\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])){3}", RegexOptions.Compiled);

        private readonly Regex _emojiPattern = new Regex(@":[\w\.]+:", RegexOptions.Compiled); //new Regex(@":\w+:", RegexOptions.Compiled);

        private const int DISCORD_SOURCE_ID = -744;

        private const int RED_SOURCE_ID = 1;
        private const int BLUE_SOURCE_ID = 2;

        //originally LaserHydra's plugin, but so extremely modified by me that it was basically rewritten in 80% of places
        private readonly Command commands = Interface.Oxide.GetLibrary<Command>();
        private List<string> muted = new List<string>();

        [PluginReference]
        private readonly Plugin Ignore;

        [PluginReference]
        private readonly Plugin Clans;

        [PluginReference]
        private readonly Plugin ShadowBan;

        [PluginReference]
        private readonly Plugin Compilation;

        private const string RustbergWarning = "<color=green>Did you know?</color> <color=red>Rustberg is NOTORIOUS for FAKING their player count. Don't just take our word for it, ask for video proof on our Discord here:</color> <color=yellow>discord.gg/DUCnZhZ</color>";

        private string RemoveRepeatingCharacters(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                var chr = str[i];
                if ((i + 1 < str.Length) && str[i + 1] == chr) continue;
                sb.Append(chr.ToString());
            }
            return sb.ToString();
        }


        private void Loaded()
        {
            LoadData();
            LoadConfig();
            LoadMessages();

            RegisterPerm("mute");
            RegisterPerm("formatting");
            permission.RegisterPermission("betterchat.noavatar", this);

            foreach (var kvp in Config)
            {
                string group = kvp.Key;
                if (group == "Mute" || group == "WordFilter" || group == "AntiSpam") continue;


                RegisterPerm(GetConfig(group, group, "Permission"));
                //BroadcastChat($"--> <color=red>Registered permission '{PermissionPrefix}.{GetConfig(group, group, "Permission")}'</color>");

                if (!permission.GroupExists(group))
                {
                    permission.CreateGroup(group, GetConfig("[Player]", group, "Title"), GetConfig(1, group, "Rank"));
                    //BroadcastChat($"--> <color=red>Created group '{group}' as it did not exist</color>");
                }

                permission.GrantGroupPermission(group, $"{PermissionPrefix}.{GetConfig(group, group, "Permission")}", this);
                //BroadcastChat($"--> <color=red>Granted permission '{PermissionPrefix}.{GetConfig(group, group, "Permission")}' to group '{group}'</color>");
            }

            if (GetConfig(true, "Mute", "Enabled"))
            {
                commands.AddChatCommand("mute", this, "cmdMute");
                commands.AddChatCommand("unmute", this, "cmdUnmute");
            }
            //    ignoreAPI = plugins.Find("Ignore");
            if (!plugins.Exists("Ignore"))
            {
                PrintWarning("Failed to load IgnoreAPI");
            }
            // if (ignoreAPI != null)
            // {
            //   Puts("Loaded Ignore API successfully");
            // }
            //   else PrintWarning("failed to load ignore api");
            timer.Every(120f, () =>
            {
                ipSpamCount?.Clear();
            });
        }

        private void LoadData()
        {
            muted = Interface.Oxide.DataFileSystem.ReadObject<List<string>>("BetterChat_Muted");
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("BetterChat_Muted", muted);
        }

        private void LoadMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                { "No Permission", "You don't have permission to use this command." },
                { "Muted Broadcast", "{player} was muted!" },
                { "Unmuted Broadcast", "{player} was unmuted!" },
                { "Muted", "You are muted." },
            }, this);
        }

        private void LoadConfig()
        {
            SetConfig("WordFilter", "Enabled", false);
            SetConfig("WordFilter", "FilterList", new List<string> { "fuck", "bitch", "faggot" });
            SetConfig("WordFilter", "UseCustomReplacement", false);
            SetConfig("WordFilter", "CustomReplacement", "Unicorn");

            SetConfig("AntiSpam", "Enabled", false);
            SetConfig("AntiSpam", "MaxCharacters", 85);

            SetConfig("Mute", "Enabled", true);

            SetConfig("player", new Dictionary<string, object> {
                { "Formatting", "{Title} {Name}<color={TextColor}>:</color> {Message}" },
                { "ConsoleFormatting", "{Title} {Name}: {Message}" },
                { "Permission", "player" },
                { "Title", "[Player]" },
                { "TitleColor", "#C4FF00" },
                { "NameColor", "#DCFF66" },
                { "TextColor", "white" },
                { "Rank", 1 }
            });

            SetConfig("moderator", new Dictionary<string, object> {
                { "Formatting", "{Title} {Name}<color={TextColor}>:</color> {Message}" },
                { "ConsoleFormatting", "{Title} {Name}: {Message}" },
                { "Permission", "moderator" },
                { "Title", "[Mod]" },
                { "TitleColor", "yellow" },
                { "NameColor", "#DCFF66" },
                { "TextColor", "white" },
                { "Rank", 2 }
            });

            SetConfig("admin", new Dictionary<string, object> {
                { "Formatting", "{Title} {Name}<color={TextColor}>:</color> {Message}" },
                { "ConsoleFormatting", "{Title} {Name}: {Message}" },
                { "Permission", "admin" },
                { "Title", "[Admin]" },
                { "TitleColor", "red" },
                { "NameColor", "#DCFF66" },
                { "TextColor", "white" },
                { "Rank", 3 }
            });

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Generating new config file...");
        }

        ////////////////////////////////////////
        ///  BetterChat API
        ////////////////////////////////////////

        private Dictionary<string, object> GetPlayerFormatting(string UserIDString)
        {
            string uid = UserIDString;

            Dictionary<string, object> playerData = new Dictionary<string, object>();

            playerData["GroupRank"] = 0;
            playerData["Formatting"] = GetConfig("{Title} {Name}<color={TextColor}>:</color> {Message}", "player", "Formatting");
            playerData["ConsoleFormatting"] = GetConfig("{Title} {Name}: {Message}", "player", "ConsoleFormatting");
            playerData["GroupRank"] = GetConfig(1, "player", "Rank");
            playerData["TitleColor"] = GetConfig("#C4FF00", "player", "TitleColor");
            playerData["NameColor"] = GetConfig("#DCFF66", "player", "NameColor");
            playerData["TextColor"] = GetConfig("white", "player", "TextColor");

            Dictionary<string, string> titles = new Dictionary<string, string>();
            titles.Add(GetConfig("[Player]", "player", "Title"), GetConfig("#C4FF00", "player", "TitleColor"));

            foreach (var group in Config)
            {
                //BroadcastChat($"--> <color=red>Current group '{group.Key}'</color>");
                string groupName = group.Key;

                if (groupName == "Mute" || groupName == "WordFilter" || groupName == "AntiSpam") continue;

                if (HasPerm(UserIDString, GetConfig(groupName, groupName, "Permission")))
                {
                    //BroadcastChat($"--> <color=red>Has permission for group '{group.Key}'</color>");
                    if (Convert.ToInt32(Config[groupName, "Rank"].ToString()) > Convert.ToInt32(playerData["GroupRank"].ToString()) || groupName == "vip")
                    {
                        playerData["Formatting"] = GetConfig("{Title} {Name}<color={TextColor}>:</color> {Message}", groupName, "Formatting");
                        playerData["ConsoleFormatting"] = GetConfig("{Title} {Name}: {Message}", groupName, "ConsoleFormatting");
                        playerData["GroupRank"] = GetConfig(1, groupName, "Rank");
                        playerData["TitleColor"] = GetConfig("#C4FF00", groupName, "TitleColor");
                        playerData["NameColor"] = GetConfig("#DCFF66", groupName, "NameColor");
                        playerData["TextColor"] = GetConfig("white", groupName, "TextColor");
                    }
                    string outTitle;
                    if (!titles.TryGetValue(GetConfig("[Player]", groupName, "Title"), out outTitle)) titles[GetConfig("[Player]", groupName, "Title")] = GetConfig("#C4FF00", groupName, "TitleColor");
                }
            }


            if (titles.Count > 1 && titles.ContainsKey(GetConfig("[Player]", "player", "Title")))
                titles.Remove(GetConfig("[Player]", "player", "Title"));

            playerData["Titles"] = titles;

            return playerData;
        }

        private string GetPermissionByTitle(string titleName)
        {
            if (string.IsNullOrEmpty(titleName)) return string.Empty;
            Dictionary<string, object> goodDict = null;
            foreach (var kvp in Config)
            {
                if (kvp.Value is Dictionary<string, object>)
                {
                    var valDict = kvp.Value as Dictionary<string, object>;
                    object getVal = null;
                    foreach (var kvp2 in valDict)
                    {
                        if (kvp2.Key == "Title")
                        {
                            getVal = kvp2.Value;
                            break;
                        }
                    }
                    // var getVal = valDict?.Where(p => p.Key == "Title")?.FirstOrDefault().Value;
                    if (getVal == null) continue;
                    if (getVal.ToString() == titleName)
                    {
                        goodDict = valDict;
                        break;
                    }
                    //         else PrintWarning("Not equal: " + getVal.ToString() + ", " + titleName);
                    //    PrintWarning("getVal: " + getVal?.GetType() + ", " + getVal);
                }
            }
            if (goodDict == null || goodDict.Count < 1)
            {
                PrintWarning("Good dict is not good: " + titleName);
                return null;
            }
            object perm;
            if (goodDict.TryGetValue("Permission", out perm)) return perm.ToString();
            else
            {
                PrintWarning("No get perm from gooddict, " + titleName);
                return string.Empty;
            }
        }

        private List<string> GetGroups()
        {
            List<string> groups = new List<string>();
            foreach (var group in Config)
            {
                groups.Add(group.Key);
            }

            return groups;
        }

        private Dictionary<string, object> GetGroup(string name)
        {
            Dictionary<string, object> group = new Dictionary<string, object>();

            GetConfig(new Dictionary<string, object> {
                { "Formatting", "{Title} {Name}<color={TextColor}>:</color> {Message}" },
                { "ConsoleFormatting", "{Title} {Name}: {Message}" },
                { "Permission", "player" },
                { "Title", "[Player]" },
                { "TitleColor", "#C4FF00" },
                { "NameColor", "#C4FF00" },
                { "TextColor", "white" },
                { "Rank", 1 }
            }, name);

            return group;
        }

        private List<string> GetPlayersGroups(BasePlayer player)
        {
            List<string> groups = new List<string>();
            foreach (var group in Config)
            {
                if (HasPerm(player.UserIDString, GetConfig("player", group.Key, "Permission")))
                    groups.Add(group.Key);
            }

            return null;
        }

        private bool GroupExists(string name)
        {
            if (Config[name] == null)
                return false;
            else
                return true;
        }

        private bool AddPlayerToGroup(BasePlayer player, string name)
        {
            if (GetConfig("player", name, "Permission") != null && !HasPerm(player.UserIDString, GetConfig("player", name, "Permission")))
            {
                permission.GrantUserPermission(player.UserIDString, GetConfig("player", name, "Permission"), this);
                return true;
            }

            return false;
        }

        private bool RemovePlayerFromGroup(BasePlayer player, string name)
        {
            if (GetConfig("player", name, "Permission") != null && HasPerm(player.UserIDString, GetConfig("player", name, "Permission")))
            {
                permission.RevokeUserPermission(player.UserIDString, GetConfig("player", name, "Permission"));
                return true;
            }

            return false;
        }

        private bool PlayerInGroup(BasePlayer player, string name)
        {
            if (GetPlayersGroups(player).Contains(name))
                return true;

            return false;
        }

        private bool AddGroup(string name, Dictionary<string, object> group)
        {
            try
            {
                if (!group.ContainsKey("ConsoleFormatting"))
                    group["ConsoleFormatting"] = "{Title} {Name}: {Message}";

                if (!group.ContainsKey("Formatting"))
                    group["Formatting"] = "{Title} {Name}<color={TextColor}>:</color> {Message}";

                if (!group.ContainsKey("NameColor"))
                    group["NameColor"] = "#DCFF66";

                if (!group.ContainsKey("Permission"))
                    group["Permission"] = "color_none";

                if (!group.ContainsKey("Rank"))
                    group["Rank"] = 1;

                if (!group.ContainsKey("TextColor"))
                    group["TextColor"] = "white";

                if (!group.ContainsKey("Title"))
                    group["Title"] = "[None]";

                if (!group.ContainsKey("TitleColor"))
                    group["TitleColor"] = "#C4FF00";

                if (Config[name] == null)
                    Config[name] = group;
                else
                    return false;

                SaveConfig();

                return true;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return false;
        }

        ////////////////////////////////////////
        ///  Chat Related
        ////////////////////////////////////////

        private string GetFilteredMesssage(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return msg;
            var wordList = Config["WorldFilter", "FilterList"] as List<object>;
            var newmsgSB = new StringBuilder(msg);
            for (int i = 0; i < wordList.Count; i++)
            {
                var word = wordList[i];
                MatchCollection matches = new Regex($@"((?:\S+)?{word}(?:\S+)?)").Matches(msg);
                for (int j = 0; j < matches.Count; j++)
                {
                    var match = matches[j];
                    if (match.Success)
                    {
                        string found = match.Groups[1].ToString();
                        var replacedSB = new StringBuilder();
                        //     string replaced = "";

                        if (GetConfig(false, "WordFilter", "UseCustomReplacement")) newmsgSB.Replace(found, GetConfig("Unicorn", "WordFilter", "CustomReplacement"));
                        else
                        {
                            for (int k = 0; k < found.Length; k++) replacedSB.Append("*");

                            newmsgSB.Replace(found, replacedSB.ToString());
                        }
                    }
                    else break;
                }
            }
            return newmsgSB.ToString();
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

        private void MutePlayer(BasePlayer player)
        {
            if (!muted.Contains(player.UserIDString))
                muted.Add(player.UserIDString);

            SaveData();
        }

        private void UnmutePlayer(BasePlayer player)
        {
            if (muted.Contains(player.UserIDString))
                muted.Remove(player.UserIDString);

            SaveData();
        }

        private bool IsMuted(BasePlayer player)
        {
            if (muted.Contains(player.UserIDString) && GetConfig(true, "Mute", "Enabled"))
                return true;

            var ChatMute = plugins.Find("chatmute");

            if (ChatMute != null)
            {
                bool isMuted = (bool)ChatMute.Call("IsMuted", player);

                if (isMuted)
                    return true;
            }

            if (player.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute))
                return true;

            return false;
        }

        private bool HasIP(string value) { return _ipPattern.IsMatch(value); } //change to check matches
        

        [ChatCommand("colors")]
        private void ColorList(BasePlayer player)
        {
            List<string> colorList = new List<string> { "aqua", "black", "blue", "brown", "darkblue", "green", "grey", "lightblue", "lime", "magenta", "maroon", "navy", "olive", "orange", "purple", "red", "silver", "teal", "white", "yellow" };
            colorList = (from color in colorList select $"<color={color}>{color.ToUpper()}</color>").ToList();

            SendChatMessage(player, "<b><size=20>Available colors:</size></b><size=15>\n " + ListToString(colorList, 0, ", ") + "</size>");
        }

        [ChatCommand("bchooktest")]
        private void BCHookTest(BasePlayer player)
        {
            Interface.Oxide.CallHook("OnBetterChat", player.UserIDString, "this is a test");
            Interface.Oxide.CallHook("OnBetterChatClean", player.UserIDString, "this is a clean test");
        }

        [ChatCommand("coltest")]
        private void cmdPoss(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 2)
            {
                return;
            }
            var findPlayer = FindPlayerByPartialName(args[0]);
            if (findPlayer == null) return;
            var getPerm = GetPermissionByTitle(args[1]);
            PrintWarning(getPerm);
        }

        //public string ValidCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=[]{}\|\',./`*!@#$%^&*()_+<>? "; //extra space at end is to allow space as a valid character

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
            Pool.FreeList(ref matches);
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
            var matches = Pool.GetList<BasePlayer>();
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
            Pool.FreeList(ref matches);
            return player;
        }

        private Dictionary<string, string> GetPlayerTitles(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            object outObj;
            var data = GetPlayerFormatting(userId);
            if (data == null || data.Count < 1) return null;
            if (data.TryGetValue("Titles", out outObj) && outObj != null) return (outObj as Dictionary<string, string>);
            return null;
        }

        private readonly Dictionary<string, float> lastMessage = new Dictionary<string, float>();
        private readonly Dictionary<string, int> ipSpamCount = new Dictionary<string, int>();
        private static string ReplaceCaseInsensitive(string input, string search, string replacement)
        {
            string result = Regex.Replace(
                input,
                Regex.Escape(search),
                replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
            return result;
        }

        private void SendReply(IPlayer player, string msg, bool keepTagsConsole = false)
        {
            if (player == null) return;
            msg = !player.IsServer ? msg : (keepTagsConsole) ? msg : RemoveTags(msg);
            if (player.IsServer) ConsoleSystem.CurrentArgs.ReplyWith(msg);
            else player.Reply(msg);
        }

        private void SendChatID(string userId, string message, bool callFinalHook = true, int source = 0)
        {
            var ply = covalence.Players?.FindPlayerById(userId) ?? null;
            if (ply != null) SendChat(ply, message, callFinalHook);
        }

        private float GetMessageCooldownTime(IPlayer player)
        {
            if (player == null) return -1f;
            if (player.IsServer || player.IsAdmin) return 0f;

            var val = 1.2f;

            int spamCount;
            if (repeatedSpamCount.TryGetValue(player.Id, out spamCount))
            {
                val += (spamCount * 0.725f);
            }

            return val;
        }

        private readonly Dictionary<string, int> repeatedSpamCount = new Dictionary<string, int>();
        private readonly Dictionary<string, string> lastMessageContent = new Dictionary<string, string>();
        private bool HasAdvertisingOrSpam(string message)
        {
            if (string.IsNullOrEmpty(message)) return false;

            if (HasIP(message) && !message.Equals(covalence.Server.Address.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                PrintWarning("message detected as having IP that isn't ours: " + message);
                return true;
            }

            var spaceSplit = message.Split(' ');

            for (int i = 0; i < spaceSplit.Length; i++)
            {
                var split = spaceSplit[i];
                if (!HasIP(split) || split.Equals(covalence.Server.Address.ToString(), StringComparison.OrdinalIgnoreCase))
                    continue;


                PrintWarning("message detected as having IP that isn't ours (2): " + message);
                return true;
            }

            var lowerMsg = message.ToLower();
            var noSpaceNoRepeat = RemoveRepeatingCharacters(lowerMsg.Replace(" ", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty));

            var noSpace = lowerMsg.Replace(" ", string.Empty);
            if (!noSpaceNoRepeat.Contains("kngsgaming") && !noSpaceNoRepeat.Contains("rust.kn") && !noSpaceNoRepeat.Contains("prism"))
            {
                if ((noSpaceNoRepeat.Contains("connect") && noSpaceNoRepeat.Contains(":280")) || noSpaceNoRepeat.Contains("28015") || lowerMsg.Contains("client.connect") || noSpaceNoRepeat.Contains("rusticaland") || noSpaceNoRepeat.Contains("rustica") || noSpaceNoRepeat.Contains("rustland") || lowerMsg.Contains("rustberg") || lowerMsg.Contains("rustburg") || lowerMsg.Contains("rusticaland") || lowerMsg.Contains("rusticland") || noSpace.Contains("28015") || noSpace.Contains("client.connect") || noSpace.Contains("rusticaland") || noSpaceNoRepeat.Contains("rustica") || noSpaceNoRepeat.Contains("rustland") || noSpaceNoRepeat.Contains("rustlands") || noSpaceNoRepeat.Contains("rusticland") || noSpaceNoRepeat.Contains("virginrust"))
                {
                    return true;
                }
            }

            return false;
        }

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private void SendChatByNameAndId(string userId, string displayName, string message, bool callFinalHook = true, int source = 0)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrWhiteSpace(message))
                return;

          //  if (source != 0)
        //        PrintWarning(nameof(SendChatByNameAndId) + " : " + userId + " : " + message + " : " + callFinalHook + " " + source);

            var watch = Pool.Get<Stopwatch>();

         

            try
            {
                watch.Restart(); //we really have to be careful with these pooled watches - i keep writing them without restarting them after grabbing pooled


                //player is often going to be null, because most players will have only ever connected to ONE server, this method is used by both in cross chat
                var player = covalence?.Players?.FindPlayerById(userId);

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = player?.Name ?? userId;

                    if (string.IsNullOrWhiteSpace(displayName))
                        PrintError("covalence name is null after initial displayName was null!!!!, userId: " + userId);

                }

                //commenting out as i think the SB below handles this fine
                //message = message.TrimStart(null).TrimEnd(null);

                _stringBuilder.Clear();

                var validChr = GetValidCharacters();

                for (int i = 0; i < message.Length; i++)
                {
                    var chr = message[i];

                    if (char.IsLetterOrDigit(chr) || validChr.Contains(char.ToUpperInvariant(chr))) _stringBuilder.Append(chr);
                }

                if (_stringBuilder.Length < 1)
                    return;

              //  if (source != 0)
                //    PrintWarning("stringbuilder length after message length check: " + _stringBuilder.Length + " vs message length: " + message.Length);

                message = _stringBuilder.ToString();

            //    if (source != 0)
                //    PrintWarning("new message: " + message);

                _stringBuilder.Clear();

              

                var isConnected = player?.IsConnected ?? false;

                //handle &, ! and ? as if it were / (command)
                if (isConnected && message.Length > 1 && message[1] != ' ' && (message.StartsWith("&", StringComparison.OrdinalIgnoreCase) || message.StartsWith("?", StringComparison.OrdinalIgnoreCase) || message.StartsWith("!", StringComparison.OrdinalIgnoreCase)) && message[1] != message[0])
                {

                    var newSB = _stringBuilder.Clear().Append("/");
                    var first = true;

                    for (int i = 0; i < message.Length; i++)
                    {
                        var c = message[i];

                        if (first) first = false;
                        else newSB.Append(c);
                    }

                    if (newSB.Length < 1)
                        PrintError("newSB length <1?!");

                    var pObj = player?.Object as BasePlayer;
                    if (pObj != null)
                        Interface.CallHook("IOnPlayerCommand", pObj, newSB.ToString());

                    return;
                }

                float lastTime;
                if (!lastMessage.TryGetValue(userId, out lastTime)) lastTime = 0f;
                var msgTimeDiff = lastTime > 0 ? UnityEngine.Time.realtimeSinceStartup - lastTime : -1;

                string lastMsgTxt;
                var spamCount = 0;

                if (lastTime > 0 && msgTimeDiff < 25 && lastMessageContent.TryGetValue(userId, out lastMsgTxt) && message.Equals(lastMsgTxt, StringComparison.OrdinalIgnoreCase))
                {
                    if (!repeatedSpamCount.TryGetValue(userId, out spamCount)) repeatedSpamCount[userId] = 1;
                    else repeatedSpamCount[userId] += 1;
                }

                if (spamCount > 2 && message.Contains("server", CompareOptions.OrdinalIgnoreCase))
                {
                    covalence.Server.Command("ban \"" + userId + "\" \"Advertising (automatic)\"");
                    return;
                }

                if (player != null && lastTime > 0 && msgTimeDiff < GetMessageCooldownTime(player))
                {
                    if (isConnected) 
                        SendReply(player, "You are chatting too fast!");

                    return;
                }


                var lowerMsg = message.ToLower(CultureInfo.CurrentCulture);

                var pName = RemoveTags(displayName);

                var clanName = Clans?.Call<string>("GetClanOf", userId) ?? string.Empty;
                if (!string.IsNullOrEmpty(clanName)) pName = _stringBuilder.Clear().Append("[").Append(clanName).Append("] ").Append(pName).ToString();


                var capsCount = 0;
                if (message.Length >= 10)
                {
                    for (int i = 0; i < message.Length; i++)
                    {
                        var chr = message[i];
                        if (!char.IsNumber(chr) && char.IsUpper(chr)) 
                            capsCount++;
                    }
                }

                if (capsCount > 0 && capsCount > (message.Length * 0.67))
                    message = lowerMsg;
                

                message = RemoveTags(message);


                //	Getting Data
                var playerData = GetPlayerFormatting(userId);
                if (playerData == null || playerData.Count < 1)
                {
                    PrintError("Bad playerData!! player: " + userId);
                    return;
                }

                var titles = playerData["Titles"] as Dictionary<string, string>;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///		Chat Output	
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                playerData["FormattedOutput"] = playerData["Formatting"];
                var bestGroup = Compilation?.Call<string>("GetBestGroup", userId) ?? string.Empty;
                var getTag = Compilation?.Call<string>("GetTagColor", userId, bestGroup) ?? string.Empty;
                var titleColor = !string.IsNullOrEmpty(getTag) ? getTag : (playerData["TitleColor"]).ToString();
                var nameColor = !string.IsNullOrEmpty(getTag) ? getTag : (playerData["NameColor"]).ToString();

                var formatNameSb = _stringBuilder.Clear().Append("<color=").Append(nameColor).Append(">").Append(pName).Append("</color>");

                if (source != 0)
                {
                    var tag = source switch
                    {
                        DISCORD_SOURCE_ID => "<color=#5662F6>via /Discord</color></size> ]</size>",
                        RED_SOURCE_ID => "<color=#cc0000><i>RED</i></color></size> ]</size>",
                        BLUE_SOURCE_ID => "<color=#33ccff><i>BLUE</i></color></size> ]</size>",
                        _ => string.Empty,
                    };
                    if (!string.IsNullOrWhiteSpace(tag)) 
                        formatNameSb.Append(" <size=11>[ <size=10>").Append(tag);

                }

                var formattedName = formatNameSb.ToString();

                var textColor = isConnected ? playerData["TextColor"].ToString() : "#b3b3b3"; //grey for Discord, cross-chat messages

                var playerFormatStr = string.Empty;

                var playerFormatSB = Pool.Get<StringBuilder>();
                try 
                {
                    playerFormatSB.Clear().Append(playerData["FormattedOutput"].ToString());
                    playerFormatSB.Replace("{Rank}", playerData["GroupRank"].ToString());
                    playerFormatSB.Replace("{TitleColor}", ((titleColor == getTag) ? string.Empty : titleColor));
                    playerFormatSB.Replace("{NameColor}", playerData["NameColor"].ToString());



                    playerFormatSB.Replace("{TextColor}", textColor);

                    playerFormatSB.Replace("{Name}", formattedName);
                    playerFormatSB.Replace("{ID}", userId);
                    playerFormatSB.Replace("{Message}", _stringBuilder.Clear().Append("<color=").Append(textColor).Append(">").Append(message).Append("</color>").ToString());
                    playerFormatSB.Replace("{Time}", string.Empty);


                    if (message.StartsWith(">") || (message.Length > 1 && message[1] == '>')) //that isn't a blank space
                    {
                        playerFormatSB.Replace("<color=white>", "<color=#789922>");
                    }


                    if (playerFormatSB.Length < 1)
                        PrintError(nameof(playerFormatSB) + " Length < 1?!?!");

                    playerFormatStr = playerFormatSB.ToString();

                }
                finally { Pool.Free(ref playerFormatSB); }

           
                playerData["FormattedOutput"] = playerFormatStr;

                var chatTitleSB = Pool.Get<StringBuilder>();
                var customColor = string.Empty;
                try 
                {

                    chatTitleSB.Clear();

                    if (isConnected)
                    {
                        foreach (var kvp in titles)
                        {
                            var perm = GetPermissionByTitle(kvp.Key);
                            if (string.IsNullOrEmpty(perm)) continue;

                            var color = kvp.Value;
                            if (!string.IsNullOrEmpty(perm))
                            {
                                var getColor = Compilation?.Call<string>("GetTagColor", userId, perm);
                                if (!string.IsNullOrEmpty(getColor)) color = getColor;
                            }

                            if (string.IsNullOrEmpty(color))
                            {
                                PrintWarning("null or empty color: " + kvp.Key);
                                continue;
                            }
                            customColor = color;
                            chatTitleSB.Append("<color=").Append(color).Append(">").Append(kvp.Key).Append("</color>");
                            //break;
                        }
                    }

               
                    var chatTitle = chatTitleSB.Length < 1 ? string.Empty : chatTitleSB.ToString().TrimEnd(' ');

                    var titleHookVal = Interface.Oxide.CallHook("OnBetterChatGetTitle", userId, chatTitle);
                    if (titleHookVal != null)
                        chatTitle = titleHookVal.ToString();

                    playerData["FormattedOutput"] = playerFormatStr.Replace("{Title}", chatTitle);
                }
                finally { Pool.Free(ref chatTitleSB); }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///		Sending
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                var strOutput = playerData["FormattedOutput"].ToString();
                var canMsgObj = Interface.CallHook("CanBetterChat", userId, strOutput, false);
                if (canMsgObj != null)
                {
                    var str = canMsgObj as string;
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (isConnected)
                            SendReply(player, str);

                        return;
                    }
                    else if (!(bool)canMsgObj) return;
                }

                var isAdmin = player?.IsAdmin ?? false;
                if (Ignore != null && Ignore.IsLoaded)
                {
                    var spaceSplit = message.Split(' ');
                    var basePly = player?.Object as BasePlayer;

                    var userIdU = basePly?.userID ?? 0;

                    //todo: use SB in ReplaceCaseInsensitive below
                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var target = BasePlayer.activePlayerList[i];
                        if (target == null || !target.IsConnected) 
                            continue;

                        var isIgnoring = !isAdmin && (Ignore?.Call<bool>("HasIgnored", target.userID, userIdU) ?? false);
                        if (isIgnoring) 
                            continue;

                        var customOutput = strOutput;

                        if (target.UserIDString != userId)
                        {
                            if (customOutput.IndexOf(target.displayName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                customOutput = ReplaceCaseInsensitive(customOutput, target.displayName, "<color=" + "#F59B0A" + ">" + target.displayName + "</color>");
                                //  customOutput = Regex.Replace(customOutput, target.displayName, "<color=" + "#F59B0A" + ">" + target.displayName + "</color>", RegexOptions.IgnoreCase);
                            }

                            var targetClanName = Clans?.Call<string>("GetClanOf", target.UserIDString);
                            if (!string.IsNullOrEmpty(targetClanName) && customOutput.Replace(pName, string.Empty, StringComparison.OrdinalIgnoreCase).IndexOf(targetClanName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                customOutput = ReplaceCaseInsensitive(customOutput, targetClanName, "<color=" + "#3dbbff" + ">" + targetClanName + "</color>");
                            }
                        }


                        if (isAdmin && customOutput.IndexOf("@everyone", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            customOutput = ReplaceCaseInsensitive(customOutput, "@everyone", "<color=" + "#F59B0A" + ">@everyone</color>");
                            for (int j = 0; j < 3; j++)
                            {
                                if (target == null || !target.IsConnected) 
                                    break;

                                SendLocalEffect(target, "assets/bundled/prefabs/fx/invite_notice.prefab", target.eyes.position);
                            }
                        }

                        SendMessageToPlayer(target, formattedName, strOutput, userId, nameColor);
                    }
                }
                else BroadcastPlayerMessage(formattedName, strOutput, userId, nameColor);

                lastMessage[userId] = UnityEngine.Time.realtimeSinceStartup;

                var chatEntry = new Chat.ChatEntry()
                {
                    Channel = Chat.ChatChannel.Global,
                    Message = message,
                    UserId = userId,
                    Username = pName,
                    Color = customColor,
                    Time = Epoch.Current
                };

                Chat.Record(chatEntry);

                Interface.CallHook("OnBetterChat", userId, strOutput);

                if (player != null)
                    Interface.CallHook("OnBetterChatPly", player, strOutput);

                if (callFinalHook)
                {
                    Interface.CallHook("OnBetterChatClean", userId, message);

                    if (player != null)
                        Interface.CallHook("OnBetterChatCleanPly", player, message);
                }


                Puts(RemoveTags(strOutput));
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + " Exception on " + nameof(SendChatByNameAndId)); }
            finally
            {
                try { if (watch.Elapsed.TotalMilliseconds >= 8) PrintWarning(nameof(SendChatByNameAndId) + " took: " + watch.Elapsed.TotalMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }
        }

        private void SendChat(IPlayer player, string message, bool callFinalHook = true, int source = 0)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            SendChatByNameAndId(player.Id, player?.Name, message, callFinalHook, source);
        }

        private void SendTeamMessage(RelationshipManager.PlayerTeam team, string message, string senderId = "")
        {
            if (team == null || string.IsNullOrEmpty(message)) return;

            for (int i = 0; i < team.members.Count; i++)
            {
                var p = RelationshipManager.FindByID(team.members[i]);
                if (p == null || !p.IsConnected)
                    continue;

                if (string.IsNullOrWhiteSpace(senderId))
                    SendReply(p, message);
                else
                    p.Command("chat.add", string.Empty, senderId, message);
            }
        }



        private object OnPlayerChat(BasePlayer player, string message, Chat.ChatChannel channel)
        {
            if (player == null) return null;
            if (string.IsNullOrEmpty(message) || message.StartsWith("/", StringComparison.OrdinalIgnoreCase)) return null;

            var uid = player?.UserIDString ?? string.Empty;

            var ply = player?.IPlayer;
            if (ply == null)
            {
                PrintWarning("covalence player is null for uid: " + uid + " on OnPlayerChat?!");
                return null;
            }

            if (channel == Chat.ChatChannel.Team) //intercept team messages
            {
                var pTeam = player?.Team;
                if (pTeam == null)
                {
                    PrintWarning("Channel > 0 but player team null! Player: " + player?.displayName + " (" + player?.UserIDString + ")");
                    player.SendConsoleCommand("relationshipmanager.leaveteam");
                    player.currentTeam = 0;
                    player.TeamUpdate();
                }
                var canMsgObj = Interface.CallHook("CanBetterChat", uid, message, true);
                if (canMsgObj != null)
                {
                    var str = canMsgObj as string;
                    if (!string.IsNullOrEmpty(str)) SendReply(player, str);
                    PrintWarning("canMsgObj returned false for: " + uid + ", " + message + " " + channel);

                    return ((bool)canMsgObj) ? null : (object)true;
                }
                var lowerNoSpace = message.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                // var preNo = lowerNoSpace;
                lowerNoSpace = RemoveRepeatingCharacters(lowerNoSpace);
                //   PrintWarning("pre: " + preNo + " post: " + lowerNoSpace);
                if (lowerNoSpace.Contains("rustberg") || lowerNoSpace.Contains("rustburg"))
                {
                    PrintWarning("Rustberg for message: " + message + " from user: " + player?.displayName + " (" + player?.UserIDString + ")");
                    SendReply(player, RustbergWarning);
                    return true;
                }
                if (HasAdvertisingOrSpam(message))
                {
                    PrintWarning("HasAdvertisingOrSpam for message: " + message + " from user: " + player?.displayName + " (" + player?.UserIDString + ")");
                    SendReply(player, "Believe it or not, we don't allow advertising or spam even in team chat. This has been logged, and repeated offenses may result in punishment.");
                    return true;
                }
                if (pTeam == null)
                {
                    SendChat(ply, message);
                    return true;
                }
                var msg = message;
                var clanTag = Clans?.Call<string>("GetClanOf", player.UserIDString) ?? string.Empty;

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    var prefix = !string.IsNullOrEmpty(clanTag) ? (sb.Clear().Append("[Team Chat (").Append(clanTag).Append(")]").ToString()) : "[Team Chat]";

                    ServerConsole.PrintColoured(ConsoleColor.DarkYellow, prefix, ConsoleColor.DarkGreen, sb.Clear().Append(" ").Append(player.displayName).Append(": ").Append(msg).ToString());

                    var playerNameColor = "#33e1f5";

                    var hookVal = Interface.Oxide.CallHook("OnTeamChatGetPlayerNameColor", player, playerNameColor);
                    if (hookVal != null)
                        playerNameColor = hookVal.ToString();

                    SendTeamMessage(pTeam, sb.Clear().Append("[<color=#53fa39>Team</color>").Append((pTeam.teamLeader == player.userID ? "<color=#d9ecff> <size=11>*</size></color>" : string.Empty)).Append("] <color=").Append(playerNameColor).Append(">").Append(player.displayName).Append("</color>: <color=#ff3d67>").Append(RemoveTags(msg)).Append("</color>").ToString(), player.UserIDString);

                    Chat.ChatEntry chatEntry = new Chat.ChatEntry()
                    {
                        Channel = Chat.ChatChannel.Team,
                        Message = msg,
                        UserId = player.UserIDString,
                        Username = sb.Clear().Append("[").Append(clanTag).Append("] ").Append(player.displayName).ToString(),
                        Time = Epoch.Current
                    };

                    RCon.Broadcast(RCon.LogType.Chat, chatEntry);

                    pTeam?.BroadcastTeamChat(player.userID, player.displayName, msg, string.Empty);
                }
                finally { Pool.Free(ref sb); }

                return true;
            }

            SendChat(ply, message);
            return true;
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


        ////////////////////////////////////////
        ///     Converting
        ////////////////////////////////////////

        private string ListToString(List<string> list, int first, string seperator)
        {
            return String.Join(seperator, list.Skip(first).ToArray());
        }

        private string ArrayToString(string[] array, int first, string seperator)
        {
            return String.Join(seperator, array.Skip(first).ToArray());
        }

        private string FirstUpper(string original)
        {
            if (string.IsNullOrEmpty(original)) return original;


            List<string> output = new List<string>();
            var split = original.Split(' ');
            for (int i = 0; i < split.Length; i++)
            {
                var word = split[i];
                output.Add(word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1));
            }


            return ListToString(output, 0, " ");
        }
        /*/
        private string ReplaceCaseInsensitive(string original, string toReplace, string replaceWith)
        {
            if (string.IsNullOrEmpty(original)) throw new ArgumentNullException(nameof(original));

            var sb = new StringBuilder(original);


            return string.Empty;
        }/*/

        ////////////////////////////////////////
        ///     Config & Message Related
        ////////////////////////////////////////

        private void SetConfig(params object[] args)
        {
            List<string> stringArgs = (from arg in args select arg.ToString()).ToList();
            stringArgs.RemoveAt(args.Length - 1);

            if (Config.Get(stringArgs.ToArray()) == null) Config.Set(args);
        }

        private T GetConfig<T>(T defaultVal, params object[] args)
        {
            var stringArgs = args.Select(p => p.ToString())?.ToArray() ?? null;
            if (stringArgs == null || stringArgs.Length < 1) return defaultVal;
            //  List<string> stringArgs = (from arg in args select arg.ToString()).ToList<string>();
            if (Config.Get(stringArgs) == null)
            {
                PrintError($"The plugin failed to read something from the config: {ArrayToString(stringArgs, 0, "/")}{Environment.NewLine}Please reload the plugin and see if this message is still showing. If so, please post this into the support thread of this plugin.");
                return defaultVal;
            }

            return (T)Convert.ChangeType(Config.Get(stringArgs), typeof(T));
        }

        private string GetMsg(string key, object userID = null)
        {
            return lang.GetMessage(key, this, userID.ToString());
        }

        ////////////////////////////////////////
        ///     Permission Related
        ////////////////////////////////////////

        private void RegisterPerm(params string[] permArray)
        {
            if (permArray == null || permArray.Length < 1) return;
            string perm = ArrayToString(permArray, 0, ".");
            if (perm.Contains("credits") || perm.Contains("kngs")) return;
            if (!PermExists(perm))
            {
                permission.RegisterPermission(perm, this);
                //PrintWarning("Registered permission " + perm);
            }
        }

        private bool PermExists(string perm)
        {
            if (string.IsNullOrEmpty(perm)) return false;
            var plgs = plugins?.GetAll() ?? null;
            // var plgs = plugins?.GetAll()?.Where(p => p != null && !p.IsCorePlugin && p.IsLoaded)?.ToList() ?? null;
            if (plgs != null && plgs.Length > 0)
            {
                for (int i = 0; i < plgs.Length; i++)
                {
                    var plg = plgs[i];
                    if (plg == null || plg.IsCorePlugin || !plg.IsLoaded) continue;
                    if (permission.PermissionExists(perm, plg) || permission.PermissionExists(perm, null)) return true;
                }
            }
            return false;
        }

        private bool HasPerm(string uid, params string[] permArray)
        {
            string perm = ArrayToString(permArray, 0, ".");

            //BroadcastChat($"--> <color=red>Checking for permission '{PermissionPrefix}.{perm}'...</color>");
            //BroadcastChat($"--> <color=red>Permission Check result: {permission.UserHasPermission(uid.ToString(), $"{PermissionPrefix}.{perm}").ToString()}</color>");
            //Puts($"{perm}");
            return permission.UserHasPermission(uid.ToString(), $"{perm}");
        }

        private string PermissionPrefix
        {
            get
            {
                return this.Title.Replace(" ", "").ToLower();
            }
        }

        ////////////////////////////////////////
        ///     Chat Handling
        ////////////////////////////////////////

        private void SendMessageToPlayer(BasePlayer target, string sourcePlayerName, string message, string chatUserID = "0", string nameColor = "white")
        {
            if (target == null || !target.IsConnected) return;

            if (_emojiPattern.IsMatch(message))
            {
                message = RemoveTags(message);

                var pName = RemoveTags(sourcePlayerName);


                var sb = Pool.Get<StringBuilder>();
                try
                {
                    var pNameColon = sb.Clear().Append(pName).Append(":").ToString();

                    target.SendConsoleCommand("chat.add2", 0, chatUserID, sb.Clear().Append(message).Replace(pNameColon, string.Empty).ToString(), pName, nameColor, 1f);

                }
                finally { Pool.Free(ref sb); }
            
                return;
            }

            target.SendConsoleCommand("chat.add", string.Empty, chatUserID, message);
        }

        private void BroadcastPlayerMessage(string displayName, string message, string chatUserID = "0", string nameColor = "white")
        {
            if (_emojiPattern.IsMatch(message))
            {
                message = RemoveTags(message);

                var pName = RemoveTags(displayName);

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    var pNameColon = sb.Clear().Append(pName).Append(":").ToString();

                    ConsoleNetwork.BroadcastToAllClients("chat.add2", 0, chatUserID, sb.Clear().Append(message).Replace(pNameColon, string.Empty).ToString(), pName, nameColor, 1f);
                }
                finally { Pool.Free(ref sb); }

          
                return;
            }


            ConsoleNetwork.BroadcastToAllClients("chat.add", 0, chatUserID, message);
        }

        private void SendChatMessage(BasePlayer player, string prefix, string msg = null) => rust.SendChatMessage(player, msg == null ? prefix : "<color=#C4FF00>" + prefix + "</color>: " + msg);
    }
}
