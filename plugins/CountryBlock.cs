/*
TODO:
- Add command to check player's country, kick/ban optional - seems useless
- Add optional customizable message when players connect - dude what
- Add notices about local IP, admin being excluded, etc. - useless
- Add support for country, country code, and Steam ID in messages - dude what
*/

using System;
using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using System.Net;
using System.Text.RegularExpressions;

namespace Oxide.Plugins
{
    [Info("CountryBlock", "Shady/MBR", "2.1.0", ResourceId = 1920)]
    [Description("Block or allow players only from configured countries")]
    internal class CountryBlock : CovalencePlugin
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
        [PluginReference]
        private readonly Plugin DiscordAPI2, PRISMID;

        [PluginReference]
        private readonly Plugin SteamAuthOverride;

        #region Initialization

        private const string permBypass = "countryblock.bypass";

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

        private class IPInfo
        {
            public string IP { get; set; } = string.Empty;
            public string CountryCode { get; set; } = string.Empty;

            [JsonRequired]
            private string _time = string.Empty;
            //>not using ISO format :joy:
            [JsonIgnore]
            public DateTime LastCheck
            {
                get
                {
                    DateTime val;
                    DateTime.TryParse(_time, out val);
                    return val;
                }
                set
                {
                    _time = value.ToString();
                }
            }
        }

        private class StoredData
        {
            public List<IPInfo> IPInfos = new List<IPInfo>();
        }

        private StoredData data;
        private Action UpdateAction = null;

        private void Init()
        {
            UpdateAction = new Action(() =>
            {
                if (DiscordAPI2 == null || !DiscordAPI2.IsLoaded)
                {
                    PrintWarning("DiscordAPI2 is not loaded!");
                    return;
                }

                try
                {
                    var oldCount = countryList?.Count ?? -1;
                    var blocked = DiscordAPI2?.Call<List<string>>("GetBlockedCountries") ?? null;
                    if (blocked == null)
                    {
                        PrintWarning("Got null list from DiscordAPI2.GetBlockedCountries()");
                        return;
                    }

                    countryList = new List<object>();
                    for (int i = 0; i < blocked.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(blocked[i])) countryList.Add(blocked[i]);
                    }

                    var newCount = countryList?.Count ?? -1;
                    if (oldCount != newCount) Puts("Updated countryList, new count: " + newCount + ", old: " + oldCount);
                }

                catch (Exception ex)
                {
                    PrintError(ex.ToString());
                    PrintWarning("Failed to complete UpdateAction()");
                }
            });

            LoadDefaultConfig();
            permission.RegisterPermission(permBypass, this);
            lang.RegisterMessages(new Dictionary<string, string> { ["NotAllowed"] = "This server doesn't allow players from {0}" }, this);
            AddCovalenceCommand("cblock", "CountryBlockCMD");

            data = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("CountryBlockCache") ?? new StoredData();
            timer.Once(4f, () => UpdateAction?.Invoke());
            timer.Every(45f, () => UpdateAction?.Invoke());
        }

        private void Unload()
        {
            Interface.Oxide.DataFileSystem.WriteObject("CountryBlockCache", data);
        }

        #endregion

        #region Configuration

        private bool adminExcluded;
        private bool banInstantly;
        private List<object> countryList;
        private bool whitelist;

        protected override void LoadDefaultConfig()
        {
            Config["AdminExcluded"] = adminExcluded = GetConfig("AdminExcluded", true);
            Config["BanInstantly"] = banInstantly = GetConfig("BanInstantly", false);
            Config["CountryList"] = countryList = GetConfig("CountryList", new List<object> { "CN", "RU" });
            Config["Whitelist"] = whitelist = GetConfig("Whitelist", false);
            SaveConfig();
        }

        #endregion

        #region Country Lookup
        private void GetCountry(string ip, Action<string> callback)
        {
            var token = DiscordAPI2.Call<string>("GenerateToken", "", "countryblock_prism_" + ServerId.ToLower());

            var headers = new Dictionary<string, string>
            {
                { "User-Agent",  Name + " " + ServerId + "/" + Version },
                { "Authorization", token }
            };

            var url = "https://prismrust.com/api/ipinfo/" + ip + "?simple=country";
            webrequest.Enqueue(url, null, (code, response) =>
            {
                if (code != 200 || string.IsNullOrEmpty(response))
                {
                    PrintWarning($"Getting country for {ip} failed! ({code})");
                    callback?.Invoke(null);
                    return;
                }

                var country = response;

                if (string.IsNullOrEmpty(country))
                {
                    PrintWarning("Country is null!");
                    callback?.Invoke(null);
                    return;
                }

                if (country.Length != 2)
                {
                    PrintWarning("Got a response with length that is != 2 (received JSON?) (" + response + ")");
                    callback?.Invoke(null);
                    return;
                }

                IPInfo findCache = data?.IPInfos.Find(x => x.IP == ip);
                if (findCache == null)
                {
                    findCache = new IPInfo();
                    if (data?.IPInfos != null) data.IPInfos.Add(findCache);
                }

                findCache.IP = ip;
                findCache.CountryCode = country;
                findCache.LastCheck = DateTime.UtcNow;
                PrintWarning("Created cached country for IP: " + ip + " as: " + country);

                callback?.Invoke(country);
            }, this, Core.Libraries.RequestMethod.GET, headers, 10);
        }

        private void IsCountryBlocked(IPlayer player)
        {
            if (player == null) return;

            var ip = player.Address;
            if (string.IsNullOrEmpty(ip)) return;

            var pName = player.Name;
#if DEBUG
            PrintWarning($"Admin: {(adminExcluded && player.IsAdmin)}, Perm: {permission.UserHasPermission(player.Id, permBypass)}");
#endif

            if ((adminExcluded && player.IsAdmin) || permission.UserHasPermission(player.Id, permBypass)) return;

            IPInfo findCache = null;
            if (data?.IPInfos != null && data.IPInfos.Count > 0)
            {
                for (int i = 0; i < data.IPInfos.Count; i++)
                {
                    var info = data.IPInfos[i];
                    if (info?.IP == ip)
                    {
                        findCache = info;
                        break;
                    }
                }

                if (findCache != null && (DateTime.UtcNow - findCache.LastCheck).TotalDays < 2)
                {
                    var country = findCache.CountryCode;
                    if ((!countryList.Contains(country) && whitelist) || (countryList.Contains(country) && !whitelist))
                    {
                        PlayerRejection(player, country);
                    }

                    PrintWarning("Cached country: " + country + " for: " + findCache.IP);
                    return;
                }
            }

            GetCountry(ip, country =>
            {
                if (string.IsNullOrEmpty(country))
                {
                    PrintWarning("Failed to fetch country for " + ip);
                    return;
                }

                if ((!countryList.Contains(country) && whitelist) || (countryList.Contains(country) && !whitelist))
                {
                    PlayerRejection(player, country);
                }
            });
        }

#if RUST
        void IsCountryBlocked(Network.Connection connection)
        {
            if (connection == null) return;

            var authed = SteamAuthOverride?.Call<bool>("IsAuthed", connection.userid) ?? false;
           // var authed = connection?.CgsPlayer?.Authenticated ?? false;
            var ip = connection.ipaddress.Split(':')[0];

            if (string.IsNullOrEmpty(ip)) return;

            var pId = (connection?.userid ?? 0).ToString();
            var pName = connection?.username ?? string.Empty;

#if DEBUG
            PrintWarning($"Admin: {(adminExcluded && connection.authLevel != 0)}, Perm: {permission.UserHasPermission(pId, permBypass)}");
#endif

            if ((adminExcluded && connection.authLevel != 0) || permission.UserHasPermission(pId, permBypass)) return;

            IPInfo findCache = null;
            if (data?.IPInfos != null)
            {
                for (int i = 0; i < data.IPInfos.Count; i++)
                {
                    var info = data.IPInfos[i];
                    if (info.IP == ip)
                    {
                        findCache = info;
                        break;
                    }
                }
            }

            if (findCache != null && (DateTime.UtcNow - findCache.LastCheck).TotalDays < 2)
            {
                var country = findCache.CountryCode;

                for (int i = 0; i < countryList.Count; i++)
                {
                    var ct = countryList[i]?.ToString();
                    if (string.IsNullOrEmpty(ct)) continue;

                    var ctRep = ct;
                    if (ctRep.EndsWith("-c", StringComparison.OrdinalIgnoreCase))
                    {
                        if (authed) continue; //country is blocked, but only for 480 players it seems
                        else ctRep = ReplaceCaseInsensitive(ctRep, "-c", string.Empty);
                    }

                    if (!ctRep.Equals(country, StringComparison.OrdinalIgnoreCase)) continue; //not the users' country, continue until found (if found)

                    Network.Net.sv.Kick(connection, Lang("NotAllowed", pId, ct));
                    NextTick(() => ServerMgr.Instance.connectionQueue.RemoveConnection(connection));
                    Puts("Kicked: " + pName + " (" + pId + ") for blocked country: " + ct + ", auth status: " + authed + ", IP: " + ip);
                }

                PrintWarning("Cached country: " + country + " for: " + findCache.IP);
                return;
            }

            GetCountry(ip, country =>
            {
                if (string.IsNullOrEmpty(country))
                {
                    PrintWarning("Failed to fetch country for " + ip);
                    return;
                }

                for (int i = 0; i < countryList.Count; i++)
                {
                    var ct = countryList[i]?.ToString();
                    if (string.IsNullOrEmpty(ct)) continue;

                    var ctRep = ct;
                    if (ctRep.EndsWith("-c", StringComparison.OrdinalIgnoreCase))
                    {
                        if (authed) continue; //country is blocked, but only for 480 players it seems
                        else ctRep = ReplaceCaseInsensitive(ctRep, "-c", string.Empty);
                    }

                    if (!ctRep.Equals(country, StringComparison.OrdinalIgnoreCase)) continue; //not the users' country, continue until found (if found)

                    Network.Net.sv.Kick(connection, Lang("NotAllowed", pId, ct));
                    NextTick(() => ServerMgr.Instance.connectionQueue.RemoveConnection(connection));
                    PrintWarning("Kicked: " + pName + " (" + pId + ") for blocked country: " + ct + ", auth status: " + authed + ", IP: " + ip);
                }
            });
        }

#endif
        #endregion

        #region Player Rejection

        private void PlayerRejection(IPlayer player, string country)
        {
            if (banInstantly) player.Ban(Lang("NotAllowed", player.Id, country), TimeSpan.Zero);
            else player.Kick(Lang("NotAllowed", player.Id, country));
        }

        private void CountryBlockCMD(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 1)
            {
                player.Message("Use /cblock list or /cblock auth <playername>");
                return;
            }

            var arg0Lower = args[0].ToLower();

            if (arg0Lower == "list")
            {
                if (countryList == null || countryList.Count < 1)
                {
                    player.Message("No blocked countries!");
                    return;
                }

                var cbSB = new StringBuilder();
                for (int i = 0; i < countryList.Count; i++) cbSB.AppendLine(countryList[i].ToString() + ", ");

                player.Message("Blocked countries:" + Environment.NewLine + cbSB.ToString().TrimEnd().TrimEnd(", ".ToCharArray()));
                return;
            }

            if (arg0Lower == "test")
            {
                if (args.Length < 2)
                {
                    player.Message("You need to provide an IP address");
                    return;
                }

                IPAddress ip;
                if (!IPAddress.TryParse(args[1], out ip))
                {
                    player.Message(args[1] + " is not a valid IP");
                    return;
                }

                GetCountry(ip.ToString(), country => player.Message(ip + " is " + country));
                return;
            }

            if (arg0Lower == "auth")
            {
                if (args.Length < 2)
                {
                    player.Message("You must supply a user name/steamid!");
                    return;
                }

                var target = FindIPlayer(args[1]);
                if (target == null)
                {
                    player.Message("No player found: " + args[1]);
                    return;
                }

                if (permission.UserHasPermission(target.Id, permBypass))
                {
                    player.Message(target.Name + "/" + target.Id + " is already authorized!");
                    return;
                }

                permission.GrantUserPermission(target.Id, permBypass, this);
                player.Message(target.Name + "/" + target.Id + " has been authorized!");
                return;
            }

            player.Message("Invalid argument! " + "Use /cblock list or /cblock auth <playername>");
        }

#if RUST
        void CanClientLogin(Network.Connection connection)
        {
            if (connection == null) return;
        NextTick(() => IsCountryBlocked(connection));
        }
#else
        private void OnUserConnected(IPlayer player)
        {
            Puts("Not Rust? Using OnUserConnected hook with IF RUST ELSE check");
            IsCountryBlocked(player);
        }
#endif

        #endregion

        #region Helpers

        private T GetConfig<T>(string name, T value) => Config[name] == null ? value : (T)Convert.ChangeType(Config[name], typeof(T));

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        private IPlayer FindIPlayer(string nameOrIdOrIp, bool onlyConnected = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) return null;
            var p = covalence.Players.FindPlayer(nameOrIdOrIp);
            if (p != null) if ((!p.IsConnected && !onlyConnected) || p.IsConnected) return p;
            var connected = covalence.Players.Connected;
            List<IPlayer> players = new List<IPlayer>();
            foreach (var player in connected)
            {
                var IP = player?.Address ?? string.Empty;
                var name = player?.Name ?? string.Empty;
                var ID = player?.Id ?? string.Empty;
                if (name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || IP == nameOrIdOrIp || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || ID == nameOrIdOrIp) players.Add(player);
            }
            if (players.Count <= 0 && !onlyConnected)
            {
                foreach (var offlinePlayer in covalence.Players.All)
                {
                    if (offlinePlayer.IsConnected) continue;
                    var name = offlinePlayer?.Name ?? string.Empty;
                    var ID = offlinePlayer?.Id ?? string.Empty;
                    if (name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || ID == nameOrIdOrIp) players.Add(offlinePlayer);
                }
            }
            if (players.Count != 1) return null;
            else return players[0];
        }

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
        #endregion
    }
}