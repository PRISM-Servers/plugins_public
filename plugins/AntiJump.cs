using Oxide.Core;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Anti Jump", "Shady", "0.1.2")]

    public class AntiJump : RustPlugin
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
        #region Integers
        private const uint LStairsPrefabID = 3250880722;
        private const uint UStairsPrefabID = 1961464529;
        private const uint SpiralStairsPrefabID = 2700861605;

        private const uint FoundationStepsPrefabID = 1886694238;
        private const uint RoofPrefabID = 3895720527;
        private const uint RampPrefabID = 623529040;
        #endregion
        #region Floats
        private const float DiffCheck = 2.58f;
        #endregion
        #region Dictionaries
        private readonly Dictionary<string, float> lastJumpCheck = new Dictionary<string, float>();
        #endregion
        #region Lists/HashSets
        private readonly HashSet<ulong> recentLadder = new HashSet<ulong>();
        #endregion
        #endregion
        #region Hooks
        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (!player.IsAlive() || !player.IsConnected || player.IsSleeping() || player.IsWounded() || player.IsSwimming() || (player.IsAdmin && player.IsFlying) || player.isMounted) return;


            var core = PRISMAntiCheatCore;
            if (core == null || !core.IsLoaded)
            {
                return;
            }

            if (player?.OnLadder() ?? false)
            {
                var userID = player.userID;
                if (recentLadder.Add(userID))
                {
                    InvokeHandler.Invoke(ServerMgr.Instance, () =>
                    {
                        recentLadder.Remove(userID);
                    }, 0.8f);
                }

                return;
            }

            if (core.Call<bool>("IsServerStalled") || core.Call<bool>("IsSaving")) return;

            var heldEntity = player?.GetHeldEntity();

            var baseProj = heldEntity as BaseProjectile;

            if (baseProj != null && (heldEntity as BowWeapon) == null)
            {

                var wasAiming = player?.modelState?.aiming ?? player?.IsAiming ?? false;

                var wasGrounded = player.IsOnGround();
                if (!wasGrounded && wasAiming)
                {
                    var sb = Facepunch.Pool.Get<StringBuilder>();
                    try
                    {
                        core.Call("SendWarning", sb.Clear().Append("<color=yellow>").Append(player.displayName).Append(" may be aiming while jumping!</color>").ToString());
                    }
                    finally { Facepunch.Pool.Free(ref sb); }

                    return;
                }

                if (input.WasJustPressed(BUTTON.JUMP))
                {
                    player.Invoke(() =>
                    {
                        var isAiming = player?.modelState?.aiming ?? player?.IsAiming ?? false;
                        var isGrounded = player?.IsOnGround() ?? false;

                        if (isAiming && !isGrounded)
                        {
                            var sb = Facepunch.Pool.Get<StringBuilder>();
                            try
                            {
                                core.Call("SendWarning", sb.Clear().Append("<color=yellow>").Append(player.displayName).Append(" may be aiming while jumping!</color>").ToString());
                            }
                            finally { Facepunch.Pool.Free(ref sb); }
                        }
                    }, 0.075f);

                }
            }

            var ping = GetPing(player);
            var stalled = IsStalled(player);

            if ((input.WasJustPressed(BUTTON.JUMP) || input.IsDown(BUTTON.JUMP)) && !stalled && ping < 500 && player.IsOnGround() && player.GetParentEntity() == null)
            {
                var lastJump = -1f;
                if (lastJumpCheck.TryGetValue(player.UserIDString, out lastJump) && (Time.realtimeSinceStartup - lastJump) < 0.5f) return;

                var oldPos = player?.transform?.position ?? Vector3.zero;
                var watch = Stopwatch.StartNew();
                InvokeHandler.Invoke(player, () =>
                {

                    if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? false) || (player.IsAdmin && player.IsFlying) || player.OnLadder() || player.isMounted || recentLadder.Contains(player.userID)) return;
                    if (player.IsAdmin && player.UsedAdminCheat(1.5f)) return;

                    if (!player.IsAdmin) player.SendConsoleCommand("global.noclip");

                    var newPos = player?.transform?.position ?? Vector3.zero;

                    var newPos2 = newPos;
                    newPos2.y = oldPos.y;
                    if (Vector3.Distance(newPos2, oldPos) >= 25) return;

                 

                    var nearStairs = false;
                    var hits = Physics.RaycastAll(new Ray(new Vector3(oldPos.x, oldPos.y + 0.5f, oldPos.z), Vector3.down), 4f, -1);
                    if (hits != null && hits.Length > 0)
                    {
                        for (int i = 0; i < hits.Length; i++)
                        {
                            var hitInfo = hits[i];
                            var colNameAlt = hitInfo.collider?.name ?? hitInfo.collider?.sharedMaterial?.name ?? hitInfo.GetCollider()?.name ?? hitInfo.GetCollider()?.sharedMaterial?.name ?? string.Empty;
                            var colName = hitInfo.GetCollider()?.GetComponent<MeshCollider>()?.sharedMesh?.name ?? hitInfo.GetCollider()?.name ?? hitInfo.collider?.name ?? colNameAlt;
                            if (colName.Contains("steps", CompareOptions.OrdinalIgnoreCase) || colName.Contains("stair", CompareOptions.OrdinalIgnoreCase) || colName.Contains("roof", CompareOptions.OrdinalIgnoreCase) || colNameAlt.Contains("prevent_movment_hack", CompareOptions.OrdinalIgnoreCase))
                            {
                                nearStairs = true;
                                break;
                            }
                        }
                    }


                    if (!nearStairs)
                    {

                        var ents = Facepunch.Pool.GetList<BuildingBlock>();
                        try 
                        {
                            Vis.Entities(oldPos, 3f, ents, Rust.Layers.Construction);
                            if (ents.Count > 0)
                            {
                                for (int i = 0; i < ents.Count; i++)
                                {
                                    var ent = ents[i];
                                    if (ent == null) continue;
                                    if (ent.prefabID == LStairsPrefabID || ent.prefabID == UStairsPrefabID || ent.prefabID == SpiralStairsPrefabID || ent.prefabID == RampPrefabID || ent.prefabID == FoundationStepsPrefabID || ent.prefabID == RoofPrefabID)
                                    {
                                        nearStairs = true;
                                        break;
                                    }
                                }
                            }
                        }
                        finally { Facepunch.Pool.FreeList(ref ents); }

                    }

                    var diffCheck = DiffCheck;

                    if (nearStairs) diffCheck += 0.7f;


                    if (newPos.y > oldPos.y)
                    {
                        var diff = newPos.y - oldPos.y;
                        if (diff >= diffCheck)
                        {
                            watch.Stop();
                            if (watch.Elapsed.TotalSeconds < 0.85)
                            {
                                var warnMsg = string.Empty;

                                var sb = Facepunch.Pool.Get<StringBuilder>();
                                try 
                                {
                                    warnMsg = sb.Clear().Append("<color=yellow>").Append(player.displayName).Append("</color> just jumped <color=#ff912b>").Append(diff.ToString("0.00").Replace(".00", string.Empty)).Append("</color>m in ").Append(watch.Elapsed.TotalSeconds.ToString("0.00").Replace(".00", string.Empty)).Append(" seconds!").ToString();
                                }
                                finally { Facepunch.Pool.Free(ref sb); }

                                core.Call("SendWarning", warnMsg, 2); //ViolationType.Jump (2)
                                core.Call("GiveAntiHack", player, 50f + diff, AntiHackType.FlyHack);

                                core.Call("TeleportPlayer", player, oldPos, false, false);
                                core.Call("FreezePlayer", player.UserIDString, 1.5f);
                                core.Call("AddViolationFull", player.UserIDString, 2, oldPos, newPos, warnMsg, (50f + diff), ping); //2 = ViolationType.Jump

                            }
                            else PrintWarning("watch had totalseconds too high: " + watch.Elapsed.TotalSeconds);
                        }
                    }

                }, 0.7f);
                lastJumpCheck[player.UserIDString] = Time.realtimeSinceStartup;
            }
        }


        #endregion
        #region Util
        private int GetPing(BasePlayer player) => player?.net?.connection != null ? Network.Net.sv.GetAveragePing(player.net.connection) : -1;

        private bool IsStalled(BasePlayer player)
        {
            return (player?.desyncTimeRaw ?? 0f) > 0.095f;
        }
        #endregion
    }
}