using System;
using System.Collections.Generic;
using UnityEngine;
using Rust;
using System.Reflection;
using Oxide.Core.Plugins;
using Oxide.Core;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Collections;

namespace Oxide.Plugins
{
    [Info("AttackPlanes", "Shady", "1.0.0", ResourceId = 0)]
    class AttackPlanes : RustPlugin
    {

        #region Fields

        //static float rocketSpread = 2f;
        //static int rocketAmount = 30;
        //static float planeDistance = 10f; //distance before firing
        //static float rocketInterval = 0.1f; //time between projectiles

        [PluginReference]
        Plugin ZoneManager;


        static FieldInfo TimeToTake = typeof(CargoPlane).GetField("secondsToTake", (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic));
        static FieldInfo StartPos = typeof(CargoPlane).GetField("startPos", (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic));
        static FieldInfo EndPos = typeof(CargoPlane).GetField("endPos", (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic));
        static FieldInfo DropPos = typeof(CargoPlane).GetField("dropPosition", (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic));

        private static readonly int groundWorldConstLayer = LayerMask.GetMask("World", "Default", "Terrain", "Construction", "Deployable", "Prevent Building", "Deployed");
        private static readonly int groundWorldLayer = LayerMask.GetMask("World", "Default", "Terrain");
        private static readonly int worldLayer = LayerMask.GetMask("World", "Default");
        private static readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        private static readonly int treeLayer = LayerMask.GetMask("Tree");

        public enum StrikeType { Cannon, Nuke, Rockets };
        
        public Dictionary<ulong, bool> toggleList = new Dictionary<ulong, bool>();

        private List<AttackPlane> allPlanes = new List<AttackPlane>();

        public static AttackPlanes apMain;
        #endregion

        public static Vector3 SpreadVector(Vector3 vec, float rocketSpread = 1.5f) { return Quaternion.Euler(UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f)) * vec; }
        Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

        public static void QuickHurt(BaseCombatEntity entity, float dmg, Rust.DamageType type, BaseEntity attacker = null, bool useProtection = true, bool useKill = false)
        {
            if (entity == null || entity.IsDestroyed || entity?.transform == null) return;
            entity.Hurt(dmg, type, attacker, useProtection);
            return;
            if (entity is BasePlayer)
            {
                entity.Hurt(dmg, type, attacker, useProtection);
                return;
            }
            if (!useProtection && dmg >= (entity?.health ?? 0))
            {
                entity.Kill();
                return;
            }
            var newInfo = new HitInfo(attacker, entity, type, dmg, entity.transform.position);
            if (useProtection) entity.ScaleDamage(newInfo);
            var hp = entity.health;
            entity.health = hp - (newInfo?.damageTypes?.Total() ?? 0f);
            if (!useKill)
            {
                entity.lastDamage = type;
                entity.lastAttacker = attacker;
            }
            if (entity.health <= 0.0)
            {
                if (useKill) entity.Kill();
                else entity.Die();
            }
            else entity.SendNetworkUpdate();
        }

        public static IEnumerator HurtEntities(List<BaseCombatEntity> entities, float dmg, Rust.DamageType type, BaseEntity attacker = null, bool useProtection = true)
        {
            for(int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                if (entity != null) entity.Hurt(dmg, type, attacker, useProtection);
                yield return CoroutineEx.waitForSeconds(0.005f);
            }
        }

        public static IEnumerator HurtEntities(Dictionary<BaseCombatEntity, HitInfo> entities)
        {
            var count = 0;
            var entCount = entities.Count;
            var countMax = (entities.Count >= 2500) ? 3 : 9; //large bases take too long to nuke, but 3 will increase lag. should be a middle ground
            var hurtMax = 10000L;
            var hurtComp = 0L;
            foreach(var kvp in entities)
            {
                if (kvp.Value == null) continue;
                var entity = kvp.Key;
                var info = kvp.Value;
                if (entity == null || entity.IsDestroyed || entity?.transform == null || info == null) continue;
                count++;
                hurtComp++;
                if (hurtComp >= hurtMax)
                {
                    Interface.Oxide.LogWarning("break on HurtEntities, hurtComp >= hurtMax, total count was: " + entities.Count);
                    break; //lag safety
                }
                var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? DamageType.Generic;
                var dmgTotal = info?.damageTypes?.Total() ?? 0f;
                var attacker = info?.Initiator ?? null;
                QuickHurt(entity, dmgTotal, dmgType, attacker, true, true);
                if (count >= countMax || entCount < 5)
                {
                    count = 0;
                    yield return CoroutineEx.waitForSeconds(0.00215f);
                }
            }
        }

        public static void HurtEntities2(Dictionary<BaseCombatEntity, HitInfo> entities)
        {
            if (entities == null || entities.Count < 1) return;
            foreach (var kvp in entities)
            {
                if (kvp.Value == null) continue;
                var entity = kvp.Key;
                var info = kvp.Value;
                if (entity == null || entity.IsDestroyed || entity?.transform == null || info == null) continue;
                var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? DamageType.Generic;
                var dmgTotal = info?.damageTypes?.Total() ?? 0f;
                var attacker = info?.Initiator ?? null;
                QuickHurt(entity, dmgTotal, dmgType, attacker, true, true);
            }
        }


        #region Classes
        class NukeMovement : MonoBehaviour
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
            public float Distance { get { return Vector3.Distance(startPos, endPos); } }
            private float startTime = 0f;
            public float speed = 10f;
            private float lerpTime = 0f;
            private float currentLerpTime = 0f;
            private float journeyLength;

            void Awake()
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

            public void DoDestroy() => GameObject.Destroy(this);

            void Update()
            {
                if (entity == null || entity?.transform == null) return;
                if (startPos == Vector3.zero || endPos == Vector3.zero) return;
                var fracJourney = ((Time.time - startTime) * speed) / journeyLength;
                entity.transform.position = Vector3.Lerp(startPos, endPos, fracJourney);
                entity.SendNetworkUpdateImmediate();
            }

        }

        public class NukeSettings
        {
            public float RadZoneRadiusMin = 85f;
            public float RadZoneRadiusMax = 170f;
            public float RadZoneLifeMin = 90f;
            public float RadZoneLifeMax = 240f;
            public float RadRadius = 400f;
            public float RadDmgRadius = 230f;
            public float TemperatureRadius = 400f;
            public bool CreateFire = true;
            public bool SphereEffect = true;
           // public float DamageScalar = 1.0f;

            public NukeSettings() { }
        }

        private void ExplodeNuke(BaseEntity nukeEnt, BaseCombatEntity attacker = null, NukeSettings nukeSettings = null)
        {
            if (nukeEnt == null) return;
            var nukePos = nukeEnt?.transform?.position ?? Vector3.zero;
            if (!nukeEnt.IsDestroyed) nukeEnt.Kill();
            for (int i = 0; i < 7; i++)
            {
                var rngPos = SpreadVector(nukePos, UnityEngine.Random.Range(0.8f, 2.5f));
                if (i < 7) Effect.server.Run("assets/prefabs/npc/m2bradley/effects/maincannonattack.prefab", rngPos, Vector3.zero);
                if (i < 5) Effect.server.Run("assets/bundled/prefabs/fx/explosions/explosion_01.prefab", rngPos, Vector3.zero);
                if (i < 8) Effect.server.Run("assets/bundled/prefabs/fx/weapons/landmine/landmine_explosion.prefab", SpreadVector(rngPos, UnityEngine.Random.Range(3f, 9f)), Vector3.zero);
                if (i < 3) Effect.server.Run("assets/bundled/prefabs/fx/gas_explosion_small.prefab", SpreadVector(rngPos, 1.5f), Vector3.zero);
                if (i < 3) Effect.server.Run("assets/prefabs/npc/patrol helicopter/effects/rocket_fire.prefab", rngPos, Vector3.zero);
                if (i < 1) Effect.server.Run("assets/prefabs/npc/patrol helicopter/effects/heli_explosion.prefab", SpreadVector(rngPos, 2f), Vector3.zero);
            }
            if (nukeSettings != null)
            {
                if (nukeSettings.SphereEffect)
                {
                    var lerpRadius = UnityEngine.Random.Range(330, 570);
                    var lerpLife = UnityEngine.Random.Range(4f, 6f);
                    var lerpSpeed = UnityEngine.Random.Range(480, 680);
                    var startRadius = UnityEngine.Random.Range(50, 70);
                    for (int i = 0; i < 4; i++)
                    {
                        var sphere = (SphereEntity)GameManager.server.CreateEntity("assets/prefabs/visualization/sphere.prefab", nukePos);
                        if (sphere == null) break;
                        sphere.currentRadius = startRadius;
                        sphere.lerpRadius = lerpRadius;
                        sphere.lerpSpeed = lerpSpeed;
                        sphere.Spawn();
                        InvokeHandler.Invoke(sphere, () => { if (!sphere?.IsDestroyed ?? true) sphere.Kill(); }, lerpLife);
                    }
                }
            }
            
            PrintWarning("explode nuke, did effect & settings/sphere, now starting instance invoke");
            InvokeHandler.Invoke(ServerMgr.Instance, () =>
            {
                PrintWarning("start nuke dmg invoke");
                var dmgList = new List<DamageTypeEntry>();
                var expDmg = new DamageTypeEntry();
                expDmg.amount = 75f;
                expDmg.type = DamageType.Explosion;
                var radDmg = new DamageTypeEntry();
                radDmg.amount = UnityEngine.Random.Range(6f, 12f);
                radDmg.type = DamageType.Radiation;
                dmgList.Add(expDmg);

                DamageUtil.RadiusDamage(attacker, null, nukePos, 100f, 115f, dmgList, Rust.Layers.Mask.Player_Server, true);
                dmgList.Remove(expDmg);
                expDmg = new DamageTypeEntry();
                expDmg.type = DamageType.Explosion;
                expDmg.amount = UnityEngine.Random.Range(90f, 200f);
                Interface.Oxide.LogWarning("exp dmg: " + expDmg.amount);
                var nearConst = new List<BaseCombatEntity>();
                Vis.Entities(nukePos, UnityEngine.Random.Range(230f, 275f), nearConst);
                var nearTrees = new List<TreeEntity>();
                Vis.Entities(nukePos, UnityEngine.Random.Range(280f, 350f), nearTrees, treeLayer);
                nearConst.Distinct();
                nearTrees.Distinct();
                nearTrees = nearTrees.OrderBy(p => Vector3.Distance((p?.transform?.position ?? Vector3.zero), nukePos))?.ToList() ?? null;
                nearConst = nearConst.OrderBy(p => Vector3.Distance((p?.transform?.position ?? Vector3.zero), nukePos))?.ToList() ?? null;
                if (nearTrees != null && nearTrees.Count > 0)
                {
                    Interface.Oxide.LogWarning("Near trees!");
                    for (int i = 0; i < nearTrees.Count; i++)
                    {
                        var tree = nearTrees[i];
                        if (tree == null) continue;
                        var treePos = tree?.transform?.position ?? Vector3.zero;
                        if (treePos == Vector3.zero) continue;
                        var newHit = new HitInfo();
                        newHit.PointStart = treePos;
                        newHit.PointEnd = SpreadVector2(treePos, UnityEngine.Random.Range(1f, 2f));
                        tree.OnKilled(newHit);
                    }
                }
                if (nearConst != null && nearConst.Count > 0)
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    //  List<Stopwatch> stopWatches = new List<Stopwatch>();
                    var hurts = new Dictionary<BaseCombatEntity, HitInfo>();
                    for (int i = 0; i < nearConst.Count; i++)
                    {
                        var entity = nearConst[i];
                        if (entity == null || entity?.transform == null || entity.IsDestroyed || entity.OwnerID == 757) continue;

                        if (entity?.baseProtection == null || entity?.baseProtection?.amounts == null) continue;
                        var fullProtCount = 0;
                        for (int j = 0; j < entity.baseProtection.amounts.Length; j++) if (entity.baseProtection.amounts[j] >= 1.0f) fullProtCount++;
                        if (fullProtCount >= entity.baseProtection.amounts.Length) continue;
                        //  var watch = new Stopwatch();
                        //watch.Start();

                        var entPos = entity?.transform?.position ?? Vector3.zero;
                        var dmg = UnityEngine.Random.Range(625f, 2200f);
                        var dist = Vector3.Distance(entPos, nukePos);
                        var distReduc = dist / UnityEngine.Random.Range(95f, 180f);
                        if (distReduc > 999f) distReduc = 999f;
                        else if (distReduc < 0.85f) distReduc = 0.85f;
                        //     var distReduc = Mathf.Clamp(dist / UnityEngine.Random.Range(80f, 161f), 0.85f, 999f);

                        dmg = (dmg) / distReduc;

                        if (entity is BuildingBlock && !entity.IsVisible(nukePos)) dmg /= UnityEngine.Random.Range(1.15f, 1.36f);
                        if ((entity is DecorDeployable) && !entity.IsVisible(nukePos)) dmg /= UnityEngine.Random.Range(2f, 3f);
                        if ((entity is BasePlayer) && !entity.IsVisible(nukePos)) dmg /= UnityEngine.Random.Range(7.2f, 21f);
                        var inCave = IsInCave(entity);
                        if (inCave) dmg /= UnityEngine.Random.Range(7f, 15f);

                        var spreadCenter = SpreadVector2(entity.CenterPoint(), 1.1f);

                        if (nukeSettings != null && nukeSettings.CreateFire && !inCave)
                        {
                            var fireRNG = UnityEngine.Random.Range(0, 100);
                            if ((fireRNG <= 1))
                            {
                                InvokeHandler.Invoke(ServerMgr.Instance, () =>
                                {
                                    var fire = (FireBall)GameManager.server.CreateEntity("assets/bundled/prefabs/oilfireballsmall.prefab", spreadCenter);
                                    if (fire == null) return;
                                    fire.Spawn();
                                    fire.CancelInvoke(fire.Extinguish);
                                    var rngLife = UnityEngine.Random.Range(fire.lifeTimeMin, fire.lifeTimeMax);
                                    fire.Invoke(() =>
                                    {
                                        if (fire != null && !fire.IsDestroyed) fire.Kill();
                                    }, rngLife);
                                    if (attacker != null)
                                    {
                                        fire.OwnerID = attacker.OwnerID;
                                        fire.creatorEntity = attacker;
                                    }
                                }, UnityEngine.Random.Range(0.3f, 0.8f));
                            }
                        }
                        /*?
                        if (entity is BuildingBlock && (BasePlayer.activePlayerList?.Any(p => p != null && p.transform != null && Vector3.Distance(p.transform.position, spreadCenter) < 60) ?? false)) //check for near players to avoid unneeded effects
                        {
                            var block = (entity as BuildingBlock);
                            var gibFX = "assets/bundled/prefabs/fx/building/" + (block.grade == BuildingGrade.Enum.Twigs ? "thatch" : block.grade == BuildingGrade.Enum.Wood ? "wood" : block.grade == BuildingGrade.Enum.Stone ? "stone" : "metal_sheet") + "_gib.prefab";
                            Effect.server.Run(gibFX, spreadCenter, Vector3.zero);
                        }/*/
                        var info = new HitInfo(attacker, entity, DamageType.Explosion, dmg);
                        hurts[entity] = info;
                        //     QuickHurt(entity, dmg, DamageType.Explosion, attacker);
                        //  watch.Stop();
                        //  stopWatches.Add(watch);
                    }
                    if (hurts != null && hurts.Count > 0)
                    {
                        if (ServerMgr.Instance == null)
                        {
                            PrintWarning("ServerMgr.Instance is null!!");
                        }
                        else
                        {
                            ServerMgr.Instance.StartCoroutine(HurtEntities(hurts));
                            Interface.Oxide.LogWarning("Started Coroutine for HurtEntities");
                        }
                        // CommunityEntity.ServerInstance.StartCoroutine(HurtEntities(hurts));
                   
                    }
                    else PrintWarning("No hurts!");
                    stopWatch.Stop();
                    Interface.Oxide.LogWarning("Entity loop of " + nearConst.Count + " took: " + stopWatch.ElapsedMilliseconds.ToString("0.000") + "ms");
                    var totalTime = stopWatch.ElapsedMilliseconds;
                    var watchSB = new StringBuilder();
                    /*/
                    if (stopWatches.Count > 0)
                    {
                        for (int i = 0; i < stopWatches.Count; i++)
                        {
                            var timeTaken = stopWatches[i].ElapsedMilliseconds;
                            totalTime += timeTaken;
                            watchSB.AppendLine("Stopwatch: " + i + " took: " + timeTaken.ToString("0.000") + "ms");
                        }
                        Interface.Oxide.LogWarning(watchSB.ToString().TrimEnd());
                    }/*/
                    Interface.Oxide.LogWarning("Total time: " + totalTime.ToString("0.000") + "ms");
                }
                if (nukeSettings != null && nukeSettings.RadDmgRadius > 0.0f)
                {
                    dmgList.Add(radDmg);
                    dmgList.Remove(expDmg);
                    var heatDmg = new Rust.DamageTypeEntry();
                    heatDmg.type = DamageType.Heat;
                    heatDmg.amount = UnityEngine.Random.Range(6f, 18f);
                    dmgList.Add(heatDmg);
                    DamageUtil.RadiusDamage(attacker, null, nukePos, nukeSettings.RadDmgRadius, nukeSettings.RadDmgRadius, dmgList, Rust.Layers.Mask.Player_Server, false);
                }
            }, 0.1f);
            if (nukeSettings != null && (nukeSettings.TemperatureRadius > 0.0f || nukeSettings.RadRadius > 0.0f))
            {
                InvokeHandler.Invoke(ServerMgr.Instance, () =>
                {
                    var checkDistance = Math.Max(nukeSettings.TemperatureRadius, nukeSettings.RadRadius);
                    var nearPlayers = BasePlayer.activePlayerList?.Where(p => p != null && p?.transform != null && p?.metabolism != null && Vector3.Distance(p.transform.position, nukePos) <= checkDistance)?.ToList() ?? null;
                    if (nearPlayers != null && nearPlayers.Count > 0)
                    {
                        for (int i = 0; i < nearPlayers.Count; i++)
                        {
                            var player = nearPlayers[i];
                            var pDist = Vector3.Distance(player.transform.position, nukePos);
                            var inCave = IsInCave(player);
                            Interface.Oxide.LogWarning("do rad & temp for player: " + player.displayName);
                            var radMin = (inCave) ? 75f : 200f;
                            var radMax = (inCave) ? 200f : 800f;
                            var oldMin = player.metabolism.temperature.min;
                            if (pDist <= nukeSettings.RadRadius)
                            {
                                player.metabolism.radiation_poison.value = Mathf.Clamp(player.metabolism.radiation_poison.value + UnityEngine.Random.Range(radMin, radMax), player.metabolism.radiation_poison.min, player.metabolism.radiation_poison.max);
                                player.metabolism.radiation_level.value = player.metabolism.radiation_poison.value;
                            }
                            if (pDist <= nukeSettings.TemperatureRadius)
                            {
                                var tempMod = Mathf.Clamp(pDist / 64, 1f, 999f);
                                var tempMin = (inCave) ? 80f : 600f;
                                var tempMax = (inCave) ? 500f : 1200f;
                                player.metabolism.temperature.value = Mathf.Clamp(player.metabolism.temperature.value + (UnityEngine.Random.Range(tempMin, tempMax) / tempMod), player.metabolism.temperature.min, player.metabolism.temperature.max);
                                Interface.Oxide.LogWarning("temp value after nuke: " + player.metabolism.temperature.value);
                                //    var oldMin = player.metabolism.temperature.min;
                                player.metabolism.temperature.min = player.metabolism.temperature.value / UnityEngine.Random.Range(1f, 1.25f);
                            }
                            player.metabolism.SendChangesToClient();
                            InvokeHandler.Invoke(ServerMgr.Instance, () =>
                            {
                                if (player?.metabolism != null) player.metabolism.temperature.min = oldMin;
                            }, UnityEngine.Random.Range(10f, 30f));
                        }
                    }
                }, 0.75f);
            }



            if (nukeSettings != null && nukeSettings.RadZoneRadiusMin > 0.0f && nukeSettings.RadZoneRadiusMax > 0.0f && nukeSettings.RadZoneLifeMin > 0.0f && nukeSettings.RadZoneLifeMax > 0.0f)
            {
                if (apMain.ZoneManager != null && apMain.ZoneManager.IsLoaded)
                {
                    var zoneRadiation = UnityEngine.Random.Range(80f, 220f);
                    string[] args = { "radius", UnityEngine.Random.Range(nukeSettings.RadZoneRadiusMin, nukeSettings.RadZoneRadiusMax).ToString(), "radiation", zoneRadiation.ToString() };
                    var nukeName = UnityEngine.Random.Range(0, 100000) + " NUKE";
                    var newZone = apMain?.ZoneManager?.Call<bool>("CreateOrUpdateZone", nukeName, args, nukePos) ?? false;
                    if (newZone) Interface.Oxide.LogWarning("Created new nuke rad zone");
                    else Interface.Oxide.LogWarning("Did not create new nuke rad zone");
                    InvokeHandler.Invoke(ServerMgr.Instance, () =>
                    {
                        var newRad = Math.Round(zoneRadiation / UnityEngine.Random.Range(1.26f, 1.9f), MidpointRounding.AwayFromZero);
                        Interface.Oxide.LogWarning("new rad: " + newRad + ", old: " + zoneRadiation);
                        string[] updArgs = { "radiation", newRad.ToString() };
                        var updZone = apMain?.ZoneManager?.Call<bool>("CreateOrUpdateZone", nukeName, updArgs, nukePos) ?? false;
                        if (updZone) Interface.Oxide.LogWarning("Upd zone!");
                        else Interface.Oxide.LogWarning("failed to upd zone");
                    }, UnityEngine.Random.Range(45f, 70f));

                    InvokeHandler.Invoke(ServerMgr.Instance, () =>
                    {
                        if (apMain == null || apMain.ZoneManager == null || !apMain.ZoneManager.IsLoaded)
                        {
                            Interface.Oxide.LogWarning("no apmain or zone manager, ap main: " + (apMain != null) + " zm: " + (apMain?.ZoneManager?.IsLoaded ?? false));
                            return;
                        }
                        var delZone = apMain?.ZoneManager?.Call<bool>("EraseZone", nukeName) ?? false;
                        if (delZone) Interface.Oxide.LogWarning("Deleted nuclear ground zero zone");
                        else Interface.Oxide.LogWarning("Did NOT delete nuclear ground zero zone");
                    }, UnityEngine.Random.Range(nukeSettings.RadZoneLifeMin, nukeSettings.RadZoneLifeMax));
                }
            }


            Interface.Oxide.LogWarning("NUKE GROUNED!");
            return;
        }
        HashSet<BaseEntity> nukeRockets = new HashSet<BaseEntity>();
        void LaunchICBM(Vector3 startPosition, Vector3 endPosition)
        {
            var initEndPos = startPosition;
            initEndPos.y += 3000;
            var rocket = LaunchRocket(startPosition, initEndPos, Vector3.zero, 160, 0.05f, globalBroadcast: true);
            if (rocket == null || rocket.IsDestroyed) return;
            rocket.Invoke(() =>
            {
                rocket.SendMessage("InitializeVelocity", (endPosition - rocket.transform.position));
            }, 8f);
            for(int i = 0; i < 9; i++)
            {
                timer.Once(UnityEngine.Random.Range(0.03f, 0.1f), () =>
                {
                    var newRocket = LaunchRocket(SpreadVector2(startPosition, UnityEngine.Random.Range(0.7f, 1.5f)), SpreadVector2(initEndPos, UnityEngine.Random.Range(0.7f, 1.3f)), Vector3.zero, 160, 0.05f, globalBroadcast: true);
                    var endRng = SpreadVector2(endPosition, UnityEngine.Random.Range(0.7f, 1.4f));
                    
                    newRocket.Invoke(() => newRocket.SendMessage("InitializeVelocity", (endRng - newRocket.transform.position)), 8f);
                });
        
            }
           
            nukeRockets.Add(rocket);
        }

        void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null) return;
            var rocket = entity as BaseEntity;
            if (nukeRockets.Contains(rocket))
            {
                nukeRockets.Remove(rocket);
                var ply = BasePlayer.FindByID(rocket.OwnerID) ?? BasePlayer.FindSleeping(rocket.OwnerID);
                PrintWarning("Would explode now");
                //var nukeSet = new NukeSettings();
              //  ExplodeNuke(rocket, ply, nukeSet);
            }
        }

        private BaseEntity LaunchRocket(Vector3 startPos, Vector3 targetPos, Vector3 offset = default(Vector3), float speed = 80, float gravity = 0f, float radiusScalar = 0f, float dmgScalar = 0f, BaseEntity trackEntity = null, bool globalBroadcast = false, bool incen = false)
        {

            var rocket = incen ? "ammo.rocket.fire" : "ammo.rocket.hv";
            var launchPos = startPos;

            ItemDefinition projectileItem = ItemManager.FindItemDefinition(rocket);
            ItemModProjectile component = projectileItem.GetComponent<ItemModProjectile>();
            if (offset != default(Vector3))
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

            Vector3 newDirection = (targetPos - launchPos);

            entity.SendMessage("InitializeVelocity", (newDirection));
            if (globalBroadcast) entity.globalBroadcast = true;
            entity.Spawn();
            if (globalBroadcast) entity.globalBroadcast = true;
            entity.SendNetworkUpdateImmediate(true);

            if (trackEntity != null && !(trackEntity?.IsDestroyed ?? true))
            {
                Action trackInvoke = null;
                trackInvoke = new Action(() =>
                {
                    if ((entity?.IsDestroyed ?? true) || (trackEntity?.IsDestroyed ?? true))
                    {
                        InvokeHandler.CancelInvoke(entity, trackInvoke);
                        return;
                    }
                    entity.SendMessage("InitializeVelocity", (trackEntity.transform.position - entity.transform.position));
                });
                InvokeHandler.InvokeRepeating(entity, trackInvoke, 0.5f, 0.1f);
            }
            return entity;
        }


        public class AttackPlane : MonoBehaviour
        {
            public CargoPlane Plane;
            public Vector3 TargetPosition;
            public int FiredRockets = 0;
            public ulong OwnerID = 0;
            public BasePlayer GetOwnerPlayer() { return OwnerID == 0 ? null : BasePlayer.FindByID(OwnerID) ?? BasePlayer.FindSleeping(OwnerID) ?? null; }
            public StrikeType type = StrikeType.Cannon;
            public NukeSettings nukeSettings = new NukeSettings();
            public float spreadMin = 2.75f;
            public float spreadMax = 5.8f;
            public float fireDistance = 12f;
            public int fireAmount = 24;
            public float rocketInterval = 0.08f;
            public float damageModifier = 1f;
            public float rocketRadiusScalar = 1f;
            

            public void SpawnPlane(CargoPlane plane, Vector3 position, float speed, bool isSquad = false, Vector3 offset = new Vector3())
            {
                if (plane == null) throw new ArgumentNullException();
                enabled = true;                
                Plane = plane;                
                TargetPosition = position;
                Plane.InitDropPosition(position);
                Plane.Spawn();
                if (isSquad)
                {
                    Vector3 SpawnPos = CalculateSpawnPos(offset);
                    StartPos.SetValue(Plane, SpawnPos);
                    EndPos.SetValue(Plane, SpawnPos - new Vector3(0, 0, TerrainMeta.Size.x * 1.1f));
                    Plane.transform.position = SpawnPos;
                    Plane.transform.rotation = new Quaternion(0, 180, 0, 0);
                }
                TimeToTake.SetValue(Plane, Vector3.Distance((Vector3)StartPos.GetValue(Plane), (Vector3)EndPos.GetValue(Plane)) / speed);
                InvokeHandler.InvokeRepeating(this, CheckDistance, 0.5f, 0.1f);
            }
            private Vector3 CalculateSpawnPos(Vector3 offset)
            {
                float mapSize = (TerrainMeta.Size.x / 2) + 150f;
                Vector3 spawnPos = new Vector3();
                spawnPos.x = TargetPosition.x + offset.x;
                spawnPos.z = mapSize + offset.z;
                spawnPos.y = 150 + offset.y;
                return spawnPos;
            }
            private void CheckDistance()
            {
                var currentPos = Plane.transform.position;
                var dist = Vector3.Distance(currentPos, TargetPosition);
           //     Interface.Oxide.LogWarning("currentpos, targetposition distance: " + dist + ", curr pos.y + fire distance: " + (currentPos.y + fireDistance));
                var altPos = currentPos;
                altPos.y = TargetPosition.y;
                var altDist = Vector3.Distance(altPos, TargetPosition);
              //  Interface.Oxide.LogWarning("Alt dist: " + altDist);
                if (altDist <= fireDistance)
                {
                    Interface.Oxide.LogWarning("Fire dist");
                    FireRockets();
                    InvokeHandler.CancelInvoke(this, CheckDistance);
                }
                /*/
                if (Vector3.Distance(currentPos, TargetPosition) < (currentPos.y + fireDistance))
                {
                    FireRockets();
                    InvokeHandler.CancelInvoke(this, CheckDistance);
                }/*/
            }
            private void FireRockets()
            {
                if (InvokeHandler.IsInvoking(this, SpreadRockets)) InvokeHandler.CancelInvoke(this, SpreadRockets);
                InvokeHandler.InvokeRepeating(this, SpreadRockets, 0.001f, rocketInterval);
            }
            private void SpreadRockets()
            {
                if (FiredRockets >= fireAmount)
                {
                    InvokeHandler.CancelInvoke(this, SpreadRockets);
                    return;
                }
                FiredRockets++;
                if (type == StrikeType.Cannon) LaunchCannon(TargetPosition);
                if (type == StrikeType.Rockets) LaunchRocket(TargetPosition);
                if (type == StrikeType.Nuke)
                {
                    Interface.Oxide.LogWarning("NUKE");
                    LaunchNuke(TargetPosition);
                    FiredRockets = fireAmount;
                }

            }

            private void LaunchCannon(Vector3 targetPos)
            {
                if (targetPos == Vector3.zero) return;
                var launchPos = Plane?.transform?.position ?? Vector3.zero;
                if (launchPos == Vector3.zero) return;
                var modLaunch = SpreadVector(launchPos, UnityEngine.Random.Range(2.35f, 3.4f));
           //     Interface.Oxide.LogWarning("start launch: " + modLaunch);
         //       Interface.Oxide.LogWarning("aim cone: " + modLaunch);
                Effect.server.Run("assets/prefabs/npc/m2bradley/effects/maincannonattack.prefab", modLaunch, Vector3.zero);
                var modAdj = modLaunch;
                modAdj.y -= UnityEngine.Random.Range(3f, 4f);
                var shell = GameManager.server.CreateEntity("assets/prefabs/npc/m2bradley/maincannonshell.prefab", modAdj);
                var newDir = (targetPos - modAdj);
                newDir = SpreadVector(newDir, UnityEngine.Random.Range(spreadMin, spreadMax));
                shell.SendMessage("InitializeVelocity", newDir);
                shell.creatorEntity = GetOwnerPlayer();
                shell.OwnerID = OwnerID;
                shell.Spawn();
            }
            

           

            private void LaunchNuke(Vector3 targetPos)
            {
                if (targetPos == Vector3.zero) return;
                var launchPos = Plane?.transform?.position ?? Vector3.zero;
                if (launchPos == Vector3.zero) return;
                var modLaunch = SpreadVector(launchPos, UnityEngine.Random.Range(0.2f, 0.8f));
                var nukePrefab = "assets/prefabs/deployable/water catcher/water_catcher_small.prefab";
       //         var nukePrefab = "assets/bundled/prefabs/static/hobobarrel_static.prefab";
                var nukeEnt = GameManager.server.CreateEntity(nukePrefab, modLaunch, Quaternion.identity);
                nukeEnt.OwnerID = 757;
                Interface.Oxide.LogWarning("net limit before: " + nukeEnt.globalBroadcast);
                nukeEnt.globalBroadcast = true;
                nukeEnt.Spawn();
                Interface.Oxide.LogWarning("nuke pre rot: " + nukeEnt.transform.rotation);
             //   nukeEnt.transform.rotation = new Quaternion(-0.5f, -0.5f, -0.5f, -0.5f);
               // nukeEnt.transform.rotation = Quaternion.Euler(Plane?.transform?.eulerAngles ?? Vector3.zero);
                Interface.Oxide.LogWarning("nuke after rot: " + nukeEnt.transform.rotation);
                Interface.Oxide.LogWarning("do launch nuke, nukeent: " + nukeEnt.transform.position);
                var startPos = nukeEnt?.transform?.position ?? Vector3.zero;
                var endPos = Vector3.zero;
         //       RaycastHit rayInfo;
                var rayPos = startPos;
                rayPos.y += 300;
                var hits = UnityEngine.Physics.RaycastAll(new Ray(rayPos, Vector3.down), modLaunch.y + 400f, groundWorldConstLayer);
                var hitDistances = new Dictionary<Vector3, float>();
             //   var hitDistances = new List<float>();
                if (hits == null || hits.Length < 1) Interface.Oxide.LogWarning("NO HITS!");
                else
                {
                    for(int i = 0; i < hits.Length; i++)
                    {
                        var hit = hits[i];
                        var hitEnt = hit.GetEntity();
                        if (hitEnt != null && hitEnt.ShortPrefabName == nukeEnt.ShortPrefabName) continue;
                        if (hit.point != Vector3.zero)
                        {
                            var dist = Vector3.Distance(hit.point, startPos);
                            hitDistances[hit.point] = dist;
                         //   hitDistances.Add(dist);
                        }
                  //      endPos = hit.point;
                    }
                }
                if (hitDistances.Count < 1)
                {
                    Interface.Oxide.LogWarning("no hit dists!");
                    return;
                }
                var valMin = hitDistances.Values.Min();
                endPos = hitDistances.Where(p => p.Value == valMin)?.FirstOrDefault().Key ?? Vector3.zero;
             //   endPos = hits?.Where(p => Vector3.Distance(p.point, startPos).ToString() == hitDistances.Min().ToString())?.FirstOrDefault().point ?? Vector3.zero;
                
                /*/
                if (UnityEngine.Physics.Raycast(new Ray(rayPos, Vector3.down), out rayInfo, modLaunch.y + 400f, groundWorldConstLayer))
                {
                    endPos = rayInfo.point;
                    Interface.Oxide.LogWarning("ray hit: " + rayInfo.GetEntity()?.ShortPrefabName + ", " + rayInfo.collider?.sharedMaterial?.name);
                }/*/
           //     if (UnityEngine.Physics.Raycast(new Ray(startPos, Vector3.down), out rayInfo, modLaunch.y + 300f, groundWorldLayer)) endPos = rayInfo.point;
                if (endPos == Vector3.zero)
                {
                    nukeEnt.Kill();
                    Interface.Oxide.LogWarning("BAD ENDPOS");
                    return;
                }
                Interface.Oxide.LogWarning("end pos: " + endPos);
                var nukeClass = nukeEnt.gameObject.AddComponent<NukeMovement>();
              //  var nukeClass = nukeEnt.GetComponent<NukeMovement>();
                nukeClass.speed = UnityEngine.Random.Range(32f, 64f);
                nukeClass.endPos = endPos;
                //nukeEnt.GetComponent<NukeClass>().endPos = endPos;
                var attacker = GetOwnerPlayer();
                Action nukeAct = null;
                nukeAct = new Action(() =>
                {
                    if (nukeEnt == null || nukeEnt.IsDestroyed)
                    {
                        InvokeHandler.CancelInvoke(nukeEnt, nukeAct);
                        return;
                    }
                    var nukePos = nukeEnt?.transform?.position ?? Vector3.zero;
                    if (Vector3.Distance(nukePos, endPos) <= UnityEngine.Random.Range(55f, 75f)) //explode a bit off the ground for more realism
                    {

                        InvokeHandler.CancelInvoke(nukeEnt, nukeAct);
                        apMain?.ExplodeNuke(nukeEnt, attacker, nukeSettings);
                    //    Interface.Oxide.LogWarning("NUKE GROUNED!");
                        return;
                    }
                });
                InvokeHandler.InvokeRepeating(nukeEnt, nukeAct, 0.05f, 0.05f);
            }
            
            private void LaunchRocket(Vector3 targetPos)
            {  
                var launchPos = Plane.transform.position;
                
                ItemDefinition projectileItem = ItemManager.FindItemDefinition("ammo.rocket.basic");
                ItemModProjectile component = projectileItem.GetComponent<ItemModProjectile>();

                BaseEntity entity = GameManager.server.CreateEntity(component.projectileObject.resourcePath, launchPos, new Quaternion(), true);

                TimedExplosive rocketExplosion = entity.GetComponent<TimedExplosive>();
                ServerProjectile rocketProjectile = entity.GetComponent<ServerProjectile>();

                rocketProjectile.speed = 90;
                rocketProjectile.gravityModifier = 0;
                rocketExplosion.timerAmountMin = 60;
                rocketExplosion.timerAmountMax = 60;
                rocketExplosion.minExplosionRadius *= rocketRadiusScalar;
                rocketExplosion.explosionRadius *= rocketRadiusScalar;
                for (int i = 0; i < rocketExplosion.damageTypes.Count; i++)
                    rocketExplosion.damageTypes[i].amount *= damageModifier;

                Vector3 newDirection = (targetPos - launchPos);                

                entity.SendMessage("InitializeVelocity", (newDirection));
                entity.Spawn();
                if (this.OwnerID != 0)
                {
                    entity.creatorEntity = GetOwnerPlayer();
                    entity.OwnerID = this.OwnerID;
                }
            }           
        }
        private CargoPlane CreatePlane() => (CargoPlane)GameManager.server.CreateEntity(cargoPlanePrefab, new Vector3(), new Quaternion(), true);
        #endregion

        #region Oxide Hooks
        void Init() => apMain = this;
        void Unload()
        {
            DestroyAllPlanes();      
        }

        void OnEntitySpawned(BaseEntity entity)
        {
            if (entity != null && (entity is SupplyDrop)) CheckStrikeDrop(entity as SupplyDrop);
        }
        #endregion


        #region Core Functions       
        static bool IsInCave(BaseEntity entity)
        {
            if (entity == null || entity?.transform == null || entity.IsDestroyed) return false;
            var targets = Physics.RaycastAll(new Ray(entity.transform.position, Vector3.up), 250, worldLayer);
            if (targets == null || targets.Length < 1) return false;
            for(int i = 0; i < targets.Length; i++)
            {
                var hit = targets[i];
                var collider = (hit.collider ?? null) as MeshCollider;
                if (collider == null) continue;
                if (collider.sharedMesh.name.StartsWith("cave_") || collider.sharedMesh.name.StartsWith("rock_")) return true;
            }
            return false;
        }

        static bool IsOnGround(BaseEntity entity)
        {
            if (entity == null || (entity?.IsDestroyed ?? true)) return false;
            var ray = new Ray((entity?.transform?.position ?? Vector3.zero), Vector3.down);
            RaycastHit hit;
            if (UnityEngine.Physics.SphereCast(ray, 1f, out hit, 0.75f, groundWorldConstLayer)) if (hit.GetEntity() != null || hit.GetCollider() != null) return true;
            return false;
        }

        public const string cargoPlanePrefab = "assets/prefabs/npc/cargo plane/cargo_plane.prefab";    
        
        private void DestroyPlane(AttackPlane plane)
        {
            if (plane == null)
            {
                PrintWarning("destroyPlane call but plane is already null!");
                return;
            }
            PrintWarning("destroyPlane called!!");
            if (allPlanes.Contains(plane))
            {
                if (plane?.Plane != null && !plane.Plane.IsDestroyed) plane.Plane.Kill();
                UnityEngine.Object.Destroy(plane);
            }
            if (allPlanes.Contains(plane)) allPlanes.Remove(plane);
        }

        private void DestroyAllPlanes()
        {
            for (int i = 0; i < allPlanes.Count; i++) DestroyPlane(allPlanes[i]);
        }

        public bool CheckStrikeDrop(SupplyDrop drop)
        {
            if (drop == null) return false;
            bool istrue = false;
            foreach (var plane in allPlanes)
            {
                if (plane.Plane == null) continue;
                float dropDistance = ((Vector3.Distance(plane.Plane.transform.position, drop.transform.position)));
                if (dropDistance <= 130 && !(drop?.IsDestroyed ?? true)) drop.Kill();
                istrue = true;
            }
            return istrue;
        }

        private void MassSet(Vector3 position, Vector3 offset, int speed = -1, float dmgScalar = 1f, float radiusScalar = 1f, ulong OwnerID = 0, StrikeType type = StrikeType.Cannon)
        {
            if (speed == -1) speed = 100;
            CargoPlane plane = CreatePlane();
            if (plane == null || plane.IsDestroyed)
            {
                PrintWarning("plane is null or destroyed on MassSet!!!!");
                return;
            }

            var AttackPlane = plane.gameObject.AddComponent<AttackPlane>();
            AttackPlane.type = type;
            allPlanes.Add(AttackPlane);
            AttackPlane.OwnerID = OwnerID;
            AttackPlane.rocketRadiusScalar = radiusScalar;
            AttackPlane.damageModifier = dmgScalar;
            if (type == StrikeType.Rockets) AttackPlane.fireDistance = 32;
            AttackPlane.SpawnPlane(plane, position, speed, true, offset);
            
            var removePlane = ((Vector3.Distance((Vector3)StartPos.GetValue(plane), (Vector3)DropPos.GetValue(plane)) / speed) + 55);
            Interface.Oxide.LogWarning("will destroy plane in: " + removePlane);
            plane.Invoke(() =>
            {
                PrintWarning("invoked destroyplane");
                DestroyPlane(AttackPlane);
            }, removePlane);
        }

        private void DoNuke(Vector3 position, Vector3 offset = default(Vector3), bool squad = false, int speed = -1, ulong OwnerID = 0, float radDmgRadius = -1f, float radRadius = -1f, float tempRadius = -1f, bool doFire = true, float radZoneMin = -1f, float radZoneMax = -1f)
        {
            if (speed <= 0) speed = 100;
            var plane = CreatePlane();
            if (plane == null) return;
            var atkPlane = plane.gameObject.AddComponent<AttackPlane>();
            atkPlane.type = StrikeType.Nuke;
            allPlanes.Add(atkPlane);
            atkPlane.OwnerID = OwnerID;
            atkPlane.nukeSettings.CreateFire = doFire;
            if (radDmgRadius != -1f) atkPlane.nukeSettings.RadDmgRadius = radDmgRadius;
            if (radRadius != -1f) atkPlane.nukeSettings.RadRadius = radRadius;
            if (tempRadius != -1f) atkPlane.nukeSettings.TemperatureRadius = tempRadius;
            if (radZoneMin != -1f) atkPlane.nukeSettings.RadZoneLifeMin = radZoneMin;
            if (radZoneMax != -1f) atkPlane.nukeSettings.RadZoneLifeMax = radZoneMax;
            atkPlane.SpawnPlane(plane, position, speed, squad, offset);

            var removePlane = ((Vector3.Distance((Vector3)StartPos.GetValue(plane), (Vector3)DropPos.GetValue(plane)) / speed) + 15);
            Interface.Oxide.LogWarning("will destroy nuke plane in: " + removePlane);
            timer.Once(removePlane, () => DestroyPlane(atkPlane));
        }

        private void DoCannons(Vector3 position, Vector3 offset = default(Vector3), bool squad = false, int speed = -1, ulong OwnerID = 0, int amountToFire = -1, float interval = -1f, float spreadMin = -1f, float spreadMax = -1f)
        {
            if (speed <= 0) speed = 100;
            var plane = CreatePlane();
            if (plane == null) return;
            var atkPlane = plane.gameObject.AddComponent<AttackPlane>();
            atkPlane.type = StrikeType.Cannon;
            allPlanes.Add(atkPlane);
            atkPlane.OwnerID = OwnerID;
            if (amountToFire != -1) atkPlane.fireAmount = amountToFire;
            if (spreadMin != -1f) atkPlane.spreadMin = spreadMin;
            if (spreadMax != -1f) atkPlane.spreadMax = spreadMax;
            if (interval != -1f) atkPlane.rocketInterval = interval;
            atkPlane.SpawnPlane(plane, position, speed, squad, offset);

            var removePlane = ((Vector3.Distance((Vector3)StartPos.GetValue(plane), (Vector3)DropPos.GetValue(plane)) / speed) + 15);
            Interface.Oxide.LogWarning("will destroy cannon plane in: " + removePlane);
            timer.Once(removePlane, () => DestroyPlane(atkPlane));
        }

        #endregion

        #region Callstrike Functions        

        private void callStrike(Vector3 position, int speed = -1, ulong OwnerID = 0, StrikeType type = StrikeType.Cannon)
        {
            if (speed == -1) speed = 70;

            Puts(string.Format(lang.GetMessage("calledTo", this), position.ToString()));

            CargoPlane plane = CreatePlane();
            if (plane == null)
            {
                PrintWarning("Create plane was null!");
                return;
            }
            var AttackPlane = plane.gameObject.AddComponent<AttackPlane>();
            if (AttackPlane == null)
            {
                Puts("plane null after add?!");
                return;
            }
            AttackPlane.type = type;
            allPlanes.Add(AttackPlane);
            AttackPlane.OwnerID = OwnerID;
            AttackPlane.SpawnPlane(plane, position, speed);

            var removePlane = ((Vector3.Distance((Vector3)StartPos.GetValue(plane), (Vector3)DropPos.GetValue(plane)) / speed) + 15);
            InvokeHandler.Invoke(plane, () => DestroyPlane(AttackPlane), removePlane);
        }
        private void massStrike(Vector3 position, ulong OwnerID = 0, string type = "cannons", int speed = -1, float dmgScalar = 1f, float radiusScalar = 1f)
        {
            type = type.ToLower();
            if (type != "cannons" && type != "nuke" && type != "rockets") return;
            Puts(lang.GetMessage("calledTo", this), position.ToString());
            var useType = (type == "cannons") ? StrikeType.Cannon : (type == "nuke") ? StrikeType.Nuke : StrikeType.Rockets;
            MassSet(position, new Vector3(0, 0, 0), speed, dmgScalar, radiusScalar, OwnerID, useType);
            MassSet(position, new Vector3(-70, 0, 80), speed, dmgScalar, radiusScalar, OwnerID, useType);
            MassSet(position, new Vector3(70, 0, 80), speed, dmgScalar, radiusScalar, OwnerID, useType);
        }       

        private void callRandomStrike(bool single)
        {
            float mapSize = (TerrainMeta.Size.x / 2) - 600f;

            float randomX = UnityEngine.Random.Range(-mapSize, mapSize);
            float randomY = UnityEngine.Random.Range(-mapSize, mapSize);

            Vector3 pos = new Vector3(randomX, 0f, randomY);

            if (single)callStrike(pos);
            if (!single) massStrike(pos);            
        }  
  
        private bool isAttackPlane(CargoPlane plane)
        {
            return (plane == null) ? false : plane?.GetComponent<AttackPlane>() != null;
        }

        #endregion
   

        [ChatCommand("ap")]
        private void chatsquadStrike(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 0)
            {
                SendReply(player, "No args!");
                return;
            }
            var arg0Lower = args[0].ToLower();
            if (arg0Lower != "strafe" && arg0Lower != "cannon" && arg0Lower != "nuke" && arg0Lower != "icbm")
            {
                SendReply(player, "Bad args!");
                return;
            }
            if (arg0Lower == "icbm")
            {
                PrintWarning("nuke icbm");
                LaunchICBM(new Vector3(-545, 25, 504), player.transform.position);
                SendReply(player, "nuke icbm");
                return;
            }
            if (arg0Lower == "nuke")
            {
                PrintWarning("nuke");
                MassSet(player.transform.position, new Vector3(0, UnityEngine.Random.Range(425f, 500f), 0), 115, 1f, 1f, 0, StrikeType.Nuke);
                SendReply(player, "Nuke");
            }
            else
            {
                PrintWarning("strafe");
                massStrike(player.transform.position, player.userID);
            }
            
        }
    }
}
