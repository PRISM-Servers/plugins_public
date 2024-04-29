using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AntiRecoilCheat", "Shady", "0.0.6")]

    public class AntiRecoilCheat : RustPlugin
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
        private readonly Dictionary<ulong, Vector3> _lastHitPos = new Dictionary<ulong, Vector3>();
        #endregion
        #endregion
        #region Hooks
        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile projmod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (projectile == null || player == null || projmod == null || projectiles == null || !player.IsConnected || player.IsDead()) return;

            var core = PRISMAntiCheatCore;
            if (core == null || !core.IsLoaded) return;

            if (core.Call<bool>("IsSaving") || core.Call<bool>("IsServerStalled")) return;

            var angles = player?.serverInput?.current?.aimAngles ?? Vector3.zero;

            var weaponItem = projectile?.GetItem();

            var weaponName = weaponItem?.info?.displayName?.english ?? "Unknown";
            var wepShortName = weaponItem?.info?.shortname ?? string.Empty;

            var plyPos = player?.transform?.position ?? Vector3.zero;

            var isBow = weaponItem?.GetHeldEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine?.definition.ammoTypes == Rust.AmmoTypes.BOW_ARROW;


            if (!string.IsNullOrEmpty(wepShortName) && !isBow)
            {

                var timerTime = 0.132f + (Mathf.Clamp(GetPing(player), 0f, 200f) * 0.0002f); //add extra padding up to 200 ping (40 ms at 2f, 20ms at 1f)

                player.Invoke(() =>
                {
                    if (player == null || player.IsDead() || !player.IsConnected) return;

                    var desync = player?.desyncTimeRaw ?? 0f;
                    if (desync >= 0.085f) return; //ensure player isn't significantly desynced/lagging

                    var newAngles = player?.serverInput?.current?.aimAngles ?? Vector3.zero;
                    if (newAngles == Vector3.zero) return;

                  

                    var newAnglesString = newAngles.ToString();
                    var oldAnglesString = angles.ToString();

                    var isExactMatch = newAnglesString.Equals(oldAnglesString, StringComparison.OrdinalIgnoreCase);

                    var angleDist = isExactMatch ? 0f : Vector3.Distance(newAngles, angles);

                    if (angleDist <= 0.035f && Physics.Raycast(new Ray(player?.eyes?.position ?? Vector3.zero, Quaternion.Euler(newAngles) * Vector3.forward), 650f)) //ensure player isn't just shooting at sky
                    {
                        var sb = Facepunch.Pool.Get<StringBuilder>();
                        try 
                        {
                            
                            var warnMsg = sb.Clear().Append("<color=yellow>").Append(player.displayName).Append(" (").Append(weaponName).Append(") may be shooting with no recoil! (").Append((timerTime * 1000).ToString("N0")).Append("ms angles check, diff: ").Append(angleDist.ToString("0.000").Replace(".000", string.Empty)).Append(isExactMatch ? " (EXACT)" : string.Empty).Append(")</color>").ToString();
                            core.Call("SendWarning", warnMsg);
                            core.Call("AddViolation", player.UserIDString, 6, plyPos, warnMsg, 0f, GetPing(player), weaponName); //6 = ViolationType.Recoil
                        }
                        finally { Facepunch.Pool.Free(ref sb); }
                      
                    }
                }, timerTime);
            }
        }

        private void OnPlayerAttack(BasePlayer attacker, HitInfo hitInfo)
        {
            if (attacker == null || hitInfo == null) return; 
            if ((hitInfo?.Weapon == null && hitInfo?.WeaponPrefab == null) || !(hitInfo?.IsProjectile() ?? false)) return;


            var core = PRISMAntiCheatCore;
            if (core == null || !core.IsLoaded) return;

            if (core.Call<bool>("IsSaving") || core.Call<bool>("IsServerStalled")) return;


            var distance = Vector3.Distance(hitInfo?.HitPositionWorld ?? Vector3.zero, attacker?.transform?.position ?? Vector3.zero);

            var weaponItem = hitInfo?.Weapon?.GetItem() ?? hitInfo?.WeaponPrefab?.GetItem() ?? null;

            //  var ammoName = weaponItem?.GetHeldEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine?.ammoType?.shortname ?? string.Empty;

            //   var mag = weaponItem?.GetHeldEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine;
            //  if (mag.definition.ammoTypes == Rust.AmmoTypes.SHOTGUN_12GUAGE 

            var isShotgun = weaponItem?.GetHeldEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine?.definition.ammoTypes == Rust.AmmoTypes.SHOTGUN_12GUAGE;

            //var isShotgun = ammoName.Contains("ammo.shotgun") || ammoName.Contains("handmade");
            var weaponName = weaponItem?.info?.displayName?.english ?? "Unknown";


         


            if (distance >= 6 && !isShotgun)
            {
                var hitPos = hitInfo?.HitPositionWorld ?? hitInfo?.HitPositionLocal ?? Vector3.zero;

                if (hitPos != Vector3.zero)
                {
                    Vector3 lastHitVector;
                    if (_lastHitPos.TryGetValue(attacker.userID, out lastHitVector))
                    {

                        var distDiff = Vector3.Distance(lastHitVector, hitPos);
                        if (distDiff <= 0.025f)
                        {
                            var sb = Facepunch.Pool.Get<StringBuilder>();
                            try
                            {
                                var warnMsg = sb.Clear().Append("<color=yellow>").Append(attacker.displayName).Append(" (").Append(weaponName).Append(") may be shooting with no recoil (hit pos, dist diff: ").Append(distDiff.ToString("0.00").Replace(".00", string.Empty)).Append("m)</color>").ToString();
                                core.Call("SendWarning", warnMsg);
                            }
                            finally { Facepunch.Pool.Free(ref sb); }

                        }
                    }

                    _lastHitPos[attacker.userID] = hitPos;
                }
            }
        }

        #endregion
        #region Util
        private int GetPing(BasePlayer player) => player?.net?.connection != null ? Network.Net.sv.GetAveragePing(player.net.connection) : -1;
        #endregion
    }
}