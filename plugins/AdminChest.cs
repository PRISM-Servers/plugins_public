using Network;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AdminChest", "Shady", "1.0.0")]
    class AdminChest : RustPlugin
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
        private readonly int constructionColl = UnityEngine.LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        private readonly int deployColl = UnityEngine.LayerMask.GetMask(new string[] { "Deployable", "Deployed" });
        private const int MaxStack = 2147450880;
        class StoredData
        {
            public List<ulong> Chests = new List<ulong>();
            public StoredData() { }
        }
        StoredData data;

        bool IsAdminChest(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed || entity?.net?.ID == null) return false;
            return data?.Chests?.Contains(entity.net.ID.Value) ?? false;
        }

        void AddAdminChest(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed || entity?.net?.ID == null) return;
            if (IsAdminChest(entity)) return;
            var netID = entity?.net?.ID.Value ?? 0;
            if (data?.Chests != null)
            {
                data.Chests.Add(netID);
                if (!entity.globalBroadcast)
                {
                    entity.globalBroadcast = true;
                    entity.SendNetworkUpdate();
                }
            }
         
        }

        void RemoveAdminChest(BaseEntity entity)
        {
            if (!IsAdminChest(entity)) return;
            var netID = entity?.net?.ID.Value ?? 0;
            if (data?.Chests != null) data.Chests.Remove(netID);
        }

        object CanStackItem(Item item, Item targetItem)
        {
            if (item == null || targetItem == null) return null;
            var cont1 = item?.parent?.entityOwner ?? targetItem?.parent?.entityOwner ?? null;
            var cont2 = targetItem?.parent?.entityOwner ?? item?.parent?.entityOwner ?? null;
            if (IsAdminChest(cont1) || IsAdminChest(cont2))
            {
                var canStack = targetItem != item && item.info.stackable > 1 && (targetItem.info.stackable > 1 && targetItem.info.itemid == item.info.itemid) && (item.IsValid()) && (!item.IsBlueprint() || item.blueprintTarget == item.blueprintTarget);
                if (canStack) return true;
            }
            return null;
        }

        object OnMaxStackable(Item item)
        {
            if (item == null) return null;
            var cont1 = item?.parent?.entityOwner ?? null;
            if (IsAdminChest(cont1)) return MaxStack;
            return null;
        }

        void Init()
        {
            data = Interface.Oxide?.DataFileSystem?.ReadObject<StoredData>("AdminChests") ?? new StoredData();
        }

        void OnServerInitialized()
        {
            var count = 0;
            if (data?.Chests != null && data.Chests.Count > 0)
            {
                var chests = data?.Chests?.ToList() ?? null;
                for(int i = 0; i < chests.Count; i++)
                {
                    var chest = chests[i];
                    var findChest = BaseEntity.saveList?.Where(p => (p?.net?.ID.Value ?? 0) == chest)?.FirstOrDefault() ?? null;
                    if (findChest == null || findChest.IsDestroyed)
                    {
                        data.Chests.Remove(chest);
                        count++;
                    }
                    else
                    {
                        if (!findChest.globalBroadcast)
                        {
                            findChest.globalBroadcast = true;
                            findChest.SendNetworkUpdate();
                        }
                    }
                }
            }
            if (count > 0) PrintWarning("Removed " + count.ToString("N0") + " null/destroyed admin chests.");
        }

        void OnServerSave() => SaveData();

        void SaveData() => Interface.Oxide.DataFileSystem.WriteObject<StoredData>("AdminChests", data);
        void Unload() => SaveData();

        void StartLootingChest(BasePlayer player, StorageContainer chest)
        {
            if (player == null || player.IsDead() || !player.IsConnected || player?.inventory?.loot == null || chest == null || chest.IsDestroyed) return;
            PositionHack(player, chest, new Vector3(player.transform.position.x, player.transform.position.y - 100, player.transform.position.z));
            player.inventory.loot.containers.Clear();
            player.inventory.loot.entitySource = null;
            player.inventory.loot.itemSource = null;
            player.inventory.loot.MarkDirty();
            player.inventory.loot.PositionChecks = false;

            player.inventory.loot.StartLootingEntity(chest, false);
            player.inventory.loot.AddContainer(chest.inventory);
            player.inventory.loot.SendImmediate();

            player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", "generic");
      //      LanternGUI(player, string.Empty);
        }

        void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null) return;
            var ent = entity as StorageContainer;
            if (ent == null || !IsAdminChest(ent)) return;
            RemoveAdminChest(ent);
        }


        object CanCustomNetworkTo(BaseNetworkable entity, BasePlayer target)
        {
            if (entity == null || target == null || target.IsAdmin) return null;
            var storage = entity as StorageContainer;
            if (storage != null && IsAdminChest(storage)) return false;
            return null;
        }

        void OnPlayerLootEnd(PlayerLoot inventory)
        {
            if (inventory == null) return;
            var player = inventory.GetComponent<BasePlayer>();
            if (player == null) return;
            var ent = (inventory?.entitySource) as StorageContainer;
            if (ent != null && IsAdminChest(ent)) PositionHack(player, ent, ent?.transform?.position ?? Vector3.zero);
        }

        object PositionHack(BasePlayer target, BaseEntity entity, Vector3 setPos = default(Vector3))
        {
            if (target == null || entity == null) return null;
            try
            {
                var connection = target?.net?.connection ?? null;
                if (connection != null)
                {

                    var netWrite = Network.Net.sv.StartWrite();

                    connection.validate.entityUpdates = connection.validate.entityUpdates + 1;
                    BaseNetworkable.SaveInfo saveInfo = new global::BaseNetworkable.SaveInfo
                    {
                        forConnection = connection,
                        forDisk = false
                    };
                    netWrite.PacketID(Network.Message.Type.Entities);
                    netWrite.UInt32(target.net.connection.validate.entityUpdates);
                    using (saveInfo.msg = Facepunch.Pool.Get<ProtoBuf.Entity>())
                    {
                        entity.Save(saveInfo);
                        if (saveInfo.msg.baseEntity == null) PrintError(this + ": ToStream - no BaseEntity!?");
                        if (saveInfo.msg.baseNetworkable == null) PrintError(this + ": ToStream - no baseNetworkable!?");
                        saveInfo.msg.baseEntity.pos = (setPos != Vector3.zero) ? setPos : (target?.transform?.position ?? Vector3.zero);

                        saveInfo.msg.ToProto(netWrite);
                        entity.PostSave(saveInfo);
                        netWrite.Send(new SendInfo(connection));
                    }
                    return true;
                }
            }
            catch (Exception ex) { PrintError(ex.ToString()); }
            return null;
        }


        [ChatCommand("adminc")]
        private void cmdAdminc(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if (args.Length < 1)
            {
                SendReply(player, "No arguments, so here's the chest count: " + (data?.Chests?.Count ?? -1).ToString("N0"));
                return;
            }
            int index;
            var arg0Lower = args[0].ToLower();
            if (int.TryParse(arg0Lower, out index))
            {
                if (index > (data.Chests.Count - 1))
                {
                    SendReply(player, "Index too high!");
                    return;
                }
                if (index < 0)
                {
                    SendReply(player, "Index too low!");
                    return;
                }
                var chestID = data.Chests[index];
                var chest = (BaseNetworkable.serverEntities?.Where(p => (p?.net?.ID.Value ?? 0) == chestID)?.FirstOrDefault() ?? null) as StorageContainer;
                if (chest == null || chest.IsDestroyed || !IsAdminChest(chest))
                {
                    SendReply(player, "No chest: " + index);
                    return;
                }
                timer.Once(0.25f, () => StartLootingChest(player, chest));
                return;
            }
            var arg1Lower = args.Length > 1 ? args[1].ToLower() : string.Empty;
            if (arg0Lower == "add")
            {
                var box = GetLookAtEntity(player, 10f, deployColl) as StorageContainer;
                if (box == null || box.IsDestroyed)
                {
                    SendReply(player, "You must be looking at a box to add it.");
                    return;
                }
                var netID = box?.net?.ID.Value ?? 0;
                if (netID == 0)
                {
                    SendReply(player, "Box has no ID!");
                    return;
                }
                if (IsAdminChest(box))
                {
                    SendReply(player, "Already an admin chest! Index: " + data.Chests.FindIndex(p => p == netID));
                    return;
                }
                AddAdminChest(box);
                SendReply(player, "Added admin chest. Index: " + data.Chests.FindIndex(p => p == netID));
            }
            if (arg0Lower == "remove")
            {
                var box = GetLookAtEntity(player, 10f, deployColl) as StorageContainer;
                if (box == null || box.IsDestroyed)
                {
                    
                    if (string.IsNullOrEmpty(arg1Lower) || !int.TryParse(arg1Lower, out index))
                    {
                        SendReply(player, "You must be looking at a box to use this, or supply an index!");
                        return;
                    }
                    else
                    {
                        if (index > (data.Chests.Count - 1))
                        {
                            SendReply(player, "Index too high!");
                            return;
                        }
                        if (index < 0)
                        {
                            SendReply(player, "Index too low!");
                            return;
                        }
                        var chestID = data.Chests[index];
                        box = (BaseNetworkable.serverEntities?.Where(p => (p?.net?.ID.Value ?? 0) == chestID)?.FirstOrDefault() ?? null) as StorageContainer;
                    }
           
                }
                if (box == null || box.IsDestroyed)
                {
                    SendReply(player, "No box found!");
                    return;
                }
                if (!IsAdminChest(box))
                {
                    SendReply(player, "Not an admin chest!");
                    return;
                }
                RemoveAdminChest(box);
                SendReply(player, "Removed admin box");
            }
        }

        BaseEntity GetLookAtEntity(BasePlayer player, float maxDist = 250, int coll = -1)
        {
            if (player == null || player.IsDestroyed || player.gameObject == null || player.IsDead()) return null;
            RaycastHit hit;
            var currentRot = Quaternion.Euler(player?.serverInput?.current?.aimAngles ?? Vector3.zero) * Vector3.forward;
            var ray = new Ray((player?.eyes?.position ?? Vector3.zero), currentRot);
            if (UnityEngine.Physics.Raycast(ray, out hit, maxDist, ((coll != -1) ? coll : constructionColl)))
            {
                var ent = hit.GetEntity() ?? null;
                if (ent != null && !(ent?.IsDestroyed ?? true)) return ent;
            }
            return null;
        }

        void RemoveFromWorld(Item item)
        {
            if (item == null) return;
            item.RemoveFromWorld();
            item.RemoveFromContainer();
            item.Remove();
        }

    }
}