using System.Collections.Generic;
using Oxide.Core.Plugins;
using UnityEngine;
using System.Text;
using Oxide.Core;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Reflection;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("LocalStatsAPI", "Shady", "0.0.6", ResourceId = 0000)]
    internal class LocalStatsAPI : RustPlugin
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
        internal static LocalStatsAPI Instance = null;

        [PluginReference]
        private readonly Plugin BanSystem;

        [PluginReference]
        private readonly Plugin Deathmatch;

        private FieldInfo[] _statsInfoFields = null;

        public FieldInfo[] StatsInfoFields
        {
            get
            {
                if (_statsInfoFields == null) _statsInfoFields = typeof(StatsInfo).GetFields();

                return _statsInfoFields;
            }
        }


        public class StatsInfo
        {
            public Dictionary<string, long> bodyPartsHit = new Dictionary<string, long>();
            public Dictionary<int, long> resourcesGathered = new Dictionary<int, long>();
            public Dictionary<uint, long> prefabsKilled = new Dictionary<uint, long>();

            public long deaths = 0;
            public long pvpDeaths = 0;
            public long pvpDeathsDM = 0;
            public long pvpKills = 0;
            public long pvpKillsDM = 0;
            public long kills = 0;
            public long shots = 0;
            public long meleeHits = 0;
            public long projHits = 0;
            public long npcHits = 0;

            public StatsInfo() { }
        }

        private StatsInfo GetStatsInfo(BasePlayer player) { return GetStatsInfo(player?.UserIDString ?? "0"); }

        private StatsInfo GetStatsInfo(ulong userID) { return GetStatsInfo(userID.ToString()); }

        private StatsInfo GetStatsInfo(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return null;
            StatsInfo statsInfo;
            if (storedData?.statsInfos != null && storedData.statsInfos.TryGetValue(userID, out statsInfo)) return statsInfo;
            else return null;
        }

        private StatsInfo GetStats2(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return null;
            StatsInfo statsInfo;
            storedData.statsInfos.TryGetValue(userID, out statsInfo);
            return statsInfo;
        }

        private class StoredData
        {
            public Dictionary<string, StatsInfo> statsInfos = new Dictionary<string, StatsInfo>();
            public StoredData() { }
        }

        private StoredData storedData;

        private StatsInfo CreateStats(string userID)
        {
            if (string.IsNullOrEmpty(userID) || userID == "0" || storedData == null || storedData?.statsInfos == null) return null;
            StatsInfo pInfo;
            if (!storedData.statsInfos.TryGetValue(userID, out pInfo)) storedData.statsInfos[userID] = (pInfo = new StatsInfo());
            return pInfo;
        }

        private void Unload() => SaveData();

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("LS_Data", storedData);

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            var stats = GetStatsInfo(player) ?? CreateStats(player.UserIDString);
            stats.shots++;
        }

        private void Init()
        {
            Instance = this;
            if ((storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>("LS_Data")) == null) storedData = new StoredData();
        }

        private bool IsMeleeType(Rust.DamageType dmgType) { return (dmgType == Rust.DamageType.Slash || dmgType == Rust.DamageType.Stab || dmgType == Rust.DamageType.Blunt); }

        private bool IsProjectileType(Rust.DamageType dmgType) { return (dmgType == Rust.DamageType.Arrow || dmgType == Rust.DamageType.Bullet); }

        private bool IsBleedingType(Rust.DamageType dmgType) { return (dmgType == Rust.DamageType.Bleeding); }

        private readonly Dictionary<ulong, float> lastProjHit = new Dictionary<ulong, float>();

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;
            var victim = entity as BasePlayer;
            var attacker = (info?.Initiator as BasePlayer) ?? info?.Weapon?.GetItem()?.GetOwnerPlayer() ?? info?.WeaponPrefab?.GetItem()?.GetOwnerPlayer() ?? BasePlayer.FindByID(info?.Weapon?.OwnerID ?? info?.WeaponPrefab?.OwnerID ?? 0);
            if (victim == null || attacker == null || !IsValidPlayer(victim) || !IsValidPlayer(attacker)) return;
            var atkStats = GetStatsInfo(attacker) ?? CreateStats(attacker.UserIDString);
            var hitBone = FirstUpper(GetBoneName(entity, info?.HitBone ?? 0));
            var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            var isProj = info?.IsProjectile() ?? false;
            if (IsMeleeType(dmgType) || IsProjectileType(dmgType))
            {
                if (atkStats.bodyPartsHit == null)
                {
                    atkStats.bodyPartsHit = new Dictionary<string, long>();
                    atkStats.bodyPartsHit[hitBone] = 1;
                }
                else
                {
                    long hits;
                    if (!atkStats.bodyPartsHit.TryGetValue(hitBone, out hits)) atkStats.bodyPartsHit[hitBone] = 1;
                    else atkStats.bodyPartsHit[hitBone]++;
                }
            }
            if (IsProjectileType(dmgType) && isProj)
            {
                float lastTime;
                if (!lastProjHit.TryGetValue(attacker.userID, out lastTime) || (UnityEngine.Time.realtimeSinceStartup - lastTime) >= 0.1f)
                {
                    atkStats.projHits += 1;
                    lastProjHit[attacker.userID] = UnityEngine.Time.realtimeSinceStartup;
                }
            }
            else if (IsMeleeType(dmgType)) atkStats.meleeHits += 1;
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || entity == null) return;
            var attacker = (info?.Initiator as BasePlayer) ?? info?.Weapon?.GetItem()?.GetOwnerPlayer() ?? info?.WeaponPrefab?.GetItem()?.GetOwnerPlayer() ?? BasePlayer.FindByID(info?.Weapon?.OwnerID ?? info?.WeaponPrefab?.OwnerID ?? 0);
            var victim = entity as BasePlayer;
            var vicStats = GetStatsInfo(victim);
            var atkStats = GetStatsInfo(attacker);
            if (IsValidPlayer(victim))
            {
              //  PrintWarning("is valid: " + victim.ShortPrefabName + ", " + victim.displayName);
                if (vicStats == null) vicStats = CreateStats(victim.UserIDString);
                vicStats.deaths += 1;
            }
            if (attacker != null && !(attacker != null && victim != null && attacker.UserIDString == victim.UserIDString) && IsValidPlayer(attacker))
            {
                if (atkStats == null) atkStats = CreateStats(attacker.UserIDString);
                atkStats.kills += 1;
                if (IsValidPlayer(victim))
                {
                    var atkDM = Deathmatch?.Call<bool>("InAnyDM", attacker.UserIDString) ?? false;
                    if (atkDM) atkStats.pvpKillsDM += 1;
                    atkStats.pvpKills += 1;
                    if (vicStats != null)
                    {
                        var vicDM = atkDM || (Deathmatch?.Call<bool>("InAnyDM", victim.UserIDString) ?? false);
                        if (vicDM) vicStats.pvpDeathsDM += 1;
                        vicStats.pvpDeaths += 1;
                    }
                }

                long prefabKills;
                if (!atkStats.prefabsKilled.TryGetValue(entity.prefabID, out prefabKills)) atkStats.prefabsKilled[entity.prefabID] = 1;
                else atkStats.prefabsKilled[entity.prefabID]++;
            }
        }

        private int GetAmountInInventory(BasePlayer player, int itemId)
        {
            var total = 0;

            var items = Pool.GetList<Item>();

            try
            {
                player?.inventory?.AllItemsNoAlloc(ref items);

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.info.itemid == itemId)
                        total += item.amount;
                }

            }
            finally { Pool.FreeList(ref items); }

            return total;
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (dispenser == null) return;

            var player = entity as BasePlayer;
            if (player == null) return;

            var info = GetStatsInfo(player) ?? CreateStats(player.UserIDString);
            if (info == null) return;

            var itemId = item?.info?.itemid ?? 0;

            var oldTotal = GetAmountInInventory(player, itemId);

            NextTick(() =>
            {
                var itemAmt = Mathf.Max(item?.amount ?? 0, (GetAmountInInventory(player, itemId) - oldTotal));

                long hit;
                if (!info.resourcesGathered.TryGetValue(itemId, out hit)) info.resourcesGathered[itemId] = itemAmt;
                else info.resourcesGathered[itemId] += itemAmt;
            });
        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            if (dispenser == null || player == null || item == null) return;
            var info = GetStatsInfo(player) ?? CreateStats(player.UserIDString);
            if (info == null) return;

            var itemId = item?.info?.itemid ?? 0;

            var oldTotal = GetAmountInInventory(player, itemId);

            NextTick(() =>
            {
                var itemAmt = Mathf.Max(item?.amount ?? 0, (GetAmountInInventory(player, itemId) - oldTotal));

                long hit;
                if (!info.resourcesGathered.TryGetValue(itemId, out hit)) info.resourcesGathered[itemId] = itemAmt;
                else info.resourcesGathered[itemId] += itemAmt;
            });
        }

        private void OnCollectiblePickup(Item item, BasePlayer player)
        {
            if (player == null || item == null) 
                return;

            var info = GetStatsInfo(player) ?? CreateStats(player.UserIDString);

            var itemId = item?.info?.itemid ?? 0;

            var oldTotal = GetAmountInInventory(player, itemId);

            NextTick(() =>
            {
                var itemAmt = Mathf.Max(item?.amount ?? 0, (GetAmountInInventory(player, itemId) - oldTotal));

                long hit;
                if (!info.resourcesGathered.TryGetValue(itemId, out hit)) info.resourcesGathered[itemId] = itemAmt;
                else info.resourcesGathered[itemId] += itemAmt;
            });
        }

        private void OnGrowableGathered(GrowableEntity plant, Item item, BasePlayer player)
        {
            if (player == null || item == null) 
                return;

            var info = GetStatsInfo(player) ?? CreateStats(player.UserIDString);

            var itemId = item?.info?.itemid ?? 0;

            var oldTotal = GetAmountInInventory(player, itemId);

            NextTick(() =>
            {
                var itemAmt = Mathf.Max(item?.amount ?? 0, (GetAmountInInventory(player, itemId) - oldTotal));

                long hit;
                if (!info.resourcesGathered.TryGetValue(itemId, out hit)) info.resourcesGathered[itemId] = itemAmt;
                else info.resourcesGathered[itemId] += itemAmt;
            });
        }

        private void AddGather(string userId, int itemId, int amount)
        {
            if (string.IsNullOrEmpty(userId)) return;
            var def = ItemManager.FindItemDefinition(itemId);
            if (def == null)
            {
                PrintWarning("AddGather called on null item def: " + itemId + " x" + amount);
                return;
            }
            var info = GetStatsInfo(userId) ?? CreateStats(userId);
            long hit;
            if (!info.resourcesGathered.TryGetValue(itemId, out hit)) info.resourcesGathered[itemId] = amount;
            else info.resourcesGathered[itemId] += amount;
        }

        private void AddGatherDef(string userId, ItemDefinition def, int amount)
        {
            if (string.IsNullOrEmpty(userId) || def == null) return;
            var itemId = def.itemid;
            var info = GetStatsInfo(userId) ?? CreateStats(userId);
            long hit;
            if (!info.resourcesGathered.TryGetValue(itemId, out hit)) info.resourcesGathered[itemId] = amount;
            else info.resourcesGathered[itemId] += amount;
        }

        private void AddGatherItem(string userId, Item item)
        {
            if (string.IsNullOrEmpty(userId) || item == null || item.amount < 1) return;
            var itemId = item.info.itemid;
            var amount = item.amount;
            var info = GetStatsInfo(userId) ?? CreateStats(userId);
            long hit;
            if (!info.resourcesGathered.TryGetValue(itemId, out hit)) info.resourcesGathered[itemId] = amount;
            else info.resourcesGathered[itemId] += amount;

        }



        #region Console Commands
        [ConsoleCommand("stats.clean")]
        private void consoleTPRB(ConsoleSystem.Arg arg)
        {
            if (arg == null) return;
            PrintWarning("stats.clean");
            var player = arg?.Player() ?? null;
            if (player != null && !player.IsAdmin)
            {
                PrintWarning("non-admin ran stats.clean");
                return;
            }
            SendReply(arg, "Clearing stats for banned players...");
            var watch = Stopwatch.StartNew();
            var stats = new Dictionary<string, StatsInfo>();
            foreach (var kvp in storedData.statsInfos) stats[kvp.Key] = kvp.Value;
            var banSB = new StringBuilder();
            var banCount = 0;
            foreach (var kvp in stats)
            {
                var isBanned = BanSystem?.Call<bool>("IsBanned", kvp.Key) ?? false;
                if (isBanned)
                {
                    banSB.AppendLine("Removing: " + kvp.Key);
                    banCount++;
                    storedData.statsInfos.Remove(kvp.Key);
                }

            }
            watch.Stop();
            SendReply(arg, banSB.ToString().TrimEnd());
            SendReply(arg, "Removed " + banCount.ToString("N0") + " banned stats, took: " + watch.Elapsed.TotalMilliseconds + "ms");
        }
        [ConsoleCommand("sapi.json")]
        private void consoleSAPIJson(ConsoleSystem.Arg arg)
        {
            if (!(arg?.IsAdmin ?? false)) return;

            if (arg?.Args == null || arg.Args.Length < 1) return;

            var sb = Pool.Get<StringBuilder>();
            try
            {
                var arg0 = arg.Args[0];

                ulong userId;
                if (!ulong.TryParse(arg0, out userId))
                {
                    arg.ReplyWith(sb.Clear().Append("Not a ulong: ").Append(arg0).ToString());
                    return;
                }

                var info = GetStatsInfo(userId);
             
                arg.ReplyWith((info == null) ? sb.Clear().Append("No stats for: ").Append(userId).ToString() : JsonConvert.SerializeObject(info, Formatting.Indented));
            }
            finally { Pool.Free(ref sb); }

        }
        #endregion
        #region Exposed API Calls


        private Dictionary<string, object> GetStatsDict(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            var stats = GetStatsInfo(userId);
            if (stats == null) return null;
            var newDict = new Dictionary<string, object>();
            newDict["kills"] = stats.kills;
            newDict["pvpkills"] = stats.pvpKills;
            newDict["deaths"] = stats.deaths;
            newDict["bodypartshit"] = stats.bodyPartsHit;
            newDict["resourcesgathered"] = stats.resourcesGathered;
            newDict["shots"] = stats.shots;
            newDict["projhits"] = stats.projHits;
            newDict["meleehits"] = stats.meleeHits;
            newDict["npchits"] = stats.npcHits;
            newDict["pvpdeaths"] = stats.pvpDeaths;
            return newDict;
        }

        private object GetStat(string userId, string statName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(statName)) return null;

            var info = GetStatsInfo(userId);
            if (info == null) return null;
        
            for (int i = 0; i < StatsInfoFields.Length; i++)
            {
                var curr = StatsInfoFields[i];
                if (curr.Name == statName) return curr.GetValue(info);
            }

            return null;
        }


        #endregion

        #region Helpers
        private string GetBoneName(BaseCombatEntity entity, uint boneId) => entity?.skeletonProperties?.FindBone(boneId)?.name?.english ?? "Body";
        private string FirstUpper(string original)
        {
            if (string.IsNullOrEmpty(original)) return string.Empty;
            var array = original.ToCharArray();
            array[0] = char.ToUpper(array[0]);
            return new string(array);
        }
        private bool IsValidPlayer(BasePlayer player) { return !(player == null || player.UserIDString == "0" || player.ShortPrefabName.Contains("scientist") || string.IsNullOrEmpty(player.UserIDString) || string.IsNullOrEmpty(player.displayName) || (player?.net?.connection == null && !player.IsSleeping())); }
        
        #endregion

    }
}