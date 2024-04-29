// Reference: 0Harmony
using Oxide.Core;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using Network;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using System.Diagnostics;
using System.Text;
using HarmonyLib;
using System.Reflection;
using Facepunch;

namespace Oxide.Plugins
{
    [Info("AAESPBlocker", "Shady", "1.0.9")]
    internal class AAESPBlocker : RustPlugin
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
        private static readonly int blockedLayer = LayerMask.GetMask("Deployed", "Player (Server)", "Prevent Building");
        private static readonly int visLayer = LayerMask.GetMask("Construction", "World", "Terrain", "Default", "Player (Server)");
        private readonly Dictionary<string, string> playerClans = new Dictionary<string, string>();


        #region Fields
        private Harmony _harmony;
        #endregion
        #region Hooks
        private void Init() => DoHarmonyPatches(_harmony = new Harmony(GetType().Name));

        #endregion
        #region Custom Harmony Methods
        private void DoHarmonyPatches(Harmony instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var c = 0;

            var watch = Pool.Get<Stopwatch>();
            try { c = PatchAllHarmonyAttributes(GetType(), instance); }
            finally
            {
                var elapsedMs = watch.ElapsedMilliseconds;
                Pool.Free(ref watch);

                var sb = Pool.Get<StringBuilder>();
                try { PrintWarning(sb.Clear().Append("Took: ").Append(elapsedMs.ToString("0.00").Replace(".00", string.Empty)).Append("ms to apply ").Append(c.ToString("N0")).Append(" patch").Append(c > 1 ? "es" : string.Empty).ToString()); }
                finally { Pool.Free(ref sb); }
            }

        }

        private int PatchAllHarmonyAttributes(Type type, Harmony harmony, BindingFlags? flags = null)
        {
            var patched = 0;


            var types = Assembly.GetExecutingAssembly().GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];

                if (t != type && t.IsClass && t.FullName.Contains(type.FullName))
                {
                    var attributes = Attribute.GetCustomAttributes(t);
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        try
                        {
                            var patch = attributes[j] as HarmonyPatch;
                            if (patch == null) continue;

                            if (string.IsNullOrEmpty(patch?.info?.methodName))
                            {
                                PrintWarning("patch.info.methodName is null/empty!!");
                                continue;
                            }

                            if (patch?.info?.declaringType == null)
                            {
                                PrintWarning("declaringType is null?!: " + ", info: " + (patch?.info?.ToString() ?? string.Empty) + ", declaringType: " + (patch?.info?.declaringType?.ToString() ?? string.Empty));
                                continue;
                            }

                            var originalMethod = patch.info.declaringType.GetMethod(patch.info.methodName, (flags != null && flags.HasValue) ? (BindingFlags)flags : BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                            if (originalMethod == null)
                            {
                                PrintWarning("originalMethod is null!! patch.info.methodName: " + patch.info.methodName + Environment.NewLine + "patch.info.declaringType: " + patch.info.declaringType.FullName);
                                continue;
                            }

                            HarmonyMethod prefix = null;
                            HarmonyMethod postfix = null;

                            var prefixMethod = t.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                            var postFixMethod = t.GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                            if (prefixMethod != null) prefix = new HarmonyMethod(prefixMethod);
                            if (postFixMethod != null) postfix = new HarmonyMethod(postFixMethod);

                            harmony.Patch(originalMethod, prefix, postfix);

                            patched++;
                        }
                        catch (Exception ex) { PrintError(ex.ToString()); }

                    }
                }

            }

            return patched;
        }
        #endregion

        #region Patches
        [HarmonyPatch(typeof(GunTrap), "FireWeapon")]
        private class GunTrapPatch
        {
            private static bool Prefix(GunTrap __instance)
            {
                if (!__instance.UseAmmo())
                    return false;

                Effect.server.Run(__instance.gun_fire_effect.resourcePath, __instance.muzzlePos.position, Vector3.zero);

                for (int index = 0; index < __instance.numPellets; ++index)
                    __instance.FireBullet();

                return false;
            }
        }

        [HarmonyPatch(typeof(AutoTurret), "FireAttachedGun", new[] { typeof(Vector3), typeof(float), typeof(Transform), typeof(BaseCombatEntity) })]
        private class AutoTurretPatch
        {
            private static bool Prefix(Vector3 targetPos, float aimCone, Transform muzzleToUse, BaseCombatEntity target, AutoTurret __instance)
            {
                if (__instance.IsOffline()) return false;

                var attachedWeapon = __instance.GetAttachedWeapon();
                if (attachedWeapon == null) return false;

                attachedWeapon.ServerUse(1f, __instance.gun_pitch);

                Effect.server.Run(attachedWeapon.attackFX.resourcePath, attachedWeapon.MuzzleTransform.position, Vector3.zero);

                return false;
            }
        }

        [HarmonyPatch(typeof(BaseProjectile), "CreateProjectileEffectClientside", new[] { typeof(string), typeof(Vector3), typeof(Vector3), typeof(int), typeof(Connection), typeof(bool), typeof(bool) })]
        private class BaseProjectilePatch
        {
            private static bool Prefix(string prefabName, Vector3 pos, Vector3 velocity, int seed, Connection sourceConnection, bool silenced, bool forceClientsideEffects, BaseProjectile __instance)
            {
                return __instance?.GetOwnerPlayer() != null;
            }
        }

        #endregion

        public bool IsZoneWhitelisted(BaseEntity entity)
        {
            if (ZoneManager == null || !ZoneManager.IsLoaded || entity == null || entity.IsDestroyed || entity?.gameObject == null) return false;
            return instance?.ZoneManager?.Call<bool>("EntityHasFlag", entity, "ESPWhitelist") ?? false;
        }

        public bool IsValid(BaseEntity entity) { return !(entity == null || entity.IsDestroyed || entity.gameObject == null || !entity.gameObject.activeSelf); }

        private class ESPPlayer : MonoBehaviour
        {
            private Transform _plyTransform = null;

            private Transform PlyTransform
            {
                get { return _plyTransform ?? (_plyTransform = GetTransform(player)); }
                set { _plyTransform = value; }
            }

            private Transform GetTransform(BaseEntity entity)
            {
                try { return (entity != null && !entity.IsDestroyed && entity?.gameObject != null && entity.gameObject.activeSelf) ? entity?.transform : null; }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^GetTransform()^ entity: " + (entity?.ShortPrefabName ?? string.Empty) + ", alive: " + ((entity as BaseCombatEntity)?.IsAlive() ?? false) + ", gameObject != null: " + (entity?.gameObject != null) + ", destroyed: " + entity?.IsDestroyed);
                }
                return null;
            }

            private Vector3 GetCenterPoint(BaseEntity entity)
            {
                try { return (entity != null && !entity.IsDestroyed && entity?.gameObject != null && entity.gameObject.activeSelf) ? (entity?.CenterPoint() ?? Vector3.zero) : Vector3.zero; }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^GetCenterPoint()^ entity: " + (entity?.ShortPrefabName ?? string.Empty) + ", alive: " + ((entity as BaseCombatEntity)?.IsAlive() ?? false) + ", gameObject != null: " + (entity?.gameObject != null) + ", destroyed: " + entity?.IsDestroyed);
                }
                return Vector3.zero;
            }

            private BasePlayer player;
            private HashSet<BaseEntity> blockedEntities = new HashSet<BaseEntity>();

            //private readonly int current = 0;

            private bool init = true;

            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                if (player == null)
                {
                    Interface.Oxide.LogError("ESPPlayer added on non-player gameObject!!");
                    Destroy(this);
                    return;
                }
                _plyTransform = GetTransform(player);
                if (PlyTransform == null) Interface.Oxide.LogError("ESPPlayer added on null transform player!");

                if (player.IsAdmin) Interface.Oxide.LogWarning("Awake() on admin player");

                InvokeHandler.InvokeRepeating(this, CheckInvoke, 0.4f, 0.2f);
            }

            private float nextTick = 0;
            private float nextUpdaterStorages = 0;

            //   private Vector3 lastPosition = Vector3.zero;

            private bool IsTimerBlock()
            {
                var time = Time.time;
                if (time < nextTick) return true;
                nextTick = time + 0.185f;
                return false;
            }

            private void UpdateBlockedEntities()
            {
                var time = Time.time;
                if (time < nextUpdaterStorages) return;

                nextUpdaterStorages = time + UnityEngine.Random.Range(0.325f, 0.5f);

                blockedEntities.Clear();

                if (PlyTransform == null) return;

                var userId = player?.userID ?? 0;

                var list = Pool.GetList<BaseEntity>();
                try
                {
                    Vis.Entities(PlyTransform.position, instance.storageRadius, list, blockedLayer, QueryTriggerInteraction.Collide);

                    List<ulong> whitelistIds;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var ent = list[i];
                        if (ent == null || ent.IsDestroyed || ent.gameObject == null) continue;

                        if (instance.whitelists.TryGetValue(ent.net.ID, out whitelistIds) && whitelistIds.Contains(player.userID)) continue;
                        if (!instance.IsBlockEntity(ent, userId)) continue;

                        blockedEntities.Add(list[i]);
                    }
                }
                finally { Pool.FreeList(ref list); }


                if (instance?.whitelists != null)
                {
                    foreach (var entity in blockedEntities)
                    {
                        if (entity == null || entity?.net?.ID == null) continue;
                        List<ulong> outList;
                        if (!instance.whitelists.TryGetValue(entity.net.ID, out outList)) instance.whitelists[entity.net.ID] = new List<ulong>();
                    }
                }


                //blockedEntities.RemoveAll(e => e == null || e.IsDestroyed || e?.gameObject == null || !e.gameObject.activeSelf || e?.net == null || e?.net?.ID == null || e == player || (e?.OwnerID != 0 && e.OwnerID == player?.userID) || instance.whitelists[e.net.ID].Contains(player.userID) || !instance.IsBlockEntity(e, player?.userID ?? 0));

                // if (player.IsAdmin) Interface.Oxide.LogWarning("Blocked ents post remove: " + blockedEntities.Count + ", for player: " + player.displayName);
                if (init)
                {

                    init = false;
                    var removeSB = Pool.Get<StringBuilder>();
                    try 
                    {
                        removeSB.Clear();

                        var isAdmin = player.IsAdmin;

                        foreach (var ent in blockedEntities)
                        {
                            if (ent == null) continue;
                            if (isAdmin) removeSB.AppendLine(ent.ShortPrefabName + " (" + ent?.net?.ID + ") Position: " + (ent?.transform?.position ?? Vector3.zero));
                            instance.DestroyClientEntity(player, ent);
                        }

                        if (removeSB.Length > 0) Interface.Oxide.LogWarning("Removed entities for " + player.displayName + ": " + Environment.NewLine + removeSB.ToString().TrimEnd());
                    }
                    finally { Pool.Free(ref removeSB); }
                }
            }

            private bool IsAFK() { return (player?.IdleTime ?? 0f) > 10; }


            private bool IsChecking { get; set; } = false;

            private IEnumerator CheckBlocked()
            {
                IsChecking = true;
                try 
                {
                    var count = 0;
                    var useCount = blockedEntities.Count <= 10 ? blockedEntities.Count : blockedEntities.Count / 8; 
                    var max = Mathf.Clamp(useCount, 8, 128);

                    foreach (var entity in blockedEntities)
                    {
                        if (entity == null || entity.IsDestroyed || entity?.gameObject == null) continue;

                        if (count >= max)
                        {
                            count = 0;
                            yield return CoroutineEx.waitForSecondsRealtime(0.1f);
                        }

                        Vector3 entityPosition = GetCenterPoint(entity);
                        if (entityPosition == Vector3.zero) continue;

                        if (entity.OwnerID == player.userID || CanVisible(entityPosition) || CanVisible(entityPosition, (PlyTransform?.position ?? Vector3.zero) + new Vector3(0, 0.1f, 0))) instance.SetVisibleEntity(entity, player.userID);

                        count++;
                    }
                }
                finally { IsChecking = false; }
            }

            private void CheckInvoke()
            {
                if (player == null || IsChecking || IsAFK() || IsTimerBlock() || PlyTransform == null || player.IsDead() || player?.net?.connection == null || player.IsSleeping() || player.IsReceivingSnapshot || ServerMgr.Instance == null) return;

                UpdateBlockedEntities();
               
                if (blockedEntities == null || blockedEntities.Count < 1) return;
               
                if (ServerMgr.Instance != null) ServerMgr.Instance.StartCoroutine(CheckBlocked());
            }
          
            public bool ContainsAny(string value, params string[] args)
            {
                for (int i = 0; i < args.Length; i++) { if (value.Contains(args[i])) return true; }

                return false;
            }

            private readonly RaycastHit[] _hits = new RaycastHit[50];
            private bool CanVisible(Vector3 pos, Vector3 plyPos = default(Vector3))
            {
                Array.Clear(_hits, 0, _hits.Length);

                Vector3 pos1 = pos;
                Vector3 pos2 = (plyPos == Vector3.zero) ? (player?.eyes?.position ?? Vector3.zero) : plyPos;

                var length = Physics.RaycastNonAlloc(new Ray(pos1, pos2 - pos1), _hits, instance.storageRadius, visLayer, QueryTriggerInteraction.Collide);


                var objOrder = _hits.OrderBy(h => h.distance);

                foreach (var obj in objOrder)
                {
                    if (obj.collider == null) continue;

                    var ent = obj.GetEntity();
                    if (ent != null)
                    {
                        if (ent.ShortPrefabName != "wall" && !ContainsAny(ent.ShortPrefabName, "foundation", "door", "player", "floor")) continue;

                        return ent == player;
                    }
                }


                return false;
            }
        }

        private float storageRadius;
        private bool adminIgnore;
        private bool clansSupport;

        protected override void LoadDefaultConfig()
        {
            Config["Radius of visibility of drawers"] = storageRadius = GetConfig("Radius of visibility of drawers", 50f);
            Config["Ignoring Admins"] = adminIgnore = GetConfig("Ignoring Admins", true);
            Config["Clan support (Reduces the load if there are players playing together)"] = clansSupport = GetConfig("Clan support (Reduces the load if there are players playing together)", true);

            SaveConfig();
        }

        private T GetConfig<T>(string name, T defaultValue) => Config[name] == null ? defaultValue : (T)Convert.ChangeType(Config[name], typeof(T));

        private static AAESPBlocker instance;
        private readonly Dictionary<BasePlayer, ESPPlayer> players = new Dictionary<BasePlayer, ESPPlayer>();
        private Dictionary<NetworkableId, List<ulong>> whitelists;

        private void Loaded()
        {
            instance = this;
            LoadData();
        }

        private bool init = false;
        private Timer clanTimer = null;

        private void UpdatePlayerClans()
        {
            if (Clans == null || !Clans.IsLoaded) return;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var ply = BasePlayer.activePlayerList[i];
                if (ply == null || !ply.IsConnected) continue;
                var clan = Clans?.Call<string>("GetClanOf", ply.UserIDString);
                if (string.IsNullOrEmpty(clan))
                {
                    string clanOut;
                    if (playerClans.TryGetValue(ply.UserIDString, out clanOut)) playerClans.Remove(ply.UserIDString);
                    continue;
                }
                playerClans[ply.UserIDString] = clan;
            }
        }

        private void OnServerInitialized()
        {
            LoadDefaultConfig();

            CommunityEntity.ServerInstance.StartCoroutine(InitCore());
            UpdatePlayerClans();

            clanTimer = timer.Every(60f, () => UpdatePlayerClans());

        }

        private void Unload()
        {
            try { _harmony?.UnpatchAll(GetType().Name); }
            catch (Exception ex) { PrintError(ex.ToString()); }

            try 
            {
                if (clanTimer != null) clanTimer.Destroy();
                clanTimer = null;
            }
            catch(Exception ex) { PrintError(ex.ToString()); }


            try
            {
                foreach (var p in players)
                    UnityEngine.Object.Destroy(p.Value);
            }
            catch (Exception ex) { PrintError(ex.ToString()); }



            SaveData();
         
        }

        private void CanJoinDeathmatch(BasePlayer player, string arenaName)
        {
            if (player == null) return;
            if (!RemoveESPPlayer(player)) PrintWarning("Did not remove ESP on join DM!!!");
        }

        private void OnLeaveDeathmatch(BasePlayer player, string arenaName)
        {
            if (player == null) return;
            var esp = GetEspPlayer(player);
            if (esp != null) AddEspPlayer(player);
            else PrintWarning("ESP wasn't null on leave dm?");
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity?.net?.ID == null || !init) return;

            if (whitelists != null) whitelists.Remove(entity.net.ID);
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;
            ESPPlayer esp;
            if (players.TryGetValue(player, out esp))
            {
                UnityEngine.Object.Destroy(esp);
                players.Remove(player);
            }
        }

        private void OnPlayerConnected(BasePlayer player) => AddEspPlayer(player);

        private object CanNetworkTo(BaseNetworkable entity, BasePlayer target)
        {
            if (!init || entity == null || target == null || entity?.net?.ID == null) return null;
            if (adminIgnore && target.IsAdmin)
            {
                return Interface.Oxide.CallHook("CanCustomNetworkTo", entity, target);
            }


            var playerSource = entity as BasePlayer;
            if (playerSource != null && playerSource == target)
            {
                return Interface.Oxide.CallHook("CanCustomNetworkTo", entity, target);
            }

            var baseEntity = entity as BaseEntity;
            if (!IsBlockEntity(baseEntity, target.userID))
            {
                return Interface.Oxide.CallHook("CanCustomNetworkTo", entity, target);
            }


            if (playerSource != null)
            {
                var sourceVanish = Vanish?.Call<bool>("IsInvisible", playerSource) ?? false;
                if (sourceVanish)
                {
                    var customNet = Interface.Oxide.CallHook("CanCustomNetworkTo", entity, target);
                    if (target.IsAdmin) PrintWarning("source is vanished, returning customNet");
                    return customNet;
                }
            }

            var cupboard = entity as BuildingPrivlidge;
            if (cupboard?.authorizedPlayers != null && cupboard.authorizedPlayers.Count > 0)
            {
                foreach (var ap in cupboard.authorizedPlayers)
                {
                    if (ap.userid == target.userID)
                    {
                        return Interface.Oxide.CallHook("CanCustomNetworkTo", entity, target);
                    }
                }
            }


            List<ulong> whitelistPlayers;
            if (whitelists.TryGetValue(baseEntity.net.ID, out whitelistPlayers))
            {
                if (!whitelistPlayers.Contains(target.userID))
                {
                    DestroyClientEntity(target, baseEntity, true);
                    return false;
                }
                else
                {
                    return Interface.Oxide.CallHook("CanCustomNetworkTo", entity, target);
                }
            }
            else if (baseEntity.OwnerID == target.userID)
            {
                SetVisibleEntity(baseEntity, target.userID);
                var customNet = Interface.Oxide.CallHook("CanCustomNetworkTo", entity, target);
                if (target.IsAdmin) PrintWarning("ownerID was target ID, returning customNet");
                return customNet;
            }
            return false;
        }

        private readonly HashSet<ulong> _adminIdCache = new HashSet<ulong>();
        private bool IsBlockEntity(BaseEntity entity, ulong userID = 0)
        {
            if (entity == null || entity.IsDestroyed || entity?.gameObject == null) return false;

            var ply = entity as BasePlayer;
            if (ply != null && !ply.IsConnected && ply.IsSleeping()) return true;


            if (entity.OwnerID == 0 || entity.OwnerID == userID) return false;

            var entCheck = entity is Workbench || entity is RepairBench || entity.prefabID == 501605075 /*/shelves/*/ || entity is BoxStorage || entity is ResearchTable || entity is BuildingPrivlidge || entity is SleepingBag || entity is AutoTurret || entity is FlameTurret || entity is GunTrap || entity is Locker || entity is MixingTable || entity.prefabID == 2931042549 || entity.prefabID == 1374462671; //furnace, large furnace
            if (!entCheck || userID == 0) return entCheck;

            if (_adminIdCache.Contains(entity.OwnerID)) return false;

            var plyOwn = BasePlayer.FindByID(entity.OwnerID) ?? BasePlayer.FindSleeping(entity.OwnerID) ?? null;
            if (plyOwn != null && plyOwn.IsAdmin)
            {
                _adminIdCache.Add(entity.OwnerID);
                return false;
            }

            var strOwnId = entity.OwnerID.ToString();
            var plyOwnC = covalence.Players?.FindPlayerById(strOwnId) ?? null;
            if (plyOwnC != null && plyOwnC.IsAdmin)
            {
                _adminIdCache.Add(entity.OwnerID);
                return false;
            }

            return entCheck;
        }

        private void SetVisibleEntity(BaseEntity entity, ulong userID)
        {
            if (entity == null || entity?.net == null || userID == 0) return;
          
            List<ulong> whitelist;
            if (!instance.whitelists.TryGetValue(entity.net.ID, out whitelist)) instance.whitelists[entity.net.ID] = whitelist = new List<ulong>();
            if (!whitelist.Contains(userID)) whitelist.Add(userID);

            if (instance.clansSupport)
            {
                var list = Pool.Get<HashSet<ulong>>();
                try 
                {
                    GetClanMembers2NoAlloc(userID, ref list);
                    foreach (var member in list)
                    {
                        if (!whitelist.Contains(member)) whitelist.Add(member);
                    }
                }
                finally { Pool.Free(ref list); }
            }

            instance.whitelists[entity.net.ID] = whitelist;

            entity.SendNetworkUpdateImmediate();

            var getLock = entity?.GetSlot(BaseEntity.Slot.Lock) ?? null;
            if (getLock != null) getLock.SendNetworkUpdateImmediate();
        }

        [ChatCommand("clearesp")]
        private void cmdClear(BasePlayer player)
        {
            if (player.IsAdmin)
                UnityEngine.Object.FindObjectOfType<BoxStorage>().SendNetworkUpdate();
        }

        private ESPPlayer GetEspPlayer(BasePlayer player)
        {
            if (player == null) return null;
            ESPPlayer espPlayer;
            if (players.TryGetValue(player, out espPlayer)) return espPlayer;
            return AddEspPlayer(player);
        }

        private ESPPlayer AddEspPlayer(BasePlayer player)
        {
            if (player == null || !player.IsConnected || player?.gameObject == null) return null;
            ESPPlayer esp;
            if (players.TryGetValue(player, out esp)) UnityEngine.Object.Destroy(esp);

            esp = player.gameObject.AddComponent<ESPPlayer>();
            players[player] = esp;
            return esp;
        }

        private bool RemoveESPPlayer(BasePlayer player)
        {
            if (player == null) return false;
            ESPPlayer esp;
            if (players.TryGetValue(player, out esp)) players.Remove(player);
            if (esp == null) esp = player?.GetComponent<ESPPlayer>() ?? null;
            if (esp == null) return false;
            UnityEngine.Object.Destroy(esp);
            return true;
        }

        private readonly Dictionary<ulong, HashSet<NetworkableId>> netDestroyed = new Dictionary<ulong, HashSet<NetworkableId>>();

        private void DestroyClientEntity(BasePlayer player, BaseEntity entity, bool cache = false)
        {
            if (player == null || !player.IsConnected || player?.net == null || player?.net?.connection == null || entity == null || entity?.net == null || entity.IsDestroyed) return;
            HashSet<NetworkableId> netIDs = null;
            if (cache && netDestroyed.TryGetValue(player.userID, out netIDs) && netIDs.Contains(entity.net.ID)) return;//already destroyed, performance saver


            var netWrite = Net.sv.StartWrite();



            netWrite.PacketID(Message.Type.EntityDestroy);
            netWrite.EntityID(entity.net.ID);
            netWrite.UInt8((byte)BaseNetworkable.DestroyMode.None);
            netWrite.Send(new SendInfo(player.net.connection)
            {
                priority = Network.Priority.Immediate
            });
            if (cache)
            {
                var netID = entity.net.ID;
                if (netIDs != null) netIDs.Add(netID);
                else
                {
                    netDestroyed[player.userID] = new HashSet<NetworkableId>
                        {
                            netID
                        };
                }
            }
        }

        private IEnumerator InitCore()
        {
            //var objs = UnityEngine.Object.FindObjectsOfType<BoxStorage>();
            int i = 0;
            int lastpercent = -1;
            StopwatchUtils.StopwatchStart("ESPBlocker.InitCore");
            var players = new HashSet<BasePlayer>(BasePlayer.activePlayerList);
            var count = players.Count;
            var watch = Stopwatch.StartNew();
            foreach (var player in players)
            {
                if (player == null || !player.IsConnected || player?.gameObject == null || player.IsDestroyed || (adminIgnore && player.IsAdmin))
                {
                    count--;
                    continue;
                }
                i++;
                var percent = (int)(i / (float)count * 100);
                if (StopwatchUtils.StopwatchElapsedMilliseconds("ESPBlocker.InitCore") > 10 || percent != lastpercent)
                {
                    StopwatchUtils.StopwatchStart("ESPBlocker.InitCore");
                    if (percent != lastpercent)
                    {
                        if (percent % 20 == 0) Puts($"Loading ESPPlayer: {percent}%");
                        lastpercent = percent;
                    }
                    if (Performance.report.frameRate < 100) yield return CoroutineEx.waitForEndOfFrame;
                }
                player.Invoke(() =>
                {
                    AddEspPlayer(player);
                }, UnityEngine.Random.Range(0.2f, 1f));
            }
            watch.Stop();
            Puts(Name + " InitCore() finished in: " + watch.Elapsed.TotalMilliseconds + "ms");

            init = true;
        }

        public static class StopwatchUtils
        {
            private static readonly Dictionary<string, Stopwatch> watches = new Dictionary<string, Stopwatch>();

            /// <summary>
            /// Start Stopwatch
            /// </summary>
            /// <param name="name">KEY</param>
            public static void StopwatchStart(string name) => watches[name] = Stopwatch.StartNew();


            /// <summary>
            /// Get Elapsed Milliseconds
            /// </summary>
            /// <param name="name">KEY</param>
            /// <returns></returns>
            public static long StopwatchElapsedMilliseconds(string name) => watches[name].ElapsedMilliseconds;

            /// <summary>
            /// Remove StopWatch
            /// </summary>
            /// <param name="name"></param>
            public static void StopwatchStop(string name) => watches.Remove(name);

        }

        public void Arrow(BasePlayer player, Vector3 from, Vector3 to) => player?.SendConsoleCommand("ddraw.arrow", 5, Color.magenta, from, to, 0.1f);

        [PluginReference]
        private readonly Plugin Clans;

        [PluginReference]
        private readonly Plugin Vanish;

        [PluginReference]
        private readonly Plugin ZoneManager;

        private List<ulong> GetClanMembers(ulong uid) { return Clans?.Call<List<ulong>>("GetClanMembers", uid) ?? null; }

        private HashSet<string> GetClanMembers2(string uid)
        {
            if (string.IsNullOrEmpty(uid)) throw new ArgumentNullException(nameof(uid));

            var findClan = string.Empty;
            foreach (var kvp in playerClans)
            {
                if (kvp.Key == uid)
                {
                    findClan = kvp.Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(findClan)) return null;

            var members = new HashSet<string>();
            foreach (var kvp in playerClans) if (kvp.Value == findClan) members.Add(kvp.Key);

            return members;
        }

        private HashSet<ulong> GetClanMembers2(ulong uid)
        {
            if (uid == 0) return null;
            var watch = Stopwatch.StartNew();
            var findClan = string.Empty;
            var uidStr = uid.ToString();
            foreach (var kvp in playerClans)
            {
                if (kvp.Key == uidStr)
                {
                    findClan = kvp.Value;
                    break;
                }
            }
            if (string.IsNullOrEmpty(findClan)) return null;
            var members = new HashSet<ulong>();
            foreach (var kvp in playerClans)
            {
                if (kvp.Value == findClan)
                {
                    ulong userID;
                    if (ulong.TryParse(kvp.Key, out userID)) members.Add(userID);
                }
            }
            watch.Stop();
            if (watch.Elapsed.TotalMilliseconds > 1) PrintWarning("GetClanMembers2 took: " + watch.Elapsed.TotalMilliseconds + "ms");
            return members;
        }

        private void GetClanMembers2NoAlloc(ulong uid, ref HashSet<ulong> collection)
        {
            if (uid == 0) throw new ArgumentOutOfRangeException(nameof(uid));
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var watch = Pool.Get<Stopwatch>();
            try 
            {
                watch.Restart();

                collection.Clear();

                var findClan = string.Empty;
                var uidStr = uid.ToString();

                foreach (var kvp in playerClans)
                {
                    if (kvp.Key == uidStr)
                    {
                        findClan = kvp.Value;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(findClan)) return;

                foreach (var kvp in playerClans)
                {
                    if (kvp.Value == findClan)
                    {
                        ulong userID;
                        if (ulong.TryParse(kvp.Key, out userID)) collection.Add(userID);
                    }
                }

                if (watch.Elapsed.TotalMilliseconds > 1) PrintWarning(nameof(GetClanMembers2NoAlloc) + " took: " + watch.Elapsed.TotalMilliseconds + "ms");
            }
            finally { Pool.Free(ref watch); }

          
        }

        private readonly DynamicConfigFile whitelistFile = Interface.Oxide.DataFileSystem.GetFile("ESPBlockerWhitelist");


        private void LoadData()
        {
            try
            {
                var readDic = new Dictionary<NetworkableId, List<ulong>>();
                foreach (var kvp in whitelistFile.ReadObject<Dictionary<string, List<ulong>>>())
                {
                    if (kvp.Value.Count > 0) readDic[new NetworkableId(ulong.Parse(kvp.Key))] = kvp.Value;
                }
                whitelists = readDic;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                whitelists = new Dictionary<NetworkableId, List<ulong>>();
            }
        }

        private void SaveData()
        {
            var saveDic = new Dictionary<string, List<ulong>>();
            foreach (var kvp in whitelists)
            {
                if (kvp.Value.Count > 0) saveDic[kvp.Key.ToString()] = kvp.Value;
            }
            try { whitelistFile.WriteObject(saveDic); }
            catch (Exception ex) { PrintError(ex.ToString()); }
        }
    }
}
