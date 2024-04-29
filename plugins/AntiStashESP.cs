using Network;
using Oxide.Core;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AntiStashESP", "Shady", "0.0.6")]

    public class AntiStashESP : RustPlugin
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
        [PluginReference]
        private readonly Plugin CGSAuth;

        private static AntiStashESP _instance = null;

        private readonly HashSet<StashContainer> _allStashes = new HashSet<StashContainer>();
        private readonly HashSet<StashContainer> _temporaryUnderground = new HashSet<StashContainer>();
        #endregion
        #region Hooks
        private void Init()
        {
            _instance = this;

            Unsubscribe(nameof(OnEntitySpawned));
            Unsubscribe(nameof(OnEntityKill));
        }

        private void Unload()
        {
            try 
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;

                    var stashCheck = p?.GetComponent<StashChecker>();
                    if (stashCheck != null) stashCheck.DoDestroy();
                }
            }
            finally { _instance = null; }
       
        }

        private void OnServerInitialized()
        {
            foreach(var entity in BaseNetworkable.serverEntities)
            {
                var stash = entity as StashContainer;
                if (stash != null) _allStashes.Add(stash);
            }

            for(int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null) continue;

                var isAuthed = CGSAuth?.Call<bool>("IsAuthed", p.UserIDString) ?? true;
                if (!isAuthed)
                {
                    var stashCheck = p?.GetComponent<StashChecker>();
                    if (stashCheck == null)
                    {
                        p.gameObject.AddComponent<StashChecker>();
                    }
                }

               
            }

            Subscribe(nameof(OnEntitySpawned));
            Subscribe(nameof(OnEntityKill));
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;

            var stashCheck = player?.GetComponent<StashChecker>() ?? null;
            if (stashCheck != null) stashCheck.DoDestroy();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;

            var isAuthed = CGSAuth?.Call<bool>("IsAuthed", player.UserIDString) ?? true;
            if (!isAuthed)
            {
                var stashCheck = player?.GetComponent<StashChecker>() ?? null;
                if (stashCheck == null) player.gameObject.AddComponent<StashChecker>();
            }
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var stash = entity as StashContainer;
            if (stash != null) _allStashes.Add(stash);
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            var stash = entity as StashContainer;
            if (stash != null) _allStashes.Remove(stash);
        }

        private object CanCustomNetworkTo(BaseNetworkable entity, BasePlayer target)
        {
            if (entity == null || target == null) return null;

            var stash = entity as StashContainer;
            if (stash != null)
            {
                var stashCheck = target?.GetComponent<StashChecker>() ?? null;
                if (stashCheck != null && stashCheck.IsHidden(stash)) return false;
            }

            return null;
        }

        #endregion
        #region GameObject
        private class StashChecker : MonoBehaviour
        {
            public BasePlayer player;

            private readonly Stopwatch _watch = new Stopwatch();

            private Vector3 GetPlayerPosition()
            {
                return player != null && !player.IsDestroyed && player.gameObject != null ? (player?.transform?.position ?? Vector3.zero) : Vector3.zero;
            }

            public float DistanceBeforeNetworking { get; set; } = 4f;

            public HashSet<StashContainer> VisibleStashes { get; set; } = new HashSet<StashContainer>();

            public bool IsHidden(StashContainer stash)
            {
                if (stash == null) throw new ArgumentNullException(nameof(stash));

                return !VisibleStashes.Contains(stash);
            }

            private void CheckNearbyStashes()
            {
                _watch.Restart();
                try 
                {
                    if (player == null || player.IsDestroyed || !player.IsConnected)
                    {
                        DoDestroy();
                        return;
                    }

                    if (player.IsDead() || player.IsSleeping()) return;

                    var pos = GetPlayerPosition();

                    foreach (var entity in _instance._allStashes)
                    {
                        if (entity == null || entity.IsDestroyed || entity.gameObject == null) continue;

                        var adjustedPos = entity.transform.position;
                        adjustedPos.y = pos.y;

                        var dist = Vector3.Distance(adjustedPos, pos);

                        var wasHidden = entity.IsHidden();

                        if ((!wasHidden || dist <= DistanceBeforeNetworking) && VisibleStashes.Add(entity))
                        {
                            if (wasHidden && _instance._temporaryUnderground.Add(entity))
                            {
                                var oldPos = entity.transform.position;

                                entity.transform.position = new Vector3(entity.transform.position.x, entity.transform.position.y - 3, entity.transform.position.z);
                                entity.SendNetworkUpdateImmediate();

                                entity.Invoke(() =>
                                {
                                    if (entity == null || entity.IsDestroyed || entity.gameObject == null) return;
                                    entity.transform.position = oldPos;
                                    entity.SendNetworkUpdateImmediate();
                                    _instance._temporaryUnderground.Remove(entity);
                                }, 1f);
                            }


                            _instance?.AppearEntity(entity, player);
                        }
                        else if (wasHidden && dist >= DistanceBeforeNetworking && VisibleStashes.Remove(entity))
                        {
                            _instance?.DisappearEntity(entity, player);
                        }
                    }
                }
                finally
                {
                    if (_watch.ElapsedMilliseconds > 3) Interface.Oxide.LogWarning(nameof(CheckNearbyStashes) + " took too long: " + _watch.ElapsedMilliseconds + "ms");
                }
            
            }


            private void Awake()
            {
                player = GetComponent<BasePlayer>();

                if (player == null || player.IsDestroyed)
                {
                    DoDestroy();
                    return;
                }

                InvokeHandler.InvokeRepeating(this, CheckNearbyStashes, 6f, 3f);
            }

            public void DoDestroy()
            {
                try
                {
                    InvokeHandler.CancelInvoke(this, CheckNearbyStashes);
                }
                finally { Destroy(this); }
            }

        }
        #endregion
        #region Util


        private void DisappearEntity(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || entity?.net == null || player == null || player.net == null || player.net.connection == null) return;

            var conList = Facepunch.Pool.GetList<Connection>();
            try
            {
                conList.Add(player.net.connection);
                entity.OnNetworkSubscribersLeave(conList);
            }
            finally { Facepunch.Pool.FreeList(ref conList); }
        }

        private void AppearEntity(BaseEntity entity, BasePlayer player)
        {
            if (entity == null || entity?.net == null || player == null || player.net == null || player.net.connection == null) return;

            var conList = Facepunch.Pool.GetList<Connection>();
            try
            {
                conList.Add(player.net.connection);
                entity.OnNetworkSubscribersEnter(conList);
            }
            finally { Facepunch.Pool.FreeList(ref conList); }
        }
        private int GetPing(BasePlayer player) => player?.net?.connection != null ? Net.sv.GetAveragePing(player.net.connection) : -1;
        #endregion
    }
}