using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Steam Bans API", "Shady", "1.0.0", ResourceId = 1711)]
    [Description("API for getting player's steam bans")]
    class SteamBansAPI : CovalencePlugin
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
        #region Config/Init

        string APIKey => GetConfig("APIKey", string.Empty);

        bool CheckSleepingPlayers => GetConfig("CheckOfflinePlayers", false);
        bool CheckPlayersAfterReload => GetConfig("CheckPlayersAfterReload", false);
        bool CheckEveryConnection;

        Dictionary<string, Dictionary<string, object>> playerBans = new Dictionary<string, Dictionary<string, object>>();
        Dictionary<string, string> playerBansRaw = new Dictionary<string, string>();

        JsonSerializerSettings jsonsettings = new JsonSerializerSettings();


        /*--------------------------------------------------------------//
		//			Load up the default config on first use				//
		//--------------------------------------------------------------*/
        protected override void LoadDefaultConfig()
        {
            Config["APIKey"] = APIKey;
            Config["CheckPlayersAfterReload"] = CheckPlayersAfterReload;
            Config["CheckOfflinePlayers"] = CheckSleepingPlayers;
            Config["CheckEveryConnection"] = CheckEveryConnection = GetConfig("CheckEveryConnection", false);
            SaveConfig();
        }

        StoredData storedData;

        class StoredData
        {
            public Dictionary<string, Dictionary<string, object>> playerBans = new Dictionary<string, Dictionary<string, object>>();
            public Dictionary<string, string> lastCheckTime = new Dictionary<string, string>();
            public Dictionary<string, string> playerBansRaw = new Dictionary<string, string>();
            public StoredData() { }
        }



        #endregion;
        #region Hooks
        private void Init()
        {
            LoadDefaultConfig();
            jsonsettings.Converters.Add(new KeyValuesConverter());
            var data = Interface.GetMod().DataFileSystem?.ReadObject<StoredData>("SteamStats") ?? null;
            if (data == null)
            {
                storedData = new StoredData();
            }
            else
            {
                storedData = data;
                playerBans = storedData?.playerBans ?? null;
            }
            Puts("playerBans: " + (playerBans?.Count ?? 0) + " is storeddata null? " + ((storedData?.playerBans ?? null) == null) + ", alt count: " + (storedData?.playerBans?.Count ?? 0));
            permission.RegisterPermission(this.Name + ".checkbans", this);
        }

        private void Unload()
        {
            storedData.playerBansRaw = playerBansRaw;
            SaveStats();
        }

        void OnServerSave()
        {
            storedData.playerBansRaw = playerBansRaw;
            var rndTime = UnityEngine.Random.Range(2f, 5f); //delay saving to prevent lag spikes
            timer.Once(rndTime, () =>
            {
                SaveStats();
            });
        }

        void OnServerInitialized()
        {
            if (CheckPlayersAfterReload) UpdateAllPlayerBans(CheckSleepingPlayers);
        }

        DateTime GetLastCheckTime(string Id)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException();
            string lastTime;
            if (!storedData.lastCheckTime.TryGetValue(Id, out lastTime)) return DateTime.MinValue;
            DateTime time;
            if (!DateTime.TryParse(Id, out time)) return DateTime.MinValue;
            return time;
        }

        TimeSpan GetLastCheck(string Id)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException();
            var lastTime = GetLastCheckTime(Id);
            return lastTime == DateTime.MinValue ? TimeSpan.MinValue : (DateTime.Now - lastTime);
        }

        double LastCheckSeconds(string Id) { return TimeSpan.FromSeconds(GetLastCheckTime(Id).Second).TotalSeconds; ; }

        void SetLastCheck(string Id, string time = "")
        {
            if (string.IsNullOrEmpty(time)) time = DateTime.Now.ToString();
            DateTime dt;
            if (!DateTime.TryParse(time, out dt)) return;
            storedData.lastCheckTime[Id] = time;
        }

        void OnUserConnected(IPlayer player)
        {
            var lastCheck = GetLastCheck(player.Id);
            if (lastCheck.TotalHours < 24 && lastCheck.Milliseconds > 0)
            {
                PrintWarning("Returning... time since last check < 24 hours: " + GetLastCheck(player.Id).TotalHours);
                return;
            }
            UpdatePlayerBans(player.Id);
            //1 second timer to make sure stats have been gotten
            timer.Repeat(0.5f, 8, () =>
            {
                if (!playerBans.ContainsKey(player.Id)) return; //not loaded yet?
                if (storedData.playerBans.ContainsKey(player.Id) && storedData.playerBans.ContainsValue(playerBans[player.Id])) return; //already loaded
                storedData.playerBans.Add(player.Id, new Dictionary<string, object>());
                storedData.playerBans[player.Id] = playerBans[player.Id];
                storedData.playerBansRaw = playerBansRaw;
            });
            SetLastCheck(player.Id);
        }
        #endregion
        #region Util
        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        int GetPlayerStat(string ID, string stat)
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(stat)) return -1;
            var statInt = 0;
            object statObj;
            var outDict = new Dictionary<string, object>();
            if (!playerBans.TryGetValue(ID, out outDict)) return -1;
            if (!outDict.TryGetValue(stat, out statObj) || statObj == null) return -1;
            if (!int.TryParse(statObj.ToString(), out statInt)) statInt = -1;
            return statInt;
        }

        Dictionary<string, object> GetPlayerBans(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            var outDict = new Dictionary<string, object>();
            if (playerBans.TryGetValue(ID, out outDict)) return outDict;
            return null;
        }

        string GetRawBans(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            string outStr;
            if (playerBansRaw.TryGetValue(ID, out outStr) && !string.IsNullOrEmpty(outStr)) return outStr;
            return string.Empty;
        }

        private void UpdateAllPlayerBans(bool checkSleepers = false)
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null) continue;
                UpdatePlayerBans(player.UserIDString);
            }
            if (CheckSleepingPlayers)
            {
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var player = BasePlayer.sleepingPlayerList[i];
                    if (player == null) continue;
                    UpdatePlayerBans(player.UserIDString);
                }
            }
        }

        private void SaveStats() => Interface.GetMod().DataFileSystem.WriteObject("SteamStats", storedData);

        [ConsoleCommand("steam.buildbans")]
        private void consoleBuildDatabase(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection != null)
            {
                arg.ReplyWith("This cannot be run as a player!");
                return;
            }
            var allPlayers = covalence.Players.All;
            var updateSB = new StringBuilder();
            foreach (var p in allPlayers)
            {
                if (p == null) continue;
                UpdatePlayerBans(p.Id);
                var name = p?.Name ?? "Unknown";
                var ID = p?.Id ?? "Unknown ID";
                updateSB.AppendLine(name + " (" + ID + ")");
            }
            PrintWarning("Updated bans for players: " + updateSB.ToString());
        }

        [ConsoleCommand("steam.buildban")]
        private void consoleBuildDatabaseSingle(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection != null)
            {
                arg.ReplyWith("This cannot be run as a player!");
                return;
            }
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1)
            {
                arg.ReplyWith("You must supply a target name!");
                return;
            }
            var target = covalence.Players.FindPlayer(args[0]);
            if (target == null)
            {
                arg.ReplyWith("No player find with name/id/ip: " + args[0]);
                return;
            }
            UpdatePlayerBans(target.Id);
            timer.Once(2f, () =>
            {
                if (playerBans.ContainsKey(target.Id) && playerBans[target.Id].Count > 0) arg.ReplyWith("Updated bans successfully");
            });
        }

        bool IsListValid<T>(List<T> list) { return (list != null && list.Count > 0); }

        void UpdatePlayerBans(string userID)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(APIKey)) return;
            var url = string.Format("http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={0}&steamids={1}", APIKey, userID);
          //  var url = string.Format("http://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v0002/?appid=252490&key={0}&steamid={1}", APIKey, userID);
            var properStats = new Dictionary<string, long>();
            var name = covalence.Players.FindPlayerById(userID)?.Name ?? "Unknown";
            webrequest.Enqueue(url, null, (code, response) =>
            {
                if (code != 200) return;
                if (string.IsNullOrEmpty(response))
                {
                    PrintWarning("Got null or empty response for: " + name);
                    return;
                }
                if (response.Contains(@"</div>"))
                {
                    PrintWarning("Got invalid steam response for: " + name + " - not on steam?");
                    return;
                }
                playerBansRaw[userID] = response;
                var jsonresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                if (jsonresponse == null)
                {
                    PrintWarning("NULL!");
                    return;
                }
                if (!jsonresponse.ContainsKey("players")) return;
                if (!(jsonresponse["players"] is List<object>)) return;
                var listPlayers = (List <object>)jsonresponse["players"];
                if (!IsListValid(listPlayers)) return;
                if (!(((List<object>)jsonresponse["players"])[0] is Dictionary<string, object>)) return;
                var playerdata = ((List<object>)jsonresponse["players"])[0] as Dictionary<string, object>;
                playerBans[userID] = playerdata;
            }, this);

        }
        #endregion
    }
}