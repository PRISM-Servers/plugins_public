using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("CombatLog", "Shady", "1.0.0")]
    internal class CombatLog : RustPlugin
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
        public readonly List<Event> CombatEvents = new List<Event>();

        public struct Event
        {
            public float time;
            public ulong attacker_id;
            public ulong target_id;
            public string attacker;
            public string target;
            public string weapon;
            public string ammo;
            public string bone;
            public HitArea area;
            public float distance;
            public float health_old;
            public float health_new;
            public string info;
        }

        private Action _purgeAction = null;

        private void OnServerInitialized()
        {
            _purgeAction = new Action(() => PurgeEvents());
            if (ServerMgr.Instance != null) ServerMgr.Instance.InvokeRepeating(_purgeAction, 600f, 600f);
        }

        private void Unload()
        {
            if (_purgeAction != null && ServerMgr.Instance != null)
            {
                ServerMgr.Instance.CancelInvoke(_purgeAction);
                PrintWarning("Canceled PurgeAction invoke successfully");
            }
            else PrintWarning("PurgeAction was null or ServerMgr.Instance was null!!");
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;

            var vicPly = entity as BasePlayer;

            var atkPly = info?.InitiatorPlayer;

            if (vicPly == null && atkPly == null) return;
            
            var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
            if (dmgType == Rust.DamageType.Bleeding || dmgType == Rust.DamageType.Hunger || dmgType == Rust.DamageType.Thirst) return;

            var oldHP = entity?.Health() ?? 0f;
            NextTick(() =>
            {
                var dmg = info?.damageTypes?.Total() ?? 0f;
                var newHP = oldHP - dmg;
                if (vicPly != null && vicPly.IsConnected) Log(vicPly, info, oldHP, newHP);
                if (atkPly != null && atkPly.IsConnected) Log(atkPly, info, oldHP, newHP);
            });
        }

        public Transform GetTransform(BaseNetworkable entity)
        {
            if (entity == null || entity?.gameObject == null) return null;
            Transform transform = null;
            try { transform = entity?.transform ?? null; }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return transform;
        }

        public string GetName(BaseNetworkable entity)
        {
            if (entity == null || entity?.gameObject == null) return null;
            var name = string.Empty;
            try { name = entity?.name ?? string.Empty; }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return name;
        }

        public void Log(BasePlayer player, HitInfo info, float health_old, float health_new, string description = "")
        {
            var startPoint = info?.PointStart ?? Vector3.zero;

            if (startPoint == Vector3.zero) startPoint = GetTransform(info.HitEntity)?.position ?? Vector3.zero;
            if (startPoint == Vector3.zero) startPoint = GetTransform(player)?.position ?? Vector3.zero;

            var endPoint = info?.HitPositionWorld ?? Vector3.zero;

            if (endPoint == Vector3.zero) endPoint = GetTransform(info.Initiator)?.position ?? Vector3.zero;
            if (endPoint == Vector3.zero) endPoint = GetTransform(player)?.position ?? Vector3.zero;

            var weaponName = GetName(info.WeaponPrefab) ?? (info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic).ToString();

            Log(new Event()
            {
                time = Time.realtimeSinceStartup,
                attacker_id = info?.Initiator?.net?.ID.Value ?? 0,
                target_id = info?.HitEntity?.net?.ID.Value ?? player?.net?.ID.Value ?? 0,
                attacker = !(player == info.Initiator) ? ((info?.Initiator == null || info.Initiator.gameObject == null) ? "N/A" : info.Initiator.ShortPrefabName) : "you",
                target = !(player == info.HitEntity) ? ((info.HitEntity == null || info.HitEntity.gameObject == null) ? "N/A" : info.HitEntity.ShortPrefabName) : "you",
                weapon = weaponName,
                ammo = (info?.ProjectilePrefab == null | info?.ProjectilePrefab?.gameObject == null) ? "N/A" : info.ProjectilePrefab.name,
                bone = info.boneName,
                area = info.boneArea,
                distance = !info.IsProjectile() ? Vector3.Distance(startPoint, endPoint) : info.ProjectileDistance,
                health_old = health_old,
                health_new = health_new,
                info = description
            });
        }

        public void PurgeEvents()
        {
            if (CombatEvents == null || CombatEvents.Count < 1) return;
            
            var eventsTemporary = Facepunch.Pool.GetList<Event>();
            try 
            {
                for (int i = 0; i < CombatEvents.Count; i++) eventsTemporary.Add(CombatEvents[i]);

                var now = Time.realtimeSinceStartup;

                for (int i = 0; i < eventsTemporary.Count; i++)
                {
                    var tempEvent = eventsTemporary[i];
                    var time = tempEvent.time;

                    if (now - time >= 600)
                    {
                        CombatEvents.Remove(tempEvent);
                    }
                }

            }
            finally { Facepunch.Pool.FreeList(ref eventsTemporary); }
        }

        public void Log(Event val) => CombatEvents.Add(val);
        

        [ConsoleCommand("server.combatlog")]
        private void consoleCombatLog(ConsoleSystem.Arg arg)
        {
            if (arg == null) return;
            var player = arg?.Player() ?? null;
            if (player == null || !player.IsConnected) return;

            var netID = player?.net?.ID.Value ?? 0;


            var findEvents = Facepunch.Pool.GetList<Event>();
            try 
            {
                for (int i = 0; i < CombatEvents.Count; i++)
                {
                    var evt = CombatEvents[i];
                    if (evt.attacker_id == netID || evt.target_id == netID) findEvents.Add(evt);
                }

                if (findEvents == null || findEvents.Count < 1)
                {
                    SendReply(arg, "No combat logs!");
                    return;
                }

                var showCount = Mathf.Clamp(arg.GetInt(0, ConVar.Server.combatlogsize), 1, ConVar.Server.combatlogsize);

                var str = Get(findEvents, showCount);
                if (string.IsNullOrEmpty(str))
                {
                    SendReply(arg, "No string for combat log (report to an admin?!)");
                    return;
                }
                else SendReply(arg, str);
            }
            finally { Facepunch.Pool.FreeList(ref findEvents); }

           
        }

        [ConsoleCommand("combatlog")]
        private void consoleCombatLog2(ConsoleSystem.Arg arg) => consoleCombatLog(arg);

        public string Get(List<Event> events, int count)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            var textTable = Facepunch.Pool.Get<TextTable>();
            try 
            {
                textTable.Clear();

                textTable.AddColumn("time");
                textTable.AddColumn("attacker");
                textTable.AddColumn("id");
                textTable.AddColumn("target");
                textTable.AddColumn("id");
                textTable.AddColumn("weapon");
                textTable.AddColumn("ammo");
                textTable.AddColumn("area");
                textTable.AddColumn("distance");
                textTable.AddColumn("old_hp");
                textTable.AddColumn("new_hp");
                textTable.AddColumn("info");

                var combatlogdelay = ConVar.Server.combatlogdelay;

                var added = 0;
                for (int i = 0; i < events.Count; i++)
                {
                    if (i > count) break;

                    var @event = events[i];

                    var num3 = Time.realtimeSinceStartup - @event.time;
                    if (num3 < combatlogdelay) continue;


                    string str1 = num3.ToString("0.0").Replace(".0", string.Empty) + "s";
                    string attacker = @event.attacker;
                    string str2 = @event.attacker_id.ToString();
                    string target = @event.target;
                    string str3 = @event.target_id.ToString();
                    string weapon = @event.weapon;
                    string ammo = @event.ammo;
                    string lower = HitAreaUtil.Format(@event.area).ToLower();
                    string str4 = @event.distance.ToString("0.0").Replace(".0", string.Empty) + "m";
                    string str5 = @event.health_old.ToString("0.0").Replace(".0", string.Empty);
                    string str6 = @event.health_new.ToString("0.0").Replace(".0", string.Empty);
                    string info = @event.info;
                    textTable.AddRow(str1, attacker, str2, target, str3, weapon, ammo, lower, str4, str5, str6, info);
                    added++;
                }

                return added > 0 ? textTable.ToString() : string.Empty;
            }
            finally { Facepunch.Pool.Free(ref textTable); }
        }
    }
}
