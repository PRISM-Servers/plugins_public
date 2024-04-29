using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("ShadowBan", "Shady", "1.0.0", ResourceId = 0)]
    class ShadowBan : RustPlugin
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
        bool init = false;

        void Unload()
        {
            SavePlayerFile();
        }
        void OnPlayerConnected(BasePlayer player)
        {
            if (player != null && isShadowBanned(player)) UpdateHooks();
        }

        void OnServerSave()
        {

            UpdateHooks();
        }

        private void ShockPlayer(BasePlayer target, double dmg)
        {
            if (target == null || !target.IsAlive()) return;
            var reduceAmount = 4.6f;
            var increaseAmount = 0f;
            if (target.IsRunning())
            {
                increaseAmount = 1.51f;
                reduceAmount = 0f;
            }
            if (dmg > 0.0f) target.Hurt((float)dmg, Rust.DamageType.ElectricShock, target, false);
            var eyesforward = target?.eyes?.HeadForward() ?? Vector3.zero;
            if (reduceAmount > 0.0f) eyesforward /= reduceAmount;
            if (increaseAmount > 0.0f) eyesforward *= increaseAmount;
            var eyesPos = target?.eyes?.position ?? Vector3.zero;
            var targPos = eyesPos + eyesforward;
            Effect.server.Run("assets/prefabs/locks/keypad/effects/lock.code.shock.prefab", targPos, targPos, null, false);
        }

        object OnCodeEntered(CodeLock codeLock, BasePlayer player, string code)
        {
            if (isShadowBanned(player))
            {
                ShockPlayer(player, 34);
                return true;
            }
            return null;
        }

        private object OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return null;
            if (isShadowBanned(player) && UnityEngine.Random.Range(0, 101) <= 5)
            {
                CancelDamage(info);
                return true;
            }
            return null;
        }

        [PluginReference]
        Plugin AAIPDatabase;
        [PluginReference]
        Plugin Clans;

        void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            if (item?.parent != null) item.RemoveFromContainer();
            item.Remove(0.0f);
        }
        object CanPlayerPM(string userID, string targetID, string msg)
        {
            var ulID = 0ul;
            if (!ulong.TryParse(userID, out ulID)) return null;
            var player = BasePlayer.FindByID(ulID) ?? BasePlayer.FindSleeping(ulID) ?? null;
            if (player == null) return null;
            if (isShadowBanned(player)) return false;
            return null;
        }
        object CanUpdateSign(BasePlayer player, Signage sign)
        {
            if (player != null && isShadowBanned(player)) return false;
            return null;
        }
        void OnClanCreate(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            var members = Clans?.Call<List<ulong>>("GetClanMembers", tag) ?? null;
            if (members != null && members.Count > 0)
            {
                var member = members[0];
                var ply = BasePlayer.FindByID(member) ?? BasePlayer.FindSleeping(member);
                if (isShadowBanned(ply))
                {
                    ply.SendConsoleCommand("csay /clan disband");
                }
            }
        }
        /*/
        void OnItemRemovedFromContainer(ItemContainer container, Item item)
        {
            if (!init) return;
            if (container == null || item == null) return;
            var player = container?.playerOwner ?? null;
            var entityOwner = container?.entityOwner ?? null;
            if (player == null) return;
            if (player.IsAdmin) return;
            if (player.IsDead() || !player.IsConnected) return;
            if (!isRegistered(player) && !isShadowBanned(player)) return;
            if (isLoggedIn(player) && !isShadowBanned(player)) return;
            
            Puts(container.playerOwner.displayName);
            var pos = item.position;
            var parent = container;
            Puts(isShadowBanned(player) + " <-- shadowbanned?");
            NextTick(() =>
            {
                if (player != null)
                {
                    if (!player.IsConnected) return;
                }
                if (container == null || item == null)
                {
                    PrintWarning("cont or item is null");
                    return;
                }
                var parentName = item?.parent?.entityOwner?.ShortPrefabName ?? string.Empty;
                Puts(parentName + " <-- new parent");
                if (item.parent != null && !parentName.Contains("crate") && !parentName.Contains("trash"))
                {
                    Puts("parent is not null, so it wasn't dropped?");
                    Puts("logged in: " + isLoggedIn(player) + ", is registered: " + isRegistered(player));
                    return;
                }
                if (!item.MoveToContainer(parent))
                {
                    PrintWarning("Unable to move to parent! this really shouldn't happen -- destroying item");
                    RemoveFromWorld(item);
                    return;
                }
             //   item.MoveToContainer(parent);
                item.position = pos;
               if (!isShadowBanned(player)) SendReply(player, "<color=orange>You cannot drop items while you are not logged in!</color>");
            });
        }/*/

        void CancelDamage(HitInfo hitinfo)
        {
            Rust.DamageTypeList emptyDamage = new Rust.DamageTypeList();
            hitinfo.damageTypes = emptyDamage;
            hitinfo.HitEntity = null;
        }
        /*/
        void OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player != null && isShadowBanned(player) && info?.Initiator != player)
            {
                CancelDamage(info);
                PrintWarning("Canceling damage: " + (player?.ShortPrefabName ?? "Unknown") + ", sban player is: " + player.displayName);
            }
        }/*/

        private void OnEntityBuilt(Planner plan, GameObject objectBlock)
        {
            var player = plan?.GetOwnerPlayer() ?? null;
            if (player == null) return;
            if (isShadowBanned(player) && UnityEngine.Random.Range(0, 101) <= 18)
            {
                var block = objectBlock?.ToBaseEntity() as BuildingBlock;
                NextTick(() =>
                {
                    if (block != null && !(block?.IsDestroyed ?? true)) block.Kill(BaseNetworkable.DestroyMode.None);
                });
            }
        }
        /*/
        object OnHealingItemUse(HeldEntity item, BasePlayer target)
        {
            if (!isShadowBanned(target)) return null;
            if (target.health <= 1f) return null;
            target.Hurt(target.Health() - 1f);
            return true;
        }

        void OnConsumableUse(Item item)
        {
            var player = item?.GetOwnerPlayer() ?? null;
            if (player == null) return;
            if (!isShadowBanned(player)) return;
            if (player.Health() <= 1f) return;
            Puts(player.displayName + " is eating while shadowbanned!");
            player.Hurt(player.Health() - 1f);
            player.metabolism.calories.Subtract(500f);
            player.metabolism.hydration.Subtract(500f);
        }

        void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (isShadowBanned(player))
            {
                NextTick(() => player.EndLooting());
                PrintWarning(player.displayName + " is shadow banned!");
            }
            var name = entity?.ShortPrefabName ?? string.Empty;
            if (string.IsNullOrEmpty(name)) return;
            var isBox = entity?.GetComponent<StorageContainer>() ?? null != null;
            var isSleeper = entity?.GetComponent<BasePlayer>() ?? null != null;
            var isLoot = entity?.GetComponent<LootContainer>() ?? null != null;
            if (isLoot) return;
            if (!isBox && !isSleeper) return;
           if (isRegistered(player) && !isLoggedIn(player))
            {
                SendReply(player, "Because you are registered, you must be logged in to loot!");
                NextTick(() =>
                {
                    if (player == null || entity == null) return;
                    player.EndLooting();
                   
                    
                });
            }
        }/*/


        object CanLootEntity(BasePlayer player, StorageContainer target)
        {
            if (player == null || target == null) return null;
            if (isShadowBanned(player))
            {
                PrintWarning(player.displayName + " is shadow banned and trying to loot!");
                return false;
            }
            return null;
        }
        /*/
        object OnServerCommand(ConsoleSystem.Arg arg)
        {
           // PrintWarning("on server cmd");
            var IP = arg?.Connection?.ipaddress ?? string.Empty;
         //   if ((arg?.Player() ?? null) == null && !string.IsNullOrEmpty(IP)) PrintWarning("Command without player: " + IP + ", " + (arg?.cmd?.FullName ?? "Unknown"));
            if (arg == null || arg.Connection == null || arg.cmd == null || arg.cmd.Name == null || !init) return null;
            var player = arg?.Player() ?? null;
            var command = arg?.cmd?.FullName ?? string.Empty;
            var argsStr = arg?.FullString ?? string.Empty;
            var args = arg?.Args ?? null;

            if (string.IsNullOrEmpty(command) || player == null) return null;
            if (player.IsAdmin) PrintWarning("Command: " + command);
            if (args != null && args.Length > 1 && command == "note.update")
            {
                var str = string.Join(" ", args, 1, (args.Length - 1));
                PrintWarning("Str update: " + str);
                //    var argStr = string.Join()
                Item note = null;
              //  var note = FindItemByID(args[0]);
                if (note == null) return null;
                var oldText = note.text;

                NextTick(() =>
                {
                    if (note == null) return;
                    note.text = note.text.Replace("$", "");
                });
            }

            return null;
        }/*/

        object OnNoteUpdate(BasePlayer player, Item note)
        {
            if (player == null || note == null) return null;
            if (isShadowBanned(player))
            {
                return "";
            }
            return null;
        }

        object CanTeleport(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return null;
            if (isShadowBanned(player)) return "You cannot teleport while building blocked!";
            return null;
        }

        string getIPString(BasePlayer player)
        {
            var uID = player?.UserIDString ?? "0";
            var IP = player?.net?.connection?.ipaddress ?? string.Empty;
            if (!string.IsNullOrEmpty(IP) && IP.Contains(':')) IP = IP.Split(':')[0];
            if (string.IsNullOrEmpty(IP) && AAIPDatabase != null) IP = AAIPDatabase?.Call<string>("GetLastIP", uID) ?? string.Empty;
            return IP;
        }

        string getIPString(IPlayer player)
        {
            var uID = player?.Id ?? "0";
            var IP = string.Empty;
            if ((player?.IsConnected ?? false)) IP = player?.Address ?? string.Empty;
            if (!string.IsNullOrEmpty(IP) && IP.Contains(':')) IP = IP.Split(':')[0];
            if (string.IsNullOrEmpty(IP) && AAIPDatabase != null) IP = AAIPDatabase?.Call<string>("GetLastIP", uID) ?? string.Empty;
            if (string.IsNullOrEmpty(IP) & (player?.IsConnected ?? false)) IP = player?.Address ?? string.Empty;
            return IP;
        }

        string getIPString(Network.Connection connection)
        {
            var IP = connection?.ipaddress ?? string.Empty;
            if (!string.IsNullOrEmpty(IP) && IP.Contains(':')) IP = IP.Split(':')[0];
            return IP;
        }
        bool? lastUpdateState;
        void UpdateHooks()
        {
            var state = storedData?.shadowBanned != null && storedData.shadowBanned.Count > 0;
            if (lastUpdateState.HasValue && ((bool)lastUpdateState) == state) return;
            if (state)
            {
                Subscribe(nameof(CanLootEntity));
                Subscribe(nameof(OnEntityBuilt));
                Subscribe(nameof(CanUpdateSign));
             //   Subscribe(nameof(OnServerCommand));
            }
            else
            {
                Unsubscribe(nameof(CanLootEntity));
                Unsubscribe(nameof(OnEntityBuilt));
                Unsubscribe(nameof(CanUpdateSign));
             //   Unsubscribe(nameof(OnServerCommand));
            }
        }

        class StoredData
        {
            [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            private List<string> _shadowBanned = new List<string>();
            [JsonIgnore]
            public List<string> ShadowBanned
            {
                get { return _shadowBanned; }
                set
                {
                    _shadowBanned = value;
                    ins?.UpdateHooks();
                }
            }
            public List<string> shadowBanned = new List<string>();
            public StoredData() { }
        }


        StoredData storedData;
        public static ShadowBan ins;
        private void Init()
        {
            ins = this;
            if ((storedData = Interface.GetMod().DataFileSystem.ReadObject<StoredData>("ShadowData")) == null) storedData = new StoredData();
            UpdateHooks();
        }

        bool isShadowBanned(BasePlayer player)
        {
            var ip = getIPString(player);
            if (string.IsNullOrEmpty(ip) || player == null) return false;
            return storedData.shadowBanned.Contains(ip);
        }

        bool isShadowBannedID(ulong userId)
        {
            return isShadowBanned(BasePlayer.FindByID(userId) ?? BasePlayer.FindSleeping(userId));
        }

        bool IsBanned(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return false;
            var covPlayer = covalence.Players?.FindPlayerById(userID) ?? null;
            if (covPlayer == null) return false;
            return storedData.shadowBanned.Contains(getIPString(covPlayer));
        }

        void ShadowBanPlayer(BasePlayer player)
        {
            if (player == null) return;
            var ip = getIPString(player);
            if (string.IsNullOrEmpty(ip)) return;
            if (storedData.shadowBanned.Contains(ip)) return;
            else storedData.shadowBanned.Add(ip);
            UpdateHooks();
        }

        void unShadowBan(BasePlayer player, bool IP = false)
        {
            var ip = getIPString(player);
            if (string.IsNullOrEmpty(ip)) return;
            if (storedData.shadowBanned.Contains(ip)) storedData.shadowBanned.Remove(ip);
            UpdateHooks();
        }

       

        [ChatCommand("shadowban")]
        private void cmdShadowBan(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length <= 0)
            {
                SendReply(player, "You must supply a player name!");
                return;
            }
            var target = FindPlayerByPartialNameSleepers(args[0]);
            if (target == null)
            {
                SendReply(player, "Unable to find a player with that name!");
                return;
            }
            ShadowBanPlayer(target);
            SendReply(player, "Shadow banned: " + target.displayName);
            UpdateHooks();
        }


        [ChatCommand("unbanshadow")]
        private void cmdUnshadowban(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length <= 0)
            {
                SendReply(player, "You must supply a player name!");
                return;
            }
            var target = FindPlayerByPartialNameSleepers(args[0]);
            if (target == null)
            {
                SendReply(player, "Unable to find a player with that name!");
                return;
            }
            if (!isShadowBanned(target))
            {
                SendReply(player, target.displayName + " is not shadow banned!");
                return;
            }
            unShadowBan(target, true);
            SendReply(player, "Un-Shadow banned: " + target.displayName);
            UpdateHooks();
        }

        [ConsoleCommand("shadowban")]
        private void consoleShadowBan(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null) return;
            if (arg.Args.Length <= 0) return;
            var target = FindPlayerByPartialNameSleepers(arg.Args[0]);
            if (target == null)
            {
                SendReply(arg, "Unable to find a player by the name of: " + arg.Args[0]);
                return;
            }
            ShadowBanPlayer(target);
            SendReply(arg, "Shadowbanned: " + target.displayName);
            UpdateHooks();
        }

        string GetDisplayNameFromID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return string.Empty;
            return getIPlayer(userID)?.Name ?? "Unknown";
        }

        string GetDisplayNameFromID(ulong userID)
        {
            return GetDisplayNameFromID(userID.ToString());
        }

        IPlayer getIPlayer(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            return covalence.Players?.FindPlayerById(ID) ?? null;
        }

        IPlayer getIPlayerPartialName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var players = covalence.Players?.FindPlayers(name);
            if (!players.Any() || players.Count() >= 2) return null;
            else return players.FirstOrDefault();
        }


        private BasePlayer FindPlayerByID(string playerid)
        {
            ulong id;
            if (!ulong.TryParse(playerid, out id)) return null;
            return BasePlayer.FindByID(id);
        }

        private BasePlayer FindPlayerByID(ulong playerid)
        {
            return BasePlayer.FindByID(playerid) ?? null;
        }

        private void SavePlayerFile() => Interface.GetMod().DataFileSystem.WriteObject("ShadowData", storedData);

        /*--------------------------------------------------------------//
		//		canExecute - check if the player has permission			//
		//--------------------------------------------------------------*/
        private bool canExecute(BasePlayer player, string perm)
        {
            if (string.IsNullOrEmpty(perm) || player == null) return false;
            return permission.UserHasPermission(player.UserIDString, perm);
        }


        private BasePlayer FindSleeperByPartialName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            BasePlayer player = null;
            name = name.ToLower();
            var allPlayers = BasePlayer.sleepingPlayerList.ToArray();
            // Try to find an exact match first
            foreach (var p in allPlayers)
            {
                if (p.displayName == name)
                {
                    if (player != null)
                        return null; // Not unique
                    player = p;
                }
            }
            if (player != null)
                return player;
            // Otherwise try to find a partial match
            foreach (var p in allPlayers)
            {
                if (p.displayName.ToLower().IndexOf(name) >= 0)
                {
                    if (player != null)
                        return null; // Not unique
                    player = p;
                }
            }
            return player;
        }



        /*--------------------------------------------------------------//
		//			  Find a player by name/partial name				//
		//				Thank You Whoever Wrote This					//
		//--------------------------------------------------------------*/
        private BasePlayer FindPlayerByPartialName(string name, bool sleepers = false)
        {
            if (string.IsNullOrEmpty(name)) return null;
            BasePlayer player = null;
            name = name.ToLower();
            try
            {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
                {
                    var p = BasePlayer.activePlayerList[i];
                    if (p == null) continue;
                    var pName = (p?.displayName ?? string.Empty).ToLower();
                    if (pName == name)
                    {
                        if (player != null) return null;
                        player = p;
                        return player;
                    }
                    if (pName.IndexOf(name) >= 0)
                    {
                        if (player != null) return null;
                        player = p;
                    }
                }
                if (sleepers)
                {
                    for (int i = 0; i < BasePlayer.sleepingPlayerList.Count; i++)
                    {
                        var p = BasePlayer.sleepingPlayerList[i];
                        if (p == null) continue;
                        var pName = (p?.displayName ?? string.Empty).ToLower();
                        if (pName == name)
                        {
                            if (player != null) return null;
                            player = p;
                            return player;
                        }
                        if (pName.IndexOf(name) >= 0)
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
                PrintError(ex.Message + Environment.NewLine + ex.ToString());
                return null;
            }
            return player;
        }

        private BasePlayer FindPlayerByPartialNameSleepers(string name)
        {
            return FindPlayerByPartialName(name, true);
        }
    }
}