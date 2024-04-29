using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AntiMeleeHack", "Shady", "0.0.1")]

    public class AntiMeleeHack : RustPlugin
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
        private readonly Dictionary<ulong, Dictionary<ItemId, float>> lastMeleeAttack = new Dictionary<ulong, Dictionary<ItemId, float>>();
        private readonly Dictionary<ulong, int> meleeSpeedhacks = new Dictionary<ulong, int>();
        #endregion
        #region Timers
        private Timer _clearTimer = null;
        #endregion
        #endregion
        #region Hooks
        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;

            var attacker = ((info?.Initiator ?? null) as BasePlayer) ?? info?.Weapon?.GetItem()?.GetOwnerPlayer() ?? info?.WeaponPrefab?.GetItem()?.GetOwnerPlayer() ?? null;

            if (attacker == null) return;

            var victim = entity as BasePlayer;
           
            if (victim == null) return;

            var core = PRISMAntiCheatCore;
            if (core == null || !core.IsLoaded) return;

            if (core.Call<bool>("IsSaving") || core.Call<bool>("IsServerStalled")) return;


            var stalled = IsStalled(attacker);

            var baseMelee = (info?.Weapon != null && !info.Weapon.IsDestroyed && info.Weapon.gameObject != null) ? info?.Weapon?.GetComponent<BaseMelee>() : null;
            var isMelee = baseMelee != null && !(info?.IsProjectile() ?? false);

            var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;

            var hitPos = info?.HitPositionWorld ?? info?.HitPositionLocal ?? info?.HitEntity?.transform?.position ?? entity?.transform?.position ?? Vector3.zero;
            var atkPos = attacker?.transform?.position ?? Vector3.zero;
            var distFromHitPos = Vector3.Distance(hitPos, atkPos);
            var weaponName = info?.Weapon?.GetItem()?.info?.displayName?.english ?? info?.WeaponPrefab?.ShortPrefabName ?? info?.Weapon?.ShortPrefabName ?? "Unknown";

            if (isMelee && attacker != null && victim != null && attacker == victim && IsMeleeDamageType(majDmg))
            {
                var ents = Facepunch.Pool.GetList<BasicCar>();
                try
                {
                    Vis.Entities(atkPos, 5f, ents, 8192);
                    if (ents.Count > 0)
                    {
                  //      PrintWarning("Potential false positive on MH auto-ban, so not banning! (near a car) for player: " + attacker?.displayName + "/" + attacker?.UserIDString + "/" + atkPos);
                    }
                    else
                    {
                        // PrintWarning("self melee hit with dmg type: " + majDmg);

                        var sb = Facepunch.Pool.Get<StringBuilder>();
                        try 
                        {
                            var warnMsg = sb.Clear().Append("<color=yellow>").Append(attacker?.displayName).Append(" just hit themselves with melee weapon: ").Append(weaponName).Append(" from: ").Append(distFromHitPos.ToString("0.000").Replace(".000", string.Empty)).Append("m</color>").ToString();
                            core.Call("SendWarning", warnMsg);

                            var banStr = sb.Clear().Append("ban \"").Append(attacker.UserIDString).Append("\" \" Cheating - Other (MH)\"").ToString();
                            NextTick(() =>
                            {
                                var dmgAmount = info?.damageTypes?.Total() ?? 0f;
                                if (dmgAmount > 0f) covalence.Server.Command(banStr);
                            });
                        }
                        finally { Facepunch.Pool.Free(ref sb); }
                        
                       

                        
                    }
                }
                finally { Facepunch.Pool.FreeList(ref ents); }
            }

            if (isMelee && IsMeleeDamageType(majDmg) && !stalled)
            {
                var maxDist = (baseMelee?.maxDistance ?? 0f) + 1.39f;

               

                if (distFromHitPos > maxDist)
                {
                    if ((victim != null && IsValidPlayer(victim)) || entity is BuildingPrivlidge) CancelDamage(info); //cancel damage for players & cupboards if past max dist.

                    
                   

                    var warnMsg = "<color=yellow>" + attacker?.displayName + ", just hit a: " + entity?.ShortPrefabName + ", from: " + distFromHitPos.ToString("0.000").Replace(".000", string.Empty) + " meters away, using a: " + weaponName + ", max: " + maxDist.ToString("0.000") + " (Hit position: " + hitPos + ", player pos: " + atkPos + "</color>";

                    core.Call("SendWarning", warnMsg);
                }
            }
        }

        private void OnPlayerAttack(BasePlayer attacker, HitInfo hitInfo)
        {
            if (attacker == null || hitInfo == null) return;
            if ((hitInfo?.Weapon == null && hitInfo?.WeaponPrefab == null) || (hitInfo?.IsProjectile() ?? false)) return;


            var core = PRISMAntiCheatCore;
            if (core == null || !core.IsLoaded) return;

            if (core.Call<bool>("IsSaving") || core.Call<bool>("IsServerStalled")) return;

            var weaponEntity = hitInfo?.Weapon;

            var weaponItem = hitInfo?.Weapon?.GetItem() ?? hitInfo?.WeaponPrefab?.GetItem() ?? null;

            var weaponName = weaponItem?.info?.displayName?.english ?? "Unknown";

            var now = Time.realtimeSinceStartup;

            var repeatDelay = hitInfo?.Weapon?.GetItem()?.GetHeldEntity()?.GetComponent<BaseMelee>()?.repeatDelay ?? 0;
            var repeatDelayRelaxed = 0f;

            if (repeatDelay > 0) repeatDelayRelaxed = (repeatDelay / 1.33f);

            var itemID = hitInfo.Weapon.GetItem().uid;

            //change from str comparison to itemid
            if (weaponItem != null && (weaponItem.info.shortname == "chainsaw" || weaponItem.info.shortname == "jackhammer") || (weaponEntity != null && (weaponEntity.ShortPrefabName.Contains("chainsaw") || weaponEntity.ShortPrefabName.Contains("jackhammer")))) return;

            Dictionary<ItemId, float> lastMeleeOut;
            float lastMeleeTime;
            if (!lastMeleeAttack.TryGetValue(attacker.userID, out lastMeleeOut)) lastMeleeOut = lastMeleeAttack[attacker.userID] = new Dictionary<ItemId, float>();

            if (lastMeleeOut.TryGetValue(itemID, out lastMeleeTime))
            {
                var time = now - lastMeleeTime;
                if (time < repeatDelayRelaxed && !IsStalled(attacker) && attacker.desyncTimeRaw < 0.085 && (GetPing(attacker) < 500))
                {
                    var sb = Facepunch.Pool.Get<StringBuilder>();
                    try
                    {
                        var warnMsg = sb.Clear().Append("<color=yellow>").Append(attacker.displayName).Append(" has been detected attacking too fast with: ").Append(weaponName).Append(" (" + time.ToString("0.000").Replace(".000", string.Empty)).Append("s < " + repeatDelayRelaxed.ToString("0.000").Replace(".000", string.Empty)).Append("s, desync: ").Append((attacker?.desyncTimeRaw ?? 0f).ToString("0.000").Replace(".000", string.Empty)).Append(", ping: ").Append(GetPing(attacker).ToString("N0")).Append("ms)</color>").ToString();
                        core.Call("SendWarning", warnMsg);

                        CancelDamage(hitInfo);
                        if ((attacker?.desyncTimeRaw ?? 0) <= 0.075)
                        {
                            int sphCount;
                            if (!meleeSpeedhacks.TryGetValue(attacker.userID, out sphCount)) meleeSpeedhacks[attacker.userID] = 1;
                            else
                            {
                                meleeSpeedhacks[attacker.userID]++;
                                sphCount++;
                            }

                            if (sphCount >= 10)
                            {

                                Puts(sb.Clear().Append("Banning: ").Append(attacker.displayName).Append(" (").Append(attacker.UserIDString).Append(", ").Append(attacker?.net?.connection?.ipaddress ?? string.Empty).Append(")").ToString());


                                covalence.Server.Command(sb.Clear().Append("ban \"").Append(attacker.UserIDString).Append("\" ").Append("\" Cheating - Other (MSPH) \"").ToString());
                            }
                        }
                    }
                    finally { Facepunch.Pool.Free(ref sb); }
                }
            }
            lastMeleeAttack[attacker.userID][itemID] = now;
        }

        private void OnServerInitialized()
        {
            _clearTimer = timer.Every(16f, () => meleeSpeedhacks?.Clear());   
        }

        private void Unload()
        {
            if (_clearTimer != null) _clearTimer.Destroy();
        }

        #endregion
        #region Util
        private int GetPing(BasePlayer player) => player?.net?.connection != null ? Network.Net.sv.GetAveragePing(player.net.connection) : -1;

        private bool IsStalled(BasePlayer player)
        {
            return (player?.desyncTimeRaw ?? 0f) > 0.095f;
        }

        private bool IsValidPlayer(BasePlayer player)
        {
            return player != null && player.gameObject != null && !player.IsDestroyed && !player.IsNpc && player.prefabID == 4108440852;
        }

        private bool IsMeleeDamageType(Rust.DamageType dmgType) { return dmgType == Rust.DamageType.Blunt || dmgType == Rust.DamageType.Slash || dmgType == Rust.DamageType.Stab; }

        private void CancelDamage(HitInfo hitinfo)
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