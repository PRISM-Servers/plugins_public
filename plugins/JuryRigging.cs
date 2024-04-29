using Facepunch;
using Oxide.Core;
using Oxide.Core.Plugins;
using Rust;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Jury Rigging (Part of Luck.cs)", "Shady", "0.0.1", ResourceId = 0)]
    internal class JuryRigging : RustPlugin
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
        //could we modularize parts of the Luck system? I believe I may try to do so with this plugin.
        //in theory this plugin needs only to grab level of the skill that handles this and deal with that accordingly

        //need to add half ammo consumpition part (if is player, use full ammo)

        [PluginReference]
        private readonly Plugin Luck;

        private readonly HashSet<HeliScan> _heliScanComps = new HashSet<HeliScan>();
        private readonly HashSet<SAMComponent> _samComps = new HashSet<SAMComponent>();

        private const uint PATROL_HELI_PREFAB_ID = 3029415845;

        private const string JURY_RIG_SKILL_NAME = "JuryRig";
        private const string THORNS_SKILL_NAME = "Thorns";

        #region Hooks

        private void OnTurretFired(AutoTurret turret)
        {
            var owner = RelationshipManager.FindByID(turret.OwnerID);
            if (owner == null || GetStatLevelByName(owner.UserIDString, JURY_RIG_SKILL_NAME) < 1)
                return;
            

            var plyTarget = turret?.target as BasePlayer;
            if (plyTarget != null && plyTarget.userID.IsSteamId())
                return;
            
            var weapon = turret?.GetAttachedWeapon();

            if (weapon == null)
                return;


            var currentAmmoCount = weapon?.primaryMagazine?.contents ?? 0;
            if (currentAmmoCount < 0)
                return;
            

            if (!IsDivisble(currentAmmoCount, 2))
                return;

            //add the ammo (we add every other shot)!
            weapon.primaryMagazine.contents++;

        }


        //if heli takes damage, this hook is called
        private void OnEntityTakeDamage(PatrolHelicopter heli, HitInfo hitInfo)
        {
            if (heli == null || hitInfo == null)
                return;

            var sam = hitInfo?.Initiator as SamSite;

            var turret = hitInfo?.Initiator as AutoTurret;

            if (sam == null && turret == null)
                return;

            var ownerId = sam?.OwnerID ?? turret?.OwnerID ?? 0;

            var ownerPly = RelationshipManager.FindByID(ownerId);

            if (ownerPly == null)
                return;

            var jrLvl = GetStatLevelByName(ownerPly.UserIDString, JURY_RIG_SKILL_NAME);

            if (sam != null)
            {
                hitInfo.damageTypes.Clear();
                hitInfo.damageTypes.Set(DamageType.Explosion, jrLvl < 4 ? 57.5f : 100.625f); //at lvl 4, get dmg boost
            }
            else hitInfo.damageTypes.ScaleAll(0.1f); //0.1x dmg for turrets
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            var turret = entity as AutoTurret;
            var SAM = entity as SamSite;

            if (turret == null && SAM == null)
                return;

            if (info?.Initiator == null || info?.InitiatorPlayer != null && IsValidPlayer(info?.InitiatorPlayer))
                return;

            var attacker = info?.Initiator as BaseCombatEntity;
            if (attacker == null)
                return;

            var ownerId = turret?.OwnerID ?? SAM?.OwnerID ?? 0;

            var ownerPlayer = RelationshipManager.FindByID(ownerId);

            if (ownerPlayer == null)
                return;

            var thorns = GetStatLevelByName(ownerPlayer.UserIDString, THORNS_SKILL_NAME);
            if (thorns < 1)
                return;

            var jrLvl = GetStatLevelByName(ownerPlayer.UserIDString, JURY_RIG_SKILL_NAME);

            if (jrLvl < 1)
                return;

            PrintWarning(entity + " was hit by player with thorns & jury rigging > 0!!: " + thorns);

            var dmgTotal = info?.damageTypes?.Total() ?? 0f;

            NextTick(() =>
            {
                if (attacker == null || attacker.IsDestroyed || attacker.IsDead())
                    return;

                dmgTotal = info?.damageTypes?.Total() ?? dmgTotal;

                var perc = 100 * thorns;
                perc += 400;

                var dmgPerc = dmgTotal * perc / 100;

            });

        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null)
                return;


            var bce = entity as BaseCombatEntity;

            if (bce != null && (entity is PatrolHelicopter || entity is CH47HelicopterAIController))
                AddSAMComp(bce);

            var turret = entity as AutoTurret;
            if (turret == null || turret.OwnerID == 0)
                return;

            var ownerPly = RelationshipManager.FindByID(turret.OwnerID);

            if (ownerPly == null) return;

            var jrLvl = GetStatLevelByName(ownerPly.UserIDString, JURY_RIG_SKILL_NAME);

            if (jrLvl < 1)
                return;

            //must have at least level 2 jury rigging for user's turrets to target heli
            if (jrLvl < 2)
                return;

            AddHeliScanComp(turret);

        }

        private void OnSamSiteTargetScan(SamSite sam, List<SamSite.ISamSiteTarget> list)
        {
            if (sam == null || list == null) return;

            var ply = RelationshipManager.FindByID(sam.OwnerID);
            if (ply == null)
                return;

            var jrLvl = GetStatLevelByName(ply.UserIDString, JURY_RIG_SKILL_NAME);

            //must have at least level 3 jury rigging for user's SAMs to target heli
            if (jrLvl < 3)
                return;

            var nearHelis = Pool.GetList<PatrolHelicopter>();
            try
            {
                Vis.Entities(sam.transform.position, 300f, nearHelis, Layers.Server.VehiclesSimple);

                for (int i = 0; i < nearHelis.Count; i++)
                {
                    var heli = nearHelis[i];
                    if (heli == null || heli.prefabID != PATROL_HELI_PREFAB_ID) continue;

                    var comp = heli.GetComponent<SAMComponent>();

                    if (comp != null && !list.Contains(comp))
                        list.Add(comp);
                    
                }

            }
            finally { Pool.FreeList(ref nearHelis); }
        }

    
        private void Unload()
        {
            try
            {
                foreach (var comp in _heliScanComps)
                    comp?.DoDestroy();
            }
            catch(Exception ex) { PrintError(ex.ToString()); }

            try
            {
                foreach (var comp in _samComps)
                    comp?.DoDestroy();
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

        }

        #endregion

        #region Chat Commands

        [ChatCommand("turtest")]
        private void CmdAddTurret(BasePlayer player, string cmd, string[] args)
        {
            if (!player.IsAdmin)
                return;

            var turret = GetLookAtEntity(player, 15f, 0.5f, 256) as AutoTurret;
            if (turret == null)
            {
                SendReply(player, "Not a turret!!");
                return;
            }

            var comp = turret?.GetComponent<HeliScan>();
            if (comp != null)
            {
                SendReply(player, "Found comp already, destroying first");
                comp.DoDestroy();
            }

            SendReply(player, "Adding");

            AddHeliScanComp(turret);

            SendReply(player, "Added!");
        }

        #endregion

        #region Util

        private HeliScan AddHeliScanComp(AutoTurret turret)
        {
            if (turret == null)
                throw new ArgumentNullException(nameof(turret));

            var heliScan = turret?.GetComponent<HeliScan>() ?? turret?.gameObject?.AddComponent<HeliScan>();

            //hashset, no need to check if contains already

            _heliScanComps.Add(heliScan);

            return heliScan;

        }

        private SAMComponent AddSAMComp(BaseCombatEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var samComp = entity?.GetComponent<SAMComponent>() ?? entity?.gameObject?.AddComponent<SAMComponent>();

            //hashset, no need to check if contains already

            _samComps.Add(samComp);

            return samComp;

        }

        //so, turrets may already be trying to target heli but just can't because of distance? this is really odd lol
        //needs investigation. the entire class below may be unnecessary.
        private class HeliScan : FacepunchBehaviour
        {
            public AutoTurret Turret { get; set; }

            public PatrolHelicopter TargetHeli { get; private set; }

            private float _oldSightRange = 30f;

            private void Awake()
            {
                Interface.Oxide.LogWarning(nameof(HeliScan) + "." + nameof(Awake));

                Turret = GetComponent<AutoTurret>();
                if (Turret == null || Turret.IsDestroyed)
                {
                    Interface.Oxide.LogError("Turret is null/destroyed on Awake!!!");
                    return;
                }

                _oldSightRange = Turret.sightRange;

                InvokeRepeating(Scan, 5f, 5f);

            }

            public void ClearTarget()
            {
                SetTargetHeli(null);
            }

            /// <summary>
            /// Clear's the turret's current target, but only if it's a heli.
            /// </summary>
            public void ClearTargetIfHeli()
            {
                if (Turret.target != TargetHeli || TargetHeli == null)
                    Turret.sightRange = _oldSightRange;

                if (TargetHeli == null)
                    return;

                if (Turret.target == TargetHeli)
                    SetTargetHeli(null);

            }

            public void SetTargetHeli(PatrolHelicopter targetHeli)
            {
                if (targetHeli != null && TargetHeli == targetHeli && Turret.target == targetHeli && Turret.SecondsSinceDealtDamage < 5f)
                    return;

                TargetHeli = targetHeli;

                Turret.SetTarget(TargetHeli);

                Turret.sightRange = targetHeli != null ? 250 : _oldSightRange;
            }

            /// <summary>
            /// If turret is not null, is online, and does not have a target equal to TargetHeli, it scans to find a nearby heli.
            /// </summary>
            public void Scan()
            {
                //Interface.Oxide.LogWarning(nameof(Scan));

                if (Turret == null || Turret.IsDestroyed || Turret.IsOffline() || (Turret?.target != null && Turret?.target == TargetHeli))
                    return;

                //really rough method for grabbing heli for now

                var helis = Pool.GetList<PatrolHelicopter>();

                try 
                {

                    var adjPos = Turret.transform.position;

                    adjPos = new Vector3(adjPos.x, adjPos.y + 30f, adjPos.z);

                    Vis.Entities(adjPos, 180f, helis);

                    if (helis.Count < 1)
                        ClearTargetIfHeli();
                    else
                        SetTargetHeli(helis[0]);

                }
                finally { Pool.FreeList(ref helis); }


            }

            public void DoDestroy()
            {
                try
                {
                    if (Turret != null && !Turret.IsDestroyed)
                        Turret.sightRange = _oldSightRange;

                    CancelInvoke(Scan);
                }
                finally { Destroy(this); }
            }

        }

        private void ShowToast(BasePlayer player, string message, GameTip.Styles type = GameTip.Styles.Blue_Normal)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.IsConnected)
                return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                player.SendConsoleCommand(sb.Clear().Append("gametip.showtoast ").Append((int)type).Append(" \"").Append(message).Append("\"").ToString());
            }
            finally { Pool.Free(ref sb); }
        }

        private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, float radius = 0f, int coll = -1)
        {
            if (player == null || player.IsDead()) return null;

            RaycastHit hit;

            var ray = new Ray(player?.eyes?.position ?? Vector3.zero, player.eyes.rotation * Vector3.forward);

            if (radius <= 0)
            {
                if (Physics.Raycast(ray, out hit, maxDist, coll))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }
            else
            {
                if (Physics.SphereCast(ray, radius, out hit, maxDist, coll))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
            }


            return null;
        }

        private int GetStatLevelByName(string userId, string statName, bool callHook = true)
        {
            return Luck?.Call<int>(nameof(GetStatLevelByName), userId, statName, callHook) ?? -1;
        }


        private class SAMComponent : FacepunchBehaviour, SamSite.ISamSiteTarget
        {

            private BaseCombatEntity _entity;

            private void Awake()
            {
                _entity = GetComponent<BaseCombatEntity>();

                if (_entity == null)
                {
                    DoDestroy();
                    return;
                }

                Interface.Oxide.LogWarning(nameof(SAMComponent) + "." + nameof(Awake) + " time to try and set speed mult");

                _overrideType = new SamSite.SamTargetType(250f, 10f, 0.01f); //temp! (not temp?!)
            }

            public void DoDestroy() => Destroy(this);


            private SamSite.SamTargetType _overrideType = null;

            public SamSite.SamTargetType SAMTargetType
            {
                get { return _overrideType ?? SamSite.targetTypeVehicle; }
            }

            public bool isClient => false;

            public Vector3 CenterPoint()
            {
                return _entity?.CenterPoint() ?? Vector3.zero;
            }

            public Vector3 GetWorldVelocity()
            {
                return _entity?.GetWorldVelocity() ?? Vector3.zero;
            }

            public bool IsValidSAMTarget(bool staticRespawn) { return true; }


            public bool IsVisible(Vector3 position, float maxDistance = float.PositiveInfinity)
            {
                return _entity?.IsVisible(position, maxDistance) ?? false;
            }
        }

        private bool IsValidPlayer(BasePlayer player) { return player != null && player.gameObject != null && !player.IsDestroyed && !player.IsNpc && player.prefabID == 4108440852; }

        public bool IsDivisble(int x, int n) { return (x % n) == 0; }

        #endregion

    }
}