using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("PRISM Achievements", "Shady", "1.0.2", ResourceId = 0)]
    [Description("Dopamine abuse")]
    public class PRISMAchievements : RustPlugin
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
        //TO DO:
        //SHOW PLAYER THE % OF PEOPLE WHO HAVE THIS ACHIEVEMENT WHEN IT'S UNLOCKED (CELEBRATION)
        //CREATE AN ACHIEVEMENT COMMAND WITH AN ORDERED MENU. ORDERED BY EACH AchievementType
        //GIVE ACHIEVEMENTS VALUE (LIKE GAMER SCORE FROM XBOX, BUT PRISM SCORE. USE THE STRANGE TRIANGULAR THING TO SYMBOLIZE IT)
        //LEADERBOARD BASED ON TOP PRISM SCORE
        //SHOW LATEST UNLOCKED ACHIEVEMENT WHEN TYPING ACHIEVEMENT COMMAND
        //HAVE A BOOL VALUE TO DETERMINE IF THE ACHIEVEMENT SHOULD BE RESET ON SERVER WIPE
        //JSON optimization/ignore default vals
        //registering achievements from another plugin doesn't really seem to be very ideal. it works, but it feels tough to implement - how can we make it better?

        //SCREEN SHAKE EFFECT: assets/bundled/prefabs/fx/elevator_arrive.prefab
        //EGG PICKUP CUE: assets/prefabs/misc/easter/painted eggs/effects/eggpickup.prefab
        //HALLOWEEN PICKUP CUE: assets/prefabs/misc/halloween/lootbag/effects/loot_bag_upgrade.prefab

        // private readonly AchievementInfo _stayAFKFor30Seconds = new AchievementInfo("AFK Pro");

        #region Fields
        private const string DATA_FILE_NAME = "PRISMAchievementsData";
        private const string ACHIEVEMENT_FILE_NAME = "PRISMAchievements";

        private const string PAS_SYMBOL = "Δ";

        private readonly HashSet<IPlayer> _recentPlayers = new HashSet<IPlayer>();

        private PRISMAchievements _Instance = null;

        #region Plugin References
        [PluginReference]
        private readonly Plugin LocalStatsAPI;

        [PluginReference]
        private readonly Plugin Playtimes;
        #endregion

        private readonly Dictionary<string, Timer> _popupTimer = new Dictionary<string, Timer>();

        private readonly List<string> _brightColors = new List<string> { "#ffe866", "#a4ff6b", "#ff6eff", "#6bfaff", "#ee36ff", "#ff4060", "#ff904f", "#feb6dc", "#ff00aa", "#a4f502", "#18f500", "#ffee00", "#ff1f80", "#bb29ff", "#ffdb29", "#ff5b2e", "#30b7ff", "#d2ff30" };

        private Dictionary<string, UserAchievementData> _userData;

        private List<AchievementInfo> _achievements;

        #region Achievement Fields
        private readonly AchievementInfo _firstWaking = new AchievementInfo("wake_up", "Rise & shine", "Wake up from sleeping. Tougher than it sounds! :^)");

        private readonly AchievementInfo _stayAfk30Minutes = new AchievementInfo("stay_afk_30m", "Stay a while & listen", "Stay idle for 30 minutes");

        #endregion
        #endregion



        #region Classes

        private enum AchievementType { Misc, PvP, PvE, Looting, Gathering, Skill, Classes, All };

        //POTENTIAL USE FOR THIS IS TO ORDER ACHIEVEMENTS BY THEIR IMPRESSIVENESS
        //private enum AchievementLevel { Basic, Advanced, High, Super}

        private class UserAchievementData
        {
            public readonly List<string> UnlockedAchievements = new List<string>();

            public bool IsUnlocked(AchievementInfo info)
            {
                if (info == null) 
                    throw new ArgumentNullException(nameof(info));

                return UnlockedAchievements.Contains(info.ShortName);
            }

            public bool UnlockAchievement(AchievementInfo info)
            {
                if (IsUnlocked(info)) return false;

                UnlockedAchievements.Add(info.ShortName);
                return true;
            }

            public int GetTotalAchievementScore()
            {
                var score = 0;
                var achievements = AchievementInfo.AllAchievements;
                for (int i = 0; i < achievements.Count; i++)
                {
                    var achieve = achievements[i];
                    if (achieve == null) continue;

                    if (IsUnlocked(achieve))
                    {
                        score += achieve.Points;
                    }
                }

                return score;
            }

            public string GetLastUnlockedAchievementName()
            {
                return UnlockedAchievements?.Count < 1 ? string.Empty : UnlockedAchievements[UnlockedAchievements.Count - 1];
            }

            public AchievementInfo GetLastUnlockedAchievement()
            {
                return GetAchievementInfoFromName(GetLastUnlockedAchievementName());
            }

            public AchievementInfo GetAchievementInfoFromName(string name, StringComparison comparison = StringComparison.CurrentCulture)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException(nameof(name));

                var achievements = AchievementInfo.AllAchievements;
                for (int i = 0; i < achievements.Count; i++)
                {
                    var achieve = achievements[i];
                    if (achieve == null) continue;

                    if (achieve.ShortName.Equals(name, comparison) || achieve.Name.Equals(name, comparison))
                    {
                        return achieve;
                    }
                }

                return null;
            }

            public UserAchievementData() { }

        }


        private class AchievementInfo
        {
            public string Name { get; set; }
            public string ShortName { get; set; }
            public string Description { get; set; }
            public int Points { get; set; } = 100;
            public AchievementType Type { get; set; } = AchievementType.Misc;
            public string FX { get; set; } = string.Empty;
            public int TimesToRunFX { get; set; } = 1;
            public bool GlobalFX { get; set; } = false;
            public bool ResetOnWipe { get; set; } = false;
            public bool ResetOnBPWipe { get; set; } = false;
            public uint MountID { get; set; } = 0;

            public Dictionary<int, int> CollectItemAmounts = new Dictionary<int, int>();

            public Dictionary<uint, int> KillTargets = new Dictionary<uint, int>(); //ALL of these targets must be killed at least this many times to get achievement

            public Dictionary<uint, int> KillTarget = new Dictionary<uint, int>(); //ANY of these targets must be killed at least this many times to get achievement

            [JsonIgnore]
            public static List<AchievementInfo> AllAchievements { get; set; } = new List<AchievementInfo>();

            public static List<AchievementInfo> GetAchievements(AchievementType type = AchievementType.All)
            {
                if (type == AchievementType.All) return AllAchievements;

                var achievements = new List<AchievementInfo>();

                for (int i = 0; i < AllAchievements.Count; i++)
                {
                    var achievement = AllAchievements[i];
                    if (achievement.Type == type) achievements.Add(achievement);
                }

                return achievements;
            }

            public static int GetNumberOfAchievements(AchievementType type = AchievementType.All)
            {
                if (type == AchievementType.All) return AllAchievements.Count;

                var count = 0;

                for(int i = 0; i < AllAchievements.Count; i++)
                {
                    var achievement = AllAchievements[i];
                    if (achievement.Type == type) count++;
                }

                return count;
            }

            public static AchievementInfo FindAchievement(string name, StringComparison comparison = StringComparison.CurrentCulture)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException(nameof(name));

                var achievements = GetAchievements();
                for (int i = 0; i < achievements.Count; i++)
                {
                    var achieve = achievements[i];
                    if (achieve == null) continue;

                    if (achieve.ShortName.Equals(name, comparison) || achieve.Name.Equals(name, comparison))
                    {
                        return achieve;
                    }
                }

                return null;
            }


            public AchievementInfo()
            {
                if (!AllAchievements.Contains(this)) AllAchievements.Add(this);
                else Interface.Oxide.LogWarning("Somehow allachievements already contains this achievement!");
            }

            public AchievementInfo(string shortName, string displayName, string description = "", AchievementType type = AchievementType.Misc, int points = 0, string fx = "", int timesToRunFX = 1, bool globalFx = false)
            {
                if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException(nameof(shortName));
                if (!string.IsNullOrEmpty(fx) && timesToRunFX < 1) throw new ArgumentOutOfRangeException(nameof(timesToRunFX));

                ShortName = shortName;
                Name = displayName;
                Description = description;
                FX = fx;
                TimesToRunFX = timesToRunFX;
                GlobalFX = globalFx;
                Type = type;
                Points = points;

                if (!AllAchievements.Contains(this)) AllAchievements.Add(this);
                else Interface.Oxide.LogWarning("Somehow allachievements already contains this achievement (args)!");

            }

            public override string ToString()
            {
                return Name + "/" + ShortName + "/" + Description;
            }
        }

        #endregion

        #region Hooks
        private void Init()
        {
            try
            {
                _achievements = Interface.Oxide?.DataFileSystem?.ReadObject<List<AchievementInfo>>(ACHIEVEMENT_FILE_NAME) ?? new List<AchievementInfo>();

                if (_achievements?.Count < 1)
                {

                    var collectWood = new AchievementInfo("collect_wood", "Collect Vood", "Get some wood");// { GatherItemID = -151838493, GatherAmountNeeded = 1 };
                    collectWood.CollectItemAmounts[-151838493] = 1;

                    _achievements.Add(collectWood);
                }

                _userData = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, UserAchievementData>>(DATA_FILE_NAME);

                //PrintWarning("AllAchievements length: " + AchievementInfo.AllAchievements.Count + ", _" + _achievements.Count);


                AddCovalenceCommand("achtest", nameof(cmdAchievementTest));

                string[] paCmds = new string[] { "achievement", "achievements", "ach", "pa", "pach" };

                AddCovalenceCommand(paCmds, nameof(cmdPlayerAchivements));
            }
            finally { _Instance = this; }
        }

        private void OnUserConnected(IPlayer player) { _recentPlayers.Add(player); }

        private void OnServerInitialized()
        {
            var watch = Facepunch.Pool.Get<Stopwatch>();

            try 
            {
                var now = DateTime.UtcNow;

                foreach (var p in covalence.Players.All)
                {
                    var lastCon = GetLastConnectionS(p.Id);
                    if (lastCon != DateTime.MinValue && (now - lastCon).TotalDays < 30) _recentPlayers.Add(p);
                }
            }
            finally
            {
                var sb = Facepunch.Pool.Get<StringBuilder>();
                try { PrintWarning(sb.Clear().Append(nameof(OnServerInitialized)).Append(" took: ").Append(watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty)).Append("ms").ToString()); }
                finally
                {
                    try { Facepunch.Pool.Free(ref watch); }
                    finally { Facepunch.Pool.Free(ref sb); }
                }
               
            }
        }

        //private void OnServerSave() => SaveData();
        
        private void Unload()
        {
            try 
            {
                Timer popupTimer;
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || !p.IsConnected) continue;

                    if (_popupTimer.TryGetValue(p.UserIDString, out popupTimer))
                    {
                        popupTimer.Callback.Invoke();
                        popupTimer.Destroy();
                    }
                }

                //try to unregister all achievements registered by other plugins
                //turns out this is unnecessary imo
                /*/
                var allPlugins = plugins.GetAll();
                for (int i = 0; i < _achievements.Count; i++)
                {
                    var achievement = _achievements[i];
                    if (achievement == null) continue;

                    if (!achievement.ShortName.Contains("_")) continue;

                    var splitByUnderScore = achievement.ShortName.Split('_')[0];
                    PrintWarning("split by under: " + splitByUnderScore);

                    for (int j = 0; j < allPlugins.Length; j++)
                    {
                        var plugin = allPlugins[j];
                        if (plugin.Name.Equals(splitByUnderScore, StringComparison.OrdinalIgnoreCase))
                        {
                            PrintWarning("found the plugin! now going to unregister its achievement(s)");
                            UnregisterAchievement(achievement);
                            break;
                        }
                    }

                }/*/

            }
            finally 
            {
                try { SaveData(); }
                finally
                {
                    try { AchievementInfo.AllAchievements.Clear(); } //this MUST be cleared in order to avoid duplicating on next load because it is static
                    finally { _Instance = null; }
                }
            }
            
        }

        #region Achievement Hooks
        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player?.IdleTime >= 1800)
            {
                GiveAchievement(player, _stayAfk30Minutes);
            }
        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null || HasAchievement(player, _firstWaking)) return;

            player.Invoke(() =>
            {
                if (player == null || player.IsDestroyed || player.gameObject == null || player.IsSleeping() || !player.IsConnected || player.IsDead()) return;
                GiveAchievement(player, _firstWaking);
            }, 0.66f);


        }

        private void OnEntityMounted(BaseMountable entity, BasePlayer player)
        {
            if (entity == null || player == null || !player.IsConnected) return;
            
            for (int i = 0; i < AchievementInfo.AllAchievements.Count; i++)
            {
                var achievement = AchievementInfo.AllAchievements[i];
                if (achievement == null) continue;
                
                if (achievement.MountID == entity.prefabID)
                    GiveAchievement(player, achievement);
                
            }
            
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;

            var ply = info?.Initiator as BasePlayer;
            if (ply == null || ply.IsDestroyed || ply.gameObject == null) return;

            var prefabKills = GetStat2(ply.UserIDString, "prefabsKilled") as Dictionary<uint, long>;
            if (prefabKills == null || prefabKills.Count < 1) return;

            long prefabKillCount;
            if (!prefabKills.TryGetValue(entity.prefabID, out prefabKillCount)) return;

            int neededKills;

            var achieves = AchievementInfo.AllAchievements;
            for (int i = 0; i < achieves.Count; i++)
            {
                var achieve = achieves[i];
                if (achieve.KillTarget?.Count > 0 && !HasAchievement(ply, achieve) && achieve.KillTarget.TryGetValue(entity.prefabID, out neededKills) && prefabKillCount >= neededKills)
                {
                    GiveAchievement(ply, achieve);
                }

                //killtargetS needs implemented lol
            }
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (dispenser == null || entity == null || item == null) return;
            var player = entity as BasePlayer;
            if (player == null) return;

            var gathers = GetStat2(player.UserIDString, "resourcesGathered") as Dictionary<int, long>;
            if (gathers == null || gathers.Count < 1) return;

            long val;
            int collectNeeded;

            var achieves = AchievementInfo.AllAchievements;
            for (int i = 0; i < achieves.Count; i++)
            {
                var achieve = achieves[i];
                if (achieve.CollectItemAmounts?.Count > 0 && !HasAchievement(player, achieve) && achieve.CollectItemAmounts.TryGetValue(item.info.itemid, out collectNeeded))
                {

                    if (gathers.TryGetValue(item.info.itemid, out val) && val >= collectNeeded) GiveAchievement(player, achieve);

                }
            }
        }
        #endregion

        #endregion

        #region Commands
        private void cmdPlayerAchivements(IPlayer player, string command, string[] args)
        {
            if (player == null || player.IsServer) return;

            PrintWarning("PA command. all achievements count: " + AchievementInfo.AllAchievements.Count + ", _: " + _achievements.Count);

            var pObj = player?.Object as BasePlayer;
            if (pObj == null)
            {
                PrintWarning(nameof(cmdPlayerAchivements) + " called with null BasePlayer!!");
                return;
            }

            var achievementData = GetAchievementDataS(player.Id);
            if (achievementData == null)
            {
                player.Message("Unable to find achievement data!!!");
                return;
            }

            var sb = Facepunch.Pool.Get<StringBuilder>();
            var typesLength = Enum.GetNames(typeof(AchievementType)).Length;
            try
            {
                if (args?.Length < 1)
                {
                    var latestAchievement = achievementData.GetLastUnlockedAchievement();
                    if (latestAchievement != null)
                    {
                        player.Message(sb.Clear().Append("<i><color=#9dff73><size=19>LATEST UNLOCKED ACHIEVEMENT:</size></i></color>").Append(Environment.NewLine).Append(GetWackyColoredText("---", _brightColors)).Append("<size=22><color=#68e1fc>").Append(latestAchievement.Name).Append(" </color>(<color=#ff40ff>").Append(latestAchievement.Type).Append("</color>)</color></size>").Append(GetWackyColoredText("---", _brightColors)).Append(Environment.NewLine).Append(GetWackyColoredText("--", _brightColors)).Append("<size=17><color=#ff69c0>").Append(latestAchievement.Description).Append("</color></size>").Append(GetWackyColoredText("--", _brightColors)).ToString());
                    }
                    else player.Message("<color=#ff1212><size=21>You don't appear to have unlocked any achievements!</size></color> <color=#12d8ff>:(</color>");

                    player.Message(sb.Clear().Append("<i><color=#ff4f69><size=19>TOTAL PRISM ACHIEVEMENT (P").Append(PAS_SYMBOL).Append(") SCORE</color>:</size></i>").Append(Environment.NewLine).Append("<size=19><color=#a0ff57>P<size=22>").Append(PAS_SYMBOL).Append("</size></color></size><color=#d56bff> ").Append(achievementData.GetTotalAchievementScore().ToString("N0")).Append("</color>").ToString());

                    sb.Clear().Append("<i><size=17><color=#62ff1f>ACHIEVEMENT TYPES</color>:</size></i>").Append(Environment.NewLine);

                    for (int i = 0; i < typesLength; i++)
                    {
                        var achType = (AchievementType)i;
                        var countOfType = AchievementInfo.GetNumberOfAchievements(achType);
                        if (countOfType < 1) continue;

                        var unlockedOfType = 0;

                        for(int j = 0; j < AchievementInfo.AllAchievements.Count; j++)
                        {
                            var achieve = AchievementInfo.AllAchievements[j];
                            if ((achieve.Type == achType || achType == AchievementType.All) && HasAchievementS(player.Id, achieve)) unlockedOfType++;
                        }

                        sb.Append("/").Append("<color=#3bbeff>").Append(command).Append("</color> <color=#ff40ff>").Append(achType.ToString()).Append("</color>").Append(" (").Append(unlockedOfType.ToString("N0")).Append("/").Append(countOfType.ToString("N0")).Append(")").Append(Environment.NewLine);
                    }

                    sb.Length -= 1;

                    player.Message(sb.ToString());
                }
                else
                {
                    var arg0 = args[0];
                    if (arg0.Equals("top", StringComparison.OrdinalIgnoreCase))
                    {
                        var playerAchievementScores = new Dictionary<string, int>();
                        foreach(var p in _recentPlayers)
                        {
                            var score = GetAchievementDataS(p.Id).GetTotalAchievementScore();
                            if (score > 0) playerAchievementScores[p.Name] = score;
                        }
                        

                        PrintWarning("top arg detected. pas: " + playerAchievementScores.Count);

                        if (playerAchievementScores.Count < 1)
                        {
                            player.Message("No scores!");
                            return;
                        }

                        var ordered = playerAchievementScores.OrderByDescending(p => p.Value);
                        sb.Clear();

                        foreach (var p in ordered)
                        {
                            sb.Append(p.Key).Append(": ").Append(p.Value.ToString("N0")).Append(Environment.NewLine);
                        }

                        sb.Length -= 1;

                        player.Message(sb.ToString());
                        return;
                    }

                    var type = AchievementType.All;
                    var foundType = false;
                    

                    for (int i = 0; i < typesLength; i++)
                    {
                        var achType = (AchievementType)i;

                        if (achType.ToString().Equals(arg0, StringComparison.OrdinalIgnoreCase))
                        {
                            type = achType;
                            foundType = true;
                            break;
                        }
                    }

                    if (!foundType)
                    {
                        player.Message("Invalid achievement type specified: " + arg0);
                        return;
                    }

                    var allOfType = AchievementInfo.GetAchievements(type);

                    if (allOfType == null || allOfType.Count < 1)
                    {
                        player.Message("There are currently no achievements of type: " + type);
                        return;
                    }

                    sb.Clear();

                    if (type != AchievementType.All) sb.Append("<size=18><i><color=#ffc042>").Append(type).Append(" Achievements</color>:</i></size>").Append(Environment.NewLine);

                    for (int i = 0; i < allOfType.Count; i++)
                    {
                        var achieve = allOfType[i];

                        if (type == AchievementType.All) sb.Append(achieve.Type).Append(": ");

                        sb.Append("<color=#59baff><size=16>").Append(achieve.Name).Append("</size> <color=#ff61ef>P").Append(PAS_SYMBOL).Append("</color><color=#c6fc58>").Append(achieve.Points.ToString("N0")).Append(" ").Append(HasAchievementS(player.Id, achieve) ? "<color=#6cff5e><i>√</i></color>" : "<color=#ff0000><i>〤</i></color>").Append("</color>").Append("  (")/*/DOUBLE LOOKS BETTER HERE SPACE/*/.Append(GetPercentageOfUsersWithAchievement(achieve).ToString("0.00").Replace(".00", string.Empty)).Append("%)").Append(Environment.NewLine).Append("</color><size=13><color=#e1ffd6>").Append(achieve.Description).Append("</color></size>").Append(Environment.NewLine).Append("<size=18><color=#00c0ff>—</color></size>").Append(Environment.NewLine);
                    }

                    if (sb.Length > 1) sb.Length -= 1;

                    player.Message(sb.ToString());

                }


            }
            finally { Facepunch.Pool.Free(ref sb); }

        }

        private void cmdAchievementTest(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsAdmin || player.IsServer) return;

            var pObj = player?.Object as BasePlayer;
            if (pObj == null)
            {
                PrintWarning(nameof(cmdAchievementTest) + " called with null BasePlayer!!");
                return;
            }

            if (args.Length > 0 && args[0].Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                var data = GetAchievementDataS(player.Id);
                data.UnlockedAchievements.Clear();

                player.Message("Reset all achievements");
                return;
            }
            
            var achiev = new AchievementInfo("placeholder_achievement", "An achievement!!", "Placeholder achievement description", AchievementType.Misc, 1, args.Length > 0 ? args[0] : string.Empty, args.Length > 1 ? int.Parse(args[1]) : 1);
            CelebrateAchievement(pObj, achiev);
        }

        /*/
        private void cmdAchievementTest2(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsAdmin) return;

            var all = AchievementInfo.AllAchievements;
            for(int i = 0; i < all.Count; i++)
            {
                var achieve = all[i];
                var asGather = achieve as GatheringAchievement;
                player.Message("Achieve: " + achieve.Name + ", type: " + achieve.GetType() + " gather?: " + (asGather != null));
            }

            if (!_achievements.Any(p => p.ShortName == _gatherSomeStone.ShortName))
            {
                player.Message("no achievement found with gathersomestone's name, adding to list & attempting save");
                _achievements.Add(_gatherSomeStone);
                
            }

            if (!_achievements.Any(p => p.ShortName == _firstWaking.ShortName))
            {
                player.Message("no achievement found for waking, adding to list & attempting save");
                _achievements.Add(_firstWaking);

            }
            SaveData();

        }/*/
        #endregion

        #region Util
        private DateTime GetLastConnectionS(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            return Playtimes?.Call<DateTime>("GetLastConnection", userId) ?? DateTime.MinValue;
        }

        private double GetPercentageOfUsersWithAchievement(AchievementInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            var totalCount = 0d;
            var withAchievement = 0d;

            foreach(var p in _recentPlayers)
            {
                if (p == null) continue;

                totalCount++;

                if (HasAchievementS(p.Id, info)) withAchievement++;
            }

            return (withAchievement <= 0 || totalCount <= 0) ? 0 : withAchievement / totalCount * 100d;

        }

        private UserAchievementData GetAchievementDataS(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            UserAchievementData data;
            if (_userData.TryGetValue(userId, out data)) return data;
            else
            {
                var newData = new UserAchievementData();
                _userData[userId] = newData;
                return newData;
            }
        }

        private UserAchievementData GetAchievementData(BasePlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            return GetAchievementDataS(player.UserIDString);
        }

        private void GiveAchievement(BasePlayer player, AchievementInfo achievement)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (achievement == null) throw new ArgumentNullException(nameof(achievement));

            if (player.IsNpc)
            {
                PrintWarning(nameof(GiveAchievement) + " called on NPC player: " + player + ", " + achievement);
                return;
            }

            if (GetAchievementDataS(player.UserIDString).UnlockAchievement(achievement))
            {
                CelebrateAchievement(player, achievement);
            }
        }

        private void GiveAchievementByName(BasePlayer player, string achievementName)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(achievementName)) throw new ArgumentNullException(nameof(achievementName));

            if (player.IsNpc)
            {
                PrintWarning(nameof(GiveAchievementByName) + " called on NPC player: " + player + ", " + achievementName);
                return;
            }

            var achievement = AchievementInfo.FindAchievement(achievementName, StringComparison.OrdinalIgnoreCase);
            if (achievement == null)
            {
                PrintWarning("Could not find achievement with name: " + achievementName);
                return;
            }

            GiveAchievement(player, achievement);
        }


        //PRISMAchievements.Call("RegisterAchievement", "zlevelsremastered_woodcutting_mastery", "Woodcutting Master", "Reach level 99 in Woodcutting", 5, 125);
        private void RegisterAchievement(string achievementShortName, string achievementDisplayName, string achievementDescription = "", int achievementType = 0, int points = 0, string fx = "", int timesToRunFx = 1, bool globalFx = false)
        {
            if (string.IsNullOrEmpty(achievementShortName)) throw new ArgumentNullException(nameof(achievementShortName));
            if (string.IsNullOrEmpty(achievementDisplayName)) throw new ArgumentNullException(nameof(achievementDisplayName));

            if (AchievementInfo.FindAchievement(achievementShortName, StringComparison.OrdinalIgnoreCase) != null)
            {
                //PrintWarning("Achievement with shortname: " + achievementShortName + " already exists!");
                return;
            }

            if (AchievementInfo.FindAchievement(achievementDisplayName, StringComparison.OrdinalIgnoreCase) != null)
            {
               // PrintWarning("Achievement with displayname: " + achievementDisplayName + " already exists!");
                return;
            }
            
            var achievement = new AchievementInfo(achievementShortName, achievementDisplayName, achievementDescription, (AchievementType)achievementType, points, fx, timesToRunFx, globalFx);

            RegisterAchievementInfo(achievement);
        }

        private void RegisterAchievementFromPlugin(string achievementShortName, string achievementDisplayName, Plugin owner, string achievementDescription = "", int achievementType = 0, int points = 0, string fx = "", int timesToRunFx = 1, bool globalFx = false)
        {
            //PrintWarning(nameof(RegisterAchievementFromPlugin) + " plugin: " + owner.Name);

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try { RegisterAchievement(sb.Clear().Append(owner.Name).Append("_").Append(achievementShortName).ToString(), achievementDisplayName, achievementDescription, achievementType, points, fx, timesToRunFx, globalFx); }
            finally { Facepunch.Pool.Free(ref sb); }
        }
        
        private void UnregisterAchievement(AchievementInfo achievement)
        {
            if (achievement == null) throw new ArgumentNullException(nameof(achievement));

            if (!AchievementInfo.AllAchievements.Remove(achievement))
                PrintWarning("Could not unregister achievement from AchievementInfo: " + achievement);


            if (!_achievements.Remove(achievement))
                PrintWarning("Could not find _achievements: " + achievement + " to unregister!");
        }

        private void UnregisterAchievementByName(string achievementName)
        {
            if (string.IsNullOrEmpty(achievementName)) 
                throw new ArgumentNullException(nameof(achievementName));

            var achievement = AchievementInfo.FindAchievement(achievementName, StringComparison.OrdinalIgnoreCase);
            if (achievement == null)
            {
                PrintWarning("Could not find achievement with name: " + achievementName);
                return;
            }

            UnregisterAchievement(achievement);
        }
        
        private void UnregisterAllPluginAchievements(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName)) 
                throw new ArgumentNullException(nameof(pluginName));

            for (int i = 0; i < AchievementInfo.AllAchievements.Count; i++)
            {
                var achievement = AchievementInfo.AllAchievements[i];
                if (achievement == null) continue;

                if (!achievement.ShortName.Contains("_")) continue;

                var splitByUnderScore = achievement.ShortName.Split('_')[0];
                
                if (splitByUnderScore.Equals(pluginName, StringComparison.OrdinalIgnoreCase))
                    UnregisterAchievement(achievement);
            }
            
        }

        private void RegisterAchievementInfo(AchievementInfo achievement)
        {
            if (achievement == null) 
                throw new ArgumentNullException(nameof(achievement));

            _achievements.Add(achievement);
        }

        private bool HasAchievement(BasePlayer player, AchievementInfo achievement)
        {
            return GetAchievementData(player).IsUnlocked(achievement);
        }

        private bool HasAchievementS(string userId, AchievementInfo achievement)
        {
            return GetAchievementDataS(userId).IsUnlocked(achievement);
        }

        private bool HasAchievementByName(BasePlayer player, string achievementName)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(achievementName)) throw new ArgumentNullException(nameof(achievementName));

            var achievement = AchievementInfo.FindAchievement(achievementName, StringComparison.OrdinalIgnoreCase);
            if (achievement == null)
            {
                PrintWarning("Could not find achievement with name: " + achievementName);
                return false;
            }

            return HasAchievement(player, achievement);
        }

        private bool HasAchievementByNameS(string userId, string achievementName)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(achievementName)) throw new ArgumentNullException(nameof(achievementName));

            var achievement = AchievementInfo.FindAchievement(achievementName, StringComparison.OrdinalIgnoreCase);
            if (achievement == null)
            {
                PrintWarning("Could not find achievement with name: " + achievementName);
                return false;
            }

            return HasAchievementS(userId, achievement);
        }

        private void CelebrateAchievement(BasePlayer player, AchievementInfo achievement)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (achievement == null) throw new ArgumentNullException(nameof(achievement));

            if (player.IsDestroyed || player.gameObject == null || player.IsDead())
            {
                PrintWarning(nameof(CelebrateAchievement) + " called on null or dead player!!");
                return;
            }

            if (!player.IsConnected)
            {
                PrintWarning(nameof(CelebrateAchievement) + " called on disconnected player!!: " + player?.displayName + ", " + player?.UserIDString);
                return;
            }

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                CelebrateEffects(player, UnityEngine.Random.Range(1, 4));

                if (!string.IsNullOrEmpty(achievement.FX) && achievement.TimesToRunFX > 0)
                {
                    for (int i = 0; i < achievement.TimesToRunFX; i++)
                    {
                        if (!achievement.GlobalFX) SendLocalEffect(player, achievement.FX);
                        else Effect.server.Run(achievement.FX, player, 0, Vector3.zero, Vector3.zero);
                    }
                }

                var popupMsg = sb.Clear().Append("<size=24>").Append(GetWackyColoredText("Congratulations", _brightColors)).Append("<color=#ffe866>!</size>\n----</color><color=#ff6eff>Achievement unlocked</color><color=#ffe866>----</color>\n<size=22>").Append("<color=#57ff5f>").Append(achievement.Name).Append("</color> </size><size=20><color=#ff6eff> :)</size>").Append(Environment.NewLine).Append("<size=19><color=#d56bff>P</size><size=22>").Append(PAS_SYMBOL).Append("</size>").Append("<size=24>").Append(achievement.Points.ToString("N0")).Append("</size>").ToString();

                ShowPopup(player.UserIDString, popupMsg, 8f);
                SendReply(player, popupMsg + Environment.NewLine + Environment.NewLine + "<color=#ff145b>Learn more about this achievement by typing <size=20><color=#f080ff>/pa</color>!</color></size>"); // + Environment.NewLine + Environment.NewLine + "You can learn more about this achievement you just unlocked by typing *command here*!"



            }
            finally { Facepunch.Pool.Free(ref sb); }

        }

        private string GetWackyColoredText(string original, List<string> colors)
        {
            if (string.IsNullOrEmpty(original)) throw new ArgumentNullException(nameof(original));
            if (colors == null) throw new ArgumentNullException(nameof(colors));
            if (colors.Count < 1) throw new ArgumentOutOfRangeException(nameof(colors));

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try
            {
                sb.Clear();

                for (int i = 0; i < original.Length; i++)
                {
                    sb.Append("<color=").Append(colors[UnityEngine.Random.Range(0, colors.Count - 1)]).Append(">").Append(original[i]).Append("</color>");
                }

                return sb.ToString();
            }
            finally { Facepunch.Pool.Free(ref sb); }

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

        private void CelebrateEffects(BasePlayer player, int timesToRun = 1)
        {
            if (timesToRun < 1) throw new ArgumentOutOfRangeException(nameof(timesToRun));
            if (player == null || player.gameObject == null || player.IsDestroyed || player.transform == null) return;

            var eyePos = player?.eyes?.transform?.position ?? Vector3.zero;
            var eyeForward = player?.eyes?.HeadForward() ?? Vector3.zero;

            for (int j = 0; j < timesToRun; j++)
            {
             //   Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", SpreadVector2(eyePos + eyeForward * UnityEngine.Random.Range(1.5f, 1.9f), UnityEngine.Random.Range(1.35f, 2.2f)), Vector3.zero);
                SendLocalEffect(player, "assets/prefabs/misc/orebonus/effects/bonus_hit.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + UnityEngine.Random.Range(0.5f, 1f), eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(0.8f, 2f)));
                SendLocalEffect(player, "assets/prefabs/misc/orebonus/effects/hotspot_death.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + 0.6f, eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(1.1f, 3f)));
            }

            // SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");
            SendLocalEffect(player, "assets/prefabs/misc/easter/painted eggs/effects/eggpickup.prefab");

            player.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
        }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

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

        private object GetStat2(string userId, string stat)
        {
            return LocalStatsAPI?.Call("GetStat", userId, stat);
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(DATA_FILE_NAME, _userData);
            Interface.Oxide.DataFileSystem.WriteObject(ACHIEVEMENT_FILE_NAME, _achievements);
        }

        #endregion
    }
}
