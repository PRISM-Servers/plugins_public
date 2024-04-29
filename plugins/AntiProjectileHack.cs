using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AntiProjectileHack", "Shady", "0.0.6")]

    public class AntiProjectileHack : RustPlugin
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
        #region Fields
        #region Plugin References
        [PluginReference] private readonly Plugin PRISMAntiCheatCore;
        #endregion
        #region Dictionaries
        private readonly Dictionary<ulong, List<FiredProjectileInfo>> firedProjectiles = new Dictionary<ulong, List<FiredProjectileInfo>>();
        #endregion
        #endregion
        #region Classes
        private class FiredProjectileInfo
        {
            public int ProjectileID { get; set; }
            public Vector3 FiredPosition { get; set; }
            public Vector3 FiredDirection { get; set; }
            public DateTime FiredTime { get; set; }
            public FiredProjectileInfo() { }
        }
        #endregion
        #region Hooks
        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile projmod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (projectile == null || player == null || projmod == null || projectiles == null || !player.IsConnected || player.IsDead()) return;


            var projList = projectiles?.projectiles ?? null;
            var eyePos = player?.eyes?.position ?? Vector3.zero;
            var rotation = GetPlayerRotation(player);

            var now = DateTime.UtcNow;

            if (projList != null && projList.Count > 0)
            {
                List<FiredProjectileInfo> projectileInfos;
                if (!firedProjectiles.TryGetValue(player.userID, out projectileInfos)) firedProjectiles[player.userID] = new List<FiredProjectileInfo>();

                for (int i = 0; i < projList.Count; i++)
                {
                    var proj = projList[i];
                    firedProjectiles[player.userID].Add(new FiredProjectileInfo
                    {
                        FiredDirection = rotation,
                        FiredPosition = eyePos,
                        ProjectileID = proj.projectileID,
                        FiredTime = now.AddSeconds(Time.deltaTime)
                    });
                }
            }

            var core = PRISMAntiCheatCore;
            if (core == null || !core.IsLoaded) return;

            if ((player?.IsHeadUnderwater() ?? false))
            {
                var warnMsg = "<color=yellow>" + player.displayName + " may be shooting while underwater!";

                core.Call("SendWarning", warnMsg);
                core.Call("GiveAntiHack", player, 50f, AntiHackType.ProjectileHack);
                core.Call("AddViolation", player.UserIDString, 4, player?.transform?.position ?? Vector3.zero, warnMsg, 50f, GetPing(player)); //4 = ViolationType.LOS
            }
        }

        private void OnPlayerAttack(BasePlayer attacker, HitInfo hitInfo)
        {
            if (attacker == null || hitInfo == null) return;
            try 
            {
                if ((hitInfo?.Weapon == null && hitInfo?.WeaponPrefab == null) || !(hitInfo?.IsProjectile() ?? false)) return;

                var core = PRISMAntiCheatCore;
                if (core == null || !core.IsLoaded) return;

                if (core.Call<bool>("IsSaving") || core.Call<bool>("IsServerStalled")) return;

                var weaponItem = hitInfo?.Weapon?.GetItem() ?? hitInfo?.WeaponPrefab?.GetItem() ?? null;

                var weaponName = weaponItem?.info?.displayName?.english ?? "Unknown";

                var projID = hitInfo?.ProjectileID ?? 0;

                var victim = hitInfo?.HitEntity;
                var vicPly = hitInfo?.HitEntity as BasePlayer;

                var watch = Facepunch.Pool.Get<Stopwatch>();

                try
                {
                    watch.Restart();

                    if (/*/weaponName.Contains("Cross") || /*/(weaponName.Contains("Bow") && !weaponName.Contains("Compound")))
                    {
                        List<FiredProjectileInfo> projectiles;
                        if (firedProjectiles.TryGetValue(attacker.userID, out projectiles) && projectiles?.Count > 0)
                        {
                            FiredProjectileInfo lastProjectile = null;

                            for (int i = 0; i < projectiles.Count; i++)
                            {
                                var proj = projectiles[i];
                                if (proj?.ProjectileID == hitInfo.ProjectileID)
                                {
                                    lastProjectile = proj;
                                    break;
                                }
                            }
                            if (lastProjectile != null)
                            {
                                var tookTime = DateTime.UtcNow - lastProjectile.FiredTime;
                                tookTime.Subtract(watch.Elapsed);
                                watch.Stop();
                                // tookTime.Subtract(TimeSpan.FromSeconds(Performance.report.frameTimeAverage));

                                var eyeStartPos = lastProjectile.FiredPosition;
                                var endPos = hitInfo?.HitPositionWorld ?? hitInfo?.PointEnd ?? lastProjectile?.FiredDirection ?? Vector3.zero;
                                var dist = endPos == Vector3.zero ? -1 : Vector3.Distance(eyeStartPos, endPos);


                                var bowWeapon = weaponItem != null ? ((weaponItem?.GetHeldEntity() ?? null) as BowWeapon) : null; //(weaponItem?.GetHeldEntity() ?? attacker?.GetHeldEntity() ?? null) as BowWeapon;
                                if (bowWeapon == null) bowWeapon = (attacker?.GetHeldEntity() ?? null) as BowWeapon;

                                var ammoType = bowWeapon?.primaryMagazine?.ammoType;
                                var ammoName = ammoType?.displayName?.english ?? "Unknown Ammo";

                                var isHV = ammoType?.itemid == -1023065463;

                                var minimumTimeToTakeForDist = isHV ? 0.2f : 0.8f;

                                var maxMPS = isHV ? 152f : 76f;
                                var metersPerSecond = Mathf.Clamp((float)(dist / tookTime.TotalSeconds), 0f, float.MaxValue);

                                if (dist > 25 && metersPerSecond >= maxMPS)
                                {
                                    var warnMsg = "<color=yellow>Arrow too fast for " + attacker.displayName + " (" + attacker.UserIDString + ", " + (attacker?.net?.connection?.ipaddress ?? string.Empty) + ") Distance from fire position: " + dist.ToString("0.0") + ", took: " + tookTime.TotalSeconds.ToString("0.00").Replace(".00", string.Empty) + " seconds to travel! (minimum time to take:  " + minimumTimeToTakeForDist + ")\nStart pos: " + eyeStartPos + ", end: " + endPos + " (weapon: " + weaponName + ", ammo: " + ammoName + ")</color>";
                                    
                                    PrintWarning("metersPerSecond: " + metersPerSecond + ", took: " + tookTime.TotalSeconds + " seconds to travel: " + dist + "m for: " + ammoName + ", " + bowWeapon?.ShortPrefabName + ", delta: " + UnityEngine.Time.deltaTime);

                                    CancelDamage(hitInfo);

                                    core.Call("SendWarning", warnMsg);
                                    core.Call("AddViolationFull", attacker.UserIDString, 9, eyeStartPos, endPos, warnMsg, 20f, GetPing(attacker)); //ViolationType.ArrowSpeed = 9
                                    core.Call("GiveAntiHack", attacker, 8f, AntiHackType.ProjectileHack);
                                }
                            }
                        }
                    }

                }

                finally { Facepunch.Pool.Free(ref watch); }



                if (vicPly != null && projID != 0 && vicPly?.GetParentEntity() == null)
                {
                    List<FiredProjectileInfo> projectiles;
                    if (firedProjectiles.TryGetValue(attacker.userID, out projectiles))
                    {
                        FiredProjectileInfo findInfo = null;
                        for (int i = 0; i < projectiles.Count; i++)
                        {
                            var proj = projectiles[i];
                            if (proj?.ProjectileID == projID)
                            {
                                findInfo = proj;
                                break;
                            }
                        }

                        if (findInfo != null)
                        {
                            //   var radius = 0.0075f;
                            var eyeDistance = Vector3.Distance(hitInfo?.HitPositionWorld ?? Vector3.zero, attacker?.eyes?.position ?? Vector3.zero);

                            var hits = Physics.RaycastAll(new Ray(findInfo.FiredPosition, findInfo.FiredDirection), (eyeDistance + 1f));

                            //var hits = UnityEngine.Physics.SphereCastAll(new Ray(findInfo.FiredPosition, findInfo.FiredDirection), radius, (eyeDistance + 0.75f));

                            var didFind = true;
                            if (hits != null && hits.Length > 0)
                            {
                                var sb = Facepunch.Pool.Get<StringBuilder>();

                                try
                                {
                                    sb.Clear();

                                    BaseEntity hitObj = null;

                                    var anyHitContainedVic = false;

                                    for(int i = 0; i < hits.Length; i++)
                                    {
                                        var hit = hits[i];

                                        var ent = hit.GetEntity();
                                        if (ent != null && !ent.IsDestroyed && ent.gameObject != null && ent == victim)
                                        {
                                            anyHitContainedVic = true;
                                            break;
                                        }
                                    }


                                    if (!anyHitContainedVic)
                                    {
                                        for (int i = 0; i < hits.Length; i++)
                                        {
                                            var hit = hits[i];
                                            /*/
                                            if (attacker.IsAdmin)
                                            {
                                               // attacker.SendConsoleCommand("ddraw.arrow", 4.5f, Color.blue, findInfo.FiredPosition, hit.point, 0.25f);
                                             //   attacker.SendConsoleCommand("ddraw.text", 4.5f, Color.yellow, hit.point, "Hit: " + i + ", " + hit.point);
                                            }/*/
                                            BaseEntity ent = null;

                                            try
                                            {
                                                ent = hit.GetEntity();
                                                sb.AppendLine("ray hit: " + ent?.ShortPrefabName + ", " + hit.point);
                                            }
                                            catch (Exception ex) { PrintError(ex.ToString()); }
                                            if (ent == attacker) continue;
                                            if (ent != null && !(ent == victim || ent?.ShortPrefabName == victim?.ShortPrefabName))
                                            {
                                                if (ent is FireBall || ent is SleepingBag) continue;
                                                didFind = false;
                                                sb.AppendLine("Found ent: ").Append(ent?.ShortPrefabName).Append(" ").Append(ent?.transform?.position).Append(", before target ent, found on: ").Append(i);
                                                hitObj = ent;
                                                break;
                                            }

                                            if (ent == victim || ent?.ShortPrefabName == victim?.ShortPrefabName)
                                            {
                                                didFind = true;
                                                sb.AppendLine("Found on ray hit: ").Append(i);
                                                break;
                                            }
                                        }
                                    }
                                  
                                    if (sb.Length > 0 && attacker.IsAdmin) PrintWarning(sb.ToString().TrimEnd());


                                    if (!didFind)
                                    {

                                        var vicName = vicPly != null ? (vicPly?.displayName ?? "Unknown Player") : (victim?.ShortPrefabName ?? "Unknown Prefab");

                                        core.Call("SendWarning", sb.Clear().Append("<color=yellow>").Append(attacker.displayName).Append(" may have just hit ").Append(vicName).Append(" ").Append((victim?.transform?.position ?? Vector3.zero)).Append(" through: ").Append((hitObj?.ShortPrefabName ?? "Unknown")).Append(" ").Append((hitObj?.transform?.position ?? Vector3.zero)).Append("</color>").ToString());
                                    }
                                }
                                finally { Facepunch.Pool.Free(ref sb); }



                            }


                        }
                        else { if (attacker.IsAdmin) PrintWarning("no projectile info for ID: " + projID); }

                    }
                }
            }
            catch (Exception ex) { PrintError(ex.ToString()); }

         

        }

        #endregion
        #region Util

        private Vector3 GetPlayerRotation(BasePlayer player)
        {
            return player == null ? Vector3.zero : Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
        }

        private int GetPing(BasePlayer player) => player?.net?.connection != null ? Network.Net.sv.GetAveragePing(player.net.connection) : -1;

        private static void CancelDamage(HitInfo hitinfo)
        {
            if (hitinfo == null) return;
            hitinfo.damageTypes.Clear();
            hitinfo.PointStart = Vector3.zero;
            hitinfo.HitEntity = null;
            hitinfo.DoHitEffects = false;
            hitinfo.HitMaterial = 0;
        }
        #endregion
    }
}