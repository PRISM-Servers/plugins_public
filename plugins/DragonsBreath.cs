using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Oxide.Plugins
{
    [Info("DragonsBreath", "Shady", "1.0.2", ResourceId = 0)]
    internal class DragonsBreath : RustPlugin
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
        public Hash<BasePlayer, bool> doDragonsBreath = new Hash<BasePlayer, bool>();
        public Hash<ulong, float> flamestart = new Hash<ulong, float>();
        private readonly int playerColl = LayerMask.GetMask(new string[] { "Player (Server)" });
        private readonly int blockColl = LayerMask.GetMask(new string[] { "Construction" });
        private readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        private readonly int deployColl = LayerMask.GetMask(new string[] { "Deployable", "Deployed" });
        private const int flamerColl = 1084427009;
        private const int c4Coll = 2230528;
        private const string BIG_FLAME_FX = "assets/prefabs/ammo/arrow/fire/fireexplosion.prefab";

        private void Init()
        {
            permission.RegisterPermission("dragonsbreath.enabled", this);
            permission.RegisterPermission("dragonsbreath.napalm", this);
            permission.RegisterPermission("dragonsbreath.water", this);
            UpdateHooks(false);
        }

        private void OnServerInitialized()
        {
            UpdateHooks(AnyDragonsBreath());
            timer.Every(45f, () => UpdateHooks(AnyDragonsBreath()));
        }

        private bool AnyDragonsBreath()
        {
            var val = false;
            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var ply = BasePlayer.activePlayerList[i];
                if (ply != null && ply.IsConnected && (permission.UserHasPermission(ply.UserIDString, "dragonsbreath.enabled") || permission.UserHasPermission(ply.UserIDString, "dragonsbreath.napalm") || permission.UserHasPermission(ply.UserIDString, "dragonsbreath.water")))
                {
                    val = true;
                    break;
                }
            }
            return val;
        }

        //  bool AnyDragonsBreath() { return doDragonsBreath?.Any(p => p.Value == true) ?? false; }

        private readonly bool? lastUpdateState;

        private void UpdateHooks(bool state)
        {
            if (lastUpdateState.HasValue && ((bool)lastUpdateState) == state) return;
            if (state)
            {
                Subscribe(nameof(OnWeaponFired));
                Subscribe(nameof(OnEntitySpawned));
                Subscribe(nameof(OnPlayerAttack));
            }
            else
            {
                Unsubscribe(nameof(OnWeaponFired));
                Unsubscribe(nameof(OnEntitySpawned));
                Unsubscribe(nameof(OnPlayerAttack));
            }
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || entity == null) return;
            var name = entity?.ShortPrefabName ?? "Unknown";
            //    var attacker = info?.Initiator as BasePlayer ?? null;
            List<BaseEntity> fbList;
            if (entFireball.TryGetValue(entity, out fbList))
            {
                for(int i = 0; i < fbList.Count; i++)
                {
                    var fireballKine = fbList[i];
                    if (fireballKine == null || (fireballKine?.IsDestroyed ?? true)) continue;
                    var rigid = fireballKine?.GetComponent<Rigidbody>() ?? null;
                    if (fireballKine != null && !(fireballKine?.IsDestroyed ?? true))
                    {
                        if (rigid != null)
                        {
                            rigid.isKinematic = false;
                            
                      //      fireballKine.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                            //PrintWarning("no longer kinematic!");
                        }
                        else PrintWarning("rigid null!");
                    }
                    else PrintWarning("fireball kine shit!");
                }
            }
            var napalm = entity?.GetComponent<FireBall>() ?? null;
            if (napalm != null && napalm.OwnerID != 0)
            {
                var attacker = BasePlayer.FindByID(napalm.OwnerID);
                if (attacker == null) return;
                napalm.creatorEntity = attacker;
            }
            
        }

        private int GetAmount(BasePlayer player, int itemid)
        {
            if (player == null || player?.inventory == null) return -1;
            var amount = 0;
            if (player?.inventory?.containerMain?.itemList != null && player.inventory.containerMain.itemList.Count > 0)
            {
                for (int i = 0; i < player.inventory.containerMain.itemList.Count; i++)
                {
                    var item = player.inventory.containerMain.itemList[i];
                    if (item != null && item.info.itemid == itemid) amount += item.amount;
                }
            }
            if (player?.inventory?.containerBelt?.itemList != null && player.inventory.containerBelt.itemList.Count > 0)
            {
                for (int i = 0; i < player.inventory.containerBelt.itemList.Count; i++)
                {
                    var item = player.inventory.containerBelt.itemList[i];
                    if (item != null && item.info.itemid == itemid) amount += item.amount;
                }
            }
            if (player?.inventory?.containerWear?.itemList != null && player.inventory.containerWear.itemList.Count > 0)
            {
                for (int i = 0; i < player.inventory.containerWear.itemList.Count; i++)
                {
                    var item = player.inventory.containerWear.itemList[i];
                    if (item != null && item.info.itemid == itemid) amount += item.amount;
                }
            }
            return amount;
        }

        private readonly Dictionary<BaseEntity, List<BaseEntity>> entFireball = new Dictionary<BaseEntity, List<BaseEntity>>();

        //Hash<BaseEntity, List<BaseEntity>> entFireball = new Hash<BaseEntity, List<BaseEntity>>();
        private void OnEntitySpawned(BaseNetworkable entity)
        {

            if (entity == null) return;
            string entnamelong = entity.PrefabName;
            string entname = entity.ShortPrefabName;
            var creatorEnt = (entity as BaseEntity)?.creatorEntity ?? null;

            if (entname.Contains("fireball") && creatorEnt != null && creatorEnt is BasePlayer)
            {
                var fireball = entity?.GetComponent<FireBall>() ?? null;
                if (fireball == null) return;
                if (fireball.OwnerID != 1337)
                {
                    var player = fireball?.creatorEntity?.GetComponent<BasePlayer>() ?? null;
                    if (player == null) return;
                    if (!permission.UserHasPermission(player.UserIDString, "dragonsbreath.napalm") && permission.UserHasPermission(player.UserIDString, "dragonsbreath.water"))
                    {
                        var path = "assets/prefabs/weapons/waterbucket/waterball.prefab";



                        var count = 20;
                        for (int i = 0; i < count; i++)
                        {
                            var waterBallObj = GameManager.server.CreateEntity(path, fireball.transform.position, fireball.transform.rotation);
                            if (waterBallObj == null) continue;
                            var waterBall = (WaterBall)waterBallObj;
                            if (waterBall == null) continue;
                            waterBall.waterAmount = 1000;
                            waterBall.SetVelocity(player.GetDropVelocity() * 32);
                            waterBall.GetComponent<Rigidbody>().AddForce(player.GetDropVelocity() * 32);
                            waterBall.Spawn();
                            waterBall.GetComponent<Rigidbody>().AddForce(player.GetDropVelocity() * 32);
                        }

                        // waterBall.SetVelocity(waterBall.transform.position + Vector3.forward * 1);

                        if (!fireball.IsDestroyed) fireball.Kill(BaseNetworkable.DestroyMode.None);
                    }
                    if (!permission.UserHasPermission(player.UserIDString, "dragonsbreath.napalm")) return;
                    var rng = UnityEngine.Random.Range(0, 101);
                    if (rng < 18)
                    {
                        var required1 = 10;
                       // var required1 = UnityEngine.Random.Range(4, 14);
                        var id1 = -321733511;
                        if (GetAmount(player, id1) < required1) return;
                        var flamer = player?.GetActiveItem()?.GetHeldEntity()?.GetComponent<FlameThrower>() ?? null;
                        if (flamer == null) return;
                        player.inventory.Take(null, id1, required1);
                        player.SendConsoleCommand("note.inv " + id1 + " -" + required1);
                     

                        var rngammo = UnityEngine.Random.Range(6, 10);
                        if (flamer.ammo >= rngammo) flamer.ammo -= rngammo;
                        else flamer.ammo = 0;
                        flamer.SendNetworkUpdate();

                        var dmgMultiplier = 1.5f;
                        var tickRate = 0.5f;

                       
                        var fireballNew = (FireBall)GameManager.server.CreateEntity("assets/bundled/prefabs/napalm.prefab", fireball.transform.position, fireball.transform.rotation);
                        fireballNew.OwnerID = player?.userID ?? 1337;
                        fireballNew.creatorEntity = fireball;
                        fireballNew.damagePerSecond = fireball.damagePerSecond * dmgMultiplier;
                        fireballNew.lifeTimeMax = fireball.lifeTimeMax * 5f;
                        fireballNew.lifeTimeMin = fireball.lifeTimeMin * 4f;
                        fireballNew.generation = fireball.generation;
                        fireballNew.tickRate = tickRate;
                        fireballNew.radius = fireball.radius * 1.175f;
                        fireballNew.waterToExtinguish = fireball.waterToExtinguish * 2;
                  //      fireballNew.spreadSubEntityString = fireball.PrefabName; //spread a regular fireball
                        fireballNew.AttackLayers = fireball.AttackLayers;
                        fireballNew.Spawn();
                        fireball.radius = 0; //keep second fireball but do no damage, prevent spreading to ensure there's ambient lighting, or else napalm looks worse
                        fireball.damagePerSecond = 0; //ditto
                       // fireball.spreadSubEntityString = ""; //make sure it doesn't spread!
                        fireball.lifeTimeMax = fireballNew.lifeTimeMax;
                        fireball.lifeTimeMin = fireballNew.lifeTimeMin;
                        fireball.generation = 0;
                        
                        NextTick(() =>
                        {
                            if (fireball != null && !(fireball?.IsDestroyed ?? true)) fireball.CancelInvoke(fireball.TryToSpread);

                            if (fireballNew != null && !(fireballNew?.IsDestroyed ?? true))
                            {
                                var newRigid = fireballNew?.GetComponent<Rigidbody>() ?? null;
                                var nearEnts = new List<BaseEntity>();
                                Vis.Entities((fireballNew?.transform?.position ?? Vector3.zero), 2f, nearEnts, 1084427009);
                                if (nearEnts != null && nearEnts.Count > 1)
                                {
                                    var nearEnt = nearEnts?.FirstOrDefault() ?? null;
                                    if (nearEnt != null && !(nearEnt?.IsDestroyed ?? true))
                                    {
                                        var hitRigid = nearEnt?.GetComponent<Rigidbody>() ?? null;
                                        if (hitRigid == null && newRigid != null)
                                        {
                                            newRigid.isKinematic = true;
                                            var outEntList = new List<BaseEntity>();
                                            if (!entFireball.TryGetValue(nearEnt, out outEntList)) entFireball[nearEnt] = new List<BaseEntity>();
                                            //   if (!entFireball.ContainsKey(nearEnt)) entFireball[nearEnt] = new List<BaseEntity>();
                                            entFireball[nearEnt].Add(fireballNew);
                                           // PrintWarning("kinematic for napalm!");
                                        }
                                        else
                                        {
                                            if (player.IsAdmin) PrintWarning("hitRigid null?: " + (hitRigid == null) + ", newrigid null?: " + (newRigid == null) + ", hit prefab: " + nearEnt.ShortPrefabName);
                                        }
                                        
                                    }
                                    else PrintWarning("no near ent!");
                                }
                                else PrintWarning("no near ent list!");
                            }
                            else PrintWarning("fireball new null!");
                        });
                    //   if (!fireball.IsDestroyed) fireball.Kill(BaseNetworkable.DestroyMode.None); //destroy the original fireball so that only the napalm one exists and we are not stacking
                    }
                }
            }
        }

        private readonly Dictionary<BaseEntity, float> timeSinceAttack = new Dictionary<BaseEntity, float>();

        private float TimeSinceAttacked(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed) return -1;
            var combatEntity = entity?.GetComponent<BaseCombatEntity>() ?? null;
            if (combatEntity != null)
            {
                var cbeTime = combatEntity?.SecondsSinceAttacked ?? -1f;
                if (cbeTime != -1f) return cbeTime;
            }
            var time = 0f;
            if (timeSinceAttack.TryGetValue(entity, out time)) return (Time.realtimeSinceStartup - time);
            else return -1f;
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;
            var attacker = (info?.Initiator as BasePlayer) ?? info?.Weapon?.GetOwnerPlayer() ?? info?.WeaponPrefab?.GetItem()?.GetOwnerPlayer() ?? null;
            var prefabName = entity?.ShortPrefabName ?? string.Empty;
            var attackPrefab = info?.Initiator?.ShortPrefabName ?? string.Empty;
            var wepPrefab = info?.WeaponPrefab?.ShortPrefabName ?? info?.Weapon?.ShortPrefabName ?? string.Empty;
            var hitMat = info?.HitMaterial ?? 0;
            var time = Time.realtimeSinceStartup;
            var grade = (entity as BuildingBlock)?.grade ?? BuildingGrade.Enum.None;
           // var grade = entity?.GetComponent<BuildingBlock>()?.grade ?? BuildingGrade.Enum.None;
            if ((attackPrefab.Contains("napalm") || wepPrefab.Contains("napalm")))
            {
                var dmgList = new List<Rust.DamageTypeEntry>();
                var dmgType = new Rust.DamageTypeEntry
                {
                    type = Rust.DamageType.Heat,
                    amount = 2
                };
                dmgList.Add(dmgType);
                if (hitMat == 3655341 || prefabName.Contains("wood") || (grade == BuildingGrade.Enum.Wood || grade == BuildingGrade.Enum.Twigs))
                {
                    var tsa = TimeSinceAttacked(entity);
                    if (tsa >= 2.75 || tsa < 0)
                    {
                        DamageUtil.RadiusDamage((attacker != null ? attacker : (entity?.creatorEntity ?? entity)), (entity?.LookupPrefab() ?? null), (entity?.transform?.position ?? Vector3.zero), 0.9f, 1f, dmgList, flamerColl, false);
                        timeSinceAttack[entity] = time;
                    }
                }
            }
        }

        private readonly Hash<ulong, float> lastFXtime = new Hash<ulong, float>();
        private readonly Hash<ulong, bool> explosiveBullets = new Hash<ulong, bool>();

        private void OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || player.IsDead() || !player.IsConnected || info == null) return;
            var weapon = info?.Weapon ?? null;
            if (weapon == null) return;
            var hitWorld = info?.HitPositionWorld ?? info?.HitEntity?.transform?.position ?? Vector3.zero;
            if (hitWorld == Vector3.zero) return;
            var weaponItem = weapon?.GetItem() ?? null;
            var shortname = weaponItem?.info?.shortname ?? string.Empty;
            var name = weaponItem?.info?.shortname ?? "Unknown";
            var heldPrefab = info?.WeaponPrefab ?? player?.GetHeldEntity()?.LookupPrefab() ?? (player as BaseEntity);
            var rotation = info?.HitEntity?.transform?.rotation ?? Quaternion.identity;
            var hitEntity = info?.HitEntity ?? null;
            var fxprefab = "assets/bundled/prefabs/fx/impacts/additive/fire.prefab";
            var entprefab = "assets/bundled/prefabs/fireball.prefab";
            var hitCombat = hitEntity?.GetComponent<BaseCombatEntity>() ?? null;
            var hitBone = info?.HitBone ?? 0;
            var hitBoneName = (hitCombat?.skeletonProperties?.FindBone(hitBone)?.name?.english ?? string.Empty);
            var projID = info?.ProjectileID ?? 0;
            if (arrowIds.Contains(projID))
            {
                Effect.server.Run(fxprefab, hitWorld, hitWorld, null);

                var fireball = (FireBall)GameManager.server.CreateEntity(entprefab, hitWorld, rotation);
                if (fireball == null) return;

                fireball.generation = 0f;
                fireball.SetGeneration(0);
       //         fireball.spreadSubEntityString = string.Empty;
                fireball.tickRate = 0.333f;
                fireball.damagePerSecond = 2f;
                fireball.Spawn();
                fireball.CancelInvoke(fireball.TryToSpread);
                fireball.CancelInvoke(fireball.Extinguish);
                var rngTime = UnityEngine.Random.Range(8f, 24f);
                fireball.Invoke(fireball.Extinguish, rngTime);
                Puts("Radius: " + fireball.radius + ", tickrate: " + fireball.tickRate + ", dps: " + fireball.damagePerSecond);
                //       fireball.CancelInvoke("TryToSpread");
                if (hitEntity != null)
                {
                    var rigid = fireball?.GetComponent<Rigidbody>() ?? null;
                    var hitRigid = hitEntity?.GetComponent<Rigidbody>() ?? null;
                    PrintWarning("hit rigid null?: " + (hitRigid == null));
                    if (rigid != null && hitRigid == null)
                    {
                    //    rigid.isKinematic = true;
                  //      PrintWarning("kinematic!");
                    }
                }

                if (hitCombat != null && hitCombat.IsAlive())
                {
                    var dmg = UnityEngine.Random.Range(1f, 4.5f);
                    hitCombat.Hurt(dmg, Rust.DamageType.Heat, player);
                    Puts("hurting: " + hitEntity.ShortPrefabName + ", dmg: " + dmg);
                }
            }

            if (explosiveBullets[player.userID])
            {
                Vector3 hitEntityPos = hitWorld;
                var distFrom = Vector3.Distance(hitEntityPos, player.transform.position);
                if (distFrom >= 160) return;
                //      var entity1 = GameManager.server.CreateEntity("assets/bundled/prefabs/fx/explosions/explosion_01.prefab", player.eyes.position + player.eyes.BodyForward() * distFrom);
                // var fxpath = "assets/bundled/prefabs/fx/explosions/explosion_01.prefab";
                var fxpath = "assets/prefabs/tools/c4/effects/c4_explosion.prefab";
               
                var rng = UnityEngine.Random.Range(1, 4);
                if (rng == 2) fxpath = "assets/prefabs/weapons/f1 grenade/effects/f1grenade_explosion.prefab";
                if (rng == 3) fxpath = "assets/prefabs/weapons/beancan grenade/effects/beancan_grenade_explosion.prefab";
               // if (rng == 2) fxpath = fxpath.Replace("_01", "_02");
               //   if (rng == 3) fxpath = fxpath.Replace("_01", "_03");
                var now = Time.realtimeSinceStartup;
           //     if (!lastFXtime.ContainsKey(player.userID)) lastFXtime.Add(player.userID, now - 2);
        //       if (now - lastFXtime[player.userID] >= 0.06f)
              //  {
                    Effect.server.Run(fxpath, hitEntityPos, hitEntityPos, null);
                    lastFXtime[player.userID] = now;
             //   }
                
                var dmg = new List<Rust.DamageTypeEntry>();
                var dmgg = new Rust.DamageTypeEntry
                {
                    amount = 550,
                    type = Rust.DamageType.Explosion
                };
                dmg.Add(dmgg);
                //2230528 -- c4 layer?
                //1084427009 -- fireball layer? (hits more things?)
                //      DamageUtil.RadiusDamage(player, heldPrefab, hitEntityPos, 3.5f, 3.5f, dmg)

                DamageUtil.RadiusDamage(player, heldPrefab, hitEntityPos, 3.5f, 3.5f, dmg, 1084427009, false);

            }
            if (!doDragonsBreath[player])
            {
                return;
            }
            var userID = player.userID;
            var cont = false;
            var weaponName = weapon?.GetItem()?.info?.shortname ?? string.Empty;
          

                if (weaponName == "shotgun.pump" || (player.IsAdmin && weaponName == "shotgun.spas12"))
            {
                var rng = UnityEngine.Random.Range(0, 101);
                if (rng <= UnityEngine.Random.Range(21, 33)) cont = true;

                if (!cont) return;

                var playerPos = player?.CenterPoint() ?? Vector3.zero;
                if (playerPos == Vector3.zero) return;
               
                Vector3 hitEntityPos = Vector3.zero;
                var reduceAmount = 0f;

                hitEntityPos = hitWorld;
                if (hitEntityPos == null || hitEntityPos == Vector3.zero) return;
                var distFrom = Vector3.Distance(hitEntityPos, playerPos);
                if (distFrom >= 160 || distFrom < 0.8f) return; //if it's too far, don't do anything, if it's too close, don't do anything for safety
                flamestart[userID] = distFrom - reduceAmount;

                var heatDmg = new Rust.DamageTypeEntry
                {
                    type = Rust.DamageType.Heat,
                    amount = UnityEngine.Random.Range(2f, 3.76f)
                };

                var heatDmgList = new List<Rust.DamageTypeEntry>
                {
                    heatDmg
                };

                var weaponPrefab = info?.WeaponPrefab ?? null;
                if (hitCombat != null && hitCombat.IsAlive() && !(hitCombat is BuildingBlock || hitCombat is SimpleBuildingBlock || hitCombat is DecorDeployable))
                {
                    var dmg = UnityEngine.Random.Range(2.4f, 5.8f);
                    var healthBefore = hitCombat?.Health() ?? 0f;
                    hitCombat.Hurt(dmg, Rust.DamageType.Heat, player, true);
                    var healthAfter = hitCombat?.Health() ?? 0f;
                    if (healthBefore != healthAfter) Puts("hurting: " + hitEntity.ShortPrefabName + ", dmg: " + dmg + " health before: " + healthBefore + " after: " + healthAfter);
                }
                    
                    Effect.server.Run(fxprefab, hitEntityPos);

                for(int i = 0; i < UnityEngine.Random.Range(2, 5); i++)
                {

                    var entity1 = GameManager.server.CreateEntity(entprefab, i > 1 ? SpreadVector2(hitEntityPos, UnityEngine.Random.Range(1.15f, 2.1f)) : hitEntityPos, rotation);

                    if (entity1 == null) return;
                    FireBall fball = entity1?.GetComponent<FireBall>() ?? null;
                    if (fball == null) return;
                    fball.damagePerSecond = UnityEngine.Random.Range(3.9f, 5.3f);
                    fball.radius = UnityEngine.Random.Range(1.57f, 2.1f);
                    fball.lifeTimeMin = UnityEngine.Random.Range(2f, 6.33f);
                    fball.lifeTimeMax = UnityEngine.Random.Range(8.25f, 14f);
                    fball.generation = UnityEngine.Random.Range(1.8f, 2.75f);
                    fball.tickRate = UnityEngine.Random.Range(0.45f, 0.75f);
                    if (UnityEngine.Random.Range(0, 100) <= 8) fball.AttackLayers = playerColl;

                    entity1.OwnerID = player.userID; //assign owner so we can track this
                    fball.OwnerID = player.userID; //assign owner so we can track this as being one of our fireballs
                    fball.creatorEntity = player;
                    entity1.creatorEntity = player;

                    var blocks = Facepunch.Pool.GetList<BaseEntity>();
                    try 
                    {
                        Vis.Entities(entity1.transform.position, 5f, blocks, constructionColl);
                        if (blocks != null && blocks.Count > 0) fball.damagePerSecond = 0.8f;
                    }
                    finally { Facepunch.Pool.FreeList(ref blocks); }
                    
                   
                    

                    entity1.Spawn();
                }
               
               
            }
        }

        private Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

        private readonly List<int> arrowIds = new List<int>();

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;
            var weapon = projectile?.GetItem()?.info ?? null;
            var ammo = projectile?.primaryMagazine?.ammoType ?? null;
            var weaponName = weapon?.shortname ?? string.Empty;
            var ammoName = ammo?.shortname ?? string.Empty;
            var fuel_amount = GetAmount(player, -946369541);
            var fuel_required = -1;
            var projID = projectiles?.projectiles?.Where(p => p?.projectileID != 0)?.FirstOrDefault()?.projectileID ?? 0;
            /*/
            if (player.IsAdmin && explosiveBullets[player.userID])
            {
                var c4 = (TimedExplosive)GameManager.server.CreateEntity("assets/prefabs/tools/c4/explosive.timed.deployed.prefab", player.eyes.position, new Quaternion(), true);
                c4.creatorEntity = player;
                var velocityScale = 16f;
                c4.SetVelocity(player.GetDropVelocity() * velocityScale);
                c4.timerAmountMax = 3.5f;
                c4.timerAmountMin = 1.5f;
                c4.SetFuse(c4.GetRandomTimerTime());
                c4.Spawn();
            }/*/

            doDragonsBreath[player] = ammoName.Contains("shotgun") && !ammoName.Contains("slug");
            if ((weaponName == "shotgun.pump" || (player.IsAdmin && weaponName == "shotgun.spas12")) && doDragonsBreath[player])
            {
                fuel_required = 25;

                if (fuel_amount < fuel_required)
                {
                    doDragonsBreath[player] = false;
                    return;
                }
                if (!permission.UserHasPermission(player.UserIDString, "dragonsbreath.enabled"))
                {
                    doDragonsBreath[player] = false;
                    return;
                }
                //    player.inventory.Take(null, -946369541, fuel_required);
                doDragonsBreath[player] = true;
                var lowEyePos = player.eyes.position;
                lowEyePos.y -= 0.125f;
                var hasSilencer = projectile?.GetItem()?.contents?.itemList?.Any(p => p != null && p.info.shortname == "weapon.mod.silencer") ?? false;
                var posScalar = (hasSilencer) ? 1.8f : 1.725f;
                if (player.serverInput.IsDown(BUTTON.FORWARD)) posScalar += 1.1f;
                var forPos = lowEyePos + player.eyes.HeadForward() * posScalar;
                var hits = Physics.RaycastAll(new Ray(player.eyes.position, Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward), 1.9f);
                if (hits != null && hits.Length > 0 && hits.Any(p => Vector3.Distance(player.eyes.position, p.point) < 1.77f)) forPos = (lowEyePos + player.eyes.HeadForward() * (hasSilencer ? 1.05f : 0.8f));
                Effect.server.Run(BIG_FLAME_FX, forPos, Vector3.zero);
                var nearEnts = new List<BaseCombatEntity>();
                Vis.Entities(forPos, 0.75f, nearEnts);
                if (nearEnts.Count > 0)
                {
                    for(int i = 0; i < nearEnts.Count; i++)
                    {
                        var ent = nearEnts[i];
                        if (ent == null || ent.IsDestroyed || ent == player) continue;
                        var ply = ent as BasePlayer;
                        if (ply != null)
                        {
                            if (!ply.IsDead() && ply?.metabolism != null)
                            {
                                ply.metabolism.temperature.Add(UnityEngine.Random.Range(28f, 43f));
                                ply.Hurt(UnityEngine.Random.Range(1.25f, 3.5f), Rust.DamageType.Heat, player);
                            }
                        }
                        else ent.Hurt(UnityEngine.Random.Range(2.2f, 5.1f), Rust.DamageType.Heat, player);
                    }
                }
            }
                /*/
                if (player.IsAdmin)
                {
                    var startPos = player?.eyes?.position ?? Vector3.zero;
                    var startForward = player?.eyes?.HeadForward() ?? Vector3.zero;
                    var scalar = 1f;
                    for(int i = 0; i < 15; i++)
                    {
                        startPos = (startPos + (startForward * scalar));
                        PrintWarning("StartPos: " + startPos + ", scalar: " + scalar);
                        var newFB = (FireBall)GameManager.server.CreateEntity("assets/bundled/prefabs/fireball.prefab", startPos);
                        if (newFB != null)
                        {
                            newFB.Spawn();
                            newFB.GetComponent<Rigidbody>().isKinematic = true;
                            newFB.Invoke(newFB.Think, 0.0f);
                            newFB.SendNetworkUpdateImmediate(true);
                            newFB.CancelInvoke(newFB.TryToSpread);
                         //   newFB.Invoke(newFB.Kill, 0.75f);
                            newFB.Invoke(() => newFB.Kill(), 0.75f);
                       //     newFB.Invoke(newFB.Extinguish, 0.75f);
                        }
                        scalar += 0.0001f;
                    }
                }
            }/*/

            if (weaponName.Contains("bow") && arrowUsers.Contains(player.UserIDString))
            {
                fuel_required = 2;
                var clothReq = 1;
                var clothID = ItemManager.FindItemDefinition("cloth")?.itemid ?? 0;
                var clothAmt = player?.inventory?.GetAmount(clothID) ?? 0;
                if (clothAmt >= clothReq)
                {
                    player.inventory.Take(null, clothID, clothReq);
                    arrowIds.Add(projID);
                }
            }

            if (fuel_required > 0 && fuel_amount >= fuel_required)
            {
                player.inventory.Take(null, -946369541, fuel_required);
            }
     
        }

        [ChatCommand("db")]
        private void cmdDB(BasePlayer player, string command, string[] args)
        {
            cmdDragonsBreath(player, command, args);
        }



        [ChatCommand("expbullet")]
        private void cmdExplosiveBullet(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "You must supply a target name!");
                return;
            }
            var target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, "No player found with the name: " + args[0]);
                return;
            }
            if (!explosiveBullets.ContainsKey(target.userID))
            {
                explosiveBullets[target.userID] = true;
                SendReply(player, "Enabled explosive bullets for: " + target.displayName);
                return;
            }
            explosiveBullets[target.userID] = !explosiveBullets[target.userID];
            SendReply(player, "Explosive bullets for: " + target.displayName + " = " + explosiveBullets[target.userID]);
        }

        [ChatCommand("dragonsbreath")]
        private void cmdDragonsBreath(BasePlayer player, string command, string[] args)
        {
            if (permission.UserHasPermission(player.UserIDString, "dragonsbreath.enabled"))
            {
                permission.RevokeUserPermission(player.UserIDString, "dragonsbreath.enabled");
                doDragonsBreath[player] = false; //extra safety check
                SendReply(player, "<color=red>Dragon's Breath</color> disabled.");
            }
            else if (!permission.UserHasPermission(player.UserIDString, "dragonsbreath.enabled"))
            {
                permission.GrantUserPermission(player.UserIDString, "dragonsbreath.enabled", this);
                SendReply(player, "<color=red>Dragon's Breath</color> enabled. You will now shoot <color=red>Dragon's Breath</color> when using a Pump Shotgun with <color=red>Buckshot</color>.\nPlease keep in mind each pull of the trigger will require 25 <color=orange>Low Grade Fuel</color>.");
            }
            UpdateHooks(AnyDragonsBreath());
        }

        private readonly List<string> arrowUsers = new List<string>();
        [ChatCommand("fa")]
        private void cmdFA(BasePlayer player, string command, string[] args)
        {
            var userID = player.UserIDString;
            if (arrowUsers.Contains(userID)) arrowUsers.Remove(userID);
            else arrowUsers.Add(userID);
            SendReply(player, "<color=red>Fire Arrows</color> " + (arrowUsers.Contains(userID) ? "enabled" : "disabled"));
        }

        [ChatCommand("napalm")]
        private void cmdNapalm(BasePlayer player, string command, string[] args)
        {
            if (permission.UserHasPermission(player.UserIDString, "dragonsbreath.napalm"))
            {
                permission.RevokeUserPermission(player.UserIDString, "dragonsbreath.napalm");
                SendReply(player, "<color=red>Napalm</color> disabled.");
            }
            else if (!permission.UserHasPermission(player.UserIDString, "dragonsbreath.napalm"))
            {
                permission.GrantUserPermission(player.UserIDString, "dragonsbreath.napalm", this);
                SendReply(player, "<color=red>Napalm</color> enabled. You will now shoot <color=red>Napalm</color> through your Flamethrower provided you have <color=#ff912b>Crude Oil</color> and <color=orange>Low Grade Fuel</color>.");
           //     SendReply(player, "<color=red>Dragon's Breath</color> enabled. You will now shoot <color=red>Dragon's Breath</color> when using a Pump Shotgun with <color=red>Buckshot</color>.\nPlease keep in mind each pull of the trigger will require 30 <color=orange>Low Grade Fuel</color>.");
            }
        }

        [ChatCommand("waterflame")]
        private void cmdWaterFlame(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var userID = player.userID;
            if (permission.UserHasPermission(player.UserIDString, "dragonsbreath.water"))
            {
                permission.RevokeUserPermission(player.UserIDString, "dragonsbreath.water");
                SendReply(player, "<color=orange>Waterthrower</color> disabled.");
            }
            else if (!permission.UserHasPermission(player.UserIDString, "dragonsbreath.water"))
            {
                permission.GrantUserPermission(player.UserIDString, "dragonsbreath.water", this);
                SendReply(player, "<color=orange>Waterthrower</color> enabled.");
                //     SendReply(player, "<color=red>Dragon's Breath</color> enabled. You will now shoot <color=red>Dragon's Breath</color> when using a Pump Shotgun with <color=red>Buckshot</color>.\nPlease keep in mind each pull of the trigger will require 30 <color=orange>Low Grade Fuel</color>.");
            }
        }




        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            BasePlayer player = null;
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = p?.displayName ?? string.Empty;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
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
                        if (p == null) continue;
                        var pName = p?.displayName ?? string.Empty;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                return null;
            }
            return player;
        }

    }
}
