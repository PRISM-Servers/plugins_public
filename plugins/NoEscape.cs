using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("NoEscape", "Calytic (major edits by Shady)", "0.4.5", ResourceId = 0)] //1394
    [Description("Prevent commands while raid/combat is occuring")]
    class NoEscape : RustPlugin
    {
        #region Setup & Configuration

        Dictionary<string, Timer> raidBlocked = new Dictionary<string, Timer>();
        Dictionary<string, Timer> combatBlocked = new Dictionary<string, Timer>();

        List<string> blockTypes = new List<string>()
        {
            "remove",
            "tp",
            "bank",
            "trade",
            "recycle",
            "shop",
            "bgrade",
            "build",
            "repair"
        };

        // COMBAT SETTINGS
        private bool combatBlock;
        private float combatDuration;
        private bool combatOnHitPlayer;
        private bool combatOnTakeDamage;

        // RAID BLOCK SETTINGS
        private bool raidBlock;
        private float raidDuration;
        private float raidDistance;
        private bool blockOnDamage;
        private bool blockOnDestroy;

        // RAID-ONLY SETTINGS
        private bool blockAll; // IGNORES ALL OTHER CHECKS
        private bool ownerBlock;
        private bool cupboardShare;
        private bool friendShare;
        private bool clanShare;
        private bool clanCheck;
        private bool friendCheck;
        private bool raiderBlock;
        private bool raiderFriendShare;
        private bool raiderClanShare;
        private List<string> damageTypes;

        // RAID UNBLOCK SETTINGS
        private bool raidUnblockOnDeath;
        private bool raidUnblockOnWakeup;
        private bool raidUnblockOnRespawn;

        // COMBAT UNBLOCK SETTINGS
        private bool combatUnblockOnDeath;
        private bool combatUnblockOnWakeup;
        private bool combatUnblockOnRespawn;

        private float cacheTimer;

        // MESSAGES
        private bool raidBlockNotify;
        private bool combatBlockNotify;

        private bool useZoneManager;
        private bool zoneEnter;
        private bool zoneLeave;

        private Dictionary<string, List<string>> memberCache = new Dictionary<string, List<string>>();
        private Dictionary<string, string> clanCache = new Dictionary<string, string>();
        private Dictionary<string, List<string>> friendCache = new Dictionary<string, List<string>>();
        private Dictionary<string, DateTime> lastClanCheck = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> lastCheck = new Dictionary<string, DateTime>();

        private Dictionary<string, DateTime> lastFriendCheck = new Dictionary<string, DateTime>();

        private Dictionary<string, DateTime> lastRaidBlock = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> lastCombatBlock = new Dictionary<string, DateTime>();

        private Dictionary<string, DateTime> lastRaidBlockNotification = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> lastCombatBlockNotification = new Dictionary<string, DateTime>();

        [PluginReference]
        Plugin Clans;

        [PluginReference]
        Plugin Friends;

        [PluginReference]
        Plugin ZoneManager;

        [PluginReference]
        Plugin Vanish;

        int blockLayer = LayerMask.GetMask(new string[] { "Player (Server)" });
        List<string> prefabs = new List<string>()
        {
            "door.hinged",
            "door.double.hinged",
            "window.bars",
            "floor.ladder.hatch",
            "floor.frame",
            "wall.frame",
            "shutter",
            "external"
        };

        private List<string> GetDefaultDamageTypes()
        {
            return new List<string>()
            {
                Rust.DamageType.Bullet.ToString(),
                Rust.DamageType.Blunt.ToString(),
                Rust.DamageType.Stab.ToString(),
                Rust.DamageType.Slash.ToString(),
                Rust.DamageType.Explosion.ToString(),
                Rust.DamageType.Heat.ToString(),
            };
        }

        protected override void LoadDefaultConfig()
        {
            Config["VERSION"] = Version.ToString();

            Config["raidBlock"] = true;
            Config["raidDuration"] = 300f; // 5 minutes
            Config["raidDistance"] = 100f;

            Config["blockOnDamage"] = true;
            Config["blockOnDestroy"] = false;

            Config["combatBlock"] = false;
            Config["combatDuration"] = 180f; // 3 minutes
            Config["combatOnHitPlayer"] = true;
            Config["combatOnTakeDamage"] = true;

            Config["ownerBlock"] = true;
            Config["cupboardShare"] = false;
            Config["clanShare"] = false;
            Config["friendShare"] = false;
            Config["raiderBlock"] = false;
            Config["raiderClanShare"] = false;
            Config["raiderFriendShare"] = false;
            Config["blockAll"] = false;
            Config["friendCheck"] = false;
            Config["clanCheck"] = false;

            Config["raidUnblockOnDeath"] = true;
            Config["raidUnblockOnWakeup"] = false;
            Config["raidUnblockOnRespawn"] = true;
            Config["combatUnblockOnDeath"] = true;
            Config["combatUnblockOnWakeup"] = false;
            Config["combatUnblockOnRespawn"] = true;

            Config["damageTypes"] = GetDefaultDamageTypes();
            Config["cacheMinutes"] = 1f;

            Config["useZoneManager"] = false;
            Config["zoneEnter"] = true;
            Config["zoneLeave"] = false;

            Config["raidBlockNotify"] = true;
            Config["combatBlockNotify"] = false;

            Config["blockingPrefabs"] = prefabs;

            Config["VERSION"] = Version.ToString();
        }

        void Loaded()
        {
            LoadMessages();
        }

        void Unload()
        {
            if(useZoneManager) {
                foreach (var zone in zones)
                {
                    ZoneManager.CallHook("EraseZone", zone.Value.zoneid);
                }
            }
        }

        void LoadMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"Raid Blocked Message", "You may not do that while raid blocked ({time})"},
                {"Combat Blocked Message", "You may do that while combat blocked ({time})"},
                {"Raid Block Complete", "You are no longer raid blocked."},
                {"Combat Block Complete", "You are no longer combat blocked."},
                {"Raid Block Notifier", "You are raid blocked for {time}"},
                {"Combat Block Notifier", "You are combat blocked for {time}"},
                {"Unit Seconds", "second(s)"},
                {"Unit Minutes", "minute(s)"},
                {"Prefix", ""}
            }, this);
        }

        void CheckConfig()
        {
            if (Config["VERSION"] == null)
            {
                // FOR COMPATIBILITY WITH INITIAL VERSIONS WITHOUT VERSIONED CONFIG
                ReloadConfig();
            }
            else if (GetConfig<string>("VERSION", "") != Version.ToString())
            {
                // ADDS NEW, IF ANY, CONFIGURATION OPTIONS
                ReloadConfig();
            }
        }

        protected void ReloadConfig()
        {
            Config["VERSION"] = Version.ToString();

            // NEW CONFIGURATION OPTIONS HERE
            Config["blockingPrefabs"] = GetConfig("blockingPrefabs", prefabs);
            Config["cupboardShare"] = GetConfig("cupboardShare", false);
            Config["raidBlockNotify"] = GetConfig("raidBlockNotify", true);
            Config["combatBlockNotify"] = GetConfig("combatBlockNotify", false);

            Config["blockOnDamage"] = GetConfig("blockOnDamage", true);
            Config["blockOnDestroy"] = GetConfig("blockOnDestroy", false);

            Config["raidBlock"] = GetConfig("raidBlock", true);
            Config["raidDuration"] = GetConfig("duration", 300f); // 5 minutes
            Config["raidDistance"] = GetConfig("distance", 100f);

            Config["combatBlock"] = GetConfig("combatBlock", false);
            Config["combatDuration"] = GetConfig("combatDuration", 180f); // 3 minutes
            Config["combatOnHitPlayer"] = GetConfig("combatOnHitPlayer", true);
            Config["combatOnTakeDamage"] = GetConfig("combatOnTakeDamage", true);

            Config["friendShare"] = GetConfig("friendShare", false);
            Config["raiderFriendShare"] = GetConfig("raiderFriendShare", false);
            Config["friendCheck"] = GetConfig("friendCheck", false);
            Config["raidUnblockOnDeath"] = GetConfig("raidUnblockOnDeath", true);
            Config["raidUnblockOnWakeup"] = GetConfig("raidUnblockOnWakeup", false);
            Config["raidUnblockOnRespawn"] = GetConfig("raidUnblockOnRespawn", true);

            Config["combatUnblockOnDeath"] = GetConfig("combatUnblockOnDeath", true);
            Config["combatUnblockOnWakeup"] = GetConfig("combatUnblockOnWakeup", false);
            Config["combatUnblockOnRespawn"] = GetConfig("combatUnblockOnRespawn", true);

            Config["useZoneManager"] = GetConfig("useZoneManager", false);
            Config["zoneEnter"] = GetConfig("zoneEnter", true);
            Config["zoneLeave"] = GetConfig("zoneLeave", false);

            Config["cacheMinutes"] = GetConfig("cacheMinutes", 1f);
            // END NEW CONFIGURATION OPTIONS

            PrintToConsole("Upgrading configuration file");
            SaveConfig();
        }

        void OnServerInitialized()
        {
            foreach (string command in blockTypes)
            {
                permission.RegisterPermission("noescape.raid." + command + "block", this);
                permission.RegisterPermission("noescape.combat." + command + "block", this);
            }

            CheckConfig();

            prefabs = GetConfig("blockingPrefabs", prefabs);

            raidBlock = GetConfig("raidBlock", true);
            raidDuration = GetConfig("raidDuration", 50f);
            raidDistance = GetConfig("raidDistance", 100f);

            blockOnDamage = GetConfig("blockOnDamage", true);
            blockOnDestroy = GetConfig("blockOnDestroy", false);

            combatBlock = GetConfig("combatBlock", false);
            combatDuration = GetConfig("combatDuration", 180f);
            combatOnHitPlayer = GetConfig("combatOnHitPlayer", true);
            combatOnTakeDamage = GetConfig("combatOnTakeDamage", true);

            friendShare = GetConfig("friendShare", false);
            friendCheck = GetConfig("friendCheck", false);
            clanShare = GetConfig("clanShare", false);
            clanCheck = GetConfig("clanCheck", false);
            blockAll = GetConfig("blockAll", false);
            raiderBlock = GetConfig("raiderBlock", false);
            ownerBlock = GetConfig("ownerBlock", true);
            cupboardShare = GetConfig("cupboardShare", false);
            raiderClanShare = GetConfig("raiderClanShare", false);
            raiderFriendShare = GetConfig("raiderFriendShare", false);
            damageTypes = GetConfig<List<string>>("damageTypes", new List<string>());
            raidUnblockOnDeath = GetConfig("raidUnblockOnDeath", true);
            raidUnblockOnWakeup = GetConfig("raidUnblockOnWakeup", false);
            raidUnblockOnRespawn = GetConfig("raidUnblockOnRespawn", true);
            combatUnblockOnDeath = GetConfig("combatUnblockOnDeath", true);
            combatUnblockOnWakeup = GetConfig("combatUnblockOnWakeup", false);
            combatUnblockOnRespawn = GetConfig("combatUnblockOnRespawn", true);
            cacheTimer = GetConfig("cacheMinutes", 1f);

            useZoneManager = GetConfig("useZoneManager", false);
            zoneEnter = GetConfig("zoneEnter", true);
            zoneLeave = GetConfig("zoneLeave", false);

            raidBlockNotify = GetConfig("raidBlockNotify", true);
            combatBlockNotify = GetConfig("combatBlockNotify", false);

            if (clanShare || clanCheck || raiderClanShare)
            {
                if (!plugins.Exists("Clans"))
                {
                    clanShare = false;
                    clanCheck = false;
                    raiderClanShare = false;
                    PrintWarning("Clans not found! All clan options disabled. Cannot use clan options without this plugin. http://oxidemod.org/plugins/clans.2087/");
                }
            }

            if (friendShare || raiderFriendShare)
            {
                if (!plugins.Exists("Friends"))
                {
                    friendShare = false;
                    raiderFriendShare = false;
                    friendCheck = false;
                    PrintWarning("Friends not found! All friend options disabled. Cannot use friend options without this plugin. http://oxidemod.org/plugins/friends-api.686/");
                }
            }

            if (useZoneManager)
            {
                if (!plugins.Exists("ZoneManager"))
                {
                    useZoneManager = false;
                    PrintWarning("ZoneManager not found! All zone options disabled. Cannot use zone options without this plugin. http://oxidemod.org/plugins/zones-manager.739/");
                }
            }
            if (!combatBlock) Unsubscribe(nameof(OnPlayerAttack));
            if (!blockOnDamage) Unsubscribe(nameof(OnEntityTakeDamage));
        }

        #endregion

        #region Oxide Hooks

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitInfo)
        {
            if (hitInfo == null || hitInfo.WeaponPrefab == null || hitInfo.Initiator == null || !IsEntityBlocked(entity))
            {
                return;
            }

            if (hitInfo.Initiator.transform == null)
            {
                return;
            }
            if (hitInfo.Initiator.transform.position == null)
            {
                return;
            }

            if (damageTypes.Contains(hitInfo.damageTypes.GetMajorityDamageType().ToString()))
            {
                StructureAttack(entity, hitInfo.Initiator, hitInfo.WeaponPrefab.ShortPrefabName, hitInfo.HitPositionWorld);
            }
        }

        void OnPlayerAttack(BasePlayer attacker, HitInfo hitInfo)
        {
            if (attacker == null || !attacker.IsConnected || !IsValidPlayer(attacker) || hitInfo == null || hitInfo?.HitEntity == null) return;
            var majDmg = hitInfo?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            var target = (hitInfo?.HitEntity ?? null) as BasePlayer;
            if (!IsValidPlayer(target) || target == attacker || !IsDamageBlocking(majDmg)) return;
            NextTick(() =>
            {
                try
                {
                    if ((hitInfo?.damageTypes?.Total() ?? 0f) <= 0.0f) return;
                    if (target != null && friendCheck)
                    {
                        var friends = getFriends(attacker.UserIDString);
                        if ((friends?.Count ?? 0) > 0 && friends.Contains(target.UserIDString)) return;
                    }

                    if (combatOnTakeDamage && target != null && (target?.IsConnected ?? false) && IsValidPlayer(target))
                    {
                        var canBlockObj = Interface.CallHook("CanBlockOnDmg", attacker, hitInfo);
                        if (canBlockObj == null || ((canBlockObj is bool) && (bool)canBlockObj)) StartCombatBlocking(target.UserIDString);
                    }

                    if (combatOnHitPlayer && attacker != null && (attacker?.IsConnected ?? false) && IsValidPlayer(attacker) && IsValidPlayer(target))
                    {
                        var canBlockObj = Interface.CallHook("CanBlockOnHit", attacker, hitInfo);
                        if (canBlockObj == null || ((canBlockObj is bool) && (bool)canBlockObj)) StartCombatBlocking(attacker.UserIDString);
                    }
                }
                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete OnPlayerAttack NextTick callback^"); }
            });
        }

        object OnPlayerCommand(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null) return null;
           
            var parm = (arg?.FullString ?? "Unknown").Replace("\"", string.Empty).Replace(@"\", "\"");
            var parmLower = parm.ToLower();
            if (parmLower.StartsWith("/tpr") || parmLower.StartsWith("/tpa") || parmLower.StartsWith("/home") || parmLower.StartsWith("/sethome"))
            {
                var canTP = CanTeleport(player)?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(canTP))
                {
                    SendReply(player, canTP);
                    return true;
                }
            }
            return null;
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo hitInfo)
        {
            if (entity == null || hitInfo == null) return;
            try
            {
                if (blockOnDestroy)
                {
                    if (hitInfo == null || hitInfo.WeaponPrefab == null || hitInfo.Initiator == null || !IsEntityBlocked(entity))
                        return;

                    if (damageTypes.Contains(hitInfo.damageTypes.GetMajorityDamageType().ToString()))
                        StructureAttack(entity, hitInfo.Initiator, hitInfo.WeaponPrefab.ShortPrefabName, hitInfo.HitPositionWorld);

                    return;
                }
                var player = entity as BasePlayer;
                if (player == null) return;
                if (raidBlock && raidUnblockOnDeath && IsRaidBlocked(player))
                {
                    timer.In(0.3f, delegate ()
                    {
                        StopBlocking(player.UserIDString);
                    });
                }

                if (combatBlock && combatUnblockOnDeath && IsCombatBlocked(player))
                {
                    timer.In(0.3f, delegate ()
                    {
                        StopBlocking(player.UserIDString);
                    });
                }
            }
            catch(Exception ex) { PrintError(ex.ToString()); }
            
        }

        void OnPlayerSleepEnded(BasePlayer player)
        {
            if (raidBlock && raidUnblockOnWakeup && IsRaidBlocked(player))
            {
                timer.In(0.3f, delegate()
                {
                    StopBlocking(player.UserIDString);
                });
            }

            if (combatBlock && combatUnblockOnWakeup && IsCombatBlocked(player))
            {
                timer.In(0.3f, delegate()
                {
                    StopBlocking(player.UserIDString);
                });
            }
        }

        void OnPlayerRespawned(BasePlayer player)
        {
            if (raidBlock && raidUnblockOnRespawn && IsRaidBlocked(player))
            {
                timer.In(0.3f, delegate()
                {
                    StopBlocking(player.UserIDString);
                });
            }

            if (combatBlock && combatUnblockOnRespawn && IsCombatBlocked(player))
            {
                timer.In(0.3f, delegate()
                {
                    StopBlocking(player.UserIDString);
                });
            }
        }

        #endregion

        #region Block Handling

        void StructureAttack(BaseEntity targetEntity, BaseEntity sourceEntity, string weapon, Vector3 hitPosition)
        {
            if (targetEntity == null || sourceEntity == null) return;
            string source;
            var sourcePlayer = sourceEntity as BasePlayer;
            if (sourcePlayer != null) source = sourcePlayer.UserIDString;
            else
            {
                var ownerID = FindOwner(sourceEntity);
                if (!string.IsNullOrEmpty(ownerID)) source = ownerID;
                else return;
            }
            if (string.IsNullOrEmpty(source)) return;


            var targetID = FindOwner(targetEntity);
            if (!string.IsNullOrEmpty(targetID))
            {
               // var target = covalence.Players.FindPlayerById(targetID);
                List<string> sourceMembers = null;

                if (clanCheck || friendCheck) sourceMembers = getFriends(source);

                if (blockAll)
                {
                    BlockAll(source, targetEntity.transform.position, sourceMembers);
                    return;
                }

                if (ownerBlock) OwnerBlock(source, sourceEntity, targetID, targetEntity.transform.position, sourceMembers);

                if (raiderBlock) RaiderBlock(source, targetID, targetEntity.transform.position, sourceMembers);
            }
        }

        void BlockAll(string source, Vector3 position, List<string> sourceMembers = null)
        {
            StartRaidBlocking(source, position);
            var nearbyTargets = new List<BasePlayer>();
            Vis.Entities<BasePlayer>(position, raidDistance, nearbyTargets, blockLayer);
            if (nearbyTargets.Count > 0)
            {
                for (int i = 0; i < nearbyTargets.Count; i++)
                {
                    var nearbyTarget = nearbyTargets[i];
                    if (nearbyTarget == null) continue;
                    if (ShouldBlockEscape(nearbyTarget.UserIDString, source, sourceMembers)) StartRaidBlocking(nearbyTarget.UserIDString, position);
                }
            }
        }

        private readonly int cupboardMask = UnityEngine.LayerMask.GetMask("Deployed");

        void OwnerBlock(string source, BaseEntity sourceEntity, string target, Vector3 position, List<string> sourceMembers = null)
        {
            var targetMembers = new List<string>();

            if (clanShare || friendShare) targetMembers = getFriends(target);

            var nearbyTargets = new List<BasePlayer>();
            Vis.Entities<BasePlayer>(position, raidDistance, nearbyTargets, blockLayer);
            if (cupboardShare)
            {
                sourceMembers = CupboardShare(target, position, sourceEntity, sourceMembers);
            }
            if (nearbyTargets.Count > 0)
            {
                for(int i = 0; i < nearbyTargets.Count; i++)
                {
                    var nearbyTarget = nearbyTargets[i];
                    if (nearbyTarget == null) continue;
                    if (ShouldBlockEscape(target, source, sourceMembers) && (nearbyTarget.UserIDString == target || targetMembers != null && targetMembers.Contains(nearbyTarget.UserIDString)))
                    {
                        StartRaidBlocking(nearbyTarget.UserIDString, position);
                    }
                }
            }
        }

        List<string> CupboardShare(string owner, Vector3 position, BaseEntity sourceEntity, List<string> sourceMembers = null)
        {
            var nearbyCupboards = new List<BuildingPrivlidge>();
            Vis.Entities<BuildingPrivlidge>(position, raidDistance, nearbyCupboards, cupboardMask);
            if (sourceMembers == null)
            {
                sourceMembers = new List<string>();
            }
            List<string> cupboardMembers = new List<string>();

            foreach (var cup in nearbyCupboards)
            {
                bool ownerOrFriend = false;

                if (owner == cup.OwnerID.ToString())
                {
                    ownerOrFriend = true;
                }

                foreach (var member in sourceMembers)
                {
                    if (member == cup.OwnerID.ToString())
                    {
                        ownerOrFriend = true;
                    }
                }

                if (ownerOrFriend)
                {
                    foreach (var proto in cup.authorizedPlayers)
                    {
                        if (!sourceMembers.Contains(proto.userid.ToString()))
                        {
                            cupboardMembers.Add(proto.userid.ToString());
                        }
                    }
                }
            }

            sourceMembers.AddRange(cupboardMembers);

            return sourceMembers;
        }

        void RaiderBlock(string source, string target, Vector3 position, List<string> sourceMembers = null)
        {
            var targetMembers = new List<string>();

            if ((raiderClanShare || raiderFriendShare) && sourceMembers == null)
                sourceMembers = getFriends(source);

            var nearbyTargets = new List<BasePlayer>();
            Vis.Entities<BasePlayer>(position, raidDistance, nearbyTargets, blockLayer);
            if (nearbyTargets.Count > 0)
            {
                for (int i = 0; i < nearbyTargets.Count; i++)
                {
                    var nearbyTarget = nearbyTargets[i];
                    if (nearbyTarget == null) continue;
                    if (ShouldBlockEscape(target, source, sourceMembers) && (nearbyTarget.UserIDString == target || sourceMembers != null && sourceMembers.Contains(nearbyTarget.UserIDString))) StartRaidBlocking(nearbyTarget.UserIDString, position);
                }
            }
        }

        #endregion

        #region API

        bool IsEscapeBlocked(BasePlayer target)
        {
            if (target == null) return false;
            return IsEscapeBlockedS(target.UserIDString);
        }

        bool IsRaidBlocked(BasePlayer target)
        {
            if (target == null) return false;
            return IsRaidBlockedS(target.UserIDString);
        }

        bool IsCombatBlocked(BasePlayer target)
        {
            if (target == null) return false;
            return IsCombatBlockedS(target.UserIDString);
        }

        bool IsEscapeBlockedS(string target)
        {
            if (string.IsNullOrEmpty(target)) return false;
            Timer outTimer;
            return raidBlocked.TryGetValue(target, out outTimer) || combatBlocked.TryGetValue(target, out outTimer);
        }

        bool IsRaidBlockedS(string target)
        {
            if (string.IsNullOrEmpty(target)) return false;
            Timer outTimer;
            return raidBlocked.TryGetValue(target, out outTimer);
        }

        bool IsCombatBlockedS(string target)
        {
            if (string.IsNullOrEmpty(target)) return false;
            Timer outTimer;
            return combatBlocked.TryGetValue(target, out outTimer);
        }

        bool ShouldBlockEscape(string target, string source, List<string> sourceMembers = null)
        {
            if (target == source)
            {
                if ((ownerBlock || raiderBlock) && (!clanCheck || !friendCheck)) return true;
               // return false;
            }

            if (sourceMembers is List<string> && sourceMembers.Count > 0 && sourceMembers.Contains(target))
                return false;

            return true;
        }

        class RaidZone
        {
            public string zoneid;
            public Vector3 position;
            public Timer timer;

            public RaidZone(string zoneid, Vector3 position)
            {
                this.zoneid = zoneid;
                this.position = position;
            }

            public float Distance(RaidZone zone)
            {
                return Vector3.Distance(position, zone.position);
            }

            public float Distance(Vector3 pos)
            {
                return Vector3.Distance(position, pos);
            }

            public RaidZone ResetTimer()
            {
                if (this.timer is Timer && !this.timer.Destroyed)
                {
                    this.timer.Destroy();
                }

                return this;
            }
        }

        private Dictionary<string, RaidZone> zones = new Dictionary<string, RaidZone>();

        void CreateRaidZone(Vector3 position)
        {
            string zoneid = position.ToString();

            RaidZone zone;
            if (zones.TryGetValue(zoneid, out zone))
            {
                zone.ResetTimer().timer = timer.In(raidDuration, delegate()
                {
                    ZoneManager.CallHook("EraseZone", zoneid);
                    zones.Remove(zoneid);
                });
                return;
            }
            else
            {
                foreach (var nearbyZone in zones)
                {
                    if (nearbyZone.Value.Distance(position) < (raidDistance / 2))
                    {
                        nearbyZone.Value.ResetTimer().timer = timer.In(raidDuration, delegate()
                        {
                            ZoneManager.CallHook("EraseZone", zoneid);
                            zones.Remove(zoneid);
                        });
                        return;
                    }
                }
            }

            ZoneManager.CallHook("CreateOrUpdateZone", zoneid, new string[] {
                "radius",
                raidDistance.ToString()
            }, position);

            zone = new RaidZone(zoneid, position);

            zones.Add(zoneid, zone);

            zone.timer = timer.In(raidDuration, delegate()
            {
                ZoneManager.CallHook("EraseZone", zoneid);
                zones.Remove(zoneid);
            });
        }

        [HookMethod("OnEnterZone")]
        void OnEnterZone(string zoneid, BasePlayer player)
        {
            if (!zoneEnter) return;
            RaidZone outZone;
            if (!zones.TryGetValue(zoneid, out outZone)) return;

            StartRaidBlocking(player.UserIDString, player.transform.position, false);
        }

        [HookMethod("OnExitZone")]
        void OnExitZone(string zoneid, BasePlayer player)
        {
            if (!zoneLeave) return;
            RaidZone outZone;
            if (!zones.TryGetValue(zoneid, out outZone)) return;

            if (IsRaidBlocked(player))
            {
                ClearRaidBlocking(player.UserIDString);
            }
        }

        void StartRaidBlocking(string target, Vector3 position, bool createZone = true)
        {
            var canRaidBlock = Interface.Oxide.CallHook("CanRaidBlock", target, position, createZone);
            if (canRaidBlock != null && (canRaidBlock is bool) && !((bool)canRaidBlock)) return;
            var uID = 0ul;
            BasePlayer player = null;
            if (ulong.TryParse(target, out uID)) player = BasePlayer.FindByID(uID) ?? BasePlayer.FindSleeping(uID) ?? null;
            StopRaidBlocking(target);
            var isVanished = player == null ? false : Vanish?.Call<bool>("IsInvisible", player) ?? false;
            if (isVanished) return;

            lastRaidBlock[target] = DateTime.Now;

          

            SendRaidBlockMessage(target);

            raidBlocked[target] = timer.In(raidDuration, delegate ()
            {
                Timer outTimer;
                if (raidBlocked.TryGetValue(target, out outTimer)) raidBlocked.Remove(target);
                if (raidBlockNotify && player != null && player.IsConnected) SendReply(player, GetPrefix(player.UserIDString) + GetMsg("Raid Block Complete", player.UserIDString));
            });

           // if (player != null) player.MarkHostileFor(raidDuration);

            if (useZoneManager && createZone && (zoneEnter || zoneLeave)) CreateRaidZone(position);
        }

        void SendRaidBlockMessage(string target)
        {
            if (!raidBlockNotify)
                return;

            var send = false;
            System.DateTime lastNotified;
            if (lastRaidBlockNotification.TryGetValue(target, out lastNotified))
            {
                TimeSpan diff = DateTime.Now - lastNotified;
                if (diff.TotalSeconds >= raidDuration)
                {
                    send = true;
                    lastRaidBlockNotification.Remove(target);
                }
            }
            else
                send = true;

            if (send)
            {
                var targetPlayer = BasePlayer.Find(target);
                if (targetPlayer != null)
                {
             //       var getCoolDown = GetCooldownTime(raidDuration);
                    SendReply(targetPlayer, GetPrefix(targetPlayer.UserIDString) + GetMsg("Raid Block Notifier", targetPlayer.UserIDString).Replace("{time}", GetCooldownTime(raidDuration)));
                    lastRaidBlockNotification[target] = DateTime.Now;
                }
            }
        }

        void StartCombatBlocking(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                PrintWarning("StartCombatBlocking called with null/empty target!");
                return;
            }
            //PrintWarning("StartCombatBlocking called: " + target);
          
            try
            {
                StopCombatBlocking(target);
                var targetUID = 0ul;
                if (!ulong.TryParse(target, out targetUID))
                {
                    PrintWarning("StartCombatBlocking had non-ulong target: " + target + " !!!");
                    return;
                }
                var targetPlayer = BasePlayer.FindByID(targetUID) ?? BasePlayer.FindSleeping(targetUID);
                if (targetPlayer == null)
                {
                    PrintWarning("targetPlayer is null on StartCombatBlocking for target: " + target + " !!!");
                    return;
                }

                lastCombatBlock[target] = DateTime.Now;

              


                SendCombatBlockMessage(target);
                
                combatBlocked.Add(target, timer.In(combatDuration, delegate ()
                {
                    //PrintWarning("canceled combat block after duration");
                    combatBlocked.Remove(target);
                    if (combatBlockNotify && targetPlayer.IsConnected) SendReply(targetPlayer, GetPrefix(target) + GetMsg("Combat Block Complete", target));
                    
                }));

                if (!(targetPlayer?.InSafeZone() ?? true)) targetPlayer.MarkHostileFor(combatDuration);

                //PrintWarning("Started combat blocking: " + target);
            }
            catch(Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete StartCombatBlocking^"); }
        }

        void SendCombatBlockMessage(string target)
        {
            if (string.IsNullOrEmpty(target)) return;
            if (!combatBlockNotify)
                return;

            var send = false;
            DateTime lastNofified;
            if (lastCombatBlockNotification.TryGetValue(target, out lastNofified))
            {
                TimeSpan diff = DateTime.Now - lastNofified;
                if (diff.TotalSeconds >= combatDuration)
                {
                    send = true;
                    lastCombatBlockNotification.Remove(target);
                }
            }
            else
                send = true;

            if (send)
            {
                var targetPlayer = BasePlayer.Find(target);
                if (targetPlayer != null)
                {
                   
                    SendReply(targetPlayer, GetPrefix(targetPlayer.UserIDString) + GetMsg("Combat Block Notifier", targetPlayer.UserIDString).Replace("{time}", GetCooldownTime(combatDuration)));
                    lastCombatBlockNotification[target] = DateTime.Now;
                }
            }
        }

        void StopBlocking(string target)
        {
            if (string.IsNullOrEmpty(target)) return;
            if (IsRaidBlockedS(target))
                StopRaidBlocking(target);
            if (IsCombatBlockedS(target))
                StopCombatBlocking(target);
        }

        void StopRaidBlocking(string target)
        {
            if (string.IsNullOrEmpty(target)) return;
            DestroyRaidBlockTimer(target);

            raidBlocked.Remove(target);
        }

        void ClearRaidBlocking(string target)
        {
            if (string.IsNullOrEmpty(target)) return;
            DestroyRaidBlockTimer(target);

            lastRaidBlock.Remove(target);
            lastRaidBlockNotification.Remove(target);
            raidBlocked.Remove(target);
        }

        void DestroyRaidBlockTimer(string target)
        {
            Timer raidTimer;
            if (raidBlocked.TryGetValue(target, out raidTimer) && !(raidTimer?.Destroyed ?? true)) raidTimer.Destroy();
        }

        void StopCombatBlocking(string target)
        {
            if (string.IsNullOrEmpty(target)) return;
            DestroyCombatBlockTimer(target);

            combatBlocked.Remove(target);
        }

        void ClearCombatBlocking(string target)
        {
            if (string.IsNullOrEmpty(target)) return;
            DestroyCombatBlockTimer(target);

            lastCombatBlock.Remove(target);
            lastCombatBlockNotification.Remove(target);
            combatBlocked.Remove(target);
        }

        void DestroyCombatBlockTimer(string target)
        {
            Timer combatTimer;
            if (combatBlocked.TryGetValue(target, out combatTimer) && !(combatTimer?.Destroyed ?? true)) combatTimer.Destroy();
        }

        #endregion

        #region Friend/Clan Integration

        public List<string> getFriends(string player)
        {
            if (string.IsNullOrEmpty(player)) return null;
            var players = new List<string>();

            if (friendShare || raiderFriendShare || friendCheck) players.AddRange(getFriendList(player));

            if (clanShare || raiderClanShare || clanCheck)
            {
                var members = getClanMembers(player);
                if (members != null) players.AddRange(members);
            }
            return players;
        }

        public List<string> getFriendList(string player)
        {
            if (string.IsNullOrEmpty(player)) return null;
            object friends_obj = null;
            DateTime lastFriendCheckPlayer;
            if (lastFriendCheck.TryGetValue(player, out lastFriendCheckPlayer))
            {
                if ((DateTime.Now - lastFriendCheckPlayer).TotalMinutes <= cacheTimer)
                    return friendCache[player];
                else
                {
                    friends_obj = Friends?.CallHook("IsFriendOfS", player);
                    lastFriendCheck[player] = DateTime.Now;
                }
            }
            else
            {
                friends_obj = Friends?.CallHook("IsFriendOfS", player);

                if (friendCache.ContainsKey(player))
                    friendCache.Remove(player);

                lastFriendCheck[player] = DateTime.Now;
            }

            var players = new List<string>();

            if (friends_obj == null) return players;

            string[] friends = friends_obj as string[];

            for (int i = 0; i < friends.Length; i++) players.Add(friends[i]);

            friendCache[player] = players;

            return players;
        }

        public List<string> getClanMembers(string player)
        {
            string tag = null;
            DateTime lastClanCheckPlayer;
            string lastClanCached;
            if (lastClanCheck.TryGetValue(player, out lastClanCheckPlayer) && clanCache.TryGetValue(player, out lastClanCached))
            {
                if ((DateTime.Now - lastClanCheckPlayer).TotalMinutes <= cacheTimer)
                    tag = lastClanCached;
                else
                {
                    tag = Clans.Call<string>("GetClanOf", player);
                    clanCache[player] = tag;
                    lastClanCheck[player] = DateTime.Now;
                }
            }
            else
            {
                tag = Clans.Call<string>("GetClanOf", player);
                clanCache[player] = tag;
                lastClanCheck[player] = DateTime.Now;
            }
            if (string.IsNullOrEmpty(tag)) return null;

            List<string> lastMemberCache;
            if (memberCache.TryGetValue(tag, out lastMemberCache)) return lastMemberCache;

            var clan = GetClan(tag);
            return (clan == null) ? null : CacheClan(clan);
        }

        JObject GetClan(string tag) { return string.IsNullOrEmpty(tag) ? null : Clans?.Call<JObject>("GetClan", tag) ?? null; }
        

        List<string> CacheClan(JObject clan)
        {
            if (clan == null) return null;
            string tag = clan["tag"].ToString();
            List<string> players = new List<string>();
            foreach (string memberid in clan["members"])
            {
                clanCache[memberid] = tag;
                players.Add(memberid);
            }

            memberCache[tag] = players;
            lastCheck[tag] = DateTime.Now;
            return players;
        }

        void OnClanCreate(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            var clan = GetClan(tag);
            if (clan != null) CacheClan(clan);
            else PrintWarning("Unable to find clan after creation: " + tag);
            
        }

        void OnClanUpdate(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            var clan = GetClan(tag);
            if (clan != null) CacheClan(clan);
            else PrintWarning("Unable to find clan after update: " + tag);
            
        }

        void OnClanDestroy(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            if (lastCheck.ContainsKey(tag)) lastCheck.Remove(tag);
            

            if (memberCache.ContainsKey(tag)) memberCache.Remove(tag);
            
        }

        #endregion

        #region Permission Checking & External API Handling

        bool HasPerm(string userid, string perm)
        {
            return permission.UserHasPermission(userid, "noescape." + perm);
        }

        bool CanRaidCommand(string playerID, string command)
        {
            return raidBlock && HasPerm(playerID, "raid." + command + "block") && IsRaidBlockedS(playerID);
        }

        bool CanCombatCommand(string playerID, string command)
        {
            return combatBlock && HasPerm(playerID, "combat." + command + "block") && IsCombatBlockedS(playerID);
        }

        object CanDo(string command, BasePlayer player)
        {
            if (string.IsNullOrEmpty(command) || player == null) return null;
            if (CanRaidCommand(player.UserIDString, command)) return GetRaidMessage(player.UserIDString);
            else if (CanCombatCommand(player.UserIDString, command)) return GetCombatMessage(player.UserIDString);

            return null;
        }

        object OnStructureRepair(BaseCombatEntity entity, BasePlayer player)
        {
            if (entity == null || player == null) return null;
            var result = CanDo("repair", player);
            if (result == null) return null;
            if (result is string)
            {
                if (entity.health < entity.MaxHealth())
                {
                    return null;
                }
                SendReply(player, result.ToString());
                return true;
            }

            return null;
        }

        object CanBuild(Planner plan, Construction prefab)
        {
            if (plan == null || prefab == null) return null;
            var player = plan.GetOwnerPlayer();
            if (player == null) return null;
            var result = CanDo("build", player);
            if (result is string)
            {
                SendReply(player, result.ToString());
                return true;
            }

            return null;
        }

        object CanBank(BasePlayer player)
        {
            return CanDo("bank", player);
        }

        object CanTrade(BasePlayer player)
        {
            return CanDo("trade", player);
        }

        object canRemove(BasePlayer player)
        {
            return CanDo("remove", player);
        }

        object CanShop(BasePlayer player)
        {
            return CanDo("shop", player);
        }

        object CanTeleport(BasePlayer player)
        {
            return CanDo("tp", player);
        }

        object canTeleport(BasePlayer player) // ALIAS FOR MagicTeleportation
        {
            return CanTeleport(player);
        }
        /*/
        object CanRecycle(BasePlayer player)
        {
            return CanDo("recycle", player);
        }/*/

        object CanAutoGrade(BasePlayer player, int grade, BuildingBlock buildingBlock, Planner planner)
        {
            if (CanRaidCommand(player.UserIDString, "bgrade") || CanCombatCommand(player.UserIDString, "bgrade"))
                return -1;
            return null;
        }

        #endregion

        #region Messages

        string GetCooldownTime(float f)
        {
            if (f <= 0) return string.Empty;
            var now = DateTime.Now;
            return ReadableTimeSpan(now.AddSeconds(f) - now);
        }

        public string GetMessage(string player)
        {
            if (IsRaidBlockedS(player))
                return GetRaidMessage(player);
            else if (IsCombatBlockedS(player))
                return GetCombatMessage(player);

            return null;
        }

        public string GetPrefix(string player)
        {
            string prefix = GetMsg("Prefix", player);
            if (!string.IsNullOrEmpty(prefix))
            {
                return prefix + ": ";
            }

            return "";
        }

        public string GetRaidMessage(string player)
        {
            if (string.IsNullOrEmpty(player)) return string.Empty;
            if (raidDuration > 0)
            {
                DateTime lastBlock;
                if (!lastRaidBlock.TryGetValue(player, out lastBlock)) return string.Empty;
                var now = DateTime.Now;
                var timePassed = now - lastBlock;

                if (timePassed.TotalMinutes <= raidDuration)
                {
                    var readSpan2 = ReadableTimeSpan(now.AddSeconds((double)raidDuration - timePassed.TotalSeconds) - now);
                    return GetPrefix(player) + GetMsg("Raid Blocked Message", player).Replace("{time}", readSpan2);
                }
                
            }

            return string.Empty;
        }

        public string GetCombatMessage(string player)
        {
            if (string.IsNullOrEmpty(player)) return string.Empty;
            if (combatDuration > 0)
            {
                DateTime lastBlock;
                if (!lastCombatBlock.TryGetValue(player, out lastBlock)) return string.Empty;
                var now = DateTime.Now;
                var timePassed = now - lastBlock;

                if (timePassed.TotalMinutes <= combatDuration)
                {
                    var readSpan2 = ReadableTimeSpan(now.AddSeconds((double)combatDuration - timePassed.TotalSeconds) - now);
                    return GetPrefix(player) + GetMsg("Combat Blocked Message", player).Replace("{time}", readSpan2);
                }
            }

            return string.Empty;
        }

        #endregion

        #region Utility Methods

        string ReadableTimeSpan(TimeSpan span, string stringFormat = "N0")
        {
            if (span == TimeSpan.MinValue) return string.Empty;
            var str = string.Empty;
            if (span.TotalHours >= 24) str = (int)span.TotalDays + " day" + (span.TotalDays >= 2 ? "s" : string.Empty) + " " + (span.TotalHours - ((int)span.TotalDays * 24)).ToString(stringFormat) + " hour(s)";
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : string.Empty) + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = (span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : string.Empty)) + (span.Seconds < 1 ? string.Empty : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : string.Empty));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? (span.TotalDays.ToString(stringFormat)) + " day" + (span.TotalDays >= 1.5 ? "s" : string.Empty) : (span.TotalHours >= 1.0) ? (span.TotalHours.ToString(stringFormat)) + " hour" + (span.TotalHours >= 1.5 ? "s" : string.Empty) : (span.TotalMinutes >= 1.0) ? (span.TotalMinutes.ToString(stringFormat)) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : string.Empty) : (span.TotalSeconds >= 1.0) ? (span.TotalSeconds.ToString(stringFormat)) + " second" + (span.TotalSeconds >= 1.5 ? "s" : string.Empty) : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : string.Empty);
        }

        string FindOwner(BaseEntity entity)
        {
            if (entity == null) return string.Empty;
            var ownerid = entity.OwnerID;
            return (ownerid == 0) ? string.Empty : ownerid.ToString();
        }

        public bool IsEntityBlocked(BaseCombatEntity entity)
        {
            if (entity == null) return false;
            var block = entity as BuildingBlock;
            if (block != null)
            {
                if (block.grade == BuildingGrade.Enum.Twigs) return false;
                return true;
            }

            var prefabName = entity?.ShortPrefabName ?? string.Empty;
            for(int i = 0; i < prefabs.Count; i++)
            {
                var prefab = prefabs[i];
                if (prefabName.IndexOf(prefab) != -1) return true;
            }
            return false;
        //    return prefabs?.Any(p => prefabName.IndexOf(p) != -1) ?? false;
        }

        bool IsDamageBlocking(Rust.DamageType dt)
        {
            switch (dt)
            {
                case Rust.DamageType.Bullet:
                case Rust.DamageType.Stab:
                case Rust.DamageType.Explosion:
                case Rust.DamageType.ElectricShock:
                case Rust.DamageType.Heat:
                    return true;
            }
            return false;
        }

        bool IsValidPlayer(BasePlayer player) { return player != null && player.gameObject != null && !player.IsDestroyed && !player.IsNpc && player.ShortPrefabName == "player"; }

        T GetConfig<T>(string key, T defaultValue)
        {
            try
            {
                var val = Config[key];
                if (val == null)
                    return defaultValue;
                if (val is List<object>)
                {
                    var t = typeof(T).GetGenericArguments()[0];
                    if (t == typeof(String))
                    {
                        var cval = new List<string>();
                        foreach (var v in val as List<object>)
                            cval.Add((string)v);
                        val = cval;
                    }
                    else if (t == typeof(int))
                    {
                        var cval = new List<int>();
                        foreach (var v in val as List<object>)
                            cval.Add(Convert.ToInt32(v));
                        val = cval;
                    }
                }
                else if (val is Dictionary<string, object>)
                {
                    var t = typeof(T).GetGenericArguments()[1];
                    if (t == typeof(int))
                    {
                        var cval = new Dictionary<string, int>();
                        foreach (var v in val as Dictionary<string, object>)
                            cval.Add(Convert.ToString(v.Key), Convert.ToInt32(v.Value));
                        val = cval;
                    }
                }
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch (Exception ex)
            {
                PrintWarning("Invalid config value: " + key + " (" + ex.Message + ")");
                return defaultValue;
            }
        }

        string GetMsg(string key, object userID = null)
        {
            return lang.GetMessage(key, this, userID == null ? null : userID.ToString());
        }

        #endregion
    }
}
