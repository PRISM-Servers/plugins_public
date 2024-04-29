using Facepunch;
using Oxide.Core;
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Explosive Barrels", "Shady", "0.0.3", ResourceId = 0)]
    internal class ExplosiveBarrels : RustPlugin
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
        private const uint OIL_BARREL_PREFAB_ID = 3438187947;

        private const uint LOOT_BARREL_1 = 966676416;
        private const uint LOOT_BARREL_1_2 = 3364121927;

        private const uint LOOT_BARREL_2 = 555882409;
        private const uint LOOT_BARREL_2_2 = 3269883781;

        #region Hooks

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || entity == null) return;

            var stopwatch = Pool.Get<Stopwatch>();

            var name = string.Empty;

            try
            {
                stopwatch.Restart();

                name = entity?.ShortPrefabName;

                if (!IsBarrel(entity.prefabID) || !IsApplicableBarrelHit(info))
                    return;

                var attacker = info?.Initiator as BasePlayer;

                var ammoType = ((info?.Weapon ?? info?.WeaponPrefab) as BaseProjectile)?.primaryMagazine?.ammoType ?? null;
                var ammoName = ammoType?.shortname ?? string.Empty;


                var majDmg = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;

                var canExplo = majDmg == Rust.DamageType.Explosion || majDmg == Rust.DamageType.Heat || (majDmg == Rust.DamageType.Bullet && UnityEngine.Random.Range(0, 101) <= 33);

                var hookVal = Interface.Oxide.CallHook("CanBarrelExplode", entity, info);
                if (hookVal != null)
                    canExplo = (bool)hookVal;

                if (!canExplo)
                    return;

                var dmgList = Pool.GetList<Rust.DamageTypeEntry>();

                try
                {
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

                    ServerMgr.Instance.Invoke(() =>
                    {
                        NextTick(() =>
                        {
                            //2230528 <-- layer is player only?
                            //1084427009 <-- fireball layer?

                            try { DamageUtil.RadiusDamage(attacker, prefab, entCenter, 5f, 7f, dmgList, 1084427009, true); }
                            finally { Pool.FreeList(ref dmgList); }
                        });

                        var path2 = "assets/prefabs/npc/patrol helicopter/effects/rocket_fire.prefab";
                        var path3 = "assets/bundled/prefabs/fx/explosions/explosion_01.prefab";
                        var path4 = "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab";

                        Effect.server.Run(path, entCenter, entCenter, null, false);
                        Effect.server.Run(path2, entCenter, entCenter, null, false);
                        Effect.server.Run(path3, entCenter, entCenter, null, false);
                        Effect.server.Run(path4, entCenter, entCenter, null, false);


                    }, timerr);

                }
                catch (Exception ex)
                {
                    try { PrintError(ex.ToString()); }
                    finally { Pool.FreeList(ref dmgList); }
                }


            }
            finally
            {
                try { if (stopwatch.ElapsedMilliseconds > 3) PrintWarning("OnEntityDeath took: " + stopwatch.ElapsedMilliseconds + "ms for: " + name); }
                finally { Pool.Free(ref stopwatch); }
            }
        }

        #endregion

        #region Util

        private bool IsApplicableBarrelHit(HitInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            if (info?.HitEntity == null)
                return false;

            var hookVal = Interface.Oxide.CallHook("IsBarrelExplodable", info);

            if (hookVal != null)
                return (bool)hookVal;

            var entity = info.HitEntity;

            return entity.prefabID == OIL_BARREL_PREFAB_ID;
        }

        private bool IsBarrel(uint prefabId)
        {
            switch(prefabId)
            {
                case OIL_BARREL_PREFAB_ID:
                case LOOT_BARREL_1:
                case LOOT_BARREL_1_2:
                case LOOT_BARREL_2:
                case LOOT_BARREL_2_2:
                    return true;
                default: return false;
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
        #endregion

    }
}