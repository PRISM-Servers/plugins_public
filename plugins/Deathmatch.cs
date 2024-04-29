using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Core.Plugins;
using System.Reflection;
using System;
using Oxide.Core;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using Oxide.Game.Rust.Cui;
using Network;
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Pool = Facepunch.Pool;

namespace Oxide.Plugins
{
    [Info("Deathmatch", "Shady", "2.1.1221")]
    internal class Deathmatch : RustPlugin
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
        public string PluginNamePrefix { get { return Name + ":"; } }

        private bool init = false;
        #region Statics
        private const int PLAYER_LAYER = 131072;//LayerMask.GetMask(new string[] { "Player (Server)" });
        private readonly int _groundWorldConstLayer = LayerMask.GetMask("World", "Default", "Terrain", "Construction", "Deployable", "Prevent Building", "Deployed");
        private float boundary;

        private static readonly System.Random randomSpawn = new System.Random();
        private static readonly System.Random randomSkins = new System.Random();
        public static Deathmatch DMMain;
        public static EnvSync envSync;
        #endregion

        private readonly HashSet<string> _forbiddenTags = new HashSet<string> { "</color>", "</size>", "<b>", "</b>", "<i>", "</i>" };

        private readonly Regex _colorRegex = new Regex("(<color=.+?>)", RegexOptions.Compiled);
        private readonly Regex _sizeRegex = new Regex("(<size=.+?>)", RegexOptions.Compiled);

        private Coroutine _saveCoroutine = null;

        [PluginReference]
        private readonly Plugin Vanish;

        [PluginReference]
        private readonly Plugin Godmode;

        [PluginReference]
        private readonly Plugin NoEscape;

        [PluginReference]
        private readonly Plugin FindGround;

        [PluginReference]
        private readonly Plugin PUBG;

        [PluginReference]
        private readonly Plugin AAESPBlocker;

        private ArenaData arenaData;

        private List<ArenaInfo> Arenas
        {
            get { return arenaData?.arenas != null ? arenaData.arenas : new List<ArenaInfo>(); }
            set { if (arenaData != null) arenaData.arenas = value; }
        }

        private class ArenaData
        {

            public List<ArenaInfo> arenas = new List<ArenaInfo>();
            public ArenaData() { }
        }

        public enum StrikeType { Rockets, Cannons, Nuke };
        public enum GunType { Pistol, AssaultRifle, Rifle, Shotgun, SMG, LMG, RPG, None };

        public static GunType GetGunType(string shortName)
        {
            if (string.IsNullOrEmpty(shortName)) return GunType.None;
            var findDef = ItemManager.FindItemDefinition(shortName);
            if (findDef == null || findDef.category != ItemCategory.Weapon) return GunType.None;
            if (shortName.Contains("smg")) return GunType.SMG;
            if (shortName.Contains("rifle.ak") || shortName.Contains("300")) return GunType.AssaultRifle;
            if (shortName.Contains("bolt") || shortName.Contains("rifle.semi")) return GunType.Rifle;
            if (shortName.Contains("lmg")) return GunType.LMG;
            if (shortName.Contains("pistol")) return GunType.Pistol;
            if (shortName.Contains("shotgun")) return GunType.Shotgun;
            if (shortName.Contains("launcher")) return GunType.RPG;
            return GunType.None;
        }

        private class DMData
        {
            public Dictionary<string, string> lastPlayerPositions = new Dictionary<string, string>();
            public Dictionary<string, string> lastDMTime = new Dictionary<string, string>();
            public Dictionary<string, string> lastDMName = new Dictionary<string, string>();
            public Dictionary<string, ArenaInfo.Team> lastDMTeam = new Dictionary<string, ArenaInfo.Team>();
            public DMData() { }
        }

        private DMData dmData;

        #region Dictionaries
        private readonly Dictionary<ulong, int> strikeAmounts = new Dictionary<ulong, int>();
        private readonly Dictionary<ulong, float> lastFiredWeaponTime = new Dictionary<ulong, float>();
        private readonly Dictionary<ulong, float> lastAttackTime = new Dictionary<ulong, float>();
        private readonly Dictionary<Item, ulong> DMowners = new Dictionary<Item, ulong>();
        private BuildingBlock strikeFoundation;
        private readonly Dictionary<ulong, float> lastStrikeTime = new Dictionary<ulong, float>();
        private readonly Dictionary<ulong, float> dmJoinTime = new Dictionary<ulong, float>();
        private readonly Dictionary<string, Vector3> lastDeath = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, float> lastDeathTime = new Dictionary<string, float>();
        private readonly Dictionary<string, int> heliHitTimes = new Dictionary<string, int>();
        private readonly Dictionary<ulong, Dictionary<string, float>> oldMetabolism = new Dictionary<ulong, Dictionary<string, float>>();
        private readonly Dictionary<ulong, int> killsWithoutDying = new Dictionary<ulong, int>();
        private readonly Dictionary<int, Dictionary<int, ulong>> itemSkins = new Dictionary<int, Dictionary<int, ulong>>();
        private readonly Dictionary<string, float> lastCheckTime = new Dictionary<string, float>();
        private readonly Dictionary<string, float> lastNoteTime = new Dictionary<string, float>();
        private readonly Dictionary<ulong, int> highPingTimes = new Dictionary<ulong, int>();
        private readonly Dictionary<ItemId, float> grenadeCook = new Dictionary<ItemId, float>();
        private readonly Dictionary<ulong, float> lastCookTime = new Dictionary<ulong, float>();
        #endregion

        #region FieldInfos
        public static FieldInfo _curList = typeof(InvokeHandler).GetField("curList", BindingFlags.Instance | BindingFlags.NonPublic);
        #endregion

        #region References
        [PluginReference]
        private readonly Plugin ZoneManager;

        [PluginReference]
        private readonly Plugin Playtimes;

        [PluginReference]
        private readonly Plugin HeliControl;

        [PluginReference]
        private readonly Plugin Airstrike;

        [PluginReference]
        private readonly Plugin AttackPlanes;
        #endregion

        #region Hooks

        private void OnLoseCondition(Item item, float amount)
        {
            if (item == null || amount <= 0.0f) return;
            var player = item?.GetOwnerPlayer() ?? null;
            player = player ?? item?.parent?.entityOwner?.GetComponent<BaseProjectile>()?.GetOwnerPlayer() ?? null;
            if (player == null) return;
            if (InAnyDM(player.UserIDString)) NextTick(() => item?.RepairCondition(item?.maxCondition ?? 0f));
        }

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item newItem)
        {
            if (player == null || newItem == null) return;
            var getDM = GetPlayerDM(player);
            if (getDM != null && getDM.Airstrikes && newItem.info.shortname == "tool.binoculars")
            {
                ShowPopup(player.UserIDString, "<color=#ff3d81>Aim your binoculars and click to call an airstrike on that position!</color>");
            }
        }

        private void OnLootSpawn(LootContainer container)
        {
            if (container == null) return;
            var prefabName = container?.ShortPrefabName ?? string.Empty;
            if (!prefabName.Contains("tier3")) return;
            var pos = container?.transform?.position ?? Vector3.zero;
            timer.Once(0.5f, () =>
            {
                if (container == null || container.IsDestroyed) return;
                var findGun = container?.inventory?.itemList?.Where(p => p.info.category == ItemCategory.Weapon && (p?.GetHeldEntity()?.GetComponent<BaseProjectile>() != null))?.FirstOrDefault() ?? null;
                if (findGun == null)
                {
                    PrintWarning("No gun for: " + container.ShortPrefabName + ", " + pos);
                    return;
                }
                if (findGun != null)
                {
                    var type = GetGunType(findGun.info.shortname);
                    var oldAmmo = findGun?.GetHeldEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine?.ammoType ?? null;
                    var rarity = findGun.info.rarity;
                    var rngReplace = UnityEngine.Random.Range(0, 101);
                    var rngNeed = (type == GunType.SMG && findGun.info.shortname.Contains("mp5")) ? 10 : (type == GunType.SMG) ? 30 : (type == GunType.AssaultRifle) ? 40 : (type == GunType.Shotgun && findGun.info.shortname.Contains("spas")) ? 12 : (type == GunType.Shotgun) ? 25 : (type == GunType.Rifle && findGun.info.shortname.Contains("bolt")) ? 10 : (type == GunType.Rifle) ? 30 : (type == GunType.RPG) ? 65 : 40;

                    if (rngReplace <= rngNeed)
                    {
                        //PrintWarning("rngReplace <= rngNeed: " + rngReplace + " <= " + rngNeed + ", for: " + findGun.info.shortname + ", " + rarity);
                        var replace = ItemManager.itemList?.Where(p => p != null && p.shortname != findGun.info.shortname && ((type == GunType.RPG && GetGunType(p.shortname) != GunType.None) || GetGunType(p.shortname) == type) && (p.rarity == rarity || (rarity != Rust.Rarity.None && p.rarity >= rarity)))?.ToList()?.GetRandom() ?? null;
                        if (replace == null)
                        {
                            PrintWarning("replace is null!! original: " + findGun?.info?.displayName?.english);
                            return;
                        }
                        //PrintWarning("Got replace: " + replace.shortname);
                        var oldPos = findGun?.position ?? -1;
                        var oldParent = findGun?.parent ?? null;
                        RemoveFromWorld(findGun);
                        var replaceItem = ItemManager.Create(replace, 1);
                        if (replaceItem == null)
                        {
                            PrintWarning("replaceItem null somehow?!");
                            return;
                        }
                        if (!replaceItem.MoveToContainer(oldParent, oldPos) && !replaceItem.MoveToContainer(oldParent))
                        {
                            RemoveFromWorld(replaceItem);
                            PrintWarning("replaceItem couldn't be moved!!");
                            return;
                        }
                        var proj = replaceItem?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                        if (proj == null)
                        {
                            PrintWarning("baseprojectile is null: " + replace?.shortname);
                            return;
                        }
                        var newAmmo = proj?.primaryMagazine?.ammoType ?? null;
                        var capacity = proj?.primaryMagazine?.capacity ?? 0;
                        var ammoAmount = (int)Math.Round(capacity * 0.9, MidpointRounding.AwayFromZero);
                        //   var ammoAmount = proj?.primaryMagazine?.capacity ?? 8;
                        if (proj?.primaryMagazine != null) proj.primaryMagazine.contents = capacity;
                        if (ammoAmount < 5) ammoAmount *= (int)Math.Round(UnityEngine.Random.Range(1.7f, 3f), MidpointRounding.AwayFromZero);
                        if (newAmmo != null)
                        {
                            //        PrintWarning("oldAmmo != newAmmo");
                            var allAmmo = container?.inventory?.itemList?.Where(p => p != null && p.info.category == ItemCategory.Ammunition)?.ToList() ?? null;
                            if (allAmmo != null && allAmmo.Count > 0) for (int i = 0; i < allAmmo.Count; i++) RemoveFromWorld(allAmmo[i]);
                            var ammoItem = ItemManager.Create(newAmmo, ammoAmount);
                            if (ammoItem == null)
                            {
                                PrintWarning("new ammo item is null!!");
                                return;
                            }
                            if (!ammoItem.MoveToContainer(oldParent))
                            {
                                RemoveFromWorld(ammoItem);
                                PrintWarning("ammoItem couldn't be moved!!");
                                return;
                            }
                        }
                    }
                }
            });
        }

        private void Unload()
        {
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var player = BasePlayer.activePlayerList[i];
                    if (player == null || !(player?.IsConnected ?? false)) continue;
                    CuiHelper.DestroyUi(player, "BlindUI");
                    CuiHelper.DestroyUi(player, "BlindUIText");
                    CuiHelper.DestroyUi(player, "DMGUI");
                    CuiHelper.DestroyUi(player, "DMSGUI");
                    CuiHelper.DestroyUi(player, "DMAlive");
                    CuiHelper.DestroyUi(player, "DMAliveText");
                    CuiHelper.DestroyUi(player, "DMKills");
                    CuiHelper.DestroyUi(player, "DMKillsText");
                    var getDM = GetPlayerDM(player);
                    if (getDM != null)
                    {
                        if (getDM.Teams) dmData.lastDMTeam[player.UserIDString] = getDM.GetTeam(player);
                        if (!getDM.LeaveDM(player)) PrintWarning("getDM was not null but player could not be forced to leave!!");
                        else
                        {
                            dmData.lastDMTime[player.UserIDString] = DateTime.Now.ToString();
                            dmData.lastDMName[player.UserIDString] = getDM.ArenaName;
                        }
                    }
                }
                var exSB = new StringBuilder();
                try
                {
                    var invList = InvokeList;
                    var objStr = this?.Object?.ToString() ?? string.Empty;
                    //   var invokeEnum = (invList == null || invList.Count < 1 || string.IsNullOrEmpty(objStr)) ? null : invList?.Where(p => p.Key != null && (p.Key.action?.Target?.ToString() ?? string.Empty).Contains(objStr)) ?? null;
                    var newEnums = new ListDictionary<InvokeAction, float>();
                    foreach (var inv in invList)
                    {
                        try { if (inv.Key == null || inv.Key.action == null || inv.Key.action.Target == null) continue; }
                        catch (Exception ex)
                        {
                            exSB.AppendLine(ex.ToString() + " (couldn't even do null checks without an error!");
                            continue;
                        }

                        var strTarg = string.Empty;
                        try { strTarg = inv.Key.action?.Target?.ToString() ?? string.Empty; }
                        catch (Exception ex)
                        {
                            exSB.AppendLine(ex.ToString());
                            continue;
                        }

                        if (!string.IsNullOrEmpty(strTarg) && strTarg.Contains(objStr)) newEnums.Add(inv.Key, inv.Value);
                    }
                    if (newEnums != null && newEnums.Count > 0)
                    {
                        PrintWarning("Got newEnums: " + newEnums.Count);
                        foreach (var inv in newEnums) InvokeHandler.CancelInvoke(inv.Key.sender, inv.Key.action);
                    }
                    else PrintWarning("no newEnums!");

                }
                catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Error on Unload() for invokes"); }
                if (exSB.Length > 0) PrintError(exSB.ToString());
                /*/
                try
                {
                    var invokes = InvokeList.Where(p => (p.Key.action?.Target ?? null).ToString() == this.Object.ToString())?.ToList() ?? null;
                    if (invokes != null && invokes.Count > 0)
                    {
                        var invokeSB = new StringBuilder();
                        for (int i = 0; i < invokes.Count; i++)
                        {
                            var inv = invokes[i];
                            try
                            {
                                InvokeHandler.CancelInvoke(inv.Key.sender, inv.Key.action);
                                invokeSB.AppendLine("cancel invoke: " + inv.Key.action.Method.Name);
                            }
                            catch(Exception ex) { PrintError(ex.ToString()); }

                        }
                        PrintWarning(invokeSB.ToString().TrimEnd());
                    }
                }
                catch(Exception ex) { PrintError(ex.ToString()); }/*/

                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena == null) continue;
                    if (arena.GameTimer != null)
                    {
                        arena.GameTimer.Destroy();
                        arena.GameTimer = null;
                    }
                    arena.EndSphere();
                }
                SaveData();
            }
            finally
            {
                DMMain = null;
            }

        }

        private object CanUseOverdrive(BasePlayer player)
        {
            if (player == null) return null;

            if (InAnyDM(player.UserIDString))
                return false;
            

            return null;
        }

        private object CanTeleport(BasePlayer player)
        {
            if (player == null) return null;
            if (InAnyDM(player.UserIDString)) return "You cannot teleport while in Deathmatch!";
            return null;
        }

        private object CanPlayerTrade(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return null;
            if (InAnyDM(player.UserIDString)) return "You cannot trade while in Deatchmatch!";
            return null;
        }
        /*/
        private object OnPlayerDropActiveItem(BasePlayer player, Item item)
        {
            if (player == null || item == null) return null;
            var getDM = GetPlayerDM(player);
            if (getDM != null && !getDM.AllowDroppingActiveItem)
            {
                if (!getDM.AllowDropping)
                {

                }
                return true;
            }
         //   if (InAnyDM(player.UserIDString)) return true;
            return null;
        }/*/

        private object CanLootEntity(BasePlayer player, StorageContainer target)
        {
            if (player == null || target == null) return null;
            var getDM = GetPlayerDM(player);
            if (target.ShortPrefabName.Contains("dm ") && getDM == null && !player.IsAdmin) return false;
            if (getDM == null || string.IsNullOrEmpty(getDM.ZoneID)) return null;
            var lootContainer = target as LootContainer;
            if (lootContainer != null) return null;
            var targetPly = target?.GetComponent<BasePlayer>() ?? null;
            if (targetPly != null) return false;
            if (!isEntityInZone(target, getDM.ZoneID)) return false;
            return null;
        }

        private object CanLootPlayer(BasePlayer target, BasePlayer player)
        {
            if (player == null || target == null) return null;
            if (!player.IsAdmin && (InAnyDM(target.UserIDString) || InAnyDM(player.UserIDString))) return false;
            return null;
        }

        private object OnMaxStackable(Item item)
        {
            if (item == null) return null;
            var player = item?.parent?.playerOwner ?? item?.GetOwnerPlayer() ?? null;
            if (player == null) return null;
            var getDM = GetPlayerDM(player);
            if (getDM != null && getDM.IgnoreStacks) return int.MaxValue - 1;
            return null;
        }

        private object CanStackItem(Item item, Item targetItem)
        {
            if (item == null || targetItem == null) return null;
            if (item?.info?.shortname != targetItem?.info?.shortname) return null;
            var player = targetItem?.parent?.playerOwner ?? targetItem?.GetOwnerPlayer() ?? item?.parent?.playerOwner ?? item?.GetOwnerPlayer() ?? null;
            if (player == null) return null;
            var getDM = GetPlayerDM(player);
            if (getDM != null && getDM.IgnoreStacks) return true;
            return null;
        }

        //   var hookVal = Interface.Oxide.CallHook("CanScaleNomadAttackerDamage", attackerPlayer, hitInfo);

        private object CanScaleNomadAttackerDamage(BasePlayer attacker, HitInfo info)
        {
            if (InAnyDM(attacker.UserIDString)) return false;

            return null;
        }

        private object CanScaleNomadVictimDamage(BasePlayer attacker, HitInfo info)
        {
            if (InAnyDM(attacker.UserIDString)) return false;

            return null;
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            var player = entity as BasePlayer;
            if (player == null || dispenser == null) return;
            var getDM = GetPlayerDM(player);
            if (getDM != null && !getDM.AllowGather)
            {
                //  var dispEnt = dispenser?.GetComponent<BaseEntity>() ?? null;
                //    if (dispEnt != null && !dispEnt.IsDestroyed) dispEnt.Kill();
                item.amount = 0;
            }
        }

        private object OnRunPlayerMetabolism(PlayerMetabolism metabolism)
        {
            if (metabolism == null) return null;
            var player = metabolism?.GetComponent<BasePlayer>() ?? null;
            if (player == null || (player?.IsDead() ?? true) || !player.IsConnected) return null;
            var getDM = GetPlayerDM(player);
            if (getDM != null)
            {
                metabolism.calories.value = 500;
                metabolism.hydration.value = 250;
                if (metabolism.temperature.value < 25) metabolism.temperature.value = 25;
                metabolism.radiation_level.value = 0;
                metabolism.radiation_poison.value = 0;
                if (getDM.BleedingScalar <= 0) metabolism.bleeding.value = 0;
                metabolism.SendChangesToClient();
                if (getDM.Fortnite) return true;
            }
            return null;
        }

        private void OnHealingItemUse(MedicalTool tool, BasePlayer player)
        {
            if (tool == null || player == null) return;
            var arena = GetPlayerDM(player);
            if (arena == null || arena.HealingScalar == 1f) return;
            var oldHP = player?.Health() ?? 0f;
            NextTick(() =>
            {
                if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead()) return;
                var newHP = player?.Health() ?? 0f;
                var healAmount = newHP - oldHP;

                var newHeal = healAmount * arena.HealingScalar;
                //PrintWarning("oldHP: " + oldHP + " new hp: " + newHP + ", healAmount: " + healAmount + " newHeal: " + newHeal);
                player.health = oldHP + newHeal;
                player?.metabolism?.SendChangesToClient();
            });
        }

        private object CanHelicopterStrafeTarget(PatrolHelicopterAI entity, BasePlayer target)
        {
            if (entity == null || target == null) return null;
            if (InAnyDM(target.UserIDString)) return false;
            return null;
        }

        private object CanHelicopterTarget(PatrolHelicopterAI heli, BasePlayer player)
        {
            if (heli == null || player == null || IsVanished(player)) return null;
            if (InAnyDM(player.UserIDString)) return false;
            return null;
        }

        private object OnHelicopterTarget(HelicopterTurret turret, BaseCombatEntity entity)
        {
            if (turret == null || entity == null) return null;
            var player = entity as BasePlayer;
            if (player == null || !(player?.IsConnected ?? false) || IsVanished(player)) return null;
            if (InAnyDM(player.UserIDString)) return false;
            return null;
        }

        private object CanBeTargeted(BaseCombatEntity entity, MonoBehaviour monoTurret)
        {
            if (!init || entity == null || (entity?.IsDestroyed ?? true) || monoTurret == null) return null;
            try
            {
                if (!(monoTurret is HelicopterTurret)) return null;
                if (!(entity is BasePlayer)) return null;
                var player = entity as BasePlayer;
                if (player == null) return null;
                if (IsVanished(player)) return null;
                if (InAnyDM(player.UserIDString)) return false;
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete CanBeTargeted^"); }
            return null;
        }

        private object CanBeWounded(BasePlayer player, HitInfo hitinfo)
        {
            if (player == null) return null;
            var getDM = GetPlayerDM(player);
            if (getDM != null && getDM.Fortnite) return false;
            return null;
        }

        private object OnServerCommand(ConsoleSystem.Arg arg)
        {
            var IP = arg?.Connection?.ipaddress ?? string.Empty;
            if ((arg?.Player() ?? null) == null && !string.IsNullOrEmpty(IP)) PrintWarning("Command without player: " + IP + ", " + (arg?.cmd?.FullName ?? "Unknown"));
            if (arg == null || arg.Connection == null || arg.cmd == null || arg.cmd.Name == null || !init) return null;
            var player = arg?.Player() ?? null;
            if (player == null) return null;
            var command = arg?.cmd?.FullName ?? string.Empty;
            ArenaInfo getDM;
            if (command.Contains("kill") && (getDM = GetPlayerDM(player)) != null)
            {

                var inZone = isPlayerInZone(player, getDM.ZoneID);
                if (!inZone)
                {
                    PrintWarning("got all dms but not in zone: " + player.displayName);
                    arg.ReplyWith("You cannot use this right now!");
                    return false;
                }
            }
            if ((command.Contains("addtoteam") || command.Contains("trycreateteam") || command.Contains("acceptinvite")) && InAnyDM(player.UserIDString))
            {
                SendReply(player, "You cannot make teams while in Deathmatch!");
                return false;
            }
            return null;
        }

        private object OnPlayerViolation(BasePlayer player, AntiHackType type, float amount)
        {
            if (player == null || amount < 0) return null;
            var getDM = GetPlayerDM(player);
            if (getDM == null) return null;
            if (player.lifeStory != null && GetTimeAlive(player) <= 5) return true;
            if (getDM.SoftAntiHack && type == AntiHackType.FlyHack) return true;
            return null;
        }

        private readonly Dictionary<string, float> lastScoreGUI = new Dictionary<string, float>();
        private readonly Dictionary<BasePlayer, Timer> kickTimer = new Dictionary<BasePlayer, Timer>();
        private readonly Dictionary<ulong, float> lastBinocularTime = new Dictionary<ulong, float>();

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player == null || input == null) return;
            if (player.IsDead() || player.IsSleeping() || player.IsWounded()) return;
            var watch = Stopwatch.StartNew();
            var heldItem = player?.GetActiveItem() ?? null;
            var isGrenade = (heldItem?.info?.shortname ?? string.Empty) == "grenade.f1";
            var arena = GetPlayerDM(player);
            if (arena != null && arena.Airstrikes && heldItem?.info?.shortname == "tool.binoculars")
            {
                float lastBinoc;
                if (lastBinocularTime.TryGetValue(player.userID, out lastBinoc) && (Time.realtimeSinceStartup - lastBinoc) < 2)
                {
                    return;
                }
                if (input.WasJustPressed(BUTTON.FIRE_PRIMARY) && (input.IsDown(BUTTON.FIRE_SECONDARY) || input.WasDown(BUTTON.FIRE_SECONDARY)))
                {
                    int airstrikes;
                    if (player.IsAdmin || (strikeAmounts.TryGetValue(player.userID, out airstrikes) && airstrikes > 0))
                    {
                        player.SendConsoleCommand("dm.strike");
                    }
                    else
                    {
                        ShowPopup(player.UserIDString, "No airstrike available!", 3.5f);
                    }
                    lastBinocularTime[player.userID] = Time.realtimeSinceStartup;
                }
            }
            if (isGrenade && arena != null && input.IsDown(BUTTON.FIRE_PRIMARY))
            {
                var lastCookTimeF = 0f;
                var grenadeCookFuse = 0f;
                if (!grenadeCook.TryGetValue(heldItem.uid, out grenadeCookFuse)) grenadeCookFuse = grenadeCook[heldItem.uid] = 5f;
                var eyePos = player?.eyes?.position ?? Vector3.zero;
                var centerPos = player?.CenterPoint() ?? Vector3.zero;
                if (!lastCookTime.TryGetValue(player.userID, out lastCookTimeF))
                {
                    lastCookTime[player.userID] = Time.realtimeSinceStartup;
                    grenadeCookFuse -= 1f;
                }
                else
                {
                    if ((Time.realtimeSinceStartup - lastCookTimeF) >= 1)
                    {
                        grenadeCookFuse -= 1f;
                        lastCookTime[player.userID] = Time.realtimeSinceStartup;
                    }
                }
                if (grenadeCookFuse != grenadeCook[heldItem.uid])
                {
                    SendLocalEffect(player, "assets/bundled/prefabs/fx/weapons/landmine/landmine_trigger.prefab", 1, 0, new Vector3(0, 0.125f));
                }
                if (grenadeCookFuse <= 0.05f)
                {
                    Effect.server.Run("assets/prefabs/weapons/f1 grenade/effects/f1grenade_explosion.prefab", eyePos, Vector3.zero);
                    var newDmgList = new List<Rust.DamageTypeEntry>();
                    var newDmg = new Rust.DamageTypeEntry
                    {
                        amount = UnityEngine.Random.Range(50, 75),
                        type = Rust.DamageType.Stab
                    };
                    newDmgList.Add(newDmg);
                    newDmg = new Rust.DamageTypeEntry
                    {
                        amount = UnityEngine.Random.Range(20, 75),
                        type = Rust.DamageType.Explosion
                    };
                    newDmgList.Add(newDmg);
                    DamageUtil.RadiusDamage(player, heldItem.GetHeldEntity(), player.eyes.position, 3f, 3f, newDmgList, PLAYER_LAYER, true);
                    if (heldItem.amount > 1)
                    {
                        heldItem.amount--;
                        heldItem.MarkDirty();
                        grenadeCook[heldItem.uid] = 5f;
                    }
                    else RemoveFromWorld(heldItem);
                    return;
                }
                grenadeCook[heldItem.uid] = Mathf.Clamp(grenadeCookFuse, 0.05f, 6);
            }


            if (arena != null && arena.Teams)
            {
                if (player.IsSpectating() && arena.SpectateTeamDead && arena.Teams && arena.GetTeamCount(arena.GetTeam(player)) > 1)
                {
                    if (input.WasJustPressed(BUTTON.DUCK) || input.WasJustPressed(BUTTON.JUMP))
                    {
                        var rngTeam = arena.GetTeamPlayers(arena.GetTeam(player))?.Where(p => p != player && !p.IsDead() && !p.IsSpectating() && p.UserIDString != player.spectateFilter)?.ToList()?.GetRandom() ?? null;
                        if (rngTeam != null) player.spectateFilter = rngTeam.UserIDString;
                    }
                }

                float lastGUI;
                if (!lastScoreGUI.TryGetValue(player.UserIDString, out lastGUI) || (Time.realtimeSinceStartup - lastGUI) > 1.3f)
                {
                    if (input.IsDown(BUTTON.SPRINT) && input.IsDown(BUTTON.USE))
                    {
                        ScoreGUILarge(player, arena, 1.33f);
                        lastScoreGUI[player.UserIDString] = Time.realtimeSinceStartup;
                    }
                }
            }
            if (arena != null && arena.Fortnite)
            {
                if (player.IsSpectating() && (input.WasJustPressed(BUTTON.DUCK) || input.WasJustPressed(BUTTON.JUMP)))
                {
                    PrintWarning("Spectate filter prior to pressing space was: " + player.spectateFilter);
                    var rngSpec = arena?.ActivePlayers?.Where(p => p != null && !p.IsDead() && !p.IsSpectating() && p != player && !p.UserIDString.Equals(player.spectateFilter.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))?.ToList()?.GetRandom() ?? null;
                    if (rngSpec != null)
                    {
                        player.spectateFilter = rngSpec.UserIDString;
                        PrintWarning("Spectate filter after pressing space was: " + player.spectateFilter);
                    }
                }
            }
            var now = Time.realtimeSinceStartup;
            var lastTime = 0f;
            if (!lastCheckTime.TryGetValue(player.UserIDString, out lastTime)) lastCheckTime[player.UserIDString] = now - 10;
            else if ((lastTime = now - lastTime) < 2.5) return;

            if (arena != null)
            {
                var zoneID = arena?.ZoneID ?? string.Empty;
                if (!isPlayerInZone(player, zoneID) && !player.IsSpectating() && GetTimeAlive(player) >= 1.5f)
                {
                    if (arena.Fortnite) player.Hurt(UnityEngine.Random.Range(3f, 8f), Rust.DamageType.ElectricShock, null, false);
                    else
                    {
                        Timer outTimer;
                        if (!kickTimer.TryGetValue(player, out outTimer) || outTimer == null || outTimer.Destroyed)
                        {
                            Timer newTimer = null;
                            newTimer = timer.Once(1f, () =>
                            {
                                if (player == null || player.IsDead()) return;
                                if (!isPlayerInZone(player, zoneID))
                                {
                                    SendReply(player, "You are being kicked because you're outside of the <color=#fcb355>" + arena.ArenaName + "</color> zone!");
                                    arena.LeaveDM(player);
                                    GrantImmunity(player, 2.5f);
                                    WakePlayerUp(player, true);
                                    newTimer = null;
                                    kickTimer[player] = null;
                                }
                            });
                            kickTimer[player] = newTimer;
                        }
                    }
                }
            }

            if (!player.IsAdmin)
            {
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena2 = Arenas[i];
                    if (arena2 == null || arena2.Disabled || arena2.Fortnite || arena2.ZoneID == "0" || string.IsNullOrEmpty(arena2.ZoneID)) continue;
                    if (isPlayerInZone(player, arena2.ZoneID) && !arena2.ActivePlayers.Contains(player))
                    {
                        SendReply(player, "You are in the <color=#fcb355>" + arena2.ArenaName + "</color> deathmatch zone, but you have not joined! Please leave this area.");
                        if (player?.metabolism != null)
                        {
                            player.metabolism.radiation_poison.MoveTowards(player.metabolism.radiation_poison.value + 36f, 5f);
                            player.metabolism.radiation_level.value = player.metabolism.radiation_poison.value;
                        }
                    }
                }
            }

            var lastNote = 0f;
            if (!lastNoteTime.TryGetValue(player.UserIDString, out lastNote)) lastNoteTime[player.UserIDString] = now - 10;
            else lastNote = now - lastNote;
            if (lastNote < 5) return;
            lastCheckTime[player.UserIDString] = now;
            var isDM = arena != null;
            if (isDM)
            {
                var HPT = 0;
                if (highPingTimes.TryGetValue(player.userID, out HPT) && HPT > 9)
                {
                    arena.LeaveDM(player);
                    SendReply(player, "<color=#a6fc60>You were kicked from DM because your ping was too high (300+) for too long! Please try again later.</color>");
                    highPingTimes[player.userID] = 0;
                    PrintWarning("Kicking: " + player.displayName + " for too high ping for too long!");
                    return;
                }
                var maxPing = arena?.MaxPing ?? -1;
                if (maxPing != -1 && GetPing(player) >= maxPing)
                {
                    highPingTimes[player.userID] = HPT++;
                    SendReply(player, "Your ping is too high! If it does not go down soon, you'll be kicked from DM!");
                }
                else highPingTimes[player.userID] = 0;
                if (lastNote >= 8)
                {
                    var noteSB = new StringBuilder();
                    if (IsVanished(player)) noteSB.AppendLine("<size=16>NOTE: You're vanished while in Deathmatch!</size>");
                    if (IsGodmode(player)) noteSB.AppendLine("<size=16>NOTE: You're in Godmode while in Deathmatch!</size>");
                    if (noteSB.Length > 0)
                    {
                        SendReply(player, noteSB.ToString().TrimEnd());
                        lastNoteTime[player.UserIDString] = now;
                    }
                }
                if (!player.IsAdmin && !player.IsSpectating())
                {
                    var idle = player?.IdleTime ?? 0f;
                    if (idle >= 300)
                    {
                        SendReply(player, "Kicking you from DM for being idle too long!");
                        arena.LeaveDM(player);
                        return;
                    }
                }
            }
            watch.Stop();
            if (watch.Elapsed.TotalMilliseconds > 1.5) PrintWarning("OnPlayerInput for Deathmatch took: " + watch.Elapsed.TotalMilliseconds + "ms");
        }

        private float TimeSinceFiredWeapon(BasePlayer player)
        {
            if (player == null) return -1f;
            float lastTime;
            if (lastFiredWeaponTime.TryGetValue(player.userID, out lastTime)) return Time.realtimeSinceStartup - lastTime;
            return -1f;
        }

        private float TimeSinceAttacked(BasePlayer player)
        {
            if (player == null) return -1f;
            float lastTime;
            if (lastAttackTime.TryGetValue(player.userID, out lastTime)) return Time.realtimeSinceStartup - lastTime;
            return -1f;
        }

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles) => lastFiredWeaponTime[player.userID] = Time.realtimeSinceStartup;

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;
            var findDM = GetPlayerDM(player) ?? null;
            if (findDM != null)
            {
                GrantImmunity(player, 1.5f);
                findDM.LeaveDM(player);
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            RemoveAllDMItems(player);

            if (Arenas != null && Arenas.Count > 0)
            {
                ArenaInfo findForce = null;
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena != null && arena.ForceJoin)
                    {
                        findForce = arena;
                        break;
                    }
                }

                if (findForce == null) return;

                PrintWarning("Force joining DM: " + findForce.ArenaName + " for player: " + player.displayName);
                if (!findForce.JoinDM(player))
                {
                    PrintWarning("!!!Could not forcefully join DM: " + player.displayName + "!!!");
                    if (findForce.RoundBased && findForce.GameInProgress && !findForce.JoinMidRound)
                    {
                        PrintWarning("findForce is roundbased, game in progress & no join mid round, attempting queue...");
                        if (findForce.JoinQueue(player, false)) PrintWarning("Did join queue!");
                        else PrintWarning("Did not join queue!!");
                    }
                }
            }
        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            if (player == null) return;
            if (!InAnyDM(player.UserIDString)) RemoveAllDMItems(player);
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null) return;
            if (InAnyDM(player.UserIDString)) player.SendConsoleCommand("gametip.hidegametip");
        }

        /*/
        void OnPlayerRespawned(BasePlayer player)
        {
            NextTick(() =>
            {
                if (player == null) return;
                if (activeDM.Contains(player.userID) && !isPlayerInDM(player))
                {
                    PrintWarning("Calling OnPlayerRespawned: " + player.displayName + ", pos: " + (player?.transform?.position ?? Vector3.zero));
                    if (DoSpawns(player))
                    {
                        SendReply(player, "You have been teleported back to the DM arena! If you wish to leave, type /DM leave");
                        PrintWarning("Now pos: " + (player?.transform?.position ?? Vector3.zero));
                        lastAttacked.SetValue(player, UnityEngine.Time.realtimeSinceStartup - 60);

                        createDMItems(player);
                    }
                    else PrintWarning("Failed to do spawns for: " + player.displayName + " at OnPlayerRespawned");
                }
            });
        }/*/

        private object CanCrit(BasePlayer attacker, HitInfo info)
        {
            if (attacker == null || info == null) return null;
            var victim = info?.Initiator as BasePlayer;
            if (attacker == null || victim == null) return null;
            var plyAtk = attacker;
            if (InAnyDM(plyAtk.UserIDString) || InAnyDM(victim.UserIDString)) return false;
            return null;
        }

        private object CanThorns(BaseCombatEntity attacker, BaseCombatEntity victim, HitInfo hitInfo)
        {
            if (attacker == null || victim == null || hitInfo == null) return null;
            var player = attacker as BasePlayer;
            if (player != null && InAnyDM(player.UserIDString)) return false;
            return null;
        }

        private object CanFriendlyFire(BasePlayer attacker, BasePlayer victim)
        {
            if (InAnyDM(attacker.UserIDString) || InAnyDM(victim.UserIDString)) return false;
            return null;
        }

        private object CanUseKit(BasePlayer player, string kit)
        {
            if (player == null) return null;
            if (InAnyDM(player.UserIDString))
            {
                SendReply(player, "You cannot use kits in <color=#ff3052>Deathmatch</color>!");
                return false;
            }
            return null;
        }

        private object CanUseLockedEntity(BasePlayer player, BaseLock baseLock)
        {
            if (player != null && InAnyDM(player.UserIDString) && !player.IsAdmin) return false;
            return null;
        }

        private object CanRemoveCommand(BasePlayer player) { return (player == null) ? null : InAnyDM(player.UserIDString) ? "You cannot use remover tool while in <color=#fcb355>Deathmatch</color>!" : null; }

        private void OnDoorOpened(Door door, BasePlayer player)
        {
            if (door == null || player == null) return;
            if (door.OwnerID != 0 && InAnyDM(player.UserIDString)) NextTick(() => { if (door != null && !door.IsDestroyed) door.CloseRequest(); });
        }

        private object CanKitCommand(BasePlayer player)
        {
            if (player == null) return null;
            if (InAnyDM(player.UserIDString))
            {
                SendReply(player, "You cannot use kits in <color=#fcb355>Deathmatch</color>!");
                return false;
            }
            return null;
        }

        private BasePlayer FindPlayerByContainer(ItemContainer container, bool checkSleepers = false)
        {
            if (container == null) return null;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (p == null || p.IsDead()) continue;
                if (p.inventory.containerMain == container || p.inventory.containerWear == container || p.inventory.containerBelt == container) return p;
            }
            if (checkSleepers)
            {
                for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                {
                    var p = BasePlayer.sleepingPlayerList[i];
                    if (p == null || p.IsDead()) continue;
                    if (p.inventory.containerMain == container || p.inventory.containerWear == container || p.inventory.containerBelt == container) return p;
                }
            }
            return null;
        }

        private object OnItemPickup(Item item, BasePlayer player)
        {
            if (!InAnyDM(player.UserIDString))
            {
                if (InAnyDMZone(player.UserIDString))
                {
                    PrintWarning("Player: " + player?.displayName + " (" + player?.UserIDString + ") tried to pick up an item while not in DM, but in a DM zone! Item: " + item?.info?.shortname + " x" + (item?.amount ?? 0).ToString("N0"));
                    return true;
                }
                var dmItem = false;
                if (item?.text?.Contains("DM Item", System.Globalization.CompareOptions.IgnoreCase) ?? false) dmItem = true;
                if (!dmItem)
                {
                    var itemEnt = item?.GetWorldEntity() ?? item?.GetHeldEntity() ?? null;
                    if (itemEnt != null && !itemEnt.IsDestroyed && itemEnt.gameObject != null)
                    {
                        for (int i = 0; i < Arenas.Count; i++)
                        {
                            var arena = Arenas[i];
                            if (arena == null || arena.Disabled || string.IsNullOrEmpty(arena.ZoneID)) continue;
                            if (isEntityInZone(itemEnt, arena.ZoneID))
                            {
                                dmItem = true;
                                break;
                            }
                        }
                    }

                }
                if (dmItem)
                {
                    PrintWarning("Player: " + player?.displayName + " (" + player?.UserIDString + ") tried to pick up a DM item while not in DM! Item: " + item?.info?.shortname + " x" + (item?.amount ?? 0).ToString("N0"));
                    return true;
                }
            }
            return null;
        }

        private void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null || !string.IsNullOrEmpty(item?.text)) return;
            var ply = FindPlayerByContainer(container);
            if (ply != null && InAnyDM(ply.UserIDString)) item.text = "DM Item";
        }

        private void OnItemDropped(Item item, BaseEntity entity)
        {
            if (item == null || entity == null) return;
            if (ignoreDroppedActives.Contains(item.uid)) return;
            var entPos = entity?.transform?.position ?? Vector3.zero;
            entity.Invoke(() =>
            {
                if (item == null)
                {
                    PrintWarning("item null on timer");
                    return;
                }

                if (ignoreDroppedActives.Contains(item.uid)) return;

                var newPos = (entity == null || entity?.transform == null) ? entPos : entity?.transform?.position ?? entPos;

                var anyNear = false;
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p != null && !p.IsDestroyed && p.gameObject != null && InAnyDM(p.UserIDString) && !(GetPlayerDM(p)?.AllowDropping ?? false) && (Vector3.Distance(p.transform.position, newPos) < 6 || Vector3.Distance(p.transform.position, entPos) < 6))
                    {
                        anyNear = true;
                        break;
                    }
                }
                //var anyNear = BasePlayer.activePlayerList?.Any(p => p != null && p?.transform != null && InAnyDM(p.UserIDString) && !(GetPlayerDM(p)?.AllowDropping ?? false) && (Vector3.Distance(p.transform.position, newPos) < 6 || Vector3.Distance(p.transform.position, entPos) < 6)) ?? false;
                var entityArena = (entity == null) ? null : (GetEntityArena(entity) ?? null);
                if (!(entityArena?.AllowDropping ?? true))
                {
                    //PrintWarning("ent arena not null && disallows dropping! removing: " + item?.info?.shortname + " arena: " + (entityArena?.ArenaName ?? "unknown name") + ", anynear: " + anyNear);

                    RemoveFromWorld(item);
                    if (!(entity?.IsDestroyed ?? true)) entity.Kill();
                }
            }, 0.35f);
        }

        private object CanBuild(Planner plan, Construction prefab)
        {
            var player = plan?.GetOwnerPlayer() ?? null;
            if (player == null || player?.inventory == null) return null;
            if (player.IsAdmin) return null;
            var getDM = GetPlayerDM(player);
            if (getDM != null && !getDM.PlayersCanBuild)
            {
                SendReply(player, "You cannot build while in the <color=#fcb355>" + getDM.ArenaName + "</color> arena.");
                return false;
            }
            return null;
        }

        private readonly HashSet<ItemId> ignoreDroppedActives = new HashSet<ItemId>();
        private object CanDropActiveItem(BasePlayer player)
        {
            if (player == null) return null;
            var getDM = GetPlayerDM(player);
            if (getDM == null) return null;
            if (!getDM.AllowDroppingActiveItem)
            {
                return false;
            }
            else
            {
                if (!getDM.AllowDropping)
                {
                    var item = player?.GetActiveItem() ?? null;
                    if (item != null)
                    {
                        ignoreDroppedActives.Add(item.uid);
                    }
                    else PrintWarning("active item was null on candropactiveitem!!");
                }
            }

            return null;
        }

        private void OnItemDeployed(Deployer deployer, BaseEntity entity)
        {
            if (deployer == null || entity == null) return;
            var player = deployer?.ToPlayer() ?? deployer?.GetOwnerPlayer() ?? null;
            if (player == null)
            {
                PrintWarning("null player on OnItemDeployed!!");
                return;
            }
            if (player.IsAdmin) return;
            var getDM = GetPlayerDM(player);
            if (getDM != null && !getDM.PlayersCanBuild)
            {
                SendReply(player, "You cannot build while in the <color=#fcb355>" + getDM.ArenaName + "</color> arena.");
                entity.Kill();
            }
        }

        private void OnJoinDeathmatchArena(BasePlayer player, ArenaInfo arena)
        {
            if (player == null || arena == null) return;
            UpdateHooks(true);
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (!init || entity == null) return;
            if (entity is PlayerCorpse)
            {
                var corpse = entity as PlayerCorpse;
                if (corpse == null || corpse.playerSteamID == 0) return;
                var ply = BasePlayer.FindByID(corpse.playerSteamID) ?? BasePlayer.FindSleeping(corpse.playerSteamID) ?? null;
                if (ply == null) return;
                var getDM = GetPlayerDM(ply);
                var corpsePos = corpse?.transform?.position ?? Vector3.zero;
                if (getDM == null || !getDM.Fortnite) return;
                corpse.Invoke(() =>
                {
                    if (!corpse.IsDestroyed) corpse.Kill();
                    for (int i = 0; i < UnityEngine.Random.Range(3, 8); i++)
                    {
                        var newPos = (i <= 1) ? corpsePos : new Vector3(corpsePos.x, corpsePos.y - UnityEngine.Random.Range(0.12f, 0.25f), corpsePos.z);
                        Effect.server.Run("assets/prefabs/locks/keypad/effects/lock.code.shock.prefab", newPos, Vector3.zero);
                    }
                }, 0.5f);
            }
            var fireball = entity as FireBall;
            if (fireball != null)
            {
                fireball.Invoke(() =>
                {
                    if (fireball == null || (fireball?.IsDestroyed ?? true)) return;
                    var entDM = GetEntityArena(fireball);
                    if (entDM == null || (entDM?.FireballTime ?? -1f) == -1f) return;
                    fireball.SetGeneration(0);
                    fireball.generation = 0;
                    fireball.CancelInvoke(fireball.TryToSpread);
                    fireball.Invoke(fireball.Extinguish, UnityEngine.Random.Range(1f, Mathf.Clamp(entDM.FireballTime, 1f, 300)));
                }, 0.4f);
            }
        }

        private object CanAcceptItem(ItemContainer container, Item item)
        {
            if (container == null || item == null) return null;
            //var containerEnt = container?.entityOwner ?? container?.playerOwner?.GetComponent<BaseEntity>() ?? null;
            var ownerPlayer = container?.playerOwner ?? item?.GetOwnerPlayer() ?? null;
            if (ownerPlayer == null) return null;
            var getDM = GetPlayerDM(ownerPlayer);
            for (int i = 0; i < Arenas.Count; i++)
            {
                var arena = Arenas[i];
                if (arena == null || arena.SpawnedItems == null || arena.SpawnedItems.Count < 1) continue;
                foreach (var dropItem in arena.SpawnedItems)
                {
                    if (dropItem == null) continue;
                    if (dropItem == item && (getDM == null || arena != getDM)) return ItemContainer.CanAcceptResult.CannotAccept;
                }
            }
            //if (getDM == null || !getDM.Fortnite) return null;
            //var category = item?.info?.category ?? ItemCategory.All;
            /*/
            if (!(item?.info?.shortname ?? string.Empty).Contains(".mod"))
            {
                if ((category == ItemCategory.Weapon && container != ownerPlayer.inventory.containerBelt) || (category == ItemCategory.Ammunition && container != ownerPlayer.inventory.containerMain) || (category == ItemCategory.Attire && container != ownerPlayer.inventory.containerWear))
                {
                    var efx = new Effect("assets/prefabs/locks/keypad/effects/lock.code.denied.prefab", ownerPlayer?.eyes?.position ?? Vector3.zero, Vector3.zero);
                    for (int i = 0; i < 5; i++) EffectNetwork.Send(efx, ownerPlayer?.net?.connection);
                    NextTick(() =>
                    {
                        if (item == null) return;
                        var drop = item?.GetWorldEntity()?.GetComponent<DroppedItem>() ?? null;
                        timer.Once(0.25f, () =>
                        {
                            if (drop != null && item != null) MakeItemFloat(item, drop?.transform?.position ?? Vector3.zero);
                        });

                    });
                    return ItemContainer.CanAcceptResult.CannotAccept;
                }
            }/*/


            return null;
            //return ItemContainer.CanAcceptResult.CannotAccept;
        }

        private object CanBlockOnHit(BasePlayer attacker, HitInfo info)
        {
            if (attacker == null || info == null) return null;
            if ((info?.HitEntity ?? null) == null || (info?.HitEntity?.IsDestroyed ?? true)) return null;
            try
            {
                if (InAnyDM(attacker.UserIDString)) return false;
                var victim = (info?.HitEntity != null) ? info?.HitEntity?.GetComponent<BasePlayer>() ?? null : null;
                if (victim != null && InAnyDM(victim.UserIDString)) return false;
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete CanBlockOnHit^"); }
            return null;
        }

        private object CanBlockOnDmg(BasePlayer attacker, HitInfo info)
        {
            if (attacker == null || info == null) return null;
            try
            {
                if (InAnyDM(attacker.UserIDString)) return false;
                var victim = (info?.HitEntity != null) ? info?.HitEntity?.GetComponent<BasePlayer>() ?? null : null;
                if (victim != null && InAnyDM(victim.UserIDString)) return false;
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete CanBlockOnDmg^"); }
            return null;
        }

        private object CanRaidBlock(string userID, Vector3 pos, bool createZone)
        {
            if (InAnyDM(userID)) return false;
            return null;
        }

        private object CanLuckDodge(BasePlayer victim, BaseEntity attacker, float scalar)
        {
            if (victim == null) return null;
            if (InAnyDM(victim.UserIDString)) return false;
            return null;
        }

        private object CanRejack(BaseCombatEntity attacker, BasePlayer player, HitInfo info)
        {
            if (attacker == null || player == null) return null;
            if (InAnyDM(player.UserIDString)) return false;
            return null;
        }

        /*/
        private object OnPlayerDeath(BasePlayer victim, HitInfo info)
        {
            if (victim == null || info == null) return null;
            var getDM = GetPlayerDM(victim);
            if (getDM != null && getDM.FakeDeaths)
            {
                NextTick(() =>
                {
                    if (victim != null && victim?.metabolism != null) victim.metabolism.bleeding.value = 0f;
                });
                return false;
            }
            return null;
        }/*/

        /*/
        public void SendAsSnapshot(BaseNetworkable entity, Connection connection, bool justCreated = false)
        {
            if (entity == null || entity.IsDestroyed || entity?.net == null || connection == null || !Net.sv.write.Start()) return;
            ++connection.validate.entityUpdates;
            BaseNetworkable.SaveInfo saveInfo = new BaseNetworkable.SaveInfo()
            {
                forConnection = connection,
                forDisk = false
            };
            Net.sv.write.PacketID(Message.Type.Entities);
            Net.sv.write.UInt32(connection.validate.entityUpdates);
            entity.ToStreamForNetwork(Net.sv.write, saveInfo);
            Net.sv.write.Send(new SendInfo(connection));
        }/*/

        public void SendNetworkUpdate(BaseNetworkable entity, BasePlayer.NetworkQueue queue = BasePlayer.NetworkQueue.Update, BasePlayer target = null)
        {
            if (entity == null || entity.IsDestroyed || entity.net == null || !entity.isSpawned) return;
            if (target != null && (target?.net?.connection == null || !target.IsConnected)) return;
            entity.LogEntry(BaseMonoBehaviour.LogEntryType.Network, 2, "SendNetworkUpdate");
            entity.InvalidateNetworkCache();
            if (target == null)
            {
                var subscribers = entity?.net?.group?.subscribers ?? null;
                if (subscribers != null && subscribers.Count > 0)
                {
                    for (int i = 0; i < subscribers.Count; i++)
                    {
                        var ply = subscribers[i]?.player as BasePlayer;
                        if (ply != null && entity.ShouldNetworkTo(ply)) ply.QueueUpdate(queue, entity);
                    }
                }
            }
            else target.QueueUpdate(queue, entity);

            if (entity?.gameObject != null) entity.gameObject.SendOnSendNetworkUpdate(entity as BaseEntity);
        }

        /// <summary>
        /// Forcefully send entity updates to specified connections. For performance reasons, this does NOT run the CanNetworkTo check. Handle this yourself or don't use this.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queue"></param>
        /// <param name="connections"></param>
        public void SendNetworkUpdate(BaseNetworkable entity, BasePlayer.NetworkQueue queue = BasePlayer.NetworkQueue.Update, List<Connection> connections = null)
        {
            if (entity == null || entity.IsDestroyed || entity.net == null || !entity.isSpawned) return;
            entity.LogEntry(BaseMonoBehaviour.LogEntryType.Network, 2, "SendNetworkUpdate");
            entity.InvalidateNetworkCache();
            var subscribers = (connections != null && connections.Count > 0) ? connections : entity?.net?.group?.subscribers ?? null;
            if (subscribers != null && subscribers.Count > 0)
            {
                for (int i = 0; i < subscribers.Count; i++)
                {
                    var ply = subscribers[i]?.player as BasePlayer;
                    if (ply != null && ply?.net?.connection != null) ply.QueueUpdate(queue, entity);
                }
            }
            if (entity?.gameObject != null) entity.gameObject.SendOnSendNetworkUpdate(entity as BaseEntity);
        }

        private void OnEnterStorm(BasePlayer player)
        {
            if (player == null) return;
            if (envSync != null) SendNetworkUpdate(envSync as BaseNetworkable, BasePlayer.NetworkQueue.Update, player);
        }

        private void OnExitStorm(BasePlayer player)
        {
            if (player == null) return;
            if (envSync != null) SendNetworkUpdate(envSync as BaseNetworkable, BasePlayer.NetworkQueue.Update, player);
        }

        private void OnTriggerEnterSphere(SphereTrigger trigger, Collider col)
        {
            try
            {
                if (trigger == null || col == null) return;
                if (trigger.Name != "ammo") return;
                // PrintWarning("trigger enter for ammo!");
                var player = col?.GetComponentInParent<BasePlayer>() ?? null;
                if (player == null) return;
                PrintWarning("player enter ammo trigger: " + player.displayName);
                if (player.IsDead() || player.inventory == null || player.IsSpectating() || player?.net == null)
                {
                    PrintWarning("player is dead or inv null on trigger enter");
                    return;
                }
                PrintWarning("ENTER player for AMMO");
                var dropped = trigger?.GetComponent<DroppedItem>() ?? trigger?.parentObject?.ToBaseEntity()?.GetComponentInParent<DroppedItem>() ?? null;
                if (dropped == null || dropped.item == null)
                {
                    PrintWarning("Ammo trigger has no dropped/dropped.item");
                    return;
                }
                var droppedId = dropped?.item?.info?.itemid ?? 0;
                var droppedAmt = dropped?.item?.amount ?? 0;
                if (!dropped.item.MoveToContainer(player.inventory.containerMain) && !dropped.item.MoveToContainer(player.inventory.containerBelt))
                {
                    PrintWarning("no move ammo on trigger!");
                    return;
                }
                else
                {
                    player.SendConsoleCommand("note.inv " + droppedId + " " + droppedAmt);
                    PlayCodeLockUpdate(player, UnityEngine.Random.Range(1, 4));
                }
            }
            catch (Exception ex) { PrintError(ex.ToString() + Environment.NewLine + "Exception for OnTriggerEnterSphere"); }

        }

        private void ClearCorpse(LootableCorpse corpse)
        {
            if (corpse == null || corpse.IsDestroyed || corpse?.containers == null || corpse.containers.Length < 1) return;
            for (int i = 0; i < corpse.containers.Length; i++)
            {
                var container = corpse.containers[i];
                if (container?.itemList != null && container.itemList.Count > 0)
                {
                    for (int j = 0; j < container.itemList.Count; j++)
                    {
                        var item = container.itemList[j];
                        if (item != null) RemoveFromWorld(item);
                    }
                }
            }
        }

        private static void ClearInventory(BasePlayer player)
        {
            if (player == null || player?.inventory == null) return;
            var items = Pool.GetList<Item>();
            try
            {
                player.inventory.AllItemsNoAlloc(ref items);

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item != null) RemoveFromWorld(item);
                }
            }
            finally { Pool.FreeList(ref items); }

        }

        public static void RespawnAt(BasePlayer player, Vector3 position, Quaternion rotation, bool loading = false, float loadingDist = 0.0f)
        {
            if (player == null || !player.IsDead()) return;
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Wounded, false);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Unused2, false);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Unused1, false);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.DisplaySash, false);
            ++ServerPerformance.spawns;
            player.SetParent(null, true, false);
            player.transform.position = position;
            player.transform.rotation = rotation;
            //  player.tickInterpolator.Reset(position);
            //    player.lastTickTime = 0.0f;
            player.StopWounded();
            player.StopSpectating();
            player.UpdateNetworkGroup();
            player.EnablePlayerCollider();
            //player.UpdatePlayerCollider(true);
            //rigid body
            player.StartSleeping();
            if (player.lifeStory != null) player.LifeStoryEnd();
            player.lifeStory = new ProtoBuf.PlayerLifeStory()
            {
                ShouldPool = false
            };
            player.lifeStory.timeBorn = (uint)Facepunch.Math.Epoch.Current;
            player.metabolism.Reset();
            player.InitializeHealth(player.StartHealth(), player.StartMaxHealth());
            player.inventory.GiveDefaultItems();
            player.SendNetworkUpdateImmediate();
            if (loading)
            {
                if (loadingDist > 0)
                {
                    var oldPos = player?.transform?.position ?? Vector3.zero;
                    if (Vector3.Distance(oldPos, position) < loadingDist) return;
                }
                player.ClientRPCPlayer(null, player, "StartLoading");
                if (player.net != null)
                    EACServer.OnStartLoading(player.net.connection);
            }
        }

        public Dictionary<BasePlayer, List<Item>> TransferDeathItems = new Dictionary<BasePlayer, List<Item>>();

        private void OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return;

            var target = info?.HitEntity as ReactiveTarget;
            if (target == null) return;

            if (InAnyDM(player.UserIDString))
            {
                target.Invoke(() =>
                {
                    if (target != null && target.IsKnockedDown())
                    {
                        target.SetFlag(BaseEntity.Flags.On, true, false, false);
                        target.SendNetworkUpdate();
                    }
                }, 0.02f);
              
                target.knockdownHealth = 100f;
            }
  
        }

        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (player == null || !init) return;
            var deathPos = player?.transform?.position ?? Vector3.zero;
            var weapon = info?.Weapon ?? info?.WeaponPrefab ?? null;
            /*/
            if (entity.ShortPrefabName.Contains("scientist"))
            {
                if (EntityHasZoneFlag(entity, "AutoScientists"))
                {
                    PrintWarning("respawn scientist");
                    var scientistZone = GetPlayerZone(player);
                    var scientistDM = arenas?.Where(p => p.ZoneID.Equals(scientistZone, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault() ?? null;
                    if (scientistDM == null) scientistDM = GetPlayerDM(info?.Initiator as BasePlayer);
                    var spawn = entity?.transform?.position ?? Vector3.zero;
                    if (scientistDM != null) spawn = scientistDM.GetSmartSpawn(player);
                    if (spawn == Vector3.zero)
                    {
                        PrintWarning("vec3 zero");
                        return;
                    }
                    var scientist = GameManager.server.CreateEntity("assets/prefabs/npc/scientist/scientist.prefab", spawn, entity.transform.rotation);
                    scientist.Spawn();
                }
                return;
            }/*/
            var getDM = GetPlayerDM(player);
            if (getDM != null && !getDM.Fortnite)
            {

                List<Item> deathItemsList = null;
                if (!TransferDeathItems.TryGetValue(player, out deathItemsList)) deathItemsList = new List<Item>();
                else deathItemsList.Clear();

                var allItems = Pool.GetList<Item>();
                try
                {
                    player?.inventory?.AllItemsNoAlloc(ref allItems);


                    if (allItems?.Count > 0)
                    {
                        for (int i = 0; i < allItems.Count; i++)
                        {
                            var item = allItems[i];
                            if (item != null)
                            {
                                item.RemoveFromContainer();
                                deathItemsList.Add(item);
                            }
                        }
                    }
                    else
                    {
                        PrintWarning("player had no items on death!");
                    }

                    TransferDeathItems[player] = deathItemsList;//new List<Item>(allItems);

                }
                finally { Pool.FreeList(ref allItems); }

                //    ClearInventory(player);
                /*/
                 var pos = player?.transform?.position ?? Vector3.zero;
                 var uid = player.userID;
                 NextTick(() =>
                 {
                     var corpses = new List<LootableCorpse>();
                     Vis.Entities(pos, 3f, corpses, 512);
                     if (corpses != null && corpses.Count > 0)
                     {
                         LootableCorpse useCorpse = null;
                         for(int i = 0; i < corpses.Count; i++)
                         {
                             var corpse = corpses[i];
                             if (corpse == null) continue;
                             if (corpse.playerSteamID == uid)
                             {
                                 useCorpse = corpse;
                                 break;
                             }
                         }
                         if (useCorpse != null)
                         {
                             var corpseItems = new List<Item>();
                             PrintWarning("Corpse containers length: " + useCorpse.containers.Length);
                             for (int i = 0; i < useCorpse.containers.Length; i++)
                             {
                                 var cont = useCorpse.containers[i];
                                 PrintWarning("corpse container itemlist count: " + cont.itemList.Count);
                                 for (int j = 0; j < cont.itemList.Count; j++)
                                 {
                                     var corpseItem = cont.itemList[j];
                                     if (corpseItem == null) continue;
                                     corpseItems.Add(corpseItem);
                                     corpseItem.RemoveFromContainer();
                                 }
                             }
                             if (corpseItems != null && corpseItems.Count > 0)
                             {
                                 PrintWarning("CORPSE ITEMS!");

                                 timer.Once(0.15f, () =>
                                 {
                                     ClearInventory(player);
                                     PrintWarning("CORPSE ITEMS TIMER!");
                                     for (int i = 0; i < corpseItems.Count; i++)
                                     {
                                         var item = corpseItems[i];
                                         if (item == null) continue;
                                         if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt) && !item.MoveToContainer(player.inventory.containerWear))
                                         {
                                             RemoveFromWorld(item);
                                             PrintWarning("Couldn't move corpse item: " + item?.info?.shortname);
                                         }
                                         else PrintWarning("DID move corpse item: " + item?.info?.shortname);
                                     }
                                     getDM.SortAndRefreshItems(player);
                                 });
                             }
                             else
                             {
                                 PrintWarning("COULD NOT GET CORPSE ITEMS AT ALL!");
                                 getDM.CreateItems(player, true);
                             }

                             if (!getDM.TransferCorpse(useCorpse, player))
                             {
                                 getDM.CreateItems(player, true);
                                 PrintWarning("Attempted to transfer corpse - couldn't! had to fall back on CreateItems");
                             }
                             else
                             {
                                 getDM.SortAndRefreshItems(player);
                                 PrintWarning("Transfer corpse returned true, seems ok. Ran SortAndRefresh items, now killing corpse.");
                                 if (!useCorpse.IsDestroyed) useCorpse.Kill();
                             }

                         }
                         else
                         {
                             PrintWarning("PLAYER DID NOT HAVE A CORPSE!");
                             getDM.CreateItems(player, true);
                         }

                     }
                     else getDM.CreateItems(player, true);
                 });/*/
                // var oldPos = deathPos;
                /*/
                NextTick(() =>
                {
                    var findCorpses = new List<PlayerCorpse>();
                    foreach (var ent in BaseEntity.serverEntities)
                    {
                        if (ent == null) continue;
                        var corpse = ent as PlayerCorpse;
                        if (corpse == null) continue;
                        if (corpse.playerSteamID == player.userID && Vector3.Distance(oldPos, corpse.transform.position) <= 2.35f) findCorpses.Add(corpse);

                    }
                    if (findCorpses.Count > 0)
                    {
                        for (int i = 0; i < findCorpses.Count; i++)
                        {
                            var corpse = findCorpses[i];
                            if (corpse != null && !corpse.IsDestroyed)
                            {
                                ClearCorpse(corpse);
                                corpse.Kill();
                            }
                        }
                    }
                });/*/
            }
            /*/
            CTFFlag flag;
            if (flagCarrier.TryGetValue(player.UserIDString, out flag))
            {
                PrintWarning("flag death");
                flagCarrier.Remove(player.UserIDString);
                var flagEnt = flag?.GetEntity() ?? null;
                var oldPos = flagEnt?.GetParentEntity()?.transform?.position ?? Vector3.zero;
                if (flagEnt != null)
                {
                    //  PrintWarning("Flag ent pos: " + flagEnt.transform.position);
                    flagEnt.SetParent(null);
                    flagEnt.transform.localEulerAngles = Vector3.zero;
                    flagEnt.transform.localPosition = Vector3.zero;
                    var newPos = GetGroundPosition(oldPos);
                    if (newPos == Vector3.zero)
                    {
                        PrintWarning("newpos vec 3 zero ctf");
                        return;
                    }

                    flagEnt.transform.position = newPos;
                    //    flagEnt.transform.rotation = Quaternion.Euler(271.0f, 247.5f, 0.0f);
                    flagEnt.transform.eulerAngles = new Vector3(271, 157.3f, 0.0f);
                    flagEnt.SendNetworkUpdate();
                    //       timer.Once(1f, () => PrintWarning("flag ent pos NOW: " + (flagEnt?.transform?.position ?? Vector3.zero)));
                }
                Interface.Oxide.CallHook("OnFlagDropped", flag);
            }/*/
            //  var getDM = GetPlayerDM(player);
            var attacker = info?.Initiator as BasePlayer;
            var atkDM = GetPlayerDM(attacker);
            if (getDM != null)
            {
                if (getDM.Fortnite && getDM.GameInProgress && !getDM.StartingGame && deathPos != Vector3.zero)
                {
                    var vicItems = player?.inventory?.AllItems()?.ToList() ?? null;
                    if (vicItems != null && vicItems.Count > 0)
                    {
                        for (int i = 0; i < vicItems.Count; i++)
                        {
                            var item = vicItems[i];
                            if (item == null) continue;
                            getDM.RemoveOnEnd.Add(item);
                            var category = item?.info?.category ?? ItemCategory.All;
                            if (category == ItemCategory.Attire)
                            {
                                RemoveFromWorld(item);
                                continue;
                            }
                            var shortName = item?.info?.shortname ?? string.Empty;
                            var spreadPos = SpreadVector(deathPos, UnityEngine.Random.Range(0.45f, 1f));
                            spreadPos.y = deathPos.y + 0.25f;
                            var floatItem = MakeItemFloat(item, spreadPos);
                            if (floatItem == null || floatItem?.gameObject == null)
                            {
                                PrintWarning("Couldn't float: " + shortName);
                                continue;
                            }
                            if (category != ItemCategory.Ammunition) continue;
                            timer.Once(0.8f, () =>
                            {
                                if (floatItem == null || floatItem.gameObject == null)
                                {
                                    PrintWarning("floatItem or floatItem.gameObject == null after 0.7f");
                                    return;
                                }
                                var sphereCollider = floatItem.gameObject.AddComponent<SphereTrigger>();
                                sphereCollider.Radius = 0.925f;
                                sphereCollider.Name = "ammo";
                                //    PrintWarning("Added sphereCollider");
                            });
                        }
                    }
                }
                if (atkDM != null)
                {
                    var vicTeam = getDM?.GetTeam(player) ?? ArenaInfo.Team.None;
                    var atkTeam = atkDM?.GetTeam(attacker) ?? ArenaInfo.Team.None;
                    atkDM.AddPlayerKills(attacker.UserIDString, 1);
                    atkDM.AddPlayerDeaths(player.UserIDString, 1);
                    if (atkDM == getDM && vicTeam != ArenaInfo.Team.None && atkTeam != ArenaInfo.Team.None && atkTeam != vicTeam && atkDM.GameInProgress && !atkDM.StartingGame)
                    {
                        atkDM.AddTeamKills(atkTeam, 1);

                        var atkKills = atkDM.GetTeamKills(atkTeam);
                        if (atkKills >= atkDM.MaxScore && atkKills > 0 && atkDM.MaxScore > 0)
                        {
                            PrintWarning("atk kills >= max: " + atkKills + " >= " + atkDM.MaxScore);
                            var winMsg = "<size=20><color=#62edfc>" + atkTeam + " win!</color></size>";
                            SendMessageToDM(atkDM, winMsg);
                            ShowPopupDM(atkDM, winMsg);


                            PrintWarning("END GAME");
                            atkDM.GameInProgress = false;
                            ServerMgr.Instance.Invoke(() =>
                            {
                                if (atkDM == null) return;
                                PrintWarning("end game invoke!!!");
                                atkDM.EndGame();
                                atkDM.StartGame(atkDM.NewGameTime);
                            }, 5f);

                        }

                        var teamPlayers = atkDM?.ActivePlayers?.Where(p => atkDM.GetTeam(p) == atkTeam)?.ToList() ?? null;
                        if (teamPlayers != null && teamPlayers.Count > 0) for (int i = 0; i < teamPlayers.Count; i++) ScoreGUI(teamPlayers[i], atkDM, atkDM.GetTeamKills(atkTeam), 5f);
                    }
                    //          else PrintWarning("atk dm == getdM? " + (atkDM == getDM) + ", victeam: " + vicTeam + ", atk: " + atkTeam + ", atkdm.gameinprogress: " + atkDM.GameInProgress);
                }
                lastDeath[player.UserIDString] = player?.transform?.position ?? Vector3.zero;
                lastDeathTime[player.UserIDString] = Time.realtimeSinceStartup;
                NextTick(() =>
                {
                    if (player == null || !player.IsConnected) return;
                    if (getDM == null)
                    {
                        PrintWarning("getDM is null after nexttick!");
                        return;
                    }
                    if (getDM.DoSpawns(player))
                    {
                        var doCreate = false;
                        List<Item> transferItems;
                        if (TransferDeathItems.TryGetValue(player, out transferItems) && transferItems?.Count > 0)
                        {
                            ClearInventory(player);
                            for (int i = 0; i < transferItems.Count; i++)
                            {
                                var item = transferItems[i];
                                if (item == null) continue;

                                if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt) && !item.MoveToContainer(player.inventory.containerWear))
                                {
                                    PrintWarning("Failed to move item at all!");
                                    RemoveFromWorld(item);
                                }

                            }
                            if (!getDM.SortAndRefreshItems(player))
                            {
                                PrintWarning("SortAndRefreshItems returned false with " + transferItems?.Count + " transfer items!");
                                doCreate = true;
                            }
                            //     PrintWarning("Called SortAndRefreshItems with " + transferItems?.Count + " transfer items");
                        }
                        else
                        {
                            PrintWarning("Didn't get transfer items for player on getdm dospawns (after death & spawn)");
                            doCreate = true;
                        }

                        if (doCreate)
                        {
                            if (!getDM.CreateItems(player, true)) PrintWarning("Failed to create items after spawn!!!");
                        }


                        if (getDM.RespawnTime > 0.0)
                        {
                            if (getDM.SpectateTeamDead && getDM.GetTeamCount(getDM.GetTeam(player)) > 1)
                            {
                                NextTick(() =>
                                {
                                    if (getDM == null || player == null) return;
                                    var rngTeam = getDM.GetTeamPlayers(getDM.GetTeam(player))?.Where(p => p != player && !p.IsDead() && !p.IsSpectating())?.ToList()?.GetRandom() ?? null;
                                    if (rngTeam != null)
                                    {
                                        SpectatePlayer(player, rngTeam, false);
                                        PrintWarning(player.displayName + " spectate: " + rngTeam.displayName);
                                        player.Invoke(() =>
                                        {
                                            PrintWarning("Pre stop spec pos: " + player.transform.position);
                                            StopSpectatePlayer(player);
                                            PrintWarning("pos directly after stop spec: " + player.transform.position);
                                            PrintWarning("Do spawns?: " + getDM.DoSpawns(player));
                                            PrintWarning("now pos: " + player.transform.position);
                                            WakePlayerUp(player, true);
                                            PrintWarning("unspectate & wakeup, now pos: " + player.transform.position);
                                        }, getDM.RespawnTime);
                                    }
                                });
                            }
                            else
                            {
                                RenderUIBlind(player, "Respawning in... " + getDM.RespawnTime.ToString("N0"));
                                if (getDM.RespawnTime > 1)
                                {
                                    var spawnTime = 0f;
                                    var newTime = getDM.RespawnTime;
                                    Action newAction = null;
                                    newAction = new Action(() =>
                                    {
                                        spawnTime += 1f;
                                        newTime -= 1f;
                                        if (spawnTime >= getDM.RespawnTime) InvokeHandler.CancelInvoke(player, newAction);
                                        else RenderUIBlindText(player, "Respawning in... " + newTime.ToString("N0"));
                                    });
                                    player.InvokeRepeating(newAction, 1f, 1f);
                                }
                                player.Invoke(() =>
                                {
                                    getDM = GetPlayerDM(player);
                                    if (getDM != null && getDM.DoSpawns(player) && getDM.CreateItems(player, true)) WakePlayerUp(player, true);
                                    CuiHelper.DestroyUi(player, "BlindUI");
                                    CuiHelper.DestroyUi(player, "BlindUIText");
                                }, getDM.RespawnTime);
                            }
                        }
                        else
                        {
                            WakePlayerUp(player, true, 0.05f);
                            if (getDM.Fortnite)
                            {
                                var specPlayer = (attacker != player && attacker != null && attacker.IsAlive()) ? attacker : getDM?.ActivePlayers?.Where(p => p != null && p != attacker && p != player && p.IsAlive() && !p.IsSpectating())?.ToList()?.GetRandom() ?? null;
                                if (specPlayer != null && specPlayer.IsAlive())
                                {
                                    SpectatePlayer(player, specPlayer, false);
                                    PrintWarning("Tried to do spectate for: " + player.displayName + ", initial target: " + specPlayer.displayName);
                                }
                                else PrintWarning("no other player to spectate for: " + player.displayName);
                            }
                        }
                    }
                    else
                    {
                        player.Respawn();
                        PrintWarning("Failed to DoSpawns for " + player.displayName + "!!!!");
                    }
                });
                /*/
                if (getDM.RemoveItemsOnDeath)
                {
                    var radOut = new List<Item>();
                    if (getDM.removeAfterDeath.TryGetValue(player.UserIDString, out radOut) && radOut != null && radOut.Count > 0)
                    {
                        for (int i = 0; i < radOut.Count; i++)
                        {
                            var item = radOut[i];
                            if (item != null) RemoveFromWorld(item);
                        }
                        getDM.removeAfterDeath[player.UserIDString].Clear();
                    }
                }/*/
            }


            if (atkDM != null && atkDM.OIC)
            {
                //  var atkWep = info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem() ?? null;
                var atkWep = info?.Weapon != null ? info?.Weapon?.GetItem() : info?.WeaponPrefab != null ? info?.Weapon?.GetItem() : null;
                if (atkWep != null)
                {
                    var atkProj = atkWep?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                    if (atkProj != null && atkProj?.primaryMagazine != null)
                    {
                        atkProj.primaryMagazine.contents++;
                        atkProj.SendNetworkUpdate();
                    }
                    else
                    {
                        var findAmmos = (!atkDM.Teams) ? atkDM?.DMItems?.Where(p => !string.IsNullOrEmpty(p?.ammoName))?.Select(p => p?.ammoName)?.ToList() ?? null : atkDM?.GetTeamItemInfos(atkDM.GetTeam(attacker))?.Where(p => !string.IsNullOrEmpty(p?.ammoName))?.Select(p => p?.ammoName)?.ToList() ?? null;
                        if (findAmmos != null && findAmmos.Count > 0)
                        {
                            for (int i = 0; i < findAmmos.Count; i++)
                            {
                                var ammo = ItemManager.CreateByName(findAmmos[i]);
                                if (ammo == null) continue;
                                if (!ammo.MoveToContainer(attacker.inventory.containerBelt) && !ammo.MoveToContainer(attacker.inventory.containerMain)) RemoveFromWorld(ammo);
                            }
                        }
                    }
                    SendReply(attacker, "<color=#62edfc>+1 bullet!</color>");
                }
            }

            killsWithoutDying[player.userID] = 0;



            if (attacker == null || atkDM == null) return;
            if (weapon != null && weapon.ShortPrefabName.Contains("rocket")) return;
            var attackerKWD = 0;

            if (!killsWithoutDying.TryGetValue(attacker.userID, out attackerKWD)) attackerKWD = killsWithoutDying[attacker.userID] = 1;
            else killsWithoutDying[attacker.userID]++;
            var killstreak = killsWithoutDying[attacker.userID];
            var killstreakStr = killstreak.ToString();
            if (killstreakStr.EndsWith("5") || killstreakStr.EndsWith("0")) SendMessageToDMPlayers("<color=#a6fc60>" + attacker.displayName + " is on a " + killstreakStr + " killstreak!</color>", atkDM.ZoneID);
            if (killstreak == atkDM.AirstrikeKS && atkDM.Airstrikes)
            {
                strikeAmounts[attacker.userID] = 1;
                SendReply(attacker, "<color=#a6fc60> 5 Killstreak!</color> You have earned an <color=#fcb355>Airstrike!</color> Use your <color=#fcb355>binoculars</color> to call it in!");
                for (int i = 0; i < 3; i++)
                {
                    SendLocalEffect(attacker, "assets/bundled/prefabs/fx/weapons/rifle_jingle1.prefab");
                }
            }
            if (killstreak == 15 && HasNuke.Add(attacker.userID))
            {
                SendReply(attacker, "<color=red>15 killstreak!</color> <color=#fcb355>You've earned a <color=yellow>nuclear</color> <color=#a6fc60>bomb</color>.</color> <color=#a6fc60>Type <color=#fcb355>/dmn</color> to use it!</color>");
            }
        }

        private object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || (entity?.IsDestroyed ?? true) || !init) return null;
            var errorSB = new StringBuilder();
            try
            {
                var ownerID = info?.Initiator?.OwnerID ?? info?.Weapon?.OwnerID ?? info?.WeaponPrefab?.OwnerID ?? 0;
                var player = (info?.Initiator as BasePlayer) ?? info?.Weapon?.GetItem()?.GetOwnerPlayer() ?? info?.WeaponPrefab?.GetItem()?.GetOwnerPlayer() ?? BasePlayer.FindByID(info?.Weapon?.OwnerID ?? info?.WeaponPrefab?.OwnerID ?? 0);
                var victim = entity as BasePlayer;
                var isHeli = entity is PatrolHelicopter;
                // if (isHeli) PrintWarning("hit heli");
                //   var isHeli = (entity is PatrolHelicopter) || entity.ShortPrefabName.Contains("patrolheli");
                var weaponItem = (info?.Weapon != null) ? info?.Weapon?.GetItem() ?? null : (info?.WeaponPrefab != null) ? info?.WeaponPrefab?.GetItem() ?? null : null;
                var weapon = (weaponItem != null) ? weaponItem?.GetHeldEntity() ?? null : null;
                //  var weapon = info?.Weapon?.GetItem() ?? info?.WeaponPrefab?.GetItem() ?? null;

                //   var isHeli = (entity?.ShortPrefabName ?? string.Empty).Contains("heli");
                var dmgType = info?.damageTypes?.GetMajorityDamageType() ?? Rust.DamageType.Generic;
                var isVicDM = victim == null ? false : InAnyDM(victim.UserIDString);
                if (victim != null && InAnyDM(victim.UserIDString) && (dmgType == Rust.DamageType.Radiation || dmgType == Rust.DamageType.RadiationExposure))
                {
                    CancelDamage(info);
                    return true;
                }
                if (isVicDM && info?.Initiator is Barricade && info?.Initiator.ShortPrefabName == "spikes_static") info.damageTypes.ScaleAll(1000f);

                // if (victim != null && victim.IsAdmin && isDMVic) PrintWarning("attacker: " + (info?.Initiator?.ShortPrefabName ?? string.Empty));
                if (victim != null && InAnyDM(victim.UserIDString) && info?.Initiator != null && (info?.Initiator?.ShortPrefabName ?? string.Empty).Contains("external")) info?.damageTypes?.ScaleAll(4f);

                errorSB.AppendLine("Past vars");
                var atkDM = GetPlayerDM(player);
                if (atkDM != null && atkDM.Fortnite && (!atkDM.GameInProgress || atkDM.StartingGame))
                {
                    CancelDamage(info);
                    return true;
                }

                if (atkDM != null && weaponItem != null)
                {
                    var dmgScalar = atkDM.GetDmgScalar(weaponItem.info);
                    // PrintWarning("dmg scalar: " + dmgScalar + ", item: " + weaponItem.info.shortname);
                    if (dmgScalar != -1f) info?.damageTypes?.ScaleAll(dmgScalar);
                }
                if (isHeli && player != null && atkDM != null)
                {
                    //   PrintWarning("heli hit");
                    SendReply(player, "You cannot damage a Helicopter while in the zone!");
                    CancelDamage(info);
                    var hitTimes = 0;
                    if (!heliHitTimes.TryGetValue(player.UserIDString, out hitTimes)) heliHitTimes[player.UserIDString] = 1;
                    else hitTimes = heliHitTimes[player.UserIDString] += 1;
                    if (hitTimes > 3)
                    {
                        player.Kick("Do not attack the Helicopter while in Deathmatch.");
                        //       PrintWarning("hit times > 3, kick!");
                    }
                    else player.Hurt(999f, Rust.DamageType.ElectricShock, player, false);
                    return true;
                }
                if (entity?.net != null && immuneEntities.Contains(entity.net.ID))
                {
                    CancelDamage(info);
                    return true;
                }

                if (victim != null && !(victim?.IsSleeping() ?? false) && !(victim?.IsConnected ?? false)) return null;
                var vicDM = GetPlayerDM(victim);
                if (vicDM != null && vicDM.Fortnite && (!vicDM.GameInProgress || vicDM.StartingGame))
                {
                    CancelDamage(info);
                    return true;
                }
                var isDMVic = vicDM != null;
                var isDMAtk = atkDM != null;
                if (player != null && (info?.WeaponPrefab ?? info?.Weapon ?? null) != null) lastAttackTime[player.userID] = Time.realtimeSinceStartup;
                var atkTeam = atkDM?.GetTeam(player) ?? ArenaInfo.Team.None;
                var vicTeam = vicDM?.GetTeam(victim) ?? ArenaInfo.Team.None;
                if (atkDM != null && vicDM != null && atkDM == vicDM && atkDM.Teams && atkTeam == vicTeam && player != victim)
                {
                    //SendReply(player, "<color=#91ff52>You cannot hurt your teammates!</color>");
                    //PrintWarning("team hurt cancel, same team: " + atkTeam + ", " + vicTeam);
                    CancelDamage(info);
                    return true;
                }


                var bleedScalar = atkDM?.BleedingScalar ?? -1f;

                if (victim != null && isDMVic && bleedScalar != -1f) info?.damageTypes?.Scale(Rust.DamageType.Bleeding, bleedScalar);

                if (victim != null && player != null && isDMVic && !isDMAtk)
                {
                    var pTimeAlive = GetTimeAlive(player);
                    if (pTimeAlive > 1 || pTimeAlive < 0)
                    {
                        if ((info?.damageTypes?.Total() ?? 0f) > 0 && (player?.IsConnected ?? false) && !(player?.IsSleeping() ?? false) && dmgType != Rust.DamageType.Bleeding) SendReply(player, "You cannot attack players who are in DM while you are not!");
                        CancelDamage(info);
                        return true;
                    }
                }
                errorSB.AppendLine("Past dm vic/player first check");
                if (victim != null && isDMVic)
                {
                    var sptTime = vicDM?.SpawnProtectionTime ?? -1f;
                    var timeAlive = GetTimeAlive(victim);
                    var timeSinceFired = TimeSinceFiredWeapon(victim);
                    var lastAttackTime = TimeSinceAttacked(victim);
                    if (victim != player && sptTime != -1f && timeAlive > 0 && (TimeSinceFiredWeapon(victim) >= 1 || TimeSinceAttacked(victim) >= 1 || (lastAttackTime == -1f && timeSinceFired == -1f)))
                    {
                        if (timeAlive < sptTime || (victim?.IsReceivingSnapshot ?? true) || (victim?.IsSleeping() ?? false))
                        {
                            CancelDamage(info);
                            if (player != null && player.IsConnected) SendReply(player, "Spawn protection!");
                            return true;
                        }
                    }
                    NextTick(() =>
                    {
                        if (victim == null || victim.IsDead() || victim?.metabolism == null) return;
                        var dmgAmount = info?.damageTypes?.Total() ?? 0f;
                        if (player != null && dmgType != Rust.DamageType.Bleeding && dmgType != Rust.DamageType.Heat)
                        {
                            var bleedingAdd = dmgAmount / 4f;
                            if (bleedingAdd > victim.metabolism.bleeding.max) victim.metabolism.bleeding.max = bleedingAdd;
                            else victim.metabolism.bleeding.max = (victim?.metabolism?.bleeding?.value ?? 1f) + 1f;
                            victim.metabolism.bleeding.Add(bleedingAdd);
                        }
                    });


                }
                if (player != null && atkDM != null && victim == null)
                {
                    if (!isEntityInZone(entity, atkDM?.ZoneID ?? vicDM?.ZoneID ?? string.Empty))
                    {
                        CancelDamage(info);
                        return true;
                    }
                }

                errorSB.AppendLine("past ent checks");
                if (player == null && victim == null) return null;

                if (player == null && victim != null && vicDM != null) victim.lastAttackedTime = Time.realtimeSinceStartup - 55;
                errorSB.AppendLine("past moderate checks");
                if (player == null) return null;
                errorSB.AppendLine("Past advanced checks");
                if (victim != null && (vicDM != atkDM) && !string.IsNullOrEmpty(player.UserIDString) && !string.IsNullOrEmpty(player?.displayName))
                {
                    SendReply(player, "You cannot hurt this player!");
                    CancelDamage(info);
                    return true;
                }
                errorSB.AppendLine("Past all");
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                PrintError(ex.ToString());
                PrintWarning(errorSB.ToString().TrimEnd());
            }
            return null;
        }

        protected override void LoadDefaultConfig() => SaveConfig();

        private void Init()
        {
            DMMain = this;

            dmData = Interface.Oxide.DataFileSystem.ReadObject<DMData>("DMData");
            arenaData = Interface.Oxide.DataFileSystem.ReadObject<ArenaData>("DMArenas");


            if (dmData == null) dmData = new DMData();
            if (arenaData == null) arenaData = new ArenaData();
            if (Arenas != null && Arenas.Count > 0)
            {
                var expired = 0;
                var now = DateTime.UtcNow;
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena?.Bans == null || arena.Bans.Count < 1) continue;
                    var bans = arena.Bans.ToList();
                    for (int j = 0; j < bans.Count; j++)
                    {
                        var ban = bans[j];
                        if (ban == null) continue;
                        if (!ban.Permanent && now >= ban.EndTime)
                        {
                            arena.Bans.Remove(ban);
                            expired++;
                        }
                    }
                }
                if (expired > 0) PrintWarning("Removed " + expired.ToString("N0") + " expired DM arena bans");
            }
        }
        public bool CheckingZM { get; set; } = false;

        private IEnumerator CheckZM()
        {
            if (CheckingZM) yield return null;
            if (ZoneManager == null || !(ZoneManager?.IsLoaded ?? false))
            {
                var arenaMax = 4;
                var arenaCount = 0;
                var apMax = 10;
                var apCount = 0;
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena == null || arena.ActivePlayers == null || arena.ActivePlayers.Count < 1) continue;
                    arenaCount++;
                    if (arenaCount >= arenaMax)
                    {
                        arenaCount = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    var ap = Pool.GetList<BasePlayer>();
                    try
                    {
                        foreach (var p in arena.ActivePlayers) ap.Add(p);

                        for (int j = 0; j < ap.Count; j++)
                        {
                            var ply = ap[j];
                            if (ply == null) continue;
                            apCount++;
                            if (apCount >= apMax)
                            {
                                apCount = 0;
                                yield return CoroutineEx.waitForEndOfFrame;
                            }
                            if (!arena.LeaveDM(ply)) PrintWarning("Couldn't leave DM (CheckZM) for: " + ply.displayName + ", arena: " + arena.ArenaName);
                        }
                    }
                    finally { Pool.FreeList(ref ap); }

                }
                PrintWarning("Kicking all players from DM because no zone manager!");
            }
        }
        public bool CheckingItems { get; set; } = false;

        private IEnumerator CheckItems()
        {
            if (CheckingItems || Arenas == null || Arenas.Count < 1) yield return null;
            var arenaMax = 5;
            var arenaCount = 0;
            var plyMax = 10;
            var plyCount = 0;
            var itemMax = 20;
            var itemCount = 0;
            for (int i = 0; i < Arenas.Count; i++)
            {
                var arena = Arenas[i];
                if (arena == null) continue;
                arenaCount++;
                if (arenaCount >= arenaMax)
                {
                    arenaCount = 0;
                    yield return CoroutineEx.waitForEndOfFrame;
                }
                var dmPlayers = Pool.GetList<BasePlayer>();
                try
                {
                    foreach (var p in arena.ActivePlayers) dmPlayers.Add(p);

                    for (int j = 0; j < dmPlayers.Count; j++)
                    {
                        var player = dmPlayers[j];
                        if (player == null || !(player?.IsConnected ?? true) || (player?.IsDead() ?? true)) continue;
                        plyCount++;
                        if (plyCount >= plyMax)
                        {
                            plyCount = 0;
                            yield return CoroutineEx.waitForEndOfFrame;
                        }

                        if (arena?.ActivePlayers != null && !arena.ActivePlayers.Contains(player))
                        {
                            PrintWarning("Arena " + (arena?.ArenaName ?? "Unknown") + "'s active players no longer contains a player that was in the for loop. Continuing!");
                            continue;
                        }

                        List<ModInfo> modList;
                        if (!arena.playerModInfos.TryGetValue(player.UserIDString, out modList)) modList = new List<ModInfo>();
                        else modList.Clear();

                        var allItems = Pool.GetList<Item>();
                        try
                        {
                            player?.inventory?.AllItemsNoAlloc(ref allItems);

                            if (allItems?.Count > 0)
                            {
                                for (int k = 0; k < allItems.Count; k++)
                                {
                                    var item = allItems[k];
                                    if (item == null) continue;

                                    itemCount++;
                                    if (itemCount >= itemMax)
                                    {
                                        itemCount = 0;
                                        yield return CoroutineEx.waitForEndOfFrame;
                                    }

                                    var getAmmo = item?.GetHeldEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine?.ammoType ?? null;
                                    if (getAmmo != null) arena.SetAmmo(player.UserIDString, item.info.shortname, getAmmo.shortname);
                                    var contents = item?.contents?.itemList ?? null;

                                    if (item.skin != 0) arena.SetSkin(player.UserIDString, item.info.shortname, item?.skin ?? 0);
                                    else arena.ResetSkin(player.UserIDString, item.info.shortname);

                                    if (!string.IsNullOrEmpty(item.name)) arena.SetSavedItemName(player.UserIDString, item.info.shortname, item.name);
                                    else arena.ResetSavedItemName(player.UserIDString, item.info.shortname);

                                    if (contents != null && contents.Count > 0)
                                    {
                                        for (int kk = 0; kk < contents.Count; kk++)
                                        {
                                            var mod = contents[kk];
                                            if (mod == null) continue;
                                            var newMod = new ModInfo(mod)
                                            {
                                                parentShortName = item.info.shortname
                                            };
                                            modList.Add(newMod);
                                        }
                                    }
                                }
                                if (modList?.Count > 0) arena.playerModInfos[player.UserIDString] = modList;
                                else arena.playerModInfos.Remove(player.UserIDString);
                            }
                        }
                        finally { Pool.FreeList(ref allItems); }


                        var beltItems = player?.inventory?.containerBelt?.itemList ?? null;
                        if (beltItems == null || beltItems.Count < 1) continue;


                        List<ItemPosition> dmPosList;
                        if (!arena.dmItemPositions.TryGetValue(player.UserIDString, out dmPosList)) dmPosList = new List<ItemPosition>();
                        else dmPosList.Clear();

                        var bCount = beltItems.Count;

                        for (int k = 0; k < bCount; k++)
                        {
                            if (((beltItems?.Count ?? -1) - 1) < k)
                            {
                                PrintWarning("beltItems count - 1 is < k, breaking loop!");
                                break;
                            }

                            var item = beltItems[k];
                            if (item?.info == null) continue;

                            var findPos = new ItemPosition(item.info.itemid, "belt", item.position);
                            dmPosList.Add(findPos);
                        }

                        if (dmPosList?.Count > 0) arena.dmItemPositions[player.UserIDString] = dmPosList;
                        else arena.dmItemPositions.Remove(player.UserIDString);

                    }
                }
                finally { Pool.FreeList(ref dmPlayers); }

            }
        }

        public bool UpdatingTeams { get; set; } = false;

        private IEnumerator UpdateTeams()
        {
            if (Arenas == null || Arenas.Count < 1 || UpdatingTeams) yield return null;
            var bpCount = BasePlayer.activePlayerList?.Count ?? 0;
            if (bpCount > 0)
            {
                var max = 10;
                var count = 0;
                for (int i = 0; i < bpCount; i++)
                {
                    if (((BasePlayer.activePlayerList?.Count ?? -1) - 1) < i)
                    {
                        PrintWarning("UpdateTeams breaking loop because we've gone over the active player list count (player probably disconnected)");
                        break;
                    }

                    var p = BasePlayer.activePlayerList[i];
                    if (p == null || p.IsDestroyed || p.gameObject == null || !p.IsConnected || p.IsDead()) continue;

                    var getDM = GetPlayerDM(p);
                    if (getDM == null || getDM.Teams) continue; //if they're not in DM **or** they're in a DM with teams enabled, we don't wanna mess that up

                    if (count >= max)
                    {
                        count = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }

                    p.CancelInvoke(p.TeamUpdate);
                    p.CancelInvoke(p.DelayedTeamUpdate);

                    count++;
                }
            }
        }

        private void BroadcastDMs()
        {
            if (Arenas == null || Arenas.Count < 1) return;
            var dmSB = new StringBuilder();
            var sbList = Pool.GetList<StringBuilder>();
            for (int i = 0; i < Arenas.Count; i++)
            {
                var arena = Arenas[i];
                if (arena == null || arena.LocalArena || arena.Disabled || arena.Hide || (arena?.ActivePlayers?.Count ?? 0) < 1) continue;
                var dmTxt = "<color=#a6fc60>There are </color><color=#fcb355>" + arena.ActivePlayers.Count.ToString("N0") + "</color><color=#a6fc60> players in the </color><color=#fcb355>" + (arena?.ArenaName ?? "Unknown") + "</color><color=#a6fc60> arena. Type </color><color=#fcb355>/dm</color> <color=#a6fc60>to join!</color>";
                if ((dmSB.Length + dmTxt.Length) >= 768)
                {
                    sbList.Add(dmSB);
                    dmSB = new StringBuilder();
                }
                dmSB.AppendLine(dmTxt);
            }
            if (dmSB.Length > 0) SimpleBroadcast(dmSB.ToString().TrimEnd());
            if (sbList.Count > 0) for (int i = 0; i < sbList.Count; i++) SimpleBroadcast(sbList[i].ToString().TrimEnd());
            Pool.FreeList(ref sbList);
        }

        private IEnumerator InitPlayers()
        {
            if (BasePlayer.activePlayerList == null || BasePlayer.activePlayerList.Count < 1) yield return null;
            var count = 0;
            var max = 15;
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || player.IsDead() || player?.transform == null) continue;
                count++;
                if (count >= max)
                {
                    count = 0;
                    yield return CoroutineEx.waitForEndOfFrame;
                }

                string lastDMTime;
                if (!dmData.lastDMTime.TryGetValue(player.UserIDString, out lastDMTime)) continue;
                string lastDMName;
                if (!dmData.lastDMName.TryGetValue(player.UserIDString, out lastDMName))
                {
                    PrintWarning("no last dm name: " + player.displayName);
                    continue;
                }
                if (string.IsNullOrEmpty(lastDMTime) || string.IsNullOrEmpty(lastDMName))
                {
                    PrintWarning("nullempty dmtime or dmname: " + player.displayName);
                    continue;
                }
                DateTime lastDT;
                if (!DateTime.TryParse(lastDMTime, out lastDT))
                {
                    PrintWarning("bad lastDMTime: " + lastDMTime + ", " + player.displayName);
                    continue;
                }
                if ((DateTime.Now - lastDT).TotalSeconds > 5) continue;
                var lastDM = FindArena(lastDMName);
                if (lastDM == null || (lastDM.Disabled && !player.IsAdmin) || ((lastDM.SpawnList == null || lastDM.SpawnList.Count < 1) && (lastDM.AxisSpawns == null || lastDM.AxisSpawns.Count < 1 || lastDM.AlliesSpawns == null || lastDM.AlliesSpawns.Count < 1))) continue;
                if (lastDM.Teams)
                {
                    ArenaInfo.Team lastTeam;
                    if (dmData.lastDMTeam.TryGetValue(player.UserIDString, out lastTeam) && lastTeam != ArenaInfo.Team.None) lastDM.SetTeam(player, lastTeam);
                }
                if (!lastDM.JoinDM(player)) PrintWarning("failed automatic JoinDM for: " + player.displayName);
                else
                {
                    SendReply(player, "Joined <color=#fcb355>" + lastDM.ArenaName + "</color> arena! Type <color=#fcb355>/dm leave</color> to leave.");
                    WakePlayerUp(player, true);
                }
            }
        }

        private void OnServerInitialized()
        {
            if (NewSave)
            {
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena != null) arena.Disabled = true;
                }
            }
            PrintWarning("Sphere scale is: " + GetSphereScale());
            boundary = TerrainMeta.Size.x / 2;
            ServerMgr.Instance.Invoke(() =>
            {
                ServerMgr.Instance.StartCoroutine(InitPlayers());
            }, 0.1f);

            timer.Every(30f, () =>
            {
                if (Arenas == null || Arenas.Count < 1) return;
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena == null || !arena.Teams || arena.ActivePlayers == null || arena.ActivePlayers.Count < 1 || !arena.GameInProgress || arena.StartingGame) continue;
                    var axisDiff = arena.GetTeamCount(ArenaInfo.Team.Axis) - arena.GetTeamCount(ArenaInfo.Team.Allies);
                    var alliesDiff = arena.GetTeamCount(ArenaInfo.Team.Allies) - arena.GetTeamCount(ArenaInfo.Team.Axis);
                    var topTeam = (alliesDiff > 1) ? ArenaInfo.Team.Allies : (axisDiff > 1) ? ArenaInfo.Team.Axis : ArenaInfo.Team.None;
                    if (topTeam == ArenaInfo.Team.None)
                    {
                        // PrintWarning("Didn't need to auto balance, or team is somehow otherwise none (topTeam == None)");
                        continue;
                    }

                    var teamPlayers = arena.GetTeamPlayers(topTeam);
                    if (teamPlayers == null || teamPlayers.Count < 2) continue;

                    PrintWarning("Auto balancing teams, allies diff: " + alliesDiff + ", axis: " + axisDiff);
                    SendMessageToDM(arena, "Auto balancing teams...");

                    var newTeam = topTeam == ArenaInfo.Team.Allies ? ArenaInfo.Team.Axis : ArenaInfo.Team.Allies;

                    var lastDeathTime = -1f;
                    BasePlayer findPlayer = null;
                    for (int j = 0; j < teamPlayers.Count; j++)
                    {
                        var p = teamPlayers[j];
                        if (p == null) continue;
                        var deathTime = GetLastDeathTime(p.UserIDString);
                        if (lastDeathTime < 0 || deathTime < lastDeathTime)
                        {
                            lastDeathTime = deathTime;
                            findPlayer = p;
                        }
                    }


                    if (findPlayer == null)
                    {
                        PrintWarning("find player is null for min death time!!");
                        continue;
                    }

                    arena.SetTeam(findPlayer, newTeam);
                    if (arena.DoSpawns(findPlayer, SpawnPoint.SpawnType.Base))
                    {
                        if (!arena.CreateItems(findPlayer, true)) PrintWarning("failed to create items for find player after team balance!!!");
                        var autoMsg = "You were auto balanced to <color=#a6fc60><size=18>" + newTeam + "</size></color>";
                        ShowPopup(findPlayer.UserIDString, autoMsg, 7f);
                        SendReply(findPlayer, autoMsg);
                        SendMessageToDM(arena, "<color=#fcb355>" + findPlayer.displayName + "</color> was switched to " + newTeam);
                    }
                    else
                    {
                        arena.SetTeam(findPlayer, topTeam);
                        PrintWarning("did NOT do spawns for player after team balance!!");
                    }
                }
            });


            InvokeHandler.InvokeRepeating(ServerMgr.Instance, () => ServerMgr.Instance.StartCoroutine(CheckZM()), 5f, 8f);
            InvokeHandler.InvokeRepeating(ServerMgr.Instance, () => ServerMgr.Instance.StartCoroutine(CheckItems()), 5f, 9f);
            InvokeHandler.InvokeRepeating(ServerMgr.Instance, BroadcastDMs, 180f, 180f);
            //InvokeHandler.InvokeRepeating(ServerMgr.Instance, UpdateDMGUIs, 5f, 2f);
            InvokeHandler.InvokeRepeating(ServerMgr.Instance, () =>
            {
                var anyPlayers = false;
                if (Arenas != null && Arenas.Count > 0)
                {
                    for (int i = 0; i < Arenas.Count; i++)
                    {
                        var arena = Arenas[i];
                        if (arena?.ActivePlayers != null && arena.ActivePlayers.Count > 0)
                        {
                            anyPlayers = true;
                            break;
                        }
                    }
                }
                UpdateHooks(anyPlayers);
            }, 1f, 120f);

            InvokeHandler.InvokeRepeating(ServerMgr.Instance, () => ServerMgr.Instance.StartCoroutine(UpdateTeams()), 5f, 12.5f);
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var sync = entity as EnvSync;
                if (sync != null)
                {
                    envSync = sync;
                    break;
                }
            }
            if (envSync == null || envSync.IsDestroyed) PrintError("envSync is null or destroyed!!!");
            
            _saveCoroutine = ServerMgr.Instance.StartCoroutine(SaveAllData(18000, true));

            init = true;
        }

        private bool NewSave { get; set; } = false;

        private void OnNewSave(string save)
        {
            PrintWarning("OnNewSave: " + save);
            var saveFile = save;
            var getSave = GetConfig("Save", string.Empty);
            if (string.IsNullOrEmpty(getSave) || getSave != saveFile)
            {
                NewSave = true;
                Config["Save"] = saveFile;
                PrintWarning("getSave: " + getSave + " != " + saveFile + ", updating & disabling DMs");
                SaveConfig();
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena != null) arena.Disabled = true;
                }
            }
        }

        private void UpdateDMGUIs()
        {
            if (Arenas == null || Arenas.Count < 1) return;
            for (int i = 0; i < Arenas.Count; i++)
            {
                var arena = Arenas[i];
                if (arena == null || !arena.Fortnite) continue;
                arena.AliveGUIAll();
                arena.KillsGUIAll();
            }
        }


        #endregion
        private readonly Dictionary<string, Timer> popupTimer = new Dictionary<string, Timer>();

        private void ShowPopup(string Id, string msg, float duration = 5f)
        {
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException();
            if (duration <= 0.0f) throw new ArgumentOutOfRangeException();
            var player = covalence.Players.FindPlayerById(Id);
            if (player == null || !player.IsConnected) return;
            player.Command("gametip.showgametip", msg);
            Timer endTimer;
            if (popupTimer.TryGetValue(Id, out endTimer)) endTimer.Destroy();
            endTimer = timer.Once(duration, () =>
            {
                if (player != null && player.IsConnected) player.Command("gametip.hidegametip");
            });
            popupTimer[Id] = endTimer;
        }

        private void ShowPopupDM(ArenaInfo arena, string msg, float duration = 5f)
        {
            if (arena == null || arena.ActivePlayers == null || arena.ActivePlayers.Count < 1) return;
            foreach (var ply in arena.ActivePlayers) ShowPopup(ply?.UserIDString ?? string.Empty, msg, duration);
        }

        private void GetClearWeatherVarsNoAlloc(ref List<ClientVarWithValue> vars)
        {
            if (vars == null) throw new ArgumentNullException(nameof(vars));

            var rainVar = ClientVarWithValue.GetOrCreateClientVar("weather.rain", "0");// //new ClientVarWithValue("weather.rain", "0");
            var fogVar = ClientVarWithValue.GetOrCreateClientVar("weather.fog", "0");
            var cloudsVar = ClientVarWithValue.GetOrCreateClientVar("weather.cloud_coverage", "0");
            var thunderVar = ClientVarWithValue.GetOrCreateClientVar("weather.thunder", "0");
            var windVar = ClientVarWithValue.GetOrCreateClientVar("weather.wind", "0");
            var atmosphereContrastVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", "1.2");
            var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_brightness", "1");
            var atmosphereDirectionalityVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_directionality", "0.9");
            var atmosphereMieVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_mie", "1");
            var atmosphereRayleighVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_rayleigh", "1");
         //   var atmosphereBrightnessVar = ClientVarWithValue.GetOrCreateClientVar("weather.atmosphere_contrast", "1.2");
            var rainbowVar = ClientVarWithValue.GetOrCreateClientVar("weather.rainbow", "0");


            vars.Add(rainVar);
            vars.Add(fogVar);
            vars.Add(cloudsVar);
            vars.Add(thunderVar);
            vars.Add(windVar);
            vars.Add(atmosphereContrastVar);
            vars.Add(rainbowVar);
            vars.Add(atmosphereBrightnessVar);
            vars.Add(atmosphereDirectionalityVar);
            vars.Add(atmosphereMieVar);
            vars.Add(atmosphereRayleighVar);
        }

        #region Util
        private class ClientVarWithValue
        {
            public string FullCommandName { get; set; }
            public string VariableValue { get; set; } = string.Empty;

            public static List<ClientVarWithValue> All { get; set; } = new List<ClientVarWithValue>();

            public ClientVarWithValue()
            {
                All.Add(this);
            }
            public ClientVarWithValue(string commandName, string variableValue)
            {
                if (string.IsNullOrEmpty(commandName)) throw new ArgumentNullException(nameof(commandName));

                FullCommandName = commandName;
                VariableValue = variableValue;

                All.Add(this);
            }

            public static ClientVarWithValue GetOrCreateClientVar(string fullCommandName, string setVariableValue = "")
            {
                if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

                for(int i = 0; i < All.Count; i++)
                {
                    var var = All[i];
                    if (var.FullCommandName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(setVariableValue)) var.VariableValue = setVariableValue;
                        return var;
                        
                    }
                }

                return new ClientVarWithValue(fullCommandName, setVariableValue);
            }
        }

        private void SendReplicatedVarsToConnection(Network.Connection connection, string filter = "")
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var cmds = Pool.GetList<ConsoleSystem.Command>();

            try
            {
                for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
                {
                    var cmd = ConsoleSystem.Index.Server.Replicated[i];
                    if (string.IsNullOrEmpty(filter) || cmd.FullName.StartsWith(filter)) cmds.Add(cmd);

                }

                if (cmds.Count < 1)
                {
                    PrintWarning("No replicated commands found with filter: " + filter);
                    return;
                }

                var netWrite = Net.sv.StartWrite();

                netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
                netWrite.Int32(cmds.Count);

                for (int i = 0; i < cmds.Count; i++)
                {
                    var var = cmds[i];
                    netWrite.String(var.FullName);
                    netWrite.String(var.String);
                }


                netWrite.Send(new SendInfo(connection));

            }
            finally { Pool.FreeList(ref cmds); }


        }

        private void SendClientsVar(List<Network.Connection> connections, List<ClientVarWithValue> vars)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));

            if (connections.Count < 1) throw new ArgumentOutOfRangeException(nameof(connections));

            if (vars == null) throw new ArgumentNullException(nameof(vars));
            if (vars.Count < 1) throw new ArgumentOutOfRangeException(nameof(vars));

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(vars.Count);

            for (int i = 0; i < vars.Count; i++)
            {
                var var = vars[i];
                netWrite.String(var.FullCommandName);
                netWrite.String(var.VariableValue);
            }


            netWrite.Send(new SendInfo(connections));

        }

        private void SendClientVars(Network.Connection connection, List<ClientVarWithValue> vars)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (vars == null) throw new ArgumentNullException(nameof(vars));
            if (vars.Count < 1) throw new ArgumentOutOfRangeException(nameof(vars));


            var netWrite = Net.sv.StartWrite();


            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(vars.Count);

            for (int i = 0; i < vars.Count; i++)
            {
                var var = vars[i];
                netWrite.String(var.FullCommandName);
                netWrite.String(var.VariableValue);
            }


            netWrite.Send(new SendInfo(connection));

        }

        private void SendClientVar(Network.Connection connection, string fullCommandName, string varValue)
        {
            if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

            var foundCmd = false;
            for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
            {
                var cmd = ConsoleSystem.Index.Server.Replicated[i];
                if (cmd.FullName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    foundCmd = true;
                    fullCommandName = cmd.FullName;
                    break;
                }
            }

            if (!foundCmd)
            {
                PrintWarning(nameof(SendClientVar) + " failed to find replicated var!");
                return;
            }

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(1);

            netWrite.String(fullCommandName);
            netWrite.String(varValue);

            netWrite.Send(new SendInfo(connection));

        }

        private void SendClientsVar(List<Network.Connection> connections, string fullCommandName, string varValue)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));

            if (connections.Count < 1) throw new ArgumentOutOfRangeException(nameof(connections));

            if (string.IsNullOrEmpty(fullCommandName)) throw new ArgumentNullException(nameof(fullCommandName));

            var foundCmd = false;
            for (int i = 0; i < ConsoleSystem.Index.Server.Replicated.Count; i++)
            {
                var cmd = ConsoleSystem.Index.Server.Replicated[i];
                if (cmd.FullName.Equals(fullCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    foundCmd = true;
                    fullCommandName = cmd.FullName;
                    break;
                }
            }

            if (!foundCmd)
            {
                PrintWarning(nameof(SendClientsVar) + " failed to find replicated var!");
                return;
            }

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
            netWrite.Int32(1);

            netWrite.String(fullCommandName);
            netWrite.String(varValue);

            netWrite.Send(new SendInfo(connections));
        }

        public void RemoveAllDMItems(BasePlayer player)
        {
            if (player == null || player.IsDestroyed || player.IsDead() || player.inventory == null) return;

            var items = Pool.GetList<Item>();
            try
            {
                var itemCount = player?.inventory?.AllItemsNoAlloc(ref items) ?? 0;
                if (itemCount > 0)
                {
                    var remSB = new StringBuilder();
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (item?.info?.shortname != "note" && (item?.text?.Contains("DM Item", System.Globalization.CompareOptions.OrdinalIgnoreCase) ?? false))
                        {
                            remSB.AppendLine("Removing DM item from: " + player?.displayName + " Item: " + item?.info?.shortname + " x" + item?.amount);
                            RemoveFromWorld(item);
                        }
                    }
                    if (remSB.Length > 0) PrintWarning(remSB.ToString().TrimEnd());
                }
            }
            finally { Pool.FreeList(ref items); }
        }

        public static void CelebrateEffects(BasePlayer player)
        {
            if (player == null || player.gameObject == null || player.IsDestroyed || player.transform == null) return;
            var eyePos = player?.eyes?.transform?.position ?? Vector3.zero;
            var eyeForward = player?.eyes?.HeadForward() ?? Vector3.zero;
            for (int j = 0; j < UnityEngine.Random.Range(2, 6); j++)
            {
                Effect.server.Run("assets/bundled/prefabs/fx/missing.prefab", SpreadVector2(eyePos + eyeForward * UnityEngine.Random.Range(1.5f, 1.9f), UnityEngine.Random.Range(1.35f, 2.2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/bonus_hit.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + UnityEngine.Random.Range(0.5f, 1f), eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(0.8f, 2f)), Vector3.zero);
                Effect.server.Run("assets/prefabs/misc/orebonus/effects/hotspot_death.prefab", SpreadVector2(new Vector3(eyePos.x, eyePos.y + 0.6f, eyePos.z) + eyeForward * UnityEngine.Random.Range(0.6f, 1.2f), UnityEngine.Random.Range(1.1f, 3f)), Vector3.zero);
            }
            SendLocalEffect(player, "assets/prefabs/deployable/research table/effects/research-success.prefab");
            player.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
        }

        public static void SendLocalEffect(BasePlayer player, string effect, Vector3 pos, float scale = 1f)
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect) || pos == Vector3.zero) return;
            using (var fx = new Effect(effect, pos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        public static void SendLocalEffect(BasePlayer player, string effect, float scale = 1f, uint boneID = 0, Vector3 localPos = default(Vector3))
        {
            if (player == null || player?.net?.connection == null || !player.IsConnected || string.IsNullOrEmpty(effect)) return;
            using (var fx = new Effect(effect, player, boneID, localPos, Vector3.zero))
            {
                fx.scale = scale;
                EffectNetwork.Send(fx, player?.net?.connection);
            }
        }

        public static Vector3 SpreadVector2(Vector3 vec, float spread) { return vec + UnityEngine.Random.insideUnitSphere * spread; }

        private string ReadableTimeSpan(TimeSpan span, string stringFormat = "N0")
        {
            if (span == TimeSpan.MinValue) return string.Empty;
            var str = string.Empty;
            var repStr = stringFormat.StartsWith("0.0") ? ("." + stringFormat.Replace("0.", string.Empty)) : string.Empty;
            if (span.TotalHours >= 24) str = (int)span.TotalDays + " day" + (span.TotalDays >= 2 ? "s" : "") + " " + (span.TotalHours - ((int)span.TotalDays * 24)).ToString(stringFormat).Replace(repStr, string.Empty) + " hour(s)";
            else if (span.TotalMinutes > 60) str = (int)span.TotalHours + " hour" + (span.TotalHours >= 2 ? "s" : "") + " " + (span.TotalMinutes - ((int)span.TotalHours * 60)).ToString(stringFormat).Replace(repStr, string.Empty) + " minute(s)";
            else if (span.TotalMinutes > 1.0) str = span.Minutes + " minute" + (span.Minutes >= 2 ? "s" : "") + (span.Seconds < 1 ? "" : " " + span.Seconds + " second" + (span.Seconds >= 2 ? "s" : ""));
            if (!string.IsNullOrEmpty(str)) return str;
            return (span.TotalDays >= 1.0) ? span.TotalDays.ToString(stringFormat).Replace(repStr, string.Empty) + " day" + (span.TotalDays >= 1.5 ? "s" : "") : (span.TotalHours >= 1.0) ? span.TotalHours.ToString(stringFormat).Replace(repStr, string.Empty) + " hour" + (span.TotalHours >= 1.5 ? "s" : "") : (span.TotalMinutes >= 1.0) ? span.TotalMinutes.ToString(stringFormat).Replace(repStr, string.Empty) + " minute" + (span.TotalMinutes >= 1.5 ? "s" : "") : (span.TotalSeconds >= 1.0) ? span.TotalSeconds.ToString(stringFormat).Replace(repStr, string.Empty) + " second" + (span.TotalSeconds >= 1.5 ? "s" : "") : span.TotalMilliseconds.ToString("N0") + " millisecond" + (span.TotalMilliseconds >= 1.5 ? "s" : "");
        }

        private bool? lastUpdateState;

        private void UpdateHooks(bool state)
        {
            if (lastUpdateState.HasValue && lastUpdateState == state) return;
            if (state)
            {
                Subscribe(nameof(CanBeTargeted));
                Subscribe(nameof(OnHelicopterTarget));
                Subscribe(nameof(CanHelicopterTarget));
                Subscribe(nameof(CanHelicopterStrafeTarget));
                Subscribe(nameof(OnRunPlayerMetabolism));
                Subscribe(nameof(OnPlayerInput));
                Subscribe(nameof(OnEntitySpawned));
                Subscribe(nameof(OnEntityTakeDamage));
                Subscribe(nameof(OnLootSpawn));
                Subscribe(nameof(OnLoseCondition));
                Subscribe(nameof(OnMaxStackable));
                Subscribe(nameof(CanStackItem));
                Subscribe(nameof(OnDispenserGather));
                Subscribe(nameof(CanLootEntity));
                Subscribe(nameof(OnWeaponFired));
                Subscribe(nameof(OnPlayerRespawned));
                Subscribe(nameof(OnItemDropped));
                Subscribe(nameof(CanBuild));
                Subscribe(nameof(OnItemDeployed));
                Subscribe(nameof(CanNetworkTo));
                Subscribe(nameof(OnServerCommand));
                Subscribe(nameof(CanAcceptItem));
                Subscribe(nameof(OnPlayerDeath));
                Subscribe(nameof(OnPlayerViolation));
            }
            else
            {
                Unsubscribe(nameof(CanBeTargeted));
                Unsubscribe(nameof(OnHelicopterTarget));
                Unsubscribe(nameof(CanHelicopterTarget));
                Unsubscribe(nameof(CanHelicopterStrafeTarget));
                Unsubscribe(nameof(OnRunPlayerMetabolism));
                Unsubscribe(nameof(OnPlayerInput));
                Unsubscribe(nameof(OnEntitySpawned));
                Unsubscribe(nameof(OnEntityTakeDamage));
                Unsubscribe(nameof(OnLootSpawn));
                Unsubscribe(nameof(OnLoseCondition));
                Unsubscribe(nameof(OnMaxStackable));
                Unsubscribe(nameof(CanStackItem));
                Unsubscribe(nameof(OnDispenserGather));
                Unsubscribe(nameof(CanLootEntity));
                Unsubscribe(nameof(OnWeaponFired));
                Unsubscribe(nameof(OnPlayerRespawned));
                Unsubscribe(nameof(OnItemDropped));
                Unsubscribe(nameof(CanBuild));
                Unsubscribe(nameof(OnItemDeployed));
                Unsubscribe(nameof(CanNetworkTo));
                Unsubscribe(nameof(OnServerCommand));
                Unsubscribe(nameof(CanAcceptItem));
                Unsubscribe(nameof(OnPlayerDeath));
                Unsubscribe(nameof(OnPlayerViolation));
            }
            lastUpdateState = state;
        }
        public Vector3 GetAverageVector(List<Vector3> vectors)
        {
            if (vectors == null || vectors.Count < 1) return Vector3.zero;
            return new Vector3(vectors.Average(p => p.x), vectors.Average(p => p.y), vectors.Average(p => p.z));
        }

        public Vector3 GetAverageVector(Vector3[] vectors)
        {
            if (vectors == null || vectors.Length < 1) return Vector3.zero;
            return new Vector3(vectors.Average(p => p.x), vectors.Average(p => p.y), vectors.Average(p => p.z));
        }

        public Vector3 GetAverageVector(IEnumerable<Vector3> vectors)
        {
            if (vectors == null || !vectors.Any()) return Vector3.zero;
            return new Vector3(vectors.Average(p => p.x), vectors.Average(p => p.y), vectors.Average(p => p.z));
        }

        private void PlayCodeLockUpdate(BasePlayer player, int timesToRun = 1, bool self = false)
        {
            if (player == null || player?.transform == null || timesToRun < 1) return;
            var con = self ? (player?.net?.connection ?? null) : null;
            PlayCodeLockUpdate(player?.eyes?.position ?? Vector3.zero, timesToRun, con);
        }

        private void PlayCodeLockUpdate(Vector3 position, int timesToRun = 1, Connection connection = null)
        {
            if (position == Vector3.zero || timesToRun < 1) return;
            var fx = "assets/prefabs/locks/keypad/effects/lock.code.updated.prefab";
            if (connection != null)
            {
                using (var efx = new Effect(fx, position, Vector3.zero)) EffectNetwork.Send(efx, connection);
            }
            else Effect.server.Run(fx, position, Vector3.zero);
        }

        private class SphereTrigger : MonoBehaviour
        {
            public GameObject parentObject;
            private float _radius;
            public float Radius
            {
                get { return _radius; }
                set
                {
                    _radius = value;
                    UpdateColliders();
                }
            }
            public string Name
            {
                get { return gameObject?.name; }
                set { if (gameObject != null) gameObject.name = value; }
            }

            private void UpdateColliders()
            {
                var sphereCollider = gameObject.GetComponent<SphereCollider>();
                if (Radius <= 0.0f && sphereCollider != null)
                {
                    Destroy(sphereCollider);
                    return;
                }
                if (Radius <= 0.0f) return;
                if (sphereCollider == null) sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = Radius;
                sphereCollider.enabled = true;
            }

            private void Awake()
            {
                parentObject = GetComponent<BaseEntity>()?.gameObject ?? null;
                if (parentObject == null)
                {
                    Interface.Oxide.LogWarning("Awake() called with parentObject null!");
                    DoDestroy();
                    return;
                }
                gameObject.layer = (int)Rust.Layer.Reserved1; //hack to get all trigger layers...otherwise child zones
                gameObject.name = "KC";

                var rigidbody = gameObject?.GetComponent<Rigidbody>() ?? null;
                if (rigidbody == null)
                {
                    rigidbody = gameObject.AddComponent<Rigidbody>();
                    rigidbody.useGravity = false;
                    rigidbody.isKinematic = true;
                }
                rigidbody.detectCollisions = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                UpdateColliders();
                //     Interface.Oxide.LogWarning("Called UpdateColliders() on Awake(), parentObject pos: " + (GetComponent<BaseEntity>()?.transform?.position ?? Vector3.zero));
            }

            private void DoDestroy()
            {
                Destroy(this);
            }

            private void OnTriggerEnter(Collider col)
            {
                try
                {
                    if (col == null || this == null) return;
                    if (!(this is SphereTrigger))
                    {
                        Interface.Oxide.LogWarning("OnTriggerEnter called on !SphereTrigger");
                        return;
                    }
                    Interface.Oxide.CallHook("OnTriggerEnterSphere", this, col);
                }
                catch (Exception ex) { Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "Exception for OnTriggerEnter"); }
            }

            private void OnTriggerExit(Collider col)
            {
                try
                {
                    if (col == null || this == null) return;
                    if (!(this is SphereTrigger))
                    {
                        Interface.Oxide.LogWarning("OnTriggerExit called on !SphereTrigger");
                        return;
                    }
                    Interface.Oxide.CallHook("OnTriggerExitSphere", this, col);
                }
                catch (Exception ex) { Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "Exception for OnTriggerExit"); }
            }

        }

        private Vector3 SpreadVector(Vector3 vec, float rocketSpread = 1.5f) { return Quaternion.Euler(UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f), UnityEngine.Random.Range((float)(-rocketSpread * 0.2), rocketSpread * 0.2f)) * vec; }

        private Vector3 GetGroundPosition(Vector3 pos, int maxTries = 20)
        {
            if (pos == Vector3.zero || FindGround == null || !FindGround.IsLoaded) return pos;
            var newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
            maxTries = Mathf.Clamp(maxTries, 1, 100);
            if (newPosObj == null)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    if (newPosObj != null) break;
                    newPosObj = FindGround?.Call("GetTrueGroundPosition", pos) ?? null;
                }
                if (newPosObj == null)
                {
                    PrintWarning("null pos after " + maxTries.ToString("N0") + " tries");
                    return Vector3.zero;
                }
            }
            return (Vector3)newPosObj;
        }
        public static BaseEntity MakeItemFloat(Item item, Vector3 dropPos, bool setToGround = false)
        {
            if (item == null || dropPos == Vector3.zero) return null;
            var groundPos = setToGround ? DMMain?.GetGroundPosition(new Vector3(dropPos.x, dropPos.y + 0.5f, dropPos.z)) ?? Vector3.zero : dropPos;
            if (groundPos == Vector3.zero) return null;
            var drop = item?.GetWorldEntity() ?? null;
            var newPos = new Vector3(groundPos.x, groundPos.y + 0.65f, groundPos.z);
            if (drop == null) drop = item.Drop(newPos, Vector3.zero);
            else
            {
                if (drop?.transform != null)
                {
                    drop.transform.position = newPos;
                    drop.SendNetworkUpdateImmediate();
                }
            }
            if (drop == null)
            {
                RemoveFromWorld(item);
                return null;
            }
            var rigid = drop?.GetComponent<Rigidbody>() ?? null;
            if (rigid != null)
            {
                rigid.useGravity = false;
                rigid.isKinematic = true;
            }
            //          Interface.Oxide.LogWarning("Did drop/float at: " + groundPos);
            return drop;
        }

        private void JoinGUI(BasePlayer player, ArenaInfo arena)
        {
            if (player == null || !player.IsConnected || arena == null) return;


            var GUISkinElement = new CuiElementContainer();

            var GUISkin = GUISkinElement.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0.85"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1 1"
                },
                CursorEnabled = true
            }, "Overlay", "DMGUI");

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "dm.closegui",
                        Color = "0.6 0.4 0.7 0.85"
                    },
                Text =
                {
                    Text = "Close",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.1",
                    AnchorMax = "0.6 0.15"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "dm.autoteam",
                        Color = "0.6 0.4 0.7 0.85"
                    },
                Text =
                {
                    Text = "Auto Assign",
                    FontSize = 15,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.4 0.2",
                    AnchorMax = "0.6 0.25"
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = "Choose a team to join",
                    FontSize = 20,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.8",
                    AnchorMax = "1 1"
                }
            }, GUISkin);
            var buttonYMin = 0.22;
            var buttonYMax = 0.32;
            var labelMin = 0.65;
            var labelMax = 0.85;
            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = arena.GetTeamCount(ArenaInfo.Team.Allies).ToString("N0") + " players ->",
                    FontSize = 12,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = labelMin + " " + buttonYMin,
                    AnchorMax = labelMax + " " + buttonYMax
                }
            }, GUISkin);
            GUISkinElement.Add(new CuiLabel
            {
                Text =
                {
                    Text = arena.GetTeamCount(ArenaInfo.Team.Axis).ToString("N0") + " players ->",
                    FontSize = 12,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = labelMin + " " + (buttonYMin + 0.15f),
                    AnchorMax = labelMax + " " + (buttonYMax + 0.15f)
                }
            }, GUISkin);
            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "dm.jointeam 0",
                        Color = "0.2 0.4 0.65 0.75"
                    },
                Text =
                {
                    Text = "Join Allies",
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.75 " + buttonYMin,
                    AnchorMax = "0.95 " + buttonYMax
                }
            }, GUISkin);

            GUISkinElement.Add(new CuiButton
            {
                Button =
                    {
                        Command = "dm.jointeam 1",
                        Color = "0.2 0.4 0.65 0.75"
                    },
                Text =
                {
                    Text = "Join Axis",
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform =
                {
                    AnchorMin = "0.75 " + (buttonYMin + 0.15f),
                    AnchorMax = "0.95 " + (buttonYMax + 0.15f)
                }
            }, GUISkin);

            /*/
            for (int i = 0; i < length; i++)
            {
                var cmdText = "csay /skt ";
                var labelText = "Placeholder " + i;

               



               
                buttonYMin += 0.1;
                buttonYMax += 0.1;
            }/*/

            CuiHelper.DestroyUi(player, "DMGUI"); //destroy only after it's ready to be added
            CuiHelper.AddUi(player, GUISkinElement);
        }
        private void RenderUIBlindText(BasePlayer player, string textToUse, int fontSize = 32, string textColor = "1 0 0 1")
        {
            if (player == null || !player.IsConnected || string.IsNullOrEmpty(textToUse)) return;
            CuiHelper.DestroyUi(player, "BlindUIText");
            var elements = new CuiElementContainer();
            var textAdd = elements.Add(new CuiLabel
            {
                Text =
                {
                    Text = textToUse,
                   Color = "1 0 0 1",
                    FontSize = 32,
                    Align = TextAnchor.MiddleCenter
                },
                RectTransform =
                {
                    AnchorMin = "0 0.6",
                    AnchorMax = "1 0.8"
                }
            }, "Overlay", "BlindUIText");
            CuiHelper.AddUi(player, elements);
        }
        private void RenderUIBlind(BasePlayer player, string textToUse = "", float fadeIn = 0.5f, string backColor = "0 0 0 1", string textColor = "1 0 0 1")
        {
            if (player == null || !player.IsConnected) return;

            CuiHelper.DestroyUi(player, "BlindUI");
            CuiHelper.DestroyUi(player, "BlindUIText");
            Puts("RENDERUI blind for player: " + player.displayName);
            var blind = "0 0 0 1";

            PrintWarning("BLINDING: " + blind);

            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color =  blind,
                    FadeIn = fadeIn
                },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "1 1"
                }
            }, "Overlay", "BlindUI");
            if (!string.IsNullOrEmpty(textToUse))
            {
                var textAdd = elements.Add(new CuiLabel
                {
                    Text =
                {
                    Text = textToUse,
                   Color = "1 0 0 1",
                    FontSize = 32,
                    FadeIn = fadeIn,
                    Align = TextAnchor.MiddleCenter
                },
                    RectTransform =
                {
                    AnchorMin = "0 0.6",
                    AnchorMax = "1 0.8"
                }
                }, "Overlay", "BlindUIText");
            }

            CuiHelper.AddUi(player, elements);
        }

        private void DestroyClientEntity(BasePlayer player, BaseEntity entity, bool cache = false)
        {
            if (player == null || !player.IsConnected || player?.net == null || player?.net?.connection == null || entity == null || entity?.net == null || entity.IsDestroyed) return;

            HashSet<NetworkableId> netIDs = null;
            if (cache && netDestroyed.TryGetValue(player.userID, out netIDs) && netIDs.Contains(entity.net.ID)) return;//already destroyed, performance saver

            var netWrite = Net.sv.StartWrite();

            netWrite.PacketID(Message.Type.EntityDestroy);
            netWrite.EntityID(entity.net.ID);
            netWrite.UInt8((byte)BaseNetworkable.DestroyMode.None);
            netWrite.Send(new SendInfo(player.net.connection)
            {
                priority = Priority.Immediate
            });
            if (cache)
            {
                var netID = entity.net.ID;

                if (netIDs != null) netIDs.Add(netID);
                else
                {
                    netDestroyed[player.userID] = new HashSet<NetworkableId>
                    {
                        netID
                    };
                }
            }
        }

        private readonly Dictionary<ulong, Timer> guiTimer = new Dictionary<ulong, Timer>();
        private readonly Dictionary<ulong, Timer> guiLargeTimer = new Dictionary<ulong, Timer>();

        private void ScoreGUI(BasePlayer player, ArenaInfo arena, int score, float duration = -1f)
        {
            if (player == null || arena == null || !player.IsConnected || player?.net?.connection == null) return;
            var team = arena?.GetTeam(player) ?? ArenaInfo.Team.None;
            var enemyTeam = (team == ArenaInfo.Team.Allies) ? ArenaInfo.Team.Axis : (team == ArenaInfo.Team.Axis) ? ArenaInfo.Team.Allies : ArenaInfo.Team.None;
            var enemyScore = arena?.GetTeamKills(enemyTeam) ?? 0;
            //PrintWarning("Score gui!");
            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.25 0.25 0.25 0.7"
                },
                RectTransform =
                {
                    AnchorMin = "0.468 0.10",
                    AnchorMax = "0.52 0.145"
                }
            }, "Under", "DMGUI");


            var innerPingUIText = new CuiElement
            {
                Name = CuiHelper.GetGuid(),
                Parent = "DMGUI",
                Components =
                        {
                            new CuiTextComponent { Color = "0.9 0.7 0.85 1", Text = "<color=" + ((team == ArenaInfo.Team.Allies) ? "green" : "red") + ">" + team + "</color>: " + score.ToString("N0") + "\n<color=" + ((enemyTeam == ArenaInfo.Team.Allies) ? "green" : "red") + ">" + enemyTeam + "</color>: " + enemyScore.ToString("N0"), FontSize = 13, Align = TextAnchor.MiddleCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0 0", AnchorMax = "1 1" }
                        }
            };
            elements.Add(innerPingUIText);
            CuiHelper.DestroyUi(player, "DMGUI");
            CuiHelper.AddUi(player, elements);
            if (duration > 0.0f)
            {
                Timer outTimer;
                if (guiTimer.TryGetValue(player.userID, out outTimer))
                {
                    outTimer.Destroy();
                    outTimer = null;
                }

                outTimer = timer.Once(duration, () =>
                {
                    if (player != null && player.IsConnected) CuiHelper.DestroyUi(player, "DMGUI");
                });
                guiTimer[player.userID] = outTimer;
            }
        }

        private void ScoreGUILarge(BasePlayer player, ArenaInfo arena, float duration = -1f)
        {
            if (player == null || arena == null || !player.IsConnected || player?.net?.connection == null) return;
            var team = arena?.GetTeam(player) ?? ArenaInfo.Team.None;
            var teamScore = arena?.GetTeamKills(team) ?? 0;
            var enemyTeam = (team == ArenaInfo.Team.Allies) ? ArenaInfo.Team.Axis : (team == ArenaInfo.Team.Axis) ? ArenaInfo.Team.Allies : ArenaInfo.Team.None;
            var enemyScore = arena?.GetTeamKills(enemyTeam) ?? 0;
            var elements = new CuiElementContainer();
            var mainName = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.175 0.175 0.175 0.75"
                },
                RectTransform =
                {
                    AnchorMin = "0.33 0.306",
                    AnchorMax = "0.591 1"
                }
            }, "Under", "DMGUILarge");

            var alliesSB = new StringBuilder();
            var axisSB = new StringBuilder();
            if (arena?.ActivePlayers != null && arena.ActivePlayers.Count > 0)
            {
                foreach (var ap in arena.ActivePlayers)
                {
                    if (ap == null) continue;
                    var apScore = arena.GetPlayerKills(ap.UserIDString);
                    var apDeaths = arena.GetPlayerDeaths(ap.UserIDString);
                    var pTeam = arena.GetTeam(ap);
                    var msg = ap.displayName + ": " + apScore + "-" + apDeaths;
                    if (pTeam == ArenaInfo.Team.Allies) alliesSB.Append(msg + "\n");
                    else if (pTeam == ArenaInfo.Team.Axis) axisSB.Append(msg + "\n");
                }
            }
            var allyStr = alliesSB.Length > 0 ? alliesSB.ToString().TrimEnd() : string.Empty;
            var axisStr = axisSB.Length > 0 ? axisSB.ToString().TrimEnd() : string.Empty;
            var scoreMsg = "<color=" + ((team == ArenaInfo.Team.Allies) ? "green" : "red") + ">" + team + "</color>: " + teamScore.ToString("N0") + "\n" + (team == ArenaInfo.Team.Allies ? allyStr : axisStr) + "\n\n\n\n<color=" + ((enemyTeam == ArenaInfo.Team.Allies) ? "green" : "red") + ">" + enemyTeam + "</color>: " + enemyScore.ToString("N0") + "\n" + (enemyTeam == ArenaInfo.Team.Allies ? allyStr : axisStr);

            var teamScoreText = new CuiElement
            {
                Name = CuiHelper.GetGuid(),
                Parent = "DMGUILarge",
                Components =
                        {
                            new CuiTextComponent { Color = "1 1 1 1", Text = "\n\n\n" + scoreMsg, FontSize = 20, Align = TextAnchor.UpperCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0 0", AnchorMax = "1 1" }
                        }
            };


            elements.Add(teamScoreText);
            CuiHelper.DestroyUi(player, "DMGUILarge");
            CuiHelper.AddUi(player, elements);
            if (duration > 0.0f)
            {
                Timer outTimer;
                if (guiLargeTimer.TryGetValue(player.userID, out outTimer))
                {
                    outTimer.Destroy();
                    outTimer = null;
                }

                outTimer = timer.Once(duration, () =>
                {
                    if (player != null && player.IsConnected) CuiHelper.DestroyUi(player, "DMGUILarge");
                });
                guiLargeTimer[player.userID] = outTimer;
            }
        }

        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            BasePlayer player = null;
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    if (p?.UserIDString == name) return p;
                    var pName = p?.displayName ?? string.Empty;
                    if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                    if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        if (p?.UserIDString == name) return p;
                        var pName = p?.displayName ?? string.Empty;
                        if (string.Equals(pName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                        if (pName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                return null;
            }
            return player;
        }

        private bool IsVanished(BasePlayer player) { return Vanish?.Call<bool>("IsInvisible", player) ?? false; }

        private bool IsGodmode(BasePlayer player) { return Godmode?.Call<bool>("IsGod", player.UserIDString) ?? false; }

        private ListDictionary<InvokeAction, float> _invokeList = null;

        private ListDictionary<InvokeAction, float> InvokeList
        {
            get
            {
                if (_invokeList == null && InvokeHandler.Instance != null) _invokeList = _curList.GetValue(InvokeHandler.Instance) as ListDictionary<InvokeAction, float>;
                return _invokeList;
            }
        }

        private void CancelInvoke(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null).ToString() == targetName && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private void CancelInvoke(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName) || obj == null) return;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    var key = inv.Key;
                    if ((key.action?.Target ?? null) == obj && (key.action?.Method?.Name ?? string.Empty) == methodName)
                    {
                        InvokeHandler.CancelInvoke(key.sender, key.action);
                        break;
                    }
                }
            }
        }

        private bool IsInvoking(string methodName, object obj)
        {
            if (string.IsNullOrEmpty(methodName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null) == obj) return true;
                }
            }
            return false;
        }
        private bool IsInvoking(string methodName, string targetName)
        {
            if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(targetName)) return false;
            var invList = InvokeList;
            if (invList != null && invList.Count > 0)
            {
                foreach (var inv in invList)
                {
                    if ((inv.Key.action?.Method?.Name ?? string.Empty) == methodName && (inv.Key.action?.Target ?? null).ToString() == targetName) return true;
                }
            }
            return false;
        }

        private ArenaInfo GetEditArena(ulong userID)
        {
            ArenaInfo arena;
            if (!editArena.TryGetValue(userID, out arena)) return null;
            else return arena;
        }

        private void SetEditArena(ulong userID, ArenaInfo arena)
        {
            if (userID == 0 || arena == null) return;
            editArena[userID] = arena;
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

        private ArenaInfo GetPlayerDM(BasePlayer player)
        {
            if (player == null) return null;
            for (int i = 0; i < Arenas.Count; i++)
            {
                var arena = Arenas[i];
                if (arena == null || arena.ActivePlayers == null) continue;
                if (arena.ActivePlayers.Contains(player)) return arena;
            }
            return null;
        }

        private Vector3 GetLastDeath(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException();
            Vector3 lastPos;
            if (!lastDeath.TryGetValue(userId, out lastPos)) lastPos = Vector3.zero;
            return lastPos;
        }

        private float GetLastDeathTime(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return -1f;
            float lastDeath;
            if (!lastDeathTime.TryGetValue(userId, out lastDeath)) return -1f;
            return lastDeath;
        }

        private float GetSecondsSinceDeath(string userId)
        {
            var deathTime = GetLastDeathTime(userId);
            if (deathTime == -1f) return -1f;
            return Time.realtimeSinceStartup - deathTime;
        }

        private string RemoveTags(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;


            //	Replace Color Tags
            phrase = _colorRegex.Replace(phrase, string.Empty);
            //	Replace Size Tags
            phrase = _sizeRegex.Replace(phrase, string.Empty);

            var phraseSB = Pool.Get<StringBuilder>();
            try
            {
                phraseSB.Clear().Append(phrase);

                foreach (var tag in _forbiddenTags)
                    phraseSB.Replace(tag, string.Empty);

                return phraseSB.ToString();
            }
            finally { Pool.Free(ref phraseSB); }
        }


        private bool IsCrafting(BasePlayer player) { return (player?.inventory?.crafting?.queue?.Count ?? 0) > 0; }

        private bool IsEscapeBlocked(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            return NoEscape?.Call<bool>("IsEscapeBlockedS", userID) ?? false;
        }

        private void SimpleBroadcast(string msg, ulong userID = 0)
        {
            if (string.IsNullOrEmpty(msg)) return;
            ConVar.Chat.ChatEntry chatEntry = new ConVar.Chat.ChatEntry()
            {
                Channel = ConVar.Chat.ChatChannel.Server,
                Message = RemoveTags(msg),
                UserId = userID.ToString(),
                Time = Facepunch.Math.Epoch.Current
            };
            Facepunch.RCon.Broadcast(Facepunch.RCon.LogType.Chat, chatEntry);
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var player = BasePlayer.activePlayerList[i];
                if (player == null || !player.IsConnected) continue;
                if (userID > 0) player.SendConsoleCommand("chat.add", string.Empty, userID, msg);
                else SendReply(player, msg);
            }
        }

        private void SendMessageToDM(ArenaInfo arena, string msg)
        {
            if (arena == null || (arena?.ActivePlayers?.Count ?? 0) < 1) return;
            foreach (var ply in arena.ActivePlayers) SendReply(ply, msg);
        }

        private int GetPing(BasePlayer player)
        {
            if ((player?.net?.connection ?? null) == null) return -1;
            return Net.sv.GetAveragePing(player.net.connection);
        }

        private bool CheckBoundaries(float x, float y, float z) { return x <= boundary && x >= -boundary && y < 2000 && y >= -100 && z <= boundary && z >= -boundary; }
        private bool CheckBoundaries(Vector3 pos) { return CheckBoundaries(pos.x, pos.y, pos.z); }

        private void SaveData()
        {
            var watch = Pool.Get<Stopwatch>();
            try
            {
                watch.Restart();
                Interface.Oxide.DataFileSystem.WriteObject("DMData", dmData);

                if (arenaData != null)
                {
                    var newData = new ArenaData();
                    if (arenaData?.arenas != null)
                    {
                        for (int i = 0; i < arenaData.arenas.Count; i++)
                        {
                            var arena = arenaData.arenas[i];
                            if (arena != null && !arena.LocalArena) newData.arenas.Add(arena);
                        }
                    }
                    Interface.Oxide.DataFileSystem.WriteObject("DMArenas", newData);
                }
                if (watch.ElapsedMilliseconds > 2) PrintWarning("SaveData() for Deathmatch took: " + watch.ElapsedMilliseconds.ToString("0.00").Replace(".00", string.Empty) + "ms");
            }
            finally
            {
                Pool.Free(ref watch);
            }
        }

        private IEnumerator SaveAllData(float interval = 18000, bool init = false)
        {
            PrintWarning(nameof(Deathmatch) + "." + nameof(SaveAllData) + " called, this will create a while loop that will effectively continue as a timer until it is canceled (coroutine)");

            if (init)
            {
                PrintWarning("init true, so waiting interval time before saving first time");
                yield return CoroutineEx.waitForSecondsRealtime(interval);
            }

            while (true)
            {
                var watch = Pool.Get<Stopwatch>();
                try
                {
                    PrintWarning("Yielding 3f before continuing with saving data");

                    yield return CoroutineEx.waitForSecondsRealtime(3f);

                    PrintWarning("Saving DMData...");

                    watch.Restart();

                    Interface.Oxide.DataFileSystem.WriteObject("DMData", dmData);

                    PrintWarning("Saved DMData in: " + watch.Elapsed.TotalMilliseconds + "ms");

                    yield return CoroutineEx.waitForSecondsRealtime(5f);

                    if (arenaData != null)
                    {
                        PrintWarning("Saving DMArenas data...");

                        watch.Restart();
                        
                        var newData = new ArenaData();
                        
                        if (arenaData?.arenas != null)
                        {
                            for (int i = 0; i < arenaData.arenas.Count; i++)
                            {
                                var arena = arenaData.arenas[i];
                                if (arena != null && !arena.LocalArena) newData.arenas.Add(arena);
                            }
                        }
                        
                        Interface.Oxide.DataFileSystem.WriteObject("DMArenas", newData);

                        PrintWarning("Saved DMArenas data in: " + watch.Elapsed.TotalMilliseconds + "ms");
                    }

                    watch.Stop();
                   
                }
                finally { Pool.Free(ref watch); }

                PrintWarning("Finished all data saving, now waiting: " + interval + " until we save again");
                yield return CoroutineEx.waitForSecondsRealtime(interval);
            }


        }

        private float LastDMTime(string userID)
        {
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));

            string lastTimeStr;
            var lastTime = -1f;
            if (dmData.lastDMTime.TryGetValue(userID, out lastTimeStr)) float.TryParse(lastTimeStr, out lastTime);
            return lastTime;
        }

        private static string GetVectorString(Vector3 vector) { return vector.ToString().Replace("(", string.Empty).Replace(")", string.Empty); }

        private T GetConfig<T>(string name, T defaultValue) { return (Config[name] == null) ? defaultValue : (T)Convert.ChangeType(Config[name], typeof(T)); }

        private List<BasePlayer> GetPlayersInZone(string zoneID)
        {
            if (string.IsNullOrEmpty(zoneID)) throw new ArgumentNullException(nameof(zoneID));
            var count = BasePlayer.activePlayerList?.Count ?? 0;
            if (count < 1) return null;

            var players = new List<BasePlayer>(count);
            for (int i = 0; i < count; i++)
            {
                var p = BasePlayer.activePlayerList[i];
                if (isPlayerInZone(p, zoneID)) players.Add(p);
            }
            return players;
            //return BasePlayer.activePlayerList?.Where(p => isPlayerInZone(p, zoneID))?.ToList() ?? null; 
        }

        private static void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

        private static void StopHostile(BasePlayer player)
        {
            if (player == null) return;

            player.State.unHostileTimestamp = TimeEx.currentTimestamp;
            player.ClientRPCPlayer(null, player, "SetHostileLength", 0);
        }

        private static void ClearTeam(BasePlayer player)
        {
            if (player == null || player.currentTeam == 0) return;
            var team = RelationshipManager.ServerInstance?.FindTeam(player.currentTeam) ?? null;
            if (team != null) team.RemovePlayer(player.userID);

            player.ClearTeam();
        }

        private static void HealPlayer(BasePlayer player, float health = 100)
        {
            if (player == null || player.IsDead() || player.metabolism == null) return;
            player.Heal(health);
            player.metabolism.calories.Add(500);
            player.metabolism.hydration.Add(500);
            player.metabolism.bleeding.Subtract(100);
        }

        private void SetHealth(BasePlayer player, float health)
        {
            if (player == null || player.IsDead() || player.metabolism == null) return;
            player.metabolism.bleeding.Subtract(25); //if there's bleeding, reduce it a bit so they don't immediately die
            player.metabolism.temperature.Add(15); //add warmth so they don't die immediately to cold
            player.Hurt((player?.Health() ?? 2) - 1); //pretty sure changing health directly is not possible without re-init
            player.Heal(health);
        }

        private void SendMessageToDMPlayers(string msg, string zoneID)
        {
            if (string.IsNullOrEmpty(msg)) throw new ArgumentNullException(nameof(msg));
            if (string.IsNullOrEmpty(zoneID)) throw new ArgumentNullException(nameof(zoneID));
            if (Arenas == null || Arenas.Count < 1) return;
            for (int i = 0; i < Arenas.Count; i++)
            {
                var arena = Arenas[i];
                if (arena?.ZoneID == zoneID)
                {
                    if (arena?.ActivePlayers != null && arena.ActivePlayers.Count > 0)
                    {
                        foreach (var ply in arena.ActivePlayers) SendReply(ply, "DM: " + msg);
                    }
                    break;
                }
            }
        }

        private void OnExplosiveThrown(BasePlayer player, BaseEntity entity)
        {
            if (player == null || entity == null) return;
            var grenade = entity?.GetComponent<TimedExplosive>() ?? null;
            var isGrenade = (entity?.ShortPrefabName ?? string.Empty).Contains("grenade");
            if (!isGrenade || grenade == null) return;
            if (!InAnyDM(player.UserIDString)) return;
            var rngRadiusMult = UnityEngine.Random.Range(1.5f, 2f);
            grenade.explosionRadius *= rngRadiusMult;
            grenade.minExplosionRadius *= rngRadiusMult;
            var fuseTime = 0f;
            var grenadeItem = entity?.GetItem() ?? player?.GetActiveItem() ?? null;
            if (grenadeItem != null)
            {
                if (grenadeCook.TryGetValue(grenadeItem.uid, out fuseTime)) grenadeCook.Remove(grenadeItem.uid);
                else fuseTime = 5f;
            }
            // if (grenadeItem != null && !grenadeCook.TryGetValue(grenadeItem.uid, out fuseTime)) fuseTime = 5f;
            grenade.SetFuse(fuseTime);
            var newVelocity = Vector3.zero;
            if (player.IsRunning() || player.serverInput.IsDown(BUTTON.SPRINT) || player.serverInput.WasDown(BUTTON.SPRINT)) newVelocity = player.eyes.HeadForward() * 12f;
            if (!player.IsOnGround() || player.serverInput.WasJustPressed(BUTTON.JUMP)) newVelocity += player.eyes.HeadForward() * UnityEngine.Random.Range(4.8f, 5.2f);
            if (newVelocity != Vector3.zero)
            {
                grenade.SetVelocity(newVelocity);
            }
            for (int i = 0; i < grenade.damageTypes.Count; i++) grenade.damageTypes[i].amount *= 1.3f;
        }

        private int GetFreePosition(ItemContainer container, int startPos = 0)
        {
            if (container == null || startPos < 0 || startPos > (container?.capacity ?? 0)) return -1;
            try
            {
                for (int i = startPos; i < container.capacity; i++)
                {
                    var slot = container?.GetSlot(i) ?? null;
                    if (slot == null) return i;
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
                PrintError("Failed to complete GetFreePosition");
            }
            return -1;
        }

        private bool TryMoveItem(Item item, ItemContainer container, int targetPos = -1)
        {
            if (item == null || container == null) return false;
            return item.MoveToContainer(container, targetPos);
        }

        /// <summary>
        /// Attempts to wake player up, if repeat is true; keeps trying every 0.5 seconds (or specified time) until player is awake, disconnected, dead or null
        /// </summary>
        private void WakePlayerUp(BasePlayer player, bool repeat = false, float interval = 0.5f)
        {
            if (player == null || !player.IsConnected || !player.IsSleeping() || player.IsDead()) return;
            if (!repeat) player.EndSleeping();
            else
            {
                if (!(player?.IsReceivingSnapshot ?? false))
                {
                    player.EndSleeping();
                    return;
                }
                player.Invoke(() =>
                {
                    if (player == null || !player.IsConnected || player.IsDead() || !player.IsSleeping()) return;
                    if (player?.IsReceivingSnapshot ?? false) WakePlayerUp(player, true, interval);
                    else player.EndSleeping();
                }, interval);
            }
        }

        #endregion

        #region ExternalCalls
        private bool isPlayerInZone(BasePlayer player, string zoneID)
        {
            if (player == null || string.IsNullOrEmpty(zoneID)) return false;
            return ZoneManager?.Call<bool>("isPlayerInZone", zoneID, player) ?? false;
        }

        private bool isEntityInZone(BaseEntity entity, string zoneID)
        {
            if (entity == null || string.IsNullOrEmpty(zoneID) || ZoneManager == null) return false;
            return ZoneManager?.Call<bool>("isEntityInZone", zoneID, entity) ?? false;
        }

        private string GetPlayerZone(BasePlayer player) { return ZoneManager?.Call<string>("GetPlayerZone", player) ?? string.Empty; }

        private bool EntityHasZoneFlag(BaseEntity entity, string flag)
        {
            if (entity == null || string.IsNullOrEmpty(flag)) return false;
            return ZoneManager?.Call<bool>("EntityHasFlag", entity, flag) ?? false;
        }

        private Vector3 GetZonePosition(string zoneID)
        {
            if (string.IsNullOrEmpty(zoneID) || ZoneManager == null) return Vector3.zero;
            return ZoneManager?.Call<Vector3>("GetZonePosition", zoneID) ?? Vector3.zero;
        }

        private float GetZoneRadius(string zoneID)
        {
            if (string.IsNullOrEmpty(zoneID) || ZoneManager == null) return -1f;
            var obj = ZoneManager?.Call<object>("GetZoneRadius", zoneID) ?? null;
            if (obj == null) return -1f;
            else return (float)obj;
        }

        private bool TeleportPlayer(BasePlayer player, Vector3 dest, bool distChecks = true, bool doSleep = true, bool respawnIfDead = false, int repeatTeleports = 0)
        {
            try
            {
                if (player == null || player?.gameObject == null || player?.transform == null)
                {
                    PrintWarning("tele player called on null player/transform");
                    return false;
                }
                var playerPos = player?.transform?.position ?? Vector3.zero;
                var isConnected = player?.IsConnected ?? false;
                if (player.IsDead() && respawnIfDead)
                {
                    var rotation = player?.transform?.rotation ?? default(Quaternion);
                    player.RespawnAt(dest, rotation);
                    PrintWarning("is dead, do respawn for death");
                    return true;
                }
                var distFrom = Vector3.Distance(playerPos, dest);

                if (distFrom >= 250 && isConnected && distChecks) player.ClientRPCPlayer(null, player, "StartLoading");
                if (doSleep && isConnected) player.SetPlayerFlag(BasePlayer.PlayerFlags.Sleeping, true);
                if (!BasePlayer.sleepingPlayerList.Contains(player) && doSleep) BasePlayer.sleepingPlayerList.Add(player);
                player.MovePosition(dest);
                if (isConnected)
                {
                    if (doSleep && IsInvoking("InventoryUpdate", player)) CancelInvoke("InventoryUpdate", player);
                    if (doSleep) player.inventory.crafting.CancelAll(true);
                    player.ClientRPCPlayer(null, player, "ForcePositionTo", dest);
                    if (distFrom >= 250 && distChecks) player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
                    player.UpdateNetworkGroup();
                    player.SendNetworkUpdate();
                    try { player?.ClearEntityQueue(null); } catch { }
                    player.SendFullSnapshot();
                }
                if (repeatTeleports > 0)
                {
                    if (repeatTeleports > 5) repeatTeleports = 5;
                    var act = new Action(() =>
                    {
                        if (player == null || player?.gameObject == null || player.IsDestroyed || player.IsDead()) return;
                        TeleportPlayer(player, dest, distChecks, doSleep, respawnIfDead);
                    });
                    var time = 0.2f;
                    for (int i = 0; i < 5; i++)
                    {
                        var add = i > 0 ? i * 0.05f : 0;
                        InvokeHandler.Invoke(player, act, time + add);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString() + Environment.NewLine + "^Failed to complete TeleportPlayer^");
                return false;
            }
        }
        #endregion

        #region ChatCommands

        private void DoStrike(Vector3 coords, ulong OwnerID = 0, StrikeType type = StrikeType.Rockets, int speed = -1, float dmgScalar = 1f, float radiusScalar = 1f)
        {
            if (coords == Vector3.zero) return;
            if (type == StrikeType.Rockets) AttackPlanes?.Call("massStrike", coords, OwnerID, type.ToString(), speed, dmgScalar, radiusScalar);
            else
            {
                if (type == StrikeType.Nuke) AttackPlanes?.Call("DoNuke", coords, new Vector3(0, UnityEngine.Random.Range(210, 340), 0), true, 150, OwnerID, 150f, 0f, 0f, false, 0f, 0f);
                if (type == StrikeType.Cannons)
                {
                    AttackPlanes?.Call("DoCannons", coords, new Vector3(0, coords.y + 120, 0), true, 230, OwnerID, UnityEngine.Random.Range(16, 24), 0.1f, 3f, 6.5f);
                    AttackPlanes?.Call("DoCannons", coords, new Vector3(-70, coords.y + 115, 80), true, 225, OwnerID, UnityEngine.Random.Range(16, 24), 0.1f, 2f, 4f);
                    AttackPlanes?.Call("DoCannons", coords, new Vector3(70, coords.y + 110, 80), true, 220, OwnerID, UnityEngine.Random.Range(16, 24), 0.1f, 5f, 8f);
                }
            }
        }

        private object CanSendDeathNotice(BasePlayer target, BaseEntity attacker, BaseCombatEntity victim)
        {
            if (target == null || !(target?.IsConnected ?? false)) return null;
            var attackPly = attacker as BasePlayer;
            var vicPly = victim as BasePlayer;
            var targDM = GetPlayerDM(target);
            var atkDM = GetPlayerDM(attackPly);
            var vicDM = GetPlayerDM(vicPly);
            if (targDM == null && (atkDM != null || vicDM != null)) return false;
            if (targDM != null)
            {
                if (atkDM == targDM || vicDM == targDM) return null;
                else if (atkDM == null || vicDM == null) return null;
                else return false;
            }
            return null;
        }

        private string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return covalence.Players.FindPlayerById(userID)?.Name ?? string.Empty;
        }

        [ConsoleCommand("dm.purge")]
        private void ccmdDMPurge(ConsoleSystem.Arg arg)
        {
            var stopw = new Stopwatch();
            stopw.Start();
            if (arg?.Connection != null && !(arg?.Player()?.IsAdmin ?? false))
            {
                SendReply(arg, "No perms");
                return;
            }
            var arenaList = Arenas.ToList();
            if (arenaList == null || arenaList.Count < 1)
            {
                SendReply(arg, "No arenas");
                return;
            }
            if (Playtimes == null || !Playtimes.IsLoaded)
            {
                SendReply(arg, "No playtimes!");
                return;
            }
            var lastConnections = new Dictionary<string, DateTime>();
            var noCon = 0;
            foreach (var p in covalence.Players.All)
            {
                if (p == null) continue;
                var pConnection = Playtimes?.Call<DateTime>("GetLastConnection", p.Id) ?? DateTime.MinValue;
                if (pConnection <= DateTime.MinValue)
                {
                    noCon++;
                    continue;
                }
                lastConnections[p.Id] = pConnection;
            }
            PrintWarning("No connection time count: " + noCon.ToString("N0"));
            var arenaSB = new StringBuilder();
            var arenaSB2 = new StringBuilder();
            DateTime lastOut;
            for (int i = 0; i < arenaList.Count; i++)
            {
                var arena = arenaList[i];
                if (arena == null) continue;
                var oldItemPos = new List<string>();
                foreach (var kvp in arena.playerItemPositions.ToDictionary(p => p.Key, p => p.Value))
                {
                    var del = false;
                    if (!lastConnections.TryGetValue(kvp.Key, out lastOut))
                    {
                        arenaSB2.AppendLine("No last connection for: " + kvp.Key);
                        del = true;
                    }
                    else
                    {
                        var days = (DateTime.Now - lastOut).TotalDays;
                        if (days >= 7)
                        {
                            arenaSB2.AppendLine("Total days >= 7 for: " + kvp.Key);
                            del = true;
                        }
                        else arenaSB2.AppendLine("Days < 7 for: " + kvp.Key + ", " + days);
                    }
                    if (del)
                    {
                        arena.playerItemPositions.Remove(kvp.Key);

                    }
                }
                //   var oldItemPos = arena?.playerItemPositions?.Where(p => !lastConnections.TryGetValue(p.Key, out lastOut) || (lastOut <= DateTime.MinValue || (DateTime.Now - lastOut).TotalDays >= 7))?.Select(p => p.Key)?.ToList() ?? null;
                var oldAmmos = arena?.playerAmmos?.Where(p => !lastConnections.TryGetValue(p.Key, out lastOut) || lastOut <= DateTime.MinValue || (DateTime.Now - lastOut).TotalDays >= 7)?.Select(p => p.Key)?.ToList() ?? null;
                var oldSkins = arena?.playerItemSkins?.Where(p => !lastConnections.TryGetValue(p.Key, out lastOut) || lastOut <= DateTime.MinValue || (DateTime.Now - lastOut).TotalDays >= 7)?.Select(p => p.Key)?.ToList() ?? null;
                var oldMods = arena?.playerModInfos?.Where(p => !lastConnections.TryGetValue(p.Key, out lastOut) || lastOut <= DateTime.MinValue || (DateTime.Now - lastOut).TotalDays >= 7)?.Select(p => p.Key)?.ToList() ?? null;
                //    if (oldItemPos != null && oldItemPos.Count > 0) for (int j = 0; j < oldItemPos.Count; j++) arena.playerItemPositions.Remove(oldItemPos[j]);
                if (oldAmmos != null && oldAmmos.Count > 0) for (int j = 0; j < oldAmmos.Count; j++) arena.playerAmmos.Remove(oldAmmos[j]);
                if (oldSkins != null && oldSkins.Count > 0) for (int j = 0; j < oldSkins.Count; j++) arena.playerItemSkins.Remove(oldSkins[j]);
                if (oldMods != null && oldMods.Count > 0) for (int j = 0; j < oldMods.Count; j++) arena.playerModInfos.Remove(oldMods[j]);
                arenaSB.AppendLine("Counts for arena: " + arena.ArenaName + Environment.NewLine + ", itemPos: " + oldItemPos.Count + ", ammo: " + oldAmmos.Count + ", skins: " + oldSkins.Count + ", mods: " + oldMods.Count);
            }
            stopw.Stop();
            PrintWarning(arenaSB2.ToString().TrimEnd() + Environment.NewLine + arenaSB.ToString().TrimEnd());
            PrintWarning("Took: " + stopw.Elapsed.TotalSeconds + " seconds to cleanup file");
        }

        [ConsoleCommand("dm.active")]
        private void ccmdDMActive(ConsoleSystem.Arg arg)
        {
            var isAuth = arg?.Player()?.IsAdmin ?? false || arg?.Connection == null;
            if (!isAuth)
            {
                PrintWarning("Someone tried to use dm.active with no permission!");
                return;
            }
            var countSB = new StringBuilder();
            var playerSB = new StringBuilder();
            for (int i = 0; i < Arenas.Count; i++)
            {
                var arena = Arenas[i];
                var activePlayers = arena?.ActivePlayers ?? null;
                if ((activePlayers?.Count ?? 0) < 1) continue;
                countSB.AppendLine((arena?.ArenaName ?? string.Empty) + " has " + activePlayers.Count + " players");
                foreach (var ply in activePlayers) playerSB.Append((arena?.ArenaName ?? string.Empty) + ": " + (ply?.displayName ?? "Unknown") + ", ");
            }
            if (countSB.Length < 1)
            {
                SendReply(arg, "No players in any arenas!");
                return;
            }
            SendReply(arg, "Active players: " + playerSB.ToString().TrimEnd(", ".ToCharArray()));
            SendReply(arg, "Active player count: " + countSB.ToString().TrimEnd());
        }

        [ConsoleCommand("dm.closegui")]
        private void ccmdDMClose(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null || !player.IsConnected) return;
            CuiHelper.DestroyUi(player, "DMGUI");
            var getDM = GetPlayerDM(player);
            if (getDM != null && getDM.GetTeam(player) == ArenaInfo.Team.None) getDM.LeaveDM(player);
        }

        [ConsoleCommand("dm.jointeam")]
        private void ccmdDMJoinTeam(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null || !player.IsConnected) return;
            if (arg.Args == null || arg.Args.Length < 1)
            {
                PrintWarning("no args for dm join team");
                return;
            }
            ArenaInfo arena;
            if (!teamJoin.TryGetValue(player.UserIDString, out arena)) return;
            if (arena == null || !arena.Teams)
            {
                PrintWarning("dm.jointeam called for null or non-team arena, player: " + player.displayName);
                return;
            }
            var lastChar = arg.Args.LastOrDefault();
            int teamInt;
            if (!int.TryParse(lastChar, out teamInt))
            {
                PrintWarning("not an int: " + lastChar);
                return;
            }
            var team = (ArenaInfo.Team)teamInt;
            if (team != ArenaInfo.Team.None && team == arena.GetTeam(player))
            {
                PrintWarning(player.displayName + " tried to join team they're already on: " + team);
                return;
            }
            var teamCount = arena.GetTeamCount(team);
            var altCount = arena.GetTeamCount((team == ArenaInfo.Team.Allies) ? ArenaInfo.Team.Axis : ArenaInfo.Team.Allies);
            if (teamCount > altCount)
            {
                SendReply(player, "Joining this team would leave it unbalanced. Try joining <color=#fcb355>" + (team == ArenaInfo.Team.Allies ? ArenaInfo.Team.Axis : ArenaInfo.Team.Allies) + "</color>");
                return;
            }

            arena.SetTeam(player, team);
            if (!arena.JoinDM(player)) SendReply(player, "Currently unable to join the arena!");
            else SendReply(player, "You've joined the <color=orange>" + team + "</color> team!");

            CuiHelper.DestroyUi(player, "DMGUI");
        }

        [ConsoleCommand("dm.autoteam")]
        private void ccmdDMJoinAuto(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player() ?? null;
            if (player == null || !player.IsConnected) return;
            ArenaInfo arena;
            if (!teamJoin.TryGetValue(player.UserIDString, out arena)) return;
            if (arena == null || !arena.Teams) return;
            var team = arena?.GetBestTeam() ?? ArenaInfo.Team.None;
            if (team == ArenaInfo.Team.None)
            {
                SendReply(player, "Failed to automatically find best team. Please join a team manually.");
                PrintWarning("Got bad team on auto assign!!!");
                return;
            }
            player.SendConsoleCommand("dm.jointeam", (int)team);
        }

        private void SendStrike(BasePlayer player, Vector3 pos = default(Vector3), bool isNuke = false)
        {
            if (player == null || !player.IsConnected || player.IsDead()) return;
            var strikePos = pos;
            var getDM = GetPlayerDM(player);
            if (getDM == null) return;
            if (strikePos == Vector3.zero) strikePos = player?.CenterPoint() ?? GetZonePosition(getDM.ZoneID);
            //       if (!CheckBoundaries(strikePos)) return;
            var UID = player?.userID ?? 0;
            timer.Once(UnityEngine.Random.Range(2f, 4.76f), () =>
            {
                SendMessageToDMPlayers("<color=#fcb355>" + (isNuke ? "Nuke" : "Airstrike") + "</color> <color=#a6fc60>incoming!</color>", getDM.ZoneID);
            });
            // StrikeType airType;
            //   if (!dmData.airstrikeTypes.TryGetValue(player.UserIDString, out airType)) airType = StrikeType.Rockets;
            DoStrike(strikePos, UID, isNuke ? StrikeType.Nuke : StrikeType.Rockets, 160, 10f, UnityEngine.Random.Range(4f, 9f));
            var airstrikeTxt = "<color=#ff1934>Called airstrike, take cover!</color>";
            SendReply(player, airstrikeTxt);
            ShowPopup(player.UserIDString, airstrikeTxt);
        }

        [ChatCommand("dmtg")]
        private void cmdDMTestGUI(BasePlayer player, string command, string[] args)
        {
            var getDM = GetPlayerDM(player);
            if (getDM == null || !getDM.Teams)
            {
                SendReply(player, "bad dm or not team based dm");
                return;
            }
            ScoreGUI(player, getDM, UnityEngine.Random.Range(0, 100), 3f);
        }

        [ChatCommand("dmjt")]
        private void cmdDMJT(BasePlayer player, string command, string[] args)
        {
            var getDM = GetPlayerDM(player);
            if (getDM == null) return;
            JoinGUI(player, getDM);
        }

        [ChatCommand("dmstorm")]
        private void cmdDMStorm(BasePlayer player, string command, string[] args)
        {
            var getDM = GetPlayerDM(player);
            if (getDM == null || !getDM.Fortnite) return;
            SendReply(player, "In storm: " + getDM.InStorm(player));
        }

        [ChatCommand("dmstart")]
        private void cmdDMStart(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var getDM = GetPlayerDM(player);
            if (getDM == null) return;
            if (!getDM.RoundBased)
            {
                SendReply(player, "DM must be round based");
                return;
            }
            else
            {
                var time = 0f;
                if (args.Length > 0)
                {
                    if (!float.TryParse(args[0], out time))
                    {
                        SendReply(player, "Not a float: " + args[0]);
                        return;
                    }
                }
                getDM.StartGame(time);
                SendReply(player, "Started DM");
            }
        }

        [ChatCommand("dmdt2")]
        private void cmdDMDT2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var getDM = GetPlayerDM(player);
            if (getDM == null) return;
            if (!getDM.Fortnite)
            {
                SendReply(player, "Bad DM mode");
                return;
            }
            var positions = getDM.ItemSpawns?.Select(p => p.Position)?.ToList() ?? null;
            var toSpawns = new List<ItemSpawn>();
            for (int i = 0; i < getDM.ItemSpawns.Count; i++)
            {
                var spawn = getDM.ItemSpawns[i];
                var rng = randomSpawn.Next(0, 101);
                if (rng <= (spawn.ChanceToSpawn * getDM.LootChanceScalar))
                {
                    PrintWarning("rng: " + rng + " <= chance: " + spawn.ChanceToSpawn + ", item: " + spawn.Item.shortName + " x" + spawn.Item.Amount);
                    toSpawns.Add(spawn);
                }
            }
            if (toSpawns == null || toSpawns.Count < 1)
            {
                SendReply(player, "No toSpawns ??!");
                PrintWarning("no tospawns means we must be spawning fucking nothing lol");
                return;
            }
            var findSame = toSpawns?.Select(p => toSpawns?.Where(x => (p?.Item?.GetDefinition()?.category != x?.Item?.GetDefinition()?.category) && Vector3.Distance(p.Position, x.Position) <= 0.2f)?.FirstOrDefault() ?? null)?.ToList() ?? null;
            findSame.RemoveAll(p => p == null);
            findSame.Distinct();
            var findNotSame = toSpawns?.Where(p => !findSame.Any(x => Vector3.Distance(x.Position, p.Position) <= 0.2f))?.ToList() ?? null;
            findNotSame.RemoveAll(p => p == null);
            findNotSame.Distinct();

            if (findSame != null)
            {
                PrintWarning("findSame: " + findSame.Count);
                var lowestChance = findSame?.Where(p => p.ChanceToSpawn > 0f)?.Min(p => p.ChanceToSpawn) ?? 0f;
                if (lowestChance == 0f)
                {
                    PrintWarning("got lowest chance of 0f!!!");
                    return;
                }
                var findUse = findSame?.Where(p => p.ChanceToSpawn <= lowestChance)?.FirstOrDefault() ?? null;
                if (findUse == null)
                {
                    PrintWarning("findUse is null for chance: " + lowestChance + " !!");
                    return;
                }
                //      PrintWarning("got finduse: " + findUse.ChanceToSpawn + "%, " + findUse.Position + ", " + findUse.Item.shortName);
                var item = ItemManager.CreateByName(findUse.Item.shortName, findUse.Item.Amount, findUse.Item.skinID);
                if (item == null)
                {
                    PrintWarning("item is null on findUse !!");
                    return;
                }
                if (!item.Drop(findUse.Position, Vector3.zero))
                {
                    PrintWarning("no drop item findUse at: " + findUse.Position + " !!");
                    RemoveFromWorld(item);
                    return;
                }
                else
                {
                    MakeItemFloat(item, findUse.Position);
                    //   PrintWarning("did spawn & float: " + item.info.shortname + " x" + item.amount);
                }
            }
            else PrintWarning("no find same");
            if (findNotSame != null && findNotSame.Count > 0)
            {
                for (int i = 0; i < findNotSame.Count; i++)
                {
                    var findUse = findNotSame[i];
                    if (findUse == null) continue;
                    PrintWarning("is in notSame: " + findUse.Item.shortName + " x" + findUse.Item.Amount + ", " + findUse.Position + ", " + findUse.ChanceToSpawn);
                    var item = ItemManager.CreateByName(findUse.Item.shortName, findUse.Item.Amount, findUse.Item.skinID);
                    if (item == null)
                    {
                        PrintWarning("item is null on findUse (not same) !!");
                        return;
                    }
                    if (!item.Drop(findUse.Position, Vector3.zero))
                    {
                        PrintWarning("no drop item findUse (not same) at: " + findUse.Position + " !!");
                        RemoveFromWorld(item);
                        return;
                    }
                    else
                    {
                        MakeItemFloat(item, findUse.Position);
                        PrintWarning("did spawn & float not same: " + item.info.shortname + " x" + item.amount);
                    }
                }
            }
            PrintWarning("findNotSame: " + (findNotSame?.Count ?? -1));
        }

        [ChatCommand("dmdt")]
        private void cmdDMDT(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var held = player?.GetActiveItem() ?? null;
            if (held == null)
            {
                SendReply(player, "No held");
                return;
            }
            var drop = MakeItemFloat(held, SpreadVector(player.transform.position, 3f));
            if (drop == null)
            {
                SendReply(player, "Drop is null after float attempt!!");
                return;
            }
            timer.Once(1f, () =>
            {
                if (drop == null || drop.gameObject == null)
                {
                    if (player != null && player.IsConnected) SendReply(player, "Dropped item or gameObject is null after 1 second");
                    return;
                }
                var sphere = drop.gameObject.AddComponent<SphereTrigger>();
                sphere.Radius = 0.2f;
                sphere.name = "ammo";
                SendReply(player, "Added sphere");
            });

        }

        [ChatCommand("dmst")]
        private void cmdDMSpecTest(BasePlayer player, string command, string[] args)
        {
            var specFilter = player.spectateFilter;
            SendReply(player, "Spec filter: " + specFilter);
            if (args.Length < 1)
            {
                if (player.IsSpectating())
                {
                    StopSpectatePlayer(player);
                    SendReply(player, "stop spec");
                }
                return;
            }
            var target = FindPlayerByPartialName(args[0]);
            if (target == null)
            {
                SendReply(player, "No player found with the name: " + args[0]);
                return;
            }
            SpectatePlayer(player, target);
            SendReply(player, "start spec: " + target.displayName);
        }

        [ChatCommand("dmlb")]
        private void cmdDMBox(BasePlayer player, string command, string[] args)
        {
            RaycastHit info;
            var pos = Vector3.zero;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            if (Physics.Raycast(new Ray(player.eyes.position, currentRot), out info, 30f))
            {
                pos = info.point;
            }
            if (pos == Vector3.zero)
            {
                SendReply(player, "No pos");
                return;
            }
            var box = (LootContainer)GameManager.server.CreateEntity(tier3BoxPrefab, pos);
            if (box == null)
            {
                SendReply(player, "Box null");
                return;
            }
            box.Spawn();
            NextTick(() =>
            {
                //     EnsureWeapon(box);   
            });
        }

        private readonly Dictionary<string, BaseEntity> flagEnt = new Dictionary<string, BaseEntity>();
        private readonly Dictionary<string, CTFFlag> flagCarrier = new Dictionary<string, CTFFlag>();
        [ChatCommand("ctftest")]
        private void cmdCtfTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var newFlag = new CTFFlag
            {
                prefabName = "assets/prefabs/deployable/signs/sign.large.wood.prefab",
                StartPosition = new Vector3(player.eyes.position.x, player.eyes.position.y + 0.75f, player.eyes.position.z)
            };
            newFlag.Position = newFlag.StartPosition;
            var ent = newFlag.SpawnEntity();
            PrintWarning("Ent?: " + !(ent?.IsDestroyed ?? false) + ", pos: " + newFlag.Position);
            timer.Once(0.1f, () => SendReply(player, newFlag.Position + " <--"));
            ent.SetParent(player, "head");
            ent.transform.localPosition = new Vector3(0, 0.85f, -0.125f);
            ent.transform.localEulerAngles = new Vector3(0, 0, 0);
            ent.SendNetworkUpdateImmediate();
            var oldPos = Vector3.zero;
            flagCarrier[player.UserIDString] = newFlag;
        }

        [ChatCommand("ctftest2")]
        private void cmdCtfTest2(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var newFlag = new CTFFlag
            {
                prefabName = "assets/prefabs/deployable/signs/sign.post.single.prefab",
                StartPosition = player.transform.position
            };
            newFlag.Position = newFlag.StartPosition;
            var ent = newFlag.SpawnEntity();
            PrintWarning("Ent?: " + !(ent?.IsDestroyed ?? false) + ", pos: " + newFlag.Position);
            timer.Once(0.1f, () => SendReply(player, newFlag.Position + " <--"));

        }

        private void OnFlagDropped(CTFFlag flag)
        {
            if (flag == null || flag.GetEntity() == null) return;
            var flagEnt = flag.GetEntity();
            PrintWarning("get ent");
            //  CreateCTFZone(flag);
            PrintWarning("did zone: " + CreateCTFZone(flag));
            InvokeHandler.Invoke(flagEnt, () =>
            {
                PrintWarning("INVOKE");
                if (flagEnt == null || flagEnt.IsDestroyed) return;
                if (flag.StartPosition == Vector3.zero) return;
                flagEnt.transform.localPosition = Vector3.zero;
                flagEnt.transform.position = flag.StartPosition;
                flagEnt.transform.eulerAngles = flag.StartRotation;
                flagEnt.SendNetworkUpdateImmediate();
                PrintWarning("Flag returned to start pos: " + flag.StartPosition + ", " + flagEnt.transform.position);
                timer.Once(2f, () =>
                {
                    PrintWarning("flagent pos now: " + (flagEnt?.transform?.position ?? Vector3.zero));
                });
                PrintWarning("Destroy zone?: " + DestroyCTFZone(flag));
            }, 15f);
            PrintWarning("OnFlagDropped");
        }

        private readonly Dictionary<CTFFlag, string> flagZoneName = new Dictionary<CTFFlag, string>();

        private bool CreateCTFZone(CTFFlag flag, float radius = 4f)
        {
            if (flag == null || radius <= 0.0f) return false;
            if (flag.GetEntity() == null) return false;
            PrintWarning("start ctf after checks");
            var pos = flag.Position;
            PrintWarning("get pos");
            if (pos == Vector3.zero) return false;
            PrintWarning("Past pos check");
            string[] zoneArgs = { "radius", radius.ToString("0.00") };
            PrintWarning("args");
            var zoneName = "ctf-" + (flag?.GetEntity()?.net?.ID.Value ?? 0);
            PrintWarning("name: " + zoneName);
            flagZoneName[flag] = zoneName;
            PrintWarning("flag zone name set");
            return ZoneManager?.Call<bool>("CreateOrUpdateZone", zoneName, zoneArgs, pos) ?? false;
            //    return false;
        }

        private bool DestroyCTFZone(CTFFlag flag)
        {
            if (flag == null) throw new ArgumentNullException();
            string name;
            if (!flagZoneName.TryGetValue(flag, out name) || string.IsNullOrEmpty(name)) return false;
            return ZoneManager?.Call<bool>("EraseZone", name) ?? false;
        }

        [ChatCommand("dmn")]
        private void cmdDMNuke(BasePlayer player, string command, string[] args)
        {
            if (!InAnyDM(player.UserIDString))
            {
                SendReply(player, "You must be in a Deathmatch arena to use this!");
                return;
            }
            player.SendConsoleCommand("dm.nuke");
            //    player.SendConsoleCommand("dm.strike nuke");
        }

        /*/
        [ChatCommand("dma")]
        private void cmdDMA(BasePlayer player, string command, string[] args)
        {
            StrikeType airType;
            if (!dmData.airstrikeTypes.TryGetValue(player.UserIDString, out airType)) dmData.airstrikeTypes[player.UserIDString] = StrikeType.Rockets;
            dmData.airstrikeTypes[player.UserIDString] = (airType == StrikeType.Rockets) ? StrikeType.Cannons : StrikeType.Rockets;
            airType = dmData.airstrikeTypes[player.UserIDString];
            SendReply(player, "Set type to: " + airType);
        }/*/
        private readonly Dictionary<ulong, HashSet<NetworkableId>> netDestroyed = new Dictionary<ulong, HashSet<NetworkableId>>();

        private object CanNetworkTo(BaseNetworkable entity, BasePlayer target)
        {
            if (entity == null || target == null) return null;
            var entPly = entity as BasePlayer;
            if (entPly != null && target.UserIDString != entPly.UserIDString && entPly.IsSpectating())
            {
                DestroyClientEntity(target, entPly, true);
                return false;
            }

            if (entity is BasicCar || entity is CargoPlane || entity is CH47HelicopterAIController) return null;


            if (target != null && ((entity as BasePlayer) == target) && target.IsSleeping() && target.IsSpectating())
            {
                PrintWarning("target != null && target is sleeping && spectating, and entity as baseplayer == target");
                DestroyClientEntity(target, entity as BaseEntity, true);
                return false;
            }
            var ply = entity as BasePlayer;
            /*/
          
            if (ply != null)
            {
                var getDM = GetPlayerDM(ply);
                var targDM = GetPlayerDM(target);
                if (targDM != null && targDM != getDM || (targDM == null && getDM != null))
                {
                    DestroyClientEntity(target, ply, true);
                    return false;
                }
            }/*/
            /*/
            if (!target.IsAdmin)
            {
             
            }/*/
            var baseEnt = entity as BaseEntity;


            ArenaInfo targetDM = null;
            var sphere = entity as SphereEntity;
            if (sphere != null)
            {
                var dist = Vector3.Distance(target?.transform?.position ?? Vector3.zero, entity?.transform?.position ?? Vector3.zero);
                if (dist > 250) return false;
                targetDM = GetPlayerDM(target);
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena == null || !arena.Fortnite || arena?.Sphere?.Spheres == null) continue;
                    if ((arena.Sphere.Sphere == sphere || arena.Sphere.Spheres.Contains(sphere)) && (targetDM == null || targetDM != arena)) return false;
                }
            }
            if (targetDM == null) targetDM = GetPlayerDM(target);
            /*/
            var ch47 = entity as CH47HelicopterAIController;
            if (targetDM != null && ch47 != null && ch47.OwnerID == 757 && !targetDM.CH47s.Contains(ch47))
            {
                DestroyClientEntity(target, ch47, true);
                return false;
            }/*/
            /*/
            if (entity is DecayEntity || entity is StabilityEntity || entity is Signage || entity is BaseLadder)
            {
              //  var baseEnt = entity as BaseEntity;
                var blockDM = GetEntityArena(baseEnt);
                if (blockDM != null)
                {
                    if (blockDM.NetworkOut || (blockDM.AdminNetAlways && target.IsAdmin))
                    {
                       // if (target.IsAdmin) PrintWarning("Rendering: " + entity.ShortPrefabName + ", for: " + target.displayName + ", networkout: " + blockDM.NetworkOut + ", adminnetalways: " + blockDM.AdminNetAlways);
                        return null;
                    }
                    else
                    {
                        if (targetDM == null || targetDM != blockDM)
                        {
                       //     if (target.IsAdmin) PrintWarning("NOt allowing render: " + target.displayName + ": " + entity.ShortPrefabName);
                            DestroyClientEntity(target, baseEnt, true);
                            return false;
                        }
                    }
                }
            }/*/
            var marker = entity as MapMarkerGenericRadius;
            if (marker != null)
            {
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena == null || !arena.Fortnite || arena?.Sphere?.Marker == null) continue;
                    if (arena.Sphere.Marker == marker && (targetDM == null || targetDM != arena)) return false;
                }
            }

            var env = entity as EnvSync;
            /*/
            if (baseEnt != null && ply == null && env == null)
            {
                var targDM = GetPlayerDM(target);
                if (targDM != null && !string.IsNullOrEmpty(targDM.ZoneID) && !isEntityInZone(baseEnt, targDM.ZoneID)) return false;
            }/*/

         

            if (env != null && targetDM != null && (targetDM.Fortnite || targetDM.ForceDay || targetDM.ForceNight))
            {
                var fortnite = targetDM.Fortnite;
                var connection = target?.net?.connection ?? null;

                if (!fortnite && (targetDM.ForceDay || targetDM.ForceNight) && connection != null)
                {
                    var vars = Pool.GetList<ClientVarWithValue>();
                    
                    try 
                    {
                        GetClearWeatherVarsNoAlloc(ref vars);
                        SendClientVars(connection, vars); 
                    }
                    finally { Pool.FreeList(ref vars); }
                   
                }
               
                

                if (connection != null)
                {
                    var netWrite = Net.sv.StartWrite();

                    connection.validate.entityUpdates += 1;
                    BaseNetworkable.SaveInfo saveInfo = new BaseNetworkable.SaveInfo
                    {
                        forConnection = connection,
                        forDisk = false
                    };

                    netWrite.PacketID(Message.Type.Entities);
                    netWrite.UInt32(target.net.connection.validate.entityUpdates);
                    using (saveInfo.msg = Pool.Get<ProtoBuf.Entity>())
                    {
                        env.Save(saveInfo);
                        if (saveInfo.msg.baseEntity == null) PrintError(this + ": ToStream - no BaseEntity!?");

                        if (fortnite)
                        {
                            var inStorm = targetDM?.InStorm(target) ?? false;
                            saveInfo.msg.environment.dateTime = new DateTime().AddHours(Mathf.Clamp(TOD_Sky.Instance.Cycle.Hour, 7.75f, 17.5f)).ToBinary();
                            if (inStorm)
                            {
                                saveInfo.msg.environment.fog = 1;
                                saveInfo.msg.environment.rain = 1;
                                saveInfo.msg.environment.clouds = 1;
                                saveInfo.msg.environment.wind = 1;
                            }
                            else
                            {
                                saveInfo.msg.environment.fog = Climate.GetFog(Vector3.zero);
                                saveInfo.msg.environment.rain = Climate.GetRain(Vector3.zero);
                                saveInfo.msg.environment.clouds = Climate.GetClouds(Vector3.zero);
                                saveInfo.msg.environment.wind = Climate.GetWind(Vector3.zero);
                            }

                        }
                        else if (targetDM.ForceDay || targetDM.ForceNight)
                        {
                            saveInfo.msg.environment.dateTime = targetDM.ForceDay ? 432000000000 : 120000000; //12 sharp (432...)!




                            /*/
                            saveInfo.msg.environment.fog = 0;
                            saveInfo.msg.environment.rain = 0;
                            saveInfo.msg.environment.clouds = 0;
                            saveInfo.msg.environment.wind = 0;/*/
                        }

                        if (saveInfo.msg.baseNetworkable == null) PrintError(this + ": ToStream - no baseNetworkable!?");

                        saveInfo.msg.ToProto(netWrite);
                        env.PostSave(saveInfo);
                        netWrite.Send(new SendInfo(connection));
                    }
                }
            
                return false;
            }
            if (env != null) return null;
            var prefabName = entity?.ShortPrefabName ?? string.Empty;
            if (prefabName.Contains("rocket") && !prefabName.Contains("launch") && !prefabName.Contains("heli"))
            {
                // var baseEnt = entity?.GetComponent<BaseEntity>() ?? null;
                if (baseEnt != null)
                {
                    var ownerID = baseEnt?.OwnerID ?? 0;
                    if (ownerID == 0) return null;
                    var player = BasePlayer.FindByID(ownerID) ?? BasePlayer.FindSleeping(ownerID);
                    if (player != null)
                    {
                        var ownerDM = GetPlayerDM(player);
                        if (targetDM == null || ownerDM != targetDM)
                        {
                            // DestroyClientEntity(target, baseEnt);
                            return false;
                        }
                    }
                }
            }
            var plane = entity as CargoPlane;
            if (plane != null && plane.OwnerID != 0)
            {
                var planePly = BasePlayer.FindByID(plane.OwnerID) ?? BasePlayer.FindSleeping(plane.OwnerID) ?? null;
                if (planePly != null)
                {
                    var planeDM = GetPlayerDM(planePly);
                    if (targetDM == null || planeDM != targetDM)
                    {
                        //      DestroyClientEntity(target, plane);
                        return false;
                    }
                }
            }
            if (strikeFoundation != null && !(strikeFoundation?.IsDestroyed ?? true))
            {
                var foundation = entity as BuildingBlock;
                if (foundation != null && strikeFoundation == foundation) return false;
            }
            if (ply != null)
            {
                var plyDM = GetPlayerDM(ply);
                if ((targetDM != null && targetDM.LocalArena && targetDM != plyDM) || (plyDM != null && plyDM.LocalArena && targetDM != plyDM)) return false;
                if (plyDM != null && plyDM.LocalArena && target != null && !plyDM.ActivePlayers.Contains(target)) return false;
            }

            if (targetDM != null && targetDM.LocalArena)
            {
                if (ply != null && !targetDM.ActivePlayers.Contains(ply)) return false;
                var baseEntity = entity as BaseEntity;
                if (baseEntity == null || baseEntity.OwnerID == target.userID) return null;
                if (baseEntity.OwnerID != 0 && !targetDM.ActivePlayers.Any(p => p != null && p.userID == baseEntity.OwnerID)) return false;
            }
            return null;
        }

        private float GetLastStrikeTime(BasePlayer player)
        {
            if (player == null) return -1;
            float lastTime;
            if (lastStrikeTime.TryGetValue(player.userID, out lastTime)) lastTime = Time.realtimeSinceStartup - lastStrikeTime[player.userID];
            else lastTime = -1f;
            return lastTime;
        }

        private float GetTimeAlive(BasePlayer player)
        {
            float time = -1f;
            try
            {
                time = (player != null && player.gameObject != null && !player.IsNpc && !player.IsDestroyed && !player.IsDead() && player.lifeStory != null) ? player.TimeAlive() : -1f;
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return time;
        }


        [ConsoleCommand("dm.strike")]
        private void ccmdDMStrike(ConsoleSystem.Arg arg)
        {
            if (arg.Connection == null) return;
            var player = arg?.Player() ?? null;
            if (player == null)
            {
                SendReply(arg, "You must be in-game to use this command!");
                return;
            }
            if (!player.IsAlive() || player.IsDead() || !player.IsConnected) return;
            var getDM = GetPlayerDM(player);
            if (getDM == null) return;
            if (!player.IsAdmin)
            {
                var saInt = 0;
                if (!strikeAmounts.TryGetValue(player.userID, out saInt) || saInt < 1) return;
            }
            var lastStrike = GetLastStrikeTime(player);
            if (lastStrike < 5 && lastStrike != -1f)
            {
                SendReply(player, "You must wait before using this again!");
                return;
            }


            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new Ray(player.eyes.position, currentRot);
            RaycastHit hitt;


            if (Physics.Raycast(ray, out hitt, 200f, _groundWorldConstLayer))
            {
                var hitPos = hitt.point;
                var adjustedhitPos = hitPos;
                adjustedhitPos.y -= 5;
                var highHitPos = hitPos;
                highHitPos.y += 5;
                var foundation = strikeFoundation = (BuildingBlock)GameManager.server.CreateEntity("assets/prefabs/building core/foundation/foundation.prefab", hitPos, Quaternion.identity);
                foundation.enableSaving = false;
                foundation.Spawn();
                InvokeHandler.Invoke(player, () =>
                {
                    if (foundation == null || (foundation?.IsDestroyed ?? false) || player == null || !(player?.IsConnected ?? false)) return;
                    var inZone = isEntityInZone(foundation, getDM.ZoneID);
                    if (!inZone) SendReply(player, "Invalid area marked! (outside arena)");
                    else
                    {
                        player.SendConsoleCommand("ddraw.arrow", 7f, Color.yellow, highHitPos, adjustedhitPos, 5);
                        //  var isNuke = arg?.Args != null && arg.Args.Length > 0 && arg.Args[0].ToLower() == "nuke";
                        SendStrike(player, hitPos);
                        var strkAmt = 0;
                        if (!strikeAmounts.TryGetValue(player.userID, out strkAmt)) strikeAmounts[player.userID] = 0;
                        else strikeAmounts[player.userID] -= 1;
                        lastStrikeTime[player.userID] = Time.realtimeSinceStartup;
                    }
                    InvokeHandler.Invoke(player, () =>
                    {
                        strikeFoundation = null;
                        if (foundation == null || (foundation?.IsDestroyed ?? true)) return;
                        foundation.Kill();
                    }, 0.001f);
                }, 0.08f);
            }
            else SendReply(player, "Invalid area marked! (out of range)");
        }

        private readonly HashSet<ulong> HasNuke = new HashSet<ulong>();
        [ConsoleCommand("dm.nuke")]
        private void ccmdDMStrikeNuke(ConsoleSystem.Arg arg)
        {
            if (arg.Connection == null) return;
            var player = arg?.Player() ?? null;
            if (player == null)
            {
                SendReply(arg, "You must be in-game to use this command!");
                return;
            }
            if (!player.IsAlive() || player.IsDead() || !player.IsConnected) return;
            var getDM = GetPlayerDM(player);
            if (getDM == null) return;
            if (!player.IsAdmin && !HasNuke.Remove(player.userID))
            {
                SendReply(player, "You have not earned a nuke!");
                return;
            }

            var lastStrike = GetLastStrikeTime(player);
            if (lastStrike < 5 && lastStrike != -1f)
            {
                SendReply(player, "You must wait before using this again!");
                return;
            }


            var input = player?.serverInput ?? null;
            var currentRot = Quaternion.Euler(input.current.aimAngles) * Vector3.forward;


            Ray ray = new Ray(player.eyes.position, currentRot);
            RaycastHit hitt;


            if (Physics.Raycast(ray, out hitt, 220f, _groundWorldConstLayer))
            {
                var hitPos = hitt.point;
                var adjustedhitPos = hitPos;
                adjustedhitPos.y -= 5;
                var highHitPos = hitPos;
                highHitPos.y += 5;
                var foundation = strikeFoundation = (BuildingBlock)GameManager.server.CreateEntity("assets/prefabs/building core/foundation/foundation.prefab", hitPos, Quaternion.identity);
                foundation.Spawn();
                InvokeHandler.Invoke(player, () =>
                {
                    if (foundation == null || (foundation?.IsDestroyed ?? false) || player == null || !(player?.IsConnected ?? false)) return;
                    var inZone = isEntityInZone(foundation, getDM.ZoneID);
                    if (!inZone) SendReply(player, "Invalid area marked! (outside arena)");
                    else
                    {
                        player.SendConsoleCommand("ddraw.arrow", 7f, Color.yellow, highHitPos, adjustedhitPos, 5);
                        SendStrike(player, hitPos, true);
                        lastStrikeTime[player.userID] = Time.realtimeSinceStartup;
                        SendReply(player, "Nuclear strike confirmed!");
                    }
                    InvokeHandler.Invoke(player, () =>
                    {
                        strikeFoundation = null;
                        if (foundation == null || (foundation?.IsDestroyed ?? true)) return;
                        foundation.Kill();
                    }, 0.001f);
                }, 0.08f);
            }
            else SendReply(player, "Invalid area marked! (out of range)");
        }

        private readonly HashSet<NetworkableId> immuneEntities = new HashSet<NetworkableId>();
        private readonly Dictionary<NetworkableId, Timer> immuneTimers = new Dictionary<NetworkableId, Timer>();

        private void GrantImmunity(BaseEntity entity, float duration = -1f) { if (entity != null && entity?.net != null && !entity.IsDestroyed) GrantImmunity(entity.net.ID, duration); }

        private void GrantImmunity(NetworkableId netID, float duration = -1f)
        {
            if (!immuneEntities.Add(netID)) return;

            if (duration != -1f)
            {
                Timer newTimer;
                newTimer = timer.Once(duration, () =>
                {
                    Timer getTimer;
                    if (immuneTimers.TryGetValue(netID, out getTimer)) immuneTimers.Remove(netID);
                    if (immuneEntities.Contains(netID)) immuneEntities.Remove(netID);
                });
                immuneTimers[netID] = newTimer;
            }
        }

        private void RemoveImmunity(NetworkableId netID)
        {
            immuneEntities.Remove(netID);

            Timer getTimer;
            if (immuneTimers.TryGetValue(netID, out getTimer)) immuneTimers.Remove(netID);
        }

        private void SaveMetabolism(BasePlayer player)
        {
            if (player == null || player.IsDead() || player.metabolism == null) return;
            var metaDictOut = new Dictionary<string, float>();
            if (!oldMetabolism.TryGetValue(player.userID, out metaDictOut)) metaDictOut = oldMetabolism[player.userID] = new Dictionary<string, float>();
            var calories = player?.metabolism?.calories?.value ?? 0;
            var hydration = player?.metabolism?.hydration?.value ?? 0;
            var health = player?.Health() ?? 100;
            oldMetabolism[player.userID]["calories"] = calories;
            oldMetabolism[player.userID]["hydration"] = hydration;
            oldMetabolism[player.userID]["health"] = health;
        }

        private void LoadOldMetabolism(BasePlayer player)
        {
            if (player == null || player.IsDead() || player.metabolism == null) return;
            var metaDictOut = new Dictionary<string, float>();
            if (!oldMetabolism.TryGetValue(player.userID, out metaDictOut)) return;
            player.metabolism.calories.value = metaDictOut["calories"];
            player.metabolism.hydration.value = metaDictOut["hydration"];
            SetHealth(player, metaDictOut["health"]);
        }

        private static Vector3 GetVector3FromString(string vectorStr)
        {
            var vector = Vector3.zero;
            if (string.IsNullOrEmpty(vectorStr)) return vector;
            vectorStr = vectorStr.Replace("(", "").Replace(")", "");
            var split1 = vectorStr.Split(',');
            try { vector = new Vector3(Convert.ToSingle(split1[0]), Convert.ToSingle(split1[1]), Convert.ToSingle(split1[2])); }
            catch (Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
            return vector;
        }

        private class ItemInfo
        {
            [JsonProperty(PropertyName = "amt", Required = Required.Always)]
            private int _amount = 1;

            [JsonIgnore]
            public int Amount
            {
                get { return Mathf.Clamp(_amount, 1, int.MaxValue); }
                set { _amount = Mathf.Clamp(value, 1, int.MaxValue); }
            }

            [JsonProperty(PropertyName = "sName", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string shortName = string.Empty;
            [JsonProperty(PropertyName = "cont", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string containerName = string.Empty;

            public Dictionary<string, int> mods = new Dictionary<string, int>();

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string ammoName = string.Empty;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int ammoAmount = 0;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public ulong skinID = 0;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(-1)]
            public int itemPos = -1;

            public ItemInfo(string shortname, int itemAmount = 1, ulong skin = 0)
            {
                shortName = shortname;
                Amount = itemAmount;
                skinID = skin;
            }

            public ItemDefinition GetDefinition() => ItemManager.FindItemDefinition(shortName);

            public Item GetItem() => ItemManager.Create(GetDefinition(), Amount, skinID);

            public List<ItemDefinition> GetModDefinitions()
            {
                var defs = new List<ItemDefinition>();
                foreach (var kvp in mods)
                {
                    var def = ItemManager.FindItemDefinition(kvp.Key);
                    if (def != null) defs.Add(def);
                }
                return defs;
            }

            public List<Item> GetModItems()
            {
                var defs = GetModDefinitions();
                var items = new List<Item>();
                for (int i = 0; i < defs.Count; i++)
                {
                    var def = defs[i];
                    var newItem = ItemManager.Create(def, mods?.Where(p => p.Key == def.shortname)?.FirstOrDefault().Value ?? 1, 0);
                    if (newItem != null) items.Add(newItem);
                }
                return items;
            }

        }

        private class ItemPosition
        {
            [JsonProperty(PropertyName = "cont", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string containerName = string.Empty;
            [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int itemID = 0;
            [JsonIgnore]
            public string shortName { get { return ItemManager.FindItemDefinition(itemID)?.shortname ?? string.Empty; } }
            [JsonProperty(PropertyName = "pos", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(-1)]
            public int position = -1;

            public ItemPosition() { }

            public ItemPosition(int Id, string containername, int pos = -1)
            {
                itemID = Id;
                containerName = containername;
                position = pos;
            }
        }

        private class ModInfo
        {
            [JsonProperty(PropertyName = "sName", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string shortName = string.Empty;
            [JsonProperty(PropertyName = "psName", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string parentShortName = string.Empty;

            [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            private int _amt = 1;

            [JsonIgnore]
            public int amount
            {
                get { return Mathf.Clamp(_amt, 1, int.MaxValue); }
                set { _amt = Mathf.Clamp(value, 1, int.MaxValue); }
            }

            public ModInfo() { }
            public ModInfo(string shortModName, string parentShort, int amt = 1)
            {
                parentShortName = parentShort;
                shortName = shortModName;
                amount = amt;
            }

            public ModInfo(ItemDefinition def, ItemDefinition parentDef = null, int amt = 1)
            {
                shortName = def?.shortname ?? string.Empty;
                parentShortName = parentDef?.shortname ?? string.Empty;
                amount = amt;
            }

            public ModInfo(Item item, Item parent = null)
            {
                if (item == null) return;
                shortName = item.info.shortname;
                parentShortName = parent?.info?.shortname ?? string.Empty;
            }

            public ItemDefinition GetDefinition() { return ItemManager.FindItemDefinition(shortName); }
            public Item GetItem()
            {
                var def = GetDefinition();
                return (def == null) ? null : ItemManager.Create(def, amount);
            }
        }

        private ItemContainer GetContainerFromName(BasePlayer player, string contName)
        {
            if (player == null || string.IsNullOrEmpty(contName)) return null;
            return (contName == "wear") ? (player?.inventory?.containerWear ?? null) : (contName == "belt" ? (player?.inventory?.containerBelt ?? null) : (player?.inventory?.containerMain ?? null));
        }

        private string GetContainerName(ItemContainer container)
        {
            var ownerPlayer = container?.playerOwner ?? null;
            if (ownerPlayer == null) return container?.entityOwner?.ShortPrefabName ?? string.Empty;
            return container == ownerPlayer.inventory.containerMain ? "main" : (container == ownerPlayer.inventory.containerBelt ? "belt" : (container == ownerPlayer.inventory.containerWear ? "wear" : string.Empty));
        }

        private class FlagMovement : MonoBehaviour
        {
            private BaseEntity entity;
            //     public BaseEntity entity;
            public BasePlayer parent;
            public float speed = 5;
            public float distInFront = 3.5f;

            private void Awake()
            {
                entity = GetComponent<BaseEntity>();
                if (entity == null || entity.IsDestroyed)
                {
                    DoDestroy();
                    return;
                }
            }

            public void DoDestroy() => Destroy(this);

            private void Update()
            {
                if (entity == null || entity?.transform == null || parent == null || parent?.transform == null || parent.IsDestroyed) return;
                entity.transform.position = parent.eyes.transform.position;
                entity.SendNetworkUpdateImmediate();
            }
        }

        private readonly Dictionary<string, Vector3> specPos = new Dictionary<string, Vector3>();
        public void SpectatePlayer(BasePlayer player, BasePlayer target, bool forceOnlyTarget = true)
        {
            if (player == null || player?.transform == null || player.IsSpectating() || !player.IsConnected || target == null || target.IsDead() || !target.IsConnected) return;
            var playerPos = player?.transform?.position ?? Vector3.zero;
            if (playerPos == Vector3.zero) return;
            else PrintWarning("spec pos set to: " + playerPos);
            specPos[player.UserIDString] = playerPos;
            var heldEntity = (player?.GetActiveItem()?.GetHeldEntity() ?? null) as HeldEntity;
            if (heldEntity != null) heldEntity?.SetHeld(false);
            PrintWarning("Got past pos setting, held entity setting");

            // Put player in spectate mode
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Spectating, true);
            player.gameObject.SetLayerRecursive(10);
            CancelInvoke("MetabolismUpdate", player);
            CancelInvoke("InventoryUpdate", player);
            PrintWarning("Cancelled invokes, set flags");
            player.ClearEntityQueue();
            player.SendEntitySnapshot(target);

            player.gameObject.Identity();
            player.SetParent(target);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, true);
            player.Command("client.camoffset", new object[] { new Vector3(0, 3.5f, 0) });
            //  if (forceOnlyTarget)
            player.spectateFilter = target.UserIDString;
            PrintWarning("Spectated: " + target.displayName + ", for: " + player.displayName);
        }

        public void StopSpectatePlayer(BasePlayer player)
        {
            if (player == null || player?.transform == null || !player.IsSpectating() || !player.IsConnected) return;
            player.Command("camoffset", "0,1,0");
            player.SetParent(null);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Spectating, false);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, false);
            player.gameObject.SetLayerRecursive(17);

            var heldEntity = (player?.GetActiveItem()?.GetHeldEntity() ?? null) as HeldEntity;
            if (heldEntity != null) heldEntity?.SetHeld(true);
            player.StartSleeping();
            Vector3 lastPos;
            if (specPos.TryGetValue(player.UserIDString, out lastPos) && lastPos != Vector3.zero)
            {
                PrintWarning("teleport player last spec pos: " + lastPos);
                if (!TeleportPlayer(player, lastPos, true, false, true)) PrintWarning("NO TELEPORT PLAYER!!");
                else PrintWarning("DID teleport player, now pos: " + player.transform.position);
            }
            else PrintWarning("no spec pos");
        }

        private bool IsNullableType(Type type) { return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)); }

        private void SetValue(object inputObject, string propertyName, object propertyVal)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException();
            //find out the type
            var type = inputObject.GetType();

            //get the property information based on the type
            var propertyInfo = type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

            //find the property type
            var propertyType = propertyInfo.FieldType;

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal);
        }

        private object GetValue(object inputObject, string propertyName)
        {
            if (inputObject == null || string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException();
            //find out the type
            var type = inputObject.GetType();

            //get the property information based on the type
            var propertyInfo = type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(inputObject);
            return propertyInfo;
        }

        private class CTFFlag
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string prefabName = string.Empty;

            public string _position = string.Empty;
            //[JsonProperty(Required = Required.AllowNull)]
            //  private string _position = string.Empty;

            public string _rotation = string.Empty;
            [JsonIgnore]
            public Vector3 Position
            {
                get { return (_entity?.transform != null) ? (_entity?.transform?.position ?? Vector3.zero) : Vector3.zero; }
                set
                {
                    if (_entity?.transform != null)
                    {
                        _entity.transform.position = value;
                        _entity.SendNetworkUpdate();
                    }
                }
            }
            [JsonIgnore]
            public Vector3 StartPosition
            {
                get { return GetVector3FromString(_position); }
                set { _position = value.ToString(); }
            }
            [JsonIgnore]
            public Vector3 StartRotation
            {
                get { return GetVector3FromString(_rotation); }
                set { _rotation = value.ToString(); }
            }
            [JsonIgnore]
            public Vector3 Rotation
            {
                get { return (_entity?.transform != null) ? (_entity?.transform?.eulerAngles ?? Vector3.zero) : Vector3.zero; }
                set
                {
                    if (_entity?.transform != null)
                    {
                        _entity.transform.eulerAngles = value;
                        _entity.SendNetworkUpdate();
                    }
                }
            }

            [JsonIgnore]
            public bool Dropped
            {
                get
                {
                    if (_entity?.parentEntity != null) return false;
                    return Position.ToString() == StartPosition.ToString();
                }
            }
            //    public bool dropped = false;
            /*/
                [JsonIgnore]
                public BasePlayer parent
                {
                    set
                    {
                        if (value != null && !value.IsDestroyed)
                        {
                            if (_entity != null && !_entity.IsDestroyed)
                            {
                                var track = _entity?.GetComponent<FlagMovement>() ?? null;
                                if (track == null) track = _entity.gameObject.AddComponent<FlagMovement>();
                                track.distInFront = -0.35f;
                                track.parent = value;
                                track.speed = 1000;
                            }
                        }
                    }
                    get { return (_entity == null || _entity.IsDestroyed) ? null : _entity?.GetComponent<FlagMovement>()?.parent ?? null; }
                }/*/
            [JsonIgnore]
            private BaseEntity _entity;

            public BaseEntity SpawnEntity()
            {
                if (_entity != null && !_entity.IsDestroyed) return _entity;
                else _entity = null;
                if (string.IsNullOrEmpty(prefabName)) return null;
                var newEnt = GameManager.server.CreateEntity(prefabName, StartPosition, Quaternion.Euler(StartRotation));
                if (newEnt != null)
                {
                    newEnt.Spawn();
                    _entity = newEnt;
                    return newEnt;
                }
                return null;
            }

            public BaseEntity GetEntity() { return _entity; }


            public void SetEntity(BaseEntity entity, bool doSpawn = true)
            {
                if (entity == null) throw new ArgumentNullException();
                if (_entity != null && !_entity.IsDestroyed) _entity.Kill();
                _entity = entity;
                prefabName = entity.ShortPrefabName;
                if (doSpawn) entity.Spawn();
            }
        }

        private class SpawnPoint
        {
            [JsonIgnore]
            public Vector3 Point
            {
                get { return GetVector3FromString(_point); }
                set { _point = value.ToString().Replace("(", "").Replace(")", ""); }
            }

            [JsonProperty(Required = Required.Default)]
            private string _point = string.Empty;
            public enum SpawnType { Base, Default, None }
            public SpawnType Type = SpawnType.Default;
            public string Name = string.Empty;
        }

        private class LocalArena
        {
            public ArenaInfo Arena = null;

            public LocalArena(ArenaInfo arena)
            {
                if (arena == null) throw new ArgumentNullException();
                else Arena = arena;
            }
        }

        private class ItemSpawn
        {
            [JsonProperty(PropertyName = "i", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public ItemInfo Item = null;

            [JsonProperty(PropertyName = "p", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            private string _pos = string.Empty;
            [JsonIgnore]
            public Vector3 Position
            {
                get { return GetVector3FromString(_pos); }
                set { _pos = value.ToString().Replace("(", "").Replace(")", ""); }
            }

            [JsonProperty(PropertyName = "sc", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            private float _spawnPerc = 0f;
            public float ChanceToSpawn
            {
                get { return _spawnPerc; }
                set { _spawnPerc = Mathf.Clamp(value, 0f, 100f); }
            }

            public ItemSpawn() { }
            public ItemSpawn(ItemInfo itemInfo, Vector3 position)
            {
                if (itemInfo == null || position == Vector3.zero) return;
                Item = itemInfo;
                Position = position;
            }
        }

        private class BoxSpawn
        {
            [JsonProperty(PropertyName = "pfab", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string PrefabName = string.Empty;

            [JsonProperty(PropertyName = "p", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            private string _pos = string.Empty;
            [JsonIgnore]
            public Vector3 Position
            {
                get { return GetVector3FromString(_pos); }
                set { _pos = value.ToString().Replace("(", "").Replace(")", ""); }
            }
            [JsonIgnore]
            private BaseEntity _entity;
            public BaseEntity GetEntity() { return _entity; }

            public BaseEntity SpawnEntity()
            {
                if (string.IsNullOrEmpty(PrefabName))
                {

                    Interface.Oxide.LogWarning("SpawnEntity() called with empty PrefabName");
                    return null;
                }
                var newEnt = GameManager.server.CreateEntity(PrefabName, Position);
                if (newEnt == null) return null;
                var getLoot = newEnt?.GetComponent<LootContainer>() ?? null;
                if (getLoot == null)
                {
                    Interface.Oxide.LogError("SpawnEntity() for BoxSpawn called on non-loot container!!");
                    return null;
                }
                newEnt.Spawn();
                _entity = newEnt;
                return newEnt;
            }
            public BaseEntity SpawnRandomEntity(List<string> blackList = null)
            {
                if (_entity != null && !_entity.IsDestroyed)
                {
                    _entity.Kill();
                    _entity = null;
                }
                var prefab = DMMain?.GetRandomBoxPrefab(blackList) ?? string.Empty;
                if (string.IsNullOrEmpty(prefab))
                {
                    Interface.Oxide.LogWarning("GetRandomBoxPrefab() returned empty string");
                    return null;
                }
                var newEnt = GameManager.server.CreateEntity(prefab, Position) as LootContainer;
                if (newEnt == null) return null;
                newEnt.Spawn();
                _entity = newEnt;
                return newEnt;
            }

            public void EnsureWeapon()
            {
                if (_entity == null || _entity.IsDestroyed)
                {
                    Interface.Oxide.LogWarning("EnsureWeapon() called on null entity");
                    return;
                }
                var loot = _entity?.GetComponent<LootContainer>() ?? null;
                if (loot == null)
                {
                    Interface.Oxide.LogWarning("EnsureWeapon() called on null loot container");
                    return;
                }
                bool hasWeapon;
                for (int i = 0; i < 40; i++)
                {
                    hasWeapon = loot?.inventory?.itemList?.Any(p => (p?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null) != null) ?? false;
                    if (!hasWeapon) loot.SpawnLoot();
                    else
                    {
                        Interface.Oxide.LogWarning("hasWeapon after: " + i + " tries");
                        break;
                    }
                }
                hasWeapon = loot?.inventory?.itemList?.Any(p => (p?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null) != null) ?? false;
                if (!hasWeapon) Interface.Oxide.LogWarning("Does not have weapon after 40 tries! " + _entity.ShortPrefabName);
            }

            [JsonProperty(PropertyName = "sc", Required = Required.AllowNull)]
            private float _spawnPerc = 0f;
            public float ChanceToSpawn
            {
                get { return _spawnPerc; }
                set { _spawnPerc = Mathf.Clamp(value, 0f, 100f); }
            }

            public BoxSpawn() { }
            public BoxSpawn(string prefabName, float chanceToSpawn)
            {
                PrefabName = prefabName;
                ChanceToSpawn = ChanceToSpawn;
            }
        }

        private class Updater : MonoBehaviour
        {
            public MapMarkerGenericRadius Marker;
            public float localSpeed, finalRadius;
            public ArenaSphere SphereInfo = null;

            private void Awake()
            {
                InvokeHandler.Invoke(this, () =>
                {
                    if (SphereInfo == null)
                    {
                        Interface.Oxide.LogWarning("Awake() on Updater called without SphereInfo");
                        Awake();
                        return;
                    }
                    localSpeed = SphereInfo.speed / GetSphereScale();
                    finalRadius = SphereInfo.targetRadius / GetSphereScale();
                    Marker = gameObject.GetComponent<MapMarkerGenericRadius>();
                    if (Marker == null || Marker.IsDestroyed) Interface.Oxide.LogWarning("Awake() called on Marker null");
                }, 0.125f);
                InvokeHandler.InvokeRepeating(this, () =>
                {
                    localSpeed = SphereInfo.speed / GetSphereScale();
                    finalRadius = SphereInfo.targetRadius / GetSphereScale();
                }, 1f, 0.5f);
            }

            private void Update()
            {
                if (Marker == null || Marker.IsDestroyed || Marker.radius <= finalRadius) return;

                Marker.radius = Mathf.MoveTowards(Marker.radius, finalRadius, Time.deltaTime * localSpeed);
                Marker.SendUpdate();
            }
        }

        private class ArenaSphere
        {
            private float _speed = 10f;
            public float speed
            {
                get { return _speed; }
                set
                {
                    oldSpeed = _speed;
                    _speed = value;
                }
            }
            public Vector3 Position
            {
                get { return Sphere?.transform?.position ?? Vector3.zero; }
                set
                {
                    if (Sphere?.transform != null) Sphere.transform.position = value;
                    if (Marker != null && Marker?.transform != null)
                    {
                        Marker.transform.position = value;
                        Marker.SendNetworkUpdate();
                        Marker.SendUpdate();
                    }
                    if (Spheres != null && Spheres.Count > 0 && ServerMgr.Instance != null) ServerMgr.Instance.StartCoroutine(UpdateAllSpherePositions(value));
                }
            }

            public float oldSpeed = 10f;
            public float targetRadius = 200f;
            private SphereEntity _sphere;
            public SphereEntity Sphere
            {
                get { return _sphere; }
                set
                {
                    if (_sphere != null && !_sphere.IsDestroyed) _sphere.Kill();
                    _sphere = value;
                }
            }
            public HashSet<SphereEntity> Spheres = new HashSet<SphereEntity>();
            private MapMarkerGenericRadius _marker;
            public MapMarkerGenericRadius Marker
            {
                get { return _marker; }
                set
                {
                    if (_marker != null && !_marker.IsDestroyed) _marker.Kill();
                    _marker = value;
                }
            }

            public void UpdateAllSpheres()
            {
                if (Sphere != null)
                {
                    Sphere.lerpSpeed = speed;
                    Sphere.lerpRadius = targetRadius;
                }
                if (Spheres != null && Spheres.Count > 0 && ServerMgr.Instance != null) ServerMgr.Instance.StartCoroutine(UpdateAllSpheres2());
            }

            private IEnumerator UpdateAllSpheres2()
            {
                if (Spheres == null || Spheres.Count < 1) yield break;
                var sphereMax = 4;
                var sphereCount = 0;
                foreach (var sphere in Spheres)
                {
                    if (sphere == null || sphere.IsDestroyed) continue;
                    sphereCount++;
                    if (sphereCount >= sphereMax)
                    {
                        sphereCount = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    sphere.lerpSpeed = speed;
                    sphere.lerpRadius = targetRadius;
                    sphere.currentRadius = Sphere.currentRadius;
                }
            }

            private IEnumerator UpdateAllSpherePositions(Vector3 pos)
            {
                if (Spheres == null || Spheres.Count < 1) yield break;
                var sphereMax = 4;
                var sphereCount = 0;
                foreach (var sphere in Spheres)
                {
                    if (sphere == null || sphere.IsDestroyed) continue;
                    sphereCount++;
                    if (sphereCount >= sphereMax)
                    {
                        sphereCount = 0;
                        yield return CoroutineEx.waitForEndOfFrame;
                    }
                    sphere.transform.position = pos;
                }
            }

            public ArenaSphere() { }
            public ArenaSphere(float Speed, float TargetRadius)
            {
                speed = Speed;
                targetRadius = TargetRadius;
            }
        }

        public static float GetSphereScale()
        {
            var mapSize = ConVar.Server.worldsize;
            //      var inc = mapSize > 3100;
            var perc1 = (mapSize - (float)3100) / 3100 * 100f;
            // var perc2 = ((((float)3100 - (float)mapSize) / (float)mapSize) * 100f);
            return 7.272727272727273f + (7.272727272727273f * perc1 / 100);
        }

        private class BanInfo
        {
            public ulong UserID = 0;
            [JsonIgnore]
            public string UserIDString
            {
                get { return UserID.ToString(); }
                set { UserID = ulong.Parse(value); }
            }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DateTime? EndTime { get; set; } = null;
            [JsonIgnore]
            public bool Permanent
            {
                get
                {
                    return EndTime == null || EndTime <= DateTime.MinValue;
                }
            }

            public BanInfo() { }
            public BanInfo(ulong userId, DateTime? endTime = null)
            {
                UserID = userId;
                EndTime = endTime;
            }
        }

        private class ArenaInfo
        {

            public float SpawnProtectionTime = 6f;

            public float SpawnWithHealth = 100f;

            public float BleedingScalar = 1f;
            public float HealingScalar = 1f;

            public int MaxPlayers = 12;

            public int MaxPing = 300;
            public float LastAttackedCooldown = 30f;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(-1f)]
            public float FireballTime = -1f;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool CanJoinBuildingBlocked = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string ArenaName;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string ZoneID;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool Disabled = false;

            public bool Airstrikes = true;

            public int AirstrikeKS = 5;

            public string DisabledText = "This arena is currently disabled.";
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string Description = string.Empty;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool AllowDropping = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool AllowDroppingActiveItem = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool PlayersCanBuild = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool OIC = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool CTF = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool Teams = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(-1)]
            public int MaxTeamImbalance = -1;

            public string AlliedName = "Allies";

            public string AxisName = "Axis";

            public int CTFScore = 5;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(-1f)]
            public float RespawnTime = -1f;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool SpectateTeamDead = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool CanJoinAnytime = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool FakeDeaths = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool IgnoreStacks = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool AllowGather = false;

            public CTFFlag AlliedFlag = null;

            public CTFFlag AxisFlag = null;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool Hide = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool Fortnite = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool RemoveItemsOnDeath = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool RoundBased = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool JoinMidRound = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool ForceDay = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool ForceNight = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool NetworkOut = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(true)]
            public bool AdminNetAlways = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool CH47Drop = false;

            public List<BanInfo> Bans = new List<BanInfo>();
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool SoftAntiHack = false;
            /// <summary>
            /// Scale the chance for loot to spawn by this value. Fortnite mode only.
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(1.0f)]
            public float LootChanceScalar = 1.0f;
            /// <summary>
            /// Waiting period before starting a new game (round) for a round based mode.
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(20f)]
            public float NewGameTime = 20f;
            /// <summary>
            /// Forces all players who connect to join this game mode. Bypasses all 'can join' checks, strange behavior may occur if this is set to true for more than one arena. This will also prevent leaving the Deathmatch arena (unless admin).
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(false)]
            public bool ForceJoin = false;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            private string _waitPos = string.Empty;
            /// <summary>
            /// If set on a round based arena that cannot be joined mid-game, players who try to join will be placed in this waiting area until they can join.
            /// </summary>
            [JsonIgnore]
            public Vector3 WaitingArea
            {
                get { return GetVector3FromString(_waitPos); }
                set { _waitPos = value.ToString(); }
            }
            /// <summary>
            /// Players required to start the game. RoundBased must be true.
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(2)]
            public int PlayersForStart = 2;
            [JsonIgnore]
            public Dictionary<string, float> lastStormHurt = new Dictionary<string, float>();
            [JsonIgnore]
            public Dictionary<string, Timer> queueTimer = new Dictionary<string, Timer>();
            [JsonIgnore]
            public ArenaSphere Sphere = null;
            [JsonIgnore]
            public List<CH47HelicopterAIController> CH47s = new List<CH47HelicopterAIController>();
            public int PurgeCH47s()
            {
                if (CH47s != null && CH47s.Count > 0) return CH47s.RemoveAll(p => p == null || p.IsDestroyed);
                else return -1;
            }
            public void KillCH47s()
            {
                if (CH47s != null && CH47s.Count > 0)
                {
                    for (int i = 0; i < CH47s.Count; i++)
                    {
                        var heli = CH47s[i];
                        if (heli != null && !heli.IsDestroyed) heli.Kill();
                    }
                }
                PurgeCH47s();
            }
            public bool DropPlayers()
            {
                KillCH47s();
                var pubg = DMMain?.PUBG ?? null;
                if (pubg == null || !pubg.IsLoaded)
                {
                    Interface.Oxide.LogWarning("PUBG not loaded!");
                    return false;
                }
                if (ActivePlayers == null || ActivePlayers.Count < 1)
                {
                    Interface.Oxide.LogWarning("DropPlayers() called with no players in arena!");
                    return false;
                }
                var ch47s = pubg?.Call<List<CH47HelicopterAIController>>("SpawnAndMount", ActivePlayers.ToList());
                if (ch47s == null || ch47s.Count < 1)
                {
                    Interface.Oxide.LogWarning("No CH47 list after SpawnAndMount call!");
                    return false;
                }
                for (int i = 0; i < ch47s.Count; i++) ch47s[i].OwnerID = 757;
                CH47s = ch47s;
                return true;
            }
            public void StartSphere(bool startPaused = false)
            {
                var pos = DMMain?.GetZonePosition(ZoneID) ?? Vector3.zero;
                if (pos == Vector3.zero)
                {
                    Interface.Oxide.LogWarning("StartSphere got zone pos of vec3 zero");
                    return;
                }
                Sphere = new ArenaSphere(4f, 0.01f);
                var sphereEnt = (SphereEntity)GameManager.server.CreateEntity("assets/prefabs/visualization/sphere.prefab", pos);
                if (sphereEnt == null) return;
                for (int i = 0; i < 9; i++)
                {
                    var spheres = (SphereEntity)GameManager.server.CreateEntity("assets/prefabs/visualization/sphere.prefab", pos);
                    spheres.enableSaving = false;
                    for (int j = 0; j < BasePlayer.activePlayerList.Count; j++)
                    {
                        var ply = BasePlayer.activePlayerList[j];
                        if (ply == null) continue;
                        if (!ActivePlayers.Contains(ply)) DMMain?.DestroyClientEntity(ply, spheres);
                    }
                    Sphere.Spheres.Add(spheres);
                }


                sphereEnt.currentRadius = (ZoneRadius * 2) - 7.5f;
                Interface.Oxide.LogWarning("currentRadius: " + sphereEnt.currentRadius + ", zoneradius: " + ZoneRadius);
                sphereEnt.lerpSpeed = 0;
                sphereEnt.lerpRadius = Sphere.targetRadius;

                sphereEnt.enableSaving = false;
                sphereEnt.globalBroadcast = false;
                sphereEnt.Spawn();
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var ply = BasePlayer.activePlayerList[i];
                    if (ply == null) continue;
                    if (!ActivePlayers.Contains(ply)) DMMain?.DestroyClientEntity(ply, sphereEnt);
                }
                Sphere.Sphere = sphereEnt;
                foreach (var sphere in Sphere.Spheres)
                {
                    if (sphere == null) continue;
                    sphere.lerpRadius = sphereEnt.lerpRadius;
                    sphere.currentRadius = sphereEnt.currentRadius;
                    sphere.lerpSpeed = 0f;
                    sphere.globalBroadcast = false;
                    sphere.Spawn();
                    InvokeHandler.Invoke(sphere, () =>
                    {
                        sphere.lerpSpeed = sphereEnt.lerpSpeed = Sphere.speed;
                    }, 1f);
                }
                Sphere.Spheres.Add(sphereEnt);
                var marker = (MapMarkerGenericRadius)GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", pos);
                marker.alpha = 0.5f;
                marker.color1 = Color.blue;
                marker.color2 = Color.blue;
                marker.radius = sphereEnt.currentRadius / GetSphereScale();
                marker.Spawn();
                marker.SendUpdate();
                var upd = marker.gameObject.AddComponent<Updater>();
                upd.SphereInfo = Sphere;
                Sphere.Marker = marker;
                Timer hurtTimer = null;
                Timer hookTimer = null;
                hookTimer = DMMain.timer.Every(0.25f, () =>
                {
                    if (ActivePlayers == null || ActivePlayers.Count < 1 || !GameInProgress || StartingGame || !Fortnite)
                    {
                        hookTimer.Destroy();
                        hookTimer = null;
                        return;
                    }
                    foreach (var ply in ActivePlayers)
                    {
                        if (ply == null || ply.IsDead()) continue;
                        UpdateStormValue(ply);
                    }
                });
                hurtTimer = DMMain.timer.Every(0.5f, () =>
                {
                    if (ActivePlayers == null || ActivePlayers.Count < 1 || !GameInProgress || StartingGame || !Fortnite)
                    {
                        hurtTimer.Destroy();
                        hurtTimer = null;
                        return;
                    }
                    foreach (var ply in ActivePlayers)
                    {
                        if (ply == null || ply.IsDead()) continue;
                        if (InStorm(ply))
                        {
                            float lastTime;
                            if (!lastStormHurt.TryGetValue(ply.UserIDString, out lastTime) || (Time.realtimeSinceStartup - lastTime) >= 1)
                            {
                                ply.Hurt(1.15f, Rust.DamageType.ElectricShock, null, false);
                                lastStormHurt[ply.UserIDString] = Time.realtimeSinceStartup;
                            }

                            var eyePos = ply?.eyes?.position ?? Vector3.zero;

                            for (int j = 0; j < UnityEngine.Random.Range(2, 8); j++)
                            {
                                var spreadPos = DMMain?.SpreadVector(eyePos, UnityEngine.Random.Range(0.15f, 0.725f)) ?? eyePos;
                                var efx = new Effect("assets/prefabs/locks/keypad/effects/lock.code.shock.prefab", spreadPos, Vector3.zero);

                                EffectNetwork.Send(efx, ply?.net?.connection);
                            }
                        }
                    }
                });
                var nextSphereTime = DateTime.MinValue;
                //var invokeSeconds = 0f;
                Action sphereAct = null;
                sphereAct = new Action(() =>
                {
                    if (Sphere == null || Sphere.Sphere == null || Sphere.Spheres == null || Sphere.Spheres.Count < 1 || Sphere.targetRadius == Sphere.Sphere.currentRadius)
                    {
                        InvokeHandler.CancelInvoke(sphereEnt, sphereAct);
                        return;
                    }
                    if (Sphere.speed <= 0f)
                    {
                        DMMain?.SendMessageToDM(this, "Storm incoming!");
                        Interface.Oxide.LogWarning("sphere speed <= 0: " + Sphere.speed + ", resuming!");
                        ResumeSphere();
                        var posPlayers = ActivePlayers?.Where(p => p != null && p?.transform != null && p.IsAlive() && !p.IsSpectating() && !InStorm(p)) ?? null;
                        var posList = (posPlayers == null || !posPlayers.Any()) ? null : posPlayers?.Select(p => p?.transform?.position ?? Vector3.zero);
                        if (posList == null || !posList.Any()) Interface.Oxide.LogWarning("no players in posList for storm");
                        else
                        {
                            Interface.Oxide.LogWarning("Got posList, count: " + posList.Count());
                            var newPos = DMMain?.GetAverageVector(posList) ?? Vector3.zero;
                            if (newPos != Vector3.zero)
                            {
                                newPos = DMMain?.SpreadVector(newPos, UnityEngine.Random.Range(0.5f, 2f)) ?? newPos;
                                if (InStorm(newPos)) Interface.Oxide.LogWarning("got new storm position that inside old storm position?!");
                                var oldPos = Sphere?.Position ?? Vector3.zero;
                                Interface.Oxide.LogWarning("Got new pos, old: " + Sphere.Position + ", new: " + newPos + ": " + newPos);
                                Sphere.Position = newPos;
                            }
                        }

                        DMMain?.SendMessageToDM(this, "Storm stopping in 30 seconds");
                    }
                    else
                    {
                        Interface.Oxide.LogWarning("Sphere speed not <= 0: " + Sphere.speed + ", pausing!");
                        PauseSphere();
                        DMMain?.SendMessageToDM(this, "Storm resuming in 30 seconds");
                    }
                });


                if (startPaused)
                {
                    sphereEnt.Invoke(() =>
                    {
                        PauseSphere();
                    }, 0.5f);
                    sphereEnt.Invoke(() =>
                    {
                        DMMain?.SendMessageToDM(this, "Storm incoming!");
                        ResumeSphere();
                        sphereEnt.InvokeRepeating(sphereAct, 30f, 30f);
                    }, 45f);
                    DMMain?.SendMessageToDM(this, "Storm coming in 45 seconds");
                }
                else
                {
                    DMMain?.SendMessageToDM(this, "Storm coming in 30 seconds");
                    sphereEnt.InvokeRepeating(sphereAct, 30f, 30f);
                }

                //sphereEnt.InvokeRepea
            }

            public bool IsBanned(ulong UserID)
            {
                var now = DateTime.UtcNow;
                for (int i = 0; i < Bans.Count; i++)
                {
                    var ban = Bans[i];
                    if (ban.UserID == UserID && (ban.Permanent || now < ban.EndTime)) return true;
                }
                return false;
            }

            public BanInfo Ban(ulong UserID, DateTime? EndTime = null)
            {
                var findBan = Bans?.Where(p => p.UserID == UserID)?.FirstOrDefault() ?? null;
                if (findBan != null) findBan.EndTime = EndTime;
                else
                {
                    findBan = new BanInfo(UserID, EndTime);
                    Bans.Add(findBan);
                }
                var findPly = ActivePlayers?.Where(p => p?.userID == UserID)?.FirstOrDefault() ?? null;
                if (findPly != null) LeaveDM(findPly);
                return findBan;
            }

            public BanInfo GetBan(ulong UserID)
            {
                return Bans?.Where(p => p.UserID == UserID)?.FirstOrDefault() ?? null;
            }

            public void Unban(ulong UserID)
            {
                var findBan = Bans?.Where(p => p.UserID == UserID)?.FirstOrDefault() ?? null;
                if (findBan != null) Bans.Remove(findBan);
            }

            [JsonIgnore]
            public HashSet<string> AliveGUIImagePlayers = new HashSet<string>();
            public void AliveGUI(BasePlayer player)
            {
                if (player == null || !player.IsConnected)
                {
                    Interface.Oxide.LogWarning("AliveGUI called on bad player!!");
                    return;
                }

                var elements = new CuiElementContainer();
                var hasGUI = AliveGUIImagePlayers.Contains(player.UserIDString);
                if (!hasGUI)
                {
                    elements.Add(new CuiElement
                    {
                        Name = "DMAlive",
                        Components =
                    {
                        new CuiRawImageComponent { Color = "1 1 1 0.8", Url = "http://198.27.98.45:81/webstuff/fnitealive2.png", Sprite = "assets/content/textures/generic/fulltransparent.tga" },
                        new CuiRectTransformComponent { AnchorMin = "0.802 0.932",  AnchorMax = "0.821 0.965" }
                    }
                    });
                    AliveGUIImagePlayers.Add(player.UserIDString);
                }
                var innerText = new CuiElement
                {
                    Name = "DMAliveText",
                    Components =
                        {
                            new CuiTextComponent { Color = "1 1 1 1", Text = (ActivePlayers == null || ActivePlayers.Count < 1) ? "0" : ActivePlayers.Count(p => p != null && p.IsAlive() && !p.IsSpectating()).ToString("N0"), FontSize = 18, Align = TextAnchor.MiddleCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0.821 0.932", AnchorMax = "0.84 0.965" }
                        }
                };
                elements.Add(innerText);
                if (!hasGUI) CuiHelper.DestroyUi(player, "DMAlive");
                CuiHelper.DestroyUi(player, "DMAliveText");
                CuiHelper.AddUi(player, elements);
            }
            public void AliveGUIAll() { foreach (var ply in ActivePlayers) AliveGUI(ply); }
            public void StopAliveGUIAll(bool textOnly = false) { foreach (var ply in ActivePlayers) StopAliveGUI(ply, textOnly); }
            public void StopAliveGUI(BasePlayer player, bool textOnly = false)
            {
                if (player == null || !player.IsConnected) return;
                if (!textOnly)
                {
                    CuiHelper.DestroyUi(player, "DMAlive");
                    if (AliveGUIImagePlayers.Contains(player.UserIDString)) AliveGUIImagePlayers.Remove(player.UserIDString);
                }
                CuiHelper.DestroyUi(player, "DMAliveText");
            }
            [JsonIgnore]
            public HashSet<string> KillsGUIImagePlayers = new HashSet<string>();
            public void KillsGUI(BasePlayer player)
            {
                if (player == null || !player.IsConnected)
                {
                    Interface.Oxide.LogWarning("KillsGUI called on bad player!!");
                    return;
                }

                var elements = new CuiElementContainer();
                var hasGUI = KillsGUIImagePlayers.Contains(player.UserIDString);
                if (!hasGUI)
                {
                    elements.Add(new CuiElement
                    {
                        Name = "DMKills",
                        Components =
                    {
                        new CuiRawImageComponent { Color = "1 1 1 0.8", Url = "http://198.27.98.45:81/webstuff/fnitekill2.png", Sprite = "assets/content/textures/generic/fulltransparent.tga" },
                        new CuiRectTransformComponent { AnchorMin = "0.891 0.932",  AnchorMax = "0.909 0.965" }
                    }
                    });
                    KillsGUIImagePlayers.Add(player.UserIDString);
                }
                var innerText = new CuiElement
                {
                    Name = "DMKillsText",
                    Components =
                        {
                            new CuiTextComponent { Color = "1 1 1 1", Text = GetPlayerKills(player.UserIDString).ToString("N0"), FontSize = 18, Align = TextAnchor.MiddleCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0.914 0.932", AnchorMax = "0.933 0.965" }
                        }
                };
                elements.Add(innerText);
                if (!hasGUI) CuiHelper.DestroyUi(player, "DMKills");
                CuiHelper.DestroyUi(player, "DMKillsText");
                CuiHelper.AddUi(player, elements);
            }
            public void KillsGUIAll() { foreach (var ply in ActivePlayers) KillsGUI(ply); }
            public void StopKillsGUIAll(bool textOnly = false) { foreach (var ply in ActivePlayers) StopKillsGUI(ply, textOnly); }
            public void StopKillsGUI(BasePlayer player, bool textOnly = false)
            {
                if (player == null || !player.IsConnected) return;
                if (!textOnly)
                {
                    CuiHelper.DestroyUi(player, "DMKills");
                    if (KillsGUIImagePlayers.Contains(player.UserIDString)) KillsGUIImagePlayers.Remove(player.UserIDString);
                }
                CuiHelper.DestroyUi(player, "DMKillsText");
            }
            public void EndSphere()
            {
                if (Sphere == null) return;
                if (Sphere.Spheres != null & Sphere.Spheres.Count > 0)
                {
                    Interface.Oxide.LogWarning("Killing: " + Sphere.Spheres.Count.ToString("N0") + " spheres in arena:" + ArenaName);
                    foreach (var sphere in Sphere.Spheres) if (sphere != null && !sphere.IsDestroyed) sphere.Kill();
                }
                if (Sphere.Marker != null)
                {
                    Sphere.Marker = null;
                    Interface.Oxide.LogWarning("Killing marker in arena: " + ArenaName);
                }
                Sphere = null;
            }
            public void PauseSphere()
            {
                if (Sphere == null) return;
                Sphere.speed = 0f;
                Sphere.UpdateAllSpheres();
            }
            public void ResumeSphere(float newSpeed = -1f)
            {
                if (newSpeed != -1f) Sphere.speed = newSpeed;
                else Sphere.speed = Sphere.oldSpeed;
                Sphere.UpdateAllSpheres();
            }
            [JsonIgnore]
            public Dictionary<string, bool> lastStormValue = new Dictionary<string, bool>();
            public void UpdateStormValue(BasePlayer player)
            {
                if (player == null) return;
                var isIn = InStorm(player);
                bool lastVal;
                if (lastStormValue.TryGetValue(player.UserIDString, out lastVal))
                {
                    if (!lastVal && isIn) Interface.Oxide.CallHook("OnEnterStorm", player);
                    else if (lastVal && !isIn) Interface.Oxide.CallHook("OnExitStorm", player);
                }
                lastStormValue[player.UserIDString] = isIn;
            }
            public bool InStorm(BasePlayer player)
            {
                if (!Fortnite || Sphere == null || Sphere?.Sphere == null || player == null || player.IsDead() || player.IsSpectating() || (DMMain?.GetTimeAlive(player) < 1)) return false;
                return InStorm(player?.transform?.position ?? Vector3.zero);
            }

            public bool InStorm(Vector3 position)
            {
                if (position == Vector3.zero) return false;
                var spherePos = Sphere?.Position ?? Vector3.zero;
                if (spherePos == Vector3.zero) return false;
                var dist = (Vector3.Distance(position, spherePos) * 2f) + 8f;
                var diff = (dist < Sphere.Sphere.currentRadius) ? (Sphere.Sphere.currentRadius - dist) : (dist - Sphere.Sphere.currentRadius);

                if (dist > Sphere.Sphere.currentRadius && diff > 7.5f)
                {
                    //    Interface.Oxide.LogWarning("dist > radius: " + dist + " > " + Sphere.Sphere.currentRadius + ", diff: " + diff);
                    return true;
                }
                return false;
            }

            [JsonIgnore]
            public bool LocalArena = false;

            //public bool IsLocalArena() { return localArena != null; }

            //  [JsonIgnore]
            //    public LocalArena localArena = null;

            [JsonIgnore]
            public List<BasePlayer> ZonePlayers { get { return DMMain?.GetPlayersInZone(ZoneID) ?? null; } }

            [JsonIgnore]
            public HashSet<BasePlayer> ActivePlayers = new HashSet<BasePlayer>();

            public List<ItemSpawn> ItemSpawns = new List<ItemSpawn>();
            public List<BoxSpawn> BoxSpawns = new List<BoxSpawn>();
            [JsonIgnore]
            public List<Item> RemoveOnEnd = new List<Item>();

            [JsonIgnore]
            public HashSet<Item> SpawnedItems = new HashSet<Item>();

            // [JsonRequired]
            //   private Dictionary<string, string> spawnVectors = new Dictionary<string, string>();


            //     public Dictionary<Team, Dictionary<string, string>> teamSpawnVectors = new Dictionary<Team, Dictionary<string, string>>();

            public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

            public Dictionary<Team, List<SpawnPoint>> teamSpawnPoints = new Dictionary<Team, List<SpawnPoint>>();

            [JsonIgnore]
            public Dictionary<string, Team> PlayerTeams = new Dictionary<string, Team>();

            [JsonIgnore]
            public Dictionary<Team, int> TeamKills = new Dictionary<Team, int>();

            [JsonIgnore]
            public Dictionary<string, int> PlayerKills = new Dictionary<string, int>();

            [JsonIgnore]
            public Dictionary<string, int> PlayerDeaths = new Dictionary<string, int>();

            [JsonIgnore]
            public float GameTime = 0f;

            [JsonIgnore]
            public bool GameInProgress = false;

            [JsonIgnore]
            public Vector3 ZonePosition { get { return DMMain?.GetZonePosition(ZoneID) ?? Vector3.zero; } }

            [JsonIgnore]
            public float ZoneRadius { get { return DMMain?.GetZoneRadius(ZoneID) ?? 0f; } }

            [JsonIgnore]
            public float TimeLimit
            {
                get
                {
                    float val;
                    if (!TimeLimits.TryGetValue(Type, out val)) val = -1f;
                    return val;
                }
                set { TimeLimits[Type] = value; }
            }

            public Dictionary<GameType, float> TimeLimits = new Dictionary<GameType, float>();
            public Dictionary<GameType, int> MaxScores = new Dictionary<GameType, int>();

            [JsonIgnore]
            public int MaxScore
            {
                get
                {
                    int val;
                    if (!MaxScores.TryGetValue(Type, out val)) val = -1;
                    return val;
                }
                set { MaxScores[Type] = value; }
            }


            public Dictionary<string, List<ItemPosition>> playerItemPositions = new Dictionary<string, List<ItemPosition>>();

            public Dictionary<string, List<ItemPosition>> dmItemPositions = new Dictionary<string, List<ItemPosition>>();

            public Dictionary<string, Dictionary<string, ulong>> playerItemSkins = new Dictionary<string, Dictionary<string, ulong>>();

            public Dictionary<string, Dictionary<string, string>> playerItemNames = new Dictionary<string, Dictionary<string, string>>();

            public Dictionary<string, Dictionary<string, string>> playerAmmos = new Dictionary<string, Dictionary<string, string>>();

            public Dictionary<string, List<ModInfo>> playerModInfos = new Dictionary<string, List<ModInfo>>();
            //       public Dictionary<string, Dictionary<string, ModInfo>> playerModInfos = new Dictionary<string, Dictionary<string, ModInfo>>();
            //    public Dictionary<string, List<ModInfo>> playerItemMods = new Dictionary<string, List<ModInfo>>();
            // public Dictionary<string, Dictionary<string, List<string>>> playerItemMods = new Dictionary<string, Dictionary<string, List<string>>>();

            public Dictionary<Team, List<ItemInfo>> TeamItems = new Dictionary<Team, List<ItemInfo>>();

            public List<ItemInfo> DMItems = new List<ItemInfo>();

            [JsonIgnore]
            public Dictionary<string, List<Item>> PlayerItems = new Dictionary<string, List<Item>>();

            [JsonIgnore]
            public Dictionary<string, List<Item>> removeAfterDeath = new Dictionary<string, List<Item>>();

            public Dictionary<string, int> magSizes = new Dictionary<string, int>();

            public Dictionary<string, float> itemDmgScalars = new Dictionary<string, float>();
            [JsonIgnore]
            public Dictionary<string, Dictionary<string, int>> preJoinCapacity = new Dictionary<string, Dictionary<string, int>>();

            [JsonIgnore]
            public Dictionary<string, ulong> oldTeam = new Dictionary<string, ulong>();

            [JsonIgnore]
            public bool PreGame = false;

            [JsonIgnore]
            public bool StartingGame = false;

            public enum GameType { DM, TDM, CTF };

            public enum Team { Allies, Axis, None };

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(0)]
            public GameType Type = GameType.DM;

            public DateTime GetTimeLeft()
            {
                var timeLimit = GetTimeLimit();
                if (timeLimit <= 0.0f) return DateTime.MinValue;
                return new DateTime(TimeSpan.FromSeconds(timeLimit - GameTime).Ticks);
            }

            public float GetSecondsLeft()
            {
                var timeLimit = GetTimeLimit();
                return timeLimit <= 0f ? 0f : timeLimit - GameTime;
            }

            public void TimeGUI(BasePlayer player)
            {
                if (player == null || this == null || !player.IsConnected || player?.net?.connection == null) return;
                var timeLeft = GetTimeLeft();
                if (timeLeft <= DateTime.MinValue) return;

                var elements = new CuiElementContainer();

                elements.Add(new CuiPanel
                {
                    Image =
                {
                    Color = "1 1 1 0"
                },
                    RectTransform =
                {
                    AnchorMin = "0.849 0.157",
                    AnchorMax = "0.964 0.204"
                }
                }, "Overlay", "DMSGUI");


                var innerPingUIText = new CuiElement
                {
                    Name = CuiHelper.GetGuid(),
                    Parent = "DMSGUI",
                    Components =
                        {
                            new CuiTextComponent { Color = "0.725 0.725 0.725 1", Text = timeLeft.ToString("mm:ss"), FontSize = 22, Align = TextAnchor.MiddleCenter},
                            new CuiRectTransformComponent{ AnchorMin = "0 0", AnchorMax = "1 1" }
                        }
                };
                elements.Add(innerPingUIText);
                CuiHelper.DestroyUi(player, "DMSGUI");
                CuiHelper.AddUi(player, elements);
            }

            public float GetTimeLimit()
            {
                float val;
                if (!TimeLimits.TryGetValue(Type, out val)) val = -1f;
                return val;
            }

            public void TimeGUIAll() { if (ActivePlayers != null && ActivePlayers.Count > 0) foreach (var ply in ActivePlayers) TimeGUI(ply); }

            public void StopTimeGUI(BasePlayer player) { if (player != null && player.IsConnected) CuiHelper.DestroyUi(player, "DMSGUI"); }
            public void StopTimeGUIAll() { if (ActivePlayers != null && ActivePlayers.Count > 0) foreach (var ply in ActivePlayers) StopTimeGUI(ply); }

            public void BroadcastDM(string message, string userID = "0")
            {
                if (string.IsNullOrEmpty(message) || ActivePlayers == null || ActivePlayers.Count < 1) return;
                foreach (var player in ActivePlayers) { if (player != null && player.IsConnected) player.SendConsoleCommand("chat.add", string.Empty, userID, message); }
            }

            public void BroadcastPopup(string message, float duration = 5f)
            {
                if (string.IsNullOrEmpty(message) || ActivePlayers == null || ActivePlayers.Count < 1) return;
                foreach (var player in ActivePlayers)
                {
                    if (player != null && player.IsConnected)
                    {
                        player.SendConsoleCommand("gametip.showgametip \"" + message + "\"");
                        InvokeHandler.Invoke(player, () =>
                        {
                            if (player != null && player.IsConnected) player.SendConsoleCommand("gametip.hidegametip");
                        }, duration);
                    }
                }
            }

            [JsonIgnore]
            public Timer GameTimer
            {
                get { return _gameTimer; }
                set
                {
                    if (_gameTimer != null) _gameTimer.Destroy();
                    _gameTimer = value;
                }
            }

            [JsonIgnore]
            public Timer StartGameTimer
            {
                get { return _startGameTimer; }
                set
                {
                    if (_startGameTimer != null) _startGameTimer.Destroy();
                    _startGameTimer = value;
                }
            }


            [JsonIgnore]
            public Timer CountTimer
            {
                get { return _countTimer; }
                set
                {
                    if (_countTimer != null) _countTimer.Destroy();
                    _countTimer = value;
                }
            }

            [JsonIgnore]
            public Timer StartStormTimer
            {
                get { return _startStormTimer; }
                set
                {
                    if (_startStormTimer != null) _startStormTimer.Destroy();
                    _startStormTimer = value;
                }
            }

            [JsonIgnore]
            private Timer _gameTimer = null;

            [JsonIgnore]
            private Timer _countTimer = null;

            [JsonIgnore]
            private Timer _startGameTimer = null;

            [JsonIgnore]
            private Timer _startStormTimer = null;


            public void StartGame(float time = 0.0f)
            {
                if (time < 0f)
                {
                    Interface.Oxide.LogWarning("StartGame called with negative time: " + time);
                    return;
                }
                if (!RoundBased)
                {
                    Interface.Oxide.LogWarning("StartGame called on non-round based arena: " + ArenaName);
                    return;
                }
                StartingGame = true;
                Interface.Oxide.LogWarning("start game called: " + ArenaName + ", time: " + time + ", type: " + Type + ", round based: " + RoundBased);
                StopTimeGUIAll();
                if (CountTimer != null)
                {
                    CountTimer.Destroy();
                    CountTimer = null;
                }
                if (GameTimer != null)
                {
                    GameTimer.Destroy();
                    GameTimer = null;
                }
                if (StartGameTimer != null)
                {
                    StartGameTimer.Destroy();
                    StartGameTimer = null;
                }



                EndSphere();
                if (time > 0.0)
                {
                    var countTime = 0f;
                    BroadcastDM("<size=18>Game starting in <color=#a6fc60>" + time.ToString("N0") + "</color> seconds</size>");
                    var timerRep = Mathf.Clamp(5f, time < 5f ? 0f : 5f, time);
                    //  Interface.Oxide.LogWarning("timer rep: " + timerRep);
                    CountTimer = DMMain.timer.Every(timerRep, () =>
                    {
                        if (ActivePlayers == null || ActivePlayers.Count < PlayersForStart)
                        {
                            Interface.Oxide.LogWarning("Not enough players to continue count timer (needs: " + PlayersForStart + ")!");
                            BroadcastDM("<size=21>Not enough players to start!</color> This arena requires at least " + PlayersForStart.ToString("N0") + " players before it can start a game.");
                            EndGame();
                            CountTimer.Destroy();
                            CountTimer = null;
                            return;
                        }
                        Interface.Oxide.LogWarning("count timer: " + countTime + ", " + (countTime + 5) + "/" + time);
                        countTime += 5f;
                        if (countTime >= time)
                        {
                            Interface.Oxide.LogWarning("count time >= time: " + countTime + " < " + time + ": " + (countTime < time));
                            var startNowMsg = "<size=20>Game starting <color=#a6fc60>NOW!</color></size>";
                            EnsurePlayerTeams();
                            BroadcastPopup(startNowMsg);
                            BroadcastDM(startNowMsg);
                            CountTimer.Destroy();
                            CountTimer = null;
                            return;
                        }
                        var startMsg = "<size=18>Game starting in <color=#a6fc60>" + (time - countTime).ToString("N0") + "</color> seconds</size>";
                        BroadcastDM(startMsg);
                        BroadcastPopup(startMsg, 4f);
                    });
                    Interface.Oxide.LogWarning("count timer null: " + (CountTimer == null));
                }
                var startAction = new Action(() =>
                {
                    if (ActivePlayers == null || ActivePlayers.Count < PlayersForStart)
                    {
                        Interface.Oxide.LogWarning("no or not enough active players on game start: " + (ActivePlayers?.Count ?? 0) + ", need: " + PlayersForStart);
                        EndGame();
                        return;
                    }
                    Interface.Oxide.LogWarning("start!");
                    foreach (var player in ActivePlayers)
                    {
                        if (player == null) continue;
                        if (player.IsSpectating()) player.StopSpectating();
                        if (DoSpawns(player, SpawnPoint.SpawnType.Base))
                        {
                            if (!CreateItems(player, true)) Interface.Oxide.LogWarning("failed to create items on restart");
                            DMMain?.WakePlayerUp(player, true);
                        }
                        else Interface.Oxide.LogWarning("Failed to do spawns on restart");
                    }
                    if (Fortnite)
                    {
                        BroadcastDM("Storm coming in 120 seconds!");
                        StartStormTimer = DMMain.timer.Once(120f, () =>
                        {
                            if (!GameInProgress) return;
                            StartSphere(true);
                        });
                        // StartSphere(true);
                        ClearSpawnedItems();
                        ClearSpawnedBoxes();
                        RemoveOnEndItems();
                        if (!DropPlayers())
                        {
                            Interface.Oxide.LogWarning("Couldn't drop players! (returned false");
                        }
                        var positions = ItemSpawns?.Select(p => p.Position)?.ToList() ?? null;
                        var toSpawns = new List<ItemSpawn>();
                        for (int i = 0; i < ItemSpawns.Count; i++)
                        {
                            var spawn = ItemSpawns[i];
                            var rng = randomSpawn.Next(0, 101);
                            if (rng <= (spawn.ChanceToSpawn * LootChanceScalar))
                            {
                                Interface.Oxide.LogWarning("rng: " + rng + " <= chance: " + spawn.ChanceToSpawn + ", item: " + spawn.Item.shortName + " x" + spawn.Item.Amount);
                                toSpawns.Add(spawn);
                            }
                        }
                        for (int i = 0; i < BoxSpawns.Count; i++)
                        {
                            var box = BoxSpawns[i];
                            if (box == null) continue;
                            var rng = randomSpawn.Next(0, 101);
                            if (rng <= (box.ChanceToSpawn * LootChanceScalar))
                            {
                                if (box.Position != Vector3.zero)
                                {
                                    if (string.IsNullOrEmpty(box.PrefabName))
                                    {
                                        var blackList = new List<string>();
                                        //if (randomSpawn.Next(0, 101) <= 95)
                                        blackList.Add(DMMain?.ammoBoxPrefab);
                                        if (randomSpawn.Next(0, 101) <= 90) blackList.Add(DMMain?.c4BoxPrefab);
                                        if (randomSpawn.Next(0, 101) <= 25) blackList.Add(DMMain?.medicalBoxPrefab);
                                        var getEnt = box.SpawnRandomEntity(blackList);
                                        // var getEnt = box.GetEntity();
                                        if (getEnt != null && getEnt.ShortPrefabName.Contains("tier3"))
                                        {
                                            var rng2 = randomSpawn.Next(0, 101);
                                            if (rng2 <= 90)
                                            {
                                                //  Interface.Oxide.LogWarning("EnsureWeapon(), rng: " + rng2);
                                                box.EnsureWeapon();
                                            }
                                            else Interface.Oxide.LogWarning("no EnsureWeapon() rng2: " + rng2);
                                        }
                                    }
                                    else box.SpawnEntity();
                                    //       Interface.Oxide.LogWarning("Spawned box entity at: " + box.Position);
                                }
                                else Interface.Oxide.LogWarning("box position is vector3 zero, index: " + i);
                            }
                        }
                        if (toSpawns == null || toSpawns.Count < 1)
                        {
                            // SendReply(player, "No toSpawns ??!");
                            Interface.Oxide.LogWarning("no tospawns means we must be spawning fucking nothing lol");
                            return;
                        }
                        var findSame = toSpawns?.Where(p => toSpawns.Any(x => x != p && (x.Position.ToString() == p.Position.ToString() || Vector3.Distance(x.Position, p.Position) <= 1f)))?.ToList() ?? null;
                        //    var findSame = toSpawns?.Select(p => (toSpawns?.Where(x => ((p?.Item?.GetDefinition()?.category ?? ItemCategory.All) == (x?.Item?.GetDefinition()?.category ?? ItemCategory.All)) && Vector3.Distance(p.Position, x.Position) <= 1f))?.FirstOrDefault() ?? null)?.ToList() ?? null;
                        //              var findSame = toSpawns?.Select(p => (toSpawns?.Where(x => Vector3.Distance(p.Position, x.Position) <= 0.2f)?.FirstOrDefault() ?? null))?.ToList() ?? null;
                        findSame.RemoveAll(p => p == null);
                        findSame.Distinct();
                        //    var findNotSame = toSpawns?.Select(p => (toSpawns?.Where(x => ((p?.Item?.GetDefinition()?.category ?? ItemCategory.All) != (x?.Item?.GetDefinition()?.category ?? ItemCategory.All)) || Vector3.Distance(p.Position, x.Position) > 1f)?.FirstOrDefault() ?? null))?.ToList() ?? null;
                        //      findNotSame.RemoveAll(p => p == null);
                        //        findNotSame.Distinct();

                        if (findSame != null && findSame.Count > 0)
                        {
                            Interface.Oxide.LogWarning("findSame: " + findSame.Count + " count");
                            var lowestChance = findSame?.Where(p => p.ChanceToSpawn > 0f)?.Min(p => p.ChanceToSpawn) ?? 0f;
                            if (lowestChance == 0f)
                            {
                                Interface.Oxide.LogWarning("got lowest chance of 0f!!!");
                                return;
                            }
                            var findUse = findSame?.Where(p => p.ChanceToSpawn <= lowestChance)?.FirstOrDefault() ?? null;
                            if (findUse == null)
                            {
                                Interface.Oxide.LogWarning("findUse is null for chance: " + lowestChance + " !!");
                                return;
                            }
                            Interface.Oxide.LogWarning("got finduse: " + findUse.ChanceToSpawn + "%, " + findUse.Position + ", " + findUse.Item.shortName);
                            var item = ItemManager.CreateByName(findUse.Item.shortName, findUse.Item.Amount, findUse.Item.skinID);
                            if (item == null)
                            {
                                Interface.Oxide.LogWarning("item is null on findUse !!");
                                return;
                            }
                            if (!item.Drop(findUse.Position, Vector3.zero))
                            {
                                Interface.Oxide.LogWarning("no drop item findUse at: " + findUse.Position + " !!");
                                RemoveFromWorld(item);
                                return;
                            }
                            else
                            {
                                var floater = MakeItemFloat(item, findUse.Position);
                                if (item != null)
                                {
                                    SpawnedItems.Add(item);
                                    if (item.info.category == ItemCategory.Ammunition)
                                    {
                                        var sphereCollider = floater.gameObject.AddComponent<SphereTrigger>();
                                        sphereCollider.Radius = 0.925f;
                                        sphereCollider.Name = "ammo";
                                    }
                                    if (item.info.category == ItemCategory.Weapon)
                                    {
                                        Interface.Oxide.LogWarning("Weapon category!");
                                        var baseProj = (item?.GetHeldEntity() ?? null) as BaseProjectile;
                                        if (baseProj != null && baseProj?.primaryMagazine != null)
                                        {
                                            Interface.Oxide.LogWarning("got base proj");
                                            baseProj.primaryMagazine.contents = baseProj.primaryMagazine.capacity;
                                            //     baseProj.SendNetworkUpdate();
                                        }
                                        else Interface.Oxide.LogWarning("no base proj for: " + item.info.shortname);
                                    }
                                }
                                Interface.Oxide.LogWarning("did spawn & float: " + item.info.shortname + " x" + item.amount + ", at: " + floater.transform.position);
                            }
                        }
                        else Interface.Oxide.LogWarning("no find same");
                        for (int i = 0; i < toSpawns.Count; i++)
                        {
                            var findUse = toSpawns[i];
                            if (findUse == null || findSame.Contains(findUse))
                            {
                                //Interface.Oxide.LogWarning("find use null?: " + (findUse == null) + ", findsame contains: " + findSame.Contains(findUse));
                                continue;
                            }
                            //Interface.Oxide.LogWarning("is in notSame: " + findUse.Item.shortName + " x" + findUse.Item.amount + ", " + findUse.Position + ", " + findUse.ChanceToSpawn);
                            var item = ItemManager.CreateByName(findUse.Item.shortName, findUse.Item.Amount, findUse.Item.skinID);
                            if (item == null)
                            {
                                //      Interface.Oxide.LogWarning("item is null on findUse (not same) !!");
                                return;
                            }
                            if (!item.Drop(findUse.Position, Vector3.zero))
                            {
                                //  Interface.Oxide.LogWarning("no drop item findUse (not same) at: " + findUse.Position + " !!");
                                RemoveFromWorld(item);
                                return;
                            }
                            else
                            {
                                var floater = MakeItemFloat(item, findUse.Position);
                                if (item != null)
                                {
                                    SpawnedItems.Add(item);
                                    if (item.info.category == ItemCategory.Ammunition)
                                    {
                                        var sphereCollider = floater.gameObject.AddComponent<SphereTrigger>();
                                        sphereCollider.Radius = 0.925f;
                                        sphereCollider.Name = "ammo";
                                    }
                                    if (item.info.category == ItemCategory.Weapon)
                                    {
                                        //      Interface.Oxide.LogWarning("Weapon category!");
                                        var baseProj = (item?.GetHeldEntity() ?? null) as BaseProjectile;
                                        if (baseProj != null && baseProj?.primaryMagazine != null)
                                        {
                                            //        Interface.Oxide.LogWarning("got base proj");
                                            baseProj.primaryMagazine.contents = baseProj.primaryMagazine.capacity;
                                            //    baseProj.SendNetworkUpdate();
                                        }
                                        //     else Interface.Oxide.LogWarning("no base proj for: " + item.info.shortname);
                                    }
                                }
                                //    Interface.Oxide.LogWarning("did spawn & float not same: " + item.info.shortname + " x" + item.amount + ", at: " + floater.transform.position);
                            }
                        }

                        //          Interface.Oxide.LogWarning("findNotSame: " + (findNotSame?.Count ?? -1));
                    }
                    PreGame = false;
                    StartingGame = false;
                    GameInProgress = true;
                    Interface.Oxide.LogWarning("Starting game timer!");
                    GameTimer = DMMain.timer.Every(1f, () =>
                    {
                        var limit = GetTimeLimit();
                        if (this == null || (limit > 0 && GameTime >= limit))
                        {
                            if (GameTimer != null)
                            {
                                GameTimer.Destroy();
                                GameTimer = null;
                            }
                            return;
                        }
                        GameTime++;
                        TimeGUIAll();
                        if (limit > 10)
                        {
                            var timeLeft = GetSecondsLeft();
                            if (timeLeft < 10)
                            {
                                foreach (var p in ActivePlayers)
                                {
                                    if (p == null || p.IsDestroyed || p.gameObject == null || p.IsDead()) continue;
                                    p.Invoke(() =>
                                    {
                                        SendLocalEffect(p, "assets/prefabs/tools/pager/effects/beep.prefab", 1);
                                    }, 1f);
                                }
                            }
                        }

                        var fnWin = ActivePlayers.Count(p => p != null && p.IsAlive() && !p.IsSpectating()) <= 1;
                        // if (Fortnite) Interface.Oxide.LogWarning("alive & not spectating <= 1: " + fnWin);
                        if ((limit > 0 && GameTime >= limit) || ActivePlayers.Count < 1 || (Fortnite && fnWin && PlayersForStart > 1) || (Type != GameType.DM && !Fortnite && (GetTeamScore(Team.Allies) >= MaxScore || GetTeamScore(Team.Axis) >= MaxScore)))
                        {
                            Interface.Oxide.LogWarning("Game timer end");
                            var alliesKills = GetTeamKills(Team.Allies);
                            var axisKills = GetTeamKills(Team.Axis);
                            var winner = (!Fortnite) ? null : ActivePlayers?.Where(p => p != null && !p.IsDead() && !p.IsSpectating())?.FirstOrDefault() ?? null;
                            var winTeam = Fortnite ? Team.None : (alliesKills > axisKills) ? Team.Allies : (axisKills > alliesKills) ? Team.Axis : Team.None;
                            var winTeamStr = Fortnite ? (winner?.displayName ?? "Unknown") : (alliesKills > axisKills) ? Team.Allies.ToString() : (axisKills > alliesKills) ? Team.Axis.ToString() : "Tie";
                            var winMsg = winTeamStr == "Tie" ? "Round draw" : ("<size=20><color=#62edfc>" + winTeamStr + " win!</color></size>");
                            if (Fortnite && winner != null && winner.IsConnected && winner.IsAlive() && !winner.IsSleeping())
                            {
                                foreach (var ply in ActivePlayers)
                                {
                                    if (ply != winner) DMMain?.SpectatePlayer(ply, winner, true);
                                }
                                winner.SetPlayerFlag(BasePlayer.PlayerFlags.EyesViewmode, true);
                                winner.SendNetworkUpdateImmediate();
                                winner.SignalBroadcast(BaseEntity.Signal.Gesture, "victory", null);
                                winner.Invoke(() =>
                                {
                                    winner.SetPlayerFlag(BasePlayer.PlayerFlags.EyesViewmode, false);
                                    if (winner.IsConnected) winner.SendNetworkUpdateImmediate();
                                }, 1.3f);
                            }

                            BroadcastDM(winMsg);
                            BroadcastPopup(winMsg);

                            foreach (var ply in ActivePlayers)
                            {
                                if (ply == null || ply.IsDestroyed || ply.gameObject == null || ply.IsDead()) continue;
                                var pTeam = GetTeam(ply);
                                if (pTeam == Team.None) continue;

                                var pKills = GetPlayerKills(ply.UserIDString);
                                var pDeaths = GetPlayerDeaths(ply.UserIDString);
                                var kdr = pDeaths < 1 ? pKills : (pKills / (float)pDeaths);
                                var kdrStr = kdr.ToString("0.00").Replace(".00", string.Empty);
                                var finishStr = "You finished with: <color=#62edfc>" + pKills.ToString("N0") + "</color> kills and <color=red>" + pDeaths.ToString("N0") + "</color> deaths. Your KDR was: <color=#62edfc>" + kdrStr + "</color>";

                                if (pTeam == winTeam)
                                {
                                    CelebrateEffects(ply);
                                    ply.ChatMessage("<color=#62edfc>Your team won!</color>" + Environment.NewLine + finishStr);
                                }
                                else
                                {
                                    if (winTeam == Team.None) ply.ChatMessage("<color=orange>Round draw!</color>" + Environment.NewLine + finishStr);
                                    else ply.ChatMessage("<color=red>Your team lost!</color> <color=orange>:(</color>" + Environment.NewLine + finishStr);
                                }

                            }

                            GameInProgress = false;

                            DMMain.timer.Once(10f, () =>
                            {
                                StopTimeGUIAll();
                                Interface.Oxide.LogWarning("end game 10f");
                                EndGame();
                                if (!StartingGame && !GameInProgress && ActivePlayers.Count >= PlayersForStart)
                                {
                                    Interface.Oxide.LogWarning("game end pre-timer!! && activeplayers count >= playersforstart");
                                    StartGame(NewGameTime);
                                }
                            });

                            if (GameTimer != null)
                            {
                                GameTimer.Destroy();
                                GameTimer = null;
                            }
                            StopTimeGUIAll();
                            return;
                        }
                    });
                });
                if (time > 0.0f)
                {
                    Interface.Oxide.LogWarning("timer for start action: " + time);
                    StartGameTimer = DMMain.timer.Once(time, startAction);
                }
                else
                {
                    Interface.Oxide.LogWarning("start action invoke (no timer)");
                    startAction.Invoke();
                }
                //Timer gameTimer = null;
            }

            public void EndGame()
            {
                if (this == null) return;

                PreGame = false;
                StartingGame = false;
                GameInProgress = false;
                GameTime = 0;
                CTFScore = 0;
                TeamKills[Team.Allies] = 0;
                TeamKills[Team.Axis] = 0;
                TeamKills[Team.None] = 0;
                PlayerKills = new Dictionary<string, int>();
                PlayerDeaths = new Dictionary<string, int>();

                /*/
                var pKills = PlayerKills?.ToDictionary(p => p.Key, p => p.Value) ?? null;
                var pDeaths = PlayerDeaths?.ToDictionary(p => p.Key, p => p.Value) ?? null;
                if (pKills != null && pKills.Count > 0) foreach (var kvp in pKills) PlayerKills.Remove(kvp.Key);
                if (pDeaths != null && pDeaths.Count > 0) foreach (var kvp in pDeaths) PlayerDeaths.Remove(kvp.Key);/*/

                if (GameTimer != null) GameTimer = null;
                if (StartStormTimer != null) StartStormTimer = null;

                StopTimeGUIAll();
                EndSphere();
                ClearSpawnedItems();
                ClearSpawnedBoxes();
                RemoveOnEndItems();
                KillCH47s();
                CleanupTeams();



                foreach (var ply in ActivePlayers)
                {
                    if (ply == null) continue;
                    HealPlayer(ply);
                    ClearInventory(ply);
                    if (!CreateItems(ply)) Interface.Oxide.LogWarning("No create items for: " + ply.displayName + " on EndGame()");
                    if (ply.IsSpectating()) DMMain?.StopSpectatePlayer(ply);
                    var tpPos = WaitingArea != Vector3.zero ? WaitingArea : SpawnList?.FirstOrDefault() ?? Vector3.zero;
                    if (tpPos != Vector3.zero) DMMain?.TeleportPlayer(ply, tpPos, true, false);
                }
            }

            public void ClearSpawnedItems()
            {
                if (SpawnedItems == null || SpawnedItems.Count < 1) return;
                foreach (var item in SpawnedItems) RemoveFromWorld(item);
                SpawnedItems.Clear();
            }
            public void ClearSpawnedBoxes()
            {
                if (BoxSpawns == null || BoxSpawns.Count < 1) return;
                for (int i = 0; i < BoxSpawns.Count; i++)
                {
                    var box = BoxSpawns[i];
                    if (box == null) continue;
                    var boxEnt = box?.GetEntity() ?? null;
                    if (boxEnt != null && !boxEnt.IsDestroyed) boxEnt.Kill();
                }
            }
            public void RemoveOnEndItems()
            {
                if (RemoveOnEnd != null || RemoveOnEnd.Count > 0)
                {
                    for (int i = 0; i < RemoveOnEnd.Count; i++) RemoveFromWorld(RemoveOnEnd[i]);
                }
                var itemsOut = BaseNetworkable.serverEntities?.Where(p => p != null && (p is DroppedItem) && DMMain.isEntityInZone(p as BaseEntity, ZoneID))?.ToList() ?? null;
                if (itemsOut != null && itemsOut.Count > 0)
                {
                    for (int i = 0; i < itemsOut.Count; i++)
                    {
                        var item = itemsOut[i];
                        if (item != null && !item.IsDestroyed) item.Kill();
                    }
                }
            }

            public Team GetTeam(BasePlayer player)
            {
                if (player == null || !Teams) return Team.None;
                Team team;
                if (PlayerTeams.TryGetValue(player.UserIDString, out team)) return team;
                else return Team.None;
            }

            public void SetTeam(BasePlayer player, Team team)
            {
                if (player == null || !Teams) return;
                Interface.Oxide.LogWarning("set team: " + team + ", " + player.displayName);
                PlayerTeams[player.UserIDString] = team;
                EnsurePlayerTeams();
            }

            public int GetTeamCount(Team team)
            {
                if (!Teams) return 0;
                var count = 0;
                foreach (var kvp in PlayerTeams)
                {
                    if (kvp.Value == team) count++;
                }
                return count;
            }

            public int GetTeamKills(Team team)
            {
                if (!Teams) return 0;
                var kills = 0;
                foreach (var kvp in TeamKills)
                {
                    if (kvp.Key == team) kills += kvp.Value;
                }
                return kills;
                // return (!Teams) ? 0 : TeamKills?.Where(p => p.Key == team)?.Sum(p => p.Value) ?? 0; 
            }

            public int GetTeamScore(Team team)
            {
                if (!Teams) return 0;
                return Type == GameType.CTF ? -1 : Type == GameType.TDM ? TeamKills?.Where(p => p.Key == team)?.Sum(p => p.Value) ?? 0 : 0;
            }

            public List<BasePlayer> GetTeamPlayers(Team team)
            {
                if (ActivePlayers.Count < 1 || team == Team.None) return new List<BasePlayer>(ActivePlayers);
                var players = new List<BasePlayer>(ActivePlayers.Count);
                foreach (var p in ActivePlayers) { if (GetTeam(p) == team) players.Add(p); }
                return players;
                //  return ActivePlayers?.Where(p => p != null && GetTeam(p) == team)?.ToList() ?? null; 
            }


            public void AddTeamKills(Team team, int valueToAdd)
            {
                if (!Teams) return;
                int kills;
                if (!TeamKills.TryGetValue(team, out kills)) TeamKills[team] = valueToAdd;
                else TeamKills[team] = kills + valueToAdd;
            }

            public void AddPlayerKills(string userID, int valueToAdd)
            {
                if (string.IsNullOrEmpty(userID)) return;
                int kills;
                if (!PlayerKills.TryGetValue(userID, out kills)) PlayerKills[userID] = valueToAdd;
                else PlayerKills[userID] = kills + valueToAdd;
            }

            public int GetPlayerKills(string userID)
            {
                if (string.IsNullOrEmpty(userID)) return 0;
                int kills;
                PlayerKills.TryGetValue(userID, out kills);
                return kills;
            }

            public void AddPlayerDeaths(string userID, int valueToAdd)
            {
                if (string.IsNullOrEmpty(userID)) return;
                int deaths;
                if (!PlayerDeaths.TryGetValue(userID, out deaths)) PlayerDeaths[userID] = valueToAdd;
                else PlayerDeaths[userID] = deaths + valueToAdd;
            }

            public int GetPlayerDeaths(string userID)
            {
                if (string.IsNullOrEmpty(userID)) return 0;
                int deaths;
                PlayerDeaths.TryGetValue(userID, out deaths);
                return deaths;
            }

            public Team GetBestTeam()
            {
                if (!Teams)
                {
                    Interface.Oxide.LogWarning("GetBestTeam called with Teams set to false!");
                    return Team.None;
                }
                var alliesCount = 0;
                var axisCount = 0;
                foreach (var p in PlayerTeams)
                {
                    if (p.Value == Team.Allies) alliesCount++;
                    else if (p.Value == Team.Axis) axisCount++;
                }
                if (alliesCount == axisCount) Interface.Oxide.LogWarning("equal team count: " + alliesCount);

                if (Type == GameType.TDM)
                {
                    var alliesKills = GetTeamKills(Team.Allies);
                    var axisKills = GetTeamKills(Team.Axis);
                    Interface.Oxide.LogWarning("Allies kills: " + alliesKills + ", axis: " + axisKills);
                    if (alliesCount == axisCount && (alliesKills == axisKills))
                    {
                        Interface.Oxide.LogWarning("same kills AND same count: " + alliesCount + ", kills: " + alliesKills);
                        return (Team)UnityEngine.Random.Range(0, 2);
                    }
                    if (alliesKills < axisKills && (alliesCount <= axisCount))
                    {
                        Interface.Oxide.LogWarning("allies kills < axis, allies count <= axiscount, " + alliesKills + ", " + axisKills + ", " + alliesCount + ", " + axisCount);
                        return Team.Allies;
                    }
                    if (axisKills < alliesKills && (axisCount <= alliesCount))
                    {
                        Interface.Oxide.LogWarning("axis kills < allies, axis count <= aliiescount: " + axisKills + ", " + alliesKills + ", " + axisCount + ", " + alliesCount);
                        return Team.Axis;
                    }
                    if (alliesKills == axisKills && (alliesCount < axisCount))
                    {
                        Interface.Oxide.LogWarning("allies kill equal axis, allies count < axis: " + alliesKills + ", count: " + alliesCount + ", " + axisCount);
                        return Team.Allies;
                    }
                    if (axisKills == alliesKills && (axisCount < alliesCount))
                    {
                        Interface.Oxide.LogWarning("axis kills equal allies, axis count < allies count, " + axisKills + ", count:" + axisCount + ", " + alliesCount);
                        return Team.Axis;
                    }
                    if (alliesKills >= axisKills && (alliesCount < axisCount))
                    {
                        Interface.Oxide.LogWarning("alliesKills >= axisKills, alliesCount < axisCount: " + alliesKills + ", " + axisKills + ", " + alliesCount + ", " + axisCount);
                        return Team.Allies;
                    }
                    if (axisKills >= alliesKills && (axisCount < alliesCount))
                    {
                        Interface.Oxide.LogWarning("axisKills >= alliesKills, axisCount < alliescount: " + axisKills + ", " + alliesKills + ", " + axisCount + ", " + alliesCount);
                        return Team.Axis;
                    }
                    /*/
                    if (((axisCount + 1) - alliesCount) > 1)
                    {
                        Interface.Oxide.LogWarning("axisCount + 1 (" + (axisCount + 1) + ") - alliesCount (" + alliesCount + ") > 1, use team allies");
                        return Team.Allies;
                    }
                    if (((alliesCount + 1) - axisCount) > 1)
                    {
                        Interface.Oxide.LogWarning("alliesCount + 1 (" + (alliesCount + 1) + ") - axisCount (" + axisCount + ") > 1, use team axis");
                        return Team.Axis;
                    }/*/ //unsure if this is retarded or not ^
                    if (axisCount > alliesCount)
                    {
                        Interface.Oxide.LogWarning("axisCount (" + axisCount + ") > alliesCount (" + alliesCount + ")");
                        return Team.Allies;
                    }
                    if (alliesCount > axisCount)
                    {
                        Interface.Oxide.LogWarning("alliesCount (" + alliesCount + ") > axisCount (" + axisCount + ")");
                        return Team.Axis;
                    }

                    Interface.Oxide.LogWarning("got no team, allies count: " + alliesCount + ", axis: " + axisCount + ", kills respectively: " + alliesKills + ", " + axisKills);
                    return (Team)UnityEngine.Random.Range(0, 2);
                }
                else Interface.Oxide.LogWarning("Returning a team for non-TDM team based gamemode");
                Interface.Oxide.LogWarning("got no team, returning rng");
                return (Team)UnityEngine.Random.Range(0, 2);
            }


            public bool DoSpawns(BasePlayer player, SpawnPoint.SpawnType type = SpawnPoint.SpawnType.None)
            {
                if (player == null || player.IsDestroyed || !(player?.IsConnected ?? false)) return false;
                var spawnPoint = GetSmartSpawn(player, type);
                if (spawnPoint == Vector3.zero) return false;
                if (player.IsDead())
                {
                    //player.RespawnAt(spawnPoint, Quaternion.identity);
                    RespawnAt(player, spawnPoint, Quaternion.identity, true, 250); //custom respawn option to disable loading
                    player.Invoke(() =>
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (player == null) break;
                            DMMain?.TeleportPlayer(player, spawnPoint, false, false);
                        }
                    }, 0.1f); //tp multiple times to try to avoid strange bug
                }
                else
                {
                    DMMain.TeleportPlayer(player, spawnPoint, true, false);
                    player.Invoke(() =>
                    {
                        DMMain?.TeleportPlayer(player, spawnPoint, true, false);
                    }, 0.1f); //tp twice to try to avoid strange bug
                }
                HealPlayer(player, Mathf.Clamp(SpawnWithHealth, 1, 100));
                return true;
            }

            public void ClearSpawns()
            {
                if (spawnPoints != null) spawnPoints.Clear();
                else spawnPoints = new List<SpawnPoint>();
            }

            public void ClearTeamSpawns(Team team)
            {
                List<SpawnPoint> teamSpawns;
                if (teamSpawnPoints.TryGetValue(team, out teamSpawns)) teamSpawnPoints[team].Clear();
                else teamSpawnPoints[team] = new List<SpawnPoint>();
            }

            public void SetAmmo(string userID, string itemShortName, string ammoName)
            {
                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(itemShortName) || string.IsNullOrEmpty(ammoName)) return;
                var outDict = new Dictionary<string, string>();
                if (!playerAmmos.TryGetValue(userID, out outDict)) outDict = new Dictionary<string, string>();
                outDict[itemShortName] = ammoName;
                playerAmmos[userID] = outDict;
            }


            public void ClearAmmo(string userID, string itemShortName)
            {
                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(itemShortName)) return;
                var outDict = new Dictionary<string, string>();
                if (!playerAmmos.TryGetValue(userID, out outDict)) return;
                string ammoName;
                if (outDict.TryGetValue(itemShortName, out ammoName)) playerAmmos[userID].Remove(itemShortName);
            }

            public string GetAmmoS(string userID, string itemShortName)
            {
                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(itemShortName)) return string.Empty;
                var ammoName = string.Empty;
                var outDict = new Dictionary<string, string>();
                if (playerAmmos.TryGetValue(userID, out outDict)) outDict.TryGetValue(itemShortName, out ammoName);
                return ammoName;
            }

            public ItemDefinition GetAmmo(string userID, string itemShortName)
            {
                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(itemShortName)) return null;
                var ammoStr = GetAmmoS(userID, itemShortName);
                if (string.IsNullOrEmpty(ammoStr)) return null;
                return ItemManager.FindItemDefinition(ammoStr);
            }

            public ulong GetSkin(string userID, string shortName)
            {
                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(shortName)) return 0;
                var skinID = 0ul;
                Dictionary<string, ulong> pSkins;
                if (!playerItemSkins.TryGetValue(userID, out pSkins)) return skinID;
                if (!pSkins.TryGetValue(shortName, out skinID)) return 0;
                return skinID;
            }

            public void SetSkin(string userID, string shortName, ulong skinID)
            {
                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(shortName)) return;
                Dictionary<string, ulong> pSkins;
                if (!playerItemSkins.TryGetValue(userID, out pSkins)) pSkins = new Dictionary<string, ulong>();
                pSkins[shortName] = skinID;
                playerItemSkins[userID] = pSkins;
            }

            public void ResetSkin(string userID, string shortName)
            {
                Dictionary<string, ulong> pSkins;
                ulong skinID;
                if (!playerItemSkins.TryGetValue(userID, out pSkins) || !pSkins.TryGetValue(shortName, out skinID) || skinID == 0) return;
                playerItemSkins[userID].Remove(shortName);
            }

            public string GetSavedItemName(string userId, string shortName)
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(shortName)) return string.Empty;
                Dictionary<string, string> pNames;
                string name;
                if (!playerItemNames.TryGetValue(userId, out pNames)) return string.Empty;
                if (pNames.TryGetValue(shortName, out name)) return name;
                return string.Empty;
            }

            public void SetSavedItemName(string userId, string shortName, string customName)
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(shortName)) return;
                Dictionary<string, string> pNames;
                if (!playerItemNames.TryGetValue(userId, out pNames)) playerItemNames[userId] = new Dictionary<string, string>();
                playerItemNames[userId][shortName] = customName;
            }

            public void ResetSavedItemName(string userId, string shortName)
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(shortName)) return;
                Dictionary<string, string> pNames;
                string custom;
                if (playerItemNames.TryGetValue(userId, out pNames) && pNames.TryGetValue(shortName, out custom)) pNames.Remove(shortName);
            }



            public int GetMagSizeS(string shortName)
            {
                if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException();
                int magSize;
                if (!magSizes.TryGetValue(shortName, out magSize)) return -1;
                return magSize;
            }

            public int GetMagSize(ItemDefinition item) { return GetMagSizeS(item?.shortname); }

            public void SetMagSizeS(string shortName, int capacity)
            {
                if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException();
                magSizes[shortName] = Mathf.Clamp(capacity, 0, int.MaxValue);
            }

            public void SetMagSize(ItemDefinition item, int capacity) => SetMagSizeS(item?.shortname, capacity);

            public void ResetMagSizeS(string shortName) { if (GetMagSizeS(shortName) != -1) magSizes.Remove(shortName); }

            public void ResetMagSize(ItemDefinition item) => ResetMagSizeS(item?.shortname);

            public float GetDmgScalarS(string shortName)
            {
                if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException();
                float scalar;
                if (!itemDmgScalars.TryGetValue(shortName, out scalar)) return -1f;
                return scalar;
            }

            public float GetDmgScalar(ItemDefinition item) { return GetDmgScalarS(item?.shortname); }

            public void SetDmgScalarS(string shortName, float scalar)
            {
                if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException();
                itemDmgScalars[shortName] = Mathf.Clamp(scalar, 0.0f, float.MaxValue);
            }

            public void SetDmgScalar(ItemDefinition item, float scalar) => SetDmgScalarS(item?.shortname, scalar);

            public void ResetDmgScalarS(string shortName) { if (GetDmgScalarS(shortName) != -1f) itemDmgScalars.Remove(shortName); }

            public void ResetDmgScalar(ItemDefinition item) => ResetDmgScalarS(item?.shortname);

            public bool TransferCorpse(LootableCorpse corpse, BasePlayer target)
            {
                if (corpse == null || corpse.IsDestroyed || corpse?.containers == null || corpse.containers.Length < 1 || target == null || target.IsDestroyed || target.IsDead() || target?.inventory == null) return false;
                Interface.Oxide.LogWarning("Corpse containers length is: " + corpse?.containers?.Length);
                for (int i = 0; i < corpse.containers.Length; i++)
                {
                    var container = corpse.containers[i];
                    Interface.Oxide.LogWarning("Container itemlist count is: " + container?.itemList?.Count);
                    if (container?.itemList != null && container.itemList.Count > 0)
                    {
                        for (int j = 0; j < container.itemList.Count; j++)
                        {
                            var item = container.itemList[j];
                            if (item == null) continue;
                            if (!item.MoveToContainer(target.inventory.containerMain) && !item.MoveToContainer(target.inventory.containerBelt) && !item.MoveToContainer(target.inventory.containerWear)) RemoveFromWorld(item);
                            else Interface.Oxide.LogWarning("Moved item successfully: " + item?.info?.shortname + " x" + item?.amount);
                        }
                    }
                }
                return true;
            }

            public bool SortAndRefreshItems(BasePlayer player)
            {
                if (player == null || player.IsDestroyed || (player?.IsDead() ?? true)) return false;
                try
                {


                    //var allItems = new List<Item>(pItems);



                    List<ItemPosition> dmPosList;

                    if (!dmItemPositions.TryGetValue(player.UserIDString, out dmPosList)) dmPosList = new List<ItemPosition>();



                    if (dmPosList == null || dmPosList.Count < 1) Interface.Oxide.LogWarning("dmPosList is null/empty on SortAndRefreshItems for: " + player?.displayName + " (" + player?.UserIDString + ")");

                    List<ModInfo> allMods;
                    if (!playerModInfos.TryGetValue(player.UserIDString, out allMods)) allMods = new List<ModInfo>();

                    var pTeam = GetTeam(player);
                    var infos = GetTeamItemInfos(pTeam);
                    if (infos == null || infos.Count < 1) return false;

                    var maxBeltPos = -1;
                    var maxMainPos = -1;
                    var maxWearPos = -1;

                    var beltInfos = infos?.Where(p => p != null && p.containerName == "belt");
                    var mainInfos = infos?.Where(p => p != null && p.containerName == "main");
                    var wearInfos = infos?.Where(p => p != null && p.containerName == "wear");
                    if (beltInfos != null && beltInfos.Any()) maxBeltPos = beltInfos.Max(p => p.itemPos);
                    if (mainInfos != null && mainInfos.Any()) maxMainPos = mainInfos.Max(p => p.itemPos);
                    if (wearInfos != null && wearInfos.Any()) maxWearPos = wearInfos.Max(p => p.itemPos);

                    if (maxBeltPos != -1 && maxBeltPos > (player.inventory.containerBelt.capacity - 1)) player.inventory.containerBelt.capacity = maxBeltPos + 1;
                    if (maxMainPos != -1 && maxMainPos > (player.inventory.containerMain.capacity - 1)) player.inventory.containerMain.capacity = maxMainPos + 1;
                    if (maxWearPos != -1 && maxWearPos > (player.inventory.containerWear.capacity - 1)) player.inventory.containerWear.capacity = maxWearPos + 1;

                    var withPositions = new Dictionary<Item, ItemPosition>();
                    var withoutPositions = new Dictionary<Item, ItemInfo>();

                    var allItems = Pool.GetList<Item>();
                    try
                    {
                        player?.inventory?.AllItemsNoAlloc(ref allItems);

                        for (int i = 0; i < allItems.Count; i++)
                        {
                            var item = allItems[i];
                            if (item != null) item.RemoveFromContainer();
                        }

                        for (int i = 0; i < infos.Count; i++)
                        {
                            var item = infos[i];
                            if (item == null)
                            {
                                Interface.Oxide.LogWarning("null item in infos!!!");
                                continue;
                            }

                            Item newItem = null;
                            for (int j = 0; j < allItems.Count; j++)
                            {
                                var item2 = allItems[j];
                                if (item2?.info?.shortname == item.shortName)
                                {
                                    newItem = item2;
                                    break;
                                }
                            }

                            if (newItem == null) newItem = ItemManager.CreateByName(item.shortName, item.Amount, item.skinID);
                            if (newItem == null)
                            {
                                Interface.Oxide.LogWarning("newItem still null after attempting to create!");
                                continue;
                            }

                            if (newItem?.contents?.itemList != null && newItem.contents.itemList.Count > 0)
                            {
                                for (int j = 0; j < newItem.contents.itemList.Count; j++)
                                {
                                    var mod = newItem.contents.itemList[j];
                                    if (mod != null) mod.RepairCondition(999f);
                                }
                            }


                            if (newItem?.info?.itemid == 143803535 && DMMain?.grenadeCook != null) DMMain.grenadeCook.Remove(newItem.uid);


                            var pSkin = GetSkin(player.UserIDString, item.shortName);
                            if (pSkin == 0) pSkin = item.skinID;

                            var pName = GetSavedItemName(player.UserIDString, item.shortName);
                            if (!string.IsNullOrEmpty(pName)) newItem.name = pName;

                            //ITEM REFRESH PROCESS//
                            newItem.text = "DM Item";
                            newItem.skin = pSkin;
                            if (newItem.amount != item.Amount)
                            {
                                newItem.amount = item.Amount;
                                newItem.MarkDirty();
                            }
                            if (newItem.amount < 1) Interface.Oxide.LogWarning("newItem: " + newItem?.info?.shortname + " has < 1 amount!!! " + newItem.amount);

                            var itemPosList = new List<ItemPosition>();
                            for (int j = 0; j < dmPosList.Count; j++)
                            {
                                var p = dmPosList[j];
                                if (p?.itemID == newItem.info.itemid) itemPosList.Add(p);
                            }
                            var heldEnt = newItem?.GetHeldEntity() ?? null;
                            //   var itemPosList = dmPosList?.Where(p => p.itemID == newItem.info.itemid)?.ToList() ?? null; //use list because there's a possibility of two items having different positions(?)
                            var baseProj = heldEnt as BaseProjectile;
                            if (baseProj != null)
                            {
                                var ammoDef = GetAmmo(player.UserIDString, newItem.info.shortname);
                                if (ammoDef == null) ammoDef = ItemManager.FindItemDefinition(item?.ammoName ?? string.Empty) ?? null;
                                if (ammoDef != null) baseProj.primaryMagazine.ammoType = ammoDef;
                                if (baseProj?.primaryMagazine != null)
                                {
                                    var getCap = GetMagSize(newItem.info);
                                    if (getCap != -1) baseProj.primaryMagazine.capacity = getCap;
                                    baseProj.primaryMagazine.contents = item?.ammoAmount ?? 0;
                                    //          baseProj.primaryMagazine.contents = baseProj.primaryMagazine.capacity;
                                }
                                baseProj.skinID = pSkin;
                                baseProj.SendNetworkUpdate();
                            }
                            var chainsaw = heldEnt as Chainsaw;
                            if (chainsaw != null)
                            {
                                chainsaw.ammo = item?.ammoAmount ?? 0;
                                chainsaw.SendNetworkUpdate();
                            }


                            List<ModInfo> itemMods;
                            // ModInfo itemMods;
                            itemMods = allMods?.Where(p => p?.parentShortName == newItem?.info?.shortname)?.ToList() ?? null;
                            //      if (!allMods.TryGetValue(newItem.info.shortname, out itemMods)) itemMods = null;
                            //  var itemMods = playerModInfos?.Where(p => p?.shortName == newItem.info.shortname)?.ToList() ?? new List<ModInfo>();

                            if (newItem?.contents != null)
                            {
                                if (item?.mods?.Count > 0 && (itemMods == null || itemMods.Count < 1)) //item.mods is mods that are saved in the dm via /dm_edit items, itemMods list is players' mods/selected mods
                                {
                                    Interface.Oxide.LogWarning("itemMods.Count < 1");
                                    var modItems = item?.GetModItems() ?? null;
                                    if (modItems != null && modItems.Count > 0)
                                    {
                                        for (int j = 0; j < modItems.Count; j++)
                                        {
                                            var modItem = modItems[j];
                                            if (!modItem.MoveToContainer(newItem.contents)) RemoveFromWorld(modItem);
                                        }
                                    }
                                }
                                else if (itemMods != null && itemMods.Count > 0)
                                {
                                    for (int j = 0; j < itemMods.Count; j++)
                                    {
                                        var modItem = itemMods[j];
                                        var contentList = newItem?.contents?.itemList;
                                        var hasMod = false;
                                        if (contentList != null && contentList.Count > 0)
                                        {
                                            for (int k = 0; k < contentList.Count; k++)
                                            {
                                                if (contentList[k]?.info?.shortname == modItem?.shortName)
                                                {
                                                    hasMod = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (hasMod) continue;
                                        var getMod = modItem?.GetItem() ?? null;
                                        if (getMod != null && !getMod.MoveToContainer(newItem.contents))
                                        {
                                            Interface.Oxide.LogWarning("no move player mod item: " + getMod?.info?.shortname + " x" + getMod?.amount + ", newItem: " + newItem?.info?.shortname + " x" + newItem?.amount);
                                            RemoveFromWorld(getMod);
                                        }
                                    }
                                }
                            }
                            //END ITEM REFRESH PROCESS//

                            if (itemPosList == null || itemPosList.Count < 1) withoutPositions[newItem] = item;
                            else
                            {
                                ItemPosition pos;
                                if (!withPositions.TryGetValue(newItem, out pos))
                                {
                                    for (int j = 0; j < itemPosList.Count; j++)
                                    {
                                        var pos2 = itemPosList[j];
                                        if (pos2 == null || pos2.itemID != newItem?.info?.itemid) continue;
                                        withPositions[newItem] = pos2;
                                    }
                                }
                            }

                        }
                    }
                    finally { Pool.FreeList(ref allItems); }


                    foreach (var kvp in withPositions.ToDictionary(p => p.Key, p => p.Value))
                    {
                        var item = kvp.Key;
                        var pos = kvp.Value;
                        if (pos == null)
                        {
                            Interface.Oxide.LogWarning("pos is null in kvp");
                            RemoveFromWorld(item);
                            continue;
                        }

                        var cont = DMMain.GetContainerFromName(player, pos.containerName);
                        if (cont == null)
                        {
                            Interface.Oxide.LogWarning("null container on itemposlist loop");
                            continue;
                        }

                        if (!item.MoveToContainer(cont, pos.position))
                        {
                            var getSlot = cont?.GetSlot(pos.position) ?? null;
                            Interface.Oxide.LogWarning("failed to move to specific position: " + item?.info?.shortname + ", cont: " + pos.containerName + ", pos: " + pos.position + ", newItem has parent?: " + (item?.parent != null) + ", slot taken by: " + (getSlot?.info?.shortname ?? "nothing"));
                            if (!item.MoveToContainer(cont))
                            {
                                Interface.Oxide.LogWarning("failed to move to specific container in any position: " + item?.info?.shortname);
                                if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerWear) && !item.MoveToContainer(player.inventory.containerBelt))
                                {
                                    Interface.Oxide.LogWarning("failed to move item to any container in any position at all: " + item?.info?.shortname);
                                    RemoveFromWorld(item);
                                }
                            }
                        }
                    }

                    foreach (var kvp in withoutPositions.ToDictionary(p => p.Key, p => p.Value))
                    {
                        var item = kvp.Value;
                        var newItem = kvp.Key;
                        var contMove = DMMain.GetContainerFromName(player, item?.containerName ?? string.Empty);
                        if (contMove != null)
                        {
                            var itemPosi = item?.itemPos ?? -1;


                            if (!newItem.MoveToContainer(contMove, itemPosi) && !newItem.MoveToContainer(player.inventory.containerMain) && !newItem.MoveToContainer(player.inventory.containerBelt))
                            {
                                newItem.RemoveFromContainer();
                                if (!newItem.MoveToContainer(contMove, itemPosi) && !newItem.MoveToContainer(player.inventory.containerMain) && !newItem.MoveToContainer(player.inventory.containerBelt))
                                {
                                    Interface.Oxide.LogWarning("no move newitem to contMove (or any cont): " + item?.containerName + ", item: " + newItem?.info?.shortname + " x" + newItem?.amount + ", itemPosi: " + itemPosi + ", parent null?: " + (newItem?.parent == null));
                                    RemoveFromWorld(newItem);
                                }
                            }

                        }
                        else
                        {
                            Interface.Oxide.LogWarning("null cont itempos null: " + item?.containerName);
                            RemoveFromWorld(newItem);
                        }
                    }

                    var finalList = Pool.GetList<Item>();
                    try
                    {
                        player?.inventory?.AllItemsNoAlloc(ref finalList);

                        List<Item> removeOnDeathList = null;
                        if (RemoveItemsOnDeath)
                        {
                            if (!removeAfterDeath.TryGetValue(player.UserIDString, out removeOnDeathList)) removeOnDeathList = new List<Item>();
                            else removeOnDeathList.Clear();
                        }


                        if (finalList?.Count > 0 && DMMain?.DMowners != null)
                        {
                            for (int i = 0; i < finalList.Count; i++)
                            {
                                var item = finalList[i];
                                if (item != null)
                                {
                                    if (RemoveItemsOnDeath) removeOnDeathList.Add(item);

                                    DMMain.DMowners[item] = player.userID;
                                }
                            }
                        }

                        if (RemoveItemsOnDeath) removeAfterDeath[player.UserIDString] = removeOnDeathList;

                    }
                    finally { Pool.FreeList(ref finalList); }

                    return true;
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^Failed to complete SortAndRefreshItems^");
                    return false;
                }
            }

            public bool CreateItems(BasePlayer player, bool emptyInv = false)
            {
                if (player == null || player.IsDestroyed || player?.inventory == null || player.IsDead()) return false;
                if (emptyInv)
                {
                    ClearInventory(player);

                    //  var pItems = player?.inventory?.AllItems()?.ToList() ?? null;
                    //    if (pItems != null && pItems.Count > 0) for (int i = 0; i < pItems.Count; i++) RemoveFromWorld(pItems[i]);
                }
                var errSB = new StringBuilder();
                try
                {

                    List<ItemPosition> dmPosList = null;
                    if (dmItemPositions != null && !dmItemPositions.TryGetValue(player.UserIDString, out dmPosList)) dmPosList = new List<ItemPosition>();

                    if (dmPosList == null || dmPosList.Count < 1)
                    {
                        Interface.Oxide.LogWarning("dmPosList is null/empty!");
                    }

                    errSB.AppendLine("Got dmPosList");

                    List<ModInfo> allMods;
                    if (!playerModInfos.TryGetValue(player.UserIDString, out allMods)) allMods = new List<ModInfo>();

                    var pTeam = GetTeam(player);
                    var infos = GetTeamItemInfos(pTeam);
                    if (infos == null || infos.Count < 1)
                    {
                        Interface.Oxide.LogWarning("Item infos is null/empty!");
                        return false;
                    }
                    errSB.AppendLine("Got team item infos");
                    var maxBeltPos = -1;
                    var maxMainPos = -1;
                    var maxWearPos = -1;

                    var beltInfos = infos?.Where(p => p != null && p.containerName == "belt");
                    var mainInfos = infos?.Where(p => p != null && p.containerName == "main");
                    var wearInfos = infos?.Where(p => p != null && p.containerName == "wear");
                    if (beltInfos != null && beltInfos.Any()) maxBeltPos = beltInfos.Max(p => p.itemPos);
                    if (mainInfos != null && mainInfos.Any()) maxMainPos = mainInfos.Max(p => p.itemPos);
                    if (wearInfos != null && wearInfos.Any()) maxWearPos = wearInfos.Max(p => p.itemPos);

                    if (maxBeltPos != -1 && maxBeltPos > (player.inventory.containerBelt.capacity - 1)) player.inventory.containerBelt.capacity = maxBeltPos + 1;
                    if (maxMainPos != -1 && maxMainPos > (player.inventory.containerMain.capacity - 1)) player.inventory.containerMain.capacity = maxMainPos + 1;
                    if (maxWearPos != -1 && maxWearPos > (player.inventory.containerWear.capacity - 1)) player.inventory.containerWear.capacity = maxWearPos + 1;

                    errSB.AppendLine("got item positions/capacities");

                    for (int i = 0; i < infos.Count; i++)
                    {
                        try
                        {
                            var item = infos[i];
                            if (item == null) continue;
                            errSB.AppendLine("Got to infos[" + i + "]");
                            var pSkin = GetSkin(player.UserIDString, item.shortName);
                            if (pSkin == 0) pSkin = item.skinID;
                            //ITEM CREATION PROCESS//
                            var newItem = ItemManager.CreateByName(item.shortName, item?.Amount ?? 1, pSkin);
                            if (newItem == null) continue;
                            newItem.text = "DM Item";

                            var itemPosList = new List<ItemPosition>();
                            var itemId = newItem?.info?.itemid ?? 0;
                            if (dmPosList != null && dmPosList.Count > 0)
                            {
                                for (int j = 0; j < dmPosList.Count; j++)
                                {
                                    var p = dmPosList[j];
                                    if (p == null) continue;
                                    if (p.itemID == itemId) itemPosList.Add(p);

                                }
                            }

                            var heldEnt = newItem?.GetHeldEntity() ?? null;
                            //   var itemPosList = dmPosList?.Where(p => p.itemID == newItem.info.itemid)?.ToList() ?? null; //use list because there's a possibility of two items having different positions(?)
                            var baseProj = heldEnt as BaseProjectile;
                            if (baseProj != null)
                            {
                                var ammoDef = GetAmmo(player.UserIDString, newItem.info.shortname);
                                if (ammoDef == null) ammoDef = ItemManager.FindItemDefinition(item?.ammoName ?? string.Empty) ?? null;
                                if (ammoDef != null) baseProj.primaryMagazine.ammoType = ammoDef;
                                if (baseProj?.primaryMagazine != null)
                                {
                                    var getCap = GetMagSize(newItem.info);
                                    if (getCap != -1) baseProj.primaryMagazine.capacity = getCap;
                                    baseProj.primaryMagazine.contents = item?.ammoAmount ?? 0;
                                    //          baseProj.primaryMagazine.contents = baseProj.primaryMagazine.capacity;
                                }
                            }
                            var chainsaw = heldEnt as Chainsaw;
                            if (chainsaw != null)
                            {
                                chainsaw.ammo = item?.ammoAmount ?? 0;
                                chainsaw.SendNetworkUpdate();
                            }

                            List<ModInfo> itemMods;
                            // ModInfo itemMods;
                            itemMods = allMods?.Where(p => p?.parentShortName == newItem?.info?.shortname)?.ToList() ?? new List<ModInfo>();
                            //      if (!allMods.TryGetValue(newItem.info.shortname, out itemMods)) itemMods = null;
                            //  var itemMods = playerModInfos?.Where(p => p?.shortName == newItem.info.shortname)?.ToList() ?? new List<ModInfo>();

                            if (newItem?.contents != null)
                            {
                                if (item?.mods?.Count > 0 && (itemMods == null || itemMods.Count < 1)) //item.mods is mods that are saved in the dm via /dm_edit items, itemMods list is players' mods/selected mods
                                {
                                    Interface.Oxide.LogWarning("itemMods.Count < 1");
                                    var modItems = item?.GetModItems() ?? null;
                                    if (modItems != null && modItems.Count > 0)
                                    {
                                        for (int j = 0; j < modItems.Count; j++)
                                        {
                                            var modItem = modItems[j];
                                            if (!modItem.MoveToContainer(newItem.contents)) RemoveFromWorld(modItem);
                                        }
                                    }
                                }
                                else if (itemMods != null && itemMods.Count > 0)
                                {
                                    for (int j = 0; j < itemMods.Count; j++)
                                    {
                                        var modItem = itemMods[j];
                                        var getMod = modItem?.GetItem() ?? null;
                                        if (getMod != null && !getMod.MoveToContainer(newItem.contents))
                                        {
                                            Interface.Oxide.LogWarning("no move player mod item!");
                                            RemoveFromWorld(getMod);
                                        }
                                    }
                                }
                            }
                            //END ITEM CREATION PROCESS//

                            //ITEM MOVING PROCESS//
                            if (itemPosList == null || itemPosList.Count < 1)
                            {
                                var contMove = DMMain.GetContainerFromName(player, item?.containerName ?? string.Empty);
                                if (contMove != null)
                                {
                                    var itemPosi = item?.itemPos ?? -1;
                                    /*/
                                    if ((itemPosi + 1) > contMove.capacity)
                                    {
                                        Interface.Oxide.LogWarning("itemPosi + 1 (" + (itemPosi + 1) + ") < contMove capacity: " + contMove.capacity + ", setting!");
                                        contMove.capacity = itemPosi + 1;

                                    }/*/
                                    if (!newItem.MoveToContainer(contMove, itemPosi) && !newItem.MoveToContainer(player.inventory.containerMain) && !newItem.MoveToContainer(player.inventory.containerBelt)) RemoveFromWorld(newItem);
                                }
                                else
                                {
                                    Interface.Oxide.LogWarning("null cont itempos null: " + item?.containerName);
                                    RemoveFromWorld(newItem);
                                }
                            }
                            else
                            {
                                var contMove = player?.inventory?.containerBelt ?? null;
                                if (contMove == null) return false;
                                for (int j = 0; j < itemPosList.Count; j++)
                                {
                                    var pos = itemPosList[j];
                                    if (pos == null)
                                    {
                                        Interface.Oxide.LogWarning("pos == null");
                                        continue;
                                    }
                                    var itemPos = pos?.position ?? -1;
                                    if (itemPos > contMove.capacity)
                                    {
                                        Interface.Oxide.LogWarning("itemPos > contMove.capacity! " + itemPos + " > " + contMove.capacity);
                                        itemPos = contMove.capacity;
                                    }
                                    if (itemPos < 0)
                                    {
                                        Interface.Oxide.LogWarning("itemPos < 0!!! " + itemPos);
                                    }
                                    var getSlot = player?.inventory?.containerBelt?.GetSlot(itemPos) ?? null;
                                    if (getSlot != null)
                                    {
                                        if (getSlot.info.itemid != newItem.info.itemid)
                                        {
                                            //workaround for above code moving stuff
                                            //       Interface.Oxide.LogWarning("something has taken slot: " + itemPos + ", item name: " + getSlot.info.shortname + ", we are going to override it with " + newItem.info.shortname);
                                            if (!getSlot.MoveToContainer(player.inventory.containerMain)) getSlot.Drop(player.GetDropPosition(), player.GetDropVelocity());
                                        }

                                    }
                                    if (!newItem.MoveToContainer(player.inventory.containerBelt, itemPos))
                                    {
                                        if (!newItem.MoveToContainer(player.inventory.containerMain))
                                        {
                                            //RemoveFromWorld(newItem);

                                            Interface.Oxide.LogWarning("no move to containerBelt with itemPos: " + itemPos + ", " + item.shortName + ", and we couldn't move it to containerMain at all!");
                                            RemoveFromWorld(newItem);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex2) { Interface.Oxide.LogError(ex2.ToString() + Environment.NewLine + "^infos[" + i + "]^"); }
                    }
                    //END ITEM MOVING PROCESS

                    var finalList = Pool.GetList<Item>();
                    try
                    {
                        player?.inventory?.AllItemsNoAlloc(ref finalList);

                        List<Item> removeOnDeathList = null;
                        if (RemoveItemsOnDeath)
                        {
                            if (!removeAfterDeath.TryGetValue(player.UserIDString, out removeOnDeathList)) removeOnDeathList = new List<Item>();
                            else removeOnDeathList.Clear();
                        }


                        if (finalList?.Count > 0 && DMMain?.DMowners != null)
                        {
                            for (int i = 0; i < finalList.Count; i++)
                            {
                                var item = finalList[i];
                                if (item != null)
                                {
                                    if (RemoveItemsOnDeath) removeOnDeathList.Add(item);

                                    DMMain.DMowners[item] = player.userID;
                                }
                            }
                        }

                        if (RemoveItemsOnDeath) removeAfterDeath[player.UserIDString] = removeOnDeathList;

                    }
                    finally { Pool.FreeList(ref finalList); }

                    return true;
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^Failed to complete CreateItems^" + Environment.NewLine + errSB.ToString().TrimEnd());
                    return false;
                }
            }

            public bool JoinDM(BasePlayer player, bool doSpawns = true)
            {
                try
                {
                    Interface.Oxide.LogWarning("JoinDM, game in progress: " + GameInProgress + ", round based: " + RoundBased + ", join mid round: " + JoinMidRound);
                    if (GameInProgress && RoundBased && !JoinMidRound)
                    {
                        Interface.Oxide.LogWarning("GameInProgress, RoundBased && !JoinMidRound for arena: " + ArenaName);
                        return false;
                    }
                    if (player == null || (player?.IsDead() ?? true) || !(player?.IsConnected ?? false))
                    {
                        Interface.Oxide.LogWarning("joindm called on bad player");
                        return false;
                    }
                    if (ActivePlayers.Contains(player))
                    {
                        Interface.Oxide.LogWarning("join dm called on already active player");
                        return false;
                    }
                    var canJoin = Interface.Oxide.CallHook("CanJoinDeathmatch", player, ArenaName);
                    if (canJoin != null)
                    {
                        var str = canJoin as string;
                        if (!string.IsNullOrEmpty(str)) player.ChatMessage(str);
                        return false;
                    }

                    if (DMMain?.AAESPBlocker != null && DMMain.AAESPBlocker.IsLoaded)
                    {
                        DMMain.AAESPBlocker.Call("RemoveESPPlayer", player);
                    }

                    if (player.currentTeam != 0)
                    {
                        oldTeam[player.UserIDString] = player.currentTeam;

                        var memberCount = RelationshipManager.ServerInstance?.FindTeam(player.currentTeam)?.members?.Count ?? 0;
                        if (memberCount > 1) ClearTeam(player); //leader isn't only member. if the leader is only member, we don't want to clear their team because we already prevent invites while in DM arena. pointless team destruction
                    }
                

                    if (player?.IsSpectating() ?? false) DMMain?.StopSpectatePlayer(player);
                    //redundant code?? refer to DoSpawns below instead
                    /*/
                    var rndSpawn = GetRandomSpawn(GetTeam(player));
                    if (rndSpawn == Vector3.zero && doSpawns)
                    {
                        Interface.Oxide.LogWarning("got bad spawn from getrandomspawn");
                        return false;
                    }/*/
                    DMMain?.SaveMetabolism(player);
                    var listItems = new List<Item>();
                    var listPos = new List<ItemPosition>();
                    var allItems = player?.inventory?.AllItems()?.ToList() ?? null;
                    if (allItems != null && allItems.Count > 0)
                    {
                        for (int i = 0; i < allItems.Count; i++)
                        {
                            var item = allItems[i];
                            if (item == null) continue;
                            var contName = (item?.parent == player.inventory.containerWear) ? "wear" : (item?.parent == player.inventory.containerBelt) ? "belt" : (item?.parent == player.inventory.containerMain) ? "main" : string.Empty;
                            //   Interface.Oxide.LogWarning("contName: " + contName + ", for: " + item.info.shortname);
                            var pos = item?.position ?? -1;
                            var newItemPos = new ItemPosition(item.info.itemid, contName, pos);
                            listPos.Add(newItemPos);
                            listItems.Add(item);
                            item.RemoveFromContainer();
                        }
                    }
                    PlayerItems[player.UserIDString] = listItems;
                    playerItemPositions[player.UserIDString] = listPos;
                    var didSpawns = false;
                    if (doSpawns)
                    {
                        didSpawns = DoSpawns(player, Teams ? SpawnPoint.SpawnType.Base : SpawnPoint.SpawnType.None);
                        if (!didSpawns)
                        {
                            Interface.Oxide.LogWarning("No do spawns: " + player.displayName + ", teams: " + Teams);
                            if (!LeaveDM(player)) Interface.Oxide.LogWarning("LeaveDM returned false after we failed to do spawns!!"); //Call LeaveDM() to ensure player cleanly leaves, considering we've already taken their items
                            return false;
                        }
                    }
                    Dictionary<string, int> capacity = new Dictionary<string, int>();
                    if (!preJoinCapacity.TryGetValue(player.UserIDString, out capacity)) capacity = new Dictionary<string, int>();
                    capacity["main"] = player?.inventory?.containerMain?.capacity ?? 24;
                    capacity["belt"] = player?.inventory?.containerBelt?.capacity ?? 6;
                    capacity["wear"] = player?.inventory?.containerWear?.capacity ?? 6;
                    preJoinCapacity[player.UserIDString] = capacity;
                    if (!CreateItems(player))
                    {
                        Interface.Oxide.LogWarning("Failed to create items in arena: " + ArenaName + ", for: " + player.displayName);
                        if (!LeaveDM(player)) Interface.Oxide.LogWarning("LeaveDM returned false after we failed to create items!!"); //Call LeaveDM() to ensure player cleanly leaves, considering we've already taken their items
                        return false;
                    }

                    ActivePlayers.Add(player);

                    if (RoundBased && ActivePlayers.Count >= PlayersForStart && !GameInProgress && !StartingGame)
                    {
                        StartGame(NewGameTime);
                        Interface.Oxide.LogWarning("Called start game with NewGameTime: " + NewGameTime);
                    }

                    if (ForceDay && envSync != null) DMMain?.SendNetworkUpdate(envSync, BasePlayer.NetworkQueue.Update, player);

                    if (Teams && GameInProgress) EnsurePlayerTeams();

                    Interface.Oxide.CallHook("OnJoinDeathmatchArena", player, this);
                    Interface.Oxide.CallHook("OnJoinDeathmatch", player, ArenaName);

                    return true;
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^Failed to complete JoinDM^");
                    return false;
                }
            }

            /// <summary>
            /// Ensures the player cleanly leaves the Arena and returns all their original items
            /// </summary>
            /// <param name="player"></param>
            /// <returns></returns>
            public bool LeaveDM(BasePlayer player)
            {
                try
                {
                    if (player == null) return false;
                    if (DMMain?.AAESPBlocker != null && DMMain.AAESPBlocker.IsLoaded)
                    {
                        DMMain?.AAESPBlocker?.Call("AddEspPlayer", player);
                    }

                    if (ActivePlayers.Contains(player)) ActivePlayers.Remove(player);
                    if (Teams) SetTeam(player, Team.None);
                    player.EnsureDismounted();
                    if (player?.IsSpectating() ?? false) DMMain?.StopSpectatePlayer(player);

                    var allItems = player?.inventory?.AllItems()?.ToList() ?? null;
                    if (allItems != null && allItems.Count > 0) for (int i = 0; i < allItems.Count; i++) RemoveFromWorld(allItems[i]);

                    var lastPosStr = string.Empty;
                    var lastPos = Vector3.zero;
                    if (DMMain.dmData.lastPlayerPositions.TryGetValue(player.UserIDString, out lastPosStr)) lastPos = GetVector3FromString(lastPosStr);
                    if (lastPos == Vector3.zero)
                    {
                        player.Respawn();
                        Interface.Oxide.LogWarning("Got vec 3 zero last pos, respawning player!");
                    }
                    else DMMain.TeleportPlayer(player, lastPos, repeatTeleports: 5);

                    DMMain.LoadOldMetabolism(player);


                    player.SendConsoleCommand("gametip.hidegametip"); //remove annoying tip


                    DMMain.killsWithoutDying.Remove(player.userID); //reset killstreak

                    if (Teams) ClearTeam(player);

                    ulong team;
                    if (oldTeam.TryGetValue(player.UserIDString, out team))
                    {
                        if (player.currentTeam != team) ClearTeam(player);

                        var findTeam = RelationshipManager.ServerInstance?.FindTeam(team) ?? null;
                        if (findTeam != null)
                        {
                            if (!findTeam.members.Contains(player.userID)) findTeam.AddPlayer(player);
                            else Interface.Oxide.LogWarning("findTeam with oldTeam already contained member: " + player?.displayName + " (" + player?.UserIDString + "), team: " + team);
                        }
                        else Interface.Oxide.LogWarning(player?.displayName + " (" + player?.UserIDString + ") had oldTeam of: " + team + ", but we couldn't find the team instance!");
                    }

                   
                    var listItems = new List<Item>();
                    var listPos = new List<ItemPosition>();

                    if (PlayerItems.TryGetValue(player.UserIDString, out listItems)) listItems = listItems?.ToList() ?? null;
                    else Interface.Oxide.LogWarning("Couldn't get PlayerItems on LeaveDM for: " + player?.displayName);

                    if (!playerItemPositions.TryGetValue(player.UserIDString, out listPos)) Interface.Oxide.LogWarning("No out listPos");
                    Dictionary<string, int> capacity;
                    if (preJoinCapacity.TryGetValue(player.UserIDString, out capacity))
                    {
                        int count;
                        if (capacity.TryGetValue("main", out count)) player.inventory.containerMain.capacity = count;
                        if (capacity.TryGetValue("belt", out count)) player.inventory.containerBelt.capacity = count;
                        if (capacity.TryGetValue("wear", out count)) player.inventory.containerWear.capacity = count;
                    }
                    else
                    {
                        Interface.Oxide.LogWarning("didn't get prejoin capacity: " + player.displayName);
                        if (player.inventory.containerMain.capacity > 24) player.inventory.containerMain.capacity = 24;
                        if (player.inventory.containerWear.capacity > 6) player.inventory.containerWear.capacity = 7;
                        if (player.inventory.containerBelt.capacity > 6) player.inventory.containerBelt.capacity = 6;
                    }

                    // listItems = listItems?.ToList() ?? null;
                    if (listItems.Count > 0)
                    {
                        for (int i = 0; i < listItems.Count; i++)
                        {
                            var item = listItems[i];
                            if (item == null) continue;
                            var findPos = listPos?.Where(p => (p?.itemID ?? 0) == item.info.itemid)?.FirstOrDefault() ?? null;
                            if (findPos == null) Interface.Oxide.LogWarning("FIND POS IS NULL!!!!: " + item.uid);
                            if (findPos != null)
                            {
                                var cont = DMMain.GetContainerFromName(player, findPos.containerName);
                                if (!item.MoveToContainer(cont, findPos.position) && !item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt) && !item.MoveToContainer(player.inventory.containerWear))
                                {
                                    Interface.Oxide.LogWarning("Couldn't move an item back into a player's inventory on LeaveDM for player: " + player?.displayName);
                                    RemoveFromWorld(item);
                                }
                            }
                            else if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt) && !item.MoveToContainer(player.inventory.containerWear))
                            {
                                Interface.Oxide.LogWarning("Couldn't move an item back into a player's inventory on LeaveDM for player (2): " + player?.displayName);
                                RemoveFromWorld(item);
                            }
                        }
                    }



                    StopTimeGUI(player);
                    StopAliveGUI(player);
                    StopKillsGUI(player);
                    if (RoundBased && !Fortnite && (ActivePlayers == null || ActivePlayers.Count < PlayersForStart)) EndGame();

                    if (ForceDay && (player?.IsConnected ?? false))
                    {
                        player.Invoke(() =>
                        {
                            if (player == null || player.IsDestroyed || player.gameObject == null || !player.IsConnected) return;

                            DMMain?.SendReplicatedVarsToConnection(player.Connection, "weather.");

                            if (envSync != null) DMMain?.SendNetworkUpdate(envSync, BasePlayer.NetworkQueue.Update, player);
                        }, 1.25f);
                   
                    }

                    player.Invoke(() =>
                    {
                        if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead()) return;
                        StopHostile(player);
                    }, 0.75f);

                    Interface.Oxide.CallHook("OnLeaveDeathmatch", player, ArenaName);
                    return true;
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^Failed to complete LeaveDM^");
                    return false;
                }
            }

            public bool JoinQueue(BasePlayer player, bool useWaitingArea = true, bool doJoinChecks = true)
            {
                if (player == null || player.IsDead()) return false;
                Timer endTimer = null;
                if (queueTimer.TryGetValue(player.UserIDString, out endTimer))
                {
                    endTimer.Destroy();
                    endTimer = null;
                }
                if (useWaitingArea)
                {
                    if (WaitingArea == Vector3.zero) Interface.Oxide.LogWarning("useWaitingArea but WaitingArea is vector3 zero!!");
                    else
                    {
                        var oldPos = player?.transform?.position ?? Vector3.zero;
                        DMMain.dmData.lastPlayerPositions[player.UserIDString] = GetVectorString(oldPos);
                        DMMain?.TeleportPlayer(player, WaitingArea);
                    }
                }
                endTimer = DMMain.timer.Every(1f, () =>
                {
                    if (GameInProgress) return;
                    if (doJoinChecks)
                    {
                        var reason = string.Empty;
                        var canJoin = DMMain?.CanJoinDM(player, this, out reason) ?? false;
                        if (!canJoin)
                        {
                            if (!string.IsNullOrEmpty(reason)) DMMain?.SendReply(player, reason);
                            else DMMain?.SendReply(player, "Unable to join Deathmatch at this time.");
                            return;
                        }
                    }

                    if (!JoinDM(player, true)) Interface.Oxide.LogWarning("Got all clear for joining DM on queue, but failed to join (returned false)!");
                    endTimer.Destroy();
                    endTimer = null;
                });
                queueTimer[player.UserIDString] = endTimer;
                return true;
            }

            public bool LeaveQueue(BasePlayer player)
            {
                if (player == null || player.IsDead()) return false;
                Timer qTimer;
                if (queueTimer.TryGetValue(player.UserIDString, out qTimer))
                {
                    qTimer.Destroy();
                    queueTimer.Remove(player.UserIDString);
                }
                return true;
            }

            public bool InQueue(BasePlayer player)
            {
                Timer qTimer;
                return (player == null) ? false : queueTimer.TryGetValue(player.UserIDString, out qTimer);
            }

            [JsonIgnore]
            public List<Vector3> SpawnList
            {
                get
                {
                    var newList = new List<Vector3>();
                    if ((spawnPoints?.Count ?? 0) > 0) for (int i = 0; i < spawnPoints.Count; i++) newList.Add(spawnPoints[i].Point);
                    return newList;
                }
            }

            [JsonIgnore]
            public List<Vector3> AlliesSpawns
            {
                get
                {
                    var newList = new List<Vector3>();
                    if ((teamSpawnPoints?.Count ?? 0) > 0)
                    {
                        var alliedSpawns = teamSpawnPoints?.Where(p => p.Key == Team.Allies)?.FirstOrDefault().Value ?? null;
                        if (alliedSpawns == null || alliedSpawns.Count < 1) return newList;
                        for (int i = 0; i < alliedSpawns.Count; i++) newList.Add(alliedSpawns[i].Point);
                    }
                    return newList;
                }
            }

            [JsonIgnore]
            public List<Vector3> AxisSpawns
            {
                get
                {
                    var newList = new List<Vector3>();
                    if ((teamSpawnPoints?.Count ?? 0) > 0)
                    {
                        var axisSpawns = teamSpawnPoints?.Where(p => p.Key == Team.Axis)?.FirstOrDefault().Value ?? null;
                        if (axisSpawns == null || axisSpawns.Count < 1) return newList;
                        for (int i = 0; i < axisSpawns.Count; i++) newList.Add(axisSpawns[i].Point);
                    }
                    return newList;
                }
            }

            [JsonIgnore]
            private RelationshipManager.PlayerTeam _alliesTeam = null;

            [JsonIgnore]
            public RelationshipManager.PlayerTeam AlliesTeam
            {
                get
                {
                    if (_alliesTeam == null)
                    {
                        _alliesTeam = RelationshipManager.ServerInstance.CreateTeam();
                        _alliesTeam.SetTeamLeader(0);
                    }
                    return _alliesTeam;
                }
            }

            [JsonIgnore]
            private RelationshipManager.PlayerTeam _axisTeam = null;

            [JsonIgnore]
            public RelationshipManager.PlayerTeam AxisTeam
            {
                get
                {
                    if (_axisTeam == null)
                    {
                        _axisTeam = RelationshipManager.ServerInstance.CreateTeam();
                        _axisTeam.SetTeamLeader(0);
                    }
                    return _axisTeam;
                }
            }

            private void ForceAddMemberToTeam(RelationshipManager.PlayerTeam team, ulong userId)
            {
                if (team == null) throw new ArgumentNullException(nameof(team));
                if (team.members.Contains(userId))
                {
                    Interface.Oxide.LogWarning("team.members contained userId already on ForceAddMemberToTeam - not continuing");
                    return;
                }
                var ply = BasePlayer.FindByID(userId) ?? BasePlayer.FindSleeping(userId);
                if (ply != null)
                {
                    ClearTeam(ply);
                    ply.currentTeam = team.teamID;
                }
                team.members.Add(userId);
                RelationshipManager.ServerInstance.playerToTeam.Add(userId, team);
                team.MarkDirty();
                if (ply != null && ply.IsConnected) ply.SendNetworkUpdate();
            }

            private void DisbandTeam(RelationshipManager.PlayerTeam team)
            {
                if (team == null) throw new ArgumentNullException(nameof(team));
                var members = new List<ulong>(team.members);
                for (int i = 0; i < members.Count; i++)
                {
                    var userId = members[i];
                    var ply = BasePlayer.FindByID(userId) ?? BasePlayer.FindSleeping(userId);
                    if (ply != null) ClearTeam(ply);
                    else
                    {
                        team.RemovePlayer(userId);
                    }
                }
                team.Disband();
            }

            public void CleanupTeams()
            {
                if (_alliesTeam != null)
                {
                    DisbandTeam(_alliesTeam);
                    _alliesTeam = null;
                }
                if (_axisTeam != null)
                {
                    DisbandTeam(_axisTeam);
                    _axisTeam = null;
                }
            }

            public void EnsurePlayerTeams()
            {
                if (!Teams)
                {
                    Interface.Oxide.LogWarning("EnsurePlayerTeams called on non-teams gamemode");
                    return;
                }

                var skipAxis = GetTeamCount(Team.Axis) < 2;
                var skipAllies = GetTeamCount(Team.Allies) < 2;
                if (skipAxis && skipAllies) return;
                foreach (var p in ActivePlayers)
                {
                    if (p == null || p.IsDestroyed || p.gameObject == null)
                    {
                        Interface.Oxide.LogError("Very bad player object detected in ActivePlayers - skipping");
                        continue;
                    }
                    var pTeam = GetTeam(p);
                    if (pTeam == Team.None)
                    {
                        Interface.Oxide.LogWarning("Player has Team.None in team based mode, player: " + p);
                        continue;
                    }
                    var useTeam = pTeam == Team.Allies ? AlliesTeam : AxisTeam;
                    if (!useTeam.members.Contains(p.userID))
                    {
                        ClearTeam(p);
                        ForceAddMemberToTeam(useTeam, p.userID);
                    }
                }
            }

            public List<SpawnPoint> GetSpawnPoints(Team team = Team.None)
            {
                if (team == Team.None) return spawnPoints;
                var points = new List<SpawnPoint>();
                if (teamSpawnPoints.TryGetValue(team, out points)) return points;
                return null;
            }

            public string GetSpawnName(Vector3 spawn)
            {
                var spawnName = spawnPoints?.Where(p => p.Point == spawn)?.FirstOrDefault()?.Name ?? string.Empty;
                if (string.IsNullOrEmpty(spawnName))
                {
                    foreach (var kvp in teamSpawnPoints)
                    {
                        if (kvp.Value == null || kvp.Value.Count < 1) continue;
                        for (int i = 0; i < kvp.Value.Count; i++)
                        {
                            var entry = kvp.Value[i];
                            if (entry.Point == spawn) return entry.Name;
                        }
                    }
                }
                return spawnName;
            }

            public Vector3 FindSpawnByName(string name)
            {
                if (string.IsNullOrEmpty(name)) return Vector3.zero;
                var findVec = spawnPoints?.Where(p => p.Name == name)?.FirstOrDefault()?.Point ?? Vector3.zero;
                if (findVec != Vector3.zero) return findVec;
                return teamSpawnPoints.Values.Where(p => p.Any(x => x.Name == name))?.FirstOrDefault()?.FirstOrDefault()?.Point ?? Vector3.zero;
            }

            public ItemInfo GetItemInfo(Item item)
            {
                if (item == null) return null;
                if (DMItems != null && DMItems.Count > 0)
                {
                    for (int i = 0; i < DMItems.Count; i++)
                    {
                        var itemInf = DMItems[i];
                        if (itemInf.shortName.Equals(item.info.shortname, StringComparison.OrdinalIgnoreCase))
                        {
                            return itemInf;
                        }
                    }
                }


                foreach (var kvp in TeamItems)
                {
                    if (kvp.Value == null || kvp.Value.Count < 1) continue;
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        var info = kvp.Value[i];
                        if (info != null && info.shortName.Equals(item.info.shortname, StringComparison.OrdinalIgnoreCase)) return info;
                    }
                }
                return null;
            }

            //    public ItemInfo GetItemInfo(Item item) { return (item == null) ? null : DMItems?.Where(p => p.shortName.Equals(item.info.shortname))?.FirstOrDefault() ?? null; }

            public List<Item> GetTeamItemList(Team team)
            {
                if (team == Team.None) return DMItemList;
                var newList = new List<Item>();
                var infos = new List<ItemInfo>();
                if (!TeamItems.TryGetValue(team, out infos) || infos.Count < 1) return newList;
                for (int i = 0; i < infos.Count; i++)
                {
                    var item = infos[i];
                    if (item == null || string.IsNullOrEmpty(item?.shortName ?? string.Empty)) continue;
                    var newItem = ItemManager.CreateByName(item.shortName, item.Amount, item.skinID);
                    if (newItem == null) continue;
                    var heldProj = newItem?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                    if (heldProj != null)
                    {
                        if (!string.IsNullOrEmpty(item?.ammoName ?? string.Empty)) heldProj.primaryMagazine.ammoType = ItemManager.FindItemDefinition(item.ammoName) ?? null;
                        heldProj.primaryMagazine.contents = item?.ammoAmount ?? 0;
                    }
                    if (item.mods != null && item.mods.Count > 0 && newItem?.contents != null)
                    {
                        var modItems = item?.GetModItems() ?? null;
                        if (modItems != null && modItems.Count > 0)
                        {
                            for (int j = 0; j < modItems.Count; j++)
                            {
                                var modItem = modItems[j];
                                if (!modItem.MoveToContainer(newItem.contents)) RemoveFromWorld(modItem);
                            }
                        }
                    }
                    newList.Add(newItem);
                }
                return newList;
            }

            public List<ItemInfo> GetTeamItemInfos(Team team)
            {
                if (team == Team.None) return DMItems;
                List<ItemInfo> infos;
                if (!TeamItems.TryGetValue(team, out infos)) infos = new List<ItemInfo>();
                return infos;
            }

            [JsonIgnore]
            public List<Item> DMItemList
            {
                get
                {
                    try
                    {
                        var newList = new List<Item>();
                        if ((DMItems?.Count ?? 0) > 0)
                        {
                            for (int i = 0; i < DMItems.Count; i++)
                            {
                                var item = DMItems[i];
                                if (item == null || string.IsNullOrEmpty(item?.shortName ?? string.Empty)) continue;
                                var newItem = ItemManager.CreateByName(item.shortName, item.Amount, item.skinID);
                                if (newItem == null) continue;
                                var heldProj = newItem?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                                if (heldProj != null)
                                {
                                    if (!string.IsNullOrEmpty(item?.ammoName ?? string.Empty)) heldProj.primaryMagazine.ammoType = ItemManager.FindItemDefinition(item.ammoName) ?? null;
                                    heldProj.primaryMagazine.contents = item?.ammoAmount ?? 0;
                                }
                                if (item.mods != null && item.mods.Count > 0 && newItem?.contents != null)
                                {
                                    var modItems = item?.GetModItems() ?? null;
                                    if (modItems != null && modItems.Count > 0)
                                    {
                                        for (int j = 0; j < modItems.Count; j++)
                                        {
                                            var modItem = modItems[j];
                                            if (!modItem.MoveToContainer(newItem.contents)) RemoveFromWorld(modItem);
                                        }
                                    }
                                }
                                newList.Add(newItem);
                            }
                        }
                        return newList;
                    }
                    catch (Exception ex)
                    {
                        Interface.Oxide.LogError(ex.ToString() + Environment.NewLine + "^Failed to complete DMItemList^");
                        return null;
                    }
                }
            }

            public void AddSpawn(string spawnName, Vector3 vector, SpawnPoint.SpawnType type = SpawnPoint.SpawnType.Default, Team team = Team.None)
            {
                var vecStr = vector.ToString().Replace("(", "").Replace(")", "");
                if (team == Team.None)
                {
                    var findSpawn = spawnPoints?.Where(p => p.Name == spawnName)?.FirstOrDefault() ?? null;
                    if (findSpawn == null)
                    {
                        findSpawn = new SpawnPoint();
                        spawnPoints.Add(findSpawn);
                    }
                    findSpawn.Name = spawnName;
                    findSpawn.Point = vector;
                    findSpawn.Type = type;
                }
                else
                {
                    List<SpawnPoint> teamSpawns = new List<SpawnPoint>();
                    if (!teamSpawnPoints.TryGetValue(team, out teamSpawns)) teamSpawns = new List<SpawnPoint>();
                    var findSpawn = teamSpawns?.Where(p => p.Name == spawnName)?.FirstOrDefault() ?? null;
                    if (findSpawn == null)
                    {
                        findSpawn = new SpawnPoint();
                        teamSpawns.Add(findSpawn);
                    }
                    findSpawn.Name = spawnName;
                    findSpawn.Point = vector;
                    findSpawn.Type = type;
                    teamSpawnPoints[team] = teamSpawns;
                }

            }


            public void RemoveSpawn(string spawnName)
            {
                if (string.IsNullOrEmpty(spawnName)) throw new ArgumentNullException(nameof(spawnName));
                SpawnPoint findSpawn = null;
                if (spawnPoints?.Count > 0)
                {
                    for (int i = 0; i < spawnPoints.Count; i++)
                    {
                        var point = spawnPoints[i];
                        if (point?.Name == spawnName)
                        {
                            findSpawn = point;
                            break;
                        }
                    }
                }
                if (findSpawn != null) spawnPoints.Remove(findSpawn);
                else
                {
                    var teamSpawns = teamSpawnPoints?.ToDictionary(p => p.Key, p => p.Value) ?? null;
                    if (teamSpawns == null || teamSpawns.Count < 1) return;
                    foreach (var kvp in teamSpawns) foreach (var val in kvp.Value) if (val.Name == spawnName) teamSpawnPoints[kvp.Key].Remove(val);
                }
            }

            public Vector3 GetRandomSpawn(Team team = Team.None)
            {
                var newList = (team == Team.None) ? SpawnList?.ToList() ?? null : (team == Team.Allies) ? AlliesSpawns?.ToList() ?? null : AxisSpawns?.ToList() ?? null;
                if (team != Team.None && (newList == null || newList.Count < 1)) newList = SpawnList?.ToList() ?? null;
                Interface.Oxide.LogWarning("get random spawn, team: " + team + ", count: " + (newList?.Count ?? 0));
                if (newList != null && newList.Count > 0) newList.Shuffle((uint)UnityEngine.Random.Range(0, int.MaxValue));
                else return Vector3.zero;
                return newList?.GetRandom() ?? Vector3.zero;
            }

            public Vector3 GetSmartSpawn(BasePlayer player = null, SpawnPoint.SpawnType type = SpawnPoint.SpawnType.None)
            {
                var team = (player != null) ? GetTeam(player) : Team.None;
                var points = GetSpawnPoints((type == SpawnPoint.SpawnType.None) ? Team.None : team);
                if (points == null)
                {
                    Interface.Oxide.LogWarning("Was told to get spawns for team: " + team + ", but that team has no spawns. Defaulting to Team.None");
                    points = GetSpawnPoints(Team.None);
                }
                if (points == null || points.Count < 1)
                {
                    Interface.Oxide.LogWarning("GetSmartSpawn called with no spawn points!");
                    return Vector3.zero;
                }
                var randomSpawns = new List<Vector3>();
                for (int i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    if (point == null) continue;
                    if (type == SpawnPoint.SpawnType.None || point.Type == type) randomSpawns.Add(point.Point);
                }
                //var randomSpawns = (type != SpawnPoint.SpawnType.None) ? points?.Where(p => p.Type == type)?.Select(p => p.Point)?.ToList() ?? null : points?.Select(p => p.Point)?.ToList() ?? null;
                if ((randomSpawns == null || randomSpawns.Count < 1) && type != SpawnPoint.SpawnType.None)
                {
                    Interface.Oxide.LogWarning("bad spawns by type! getting all spawns");
                    randomSpawns = points?.Select(p => p.Point)?.ToList() ?? null; //no spawns of that SpawnType, so use any
                }
                //   if (team != Team.None) Interface.Oxide.LogWarning("do spawns for team: " + team + ", " + player.displayName + ", rd spawns count: " + (randomSpawns?.Count ?? 0));
                if (randomSpawns != null && randomSpawns.Count > 0) randomSpawns.Shuffle((uint)UnityEngine.Random.Range(0, int.MaxValue));
                else return Vector3.zero;
                if (Teams && team != Team.None)
                {
                    var teamPlayers = GetTeamPlayers(team);
                    if (teamPlayers.Count > 1)
                    {
                        var newList = new List<Vector3>();

                        for (int i = 0; i < randomSpawns.Count; i++)
                        {
                            var spawn = randomSpawns[i];
                            for (int j = 0; j < teamPlayers.Count; j++)
                            {
                                var p = teamPlayers[j];
                                if (Vector3.Distance(p?.transform?.position ?? Vector3.zero, spawn) <= 50)
                                {
                                    newList.Add(spawn);
                                }
                            }
                        }
                        if (newList.Count > 0) randomSpawns = newList;
                    }
                }
                try
                {
                    for (int i = 0; i < randomSpawns.Count; i++)
                    {
                        var spawn = randomSpawns[i];
                        if (spawn == Vector3.zero) return Vector3.zero;
                        if (player != null)
                        {
                            var lastDeath = DMMain?.GetLastDeath(player.UserIDString) ?? Vector3.zero;
                            if (Vector3.Distance(lastDeath, spawn) < 25) continue;
                        }

                        var visPlayersList = new HashSet<BasePlayer>();
                        var pid = player?.UserIDString ?? string.Empty;
                        foreach (var p in ActivePlayers)
                        {
                            if (p == null || p.IsDestroyed || p.gameObject == null || p.IsDead() || p.transform == null || p.UserIDString == pid) continue;
                            if ((!Teams || GetTeam(p) != team) && Vector3.Distance(p.transform.position, spawn) <= 36) visPlayersList.Add(p);
                        }
                        //var visPlayersList = (!Teams) ? ActivePlayers?.Where(p => (p?.IsAlive() ?? false) && (Vector3.Distance((p?.transform?.position ?? Vector3.zero), spawn)) < 36)?.ToList() ?? null : ActivePlayers?.Where(p => (p?.IsAlive() ?? false) && (Vector3.Distance((p?.transform?.position ?? Vector3.zero), spawn)) < 36 && GetTeam(p) != team)?.ToList() ?? null;

                        if (player != null)
                        {
                            var foundLook = false;
                            foreach (var ap in ActivePlayers)
                            {
                                if (ap == null || (ap?.IsDead() ?? true) || !(ap?.IsConnected ?? false) || (ap?.UserIDString ?? string.Empty) == (player?.UserIDString ?? string.Empty)) continue;
                                if (team != Team.None && GetTeam(ap) == team) continue;
                                var apRot = Quaternion.Euler(ap?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
                                var sphereHits = Physics.SphereCastAll(new Ray(ap.eyes.position, apRot), 8, 90, PLAYER_LAYER);
                                if ((sphereHits?.Length ?? 0) < 1) continue;
                                else
                                {
                                    for (int k = 0; k < sphereHits.Length; k++)
                                    {
                                        var hit = sphereHits[k];
                                        var hitEnt = hit.GetEntity() ?? null;
                                        var hitPos = hit.point != Vector3.zero ? hit.point : hit.GetTransform()?.position ?? hit.GetEntity()?.transform?.position ?? Vector3.zero;
                                        if (Vector3.Distance(spawn, hitPos) <= 8)
                                        {
                                            foundLook = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (foundLook) continue;
                        }
                        if (visPlayersList.Count > 0)
                        {/*/
                            var isSelf = visPlayersList?.Any(p => (p?.GetComponent<BasePlayer>()?.UserIDString ?? string.Empty) == (player?.UserIDString ?? string.Empty)) ?? false;
                            if (player != null && isSelf && visPlayersList.Count < 2)
                            {
                                Interface.Oxide.LogWarning("Got spawn with nearby player as self? -- using this spawn");
                                return spawn;
                            }/*/
                            continue;
                        }
                        else return spawn;
                    }
                }
                catch (Exception ex) { Interface.Oxide.LogError(ex.ToString()); }
                return randomSpawns?.GetRandom((uint)UnityEngine.Random.Range(0, uint.MaxValue)) ?? Vector3.zero;
            }

            public ArenaInfo(string arenaName, string zoneID)
            {
                if (string.IsNullOrEmpty(arenaName) || string.IsNullOrEmpty(zoneID)) return;
                ZoneID = zoneID;
                ArenaName = arenaName;
            }

            public ArenaInfo() { }
        }

        private bool InAnyDM(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (Arenas != null && Arenas.Count > 0)
            {
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena?.ActivePlayers != null && arena.ActivePlayers.Count > 0)
                    {
                        foreach (var p in arena.ActivePlayers) { if (p?.UserIDString == userId) return true; }
                    }
                }
            }
            return false;
            //return arenas?.Any(p => (p?.ActivePlayers?.Any(x => x?.UserIDString == userId) ?? false)) ?? false; 
        }

        private bool InAnyDMZone(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (Arenas != null && Arenas.Count > 0)
            {
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var zp = Arenas[i]?.ZonePlayers;
                    if (zp == null || zp.Count < 1) continue;
                    for (int j = 0; j < zp.Count; j++)
                    {
                        if (zp[j]?.UserIDString == userId) return true;
                    }
                }
            }
            return false;
        }

        //bool InAnyDMZone(string userId) { return arenas?.Any(p => (p?.ZonePlayers?.Any(x => x?.UserIDString == userId) ?? false)) ?? false; }

        private bool EntityInAnyDM(BaseEntity entity)
        {
            if (entity == null || (entity?.IsDestroyed ?? true)) return false;
            return Arenas?.Any(p => isEntityInZone(entity, p?.ZoneID ?? string.Empty)) ?? false;
        }

        private ArenaInfo GetEntityArena(BaseEntity entity)
        {
            if (entity == null || entity?.transform == null) return null;
            ArenaInfo entArena = null;
            if (Arenas != null && Arenas.Count > 0)
            {
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena == null || string.IsNullOrEmpty(arena.ZoneID) || arena.ZoneID == "0") continue;
                    if (isEntityInZone(entity, arena.ZoneID))
                    {
                        entArena = arena;
                        break;
                    }
                }
            }
            return entArena;
        }

        private readonly Dictionary<ulong, ArenaInfo> editArena = new Dictionary<ulong, ArenaInfo>();

        private ArenaInfo FindArena(string arenaName)
        {
            if (string.IsNullOrEmpty(arenaName)) throw new ArgumentNullException(nameof(arenaName));
            if (Arenas != null && Arenas.Count > 0)
            {
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena?.ArenaName?.Equals(arenaName, StringComparison.OrdinalIgnoreCase) ?? false) return arena;
                }
            }
            return null;
            //return arenas?.Where(p => string.Equals(p?.ArenaName, arenaName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault() ?? null; 
        }

        private ArenaInfo FindArenaByPartialName(string partialName, bool checkLocal = false)
        {
            if (string.IsNullOrEmpty(partialName)) return null;
            var findExact = FindArena(partialName);
            if (findExact != null) return findExact;

            var foundArenas = Pool.GetList<ArenaInfo>();
            try
            {
                if (Arenas != null && Arenas.Count > 0)
                {
                    for (int i = 0; i < Arenas.Count; i++)
                    {
                        var arena = Arenas[i];
                        if (arena == null || (arena.LocalArena && !checkLocal)) continue;
                        var arenaName = arena?.ArenaName ?? string.Empty;
                        if (arenaName.IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0) foundArenas.Add(arena);
                    }
                }

                return foundArenas.Count == 1 ? foundArenas[0] : null;
            }
            finally { Pool.FreeList(ref foundArenas); }


        }

        [ChatCommand("dm_add")]
        private void cmdDMAdd(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "Invalid syntax! Try: /dm_add ArenaName");
                return;
            }
            var arena = FindArena(args[0]);
            if (arena != null) SendReply(player, "An arena with this name already exists!");
            else
            {
                var newArena = new ArenaInfo
                {
                    ArenaName = args[0]
                };
                Arenas.Add(newArena);
                SendReply(player, "Created new arena with name: " + args[0] + ", to edit type: /dm_edit \"" + args[0] + "\"");
            }
        }

        [ChatCommand("dm_test")]
        private void cmdDMTest(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var arena = FindArena("1v1template");
            if (arena == null)
            {
                SendReply(player, "No 1v1template arena");
                return;
            }
            var join = args.Length > 0 && args[0].ToLower() == "j";
            ArenaInfo newArena = null;
            if (join)
            {
                newArena = Arenas?.Where(p => p != null && p.LocalArena && p.ActivePlayers.Count == 1)?.FirstOrDefault() ?? null;
                SendReply(player, "Found existing arena: " + arena.ArenaName + ", with: " + (newArena?.ActivePlayers?.Where(p => p != null && p != player)?.FirstOrDefault()?.displayName ?? "Unknown"));
                PrintWarning("Existing arena");
            }
            else
            {
                newArena = new ArenaInfo("1v1" + UnityEngine.Random.Range(0, 1000), arena.ZoneID)
                {
                    DMItems = arena.DMItems.ToList(),
                    spawnPoints = arena.spawnPoints.ToList(),
                    SpawnWithHealth = arena.SpawnWithHealth,
                    SpawnProtectionTime = arena.SpawnProtectionTime,
                    LocalArena = true,
                    dmItemPositions = arena.dmItemPositions,
                    playerItemPositions = arena.playerItemPositions,
                    playerItemSkins = arena.playerItemSkins
                };
                var oldPos = player?.transform?.position ?? Vector3.zero;
                dmData.lastPlayerPositions[player.UserIDString] = GetVectorString(oldPos);
                Arenas.Add(newArena);
            }
            if (newArena == null)
            {
                SendReply(player, "somehow no arena");
                PrintWarning("newArena == null!!");
                return;
            }
            NextTick(() =>
            {
                if (!newArena.JoinDM(player))
                {
                    newArena = null;
                    SendReply(player, "No join!!");
                }
                else SendReply(player, "Joined!!");
            });

        }

        [ChatCommand("dm_remove")]
        private void cmdDMRemove(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "Invalid syntax! Try: /dm_remove ArenaName");
                return;
            }
            var arena = FindArena(args[0]);
            if (arena != null)
            {
                SendReply(player, "Deleted: " + arena.ArenaName);
                Arenas.Remove(arena);
            }
            else SendReply(player, "No arena found with that name!");
        }

        private readonly string ammoBoxPrefab = "assets/bundled/prefabs/radtown/dmloot/dm ammo.prefab";
        private readonly string c4BoxPrefab = "assets/bundled/prefabs/radtown/dmloot/dm c4.prefab";
        private readonly string medicalBoxPrefab = "assets/bundled/prefabs/radtown/dmloot/dm medical.prefab";
        private readonly string tier3BoxPrefab = "assets/bundled/prefabs/radtown/dmloot/dm tier3 lootbox.prefab";

        private string GetRandomBoxPrefab(List<string> blackList = null)
        {
            string[] prefabs = { "assets/bundled/prefabs/radtown/dmloot/dm ammo.prefab", "assets/bundled/prefabs/radtown/dmloot/dm c4.prefab", "assets/bundled/prefabs/radtown/dmloot/dm medical.prefab", "assets/bundled/prefabs/radtown/dmloot/dm tier3 lootbox.prefab" };
            if (blackList != null && blackList.Count > 0) return prefabs?.Where(p => !blackList.Contains(p))?.ToList()?.GetRandom() ?? string.Empty;
            else return prefabs?.ToList()?.GetRandom() ?? string.Empty;
        }

        private readonly List<string> autoSpawns = new List<string>();
        private readonly Dictionary<string, ArenaInfo.Team> selectedEditTeam = new Dictionary<string, ArenaInfo.Team>();
        private readonly Dictionary<string, SpawnPoint.SpawnType> selectedSpawnType = new Dictionary<string, SpawnPoint.SpawnType>();
        [ChatCommand("dm_edit")]
        private void cmdDMEdit(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            var arena = GetEditArena(player.userID);
            if (args.Length > 0)
            {
                var findArena = FindArena(args[0]);
                if (findArena != null && findArena != arena)
                {
                    SetEditArena(player.userID, arena = findArena);
                    SendReply(player, "Now editing arena: " + arena?.ArenaName);
                    return;
                }
            }

            if (arena == null)
            {
                SendReply(player, "You are not editing an arena or specified an invalid arena, try /dm_edit ArenaName");
                return;
            }

            ArenaInfo.Team editTeam;
            if (!selectedEditTeam.TryGetValue(player.UserIDString, out editTeam)) editTeam = ArenaInfo.Team.None;
            SpawnPoint.SpawnType editType;
            if (!selectedSpawnType.TryGetValue(player.UserIDString, out editType)) editType = SpawnPoint.SpawnType.Default;

            var heldItem = player?.GetActiveItem() ?? null;

            if (args.Length < 1)
            {
                var editSB = new StringBuilder();
                var editSB2 = new StringBuilder();
                editSB.AppendLine("Spawn Protection Time (spt) => " + (arena?.SpawnProtectionTime ?? -1f));
                editSB.AppendLine("Players start health (health) => " + (arena?.SpawnWithHealth ?? 100f));
                editSB.AppendLine("Bleeding Scalar (bleed) => " + (arena?.BleedingScalar ?? 1f));
                editSB.AppendLine("Healing Scalar (heal) => " + (arena?.HealingScalar ?? 1f));
                editSB.AppendLine("Fireball time (firetime) => " + (arena?.FireballTime ?? -1f));
                editSB.AppendLine("Description (desc) => " + (arena?.Description ?? string.Empty));
                editSB.AppendLine("Disabled (disabled) => " + (arena?.Disabled ?? false));
                editSB.AppendLine("Disabled Text (dtext) => " + (arena?.DisabledText ?? string.Empty));
                editSB.AppendLine("Airstrikes (as) => " + (arena?.Airstrikes ?? false));
                editSB.AppendLine("Airstrike kills required (aks) => " + (arena?.AirstrikeKS ?? -1));
                editSB.AppendLine("Items (items) => " + (arena?.DMItems?.Count ?? 0));
                editSB.AppendLine("Allies items => " + (arena?.GetTeamItemInfos(ArenaInfo.Team.Allies)?.Count ?? 0));
                editSB.AppendLine("Axis items => " + (arena?.GetTeamItemInfos(ArenaInfo.Team.Axis)?.Count ?? 0));
                editSB.AppendLine("Max Players (mp) => " + (arena?.MaxPlayers ?? -1));
                editSB.AppendLine("Name (name): " + (arena?.ArenaName ?? "Unknown"));
                editSB.AppendLine("Zone ID (zone): " + (arena?.ZoneID ?? "Unknown"));
                editSB.AppendLine("Spawns (spawn): " + (arena?.SpawnList?.Count ?? 0));
                editSB.AppendLine("Allow dropping items (drop): " + (arena?.AllowDropping ?? false));
                editSB.AppendLine("Allow dropping activem items (dropactive): " + (arena?.AllowDroppingActiveItem ?? false));
                editSB.AppendLine("Allow players to build (build): " + (arena?.PlayersCanBuild ?? false));
                editSB.AppendLine("One in the Chamber mode (oc) =>: " + (arena?.OIC ?? false));
                editSB2.AppendLine("Teams (teams) =>: " + (arena?.Teams ?? false));
                editSB2.AppendLine("Score limit (scl) =>: " + (arena?.MaxScore ?? 0));
                editSB2.AppendLine("Time limit (tl) =>: " + (arena?.TimeLimit ?? 0));
                editSB2.AppendLine("Round based (rb) =>: " + (arena?.RoundBased ?? false));
                editSB2.AppendLine("Players needed for start (ps) =>: " + (arena?.PlayersForStart ?? 0));
                editSB2.AppendLine("Force constant day (day) =>: " + (arena?.ForceDay ?? false));
                editSB2.AppendLine("Network to players outside of arena (net) =>: " + (arena?.NetworkOut ?? true));
                editSB2.AppendLine("Always network to admins (adnet) =>: " + (arena?.AdminNetAlways ?? true));
                editSB2.AppendLine("Soft antihack (antihack) =>: " + (arena?.SoftAntiHack ?? false));
                editSB2.AppendLine("Respawn Time (rt) =>: " + (arena?.RespawnTime ?? 0f));
                editSB2.AppendLine("Spectate team on death (spec) =>: " + (arena?.SpectateTeamDead ?? false));
                editSB2.AppendLine("Max Ping (ping) =>: " + (arena?.MaxPing ?? 0));
                editSB2.AppendLine("Hide on list (hide) =>: " + (arena?.Hide ?? false));
                editSB2.AppendLine("Remove items on death (rod) =>: " + (arena?.RemoveItemsOnDeath ?? true));
                editSB2.AppendLine("Join mid round (mr) =>: " + (arena?.JoinMidRound ?? false));
                editSB2.AppendLine("Waiting area (wa) => : " + (arena?.WaitingArea ?? Vector3.zero));
                SendReply(player, editSB.ToString().TrimEnd());
                SendReply(player, editSB2.ToString().TrimEnd());
                return;
            }
            var arg0Lower = (args.Length > 0) ? args[0].ToLower() : string.Empty;
            if (arg0Lower == "wa")
            {
                SendReply(player, "Set waiting area to: " + (arena.WaitingArea = player?.transform?.position ?? Vector3.zero));
                return;
            }
            if (args.Length > 0 && (arg0Lower == "items" || arg0Lower == "gitems"))
            {
                if (args[0].ToLower() == "items")
                {
                    var itemList = player?.inventory?.AllItems()?.ToList() ?? null;
                    var itemInfos = new List<ItemInfo>();
                    if (itemList != null && itemList.Count > 0)
                    {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            var item = itemList[i];
                            if (item == null) continue;
                            var newInfo = new ItemInfo(item.info.shortname, item?.amount ?? 1, item?.skin ?? 0);
                            if (item.parent == player.inventory.containerWear) newInfo.containerName = "wear";
                            if (item.parent == player.inventory.containerMain) newInfo.containerName = "main";
                            if (item.parent == player.inventory.containerBelt) newInfo.containerName = "belt";
                            newInfo.itemPos = item?.position ?? -1;
                            var heldEnt = item?.GetHeldEntity() ?? null;
                            var baseProj = item?.GetHeldEntity()?.GetComponent<BaseProjectile>() ?? null;
                            newInfo.ammoName = baseProj?.primaryMagazine?.ammoType?.shortname ?? string.Empty;
                            newInfo.ammoAmount = baseProj?.primaryMagazine?.contents ?? 0;
                            if (heldEnt != null && heldEnt is Chainsaw)
                            {
                                newInfo.ammoAmount = (heldEnt as Chainsaw)?.ammo ?? 0;
                                newInfo.ammoName = "lowgradefuel";
                            }
                            var contents = item?.contents?.itemList ?? null;
                            if (contents != null && contents.Count > 0)
                            {
                                for (int j = 0; j < contents.Count; j++)
                                {
                                    var content = contents[j];
                                    if (content == null) continue;
                                    newInfo.mods[content.info.shortname] = content.amount;
                                }
                            }
                            itemInfos.Add(newInfo);
                        }
                    }
                    if (editTeam == ArenaInfo.Team.None) arena.DMItems = itemInfos;
                    else arena.TeamItems[editTeam] = itemInfos;
                    PrintWarning("set team infos: " + editTeam + ", count: " + (itemInfos?.Count ?? -1));
                    SendReply(player, "Set DM items to your inventory.");

                    return;
                }

                if (args[0].ToLower() == "gitems")
                {
                    var itemList = arena?.GetTeamItemList(editTeam) ?? null;
                    if (itemList == null || itemList.Count < 1)
                    {
                        SendReply(player, "No items!");
                        return;
                    }

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var item = itemList[i];
                        if (item == null) continue;
                        var itemInf = arena?.GetItemInfo(item) ?? null;
                        ItemContainer cont = null;
                        var itemPos = -1;
                        if (itemInf != null)
                        {
                            var contName = itemInf?.containerName ?? string.Empty;
                            cont = (contName == "wear") ? (player?.inventory?.containerWear ?? null) : (contName == "belt" ? (player?.inventory?.containerBelt ?? null) : (player?.inventory?.containerMain ?? null));
                            if (cont == null) PrintWarning("CONT NULL: " + contName);
                            itemPos = itemInf?.itemPos ?? -1;
                            if (cont != null && (itemPos + 1) > cont.capacity)
                            {
                                PrintWarning("Cont capacity lower than item pos: " + cont.capacity + ", item pos: " + itemPos + " (starting at 1: " + (itemPos + 1) + ")");
                                cont.capacity = itemPos + 1;
                            }
                        }
                        else PrintWarning("item inf null: " + item.info.shortname);
                        if (cont == null)
                        {
                            PrintWarning("cont null:" + item.info.shortname);
                            if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt))
                            {
                                PrintWarning("no move!");
                                RemoveFromWorld(item);
                            }
                        }
                        else
                        {
                            // PrintWarning("cont not null: " + item.info.shortname);
                            if (!item.MoveToContainer(cont, itemPos))
                            {
                                PrintWarning("NO move to cont: " + item.info.shortname + ", " + itemPos);
                                RemoveFromWorld(item);
                            }
                        }
                    }
                    SendReply(player, "Gave DM items!");
                    return;
                }
            }


            if (args.Length >= 2)
            {
                if (args[0].Equals("team", StringComparison.OrdinalIgnoreCase))
                {
                    int teamInt;
                    if (int.TryParse(args[1], out teamInt))
                    {
                        if (teamInt > 2)
                        {
                            SendReply(player, "Team integer should be between 0-2");
                            return;
                        }
                        var team = (ArenaInfo.Team)teamInt;
                        selectedEditTeam[player.UserIDString] = team;
                        SendReply(player, "Set selected edit team to: " + team);
                    }
                    else SendReply(player, "Not an integer, couldn't select team: " + args[1]);
                }
                if (args[0].Equals("spawntype", StringComparison.OrdinalIgnoreCase))
                {
                    int spawnInt;
                    if (int.TryParse(args[1], out spawnInt))
                    {
                        if (spawnInt > 2)
                        {
                            SendReply(player, "Spawn type integer should be between 0-2");
                            return;
                        }
                        var type = (SpawnPoint.SpawnType)spawnInt;
                        selectedSpawnType[player.UserIDString] = type;
                        SendReply(player, "Set selected edit spawn type to: " + type);
                    }
                    else SendReply(player, "Not an integer, couldn't select type: " + args[1]);
                }
                if (args[0].ToLower() == "ban")
                {
                    BasePlayer target = null;
                    var arg1 = args[1];
                    foreach (var p in arena.ActivePlayers)
                    {
                        if (p == null) continue;
                        if (p.UserIDString == args[1] || p.displayName.Equals(arg1, StringComparison.OrdinalIgnoreCase) || p.displayName.IndexOf(arg1, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            target = p;
                            break;
                        }
                    }
                    if (target == null) target = FindPlayerByPartialName(args[1], true);
                    if (target == null)
                    {
                        SendReply(player, "Couldn't find a player with the name or ID of: " + args[1]);
                        return;
                    }
                    if (arena.IsBanned(target.userID))
                    {
                        SendReply(player, target.displayName + " is already banned!");
                        return;
                    }
                    var endTime = DateTime.MinValue;
                    if (args.Length > 2)
                    {
                        if (!DateTime.TryParse(args[2], out endTime))
                        {
                            SendReply(player, "Not a DateTime: " + args[2]);
                            return;
                        }
                        else SendReply(player, "Using DateTime: " + endTime);
                    }

                    arena.Ban(target.userID, endTime);
                    SendReply(player, "Banned: " + target.displayName + ", until: " + endTime);
                }
                if (args[0].ToLower() == "unban")
                {
                    BasePlayer target = null;
                    var arg1 = args[1];
                    foreach (var p in arena.ActivePlayers)
                    {
                        if (p == null) continue;
                        if (p.UserIDString == args[1] || p.displayName.Equals(arg1, StringComparison.OrdinalIgnoreCase) || p.displayName.IndexOf(arg1, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            target = p;
                            break;
                        }
                    }
                    if (target == null) target = FindPlayerByPartialName(args[1], true);
                    if (target == null)
                    {
                        SendReply(player, "Couldn't find a player with the name: " + args[1]);
                        return;
                    }
                    if (!arena.IsBanned(target.userID))
                    {
                        SendReply(player, target.displayName + " is not banned!");
                        return;
                    }

                    arena.Unban(target.userID);
                    SendReply(player, "Unbanned: " + target.displayName);
                }
                if (args[0].ToLower() == "dmg")
                {
                    if (heldItem == null)
                    {
                        SendReply(player, "You must be holding an item to use this!");
                        return;
                    }
                    float scalar;
                    if (!float.TryParse(args[1], out scalar))
                    {
                        SendReply(player, "Bad scalar: " + args[1]);
                        return;
                    }
                    if (scalar == -1f) arena.ResetDmgScalar(heldItem.info);
                    else arena.SetDmgScalar(heldItem.info, scalar);
                    SendReply(player, "Set " + heldItem.info.displayName.english + " damage scalar to " + scalar);
                }
                if (args[0].ToLower() == "ping")
                {
                    int maxPing;
                    if (!int.TryParse(args[1], out maxPing))
                    {
                        SendReply(player, "Not an integer: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set max ping to: " + (arena.MaxPing = maxPing));
                }
                if (args[0].ToLower() == "mag")
                {
                    if (heldItem == null)
                    {
                        SendReply(player, "You must be holding an item to use this!");
                        return;
                    }
                    int capacity;
                    if (!int.TryParse(args[1], out capacity))
                    {
                        SendReply(player, "Bad mag size: " + args[1]);
                        return;
                    }
                    if (capacity == -1) arena.ResetMagSize(heldItem.info);
                    else arena.SetMagSize(heldItem.info, capacity);
                    arena.SetMagSize(heldItem.info, capacity);
                    SendReply(player, "Set " + heldItem.info.displayName.english + " mag size to " + capacity);
                }
                if (args[0].ToLower() == "fn")
                {
                    bool fnite;
                    if (!bool.TryParse(args[1], out fnite))
                    {
                        SendReply(player, "Not valid bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set fn to: " + (arena.Fortnite = fnite));
                }
                if (args[0].ToLower() == "rod")
                {
                    bool rod;
                    if (!bool.TryParse(args[1], out rod))
                    {
                        SendReply(player, "Not valid bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set remove on death to: " + (arena.RemoveItemsOnDeath = rod));
                }
                if (args[0].ToLower() == "teams")
                {
                    bool doTeams;
                    if (!bool.TryParse(args[1], out doTeams))
                    {
                        SendReply(player, "Not valid bool: " + args[1]);
                        return;
                    }
                    SendReply(player, "Set teams to: " + (arena.Teams = doTeams));
                }
                if (args[0].ToLower() == "scl")
                {
                    int limit;
                    if (!int.TryParse(args[1], out limit))
                    {
                        SendReply(player, "Invalid number: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set limit to: " + (arena.MaxScore = limit));
                }
                if (args[0].ToLower() == "spec")
                {
                    bool spec;
                    if (!bool.TryParse(args[1], out spec))
                    {
                        SendReply(player, "Not valid bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set spectate team on death to: " + (arena.SpectateTeamDead = spec));
                }
                if (args[0].ToLower() == "hide")
                {
                    bool hide;
                    if (!bool.TryParse(args[1], out hide))
                    {
                        SendReply(player, "Not a valid bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set hide on list to: " + (arena.Hide = hide));
                }
                if (args[0].ToLower() == "rt")
                {
                    float rt;
                    if (!float.TryParse(args[1], out rt) || rt < 0.0)
                    {
                        SendReply(player, "Not valid number: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set respawn time to: " + (arena.RespawnTime = rt));
                }
                if (args[0].ToLower() == "tl")
                {
                    float timeLimit;
                    if (!float.TryParse(args[1], out timeLimit))
                    {
                        SendReply(player, "Invalid number: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set time limit to: " + (arena.TimeLimit = timeLimit));
                }
                if (args[0].ToLower() == "rb")
                {
                    bool round;
                    if (!bool.TryParse(args[1], out round))
                    {
                        SendReply(player, "Not a bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set round based to: " + (arena.RoundBased = round));
                }
                if (args[0].ToLower() == "day")
                {
                    bool day;
                    if (!bool.TryParse(args[1], out day))
                    {
                        SendReply(player, "Not a bool: " + args[1]);
                        return;
                    }
                    else
                    {
                        arena.ForceDay = day;
                        if (day) arena.ForceNight = false;

                        SendReply(player, "Set force day to: " + day);
                    }
                }
                if (args[0].ToLower() == "night")
                {
                    bool night;
                    if (!bool.TryParse(args[1], out night))
                    {
                        SendReply(player, "Not a bool: " + args[1]);
                        return;
                    }
                    else
                    {
                        arena.ForceNight = night;
                        if (night) arena.ForceDay = false;

                        SendReply(player, "Set force night to: " + night);
                    }
                }
                if (args[0].ToLower() == "net")
                {
                    bool net;
                    if (!bool.TryParse(args[1], out net))
                    {
                        SendReply(player, "Not a bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set network out to: " + (arena.NetworkOut = net));
                }
                if (args[0].ToLower() == "adnet")
                {
                    bool net;
                    if (!bool.TryParse(args[1], out net))
                    {
                        SendReply(player, "Not a bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set admin network to: " + (arena.AdminNetAlways = net));
                }
                if (args[0].ToLower() == "antihack")
                {
                    bool ah;
                    if (!bool.TryParse(args[1], out ah))
                    {
                        SendReply(player, "Not a bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set soft antihack to: " + (arena.SoftAntiHack = ah));
                }
                if (args[0].ToLower() == "ps")
                {
                    int start;
                    if (!int.TryParse(args[1], out start))
                    {
                        SendReply(player, "Not an int: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set players needed for start to: " + (arena.PlayersForStart = start));
                }
                if (args[0].ToLower() == "mr")
                {
                    bool midRound;
                    if (!bool.TryParse(args[1], out midRound))
                    {
                        SendReply(player, "Not a bool: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set mid round: " + (arena.JoinMidRound = midRound));
                }
                if (args[0].ToLower() == "type")
                {
                    int type;
                    if (!int.TryParse(args[1], out type))
                    {
                        SendReply(player, "Not valid int: " + args[1]);
                        return;
                    }
                    if (type > 2)
                    {
                        SendReply(player, "Type is bad: " + type);
                        return;
                    }
                    var newType = (ArenaInfo.GameType)type;
                    SendReply(player, "Set teams to: " + (arena.Type = newType));
                }
                if (args[0].ToLower() == "spt")
                {
                    var spt = 0f;
                    if (!float.TryParse(args[1], out spt))
                    {
                        SendReply(player, "Bad SPT: " + args[1]);
                        return;
                    }
                    else arena.SpawnProtectionTime = spt;
                    SendReply(player, "Set spt to: " + args[1]);
                }
                if (args[0].ToLower() == "drop")
                {
                    var drop = false;
                    if (!bool.TryParse(args[1], out drop))
                    {
                        SendReply(player, "Not true/false: " + args[1]);
                        return;
                    }
                    arena.AllowDropping = drop;
                    SendReply(player, "Set 'Allow dropping items' to: " + arena.AllowDropping);
                }
                if (args[0].Equals("dropactive", StringComparison.OrdinalIgnoreCase))
                {
                    var drop = false;
                    if (!bool.TryParse(args[1], out drop))
                    {
                        SendReply(player, "Not true/false: " + args[1]);
                        return;
                    }
                    arena.AllowDroppingActiveItem = drop;
                    SendReply(player, "Set 'Allow dropping active item' to: " + arena.AllowDroppingActiveItem);
                }
                if (args[0].ToLower() == "oc")
                {
                    var isOIC = false;
                    if (!bool.TryParse(args[1], out isOIC))
                    {
                        SendReply(player, "Not true/false: " + args[1]);
                        return;
                    }
                    arena.OIC = isOIC;
                    //   arena.AllowDropping = drop;
                    SendReply(player, "Set 'OIC' to: " + arena.OIC);
                }
                if (args[0].ToLower() == "build")
                {
                    var build = false;
                    if (!bool.TryParse(args[1], out build))
                    {
                        SendReply(player, "Not true/false: " + args[1]);
                        return;
                    }
                    arena.PlayersCanBuild = build;
                    SendReply(player, "Set 'Allow players build' to: " + arena.AllowDropping);
                }
                if (args[0].ToLower() == "disabled")
                {
                    var disabled = false;
                    if (!bool.TryParse(args[1], out disabled))
                    {
                        SendReply(player, "Not true/false: " + args[1]);
                        return;
                    }
                    arena.Disabled = disabled;
                    SendReply(player, "Set 'Disabled' to: " + disabled);
                    return;
                }
                if (args[0].ToLower() == "dtext")
                {
                    SendReply(player, "Set disabled text to: " + (arena.DisabledText = args[1]));
                    return;
                }
                if (args[0].ToLower() == "firetime")
                {
                    var newLife = 0f;
                    if (!float.TryParse(args[1], out newLife))
                    {
                        SendReply(player, "Not a number: " + args[1]);
                        return;
                    }
                    SendReply(player, "Set fire life time to: " + (arena.FireballTime = newLife));
                    return;
                }
                if (args[0].ToLower() == "aks")
                {
                    var newKS = 0;
                    if (!int.TryParse(args[1], out newKS))
                    {
                        SendReply(player, "Not a number: " + args[1]);
                        return;
                    }
                    SendReply(player, "Set airstrike killstreak to: " + (arena.AirstrikeKS = newKS));
                }
                if (args[0].ToLower() == "as")
                {
                    var newBool = false;
                    if (!bool.TryParse(args[1], out newBool))
                    {
                        SendReply(player, "Not true/false: " + args[1]);
                        return;
                    }
                    SendReply(player, "Set airstrikes enabled to: " + (arena.Airstrikes = newBool));
                }
                if (args[0].ToLower() == "health")
                {
                    var health = 0f;
                    if (!float.TryParse(args[1], out health))
                    {
                        SendReply(player, "Bad health specified: " + args[1]);
                        return;
                    }
                    else arena.SpawnWithHealth = Mathf.Clamp(health, 1, 100);
                    SendReply(player, "Set player spawn health to: " + arena.SpawnWithHealth);
                }
                if (args[0].ToLower() == "itemdrop")
                {
                    var isClear = args[1].Equals("clear", StringComparison.OrdinalIgnoreCase);
                    var chance = 0f;
                    if (!isClear)
                    {

                        if (!float.TryParse(args[1], out chance))
                        {
                            SendReply(player, "Bad chance specified: " + args[1]);
                            return;
                        }
                    }
                    if (isClear)
                    {
                        var oldCount = arena.ItemSpawns.Count;
                        arena.ClearSpawnedItems();
                        arena.ItemSpawns.Clear();
                        SendReply(player, "Cleared all item drops (" + oldCount.ToString("N0") + ") for arena: " + arena.ArenaName);
                        return;
                    }
                    if (chance < 0f || chance > 100f)
                    {
                        SendReply(player, "Chance must be between 0 and 100!");
                        return;
                    }
                    if (heldItem == null)
                    {
                        SendReply(player, "You must be holding the item you wish to make a drop for!");
                        return;
                    }
                    var plyPos = player?.transform?.position ?? Vector3.zero;
                    if (plyPos == Vector3.zero)
                    {
                        SendReply(player, "Bad position");
                        return;
                    }
                    var spawnItem = new ItemSpawn
                    {
                        Item = new ItemInfo(heldItem.info.shortname, heldItem.amount, heldItem.skin),
                        Position = new Vector3(plyPos.x, plyPos.y + 0.1f, plyPos.z),
                        ChanceToSpawn = chance
                    };
                    arena.ItemSpawns.Add(spawnItem);
                    SendReply(player, "Created drop spawn item: " + heldItem.info.displayName.english + " x" + heldItem.amount + ", chance: " + chance);
                }
                if (args[0].ToLower() == "lootbox")
                {
                    if (args[1].Equals("show", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var spawn in arena.BoxSpawns)
                        {
                            player.SendConsoleCommand("ddraw.sphere", 15f, Color.green, spawn.Position, 1f);
                            var str = "Chance: " + spawn.ChanceToSpawn.ToString("0.00") + Environment.NewLine + "i: <color=#a6fc60>" + arena.BoxSpawns.FindIndex(p => p == spawn).ToString("N0") + "</color>" + (!string.IsNullOrEmpty(spawn.PrefabName) ? " prefab: " + spawn.PrefabName : "");
                            player.SendConsoleCommand("ddraw.text", 15f, Color.green, spawn.Position, str);
                        }
                        return;
                    }
                    var isClear = args[1].Equals("clear", StringComparison.OrdinalIgnoreCase);
                    var isRemove = args[1].Equals("remove", StringComparison.OrdinalIgnoreCase);
                    var chance = 0f;
                    if (!isClear && !isRemove)
                    {
                        if (!float.TryParse(args[1], out chance))
                        {
                            SendReply(player, "Bad chance specified: " + args[1]);
                            return;
                        }
                    }
                    if (isRemove)
                    {
                        if (args.Length < 3)
                        {
                            SendReply(player, "You must specify an index to remove!");
                            return;
                        }
                        int index;
                        if (!int.TryParse(args[2], out index))
                        {
                            SendReply(player, "Not an int: " + args[2]);
                            return;
                        }
                        if (index < 0 || index > (arena.BoxSpawns.Count - 1))
                        {
                            SendReply(player, "Index out of range. Box spawns count is: " + arena.BoxSpawns.Count);
                            return;
                        }
                        arena.BoxSpawns.RemoveAt(index);
                        SendReply(player, "Removed box at index: " + index);
                        return;
                    }

                    if (isClear)
                    {
                        var oldCount = arena.BoxSpawns.Count;
                        arena.ClearSpawnedBoxes();
                        arena.BoxSpawns.Clear();
                        SendReply(player, "Cleared all box spawns (" + oldCount.ToString("N0") + ") for arena: " + arena.ArenaName);
                        return;
                    }
                    if (chance < 0f || chance > 100f)
                    {
                        SendReply(player, "Chance must be between 0 and 100!");
                        return;
                    }
                    var plyPos = player?.transform?.position ?? Vector3.zero;
                    if (plyPos == Vector3.zero)
                    {
                        SendReply(player, "Bad position");
                        return;
                    }
                    var ammo = false;
                    if (args.Length > 2 && args[2].ToLower() == "ammo") ammo = true;
                    var spawnBox = new BoxSpawn
                    {
                        ChanceToSpawn = chance
                    };
                    if (ammo) spawnBox.PrefabName = ammoBoxPrefab;
                    var usePos = new Vector3(plyPos.x, plyPos.y + 0.1f, plyPos.z);
                    spawnBox.Position = usePos;
                    arena.BoxSpawns.Add(spawnBox);
                    SendReply(player, "Created loot box with chance: " + chance + " at position: " + usePos + ", is ammo: " + ammo);
                }
                if (args[0].ToLower() == "bleed")
                {
                    var bleed = 0f;
                    if (!float.TryParse(args[1], out bleed))
                    {
                        SendReply(player, "Bad bleed scalar specified: " + args[1]);
                        return;
                    }
                    else arena.BleedingScalar = bleed;
                    SendReply(player, "Set bleed scalar to: " + arena.BleedingScalar);
                }
                if (args[0].ToLower() == "heal")
                {
                    var heal = 0f;
                    if (!float.TryParse(args[1], out heal))
                    {
                        SendReply(player, "Bad heal scalar specified: " + args[1]);
                        return;
                    }
                    else arena.HealingScalar = heal;
                    SendReply(player, "Set heal scalar to: " + arena.HealingScalar);
                }
                if (args[0].ToLower() == "ngt")
                {
                    var time = 0f;
                    if (!float.TryParse(args[1], out time))
                    {
                        SendReply(player, "Not a float: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set new game time to: " + (arena.NewGameTime = time));
                }
                if (args[0].ToLower() == "lootscale")
                {
                    var scalar = 0f;
                    if (!float.TryParse(args[1], out scalar))
                    {
                        SendReply(player, "Not a float: " + args[1]);
                        return;
                    }
                    else SendReply(player, "Set loot chance scalar to: " + (arena.LootChanceScalar = scalar));
                }
                if (args[0].ToLower() == "desc")
                {
                    arena.Description = args[1];
                    SendReply(player, "Set description to: " + args[1]);
                }

                if (args[0].ToLower() == "name")
                {
                    var foundArena = FindArena(args[1]);
                    if (foundArena != null && foundArena.ArenaName == args[1])
                    {
                        SendReply(player, "This is already the name or an arena with this name already exists!");
                        return;
                    }
                    arena.ArenaName = args[1];
                    SendReply(player, "Set arena name to: " + args[1]);
                }
                if (args[0].ToLower() == "zone")
                {
                    var zoneIDL = 0L;
                    if (!long.TryParse(args[1], out zoneIDL))
                    {
                        SendReply(player, "Bad zone ID: " + args[1]);
                        return;
                    }
                    arena.ZoneID = args[1];
                    SendReply(player, "Set zone ID to: " + args[1]);
                }
                if (args[0].ToLower() == "mp")
                {
                    var mpCount = 0;
                    if (!int.TryParse(args[1], out mpCount))
                    {
                        SendReply(player, "Bad player count: " + args[1]);
                        return;
                    }
                    arena.MaxPlayers = mpCount;
                    SendReply(player, "Set max players to: " + args[1]);
                }
                if (args[0].ToLower() == "spawn")
                {
                    var arg1Lower = args[1].ToLower();
                    if (arg1Lower != "list" && arg1Lower != "add" && arg1Lower != "delete" && arg1Lower != "clear" && arg1Lower != "show" && arg1Lower != "autoadd")
                    {
                        SendReply(player, "Invalid spawn arg: " + args[1]);
                        return;
                    }

                    if (arg1Lower == "autoadd")
                    {
                        int parseSpawn;
                        List<SpawnPoint> points;
                        if (editTeam != ArenaInfo.Team.None)
                        {
                            if (!arena.teamSpawnPoints.TryGetValue(editTeam, out points))
                            {
                                SendReply(player, "no teamSpawnPoints found for: " + editTeam + " !!");
                                return;
                            }
                            else PrintWarning("Got spawn points list for: " + editTeam);
                        }
                        else points = arena?.spawnPoints ?? null;

                        if (points == null)
                        {
                            SendReply(player, "Got null spawn points list?!");
                            return;
                        }
                        var maxSpawn = -1;
                        var lastSpawn = 0;
                        if (points.Count > 0)
                        {
                            for (int i = 0; i < points.Count; i++)
                            {
                                var point = points[i];
                                if (int.TryParse(point.Name, out parseSpawn))
                                {
                                    lastSpawn = parseSpawn;
                                    if (lastSpawn > maxSpawn) maxSpawn = lastSpawn;
                                }
                                else PrintWarning("Spawn name: " + point.Name + " is not an integer!");
                            }
                        }
                        else PrintWarning("No spawn points found, it wil default to 0");

                        var useSpawn = (maxSpawn + 1).ToString();

                        arena.AddSpawn(useSpawn, player?.transform?.position ?? Vector3.zero, editType, editTeam);
                        SendReply(player, "Added auto spawn: " + useSpawn);
                    }

                    var spawnList = (editTeam == ArenaInfo.Team.None) ? arena?.SpawnList ?? null : (editTeam == ArenaInfo.Team.Allies) ? arena?.AlliesSpawns ?? null : arena?.AxisSpawns ?? null;
                    if (arg1Lower == "list")
                    {
                        var spawnSB = new StringBuilder();

                        if ((spawnList?.Count ?? 0) < 1)
                        {
                            SendReply(player, "No spawns! Add a spawn using /dm_edit spawn add");
                            return;
                        }
                        for (int i = 0; i < spawnList.Count; i++) spawnSB.Append(i + ": " + spawnList[i] + ", ");
                        SendReply(player, "Spawns for " + (arena?.ArenaName ?? "Unknown") + ":" + Environment.NewLine + spawnSB.ToString().TrimEnd(", ".ToCharArray()));
                    }
                    if (arg1Lower == "clear")
                    {
                        if (editTeam == ArenaInfo.Team.None) arena.ClearSpawns();
                        else arena.ClearTeamSpawns(editTeam);
                        SendReply(player, "Cleared spawns!");
                    }
                    if (arg1Lower == "show")
                    {
                        if (spawnList == null || spawnList.Count < 1)
                        {
                            SendReply(player, "There are no spawns in this DM.");
                            return;
                        }
                        var revList = spawnList.ToList();
                        revList.Reverse();
                        for (int i = 0; i < revList.Count; i++)
                        {
                            var spawn = revList[i];
                            player.SendConsoleCommand("ddraw.text", 15f, Color.white, spawn, "Spawn: " + arena.GetSpawnName(spawn) + " (#" + (i + 1).ToString("N0") + ")");
                            player.SendConsoleCommand("ddraw.sphere", 15f, Color.blue, spawn, 2.5f);
                        }
                    }
                    if ((arg1Lower == "add" || arg1Lower == "delete") && args.Length < 3)
                    {
                        SendReply(player, "Invalid args (length < 3), try: /dm_edit spawn add X");
                        return;
                    }

                    if (arg1Lower == "add")
                    {
                        if (arena.FindSpawnByName(args[2]) != Vector3.zero)
                        {
                            SendReply(player, "A spawn with this name already exists!");
                            return;
                        }
                        arena.AddSpawn(args[2], player?.transform?.position ?? Vector3.zero, editType, editTeam);
                        SendReply(player, "Added new spawn for " + editTeam + ": " + args[2] + ", " + (player?.transform?.position ?? Vector3.zero) + ", type: " + editType);
                    }
                    if (arg1Lower == "delete")
                    {
                        if (arena.FindSpawnByName(args[2]) == Vector3.zero)
                        {
                            SendReply(player, "No spawn found with this name!");
                            return;
                        }
                        arena.RemoveSpawn(args[2]);
                        SendReply(player, "Removed spawn: " + args[2]);
                    }
                }
            }
            //else if (findArena == null) SendReply(player, "Invalid args!");
        }

        private readonly Dictionary<string, ArenaInfo> teamJoin = new Dictionary<string, ArenaInfo>();

        private bool CanJoinDM(BasePlayer player, ArenaInfo arena, out string reason)
        {
            reason = string.Empty;
            if (player == null || arena == null) return false;
            if ((arena?.Disabled ?? false) && !player.IsAdmin)
            {
                reason = "Unable to join: " + (arena?.DisabledText ?? "Unknown Reason");
                return false;
            }
            if (!(arena?.CanJoinAnytime ?? true))
            {
                reason = "You cannot join this game while it's in progress!";
                return false;
            }
            if (InAnyDM(player.UserIDString))
            {
                var curDM = GetPlayerDM(player);
                if (curDM == arena)
                {
                    reason = "You're already in this arena!";
                    return false;
                }
                if (curDM != null && !curDM.LeaveDM(player))
                {
                    PrintWarning("GetPlayerDM was not null, but failed to leave! Player name: " + player.displayName + ", arena: " + curDM?.ArenaName);
                    reason = "You're already in a DM! try <color=#fcb355>/dm leave</color>, first.";
                    return false;
                }
            }
            if (arena.ActivePlayers.Count >= arena.MaxPlayers)
            {
                reason = "This arena is currently full! Please try again later.";
                return false;
            }
            if ((player?.metabolism?.radiation_level?.value ?? 0) >= 10 || (player?.metabolism?.radiation_poison?.value ?? 0) >= 10)
            {
                reason = "Your radiation is too high to join Deathmatch!";
                return false;
            }
            if (player.GetParentEntity() != null)
            {
                reason = "You cannot join while parented!";
                return false;
            }
            if (player.IsHostile() && player.InSafeZone())
            {
                reason = "You cannot join while hostile!";
                return false;
            }
            var ping = GetPing(player);
            if (ping > arena.MaxPing)
            {
                reason = "You cannot join: Your ping is: " + ping.ToString("N0") + ", the maximum in this arena is: " + arena.MaxPing.ToString("N0");
                return false;
            }
            if (!InAnyDM(player.UserIDString) && !(player?.CanBuild() ?? false) && !(arena?.CanJoinBuildingBlocked ?? false))
            {
                reason = "You cannot join while building blocked!";
                return false;
            }
            if (player.isMounted)
            {
                reason = "You cannot join while sitting!";
                return false;
            }
            if (IsCrafting(player))
            {
                reason = "You cannot join while crafting!";
                return false;
            }

            if (player.IsWounded())
            {
                reason = "You cannot join while wounded!";
                return false;
            }
            if (player.IsSwimming())
            {
                reason = "You cannot join while swimming!";
                return false;
            }
            if (!player.IsOnGround() && !player.IsFlying)
            {
                reason = "You cannot join while in mid-air!";
                return false;
            }
            if (player.HasPlayerFlag(BasePlayer.PlayerFlags.SafeZone))
            {
                reason = "You cannot join while in a safe zone!";
                return false;
            }
            if (IsEscapeBlocked(player.UserIDString))
            {
                reason = "You cannot join while combat/raid blocked!";
                return false;
            }
            var banInfo = arena.GetBan(player.userID);
            var now = DateTime.UtcNow;
            if (banInfo != null && (banInfo.Permanent || now < banInfo.EndTime))
            {
                var str = banInfo.EndTime != null ? ReadableTimeSpan((DateTime)banInfo.EndTime - now, "0.0") : string.Empty;
                reason = "You are banned from this Deathmatch arena " + ((banInfo.Permanent || string.IsNullOrEmpty(str)) ? "permanently!" : "for: " + str);
                return false;
            }
            if (!player.IsAdmin)
            {
                var lastAtk = player?.lastAttackedTime ?? -1f;
                var atkCooldown = arena?.LastAttackedCooldown ?? 30;
                var lastDmg = player?.lastDamage ?? Rust.DamageType.Generic;
                if (((Time.realtimeSinceStartup - lastAtk) < atkCooldown) && lastDmg != Rust.DamageType.Generic && lastDmg != Rust.DamageType.Cold && lastDmg != Rust.DamageType.ColdExposure)
                {
                    reason = "You have been hurt within the last " + atkCooldown.ToString("N0") + " seconds! Please wait.";
                    return false;
                }
            }
            var zoneID = arena?.ZoneID ?? string.Empty;
            if (string.IsNullOrEmpty(zoneID) || zoneID == "0")
            {
                reason = "This arena does not have a zoneID! (not setup yet)";
                return false;
            }
            if ((arena?.SpawnList?.Count ?? 0) < 1 && (arena?.teamSpawnPoints?.Count ?? 0) < 1)
            {
                reason = "This arena does not have any spawns! (not setup yet)";
                return false;
            }
            var heliIns = PatrolHelicopterAI.heliInstance;
            if (heliIns != null && heliIns?.helicopterBase != null && !heliIns.isDead && !heliIns.helicopterBase.IsDead() && !heliIns.helicopterBase.IsDestroyed)
            {
                var isTarget = PatrolHelicopterAI.heliInstance?._targetList?.Any(p => p != null && p.ply != null && p.ply.UserIDString == player.UserIDString) ?? false;
                if (isTarget)
                {
                    reason = "You cannot join an arena while being targeted by a helicopter.";
                    return false;
                }
            }

            return true;
        }
        [ChatCommand("dm")]
        private void cmdDM3(BasePlayer player, string command, string[] args)
        {
            var now = Time.realtimeSinceStartup;
            var lastTime = LastDMTime(player.UserIDString);
            if (lastTime != -1f && (now - lastTime) < 15 && !player.IsAdmin)
            {
                SendReply(player, "You must wait a total of 10 seconds before using this command again!");
                return;
            }
            var msgSB = new StringBuilder();
            msgSB.AppendLine("<color=#fcb355>Deathmatches</color> are PVP arenas that are completely separate from the rest of the game. When you join an arena, your items are saved and returned to you after you leave the arena.");
            msgSB.AppendLine("\nType <color=#fcb355>/dm list</color>, to view a list of Deathmatch arenas you can join using <color=#fcb355>/dm join ArenaName</color>");
            msgSB.AppendLine("\n<color=#a6fc60>Be sure to check your inventory when you join! Any items you put into your belt will be in the same position after you respawn!</color>\n\nYou can leave Deathmatch at any time by typing <color=#fcb355>/dm leave</color>.\nIf your item positions keep resetting, try typing <color=#fcb355>/dm reset</color>. This will clear all your Deathmatch data.");
            var msg = msgSB.ToString().TrimEnd();
            if (args.Length <= 0)
            {
                SendReply(player, msg);
                return;
            }
            var getDM = GetPlayerDM(player);
            if (getDM == null) getDM = Arenas?.Where(p => p.InQueue(player))?.FirstOrDefault() ?? null;

            if (getDM != null && getDM.ForceJoin && !player.IsAdmin)
            {
                SendReply(player, "You may not use this right now.");
                return;
            }
            var arg0Lower = args[0].ToLower();
            var arg1Lower = args.Length > 1 ? args[1].ToLower() : string.Empty;
            if (arg0Lower == "list")
            {
                var dmSB = new StringBuilder();
                var dmSBList = new List<StringBuilder>();
                for (int i = 0; i < Arenas.Count; i++)
                {
                    var arena = Arenas[i];
                    if (arena == null || arena.LocalArena || arena.Hide) continue;
                    var desc = (!string.IsNullOrEmpty(arena?.Description)) ? arena.Description : "No description";
                    if (dmSB.Length >= 768)
                    {
                        dmSBList.Add(dmSB);
                        dmSB = new StringBuilder();
                    }
                    dmSB.AppendLine((arena.Disabled ? "<color=red>(DISABLED) </color>" : string.Empty) + "<color=#fcb355>" + (arena?.ArenaName ?? "Unknown") + "</color> - " + desc + " - Players: <color=#fcb355>" + (arena?.ActivePlayers?.Count ?? 0) + "</color>/<color=#fcb355>" + (arena?.MaxPlayers ?? -1) + "</color>");
                }
                SendReply(player, "You can join an arena by typing /dm join ArenaName");
                if (dmSBList.Count > 0) for (int i = 0; i < dmSBList.Count; i++) SendReply(player, dmSBList[i].ToString().TrimEnd());
                if (dmSB.Length > 0) SendReply(player, dmSB.ToString().TrimEnd());
                //    SendReply(player, (dmSB.Length > 0) ? dmSB.ToString().TrimEnd() : "No arenas!");
                return;
            }
            else if (arg0Lower == "reset")
            {
                if (getDM != null && !getDM.LeaveDM(player))
                {
                    SendReply(player, "Couldn't leave Deathmatch to reset data. Try leaving manually.");
                    return;
                }
                if (arg1Lower == "allplayers" && player.IsAdmin)
                {
                    for (int i = 0; i < Arenas.Count; i++)
                    {
                        var arena = Arenas[i];
                        if (arena == null) continue;
                        if (arena?.ActivePlayers != null && arena.ActivePlayers.Count > 0) foreach (var ply in arena.ActivePlayers) arena.LeaveDM(ply);
                        arena.dmItemPositions = new Dictionary<string, List<ItemPosition>>();
                        arena.playerItemPositions = new Dictionary<string, List<ItemPosition>>();
                        arena.playerItemSkins = new Dictionary<string, Dictionary<string, ulong>>();
                        arena.playerModInfos = new Dictionary<string, List<ModInfo>>();
                        arena.playerAmmos = new Dictionary<string, Dictionary<string, string>>();
                    }
                    SendReply(player, "Reset all player's data");
                }
                else
                {
                    for (int i = 0; i < Arenas.Count; i++)
                    {
                        var arena = Arenas[i];
                        if (arena == null) continue;
                        arena.dmItemPositions[player.UserIDString] = new List<ItemPosition>();
                        arena.playerItemPositions[player.UserIDString] = new List<ItemPosition>();
                        arena.playerItemSkins[player.UserIDString] = new Dictionary<string, ulong>();
                        arena.playerModInfos[player.UserIDString] = new List<ModInfo>();
                        arena.playerAmmos[player.UserIDString] = new Dictionary<string, string>();
                    }
                }
                SendReply(player, "Reset Deathmatch data successfully.");
                return;
            }
            else if (arg0Lower == "leave")
            {
                if (getDM == null)
                {
                    SendReply(player, "You are not currently in a Deathmatch!");
                    return;
                }
                if (!getDM.LeaveDM(player))
                {
                    SendReply(player, "Failed to leave deathmatch! Please report this to an administrator.");
                    PrintWarning("Failed to leave deathmatch: " + player.displayName);
                    return;
                }

                SendReply(player, "You have left <color=#fcb355>" + getDM.ArenaName + "</color>.");
                SendMessageToDM(getDM, "<color=#a6fc60>" + player.displayName + "</color> has left the <color=#fcb355>" + getDM.ArenaName + "</color> arena.");
                if (player?.metabolism != null)
                {
                    player.metabolism.radiation_level.max = 500f;
                    player.metabolism.radiation_poison.max = 500f;
                }
                GrantImmunity(player, 1.5f);
                WakePlayerUp(player, true);
                return;
            }
            else if (arg0Lower == "cancel")
            {
                if (getDM == null)
                {
                    SendReply(player, "Failed to get DM!");
                    return;
                }
                if (!getDM.InQueue(player))
                {
                    SendReply(player, "You're not currently in a queue to join!");
                    return;
                }
                else
                {
                    if (getDM.LeaveQueue(player))
                    {
                        SendReply(player, "Left queue!");
                        if (getDM.WaitingArea != Vector3.zero)
                        {
                            var tpPos = string.Empty;
                            if (!dmData.lastPlayerPositions.TryGetValue(player.UserIDString, out tpPos)) PrintWarning("no last pos to tp back from waiting area!!");
                            else
                            {
                                var tpVec = GetVector3FromString(tpPos);
                                TeleportPlayer(player, tpVec);
                            }
                        }
                    }
                    else SendReply(player, "Couldn't leave queue!");
                }
            }
            else if (arg0Lower == "join")
            {
                if (args.Length < 2)
                {
                    SendReply(player, msg);
                    return;
                }
                var lowerArg1 = args[1].ToLower();
                var arenaFind = FindArenaByPartialName(lowerArg1);
                if (arenaFind == null)
                {
                    SendReply(player, "No arena found with the name: " + args[1] + "\nType <color=#fcb355>/dm list</color> to see all available arenas.");
                    return;
                }
                var reason = string.Empty;
                var canJoin = CanJoinDM(player, arenaFind, out reason);
                if (!canJoin)
                {
                    if (!string.IsNullOrEmpty(reason)) SendReply(player, reason);
                    else SendReply(player, "Unable to join Deathmatch at this time.");
                    return;
                }

                string lastDMTimeStr;
                DateTime lastDmTime;
                if (dmData.lastDMTime.TryGetValue(player.UserIDString, out lastDMTimeStr) && DateTime.TryParse(lastDMTimeStr, out lastDmTime) && (DateTime.Now - lastDmTime).TotalSeconds < 5)
                {
                    SendReply(player, "You must wait before joining Deathmatch again!");
                    return;
                }

                if (arenaFind.RoundBased && !arenaFind.JoinMidRound && arenaFind.GameInProgress && !arenaFind.StartingGame)
                {
                    SendReply(player, "A game is currently in progress. You have been placed in a queue to join. To cancel, type <color=#fcb355>/dm cancel</color>");
                    if (getDM != null) getDM.LeaveQueue(player);
                    arenaFind.LeaveQueue(player);
                    if (arenaFind.JoinQueue(player)) SendReply(player, "Joined queue for: <color=#a6fc60>" + arenaFind.ArenaName + "</color>");
                    else SendReply(player, "Failed to join queue!");
                    /*/
                    Timer endTimer = null;
                    if (queueTimer.TryGetValue(player.UserIDString, out endTimer))
                    {
                        endTimer.Destroy();
                        endTimer = null;
                    }
                    endTimer = timer.Once(1f, () =>
                    {
                        if (arenaFind.GameInProgress) return;
                        canJoin = CanJoinDM(player, arenaFind, out reason);
                        if (!canJoin)
                        {
                            if (!string.IsNullOrEmpty(reason)) SendReply(player, reason);
                            else SendReply(player, "Unable to join Deathmatch at this time.");
                            return;
                        }
                        if (!arenaFind.JoinDM(player, true)) PrintWarning("Got all clear for joining DM on queue, but failed to join (returned false)!");
                        endTimer.Destroy();
                        endTimer = null;
                    });/*/
                    return;
                }

                var oldPos = player?.transform?.position ?? Vector3.zero;
                dmData.lastPlayerPositions[player.UserIDString] = GetVectorString(oldPos);
                /*/
                if (arenaFind.Teams)
                {
                    var team = ArenaInfo.Team.None;
                    if (args.Length < 3 || args[2].Equals("s", StringComparison.OrdinalIgnoreCase))
                    {
                        team = arenaFind.GetBestTeam();
                        PrintWarning("found best team: " + team);

                        SendReply(player, "Found best team: " + team);
                    }
                    else
                    {
                        int teamInt;
                        if (!int.TryParse(args[2], out teamInt))
                        {
                            SendReply(player, "Bad number");
                            return;
                        }
                        PrintWarning("got teamint: " + teamInt);
                        team = (ArenaInfo.Team)Mathf.Clamp(teamInt, 0, 1);
                        PrintWarning("team int = " + team);
                    }
                    arenaFind.SetTeam(player, team);
                    SendReply(player, "Set team: " + team);
                }/*/
                if (!arenaFind.Teams && !arenaFind.JoinDM(player))
                {
                    if (player.transform.position.ToString() != oldPos.ToString()) TeleportPlayer(player, oldPos);
                    SendReply(player, "Could not join DM! This arena may not have spawns set up yet.");
                    return;
                }
                dmData.lastDMTime[player.UserIDString] = DateTime.Now.ToString();
                dmData.lastDMName[player.UserIDString] = arenaFind.ArenaName;
                if (!arenaFind.Teams)
                {
                    GrantImmunity(player, 1.25f);
                    WakePlayerUp(player, true);
                    arenaFind.StopTimeGUI(player);
                }

                var isSilent = args.Length >= 3 && args[2].ToLower() == "s";
                if (!isSilent && !arenaFind.Disabled) SimpleBroadcast("<color=#a6fc60>" + player.displayName + "</color> has joined the <color=#fcb355>" + arenaFind.ArenaName + "</color> deathmatch arena! Type <color=#fcb355>/dm</color> to join them.");

                SendReply(player, "Joined <color=#fcb355>" + arenaFind.ArenaName + "</color> arena! Type <color=#fcb355>/dm leave</color> to leave.");
                if (arenaFind.Teams)
                {
                    JoinGUI(player, arenaFind);
                    teamJoin[player.UserIDString] = arenaFind;
                    //   if (arenaFind?.ActivePlayers != null && !arenaFind.ActivePlayers.Contains(player)) arenaFind.ActivePlayers.Add(player);
                    // var joinTeamMsg = "<size=20>Joined team <color=#fcb355>" + arenaFind.GetTeam(player) + "</color>!</size>";
                    //ShowPopup(player.UserIDString, joinTeamMsg);
                    //  SendReply(player, joinTeamMsg);
                }
            }
            else SendReply(player, msgSB.ToString().TrimEnd());
        }
        #endregion
    }
}
