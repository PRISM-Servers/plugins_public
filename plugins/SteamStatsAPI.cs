using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("Steam Stats API", "Shady", "1.0.0", ResourceId = 1711)]
    [Description("API for getting player's steam stats")]
    class SteamStatsAPI : RustPlugin
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

        Dictionary<string, Dictionary<string, long>> playerStats = new Dictionary<string, Dictionary<string, long>>();

        JsonSerializerSettings jsonsettings = new JsonSerializerSettings();


        /*--------------------------------------------------------------//
		//			Load up the default config on first use				//
		//--------------------------------------------------------------*/
        protected override void LoadDefaultConfig()
        {
            Config["APIKey"] = APIKey;
            Config["CheckPlayersAfterReload"] = CheckPlayersAfterReload;
            Config["CheckOfflinePlayers"] = CheckSleepingPlayers;
            SaveConfig();
        }

        StoredData storedData;

        class StoredData
        {
            public Dictionary<string, Dictionary<string, long>> playerStats = new Dictionary<string, Dictionary<string, long>>();
            public Dictionary<string, double> AccountCreated = new Dictionary<string, double>();
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
                playerStats = storedData?.playerStats ?? null;
            }
            Puts("playerStats: " + (playerStats?.Count ?? 0) + " is storeddata null? " + ((storedData?.playerStats ?? null) == null) + ", alt count: " + (storedData?.playerStats?.Count ?? 0));
        }

        private void Unload()
        {
            SaveStats();
        }

        void OnServerSave()
        {
            var rndTime = UnityEngine.Random.Range(2f, 5f); //delay saving to prevent lag spikes
            timer.Once(rndTime, () =>
            {
                SaveStats();
            });
        }

        void OnServerInitialized()
        {
            if (CheckPlayersAfterReload) UpdateAllPlayerStats(CheckSleepingPlayers);
        }

        object CanClientLogin(Network.Connection connection)
        {
            if (connection == null) return null;
            var id = connection.userid.ToString();
            NextTick(() =>
            {
                if (connection == null || !connection.active || connection.rejected)
                {
                   // PrintWarning("Connection is null, not active, or rejected on CanClientLogin NextTick");
                    return;
                }
                double time;

                if (!storedData.AccountCreated.TryGetValue(id, out time) || time <= 0)
                {
                    webrequest.Enqueue(string.Format("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}", APIKey, id), null, (code, response) =>
                    {
                        if (code != 200 || string.IsNullOrEmpty(response))
                        {
                            PrintWarning("Bad webrequest for: " + id);
                            return;
                        }
                        var summary = JsonConvert.DeserializeObject<Summaries>(response);
                        if (summary == null)
                        {
                            PrintWarning("summary null for: " + id);
                            return;
                        }
                        var pSummary = summary?.Response?.Players?.FirstOrDefault() ?? null;
                        if (pSummary == null)
                        {
                            PrintWarning("summary not null but pSummary is: " + id);
                            return;
                        }
                        storedData.AccountCreated[id] = pSummary.TimeCreated;

                    }, this);
                }
            });
            return null;
        }

        void OnPlayerConnected(BasePlayer player)
        {
            var id = player.UserIDString;
            if (!storedData.playerStats.ContainsKey(player.UserIDString))
            {
                UpdatePlayerStats(player);
                //1 second timer to make sure stats have been gotten
                timer.Repeat(0.5f, 8, () =>
                {
                    if (!playerStats.ContainsKey(player.UserIDString)) return; //not loaded yet?
                    if (storedData.playerStats.ContainsKey(player.UserIDString) && storedData.playerStats.ContainsValue(playerStats[player.UserIDString])) return; //already loaded
                    storedData.playerStats.Add(player.UserIDString, new Dictionary<string, long>());
                    storedData.playerStats[player.UserIDString] = playerStats[player.UserIDString];
                });
            }
        }

        #endregion
        #region Util
            T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        Dictionary<string, long> GetPlayerStats(IPlayer player)
        {
            return GetPlayerStats(player?.Id ?? string.Empty);
        }

        double GetCreationTime(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return -1;
            var val = -1d;
            if (storedData?.AccountCreated != null && !storedData.AccountCreated.TryGetValue(userId, out val)) val = -1;
            return val;
        }

        private class Summaries
        {
            [JsonProperty("response")]
            public Content Response;

            public class Content
            {
                [JsonProperty("players")]
                public Player[] Players;

                public class Player
                {
                    [JsonProperty("steamid")]
                    public string SteamId;
                    [JsonProperty("communityvisibilitystate")]
                    public int CommunityVisibilityState;
                    [JsonProperty("profilestate")]
                    public int ProfileState;
                    [JsonProperty("personaname")]
                    public string PersonaName;
                    [JsonProperty("lastlogoff")]
                    public double LastLogOff;
                    [JsonProperty("commentpermission")]
                    public int CommentPermission;
                    [JsonProperty("profileurl")]
                    public string ProfileUrl;
                    [JsonProperty("avatar")]
                    public string Avatar;
                    [JsonProperty("avatarmedium")]
                    public string AvatarMedium;
                    [JsonProperty("avatarfull")]
                    public string AvatarFull;
                    [JsonProperty("personastate")]
                    public int PersonaState;
                    [JsonProperty("realname")]
                    public string RealName;
                    [JsonProperty("primaryclanid")]
                    public string PrimaryClanId;
                    [JsonProperty("timecreated")]
                    public double TimeCreated;
                    [JsonProperty("personastateflags")]
                    public int PersonaStateFlags;
                    [JsonProperty("loccountrycode")]
                    public string LocCountryCode;
                    [JsonProperty("locstatecode")]
                    public string LocStateCode;
                }
            }
        }

        int GetPlayerStat(string ID, string stat)
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(stat)) return -1;
            var statInt = 0;
            if (!playerStats.ContainsKey(ID)) return -1;
            if (!playerStats[ID].ContainsKey(stat)) return -1;
            if (!int.TryParse(playerStats[ID][stat].ToString(), out statInt)) statInt = -1;
            return statInt;
        }

        int GetPlayerStat(IPlayer player, string stat)
        {
            return GetPlayerStat(player?.Id ?? string.Empty, stat);
        }

        Dictionary<string, long> GetPlayerStats(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            if (playerStats.ContainsKey(ID)) return playerStats[ID];
            return null;
        }

        Dictionary<string, long> GetPlayerStats(BasePlayer player)
        {
            return GetPlayerStats(player?.UserIDString ?? string.Empty);
        }

        int GetPlayerStat(BasePlayer player, string stat)
        {
            return GetPlayerStat(player?.UserIDString ?? string.Empty, stat);
        }
        

        private void UpdateAllPlayerStats(bool checkSleepers = false)
        {
            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null) continue;
                UpdatePlayerStats(player);
            }
            if (CheckSleepingPlayers)
            {
                for(int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var player = BasePlayer.sleepingPlayerList[i];
                    if (player == null) continue;
                    UpdatePlayerStats(player);
                }
            }
        }

        private void SaveStats() => Interface.GetMod().DataFileSystem.WriteObject("SteamStats", storedData);

        [ConsoleCommand("steam.buildstats")]
        private void consoleBuildDatabase(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection != null)
            {
                SendReply(arg, "This cannot be run as a player!");
                return;
            }
            var allPlayers = covalence.Players.All;
            var updateSB = new StringBuilder();
            foreach(var p in allPlayers)
            {
                UpdatePlayerStats(p);
                var name = p?.Name ?? "Unknown";
                var ID = p?.Id ?? "Unknown ID";
                updateSB.AppendLine(name + " (" + ID + ")");
            }
            PrintWarning("Updated stats for players: " + updateSB.ToString());
        }

        void UpdatePlayerStats(IPlayer player)
        {
            UpdatePlayerStats(player?.Id ?? string.Empty);
        }

        void UpdatePlayerStats(BasePlayer player)
        {
           UpdatePlayerStats(player?.UserIDString ?? string.Empty);
        }

        void UpdatePlayerStats(string userID)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(APIKey)) return;
            var url = string.Format("http://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v0002/?appid=252490&key={0}&steamid={1}", APIKey, userID);
            var properStats = new Dictionary<string, long>();
            var name = covalence.Players.FindPlayerById(userID)?.Name ?? "Unknown";

            webrequest.EnqueueGet(url, (code, response) =>
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
                var jsonresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                if (!jsonresponse.ContainsKey("playerstats")) return;
                var playerdata = jsonresponse["playerstats"] as Dictionary<string, object>;
                if (!playerdata.ContainsKey("stats")) return;
                var stats = playerdata["stats"] as List<object>;

                var lastKey = string.Empty;
                for (int i = 0; i < stats.Count; i++)
                {
                    var stat = stats[i];
                    var dict = (Dictionary<string, object>)stat;

                    foreach (var kvp in dict)
                    {
                        var key = kvp.Key;
                        var value = kvp.Value;
                        if (key == "name") lastKey = value.ToString();
                        if (key == "value")
                        {
                            if (string.IsNullOrEmpty(lastKey)) continue;
                            long longVal;
                            if (long.TryParse(value.ToString(), out longVal)) properStats[lastKey] = longVal;
                        }
                    }
                }
                playerStats[userID] = properStats;
            }, this);
        }
        #endregion
    }
}