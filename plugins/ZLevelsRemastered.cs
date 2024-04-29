using UnityEngine;
using System.Collections.Generic;
using System;
using Oxide.Core;
using Oxide.Core.Plugins;
using System.Text;
using System.Reflection;
using Oxide.Core.Libraries.Covalence;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Zeiser Levels REMASTERED", "Zeiser/Visagalis/Shady", "3.0.9", ResourceId = 1453)]
    [Description("Lets players level up as they harvest different resources and when crafting.")]
    public class ZLevelsRemastered : RustPlugin
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
        private readonly FieldInfo lastConsumeTime = typeof(PlayerMetabolism).GetField("lastConsumeTime", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly Dictionary<string, string> skillColors = new();

        private readonly Dictionary<string, List<string>> _skillsTop5 = new();

        
        private readonly Dictionary<BasePlayer, Item> _lastActiveItem = new();

        private readonly Dictionary<string, float> _lastGuiUpdate = new();

        //private readonly Dictionary<string, Dictionary<string, long>> lastXP = new Dictionary<string, Dictionary<string, long>>();

        private readonly List<string> xpHitOff = new();

        public const string LUCK_COLOR = "#5DC540";

        private const int RockID = 963906841;

        //pickaxes:
        private const int PICKAXE_ID = -1302129395;
        private const int STONE_PICKAXE_ID = 171931394;
        private const int ICE_PICK_ID = -1780802565;
        private const int JACKHAMMER_ID = 1488979457;
        private const int PROTO_PICK_ID = 236677901;
        private const int CONCRETE_PICK_ID = -1360171080;
        private const int ABYSS_PICK_ID = 1561022037;

        //axes:
        private const int CHAINSAW_ID = 1104520648;
        private const int HATCHET_ID = -1252059217;
        private const int SALVAGED_AXE_ID = -262590403;
        private const int STONE_AXE_ID = -1583967946;
        private const int PROTO_AXE_ID = -399173933;
        private const int CONCRETE_AXE_ID = 1176355476;
        private const int ABYSS_AXE_ID = 1046904719;

        private const int CactusFleshID = 1783512007;
        private const int ClothID = -858312878;
        private const int TroutID = -1878764039;

        private const int RAW_FISH_ITEM_ID = 989925924;

        private readonly HashSet<string> _forbiddenTags = new(6) { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);

        [PluginReference]
        private readonly Plugin LevelsGUI;

        [PluginReference]
        private readonly Plugin Luck;

        [PluginReference]
        private readonly Plugin PRISMAchievements;

        [PluginReference]
        private readonly Plugin PlayersByDatabase;

        [PluginReference]
        private readonly Plugin RadRain;

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;

            var hasAchievements = PRISMAchievements?.IsLoaded ?? false;



            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skill = Skills.ALL[i];
                if (IsSkillDisabled(skill)) continue;

                if (hasAchievements && GetLevel(player.UserIDString, skill) >= 99)
                    GrantMasteryAchievement(player, skill);

                NewLvlInfo(player.UserIDString, skill, 10, 1); //this only creates a new level info if one does not exist
            }

            player.Invoke(() =>
            {
                if (player == null || player.IsDestroyed || !player.IsConnected) return;
                CreateGUIAllSkills(player);
            }, 0.125f);
        }

        private void GrantMasteryAchievement(BasePlayer player, string skill)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrEmpty(skill))
                throw new ArgumentNullException(nameof(skill));

            if (!(PRISMAchievements?.IsLoaded ?? false))
            {
                PrintWarning(nameof(GrantMasteryAchievement) + " called but PA is null!!");
                return;
            }

            var foundSkill = false;

            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skillName = Skills.ALL[i];
                if (skillName.Equals(skill, StringComparison.OrdinalIgnoreCase))
                {
                    foundSkill = true;
                    break;
                }
            }

            if (!foundSkill)
                throw new ArgumentException($"Skill {skill} is not a valid skill.", nameof(skill)); //auto generated code lol

            PrintWarning("Trying to grant achievement for >= 99: " + skill);

            var sb = Pool.Get<StringBuilder>();

            try
            {
                PRISMAchievements.Call("GiveAchievementByName", player, sb.Clear().Append(Name).Append("_").Append(skill).Append("_mastery").ToString());
            }
            finally { Pool.Free(ref sb); }
        }

        public bool IsDivisble(int x, int n) { return (x % n) == 0; }

        private void CelebrateEffects(BasePlayer player, int timesToRun = 1)
        {
            if (timesToRun < 1) throw new ArgumentOutOfRangeException(nameof(timesToRun));
            if (player == null || player.gameObject == null || player.IsDestroyed || player.transform == null) return;
            var eyePos = player?.eyes?.transform?.position ?? Vector3.zero;
            var eyeForward = player?.eyes?.HeadForward() ?? Vector3.zero;

            for (int j = 0; j < timesToRun; j++)
            {
                Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", SpreadVector2(eyePos + eyeForward * UnityEngine.Random.Range(1.5f, 1.9f), UnityEngine.Random.Range(1.35f, 2.2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/bonus_hit.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + UnityEngine.Random.Range(0.5f, 1f), eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(0.8f, 2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/hotspot_death.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + 0.6f, eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(1.1f, 3f)), Vector3.zero);
            }

            SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");
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


        public static class Skills
        {
            public const string WOODCUTTING = "WC";
            public const string SURVIVAL = "S";
            public const string MINING = "M";
            public static string[] ALL = new[] { WOODCUTTING, MINING, SURVIVAL };
        }


        private readonly Dictionary<string, string> colors = new()
        {
           // {Skills.WOODCUTTING, "#CC6600"},
            {Skills.WOODCUTTING, "#977C46"},
            {Skills.MINING, "#6DA7BF" },
            //{Skills.MINING, "#167FCC"},
            {Skills.SURVIVAL, "#7F7B66" }
           // {Skills.SURVIVAL, "#CC1600"}
        };

        private class LevelInfo
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string Skill { get; private set; } = string.Empty;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(10L)]
            public long XP = 10;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(1L)]
            public long Level = 1;
            public LevelInfo(string skill, long xp, long level)
            {
                if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));
                if (xp < 10) xp = 10;
                if (level < 1) level = 1;

                Skill = skill;
                XP = xp;
                Level = level;
            }
        }

        private class UserLevelInfo
        {
            public Dictionary<string, LevelInfo> _classNameToSkill;
        }

        private class XPData
        {
            public Dictionary<string, List<LevelInfo>> levelInfos = new();
            public XPData() { }
        }

        private XPData xpData;

        private LevelInfo NewLvlInfo(string userID, string skill, long xp = 0, long level = 0)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(skill)) return null;
            var lvlInfo = GetLvlInfo(userID, skill);
            if (lvlInfo != null) return lvlInfo;

            lvlInfo = new LevelInfo(skill, xp, level);
            if (!xpData.levelInfos.TryGetValue(userID, out var listInfo)) xpData.levelInfos[userID] = new List<LevelInfo>();
            if (!xpData.levelInfos[userID].Contains(lvlInfo)) xpData.levelInfos[userID].Add(lvlInfo);
            return lvlInfo;
        }

        private LevelInfo GetLvlInfo(string userID, string skill)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(skill) || xpData?.levelInfos == null) return null;

            if (!xpData.levelInfos.TryGetValue(userID, out var lvlInfos)) return null;

            if (lvlInfos == null || lvlInfos.Count < 1) return null;

            for (int i = 0; i < lvlInfos.Count; i++)
            {
                var lvl = lvlInfos[i];
                if (lvl?.Skill == skill) return lvl;
            }
            return null;
        }

        private void SetLvlInfo(string userID, string skill, long points)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(skill)) return;

            var pointsLvl = GetPointsLevel(points, skill);
            var lvlInfo = GetLvlInfo(userID, skill);

            if (points < 10) points = 10;

            if (lvlInfo != null)
            {
                var oldLvl = lvlInfo.Level;
             

                lvlInfo.Level = pointsLvl;
                lvlInfo.XP = points;

                if (pointsLvl != oldLvl) UpdateTop5Cache(skill);
            }
            else NewLvlInfo(userID, skill, points, pointsLvl);
        }

        private void ResetLvl(string userId, string skill)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            var lvlInfo = GetLvlInfo(userId, skill);


            var newXp = GetLevelPoints(1);

            if (lvlInfo != null)
            {
                lvlInfo.Level = 1;
                lvlInfo.XP = newXp;
            }
            else NewLvlInfo(userId, skill, newXp, 1);

            if (ulong.TryParse(userId, out var userIdLong))
            {
                var ply = BasePlayer.FindByID(userIdLong);
                if (ply != null && ply.IsConnected) GUIUpdateSkill(ply, skill);
            }
        }

        private double GetPointsPerHit(string skill)
        {
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            return pointsPerHit.TryGetValue(skill, out var xp) ? xp : -1;
        }


        #region Stats
        [HookMethod("SendHelpText")]
        private void SendHelpText(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;
            string text = "/stats - Displays your stats.\n/statsui - Displays/hides stats UI.\n/statinfo [statsname] - Displays information about stat.\n" +
                          "/topskills - Display max levels reached so far.";
            SendReply(player, text);
        }

        private Dictionary<string, object> getMessages() { return messages; }

        private Dictionary<string, string> getColors() { return colors; }

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty;
        }


        private void UpdateTop5AllSkills()
        {
            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skill = Skills.ALL[i];
                if (!IsSkillDisabled(skill)) UpdateTop5Cache(skill);
            }
        }

        private List<string> UpdateTop5Cache(string skillName)
        {
            if (string.IsNullOrEmpty(skillName)) throw new ArgumentNullException(nameof(skillName));


            if (!_skillsTop5.TryGetValue(skillName, out var list)) list = new List<string>();
            else list.Clear();

            GetTop5NoAlloc(skillName, ref list);
            _skillsTop5[skillName] = list;

            return list;
        }

        private List<string> GetCachedTop5(string skillName)
        {
            if (string.IsNullOrEmpty(skillName)) throw new ArgumentNullException(nameof(skillName));

            if (!_skillsTop5.TryGetValue(skillName, out var list)) list = UpdateTop5Cache(skillName);


            return list;

        }

        private void GetTop5NoAlloc(string skillName, ref List<string> list, params string[] ignoreIds)
        {
            if (string.IsNullOrEmpty(skillName)) throw new ArgumentNullException(nameof(skillName));
            if (list == null) throw new ArgumentNullException(nameof(list));

            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                var skillFind = string.Empty;
                for (int i = 0; i < Skills.ALL.Length; i++)
                {
                    var skill = Skills.ALL[i];
                    if (skill.Equals(skillName, StringComparison.OrdinalIgnoreCase))
                    {
                        skillFind = skill;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(skillFind)) return;

                var xps = Pool.Get<Dictionary<string, Dictionary<string, long>>>();
                try
                {
                    xps.Clear();

                    var top5Skill = Pool.Get<Dictionary<string, List<string>>>();

                    try
                    {
                        top5Skill.Clear();

                        xps[skillFind] = new Dictionary<string, long>();

                        var players = Pool.GetList<string>();
                        try 
                        {
                            GetAllPlayerIDsNoAlloc(ref players);

                            for (int i = 0; i < players.Count; i++)
                            {
                                var userId = players[i];
                                if (ignoreIds != null && ignoreIds.Contains(userId)) continue;

                                var points = GetPoints(userId, skillFind);
                                if (points < 15) continue; //filter out extremely low level players

                                xps[skillFind][userId] = points;
                            }
                        }
                        finally { Pool.FreeList(ref players); }


                        if (xps[skillFind].Keys.Count < 1 || xps[skillFind].Values.Count < 5) return;

                        var top5 = (from entry in xps[skillFind] orderby entry.Value descending select entry).Take(5);

                        foreach (var kvp in top5)
                        {
                            var findID = string.Empty;
                            foreach (var kvp2 in xps[skillFind])
                            {
                                if (kvp2.Value == kvp.Value)
                                {
                                    findID = kvp.Key;
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(findID)) list.Add(findID);
                        }
                    }
                    finally { Pool.Free(ref top5Skill); }

                }
                finally { Pool.Free(ref xps); }
            }
            finally
            {
                try { if (watch.ElapsedMilliseconds >= 10) PrintWarning(nameof(GetTop5NoAlloc) + " took: " + watch.ElapsedMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }
        }


        private void StatsTopCommand(IPlayer player, string command, string[] args)
        {
            var xps = new Dictionary<string, Dictionary<string, long>>();
            var top5Skill = new Dictionary<string, List<string>>();
            var topSkillsSB = new StringBuilder();
            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skill = Skills.ALL[i];
                if (IsSkillDisabled(skill)) continue;


                if (!top5Skill.TryGetValue(skill, out var outList)) top5Skill[skill] = new List<string>();
                if (!xps.TryGetValue(skill, out var xpOut)) xps[skill] = new Dictionary<string, long>();


                var players = Pool.GetList<string>();
                try
                {
                    GetAllPlayerIDsNoAlloc(ref players);

                    for (int j = 0; j < players.Count; j++)
                    {
                        var userId = players[j];

                        var points = GetPoints(userId, skill);
                        if (points < 15) continue; //filter out extremely low level players
                        xps[skill][userId] = points;
                    }
                }
                finally { Pool.FreeList(ref players); }

                if (xps[skill].Keys.Count < 1 || xps[skill].Values.Count < 1) continue;
                var max = xps[skill]?.Values?.Max() ?? 0f;
                var newXp = new KeyValuePair<string, long>();
                foreach (var kvp in xps[skill])
                {
                    if (kvp.Value == max)
                    {
                        newXp = kvp;
                        break;
                    }
                }
                var top5 = (from entry in xps[skill] orderby entry.Value descending select entry).Take(5);

                foreach (var kvp in top5) if (!top5Skill[skill].Contains(kvp.Key)) top5Skill[skill].Add(kvp.Key);

                var maxLvl = GetLevel(newXp.Key.ToString(), skill);
                var name = GetDisplayNameFromID(newXp.Key);
                if (string.IsNullOrEmpty(name) || name.Equals("Shady", StringComparison.OrdinalIgnoreCase)) name = "Null Player";
                topSkillsSB.AppendLine("<color=" + colors[skill] + ">" + messages[skill + "Skill"] + ": " + maxLvl + " (XP: " + newXp.Value.ToString("N0") + ")</color> <- " + name);
            }
            if (args.Length == 1)
            {
                var specSB = new StringBuilder();
                var skillFind = string.Empty;
                var arg0Lower = args[0].ToLower();
                if (arg0Lower == "wc" || arg0Lower == "woodcutting") skillFind = "WC";
                if (arg0Lower == "m" || arg0Lower == "mining") skillFind = "M";
                if (arg0Lower == "s" || arg0Lower == "skinning" || arg0Lower == "survival") skillFind = "S";
                if (string.IsNullOrEmpty(skillFind))
                {
                    SendReply(player, "Bad skill: " + args[0] + ", try WC, M, or S");
                    return;
                }
                var top5spec = top5Skill.Where(p => p.Key == skillFind);
                if (top5spec == null || !top5spec.Any())
                {
                    SendReply(player, "Bad skill: " + args[0] + ", try WC, M, or S");
                    return;
                }
                foreach (var kvp in top5spec)
                {
                    foreach (var entry in kvp.Value)
                    {
                        var getLvl = GetLevel(entry, kvp.Key);
                        var getXP = GetPoints(entry, kvp.Key);
                        var name = GetDisplayNameFromID(entry);
                        if (string.IsNullOrEmpty(name)) name = "Null Player";
                        specSB.AppendLine("<color=" + colors[skillFind] + ">" + messages[skillFind + "Skill"] + ": " + getLvl.ToString("N0") + " (XP: " + getXP.ToString("N0") + ")</color> <- " + name);
                    }
                }
                if (specSB.Length > 0) SendReply(player, specSB.ToString().TrimEnd());
                else SendReply(player, "Failed to get skill info for: " + args[0]);
                return;
            }
            var finalMsg = "<color=#00b2c6>You can view the top 5 for specific skills by doing:</color> /" + command + " WC, /" + command + " M, or /" + command + " S\nYou can view top <color=" + LUCK_COLOR + ">Luck</color> levels by typing <color=" + LUCK_COLOR + ">/topluck</color>.";
            if (topSkillsSB.Length > 0) SendReply(player, topSkillsSB.ToString().TrimEnd() + Environment.NewLine + finalMsg);
            else SendReply(player, "No top skills yet!" + Environment.NewLine + finalMsg);

            if ((Luck?.IsLoaded ?? false)) Luck.Call("SendTopLuckText", player);

        }


        [ConsoleCommand("zlvls.arithmetic")]
        private void cmdMathTest(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1)
            {
                SendReply(arg, "Please supply a points value (float)");
                return;
            }
            if (!float.TryParse(args[0], out var val))
            {
                SendReply(arg, "Not a float: " + args[0]);
                return;
            }
            var watch = Stopwatch.StartNew();
            double x1 = (-100 - Math.Sqrt(100 * 100 - 4 * 110 * -val)) / (2 * 110);
            var x2 = (int)-x1;
            watch.Stop();
            PrintWarning("Negative math (twice) took: " + watch.Elapsed.TotalMilliseconds + "ms");
            watch.Reset();
            watch.Start();
            var x3 = (100 + Math.Sqrt(100 * 100 + 4 * 110 * val)) / (2 * 110);
            watch.Stop();
            PrintWarning("Positive math (once) took: " + watch.Elapsed.TotalMilliseconds + "ms");
            PrintWarning("Values: x1: " + x1 + Environment.NewLine + "x2: " + x2 + Environment.NewLine + "x3: " + x3);
        }

        [ConsoleCommand("zlvls.fix")]
        private void zlvlFix(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var newDict = xpData?.levelInfos?.ToDictionary(p => p.Key, p => p.Value) ?? null;
            if (newDict == null || newDict.Count < 1)
            {
                SendReply(arg, "No zlvl infos!");
                return;
            }
            var removeSB = new StringBuilder();
            var removeCount = 0;
            foreach (var kvp in newDict)
            {
                var findLvl = kvp.Value?.Where(p => p.Level < 1)?.ToList() ?? null;
                if (findLvl != null && findLvl.Count > 0)
                {
                    for (int i = 0; i < findLvl.Count; i++)
                    {
                        var lvl = findLvl[i];
                        if (lvl == null) continue;
                        SetLvlInfo(kvp.Key, lvl.Skill, 10);
                        removeSB.AppendLine(GetDisplayNameFromID(kvp.Key) + " (" + kvp.Key + ") : " + lvl.Skill);
                    }

                }
                //     var addCount = kvp.Value.RemoveAll(p => ((p?.Level ?? 0) <= 3));
                //   if (addCount > 0) removeSB.AppendLine(GetDisplayNameFromID(kvp.Key) + " (" + kvp.Key + ")");
                // removeCount += addCount;
                //       removeCount += kvp.Value.RemoveAll(p => ((p?.Level ?? 0) <= 1));
            }
            xpData.levelInfos = newDict;
            SaveXPData();
            SendReply(arg, "Fixed " + removeCount + " entries:\n" + removeSB.ToString().TrimEnd());
        }

        [ConsoleCommand("zlvls.purge")]
        private void lkPurge(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var newDict = xpData?.levelInfos?.ToDictionary(p => p.Key, p => p.Value) ?? null;
            if (newDict == null || newDict.Count < 1)
            {
                SendReply(arg, "No zlvl infos!");
                return;
            }
            var removeSB = new StringBuilder();
            var removeCount = 0;
            foreach (var kvp in newDict)
            {
                if (FindConnectedPlayer(kvp.Key) != null) continue;

                var findLvl = kvp.Value?.Where(p => p.Level <= 1)?.ToList() ?? null;
                if (findLvl != null && findLvl.Count > 0)
                {
                    for (int i = 0; i < findLvl.Count; i++)
                    {
                        var lvl = findLvl[i];
                        if (lvl == null) continue;
                        removeSB.AppendLine(GetDisplayNameFromID(kvp.Key) + " (" + kvp.Key + "), skill: " + lvl.Skill + ", " + lvl.Level + ", " + lvl.XP);
                        kvp.Value.Remove(lvl);
                    }

                }
                //     var addCount = kvp.Value.RemoveAll(p => ((p?.Level ?? 0) <= 3));
                //   if (addCount > 0) removeSB.AppendLine(GetDisplayNameFromID(kvp.Key) + " (" + kvp.Key + ")");
                // removeCount += addCount;
                //       removeCount += kvp.Value.RemoveAll(p => ((p?.Level ?? 0) <= 1));
            }
            xpData.levelInfos = newDict;
            SaveXPData();
            SendReply(arg, "Removed " + removeCount + " entries:\n" + removeSB.ToString().TrimEnd());
        }

        [ConsoleCommand("zlvl.transfer")]
        private void TransferCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 2)
            {
                SendReply(arg, "Bad args, try: zlvl.transfer steamID newSteamID");
                return;
            }
            if (!ulong.TryParse(args[0], out var orgID))
            {
                SendReply(arg, "Failed to parse: " + args[0] + " as steam ID!");
                return;
            }
            if (!ulong.TryParse(args[1], out var newID))
            {
                SendReply(arg, "Failed to parse: " + args[1] + " as steam ID!");
                return;
            }
            if (orgID == newID)
            {
                SendReply(arg, "You cannot transfer stats to the same player!");
                return;
            }
            var newIDStr = newID.ToString();
            var orgIDStr = orgID.ToString();
            var setSB = new StringBuilder();
            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skill = Skills.ALL[i];
                var oldLevel = GetLevel(orgIDStr, skill);
                var oldPoints = GetPoints(orgIDStr, skill);
                if (oldPoints > GetPoints(newIDStr, skill))
                {
                    SetLvlInfo(newIDStr, skill, oldPoints);
                    setSB.AppendLine("Set: " + skill + " to level " + oldLevel + ", points: " + oldPoints);
                }
            }
            var newPlayer = covalence.Players?.FindPlayerById(newIDStr)?.Object as BasePlayer;
            if (newPlayer != null && newPlayer.IsConnected) CreateGUIAllSkills(newPlayer);
            SendReply(arg, setSB.ToString().TrimEnd());
        }

        [ConsoleCommand("zlvl.reset")]
        private void ResetCommand(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin) return;

            var args = arg?.Args ?? null;

            var playerId = (args == null || args.Length < 1) ? (arg?.Player()?.UserIDString ?? string.Empty) : (FindConnectedPlayer(args[0], true)?.Id ?? string.Empty);

            if (string.IsNullOrWhiteSpace(playerId))
            {
                SendReply(arg, "No player found!");
                return;
            }


            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skill = Skills.ALL[i];
                SetLvlInfo(playerId, skill, 10);
            }

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var ply = BasePlayer.activePlayerList[i];
                if (ply.UserIDString == playerId)
                {
                    CreateGUIAllSkills(ply);
                    PrintWarning("Called CreateGUIAllSkills for ply: " + ply?.displayName + " after stats reset");
                    break;
                }
            }

            SendReply(arg, "Reset all stats for " + playerId + "!");
        }

        [ConsoleCommand("zlvl.set")]
        private void SetLvlCommand(ConsoleSystem.Arg arg)
        {
            if (arg == null || !arg.IsAdmin) return;

            var args = arg?.Args ?? null;
            if (args?.Length < 3)
            {
                SendReply(arg, "You must supply a username, skill and level to set. First integer is skill index (0 = Woodcutting, 1 = Mining, 2 = Survival) Example: " + "zlvl.set Name 1 1");
                return;
            }

            var playerObj = FindConnectedPlayer(args[0], true)?.Object;
            if (playerObj == null)
            {
                SendReply(arg, "No player found!");
                return;
            }

            var pid = (playerObj as BasePlayer)?.UserIDString ?? (playerObj as IPlayer)?.Id ?? string.Empty;

            if (string.IsNullOrEmpty(pid))
            {
                SendReply(arg, "No ID found on player object!");
                return;
            }

            if (!int.TryParse(args[1], out var index))
            {
                SendReply(arg, "Not an integer: " + args[1]);
                return;
            }
            if (index < 0 || index > 2)
            {
                SendReply(arg, "Index is out of range, should be 0-2");
                return;
            }

            if (!int.TryParse(args[2], out var level))
            {
                SendReply(arg, "Not an integer: " + args[2]);
                return;
            }


            SetLvlInfo(pid, Skills.ALL[index], GetLevelPoints(level));


            var ply = playerObj as BasePlayer;
            if (ply != null && ply.IsConnected)
            {
                CreateGUIAllSkills(ply);
                PrintWarning("Called CreateGUIAllSkills for ply: " + ply?.displayName + " after stats change");
            }

            SendReply(arg, "Changed stats!");
        }

        private void StatsCommand(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;

            var textSB = Pool.Get<StringBuilder>();

            try
            {
                textSB.Clear();

                for (int i = 0; i < Skills.ALL.Length; i++)
                {
                    var skill = Skills.ALL[i];
                    if (IsSkillDisabled(skill)) continue;
                    textSB.Append(GetStatPrint(player.Id, skill)).Append(Environment.NewLine);
                }

                textSB.Length -= 1;

                var xpMult = GetXPMultiplier(player.Id);
                var itemMult = GetItemMultiplier(player.Id);

                textSB.Append("Your XP rates are: ").Append(xpMult.ToString("N0")).Append("%").Append(Environment.NewLine);
                textSB.Append("Your Item rates are: ").Append(itemMult.ToString("N0")).Append("%").Append(Environment.NewLine);

                for (int i = 0; i < Skills.ALL.Length; i++)
                {
                    var skill = Skills.ALL[i];
                    if (IsSkillDisabled(skill)) continue;
                    textSB.Append("Your <color=").Append(colors[skill]).Append(">").Append(messages[skill + "Skill"]).Append("</color> XP catch-up multiplier is: ").Append(GetCatchupMultiplier(player.Id, skill).ToString("N0")).Append("%").Append(Environment.NewLine);
                }

                textSB.Append("NOTE: The average of the top 5 highest player levels in a skill must be at least 40 for any catch-up bonuses");

                SendReply(player, textSB.ToString());
            }
            finally { Pool.Free(ref textSB); }

        }

        private void SendReply(IPlayer player, string msg, string userId = "0", bool keepTagsConsole = false)
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

        [ChatCommand("statinfo")]
        private void StatInfoCommand(BasePlayer player, string command, string[] args)
        {
            string messagesText;
            long xpMultiplier = 100;

            if (args.Length == 1)
            {
                string statname = args[0].ToLower();
                switch (statname)
                {
                    case "mining":
                        messagesText = "<color=" + colors[Skills.MINING] + ">Mining</color>" + (IsSkillDisabled(Skills.MINING) ? "(DISABLED)" : "") + "\n";
                        messagesText += "XP per hit: <color=" + colors[Skills.MINING] + ">" + ((int)pointsPerHitObj[Skills.MINING] * (xpMultiplier / 100f)) + "</color>\n";
                        messagesText += "Bonus materials per level: <color=" + colors[Skills.MINING] + ">" + ((GetGathMult(2, Skills.MINING) - 1) * 100).ToString("0.##") + "%</color>\n";
                        break;
                    case "woodcutting":
                        messagesText = "<color=" + colors[Skills.WOODCUTTING] + ">Woodcutting</color>" + (IsSkillDisabled(Skills.WOODCUTTING) ? "(DISABLED)" : "") + "\n";
                        messagesText += "XP per hit: <color=" + colors[Skills.WOODCUTTING] + ">" + ((int)pointsPerHitObj[Skills.WOODCUTTING] * (xpMultiplier / 100f)) + "</color>\n";
                        messagesText += "Bonus materials per level: <color=" + colors[Skills.WOODCUTTING] + ">" + ((GetGathMult(2, Skills.WOODCUTTING) - 1) * 100).ToString("0.##") + "%</color>\n";
                        break;
                    case "survival":
                        messagesText = "<color=" + colors[Skills.SURVIVAL] + '>' + "Survival" + "</color>" + (IsSkillDisabled(Skills.SURVIVAL) ? "(DISABLED)" : "") + "\n";
                        messagesText += "XP per hit: <color=" + colors[Skills.SURVIVAL] + ">" + ((int)pointsPerHitObj[Skills.SURVIVAL] * (xpMultiplier / 100f)) + "</color>\n";
                        messagesText += "Bonus materials per level: <color=" + colors[Skills.SURVIVAL] + ">" + ((GetGathMult(2, Skills.SURVIVAL) - 1) * 100).ToString("0.##") + "%</color>\n";
                        break;
                    default:
                        messagesText = "No such stat: " + args[0];
                        messagesText += "\nYou must choose from these stats: <color=" + colors[Skills.MINING] + ">Mining</color>, <color=" + colors[Skills.SURVIVAL] + ">Survival</color>, <color=" + colors[Skills.WOODCUTTING] + ">Woodcutting</color>";
                        break;
                }
            }
            else
            {
                messagesText = "You must choose from these stats: <color=" + colors[Skills.MINING] + ">Mining</color>, <color=" + colors[Skills.SURVIVAL] + ">Survival</color>, <color=" + colors[Skills.WOODCUTTING] + ">Woodcutting</color>";
            }
            PrintToChat(player, messagesText);
        }


        [ChatCommand("xphit")]
        private void XPHitCommand(BasePlayer player, string command, string[] args)
        {
            if (!xpHitOff.Contains(player.UserIDString)) xpHitOff.Add(player.UserIDString);
            else xpHitOff.Remove(player.UserIDString);
            SendReply(player, (xpHitOff.Contains(player.UserIDString) ? "Diabled" : "Enabled") + " XP GUI notifications");
        }

        private void GUIUpdateSkill(BasePlayer player, string skill, int points = 0, bool ignoreCooldown = false)
        {

            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));
            if (LevelsGUI == null || !LevelsGUI.IsLoaded)
            {
                PrintWarning("GUIUpdateSkill called but LevelsGUI not found/loaded!!");
                return;
            }

            var now = Time.realtimeSinceStartup;

            if (!ignoreCooldown)
            {


                if (_lastGuiUpdate.TryGetValue(player.UserIDString, out var lastTime) && now - lastTime < 0.2)
                    return;
            }
          

            var percent = (float)GetExperiencePerc(player.UserIDString, skill);
            var level = GetLevel(player.UserIDString, skill);
            if (level < 0) level = 1;


            var sb = Pool.Get<StringBuilder>();
            try
            {
                var skillMsg = messages[sb.Clear().Append(skill).Append("Skill").ToString()].ToString();

                if (!skillColors.TryGetValue(skill, out var skillColor)) PrintWarning("SKILL WITH NO COLOR!!: " + skill);

                _lastGuiUpdate[player.UserIDString] = now;

                LevelsGUI.Call("GUIUpdateSkill", player, skillMsg, level, percent, points, skillColor);
            }
            finally { Pool.Free(ref sb); }
        }

        private void CreateGUIAllSkills(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skill = Skills.ALL[i];
                if (!IsSkillDisabled(skill)) 
                    GUIUpdateSkill(player, skill, ignoreCooldown:true);
            }
        }

        private void DestroyGUIAllSkills()
        {
            if (LevelsGUI == null || !LevelsGUI.IsLoaded)
            {
                if (!ServerMgr.Instance.Restarting) PrintWarning("LevelsGUI is null or unloaded on DestroyGUIAllSkills()!");
                return;
            }
            for (int i = 0; i < Skills.ALL.Length; i++)
            {
                var skill = Skills.ALL[i];
                if (IsSkillDisabled(skill)) continue;
                var skillName = skill;
                if (messages.TryGetValue(skill + "Skill", out var fullNameObj)) skillName = fullNameObj.ToString();
                LevelsGUI?.Call("RemoveSkill", skillName);
            }
            PrintWarning("Called RemoveSkill in LevelsGUI for all non-disabled skills");
        }

        private void OnLevelsGUICreated(BasePlayer player)
        {
            if (player == null)
            {
                PrintWarning("OnLevelsGUICreated called with null player!!");
                return;
            }

            CreateGUIAllSkills(player); //delay to fix getting level 0
        }



        private string GetStatPrint(string userID, string skill)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(skill) || IsSkillDisabled(skill)) return string.Empty;

            var cap = GetLevelCap(skill, userID);

            var skillMaxed = cap != 0 && GetLevel(userID, skill) == cap;
            var bonusText = ((GetGathMult(GetLevel(userID, skill), skill) - 1) * 100).ToString("0.##");

            var nextXp = GetLevelPoints(GetLevel(userID, skill) + 1);
            PrintWarning("nextXp: " + nextXp + ", current: " + GetPoints(userID, skill) + ", getLevel: " + GetLevel(userID, skill));

            return string.Format("<color=" + colors[skill] + '>' + (string)messages["StatsText"] + "</color>\n",
                (string)messages[skill + "Skill"],
                GetLevel(userID, skill).ToString("N0") + (cap > 0 ? ("/" + cap.ToString("N0")) : ""),
                GetPoints(userID, skill).ToString("N0"),
                skillMaxed ? "∞" : GetLevelPoints(GetLevel(userID, skill) + 1).ToString("N0"),
                bonusText,
                GetExperiencePercent(userID, skill));
        }

        #endregion

        #region Main/Other

        /*/
        private void OnQuarryGather(MiningQuarry quarry, Item item)
        {
            if (quarry == null || item == null || quarry?.OwnerID == 0) return;
            var player = BasePlayer.FindByID(quarry?.OwnerID ?? 0);
            if (player == null || !(player?.IsConnected ?? true) || (player?.IsDead() ?? true) || player?.transform == null) return;
            var dist = Vector3.Distance(quarry.CenterPoint(), player.transform.position);
            if (dist > 100) return;
            
            if (!player.IsAdmin) return;
            var ogAmount = item?.amount ?? 0;
            var finalItemAmount = item?.amount ?? 0;
            finalItemAmount = (int)Math.Round((item.amount * getGathMult(getLevel(player.UserIDString, "M"), "M")));
            finalItemAmount = (int)Math.Round((finalItemAmount * (getItemMultiplier(player.UserIDString))) / 100, MidpointRounding.AwayFromZero);
            item.amount = (int)Math.Round(Mathf.Clamp(finalItemAmount / 2.5f, ogAmount, 40));
            PrintWarning("new amt: " + finalItemAmount + ", og: " + ogAmount);
            var getXP = (int)Math.Round(UnityEngine.Random.Range(8, 40) / (dist / 6f));
            var catchup = getCatchupMultiplier(player.UserIDString, "M");
            var xpMult = getXPMultiplier(player.UserIDString);
            getXP = (int)Math.Round(getXP * (xpMult / 100f));
            getXP = (int)Math.Round(getXP * (catchup / 100f));
            SetLvlInfo(player.UserIDString, "M", getPoints(player.UserIDString, "M") + getXP);
            if (!xpHit.Contains(player.UserIDString)) RenderUI(player);
            else
            {
                RenderUI(player, "M", getXP.ToString("N0"));
                var timerDelay = 1.15f;
                Timer newXP = null;
                if (xpTimer.TryGetValue(player.UserIDString, out newXP) && newXP != null && !(newXP?.Destroyed ?? true))
                {
                    newXP.Destroy();
                    newXP = null;
                }
                newXP = timer.Once((timerDelay + 0.1f), () => RenderUI(player));
                xpTimer[player.UserIDString] = newXP;
            }
        }/*/

        private void OnFishCatch(Item item, BaseFishingRod rod, BasePlayer player) //only for catching fish via fishing rods - not traps
        {
            if (item == null || rod == null || player == null) return;


            var fishable = item?.info?.GetComponent<ItemModFishable>();

            if (fishable == null)
            {
                PrintError("fishable null somehow?!?!?!?");
                return;
            }

            var compost = item?.info?.GetComponent<ItemModCompostable>();

            if (compost != null)
            {
                PrintWarning("compost bait: " + compost.BaitValue + " for: " + item.info.displayName.english);
            }

            PrintWarning(item.info.displayName.english + " fishable info: chance: " + fishable.Chance + ", strain mod: " + fishable.StrainModifier + ", reel mod: " + fishable.ReelInSpeedMultiplier);

            var reduc = 12f;

            reduc /= fishable.StrainModifier;
            reduc *= fishable.ReelInSpeedMultiplier;


            var xp = Mathf.Clamp(32f - reduc, 8f, 32f);

            PrintWarning("base xp is 32, reduc is: " + reduc + ", 32 - " + reduc + " is: " + xp + ", strain mod: " + fishable.StrainModifier);

            HandleLevel(player, item, Skills.SURVIVAL, null, null, xp);

        }

        private object OnItemAction(Item item, string action, BasePlayer player) //gutting fish xp
        {
            if (player == null) return null;

            if (!action.Equals("gut", StringComparison.OrdinalIgnoreCase)) return null;


            var itemSwap = item?.info?.GetComponent<ItemModSwap>();

            if (itemSwap.actionEffect.isValid)
                Effect.server.Run(itemSwap.actionEffect.resourcePath, player, StringPool.Get("head"), Vector3.up, Vector3.up); //the og code uses position, but i prefer parenting to head

            try
            {
                for (int i = 0; i < itemSwap.becomeItem.Length; i++)
                {
                    var itemToBecome = itemSwap.becomeItem[i];

                    if (itemToBecome?.itemDef == null) continue;

                    var newItem = ItemManager.Create(itemToBecome.itemDef, (int)Mathf.Clamp(itemToBecome.amount, 1, int.MaxValue));

                    try
                    {
                        if (itemToBecome.itemDef.itemid != RAW_FISH_ITEM_ID) //raw fish item id
                            continue;

                        HandleLevel(player, newItem, Skills.SURVIVAL, null, null, 20); //20 xp
                    }
                    finally
                    {
                        if (!newItem.MoveToContainer(item.parent))
                            player?.GiveItem(newItem, BaseEntity.GiveItemReason.Generic);
                        else player?.Command("note.inv", newItem.info.itemid, newItem.amount, newItem.GetName(), 0);
                    }
                }

                if (itemSwap.RandomOptions.Length > 0)
                {
                    var index = UnityEngine.Random.Range(0, itemSwap.RandomOptions.Length);

                    
                    var rndOption = itemSwap.RandomOptions[index];

                    var amt = (int)Mathf.Clamp(rndOption.amount, 1, rndOption.amount);

                    var rndItem = ItemManager.Create(rndOption.itemDef, amt);

                    if (!rndItem.MoveToContainer(item.parent))
                        player?.GiveItem(rndItem, BaseEntity.GiveItemReason.Generic);
                    else player?.Command("note.inv", rndItem.info.itemid, rndItem.amount, rndItem.GetName(), 0);

                }

                return true;
            }
            finally { item?.UseItem(); }
        }

        private void OnCollectiblePickup(CollectibleEntity entity, BasePlayer player) //still needs properly implemented
        {
            if (entity == null || player == null) return;

            var items = entity.itemList;
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item.itemDef.shortname.Contains("seed")) continue;

                HandleLevel(player, item, Skills.SURVIVAL);
            }
        }

        private void OnGrowableGathered(GrowableEntity plant, Item item, BasePlayer player)
        {
            if (plant == null || item == null || player == null) return;

            if (!item.hasCondition)
            {
                var originalAmount = item?.amount ?? 0;
                var level = GetLevel(player.UserIDString, Skills.SURVIVAL);

                var gathMult = GetGathMult(level, Skills.SURVIVAL);


                var finalItemAmount = (int)Math.Round(originalAmount * gathMult, MidpointRounding.AwayFromZero);
                finalItemAmount = (int)Math.Round(finalItemAmount * GetItemMultiplier(player.UserIDString) / 100, MidpointRounding.AwayFromZero);
                item.amount = finalItemAmount;
            }

            var baseXp = 24;
            var minimumXp = 8;
            var points = (double)Mathf.Clamp((float)Math.Round(baseXp * Mathf.Clamp(item.amount, 1, 4) * Mathf.Clamp(item.amount * 0.000125f, 1, 10) * plant.healthFraction * 1.34f * (GetXPMultiplier(player.UserIDString) / 100f), MidpointRounding.AwayFromZero), minimumXp, long.MaxValue);

            var catchUp = GetCatchupMultiplier(player.UserIDString, Skills.SURVIVAL);
            if (catchUp > 0) points += (long)Math.Round(points * (catchUp / 100), MidpointRounding.AwayFromZero);

            var onEarnZXP = Interface.CallHook("OnEarnZXP", player, item, Skills.SURVIVAL, points);
            if (onEarnZXP != null)
            {
                var oldPoints = points;
                if (!double.TryParse(onEarnZXP.ToString(), out points)) points = oldPoints;
            }

            if (points <= 0.0)
            {
                PrintWarning("<= 0.0 points!");
                return;
            }

            var finalPoints = (int)Math.Round(points, MidpointRounding.AwayFromZero);

            AddPointsBySkillName(player.UserIDString, Skills.SURVIVAL, finalPoints);

            ShowXP(player, Skills.SURVIVAL, 1.75f, finalPoints);
        }

     
        private void OnMovementSpeedBonus(BasePlayer player, InputState input, ref float moveScale)
        {
            var survLvl = GetLevel(player.UserIDString, Skills.SURVIVAL);

            if (survLvl <= 1)
                return;

            var scale = survLvl * 0.0133f;

            if (player.IsDucked() || input.IsDown(BUTTON.DUCK))
                scale *= 0.33f;
            else if (!player.IsRunning())
                scale *= 0.87f;

            moveScale += scale;
        }

        private void OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return;


            object onEarnZXP = null;

            var playerVictim = info?.HitEntity as BasePlayer;
            var isNpcPlayer = playerVictim?.IsNpc ?? false;

            var scientist = info?.HitEntity as ScientistNPC;
            if (isNpcPlayer)
            {

                player.Invoke(() =>
                {
                    if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead() || !player.IsConnected) return;

                    if (playerVictim == null || playerVictim.IsDestroyed || playerVictim.IsDead())
                    {
                        var scientistPoints = Math.Round(UnityEngine.Random.Range(160, 240) * (GetXPMultiplier(player.UserIDString) / 100f), MidpointRounding.AwayFromZero);

                        var catchupMult = GetCatchupMultiplier(player.UserIDString, Skills.SURVIVAL);
                        if (catchupMult > 0) scientistPoints *= catchupMult / 100f;

                        //PrintWarning("init scientistpoints: " + scientistPoints);

                        onEarnZXP = Interface.CallHook("OnEarnZXP", player, null, Skills.SURVIVAL, scientistPoints);
                        if (onEarnZXP != null)
                        {
                            var oldPoints = scientistPoints;
                            if (!double.TryParse(onEarnZXP.ToString(), out scientistPoints)) scientistPoints = oldPoints;
                        }

                        if (scientistPoints <= 0.0)
                        {
                            PrintWarning("<= 0.0 points! was null?: " + (onEarnZXP == null));
                            return;
                        }

                        var finalPoints = (int)Math.Round(scientistPoints, MidpointRounding.AwayFromZero);

                        AddPointsBySkillName(player.UserIDString, Skills.SURVIVAL, finalPoints);
                        ShowXP(player, Skills.SURVIVAL, 1.75f, finalPoints);
                    }

                }, 0.01f);


                return;
            }

            var lootContainer = info?.HitEntity as LootContainer;
            if (lootContainer == null || lootContainer.isLootable || lootContainer is HackableLockedCrate) return; //ensure it's a barrel

            var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            var isMelee = (info?.Weapon != null) ? (info?.Weapon?.GetComponent<BaseMelee>() != null) : (info?.WeaponPrefab != null) ? (info?.WeaponPrefab?.GetComponent<BaseMelee>() != null) : (dmgType == Rust.DamageType.Blunt || dmgType == Rust.DamageType.Slash || dmgType == Rust.DamageType.Stab);

            if (!isMelee) return;


            player.Invoke(() =>
            {
                if (player == null || player.IsDestroyed || !player.IsConnected || player.gameObject == null || player.IsDead()) return;

                var dmg = info?.damageTypes?.Total() ?? 0f;
                var isDead = info?.HitEntity == null || info.HitEntity.IsDestroyed || ((info?.HitEntity as BaseCombatEntity)?.IsDead() ?? false);

                var scalar = Mathf.Clamp(1f + (dmg * 0.0175f), 1f, 16f); //crit hits

                var points = (double)UnityEngine.Random.Range(24, 97) * (isDead ? Mathf.Max(2.5f, scalar) : scalar); //where xp mult??

                var catchUp = GetCatchupMultiplier(player.UserIDString, Skills.SURVIVAL);
                if (catchUp > 0) points += Math.Round(points * (catchUp / 100), MidpointRounding.AwayFromZero);

                onEarnZXP = Interface.CallHook("OnEarnZXP", player, null, Skills.SURVIVAL, points);
                if (onEarnZXP != null)
                {
                    var oldPoints = points;
                    if (!double.TryParse(onEarnZXP.ToString(), out points)) points = oldPoints;
                }

                if (points <= 0.0)
                {
                    PrintWarning("<= 0.0 points!");
                    return;
                }

                var finalBarrelPoints = (int)Math.Round(points, MidpointRounding.AwayFromZero);

                AddPointsBySkillName(player.UserIDString, Skills.SURVIVAL, finalBarrelPoints);
                ShowXP(player, Skills.SURVIVAL, 1.75f, finalBarrelPoints);

            }, 0.01f);

            
        }

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item item)
        {
            if (player == null || item == null) return;
            
            _lastActiveItem[player] = item;
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (dispenser == null || entity == null || item == null) return;

            var player = entity as BasePlayer;
            if (player == null) return;

            var gatherType = dispenser.gatherType;

            var dispEnt = dispenser?.GetComponent<BaseEntity>();
            var resEnt = dispEnt as ResourceEntity;

            if (gatherType == ResourceDispenser.GatherType.Ore && !IsSkillDisabled(Skills.MINING)) HandleLevel(player, item, Skills.MINING, resEnt);
            else if (gatherType == ResourceDispenser.GatherType.Tree && !IsSkillDisabled(Skills.WOODCUTTING))
            {
                var toolItem = player?.GetActiveItem();

                if (toolItem == null)
                {
                    if (_lastActiveItem.TryGetValue(player, out var lastItem)) toolItem = lastItem;
                }

                HandleLevel(player, item, Skills.WOODCUTTING, resEnt, toolItem);

            }
            else if (gatherType == ResourceDispenser.GatherType.Flesh && !IsSkillDisabled(Skills.SURVIVAL) && !(dispEnt is PlayerCorpse))
            {
                HandleLevel(player, item, Skills.SURVIVAL, resEnt);
            }
        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            if (dispenser == null || player == null || item == null) return;

            var skill = Skills.MINING;
            var level = GetLevel(player.UserIDString, skill);

            var finalItemAmount = (int)Math.Round(item.amount * GetGathMult(level, skill), MidpointRounding.AwayFromZero);
            finalItemAmount = (int)Math.Round(finalItemAmount * GetItemMultiplier(player.UserIDString) / 100, MidpointRounding.AwayFromZero);

            item.amount = finalItemAmount;
        }

        private void HandleLevel<T>(BasePlayer player, T item, string skill, ResourceEntity entity = null, Item forceActiveItem = null, double overridePointsToGet = 0f)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));


            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                var asItem = item as Item;
                var asItemAmount = item as ItemAmount;

                if (asItem == null && asItemAmount == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                if (!player.IsConnected) return;

                var toolItem = forceActiveItem ?? player?.GetActiveItem();
                if (toolItem?.name?.Contains("DM Item") ?? false) return;

                var xpMultiplier = GetXPMultiplier(player.UserIDString);

                var lvlInfo = (GetLvlInfo(player.UserIDString, skill) ?? null) ?? NewLvlInfo(player.UserIDString, skill, 10, 1);

                if (lvlInfo.Level < 1 || lvlInfo.XP < 10)
                {
                    SetLvlInfo(player.UserIDString, skill, 10);
                    lvlInfo = GetLvlInfo(player.UserIDString, skill);
                }

                var canEarnXP = Interface.CallHook("CanEarnZXP", player, item, skill);
                if (canEarnXP != null && (canEarnXP is bool) && !(bool)canEarnXP) return;

                var xpPercentBefore = GetExperiencePercent(player.UserIDString, skill);

                long Level = lvlInfo.Level;
                long Points = lvlInfo.XP;
                var initLevel = Level;

                
                var originalAmount = asItem?.amount ?? (int)(asItemAmount?.amount ?? 0);
                var finalItemAmount = asItem?.amount ?? (int)(asItemAmount?.amount ?? 0);

                var toolMelee = (toolItem != null) ? (toolItem?.GetHeldEntity() as BaseMelee) ?? null : null;
                var repeatDelay = (toolMelee != null) ? (toolMelee?.repeatDelay ?? 0f) : 0f;
                //   var itemProf = (toolMelee != null) ? (toolMelee?.gathering?.GetProficiency() ?? -1f) : -1f;
                var toolId = toolItem?.info?.itemid ?? 0;


                finalItemAmount = (int)Math.Round(originalAmount * GetGathMult(Level, skill), MidpointRounding.AwayFromZero);
                finalItemAmount = (int)Math.Round(finalItemAmount * GetItemMultiplier(player.UserIDString) / 100, MidpointRounding.AwayFromZero);

                var pointsToGet = overridePointsToGet != 0 ? overridePointsToGet : GetPointsPerHit(skill);
                if (pointsToGet < 0)
                {
                    PrintWarning("Got bad points for skill: " + skill);
                    return;
                }

                var preModifiedPoints = pointsToGet;

                pointsToGet *= xpMultiplier / 100f;

                var catchUp = GetCatchupMultiplier(player.UserIDString, skill);
                if (catchUp > 0) pointsToGet += pointsToGet * (catchUp / 100f); //*= mult / 100f?

                var itemName = asItem?.info?.shortname ?? asItemAmount?.itemDef?.shortname ?? string.Empty;

                var itemId = asItem?.info?.itemid ?? asItemAmount?.itemid ?? 0;

                if (skill == Skills.WOODCUTTING && (itemId == ClothID || itemId == CactusFleshID)) pointsToGet *= 2.6f;


                if (item != null)
                {
                    var category = asItem?.info?.category ?? asItemAmount?.itemDef?.category ?? ItemCategory.All;

                    if (category == ItemCategory.Food) finalItemAmount = Mathf.Clamp((int)Math.Round(finalItemAmount * 0.33f, MidpointRounding.AwayFromZero), originalAmount, 999);

                    if (asItem != null)
                        asItem.amount = finalItemAmount;
                    else if (asItemAmount != null)
                        asItemAmount.amount = finalItemAmount;
                }

                //if your points to get was 32, this will result in 40 (code below)
                pointsToGet += pointsToGet * (originalAmount * 0.5 / 100f);

                if (toolItem != null)
                {
                    var toolCondNorm = toolItem?.conditionNormalized ?? 0;
                    pointsToGet += Math.Round(toolCondNorm * 3, MidpointRounding.AwayFromZero);
                }


                //XP Reductions Start
                var lvlReductionPerc = 0;

                if (Level >= 95)
                    lvlReductionPerc = 92;
                else if (Level >= 90)
                    lvlReductionPerc = 87;
                else if (Level >= 85)
                    lvlReductionPerc = 81;
                else if (Level >= 80)
                    lvlReductionPerc = 75;
                else if (Level >= 75)
                    lvlReductionPerc = 68;
                else if (Level >= 70)
                    lvlReductionPerc = 64;
                else if (Level >= 65)
                    lvlReductionPerc = 55;
                else if (Level >= 60)
                    lvlReductionPerc = 50;
                else if (Level >= 55)
                    lvlReductionPerc = 42;
                else if (Level >= 50)
                    lvlReductionPerc = 38;
                else if (Level >= 45)
                    lvlReductionPerc = 30;
                else if (Level >= 40)
                    lvlReductionPerc = 25;
                else if (Level >= 35)
                    lvlReductionPerc = 20;
                else if (Level >= 30)
                    lvlReductionPerc = 15;
                else if (Level >= 25)
                    lvlReductionPerc = 8;

                if (lvlReductionPerc > 0)
                {
                    pointsToGet -= pointsToGet * lvlReductionPerc / 100;
                }

                if (toolId != RockID)
                {
                    if (skill == Skills.WOODCUTTING && (toolId == CHAINSAW_ID || !IsHatchetItemID(toolId)))
                        pointsToGet *= 0.33f;


                    if (skill == Skills.MINING)
                    {
                        if (toolId == JACKHAMMER_ID) pointsToGet *= 0.231221;
                        else if (!IsPickaxeItemID(toolId)) pointsToGet *= 0.266;
                    }
                }

                //End reductions

                var onEarnZXP = Interface.CallHook("OnEarnZXP", player, item, skill, pointsToGet);
                if (onEarnZXP != null)
                {
                    var oldPoints = pointsToGet;
                    if (!double.TryParse(onEarnZXP.ToString(), out pointsToGet)) pointsToGet = oldPoints;
                }


                var longPoints = Convert.ToInt64(Math.Round(pointsToGet, MidpointRounding.AwayFromZero));

                AddPointsByInfo(lvlInfo, longPoints, player);

                if (xpHitOff.Contains(player.UserIDString))
                {
                    if (xpPercentBefore != GetExperiencePercent(player.UserIDString, skill)) GUIUpdateSkill(player, skill);
                }
                else
                {
                    if (longPoints > 0) ShowXP(player, skill, (repeatDelay != 0f) ? repeatDelay : 1.25f, longPoints);
                }

                if (skill == Skills.MINING && entity != null)
                {
                    var oreRes = entity as OreResourceEntity;
                    if (oreRes != null)
                    {
                        var gatherTool = player?.GetHeldEntity() as BaseMelee;
                        var gatherInfo = gatherTool?.GetGatherInfoFromIndex(skill == Skills.MINING ? ResourceDispenser.GatherType.Ore : skill == Skills.WOODCUTTING ? ResourceDispenser.GatherType.Tree : skill == Skills.SURVIVAL ? ResourceDispenser.GatherType.Flesh : ResourceDispenser.GatherType.UNSET) ?? null;
                        var destroyFrac = gatherInfo?.destroyFraction ?? -1f;
                        if (destroyFrac != -1f && destroyFrac < 0.15)
                        {
                            player.Invoke(() =>
                            {
                                if (oreRes?.IsDestroyed ?? true)
                                {
                                    var xpBonus = longPoints * 5;
                                    AddPointsByInfo(lvlInfo, xpBonus, player);
                                    if (!xpHitOff.Contains(player.UserIDString)) ShowXP(player, skill, (repeatDelay != 0f) ? repeatDelay : 1.25f, xpBonus);
                                }
                            }, 0.01f);
                        }
                    }
                }

            }
            finally
            {
                try { if (watch.ElapsedMilliseconds > 3) PrintWarning(nameof(HandleLevel) + " took: " + watch.ElapsedMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }

        }

        private void ShowXP(BasePlayer player, string skill, float time = 1.25f, long points = -1)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            if (!player.IsConnected) return;

            GUIUpdateSkill(player, skill, (int)points);


            if (time > 0.0f)
                player?.Invoke(() => GUIUpdateSkill(player, skill), time + 0.1f);
        }

        private void AddPointsBySkillName(string userID, string skill, long points, float showXpTime = 0f)
        {
            if (points == 0) return;
            var curPoints = GetPoints(userID, skill);
            if (curPoints < 1) return;

            var newPoints = curPoints + points;
            HandlePointsBySkillName(userID, skill, newPoints);

            if (showXpTime > 0f)
                ShowXP(RelationshipManager.FindByID(ulong.Parse(userID)), skill, showXpTime, points);

            //  SetLastXP(userID, skill, points);
        }

        private void AddPointsByInfoID(LevelInfo info, long pointsToAdd, string userId = "")
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (pointsToAdd == 0) return;

            HandlePointsByInfoID(info, info.XP + pointsToAdd, userId);

        }

        private void AddPointsByInfo(LevelInfo info, long pointsToAdd, BasePlayer player = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (pointsToAdd == 0) return;

            HandlePointsByInfo(info, info.XP + pointsToAdd, player);
        }

        private readonly string[] allowedColors = { "", "#75ff47", "#459b20", "#8800cc", "#3385ff", "#e6e600", "#669900", "#4d4dff", "#cc0000", "#33ffbb", "#ff00ff", "#725142", "#595959", "#00ffff", "#ffb3cc", "#5c8a8a", "#e6004c", "#db890f", "#ce7235", "#e64d00", "#2c528c", "#42f4e8", "#494f4e", "#b5c40f", "#681144", "#562c2c", "#356656", "#57776d", "#255143", "#1e7258", "#3c3051", "#512f8e", "#2c1d7c", "#DD0000", "#8877ad", "#736b84", "#ff72ad", "#b70b53", "#eaa8d3", "#770750", "#3144a0", "#55a4c1", "#4332fc", "#1d5b77", "#1281C6", "#3E3E3E", "#661B77", "#FACD78", "#E4DCA4", "#4F4533", "#ffe6e6", "#89dc93", "#2B5116", "#BE2227", "#c5a787", "#FF0030", "#FE950A", "#FEF506", "#53D559", "#2BACE2", "#9E88D2", "#58767D", "#525354", "#95A26D", "#552E84", "#939296", "#9B5B5D", "#cd2a2a", "#8D1028", "#C40C23", "#D95A19", "#ECA013", "#6B3421", "#733053", "#FBFF93", "#F9D0DD", "#ffe14a", "#ffabe9", "#ffe0f3", "#c7fcff", "#45f5ff", "#ccf4ff", "#fcffcc", "#f2ff26", "#ff8e52", "#fdebff", "#f494ff", "#c3a1ff", "#7b30ff", "#ff91e7", "#ff00c7", "#ffbff1", "#ff82e4", "#cfff82", "#b5ffd6", "#a0ff52", "#ffdd94", "#ff8fa6", "#2f1cff", "#1c20ff", "#7376ff", "#999bff", "#1d20cc", "#b09419", "#12628a", "#198791", "#37b38e", "#a32a5d", "#992c90", "#2e992c", "#55ab33", "#8aba3d", "#cee64c", "#faaf64", "#ffa587", "#ff9587", "#ff7070", "#8fdbff", "#532696", "#96267e", "#177578", "#006366", "#00521d", "#474000", "#402800", "#421300", "#6e2102", "#8f0c00", "#1b00b3", "#69111e", "#380000" };

        public static T RandomElement<T>(IEnumerable<T> source, System.Random rng)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }

        private void HandleLevelUp(BasePlayer player, LevelInfo info)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (info == null) throw new ArgumentNullException(nameof(info));

            if (!player.IsConnected) return;

            var level = info.Level;
            var points = info.XP;

            var userId = player.UserIDString;

            // CelebrateEffects(player, UnityEngine.Random.Range(1, 5));

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var skillMsgTxt = messages[sb.Clear().Append(info.Skill).Append("Skill").ToString()];

                var skillColor = sb.Clear().Append("<color=").Append(colors[info.Skill]).Append(">").ToString();
                ShowPopup(userId, sb.Clear().Append(skillColor).Append("<size=18>").Append(skillMsgTxt).Append("</size></color>").Append(Environment.NewLine).Append("Level <size=20>").Append(skillColor).Append(level.ToString("N0")).Append("</color></size> reached!").ToString());

                SendReply(player, string.Format(skillColor + (string)messages["LevelUpText"] + "</color>",
                    skillMsgTxt,
                    level,
                    points.ToString("N0"),
                    GetLevelPoints(level + 1).ToString("N0"),
                    ((GetGathMult(level, info.Skill) - 1) * 100).ToString("0.##")
                    )
                );
            }
            finally { Pool.Free(ref sb); }

            if (level >= 99)
                GrantMasteryAchievement(player, info.Skill);

        }

        private void HandlePointsByInfo(LevelInfo info, long points, BasePlayer player = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (points == 0) return;

            info.XP = points;

            var cap = GetLevelCap(info.Skill, (player?.UserIDString ?? string.Empty));


            var Level = info.Level;
            var initLevel = Level;

            if (cap > 0 && Level + 1 <= cap && points >= GetLevelPoints(Level + 1))
            {
                info.Level = Level = GetPointsLevel(points, info.Skill);
                if (info.Level > cap) info.Level = cap;


                HandleLevelUp(player, info);
            }

            if ((Level == cap || cap > 99 && Level == 99) && initLevel != Level)
            {
                PrintWarning("LVL OBJ!");
                if (messages.TryGetValue("MasteryText", out var lvlMsgObj) && lvlMsgObj != null)
                {
                    var useColors = allowedColors.Where(p => !string.IsNullOrEmpty(p));

                    var lvlMsg = string.Format(lvlMsgObj.ToString(), player?.displayName ?? "Unknown", Level, "<color=" + colors[info.Skill] + ">" + messages[info.Skill + "Skill"] + "</color>");

                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var p = BasePlayer.activePlayerList[i];
                        if (p == null || p.IsDestroyed || !p.IsConnected || p.IsDead()) continue;

                        p.Invoke(() =>
                        {
                            if (p == null || p.IsDestroyed || !p.IsConnected || p.IsDead()) return;
                            var max = 15;
                            var time = 0.066f;
                            var timerI = 0;
                            for (int j = 0; j < max; j++)
                            {
                                if (p == null || p.IsDestroyed || !p.IsConnected || p.IsDead()) break;
                                timer.Once(time, () =>
                                {
                                    if (p == null || p.IsDestroyed || !p.IsConnected || p.IsDead()) return;
                                    timerI++;
                                    var adjMsg = "<color=" + RandomElement(useColors, new System.Random()) + "><size=" + UnityEngine.Random.Range(10, 29) + ">" + lvlMsg + "</size></color>";
                                    if (i < 2) PrintWarning("adjmsg: " + adjMsg);
                                    var tipMsg = "<color=" + RandomElement(useColors, new System.Random()) + ">" + lvlMsg + "</color>";
                                    SendReply(p, adjMsg);
                                    ShowPopup(p.UserIDString, tipMsg, time / 1.5f);
                                });
                                time += 0.045f + (0.00555f * (i / 1.5f)) + ((i > 5) ? 0.0525f : 0f);
                            }
                        }, 0.1f);
                    }

                    CelebrateEffects(player, 7);
                    PrintWarning("Lvl msg: " + lvlMsg);
                }
                else PrintWarning("not get/unnull");
            }

            //info.Level = Level;
            //    info.XP = points;
        }

        private void HandlePointsByInfoID(LevelInfo info, long points, string userId = "")
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            var pObj = covalence.Players?.FindPlayerById(userId)?.Object as BasePlayer;
            HandlePointsByInfo(info, points, pObj);
        }

        private void HandlePointsBySkillName(string userID, string skill, long points)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(skill) || points <= 0) throw new ArgumentNullException();

            var info = GetLvlInfo(userID, skill);
            if (info == null)
            {
                PrintError(nameof(HandlePointsBySkillName) + " got null info for skill: " + skill + " !!");
                return;
            }

            HandlePointsByInfoID(info, points, userID);


        }
        #endregion

        #region Utility

        private bool IsRadStormActive() => (RadRain?.Call<bool>("IsRadStormActive") ?? false);

        private bool IsHatchetItem(Item item)
        {
            return IsHatchetItemID(item.info.itemid);
        }

        private bool IsHatchetItemID(int itemId)
        {
            return itemId switch
            {
                CHAINSAW_ID or SALVAGED_AXE_ID or HATCHET_ID or STONE_AXE_ID or PROTO_AXE_ID or CONCRETE_AXE_ID or ABYSS_AXE_ID => true,
                _ => false,
            };
        }

        private bool IsPickaxeItem(Item item)
        {
            return IsPickaxeItemID(item.info.itemid);
        }

        private bool IsPickaxeItemID(int itemId)
        {
            return itemId switch
            {
                PICKAXE_ID or ICE_PICK_ID or JACKHAMMER_ID or STONE_PICKAXE_ID or PROTO_PICK_ID or CONCRETE_PICK_ID or ABYSS_PICK_ID => true,
                _ => false,
            };
        }

        private void GetAllPlayerIDsNoAlloc(ref List<string> list)
        {
            PlayersByDatabase?.Call("GetAllPlayerIDsNoAlloc", list);
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private readonly Dictionary<string, Timer> popupTimer = new();

        private void ShowPopup(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException(nameof(Id));
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException(nameof(duration));

            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;

            player.Command("gametip.showgametip", msg);
            if (popupTimer.TryGetValue(Id, out var endTimer)) endTimer.Destroy();
            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });
            popupTimer[Id] = endTimer;
        }

        private BasePlayer FindLooterFromCrate(BaseEntity crate)
        {
            if (crate == null) throw new ArgumentNullException(nameof(crate));

            BasePlayer foundLooter = null;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || player.IsDead() || !player.IsConnected) continue;

                var lootSource = player?.inventory?.loot?.entitySource ?? null;
                if (lootSource == crate)
                {
                    if (foundLooter != null) return null;
                    else foundLooter = player;
                }
            }

            return foundLooter;
        }

        private BasePlayer FindJustConsumed()
        {
            var justConsumed = Pool.GetList<BasePlayer>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? false) || (player?.IsSleeping() ?? false) || player?.metabolism == null) continue;

                    var lastConsumeObj = lastConsumeTime.GetValue(player.metabolism);
                    if (lastConsumeObj == null)
                    {
                        PrintWarning("Null lastConsumeObj for: " + player.displayName);
                        continue;
                    }

                    if ((float)lastConsumeObj <= 0.085f) justConsumed.Add(player);
                }

                return justConsumed.Count == 1 ? justConsumed[0] : null;
            }
            finally { Pool.FreeList(ref justConsumed); }
        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException();
            try
            {
                var p = covalence.Players.FindPlayer(nameOrIdOrIp);
                if (p != null) if ((!p.IsConnected && tryFindOfflineIfNoOnline) || p.IsConnected) return p;
                var connected = covalence.Players.Connected;
                List<IPlayer> players = new List<IPlayer>();
                foreach (var player in connected)
                {
                    var IP = player?.Address ?? string.Empty;
                    var name = player?.Name ?? string.Empty;
                    var ID = player?.Id ?? string.Empty;
                    if (name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || IP == nameOrIdOrIp || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || ID == nameOrIdOrIp) players.Add(player);
                }
                if (players.Count <= 0 && tryFindOfflineIfNoOnline)
                {
                    foreach (var offlinePlayer in covalence.Players.All)
                    {
                        if (offlinePlayer.IsConnected) continue;
                        var name = offlinePlayer?.Name ?? string.Empty;
                        var ID = offlinePlayer?.Id ?? string.Empty;
                        if (name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || ID == nameOrIdOrIp) players.Add(offlinePlayer);
                    }
                }

                return players.Count == 1 ? players[0] : null;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString() + " ^ FindConnectedPlayer ^ ");
                return null;
            }
        }

        private bool HasXPHit(string userID) { return !xpHitOff.Contains(userID); }

        private long GetLevelPoints(long level)
        {
            if (level > 99) return GetRuneScapeExperienceByLevel(level);

            return 110 * level * level - 100 * level;
        }

        private long GetPointsLevel(long points, string skill)
        {
            var watch = Pool.Get<Stopwatch>();
                
            try 
            {
                watch.Restart();

                if (string.IsNullOrEmpty(skill)) return -1;

                var lvlCap = GetLevelCap(skill);

                var x2 = (int)((100 + Math.Sqrt(100 * 100 + 4 * 110 * points)) / (2 * 110));
                
                if (lvlCap > 99 && x2 > 99)
                {
                    PrintWarning("lvlCap > 99 and x2 > 99: " + lvlCap + ", " + x2 + ", returning runescape xp handling");
                    var runeScapeLvl = GetRuneScapeLevelByExperience(points);
                    if (runeScapeLvl > 99) return runeScapeLvl;
                    // return GetRuneScapeLevelByExperience(points); // > level 99 RSxp handling
                }

                if (x2 > lvlCap)
                {
                    PrintWarning("x2 > lvlCap, so returning lvlCap");
                    return lvlCap;
                }


                return (lvlCap == 0 || x2 <= lvlCap) ? x2 : lvlCap;
            }
            finally
            {
                try { if (watch.Elapsed.TotalMilliseconds >= 1) PrintWarning(watch.Elapsed.TotalMilliseconds + "ms passed for: " + nameof(GetPointsLevel) + " (" + points + ", " + skill + ")"); }
                finally { Pool.Free(ref watch); }
            }
        }

        public long GetRuneScapeExperienceByLevel(long lvl)
        {
            if (lvl <= 0) return 0;

            var a = 0L;

            for (int x = 1; x < lvl; x++) a += (long)(x + 300 * Math.Pow(2, x / (double)7));

            var actualRSXP = (long)(a / (double)4); //RSXP experience

            return (long)Math.Round(actualRSXP * 0.165, MidpointRounding.AwayFromZero); //scale it down for Rust, this isn't an infinitely lasting MMO
        }

        public long GetRuneScapeLevelByExperience(long experience, int max = 1000)
        {
            var count = 0;
            var start = 1; //level can't be 0, so start at 1
            var dic = new Dictionary<int, long>();
            var lvl = -1;
            while (true)
            {
                count++;
                if (count >= max) break; //safety break
                var xpLvl = GetRuneScapeExperienceByLevel(start);
                if (xpLvl == experience)
                {
                    lvl = start;
                    break;
                }
                var xpNext = GetRuneScapeExperienceByLevel(start + 1);
                var diff = (xpLvl > experience) ? (xpLvl - experience) : (xpLvl < experience) ? (experience - xpLvl) : 0;
                dic[start] = diff;
                if (experience < xpNext) break;
                start++;
            }
            if (lvl == -1)
            {
                var lowestDiff = dic?.Where(p => p.Value == dic.Values.Min())?.FirstOrDefault() ?? null;
                if (lowestDiff.HasValue) lvl = lowestDiff.Value.Key;
            }
            return lvl;
        }

        private double GetGathMult(long skillLevel, string skill) 
        {
            var val = 1 + Convert.ToDouble(resourceMultipliers[skill]) * 0.1 * (skillLevel - 1);

            if (IsRadStormActive())
                val *= 3; //200% bonus.

            return val;
        }

        private double GetXPMultiplier(string userID)
        {
            if (string.IsNullOrEmpty(userID))
                throw new ArgumentNullException(nameof(userID));

            var bonusXPPerc = IsVIP(userID) ? 30d : IsMVP(userID) ? 15d : 0;
            if (permission.UserHasPermission(userID, "zlevelsremastered.2xxp")) 
                bonusXPPerc += 100d;

            if (IsRadStormActive())
                bonusXPPerc += 200d;

            return 100d + bonusXPPerc;
        }

        private double GetCatchupMultiplier(string userID, string skillName)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            if (string.IsNullOrEmpty(skillName)) throw new ArgumentNullException(nameof(skillName));

            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();
                var getLvl = GetLevel(userID, skillName);
                if (getLvl < 1)
                {
                    PrintWarning("getLvl returned < 1 for: " + userID + " on: " + skillName);
                    return -1;
                }

                var topList = GetCachedTop5(skillName);
                // GetTop5NoAlloc(skillName, ref topList, userID);

                if (topList?.Count < 1) return -1;

                var topLvls = Pool.GetList<long>();
                try
                {
                    for (int i = 0; i < topList.Count; i++)
                    {
                        var topID = topList[i];
                        var topLvl = GetLevel(topID, skillName);
                        if (topLvl > 0) topLvls.Add(topLvl);
                    }

                    var val = 0d;

                    if (topLvls.Count > 0)
                    {
                        var lvlsCount = topLvls.Count;
                        var lvlsSum = 0L;

                        for (int i = 0; i < lvlsCount; i++)
                        {
                            var topLvl = topLvls[i];
                            lvlsSum += topLvl;
                        }

                        double topLvlAvg = lvlsSum / lvlsCount;

                        var avgTop = Math.Round(topLvlAvg, MidpointRounding.AwayFromZero);
                        if (avgTop >= 40) val = (avgTop > getLvl) ? avgTop - getLvl + (12.50 * (avgTop - getLvl)) : 0;
                    }

                    var hookVal = Interface.Oxide.CallHook("OnGetCatchupMultiplier", userID, skillName, val, getLvl, topLvls);

                    if (hookVal != null)
                        val = (double)hookVal;

                    return val;
                }
                finally { Pool.FreeList(ref topLvls); }
            }
            finally
            {
                try { if (watch.ElapsedMilliseconds > 2) PrintWarning(nameof(GetCatchupMultiplier) + " took: " + watch.ElapsedMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }

        }

        private double GetItemMultiplier(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return 100;
            var baseAmt = 100;
            var bonusItemPerc = IsVIP(userID) ? 25 : IsMVP(userID) ? 10 : 0;
            return baseAmt + bonusItemPerc;
        }

        #endregion

        #region Saving
        private void OnServerSave()
        {
            ServerMgr.Instance.Invoke(() => UpdateTop5AllSkills(), 12f);

            ServerMgr.Instance.StartCoroutine(SaveAllData());
        }

        private System.Collections.IEnumerator SaveAllData()
        {
            yield return CoroutineEx.waitForSeconds(2.5f);
            SaveXPData();
        }

        private void Unload()
        {
            try { DestroyGUIAllSkills(); }
            finally { SaveXPData(); }


            if ((PRISMAchievements?.IsLoaded ?? false))
            {
                PrintWarning("hasAchievements was true, unregistering achievements");
                PRISMAchievements.Call("UnregisterAllPluginAchievements", Name);
                PrintWarning("Called unregister achievements");
            }

        }
        #endregion

        #region Config
        private Dictionary<string, object> resourceMultipliers;
        private Dictionary<string, object> levelCaps;
        private readonly Dictionary<string, int> intLevelCaps = new();
        private readonly Dictionary<string, double> pointsPerHit = new();
        private Dictionary<string, object> pointsPerHitObj;
        private Dictionary<string, object> messages;

        protected override void LoadDefaultConfig() { }
        private void SaveXPData() => Interface.Oxide.DataFileSystem.WriteObject("ZLevelsXPData", xpData);

        private void OnServerInitialized()
        {
            if (IsSkillDisabled(Skills.SURVIVAL))
            {
                Unsubscribe(nameof(OnCollectiblePickup));
                Unsubscribe(nameof(OnGrowableGathered));
                Unsubscribe(nameof(OnPlayerAttack));
            }
            else
            {
                Subscribe(nameof(OnCollectiblePickup));
                Subscribe(nameof(OnGrowableGathered));
                Subscribe(nameof(OnPlayerAttack));
            }

            var hasAchievements = PRISMAchievements?.IsLoaded ?? false;
            if (hasAchievements)
            {
                PrintWarning("hasAchievements was true, registering achievements");

                var sb = Pool.Get<StringBuilder>();

                try
                {
                    for (int i = 0; i < Skills.ALL.Length; i++)
                    {
                        var skill = Skills.ALL[i];

                        var skillDisplayName = messages[sb.Clear().Append(skill).Append("Skill").ToString()];

                        PRISMAchievements.Call("RegisterAchievement", sb.Clear().Append(Name).Append("_").Append(skill).Append("_mastery").ToString(), sb.Clear().Append(skillDisplayName).Append(" mastery").ToString(), sb.Clear().Append("Reach level 99 in ").Append(skillDisplayName).Append("!").ToString(), 5, 750);
                    }
                }
                finally { Pool.Free(ref sb); }


                PrintWarning("Called register achievements");
            }


            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];

                if ((p?.IsConnected ?? false)) CreateGUIAllSkills(p);
            }

            UpdateTop5AllSkills();
        }

        private void Init()
        {
            try
            {
                string[] topCommands = { "topskills", "topskill", "topstat", "topstats", "toplevels", "toplvls" };
                string[] statsCommands = { "stats", "skills", "levels", "lvls" };
                AddCovalenceCommand(topCommands, "StatsTopCommand");
                AddCovalenceCommand(statsCommands, "StatsCommand");

                skillColors["WC"] = "0.592 0.486 0.274 1";
                //  skillColors.Add("WC", "0.8 0.4 0 1");
                skillColors.Add("M", "0.43 0.65 0.75 1");
                //  skillColors.Add("M", "0.1 0.5 0.8 1");
                skillColors.Add("S", "0.5 0.48 0.4 1");
                //  skillColors.Add("S", "0.8 0.1 0 1");
                skillColors.Add("C", "0.2 0.72 0.5 1");



                if ((xpData = Interface.Oxide.DataFileSystem.ReadObject<XPData>("ZLevelsXPData")) == null) xpData = new XPData();



                resourceMultipliers = CheckCfg("ResourcePerLevelMultiplier", new Dictionary<string, object>{
                {Skills.WOODCUTTING, 2.0d},
                {Skills.MINING, 2.0d},
                {Skills.SURVIVAL, 2.0d}
            });
                levelCaps = CheckCfg("LevelCaps", new Dictionary<string, object>{
                {Skills.WOODCUTTING, 0},
                {Skills.MINING, 0},
                {Skills.SURVIVAL, 0}
            });
                foreach (var kvp in levelCaps) { if (kvp.Value is int) intLevelCaps[kvp.Key] = Convert.ToInt32(kvp.Value); }

                if (intLevelCaps.Count != levelCaps.Count) PrintWarning("intLevelCaps != levelCaps count!!!!");

                pointsPerHitObj = CheckCfg("PointsPerHit", new Dictionary<string, object>{
                {Skills.WOODCUTTING, 30},
                {Skills.MINING, 30},
                {Skills.SURVIVAL, 30}
            });

                foreach (var kvp in pointsPerHitObj)
                {
                    if (!double.TryParse(kvp.Value.ToString(), out var val)) PrintWarning("not a double!: " + kvp.Value);
                    else pointsPerHit[kvp.Key] = val;
                }

                /*/
                percentLostOnDeath = checkCfg("PercentLostOnDeath", new Dictionary<string, object>{
                {Skills.WOODCUTTING, 100},
                {Skills.MINING, 100},
                {Skills.SKINNING, 50}
            });/*/

                messages = CheckCfg("Messages", new Dictionary<string, object>{
                {"StatsHeadline", "Level stats (/statinfo [statname] - To get more information about skill)"},
                {"StatsText",   "-{0}"+
                            "\nLevel: {1} (+{4}% bonus) \nXP: {2}/{3} [{5}].\n<color=red>-{6} XP loose on death.</color>"},
                {"LevelUpText", "{0} Level up"+
                            "\nLevel: {1} (+{4}% bonus) \nXP: {2}/{3}"},
                    {"MasteryText", "{0} has reached level {1} in {2}!" },
                {"WCSkill", "Woodcutting"},
                {"MSkill", "Mining"},
                {"SSkill", "Survival"}
            });
                SaveConfig();
                permission.RegisterPermission("zlevelsremastered.2xxp", this);
                permission.RegisterPermission("zlevelsremastered.vip", this);


            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintError("Failed to complete Init()");
            }
        }

        private bool IsMVP(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            var perms = permission.GetUserPermissions(userID);
            if (perms != null && perms.Length > 0)
            {
                for (int i = 0; i < perms.Length; i++) { if (perms[i].Contains(".mvp")) return true; }
            }
            return false;
        }

        private bool IsVIP(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            var perms = permission.GetUserPermissions(userID);
            if (perms != null && perms.Length > 0)
            {
                for (int i = 0; i < perms.Length; i++) { if (perms[i].Contains(".vip")) return true; }
            }
            return false;
        }

        private T CheckCfg<T>(string conf, T def)
        {
            if (Config[conf] != null)
            {
                return (T)Config[conf];
            }
            else
            {
                Config[conf] = def;
                return def;
            }
        }
        #endregion

        #region Gets&Sets

        private long GetPoints(string userID, string skill) { return GetLvlInfo(userID, skill)?.XP ?? -1; }

        private long GetLevel(string userID, string skill) { return GetLvlInfo(userID, skill)?.Level ?? 0; }

        private int GetLevelCap(string skill, string userId = "")
        {

            if (!intLevelCaps.TryGetValue(skill, out var cap)) PrintWarning("no cap get fromm dictionary for: " + skill);

            var hookVal = Interface.Oxide.CallHook("OnGetLevelCap", skill, cap, userId);
            if (hookVal != null)
                cap = (int)hookVal;

            return cap;
        }

        #endregion


        #region New stuff

        private bool IsSkillDisabled(string skill)
        {
            if (string.IsNullOrEmpty(skill)) throw new ArgumentNullException(nameof(skill));

            if (intLevelCaps.TryGetValue(skill, out var capsOut)) return capsOut < 0;

            return true;
        }

        private double GetExperiencePerc(string userID, string skill)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(skill)) return -1;

            var Level = GetLevel(userID, skill);

            var lvlCap = GetLevelCap(skill, userID);

            if (Level >= lvlCap)
                return 0;

            var startingPoints = GetLevelPoints(Level);
            var nextLevelPoints = GetLevelPoints(Level + 1) - startingPoints;
            var Points = GetPoints(userID, skill) - startingPoints;

            var val = Points / (double)nextLevelPoints * 100f;


            if (val < 0.5) val = 0.5;
            else if (val > 99.0) val = 99.0;

            return val;
        }

        private string GetExperiencePercent(string userID, string skill)
        {
            var sb = Pool.Get<StringBuilder>();
            try { return sb.Clear().Append(GetExperiencePerc(userID, skill).ToString("0")).Append("%").ToString(); }
            finally { Pool.Free(ref sb); }
        }

        private string GetSkillColor(string skillName)
        {


            if (!colors.TryGetValue(skillName, out var color) && !colors.TryGetValue(skillName.ToLower(), out color))
                return string.Empty;


            return color;
        }

        #endregion
    }
}
