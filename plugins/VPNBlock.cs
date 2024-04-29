using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Oxide.Plugins
{
    [Info ("VPNBlock", "Shady/MBR", "0.0.9", ResourceId = 0/*/2115/*/)]
    class VPNBlock : CovalencePlugin
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
        List<string> allowedISPs = new List<string>();
        string APIKey;

        public const int CacheKeepDays = 21;

        public readonly string Discord = "discord.gg/DUCnZhZ";

        protected override void LoadDefaultConfig()
        {
            Config["APIKey"] = APIKey = GetConfig("APIKey", string.Empty);
            SaveConfig();
        }

        void Init()
        {
            LoadDefaultConfig();
            LoadData();
            LoadMessages();
            permission.RegisterPermission("vpnblock.canvpn", this);
            if (string.IsNullOrEmpty(APIKey)) PrintWarning("APIKey needs to be set in config!");
          
            if (CacheKeepDays > 0 && cacheData?.infos != null && cacheData.infos.Count > 0)
            {
                var watch = Stopwatch.StartNew();
                var now = DateTime.UtcNow;
                var newInfos = new List<CacheInfo>();
                for (int i = 0; i < cacheData.infos.Count; i++)
                {
                    var info = cacheData.infos[i];
                    var span = now - info.Time;
                    if (span.TotalDays <= CacheKeepDays) newInfos.Add(info);
                }
                if (newInfos.Count < cacheData.infos.Count)
                {
                    var remCount = cacheData.infos.Count - newInfos.Count;
                    cacheData.infos = newInfos;
                    PrintWarning("Removed " + remCount.ToString("N0") + " cached entries >= " + CacheKeepDays.ToString("N0") + " days old");
                    watch.Stop();
                    PrintWarning("Removed entries " + remCount.ToString("N0") + " in: " + watch.Elapsed.TotalMilliseconds + "ms");
                }
                
            }
         
        }
        CacheData cacheData;
        class CacheInfo
        {
            private string _time;
            public DateTime Time
            {
                get
                {
                    DateTime time;
                    if (DateTime.TryParse(_time, out time)) return time;
                    return DateTime.MinValue;
                }
                set { _time = value.ToString(); }
            }
            public bool IsVPN = false;
            public string IPAddress = string.Empty;
        }
        class CacheData
        {
            public List<CacheInfo> infos = new List<CacheInfo>();
            public CacheInfo Set(string ip, bool value, DateTime? time = null)
            {
                if (string.IsNullOrEmpty(ip)) return null;
                CacheInfo findInfo = null;
                for(int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];
                    if (info?.IPAddress == ip)
                    {
                        findInfo = info;
                        break;
                    }
                }
                if (findInfo == null)
                {
                    findInfo = new CacheInfo();
                    infos.Add(findInfo);
                }
                findInfo.IPAddress = ip;
                findInfo.IsVPN = value;
                if (time.HasValue && time > DateTime.MinValue) findInfo.Time = (DateTime)time;
                return findInfo;
            }
            public CacheInfo GetInfo(string ip)
            {
                if (string.IsNullOrEmpty(ip)) return null;
                CacheInfo findInfo = null;
                for(int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];
                    if (info?.IPAddress == ip)
                    {
                        findInfo = info;
                        break;
                    }
                }
                return findInfo;
            }
            public CacheData() { }
        }

        void LoadData()
        {
            allowedISPs = Interface.Oxide.DataFileSystem.ReadObject<List<string>>("vpnblock_allowedisp");
            cacheData = Interface.Oxide.DataFileSystem.ReadObject<CacheData>("vpnblock_cachedvpns2");
            if (cacheData == null) cacheData = new CacheData();
        }

        void SaveData()
        {
            if (allowedISPs != null && allowedISPs.Count > 0) Interface.Oxide.DataFileSystem.WriteObject ("vpnblock_allowedisp", allowedISPs);
            if(cacheData != null) Interface.Oxide.DataFileSystem.WriteObject("vpnblock_cachedvpns2", cacheData);
        }

        [Command ("wisp")]
        void WhiteListISP (IPlayer player, string command, string[] args)
        {
            if (!IsAllowed(player)) return;

            if (args.Length == 0)
            {
                player.Reply(GetMsg("WISP Invalid", player.Id));
                return;
            }

            allowedISPs.Add(string.Join(" ", args));

            player.Reply(GetMsg ("ISP Whitelisted", player.Id));
            SaveData();
        }

        [Command("vpncheck")]
        void ISPCheck(IPlayer player, string command, string[] args)
        {
            if (!IsAllowed(player)) return;
            if (string.IsNullOrEmpty(APIKey))
            {
                player?.Reply("APIKey is empty");
                return;
            }
            if (args.Length == 0)
            {
                player.Reply("args < 1");
                return;
            }
            player.Reply("args: " + args[0]);
            var ip = IpAddress(args[0]);
            var getInfo = cacheData?.GetInfo(ip) ?? null;
            var cacheVPN = getInfo?.IsVPN ?? false;
            if (cacheVPN && (DateTime.UtcNow - getInfo.Time).TotalHours < 24)
            {
                player.Reply("is cached VPN");
                return;
            }
            var url = string.Format("http://v2.api.iphub.info/ip/{0}", ip);
            webrequest.Enqueue(url, string.Empty, (code, response) => {
                if (code != 200 || string.IsNullOrEmpty(response))
                {
                    PrintError("Service temporarily offline, error code: " + code);
                    return;
                }
                var msg = GetMsg("Unauthorized");
                var banMsg = GetMsg("Is Banned");
                var respLower = response.ToLower();
                var isVpn = false;
                if (respLower.Contains("115.21.6") || respLower.Contains("softbank") || respLower.Contains("cloudflare") || respLower.Contains("europe srl") || respLower.Contains("m247") || respLower.Contains("m24seven") || respLower.Contains("cht company") || respLower.Contains("amanah") || respLower.Contains("ovh sas") || respLower.Contains("redstation") || respLower.Contains("dedicated server hosting") || respLower.Contains("hostus") || respLower.Contains("makonix") || respLower.Contains("marosnet") || respLower.Contains("euro loop") || respLower.Contains("ovh sas") || respLower.Contains("yhc") || respLower.Contains("powerhouse") || respLower.Contains("phmgmt") || respLower.Contains("ru-bis") || respLower.Contains("mchost-") || respLower.Contains("fiber grid") || respLower.Contains("psych") || respLower.Contains("pjsc") || respLower.Contains("vanish") || respLower.Contains("mannford") || respLower.Contains("mbo-net") || respLower.Contains("dsl.mbo") || respLower.Contains("rostelecom") || respLower.Contains("nova hosting") || respLower.Contains("earthlink") || respLower.Contains("sakhalin") || respLower.Contains("solnce") || respLower.Contains("tencent") || respLower.Contains("capitalonline") || respLower.Contains("data service") || respLower.Contains("cdsc") || respLower.Contains("ukrcom") || respLower.Contains("yesup") || respLower.Contains("yessup")) isVpn = true; 
                var output = JsonConvert.DeserializeObject<VPN>(response);
                if (output == null) PrintWarning("Failed to parse response as VPN object!");

                if ((output?.block ?? 0) > 0 || isVpn)
                {
                    player?.Reply("Is VPN!");
                    PrintWarning(respLower);
                }
                else
                {
                    PrintWarning("not vpn: " + Environment.NewLine + respLower);
                    player?.Reply("Not VPN!");
                }
                PrintWarning(respLower);

            }, this, RequestMethod.GET, new Dictionary<string, string> {
                {"X-Key", APIKey }
            });
            player.Reply("Finished");
        }

        [Command("vpncache")]
        void VPNCache(IPlayer player, string command, string[] args)
        {
            if (!IsAllowed(player)) return;
            if (string.IsNullOrEmpty(APIKey))
            {
                player?.Reply("APIKey is empty");
                return;
            }
            if (args.Length < 1)
            {
                player.Reply("args < 1");
                return;
            }
            player.Reply("args: " + args[0]);
            var ip = IpAddress(args[0]);
            var cache = cacheData?.GetInfo(ip) ?? null;
            if (cache == null)
            {
                player.Message("No data!");
                return;
            }
            cacheData.infos.Remove(cache);
            player.Message("Removed as cache");
        }

        void LoadMessages()
        {
            lang.RegisterMessages (new Dictionary<string, string>
            {
                {"Unauthorized", "Unauthorized.  ISP/VPN not permitted"},
                {"Is Banned", "{0} is trying to connect from proxy VPN/ISP {1}"},
                {"ISP Whitelisted", "ISP Whitelisted"},
                {"WISP Invalid", "Syntax Invalid. /wisp [ISP NAME]"},
            }, this);
        }

        bool IsAllowed(IPlayer player) { return player.IsAdmin; }
        

        bool hasAccess(IPlayer player, string permissionname) { return player.IsAdmin ? true : permission.UserHasPermission(player.Id, permissionname); }
        

        private class VPN
        {
            [JsonProperty("isp")]
            public string isp;

            [JsonProperty("block")]
            public int block;
        }

        void OnServerSave() => SaveData();

        void Unload() => SaveData();

        void OnUserConnected (IPlayer player)
        {
            if (player == null || string.IsNullOrEmpty(APIKey) || hasAccess(player, "vpnblock.canvpn")) return;
            var userId = player?.Id ?? string.Empty;
            var ip = IpAddress(player.Address);
            var getInfo = cacheData?.GetInfo(ip) ?? null;
         //   if (getInfo == null) getInfo = cacheData.Set(ip, false);
            var cacheVPN = getInfo?.IsVPN ?? false;
            var msg = "Invalid IPH";
            var banMsg = GetMsg("Is Banned");
            if (getInfo != null && getInfo.Time != DateTime.MinValue && (DateTime.UtcNow - getInfo.Time).TotalHours < 24)
            {
                if (cacheVPN)
                {
                    player.Kick(msg + " - " + Discord);
                    covalence.Server.Command("banbyid \"" + userId + "\" \"" + msg + "\" silent");
                    PrintWarning(banMsg, player.Name + "(" + player.Id + "/" + ip + ")", string.Empty);
                    PrintWarning("kick cacheVPN");
                }
               // PrintWarning("Returning with cache: " + (DateTime.UtcNow - getInfo.Time).TotalHours + " total hours, is VPN cache: " + cacheVPN);
                return;
            }

            var url = "http://v2.api.iphub.info/ip/" + ip;
            webrequest.Enqueue (url, string.Empty, (code, response) => 
            {
                if (code != 200 || string.IsNullOrEmpty(response))
                {
                    PrintError("Service temporarily offline, error code: " + code);
                    return;
                }
           
               // var msg = GetMsg("Unauthorized");
              
            
                var output = JsonConvert.DeserializeObject<VPN>(response);
                if (output == null) PrintWarning("Failed to parse response as VPN object!");
                var isVpn = (output?.block ?? 0) > 0;
                var respLower = response.ToLower();
                if (!isVpn)
                {
                  
                    if (respLower.Contains("115.21.6") || respLower.Contains("softbank") || respLower.Contains("cloudflare") || respLower.Contains("europe srl") || respLower.Contains("m247") || respLower.Contains("m24seven") || respLower.Contains("cht company") || respLower.Contains("amanah") || respLower.Contains("ovh sas") || respLower.Contains("redstation") || respLower.Contains("dedicated server hosting") || respLower.Contains("hostus") || respLower.Contains("makonix") || respLower.Contains("marosnet") || respLower.Contains("euro loop") || respLower.Contains("ovh sas") || respLower.Contains("yhc") || respLower.Contains("powerhouse") || respLower.Contains("phmgmt") || respLower.Contains("ru-bis") || respLower.Contains("mchost-") || respLower.Contains("fiber grid") || respLower.Contains("psych") || respLower.Contains("pjsc") || respLower.Contains("vanish") || respLower.Contains("mannford") || respLower.Contains("mbo-net") || respLower.Contains("dsl.mbo") || respLower.Contains("rostelecom") || respLower.Contains("nova hosting") || respLower.Contains("earthlink") || respLower.Contains("sakhalin") || respLower.Contains("solnce") || respLower.Contains("tencent") || respLower.Contains("capitalonline") || respLower.Contains("data service") || respLower.Contains("cdsc") || respLower.Contains("ukrcom") || respLower.Contains("yesup") || respLower.Contains("yessup") || respLower.Contains("anjiejie") || respLower.Contains("protonvpn") || respLower.Contains("blade sas") || respLower.Contains("packet exchange") || respLower.Contains("packetexchange") || respLower.Contains("fibergrid"))
                    {
                        isVpn = true;
                        PrintWarning("got VPN from respLower: " + respLower);
                    }

                }
                if (respLower.Contains("nvidia") || respLower.Contains("geforce"))
                {
                    isVpn = false;
                    PrintWarning("got nvidia/geforce ip, setting vpn to false");
                }



                if (isVpn)
                {
                    player.Kick(msg + " - " + Discord);
                    covalence.Server.Command("banbyid \"" + userId + "\" \"" + msg + "\" silent");
                    PrintWarning(banMsg, player.Id, (output?.isp ?? string.Empty));
                    PrintWarning(banMsg, player.Name + "(" + player.Id + "/" + ip + ")", string.Empty);
                }
                cacheData.Set(ip, isVpn, DateTime.UtcNow);

            }, this, RequestMethod.GET, new Dictionary<string, string> {
                {"X-Key", APIKey }
            });
        }

        string IpAddress(string ip) { return Regex.Replace(ip, @":{1}[0-9]{1}\d*", ""); }

        string GetMsg(string key, object userID = null) { return lang.GetMessage(key, this, userID?.ToString()); }

        T GetConfig<T>(string name, T defaultValue) { return Config[name] == null ? defaultValue : (T)Convert.ChangeType(Config[name], typeof(T)); }
    }
}
