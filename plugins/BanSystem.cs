using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel;
using Oxide.Core.Libraries;
using Facepunch;

namespace Oxide.Plugins
{
    [Info("BanSystem", "Shady/MBR", "1.7.0", ResourceId = 0)]
    internal class BanSystem : CovalencePlugin
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
        private readonly Plugin AAIPDatabase;

        [PluginReference]
        private readonly Plugin Known;

        [PluginReference]
        private readonly Plugin DiscordAPI2, PRISMID;
        #endregion
        #region Dictionaries
        private readonly Dictionary<string, bool> _banMsgLink = new Dictionary<string, bool>();
        #endregion
        #region Lists
        private readonly List<string> _ipsFields = new List<string> { "IPs" }; //always use this list when sending ban updates for IPs to prevent garbage/unnecessary list creation


        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        #endregion
        #region Strings
        public const string IP_BAN_PERM = "bansystem.ipbanoverride";
        public const string STEAM_PROFILE_ID = "76561198886960011";
        public const string KICK_REASON_CHEATING = "Cheating is not tolerated on PRISM servers.";
        #endregion
        #region Integers
        private int _apiFailTimes = 0;
        #endregion
        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);


        private Timer ban_refresh_timer = null;

        private BanData _banData;

        #endregion
        #region Classes
        private class BanInfo
        {
            [JsonProperty(PropertyName = "_start", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [DefaultValue("")]
            private string _banStart = string.Empty;
            [JsonProperty(PropertyName = "_end", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [DefaultValue("")]
            private string _banEnd = string.Empty;
            [JsonIgnore]
            public DateTime BanStart
            {
                get
                {
                    return string.IsNullOrEmpty(_banStart) ? DateTime.MinValue : DateTime.Parse(_banStart);
                }
                set { _banStart = value.ToString("s", CultureInfo.InvariantCulture); }
            }
            [JsonIgnore]
            public DateTime BanEnd
            {
                get
                {
                    return string.IsNullOrEmpty(_banEnd) ? DateTime.MinValue : DateTime.Parse(_banEnd);
                }
                set { _banEnd = value.ToString("s", CultureInfo.InvariantCulture); }
            }

            [JsonProperty(PropertyName = "Issuer")]
            public string BannedIssuer = "0";

            [JsonProperty(PropertyName = "Name", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [DefaultValue("")]
            public string DisplayName = string.Empty;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string Notes { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "C", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string Country { get; set; } = string.Empty;

            [JsonIgnore]
            public bool Permanent
            {
                get { return BanEnd == DateTime.MinValue; }
                set { if (value) BanEnd = DateTime.MinValue; }
            }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool IPBanned = false;

            [JsonProperty(PropertyName = "IPs")]
            public List<string> IPAddresses = new List<string>();

            [JsonProperty(PropertyName = "Reason", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string BanReason = string.Empty;

            public bool IsIPBanned(string IP)
            {
                if (IPBanned && IPAddresses != null && IPAddresses.Contains(IP)) return true;
                return false;
            }

            public bool AnyIPBanned(List<string> IPs)
            {
                if (IPs == null || IPs.Count < 1) return false;
                if (IPBanned && IPAddresses != null) { for (int i = 0; i < IPAddresses.Count; i++) { if (IPs.Contains(IPAddresses[i])) return true; } }
                return false;
            }

            public bool HasIP(string IP)
            {
                if (IPAddresses != null && IPAddresses.Contains(IP)) return true;
                return false;
            }

            public BanInfo() { }

            public BanInfo(IPlayer player, bool ipBan = false)
            {
                Permanent = true;
                var ipAddr = (player?.IsConnected ?? false) ? (player?.Address ?? string.Empty) : string.Empty;
                if (!IPAddresses.Contains(ipAddr)) IPAddresses.Add(ipAddr);
                IPBanned = ipBan;
                DisplayName = player?.Name ?? string.Empty;
            }

            public BanInfo(IPlayer player, DateTime? endDate = null, bool ipBan = false)
            {
                var ipAddr = (player?.IsConnected ?? false) ? (player?.Address ?? string.Empty) : string.Empty;
                if (!IPAddresses.Contains(ipAddr)) IPAddresses.Add(ipAddr);
                IPBanned = ipBan;
                BanStart = DateTime.UtcNow;
                DisplayName = player?.Name ?? string.Empty;
                if (endDate != null && endDate != DateTime.MinValue)
                {
                    DateTime newBanEnd;
                    if (DateTime.TryParse(endDate.ToString(), out newBanEnd)) BanEnd = newBanEnd;
                }
            }

            public BanInfo(bool ipBan = false)
            {
                IPBanned = ipBan;
                BanStart = DateTime.UtcNow;
            }

            public BanInfo(DateTime? endDate = null, bool ipBan = false)
            {
                IPBanned = ipBan;
                BanStart = DateTime.UtcNow;
                if (endDate != null && endDate != DateTime.MinValue)
                {
                    DateTime newBanEnd;
                    if (DateTime.TryParse(endDate.ToString(), out newBanEnd)) BanEnd = newBanEnd;
                }
            }

            public BanInfo(string IP, DateTime? endDate = null)
            {
                if (!IPAddresses.Contains(IP)) IPAddresses.Add(IP);
                IPBanned = true;
                BanStart = DateTime.UtcNow;
                if (endDate != null && endDate != DateTime.MinValue)
                {
                    DateTime newBanEnd;
                    if (DateTime.TryParse(endDate.ToString(), out newBanEnd)) BanEnd = newBanEnd;
                }
            }
        }

        private class BanData
        {
            public Dictionary<string, BanInfo> banInfos = new Dictionary<string, BanInfo>();
            public BanData() { }
        }
        #endregion

        private string ServerId
        {
            get
            {
                if (!(PRISMID?.IsLoaded ?? false))
                {
                    throw new Exception("PRISMID is not loaded");
                }

                return PRISMID.Call<string>("GetServerTypeString");
            }
        }

        private string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);

        protected override void LoadDefaultMessages()
        {
            var messages = new Dictionary<string, string>
            {
                //DO NOT EDIT LANGUAGE FILES HERE!
                {"noPerms", "You do not have permission to use this command!"},
                {"targetNotFound", "Target \"{0}\" not found!" },
                {"youBanned", "You banned \"{0}\" for: \"{1}\", until: \"{2}\"" },
                {"alreadyBanned", "\"{0}\" is already banned!" },
                {"youUnbanned", "You unbanned \"{0}\"" }
            };
            lang.RegisterMessages(messages, this);
        }

        #region Hooks

        

        //     #if RUST
        private object CanClientLogin(Network.Connection connection)
        {
            if (connection == null)
            {
                PrintWarning("CanClientLogin called on null connection?!");
                return "No connection!";
            }

            try
            {
                var now = DateTime.UtcNow;

                var userID = connection?.userid ?? 0;
                var userIDStr = userID.ToString();

                var userName = connection?.username ?? string.Empty;

                var ipAddr = getIPString(connection);
                var ipAddresses = AAIPDatabase?.Call<List<string>>("GetAllIPs", userIDStr) ?? null;
                if (string.IsNullOrEmpty(ipAddr)) PrintWarning("Bad IP addr!");

                var banDataID = GetBanInfoByID(userIDStr);
                var banDataIPs = GetBanInfos(ipAddr);
                var banDataIP = GetBanInfoByIP(ipAddr);
                var ownerBan = (connection.ownerid != 0 && connection.ownerid != connection.userid) ? GetBanInfoByID(connection.ownerid.ToString()) : null;

                if (connection.ownerid != 0 && connection.ownerid != connection.userid) PrintWarning("Family share?: " + connection.userid + " has ownerid of: " + connection.ownerid);

                bool showLink;
                if (!_banMsgLink.TryGetValue(userIDStr, out showLink)) _banMsgLink[userIDStr] = showLink = false;
                else _banMsgLink[userIDStr] = showLink = !showLink;



                //TEMP DISABLE BAN EVASION - AUG 4 2023

                /*/
                if ((banDataIPs?.Count > 0 || banDataIP != null) && banDataID == null && !permission.UserHasPermission(userIDStr, IP_BAN_PERM))
                {
                    if (banDataIP == null && banDataIPs?.Count > 0) banDataIP = banDataIPs[0];
                    BanPlayer(userIDStr, "Ban Evasion (Original: " + banDataIP.BanReason + ")", banDataIP.IPBanned, null, banDataIP.BannedIssuer, false, string.Empty, userName, true, false);
                    PrintWarning("Tried to add new ban for ID " + userIDStr + ", from ip ban: " + ipAddr);
                }

                if (ownerBan != null && (ownerBan.Permanent || now < ownerBan.BanEnd) && banDataID == null && banDataIP == null && !permission.UserHasPermission(userIDStr, IP_BAN_PERM))
                {
                    BanPlayer(userIDStr, "Ban Evasion (Original: " + ownerBan.BanReason + ")", ownerBan.IPBanned, null, ownerBan.BannedIssuer, false, string.Empty, userName, true, false);
                    PrintWarning("Tried to add new ban for ID " + userIDStr + ", from owner ban: " + connection.ownerid);
                }/*/

                var banData = banDataID ?? banDataIP ?? ownerBan;
                var banId = banData == banDataID ? userID.ToString() : banData == ownerBan ? connection.ownerid.ToString() : string.Empty;

                if (string.IsNullOrEmpty(banId) && banDataIP != null)
                {
                    foreach (var kvp in _banData.banInfos)
                    {

                        var banInfo = kvp.Value;
                        if (banInfo == banDataIP)
                        {
                            banId = kvp.Key;
                            break;
                        }
                    }
                }

                //nothing, absolutely nothing worth keeping was on this line
                if (banData != null && (banId == userID.ToString() || ((banData.IsIPBanned(ipAddr) || (connection.ownerid != 0 && connection.ownerid != connection.userid && banId == connection.ownerid.ToString() && !permission.UserHasPermission(userIDStr, IP_BAN_PERM)) || banData.AnyIPBanned(ipAddresses)) && !permission.UserHasPermission(userIDStr, IP_BAN_PERM))))
                {
                    if (banData.BanEnd != DateTime.MinValue && now >= banData.BanEnd.ToUniversalTime())
                    {
                        PrintWarning("Expired ban?!, banEnd: " + banData.BanEnd + ", now: " + now + ", removing: " + userIDStr);
                        UnbanPlayer(banId);
                        //RemoveBanInfo(banData);
                        return null;
                    }

                    //removing the port
                    var ip = connection.ipaddress.Split(':')[0];

                    if (!banData.IsIPBanned(ip))
                    {
                        banData.IPAddresses.Add(ip);
                        UpdateBan(banId, new { IPs = banData.IPAddresses });
                    }

                    var banMsg = showLink ? "Appeal ban here: <color=orange>prismrust.enjin.com</color> or: <color=#ff912b>discord.gg/DUCnZhZ</color>" : !string.IsNullOrEmpty(banData?.BanReason) ? (" <color=#ff912b>Banned</color>: " + banData.BanReason) : "You are banned from this server";
                    if (banData.BanEnd != DateTime.MinValue) banMsg += " - <color=orange>Unban date: " + banData.BanEnd + " <size=10>(UTC)</size></color>";
                    return banMsg;
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                return "Unable to get login approval, exception message: " + ex.Message;
            }

            return null;
        }

        //     #endif


        //handle rust bans
        private void OnUserBanned(string name, string id, string ip, string reason)
        {
            var banInfo = GetBanInfoByID(id) ?? GetBanInfoByNameOrID(name) ?? GetBanInfoByIP(ip);
            if (banInfo == null)
            {
                banInfo = NewBanInfo(id, true);
                var userIP = getIPString(id);

                if (!string.IsNullOrEmpty(userIP) && !banInfo.IPAddresses.Contains(userIP)) banInfo.IPAddresses.Add(userIP);

                banInfo.BanReason = reason;
                banInfo.DisplayName = covalence.Players?.FindPlayerById(id)?.Name ?? ((!string.IsNullOrEmpty(name)) ? name : "Unknown");

                PrintWarning("Hook detected new ban that has no baninfo, now adding and attempting to remove old ban");

                var player = covalence.Players.FindPlayerById(id);
                if (player != null && (player?.IsBanned ?? false)) player.Unban();

                ulong userID;
                if (ulong.TryParse(id, out userID))
                {
                    var userGet = ServerUsers.Get(userID);
                    if (userGet != null && userGet.group == ServerUsers.UserGroup.Banned) ServerUsers.Set(userID, ServerUsers.UserGroup.None, name, reason);
                }
            }
        }

        private void OnUserConnected(IPlayer player)
        {
            if (player == null) return;

            var plyId = player.Id;
            var plyAddr = player.Address;
            var plyName = player.Name;

            timer.Once(3f, () =>
            {
                if (player == null) return;

                if (IsBanned(player.Id))
                {
                    PrintWarning(player?.Name + " (" + player?.Id + ") is banned on OnUserConnected timer, not checking aliases");

                    if (player.IsConnected)
                    {
                        PrintWarning("Player was still connected on OnUserConnected after apparently having been banned, so we'll manually kick");
                        player.Kick("You are banned from this server");
                    }

                    return;
                }

                var aliases = Known?.Call<List<string>>("GetAliases", plyId) ?? null;
                if (aliases == null) aliases = new List<string> { plyName };
                if (aliases.Count < 1) return;

                if (_banData?.banInfos == null || _banData.banInfos.Count < 1)
                {
                    PrintWarning("No bans infos! Not checking aliases");
                    return;
                }

                var aliasSB = new StringBuilder();

                foreach (var kvp in _banData.banInfos)
                {
                    var banId = kvp.Key;
                    if (banId == plyId) continue;

                    var banInfo = kvp.Value;
                    if (banInfo == null) continue;


                    if (banInfo.DisplayName.Equals(plyName, StringComparison.OrdinalIgnoreCase))
                    {
                        aliasSB.Append("Exact ban name/ID match: ").Append(banInfo.DisplayName).Append(" (").Append(banId).Append(") matches for connecting player: ").Append(plyName).Append(" (").Append(plyId).Append(", ").Append(plyAddr).Append(")").Append(Environment.NewLine);
                    }

                    if (string.IsNullOrEmpty(banId)) continue;

                    var banAliases = Known?.Call<List<string>>("GetAliases", banId) ?? null;
                    if (banAliases == null || banAliases.Count < 1) continue;

                    for (int j = 0; j < banAliases.Count; j++)
                    {
                        var banAlias = banAliases[j];
                        if (string.IsNullOrEmpty(banAlias)) continue;
                        if (banAlias.Equals(plyName, StringComparison.OrdinalIgnoreCase) || aliases.Contains(banAlias) || aliases.Any(p => p.Equals(banAlias, StringComparison.OrdinalIgnoreCase))) aliasSB.Append("Alias ban name/ID match: ").Append(banAlias).Append(" (").Append(banId).Append(") matches for connecting player: ").Append(plyName).Append(" (").Append(plyId).Append(", ").Append(plyAddr).Append(")").Append(Environment.NewLine);
                    }

                }

                if (aliasSB.Length > 0)
                {
                    aliasSB.Length -= 1;
                    PrintWarning(aliasSB.ToString().TrimEnd());
                }
            });
        }

        private void Init()
        {
            _banData = Interface.Oxide?.DataFileSystem?.ReadObject<BanData>("BanSystem") ?? new BanData();

            ban_refresh_timer = timer.Every(300f, UpdateBanList);
            var perms = new List<string> { "kick", "ban" };
            foreach (string perm in perms) permission.RegisterPermission("bansystem." + perm, this);
            permission.RegisterPermission(IP_BAN_PERM, this);
            timer.Once(2f, UpdateBanList);
        }

        private void Unload()
        {
            try 
            {
                ban_refresh_timer?.Destroy();
            }
            finally { SaveData(); }
        }

        #endregion

        #region Commands

        [Command("bans.addips")]
        private void cmdBansAddIPs(IPlayer player, string command, string[] args)
        {
            if (player != null && !player.IsAdmin) return;
            FillBanIPs();
        }

        [Command("bans.json")]
        private void cmdBanJson(IPlayer player, string command, string[] args)
        {
            if (player != null && !player.IsAdmin) return;
            SendReply(player, GetBanJson());
        }

        [Command("bans.transfer")]
        private void cmdBanTransfer(IPlayer player, string command, string[] args)
        {
            if (!(player?.IsAdmin ?? false))
            {
                SendReply(player, "You must be an admin to use this!");
                return;
            }

            var convSB = new StringBuilder();
            var bannedUsers = ServerUsers.GetAll(ServerUsers.UserGroup.Banned)?.ToList() ?? null;
            if (bannedUsers == null || bannedUsers.Count < 1) return;
            for (int i = 0; i < bannedUsers.Count; i++)
            {
                var bannedPlayer = bannedUsers[i];
                if (bannedPlayer == null) continue;
                var sID = bannedPlayer.steamid.ToString();
                var ipAddr = getIPString(sID);
                var banData = NewBanInfo(sID, true);
                if (banData != null)
                {
                    banData.BanReason = bannedPlayer?.notes ?? string.Empty;
                    if (!banData.IPAddresses.Contains(ipAddr)) banData.IPAddresses.Add(ipAddr);
                    convSB.AppendLine("Transferred: " + (bannedPlayer?.username ?? string.Empty) + " (" + sID + ") with reason: " + bannedPlayer?.notes ?? string.Empty);
                }
            }
            SendReply(player, convSB.ToString().TrimEnd() + "\n Total count transferred: " + bannedUsers.Count);
        }

        [Command("banlist.export")]
        private void cmdBanListExport(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            ExportBanString("output", true);
        }

        [Command("banlistex")]
        private void cmdBanListEx(IPlayer player, string command, string[] args)
        {
            var max = -1;
            if (args.Length > 0 && !int.TryParse(args[0], out max)) max = -1;
            SendReply(player, (max != -1) ? GetBanStringLatest(max) : GetBanString());
        }

        [Command("baninfo")]
        private void cmdBanInfo(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                SendReply(player, "Please specify a name, ID or IP!", STEAM_PROFILE_ID);
                return;
            }

            var id = args[0];
            var banInfo = GetBanInfoByID(id);
            IPlayer target_out = null;

            if (banInfo == null)
            {
                var target = FindConnectedPlayer(args[0], true);
                if (target == null)
                {
                    SendReply(player, "No player could be found with this name/ID/IP: " + args[0], STEAM_PROFILE_ID);
                    return;
                }

                if (!IsBannedP(target))
                {
                    SendReply(player, "This player is not banned!", STEAM_PROFILE_ID);
                    return;
                }

                banInfo = GetBanInfoByID(target.Id);
                target_out = target;

                if (banInfo == null)
                {
                    SendReply(player, "Bad ban info!", STEAM_PROFILE_ID);
                    return;
                }
            }


            var newSB = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                newSB.Clear().Append("<size=18>Ban Info for ").Append((banInfo?.DisplayName ?? target_out?.Id ?? "Unknown name")).Append(":</size>");
                newSB.Append(Environment.NewLine).Append("Ban Date: ").Append(banInfo.BanStart);
                newSB.Append(Environment.NewLine).Append("Ban End: ").Append((banInfo.Permanent ? "Permanent" : banInfo.BanEnd.ToString()));
                newSB.Append(Environment.NewLine).Append("Ban Reason: ").Append(banInfo.BanReason);
                newSB.Append(Environment.NewLine).Append("Ban Issuer: ").Append((GetNameFromID(banInfo.BannedIssuer) ?? "Unknown")).Append(" (").Append(banInfo.BannedIssuer).Append(")");

                var ips = banInfo?.IPAddresses;
                var latestIp = ips?.Count > 0 ? ips[ips.Count - 1] : string.Empty;
                newSB.Append(Environment.NewLine).AppendLine("IP Banned: ").Append(banInfo.IPBanned).Append(", latest IP: ").Append(latestIp);

                if (ips != null && ips.Count > 0)
                {
                    var sameIPBanCount = 0;
                    foreach (var kvp in _banData.banInfos)
                    {
                        var banId = kvp.Key;
                        var ban2 = kvp.Value;
                        if (ban2 == banInfo) continue;

                        var hasSameIpBan = false;

                        for (int i = 0; i < ban2.IPAddresses.Count; i++)
                        {
                            var banIP = ban2.IPAddresses[i];
                            if (ips.Contains(banIP))
                            {
                                hasSameIpBan = true;
                                break;
                            }
                        }

                        if (!hasSameIpBan) continue;
                        else
                        {
                            if (sameIPBanCount < 1) newSB.Append(Environment.NewLine).Append("Banned accounts with same IP address:").Append(Environment.NewLine);

                            sameIPBanCount++;
                        }


                        newSB.Append(ban2?.DisplayName ?? GetNameFromID(banId)).Append(" (").Append(banId).Append(")").Append(Environment.NewLine);
                    }
                    if (sameIPBanCount > 0)
                    {
                        newSB.Length -= 1; //trailing newline
                        newSB.Append(sameIPBanCount.ToString("N0")).Append(" banned accounts with same IP address");

                    }
                    else newSB.AppendLine("No other accounts found banned on same IP"); //never appended, so no IPs
                }

                SendReply(player, newSB.ToString().TrimEnd(), STEAM_PROFILE_ID);
            }
            finally { Facepunch.Pool.Free(ref newSB); }

        }

        [Command("bancount")]
        private void cmdBanListC(IPlayer player, string command, string[] args)
        {
            var sb = Facepunch.Pool.Get<StringBuilder>();
            try { SendReply(player, sb.Clear().Append("There are: <color=#ff912b>").Append(GetBannedCountWithIPOnlyBans().ToString("N0")).Append("</color> banned users!").ToString()); } //HERE  - disabled
            finally { Facepunch.Pool.Free(ref sb); }
        }

        [Command("unban")]
        private void cmdUnbanPlayer(IPlayer player, string command, string[] args)
        {
            if (!canExecute(player, "ban"))
            {
                SendReply(player, "You do not have permission to use this!", STEAM_PROFILE_ID);
                return;
            }

            if (args.Length <= 0)
            {
                SendReply(player, "You must specify a name!", STEAM_PROFILE_ID);
                return;
            }

            ulong userID = 0;
            var target = FindIPlayer(args[0]);

            if (target == null && (!ulong.TryParse(args[0], out userID) || !userID.IsSteamId()))
            {
                SendReply(player, "No or more than 1 players found with the (partial) name of: " + args[0] + "!", STEAM_PROFILE_ID);
                return;
            }

            var userIDStr = userID != 0 ? userID.ToString() : target.Id;
            var isBanned = IsBanned(userIDStr);
            var Name = target?.Name ?? string.Empty;

            if (!isBanned)
            {
                SendReply(player, Name + " (" + userIDStr + ")  is not banned!", STEAM_PROFILE_ID);
                return;
            }

            UnbanPlayer(userIDStr);
            SendReply(player, "Unbanned: " + Name + " (" + userIDStr + ")", STEAM_PROFILE_ID);
        }

        private void AnnounceBan(string userIdOrName, string reason, TimeSpan span = default(TimeSpan))
        {
            if (string.IsNullOrEmpty(userIdOrName))
            {
                PrintWarning(nameof(userIdOrName) + " was empty on " + nameof(AnnounceBan));
                return;
            }

            ulong uid;
            var name = userIdOrName;

            if (ulong.TryParse(userIdOrName, out uid)) name = covalence.Players?.FindPlayerById(userIdOrName)?.Name ?? userIdOrName;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                sb.Clear().Append("<color=#33ccff><size=20>").Append("\"").Append(name).Append("\" was banned: ").Append(reason);


                if (span > TimeSpan.Zero)
                {
                    sb.Append(" - for ");
                    if (span.TotalDays >= 1) sb.Append(span.TotalDays.ToString("N0") + " days");
                    else if (span.TotalHours >= 1) sb.Append(span.TotalHours.ToString("N0") + " hours");
                    else if (span.TotalMinutes >= 1) sb.Append(span.TotalMinutes.ToString("N0") + " minutes");
                    else sb.Append(span.TotalSeconds.ToString("0.0").Replace(".0", string.Empty) + " seconds");
                }

                sb.Append("</size></color>");

                SimpleBroadcast(sb.ToString()); //HERE
            }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private void AnnounceTotalBan()
        {
            var sb = Facepunch.Pool.Get<StringBuilder>();
            try { SimpleBroadcast(sb.Clear().Append("There are now <color=#ff912b>").Append(GetBannedCountWithIPOnlyBans().ToString("N0")).Append("</color> banned players!").ToString()); } //HERE
            finally { Facepunch.Pool.Free(ref sb); }
        }

        [Command("bansilent")]
        private void cmdBanPlayerSilent(IPlayer player, string command, string[] args)
        {
            if (args == null || args.Length < 2) return;

            var newArgs = args.ToList();
            newArgs.Add("silent");

            cmdBanPlayer(player, command, newArgs.ToArray());
        }


        [Command("ban")]
        private void cmdBanPlayer(IPlayer player, string command, string[] args)
        {
            try
            {
                var pId = player.Id;
                var pName = player.Name;

                if (!canExecute(player, "ban") && !(player?.IsAdmin ?? false))
                {
                    SendReply(player, "You do not have permission to use this!", STEAM_PROFILE_ID);
                    return;
                }

                if (args.Length <= 1)
                {
                    SendReply(player, "You must specify a name and reason!", STEAM_PROFILE_ID);
                    return;
                }

                var target = FindConnectedPlayer(args[0], true);
                ulong targetIDU = 0;

                if (target == null)
                {
                    var arg0 = args[0];
                    if (!ulong.TryParse(arg0, out targetIDU))
                    {
                        SendReply(player, "Couldn't find a player with name: " + arg0 + " and this is not a ulong: " + arg0, STEAM_PROFILE_ID);
                        return;
                    }
                }

                if (target != null && IsBannedP(target))
                {
                    SendReply(player, target.Name + " - is already banned!", STEAM_PROFILE_ID);
                    return;
                }

                var targetIP = getIPString(target);
                if (string.IsNullOrEmpty(targetIP))
                {
                    if (targetIDU != 0) targetIP = getIPString(targetIDU.ToString());
                }

                var targetID = target?.Id ?? targetIDU.ToString();
                //I need this empty so the bot can fill it
                var targetName = target?.Name ?? "";

                if (target == null) targetIP = getIPString(targetID);

                if (IsBanned(targetID))
                {
                    SendReply(player, targetID + " - is already banned!", STEAM_PROFILE_ID);
                    return;
                }

                var banEnd = DateTime.MinValue;
                var numLength = 0;
                var splitChar = string.Empty;

                if (args.Length > 2)
                {
                    var lengthArg = args[2].ToLower();
                    if (!lengthArg.Equals("silent", StringComparison.OrdinalIgnoreCase))
                    {
                        if (lengthArg.EndsWith("d")) splitChar = "d";
                        if (lengthArg.EndsWith("h")) splitChar = "h";
                        if (lengthArg.EndsWith("m")) splitChar = "m";

                        var split1 = lengthArg.Split(splitChar.ToCharArray());
                        var splitLength = split1[0];

                        if (!int.TryParse(splitLength, out numLength))
                        {
                            SendReply(player, "Bad length: " + splitLength + ", " + split1 + ", splitChar: " + splitChar + ", " + lengthArg, STEAM_PROFILE_ID);
                        }

                        else
                        {
                            if (splitChar == "d")
                            {
                                banEnd = DateTime.UtcNow.AddDays(numLength);
                                PrintWarning("add days");
                            }
                            if (splitChar == "h")
                            {
                                banEnd = DateTime.UtcNow.AddHours(numLength);
                                PrintWarning("add hours");
                            }
                            if (splitChar == "m") banEnd = DateTime.UtcNow.AddMinutes(numLength);
                        }
                    }
                }

                if (banEnd > DateTime.MinValue) PrintWarning("passing date time: " + banEnd.ToString());

                var banReason = args[1];
                var kickReason = banReason.ToLower().Contains("cheating") ? KICK_REASON_CHEATING : string.Empty;
                var ownerId = (target?.Object as BasePlayer)?.net?.connection?.ownerid ?? 0;
                var isSilent = args[args.Length - 1].Equals("silent", StringComparison.OrdinalIgnoreCase);

                BanPlayer(targetID, banReason, true, banEnd, player.Id, true, kickReason, targetName, isSilent, false);

                if (ownerId != 0 && ownerId != targetIDU)
                {
                    var ownerStr = ownerId.ToString();
                    PrintWarning("Also banning ownerId: " + ownerStr);
                    var ownerName = FindConnectedPlayer(ownerStr, true)?.Name ?? string.Empty;
                    BanPlayer(ownerStr, banReason, true, banEnd, pId, true, kickReason, ownerName, true, false);
                }

                SendReply(player, "You banned \"" + targetName + "\"" + " for reason: \"" + args[1] + "\"", STEAM_PROFILE_ID);
                Puts(player.Name + " (" + player.Id + ")" + " banned \"" + targetName + "\"" + " for reason: \"" + args[1] + "\"");

                if (!string.IsNullOrEmpty(targetIP))
                {
                    var ipMatches = covalence.Players?.All?.Where(p => p != null && p != target && getIPString(p) == targetIP) ?? null;

                    if (ipMatches != null && ipMatches.Any())
                    {
                        Puts(ipMatches.Count().ToString("N0") + " IP matches found");
                        var banSB = new StringBuilder();

                        foreach (var match in ipMatches)
                        {
                            BanPlayer(match.Id, banReason, true, banEnd, pId, true, kickReason, match.Name, true, false);
                            banSB.AppendLine(match.Name + " (" + match.Id + ")");
                        }

                        var banSBStr = banSB.ToString().TrimEnd();
                        Puts(pName + " (" + pId + ") banned:\n" + banSBStr);
                        SendReply(player, "Banned:\n" + banSBStr, STEAM_PROFILE_ID);
                    }
                }
            }

            catch (Exception ex)
            {
                SendReply(player, "Failed to complete banning: " + ex.Message, STEAM_PROFILE_ID);
                PrintWarning(ex.Message);
                PrintError(ex.ToString());
            }
        }

        [Command("banbyid")]
        private void cmdBanID(IPlayer player, string command, string[] args)
        {
            try
            {
                if (!canExecute(player, "ban") && !(player?.IsAdmin ?? false))
                {
                    SendReply(player, "You do not have permission to use this!");
                    return;
                }

                if (args.Length <= 1)
                {
                    SendReply(player, "You must specify a name and reason!");
                    return;
                }

                ulong userID;
                if (!ulong.TryParse(args[0], out userID))
                {
                    SendReply(player, "Invalid ID: " + args[0]);
                    return;
                }

                var userIDStr = userID.ToString();
                if (IsBanned(userIDStr))
                {
                    SendReply(player, userIDStr + " - is already banned!");
                    return;
                }

                var isSilent = args.LastOrDefault().Equals("silent", StringComparison.OrdinalIgnoreCase);
                var targetName = GetNameFromID(userIDStr);
                var kickReason = args[1].ToLower().Contains("cheating") ? KICK_REASON_CHEATING : string.Empty;
                var banEnd = DateTime.MinValue;
                var numLength = 0;
                var splitChar = string.Empty;

                if (args.Length > 2)
                {
                    var lengthArg = args[2].ToLower();
                    lengthArg.Replace("\"", string.Empty);

                    if (lengthArg.EndsWith("d")) splitChar = "d";
                    if (lengthArg.EndsWith("h")) splitChar = "h";
                    if (lengthArg.EndsWith("m")) splitChar = "m";

                    Puts("Split char: " + splitChar);

                    var split1 = lengthArg.Split(splitChar.ToCharArray());
                    var splitLength = split1[0];

                    if (!int.TryParse(splitLength, out numLength))
                    {
                        SendReply(player, "Bad length: " + splitLength + ", " + split1 + ", splitChar: " + splitChar + ", " + lengthArg);
                    }
                    else
                    {
                        if (splitChar == "d")
                        {
                            banEnd = DateTime.UtcNow.AddDays(numLength);
                            PrintWarning("add days");
                        }

                        if (splitChar == "h")
                        {
                            banEnd = DateTime.UtcNow.AddHours(numLength);
                            PrintWarning("add hours");
                        }

                        if (splitChar == "m") banEnd = DateTime.UtcNow.AddMinutes(numLength);
                    }
                }

                BanPlayer(userIDStr, args[1], true, banEnd, player.Id, true, kickReason, targetName, isSilent, false);
                Puts(player.Name + " (" + player.Id + ")" + " banned \"" + targetName + "\" (" + userIDStr + ") for reason: \"" + args[1] + "\"");
            }

            catch (Exception ex)
            {
                SendReply(player, "Failed to complete banning: " + ex.Message, STEAM_PROFILE_ID);
                PrintWarning(ex.Message);
                PrintError(ex.ToString());
            }
        }

        [Command("banbyip")]
        private void cmdBanIP(IPlayer player, string command, string[] args)
        {
            try
            {
                if (!canExecute(player, "ban") && !(player?.IsAdmin ?? false))
                {
                    SendReply(player, "You do not have permission to use this!", STEAM_PROFILE_ID);
                    return;
                }

                if (args.Length <= 1)
                {
                    SendReply(player, "You must specify an IP and reason!", STEAM_PROFILE_ID);
                    return;
                }

                var IP = args[0];
                if (IsIPBanned(IP))
                {
                    SendReply(player, IP + " - is already banned!", STEAM_PROFILE_ID);
                    return;
                }

                var isSilent = args.LastOrDefault().Equals("silent", StringComparison.OrdinalIgnoreCase);
                var kickReason = args[1].ToLower().Contains("cheating") ? KICK_REASON_CHEATING : string.Empty;
                var banEnd = DateTime.MinValue;
                var numLength = 0;
                var splitChar = string.Empty;

                if (args.Length > 2)
                {
                    var lengthArg = args[2].ToLower();
                    lengthArg.Replace("\"", string.Empty);

                    if (lengthArg.EndsWith("d")) splitChar = "d";
                    if (lengthArg.EndsWith("h")) splitChar = "h";
                    if (lengthArg.EndsWith("m")) splitChar = "m";

                    Puts("Split char: " + splitChar);

                    var split1 = lengthArg.Split(splitChar.ToCharArray());
                    var splitLength = split1[0];

                    if (!int.TryParse(splitLength, out numLength))
                    {
                        SendReply(player, "Bad length: " + splitLength + ", " + split1 + ", splitChar: " + splitChar + ", " + lengthArg, STEAM_PROFILE_ID);
                    }
                    else
                    {
                        if (splitChar == "d")
                        {
                            banEnd = DateTime.UtcNow.AddDays(numLength);
                            PrintWarning("add days");
                        }

                        if (splitChar == "h")
                        {
                            banEnd = DateTime.UtcNow.AddHours(numLength);
                            PrintWarning("add hours");
                        }

                        if (splitChar == "m") banEnd = DateTime.UtcNow.AddMinutes(numLength);
                    }

                }

                BanIP(IP, kickReason, banEnd, player.Id, true, kickReason);
                Puts(player.Name + " (" + player.Id + ")" + " banned IP \"" + IP + "\" for reason: \"" + args[1] + "\"");
            }

            catch (Exception ex)
            {
                SendReply(player, "Failed to complete banning: " + ex.Message, STEAM_PROFILE_ID);
                PrintWarning(ex.Message);
                PrintError(ex.ToString());
            }
        }

        [Command("bannotes")]
        private void cmdBanNote(IPlayer player, string command, string[] args)
        {
            if (!canExecute(player, "ban") && !(player?.IsAdmin ?? true))
            {
                SendReply(player, "You do not have permsision to use this!", STEAM_PROFILE_ID);
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "You must specify a name/ID!", STEAM_PROFILE_ID);
                return;
            }

            var ban = GetBanInfoByID(args[0]) ?? GetBanInfoByNameOrID(args[0]);
            if (ban == null)
            {
                SendReply(player, "Could not find a ban with: " + args[0], STEAM_PROFILE_ID);
                return;
            }



            if (args.Length < 2)
            {
                SendReply(player, "Ban notes: " + (ban?.Notes ?? "None"), STEAM_PROFILE_ID);
                return;
            }
            else
            {
                SendReply(player, "Set ban notes to: " + (ban.Notes = args[1]), STEAM_PROFILE_ID);
                var banId = GetUserIDFromBanInfo(ban);
                UpdateBan(banId, new { Notes = ban.Notes });
            }
        }

        [Command("banreason")]
        private void cmdBanReason(IPlayer player, string command, string[] args)
        {
            if (!canExecute(player, "ban") && !(player?.IsAdmin ?? true))
            {
                SendReply(player, "You do not have permsision to use this!", STEAM_PROFILE_ID);
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "You must specify a name/ID!", STEAM_PROFILE_ID);
                return;
            }

            var ban = GetBanInfoByID(args[0]) ?? GetBanInfoByNameOrID(args[0]);
            if (ban == null)
            {
                SendReply(player, "Could not find a ban with: " + args[0], STEAM_PROFILE_ID);
                return;
            }

            if (args.Length < 2)
            {
                SendReply(player, "Ban reason: " + (ban?.BanReason ?? "None"), STEAM_PROFILE_ID);
                return;
            }
            else
            {
                SendReply(player, "Set ban reason to: " + (ban.BanReason = args[1]), STEAM_PROFILE_ID);
                var banId = GetUserIDFromBanInfo(ban);
                UpdateBan(banId, new { Reason = ban.BanReason });
            }
        }

        [Command("kick")]
        private void cmdKickPlayer(IPlayer player, string command, string[] args)
        {
            if (!canExecute(player, "kick") && !(player?.IsAdmin ?? true))
            {
                SendReply(player, "You do not have permsision to use this!", STEAM_PROFILE_ID);
                return;
            }

            if (args.Length <= 1)
            {
                SendReply(player, "You must specify a name and reason!", STEAM_PROFILE_ID);
                return;
            }

            var target = FindConnectedPlayer(args[0]);
            if (target == null)
            {
                SendReply(player, "Could not find the specified player \"" + args[0] + "\".", STEAM_PROFILE_ID);
                return;
            }

            if (!(target?.IsConnected ?? false))
            {
                SendReply(player, "Target is not connected!", STEAM_PROFILE_ID);
                return;
            }

            target.Kick(args[1]);
            SendReply(player, "kicked \"" + target.Name + "\"" + " for reason: \"" + args[1] + "\"", STEAM_PROFILE_ID);
            Puts(player.Name + " (" + player.Id + ")" + " kicked \"" + target.Name + "\"" + " for reason: \"" + args[1] + "\"", STEAM_PROFILE_ID);
        }

        #endregion

        #region Util
        
        private void FillBanIPs()
        {
            var count = _banData.banInfos.Count;

            PrintWarning("Now looping through " + count.ToString("N0") + " ban entries to populate IP lists, this may take a while");

            Dictionary<string, BanInfo> newBans = new Dictionary<string, BanInfo>();
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                foreach (var kvp in _banData.banInfos)
                {
                    var info = kvp.Value;
                    var banId = kvp.Key;
                    if (banId == "0") continue;

                    var ips = AAIPDatabase?.Call<List<string>>("GetAllIPs", banId) ?? null;
                    var modified = false;

                    if (ips != null && ips.Count > 0)
                    {
                        for (int j = 0; j < ips.Count; j++)
                        {
                            var ip = ips[j];
                            if (string.IsNullOrEmpty(ip))
                            {
                                PrintWarning("IP was null/empty!! (" + banId + ") index: " + j);
                                continue;
                            }

                            if (!info.IPAddresses.Contains(ip))
                            {
                                modified = true;
                                info.IPAddresses.Add(ip);
                            }
                        }
                    }

                    if (info.IPAddresses.Contains(""))
                    {
                        Puts("Detected empty string in IPs for " + banId);
                        modified = true;
                        info.IPAddresses.Remove("");
                    }

                    if (modified) newBans.Add(banId, info);
                }

                if (newBans.Count > 0) SendBansUpdateRequest(newBans, _ipsFields);
            }
            finally
            {
                try
                {
                    PrintWarning("Looped through " + count.ToString("N0") + " ban infos and added a total of: " + (newBans?.Count ?? 0).ToString("N0") + " IPs to lists in " + watch.Elapsed.TotalMilliseconds + "ms");
                }
                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^printwarning^"); }
                Facepunch.Pool.Free(ref watch);

            }
        }

        private void CreateWebRequest(string URL, string body, RequestMethod method, Action<int, string> OnResult = null)
        {
            if (string.IsNullOrEmpty(URL)) throw new ArgumentNullException(nameof(URL));

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                var token = DiscordAPI2.Call<string>("GenerateToken", "", sb.Clear().Append("bansystem_prism_").Append(ServerId.ToLower()).ToString());
                var headers = new Dictionary<string, string>
                {
                    { "User-Agent",  sb.Clear().Append(Name).Append(" ").Append(ServerId).Append("/").Append(Version).ToString() },
                    { "Authorization", token },
                    { "Server", "PRISM " + ServerId }
                };

                if (method != RequestMethod.GET && !string.IsNullOrEmpty(body)) headers.Add("Content-Type", "application/json");

                webrequest.Enqueue(URL, body, (c, response) =>
                {
                    if (OnResult != null) OnResult.Invoke(c, response);
                }, this, method, headers, 10);
            }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        //we convert the object to JSON and then back to Dictionary<string, object>
        //then we copy only properties that are needed
        //:pepega:
        private Dictionary<string, object> StripBan(BanInfo ban, List<string> fields)
        {
            if (ban == null) throw new ArgumentNullException(nameof(ban));

            var rv = new Dictionary<string, object>();
            var converted = ConvertBan(ban);

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                rv[field] = converted[field];
            }

            return rv;
        }

        private Dictionary<string, object> ConvertBan(BanInfo ban)
        {
            return Utility.ConvertFromJson<Dictionary<string, object>>(Utility.ConvertToJson(ban));
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

        private bool updating = false;
        private bool needs_updating = false;

        private void UpdateBanList()
        {
            if (updating)
            {
                needs_updating = true;
                return;
            }

            updating = true;

            CreateWebRequest("https://prismrust.com/api/rust/bans?format=dictionary", null, RequestMethod.GET, (code, response) =>
            {
                timer.Once(1f, () =>
                {
                    updating = false;
                    if (needs_updating) UpdateBanList();
                    needs_updating = false;
                });

                if (code >= 400)
                {
                    PrintWarning("Received a non-200 when fetching ban list!");
                    return;
                }

                Dictionary<string, BanInfo> new_bans = null;
                try { new_bans = Utility.ConvertFromJson<Dictionary<string, BanInfo>>(response); }
                catch (Exception ex)
                {
                    PrintError("Got an exception while refreshing ban list!" + Environment.NewLine + ex.ToString());
                    PrintError(response);
                    return;
                }

                if (new_bans?.Count < 1)
                {
                    PrintWarning("new_bans count is 0, not updating!");
                    return;
                }

                if (new_bans.Count != _banData.banInfos.Count)
                {
                    try
                    {
                        foreach (var kvp in new_bans)
                        {
                            var banId = kvp.Key;
                            var info = kvp.Value;

                            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                            {
                                var ply = BasePlayer.activePlayerList[i];
                                if (ply?.UserIDString == banId)
                                {
                                    ply.Kick("Banned: " + info?.BanReason);
                                }
                            }
                        }

                        PrintWarning("Ban count mismatch! new count: " + new_bans.Count.ToString("N0") + " and previous: " + _banData.banInfos.Count.ToString("N0"));
                    }
                    catch (Exception ex) { PrintError(ex.ToString()); }
                }

                _banData.banInfos = new_bans;
                SaveData();
            });
        }

        private List<string> CovalenceBanIDs
        {
            get
            {
                var covBanned = new List<string>();
                try { foreach (var player in covalence.Players.All) if (player != null && (player?.IsBanned ?? false)) covBanned.Add(player.Id); }
                catch (Exception ex) { PrintError(ex.ToString()); }
                return covBanned;
            }
        }

        private List<BanInfo> GetBanInfos(string userIDorIp)
        {
            if (string.IsNullOrEmpty(userIDorIp)) return null;

            var foundDatas = new List<BanInfo>();

            foreach (var kvp in _banData.banInfos)
            {
                var banData = kvp.Value;
                if (kvp.Key == userIDorIp || banData.HasIP(userIDorIp)) foundDatas.Add(banData);
            }


            return foundDatas;
        }


        private string GetUserIDFromBanInfo(BanInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            foreach (var kvp in _banData.banInfos)
            {
                if (kvp.Value == info)
                    return kvp.Key;
            }

            return string.Empty;
        }

        private BanInfo GetBanInfoByID(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            BanInfo info;
            if (_banData.banInfos.TryGetValue(userId, out info)) return info;

            return null;
        }

        private BanInfo GetBanInfoByNameOrID(string nameOrUserID, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(nameOrUserID)) return null;

            BanInfo info;
            if (_banData.banInfos.TryGetValue(nameOrUserID, out info)) return info;

            var foundDatas = Facepunch.Pool.GetList<BanInfo>();
            try
            {

                foreach (var kvp in _banData.banInfos)
                {
                    var banData = kvp.Value;

                    if (banData.DisplayName.Equals(nameOrUserID, stringComparison)) foundDatas.Add(banData);
                }


                return foundDatas.Count == 1 ? foundDatas[0] : null;
            }
            finally { Facepunch.Pool.FreeList(ref foundDatas); }
        }

        private BanInfo GetBanInfoByIP(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) throw new ArgumentNullException(nameof(ipAddress));

            var foundDatas = Facepunch.Pool.GetList<BanInfo>();
            try
            {

                foreach (var kvp in _banData.banInfos)
                {
                    var banData = kvp.Value;
                    if (banData.IPAddresses.Contains(ipAddress)) foundDatas.Add(banData);
                }


                return foundDatas.Count == 1 ? foundDatas[0] : null;
            }
            finally { Facepunch.Pool.FreeList(ref foundDatas); }
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("BanSystem", _banData);

        private void SendReply(IPlayer player, string msg, string chatUserId = "", bool keepTagsConsole = false)
        {
            if (player == null) return;

            msg = !player.IsServer ? msg : keepTagsConsole ? msg : RemoveTags(msg);

            if (player.IsServer)
            {
#if RUST
                if (ConsoleSystem.CurrentArgs != null) ConsoleSystem.CurrentArgs.ReplyWith(msg);
                else player.Reply(msg);
#else
                player.Reply(msg);
#endif
            }
            else
            {
#if RUST
                player.Command("chat.add", string.Empty, chatUserId, msg);
#else
                player.Reply(msg);
#endif
            }
        }

        private int BannedCount { get { return _banData?.banInfos?.Count ?? 0; } }

        private int GetBannedCount() { return BannedCount; }

        private int GetBannedCountWithIPOnlyBans()
        {
            var count = _banData?.banInfos?.Count ?? 0;
            var ipsBanned = 0;


            BanInfo ipOnlyBans;
            if (_banData.banInfos.TryGetValue("0", out ipOnlyBans))
            {
                ipsBanned += ipOnlyBans?.IPAddresses?.Count ?? 0;
            }

            return count + ipsBanned + ((ipsBanned > 0) ? -1 : 0);
        }

        private bool IsBannedP(IPlayer player) { return (player != null) ? IsBanned(player.Id) : false; }

        private bool IsBanned(string userID, bool checkIP = false, bool checkCov = true)
        {
            if (string.IsNullOrEmpty(userID)) return false;

            try
            {
                var covBanned = false;
                if (checkCov)
                {
                    covBanned = covalence.Players?.FindPlayerById(userID)?.IsBanned ?? false;
                    if (covBanned) return true;
                }

                if (_banData?.banInfos == null || _banData.banInfos.Count < 1) return false;

                BanInfo info;
                if (_banData.banInfos.TryGetValue(userID, out info)) return true;

                if (checkIP)
                {
                    var ipString = getIPString(userID);

                    foreach (var kvp in _banData.banInfos)
                    {
                        if (kvp.Value.IsIPBanned(ipString)) return true;
                    }
                }

                return covBanned;
            }

            catch (Exception ex) { PrintError(ex.ToString()); }
            return false;
        }

        private BanInfo NewBanInfo(string userID, bool ipBan = false, DateTime? endDate = null, string banIssuer = "")
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));

            if (GetBanInfoByID(userID) != null)
            {
                PrintWarning("NewBanInfo called on userID that already has a ban info!! " + userID);
                return null;
            }


            var newBan = new BanInfo(endDate, ipBan);

            if (newBan != null)
            {
                var ipStr = getIPString(userID);
                if (!newBan.IPAddresses.Contains(ipStr) && !string.IsNullOrEmpty(ipStr)) newBan.IPAddresses.Add(ipStr);
                newBan.BannedIssuer = banIssuer;
            }

            BanInfo info;
            if (_banData.banInfos.TryGetValue(userID, out info)) PrintWarning("NewBanInfo being called on an ID that already has a BanInfo! ID: " + userID + ", info: " + info);

            _banData.banInfos[userID] = newBan;
            return newBan;
        }

        private BanInfo GetIPBanInfo(string IP, DateTime? endDate = null, string banIssuer = "")
        {
            if (string.IsNullOrEmpty(IP)) return null;

            BanInfo ipOnlyBans;
            if (!_banData.banInfos.TryGetValue("0", out ipOnlyBans))
            {
                var newBan = new BanInfo(IP, endDate)
                {
                    BannedIssuer = banIssuer
                };
                _banData.banInfos["0"] = newBan;
                return newBan;
            }

            if (!ipOnlyBans.IPAddresses.Contains(IP)) ipOnlyBans.IPAddresses.Add(IP);

            return ipOnlyBans;
        }

        private BanInfo NewBanInfo(IPlayer player, bool ipBan = false, DateTime? endDate = null, string banIssuer = "")
        {
            if (player != null) return NewBanInfo(player?.Id ?? string.Empty, ipBan, endDate, banIssuer);
            return null;
        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));

            try
            {
                var p = covalence.Players.FindPlayer(nameOrIdOrIp);
                if (p != null) if (p.IsConnected || tryFindOfflineIfNoOnline) return p;


                var players = Facepunch.Pool.GetList<IPlayer>();
                try
                {
                    var connected = covalence.Players.Connected;
                    foreach (var player in connected)
                    {
                        var IP = player?.Address ?? string.Empty;
                        var name = player?.Name ?? string.Empty;
                        var ID = player?.Id ?? string.Empty;

                        if (ID.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || IP == nameOrIdOrIp || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(player);
                    }

                    if (players.Count < 1 && tryFindOfflineIfNoOnline)
                    {
                        foreach (var offlinePlayer in covalence.Players.All)
                        {
                            if (offlinePlayer.IsConnected) continue;

                            var IP = getIPString(offlinePlayer);
                            var name = offlinePlayer?.Name ?? string.Empty;
                            var ID = offlinePlayer?.Id ?? string.Empty;

                            if (ID.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || IP == nameOrIdOrIp || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase)) players.Add(offlinePlayer);
                        }
                    }

                    return players.Count == 1 ? players[0] : null;
                }
                finally { Facepunch.Pool.FreeList(ref players); }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString() + " ^ FindConnectedPlayer ^ ");
                return null;
            }
        }

        private string getIPString(IPlayer player)
        {
            if (player == null) return string.Empty;

            var IP = string.Empty;
            if (player?.IsConnected ?? false) IP = player?.Address ?? string.Empty;
            if (string.IsNullOrEmpty(IP)) IP = getIPString(player.Id);

            return IP;
        }

        private string getIPString(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;

            var strIP = (AAIPDatabase != null && AAIPDatabase.IsLoaded) ? AAIPDatabase?.Call<string>("GetLastIP", userID) ?? string.Empty : string.Empty;
            if (!string.IsNullOrEmpty(strIP)) return strIP;

            var ply = covalence.Players?.FindPlayerById(userID) ?? null;
            return (ply != null && ply.IsConnected) ? ply.Address : string.Empty;
        }

        private string getIPString(Network.Connection connection)
        {
            var IP = connection?.ipaddress ?? string.Empty;
            if (!string.IsNullOrEmpty(IP) && IP.Contains(':')) IP = IP.Split(':')[0];
            return IP;
        }

        private void UnbanPlayer(string userID, bool send = true)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));

            var banData = GetBanInfoByID(userID);
            if (banData == null) return;

            var player = covalence.Players?.FindPlayerById(userID) ?? null;


            bool showMsgLink;
            if (_banMsgLink.TryGetValue(userID, out showMsgLink)) _banMsgLink[userID] = false;

            if (player != null && player.IsBanned) player.Unban();

            _banData?.banInfos?.Remove(userID);

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                if (send)
                {
                    CreateWebRequest(sb.Clear().Append("https://prismrust.com/api/rust/bans/").Append(userID).ToString(), null, RequestMethod.DELETE, (code, response) =>
                    {
                        if (code >= 400) Puts("Failed to send unban for " + userID + " to API (" + code + ": " + response + ")");
                        else
                        {
                            Interface.Oxide.CallHook("OnBanSystemUnban", userID);
                            //update timer.Once(2f, UpdateBanList);
                        }
                    });
                }

                PrintWarning(sb.Clear().Append("Unbanned: ").Append(player?.Name ?? "Unknown").Append(" (").Append(userID).Append(")").ToString());
            }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private void BanPlayer(string userID, string reason = "No Reason Given", bool ipBan = true, DateTime? endTime = null, string banIssuer = "", bool kick = true, string kickReason = "", string displayName = "", bool silent = false, bool shared = false)
        {
            if (string.IsNullOrEmpty(userID) || IsBanned(userID, false, false)) return;

            var player = covalence.Players?.FindPlayerById(userID) ?? null;

            BanInfo newBan;

            if (endTime != null && endTime != DateTime.MinValue) newBan = NewBanInfo(userID, ipBan, endTime, banIssuer);
            else newBan = NewBanInfo(userID, ipBan);

            if (newBan != null)
            {
                newBan.BanReason = reason;
                newBan.DisplayName = displayName ?? player?.Name ?? string.Empty;
                newBan.BannedIssuer = banIssuer;
            }
            else PrintWarning("newBan is null after created?!");

            if (!shared)
            {
                SendBanRequest(userID, player, reason, newBan, silent, name => {
                    if (!silent)
                    {
                        AnnounceBan(name, reason, endTime == null || !endTime.HasValue ? TimeSpan.MinValue : endTime.Value - DateTime.UtcNow);
                        AnnounceTotalBan();
                    }
                });
            }

            if (kick && player != null && (player?.IsConnected ?? false))
            {
                player.Command("echo <color=#9841f4>You can appeal your ban here:</color><color=#41aff4>discord.gg/DUCnZhZ</color>.");

                var sb = Facepunch.Pool.Get<StringBuilder>();
                try
                {
                    player.Kick(sb.Clear().Append(!string.IsNullOrEmpty(kickReason) ? kickReason : reason).Append(" - APPEAL: discord.gg/DUCnZhZ").ToString());
                }
                finally { Facepunch.Pool.Free(ref sb); }
            }

            Interface.Oxide.CallHook("OnBanSystemBan", userID, reason, ipBan, endTime != null ? endTime : DateTime.MinValue, banIssuer, kick, kickReason, displayName, silent, shared);
        }

        private void BanIP(string IP, string reason = "No Reason Given", DateTime? endTime = null, string banIssuer = "", bool kick = true, string kickReason = "", string displayName = "", bool shared = false)
        {
            if (IsIPBanned(IP))
            {
                PrintWarning("BanIP called on IP that's already banned: " + IP);
                return; //already IP banned
            }

            var ipOnlyBans = GetIPBanInfo(IP, endTime, banIssuer);

            ipOnlyBans.BanReason = reason;
            ipOnlyBans.DisplayName = displayName;
            ipOnlyBans.BannedIssuer = banIssuer;


            if (!shared) UpdateBan("0", new { IPs = ipOnlyBans.IPAddresses });

            if (kick)
            {
                foreach (var ply in covalence.Players.Connected)
                {
                    if (ply?.Address == IP) ply.Kick(kickReason ?? reason);
                }
            }

            Interface.Oxide.CallHook("OnBanSystemBanIP", IP, reason, endTime, banIssuer, kick, kickReason, displayName);
        }

        private bool IsIPBanned(string IP)
        {
            if (_banData?.banInfos != null)
            {
                foreach (var kvp in _banData.banInfos)
                {
                    if (kvp.Value.IsIPBanned(IP)) return true;
                }
            }

            return false;
        }

        private bool canExecute(IPlayer player, string perm)
        {
            return (player?.Id ?? string.Empty).Equals("server_console", StringComparison.OrdinalIgnoreCase) || permission.UserHasPermission(player.Id, "bansystem." + perm);
        }

        private string GetNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            if (userID == "server_console") return userID;

            return covalence.Players?.FindPlayerById(userID)?.Name ?? string.Empty;
        }

        private IPlayer FindIPlayer(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var players = covalence.Players?.FindPlayers(name);
            var count = players.Count();
            return count == 1 ? players.FirstOrDefault() : null;
        }

        private string GetBanString(bool reverseOrder = false)
        {
            var watch = Facepunch.Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                if ((_banData?.banInfos?.Count ?? 0) < 1) return string.Empty;

                var banSB = Facepunch.Pool.Get<StringBuilder>();

                try
                {
                    banSB.Clear().AppendLine("banlistex").Append(Environment.NewLine); //for filtering

                    var list = !reverseOrder ? _banData.banInfos : _banData.banInfos.ToDictionary(p => p.Key, p => p.Value);
                    if (reverseOrder) list.Reverse();

                    var desCount = reverseOrder ? list.Count : 0;
                    var i = 0;
                    foreach (var kvp in list)
                    {
                        var banId = kvp.Key;
                        if (banId == "0") continue;

                        var banData = kvp.Value;
                        banSB.Append(reverseOrder ? desCount : i).Append(" ").Append(banId).Append(" \"").Append(banData?.DisplayName ?? GetNameFromID(banId)).Append("\" \"").Append(banData?.BanReason ?? string.Empty).Append("\"\n");
                        if (reverseOrder) desCount--;
                        i++;
                    }

                    if (banSB.Length > 1) banSB.Length -= 1;

                    var ret = banSB.ToString();
                    watch.Stop();

                    var timeTaken = watch.Elapsed;
                    timer.Once(0.1f, () =>
                    {
                        PrintWarning("GetBanString took: " + timeTaken.TotalMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms"); //this has to be delayed, because if not, you'll "trick" the rcon into receiving this as the message instead of the ban string
                    });
                    return ret;
                }
                finally { Facepunch.Pool.Free(ref banSB); }
            }
            finally { Facepunch.Pool.Free(ref watch); }
        }

        private string GetBanStringLatest(int max)
        {
            if (max < 1) return string.Empty;

            if ((_banData?.banInfos?.Count ?? 0) < 1) return string.Empty;

            var banSB = new StringBuilder("banlistex" + Environment.NewLine); //for filtering
            var dictionary = _banData.banInfos.ToDictionary(p => p.Key, p => p.Value);
            dictionary.Reverse();
            var desCount = dictionary.Count;

            var i = 0;
            foreach (var kvp in dictionary)
            {
                var banId = kvp.Key;
                if (banId == "0") continue;

                var banData = kvp.Value;

                if (i >= max) break;
                banSB.AppendLine(desCount + " " + banId + " \"" + (banData?.DisplayName ?? GetNameFromID(banId)) + "\"" + " " + "\"" + (banData?.BanReason ?? string.Empty) + "\"");
                desCount--;
                i++;
            }

            return banSB.ToString().TrimEnd();
        }

        private void WS_Ban(string UID, bool IPBanned, string Issuer, string Reason, string end, bool silent = true, string Name = "")
        {
            PrintWarning("WS_Ban called");
            if (IsBanned(UID)) return;
            PrintWarning("WS_Ban 1st check passed, calling BanPlayer");
            BanPlayer(UID, Reason, IPBanned, string.IsNullOrEmpty(end) ? DateTime.MinValue : DateTime.Parse(end), Issuer, true, "", "", silent, true);
            if (!silent)
            {
                PrintWarning("Announcing ban");
                AnnounceBan(Name ?? UID, Reason, TimeSpan.MinValue);
                PrintWarning("Announcing total ban");
                AnnounceTotalBan();
            }

            //update UpdateBanList();
            timer.In(UnityEngine.Random.Range(1f, 10f), FillBanIPs);
        }

        private void WS_Ban_Update() => UpdateBanList();

        private void WS_Unban(string UID)
        {
            if (!IsBanned(UID)) return;

            UnbanPlayer(UID, false);
            //update UpdateBanList();
        }

        private string GetBanJson()
        {
            return JsonConvert.SerializeObject(_banData.banInfos, Formatting.Indented);
        }

        private void UpdateBan(string id, object fields)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                CreateWebRequest(sb.Clear().Append("https://prismrust.com/api/rust/bans/").Append(id).ToString(), Utility.ConvertToJson(fields), RequestMethod.PATCH, (code, response) =>
                {
                    if (code >= 400)
                    {
                        sb = Facepunch.Pool.Get<StringBuilder>();
                        try
                        {
                            PrintWarning(sb.Clear().Append("Failed to send ban data update to API (").Append(code).Append(": ").Append(response).Append(")").ToString());
                            _apiFailTimes++;
                            if (_apiFailTimes >= 10)
                            {
                                covalence.Server.Command(sb.Clear().Append("o.reload \"").Append(Name).Append("\"").ToString());
                                return;
                            }
                            else timer.In(20f, () => UpdateBan(id, fields));
                        }
                        finally { Facepunch.Pool.Free(ref sb); }
                    }
                    else _apiFailTimes = 0;
                });
            }
            finally { Facepunch.Pool.Free(ref sb); }

        }

        private void SendBansUpdateRequest(Dictionary<string, BanInfo> payload, List<string> fields)
        {
            if (payload.Count < 1) return;

            if (payload.Count == 1)
            {
                var kvp = payload.First();
                var obj = StripBan(kvp.Value, fields);

                UpdateBan(kvp.Key, obj);
                return;
            }

            //we convert the list to a Dictionary where key is the ban UID and the value is the modified field(s)
            var dictionary = new Dictionary<string, Dictionary<string, object>>();

            foreach (var item in payload)
            {
                dictionary[item.Key] = StripBan(item.Value, fields);
            }

            CreateWebRequest("https://prismrust.com/api/rust/bans", Utility.ConvertToJson(dictionary), RequestMethod.PATCH, (code, response) =>
            {
                if (code >= 400)
                {
                    PrintWarning("Failed to send bulk ban update to API (" + code + ": " + response + ")");
                    _apiFailTimes++;
                    if (_apiFailTimes >= 10)
                    {
                        covalence.Server.Command("o.reload \"" + Name + "\"");
                        return;
                    }
                    else timer.In(20f, () => SendBansUpdateRequest(payload, fields));
                    return;
                }

                if (code == 204) return;

                try
                {
                    timer.Once(2f, UpdateBanList);
                    var result = Utility.ConvertFromJson<Dictionary<string, List<string>>>(response);

                    if (result["success"].Count == dictionary.Count) return;

                    var failed = result["failed"];

                    PrintWarning("Failed to ban " + failed.Count + " bans\n" + string.Join("\n", failed));
                }
                catch (Exception e)
                {
                    PrintWarning("[BSDebug] exception: " + e.Message);
                }
            });
        }

        private void SendBanRequest(string userID, IPlayer player, string reason, BanInfo ban, bool silent, Action<string> name)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                var body = ConvertBan(ban);
                body["UID"] = userID;

                CreateWebRequest(sb.Clear().Append("https://prismrust.com/api/rust/bans").Append(silent ? "?silent=true" : string.Empty).ToString(), Utility.ConvertToJson(body), RequestMethod.POST, (code, response) => {
                    if (code >= 400)
                    {
                        Puts("Failed to ban " + userID + " (" + code + ": " + response + ")");
                        timer.In(20f, () => SendBanRequest(userID, player, reason, ban, silent, name));
                        return;
                    }
                    
                    timer.Once(2f, UpdateBanList);

                    Dictionary<string, object> name_response = new Dictionary<string, object>();

                    try { name_response = Utility.ConvertFromJson<Dictionary<string, object>>(response); }
                    catch (Exception e)
                    {
                        PrintError("Failed to complete banning: " + e.Message + "\n" + response);
                        name.Invoke(userID);
                        return;
                    }

                    if (name_response != null && name_response.ContainsKey("name") && !string.IsNullOrEmpty(name_response["name"].ToString())) name.Invoke(name_response["name"].ToString());
                    else name.Invoke(userID);
                });
            }
            finally { Facepunch.Pool.Free(ref sb); }
        }

        private void ExportBanString(string fileName, bool newestFirst = false)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            LogToFile(fileName, GetBanString(newestFirst).Replace("banlistex", string.Empty), this, false);
        }

        private void SimpleBroadcast(string msg, string chatUserId = "")
        {
            if (string.IsNullOrEmpty(msg)) throw new ArgumentNullException(nameof(msg));
            foreach (var player in covalence.Players.Connected)
            {
                if (!player.IsConnected)
                {
                    PrintWarning(player.Id + " was not connected when looping through covalence.Players.Connected!!!");
                    continue;
                }
                SendReply(player, msg, chatUserId);
            }
        }

        private string GetBanInfoJSON(string id)
        {
            var ban = GetBanInfoByID(id);
            if (ban != null) return Utility.ConvertToJson(ban);
            return "{}";
        }

        #endregion
    }
}
