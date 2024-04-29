using Oxide.Core;
using Oxide.Core.Plugins;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AntiSpider", "Shady", "0.0.5")]

    public class AntiSpider : RustPlugin
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
        private static AntiSpider Instance = null;


        #region Fields
        #region Plugin References
        [PluginReference] private readonly Plugin PRISMAntiCheatCore;
        [PluginReference] private readonly Plugin CGSAuth;
        #endregion
        #region Integers
        private static readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        private const uint LStairsPrefabID = 3250880722;
        private const uint UStairsPrefabID = 1961464529;
        private const uint SpiralStairsPrefabID = 2700861605;

        private const uint WallPrefabID = 2194854973;
        private const uint WallHalfPrefabID = 3531096400;
        private const uint WallDoorwayPrefabID = 2695954185;

        private const uint FoundationStepsPrefabID = 1886694238;
        private const uint RoofPrefabID = 3895720527;
        private const uint RampPrefabID = 623529040;
        #endregion
        #endregion
        #region Hooks
        private void OnServerInitialized()
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var ply = BasePlayer.activePlayerList[i];
                if (ply == null || ply.gameObject == null || !ply.IsConnected) continue;

                var spider = ply?.GetComponent<AntiSpiderComponent>() ?? null;
                if (spider == null)
                {
                    var isAuthed = CGSAuth?.Call<bool>("IsAuthed", ply.UserIDString) ?? true;
                    if (!isAuthed) ply.gameObject.AddComponent<AntiSpiderComponent>();
                }
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            var isAuthed = CGSAuth?.Call<bool>("IsAuthed", player.UserIDString) ?? true;
           
            var spider = player?.GetComponent<AntiSpiderComponent>() ?? null;
            if (spider == null && !isAuthed) player.gameObject.AddComponent<AntiSpiderComponent>();
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            var spider = player?.GetComponent<AntiSpiderComponent>() ?? null;
            if (spider != null) spider.DoDestroy();
        }

        private void Init() => Instance = this;
        

        private void Unload()
        {
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var ply = BasePlayer.activePlayerList[i];
                    if (ply == null || ply.gameObject == null) continue;

                    var spider = ply?.GetComponent<AntiSpiderComponent>();
                    if (spider != null) spider.DoDestroy();
                }
            }
            finally { Instance = null; }
        }
        #endregion
        #region Core
        private class AntiSpiderComponent : MonoBehaviour
        {
            public BasePlayer Player { get; set; }

            private bool _recentLadder = false;

            private Action _resetRecentLadder = null;

            public Vector3 LastPosition { get; set; } = Vector3.zero;

            private Stopwatch Watch { get; set; }

            private void Awake()
            {
                Player = gameObject.GetComponent<BasePlayer>();
                if (Player == null)
                {
                    Interface.Oxide.LogWarning("AntiSpider.Awake() called with null player!!!");
                    DoDestroy();
                    return;
                }
                if (Player.IsAdmin) Interface.Oxide.LogWarning("AntiSpider.Awake() called on admin");
                Watch = new Stopwatch();
                InvokeHandler.InvokeRepeating(this, CheckSpider, 1f, 1f);
                InvokeHandler.InvokeRepeating(this, CheckLadder, 0.8f, 0.8f);
            }

            public void DoDestroy()
            {
                InvokeHandler.CancelInvoke(this, CheckSpider);
                Destroy(this);
            }

            private BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
            {
                if (player == null || player.IsDead()) return null;
                RaycastHit hit;
                var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
                var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
                if (Physics.Raycast(ray, out hit, maxDist, coll))
                {
                    var ent = hit.GetEntity() ?? null;
                    if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                }
                if (coll == -1)
                {
                    if (Physics.Raycast(ray, out hit, maxDist, constructionColl))
                    {
                        var ent = hit.GetEntity() ?? null;
                        if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
                    }
                }
                return null;
            }

            private float LastNearestWallDistance = 0f;
            private DateTime LastWallCheckTime = DateTime.MinValue;
            private bool ShouldCheckForWall()
            {
                if (Player == null || Player.IsDestroyed || Player.gameObject == null || Player.transform == null) return false;
                var plyPos = Player?.transform?.position ?? Vector3.zero;
                if (LastPosition != Vector3.zero)
                {
                    var dist = Vector3.Distance(LastPosition, plyPos);
                    if (dist >= 10) return true;
                } //big distance change, preliminarily return true because player may have just teleported and may now be close to a wall

                var now = DateTime.UtcNow;
                var lastCheck = LastWallCheckTime <= DateTime.MinValue ? TimeSpan.MinValue : now - LastWallCheckTime;
                if (lastCheck > TimeSpan.MinValue && LastNearestWallDistance > 0f)
                {
                    var lastDist = LastNearestWallDistance;
                    var needSeconds = lastDist > 500 ? 100f : lastDist > 450 ? 90f : lastDist > 400 ? 65f : lastDist > 300 ? 45f : lastDist > 120 ? 15f : lastDist > 90 ? 10f : lastDist > 60 ? 7f : lastDist > 25 ? 4f : lastDist > 15 ? 2f : 0.85f;
                    if (lastCheck.TotalSeconds >= needSeconds)
                    {
                        //Interface.Oxide.LogWarning("totalseconds > needsceonds: " + lastCheck.TotalSeconds + " >= " + needSeconds);
                        return true;
                    }
                    else
                    {
                        // Interface.Oxide.LogWarning("totalseconds NOT > needseconds: " + lastCheck.TotalSeconds + " < " + needSeconds);
                        return false;
                    }
                }
                return true;
            }

            private void CheckLadder()
            {
                if (Player == null || Player.IsDestroyed || Player.gameObject == null || Player.IsDead() || Player.IsSleeping() || !Player.IsConnected) return;

                var onOrNearLadder = Player.OnLadder();

                if (!onOrNearLadder)
                {
                    var list = Facepunch.Pool.GetList<BaseLadder>();
                    try
                    {
                        Vis.Entities(Player.transform.position, 3f, list, 256); //256 = Deployed

                        for (int i = 0; i < list.Count; i++)
                        {
                            var ladder = list[i];
                            if (ladder?.gameObject != null && !ladder.IsDestroyed && ladder.IsVisible(Player.eyes.position, 5f))
                            {
                                onOrNearLadder = true;
                                break;
                            }
                        }
                    }
                    finally { Facepunch.Pool.FreeList(ref list); }
                }

                
                if (onOrNearLadder)
                {
                    _recentLadder = true;


                    if (_resetRecentLadder == null)
                    {
                        _resetRecentLadder = new Action(() =>
                        {
                            _recentLadder = false;
                        });
                    }

                    if (Player.IsInvoking(_resetRecentLadder)) Player.CancelInvoke(_resetRecentLadder);

                    Player.Invoke(_resetRecentLadder, 0.8f);
                }

            }

            private void CheckSpider()
            {
                Watch.Restart();
                var nearBlocks = Facepunch.Pool.GetList<BuildingBlock>();
                try
                {
                    if (_recentLadder || Player == null || Player.gameObject == null || Player.IsDestroyed || Player.IsSwimming() || Player.IsFlying || Player.IsDead() || !Player.IsConnected || Player.IdleTime >= 8 || Player.isMounted || Player.GetParentEntity() != null || Player.OnLadder() || Player.IsOnGround()) return;
                    var plyPos = Player?.transform?.position ?? Vector3.zero;
                    if (plyPos == Vector3.zero) return;
                    var lastPos = LastPosition;
                  //  Interface.Oxide.LogWarning("should check for admin: " + ShouldCheckForWall());
                    if (lastPos != Vector3.zero && ShouldCheckForWall())
                    {
                        var distFromLast = Vector3.Distance(lastPos, plyPos);
                        if (distFromLast > 0.1f && distFromLast < 8) //if the distance changed this much they probably TP'd
                        {
                            var yDiff = plyPos.y - lastPos.y;
                            //var xDiff = Math.Abs(plyPos.x) - Math.Abs(lastPos.x);
                            //  var zDiff = Math.Abs(plyPos.z) - Math.Abs(lastPos.z);
                            var yCheck = 1.51f;
                            var adjPlyPos = plyPos;
                            adjPlyPos.y = lastPos.y;
                            if (Vector3.Distance(adjPlyPos, lastPos) <= 0.2 && yDiff > yCheck)
                            {

                                var nearWall = false;
                                var minDist = -1f;
                                var currentRot = Quaternion.Euler(Player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
                                var ray = new Ray((Player?.eyes?.position ?? Vector3.zero), currentRot);
                                var rayHits = Physics.SphereCastAll(ray, 0.66f, 1.3f, constructionColl);
                                BuildingBlock lookAt = null;
                                float lastDist;
                                if (rayHits.Length > 0)
                                {
                                    for (int i = 0; i < rayHits.Length; i++)
                                    {
                                        var hit = rayHits[i];
                                        var entity = hit.GetEntity() as BuildingBlock;
                                        if (entity != null && (entity.prefabID == WallPrefabID || entity.prefabID == WallHalfPrefabID || entity.prefabID == WallDoorwayPrefabID))
                                        {
                                            lastDist = Vector3.Distance(entity.transform.position, plyPos);
                                            if (minDist < 0 || lastDist < minDist) minDist = lastDist;
                                            lookAt = entity;
                                            break;
                                        }
                                    }
                                }

                                //var lookAt = GetLookAtEntity(player, 2.75f, constructionColl) as BuildingBlock;

                                if (lookAt != null && (lookAt.prefabID == WallPrefabID || lookAt.prefabID == WallHalfPrefabID || lookAt.prefabID == WallDoorwayPrefabID))
                                {
                                    var adjPos = lookAt.transform.position;
                                    adjPos.y = plyPos.y; //more accurate checking when we "ignore" checking the Y coordinate
                                                         // var wallDist = Vector3.Distance(adjPos, plyPos);
                                    nearWall = true;
                                }

                                if (!nearWall)
                                {
                                    Vis.Entities(plyPos, 500f, nearBlocks, constructionColl);
                                    if (nearBlocks.Count > 0)
                                    {
                                        for (int i = 0; i < nearBlocks.Count; i++)
                                        {
                                            var block = nearBlocks[i];
                                            if (block != null && (block.prefabID == WallPrefabID || block.prefabID == WallHalfPrefabID || block.prefabID == WallDoorwayPrefabID))
                                            {
                                                var adjPos = block.transform.position;
                                                // adjPos.y = plyPos.y; //more accurate checking when we "ignore" checking the Y coordinate
                                                var wallDist = Vector3.Distance(adjPos, plyPos);
                                                lastDist = wallDist;
                                                if (minDist < 0 || lastDist < minDist) minDist = lastDist;
                                                if (wallDist < 10f)
                                                {
                                                    nearWall = true;
                                                }
                                           //     else if (wallDist < 20) Interface.Oxide.LogWarning("wall too far away luoldsafoaldslfasl: " + wallDist);
                                            }
                                        }
                                    }
                                }

                                //Interface.Oxide.LogWarning("ydiff > " + yCheck + ": " + yDiff + ", nearWall: " + nearWall + ", dist: " + distFromWall);
                                if (nearWall)
                                {
                                    var nearStairs = false;
                                    var stairBlocks = Facepunch.Pool.GetList<BuildingBlock>();

                                    try 
                                    {
                                        Vis.Entities(plyPos, 5f, stairBlocks, constructionColl);
                                        if (stairBlocks.Count > 0)
                                        {
                                            for (int i = 0; i < stairBlocks.Count; i++)
                                            {
                                                var block = stairBlocks[i];
                                                if (block == null || block.prefabID != LStairsPrefabID || block.prefabID != UStairsPrefabID || block.prefabID != SpiralStairsPrefabID || block.prefabID != RampPrefabID || block.prefabID != RoofPrefabID || block.prefabID != FoundationStepsPrefabID) continue;

                                                var distFromSteps = Vector3.Distance(block.CenterPoint(), plyPos);
                                                if (distFromSteps <= 3 && block.IsVisible(plyPos))
                                                {
                                                    if (Player.IsAdmin) Interface.Oxide.LogWarning("NEAR STEPS/STAIRS: " + block.transform.position + " " + block?.ShortPrefabName);
                                                    nearStairs = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    finally { Facepunch.Pool.FreeList(ref stairBlocks); }


                                    if (!nearStairs)
                                    {
                                        //3b1717
                                        var warnStr = "[<color=#f52222>Anti</color><color=#4d1a1a>Spider</color>] <color=yellow>" + Player?.displayName + "</color>:<color=#e08d19> <color=yellow>Y coordinate increased by</color> <color=#f52222>" + yDiff.ToString("0.00").Replace(".00", string.Empty) + "</color> while near a wall & not near stairs or a ladder!</color>";


                                        var core = Instance?.PRISMAntiCheatCore;
                                        if (core != null && core.IsLoaded)
                                        {
                                            core.Call("SendWarning", warnStr);

                                            core.Call("AddViolation", Player.UserIDString, 11, Player.transform.position, warnStr);
                                            core.Call("GiveAntiHack", Player, 10f + (yDiff * 3), AntiHackType.FlyHack);
                                        }

                                    }
                                    else Interface.Oxide.LogWarning("was near stairs!");
                                }
                                LastWallCheckTime = DateTime.UtcNow;
                                LastNearestWallDistance = minDist;
                                if (Player.IsAdmin)
                                {
                                    Interface.Oxide.LogWarning("last check: " + LastWallCheckTime + ", last nearest dis: " + LastNearestWallDistance + ", nearWall: " + nearWall);
                                }
                            }
                        }
                    }
                    LastPosition = plyPos;
                }
                finally
                {
                    Facepunch.Pool.FreeList(ref nearBlocks);
                    Watch.Stop();
                    if (Watch.Elapsed.TotalMilliseconds > 3) Interface.Oxide.LogWarning("AntiSpider.CheckSpider() took: " + Watch.Elapsed.TotalMilliseconds + "ms");
                }
            }
        }
        #endregion
    }
}