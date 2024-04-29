using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("OneCountry", "Shady", "0.2.0", ResourceId = 0)]
    [Description("Allow players only from one country")]
    internal class OneCountry : CovalencePlugin
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
        private const string permBypass = "onecountry.bypass";
        private const string APIUrl = "https://prismrust.com/api/ipinfo/";
        public StoredData data;

        [PluginReference]
        private Plugin DiscordAPI2, PRISMID;

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

        #region Classes
        public class CachedIPCountry
        {
            public string IP { get; set; }
            public string Country { get; set; }
            public DateTime Expiry { get; set; } = DateTime.MinValue;
            public bool Expired { get { return Expiry > DateTime.MinValue && Expiry <= DateTime.UtcNow; } }
            public CachedIPCountry() { }
            public CachedIPCountry(string ip, string country)
            {
                IP = ip;
                Country = country;
            }
            public CachedIPCountry(string ip, string country, TimeSpan expiry)
            {
                IP = ip;
                Country = country;
                Expiry = DateTime.UtcNow.Add(expiry);
            }
            public override string ToString()
            {
                return IP + "/" + Country + "/" + Expiry + "/" + Expired;
            }
        }

        public class StoredData
        {
            public Dictionary<string, string> firstCountry = new Dictionary<string, string>();
            public Dictionary<string, string> lastCountry = new Dictionary<string, string>();
            public List<CachedIPCountry> ipCache = new List<CachedIPCountry>();
            public int CleanupExpiredCaches()
            {
                if (ipCache != null && ipCache.Count > 0)
                {
                    var toRemove = new HashSet<CachedIPCountry>();
                    for (int i = 0; i < ipCache.Count; i++)
                    {
                        var cache = ipCache[i];
                        if (cache != null && cache.Expired) toRemove.Add(cache);
                    }
                    if (toRemove.Count > 0)
                    {
                        foreach (var cache in toRemove) ipCache.Remove(cache);
                        return toRemove.Count;
                    }
                }
                return 0;
            }
            public StoredData() { }
        }

        #endregion

        private void Init()
        {
            data = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("OneCountry") ?? new StoredData();
            LoadDefaultConfig();
            permission.RegisterPermission(permBypass, this);
        }

        private void Unload() => SaveData();

        private void OnServerSave() => timer.Once(3f, SaveData);

        private void SaveData()
        {
            var count = data?.CleanupExpiredCaches() ?? -1;
            if (count > 0) PrintWarning("Removed: " + count.ToString("N0") + " expired IP caches");

            Interface.Oxide.DataFileSystem.WriteObject("OneCountry", data);
        }

        private void CheckCountry(Network.Connection connection)
        {
            if (connection == null || !connection.active || connection.rejected) return;

            var ip = IpAddress(connection?.ipaddress);
            if (string.IsNullOrEmpty(ip))
            {
                PrintError("null/empty IP on connection!!!!!");
                return;
            }

            var pName = connection?.username ?? string.Empty;
            var userID = connection?.userid ?? 0;
            var userIDStr = userID.ToString();

            if (IsLocalIp(ip) || permission.UserHasPermission(userIDStr, permBypass))
            {
                PrintWarning("Local IP or has perm: " + ip + ", is local?: " + IsLocalIp(ip) + ", " + pName + ", " + userIDStr);
                return;
            }

            var cacheCountry = string.Empty;
            if (data?.ipCache != null && data.ipCache.Count > 0)
            {
                for (int i = 0; i < data.ipCache.Count; i++)
                {
                    var cache = data.ipCache[i];
                    if (!string.IsNullOrEmpty(cache.IP) && cache.IP == ip && !cache.Expired)
                    {
                        cacheCountry = cache.Country;
                        break;
                    }
                }
                data?.CleanupExpiredCaches(); //we run it here to prevent the possibility of adding duplicates because one was expired, so it is going to create a new non-expired instead of looking to see if an expired one can be updated
            }

            if (!string.IsNullOrEmpty(cacheCountry))
            {
                PrintWarning("Got cached country of: " + cacheCountry + " for IP: " + ip + ", for user: " + pName + " (" + userIDStr + ")");
                HandleCountry(connection, cacheCountry);
                return;
            }

            var requestURL = APIUrl + ip + "?simple=countryCode";

            var token = DiscordAPI2.Call<string>("GenerateToken", "", "onecountry_prism_" + ServerId);
            var headers = new Dictionary<string, string>
            {
                { "User-Agent",  this.Name + " " + ServerId + "/" + this.Version },
                { "Authorization", token }
            };

            webrequest.Enqueue(requestURL, null, (code, response) =>
            {
                if (connection == null || !connection.active || connection.rejected)
                {
                    PrintWarning("Connection is null, not active, or rejected on webrequest");
                    return;
                }

                if (code != 200 || string.IsNullOrEmpty(response))
                {
                    PrintWarning("Country webrequest failed, IP: " + ip + " (" + pName + ", " + userIDStr + ") returned with code: " + code + ", response: " + response);
                    timer.Once(3f, () => CheckCountry(connection));
                    return;
                }

                var country = response;

                PrintWarning("Country response was " + country + " for " + ip + " (" + pName + ", " + userIDStr + ")");

                if (response.Length > 3)
                {
                    PrintWarning("Got country response with length > 2! (" + response + ")");
                    return;
                }

                if (country.Equals("unknown", StringComparison.OrdinalIgnoreCase) || country.Equals("zz", StringComparison.OrdinalIgnoreCase))
                {
                    PrintWarning("Country is unknown: " + ip + ", " + pName + ", " + userIDStr);
                    return;
                }

                var cache = new CachedIPCountry(ip, country, TimeSpan.FromHours(48));
                data.ipCache.Add(cache);
                PrintWarning("Added cache: " + cache);
                HandleCountry(connection, country);
            }, this, Core.Libraries.RequestMethod.GET, headers, 10);
        }

        private void HandleCountry(Network.Connection connection, string country)
        {
            if (string.IsNullOrEmpty(country))
            {
                PrintWarning("HandleCountry called with a null/empty arg!");
                return;
            }

            if (country.Equals("unknown", StringComparison.OrdinalIgnoreCase) || country.Equals("zz", StringComparison.OrdinalIgnoreCase))
            {
                PrintWarning("HandleCountry called with: " + country + " (BAD)");
                return;
            }

            if (connection == null || !connection.active || connection.rejected)
            {
                PrintWarning("Connection is null, not active, or rejected on HandleCountry");
                return;
            }

            var ip = IpAddress(connection?.ipaddress);
            if (string.IsNullOrEmpty(ip))
            {
                PrintWarning("null/empty IP on connection (HandleCountry)!!!!!");
                return;
            }

            var uid = connection.userid.ToString();
            try
            {
                var pName = connection?.username ?? string.Empty;
                PrintWarning("HandleCountry: " + country + " for " + ip + " (" + pName + ", " + uid + ")");

                string firstCountry;

                data.lastCountry[uid] = country;
                if (!data.firstCountry.TryGetValue(uid, out firstCountry))
                {
                    PrintWarning(pName + " (" + uid + ", " + ip + ") had no first country. Setting to: " + country);
                    data.firstCountry[uid] = country;
                }
                else
                {
                    if (!country.Equals(firstCountry, StringComparison.OrdinalIgnoreCase))
                    {
                        Network.Net.sv.Kick(connection, "Invalid FCIP - discord.gg/DUCnZhZ");
                        NextTick(() => ServerMgr.Instance.connectionQueue.RemoveConnection(connection));
                        PrintWarning("Invalid FCIP: Kicking: " + pName + " (" + uid + ", " + ip + "), first country: " + firstCountry + ", new country: " + country);
                    }
                }
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^WebRequest^"); }
        }

        private void CanClientLogin(Network.Connection connection)
        {
            NextTick(() =>
            {
                if (connection == null || !connection.active || connection.rejected) return;

                CheckCountry(connection);
            });
        }

        private static bool IsLocalIp(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) throw new ArgumentNullException(nameof(ipAddress));
            var split = ipAddress.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            var ip = new[] { int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]) };
            return ip[0] == 10 || ip[0] == 127 || (ip[0] == 192 && ip[1] == 168) || (ip[0] == 172 && (ip[1] >= 16 && ip[1] <= 31));
        }

        private string GetFirstCountry(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            if (data?.firstCountry == null) return string.Empty;

            string str;
            if (data.firstCountry.TryGetValue(userID, out str)) return str;

            return string.Empty;
        }

        private string GetLastCountry(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            if (data?.firstCountry == null) return string.Empty;

            string str;
            if (data.lastCountry.TryGetValue(userID, out str)) return str;

            return string.Empty;
        }

        private static string IpAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip)) throw new ArgumentNullException(nameof(ip));
            return Regex.Replace(ip, @":{1}[0-9]{1}\d*", "");
        }

    }
}
