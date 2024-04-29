using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("CupboardGriefProtection", "Shady", "0.0.4")] //this was likely never working until a May 2023 update lol - also, should use dictionary instead of list for perf
    internal class CupboardGriefProtection : RustPlugin
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
        private class CupboardInfo
        {
            public NetworkableId CupboardId { get; set; }
            public uint BuildingId { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DateTime CupboardDestroyedTime;
            [JsonIgnore]
            public bool HasBeenDestroyed { get { return CupboardDestroyedTime > DateTime.MinValue; } }
            [JsonIgnore]
            public TimeSpan TimeSinceDestroyed { get { return HasBeenDestroyed ? (DateTime.UtcNow - CupboardDestroyedTime) : TimeSpan.MinValue; } }
            public CupboardInfo() { }
            public CupboardInfo(NetworkableId cupboardId, uint buildingId)
            {
                CupboardId = cupboardId;
                BuildingId = buildingId;
            }
        }

        private List<CupboardInfo> _cupboardInfos = new List<CupboardInfo>();

        private CupboardInfo GetCupboardInfo(NetworkableId netId)
        {
            if (_cupboardInfos == null || _cupboardInfos.Count < 1) return null;

            for (int i = 0; i < _cupboardInfos.Count; i++)
            {
                var info = _cupboardInfos[i];
                if (info?.CupboardId == netId) return info;
            }

            return null;
        }

        private CupboardInfo GetCupboardInfoBuilding(uint buildingId)
        {
            if (buildingId == 0 || _cupboardInfos == null || _cupboardInfos.Count < 1) return null;
            for (int i = 0; i < _cupboardInfos.Count; i++)
            {
                var info = _cupboardInfos[i];
                if (info?.BuildingId == buildingId)
                {
                    return info;
                }
            }
            return null;
        }

        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
        }

        private bool _serverInit = false;

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));
            _serverInit = true;
            _cupboardInfos = Interface.Oxide?.DataFileSystem?.ReadObject<List<CupboardInfo>>("CupboardInfos") ?? new List<CupboardInfo>();
            if (_cupboardInfos != null && _cupboardInfos.Count > 0)
            {
                var watch = Stopwatch.StartNew();
                var now = DateTime.UtcNow;
                var toRemove = Facepunch.Pool.GetList<CupboardInfo>();
                try 
                {
                    for (int i = 0; i < _cupboardInfos.Count; i++)
                    {
                        var info = _cupboardInfos[i];
                        if (info.TimeSinceDestroyed.TotalDays > (now - SaveRestore.SaveCreatedTime.ToUniversalTime()).TotalDays) toRemove.Add(info);
                    }
                    for (int i = 0; i < toRemove.Count; i++) _cupboardInfos.Remove(toRemove[i]);
                    var remCount = toRemove.Count;
                    if (remCount > 0) PrintWarning("Removed: " + remCount.ToString("N0") + " old cupboards (older than this wipe) in: " + watch.Elapsed.TotalMilliseconds + "ms");
                }
                finally
                {
                    Facepunch.Pool.FreeList(ref toRemove);
                }
            }
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("CupboardInfos", _cupboardInfos);
        

        [ConsoleCommand("cupboard.generate")]
        private void CmdCupboardGenerate(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            var watch = Stopwatch.StartNew();
            var genCount = 0;
            foreach(var entity in BaseEntity.saveList)
            {
                var cupboard = entity as BuildingPrivlidge;
                if (cupboard == null) continue;
                var building = cupboard.GetBuilding();
                if (building == null)
                {
                   // PrintWarning("Building is null for cupboard?!");
                    continue;
                }
                var info = new CupboardInfo(cupboard.net.ID, building.ID);
                _cupboardInfos.Add(info);
                genCount++;
            }
            watch.Stop();
            PrintWarning("cupboard.generate for " + genCount.ToString("N0") + " generations (cupboards) took: " + watch.Elapsed.TotalMilliseconds + "ms");
        }

        private void Unload() => SaveData();

        private void OnServerSave() => SaveData();

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (!_serverInit) return;
            var cupboard = entity as BuildingPrivlidge;
            if (cupboard == null) return;
            cupboard.Invoke(() =>
            {
                var building = cupboard.GetBuilding();
                if (building == null)
                {
                    PrintWarning("Building is null for cupboard?!");
                    return;
                }
                var info = new CupboardInfo(cupboard.net.ID, building.ID);
                _cupboardInfos.Add(info);
            }, 1f);
         
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;
            var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.LAST;
            if (dmgType != Rust.DamageType.Decay) return;

            var block = entity as BuildingBlock;
            if (block == null || block.buildingID == 0) return;

            var cupInfo = GetCupboardInfoBuilding(block.buildingID);
            if (cupInfo == null) return;

            if (!cupInfo.HasBeenDestroyed && cupInfo.TimeSinceDestroyed.TotalMinutes < ConVar.Decay.upkeep_grief_protection)
            {
                CancelDamage(info);
            }
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            var cupboard = entity as BuildingPrivlidge;
            if (cupboard == null) return;

            var cupInfo = GetCupboardInfo(cupboard.net.ID);
            if (cupInfo == null) return;

            cupInfo.CupboardDestroyedTime = DateTime.UtcNow;
        }

        private void CancelDamage(HitInfo hitinfo)
        {
            if (hitinfo == null) return;
            hitinfo.damageTypes.Clear();
            hitinfo.PointStart = Vector3.zero;
            hitinfo.HitEntity = null;
            hitinfo.DoHitEffects = false;
            hitinfo.HitMaterial = 0;
        }
    }
}