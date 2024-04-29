using CompanionServer;
using ConVar;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Game.Rust.Libraries.Covalence;
using Rust.Workshop;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Connection = Network.Connection;
using Net = Network.Net;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Compilation", "Shady", "1.0.0", ResourceId = 0)]
    internal class Compilation : RustPlugin
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

        /*/
        * God With Us. (Gott Mit Uns)
        * /*/

        //i purple you

        private const string CHAT_STEAM_ID = "76561198865536053";
        private const string SHOCK_EFFECT = "assets/prefabs/locks/keypad/effects/lock.code.shock.prefab";
        private const string PRISM_RAINBOW_TXT = "<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color>";
        private const string PRISM_UX_CHAT_PREFIX = "<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color> | <color=#fffcb5>UX:</color> ";


        private const uint TEST_RIDABLE_HORSE_PREFAB_ID = 2421623959;

        private const uint MURDERER_CORPSE_PREFAB_ID = 2400390439;

        private const int SCRAP_ITEM_ID = -932201673;

        private readonly HashSet<int> _ignoreItems = new() { 878301596, -2027988285, -44066823, -1364246987, 62577426, 1776460938, -17123659, 999690781, -810326667, 996757362, 1015352446, 1414245162, -1449152644, -187031121, 1768112091, 1770744540, -602717596, 1223900335, 1036321299, 204970153, 236677901, -399173933, 1561022037, 1046904719, 1394042569, -561148628, 861513346, -1366326648, -1884328185, 1878053256, 550753330, 696029452, 755224797, 1230691307, 1364514421, -455286320, 1762167092, -282193997, 180752235, -1386082991, 70102328, 22947882, 81423963, 1409529282, -1779180711, -277057363, -544317637, 1113514903 };
        private readonly HashSet<string> _forbiddenTags = new() { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new("(<size=.+?>)", RegexOptions.Compiled);
        private readonly Regex _hexRegex = new(@"^#(?:[0-9a-fA-F]{3}){1,2}$", RegexOptions.Compiled);

        //private readonly Regex _discordServerEmojiRegex = new Regex("<:[a-zA-Z0-9]+:[0-9]+>", RegexOptions.Compiled);
        private readonly Regex _discordServerEmojiRegex = new("<:[a-zA-Z0-9_]+:[0-9]+>", RegexOptions.Compiled);

        private readonly StringBuilder _decaySbBuffer = new();

        private readonly HashSet<Coroutine> _coroutines = new();


        private readonly Dictionary<string, StringBuilder> rocketBuffer = new();

        // private readonly FieldInfo lastWoundedTime = typeof(BasePlayer).GetField("lastWoundedTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly FieldInfo curList = typeof(InvokeHandler).GetField("curList", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly FieldInfo restartCoroutine = typeof(ServerMgr).GetField("restartCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly FieldInfo upkeepBrackets = typeof(BuildingPrivlidge).GetField("upkeepBrackets", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly BaseEntity.RPCMessage _defaultMessage = new();

        private readonly int _constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        public readonly int deployColl = LayerMask.GetMask(new string[] { "Deployable", "Deployed" });
        private const int CONSTRUCTION_LAYER = Rust.Layers.Construction;//LayerMask.GetMask(new string[] { "Construction" });
        private const int PLAYER_LAYER = 131072; //LayerMask.GetMask(new string[] { "Player (Server)" });
        private readonly int defaultColl = LayerMask.GetMask("Default");
        private readonly int groundWorldLayer = LayerMask.GetMask("World", "Default", "Terrain");
        private readonly int groundWorldConstLayer = LayerMask.GetMask("World", "Default", "Terrain", "Construction", "Deployable", "Prevent Building", "Deployed");
        private const int flamerColl = 1084427009;
        private const int c4Coll = 2230528;


        private readonly HashSet<PlayerCorpse> allPlayerCorpses = new();
        private readonly HashSet<BaseNpc> allAnimals = new();
        private readonly HashSet<LootContainer> allContainers = new();
        private bool init = false;
        private readonly System.Random randomSpawn = new();
        private readonly string[] invalidNames = { "battlegroundsrust", "rusteo.com", "zombienation", "knightstable", "changeme", "changeyourname" };
        private DateTime SaveTime { get { return SaveRestore.SaveCreatedTime; } }

        private DateTime LocalSaveTime { get { return SaveRestore.SaveCreatedTime.ToLocalTime(); } }
        private TimeSpan SaveSpan { get { return DateTime.UtcNow - SaveRestore.SaveCreatedTime; } }
        private TimeSpan LocalSaveSpan { get { return DateTime.Now - SaveRestore.SaveCreatedTime.ToLocalTime(); } }

        private float ScaleHours(float value, float offsetMult = 1f)
        {
            if (IsEasyServer()) value *= 0.66f;
            var span = GetWipeDate() - LocalSaveTime;
            return ((span.TotalDays < 12.5) ? (value / 2.5f) : value) * offsetMult;
        }

        private readonly HashSet<DroppedItem> droppedItems = new();

        private readonly List<int> allBlueprints = new();

        private List<string> halucFXs = new();

        private bool IsNight() { return TOD_Sky.Instance.Cycle.Hour > TOD_Sky.Instance.SunsetTime || TOD_Sky.Instance.Cycle.Hour < TOD_Sky.Instance.SunriseTime; }
        private bool IsNight(float offset) { return TOD_Sky.Instance.Cycle.Hour > (TOD_Sky.Instance.SunsetTime + offset) || TOD_Sky.Instance.Cycle.Hour < TOD_Sky.Instance.SunriseTime; }
        private bool IsAM() { return TOD_Sky.Instance.Cycle.Hour <= TOD_Sky.Instance.SunriseTime; }

        public EnvSync EnvSync
        {
            get
            {
                if (_envSync == null)
                {
                    foreach (var entity in BaseNetworkable.serverEntities)
                    {
                        var sync = entity as EnvSync;
                        if (sync != null)
                        {
                            _envSync = sync;
                            break;
                        }
                    }
                }
                return _envSync;
            }
        }

        private EnvSync _envSync = null;


        private bool? _wasEasy;
        private bool IsEasyServer()
        {
            if (_wasEasy.HasValue) return (bool)_wasEasy;
            var val = ConVar.Server.hostname.Contains("#2") || ConVar.Server.hostname.Contains("#3");
            _wasEasy = val;
            return val;
        }
        private bool? _wasHard;
        private bool IsHardServer()
        {
            if (_wasHard.HasValue) return (bool)_wasHard;
            var val = ConVar.Server.hostname.Contains("RED");
            _wasHard = val;
            return val;
        }

        private enum ServerType
        {
            None,
            x3,
            x10,
            BF
        }

        private ServerType serverType = ServerType.None;

        private bool IsStreamerMode(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return false;
            return player?.net?.connection?.info?.GetBool("global.streamermode", false) ?? false;
        }


        public string PluginNamePrefix { get { return Name + ":"; } }

        public readonly Regex basicLatin = new(@"^[\P{L}\p{IsBasicLatin}]+$", RegexOptions.Compiled);

        private HashSet<T> ToHashSet<T>(HashSet<T> hash) { return new HashSet<T>(hash); }

        private HashSet<T> ToHashSet<T>(List<T> list) { return new HashSet<T>(list); }

        private HashSet<T> ToHashSet<T>(IEnumerable<T> enumerable) { return new HashSet<T>(enumerable); }

        private PatrolHelicopter ActiveHeli { get { return PatrolHelicopterAI.heliInstance?.helicopterBase ?? null; } }

        private PatrolHelicopterAI ActiveHeliAI { get { return PatrolHelicopterAI.heliInstance; } }

        private bool IsLooting(BasePlayer player) { return player?.inventory?.loot?.entitySource != null; }

        private readonly Dictionary<ulong, Vector3> cachedRotation = new();

        public static Compilation ins;

        private Vector3 GetPlayerRotation(BasePlayer player) { return player == null ? Vector3.zero : Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward; }

        private DateTime GetWipeDate()
        {
            return WipeInfo?.Call<DateTime>("GetWipeDate") ?? DateTime.MinValue;
        }


        private Quaternion GetPlayerRotationQ(BasePlayer player) { return Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero); }

        private bool IsNullOrEmpty<T>(List<T> List) { return (List?.Count ?? 0) < 1; }

        [PluginReference] private readonly Plugin PlayersByDatabase;

        [PluginReference] private readonly Plugin SteamBansAPI;

        [PluginReference] private readonly Plugin Clans;

        [PluginReference] private readonly Plugin ZoneManager;

        [PluginReference] private readonly Plugin Godmode;

        [PluginReference] private readonly Plugin WipeInfo;

        [PluginReference] private readonly Plugin AAIPDatabase;

        [PluginReference] private readonly Plugin BanSystem;

        [PluginReference] private readonly Plugin SteamStatsAPI;

        [PluginReference]
        private readonly Plugin SkinsAPI;

        [PluginReference]
        private readonly Plugin Deathmatch;

        [PluginReference]
        private readonly Plugin Vanish;

        [PluginReference]
        private readonly Plugin ServerESP;

        [PluginReference]
        private readonly Plugin Known;

        [PluginReference]
        private readonly Plugin LocalStatsAPI;

        [PluginReference]
        private readonly Plugin Friends;

        [PluginReference]
        private readonly Plugin FindGround;

        [PluginReference]
        private readonly Plugin NoEscape;

        [PluginReference]
        private readonly Plugin BetterChat;

        [PluginReference]
        private readonly Plugin NTeleportation;

        [PluginReference]
        private readonly Plugin Vote;

        [PluginReference]
        private readonly Plugin OneCountry;


        [PluginReference]
        private readonly Plugin DiscordAPI2;

        [PluginReference]
        private readonly Plugin Luck;

        [PluginReference]
        private readonly Plugin Playtimes;

        private Dictionary<string, int> newsLevel; //read from data

        public class MovementLog
        {
            [JsonProperty(PropertyName = "t", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            private string _time = string.Empty;
            [JsonProperty(PropertyName = "p", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            private string _pos = string.Empty;
            [JsonIgnore]
            public Vector3 Position
            {
                get { return GetVector3FromString(_pos); }
                set { _pos = value.ToString().Replace("(", "").Replace(")", ""); }
            }

            [JsonIgnore]
            public DateTime Time
            {
                get
                {
                    if (DateTime.TryParse(_time, out DateTime time)) return time;
                    return DateTime.MinValue;
                }
                set { _time = value.ToString(); }
            }
            public MovementLog() { }
            public MovementLog(Vector3 position, DateTime time)
            {
                if (position == Vector3.zero || time == DateTime.MinValue) return;
                Position = position;
                Time = time;
            }
        }

        public class MovementInfo
        {
            [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public ulong UserID = 0;
            [JsonIgnore]
            public string UserIDString
            {
                get { return UserID.ToString(); }
                set
                {
                    if (ulong.TryParse(value, out ulong uid)) UserID = uid;
                }
            }
            [JsonProperty(PropertyName = "l", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public List<MovementLog> Logs = new();
            public MovementInfo() { }
            public MovementInfo(string userID) { if (!string.IsNullOrEmpty(userID)) UserIDString = userID; }
            public MovementInfo(ulong userID) { UserID = userID; }

        }

        private MovementData moveData;

        private class MovementData
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public List<MovementInfo> infos = new();
            public MovementData() { }
        }

        private void AddMovementLog(ulong userID, Vector3 position, DateTime time)
        {
            if (userID == 0 || position == Vector3.zero || time == DateTime.MinValue || moveData?.infos == null)
            {
                PrintWarning("AddMovementLog called with bad args or moveData.infos == null!!!");
                return;
            }
            MovementInfo findInfo = null;
            for (int i = 0; i < moveData.infos.Count; i++)
            {
                var info = moveData.infos[i];
                if (info != null && info.UserID == userID)
                {
                    findInfo = info;
                    break;
                }
            }
            if (findInfo == null)
            {
                findInfo = new MovementInfo(userID);
                if (!moveData.infos.Contains(findInfo)) moveData.infos.Add(findInfo);
            }
            if (findInfo?.Logs == null) findInfo.Logs = new List<MovementLog>();
            var lastPos = findInfo.Logs.Count > 0 ? findInfo?.Logs[findInfo.Logs.Count - 1]?.Position ?? Vector3.zero : Vector3.zero;
            if (lastPos != Vector3.zero && Vector3.Distance(lastPos, position) <= 0.75f) return;
            findInfo.Logs.Add(new MovementLog(position, time));
        }

        private void AddMovementLog(string userID, Vector3 position, DateTime time)
        {
            if (!ulong.TryParse(userID, out ulong uID)) return;
            else AddMovementLog(uID, position, time);
        }

        private List<MovementLog> GetMovementLogs(ulong userID, TimeSpan maxLogAge = default)
        {
            if (moveData?.infos == null || moveData.infos.Count < 1)
            {
                PrintWarning("no move log infos");
                return null;
            }
            var isDefaultAge = maxLogAge == default;
            var now = DateTime.UtcNow;
            PrintWarning("GetMovementLogs called: " + userID + " , maxLogAge: " + maxLogAge);
            for (int i = 0; i < moveData.infos.Count; i++)
            {
                var info = moveData.infos[i];
                if (info?.Logs == null)
                {
                    PrintWarning("info?.Logs == null for index: " + i + "!!");
                    continue;
                }
                if (info.UserID != userID) continue;
                if (isDefaultAge)
                {
                    PrintWarning("no max age specified, returning info.Logs with count: " + info.Logs.Count + " for: " + userID);
                    return info.Logs;
                }
                else
                {
                    var newLogs = new List<MovementLog>();
                    if (info.Logs.Count > 0)
                    {
                        for (int j = 0; j < info.Logs.Count; j++)
                        {
                            var log = info.Logs[j];
                            if (log != null && (log.Time <= DateTime.MinValue || (now - log.Time) <= maxLogAge)) newLogs.Add(log);
                        }
                    }
                    return newLogs;
                }
            }
            return null;
        }

        private class BPInfo
        {
            public ulong UserID = 0;
            public List<int> unlocked = new();

            public bool IsUnlocked(int itemid) => unlocked.Contains(itemid);
            public void Unlock(int itemid) { if (!unlocked.Contains(itemid)) unlocked.Add(itemid); }
            public void Lock(int itemid) { if (unlocked.Contains(itemid)) unlocked.Remove(itemid); }

            public BPInfo() { }
            public BPInfo(ulong uid) { UserID = uid; }
        }

        private class BPData
        {
            public List<BPInfo> blueprints = new();
            public BPInfo GetBPInfo(ulong userID)
            {
                if (blueprints?.Count < 1) return null;
                for (int i = 0; i < blueprints.Count; i++)
                {
                    var bp = blueprints[i];
                    if (bp?.UserID == userID) return bp;
                }
                return null;
            }
            public bool HasBPInfo(ulong userID)
            {
                if (blueprints?.Count < 1) return false;
                for (int i = 0; i < blueprints.Count; i++)
                {
                    var bp = blueprints[i];
                    if (bp?.UserID == userID) return true;
                }
                return false;
            }
            public BPInfo AddBPInfo(ulong userID)
            {
                if (!HasBPInfo(userID))
                {
                    var bpInfo = new BPInfo(userID);
                    blueprints.Add(bpInfo);
                    return bpInfo;
                }
                else return GetBPInfo(userID);
            }
            public BPData() { }
        }



        private class TagData
        {
            public List<TagInfo> tagList = new();
            public TagData() { }
        }

        private class TagInfo
        {
            public string permission = string.Empty;
            public ulong userID = 0;
            public string UserIDString
            {
                get { return userID.ToString(); }
                set
                {
                    if (ulong.TryParse(value, out ulong id)) userID = id;
                }
            }
            public string colorOverride = string.Empty;
            public bool hide = false;
        }

        private TagData tagData;


        private BPData bpData;

        private void SaveTagData() => Interface.Oxide.DataFileSystem.WriteObject("TagData", tagData);

        private void SaveNewsData() => Interface.Oxide.DataFileSystem.WriteObject("NewsLevel", newsLevel);

        private void SaveBPData()
        {
            if (bpData?.blueprints != null && bpData.blueprints.Count > 0) Interface.Oxide.DataFileSystem.WriteObject("BPData", bpData);
        }

        private void SaveRangeData()
        {
            if (turretRange != null && turretRange.Count > 0)
            {
                var toRemove = new HashSet<NetworkableId>();
                foreach (var kvp in turretRange)
                {
                    var foundTurret = false;
                    foreach (var ent in BaseEntity.saveList)
                    {
                        if (ent?.net?.ID == kvp.Key)
                        {
                            foundTurret = true;
                            break;
                        }
                    }
                    if (!foundTurret) toRemove.Add(kvp.Key);
                }
                foreach (var key in toRemove) turretRange.Remove(key);
            }
            Interface.Oxide.DataFileSystem.WriteObject("TurretRange", turretRange);
        }
        private void LoadRangeData()
        {
            turretRange = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<NetworkableId, float>>("TurretRange") ?? new Dictionary<NetworkableId, float>();
            if (turretRange != null && turretRange.Count > 0)
            {
                foreach (var kvp in turretRange)
                {
                    AutoTurret findTurret = null;
                    foreach (var ent in BaseEntity.saveList)
                    {
                        if (ent?.net?.ID == kvp.Key)
                        {
                            findTurret = ent as AutoTurret;
                            break;
                        }
                    }
                    if (findTurret != null && !findTurret.IsDestroyed)
                    {
                        findTurret.sightRange = kvp.Value;
                        findTurret.SendNetworkUpdate();
                    }
                }
            }
        }


        public bool IsPurging { get; set; } = false;
        private System.Collections.IEnumerator PurgeMoveData2()
        {
            if (IsPurging) yield return null;
            var watch = Stopwatch.StartNew();
            Puts("Beginning PurgeMoveData2()");
            IsPurging = true;
            var totalCount = 0;
            var enumCount = 0;
            var enumMax = 20;
            var logMax = 50;
            var utcNow = DateTime.UtcNow;

            for (int i = 0; i < moveData.infos.Count; i++)
            {
                var info = moveData.infos[i];
                if (info == null || info.Logs == null || info.Logs.Count < 1) continue;
                int logCount = 0;
                enumCount++;
                if (enumCount >= enumMax)
                {
                    enumCount = 0;
                    yield return CoroutineEx.waitForEndOfFrame;
                }
                var toRemove = new HashSet<MovementLog>();
                for (int j = 0; j < info.Logs.Count; j++)
                {
                    var log = info.Logs[j];
                    if (log == null) continue;
                    if (logCount >= logMax)
                    {
                        logCount = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    if ((utcNow - log.Time).TotalHours > 4) toRemove.Add(log);
                    logCount++;
                }
                totalCount += toRemove.Count;
                if (toRemove.Count > 0) foreach (var log in toRemove) info.Logs.Remove(log);
            }
            IsPurging = false;
            watch.Stop();

            PrintWarning("Removed " + totalCount.ToString("N0") + " logs older than 4 hours (coroutine) (took: " + watch.Elapsed.TotalMilliseconds + "ms).");
        }

        private string PlayerListString = string.Empty;

        private Admin.PlayerInfo[] GetPlayerList()
        {
            var count = BasePlayer.activePlayerList.Count;
            if (count < 1) return null;

            var array = new Admin.PlayerInfo[count];
            for (int i = 0; i < count; i++)
            {
                var x = BasePlayer.activePlayerList[i];
                array[i] = new Admin.PlayerInfo
                {
                    SteamID = x.UserIDString,
                    OwnerSteamID = x.OwnerID.ToString(),
                    DisplayName = x.displayName,
                    Ping = (x?.net?.connection == null) ? -1 : Net.sv.GetAveragePing(x.net.connection),
                    Address = (x?.net?.connection == null) ? string.Empty : x.net.connection.ipaddress,
                    ConnectedSeconds = (x?.net?.connection == null) ? -1 : (int)x.net.connection.GetSecondsConnected(),
                    VoiationLevel = x.violationLevel,
                    Health = x.Health()
                };
            }
            return array;
        }

        private System.Collections.IEnumerator LoadAllHalucFX()
        {
            yield return CoroutineEx.waitForEndOfFrame;
            var strs = GameManifest.Current.pooledStrings;
            halucFXs = new List<string>();
            var cur = 0;
            var max = 75;
            for (int i = 0; i < strs.Length; i++)
            {
                var str = strs[i];
                if (cur >= max)
                {
                    cur = 0;
                    yield return CoroutineEx.waitForEndOfFrame;
                }
                if (string.IsNullOrEmpty(str.str)) continue;
                if (str.str.Contains("assets/bundled/prefabs/fx/impacts/") || str.str.EndsWith("attack.prefab") || str.str.EndsWith("attack_muzzlebrake") || str.str.EndsWith("attack_silenced")) halucFXs.Add(str.str);
                cur++;
            }
            //    halucFXs = GameManifest.Current.pooledStrings?.Select(p => p.str)?.Where(p => p.Contains("assets/bundled/prefabs/fx/impacts/") || p.EndsWith("attack.prefab") || p.EndsWith("attack_muzzlebrake") || p.EndsWith("attack_silenced"))?.ToList() ?? new List<string>();
            halucFXs.Add("assets/bundled/prefabs/fx/build/promote_toptier.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/collect/collect stone.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/collect/collect stump.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/collect/collect plant.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/impacts/additive/explosion.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/explosions/water_bomb.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/entities/loot_barrel/impact.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/entities/loot_barrel/gib.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/impacts/stab/rock/stab_rock_01.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/player/howl.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/player/gutshot_scream.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/weapons/landmine/landmine_trigger.prefab");
            yield return CoroutineEx.waitForSeconds(0.05f);
            halucFXs.Add("assets/prefabs/locks/keypad/effects/lock.code.denied.prefab");
            halucFXs.Add("assets/prefabs/npc/autoturret/effects/targetacquired.prefab");
            halucFXs.Add("assets/prefabs/npc/autoturret/effects/targetlost.prefab");
            halucFXs.Add("assets/prefabs/npc/m2bradley/effects/coaxmgmuzzle.prefab");
            halucFXs.Add("assets/prefabs/npc/murderer/sound/breathing.prefab");
            halucFXs.Add("assets/prefabs/npc/murderer/sound/death.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/oiljack/pump_up.prefab");
            halucFXs.Add("assets/bundled/prefabs/fx/oiljack/pump_down.prefab");
            halucFXs.Add("assets/prefabs/tools/c4/effects/c4_stick.prefab");
        }

        //emitted by SteamAuthOverride *after* the connection was approved, oxide hooks don't provide that
        private void OnConnectionApproved(Connection connection)
        {
            if (connection == null) return;
            timer.Once(0.5f, () =>
            {
                if (connection == null) return;

                var netWrite = Net.sv.StartWrite();

                netWrite.PacketID(Message.Type.Message);
                netWrite.String("<size=52><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color></size>");
                netWrite.String(string.Empty);
                netWrite.Send(new SendInfo(connection));
            });
        }

        private void Init()
        {

            var watch = Stopwatch.StartNew();
            ins = this;
            Unsubscribe(nameof(OnEntityDeath));
            Unsubscribe(nameof(OnEntityTakeDamage));
            Unsubscribe(nameof(OnPlayerInput));
            //  Subscribe(nameof(CanCustomNetworkTo));
            var watchData = Stopwatch.StartNew();

            // if ((suspectData = Interface.Oxide.DataFileSystem.ReadObject<SuspectData>("SuspectData")) == null) suspectData = new SuspectData();
            if ((tagData = Interface.Oxide.DataFileSystem.ReadObject<TagData>("TagData")) == null) tagData = new TagData();

            if ((bpData = Interface.Oxide.DataFileSystem.ReadObject<BPData>("BPData")) == null) bpData = new BPData();
            newsLevel = Interface.Oxide?.DataFileSystem?.ReadObject<Dictionary<string, int>>("NewsLevel") ?? new Dictionary<string, int>();

            moveData = new MovementData();

            watchData.Stop();


            jsonsettings = new JsonSerializerSettings();
            jsonsettings.Converters.Add(new KeyValuesConverter());

            var perms = new List<string>(15) { "warn", "say", "tp", "mcpick", "steamid", "alias", "clearchat", "getup", "blind", "pinfo", "heal", "bug", "sapi", "teleportany", "nojoinmsg" };
            for (int i = 0; i < perms.Count; i++)
            {
                var permName = Name + "." + perms[i];
                if (!permission.PermissionExists(permName, this)) permission.RegisterPermission(permName, this);
            }

            var watchCmd = Stopwatch.StartNew();

            string[] donateCommands = { "donate", "vipd", "mvp", "donating" };

            string[] playersByDatabaseDebugCmds = { "pbd" };

            string[] addVmCmds = { "addvm", "vmadd", "vmorder", "addorder" };

            AddCovalenceCommand(addVmCmds, nameof(cmdAddVendingMachineSellOrder));

            AddCovalenceCommand(playersByDatabaseDebugCmds, nameof(cmdPbdDebug));


            AddCovalenceCommand("loot.delete", nameof(cmdKillAllLoot));


            AddCovalenceCommand("movelog", "cmdMoveLog");

            AddCovalenceCommand(donateCommands, "cmdDonate");
            AddCovalenceCommand("joinmsg", "cmdJoinMsg");

            string[] playerCmds = { "players", "online", "who", "pop", "population", "player" };
            string[] ipCmds = { "10x", "2x", "0.5x", "server2", "server1", "3x", "#2", "#1", "ip", "ips", "server", "servers", "prism", "1x", "red", "blue", "arpg" };
            AddCovalenceCommand(ipCmds, "cmdServers");
            AddCovalenceCommand(playerCmds, nameof(cmdPlayers));
            AddCovalenceCommand("popupall", "cmdPopup");
            AddCovalenceCommand("sapi", "cmdInfo");
            AddCovalenceCommand("pinfo", nameof(cmdPInfo));
            AddCovalenceCommand("steamid", "cmdSteamID");
            AddCovalenceCommand("gameban", "cmdGameBan");
            AddCovalenceCommand("fc", "cmdForceCommand");
            AddCovalenceCommand("teleportany", "cmdTeleportAny");
            string[] adminHelpCmds = { "hack", "hacks", "hacker", "cheats", "cheat", "cheaters", "hax", "admin", "admins", "mods", "reporth", "reporthelp", "adm", "mod", "moderator", "administrator" };
            string[] contactCmds = { "contact", "ts3", "disc", "discord", "discordlink", "tslink", "teamspeak", "client", "download" };
            string[] shopCmds = { "vms", "vm", "vending", "battery", "batteries", "sell", "vendingmachine", "vendingmachines" };
            string[] townCmds = { "town", "towns", "outpost", "bandit", "banditcamp" };
            AddCovalenceCommand(adminHelpCmds, "cmdAdminHelp");
            AddCovalenceCommand(contactCmds, nameof(cmdContactHelp));
            AddCovalenceCommand("playerlist", "cmdPlayerList");
            string[] helpCmds = { "help", "info", "infos", "information", "cmds", "commands", "how", "gelp", "helps", "hlep", "hepl" };
            AddCovalenceCommand(helpCmds, nameof(cmdHelp2));
            AddCovalenceCommand(shopCmds, nameof(cmdShop));
            AddCovalenceCommand("shock", nameof(cmdSlapPlayer));
            string[] tagCmds = { "tagc", "ctag", "tegc", "cteg", "tagcol", "coltag", "tagcolor", "tagcolors", "tagcolour", "tagcolours", "colortag", "namecolor", "colorname", "titlecolor" };
            AddCovalenceCommand(tagCmds, nameof(cmdTag));
            timer.Once(1f, () => AddCovalenceCommand(townCmds, nameof(cmdTown)));

            AddCovalenceCommand("setoldwipe", "cmdSetOldWipe");
            AddCovalenceCommand("dumpe", "cmdEffectDump");
            AddCovalenceCommand("frametest", "cmdFrameTest");
            AddCovalenceCommand("gametags", "cmdGameTags");
            AddCovalenceCommand("connections", "cmdConnections");
            AddCovalenceCommand("idlers", "cmdIdlePlayers");

            AddCovalenceCommand(new string[] { "mymini", "Minicopter", "Minicopters", "myheli", "minimy" }, nameof(cmdMyMini));

            watchCmd.Stop();
            watch.Stop();
            PrintWarning("Init() took: " + watch.Elapsed.TotalMilliseconds + "ms, watchData: " + watchData.Elapsed.TotalMilliseconds + "ms, watchCmd: " + watchCmd.Elapsed.TotalMilliseconds + "ms");
        }

        private void cmdForceCommand(IPlayer player, string command, string[] args)
        {
            if (player == null) return;
            if (!player.IsAdmin) return;
            if (args.Length < 2)
            {
                SendReply(player, "You must supply a target and command!");
                return;
            }
            var target = FindConnectedPlayer(args[0]);
            if (target == null)
            {
                SendReply(player, "No target found by name: " + args[0]);
                return;
            }
            var consCmd = !args[1].StartsWith("/", StringComparison.OrdinalIgnoreCase);
            var cmdStr = (!consCmd ? "chat.say " + "\"" + args[1] : args[1]) + ((args.Length > 2) ? " " : "");
            var cmdSB = Pool.Get<StringBuilder>();
            try
            {
                cmdSB.Clear().Append(cmdStr);

                if (args.Length > 2)
                {
                    for (int i = 2; i < args.Length; i++)
                    {
                        var arg = args[i];
                        cmdSB.Append(arg).Append(" ");
                    }
                    cmdSB.Length -= 1;
                }

                var cmd = cmdSB.Append(!consCmd ? "\"" : "").ToString();
               
                target.Command(cmd);

                PrintWarning("CMD: " + cmd);
            }
            finally { Pool.Free(ref cmdSB); }



        }


        private const string NEWS_URL = @"https://prismrust.com/api/discord/server-updates?simple=true";
        private const string grulesURL = @"https://prismrust.com/api/rust/grules?simple=true";
        private Timer _newsTimer = null;
        public Timer NewsTimer
        {
            get { return _newsTimer; }
            set
            {
                _newsTimer?.Destroy();
                _newsTimer = value;
            }
        }
        private DateTime lastNews = DateTime.MinValue;
        private Timer pingCheck = null;
        private Timer signTimer = null;
        private int bingFailTimes = 0;


        private MonumentInfo _largeOilRig = null;
        public MonumentInfo LargeOilRig
        {
            get
            {
                if (_largeOilRig == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if (mon.displayPhrase.english.Contains("large oil rig", CompareOptions.OrdinalIgnoreCase))
                        {
                            _largeOilRig = mon;
                            break;
                        }
                    }
                }

                if (_largeOilRig == null)
                    PrintError(nameof(_largeOilRig) + " could not be found!!!");

                return _largeOilRig;
            }
        }

        private MonumentInfo _oilRig = null;
        public MonumentInfo SmallOilRig
        {
            get
            {
                if (_oilRig == null)
                {
                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                    {
                        var mon = TerrainMeta.Path.Monuments[i];
                        if (mon != LargeOilRig && mon.displayPhrase.english.Contains("oil rig", CompareOptions.OrdinalIgnoreCase))
                        {
                            _oilRig = mon;
                            break;
                        }
                    }
                }

                if (_oilRig == null)
                    PrintError(nameof(_oilRig) + " could not be found!!!");

                return _oilRig;
            }
        }

        public bool IsMoveLogging { get; set; } = false;

        private System.Collections.IEnumerator MoveLogAllPlayers()
        {
            if (BasePlayer.activePlayerList != null && BasePlayer.activePlayerList.Count > 0 && !IsMoveLogging)
            {
                IsMoveLogging = true;
                try
                {
                    var count = 0;
                    var max = (int)Math.Round(BasePlayer.activePlayerList.Count * 0.25f, MidpointRounding.AwayFromZero);

                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var player = BasePlayer.activePlayerList[i];
                        if (player == null || player.gameObject == null || player.IsDestroyed || player?.transform == null || !player.IsConnected || player.IsSleeping() || player.IsDead() || player.IdleTime >= 20) continue;
                        if (count >= max)
                        {
                            count = 0;
                            yield return CoroutineEx.waitForEndOfFrame;
                        }
                        var pos = player?.transform?.position ?? Vector3.zero;
                        if (pos == Vector3.zero) continue;
                        var newPos = new Vector3(pos.x, pos.y + 0.15f, pos.z);
                        AddMovementLog(player.userID, newPos, DateTime.UtcNow);
                        count++;
                    }
                }
                finally { IsMoveLogging = false; }
            }
        }

        private string GetDescriptionFromData()
        {
            var str = string.Empty;
            try { str = (Interface.Oxide?.DataFileSystem?.ReadObject<object>("Description") ?? null)?.ToString() ?? string.Empty; }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + " ^ GetDescriptionFromData() ^ "); }
            return str;
        }

        private readonly bool oldUpkeep = ConVar.Decay.upkeep;
        private readonly float oldScale = ConVar.Decay.scale;

        private int _initFpsLimit = -1;
        private void OnServerInitialized()
        {
            try
            {
                var watch = Stopwatch.StartNew();
                if (ConVar.Server.hostname.Contains("#2")) serverType = ServerType.x10;
                else if (ConVar.Server.hostname.Contains("#3")) serverType = ServerType.BF;
                else serverType = ServerType.x3;


                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item?.Blueprint == null || !item.Blueprint.userCraftable || !item.Blueprint.isResearchable) continue;
                    allBlueprints.Add(item.itemid);
                }


                NewsTimer = timer.Every(1500f, () =>
                {
                    if (lastNews != DateTime.MinValue && (DateTime.Now - lastNews).TotalSeconds < 1490)
                    {
                        PrintWarning("NewsTimer ticked too quickly!!");
                        return;
                    }
                    BroadcastNews();
                    lastNews = DateTime.Now;
                });

                timer.Once(UnityEngine.Random.Range(25f, 45f), () =>
                {
                    Action pingAct = null;
                    pingAct = new Action(() =>
                    {
                        if (pingCheck != null)
                        {
                            pingCheck.Destroy();
                            pingCheck = null;
                        }
                        var hasAnyConnectedPlayers = false;
                        if (BasePlayer.activePlayerList != null && BasePlayer.activePlayerList.Count > 0)
                        {
                            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                            {
                                var p = BasePlayer.activePlayerList[i];
                                if (p != null && p.gameObject != null && !p.IsDestroyed && p.IsConnected)
                                {
                                    hasAnyConnectedPlayers = true;
                                    break;
                                }
                            }
                        }
                        if (hasAnyConnectedPlayers) return;
                        webrequest.Enqueue("https://www.bing.com", null, (code, response) =>
                        {
                            if (code != 200)
                            {
                                PrintWarning("Code != 200: " + code);
                                bingFailTimes++;
                                if (bingFailTimes > 3)
                                {
                                    PrintWarning("bing fail times > 3! We're going to disable AI and Decay. Old decay scale: " + ConVar.Decay.scale);
                                    AI.move = false;
                                    AI.think = false;
                                    ConVar.Decay.upkeep = false;
                                    ConVar.Decay.scale = 0f;
                                }
                            }
                            else
                            {
                                bingFailTimes = 0;
                                // ConVar.Decay.upkeep = oldUpkeep;
                                //   ConVar.Decay.scale = oldScale;
                            }
                        }, this, Core.Libraries.RequestMethod.GET, null, 25f);
                        pingCheck = timer.Once(UnityEngine.Random.Range(45, 90), pingAct);
                    });
                    pingAct.Invoke();
                });



                FillItemList();

                foreach (var entity in BaseNetworkable.serverEntities)
                {
                    if (entity == null) continue;

                    var dropItem = entity as DroppedItem;
                    var corpse = entity as PlayerCorpse;
                    var loot = entity as LootContainer;
                    var storage = entity as StorageContainer;

                    if (dropItem != null) droppedItems.Add(dropItem);
                    if (corpse != null) allPlayerCorpses.Add(corpse);
                    if (loot != null) allContainers.Add(loot);
                    if (storage != null) storageContainers.Add(loot);
                }

                Subscribe(nameof(OnEntityTakeDamage));
                Subscribe(nameof(OnEntityDeath));
                Subscribe(nameof(OnPlayerInput));


                oldHostName = ConVar.Server.hostname;

                //eu format
                //partly handled in gametagsharmony
                ConVar.Server.hostname = newHostName = SaveSpan.TotalDays < 2 ? "JUST WIPED " + oldHostName : LocalSaveTime.Day + "/" + LocalSaveTime.Month + " " + oldHostName;

                oldDescription = ConVar.Server.description;

                var readDescription = GetDescriptionFromData();
                if (!string.IsNullOrWhiteSpace(readDescription)) ConVar.Server.description = readDescription;
                else PrintWarning(nameof(readDescription) + " is null — the server will have no description or it will use the one from the ConVar!!");



                var playerListAct = new Action(() =>
                {
                    PlayerListString = JsonConvert.SerializeObject(GetPlayerList(), Formatting.Indented);
                });
                playerListAct.Invoke();


                timer.Every(40f, () =>
                {
                    try { playerListAct.Invoke(); }
                    catch (Exception ex) { PrintError("Failed to invoke playerListAct: " + ex); }

                });


                /*/ ryzen prep - we don't want this anymore
                _initFpsLimit = FPS.limit;
                timer.Every(3f, () =>
                {
                   // if (ServerMgr.Instance != null) ServerMgr.Instance.StartCoroutine(MoveLogAllPlayers());

                    try
                    {
                        if (BasePlayer.activePlayerList?.Count < 1 && !ServerMgr.Instance.Restarting && ServerMgr.Instance.connectionQueue.joining.Count < 1) FPS.limit = 8;
                        else
                        {
                            if (FPS.limit == 8)
                            {
                                PrintWarning("setting fps limit back to initFpsLimit: " + _initFpsLimit);
                                FPS.limit = _initFpsLimit;
                            }
                        }
                    }
                    catch (Exception ex) { PrintError("Failed to set FPS limits: " + ex); }
                });/*/


                timer.Every(240f, () =>
                {
                    foreach (var ent in BaseEntity.saveList)
                    {
                        var stash = ent as StashContainer;
                        if (stash != null && !HasAnyLooters(stash)) stash.SetHidden(true);
                    }
                });


                LoadRangeData();
                ServerMgr.Instance.StartCoroutine(LoadAllHalucFX());




             

                watch.Stop();
                PrintWarning("Finished init for server type " + serverType + " (easy: " + IsEasyServer() + ", hard: " + IsHardServer() + ") Took: " + watch.Elapsed.TotalMilliseconds + "ms, to complete server init code");
            }
            finally { init = true; }
        }



        private void PuzzleResetAll()
        {
            var resets = UnityEngine.Object.FindObjectsOfType<PuzzleReset>();
            if (resets == null || resets.Length < 1) return;
            for (int i = 0; i < resets.Length; i++)
            {
                var reset = resets[i];
                if (reset == null) continue;
                reset.DoReset();
                reset.ResetTimer();
            }
        }

        private void ResetPuzzles()
        {
            var timerWatch = Stopwatch.StartNew();

            try { PuzzleResetAll(); }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^ puzzle resets ^"); }

            try { covalence.Server.Command("debug.puzzleprefabrespawn"); }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^ debug.puzzleprefabrespawn ^"); }

            try { PuzzleResetAll(); }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^ puzzle resets2 ^"); }

            timerWatch.Stop();
            PrintWarning("ResetPuzzles() took: " + timerWatch.Elapsed.TotalMilliseconds + "ms");
        }

        private Timer trackTimer = null;
        private string oldHostName = string.Empty;
        private string newHostName = string.Empty;
        private string oldDescription = string.Empty;
        private int _bornTime = 0;
        public int BornTime { get { return (_bornTime == 0) ? (_bornTime = Facepunch.Math.Epoch.FromDateTime(SaveRestore.SaveCreatedTime)) : _bornTime; } }

        private T Deserialise<T>(string json) => JsonConvert.DeserializeObject<T>(json);
        private class Games
        {
            [JsonProperty("response")]
            public Content Response;

            public class Content
            {
                [JsonProperty("game_count")]
                public int GameCount;
                [JsonProperty("games")]
                public Game[] Games;

                public class Game
                {
                    [JsonProperty("appid")]
                    public uint AppId;
                    [JsonProperty("playtime_2weeks")]
                    public int PlaytimeTwoWeeks;
                    [JsonProperty("playtime_forever")]
                    public int PlaytimeForever;
                }
            }
        }

        private void Unload()
        {
            try
            {
                var watch = Stopwatch.StartNew();

                try
                {

                    foreach (var routine in _coroutines)
                    {
                        if (routine == null)
                            continue;

                        ServerMgr.Instance.StopCoroutine(routine);
                    }

                    _coroutines?.Clear();

                }
                catch (Exception ex) { PrintError(ex.ToString()); }

                if (FPS.limit == 8 && _initFpsLimit != -1) FPS.limit = _initFpsLimit;

                if (_decaySbBuffer?.Length > 0) LogToFile("decay_log", _decaySbBuffer.ToString(), this);

                ConVar.Server.hostname = oldHostName;
                ConVar.Server.description = oldDescription;

                NewsTimer = null;

                if (trackTimer != null)
                {
                    trackTimer.Destroy();
                    trackTimer = null;
                }
                if (signTimer != null)
                {
                    signTimer.Destroy();
                    signTimer = null;
                }
                if (pingCheck != null)
                {
                    pingCheck.Destroy();
                    pingCheck = null;
                }

                foreach (var marker in dropMarkers)
                {
                    if (marker != null && !marker.IsDestroyed) marker.Kill();
                }

                foreach (var entity in BaseNetworkable.serverEntities)
                {
                    if (entity == null || entity.IsDestroyed || entity?.gameObject == null) continue;
                    var trigger = entity?.GetComponent<SphereTrigger>() ?? entity?.GetComponentInChildren<SphereTrigger>() ?? entity?.GetComponentInParent<SphereTrigger>() ?? null;
                    trigger?.DoDestroy();
                }

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || player?.gameObject == null) continue;

                    if (player.IsConnected) 
                        CuiHelper.DestroyUi(player, "SpriteUI");
                }

                var errorSB = new StringBuilder("ErrorSB start");
                var exSB = new StringBuilder();
                /*/
                 *    try { if (!(kvp.Key.action?.Target?.ToString() ?? string.Empty).Contains("Oxide")) continue; }
                        catch (Exception ex) { exSB.AppendLine(ex.ToString()); }
                 * /*/
                try
                {
                    var invList = InvokeList;
                    var objStr = this?.Object?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(objStr)) PrintWarning("objStr is null/empty!!!");
                    //   var invokeEnum = (invList == null || invList.Count < 1 || string.IsNullOrEmpty(objStr)) ? null : invList?.Where(p => p.Key != null && (p.Key.action?.Target?.ToString() ?? string.Empty).Contains(objStr)) ?? null;
                    var newEnums = new ListDictionary<InvokeAction, float>();
                    foreach (var inv in invList)
                    {
                        try { if (inv.Key == null || inv.Key.action == null || inv.Key.action.Target == null) continue; }
                        catch (Exception ex)
                        {
                            exSB.AppendLine(ex.ToString());
                            continue;
                        }

                        var strTarg = string.Empty;
                        //  var obj = inv.Key.action?.Target as BaseNetworkable;
                        /*/
                        if (obj == null || obj.gameObject == null || obj.IsDestroyed)
                        {
                            //exSB.AppendLine("Obj as BaseNetworkable is null, destroyed or gameObject is null!");
                            continue;
                        }/*/
                        try
                        {
                            var net = inv.Key.action?.Target as BaseNetworkable;
                            if (net != null && (net.gameObject == null || net.IsDestroyed))
                                continue;

                            strTarg = inv.Key.action?.Target?.ToString() ?? string.Empty;
                        }
                        catch (Exception ex)
                        {
                            exSB.AppendLine(ex.ToString());
                            continue;
                        }

                        if (!string.IsNullOrEmpty(strTarg) && strTarg.Contains(objStr)) newEnums.Add(inv.Key, inv.Value);
                    }
                    if (newEnums != null && newEnums.Count > 0)
                    {
                        // PrintWarning("Got newEnums: " + newEnums.Count);
                        foreach (var inv in newEnums) InvokeHandler.CancelInvoke(inv.Key.sender, inv.Key.action);
                    }
                    else PrintWarning("no newEnums!!!");

                }
                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Error on Unload() for invokes"); }
                try
                {
                    errorSB.AppendLine("invokes finished, start repeat");
                    if ((repeatingInvokes?.Count ?? 0) > 0)
                    {
                        PrintWarning("Found repeating invokes: " + repeatingInvokes.Count);
                        foreach (var repeat in repeatingInvokes)
                        {
                            if (repeat == null || repeat.Method == null || repeat.Target == null) continue;
                            CancelInvoke(repeat.Method.Name, repeat.Target);
                        }
                    }
                    errorSB.AppendLine("repeating finished");
                }
                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Error on Unload() for invokes (repeating)"); }
                if (exSB.Length > 0) PrintError("Exception SB: " + Environment.NewLine + exSB.ToString().TrimEnd());



                SaveAllData();

                watch.Stop();
                PrintWarning("Unload took: " + watch.Elapsed.TotalMilliseconds + "ms");
            }
            finally { ins = null; }


        }

        private void SaveAllData()
        {
            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                SaveTagData();

                SaveBPData();

                SaveRangeData();
                SaveNewsData();

                if (watch.Elapsed.TotalMilliseconds >= 10)
                {
                    var sb = Pool.Get<StringBuilder>();
                    try { PrintWarning(sb.Clear().Append("SaveAllData() took: ").Append(watch.ElapsedMilliseconds.ToString("0.00")).Replace(".00", string.Empty).Append("ms").ToString()); }
                    finally { Pool.Free(ref sb); }
                }
            }
            finally { Pool.Free(ref watch); }

        }

        private void OnServerSave()
        {
            ServerMgr.Instance.Invoke(() =>
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || p.IsDestroyed || p.gameObject == null || p.IsDead()) continue;
                    DoVeteran(p);
                }
            }, 10f);
        }

        private bool EntityHasZoneFlag(BaseEntity entity, string flag) { return ZoneManager?.Call<bool>("EntityHasFlag", entity, flag) ?? false; }

        private bool IsPlayerInflictedType(Rust.DamageType dmgType) { return dmgType == Rust.DamageType.Arrow || dmgType == Rust.DamageType.Bleeding || dmgType == Rust.DamageType.Blunt || dmgType == Rust.DamageType.Bullet || dmgType == Rust.DamageType.Explosion || dmgType == Rust.DamageType.Heat || dmgType == Rust.DamageType.Slash || dmgType == Rust.DamageType.Stab; }

        private void OnCodeEntered(CodeLock codeLock, BasePlayer player, string code)
        {
            if (codeLock == null || player == null || string.IsNullOrEmpty(code)) return;
            var hasCode = code.Equals(codeLock.code, StringComparison.OrdinalIgnoreCase);
            var str = player.displayName + " used code: " + code + ", " + getVectorString(player?.transform?.position ?? Vector3.zero) + " (" + (hasCode ? "CORRECT" : "INCORRECT") + ")";
            LogToFile("code_log", str, this, true);
        }

        private readonly Dictionary<string, TimeCachedValue<int>> _globalBuildLimitCache = new();

        private void OnGetGlobalBuildLimit(BasePlayer player, string clanTag, ref int limit)
        {
            if (player == null) return;

            if (!IsVIP(player.UserIDString))
                return;

            var l = limit;

            if (!_globalBuildLimitCache.TryGetValue(player.UserIDString, out _))
                _globalBuildLimitCache[player.UserIDString] = new TimeCachedValue<int>()
                {
                    refreshCooldown = 300f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<int>(() =>
                    {
                        return (int)Math.Round(l * 1.5, MidpointRounding.AwayFromZero);
                    })
                };

            limit = _globalBuildLimitCache[player.UserIDString].Get(false);
        }

        private readonly Dictionary<string, Dictionary<uint, TimeCachedValue<int>>> _buildLimitCache = new();


        private void OnGetBuildLimit(BasePlayer player, BuildingBlock block, ref int limit)
        {
            if (player == null) return;

            if (limit <= 0)
            {
                PrintWarning(nameof(OnGetBuildLimit) + " has limit <= 0!!: " + limit);
                return;
            }

            if (!IsVIP(player.UserIDString))
                return;



            var l = limit;

            if (!_buildLimitCache.TryGetValue(player.UserIDString, out _))
                _buildLimitCache[player.UserIDString] = new();

            if (!_buildLimitCache[player.UserIDString].TryGetValue(block.buildingID, out _))
                _buildLimitCache[player.UserIDString][block.buildingID] = new TimeCachedValue<int>()
                {
                    refreshCooldown = 300f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<int>(() =>
                    {
                        return l *= 2;
                    })
                };

            limit = _buildLimitCache[player.UserIDString][block.buildingID].Get(false);
        }

        private void OnPlayerDeath(BasePlayer victim, HitInfo info)
        {
            if (victim == null) return;
            var atk = info?.Initiator ?? null;
            var attacker = atk as BasePlayer;
            var respawnPos = Vector3.zero;
            if (forcedSpawnPos.TryGetValue(victim.userID, out respawnPos))
            {
                timer.Once(1f, () =>
                {
                    if (victim == null) return;
                    victim?.RespawnAt(respawnPos, Quaternion.identity);
                });
            }
            var attackerID = attacker?.userID ?? 0;
            var victimID = victim?.userID ?? 0;
            NextTick(() =>
            {
                var errorTestSB = new StringBuilder();
                try
                {
                    var totaldmg = info?.damageTypes?.Total() ?? 0f;
                    var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
                    if (!IsPlayerInflictedType(dmgType)) return;
                    if (victim == null && attacker == null) return;
                    if (victim != null && attacker != null && (attacker == victim)) return;
                    if (attacker != null && attackerID == 0 || victim != null && victimID == 0) return;


                    if (victim != null)
                    {
                        for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                        {
                            var player = BasePlayer.activePlayerList[i];
                            if (player == null || player.IsConnected) continue;
                            if (hits.ContainsKey(player.userID) && hits[player.userID].ContainsKey(victim.userID)) hits[player.userID].Remove(victim.userID);
                            if (damages.ContainsKey(player.userID) && damages[player.userID].ContainsKey(victim.userID)) damages[player.userID].Remove(victim.userID);
                            if (weaponsUsed.ContainsKey(player.userID) && weaponsUsed[player.userID].ContainsKey(victim.userID)) weaponsUsed[player.userID].Remove(victim.userID);
                            if (bodyPartsHit.ContainsKey(player.userID) && bodyPartsHit[player.userID].ContainsKey(victim.userID)) bodyPartsHit[player.userID].Remove(victim.userID);
                        }
                    }
                    if (attacker != null && victim != null && !(attacker == victim))
                    {
                        errorTestSB.AppendLine("attacker != null && vic != null, attacker name: " + (attacker?.displayName ?? "Unknown") + ", victim: " + (victim?.displayName ?? "Unknown"));
                        if (!damages.ContainsKey(attacker.userID)) damages[attacker.userID] = new Dictionary<ulong, float>();
                        if (!damages[attacker.userID].ContainsKey(victim.userID)) damages[attacker.userID][victim.userID] = totaldmg;
                        errorTestSB.AppendLine("past damages contains");
                        if (!hits.ContainsKey(attacker.userID)) hits[attacker.userID] = new Dictionary<ulong, int>();
                        if (!hits[attacker.userID].ContainsKey(victim.userID)) hits[attacker.userID][victim.userID] = 1;
                        errorTestSB.AppendLine("past hits contains");
                        if (hits.ContainsKey(attacker.userID))
                        {
                            errorTestSB.AppendLine("contains key hits attacker userid");
                            if (!hits[attacker.userID].TryGetValue(victim.userID, out int hitAmounts)) PrintWarning("Failed to get hit amounts on attacker to vic");
                            errorTestSB.AppendLine("got hitAmounts");
                            var weapons = new List<string>();
                            var damageAmount = 0f;
                            var bodyParts = new List<string>();
                            if (damages.ContainsKey(attacker.userID)) damages[attacker.userID].TryGetValue(victim.userID, out damageAmount);
                            errorTestSB.AppendLine("outted damageAmount for attacker");
                            if (weaponsUsed.ContainsKey(attacker.userID)) weaponsUsed[attacker.userID].TryGetValue(victim.userID, out weapons);
                            errorTestSB.AppendLine("outted weaponsUsed for attacker");
                            if (bodyPartsHit.ContainsKey(attacker.userID)) bodyPartsHit[attacker.userID].TryGetValue(victim.userID, out bodyParts);
                            errorTestSB.AppendLine("outted bodypartshit for attacker");
                            var weaponString = string.Empty;
                            var bodyPartString = string.Empty;
                            var bodyPartSB = new StringBuilder();
                            var weaponSB = new StringBuilder();
                            if (!IsNullOrEmpty(weapons)) for (int i = 0; i < weapons.Count; i++) weaponSB.Append(weapons[i]).Append(", ");
                            if (!IsNullOrEmpty(bodyParts)) for (int i = 0; i < bodyParts.Count; i++) bodyPartSB.Append(bodyParts[i]).Append(", ");
                            if (weaponSB.Length > 3) weaponSB.Length -= 2;
                            if (bodyPartSB.Length > 3) weaponSB.Length -= 2;
                            weaponString = weaponSB.ToString();
                            bodyPartString = bodyPartSB.ToString();
                            //  PrintWarning("do send");
                            var atkMsg = "<color=#ff912b>" + damageAmount.ToString("N0") + "</color> damage dealt to " + "<color=#4d4dff>" + victim.displayName + "</color> in <color=#ff912b>" + hitAmounts + "</color> hits, hitting: " + bodyPartString + ", using weapon: <color=#cc3300>" + weaponString + "</color>";
                            attacker.SendConsoleCommand("echo " + atkMsg);
                            var vicMsg = "<color=#4d4dff>" + attacker.displayName + "</color> dealt <color=#ff912b>" + damageAmount.ToString("N0") + "</color> damage in <color=#ff912b>" + hitAmounts + "</color> hits, hitting: " + bodyPartString + ", using weapon: <color=#cc3300>" + weaponString + "</color>";
                            //  PrintWarning("Messages for (attacker != null && victim != null && attacker != victim) and hits contains attacker.userID: atkMsg: " + RemoveTags(atkMsg) + ", vicMsg: " + RemoveTags(vicMsg));
                            victim.SendConsoleCommand("echo " + vicMsg);
                            //    PrintWarning("did send: " + atkMsg + Environment.NewLine + vicMsg);
                            if (hits.ContainsKey(attacker.userID)) hits.Remove(attacker.userID);
                            if (damages.ContainsKey(attacker.userID)) damages.Remove(attacker.userID);
                            if (weaponsUsed.ContainsKey(attacker.userID)) weaponsUsed.Remove(attacker.userID);
                            if (bodyPartsHit.ContainsKey(attacker.userID)) bodyPartsHit.Remove(attacker.userID);
                        }
                        if (hits.ContainsKey(victim.userID))
                        {
                            errorTestSB.AppendLine("contains key vic userID for hits");
                            if (hits[victim.userID].TryGetValue(attacker.userID, out int vicHits))
                            {
                                //                 var vicHits = hits[victim.userID][attacker.userID];
                                errorTestSB.AppendLine("got vicHits");
                                var vicWeapons = new List<string>();
                                var bodyParts = new List<string>();
                                var vicDamage = 0f;
                                if (damages.ContainsKey(victim.userID)) damages[victim.userID].TryGetValue(attacker.userID, out vicDamage);
                                errorTestSB.AppendLine("outted vicDamage for vic");
                                if (weaponsUsed.ContainsKey(victim.userID)) weaponsUsed[victim.userID].TryGetValue(attacker.userID, out vicWeapons); //vicWeapons.Add("Unknown weapon(s)");
                                errorTestSB.AppendLine("outted vicWeaponsUsed");
                                if (bodyPartsHit.ContainsKey(victim.userID)) bodyPartsHit[victim.userID].TryGetValue(attacker.userID, out bodyParts); //bodyParts.Add("Unknown bodypart(s)");
                                errorTestSB.AppendLine("outted bodyparts hit for vic");
                                var weaponString = string.Empty;
                                var bodyPartString = string.Empty;
                                var bodyPartSB = new StringBuilder();
                                var weaponSB = new StringBuilder();
                                if (!IsNullOrEmpty(vicWeapons)) for (int i = 0; i < vicWeapons.Count; i++) weaponSB.Append(vicWeapons[i]).Append(", ");
                                if (!IsNullOrEmpty(bodyParts)) for (int i = 0; i < bodyParts.Count; i++) bodyPartSB.Append(bodyParts[i]).Append(", ");
                                if (weaponSB.Length > 3) weaponSB.Length -= 2;
                                if (bodyPartSB.Length > 3) weaponSB.Length -= 2;
                                weaponString = weaponSB.ToString();
                                bodyPartString = bodyPartSB.ToString();
                                //     PrintWarning("do send");
                                var vicMsg = "<color=#ff912b>" + vicDamage.ToString("N0") + "</color> damage dealt in <color=#ff912b>" + vicHits + "</color> hits, hit body parts: " + bodyPartString + ", using weapon: <color=#cc3300>" + weaponString + "</color>";
                                //    PrintWarning("vicMsg: " + RemoveTags(vicMsg) + ", for hits containskey victim userid");
                                victim.SendConsoleCommand("echo " + vicMsg);
                                //       PrintWarning("Did send");
                                hits.Remove(victim.userID);
                                damages.Remove(victim.userID);
                                weaponsUsed.Remove(victim.userID);
                                bodyPartsHit.Remove(victim.userID);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintError("FAILED TO COMPLETE ONENTITYDEATH'S NEXTTICK (" + ex.Message + "): " + Environment.NewLine + ex.ToString());
                    if (errorTestSB.Length > 0) PrintWarning(errorTestSB.ToString().TrimEnd());
                    else PrintWarning("testsb length is not > 0: " + errorTestSB.ToString());
                }

            }); //temp disable until rust command fix!
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || entity == null || !init) return;
            var stopwatch = Pool.Get<Stopwatch>();
            var name = string.Empty;
            try
            {
                stopwatch.Restart();

                name = entity?.ShortPrefabName ?? string.Empty;

                var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.LAST;
                if (dmgType == Rust.DamageType.Decay)
                {
                    var block = entity as BuildingBlock;
                    if (block != null && block.grade > BuildingGrade.Enum.Wood)
                    {
                        var sb = Pool.Get<StringBuilder>();
                        try
                        {
                            var pos = entity?.transform?.position ?? Vector3.zero;

                            var xPos = (int)Math.Round(pos.x, MidpointRounding.AwayFromZero);
                            var yPos = (int)Math.Round(pos.y, MidpointRounding.AwayFromZero);
                            var zPos = (int)Math.Round(pos.z, MidpointRounding.AwayFromZero);

                            var vecStr = sb.Clear().Append(xPos).Append(" ").Append(yPos).Append(" ").Append(zPos).ToString();

                            var strToAppend = sb.Clear().Append(name).Append(" (").Append((int)block.grade).Append(") (").Append(entity.OwnerID).Append(") @ ").Append(vecStr).ToString();

                            _decaySbBuffer.Append(strToAppend).Append(Environment.NewLine);

                            if (_decaySbBuffer.Length >= 1024)
                            {
                                LogToFile("decay_log", _decaySbBuffer.ToString(), this);
                                _decaySbBuffer.Clear();
                            }

                            //LogToFile("decay_log", sb.Clear().Append(name).Append(" (").Append(block.grade).Append(") (").Append(entity.OwnerID).Append(") @ ").Append(vecStr).ToString(), this);
                        }
                        finally { Pool.Free(ref sb); }
                    }

                    return;
                    //  LogToFile("decay_log", entity?.ShortPrefabName + " (" + GetDisplayNameFromID(entity.OwnerID) + ") killed by decay at: " + (entity?.transform?.position ?? Vector3.zero), this);
                }

                var attacker = info?.Initiator as BasePlayer ?? null;

                var lootCont = entity as LootContainer;
                if (lootCont != null) allContainers.Remove(lootCont);

                var storage = entity as StorageContainer;
                if (storage != null) storageContainers.Remove(storage);



                var cupboard = entity as BuildingPrivlidge;
                if (cupboard != null)
                {
                    Puts("Cupboard (" + GetDisplayNameFromID(cupboard.OwnerID) + ") killed with dmgType: " + dmgType + " at: " + (entity?.transform?.position ?? Vector3.zero) + " attacker: " + (info?.InitiatorPlayer?.displayName ?? info?.Initiator?.ShortPrefabName ?? info?.WeaponPrefab?.ShortPrefabName ?? "Unknown Attacker"));
                    return;
                }

                if (serverType != ServerType.BF && IsEasyServer() && !IsHardServer() && lootCont?.inventory?.itemList != null && name.Contains("barrel")) //BF has all BPs
                {
                    var rng = UnityEngine.Random.Range(0, 101);
                    if (rng <= 90)
                    {
                        Item findBP = null;
                        for (int i = 0; i < lootCont.inventory.itemList.Count; i++)
                        {
                            var loot = lootCont.inventory.itemList[i];
                            if (loot != null && loot.IsBlueprint())
                            {
                                findBP = loot;
                                break;
                            }
                        }

                        if (findBP != null && (attacker?.blueprints?.HasUnlocked(findBP.blueprintTargetDef) ?? false))
                        {
                            var rngCat = GetRandomCategory(ItemCategory.Food, ItemCategory.Component, ItemCategory.All, ItemCategory.Common, ItemCategory.Resources);
                            var oldRarity = findBP.blueprintTargetDef.rarity;
                            var newRarity = UnityEngine.Random.Range(0, 101) <= 45 ? (Rust.Rarity)Mathf.Clamp(((int)oldRarity) + 1, 0, 4) : oldRarity;

                            var newBP = GetUnlearnedBP(newRarity, attacker, rngCat);
                            if (newBP == null)
                            {
                                //  PrintWarning("newBP is null!!!!! trying again!");
                                var maxBPTries = 125;
                                for (int i = 0; i < maxBPTries; i++)
                                {
                                    newBP = GetUnlearnedBP(newRarity, attacker, rngCat);
                                    if (newBP != null)
                                    {
                                        //    PrintWarning("Got newBP after: " + i + " tries");
                                        break;
                                    }
                                    if (i > 20) rngCat = GetRandomCategory();
                                    if (i > 5) newRarity = GetRandomRarity(rngCat, true, Rust.Rarity.VeryRare);
                                    else if (i > 25) newRarity = GetRandomRarity(rngCat, true);
                                }
                                if (newBP == null)
                                {
                                    PrintWarning("could not get newBP after " + maxBPTries + " tries!");
                                    return;
                                }
                            }
                            if (newBP.blueprintTargetDef.itemid == findBP.blueprintTargetDef.itemid)
                            {
                                PrintWarning("newBP was same itemid as original BP?!");
                                return;
                            }
                            if (newBP != null)
                            {
                                RemoveFromWorld(findBP);
                                if (!newBP.MoveToContainer(lootCont.inventory))
                                {
                                    newBP.Drop(lootCont.CenterPoint(), Vector3.up * 2f);
                                }
                            }
                            else PrintWarning("newBP is null, not removing findBP!");
                        }
                    }
                }




                if (entity?.prefabID == 3438187947) //oil_barrel
                {
                    //move to own explosive barrels plugin


                    var ammoType = info?.Weapon?.GetItem()?.GetHeldEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine?.ammoType ?? null;
                    var ammoName = ammoType?.shortname ?? string.Empty;


                    var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
                    var hitRng = majDmg == Rust.DamageType.Explosion || majDmg == Rust.DamageType.Heat || UnityEngine.Random.Range(0, 101) <= 33;
                    if (!hitRng) return;

                    if (majDmg != Rust.DamageType.Bullet && majDmg != Rust.DamageType.Explosion && majDmg != Rust.DamageType.Heat && !ammoName.Contains("explo") && !ammoName.Contains("incen") && !ammoName.Contains("fire")) return;


                    var dmgList = Pool.GetList<Rust.DamageTypeEntry>();

                    var dmg1 = new Rust.DamageTypeEntry
                    {
                        type = Rust.DamageType.Explosion,
                        amount = UnityEngine.Random.Range(65f, 75f)
                    };
                    dmgList.Add(dmg1);
                    var dmg2 = new Rust.DamageTypeEntry
                    {
                        type = Rust.DamageType.Heat,
                        amount = UnityEngine.Random.Range(6f, 12f)
                    };
                    dmgList.Add(dmg2);
                    var path = "assets/bundled/prefabs/fx/gas_explosion_small.prefab";
                    var prefab = entity.LookupPrefab();
                    var entCenter = entity?.CenterPoint() ?? Vector3.zero;
                    var entRotation = entity?.transform?.rotation ?? Quaternion.identity;
                    var fireballNew = GameManager.server.CreateEntity("assets/bundled/prefabs/napalm.prefab", entCenter, entRotation) as FireBall;
                    fireballNew.creatorEntity = attacker;
                    fireballNew.damagePerSecond = 12;
                    fireballNew.lifeTimeMin = 30;
                    fireballNew.lifeTimeMax = 50;
                    var tickrate = UnityEngine.Random.Range(0.075f, 0.175f);
                    fireballNew.tickRate = tickrate;
                    fireballNew.waterToExtinguish = 5000;
                    fireballNew.radius = 3.5f;
                    fireballNew.Spawn();
                    var timerr = UnityEngine.Random.Range(0.85f, 2.7f);
                    timer.Once(timerr, () =>
                    {
                        var path2 = "assets/prefabs/npc/patrol helicopter/effects/rocket_fire.prefab";
                        var path3 = "assets/bundled/prefabs/fx/explosions/explosion_01.prefab";
                        var path4 = "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab";

                        Effect.server.Run(path, entCenter, entCenter, null, false);
                        Effect.server.Run(path2, entCenter, entCenter, null, false);
                        Effect.server.Run(path3, entCenter, entCenter, null, false);
                        Effect.server.Run(path4, entCenter, entCenter, null, false);
                        NextTick(() =>
                        {
                            //2230528 <-- layer is player only?
                            //1084427009 <-- fireball layer?
                            DamageUtil.RadiusDamage(attacker, prefab, entCenter, 5f, 7f, dmgList, 1084427009, true);
                            Pool.FreeList(ref dmgList);
                        });

                    });

                }



            }
            finally
            {
                try { if (stopwatch.ElapsedMilliseconds > 7) PrintWarning("OnEntityDeath took: " + stopwatch.ElapsedMilliseconds + "ms for: " + name); }
                finally { Pool.Free(ref stopwatch); }
            }
        }

        private string GetBoneName(BaseCombatEntity entity, uint boneId) => entity?.skeletonProperties?.FindBone(boneId)?.name?.english ?? "Body";

        private string FirstUpper(string original)
        {
            if (string.IsNullOrEmpty(original)) return string.Empty;
            var array = original.ToCharArray();
            array[0] = char.ToUpper(array[0], CultureInfo.CurrentCulture);
            return new string(array);
        }

        private readonly Dictionary<ulong, float> lastHitMsg = new();
        private readonly Dictionary<ulong, Dictionary<ulong, int>> hits = new();
        private readonly Dictionary<ulong, Dictionary<ulong, float>> damages = new();
        private readonly Dictionary<ulong, Dictionary<ulong, List<string>>> weaponsUsed = new();
        private readonly Dictionary<ulong, Dictionary<ulong, List<string>>> bodyPartsHit = new();

        private bool isDM(BasePlayer player) { return Deathmatch?.Call<bool>("InAnyDM", player.UserIDString) ?? false; }

        private bool isVanished(BasePlayer player) { return Vanish?.Call<bool>("IsInvisible", player) ?? false; }

        private bool isESP(BasePlayer player) { return ServerESP?.Call<bool>("HasESP", player) ?? false; }

        private bool isGod(BasePlayer player) { return Godmode?.Call<bool>("IsGod", player.UserIDString) ?? false; }

        private static readonly System.Random fireRnd;

        //used for tea? i tuned it down a bit.
        private void OnBonusItemDrop(Item bonusItem, BasePlayer player)
        {
            if (bonusItem == null || player == null) return;

            PrintWarning(nameof(OnBonusItemDrop) + " " + bonusItem.info.shortname + " x" + bonusItem.amount + " for " + player.displayName);

            // if (bonusItem.info.itemid == SCRAP_ITEM_ID && IsEasyServer())
            //       bonusItem.amount *= UnityEngine.Random.Range(3, 5);

        }

        private bool IsOnCargo(BaseEntity entity)
        {
            return (entity?.GetParentEntity()?.prefabID ?? 0) == 3234960997;
        }

        private bool IsNearOilRig(BaseEntity entity) //yes, this code CAN be improved.
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

           return Vector3.Distance(SmallOilRig.transform.position, entity.transform.position) < 200 || Vector3.Distance(LargeOilRig.transform.position, entity.transform.position) < 200;
        }

        private object OnEntityTakeDamage(BasePlayer player, HitInfo info)
        {
            if (!init || player == null || info == null) return true;


            if (player?.net != null && immuneEntities.Contains(player.net.ID))
            {
                CancelDamage(info);
                return true;
            }

            if (info.Initiator != null && info.Initiator is BaseNpc && (info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic) != Rust.DamageType.Bleeding)
            {
                var chance = 45f;

                var connectedTimeModifier = Mathf.Clamp(100f - ((player?.secondsConnected ?? 0) * 0.1f), 0, 38);

                chance += connectedTimeModifier;

                var wearItemsCount = player?.inventory?.containerWear?.itemList?.Count ?? 0;

                chance -= (wearItemsCount * 3);


                if (UnityEngine.Random.Range(0, 101) <= chance)
                {
                    SendReply(player, "You shrug off the attempted bite from the animal and take no damage!");
                    ShowToast(player, "You shrug off the animal's bite!");
                    CancelDamage(info);
                    return true;
                }

            }



            var victim = player;
            var attacker = info?.Initiator as BasePlayer;
            var attackEnt = info?.Initiator;
            var boneID = info?.HitBone ?? 0;
            var boneName = (boneID == 0) ? "Body" : FirstUpper(GetBoneName(player, boneID)) ?? "Body";
            var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            var boneNameLower = boneName.ToLower().TrimEnd(' ');
            var dmgType = majDmg;

            var heliAttacker = info?.Initiator as PatrolHelicopter;
            if (heliAttacker != null)
            {
                var hasAnyGun = false;

                var items = Pool.GetList<Item>();
                try
                {
                    player.inventory.AllItemsNoAlloc(ref items);
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i]?.GetHeldEntity() is BaseProjectile)
                        {
                            hasAnyGun = true;
                            break;
                        }
                    }
                }
                finally { Pool.FreeList(ref items); }

                var wearCount = player?.inventory?.containerWear?.itemList?.Count ?? 0;
                if (!hasAnyGun) info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(0.25f, 0.67f));
                var armorScaleMin = wearCount == 0 ? 0.25f : wearCount == 1 ? 0.35f : wearCount == 2 ? 0.4f : 0.5f;
                var armorScaleMax = wearCount == 0 ? 0.5f : wearCount == 1 ? 0.67f : wearCount == 2 ? 0.75f : 1f;

                info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(armorScaleMin, armorScaleMax));
            }



            if (IsValidPlayer(victim))
            {
                if (victim?.metabolism != null && majDmg == Rust.DamageType.Explosion && (info?.damageTypes?.Total() ?? 0f) > 0)
                {
                    NextTick(() =>
                    {
                        if (victim == null || victim?.metabolism == null || (victim?.IsDead() ?? true)) return;
                        victim.metabolism.bleeding.Add(info.damageTypes.Total() / 8f);
                    });
                }
                if (majDmg == Rust.DamageType.Bite || majDmg == Rust.DamageType.Explosion || majDmg == Rust.DamageType.Heat || majDmg == Rust.DamageType.Bullet || majDmg == Rust.DamageType.Arrow)
                {
                    if (victim?.inventory?.containerWear?.itemList?.Count == 1)
                    {
                        var heavyScale = 0f;
                        for (int i = 0; i < victim.inventory.containerWear.itemList.Count; i++)
                        {
                            var item = victim.inventory.containerWear.itemList[i];
                            if (item == null) continue;
                            if (item?.info?.itemid == -1772746857)
                            {
                                heavyScale = 0.175f;
                                break;
                            }
                        }
                        if (heavyScale != 0f) info?.damageTypes?.ScaleAll(heavyScale);
                    }


                }


                if (dmgType == Rust.DamageType.Heat && ((info?.Initiator ?? null) == null))
                {
                    var vicTemp = victim?.metabolism?.temperature?.value ?? 0f;
                    if (vicTemp >= 45) info.damageTypes.ScaleAll((float)Math.Round(vicTemp / UnityEngine.Random.Range(11, 14)));
                }
            }



            if (info?.Initiator != null && IsValidPlayer(victim) && !IsHardServer() && info.Initiator.ShortPrefabName.Contains("scientist"))
            {
                var isOilRigScientist = IsNearOilRig(info.Initiator);


                if (!IsHardServer())
                {
                    var isJunk = info.Initiator.ShortPrefabName.Contains("scientist_junk");
                    var rngDmg = UnityEngine.Random.Range(0, 101) <= (IsEasyServer() ? UnityEngine.Random.Range(50, 76) : 40);

                    if (rngDmg && !isOilRigScientist && !IsOnCargo(info.Initiator))
                    {
                        if (isJunk && UnityEngine.Random.Range(0, 101) <= 10) CancelDamage(info);
                        else info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(0.775f, 0.925f));
                    }
                }


            }

            if (attacker != null)
            {
                var weaponItem = info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem() ?? null;
                if (weaponItem != null && banHammers.Contains(weaponItem))
                {
                    if (victim != null && !victim.IsAdmin)
                    {
                        if (!bhReason.TryGetValue(attacker.UserIDString, out string reason)) reason = "Cheating - Other (BH)";
                        //  var reason = "";
                        if (!kickHammers.Contains(weaponItem.uid))
                        {
                            if (attacker.IsAdmin) attacker.SendConsoleCommand("ban", victim.UserIDString, reason);
                            else covalence.Server.Command("ban", victim.UserIDString, reason);
                            SimpleBroadcast("<color=#33ccff><size=20>" + victim.displayName + " was struck by the banhammer for cheating!</size></color>");
                            SendReply(attacker, "Banned: " + victim.displayName + " (" + victim.UserIDString + ")");
                        }
                        else
                        {
                            if (victim.IsConnected)
                            {
                                victim.Kick(reason);
                                SendReply(attacker, "Kicked: " + victim.displayName + ", reason: " + reason);
                            }
                        }

                    }

                }
            }

            var majDmgStr = majDmg.ToString();

            var isEasy = IsEasyServer();
            if (majDmg == Rust.DamageType.Cold) info?.damageTypes?.Scale(Rust.DamageType.Cold, isEasy ? 1.15f : 2f);
            //var vicBleed = victim?.metabolism?.bleeding?.value ?? 0f;
          //  if (majDmg == Rust.DamageType.Bleeding) info?.damageTypes?.Scale(Rust.DamageType.Bleeding, 1.5f + Mathf.Clamp(vicBleed / (isEasy ? 20f : 12f), 0f, 10f));
            var heatScaler = 1.125f;
            if (isEasy && (majDmg == Rust.DamageType.Radiation || majDmg == Rust.DamageType.RadiationExposure)) info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(0.6f, 0.86f));
            if (majDmg == Rust.DamageType.Heat && victim != null && attackEnt != null)
            {
                info?.damageTypes?.Scale(Rust.DamageType.Heat, heatScaler);
                NextTick(() =>
                {
                    if (victim == null || (victim?.IsDead() ?? false) || !(victim?.IsConnected ?? false)) return;
                    var dmgAmount = info?.damageTypes?.Total() ?? 0f;
                    var temp = victim?.metabolism?.temperature?.value ?? 0f;
                    var addAmount = (1.25f + dmgAmount) * UnityEngine.Random.Range(1.1776f, 1.812f);

                    var rate = UnityEngine.Random.Range(1.1776f, 2.1812f);
                    if (attackEnt.ShortPrefabName.Contains("camp"))
                    {
                        addAmount += UnityEngine.Random.Range(6, 18);
                        rate += UnityEngine.Random.Range(1.5f, 4f);
                    }
                    victim.metabolism.temperature.MoveTowards(victim.metabolism.temperature.value + addAmount, rate);
                });
            }

            if (majDmg == Rust.DamageType.Bite)
            {
                NextTick(() =>
                {
                    if (victim == null || victim.IsDead() || victim?.metabolism == null) return;
                    var dmg = info?.damageTypes?.Total() ?? 0f;
                    if (dmg <= 0.0f) return;
                    victim.metabolism.bleeding.Add(dmg / UnityEngine.Random.Range(6f, 13f));
                });
            }

            if (majDmg == Rust.DamageType.Cold)
            {
                var wearList = victim?.inventory?.containerWear?.itemList ?? null;

                if (wearList?.Count == 1 && wearList[0]?.info?.itemid == -253079493)
                    info?.damageTypes?.Scale(Rust.DamageType.Cold, 0.45f);

                //historical hilarity purposes:

                /*/
                if (wearList?.Count == 1) //wearList.Count == 1 is an optimization check so we don't loop over items needlessly. the scientist suit cannot be worn without pieces, so this should always be 1
                {
                    //why are we looping for a count of 1? lmao. come on
                    for (int i = 0; i < wearList.Count; i++)
                    {
                        var item = wearList[i];
                        if (item?.info?.itemid == -253079493)
                        {
                            info?.damageTypes?.Scale(Rust.DamageType.Cold, 0.45f);
                            break;
                        }
                    }
                }/*/
            }
            return null;
        }

        private object OnCupboardAuthorize(BuildingPrivlidge privilege, BasePlayer player)
        {
            if (privilege == null || player == null) return null;
            if (privilege.OwnerID == 528491 && !player.IsAdmin) return true;
            return null;
        }

        private object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (!init || entity == null || info == null) return null;
            try
            {
                if (entity.OwnerID == 528491)
                {
                    CancelDamage(info);
                    return true;
                }
                if (entity?.net != null && immuneEntities.Contains(entity.net.ID))
                {
                    PrintWarning("has immunity: " + entity);
                    CancelDamage(info);
                    return true;
                }

                var victim = entity as BasePlayer;
                var attacker = info?.Initiator as BasePlayer;
                var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? null;

                var weapon = info?.WeaponPrefab ?? info?.Weapon ?? null;
                if (weapon == null) weapon = attacker?.GetHeldEntity()?.GetComponent<AttackEntity>() ?? null;

                ItemDefinition weaponAmmo = null;
                if (info?.Weapon != null) weaponAmmo = weapon?.GetComponent<BaseProjectile>()?.primaryMagazine?.ammoType ?? null;
                if (weaponAmmo == null && info?.WeaponPrefab != null) weaponAmmo = info?.WeaponPrefab?.GetComponent<BaseProjectile>()?.primaryMagazine?.ammoType ?? null;
                var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;

                var attackEnt = info?.Initiator ?? null;
                var isMelee = (info?.Weapon != null) ? (info?.Weapon?.GetComponent<BaseMelee>() != null) : (info?.WeaponPrefab != null) ? (info?.WeaponPrefab?.GetComponent<BaseMelee>() != null) : (dmgType == Rust.DamageType.Blunt || dmgType == Rust.DamageType.Slash || dmgType == Rust.DamageType.Stab);


                if (entity is Workbench)
                {
                    if (attacker != null && attacker.IsConnected && attacker.userID == entity.OwnerID) SendReply(attacker, "<color=#7a93bc>Use <color=#2872ed>/remove</color> if you want to remove this workbench.</color>");

                    var scalar = (entity.ShortPrefabName.Contains("bench1") && isMelee) ? 0.009f : entity.ShortPrefabName.Contains("bench2") ? 0.25f : entity.ShortPrefabName.Contains("bench3") ? 0.315f : 0.35f;
                    info?.damageTypes?.ScaleAll(scalar);
                }

                if (dmgType == Rust.DamageType.Explosion && IsEasyServer() && (weaponAmmo?.shortname?.Equals("ammo.rifle.explosive") ?? false))
                {
                    var rng = UnityEngine.Random.Range(0.45f, 0.68f);
                    info?.damageTypes?.ScaleAll(rng);
                }

                if (dmgType == Rust.DamageType.Decay && entity.OwnerID != 0)
                {
                    BasePlayer ply = null;
                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var p = BasePlayer.activePlayerList[i];
                        if (p != null && p.userID == entity.OwnerID && p.IsConnected)
                        {
                            ply = p;
                            break;
                        }
                    }
                    if (ply == null) //owner player is connected. no extra decay
                    {
                        var lastCon = GetLastConnection(entity.OwnerID);
                        if (lastCon != DateTime.MinValue)
                        {
                            var span = DateTime.UtcNow - lastCon;
                            var scaleAmount = 1.075f + (0.04f * (float)span.TotalHours);
                            info.damageTypes.ScaleAll(scaleAmount);
                        }
                    }
                }

                var getType = info?.Initiator?.GetType().ToString()?.ToLower() ?? string.Empty;

                var vicPrefab = entity?.ShortPrefabName ?? string.Empty;
                var isProj = info?.IsProjectile() ?? false;
                var hitPos = info?.HitPositionWorld ?? info?.HitEntity?.CenterPoint() ?? Vector3.zero;

                var projID = info?.ProjectileID ?? 0;
                var hitMaterial = info?.HitMaterial ?? 0;
                //BHAMMER START
                if (attacker != null)
                {
                    var weaponItem = info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem() ?? null;
                    if (weaponItem != null && banHammers.Contains(weaponItem))
                    {

                        entity.Invoke(() =>
                        {
                            if (entity == null || (entity?.IsDestroyed ?? true)) return;
                            for (int i = 0; i < UnityEngine.Random.Range(45, 75); i++) Effect.server.Run(SHOCK_EFFECT, SpreadVector(hitPos, UnityEngine.Random.Range(0.1f, 1.75f)), Vector3.zero);
                            entity.Hurt(99999f, Rust.DamageType.ElectricShock, entity, false);
                        }, 0.15f);

                        SendReply(attacker, "Ban hammer hit: " + (victim?.displayName ?? entity?.ShortPrefabName ?? "Unknown"));
                    }
                }
                //BHAMMER END

                var hitEnt = entity;
                var wepPrefab = info?.WeaponPrefab ?? null;
                var wepPrefabStr = wepPrefab?.ShortPrefabName ?? string.Empty;
                var prefabname = hitEnt?.ShortPrefabName?.ToLower() ?? string.Empty;

                var weaponName = hitEnt?.GetItem()?.info?.displayName?.english ?? "Unknown";
                if (majDmg == Rust.DamageType.Explosion && attacker != null && !isDM(attacker))
                {
                    var weaponPrefab = info?.WeaponPrefab?.ShortPrefabName ?? string.Empty;
                    if (weaponPrefab.Contains("rocket"))
                    {
                        var pos = GetShortVectorString(entity.transform.position);
                        var rocketMsg = string.Empty;

                        var sb = Pool.Get<StringBuilder>();
                        try
                        {
                            var replacePrefabStr = sb.Clear().Append(weaponPrefab).Replace("_", ".").ToString();

                            var itemDef = ItemManager.FindItemDefinition(sb.Clear().Append("ammo.").Append(replacePrefabStr).ToString());

                            var itemName = itemDef != null ? itemDef.displayName.english : sb.Clear().Append(wepPrefabStr).Replace("_basic", string.Empty).ToString();

                            var isVanish = isVanished(attacker);
                            var isEsp = isESP(attacker);

                            rocketMsg = sb.Clear().Append(attacker.displayName).Append(" hit: ").Append((GetItemDefFromPrefabName(prefabname)?.displayName?.english ?? prefabname ?? "Unknown")).Append(" (").Append(GetDisplayNameFromID(hitEnt?.OwnerID ?? 0)).Append("), with: ").Append(itemName).Append(" @ ").Append(pos).Append(isVanish ? ", is vanished" : string.Empty).Append(isEsp ? ", has ESP" : string.Empty).ToString();
                        }
                        finally { Pool.Free(ref sb); }



                        //  if (isVanish) rocketMsg = sb.Clear().Append(rocketMsg).Append(", is vanished").ToString();
                        //  if (isEsp) rocketMsg += ", has ESP";
                        if (!rocketBuffer.TryGetValue(attacker.UserIDString, out StringBuilder outSB)) rocketBuffer[attacker.UserIDString] = outSB = new StringBuilder();
                        /*/
                        if (outSB.Length > 0 && (outSB.Length + rocketMsg.Length) > 900)
                        {
                            sbs.Add(outSB);
                            outSB = new StringBuilder();
                        }/*/
                        outSB.AppendLine(rocketMsg);
                        rocketBuffer[attacker.UserIDString] = outSB;
                        InvokeHandler.Invoke(ServerMgr.Instance, () =>
                        {
                            //      if (sbs.Count > 0) for (int i = 0; i < sbs.Count; i++) PrintWarning(sbs[i].ToString().TrimEnd());
                            if (rocketBuffer.TryGetValue(attacker.UserIDString, out StringBuilder nowSB) && nowSB.Length > 0)
                            {
                                PrintWarning(nowSB.ToString().TrimEnd());
                                rocketBuffer[attacker.UserIDString].Clear();
                            }
                        }, 0.1f);
                    }
                }
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Failed to complete OnEntityTakeDamage"); }
            return null;
        }

        private void OnPlayerLootEnd(PlayerLoot inventory)
        {
            if (inventory == null) return;
            var player = inventory.GetComponent<BasePlayer>();
            if (player == null) return;
            var ent = inventory?.entitySource as LootContainer;

            var isLootable = ent?.isLootable ?? false;
            var itemList = ent?.inventory?.itemList ?? null;
            if (!isLootable && (itemList == null || itemList.Count < 1) && !(ent?.IsDestroyed ?? true))
            {
                PrintWarning("ent kill for some sort of lootable thing");
                ent.Kill();
                return;
            }

        }

        //SCREAM CODE
        /*/
        private void OnPlayerWound(BasePlayer player)
        {
            if (player == null || player?.gameObject == null || player.IsDestroyed || isVanished(player)) return;
            var pos = player?.transform?.position ?? Vector3.zero;
           // PlayScream(pos);
            Action woundInvoke = null;
            woundInvoke = InvokeRepeating(player, () =>
            {
                try
                {
                    if (player == null || player?.gameObject == null || player.IsDestroyed || !(player?.IsConnected ?? false) || !(player?.IsWounded() ?? false) || (player.IsAlive() && player.TimeAlive() < 30f) || player.IsDead())
                    {
                        InvokeHandler.CancelInvoke(player, woundInvoke);
                        return;
                    }
                    if (UnityEngine.Random.Range(0, 101) <= 60) PlayScream(pos);
                }
                catch (Exception ex)
                {
                    PrintError("woundInvoke exception: " + Environment.NewLine + ex.ToString());
                    InvokeHandler.CancelInvoke(player, woundInvoke);
                    return;
                }
            }, 0.01f, UnityEngine.Random.Range(7f, 10f), UnityEngine.Random.Range(15, 25));
        }/*/

        private void PlayScream(Vector3 pos) => Effect.server.Run("assets/bundled/prefabs/fx/player/beartrap_scream.prefab", pos, Vector3.zero, null);

        private void cmdServers(IPlayer player, string command, string[] args)
        {
            SendReply(player, "<size=20><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color></size>");
            SendReply(player, "<color=#66d7f9>Curious about our other servers?</color> <color=#51f7aa>We have three servers!\nOur first server has a <color=orange>0.5x</color> gather rate and is generally calmer. The second server is our <color=#FF0030>10x</color>, which has more players, higher gather rate, quicker gameplay, and much more.\nOur third server is a Battlefield server, with a gather rate of 100x, you spawn with guns & everything!\n\n\n10x IP: <color=#FF0030>prismrust.com</color>:<color=#8aff47>28016</color>\n2x IP: <color=#FE950A>prismrust.com</color>:<color=#53D559>28015</color></color>\n100x (BF) IP: <color=#FF0030>prismrust.com:28017</color>\n\n<color=#ff7ce4>We also have a Discord and Teamspeak server! Type <color=#8aff47>/discord</color> to find out more.</color>");
        }

        public bool IsDivisble(int x, int n) { return (x % n) == 0; }

        private bool IsValidPlayer(BasePlayer player) { return player != null && player.gameObject != null && !player.IsDestroyed && !player.IsNpc && player.prefabID == 4108440852; }

        private void cmdPlayers(IPlayer player, string command, string[] args)
        {
            if (player == null)
            {
                PrintWarning("cmdPlayers with null iplayer");
                return;
            }

            //this entire thing needs SB improvements

            var playerSB = new StringBuilder();


            var conStr = (BasePlayer.activePlayerList?.Count ?? 0).ToString("N0");
            var connectedTimes = new Dictionary<string, double>();

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null)
                    continue;

                var conSpan = GetConnectedTimeSpanS(p.UserIDString);

                if (conSpan <= TimeSpan.Zero)
                {
                    PrintWarning("p: " + p + " has <= timespan.zero!!!");
                    conSpan = TimeSpan.FromMinutes(15);
                }

                connectedTimes[p.UserIDString] = conSpan.TotalSeconds;
            }

            var top = from entry in connectedTimes orderby entry.Value descending select entry;
            var loopCount = 0;

            var isServer = player.IsServer;

            var sb = Pool.Get<StringBuilder>();
            try
            {

                foreach (var p in top)
                {
                    var txt = IsDivisble(loopCount, 2) ? (sb.Clear().Append(isServer ? "" : "<color=#2376fc>").Append(GetDisplayNameFromID(p.Key)).Append(isServer ? "" : "</color>").Append(", ").ToString()) : sb.Clear().Append(GetDisplayNameFromID(p.Key)).Append(", ").ToString();

                    playerSB.Append(txt);

                    loopCount++;
                }
            }
            finally { Pool.Free(ref sb); }

            if (isServer)
            {
                if (playerSB.Length > 2) playerSB.Length -= 2; //trailing ", "
                SendReply(player, conStr + " players: " + playerSB.ToString());
                return;
            }

            var pMsg = "<color=#8aff47>" + conStr + "</color> players connected, " + ServerMgr.Instance.connectionQueue.joining.Count.ToString("N0") + " joining (highlights for readability):";
            SendReply(player, pMsg);


            if (playerSB.Length > 0)
            {
                if (playerSB.Length > 2)
                    playerSB.Length -= 2;

                SendReply(player, "<color=#547ab7>" + playerSB.ToString() + "</color>");
            }

            var legitCount = GetUniquePlayerCount(TimeSpan.FromDays(30), args.Length > 0 && args[0].Equals("nocache", StringComparison.OrdinalIgnoreCase));

            var legitCountButMakeItFake = legitCount * 2;

            //var legitCountButMakeItFake = GetUniquePlayerCount(TimeSpan.FromDays(30), args.Length > 0 && args[0].Equals("nocache", StringComparison.OrdinalIgnoreCase)) * 2;

            SendReply(player, "In the last 30 days, there have been <color=#8aff47>" + (player.IsAdmin ? legitCount : legitCountButMakeItFake).ToString("N0") + "</color> unique players! (this number is cached and only updates every hour)");

            if (player.IsAdmin)
                SendReply(player, nameof(legitCountButMakeItFake) + ": " + legitCountButMakeItFake.ToString("N0"));
        }

        private DateTime GetSaveTime()
        {
            return SaveRestore.SaveCreatedTime;
        }



        private void cmdMoveLog(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var now = DateTime.UtcNow;
            if (args.Length < 1)
            {
                SendReply(player, "You must specify a target!");
                return;
            }
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                SendReply(player, "No target found: " + args[0]);
                return;
            }
            if (!ulong.TryParse(target.Id, out ulong uID))
            {
                SendReply(player, "Not a ulong: " + target.Id);
                return;
            }
            var outTime = default(TimeSpan);
            if (args.Length > 1)
            {
                if (!double.TryParse(args[1], out double minutes))
                {
                    SendReply(player, "Bad minutes: " + args[1]);
                    return;
                }
                else
                {
                    outTime = now - now.AddMinutes(-minutes);
                    PrintWarning("parsed as minutes: " + minutes + ", outTime = now.AddMinutes(-minutes) = " + outTime);
                }
            }
            var noText = false;
            if (args.Length > 2 && !bool.TryParse(args[2], out noText))
            {
                SendReply(player, "Not a bool: " + args[2]);
                return;
            }
            var maxTime = outTime != default ? outTime : (now - now.AddMinutes(-1440));
            var logs = GetMovementLogs(uID, maxTime);
            if (logs == null || logs.Count < 1)
            {
                SendReply(player, "No logs for max time: " + maxTime + " (" + maxTime.TotalMinutes.ToString("N0") + " minutes)");
                return;
            }
            ServerMgr.Instance.StartCoroutine(SendMoveLogs(player, logs, !noText));
        }

        private System.Collections.IEnumerator SendMoveLogs(IPlayer player, List<MovementLog> logs, bool ddrawText = true)
        {
            if (player == null || logs == null || logs.Count < 1) yield return null;
            var logSB = new StringBuilder();
            var logSBs = Pool.GetList<StringBuilder>();
            var logCount = 0;
            var logMax = 80;
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log == null) continue;
                logCount++;
                if (logCount >= logMax)
                {
                    logCount = 0;
                    yield return CoroutineEx.waitForEndOfFrame;
                }
                var logStr = i.ToString("N0") + ": " + log.Position + ", " + log.Time + " (" + (DateTime.UtcNow - log.Time).TotalMinutes.ToString("N0") + " minutes ago)";
                if (logSB.Length + logStr.Length >= 768)
                {
                    logSBs.Add(logSB);
                    logSB = new StringBuilder();
                }
                logSB.AppendLine(logStr);
                var ply = player?.Object as BasePlayer;
                if (ply != null && ply.IsConnected)
                {
                    if (Vector3.Distance(log.Position, ply?.transform?.position ?? Vector3.zero) > 300) continue;
                    if (ddrawText) ply.SendConsoleCommand("ddraw.text", 15f, Color.green, log.Position, "<size=12>" + logStr + "</size>");
                    if ((i + 1) <= (logs.Count - 1))
                    {
                        var nextLog = logs[i + 1];
                        if (nextLog != null) ply.SendConsoleCommand("ddraw.arrow", 15f, Color.yellow, log.Position, nextLog.Position, 0.35f);
                    }
                }
            }
            if (logSBs.Count > 0)
            {
                var sbCount = 0;
                var sbMax = 25;
                for (int i = 0; i < logSBs.Count; i++)
                {
                    sbCount++;
                    if (sbCount >= sbMax)
                    {
                        sbCount = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    SendReply(player, logSBs[i].ToString().TrimEnd());
                }
            }
            if (logSB.Length > 0) SendReply(player, logSB.ToString().TrimEnd());
            SendReply(player, "Showing " + logs.Count.ToString("N0") + " logs");
            Pool.FreeList(ref logSBs);
        }

        private object CanBeWounded(BasePlayer player, HitInfo hitinfo)
        {
            if (player == null) return null;
            var lastDmg = player?.lastDamage ?? Rust.DamageType.Generic;
            var dmgType = hitinfo?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            if (dmgType == Rust.DamageType.Fall || lastDmg == Rust.DamageType.Fall) return false;
            else return null;
        }

        private object CanBetterChatClean(string userId, string message)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(message)) return null;
            var player = covalence.Players?.FindPlayerById(userId) ?? null;
            if (player == null || !player.IsConnected || player.IsAdmin) return null;
            var msgLower = message.ToLower();
            var msgLowerNoSpace = msgLower.Replace(" ", string.Empty).Replace(".", string.Empty);
            if (msgLowerNoSpace.Contains("hacking") || msgLowerNoSpace.Contains("hacks") || msgLowerNoSpace.Contains("hacker") || msgLowerNoSpace.Contains("cheat") || msgLowerNoSpace.Contains("cheet") || msgLowerNoSpace.Contains("h4x") || msgLowerNoSpace.Contains("haker") || msgLowerNoSpace.Contains("cheter") || msgLowerNoSpace.Contains("h4ck") || msgLowerNoSpace.Contains("ch3at") || msgLowerNoSpace.Contains("aimbot") || msgLower.Contains("noclip") || msgLower.Contains("esp ") || msgLower.EndsWith("esp") || msgLowerNoSpace.Contains("haking"))
            {
                PrintWarning("Blocking cheat message: " + message + Environment.NewLine + " from: " + player?.Name);
                SendReply(player, "<color=#DD0000>            <size=30>Nope.</size>            </color>\n<color=#F46FA0>Your message appears to have contained something about a cheater, and that could let the potential cheater know someone suspects them, so we've blocked your message.</color>\n<color=#DD3E80>Please do not report cheaters in public chat, instead report them privately.</color> <color=#53D559>Read <color=#33D5FF>/reporthelp</color> for more information. <color=#8aff47>Thanks!\n\nIf you don't like this feature, please let an administrator or moderator know.</color></color>");
                var basePly = player?.Object as BasePlayer;
                if (basePly != null && !basePly.IsDestroyed && !basePly.IsDead() && basePly.IsConnected) SendLocalEffect(basePly, "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab");
                return false;
            }
            return null;
        }

        private object CanPlayerPM(string userID, string targetID, string msg)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(targetID) || string.IsNullOrEmpty(msg)) return null;
            var player = covalence.Players?.FindPlayerById(userID) ?? null;
            var target = covalence.Players?.FindPlayerById(targetID) ?? null;
            if ((player != null && player.IsAdmin) || target != null && target.IsAdmin) return null;
            var lowerMsg = msg.ToLower();
            if (lowerMsg.Contains("rusticaland") || lowerMsg.Replace(" ", "").Contains("rusticaland")) return false;
            return null;
        }


        private readonly Dictionary<ulong, float> lastBleachTime = new();

        private float TimeSinceLastBleach(BasePlayer player)
        {
            if (player == null) return -1f;

            return lastBleachTime.TryGetValue(player.userID, out float lastBleach) ? UnityEngine.Time.realtimeSinceStartup - lastBleach : -1f;
        }

        private class SmoothMovement : MonoBehaviour
        {
            public BaseEntity entity;
            public Vector3 startPos = Vector3.zero;
            private Vector3 _endPos = Vector3.zero;
            public Vector3 endPos
            {
                get { return _endPos; }
                set
                {
                    _endPos = value;
                    journeyLength = Distance;
                }
            }
            public Vector3 EulerAngles = Vector3.zero;
            public float Distance { get { return Vector3.Distance(startPos, endPos); } }
            public float startTime = 0f;
            public float speed = 10f;
            //   private readonly float lerpTime = 0f;
            // private readonly float currentLerpTime = 0f;
            private float journeyLength;
            public bool doRotation = false;
            public bool DestroyOnDestinationHit = true;

            private void Awake()
            {
                entity = GetComponent<BaseEntity>();
                if (entity == null || entity.IsDestroyed)
                {
                    DoDestroy();
                    return;
                }
                startPos = entity.transform.position;
                startTime = UnityEngine.Time.time;
            }

            public void DoDestroy() => Destroy(this);

            private void Update()
            {
                if (entity == null || entity?.transform == null) return;
                if (startPos == Vector3.zero || endPos == Vector3.zero) return;
                if (doRotation && EulerAngles != Vector3.zero)
                {
                    var lerpEuler = Vector3.Lerp(entity.transform.eulerAngles, EulerAngles, UnityEngine.Time.deltaTime * speed);
                    var rotEuler = Quaternion.Euler(lerpEuler);
                    entity.transform.rotation = rotEuler;
                }

                var fracJourney = (UnityEngine.Time.time - startTime) * speed / journeyLength;
                entity.transform.position = Vector3.Lerp(startPos, endPos, fracJourney);
                entity.SendNetworkUpdateImmediate();
                if (DestroyOnDestinationHit && Vector3.Distance(entity.transform.position, endPos) <= 0.05) DoDestroy();
            }

        }


        private void SetLastBleachTime(BasePlayer player, float time) => lastBleachTime[player.userID] = time;

        private readonly List<string> ignoreAmmo = new();
        private readonly Dictionary<ulong, float> lastInput = new();
        private readonly HashSet<ulong> glEnabled = new();
        private readonly Dictionary<string, float> lastCannonTime = new();
        private readonly Dictionary<string, float> cannonRate = new();
        private readonly Dictionary<string, long> lastFreeze = new();
        private readonly Dictionary<string, float> lastApcMG = new();

        private void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using var fx = new Effect(effect, pos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using var fx = new Effect(effect, player, boneID, localPos, Vector3.zero);
            fx.scale = scale;
            EffectNetwork.Send(fx, player?.net?.connection);
        }

        private BaseVehicle GetVehicleFromSeat(BaseMountable mountable)
        {
            if (mountable == null || !(mountable is BaseVehicleSeat)) return null;
            return null;
            //  return (mountable as BaseVehicleSeat)?.GetVehicleParent() ?? null;
        }

        private BaseVehicle GetVehicleFromSeat(BaseVehicleSeat seat)
        {
            if (seat == null) return null;

            return null;
            // return seat?.GetVehicleParent() ?? null;
        }

        public bool NearGround(Vector3 pos, float dist = 0.1f)
        {
            if (pos == Vector3.zero) return false;
            if (UnityEngine.Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit hit, dist, groundWorldLayer)) return true;
            return false;
        }

        private void DoGlueEffect(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead() || !player.IsConnected) return;
            RenderGlueUI(player);
            var times = 0;
            var halucTimes = 0;
            var halucMax = UnityEngine.Random.Range(10, 20);
            var maxTimes = UnityEngine.Random.Range(10, 30);
            Action uiAct = null;
            Action halucAct = null;
            halucAct = new Action(() =>
            {
                halucTimes++;
                //    PrintWarning("Haluc times: " + halucTimes + "/" + halucMax);
                if (player == null || player.IsDead() || times >= maxTimes || halucFXs == null || halucFXs.Count < 1)
                {
                    InvokeHandler.CancelInvoke(player, halucAct);
                    return;
                }
                var effectsToPlay = UnityEngine.Random.Range(0, 100) <= 50 ? UnityEngine.Random.Range(2, 4) : 1;
                var rngFX = UnityEngine.Random.Range(0, 100) <= 30 ? halucFXs.GetRandom() : halucFXs.Where(p => !p.Contains("impacts"))?.ToList()?.GetRandom() ?? halucFXs.GetRandom();
                //PrintWarning("haluc: " + rngFX);
                var spread = !rngFX.Contains("impacts") ? SpreadVector(player.CenterPoint(), UnityEngine.Random.Range(2.5f, 12f)) : SpreadVector(player.CenterPoint(), UnityEngine.Random.Range(1.2f, 3.5f));
                if (effectsToPlay < 2)
                {
                    if (rngFX.Contains("attack_") || rngFX.EndsWith("attack.prefab")) SendLocalEffect(player, rngFX, 1f);
                    else SendLocalEffect(player, rngFX, spread);
                }
                else
                {
                    for (int i = 0; i < effectsToPlay; i++)
                    {
                        if (player == null || player.IsDead()) break;
                        rngFX = UnityEngine.Random.Range(0, 100) <= 30 ? halucFXs.GetRandom() : halucFXs.Where(p => !p.Contains("impacts"))?.ToList()?.GetRandom() ?? halucFXs.GetRandom();
                        //    PrintWarning("Haluc: " + rngFX + " (" + i + "/" + effectsToPlay + ")");
                        spread = !rngFX.Contains("impacts") ? SpreadVector(player.CenterPoint(), UnityEngine.Random.Range(2.5f, 12f)) : SpreadVector(player.CenterPoint(), UnityEngine.Random.Range(1.2f, 3.5f));
                        if (rngFX.Contains("attack_") || rngFX.EndsWith("attack.prefab")) SendLocalEffect(player, rngFX, 1f);
                        else SendLocalEffect(player, rngFX, spread);
                    }
                }
            });
            uiAct = new Action(() =>
            {
                times++;
                // PrintWarning("Times: " + times + "/" + maxTimes);
                if (player == null || player.IsDead() || times >= maxTimes)
                {
                    if (player != null && player.IsConnected)
                    {
                        CuiHelper.DestroyUi(player, "GlueUI2");
                        CuiHelper.DestroyUi(player, "GlueUI");
                    }
                    InvokeHandler.CancelInvoke(player, uiAct);
                    return;
                }
                var rngGreen = UnityEngine.Random.Range(0f, 1f);
                var rngBlue = UnityEngine.Random.Range(0f, 1f);
                var rngRed = UnityEngine.Random.Range(0f, 1f);
                var rngAlpha = UnityEngine.Random.Range(0.1f, 0.48f);
                RenderGlueUI(player, rngRed.ToString("0.000") + " " + rngGreen.ToString("0.000") + " " + rngBlue.ToString("0.000") + " " + rngAlpha.ToString("0.000"));
            });
            InvokeHandler.InvokeRandomized(player, uiAct, UnityEngine.Random.Range(0.5f, 4f), UnityEngine.Random.Range(0.8f, 5f), UnityEngine.Random.Range(0.7f, 2.1f));
            InvokeHandler.InvokeRandomized(player, halucAct, UnityEngine.Random.Range(8f, 20f), UnityEngine.Random.Range(9f, 28f), UnityEngine.Random.Range(0.5f, 3f));
        }

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                if (input == null || player == null) return;
                if (!player.IsAlive() || player.IsSleeping() || player.IsWounded()) return;

                var activeItem = player?.GetActiveItem() ?? null;
                var isAdmin = player?.IsAdmin ?? false;
                var userID = player?.userID ?? 0;
                var userIDString = player?.UserIDString ?? string.Empty;
                var rotation = GetPlayerRotation(player);
                var eyePos = player?.eyes?.position ?? Vector3.zero;

                var usePressed = input.WasJustPressed(BUTTON.USE);


                if (usePressed)
                {
                    if (activeItem != null && activeItem.info.shortname == "fish.troutsmall")
                    {
                        var swap = activeItem?.info?.GetComponent<ItemModSwap>() ?? null;
                        swap?.DoAction(activeItem, player);
                    }

                    /*/
                    var getLook = GetLookAtEntity(player, 3f, 1) as FreeableLootContainer;
                    if (getLook != null && !getLook.IsDestroyed)
                    {
                        var lootTime = getLook.ShortPrefabName.Contains("advanced") ? UnityEngine.Random.Range(8f, 11f) : UnityEngine.Random.Range(4f, 8f);
                        InvokeHandler.Invoke(player, () =>
                        {
                            getLook = GetLookAtEntity(player, 3f, 1) as FreeableLootContainer;
                            if (getLook == null || getLook.IsDestroyed || !player.serverInput.IsDown(BUTTON.USE)) return;

                            getLook.RPC_FreeCrate(_defaultMessage);
                        }, lootTime);
                    }/*/
                }



                if (isAdmin)
                {

                    if (usePressed && rotEnts.TryGetValue(player.UserIDString, out BaseEntity rotEnt) && rotEnt != null && !rotEnt.IsDestroyed)
                    {
                        var rot = rotEnt.transform.rotation;
                        rotEnt.transform.eulerAngles = new Vector3(Mathf.Clamp(rot.eulerAngles.x + 1, 0, 999), rot.eulerAngles.y, rot.eulerAngles.z);
                        rotEnt.SendNetworkUpdate();
                        (rotEnt as BuildingBlock)?.UpdateSkin(true);
                    }

                    /*/
                    if (input.IsDown(BUTTON.FORWARD))
                    {
                        player.PauseFlyHackDetection(1f);
                        player.PauseVehicleNoClipDetection(1f);

                        player.ApplyInheritedVelocity(player.eyes.HeadForward() * 12f);
                  
                    }
                    else player.ApplyInheritedVelocity(Vector3.zero);

                    if (input.WasJustPressed(BUTTON.JUMP))
                    {
                        player.PauseFlyHackDetection(1f);
                        player.PauseFlyHackDetection(1f);
                        player.ApplyInheritedVelocity(Vector3.up * 30f);
                       

                    }/*/ //this all works! :)

                    var lastInputTime = -1f;
                    if (lastInput.TryGetValue(player.userID, out lastInputTime))
                    {
                        if ((UnityEngine.Time.realtimeSinceStartup - lastInputTime) >= 5f) lastInput[player.userID] = UnityEngine.Time.realtimeSinceStartup;
                    }
                    else lastInput[player.userID] = UnityEngine.Time.realtimeSinceStartup - 5;
                    if (lastInputTime != -1f)
                    {
                        var inputTimeSince = UnityEngine.Time.realtimeSinceStartup - lastInputTime;
                        if (inputTimeSince >= 5f)
                        {
                            if ((player?.IsFlying ?? false) && !isVanished(player) && Vanish != null) SendReply(player, "<size=20><color=orange>You're flying while un-vanished!</color></size>");
                        }
                    }
                    if (input.IsDown(BUTTON.FIRE_SECONDARY) && cannonUsers.Contains(player.UserIDString))
                    {
                        if (!cannonRate.TryGetValue(player.UserIDString, out float cannonDelay)) cannonDelay = 0.075f;
                        if (!lastApcMG.TryGetValue(player.UserIDString, out float lastMG) || (UnityEngine.Time.realtimeSinceStartup - lastMG) >= cannonDelay)
                        {
                            var aimConeDirection = AimConeUtil.GetModifiedAimConeDirection(UnityEngine.Random.Range(8f, 12f), player.eyes.rotation * Vector3.forward, true);
                            Effect.server.Run("assets/prefabs/npc/m2bradley/effects/coaxmgmuzzle.prefab", eyePos, Vector3.zero);

                            if (UnityEngine.Physics.SphereCast(new Ray(eyePos, aimConeDirection), 0.125f, out RaycastHit rayHit, 150f))
                            {
                                if (rayHit.point != Vector3.zero)
                                {
                                    player.ClientRPC(null, "CLIENT_FireGun", true, rayHit.point);
                                    var hitEnt = rayHit.GetEntity();
                                    var collider = rayHit.collider ?? rayHit.GetCollider() ?? GetEntityColliders(hitEnt)?.FirstOrDefault() ?? null;
                                    var hitMat = ((collider == null) ? string.Empty : collider?.material?.name ?? collider?.sharedMaterial?.name ?? string.Empty).Replace("(Instance)", string.Empty, StringComparison.OrdinalIgnoreCase).TrimEnd().ToLower();
                                    var hitCol = (hitMat != "default") ? StringPool.Get(hitMat) : 0;
                                    Effect.server.ImpactEffect(new HitInfo()
                                    {
                                        HitEntity = (hitEnt != null && hitEnt.IsDestroyed) ? hitEnt : null,
                                        HitPositionWorld = rayHit.point - aimConeDirection * 0.25f,
                                        HitNormalWorld = -aimConeDirection,
                                        HitMaterial = hitCol
                                    });
                                    if (hitEnt != null && !hitEnt.IsDestroyed)
                                    {
                                        var info = new HitInfo(null, hitEnt, Rust.DamageType.Bullet, UnityEngine.Random.Range(12, 25), rayHit.point);
                                        if (hitEnt is BaseCombatEntity) (hitEnt as BaseCombatEntity)?.OnAttacked(info);
                                        else hitEnt.OnAttacked(info);
                                    }
                                }
                            }
                            lastApcMG[player.UserIDString] = UnityEngine.Time.realtimeSinceStartup;
                        }
                    }
                    if (input.IsDown(BUTTON.FIRE_PRIMARY))
                    {
                        if (cannonUsers.Contains(player.UserIDString))
                        {
                            if (!cannonRate.TryGetValue(player.UserIDString, out float cannonDelay)) cannonDelay = 0.2f;
                            if (!lastCannonTime.TryGetValue(player.UserIDString, out float lastCannon) || (UnityEngine.Time.realtimeSinceStartup - lastCannon) >= cannonDelay)
                            {
                                lastCannonTime[player.UserIDString] = UnityEngine.Time.realtimeSinceStartup;
                                //  var eyePos = player?.eyes?.position ?? Vector3.zero;
                                var eyeForward = player?.eyes?.HeadForward() ?? Vector3.zero;
                                if (player.IsRunning() || input.IsDown(BUTTON.SPRINT)) eyeForward *= player.IsFlying ? 7f : 1.4f;
                                //   var pSpeed = 2.5;// GetMetersPerSecond(player);
                                /*/
                                if (pSpeed <= 0) pSpeed = 2.5;
                                if (pSpeed > 1)
                                {
                                    //   PrintWarning("eyeForward before: " + eyeForward);
                                    eyeForward *= Mathf.Clamp((float)pSpeed, 1f, 10f);
                                    //     PrintWarning("now: " + eyeForward + ", pSpeed: " + (float)pSpeed);
                                }/*/
                                eyePos += eyeForward;
                                var usePoint = eyePos;
                                var aimConeDirection = AimConeUtil.GetModifiedAimConeDirection(2f, player.eyes.rotation * Vector3.forward, true);
                                if (UnityEngine.Physics.SphereCast(new Ray(player.eyes.position, aimConeDirection), 0.1f, out RaycastHit rayHit, Vector3.Distance(player.eyes.position, eyePos)))
                                {
                                    if (rayHit.point != Vector3.zero) eyePos = rayHit.point;
                                }
                                Effect.server.Run("assets/prefabs/npc/m2bradley/effects/maincannonattack.prefab", eyePos, Vector3.zero);
                                var shell = GameManager.server.CreateEntity("assets/prefabs/npc/m2bradley/maincannonshell.prefab", eyePos, Quaternion.LookRotation(aimConeDirection));
                                var projectile = shell.GetComponent<ServerProjectile>();
                                projectile?.InitializeVelocity(eyePos + eyeForward * 25f - eyePos);
                                shell.Spawn();
                            }
                        }
                        if (targetRockets.Contains(player.userID))
                        {
                            //  var eyePos = player?.eyes?.position ?? Vector3.zero;
                            var ray = new Ray(eyePos, rotation);
                            var pos = Vector3.zero;
                            BaseEntity target = null;
                            if (UnityEngine.Physics.Raycast(ray, out RaycastHit rayHit, 500, 1084427009)) pos = rayHit.point;
                            var sphereHits = UnityEngine.Physics.SphereCastAll(ray, 2.5f, 250f, flamerColl)?.Where(p => p.GetEntity() != null && p.GetEntity()?.transform != null && p.GetEntity() != player && !p.GetEntity().ShortPrefabName.Contains("rocket"))?.ToArray() ?? null;
                            var distances = sphereHits != null && sphereHits.Length > 0 ? sphereHits.Select(p => Vector3.Distance(p.GetEntity().transform.position, eyePos)) : null;
                            if (distances != null)
                            {
                                var minDist = distances?.Min() ?? -1f;
                                target = sphereHits?.Where(p => p.GetEntity() != null && Vector3.Distance(p.GetEntity().transform.position, eyePos) <= minDist)?.FirstOrDefault().GetEntity() ?? null;
                                if (target != null) SendReply(player, "Got target: " + (target?.ShortPrefabName ?? string.Empty));
                            }

                            if (pos == Vector3.zero) return;
                            var adjPos = player.eyes.BodyForward() * ((player.IsRunning() && player.IsFlying) ? 8f : player.IsRunning() ? 4f : 2f);
                            var rocket = LaunchRocket(player.eyes.position + player.eyes.BodyForward() * 2f, pos, Vector3.zero, (target is BasePlayer) ? 3.5f : 20f, 0.1f, trackEntity: target);
                            DisableCollision(rocket, player);
                        }
                    }

                }

                if (input.IsDown(BUTTON.SPRINT) && (usePressed || input.IsDown(BUTTON.USE)))
                {
                    var isBleach = (activeItem?.info?.shortname ?? string.Empty).Contains("bleach");
                    var isGlue = (activeItem?.info?.shortname ?? string.Empty).Contains("glue");
                    if (isBleach || isGlue)
                    {
                        var lastBleach = TimeSinceLastBleach(player);
                        if (lastBleach >= 3.5f || lastBleach == -1f || (isGlue && lastBleach <= 0.7f))
                        {

                            // var DidRemoveItem = ItemHooks?.Call<bool>("RemoveItem", player, activeItem, 1) ?? false;
                            if (activeItem.amount > 1)
                            {
                                activeItem.amount--;
                                activeItem.MarkDirty();
                            }
                            else RemoveFromWorld(activeItem);
                            var DidRemoveItem = true;
                            if (DidRemoveItem)
                            {
                                if (isBleach)
                                {
                                    var rngTime = 0.01f;
                                    for (int i = 0; i < UnityEngine.Random.Range(4, 6); i++)
                                    {
                                        // PrintWarning("Starting invoke?");

                                        timer.Once(rngTime, () =>
                                        {
                                            //      PrintWarning("drink generic, rng time: " + rngTime);
                                            Effect.server.Run("assets/bundled/prefabs/fx/gestures/drink_generic.prefab", player.CenterPoint(), player.CenterPoint());
                                        });

                                        //  PrintWarning("Did invoke, but does this piece of shit not run multiple times, somehow?");
                                        rngTime += UnityEngine.Random.Range(0.5f, 0.515f);

                                    }
                                    player.Invoke(() =>
                                    {
                                        for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++) Effect.server.Run("assets/bundled/prefabs/fx/gestures/drink_vomit.prefab", player.CenterPoint(), player.CenterPoint());
                                    }, UnityEngine.Random.Range(3f, 3.35f));

                                }
                                else
                                {
                                    DoGlueEffect(player);

                                }

                                player.metabolism.poison.Add(!isGlue ? UnityEngine.Random.Range(4f, 10f) : UnityEngine.Random.Range(0.175f, 0.85f));
                                player.Hurt(!isGlue ? UnityEngine.Random.Range(10, 16) : UnityEngine.Random.Range(1f, 2.5f), Rust.DamageType.Poison, player, false);
                                SetLastBleachTime(player, UnityEngine.Time.realtimeSinceStartup);
                            }
                            else
                            {
                                SendReply(player, "Failed to drink bleach or sniff glue! Report to an Administrator.");
                                PrintWarning("Failed to drink bleach?: " + (player?.displayName ?? "Unknown"));
                            }
                        }
                    }
                }
                if (frozenPlayers.Contains(player.UserIDString))
                {
                    if (input.IsDown(BUTTON.FORWARD) || input.WasJustPressed(BUTTON.FORWARD) || input.IsDown(BUTTON.BACKWARD) || input.WasJustPressed(BUTTON.BACKWARD) || input.IsDown(BUTTON.LEFT) || input.WasJustPressed(BUTTON.LEFT) || input.IsDown(BUTTON.RIGHT) || input.WasJustPressed(BUTTON.RIGHT) || input.WasJustPressed(BUTTON.JUMP))
                    {
                        lastFreeze[player.UserIDString] = DateTime.UtcNow.Ticks;
                        var oldPos = player?.transform?.position ?? Vector3.zero;
                        if (oldPos == Vector3.zero) return;
                        NextTick(() =>
                        {
                            if (player == null) return;
                            var newPos = player?.transform?.position ?? Vector3.zero;
                            if (Vector3.Distance(oldPos, newPos) < 0.02) return;
                            TeleportPlayer(player, oldPos, false, false);
                        });
                    }
                }


            }
            finally
            {
                if (watch.ElapsedMilliseconds >= 4) PrintWarning("OnPlayerInput took too long! took: " + watch.ElapsedMilliseconds + "ms");
                Pool.Free(ref watch);
            }

        }

        private readonly Dictionary<int, int> craftedMsgTimes = new();

        //todo: not generate garbage - use pooled SBs
        private void OnItemCraftFinished(ItemCraftTask task, Item item, ItemCrafter crafter)
        {
            var player = crafter?.owner;
            if (player == null)
            {
                PrintWarning("player is null!!! crafter null?: " + (crafter == null));
                return;
            }

            if (item == null)
            {
                PrintWarning("item on craft finish is null!!!!");
                return;
            }

            var itemName = item?.info?.displayName?.english ?? "Unknown";
            var itemAmount = item?.amount ?? 0;
            var taskAmount = task?.amount ?? 0;
            var taskCrafted = task?.numCrafted ?? 0;
            var taskID = task?.taskUID ?? 0;
            var adjustedItemAmount = itemAmount * taskCrafted;
            var rarity = item?.info?.rarity ?? Rust.Rarity.None;
            if (!craftedMsgTimes.TryGetValue(taskID, out int craftMsgTimes)) craftedMsgTimes[taskID] = 0;
            else
            {
                if (craftMsgTimes > 1)
                {
                    return;
                }
            }
            //  if (craftedMsgTimes.TryGetValue(taskID, out craftMsgTimes) && craftMsgTimes > 1) return;
            string craftStr;
            if (taskAmount > 1)
            {
                craftStr = player.displayName + " has/is crafted (crafting): " + itemName + " x" + (taskAmount + 1) + ", which is itemamount: " + adjustedItemAmount + ", rarity: " + rarity;
                craftedMsgTimes[taskID] = taskAmount;
            }
            else
            {
                craftedMsgTimes[taskID]++;
                craftStr = player.displayName + " has crafted " + itemName + " x" + itemAmount + " (taskAmount: " + taskAmount + ", taskCrafted: " + taskCrafted + ") (Rarity: " + rarity + "), adjust: " + adjustedItemAmount;
            }
            LogToFile("craft_log", craftStr, this, true);
        }

        private readonly Dictionary<ItemId, int> lastItemPosition = new();

        private BasePlayer FindLooterFromCrate(BaseEntity crate)
        {
            if (crate == null) return null;

            var looters = Pool.GetList<BasePlayer>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || player.IsDead() || !player.IsConnected) continue;

                    var lootSource = player?.inventory?.loot?.entitySource ?? null;
                    if (lootSource != null && lootSource == crate) looters.Add(player);
                }

                return looters.Count == 1 ? looters[0] : null;
            }
            finally { Pool.FreeList(ref looters); }
        }

        private bool HasAnyLooters(BaseEntity crate)
        {
            if (crate == null) return false;

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || player.IsDead() || !player.IsConnected) continue;

                var lootSource = player?.inventory?.loot?.entitySource ?? null;
                if (lootSource == crate) return true;
            }

            return false;
        }

        private bool IsOnGround(BaseEntity entity)
        {
            if (entity == null || (entity?.IsDestroyed ?? true)) return false;
            var ray = new Ray(entity?.transform?.position ?? Vector3.zero, Vector3.down);
            if (UnityEngine.Physics.SphereCast(ray, 1f, out RaycastHit hit, 0.75f, groundWorldConstLayer)) if (hit.GetEntity() != null || hit.GetCollider() != null) return true;
            return false;
        }

        private float DistanceFromGround(BaseEntity entity, float maxDistanceToCheck = 300f, float heightAdjust = 0.5f)
        {
            if (entity == null || entity.IsDestroyed || entity.gameObject == null || entity.transform == null) return -1f;
            var pos = entity?.transform?.position ?? Vector3.zero;
            pos = new Vector3(pos.x, pos.y + heightAdjust, pos.z);
            if (UnityEngine.Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit info, maxDistanceToCheck))
            {
                if (info.collider == null && info.GetCollider() == null) return -1f;
                return Vector3.Distance(pos, info.point);
            }
            return -1f;
        }

        private bool OnGround(BaseEntity entity)
        {
            if (entity == null || (entity?.IsDestroyed ?? true) || entity?.transform == null) return false;
            var ray = new Ray(entity.transform.position, Vector3.down);
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 0.5f, groundWorldLayer)) return true; //checks only ground
            if (UnityEngine.Physics.Raycast(ray, out hitt, 0.5f, groundWorldConstLayer)) return true; //checks world AND construction/deployable
            var ray2 = new Ray(entity?.CenterPoint() ?? Vector3.zero, Vector3.down);




            //      if (UnityEngine.Physics.Raycast(ray, out hitt, 0.5f, constructionColl)) return true; //redundant^
            if (UnityEngine.Physics.Raycast(ray, out hitt, 0.5f, c4Coll)) return true;
            if (UnityEngine.Physics.Raycast(ray, out hitt, 0.5f, flamerColl)) return true;
            if (UnityEngine.Physics.Raycast(ray2, out hitt, 0.7f, groundWorldConstLayer)) return true;
            if (UnityEngine.Physics.Raycast(ray2, out hitt, 0.7f, c4Coll)) return true;
            if (UnityEngine.Physics.Raycast(ray2, out hitt, 0.7f, flamerColl)) return true;

            return false;
        }

        private bool HasPerms(string userId, string perm) { return permission.UserHasPermission(userId, perm); }

        private bool HasPerms(BasePlayer player, string perm) { return HasPerms(player?.UserIDString ?? string.Empty, perm); }

        private bool HasPerms(IPlayer player, string perm) { return HasPerms(player?.Id ?? string.Empty, perm); }

        private readonly Dictionary<string, float> lastGRules = new();
        [ChatCommand("grules")]
        private void GrulesCMD(BasePlayer player, string command, string[] args)
        {
            if (serverType == ServerType.x3)
            {
                SendReply(player, "This is a hardcore server. Even if your group is larger, it won't be easy. There's no hard limit on groups.");
                //SendReply(player, "Group rules are pretty simple here: Be a solo or play with a friend.\nDon't make things difficult! :)");
                return;
            }
            var now = UnityEngine.Time.realtimeSinceStartup;
            if (!lastGRules.TryGetValue(player.UserIDString, out float last) || (now - last) > 5)
            {
                SendGRules(player);
                lastGRules[player.UserIDString] = now;
            }
            else SendReply(player, "You're running this command too quickly!");
        }

        private void cmdSteamID(IPlayer player, string command, string[] args)
        {
            if (!player.IsServer && !permission.UserHasPermission(player.Id, Name + ".steamid"))
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }
            if (player?.Object != null)
            {
                var centerPoint = (player?.Object as BasePlayer)?.CenterPoint() ?? Vector3.zero;
                if (args.Length <= 0)
                {
                    SendReply(player, "No arguments supplied, showing players within a 30m radius...");
                    var nearSB = new StringBuilder();
                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var targetBP = BasePlayer.activePlayerList[i];
                        if (targetBP == null || targetBP.UserIDString == player.Id) continue;
                        var dist = Vector3.Distance(targetBP?.CenterPoint() ?? Vector3.zero, centerPoint);
                        if (dist <= 30)
                        {
                            nearSB.Append(targetBP.displayName).Append("(").Append(targetBP.UserIDString).Append(", ").Append(getIPString(targetBP)).Append(")");
                            if (IsBanned(targetBP.UserIDString)) nearSB.Append(" (<color=red>Banned</color>)");
                            nearSB.Append("\n");
                        }
                    }
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var targetBP = BasePlayer.sleepingPlayerList[i];
                        if (targetBP == null || targetBP.UserIDString == player.Id) continue;
                        var dist = Vector3.Distance(targetBP?.CenterPoint() ?? Vector3.zero, centerPoint);
                        if (dist <= 30)
                        {
                            nearSB.Append(targetBP.displayName).Append("(").Append(targetBP.UserIDString).Append(", ").Append(getIPString(targetBP)).Append(")");
                            if (IsBanned(targetBP.UserIDString)) nearSB.Append(" (<color=red>Banned</color>)");
                            nearSB.Append("\n");
                        }
                    }
                    if (nearSB.Length > 0) SendReply(player, nearSB.ToString().TrimEnd());
                    else SendReply(player, "No players found within 30m.");
                    return;
                }
            }
            if (args.Length > 1 && args[0].ToLower() == "ip")
            {
                var IPtoFind = args[1];
                if (string.IsNullOrEmpty(IPtoFind))
                {
                    SendReply(player, "Specify an IP!");
                    return;
                }

                var IPSB = new StringBuilder("Found following IPs matching " + IPtoFind);
                var ipSBs = Pool.GetList<StringBuilder>();
                foreach (var cPlayer in covalence.Players.All)
                {
                    var playerIP = getIPString(cPlayer);
                    if (string.IsNullOrEmpty(playerIP)) continue;
                    if (playerIP.IndexOf(IPtoFind, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (IPSB.Length >= 896)
                        {
                            ipSBs.Add(IPSB);
                            IPSB = new StringBuilder();
                        }
                        IPSB.AppendLine((cPlayer?.Name ?? "Unknown") + ": " + playerIP + (IsBanned(cPlayer.Id) ? " (<color=red>Banned</color>)" : ""));
                    }
                }
                if (ipSBs.Count > 0) for (int i = 0; i < ipSBs.Count; i++) SendReply(player, ipSBs[i].ToString().TrimEnd());
                if (IPSB.Length > 0) SendReply(player, IPSB.ToString().TrimEnd());
                if (IPSB.Length < 1 && ipSBs.Count < 1) SendReply(player, "No IPs found matching this!");
                Pool.FreeList(ref ipSBs);
                return;
            }
            if (args.Length > 1 && args[0].ToLower() == "all")
            {
                var targets = FindIPlayers(args[1]);
                if (targets == null || !targets.Any())
                {
                    SendReply(player, "Failed to find any targets with the name/id/ip: " + args[1]);
                    return;
                }
                var targetsSB = new StringBuilder();
                var finishList = Pool.GetList<StringBuilder>();
                foreach (var targ in targets)
                {
                    var IP = getIPString(targ);
                    if (targetsSB.Length >= 896)
                    {
                        finishList.Add(targetsSB);
                        targetsSB = new StringBuilder();
                    }
                    targetsSB.Append(targ.Name + ": " + targ.Id + ": " + IP);
                    if (IsBanned(targ.Id)) targetsSB.Append(" (<color=red>Banned</color>)");
                    targetsSB.AppendLine("\n"); //add extra newline because spacing is much easier to read
                }
                SendReply(player, "Found players:");
                if (finishList.Count > 0)
                {
                    for (int i = 0; i < finishList.Count; i++)
                    {
                        var sb = finishList[i];
                        SendReply(player, sb.ToString().TrimEnd());
                    }
                }
                SendReply(player, targetsSB.ToString().TrimEnd());
                Pool.FreeList(ref finishList);
                //SendReply(player, "Found players:\n" + targetsSB.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
                return;
            }

            if (args.Length <= 0)
            {
                SendReply(player, "No args provided");
                return;
            }

            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "Failed to find a player with the name/id/ip: " + args[0]);
                return;
            }
            SendReply(player, target.displayName + "'s ID is: " + target.UserIDString + ", IP: " + getIPString(target) + " (Banned: " + IsBanned(target.UserIDString) + ")");
        }

        private List<string> GetAliases(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return null;
            return Known?.Call<List<string>>("GetAliases", userID) ?? null;
        }

        [ChatCommand("alias")]
        private void cmdAlias(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, Name + ".alias"))
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }
            if (args.Length <= 0)
            {
                SendReply(player, "You must supply a target!");
                return;
            }
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                SendReply(player, "Failed to find a player with the name: " + args[0]);
                return;
            }
            var aliases = GetAliases(target.Id);
            if (IsNullOrEmpty(aliases))
            {
                SendReply(player, target.Name + " has no known aliases!");
                return;
            }

            var nameSB = new StringBuilder();
            for (int i = 0; i < aliases.Count; i++) nameSB.AppendLine(RemoveTags(aliases[i]));

            SendReply(player, "Aliases for " + target.Name + " (" + target.Id + "):\n" + nameSB.ToString().TrimEnd());
        }

        private object OnTrapTrigger(BaseTrap trap, GameObject go)
        {
            if (trap == null || go == null) return null;
            var ply = go?.ToBaseEntity() as BasePlayer;
            if (ply != null && isVanished(ply)) return false;
            return null;
        }

        [ChatCommand("repair")]
        private void cmdRepItem(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var heldItem = player?.GetActiveItem() ?? null;
            if (heldItem == null)
            {
                SendReply(player, "Unable to find an item in your hands!");
                return;
            }
            if (heldItem.condition == heldItem.maxCondition)
            {
                SendReply(player, "This item is already fully healed!");
                return;
            }
            heldItem.RepairCondition(heldItem.maxCondition);
            var repairFX = new Effect("assets/bundled/prefabs/fx/repairbench/itemrepair.prefab", player.CenterPoint(), Vector3.zero);
            EffectNetwork.Send(repairFX, player?.net?.connection);
            //      Effect.server.Run("assets/bundled/prefabs/fx/repairbench/itemrepair.prefab", player.transform.position, player.transform.position, null, true);
            player.inventory.ServerUpdate(0.01f);
        }

        [ChatCommand("water")]
        private void cmdWater(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var bucket = player?.GetHeldEntity()?.GetComponent<BaseLiquidVessel>() ?? null;
            if (bucket == null)
            {
                SendReply(player, "You do not appear to holding a water container!");
                return;
            }
            bucket.AddLiquid(ItemManager.FindItemDefinition("water"), 4000);
            SendReply(player, "+4000 water!");
        }

        private readonly FieldInfo flamerTick = typeof(FlameThrower).GetField("tickRate", BindingFlags.Instance | BindingFlags.NonPublic);

        [ChatCommand("wetflamer")]
        private void cmdWatergun(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var flamerEntity = player?.GetHeldEntity()?.GetComponent<FlameThrower>() ?? null;
            if (flamerEntity == null)
            {
                SendReply(player, "You do not appear to be holding a flamethrower!");
                return;
            }

            flamerTick.SetValue(flamerEntity, 0.01f);
            flamerEntity.flameRange = 30f;
            flamerEntity.flameRadius = 0.01f;
            foreach (var dmg in flamerEntity.damagePerSec)
            {
                dmg.type = Rust.DamageType.Generic;
                dmg.amount = 0;
            }
            flamerEntity.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            SendReply(player, "wet flamer");

        }


        [ChatCommand("portal")]
        private void cmdPortal(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (player?.IsDead() ?? true)
            {
                SendReply(player, "You must be alive to use this command!");
                return;
            }
            var currentRot = GetPlayerRotation(player);
            Ray ray = new(player?.eyes?.position ?? Vector3.zero, currentRot);


            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 1440f)) TeleportPlayer(player, hitt.point, true, false);
            else SendReply(player, "Target out of range!");
        }


        [ConsoleCommand("teleport.randomobject")]
        private void consoleTPRB(ConsoleSystem.Arg arg)
        {
            if (arg == null || arg.Connection == null) return;
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin) return;
            if (player == null)
            {
                arg.ReplyWith("You must be a player to use this command!");
                return;
            }
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1)
            {
                arg.ReplyWith("You must specify a name/ID/ip!");
                return;
            }

          

            if (!ulong.TryParse(args[0], out ulong uID))
            {
                var findPlayer = FindConnectedPlayer(args[0], true);
                if (findPlayer == null)
                {
                    arg.ReplyWith("Failed to find a player with: " + args[0]);
                    return;
                }
                else uID = ulong.Parse(findPlayer.Id);
            }
         

            var playerEnts = Pool.GetList<BaseEntity>();

            try
            {
                foreach (var ent in BaseEntity.saveList)
                    if (ent?.OwnerID == uID) 
                        playerEnts.Add(ent);
                

                if (playerEnts.Count > 0)
                {
                    var getEnt = playerEnts[UnityEngine.Random.Range(0, playerEnts.Count)];
                    var pos = getEnt?.transform?.position ?? Vector3.zero;
                    if (getEnt == null || pos == Vector3.zero) arg.ReplyWith("Got bad position!");
                    else TeleportPlayer(player, pos, true, false);
                }
                else arg.ReplyWith("No objects found from player: " + args[0]);
            }
            finally
            {
                Pool.FreeList(ref playerEnts);
            }

       

        }

        [ConsoleCommand("items.print")]
        private void consoleItemsPrint(ConsoleSystem.Arg arg)
        {
            var sb = Pool.Get<StringBuilder>();

            try
            {
                sb.Clear();

                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];

                    sb.Append(i).Append(": ").Append(item.displayName.english).Append(" (").Append(item.shortname).Append(") (").Append(item.itemid).Append(")\n");
                }


                if (sb.Length > 1)
                    sb.Length -= 1;

                arg.ReplyWith(sb.ToString());
            }
            finally { Pool.Free(ref sb); }

        }

        [ConsoleCommand("skins.print")]
        private void consoleSkinsPrint(ConsoleSystem.Arg arg)
        {
            var sb = Pool.Get<StringBuilder>();

            try
            {
                sb.Clear();

                var workshopSkins = SkinsAPI?.Call<List<ApprovedSkinInfo>>("GetWorkshopSkins") ?? null;

                if (workshopSkins?.Count > 0)
                {
                    for (int i = 0; i < workshopSkins.Count; i++)
                    {
                        var skin = workshopSkins[i];

                        sb.Append(skin.Name).Append(" (").Append(skin.Desc).Append(", ").Append(skin.WorkshopdId).Append(")").Append(Environment.NewLine);

                    }
                }


                if (sb.Length > 1)
                    sb.Length -= 1;

                arg.ReplyWith(sb.ToString());
            }
            finally { Pool.Free(ref sb); }

        }


        [ConsoleCommand("permissions.count")]
        private void consolePermCount(ConsoleSystem.Arg arg)
        {
            if (arg == null) return;
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin) return;
            var args = arg.Args;
            if (args == null || args.Length < 1)
            {
                SendReply(arg, "You need to specify an argument name");
                return;
            }
            if (!permission.PermissionExists(args[0]))
            {
                SendReply(arg, "Permission doesn't exist: " + args[0]);
                return;
            }

            var count = covalence.Players.All.Count(p => p.HasPermission(args[0]));
            SendReply(arg, count.ToString("N0") + " users with permission '" + args[0] + "'");
        }

        [ConsoleCommand("player.say")]
        private void consolePlayerSay(ConsoleSystem.Arg arg)
        {
            if (arg == null || arg.Connection != null) return;
            if (arg.Args == null || arg.Args.Length < 2)
            {
                SendReply(arg, "Args null or args length < 2");
                return;
            }
            var args = arg.Args;
            var userId = arg.Args[0];
            var ply = covalence.Players?.FindPlayerById(userId) ?? null;
            if (ply == null)
            {
                SendReply(arg, "No player with ID: " + userId);
                return;
            }
            BetterChat?.Call("SendChat", ply, string.Join(" ", args.Skip(1)));
        }

        [ConsoleCommand("removefromqueue")]
        private void KickFromQueueCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            if (arg?.Args == null || arg?.Args.Length < 1)
            {
                arg.ReplyWith("You did not provide a SteamID!");
                return;
            }

            var ID_string = arg.Args[0];
            if (!ulong.TryParse(ID_string, out ulong ID))
            {
                arg.ReplyWith(ID_string + " is not a valid SteamID!");
                return;
            }

            foreach (var connection in SingletonComponent<ServerMgr>.Instance.connectionQueue.joining.ToList())
            {
                if (connection.userid == ID)
                {
                    SingletonComponent<ServerMgr>.Instance.connectionQueue.RemoveConnection(connection);
                    arg.ReplyWith("Removed " + ID + " from queue");
                    return;
                }
            }

            arg.ReplyWith("User with ID \"" + ID + "\" was not found in connection queue!");
        }

        [ConsoleCommand("connectionqueue")]
        private void ConnectionQueueCMD(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;

            var JoiningConnections = SingletonComponent<ServerMgr>.Instance.connectionQueue.joining;
            string Reply = "There is currenty " + JoiningConnections.Count + " player(s) joining";

            var FinalList = new List<string>();
            var ListReply = "";

            foreach (var connection in JoiningConnections) FinalList.Add((connection?.userid ?? 0) + "/" + (connection?.username ?? "Unknown name") + "/" + (connection?.ipaddress ?? "Unknown IP"));
            if (FinalList.Count > 0) ListReply = "\n" + string.Join("\n", FinalList);
            arg.ReplyWith(Reply + ListReply);
        }

        [ConsoleCommand("puzzle.resetall")]
        private void consoleResetPuzzles(ConsoleSystem.Arg arg)
        {
            if (arg == null) return;
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin) return;
            ResetPuzzles();
            SendReply(arg, "Reset all puzzles!");
        }

        [ConsoleCommand("restart.forcestop")]
        private void consoleRestartStop(ConsoleSystem.Arg arg)
        {
            if (arg == null || arg.Connection != null) return;
            if (ServerMgr.Instance == null)
            {
                SendReply(arg, "No ServerMgr instance!");
                return;
            }
            restartCoroutine.SetValue(ServerMgr.Instance, null);
        }



        private ulong GetMajorityOwner(uint buildingId)
        {
            if (buildingId == 0) return 0;
            var blocks = GetBlocksFromID(buildingId);
            if (blocks == null || blocks.Count < 1) return 0;
            var ownCount = new Dictionary<ulong, int>();
            foreach (var block in blocks)
            {
                if (!ownCount.TryGetValue(block.OwnerID, out int count)) ownCount[block.OwnerID] = 1;
                else ownCount[block.OwnerID]++;
            }
            return (ownCount == null || ownCount.Count < 1) ? 0 : ownCount?.Where(p => p.Value == ownCount.Values.Max())?.FirstOrDefault().Key ?? 0;
        }

        [ChatCommand("tplb2")]
        private void cmdTplb2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var blocks = BaseEntity.saveList?.Where(p => p is BuildingBlock)?.Select(p => p as BuildingBlock)?.ToList() ?? null;
            if (blocks == null || blocks.Count < 1) return;
            SendReply(player, blocks.Count.ToString("N0") + " total building blocks.");
            Dictionary<uint, int> blockCounts = new();
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (block == null || block.buildingID == 0) continue;
                if (!blockCounts.TryGetValue(block.buildingID, out int count)) blockCounts[block.buildingID] = 1;
                else blockCounts[block.buildingID]++;
            }
            var takeAmount = (blockCounts?.Count ?? 0) >= 40 ? 40 : blockCounts?.Count ?? 0;
            var topTake = (from entry in blockCounts orderby entry.Value descending select entry).Take(takeAmount).OrderBy(p => GetLastConnection(GetMajorityOwner(p.Key)).Ticks);
            var topSB = new StringBuilder();
            var totalJunkEnts = 0;
            foreach (var top in topTake)
            {
                topSB.AppendLine(top.Key + ": " + top.Value);
                totalJunkEnts += top.Value;
            }
            SendReply(player, topSB.ToString().TrimEnd());

        }


        [ChatCommand("tplb")]
        private void cmdTplb(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var blocks = BaseEntity.saveList?.Where(p => p is BuildingBlock)?.Select(p => p as BuildingBlock)?.ToList() ?? null;
            if (blocks == null || blocks.Count < 1) return;
            SendReply(player, blocks.Count.ToString("N0") + " total building blocks.");
            Dictionary<uint, int> blockCounts = new();
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (block == null || block.buildingID == 0) continue;
                if (!blockCounts.TryGetValue(block.buildingID, out int count)) blockCounts[block.buildingID] = 1;
                else blockCounts[block.buildingID]++;
            }
            var takeAmount = (blockCounts?.Count ?? 0) >= 20 ? 20 : blockCounts?.Count ?? 0;
            var topTake = (from entry in blockCounts orderby entry.Value descending select entry).Take(takeAmount);
            var topSB = new StringBuilder();
            var totalJunkEnts = 0;
            foreach (var top in topTake)
            {
                topSB.AppendLine(top.Key + ": " + top.Value);
                totalJunkEnts += top.Value;
            }
            //  var maxVal = blockCounts?.Values?.Max() ?? 0;
            //   SendReply(player, "Biggest structure contains: " + maxVal.ToString("N0") + " blocks");
            //  var maxKvp = blockCounts?.Where(p => p.Value == maxVal).FirstOrDefault();
            SendReply(player, topSB.ToString().TrimEnd());

        }

        [ChatCommand("tpbid")]
        private void cmdTpBid(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "Supply building ID!");
                return;
            }
            if (!uint.TryParse(args[0], out uint blockId))
            {
                SendReply(player, "Not a uint: " + args[0]);
                return;
            }

            var building = BuildingManager.server.GetBuilding(blockId);
            if (building == null)
            {
                SendReply(player, "Building is null for " + blockId);
                return;
            }

            var findEnt = building?.decayEntities?.Where(p => p?.buildingID == blockId)?.ToList()?.GetRandom() ?? null;
            if (findEnt == null)
            {
                SendReply(player, "No block found with that ID!");
                return;
            }
            TeleportPlayer(player, findEnt?.transform?.position ?? Vector3.zero);
        }

        [ConsoleCommand("junk.count")]
        private void consoleJunkCount(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var junkDict = new Dictionary<string, int>();
            foreach (var ent in BaseNetworkable.serverEntities)
            {
                var prefabName = ent?.ShortPrefabName ?? string.Empty;
                if (!string.IsNullOrEmpty(prefabName))
                {
                    if (!junkDict.TryGetValue(prefabName, out int outAmount)) junkDict[prefabName] = 1;
                    else junkDict[prefabName]++;
                }
            }
            var takeAmount = (junkDict?.Count ?? 0) >= 20 ? 20 : junkDict?.Count ?? 0;
            if (arg?.Args != null && arg.Args.Length > 0 && !int.TryParse(arg.Args[0], out takeAmount))
            {
                SendReply(arg, "Not an int: " + arg.Args[0]);
                return;
            }
            var topTake = (from entry in junkDict orderby entry.Value descending select entry).Take(takeAmount);
            var topSB = new StringBuilder();
            var totalJunkEnts = 0;
            foreach (var top in topTake)
            {
                topSB.AppendLine(top.Key + ": " + top.Value.ToString("N0"));
                totalJunkEnts += top.Value;
            }
            arg.ReplyWith(topSB.ToString().TrimEnd() + "\nCombined entity count from above: " + totalJunkEnts.ToString("N0"));

        }

        [ConsoleCommand("ores.count")]
        private void consoleOreCount(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var junkDict = new Dictionary<string, int>();
            foreach (var ent in BaseNetworkable.serverEntities)
            {
                var oreEnt = ent as OreResourceEntity;
                if (oreEnt != null)
                {
                    var prefabName = ent?.ShortPrefabName ?? string.Empty;
                    if (!string.IsNullOrEmpty(prefabName))
                    {
                        if (!junkDict.TryGetValue(prefabName, out int outAmount)) junkDict[prefabName] = 1;
                        else junkDict[prefabName]++;
                    }
                }

            }
            var takeAmount = (junkDict?.Count ?? 0) >= 20 ? 20 : junkDict?.Count ?? 0;
            if (arg?.Args != null && arg.Args.Length > 0 && !int.TryParse(arg.Args[0], out takeAmount))
            {
                SendReply(arg, "Not an int: " + arg.Args[0]);
                return;
            }
            var topTake = (from entry in junkDict orderby entry.Value descending select entry).Take(takeAmount);
            var topSB = new StringBuilder();
            var totalJunkEnts = 0;
            foreach (var top in topTake)
            {
                topSB.AppendLine(top.Key + ": " + top.Value.ToString("N0"));
                totalJunkEnts += top.Value;
            }
            arg.ReplyWith(topSB.ToString().TrimEnd() + "\nCombined entity count from above: " + totalJunkEnts.ToString("N0"));

        }



        [ConsoleCommand("prefab.count")]
        private void consolePrefabCount(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            if (arg.Args == null || arg.Args.Length < 1) return;
            var arg0Lower = arg.Args[0].ToLower();
            var junkDict = new Dictionary<string, int>();
            foreach (var ent in BaseNetworkable.serverEntities)
            {
                var prefabName = ent?.ShortPrefabName ?? string.Empty;
                if (arg0Lower == prefabName)
                {
                    if (!junkDict.TryGetValue(prefabName, out int outAmount)) junkDict[prefabName] = 1;
                    else junkDict[prefabName]++;
                }
            }
            var takeAmount = (junkDict?.Count ?? 0) >= 20 ? 20 : junkDict?.Count ?? 0;
            var topTake = (from entry in junkDict orderby entry.Value descending select entry).Take(takeAmount);
            var topSB = new StringBuilder();
            var totalJunkEnts = 0;
            foreach (var top in topTake)
            {
                topSB.AppendLine(top.Key + ": " + top.Value);
                totalJunkEnts += top.Value;
            }
            arg.ReplyWith(topSB.ToString().TrimEnd() + "\nCombined entity count from above: " + totalJunkEnts);

        }

        [ConsoleCommand("bps.resetall")]
        private void consoleBPReset(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;

            var watch = new Stopwatch();
            watch.Start();

            var userIds = Pool.Get<HashSet<ulong>>();

            try
            {
                PlayersByDatabase?.Call("GetAllPlayerIDsNoAllocUH", userIds);

                PrintWarning("userIds length: " + userIds.Count);

                var c = 0;


                foreach (var id in userIds)
                {

                    var pInfo = ServerMgr.Instance.persistance.GetPlayerInfo(id);
                    if (pInfo == null) continue;

                    pInfo.unlockedItems = new List<int>();


                    ServerMgr.Instance.persistance.SetPlayerInfo(id, pInfo);

                    var pObj = RelationshipManager.FindByID(id);

                    if (pObj != null && pObj.IsConnected)
                    {
                        pObj.blueprints.Reset();
                        pObj.SendNetworkUpdate();
                    }

                    c++; //this is C#, fool!
                }

                watch.Stop();
                PrintWarning("Took: " + watch.Elapsed.TotalMilliseconds + "ms for blueprint resetting. Reset: " + c.ToString("N0") + " ids");

            }
            finally { Pool.Free(ref userIds); }





        }

        [ConsoleCommand("bps.saveall")]
        private void consoleBPSave(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var watch = new Stopwatch();
            watch.Start();
            var srvMgr = SingletonComponent<ServerMgr>.Instance;
            foreach (var ply in covalence.Players.All)
            {
                if (ply == null) continue;
                if (!ulong.TryParse(ply.Id, out ulong userID)) continue;
                var lastCon = Playtimes?.Call<DateTime>("GetLastConnection", ply.Id) ?? DateTime.MinValue;
                if (lastCon <= DateTime.MinValue) continue;
                if ((DateTime.Now - lastCon).TotalDays >= 30) continue;
                var bps = srvMgr?.persistance?.GetPlayerInfo(userID)?.unlockedItems ?? null;
                if (bps == null || bps.Count < 1) continue;
                var bpInfo = bpData.HasBPInfo(userID) ? bpData.GetBPInfo(userID) : bpData.AddBPInfo(userID);
                if (bpInfo == null) continue;
                for (int i = 0; i < bps.Count; i++)
                {
                    var item = bps[i];
                    if (!bpInfo.IsUnlocked(item)) bpInfo.Unlock(item);
                }
            }
            SaveBPData();
            watch.Stop();
            PrintWarning("Took: " + watch.Elapsed.TotalMilliseconds + "ms for blueprint saving");
        }

        [ConsoleCommand("bps.resetplayer")]
        private void consoleBPResetPlayer(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;

            // var watch = Stopwatch.StartNew();
            if (arg.Args == null || arg.Args.Length < 1)
            {
                SendReply(arg, "Please supply ID to reset.");
                return;
            }

            var args = arg.Args;
            if (!ulong.TryParse(args[0], out ulong sourceID))
            {
                SendReply(arg, "Arg: " + args[0] + " is not a ulong!");
                return;
            }

            var persist = ServerMgr.Instance?.persistance ?? null;
            if (persist == null)
            {
                SendReply(arg, "Server has no persistance data!!");
                return;
            }

            var sourceInfo = persist?.GetPlayerInfo(sourceID) ?? null;
            if (sourceInfo == null)
            {
                SendReply(arg, "Source " + sourceID + " has no player info!");
                return;
            }

            sourceInfo.unlockedItems = new List<int>();

            SendReply(arg, "created new list");

            persist.SetPlayerInfo(sourceID, sourceInfo);

            SendReply(arg, "Set player info");

            var pObj = BasePlayer.FindByID(sourceID);
            if (pObj != null)
            {
                pObj.blueprints.Reset();

                if (pObj.IsConnected)
                {
                    SendReply(pObj, "An admin has reset your BPs");
                    pObj.SendNetworkUpdateImmediate();
                }

            }

            // watch.Stop();
            SendReply(arg, "Reset BPs for: " + sourceID);
            //   PrintWarning("Took: " + watch.Elapsed.TotalMilliseconds + "ms for blueprint transfer");
        }

        [ConsoleCommand("bps.transfer2")]
        private void consoleBPTransfer2(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var watch = Stopwatch.StartNew();
            if (arg.Args == null || arg.Args.Length < 2)
            {
                SendReply(arg, "Please supply IDs to transfer from and to.");
                return;
            }
            var args = arg.Args;
            if (!ulong.TryParse(args[0], out ulong sourceID))
            {
                SendReply(arg, "Arg: " + args[0] + " is not a ulong!");
                return;
            }

            if (!ulong.TryParse(args[1], out ulong targetID))
            {
                SendReply(arg, "Arg: " + args[1] + " is not a ulong!");
                return;
            }
            var persist = ServerMgr.Instance?.persistance ?? null;
            if (persist == null)
            {
                SendReply(arg, "Server has no persistance data!!");
                return;
            }
            var sourceInfo = persist?.GetPlayerInfo(sourceID) ?? null;
            if (sourceInfo == null)
            {
                SendReply(arg, "Source " + sourceID + " has no player info!");
                return;
            }
            var targetInfo = persist?.GetPlayerInfo(targetID) ?? null;
            if (targetInfo == null)
            {
                SendReply(arg, "Target " + targetID + " has no player info!");
                return;
            }
            targetInfo.unlockedItems.AddRange(sourceInfo.unlockedItems);
            persist.SetPlayerInfo(targetID, targetInfo);

            var pObj = BasePlayer.FindByID(targetID);
            if (pObj != null && pObj.IsConnected) pObj.SendNetworkUpdateImmediate();


            watch.Stop();
            PrintWarning("Took: " + watch.Elapsed.TotalMilliseconds + "ms for blueprint transfer");
        }

        [ConsoleCommand("bps.reload")]
        private void consoleBPReload(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            ServerMgr.Instance.persistance = new UserPersistance(ConVar.Server.rootFolder);
            SendReply(arg, "Reloaded blueprints file from disk");
        }

        [ConsoleCommand("oxide.purge")]
        private void consoleOxidePurge(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var oldCount = covalence.Players.All.Count();
            var watch = Stopwatch.StartNew();
            PrintWarning("Beginning oxide data clean up");
            watch.Stop();
            var now = DateTime.UtcNow;
            var purgeCount = 0;
            var rustPly = covalence.Players as RustPlayerManager;
            if (rustPly == null)
            {
                PrintWarning("rustPly is null!");
                return;
            }
            var allPlayersDic = GetValue(rustPly, "allPlayers") as IDictionary<string, RustPlayer>;
            if (allPlayersDic == null || allPlayersDic.Count < 1)
            {
                PrintWarning("allPlayersDic is null/empty!!");
                return;
            }
            var dic = new Dictionary<string, RustPlayer>() as IDictionary<string, RustPlayer>;
            foreach (var kvp in allPlayersDic)
            {
                var p = kvp.Value;
                if (p == null || p.IsConnected) continue;
                var lastCon = GetLastConnectionS(p.Id);
                if (lastCon == DateTime.MinValue || (now - lastCon).TotalDays < 90)
                {
                    purgeCount++;
                    continue;
                }
                dic[p.Id] = p;
            }
            if (dic == null || dic.Count < 1)
            {
                PrintWarning("dic is null/empty!");
                return;
            }

            SetValue(covalence.Players, "allPlayers", dic);

            PrintWarning("Oxide data clean up (" + purgeCount.ToString("N0") + " purged, old total: " + oldCount.ToString("N0") + ") took: " + watch.Elapsed.TotalMilliseconds + "ms");
        }

        [ConsoleCommand("bps.fix")]
        private void consoleBPFix(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var dir = ConVar.Server.rootFolder + @"\bpfix";
            var persist = new UserPersistance(ConVar.Server.rootFolder);


            var needRevert = new HashSet<ulong>();
            var needSB = new StringBuilder();
            var now = DateTime.UtcNow;
            foreach (var p in covalence.Players.All)
            {
                if (p == null) continue;

                if (!ulong.TryParse(p.Id, out ulong userId)) continue;
                var lastCon = GetLastConnectionS(p.Id);
                if (lastCon.Month != now.Month || lastCon.Day < now.Day) continue;
                var info = persist?.GetPlayerInfo(userId);
                if (info?.unlockedItems == null) continue;
                if (info.unlockedItems.Count >= allBlueprints.Count)
                {
                    needSB.AppendLine(userId + " (" + GetDisplayNameFromID(userId) + ") needs revert, unlockedItems count: " + info.unlockedItems.Count + ", all BPs count: " + allBlueprints.Count);
                    needRevert.Add(userId);
                }
            }
            var properUnlocked = new Dictionary<ulong, List<int>>();
            UserPersistance newPers;
            try { newPers = new UserPersistance(dir); } //MUST LOAD ORIGINAL BLUEPRINTS AFTER!!!
            catch (Exception ex)
            {
                PrintError("FAILED TO LOAD BLUEPRINTS FROM DIRECTORY: " + dir + Environment.NewLine + ex.ToString());
                try
                {
                    ServerMgr.Instance.persistance = new UserPersistance(ConVar.Server.rootFolder);
                    PrintWarning("Reloaded original blueprints successfully after error");
                }
                catch (Exception ex2) { PrintError("COULDN'T RESTORE ORIGINAL BLUEPRINTS!!! " + ex2.ToString()); }
                return;
            }
            PrintWarning(@"loaded \bpfix persistance!");
            foreach (var p in needRevert)
            {
                if (properUnlocked.TryGetValue(p, out List<int> outUnlocked)) continue;
                var newPerList = newPers?.GetPlayerInfo(p)?.unlockedItems ?? null;
                if (newPerList == null) continue;
                properUnlocked[p] = newPerList.ToList();
            }
            var oldPers = new UserPersistance(ConVar.Server.rootFolder);
            PrintWarning("loaded rootFolder persistance!");
            var fixedCount = 0;
            foreach (var kvp in properUnlocked)
            {
                var info = oldPers.GetPlayerInfo(kvp.Key);
                if (info == null) continue;
                info.unlockedItems = kvp.Value;
                oldPers.SetPlayerInfo(kvp.Key, info);
                var p = BasePlayer.FindByID(kvp.Key);
                if (p != null && p.IsConnected) p.SendNetworkUpdate();
                fixedCount++;
            }
            ServerMgr.Instance.persistance = oldPers;
            PrintWarning("in need of revert: " + needRevert.Count.ToString("N0") + ", fixed: " + fixedCount);
        }

        [ConsoleCommand("account.transfer")]
        private void consoleAccountTRansfer(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var watch = Stopwatch.StartNew();
            if (arg.Args == null || arg.Args.Length < 2)
            {
                SendReply(arg, "Please supply IDs to transfer from and to.");
                return;
            }
            var args = arg.Args;
            if (!ulong.TryParse(args[0], out ulong sourceID))
            {
                SendReply(arg, "Arg: " + args[0] + " is not a ulong!");
                return;
            }

            if (!ulong.TryParse(args[1], out ulong targetID))
            {
                SendReply(arg, "Arg: " + args[1] + " is not a ulong!");
                return;
            }
            if (sourceID == targetID)
            {
                SendReply(arg, "You cannot transfer to the same ID!");
                return;
            }
            var sourceIDStr = sourceID.ToString();
            var targetIDStr = targetID.ToString();
            covalence.Server.Command("bps.transfer2 " + sourceID + " " + targetID);
            covalence.Server.Command("zlvl.transfer " + sourceID + " " + targetID);
            covalence.Server.Command("luck.transfer " + sourceID + " " + targetID);
            var sourcePerms = permission.GetUserPermissions(sourceIDStr);
            if (sourcePerms != null && sourcePerms.Length > 0)
            {
                for (int i = 0; i < sourcePerms.Length; i++)
                {
                    var perm = sourcePerms[i];
                    if (!string.IsNullOrEmpty(perm) && !permission.UserHasPermission(targetIDStr, perm)) permission.GrantUserPermission(targetIDStr, perm, null);
                }
            }
            var entCount = 0;
            var whiteListCount = 0;
            foreach (var entity in BaseEntity.saveList)
            {
                if (entity == null) continue;
                if (entity.OwnerID == sourceID)
                {
                    entity.OwnerID = targetID;
                    entCount++;
                }
                var turret = entity as AutoTurret;
                if (turret != null)
                {
                    var changed = false;

                    foreach (var ap in turret.authorizedPlayers)
                    {
                        if (ap.userid == sourceID)
                        {
                            ap.userid = targetID;
                            changed = true;
                            whiteListCount++;
                            break;
                        }
                    }

                    if (changed) turret.SendNetworkUpdate();
                }
                var priv = entity as BuildingPrivlidge;
                if (priv != null)
                {
                    var changed = false;

                    foreach (var ap in priv.authorizedPlayers)
                    {
                        if (ap.userid == sourceID)
                        {
                            ap.userid = targetID;
                            changed = true;
                            whiteListCount++;
                            break;
                        }
                    }

                    if (changed) priv.SendNetworkUpdate();
                }
                var codeLock = entity as CodeLock;
                if (codeLock != null)
                {
                    if (codeLock.whitelistPlayers.Contains(sourceID))
                    {
                        codeLock.whitelistPlayers.Remove(sourceID);
                        codeLock.whitelistPlayers.Add(targetID);
                        whiteListCount++;
                    }
                }
            }

            SendReply(arg, "Attempted to transfer blueprints, ZLevels, and permissions. Transferred " + entCount.ToString("N0") + " entities' ownership, whitelisted on " + whiteListCount.ToString("N0") + " entities");

        }

        [ConsoleCommand("bps.transfer")]
        private void consoleBPTransfer(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var watch = Stopwatch.StartNew();
            if (arg.Args == null || arg.Args.Length < 2)
            {
                SendReply(arg, "Please supply IDs to transfer from and to.");
                return;
            }
            var args = arg.Args;
            if (!ulong.TryParse(args[0], out ulong sourceID))
            {
                SendReply(arg, "Arg: " + args[0] + " is not a ulong!");
                return;
            }
            if (!ulong.TryParse(args[1], out ulong targetID))
            {
                SendReply(arg, "Arg: " + args[1] + " is not a ulong!");
                return;
            }
            var sourceInfo = bpData?.blueprints?.Where(p => p.UserID == sourceID)?.FirstOrDefault() ?? null;
            var targetInfo = bpData?.blueprints?.Where(p => p.UserID == targetID)?.FirstOrDefault() ?? null;
            if (targetInfo == null)
            {
                targetInfo = bpData.AddBPInfo(targetID);
                PrintWarning("Had to create BPInfo for targetID: " + targetID);
            }
            if (sourceInfo == null)
            {
                SendReply(arg, "sourceInfo is null!");
                return;
            }
            if (targetInfo == null)
            {
                SendReply(arg, "targetInfo is null!");
                return;
            }

            var newCount = 0;
            var bpSB = new StringBuilder();
            var targetPly = BasePlayer.FindByID(targetID) ?? BasePlayer.FindSleeping(targetID) ?? null;
            if (sourceInfo.unlocked != null && sourceInfo.unlocked.Count > 0)
            {
                for (int i = 0; i < sourceInfo.unlocked.Count; i++)
                {
                    var bp = sourceInfo.unlocked[i];
                    if (!targetInfo.IsUnlocked(bp))
                    {
                        targetInfo.Unlock(bp);
                        var def = ItemManager.FindItemDefinition(bp);
                        if (def != null)
                        {
                            if (targetPly?.blueprints != null) targetPly.blueprints.Unlock(def);
                            bpSB.AppendLine(def?.displayName?.english);
                        }
                        newCount++;
                    }
                }
            }
            if (targetPly != null && targetPly.IsConnected) targetPly.SendNetworkUpdate();
            watch.Stop();
            PrintWarning(bpSB.ToString().TrimEnd());
            SendReply(arg, "Unlocked " + newCount.ToString("N0") + " blueprints for target from source.");
            PrintWarning("Took: " + watch.Elapsed.TotalMilliseconds + "ms for blueprint transfer");
        }

        [ConsoleCommand("items.dump")]
        private void consoleItemsDump(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var dumpSB = new StringBuilder();
            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                dumpSB.AppendLine(item.displayName.english + " : " + item.shortname + " : " + item.itemid);
            }
            arg.ReplyWith(dumpSB.ToString().TrimEnd());
        }

        [ConsoleCommand("alias.all")]
        private void consoleAliases(ConsoleSystem.Arg arg)
        {
            if (arg == null) return;
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin) return;
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1)
            {
                arg.ReplyWith("You must supply a target!");
                return;
            }
            var targetName = string.Join(" ", args);
            var target = FindIPlayer(targetName);
            if (target == null)
            {
                arg.ReplyWith("Failed to find a player by the name/ID/IP: " + targetName);
                return;
            }
            var aliases = Known?.Call<List<string>>("GetAliases", target.Id) ?? null;
            if (IsNullOrEmpty(aliases))
            {
                arg.ReplyWith(target.Name + " has no known aliases!");
                return;
            }
            var nameSB = new StringBuilder();
            for (int i = 0; i < aliases.Count; i++)
            {
                var name = aliases[i];
                nameSB.AppendLine(name);
            }
            arg.ReplyWith("Aliases for " + target.Name + " (" + target.Id + "):\n" + nameSB.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }

        [ConsoleCommand("allplayers")]
        private void consoleListAll(ConsoleSystem.Arg arg)
        {
            var playerArg = arg?.Player() ?? null;
            if (playerArg != null && !playerArg.IsAdmin)
            {
                arg.ReplyWith("You do not have permission to use this command.");
                return;
            }
            var startTime = UnityEngine.Time.realtimeSinceStartup;
            var allPlayersString = string.Empty;
            var allPlayersSB = new StringBuilder();
            var countPlayers = 0;
            foreach (var player in covalence.Players.All)
            {
                var name = player?.Name ?? "Unknown";
                var uID = player?.Id ?? "0";
                var uIDName = name + " (" + uID + ")";
                if (!allPlayersString.Contains(name))
                {
                    allPlayersSB.Append(uIDName + " ");
                    countPlayers++;
                }
            }
            allPlayersString = allPlayersSB.ToString();
            var timeTaken = UnityEngine.Time.realtimeSinceStartup - startTime;
            arg.ReplyWith(allPlayersString);
            arg.ReplyWith("Total Users: " + countPlayers);
            arg.ReplyWith("Took: " + timeTaken + " to loop through " + countPlayers + " players and print as a string, string length: " + allPlayersString.Length);
        }


        [ConsoleCommand("c4c")]
        private void consoleC4Count(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var count = 0;
            var rocketcount = 0;
            var hvrocket = 0;
            var incrocket = 0;
            var wepcount = 0;
            var satCount = 0;
            var itemOwners = new Dictionary<string, Dictionary<string, int>>();
            //        var itemOwners = new Dictionary<string, string>();
            foreach (var entity in BaseEntity.saveList)
            {
                if (entity == null || entity.IsDestroyed) continue;
                var storage = entity?.GetComponent<StorageContainer>() ?? null;
                if (storage == null || storage.IsDestroyed || (storage?.inventory?.itemList == null)) continue;

                for (int i = 0; i < storage.inventory.itemList.Count; i++)
                {
                    var item = storage.inventory.itemList[i];
                    var name = item?.info?.shortname ?? string.Empty;
                    if (string.IsNullOrEmpty(name)) continue;

                    if (name == "explosive.timed" || name.Contains("ammo.rocket") || name.Contains("satchel"))
                    {
                        var owner = (storage?.OwnerID ?? 0).ToString();
                        var outDic = new Dictionary<string, int>();
                        if (!itemOwners.TryGetValue(owner, out outDic)) itemOwners[owner] = new Dictionary<string, int>();
                        if (!itemOwners[owner].TryGetValue(name, out int outVal)) itemOwners[owner][name] = item.amount;
                        else itemOwners[owner][name] += item.amount;
                    }

                    if (name == "explosive.timed") count += item.amount;
                    if (name == "ammo.rocket.basic") rocketcount += item.amount;
                    if (name == "ammo.rocket.hv") hvrocket += item.amount;
                    if (name == "ammo.rocket.fire") incrocket += item.amount;
                    if (name.Contains("satchel")) satCount++;

                    var weapon = item?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                    if (weapon != null && !name.Contains("bow") && !name.Contains("eoka")) wepcount++;
                }
            }
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || (player?.IsDead() ?? true)) continue;
                for (int j = 0; j < BasePlayer.activePlayerList[i].inventory.AllItems().Length; j++)
                {
                    var item = player.inventory.AllItems()[j];
                    if (item == null) continue;
                    var name = item?.info?.shortname ?? string.Empty;
                    if (string.IsNullOrEmpty(name)) continue;

                    if (name == "explosive.timed" || name.Contains("ammo.rocket") || name.Contains("satchel"))
                    {
                        var owner = player.UserIDString;
                        var outDic = new Dictionary<string, int>();
                        if (!itemOwners.TryGetValue(owner, out outDic)) itemOwners[owner] = new Dictionary<string, int>();
                        if (!itemOwners[owner].TryGetValue(name, out int outVal)) itemOwners[owner][name] = item.amount;
                        else itemOwners[owner][name] += item.amount;
                    }

                    if (name == "explosive.timed") count += item.amount;
                    if (name == "ammo.rocket.basic") rocketcount += item.amount;
                    if (name == "ammo.rocket.hv") hvrocket += item.amount;
                    if (name == "ammo.rocket.fire") incrocket += item.amount;
                    if (name.Contains("satchel")) satCount++;

                    var weapon = item?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                    if (weapon != null && !name.Contains("bow") && !name.Contains("eoka")) wepcount++;
                }
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var player = BasePlayer.sleepingPlayerList[i];
                if (player == null || (player?.IsDead() ?? true) || player.IsConnected) continue;
                for (int j = 0; j < player.inventory.AllItems().Length; j++)
                {
                    var item = BasePlayer.sleepingPlayerList[i].inventory.AllItems()[j];
                    if (item == null) continue;
                    var name = item?.info?.shortname ?? string.Empty;
                    if (string.IsNullOrEmpty(name)) continue;

                    if (name == "explosive.timed" || name.Contains("ammo.rocket") || name.Contains("satchel"))
                    {
                        var owner = player.UserIDString;
                        var outDic = new Dictionary<string, int>();
                        if (!itemOwners.TryGetValue(owner, out outDic)) itemOwners[owner] = new Dictionary<string, int>();
                        if (!itemOwners[owner].TryGetValue(name, out int outVal)) itemOwners[owner][name] = item.amount;
                        else itemOwners[owner][name] += item.amount;
                    }

                    if (name == "explosive.timed") count += item.amount;
                    if (name == "ammo.rocket.basic") rocketcount += item.amount;
                    if (name == "ammo.rocket.hv") hvrocket += item.amount;
                    if (name == "ammo.rocket.fire") incrocket += item.amount;
                    if (name.Contains("satchel")) satCount++;

                    var weapon = item?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                    if (weapon != null && !name.Contains("bow") && !name.Contains("eoka")) wepcount++;
                }
            }
            var ownSB = new StringBuilder();
            foreach (var kvp in itemOwners) foreach (var kvp2 in kvp.Value) ownSB.AppendLine(GetDisplayNameFromID(kvp.Key) + ": " + kvp2.Key + ", x" + kvp2.Value);
            Puts(ownSB.ToString().TrimEnd());
            Puts("c4: " + count + ", satchel charges: " + satCount + ", rocket: " + rocketcount + ", hv rocket: " + hvrocket + ", incendiary rocket: " + incrocket + ", total weapons count (baseprojectile) " + wepcount);
        }

        private void SetValue(object inputObject, string propertyName, object propertyVal)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) return;
            try
            {
                //find out the type
                var type = inputObject.GetType();
                if (type == null)
                {
                    PrintWarning("inputObject has type of null?!");
                    return;
                }

                //get the property information based on the type
                var propertyInfo = type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    PrintWarning("propertyInfo is null on GetField for type: " + type + ", propertyName: " + propertyName);
                    return;
                }

                //find the property type
                var propertyType = propertyInfo.FieldType;

                //Convert.ChangeType does not handle conversion to nullable types
                //if the property type is nullable, we need to get the underlying type of the property
                var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

                //Returns an System.Object with the specified System.Type and whose value is
                //equivalent to the specified object.
                propertyVal = Convert.ChangeType(propertyVal, targetType);

                //Set the value of the property
                propertyInfo.SetValue(inputObject, propertyVal);
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintWarning("SetValue had an exception, arguments: " + inputObject + " : " + propertyName + " : " + propertyVal);
            }
        }

        private object GetValue(object inputObject, string propertyName)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) return null;
            try
            {
                //find out the type
                var type = inputObject.GetType();
                if (type == null) return null;

                //get the property information based on the type
                return type?.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(inputObject) ?? null;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return null;
        }

        private bool IsNullableType(Type type) { return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)); }

        private void SendRustPlusNotification(ulong userId, string titleMsg, string bodyMsg = "PRISM", NotificationChannel channel = NotificationChannel.SmartAlarm)
        {
            PrintWarning(nameof(SendRustPlusNotification));

            NotificationList.SendNotificationTo(userId, channel, titleMsg, bodyMsg, Util.GetServerPairingData());

            PrintWarning("Ran " + nameof(NotificationList.SendNotificationTo) + " " + userId + " " + channel + " " + titleMsg + " " + bodyMsg);
        }

        public T RandomElement<T>(IEnumerable<T> source, System.Random rng)
        {
            T current = default;
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

        private readonly Dictionary<string, Vector3> preTpPosition = new();
        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true, bool respawnIfDead = false)
        {
            if (player == null || player?.transform == null) return false;

            preTpPosition[player.UserIDString] = player?.transform?.position ?? Vector3.zero;

            var playerPos = player?.transform?.position ?? Vector3.zero;
            var isConnected = player?.IsConnected ?? false;

            if (respawnIfDead && player.IsDead())
            {
                player.RespawnAt(dest, Quaternion.identity);
                return true;
            }

            player.EndLooting(); //redundant?
            player.EnsureDismounted();
            player.UpdateActiveItem(new ItemId(0));
            player.Server_CancelGesture();

            if (player.HasParent()) player.SetParent(null, true); //if check redundant?

            var distFrom = Vector3.Distance(playerPos, dest);

            if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
            if (doSleep && isConnected) player.StartSleeping();

            player.RemoveFromTriggers();
            player.MovePosition(dest);

            if (isConnected)
            {
                if (doSleep)
                {
                    player.inventory.crafting.CancelAll(true);
                    CancelInvoke("InventoryUpdate", player); //this is safe to call without checking IsInvoking
                }

                player.ClientRPCPlayer(null, player, "ForcePositionTo", dest);

                if (distFrom >= 250 && distChecks) player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);

                player.UpdateNetworkGroup();
                player.SendEntityUpdate();
                player.SendNetworkUpdateImmediate();

                if (distFrom > 100)
                {
                    player?.ClearEntityQueue(null);
                    player.SendFullSnapshot();
                }
            }

            player.ForceUpdateTriggers();

            return true;
        }

        public void Teleport(string userID, string strName, bool playersOnly, ulong ownerID = 0)
        {
            var targets = FindTargets(strName, playersOnly, ownerID);
            if (targets == null || targets.Count < 1) return;
            var rngTarget = UnityEngine.Random.Range(0, targets.Count);
            var targetPos = targets[rngTarget]?.transform?.position ?? Vector3.zero;
            if (targetPos == Vector3.zero)
            {
                PrintWarning("got targetPos vec3 zero from rngTarget!");
                return;
            }

            var player = FindPlayerByID(userID);
            if (player != null && !player.IsDestroyed && player.gameObject != null && player.transform != null) TeleportPlayer(player, targetPos, true, true);
        }

        private List<BaseNetworkable> FindTargets(string strName, bool playersOnly, ulong ownerID = 0)
        {
            if (string.IsNullOrEmpty(strName)) return null;
            var targets2 = new List<BaseNetworkable>();
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                if (entity == null || entity.IsDestroyed || entity.gameObject == null) continue;
                var baseEntity = entity as BaseEntity;
                if ((baseEntity == null || ownerID == 0 || baseEntity.OwnerID == ownerID) && entity.ShortPrefabName.IndexOf(strName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    targets2.Add(entity);
                }
            }
            return targets2;
        }

        private void cmdTeleportAny(IPlayer player, string command, string[] args)
        {
            if (player == null) return;
            if (player.IsServer)
            {
                SendReply(player, "This command cannot be run on the server.");
                return;
            }
            if (!permission.UserHasPermission(player.Id, "compilation.teleportany"))
            {
                SendReply(player, "You do not have permission to use this command.");
                return;
            }
            if (args.Length < 1) return;
            var ownerID = 0ul;
            if (args.Length > 1 && !ulong.TryParse(args[1], out ownerID))
            {
                SendReply(player, "Not a ulong: " + args[1]);
                return;
            }
            Teleport(player.Id, args[0], false, ownerID);
        }

        private void cmdAdminHelp(IPlayer player, string command, string[] args)
        {
            if (player == null) return;
            var msg = "<color=#00ff99><size=20>Need admin help or want to report a cheater? Read below:</size></color>\n\n-You can create a ticket to report a cheater or request help by typing: <color=#33ccff>/ticket create ''Type your report/question here''</color>\nPlease take note of your ticket number so you can reply. An example of a reply: <color=#33ccff>/ticket reply 51 ''Your reply message''</color> (51 is just an example, all tickets are different).\n\n-<color=#1a75ff>You can also PM an Administrator <color=#4d94ff>(use /PM PlayerName ''Your message'')</color> if you know one that is online.</color> We will attempt to assist you in any way we can, as soon as we can.\n\n-<color=#ff8000>Please <i><color=red>*DO NOT*</color></i> attempt to report cheaters in public chat. This will very likely make them turn their cheats off and make it more difficult for us to catch them.</color>";
            SendReply(player, msg);
        }

        private string GetContactHelpMessage()
        {
            return "<color=#00ff99><size=20>Question, comment or concern?</size>\nYou can contact us, make a report, or talk to your friends here:\n</color><color=#8aff47>Discord (URL is <i>CaSe SeNsitivE):</i> <color=#eb3dff>discord.gg/DUCnZhZ</color>\n\n</color><color=#7289DA>Link your discord account by typing <color=#42dfff>/connectdiscord</color>. </color><color=#f282c3>This will grant you access to a <i><color=white>special</color></i> kit!</color>\n\n<size=22>ALSO:</size>\n<color=orange>After linking your <color=#5662F6><i>Discord</i></color> account, you'll gain access to the <color=#5662F6>#rust-blue-chat</color> channel in our <color=#5662F6>Discord</color></color>.\n<color=yellow>From here, you can chat <color=green>IN-GAME</color> with other players, even if you're not online, and even on your phone!</color></color>";

        }

        private void cmdContactHelp(IPlayer player, string command, string[] args)
        {
            if (player == null) return;

            SendReply(player, GetContactHelpMessage());
        }

        private void cmdHelp2(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected) return;

            if (IsHardServer())
            {
                SendReply(player, "<size=11><color=white>[ <color=#d9d9d9>PRISM</color> <color=#000000><i>|</color> <color=#cc0000><size=13>RED</size></color></i> ]</color></size>\n<color=#a64dff>Hi!</color>\n<color=#994d00><size=20>This server aims to restore and improve upon the survival aspects of <color=#ff6600>Rust</color></color>.</size>\nThe gather rate is <i><color=yellow>0.5x</color></i> and the <color=#85adad>crafting rate is slowed</color>, <color=#75a3a3>especially for higher tier items</color>.\n\n<color=#6699ff><size=19>Blueprints work a bit differently here</color>.</size>\nFor high tier items, you will <i>need</i> to have the blueprint for the item <i>IN YOUR INVENTORY</i>. This will automatically unlock the Blueprint and it will act as a one-time use BP. Once the item is crafted, the blueprint will be consumed permanently.\nFor most items, blueprints are unlocked normally, but may be locked until a certain amount of time has passed. You'll see this when trying to learn one that's still locked.\n\n<color=#ffdb4d><size=18>Ammunition is an important aspect here, too</color>.</size>\nIn order to craft higher tier ammo, such as pistol bullets, 5.56 and shotgun ammo, you'll *<color=red>NEED</color>* to find <color=#ffdb4d>Empty Ammo Cartridges</color>. You'll also need the blueprint to craft the ammo. Then, as long as you have enough cartridges, you can craft your ammo!\n\n<color=#e67300>Building isn't quite the same, either</color>!\nWhen upgrading builidng blocks like foundations and walls, you'll see that the upgrade takes a few seconds an does not happen immediately. This takes longer for higher tiers.\n\n<color=#99ff33>What else? Well, your <color=#ff9900>calories</color> & <color=#b3d9ff>hydration</color> are super important</color>!\nIf you're starving or thirsting to death, your player will begin to have physical effects. This means you may sometimes miss a hit when attacking with a melee weapon. It is necessary to keep a good source of food and water (think: survival)!\n\n<color=#70dbdb>We hope you enjoy it here</color>. We aim to provide a completely unique experience, and we hope you love it as much as we loved crafting it for you all.");
                return;
            }

            var isEasy = IsEasyServer();
            var msg0 = "<size=18><color=#33ccff>New to PRISM? Here's some general info:</color></size>" + (!isEasy ? "\n<color=#1affa3>We aim for a good mixture of vanilla & modded. Not too easy, not too hard.</color>" : string.Empty);
            var msg = "<color=#ff471a>---<size=16>LEVELS</size>---</color>\nOur biggest modification comes in the form levels: Woodcutting, Mining, Skinning and Luck. The first 3 are simple. Leveled up by doing their approriate actions, each level will grant more resources.\n--<color=#2be6ff><i>Catchup</i></color>\n<color=#b5ff4d>'<color=#ff66bf>Catchup</color>' Gives significant XP boosts based on how far behind others you are.</color> <color=#ff230f>Start late?</color> <color=#1fadff>No worries!</color>\n<color=#faaf5f>See</color>: <color=#fff12b>/<color=#a0ff4d>stats</color></color>\n\n\n<color=#5DC540>Luck</color> (<color=#5DC540>/luck</color>): Full of depth & leveled by picking up Hemp, Stones, Wood and other collectables.\nEach level you'll get 1 Luck skill point — used to upgrade various perks.\nSee <color=#8aff47>/skt</color> for more.\n\nOur base gather rate is <color=#8aff47>" + (isEasy ? "1" : "0.5") + "x</color>.\n\n<color=#ace600>---<size=16>ARENAS</size>---</color>\nWe have various arenas, the main being a free-for-all PvP arena! Featuring sectioned off areas where you can play without consequence. Read more by typing <color=#ff912b>/dm</color>!";
            var msg2 = "<color=#00aaff>---<size=16>CURRENCY</size>---</color>\n<color=#33cc33>Batteries are money!</color> Use batteries to purchase components from <i>server owned</i> vending machines placed at monuments.\nAcquire batteries through the Luck skill ''Battery Collector'', or from other players, be that trading or stealing.\n\n<color=#ff0080>---<size=16>TELEPORTS</size>---</color>\nTeleport to players or set home teleports with <color=red>/tpr</color> and <color=red>/home</color>, respectively.\nTeleports are limited to a max number of teleports per day. See: <color=#8aff47>/tpinfo</color>.\nThere's no teleport while raiding, being raided, attacking or being attacked.\n\n<color=#faff66>---<size=16>COMMANDS & MORE</size>---</color>\nType <color=#32dbdb>/helpc</color> to read more info.";
            var msg3 = "<color=#5DC540>---<size=16>LUCK</size>---</color>\n<color=#9df957>Get lucky!</color> <color=#bef738>Level it by picking up collectables like Hemp, Wood, Stones & more. Type <color=#5DC540>/luck</color> to learn more!</color>";
            var msg4 = "<color=#a8ff66>/tagc</color> - Used to change your name color. Limited access with MVP/VIP, but fully accessible if you have Color rank or higher, and the title must be selected!\n\n<color=#eff759>/title</color> - Used to select/deselect a title like MVP, Color, Master, etc.\n\n<color=#b973ff>/skin</color> - Use any skin on any item!\n\n<color=#ff6a3d>/bgrade</color> - Automatically upgrade buildings (walls, floors, etc.) when placing them (still costs resources!)\n\n<color=#34aefa>/sil</color> chat command <color=#34aefa>sil</color> console command - Paste image URLs onto signs! Use without <color=#96fff3>/</color> in the F1 console for long links.";
            if (args.Length > 0)
            {
                if (args[0].Equals("2", StringComparison.OrdinalIgnoreCase))
                {
                    SendReply(player, "<color=#ff912b>---<size=18>HELP PAGE 2</size>---</color>\n\n" + msg2);
                    SendReply(player, msg3);
                }
                else if (args[0].Equals("3", StringComparison.OrdinalIgnoreCase)) SendReply(player, "<color=#ff912b>---<size=18>HELP PAGE 3 (DONATORS)</size>---</color>\n\n" + msg4);
                else SendReply(player, "Invalid help page specified!");
            }
            else
            {
                SendReply(player, msg0);
                SendReply(player, msg);
                //    var streamerTxt = "---<color=#ff9736><size=16>STREAMERS---</size></color>---\n<color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M</color> <color=#66fffa>welcomes those wishing to stream!</color> <color=#30fff8>Enable '<color=#ff6be1>Streamer Mode</color>' in the Rust settings & we'll automatically make a few changes on our side ensuring your gameplay & stream continue without issue</color>.\n<color=#91ff59>Streamers, take note of the <color=#ff1919>/ignore</color> and <color=#ed4580>/clear</color> commands.\n<color=#ff457a>Ignore allows you to block all public & private messages from a player.</color>\nThe clear command will blank out your entire chat, removing any recent messages from view</color>.";
                var upStr = "---<color=#ffc629><size=20>UP</size></color>---\n<color=#d152ff>Use <color=#4dffb2>/up</color> to mass-upgrade a base. It saves time & effort!</color> <color=#ff4096>:)</color>\n\n";
                var tradeStr = "---<color=#85bb65><size=16>TRADE</size></color>---\n<color=#85bb65>Trade players by typing <color=#8aff47>/trade</color>!</color>\n\n";
                var discordStr = "---<color=#7289DA><size=20>DISCORD</size></color>---\n<color=#f282c3>Join our Discord server to talk with friends, contact an admin & much more!</color>\n<color=#ffbae1>Connect your account for a special kit! Type <color=#81ecf1>/discord</color> for more info</color>.\n\n";
                var f1HelpStr = "---Press <i><color=#9E88D1><size=20>F1</size></color></i> for our <i><size=16>CUSTOM, EXPANSIVE HELP MENU!</size></i>---";
                SendReply(player, discordStr + tradeStr + upStr + f1HelpStr + "\n<color=#57adcf>Type <color=#af67ef>/help 2</color> to read page two of help.\nType <color=#ff4df0>/help 3</color> to read page three (donators) of help.\nType <color=#e5d2b9>/helpc</color> to read even more.");
                ShowPopup(player?.Object as BasePlayer, "Open chat & click (hold) & drag down if text is cut off.", 7.25f);
            }
        }

        private void cmdShop(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected) return;
            var msg0 = "<size=18><color=#33ccff>PRISM Shops:</color></size>\n<color=#1affa3>PRISM has various server shops in the form of vending machines.</color>\n\n<color=#FFBCD9>These shops take batteries, you can find these shops (vending machines) near monuments. Check your map (usually the <color=#0088DC>G</color> or <color=#0088DC>M</color> key) to see where they are.\nFor more info, please read <color=#1affa3>/help</color>.</color>";
            SendReply(player, msg0);
        }

        private void cmdTown(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected) return;
            var msg0 = "<size=18><color=#33ccff>Towns:</color></size>\n<color=#1affa3>While there are no specific towns, you can go to the Output/Compound. This is a safe area where you can trade, and spend your batteries (and other things).\n\nYou can view it on the map (usually the <color=#0088DC>G</color> or <color=#0088DC>M</color> key) to see where it is.</color>";
            SendReply(player, msg0);
        }

        private void cmdMyMini(IPlayer player, string command, string[] args)
        {
            SendReply(player, "Minicopters spawn near the roads on this server :)");
        }


        private readonly Dictionary<string, Dictionary<BaseEntity, float>> lootedEntTime = new();

        private float GetLootTime(string userID, BaseEntity entity)
        {
            if (string.IsNullOrEmpty(userID) || entity == null) throw new ArgumentNullException();
            if (!lootedEntTime.TryGetValue(userID, out Dictionary<BaseEntity, float> outLootDic)) return -1;
            if (!outLootDic.TryGetValue(entity, out float lastTime)) lastTime = -1f;
            return lastTime;
        }

        private readonly HashSet<NetworkableId> lootedEntities = new();

        private void OnLootEntity(BasePlayer player, BaseEntity target)
        {
            if (player == null || target == null) return;
            if (target is Stocking) return;

            if (player?.metabolism != null)
            {
                var reduc = UnityEngine.Random.Range(0.3f, 0.8f);
                if (player.metabolism.calories.value > 0.0) player.metabolism.calories.value -= reduc;
                if (player.metabolism.hydration.value > 0.0) player.metabolism.hydration.value -= reduc * 0.32f;
            }

            if (!lootedEntTime.TryGetValue(player.UserIDString, out Dictionary<BaseEntity, float> lootDic)) lootedEntTime[player.UserIDString] = new Dictionary<BaseEntity, float>();

            lootedEntTime[player.UserIDString][target] = UnityEngine.Time.realtimeSinceStartup;

            var loot = target as LootContainer;
            var netID = target.net.ID;
            if (loot != null && IsEasyServer() && !IsHardServer())
            {
                if (lootedEntities.Add(netID))
                {
                    if (UnityEngine.Random.Range(0, 101) <= (SaveSpan.TotalDays < 5 ? UnityEngine.Random.Range(2, 10) : UnityEngine.Random.Range(9, 23)))
                    {
                        var rngCat = GetRandomCategory(ItemCategory.All, ItemCategory.Resources, ItemCategory.Search, ItemCategory.Food, ItemCategory.Misc, ItemCategory.Common, ItemCategory.Favourite);
                        var noRares = Pool.GetList<Rust.Rarity>();


                        try
                        {
                            if (UnityEngine.Random.Range(0, 101) >= UnityEngine.Random.Range(10, 20)) noRares.Add(Rust.Rarity.VeryRare);
                            if (UnityEngine.Random.Range(0, 101) >= UnityEngine.Random.Range(25, 45)) noRares.Add(Rust.Rarity.Rare);
                            var rngRarity = (noRares.Count > 0) ? GetRandomRarityOrDefault(rngCat, true, noRares.ToArray()) : GetRandomRarityOrDefault(rngCat, true);
                            var rngBP = GetUnlearnedBP(rngRarity, player, rngCat);
                            if (rngBP == null)
                            {
                                //  PrintWarning("rngBP was null, going to try a few more times!");
                                for (int i = 0; i < 150; i++)
                                {
                                    rngBP = GetUnlearnedBP(rngRarity, player, rngCat);
                                    if (rngBP != null)
                                    {
                                        PrintWarning("Got rngBP (crate) after: " + i + " tries");
                                        break;
                                    }
                                    if (i > 5) rngRarity = GetRandomRarity(rngCat, true, noRares.ToArray());
                                    if (i > 11) rngRarity = GetRandomRarity(rngCat, true, Rust.Rarity.VeryRare);
                                    if (i >= 20) rngCat = GetRandomCategory(ItemCategory.Search, ItemCategory.All, ItemCategory.Food, ItemCategory.Favourite, ItemCategory.Common);
                                }
                            }

                            if (rngBP != null)
                            {
                                PrintWarning("Got rngBP (crate) of: " + rngBP.blueprintTargetDef.shortname + ", for: " + player?.displayName + ", in crate: " + target?.ShortPrefabName);
                                if (!rngBP.MoveToContainer(loot.inventory) && !rngBP.Drop(player.GetDropPosition(), player.GetDropVelocity(), player.ServerRotation))
                                {
                                    PrintWarning("Failed to move OR drop rngBP!");
                                    RemoveFromWorld(rngBP);
                                }
                            }
                            else PrintWarning("rngBP *IS* null on loot entity! category: " + rngCat + ", rarity: " + rngRarity);

                        }
                        finally
                        {
                            Pool.FreeList(ref noRares);
                        }

                    }
                    /*/
                    if (findScrap != null)
                    {
                        var newAmount = findScrap.amount * 2;
                        PrintWarning("Changing scrap from: " + findScrap.amount + ", to: " + newAmount);
                        findScrap.amount = newAmount;
                        findScrap.MarkDirty();
                    }/*/

                    if (UnityEngine.Random.Range(0, 101) <= (SaveSpan.TotalDays < 4 ? UnityEngine.Random.Range(8, 28) : UnityEngine.Random.Range(25, 45)))
                    {
                        Item findScrap = null;
                        if (loot?.inventory?.itemList != null && loot.inventory.itemList.Count > 0)
                        {
                            for (int i = 0; i < loot.inventory.itemList.Count; i++)
                            {
                                var item = loot.inventory.itemList[i];
                                if (item?.info.itemid == SCRAP_ITEM_ID)
                                {
                                    findScrap = item;
                                    break;
                                }
                            }
                        }

                        // var findScrap = (target as LootContainer)?.inventory?.itemList?.Where(p => (p?.info?.shortname == "scrap"))?.FirstOrDefault() ?? null;
                        if (findScrap != null && SaveSpan.TotalDays >= 2)
                        {
                            var scrapScale = Mathf.Clamp((float)GetScrapScalar(player), 0, 800);
                            if (scrapScale >= 1)
                            {
                                if (player.IsAdmin) PrintWarning("Scrap scale is: " + scrapScale + ", for: " + player.displayName + ", pre scrap amount: " + findScrap.amount);
                                findScrap.amount += (int)Math.Round(findScrap.amount * scrapScale / 100, MidpointRounding.AwayFromZero);
                                findScrap.MarkDirty();
                                if (player.IsAdmin) PrintWarning("Post scrap amount for " + player.displayName + ": " + findScrap.amount);
                            }
                            else PrintWarning("< 1 scrap scale: " + player.displayName + ", scale: " + scrapScale);
                        }
                    }
                }
            }

            var isStash = (target?.ShortPrefabName ?? string.Empty).Contains("stash");
            if (!isStash) return;

            var msgSB = new StringBuilder();
            if ((target.OwnerID != 0) && (player.userID != target.OwnerID) && !player.IsAdmin)
            {
                var hasFriend = Friends?.Call<bool>("HasFriend", target.OwnerID, player.userID) ?? false;
                if (!hasFriend)
                {
                    msgSB.AppendLine("<color=yellow>" + player.displayName + "</color> has just looted an unowned stash at: " + target.transform.position);
                    PrintWarning(RemoveTags(msgSB.ToString().TrimEnd()));
                }
            }

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null || !(p?.IsConnected ?? false)) continue;
                if ((p?.IsAdmin ?? false) || permission.UserHasPermission(p.UserIDString, "compilation.bug")) SendReply(p, msgSB.ToString().TrimEnd());
            }
        }

        private void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private object OnGetRadiationProtection(BasePlayer player, float val)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return null;

            var wearItems = player?.inventory?.containerWear?.itemList;

            if (wearItems != null && wearItems.Count == 1 && wearItems[0]?.info?.itemid == -253079493)
                return 99999f;

            return null;
        }

        private void OnRunPlayerMetabolism(PlayerMetabolism metabolism, BaseCombatEntity ownerEntity)
        {
            if (metabolism == null) return;
            var player = ownerEntity as BasePlayer;
            if (player == null || !player.IsConnected || player.IsDead()) return;

            if (player.InSafeZone() && !player.IsHostile())
            {
                player.metabolism.temperature.value = 32f;
            }

            /*/
            if (IsNight() && IsEasyServer())
            {
                if ((player?.metabolism?.radiation_level?.value > 0.0 || player?.metabolism?.radiation_poison?.value > 0.0))
                {
                    player.metabolism.radiation_level.value -= 1.5f;
                    player.metabolism.radiation_poison.value -= 1.5f;
                }
            }/*/
            if (player.metabolism.bleeding.max < 10 && player.metabolism.bleeding.max > 0.0) player.metabolism.bleeding.max = 10;
            if (metabolism.calories.value > 0.0 || metabolism.hydration.value > 0.0)
            {
                var reduc = UnityEngine.Random.Range(0.00775f, 0.0125f);
                var swimming = player?.IsSwimming() ?? false;
                if (player.IsRunning() && !swimming) reduc *= UnityEngine.Random.Range(2.1f, 3.3f);
                if (!player.IsOnGround() && !swimming) reduc *= UnityEngine.Random.Range(1.9f, 2.5f);
                if (swimming) reduc *= UnityEngine.Random.Range(4f, 5.2f);

                var idleTime = player?.IdleTime ?? 0f;

                if (idleTime > 420) reduc *= 0.25f;

                reduc *= Mathf.Clamp(player.metabolism.temperature.value / 25, 1, 10);
                if (metabolism.hydration.value > 0.0) metabolism.hydration.value -= reduc;
                if (metabolism.calories.value > 0.0) metabolism.calories.value -= reduc;
            }
        }

        private object CanAcceptItem(ItemContainer container, Item item, int targetPos)
        {
            if (container == null || item == null) return null;

            var owner = container?.GetOwnerPlayer() ?? container?.playerOwner ?? (container?.entityOwner as BasePlayer) ?? (container?.entityOwner as BaseProjectile)?.GetOwnerPlayer() ?? container?.parent?.GetOwnerPlayer();

            var prefabId = container?.entityOwner?.prefabID ?? 0;

            if (item.amount > 1 && (prefabId == 1123731744 || prefabId == 3846783416))
            {

                var ply = owner ?? item?.GetOwnerPlayer() ?? container?.playerOwner ?? container?.GetOwnerPlayer() ?? FindLooterFromCrate(container?.entityOwner) ?? null;
                if (ply != null && ply.IsConnected) SendReply(ply, "Can only repair a single item at a time!");

                return ItemContainer.CanAcceptResult.CannotAccept;
            }

            var prefabName = container?.entityOwner?.ShortPrefabName ?? string.Empty;
            if (item.info.category == ItemCategory.Food && prefabName.Contains("refinery")) return ItemContainer.CanAcceptResult.CannotAccept;

            return null;
        }

        private void OnFuelConsume(BaseOven oven, Item fuel, ItemModBurnable burnable)
        {
            if (oven == null) return;
            var prefabName = oven?.ShortPrefabName ?? string.Empty;
            if (string.IsNullOrEmpty(prefabName)) return;
            for (int i = 0; i < oven.inventory.itemList.Count; i++)
            {
                var item = oven.inventory.itemList[i];
                if (item == null) continue;
                if (!(item.info.shortname.Contains("raw") || (item.info.category == ItemCategory.Food && !item.info.shortname.Contains("cook") && !item.info.shortname.Contains("burn")))) continue;
                var rngCook = UnityEngine.Random.Range(0, 101);
                if (rngCook <= 45) continue;
                ItemDefinition cookedDef = null;
                for (int j = 0; j < ItemManager.itemList.Count; j++)
                {
                    var item2 = ItemManager.itemList[j];
                    if (item2?.shortname == item.info.shortname)
                    {
                        cookedDef = item2?.GetComponent<ItemModCookable>()?.becomeOnCooked ?? null;
                        break;
                    }
                }
                if (cookedDef == null) continue;
                if (item.amount > 1)
                {
                    item.amount--;
                    item.MarkDirty();
                }
                else RemoveFromWorld(item);
                Item findCooked = null;
                for (int j = 0; j < oven.inventory.itemList.Count; j++)
                {
                    var item2 = oven.inventory.itemList[j];
                    if (item2?.info == cookedDef)
                    {
                        if ((item2.amount + 1) <= item2.MaxStackable())
                        {
                            findCooked = item2;
                            break;
                        }
                    }
                }
                if (findCooked != null)
                {
                    findCooked.amount++;
                    findCooked.MarkDirty();
                }
                else
                {
                    var newItem = ItemManager.Create(cookedDef, 1);
                    if (!newItem.MoveToContainer(oven.inventory))
                    {
                        if (!newItem.Drop(oven.GetDropPosition(), oven.GetDropVelocity()))
                        {
                            PrintWarning("no drop cookedDef!");
                            RemoveFromWorld(newItem);
                        }
                    }
                }

            }

            if (!prefabName.Contains("furnace") && !prefabName.Contains("refin")) return;
            if (prefabName.Contains("furnace"))
            {
                var isLarge = prefabName.Contains("large");
                var isEasy = IsEasyServer();
                var rngNeed = (isEasy ? UnityEngine.Random.Range(1, 16) : 0) + (isLarge ? 50 : 30);
                var rng = UnityEngine.Random.Range(0, 101);
                if (rng <= rngNeed)
                {
                    NextTick(() =>
                    {
                        if (oven == null || burnable == null)
                        {
                            PrintWarning("oven or burnable is null on nexttick");
                            return;
                        }
                        var scaleMin = isLarge ? 2.5f : 1.8f;
                        var scaleMax = isLarge ? 4.5f : 3.3f;
                        if (isEasy)
                        {
                            scaleMin += UnityEngine.Random.Range(isLarge ? 1.2f : 0.7f, isLarge ? 2.15f : 1.7f);
                            scaleMax += UnityEngine.Random.Range(isLarge ? 1.7f : 1.1f, isLarge ? 2.75f : 2.2f);
                        }
                        var charcoalAdd = (int)Math.Round(burnable.byproductAmount * UnityEngine.Random.Range(scaleMin, scaleMax), MidpointRounding.AwayFromZero);
                        if (charcoalAdd < 1)
                        {
                            PrintWarning("charcoal add < 1!");
                            return;
                        }
                        Item findByproduct = null;
                        for (int i = 0; i < oven.inventory.itemList.Count; i++)
                        {
                            var item = oven.inventory.itemList[i];
                            if (item != null && item.info == burnable.byproductItem && ((item.amount + charcoalAdd) <= item.MaxStackable()))
                            {
                                findByproduct = item;
                                break;
                            }
                        }
                        if (findByproduct != null)
                        {
                            findByproduct.amount += charcoalAdd;
                            findByproduct.MarkDirty();
                        }
                        else
                        {
                            var byproduct = ItemManager.Create(burnable.byproductItem, charcoalAdd);
                            if (byproduct == null)
                            {
                                PrintWarning("byproduct is null!!");
                                return;
                            }
                            if (!byproduct.MoveToContainer(oven.inventory) && !byproduct.Drop(oven.GetDropPosition(), oven.GetDropVelocity()))
                            {
                                PrintWarning("NO byproduct move or drop!!");
                                RemoveFromWorld(byproduct);
                            }
                        }
                    });

                }
            }
            for (int i = 0; i < oven.inventory.itemList.Count; i++)
            {
                var cooked = oven.inventory.itemList[i];
                if (cooked == null) continue;
                var burntStr = cooked.info.shortname.Replace("cooked", "burned");
                if (burntStr == cooked.info.shortname) continue;
                var burntDef = ItemManager.FindItemDefinition(burntStr);
                if (burntDef == null) continue;
                Item findBurnt = null;
                for (int j = 0; j < oven.inventory.itemList.Count; j++)
                {
                    var item2 = oven.inventory.itemList[j];
                    if (item2?.info == burntDef)
                    {
                        if ((item2.amount + 1) <= item2.MaxStackable())
                        {
                            findBurnt = item2;
                            break;
                        }
                    }
                }
                if (findBurnt != null)
                {
                    findBurnt.amount++;
                    findBurnt.MarkDirty();
                    RemoveFromWorld(cooked);
                    continue;
                }
                var burntItem = ItemManager.Create(burntDef, (cooked.amount > 0) ? cooked.amount : 1);
                if (burntItem == null) continue;

                var oldPos = cooked?.position ?? -1;
                RemoveFromWorld(cooked);
                if (!burntItem.MoveToContainer(oven.inventory, oldPos))
                {
                    PrintWarning("no move burnt item");
                    if (!burntItem.Drop(oven.GetDropPosition(), oven.GetDropVelocity()))
                    {
                        PrintWarning("no drop burnt item");
                        RemoveFromWorld(burntItem);
                    }
                }
            }

        }

        private readonly string[] nameBlacklist = { "Shady", "Notorious", "Koenrad", "Anarchy" };

        private object CanClientLogin(Connection connection)
        {
            if (connection == null)
            {
                PrintWarning("Disallowing null connection!");
                return false;
            }

            if (connection.userid < 76561100000000000)
            {
                PrintWarning("Connection userID is BAD: " + connection?.userid);
                return "Invalid UID";
            }

            var name = connection?.username ?? GetDisplayNameFromID(connection?.userid ?? 0);
            if (string.IsNullOrWhiteSpace(name)) return "Invalid Name! (blank)";


            if ((connection?.authLevel ?? 0) < 1 && nameBlacklist.Any(p => p.Equals(name, StringComparison.OrdinalIgnoreCase))) return "This name is blacklisted.";

            var lowerName = name.ToLower();
            var IP = getIPString(connection);

            if (string.IsNullOrWhiteSpace(IP))
                return "Invalid IP (N/E)";

            if (name.Equals("Школя ок пида ок", StringComparison.OrdinalIgnoreCase) || name.Equals("Школярок пидарок", StringComparison.OrdinalIgnoreCase) || connection.userid.ToString() == "76561198245360548")
            {
                PrintWarning("Spoofing IP bouta be banned: " + IP);
                covalence.Server.Command("banbyip \"" + IP + "\"" + " Spoofing");
                return "IVN";
            }


            // if (name.Length < 3 || name.Replace(" ", string.Empty).Length < 3) return "Your name must be 3 characters or longer! (if you changed it, restart your game)";
            //   if (name.Contains("hacker", CompareOptions.OrdinalIgnoreCase)) return "Invalid name, please remove 'hacker'";
            /*/
               timer.Once(0.1f, () =>
               {
                   if (connection != null && connection.active && connection.state != Connection.State.Disconnected && !connection.rejected)
                   {
                       connection.username = connection.username.Replace("knights-table.net", "discord.gg/DUCnZhZ", StringComparison.OrdinalIgnoreCase);
                       connection.username = connection.username.Replace("rusticaland.net", "discord.gg/DUCnZhZ", StringComparison.OrdinalIgnoreCase);
                       connection.username = connection.username.Replace("knight-table.net", "discord.gg/DUCnZhZ", StringComparison.OrdinalIgnoreCase);

                       connection.username = RemoveTags(connection.username);
                   }
               });/*/


            /*/
            for (int i = 0; i < invalidNames.Length; i++)
            {
                var invalidName = invalidNames[i];
                if (lowerName.Contains(invalidName)) return "Your name cannot contain: " + invalidName + "! (change it)";
            }/*/

            var nonLatin = name.Count(p => !basicLatin.Match(p.ToString()).Success);

            var chrs = GetValidCharactersString();

            var nonValid = name.Count(p => !chrs.Any(x => x.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase)));

            if (nonValid > 0) PrintWarning("Non valid: " + nonValid.ToString("N0") + " count, for name: " + name);

            if (nonLatin >= name.Length * 0.5f || nonValid >= name.Length * 0.5f)
            {
                PrintWarning("Non latin count: " + nonLatin + " name length: " + name.Length + ", nonValid count: " + nonValid);
                return "Your name must have mostly basic latin letters! (Change your name)";
            }

            var sb = Pool.Get<StringBuilder>();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null) continue;

                    var clanTag = Clans?.Call<string>("GetClanOf", player.UserIDString) ?? string.Empty;

                    var clanTagName = string.IsNullOrEmpty(clanTag) ? string.Empty : sb.Clear().Append("[").Append(clanTag).Append("] ").Append(player.displayName).ToString();

                    if (player.displayName.Equals(name, StringComparison.OrdinalIgnoreCase) || clanTagName.Equals(name, StringComparison.OrdinalIgnoreCase)) return "Someone is already connected with this name.";
                }
            }
            finally { Pool.Free(ref sb); }


            return null;
        }

        public bool IsBasicLetter(char c) { return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'); }

        private void SendInitMessage(BasePlayer player, string msg)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(msg)) return;
            var isSnap = player?.IsReceivingSnapshot ?? true;
            if (isSnap) timer.Once(1.5f, () => SendInitMessage(player, msg));
            else SendReply(player, msg);
        }

        private void SendJoinMessages(BasePlayer player, bool admOnly = false, bool sendDiscordMsg = true)
        {
            if (player == null) return;
            var sb = Pool.Get<StringBuilder>();
            try
            {
                var joinMsg = sb.Clear().Append("<color=#F17E06>").Append(player.displayName).Append("</color>:<color=#E9388C> joined<color=#613A00>.</color></color>").ToString();

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || !p.IsConnected || (admOnly && !p.IsAdmin)) continue;
                    SendMessage(p, string.Empty, joinMsg, player.UserIDString); //777
                }

                if (sendDiscordMsg)
                    DiscordAPI2?.Call("SendChatMessageByName", sb.Clear().Append(player.displayName).Append(" has joined").ToString(), "PRISM", @"https://cdn.prismrust.com/PRISM_BLUE_LOGO_CROP_SOLID_SQUARED_UP_11_C3_test.jpg"); //must supply a URL image link at end, not ID
            }
            finally { Pool.Free(ref sb); }


        }

        private void SendLeaveMessages(BasePlayer player, string reason = "Disconnected", bool adminOnly = false, bool sendDiscordMsg = true)
        {
            if (player == null) return;
            var sb = Pool.Get<StringBuilder>();
            try
            {
                Puts(sb.Clear().Append(player.displayName).Append(" (").Append(player.UserIDString).Append(") disconnected: ").Append(reason).ToString());

                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || !p.IsConnected || (adminOnly && !p.IsAdmin)) continue;
                    SendMessage(p, string.Empty, sb.Clear().Append("<color=#F17E06>").Append(player.displayName).Append("</color>:<color=#E9388C> disconnected </color>(<color=#613A00>").Append(reason).Append("</color>)").ToString(), player.UserIDString);
                }

                if (sendDiscordMsg)
                    DiscordAPI2?.Call("SendChatMessageByName", sb.Clear().Append(player.displayName).Append(" has disconnected").ToString(), "PRISM", @"https://cdn.prismrust.com/PRISM_BLUE_LOGO_CROP_SOLID_SQUARED_UP_11_C3_test.jpg"); //must supply a URL image link at end, not ID

            }
            finally { Pool.Free(ref sb); }

        }

        private void SendMessageOnWakeup(IPlayer player, string msg)
        {
            if (player == null || !player.IsConnected) return;
            if (!player.IsSleeping) player.Message(msg);
            else timer.Once(0.5f, () => SendMessageOnWakeup(player, msg));
        }

        private void SendMessageOnWakeup(BasePlayer player, string msg)
        {
            var ply = (player == null) ? null : covalence.Players.FindPlayerByObj(player);
            if (ply != null) SendMessageOnWakeup(ply, msg);
        }

        private void DoVeteran(BasePlayer player, bool join = false)
        {
            if (player == null) return;

            if (join)
            {
                if (permission.UserHasPermission(player.UserIDString, "kits.veteran3"))
                {
                    SendMessageOnWakeup(player, "<size=20><color=#5cc6ff>Welcome back, <color=#f0c82b>Rust God</color> <color=#a16eff>" + player.displayName + "</color>!</color></size>");
                }
                else
                {
                    if (permission.UserHasPermission(player.UserIDString, "kits.veteran1") || permission.UserHasPermission(player.UserIDString, "kits.veteran2"))
                    {
                        SendMessageOnWakeup(player, "<size=18><color=#ff912b>Welcome back, Veteran <color=orange>" + player.displayName + "</color>!</color></size>");
                    }
                }
            }

            if (!permission.UserHasPermission(player.UserIDString, "kits.veteran1"))
            {
                var playTime = Playtimes?.Call<TimeSpan>("GetTimePlayed", player.UserIDString) ?? TimeSpan.MinValue;
                if (playTime.TotalHours >= 150)
                {
                    covalence.Server.Command("o.grant user \"" + player.UserIDString + "\" kits.veteran1");
                    DiscordAPI2?.Call("GiveVeteranRole", player.UserIDString);
                    SendMessageOnWakeup(player, "<color=#ff912b><size=22>You have been granted the 150 hour <color=orange>Veteran</color> kit!</color></size> As a thanks for being a faithful player, we've given you access to a new kit. Type <color=#ff912b>/kit</color> to redeem it!");
                }
            }
            if (!permission.UserHasPermission(player.UserIDString, "kits.veteran2"))
            {
                var firstJoin = GetFirstConnectionS(player.UserIDString);
                if (firstJoin <= DateTime.MinValue)
                {
                    PrintWarning("firstJoin is MinValue: " + player);
                    return;
                }
                if ((DateTime.UtcNow - firstJoin).TotalDays >= 365)
                {
                    covalence.Server.Command("o.grant user \"" + player.UserIDString + "\" kits.veteran2");
                    DiscordAPI2?.Call("GiveVeteranRole", player.UserIDString);
                    SendMessageOnWakeup(player, "<color=#ff912b><size=22>You have been granted the 1 year <color=orange>Veteran</color> kit!</color></size> As a thanks for being a faithful player, we've given you access to a new kit. Type <color=#ff912b>/kit</color> to redeem it!");
                }
            }
            if (!permission.UserHasPermission(player.UserIDString, "kits.veteran3") && permission.PermissionExists("kits.veteran3"))
            {
                var playTime = Playtimes?.Call<TimeSpan>("GetTimePlayed", player.UserIDString) ?? TimeSpan.MinValue;
                if (playTime.TotalHours >= 1000)
                {
                    PrintWarning(player + " is receiving the Veteran 3 kit! Play time in hours: " + playTime.TotalHours.ToString("0.00").Replace(".00", string.Empty));
                    covalence.Server.Command("o.grant user \"" + player.UserIDString + "\" kits.veteran3");
                    SendMessageOnWakeup(player, "<color=#ff912b><size=22>You have been granted the 1000 hour <color=orange>Veteran</color> kit!</color></size> As a thanks for being a faithful player, we've given you access to a new kit. Type <color=#ff912b>/kit</color> to redeem it!");
                }
            }
        }

        private void ShowLoginCUI(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            string txt = "<color=#56FCFF>Welcome to</color> <i><color=#FF0030>P</color><color=#FE950A>R</color><color=#FEF506>I</color><color=#53D559>S</color><color=#2BACE2>M. </color></i> <color=#38FF94>PLEASE <size=30><i><color=#FB21FF>PRESS</color> <color=#FF1495>F1</color></i></size> FOR OUR HELP MENU<color=#FF6B21>/</color><color=#FF63C5>QUICK START GUIDE</color> <color=#AFEBFF>&</color> READ <color=#FFAABB>/help</color> <color=#FA00FF>|</color> Enjoy!</color>\n<color=#9D89D1>Next wipe: " + ReadableTimeSpan(GetWipeDate() - DateTime.Now) + "</color>";

            var LoginMenu = new CuiElementContainer();

            var mainName = LoginMenu.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.95", FadeIn = 0.5f },
                RectTransform = { AnchorMin = "0.30 0.12", AnchorMax = "0.70 0.25" },
                CursorEnabled = false,
                FadeOut = 1f
            }, "Under");

            var Text2 = new CuiElement
            {
                Name = "Text",
                FadeOut = 1f,
                Parent = "Under",
                Components =
                {
                    new CuiTextComponent { Text = txt, Align = TextAnchor.MiddleCenter, FadeIn = 0.5f, FontSize = 22, },
                    new CuiRectTransformComponent { AnchorMin = "0.30 0.12", AnchorMax = "0.70 0.25" }
                }
            };

            LoginMenu.Add(Text2);
            CuiHelper.DestroyUi(player, mainName);
            CuiHelper.AddUi(player, LoginMenu);

            player.Invoke(() =>
            {
                if (player == null || !player.IsConnected) return;

                CuiHelper.DestroyUi(player, mainName);
                CuiHelper.DestroyUi(player, "Text");

            }, 20f);
        }

        private void ShowLoginCUIOnWakeup(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;
            if (!player.IsSleeping()) ShowLoginCUI(player);
            else timer.Once(1f, () => ShowLoginCUIOnWakeup(player));
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                if (player == null) return;

                var sb = Pool.Get<StringBuilder>();
                try
                {
                    Puts(sb.Clear().Append(player.displayName).Append(" (").Append(player.UserIDString).Append(", ").Append(getIPString(player)).Append(") connected").ToString());
                }
                finally { Pool.Free(ref sb); }

                if (!permission.UserHasPermission(player.UserIDString, "compilation.nojoinmsg"))
                    SendJoinMessages(player, false);


                var wipeDate = GetWipeDate();
                var span = ReadableTimeSpan(wipeDate - DateTime.Now);
                var nextMsg = wipeDate.ToString("d");

                ShowLoginCUIOnWakeup(player); //temp red
                if (!IsEasyServer()) SendMessageOnWakeup(player, "<color=#42dfff>We have a new <size=20>ARPG</size> server (PRISM | BLUE)!</color> <color=#eb3dff>Check it out: prismrust.com:<color=#8aff47>28016</color></color>");
                var wakeMsg = "<size=20><color=#a2f252>Server wipes on</color><color=#42c8f4> " + nextMsg + " </color><color=#E9388C>(" + span + " from now)</color></size>";

                SendMessageOnWakeup(player, wakeMsg);

                if (bpData?.HasBPInfo(player.userID) ?? false)
                {
                    if (player?.blueprints == null)
                    {
                        PrintWarning("player has null blueprints!!");
                        return;
                    }
                    var bpInfo = bpData.GetBPInfo(player.userID);
                    if (bpInfo != null && bpInfo.unlocked != null && bpInfo.unlocked.Count > 0)
                    {
                        var unlockSB = new StringBuilder("Unlocking blueprints for " + player.displayName + " (" + bpInfo.unlocked.Count + " bps)" + Environment.NewLine);
                        for (int i = 0; i < bpInfo.unlocked.Count; i++)
                        {
                            var bp = bpInfo.unlocked[i];
                            var def = ItemManager.FindItemDefinition(bp);
                            if (def == null) continue;
                            if (!player.blueprints.HasUnlocked(def))
                            {
                                player.blueprints.Unlock(def);
                                if (player.IsAdmin) unlockSB.AppendLine("Unlocked: " + def.displayName.english + " (" + def.shortname + ", " + def.itemid + ")");
                            }
                        }
                        PrintWarning(unlockSB.ToString().TrimEnd());
                    }
                }



                DoVeteran(player, true);

                _cachedLegitPlayerIDs.Add(player.UserIDString);
            }
            finally
            {
                try
                {
                    if (watch.ElapsedMilliseconds > 5) PrintWarning(nameof(OnPlayerConnected) + " took: " + watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms");
                }
                finally { Pool.Free(ref watch); }
            }
        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null) return;

            RemoveImmunity(player.net.ID);
            var nearPlayers = Pool.GetList<BasePlayer>();
            try
            {
                Vis.Entities(player.transform.position, 550f, nearPlayers, 131072);
                var userIDStr = player?.UserIDString;
                if (nearPlayers.Count > 0)
                {
                    for (int i = 0; i < nearPlayers.Count; i++)
                    {
                        var ply = nearPlayers[i];
                        if (ply != null && !ply.IsAdmin && ply.IsConnected && ply?.UserIDString != userIDStr) AppearEntity(player, ply);
                    }
                }
            }
            finally { Pool.FreeList(ref nearPlayers); }
        }

        private readonly HashSet<NetworkableId> immuneEntities = new();
        private readonly Dictionary<NetworkableId, Timer> immuneTimers = new();

        private void GrantImmunity(BaseEntity entity, float duration = -1f)
        {
            if (entity != null && entity?.net != null && !entity.IsDestroyed) GrantImmunity(entity.net.ID, duration);
        }

        private void GrantImmunity(NetworkableId netID, float duration = -1f)
        {
            if (!immuneEntities.Add(netID))
            {
                PrintWarning("!immuneEntities.Add: " + netID);
                return;
            }

            if (duration > 0f)
            {
                Timer newTimer;
                newTimer = timer.Once(duration, () =>
                {
                    if (immuneTimers.TryGetValue(netID, out Timer getTimer)) immuneTimers.Remove(netID);
                    immuneEntities.Remove(netID);
                });
                immuneTimers[netID] = newTimer;
            }
        }

        private void RemoveImmunity(NetworkableId netID)
        {
            immuneEntities.Remove(netID);

            if (immuneTimers.TryGetValue(netID, out Timer getTimer))
            {
                getTimer.Destroy();
                immuneTimers.Remove(netID);
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp) { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime(); }

        [ChatCommand("immune")]
        private void cmdImmuneToggle(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            float time;
            if (args.Length < 1) time = -1f;
            else
            {
                if (args[0].Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    SendReply(player, "Stopped immunity");
                    RemoveImmunity(player.net.ID);
                    return;
                }
                if (!float.TryParse(args[0], out time))
                {
                    SendReply(player, "Not a float: " + args[0]);
                    return;
                }
            }
            GrantImmunity(player, time);
            SendReply(player, "GrantImmunity called with time: " + time);

        }

        [ChatCommand("newsbc")]
        private void cmdNewsBC(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            BroadcastNews();
        }


        private void SendGRules(BasePlayer target)
        {
            webrequest.Enqueue(grulesURL, null, (code, response) =>
            {
                if (code != 200) return;
                if (string.IsNullOrEmpty(response)) return;

                var broadSB = new StringBuilder();
                var listSB = Pool.GetList<StringBuilder>();

                var split = response.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (split != null && split.Length > 0)
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        var line = split[i];
                        var sb = new StringBuilder(line);

                        var newStr = sb.ToString();
                        if ((broadSB.Length + newStr.Length) >= 896)
                        {
                            listSB.Add(broadSB);
                            broadSB = new StringBuilder();
                        }
                        broadSB.AppendLine(newStr);
                    }
                }

                if (listSB.Count > 0)
                {
                    for (int i = 0; i < listSB.Count; i++)
                    {
                        var msg = listSB[i].ToString().TrimEnd();
                        if (target?.IsConnected ?? false) SendReply(target, msg);
                    }
                }

                if (broadSB.Length > 0 && (target?.IsConnected ?? false)) SendReply(target, broadSB.ToString().TrimEnd());

                Pool.FreeList(ref listSB);

            }, this);
        }

        private void BroadcastNews(BasePlayer target = null)
        {
            webrequest.Enqueue(NEWS_URL, null, (code, response) =>
            {
                if (code != 200) return;
                if (string.IsNullOrWhiteSpace(response))
                {
                    PrintError(nameof(BroadcastNews) + " got null/empty response!!!");
                    return;
                }





                var poolSb = Pool.Get<StringBuilder>();
                try
                {
                    var maxLength = 512;
                    if (response.Length > maxLength)
                    {
                        var subResponse = response.Substring(0, maxLength);
                        if ((subResponse.Length + 10) < response.Length) response = poolSb.Clear().Append(subResponse).Append("...\n<size=19>Read the entire post here:\ndiscord.gg/DUCnZhZ</size>").ToString();
                    }

                    response = poolSb.Clear().Append(PRISM_RAINBOW_TXT).Append(" | <color=#33ccff>NEWS</color>\n").Append(response).ToString();

                    response = _discordServerEmojiRegex.Replace(response, string.Empty);


                }
                finally { Pool.Free(ref poolSb); }

                var broadSB = new StringBuilder();

                var split = response.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (split != null && split.Length > 0)
                {
                    var sb = Pool.Get<StringBuilder>();

                    try
                    {
                        for (int i = 0; i < split.Length; i++)
                        {
                            var line = split[i];
                            if (line.StartsWith("//", StringComparison.OrdinalIgnoreCase)) continue; //commented line

                            if (line.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || line.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) continue;



                            sb.Clear().Append(line);

                            sb.Replace("!//", "//").Replace("{save.time}", SaveRestore.SaveCreatedTime.ToString("d")).Replace("{wipe.next}", GetWipeDate().ToString("d"));

                            sb.Replace("~~", string.Empty).Replace("***", string.Empty).Replace("**", string.Empty).Replace("*", string.Empty).Replace("```", string.Empty).Replace("``", string.Empty); //handle later


                            if (sb.Length > 1 && sb[0].Equals('#'))
                                sb.Replace("#", "<size=23>", 0, 1).Append("</size>");

                            broadSB.Append(sb.ToString()).Append(Environment.NewLine);
                        }
                    }
                    finally { Pool.Free(ref sb); }


                }

                if (broadSB.Length < 1)
                    return;

                if (broadSB.Length > 1)
                    broadSB.Length--; //trim newline

                var skip = Pool.Get<HashSet<string>>();

                try
                {
                    if (target == null)
                    {

                        for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                        {
                            var ply = BasePlayer.activePlayerList[i];
                            if (ply == null || !ply.IsConnected || !newsLevel.TryGetValue(ply.UserIDString, out int level) || level > 1 || rarityRng.Next(0, 101) < 50) continue;

                            skip.Add(ply.UserIDString);
                            PrintWarning("Skipping news for: " + ply?.displayName + " (" + ply?.UserIDString + ") (level: " + level + ")");
                        }

                    }


                    if (target == null)
                    {
                        var msg = broadSB.ToString();

                        for (int j = 0; j < BasePlayer.activePlayerList.Count; j++)
                        {
                            var ply = BasePlayer.activePlayerList[j];
                            if (ply == null || !ply.IsConnected || skip.Contains(ply.UserIDString))
                                continue;

                            SendReply(ply, msg);
                        }
                    }
                    else if (target?.IsConnected ?? false) SendReply(target, broadSB.ToString());
                }
                finally
                {
                    try { skip?.Clear(); } //unsure if necessary
                    finally { Pool.Free(ref skip); }
                }
            }, this);

        }

        private void SendMessage(BasePlayer player, string prefix, string message, string chatUserID = "0")
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected) return;
            var msg = (!string.IsNullOrEmpty(prefix)) ? prefix + ": " + message : message;
            player.SendConsoleCommand("chat.add", string.Empty, chatUserID, msg);
        }


        [ChatCommand("pos")]
        private void cmdPos(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            SendReply(player, "Your position is: X " + player.transform.position.x + ", Y " + player.transform.position.y + ", Z " + player.transform.position.z + ", rotation: " + player.transform.rotation);
            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var currentRotEuler = Quaternion.Euler(input?.current?.aimAngles ?? Vector3.zero);
            var altEuler = player?.transform?.eulerAngles ?? Vector3.zero;
            var altEuler2 = input?.current?.aimAngles ?? Vector3.zero;
            SendReply(player, currentRot + " <-- current rotation? " + ", current.aimAngles: " + input.current.aimAngles + "\n" + currentRotEuler);
            SendReply(player, "Alt euler: " + altEuler + "\n" + altEuler2);
        }


        [ChatCommand("slots")]
        private void cmdSlots(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var item = player?.GetActiveItem() ?? null;
            if (item == null) return;
            var heldProj = item?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
            if (heldProj == null) return;



            if (item.contents == null)
            {
                SendReply(player, "This Item does not support any slots, but lets try anyway!");
                Puts("unsupported slot item");
                item.contents = new ItemContainer
                {
                    itemList = new List<Item>()
                };
            }

            var AK = ItemManager.CreateByName("rifle.m39", 1);

            item.contents.allowedContents = AK.contents.allowedContents;
            item.contents.availableSlots = AK.contents.availableSlots;
            item.contents.capacity = AK.contents.capacity;

            foreach (var slot in AK.contents.availableSlots)
            {
                Puts(slot.ToString());
            }

            Puts(AK.contents.capacity + " <-- capacity, now: " + item.contents.capacity);
            item.contents.Save();

            heldProj.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            RemoveFromWorld(AK);
        }

        [ChatCommand("slotcontent")]
        private void cmdSlotContent(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var item = player?.GetActiveItem() ?? null;
            if (item == null) return;


            if (item.contents != null)
            {
                SendReply(player, "this item has contents!!");
                return;
            }

            var itemModContainer = item?.info?.GetComponent<ItemModContainer>();
            if (itemModContainer == null)
            {
                SendReply(player, nameof(itemModContainer) + " has to be created...");
                itemModContainer = item.info.gameObject.AddComponent<ItemModContainer>();
                SendReply(player, nameof(itemModContainer) + " created. may need to respawn item.");
            }

            item.contents = new ItemContainer();



            var contents = item.contents;

            //parent item necessary?
            contents.ServerInitialize(item, 1); //large backpack -- uh, this thing <-- over there, before the --, idk what that comment means. but i wrote it.
            contents.GiveUID();
            contents.entityOwner = player?.GetHeldEntity();

            item.MarkDirty();

            if (player?.GetHeldEntity() != null)
                player.GetHeldEntity().SendNetworkUpdate();

            SendReply(player, "force created contents lol good luck");

        }

        [ChatCommand("ammo")]
        private void cmdAmmo(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var projectile = player?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
            var flamer = player?.GetHeldEntity()?.GetComponent<FlameThrower>() ?? null;
            if (projectile == null && flamer == null)
            {
                SendReply(player, "You do not appear to be holding a gun/flamethrower!");
                return;
            }
            if (projectile == null && flamer != null)
            {
                if (flamer.ammo < flamer.maxAmmo) flamer.ammo = flamer.maxAmmo;
                else flamer.ammo *= 2;
                flamer.SendNetworkUpdate();
            }
            else
            {
                if (projectile.primaryMagazine.contents < projectile.primaryMagazine.capacity) projectile.primaryMagazine.contents = projectile.primaryMagazine.capacity;
                else projectile.primaryMagazine.contents *= 2;
                projectile.SendNetworkUpdate();
            }
        }


        [ChatCommand("curcond")]
        private void cmdCurcond(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var heldItem = player?.GetActiveItem() ?? null;
            if (heldItem == null) return;
            var __maxCond = heldItem._maxCondition;
            var __cond = heldItem._condition;
            var maxCond = heldItem.maxCondition;
            var cond = heldItem.condition;
            SendReply(player, "Cond is: " + __cond + "/" + __maxCond + ", " + cond + "/" + maxCond);
        }

        [ChatCommand("bcost")]
        private void cmdBuildCost(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var watch = Stopwatch.StartNew();
            var inputt = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(inputt.current.aimAngles) * Vector3.forward;
            Ray ray = new(player.eyes.position, currentRot);
            var blockID = 0u;
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 100f, CONSTRUCTION_LAYER)) blockID = hitt.GetEntity()?.GetComponent<BuildingBlock>()?.buildingID ?? 0u;
            if (blockID == 0)
            {
                SendReply(player, "Failed to find a block ID!");
                return;
            }
            var familyBlocks = GetBlocksFromID(blockID);
            var costs = new Dictionary<string, double>();
            foreach (var ent in familyBlocks)
            {
                var bloc = ent as BuildingBlock;
                if (bloc == null) continue;
                var twigCost = bloc?.blockDefinition?.grades[0]?.CostToBuild() ?? null;
                var gradeCost = bloc?.BuildCost() ?? null;
                if (twigCost != null)
                {
                    for (int i = 0; i < twigCost.Count; i++)
                    {
                        var cost = twigCost[i];
                        var engName = ItemManager.FindItemDefinition(cost.itemid)?.displayName?.english ?? "Unknown";
                        if (!costs.TryGetValue(engName, out double outAmount)) costs[engName] = cost.amount;
                        else costs[engName] += cost.amount;
                    }
                }
                if (gradeCost != null)
                {
                    for (int i = 0; i < gradeCost.Count; i++)
                    {
                        var cost = gradeCost[i];
                        var engName = ItemManager.FindItemDefinition(cost.itemid)?.displayName?.english ?? "Unknown";
                        if (!costs.TryGetValue(engName, out double outAmount)) costs[engName] = cost.amount;
                        else costs[engName] += cost.amount;
                    }
                }

                var linkEnt = ent?.FindLinkedEntity<BaseEntity>() ?? null;
                if (linkEnt != null && linkEnt.ShortPrefabName != bloc.ShortPrefabName)
                {
                    var findItem = ItemManager.FindItemDefinition(linkEnt.ShortPrefabName);
                    if (findItem != null)
                    {
                        var findCost = findItem?.Blueprint?.ingredients ?? null;
                        if (findCost != null && findCost.Count > 0)
                        {
                            for (int i = 0; i < findCost.Count; i++)
                            {
                                var ingredient = findCost[i];
                                if (ingredient == null) continue;
                                var engName = ItemManager.FindItemDefinition(ingredient.itemid)?.displayName?.english ?? "Unknown";
                                if (!costs.TryGetValue(engName, out double outAmount)) costs[engName] = ingredient.amount;
                                else costs[engName] += ingredient.amount;
                            }
                        }
                    }
                }
                var door = linkEnt as Door;
                if (door != null)
                {

                    var doorItem = ItemManager.FindItemDefinition(door.ShortPrefabName);
                    var lockSlot = door?.GetSlot(BaseEntity.Slot.Lock) ?? null;
                    var lockItem = ItemManager.FindItemDefinition(lockSlot?.ShortPrefabName ?? string.Empty);
                    if (doorItem != null)
                    {
                        var doorCost = doorItem?.Blueprint?.ingredients ?? null;
                        if (doorCost != null && doorCost.Count > 0)
                        {
                            for (int i = 0; i < doorCost.Count; i++)
                            {
                                var ingredient = doorCost[i];
                                if (ingredient == null) continue;
                                var engName = ItemManager.FindItemDefinition(ingredient.itemid)?.displayName?.english ?? "Unknown";
                                if (!costs.TryGetValue(engName, out double outAmount)) costs[engName] = ingredient.amount;
                                else costs[engName] += ingredient.amount;
                            }
                        }
                    }
                    if (lockItem != null)
                    {
                        var lockCost = lockItem?.Blueprint?.ingredients ?? null;
                        if (lockCost != null && lockCost.Count > 0)
                        {
                            for (int i = 0; i < lockCost.Count; i++)
                            {
                                var ingredient = lockCost[i];
                                if (ingredient == null) continue;
                                var engName = ItemManager.FindItemDefinition(ingredient.itemid)?.displayName?.english ?? "Unknown";
                                if (!costs.TryGetValue(engName, out double outAmount)) costs[engName] = ingredient.amount;
                                else costs[engName] += ingredient.amount;
                            }
                        }
                    }
                }
            }
            if (costs == null || costs.Count < 1)
            {
                SendReply(player, "Failed to get costs!");
                return;
            }
            var costSB = new StringBuilder();
            foreach (var kvp in costs) costSB.AppendLine(kvp.Key + ": " + kvp.Value.ToString("N0"));
            SendReply(player, costSB.ToString().TrimEnd());
            watch.Stop();
            Puts(watch.Elapsed.TotalMilliseconds + "ms to calculate building cost for " + familyBlocks.Count.ToString("N0") + " blocks");
        }


        [ChatCommand("bgradeall")]
        private void cmdBuildGrade(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length <= 0) return;

            var watch = Stopwatch.StartNew();
            if (!int.TryParse(args[0], out int grade)) return;

            if (grade >= 5 || grade < 0)
            {
                SendReply(player, "Invalid grade supplied: " + grade + ", valid: 1-4");
                return;
            }

            var inputt = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(inputt.current.aimAngles) * Vector3.forward;
            Ray ray = new(player.eyes.position, currentRot);
            var blockID = 0u;

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 100f, CONSTRUCTION_LAYER))
                blockID = (hitt.GetEntity() as BuildingBlock)?.buildingID ?? 0u;

            if (blockID == 0)
            {
                SendReply(player, "Failed to find a block ID!");
                return;
            }

            var skin = 0UL;

            if (args.Length > 1 && !ulong.TryParse(args[1], out skin))
                SendReply(player, "Could not use as skin: " + args[1]);

            var familyBlocks = GetBlocksFromID(blockID);
            Puts("family blocks count: " + familyBlocks.Count);

            foreach (var bloc in familyBlocks)
            {

                bloc.skinID = skin;

                bloc.ChangeGrade((BuildingGrade.Enum)grade, true);
            }

            watch.Stop();
            Puts("took: " + watch.Elapsed.TotalMilliseconds + "ms for " + familyBlocks.Count + " blocks");
        }

        private ListHashSet<BuildingBlock> GetBlocksFromID(uint buildingId)
        {
            return BuildingManager.server?.GetBuilding(buildingId)?.buildingBlocks;
        }

        [ChatCommand("buildhealall")]
        private void cmdBuildHeal(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var startTime = UnityEngine.Time.realtimeSinceStartup;
            var blockID = GetLookAtEntity(player)?.GetComponent<BuildingBlock>()?.buildingID ?? 0;
            if (blockID == 0) return;
            var familyBlocks = GetBlocksFromID(blockID);
            Puts("count for famiyl: " + familyBlocks.Count);
            foreach (var ent in familyBlocks)
            {
                var bloc = ent as BuildingBlock;
                if (bloc == null || (bloc?.IsDestroyed ?? true) || (bloc?.Health() ?? 0f) == (bloc?.MaxHealth() ?? -1f)) continue;
                bloc.health = bloc.MaxHealth();
                bloc.InitializeHealth(bloc.MaxHealth(), bloc.MaxHealth());
                bloc.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            }
            Puts("took: " + (UnityEngine.Time.realtimeSinceStartup - startTime) + "s for " + familyBlocks.Count + " blocks");
        }


        [ChatCommand("respawnloot")]
        private void cmdRespawnLoot(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var lookAt = GetLookAtEntity(player, 7f, 1) as LootContainer;

            if (lookAt == null || lookAt.IsDestroyed)
            {
                SendReply(player, "No ent");
                return;
            }

            SendReply(player, "Calling SpawnLoot()");
            lookAt.SpawnLoot();
            SendReply(player, "Called");


        }

        [ChatCommand("bskina")]
        private void cmdBuildSkin(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var lookAt = GetLookAtEntity(player, 5f);

            if (lookAt == null || lookAt.IsDestroyed)
            {
                SendReply(player, "No ent");
                return;
            }

            if (args.Length < 1)
            {
                SendReply(player, "skinID: " + lookAt.skinID);
                return;
            }


            if (!ulong.TryParse(args[0], out ulong skinId))
            {
                SendReply(player, "not a ulong: " + args[0]);
                return;
            }

            lookAt.skinID = skinId;
            lookAt.SendNetworkUpdate();


        }


        private readonly HashSet<string> frozenPlayers = new();
        [ChatCommand("freeze")]
        private void cmdFreeze(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length <= 0)
            {
                SendReply(player, "You must supply a player name!");
                return;
            }
            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "Unable to find a player with that name!");
                return;
            }
            if (!frozenPlayers.Contains(target.UserIDString)) FreezePlayer(target.UserIDString);
            else UnfreezePlayer(target.UserIDString);

            SendReply(player, "Frozen: " + frozenPlayers.Contains(target.UserIDString));
        }

        private void FreezePlayer(string userID, float duration = -1f)
        {
            if (string.IsNullOrEmpty(userID) || duration == 0.0f) throw new ArgumentNullException();
            if (!frozenPlayers.Contains(userID)) frozenPlayers.Add(userID);
            if (duration > 0.0f) InvokeHandler.Invoke(ServerMgr.Instance, () => UnfreezePlayer(userID), duration);
        }

        private void UnfreezePlayer(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException();
            if (frozenPlayers.Contains(userID)) frozenPlayers.Remove(userID);
        }

        private TimeSpan GetConnectedTimeSpan(BasePlayer player)
        {
            // return TimeSpan.MinValue;
            var now = DateTime.UtcNow;
            var val = (player == null || !player.IsConnected) ? TimeSpan.Zero : now - now.AddSeconds(-Mathf.Clamp(player.net.connection.GetSecondsConnected(), 0, int.MaxValue));
            if (val <= TimeSpan.Zero) PrintWarning("getconnectedtimespan has .zero or less: " + val);
            return val;
        }

        private TimeSpan GetConnectedTimeSpanS(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return TimeSpan.MinValue;
            return GetConnectedTimeSpan(FindPlayerByID(userID));
        }


        private JsonSerializerSettings jsonsettings;


        [ConsoleCommand("steam.getstats")]
        private void consoleGetSteamStats(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1)
            {
                arg.ReplyWith("You must supply a target name!");
                return;
            }
            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                arg.ReplyWith("Failed to find a player by the name: " + args[0]);
                return;
            }
            var stats = SteamStatsAPI?.Call<Dictionary<string, long>>("GetPlayerStats", target) ?? null;
            if (stats == null || stats.Count < 1)
            {
                arg.ReplyWith("Failed to find stats for: " + target.displayName);
                return;
            }
            var kvpSB = new StringBuilder();
            foreach (var kvp in stats) kvpSB.AppendLine(kvp.Key + " : " + kvp.Value);

            Puts("Stats for: " + target.displayName + " " + kvpSB.ToString());
        }

        [ChatCommand("getstats")]
        private void cmdSwStats(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args == null || args.Length < 1)
            {
                SendReply(player, "You must supply a name!");
                return;
            }

            var target = FindIPlayer(args[0]);
            if (target == null)
            {
                SendReply(player, "Failed to find a player or found more than one player with the name/ID/IP: " + args[0]);
                return;
            }
            var stats = SteamStatsAPI?.Call<Dictionary<string, long>>("GetPlayerStats", target) ?? null;
            if (stats == null || stats.Count < 1)
            {
                SendReply(player, "Failed to get stats for player; " + target.Name);
                return;
            }
            var statsSB = new StringBuilder();
            var statsSB2 = new StringBuilder();
            foreach (var kvp in stats)
            {
                if (statsSB.Length > 400)
                {
                    if (string.IsNullOrEmpty(kvp.Key)) statsSB2.AppendLine("No : Stat");
                    else statsSB2.AppendLine(kvp.Key + " : " + kvp.Value);
                }
                else
                {
                    if (string.IsNullOrEmpty(kvp.Key)) statsSB.AppendLine("No : Stat");
                    else statsSB.AppendLine(kvp.Key + " : " + kvp.Value);
                }

            }
            SendReply(player, statsSB.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
            if (statsSB2.Length > 0) SendReply(player, statsSB2.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }


        private readonly Dictionary<ulong, BaseEntity> lastHeldAttached = new();
        [ChatCommand("rotheld")]
        private void cmdRotHeld(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            BaseEntity ent = null;
            if (lastHeldAttached.ContainsKey(player.userID))
            {
                ent = lastHeldAttached[player.userID];
            }
            if (args.Length <= 1)
            {
                SendReply(player, "Wrong args, current rot: " + (ent?.transform?.localEulerAngles ?? Vector3.zero));
                return;
            }

            if (ent == null || (ent?.IsDestroyed ?? true))
            {
                SendReply(player, "No ent!");
                return;
            }
            var arg0 = args[0].ToLower();
            var arg1 = args[1];
            if (!float.TryParse(arg1, out float arg1f))
            {
                SendReply(player, "NO PARSE: " + arg1);
                return;
            }
            if (arg0 != "x" && arg0 != "y" && arg0 != "z")
            {
                SendReply(player, "No valid: " + arg0);
                return;
            }
            var currentRot = ent?.transform?.localEulerAngles ?? Vector3.zero;
            if (arg0 == "x") currentRot.x = arg1f;
            if (arg0 == "y") currentRot.y = arg1f;
            if (arg0 == "z") currentRot.z = arg1f;
            ent.transform.localEulerAngles = new Vector3(currentRot.x, currentRot.y, currentRot.z);
            //ent.TransformChanged();
            ent.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            SendReply(player, "New rot: " + currentRot);
        }

        [ChatCommand("attachheld")]
        private void cmdAttachHeld(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var heldPrefab = player?.GetHeldEntity()?.PrefabName ?? string.Empty;
            if (lastHeldAttached.ContainsKey(player.userID))
            {
                var lastEnt = lastHeldAttached[player.userID];
                if (!lastEnt.IsDestroyed)
                {
                    lastEnt.Kill(BaseNetworkable.DestroyMode.None);
                    lastHeldAttached.Remove(player.userID);
                }
                SendReply(player, "Destroyed last ent!");
                return;
            }
            if (string.IsNullOrEmpty(heldPrefab))
            {
                SendReply(player, "Invalid held prefab!");
                return;
            }

            SendReply(player, heldPrefab);

            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;
            Ray ray = new(player.eyes.position, currentRot);
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 4f))
            {
                var type = hitt.GetEntity()?.GetType()?.ToString() ?? hitt.GetCollider()?.GetType()?.ToString() ?? "Unknown Type";
                var prefabName = hitt.GetEntity()?.PrefabName ?? hitt.GetCollider()?.GetComponent<MeshCollider>()?.sharedMesh?.name ?? hitt.GetCollider()?.name ?? "Unknown Prefab";
                var ent = hitt.GetEntity() ?? null;
                if (ent == null) return;
                if (!prefabName.Contains("frame"))
                {
                    PrintWarning("not frame: " + prefabName);
                    return;
                }
                var centerPos = ent?.CenterPoint() ?? Vector3.zero;
                var rotation = ent?.transform?.rotation ?? Quaternion.identity;
                if (centerPos == Vector3.zero || rotation == Quaternion.identity)
                {
                    PrintWarning("Bad center/rot");
                    return;
                }
                var heldEnt = GameManager.server.CreateEntity(heldPrefab, centerPos);
                if (heldEnt != null)
                {
                    heldEnt.enableSaving = false;
                    heldEnt.Spawn();
                    heldEnt.enableSaving = false;
                    heldEnt.SetParent(ent);
                    //   heldEnt.transform.localEulerAngles = ent.transform.eulerAngles;
                    heldEnt.transform.localEulerAngles = new Vector3(70, 0, 350);
                    heldEnt.transform.localPosition = new Vector3(0, 1f, 0.0475f);
                    lastHeldAttached[player.userID] = heldEnt;
                }
            }
        }

        private object OnEntityGroundMissing(BaseEntity entity)
        {
            if (entity == null || entity?.net == null) return null;
            if (ignoreStability.Contains(entity.net.ID)) return false;
            var cupboard = entity as BuildingPrivlidge;
            if (cupboard != null) Puts("OnEntityGroundMissing for cupboard (" + GetDisplayNameFromID(entity.OwnerID) + ") @ " + (entity?.transform?.position ?? Vector3.zero));
            return null;
        }

        private readonly HashSet<NetworkableId> ignoreStability = new();


        [ChatCommand("heldinfo2")]
        private void cmdHeldInfo2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var heldItem = player?.GetActiveItem() ?? null;
            if (heldItem == null)
            {
                SendReply(player, "No item found!");
                return;
            }
            var heldPrefab = heldItem?.GetHeldEntity()?.ShortPrefabName ?? "Unknown";
            var heldPrefabFull = heldItem?.GetHeldEntity()?.PrefabName ?? "Unknown";
            var heldName = heldItem?.info?.displayName?.english ?? "Unknown";
            var heldShort = heldItem?.info?.shortname ?? "Unknown";
            var heldEnt = heldItem?.GetHeldEntity() ?? null;
            var repeatDelay = (heldEnt == null) ? 0.0f : (heldEnt?.GetComponent<BaseProjectile>()?.repeatDelay ?? heldEnt?.GetComponent<BaseMelee>()?.repeatDelay ?? 0.0f);
            var projObjName = (heldEnt == null) ? string.Empty : heldEnt?.GetComponent<BaseProjectile>()?.primaryMagazine?.ammoType?.GetComponent<ItemModProjectile>()?.projectileObject?.resourcePath ?? string.Empty;
            SendReply(player, heldPrefab + "\n" + heldPrefabFull + "\n" + heldName + "\n" + heldShort + "\nRepeat delay: " + repeatDelay + "\nAmmo prefab name: " + projObjName + "\nHeld type: " + (heldEnt?.GetType()?.ToString() ?? string.Empty) + "\nHas Steam Item: " + (heldItem?.info?.steamItem != null));
        }



        [ChatCommand("unparent")]
        private void cmdUnparent(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            player.SetParent(null, true, true);
        }

        private Dictionary<ulong, BaseEntity> activeSetParent = new();
        [ChatCommand("setparent")]
        private void cmdSetParent(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 1 || !int.TryParse(args[0], out int layer)) layer = -1;

            var lookAt = GetLookAtEntity(player, 250f, layer);
            if (lookAt == null || lookAt.IsDestroyed)
            {
                SendReply(player, "no ent found");
                return;
            }

            if (!activeSetParent.TryGetValue(player.userID, out BaseEntity active))
            {
                activeSetParent[player.userID] = lookAt;
                SendReply(player, "will parent next ent to: " + lookAt.ShortPrefabName);
            }
            else
            {
                if (active == null || active.IsDestroyed)
                {
                    activeSetParent.Remove(player.userID);
                    return;
                }
                if (active == lookAt)
                {
                    SendReply(player, "cannot parent same entity!!");
                    return;
                }
                active.SetParent(lookAt, true, true);
                SendReply(player, active.ShortPrefabName + " now has parent of: " + lookAt.ShortPrefabName);
            }

        }

        private object OnNpcRadioChatter(ScientistNPC npc)
        {
            if (UnityEngine.Random.Range(0, 101) <= 70)
                return true; //LESS CHATTER

            return null;
        }

        //Noob adjustments for targeting

        private object OnNpcTarget(HumanNPC npcPlayer, BasePlayer player)
        {
            if (npcPlayer == null || player) return null;

            var sash = player?.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash) ?? false;

            if (!sash)
            {
                if (npcPlayer?.lastAttacker == player) return null;

                return true;
            }

            return null;
        }

        private object CanHelicopterTarget(PatrolHelicopterAI heli, BasePlayer player)
        {
            if (heli == null || player == null) return null;

            var sash = player?.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash) ?? false;

            if (!sash)
            {
                if (heli?.helicopterBase?.lastAttacker == player) return null;
                return false;
            }
            return null;
        }

        private object CanHelicopterStrafeTarget(PatrolHelicopterAI heli, BasePlayer target)
        {
            if (heli == null || target == null) return null;

            var sash = target?.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash) ?? false;
            if (!sash)
            {
                if (heli?.helicopterBase?.lastAttacker == target) return null;
                return false;
            }

            return null;
        }

        private object OnHelicopterTarget(HelicopterTurret turret, BaseCombatEntity entity)
        {
            if (turret == null || entity == null) return null;

            var player = entity as BasePlayer;
            if (player == null) return null;

            var sash = player?.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash) ?? false;

            if (!sash)
            {
                if (turret?._heliAI?.helicopterBase?.lastAttacker == player) return null;
                return true;
            }

            return null;
        }

        //End noob adjustmenets for targeting


        private readonly Dictionary<string, HashSet<NetworkableId>> noVisEntities = new();

        [ChatCommand("invis")]
        private void cmdInvis(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var getLook = GetLookAtEntity(player);
            if (getLook == null || getLook?.net == null)
            {
                SendReply(player, "Bad/no entity");
                return;
            }

            bool makeVis = false;
            if (args.Length > 0 && !bool.TryParse(args[0], out makeVis)) makeVis = false;
            if (player.net == null || player.net.connection == null)
            {
                SendReply(player, "NO CON!");
                return;
            }

            if (makeVis) AppearEntity(getLook, player);
            else DisappearEntity(getLook, player);

            var netID = getLook.net.ID;
            SendReply(player, (makeVis ? "Show " : "Hide: ") + getLook.ShortPrefabName + ", " + netID);
        }

        private void DisappearEntity(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || entity?.net == null || player == null || player.net == null || player.net.connection == null) return;

            var conList = Pool.GetList<Connection>();
            try
            {
                conList.Add(player.net.connection);

                if (!noVisEntities.TryGetValue(player.UserIDString, out HashSet<NetworkableId> outList)) outList = new HashSet<NetworkableId>();
                if (!outList.Contains(entity.net.ID)) outList.Add(entity.net.ID);

                noVisEntities[player.UserIDString] = outList;

                entity.OnNetworkSubscribersLeave(conList);
            }
            finally { Pool.FreeList(ref conList); }
        }

        private void AppearEntity(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || entity?.net == null || player == null || player.net == null || player.net.connection == null) return;

            var conList = Pool.GetList<Connection>();
            try
            {
                conList.Add(player.net.connection);

                if (noVisEntities.TryGetValue(player.UserIDString, out HashSet<NetworkableId> outList) && outList.Contains(entity.net.ID)) noVisEntities[player.UserIDString].Remove(entity.net.ID);

                entity.OnNetworkSubscribersEnter(conList);
            }
            finally { Pool.FreeList(ref conList); }
        }

        [ChatCommand("invlist")]
        private void cmdInvList(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var itemSB = new StringBuilder();
            var itemSB2 = new StringBuilder();
            var allItems = player?.inventory?.AllItems() ?? null;
            if (allItems == null || allItems.Length < 1)
            {
                SendReply(player, "No items found!");
                return;
            }
            for (int i = 0; i < allItems.Length; i++)
            {
                var item = allItems[i];
                if (item == null) continue;
                var containerName = item?.parent == null ? string.Empty : (item.parent == player.inventory.containerMain ? "main" : item.parent == player.inventory.containerBelt ? "belt" : item.parent == player.inventory.containerWear ? "wear" : string.Empty);
                var appendMsg = item.info.displayName.english + " x" + item.amount.ToString("N0") + " (" + item.position + ", parent: " + containerName + ")  ";
                if (itemSB.Length < 1024) itemSB.Append(appendMsg);
                else itemSB2.Append(appendMsg);
            }
            SendReply(player, itemSB.ToString().TrimEnd());
            if (itemSB2.Length > 0) SendReply(player, itemSB2.ToString().TrimEnd());
        }

        private HashSet<BaseEntity> GetPlayerEntities(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            if (!ulong.TryParse(userId, out ulong uID)) return null;
            return new HashSet<BaseEntity>(BaseEntity.saveList?.Where(p => (p?.OwnerID ?? 0) == uID) ?? null);
        }

        private bool IsBanned(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            if (BanSystem != null && BanSystem.IsLoaded) return BanSystem?.Call<bool>("IsBanned", userID) ?? false;
            return covalence.Players?.FindPlayerById(userID)?.IsBanned ?? false;
        }

        private DateTime GetLastConnection(ulong userId)
        {
            return GetLastConnectionS(userId.ToString());
        }
        private DateTime GetLastConnectionS(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            return Playtimes?.Call<DateTime>("GetLastConnection", userId) ?? DateTime.MinValue;
        }

        private DateTime GetFirstConnection(ulong userId)
        {
            return GetFirstConnectionS(userId.ToString());
        }

        private DateTime GetFirstConnectionS(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            return Playtimes?.Call<DateTime>("GetFirstConnection", userId) ?? DateTime.MinValue;
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

        private void CheckGameBans(string ID, IPlayer player = null)
        {
            if (string.IsNullOrEmpty(ID)) return;
            if (!ulong.TryParse(ID, out ulong userID)) return;

            var url = "https://twitter.com/search?l=&q=steamcommunity.com%2Fprofiles%2F" + ID + "&src=typd&qf=off&lang=en";

            webrequest.Enqueue(url, string.Empty, (code, response) =>
            {
                if (code != 200) PrintWarning("Didn't recieve 200 when checking " + ID + " for game bans (" + code + ")");
                var str = string.Empty;
                var pName = GetDisplayNameFromID(ID);
                if (response.Contains("was banned", CompareOptions.OrdinalIgnoreCase))
                {
                    str = "Game banned: " + pName + " (" + ID + ")";
                    PrintWarning("Game banned: " + pName + " (" + ID + ")");
                }
                else str = "Not game banned: " + GetDisplayNameFromID(ID) + " (" + ID + ")";
                if (player != null && player.IsConnected) player.Message(str);
            }, this, Core.Libraries.RequestMethod.GET);
        }

        private void cmdGameBan(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "You must supply a steam ID!");
                return;
            }
            if (!ulong.TryParse(args[0], out ulong userID))
            {
                player.Message("Not a ulong: " + args[0]);
                return;
            }
            player.Message("Checking ban info...");
            CheckGameBans(args[0], player);
        }

        private class PlayerInfo
        {
            public string UserIDString { get; set; }
            public string Name { get; set; }
            public string IPAddress { get; set; }
            public string ClanTag { get; set; }
            public string ClientLanguage { get; set; }
            public List<string> Aliases { get; set; } = new List<string>();
            public DateTime LastConnection { get; set; } = DateTime.MinValue;
            public DateTime FirstConnection { get; set; } = DateTime.MinValue;
            public bool Alive { get; set; }
            public bool Sleeping { get; set; }
            public bool Connected { get; set; }
            public int PlacedEntities { get; set; }
            public TimeSpan ConnectionTime { get; set; }
            public double ConnectedSeconds { get { return ConnectionTime.TotalSeconds; } }
            public float ViolationLevel { get; set; }
            public int ViolationKicks { get; set; }
            public int VACs { get; set; }
            public int GameBans { get; set; }
            public bool CommunityBanned { get; set; }
            public bool TradeBanned { get; set; }
            public int DaysSinceSteamBan { get; set; }
            public int TicketReportedCount { get; set; }
            public bool Banned { get; set; }
            public string FirstCountry { get; set; }
            public string LastCountry { get; set; }
            public double AccountDays { get; set; }
            public double TotalPlayTime { get; set; }


            public PlayerInfo(IPlayer player)
            {
                if (player == null) throw new ArgumentNullException(nameof(player));
                if (ins == null)
                {
                    Interface.Oxide.LogWarning("Compilation Instance is null on new PlayerInfo!");
                    throw new NullReferenceException(nameof(ins));
                }


                UserIDString = player.Id;
                Name = player.Name;

                if (player.IsConnected)
                {
                    try { ClientLanguage = player.Language.ToString(); }
                    catch (CultureNotFoundException ex) //oxide except
                    {
                        Interface.Oxide.LogError(ex.ToString());
                        ClientLanguage = (player?.Object as BasePlayer)?.net?.connection?.info?.GetString("global.language") ?? string.Empty;
                    }
                }
                else ClientLanguage = string.Empty;

                IPAddress = player.IsConnected ? player.Address : ins?.getIPString(player) ?? string.Empty;
                Aliases = ins?.GetAliases(player.Id) ?? new List<string>();
                FirstConnection = ins?.Playtimes?.Call<DateTime>("GetFirstConnection", player.Id) ?? DateTime.MinValue;
                LastConnection = player.IsConnected ? DateTime.UtcNow : ins?.Playtimes?.Call<DateTime>("GetLastConnection", player.Id) ?? DateTime.MinValue;
                Connected = player?.IsConnected ?? false;
                Banned = ins?.IsBanned(player.Id) ?? false;

                ClanTag = ins?.Clans?.Call<string>("GetClanOf", player.Id) ?? string.Empty;
                FirstCountry = ins?.OneCountry?.Call<string>("GetFirstCountry", player.Id) ?? string.Empty;
                LastCountry = ins?.OneCountry?.Call<string>("GetLastCountry", player.Id) ?? string.Empty;

                var timePlayed = ins?.Playtimes?.Call<TimeSpan>("GetTimePlayed", player.Id) ?? TimeSpan.MinValue;
                TotalPlayTime = timePlayed.TotalMinutes;



                var pBans = ins?.SteamBansAPI?.Call<Dictionary<string, object>>("GetPlayerBans", player.Id) ?? null;
                if (pBans != null && pBans.Count > 0)
                {
                    foreach (var kvp in pBans)
                    {
                        var key = kvp.Key;
                        var valueStr = kvp.Value.ToString();

                        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(valueStr))
                            continue;

                        if (int.TryParse(valueStr, out int valueInt))
                        {
                            if (key == "NumberOfVACBans") VACs += valueInt;
                            if (key == "NumberOfGameBans") GameBans += valueInt;
                            if (key == "DaysSinceLastBan") DaysSinceSteamBan = valueInt;
                        }

                        if (key == "CommunityBanned" && valueStr == "True") CommunityBanned = true;
                        if (key == "EconomyBan" && valueStr != "none") TradeBanned = true;
                    }
                }

                if (ulong.TryParse(player.Id, out ulong idUL))
                {
                    var count = 0;

                    foreach (var ent in BaseEntity.saveList)
                        if (ent?.OwnerID == idUL)
                            count++;

                    PlacedEntities = count;
                }

                var creationTime = ins?.SteamStatsAPI?.Call<double>("GetCreationTime", player.Id) ?? -1;
                if (creationTime > 0)
                    AccountDays = (DateTime.Now - UnixTimeStampToDateTime(creationTime)).TotalDays;


                var obj = player?.Object as BasePlayer;
                if (obj?.gameObject != null)
                {
                    ConnectionTime = (obj?.IsConnected ?? false) ? (ins?.GetConnectedTimeSpan(obj) ?? TimeSpan.Zero) : TimeSpan.Zero;
                    Alive = !(obj?.IsDead() ?? true);
                    Sleeping = obj?.IsSleeping() ?? false;
                    ViolationLevel = obj?.violationLevel ?? 0;
                    ViolationKicks = AntiHack.GetKickRecord(obj);
                }
            }

            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public PlayerInfo(string userID, string playerName)
            {
                UserIDString = userID;
                Name = playerName;
            }

            public PlayerInfo() { }
        }

        private string GetPInfoJSON(string targetNameOrId)
        {
            if (string.IsNullOrEmpty(targetNameOrId)) return string.Empty;
            var target = FindConnectedPlayer(targetNameOrId, true);
            if (target == null) return string.Empty;
            return JsonConvert.SerializeObject(new PlayerInfo(target), Formatting.Indented);
        }

        private string GetListPlayers(string targetsNamesOrIDsOrIps)
        {
            var sb = new StringBuilder();
            foreach (var player in FindIPlayers(targetsNamesOrIDsOrIps))
            {
                var id = player.Id;
                sb.AppendLine(GetDisplayNameFromID(id) + " (" + id + ")");
            }
            return sb.ToString().TrimEnd();
        }


        [ConsoleCommand("pinfo.json")]
        private void consolePinfo(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;

            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1)
            {
                SendReply(arg, "You must supply arguments!");
                return;
            }

            var argStr = string.Join(" ", args);

            var target = FindPlayerByPartialName(argStr, true)?.IPlayer ?? FindConnectedPlayer(argStr, true);
            if (target == null)
            {
                SendReply(arg, "Failed to find a player with the name, ID or IP: " + args[0]);
                return;
            }

            arg.ReplyWith(JsonConvert.SerializeObject(new PlayerInfo(target), Formatting.Indented));
        }

        [ConsoleCommand("pinfo.all")]
        private void consolePinfoAll(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1) return;

            var targets = FindIPlayers(args[0]);
            if (targets == null || !targets.Any())
            {
                SendReply(arg, "No targets found with the name, ID or IP: " + args[0]);
                return;
            }

            var limit = 128;
            var count = 0;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                foreach (var target in targets)
                {
                    if (count >= limit) break;
                    var pClass = new PlayerInfo(target);
                    var serial = JsonConvert.SerializeObject(pClass, Formatting.Indented);
                    sb.AppendLine(serial);
                    count++;
                }


                arg.ReplyWith(sb.ToString().TrimEnd());
            }
            finally { Pool.Free(ref sb); }

        }

        private string GetSerializedPInfo(string target)
        {
            if (string.IsNullOrEmpty(target)) return target;
            var targetPly = FindConnectedPlayer(target, true);
            return (targetPly == null) ? target : JsonConvert.SerializeObject(new PlayerInfo(targetPly), Formatting.Indented);
        }


        private void cmdPInfo(IPlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.Id, "compilation.pinfo") && !player.IsServer) 
                return;

            var now = DateTime.UtcNow;

            var watch = Pool.Get<Stopwatch>();

            try
            {
                var sbList = Pool.GetList<StringBuilder>();
                try
                {
                    if (args.Length < 1)
                    {
                        SendReply(player, "Specify a name to search by using /pinfo <name>");
                        var nearSB = new StringBuilder();
                        if (!player.IsServer && player?.Object != null)
                        {
                            var ogPos = (player?.Object as BasePlayer)?.transform?.position ?? Vector3.zero;
                            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                            {
                                var p = BasePlayer.activePlayerList[i];
                                if (p == null || (p?.IsDead() ?? true)) continue;

                                var distance = Vector3.Distance(p.transform.position, ogPos);
                                if (nearSB.Length >= 768)
                                {
                                    sbList.Add(nearSB);
                                    nearSB = new StringBuilder();
                                }
                                if (distance <= 20) nearSB.AppendLine("<color=#ff912b>" + (p?.displayName ?? "Unknown") + "</color> (" + (p?.UserIDString ?? string.Empty) + ") " + "<color=#ff912b>" + distance.ToString("0.0") + "</color>m away");
                            }
                            if (nearSB.Length > 0)
                            {
                                SendReply(player, "Near players: " + Environment.NewLine + nearSB.ToString().TrimEnd());
                                for (int i = 0; i < sbList.Count; i++) SendReply(player, sbList[i].ToString().TrimEnd());
                            }
                        }
                    }
                    else if (args.Length > 1 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        var targets = FindIPlayers(args[1]);
                        if (targets != null && targets.Count() > 1)
                        {
                            var foundSB = new StringBuilder();
                            var index = 0;
                            foreach (var targ in targets)
                            {
                                if (targ == null) continue;
                                var div = IsDivisble(index, 2);
                                if (foundSB.Length >= 800)
                                {
                                    sbList.Add(foundSB);
                                    foundSB = new StringBuilder();
                                }
                                foundSB.Append("<color=").Append((div ? "#ff9933" : "#cc6600")).Append(">").Append(targ.Name).Append(" (").Append(targ.Id).Append(")</color>").Append(Environment.NewLine);
                                index++;
                            }
                            SendReply(player, "Found multiple players:");
                            foreach (var sb in sbList) SendReply(player, sb.ToString().TrimEnd());
                            SendReply(player, foundSB.ToString().TrimEnd());
                            return;
                        }
                        else
                        {
                            SendReply(player, "No matches found!");
                            return;
                        }
                    }

                    var findWatch = Stopwatch.StartNew();

                    var target = FindConnectedPlayer(args[0], true);

                    findWatch.Stop();

                    if (findWatch.Elapsed.TotalMilliseconds >= 5) PrintWarning("FindConnectedPlayer took: " + findWatch.Elapsed.TotalMilliseconds + "ms");

                    if (target == null)
                    {
                        SendReply(player, "Failed to find a player with the name, ID or IP: " + args[0]);
                        return;
                    }

                    var pClass = new PlayerInfo(target);
                    if (pClass == null)
                    {
                        SendReply(player, "pClass is null!!");
                        return;
                    }

                    var infoSB = new StringBuilder();
                    var lastConDate = pClass.LastConnection;
                    var firstJoinDate = pClass.FirstConnection;
                    var lastConStr = (lastConDate != DateTime.MinValue) ? lastConDate.ToString("d") : "Unknown";
                    var firstJoinStr = (firstJoinDate != DateTime.MinValue) ? firstJoinDate.ToString("d") : "Unknown";

                    var pStatusStr = string.Empty;

                    var pSb = Pool.Get<StringBuilder>();
                    try
                    {
                        pSb.Clear().Append(!pClass.Alive ? "<color=red>Dead</color>" : "<color=#8aff47>Alive</color>").ToString();
                        if (pClass.Alive && pClass.Sleeping)
                            pSb.Append(" (sleeping)");

                        pStatusStr = pSb.ToString();

                    }
                    finally { Pool.Free(ref pSb); }

                    infoSB.Append("Info for: <color=orange>").Append(target.Name).Append(" (").Append(target.Id).Append(", ").Append((target.IsAdmin ? "Hidden" : getIPString(target))).Append(")</color>").Append(Environment.NewLine);
                    infoSB.Append("AuthLevel: ").Append((target?.Object as BasePlayer)?.Connection?.authLevel).Append(", IsAdmin: ").Append(target.IsAdmin).Append(", BasePlayer IsAdmin: ").Append((target?.Object as BasePlayer)?.IsAdmin).Append(Environment.NewLine);
                    infoSB.Append("Aliases: ").Append(pClass.Aliases.Count.ToString("N0")).Append(" known aliases").Append(Environment.NewLine);
                    infoSB.Append("Last connection: ").Append(lastConStr).Append(" (").Append((now - lastConDate).TotalDays.ToString("N0")).Append(" days ago)").Append(Environment.NewLine);

                    var timePlayed = Playtimes?.Call<TimeSpan>("GetTimePlayed", target.Id) ?? TimeSpan.MinValue;

                    if (timePlayed > TimeSpan.Zero)
                        infoSB.Append("Total connection time (server): ").Append(timePlayed.TotalHours.ToString("N0")).Append(" hours (").Append(timePlayed.TotalMinutes.ToString("N0")).Append(" minutes)").Append(Environment.NewLine);


                    infoSB.Append("First joined: ").Append(firstJoinStr).Append(" (").Append((now - firstJoinDate).TotalDays.ToString("N0")).Append(" days ago)").Append(Environment.NewLine);

                    infoSB.Append("Player status: ").Append(pStatusStr).Append(Environment.NewLine);

                    if (!string.IsNullOrEmpty(pClass.ClientLanguage))
                        infoSB.Append("Client language: ").Append(pClass.ClientLanguage).Append(Environment.NewLine);

                    var connectedStr = target.IsConnected ? "<color=#8aff47>Yes</color>" : "<color=red>No</color>";

                    infoSB.Append("Connected: ").Append(connectedStr).Append(Environment.NewLine);

                    var playerEnts = pClass.PlacedEntities;
                    if (playerEnts > 0)
                        infoSB.Append("Placed/owned entities: " + playerEnts.ToString("N0")).Append(Environment.NewLine);


                    if (target.IsConnected)
                    {
                        var sessionTimeDate = pClass.ConnectionTime;
                        var sessionTimeStr = (sessionTimeDate != TimeSpan.MinValue) ? sessionTimeDate.TotalMinutes.ToString("N0") : "Unknown";

                        infoSB.Append("Session connection time: ").Append(sessionTimeStr).Append(" minutes").Append(Environment.NewLine);
                        infoSB.Append("Violation level: ").Append(pClass.ViolationLevel.ToString("0.00").Replace(".00", string.Empty)).Append(" (").Append(pClass.ViolationKicks.ToString("N0")).Append(" kicks)").Append(Environment.NewLine);
                    }



                    if (pClass.VACs > 0)
                        infoSB.Append("<color=red>").Append(pClass.VACs.ToString("N0")).Append(" VAC bans on record</color>").Append(Environment.NewLine);
                    if (pClass.CommunityBanned)
                        infoSB.Append("<color=red>Community Banned</color>").Append(Environment.NewLine);

                    if (pClass.TradeBanned)
                        infoSB.Append("<color=red>Trade banned</color>").Append(Environment.NewLine);

                    if (pClass.GameBans > 0)
                        infoSB.Append("<color=red>").Append(pClass.GameBans.ToString("N0")).Append(" Game Bans on record</color>").Append(Environment.NewLine);

                    if (pClass.DaysSinceSteamBan >= 0 && (pClass.VACs > 0 || pClass.GameBans > 0))
                        infoSB.Append("<color=red>").Append(pClass.DaysSinceSteamBan.ToString("N0")).Append(" days since last VAC/Game Ban</color>").Append(Environment.NewLine);

                    if (pClass.VACs < 1 && pClass.GameBans < 1 && !pClass.TradeBanned && !pClass.CommunityBanned)
                        infoSB.Append("No bans on record").Append(Environment.NewLine);

                    if (pClass.TicketReportedCount > 0)
                        infoSB.Append("Reported in tickets: ").Append(pClass.TicketReportedCount.ToString("N0")).Append(" times").Append(Environment.NewLine);

                    var targBanned = pClass.Banned;
                    var banText = "<color=" + (targBanned ? "red" : "green") + ">" + targBanned + " </color>";
                    infoSB.Append("Banned on server: ").Append(banText).Append(Environment.NewLine);

                    var firstCountry = pClass.FirstCountry;
                    var lastCountry = pClass.LastCountry;

                    if (!string.IsNullOrEmpty(firstCountry))
                        infoSB.Append("First Country: <color=orange>").Append(firstCountry).Append("</color>").Append(Environment.NewLine);
                    if (!string.IsNullOrEmpty(lastCountry) && lastCountry != firstCountry)
                        infoSB.Append("<color=red>Last Country: <color=orange>").Append(lastCountry).Append("</color></color>").Append(Environment.NewLine);

                    var clanTag = pClass.ClanTag;
                    if (!string.IsNullOrEmpty(clanTag))
                        infoSB.Append("Clan: ").Append(clanTag).Append(Environment.NewLine);

               
                    //we do not append a newline as we do not trim the last newline :)
                    SendReply(player, infoSB.Append("Got info in: ").Append(watch.Elapsed.TotalMilliseconds).Append("ms").ToString(), target.Id);
                }
                finally { Pool.FreeList(ref sbList); }
            }
            finally { Pool.Free(ref watch); }


        }

        private void cmdPbdDebug(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            var sb = Pool.Get<StringBuilder>();
            try
            {
                sb.Clear();

                var players = Pool.GetList<string>();
                try
                {
                    PlayersByDatabase?.Call("GetAllPlayerIDsNoAlloc", players);

                    sb.Append(players.Count.ToString("N0")).Append(" players\n");

                    for (int i = 0; i < players.Count; i++)
                    {
                        var ply = players[i];
                        sb.Append(GetDisplayNameFromID(ply)).Append(" (").Append(ply).Append(")\n");
                    }

                    if (sb.Length > 1)
                        sb.Length -= 1;

                    SendReply(player, sb.ToString());
                }
                finally { Pool.FreeList(ref players); }

            }
            finally { Pool.Free(ref sb); }

        }

        [ChatCommand("capreset")]
        private void cmdCapReset(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            player.inventory.containerMain.capacity = 24;
        }

        private readonly HashSet<ulong> rlEnabled = new();

        [ChatCommand("rl")]
        private void cmdrl(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (rlEnabled.Contains(player.userID)) rlEnabled.Remove(player.userID);
            else rlEnabled.Add(player.userID);
            SendReply(player, "rL: " + rlEnabled.Contains(player.userID));
        }

        [ChatCommand("gl")]
        private void cmdgl(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (glEnabled.Contains(player.userID)) glEnabled.Remove(player.userID);
            else glEnabled.Add(player.userID);
            SendReply(player, "GL: " + glEnabled.Contains(player.userID));
        }

        private void ShowToast(BasePlayer player, string message, int type = 0)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.IsConnected)
                return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                player.SendConsoleCommand(sb.Clear().Append("gametip.showtoast ").Append(type).Append(" \"").Append(message).Append("\"").ToString());
            }
            finally { Pool.Free(ref sb); }
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

        private void ShowPopupAll(string msg, float duration = 5f)
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                ShowPopup(BasePlayer.activePlayerList[i], msg, duration);
        }

        private void cmdPopup(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1 || player?.Object == null) return;
            var str = string.Join(" ", args);
            if (string.IsNullOrEmpty(str)) return;
            foreach (var p in covalence.Players.Connected) ShowPopup(p?.Object as BasePlayer, str);
        }

        private void cmdDonate(IPlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected) return;
            var donateStr = "-<color=#dd110b>VIP</color> has 130% (30% extra) XP rate, 125% item rate (<color=#42f459>MVP</color> is 10% less).\n\n-<color=#dd110b>VIP</color> gets access to /kit <color=#dd110b>VIP</color>, \n\n-<color=#42f459>MVP</color> gets access to /kit <color=#42f459>MVP</color>. You can view these kits on our Discord (/discord)!\n\n-<color=#dd110b>VIPs</color> and <color=#42f459>MVP</color>s have faster research times (<color=#dd110b>VIP</color> slightly faster than <color=#42f459>MVP</color>).\n\n-<color=#dd110b>VIPs</color> and <color=#42f459>MVP</color>s are able to repair items better (less permanent damage after repair, <color=#dd110b>VIP</color> slightly better than <color=#42f459>MVP</color>).";
            var donateStr2 = "-<color=#dd110b>VIPs</color> and <color=#42f459>MVP</color>s get access to /skin, where they can change the skin of their items to any skin they want.\n\n-<color=#dd110b>VIPs</color> and <color=#42f459>MVP</color>s get access to /bgrade, where they can select a building grade to automatically upgrade to when a block is placed. (still requires resources).\n\n-<color=#dd110b>VIPs</color> and <color=#42f459>MVP</color>s get access to /sil, which allows them to paste images on to a sign.\n<color=#dd110b>VIPs</color> have an extra 5% chance to get lucky.";

            player.Reply("Donator (VIP and MVP) perks:");
            player.Reply(donateStr);
            player.Reply(donateStr2);
            player.Reply("<color=#42f459>MVP</color> Cost: <color=#8aff47>$5</color> (Permanent)\n<color=#dd110b>VIP</color> Cost: <color=#8aff47>$5</color> (per month)\nInterested in donating? You can donate on this page: <size=20><color=#6338ff>prismrust.com</color></size>\n\n<color=#8aff47>You can see a list of commands donators get by typing <color=orange>/help 3</color>.</color>");
            ShowPopup(player?.Object as BasePlayer, "You can donate or read more (<color=#8aff47>and view kits!</color>) at: <size=20><color=orange>prismrust.com</color></size>", 20f);
        }

        [ChatCommand("clear")]
        private void cmdClearChatLocal(BasePlayer player, string command, string[] args)
        {
            ClearPlayerChat(player);
            SendReply(player, "Successfully cleared your chat.");
        }

        private void ClearPlayerChat(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;
            var clearSB = new StringBuilder();
            for (int i = 0; i < 100; i++) clearSB.AppendLine("\n");
            for (int i = 0; i < 10; i++) SendReply(player, clearSB.ToString());
        }

        [ChatCommand("clearchat")]
        private void cmdClearChat(BasePlayer player, string command, string[] args)
        {
            if (!HasPerms(player, Name + ".clearchat"))
            {
                SendReply(player, "You do not have permission to use this command!");
                return;
            }
            var newLineSB = new StringBuilder(); for (int j = 0; j < 60; j++) newLineSB.Append("\n");
            var nlStr = newLineSB.ToString();
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var target = BasePlayer.activePlayerList[i];
                for (int j = 0; j < 5; j++) SendReply(target, nlStr);
                SendReply(target, "Chat was cleared by a moderator.");
            }
        }


        private BaseEntity GetEntityFromNetID(uint netID)
        {
            return BaseEntity.saveList?.Where(p => (p?.net?.ID.Value ?? 0) == netID)?.FirstOrDefault() ?? null;
        }

        private BaseEntity GetEntityFromNetIDS(string netID)
        {
            if (string.IsNullOrEmpty(netID)) return null;
            if (!uint.TryParse(netID, out uint uID)) return null;
            else return GetEntityFromNetID(uID);
        }


        [ChatCommand("seton")]
        private void cmdSetOn(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var lookat = GetLookAtEntity(player, 20, deployColl);
            if (lookat == null) return;
            lookat.SetFlag(BaseEntity.Flags.On, !lookat.HasFlag(BaseEntity.Flags.On));

            lookat.SendNetworkUpdate();
            SendReply(player, "On: " + lookat.HasFlag(BaseEntity.Flags.On) + ", for: " + lookat.ShortPrefabName);
        }

        private System.Collections.IEnumerator DeleteStructure(IEnumerable<BaseEntity> entities, bool stabilityOff = false, BasePlayer player = null)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            var watch = Pool.Get<Stopwatch>();

            try
            {
                watch.Restart();

                var max = 500;
                var maxStab = 400;
                var count = 0;
                var total = 0;
                var totalCount = entities.Count();
                var totalCountStr = totalCount.ToString("N0");

                if (stabilityOff)
                {
                    if (player != null && player.IsConnected) SendReply(player, "Disabling stability first...");
                    foreach (var entity in entities)
                    {
                        if (entity == null) continue;
                        if (count >= maxStab)
                        {
                            count = 0;
                            yield return CoroutineEx.waitForSeconds(0.055f);
                            //   yield return CoroutineEx.waitForEndOfFrame;
                        }
                        var stability = entity?.GetComponent<StabilityEntity>() ?? null;
                        if (stability != null)
                        {
                            stability.cachedStability = 100;
                            stability.grounded = true;
                        }
                        count++;
                    }
                    if (player != null && player.IsConnected) SendReply(player, "Stability disabled, now starting deleting process...");
                }

                count = 0;
                var total2 = 0;
                var sb = new StringBuilder();

                foreach (var entity in entities)
                {
                    if (entity == null || entity.IsDestroyed) continue;
                    total++;
                    if (count >= max)
                    {
                        count = 0;
                        yield return CoroutineEx.waitForSeconds(0.0175f);
                    }
                    entity.Invoke(() =>
                    {
                        if (!entity.IsDestroyed) entity.KillMessage();
                        total2++;
                        if (totalCount >= 1000 && player != null && player.IsConnected) SendReply(player, sb.Clear().Append(total2.ToString("N0")).Append("/").Append(totalCountStr).ToString());
                    }, total * 0.01f);
                    count++;
                }

                if (player != null && player.IsConnected) SendReply(player, sb.Clear().Append("Took: ").Append(watch.Elapsed.TotalMilliseconds).Append("ms to delete ").Append(totalCountStr).Append(" entities").ToString());


            }
            finally { Pool.Free(ref watch); }

        }

        [ChatCommand("delstruct")]
        private void cmdRemoveStruct(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var lookEnt = GetLookAtEntity(player) as BuildingBlock;
            if (lookEnt == null)
            {
                SendReply(player, "No look at entities!");
                return;
            }
            if (lookEnt.buildingID == 0)
            {
                SendReply(player, "Invalid look at");
                return;
            }

            SendReply(player, "Starting delete coroutine...");

            var building = BuildingManager.server.GetBuilding(lookEnt.buildingID);

            if (building == null)
            {
                SendReply(player, "Invalid building ID/no building found!");
                return;
            }

            var attached = building.buildingBlocks;

            if (attached == null || attached.Count < 1)
            {
                SendReply(player, "No entities in buildingBlocks!");
                return;
            }

            var orderedAttached = attached.OrderByDescending(p => (p?.prefabID ?? 0) != 72949757); //foundation.prefab

            var stabilityOff = args.Length > 0 && args[0].Equals("true", StringComparison.OrdinalIgnoreCase);
            ServerMgr.Instance.StartCoroutine(DeleteStructure(orderedAttached, stabilityOff, player));


        }

        [ChatCommand("prod3")]
        private void cmdProd3(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var watch = new Stopwatch();
            watch.Start();
            var lookAt = GetLookAtEntity(player, 250, _constructionColl) as DecayEntity;

            var bID = lookAt?.buildingID ?? 0;
            if (lookAt == null || (lookAt?.IsDestroyed ?? true) || (lookAt?.buildingID ?? 0) == 0)
            {
                SendReply(player, "No/invalid entity found.");
                return;
            }

            var building = BuildingManager.server.GetBuilding(bID);

            if (building == null)
            {
                SendReply(player, "No building!");
                return;
            }
            else SendReply(player, "Prodding building " + bID);

            var entities = building?.decayEntities;

            var total = entities?.Count ?? 0;
            if (total < 1)
            {
                SendReply(player, "No entities found with same ID.");
                return;
            }

            var percs = new Dictionary<ulong, int>();
            foreach (var entity in entities)
            {
                if (entity == null) continue;

                if (!percs.TryGetValue(entity.OwnerID, out int entVal)) percs[entity.OwnerID] = 1;
                else percs[entity.OwnerID]++;


                var linked = entity?.FindLinkedEntity<BaseEntity>() ?? null;
                var doorLink = linked as Door;
                if (doorLink != null)
                {
                    total++;
                    if (!percs.TryGetValue(doorLink.OwnerID, out entVal)) percs[doorLink.OwnerID] = 1;
                    else percs[doorLink.OwnerID]++;
                    var doorSlot = doorLink?.GetSlot(BaseEntity.Slot.Lock) ?? null;
                    if (doorSlot != null)
                    {
                        total++;
                        var authPlayers = (doorSlot as CodeLock)?.whitelistPlayers ?? null;
                        var slotID = (authPlayers != null && authPlayers.Count > 0) ? authPlayers[0] : doorLink?.OwnerID ?? 0;
                        if (!percs.TryGetValue(slotID, out entVal)) percs[slotID] = 1;
                        else percs[slotID]++;
                    }
                }
                var entLink = linked;
                if (entLink != null && entLink != doorLink)
                {
                    var findItem = ItemManager.FindItemDefinition(entLink.ShortPrefabName);
                    if (findItem != null)
                    {
                        total++;
                        if (!percs.TryGetValue(entLink.OwnerID, out entVal)) entLink.OwnerID = 1;
                        else percs[entLink.OwnerID]++;
                    }
                }
            }
            var ownerSB = new StringBuilder();
            var sbList = Pool.GetList<StringBuilder>();
            var unknown = 100f;
            var now = DateTime.UtcNow;
            foreach (var kvp in percs.OrderByDescending(p => p.Value))
            {
                var key = kvp.Key;
                var playerName = GetDisplayNameFromID(key);
                if (string.IsNullOrEmpty(playerName)) continue;
                var lastCon = GetLastConnection(key);
                PrintWarning("lastcon: " + lastCon + " NOW: " + now);
                var perc = kvp.Value * 100f / total;
                if (ownerSB.Length >= 768)
                {
                    sbList.Add(ownerSB);
                    ownerSB = new StringBuilder();
                }
                var isConnected = false;
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var ply = BasePlayer.activePlayerList[i];
                    if (ply?.userID == key)
                    {
                        isConnected = ply?.IsConnected ?? false;
                        break;
                    }
                }
                var conStr = isConnected ? "Connected now" : "Connected: " + (now - lastCon).TotalHours.ToString("0.00").Replace(".00", string.Empty) + " hours ago";
                ownerSB.AppendLine(playerName + " (" + conStr + "): " + perc.ToString("0.##") + "%");
                unknown -= perc;
            }
            if (unknown > 0) SendReply(player, "Unknown: " + unknown.ToString("0.##") + "%");
            if (sbList.Count > 0) for (int i = 0; i < sbList.Count; i++) SendReply(player, sbList[i].ToString().TrimEnd());
            if (ownerSB.Length > 0) SendReply(player, ownerSB.ToString().TrimEnd());
            Pool.FreeList(ref sbList);
            watch.Stop();
            SendReply(player, "Took " + watch.Elapsed.TotalMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms to prod " + total.ToString("N0") + " entities");
        }

        [ChatCommand("doorauth")]
        private void cmdDoorAuth(BasePlayer player, string command, string[] args)
        {
            var watch = Stopwatch.StartNew();
            if (args.Length < 1)
            {
                SendReply(player, "<color=#2C1A07>----<color=#A28354>Door Authorization</color>----</color>\n-You can mass add/remove players to all doors (codelocks) in a base (determined by where you're looking) by typing <color=#936C3C>/doorauth add PlayerName</color>.\n-You can deauthorize a player using <color=#936C3C>/doorauth remove PlayerName</color>.");
                return;
            }
            var arg0Lower = args[0].ToLower();
            if (arg0Lower != "add" && arg0Lower != "remove")
            {
                SendReply(player, "Invalid argument supplied! Try <color=#8aff47>add</color> or <color=#ff912b>remove</color>.");
                return;
            }
            var isAdd = arg0Lower == "add";
            if (args.Length < 2)
            {
                SendReply(player, "You must supply a target!");
                return;
            }
            var target = FindConnectedPlayer(args[1], true);
            if (target == null)
            {
                SendReply(player, "Couldn't find target: " + args[1]);
                return;
            }
            if (!player.CanBuild())
            {
                SendReply(player, "You need building privilege to use this.");
                return;
            }
            var lookAt = GetLookAtEntity(player, 5f, _constructionColl);
            if (lookAt == null || lookAt.IsDestroyed)
            {
                SendReply(player, "No entity!");
                return;
            }
            var doorLook = lookAt as Door;
            var blockLook = lookAt as BuildingBlock;
            if (doorLook == null && blockLook == null)
            {
                SendReply(player, "No entity found, you must be looking at a part of the base or a door!");
                return;
            }
            var buildingID = blockLook?.buildingID ?? doorLook?.FindLinkedEntity<BuildingBlock>()?.buildingID ?? 0;
            var locks = GetLocks(buildingID, player.userID);
            if (locks == null || locks.Count < 1)
            {
                if (locks == null) locks = new HashSet<CodeLock>();
                var getLock = GetLock(doorLook);
                if (getLock != null) locks.Add(getLock);
            }
            if (locks == null || locks.Count < 1)
            {
                SendReply(player, "Couldn't find any code locks owned by you in this base!");
                return;
            }
            if (!ulong.TryParse(target.Id, out ulong targetID))
            {
                SendReply(player, "Target ID is not a ulong!");
                return;
            }
            var changedCount = 0;
            foreach (var codeLock in locks)
            {
                if (codeLock?.whitelistPlayers == null || !codeLock.whitelistPlayers.Contains(player.userID)) continue;
                if (isAdd)
                {
                    if (!codeLock.whitelistPlayers.Contains(targetID))
                    {
                        codeLock.whitelistPlayers.Add(targetID);
                        changedCount++;
                    }
                }
                else
                {
                    if (codeLock.whitelistPlayers.Contains(targetID))
                    {
                        codeLock.whitelistPlayers.Remove(targetID);
                        changedCount++;
                    }
                }
            }
            var addTxt = isAdd ? ("Added " + target.Name + " to " + changedCount.ToString("N0")) : ("Removed " + target.Name + " from " + changedCount.ToString("N0"));
            SendReply(player, addTxt + "/" + locks.Count.ToString("N0") + " code locks.");
            if (target.IsConnected && changedCount > 0) SendReply(target, player.displayName + " " + (isAdd ? "added you to" : "removed you from") + " " + changedCount.ToString("N0") + " code locks.");
            watch.Stop();
            PrintWarning("Door auth cmd took: " + watch.Elapsed.TotalMilliseconds + "ms");
        }

        private HashSet<CodeLock> GetLocks(uint buildingId, ulong OwnerID)
        {
            if (buildingId == 0) return null;
            var list = new HashSet<CodeLock>();
            foreach (var entity in BaseEntity.saveList)
            {
                var baseLock = entity as CodeLock;
                if (baseLock == null || baseLock.IsDestroyed) continue;
                var lockBuildId = (baseLock?.FindLinkedEntity<Door>() ?? baseLock?.GetComponentInParent<Door>())?.FindLinkedEntity<BuildingBlock>()?.buildingID ?? 0;
                if (lockBuildId != buildingId) continue;
                var ownerID = baseLock.OwnerID != 0 ? baseLock.OwnerID : baseLock?.GetComponentInParent<Door>()?.OwnerID ?? 0;
                if (ownerID == OwnerID) list.Add(baseLock);
            }
            return list;
        }

        private CodeLock GetLock(Door door)
        {
            if (door == null || door.IsDestroyed || door.gameObject == null) return null;
            return (door?.GetSlot(BaseEntity.Slot.Lock) ?? null) as CodeLock;
        }





        [ChatCommand("news")]
        private void cmdNews(BasePlayer player, string command, string[] args)
        {
            if (!newsLevel.TryGetValue(player.UserIDString, out int level)) level = 2;
            if (args == null || args.Length < 1)
            {
                SendReply(player, "To view the current news, type: /" + command + " view" + Environment.NewLine + Environment.NewLine + "Notification settings:" + Environment.NewLine + "1: Low" + (level == 1 ? " <color=#8aff47>(Current)</color>" : string.Empty) + Environment.NewLine + "2: Normal " + (level > 1 ? " <color=#8aff47>(Current)</color>" : string.Empty) + Environment.NewLine + Environment.NewLine + "To change notification settings, try: <color=#8aff47>/" + command + " 1</color> (or any value)");
                return;
            }
            var arg0 = args[0];
            if (arg0.Equals("view", StringComparison.OrdinalIgnoreCase)) BroadcastNews(player);
            else
            {
                if (!int.TryParse(arg0, out int newLevel))
                {
                    SendReply(player, "Not a number or invalid arugment: " + arg0);
                    return;
                }

                if (newLevel < 1 || newLevel > 2)
                {
                    SendReply(player, "Incorrect value specified: <color=red>" + newLevel + "</color>!");
                    return;
                }

                newsLevel[player.UserIDString] = newLevel;
                SendReply(player, "Set notification level to: " + newLevel);
            }

        }

        private readonly Dictionary<NetworkableId, Vector3> preGroundPos = new();
        [ChatCommand("ground")]
        private void cmdGround(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1;
            var isRevert = args.Length > 0 && args[0].ToLower() == "undo";
            if (args.Length > 0 && !int.TryParse(args[0], out layer))
            {
                SendReply(player, "Wrong layer: " + args[0]);
                return;
            }
            var getLook = GetLookAtEntity(player, 30f, layer);
            if (getLook == null || getLook.IsDestroyed || getLook.transform == null)
            {
                SendReply(player, "No entity found");
                return;
            }
            Vector3 oldPos = Vector3.zero;
            if (isRevert)
            {
                if (!preGroundPos.TryGetValue(getLook.net.ID, out oldPos))
                {
                    SendReply(player, "No pre ground pos");
                    return;
                }
            }
            var newPos = Vector3.zero;
            if (oldPos != Vector3.zero) newPos = oldPos;
            else oldPos = getLook?.transform?.position ?? Vector3.zero;


            if (!isRevert)
            {
                var groundPos = GetGroundPosition(oldPos);
                if (groundPos == Vector3.zero)
                {
                    SendReply(player, "No ground pos, can't set!");
                    return;
                }
                preGroundPos[getLook.net.ID] = oldPos;
                newPos = groundPos;
            }
            if (newPos == Vector3.zero)
            {
                SendReply(player, "No newPos");
                return;
            }
            getLook.transform.position = newPos;
            getLook.SendNetworkUpdateImmediate();
            SendReply(player, isRevert ? "Ungrounded entity" : "Grounded entity");
        }

        [ChatCommand("groundme")]
        private void cmdGroundMe(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;



            var groundPos = GetGroundPosition(player.transform.position);
            if (groundPos == Vector3.zero)
            {
                SendReply(player, "No ground pos, can't set!");
                return;
            }

            var newPos = groundPos;
            if (newPos == Vector3.zero)
            {
                SendReply(player, "No newPos");
                return;
            }
            SendReply(player, "ground pos: " + newPos);
            TeleportPlayer(player, newPos, false, false);
        }

        [ChatCommand("stock")]
        private void cmdStock(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var vm = GetLookAtEntity(player, 10f, deployColl) as VendingMachine;
            if (vm == null || vm.IsDestroyed)
            {
                SendReply(player, "You must be looking at a vending machine to use this!");
                return;
            }
            var orders = vm?.sellOrders?.sellOrders ?? null;
            if (orders == null || orders.Count < 1)
            {
                SendReply(player, "No orders");
                return;
            }

            var maxStack = (int)(int.MaxValue * 0.5);

            for (int i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                if (order == null) continue;
                var sell = order?.itemToSellID ?? 0;
                if (sell == 0) continue;
                var def = ItemManager.FindItemDefinition(sell);
                if (def == null)
                {
                    PrintWarning("sell order with null def!!!!: " + sell + " ID of item");
                    continue;
                }
                Item findItem = null;

                var items = vm?.inventory?.itemList;
                for (int j = 0; j < items.Count; j++)
                {
                    var item = items[j];
                    if (item?.info?.itemid == sell)
                    {
                        findItem = item;
                        break;
                    }
                }

                if (findItem == null)
                {
                    var newItem = ItemManager.Create(def, maxStack);
                    if (!newItem.MoveToContainer(vm.inventory)) RemoveFromWorld(newItem);
                }
                else
                {
                    findItem.amount = maxStack;
                    findItem.MarkDirty();
                }
            }

            vm.RefreshSellOrderStockLevel();

            SendReply(player, "Stocked VM");

        }

        private void cmdAddVendingMachineSellOrder(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var pObj = player?.Object as BasePlayer;

            if (pObj == null || !pObj.IsConnected)
                return;


            var vm = GetLookAtEntity(pObj, 10f, deployColl) as VendingMachine;
            if (vm == null || vm.IsDestroyed)
            {
                SendReply(player, "You must be looking at a vending machine to use this!");
                return;
            }

            if (args.Length < 4)
            {
                SendReply(player, "You must specify <itemToSell ID/name> <itemToSell amount> <currencyItem ID/name> <curencyItem amount>");
                return;
            }


            var itemSell = FindItemByNameOrID(args[0]);

            if (itemSell == null)
            {
                SendReply(player, "itemSell null for: " + args[0]);
                return;
            }

            if (!int.TryParse(args[1], out int itemSellAmount))
            {
                SendReply(player, "Invalid itemToSell amount: " + args[1]);
                return;
            }


            var currencyItem = FindItemByNameOrID(args[2]);

            if (currencyItem == null)
            {
                SendReply(player, "currencyItem null for: " + args[2]);
                return;
            }

            if (!int.TryParse(args[3], out int currencyItemAmount))
            {
                SendReply(player, "Invalid currencyItem amount: " + args[3]);
                return;
            }

            if (args.Length > 4 && (!byte.TryParse(args[4], out byte bpState) || bpState < 0 || bpState > 3))
            {
                SendReply(player, "Invalid bpState: " + args[4]);
                return;
            }
            else bpState = 0;

            AddSellOrder(vm, itemSell.itemid, itemSellAmount, currencyItem.itemid, currencyItemAmount, bpState);

            SendReply(player, "Added: " + itemSell.displayName.english + " x" + itemSellAmount + " for " + currencyItem.displayName.english + " x" + currencyItemAmount);
        }



        private void AddSellOrder(VendingMachine vm, int itemToSellID, int itemToSellAmount, int currencyToUseID, int currencyAmount, byte bpState)
        {
            if (vm == null || vm.IsDestroyed) return;

            ItemDefinition itemDefinition1 = ItemManager.FindItemDefinition(itemToSellID);
            ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(currencyToUseID);

            if (itemDefinition1 == null || itemDefinition2 == null)
                return;

            //  currencyAmount = Mathf.Clamp(currencyAmount, 1, 10000);
            //    itemToSellAmount = Mathf.Clamp(itemToSellAmount, 1, itemDefinition1.stackable);

            ProtoBuf.VendingMachine.SellOrder sellOrder = new()
            {
                ShouldPool = false,
                itemToSellID = itemToSellID,
                itemToSellAmount = itemToSellAmount,
                currencyID = currencyToUseID,
                currencyAmountPerItem = currencyAmount,
                currencyIsBP = (int)bpState == 3 || (int)bpState == 2,
                itemToSellIsBP = (int)bpState == 3 || (int)bpState == 1
            };

            Interface.CallHook("OnAddVendingOffer", vm, sellOrder);
            vm.sellOrders.sellOrders.Add(sellOrder);
            vm.RefreshSellOrderStockLevel(itemDefinition1);
            vm.UpdateMapMarker();
            vm.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);


        }

        private class ClientVarWithValue
        {
            public string FullCommandName { get; set; }
            public string VariableValue { get; set; } = string.Empty;

            public static List<ClientVarWithValue> All { get; set; } = new List<ClientVarWithValue>();

            public ClientVarWithValue()
            {
                All.Add(this);
            }
            public ClientVarWithValue(string commandName, string variableValue)
            {
                if (string.IsNullOrEmpty(commandName)) throw new ArgumentNullException(nameof(commandName));

                FullCommandName = commandName;
                VariableValue = variableValue;

                All.Add(this);
            }

            public static ClientVarWithValue GetOrCreateClientVar(string fullCommandName, string setVariableValue = "")
            {
                if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

                for (int i = 0; i < All.Count; i++)
                {
                    var var = All[i];
                    if (var.FullCommandName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(setVariableValue)) var.VariableValue = setVariableValue;
                        return var;

                    }
                }

                return new ClientVarWithValue(fullCommandName, setVariableValue);
            }
        }

        private void SendReplicatedVarsToConnection(Network.Connection connection, string filter = "")
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var cmds = Pool.GetList<ConsoleSystem.Command>();

            try
            {
                for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
                {
                    var cmd = ConsoleSystem.Index.Server.Replicated[i];
                    if (string.IsNullOrEmpty(filter) || cmd.FullName.StartsWith(filter)) cmds.Add(cmd);

                }

                if (cmds.Count < 1)
                {
                    PrintWarning("No replicated commands found with filter: " + filter);
                    return;
                }

                var netWrite = Net.sv.StartWrite();

                netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
                netWrite.Int32(cmds.Count);

                for (int i = 0; i < cmds.Count; i++)
                {
                    var var = cmds[i];
                    netWrite.String(var.FullName);
                    netWrite.String(var.String);
                }


                netWrite.Send(new SendInfo(connection));

            }
            finally { Pool.FreeList(ref cmds); }


        }

        private void SendClientsVar(List<Network.Connection> connections, List<ClientVarWithValue> vars)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));

            if (connections.Count < 1) throw new ArgumentOutOfRangeException(nameof(connections));

            if (vars == null) throw new ArgumentNullException(nameof(vars));
            if (vars.Count < 1) throw new ArgumentOutOfRangeException(nameof(vars));

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(vars.Count);

            for (int i = 0; i < vars.Count; i++)
            {
                var var = vars[i];
                netWrite.String(var.FullCommandName);
                netWrite.String(var.VariableValue);
            }


            netWrite.Send(new SendInfo(connections));

        }

        private void SendClientVars(Network.Connection connection, List<ClientVarWithValue> vars)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (vars == null) throw new ArgumentNullException(nameof(vars));
            if (vars.Count < 1) throw new ArgumentOutOfRangeException(nameof(vars));


            var netWrite = Net.sv.StartWrite();


            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(vars.Count);

            for (int i = 0; i < vars.Count; i++)
            {
                var var = vars[i];
                netWrite.String(var.FullCommandName);
                netWrite.String(var.VariableValue);
            }


            netWrite.Send(new SendInfo(connection));

        }

        private void SendClientVar(Network.Connection connection, string fullCommandName, string varValue)
        {
            if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

            var foundCmd = false;
            for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
            {
                var cmd = ConsoleSystem.Index.Server.Replicated[i];
                if (cmd.FullName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    foundCmd = true;
                    fullCommandName = cmd.FullName;
                    break;
                }
            }

            if (!foundCmd)
            {
                PrintWarning(nameof(SendClientVar) + " failed to find replicated var!");
                return;
            }

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(1);

            netWrite.String(fullCommandName);
            netWrite.String(varValue);

            netWrite.Send(new SendInfo(connection));

        }

        private void SendClientsVar(List<Network.Connection> connections, string fullCommandName, string varValue)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));

            if (connections.Count < 1) throw new ArgumentOutOfRangeException(nameof(connections));

            if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

            var foundCmd = false;
            for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
            {
                var cmd = ConsoleSystem.Index.Server.Replicated[i];
                if (cmd.FullName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    foundCmd = true;
                    fullCommandName = cmd.FullName;
                    break;
                }
            }

            if (!foundCmd)
            {
                PrintWarning(nameof(SendClientsVar) + " failed to find replicated var!");
                return;
            }

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(1);

            netWrite.String(fullCommandName);
            netWrite.String(varValue);

            netWrite.Send(new SendInfo(connections));
        }

        [ChatCommand("triggers")]
        private void cmdTriggerTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (player?.triggers == null)
            {
                SendReply(player, "triggers null!!");
                return;
            }

            if (args.Length > 0 && args[0].Equals("clear"))
            {

                var copyTo = Pool.GetList<TriggerBase>();

                try
                {
                    for (int i = 0; i < player.triggers.Count; i++)
                        copyTo.Add(player.triggers[i]);
                    

                    for (int i = 0; i < copyTo.Count; i++)
                        player.LeaveTrigger(copyTo[i]);

                }
                finally { Pool.FreeList(ref copyTo); }

                SendReply(player, "Cleared/left triggers");


                return;
            }

            var sb = new StringBuilder();

            for (int i = 0; i < player.triggers.Count; i++)
            {
                var trigger = player.triggers[i];
                sb.Append(trigger.ToString()).Append(" (").Append(trigger.GetType()).Append(") ").Append(Environment.NewLine);

                var temperatureTrigger = trigger as TriggerTemperature;
                if (temperatureTrigger != null)
                {
                    sb.Length--;

                    sb.Append(" ").Append(temperatureTrigger.Temperature + ", " + temperatureTrigger.minSize + ", " + temperatureTrigger.triggerSize).Append(Environment.NewLine);
                }
            }

            if (sb.Length > 1)
                sb.Length--;

            SendReply(player, sb.ToString());

            // player.StartWounded();
        }

        [ChatCommand("welcomecui")]
        private void cmdWelcomeCui(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            ShowLoginCUI(player);
        }


        [ChatCommand("unlockinv")]
        private void cmdUnlockInv(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            player.inventory.containerMain.SetLocked(false);
            player.inventory.containerBelt.SetLocked(false);

            player.inventory.containerWear.SetLocked(false);
            SendReply(player, "unlocked inv");

        }

        [ChatCommand("poas")]
        private void cmdPoss(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            // player.StartWounded();
        }

        [ChatCommand("mons")]
        private void cmdMons(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var monSb = Pool.Get<StringBuilder>();

            var findName = args.Length > 0 ? string.Join(" ", args) : string.Empty;

            try 
            {

                monSb.Clear();

                for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                {
                    var mon = TerrainMeta.Path.Monuments[i];

                    var monName = mon?.displayPhrase?.english ?? "Unnamed Monument";

                    if (!string.IsNullOrWhiteSpace(findName) && !(monName.Equals(findName, StringComparison.OrdinalIgnoreCase) || monName.IndexOf(findName, StringComparison.OrdinalIgnoreCase) >= 0))
                        continue;

                    monSb.Append(monName).Append(" ").Append((mon?.transform?.position ?? Vector3.zero)).Append(Environment.NewLine);

                }

                if (monSb.Length > 1)
                    monSb.Length--;

                SendReply(player, monSb.ToString());

            }
            finally { Pool.Free(ref monSb); }

        }

        [ChatCommand("pmods")]
        private void cmdPMods(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var modifiers = player?.modifiers;
            if (modifiers?.All == null || modifiers.All.Count < 1)
            {
                SendReply(player, "No modifiers contained in All?!");
                return;
            }

            if (args.Length > 0 && args[0].Equals("clear"))
            {

                modifiers.RemoveAll();
                SendReply(player, "Cleared pmods");
                return;
            }

            var sb = Pool.Get<StringBuilder>();
            try
            {
                sb.Clear();

                for (int i = 0; i < modifiers.All.Count; i++)
                {
                    var mod = modifiers.All[i];

                    sb.Append(mod.Type).Append(" ").Append(mod.Source).Append(" ").Append(mod.Value).Append(" ").Append(mod.Duration).Append(Environment.NewLine);

                }

                if (sb.Length > 1)
                    sb.Length -= 1; //trim newline

                SendReply(player, sb.ToString());

            }
            finally { Pool.Free(ref sb); }

        }

        [ChatCommand("itemgivetest")]
        private void cmdItemGiveTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var item = ItemManager.CreateByName("fish.troutsmall", 1);
            player.GiveItem(item);
            SendReply(player, "gave");

        }

        [ChatCommand("reorder")]
        private void cmdReorderTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var getLook = GetLookAtEntity(player, 10f) as LootContainer;
            if (getLook == null)
            {
                SendReply(player, "No loot container on getLook");
                return;
            }

            SendReply(player, "reordering");

            ReorderContainer(getLook?.inventory);

            SendReply(player, "reordered");
        }

        [ChatCommand("rptest")]
        private void cmdRpTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            SendReply(player, nameof(SendRustPlusNotification) + " " + player.UserIDString + " " + args[0]);
            SendRustPlusNotification(player.userID, args[0]);

        }

        [ChatCommand("spd")]
        private void cmdSpdTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;


            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out float newSpeed))
                {
                    SendReply(player, "not a float: " + args[0]);
                    return;
                }

                player.clothingMoveSpeedReduction = newSpeed;
            }

            SendReply(player, nameof(player.clothingMoveSpeedReduction) + ": " + player.clothingMoveSpeedReduction);
        }

        [ChatCommand("oilch47test")]
        private void cmdOilCh47Test(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var listener = entity as CH47ReinforcementListener;
                if (listener != null)
                {
                    PrintWarning("found listener! spawning here (calling listener.Call())");
                    listener.Call();
                    SendReply(player, "Called listener.Call()");
                    break;
                }
            }

        }

        [ChatCommand("gesturedump")]
        private void cmdGestureDump(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var gestures = player.gestureList;

            var sb = new StringBuilder();

            for (int i = 0; i < gestures.AllGestures.Length; i++)
            {
                sb.Append(gestures.AllGestures[i].convarName).Append(Environment.NewLine);
            }

            sb.Length -= 1;

            PrintWarning(sb.ToString());

        }

        [ChatCommand("gesture")]
        private void cmdGestureDebug(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 1) return;

            var gestureArg = args[0];

            var gestures = player.gestureList;

            GestureConfig gesture = null;

            for (int i = 0; i < gestures.AllGestures.Length; i++)
            {
                var ges = gestures.AllGestures[i];
                if (ges.convarName.Equals(gestureArg, StringComparison.OrdinalIgnoreCase))
                {
                    gesture = ges;
                    break;
                }
            }

            if (gesture == null)
            {
                SendReply(player, "No gesture with name: " + gestureArg);
                return;
            }

            player.Server_CancelGesture();
            player.Server_StartGesture(gesture, BasePlayer.GestureStartSource.ServerAction);

            SendReply(player, "Ran gesture: " + gesture.convarName);

        }

        [ChatCommand("ambient")]
        private void cmdAmbient(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            //var newPos = player.transform.position;


            // SendLocalEffect(player, "assets/prefabs/weapons/ak47u/effects/attack.prefab");
        }

        private void cmdKillAllLoot(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var count = 0;
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                if (entity != null && entity is LootContainer && !entity.IsDestroyed)
                {
                    entity.Kill();
                    count++;
                }
            }

            player.Reply("Killed " + count.ToString("N0") + " loot entities!");
        }


        [ChatCommand("od")]
        private void cmdOpenDoor(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var door = GetLookAtEntity(player, 10f) as Door;
            if (door == null || door.IsDestroyed)
            {
                SendReply(player, "You must be looking at a door to use this!");
                return;
            }
            door.SetOpen(!door.IsOpen());
            door.SendNetworkUpdate();
            SendReply(player, "Toggled door open");
        }


        private MonumentInfo GetNearestMonument(Vector3 position)
        {
            var pos = position;
            pos.y = 0;

            var monDist = Pool.Get<Dictionary<MonumentInfo, float>>();
            try
            {
                if (monDist?.Count > 0)
                {
                    PrintWarning("monDist.Count > 0!!!");
                    monDist.Clear();
                }

                for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                {
                    var mon = TerrainMeta.Path.Monuments[i];
                    if (mon == null || mon.gameObject == null) continue;
                    var monP = mon?.transform?.position ?? Vector3.zero;
                    monP.y = 0;
                    monDist[mon] = Vector3.Distance(monP, pos);
                }

                MonumentInfo nearest = null;
                var lastDist = -1f;
                foreach (var kvp in monDist)
                {
                    var key = kvp.Key;
                    var val = kvp.Value;
                    if (lastDist < 0f || (val < lastDist))
                    {
                        lastDist = val;
                        nearest = key;
                    }
                }
                return nearest;
            }
            finally { Pool.Free(ref monDist); }
        }

        private string GetMonumentName(MonumentInfo monument)
        {
            if (monument == null) return string.Empty;
            var str = monument?.displayPhrase?.english ?? string.Empty;
            if (string.IsNullOrEmpty(str)) str = monument?.name ?? string.Empty + " " + (monument?.Type ?? MonumentType.Town).ToString();
            return str;
        }



        [ChatCommand("nearmon")]
        private void cmdNearMon(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var nearestMon = GetNearestMonument(player.transform.position);
            var dist = Vector3.Distance(player.transform.position, nearestMon.transform.position);
            SendReply(player, "Nearest mon is: " + GetMonumentName(nearestMon));
        }

        [ChatCommand("getinfo")]
        private void cmdGetInfo(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            var info = player.GetInfoInt(args[0], -1);
            var info2 = player.net.connection.info.GetBool(args[0], false);
            var info3 = player.net.connection.info.GetFloat(args[0], -1f);
            var info4 = player.net.connection.info.GetString(args[0], string.Empty);
            SendReply(player, "Info: " + info);
            SendReply(player, "info2: " + info2);
            SendReply(player, "info3: " + info3);
            SendReply(player, "info4: " + info4);
        }

        [ChatCommand("extractitems")]
        private void cmdExtractItems(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var coll = deployColl;
            if (args.Length > 0 && !int.TryParse(args[0], out coll))
            {
                SendReply(player, "Not an int: " + args[0]);
                return;
            }
            var lookAt = GetLookAtEntity(player, 10f, coll) as StorageContainer;
            var lookAt2 = GetLookAtEntity(player, 10f, coll) as DroppedItemContainer;
            if ((lookAt == null || lookAt.IsDestroyed) && (lookAt2 == null || lookAt2.IsDestroyed))
            {
                SendReply(player, "No container!");
                return;
            }
            var items = lookAt?.inventory?.itemList?.ToList() ?? null;// ?? lookAt2?.inventory?.itemList ?? null)?.ToList() ?? null;
            if (items == null || items.Count < 1) items = lookAt2?.inventory?.itemList?.ToList() ?? null;
            if (items == null || items.Count < 1)
            {
                SendReply(player, "No items!");
                return;
            }
            var dropPos = lookAt?.GetDropPosition() ?? lookAt2?.GetDropPosition() ?? Vector3.zero;
            var dropVel = lookAt?.GetDropVelocity() ?? lookAt2?.GetDropVelocity() ?? Vector3.zero;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;
                if (!item.Drop(dropPos, dropVel)) RemoveFromWorld(item);
            }
        }

        [ChatCommand("scatchup")]
        private void cmdScrapCatchup(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var val = GetScrapScalar(player);
            var rngScrap = UnityEngine.Random.Range(5, 13);

            SendReply(player, "Scrap catchup: " + val + ", rng scrap: " + rngScrap + ", with scalar: " + ((int)Math.Round(rngScrap + (rngScrap * val / 100), MidpointRounding.AwayFromZero)));
        }

        private int ScaleScrapAmountBasedOnContainer(int scrapAmount, uint containerId)
        {
            if (scrapAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(scrapAmount));

            float scalar;
            switch (containerId)
            {
                case 3286607235:
                case 96231181:
                    scalar = 4f;
                    break;
                case 2857304752:
                case 1009499252:
                    scalar = 1.67f;
                    break;
                case 1546200557:
                case 206692676:
                case 1791916628:
                case 2276830067:
                    scalar = 0.66f;
                    break;
                default:
                    scalar = 1f;
                    break;
            }


            return (int)Mathf.Clamp(scrapAmount * scalar, 0, 9999);
        }

        private double GetScrapScalar(BasePlayer player)
        {
            if (player == null || player?.blueprints == null) return 0;
            var val = 0d;

            var serverMgr = ServerMgr.Instance;
            if (serverMgr == null) return val;

            var allUnlocked = new Dictionary<int, int>();

            foreach (var ply in BasePlayer.allPlayerList)
            {
                var BPs = ply?.blueprints ?? null;
                if (BPs == null) continue;

                var bpList = serverMgr?.persistance?.GetPlayerInfo(ply.userID)?.unlockedItems ?? null;
                if (bpList == null || bpList.Count < 1) continue;

                for (int j = 0; j < bpList.Count; j++)
                {
                    var bp = bpList[j];
                    if (!allUnlocked.TryGetValue(bp, out int count)) allUnlocked[bp] = 1;
                    else allUnlocked[bp]++;
                }
            }




            var bpInfo = serverMgr?.persistance?.GetPlayerInfo(player.userID)?.unlockedItems ?? null;

            var notUnlocked = allUnlocked?.Where(p => !bpInfo.Contains(p.Key));
            if (notUnlocked != null && notUnlocked.Any())
            {
                var reverseDes = notUnlocked?.OrderBy(p => p.Value) ?? null;
                if (reverseDes != null)
                {
                    var start = 0.5d;
                    foreach (var kvp in reverseDes)
                    {
                        val += kvp.Value * start;
                        if ((start - 0.049d) > 0) start -= 0.049d;
                        else start = 0.015d;
                    }
                }
            }
            if (val < 0) val = 0;
            return val;
        }

        [ChatCommand("avgpos")]
        private void cmdAvgPos(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var positions = BasePlayer.activePlayerList?.Where(p => p != null && !p.IsDestroyed && p.gameObject != null && p.IsConnected && p.transform != null && p != player)?.Select(p => p?.transform?.position ?? Vector3.zero)?.ToList() ?? null;
            var avgX = positions?.Average(p => p.x) ?? 0;
            var avgY = positions?.Average(p => p.y) ?? 0;
            var avgZ = positions?.Average(p => p.z) ?? 0;
            var avgPos = new Vector3(avgX, avgY, avgZ);
            SendReply(player, "avg pos is: " + avgPos);
            TeleportPlayer(player, avgPos);
        }

        [ChatCommand("wound")]
        private void cmdWound(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            //lastWoundedTime.SetValue(player, UnityEngine.Time.realtimeSinceStartup - 60);
            player.health = 100f;
            player.Hurt(99f, Rust.DamageType.Bullet, player, false);

            player.health = UnityEngine.Random.Range(2, 6);
            player.metabolism.bleeding.value = 0.0f;
            player.BecomeWounded();
            //         player.StartWounded();
        }

        private Dictionary<NetworkableId, float> turretRange = new();

        [ChatCommand("trange")]
        private void cmdTrange(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var turret = GetLookAtEntity(player, 10f, deployColl) as AutoTurret;
            if (turret == null || turret.IsDestroyed)
            {
                SendReply(player, "You must be looking at a turret to use this!");
                return;
            }
            if (args.Length < 1)
            {
                SendReply(player, "You must supply the new range. Current: " + turret.sightRange);
                return;
            }
            if (!float.TryParse(args[0], out float range))
            {
                SendReply(player, "Not a float: " + args[0]);
                return;
            }
            turretRange[turret.net.ID] = range;
            SendReply(player, "Set new range to: " + (turret.sightRange = range));
            turret.SendNetworkUpdate();
        }

        [ChatCommand("invischair")]
        private void cmdInvisChair(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var mounted = (player?.GetMounted() ?? null) as BaseChair;
            if (mounted == null || mounted.IsDestroyed || mounted?.gameObject == null || !mounted.ShortPrefabName.Contains("chair"))
            {
                SendReply(player, "You must be sitting in a chair first!");
                return;
            }
            var invisPrefab = "assets/bundled/prefabs/static/chair.invisible.static.prefab";
            var pos = mounted?.transform?.position ?? Vector3.zero;
            var rot = mounted?.transform?.rotation ?? Quaternion.identity;
            var ownerID = mounted?.OwnerID ?? 0;
            var newChair = (BaseChair)GameManager.server.CreateEntity(invisPrefab, pos, rot);
            if (newChair == null)
            {
                SendReply(player, "Couldn't create invisible chair!");
                return;
            }
            newChair.OwnerID = ownerID;
            newChair.Spawn();
            if (!mounted.IsDestroyed) mounted.Kill();
        }

        [ChatCommand("feed")]
        private void cmdFeed(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin || player.IsDead() || player?.metabolism == null) return;

            player.metabolism.calories.value = player.metabolism.calories.max;
            player.metabolism.hydration.value = player.metabolism.hydration.max;
            player.metabolism.SendChangesToClient();
        }

        [ChatCommand("starve")]
        private void cmdStarve(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin || player.IsDead() || player?.metabolism == null) return;

            player.metabolism.calories.value *= 0.25f;
            player.metabolism.hydration.value *= 0.25f;
            player.metabolism.SendChangesToClient();
        }


        [ChatCommand("nearents")]
        private void cmdNearEnts(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var plyPos = player?.transform?.position ?? Vector3.zero;
            var nearEnts = BaseNetworkable.serverEntities?.Where(p => p != null && !p.IsDestroyed && p?.transform != null && Vector3.Distance(p.transform.position, plyPos) <= 5)?.ToList() ?? null;
            var nearSB = new StringBuilder();
            var entCounts = new Dictionary<string, int>();
            for (int i = 0; i < nearEnts.Count; i++)
            {
                var ent = nearEnts[i];
                if (ent == null) continue;
                var shortName = ent?.ShortPrefabName ?? string.Empty;
                if (!entCounts.TryGetValue(shortName, out int count)) entCounts[shortName] = 1;
                else entCounts[shortName]++;
            }
            var takeAmount = (entCounts.Count < 20) ? entCounts.Count : 20;

            var top5 = (from entry in entCounts orderby entry.Value descending select entry).Take(takeAmount);
            foreach (var top in top5) nearSB.AppendLine(top.Key + " : " + top.Value);
            nearSB.AppendLine("Combined count: " + top5.Sum(p => p.Value).ToString("N0"));
            var txt = nearSB.ToString().TrimEnd();
            player.SendConsoleCommand("echo " + txt);
            SendReply(player, txt);
        }

        [ChatCommand("saving")]
        private void cmdSaving(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1;
            if (args.Length > 0 && !int.TryParse(args[0], out layer))
            {
                SendReply(player, "Not an int: " + args[0]);
                return;
            }
            var lookAt = GetLookAtEntity(player, 20f, layer);
            if (lookAt == null)
            {
                SendReply(player, "No entity");
                return;
            }
            SendReply(player, "Saving set to: " + (lookAt.enableSaving = !lookAt.enableSaving) + " for: " + lookAt.ShortPrefabName);
            if (lookAt.enableSaving && !BaseEntity.saveList.Contains(lookAt)) BaseEntity.saveList.Add(lookAt);
            if (!lookAt.enableSaving && BaseEntity.saveList.Contains(lookAt)) BaseEntity.saveList.Remove(lookAt);
        }

        private void cmdJoinMsg(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var target = args.Length < 1 ? player : FindConnectedPlayer(args[0], true);

            if (target == null)
            {
                SendReply(player, "No player found by name: " + args[0]);
                return;
            }
            if (target.IsServer)
            {
                SendReply(player, "Server doesn't have join messages, silly!");
                return;
            }
            var targetId = target?.Id ?? string.Empty;
            if (string.IsNullOrEmpty(targetId)) return;
            if (permission.UserHasPermission(targetId, "compilation.nojoinmsg"))
            {
                permission.RevokeUserPermission(targetId, "compilation.nojoinmsg");
                SendReply(player, "Enabled join messages");
            }
            else
            {
                permission.GrantUserPermission(targetId, "compilation.nojoinmsg", this);
                SendReply(player, "Disabled join messages");
            }
        }

        [ChatCommand("rhib2")]
        private void Rhib2CMD(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1f;
            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out layer))
                {
                    SendReply(player, "Bad layer: " + args[0]);
                    return;
                }
            }
            var ent = GetLookAtEntity(player, 10f, 8192) as MotorRowboat;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }
            var oldSpeed = ent.engineThrust;
            ent.engineThrust = layer;
            ent.SendNetworkUpdate();
            SendReply(player, "set speed, was: " + oldSpeed);
        }


        [ChatCommand("hspeed")]
        private void cmdChSpeed(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1f;
            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out layer))
                {
                    SendReply(player, "Bad speed: " + args[0]);
                    return;
                }
            }
            var ent = GetLookAtEntity(player, 10f, 8192) as BaseHelicopter;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }
            var oldSpeed = ent.engineThrustMax;
            ent.engineThrustMax = layer;
            ent.SendNetworkUpdate();
            SendReply(player, "set speed to: " + layer + ", was: " + oldSpeed);
        }


        [ChatCommand("chhover")]
        private void cmdCHHover(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1f;
            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out layer))
                {
                    SendReply(player, "Bad float: " + args[0]);
                    return;
                }
            }
            var ent = GetLookAtEntity(player, 10f, 8192) as CH47HelicopterAIController;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }
            var oldHeight = ent.hoverHeight;
            ent.hoverHeight = layer;
            ent.SendNetworkUpdate();
            SendReply(player, "set speed to: " + layer + ", was: " + oldHeight);
        }

        [ChatCommand("chdrop")]
        private void cmdCHDrop(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1f;
            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out layer))
                {
                    SendReply(player, "Bad float: " + args[0]);
                    return;
                }
            }
            var ent = GetLookAtEntity(player, 35f, 8192) as CH47HelicopterAIController;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }

            ent.DropCrate();

        }

        [ChatCommand("chstate")]
        private void cmdCHState(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1f;
            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out layer))
                {
                    SendReply(player, "Bad float: " + args[0]);
                    return;
                }
            }
            var ent = GetLookAtEntity(player, 30f, 8192) as CH47HelicopterAIController;

            if (ent == null)
            {
                SendReply(player, "No ent");
                return;
            }

            var brain = ent?.GetComponent<CH47AIBrain>();

            SendReply(player, "State: " + brain.CurrentState + ", out of crates: " + ent.OutOfCrates() + " mainInterest: " + brain.mainInterestPoint);

            if (args.Length > 0)
            {
                SendReply(player, "Switching to state: " + (AIState)int.Parse(args[0]));
                brain.SwitchToState((AIState)int.Parse(args[0]));
            }
        }

        public static BasePlayer GetMounted(BaseMountable mount)
        {
            if (mount == null) return null;
            var mountPly = mount.GetMounted();
            if (mountPly != null) return mountPly;
            mountPly = mount?.VehicleParent()?.GetMounted() ?? null;
            if (mountPly != null) return mountPly;
            var mountPoints = (mount as BaseVehicle)?.mountPoints ?? null;
            if (mountPoints != null && mountPoints.Count > 0) return mountPoints[0]?.mountable?.GetMounted() ?? null;
            return null;
        }


        [ChatCommand("bpbar")]
        private void cmdBpBar(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var barrel = GetLookAtEntity(player, 10f, 1) as LootContainer;
            if (barrel == null || !barrel.ShortPrefabName.Contains("barrel"))
            {
                SendReply(player, "Not a barrel");
                return;
            }
            for (int i = 0; i < 1000; i++)
            {
                if (barrel == null || barrel.IsDestroyed) break;
                var hasBP = barrel?.inventory?.itemList?.Any(p => p?.IsBlueprint() ?? false) ?? false;
                if (hasBP)
                {
                    PrintWarning("Got barrel BP after: " + i + " tries");
                    break;
                }
                barrel.SpawnLoot();

            }
            SendReply(player, "Attempted to spawn BP");
        }


        [ChatCommand("svfps")]
        private void cmdSVFps(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var current = Performance.current;
            // var report = Performance.report;
            var str = "FPS: " + current.frameRate + ", avg: " + current.frameRateAverage + ", frametime: " + current.frameTime + ", frametime avg: " + current.frameTimeAverage + ", invokehandler tasks: " + current.invokeHandlerTasks + ", loadbalancertasks: " + current.loadBalancerTasks + "\n\n" + "Memory allocations: " + current.memoryAllocations + ", collections: " + current.memoryCollections + ", usage system: " + current.memoryUsageSystem + ", ping: " + current.ping;
            // var str2 = "FPS: " + report.frameRate + ", avg: " + report.frameRateAverage + ", frametime: " + report.frameTime + ", frametime avg: " + report.frameTimeAverage + ", invokehandler tasks: " + report.invokeHandlerTasks + ", loadbalancertasks: " + report.loadBalancerTasks + "\n\n" + "Memory allocations: " + report.memoryAllocations + ", collections: " + report.memoryCollections + ", usage system: " + report.memoryUsageSystem + ", ping: " + report.ping;
            SendReply(player, str);
        }

        [ChatCommand("enttest")]
        private void cmdentTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var findEnt = BaseNetworkable.serverEntities?.Where(p => p == player)?.FirstOrDefault() ?? null;
            if (findEnt == null)
            {
                SendReply(player, "Couldn't find... you.");
                return;
            }
            BasePlayer ply = null;
            TimeSpan time1;
            TimeSpan time2;
            var watch = Stopwatch.StartNew();
            ply = findEnt as BasePlayer;
            watch.Stop();
            time1 = watch.Elapsed;
            watch.Reset();
            watch.Start();
            ply = findEnt.GetComponent<BasePlayer>();
            watch.Stop();
            time2 = watch.Elapsed;
            SendReply(player, "Entity as BasePlayer: " + time1.TotalMilliseconds + "ms, Entity.GetComponent<BasePlayer>: " + time2.TotalMilliseconds + "ms");
            var lower = (time1 < time2) ? time1 : time2;
            SendReply(player, "Better time: " + ((time1 < time2) ? "as BasePlayer" : "GetComponent<BasePlayer>"));
            watch.Reset();
            watch.Start();
            var strMatch = player.ShortPrefabName == "aaaaaaaaaa";
            watch.Stop();
            var strTime = watch.Elapsed;
            watch.Reset();
            watch.Start();
            var isMatch = player is BasePlayer;
            watch.Stop();
            var isTime = watch.Elapsed;
            SendReply(player, "String match: " + strTime.TotalMilliseconds + "ms, is BasePlayer: " + isTime.TotalMilliseconds + "ms");
            lower = (strTime < isTime) ? strTime : isTime;
            SendReply(player, "Better time: " + ((strTime < isTime) ? "string match" : "is BasePlayer"));
        }

        [ChatCommand("itemfloat")]
        private void cmdItemFloat(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var held = player?.GetActiveItem() ?? null;
            if (held == null) return;
            var groundPos = GetGroundPosition(player.transform.position);
            if (groundPos == Vector3.zero)
            {
                SendReply(player, "no ground pos!!");
                return;
            }
            groundPos.y += 0.8f;
            var dropped = held.Drop(groundPos, Vector3.zero);
            if (dropped == null)
            {
                SendReply(player, "No drop!");
                return;
            }
            var rigid = dropped?.GetComponent<Rigidbody>() ?? null;
            if (rigid != null)
            {
                rigid.useGravity = false;
                rigid.isKinematic = true;
            }

        }

        private void MakeItemFloat(Item item, Vector3 dropPos)
        {
            if (item == null || dropPos == Vector3.zero) return;
            var groundPos = GetGroundPosition(dropPos);
            if (groundPos == Vector3.zero) return;
            var drop = item.Drop(new Vector3(groundPos.x, groundPos.y + 0.7f, groundPos.z), Vector3.zero);
            if (drop == null)
            {
                RemoveFromWorld(item);
                return;
            }
            var rigid = drop?.GetComponent<Rigidbody>() ?? null;
            if (rigid != null)
            {
                rigid.useGravity = false;
                rigid.isKinematic = true;
            }
        }


        [ChatCommand("cupbupdate")]
        private void cmdCupbfix(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var vals = (BuildingPrivlidge.UpkeepBracket[])upkeepBrackets.GetValue(null);
            if (vals == null || vals.Length < 1)
            {
                PrintWarning("no vals!");
                return;
            }
            var valSB = new StringBuilder();
            for (int i = 0; i < vals.Length; i++)
            {
                var val = vals[i];
                valSB.AppendLine(val.objectsUpTo + ", " + val.fraction + ", " + val.blocksTaxPaid);
            }
            PrintWarning(valSB.ToString().TrimEnd());
            BuildingPrivlidge.UpkeepBracket[] newBrackets = new BuildingPrivlidge.UpkeepBracket[4]
 {
    new BuildingPrivlidge.UpkeepBracket(ConVar.Decay.bracket_0_blockcount, ConVar.Decay.bracket_0_costfraction),
    new BuildingPrivlidge.UpkeepBracket(ConVar.Decay.bracket_1_blockcount, ConVar.Decay.bracket_1_costfraction),
    new BuildingPrivlidge.UpkeepBracket(ConVar.Decay.bracket_2_blockcount, ConVar.Decay.bracket_2_costfraction),
    new BuildingPrivlidge.UpkeepBracket(ConVar.Decay.bracket_3_blockcount, ConVar.Decay.bracket_3_costfraction)
 };
            upkeepBrackets.SetValue(null, newBrackets);
            SendReply(player, "Updated upkeepBrackets");
        }

        [ChatCommand("cdtest")]
        private void cmdShieldTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var times = 0;
            ShowPopup(player, "5 secs", 1f);
            Action shieldAct = null;
            shieldAct = new Action(() =>
            {
                if (player == null || !player.IsConnected || player.IsDead()) return;
                if (times == 4)
                {
                    ShowPopup(player, "Done!", 1.25f);
                    InvokeHandler.CancelInvoke(player, shieldAct);
                    return;
                }
                times++;
                var size = times == 0 ? 16 : times == 1 ? 14 : times == 2 ? 12 : times == 3 ? 9 : 8;
                var showMsg = "<size=" + size + ">" + (5 - times) + " secs</size>";
                ShowPopup(player, showMsg, 1f);
            });
            InvokeHandler.InvokeRepeating(player, shieldAct, 0.98f, 0.98f);
        }

        [ChatCommand("ch47att")]
        private void cmdCH47AttachTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var lookPos = player.eyes.position + (player.eyes.HeadForward() * 5f);
            lookPos = new Vector3(lookPos.x, lookPos.y + 8f, lookPos.z);
            var chEnt = (CH47Helicopter)GameManager.server.CreateEntity("assets/prefabs/npc/ch47/ch47.entity.prefab", lookPos);
            if (chEnt == null)
            {
                SendReply(player, "chEnt null");
                return;
            }
            chEnt.Spawn();
            var chCenter = chEnt.CenterPoint();
            var sedanPos = new Vector3(chCenter.x, chCenter.y - 4f, chCenter.z);

            var sedan = GameManager.server.CreateEntity("assets/content/vehicles/boats/rowboat/rowboat.prefab", sedanPos);
            if (sedan == null)
            {
                SendReply(player, "sedan is null");
                return;
            }
            sedan.Spawn();

            sedan.SetParent(chEnt);
            sedan.transform.localPosition = new Vector3(0, -4, 0);
            sedan.SendNetworkUpdate();
        }

        [ChatCommand("ch47oil")]
        private void cmdCH47Oil(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var ents = Pool.GetList<CH47ReinforcementListener>();
            try
            {
                Vis.Entities(player.transform.position, 10f, ents);

                SendReply(player, "Found listeners: " + ents.Count);

                if (ents.Count < 1)
                {
                    SendReply(player, "No listeners found, can't call");
                    return;
                }

                for (int i = 0; i < ents.Count; i++)
                {
                    var ent = ents[i];

                    ent.Call();
                    SendReply(player, "called for " + ent);
                }

            }
            finally { Pool.FreeList(ref ents); }
        }

        [ChatCommand("dcol")]
        private void cmdDcol(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var layer = -1;
            if (args.Length > 0 && !int.TryParse(args[0], out layer))
            {
                SendReply(player, "Not int layer: " + args[0]);
                return;
            }
            var getLook = GetLookAtEntity(player, 25f, layer);
            if (getLook == null || getLook.IsDestroyed) return;
            DisableCollision(getLook, player);
            DisableCollision(player, getLook);
            getLook.SendNetworkUpdate();
            player.SendNetworkUpdate();
            SendReply(player, "Disabled collision");
        }


        [ChatCommand("kctest")]
        private void cmdKcTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var forwardPos = Vector3.zero;
            if (UnityEngine.Physics.Raycast(new Ray(player.eyes.position, GetPlayerRotation(player)), out RaycastHit rayInfo, 8f))
            {
                forwardPos = rayInfo.point;
                PrintWarning("forwardPos: " + forwardPos);
            }
            else PrintWarning("no ray hit");
            if (forwardPos == Vector3.zero) forwardPos = player.eyes.position + (player.eyes.HeadForward() * 5f);
            var prefab = (args.Length > 1) ? args[1] : "assets/bundled/prefabs/radtown/loot_barrel_1.prefab";
            var kcEnt = GameManager.server.CreateEntity(prefab, forwardPos);
            if (kcEnt == null) return;
            kcEnt.Spawn();
            DisableCollision(player, kcEnt);
            DisableCollision(kcEnt, player);
            var up = true;
            var smooth = kcEnt?.GetComponent<SmoothMovement>() ?? null;
            if (smooth == null)
            {
                smooth = kcEnt.gameObject.AddComponent<SmoothMovement>();
                smooth.speed = args.Length > 0 && float.TryParse(args[0], out float outFloat) ? outFloat : UnityEngine.Random.Range(0.075f, 0.625f);
                PrintWarning("smooth speed: " + smooth.speed);
            }
            var startPosY = kcEnt?.transform?.position.y ?? 0f;
            kcEnt.InvokeRepeating(() =>
            {
                if (kcEnt == null || kcEnt.transform == null || kcEnt.IsDestroyed) return;
                Vector3 newDest;
                if (up)
                {
                    up = false;
                    newDest = new Vector3(kcEnt.transform.position.x, kcEnt.transform.position.y + 0.58f, kcEnt.transform.position.z);
                }
                else
                {
                    up = true;
                    newDest = new Vector3(kcEnt.transform.position.x, startPosY, kcEnt.transform.position.z);
                }
                //      newDest = Vector3.Lerp(kcEnt.transform.position, newDest, UnityEngine.Time.deltaTime * 4f);
                // PrintWarning("old pos: " + kcEnt.transform.position + ", new: " + newDest);
                smooth.startTime = UnityEngine.Time.time;
                smooth.startPos = kcEnt.transform.position;
                smooth.endPos = newDest;
                //kcEnt.transform.position = newDest;
                //  rigidBody.AddForce(0, (up ? 1f : -1f), 0, ForceMode.Force);
                //  kcEnt.SendNetworkUpdateImmediate();

            }, 0.25f, UnityEngine.Random.Range(0.5f, 2f));
            kcEnt.InvokeRepeating(() =>
            {
                if (kcEnt == null || kcEnt.transform == null || kcEnt.IsDestroyed) return;
                kcEnt.transform.Rotate(0, UnityEngine.Random.Range(0.5f, 0.625f), 0, Space.Self);
                kcEnt.SendNetworkUpdateImmediate();
            }, 0.025f, 0.025f);

            var sphereCollider = kcEnt.gameObject.AddComponent<SphereTrigger>();
            sphereCollider.Radius = 1.25f;
            sphereCollider.Name = "dogtag";
            NextTick(() =>
            {
                PrintWarning("Sphere radius: " + sphereCollider.Radius);
            });

        }

        private void OnTriggerEnterSphere(SphereTrigger trigger, Collider col)
        {
            if (trigger == null || col == null) return;
            if (trigger.Name != "dogtag") return;
            var player = col?.GetComponentInParent<BasePlayer>() ?? null;
            if (player == null) return;
            // PrintWarning("Player entered sphere trigger: " + player.displayName);
            if (!player.IsConnected) return;


            var ent = trigger?.GetComponentInParent<BaseEntity>() ?? trigger?.parentObject?.GetComponentInParent<BaseEntity>() ?? null;
            if (ent == null)
            {
                PrintWarning("ent trigger is null");
                return;
            }
            else
            {
                if (!ent.IsDestroyed)
                {
                    ent.Kill();
                    SendReply(player, "Dogtag collected!");
                }
            }
        }

        private class PlaneCol : MonoBehaviour
        {
            public CargoPlane Plane;

            private void Awake()
            {
                Plane = GetComponent<CargoPlane>();
                if (Plane == null || Plane.IsDestroyed) DoDestroy();
                Interface.Oxide.LogWarning("awake for col");
            }

            private void OnCollisionEnter(Collision coll)
            {
                Interface.Oxide.LogWarning("Plane col entered: " + coll);
            }

            private void DoDestroy() => Destroy(this);

        }

        private class SphereTrigger : MonoBehaviour
        {
            public GameObject parentObject;
            private float _radius;
            public float Radius
            {
                get { return _radius; }
                set
                {
                    _radius = value;
                    UpdateColliders();
                }
            }
            public string Name
            {
                get { return gameObject?.name; }
                set
                {
                    if (gameObject != null) gameObject.name = value;
                }
            }

            private void UpdateColliders()
            {
                var sphereCollider = gameObject.GetComponent<SphereCollider>();
                if (Radius <= 0.0f && sphereCollider != null)
                {
                    Destroy(sphereCollider);
                    return;
                }
                if (Radius <= 0.0f) return;
                if (sphereCollider == null) sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = Radius;
                sphereCollider.enabled = true;
            }

            private void Awake()
            {
                gameObject.layer = (int)Rust.Layer.Reserved1; //hack to get all trigger layers...otherwise child zones
                gameObject.name = "KC";


                var rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.detectCollisions = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                parentObject = GetComponent<BaseEntity>()?.gameObject ?? null;
                if (parentObject == null)
                {
                    Interface.Oxide.LogWarning("parentObject is null!");
                    DoDestroy();
                    return;
                }
                UpdateColliders();
            }
            public void DoDestroy()
            {
                Destroy(this);
            }

            private void OnTriggerEnter(Collider col)
            {
                if (col == null) return;
                Interface.Oxide.CallHook("OnTriggerEnterSphere", this, col);
            }

            private void OnTriggerExit(Collider col)
            {
                if (col == null) return;
                Interface.Oxide.CallHook("OnTriggerExitSphere", this, col);
            }

        }


        [ChatCommand("oneloot")]
        private void cmdOneLoot(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var container = GetLookAtEntity(player, 10f, deployColl) as StorageContainer;
            if (container == null || container.IsDestroyed)
            {
                SendReply(player, "No container");
                return;
            }
            SendReply(player, "onlyOneUser set to: " + (container.onlyOneUser = !container.onlyOneUser));
            container.SendNetworkUpdate();
        }


        [ChatCommand("triforce")]
        private void cmdTriForce(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var msg = "\n   ▲\n▲ ▲";
            SimpleBroadcast(msg);
        }

        private readonly Dictionary<string, NPCAutoTurret> turretLooks = new();
        [ChatCommand("turretlook")]
        private void cmdTurretLook(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (turretLooks.TryGetValue(player.UserIDString, out NPCAutoTurret turret))
            {
                SendReply(player, "Disabled");
                turretLooks.Remove(player.UserIDString);
                return;
            }
            turret = GetLookAtEntity(player, 20f, deployColl) as NPCAutoTurret;
            if (turret == null || turret.IsDestroyed)
            {
                SendReply(player, "No turret");
                return;
            }
            turretLooks[player.UserIDString] = turret;
            SendReply(player, "Enabled");
        }

        [ChatCommand("vmname")]
        private void cmdVmName(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "You must specify a name to use!");
                return;
            }
            var vending = GetLookAtEntity(player, 10f, _constructionColl) as VendingMachine;
            if (vending == null || vending.IsDestroyed)
            {
                SendReply(player, "No vending machine found");
                return;
            }
            var newArg = args[0].Replace("\n", Environment.NewLine);
            vending.shopName = newArg;
            vending.UpdateMapMarker();
            vending.SendNetworkUpdate();
            SendReply(player, "Set VM name to: " + newArg);
        }


        [ChatCommand("sethp")]
        private void cmdSetHp(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length < 1 || !float.TryParse(args[0], out float hp))
            {
                SendReply(player, "No or bad args");
                return;
            }

            var lookAt = args.Length > 1 && args[1].Equals("self", StringComparison.OrdinalIgnoreCase) ? player : GetLookAtEntity(player) as BaseCombatEntity;
            if (lookAt == null || lookAt.IsDestroyed)
            {
                SendReply(player, "Must be looking at an entity with HP");
                return;
            }

            lookAt.health = hp;
            lookAt.SendNetworkUpdate();
            SendReply(player, "Set HP to: " + lookAt.health);
        }


        [ChatCommand("actest")]
        private void cmdAcTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.0 0.0 0.0 0.0"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "0.05 0.150"
                }
            }, "Under", "AcText");



            var acText = new CuiElement
            {
                Name = CuiHelper.GetGuid(),
                Parent = "AcText",
                Components =
                        {
                            new CuiTextComponent { Color = "1 0 0 1", Text = "Auto chat is active", FontSize = 13, Align = TextAnchor.MiddleCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0.00 0.00", AnchorMax = "1 1" }
                        }
            };
            elements.Add(acText);
            CuiHelper.DestroyUi(player, "AcText");
            CuiHelper.AddUi(player, elements);
            SendReply(player, "actest");
        }
        private readonly HashSet<MapMarkerGenericRadius> dropMarkers = new();

        private void AttachMarkerToDrop(SupplyDrop drop, Color color = default, Color outlineColor = default)
        {
            if (drop == null || drop.IsDestroyed || drop?.transform == null) return;
            var dropPos = drop?.transform?.position ?? Vector3.zero;
            if (dropPos == Vector3.zero) return;
            var newMap = (MapMarkerGenericRadius)GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", drop.CenterPoint());
            newMap.radius = 0.225f;
            newMap.color1 = (color != default) ? color : new Color(10f, 10f, 0.15f, 1f);
            newMap.color2 = (outlineColor != default) ? outlineColor : new Color(1f, 0f, 0f, 1f);
            newMap.alpha = (color != default) ? color.a : newMap.color1.a;
            newMap.Spawn();
            dropMarkers.Add(newMap);
            newMap.SendUpdate();
            newMap.SendNetworkUpdateImmediate(true);
            newMap.InvokeRepeating(() =>
            {
                if (drop == null || drop.IsDestroyed) newMap.Kill();
            }, 4f, 4f);
        }

        [ChatCommand("dropmark")]
        private void cmdDropMark(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var findDrop = UnityEngine.Object.FindObjectsOfType<SupplyDrop>()?.FirstOrDefault() ?? null;
            if (findDrop == null || findDrop.IsDestroyed || findDrop.transform == null || findDrop.transform.position == Vector3.zero)
            {
                SendReply(player, "no drop");
                return;
            }
            AttachMarkerToDrop(findDrop as SupplyDrop);
        }

        [ChatCommand("ch47debug")]
        private void cmdChDebug(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var findCh = UnityEngine.Object.FindObjectOfType<CH47HelicopterAIController>();
            if (findCh == null)
            {
                SendReply(player, "No ch47 active");
                return;
            }
            var chSB = new StringBuilder();
            chSB.AppendLine("Can drop crate: " + findCh.CanDropCrate() + ", out of crates: " + findCh.OutOfCrates());
            SendReply(player, chSB.ToString().TrimEnd());
        }

        [ChatCommand("ch47drop")]
        private void cmdChDrop(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var findCh = UnityEngine.Object.FindObjectOfType<CH47HelicopterAIController>();
            if (findCh == null || findCh.IsDestroyed || findCh.IsDead())
            {
                SendReply(player, "No ch47 active");
                return;
            }
            if (!findCh.CanDropCrate())
            {
                SendReply(player, "Can't drop");
                return;
            }
            findCh.DropCrate();
        }

        [ChatCommand("mybps")]
        private void cmdMyBps(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var info = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerInfo(player.userID);
            if (info == null)
            {
                SendReply(player, "Null info, shouldn't happen");
                return;
            }
            if (info?.unlockedItems == null || info.unlockedItems.Count < 1)
            {
                SendReply(player, "No unlocked items!");
                return;
            }
            var unlockSB = new StringBuilder();
            for (int i = 0; i < info.unlockedItems.Count; i++)
            {
                var item = info.unlockedItems[i];
                var def = ItemManager.FindItemDefinition(item);
                if (def == null) continue;
                else unlockSB.AppendLine(def.displayName.english + " : " + def.shortname + " : " + def.itemid + " (" + item + ")");
            }
            SendReply(player, "Unlocked BPs: " + Environment.NewLine + unlockSB.ToString().TrimEnd());
        }

        private void cmdSetOldWipe(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            if (!DateTime.TryParse(args[0], out DateTime outTime))
            {
                SendReply(player, "Not a date time: " + args[0]);
                return;
            }
            SendReply(player, "Set last wipe date to: " + (SaveRestore.SaveCreatedTime = outTime));
        }




        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item item)
        {
            if (player == null || item == null) return;

            var held = item?.GetHeldEntity() as HeldEntity;
            if (held == null) return;

            if (lastLightState.TryGetValue(player.UserIDString, out bool doLight)) held.SendMessage("SetLightsOn", doLight, SendMessageOptions.DontRequireReceiver);
        }


        [ChatCommand("maincap")]
        private void cmdMainCap(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            if (!int.TryParse(args[0], out int cap))
            {
                SendReply(player, "Not a number: " + args[0]);
                return;
            }
            var oldcap = player.inventory.containerMain.capacity;
            player.inventory.containerMain.capacity = cap;
            SendReply(player, "set inventory cap: " + cap + ", was: " + oldcap);
            player.inventory.ServerUpdate(0.01f);
            player.SendNetworkUpdate();
        }




        [ChatCommand("tlr")]
        private void cmdLR(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (!targetRockets.Contains(player.userID)) targetRockets.Add(player.userID);
            else targetRockets.Remove(player.userID);
            SendReply(player, "Enabled: " + targetRockets.Contains(player.userID));
        }

        [ChatCommand("setpos")]
        private void cmdSetPos(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var lookEnt = GetLookAtEntity(player, 50);
            if (lookEnt == null || lookEnt.IsDestroyed || lookEnt?.transform == null)
            {
                SendReply(player, "No or invalid look at entity");
                return;
            }
            if (args.Length < 2) return;
            if (!float.TryParse(args[1], out float newVal))
            {
                SendReply(player, "Not a proper number: " + newVal);
                return;
            }
            var lower0 = args[0].ToLower();
            if (lower0 != "x" && lower0 != "y" && lower0 != "z")
            {
                SendReply(player, "Invalid: " + lower0 + ", should be x, y, or z");
                return;
            }
            if (lower0 == "x") lookEnt.transform.position = new Vector3(newVal, lookEnt.transform.position.y, lookEnt.transform.position.z);
            if (lower0 == "y") lookEnt.transform.position = new Vector3(lookEnt.transform.position.x, newVal, lookEnt.transform.position.z);
            if (lower0 == "z") lookEnt.transform.position = new Vector3(lookEnt.transform.position.x, lookEnt.transform.position.y, newVal);
            lookEnt.SendNetworkUpdateImmediate();
        }

        [ChatCommand("getpos")]
        private void cmdGetPos(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var lookEnt = GetLookAtEntity(player, 50);
            if (lookEnt == null || lookEnt.IsDestroyed || lookEnt?.transform == null)
            {
                SendReply(player, "No or invalid look at entity");
                return;
            }
            SendReply(player, lookEnt.transform.position + " <--");
        }


        public List<Item> banHammers = new();
        private readonly Dictionary<string, string> bhReason = new();
        public List<ItemId> kickHammers = new();
        [ChatCommand("bhammer")]
        private void cmdBhammer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length > 0 && args.Length < 2)
            {
                var reason = args[0];
                bhReason[player.UserIDString] = reason;
                SendReply(player, "Set ban hammer ban (or kick) reason to: " + reason);
                return;
            }

            var hammerItem = ItemManager.CreateByName("hammer.salvaged", 1, 962567716);
            if (hammerItem == null)
            {
                SendReply(player, "Got bad hammer item!");
                return;
            }
            if (!hammerItem.MoveToContainer(player.inventory.containerBelt) && !hammerItem.MoveToContainer(player.inventory.containerMain))
            {
                SendReply(player, "Could not move hammer to belt/main!");
                return;
            }
            hammerItem.name = "<color=red>Banhammer</color>";
            banHammers.Add(hammerItem);
            if (args.Length > 1)
            {
                if (args[1].ToLower() == "kick")
                {
                    hammerItem.name = "<color=orange>Kickhammer</color>";
                    kickHammers.Add(hammerItem.uid);
                    SendReply(player, "Set to kick hammer");
                }
            }
            SendReply(player, "Gave hammer");
        }

        private readonly HashSet<string> lockOverride = new();

        private object CanUseLockedEntity(BasePlayer player, BaseLock baseLock)
        {
            if (player == null || baseLock == null) return null;
            if (lockOverride.Contains(player.UserIDString))
            {
                PrintWarning("lockOverride: " + player.UserIDString);
                return true;
            }
            return null;
        }

        private object CanUseGesture(BasePlayer player, GestureConfig gesture)
        {
            if (player == null || gesture == null) return null;

            if ((gesture.dlcItem != null && !gesture.dlcItem.CanUse(player)) || (gesture.inventoryItem != null && !player.blueprints.steamInventory.HasItem(gesture.inventoryItem.id)))
            {
                PrintWarning("force returning true for: " + gesture.convarName);
                return true;
            }

            return null;
        }

        /*/
        void OnItemPickup(Item item, BasePlayer player)
        {
            if (item == null || player == null) return;
            var parent = player?.GetParentEntity() ?? null;
            if (parent == null || parent.ShortPrefabName != "cargoshiptest") return;
            
            player.SetParent(null, true, true);
            PrintWarning("set parent null");
            NextTick(() =>
            {
                if (player == null || player.IsDestroyed || player.IsDead() || player?.gameObject == null)
                {
                    PrintWarning("player fucked on nexttick");
                    return;
                }
                if (parent == null || parent.IsDestroyed || parent?.gameObject == null)
                {
                    PrintWarning("parent fucked on nxettick");
                    return;
                }
                player.SetParent(parent, true, true);
                PrintWarning("re-set parent after null set");
            });
           
        }/*/

        private object CanStackItem(Item item, Item targetItem)
        {
            if (item == null || targetItem == null) return null;

            try
            {
                if (item.hasCondition && item.condition < item.maxCondition)
                {
                    var canStack = targetItem != item && item.info.stackable > 1 && targetItem.info.stackable > 1 && targetItem.info.itemid == item.info.itemid && item.IsValid() && (!item.IsBlueprint() || item.blueprintTarget == item.blueprintTarget);
                    if (canStack)
                    {
                        var higher = item.conditionNormalized > targetItem.conditionNormalized ? item.conditionNormalized : targetItem.conditionNormalized;
                        var comp = higher == item.conditionNormalized ? targetItem.conditionNormalized : item.conditionNormalized;
                        if ((higher - comp) > 0.02) return false; //0.12 original
                        return true;
                    }
                }
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return null;
        }
        /*/
        private object OnItemSplit(Item item, int amount)
        {
            if (item == null || amount < 1) return null;
            if (string.IsNullOrEmpty(item?.name) && item.skin == 0 && (!item.hasCondition || item.condition >= item.maxCondition) && !item.info.shortname.StartsWith("weapon.mod")) return null;
            PrintWarning("onitemsplit for: " + item.info.shortname + " x" + amount + " (" + item.amount + "), item.amount will be reduced by split amount, it'll be: " + (item.amount - amount));
            var oldParent = item?.parent ?? null;
            var oldPos = item?.position ?? -1;
            item.amount -= amount;
            var byItemId = ItemManager.CreateByItemID(item.info.itemid, 1, (item?.skin ?? 0));
            byItemId.amount = amount;
            if (item.IsBlueprint()) byItemId.blueprintTarget = item.blueprintTarget;
            item.MarkDirty();
            byItemId.name = item.name;
            if (item.hasCondition && byItemId.hasCondition)
            {
                byItemId.maxCondition = item.maxCondition;
                byItemId.condition = item.condition;
            }
            byItemId.MarkDirty();
            if (oldParent != null)
            {
                NextTick(() =>
                {
                    if (byItemId.parent != null && byItemId.parent != oldParent)
                    {
                        var icp = new ItemContainerPosition
                        {
                            Item = byItemId,
                            Position = -1,
                            Container = oldParent
                        };
                        PrintWarning("added byItemId to removedFrom in onitemsplit");
                        removedFrom.Add(icp);
                    }
                });
            }
            return byItemId;
        }/*/

        private object OnMaxStackable(Item item)
        {
            if ((item?.parent?.entityOwner?.prefabID ?? 0) != 2476970476) return null; //tool cupboard

            if (item.info.category != ItemCategory.Resources)
                return null;

            // if (item.info.shortname != "wood" && item.info.shortname != "metal.fragments" && item.info.shortname != "stones" && item.info.shortname != "metal.refined") return null;

            var maxStack = item?.info?.stackable ?? 0;

            var val = maxStack * 2;

            var hookVal = Interface.Oxide.CallHook("OnOverrideMaxStackableForCupboard", item, maxStack);
            if (hookVal != null)
                val = (int)hookVal;

            return val;
        }

        //why does below exist?
        private void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null) return;
            if (!item.info.shortname.StartsWith("weapon.mod")) return;
            ItemContainerPosition removed = null;
            foreach (var p in removedFrom)
            {
                if (p.Item == item)
                {
                    removed = p;
                    break;
                }
            }
            if (removed == null)
            {
                //    PrintWarning("removed is null for: " + item.info.shortname);
                return;
            }
            var act = new Action(() =>
            {
                if (removed == null || item == null) return;
                var oldPos = removed.Position;
                var oldParent = removed.Container;
                var flagIcon = container.HasFlag(ItemContainer.Flag.ShowSlotsOnIcon);
                var broken = container.HasFlag(ItemContainer.Flag.NoBrokenItems);
                if (flagIcon && broken)
                {
                    //  PrintWarning("flagIcon & broken");
                    if (item.amount > 1)
                    {
                        var toReturn = item.amount - 1;
                        //   PrintWarning("toReturn is item.amount - 1: " + "(" + item.amount + " - 1: " + (item.amount - 1) + ")");
                        var split = ItemManager.Create(item.info, toReturn);
                        if (split == null) PrintWarning("split is null!!");
                        else
                        {
                            var didMove = false;
                            if (!split.MoveToContainer(oldParent, oldPos))
                            {
                                PrintWarning("couldn't move split: " + split.info.shortname + " x" + split.amount + " to oldParent, oldPos: " + oldPos + ", has parent?: " + (split.parent != null) + ", current pos: " + split.position);
                                if (!split.MoveToContainer(oldParent))
                                {
                                    PrintWarning("couldn't move split to oldParent with ANY position!!");
                                    RemoveFromWorld(split);
                                }
                                else didMove = true;
                            }
                            else didMove = true;
                            if (didMove)
                            {
                                item.amount -= split.amount;
                                item.MarkDirty();
                                split.condition = item.condition;
                                split.MarkDirty();
                            }
                        }

                    }
                    //  else PrintWarning("item.amount not > 1: " + item.amount + " for: " + item.info.shortname);
                }
            });
            if (removed?.Container != null)
            {
                // PrintWarning("removed.container is not null, invoking NOW");
                act.Invoke();
            }
            else
            {
                //  PrintWarning("removed.container was null, invoking nexttick IF not null");
                NextTick(() =>
                {
                    foreach (var p in removedFrom)
                    {
                        if (p.Item == item)
                        {
                            removed = p;
                            break;
                        }
                    }
                    if (removed?.Container != null)
                    {
                        PrintWarning("not null on nexttick, invoking!");
                        act.Invoke();
                    }
                });
            }
        }

        private class ItemContainerPosition
        {
            public Item Item { get; set; }
            public ItemContainer Container { get; set; }
            public int Position { get; set; } = -1;

            public ItemContainerPosition() { }
        }

        private readonly HashSet<ItemContainerPosition> removedFrom = new();

        private void OnItemRemovedFromContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null) return;
            var oldParent = item?.parent ?? container ?? null;
            var oldPos = item?.position ?? -1;
            if (item.info.shortname.StartsWith("weapon.mod"))
            {
                // PrintWarning("we're adding: " + item.info.shortname + " to removedFrom");
                var icp = new ItemContainerPosition
                {
                    Item = item,
                    Position = oldPos,
                    Container = oldParent
                };
                removedFrom.Add(icp);
            }
            var entity = container?.entityOwner ?? null;


            if (entity == null || entity.prefabID != 2476970476) return; //tool cupboard

            NextTick(() =>
            {
                if (item == null) return;
                var newParent = item?.parent ?? null;
                if (newParent == null || newParent == oldParent) return;
                var stackable = item?.info?.stackable ?? 0;
                var amount = item?.amount ?? 0;
                if (amount > stackable)
                {
                    var diff = Mathf.Clamp(amount - stackable, 1, int.MaxValue);
                    var newItem = ItemManager.Create(item.info, diff);
                    if (newItem != null && !newItem.MoveToContainer(newParent) && !newItem.MoveToContainer(oldParent, oldPos) && !newItem.MoveToContainer(oldParent))
                    {
                        RemoveFromWorld(newItem);
                        PrintWarning("Couldn't move newItem!! amount: " + diff);
                    }
                    else
                    {
                        if ((item.amount - diff) <= 0) RemoveFromWorld(item);
                        else
                        {
                            item.amount -= diff;
                            item.MarkDirty();
                        }
                    }
                }
            });
        }


        [ChatCommand("decaynow")]
        private void cmdDecay(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var lookAt = GetLookAtEntity(player);
            if (lookAt == null || lookAt.IsDestroyed)
            {
                PrintWarning("BAD LOOK");
                return;
            }
            var decayEnt = lookAt?.GetComponent<DecayEntity>() ?? null;
            if (decayEnt == null)
            {
                PrintWarning("decay ent null!");
                return;
            }
            PrintWarning("decaying!");

            var lastDecay = typeof(DecayEntity).GetField("lastDecayTick", BindingFlags.NonPublic | BindingFlags.Instance);
            lastDecay.SetValue(decayEnt, UnityEngine.Time.time - ConVar.Decay.tick);
            NextTick(() =>
            {
                if (decayEnt == null || (decayEnt?.IsDestroyed ?? true)) return;
                decayEnt.DecayTick();
                lookAt.SendNetworkUpdate();
                PrintWarning("did do decay tack");
            });

        }

        [ChatCommand("cuptest")]
        private void cmdCup(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var lookAt = GetLookAtEntity(player);
            if (lookAt == null || lookAt.IsDestroyed)
            {
                PrintWarning("BAD LOOK");
                return;
            }
            var cupboard = lookAt?.GetComponent<BuildingPrivlidge>() ?? null;
            if (cupboard == null) return;
            SendReply(player, "Cupboard protected mins: " + cupboard.GetProtectedMinutes() + ", " + cupboard.GetProtectedMinutes(true));

        }


        [ChatCommand("unblockcr")]
        private void cmdUnblockCr(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            NoEscape?.Call("StopBlocking", player.UserIDString);
            SendReply(player, "Stopped combat/raid blocking.");
        }

        [ChatCommand("rocket")]
        private void cmdRocket(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var endPos = Vector3.zero;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 200f)) endPos = hit.point;
            if (endPos == Vector3.zero) return;
            LaunchRocket(player.eyes.position, endPos, Vector3.zero, 70, 0, trackEntity: player);
        }

        private readonly HashSet<NetworkableId> noNetworks = new();

        private System.Collections.IEnumerator SpawnSiloRockets(Vector3 startPos, Vector3 initEndPos, Vector3 newEndPos, float ogStartY, float xAdj, int amount, int speed = 80, bool incen = false)
        {
            var count = 0;
            var max = 12;
            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    yield return CoroutineEx.waitForSeconds(UnityEngine.Random.Range(0.0425f, 0.1f));
                    count++;
                    if (count >= max)
                    {
                        count = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    startPos.y = ogStartY;
                    initEndPos += UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(1.4f, 2.2f);
                    //initEndPos = SpreadVector(initEndPos, UnityEngine.Random.Range(0.8f, 1.1f));
                    if (i > 0) xAdj += UnityEngine.Random.Range(1.2f, 2.2f);
                    var offset = Vector3.zero + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(3.5f, 8f);
                    offset.y = 0;
                    var rocket = LaunchRocket(startPos, initEndPos, offset, speed, 0f, UnityEngine.Random.Range(3.5f, 4.5f), UnityEngine.Random.Range(2.7f, 4f), globalBroadcast: true, incen: incen);

                    Action posCheck = null;
                    newEndPos += UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(2.4f, 5f);

                    newEndPos.y = startPos.y += UnityEngine.Random.Range(198f, 203f);
                    posCheck = InvokeRepeating(rocket, () =>
                    {
                        if (rocket == null || rocket.IsDestroyed)
                        {
                            InvokeHandler.CancelInvoke(rocket, posCheck);
                            return;
                        }
                        var initEndY = Math.Floor(initEndPos.y);
                        var curY = Math.Floor(rocket.transform.position.y);
                        var groundPos = GetGroundPosition(newEndPos, 3);
                        // var groundCurPos = GetGroundPosition(rocket.transform.position, 3);


                        var rocketPosAdj = rocket.transform.position;
                        rocketPosAdj.y = groundPos.y;
                        var compEnd = newEndPos;
                        compEnd.y = rocketPosAdj.y;
                        var targDist = Vector3.Distance(rocketPosAdj, compEnd);
                        if (curY > initEndY)
                        {
                            //     PrintWarning("curY > initEndY: " + curY + " > " + initEndY);
                            newEndPos.y -= UnityEngine.Random.Range(1.25f, 2.3f);
                            var newVelPos = (targDist < 120) ? (groundPos - rocket.transform.position) : (newEndPos - rocket.transform.position);
                            ///  PrintWarning("targ dist: " + targDist);
                            var projectile = rocket.GetComponent<ServerProjectile>();
                            projectile?.InitializeVelocity(newVelPos);

                        }
                        if (targDist < 41f)
                        {
                            var finalPos = groundPos;
                            var oldYF = finalPos.y;
                            var rngSpread = UnityEngine.Random.Range(8f, 18f);

                            finalPos += UnityEngine.Random.insideUnitSphere * rngSpread;
                            finalPos.y = oldYF;
                            var projectile = rocket.GetComponent<ServerProjectile>();
                            projectile?.InitializeVelocity(finalPos - rocket.transform.position);
                            InvokeHandler.CancelInvoke(rocket, posCheck);
                            return;
                        }
                    }, 1f, 0.08f);
                }
            }
        }

        [ChatCommand("silo")]
        private void cmdRocketSilo(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var amount = 5;
            BasePlayer target = null;
            var incen = false;
            if (args.Length > 0)
            {
                if (!int.TryParse(args[0], out amount))
                {
                    SendReply(player, "bad amount!: " + args[0]);
                    return;
                }
                if (args.Length > 1)
                {
                    target = FindPlayerByPartialName(args[1], true);
                    if (target == null)
                    {
                        SendReply(player, "No target found: " + args[1]);
                        return;
                    }
                    if (args.Length > 2)
                    {
                        if (!bool.TryParse(args[2], out incen))
                        {
                            SendReply(player, "Not a bool: " + args[2]);
                            return;
                        }
                    }
                }
            }
            if (amount > 128) amount = 128;
            //  var startPos = new Vector3(-967, 19.25f, 815);
            var cols = UnityEngine.Object.FindObjectsOfType<BoxCollider>();
            var trigCols = cols?.Where(p => p.name == "Trigger (13)" || (p?.GetComponent<MeshCollider>() ?? null)?.sharedMesh?.name == "Trigger (13)") ?? null;
            var ladderSewerCols = cols?.Where(p => p.name == "ladder_trigger_sewers (2)" || (p?.GetComponent<MeshCollider>() ?? null)?.sharedMesh?.name == "ladder_trigger_sewers (2)") ?? null;
            var ladderCol = ladderSewerCols?.FirstOrDefault() ?? null;
            var trigCol = trigCols?.FirstOrDefault() ?? null;
            var milTunnels = UnityEngine.Object.FindObjectsOfType<MonumentInfo>()?.Where(p => p.displayPhrase.english.Contains("Military"))?.FirstOrDefault() ?? null;
            if (milTunnels != null)
            {
                var minDist = trigCols?.Min(p => Vector3.Distance(p.transform.position, milTunnels.transform.position)) ?? 0;
                var findMin = trigCols?.Where(p => Vector3.Distance(p.transform.position, milTunnels.transform.position) <= minDist)?.FirstOrDefault() ?? null;
                if (findMin != null) trigCol = findMin;
                var minDistLad = trigCols?.Min(p => Vector3.Distance(p.transform.position, milTunnels.transform.position)) ?? 0;
                var findMinLad = ladderSewerCols?.Where(p => Vector3.Distance(p.transform.position, milTunnels.transform.position) <= minDistLad)?.FirstOrDefault() ?? null;
                if (findMinLad != null) ladderCol = findMinLad;
                if (ladderCol != null)
                {
                    var minDist2 = trigCols?.Min(p => Vector3.Distance(p.transform.position, ladderCol.transform.position)) ?? 0;
                    var findMin2 = trigCols?.Where(p => Vector3.Distance(p.transform.position, ladderCol.transform.position) <= minDist2)?.FirstOrDefault() ?? null;
                    if (findMin2 != null) trigCol = findMin2;
                }
            }
            var startPos = trigCol?.transform?.position ?? Vector3.zero;
            if (startPos == Vector3.zero)
            {
                PrintWarning("startPos vec3 zero");
                return;
            }
            else PrintWarning("Got start pos: " + startPos + ", from trigCol: " + trigCol.name);
            //var startPos = GameObject.FindObjectsOfType<MonumentInfo>()?.Where(p => p.displayPhrase.english.Contains("Dome"))?.FirstOrDefault()?.transform?.position ?? Vector3.zero;
            startPos.y += 4.47f;
            var ogStartY = startPos.y;
            PrintWarning("got start pos: " + startPos);

            var initEndPos = startPos;
            initEndPos.y += 230;
            var newEndPos = (target?.transform != null) ? target.transform.position : new Vector3(26.9f, initEndPos.y, 767.7f);
            newEndPos.y = initEndPos.y;
            var xAdj = -2;
            ServerMgr.Instance.StartCoroutine(SpawnSiloRockets(startPos, initEndPos, newEndPos, ogStartY, xAdj, amount, UnityEngine.Random.Range(90, 180), incen));
            SendReply(player, "Launching " + amount.ToString("N0") + " rockets");
        }

        [ChatCommand("silotarget")]
        private void cmdRocketSiloTarget(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 2)
            {
                SendReply(player, "Bad args, supply player name and amount!");
                return;
            }
            var target = FindPlayerByPartialName(args[0], true);
            if (target == null || (target?.IsDead() ?? true))
            {
                SendReply(player, "Target is dead or not found");
                return;
            }
            if (!int.TryParse(args[1], out int amount))
            {
                SendReply(player, "bad amount!: " + args[0]);
                return;
            }
            if (amount > 32) amount = 32;
            var startPos = new Vector3(-967, 24f, 815);
            var initEndPos = startPos;
            initEndPos.y += 200;
            var newEndPos = target?.transform?.position ?? Vector3.zero;
            newEndPos.y = initEndPos.y;
            var rockets = new List<BaseEntity>();
            var xAdj = -2;
            var rocketSpeed = 70f;
            for (int i = 0; i < amount; i++)
            {
                startPos.y = UnityEngine.Random.Range(24.25f, 25f);
                initEndPos = SpreadVector(initEndPos, 0.5f);
                xAdj += 2;
                var rocket = LaunchRocket(startPos, initEndPos, new Vector3(xAdj, 0, 0), rocketSpeed);
                rocketSpeed -= 1f;
                Action posCheck = null;
                newEndPos = SpreadVector(newEndPos, UnityEngine.Random.Range(0.5f, 0.75f));
                newEndPos.y = startPos.y += 200;
                posCheck = InvokeRepeating(rocket, () =>
                {
                    if (rocket == null || (rocket?.IsDestroyed ?? true))
                    {
                        InvokeHandler.CancelInvoke(rocket, posCheck);
                        return;
                    }
                    var initEndY = Math.Floor(initEndPos.y);
                    var curY = Math.Floor(rocket.transform.position.y);

                    if (curY > initEndY)
                    {
                        // PrintWarning("Y get, pre mod endpos: " + newEndPos);
                        newEndPos.y -= UnityEngine.Random.Range(1.25f, 1.85f);
                        // PrintWarning("new end pos?: " + newEndPos);
                        var project = rocket.GetComponent<ServerProjectile>();
                        project?.InitializeVelocity(newEndPos - rocket.transform.position);
                        //   PrintWarning("init end y: " + initEndY + ", curY: " + curY);
                    }

                    var targDist = Math.Floor(Vector3.Distance(rocket.transform.position, newEndPos));
                    if (targDist <= 25f)
                    {
                        PrintWarning("DIST GET!");
                        var finalPos = newEndPos;
                        finalPos = SpreadVector(finalPos, UnityEngine.Random.Range(1.1f, 1.35f));
                        finalPos.y -= 200;
                        //        finalPos.y = UnityEngine.Random.Range(90, 92);
                        var project = rocket.GetComponent<ServerProjectile>();
                        project?.InitializeVelocity(finalPos - rocket.transform.position);

                        InvokeHandler.CancelInvoke(rocket, posCheck);
                        return;
                    }
                }, 0.25f, 0.05f);
                noNetworks.Add(rocket.net.ID);
                rockets.Add(rocket);
            }
            SendReply(player, "Launched " + rockets.Count + " rockets");
        }

        private Vector3 SpreadVector(Vector3 vec, float spread = 1.5f) { return Quaternion.Euler(UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f), UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f), UnityEngine.Random.Range((float)(-spread * 0.2), spread * 0.2f)) * vec; }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

        private readonly int terrainWorld = LayerMask.GetMask("Terrain", "World");

        private Vector3 SpreadVector3WorldOnly(Vector3 vec, float spread, int maxTries = 20)
        {
            if (maxTries > 150) maxTries = 150;
            if (maxTries < 1) maxTries = 1;
            var startPos = vec + UnityEngine.Random.insideUnitSphere * spread;
            var finalPos = startPos;
            for (int i = 0; i < maxTries; i++)
            {
                if (i > 1) spread = UnityEngine.Random.Range(spread, spread * UnityEngine.Random.Range(1.1f, 2.2f));
                var pos = vec + UnityEngine.Random.insideUnitSphere * spread;
                if (UnityEngine.Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit info, pos.y + 50, terrainWorld))
                {
                    if (info.GetEntity() != null) continue;
                    var coll = info.collider ?? info.GetCollider() ?? null;
                    if (coll == null) continue;
                    var colMat = (coll.material?.name ?? coll.sharedMaterial?.name ?? string.Empty).ToLower();
                    var colName = coll?.name ?? string.Empty;
                    if (string.IsNullOrEmpty(colName) || string.IsNullOrEmpty(colMat) || colMat.Contains("zero friction")) continue;

                    if (colName.Contains("Terrain", CompareOptions.OrdinalIgnoreCase) || colMat.Contains("grass") || colMat.Contains("ice") || colMat.Contains("snow") || colMat.Contains("sand") || colMat.Contains("dirt"))
                    {
                        finalPos = pos;
                        break;
                    }
                }
                else continue;
            }
            if (Vector3.Distance(startPos, finalPos) <= 0.1) return Vector3.zero;
            return finalPos;
        }

        private BaseEntity LaunchRocket(Vector3 startPos, Vector3 targetPos, Vector3 offset = default, float speed = 80, float gravity = 0f, float radiusScalar = 0f, float dmgScalar = 0f, BaseEntity trackEntity = null, bool globalBroadcast = false, bool incen = false)
        {

            var rocket = incen ? "ammo.rocket.fire" : "ammo.rocket.hv";
            var launchPos = startPos;

            ItemDefinition projectileItem = ItemManager.FindItemDefinition(rocket);
            ItemModProjectile component = projectileItem.GetComponent<ItemModProjectile>();
            if (offset != default)
            {
                launchPos.x += offset.x;
                launchPos.y += offset.y;
                launchPos.z += offset.z;
            }

            BaseEntity entity = GameManager.server.CreateEntity(component.projectileObject.resourcePath, launchPos, new Quaternion(), true);
            if (entity == null) return null;
            entity.Invoke(() =>
            {
                if (entity == null || (entity?.IsDestroyed ?? true)) return;
                entity.Kill();
            }, 40f);

            TimedExplosive rocketExplosion = entity as TimedExplosive;
            ServerProjectile rocketProjectile = entity.GetComponent<ServerProjectile>();

            rocketProjectile.speed = speed;
            rocketProjectile.gravityModifier = gravity;
            rocketExplosion.timerAmountMin = 60;
            rocketExplosion.timerAmountMax = 60;
            if (radiusScalar < 0 || radiusScalar > 0)
            {
                rocketExplosion.minExplosionRadius *= radiusScalar;
                rocketExplosion.explosionRadius *= radiusScalar;
            }
            if (dmgScalar > 0 || dmgScalar < 0) rocketExplosion.damageTypes.ForEach(p => p.amount *= dmgScalar);

            Vector3 newDirection = targetPos - launchPos;

            rocketProjectile?.InitializeVelocity(newDirection);

            if (globalBroadcast) entity.globalBroadcast = true;
            entity.Spawn();
            if (globalBroadcast) entity.globalBroadcast = true;
            entity.SendNetworkUpdateImmediate(true);

            if (trackEntity != null && !(trackEntity?.IsDestroyed ?? true))
            {
                Action trackInvoke = null;
                trackInvoke = InvokeRepeating(entity, () =>
                {
                    if ((entity?.IsDestroyed ?? true) || (trackEntity?.IsDestroyed ?? true))
                    {
                        InvokeHandler.CancelInvoke(entity, trackInvoke);
                        return;
                    }
                    rocketProjectile?.InitializeVelocity(trackEntity.transform.position - entity.transform.position);
                }, 0.5f, 0.1f);
            }
            return entity;
        }

        [ChatCommand("invokes")]
        private void cmdInvokes(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var invokeSB = new StringBuilder();
            var oxideOnly = false;
            if (args.Length > 0 && !bool.TryParse(args[0], out oxideOnly))
            {
                SendReply(player, "Not a bool: " + args[0]);
                return;
            }
            var exSB = new StringBuilder();
            foreach (var kvp in InvokeList)
            {
                if (oxideOnly)
                {
                    try { if (!(kvp.Key.action?.Target?.ToString() ?? string.Empty).Contains("Oxide")) continue; }
                    catch (Exception ex) { exSB.AppendLine(ex.ToString()); }
                }
                invokeSB.AppendLine("Method: " + (kvp.Key.action?.Method?.Name ?? "Unknown") + ", Target: " + (kvp.Key.action?.Target ?? null) + ", Sender: " + (kvp.Key.sender ?? null) + ", interval: " + kvp.Key.initial + ", repeat: " + kvp.Key.repeat + ", random: " + kvp.Key.random + ", key value: " + kvp.Value);
            }
            PrintWarning(invokeSB.ToString().TrimEnd());
            if (exSB.Length > 0) PrintError(exSB.ToString().TrimEnd());
        }

        [ChatCommand("heldname")]
        private void cmdHeldName(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var heldItem = player?.GetActiveItem() ?? null;
            if (heldItem != null)
            {
                heldItem.name = args.Length > 0 ? args[0] : string.Empty;
                heldItem.MarkDirty();
                SendReply(player, "Set name to: " + heldItem?.name);
            }
        }

        [ChatCommand("fillnear")]
        private void cmdFillNear(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var radius = 25f;
            if (args.Length > 0 && !float.TryParse(args[0], out radius))
            {
                SendReply(player, "bad radius");
                return;
            }


            var visList = new List<BaseEntity>();
            Vis.Entities(player.transform.position, radius, visList, deployColl);

            if (visList.Count > 0)
            {

                SendReply(player, "visList for deployColl count: " + visList.Count);

                for (int i = 0; i < visList.Count; i++)
                {
                    var entity = visList[i];
                    if (entity == null || (entity?.IsDestroyed ?? true)) continue;

                    if (entity.OwnerID == 0) continue;

                    var ownerPly = BasePlayer.FindByID(entity.OwnerID);
                    if ((ownerPly == null || !ownerPly.IsAdmin) && entity.OwnerID != 528491) continue;

                    var flame = entity as FlameTurret;
                    if (flame != null)
                    {
                        flame.maxStackSize = int.MaxValue;
                        flame.inventory.maxStackSize = int.MaxValue;
                        flame.SendNetworkUpdate();
                    }
                    var ammoname = (entity is GunTrap) ? "ammo.handmade.shell" : (entity is AutoTurret) ? (entity as AutoTurret).GetDesiredAmmo()?.shortname : (entity is FlameTurret || entity is FogMachine) ? "lowgradefuel" : (entity is SamSite) ? "ammo.rocket.sam" : string.Empty;
                    if (string.IsNullOrEmpty(ammoname))
                    {
                        PrintWarning("ammoname null for: " + entity);
                        continue;
                    }
                    var amt = 200000;
                    var newAmmo = ItemManager.CreateByName(ammoname, amt);
                    var inventory = entity?.GetComponent<StorageContainer>()?.inventory ?? null;
                    if (inventory == null) inventory = (entity as ContainerIOEntity)?.inventory ?? null;
                    if (inventory == null)
                    {
                        PrintWarning("null inv: " + (entity?.ShortPrefabName ?? "Unknown"));
                        continue;
                    }
                    else if (inventory.itemList.Count > 0) for (int j = entity is AutoTurret ? 1 : 0; j < inventory.itemList.Count; j++) RemoveFromWorld(inventory.itemList[j]);
                    if (!newAmmo.MoveToContainer(inventory)) RemoveFromWorld(newAmmo);
                    else
                    {
                        timer.Once(1f, () =>
                        {
                            if (newAmmo == null) return;
                            newAmmo.amount = amt;
                            newAmmo.MarkDirty();
                        });
                    }
                }
            }
        }

        [ChatCommand("ttn")]
        private void cmdToggleTurretNear(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var radius = 25f;
            if (args.Length > 0 && !float.TryParse(args[0], out radius))
            {
                SendReply(player, "bad radius");
                return;
            }
            var hasToggle = false;
            var toggle = false;
            if (args.Length > 1)
            {
                if (bool.TryParse(args[1], out toggle)) hasToggle = true;
                else
                {
                    SendReply(player, "Bad bool: " + args[1]);
                    return;
                }
            }
            var visList = new List<BaseEntity>();
            Vis.Entities(player.transform.position, radius, visList, deployColl);
            if (visList.Count > 0)
            {
                for (int i = 0; i < visList.Count; i++)
                {
                    var entity = visList[i];
                    if (entity == null || (entity?.IsDestroyed ?? true)) continue;
                    var turret = entity as AutoTurret;
                    if (turret == null) continue;
                    if (!hasToggle)
                    {
                        if (turret.HasFlag(BaseEntity.Flags.On)) turret.InitiateShutdown();
                        else turret.InitiateStartup();
                    }
                    else
                    {
                        if (toggle) turret.InitiateStartup();
                        else turret.InitiateShutdown();
                    }
                }
            }
        }


        [ChatCommand("cancelinvoke")]
        private void cmdCancelInvoke(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            var findInvoke = InvokeList?.Where(p => p.Key.action?.Method?.Name == args[0])?.FirstOrDefault().Key ?? null;
            if (findInvoke != null && findInvoke.HasValue)
            {
                PrintWarning("found invoke");
                SendReply(player, "Found invoke... Cancelling/attempting to: " + findInvoke.Value.action?.Method + ", " + findInvoke.Value.action?.Target);
                InvokeHandler.CancelInvoke(findInvoke.Value.sender, findInvoke.Value.action);
            }
            else
            {
                PrintWarning("no invoke found");
                SendReply(player, "Invoke not found: " + args[0]);
            }
        }

        [ChatCommand("emoji")]
        private void cmdEmoji(BasePlayer player, string command, string[] args)
        {
            //      if (!player.IsAdmin) return;
            player.SetPlayerFlag(BasePlayer.PlayerFlags.EyesViewmode, true);
            player.SendNetworkUpdateImmediate();
            var str = args.Length > 0 ? args[0] : "wave";
            player.SignalBroadcast(BaseEntity.Signal.Gesture, str, null);
            var isAdmin = player.IsAdmin;
            timer.Once(1.2f, () =>
            {
                if (player != null)
                {

                    if (isAdmin) player.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, false);
                    player.SetPlayerFlag(BasePlayer.PlayerFlags.EyesViewmode, false);
                    if (isAdmin) player.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, true);
                    player.SendNetworkUpdateImmediate();
                }

            });
        }

        [ChatCommand("rndpos")]
        private void cmdBeach(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var pos = Vector3.zero;
            var terrainSize = (ConVar.Server.worldsize / 2) - 500;
            pos.x = UnityEngine.Random.Range(-terrainSize, terrainSize);
            pos.y += 500;
            pos.z = UnityEngine.Random.Range(-terrainSize, terrainSize);
            var newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
            if (newPosObj == null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (newPosObj != null) break;
                    newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
                }
                if (newPosObj == null)
                {
                    PrintWarning("null after 10");
                    return;
                }
            }
            var newPos = (Vector3)newPosObj;
            if (newPos == Vector3.zero)
            {
                PrintWarning("vec3.zero");
                return;
            }
            TeleportPlayer(player, newPos);
        }

        private Vector3 GetGroundPosition(Vector3 pos, int maxTries = 20)
        {
            if (pos == Vector3.zero || FindGround == null || !FindGround.IsLoaded) return pos;
            var newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
            maxTries = maxTries < 1 ? 1 : maxTries > 100 ? 100 : maxTries;
            if (newPosObj == null)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    if (newPosObj != null) break;
                    newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
                }
                if (newPosObj == null)
                {
                    PrintWarning("null pos after " + maxTries.ToString("N0") + " tries");
                    return Vector3.zero;
                }
            }
            return (Vector3)newPosObj;
        }

        private readonly HashSet<string> expAmmoUsers = new();
        private readonly List<string> cannonUsers = new();
        [ChatCommand("expammo")]
        private void cmdHeldAmmo(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (!expAmmoUsers.Contains(player.UserIDString)) expAmmoUsers.Add(player.UserIDString);
            else expAmmoUsers.Remove(player.UserIDString);
            SendReply(player, (expAmmoUsers.Contains(player.UserIDString) ? "Enabled" : "Disabled") + " explosive ammo");
        }

        [ChatCommand("cannon")]
        private void cmdCannonAmmo(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length > 0)
            {
                if (!float.TryParse(args[0], out float delay))
                {
                    SendReply(player, "Not a float: " + args[0]);
                    return;
                }
                cannonRate[player.UserIDString] = delay;
                SendReply(player, "Set cannon fire rate to " + delay);
                return;
            }
            if (!cannonUsers.Contains(player.UserIDString)) cannonUsers.Add(player.UserIDString);
            else cannonUsers.Remove(player.UserIDString);
            SendReply(player, (cannonUsers.Contains(player.UserIDString) ? "Enabled" : "Disabled") + " cannon ammo");
        }

        [ChatCommand("durperc")]
        private void cmdDurPerc(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var heldItem = player?.GetActiveItem() ?? null;
            if (heldItem == null) return;
            if (args.Length < 1)
            {
                SendReply(player, "Please supply a %");
                return;
            }
            if (!float.TryParse(args[0], out float perc))
            {
                SendReply(player, "Not a valid number: " + perc);
                return;
            }
            perc = Mathf.Clamp(perc, 0, 100);

            var newCond = Mathf.Clamp(heldItem.maxCondition * perc / 100, 0, heldItem.maxCondition);
            heldItem.condition = newCond;
            heldItem.MarkDirty();
            SendReply(player, "Set " + heldItem.info.displayName.english + " condition to " + perc + "% cond (cond value: " + heldItem.condition.ToString("N0") + ")");
        }

        [ChatCommand("fsleep")]
        private void cmdForceSleep(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var target = GetLookAtEntity(player, 75f, PLAYER_LAYER) as BasePlayer;
            if (target == null || (target?.IsDead() ?? true)) return;
            if (!(target?.IsSleeping() ?? true)) target.StartSleeping();
            if (args.Length > 0 && args[0].Equals("test", StringComparison.OrdinalIgnoreCase))
            {
                PrintWarning("disappear");
                target.Invoke(() =>
                {
                    PrintWarning("actual disappear");
                    DisappearEntity(target, player);
                }, 0.5f);
                target.Invoke(() =>
                {
                    PrintWarning("re-appear");
                    AppearEntity(target, player);
                }, 3f);
            }
        }

        [ChatCommand("fname")]
        private void cmdForceName(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var target = GetLookAtEntity(player, 75f, PLAYER_LAYER) as BasePlayer;
            if (target == null || (target?.IsDead() ?? true)) return;

            if (target.IsConnected)
            {
                SendReply(player, "Not for us on connected players!! should use /rename");
                return;
            }

            var oldName = target.displayName;
            target.displayName = string.Join(" ", args);

            SendReply(player, "Set " + oldName + " to name: " + target.displayName);

            if (args.Length > 0 && args[0].Equals("test", StringComparison.OrdinalIgnoreCase))
            {
                PrintWarning("disappear");
                target.Invoke(() =>
                {
                    PrintWarning("actual disappear");
                    DisappearEntity(target, player);
                }, 0.5f);
                target.Invoke(() =>
                {
                    PrintWarning("re-appear");
                    AppearEntity(target, player);
                }, 3f);
            }
        }

        [ChatCommand("killheld")]
        private void cmdKillHeld(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var heldItem = player?.GetActiveItem() ?? null;
            if (heldItem == null) return;
            var heldEnt = heldItem?.GetHeldEntity() ?? null;
            var worldEnt = heldItem?.GetWorldEntity() ?? null;
            PrintWarning("Held ent?: " + (heldEnt != null) + ", world: " + (worldEnt != null));
            RemoveFromWorld(heldItem);
            NextTick(() =>
            {
                if (heldItem == null)
                {
                    PrintWarning("helditem is null!");
                    return;
                }
                var worldNew = heldItem?.GetWorldEntity() ?? null;
                PrintWarning("world now?: " + (worldNew != null));
                var find = BaseEntity.saveList?.Where(p => worldNew)?.FirstOrDefault() ?? null;
                PrintWarning("Find?: " + (find != null));
            });
        }





        [ChatCommand("togglepickup")]
        private void cmdTogglePickup(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var ent = GetLookAtEntity(player);
            if (ent == null)
            {
                SendReply(player, "Failed to find an entity! Try looking at a door.");
                return;
            }
            var entDoor = ent?.GetComponent<Door>() ?? null;
            if (entDoor == null)
            {
                SendReply(player, "Found entity but it does not appear to be a door!");
                return;
            }
            entDoor.pickup.enabled = !entDoor.pickup.enabled;
            entDoor.SendNetworkUpdate();
            SendReply(player, entDoor.pickup.enabled ? "This door can now be picked up." : "This door can no longer be picked up.");
        }


        [ChatCommand("myid")]
        private void cmdMyInfo(BasePlayer player, string command, string[] args)
        {
            SendReply(player, player.displayName + " (" + (covalence.Players.FindPlayerById(player.UserIDString)?.Name ?? "Unknown") + ") ID: " + player.UserIDString);
            var findSB = new StringBuilder();
            var findPlayer = covalence.Players.All.Where(p => p.Name == player.displayName);
            var findPlayer2 = covalence.Players.All.Where(p => p.Name.IndexOf(player.displayName) >= 0);
            var findConnected2 = covalence.Players.Connected.Where(p => p.Name == player.displayName);
            var findConnected3 = covalence.Players.Connected.Where(p => p.Name.IndexOf(player.displayName) >= 0);

            var findPlayer3 = FindConnectedPlayer(player.displayName);
            //       var findPlayers = FindPlayers(player.displayName);
            var findConnected = FindConnectedPlayer(player.displayName);
            Puts("before conn");
            var allConnected = FindIPlayers(player.displayName, true);
            Puts("after conn");
            foreach (var p in findPlayer)
            {
                findSB.AppendLine(p.Name + ": " + p.Id);
            }
            Puts("first append");
            foreach (var p in findPlayer2) findSB.AppendLine(p.Name + " (PARTIAL FIND/IndexOf): " + p.Id);
            Puts("second append");
            //          foreach (var p in findPlayers) findSB.AppendLine(p.Name + " (findPlayers find): " + p.Id);
            foreach (var p in findConnected2) findSB.AppendLine(p.Name + " (findConnected2: " + p.Id);
            foreach (var p in findConnected3) findSB.AppendLine(p.Name + " (findConnected3/partial: " + p.Id);
            Puts("third");
            if (allConnected != null && allConnected.Any())
            {
                foreach (var p in allConnected) findSB.AppendLine(p.Name + " (connected only): " + p.Id);
            }
            else SendReply(player, "<color=orange>NULL/COUNT < 0!!!!</color>");
            Puts("fourth");
            if (findSB.Length > 0) SendReply(player, findSB.ToString().TrimEnd());
            else SendReply(player, "No players found with your name in covalence.players.all!");
            SendReply(player, "found player 3 is null?: " + (findPlayer3 == null));
            SendReply(player, "findConnected is null?: " + (findConnected == null));
            SendReply(player, "FindConnected2 count: " + findConnected2.Count() + ", 3 count: " + findConnected3.Count());
        }

        [ChatCommand("iha")]
        private void cmdInfAmmo(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (!ignoreAmmo.Contains(player.UserIDString))
            {
                SendReply(player, "Unlimited heli ammo");
                ignoreAmmo.Add(player.UserIDString);
            }
            else
            {
                SendReply(player, "removed unlimited heli ammo");
                ignoreAmmo.Remove(player.UserIDString);
            }
        }

        [ChatCommand("sapidebug")]
        private void cmdSAPI(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (LocalStatsAPI == null || !LocalStatsAPI.IsLoaded)
            {
                SendReply(player, "No Local Stats API!");
                return;
            }
            var dict = LocalStatsAPI?.Call<Dictionary<string, object>>("GetStatsDict", player.UserIDString) ?? null;
            if (dict == null || dict.Count < 1)
            {
                SendReply(player, "Got bad dict!");
                return;
            }
            foreach (var kvp in dict)
            {
                if (!(kvp.Value is Dictionary<string, long>)) SendReply(player, kvp.Key + " <-- key -- " + kvp.Value + " <-- val");
                else
                {
                    var newDict = kvp.Value as Dictionary<string, long>;
                    if (newDict == null || newDict.Count < 1) return;
                    foreach (var kvp2 in newDict) SendReply(player, kvp.Key + " <-- key, val: " + kvp.Value);
                }
            }
        }

        private Dictionary<string, object> GetLocalStats(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            var stats = LocalStatsAPI?.Call<Dictionary<string, object>>("GetStatsDict", userId) ?? null;
            if (stats != null && stats.Count > 0) return stats;
            return null;
        }

        private long GetStat(string key, Dictionary<string, object> stats)
        {
            if (string.IsNullOrEmpty(key) || stats == null) throw new ArgumentNullException();
            var stat = 0L;
            if (stats.TryGetValue(key, out object obj) && !long.TryParse(obj.ToString(), out stat)) stat = -1;
            return stat;
        }

        private void cmdInfo(IPlayer player, string command, string[] args)
        {
            if (!player.HasPermission("compilation.sapi") && !player.IsServer && !player.IsAdmin)
            {
                SendReply(player, "You do not have permission to use this command.");
                return;
            }
            if (LocalStatsAPI == null || !LocalStatsAPI.IsLoaded)
            {
                SendReply(player, "No Local Stats API! (A required plugin is not loaded)");
                return;
            }
            if (args == null || args.Length < 1)
            {
                SendReply(player, "You must supply a target!");
                return;
            }
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                SendReply(player, "Found 0 or more than 1 player with the name: " + args[0]);
                return;
            }
            var statsDict = GetLocalStats(target.Id);
            if (statsDict == null)
            {
                SendReply(player, "Failed to find any stats for: " + target.Name);
                return;
            }
            var statsSB = new StringBuilder();
            var kills = (long)statsDict["kills"];
            var pKills = (long)statsDict["pvpkills"];
            var deaths = (long)statsDict["deaths"];
            var pDeaths = (long)statsDict["pvpdeaths"];
            var pvpKD = pKills / (float)pDeaths;
            var genKD = (pKills + kills) / (float)(deaths + pDeaths);
            var pvpKDStr = pvpKD.ToString("0.00");
            var genKDStr = genKD.ToString("0.00");
            if (pvpKDStr == "Infinity") pvpKDStr = kills.ToString();
            if (genKDStr == "Infinity") genKDStr = kills.ToString();
            var shots = (long)statsDict["shots"];
            var projhits = (long)statsDict["projhits"];
            var meleehits = (long)statsDict["meleehits"];
            var bphits = statsDict["bodypartshit"] as Dictionary<string, long>;
            var pvpHits = bphits?.Values?.Sum() ?? 0;

            PrintWarning("pvpHits: " + pvpHits + ", proj hits: " + projhits);
            var accuracy = projhits / (float)shots;
            var pvpAccuracy = pvpHits / (float)shots;
            statsSB.AppendLine("Stats for: <color=orange>" + (target?.Name ?? "Unknown") + "</color>");
            if (deaths > 0) statsSB.AppendLine("Total deaths: " + deaths);
            if (pDeaths > 0) statsSB.AppendLine("PVP deaths: " + pDeaths);
            if (kills > 0)
            {
                statsSB.AppendLine("Players killed: " + pKills);
                statsSB.AppendLine("PVP K/D Ratio: " + pvpKDStr);
                statsSB.AppendLine("General K/D Ratio (all types of deaths and kills included): " + genKDStr);
            }


            if (shots > 0) statsSB.AppendLine("Shots Fired: " + shots.ToString("N0"));
            if (projhits > 0) statsSB.AppendLine("Shots hit: " + projhits.ToString("N0"));
            if (pvpHits > 0) statsSB.AppendLine("PVP shots hit: " + pvpHits.ToString("N0"));
            if (shots > 0 && projhits > 0) statsSB.AppendLine("Accuracy: " + (accuracy * 100).ToString("0.00") + "%");
            if (shots > 0 && pvpHits > 0) statsSB.AppendLine("PVP accuracy " + (pvpAccuracy * 100).ToString("0.00") + "%");
            if (bphits != null && bphits.Count > 0)
            {
                statsSB.AppendLine("Most commonly hit bodyparts:");
                var top5 = (from entry in bphits orderby entry.Value descending select entry).Take(5);
                foreach (var top in top5) statsSB.Append(top.Key + ", hit: " + top.Value + " times. ");
            }
            SendReply(player, statsSB.ToString().TrimEnd());
        }

        private object OnPlayerViolation(BasePlayer player, AntiHackType type, float amount)
        {
            if (player == null) return null;

            if (player?.GetParentEntity() != null) return true;
            if (type == AntiHackType.FlyHack)
            {
                var ray = new Ray(player.CenterPoint(), Vector3.down);

                var hits = UnityEngine.Physics.SphereCastAll(ray, 0.25f, 3f, 1073741824);
                if (hits.Length > 0)
                {
                    PrintWarning("flyhack for user standing on tree will be canceled: " + player + " @ " + player.transform.position);
                    return true;
                }
            }
            if (player.IsAdmin) return true;


            //  if (type == AntiHackType.FlyHack && player.IsAdmin && (player.IsFlying || player.UsedAdminCheat(1.5f))) return false;
            return null;
        }
        /*/
        [ConsoleCommand("playtime.top")]
        private void consoleTopPlaytime(ConsoleSystem.Arg arg)
        {
            var timeSB = new StringBuilder();
            var minsPlayed2 = new Dictionary<ulong, float>();
            foreach (var time in storedData.playerList)
            {
                var UID = time.Key;
                foreach (var time2 in time.Value)
                {
                    if (time2.Key != "MinutesPlayed") continue;
                    minsPlayed2[UID] = float.Parse(time2.Value);
                }
            }
            var avgPlayTime = minsPlayed2.Values.Average();
            var maxPlayTime = minsPlayed2.Values.Max();
            var maxPlayTimeName = GetDisplayNameFromID(minsPlayed2.Where(p => p.Value == maxPlayTime).FirstOrDefault().Key);
            var hours = TimeSpan.FromMinutes(maxPlayTime).TotalHours;
            arg.ReplyWith(timeSB.ToString());
            arg.ReplyWith("AVG PLAY TIME: " + avgPlayTime + " MAX PLAY TIME: " + maxPlayTime + " NAME: " + maxPlayTimeName + " HOURS: " + hours);
        }
         //FIX PLAYTIME
        [ConsoleCommand("playtime.topx")]
        private void consoleTopPlaytimeX(ConsoleSystem.Arg arg)
        {
            var timeSB = new StringBuilder();
            var minsPlayed2 = new Dictionary<ulong, float>();
            var takeAmt = 10;
            if (arg.Args != null && arg.Args.Length > 0 && !int.TryParse(arg.Args[0], out takeAmt)) takeAmt = 10;
            foreach (var time in storedData.playerList)
            {
                var UID = time.Key;
                foreach (var time2 in time.Value)
                {
                    if (time2.Key != "MinutesPlayed") continue;
                    minsPlayed2[UID] = float.Parse(time2.Value);
                }
            }
            if (minsPlayed2.Count < 1) return;
            var takeDic = (from entry in minsPlayed2 orderby entry.Value descending select entry).Take(Mathf.Clamp(takeAmt, 1, minsPlayed2.Count));
            foreach (var top in takeDic)
            {
                var hours = TimeSpan.FromMinutes(top.Value).TotalHours;
                timeSB.AppendLine(GetDisplayNameFromID(top.Key) + ": " + top.Value + " ( hours: " + hours.ToString("0.0") + ")");
            }
            arg.ReplyWith(timeSB.ToString().TrimEnd());
        }/*/

        [ConsoleCommand("legitstats.last30")]
        private void consoleLegitStatsLast(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection != null) return;
            var now = DateTime.Now;
            var last30 = Pool.Get<HashSet<string>>();
            try
            {
                var sb = Pool.Get<StringBuilder>();
                try
                {
                    sb.Clear();

                    var index = 0;
                    foreach (var p in covalence.Players.All)
                    {
                        if (p == null) continue;
                        var lastTime = GetLastConnectionS(p.Id);
                        if (lastTime <= DateTime.MinValue) continue;

                        var span = now - lastTime;
                        if (span > TimeSpan.Zero && span.TotalDays < 31)
                        {
                            index++;
                            last30.Add(p.Id);
                            sb.Append(index).Append(": ").Append(p.Name).Append("/").Append(p.Id).Append(": ").Append(span.TotalDays).Append(" days since last connection. raw span: ").Append(span).Append(", date time: ").Append(lastTime).Append(Environment.NewLine);
                        }
                    }

                    if (sb.Length > 1) sb.Length -= 1;

                    PrintWarning(sb.ToString().TrimEnd());

                    // var last30 = covalence.Players.All.Where(p => (now - GetLastConnectionS(p.Id)).TotalDays < 31);
                    // var totalCount = last30.Count;

                    var nonLegit = 0;
                    var totalCount = last30.Count;

                    SendReply(arg, "Total Count of unique players in last 30 days: " + totalCount.ToString("N0") + ", legit: " + (totalCount - nonLegit).ToString("N0") + ", non-legit: " + nonLegit.ToString("N0"));
                }
                finally { Pool.Free(ref sb); }
            }
            finally { Pool.Free(ref last30); }
            //var last30 = new HashSet<string>();



        }

        private HashSet<string> _cachedLegitPlayerIDs = new();

        // private int _cachedLegitPlayerCount = 0;
        //  private int _cachedNonLegitPlayerCount = 0;

        private DateTime _lastPlayerCountCacheUpdate = DateTime.MinValue;

        private int GetUniquePlayerCount(TimeSpan timeSpan, bool ignoreCache = false)
        {
            if (timeSpan <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeSpan));

            var now = DateTime.UtcNow;
            if (!ignoreCache && _lastPlayerCountCacheUpdate > DateTime.MinValue && (now - _lastPlayerCountCacheUpdate).TotalHours < 1)
            {
                return _cachedLegitPlayerIDs.Count;
            }


            foreach (var p in covalence.Players.All)
            {
                if (p == null) continue;

                var lastTime = GetLastConnectionS(p.Id);
                if (lastTime <= DateTime.MinValue) continue;

                var span = now - lastTime;
                if (span > TimeSpan.Zero && span <= timeSpan)
                    _cachedLegitPlayerIDs.Add(p.Id);

            }

            //  _cachedLegitPlayerCount = totalCount - nonLegit;
            //  _cachedNonLegitPlayerCount = nonLegit;
            _lastPlayerCountCacheUpdate = now;


            return _cachedLegitPlayerIDs.Count;
        }



        [ChatCommand("kd")]
        private void cmdKd(BasePlayer player, string command, string[] args)
        {
            if (LocalStatsAPI == null || !LocalStatsAPI.IsLoaded)
            {
                SendReply(player, "No Local Stats API! (A required plugin is not loaded)");
                return;
            }
            var statsDict = GetLocalStats(player.UserIDString);
            if (statsDict == null)
            {
                SendReply(player, "Failed to get stats!");
                return;
            }
            //   var kills = GetStat("kills", statsDict);
            var pKills = GetStat("pvpkills", statsDict);
            var deaths = GetStat("deaths", statsDict);
            var pDeaths = GetStat("pvpdeaths", statsDict);
            var genKD = pKills / (float)(deaths + pDeaths);
            var pvpKD2 = pKills / (float)pDeaths;
            var pvpKDStr = pvpKD2.ToString("0.00");
            var genKDStr = genKD.ToString("0.00");
            if (pvpKDStr == "Infinity") pvpKDStr = pKills.ToString();
            if (genKDStr == "Infinity") genKDStr = pKills.ToString();
            var shots = GetStat("shots", statsDict);
            //   PrintWarning("pKills: " + ((float)pKills) + ", deaths: " + ((float)deaths) + ", kdr: " + pvpKD2 + ", str: " + pvpKDStr);
            SendReply(player, "Total kills: " + pKills.ToString("N0") + "\nTotal deaths: " + deaths.ToString("N0") + " (" + pDeaths.ToString("N0") + " pvp deaths)\nPVP Kill/Death Ratio: " + pvpKDStr + "\nGeneral Kill/Death Ratio: " + genKDStr + "\nTotal shots fired: " + shots.ToString("N0"));

        }


        [ChatCommand("getname")]
        private void cmdNameFromID(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "Please supply a ID!");
                return;
            }
            if (!ulong.TryParse(args[0], out ulong ID))
            {
                SendReply(player, "Failed to parse: " + ID + " as a ulong!");
                return;
            }
            SendReply(player, ID + " is: " + GetDisplayNameFromID(ID));
        }

        private bool HasCupboardAccess(BuildingPrivlidge cupboard, BasePlayer player) { return cupboard?.IsAuthed(player) ?? false; }

        private bool HasCupboardAccess(BuildingPrivlidge cupboard, ulong userID)
        {
            return cupboard?.IsAuthed(userID) ?? false;
        }

        private bool HasCupboardAccess(BuildingPrivlidge cupboard, string userID)
        {
            if (!ulong.TryParse(userID, out ulong uID)) return false;
            else return HasCupboardAccess(cupboard, uID);
        }

        private bool TryTakeCupboardAccess(BuildingPrivlidge cupboard, BasePlayer player) { return TryTakeCupboardAccess(cupboard, player?.userID ?? 0); }

        private bool TryTakeCupboardAccess(BuildingPrivlidge cupboard, ulong userID)
        {
            if (cupboard == null || userID == 0) return false;

            var remCount = cupboard.authorizedPlayers.RemoveWhere(p => p.userid == userID);
            if (remCount > 0)
            {
                cupboard.SendNetworkUpdate();
                var player = BasePlayer.FindByID(userID);
                player?.SendNetworkUpdate();
            }

            return true;
        }

        private bool TryGiveCupboardAccess(BuildingPrivlidge cupboard, BasePlayer player) { return TryGiveCupboardAccess(cupboard, player?.userID ?? 0); }

        private bool TryGiveCupboardAccess(BuildingPrivlidge cupboard, ulong userID)
        {
            if (cupboard == null || userID == 0) return false;
            if (HasCupboardAccess(cupboard, userID)) return true;
            var player = BasePlayer.FindByID(userID) ?? BasePlayer.FindSleeping(userID) ?? null;
            var newAuth = new ProtoBuf.PlayerNameID
            {
                userid = userID,
                username = player?.displayName ?? string.Empty
            };
            cupboard.authorizedPlayers.Add(newAuth);
            cupboard.SendNetworkUpdate();
            if (player != null && player.IsConnected) player.SendNetworkUpdate();
            return true;
        }

        private bool TryGiveCupboardAccess(BuildingPrivlidge cupboard, string userID)
        {
            if (!ulong.TryParse(userID, out ulong uID)) return false;
            else return TryGiveCupboardAccess(cupboard, uID);
        }

        private bool TryGiveTurretAccess(AutoTurret turret, BasePlayer player)
        {
            if (turret == null || player == null) return false;
            var isAuthed = turret?.IsAuthed(player) ?? false;
            if (isAuthed) return true;
            var newAuth = new ProtoBuf.PlayerNameID
            {
                userid = player.userID,
                username = player.displayName
            };
            turret.authorizedPlayers.Add(newAuth);
            turret.SendNetworkUpdate();
            return true;
        }

        private bool TryTakeTurretAccess(AutoTurret turret, BasePlayer player)
        {
            if (turret == null || player == null) return false;
            turret.authorizedPlayers.RemoveWhere(p => p.userid == player.userID);
            turret.SendNetworkUpdate();
            return true;
        }

        private bool HasCodeLockAccess(CodeLock codeLock, BasePlayer player)
        {
            if (player == null || codeLock == null) return false;
            return codeLock?.whitelistPlayers?.Contains(player.userID) ?? false;
        }

        private bool TryGiveCodeLockAccess(CodeLock codeLock, BasePlayer player)
        {
            if (codeLock == null || player == null) return false;
            var listPlayers = codeLock?.whitelistPlayers ?? null;
            if (listPlayers == null) return false;
            if (!listPlayers.Contains(player.userID)) listPlayers.Add(player.userID);
            return true;
        }

        private bool TryTakeCodeLockAccess(CodeLock codeLock, BasePlayer player)
        {
            if (codeLock == null || player == null) return false;
            var listPlayers = codeLock?.whitelistPlayers ?? null;
            if (listPlayers == null) return false;
            if (listPlayers.Contains(player.userID)) listPlayers.Remove(player.userID);
            return true;
        }

        private bool HasCodeAccess(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || player == null) return false;
            var codeLock = entity?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (codeLock != null)
            {
                if (codeLock.whitelistPlayers.Contains(player.userID)) return true;
                else return false;
            }
            return true;
        }

        private bool TryGiveCodeAccess(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || player == null) return false;
            var codeLock = entity?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (codeLock != null && !codeLock.whitelistPlayers.Contains(player.userID)) codeLock.whitelistPlayers.Add(player.userID);
            return true;
        }

        private bool TryTakeCodeAccess(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || player == null) return false;
            var codeLock = entity?.GetSlot(BaseEntity.Slot.Lock) as CodeLock;
            if (codeLock != null && codeLock.whitelistPlayers.Contains(player.userID)) codeLock.whitelistPlayers.Remove(player.userID);
            return true;
        }


        [ChatCommand("cleancorpses")]
        private void cmdCorpseCleanup(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            foreach (var corpse in allPlayerCorpses)
            {
                var destroyed = corpse?.IsDestroyed ?? true;
                if (destroyed || corpse == null) continue;
                corpse?.Kill();
            }
            SendReply(player, "Cleared " + allPlayerCorpses.RemoveWhere(p => p?.IsDestroyed ?? true) + " corpses!");
        }

        private class ItemInfo
        {
            public Item item;
            public ItemContainer container;
            public int itemPos;
            public ItemInfo(Item newItem, int pos = -1, ItemContainer newContainer = null)
            {
                item = newItem;
                itemPos = pos;
                container = newContainer;
            }
            public ItemInfo() { }
        }

        private void RefreshPlayer(BasePlayer player)
        {
            if (player == null || (player?.IsDead() ?? true) || !player.IsConnected) return;
            Dictionary<ulong, List<ItemInfo>> refreshItems = new()
            {
                [player.userID] = new List<ItemInfo>()
            };
            var oldPos = player?.transform?.position ?? Vector3.zero;
            var oldRot = player?.transform?.rotation ?? Quaternion.identity;
            var allItems = player?.inventory?.AllItems()?.ToList() ?? null;
            if (allItems != null && allItems.Count > 0)
            {
                for (int i = 0; i < allItems.Count; i++)
                {
                    var item = allItems[i];
                    if (item == null) continue;
                    var itemInfo = new ItemInfo(item, item?.position ?? -1, item?.parent ?? null);
                    refreshItems[player.userID].Add(itemInfo);
                    var uid = item.uid;
                    var pos = item?.position ?? 0;
                    if (item?.parent != null) item.RemoveFromContainer();
                }
            }
            player.lastDamage = Rust.DamageType.Generic;
            player.lastAttacker = null;
            player.Hurt(999f, Rust.DamageType.Heat, player, false);
            var visList = new List<BaseEntity>();
            Vis.Entities(oldPos, 2f, visList);
            if (visList != null && visList.Count > 0)
            {
                for (int i = 0; i < visList.Count; i++)
                {
                    var corpse = visList[i];
                    if (corpse == null || (corpse?.IsDestroyed ?? true)) continue;
                    var playerCorpse = (corpse is PlayerCorpse) ? (corpse as PlayerCorpse) : null;
                    if ((playerCorpse?.playerSteamID ?? 0) == player.userID) corpse.Kill();
                }
            }
            InvokeHandler.Invoke(player, () =>
            {
                player?.RespawnAt(oldPos, oldRot);
            }, 0.01f);
            InvokeHandler.Invoke(player, () =>
            {
                if (player == null || !player.IsConnected) return;
                var newItems = new List<ItemInfo>();
                if (!refreshItems.TryGetValue(player.userID, out newItems) || (newItems?.Count ?? 0) < 1) return;
                var items = player?.inventory?.AllItems()?.ToList() ?? null;
                if (items != null && items.Count > 0)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        RemoveFromWorld(item);
                    }
                }
                for (int i = 0; i < newItems.Count; i++)
                {
                    var item = newItems[i];
                    if (item == null) continue;
                    var moveItem = item?.item ?? null;
                    if (moveItem == null) continue;
                    if (item?.container != null) if (!moveItem.MoveToContainer(item.container, item?.itemPos ?? -1)) moveItem.Drop(player.GetDropPosition(), player.GetDropPosition(), Quaternion.identity);
                }
            }, 1f);
        }

        [ChatCommand("refresh")]
        private void cmdRefreshPlayer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var isGod = Godmode?.Call<bool>("IsGod", player.UserIDString) ?? false;
            if (isGod)
            {
                SendReply(player, "Disable Godmode first!");
                return;
            }
            RefreshPlayer(player);
        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;

            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, currentRot);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, maxDist, coll))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }

            if (coll == -1)
            {
                if (UnityEngine.Physics.Raycast(ray, out hit, maxDist, _constructionColl))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }

            return null;
        }

        private HashSet<Collider> GetEntityColliders(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed) return null;
            var colliders = new HashSet<Collider>();
            try
            {
                var cols = entity?.transform?.GetComponentsInChildren<Collider>() ?? null;
                var cols2 = entity?.transform?.GetComponentsInParent<Collider>() ?? null;
                var cols3 = entity?.GetComponentsInChildren<Collider>() ?? null;
                var cols4 = entity?.GetComponentsInParent<Collider>() ?? null;
                //   var col = entity?.GetComponent<BaseCombatEntity>()?._collider ?? null;
                var col2 = entity?.transform?.GetComponentInChildrenIncludeDisabled<Collider>() ?? null;
                if (cols != null) for (int i = 0; i < cols.Length; i++) colliders.Add(cols[i]);
                if (cols2 != null) for (int i = 0; i < cols2.Length; i++) colliders.Add(cols2[i]);
                if (cols3 != null) for (int i = 0; i < cols3.Length; i++) colliders.Add(cols3[i]);
                if (cols4 != null) for (int i = 0; i < cols4.Length; i++) colliders.Add(cols4[i]);
                //  if (col != null && !colliders.Contains(col)) colliders.Add(col);
                if (col2 != null && !colliders.Contains(col2)) colliders.Add(col2);
                if (entity is BasePlayer)
                {
                    var ply = entity as BasePlayer;
                    if (ply == null) return colliders;
                    var pCol = ply?.GetComponentInChildren<Collider>() ?? null;
                    if (pCol != null) colliders.Add(pCol);

                    var pCol2 = (Collider)GetValue(ply, "triggerCollider");
                    if (pCol2 != null) colliders.Add(pCol2);

                    var rigid = (Rigidbody)GetValue(ply, "physicsRigidbody");
                    if (rigid != null)
                    {
                        var rigidCol = rigid?.GetComponentInChildren<Collider>() ?? null;
                        if (rigidCol != null) colliders.Add(rigidCol);
                    }
                    //   var pCol3 = ply?.collision?.transform?.GetComponentInChildrenIncludeDisabled<Collider>() ?? null;
                    // if (pCol3 != null) colliders.Add(pCol3);
                    var pCol4 = ply?.GetComponentsInChildren<Collider>() ?? null;
                    if (pCol4 != null && pCol4.Length > 0) for (int i = 0; i < pCol4.Length; i++) colliders.Add(pCol4[i]);
                    var pCol5 = ply?.GetComponentsInParent<Collider>() ?? null;
                    if (pCol5 != null && pCol5.Length > 0) for (int i = 0; i < pCol5.Length; i++) colliders.Add(pCol5[i]);
                    var pCol6 = ply?.transform?.GetComponentsInChildren<Collider>() ?? null;
                    if (pCol6 != null && pCol6.Length > 0) for (int i = 0; i < pCol6.Length; i++) colliders.Add(pCol6[i]);
                    var pCol7 = ply?.transform?.GetComponentsInParent<Collider>() ?? null;
                    if (pCol7 != null && pCol7.Length > 0) for (int i = 0; i < pCol7.Length; i++) colliders.Add(pCol7[i]);
                }
                // colliders.Distinct();
                //PrintWarning("colliders count: " + colliders.Count + ", ent: " + entity.ShortPrefabName);
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Exception on GetEntityColliders"); }


            return colliders;
        }

        private void DisableCollision(BaseEntity entity, BaseEntity target, bool enable = false)
        {
            if (entity == null || entity.IsDestroyed || target == null || target.IsDestroyed) return;
            try
            {
                var entCols = GetEntityColliders(entity);
                var targetCols = GetEntityColliders(target);
                if (entCols != null && entCols.Count > 0 && targetCols != null && targetCols.Count > 0) foreach (var entCol in entCols) foreach (var targetCol in targetCols) UnityEngine.Physics.IgnoreCollision(entCol, targetCol, !enable);
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Exception on DisableCollision"); }

        }

        [ChatCommand("compr")]
        private void cmdCompr(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var getLook = GetLookAtEntity(player, 100f, defaultColl);
            if (getLook == null || getLook.IsDestroyed || !(getLook is LootContainer))
            {
                SendReply(player, "getLook is null or not loot container, null: " + (getLook == null) + ", type: " + getLook?.GetType());
                PrintWarning("Not a proper loot container");
                return;
            }
            ReplaceRiflebody(getLook as LootContainer, SaveSpan);
            ReplaceHazmat(getLook as LootContainer);
        }

        // public static BasePlayer GetMounted(BaseMountable mount) { return mount?._mounted ?? null; }
        private readonly List<NetworkableId> removeOnDismount = new();

        private object OnEntityDismounted(BaseMountable mount, BasePlayer player)
        {
            if (mount == null || player == null) return null;
            if (mount?.net != null)
            {
                if (removeOnDismount.Contains(mount.net.ID))
                {
                    var mountPos = mount?.transform?.position ?? Vector3.zero;
                    var newPos = new Vector3(mountPos.x, mountPos.y + 1.45f, mountPos.z);
                    player.transform.position = newPos;
                    if (player.IsConnected && player?.net != null)
                    {
                        player.ClientRPCPlayer(null, player, "ForcePositionTo", newPos);
                        player.SendNetworkUpdateImmediate();
                    }
                    if (!mount.IsDestroyed) mount.Kill();
                }
            }
            return null;
        }

        [ChatCommand("ait")]
        private void cmdAit(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var getLook = GetLookAtEntity(player, 100f, 2048) as BaseNpc;
            if (getLook == null || getLook.IsDestroyed)
            {
                SendReply(player, "getLook is null or not ai, null: " + (getLook == null) + ", type: " + getLook?.GetType());
                return;
            }
            getLook.CancelInvoke(getLook.TickNavigation);
            getLook.CancelInvoke(getLook.TickAi);
            getLook.SendNetworkUpdate();
            BaseEntity chair = GameManager.server.CreateEntity("assets/prefabs/deployable/chair/chair.deployed.prefab", player.CenterPoint());
            chair.Spawn();
            if (chair?.net == null)
            {
                chair?.Kill();
                return;
            }
            var rigid = (Rigidbody)chair.gameObject.AddComponent(typeof(Rigidbody));
            rigid.isKinematic = true;
            chair.SetParent(getLook);
            chair.enableSaving = false;
            removeOnDismount.Add(chair.net.ID);

            // var colliders = GetEntityColliders(chair);
            //var parentColliders = GetEntityColliders(car);

            //    if (colliders != null && colliders.Count > 0 && parentColliders != null && parentColliders.Count > 0) for (int i = 0; i < colliders.Count; i++) for (int j = 0; j < parentColliders.Count; j++) UnityEngine.Physics.IgnoreCollision(colliders[i], parentColliders[j]);

            ignoreStability.Add(chair.net.ID);
            // if (entity?.net != null) carsWithChairs.Add(entity.net.ID);

            //      if (chairs == null || chairs.Count < 1) chairs = new List<NetworkableId>();

            //  chairs.Add(chair.net.ID);
            //   carChairs[car.net.ID] = chairs;

            // var chairPos = new Dictionary<int, Vector3>();
            // chairPos[0] = new Vector3(0.5f, 0.075f, 0.578f);
            //   chairPos[1] = new Vector3(0.5f, 0.075f, -0.35f);
            //  chairPos[2] = new Vector3(-0.5f, 0.075f, -0.35f);


            //    var chairEnts = BaseNetworkable.serverEntities?.Where(p => p?.transform != null && p?.net != null && chairs.Contains(p.net.ID))?.ToList() ?? null;
            var localPos = new Vector3(0, 0.15f, 0);
            //      var localPos = chairs.Count == 1 ? new Vector3(0.5f, 0.075f, 0.578f) : chairs.Count == 2 ? new Vector3(0.5f, 0.075f, -0.35f) : chairs.Count == 3 ? new Vector3(-0.5f, 0.075f, -0.35f) : Vector3.zero;
            PrintWarning("Local pos: " + localPos);
            chair.transform.localPosition = localPos;
            chair.transform.localEulerAngles = new Vector3(180, 0, 0);



            var lastUpdatePos = Vector3.zero;
            InvokeHandler.InvokeRepeating(chair, () =>
            {
                if (chair == null || chair.IsDestroyed || chair?.transform == null) return;
                var chPos = chair?.transform?.position ?? Vector3.zero;
                if (lastUpdatePos == Vector3.zero || Vector3.Distance(chPos, lastUpdatePos) >= 1.75)
                {
                    lastUpdatePos = chPos;
                    var bm = chair?.GetComponent<BaseMountable>() ?? null;
                    if (bm != null)
                    {
                        var mounted = GetMounted(bm);
                        if (mounted != null)
                        {
                            mounted.transform.position = bm.transform.position;
                            mounted.SendNetworkUpdate();
                        }

                        bm.SendNetworkUpdate();
                        bm.UpdateNetworkGroup();
                        bm.OnPositionalNetworkUpdate();
                        //bm.FixedUpdate();
                    }
                }
            }, 10f, 0.7f);
            player.EnsureDismounted();

            (chair as BaseMountable).MountPlayer(player);
            var smooth = getLook?.GetComponent<SmoothMovement>() ?? null;
            if (smooth == null) smooth = getLook.gameObject.AddComponent<SmoothMovement>();
            smooth.doRotation = true;
            SendReply(player, "Disabled AI for: " + getLook.ShortPrefabName + " (" + getLook.net.ID + ")");
        }

        [ChatCommand("aides")]
        private void cmdAiDest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var getLook = GetLookAtEntity(player, 100f, 2048) as BaseNpc;
            if (getLook == null || getLook.IsDestroyed)
            {
                SendReply(player, "getLook is null or not ai, null: " + (getLook == null) + ", type: " + getLook?.GetType());
                return;
            }
            getLook.UpdateDestination(new Vector3(711, 20, 585));
            getLook.SendNetworkUpdate();
            SendReply(player, "Disabled AI for: " + getLook.ShortPrefabName + " (" + getLook.net.ID + ")");
        }


        [ChatCommand("killlook")]
        private void cmdLookAtKill(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new(player.eyes.position, currentRot);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 10f))
            {
                var getEnt = hitt.GetEntity();
                if (getEnt != null && !getEnt.IsDestroyed) getEnt.Kill();
            }
        }

        public static Vector3 GetVector3FromString(string vectorStr)
        {
            var vector = Vector3.zero;
            if (string.IsNullOrEmpty(vectorStr)) return vector;
            var vecStr = vectorStr;
            if (vecStr.StartsWith("(") && vecStr.EndsWith(")")) vecStr = vecStr.Substring(1, vecStr.Length - 2);
            var split = vecStr.Split(',');
            return (split == null || split.Length < 3) ? vector : new Vector3(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]));
        }

        private readonly Dictionary<string, BaseEntity> rotEnts = new();
        [ChatCommand("rotmod")]
        private void cmdRotMod(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (!rotEnts.TryGetValue(player.UserIDString, out BaseEntity rotEnt))
            {
                rotEnt = GetLookAtEntity(player, 10f);
                if (rotEnt == null || rotEnt.IsDestroyed)
                {
                    SendReply(player, "No entity");
                    return;
                }
                rotEnts[player.UserIDString] = rotEnt;
                SendReply(player, "Rotating entity: " + rotEnt.ShortPrefabName);
            }
            else
            {
                rotEnts.Remove(player.UserIDString);
                SendReply(player, "Stopped rotating: " + rotEnt.ShortPrefabName);
            }
        }


        [ChatCommand("rotlook")]
        private void cmdRotLook(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new(player.eyes.position, currentRot);
            var customRotation = Vector3.zero;
            if (args.Length > 0)
            {
                try
                {
                    customRotation = GetVector3FromString(args[0]);
                    PrintWarning("Custom rotation now: " + customRotation);
                }
                catch (Exception ex)
                {
                    PrintError(ex.ToString());
                    customRotation = Vector3.zero;
                }
            }


            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 10f, _constructionColl))
            {
                var getEnt = hitt.GetEntity() as BuildingBlock;
                if (getEnt != null)
                {
                    var rotAmount = getEnt?.blockDefinition?.rotationAmount ?? Vector3.zero;
                    if (customRotation != Vector3.zero)
                    {
                        rotAmount = customRotation;
                        PrintWarning("rotAmount: " + rotAmount);
                    }
                    if (rotAmount == Vector3.zero)
                    {
                        var findRotationAmount = ((BaseEntity.saveList?.Where(p => (p is BuildingBlock) && p.ShortPrefabName == "wall")?.FirstOrDefault() ?? null) as BuildingBlock)?.blockDefinition?.rotationAmount ?? Vector3.zero;
                        if (findRotationAmount != null)
                        {
                            PrintWarning("Found rotation amount: " + findRotationAmount + ", old: " + rotAmount);
                            rotAmount = findRotationAmount;
                        }

                    }

                    getEnt.transform.localRotation *= Quaternion.Euler(rotAmount);
                    getEnt.RefreshEntityLinks();
                    getEnt.UpdateSurroundingEntities();
                    getEnt.UpdateSkin(true);
                    getEnt.SendNetworkUpdateImmediate(false);
                    getEnt.ClientRPC(null, "RefreshSkin");
                    SendReply(player, "Rotated");
                }
                else SendReply(player, "Not building block!!");
            }
            else SendReply(player, "No ray");
        }

        [ChatCommand("setowner")]
        private void cmdSetOwner(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "You must specify a target to set as owner!");
                return;
            }
            var lookAt = GetLookAtEntity(player);
            if (lookAt == null)
            {
                SendReply(player, "You must be looking at an entity to use this command!");
                return;
            }
            if (!ulong.TryParse(args[0], out ulong targetID))
            {
                var target = FindConnectedPlayer(args[0], true);
                if (target == null)
                {
                    SendReply(player, "No player found with: " + args[0]);
                    return;
                }
                if (!ulong.TryParse(target.Id, out targetID))
                {
                    SendReply(player, "Couldn't parse player Id to ulong: " + target.Id + " (" + target.Name + ")");
                    return;
                }
            }
            if (args.Length > 1 && args[1].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                var block = lookAt as BuildingBlock;
                if (block != null)
                {
                    var blocks = GetBlocksFromID(block.buildingID);
                    if (blocks != null && blocks.Count > 1)
                    {
                        SendReply(player, "Will set owner of all blocks in this buildingId");
                        foreach (var b in blocks) b.OwnerID = targetID;
                    }


                }

            }
            SendReply(player, "Old owner ID: " + lookAt.OwnerID + ", now: " + (lookAt.OwnerID = targetID));
        }


        [ChatCommand("entnear")]
        private void cmdEntNear(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var ents = new List<BaseEntity>();
            var layer = -1;
            var radius = 5f;
            if (args.Length > 0 && !int.TryParse(args[0], out layer))
            {
                SendReply(player, "Not an int: " + args[0]);
                return;
            }
            if (args.Length > 1 && !float.TryParse(args[0], out radius))
            {
                SendReply(player, "Not a float: " + args[0]);
                return;
            }
            Vis.Entities(player.transform.position, radius, ents, layer);
            ents = (ents == null || ents.Count < 1) ? null : ents?.Where(p => p != player)?.Distinct()?.ToList() ?? null;
            if (ents == null || ents.Count < 1)
            {
                SendReply(player, "No near ents");
                return;
            }
            var entSB = new StringBuilder();
            for (int i = 0; i < ents.Count; i++)
            {
                var ent = ents[i];
                if (ent == null || ent?.gameObject == null || ent?.transform == null) continue;
                var layerStr = ent.gameObject.layer + ", " + LayerMask.LayerToName(ent.gameObject.layer) + ", mask: " + LayerMask.GetMask(LayerMask.LayerToName(ent.gameObject.layer));
                entSB.AppendLine(ent.PrefabName + " (" + ent.ShortPrefabName + "): " + ent.transform.position + ", Skin: " + ent.skinID + Environment.NewLine + "Type: " + ent.GetType() + Environment.NewLine + "Layer: " + layerStr + Environment.NewLine + "<color=orange>-----</color>");
            }
            var entStr = entSB.ToString().TrimEnd();
            SendReply(player, entStr);
            player.SendConsoleCommand("echo " + entStr);
        }

        private readonly HashSet<string> kr = new();
        [ChatCommand("kr")]
        private void cmdKR(BasePlayer player, string command, string[] args)
        {
            if ((!player.IsAdmin || player.displayName != "Shady") && !kr.Contains(player.UserIDString)) return;
            if (kr.Contains(player.UserIDString)) kr.Remove(player.UserIDString);
            else kr.Add(player.UserIDString);
            SendReply(player, "KR: " + kr.Contains(player.UserIDString));
        }

        private readonly Dictionary<ulong, BaseEntity> lastAttached = new();
        private readonly Dictionary<BaseEntity, Vector3> lastEntPos = new();
        private readonly Dictionary<BaseEntity, BaseEntity> lastEntParent = new();

        [ChatCommand("attachme")]
        private void cmdAttach(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;



            Ray ray = new(player.eyes.position, currentRot);
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 10f))
            {
                if (hitt.GetEntity() == null) return;
                var type = hitt.GetEntity()?.GetType()?.ToString() ?? hitt.GetCollider()?.GetType()?.ToString() ?? "Unknown Type";
                var prefabName = hitt.GetEntity()?.ShortPrefabName ?? hitt.GetCollider()?.GetComponent<MeshCollider>()?.sharedMesh?.name ?? hitt.GetCollider()?.name ?? "Unknown Prefab";
                var ent = hitt.GetEntity();

                lastAttached[player.userID] = ent;
                lastEntPos[ent] = ent?.transform?.position ?? Vector3.zero;
                lastEntParent[ent] = ent?.GetParentEntity() ?? null;
                ent.SetParent(player, 0);

                ent.transform.localPosition = new Vector3(0, 0f, 0);
                ent.transform.localRotation = Quaternion.identity;
                DisableCollision(ent, player);

                SendReply(player, "Attached: " + prefabName);
            }
        }


        [ChatCommand("cfp")]
        private void cmdCampfireParent(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var fire = GameManager.server.CreateEntity("assets/prefabs/deployable/campfire/campfire.prefab", player.CenterPoint());
            fire.Spawn();

            fire.SetParent(player, 0);
            fire.transform.localPosition = new Vector3(0, 1.85f, 0);
            fire.transform.localRotation = Quaternion.identity;
            DisableCollision(fire, player);
            fire.SendNetworkUpdate();
        }

        [ChatCommand("detachlast")]
        private void cmdDetach(BasePlayer player, string command, string[] args)
        {
            player.gameObject.SetLayerRecursive(17);
            if (!lastAttached.ContainsKey(player.userID))
            {
                SendReply(player, "No last attached!");
                return;
            }
            var ent = lastAttached[player.userID];
            if (ent.IsDestroyed || ent == null)
            {
                SendReply(player, "Ent is destroyed or null");
                return;
            }
            var pColl = player?.transform?.GetComponentInChildrenIncludeDisabled<Collider>() ?? player?.GetComponent<Collider>() ?? null;
            if (lastEntPos.ContainsKey(lastAttached[player.userID]))
            {
                if (lastEntParent.ContainsKey(ent)) ent.SetParent(lastEntParent[ent]);
                else ent.SetParent(null);
                ent.transform.position = lastEntPos[ent];
                ent.gameObject?.SetActive(true);
                var coll = ent?.transform?.GetComponentInChildrenIncludeDisabled<Collider>() ?? null;
                if (coll != null && pColl != null) UnityEngine.Physics.IgnoreCollision(coll, pColl, false);
            }
            else ent.Kill(BaseNetworkable.DestroyMode.None);

            var entColliders = GetEntityColliders(ent);
            var playerColliders = GetEntityColliders(player);
            if (entColliders != null && entColliders.Count > 0 && playerColliders != null && playerColliders.Count > 0) foreach (var entCollider in entColliders) foreach (var playerCollider in playerColliders) UnityEngine.Physics.IgnoreCollision(entCollider, playerCollider, false);

            lastEntPos.Remove(ent);
            lastAttached.Remove(player.userID);

        }



        private readonly Dictionary<int, Dictionary<int, int>> itemSkins = new();

        private bool HasSkins(Item item) { return (item?.info == null) ? false : HasSkins(item.info); }


        private void FillItemList()
        {
            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                if (HasSkins(item))
                {
                    itemSkins.Add(item.itemid, new Dictionary<int, int>());
                    FillSkinList(item);
                }
            }
        }

        private void FillSkinList(ItemDefinition item)
        {
            if (itemSkins.ContainsKey(item.itemid))
            {
                int i = 0;
                var skins = ItemSkinDirectory.ForItem(item).ToList();
                itemSkins[item.itemid].Add(i, 0);
                i++;
                foreach (var entry in skins) { itemSkins[item.itemid].Add(i, entry.id); i++; }
            }
        }

        private bool HasSkins(ItemDefinition item) { return SkinsAPI?.Call<bool>("HasSkins", item) ?? false; }

        private ulong GetRandomSkinID(string itemidStr)
        {
            if (string.IsNullOrEmpty(itemidStr)) return 0;
            return SkinsAPI?.Call<ulong>("GetRandomSkinID", itemidStr) ?? 0;
        }

        private ulong GetRandomSkinID(int itemID) { return GetRandomSkinID(itemID.ToString()); }

        private readonly HashSet<StorageContainer> storageContainers = new();

        private Item FindItemByIDU(ulong uID)
        {
            if (uID == 0) return null;
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
                        if (item?.uid.Value == uID) return item;
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
                                if (item?.uid.Value == uID) return item;
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
                        if (item?.uid.Value == uID) return item;
                    }
                }

                foreach (var lootCont in allContainers)
                {
                    if (lootCont == null || lootCont?.inventory?.itemList == null || lootCont.inventory.itemList.Count < 1) continue;
                    for (int i = 0; i < lootCont.inventory.itemList.Count; i++)
                    {
                        if (lootCont == null || lootCont?.inventory?.itemList == null || lootCont.inventory.itemList.Count < 1) break;
                        var item = lootCont.inventory.itemList[i];
                        if (item?.uid.Value == uID) return item;
                    }
                }

                foreach (var entry in BaseNetworkable.serverEntities)
                {
                    if (entry == null) continue;
                    var storage = entry as StorageContainer;
                    if ((storage?.inventory?.itemList?.Count ?? 0) > 0)
                    {
                        for (int i = 0; i < storage.inventory.itemList.Count; i++)
                        {
                            if (storage?.inventory?.itemList == null || storage.inventory.itemList.Count < 1) break;
                            var item = storage.inventory.itemList[i];
                            if (item?.uid.Value == uID) return item;
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
                                var item = container.itemList[j];
                                if (item?.uid.Value == uID) return item;
                            }
                        }
                    }
                }

                return null;
            }
            finally { Pool.FreeList(ref allItems); }

        }

        private Item FindItemByID(ItemId uID)
        {
            return FindItemByIDU(uID.Value);
        }

        private Item FindItemByID(string ID)
        {
            if (ulong.TryParse(ID, out ulong uID)) return FindItemByIDU(uID);
            else return null;
        }

        private ItemDefinition FindItemByNameOrID(string engOrShortNameOrId)
        {
            if (string.IsNullOrEmpty(engOrShortNameOrId)) throw new ArgumentNullException(nameof(engOrShortNameOrId));


            var matches = Pool.GetList<ItemDefinition>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item.itemid.ToString().Equals(engOrShortNameOrId)) return item;
                    if (item.displayName.english.Equals(engOrShortNameOrId, StringComparison.OrdinalIgnoreCase) || item.shortname.Equals(engOrShortNameOrId, StringComparison.OrdinalIgnoreCase)) return item;

                    var engName = item?.displayName?.english;

                    if (engName.IndexOf(engOrShortNameOrId, StringComparison.OrdinalIgnoreCase) >= 0 && !matches.Contains(item)) matches.Add(item);
                    if (item.shortname.IndexOf(engOrShortNameOrId, StringComparison.OrdinalIgnoreCase) >= 0 && !matches.Contains(item)) matches.Add(item);
                }

                return matches.Count != 1 ? null : matches[0];
            }
            finally { Pool.FreeList(ref matches); }
        }

        [ChatCommand("heldinfo")]
        private void cmdHeldInfo(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;



            var heldItem = player?.GetActiveItem() ?? null;
            if (heldItem == null)
            {
                if (args.Length < 1)
                {
                    SendReply(player, "args < 1 and helditem null. specify slot to check main inv");
                    return;
                }

                if (!int.TryParse(args[0], out int slotInt))
                {
                    SendReply(player, "Not a valid slot!!! This must be in your main container - not belt.");
                    return;
                }

                if (slotInt < 0)
                {
                    SendReply(player, "must be 0 or greater");
                    return;
                }

                var mainItemList = player.inventory.containerMain.itemList;

                if (mainItemList.Count - 1 < slotInt)
                {
                    SendReply(player, "count - 1 < " + slotInt);
                    return;
                }

                heldItem = mainItemList[slotInt];

                if (heldItem == null)
                {
                    SendReply(player, "no item found with: " + slotInt);
                    return;
                }

            }


            var name = heldItem?.info?.displayName?.english ?? "Unknown";
            var shortname = heldItem?.info?.shortname ?? "Unknown";
            var uID = heldItem.uid;
            var type = heldItem?.GetHeldEntity()?.GetType()?.ToString() ?? "Unknown";
            var altUID = player?.svActiveItemID;
            var rarity = heldItem?.info?.rarity ?? Rust.Rarity.None;
            var itemID = heldItem?.info?.itemid ?? 0;
            var worldEnt = heldItem?.GetWorldEntity() ?? null;
            var heldEnt = heldItem?.GetHeldEntity() ?? null;
            SendReply(player, "World ent: " + (worldEnt != null) + "Held ent:" + (heldEnt != null));
            timer.Once(2f, () =>
            {
                var newWorld = heldItem?.GetWorldEntity() ?? null;
                var newHeld = heldItem?.GetHeldEntity() ?? null;
                var getItem = newHeld?.GetItem() ?? newWorld?.GetItem() ?? null;





                SendReply(player, "Now ent: " + (newWorld != null) + ", Held: " + (heldEnt != null) + ", get item: " + (getItem != null) + ", world ID: " + (newWorld?.net?.ID.Value ?? 0) + ", " + (newHeld?.net?.ID.Value ?? 0));
            });

            var infoComps = heldItem?.info?.GetComponents<Component>();

            var compSb = new StringBuilder();

            if (infoComps != null && infoComps.Length > 0)
            {
                compSb.Append(infoComps.Length).Append(" comps").Append(":").Append(Environment.NewLine);
                for (int i = 0; i < infoComps.Length; i++)
                {
                    compSb.Append(infoComps[i].GetType().ToString()).Append(" ");
                }

                if (compSb.Length > 1) compSb.Length -= 1;
            }

            SendReply(player, name + " (" + shortname + "), uID: " + uID + ", type: " + type + " svActiveItemID: " + altUID + " Item ID: " + itemID + " Rarity: " + rarity + " Category: " + (heldItem?.info?.category ?? ItemCategory.All) + "\nPosition: " + heldItem.position + "\nText: " + heldItem.text + "\nSkin: " + heldItem.skin + "\n" + compSb.ToString());
        }


        private readonly Dictionary<string, TimeCachedValue<bool>> _isMvpCache = new();


        private bool IsMVP(string userID, bool skipCache = false)
        {
            if (string.IsNullOrEmpty(userID))
                throw new ArgumentNullException(nameof(userID));

            if (!_isMvpCache.TryGetValue(userID, out _))
                _isMvpCache[userID] = new TimeCachedValue<bool>()
                {
                    refreshCooldown = 60f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<bool>(() =>
                    {
                        var perms = permission.GetUserPermissions(userID);
                        if (perms.Length < 1)
                            return false;

                        for (int i = 0; i < perms.Length; i++)
                            if (perms[i].Contains("mvp", CompareOptions.OrdinalIgnoreCase)) return true;

                        return false;
                    })
                };

            return _isMvpCache[userID].Get(skipCache);
        }
        private readonly Dictionary<string, TimeCachedValue<bool>> _isVipCache = new();

        private bool IsVIP(string userID, bool skipCache = false)
        {
            if (string.IsNullOrEmpty(userID))
                throw new ArgumentNullException(nameof(userID));


            if (!_isVipCache.TryGetValue(userID, out _))
                _isVipCache[userID] = new TimeCachedValue<bool>()
                {
                    refreshCooldown = 60f,
                    refreshRandomRange = 5f,
                    updateValue = new Func<bool>(() =>
                    {
                        var perms = permission.GetUserPermissions(userID);
                        if (perms.Length < 1)
                            return false;

                        for (int i = 0; i < perms.Length; i++)
                            if (perms[i].Contains("vip", CompareOptions.OrdinalIgnoreCase)) return true;

                        return false;
                    })
                };


            return _isVipCache[userID].Get(skipCache);
        }

        private void OnItemResearch(ResearchTable table, Item targetItem, BasePlayer player)
        {
            if (table == null || player == null) return;

            table.researchDuration = IsVIP(player.UserIDString) ? 5f : IsMVP(player.UserIDString) ? 7.5f : 10f; //looters.Any(p => IsVIP(p.UserIDString)) ? 5f : looters.Any(p => IsMVP(p.UserIDString)) ? 7.5f : 10f;
        }

        private void OnItemRepair(BasePlayer player, Item item)
        {
            if (player == null || item == null) return;
            var bench = (item?.parent?.entityOwner ?? null) as RepairBench;
            if (bench == null) return;
            bench.maxConditionLostOnRepair = IsVIP(player.UserIDString) ? 0.075f : IsMVP(player.UserIDString) ? 0.111f : 0.2f;
        }

        [ConsoleCommand("set.admin")]
        private void consoleSetAdmin(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection != null && !arg.IsAdmin)
            {
                SendReply(arg, "connection != null && !isadmin");
                return;
            }
            if (arg?.Args == null || arg.Args.Length < 2)
            {
                SendReply(arg, "no or less than 2 args");
                return;
            }

            var arg0 = arg.Args[0];
            var p = FindConnectedPlayer(arg0)?.Object as BasePlayer;
            if (p == null || !p.IsConnected)
            {
                SendReply(arg, "player is null or not connected for: " + arg0);
                return;
            }
            var level = (uint)arg.GetInt(1);

            p.net.connection.authLevel = level;
            p.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, level > 0);
            p.SendNetworkUpdateImmediate();
            SendReply(arg, "set admin to: " + level);

        }

        [ConsoleCommand("tp.net")]
        private void consoleTPnet(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null || (player?.IsDead() ?? true) || player?.transform == null) return;
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1) return;
            if (!uint.TryParse(args[0], out uint netID))
            {
                SendReply(player, "not an ID: " + args[0]);
                return;
            }
            var pos = BaseEntity.saveList?.Where(p => p != null && (p?.net?.ID.Value ?? 0) == netID)?.FirstOrDefault()?.transform?.position ?? Vector3.zero;
            if (pos == Vector3.zero)
            {
                SendReply(player, "Bad position");
                return;
            }
            TeleportPlayer(player, pos);
        }

        [ConsoleCommand("ip.findall")]
        private void consoleIPFind(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null)
            {
                arg.ReplyWith("For security, this must be ran from the server/RCon!");
                return;
            }
            var args = arg?.Args ?? null;
            if (args == null || args.Length < 1)
            {
                arg.ReplyWith("You must supply a name/ID/IP!");
                return;
            }
            var player = FindConnectedPlayer(args[0], true);
            if (player == null)
            {
                arg.ReplyWith("No player found with the name: " + args[0]);
                return;
            }
            var ips = AAIPDatabase?.Call<List<string>>("GetAllIPs", player.Id) ?? null;
            var lastIP = AAIPDatabase?.Call<string>("GetLastIP", player.Id) ?? string.Empty;
            var ipSB = new StringBuilder();
            if (ips != null && ips.Count > 0) for (int i = 0; i < ips.Count; i++) ipSB.AppendLine(ips[i]);
            arg.ReplyWith(((ipSB.Length > 0) ? "Showing " + ips.Count + " IPs for " + (player?.Name ?? args[0]) + Environment.NewLine + ipSB.ToString().TrimEnd() : "No IPs found from: " + (player?.Name ?? args[0])) + Environment.NewLine + "Last IP: " + (!string.IsNullOrEmpty(lastIP) ? lastIP : "Unknown"));
        }

        [ConsoleCommand("bcnews")]
        private void consoleBCNews(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && !(arg?.Player()?.IsAdmin ?? false))
            {
                arg.ReplyWith("No perms!");
                return;
            }
            BroadcastNews();
            arg.ReplyWith("Broadcasted news");
        }

        [ConsoleCommand("killplayer")]
        private void consoleKillPlayer(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var args = arg?.Args ?? null;
            if (args == null) return;
            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                arg.ReplyWith("Failed to find any player with the name: " + args[0]);
                return;
            }
            if (target?.IsDead() ?? true)
            {
                arg.ReplyWith(target.displayName + " is already dead!");
                return;
            }
            target.Hurt(999);
            NextTick(() =>
            {
                if (target == null) return;
                if ((target?.IsAlive() ?? false) && !target.IsConnected) target.Kill(BaseNetworkable.DestroyMode.None);
            });
            arg.ReplyWith("Killed: " + target.displayName);
        }

        [ConsoleCommand("cleardropped")]
        private void consoleClearDropped(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;
            var saveDropped = ToHashSet(BaseEntity.saveList?.Where(p => p != null && !p.IsDestroyed && p is DroppedItem) ?? null);
            if (saveDropped == null || saveDropped.Count < 1)
            {
                arg.ReplyWith("No dropped items found!");
                return;
            }
            foreach (var droppedItem in saveDropped)
            {
                if (droppedItem == null || droppedItem.IsDestroyed) continue;
                droppedItem.Kill();
            }
            arg.ReplyWith("Cleared " + saveDropped.Count + " dropped items!");
        }

        [ConsoleCommand("killnpcs")]
        private void consoleKillNpcs(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var npcs = UnityEngine.Object.FindObjectsOfType<BaseNpc>()?.Where(p => p != null && !(p?.IsDestroyed ?? true))?.ToList() ?? null;
            if (npcs == null || npcs.Count < 1) return;
            for (int i = 0; i < npcs.Count; i++)
            {
                var npc = npcs[i];
                if (npc == null || (npc?.IsDestroyed ?? true)) continue;
                npc.Kill();
            }
        }

        private readonly Dictionary<ulong, float> lastMcPick = new();
        [ConsoleCommand("mc.pick")]
        private void consoleMCPick(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null)
            {
                arg.ReplyWith("This can only be ran as a player!");
                return;
            }
            if (player.IsDead() || !player.IsConnected || player.IsWounded() || IsLooting(player)) return;
            if (!permission.UserHasPermission(player.UserIDString, Name + ".mcpick"))
            {
                PrintWarning(player.displayName + " tried to use mcpick with no permission!");
                return;
            }
            var fullBelt = player?.inventory?.containerBelt?.IsFull() ?? true;
            var heldItem = player?.GetActiveItem() ?? null;
            if (fullBelt && heldItem == null) return;
            var currentRot = GetPlayerRotation(player);
            var timeDiff = 0f;
            if (lastMcPick.TryGetValue(player.userID, out float lastPickTime))
            {
                timeDiff = UnityEngine.Time.realtimeSinceStartup - lastPickTime;
                if (timeDiff <= 0.275f)
                {
                    PrintWarning("Time diff < 0.275: " + timeDiff);
                    if (heldItem != null) RemoveFromWorld(heldItem);
                }
            }
            lastMcPick[player.userID] = UnityEngine.Time.realtimeSinceStartup;

            Ray ray = new(player.eyes.position, currentRot);
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 10f, _constructionColl))
            {
                var type = hitt.GetEntity()?.GetType()?.ToString() ?? hitt.GetCollider()?.GetType()?.ToString() ?? "Unknown Type";
                var prefabName = hitt.GetEntity()?.ShortPrefabName ?? hitt.GetCollider()?.GetComponent<MeshCollider>()?.sharedMesh?.name ?? hitt.GetCollider()?.name ?? "Unknown Prefab";
                var longPrefab = hitt.GetEntity()?.PrefabName ?? "Unknown";
                Item itemToGive = null;
                var pos = hitt.transform?.position ?? Vector3.zero;
                var owner = GetDisplayNameFromID(hitt.GetEntity()?.OwnerID ?? 0);
                if (string.IsNullOrEmpty(owner)) owner = "Unknown/None";
                var itemDef = GetItemDefFromPrefabName(prefabName);
                if (longPrefab.Contains("building core")) itemDef = ItemManager.FindItemDefinition("building.planner");
                if (itemDef != null)
                {
                    itemToGive = ItemManager.CreateByItemID(itemDef.itemid);
                    timer.Once(0.5f, () =>
                    {
                        if (itemToGive != null && itemToGive?.parent == null) RemoveFromWorld(itemToGive);
                    });
                }
                if (itemToGive == null)
                {
                    SendReply(player, "Failed to get an item for prefab/collider: " + prefabName);
                }
                else
                {
                    if (heldItem != null && heldItem.info.shortname == itemToGive.info.shortname) return;
                    var heldItemSlot = -1;
                    if (heldItem != null && player.inventory.containerBelt.IsFull())
                    {
                        heldItemSlot = heldItem?.position ?? -1;
                        RemoveFromWorld(heldItem);
                    }
                    if (!itemToGive.MoveToContainer(player.inventory.containerBelt, heldItemSlot, false))
                    {
                        SendReply(player, "Failed to move item your belt!");
                        RemoveFromWorld(itemToGive);
                    }
                    else
                    {
                        heldItem = player?.GetActiveItem() ?? null;
                        if (heldItem != itemToGive)
                        {
                            PrintWarning("UPD ITEM");
                            player.svActiveItemID = new ItemId(0);

                            heldItem?.GetHeldEntity()?.GetComponent<HeldEntity>()?.SetHeld(false);

                            player.svActiveItemID = itemToGive.uid;

                            player.SendNetworkUpdate();
                            itemToGive.GetHeldEntity().GetComponent<HeldEntity>().SetHeld(true);

                            player.inventory.UpdatedVisibleHolsteredItems();
                            player.inventory.ServerUpdate(0.001f);
                        }
                    }
                }
            }
        }

        [ConsoleCommand("nexttick.test")]
        private void consoleNexttick(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var start = Stopwatch.StartNew();
            NextTick(() =>
            {
                start.Stop();
                var end = start.Elapsed.TotalMilliseconds;
                PrintWarning("Next Tick was: " + end + "ms difference");
            });
        }

        [ConsoleCommand("invoke.test")]
        private void consoleInvoke(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            if ((arg?.Args?.Length ?? 0) < 1) return;
            if (!float.TryParse(arg.Args[0], out float time)) return;
            var start = Stopwatch.StartNew();
            InvokeHandler.Invoke(ServerMgr.Instance, () =>
            {
                start.Stop();
                var endTime = start.Elapsed.TotalSeconds;
                PrintWarning("Invoke of " + time + " was: " + endTime + "sec difference");
            }, time);
        }

        [ConsoleCommand("invoke.repeat")]
        private void consoleInvokeRepeat(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            var invoke = InvokeRepeating(ServerMgr.Instance, () =>
            {
                PrintWarning("+");
            }, 0.01f, 0.33f, 5);
        }

        private readonly HashSet<Action> repeatingInvokes = new();

        private Action InvokeRepeating(Behaviour sender, Action action, float time, float repeat, int repeats = -1)
        {
            var repeatCount = 0;
            Action newAction = null;
            newAction = new Action(() =>
            {
                if (action == null || repeats > 0 && (repeatCount >= repeats))
                {
                    InvokeHandler.CancelInvoke(sender, newAction);
                    repeatingInvokes?.Remove(newAction);
                    return;
                }
                action?.Invoke();
                repeatCount++;
            });
            InvokeHandler.InvokeRepeating(sender, newAction, time, repeat);
            if (repeatingInvokes != null) repeatingInvokes.Add(newAction);
            else PrintWarning("repeatingInvokes isnull?!");
            return newAction;
        }

        private readonly Dictionary<string, Action> _frameAction = new();
        private void cmdFrameTest(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (!player.IsServer && args.Length > 0 && args[0].Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                var pObj = player?.Object as BasePlayer;
                if (pObj == null) return;

                if (_frameAction.TryGetValue(player.Id, out Action action))
                {
                    InvokeHandler.CancelInvoke(pObj, action);
                    _frameAction.Remove(player.Id);
                    SendReply(player, "Disabled action");
                    return;
                }

                var watch = new Stopwatch();
                var first = true;
                action = new Action(() =>
                {
                    if (player == null || !player.IsConnected)
                    {
                        InvokeHandler.CancelInvoke(pObj, action);
                        return;
                    }

                    ServerMgr.Instance.StartCoroutine(DoFrameTest(player));
                    if (!first) SendReply(player, "Invoke + start coroutine took: " + watch.Elapsed.TotalSeconds.ToString("0.00") + " seconds (should be 1)");
                    watch.Restart();
                    first = false;
                });

                _frameAction[player.Id] = action;
                InvokeHandler.InvokeRepeating(pObj, action, 1f, 1f);


            }
            else ServerMgr.Instance.StartCoroutine(DoFrameTest(player));
        }

        private void cmdGameTags(IPlayer player, string command, string[] args)
        {
            if (player.IsAdmin)
            {
                var bornTime = BornTime;
                var time = FromUnixTime(BornTime);
                SendReply(player, SteamServer.GameTags + Environment.NewLine + "Born time: " + time + " (" + bornTime + ")");
            }
        }

        public bool IsConnected2(ulong iSteamID)
        {
            var b1 = BasePlayer.FindByID(iSteamID) != null;
            var find = ConnectionAuth.m_AuthConnection?.Where(p => p.userid == iSteamID)?.ToList() ?? null;
            if (find != null && find.Count > 0)
            {
                PrintWarning("FOUND userid in connection auths!, count: " + find.Count);
                foreach (var f in find) PrintWarning(f + ", " + f.ownerid + ", " + f.authStatusSteam + ", " + f.active + ", " + f.connected + ", " + f.connectionTime);
            }
            var b2 = ConnectionAuth.m_AuthConnection.Any((Connection item) => item.userid == iSteamID);
            PrintWarning("b1: " + b1 + ", b2: " + b2);
            return b1 || (find != null && find.Count > 0);
        }

        private void cmdConnections(IPlayer player, string command, string[] args)
        {
            if (player.IsAdmin)
            {
                if (args.Length > 0)
                {
                    var arg0 = args[0];
                    if (!ulong.TryParse(arg0, out ulong userId))
                    {
                        SendReply(player, "not a ulong: " + arg0);
                        return;
                    }
                    SendReply(player, "Is connected: " + IsConnected2(userId) + ", for: " + arg0);
                    return;
                }
                var sb = new StringBuilder();
                for (int i = 0; i < Net.sv.connections.Count; i++)
                {
                    var connection = Net.sv.connections[i];
                    if (connection == null) continue;
                    sb.AppendLine(connection.userid + ", " + connection.ownerid + ", " + connection.ipaddress + ", " + connection.active + ", " + connection.connected + ", " + connection.authStatusSteam);
                }
                SendReply(player, sb.ToString().TrimEnd());
                sb.Clear();
                for (int i = 0; i < ConnectionAuth.m_AuthConnection.Count; i++)
                {
                    var connection = ConnectionAuth.m_AuthConnection[i];
                    if (connection == null) continue;
                    sb.AppendLine(connection.userid + ", " + connection.ownerid + ", " + connection.ipaddress + ", " + connection.active + ", " + connection.connected + ", " + connection.authStatusSteam);
                }
                SendReply(player, "ConnectionAuth connections: " + Environment.NewLine + sb.ToString().TrimEnd());
            }
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
        private static readonly DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private void cmdIdlePlayers(IPlayer player, string command, string[] args)
        {
            var idleSort = BasePlayer.activePlayerList?.OrderByDescending(p => p?.IdleTime ?? 0f) ?? null;
            if (idleSort == null)
            {
                SendReply(player, "No players");
                return;
            }
            var idleSB = new StringBuilder();
            foreach (var ply in idleSort)
            {
                if (ply.IdleTime > 0.0) idleSB.AppendLine(ply.displayName + ": " + ply.IdleTime.ToString("0.00").Replace(".00", string.Empty));
            }
            SendReply(player, idleSB.ToString().TrimEnd());
        }

        private double LastFrameTime = -1;

        private System.Collections.IEnumerator DoFrameTest(IPlayer player = null)
        {
            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();
                yield return CoroutineEx.waitForEndOfFrame;
                watch.Stop();
                if (player != null && player.IsConnected)
                {
                    var sb = Pool.Get<StringBuilder>();
                    try
                    {
                        player.Reply(sb.Clear().Append("Frame took: ").Append(watch.Elapsed.TotalMilliseconds).Append("ms").ToString());
                    }
                    finally { Pool.Free(ref sb); }
                }
            }
            finally
            {
                Pool.Free(ref watch);
            }
        }

        private System.Collections.IEnumerator SetFrameTime()
        {
            var watch = Stopwatch.StartNew();
            yield return CoroutineEx.waitForEndOfFrame;
            watch.Stop();
            LastFrameTime = watch.Elapsed.TotalMilliseconds;
        }

        [ChatCommand("sphere")]
        private void cmdSphereHere(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var pos = player?.CenterPoint() ?? Vector3.zero;
            if (pos == Vector3.zero) return;
            var invalidMsg = "Invalid syntax! try: /sphere startingSize (1) sizeOverTime (1) sizeOverTimeSpeed (1)";
            if (args.Length <= 2)
            {
                SendReply(player, invalidMsg);
                return;
            }

            if (!float.TryParse(args[0], out float currentRadius) || !float.TryParse(args[1], out float lerpRadius) || !float.TryParse(args[2], out float lerpSpeed))
            {
                SendReply(player, invalidMsg);
                return;
            }

            var sphere = (SphereEntity)GameManager.server.CreateEntity("assets/prefabs/visualization/sphere.prefab", pos, player.transform.rotation, true);
            if (sphere == null) return;
            sphere.currentRadius = currentRadius;
            sphere.lerpRadius = lerpRadius;
            sphere.lerpSpeed = lerpSpeed;

            SendReply(player, "Layer info: " + sphere.gameObject.layer + ", " + LayerMask.LayerToName(sphere.gameObject.layer) + ", mask: " + LayerMask.GetMask(LayerMask.LayerToName(sphere.gameObject.layer)));

            sphere.Spawn();
            PrintWarning("has global broadcast?: " + sphere.globalBroadcast);
            SendReply(player, "Created sphere at: " + getVectorString(pos) + ", radius: " + sphere.currentRadius + ", lerp radius: " + sphere.lerpRadius + ", lerp speed: " + sphere.lerpSpeed);
        }


        [ChatCommand("killsphere")]
        private void cmdKillSphere(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var spheres = UnityEngine.Object.FindObjectsOfType<SphereEntity>();
            var count = 0;
            foreach (var sphere in spheres)
            {
                count++;
                sphere.Kill(BaseNetworkable.DestroyMode.None);
            }
            SendReply(player, "Destroyed " + count + " spheres!");
        }

        [ChatCommand("clearinv")]
        private void cmdClearInv(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var items = Pool.GetList<Item>();
            try
            {
                player.inventory.AllItemsNoAlloc(ref items);

                for (int i = 0; i < items.Count; i++) RemoveFromWorld(items[i]);
            }
            finally { Pool.FreeList(ref items); }

            player.SetPlayerFlag(BasePlayer.PlayerFlags.DisplaySash, false);
            player.SendNetworkUpdate();
        }


        private void cmdEffectDump(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var noAsset = false;
            if (args.Length > 0) bool.TryParse(args[0], out noAsset); // && !bool.TryParse(args[0], out noAsset)) noAsset = false;

            SendReply(player, "Dumping effects to console... noAsset: " + noAsset);
            ServerMgr.Instance.StartCoroutine(DumpAssets(noAsset));
        }

        private System.Collections.IEnumerator DumpAssets(bool noAsset = false)
        {
            var oldSvFPS = FPS.limit;


            try
            {
                var dumpSB = new StringBuilder();
                var percSB = new StringBuilder();

                var strs = GameManifest.Current.pooledStrings;

                var count = 0;
                var countBeforeReset = 128;

                if (strs.Length < countBeforeReset)
                    countBeforeReset = strs.Length;

                PrintWarning("Coroutine for DumpAssets called. pooledStrings has length: " + strs.Length);

                var lastShownPerc = 0f;

                var length = strs.Length;

                FPS.limit = 501;

                for (int i = 0; i < length; i++)
                {
                    var str = strs[i].str ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(str) || noAsset && str.EndsWith(".asset"))
                        continue;



                    count++;
                    dumpSB.Append(str).Append(Environment.NewLine);

                    var percent = (i / (float)length * 100);
                    if (lastShownPerc <= 0 || percent - lastShownPerc >= 1)
                    {
                        lastShownPerc = percent;

                        Puts(percSB.Clear().Append("Dump is now ").Append(percent.ToString("N0")).Append("% complete.").ToString());
                    }




                    if (count == countBeforeReset)
                    {
                        count = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }



                }

                if (dumpSB.Length > 1)
                    dumpSB.Length--;

                Puts(dumpSB.ToString().TrimEnd());
            }
            finally
            {
                if (FPS.limit == 501)
                    FPS.limit = oldSvFPS;
            }


        }

        private void SetTagOverride(string userID, string permission, string color)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(permission)) return;
            var findTagInfo = tagData?.tagList?.Where(p => p.UserIDString == userID && string.Equals(permission, p.permission, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault() ?? null;
            if (findTagInfo == null)
            {
                findTagInfo = new TagInfo
                {
                    UserIDString = userID
                };
            }
            findTagInfo.permission = permission;
            findTagInfo.colorOverride = color;
            if (!tagData.tagList.Contains(findTagInfo)) tagData.tagList.Add(findTagInfo);
        }

        private void SetTagHide(string userID, string permission, bool hide)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(permission)) return;
            var findTagInfo = tagData?.tagList?.Where(p => p.UserIDString == userID && string.Equals(permission, p.permission, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault() ?? null;
            if (findTagInfo == null)
            {
                findTagInfo = new TagInfo
                {
                    UserIDString = userID
                };
            }
            findTagInfo.permission = permission;
            findTagInfo.hide = hide;
            if (!tagData.tagList.Contains(findTagInfo)) tagData.tagList.Add(findTagInfo);
        }

        private string GetBestGroup(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return null;
            var perms = permission?.GetUserPermissions(userID) ?? null;
            if (perms == null || perms.Length < 1) return null;
            for (int i = 0; i < perms.Length; i++)
            {
                var perm = perms[i];
                if (perm.StartsWith("kngscredits.", StringComparison.OrdinalIgnoreCase) && !perm.Contains("mvp") && !perm.Contains("vip"))
                {
                    return perm;
                }
            }
            for (int i = 0; i < perms.Length; i++)
            {
                var perm = perms[i];
                if (perm.StartsWith("kngscredits.", StringComparison.OrdinalIgnoreCase))
                {
                    return perm;
                }
            }
            return string.Empty;
        }

        private readonly string[] allowedColorsNoSpace = { "#75ff47", "#459b20", "#8800cc", "#3385ff", "#e6e600", "#669900", "#4d4dff", "#cc0000", "#33ffbb", "#ff00ff", "#725142", "#595959", "#00ffff", "#ffb3cc", "#5c8a8a", "#e6004c", "#db890f", "#ce7235", "#e64d00", "#2c528c", "#42f4e8", "#494f4e", "#b5c40f", "#681144", "#562c2c", "#356656", "#57776d", "#255143", "#1e7258", "#3c3051", "#512f8e", "#2c1d7c", "#DD0000", "#8877ad", "#736b84", "#ff72ad", "#b70b53", "#eaa8d3", "#770750", "#3144a0", "#55a4c1", "#4332fc", "#1d5b77", "#1281C6", "#3E3E3E", "#661B77", "#FACD78", "#E4DCA4", "#4F4533", "#ffe6e6", "#89dc93", "#2B5116", "#BE2227", "#c5a787", "#FF0030", "#FE950A", "#FEF506", "#53D559", "#2BACE2", "#9E88D2", "#58767D", "#525354", "#95A26D", "#552E84", "#939296", "#9B5B5D", "#cd2a2a", "#8D1028", "#C40C23", "#D95A19", "#ECA013", "#6B3421", "#733053", "#FBFF93", "#F9D0DD", "#ffe14a", "#ffabe9", "#ffe0f3", "#c7fcff", "#45f5ff", "#ccf4ff", "#fcffcc", "#f2ff26", "#ff8e52", "#fdebff", "#f494ff", "#c3a1ff", "#7b30ff", "#ff91e7", "#ff00c7", "#ffbff1", "#ff82e4", "#cfff82", "#b5ffd6", "#a0ff52", "#ffdd94", "#ff8fa6", "#2f1cff", "#1c20ff", "#7376ff", "#999bff", "#1d20cc", "#b09419", "#12628a", "#198791", "#37b38e", "#a32a5d", "#992c90", "#2e992c", "#55ab33", "#8aba3d", "#cee64c", "#faaf64", "#ffa587", "#ff9587", "#ff7070", "#8fdbff", "#532696", "#96267e", "#177578", "#006366", "#00521d", "#474000", "#402800", "#421300", "#6e2102", "#8f0c00", "#1b00b3", "#69111e", "#380000", "#000000" };

        private readonly string[] allowedColors = { "", "#75ff47", "#459b20", "#8800cc", "#3385ff", "#e6e600", "#669900", "#4d4dff", "#cc0000", "#33ffbb", "#ff00ff", "#725142", "#595959", "#00ffff", "#ffb3cc", "#5c8a8a", "#e6004c", "#db890f", "#ce7235", "#e64d00", "#2c528c", "#42f4e8", "#494f4e", "#b5c40f", "#681144", "#562c2c", "#356656", "#57776d", "#255143", "#1e7258", "#3c3051", "#512f8e", "#2c1d7c", "#DD0000", "#8877ad", "#736b84", "#ff72ad", "#b70b53", "#eaa8d3", "#770750", "#3144a0", "#55a4c1", "#4332fc", "#1d5b77", "#1281C6", "#3E3E3E", "#661B77", "#FACD78", "#E4DCA4", "#4F4533", "#ffe6e6", "#89dc93", "#2B5116", "#BE2227", "#c5a787", "#FF0030", "#FE950A", "#FEF506", "#53D559", "#2BACE2", "#9E88D2", "#58767D", "#525354", "#95A26D", "#552E84", "#939296", "#9B5B5D", "#cd2a2a", "#8D1028", "#C40C23", "#D95A19", "#ECA013", "#6B3421", "#733053", "#FBFF93", "#F9D0DD", "#ffe14a", "#ffabe9", "#ffe0f3", "#c7fcff", "#45f5ff", "#ccf4ff", "#fcffcc", "#f2ff26", "#ff8e52", "#fdebff", "#f494ff", "#c3a1ff", "#7b30ff", "#ff91e7", "#ff00c7", "#ffbff1", "#ff82e4", "#cfff82", "#b5ffd6", "#a0ff52", "#ffdd94", "#ff8fa6", "#2f1cff", "#1c20ff", "#7376ff", "#999bff", "#1d20cc", "#b09419", "#12628a", "#198791", "#37b38e", "#a32a5d", "#992c90", "#2e992c", "#55ab33", "#8aba3d", "#cee64c", "#faaf64", "#ffa587", "#ff9587", "#ff7070", "#8fdbff", "#532696", "#96267e", "#177578", "#006366", "#00521d", "#474000", "#402800", "#421300", "#6e2102", "#8f0c00", "#1b00b3", "#69111e", "#380000", "#000000" };
        private readonly string[] basicAllowedColors = { "", "#e6004c", "#ffe14a", "#8800cc", "#3385ff", "#ffabe9", "#ff8e52", "#ffe0f3", "#8fdbff", "#69111e" };

        private bool IsColorTooDark(string hex, double threshold = 0.25)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out var color))
                throw new ArgumentException(nameof(hex) + " was not parsable");

            //Calculate luminance using YIQ formula
            return 0.299 * color.r + 0.587 * color.g + 0.114 * color.b < threshold;
        }

        private bool IsValidHex(string hex) => string.IsNullOrWhiteSpace(hex) ? throw new ArgumentNullException(nameof(hex)) : _hexRegex.IsMatch(hex);

        private void cmdTag(IPlayer player, string command, string[] args)
        {
            if (player == null || player.IsServer)
            {
                if (player != null) SendReply(player, "This can only be ran as a player!");
                return;
            }

            var arg0 = args.Length > 0 ? args[0] : string.Empty;

            var firstRank = GetBestGroup(player.Id);
            var isMVPorVIP = firstRank.Contains("vip") || firstRank.Contains("mvp");
            if (string.IsNullOrEmpty(firstRank))
            {
                SendReply(player, "Failed to find rank, rank is invalid, or rank is conflicting/unsupported.\nIf you're sure you own a supported rank, make sure it's selected in title. Example: <color=orange>/title color</color> and *then* try running this command. :)");
                return;
                // 
            }

            //this needs SB/cleanup.

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(arg0) && IsValidHex(arg0))
            {
      

                if (IsColorTooDark(arg0, 0.195f))
                {
                    SendReply(player, "We're so sorry, " + player.Name + ". Unfortunately, <color=" + arg0 + ">this</color> color is too dark.\nPeople may struggle to read your name and get upset. We don't want that, so we hope you understand.");
                    return;
                }

                SendReply(player, "Hi, <color=" + arg0 + ">" + player.Name + "</color>! You're certainly looking snazzy in your new color. Go show it off!");

                SetTagOverride(player.Id, firstRank, arg0);
                return;
            }

            if (isMVPorVIP)
            {
                if (args == null || args.Length < 1) SendReply(player, "You do not own a supported title, but you do have MVP or VIP! You'll have access to a very limited number of colors.");
            }
            var useColors = isMVPorVIP ? basicAllowedColors : allowedColors;
            if (useColors == null || useColors.Length < 1)
            {
                SendReply(player, "No colors found, you may want to report this to an admin/moderator!");
                return;
            }

            var plyName = player?.Name ?? string.Empty;
            PrintWarning("Found rank/perm: " + firstRank + ", for: " + player.Name);
            if (args.Length < 1)
            {
                var colorSB = new StringBuilder();
                var colorSBList = Pool.GetList<StringBuilder>();
                try
                {
                    for (int i = 0; i < useColors.Length; i++)
                    {
                        if (colorSB.Length >= 1024)
                        {
                            colorSBList.Add(colorSB);
                            colorSB = new StringBuilder();
                        }
                        var color = useColors[i];
                        var emptyColor = string.IsNullOrEmpty(color);
                        var strAppend = (!emptyColor ? ("<color=" + color + ">") : "") + i + " (" + plyName + ")" + (!emptyColor ? "</color>" : string.Empty);
                        colorSB.AppendLine(strAppend);
                        // colorSB.AppendLine("<color=" + allowedColors[i] + ">" + i + " (The quick brown fox jumps over the lazy dog)</color>");
                    }
                    if (colorSBList.Count > 0) for (int i = 0; i < colorSBList.Count; i++) SendReply(player, colorSBList[i].ToString().TrimEnd());
                    if (colorSB.Length > 0) SendReply(player, colorSB.ToString().TrimEnd());
                    var rngIndex = UnityEngine.Random.Range(0, useColors.Length);
                    if (rngIndex == 0) rngIndex = UnityEngine.Random.Range(0, useColors.Length);
                    var rngColorExample = useColors[rngIndex];
                    SendReply(player, "Please supply an index color to use, e.g <color=#8aff47>/" + command + " <color=" + rngColorExample + ">" + rngIndex.ToString("N0") + "</color>.</color>\n^ List of indexes shown above ^");

                    return;
                }
                finally { Pool.FreeList(ref colorSBList); }
            }
            if (!int.TryParse(args[0], out int index))
            {
                SendReply(player, "Not a number: " + args[0] + ", try <color=orange>/" + command + "</color> NUMBER\nTo view all colors you can select, type the command without arguments: /" + command);
                return;
            }
            if (index < 0 || index > (useColors.Length - 1))
            {
                SendReply(player, "Invalid index: " + index + ", too high or too low!\nTo view all colors you can select, type the command without arguments: /" + command);
                return;
            }
            SetTagOverride(player.Id, firstRank, useColors[index]);
            SendReply(player, "<color=" + useColors[index] + ">Set color successfully!</color>");
        }

        private uint GetAuthLevel(BasePlayer player) { return player?.net?.connection?.authLevel ?? 0; }

        [ChatCommand("tpall")]
        private void cmdTpall(BasePlayer player, string command, string[] args)
        {
            if (GetAuthLevel(player) < 2 || !player.IsAdmin) return;
            if (args.Length <= 0) return;
            var pos = player?.CenterPoint() ?? Vector3.zero;
            if (pos == Vector3.zero) return;
            if (args[0].ToLower() == "sleepers")
            {
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var sleeper = BasePlayer.sleepingPlayerList[i];
                    if (sleeper.IsConnected) continue;
                    TeleportPlayer(sleeper, pos, false, true, false);
                }
            }
            else if (args[0].ToLower() == "online")
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var online = BasePlayer.activePlayerList[i];
                    TeleportPlayer(online, pos, false, true, false);
                }
            }
        }

        [ChatCommand("ddraw")]
        private void cmdDdraw(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            for (int i = 0; i < 50; i++)
            {
                player.SendConsoleCommand("ddraw.line", 5f, Color.yellow, player.transform.position, default(Vector3));
                player.SendConsoleCommand("ddraw.sphere", 7f, Color.yellow, player.transform.position, 10f);
                player.SendConsoleCommand("ddraw.text", 5f, Color.green, player.CenterPoint() + new Vector3(0, .5f), $"text");
            }
        }

        [ChatCommand("darrow")]
        private void cmdDdrawArrow(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var adjPos = player.transform.position;
            adjPos.y -= 10;
            player.SendConsoleCommand("ddraw.arrow", 7f, Color.yellow, player.transform.position, adjPos, 5);
        }

        private string getVectorString(Vector3 vector) { return new StringBuilder(vector.ToString()).Replace("(", "").Replace(")", "").ToString(); }

        private string GetShortVectorString(Vector3 vector, bool roundBeforeInteger = true)
        {
            var sb = Pool.Get<StringBuilder>();
            try
            {
                var newVec = new Vector3((float)(roundBeforeInteger ? Math.Round(vector.x, MidpointRounding.AwayFromZero) : (int)vector.x), (float)(roundBeforeInteger ? Math.Round(vector.y, MidpointRounding.AwayFromZero) : (int)vector.y), (float)(roundBeforeInteger ? Math.Round(vector.z, MidpointRounding.AwayFromZero) : (int)vector.z));
                return sb.Clear().Append((int)newVec.x).Append(" ").Append((int)newVec.y).Append(" ").Append((int)newVec.z).ToString();
            }
            finally { Pool.Free(ref sb); }
        }


        [ChatCommand("tp")]
        private void cmdTPPlayer(BasePlayer player, string command, string[] args)
        {
            if (!canExecute(player, "tp"))
            {
                SendReply(player, "You don't have permission to use this command! Did you mean to use <color=orange>/tpr</color>?");
                PrintWarning("No perms tp for: " + player.displayName);
                return;
            }
            if (args.Length <= 0 || args.Length >= 3)
            {
                SendReply(player, "Invalid argument specified, syntax example: /tp <playername> OR /tp <playername> <toThisPlayer>");
                return;
            }
            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "Player " + "\"" + args[0] + "\"" + " could not be found!");
                return;
            }
            if (target.IsDestroyed || target.IsDead())
            {
                SendReply(player, "Player " + "\"" + target?.displayName + "\"" + " is dead!");
                return;
            }

            var targPos = target?.transform?.position ?? Vector3.zero;
            if (args.Length < 2)
            {
                if (player.IsAdmin && !isVanished(player))
                {
                    player.SendConsoleCommand("vanish");
                    SendReply(player, "You were automatically vanished");
                }
                TeleportPlayer(player, targPos, true, true, true);
                SendReply(player, "Teleported to: " + "\"" + target.displayName + "\"");
            }
            else
            {
                var target2 = FindPlayerByPartialName(args[1], true);
                if (target2 == null)
                {
                    SendReply(player, "Player " + "\"" + args[1] + "\"" + " could not be found!");
                    return;
                }
                if (target.IsDestroyed || target2.IsDead())
                {
                    SendReply(player, "Player " + "\"" + target?.displayName + "\"" + " is dead!");

                    return;
                }
                var targ2Pos = target2?.transform?.position ?? Vector3.zero;
                TeleportPlayer(target, targ2Pos, true, true, true);
                SendReply(player, "Teleported: " + "\"" + target.displayName + "\"" + " to: " + "\"" + target2.displayName + "\"" + ".");
            }
        }

        [ChatCommand("tpback")]
        private void cmdTPBack(BasePlayer player, string command, string[] args)
        {
            if (!canExecute(player, "tp"))
            {
                SendReply(player, "You don't have permission to use this command! Did you mean to use <color=orange>/tpr</color>?");
                PrintWarning("No perms tp for: " + player.displayName);
                return;
            }
            if (!preTpPosition.TryGetValue(player.UserIDString, out Vector3 pos))
            {
                SendReply(player, "No old position found!");
                return;
            }
            TeleportPlayer(player, pos);
        }

        private Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var coroutine = ServerMgr.Instance.StartCoroutine(routine);

            _coroutines.Add(coroutine);

            return coroutine;
        }

        private void StopCoroutine(Coroutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            ServerMgr.Instance.StopCoroutine(routine);

            _coroutines.Remove(routine);
        }

        private BasePlayer FindPlayerByID(ulong playerid) { return BasePlayer.FindByID(playerid) ?? BasePlayer.FindSleeping(playerid) ?? null; }

        private BasePlayer FindPlayerByID(string UserIDString)
        {
            if (string.IsNullOrEmpty(UserIDString)) return null;
            if (BasePlayer.activePlayerList != null && BasePlayer.activePlayerList.Count > 0)
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p != null && p.UserIDString == UserIDString) return p;
                }
            }
            if (BasePlayer.sleepingPlayerList != null && BasePlayer.sleepingPlayerList.Count > 0)
            {
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var p = BasePlayer.sleepingPlayerList[i];
                    if (p != null && p.UserIDString == UserIDString) return p;
                }
            }
            return null;
        }

        private void CancelDamage(HitInfo hitinfo)
        {
            if (hitinfo == null) return;
            hitinfo.damageTypes.Clear();
            hitinfo.PointStart = Vector3.zero;
            hitinfo.HitEntity = null;
            hitinfo.DoHitEffects = false;
            hitinfo.HitMaterial = 0;
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

        private void OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return;

            var hitEnt = info?.HitEntity ?? null;
            var entity = hitEnt;
            var type = hitEnt?.GetType()?.ToString() ?? string.Empty;
            var name = hitEnt?.ShortPrefabName ?? "Unknown";
            var targPos = info?.HitPositionWorld ?? hitEnt?.CenterPoint() ?? hitEnt?.transform?.position ?? Vector3.zero;
            var attacker = player;
            var hitPos = info?.HitPositionWorld ?? info?.PointEnd ?? info?.HitEntity?.CenterPoint() ?? Vector3.zero;
            var isProj = info?.IsProjectile() ?? false;
            var projID = info?.ProjectileID ?? 0;
            var victim = info?.HitEntity as BasePlayer;

            var wepEnt = info?.Weapon ?? null;
            var wepPrefab = info?.WeaponPrefab ?? null;
            var wepPrefabStr = wepPrefab?.ShortPrefabName ?? string.Empty;
            var repeatDelay = (wepEnt as BaseMelee)?.repeatDelay ?? (wepPrefab as BaseMelee)?.repeatDelay ?? 0f;
            if (!(info?.IsProjectile() ?? false) && player?.metabolism != null && !(wepEnt?.ShortPrefabName ?? string.Empty).Contains("chainsaw") && !(wepEnt?.ShortPrefabName ?? string.Empty).Contains("jackhammer"))
            {
                var scalar = Mathf.Clamp(1f + repeatDelay + UnityEngine.Random.Range(0.075f, 0.14f), 1f, 5f);
                var hydrationLoss = 0.115f * scalar;
                var calorieLoss = UnityEngine.Random.Range(0.131f, 0.167f) * scalar;
                if (player.metabolism.hydration.value > 0.0) player.metabolism.hydration.value -= hydrationLoss;
                if (player.metabolism.calories.value > 0.0) player.metabolism.calories.value -= calorieLoss;
            }
            var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            var isEasy = IsEasyServer();

            var weaponNiceName = wepEnt?.GetItem()?.info?.displayName?.english ?? wepPrefab?.GetItem()?.info?.displayName?.english ?? player?.GetActiveItem()?.info?.displayName?.english ?? string.Empty;

            if (victim != null)
            {
                if (!weaponsUsed.TryGetValue(attacker.userID, out Dictionary<ulong, List<string>> attackerDic))
                {
                    attackerDic = weaponsUsed[attacker.userID] = new Dictionary<ulong, List<string>>();

                }

                if (!attackerDic.TryGetValue(victim.userID, out List<string> victimList)) weaponsUsed[attacker.userID][victim.userID] = new List<string>();

                weaponsUsed[attacker.userID][victim.userID].Add(weaponNiceName);

            }

            if (entity != null)
            {
                //     if (player.IsAdmin) PrintWarning("admin hit: " + hitEnt?.ShortPrefabName + ", majDmg: " + majDmg + ", isEasy: " + isEasy + ", estimated dmg: " + (info?.damageTypes?.Total() ?? -1f));
                if (entity.prefabID == 931526157 || entity.prefabID == 2483166070) info?.damageTypes?.ScaleAll(10f); //door barricades at monuments

                if (entity.ShortPrefabName.Contains("scientist") && (!(entity as ScientistNPC)?.IsMounted() ?? false))
                {
                    var adjustedPlayerPos = player.transform.position;

                    adjustedPlayerPos.y = entity.transform.position.y;

                    var playerDistFromScientist = Vector3.Distance(hitPos, adjustedPlayerPos);

                    var distFromLarge = Vector3.Distance(LargeOilRig.transform.position, entity.transform.position);
                    var distFromSmall = Vector3.Distance(SmallOilRig.transform.position, entity.transform.position);

                    var isOilScientist = distFromLarge < 150 || distFromSmall < 150;

                    if (playerDistFromScientist > 14)
                    {

                        var dmgDistScalar = 1 - (playerDistFromScientist * (isOilScientist ? 0.00175f : 0.00133f));

                        if (dmgDistScalar < 0.9)
                            SendCooldownMessage(player, "These advanced scientists seem to have armored suit-lining that protects against long range shots. I should try getting closer.", 15f, "SCI_HIT_DISTANCE_REMINDER");


                        info?.damageTypes?.ScaleAll(dmgDistScalar);
                    }

                    if (wepEnt != null && ((wepEnt as BowWeapon) != null || (wepEnt as BaseProjectile) == null || (wepEnt?.GetItem()?.info?.shortname ?? string.Empty).Contains("nail")))
                    {
                        info?.damageTypes?.ScaleAll(UnityEngine.Random.Range(0.15f, 0.625f));
                    }
                }

            }


            var prefabname = hitEnt?.ShortPrefabName?.ToLower() ?? string.Empty;

            var weaponName = hitEnt?.GetItem()?.info?.displayName?.english ?? "Unknown";



            if (expAmmoUsers.Contains(player.UserIDString) && (wepEnt != null || wepPrefab != null))
            {
                var getMag = (wepEnt?.GetComponent<BaseProjectile>() ?? wepPrefab?.GetComponent<BaseProjectile>() ?? null)?.primaryMagazine ?? null;
                if (getMag != null)
                {
                    Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/explosion.prefab", hitPos, hitPos);
                    var dmgList = Pool.GetList<Rust.DamageTypeEntry>();
                    var newDmg = new Rust.DamageTypeEntry()
                    {
                        type = Rust.DamageType.Explosion,
                        amount = UnityEngine.Random.Range(3f, 5f)
                    };
                    dmgList.Add(newDmg);
                    DamageUtil.RadiusDamage(player, wepPrefab, hitPos, 0.75f, 0.75f, dmgList, flamerColl, true);
                    Pool.FreeList(ref dmgList);
                }
            }

            if (kr.Contains(player.UserIDString) && hitEnt != null && !(hitEnt?.IsDestroyed ?? true))
            {
                var combat = hitEnt?.GetComponent<BaseCombatEntity>() ?? null;
                var hitPlayer = hitEnt?.GetComponent<BasePlayer>() ?? null;
                if (hitPlayer != null && combat == null) hitPlayer.Hurt(999f, Rust.DamageType.ElectricShock, null, false);

                if (combat != null)
                {
                    combat.Hurt(combat.MaxHealth() * 2, Rust.DamageType.ElectricShock, player, false);
                    NextTick(() => { if (combat != null && !combat.IsDestroyed) combat.Kill(); });
                }
                else if (!(hitEnt?.IsDestroyed ?? true))
                {
                    hitEnt.Invoke(() =>
                    {
                        if (!hitEnt.IsDestroyed) hitEnt.Kill();
                    }, 0.01f);
                }
                for (int i = 0; i < 5; i++) Effect.server.Run("assets/prefabs/locks/keypad/effects/lock.code.shock.prefab", targPos, targPos, null, false);

                SendReply(player, "Killed: " + name);
            }

        }


        private readonly List<ulong> targetRockets = new();

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (projectile == null || player == null || mod == null || projectiles == null) return;

            var projectileList = projectiles?.projectiles ?? null;
            if (projectileList == null || projectileList.Count < 1) return; //may occur during antihack?

            if (glEnabled.Contains(player.userID))
            {
                var grenade = (TimedExplosive)GameManager.server.CreateEntity("assets/prefabs/weapons/f1 grenade/grenade.f1.deployed.prefab", player.eyes.position, GetPlayerRotationQ(player));
                if (grenade == null) return;
                grenade.OwnerID = player.userID;
                grenade.creatorEntity = player;
                grenade.SetVelocity(player.GetDropVelocity() * 16f);
                grenade.minExplosionRadius = 2f;
                grenade.explosionRadius = 2.5f;
                grenade.SetFuse(1.33f);
                grenade.Spawn();
            }
            if (rlEnabled.Contains(player.userID))
            {
                var newItems = ItemManager.itemList.Where(p => p.rarity == Rust.Rarity.Common).ToList();
                newItems.Shuffle((uint)UnityEngine.Random.Range(0, int.MaxValue));
                var newRock = ItemManager.Create(newItems.GetRandom());
                var velocity = GetPlayerRotation(player) * UnityEngine.Random.Range(14, 35);
                var drop = newRock.Drop(player.eyes.position, velocity, Quaternion.identity);
                drop.Invoke(() => drop.Kill(), 15f);

            }

            var ammo = projectile?.primaryMagazine?.ammoType ?? null;
            if (ammo?.itemid != -1321651331)
                return; //explosive ammo




            Item findSilencer = null;

            var item = projectile?.GetItem() ?? null;



            if (item?.contents?.itemList != null)
            {
                for (int i = 0; i < item.contents.itemList.Count; i++)
                {
                    var attachment = item.contents.itemList[i];
                    if (attachment?.info?.itemid == -1850571427) // silencer //shortname == "weapon.mod.silencer")
                    {
                        findSilencer = attachment;
                        break;
                    }
                }
            }

            item?.LoseCondition(UnityEngine.Random.Range(0.125f, 0.37f));
            findSilencer?.LoseCondition(UnityEngine.Random.Range(0.175f, 0.5f));

        }

        [ChatCommand("gufx")]
        private void cmdEffectGuitar(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "Unknown command: gufx");
                return;
            }
            if (player?.transform == null) return;
            var scale = 1f;
            var fx = "assets/prefabs/instruments/guitar/effects/guitarstrum.prefab";
            if (args.Length > 0 && !float.TryParse(args[0], out scale)) scale = 1f;
            if (args.Length > 1 && args[1].ToLower() == "pluck") fx = "assets/prefabs/instruments/guitar/effects/guitarpluck.prefab";
            var repeat = 0;
            if (args.Length > 2)
            {
                if (!int.TryParse(args[2], out repeat)) repeat = 0;
            }
            if (repeat > 0)
            {
                for (int i = 0; i < repeat; i++)
                {
                    using var effect = new Effect(fx, player, 0U, Vector3.zero, Vector3.zero);
                    effect.scale = scale;
                    EffectNetwork.Send(effect);
                }
            }
            else
            {
                using var effect = new Effect(fx, player, 0U, Vector3.zero, Vector3.zero);
                effect.scale = scale;
                EffectNetwork.Send(effect);
            }

            SendReply(player, "sent effect");
        }

        [ChatCommand("gufxr")]
        private void cmdEffectGuitarRepeat(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, "Unknown command: gufxr");
                return;
            }
            if (player?.transform == null) return;
            var scale = 1f;
            if (args.Length > 0 && !float.TryParse(args[0], out scale)) scale = 1f;
            if (args.Length > 1 && !int.TryParse(args[1], out int repeat)) repeat = 0;

            using (var effect = new Effect("assets/prefabs/instruments/guitar/effects/guitarstrum.prefab", player, 0U, Vector3.zero, Vector3.forward))
            {
                effect.scale = scale;
                EffectNetwork.Send(effect);
            };

            //       if (repeat > 0) InvokeRepeating(player, () => EffectNetwork.Send(effect), 0.01f, 0.01f, repeat);
            SendReply(player, "sent effect");
        }

        [ChatCommand("bed")]
        private void cmdBedPlayer(BasePlayer player, string command, string[] args) => cmdBagPlayer(player, command, args);

        [ChatCommand("fx")]
        private void cmdEffectPlayer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length <= 1 || args.Length > 3)
            {
                SendReply(player, "Invalid argument specified, syntax example: /effect <playername> <resourcepath> OR /effect all <resourcepath>");
                return;
            }
            var fxpath = args[1];
            if (args[0].ToLower() == "all")
            {
                foreach (BasePlayer playerr in BasePlayer.activePlayerList)
                {
                    if (!playerr.IsConnected) continue;
                    Effect.server.Run(fxpath, playerr.transform.position, playerr.transform.position, null, true);
                }
                return;
            }
            var repeat = 0;
            if (args.Length > 2) int.TryParse(args[2], out repeat);
            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "Player " + "\"" + args[0] + "\"" + " could not be found.");
                return;
            }
            if (repeat > 0)
            {
                for (int i = 0; i < repeat; i++)
                {
                    if (target == null || target?.transform == null) break;
                    Effect.server.Run(fxpath, target.transform.position, target.transform.position, null, true);
                }
            }
            else Effect.server.Run(fxpath, target.transform.position, target.transform.position, null, true);
        }

        [ChatCommand("fxe")]
        private void cmdFxeBoot(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length <= 1 || args.Length > 3)
            {
                SendReply(player, "Invalid argument specified, syntax example: /fxe <playername> <resourcepath>");
                return;
            }
            var fxpath = args[1];


            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "Player " + "\"" + args[0] + "\"" + " could not be found.");
                return;
            }

            var effectEntity = new Effect(fxpath, target, 0, Vector3.zero, Vector3.zero);

            if (effectEntity.gameObject == null)
            {
                PrintWarning("effect has no gameobject!!!");
                effectEntity.gameObject = new GameObject();
            }

            PrintWarning("try add effect recyc");
            effectEntity.gameObject.AddComponent<EffectRecycle>();

            EffectNetwork.Send(effectEntity, target.Connection);

            SendReply(player, "Attempted to run: " + fxpath);

        }

        [ChatCommand("fxs")]
        private void cmdFXS(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (player?.transform == null) return;
            if (args.Length < 2) return;
            if (!float.TryParse(args[1], out float scale))
            {
                SendReply(player, "Not valid scale: " + args[1]);
                return;
            }
            var fx = args[0];
            using (var effect = new Effect(fx, player, 0U, Vector3.zero, Vector3.zero))
            {
                effect.scale = scale;
                EffectNetwork.Send(effect);
            }
            SendReply(player, "sent effect: " + fx);
        }

        [ChatCommand("fxme")]
        private void cmdFXMe(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (player?.transform == null || player?.net?.connection == null) return;
            if (args.Length < 1) return;
            var scale = 1f;
            if (args.Length > 1)
            {
                if (!float.TryParse(args[1], out scale))
                {
                    SendReply(player, "Not valid scale: " + args[1]);
                    return;
                }
            }
            var fx = args[0];
            Effect effect = new(fx, player, 0U, Vector3.zero, Vector3.forward);
            if (scale != 1f) effect.scale = scale;
            EffectNetwork.Send(effect, player?.net?.connection);
            SendReply(player, "sent effect: " + fx);
        }

        private readonly Dictionary<string, Effect> playerEffect = new();
        [ChatCommand("fx2")]
        private void cmdFX2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (player?.transform == null || player?.net?.connection == null) return;
            if (playerEffect.TryGetValue(player.UserIDString, out Effect old))
            {
                old.ResetToPool();
                old.Clear();
                SendReply(player, "Cleared old effect (or tried to)");
            }
            if (args.Length < 1) return;
            var scale = 1f;
            if (args.Length > 1)
            {
                if (!float.TryParse(args[1], out scale))
                {
                    SendReply(player, "Not valid scale: " + args[1]);
                    return;
                }
            }
            var fx = args[0];
            Effect effect = new(fx, player, 0U, Vector3.zero, Vector3.forward);
            if (scale != 1f) effect.scale = scale;
            EffectNetwork.Send(effect);
            playerEffect[player.UserIDString] = effect;
            SendReply(player, "sent effect: " + fx);
        }

        [ChatCommand("cupboard")]
        private void cmdGiveCupboard(BasePlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                SendReply(player, "<color=orange>-</color>You can authorize a player to your Tool Cupboard (must be owned by you) by typing <color=#ff912b>/cupboard PlayerName</color>.\n<color=orange>-</color>You cannot deauthorize a player using this command!");
                return;
            }
            var targetID = FindPlayerByPartialName(args[0], true)?.UserIDString ?? FindConnectedPlayer(args[0], true)?.Id ?? string.Empty;
            var targetName = GetDisplayNameFromID(targetID);
            if (string.IsNullOrEmpty(targetID))
            {
                SendReply(player, "Could not find a player by the name: \"" + args[0] + "\".");
                return;
            }
            if (targetID == player.UserIDString)
            {
                SendReply(player, "You cannot authorize yourself!");
                return;
            }
            var curCupboard = GetLookAtEntity(player, 5f) as BuildingPrivlidge;
            if (curCupboard?.IsDestroyed ?? true)
            {
                SendReply(player, "You must be looking at a Tool Cupboard to use this!");
                return;
            }
            if (curCupboard.OwnerID != player.userID)
            {
                SendReply(player, "You do not own this Tool Cupboard!");
                return;
            }
            if (!HasCupboardAccess(curCupboard, player))
            {
                SendReply(player, "You must be authorized to this Tool Cupboard to give it to another player!");
                return;
            }
            if (HasCupboardAccess(curCupboard, targetID))
            {
                SendReply(player, "This player is already authorized!");
                return;
            }
            if (TryGiveCupboardAccess(curCupboard, targetID)) SendReply(player, targetName + " now has access to this Tool Cupboard.");
            else SendReply(player, "Failed to grant authorization to: " + targetName);
        }

        [ChatCommand("turret")]
        private void cmdGiveTurret(BasePlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                SendReply(player, "<color=orange>-</color>You can authorize a player to your Auto Turret (must be owned by you) by typing <color=#ff912b>/turret PlayerName</color>.\n<color=orange>-</color>You cannot deauthorize a player using this command!");
                return;
            }
            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "Could not find a player by the name: \"" + args[0] + "\". They may be disconnected and dead.");
                return;
            }
            if (target.userID == player.userID)
            {
                SendReply(player, "You cannot authorize yourself!");
                return;
            }
            var curTurret = GetLookAtEntity(player, 5f) as AutoTurret;
            if (curTurret?.IsDestroyed ?? true)
            {
                SendReply(player, "You must be looking at an Auto Turret to use this!");
                return;
            }
            if (curTurret.OwnerID != player.userID)
            {
                SendReply(player, "You do not own this Auto Turret!");
                return;
            }
            if (!curTurret.IsAuthed(player))
            {
                SendReply(player, "You must be authorized to this Auto Turret to grant access to another player!");
                return;
            }
            if (curTurret.IsAuthed(target))
            {
                SendReply(player, "This player is already authorized!");
                return;
            }
            if (TryGiveTurretAccess(curTurret, target)) SendReply(player, target.displayName + " now has access to this Auto Turret.");
            else SendReply(player, "Failed to grant authorization to: " + target.displayName);
        }

        [ChatCommand("bag")]
        private void cmdBagPlayer(BasePlayer player, string command, string[] args)
        {
            if (Friends == null || !(Friends?.IsLoaded ?? false))
            {
                SendReply(player, "Friends plugin not found! Please contact the server administrator.");
                return;
            }
            if (args.Length <= 0)
            {
                SendReply(player, "Invalid syntax! Try looking at the sleeping bag you want to give, then typing: <color=#8aff47>/bag <playername></color>");
                return;
            }
            BasePlayer target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "A player was not found online or sleeping with that name!");
                return;
            }
            if (target.userID == player.userID)
            {
                SendReply(player, "You cannot give yourself a sleeping bag/bed!");
                return;
            }
            var isFriends = Friends?.Call<bool>("HasFriend", target.userID, player.userID) ?? false;
            if (!isFriends)
            {
                SendReply(player, "This player does not have you added as a friend!");
                return;
            }


            var input = player?.serverInput ?? null;

            //   var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new(player.eyes.position, Quaternion.Euler(input.current.aimAngles) * Vector3.forward);
            var prefabname = "Unknown";



            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hitt, 4f, deployColl))
            {
                var ent = hitt.GetEntity();
                if (ent == null || ent.IsDestroyed) return;
                var bag = ent?.GetComponent<SleepingBag>() ?? null;
                if (bag == null) return;
                var cleanname = "sleeping bag";
                if (!string.IsNullOrEmpty(prefabname)) if (prefabname.Contains("bed")) cleanname = "bed";
                if (bag.deployerUserID != player.userID)
                {
                    SendReply(player, "You do not own this " + cleanname + "!");
                    return;
                }
                bag.deployerUserID = target.userID;
                bag.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                SendReply(player, "Your " + cleanname + " has been given to: \"" + target.displayName + "\"" + ", they can now respawn using it.");
            }

        }


        [ChatCommand("code")]
        private void cmdCodeLock(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var eyePos = player?.eyes?.position ?? Vector3.zero;
            Ray ray = new(eyePos, GetPlayerRotation(player));

            var hits = UnityEngine.Physics.SphereCastAll(ray, 0.075f, 4f, _constructionColl);
            var hitLocks = hits?.Where(p => p.GetEntity() != null && (p.GetEntity().GetSlot(BaseEntity.Slot.Lock) != null || (p.GetEntity()?.children?.Any(x => x != null && x.GetSlot(BaseEntity.Slot.Lock) != null) ?? false)))?.Select(p => p.GetEntity())?.ToList() ?? null;
            if (hitLocks == null || hitLocks.Count < 1)
            {
                SendReply(player, "Invalid/no entity");
                return;
            }
            hitLocks.Distinct();
            var minDist = hitLocks?.Min(p => Vector3.Distance(p?.transform?.position ?? Vector3.zero, eyePos)) ?? -1f;
            if (minDist == -1f)
            {
                SendReply(player, "Got bad entity distance");
                return;
            }
            var hitt = hitLocks?.Where(p => Vector3.Distance(p?.transform?.position ?? Vector3.zero, eyePos) <= minDist)?.FirstOrDefault() ?? null;
            if (hitt == null || hitt.IsDestroyed)
            {
                SendReply(player, "Got bad entity (distance)");
                return;
            }

            if (hitt != null)
            {
                var hitEnt = hitt;
                var prefabname = hitt?.ShortPrefabName ?? "Unknown";
                var bag = hitEnt as BaseEntity;
                var childLocks = hitt?.children?.Where(p => p != null && p.GetSlot(BaseEntity.Slot.Lock)) ?? null;
                var nearestLockDistance = (childLocks != null && childLocks.Any()) ? childLocks?.Min(p => Vector3.Distance(p?.transform?.position ?? Vector3.zero, eyePos)) ?? -1f : -1f;
                var nearestLock = (childLocks != null && childLocks.Any()) ? childLocks?.Where(p => Vector3.Distance(p?.transform?.position ?? Vector3.zero, eyePos) <= nearestLockDistance)?.FirstOrDefault()?.GetSlot(BaseEntity.Slot.Lock) ?? null : null;
                var slot = hitt?.GetSlot(BaseEntity.Slot.Lock) ?? nearestLock ?? null;
                var slotlock = slot as CodeLock;
                var baseLock = slot as BaseLock;
                if (args.Length >= 1)
                {
                    if (args[0] == "open")
                    {
                        if (hitEnt?.GetEntity()?.GetComponent<StorageContainer>() == null)
                        {
                            SendReply(player, "This entity does not appear to be a valid box!");
                            return;
                        }
                        var box = hitEnt.GetComponent<StorageContainer>();
                        var flag = box.HasFlag(BaseEntity.Flags.Open); // if the box is already open, we don't wanna see the flag to not be open. If the box is already not open, we don't wanna set it to open, since this is supposed to be silent

                        box.PlayerOpenLoot(player);
                        box.SetFlag(BaseEntity.Flags.Open, flag);
                        return;
                    }
                    if (args[0].ToLower() == "list")
                    {
                        if (slotlock == null)
                        {
                            SendReply(player, "no slot lock!");
                            return;
                        }
                        var players = slotlock?.whitelistPlayers ?? null;
                        if (players != null && players.Count > 0)
                        {
                            var pSB = new StringBuilder();
                            foreach (var p in players) pSB.Append(GetDisplayNameFromID(p) + ", ");
                            SendReply(player, pSB.ToString().TrimEnd(", ".ToCharArray()));
                        }
                        else SendReply(player, "No players");
                    }
                }

                if (bag.ShortPrefabName.Contains("turret"))
                {
                    var turret = bag?.GetComponent<AutoTurret>() ?? null;
                    if (turret == null) return;
                    if (!turret.IsAuthed(player))
                    {
                        turret.authorizedPlayers.Add(new ProtoBuf.PlayerNameID()
                        {
                            userid = player.userID,
                            username = player.displayName
                        });
                        turret.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                        if (turret.target == player) turret.SetTarget(null);
                        SendReply(player, "Added you to list of authed players for this turret");
                    }
                    else
                    {
                        foreach (var p in turret.authorizedPlayers)
                        {
                            if (p == null) continue;
                            if (p.userid == player.userID)
                            {
                                turret.authorizedPlayers.Remove(p);
                                turret.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                                if (turret.target == player) turret.SetTarget(null);
                                SendReply(player, "Removed authorization from this turret.");
                            }
                        }
                    }

                    return;
                }


                if (slot == null || (slotlock == null && baseLock == null))
                {
                    SendReply(player, "Unable to find a code lock on this entity: " + bag.ShortPrefabName);
                    return;
                }
                if (args.Length >= 1)
                {
                    if (args[0] == "add")
                    {
                        var list = slotlock?.whitelistPlayers ?? null;
                        var target = player;
                        if (args.Length > 1) target = FindPlayerByPartialName(args[1], true);
                        if (target == null)
                        {
                            SendReply(player, "Bad target!");
                            return;
                        }
                        if (slotlock != null)
                        {
                            if (list.Contains(target.userID))
                            {
                                list.Remove(target.userID);
                                SendReply(player, "Removed " + target.displayName + " from list of players authorized to this code lock.");
                            }
                            else
                            {
                                list.Add(target.userID);
                                SendReply(player, "Added " + target.displayName + " to the list of players authorized to this code lock.");
                            }
                        }
                        else
                        {
                            if (lockOverride.Contains(target.UserIDString))
                            {
                                lockOverride.Remove(target.UserIDString);
                                SendReply(player, "Removed: " + target.displayName);
                            }
                            else
                            {
                                lockOverride.Add(target.UserIDString);
                                SendReply(player, "Added: " + target.displayName);
                            }
                        }

                        return;
                    }
                    if (args[0] == "unlock")
                    {
                        if (!baseLock.HasFlag(BaseEntity.Flags.Locked))
                        {
                            SendReply(player, "This code lock is already unlocked!");
                            return;
                        }
                        baseLock.SetFlag(BaseEntity.Flags.Locked, false);
                        baseLock.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                        return;
                    }
                }
                var code = slotlock?.code ?? string.Empty;
                var guestCode = slotlock?.guestCode ?? string.Empty;
                SendReply(player, "The code to this code lock is: " + code + ", if you wish to unlock this forcefully, please type /code unlock.\nIf you wish to open this (box) silently, please type /code open");
                if (!string.IsNullOrEmpty(guestCode)) SendReply(player, guestCode + " <-- guest code");
            }
        }

        //   [ChatCommand("shock")]
        private void cmdSlapPlayer(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin && !player.HasPermission("compilation.heal")) return;
            if (args.Length >= 5 || args.Length < 2)
            {
                SendReply(player, "Incorrect Syntax, example: /slap playername 5");
                return;
            }
            var count = 0;
            var repeatTime = 0.180f;
            if (args.Length >= 3)
            {
                if (!int.TryParse(args[2], out count))
                {
                    SendReply(player, args[2] + " is not a valid number!");
                    return;
                }
            }
            if (args.Length >= 4)
            {
                if (!float.TryParse(args[3], out repeatTime))
                {
                    SendReply(player, args[3] + " is not a valid number!");
                    return;
                }
            }

            var target = FindPlayerByPartialName(args[0], true);
            if (target == null)
            {
                SendReply(player, "Could not find the specified player \"" + args[0] + "\".");
                return;
            }
            if (target.IsDead())
            {
                SendReply(player, target.displayName + " is dead!");
                return;
            }
            if (!double.TryParse(args[1], out double dmgs))
            {
                SendReply(player, args[1] + " is not a valid number!");
                return;
            }
            if (count == 1) ShockPlayer(target, dmgs);
            else
            {

                if (repeatTime <= 0.01) for (int i = 1; i < count; i++) ShockPlayer(target, dmgs);
                else
                {
                    Action shockInv = null;
                    shockInv = InvokeRepeating(target, () =>
                    {
                        if (target == null || (target?.IsDead() ?? true))
                        {
                            InvokeHandler.CancelInvoke(target, shockInv);
                            return;
                        }
                        ShockPlayer(target, dmgs);
                    }, 0.01f, repeatTime, count);
                }
            }
        }

        private void ShockPlayer(BasePlayer target, double dmg)
        {
            if (target == null || !target.IsAlive()) return;
            var reduceAmount = 4.6f;
            var increaseAmount = 0f;
            if (target.IsRunning())
            {
                increaseAmount = 1.51f;
                reduceAmount = 0f;
            }
            if (dmg > 0.0f) target.Hurt((float)dmg, Rust.DamageType.ElectricShock, target, false);
            var eyesforward = target?.eyes?.HeadForward() ?? Vector3.zero;
            if (reduceAmount > 0.0f) eyesforward /= reduceAmount;
            if (increaseAmount > 0.0f) eyesforward *= increaseAmount;
            var eyesPos = target?.eyes?.position ?? Vector3.zero;
            var targPos = eyesPos + eyesforward;
            Effect.server.Run("assets/prefabs/locks/keypad/effects/lock.code.shock.prefab", targPos, targPos, null, false);
        }

        [ChatCommand("heal")]
        private void cmdHealPlayer(BasePlayer player, string command, string[] args)
        {
            if (!canExecute(player, "heal")) return;
            if (args.Length >= 3 || args.Length < 2)
            {
                SendReply(player, "Incorrect Syntax, example: /heal playername 5");
                return;
            }
            BasePlayer target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, "Could not find the specified player \"" + args[0] + "\".");
                return;
            }

            if (!int.TryParse(args[1], out int dmgs)) SendReply(player, "Couldn't parse: " + args[1] + " as an integer");
            target.Heal(dmgs);
            target.metabolism.bleeding.value = 0;
        }

        //IsUnderFoundation - compliments of Rusty
        private bool IsUnderFoundation(Vector3 position)
        {
            var val = false;
            var colliders = Pool.GetList<Collider>();
            try
            {
                Vis.Colliders(position, 2f, colliders, Rust.Layers.Construction);
                foreach (var collider in colliders)
                {
                    var block = collider.GetComponentInParent<BuildingBlock>();
                    if (block == null) continue;
                    if (block.prefabID == 72949757 && block.transform.position.y > position.y)
                    {
                        val = true;
                        break;
                    }
                }
                return val;
            }
            finally { Pool.FreeList(ref colliders); }

        }

        private bool IsEscapeBlocked(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            return NoEscape?.Call<bool>("IsEscapeBlockedS", userID) ?? false;
        }

        private bool FoundationBelowEnt(BaseEntity entity)
        {
            if (entity == null || (entity?.IsDestroyed ?? true)) return false;
            var position = entity?.transform?.position ?? Vector3.zero;
            var rays = UnityEngine.Physics.RaycastAll(new Ray(position + Vector3.down * 200f, Vector3.up), 250, Rust.Layers.Construction);
            if (rays != null && rays.Length > 0)
            {
                for (int i = 0; i < rays.Length; i++)
                {
                    var hit = rays[i];
                    var ent = hit.GetEntity();
                    if (ent == null || (ent?.IsDestroyed ?? true) || ent == entity) continue;
                    if (ent.ShortPrefabName == "foundation") return true;
                }
            }
            return false;
        }

        private bool IsUnderFoundation2(Vector3 position, bool ignoreTwigWood = true)
        {
            var rays = UnityEngine.Physics.RaycastAll(new Ray(position + Vector3.up * 200f, Vector3.down), 250, Rust.Layers.Construction);
            if (rays != null && rays.Length > 1)
            {
                for (int i = 0; i < rays.Length; i++)
                {
                    var hit = rays[i];
                    var entity = hit.GetEntity();
                    if (entity == null || (entity?.IsDestroyed ?? true)) continue;
                    var grade = entity?.GetComponent<BuildingBlock>()?.grade ?? BuildingGrade.Enum.None;
                    if ((grade == BuildingGrade.Enum.Twigs || grade == BuildingGrade.Enum.Wood) && ignoreTwigWood) continue;
                    if (entity.ShortPrefabName == "foundation") return true;
                }
            }
            return false;
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
                    var lastConsumeObj = GetValue(player.metabolism, "lastConsumeTime");
                    if (lastConsumeObj == null)
                    {
                        PrintWarning("Null lastConsumeObj for: " + player.displayName);
                        continue;
                    }
                    if (float.TryParse(lastConsumeObj.ToString(), out float lastConsume) && lastConsume > 0.0f)
                    {
                        var timeDiff = UnityEngine.Time.time - lastConsume;
                        if (timeDiff <= 0.085) justConsumed.Add(player);
                    }
                }

                return justConsumed.Count == 1 ? justConsumed[0] : null;
            }
            finally { Pool.FreeList(ref justConsumed); }
        }

        private void OnItemUse(Item item, int amountToUse)
        {
            if (item == null || amountToUse < 1) return;
            if (item.IsBlueprint())
            {
                var player = item?.GetOwnerPlayer() ?? item?.parent?.playerOwner ?? FindJustConsumed() ?? FindLooterFromCrate(item?.parent?.entityOwner ?? null);
                if (player != null && player.IsConnected)
                {
                    player.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
                }
            }
        }

        private static readonly System.Random replaceRnd = new();

        private void ReplaceRiflebody(LootContainer lootCont, TimeSpan span = default)
        {
            if (lootCont == null || lootCont.IsDestroyed || lootCont.inventory == null) return;
            var itemList = lootCont?.inventory?.itemList ?? null;
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (item == null) continue;
                var oldPos = item?.position ?? -1;
                if (item.info.shortname == "riflebody")
                {
                    var nextDub = replaceRnd.Next(0, 100);
                    var perc = (span == TimeSpan.MinValue) ? -1 : (span.TotalHours < ScaleHours(72)) ? 90 : (span.TotalHours < ScaleHours(96)) ? 60 : (span.TotalHours < ScaleHours(120)) ? 40 : 20;
                    if (IsEasyServer()) perc = (int)Math.Round(perc * 0.6f, MidpointRounding.AwayFromZero);
                    if (nextDub < perc) //90% chance?
                    {
                        var itemRng = UnityEngine.Random.Range(0, 4);
                        var replaceDef = ItemManager.FindItemDefinition((itemRng == 0 || itemRng == 1) ? "smgbody" : "semibody");
                        if (replaceDef == null)
                        {
                            PrintWarning("replace def null?!");
                            continue;
                        }
                        var replaceItem = ItemManager.Create(replaceDef, item.amount);
                        RemoveFromWorld(item);
                        var findItem = itemList?.Where(p => p.info.itemid == replaceDef.itemid)?.FirstOrDefault() ?? null;
                        if (findItem != null && (findItem.amount + replaceItem.amount) <= replaceDef.stackable) findItem.amount += replaceItem.amount;
                        else
                        {
                            if (!replaceItem.MoveToContainer(lootCont.inventory, oldPos))
                            {
                                RemoveFromWorld(replaceItem);
                            }
                        }
                    }
                }
            }
        }

        private void ReplaceHQM(LootContainer lootCont, TimeSpan span = default)
        {
            if (lootCont == null || lootCont.IsDestroyed || lootCont.inventory == null) return;
            var itemList = lootCont?.inventory?.itemList ?? null;
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (item == null) continue;
                //   var oldPos = item?.position ?? -1;
                if (item.info.shortname == "metal.refined")
                {
                    // PrintWarning("found hqm");
                    var nextDub = replaceRnd.Next(0, 100);
                    var perc = (span == TimeSpan.MinValue) ? -1 : (span.TotalHours < ScaleHours(72)) ? 90 : (span.TotalHours < ScaleHours(96)) ? 60 : (span.TotalHours < ScaleHours(120)) ? 40 : 20;
                    if (nextDub < perc) //90% chance?
                    {
                        var replaceAmt = Mathf.Clamp((int)Math.Round(item.amount / UnityEngine.Random.Range(1.6f, 3.5f), MidpointRounding.AwayFromZero), 1, item.amount);
                        item.amount = replaceAmt;
                        item.MarkDirty();
                    }
                }
            }
        }

        private ItemAmount CreateItemAmount(string shortName, int amount = 1)
        {
            if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException(nameof(shortName));
            if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));

            var def = ItemManager.FindItemDefinition(shortName);
            if (def == null) throw new NullReferenceException(nameof(def));


            var newAmount = new ItemAmount(def, amount);
            newAmount.amount = newAmount.startAmount = amount;

            return newAmount;
        }
        /*/
        private void ReplaceItem(ItemDefinition target, ItemAmount replace, LootContainer lootCont, float perc, bool removeMultiple = false)
        {
            if (target == null || lootCont == null || lootCont.IsDestroyed || lootCont.inventory == null || perc < 0 || perc > 100 || (replace != null && replace.amount < 1))
            {
                PrintWarning("failed init checks, target: " + (target?.shortname ?? string.Empty) + ", replace: " + (replace?.itemDef?.shortname ?? string.Empty) + " x" + (replace?.amount ?? 0) + ", lootCont: " + (lootCont?.ShortPrefabName ?? string.Empty) + ", perc: " + perc + ", isdestroyed: " + (lootCont?.IsDestroyed ?? false) + ", inv null: " + (lootCont?.inventory == null));
                //    RemoveFromWorld(replace);
                return;
            }
            var findItems = lootCont?.inventory?.itemList?.Where(p => p.info == target)?.ToList() ?? null;
            if (findItems == null) return;
            foreach (var findItem in findItems)
            {
                var nextDub = replaceRnd.Next(0, 100);
                var oldPos = findItem?.position ?? -1;
                if (nextDub <= perc)
                {
                    //PrintWarning("next dub <= perc: " + perc + ", " + target.shortname + ", " + replace?.itemDef?.shortname + " x" + replace?.amount);
                    RemoveFromWorld(findItem);
                    var replaceItem = ItemManager.Create(replace.itemDef, (int)Math.Round(replace.amount, MidpointRounding.AwayFromZero));
                    if (replaceItem != null && !replaceItem.MoveToContainer(lootCont.inventory, oldPos)) RemoveFromWorld(replaceItem);
                }
                if (!removeMultiple) return;
            }
        }/*/

        private int GetFirstFreeSlot(ItemContainer container)
        {
            if (container == null || container.itemList == null || container.itemList.Count < 1) return -1;

            for (int i = 0; i < container.capacity; i++) { if (container?.GetSlot(i) == null) return i; }

            return -1;
        }

        private void ReorderContainer(ItemContainer container)
        {
            if (container == null || container?.itemList == null) return;

            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

                for (int i = 0; i < container.itemList.Count; i++)
                {
                    var item = container.itemList[i];
                    item.position = i;
                    item.MarkDirty();
                }

            }
            finally
            {
                try { if (watch.Elapsed.TotalMilliseconds >= 2) PrintWarning(nameof(ReorderContainer) + " took " + watch.Elapsed.TotalMilliseconds + "ms"); }
                finally { Pool.Free(ref watch); }
            }

            //is it really as easy as this? ^^
            //it was. it doesn't necessarily keep the order, but this doesn't matter. players won't know the order of a lootcontainer lol
        }

        private Item FindStackableInContainer(ItemContainer container, ItemDefinition itemWantsToStack, int amount)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            Item stackable = null;

            for (int i = 0; i < container.itemList.Count; i++)
            {
                var item = container.itemList[i];
                if (item == null) continue;

                if (item?.info == itemWantsToStack && item.amount + amount <= item.MaxStackable())
                {
                    stackable = item;
                    break;
                }

            }

            return stackable;
        }

        private void ReplaceItem<T>(T itemToReplace, ItemAmount replacement, ItemContainer container, float perc, bool removeMultiple = false)
        {
            if (itemToReplace == null) throw new ArgumentNullException(nameof(itemToReplace));
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            if (replacement.amount < 1) throw new ArgumentOutOfRangeException(nameof(replacement.amount));
            if (container == null) throw new ArgumentNullException(nameof(container));

            if (perc > 100) perc = 100;
            if (perc <= 0) throw new ArgumentOutOfRangeException(nameof(perc));

            ItemDefinition def = null;
            var replaceId = -1;
            var replaceStr = string.Empty;

            if (itemToReplace is int) replaceId = Convert.ToInt32(itemToReplace);
            else if ((def = itemToReplace as ItemDefinition) == null)
            {
                replaceStr = itemToReplace?.ToString();
                if (string.IsNullOrEmpty(replaceStr))
                {
                    PrintWarning(nameof(ReplaceItem) + " was called with a type that is not int, def, or string!!!");
                    return;
                }
            }


            for (int i = 0; i < container.itemList.Count; i++)
            {
                var item = container.itemList[i];

                if (item?.info == null)
                    continue;

                var rng = replaceRnd.Next(0, 101);


                if (rng > perc) //rng not met. why even bother checking items? I ASSUME checking this before seeing if we found our item is more performant even if it is ran more often
                    continue;


                if (def != null && item.info != def)
                    continue;

                if (replaceId != -1 && item.info.itemid != replaceId)
                    continue;


                if (def == null && replaceId == -1 && (!replaceStr.Equals(item.info.shortname, StringComparison.OrdinalIgnoreCase) && !replaceStr.Equals(item.info.displayName.english, StringComparison.OrdinalIgnoreCase)))
                    continue;


                var oldPos = item?.position ?? -1;

                RemoveFromWorld(item);

                var intAmount = (int)Math.Round(replacement.amount, MidpointRounding.AwayFromZero);

                var foundStackable = FindStackableInContainer(container, replacement.itemDef, intAmount);
                if (foundStackable != null)
                {
                    foundStackable.amount += intAmount;
                    foundStackable.MarkDirty();

                    ReorderContainer(container);
                }
                else
                {
                    var replaceItem = ItemManager.Create(replacement.itemDef, (int)Math.Round(replacement.amount, MidpointRounding.AwayFromZero));
                    if (replaceItem == null)
                    {
                        PrintWarning("replaceItem somehow null!!");
                        continue;
                    }

                    if (!replaceItem.MoveToContainer(container, oldPos) && !replaceItem.MoveToContainer(container)) RemoveFromWorld(replaceItem);
                }

                if (!removeMultiple) return;
            }

        }

        private void ReplaceC4(LootContainer lootCont, TimeSpan span = default)
        {
            if (lootCont == null || lootCont.IsDestroyed || lootCont.inventory == null) return;
            var itemList = lootCont?.inventory?.itemList ?? null;
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (item == null) continue;
                var oldPos = item?.position ?? -1;
                if (item.info.shortname == "explosive.timed.charge")
                {
                    PrintWarning("found c4");
                    var nextDub = replaceRnd.Next(0, 100);
                    var perc = (span == TimeSpan.MinValue) ? -1 : (span.TotalHours < ScaleHours(72)) ? 95 : (span.TotalHours < ScaleHours(96)) ? 80 : (span.TotalHours < ScaleHours(120)) ? 50 : 30;

                    if (IsEasyServer()) perc = (int)Math.Round(perc * 0.9, MidpointRounding.AwayFromZero);
                    else if (IsHardServer()) perc = (int)Math.Round(perc * 1.75, MidpointRounding.AwayFromZero);

                    PrintWarning("perc: " + perc + ", span hours: " + span.TotalHours + ", days: " + span.TotalDays);
                    if (nextDub < perc) //90% chance?
                    {
                        PrintWarning("next dub <: " + nextDub);
                        var rngReplace = ItemManager.itemList?.Where(p => p.rarity == Rust.Rarity.Rare)?.ToList()?.GetRandom() ?? null;
                        if (rngReplace == null)
                        {
                            PrintWarning("got null rng replace");
                        }
                        RemoveFromWorld(item);
                        if (rngReplace != null)
                        {
                            var rngReplaceItem = ItemManager.Create(rngReplace, 1);
                            PrintWarning("replacing c4 with: " + rngReplace?.shortname);
                            if (!rngReplaceItem.MoveToContainer(lootCont.inventory, oldPos)) RemoveFromWorld(rngReplaceItem);
                        }




                    }
                    else PrintWarning("next c4 dub: " + nextDub);
                }
            }
        }

        private void ReplaceSignal(LootContainer lootCont, TimeSpan span = default)
        {
            if (lootCont == null || lootCont.IsDestroyed || lootCont.inventory == null) return;
            var itemList = lootCont?.inventory?.itemList ?? null;
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (item == null) continue;
                var oldPos = item?.position ?? -1;
                if (item.info.shortname == "supply.signal")
                {
                    PrintWarning("found ss");
                    var nextDub = replaceRnd.Next(0, 100);
                    var perc = (span == TimeSpan.MinValue) ? -1 : (span.TotalHours < ScaleHours(72)) ? 70 : (span.TotalHours < ScaleHours(96)) ? 40 : (span.TotalHours < ScaleHours(120)) ? 30 : 15;
                    if (IsEasyServer()) perc = (int)Math.Round(perc / 1.75, MidpointRounding.AwayFromZero);
                    PrintWarning("perc: " + perc + ", span hours: " + span.TotalHours + ", days: " + span.TotalDays);
                    if (nextDub < perc) //90% chance?
                    {
                        PrintWarning("next dub <: " + nextDub);
                        RemoveFromWorld(item);
                        PrintWarning("Destroy signal!");
                    }
                    else PrintWarning("next signal dub: " + nextDub);
                }
            }
        }

        private void ReplaceHazmat(LootContainer lootCont)
        {
            if (lootCont == null || lootCont.IsDestroyed || lootCont.inventory == null) return;
            var itemList = lootCont?.inventory?.itemList ?? null;
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (item == null) continue;
                //  var oldPos = item?.position ?? -1;
                if (item.info.shortname == "hazmatsuit")
                {
                    var nextDub = replaceRnd.Next(0, 101);
                    var perc = (SaveSpan.TotalHours < ScaleHours(24)) ? 90 : (SaveSpan.TotalHours < ScaleHours(48)) ? 80 : (SaveSpan.TotalHours < ScaleHours(72)) ? 60 : (SaveSpan.TotalHours < ScaleHours(96)) ? 55 : 50;
                    if (IsEasyServer()) perc = (int)Math.Round(perc * 0.66, MidpointRounding.AwayFromZero);
                    if (nextDub <= perc)
                    {
                        //     PrintWarning("perc: " + perc + " for hazmat replace");
                        var repRng = UnityEngine.Random.Range(0, 6);
                        var repName = (repRng > 3) ? "bone.armor.suit" : "scrap";
                        var repAmt = (repName == "scrap") ? UnityEngine.Random.Range(6, 15) : 1;
                        var replaceItem = ItemManager.Create(ItemManager.FindItemDefinition(repName), repAmt);
                        if (replaceItem == null)
                        {
                            PrintWarning("replace item null for haz?");
                            return;
                        }
                        // PrintWarning("replacing hazzy with: " + repName + " x" + repAmt);
                        RemoveFromWorld(item);
                        if (!replaceItem.MoveToContainer(lootCont.inventory)) RemoveFromWorld(replaceItem);
                    }
                }
            }
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null) return;
            var cupboard = entity as BuildingPrivlidge;
            if (cupboard != null) Puts("Cupboard (" + GetDisplayNameFromID(cupboard.OwnerID) + ") destroyed @ " + (cupboard?.transform?.position ?? Vector3.zero));

            var loot = entity as LootContainer;
            var storage = entity as StorageContainer;

            if (loot != null) allContainers.Remove(loot);
            if (storage != null) storageContainers.Remove(storage);

            if (entity.prefabID == 3032863244)
            {
                var pos = entity?.transform?.position ?? Vector3.zero;
                var expDmg = new Rust.DamageTypeEntry
                {
                    type = Rust.DamageType.Explosion,
                    amount = UnityEngine.Random.Range(7f, 21f)
                };
                var bluntDmg = new Rust.DamageTypeEntry
                {
                    type = Rust.DamageType.Blunt,
                    amount = UnityEngine.Random.Range(10f, 21f)
                };
                var dmgList = Pool.GetList<Rust.DamageTypeEntry>();
                try
                {
                    dmgList.Add(bluntDmg);
                    dmgList.Add(expDmg);
                    var baseEnt = entity.GetComponent<BaseEntity>();
                    DamageUtil.RadiusDamage(baseEnt, baseEnt, pos, 1f, 1.5f, dmgList, 1, true);
                    dmgList.Remove(expDmg);
                    expDmg.amount = UnityEngine.Random.Range(60f, 120f);
                    dmgList.Add(expDmg);
                    DamageUtil.RadiusDamage(baseEnt, baseEnt, pos, 1f, 1.25f, dmgList, _constructionColl, true);
                }
                finally { Pool.FreeList(ref dmgList); }
            }

        }

        private List<ItemDefinition> _steamItems = null;
        private List<ItemDefinition> _funItems = null;
        private List<ItemDefinition> _teaItems = null;
        private readonly string[] _rngFallbacks = { "door.hinged.toptier", "syringe.medical", "largemedkit", "furnace.large", "workbench1", "wall.external.high", "gates.external.high.wood", "wall.frame.garagedoor", "fuse", "sparkplug1", "sparkplug2", "surveycharge", "toolgun", "cakefiveyear", "xmas.window.garland" };

        private void OnLootSpawn(LootContainer container)
        {
            if (container == null)
            {
                PrintWarning("Container called null for OnLootSpawn");
                return;
            }

            container.Invoke(() => ReorderContainer(container?.inventory), 5f);

            var prefabName = container?.ShortPrefabName ?? string.Empty;
            if (prefabName.StartsWith("dm ")) return;

            var prefabId = container?.prefabID ?? 0;

            //cap checks!

            if (prefabId == 3286607235 || prefabId == 96231181 || prefabId == 1314849795 || prefabId == 1892026534 || prefabId == 3027334492 || prefabId == 1546200557 || prefabId == 2857304752 || prefabId == 1009499252) //underwater elite crate is the second one, third is heli_crate, final is tools crate and tools crate underwater, respectively. actual final is crate_normal, crate_normal_2
            {
                //it actually opens a panel with 12 slots but only has 6 available by default

                container.inventory.capacity = 12;
            }

            var hackableCrate = container as HackableLockedCrate;

            if (hackableCrate?.inventory != null)
                hackableCrate.inventory.capacity = 36;

            var supply = container as SupplyDrop;

            if (supply?.inventory != null)
                supply.inventory.capacity = 36;

            //end cap checks!

            var pos = container?.transform?.position ?? Vector3.zero;
            timer.Once(0.5f, () =>
            {
                if (container == null || container.IsDestroyed || container.gameObject == null || container.inventory == null) return;
                var watch = Pool.Get<Stopwatch>();
                try
                {
                    watch.Restart();

                    var lootEnt = container;
                    Item findScrap = null;
                    for (int i = 0; i < lootEnt.inventory.itemList.Count; i++)
                    {
                        var item = lootEnt.inventory.itemList[i];
                        if (item?.info?.itemid == SCRAP_ITEM_ID)
                        {
                            findScrap = item;
                            break;
                        }
                    }

                    if (_steamItems == null)
                    {
                        _steamItems = new List<ItemDefinition>();

                        for (int i = 0; i < ItemManager.itemList.Count; i++)
                        {
                            var item = ItemManager.itemList[i];
                            if (item != null && item?.steamItem != null && !_ignoreItems.Contains(item.itemid)) //fix ox mask, AK spawn
                                _steamItems.Add(item);
                        }


                    }

                    if (_steamItems?.Count > 0)
                    {
                        var steamRng = UnityEngine.Random.Range(0, 101);
                        var steamNeed = prefabName.Contains("barrel") ? 0 : prefabName.Contains("crate") ? 3 : -1;
                        if (steamRng <= steamNeed)
                        {
                            var sign = ItemManager.Create(_steamItems.GetRandom(), 1);
                            if (sign != null && !sign.MoveToContainer(container.inventory)) RemoveFromWorld(sign);
                        }
                    }
                    else PrintWarning("no steamSigns?!");


                    //CHRISTMAS/XMAS
                    if (XMas.enabled && UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(1, 8))
                    {
                        var christmasItem = GetRandomChristmasItem();
                        if (christmasItem != null && !christmasItem.MoveToContainer(container.inventory)) RemoveFromWorld(christmasItem);
                    }

                    if (UnityEngine.Random.Range(0, 101) < 1) //1% chance to spawn cassette-related item regardless of container
                    {
                        var rngItem = UnityEngine.Random.Range(0, 4);

                        var itemToSpawn = rngItem == 0 ? "fun.casetterecorder" : ("cassette" + (rngItem == 1 ? string.Empty : rngItem == 2 ? ".medium" : ".short"));

                        //  PrintWarning("spawning cassette item: " + itemToSpawn);

                        var casetteItem = ItemManager.CreateByName(itemToSpawn);
                        if (!casetteItem.MoveToContainer(container.inventory)) RemoveFromWorld(casetteItem);

                    }

                    if (UnityEngine.Random.Range(0, 201) <= 1) //0.5%? chance to spawn any "Fun" category item regardless of container
                    {
                        if (_funItems == null)
                        {
                            _funItems = new List<ItemDefinition>();

                            for (int i = 0; i < ItemManager.itemList.Count; i++)
                            {
                                var item = ItemManager.itemList[i];
                                if (item.category == ItemCategory.Fun && !_ignoreItems.Contains(item.itemid)) _funItems.Add(item);
                            }
                        }

                        var rngFun = _funItems[UnityEngine.Random.Range(0, _funItems.Count)];
                        //PrintWarning("got rngFun item: " + rngFun.shortname);

                        var funItem = ItemManager.Create(rngFun);
                        if (!funItem.MoveToContainer(container.inventory)) RemoveFromWorld(funItem);

                    }

                    var guitarRng = UnityEngine.Random.Range(0, 201);
                    var guitarNeed = prefabName.Contains("barrel") ? 1 : prefabName.Contains("crate") ? UnityEngine.Random.Range(2, 4) : 0;
                    if (guitarRng <= guitarNeed)
                    {
                        var guitar = ItemManager.CreateByName("fun.guitar");
                        if (!guitar.MoveToContainer(container.inventory)) RemoveFromWorld(guitar);
                    }

                    var isEasy = IsEasyServer();
                    var isHard = IsHardServer();

                    var isCrateTools = prefabId == 1892026534;

                    if (isCrateTools || prefabName.Contains("barrel"))
                    {

                        if (isCrateTools && UnityEngine.Random.Range(0, 101) < 1 && (!isHard || SaveSpan.TotalDays > 4))
                        {
                            var coffinItem = ItemManager.CreateByName("coffin.storage", 1);
                            if (!coffinItem.MoveToContainer(container.inventory)) RemoveFromWorld(coffinItem);
                        }

                        if (!isHard && UnityEngine.Random.Range(0, 101) <= (isCrateTools ? 20 : 8))
                        {
                            //PrintWarning("!isHard, rng hit, is crate tools?: " + isCrateTools);

                            var rngComp = GetRandomComponent(UnityEngine.Random.Range(1, 5));
                            if (rngComp != null)
                            {
                                //   PrintWarning("Adding rngComp: " + rngComp.info.shortname + " x" + rngComp.amount + ", to: " + prefabName + " @ " + (container?.transform?.position ?? Vector3.zero));
                                if (!rngComp.MoveToContainer(container.inventory))
                                {
                                    RemoveFromWorld(rngComp);
                                    PrintWarning("couldn't move rngcomp to container!!");
                                }

                            }
                            else PrintWarning("rngComp is null!!!!");

                            //why are we spawning comps twice, separately? I'm not sure I understand. maybe I just wanted a chance for even more comps to potentially spawn - don't know

                            if (UnityEngine.Random.Range(0, 101) <= 7)
                            {
                                var rngComp2 = GetRandomComponent(UnityEngine.Random.Range(1, 4));
                                if (rngComp2 != null)
                                {
                                    if (!rngComp2.MoveToContainer(container.inventory))
                                    {
                                        RemoveFromWorld(rngComp2);
                                        PrintWarning("couldn't move rngcomp2 to container!!");
                                    }
                                }
                                else PrintWarning("rngComp2 is null!!!!");
                            }

                        }
                    }

                    if (!isHard)
                    {
                        if ((prefabName.Contains("crate") || (UnityEngine.Random.Range(0, 101) <= 12 && prefabName.Contains("barrel"))) && UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(0, 5))
                        {

                            var sb = Pool.Get<StringBuilder>();
                            try
                            {
                                var keyBase = sb.Clear().Append("keycard_");

                                var keyAdd = UnityEngine.Random.Range(0, 101) <= 66 ? (UnityEngine.Random.Range(0, 101) <= 17 ? "blue" : "green") : (UnityEngine.Random.Range(0, 101) <= 17 ? "red" : "blue");

                                var keyName = keyBase.Append(keyAdd).ToString();

                                var keyItem = ItemManager.CreateByName(keyName, UnityEngine.Random.Range(0, 101) <= 10 ? UnityEngine.Random.Range(1, 3) : 1);

                                if (!keyItem.MoveToContainer(container.inventory)) RemoveFromWorld(keyItem);
                            }
                            finally { Pool.Free(ref sb); }


                        }

                        if (findScrap != null)
                        {
                            if (findScrap.amount < 2) PrintWarning("findScrap amount < 2: " + container.ShortPrefabName + " " + (container?.transform?.position ?? Vector3.zero));
                            var addAmount = !isEasy ? UnityEngine.Random.Range(1, 6) : UnityEngine.Random.Range(3, 10);

                            if (prefabName.Contains("barrel")) //add a isBarrel check later
                                addAmount = (int)Mathf.Clamp(addAmount * 0.5f, 0, int.MaxValue);

                            if (isEasy)
                                addAmount = ScaleScrapAmountBasedOnContainer(addAmount, prefabId);

                            findScrap.amount += addAmount;
                            findScrap.MarkDirty();
                        }

                        if (isEasy && UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(5, 11))
                        {
                            var batteries = ItemManager.CreateByItemID(609049394, UnityEngine.Random.Range(1, 4));
                            if (batteries != null && !batteries.MoveToContainer(container.inventory)) RemoveFromWorld(batteries);
                        }
                    }



                    if (supply != null && isEasy)
                    {


                        var rng1 = UnityEngine.Random.Range(0, 101);

                        if (rng1 <= 1)
                        {
                            PrintWarning("rng1 <= 1 for: " + container?.ShortPrefabName + " at: " + (container?.transform?.position ?? Vector3.zero) + " (supply crate)");
                            var m249 = ItemManager.CreateByName("lmg.m249");
                            if (m249 != null && !m249.MoveToContainer(container.inventory)) RemoveFromWorld(m249);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 3)
                        {
                            var l96 = ItemManager.CreateByName("rifle.l96");
                            if (!l96.MoveToContainer(supply.inventory)) RemoveFromWorld(l96);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 8)
                        {
                            var lr300 = ItemManager.CreateByName("rifle.lr300");
                            if (lr300 != null && !lr300.MoveToContainer(container.inventory)) RemoveFromWorld(lr300);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 15)
                        {
                            var m92 = ItemManager.CreateByName("pistol.m92");
                            if (m92 != null && !m92.MoveToContainer(container.inventory)) RemoveFromWorld(m92);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 10)
                        {
                            var m39 = ItemManager.CreateByName("rifle.m39");
                            if (!m39.MoveToContainer(supply.inventory)) RemoveFromWorld(m39);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 5)
                        {
                            var expAmount = UnityEngine.Random.Range(5, 30);
                            var explosives = ItemManager.CreateByName("explosives", expAmount);
                            if (!explosives.MoveToContainer(supply.inventory)) RemoveFromWorld(explosives);
                        }
                        if (UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(3, 8))
                        {
                            var rngSupply = UnityEngine.Random.Range(1, UnityEngine.Random.Range(0, 101) <= 33 ? 4 : 3);
                            var signals = ItemManager.CreateByName("supply.signal", rngSupply);
                            if (!signals.MoveToContainer(supply.inventory)) RemoveFromWorld(signals);
                        }
                        if (UnityEngine.Random.Range(0, 101) <= 21)
                        {
                            var rngExpAmmo = UnityEngine.Random.Range(5, 20);
                            var ammo = ItemManager.CreateByName("ammo.rifle.explosive", rngExpAmmo);
                            if (!ammo.MoveToContainer(supply.inventory)) RemoveFromWorld(ammo);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 2)
                        {
                            var mgl = ItemManager.CreateByName("multiplegrenadelauncher");

                            if (!mgl.MoveToContainer(supply.inventory)) RemoveFromWorld(mgl);
                            else
                            {
                                if (UnityEngine.Random.Range(0, 101) <= 33)
                                {
                                    var rngAmmo = UnityEngine.Random.Range(1, 7);
                                    var mglAmmo = ItemManager.CreateByName("ammo.grenadelauncher.he", rngAmmo);
                                    if (!mglAmmo.MoveToContainer(supply.inventory)) RemoveFromWorld(mglAmmo);
                                }
                            }

                        }

                        if (UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(12, 27))
                        {
                            var rngBerries = UnityEngine.Random.Range(5, 15);
                            var berries = ItemManager.CreateByName("blueberries", rngBerries);
                            if (!berries.MoveToContainer(supply.inventory)) RemoveFromWorld(berries);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 12)
                        {
                            var rngSyringes = UnityEngine.Random.Range(3, 11);
                            var syringes = ItemManager.CreateByName("syringe.medical", rngSyringes);
                            if (!syringes.MoveToContainer(supply.inventory)) RemoveFromWorld(syringes);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 10)
                        {
                            var rngBattery = UnityEngine.Random.Range(40, 200);
                            var batteries = ItemManager.CreateByName("battery.small", rngBattery);
                            if (!batteries.MoveToContainer(supply.inventory)) RemoveFromWorld(batteries);
                        }

                        /*/
                        if (UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(19, 34))
                        {
                            var rngOre = UnityEngine.Random.Range(1, 4);
                            var rngType = rngOre == 1 ? "stones" : rngOre == 2 ? "metal.ore" : "sulfur.ore";
                            var rngAmount = UnityEngine.Random.Range(7500, 40000);
                            var ore = ItemManager.CreateByName(rngType, rngAmount);
                            if (!ore.MoveToContainer(supply.inventory)) RemoveFromWorld(ore);
                        }/*/

                        if (UnityEngine.Random.Range(0, 101) <= 25)
                        {
                            var rngHQM = UnityEngine.Random.Range(10, 51);
                            var hqm = ItemManager.CreateByName("metal.refined", rngHQM);
                            if (!hqm.MoveToContainer(supply.inventory)) RemoveFromWorld(hqm);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 22)
                        {
                            var rngPipes = UnityEngine.Random.Range(6, UnityEngine.Random.Range(12, 21));
                            var pipes = ItemManager.CreateByName("metalpipe", rngPipes);
                            if (!pipes.MoveToContainer(supply.inventory)) RemoveFromWorld(pipes);
                        }

                        /*/
                        if (UnityEngine.Random.Range(0, 101) <= 20)
                        {
                            var rngCharcoal = UnityEngine.Random.Range(3750, UnityEngine.Random.Range(5000, 12000));
                            var charcoal = ItemManager.CreateByName("charcoal", rngCharcoal);
                            if (!charcoal.MoveToContainer(supply.inventory)) RemoveFromWorld(charcoal);
                        }
                        if (UnityEngine.Random.Range(0, 101) <= 1)
                        {
                            var rngCharcoal = UnityEngine.Random.Range(45000, 75000);
                            var charcoal = ItemManager.CreateByName("charcoal", rngCharcoal);
                            if (!charcoal.MoveToContainer(supply.inventory)) RemoveFromWorld(charcoal);
                        }/*/

                        if (UnityEngine.Random.Range(0, 101) <= 66)
                        {
                            var rarity = UnityEngine.Random.Range(0, 101) <= 15 ? Rust.Rarity.VeryRare : Rust.Rarity.Rare;
                            var rngBP = GetRandomBP(rarity, GetRandomCategory(ItemCategory.All, ItemCategory.Fun, ItemCategory.Resources));
                            if (rngBP != null && !rngBP.MoveToContainer(supply.inventory)) RemoveFromWorld(rngBP);
                        }


                        var rngScrap = UnityEngine.Random.Range(UnityEngine.Random.Range(90, 181), UnityEngine.Random.Range(240, 301));

                        var supplyScrap = ItemManager.CreateByName("scrap", rngScrap);

                        if (!supplyScrap.MoveToContainer(supply.inventory))
                            RemoveFromWorld(supplyScrap);
                    }


                    var isShipHackCrate = hackableCrate != null && (container?.GetParentEntity()?.prefabID ?? 0) == 3234960997; //cargoship ID

                    if (isShipHackCrate && !isHard && SaveSpan.TotalHours > 48)
                    {
                        var rng1 = UnityEngine.Random.Range(0, 101);
                        var rng2 = UnityEngine.Random.Range(1, 6);
                        if (rng1 <= rng2)
                        {
                            PrintWarning("rng1 <= " + rng2 + " for: " + container?.ShortPrefabName + " at: " + (container?.transform?.position ?? Vector3.zero) + " (ship crate)");
                            var m249 = ItemManager.CreateByName("lmg.m249");
                            if (m249 != null && !m249.MoveToContainer(container.inventory)) RemoveFromWorld(m249);
                        }
                        if (UnityEngine.Random.Range(0, 101) <= 8)
                        {
                            var lr300 = ItemManager.CreateByName("rifle.lr300");
                            if (lr300 != null && !lr300.MoveToContainer(container.inventory)) RemoveFromWorld(lr300);
                        }
                        if (UnityEngine.Random.Range(0, 101) <= 15)
                        {
                            var m92 = ItemManager.CreateByName("pistol.m92");
                            if (m92 != null && !m92.MoveToContainer(container.inventory)) RemoveFromWorld(m92);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 10)
                        {
                            var rngAmmo = UnityEngine.Random.Range(1, 4);
                            var mglAmmo = ItemManager.CreateByName("ammo.grenadelauncher.he", rngAmmo);
                            if (mglAmmo != null && !mglAmmo.MoveToContainer(container.inventory)) RemoveFromWorld(mglAmmo);
                        }

                    }


                    var isHeliCrate = container.prefabID == 1314849795;

                    if (isHeliCrate || isShipHackCrate)
                    {

                        if (SaveSpan.TotalDays >= 2 && UnityEngine.Random.Range(0, 101) <= 40)
                        {
                            var charmItem = Luck?.Call<Item>("CreateRandomCharm") ?? null;
                            if (charmItem == null)
                            {
                                PrintWarning("Luck CreateRandomCharm() returned null item (heli/ship)!");
                            }
                            else
                            {
                                if (!charmItem.MoveToContainer(container.inventory)) RemoveFromWorld(charmItem);
                                else PrintWarning("Compilation successfully called CreateRandomCharm from Luck and moved charm into heli/ship container @ " + container?.transform?.position);
                            }
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 25)
                        {
                            var rngBattery = UnityEngine.Random.Range((isHeliCrate ? 100 : 40), (isHeliCrate ? 500 : 200));

                            var batteries = ItemManager.CreateByName("battery.small", rngBattery);
                            if (!batteries.MoveToContainer(container.inventory)) RemoveFromWorld(batteries);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= (isHeliCrate ? 17 : 8))
                        {
                            var rngAmount = UnityEngine.Random.Range(1, (isHeliCrate ? 4 : 3));
                            var signalItem = ItemManager.CreateByName("supply.signal", rngAmount);
                            if (signalItem != null && !signalItem.MoveToContainer(container.inventory)) RemoveFromWorld(signalItem);
                        }

                        if (UnityEngine.Random.Range(0, 101) <= 30)
                        {
                            var rngAmount = UnityEngine.Random.Range(1, 4);

                            if (_teaItems == null)
                            {
                                _teaItems = new List<ItemDefinition>();

                                for (int i = 0; i < ItemManager.itemList.Count; i++)
                                {
                                    var item = ItemManager.itemList[i];
                                    if (item.shortname.Contains("tea"))
                                        _teaItems.Add(item);

                                }
                            }

                            var rngTea = _teaItems[UnityEngine.Random.Range(0, _teaItems.Count)];
                            var tea = ItemManager.CreateByName(rngTea.shortname, rngAmount);
                            if (tea != null && !tea.MoveToContainer(container.inventory)) RemoveFromWorld(tea);

                        }
                    }

                    if (!isHard)
                    {

                        if (isHeliCrate || container.prefabID == 1737870479 || isShipHackCrate) //heli_crate, bradley_crate, ship crate
                        {
                            var rngNeed = container.ShortPrefabName.Contains("heli") ? 7 : 2;
                            if (UnityEngine.Random.Range(0, 101) <= rngNeed)
                            {
                                var suit = ItemManager.CreateByName("hazmatsuit_scientist");
                                if (suit != null && !suit.MoveToContainer(container.inventory))
                                {
                                    PrintWarning("No move suit!");
                                    RemoveFromWorld(suit);
                                }
                                else PrintWarning("Moved scientist hazmat suit to: " + container.ShortPrefabName + " @ " + pos);
                            }
                        }

                        if (isHeliCrate)
                        {
                            if (UnityEngine.Random.Range(0, 101) <= 75)
                            {
                                var rifleAmmo = ItemManager.CreateByName("ammo.rifle", UnityEngine.Random.Range(55, 108));
                                if (rifleAmmo != null && !rifleAmmo.MoveToContainer(container.inventory))
                                {
                                    PrintWarning("couldn't move rifleAmmo to heli_crate!");
                                    RemoveFromWorld(rifleAmmo);
                                }
                            }
                            if (UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(20f, 34))
                            {
                                var syringes = ItemManager.CreateByName("syringe.medical", UnityEngine.Random.Range(1, 6));
                                if (syringes != null && !syringes.MoveToContainer(container.inventory))
                                {
                                    PrintWarning("couldn't move syringes to heli_crate!");
                                    RemoveFromWorld(syringes);
                                }
                            }
                            if (UnityEngine.Random.Range(0, 101) <= 80)
                            {
                                var scrap = ItemManager.CreateByName("scrap", UnityEngine.Random.Range(85, 220));
                                if (scrap != null && !scrap.MoveToContainer(container.inventory))
                                {
                                    PrintWarning("couldn't move scrap to heli_crate!");
                                    RemoveFromWorld(scrap);
                                }
                            }

                            if (UnityEngine.Random.Range(0, 101) <= 55)
                            {

                                for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++)
                                {
                                    var rarity = UnityEngine.Random.Range(0, 101) <= 33 ? Rust.Rarity.VeryRare : Rust.Rarity.Rare;
                                    var catRNG = UnityEngine.Random.Range(0, 101);
                                    var category = catRNG <= 25 ? ItemCategory.Ammunition : catRNG <= 50 ? ItemCategory.Weapon : catRNG <= 75 ? ItemCategory.Attire : ItemCategory.Medical;
                                    var bp = GetRandomBP(rarity, category);
                                    if (bp != null && !bp.MoveToContainer(container.inventory))
                                    {
                                        PrintWarning("couldn't move bp: " + bp?.blueprintTargetDef?.shortname + " to heli_crate");
                                        RemoveFromWorld(bp);
                                    }
                                }


                            }

                            if (UnityEngine.Random.Range(0, 101) <= 33)
                            {
                                var rngDieselAmount = UnityEngine.Random.Range(3, 16);

                                var diesel = ItemManager.CreateByItemID(1568388703, rngDieselAmount);
                                if (diesel != null && !diesel.MoveToContainer(container.inventory))
                                {
                                    PrintWarning("couldn't move diesel to heli_crate");
                                    RemoveFromWorld(diesel);
                                }
                            }

                        }
                    }



                    if (isEasy)
                    {
                        Item findOil = null;
                        Item findFuel = null;

                        var isRoadSign = (container?.ShortPrefabName ?? string.Empty).Contains("roadsign"); //i hate using strs, but SO many IDs

                        for (int i = 0; i < lootEnt.inventory.itemList.Count; i++)
                        {
                            var item = lootEnt.inventory.itemList[i];

                            if (findFuel == null && item?.info?.itemid == -946369541) findFuel = item;
                            else if (findOil == null && item?.info?.itemid == -321733511) findOil = item;
                            else if (item?.info?.category == ItemCategory.Component)
                            {
                                var rng = bpRng.Next(0, 101);
                                if (rng <= 15)
                                {
                                    var newAmt = bpRng.Next(1, 4);
                                    //      PrintWarning("Adding comp amt: " + newAmt + " (new total: " + (newAmt + item.amount) + "), after rng: " + rng + ", item: " + item.info.shortname + ", old amt: " + item.amount);
                                    item.amount += newAmt;
                                    item.MarkDirty();
                                }
                            }
                            else if (item?.info?.itemid == 1397052267 && !isHeliCrate)
                            {
                                var percRemove = (SaveSpan.TotalHours < ScaleHours(24)) ? (isEasy ? 60 : 90) : (SaveSpan.TotalHours < ScaleHours(48)) ? (isEasy ? 45 : 80) : (SaveSpan.TotalHours < ScaleHours(72)) ? (isEasy ? 35 : 55) : (SaveSpan.TotalHours < ScaleHours(96)) ? (isEasy ? 20 : 35) : (isEasy ? 10 : 20);
                                var sigRng = UnityEngine.Random.Range(0, 101);
                                if (percRemove <= sigRng)
                                {
                                    item.RemoveFromContainer();

                                    lootEnt.Invoke(() =>
                                    {
                                        if (item != null) RemoveFromWorld(item);
                                    }, 0.1f);

                                    var newScrap = ItemManager.CreateByName("scrap", UnityEngine.Random.Range(50, 90));
                                    PrintWarning("removed signal with perc: " + percRemove + ", new scrap: " + newScrap.amount);
                                    if (!newScrap.MoveToContainer(lootEnt.inventory)) RemoveFromWorld(newScrap);
                                }
                            }

                        }

                        if (!isHard)
                        {
                            if (findFuel != null)
                            {
                                findFuel.amount += UnityEngine.Random.Range(15, 61);
                                findFuel.MarkDirty();
                            }

                            if (findOil != null)
                            {
                                findOil.amount += UnityEngine.Random.Range(9, 25);
                                findOil.MarkDirty();
                            }



                            if (isHeliCrate || isShipHackCrate || (prefabName.Contains("crate") || ((prefabName.Contains("barrel") && UnityEngine.Random.Range(0, 101) <= 33))) && SaveSpan.TotalHours > 3)
                            {
                                var rng = UnityEngine.Random.Range(0, 101);
                                var isUnderWater = prefabName.Contains("underwater") || lootEnt.WaterFactor() > 0;
                                var rngNeed = isHeliCrate ? 95 : isShipHackCrate ? 85 : prefabName.Contains("elite") ? 36 : prefabName.Contains("underwater_adv") ? 35 : isUnderWater ? 28 : prefabName == "crate_normal" ? 26 : prefabName.Contains("crate") ? 6 : 3;
                                if (rng <= rngNeed)
                                {
                                    var nearRad = false;

                                    for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
                                    {
                                        var monPos = TerrainMeta.Path.Monuments[i]?.transform?.position ?? Vector3.zero;

                                        if (Vector3.Distance(new Vector3(monPos.x, pos.y, monPos.z), pos) <= 350)
                                        {
                                            nearRad = true;
                                            break;
                                        }
                                    }

                                    //var nearRad = TerrainMeta.Path.Monuments?.Any(p => Vector3.Distance(new Vector3(p.transform.position.x, pos.y, p.transform.position.z), pos) <= 320) ?? false;
                                    if (nearRad || isUnderWater || isShipHackCrate || isHeliCrate)
                                    {
                                        if (SaveSpan.TotalDays > 2 && UnityEngine.Random.Range(0, 101) <= 50)
                                        {
                                            //PrintWarning("meeting criteria to spawn basic gun");

                                            var gunRng = UnityEngine.Random.Range(0, 101);
                                            var gunToSpawn = gunRng <= 10 ? "smg.thompson" : gunRng <= 18 ? "pistol.python" : gunRng <= 33 ? "pistol.semiauto" : gunRng <= 66 ? "shotgun.pump" : gunRng <= 85 ? "pistol.revolver" : gunRng <= 95 ? "shotgun.double" : "pistol.nailgun";

                                            var gunItem = ItemManager.CreateByName(gunToSpawn);

                                            if (!gunItem.MoveToContainer(container.inventory))
                                            {
                                                PrintWarning("couldn't move rng gun item!! cont: " + container.ShortPrefabName + ", " + container.inventory.capacity + " <-- capacity, count: " + container.inventory.itemList.Count);
                                                RemoveFromWorld(gunItem);
                                            }
                                            else gunItem.conditionNormalized = UnityEngine.Random.Range(0.165f, 0.66f);

                                        }

                                        var rngCat = GetRandomCategory(ItemCategory.All, ItemCategory.Search, ItemCategory.Food, ItemCategory.Misc, ItemCategory.Common, ItemCategory.Favourite);

                                        var noRares = Pool.GetList<Rust.Rarity>();
                                        try
                                        {
                                            if (UnityEngine.Random.Range(0, 101) <= ((isHeliCrate || isShipHackCrate) ? 1 : 10)) noRares.Add(Rust.Rarity.Common);
                                            if (UnityEngine.Random.Range(0, 101) <= ((isHeliCrate || isShipHackCrate) ? 15 : 40)) noRares.Add(Rust.Rarity.Rare);
                                            if (UnityEngine.Random.Range(0, 101) <= ((isHeliCrate || isShipHackCrate) ? 30 : 65)) noRares.Add(Rust.Rarity.VeryRare);


                                            var rngItemCount = (isHeliCrate || isShipHackCrate) ? UnityEngine.Random.Range(2, 6) : 1;
                                            for (int i = 0; i < rngItemCount; i++)
                                            {
                                                var rngRarity = GetRandomRarityOrDefault(rngCat, true, noRares.ToArray());
                                                var rngItem = GetRandomItem(rngRarity, rngCat, true);

                                                if (rngItem != null)
                                                    rngItem.amount = Mathf.Clamp((int)(rngItem.MaxStackable() * UnityEngine.Random.Range(0.05f, 0.15f)), 1, UnityEngine.Random.Range(320, 1080));


                                                if (rngItem == null)
                                                {
                                                    PrintWarning("rng item was null, picking random from fallbacks. failed category was: " + rngCat + ", rngRarity: " + rngRarity + ", no rares was: " + string.Join(", ", noRares));
                                                    var rngStr = _rngFallbacks.GetRandom();
                                                    PrintWarning("fall back was: " + rngStr);
                                                    rngItem = ItemManager.CreateByName(rngStr, 1);

                                                }

                                                if (rngItem == null) PrintWarning("rngItem null for: " + rngRarity + ", " + rngCat);
                                                else if (!rngItem.MoveToContainer(container.inventory) && !rngItem.Drop(container.transform.position, Vector3.up * 50f))
                                                {
                                                    PrintWarning("no move OR drop: " + rngItem.info.shortname + " x" + rngItem.amount);
                                                    RemoveFromWorld(rngItem);
                                                }
                                            }
                                        }
                                        finally { Pool.FreeList(ref noRares); }
                                    }
                                }
                            }
                        }


                    }

                    if (!isHeliCrate)
                    {
                        ReplaceRiflebody(lootEnt, SaveSpan);
                        ReplaceC4(lootEnt, SaveSpan);
                        ReplaceHazmat(lootEnt);
                    }


                    if (!isEasy || UnityEngine.Random.Range(0, 101) <= 5) ReplaceHQM(lootEnt, SaveSpan);


                    if (!isHard && !isShipHackCrate && !isHeliCrate) //handled in RED plugins
                    {
                        var gunPerc = SaveSpan.TotalHours < ScaleHours(48) ? (isEasy ? 60 : 85f) : SaveSpan.TotalHours < ScaleHours(72) ? (isEasy ? 40f : 70f) : SaveSpan.TotalHours < ScaleHours(96) ? (isEasy ? 25 : 37.5f) : SaveSpan.TotalHours < ScaleHours(120) ? (isEasy ? 18 : 25f) : (isEasy ? 7f : 11f);

                        var satchPerc = SaveSpan.TotalHours < ScaleHours(72) ? (isEasy ? 70 : 85f) : SaveSpan.TotalHours < ScaleHours(96) ? (isEasy ? 50.5f : 67.5f) : SaveSpan.TotalHours < ScaleHours(120) ? (isEasy ? 30f : 42.5f) : SaveSpan.TotalHours < ScaleHours(144) ? (isEasy ? 15 : 25f) : (isEasy ? 7 : 14f);
                        ReplaceItem(ItemManager.FindItemDefinition("explosive.satchel"), CreateItemAmount("grenade.beancan"), container.inventory, satchPerc);
                        ReplaceItem(ItemManager.FindItemDefinition("smg.thompson"), CreateItemAmount("scrap", 20), container.inventory, gunPerc - 5);
                        ReplaceItem(ItemManager.FindItemDefinition("pistol.python"), CreateItemAmount("scrap", 12), container.inventory, gunPerc - 6);
                        ReplaceItem(ItemManager.FindItemDefinition("rifle.ak"), CreateItemAmount("scrap", 30), container.inventory, gunPerc + (isEasy ? 4.5f : 7f));
                        var rocketRep = Mathf.Clamp(satchPerc + UnityEngine.Random.Range(28f, 34f), 0f, 99.95f);
                        if (isEasy) rocketRep = Mathf.Clamp(rocketRep * (SaveSpan.TotalHours < 72 ? 0.985f : 0.9725f), 0f, 100f);
                        ReplaceItem(ItemManager.FindItemDefinition("rocket.launcher"), CreateItemAmount("scrap", UnityEngine.Random.Range(20, 90)), container.inventory, rocketRep, true);

                        ReplaceItem("ammo.rocket.mlrs", CreateItemAmount("scrap", 12), container.inventory, SaveSpan.TotalHours < 72 ? 95 : SaveSpan.TotalHours < 96 ? 85 : 75);
                        ReplaceItem("aiming.module.mlrs", CreateItemAmount("scrap", 7), container.inventory, SaveSpan.TotalHours < 72 ? 95 : 85);


                    }

                    if (isEasy && (isCrateTools || isHeliCrate || isShipHackCrate || supply != null))
                    {

                        var rngNeeded = isHeliCrate ? 30 : isShipHackCrate ? 15 : supply ? 25 : 2;

                        if (UnityEngine.Random.Range(0, 201) <= rngNeeded)
                        {
                            var isQuarry = UnityEngine.Random.Range(0, 101) <= 40;

                            var itemId = isQuarry ? 1052926200 : -1130709577; //quarry item id; oil pump jack

                            var miningItem = ItemManager.CreateByItemID(itemId);

                            if (miningItem != null && !miningItem.MoveToContainer(container.inventory) && !miningItem.Drop(container.GetDropPosition(), container.GetDropVelocity()))
                            {
                                PrintWarning("couldn't move! destroying quarry/jack");
                                RemoveFromWorld(miningItem);
                            }

                            var surveryCharges = ItemManager.CreateByItemID(1975934948, UnityEngine.Random.Range(5, 11)); //survey charge item id
                            if (!surveryCharges.MoveToContainer(container.inventory) && !surveryCharges.Drop(container.GetDropPosition(), container.GetDropVelocity()))
                                RemoveFromWorld(surveryCharges);

                        }
                    }


                }
                finally
                {
                    try
                    {
                        if (watch.ElapsedMilliseconds >= 4)
                        {
                            var sb = Pool.Get<StringBuilder>();
                            try { PrintWarning(sb.Clear().Append(nameof(OnLootSpawn)).Append(" took: ").Append(watch.ElapsedMilliseconds.ToString("0.##")).Append("ms").ToString()); }
                            finally { Pool.Free(ref sb); }
                        }
                    }
                    finally { Pool.Free(ref watch); }
                }

            });
        }

        private List<Item> CreateSantaItems(bool glowEyes = false)
        {
            var hat = ItemManager.CreateByName("santahat");
            var hoodie = ItemManager.CreateByName("hoodie");
            var pants = ItemManager.CreateByName("pants", 1, 10021);
            var boots = ItemManager.CreateByName("shoes.boots", 1, 10023);
            var gloves = ItemManager.CreateByName("burlap.gloves", 1, 949616124);
            var jacket = ItemManager.CreateByName("jacket", 1, 10010);
            var newList = new List<Item> { hat, hoodie, pants, boots, gloves, jacket };
            if (glowEyes) newList.Add(ItemManager.CreateByName("gloweyes"));
            return newList;
        }

        private List<Item> CreateElfItems(bool glowEyes = false)
        {
            var hat = ItemManager.CreateByName("mask.balaclava", 1, 807719156);
            var hoodie = ItemManager.CreateByName("tshirt.long", 1, 808300545);
            var pants = ItemManager.CreateByName("pants", 1, 10019);
            var boots = ItemManager.CreateByName("shoes.boots", 1, 10023);
            var gloves = ItemManager.CreateByName("burlap.gloves", 1, 949616124);
            var newList = new List<Item> { hat, hoodie, pants, boots, gloves };
            if (glowEyes) newList.Add(ItemManager.CreateByName("gloweyes"));
            return newList;
        }

        private List<Item> CreateElfItemsAlt(bool glowEyes = false)
        {
            var mask = ItemManager.CreateByName("mask.bandana", 1, 1185794212);
            var hat = UnityEngine.Random.Range(0, 101) <= 50 ? ItemManager.CreateByName("attire.reindeer.headband") : ItemManager.CreateByName("santahat");
            var hoodie = ItemManager.CreateByName("burlap.shirt", 1, 1229561297);
            var pants = ItemManager.CreateByName("pants", 1, 1229552157);
            var boots = ItemManager.CreateByName("shoes.boots", 1, 10023);
            var newList = new List<Item> { hat, hoodie, pants, boots, mask };
            if (glowEyes) newList.Add(ItemManager.CreateByName("gloweyes"));
            return newList;
        }

        private static readonly System.Random christmasRng = new();

        private Item GetRandomChristmasItem()
        {
            Item item = null;
            var rng = christmasRng.Next(0, 116);
            //   var rng = UnityEngine.Random.Range(0, 125);
            var rngPresent = christmasRng.Next(0, 101);
            //  var rngPresent = UnityEngine.Random.Range(0, 100);
            var present = string.Empty;
            if (rngPresent <= 10) present = "large";
            else if (rngPresent <= 20) present = "medium";
            else present = "small";
            var rngItems = ItemManager.itemList?.Where(p => p != null && p.shortname.Contains("xmas"))?.Select(p => p.shortname)?.ToList() ?? null;
            rngItems.Add("snowman");
            rngItems.Add("snowball");
            for (int i = 0; i < UnityEngine.Random.Range(5, 15); i++) rngItems.Add("coal");
            var rngItem = rngItems[christmasRng.Next(0, rngItems.Count)];
            item = ItemManager.CreateByName(rng < 5 ? "stocking.large" : rng < 16 ? "stocking.small" : (rng > 20 && rng < 25) ? "santahat" : (rng > 30 && rng < 40) ? "pookie.bear" : (rng <= 65 && rng >= 40) ? "candycaneclub" : (rng > 70 && rng < 73) ? "snowman" : (rng > 72 && rng < 76) ? "xmas.lightstring" : (rng > 80 && rng < 85) ? "xmasdoorwreath" : (rng >= 85 && rng < 90) ? "xmas.present." + present : rngItem);
            return item;
        }

      

        private Item GetRandomComponent(int amount = 1, bool enforceStackLimits = true)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var components = Pool.GetList<ItemDefinition>();

            try
            {

                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];

                    if (item.category == ItemCategory.Component && !_ignoreItems.Contains(item.itemid) && !item.shortname.Contains("vehicle.") && item.shortname != "glue" && item.shortname != "bleach" && item.shortname != "ducttape" && item.shortname != "sticks")
                        components.Add(item);
                }

                var itemInfo = components[UnityEngine.Random.Range(0, components.Count)];
                var component = ItemManager.Create(itemInfo, 1);
                component.amount = Mathf.Clamp(amount, 1, component.MaxStackable());

                return component;
            }
            finally { Pool.FreeList(ref components); }
        }

        private static readonly System.Random categoryRng = new();

        private ItemCategory GetRandomCategory() { return (ItemCategory)categoryRng.Next(0, 18); }

        private ItemCategory GetRandomCategory(params ItemCategory[] ignoreCategories)
        {
            var category = (ItemCategory)categoryRng.Next(0, 18);
            if (!ignoreCategories.Any(p => p == category)) return category;

            var max = 400;
            var count = 0;

            while (ignoreCategories.Any(p => p == category))
            {
                count++;
                if (count >= max) break;
                category = (ItemCategory)categoryRng.Next(0, 18);
            }

            return category;
        }

        private Item GetRandomItem(Rust.Rarity rarity, ItemCategory category = ItemCategory.Items, bool craftableOnly = true, bool researchableOnly = true)
        {
            Item item = null;

            var applicableItems = Pool.GetList<ItemDefinition>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var itemDefs = ItemManager.itemList[i];
                    if (itemDefs == null) continue;

                    if (_ignoreItems.Contains(itemDefs.itemid) || itemDefs.GetComponent<Rust.Modular.ItemModVehicleChassis>() != null)
                        continue;


                    applicableItems.Add(itemDefs);

                }

                var itemDef = applicableItems?.Where(p => p != null && p.rarity == rarity && p.category == category && (!craftableOnly || craftableOnly && (p?.Blueprint?.userCraftable ?? false)) && (!researchableOnly || (p?.Blueprint?.isResearchable ?? false) || (p?.Blueprint?.defaultBlueprint ?? false)))?.ToList()?.GetRandom() ?? null;
                if (itemDef != null) item = ItemManager.Create(itemDef, 1);

                return item;
            }
            finally { Pool.FreeList(ref applicableItems); }
        }

        private Item GetRandomBP(Rust.Rarity rarity, ItemCategory category = ItemCategory.Items)
        {
            var item = ItemManager.CreateByName("blueprintbase");
            item.blueprintTarget = ItemManager.itemList?.Where(p => p != null && p.rarity == rarity && p.category == category && !(p?.Blueprint?.defaultBlueprint ?? false) && (p?.Blueprint?.userCraftable ?? false) && (p?.Blueprint?.isResearchable ?? false))?.ToList()?.GetRandom()?.itemid ?? 0;
            return item.blueprintTarget == 0 ? null : item;
        }

        private static readonly System.Random bpRng = new();

        private Item GetUnlearnedBP(Rust.Rarity rarity, BasePlayer player, ItemCategory category = ItemCategory.Items)
        {
            if (player == null || player.blueprints == null) return null;

            var list = ItemManager.itemList?.Where(p => p.rarity == rarity && p.category == category && !(p?.Blueprint?.defaultBlueprint ?? false) && (p?.Blueprint?.isResearchable ?? false) && (p?.Blueprint?.userCraftable ?? false) && !(player?.blueprints?.HasUnlocked(p) ?? false)) ?? null;
            if (list == null || !list.Any())
            {
                //    PrintWarning("couldn't get rng bp at all!!!! we must remove item");
                //   RemoveFromWorld(item);
                return null;
            }

            var item = ItemManager.CreateByName("blueprintbase");
            item.blueprintTarget = 0;



            var listCount = list?.Count() ?? 0;
            item.blueprintTarget = list.ElementAtOrDefault(bpRng.Next(0, listCount))?.itemid ?? 0;


            if (item.blueprintTarget == 0)
            {
                PrintWarning("item.blueprint target 0!!!!");
                RemoveFromWorld(item);
                return null;
            }

            return item.blueprintTarget == 0 ? null : item;
        }

        private bool HasRarity(Rust.Rarity rarity, ItemCategory category, bool onlyResearchable = false, bool ignoreDefault = false)
        {
            for (int i = 0; i < ItemManager.itemList.Count; i++)
            {
                var item = ItemManager.itemList[i];
                if (item == null) continue;
                if (item.category == category && item.rarity == rarity)
                {
                    if (!onlyResearchable || (item?.Blueprint?.isResearchable ?? false)) if (!ignoreDefault || !(item?.Blueprint?.defaultBlueprint ?? false)) return true;
                }
            }
            return false;
        }

        private Rust.Rarity GetHighestRarity(ItemCategory category, bool onlyResearchable = false)
        {
            try
            {
                if (onlyResearchable && !(ItemManager.itemList?.Any(p => p != null && p.category == category && (p?.Blueprint?.isResearchable ?? false)) ?? false)) return Rust.Rarity.None;
                else if (!onlyResearchable) if (!(ItemManager.itemList?.Any(p => p != null && p.category == category) ?? false)) return Rust.Rarity.None;
                return (!onlyResearchable ? ItemManager.itemList?.Where(p => p.category == category) : ItemManager.itemList?.Where(p => p.category == category && (p?.Blueprint?.isResearchable ?? false) && !(p?.Blueprint?.defaultBlueprint ?? false)))?.Select(p => p.rarity)?.Max() ?? Rust.Rarity.None;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintError("^GetHighestRarity^");
                return Rust.Rarity.None;
            }
        }

        private static readonly System.Random rarityRng = new();

        private Rust.Rarity GetRandomRarity(ItemCategory category, bool onlyResearchable = false, params Rust.Rarity[] ignoreRarity)
        {
            var rarity = Rust.Rarity.None;
            var rarities = Pool.GetList<Rust.Rarity>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item == null) continue;

                    if (item.category == category)
                    {
                        if (rarities.Contains(item.rarity)) continue;
                        if (!onlyResearchable || ((item?.Blueprint?.isResearchable ?? false) && !(item?.Blueprint?.defaultBlueprint ?? false))) if (ignoreRarity == null || ignoreRarity.Length < 1 || !ignoreRarity.Any(p => p == item.rarity)) rarities.Add(item.rarity);
                    }
                }
                if (rarities != null && rarities.Count > 0)
                {
                    var rngRarity = rarities[rarityRng.Next(0, rarities.Count)];
                    //   PrintWarning("rng rarity: " + rngRarity + ", has: " + HasRarity(rngRarity, category, true, true));
                    rarity = rngRarity;
                }
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^GetRandomRarity^"); }
            Pool.FreeList(ref rarities);
            return rarity;
        }

        private Rust.Rarity GetRandomRarityOrDefault(ItemCategory category, bool onlyResearchable = false, params Rust.Rarity[] ignoreRarity)
        {
            var rarity = Rust.Rarity.None;
            var rarities = Pool.GetList<Rust.Rarity>();
            try
            {
                for (int i = 0; i < ItemManager.itemList.Count; i++)
                {
                    var item = ItemManager.itemList[i];
                    if (item == null) continue;

                    if (item.category == category)
                    {
                        if (rarities.Contains(item.rarity)) continue;
                        if (!onlyResearchable || ((item?.Blueprint?.isResearchable ?? false) && !(item?.Blueprint?.defaultBlueprint ?? false))) if (ignoreRarity == null || ignoreRarity.Length < 1 || !ignoreRarity.Any(p => p == item.rarity)) rarities.Add(item.rarity);
                    }
                }
                if (rarities.Count > 0)
                {
                    var rngRarity = rarities[rarityRng.Next(0, rarities.Count)];
                    //   PrintWarning("rng rarity: " + rngRarity + ", has: " + HasRarity(rngRarity, category, true, true));
                    rarity = rngRarity;
                }
                else
                {
                    var itemCats = Pool.GetList<ItemDefinition>();
                    for (int i = 0; i < ItemManager.itemList.Count; i++)
                    {
                        var item = ItemManager.itemList[i];
                        if (item.category == category) itemCats.Add(item);
                    }
                    if (itemCats.Count > 0)
                    {
                        rarity = itemCats?.Select(p => p.rarity)?.Min() ?? Rust.Rarity.None;
                    }

                    Pool.FreeList(ref itemCats);
                }

            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^GetRandomRarityOrDefault^"); }
            Pool.FreeList(ref rarities);
            return rarity;
        }


        private readonly List<string> Ores = new() { "assets/bundled/prefabs/autospawn/resource/ores/metal-ore.prefab", "assets/bundled/prefabs/autospawn/resource/ores/sulfur-ore.prefab", "assets/bundled/prefabs/autospawn/resource/ores/stone-ore.prefab" };
        private const string METAL_ORE_PREFAB = "assets/bundled/prefabs/autospawn/resource/ores/metal-ore.prefab";
        private const string STONE_ORE_PREFAB = "assets/bundled/prefabs/autospawn/resource/ores/stone-ore.prefab";
        private const string SULFUR_ORE_PREFAB = "assets/bundled/prefabs/autospawn/resource/ores/sulfur-ore.prefab";

        private object GetRandomNPCPosition(BaseNpc npc)
        {
            if (npc == null || npc.IsDestroyed) return null;
            var npcPos = npc.transform.position;
            var startPos = Vector3.zero + new Vector3(UnityEngine.Random.Range(0, 5), 50, UnityEngine.Random.Range(0, 5));
            var rngPos = SpreadVector3WorldOnly(startPos, UnityEngine.Random.Range(UnityEngine.Random.Range(380, 675), UnityEngine.Random.Range(700, 1200f)), 120);
            rngPos.y = npcPos.y + 80f;
            var getGround = GetGroundPosition(rngPos, 100);
            return getGround; //temp
                              //
                              //if (WaterLevel.GetWaterDepth(getGround) > 0) getGround = GetGroundPosition(SpreadVector3WorldOnly(startPos, UnityEngine.Random.Range(420f, 1440f), 80));
                              // return WaterLevel.GetWaterDepth(getGround) < 0.1 ? (object)getGround : null;
        }



        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null) return;
            var watch = Pool.Get<Stopwatch>();

            try
            {
                watch.Restart();

                var isEasy = IsEasyServer();
                var isHard = IsHardServer();

                if (isEasy && entity.prefabID == 1314849795) //heli crate
                {

                    if (UnityEngine.Random.Range(0, 101) <= 33)
                    {
                        var spawnPos = entity.transform.position;

                        spawnPos.y += 3;

                        for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++)
                        {
                            var rng = UnityEngine.Random.Range(0, 101);

                            var cratePrefab = rng <= 33 ? "assets/bundled/prefabs/radtown/underwater_labs/crate_ammunition.prefab" : rng <= 66 ? "assets/bundled/prefabs/radtown/underwater_labs/crate_fuel.prefab" : rng <= 75 ? "assets/bundled/prefabs/radtown/underwater_labs/crate_medical.prefab" : rng <= 85 ? "assets/bundled/prefabs/radtown/crate_elite.prefab" : "assets/prefabs/npc/m2bradley/bradley_crate.prefab";

                            var ammoCrate = GameManager.server.CreateEntity(cratePrefab, spawnPos, entity.transform.rotation) as BaseCombatEntity;


                            ammoCrate.syncPosition = true;


                            ammoCrate.Spawn();


                            var rigidbody = ammoCrate?.GetComponent<Rigidbody>();
                            if (rigidbody == null)
                            {

                                rigidbody = ammoCrate.gameObject.AddComponent<Rigidbody>();

                                var onUnitSphere = UnityEngine.Random.onUnitSphere;

                                rigidbody.useGravity = true;
                                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                                rigidbody.mass = 2f;
                                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                                rigidbody.velocity = Vector3.down + onUnitSphere * UnityEngine.Random.Range(1f, 3f);
                                rigidbody.angularVelocity = Vector3Ex.Range(-1.75f, 1.75f);
                                rigidbody.drag = (float)(0.5 * ((double)rigidbody.mass / 5.0));
                                rigidbody.angularDrag = (float)(0.200000002980232 * ((double)rigidbody.mass / 5.0));
                            }


                            ammoCrate.InvokeRepeating(() =>
                            {
                                ammoCrate.NetworkPositionTick();
                            }, 0.05f, 0.05f);
                        }


                    }

                }

                if (!isHard)
                {
                    var train = entity as TrainEngine;
                    if (train != null)
                        train.maxFuelPerSec = 0.175f;


                    var scrapHeli = entity as ScrapTransportHelicopter;
                    if (scrapHeli != null)
                    {
                        scrapHeli.fuelPerSec *= 1.9f;
                        //changed in update. idk what the new variable is
                        //scrapHeli.//.maxRotorSpeed = 5000;
                    }

                    var boat = entity as MotorRowboat;
                    if (boat != null) boat.fuelPerSec *= 1.75f;

                    var rhib = entity as RHIB;
                    if (rhib != null) rhib.fuelPerSec *= 1.6f;

                    var hab = entity as HotAirBalloon;
                    if (hab != null) hab.fuelPerSec *= 1.2f;

                    var Minicopter = entity as Minicopter;
                    if (Minicopter != null)
                    {
                        Minicopter.fuelPerSec *= 1.4f;

                        if (!init)
                            Minicopter.SetHealth(Minicopter.MaxHealth() * 0.33f);
                    }
                }


                var dropItem = entity as DroppedItemContainer;
                if (dropItem?.inventory != null && (dropItem.prefabID == 545786656 || dropItem.prefabID == 1519640547)) //item_drop, item_drop_backpack
                {
                    dropItem.inventory.capacity = Mathf.Clamp(36, dropItem.inventory.capacity, 9999);
                }


                var fireball = entity as FireBall;
                if (fireball != null && !fireball.IsDestroyed && (fireball?.GetParentEntity() == null))
                {
                    fireball.lifeTimeMin = UnityEngine.Random.Range(7, 17);
                    fireball.lifeTimeMax = UnityEngine.Random.Range(20, 48);
                    fireball.Invoke(() =>
                    {
                        if (!fireball.IsDestroyed) fireball.Kill();
                    }, UnityEngine.Random.Range(fireball.lifeTimeMin, fireball.lifeTimeMax));
                }


                var npc = entity as BaseNpc;

                //extra ore spawning

                if (serverType == ServerType.x10)
                {
                    var oreEnt = entity as OreResourceEntity;

                    if (oreEnt != null && oreEnt.OwnerID != 1337)// && (!oreEnt.ShortPrefabName.Contains("ore_") || UnityEngine.Random.Range(0, 101) <= 10) && randomSpawn.Next(0, 101) <= UnityEngine.Random.Range(5, maxPerc))
                    {
                        var pCount = BasePlayer.activePlayerList?.Count ?? 0;
                        var maxPerc = pCount > 70 ? 27 : pCount > 50 ? 21 : pCount > 30 ? 18 : pCount > 25 ? 15 : 10;

                        if ((!oreEnt.ShortPrefabName.Contains("ore_") || UnityEngine.Random.Range(0, 101) <= 10) && randomSpawn.Next(0, 101) <= UnityEngine.Random.Range(5, maxPerc))
                        {
                            var rngPos = SpreadVector3WorldOnly(entity.transform.position, UnityEngine.Random.Range(33f, 66f), 45);
                            if (rngPos != Vector3.zero)
                            {
                                rngPos.y = entity.transform.position.y + (oreEnt.ShortPrefabName.Contains("ore_") ? 125f : 75f);
                                var getGround = GetGroundPosition(rngPos, 100);
                                /*/   if (WaterLevel.GetWaterDepth(getGround) > 0.2)
                                   {
                                       getGround = GetGroundPosition(SpreadVector3WorldOnly(rngPos, UnityEngine.Random.Range(66f, 99f)), 100);
                                   }/*/

                                var rng = UnityEngine.Random.Range(0, 101);
                                var usePrefab = rng <= 20 ? entity.PrefabName : rng <= 80 ? METAL_ORE_PREFAB : STONE_ORE_PREFAB;// : (rng <= 20 ? entity.PrefabName : StoneOrePrefab);
                                var newEnt = (OreResourceEntity)GameManager.server.CreateEntity(usePrefab, getGround, entity.transform.rotation);
                                if (newEnt != null)
                                {
                                    newEnt.OwnerID = 1337;
                                    newEnt.Spawn();
                                }
                                /*/
                                if (WaterLevel.GetWaterDepth(getGround) <= 0.2)
                                {
                                  
                                }/*/ //TEMP WATER
                            }
                        }
                    }
                }


                //  var oreEnt = entity as OreResourceEntity;
                //  if (oreEnt != null && UnityEngine.Random.Range(0, 101) <= 95) oreEnt.Kill();

                //extra npc spawning
                /*/
                if (npc != null && npc.prefabID != TEST_RIDABLE_HORSE_PREFAB_ID) //testridablehorse
                {
                    var oldPos = npc.transform.position;
                    var getPos = GetRandomNPCPosition(npc);
                    if (getPos != null)
                    {
                        npc.transform.position = (Vector3)getPos;
                        npc.SendNetworkUpdate();
                    }

                    if (npc.OwnerID != 1337 && UnityEngine.Random.Range(0, 101) <= UnityEngine.Random.Range(8, 17))
                    {
                        var newNpc = GameManager.server.CreateEntity(npc.PrefabName);
                        if (newNpc != null)
                        {
                            newNpc.OwnerID = 1337;
                            newNpc.Spawn();
                        }h
                    }
                }/*/

                var murderer = entity as ScarecrowNPC;

                if (murderer != null && XMas.enabled)
                {

                    var wearItems = murderer?.inventory?.containerWear?.itemList ?? null;
                    if (wearItems != null && wearItems.Count > 0)
                    {
                        RemoveFromWorld(wearItems[0]);
                        var rng = UnityEngine.Random.Range(0, 101);
                        if (rng <= 5)
                        {
                            var newHat = ItemManager.CreateByName("hat.dragonmask");
                            if (!newHat.MoveToContainer(murderer.inventory.containerWear)) RemoveFromWorld(newHat);
                        }
                        else if (rng <= 25)
                        {
                            var newHat = ItemManager.CreateByName("scarecrowhead");
                            if (!newHat.MoveToContainer(murderer.inventory.containerWear)) RemoveFromWorld(newHat);
                        }

                        var newItems = rng <= 33 ? CreateSantaItems(true) : rng <= 66 ? CreateElfItems(true) : CreateElfItemsAlt(true);
                        if (newItems.Count > murderer.inventory.containerWear.capacity) murderer.inventory.containerWear.capacity = newItems.Count;
                        for (int i = 0; i < newItems.Count; i++)
                        {
                            var item = newItems[i];
                            if (item != null && !item.MoveToContainer(murderer.inventory.containerWear)) RemoveFromWorld(item);
                        }
                    }
                } //this is actually christmas stuff?!?!?


                /*/
                var droppedContainer = entity as DroppedItemContainer;
                if (droppedContainer != null && !droppedContainer.IsDestroyed && droppedContainer.ShortPrefabName == "item_drop_backpack")
                {
                    var steamId = droppedContainer?.playerSteamID.ToString() ?? string.Empty;
                    var ply = (string.IsNullOrEmpty(steamId) || !steamId.StartsWith("7656")) ? null : ((object)covalence.Players.FindPlayerById(droppedContainer.playerSteamID.ToString()) ?? FindPlayerByID(steamId));
                    if (ply == null)
                    {
                        InvokeHandler.Invoke(droppedContainer, () =>
                        {
                            if (droppedContainer == null || droppedContainer.IsDestroyed || droppedContainer.gameObject == null) return;
                            PrintWarning("Would kill dropped item container at: " + droppedContainer?.transform?.position + ", steamId: " + steamId + ", ply is equal to null!");
                        }, 30f);

                    }
                }/*/


                //CHRISTMAS STUFF

                //is this identical to above?
                if (murderer != null && XMas.enabled)
                {


                    var wearItems = murderer?.inventory?.containerWear?.itemList?.ToList() ?? null;
                    if (wearItems != null && wearItems.Count > 0)
                    {
                        PrintWarning("murderer spawn with wearItems " + wearItems.Count + " at: " + (entity?.transform?.position ?? Vector3.zero));
                        for (int i = 0; i < wearItems.Count; i++) RemoveFromWorld(wearItems[i]);
                        PrintWarning("removed all items from murderer wear items");
                    }

                    var rng = UnityEngine.Random.Range(0, 101);

                    var newItems = rng <= 33 ? CreateSantaItems(true) : rng <= 66 ? CreateElfItems(true) : CreateElfItemsAlt(true);
                    if (newItems.Count > murderer.inventory.containerWear.capacity) murderer.inventory.containerWear.capacity = newItems.Count;

                    for (int i = 0; i < newItems.Count; i++)
                    {
                        var item = newItems[i];
                        if (item != null && !item.MoveToContainer(murderer.inventory.containerWear)) RemoveFromWorld(item);
                    }
                }

                //CHRISTMAS STUFF

                var npcCorpse = entity as NPCPlayerCorpse;
                if (XMas.enabled && npcCorpse?.prefabID == MURDERER_CORPSE_PREFAB_ID)
                {
                    npcCorpse.SetLootableIn(0.01f);

                    npcCorpse.Invoke(() =>
                    {
                        if (npcCorpse?.containers != null && npcCorpse.containers.Length > 0)
                        {
                            var itemList = npcCorpse?.containers[0]?.itemList?.ToList() ?? null;
                            if (itemList != null && itemList.Count > 0) for (int i = 0; i < itemList.Count; i++) RemoveFromWorld(itemList[i]);

                            var amtItems = UnityEngine.Random.Range(2, 6);

                            for (int i = 0; i < amtItems; i++)
                            {
                                if (npcCorpse == null || npcCorpse?.containers == null || npcCorpse.containers.Length < 1)
                                {
                                    PrintWarning("corpse or containers were null during loop");
                                    break;
                                }

                                var item = GetRandomChristmasItem();
                                if (item != null && !item.MoveToContainer(npcCorpse.containers[0]))
                                {
                                    PrintWarning("failed to move item to corpse or null item!!");
                                    RemoveFromWorld(item);
                                }
                            }
                        }
                    }, 0.05f);

                }

                if (!init) return;
                var sciCorpse = entity as NPCPlayerCorpse;

                if (sciCorpse != null && !isHard)
                {
                    //PrintWarning("!isHard, npcPlayercorpse spawn!!");
                    sciCorpse.SetLootableIn(0.7f);

                    sciCorpse.Invoke(() =>
                    {
                        var isApplicableScientist = IsOnCargo(sciCorpse) || Vector3.Distance(sciCorpse?.transform?.position ?? Vector3.zero, LargeOilRig?.transform?.position ?? Vector3.zero) < 300 || Vector3.Distance(sciCorpse?.transform?.position ?? Vector3.zero, SmallOilRig?.transform?.position ?? Vector3.zero) < 300;
                        if (isApplicableScientist)
                        {
                            PrintWarning("applicable scientist death!: " + sciCorpse + " @ " + (sciCorpse?.transform?.position ?? Vector3.zero));
                            sciCorpse.Invoke(() =>
                            {
                                if (sciCorpse == null || sciCorpse.IsDestroyed || sciCorpse.gameObject == null || sciCorpse.containers == null || sciCorpse.containers.Length < 1)
                                {
                                    PrintWarning("sciCorpse is all screwed up on 1 second invoke!");
                                    return;
                                }
                                ItemContainer mainContainer = null;
                                for (int i = 0; i < sciCorpse.containers.Length; i++)
                                {
                                    var cont = sciCorpse?.containers[i] ?? null;
                                    if (cont?.itemList != null && cont.itemList.Count > 0 && cont.itemList.Count < cont.capacity)
                                    {
                                        mainContainer = cont;
                                        break;
                                    }
                                }
                                if (mainContainer == null)
                                {
                                    mainContainer = sciCorpse.containers.FirstOrDefault();
                                    PrintWarning("mainContainer was null, grabbing default!");
                                }
                                if (mainContainer == null)
                                {
                                    PrintWarning("mainContainer still null after FirstOrDefault!!");
                                    return;
                                }
                                var lootRng = UnityEngine.Random.Range(0, 101);
                                if (lootRng <= 25)
                                {
                                    var cat = GetRandomCategory(ItemCategory.All, ItemCategory.Common, ItemCategory.Food, ItemCategory.Electrical);
                                    Item rngItem = null;

                                    for (int i = 0; i < 100; i++)
                                    {
                                        if (rngItem != null) break;
                                        Rust.Rarity rarity;

                                        if (i < 10)
                                        {
                                            rarity = GetRandomRarityOrDefault(cat, false, Rust.Rarity.Common, Rust.Rarity.Uncommon);
                                        }
                                        else if (i < 25)
                                        {
                                            rarity = GetRandomRarityOrDefault(cat, false, Rust.Rarity.Common);
                                        }
                                        else if (i < 50)
                                        {
                                            rarity = GetRandomRarity(cat, false);
                                        }
                                        else
                                        {
                                            cat = GetRandomCategory(ItemCategory.All, ItemCategory.Food);
                                            rarity = GetRandomRarity(cat, false);
                                        }
                                        rngItem = GetRandomItem(rarity, cat, false);
                                    }

                                    if (rngItem != null)
                                    {
                                        rngItem.amount = (int)Mathf.Clamp(rngItem.MaxStackable() * UnityEngine.Random.Range(0.01f, 0.075f), 1, 400);

                                        PrintWarning("got high tier loot rngItem for scientist: " + rngItem.info.displayName.english + " x" + rngItem.amount);

                                        if (!rngItem.MoveToContainer(mainContainer))
                                        {
                                            PrintWarning("failed to move high tier rngItem to container!!");
                                            RemoveFromWorld(rngItem);
                                        }
                                    }
                                    else PrintWarning("null rngItem for lootRng high tier!");
                                }
                                else if (lootRng <= 33)
                                {
                                    var cat = GetRandomCategory(ItemCategory.All, ItemCategory.Common, ItemCategory.Food, ItemCategory.Electrical);
                                    var rngItem = GetRandomItem(GetRandomRarityOrDefault(cat, false, Rust.Rarity.Common, Rust.Rarity.Uncommon, Rust.Rarity.VeryRare), cat, false);
                                    if (rngItem != null)
                                    {

                                        rngItem.amount = (int)Mathf.Clamp(rngItem.MaxStackable() * UnityEngine.Random.Range(0.008f, 0.045f), 1, 300);

                                        PrintWarning("got mid tier loot rngItem for scientist: " + rngItem.info.displayName.english + " x" + rngItem.amount);
                                        if (!rngItem.MoveToContainer(mainContainer))
                                        {
                                            PrintWarning("failed to move mid tier rngItem to container!!");
                                            RemoveFromWorld(rngItem);
                                        }
                                    }
                                    else PrintWarning("null rngItem for lootRng mid tier!");
                                }
                                else
                                {

                                    var cat = GetRandomCategory();
                                    Item rngItem = null;

                                    for (int i = 0; i < 100; i++)
                                    {
                                        if (rngItem != null) break;
                                        if (i > 25) cat = GetRandomCategory();
                                        else if (i > 50) cat = GetRandomCategory(ItemCategory.All, ItemCategory.Favourite, ItemCategory.Search, ItemCategory.Resources);
                                        rngItem = GetRandomItem(GetRandomRarityOrDefault(cat, false, Rust.Rarity.None), cat, false, false);
                                    }
                                    if (rngItem == null)
                                    {
                                        PrintWarning("rngItem for low tier is null!!");
                                    }
                                    else
                                    {
                                        rngItem.amount = (int)Mathf.Clamp(rngItem.MaxStackable() * UnityEngine.Random.Range(0.004f, 0.025f), 1, 115);

                                        if (!rngItem.MoveToContainer(mainContainer))
                                        {
                                            PrintWarning("failed to move low tier rngItem to container!!");
                                            RemoveFromWorld(rngItem);
                                        }
                                    }

                                }

                                var scrapRng = UnityEngine.Random.Range(0, 101);
                                if (scrapRng <= 40)
                                {
                                    var scrapAmount = UnityEngine.Random.Range(UnityEngine.Random.Range(4, 21), UnityEngine.Random.Range(30, 110));
                                    var scrap = ItemManager.CreateByName("scrap", scrapAmount);
                                    if (scrap == null) PrintWarning("scrap item is null?!");
                                    else
                                    {
                                        if (!scrap.MoveToContainer(mainContainer)) RemoveFromWorld(scrap);
                                    }

                                }
                                var compRng = UnityEngine.Random.Range(0, 101);
                                if (compRng <= 45)
                                {
                                    var comp = GetRandomComponent(UnityEngine.Random.Range(2, 7));
                                    if (comp == null) PrintWarning("rng comp is null!!!");
                                    else
                                    {
                                        if (!comp.MoveToContainer(mainContainer)) RemoveFromWorld(comp);
                                    }
                                }
                                var militaryGunRng = UnityEngine.Random.Range(0, 101);
                                if (militaryGunRng <= 5)
                                {
                                    var rngType = UnityEngine.Random.Range(0, 101);
                                    var gun = rngType < 1 ? "lmg.m249" : rngType <= 3 ? "multiplegrenadelauncher" : rngType <= 5 ? "rifle.l96" : rngType <= 12 ? "rifle.lr300" : rngType <= 30 ? "rifle.m39" : rngType <= 42 ? "shotgun.spas12" : rngType <= 66 ? "smg.mp5" : "pistol.m92";
                                    PrintWarning("rngType: " + rngType + " is going to give large oil rig scientist military gun: " + gun);
                                    var gunItem = ItemManager.CreateByName(gun);

                                    if (gunItem == null) PrintWarning("gunItem is null when trying to create for: " + gun + " !!!");
                                    else
                                    {
                                        if (!gunItem.MoveToContainer(mainContainer)) RemoveFromWorld(gunItem);
                                    }
                                }
                                var hqmRng = UnityEngine.Random.Range(0, 101);
                                if (hqmRng <= UnityEngine.Random.Range(22, 34))
                                {
                                    var hqm = ItemManager.CreateByName("metal.refined", UnityEngine.Random.Range(10, 92));
                                    if (hqm == null) PrintWarning("hqm is null!!");
                                    else
                                    {
                                        if (!hqm.MoveToContainer(mainContainer)) RemoveFromWorld(hqm);
                                    }

                                }
                                var bpRng = UnityEngine.Random.Range(0, 101);
                                if (bpRng <= 20)
                                {
                                    var rarityRNG = UnityEngine.Random.Range(0, 101);
                                    var rarity = rarityRNG <= 12 ? Rust.Rarity.VeryRare : rarityRNG <= 33 ? Rust.Rarity.Rare : rarityRNG <= 66 ? Rust.Rarity.Uncommon : Rust.Rarity.Common;
                                    var catRNG = UnityEngine.Random.Range(0, 101);
                                    var category = catRNG <= 25 ? ItemCategory.Ammunition : catRNG <= 50 ? ItemCategory.Weapon : catRNG <= 75 ? ItemCategory.Attire : ItemCategory.Medical;
                                    var bp = GetRandomBP(rarity, category);
                                    if (bp == null) PrintWarning("GetRandomBP null on scientist corpse!!");
                                    else
                                    {
                                        if (!bp.MoveToContainer(mainContainer)) RemoveFromWorld(bp);
                                    }
                                }
                                var charmRng = UnityEngine.Random.Range(0, 101);
                                if (SaveSpan.TotalDays > 2 && charmRng <= 15)
                                {
                                    var charmItem = Luck?.Call<Item>("CreateRandomCharm") ?? null;
                                    if (charmItem == null)
                                    {
                                        PrintWarning("Luck CreateRandomCharm() returned null item!");
                                    }
                                    else
                                    {
                                        if (!charmItem.MoveToContainer(mainContainer)) RemoveFromWorld(charmItem);
                                        else PrintWarning("Compilation successfully called CreateRandomCharm from Luck and moved charm into container @ " + sciCorpse?.transform?.position);
                                    }
                                }
                            }, 0.66f);


                        }
                    }, 0.05f);


                }

                if (entity is PatrolHelicopter)
                {
                    var scaledHeli = (int)ScaleHours(isHard ? 256 : 72, 0.59f);
                    Puts("Scaled heli time: " + scaledHeli + " (256 or 72 if not hard server)");
                    if ((int)SaveSpan.TotalHours < scaledHeli)
                    {
                        PrintWarning("heli spawn before " + scaledHeli.ToString("N0") + " hour old save (currently:  " + SaveSpan.TotalHours.ToString("0.00").Replace(".00", string.Empty) + ")!");
                        if (!entity.IsDestroyed) entity.Kill();
                    }
                    else PrintWarning("SaveSpan.TotalHours (" + SaveSpan.TotalHours + ") >= scaledHeli (" + scaledHeli + ")");
                }

                /*/
                if (entity is CH47HelicopterAIController)
                {
                    var scaledHeli = (int)ScaleHours(isHard ? 128 : 64, 0.59f);
                    Puts("Scaled chinook time: " + scaledHeli + " (128 or 64 if not hard server)");
                    if ((int)SaveSpan.TotalHours < scaledHeli)
                    {
                        PrintWarning("chinook spawn before " + scaledHeli.ToString("N0") + " hour old save (currently:  " + SaveSpan.TotalHours.ToString("0.00").Replace(".00", string.Empty) + ")!");
                        entity.Invoke(() =>
                        {
                            if (entity == null || entity.IsDestroyed) return;

                            entity.Kill();
                        }, 1f);
                    }
                    else PrintWarning("chinook SaveSpan.TotalHours (" + SaveSpan.TotalHours + ") >= scaledHeli (" + scaledHeli + ")");
                }/*/



                if (!isHard)
                {
                    var supply = entity as SupplyDrop;
                    if (supply != null)
                    {
                        entity.Invoke(() =>
                        {
                            AttachMarkerToDrop(supply);
                            Puts("Attached marker to supply drop at: " + (entity?.transform?.position ?? Vector3.zero));
                            SimpleBroadcast("<size=18><color=#78A835>A supply drop has been marked with a <color=yellow>yellow</color> marker on the map!</color></size>");
                        }, 480f);
                    }

                    var ch47 = entity as CH47HelicopterAIController;
                    if (ch47 != null && ch47.OwnerID == 0) SimpleBroadcast("<color=#2890cc>A Chinook-47 Helicopter has entered the map!</color>");
                }


                var storage = entity as StorageContainer;
                if (storage != null) storageContainers.Add(storage);

                if (npc != null) allAnimals.Add(npc);

                var cupboard = entity as BuildingPrivlidge;
                if (cupboard != null)
                {
                    var ownerID = cupboard.OwnerID;
                    var owner = FindPlayerByID(ownerID);
                    if (cupboard != null && owner != null && !TryGiveCupboardAccess(cupboard, owner)) PrintWarning("FAILED TO GIVE CUPBOARD ACCESS: " + (owner?.displayName ?? "Unknown"));
                }


                watch.Stop();
                if (watch.Elapsed.TotalMilliseconds > 1.7) PrintWarning("Took: " + watch.Elapsed.TotalMilliseconds + "ms for OnEntitySpawned for: " + entity.ShortPrefabName + " pos: " + (entity?.transform?.position ?? Vector3.zero));
            }
            finally { Pool.Free(ref watch); }
        }

    
        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null) return;
            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();

              
                var userIDStr = player?.UserIDString;

                var wasViolation = reason.Contains("violation", CompareOptions.OrdinalIgnoreCase);

                SendLeaveMessages(player, reason, false); //!(wasViolation || reason.Contains("invalid", CompareOptions.OrdinalIgnoreCase)));

                if (!reason.Equals("disconnected", StringComparison.OrdinalIgnoreCase) && (isVanished(player) || isGod(player)))
                    TeleportPlayer(player, Vector3.zero, false, false);
                

                if (wasViolation || reason.Contains("timed", CompareOptions.OrdinalIgnoreCase) || reason.Contains("ticket", CompareOptions.OrdinalIgnoreCase) || reason.Contains("flooding", CompareOptions.OrdinalIgnoreCase) || reason.Contains("packet", CompareOptions.OrdinalIgnoreCase))
                {
                    PrintWarning("player: " + player?.displayName + "/" + player?.UserIDString + " was just kicked for reason: " + reason + ", so we will now hide their sleeper temporarily");

                    var nearPlayers = new List<BasePlayer>();
                    var time = 180f;


                    GrantImmunity(player, time);

                    Vis.Entities(player.transform.position, 250f, nearPlayers, 131072);

                    if (nearPlayers.Count > 0)
                    {
                        for (int i = 0; i < nearPlayers.Count; i++)
                        {
                            var ply = nearPlayers[i];
                            if (ply != null && !ply.IsAdmin && ply.IsConnected && ply?.UserIDString != userIDStr) DisappearEntity(player, ply);
                        }

                        player.Invoke(() =>
                        {
                            for (int i = 0; i < nearPlayers.Count; i++)
                            {
                                var ply = nearPlayers[i];
                                if (ply != null && !ply.IsAdmin && ply.IsConnected && ply?.UserIDString != userIDStr) AppearEntity(player, ply);
                            }
                        }, time);
                    }
                    else PrintWarning("no nearPlayers for disconnected player: " + player?.displayName + " (" + player?.UserIDString + ")");
                }




                lastHitMsg.Remove(player.userID);
                cachedPlayers.Remove(player.userID);
                damages.Remove(player.userID);
                hits.Remove(player.userID);
                bodyPartsHit.Remove(player.userID);
                weaponsUsed.Remove(player.userID);
            }
            finally
            {
                try { if (watch.ElapsedMilliseconds > 3) PrintWarning("OnPlayerDisconnected took: " + watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms"); }
                finally { Pool.Free(ref watch); }
            }
        }

        [ChatCommand("gluep")]
        private void cmdGluePlayer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1) return;
            var target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, "No player found!");
                return;
            }
            var count = 1;
            if (args.Length > 1 && !int.TryParse(args[1], out count))
            {
                SendReply(player, "Invalid: " + args[1]);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                DoGlueEffect(target);
            }
            SendReply(player, "Did glue effect for: " + target?.displayName + " x" + count.ToString("N0"));
        }

        [ChatCommand("blind")]
        private void cmdBlindPlayer(BasePlayer player, string command, string[] args)
        {
            if (!canExecute(player, "blind"))
            {
                SendReply(player, "You do not have permission to use this!");
                return;
            }
            var target = (args.Length > 0) ? FindPlayerByPartialName(args[0]) : player;
            if (target == null || !(target?.IsConnected ?? false))
            {
                SendReply(player, "Unable to find any online player with the name: " + args[0]);
                return;
            }
            RenderUIBlind(target);
            SendReply(player, "Blinded: " + target.displayName);
        }

        // private T Deserialise<T>(string json) => JsonConvert.DeserializeObject<T>(json);
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

        private void cmdPlayerList(IPlayer player, string command, string[] args)
        {
            if (!init || ServerMgr.Instance == null || !player.IsServer) return;

            var str = PlayerListString;
            if (string.IsNullOrEmpty(str)) PrintWarning("playerListString is null/empty!!");
            else SendReply(player, str);

        }

        private bool TryMoveItemToContainer(Item item, ItemContainer container, bool TryDropIfFailed = true)
        {
            if (item == null || container == null) return false;
            if (!item.MoveToContainer(container))
            {
                var pos = item?.parent?.entityOwner?.transform?.position ?? item?.parent?.playerOwner?.transform?.position ?? Vector3.zero;
                var velocity = item?.parent?.playerOwner?.GetDropVelocity() ?? item?.parent?.entityOwner?.GetDropVelocity() ?? Vector3.zero;
                var rotation = item?.parent?.entityOwner?.transform?.rotation ?? item?.parent?.playerOwner?.transform?.rotation ?? default;
                if (TryDropIfFailed && pos != Vector3.zero) item.Drop(pos, velocity, rotation);
                return false;
            }
            else return true;
        }


        [ChatCommand("unblind")]
        private void cmdUnBlindPlayer(BasePlayer player, string command, string[] args)
        {
            if (!canExecute(player, "blind")) return;
            if (args.Length <= 0)
            {
                CuiHelper.DestroyUi(player, "BlindUI");
                CuiHelper.DestroyUi(player, "BlindUIText");
                return;
            }
            BasePlayer target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, "Unable to find any online player with the name: " + args[0]);
                return;
            }
            CuiHelper.DestroyUi(target, "BlindUI");
            CuiHelper.DestroyUi(target, "BlindUIText");
            SendReply(player, "Un-blinded: " + target.displayName);
        }

        private void ImgCmd(string[] args, BasePlayer player = null, ConsoleSystem.Arg arg = null)
        {
            if (args.Length < 2) return;
            float.TryParse(args[1], out float time);
            if (time == 0f)
            {
                if (player != null) SendReply(player, "Incorrect Time arg: " + "\"" + time + "\"" + " supplied!");
                arg?.ReplyWith("Incorrect Time arg: " + "\"" + time + "\"" + " supplied!");
                return;
            }
            if (player != null)
            {

                RenderImage(player, args[0]);

                timer.Once(time, () =>
                {
                    if (player == null || !player.IsConnected) return;
                    CuiHelper.DestroyUi(player, "ImgUI");
                });
            }
            else
            {
                foreach (BasePlayer playerr in BasePlayer.activePlayerList)
                {
                    RenderImage(playerr, args[0]);

                    timer.Once(time, () =>
                    {
                        if (playerr == null || !playerr.IsConnected) return;
                        CuiHelper.DestroyUi(playerr, "ImgUI");
                    });
                }
            }

        }

        [ChatCommand("img")]
        private void cmdImgPlayer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 2 || args.Length >= 3)
            {
                SendReply(player, "Incorrect args!");
                return;
            }
            ImgCmd(args, player);

        }

        [ChatCommand("img2")]
        private void cmdImg2Player(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "Incorrect args!");
                return;
            }

            if (args[0].Equals("stop"))
            {
                CuiHelper.DestroyUi(player, "ImgUI");
                return;
            }

            SendReply(player, "Trying to use url: " + args[0]);

            var anchorMin = args.Length > 1 ? args[1] : "0 0";
            var anchorMax = args.Length > 2 ? args[2] : "1 1";

            RenderAnImage(player, args[0], anchorMin, anchorMax);

        }

        [ConsoleCommand("cs.test")]
        private void cmdClassSelectTest(ConsoleSystem.Arg arg)
        {
            if (arg.Connection == null) return;

            var player = arg?.Player();

            if (player == null || !player.IsAdmin) return;


            var args = arg.Args;

            if (args == null || args.Length < 1)
            {
                SendReply(player, "specify initial class int");
                return;
            }

            if (args[0].Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                CuiHelper.DestroyUi(player, "ImgUI");
                return;
            }

            ClassSelectUI(player, int.Parse(args[0]));

        }


        private readonly Dictionary<string, int> _lastSelectedClass = new();

        [ConsoleCommand("cs.next")]
        private void cmdClassNext(ConsoleSystem.Arg arg)
        {
            if (arg.Connection == null) return;

            var player = arg?.Player();

            if (player == null || !player.IsAdmin) return;

            if (!_lastSelectedClass.TryGetValue(player.UserIDString, out int last)) last = 0;

            var desired = last + 1;
            if (desired > 6)
                desired = 0;

            _lastSelectedClass[player.UserIDString] = desired;

            ClassSelectUI(player, desired);

        }

        [ConsoleCommand("cs.prev")]
        private void cmdClassPrev(ConsoleSystem.Arg arg)
        {
            if (arg.Connection == null) return;

            var player = arg?.Player();

            if (player == null || !player.IsAdmin) return;

            if (!_lastSelectedClass.TryGetValue(player.UserIDString, out int last)) last = 0;

            var desired = last - 1;
            if (desired < 0)
                desired = 0;

            _lastSelectedClass[player.UserIDString] = desired;

            ClassSelectUI(player, desired);

        }

        private void ClassSelectUI(BasePlayer player, int userClass)
        {


            var imageAnchorMin = "0.432 0.319";
            var imageAnchorMax = "0.549 0.736";


            var className = "";
            switch (userClass)
            {
                case 0:
                    className = "lumberjack";
                    break;
                case 1:
                    className = "miner";
                    break;
                case 2:
                    className = "nomad";
                    break;
                case 3:
                    className = "mercenary";
                    break;
                case 4:
                    className = "packrat";
                    break;
                case 5:
                    className = "farmer";
                    break;
                case 6:
                    className = "loner";
                    break;

            }

            var url = "https://cdn.prismrust.com/class_selects/" + className + ".png";

            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = true
            }, "Overlay", "ImgUI");

            GUISkinElement.Add(new CuiElement
            {
                Name = "ImgUIImg",
                Components =
                {
                    new CuiRawImageComponent
                    {
                     //   Color = config.ImageColor,
                        Url = url
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = imageAnchorMin,
                        AnchorMax = imageAnchorMax
                    }
                },
                Parent = GUISkin,

            });

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "cs.test stop",
                        Color = "0.75 0.33 0.25 1"
                    },
                Text =
                {
                    Text = "Close",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.432 0.25",
                    AnchorMax = "0.549 0.285"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "cs.test stop",
                        Color = "0.75 0.33 0.25 1"
                    },
                Text =
                {
                    Text = "Select",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.432 0.285",
                    AnchorMax = "0.549 0.319"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "cs.next",
                        Color = "0.75 0.33 0.25 1"
                    },
                Text =
                {
                    Text = "-->",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.549 0.319",
                    AnchorMax = "0.58 0.347"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "cs.prev",
                        Color = "0.75 0.33 0.25 1"
                    },
                Text =
                {
                    Text = "<--",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.401 0.319",
                    AnchorMax = "0.432 0.347"
                }
            }, GUISkin);


            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "cs.prev",
                        Color = "0.75 0.33 0.25 0"
                    },
                Text =
                {
                    Text = "<color=red>TEST PLS CLICK ME</color>",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.441 0.339",
                    AnchorMax = "0.472 0.367"
                }
            }, GUISkin);

            CuiHelper.DestroyUi(player, "ImgUI");


            CuiHelper.AddUi(player, GUISkinElement);

        }

        [ConsoleCommand("cap.fixwear")]
        private void cmdCapFixWear(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var ply = BasePlayer.activePlayerList[i];
                if (ply != null && ply?.inventory != null)
                {
                    ply.inventory.containerWear.capacity = 7;
                }
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var ply = BasePlayer.sleepingPlayerList[i];
                if (ply != null && ply?.inventory != null)
                {
                    ply.inventory.containerWear.capacity = 7;
                }
            }
            arg.ReplyWith("Fixed all wear capacity");
        }

        [ConsoleCommand("cap.fixall")]
        private void cmdCapFixall(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var ply = BasePlayer.activePlayerList[i];
                if (ply != null && ply?.inventory != null)
                {
                    if (ply.inventory.containerMain.capacity > 24) ply.inventory.containerMain.capacity = 24;
                    if (ply.inventory.containerWear.capacity > 7) ply.inventory.containerWear.capacity = 7;
                    if (ply.inventory.containerBelt.capacity > 6) ply.inventory.containerBelt.capacity = 6;
                }
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var ply = BasePlayer.sleepingPlayerList[i];
                if (ply != null && ply?.inventory != null)
                {
                    if (ply.inventory.containerMain.capacity > 24) ply.inventory.containerMain.capacity = 24;
                    if (ply.inventory.containerWear.capacity > 7) ply.inventory.containerWear.capacity = 7;
                    if (ply.inventory.containerBelt.capacity > 6) ply.inventory.containerBelt.capacity = 6;
                }
            }
            arg.ReplyWith("Fixed all capacity");
        }


        [ConsoleCommand("cov.count")]
        private void cmdCovCount(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            SendReply(arg, "Covalence players count: " + covalence.Players.All.Count().ToString("N0"));
        }

        [ConsoleCommand("entity.deleteby")]
        private void consoleDeleteBy(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin) return;
            var args = arg?.Args ?? null;
            if (args.Length < 1) return;
            var argID = args[0];
            if (!ulong.TryParse(argID, out ulong targetID))
            {
                arg.ReplyWith("Bad ID: " + targetID);
                return;
            }
            var findPlayer = covalence.Players?.FindPlayerById(targetID.ToString());
            if (findPlayer == null)
            {
                arg.ReplyWith("Bad player!");
                return;
            }
            var ents = BaseEntity.saveList.Where(p => (p?.OwnerID ?? 0) == targetID)?.ToList() ?? null;
            if (ents == null || ents.Count < 1)
            {
                arg.ReplyWith("No entities by this player!");
                return;
            }
            var total = 0;
            for (int i = 0; i < ents.Count; i++)
            {
                var ent = ents[i];
                if (ent == null) continue;
                total++;
                ent.Invoke(() =>
                {
                    if (!ent.IsDestroyed) ent.Kill();
                }, total * 0.02f);
            }
            arg.ReplyWith("Deleted " + ents.Count + " entities by " + (findPlayer?.Name ?? argID));
        }


        [ConsoleCommand("img.load")]
        private void consoleImg(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null) return;
            ImgCmd(arg.Args, null, arg);
        }

        [ConsoleCommand("sprite.load")]
        private void consoleSprite(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null) return;
            if (arg.Args.Length < 1) return;
            var player = arg?.Player() ?? null;
            if (player == null) return;
            RenderSprite(player, arg.Args[0]);
            arg.ReplyWith("Used sprite: " + arg.Args[0]);
        }

        private void RenderSprite(BasePlayer player, string sprite)
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(sprite)) return;
            CuiHelper.DestroyUi(player, "SpriteUI");

            var elements = new CuiElementContainer
            {
                new CuiElement
                {
                    Name = "SpriteUI",
                    Parent = "Overlay",
                    Components =
    {
        new CuiRawImageComponent
        {
            Sprite = sprite
        },
        new CuiRectTransformComponent
        {
            AnchorMin = "0.333 0.157",
            AnchorMax = "0.723 0.851"
        }
    }
                }
            };
            CuiHelper.AddUi(player, elements);
            PrintWarning("rendered sprite: " + sprite);
        }

        private void RenderImage(BasePlayer player, string url)
        {

            CuiHelper.DestroyUi(player, "ImgUI");

            var elements = new CuiElementContainer
            {
                new CuiElement
                {
                    Name = "ImgUI",
                    Parent = "Overlay",
                    Components =
    {
        new CuiRawImageComponent
        {
            Sprite = "assets/content/textures/generic/fulltransparent.tga",

        Url = url

        },
        new CuiRectTransformComponent
        {
            AnchorMin = "0.46 0.26",
            AnchorMax = "0.66 0.56"
        }
    }
                }
            };
            CuiHelper.AddUi(player, elements);

        }

        private void RenderAnImage(BasePlayer player, string url, string anchorMin = "0 0", string anchorMax = "1 1")
        {

            CuiHelper.DestroyUi(player, "ImgUI");

            var elements = new CuiElementContainer
            {
                new CuiElement
                {
                    Name = "ImgUI",
                    Parent = "Overlay",
                    Components =
    {
        new CuiRawImageComponent
        {
            Sprite = "assets/content/textures/generic/fulltransparent.tga",

        Url = url

        },
        new CuiRectTransformComponent
        {
            AnchorMin = anchorMin,
            AnchorMax = anchorMax
        }
    }
                }
            };
            CuiHelper.AddUi(player, elements);

        }

        private void RenderGlueUI(BasePlayer player, string color = "0.4 0.6 0.7 0.25")
        {
            if (player == null || !player.IsConnected) return;


            //   Puts("RENDERUI glue for player: " + player.displayName + ", color: " + color);
            //   var blind = "0 0 0 1";

            // PrintWarning("BLINDING: " + blind);

            var elements = new CuiElementContainer();
            var elements2 = new CuiElementContainer();
            var mainName = elements.Add(new CuiPanel
            {
                CursorEnabled = false,
                FadeOut = UnityEngine.Random.Range(0.1f, 0.2f),
                Image =
                {
                    Color = color,
                    FadeIn = UnityEngine.Random.Range(0.1f, 0.5f)
                },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "1 1"
                }
            }, "Hud", "GlueUI");
            var mainName2 = elements2.Add(new CuiPanel
            {
                CursorEnabled = false,
                FadeOut = UnityEngine.Random.Range(0.1f, 0.2f),
                Image =
                {
                    Color = color,
                    FadeIn = 0f
                },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "1 1"
                }
            }, "Hud", "GlueUI2");
            CuiHelper.AddUi(player, elements2);
            CuiHelper.DestroyUi(player, "GlueUI");
            CuiHelper.AddUi(player, elements);
            CuiHelper.DestroyUi(player, "GlueUI2");
        }

        private void RenderUIBlind(BasePlayer player, string textToUse = "CHEATING IS NOT TOLERATED")
        {
            if (player == null || !player.IsConnected) return;

            CuiHelper.DestroyUi(player, "BlindUI");
            Puts("RENDERUI blind for player: " + player.displayName);
            var blind = "0 0 0 1";

            PrintWarning("BLINDING: " + blind);

            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color =  blind,
                    FadeIn = 0.75f
                },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "1 1"
                }
            }, "Overlay", "BlindUI");
            if (!string.IsNullOrEmpty(textToUse))
            {
                var textAdd = elements.Add(new CuiLabel
                {
                    Text =
                {
                    Text = textToUse,
                   Color = "1 0 0 1",
                    FontSize = 32,
                    FadeIn = 0.75f,
                    Align = TextAnchor.MiddleCenter
                },
                    RectTransform =
                {
                    AnchorMin = "0 0.6",
                    AnchorMax = "1 0.8"
                }
                }, "Overlay", "BlindUIText");
            }

            CuiHelper.AddUi(player, elements);
        }

        private string getIPString(BasePlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            var uID = player?.UserIDString ?? "0";
            var IP = player?.net?.connection?.ipaddress ?? string.Empty;
            if (!string.IsNullOrEmpty(IP)) IP = IP.Split(':')[0];
            else if (string.IsNullOrEmpty(IP) && AAIPDatabase != null) IP = AAIPDatabase?.Call<string>("GetLastIP", uID) ?? string.Empty;
            return IP;
        }

        private string getIPString(IPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            var uID = player.Id;
            var IP = string.Empty;
            if (player?.IsConnected ?? false) IP = player?.Address ?? string.Empty;
            if (string.IsNullOrEmpty(IP) && AAIPDatabase != null && AAIPDatabase.IsLoaded) IP = AAIPDatabase?.Call<string>("GetLastIP", uID) ?? string.Empty;
            return IP;
        }

        private string getIPString(Connection connection)
        {
            var IP = connection?.ipaddress ?? string.Empty;
            if (!string.IsNullOrEmpty(IP)) IP = IP.Split(':')[0];
            return IP;
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

                str = (int)span.TotalDays + (!showHours && span.TotalHours >= 48 ? 1 : 0) + " day" + (span.TotalDays >= 1.5 ? "s" : "") + (showHours ? (" " + totalHoursWereGoingToShowToTheUserAsAString + " hour(s)") : "");
            }
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        [ConsoleCommand("item.findbyid")]
        private void consoleFindItemID(ConsoleSystem.Arg arg)
        {
            var args = arg.Args;
            var playerArg = arg?.Player() ?? null;
            if (playerArg != null && !playerArg.IsAdmin) return;
            if (args == null || args.Length < 1)
            {
                arg.ReplyWith("You must supply a name!");
                return;
            }


            if (!ulong.TryParse(arg.Args[0], out ulong netID))
            {
                arg.ReplyWith("Not an id: " + arg.Args[0]);
                return;
            }

            var findItem = FindItemByIDU(netID);
            arg.ReplyWith("Find item?: " + (findItem != null));
        }


        [ConsoleCommand("item.findoverstack")]
        private void consoleFindItemOverStack(ConsoleSystem.Arg arg)
        {
            var playerArg = arg?.Player() ?? null;
            if (playerArg != null && !playerArg.IsAdmin) return;


            var foundSB = new StringBuilder();
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                for (int j = 0; j < player.inventory.AllItems().Length; j++)
                {
                    var item = player.inventory.AllItems()[j];
                    var amount = item.amount;
                    var englishName = item?.info?.displayName?.english ?? string.Empty;
                    if (item.amount > item.MaxStackable()) foundSB.AppendLine("Found: " + englishName + " on player: " + (player?.displayName ?? "Unknown") + " amount: " + amount);
                }
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var player = BasePlayer.sleepingPlayerList[i];
                for (int j = 0; j < player.inventory.AllItems().Length; j++)
                {
                    var item = player.inventory.AllItems()[j];
                    var amount = item.amount;
                    var englishName = item?.info?.displayName?.english ?? string.Empty;
                    if (item.amount > item.MaxStackable()) foundSB.AppendLine("Found: " + englishName + " on player: " + (player?.displayName ?? "Unknown") + " amount: " + amount);
                }
            }
            foreach (var ent in BaseEntity.saveList)
            {
                if (ent == null || ent.IsDestroyed) continue;
                var storage = ent as StorageContainer;
                if (storage == null) continue;
                for (int i = 0; i < storage.inventory.itemList.Count; i++)
                {
                    var item = storage.inventory.itemList[i];
                    var name = item?.info?.shortname ?? string.Empty;
                    var englishName = item?.info?.displayName?.english ?? string.Empty;
                    var amount = item?.amount ?? 0;
                    var name2 = item?.name ?? string.Empty;
                    var entName = ent?.ShortPrefabName ?? "Unknown";
                    var entOwner = GetDisplayNameFromID(ent?.OwnerID.ToString() ?? "0");

                    if (item.amount > item.MaxStackable()) foundSB.AppendLine("Found: " + englishName + " on inventory/box: " + entName + " (pos: " + ent.transform.position + ") (Owner: " + entOwner + ") amount: " + amount);
                }
            }
            if (foundSB.Length > 0) arg.ReplyWith(foundSB.ToString().TrimEnd());
            else arg.ReplyWith("No items found over stack size");
        }


        [ConsoleCommand("item.findname")]
        private void consoleFindItemName(ConsoleSystem.Arg arg)
        {
            var args = arg.Args;
            var playerArg = arg?.Player() ?? null;
            if (playerArg != null && !playerArg.IsAdmin) return;
            if (args == null || args.Length < 1)
            {
                arg.ReplyWith("You must supply a name!");
                return;
            }
            var argStr = arg?.FullString ?? string.Empty;
            if (string.IsNullOrEmpty(argStr)) return;
            var foundSB = new StringBuilder();
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                for (int j = 0; j < player.inventory.AllItems().Length; j++)
                {
                    var item = player.inventory.AllItems()[j];
                    var name = item?.info?.shortname ?? string.Empty;
                    var englishName = item?.info?.displayName?.english ?? string.Empty;
                    var name2 = item?.name ?? string.Empty;
                    var amount = item?.amount ?? 0;
                    if (name.Contains(argStr) || argStr.Contains(name) || englishName.Contains(argStr) || argStr.Contains(englishName) || (!string.IsNullOrEmpty(name2) && name2.Contains(argStr)) || (!string.IsNullOrEmpty(name2) && argStr.Contains(name2))) foundSB.AppendLine("Found: " + englishName + " on player: " + (player?.displayName ?? "Unknown") + " amount: " + amount);
                }
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var player = BasePlayer.sleepingPlayerList[i];
                for (int j = 0; j < player.inventory.AllItems().Length; j++)
                {
                    var item = player.inventory.AllItems()[j];
                    var name = item?.info?.shortname ?? string.Empty;
                    var englishName = item?.info?.displayName?.english ?? string.Empty;
                    var name2 = item?.name ?? string.Empty;
                    var amount = item?.amount ?? 0;
                    if (name.Contains(argStr) || argStr.Contains(name) || englishName.Contains(argStr) || argStr.Contains(englishName) || (!string.IsNullOrEmpty(name2) && name2.Contains(argStr)) || (!string.IsNullOrEmpty(name2) && argStr.Contains(name2))) foundSB.AppendLine("Found: " + englishName + " on player: " + (player?.displayName ?? "Unknown") + " amount: " + amount);
                }
            }
            foreach (var ent in BaseEntity.saveList)
            {
                if (ent == null || ent.IsDestroyed) continue;
                var storage = ent as StorageContainer;
                if (storage == null) continue;
                for (int i = 0; i < storage.inventory.itemList.Count; i++)
                {
                    var item = storage.inventory.itemList[i];
                    var name = item?.info?.shortname ?? string.Empty;
                    var englishName = item?.info?.displayName?.english ?? string.Empty;
                    var amount = item?.amount ?? 0;
                    var name2 = item?.name ?? string.Empty;
                    var entName = ent?.ShortPrefabName ?? "Unknown";
                    var entOwner = GetDisplayNameFromID(ent?.OwnerID.ToString() ?? "0");
                    ;
                    if (name.Contains(argStr) || argStr.Contains(name) || englishName.Contains(argStr) || argStr.Contains(englishName) || (!string.IsNullOrEmpty(name2) && name2.Contains(argStr)) || (!string.IsNullOrEmpty(name2) && argStr.Contains(name2))) foundSB.AppendLine("Found: " + englishName + " on inventory/box: " + entName + " (pos: " + ent.transform.position + ") (Owner: " + entOwner + ") amount: " + amount);
                }
            }
            if (foundSB.Length > 0) arg.ReplyWith(foundSB.ToString().TrimEnd());
            else arg.ReplyWith("No items found containing name: " + argStr);
        }

        [ConsoleCommand("consay")]
        private void consoleConSay(ConsoleSystem.Arg arg)
        {
            var args = arg?.Args ?? null;
            //  if (arg.Connection == null) return;
            var player = arg?.Player() ?? null;
            if (player != null && !canExecute(player, "say"))
            {
                arg.ReplyWith("You do not have permission to use this.");
                return;
            }

            if (args == null || args.Length == 0)
            {
                arg.ReplyWith("You must specify a message.");
                return;
            }
            var saycmdSB = new StringBuilder();
            saycmdSB.Append("say :<color=#33ccff><size=19> ");
            for (int i = 0; i < args.Length; i++) saycmdSB.Append(args[i]).Append(" ");
            saycmdSB.Append("</size></color>");
            rust.RunServerCommand(saycmdSB.Append(" -- Console").ToString());
        }


        [ChatCommand("say")]
        private void cmdSayConsole(BasePlayer player, string command, string[] args)
        {
            if (!canExecute(player, "say")) return;
            if (args.Length == 0)
            {
                SendReply(player, "You must specify a message.");
                return;
            }
            var saycmdSB = new StringBuilder();
            var saycmdClean = new StringBuilder();
            saycmdSB.Append("say :<color=#33ccff><size=19> ");
            var puremsgSB = new StringBuilder();
            var cleanPutsSB = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                saycmdSB.Append(args[i] + " ");
                puremsgSB.Append(args[i] + " ");
            }
            saycmdSB.Append("</size></color>");
            cleanPutsSB.Append(saycmdSB);
            cleanPutsSB.Replace("say :", string.Empty);
            var cleanPuts = RemoveTags(cleanPutsSB.ToString()).TrimEnd(' ');
            var userName = string.Empty;
            string userColor;
            if (player.UserIDString == "76561198028248023")
            {
                userColor = "<color=#660066>";
                userName = userColor + "Shady</color>";
            }
            if (player.UserIDString == "76561198086981902")
            {
                userColor = "<color=#a51d20>";
                userName = userColor + "Anarchy</color>";
            }
            var printMsg = "<size=19>" + puremsgSB.ToString().TrimEnd(' ') + "</size>";
            if (!string.IsNullOrEmpty(userName)) rust.BroadcastChat(userName, printMsg, player.UserIDString);
            else rust.RunServerCommand(saycmdSB.ToString() + " -- Anonymous Moderator");

            Puts("User: " + player.displayName + " (" + player.UserIDString + ") " + "used /say command: " + "\"" + cleanPuts + "\"");
        }

        private readonly Dictionary<ulong, Timer> permaTimers = new();
        private readonly Dictionary<ulong, Vector3> forcedSpawnPos = new();

        private void OnPlayerRespawned(BasePlayer player)
        {

            if (player.secondsConnected < 300) GrantImmunity(player, 30f);

            var forcedPos = Vector3.zero;
            if (!forcedSpawnPos.TryGetValue(player.userID, out forcedPos)) return;
            TeleportPlayer(player, forcedPos, false, true);
            NextTick(() =>
            {
                if (player == null) return;
                player.EndSleeping();
            });
        }

        [ChatCommand("forcespawn")]
        private void cmdForceSpawn(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length <= 0)
            {
                SendReply(player, "You must specify a player.");
                return;
            }
            BasePlayer target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, "That player could not be found online!");
                return;
            }
            if (!forcedSpawnPos.ContainsKey(target.userID))
            {
                forcedSpawnPos.Add(target.userID, player.transform.position);
                SendReply(player, "This player is now forced to spawn at this location.");
            }
            else
            {
                forcedSpawnPos.Remove(target.userID);
                SendReply(player, "Removed forced spawn pos for player");
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

        private string GetTagColor(string userID, string permission)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(permission)) return string.Empty;
            if (tagData?.tagList != null && tagData.tagList.Count > 0)
            {
                for (int i = 0; i < tagData.tagList.Count; i++)
                {
                    var tag = tagData.tagList[i];
                    if (tag?.UserIDString == userID && string.Equals(permission, tag.permission, StringComparison.OrdinalIgnoreCase)) { return tag.colorOverride; }

                }
            }
            return string.Empty;
            //return tagData?.tagList?.Where(p => p.UserIDString == userID && string.Equals(permission, p.permission, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.colorOverride ?? string.Empty;
        }

        private bool HideTag(string userID, string permission)
        {
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(permission)) return false;
            if (tagData?.tagList != null && tagData.tagList.Count > 0)
            {
                for (int i = 0; i < tagData.tagList.Count; i++)
                {
                    var tag = tagData.tagList[i];
                    if (tag?.UserIDString == userID && string.Equals(permission, tag.permission, StringComparison.OrdinalIgnoreCase)) { return tag.hide; }
                }
            }
            return false;
            //return tagData?.tagList?.Where(p => p.UserIDString == userID && string.Equals(permission, p.permission, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.hide ?? false;
        }

        /*--------------------------------------------------------------//
		//		canExecute - check if the player has permission			//
		//--------------------------------------------------------------*/
        private bool canExecute(BasePlayer player, string perm)
        {
            if (player == null || string.IsNullOrEmpty(perm)) throw new ArgumentNullException();
            perm = "compilation." + perm;
            var authLevel = player?.net?.connection?.authLevel ?? 0;
            if (authLevel > 1) return true;



            if (!permission.UserHasPermission(player.UserIDString, perm))
            {
                PrintWarning(player.displayName + " (" + player.UserIDString + ") tried to use a command (perm: " + perm + "), but didn't have permission. Auth level: " + authLevel);
                return false;
            }

            return true;
        }


        [ConsoleCommand("csay")]
        private void consoleChatSay(ConsoleSystem.Arg arg)
        {
            if (arg?.Connection == null) return;

            var args = arg?.Args ?? null;
            if (args == null || args.Length < 0) return;

            var player = arg?.Player() ?? null;
            if (player == null) return;


            var sb = Pool.Get<StringBuilder>();
            try
            {
                sb.Clear().Append("chat.say \"");

                for (int i = 0; i < args.Length; i++) sb.Append(args[i]).Append(" ");

                sb.Length -= 1;

                player.SendConsoleCommand(sb.Append("\"").Replace("True\"", "\"").ToString());
            }
            finally { Pool.Free(ref sb); }

        }


        private readonly Dictionary<ulong, float> cachedPlayers = new();

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
            {
                var p = BasePlayer.sleepingPlayerList[i];
                if (p?.UserIDString == userID) return p?.displayName;
            }
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty; //BasePlayer.activePlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? BasePlayer.sleepingPlayerList?.Where(p => p != null && p.UserIDString == userID)?.FirstOrDefault()?.displayName ?? string.Empty;
        }

        private string GetDisplayNameFromID(ulong userID) { return GetDisplayNameFromID(userID.ToString()); }

        private ItemDefinition GetItemDefFromPrefabName(string shortprefabName)
        {
            if (string.IsNullOrEmpty(shortprefabName)) return null;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var adjName = sb.Clear().Append(shortprefabName).Replace("_deployed", string.Empty).Replace(".deployed", string.Empty).Replace("_", string.Empty).Replace(".entity", string.Empty).ToString();
                return ItemManager.FindItemDefinition(adjName);
            }
            finally { Pool.Free(ref sb); }
        }

        private ListDictionary<InvokeAction, float> _invokeList = null;

        private ListDictionary<InvokeAction, float> InvokeList
        {
            get
            {
                if (_invokeList == null && InvokeHandler.Instance != null) _invokeList = curList.GetValue(InvokeHandler.Instance) as ListDictionary<InvokeAction, float>;
                return _invokeList;
            }
        }

        private void CancelInvoke(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null).ToString() == targetName && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private void CancelInvoke(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName) || obj == null) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null) == obj && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private bool IsInvoking(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null) == obj) return true;
                }
            }
            return false;
        }
        private bool IsInvoking(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null).ToString() == targetName) return true;
                }
            }
            return false;
        }



        private IPlayer FindIPlayer(string nameOrIdOrIp, bool onlyConnected = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) return null;
            var p = covalence.Players.FindPlayer(nameOrIdOrIp);
            if (p != null) if ((!p.IsConnected && !onlyConnected) || p.IsConnected) return p;
            var connected = covalence.Players.Connected;
            List<IPlayer> players = new();
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
                    var name = offlinePlayer?.Name ?? string.Empty;
                    var ID = offlinePlayer?.Id ?? string.Empty;
                    if (name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || ID == nameOrIdOrIp || getIPString(offlinePlayer) == nameOrIdOrIp) players.Add(offlinePlayer);
                }
            }
            if (players.Count > 1 || players.Count < 1) return null;
            else return players[0];
        }

        private IEnumerable<IPlayer> FindIPlayers(string nameOrIdOrIp, bool ignoreOffline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) return null;
            var connected = covalence.Players.Connected;
            List<IPlayer> players = new();
            foreach (var player in connected)
            {
                var IP = player?.Address ?? getIPString(player) ?? string.Empty;
                var name = player?.Name ?? string.Empty;
                var ID = player?.Id ?? string.Empty;
                if (name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || IP == nameOrIdOrIp || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || ID == nameOrIdOrIp) players.Add(player);
            }
            if (!ignoreOffline)
            {
                foreach (var offlinePlayer in covalence.Players.All)
                {
                    if (offlinePlayer.IsConnected) continue;
                    var name = offlinePlayer?.Name ?? string.Empty;
                    var ID = offlinePlayer?.Id ?? string.Empty;
                    if (ID == nameOrIdOrIp || name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(name, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || getIPString(offlinePlayer) == nameOrIdOrIp) players.Add(offlinePlayer);
                }
            }
            return players;
        }

        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp))
                throw new ArgumentNullException(nameof(nameOrIdOrIp));

            var ply = covalence.Players.FindPlayerById(nameOrIdOrIp);
            if (ply != null) if ((!ply.IsConnected && tryFindOfflineIfNoOnline) || ply.IsConnected) return ply;

            IPlayer player = null;
            foreach (var p in covalence.Players.Connected)
            {

                if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Address == nameOrIdOrIp || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                {

                    if (player != null) return null;
                    else player = p;
                }
            }

            if (!tryFindOfflineIfNoOnline || player != null) return player;

            var players = Pool.GetList<string>();
            try
            {
                PlayersByDatabase?.Call("GetAllPlayerIDsNoAlloc", players);

                for (int i = 0; i < players.Count; i++)
                {
                    var userId = players[i];

                    var covPlayer = covalence.Players.FindPlayerById(userId);
                    if (covPlayer == null)
                    {
                        continue;
                    }

                    var pName = covPlayer?.Name ?? string.Empty;

                    var cleanName = CleanPlayerName(pName);
                    if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;

                    if (string.Equals(pName, nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || pName.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (player != null) return null;
                        player = covPlayer;
                        return player;
                    }

                }
            }
            finally { Pool.FreeList(ref players); }

            foreach (var p in covalence.Players.All)
            {
                if (p.Name.Equals(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) || p.Name.IndexOf(nameOrIdOrIp, StringComparison.OrdinalIgnoreCase) >= 0 || getIPString(p) == nameOrIdOrIp)
                {
                    if (player != null) return null;
                    else player = p;
                }
            }

            return player;
        }

        [ChatCommand("howl")]
        private void cmdHowl(BasePlayer player, string command, string[] args)
        {
            var now = UnityEngine.Time.realtimeSinceStartup;
            var didHaveWolfHat = (player?.inventory?.containerWear?.itemList ?? null)?.Any(p => (p?.info?.shortname ?? string.Empty) == "hat.wolf") ?? false;
            if (!didHaveWolfHat)
            {
                SendReply(player, "You must be wearing a Wolf Headdress to use this command!");
                return;
            }
            if (!player.IsAdmin)
            {
                if (cachedPlayers.TryGetValue(player.userID, out float lastTime))
                {
                    lastTime = UnityEngine.Time.realtimeSinceStartup - lastTime;
                    if (lastTime < 4.5f)
                    {
                        SendReply(player, "You must wait before using this again!");
                        return;
                    }
                }
                cachedPlayers[player.userID] = UnityEngine.Time.realtimeSinceStartup;
            }
            var effect = "assets/bundled/prefabs/fx/player/howl.prefab";

            Effect.server.Run(effect, player.transform.position, Vector3.zero, null, false);
        }

        private void WoundAssist(BasePlayer player)
        {
            if (player == null || !player.IsConnected || player.IsDead() || player.IsSleeping()) return;
            if (player?.IsWounded() ?? false) player.StopWounded();
        }
        [ChatCommand("getup")]
        private void cmdGetupSelf(BasePlayer player, string command, string[] args)
        {
            if (!canExecute(player, "getup")) return;
            WoundAssist(player);
            // lastWoundedTime.SetValue(player, UnityEngine.Time.realtimeSinceStartup - 60);
        }

        [ChatCommand("logcmds")]
        private void cmdLogCmds(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (!logCmds.Contains(player.userID)) logCmds.Add(player.userID);
            else logCmds.Remove(player.userID);
            SendReply(player, "Log CMDs: " + logCmds.Contains(player.userID));
        }

        private readonly Dictionary<string, float> lastRespawnTime = new();
        private readonly Dictionary<string, bool> lastLightState = new();

        private readonly List<ulong> logCmds = new();

        private object OnServerCommand(ConsoleSystem.Arg arg)
        {
            var IP = arg?.Connection?.ipaddress ?? string.Empty;
            if ((arg?.Player() ?? null) == null && !string.IsNullOrEmpty(IP)) PrintWarning("Command without player: " + IP + ", " + (arg?.cmd?.FullName ?? "Unknown"));
            if (arg == null || arg.Connection == null || arg.cmd == null || arg.cmd.Name == null || !init) return null;

            var player = arg?.Player() ?? null;
            var command = arg?.cmd?.FullName ?? string.Empty;
            var argsStr = arg?.FullString ?? string.Empty;
            var args = arg?.Args ?? null;

            if (player != null && logCmds.Contains(player.userID))
            {
                PrintWarning(command + " \"" + argsStr + "\"");
            }

            if (command.Contains("respawn") && !(player?.IsAdmin ?? false))
            {
                if (!lastRespawnTime.TryGetValue(player.UserIDString, out float lastRespawn) || (UnityEngine.Time.realtimeSinceStartup - lastRespawn) >= 7)
                {
                    lastRespawnTime[player.UserIDString] = UnityEngine.Time.realtimeSinceStartup;
                    return null;
                }
                else
                {
                    arg.ReplyWith("You're doing that too quickly!");
                    return false;
                }
            }
            if (string.IsNullOrEmpty(command) || player == null) return null;
            if (player != null && command.Contains("lighttoggle"))
            {
                var held = player?.GetHeldEntity() ?? null;
                if (held != null) NextTick(() =>
                {
                    if (player == null || held == null) return;
                    lastLightState[player.UserIDString] = held.LightsOn();
                });
            }
            if (player.IsAdmin)
            {
                if (command == "global.entid")
                {
                    if (args[0].ToLower() == "lookat" && args.Length > 1)
                    {
                        PrintWarning("LOOK AT!");
                        var ID = args[1];
                        var findEnt = BaseNetworkable.serverEntities.Find(new NetworkableId(ulong.Parse(ID)));
                        SendReply(arg, "ent: " + findEnt.ShortPrefabName + " (" + ID + ")");
                    }
                    // PrintWarning("cmd: " + command + " " + (args.Length > 0 ? string.Join(" ", args) : string.Empty));
                }
            }

            if (args != null && args.Length > 1 && command == "note.update")
            {

                var note = FindItemByID(args[0]);
                if (note == null) return null;
                var oldText = note.text;
                var obj = Interface.Oxide.CallHook("OnNoteUpdate", player, note);
                if (note.text == oldText || (obj != null && (obj is string)))
                {
                    NextTick(() =>
                    {
                        if (note == null) return;
                        note.text = obj != null ? obj.ToString() : note.text.Replace("$", "");
                    });
                }

            }

            return null;
        }

        private object OnPlayerCommand(BasePlayer player, string command, string[] args)
        {
            if (player == null) return null;



            var IP = player?.Connection?.ipaddress ?? string.Empty;
            var argsStr = string.Join(" ", args);
            var cmdWithArgs = command + " " + argsStr;

            if (cmdWithArgs.StartsWith("sethome", StringComparison.OrdinalIgnoreCase) || cmdWithArgs.IndexOf("home set", StringComparison.OrdinalIgnoreCase) >= 0 || cmdWithArgs.IndexOf("home add", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var pos = player?.transform?.position ?? Vector3.zero;
                var ents = NTeleportation?.Call<List<BuildingBlock>>("GetFoundation", pos) ?? null;
                if (ents != null && ents.Count > 0)
                {
                    var ownerID = ents?.Where(p => p.OwnerID != 0)?.FirstOrDefault()?.OwnerID ?? 0;
                    var buildID = ents?.Where(p => p.buildingID != 0)?.FirstOrDefault()?.buildingID ?? 0;
                    var fullEnts = new HashSet<BaseEntity>();
                    if (ownerID != 0)
                    {
                        foreach (var ent in BaseEntity.saveList)
                        {
                            if (ent == null) continue;
                            var block = ent as BuildingBlock;
                            if (block == null || block.grade == BuildingGrade.Enum.Twigs || block.buildingID != buildID) continue;
                            fullEnts.Add(ent);
                        }
                    }

                    var ownedCount = 0;
                    if (fullEnts != null && fullEnts.Count > 0)
                    {
                        foreach (var ent in fullEnts)
                        {
                            if (ent == null || ent.IsDestroyed) continue;
                            if (ent.OwnerID == 0) continue;
                            if (ent.OwnerID == player.userID)
                            {
                                ownedCount++;
                                continue;
                            }
                            var isFriend = Friends?.Call<bool>("HasFriend", ent.OwnerID, player.userID) ?? false;
                            if (isFriend) ownedCount++;
                        }
                        var ownedPerc = ownedCount * 100d / fullEnts.Count;
                        var doesOwn = ownedPerc > 50;
                        if (!doesOwn)
                        {
                            SendReply(player, "<color=orange>You or a <color=#8aff47>friend</color> (use <color=#2aaced>/friend</color>) must own the majority of this base before using a <color=#ff912b>home</color>.</color>");
                            return true;
                        }
                    }
                }
            }

            return null;
        }

        private void SimpleBroadcast(string msg, ulong userID = 0)
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
                if (userID > 0) player.SendConsoleCommand("chat.add", string.Empty, userID, msg);
                else SendReply(player, msg);
            }

        }

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

            var strSB = Pool.Get<StringBuilder>();
            try
            {
                strSB.Clear();

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

                return strSB.ToString();
            }
            finally { Pool.Free(ref strSB); }
        }

        /// <summary>
        /// Finds a player using their entire or partial name. Entire names take top priority & will be returned over a partial match.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        /// <param name="sleepers"></param>
        /// <returns></returns>
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false, bool checkDatabase = true)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            BasePlayer player = null;

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null) continue;

                var pName = p?.displayName ?? string.Empty;

                var cleanName = CleanPlayerName(pName);
                if (!string.IsNullOrEmpty(cleanName)) pName = cleanName;


                if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                {

                    if (player != null)
                        return null;
                    

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
                        if (player != null)
                            return null;
                        

                        player = p;
                        return player;
                    }
                }
            }

            var userIds = Pool.Get<HashSet<ulong>>();

            try 
            {
                if (checkDatabase)
                {
                    PlayersByDatabase?.Call("GetAllPlayerIDsNoAllocUH", userIds);
                    foreach (var userId in userIds)
                    {
                        var p = RelationshipManager.FindByID(userId);
                        if (p == null)
                            continue;

                        var pName = p?.displayName ?? string.Empty;

                        var cleanName = CleanPlayerName(pName);
                        if (!string.IsNullOrEmpty(cleanName))
                            pName = cleanName;

                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }

                    }
                }

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

                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                            matches.Add(p);

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
                                matches.Add(p);

                        }
                    }

                    if (checkDatabase)
                    {
                        foreach (var userId in userIds)
                        {
                            var p = RelationshipManager.FindByID(userId);
                            if (p == null)
                                continue;


                            var pName = p?.displayName ?? string.Empty;
                            var cleanName = CleanPlayerName(pName);

                            if (!string.IsNullOrEmpty(cleanName))
                                pName = cleanName;

                            if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                                matches.Add(p);

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

                    return player;
                }
                finally { Pool.FreeList(ref matches); }

            }
            finally { Pool.Free(ref userIds); }

          
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



    }
}
