using System.Collections.Generic;
using UnityEngine;
using System;
using Oxide.Core.Plugins;
using System.Text;
using Oxide.Core;
using Facepunch;
using System.Diagnostics;

namespace Oxide.Plugins
{
    [Info("ServerESP", "Shady", "2.0.1", ResourceId = 0)]
    internal class ServerESP : RustPlugin
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

        //todo: optimize by removing unnecessary savelist loops by having hashsets for stuff like ore


        #region Fields
        #region Constants
        public const string ESP_PERMISSION = "serveresp.esp";

        public const int PLAYER_COLL = 131072;
        #endregion
        #region HashSets
        private readonly HashSet<BaseNpc> allAnimals = new HashSet<BaseNpc>();
        private readonly HashSet<StashContainer> allStashes = new HashSet<StashContainer>();
        private readonly HashSet<LootContainer> allLoot = new HashSet<LootContainer>();
        private readonly HashSet<DroppedItemContainer> deathContainers = new HashSet<DroppedItemContainer>();
        private readonly HashSet<Item> allDropped = new HashSet<Item>();
        private readonly HashSet<string> _visChecks = new HashSet<string>();
        #endregion

        #region Dictionaries
        private readonly Dictionary<ulong, Timer> espTimer = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> espTimerP = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> espTimerS = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> espTimerE = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> espTimerD = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> visTimer = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> lineTimer = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> oreTimer = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> allTimer = new Dictionary<ulong, Timer>();
        #endregion




        #region Booleans
        public bool IsAllESP { get; set; } = false;
        private bool _init = false;
        #endregion

        #region Plugin References
        [PluginReference]
        private readonly Plugin Godmode;

        [PluginReference]
        private readonly Plugin Vanish;
        #endregion

        public static ServerESP _instance;

        #endregion


        private void Init()
        {
            try
            {
                Unsubscribe(nameof(OnEntitySpawned));
                Unsubscribe(nameof(OnEntityDeath));

                permission.RegisterPermission(ESP_PERMISSION, this);
            }
            finally { _instance = this; }
        }

        private void Unload()
        {
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null) continue;
                    var espComp = player?.GetComponent<PlayerESP>() ?? null;
                    if (espComp != null) espComp.DoDestroy();
                }
            }
            finally { _instance = null; }

        }

        private void OnServerInitialized()
        {
            try 
            {
                foreach (var entity in BaseNetworkable.serverEntities)
                {
                    if (entity == null) continue;
                    var animal = entity as BaseNpc;
                    var loot = entity as LootContainer;
                    var dropped = entity as DroppedItem;
                    var droppedContainer = entity as DroppedItemContainer;
                    var stash = entity as StashContainer;

                    if (animal != null) allAnimals.Add(animal);

                    if (loot != null) allLoot.Add(loot);

                    if (dropped?.item != null) allDropped.Add(dropped.item);

                    if (droppedContainer != null) deathContainers.Add(droppedContainer);

                    if (stash != null) allStashes.Add(stash);
                }

                Subscribe(nameof(OnEntitySpawned));
                Subscribe(nameof(OnEntityDeath));
            }
            finally { _init = true; }

        }

        private void OnItemDropped(Item item, BaseEntity entity)
        {
            if (item == null || (entity?.IsDestroyed ?? true) || (entity?.transform?.position ?? Vector3.zero) == Vector3.zero) return;
            if (!allDropped.Contains(item)) allDropped.Add(item);
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (!_init) return;

            var loot = entity as LootContainer;
            var npc = entity as BaseNpc;
            var stash = entity as StashContainer;
            var dropContainer = entity as DroppedItemContainer;

            if (loot != null) allLoot.Add(loot);
            if (stash != null) allStashes.Add(stash);
            if (npc != null) allAnimals.Add(npc);
            if (dropContainer != null) deathContainers.Add(dropContainer);
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || !_init) return;

            var dropContainer = entity as DroppedItemContainer;
            var npc = entity as BaseNpc;
            var stash = entity as StashContainer;
            var loot = entity as LootContainer;

            if (dropContainer != null) deathContainers.Remove(entity as DroppedItemContainer);
            if (npc != null) allAnimals.Remove(npc);
            if (stash != null) allStashes.Remove(stash);
            if (loot != null) allLoot.Remove(loot);
        }


        private void OnPlayerDisconnected(BasePlayer player)
        {
            Timer _timer;

            if (espTimer.TryGetValue(player.userID, out _timer))
            {
                _timer.Destroy();
                espTimer.Remove(player.userID);
            }

            if (espTimerD.TryGetValue(player.userID, out _timer))
            {
                _timer.Destroy();
                espTimerD.Remove(player.userID);
            }

            if (espTimerE.TryGetValue(player.userID, out _timer))
            {
                _timer.Destroy();
                espTimerE.Remove(player.userID);
            }

            if (espTimerP.TryGetValue(player.userID, out _timer))
            {
                _timer.Destroy();
                espTimerP.Remove(player.userID);
            }

            if (espTimerS.TryGetValue(player.userID, out _timer))
            {
                _timer.Destroy();
                espTimerS.Remove(player.userID);
            }

            if (visTimer.TryGetValue(player.userID, out _timer))
            {
                _timer.Destroy();
                visTimer.Remove(player.userID);
            }

            if (lineTimer.TryGetValue(player.userID, out _timer))
            {
                _timer.Destroy();
                lineTimer.Remove(player.userID);
            }

            var espComp = player?.GetComponent<PlayerESP>() ?? null;
            if (espComp != null) espComp.DoDestroy();
        }

        private bool HasESP(BasePlayer player)
        {
            if (player == null || !(player?.IsConnected ?? false)) return false;
            var uID = player?.userID ?? 0;
            return (espTimer.ContainsKey(uID) || espTimerD.ContainsKey(uID) || espTimerP.ContainsKey(uID) || espTimerS.ContainsKey(uID) || espTimerE.ContainsKey(uID));
        }

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return covalence.Players?.FindPlayerById(userID)?.Name ?? "Unknown";
        }

        private string GetDisplayNameFromID(ulong userID)
        {
            return GetDisplayNameFromID(userID.ToString());
        }

        private bool IsVanished(BasePlayer player) { return Vanish?.Call<bool>("IsInvisible", player) ?? false; }

        private int GetPing(BasePlayer player) { return (player == null || player?.net?.connection == null) ? -1 : Network.Net.sv.GetAveragePing(player.net.connection); }

        private Vector3 GetPlayerRotation(BasePlayer player) { return Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward; }

        private class PlayerESP : FacepunchBehaviour
        {
            private BasePlayer _player;

            public bool Sleepers { get; set; } = false;
            public bool Online { get; set; } = true;
            private bool IsRendering { get; set; } = false;
            public float MaxDistance { get; set; } = 550;

            public float RenderInterval { get; set; } = 0.07492f;

            private float _lastPosUpdate = 0f;
            private Vector3 _pos = Vector3.zero;
            public Vector3 GetCachedPosition()
            {
                if (_pos == Vector3.zero || _lastPosUpdate == 0f || Time.realtimeSinceStartup - _lastPosUpdate >= 5)
                {
                    _lastPosUpdate = Time.realtimeSinceStartup;

                    _pos = _player?.transform?.position ?? Vector3.zero;

                }

                return _pos;
            }

            private readonly StringBuilder _stringBuilder = new StringBuilder();

            private Coroutine _espRoutine = null;

            private ListHashSet<BasePlayer> _cachedPlayers = new ListHashSet<BasePlayer>();

            private readonly Dictionary<ItemDefinition, string> _itemDefToSubStringName = new Dictionary<ItemDefinition, string>();

            private void Awake()
            {
                _player = GetComponent<BasePlayer>();
                if (_player == null)
                {
                    Interface.Oxide.LogError("!! BasePlayer is null on PlayerESP.Awake() !!");
                    DoDestroy();
                    return;
                }

                InvokeRepeating(RenderESP, 0.1f, RenderInterval);
                InvokeRepeating(UpdatePlayerCache, 0.1f, 12f);
            }



            private void UpdatePlayerCache()
            {
                var watch = Pool.Get<Stopwatch>();

                try
                {
                    watch.Restart();

                    if (!Sleepers && Online) _cachedPlayers = BasePlayer.activePlayerList;
                    else if (!Online && Sleepers) _cachedPlayers = BasePlayer.sleepingPlayerList;
                    else if (Online && Sleepers)
                    {
                        _cachedPlayers ??= new ListHashSet<BasePlayer>();

                        foreach (var p in BasePlayer.allPlayerList)
                            _cachedPlayers.Add(p);
                    }
                }
                finally
                {
                    try { if (watch.Elapsed.TotalMilliseconds >= 5) Interface.Oxide.LogWarning(nameof(PlayerESP) + ". " + nameof(UpdatePlayerCache) + " took: " + watch.Elapsed.TotalMilliseconds); }
                    finally { Pool.Free(ref watch); }
                }
            }

            private readonly Dictionary<BasePlayer, TimeCachedValue<float>> _playerDistanceCache = new();

            private System.Collections.IEnumerator DoESP(ListHashSet<BasePlayer> playerList)
            {
                if (playerList == null) 
                    throw new ArgumentNullException(nameof(playerList));

                var watch = Pool.Get<Stopwatch>();

                try
                {
                    watch.Restart();

                    IsRendering = true;

                    var count = 0;
                    var max = playerList.Count * 0.5; //(int)Math.Round(playerList.Count * 0.425, MidpointRounding.AwayFromZero);

                    var playerPos = GetCachedPosition();

                    var hasVis = _instance?._visChecks?.Contains(_player.UserIDString) ?? false;

                    var uid = _player.UserIDString;

                    try
                    {
                        foreach (var target in playerList)
                        {
                            if (target == null || target.IsDestroyed || target?.gameObject == null || target?.transform == null || target.IsDead() || (!target.IsSleeping() && target?.net?.connection == null)) continue;

                            count++;
                            if (count >= max)
                            {
                                count = 0;
                                yield return CoroutineEx.waitForEndOfFrame;
                            }

                            if (!_playerDistanceCache.TryGetValue(target, out _))
                                _playerDistanceCache[target] = new TimeCachedValue<float>()
                                {
                                    refreshCooldown = 0.67f,
                                    refreshRandomRange = 0.33f,
                                    updateValue = new Func<float>(() =>
                                    {
                                        return Vector3.Distance(GetCachedPosition(), target?.eyes?.position ?? Vector3.zero);
                                    })
                                };

                            var renderPos = target?.eyes?.position ?? Vector3.zero;

                            var distFrom = _playerDistanceCache[target].Get(false);
                            if (distFrom >= MaxDistance) continue;

                            var isVis = false;

                            if (hasVis && distFrom <= 260 && distFrom >= 1)
                            {
                                var currentRot = Quaternion.Euler((target?.serverInput?.current?.aimAngles ?? Vector3.zero)) * Vector3.forward;
                                var ray = new Ray(target?.eyes?.position ?? Vector3.zero, currentRot);
                                var spheres = Physics.SphereCastAll(ray, 0.4f, 250, PLAYER_COLL);
                                if (spheres != null && spheres.Length > 0)
                                {
                                    for (int j = 0; j < spheres.Length; j++)
                                    {
                                        var hit = spheres[j];
                                        var hitPly = hit.GetEntity() as BasePlayer;
                                        if (hitPly == null) continue;
                                        if (hitPly.UserIDString == uid)
                                        {
                                            isVis = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            var weaponItem = target?.GetActiveItem() ?? null;

                            var weaponName = string.Empty;

                            if (weaponItem != null && !_itemDefToSubStringName.TryGetValue(weaponItem.info, out weaponName))
                            {
                                //doesn't handle custom name
                                weaponName = weaponItem?.info?.displayName?.english ?? string.Empty;

                                if (weaponName.Length > 16)
                                {
                                    var subWepName = weaponName.Substring(0, 16);
                                    if ((subWepName.Length + 2) < weaponName.Length)
                                        _itemDefToSubStringName[weaponItem.info] = weaponName = _stringBuilder.Clear().Append(subWepName).Append("..").ToString();
                                }
                            }



                            var attachmentsSB = Pool.Get<StringBuilder>();

                            var attachments = string.Empty;

                            var contents = weaponItem?.contents ?? null;

                            if ((contents?.itemList?.Count ?? 0) > 0)
                            {
                                try
                                {
                                    attachmentsSB.Clear().Append("(");

                                    for (int j = 0; j < contents.itemList.Count; j++)
                                    {
                                        var item = contents.itemList[j];
                                        if (item == null) continue;

                                        var modName = string.Empty;

                                        if (!_itemDefToSubStringName.TryGetValue(item.info, out modName))
                                        {
                                            modName = item?.info?.displayName?.english ?? string.Empty;

                                            if (modName.Length > 12)
                                            {
                                                var sub = modName.Substring(0, 12);
                                                if ((sub.Length + 2) < modName.Length)
                                                    _itemDefToSubStringName[item.info] = modName = _stringBuilder.Clear().Append(sub).Append("..").ToString();
                                            }
                                        }



                                        attachmentsSB.Append(modName).Append(", ");
                                    }

                                    attachmentsSB.Length -= 2;
                                    attachmentsSB.Append(")");

                                    attachments = attachmentsSB.ToString();

                                }
                                finally { Pool.Free(ref attachmentsSB); }
                            }


                            if (target.userID == _player.userID) 
                                renderPos.y += 0.75f;

                            var animalName = target?.displayName ?? "Unknown";
                            if (animalName.Length > 22)
                            {
                                var subName = animalName.Substring(0, 22);
                                if ((subName.Length + 2) < animalName.Length) 
                                    animalName = _stringBuilder.Clear().Append(subName).Append("..").ToString();

                            }


                            _stringBuilder.Clear().Append("<size=16> ").Append(animalName).Append("</size>").Append("<size=13><color=#3ca9fc> ").Append(distFrom.ToString("N0")).Append("</color></size>m  <color=#FFA500><size=15>").Append(target.Health().ToString("N0")).Append(" </size></color><size=10>%</size>");

                            if (!string.IsNullOrEmpty(weaponName)) 
                                _stringBuilder.AppendLine("<size=15>").Append(weaponName).Append("</size> <size=13>").Append(attachments).Append("</size>");

                            var isGod = _instance?.Godmode?.Call<bool>("IsGodU", target.userID) ?? false;
                            var isVanish = _instance?.IsVanished(target) ?? false;
                            if (isVanish)
                                _stringBuilder.Append(Environment.NewLine).Append("<size=13><color=#7300e6>Vanish</color></size>");

                            if (isGod)
                                _stringBuilder.Append(Environment.NewLine).Append("<size=13><color=orange>God</color></size>");

                            if (isVis && _player.userID != target.userID)
                                _stringBuilder.Append(Environment.NewLine).Append("<size=17><color=#007AE9>Looking at you</color></size>");

                            var ping = _instance?.GetPing(target) ?? 0;
                            if (ping > 0) 
                                _stringBuilder.Append(Environment.NewLine).Append("<color=#0073e6><size=13>").Append(ping.ToString("N0")).Append("ms</size></color>");

                            _player.SendConsoleCommand("ddraw.text", 0.075f, Color.white, renderPos, _stringBuilder.ToString());



                        }
                    }
                    finally
                    {
                        IsRendering = false;
                    }

                }
                finally
                {
                    try { if (watch.Elapsed.TotalSeconds >= RenderInterval) Interface.Oxide.LogWarning(nameof(DoESP) + " took: " + watch.Elapsed.TotalMilliseconds + "ms"); }
                    finally { Pool.Free(ref watch); }
                }

               
            }

            private void RenderESP()
            {
                if (IsRendering || ServerMgr.Instance == null) return;
                if (_player == null || !_player.IsConnected || _instance == null)
                {
                    DoDestroy();
                    return;
                }

                if (_player.IsReceivingSnapshot || !_player.IsAlive() || _player.IdleTime >= 90) return;

                if (_espRoutine != null)
                {
                    ServerMgr.Instance.StopCoroutine(_espRoutine);
                    _espRoutine = null;
                }

                _espRoutine = ServerMgr.Instance.StartCoroutine(DoESP(_cachedPlayers));
            }

            public void DoDestroy()
            {
                try
                {
                    InvokeHandler.CancelInvoke(_player, UpdatePlayerCache);
                    InvokeHandler.CancelInvoke(_player, RenderESP);
                }
                finally { Destroy(this); }
            }

        }


        public static HashSet<T> ToHashSet<T>(IEnumerable<T> enumerable) { return new HashSet<T>(enumerable); }

        

        private System.Collections.IEnumerator AllESP(BasePlayer player, string prefabName)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(prefabName)) throw new ArgumentNullException(nameof(prefabName));

            if (IsAllESP) yield return null;

            IsAllESP = true;
            var count = 0;
            var max = 9000;
            uint prefabId = 0;
            var toRender = Facepunch.Pool.GetList<BaseEntity>();
            try
            {
                foreach (var entity in BaseNetworkable.serverEntities)
                {
                    if (entity == null || (entity?.IsDestroyed ?? true) || entity?.transform == null || entity?.gameObject == null) continue;
                    count++;
                    if (count >= max)
                    {
                        count = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    if (prefabId == 0 && entity.ShortPrefabName != prefabName) continue;
                    else
                    {
                        if (prefabId != 0 && entity.prefabID != prefabId) continue;
                    }

                    if (prefabId == 0) prefabId = entity.prefabID;

                    var dist = Vector3.Distance(player.transform.position, entity.transform.position);
                    if (dist > 180) continue;

                    var baseEnt = entity as BaseEntity;
                    if (baseEnt == null) continue;
                    toRender.Add(baseEnt);
                }
                var sb = Facepunch.Pool.Get<StringBuilder>();
                try
                {
                    for (int i = 0; i < toRender.Count; i++)
                    {
                        var baseEnt = toRender[i];
                        var dist = Vector3.Distance(player?.transform?.position ?? Vector3.zero, baseEnt?.transform?.position ?? Vector3.zero);
                        var ownerName = GetDisplayNameFromID(baseEnt.OwnerID);
                        var centPos = baseEnt?.CenterPoint() ?? baseEnt?.transform?.position ?? Vector3.zero;

                        sb.Clear().Append(baseEnt.ShortPrefabName).Append(" - : ").Append(dist.ToString("N0")).Append("m");
                        if (!string.IsNullOrEmpty(ownerName)) sb.Append(" - own: ").Append(ownerName);

                        player.SendConsoleCommand("ddraw.text", 0.251, Color.yellow, centPos, sb.ToString());
                    }
                }
                finally { Facepunch.Pool.Free(ref sb); }
            }
            finally
            {
                IsAllESP = false;
                Facepunch.Pool.Free(ref toRender);
            }
        }

        [ChatCommand("esp")]
        private void cmdESP(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, ESP_PERMISSION)) return;

            if (args.Length < 1)
            {
                SendReply(player, "<color=red>Invalid argument</color>, try:\nplayer\nline\narrow\nvischeck\nore\nall <prefab name>\nnpc\nstash\ndi\nloot");
                return;
            }

            var userId = player.userID;

            var arg0 = args[0].ToLower();
            if (arg0 == "line" || arg0 == "arrow")
            {

                if (DeleteFromTimerDictionary(player.userID, lineTimer))
                {
                    SendReply(player, "Disabled line checker");
                    return;
                }

                var useLine = arg0 != "arrow";

                var drawSelf = args.Length > 1 && args[1].Equals("self");

                var ddrawTxt = string.Empty;

                var sb = Facepunch.Pool.Get<StringBuilder>();

                try { ddrawTxt = sb.Clear().Append("ddraw.").Append(useLine ? "line" : "arrow").ToString(); }
                finally { Facepunch.Pool.Free(ref sb); }

                lineTimer.Add(userId, timer.Every(0.07492f, () =>
                {
                    if (player == null || !(player?.IsConnected ?? false))
                    {
                        DeleteFromTimerDictionary(userId, lineTimer);
                        return;
                    }

                    var plyPos = player?.transform?.position ?? Vector3.zero;
                    var plyCenter = player?.CenterPoint() ?? Vector3.zero;



                    for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                    {
                        var activePlayer = BasePlayer.activePlayerList[i];
                        if (activePlayer == null || !(activePlayer?.IsAlive() ?? false) || !(activePlayer?.IsConnected ?? false) || (activePlayer.userID == player.userID && !drawSelf)) continue;

                        var eyesPos = activePlayer?.eyes?.position ?? Vector3.zero;
                        if (eyesPos == Vector3.zero) continue;

                        var dist = Vector3.Distance(plyCenter, eyesPos);
                        if (dist > 300) continue;

                        //var currentRot = GetPlayerRotation(activePlayer);

                        //if (activePlayer.IsAdmin) activePlayer.SendConsoleCommand("echo", "aimAngles: " + activePlayer?.serverInput?.current?.aimAngles + " - eyesPos: " + eyesPos);

                        var direction = player.eyes.rotation * Vector3.forward;

                        RaycastHit rayHit;
                        if (Physics.Raycast(new Ray(eyesPos, direction), out rayHit, 200f))
                        {
                            var hitPos = rayHit.point;
                            if (hitPos == Vector3.zero) continue;

                            player.SendConsoleCommand(ddrawTxt, 0.075f, Color.blue, eyesPos, hitPos, 0.175f);
                        }
                    }
                }));
            }

            if (arg0 == "ore")
            {
                if (DeleteFromTimerDictionary(player.userID, oreTimer))
                {
                    SendReply(player, "Disabled ore ESP");
                    return;
                }



                oreTimer.Add(userId, timer.Every(0.5f, () =>
                {
                    if (player == null || !(player?.IsConnected ?? false))
                    {
                        DeleteFromTimerDictionary(userId, oreTimer);
                        return;
                    }

                    foreach (var entity in BaseEntity.saveList)
                    {
                        var ore = entity as OreResourceEntity;
                        if (ore == null) continue;

                        var dist = Vector3.Distance(player.transform.position, ore.transform.position);
                        if (dist > 300) continue;

                        player.SendConsoleCommand("ddraw.text", 0.51, Color.magenta, ore.CenterPoint(), ore.ShortPrefabName + " - dist: " + dist.ToString("N0") + "m");
                    }
                }));
            }

            if (arg0 == "all")
            {
                if (DeleteFromTimerDictionary(userId, allTimer))
                {
                    SendReply(player, "Disabled All ESP");
                    return;
                }

                if (args.Length < 2)
                {
                    SendReply(player, "You must specify a prefab to ESP");
                    return;
                }

                var arg1Lower = args[1].ToLower();
                allTimer[player.userID] = timer.Every(0.25f, () =>
                {
                    if (player == null || !(player?.IsConnected ?? false))
                    {
                        DeleteFromTimerDictionary(userId, allTimer);
                        return;
                    }

                    if (!IsAllESP && ServerMgr.Instance != null) ServerMgr.Instance.StartCoroutine(AllESP(player, arg1Lower));
                });
            }

            if (arg0 == "vischeck")
            {
                if (_visChecks.Add(player.UserIDString)) SendReply(player, "Enabled vis checker");
                else
                {
                    _visChecks.Remove(player.UserIDString);
                    SendReply(player, "Disabled vis checker");
                }
            }

            if (arg0 == "animal" || arg0 == "npc")
            {
                if (DeleteFromTimerDictionary(userId, espTimer))
                {
                    SendReply(player, "Disabled Animal/NPC ESP");
                    return;
                }

                espTimer.Add(player.userID, timer.Every(0.245f, () =>
                {
                    if (player == null || player.gameObject == null || player.IsDestroyed || !player.IsConnected)
                    {
                        DeleteFromTimerDictionary(userId, espTimer);
                        return;
                    }

                    if (player.IsReceivingSnapshot || !player.IsAlive()) return;

                    foreach (var animal in allAnimals)
                    {
                        if (animal == null || (animal?.IsDestroyed ?? true)) continue;

                        var dist = Vector3.Distance(player?.transform?.position ?? Vector3.zero, animal?.transform?.position ?? Vector3.zero);
                        if (dist >= 650 || (animal?.Health() ?? 0f) <= 0) continue;

                        var adjName = animal?.ShortPrefabName ?? "Unknown";

                        var nameSize = 15;

                        var sb = Facepunch.Pool.Get<StringBuilder>();
                        try
                        {
                            player.SendConsoleCommand("ddraw.text", 0.25f, Color.magenta, animal.CenterPoint() + new Vector3(0, .5f), sb.Clear().Append("<size= ").Append(nameSize).Append(">").Append(adjName).Append("</size> dist: ").Append(dist.ToString("N0")).Append("  HP: ").Append(animal.Health().ToString("N0")).ToString());
                        }
                        finally { Facepunch.Pool.Free(ref sb); }
                    }

                }));
            }

            if (arg0 == "player")
            {
                var sleepers = args.Length > 1 && args[1].Equals("sleepers", StringComparison.OrdinalIgnoreCase);

                var espComp = player?.GetComponent<PlayerESP>() ?? null;
                if (espComp != null)
                {
                    espComp.DoDestroy();
                    SendReply(player, "Disabled ESP Player");
                    return;
                }
                else
                {
                    var obj = player.gameObject.AddComponent<PlayerESP>();
                    obj.Sleepers = sleepers;

                    SendReply(player, "Enabled ESP Player");
                }
            }

            float dist2;
            if (float.TryParse(arg0, out dist2))
            {
                if (dist2 <= 0 || dist2 > 700)
                {
                    SendReply(player, "Bad distance: " + dist2);
                    return;
                }
                var obj = player?.GetComponent<PlayerESP>() ?? null;
                if (obj != null)
                {
                    SendReply(player, "Distance was: " + obj.MaxDistance + ", Set distance to: " + (obj.MaxDistance = dist2));
                }
                else SendReply(player, "ESP isn't enabled");
            }
            if (arg0 == "stash")
            {
                if (DeleteFromTimerDictionary(userId, espTimerS))
                {
                    SendReply(player, "Disabled Stash ESP");
                    return;
                }


                espTimerS.Add(player.userID, timer.Every(0.445f, () =>
                {
                    if (player == null) return;
                    if (player.IsReceivingSnapshot || !player.IsConnected || !player.IsAlive()) return;
                    foreach (var stash in allStashes)
                    {
                        if (stash == null || (stash?.IsDestroyed ?? true)) continue;
                        if (!stash.ShortPrefabName.Contains("stash")) continue;

                        var dist = Vector3.Distance(player.transform.position, stash.transform.position);
                        if (dist >= 700) continue;
                        var msg = "Stash <size=13>dist: " + dist.ToString("N0") + " Owner: " + (GetDisplayNameFromID(stash?.OwnerID ?? 0) ?? "Unknown") + "</size>";
                        player.SendConsoleCommand("ddraw.text", 0.45f, Color.white, stash.CenterPoint(), msg);
                    }
                }));
            }

            if (arg0 == "di")
            {
                if (DeleteFromTimerDictionary(userId, espTimerD))
                {
                    SendReply(player, "Disabled dropped items ESP");
                    return;
                }


                espTimerD.Add(player.userID, timer.Every(0.125f, () =>
                {
                    if (player == null) return;
                    if (player.IsReceivingSnapshot || !player.IsConnected || !player.IsAlive() || player?.transform == null) return;

                    foreach (var di in allDropped)
                    {
                        if (di == null) continue;

                        var diEnt = di?.GetWorldEntity() ?? null;
                        if (diEnt == null) continue;

                        var droppedItem = diEnt as DroppedItem;
                        if (droppedItem == null) continue;

                        var centerPoint = diEnt?.CenterPoint() ?? Vector3.zero;
                        if (centerPoint == Vector3.zero) continue;

                        var dist = Vector3.Distance(player.transform.position, centerPoint);
                        var maxDist = 350;
                        var rarity = di?.info?.rarity ?? Rust.Rarity.None;

                        if (rarity == Rust.Rarity.None) maxDist = 600;

                        var rarityStr = di?.info?.rarity.ToString() ?? "Unknown";
                        var color = Color.yellow;
                        if (rarityStr == "Rare")
                        {
                            color = Color.cyan;
                            maxDist = 420;
                        }
                        if (rarityStr == "VeryRare")
                        {
                            color = Color.magenta;
                            rarityStr = rarityStr.Replace("VeryRare", "Very Rare");
                            maxDist = 600;
                        }

                        if (dist >= maxDist) continue;

                        var name = di?.info?.displayName?.english ?? "Unknown";
                        //     player.SendConsoleCommand("ddraw.line", 4.8f, Color.yellow, player.transform.position, animal.transform.position);

                        player.SendConsoleCommand("ddraw.text", 0.126f, color, centerPoint + new Vector3(0, .25f), "<size=15>" + name + "</size> <size=13> Distance:  " + dist.ToString("N0") + "m, Rarity: </size><size=15>" + rarityStr + "</size>");
                    }
                    foreach (var dic in deathContainers)
                    {
                        if (dic == null || (dic?.IsDestroyed ?? true) || dic?.transform == null) continue;
                        var dist = Vector3.Distance(dic.transform.position, player.transform.position);
                        if (dist > 500) continue;
                        var distStr = dist.ToString("N0");
                        player.SendConsoleCommand("ddraw.text", 0.126f, Color.white, dic.CenterPoint(), "<size=15> " + dic.ShortPrefabName + "</size> <size=12> Dist: " + distStr + "m</size>, owner:" + (dic?.playerName ?? "Unknown/Non-Player"));
                    }

                }));
                Puts("ESP DI");
            }

            if (arg0 == "loot")
            {
                if (DeleteFromTimerDictionary(userId, espTimerE))
                {
                    SendReply(player, "Disabled Loot ESP");
                    return;
                }


                espTimerE.Add(player.userID, timer.Every(0.445f, () =>
                {
                    if (player == null) return;
                    if (player.IsReceivingSnapshot || !player.IsConnected || !player.IsAlive()) return;
                    foreach (var enemy in allLoot)
                    {
                        if (enemy == null || (enemy?.IsDestroyed ?? true) || (enemy?.IsDead() ?? true)) continue;
                        var dist = Vector3.Distance(player.transform.position, enemy.transform.position);
                        if (dist >= 200) continue;

                        var adjName = enemy?.ShortPrefabName ?? "Unknown";


                        var msgSB = Facepunch.Pool.Get<StringBuilder>();
                        try 
                        {
                            msgSB.Clear().Append("<size=11>").Append(adjName).Append(" ").Append(dist.ToString("N0")).Append("m</size><size=13>").Append(Environment.NewLine);

                            for (int ii = 0; ii < enemy.inventory.itemList.Count; ii++)
                            {
                                var item = enemy.inventory.itemList[ii];
                                if (item == null) continue;
                                var englishName = item?.info?.displayName?.english ?? string.Empty;
                                var amount = item?.amount ?? 0;
                                var modifier = string.Empty;
                                if (item.info.rarity == Rust.Rarity.None) modifier = "<color=white>";
                                if (item.info.rarity == Rust.Rarity.Rare) modifier = "<color=#42dfff>";
                                if (item.info.rarity == Rust.Rarity.VeryRare) modifier = "<color=#eb3dff>";

                                if (!string.IsNullOrEmpty(englishName))
                                {
                                    if (!string.IsNullOrEmpty(modifier)) msgSB.Append(modifier).Append(englishName).Append("</color> x").Append(amount.ToString("N0")).Append(", ");
                                    else msgSB.Append(modifier).Append(englishName).Append(" x").Append(amount).Append(", ");
                                }
                            }

                            var msg = msgSB.ToString().TrimEnd(", ".ToCharArray()) + "</size>";
                            player.SendConsoleCommand("ddraw.text", 0.45f, Color.yellow, enemy.CenterPoint() + new Vector3(0, .5f), msg);
                        }
                        finally { Facepunch.Pool.Free(ref msgSB); }

                        
                    }
                }));

            }
        }

        #region Util
        private bool DeleteFromTimerDictionary(ulong userId, Dictionary<ulong, Timer> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            Timer val;
            if (dictionary.TryGetValue(userId, out val))
            {
                if (val != null) val.Destroy();

                return dictionary.Remove(userId);
            }

            return false;
        }
        #endregion

    }
}
