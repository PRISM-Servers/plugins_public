using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("PACC", "Shady", "3.0.953", ResourceId = 0)]
    internal class PRISMAntiCheatCore : RustPlugin
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

        /*--------------------------------------------------------------------------------//
		//                        PRISM ANTI-CHEAT CORE (PACC)                           //
		// THE CORE OF A COLLECTION OF ANTI-CHEAT SYSTEMS DEVELOPED FOR PRISM'S RUST SERVERS //
		//-----------------------------------------------------------------------------------*/

        public static PRISMAntiCheatCore Instance = null;

        #region Fields

        #region Lists/HashSets
        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        public readonly HashSet<ulong> noMsg = new HashSet<ulong>();
        private readonly HashSet<IPlayer> SendViolations = new HashSet<IPlayer>();
        private readonly HashSet<string> frozenPlayers = new HashSet<string>();
        #endregion
        #region Integers
        public const int BanViolationThreshold = 30;
        #endregion
        #region Reflection
        private readonly FieldInfo curList = typeof(InvokeHandler).GetField("curList", BindingFlags.Instance | BindingFlags.NonPublic);
        #endregion
        #region Strings
        private const string ShowMessagePerm = nameof(PRISMAntiCheatCore) + ".showmsg";
        private const string IgnoreAllPerm = nameof(PRISMAntiCheatCore) + ".ignoreall";
        private const string ListWarningsPerm = nameof(PRISMAntiCheatCore) + ".listwarnings";
        #endregion
        #region DateTime
        private DateTime LastServerSave { get; set; }

        #endregion
        #region Booleans
        public bool IsPurging { get; set; } = false;

        private bool _isSaving = false;

        public bool IsServerSaving
        {
            get { return SaveRestore.IsSaving || _isSaving; }
            set { _isSaving = value; }
        }

        private Coroutine _autoBanCoroutine = null;
        #endregion
        #region Actions
        private Action CheckAllPoints = null;
        #endregion
        #region Plugin References
        [PluginReference] private readonly Plugin CGSAuth;
        [PluginReference] private readonly Plugin PositionLogger;
        [PluginReference] private readonly Plugin PRISM; //CIA glows in the dark
        #endregion
        #endregion
        #region Classes
        #region Class Instances
        private ViolationData violationData;
        #endregion
        private class ViolationLog
        {
            public float ViolationAmount = 0f;
            public int Ping = -1;
            [JsonRequired]
            private string _startPosition = string.Empty;
            [JsonRequired]
            private string _endPosition = string.Empty;
            [JsonIgnore]
            public Vector3 StartPosition
            {
                get { return !string.IsNullOrEmpty(_startPosition) ? GetVector3FromString(_startPosition) : Vector3.zero; }
                set { _startPosition = value.ToString(); }
            }
            [JsonIgnore]
            public Vector3 EndPosition
            {
                get { return !string.IsNullOrEmpty(_endPosition) ? GetVector3FromString(_endPosition) : Vector3.zero; }
                set { _endPosition = value.ToString(); }
            }
            [JsonIgnore]
            public Vector3 Position
            {
                get
                {
                    if (_endPosition == _startPosition) return StartPosition;
                    else return Vector3.zero;
                }
                set
                {
                    _startPosition = value.ToString();
                    _endPosition = value.ToString();
                }
            }

            [JsonIgnore]
            public float ViolationDistance { get { return Vector3.Distance(StartPosition, EndPosition); } }

            public string ViolationText = string.Empty;
            [JsonRequired]
            private string _dateTime = string.Empty;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string Weapon { get; set; }

            [JsonIgnore]
            public DateTime ViolationTime
            {
                get
                {
                    DateTime time;
                    if (!DateTime.TryParse(_dateTime, out time)) return DateTime.MinValue;
                    else return time;
                }
                set { _dateTime = value.ToString(); }
            }

            public ViolationLog()
            {
                ViolationTime = DateTime.UtcNow;
            }


            public ViolationType Type = ViolationType.None;
        }


        private class ViolationData
        {
            public Dictionary<string, List<ViolationLog>> Violations = new Dictionary<string, List<ViolationLog>>();
            public List<ViolationLog> GetViolations(string userID, ViolationType type = ViolationType.All)
            {
                if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
                List<ViolationLog> logs;
                if (!Violations.TryGetValue(userID, out logs)) return null;
                if (type != ViolationType.All)
                {
                    var newLogs = new List<ViolationLog>();
                    for (int i = 0; i < logs.Count; i++)
                    {
                        var log = logs[i];
                        if (log.Type == type) newLogs.Add(log);
                    }
                    return newLogs;
                }
                return logs;
            }
            public void GetViolationsNoAlloc(string userID, ref List<ViolationLog> list, ViolationType type = ViolationType.All)
            {
                List<ViolationLog> logs;
                if (!Violations.TryGetValue(userID, out logs)) return;

                for (int i = 0; i < logs.Count; i++)
                {
                    var log = logs[i];
                    if (type == ViolationType.All || log.Type == type) list.Add(log);
                }

            }
            public void AddViolation(string userID, ViolationLog violation)
            {
                if (string.IsNullOrEmpty(userID) || violation == null) return;
                List<ViolationLog> logs;
                if (!Violations.TryGetValue(userID, out logs)) Violations[userID] = new List<ViolationLog>();
                Violations[userID].Add(violation);
            }
            public ViolationData() { }
        }


        #endregion
        #region Config/Init


        public enum ViolationType { None, All, Jump, WeaponSpeed, LOS, Distance, Recoil, Macro, DebugCamera, ArrowSpeed, Key, Spider, Headshots, AimSprint }


        private void OnServerInitialized()
        {

            timer.Every(720f, () =>
            {
                if (ServerMgr.Instance != null && !IsPurging)
                {
                    //PrintWarning("Starting purge coroutine...");
                    ServerMgr.Instance.StartCoroutine(PurgeData());
                }
            });

            _autoBanCoroutine = ServerMgr.Instance.StartCoroutine(CheckAutoBans());

        }

        private void Init()
        {
            Instance = this;
            permission.RegisterPermission(ShowMessagePerm, this);
            permission.RegisterPermission(ListWarningsPerm, this);
            permission.RegisterPermission(IgnoreAllPerm, this);

            violationData = Interface.Oxide?.DataFileSystem?.ReadObject<ViolationData>("ViolationData") ?? new ViolationData();

            AddCovalenceCommand("lw", nameof(cmdListWarnings));
        }

        private void Unload()
        {
            try
            {
                if (ServerMgr.Instance != null)
                {
                    if (_autoBanCoroutine != null) ServerMgr.Instance.StopCoroutine(_autoBanCoroutine);
                    ServerMgr.Instance.CancelInvoke(CheckAllPoints);
                }

                SaveData();
            }
            finally { Instance = null; }
        }

        #endregion
        #region Console Commands
        [ConsoleCommand("lw.json")]
        private void consoleListWarnings(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;

            if (arg?.Args == null || arg.Args.Length < 1) return;

            var sb = Facepunch.Pool.Get<StringBuilder>();
            try 
            {
                var arg0 = arg.Args[0];

                ulong userId;
                if (!ulong.TryParse(arg0, out userId))
                {
                    arg.ReplyWith(sb.Clear().Append("Not a ulong: ").Append(arg0).ToString());
                    return;
                }

                var logs = violationData.GetViolations(userId.ToString(), ViolationType.All);
                arg.ReplyWith((logs == null || logs.Count < 1) ? sb.Clear().Append("No logs for: ").Append(userId).ToString() : JsonConvert.SerializeObject(logs, Formatting.Indented));
            }
            finally { Facepunch.Pool.Free(ref sb); }
        
        }
        #endregion
        #region Chat Commands
        [ChatCommand("taw")]
        private void cmdToggleWarning(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, ShowMessagePerm)) return;
            if (noMsg.Contains(player.userID)) noMsg.Remove(player.userID);
            else noMsg.Add(player.userID);
            SendReply(player, (noMsg.Contains(player.userID) ? "Disabled" : "Enabled") + " antihack messages");
        }

        [ChatCommand("taw2")]
        private void cmdToggleWarning2(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, ShowMessagePerm)) return;
            var iPly = covalence.Players?.FindPlayerById(player.UserIDString) ?? null;
            if (iPly == null)
            {
                SendReply(player, "No IPlayer!");
                return;
            }

            if (!SendViolations.Add(iPly)) SendViolations.Remove(iPly);

            SendReply(player, (SendViolations.Contains(iPly) ? "Enabled" : "disabled") + " rust antihack messages");
        }

        private void cmdListWarnings(IPlayer player, string command, string[] args)
        {
            if (!player.IsAdmin && !permission.UserHasPermission(player.Id, "antiwallhack.listwarnings")) return;
            if (args.Length < 1)
            {
                SendReply(player, "Please specify a target");
                return;
            }
            var target = FindConnectedPlayer(args[0], true);
            if (target == null)
            {
                SendReply(player, "No target found: " + args[0]);
                return;
            }
            var type = ViolationType.All;
            var tpArg = args.Length > 1 && args[1].Equals("tp", StringComparison.OrdinalIgnoreCase);
            if (args.Length > 1)
            {
                if (!tpArg)
                {
                    int typeInt;
                    if (!int.TryParse(args[1], out typeInt) || typeInt < (int)ViolationType.None || typeInt > (int)ViolationType.All)
                    {
                        SendReply(player, "Invalid type specified: " + args[1]);
                        return;
                    }
                    else type = (ViolationType)typeInt;
                }
            }
            var maxMinutes = -1d;
            if (args.Length > 2 && !tpArg)
            {
                if (!double.TryParse(args[2], out maxMinutes))
                {
                    SendReply(player, "Not a number: " + args[2]);
                    return;
                }
            }
            var logs = violationData.GetViolations(target.Id, type);
            if (logs == null || logs.Count < 1)
            {
                SendReply(player, "No logs of type " + type + " for: " + target.Name);
                return;
            }
            var plyObj = player?.Object as BasePlayer;
            var playerPos = plyObj?.transform?.position ?? Vector3.zero;
            if (tpArg)
            {
                if (plyObj == null || plyObj.IsDead())
                {
                    SendReply(player, "This must be ran as a player (and alive)");
                    return;
                }
                if (args.Length < 3)
                {
                    SendReply(player, "Please specify a warning to TP (overall number)");
                    return;
                }
                int tpIndex;
                if (!int.TryParse(args[2], out tpIndex))
                {
                    SendReply(player, "Not a number: " + args[2]);
                    return;
                }
                if ((logs.Count - 1) < tpIndex)
                {
                    SendReply(player, "Index is too high: " + tpIndex + ", list count is: " + logs.Count);
                    return;
                }
                var log = logs[tpIndex];
                if (log == null)
                {
                    SendReply(player, "Log is null!");
                    return;
                }
                var pos = log.Position != Vector3.zero ? log.Position : log.StartPosition != Vector3.zero ? log.StartPosition : log.EndPosition;
                if (pos == Vector3.zero)
                {
                    SendReply(player, "Log has bad position");
                    return;
                }
                SendReply(player, "Teleporting to: " + pos);
                TeleportPlayer(plyObj, pos);
                return;
            }
            var logSB = new StringBuilder();
            var logSBs = new List<StringBuilder>();
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log == null) continue;
                if (log.ViolationTime != DateTime.MinValue && maxMinutes != -1d && (DateTime.UtcNow - log.ViolationTime).TotalMinutes > maxMinutes) continue;
                var logMsg = (type == ViolationType.All ? ("Type: " + log.Type + "\n") : string.Empty) + "Text: " + log.ViolationText + "\nTime: " + log.ViolationTime;
                if (logSB.Length >= 650 || (logSB.Length + logMsg.Length) >= 650)
                {
                    logSBs.Add(logSB);
                    logSB = new StringBuilder();
                }
                logSB.AppendLine("<color=#ff912b>--------- #" + i + "</color>\n" + logMsg);
                if (log.ViolationAmount > 0.0f) logSB.AppendLine("Violation Amount: " + log.ViolationAmount.ToString("0.0"));
                var startStr = log.StartPosition.ToString();
                var endStr = log.EndPosition.ToString();
                if (log.StartPosition != Vector3.zero && startStr != endStr) logSB.AppendLine("Start Position: " + startStr);
                if (log.EndPosition != Vector3.zero && endStr != startStr) logSB.AppendLine("End Position: " + endStr);
                if (log.StartPosition != Vector3.zero && log.EndPosition != Vector3.zero && endStr == startStr) logSB.AppendLine("Position: " + startStr);
                if (log.Ping != -1) logSB.AppendLine("Ping: " + log.Ping.ToString("N0"));
                var dist = (log.Type == ViolationType.Jump && log.EndPosition.y >= log.StartPosition.y) ? (log.EndPosition.y - log.StartPosition.y) : log.ViolationDistance;

                if (plyObj != null && plyObj.IsConnected && !plyObj.IsDead())
                {
                    var PosDist = Vector3.Distance(playerPos, log.Position);
                    var endPosDist = Vector3.Distance(playerPos, log.EndPosition);
                    var startPosDist = Vector3.Distance(playerPos, log.StartPosition);

                    if (endPosDist <= 30 || startPosDist <= 30 || PosDist <= 30)
                    {
                        if (endPosDist <= 30 || startPosDist <= 30)
                        {
                            plyObj.SendConsoleCommand("ddraw.arrow", 10f, Color.yellow, log.StartPosition, log.EndPosition, 0.2f);

                            plyObj.SendConsoleCommand("ddraw.text", 10f, Color.magenta, log.EndPosition, "<size=20>" + log.Type + "</size>" + (dist > 0.0f ? " distance: <size=18>" + dist.ToString("0.0") + "</size>m" : string.Empty));
                        }

                        if (log.Type == ViolationType.Spider || log.Type == ViolationType.Jump)
                        {
                            var logger = PositionLogger;
                            if (logger != null && logger.IsLoaded)
                            {
                                //retarded identifier because userID is not saved to data file
                                var identifier = log.ViolationText + "_" + ((int)log.Type) + "_" + log.ViolationTime.ToString("s", CultureInfo.InvariantCulture);
                                List<Vector3> positions = logger.Call<List<Vector3>>("GetLog", identifier);

                                if (positions != null && positions.Count > 0)
                                {
                                    for (int p = 0; p < positions.Count - 1; p++)
                                    {
                                        plyObj.SendConsoleCommand("ddraw.arrow", 10f, Color.green, positions[p], positions[p + 1], 0.2f);
                                    }
                                }
                                else logSB.AppendLine("Detailed position data missing");
                            }
                            else logSB.AppendLine("Detailed position data missing");
                        }
                    }
                }

                if (dist > 0.0f) logSB.AppendLine("Distance: " + dist.ToString("0.00") + "m");

            }
            if (logSBs.Count < 1 && logSB.Length < 1)
            {
                SendReply(player, "No logs found");
                return;
            }
            if (logSBs.Count > 0)
            {
                for (int i = 0; i < logSBs.Count; i++)
                {
                    var sbStr = logSBs[i].ToString().TrimEnd();
                    SendReply(player, (player.IsServer || plyObj == null) ? RemoveTags(sbStr) : sbStr);
                }
            }

            if (logSB.Length > 0)
            {
                var sbStr = logSB.ToString().TrimEnd();
                SendReply(player, (player.IsServer || plyObj == null) ? RemoveTags(sbStr) : sbStr);
            }
        }
        #endregion
        #region Hooks
        private void OnServerSave()
        {
            IsServerSaving = true;
            ServerMgr.Instance.Invoke(() =>
            {
                IsServerSaving = false;
            }, 5f);
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null || string.IsNullOrEmpty(reason)) return;

            if (reason.Contains("FlyHack Violation"))
            {
                SendWarning("<color=yellow>" + player.displayName + "</color> just got kicked for FlyHack");
                RecordPlayer(player, "PACC_flyhack_kick_" + player.UserIDString + "_" + GetRecordingTime());
                AddViolation(player.UserIDString, (int)ViolationType.Jump, player.transform.position, "<color=yellow>" + player.displayName + "</color> just got kicked for FlyHack");
            }
        }

        private void OnPlayerViolation(BasePlayer player, AntiHackType type, float amount)
        {
            if (player == null || amount <= 0) return;

            if (type == AntiHackType.ProjectileHack)
            {
                if ((player.violationLevel + amount) >= ConVar.AntiHack.maxviolation)
                {
                    var sb = Facepunch.Pool.Get<StringBuilder>();
                    try 
                    {
                        var banReason = sb.Clear().Append("AH-PH").ToString();
                        var banReasonFull = sb.Clear().Append("AntiHack - Projectile Hack (").Append(amount.ToString("0.00").Replace(".00", string.Empty)).Append(")").ToString();

                        covalence.Server.Command(sb.Clear().Append("ban \"").Append(player.UserIDString).Append("\" \"").Append(banReason).Append("\"").ToString());

                        covalence.Server.Command(sb.Clear().Append("bannotes \"").Append(player.UserIDString).Append("\" \"").Append(banReasonFull).Append("\"").ToString());
                    }
                    finally { Facepunch.Pool.Free(ref sb); }
                 
                }
            }

            if (SendViolations == null || SendViolations.Count < 1) return;
            var msg = "<color=#ffcc80>" + player.displayName + "</color> <color=#ffad33>has triggered antihack.\nType: " + type + " (" + amount.ToString("0.00").Replace(".00", string.Empty) + "f, total:" + player.violationLevel.ToString("0.00").Replace(".00", string.Empty) + ")\nPosition:" + (player?.transform?.position ?? Vector3.zero) + "\nPing: " + GetPing(player) + " (" + player.desyncTimeRaw.ToString("0.00").Replace(".00", string.Empty) + ")</color>";
            foreach (var p in SendViolations)
            {
                if (p != null && p.IsConnected) SendReply(p, msg);
            }
            return;
        }

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (input == null || player == null) return;

            if (frozenPlayers.Contains(player.UserIDString))
            {
                var oldPos = player?.transform?.position ?? Vector3.zero;
                if (oldPos == Vector3.zero) return;

                player.Invoke(() =>
                {
                    if (player == null || player.IsDestroyed || player.gameObject == null || !player.IsConnected || player.IsDead() || player?.transform == null) return;
                    if (Vector3.Distance(oldPos, player.transform.position) <= 0.1) return;
                    TeleportPlayer(player, oldPos, true, false);
                }, 0.02f);
            }
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("ViolationData", violationData);

        #endregion
        #region Util
        #region Invokes
        private ListDictionary<InvokeAction, float> InvokeList { get { return (InvokeHandler.Instance == null) ? null : (ListDictionary<InvokeAction, float>)curList.GetValue(InvokeHandler.Instance); } }

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
        #endregion

        #region Violations
        private void AddViolationFull(string userID, int type, Vector3 startPosition, Vector3 endPosition, string violationText = "", float violationAmount = 0f, int ping = -1, string weapon = "")
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            var violation = new ViolationLog
            {
                Type = (ViolationType)type,
                StartPosition = startPosition,
                EndPosition = endPosition,
                ViolationAmount = violationAmount,
                ViolationText = violationText,
                Ping = ping,
                Weapon = weapon
            };

            if (violation.Type == ViolationType.Spider || violation.Type == ViolationType.Jump)
            {
                HandleJumpOrSpider(violationText, type, userID, violation);
            }

            violationData.AddViolation(userID, violation);
        }

        private void AddViolation(string userID, int type, Vector3 position, string violationText = "", float violationAmount = 0f, int ping = -1, string weapon = "")
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            var violation = new ViolationLog
            {
                Type = (ViolationType)type,
                Position = position,
                ViolationAmount = violationAmount,
                ViolationText = violationText,
                Ping = ping,
                Weapon = weapon
            };

            if (violation.Type == ViolationType.Spider || violation.Type == ViolationType.Jump)
            {
                HandleJumpOrSpider(violationText, type, userID, violation);
            }

            violationData.AddViolation(userID, violation);
        }

        private void HandleJumpOrSpider(string violationText, int type, string userID, ViolationLog violation)
        {
            var logger = PositionLogger;
            if (logger != null && logger.IsLoaded)
            {
                //retarded identifier (again) because userID is not saved to data file
                var identifier = violationText + "_" + type + "_" + violation.ViolationTime.ToString("s", CultureInfo.InvariantCulture);
                logger.Call("SaveTrackerToFullLog", userID, identifier);
                RecordPlayer(BasePlayer.FindAwakeOrSleeping(userID), "PACC_" + violation.Type.ToString() + "_" + userID + "_" + GetRecordingTime());
            }
            else PrintWarning(nameof(PositionLogger) + " null/unloaded!");
        }

        private bool ShouldBanForViolations(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var violations = violationData.GetViolations(userId);
            if (violations == null || violations.Count < 1) return false;

            var totalCount = 0d;
            var now = DateTime.UtcNow;
            for (int i = 0; i < violations.Count; i++)
            {
                var log = violations[i];
                if (log == null) continue;

                if (totalCount >= BanViolationThreshold) break;

                var secs = (now - log.ViolationTime).TotalSeconds;
                if (secs <= 90)
                {
                    if (log.Type == ViolationType.Recoil)
                    {
                        var recAdd = 1d;
                        if (!string.IsNullOrEmpty(log.Weapon))
                        {
                            if (log.Weapon.Contains("M92", CompareOptions.OrdinalIgnoreCase)) recAdd = 7;
                            else if (log.Weapon.Contains("Action Rifle", CompareOptions.OrdinalIgnoreCase) || log.Weapon.Contains("L96", CompareOptions.OrdinalIgnoreCase)) recAdd = 12;
                            else if (log.Weapon.Contains("Revolver", CompareOptions.OrdinalIgnoreCase) || log.Weapon.Contains("Shotgun", CompareOptions.OrdinalIgnoreCase)) recAdd = 8;
                         //   if (log.Weapon.Contains("M39", CompareOptions.OrdinalIgnoreCase)) recAdd = 11;
                            else if (log.Weapon.Contains("Semi-Automatic Rifle", CompareOptions.OrdinalIgnoreCase) || log.Weapon.Contains("M39", CompareOptions.OrdinalIgnoreCase)) recAdd = 4.25;
                            else if (log.Weapon.Contains("MP5", CompareOptions.OrdinalIgnoreCase)) recAdd = 3;
                            else if (log.Weapon.Equals("Assault Rifle", StringComparison.OrdinalIgnoreCase)) recAdd = 4;
                            else if (log.Weapon.Contains("Hunting Bow", CompareOptions.OrdinalIgnoreCase)) recAdd = 30;
                            else if (log.Weapon.Contains("M249", CompareOptions.OrdinalIgnoreCase)) recAdd = 0.5;
                            
                        }
                        totalCount += recAdd;
                    }
                    if (log.Type == ViolationType.AimSprint) totalCount += log.ViolationAmount;
                    if (log.Type == ViolationType.All) totalCount += log.ViolationAmount; 
                    if (log.Type == ViolationType.LOS) totalCount++;
                    if (log.Type == ViolationType.ArrowSpeed) totalCount += 7;
                    if (log.Type == ViolationType.Jump) totalCount += 5;
                    if (log.Type == ViolationType.Spider) totalCount += 8;
                    if (log.Type == ViolationType.Headshots && secs <= 33) totalCount += Mathf.Clamp(log.ViolationAmount, 1, 4) + 1;
                }
            }

            if (totalCount > 0)
            {
                var isAuthed = CGSAuth?.Call<bool>("IsAuthed", userId) ?? true; //because we're dealing with banning, we're going to assume the default as true just to be on the safe side
                if (!isAuthed) totalCount *= 1.6;
            }

            return Math.Round(totalCount, MidpointRounding.AwayFromZero) >= BanViolationThreshold;
        }

        private string GetBanReasonsFromViolations(string userId, bool full = false)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));


            var violations = violationData.GetViolations(userId);
            if (violations != null && violations.Count > 0)
            {
                var sb = Facepunch.Pool.Get<StringBuilder>();
                try
                {

                    sb.Clear().Append("Cheating - Other (");

                    var violationCounts = Facepunch.Pool.Get<Dictionary<ViolationType, int>>();

                    try 
                    {
                        violationCounts.Clear();

                        int temp;

                        for (int i = 0; i < violations.Count; i++)
                        {
                            var violation = violations[i];
                            if (violation == null) continue;
                            var type = violation.Type;
                            if (!violationCounts.TryGetValue(type, out temp)) violationCounts[type] = 1;
                            else violationCounts[type] += 1;
                        }

                        var violationsInOrder = violationCounts.OrderByDescending(p => p.Value);
                        foreach (var violation in violationsInOrder)
                        {
                            var key = violation.Key;
                            if (key == ViolationType.All || key == ViolationType.None) continue;
                            var getCount = violation.Value;
                            if (getCount > 0)
                            {
                                var typeStr = key.ToString();
                                sb.Append(full ? typeStr : char.ToUpper(typeStr[0], CultureInfo.InvariantCulture).ToString()).Append("/");
                            }
                        }
                        sb.Length -= 1;
                        return sb.Append(")").ToString();
                    }
                    finally { Facepunch.Pool.Free(ref violationCounts); }
                    
                }
                finally { Facepunch.Pool.Free(ref sb); }

            }

            return string.Empty;
        }

        private System.Collections.IEnumerator PurgeData()
        {
            if (IsPurging || violationData == null || violationData.Violations == null || violationData.Violations.Count < 1) yield return null;
            IsPurging = true;
            try
            {
                var now = DateTime.UtcNow;
                var enumMax = 8;
                var enumCount = 0;
                var logMax = 50;
                var logCount = 0;
                foreach (var kvp in violationData.Violations)
                {

                    if (enumCount >= enumMax)
                    {
                        enumCount = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    var toRemove = new HashSet<ViolationLog>();
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        var log = kvp.Value[i];
                        if (log == null || log.ViolationTime == DateTime.MinValue) continue;
                        if (logCount >= logMax)
                        {
                            logCount = 0;
                            yield return CoroutineEx.waitForEndOfFrame;
                        }
                        var diff = now - log.ViolationTime;
                        if (diff.TotalHours >= 13) toRemove.Add(log);
                        logCount++;
                    }
                    if (toRemove.Count > 0) foreach (var log in toRemove) kvp.Value.Remove(log);

                    enumCount++;
                }
            }
            finally { IsPurging = false; }
        }

        private System.Collections.IEnumerator CheckAutoBans()
        {
            if (violationData == null || violationData.Violations == null || violationData.Violations.Count < 1) yield return null;

            Interface.Oxide.LogWarning("CheckAutoBans coroutine start");
           

            var i = 0;

            var enumMax = 2;
            var enumCount = 0;

            while (true)
            {
                var pCount = BasePlayer.activePlayerList.Count;
                if (pCount < 1)
                {
                  //  PrintWarning("no players! waiting 12 seconds");
                    yield return CoroutineEx.waitForSeconds(12f);
                    continue;
                }

                if (i >= pCount)
                {
                    i = 0;
                    yield return CoroutineEx.waitForSeconds(4f);
                    continue;
                }

                var sb = Facepunch.Pool.Get<StringBuilder>();
                try
                {
                  

                    if (enumCount >= enumMax)
                    {
                        enumCount = 0;
                        yield return CoroutineEx.waitForSeconds(0.25f);
                    }

                    pCount = BasePlayer.activePlayerList?.Count ?? 0;

                    if (i >= pCount)
                    {
                        i = 0;
                        continue;
                    }

                    try
                    {
                        
                        var p = BasePlayer.activePlayerList[i];
                        if (p == null || p.IsAdmin) continue;

                        if (ShouldBanForViolations(p.UserIDString))
                        {
                            var banReason = GetBanReasonsFromViolations(p.UserIDString);
                            if (string.IsNullOrEmpty(banReason))
                            {
                                PrintWarning("null/empty banReason after ShouldBan?!?!?!");
                                banReason = "Cheating - Other (UNK)";
                            }

                            var banReasonFull = GetBanReasonsFromViolations(p.UserIDString, true);
                            PrintWarning(sb.Clear().Append("Automatically banning: ").Append(p?.displayName).Append("/").Append(p?.UserIDString).Append("/").Append(getIPString(p?.Connection)).Append(" for reason: ").Append(banReason).ToString());

                            RecordPlayer(p, "PACC_autoban_" + p.UserIDString + "_" + GetRecordingTime());

                            covalence.Server.Command(sb.Clear().Append("ban \"").Append(p.UserIDString).Append("\" \"").Append(banReason).Append("\"").ToString());

                            covalence.Server.Command(sb.Clear().Append("bannotes \"").Append(p.UserIDString).Append("\" \"").Append(banReasonFull).Append("\"").ToString());
                        }
                    }
                    catch(IndexOutOfRangeException rangeEx)
                    {
                        PrintError(nameof(CheckAutoBans) + " had out of range exception on i: " + i + ", current player count: " + (BasePlayer.activePlayerList?.Count ?? 0));
                        throw rangeEx;
                    }
                    finally
                    {
                        i++;
                        enumCount++;
                    }

                }
                finally { Facepunch.Pool.Free(ref sb); }

            }
        }
        #endregion

        private string GetRecordingTime()
        {
            var now = DateTime.UtcNow;
            return now.Year + "_" + now.Month + "_" + now.Day + "_" + now.Hour + "_" + now.Minute + "_" + now.Second + "_UTC";
        }

        private void RecordPlayer(BasePlayer player, string filename)
        {
            timer.Once(5f, () =>
            {
                var files = _RecordPlayer(player, filename);

                if (files.Count < 1) PrintError("Failed to save recording " + filename);
                else Puts("Recording " + filename + " saved");
            });
        }

        private List<string> _RecordPlayer(BasePlayer player, string filename)
        {
            var rv = new List<string>();

            if (PRISM != null && PRISM.IsLoaded)
            {
                rv = PRISM.Call<List<string>>("Save", player, filename);
            }

            return rv;
        }

        private void FreezePlayer(string userID, float duration = -1f)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));// || duration == 0.0f) throw new ArgumentNullException();
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));

            if (!frozenPlayers.Contains(userID)) frozenPlayers.Add(userID);
            if (duration > 0.0f) InvokeHandler.Invoke(ServerMgr.Instance, () => UnfreezePlayer(userID), duration);
        }

        private void UnfreezePlayer(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            if (frozenPlayers.Contains(userID)) frozenPlayers.Remove(userID);
        }

        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true, bool respawnIfDead = false)
        {
            if (player == null || player?.transform == null) return false;
            var playerPos = player?.transform?.position ?? Vector3.zero;
            var isConnected = player?.IsConnected ?? false;
            if (respawnIfDead && player.IsDead())
            {
                player.RespawnAt(dest, Quaternion.identity);
                return true;
            }
            player.EndLooting(); //redundant?
            player.EnsureDismounted();
            player.SetParent(null, true);
            var distFrom = Vector3.Distance(playerPos, dest);

            if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
            if (doSleep && isConnected) player.StartSleeping();
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
            return true;
        }

        private int GetPing(BasePlayer player) => player?.net?.connection != null ? Network.Net.sv.GetAveragePing(player.net.connection) : -1;

        private string getIPString(Network.Connection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var IP = connection?.ipaddress ?? string.Empty;
            if (!string.IsNullOrEmpty(IP) && IP.Contains(":")) IP = IP.Split(':')[0];

            return IP;
        }

        private static Vector3 GetVector3FromString(string vectorStr)
        {
            if (string.IsNullOrEmpty(vectorStr)) throw new ArgumentNullException(nameof(vectorStr));

            var split1 = new StringBuilder(vectorStr).Replace("(", string.Empty).Replace(")", string.Empty).ToString().Split(',');
            return new Vector3(Convert.ToSingle(split1[0]), Convert.ToSingle(split1[1]), Convert.ToSingle(split1[2]));
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


        private IPlayer FindConnectedPlayer(string nameOrIdOrIp, bool tryFindOfflineIfNoOnline = false)
        {
            if (string.IsNullOrEmpty(nameOrIdOrIp)) throw new ArgumentNullException(nameof(nameOrIdOrIp));

            var p = covalence.Players.FindPlayerById(nameOrIdOrIp);
            if (p != null) if ((!p.IsConnected && tryFindOfflineIfNoOnline) || p.IsConnected) return p;


            var players = Facepunch.Pool.GetList<IPlayer>();
            try
            {

                foreach (var player in covalence.Players.Connected)
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
            finally { Facepunch.Pool.FreeList(ref players); }
        }

        private void SendWarning(string msg)
        {
            if (string.IsNullOrEmpty(msg)) throw new ArgumentNullException(nameof(msg));

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if ((p?.IsConnected ?? false) && permission.UserHasPermission(p.UserIDString, ShowMessagePerm) && !noMsg.Contains(p.userID)) SendReply(p, msg);
            }
            PrintWarning(RemoveTags(msg));
        }

        private T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        private bool IsSaving() { return IsServerSaving; }
        
        private bool IsServerStalled() { return Performance.current.frameTime >= 30; }

        private bool IsLowFPS() { return Performance.current.frameRateAverage < Mathf.Clamp(100, 100, ConVar.FPS.limit) || Performance.current.frameRate < Mathf.Clamp(50, 50, ConVar.FPS.limit); }

        private bool IsLowFPSStrict() { return Performance.current.frameRate < 50; }

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return covalence.Players?.FindPlayerById(userID)?.Name ?? string.Empty;
        }

        private string GetDisplayNameFromUID(ulong userID) { return GetDisplayNameFromID(userID.ToString()); }

        private void SendReply(IPlayer player, string msg) => player?.Reply(msg);
        #endregion 
    }
}
